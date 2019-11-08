using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes
{
    public class CpuExtended : I2A03
    {
        private IOamDma _oamDma;
        private I6502 _cpu;

        public CpuExtended(IOamDma oamDma, I6502 cpu)
        {
            _oamDma = oamDma;
            _cpu = cpu;
        }

        public byte ReadByte(uint address)
        {
            if (address >= 0x4000 && address <= 0x4013 || address == 0x4015)
            {
                // Handle APU
            }
            else if (address == 0x4014)
            {
                // Handle OAMDMA
            }
            else if (address == 0x4016)
            {
                // Handle Joy1
            }
            else if (address == 0x4017)
            {
                // Handle Joy2 and APU
            }

            return 0x00;
        }

        public void WriteByte(uint address, byte data)
        {
            if (address >= 0x4000 && address <= 0x4013 || address == 0x4015)
            {
                // Handle APU
            }
            else if (address == 0x4014)
            {
                _oamDma.TransferBlock((UInt16)(data << 8), 0, 256);
                // Takes 513 or 514 cycles to do OAMDMA.
                _cpu.RemainingInstructionCycles += 513 + (_cpu.RemainingInstructionCycles % 2 == 0 ? 0 : 1);
            }
            else if (address == 0x4016)
            {
                // Handle Joy1
            }
            else if (address == 0x4017)
            {
                // Handle Joy2 and APU
            }
        }
    }
}
