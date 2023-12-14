#region Copyright (C) 2005-2023 Team MediaPortal

// Copyright (C) 2005-2023 Team MediaPortal
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
using System.IO;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class TvServerWarningDlg : DeployDialog
  {
    public TvServerWarningDlg()
    {
      InitializeComponent();
      type = DialogType.TvServerWarning;

      UpdateUI();
    }

    #region IDeployDialog interface

    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.GetBestTranslation("TvServerWarning_labelHeading");
    }

    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(
        InstallationProperties.Instance["OneClickInstallation"] == "1" ? DialogType.ExtensionChoice : DialogType.DBMSType);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    public override void SetProperties()
    {
    }

    #endregion

  }
}