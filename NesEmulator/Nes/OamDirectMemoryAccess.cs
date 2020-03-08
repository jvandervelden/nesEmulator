using Common.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Memory;

namespace TestPGE.Nes
{
    public class OamDirectMemoryAccess : IOamDma
    {
        private I2C02 _ppu;
        private Ram _cpuMemory;

        public OamDirectMemoryAccess(I2C02 ppu, Ram cpuMemory)
        {
            _ppu = ppu;
            _cpuMemory = cpuMemory;
        }

        public void TransferBlock(UInt16 memoryAddress, byte oamAddress, long size)
        {
            _ppu.WriteOamBlock(oamAddress, _cpuMemory.ReadBlock(memoryAddress, size));
        }

        public void TransferByte(UInt16 memoryAddress, byte oamAddress)
        {
            _ppu.WriteOamByte(oamAddress, _cpuMemory.ReadByte(memoryAddress));
        }
    }
}
