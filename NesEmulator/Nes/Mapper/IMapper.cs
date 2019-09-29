using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Mapper
{
    public interface IMapper
    {
        uint NonVolitileRamAddress { get; }
        uint NonVolitileRamSize { get; }

        uint PrgRomRead(UInt16 cpuAddress);

        uint? PrgRomWrite(UInt16 cpuAddress, byte cpuData);

        uint ChrRomRead(UInt16 ppuAddress);

        uint ChrRomWrite(UInt16 ppuAddress, byte ppuData);
    }
}
