using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestVS.Util;

namespace TestVS
{
    public partial class Form1 : Form
    {
        private Log LogOutput;
        public Form1()
        {
            InitializeComponent();
            LogOutput = new Log(textBox1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LogOutput.Put("button1 is pressed");
        }
    }
}
