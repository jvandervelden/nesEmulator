using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes.Memory
{
    public class MirroredRam : IBusInterface
    {
        public uint StartAddress { get; private set; } = 0;
        public uint EndAddress { get { return StartAddress + _ramToMirror.EndAddress - _ramToMirror.StartAddress; } }

        private Ram _ramToMirror;

        public MirroredRam(Ram ramToMirror, uint startAddress = 0)
        {
            _ramToMirror = ramToMirror;
            StartAddress = startAddress;
        }

        public byte ReadByte(uint address)
        {
            return _ramToMirror.ReadByteAbs(address - StartAddress);
        }

        public void WriteByte(uint address, byte data)
        {
            _ramToMirror.WriteByteAbs(address - StartAddress, data);
        }
    }
}
