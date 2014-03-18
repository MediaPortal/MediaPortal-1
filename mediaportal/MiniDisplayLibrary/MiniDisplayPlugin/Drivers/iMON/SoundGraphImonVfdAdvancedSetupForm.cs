using System;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    public partial class SoundGraphImonVfdAdvancedSetupForm : MPConfigForm
    {
        public SoundGraphImonVfdAdvancedSetupForm()
        {
            InitializeComponent();

            if (SoundGraphImonVfd.AdvancedSettings.Instance.PreferFirstLineGeneral)
            {
                rbtnGeneralFirstLine.Checked = true;
            }
            else
            {
                rbtnGeneralSecondLine.Checked = true;
            }

            if (SoundGraphImonVfd.AdvancedSettings.Instance.PreferFirstLinePlayback)
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
            SoundGraphImonVfd.AdvancedSettings.Instance.PreferFirstLineGeneral =
                rbtnGeneralFirstLine.Checked;
            SoundGraphImonVfd.AdvancedSettings.Instance.PreferFirstLinePlayback =
                rbtnPlaybackFirstLine.Checked;
            SoundGraphImonVfd.AdvancedSettings.Save();
            Close();
        }
    }
}
