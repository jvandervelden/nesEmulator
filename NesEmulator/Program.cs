using CorePixelEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestPGE.Nes;

namespace emulatorPGE
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Core core = new Core();

            if (core.Construct(256 + 128, 256, 2, 2, false, false) != RCode.FAIL)
            {
                core.Start();
            }
        }
    }
}
