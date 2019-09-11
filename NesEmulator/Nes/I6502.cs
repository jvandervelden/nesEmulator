using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public enum Flags
    {
        C = (byte)(1 << 0),   // Carry Bit
        Z = (byte)(1 << 1),   // Zero
        I = (byte)(1 << 2),   // Disable Interrupts
        D = (byte)(1 << 3),   // Decimal Mode (unused in this implementation)
        B = (byte)(1 << 4),   // Break
        U = (byte)(1 << 5),   // Unused
        V = (byte)(1 << 6),   // Overflow
        N = (byte)(1 << 7)    // Negative
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
        byte Fetched { get; set; }
        UInt16 Address { get; set; }
        SByte BranchRelativeAddress { get; set; }

        int RemainingInstructionCycles { get; set; }

        DataBus Bus { get; set; }

        bool GetFlag(Flags flag);

        void SetFlag(Flags flag, bool set);

        // Reset Interrupt - Forces CPU into known state
        void Reset();

        // Interrupt Request - Executes an instruction at a specific location
        void irq(bool SetBreak = false);

        // Non-Maskable Interrupt Request - As above, but cannot be disabled
        void nmi();

        void clock();
    }
}
