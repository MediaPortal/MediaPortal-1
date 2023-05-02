using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MPx86Proxy.Logging
{
    public partial class LogViewForm : Form
    {
        private const int WM_KEYDOWN = 0x100;

        private string _File;

        private SearchForm _SearchForm = null;

        private LogViewForm() { }
        public LogViewForm(string file)
        {
            InitializeComponent();

            this.textBox.HideSelection = false;

            this._File = file;
        }

        //Catch up "Keydown" message
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            if (msg.Msg == WM_KEYDOWN)
            {
                Keys keycode = (Keys)((int)keyData & 0xff);
                switch (keycode)
                {
                    case Keys.Escape:
                        this.Close();
                        return true;

                    case Keys.F2:
                        this.reload();
                        return true;

                    case Keys.F:
                        if ((keyData & Keys.Control) == Keys.Control)
                        {
                            this.search();
                            return true;
                        }
                        break;

                    default:
                        break;
                }
            }
            return false;

        }


        private void search()
        {
            if (this._SearchForm != null && this._SearchForm.DialogResult == System.Windows.Forms.DialogResult.None)
            {
                this._SearchForm.Focus();
            }
            else
            {
                this._SearchForm = new SearchForm("Find", null, this.textBox);
                this._SearchForm.Show();
            }
        }

        private void reload()
        {
            try
            {
                if (File.Exists(_File))
                {
                    using (Stream stream = File.Open(_File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader sr = new StreamReader(stream, Encoding.Default))
                        {
                            this.textBox.Text = sr.ReadToEnd();
                        }

                        this.textBox.SelectionStart = this.textBox.Text.Length;
                        this.textBox.ScrollToCaret();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while openning log file.\r\n\r\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void logViewForm_Shown(object sender, EventArgs e)
        {
            this.reload();
        }

        private void logViewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._SearchForm != null)
                this._SearchForm.Close();
        }


        private void textBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && this._SearchForm != null)
                this._SearchForm.StartLocation = this.textBox.SelectionStart;
            
        }


        private void toolStripButton_File_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog()
            {
                Filter = "log (*.log)|*log",
                FilterIndex = 1,
                Multiselect = false,
                InitialDirectory = Application.StartupPath
            };

            if (of.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this._File = of.FileName;
                this.reload();
            }
        }

        private void toolStripButton_Reload_Click(object sender, EventArgs e)
        {
            this.reload();
        }

        private void toolStripButton_Search_Click(object sender, EventArgs e)
        {
            this.search();
        }
    }
}
