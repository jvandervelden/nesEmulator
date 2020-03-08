using Common.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public class Controller : IBusInterface
    {
        public enum ControllerButtons
        {
            A = 0x01,
            B = 0x02,
            SELECT = 0x04,
            START = 0x08,
            UP = 0x10,
            DOWN = 0x20,
            LEFT = 0x40,
            RIGHT = 0x80
        }

        byte _currentButtonState = 0x00;
        byte _buttonShiftRegister = 0x00;
        byte _controlRegister = 0x00;

        public void SetButton(ControllerButtons button, bool pressed)
        {
            if (pressed)
                _currentButtonState |= (byte)button;
            else
                _currentButtonState &= (byte)~button;
        }

        public byte ReadByte(uint address)
        {
            if ((_controlRegister & 0x01) == 0x00)
            {
                byte data = (byte)(_buttonShiftRegister & 0x01);

                _buttonShiftRegister >>= 1;

                return data;
            }

            return 0x00;
        }

        public void WriteByte(uint address, byte data)
        {
            _controlRegister = data;

            if ((_controlRegister & 0x01) == 0x01)
            {
                _buttonShiftRegister = _currentButtonState;
            }
        }
    }
}
