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

using System;
using MediaPortal.Configuration;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.GUI.RADIOLASTFM
{
  public partial class PluginSetupForm : MPConfigForm
  {
    public PluginSetupForm()
    {
      InitializeComponent();
      LoadSettings();
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        checkBoxUseTrayIcon.Checked = xmlreader.GetValueAsBool("audioscrobbler", "showtrayicon", true);
        checkBoxShowBallonTips.Checked = xmlreader.GetValueAsBool("audioscrobbler", "showballontips", true);
        checkBoxSubmitToProfile.Checked = xmlreader.GetValueAsBool("audioscrobbler", "submitradiotracks", true);
        checkBoxDirectSkip.Checked = xmlreader.GetValueAsBool("audioscrobbler", "directskip", false);
        numericUpDownListEntries.Value = xmlreader.GetValueAsInt("audioscrobbler", "listentrycount", 24);
        comboBoxStreamPlayerType.SelectedIndex = xmlreader.GetValueAsInt("audioscrobbler", "streamplayertype", 0);
        checkBoxOneClickMode.Checked = xmlreader.GetValueAsBool("audioscrobbler", "oneclickstart", false);
        checkBoxUseSMSStyle.Checked = xmlreader.GetValueAsBool("audioscrobbler", "usesmskeyboard", true);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("audioscrobbler", "showtrayicon", checkBoxUseTrayIcon.Checked);
        xmlwriter.SetValueAsBool("audioscrobbler", "showballontips", checkBoxShowBallonTips.Checked);
        xmlwriter.SetValueAsBool("audioscrobbler", "submitradiotracks", checkBoxSubmitToProfile.Checked);
        xmlwriter.SetValueAsBool("audioscrobbler", "directskip", checkBoxDirectSkip.Checked);
        xmlwriter.SetValue("audioscrobbler", "listentrycount", numericUpDownListEntries.Value);
        xmlwriter.SetValue("audioscrobbler", "streamplayertype", 0); // comboBoxStreamPlayerType.SelectedIndex);
        xmlwriter.SetValueAsBool("audioscrobbler", "oneclickstart", checkBoxOneClickMode.Checked);
        xmlwriter.SetValueAsBool("audioscrobbler", "usesmskeyboard", checkBoxUseSMSStyle.Checked);
      }
    }

    #endregion

    #region Control handling

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void checkBoxUseTrayIcon_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUseTrayIcon.Checked)
      {
        checkBoxShowBallonTips.Enabled = true;
        checkBoxShowBallonTips.Checked = true;
        ;
      }
      else
      {
        checkBoxShowBallonTips.Checked = false;
        checkBoxShowBallonTips.Enabled = false;
      }
    }

    #endregion
  }
}