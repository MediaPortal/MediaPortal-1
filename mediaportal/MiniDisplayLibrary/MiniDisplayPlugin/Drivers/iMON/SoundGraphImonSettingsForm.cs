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
            checkReenableAfter.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "ReenableAfter");
            textReenableAfterDelayInSeconds.DataBindings.Add("Text", SoundGraphImon.Settings.Instance, "ReenableAfterDelayInSeconds");
            //

            mpEqDisplay.DataBindings.Add("Checked", SoundGraphImonVfd.Settings.Instance, "EqDisplay");
            cmbEqMode.SelectedIndex = 0;
            cmbEqMode.DataBindings.Add("SelectedIndex", SoundGraphImon.Settings.Instance, "EqMode");
            mpRestrictEQ.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "RestrictEQ");
            cmbEqRate.SelectedIndex = 0;
            cmbEqRate.DataBindings.Add("SelectedIndex", SoundGraphImon.Settings.Instance, "EqRate");
            mpDelayEQ.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "DelayEQ");
            cmbDelayEqTime.SelectedIndex = 0;
            cmbDelayEqTime.DataBindings.Add("SelectedIndex", SoundGraphImon.Settings.Instance, "DelayEqTime");
            if (SoundGraphImon.Settings.Instance.NormalEQ)
            {
                mpNormalEQ.Checked = true;
            }
            else if (SoundGraphImon.Settings.Instance.StereoEQ)
            {
                mpUseStereoEQ.Checked = true;
            }
            else if (SoundGraphImon.Settings.Instance.VUmeter)
            {
                mpUseVUmeter.Checked = true;
            }
            else
            {
                mpUseVUmeter2.Checked = true;
            }
            cbVUindicators.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "VUindicators");
            //mpDelayStartup.DataBindings.Add("Checked", SoundGraphImon.Settings.Instance, "DelayStartup");
            if (mpNormalEQ.Checked)
            {
                SoundGraphImon.Settings.Instance.NormalEQ = true;
                SoundGraphImon.Settings.Instance.StereoEQ = false;
                SoundGraphImon.Settings.Instance.VUmeter = false;
                SoundGraphImon.Settings.Instance.VUmeter2 = false;
            }
            else if (mpUseStereoEQ.Checked)
            {
                SoundGraphImon.Settings.Instance.NormalEQ = false;
                SoundGraphImon.Settings.Instance.StereoEQ = true;
                SoundGraphImon.Settings.Instance.VUmeter = false;
                SoundGraphImon.Settings.Instance.VUmeter2 = false;
            }
            else if (mpUseVUmeter.Checked)
            {
                SoundGraphImon.Settings.Instance.NormalEQ = false;
                SoundGraphImon.Settings.Instance.StereoEQ = false;
                SoundGraphImon.Settings.Instance.VUmeter = true;
                SoundGraphImon.Settings.Instance.VUmeter2 = false;
            }
            else
            {
                SoundGraphImon.Settings.Instance.NormalEQ = false;
                SoundGraphImon.Settings.Instance.StereoEQ = false;
                SoundGraphImon.Settings.Instance.VUmeter = false;
                SoundGraphImon.Settings.Instance.VUmeter2 = true;
            }

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
