using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestPGE.Nes;
using TestPGE.Nes.Bus;
using TestPGE.Nes.Memory;

namespace NesInstructionSetTests
{
    [TestClass]
    public class InstructionSetTests
    {
        //DIV

        [TestMethod]
        public void TestClearOverflowStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.V, false)).Verifiable("Overflow flag not cleared");

            Assert.AreEqual(0, InstructionSet.CLV(cpu.Object));

            cpu.Verify();
        }

        [TestMethod]
        public void TestClearInteruptStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.I, false)).Verifiable("Interupt flag not cleared");

            Assert.AreEqual(0, InstructionSet.CLI(cpu.Object));

            cpu.Verify();
        }

        [TestMethod]
        public void TestClearDecimalStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.D, false)).Verifiable("Decimal flag not cleared");

            Assert.AreEqual(0, InstructionSet.CLD(cpu.Object));

            cpu.Verify();
        }

        [TestMethod]
        public void TestClearCarryStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.C, false)).Verifiable("Carry flag not cleared");

            Assert.AreEqual(0, InstructionSet.CLC(cpu.Object));

            cpu.Verify();
        }

        [TestMethod]
        public void TestBrkToCallIrq()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.irq(true)).Verifiable("IRQ not called.");

            Assert.AreEqual(0, InstructionSet.BRK(cpu.Object));

            cpu.Verify();
        }

        [TestMethod]
        public void TestBitToZero()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x01;
            cpu.A = 0x02;

            Assert.AreEqual(0, InstructionSet.BIT(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.Z));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.V));

            Assert.AreEqual(0x02, cpu.A);
        }

        [TestMethod]
        public void TestBitToNegative()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x81;
            cpu.A = 0x82;

            Assert.AreEqual(0, InstructionSet.BIT(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.V));

            Assert.AreEqual(0x82, cpu.A);
        }

        [TestMethod]
        public void TestBitToFlagBit6ToOverflow()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x61;
            cpu.A = 0x62;

            Assert.AreEqual(0, InstructionSet.BIT(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsTrue(cpu.GetFlag(Flags.V));

            Assert.AreEqual(0x62, cpu.A);
        }

        [TestMethod]
        public void TestAndToZero()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x01;
            cpu.A = 0x02;

            Assert.AreEqual(1, InstructionSet.AND(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.Z));
            Assert.IsFalse(cpu.GetFlag(Flags.N));

            Assert.AreEqual(0x00, cpu.A);
        }

        [TestMethod]
        public void TestAndToNegative()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x82;
            cpu.A = 0x81;

            Assert.AreEqual(1, InstructionSet.AND(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.IsTrue(cpu.GetFlag(Flags.N));

            Assert.AreEqual(0x80, cpu.A);
        }

        [TestMethod]
        public void TestShiftLeftToAccumulator()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = true;
            cpu.Fetched = 0x81;

            Assert.AreEqual(0, InstructionSet.ASL(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.AreEqual(0x02, cpu.A);
        }

        [TestMethod]
        public void TestShiftLeftToRam()
        {
            Ram ram = new Ram(0xFF);
            DataBus bus = new DataBus(0xFF);
            Cpu cpu = new Cpu() { Bus = bus };

            cpu.Address = 0x0004;
            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x81;

            Assert.AreEqual(0, InstructionSet.ASL(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.AreEqual(0x02, ram.ReadByte(0x0004));
        }
    }
}
