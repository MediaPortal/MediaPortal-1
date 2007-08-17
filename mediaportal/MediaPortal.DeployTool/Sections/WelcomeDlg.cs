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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  public partial class WelcomeDlg : DeployDialog, IDeployDialog
  {
    public WelcomeDlg()
    {
      InitializeComponent();
      type = DialogType.Welcome;
      cbLanguage.SelectedIndex = 0;
      UpdateUI();
    }

    #region IDeplayDialog interface
    public override void UpdateUI()
    {
      labelHeading1.Text = Localizer.Instance.GetString("Welcome_labelHeading1");
      labelHeading2.Text = Localizer.Instance.GetString("Welcome_labelHeading2");
      labelHeading3.Text = Localizer.Instance.GetString("Welcome_labelHeading3");
    }
    public override DeployDialog GetNextDialog()
    {
      DialogFlowHandler.Instance.ResetHistory();
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE);
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("language",GetLanguageId());
    }
    #endregion

    private string GetLanguageId()
    {
      switch (cbLanguage.Text)
      {
        case "english":
          return "en-US";
        case "german":
          return "de-DE";
      }
      return "en-US";
    }

    private void cbLanguage_SelectedIndexChanged(object sender, EventArgs e)
    {
      Localizer.Instance.SwitchCulture(GetLanguageId());
      UpdateUI();
    }

  }
}
