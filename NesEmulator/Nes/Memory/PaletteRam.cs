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
                // If 3F - 10/14/18/1C then mirror down to 3F - 00/04/08/0C respectively
                (address & 0x0013) == 0x0010
                ? address & 0x000F
                : address & 0x001F;
        }

        public override byte ReadByte(uint address)
        {
            return base.ReadByte(GetPaletteRamAddress(address));
        }

        public override void WriteByte(uint address, byte data)
        {
            base.WriteByte(GetPaletteRamAddress(address), data);
        }
    }
}
