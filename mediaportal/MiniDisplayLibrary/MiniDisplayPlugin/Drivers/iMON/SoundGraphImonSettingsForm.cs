using System;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    public partial class SoundGraphImonSettingsForm : MPConfigForm
    {
        public SoundGraphImonSettingsForm()
        {
            InitializeComponent();

            //Generic iMON settings
            checkDisableWhenInBackground.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "DisableWhenInBackground");
            checkDisableWhenIdle.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "DisableWhenIdle");
            textDisableWhenIdleDelayInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "DisableWhenIdleDelayInSeconds");
            checkReenableWhenIdleAfter.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "ReenableWhenIdleAfter");
            textReenableWhenIdleAfterDelayInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "ReenableWhenIdleAfterDelayInSeconds");
            checkDisableWhenPlaying.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "DisableWhenPlaying");
            textDisableWhenPlayingDelayInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "DisableWhenPlayingDelayInSeconds");
            checkReenableWhenPlayingAfter.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "ReenableWhenPlayingAfter");
            textReenableWhenPlayingAfterDelayInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "ReenableWhenPlayingAfterDelayInSeconds");
            
            //EQ Settings
            mpEqDisplay.DataBindings.Add("Checked", SoundGraphImonVfd.Settings.Instance, "EqDisplay");
            mpRestrictEQ.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "RestrictEQ");
            cmbEqRate.SelectedIndex = 0;
            cmbEqRate.DataBindings.Add("SelectedIndex", SoundGraphImon.Settings.Instance, "EqRate");

            //EQ Start delay management
            mpDelayEQ.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "EqStartDelay");
            textEqStartDelayInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "DelayEqTime");
            
            //EQ Period management
            mpEQTitleDisplay.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "EqPeriodic");
            textEqDisabledTimeInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "EqDisabledTimeInSeconds");
            textEqEnabledTimeInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "EqEnabledTimeInSeconds");

            //LCD
            if (SoundGraphImon.Settings.Instance.PreferFirstLineGeneral)
            {
                rbtnGeneralFirstLine.Checked = true;
            }
            else
            {
                rbtnGeneralSecondLine.Checked = true;
            }

            if (SoundGraphImon.Settings.Instance.PreferFirstLinePlayback)
            {
                rbtnPlaybackFirstLine.Checked = true;
            }
            else
            {
                rbtnPlaybackSecondLine.Checked = true;
            }

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SoundGraphImon.Settings.Instance.PreferFirstLineGeneral =
            rbtnGeneralFirstLine.Checked;
            SoundGraphImon.Settings.Instance.PreferFirstLinePlayback =
                rbtnPlaybackFirstLine.Checked;

            SoundGraphImon.Settings.Save();
            Close();
        }

    }
}
