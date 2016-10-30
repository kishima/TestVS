using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestVS.Util
{
    class Log
    {
        private readonly TextBox Target;
        public Log(System.Windows.Forms.TextBox t)
        {
            this.Target = t;
        }

        public void Put(string s)
        {
            if (Target == null)
            {
                return;
            }
            Target.Text += s + "\r\n";
        }
    }
}
