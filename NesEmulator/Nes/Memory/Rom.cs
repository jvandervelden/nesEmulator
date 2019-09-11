using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes.Memory
{
    public class Rom : Ram, IBusInterface
    {
        private readonly HashSet<uint> _writtenAddresses = new HashSet<uint>();

        public Rom(uint endingAddress = 0) : base(0x00, endingAddress) { }

        public Rom(uint size, uint endingAddress = 0) : base(size, endingAddress - size) { }

        public Rom(byte[] data, uint endingAddress) : base(size: (uint)data.Length, startAddress: endingAddress - (uint)data.Length + 1)
        {
            Array.Copy(data, 0, _memory, 0, data.Length);
        }

        public Rom(byte[] data, Dictionary<uint, byte> writtenData, uint endingAddress) : base((uint)data.Length, endingAddress - (uint)data.Length)
        {
            Array.Copy(data, 0, _memory, 0, data.Length);
            foreach (KeyValuePair<uint, byte> dataToWrite in writtenData) _memory[dataToWrite.Key] = dataToWrite.Value;
        }

        public new void WriteByte(uint address, byte data)
        {
            base.WriteByte(address, data);
            _writtenAddresses.Add(address - StartAddress);
        }
    }
}
