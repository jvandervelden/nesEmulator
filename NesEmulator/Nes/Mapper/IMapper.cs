using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Mapper
{
    public interface IMapper
    {
        byte PrgRomRead(UInt16 cpuAddress);

        void PrgRomWrite(UInt16 cpuAddress, byte cpuData);

        byte ChrRomRead(UInt16 ppuAddress);

        void ChrRomWrite(UInt16 ppuAddress, byte ppuData);

        uint NameTableSize { get; }
    }
}
