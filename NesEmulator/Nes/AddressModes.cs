using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Shout out to One Loan Coder for building the address mode code that I referenced/pretty much copied while writing mine.
/// https://github.com/OneLoneCoder/olcNES/blob/master/Part%232%20-%20CPU/olc6502.cpp
/// </summary>
namespace TestPGE.Nes
{
    public static class AddressModes
    {
        public static string GetAddressInfo(Func<Cpu, byte> addressMode, Cpu cpu)
        {
            if (addressMode == IMP)
                return "\t\t\t";
            if (addressMode == IMM)
                return string.Format("#${0:X2}\t\t\t", cpu.Fetched);
            if (addressMode == ZP0)
                return string.Format("${0:X2}\t\t\t", cpu.Address);
            if (addressMode == ZPX)
                return string.Format("${0:X2},x -> ${1:X4}\t", cpu.Address - cpu.X, cpu.Address);
            if (addressMode == ZPY)
                return string.Format("${0:X2},y -> ${1:X4}\t", cpu.Address - cpu.Y, cpu.Address);
            if (addressMode == REL)
                return string.Format("{0} -> ${1:X4}\t\t", cpu.BranchRelativeAddress, cpu.ProgramCounter + cpu.BranchRelativeAddress);
            if (addressMode == ABS)
                return string.Format("${0:X4}\t\t", cpu.Address);
            if (addressMode == ABX)
                return string.Format("${0:X4},x -> ${1:X4}\t", cpu.Address - cpu.X, cpu.Address);
            if (addressMode == ABY)
                return string.Format("${0:X4},y -> ${1:X4}\t", cpu.Address - cpu.Y, cpu.Address);
            if (addressMode == IND)
                return string.Format("(${0:X4}) -> ${1:X4}\t", cpu.Address, cpu.Fetched);
            if (addressMode == IZX)
                return string.Format("($ZP0,x) -> ${0:X4}\t", cpu.Address);
            if (addressMode == IZY)
                return string.Format("($ZP0),y -> ${0:X4}\t", cpu.Address);

            return "";
        }

        public static byte IMP(Cpu cpu)
        {
            cpu.ImpliedAddress = true;
            return 0;
        }

        public static byte IMM(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            cpu.Address = cpu.ProgramCounter++;
            return 0;
        }

        public static byte ZP0(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            cpu.Address = cpu.Bus.Read(cpu.ProgramCounter++);
            cpu.Address &= 0x00FF;
            return 0;
        }

        public static byte ZPX(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            cpu.Address = (UInt16)(cpu.Bus.Read(cpu.ProgramCounter++) + cpu.X);
            cpu.Address &= 0x00FF;
            return 0;
        }

        public static byte ZPY(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            cpu.Address = (UInt16)(cpu.Bus.Read(cpu.ProgramCounter++) + cpu.Y);
            cpu.Address &= 0x00FF;
            return 0;
        }

        public static byte REL(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = (SByte)cpu.Bus.Read(cpu.ProgramCounter++);
            return 0;
        }

        public static byte ABS(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            UInt16 lo = cpu.Bus.Read(cpu.ProgramCounter++);
            UInt16 hi = cpu.Bus.Read(cpu.ProgramCounter++);

            cpu.Address = (UInt16)((hi << 8) | lo);

            return 0;
        }

        public static byte ABX(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            UInt16 lo = cpu.Bus.Read(cpu.ProgramCounter++);
            UInt16 hi = cpu.Bus.Read(cpu.ProgramCounter++);

            cpu.Address = (UInt16)((hi << 8) | lo);
            cpu.Address += cpu.X;

            if ((cpu.Address & 0xFF00) != (hi << 8))
                return 1;

            return 0;
        }

        public static byte ABY(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            UInt16 lo = cpu.Bus.Read(cpu.ProgramCounter++);
            UInt16 hi = cpu.Bus.Read(cpu.ProgramCounter++);

            cpu.Address = (UInt16)((hi << 8) | lo);
            cpu.Address += cpu.Y;

            if ((cpu.Address & 0xFF00) != (hi << 8))
                return 1;

            return 0;
        }

        public static byte IND(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            UInt16 lo = cpu.Bus.Read(cpu.ProgramCounter++);
            UInt16 hi = cpu.Bus.Read(cpu.ProgramCounter++);

            UInt16 pointer = (UInt16)((hi << 8) | lo);

            if (lo == 0x00FF)
            {
                cpu.Address = (UInt16)((cpu.Bus.Read((UInt16)(pointer & 0xFF00)) << 8) | cpu.Bus.Read(pointer));
            }
            else
            {
                cpu.Address = (UInt16)((cpu.Bus.Read((UInt16)(pointer + 1)) << 8) | cpu.Bus.Read(pointer));
            }

            return 0;
        }

        public static byte IZX(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            UInt16 pointer = cpu.Bus.Read(cpu.ProgramCounter++);

            UInt16 lo = (UInt16)(cpu.Bus.Read((UInt16)((pointer + cpu.X) & 0x00FF)));
            UInt16 hi = (UInt16)(cpu.Bus.Read((UInt16)((pointer + cpu.X + 1) & 0x00FF)));

            cpu.Address = (UInt16)((hi << 8) | lo);

            return 0;
        }

        public static byte IZY(Cpu cpu)
        {
            cpu.ImpliedAddress = false;
            UInt16 pointer = cpu.Bus.Read(cpu.ProgramCounter++);

            UInt16 lo = (UInt16)(cpu.Bus.Read((UInt16)(pointer & 0x00FF)));
            UInt16 hi = (UInt16)(cpu.Bus.Read((UInt16)((pointer + 1) & 0x00FF)));

            cpu.Address = (UInt16)((hi << 8) | lo);
            cpu.Address += cpu.Y;

            if ((cpu.Address & 0xFF00) != (hi << 8))
                return 1;

            return 0;
        }
    }
}
