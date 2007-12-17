using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaLibrary;
using System.Threading;

namespace MediaManager
{
    public partial class ImportProgress : Form
    {
        private bool m_status;
        private IMLImport m_Import;
        private IMLImportProgress m_Progress;
        public delegate void RunImportDelegate();
        public delegate bool ShowProgressDelegate(int per, string text);
        public delegate void OkBtnDelegate(String text);

        public ImportProgress()
        {
            InitializeComponent();
            m_status = true;
        }

        public string Title
        {
            set { this.Text = value; }
        }
        
        private bool Status
        {
            get { return m_status; }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (this.button1.Text == "Cancel")
            {
                m_status = false;
            }
            else
                this.Close();
        }

        public void Run(IMLImport Import, IMLImportProgress Progress)
        {
            this.m_Import = Import;
            this.m_Progress = Progress;
            Title = "Running Import: " + Import.Name;
            OkBtn("Cancel");
            this.ShowDialog();
        }

        private void ImportProgress_Load(object sender, EventArgs e)
        {
            RunImportDelegate runImport = new RunImportDelegate(RunImport);
            runImport.BeginInvoke(null, null);
        }

        private void RunImport()
        {
            string err;
            if (!m_Import.Run(m_Progress, out err))
                MessageBox.Show(err, "Import Error", MessageBoxButtons.OK);
                //this.Close();
            OkBtn("OK");
        }

        public bool ShowProgress(int per, string text)
        {
            // Make sure we're on the right thread
            if (progressBar1.InvokeRequired == false)
            {
                label1.Text = text;
                progressBar1.Value = per;
            }
            else
            {
                // Show progress asynchronously
                ShowProgressDelegate showProgress = new ShowProgressDelegate(ShowProgress);
                this.Invoke(showProgress, new object[] { per, text });
            }
            return Status;
        }

        public void OkBtn(String text)
        {
            if (button1.InvokeRequired == false)
                button1.Text = text;
            else
            {
                OkBtnDelegate okBtn = new OkBtnDelegate(OkBtn);
                this.Invoke(okBtn, new object[] { text });
            }
        }
    }
}