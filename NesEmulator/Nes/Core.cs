using System;
using System.Collections.Generic;
using System.Text;
using TestPGE.Nes.Memory;
using TestPGE.Nes.Bus;
using System.Threading;
using Common.Memory;
using _6502Cpu;
using Common.Bus;
using CorePixelEngine;

namespace TestPGE.Nes
{
    public class Core : PixelGameEngine, IDisposable
    {
        public static readonly double TARGET_CLOCK_FREQ = 21477272;
        public static readonly double TICKS_BETWEN_CYCLES = TARGET_CLOCK_FREQ / TimeSpan.TicksPerSecond;
        public static UInt16 NES_RAM_SIZE = 0x0800; // 2kb of Ram
        public static UInt16 CPU_ADDRESSABLE_RANGE = 0xFFFF; // 16 bit cpu address bus.
        public static UInt16 PPU_ADDRESSABLE_RANGE = 0x3FFF; // 14 bit ppu address bus.

        public static readonly long NTSC_FRAME_CYCLES = 29780;
        public static readonly long NTSC_V_BLANK_CYCLES = 2273;

        protected override string sAppName => "Nes Emulator";

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

        private bool _drawPalettes = false;
        private bool _drawNameTables = false;
        private bool _drawPatternTables = false;

        private Cartridge _cartridge;

        private Thread _ppuThread;
        private Thread _cpuThread;

        private bool _ppuTick = false;
        private bool _cpuTick = false;

        public override bool OnUserCreate()
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
            //_cartridge.Load(@"D:\tmp\Legend of Zelda, The (USA).nes");
            _cartridge.Load(@"E:\games\roms\Super Mario Bros (E).nes");
            //_cartridge.Load(@"D:\tmp\Super Mario Bros. 3 (USA).nes");
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

            _controller = new Controller();

            _cpuBus.ConnectDevice(_controller, 0x4016, 0x4016);

            // RESET TO INITIAL STATE
            _cpu.Reset();

            //Thread printThread = new Thread(this.tStart);

            //printThread.Start();

            //_ppuThread = new Thread(this.ppuStart);
            //_cpuThread = new Thread(this.cpuStart);

            //_ppuThread.Start(_ppu);
            //_cpuThread.Start(_cpu);

            //Thread main = new Thread(this.mainStart);
            //main.Start();

            return true;
        }

        public void Dispose()
        {
            _cartridge.Dispose();
        }

        float _currentCycleTime = 0;

        protected void UpdateControllerState()
        {
            _controller.SetButton(Controller.ControllerButtons.UP, Input.GetKey(Key.UP).bHeld);
            _controller.SetButton(Controller.ControllerButtons.DOWN, Input.GetKey(Key.DOWN).bHeld);
            _controller.SetButton(Controller.ControllerButtons.LEFT, Input.GetKey(Key.LEFT).bHeld);
            _controller.SetButton(Controller.ControllerButtons.RIGHT, Input.GetKey(Key.RIGHT).bHeld);
            _controller.SetButton(Controller.ControllerButtons.B, Input.GetKey(Key.Z).bHeld);
            _controller.SetButton(Controller.ControllerButtons.A, Input.GetKey(Key.X).bHeld);
            _controller.SetButton(Controller.ControllerButtons.SELECT, Input.GetKey(Key.SHIFT).bHeld);
            _controller.SetButton(Controller.ControllerButtons.START, Input.GetKey(Key.ENTER).bHeld);
        }

        public override bool OnUserUpdate(float elapsedTime)
        {
            int frameTicks = 0;
            _currentCycleTime += elapsedTime;

            UpdateControllerState();

            do
            {
                int ticks = _cpu.Clock();

                for (int i = 0; i < ticks * 3; i++)
                {
                    _ppu.Clock();
                }

                frameTicks += ticks;
            } while (!_ppu.FrameReady);



            //if (interestedKeys[Microsoft.Xna.Framework.Input.Keys.P] == KeyState.PRESSED)
            //    TogglePaletteWindow();
            //if (interestedKeys[Microsoft.Xna.Framework.Input.Keys.T] == KeyState.PRESSED)
            //    TogglePatternWindow();
            //if (interestedKeys[Microsoft.Xna.Framework.Input.Keys.N] == KeyState.PRESSED)
            //    ToggleNameTableWindow();

            DrawSprite(0, 0, _ppu.RenderBackground(), 1, (byte)Sprite.Flip.NONE);

            PrintPatternTables();
            DrawPalettes();

            return true;
        }

        private void TogglePaletteWindow()
        {
            //if (!_drawPatternTables)
            //    CreateSubDisplay(32, 16, 3, 3, "Pattern Table");
            //else
            //    CloseSubDisplay("Pattern Table");

            _drawPatternTables = !_drawPatternTables;
        }

        private void TogglePatternWindow()
        {
            //if (!_drawPalettes)
            //    CreateSubDisplay(32, 1, 16, 16, "Palette Table");
            //else
            //    CloseSubDisplay("Palette Table");

            _drawPalettes = !_drawPalettes;
        }

        private void ToggleNameTableWindow()
        {
            //if (!_drawNameTables)
            //    CreateSubDisplay(64 * 8, 60 * 8, 1, 1, "Name Table");
            //else
            //    CloseSubDisplay("Name Table");

            _drawNameTables = !_drawNameTables;
        }

        private void DrawPalettes()
        {
            Sprite bgTex = new Sprite(1, 1);
            Pixel bgColor = BasePixels.Palette[_ppuBus.Read(0x3F00) & 0x3F];

            bgTex.SetPixel(0, 0, bgColor);

            DrawSprite(0, 250, bgTex, 4, (byte)Sprite.Flip.NONE);

            for (int i = 0; i < 8; i++)
            {
                Sprite palTex = new Sprite(3, 1);

                palTex.SetPixel(0, 0, BasePixels.Palette[_ppuBus.Read((UInt16)(0x3F00 + i * 4 + 1)) & 0x3F]);
                palTex.SetPixel(1, 0, BasePixels.Palette[_ppuBus.Read((UInt16)(0x3F00 + i * 4 + 2)) & 0x3F]);
                palTex.SetPixel(2, 0, BasePixels.Palette[_ppuBus.Read((UInt16)(0x3F00 + i * 4 + 3)) & 0x3F]);

                DrawSprite(4 + i * 12, 250, palTex, 4, (byte)Sprite.Flip.NONE);
            }
        }

        private void tStart(object data)
        {
            //while (IsRunning)
            //{
            //    try
            //    {
            //        DrawPalettes();
            //        //PrintState();
            //        //PrintPPUData();
            //        PrintPPUInfo();
            //        PrintPatternTables();
            //        PrintNameTables();
            //    }
            //    catch (NoGraphicsException)
            //    {
            //        // Display was closed.
            //    }
            //}
        }

        private void PrintPatternTables()
        {
            for (byte p = 0; p <= 0x01; p++)
            {
                for (byte x = 0; x <= 0x0F; x++)
                {
                    for (byte y = 0; y <= 0x0F; y++)
                    {
                        DrawSprite(256 + x * 8, y * 8 + p * 128, _ppu.PrintPattern(p, (byte)((x << 4) + y)), 1, (byte)Sprite.Flip.NONE);
                    }
                }
            }
        }

        private void PrintNameTables()
        {
            if (_drawNameTables)
            {
                Sprite nameTable = new Sprite(64 * 8, 60 * 8);

                for (byte py = 0; py <= 0x01; py++)
                {
                    for (byte px = 0; px <= 0x01; px++)
                    {
                        for (byte y = 0; y < 30; y++)
                        {
                            for (byte x = 0; x < 32; x++)
                            {
                                Sprite tileTexture = _ppu.PrintTile((byte)(py * 2 + px), x, y);
                                //nameTable.Combine(x * 8 + 32 * px, y * 8 + 30 * py, tileTexture);
                                
                                //Draw(x * 8 + 32 * px * 8, y * 8 + 30 * py * 8, tileTexture, "Name Table");
                            }
                        }
                    }
                }

                //Draw(0, 0, nameTable, "Name Table");
            }
        }

        private void PrintPPUInfo()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("v: {0:x4}", _ppu.PpuAddress);
            Console.WriteLine("t: {0:x4}", _ppu.TempRegister);
            Console.WriteLine("x: {0:x2}", _ppu.ScrollXPixel);
            Console.WriteLine("Y: {0:x2}", _ppu.ScrollYPixel);
            //Console.WriteLine(": {0:x2}", _ppu.Registers);
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
            Console.WriteLine("---------- Attribute Data -------------------------------------------");

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

            Console.WriteLine("---------- Palette Data -------------------------------------------");

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

            //dumpLines[12] += String.Format("  Master Freq: {0}", _masterFps);
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
