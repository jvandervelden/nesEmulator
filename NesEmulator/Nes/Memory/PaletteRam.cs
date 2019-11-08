using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes.Memory
{
    public class PaletteRam : Ram, IBusInterface
    {
        public PaletteRam() : base(0x20) {}

        private uint GetPaletteRamAddress(uint address)
        {
            return
                (address & 0x0003) == 0x0000
                ? 0x3F00
                : address;
        }

        public override byte ReadByte(uint address)
        {
            return base.ReadByte(GetPaletteRamAddress(address) & 0x001F);
        }

        public override void WriteByte(uint address, byte data)
        {
            base.WriteByte(GetPaletteRamAddress(address) & 0x001F, data);
        }
    }
}
