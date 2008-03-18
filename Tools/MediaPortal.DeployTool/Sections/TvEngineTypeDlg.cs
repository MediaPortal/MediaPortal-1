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
  public partial class TvEngineTypeDlg : DeployDialog, IDeployDialog
  {
      public TvEngineTypeDlg()
    {
      InitializeComponent();
      type = DialogType.TvEngineType;
      labelSectionHeader.Text = "";
      rbTV3.Checked = true;
      UpdateUI();
    }

    #region IDeplayDialog interface
    public override void UpdateUI()
    {
        labelSectionHeader.Text = Localizer.Instance.GetString("TvEngineType_labelSectionHeader");
        rbTV2.Text = Localizer.Instance.GetString("TvEngineType_rbTvBuildIn");
        rbTV3.Text = Localizer.Instance.GetString("TvEngineType_rbTvEngine");
        labelTV3.Text = Localizer.Instance.GetString("TvEngineType_labelTvEngine");
    }
    public override DeployDialog GetNextDialog()
    {
      if (rbTV2.Checked)
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE_WITHOUT_TVENGINE);
      else
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.BASE_INSTALLATION_TYPE);
    }
    public override bool SettingsValid()
    {
      return true;
    }

    #endregion

  }
}
