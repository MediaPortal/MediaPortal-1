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
using System.Runtime.Serialization.Formatters.Soap;

namespace ProcessPlugins.ExternalDreamboxTV
{
    public partial class ExternalDreamBoxTVSettings : Form
    {
        // Fields
        private BackgroundWorker _ChannelbackgroundWorker;
        private BackgroundWorker _EPGbackgroundWorker;
        private ArrayList captureCards;

        private int sortPlace;
        private int channelId = -1;

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

            DreamBox.Core core1 = new DreamBox.Core("http://" + edtDreamIP.Text, edtDreamUserName.Text, edtDreamPassword.Text);
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
            ;
            this._ChannelbackgroundWorker.DoWork += new DoWorkEventHandler(_ChannelbackgroundWorker_DoWork);
            this._ChannelbackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_ChannelbackgroundWorker_RunWorkerCompleted);
            InitializeComponent();
        }

        void _ChannelbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.MapChannelsToCard();

            this.btnEPG.Enabled = true;
            this.btnEPG.Text = "Load EPG";
            this.btnLoadChannels.Text = "Load Channels";
            this.btnLoadChannels.Enabled = true;
            this.edtBoutique.Text = "";
            this.edtChannel.Text = "";
            this.progChannels.Value = 0;
            this.progLabel.Text = "";
            this.edtTotalBoutiques.Text = "";
            this.edtTotalChannels.Text = "";
        }

        void _ChannelbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            this.ImportChannels();

        }

        void _EPGbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            this.btnEPG.Enabled = true;
            this.btnEPG.Text = "Load EPG";
            this.btnLoadChannels.Text = "Load Channels";
            this.btnLoadChannels.Enabled = true;
            this.edtBoutique.Text = "";
            this.edtChannel.Text = "";
            this.progChannels.Value = 0;
            this.progLabel.Text = "";
            this.edtTotalBoutiques.Text = "";
            this.edtTotalChannels.Text = "";
            


        }

        void _EPGbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.ImportEPG();
        }
        private void ImportEPG()
        {
            DreamBox.Core core1 = new DreamBox.Core("http://" + edtDreamIP.Text, edtDreamUserName.Text, edtDreamPassword.Text);
            DataTable table1 = core1.Data.UserTVBouquets.Tables[0];
            this.edtTotalBoutiques.Text = table1.Rows.Count.ToString();
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                this.edtBoutique.Text = table1.Rows[num1]["Name"].ToString();
                string text1 = table1.Rows[num1]["Ref"].ToString();
                this.ImportEPGChannels(text1);
            }
        }
        private void ImportEPGChannels(string reference)
        {
            DreamBox.Core core1 = new DreamBox.Core("http://" + edtDreamIP.Text, edtDreamUserName.Text, edtDreamPassword.Text);
            DataTable table1 = core1.Data.Channels(reference).Tables[0];
            this.edtTotalChannels.Text = table1.Rows.Count.ToString();
            this.progChannels.Maximum = table1.Rows.Count;
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                this.edtChannel.Text = table1.Rows[num1]["Name"].ToString();
                this.progChannels.Value = num1;
                string text1 = table1.Rows[num1]["Ref"].ToString();
                try
                {

                    this.ImportChannelEPG(text1);
                }
                catch { }
                
            }
        }
        private void ImportChannelEPG(string reference)
        {
            DreamBox.Core core1 = new DreamBox.Core("http://" + edtDreamIP.Text, edtDreamUserName.Text, edtDreamPassword.Text);
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
                long t1 = Convert.ToInt64(time1.ToString("yyyyMMddHHmmss"));
                long t2 = Convert.ToInt64(time1.ToString("yyyyMMddHHmmss"));
                ArrayList programsInDatabase = new ArrayList();
                TVDatabase.GetProgramsPerChannel(data1.ServiceName, t1+1, t2+1, ref programsInDatabase);
                if (programsInDatabase.Count == 0)
                {
                    TVProgram program1 = new TVProgram(data1.ServiceName, time1, time2, event1.Description);
                    program1.Description = event1.Details;
                    program1.Genre = event1.Genre;
                    program1.Date = time1.ToString();
                    TVDatabase.AddProgram(program1);
                }
            }
        }

        private void ImportChannels()
        {
            DreamBox.Core core1 = new DreamBox.Core("http://" + edtDreamIP.Text, edtDreamUserName.Text, edtDreamPassword.Text);
            DataTable table1 = core1.Data.UserTVBouquets.Tables[0];
            this.edtTotalBoutiques.Text = table1.Rows.Count.ToString();
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                this.edtBoutique.Text = table1.Rows[num1]["Name"].ToString();
                TVGroup group1 = new TVGroup();
                group1.GroupName = table1.Rows[num1]["Name"].ToString();
                TVDatabase.AddGroup(group1);
            }
            Application.DoEvents();
            ArrayList list1 = new ArrayList();
            TVDatabase.GetGroups(ref list1);
            foreach (TVGroup group2 in list1)
            {
                this.AddChannels(group2);
            }
            
        }

        private void AddChannels(TVGroup group)
        {
            string text1 = this.GetBouquetRef(group.GroupName);
            progLabel.Text = "Current bouquet:  " + group.GroupName;
            this.edtBoutique.Text = group.GroupName;
            DreamBox.Core core1 = new DreamBox.Core("http://" + edtDreamIP.Text, edtDreamUserName.Text, edtDreamPassword.Text);
            DataTable table1 = core1.Data.Channels(text1).Tables[0];
            this.edtTotalChannels.Text = table1.Rows.Count.ToString();
            this.progChannels.Maximum = table1.Rows.Count;
            TVDatabase.DeleteChannelsFromGroup(group);
            group.TvChannels.Clear();
            Application.DoEvents();
            for (int num1 = 0; num1 < table1.Rows.Count; num1++)
            {
                this.edtChannel.Text = table1.Rows[num1]["Name"].ToString();
                this.progChannels.Value = num1;
                TVChannel channel1 = this.SaveChannel(table1.Rows[num1]["Name"].ToString(), table1.Rows[num1]["Ref"].ToString());
                group.TvChannels.Add(channel1);
                channel1.Sort = num1;
                TVDatabase.MapChannelToGroup(group, channel1);
                Application.DoEvents();
            }
        }

        private TVChannel SaveChannel(string name, string reference)
        {
            TelevisionChannel channel1 = new TelevisionChannel();
            channel1.Name = name;
            channel1.Scrambled = false;
            channel1.standard = AnalogVideoStandard.None;
            channel1.VisibleInGuide = true;
            channel1.Country = new TunerCountry(-1, "Default", "").Id;
            channel1.External = true;
            channel1.ExternalTunerChannel = reference;
            channel1.Channel = 0x3d091;
            TVChannel channel2 = new TVChannel();
            channel2.ID = -1;
            channel2.Name = channel1.Name;
            channel2.Number = channel1.Channel;
            channel2.Country = channel1.Country;
            channel2.External = channel1.External;
            channel2.ExternalTunerChannel = channel1.ExternalTunerChannel;
            channel2.TVStandard = channel1.standard;
            channel2.VisibleInGuide = channel1.VisibleInGuide;
            if (channel2.Number == 0)
            {
                bool flag1;
                ArrayList list1 = new ArrayList();
                TVDatabase.GetChannels(ref list1);
                channel2.Number = list1.Count;
                do
                {
                    flag1 = true;
                    foreach (TVChannel channel3 in list1)
                    {
                        if (channel3.Number == channel2.Number)
                        {
                            flag1 = false;
                            channel2.Number++;
                            break;
                        }
                    }
                }
                while (!flag1);
            }
            if (channel2.ID < 0)
            {
                this.channelId = TVDatabase.AddChannel(channel2);
                return channel2;
            }
            TVDatabase.UpdateChannel(channel2, this.SortingPlace);
            return channel2;
        }


        private void MapChannelsToCard()
        {
            ArrayList list1 = new ArrayList();
            TVDatabase.GetChannels(ref list1);
            foreach (TVCaptureDevice device1 in this.captureCards)
            {
                foreach (TVChannel channel1 in list1)
                {
                    TVDatabase.MapChannelToCard(channel1.ID, channel1.ID);
                }
            }
        }




        private void btnEPG_Click(object sender, EventArgs e)
        {
            this.btnEPG.Text = "Wait...";
            this.btnEPG.Enabled = false;
            this.btnLoadChannels.Enabled = false;
            if (edtDreamIP.Text.Length > 0)
            {
                try
                {
                    TVDatabase.RemovePrograms();
                }
                catch (Exception exception1)
                {
                    MessageBox.Show(exception1.Message);
                }
                this._EPGbackgroundWorker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Fill in correct location of your box");
            }

        }

        #region Channels
        public void LoadCaptureCards()
        {
            if (File.Exists(Config.GetFile(Config.Dir.Config, "capturecards.xml")))
            {
                using (FileStream stream1 = new FileStream(Config.GetFile(Config.Dir.Config, "capturecards.xml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    try
                    {
                        SoapFormatter formatter1 = new SoapFormatter();
                        this.captureCards = new ArrayList();
                        this.captureCards = (ArrayList)formatter1.Deserialize(stream1);
                        for (int num1 = 0; num1 < this.captureCards.Count; num1++)
                        {
                            ((TVCaptureDevice)this.captureCards[num1]).ID = num1 + 1;
                        }
                        stream1.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load previously configured capture card(s), you will need to re-configure your device(s).", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
        }


        #endregion

        private void btnLoadChannels_Click(object sender, EventArgs e)
        {
            this.btnLoadChannels.Text = "Wait...";
            this.btnEPG.Enabled = false;
            this.btnLoadChannels.Enabled = false;
            this.DeleteChannels();
            this._ChannelbackgroundWorker.RunWorkerAsync();

        }

        private void DeleteChannels()
        {
            progLabel.Text = "Deleting channels";
            Application.DoEvents();
            ArrayList list1 = new ArrayList();
            TVDatabase.GetChannels(ref list1);
            foreach (TVChannel channel1 in list1)
            {
                edtChannel.Text = channel1.Name;
                TVDatabase.RemoveChannel(channel1.Name);
            }
            ArrayList list2 = new ArrayList();
            TVDatabase.GetGroups(ref list2);

            edtChannel.Text = "";
            progLabel.Text = "Deleting boutiques";
            foreach (TVGroup group1 in list2)
            {
                edtBoutique.Text = group1.GroupName;
                TVDatabase.DeleteGroup(group1);
            }
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            // Save Dreambox Settings
            SaveSettings();
        }

        private void LoadSettings()
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                string ip = xmlreader.GetValueAsString("Dreambox", "IP", "dreambox");
                string userName = xmlreader.GetValueAsString("Dreambox", "UserName", "root");
                string password = xmlreader.GetValueAsString("Dreambox", "Password", "dreambox");
                string syncText = xmlreader.GetValueAsString("Dreambox", "SyncHour", "0");

                edtDreamIP.Text = ip;
                edtDreamUserName.Text = userName;
                edtDreamPassword.Text = password;
                edtSyncHours.Text = syncText;
            }
        }

        private bool SaveSettings()
        {
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                xmlwriter.SetValue("Dreambox", "IP", edtDreamIP.Text);
                xmlwriter.SetValue("Dreambox", "UserName", edtDreamUserName.Text);
                xmlwriter.SetValue("Dreambox", "Password", edtDreamPassword.Text);
            }
            return true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExternalDreamBoxTVSettings_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void edtSaveEPGSyncSettings_Click(object sender, EventArgs e)
        {
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                xmlwriter.SetValue("Dreambox", "SyncHour", edtSyncHours.Text);
            }
            MessageBox.Show("Saved");
        }


    }

    public class TelevisionChannel
    {
        public int ID;
        public string Name = String.Empty;
        public int Channel = 0;
        public Frequency Frequency = new Frequency(0);
        public bool External = false;
        public string ExternalTunerChannel = String.Empty;
        public bool VisibleInGuide = true;
        public AnalogVideoStandard standard = AnalogVideoStandard.None;
        public int Country;
        public bool Scrambled = false;
    }

    public class Frequency
    {
        public enum Format
        {
            Hertz,
            MegaHertz
        }

        public Frequency(long hertz)
        {
            this.hertz = hertz;
        }

        public Frequency(double megahertz)
        {
            this.hertz = (long)(megahertz * (1000000d));
        }

        private long hertz = 0;

        public long Hertz
        {
            get { return hertz; }
            set
            {
                hertz = value;
                if (hertz <= 1000)
                    hertz *= (int)1000000d;
            }
        }

        public double MegaHertz
        {
            get { return (double)hertz / 1000000d; }
            set
            {
                hertz = (long)(value * 1000000d);
            }
        }

        public static implicit operator Frequency(int hertz)
        {
            return new Frequency(hertz);
        }

        public static implicit operator Frequency(long hertz)
        {
            return new Frequency(hertz);
        }

        public static implicit operator Frequency(double megaHertz)
        {
            return new Frequency((long)(megaHertz * (1000000d)));
        }

        public string ToString(Format format)
        {
            string result = String.Empty;

            try
            {
                switch (format)
                {
                    case Format.Hertz:
                        result = String.Format("{0}", Hertz);
                        break;

                    case Format.MegaHertz:
                        result = String.Format("{0:#,###0.000}", MegaHertz);
                        break;
                }
            }
            catch
            {
                //
                // Failed to convert
                //
            }

            return result;
        }

        public override string ToString()
        {
            return ToString(Format.MegaHertz);
        }
    }


}