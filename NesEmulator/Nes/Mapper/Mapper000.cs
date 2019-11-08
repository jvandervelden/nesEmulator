using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Mapper
{
    class Mapper000 : IMapper
    {
        private byte[] _programRomData;
        private byte[] _characterRomData;
        private byte[] _programRamData;

        private bool _mirrorRom = false;

        public uint NameTableSize { get; private set; } = 0;

        public Mapper000(INesHeader nesHeader, byte[] programRomData, byte[] characterRomData)
        {
            _programRomData = programRomData;
            _characterRomData = characterRomData;

            _mirrorRom = nesHeader.PrgRomSize < 2;

            _programRamData = new byte[nesHeader.Flags8 == 0 ? 8192 : 8192];
        }

        public byte ChrRomRead(ushort ppuAddress)
        {
            return _characterRomData[ppuAddress];
        }

        public void ChrRomWrite(ushort ppuAddress, byte ppuData)
        {
            throw new InvalidOperationException("Cannot write to ROM character memory");
        }

        public byte PrgRomRead(ushort cpuAddress)
        {
            if (cpuAddress > 0x7FFF)
                return _programRomData[(UInt16)(_mirrorRom ? cpuAddress & 0x3FFF : cpuAddress & 0x7FFF)];
            else
                return _programRamData[(UInt16)(cpuAddress & 0x1FFF)];
        }

        public void PrgRomWrite(ushort cpuAddress, byte cpuData)
        {
            if (cpuAddress > 0x7FFF)
                throw new InvalidOperationException("Cannot write to ROM program memory");
            else
                _programRamData[cpuAddress & 0x1FFF] = cpuData;
        }
    }
}
