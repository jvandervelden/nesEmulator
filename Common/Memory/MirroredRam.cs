using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Bus;

namespace Common.Memory
{
    public class MirroredRam : IBusInterface
    {
        private Ram _ramToMirror;
        private UInt16 _mask;

        public MirroredRam(Ram ramToMirror, UInt16 mask)
        {
            _ramToMirror = ramToMirror;
            _mask = mask;
        }

        public byte ReadByte(uint address)
        {
            return _ramToMirror.ReadByte(address & _mask);
        }

        public void WriteByte(uint address, byte data)
        {
            _ramToMirror.WriteByte(address & _mask, data);
        }
    }
}
