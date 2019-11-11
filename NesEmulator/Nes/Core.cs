using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestPGE.Nes.Memory;
using TestPGE.Nes.Bus;
using System.Threading;
using Microsoft.Xna.Framework;

namespace TestPGE.Nes
{
    public class Core : GameEngine
    {
        public static readonly double TARGET_CLOCK_FREQ = 1789772;
        public static readonly double TICKS_BETWEN_CYCLES = TARGET_CLOCK_FREQ / TimeSpan.TicksPerMillisecond;
        public static UInt16 NES_RAM_SIZE = 0x0800; // 2kb of Ram
        public static UInt16 CPU_ADDRESSABLE_RANGE = 0xFFFF; // 16 bit cpu address bus.
        public static UInt16 PPU_ADDRESSABLE_RANGE = 0x3FFF; // 14 bit ppu address bus.

        public static readonly long NTSC_FRAME_CYCLES = 29780;
        public static readonly long NTSC_V_BLANK_CYCLES = 2273;

        public Core(int width, int height, int px, int py) : base(width, height, px, py) { }

        private Ram _ram;
        private NameTableRam _vram;
        private PaletteRam _paletteRam;
        private List<MirroredRam> mirroredRam = new List<MirroredRam>();
        private I6502 _cpu;
        private I2C02 _ppu;
        private DataBus _cpuBus;
        private DataBus _ppuBus;
        private PpuControlBus _ppuControlBus;
        private I2A03 _cpuExtended;
        private IOamDma _oamDma;
        private Controller _controller;

        private Cartridge _cartridge;

        protected override bool OnUserCreate()
        {
            // SETUP THE RAM
            _ram = new Ram(NES_RAM_SIZE);

            // SETUP THE BUSSES
            _cpuBus = new DataBus(CPU_ADDRESSABLE_RANGE);
            _ppuBus = new DataBus(PPU_ADDRESSABLE_RANGE);

            _cpuBus.ConnectDevice(_ram, 0x0000, 0x1FFF);
            
            // SETUP THE CPU
            _cpu = new Cpu(_cpuBus);
            
            // SETUP THE PPU
            _ppu = new Ppu(_ppuBus, _cpu);
            _oamDma = new OamDirectMemoryAccess(_ppu, _ram);
            _ppuControlBus = new PpuControlBus(_ppu);

            _cpuExtended = new CpuExtended(_oamDma, _cpu);

            _cpuBus.ConnectDevice(_cpuExtended, 0x4000, 0x4015);
            // 4016 is connected to controller device
            _cpuBus.ConnectDevice(_cpuExtended, 0x4017, 0x401F);
            _cpuBus.ConnectDevice(_ppuControlBus, 0x2000, 0x3FFF);

            // SETUP THE CARTRIDGE
            _cartridge = new Cartridge();
            //_cartridge.Load(@"D:\tmp\full_palette.nes");
            //_cartridge.Load(@"D:\tmp\nestest.nes");
            _cartridge.Load(@"D:\tmp\Legend of Zelda, The (USA).nes");
            //_cartridge.Load(@"D:\tmp\Super Mario Bros. (World).nes");
            //_cartridge.Load(@"D:\tmp\Clu Clu Land (World).nes");
            //_cartridge.Load(@"D:\tmp\DuckTales (USA).nes");
            //_cartridge.Load(@"D:\tmp\Chip n Dale - Rescue Rangers (USA).nes");
            //_cartridge.Load(@"D:\tmp\Ninja Gaiden (USA).nes");


            _cartridge.Insert(_cpuBus, _ppuBus);

            if (_cartridge.Mapper.NameTableSize < 0x2000)
            {
                _vram = new NameTableRam(_cartridge);
                _ppuBus.ConnectDevice(_vram, 0x2000, 0x3EFF);
                
                _paletteRam = new PaletteRam();
                _ppuBus.ConnectDevice(_paletteRam, 0x3F00, 0x3FFF);
            }

            // Setup Input
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.Up, false);
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.Down, false);
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.Left, false);
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.Right, false);
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.Z, false);
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.X, false);
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.RightShift, false);
            interestedKeys.Add(Microsoft.Xna.Framework.Input.Keys.Enter, false);

            _controller = new Controller();

            _cpuBus.ConnectDevice(_controller, 0x4016, 0x4016);

            // RESET TO INITIAL STATE
            _cpu.Reset();

            Thread printThread = new Thread(this.tStart);

            printThread.Start();

            CreateSubDisplay(32, 1, 16, 16, "Palette Table");
            CreateSubDisplay(32, 16, 3, 3, "Pattern Table");
            CreateSubDisplay(64, 60, 8, 8, "Name Table");
            
            return true;
        }

        long _currentCycleTimeTicks = 0;

        long _masterFps = 0;
        int frames = 0;

        protected void UpdateControllerState()
        {
            _controller.SetButton(Controller.ControllerButtons.UP, interestedKeys[Microsoft.Xna.Framework.Input.Keys.Up]);
            _controller.SetButton(Controller.ControllerButtons.DOWN, interestedKeys[Microsoft.Xna.Framework.Input.Keys.Down]);
            _controller.SetButton(Controller.ControllerButtons.LEFT, interestedKeys[Microsoft.Xna.Framework.Input.Keys.Left]);
            _controller.SetButton(Controller.ControllerButtons.RIGHT, interestedKeys[Microsoft.Xna.Framework.Input.Keys.Right]);
            _controller.SetButton(Controller.ControllerButtons.B, interestedKeys[Microsoft.Xna.Framework.Input.Keys.Z]);
            _controller.SetButton(Controller.ControllerButtons.A, interestedKeys[Microsoft.Xna.Framework.Input.Keys.X]);
            _controller.SetButton(Controller.ControllerButtons.SELECT, interestedKeys[Microsoft.Xna.Framework.Input.Keys.RightShift]);
            _controller.SetButton(Controller.ControllerButtons.START, interestedKeys[Microsoft.Xna.Framework.Input.Keys.Enter]);
        }

        protected override bool OnUserUpdate(long elapsedTicks)
        {
            UpdateControllerState();
                        
            int cpuWaitCycles = 3;

            do
            {
                if (--cpuWaitCycles == 0)
                {
                    _cpu.Clock();
                    if (_cpu.RemainingInstructionCycles == 0)
                    {
                    }
                    cpuWaitCycles = 3;
                }

                _ppu.Clock();
            } while (_ppu.RemainingDotsInFrame > 0);

            if (_ppu.BackgroundRenderEnabled)
                Draw(0, 0, _ppu.RenderBackground());

            _currentCycleTimeTicks += elapsedTicks;

            if (_currentCycleTimeTicks > TimeSpan.TicksPerSecond)
            {
                _masterFps = frames;
                _currentCycleTimeTicks = 0;
                frames = 0;
            }

            frames++;

            return true;
        }

        private void DrawPalettes()
        {
            SimpleTexture bgTex = new SimpleTexture(1, 1);
            Color bgColor = BaseColors.Palette[_ppuBus.Read(0x3F00) & 0x3F];

            bgTex.Data[0] = bgColor;

            Draw(0, 0, bgTex, "Palette Table");

            for (int i = 0; i < 8; i++)
            {
                SimpleTexture palTex = new SimpleTexture(3, 1);

                palTex.Data[0] = BaseColors.Palette[_ppuBus.Read((UInt16)(0x3F00 + i * 4 + 1)) & 0x3F];
                palTex.Data[1] = BaseColors.Palette[_ppuBus.Read((UInt16)(0x3F00 + i * 4 + 2)) & 0x3F];
                palTex.Data[2] = BaseColors.Palette[_ppuBus.Read((UInt16)(0x3F00 + i * 4 + 3)) & 0x3F];

                Draw(i * 6 + 3, 0, palTex, "Palette Table");
            }
        }

        private void tStart(object data)
        {
            while (IsRunning)
            {
                try
                {
                    //DrawPalettes();
                    //PrintState();
                    //PrintPPUData();
                    //PrintPatternTables();
                    PrintNameTables();
                }
                catch (NoGraphicsException)
                {
                    // Display was closed.
                }
            }
        }

        private void PrintPatternTables()
        {
            for (byte p = 0; p <= 0x01; p++)
            {
                for (byte x = 0; x <= 0x0F; x++)
                {
                    for (byte y = 0; y <= 0x0F; y++)
                    {
                        Draw(x + p * 16, y, _ppu.PrintPattern(p, (byte)((x << 4) + y)), "Pattern Table");
                    }
                }
            }
        }

        private void PrintNameTables()
        {
            SimpleTexture nameTable = new SimpleTexture(64 * 8, 60 * 8);
            int idx = 0;

            for (byte px = 0; px <= 0x01; px++)
            {
                for (byte py = 0; py <= 0x01; py++)
                { 
                    for (byte x = 0; x < 32; x++)
                    {
                        for (byte y = 0; y < 30; y++)
                        {
                            SimpleTexture tileTexture = _ppu.PrintTile((byte)(px * 2 + py), x, y);
                            nameTable.Append(idx, tileTexture);
                            idx += tileTexture.Data.Length;
                        }
                    }
                }
            }

            Draw(0, 0, nameTable, "Name Table");
        }

        private void PrintPPUData()
        {
            Console.SetCursorPosition(0, 0);

            StringBuilder line = new StringBuilder();
            
            /*
            Console.WriteLine("\n---------- Pattern Data -------------------------------------------");

            line.Append("0000: ");

            for (UInt16 i = 0x0000; i < 0x2000; i++)
            {
                byte data = _ppuBus.Read(i);

                line.AppendFormat("0x{0:X2}", data);
                line.Append(" ");

                if (i % 32 == 31)
                {
                    Console.WriteLine(line.ToString());

                    line.Clear().AppendFormat("{0:x4}", i + 1).Append(": ");
                }
                else if (i % 8 == 7)
                    line.Append("- ");
            }
            */
            Console.WriteLine("\n---------- Nametable Data -------------------------------------------");

            line.Clear().Append("2000: ");
            
            for (UInt16 i = 0x2000; i < 0x23C0; i++)
            {
                byte data = _ppuBus.Read(i);

                line.AppendFormat("0x{0:X2}", data);
                line.Append(" ");

                if (i % 32 == 31)
                {
                    Console.WriteLine(line.ToString());
                    
                    line.Clear().AppendFormat("{0:x4}", i + 1).Append(": ");
                }
                else if (i % 8 == 7)
                    line.Append("- ");
            }

            line.Clear();
            Console.WriteLine("\n---------- Attribute Data -------------------------------------------");

            line.Append("23C0: ");

            for (UInt16 i = 0x23C0; i < 0x2400; i++)
            {
                byte data = _ppuBus.Read(i);

                line.AppendFormat("0x{0:X2}", data);
                line.Append("                ");

                if (i % 8 == 7)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(line.ToString());

                    line.Clear().AppendFormat("{0:x4}", i + 1).Append(": ");
                }
                else if (i % 2 == 1)
                    line.Append("- ");
            }

            Console.WriteLine("\n---------- Palette Data -------------------------------------------");

            Console.Write("Palettes: ");

            for (UInt16 i = 0x3F00; i < 0x3F20; i++)
            {
                if (i % 4 == 0)
                    Console.Write("{0:d2}: ", (i - 0x3F00) / 4);

                byte palByte = _ppuBus.Read(i);

                Console.Write("0x{0:x2} ", palByte);

                if (i % 4 == 3)
                    Console.Write(" | ");
            }
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

            dumpLines[12] += String.Format("  Master Freq: {0}", _masterFps);
            dumpLines[13] += String.Format("  Dots:        {0}", _ppu.RemainingDotsInFrame);

            Console.WriteLine();
            Console.WriteLine(" Zeropage:");

            foreach (string dumpLine in dumpLines) Console.WriteLine(dumpLine);
        }

        private List<string> Disassemble(int length)
        {
            UInt16 tmpPC = (UInt16)(_cpu.ProgramCounter - 1);
            List<string> disassembly = new List<string>();

            for (int i = 0; i < length; i++)
            {
                byte opCode = _cpuBus.Read(tmpPC++);
                Instruction instruction = InstructionSet.InstuctionsByOpcode[opCode];

                disassembly.Add(instruction.Name);

                if (instruction.AddressMode == AddressModes.ABS) tmpPC += 2;
                if (instruction.AddressMode == AddressModes.ABX) tmpPC += 2;
                if (instruction.AddressMode == AddressModes.ABY) tmpPC += 2;
                if (instruction.AddressMode == AddressModes.IMM) tmpPC += 1;
                if (instruction.AddressMode == AddressModes.IMP) tmpPC += 0;
                if (instruction.AddressMode == AddressModes.IND) tmpPC += 2;
                if (instruction.AddressMode == AddressModes.IZX) tmpPC += 1;
                if (instruction.AddressMode == AddressModes.IZY) tmpPC += 1;
                if (instruction.AddressMode == AddressModes.REL) tmpPC += 1;
                if (instruction.AddressMode == AddressModes.ZP0) tmpPC += 1;
                if (instruction.AddressMode == AddressModes.ZPX) tmpPC += 1;
                if (instruction.AddressMode == AddressModes.ZPY) tmpPC += 1;
            }

            return disassembly;
        }
    }
}
