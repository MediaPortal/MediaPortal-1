#region Copyright (C) 2007-2008 Team MediaPortal

/* 
 *	Copyright (C) 2007-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#region Usings

using System;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;

#endregion

namespace MediaPortal.Plugins.Process
{
  public partial class PowerSchedulerClientSetup : MPConfigForm
  {
    public PowerSchedulerClientSetup()
    {
      InitializeComponent();
      LoadSettings();
    }

    public void LoadSettings()
    {
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        homeOnlyCheckBox.Checked = reader.GetValueAsBool("psclientplugin", "homeonly", true);
        extLogCheckBox.Checked = reader.GetValueAsBool("psclientplugin", "extensivelogging", false);
        shutModeComboBox.SelectedIndex = reader.GetValueAsInt("psclientplugin", "shutdownmode", 1);
        forceCheckBox.Checked = reader.GetValueAsBool("psclientplugin", "forceshutdown", false);
        enableShutdownCheckBox.Checked = reader.GetValueAsBool("psclientplugin", "shutdownenabled", false);
        idleNumericUpDown.Value = reader.GetValueAsInt("psclientplugin", "idletimeout", 5);
      }
    }

    public void SaveSettings()
    {
      using (Settings writer = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        writer.SetValueAsBool("psclientplugin", "homeonly", homeOnlyCheckBox.Checked);
        writer.SetValueAsBool("psclientplugin", "extensivelogging", extLogCheckBox.Checked);
        writer.SetValue("psclientplugin", "shutdownmode", shutModeComboBox.SelectedIndex.ToString());
        writer.SetValueAsBool("psclientplugin", "forceshutdown", forceCheckBox.Checked);
        writer.SetValueAsBool("psclientplugin", "shutdownenabled", enableShutdownCheckBox.Checked);
        writer.SetValue("psclientplugin", "idletimeout", idleNumericUpDown.Value);
      }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      SaveSettings();
      Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      LoadSettings();
      Close();
    }
  }
}