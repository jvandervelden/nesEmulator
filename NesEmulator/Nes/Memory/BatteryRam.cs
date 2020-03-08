using Common.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPGE.Nes.Memory
{
    public class BatteryRam : Ram, IDisposable
    {
        private string romSaveFilePath;

        public BatteryRam(uint size, string romFilePath) : base(size) {
            romSaveFilePath = romFilePath + ".sav";

            try
            {
                using (FileStream fs = File.Open(romSaveFilePath, FileMode.Open))
                {
                    if (fs.Read(_memory, 0, _memory.Length) != _memory.Length)
                        Console.Error.WriteLine("Save file wasn't the right size.");
                }
            }
            catch (IOException e)
            {
                if (!(e is FileNotFoundException))
                {
                    Console.Error.WriteLine($"Unable to load save file '{romSaveFilePath}': {e.Message}");
                }
            }
        }

        public void Dispose()
        {
            try
            {
                using (FileStream fs = File.Open(romSaveFilePath, FileMode.Create))
                    fs.Write(_memory, 0, _memory.Length);
            }
            catch (IOException e)
            {
                Console.Error.WriteLine($"Unable to save file '{romSaveFilePath}': {e.Message}");
            }
        }
       
        public override void WriteByte(uint address, byte data)
        {
            base.WriteByte(address, data);
        }
    }
}
