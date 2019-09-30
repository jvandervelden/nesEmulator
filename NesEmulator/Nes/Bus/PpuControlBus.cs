using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Bus
{
    class PpuControlBus : IBusInterface
    {
        private IPPU ppu;

        public PpuControlBus(IPPU ppu)
        {
            this.ppu = ppu;
        }

        public byte ReadByte(uint address)
        {
            return ppu.GetRegister((byte)(address & 0x07));
        }

        public void WriteByte(uint address, byte data)
        {
            ppu.SetRegister((byte)(address & 0x07), data);
        }
    }
}
