using Common.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes.Memory
{
    public class NameTableRam : IBusInterface
    {
        private enum MirroringType
        {
            VERTICAL,
            HORIZANTAL,
            ALL, // Single Screen
            NONE // 4 screen
        }

        private byte[,] _ram = new byte[4, 0x400];
        private MirroringType _mirroring;

        public NameTableRam(Cartridge cartridge)
        {
            if ((cartridge.NesHeader.Flags6 & 0x0F) == 0x0F)
                _mirroring = MirroringType.NONE;
            else
                _mirroring = (cartridge.NesHeader.Flags6 & 0x01) == 0x01 ? MirroringType.VERTICAL : MirroringType.HORIZANTAL;
        }

        private byte GetQuadrentIndex(uint address)
        {
            byte quadrant = (byte)((address & 0xC00) >> 10);

            switch(_mirroring)
            {
                case MirroringType.HORIZANTAL:
                    return (byte)(quadrant > 1 ? 1 : 0);
                case MirroringType.VERTICAL:
                    return (byte)(quadrant % 2);
                case MirroringType.ALL:
                    return 0;
                case MirroringType.NONE:
                    return quadrant;
            }

            throw new NotImplementedException($"Unsupported mirroring {_mirroring}");
        }

        public byte ReadByte(uint address)
        {
            return _ram[GetQuadrentIndex(address), address & 0x3FF];
        }

        public void WriteByte(uint address, byte data)
        {
            _ram[GetQuadrentIndex(address), address & 0x3FF] = data;
        }
    }
}
