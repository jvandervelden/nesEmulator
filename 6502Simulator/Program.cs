using _6502Cpu;
using Common.Bus;
using Common.Memory;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace _6502Simulator
{
    class Program
    {
        private enum StepMode
        {
            RUN,
            STEP
        }

        private static StepMode stepMode;
        private static bool doStep = false;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            byte[] program = File.ReadAllBytes(args[0]);

            Run(program);
        }

        static void Run(byte[] program)
        {
            Rom programMemory = new Rom(program);
            DataBus mainBus = new DataBus(0xFFFF);
            mainBus.ConnectDevice(programMemory, (uint)(0xFFFF - program.Length), 0xFFFF);
            Cpu cpu = new Cpu(mainBus);

            cpu.Reset();
            stepMode = StepMode.RUN;
            
            ProcessInput();

            while (true)
            {
                bool isDoStep = doStep;

                if (stepMode == StepMode.RUN || doStep)
                    cpu.Clock();

                //if (cpu.RemainingInstructionCycles == 0)
                //{
                    if (stepMode == StepMode.RUN || doStep)
                    {
                        Console.WriteLine("A: ${0:X2} | X: ${1:X2} | Y: ${2:X2}", cpu.A, cpu.X, cpu.Y);
                        Console.WriteLine("PC: ${0:X4}", cpu.ProgramCounter);

                        Console.WriteLine("Next: {0}", InstructionSet.InstuctionsByOpcode[programMemory.ReadByte(cpu.ProgramCounter)].Name);
                    }

                    if (stepMode == StepMode.STEP && isDoStep == doStep)
                        doStep = false;
                //}
            }
        }

        static async void ProcessInput()
        {
            await Task.Run(() => {
                while(true)
                {
                    ConsoleKeyInfo keyPressed = Console.ReadKey(false);
                    if (keyPressed.KeyChar == 's' && stepMode == StepMode.STEP)
                        doStep = true;
                    if (keyPressed.KeyChar == 'p')
                        stepMode = StepMode.STEP;
                    if (keyPressed.KeyChar == 'r')
                        stepMode = StepMode.RUN;
                }
            });
        }

        static void PrintUsage()
        {
            Console.WriteLine("dotnet 6502Simulator.dll <bin file>");
        }
    }
}
