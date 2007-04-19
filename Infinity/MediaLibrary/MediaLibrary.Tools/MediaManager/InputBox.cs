using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MediaManager
{
    public partial class InputBox : Form
    {
        private string _rtnVal;

        public InputBox()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            this._rtnVal = this.Input.Text;
            this.Close();
        }

        public string Title
        {
            set
            {
                this.Text = value;
            }
        }

        public string Description
        {
            set
            {
                this.Desc.Text = value;
            }
        }

        public string ReturnValue
        {
            get
            {
                return _rtnVal;
            }
        }

        public static string Show(string Title, string Desc)
        {
            // Create a new input box dialog
            InputBox iBox = new InputBox();
            iBox.Title = Title;
            iBox.Description = Desc;
            iBox.ShowDialog();
            return iBox.ReturnValue;
        }
    }
}