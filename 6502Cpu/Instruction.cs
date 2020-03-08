using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6502Cpu
{
    public class Instruction
    {
        public byte Opcode { get; set; }
        public string Name { get; private set; }
        public Func<Cpu, byte> Operate { get; private set; }
        public Func<Cpu, byte> AddressMode { get; private set; }
        public byte Cycles { get; private set; } = 0;
        
        public static Instruction Init(string name, Func<Cpu, byte> operate, Func<Cpu, byte> addressMode, byte cycles)
        {
            return new Instruction
            {
                Name = name,
                Operate = operate,
                AddressMode = addressMode,
                Cycles = cycles
            };
        }
    }
}
