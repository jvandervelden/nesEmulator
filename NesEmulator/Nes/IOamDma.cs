using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes
{
    public interface IOamDma
    {
        void TransferByte(UInt16 memoryAddress, byte oamAddress);
        void TransferBlock(UInt16 memoryAddress, byte oamAddress, long size);
    }
}
