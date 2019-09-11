using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestPGE.Nes;

namespace NesInstructionSetTests
{
    [TestClass]
    public class CompareTests
    {
        [TestMethod]
        public void TestAGtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.A = 0x45;
            cpu.Fetched = 0x20;

            Assert.AreEqual(1, InstructionSet.CMP(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestAEqualFetch()
        {
            Cpu cpu = new Cpu();

            cpu.A = 0x45;
            cpu.Fetched = 0x45;

            Assert.AreEqual(1, InstructionSet.CMP(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsTrue(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestALtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.A = 0x30;
            cpu.Fetched = 0x45;

            Assert.AreEqual(1, InstructionSet.CMP(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.C));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestXGtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.X = 0x45;
            cpu.Fetched = 0x20;

            Assert.AreEqual(0, InstructionSet.CPX(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestXEqualFetch()
        {
            Cpu cpu = new Cpu();

            cpu.X = 0x45;
            cpu.Fetched = 0x45;

            Assert.AreEqual(0, InstructionSet.CPX(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsTrue(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestXLtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.X = 0x30;
            cpu.Fetched = 0x45;

            Assert.AreEqual(0, InstructionSet.CPX(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.C));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestYGtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.Y = 0x45;
            cpu.Fetched = 0x20;

            Assert.AreEqual(0, InstructionSet.CPY(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestYEqualFetch()
        {
            Cpu cpu = new Cpu();

            cpu.Y = 0x45;
            cpu.Fetched = 0x45;

            Assert.AreEqual(0, InstructionSet.CPY(cpu));

            Assert.IsTrue(cpu.GetFlag(Flags.C));
            Assert.IsFalse(cpu.GetFlag(Flags.N));
            Assert.IsTrue(cpu.GetFlag(Flags.Z));
        }

        [TestMethod]
        public void TestYLtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.Y = 0x30;
            cpu.Fetched = 0x45;

            Assert.AreEqual(0, InstructionSet.CPY(cpu));

            Assert.IsFalse(cpu.GetFlag(Flags.C));
            Assert.IsTrue(cpu.GetFlag(Flags.N));
            Assert.IsFalse(cpu.GetFlag(Flags.Z));
        }
    }
}
