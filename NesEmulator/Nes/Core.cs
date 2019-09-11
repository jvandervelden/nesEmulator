using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestPGE.Nes.Memory;
using TestPGE.Nes.Bus;

namespace TestPGE.Nes
{
    public class Core : GameEngine
    {
        public static readonly double MASTER_CLOCK_FREQ = 21441960;
        public static readonly double CPU_CLOCK_FREQ = MASTER_CLOCK_FREQ / 16d;
        public static readonly double MS_BETWEN_CYCLES = CPU_CLOCK_FREQ / TICKS_PER_MILLISECOND;
        public static UInt16 NES_RAM_SIZE = 0x0800; // 2kb of Ram
        public static UInt16 NES_ADDRESSABLE_RANGE = 0xFFFF; // 16 bit address bus.

        public Core(int width, int height, int px, int py) : base(width, height, px, py) { }

        private Ram _ram;
        private Rom _rom;
        private List<MirroredRam> mirroredRam = new List<MirroredRam>();
        private I6502 _cpu;
        private DataBus _bus;

        protected override bool OnUserCreate()
        {
            _ram = new Ram(NES_RAM_SIZE, 0x0000);

            mirroredRam.Add(new MirroredRam(_ram, (uint)NES_RAM_SIZE * 1));
            mirroredRam.Add(new MirroredRam(_ram, (uint)NES_RAM_SIZE * 2));
            mirroredRam.Add(new MirroredRam(_ram, (uint)NES_RAM_SIZE * 3));

            // Simple bubble sort program from 
            string program = "A0 00 8C 32 00 B1 30 AA C8 CA B1 30 C8 D1 30 90 11 F0 0F 48 B1 30 88 91 30 68 C8 91 30 A9 FF 8D 32 00 CA D0 E5 2C 32 00 30 D6 60";
            string[] programHexCodes = program.Split(' ');

            // Extend by 6 bytes for the irq, nmi, reset addresses.
            byte[] programBytes = new byte[programHexCodes.Length + 6];

            for (int i = 0; i < programHexCodes.Length; i++)
                programBytes[i] = byte.Parse(programHexCodes[i], System.Globalization.NumberStyles.HexNumber);

            UInt16 startAddress = (UInt16)(0xFFFF - programBytes.Length + 1);

            programBytes[programBytes.Length - 3] = (byte)(startAddress >> 8);
            programBytes[programBytes.Length - 4] = (byte)startAddress;

            _rom = new Rom(programBytes, NES_ADDRESSABLE_RANGE);

            // Setup bytes to sort.
            _ram.WriteByte(0x0030, 0x40);
            _ram.WriteByte(0x0031, 0x00);
            _ram.WriteByte(0x0040, 0x05);
            _ram.WriteByte(0x0041, 0x45);
            _ram.WriteByte(0x0042, 0x29);
            _ram.WriteByte(0x0043, 0x35);
            _ram.WriteByte(0x0044, 0x75);
            _ram.WriteByte(0x0045, 0x30);

            _bus = new DataBus(NES_ADDRESSABLE_RANGE);

            _bus.ConnectDevice(_ram);
            foreach (MirroredRam mr in mirroredRam) _bus.ConnectDevice(mr);
            _bus.ConnectDevice(_rom);

            _cpu = new Cpu()
            {
                Bus = _bus
            };

            _cpu.Reset();

            return true;
        }

        long currentCycleTimeMs = 0;

        protected override bool OnUserUpdate(double elapsedTime)
        {
            bool print = false;

            currentCycleTimeMs += (int)Math.Round(elapsedTime * TICKS_PER_MILLISECOND);
            
            // Slow down to cpu cycles to what the NES clock is.
            if (currentCycleTimeMs > MS_BETWEN_CYCLES)
            {
                long currentTimeStamp = DateTime.Now.Ticks;

                print = _cpu.RemainingInstructionCycles == 0 || print;

                _cpu.clock();
                currentCycleTimeMs -= (int)MS_BETWEN_CYCLES;

                if (print) PrintState();
            }
            
            return true;
        }

        private void PrintState()
        {
            Console.SetCursorPosition(0, 0);
            string[] dumpLines = _ram.HexDump(0, 0x0800, 32).Split('\n', '\r');

            dumpLines[0] += "  Status Flags:";
            dumpLines[1] += "  N  V  U  B  D  I  Z  C";
            dumpLines[2] += String.Format("  {0}  {1}  {2}  {3}  {4}  {5}  {6}  {7}",
                _cpu.GetFlag(Flags.N) ? 1 : 0,
                _cpu.GetFlag(Flags.V) ? 1 : 0,
                _cpu.GetFlag(Flags.U) ? 1 : 0,
                _cpu.GetFlag(Flags.B) ? 1 : 0,
                _cpu.GetFlag(Flags.D) ? 1 : 0,
                _cpu.GetFlag(Flags.I) ? 1 : 0,
                _cpu.GetFlag(Flags.Z) ? 1 : 0,
                _cpu.GetFlag(Flags.C) ? 1 : 0);

            dumpLines[4] += String.Format("  A:       ${0:X2}", _cpu.A);
            dumpLines[5] += String.Format("  X:       ${0:X2}", _cpu.X);
            dumpLines[6] += String.Format("  Y:       ${0:X2}", _cpu.Y);
            dumpLines[7] += String.Format("  Stack P: ${0:X2}", _cpu.StackPointer);
            dumpLines[8] += String.Format("  PC:      ${0:X4}", _cpu.ProgramCounter);

            dumpLines[10] += String.Format("  Stack:", _cpu.ProgramCounter);
            for (byte i = _cpu.StackPointer, x = 0; i <= Cpu.INITIAL_STACK_POINTER_ADDRESS; i++, x++)
                dumpLines[11 + x] += string.Format("  $00{0:X2}: ${1:X2}", i, _ram.ReadByte(i));

            Console.WriteLine();
            Console.WriteLine(" Zeropage:");
            foreach (string dumpLine in dumpLines) Console.WriteLine(dumpLine);
        }
    }
}
