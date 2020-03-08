using _6502Cpu;
using Xunit;

namespace NesInstructionSetTests
{
    public class CompareTests
    {
        [Fact]
        public void TestAGtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.A = 0x45;
            cpu.Fetched = 0x20;

            Assert.Equal(1, InstructionSet.CMP(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestAEqualFetch()
        {
            Cpu cpu = new Cpu();

            cpu.A = 0x45;
            cpu.Fetched = 0x45;

            Assert.Equal(1, InstructionSet.CMP(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.True(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestALtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.A = 0x30;
            cpu.Fetched = 0x45;

            Assert.Equal(1, InstructionSet.CMP(cpu));

            Assert.False(cpu.GetFlag(Flags.C));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestXGtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.X = 0x45;
            cpu.Fetched = 0x20;

            Assert.Equal(0, InstructionSet.CPX(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestXEqualFetch()
        {
            Cpu cpu = new Cpu();

            cpu.X = 0x45;
            cpu.Fetched = 0x45;

            Assert.Equal(0, InstructionSet.CPX(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.True(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestXLtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.X = 0x30;
            cpu.Fetched = 0x45;

            Assert.Equal(0, InstructionSet.CPX(cpu));

            Assert.False(cpu.GetFlag(Flags.C));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestYGtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.Y = 0x45;
            cpu.Fetched = 0x20;

            Assert.Equal(0, InstructionSet.CPY(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestYEqualFetch()
        {
            Cpu cpu = new Cpu();

            cpu.Y = 0x45;
            cpu.Fetched = 0x45;

            Assert.Equal(0, InstructionSet.CPY(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.True(cpu.GetFlag(Flags.Z));
        }

        [Fact]
        public void TestYLtFetch()
        {
            Cpu cpu = new Cpu();

            cpu.Y = 0x30;
            cpu.Fetched = 0x45;

            Assert.Equal(0, InstructionSet.CPY(cpu));

            Assert.False(cpu.GetFlag(Flags.C));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.Z));
        }
    }
}
