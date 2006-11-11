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

                MessageBox.Show(hour.ToString());
                mpNumericUpDown1.Value = Convert.ToDecimal(hour);
                mpNumericUpDown2.Value = Convert.ToDecimal(minute);
            }
        }

        private bool SaveSettings()
        {
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                xmlwriter.SetValue("DreamboxDirectEPG", "Hour", mpNumericUpDown1.Value.ToString());
                xmlwriter.SetValue("DreamboxDirectEPG", "Minute", mpNumericUpDown2.Value.ToString());
            }
            return true;
        }

        private void DreamboxDirectEPGSettings_Load(object sender, EventArgs e)
        {

        }


    }
}