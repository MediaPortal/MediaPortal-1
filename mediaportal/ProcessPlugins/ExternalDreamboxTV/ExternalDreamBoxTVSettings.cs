using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using System.Collections;
using DShowNET;
using DreamBox;
using System.IO;

namespace ProcessPlugins.ExternalDreamboxTV
{
    public partial class ExternalDreamBoxTVSettings : Form
    {
        // Fields
        private BackgroundWorker _ChannelbackgroundWorker;
        private BackgroundWorker _EPGbackgroundWorker;
        private ArrayList captureCards;
        private int sortPlace;
        //private int channelId = -1;

        public int SortingPlace
        {
            get
            {
                return this.sortPlace;
            }
            set
            {
                this.sortPlace = value;
            }
        }

        private string GetBouquetRef(string name)
        {
            string text1 = "";

            DreamBox.Core core1 = new DreamBox.Core("http://gary.nu:82", "root", "dreambox");
            DataTable table1 = core1.Data.UserTVBouquets.Tables[0];
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                if (table1.Rows[num1]["Name"].ToString().ToLower() == name.ToLower())
                {
                    return table1.Rows[num1]["Ref"].ToString();
                }
            }
            return text1;
        }




        public ExternalDreamBoxTVSettings()
        {

            this._EPGbackgroundWorker = new BackgroundWorker();
            this._ChannelbackgroundWorker = new BackgroundWorker();
            this.captureCards = new ArrayList();
            this.sortPlace = 0;
            //this.channelId = -1;
            this._EPGbackgroundWorker.DoWork += new DoWorkEventHandler(_EPGbackgroundWorker_DoWork);
            this._EPGbackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_EPGbackgroundWorker_RunWorkerCompleted);
            InitializeComponent();
        }

        void _EPGbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            //this.textBox1.Enabled = true;
            //this.button1.Enabled = true;
            //this.button2.Enabled = true;
            this.btnEPG.Enabled = true;
            this.btnEPG.Text = "Load EPG";
            this.edtBoutique.Text = "";
            this.edtChannel.Text = "";
            this.progBoutique.Value = 0;
            this.progChannels.Value = 0;

        }

        void _EPGbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.ImportEPG();
        }
        private void ImportEPG()
        {
            DreamBox.Core core1 = new DreamBox.Core("http://gary.nu:82", "root", "dreambox");
            DataTable table1 = core1.Data.UserTVBouquets.Tables[0];
            this.edtTotalBoutiques.Text = table1.Rows.Count.ToString();
            this.progBoutique.Maximum = table1.Rows.Count;
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                this.edtBoutique.Text = table1.Rows[num1]["Name"].ToString();
                this.progBoutique.Value = num1;
                string text1 = table1.Rows[num1]["Ref"].ToString();
                this.ImportEPGChannels(text1);
            }
        }
        private void ImportEPGChannels(string reference)
        {
            DreamBox.Core core1 = new DreamBox.Core("http://gary.nu:82", "root", "dreambox");
            DataTable table1 = core1.Data.Channels(reference).Tables[0];
            this.edtTotalChannels.Text = table1.Rows.Count.ToString();
            this.progChannels.Maximum = table1.Rows.Count;
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                this.edtChannel.Text = table1.Rows[num1]["Name"].ToString();
                this.progChannels.Value = num1;
                string text1 = table1.Rows[num1]["Ref"].ToString();
                this.ImportChannelEPG(text1);
            }
        }
        private void ImportChannelEPG(string reference)
        {
            DreamBox.Core core1 = new DreamBox.Core("http://gary.nu:82", "root", "dreambox");
            ServiceEpgData data1 = core1.XML.EPG(reference);
            foreach (EpgEvent event1 in data1.Events)
            {
                int num1 = Convert.ToInt32(event1.Date.Split(new char[] { '.' })[2].ToString());
                int num2 = Convert.ToInt32(event1.Date.Split(new char[] { '.' })[1].ToString());
                int num3 = Convert.ToInt32(event1.Date.Split(new char[] { '.' })[0].ToString());
                int num4 = Convert.ToInt32(event1.Time.Split(new char[] { ':' })[0].ToString());
                int num5 = Convert.ToInt32(event1.Time.Split(new char[] { ':' })[1].ToString());
                DateTime time1 = new DateTime(num1, num2, num3, num4, num5, 0);
                double num6 = Convert.ToDouble(event1.Duration);
                DateTime time2 = time1.AddSeconds(num6);
                TVProgram program1 = new TVProgram(data1.ServiceName, time1, time2, event1.Description);
                program1.Description = event1.Details;
                program1.Genre=event1.Genre ;
                program1.Date= time1.ToString();
                TVDatabase.AddProgram(program1);
            }
        }





        private void btnEPG_Click(object sender, EventArgs e)
        {
            //this.btnImport.Enabled = false;
            //this.button1.Enabled = false;
            //this.button2.Enabled = false;
            //this.textBox1.Enabled = false;
            this.btnEPG.Text = "Wait...";
            if (true)
            {
                try
                {
                    TVDatabase.RemovePrograms();
                    this._EPGbackgroundWorker.RunWorkerAsync();
                }
                catch (Exception exception1)
                {
                    MessageBox.Show(exception1.Message);
                }
            }
            else
            {
                MessageBox.Show("Fill in correct location of your box");
            }

        }
    }
}