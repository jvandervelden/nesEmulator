using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public interface IPPU : IBusInterface
    {
        void SetRegister(byte registerNumber, byte data);
        byte GetRegister(byte registerNumber);
        void Clock();
        Image PrintPattern(byte tableNumber, byte sprite);
    }
}
