using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MPTvClient
{
    public partial class frmServerConnectionConfig : Form
    {
        public frmServerConnectionConfig()
        {
            InitializeComponent();
        }
        public void InitForm(string hostname)
        {
            edHostname.Text = hostname;
        }
        public void GetConfig(ref string hostname)
        {
            hostname = edHostname.Text;
        }
    }
}