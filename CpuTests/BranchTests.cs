using System;
using _6502Cpu;
using Xunit;

namespace NesInstructionSetTests
{
    public class BranchTests
    {
        public void TestWhenFlagImpeeding(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, !flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = 4;
            cpu.Address = 0x0046;
            cpu.ProgramCounter = 0x0040;

            Assert.Equal(0, instructionToTest(cpu));

            Assert.Equal(0x0040, cpu.ProgramCounter);
            Assert.Equal(0x0046, cpu.Address);
        }

        public void TestWhenFlagClearLookAhead(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = 4;
            cpu.ProgramCounter = 0x0046;

            Assert.Equal(0, instructionToTest(cpu));

            Assert.Equal(0x004A, cpu.ProgramCounter);
            Assert.Equal(0x004A, cpu.Address);
        }

        public void TestWhenFlagClearLookAheadAcrossPage(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = 4;
            cpu.ProgramCounter = 0x00FD;

            Assert.Equal(0, instructionToTest(cpu));

            Assert.Equal(0x0101, cpu.ProgramCounter);
            Assert.Equal(0x0101, cpu.Address);
        }

        public void TestWhenFlagClearLookBehind(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = -4;
            cpu.ProgramCounter = 0x0046;

            Assert.Equal(0, instructionToTest(cpu));

            Assert.Equal(0x0042, cpu.ProgramCounter);
            Assert.Equal(0x0042, cpu.Address);
        }

        public void TestWhenFlagClearLookBehindAcrossPage(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = -4;
            cpu.ProgramCounter = 0x0101;

            Assert.Equal(0, instructionToTest(cpu));

            Assert.Equal(0x00FD, cpu.ProgramCounter);
            Assert.Equal(0x00FD, cpu.Address);
        }

        // Branch if Carry Clear

        [Fact]
        public void TestCarryClearSet() { TestWhenFlagImpeeding(Flags.C, InstructionSet.BCC, false); }

        [Fact]
        public void TestCarryClearLookAhead() { TestWhenFlagClearLookAhead(Flags.C, InstructionSet.BCC, false); }

        [Fact]
        public void TestCarryClearLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.C, InstructionSet.BCC, false); }

        [Fact]
        public void TestCarryClearLookBehind() { TestWhenFlagClearLookBehind(Flags.C, InstructionSet.BCC, false); }

        [Fact]
        public void TestCarryClearLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.C, InstructionSet.BCC, false); }

        // Branch if Carry Set

        [Fact]
        public void TestCarrySetClear() { TestWhenFlagImpeeding(Flags.C, InstructionSet.BCS, true); }

        [Fact]
        public void TestCarrySetLookAhead() { TestWhenFlagClearLookAhead(Flags.C, InstructionSet.BCS, true); }

        [Fact]
        public void TestCarrySetLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.C, InstructionSet.BCS, true); }

        [Fact]
        public void TestCarrySetLookBehind() { TestWhenFlagClearLookBehind(Flags.C, InstructionSet.BCS, true); }

        [Fact]
        public void TestCarrySetLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.C, InstructionSet.BCS, true); }

        // Branch if Equal

        [Fact]
        public void TestZeroFlagClear() { TestWhenFlagImpeeding(Flags.Z, InstructionSet.BEQ, true); }

        [Fact]
        public void TestZeroEqualLookAhead() { TestWhenFlagClearLookAhead(Flags.Z, InstructionSet.BEQ, true); }

        [Fact]
        public void TestZeroEqualLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.Z, InstructionSet.BEQ, true); }

        [Fact]
        public void TestZeroEqualLookBehind() { TestWhenFlagClearLookBehind(Flags.Z, InstructionSet.BEQ, true); }

        [Fact]
        public void TestZeroEqualLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.Z, InstructionSet.BEQ, true); }

        // Branch if Not Equal

        [Fact]
        public void TestZeroFlagSet() { TestWhenFlagImpeeding(Flags.Z, InstructionSet.BNE, false); }

        [Fact]
        public void TestNotZeroLookAhead() { TestWhenFlagClearLookAhead(Flags.Z, InstructionSet.BNE, false); }

        [Fact]
        public void TestNotZeroLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.Z, InstructionSet.BNE, false); }

        [Fact]
        public void TestNotZeroLookBehind() { TestWhenFlagClearLookBehind(Flags.Z, InstructionSet.BNE, false); }

        [Fact]
        public void TestNotZeroLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.Z, InstructionSet.BNE, false); }

        // Branch if Plus

        [Fact]
        public void TestNegativeFlagSet() { TestWhenFlagImpeeding(Flags.N, InstructionSet.BPL, false); }

        [Fact]
        public void TestNotNegativeLookAhead() { TestWhenFlagClearLookAhead(Flags.N, InstructionSet.BPL, false); }

        [Fact]
        public void TestNotNegativeLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.N, InstructionSet.BPL, false); }

        [Fact]
        public void TestNotNegativeLookBehind() { TestWhenFlagClearLookBehind(Flags.N, InstructionSet.BPL, false); }

        [Fact]
        public void TestNotNegativeLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.N, InstructionSet.BPL, false); }

        // Branch if Minus

        [Fact]
        public void TestNegativeFlagClear() { TestWhenFlagImpeeding(Flags.N, InstructionSet.BMI, true); }

        [Fact]
        public void TestIsNegativeLookAhead() { TestWhenFlagClearLookAhead(Flags.N, InstructionSet.BMI, true); }

        [Fact]
        public void TestIsNegativeLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.N, InstructionSet.BMI, true); }

        [Fact]
        public void TestIsNegativeLookBehind() { TestWhenFlagClearLookBehind(Flags.N, InstructionSet.BMI, true); }

        [Fact]
        public void TestIsNegativeLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.N, InstructionSet.BMI, true); }

        // Branch if Overflow Clear

        [Fact]
        public void TestOverflowClearSet() { TestWhenFlagImpeeding(Flags.V, InstructionSet.BVC, false); }

        [Fact]
        public void TestOverflowClearLookAhead() { TestWhenFlagClearLookAhead(Flags.V, InstructionSet.BVC, false); }

        [Fact]
        public void TestOverflowClearLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.V, InstructionSet.BVC, false); }

        [Fact]
        public void TestOverflowClearLookBehind() { TestWhenFlagClearLookBehind(Flags.V, InstructionSet.BVC, false); }

        [Fact]
        public void TestOverflowClearLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.V, InstructionSet.BVC, false); }

        // Branch if Overflow Set

        [Fact]
        public void TestOverflowSetClear() { TestWhenFlagImpeeding(Flags.V, InstructionSet.BVS, true); }

        [Fact]
        public void TestOverflowSetLookAhead() { TestWhenFlagClearLookAhead(Flags.V, InstructionSet.BVS, true); }

        [Fact]
        public void TestOverflowSetLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.V, InstructionSet.BVS, true); }

        [Fact]
        public void TestOverflowSetLookBehind() { TestWhenFlagClearLookBehind(Flags.V, InstructionSet.BVS, true); }

        [Fact]
        public void TestOverflowSetLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.V, InstructionSet.BVS, true); }
    }
}
