#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using MediaPortal.Configuration;
//using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.TvNotifies
{
  public partial class NotifySetupForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    public NotifySetupForm()
    {
      InitializeComponent();
      LoadSettings();
    }

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        textBoxPreNotify.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "notifyTVBefore", 300));
        textBoxNotifyTimeoutVal.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "notifyTVTimeout", 15));
        checkBoxNotifyPlaySound.Checked = xmlreader.GetValueAsBool("movieplayer", "notifybeep", true);
      }
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("movieplayer", "notifyTVBefore", textBoxPreNotify.Text);
        xmlwriter.SetValue("movieplayer", "notifyTVTimeout", textBoxNotifyTimeoutVal.Text);
        xmlwriter.SetValueAsBool("movieplayer", "notifybeep", checkBoxNotifyPlaySound.Checked);
      }
    }
    #endregion

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }
  }
}