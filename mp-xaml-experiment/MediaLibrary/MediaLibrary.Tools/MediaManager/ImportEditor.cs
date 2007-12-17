using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaLibrary;
using MediaLibrary.Settings;
using MediaLibrary.Configuration;

namespace MediaManager
{
    public partial class ImportEditor : Form
    {
        private IMLImport Import;
        private IMLHashItemList PluginList;
        private string frequency;
        private int interval;
        private DateTime time;
        private MLPluginPropertyControlCollection PluginControls;

        
        public ImportEditor()
        {
            InitializeComponent();
        }

        public ImportEditor(IMLImport Import, IMediaLibrary Library)
        {
            InitializeComponent();

            this.Import = Import;
            iMLImportBindingSource.DataSource = this.Import;
            PluginControls = new MLPluginPropertyControlCollection("import", this.panel1, Library);
            PluginList = Library.SystemObject.GetInstalledPlugins("import");

            ArrayList modes = new ArrayList();
            modes.Add(new AddValue("Update existing items and add new items","update"));
            modes.Add(new AddValue("Delete all items and re-import","reimport"));
            this.ImportModeText.DataSource = modes;
            this.ImportModeText.DisplayMember = "Display";
            this.ImportModeText.ValueMember = "Value";

            
            ScheduleTime = Import.ScheduleTime;
            ScheduleInterval = Import.ScheduleInterval;
            ScheduleFrequency = Import.ScheduleFrequency;
            

            ArrayList plugins = new ArrayList();
            if (PluginList != null && PluginList.Count > 0)
            {
                for (int i = 0; i < PluginList.Count; i++)
                {
                    IMLHashItem plugin = PluginList[i];
                    MLPluginDescription desc = plugin["description"] as MLPluginDescription;
                    plugins.Add(new AddValue(desc.information.plugin_name, desc.information.plugin_id.ToString()));
                }
                this.PluginChoicesComboBox.DataSource = plugins;
                this.PluginChoicesComboBox.DisplayMember = "Display";
                this.PluginChoicesComboBox.ValueMember = "Value";
            }
        }

        private string ScheduleFrequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
                switch (frequency)
                {
                    case "startup":
                        ScheduleAtStartRadio_CheckedChanged(null, null);
                        break;
                    case "days":
                        ScheduleDailyRadio_CheckedChanged(null, null);
                        break;
                    case "hours":
                    case "minutes":
                        ScheduleIntervalRadio_CheckedChanged(null, null);
                        break;
                    case "never":
                    default:
                        frequency = "never";
                        ScheduleNeverRadio_CheckedChanged(null, null);
                        break;
                }
            }
        }

        private int ScheduleInterval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
                if(interval < 1 || interval > 60)
                    this.ScheduleIntervalUpDown.Value = 1;
                else
                    this.ScheduleIntervalUpDown.Value = interval;
            }
        }

        private DateTime ScheduleTime
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
                this.ScheduleIntervalDateTimePicker.Value = time;
            }
        }

        private void ScheduleNeverRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ScheduleNeverRadio.Checked == true)
            {
                //ScheduleNeverRadio.Checked = true;
                ScheduleDailyRadio.Checked = false;
                ScheduleIntervalDateTimePicker.Enabled = false;
                ScheduleAtStartRadio.Checked = false;
                ScheduleIntervalRadio.Checked = false;
                ScheduleIntervalUpDown.Enabled = false;
                ScheduleIntervalMinOrHourCombo.Enabled = false;

                frequency = "never";
                interval = 0;
                time = DateTime.ParseExact("00:00", "HH:mm", null);

                Import.ScheduleTime = ScheduleTime;
                Import.ScheduleInterval = ScheduleInterval;
                Import.ScheduleFrequency = ScheduleFrequency;
            }
        }

        private void ScheduleDailyRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ScheduleDailyRadio.Checked == true)
            {
                ScheduleNeverRadio.Checked = false;
                //ScheduleDailyRadio.Checked = true;
                ScheduleIntervalDateTimePicker.Enabled = true;
                ScheduleAtStartRadio.Checked = false;
                ScheduleIntervalRadio.Checked = false;
                ScheduleIntervalUpDown.Enabled = false;
                ScheduleIntervalMinOrHourCombo.Enabled = false;

                frequency = "days";
                interval = 0;
                time = ScheduleIntervalDateTimePicker.Value;

                Import.ScheduleTime = ScheduleTime;
                Import.ScheduleInterval = ScheduleInterval;
                Import.ScheduleFrequency = ScheduleFrequency;
            }
        }

        private void ScheduleAtStartRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ScheduleAtStartRadio.Checked == true)
            {
                ScheduleNeverRadio.Checked = false;
                ScheduleDailyRadio.Checked = false;
                ScheduleIntervalDateTimePicker.Enabled = false;
                //ScheduleAtStartRadio.Checked = true;
                ScheduleIntervalRadio.Checked = false;
                ScheduleIntervalUpDown.Enabled = false;
                ScheduleIntervalMinOrHourCombo.Enabled = false;

                frequency = "startup";
                interval = 0;
                time = DateTime.ParseExact("00:00", "HH:mm", null);

                Import.ScheduleTime = ScheduleTime;
                Import.ScheduleInterval = ScheduleInterval;
                Import.ScheduleFrequency = ScheduleFrequency;
            }
        }

        private void ScheduleIntervalRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ScheduleIntervalRadio.Checked == true)
            {
                ScheduleNeverRadio.Checked = false;
                ScheduleDailyRadio.Checked = false;
                ScheduleIntervalDateTimePicker.Enabled = false;
                ScheduleAtStartRadio.Checked = false;
                //ScheduleIntervalRadio.Checked = true;
                ScheduleIntervalUpDown.Enabled = true;
                ScheduleIntervalMinOrHourCombo.Enabled = true;

                frequency = ScheduleIntervalMinOrHourCombo.SelectedText;
                interval = Convert.ToInt32(ScheduleIntervalUpDown.Value);
                time = DateTime.ParseExact("00:00", "HH:mm", null);

                Import.ScheduleTime = ScheduleTime;
                Import.ScheduleInterval = ScheduleInterval;
                Import.ScheduleFrequency = ScheduleFrequency;
            }
        }

        private void ScheduleIntervalDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            time = ScheduleIntervalDateTimePicker.Value;
            Import.ScheduleTime = ScheduleTime;
        }

        private void ScheduleIntervalUpDown_ValueChanged(object sender, EventArgs e)
        {
            interval = Convert.ToInt32(ScheduleIntervalUpDown.Value);
            Import.ScheduleInterval = ScheduleInterval;
        }

        private void ScheduleIntervalMinOrHourCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            frequency = ScheduleIntervalMinOrHourCombo.SelectedText;
            Import.ScheduleFrequency = ScheduleFrequency;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Import.PluginProperties.Clear();
            PluginControls.SaveProperties();
            Import.Save();
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RunNow_Click(object sender, EventArgs e)
        {
            Import.PluginProperties.Clear();
            PluginControls.SaveProperties();
            ImportProgress ProgressForm = new ImportProgress();
            ProgressForm.Run(Import, new ImportProgressClass(ProgressForm) as IMLImportProgress);
        }

        private void ImportEditor_Load(object sender, EventArgs e)
        {
        }

        private void PluginChoicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PluginChoicesComboBox.SelectedIndex >= 0)
            {
                OK.Enabled = true;
                RunNow.Enabled = true;

                if (PluginList.Count > 0 && PluginChoicesComboBox.Items.Count > 0)
                {
                    PluginControls.LoadPlugin(new Guid(((AddValue)PluginChoicesComboBox.SelectedItem).Value));
                    MLPluginProperties PropCollection = new MLPluginProperties();
                    PluginControls.Plugin.GetProperties(PropCollection);
                    PluginControls.LoadProperties(PropCollection, Import.PluginProperties);
                }
            }
            else
            {
                OK.Enabled = false;
                RunNow.Enabled = false;
                PluginControls.Clear();

            }


        }
    }

    public class AddValue
    {
        private string m_Display;
        private string m_Value;
        public AddValue(string Display, string Value)
        {
            m_Display = Display;
            m_Value = Value;
        }
        public string Display
        {
            get { return m_Display; }
        }
        public string Value
        {
            get { return m_Value; }
        }
    }
}