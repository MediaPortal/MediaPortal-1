using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MPx86Proxy.Logging
{
    public partial class SearchForm : Form
    {
        private const int WM_KEYDOWN = 0x100;

        public int StartLocation
        {
            get
            {
                return this._StartLocation;
            }
            set
            {
                if (value < 0) this._StartLocation = 0;
                else if (value > this._TextBox.Text.Length) this._StartLocation = this._TextBox.Text.Length;
                else this._StartLocation = value;
            }
        }private int _StartLocation = 0;

        public string Query
        {
            get
            {
                return this.textBox1.Text;
            }
        }

        private TextBox _TextBox;

        public SearchForm(string strTitle, string strQuery, TextBox tBox)
        {
            if (tBox == null) throw new ArgumentNullException("Textbox is null.");

            InitializeComponent();

            if (!string.IsNullOrEmpty(strTitle)) this.Text = strTitle;
            if (!string.IsNullOrEmpty(strQuery)) this.textBox1.Text = strQuery;

            this._TextBox = tBox;

            this.DialogResult = System.Windows.Forms.DialogResult.None;
        }

        //Catch up "Keydown" message
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            if (msg.Msg == WM_KEYDOWN)
            {
                Keys keycode = (Keys)((int)keyData & 0xff);
                switch (keycode)
                {
                    case Keys.Enter:
                        this.Search();
                        return true;

                    case Keys.Escape:
                        this.Close();
                        return true;

                    default:
                        break;
                }
            }
            return false;

        }

        private void button_Find_Click(object sender, EventArgs e)
        {
            this.Search();
        }

        private void Search()
        {
            if (string.IsNullOrEmpty(this.textBox1.Text))
            {
                MessageBox.Show("Empty query.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int iIdx;
            if (this.checkBox_Back.Checked)
            {
                iIdx = this._TextBox.Text.LastIndexOf(this.textBox1.Text, 
                    this._StartLocation, this.checkBox_Case.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                iIdx = this._TextBox.Text.IndexOf(this.textBox1.Text, this._StartLocation,
                    this.checkBox_Case.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
            }

            if (iIdx >= 0)
            {

                if (this.checkBox_Back.Checked) this._StartLocation = iIdx;
                else this._StartLocation = iIdx + this.textBox1.Text.Length;

                if (this._StartLocation > this._TextBox.Text.Length) this._StartLocation = this._TextBox.Text.Length;


                this._TextBox.SelectionStart = iIdx;
                this._TextBox.SelectionLength = this.textBox1.Text.Length;
                this._TextBox.ScrollToCaret();
            }

            else
            {
                MessageBox.Show("Not found.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (this.checkBox_Back.Checked) this._StartLocation = this._TextBox.Text.Length;
                else this._StartLocation = 0;
                this._TextBox.SelectionStart = 0;
                this._TextBox.SelectionLength = 0;
            }
        }

        private void checkBox_Back_CheckedChanged(object sender, EventArgs e)
        {
            if (this._TextBox.SelectionLength > 0)
            {
                if (this.checkBox_Back.Checked) this._StartLocation -= this.textBox1.Text.Length;
                else this._StartLocation += this.textBox1.Text.Length;

                if (this._StartLocation > this._TextBox.Text.Length) this._StartLocation = this._TextBox.Text.Length;
                else if (this._StartLocation < 0) this._StartLocation = 0;
            }
            else
            {
                if (this.checkBox_Back.Checked) this._StartLocation = this._TextBox.Text.Length;
                else this._StartLocation = 0;
            }
        }

        private void FindQueryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
