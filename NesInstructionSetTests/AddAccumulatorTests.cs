using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes;

namespace NesInstructionSetTests
{
    [TestClass]
    public class AddAccumulatorTests
    {
        [TestMethod]
        public void TestAddToAccumulatorWithoutCarry()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x04;
            cpu.Fetched = 0x81;

            Assert.AreEqual(1, InstructionSet.ADC(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.C));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.V));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.AreEqual(0x85, cpu.A);
        }

        [TestMethod]
        public void TestAddToAccumulatorWithCarry()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, true);
            cpu.A = 0x04;
            cpu.Fetched = 0x81;

            Assert.AreEqual(1, InstructionSet.ADC(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.C));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.V));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.AreEqual(0x86, cpu.A);
        }

        [TestMethod]
        public void TestAddLargeNumbers()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0xFF;
            cpu.Fetched = 0xFF;

            Assert.AreEqual(1, InstructionSet.ADC(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.V));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.AreEqual(0xFE, cpu.A);
        }

        [TestMethod]
        public void TestPosPosOverflow()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x60;
            cpu.Fetched = 0x60;

            Assert.AreEqual(1, InstructionSet.ADC(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.C));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsTrue(cpu.GetFlag(Flags.V));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.AreEqual(0xC0, cpu.A);
        }

        [TestMethod]
        public void TestNegNegOverflow()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x81;
            cpu.Fetched = 0x81;

            Assert.AreEqual(1, InstructionSet.ADC(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsTrue(cpu.GetFlag(Flags.V));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
            Assert.AreEqual(0x02, cpu.A);
        }

        [TestMethod]
        public void TestZeroFlag()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x80;
            cpu.Fetched = 0x80;

            Assert.AreEqual(1, InstructionSet.ADC(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsTrue(cpu.GetFlag(Flags.V));
            Assert.IsTrue(cpu.GetFlag(Flags.Z));
            Assert.AreEqual(0x00, cpu.A);
        }
    }
}
