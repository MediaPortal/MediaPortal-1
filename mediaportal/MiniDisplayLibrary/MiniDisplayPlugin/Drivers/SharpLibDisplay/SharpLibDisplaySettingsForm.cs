using System;
using System.Diagnostics;


using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    public partial class SharpLibDisplaySettingsForm : MPConfigForm
    {
        public SharpLibDisplaySettingsForm()
        {
          InitializeComponent();

          //SharpLibDisplay settings
          checkBoxSingleLine.DataBindings.Add("Checked", SharpLibDisplay.Settings.Instance, "SingleLine");
          textBoxSingleLineSeparator.DataBindings.Add("Text", SharpLibDisplay.Settings.Instance, "SingleLineSeparator");
          comboBoxSingleLineOptions.DataBindings.Add("SelectedIndex", SharpLibDisplay.Settings.Instance, "SingleLineMode");
          //Enable/disable single line options as user ticks/unticks single line check box
          groupBoxSingleLineOptions.DataBindings.Add("Enabled", checkBoxSingleLine, "Checked");
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {          
          SharpLibDisplay.Settings.Save();
          Close();
        }

        private void linkLabelDocumentation_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
          //Show online documentation
          ProcessStartInfo sInfo = new ProcessStartInfo("http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/141_Configuration/MediaPortal_Configuration/95_Plugins/MiniDisplay/SharpLibDisplay");
          Process.Start(sInfo);
        }

        private void comboBoxSingleLineOptions_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}