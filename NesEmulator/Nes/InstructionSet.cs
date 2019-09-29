using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Shout out to One Loan Coder for building the instruction set code that I referenced while writing mine.
/// https://github.com/OneLoneCoder/olcNES/blob/master/Part%232%20-%20CPU/olc6502.cpp
/// </summary>
namespace TestPGE.Nes
{
    public static class InstructionSet
    {
        public static void UpdateCarry(I6502 cpu, UInt16 value)
        {
            cpu.SetFlag(Flags.C, value > 255);
        }

        public static void UpdateZero(I6502 cpu, UInt16 value)
        {
            cpu.SetFlag(Flags.Z, (value & 0x00FF) == 0);
        }

        public static void UpdateNegative(I6502 cpu, UInt16 value)
        {
            cpu.SetFlag(Flags.N, (value & 0x0080) == 0x0080);
        }

        public static byte ADC(I6502 cpu)
        {
            UInt16 sum = (UInt16)(cpu.A + cpu.Fetched + (cpu.GetFlag(Flags.C) ? 1 : 0));

            UpdateCarry(cpu, sum);
            UpdateZero(cpu, sum);
            UpdateNegative(cpu, sum);
            cpu.SetFlag(Flags.V, ((cpu.A ^ (byte)sum) & ~(cpu.A ^ cpu.Fetched) & 0x0080) == 0x0080);

            cpu.A = (byte)(sum & 0x00FF);

            return 1;
        }
        public static byte AND(I6502 cpu)
        {
            cpu.A &= cpu.Fetched;

            UpdateZero(cpu, cpu.A);
            UpdateNegative(cpu, cpu.A);

            return 1;
        }

        public static byte ASL(I6502 cpu)
        {
            UInt16 shiftedValue = (UInt16)(cpu.Fetched << 1);

            UpdateCarry(cpu, shiftedValue);
            UpdateZero(cpu, shiftedValue);
            UpdateNegative(cpu, shiftedValue);

            if (cpu.ImpliedAddress)
                cpu.A = (byte)(shiftedValue & 0x00FF);
            else
                cpu.Bus.Write(cpu.Address, (byte)shiftedValue);

            return 0;
        }

        public static byte BCC(I6502 cpu)
        {
            if (!cpu.GetFlag(Flags.C))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }

        public static byte BCS(I6502 cpu)
        {
            if (cpu.GetFlag(Flags.C))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }
        
        public static byte BEQ(I6502 cpu)
        {
            if (cpu.GetFlag(Flags.Z))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }

        public static byte BIT(I6502 cpu)
        {
            byte test = (byte)(cpu.A & cpu.Fetched);

            UpdateZero(cpu, test);
            UpdateNegative(cpu, test);
            cpu.SetFlag(Flags.V, (test & 0b0100_0000) == 0b0100_0000);

            return 0;
        }
        public static byte BMI(I6502 cpu)
        {
            if (cpu.GetFlag(Flags.N))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }

        public static byte BNE(I6502 cpu)
        {
            if (!cpu.GetFlag(Flags.Z))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }

        public static byte BPL(I6502 cpu)
        {
            if (!cpu.GetFlag(Flags.N))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }

        public static byte BRK(I6502 cpu)
        {
            cpu.irq(true);
            return 0;
        }

        public static byte BVC(I6502 cpu)
        {
            if (!cpu.GetFlag(Flags.V))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }

        public static byte BVS(I6502 cpu)
        {
            if (cpu.GetFlag(Flags.V))
            {
                cpu.RemainingInstructionCycles++;
                cpu.Address = (UInt16)(cpu.ProgramCounter + cpu.BranchRelativeAddress);

                if ((cpu.Address & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
                {
                    cpu.RemainingInstructionCycles++;
                }

                cpu.ProgramCounter = cpu.Address;
            }

            return 0;
        }
        public static byte CLC(I6502 cpu)
        {
            cpu.SetFlag(Flags.C, false);
            return 0;
        }
        public static byte CLD(I6502 cpu)
        {
            cpu.SetFlag(Flags.D, false);
            return 0;
        }

        public static byte CLI(I6502 cpu)
        {
            cpu.SetFlag(Flags.I, false);
            return 0;
        }

        public static byte CLV(I6502 cpu)
        {
            cpu.SetFlag(Flags.V, false);
            return 0;
        }

        public static byte CMP(I6502 cpu)
        {
            cpu.SetFlag(Flags.C, cpu.A >= cpu.Fetched);
            cpu.SetFlag(Flags.Z, cpu.A == cpu.Fetched);
            cpu.SetFlag(Flags.N, ((cpu.A - cpu.Fetched) & 0x80) == 0x80);

            return 1;
        }
        public static byte CPX(I6502 cpu)
        {
            cpu.SetFlag(Flags.C, cpu.X >= cpu.Fetched);
            cpu.SetFlag(Flags.Z, cpu.X == cpu.Fetched);
            cpu.SetFlag(Flags.N, ((cpu.X - cpu.Fetched) & 0x80) == 0x80);

            return 0;
        }

        public static byte CPY(I6502 cpu)
        {
            cpu.SetFlag(Flags.C, cpu.Y >= cpu.Fetched);
            cpu.SetFlag(Flags.Z, cpu.Y == cpu.Fetched);
            cpu.SetFlag(Flags.N, ((cpu.Y - cpu.Fetched) & 0x80) == 0x80);

            return 0;
        }

        public static byte DEC(I6502 cpu)
        {
            byte decrementedValue = (byte)(cpu.Fetched - 1);
            cpu.Bus.Write(cpu.Address, decrementedValue);
            UpdateNegative(cpu, decrementedValue);
            UpdateZero(cpu, decrementedValue);
            return 0;
        }

        public static byte DEX(I6502 cpu)
        {
            cpu.X--;
            UpdateNegative(cpu, cpu.X);
            UpdateZero(cpu, cpu.X);
            return 0;
        }

        public static byte DEY(I6502 cpu)
        {
            cpu.Y--;
            UpdateNegative(cpu, cpu.Y);
            UpdateZero(cpu, cpu.Y);
            return 0;
        }

        public static byte EOR(I6502 cpu)
        {
            cpu.A ^= cpu.Fetched;

            UpdateNegative(cpu, cpu.A);
            UpdateZero(cpu, cpu.A);

            return 1;
        }

        public static byte INC(I6502 cpu)
        {
            byte incrementedValue = (byte)(cpu.Fetched + 1);

            UpdateNegative(cpu, incrementedValue);
            UpdateZero(cpu, incrementedValue);

            cpu.Bus.Write(cpu.Address, incrementedValue);

            return 0;
        }
        public static byte INX(I6502 cpu)
        {
            cpu.X++;

            UpdateNegative(cpu, cpu.X);
            UpdateZero(cpu, cpu.X);

            return 0;
        }

        public static byte INY(I6502 cpu)
        {
            cpu.Y++;

            UpdateNegative(cpu, cpu.Y);
            UpdateZero(cpu, cpu.Y);

            return 0;
        }

        public static byte JMP(I6502 cpu)
        {
            cpu.ProgramCounter = cpu.Address;

            return 0;
        }
        public static byte JSR(I6502 cpu)
        {
            cpu.Bus.Write(cpu.StackPointer--, (byte)((cpu.ProgramCounter - 1 >> 8) & 0x00FF));
            cpu.Bus.Write(cpu.StackPointer--, (byte)(cpu.ProgramCounter - 1 & 0x00FF));

            cpu.ProgramCounter = cpu.Address;

            return 0;
        }

        public static byte LDA(I6502 cpu)
        {
            UpdateNegative(cpu, cpu.Fetched);
            UpdateZero(cpu, cpu.Fetched);
            cpu.A = cpu.Fetched;
            return 1;
        }

        public static byte LDX(I6502 cpu)
        {
            UpdateNegative(cpu, cpu.Fetched);
            UpdateZero(cpu, cpu.Fetched);
            cpu.X = cpu.Fetched;

            return 1;
        }

        public static byte LDY(I6502 cpu)
        {
            UpdateNegative(cpu, cpu.Fetched);
            UpdateZero(cpu, cpu.Fetched);
            cpu.Y = cpu.Fetched;

            return 1;
        }

        public static byte LSR(I6502 cpu)
        {
            byte shifted = (byte)(cpu.Fetched >> 1);
            cpu.SetFlag(Flags.C, (cpu.Fetched & 0x01) == 0x01);
            cpu.SetFlag(Flags.N, false);
            UpdateZero(cpu, shifted);

            if (cpu.ImpliedAddress)
                cpu.A = shifted;
            else
                cpu.Bus.Write(cpu.Address, shifted);

            return 0;
        }

        public static byte NOP(I6502 cpu)
        {
            return 0;
        }

        public static byte ORA(I6502 cpu)
        {
            cpu.A |= cpu.Fetched;

            UpdateNegative(cpu, cpu.A);
            UpdateZero(cpu, cpu.A);

            return 1;
        }

        public static byte PHA(I6502 cpu)
        {
            cpu.Bus.Write(cpu.StackPointer--, cpu.A);
            return 0;
        }

        public static byte PHP(I6502 cpu)
        {
            cpu.Bus.Write(cpu.StackPointer--, cpu.Status);
            return 0;
        }

        public static byte PLA(I6502 cpu)
        {
            cpu.A = cpu.Bus.Read(++cpu.StackPointer);

            UpdateZero(cpu, cpu.A);
            UpdateNegative(cpu, cpu.A);

            return 0;
        }

        public static byte PLP(I6502 cpu)
        {
            cpu.Status = cpu.Bus.Read(++cpu.StackPointer);
            return 0;
        }

        public static byte ROL(I6502 cpu)
        {
            UInt16 shifted = (UInt16)(cpu.Fetched << 1);

            if (cpu.GetFlag(Flags.C)) shifted++;

            UpdateCarry(cpu, shifted);
            UpdateZero(cpu, (byte)shifted);
            UpdateNegative(cpu, shifted);

            if (cpu.ImpliedAddress)
                cpu.A = (byte)(shifted & 0x00FF);
            else
                cpu.Bus.Write(cpu.Address, (byte)shifted);

            return 0;
        }

        public static byte ROR(I6502 cpu)
        {
            UInt16 shifted = cpu.Fetched;

            // Add carry to bit 8 if set
            if (cpu.GetFlag(Flags.C)) shifted |= 0x100;

            // Update carry flag if bit 0 is 1.
            cpu.SetFlag(Flags.C, (shifted & 0x01) == 0x01);
            
            // Shift
            shifted = (byte)(shifted >> 1);

            UpdateZero(cpu, (byte)shifted);
            UpdateNegative(cpu, shifted);

            if (cpu.ImpliedAddress)
                cpu.A = (byte)(shifted & 0x00FF);
            else
                cpu.Bus.Write(cpu.Address, (byte)shifted);

            return 0;
        }

        public static byte RTI(I6502 cpu)
        {
            cpu.Status = cpu.Bus.Read(++cpu.StackPointer);
            UInt16 lo = cpu.Bus.Read(++cpu.StackPointer);
            UInt16 hi = cpu.Bus.Read(++cpu.StackPointer);

            cpu.ProgramCounter = (UInt16)((hi << 8) | lo);

            return 0;
        }

        public static byte RTS(I6502 cpu)
        {
            UInt16 lo = cpu.Bus.Read(++cpu.StackPointer);
            UInt16 hi = cpu.Bus.Read(++cpu.StackPointer);

            cpu.ProgramCounter = (UInt16)(((hi << 8) | lo) + 1);

            return 0;
        }


        /**
         * Mirror of OLC add Overflow truth table for subtraction
         * 
         * A M ~M R | V | A^M | A^R | ~M^R |
         * ---------------------------------
         * 0 0  1 0 | 0 |  0  |  0  |   1  
         * 0 0  1 1 | 0 |  0  |  1  |   0  
         * 0 1  0 0 | 0 |  1  |  0  |   0  
         * 0 1  0 1 | 1 |  1  |  1  |   1  
         * 1 0  1 0 | 1 |  1  |  1  |   1  
         * 1 0  1 1 | 0 |  1  |  0  |   0  
         * 1 1  0 0 | 0 |  0  |  1  |   0  
         * 1 1  0 1 | 0 |  0  |  0  |   1  
         */
        public static byte SBC(I6502 cpu)
        {
            UInt16 inverse = (UInt16)~cpu.Fetched;

            UInt16 difference = (UInt16)(cpu.A + inverse + (cpu.GetFlag(Flags.C) ? 1 : 0));

            cpu.SetFlag(Flags.C, difference > 255);
            cpu.SetFlag(Flags.Z, (difference & 0x00FF) == 0);
            cpu.SetFlag(Flags.N, (difference & 0x0080) == 0x0080);
            cpu.SetFlag(Flags.V, ((cpu.A ^ difference) & (cpu.A ^ cpu.Fetched) & 0x0080) == 0x0080);

            cpu.A = (byte)(difference & 0x00FF);

            return 1;
        }
                
        public static byte SEC(I6502 cpu)
        {
            cpu.SetFlag(Flags.C, true);
            return 0;
        }

        public static byte SED(I6502 cpu)
        {
            cpu.SetFlag(Flags.D, true);
            return 0;
        }

        public static byte SEI(I6502 cpu)
        {
            cpu.SetFlag(Flags.I, true);
            return 0;
        }

        public static byte STA(I6502 cpu)
        {
            cpu.Bus.Write(cpu.Address, cpu.A);

            return 0;
        }

        public static byte STX(I6502 cpu)
        {
            cpu.Bus.Write(cpu.Address, cpu.X);

            return 0;
        }

        public static byte STY(I6502 cpu)
        {
            cpu.Bus.Write(cpu.Address, cpu.Y);

            return 0;
        }

        public static byte TAX(I6502 cpu)
        {
            cpu.X = cpu.A;

            UpdateNegative(cpu, cpu.X);
            UpdateZero(cpu, cpu.X);

            return 0;
        }

        public static byte TAY(I6502 cpu)
        {
            cpu.Y = cpu.A;

            UpdateNegative(cpu, cpu.Y);
            UpdateZero(cpu, cpu.Y);

            return 0;
        }

        public static byte TSX(I6502 cpu)
        {
            cpu.X = cpu.StackPointer;

            UpdateNegative(cpu, cpu.X);
            UpdateZero(cpu, cpu.X);

            return 0;
        }

        public static byte TXA(I6502 cpu)
        {
            cpu.A = cpu.X;

            UpdateNegative(cpu, cpu.A);
            UpdateZero(cpu, cpu.A);

            return 0;
        }
        public static byte TXS(I6502 cpu)
        {
            cpu.StackPointer = cpu.X;

            UpdateNegative(cpu, cpu.StackPointer);
            UpdateZero(cpu, cpu.StackPointer);

            return 0;
        }
        public static byte TYA(I6502 cpu)
        {
            cpu.A = cpu.Y;

            UpdateNegative(cpu, cpu.A);
            UpdateZero(cpu, cpu.A);

            return 0;
        }

        public static byte XXX(I6502 cpu)
        {
            throw new NotImplementedException("Invalid Op code");
        }

        /// <summary>
        /// Array of instructions for the 6502 cpu. The index into the array is the opcode.
        /// </summary>
        public static Instruction[] InstuctionsByOpcode { get; } = {
            Instruction.Init("BRK", InstructionSet.BRK, AddressModes.IMM, 7), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.IZX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 3), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.ZP0, 3), Instruction.Init("ASL", InstructionSet.ASL, AddressModes.ZP0, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("PHP", InstructionSet.PHP, AddressModes.IMP, 3), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.IMM, 2), Instruction.Init("ASL", InstructionSet.ASL, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.ABS, 4), Instruction.Init("ASL", InstructionSet.ASL, AddressModes.ABS, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6),
            Instruction.Init("BPL", InstructionSet.BPL, AddressModes.REL, 2), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.IZY, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.ZPX, 4), Instruction.Init("ASL", InstructionSet.ASL, AddressModes.ZPX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("CLC", InstructionSet.CLC, AddressModes.IMP, 2), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.ABY, 4), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("ORA", InstructionSet.ORA, AddressModes.ABX, 4), Instruction.Init("ASL", InstructionSet.ASL, AddressModes.ABX, 7), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7),
            Instruction.Init("JSR", InstructionSet.JSR, AddressModes.ABS, 6), Instruction.Init("AND", InstructionSet.AND, AddressModes.IZX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("BIT", InstructionSet.BIT, AddressModes.ZP0, 3), Instruction.Init("AND", InstructionSet.AND, AddressModes.ZP0, 3), Instruction.Init("ROL", InstructionSet.ROL, AddressModes.ZP0, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("PLP", InstructionSet.PLP, AddressModes.IMP, 4), Instruction.Init("AND", InstructionSet.AND, AddressModes.IMM, 2), Instruction.Init("ROL", InstructionSet.ROL, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("BIT", InstructionSet.BIT, AddressModes.ABS, 4), Instruction.Init("AND", InstructionSet.AND, AddressModes.ABS, 4), Instruction.Init("ROL", InstructionSet.ROL, AddressModes.ABS, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6),
            Instruction.Init("BMI", InstructionSet.BMI, AddressModes.REL, 2), Instruction.Init("AND", InstructionSet.AND, AddressModes.IZY, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("AND", InstructionSet.AND, AddressModes.ZPX, 4), Instruction.Init("ROL", InstructionSet.ROL, AddressModes.ZPX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("SEC", InstructionSet.SEC, AddressModes.IMP, 2), Instruction.Init("AND", InstructionSet.AND, AddressModes.ABY, 4), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("AND", InstructionSet.AND, AddressModes.ABX, 4), Instruction.Init("ROL", InstructionSet.ROL, AddressModes.ABX, 7), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7),
            Instruction.Init("RTI", InstructionSet.RTI, AddressModes.IMP, 6), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.IZX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 3), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.ZP0, 3), Instruction.Init("LSR", InstructionSet.LSR, AddressModes.ZP0, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("PHA", InstructionSet.PHA, AddressModes.IMP, 3), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.IMM, 2), Instruction.Init("LSR", InstructionSet.LSR, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("JMP", InstructionSet.JMP, AddressModes.ABS, 3), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.ABS, 4), Instruction.Init("LSR", InstructionSet.LSR, AddressModes.ABS, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6),
            Instruction.Init("BVC", InstructionSet.BVC, AddressModes.REL, 2), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.IZY, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.ZPX, 4), Instruction.Init("LSR", InstructionSet.LSR, AddressModes.ZPX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("CLI", InstructionSet.CLI, AddressModes.IMP, 2), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.ABY, 4), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("EOR", InstructionSet.EOR, AddressModes.ABX, 4), Instruction.Init("LSR", InstructionSet.LSR, AddressModes.ABX, 7), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7),
            Instruction.Init("RTS", InstructionSet.RTS, AddressModes.IMP, 6), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.IZX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 3), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.ZP0, 3), Instruction.Init("ROR", InstructionSet.ROR, AddressModes.ZP0, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("PLA", InstructionSet.PLA, AddressModes.IMP, 4), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.IMM, 2), Instruction.Init("ROR", InstructionSet.ROR, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("JMP", InstructionSet.JMP, AddressModes.IND, 5), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.ABS, 4), Instruction.Init("ROR", InstructionSet.ROR, AddressModes.ABS, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6),
            Instruction.Init("BVS", InstructionSet.BVS, AddressModes.REL, 2), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.IZY, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.ZPX, 4), Instruction.Init("ROR", InstructionSet.ROR, AddressModes.ZPX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("SEI", InstructionSet.SEI, AddressModes.IMP, 2), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.ABY, 4), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("ADC", InstructionSet.ADC, AddressModes.ABX, 4), Instruction.Init("ROR", InstructionSet.ROR, AddressModes.ABX, 7), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7),
            Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("STA", InstructionSet.STA, AddressModes.IZX, 6), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("STY", InstructionSet.STY, AddressModes.ZP0, 3), Instruction.Init("STA", InstructionSet.STA, AddressModes.ZP0, 3), Instruction.Init("STX", InstructionSet.STX, AddressModes.ZP0, 3), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 3), Instruction.Init("DEY", InstructionSet.DEY, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("TXA", InstructionSet.TXA, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("STY", InstructionSet.STY, AddressModes.ABS, 4), Instruction.Init("STA", InstructionSet.STA, AddressModes.ABS, 4), Instruction.Init("STX", InstructionSet.STX, AddressModes.ABS, 4), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 4),
            Instruction.Init("BCC", InstructionSet.BCC, AddressModes.REL, 2), Instruction.Init("STA", InstructionSet.STA, AddressModes.IZY, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("STY", InstructionSet.STY, AddressModes.ZPX, 4), Instruction.Init("STA", InstructionSet.STA, AddressModes.ZPX, 4), Instruction.Init("STX", InstructionSet.STX, AddressModes.ZPY, 4), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 4), Instruction.Init("TYA", InstructionSet.TYA, AddressModes.IMP, 2), Instruction.Init("STA", InstructionSet.STA, AddressModes.ABY, 5), Instruction.Init("TXS", InstructionSet.TXS, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 5), Instruction.Init("STA", InstructionSet.STA, AddressModes.ABX, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5),
            Instruction.Init("LDY", InstructionSet.LDY, AddressModes.IMM, 2), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.IZX, 6), Instruction.Init("LDX", InstructionSet.LDX, AddressModes.IMM, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("LDY", InstructionSet.LDY, AddressModes.ZP0, 3), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.ZP0, 3), Instruction.Init("LDX", InstructionSet.LDX, AddressModes.ZP0, 3), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 3), Instruction.Init("TAY", InstructionSet.TAY, AddressModes.IMP, 2), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.IMM, 2), Instruction.Init("TAX", InstructionSet.TAX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("LDY", InstructionSet.LDY, AddressModes.ABS, 4), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.ABS, 4), Instruction.Init("LDX", InstructionSet.LDX, AddressModes.ABS, 4), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 4),
            Instruction.Init("BCS", InstructionSet.BCS, AddressModes.REL, 2), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.IZY, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("LDY", InstructionSet.LDY, AddressModes.ZPX, 4), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.ZPX, 4), Instruction.Init("LDX", InstructionSet.LDX, AddressModes.ZPY, 4), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 4), Instruction.Init("CLV", InstructionSet.CLV, AddressModes.IMP, 2), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.ABY, 4), Instruction.Init("TSX", InstructionSet.TSX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 4), Instruction.Init("LDY", InstructionSet.LDY, AddressModes.ABX, 4), Instruction.Init("LDA", InstructionSet.LDA, AddressModes.ABX, 4), Instruction.Init("LDX", InstructionSet.LDX, AddressModes.ABY, 4), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 4),
            Instruction.Init("CPY", InstructionSet.CPY, AddressModes.IMM, 2), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.IZX, 6), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("CPY", InstructionSet.CPY, AddressModes.ZP0, 3), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.ZP0, 3), Instruction.Init("DEC", InstructionSet.DEC, AddressModes.ZP0, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("INY", InstructionSet.INY, AddressModes.IMP, 2), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.IMM, 2), Instruction.Init("DEX", InstructionSet.DEX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("CPY", InstructionSet.CPY, AddressModes.ABS, 4), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.ABS, 4), Instruction.Init("DEC", InstructionSet.DEC, AddressModes.ABS, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6),
            Instruction.Init("BNE", InstructionSet.BNE, AddressModes.REL, 2), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.IZY, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.ZPX, 4), Instruction.Init("DEC", InstructionSet.DEC, AddressModes.ZPX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("CLD", InstructionSet.CLD, AddressModes.IMP, 2), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.ABY, 4), Instruction.Init("NOP", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("CMP", InstructionSet.CMP, AddressModes.ABX, 4), Instruction.Init("DEC", InstructionSet.DEC, AddressModes.ABX, 7), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7),
            Instruction.Init("CPX", InstructionSet.CPX, AddressModes.IMM, 2), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.IZX, 6), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("CPX", InstructionSet.CPX, AddressModes.ZP0, 3), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.ZP0, 3), Instruction.Init("INC", InstructionSet.INC, AddressModes.ZP0, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 5), Instruction.Init("INX", InstructionSet.INX, AddressModes.IMP, 2), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.IMM, 2), Instruction.Init("NOP", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.SBC, AddressModes.IMP, 2), Instruction.Init("CPX", InstructionSet.CPX, AddressModes.ABS, 4), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.ABS, 4), Instruction.Init("INC", InstructionSet.INC, AddressModes.ABS, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6),
            Instruction.Init("BEQ", InstructionSet.BEQ, AddressModes.REL, 2), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.IZY, 5), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 8), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.ZPX, 4), Instruction.Init("INC", InstructionSet.INC, AddressModes.ZPX, 6), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 6), Instruction.Init("SED", InstructionSet.SED, AddressModes.IMP, 2), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.ABY, 4), Instruction.Init("NOP", InstructionSet.NOP, AddressModes.IMP, 2), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7), Instruction.Init("???", InstructionSet.NOP, AddressModes.IMP, 4), Instruction.Init("SBC", InstructionSet.SBC, AddressModes.ABX, 4), Instruction.Init("INC", InstructionSet.INC, AddressModes.ABX, 7), Instruction.Init("???", InstructionSet.XXX, AddressModes.IMP, 7)
        };
    }
}
