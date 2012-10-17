using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace Parahalf
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://github.com/piksel/parahalf");
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            lVersion.Text = String.Format("version {0}.{1} beta release, build {2} rev{3}", v.Major, v.Minor, v.Build, v.Revision);
        }
    }
}
