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
using System.Collections.Specialized;

namespace MediaPortal.DeployTool
{
  public partial class CustomInstallationTypeDlg : DeployDialog, IDeployDialog
  {
    public CustomInstallationTypeDlg()
    {
      InitializeComponent();
      type = DialogType.CUSTOM_INSTALLATION_TYPE;
      rbSingleSeat.Checked = true;
      UpdateUI();
    }

    #region IDeplayDialog interface
    public override void UpdateUI()
    {
      rbSingleSeat.Text = Localizer.Instance.GetString("CustomInstallation_rbSingleSeat");
      rbTvServerMaster.Text = Localizer.Instance.GetString("CustomInstallation_rbTvServerMaster");
      rbTvServerSlave.Text = Localizer.Instance.GetString("CustomInstallation_rbTvServerSlave");
      rbClient.Text = Localizer.Instance.GetString("CustomInstallation_rbClient");
    }
    public override DeployDialog GetNextDialog()
    {
      if (rbSingleSeat.Checked)
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.DBMSType);
      if (rbTvServerMaster.Checked)
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.DBMSType);
      if (rbTvServerSlave.Checked)
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.TvServerSettings);
      if (rbClient.Checked)
        return DialogFlowHandler.Instance.GetDialogInstance(DialogType.MPSettings);
      return null;
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      if (rbSingleSeat.Checked)
      {
        InstallationProperties.Instance.Set("InstallTypeHeader", rbSingleSeat.Text);
        InstallationProperties.Instance.Set("InstallType", "singleseat");
      }
      else if (rbTvServerMaster.Checked)
      {
        InstallationProperties.Instance.Set("InstallTypeHeader", rbTvServerMaster.Text);
        InstallationProperties.Instance.Set("InstallType", "tvserver_master");
      }
      else if (rbTvServerSlave.Checked)
      {
        InstallationProperties.Instance.Set("InstallTypeHeader", rbTvServerSlave.Text);
        InstallationProperties.Instance.Set("InstallType", "tvserver_slave");
      }
      else
      {
        InstallationProperties.Instance.Set("InstallTypeHeader", rbClient.Text);
        InstallationProperties.Instance.Set("InstallType", "client");
      }
    }
    #endregion
  }
}
