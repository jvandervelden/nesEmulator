using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes.Memory
{
    public class Ram : IBusInterface
    {
        protected byte[] _memory;
        protected uint _addressMask;

        public Ram(uint size)
        {
            _memory = new byte[size];
            _addressMask = size - 1;
        }

        public virtual byte ReadByte(uint address)
        {
            return _memory[address & _addressMask];
        }

        public virtual byte[] ReadBlock(uint address, long size)
        {
            byte[] data = new byte[size];
            Array.Copy(_memory, address & _addressMask, data, 0, size);
            return data;
        }

        public virtual void WriteByte(uint address, byte data)
        {
            _memory[address & _addressMask] = data;
        }

        public string HexDump(int index, int length, int lineWidth = 16)
        {
            int currentLineIndex = 0;
            int groupCount = 0;
            StringBuilder hexDump = new StringBuilder();

            for (int i = index; i < length * lineWidth; i++)
            {
                if (i >= _memory.Length)
                {
                    break;
                }

                if (currentLineIndex == 0)
                {
                    hexDump.Append(" ").Append(i.ToString("X4")).Append(": ");
                }

                hexDump.Append(_memory[i].ToString("X2"));

                if (++groupCount == 4)
                {
                    groupCount = 0;
                    hexDump.Append(" ");
                }
                
                if (++currentLineIndex == lineWidth)
                {
                    hexDump.Append("\n");
                    currentLineIndex = 0;
                }
            }

            return hexDump.ToString();
        }
    }
}
