using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Bus
{
    public interface IBusInterface
    {
        byte ReadByte(uint address);

        void WriteByte(uint address, byte data);
    }
}
