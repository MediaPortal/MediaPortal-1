using System;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    public partial class SoundGraphImonLcdAdvancedSetupForm : MPConfigForm
    {
        public SoundGraphImonLcdAdvancedSetupForm()
        {
            InitializeComponent();

            if (SoundGraphImonLcd.AdvancedSettings.Instance.PreferFirstLineGeneral)
            {
                rbtnGeneralFirstLine.Checked = true;
            }
            else
            {
                rbtnGeneralSecondLine.Checked = true;
            }

            if (SoundGraphImonLcd.AdvancedSettings.Instance.PreferFirstLinePlayback)
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
            SoundGraphImonLcd.AdvancedSettings.Instance.PreferFirstLineGeneral =
                rbtnGeneralFirstLine.Checked;
            SoundGraphImonLcd.AdvancedSettings.Instance.PreferFirstLinePlayback =
                rbtnPlaybackFirstLine.Checked;
            SoundGraphImonLcd.AdvancedSettings.Save();
            Close();
        }
    }
}
