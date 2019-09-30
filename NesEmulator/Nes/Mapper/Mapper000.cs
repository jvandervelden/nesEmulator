using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Mapper
{
    class Mapper000 : IMapper
    {
        public uint NonVolitileRamAddress => 0x6000;
        
        public uint NonVolitileRamSize { get; private set; } = 0;

        private UInt16 Offset = 0x8000;

        public Mapper000(INesHeader nesHeader)
        {
            Offset = (UInt16)(0x4000 * nesHeader.PrgRomSize);

            if (nesHeader.HasNonVolMemory)
                NonVolitileRamSize = 0x2000;
        }

        public uint ChrRomRead(ushort ppuAddress)
        {
            return ppuAddress;
        }

        public uint ChrRomWrite(ushort ppuAddress, byte ppuData)
        {
            throw new InvalidOperationException("Cannot write to ROM character memory");
        }

        public uint PrgRomRead(ushort cpuAddress)
        {
            return (uint)(cpuAddress - Offset - 1);
        }

        public uint? PrgRomWrite(ushort cpuAddress, byte cpuData)
        {
            return NonVolitileRamSize > 0 ? (uint?)cpuAddress - Offset - 1 : null;
        }
    }
}
