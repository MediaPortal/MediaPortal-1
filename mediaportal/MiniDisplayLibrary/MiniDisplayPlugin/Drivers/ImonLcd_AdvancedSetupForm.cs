using System;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
    public partial class ImonLcd_AdvancedSetupForm : MPConfigForm
    {
        public ImonLcd_AdvancedSetupForm()
        {
            InitializeComponent();

            if (ImonLcd.AdvancedSettings.Instance.PreferFirstLineGeneral)
            {
                rbtnGeneralFirstLine.Checked = true;
            }
            else
            {
                rbtnGeneralSecondLine.Checked = true;
            }

            if (ImonLcd.AdvancedSettings.Instance.PreferFirstLinePlayback)
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
            ImonLcd.AdvancedSettings.Instance.PreferFirstLineGeneral =
                rbtnGeneralFirstLine.Checked;
            ImonLcd.AdvancedSettings.Instance.PreferFirstLinePlayback =
                rbtnPlaybackFirstLine.Checked;
            ImonLcd.AdvancedSettings.Save();
            Close();
        }
    }
}
