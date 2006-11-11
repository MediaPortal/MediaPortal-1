using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Util;

namespace ProcessPlugins.DreamboxDirectEPG
{
    public partial class DreamboxDirectEPGSettings : Form
    {
        public DreamboxDirectEPGSettings()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void mpButton2_Click(object sender, EventArgs e)
        {
            // save it
            SaveSettings();
            this.Close();
        }

        private void mpButton1_Click(object sender, EventArgs e)
        {
            // close
            this.Close();
        }

        private void LoadSettings()
        {
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                int hour = xmlreader.GetValueAsInt("DreamboxDirectEPG", "Hour", 0);
                int minute = xmlreader.GetValueAsInt("DreamboxDirectEPG", "Minute", 0);

                mpNumericUpDown1.Value = Convert.ToDecimal(hour);
                mpNumericUpDown2.Value = Convert.ToDecimal(minute);

                string ip = xmlreader.GetValueAsString("DreamboxDirectEPG", "IP", "dreambox");
                string userName = xmlreader.GetValueAsString("DreamboxDirectEPG", "UserName", "root");
                string password = xmlreader.GetValueAsString("DreamboxDirectEPG", "Password", "dreambox");

                edtDreamboxIP.Text = ip;
                edtUserName.Text = userName;
                edtPassword.Text = password;
            }
        }

        private bool SaveSettings()
        {
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                xmlwriter.SetValue("DreamboxDirectEPG", "Hour", mpNumericUpDown1.Value.ToString());
                xmlwriter.SetValue("DreamboxDirectEPG", "Minute", mpNumericUpDown2.Value.ToString());

                xmlwriter.SetValue("DreamboxDirectEPG", "IP", edtDreamboxIP.Text);
                xmlwriter.SetValue("DreamboxDirectEPG", "UserName", edtUserName.Text);
                xmlwriter.SetValue("DreamboxDirectEPG", "Password", edtPassword.Text);
            }
            return true;
        }

        private void DreamboxDirectEPGSettings_Load(object sender, EventArgs e)
        {

        }


    }
}