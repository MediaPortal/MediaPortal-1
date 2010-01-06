#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class GeneralScreensaver : SectionSettings
  {
    #region ctor

    public GeneralScreensaver()
      : this("Screensaver") {}

    public GeneralScreensaver(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Persistance

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        checkBoxEnableScreensaver.Checked = xmlreader.GetValueAsBool("general", "IdleTimer", true);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "IdleTimeValue", 300);
        radioBtnBlankScreen.Checked = xmlreader.GetValueAsBool("general", "IdleBlanking", false);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.SetValueAsBool("general", "IdleTimer", checkBoxEnableScreensaver.Checked);
        xmlreader.SetValue("general", "IdleTimeValue", numericUpDownDelay.Value);
        xmlreader.SetValueAsBool("general", "IdleBlanking", radioBtnBlankScreen.Checked);
      }
    }

    #endregion

    private void checkBoxEnableScreensaver_CheckedChanged(object sender, System.EventArgs e)
    {
      groupBoxIdleAction.Enabled = numericUpDownDelay.Enabled = checkBoxEnableScreensaver.Checked;
    }
  }
}