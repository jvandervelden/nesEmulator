using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestPGE.Nes;

namespace TestPGE
{
    static class Program
    {
        private static Core td;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            td = new Core(256, 244, 3, 3);
            td.Run();
            
            //Application.Run();
        }
    }
}
