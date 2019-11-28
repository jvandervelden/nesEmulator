using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;
using TestPGE.Nes.Mapper;

namespace TestPGE.Nes
{ 
    public class Cartridge : IBusInterface, IDisposable
    {
        public const int TRAINER_SIZE = 512; // 512B
        public const int PROGRAM_ROM_BANK_SIZE = 0x4000; // 16KB
        public const int CHARACTER_ROM_BANK_SIZE = 0x2000; // 8KB

        public const int CHARACTER_ROM_ADDRESS_START = 0x0000;
        public const int CHARACTER_ROM_ADDRESS_END = 0x1FFF;
        public const int PROGRAM_ROM_ADDRESS_START = 0x4020;
        public const int PROGRAM_ROM_ADDRESS_END = 0xFFFF;

        private string _filename;
        public INesHeader NesHeader { get; private set; }
        
        private List<byte> _extra = new List<byte>();

        public IMapper Mapper { get; private set; }

        private DataBus _cpuBus;
        private DataBus _ppuBus;

        public Cartridge()
        {
        }

        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
                throw new ArgumentException("File does not exist: " + fileName);

            byte[] programRomData;
            byte[] characterRomData;

            using (FileStream fileHandle = File.OpenRead(fileName))
            {
                NesHeader = FetchHeaderFromFile(fileHandle);

                if (NesHeader.HasTrainer)
                    fileHandle.Read(new byte[TRAINER_SIZE], 0, TRAINER_SIZE);

                programRomData = new byte[PROGRAM_ROM_BANK_SIZE * NesHeader.PrgRomSize];
                fileHandle.Read(programRomData, 0, programRomData.Length);

                characterRomData = new byte[CHARACTER_ROM_BANK_SIZE * NesHeader.ChrRomSize];
                fileHandle.Read(characterRomData, 0, characterRomData.Length);

                byte[] buffer = new byte[512];

                while(fileHandle.Read(buffer, 0, 512) > 0)
                {
                    for (int i = 0; i < 512; i++)
                        _extra.Add(buffer[i]);
                }
            }

            Mapper = CreateMapper(NesHeader, programRomData, characterRomData, fileName);
        }

        public void Insert(DataBus cpuBus, DataBus ppuBus)
        {
            _cpuBus = cpuBus;
            _ppuBus = ppuBus;

            cpuBus.ConnectDevice(this, 0x4020, 0xFFFF);
            ppuBus.ConnectDevice(this, 0x0000, 0x1FFF);

            if (Mapper.NameTableSize > 0)
                ppuBus.ConnectDevice(this, 0x2000, 0x2000 + Mapper.NameTableSize);
        }

        private IMapper CreateMapper(INesHeader nesHeader, byte[] programRomData, byte[] characterRomData, string romFilePath)
        {
            switch(nesHeader.Mapper)
            {
                case 0x00:
                    return new Mapper000(nesHeader, programRomData, characterRomData);
                case 0x01:
                    return new Mapper001(nesHeader, programRomData, characterRomData, romFilePath);
                case 0x02:
                    return new Mapper002(nesHeader, programRomData, characterRomData);
            }

            throw new NotImplementedException($"Mapper {nesHeader.Mapper} not implemented yet.");
        }

        private INesHeader FetchHeaderFromFile(FileStream fileHandle)
        {
            INesHeader nesHeader = new INesHeader();

            byte[] header = new byte[16];

            // Read the NES<EOL> bytes
            fileHandle.Read(header, 0, 16);

            nesHeader.NesHeader = new byte[] { header[0], header[1], header[2], header[3] };
            nesHeader.PrgRomSize = header[4];
            nesHeader.ChrRomSize = header[5];
            nesHeader.Flags6 = header[6];
            nesHeader.Flags7 = header[7];
            nesHeader.Flags8 = header[8];
            nesHeader.Flags9 = header[9];
            nesHeader.Flags10 = header[10];
            nesHeader.Flags11 = header[11];
            nesHeader.Flags12 = header[12];
            nesHeader.Flags13 = header[13];
            nesHeader.Flags14 = header[14];
            nesHeader.Flags15 = header[15];
            nesHeader.INes2 = ((NesHeader.Flags7 >> 2) & 0x03) == 0x02;
            nesHeader.HasTrainer = (0x04 & NesHeader.Flags6) == 0x04;
            nesHeader.Mapper = (byte)((nesHeader.Flags7 & 0xF0) | (nesHeader.Flags6 >> 4));
            nesHeader.HasNonVolMemory = (nesHeader.Flags6 & 0x02) == 0x02;

            if (nesHeader.INes2)
            {
                nesHeader.Mapper |= (ushort)((nesHeader.Flags8 & 0x0F) << 8);
                nesHeader.SubMapper = (byte)(nesHeader.Flags8 >> 4);
            }

            return nesHeader;
        }

        public byte ReadByte(uint address)
        {
            
            // PRG Rom space
            if (address >= PROGRAM_ROM_ADDRESS_START && address <= PROGRAM_ROM_ADDRESS_END)
            {
                return Mapper.PrgRomRead((UInt16)address);
            }
            // CHR Rom space
            else if (address >= CHARACTER_ROM_ADDRESS_START && address <= CHARACTER_ROM_ADDRESS_END)
            {
                return Mapper.ChrRomRead((UInt16)address);
            }
            else
                throw new IndexOutOfRangeException("Trying to access address outside PRG and CHR rom range.");
        }

        public void WriteByte(uint address, byte data)
        {
            // PRG Rom space
            if (address >= PROGRAM_ROM_ADDRESS_START && address <= PROGRAM_ROM_ADDRESS_END)
            {
                Mapper.PrgRomWrite((UInt16)address, data);
            }
            // CHR Rom space
            else if (address >= CHARACTER_ROM_ADDRESS_START && address <= CHARACTER_ROM_ADDRESS_END)
            {
                Mapper.ChrRomWrite((UInt16)address, data);
            }
            else
                throw new IndexOutOfRangeException("Trying to access address outside PRG and CHR rom range.");
        }

        public void Dispose()
        {
            if (Mapper is IDisposable)
                ((IDisposable)Mapper).Dispose();
        }
    }
}