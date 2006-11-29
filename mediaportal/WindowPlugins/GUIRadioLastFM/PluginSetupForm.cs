#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using MediaPortal.Util;

namespace MediaPortal.GUI.RADIOLASTFM
{
  public partial class PluginSetupForm : Form
  {
    public PluginSetupForm()
    {
      InitializeComponent();
      LoadSettings();
    }

    #region Serialisation
    private void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxUseTrayIcon.Checked = xmlreader.GetValueAsBool("audioscrobbler", "showtrayicon", true);
        checkBoxShowBallonTips.Checked = xmlreader.GetValueAsBool("audioscrobbler", "showballontips", true);
        checkBoxSubmitToProfile.Checked = xmlreader.GetValueAsBool("audioscrobbler", "submitradiotracks", true);
        numericUpDownListEntries.Value = xmlreader.GetValueAsInt("audioscrobbler", "listentrycount", 12);
        comboBoxStreamPlayerType.SelectedIndex = xmlreader.GetValueAsInt("audioscrobbler", "streamplayertype", 0);
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("audioscrobbler", "showtrayicon", checkBoxUseTrayIcon.Checked);
        xmlwriter.SetValueAsBool("audioscrobbler", "showballontips", checkBoxShowBallonTips.Checked);
        xmlwriter.SetValueAsBool("audioscrobbler", "submitradiotracks", checkBoxSubmitToProfile.Checked);
        xmlwriter.SetValue("audioscrobbler", "listentrycount", numericUpDownListEntries.Value);
        xmlwriter.SetValue("audioscrobbler", "streamplayertype", comboBoxStreamPlayerType.SelectedIndex);
      }
    }
    #endregion

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
        checkBoxShowBallonTips.Checked = true; ;
      }
      else
      {
        checkBoxShowBallonTips.Checked = false;
        checkBoxShowBallonTips.Enabled = false;
      }
    }
  }
}