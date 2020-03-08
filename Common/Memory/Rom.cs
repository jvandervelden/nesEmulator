using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Bus;

namespace Common.Memory
{
    public class Rom : Ram
    {
        public Rom(byte[] data) : base((uint)data.Length)
        {
            Array.Copy(data, 0, _memory, 0, data.Length);
        }
    }
}
