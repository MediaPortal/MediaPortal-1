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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Resources;

namespace MediaPortal.DeployTool
{
  public partial class WatchTVDlg : DeployDialog, IDeployDialog
  {
    public WatchTVDlg()
    {
      // First install screen: check if OS is compliant !!!
      Utils.CheckOSRequirement(true);

      InitializeComponent();
      type = DialogType.WatchTV;
      labelSectionHeader.Text = "";
      rbYesWatchTv.Checked = true;
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      labelSectionHeader.Text = Localizer.Instance.GetString("WatchTV_labelSectionHeader");
      rbYesWatchTv.Text = Localizer.Instance.GetString("WatchTV_on");
      rbNoWatchTv.Text = Localizer.Instance.GetString("WatchTV_off");
    }

    public override DeployDialog GetNextDialog()
    {
        if (rbYesWatchTv.Checked)
            return DialogFlowHandler.Instance.GetDialogInstance(DialogType.WatchHDTv);
        else
            return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE_WITHOUT_TVENGINE);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    #endregion
  }
}
