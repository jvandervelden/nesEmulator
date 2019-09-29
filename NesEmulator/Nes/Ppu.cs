using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public class Ppu : I2A03
    {
        byte[] registers = new byte[7];

        public void Clock()
        {
            registers[0x02] = 0x80;
        }

        public byte ReadByte(uint address)
        {
            return registers[address & 0x07];
        }

        public void WriteByte(uint address, byte data)
        {
            registers[address & 0x07] = data;
        }
    }
}
