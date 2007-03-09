#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Profile;
using MediaPortal.Configuration;
#endregion

namespace MediaPortal.Plugins.Process
{
  public partial class SetupForm : Form
  {
    #region Ctor
    public SetupForm()
    {
      InitializeComponent();
      LoadSettings();
    }
    #endregion

    #region Serialization
    public void LoadSettings()
    {
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        homeOnlyCheckBox.Checked = reader.GetValueAsBool("psclientplugin", "homeonly", true);
        extLogCheckBox.Checked = reader.GetValueAsBool("psclientplugin", "extensivelogging", false);
        checkNumericUpDown.Value = reader.GetValueAsInt("psclientplugin", "checkinterval", 25);
        shutModeComboBox.SelectedIndex = reader.GetValueAsInt("psclientplugin", "shutdownmode", 2);
        idleNumericUpDown.Value = reader.GetValueAsInt("psclientplugin", "idletimeout", 5);
        forceCheckBox.Checked = reader.GetValueAsBool("psclientplugin", "forceshutdown", false);
        wakeupNumericUpDown.Value = reader.GetValueAsInt("psclientplugin", "prewakeup", 60);
      }
    }

    public void SaveSettings()
    {
      using (Settings writer = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        writer.SetValueAsBool("psclientplugin", "homeonly", homeOnlyCheckBox.Checked);
        writer.SetValueAsBool("psclientplugin", "extensivelogging", extLogCheckBox.Checked);
        writer.SetValue("psclientplugin", "checkinterval", checkNumericUpDown.Value);
        writer.SetValue("psclientplugin", "shutdownmode", shutModeComboBox.SelectedIndex.ToString());
        writer.SetValue("psclientplugin", "idletimeout", idleNumericUpDown.Value);
        writer.SetValueAsBool("psclientplugin", "forceshutdown", forceCheckBox.Checked);
        writer.SetValue("psclientplugin", "prewakeup", wakeupNumericUpDown.Value);
      }
    }
    #endregion

    private void okButton_Click(object sender, EventArgs e)
    {
      SaveSettings();
      Close();
    }

  }
}