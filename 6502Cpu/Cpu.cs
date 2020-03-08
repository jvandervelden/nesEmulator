using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Bus;

/// <summary>
/// Shout out to One loan coder for the reference while building the structure of this CPU implementation: https://github.com/OneLoneCoder/olcNES
/// </summary>
namespace _6502Cpu
{
    public class Cpu : I6502
    {
        public const int CLOCK_DIVISOR = 12;

        public const UInt16 INITIAL_EXECUTION_ADDRESS = 0xFFFC;
        public const byte INITIAL_STACK_POINTER_ADDRESS = 0xFD;
        public const UInt16 INTERUPT_EXECUTION_ADDRESS = 0xFFFE;
        public const UInt16 NON_MASKABLE_INTERUPT_EXECUTION_ADDRESS = 0xFFFA;

        public byte A { get; set; } = 0x00;       // Accumulator Register
        public byte X { get; set; } = 0x00;       // X Register
        public byte Y { get; set; } = 0x00;       // Y Register
        public byte StackPointer { get; set; } = 0x00;    // Stack Pointer (points to location on bus)
        public UInt16 ProgramCounter { get; set; } = 0x0000;  // Program Counter
        public byte Status { get; set; } = 0x00;  // Status Register

        public bool ImpliedAddress { get; set; } = false;

        private byte? _fetched = null;
        public byte Fetched { get
            {
                if (Instruction.AddressMode == AddressModes.REL)
                    throw new Exception($"Accessing fetch with address mode of {Instruction.AddressMode.ToString()}");

                if (!_fetched.HasValue)
                {
                    if (Instruction.AddressMode == AddressModes.IMP)
                        _fetched = A;
                    else
                        _fetched = Bus.Read(Address);
                }

                return _fetched.Value;
            }
        }

        private Instruction Instruction { get; set; }

        public UInt16 Address { get; set; }
        public SByte BranchRelativeAddress { get; set; }

        //public int RemainingInstructionCycles { get; set; } = 0;
        private long ClockCycles = 0;

        private bool _interuptFlagged = false;
        private UInt16 _interuptExecutionAddress = 0x0000;
        private bool _interuptSetBreak = false;
        private int _interuptRequiredCycles = 0;

        public DataBus Bus { get; set; }

        public Cpu(DataBus bus)
        {
            Bus = bus;
        }

        private static Dictionary<byte, Instruction> BuildLookup(params Instruction[] instructions)
        {
            Dictionary<byte, Instruction> mappedInstructions = new Dictionary<byte, Instruction>();

            for (int opcode = 0; opcode < instructions.Length; opcode++)
            {
                instructions[opcode].Opcode = (byte)opcode;
                mappedInstructions.Add((byte)opcode, instructions[opcode]);
            }

            return mappedInstructions;
        }

        public bool GetFlag(Flags flag)
        {
            return (byte)(Status & (byte)flag) > 0;
        }

        public void SetFlag(Flags flag, bool set)
        {
            if (set) Status |= (byte)flag;
            else Status &= (byte)~flag;
        }

        public UInt16 GetFullStackAddress()
        {
            return (UInt16)(0x0100 + StackPointer);
        }

        // Reset Interrupt - Forces CPU into known state
        public void Reset()
        {
            UInt16 lo = Bus.Read((UInt16)(INITIAL_EXECUTION_ADDRESS + 0));
            UInt16 hi = Bus.Read((UInt16)(INITIAL_EXECUTION_ADDRESS + 1));

            // Set it
            ProgramCounter = (UInt16)((hi << 8) | lo);
            //ProgramCounter = 0xc000;

            // Reset internal registers
            A = 0;
            X = 0;
            Y = 0;
            StackPointer = INITIAL_STACK_POINTER_ADDRESS;
            Status = 0x00;

            // Clear internal helper variables
            BranchRelativeAddress = 0x00;
            Address = 0x0000;
            _fetched = null;

            // Reset takes time
            //RemainingInstructionCycles = 8;
        }

        private void JumpToInteruptAddress(UInt16 interuptAddress, bool SetBreak = false)
        {
            // Push the program counter to the stack. It's 16-bits dont
            // forget so that takes two pushes
            Bus.Write(GetFullStackAddress(), (byte)((ProgramCounter >> 8) & 0x00FF));
            StackPointer--;
            Bus.Write(GetFullStackAddress(), (byte)(ProgramCounter & 0x00FF));
            StackPointer--;

            // Then Push the status register to the stack
            SetFlag(Flags.B, SetBreak);
            SetFlag(Flags.U, true);
            SetFlag(Flags.I, true);
            Bus.Write(GetFullStackAddress(), Status);
            SetFlag(Flags.B, false);
            SetFlag(Flags.U, false);
            StackPointer--;

            // Read new program counter location from fixed address
            UInt16 lo = (UInt16)Bus.Read((UInt16)(interuptAddress + 0));
            UInt16 hi = (UInt16)Bus.Read((UInt16)(interuptAddress + 1));
            ProgramCounter = (UInt16)((hi << 8) | lo);
        }

        // Interrupt Request - Executes an instruction at a specific location
        public void IRQ(bool SetBreak = false)
        {
            if (!GetFlag(Flags.I))
            {
                _interuptFlagged = true;
                _interuptExecutionAddress = INTERUPT_EXECUTION_ADDRESS;
                _interuptSetBreak = SetBreak;
                _interuptRequiredCycles = 7;
            }
        }

        // Non-Maskable Interrupt Request - As above, but cannot be disabled
        public void NMI()
        {
            _interuptFlagged = true;
            _interuptExecutionAddress = NON_MASKABLE_INTERUPT_EXECUTION_ADDRESS;
            _interuptSetBreak = false;
            _interuptRequiredCycles = 8;
        }

        public int Clock()
        {
            int instructionCycles = 1;

            if (_interuptFlagged)
            {
                JumpToInteruptAddress(_interuptExecutionAddress, _interuptSetBreak);
                _interuptFlagged = false;
                instructionCycles = _interuptRequiredCycles;
            } else { 

                byte nextOpcode = Bus.Read(ProgramCounter);
                                
                ProgramCounter++;

                Instruction = InstructionSet.InstuctionsByOpcode[nextOpcode];

                instructionCycles = Instruction.Cycles;

                try
                {
                    byte addressModeExtra = Instruction.AddressMode(this);
                    byte operateExtra = Instruction.Operate(this);

                    _fetched = null;

                    instructionCycles += addressModeExtra & operateExtra;
                }
                catch (NotImplementedException)
                {
                    Console.Error.WriteLine("Encountered un supported opcode: {0} - {1:X2}", Instruction.Name, nextOpcode);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Encountered error: {e.Message}");
                }
            }

            ClockCycles += instructionCycles;
            return instructionCycles;
        }
    }
}
