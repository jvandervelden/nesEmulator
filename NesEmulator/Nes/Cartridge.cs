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
    public class Cartridge : IBusInterface
    {
        public const int TRAINER_SIZE = 512; // 512B
        public const int PROGRAM_ROM_BANK_SIZE = 0x4000; // 16KB
        public const int CHARACTER_ROM_BANK_SIZE = 0x2000; // 8KB

        public const int CHARACTER_ROM_ADDRESS_START = 0x0000;
        public const int CHARACTER_ROM_ADDRESS_END = 0x1FFF;
        public const int PROGRAM_ROM_ADDRESS_START = 0x4020;
        public const int PROGRAM_ROM_ADDRESS_END = 0xFFFF;

        private string _filename;
        private INesHeader _nesHeader;
        private byte[] _PrgRomData;
        private byte[] _ChrRomData;
        private List<byte> _extra = new List<byte>();

        private IMapper _mapper;

        private DataBus _cpuBus;
        private DataBus _ppuBus;

        private byte[] _saveRam = new byte[0];

        public Cartridge()
        {
        }

        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
                throw new ArgumentException("File does not exist: " + fileName);

            using (FileStream fileHandle = File.OpenRead(fileName))
            {
                _nesHeader = FetchHeaderFromFile(fileHandle);

                if (_nesHeader.HasTrainer)
                    fileHandle.Read(new byte[TRAINER_SIZE], 0, TRAINER_SIZE);

                _PrgRomData = new byte[PROGRAM_ROM_BANK_SIZE * _nesHeader.PrgRomSize];
                fileHandle.Read(_PrgRomData, 0, _PrgRomData.Length);

                _ChrRomData = new byte[CHARACTER_ROM_BANK_SIZE * _nesHeader.ChrRomSize];
                fileHandle.Read(_ChrRomData, 0, _ChrRomData.Length);

                byte[] buffer = new byte[512];

                while(fileHandle.Read(buffer, 0, 512) > 0)
                {
                    for (int i = 0; i < 512; i++)
                        _extra.Add(buffer[i]);
                }
            }

            _mapper = CreateMapper(_nesHeader);

            if (_nesHeader.HasNonVolMemory)
                _saveRam = new byte[_mapper.NonVolitileRamSize];
        }

        public void Insert(DataBus cpuBus, DataBus ppuBus)
        {
            _cpuBus = cpuBus;
            _ppuBus = ppuBus;

            cpuBus.ConnectDevice(this, 0x4020, 0xFFFF);
            ppuBus.ConnectDevice(this, 0x0000, 0x1FFF);
            
            // $2000 - $2FFF possibly mapped
        }

        private IMapper CreateMapper(INesHeader nesHeader)
        {
            switch(nesHeader.Mapper)
            {
                case 0x01:
                    return new Mapper001(nesHeader);
            }

            throw new NotImplementedException($"Mapper ${nesHeader.Mapper} not implemented yet.");
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
            nesHeader.INes2 = ((_nesHeader.Flags7 >> 2) & 0x03) == 0x02;
            nesHeader.HasTrainer = (0x04 & _nesHeader.Flags6) == 0x04;
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
            if (address >= PROGRAM_ROM_ADDRESS_START)
            {
                return _PrgRomData[_mapper.PrgRomRead((UInt16)address)];
            }
            // CHR Rom space
            else if (address <= PROGRAM_ROM_ADDRESS_END)
            {
                return _ChrRomData[_mapper.ChrRomRead((UInt16)address)];
            }
            else
                throw new IndexOutOfRangeException("Trying to access address outside PRG and CHR rom range.");
        }

        public void WriteByte(uint address, byte data)
        {
            // PRG Rom space
            if (address >= PROGRAM_ROM_ADDRESS_START)
            {
                uint? nonVolitileAddress = _mapper.PrgRomWrite((UInt16)address, data);

                if (nonVolitileAddress.HasValue && _nesHeader.HasNonVolMemory)
                    _saveRam[nonVolitileAddress.Value] = data;
            }
            // CHR Rom space
            else if (address <= PROGRAM_ROM_ADDRESS_END)
            {
                _mapper.ChrRomWrite((UInt16)address, data);
            }
            else
                throw new IndexOutOfRangeException("Trying to access address outside PRG and CHR rom range.");
        }
    }
}
