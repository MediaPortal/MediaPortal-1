#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class General : SectionSettings
  {
    public General()
      : this("General") {}

    public General(string name)
      : base(name)
    {
      InitializeComponent();
    }

    private string loglevel = "3"; // 1= error, 2 = info, 3 = debug

    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private IContainer components = null;

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        loglevel = xmlreader.GetValueAsString("general", "loglevel", "3"); // set loglevel to 2:info 3:debug
        cbDebug.SelectedIndex = Convert.ToInt16(loglevel);

        string prio = xmlreader.GetValueAsString("general", "ThreadPriority", "Normal");
        // Set the selected index, otherwise the SelectedItem in SaveSettings will be null, if the box isn't checked
        mpThreadPriority.SelectedIndex = mpThreadPriority.Items.IndexOf(prio);

        checkBoxEnableWatchdog.Checked = xmlreader.GetValueAsBool("general", "watchdogEnabled", false);
        checkBoxAutoRestart.Checked = xmlreader.GetValueAsBool("general", "restartOnError", true);
        numericUpDownDelay.Value = xmlreader.GetValueAsInt("general", "restart delay", 10);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("general", "loglevel", cbDebug.SelectedIndex);
        xmlwriter.SetValue("general", "ThreadPriority", mpThreadPriority.SelectedItem.ToString());

        xmlwriter.SetValueAsBool("general", "watchdogEnabled", checkBoxEnableWatchdog.Checked);
        xmlwriter.SetValueAsBool("general", "restartOnError", checkBoxAutoRestart.Checked);
        xmlwriter.SetValue("general", "restart delay", numericUpDownDelay.Value);
      }
    }

    public override void OnSectionActivated()
    {
      mpThreadPriority.Visible = SettingsForm.AdvancedMode;
      labelPriority.Visible = SettingsForm.AdvancedMode;
      base.OnSectionActivated();
    }
  }
}