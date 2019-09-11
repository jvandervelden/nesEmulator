using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestPGE
{
    public partial class Form1 : Form
    {
        public string Message { get; set; }

        public Form1()
        {
            InitializeComponent();
            
        }

        public void SetText(string message)
        {
            this.Message = message;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 100;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            this.timer1.Start();
            // 
            // Form1
            // 
            this.Name = "Form1";
            this.ResumeLayout(false);

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.Text = Message;
        }
    }
}
