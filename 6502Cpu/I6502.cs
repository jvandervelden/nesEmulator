using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Bus;

namespace _6502Cpu
{
    public enum Flags
    {
        C = 0x01,   // Carry Bit
        Z = 0x02,   // Zero
        I = 0x04,   // Disable Interrupts
        D = 0x08,   // Decimal Mode (unused in this implementation)
        B = 0x10,   // Break
        U = 0x20,   // Unused
        V = 0x40,   // Overflow
        N = 0x80    // Negative
    }

    public interface I6502
    {
        byte A { get; set; }
        byte X { get; set; }
        byte Y { get; set; }
        byte StackPointer { get; set; }
        UInt16 ProgramCounter { get; set; }
        byte Status { get; set; }

        bool ImpliedAddress { get; set; }
        byte Fetched { get; }
        UInt16 Address { get; set; }
        SByte BranchRelativeAddress { get; set; }

        //int RemainingInstructionCycles { get; set; }

        DataBus Bus { get; set; }

        bool GetFlag(Flags flag);

        void SetFlag(Flags flag, bool set);

        UInt16 GetFullStackAddress();

        // Reset Interrupt - Forces CPU into known state
        void Reset();

        // Interrupt Request - Executes an instruction at a specific location
        void IRQ(bool SetBreak = false);

        // Non-Maskable Interrupt Request - As above, but cannot be disabled
        void NMI();

        int Clock();
    }
}
