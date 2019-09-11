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
        public uint StartAddress { get; private set; } = 0;
        public uint EndAddress { get { return StartAddress + (uint)_memory.Length - 1; } }

        public Ram(uint size, uint startAddress = 0)
        {
            StartAddress = startAddress;
            _memory = new byte[size];

            for (uint i = 0; i < size; i++) _memory[i] = 0x00;
        }

        public byte ReadByteAbs(uint address)
        {
            if (_memory.Length < address)
            {
                throw new ArgumentOutOfRangeException("address", string.Format("Invalid memory address Abs {0:X4} Rel {1:X4}", address, address + StartAddress));
            }

            return _memory[address];
        }

        public void WriteByteAbs(uint address, byte data)
        {
            if (_memory.Length < address)
            {
                throw new ArgumentOutOfRangeException("address", string.Format("Invalid memory address: Abs {0:X4} Rel {1:X4}", address, address + StartAddress));
            }

            _memory[address] = data;
        }

        public byte ReadByte(uint address)
        {
            if (address < StartAddress) throw new ArgumentOutOfRangeException($"Address is out of range: {address:X4}");

            return ReadByteAbs(address - StartAddress);
        }

        public void WriteByte(uint address, byte data)
        {
            if (address < StartAddress) throw new ArgumentOutOfRangeException($"Address is out of range: {address:X4}");

            WriteByteAbs(address - StartAddress, data);
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
