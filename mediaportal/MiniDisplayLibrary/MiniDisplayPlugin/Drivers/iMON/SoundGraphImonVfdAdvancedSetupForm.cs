using System;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    public partial class SoundGraphImonVfdAdvancedSetupForm : MPConfigForm
    {
        public SoundGraphImonVfdAdvancedSetupForm()
        {
            InitializeComponent();

            //Generic iMON settings
            checkDisableWhenInBackground.DataBindings.Add("Checked", SoundGraphImonVfd.AdvancedSettings.Instance, "DisableWhenInBackground");
            checkDisableWhenIdle.DataBindings.Add("Checked", SoundGraphImonVfd.AdvancedSettings.Instance, "DisableWhenIdle");
            textDisableWhenIdleDelayInSeconds.DataBindings.Add("Text", SoundGraphImonVfd.AdvancedSettings.Instance, "DisableWhenIdleDelayInSeconds");
            //

            mpEqDisplay.DataBindings.Add("Checked", SoundGraphImonVfd.AdvancedSettings.Instance, "EqDisplay");
            cmbEqMode.SelectedIndex = 0;
            cmbEqMode.DataBindings.Add("SelectedIndex", SoundGraphImonVfd.AdvancedSettings.Instance, "EqMode");
            mpRestrictEQ.DataBindings.Add("Checked", SoundGraphImonVfd.AdvancedSettings.Instance, "RestrictEQ");
            cmbEqRate.SelectedIndex = 0;
            cmbEqRate.DataBindings.Add("SelectedIndex", SoundGraphImonVfd.AdvancedSettings.Instance, "EqRate");
            mpDelayEQ.DataBindings.Add("Checked", SoundGraphImonVfd.AdvancedSettings.Instance, "DelayEQ");
            cmbDelayEqTime.SelectedIndex = 0;
            cmbDelayEqTime.DataBindings.Add("SelectedIndex", SoundGraphImonVfd.AdvancedSettings.Instance, "DelayEqTime");
            if (SoundGraphImonVfd.AdvancedSettings.Instance.NormalEQ)
            {
                mpNormalEQ.Checked = true;
            }
            else if (SoundGraphImonVfd.AdvancedSettings.Instance.StereoEQ)
            {
                mpUseStereoEQ.Checked = true;
            }
            else if (SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter)
            {
                mpUseVUmeter.Checked = true;
            }
            else
            {
                mpUseVUmeter2.Checked = true;
            }
            cbVUindicators.DataBindings.Add("Checked", SoundGraphImonVfd.AdvancedSettings.Instance, "VUindicators");
            //mpDelayStartup.DataBindings.Add("Checked", SoundGraphImonVfd.AdvancedSettings.Instance, "DelayStartup");
            if (mpNormalEQ.Checked)
            {
                SoundGraphImonVfd.AdvancedSettings.Instance.NormalEQ = true;
                SoundGraphImonVfd.AdvancedSettings.Instance.StereoEQ = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else if (mpUseStereoEQ.Checked)
            {
                SoundGraphImonVfd.AdvancedSettings.Instance.NormalEQ = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.StereoEQ = true;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else if (mpUseVUmeter.Checked)
            {
                SoundGraphImonVfd.AdvancedSettings.Instance.NormalEQ = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.StereoEQ = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter = true;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter2 = false;
            }
            else
            {
                SoundGraphImonVfd.AdvancedSettings.Instance.NormalEQ = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.StereoEQ = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter = false;
                SoundGraphImonVfd.AdvancedSettings.Instance.VUmeter2 = true;
            }


        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SoundGraphImonVfd.AdvancedSettings.Save();
            Close();
        }

    }
}
