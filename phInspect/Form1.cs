using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Parahalf;
using System.Runtime.InteropServices;

namespace phInspect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var handle = User32.FindWindowByCaption("foobar2000 v1.1.11");

            var newParent = panel1.Handle;
            _(String.Format("Handle: {0}\nParent: {1}", handle, newParent));
            User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, panel1.Width, panel1.Height, 0);
            var lStyle = User32.GetWindowLong(handle, User32.GWL_STYLE);
            lStyle = lStyle & (~User32.WS_CAPTION);
            User32.SetWindowLong(handle, User32.GWL_STYLE, lStyle);
            IntPtr r;
            if ((r = User32.SetParent(handle, newParent)) == IntPtr.Zero)
            {
                _(String.Format("Got error: {0}", Marshal.GetLastWin32Error()));
            }
            else
            {
                _(String.Format("Oldparent: {0}", r));
            }
        }

        void _(string s)
        {
            textBox1.AppendText(s + "\n");
        }
    }
}
