using _6502Cpu;
using Common.Bus;
using Common.Memory;
using Moq;
using Xunit;

namespace NesInstructionSetTests
{
    public class InstructionSetTests
    {
        //DIV

        [Fact]
        public void TestClearOverflowStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.V, false)).Verifiable("Overflow flag not cleared");

            Assert.Equal(0, InstructionSet.CLV(cpu.Object));

            cpu.Verify();
        }

        [Fact]
        public void TestClearInteruptStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.I, false)).Verifiable("Interupt flag not cleared");

            Assert.Equal(0, InstructionSet.CLI(cpu.Object));

            cpu.Verify();
        }

        [Fact]
        public void TestClearDecimalStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.D, false)).Verifiable("Decimal flag not cleared");

            Assert.Equal(0, InstructionSet.CLD(cpu.Object));

            cpu.Verify();
        }

        [Fact]
        public void TestClearCarryStatus()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.SetFlag(Flags.C, false)).Verifiable("Carry flag not cleared");

            Assert.Equal(0, InstructionSet.CLC(cpu.Object));

            cpu.Verify();
        }

        [Fact]
        public void TestBrkToCallIrq()
        {
            Mock<I6502> cpu = new Mock<I6502>();

            cpu.Setup(m => m.irq(true)).Verifiable("IRQ not called.");

            Assert.Equal(0, InstructionSet.BRK(cpu.Object));

            cpu.Verify();
        }

        [Fact]
        public void TestBitToZero()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x01;
            cpu.A = 0x02;

            Assert.Equal(0, InstructionSet.BIT(cpu));

            Assert.True(cpu.GetFlag(Flags.Z));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.V));

            Assert.Equal(0x02, cpu.A);
        }

        [Fact]
        public void TestBitToNegative()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x81;
            cpu.A = 0x82;

            Assert.Equal(0, InstructionSet.BIT(cpu));

            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.True(cpu.GetFlag(Flags.N));
            Assert.False(cpu.GetFlag(Flags.V));

            Assert.Equal(0x82, cpu.A);
        }

        [Fact]
        public void TestBitToFlagBit6ToOverflow()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x61;
            cpu.A = 0x62;

            Assert.Equal(0, InstructionSet.BIT(cpu));

            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.False(cpu.GetFlag(Flags.N));
            Assert.True(cpu.GetFlag(Flags.V));

            Assert.Equal(0x62, cpu.A);
        }

        [Fact]
        public void TestAndToZero()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x01;
            cpu.A = 0x02;

            Assert.Equal(1, InstructionSet.AND(cpu));

            Assert.True(cpu.GetFlag(Flags.Z));
            Assert.False(cpu.GetFlag(Flags.N));

            Assert.Equal(0x00, cpu.A);
        }

        [Fact]
        public void TestAndToNegative()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x82;
            cpu.A = 0x81;

            Assert.Equal(1, InstructionSet.AND(cpu));

            Assert.False(cpu.GetFlag(Flags.Z));
            Assert.True(cpu.GetFlag(Flags.N));

            Assert.Equal(0x80, cpu.A);
        }

        [Fact]
        public void TestShiftLeftToAccumulator()
        {
            Cpu cpu = new Cpu();

            cpu.ImpliedAddress = true;
            cpu.Fetched = 0x81;

            Assert.Equal(0, InstructionSet.ASL(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.Equal(0x02, cpu.A);
        }

        [Fact]
        public void TestShiftLeftToRam()
        {
            Ram ram = new Ram(0xFF);
            DataBus bus = new DataBus(0xFF);
            Cpu cpu = new Cpu(bus);

            cpu.Address = 0x0004;
            cpu.ImpliedAddress = false;
            cpu.Fetched = 0x81;

            Assert.Equal(0, InstructionSet.ASL(cpu));

            Assert.True(cpu.GetFlag(Flags.C));
            Assert.Equal(0x02, ram.ReadByte(0x0004));
        }
    }
}
