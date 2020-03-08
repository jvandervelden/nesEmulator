using _6502Cpu;
using Xunit;

namespace NesInstructionSetTests
{
    public class AddAccumulatorTests
    {
        [Fact]
        public void TestAddToAccumulatorWithoutCarry()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x04;
            cpu.Fetched = 0x81;

            Assert.Equal(1, InstructionSet.ADC(cpu));

            Assert.False(cpu.GetFlag(Flags.C));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.V));
            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.Equal(0x85, cpu.A);
        }

        [Fact]
        public void TestAddToAccumulatorWithCarry()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, true);
            cpu.A = 0x04;
            cpu.Fetched = 0x81;

            Assert.Equal(1, InstructionSet.ADC(cpu));

            Assert.False(cpu.GetFlag(Flags.C));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.V));
            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.Equal(0x86, cpu.A);
        }

        [Fact]
        public void TestAddLargeNumbers()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0xFF;
            cpu.Fetched = 0xFF;

            Assert.Equal(1, InstructionSet.ADC(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.V));
            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.Equal(0xFE, cpu.A);
        }

        [Fact]
        public void TestPosPosOverflow()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x60;
            cpu.Fetched = 0x60;

            Assert.Equal(1, InstructionSet.ADC(cpu));

            Assert.False(cpu.GetFlag(Flags.C));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.True(cpu.GetFlag(Flags.V));
            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.Equal(0xC0, cpu.A);
        }

        [Fact]
        public void TestNegNegOverflow()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x81;
            cpu.Fetched = 0x81;

            Assert.Equal(1, InstructionSet.ADC(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.True(cpu.GetFlag(Flags.V));
            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.Equal(0x02, cpu.A);
        }

        [Fact]
        public void TestZeroFlag()
        {
            Cpu cpu = new Cpu();

            cpu.SetFlag(Flags.C, false);
            cpu.A = 0x80;
            cpu.Fetched = 0x80;

            Assert.Equal(1, InstructionSet.ADC(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.True(cpu.GetFlag(Flags.V));
            Assert.True(cpu.GetFlag(Flags.Z));
            Assert.Equal(0x00, cpu.A);
        }
    }
}
