using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestPGE.Nes;

namespace NesInstructionSetTests
{
    [TestClass]
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

            Assert.AreEqual(0, instructionToTest(cpu));

            Assert.AreEqual(0x0040, cpu.ProgramCounter);
            Assert.AreEqual(0x0046, cpu.Address);
            Assert.AreEqual(0, cpu.RemainingInstructionCycles);
        }

        public void TestWhenFlagClearLookAhead(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = 4;
            cpu.ProgramCounter = 0x0046;

            Assert.AreEqual(0, instructionToTest(cpu));

            Assert.AreEqual(0x004A, cpu.ProgramCounter);
            Assert.AreEqual(0x004A, cpu.Address);
            Assert.AreEqual(1, cpu.RemainingInstructionCycles);
        }

        public void TestWhenFlagClearLookAheadAcrossPage(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = 4;
            cpu.ProgramCounter = 0x00FD;

            Assert.AreEqual(0, instructionToTest(cpu));

            Assert.AreEqual(0x0101, cpu.ProgramCounter);
            Assert.AreEqual(0x0101, cpu.Address);
            Assert.AreEqual(2, cpu.RemainingInstructionCycles);
        }

        public void TestWhenFlagClearLookBehind(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = -4;
            cpu.ProgramCounter = 0x0046;

            Assert.AreEqual(0, instructionToTest(cpu));

            Assert.AreEqual(0x0042, cpu.ProgramCounter);
            Assert.AreEqual(0x0042, cpu.Address);
            Assert.AreEqual(1, cpu.RemainingInstructionCycles);
        }

        public void TestWhenFlagClearLookBehindAcrossPage(Flags flag, Func<Cpu, byte> instructionToTest, bool flagBranchValue)
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(flag, flagBranchValue);
            cpu.ImpliedAddress = false;
            cpu.BranchRelativeAddress = -4;
            cpu.ProgramCounter = 0x0101;

            Assert.AreEqual(0, instructionToTest(cpu));

            Assert.AreEqual(0x00FD, cpu.ProgramCounter);
            Assert.AreEqual(0x00FD, cpu.Address);
            Assert.AreEqual(2, cpu.RemainingInstructionCycles);
        }

        // Branch if Carry Clear

        [TestMethod]
        public void TestCarryClearSet() { TestWhenFlagImpeeding(Flags.C, InstructionSet.BCC, false); }

        [TestMethod]
        public void TestCarryClearLookAhead() { TestWhenFlagClearLookAhead(Flags.C, InstructionSet.BCC, false); }

        [TestMethod]
        public void TestCarryClearLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.C, InstructionSet.BCC, false); }

        [TestMethod]
        public void TestCarryClearLookBehind() { TestWhenFlagClearLookBehind(Flags.C, InstructionSet.BCC, false); }

        [TestMethod]
        public void TestCarryClearLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.C, InstructionSet.BCC, false); }

        // Branch if Carry Set

        [TestMethod]
        public void TestCarrySetClear() { TestWhenFlagImpeeding(Flags.C, InstructionSet.BCS, true); }

        [TestMethod]
        public void TestCarrySetLookAhead() { TestWhenFlagClearLookAhead(Flags.C, InstructionSet.BCS, true); }

        [TestMethod]
        public void TestCarrySetLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.C, InstructionSet.BCS, true); }

        [TestMethod]
        public void TestCarrySetLookBehind() { TestWhenFlagClearLookBehind(Flags.C, InstructionSet.BCS, true); }

        [TestMethod]
        public void TestCarrySetLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.C, InstructionSet.BCS, true); }

        // Branch if Equal

        [TestMethod]
        public void TestZeroFlagClear() { TestWhenFlagImpeeding(Flags.Z, InstructionSet.BEQ, true); }

        [TestMethod]
        public void TestZeroEqualLookAhead() { TestWhenFlagClearLookAhead(Flags.Z, InstructionSet.BEQ, true); }

        [TestMethod]
        public void TestZeroEqualLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.Z, InstructionSet.BEQ, true); }

        [TestMethod]
        public void TestZeroEqualLookBehind() { TestWhenFlagClearLookBehind(Flags.Z, InstructionSet.BEQ, true); }

        [TestMethod]
        public void TestZeroEqualLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.Z, InstructionSet.BEQ, true); }

        // Branch if Not Equal

        [TestMethod]
        public void TestZeroFlagSet() { TestWhenFlagImpeeding(Flags.Z, InstructionSet.BNE, false); }

        [TestMethod]
        public void TestNotZeroLookAhead() { TestWhenFlagClearLookAhead(Flags.Z, InstructionSet.BNE, false); }

        [TestMethod]
        public void TestNotZeroLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.Z, InstructionSet.BNE, false); }

        [TestMethod]
        public void TestNotZeroLookBehind() { TestWhenFlagClearLookBehind(Flags.Z, InstructionSet.BNE, false); }

        [TestMethod]
        public void TestNotZeroLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.Z, InstructionSet.BNE, false); }

        // Branch if Plus

        [TestMethod]
        public void TestNegativeFlagSet() { TestWhenFlagImpeeding(Flags.N, InstructionSet.BPL, false); }

        [TestMethod]
        public void TestNotNegativeLookAhead() { TestWhenFlagClearLookAhead(Flags.N, InstructionSet.BPL, false); }

        [TestMethod]
        public void TestNotNegativeLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.N, InstructionSet.BPL, false); }

        [TestMethod]
        public void TestNotNegativeLookBehind() { TestWhenFlagClearLookBehind(Flags.N, InstructionSet.BPL, false); }

        [TestMethod]
        public void TestNotNegativeLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.N, InstructionSet.BPL, false); }

        // Branch if Minus

        [TestMethod]
        public void TestNegativeFlagClear() { TestWhenFlagImpeeding(Flags.N, InstructionSet.BMI, true); }

        [TestMethod]
        public void TestIsNegativeLookAhead() { TestWhenFlagClearLookAhead(Flags.N, InstructionSet.BMI, true); }

        [TestMethod]
        public void TestIsNegativeLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.N, InstructionSet.BMI, true); }

        [TestMethod]
        public void TestIsNegativeLookBehind() { TestWhenFlagClearLookBehind(Flags.N, InstructionSet.BMI, true); }

        [TestMethod]
        public void TestIsNegativeLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.N, InstructionSet.BMI, true); }

        // Branch if Overflow Clear

        [TestMethod]
        public void TestOverflowClearSet() { TestWhenFlagImpeeding(Flags.V, InstructionSet.BVC, false); }

        [TestMethod]
        public void TestOverflowClearLookAhead() { TestWhenFlagClearLookAhead(Flags.V, InstructionSet.BVC, false); }

        [TestMethod]
        public void TestOverflowClearLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.V, InstructionSet.BVC, false); }

        [TestMethod]
        public void TestOverflowClearLookBehind() { TestWhenFlagClearLookBehind(Flags.V, InstructionSet.BVC, false); }

        [TestMethod]
        public void TestOverflowClearLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.V, InstructionSet.BVC, false); }

        // Branch if Overflow Set

        [TestMethod]
        public void TestOverflowSetClear() { TestWhenFlagImpeeding(Flags.V, InstructionSet.BVS, true); }

        [TestMethod]
        public void TestOverflowSetLookAhead() { TestWhenFlagClearLookAhead(Flags.V, InstructionSet.BVS, true); }

        [TestMethod]
        public void TestOverflowSetLookAheadAcrossPage() { TestWhenFlagClearLookAheadAcrossPage(Flags.V, InstructionSet.BVS, true); }

        [TestMethod]
        public void TestOverflowSetLookBehind() { TestWhenFlagClearLookBehind(Flags.V, InstructionSet.BVS, true); }

        [TestMethod]
        public void TestOverflowSetLookBehindAcrossPage() { TestWhenFlagClearLookBehindAcrossPage(Flags.V, InstructionSet.BVS, true); }
    }
}
