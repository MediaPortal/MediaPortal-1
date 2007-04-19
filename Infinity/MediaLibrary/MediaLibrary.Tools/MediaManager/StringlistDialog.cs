using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaManager
{
    public partial class StringlistDialog : Form
    {
        string[] _Stringlist;

        public StringlistDialog()
        {
            InitializeComponent();
        }

        public string[] Stringlist
        {
            get
            {
                return this.textBox1.Lines;
            }
            set
            {
                _Stringlist = value;
                this.textBox1.Lines = value;
            }
        }

        private void CanclButton_Click(object sender, EventArgs e)
        {
            this.textBox1.Lines = _Stringlist;
        }
    }
}