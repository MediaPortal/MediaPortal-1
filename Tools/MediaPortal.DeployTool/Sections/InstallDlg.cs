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
using System.IO;
using Microsoft.Win32;

namespace MediaPortal.DeployTool
{
  public partial class InstallDlg : DeployDialog, IDeployDialog
  {
    public InstallDlg()
    {
      InitializeComponent();
      type = DialogType.Installation;
      PopulateListView();
      UpdateUI();
    }

    #region IDeployDialog interface
    public override void UpdateUI()
    {
      // This change the description of "Next" button to "Install" or "Download"
      InstallationProperties.Instance.Set("Install_Dialog", "yes");

      if (InstallationProperties.Instance["InstallType"] == "download_only")
        labelHeading.Text = Localizer.Instance.GetString("Install_labelHeadingDownload");
      else
        labelHeading.Text = Localizer.Instance.GetString("Install_labelHeadingInstall");

      listView.Columns[0].Text = Localizer.Instance.GetString("Install_colApplication");
      listView.Columns[1].Text = Localizer.Instance.GetString("Install_colState");
      listView.Columns[2].Text = Localizer.Instance.GetString("Install_colAction");
      labelSectionHeader.Text = "";
    }
    public override DeployDialog GetNextDialog()
    {
      foreach (ListViewItem item in listView.Items)
      {
        IInstallationPackage package = (IInstallationPackage)item.Tag;
        if (!PerformPackageAction(package, item))
          break;
      }
      PopulateListView();
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Finished);
    }
    public override bool SettingsValid()
    {
      return true;
    }
    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("finished", "yes");
    }
    #endregion

    private void AddPackageToListView(IInstallationPackage package)
    {
      listView.SmallImageList = iconsList;
      ListViewItem item = listView.Items.Add(package.GetDisplayName());
      item.Tag = package;
      CheckResult result = package.CheckStatus();
      switch (result.state)
      {
        case CheckState.INSTALLED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateInstalled"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_INSTALLED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateNotInstalled"));
          if (result.needsDownload)
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionDownloadInstall"));
          else
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionInstall"));
          item.ImageIndex = 1;
          break;
        case CheckState.CONFIGURED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateConfigured"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_CONFIGURED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateNotConfigured"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionConfigure"));
          item.ImageIndex = 1;
          break;
        case CheckState.REMOVED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateRemoved"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_REMOVED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateUninstall"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionRemove"));
          item.ImageIndex = 1;
          break;
        case CheckState.DOWNLOADED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateDownloaded"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_DOWNLOADED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateNotDownloaded"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionDownload"));
          item.ImageIndex = 1;
          break;
        case CheckState.VERSION_MISMATCH:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateVersionMismatch"));
          if (result.needsDownload)
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionUninstallDownloadInstall"));
          else
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionUninstallInstall"));
          item.ImageIndex = 2;
          break;
      }
    }

    private void PopulateListView()
    {
      listView.Items.Clear();
      if (InstallationProperties.Instance["InstallType"] != "download_only")
        AddPackageToListView(new OldPackageChecker());
      AddPackageToListView(new DirectX9Checker());
      AddPackageToListView(new VCRedistChecker());
      switch (InstallationProperties.Instance["InstallType"])
      {
        case "singleseat":
          AddPackageToListView(new MediaPortalChecker());
          if (InstallationProperties.Instance["DBMSType"] == "mssql2005")
            AddPackageToListView(new MSSQLExpressChecker());
          if (InstallationProperties.Instance["DBMSType"] == "mysql")
            AddPackageToListView(new MySQLChecker());
          AddPackageToListView(new TvServerChecker());
          AddPackageToListView(new TvPluginChecker());
          break;

        case "tvserver_master":
          if (InstallationProperties.Instance["DBMSType"] == "mssql2005")
            AddPackageToListView(new MSSQLExpressChecker());
          if (InstallationProperties.Instance["DBMSType"] == "mysql")
            AddPackageToListView(new MySQLChecker());
          AddPackageToListView(new TvServerChecker());
          break;

        case "client":
          AddPackageToListView(new MediaPortalChecker());
          AddPackageToListView(new TvPluginChecker());
          break;

        case "mp_only":
          AddPackageToListView(new MediaPortalChecker());
          break;

        case "download_only":
          AddPackageToListView(new MediaPortalChecker());
          AddPackageToListView(new MSSQLExpressChecker());
          AddPackageToListView(new MySQLChecker());
          AddPackageToListView(new TvServerChecker());
          AddPackageToListView(new TvPluginChecker());
          break;

      }
      if ((InstallationProperties.Instance["ConfigureMediaPortalFirewall"] == "1" ||
          InstallationProperties.Instance["ConfigureTVServerFirewall"] == "1") && InstallationProperties.Instance["InstallType"] != "download_only")
        AddPackageToListView(new WindowsFirewallChecker());
      listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
    }

    private void RequirementsDlg_ParentChanged(object sender, EventArgs e)
    {
      if (Parent != null)
        PopulateListView();
    }

    private bool PerformPackageAction(IInstallationPackage package, ListViewItem item)
    {
      CheckResult result = package.CheckStatus();
      if (result.state != CheckState.INSTALLED)
      {
        switch (result.state)
        {
          case CheckState.NOT_INSTALLED:
            if (result.needsDownload)
            {
              item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgDownloading");
              Update();
              if (!package.Download())
              {
                Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errInstallFailed"), package.GetDisplayName()));
                return false;
              }
            }
            item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgInstalling");
            Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errInstallFailed"), package.GetDisplayName()));
              return false;
            }
            break;

          case CheckState.NOT_CONFIGURED:
            item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgConfiguring");
            Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errConfigureFailed"), package.GetDisplayName()));
              return false;
            }
            break;

          case CheckState.NOT_REMOVED:
            item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgUninstalling");
            Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errRemoveFailed"), package.GetDisplayName()));
              return false;
            }
            break;

          case CheckState.VERSION_MISMATCH:
            item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgUninstalling");
            Update();
            if (!package.UnInstall())
            {
              Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errUinstallFailed"), package.GetDisplayName()));
              return false;
            }
            if (result.needsDownload)
            {
              item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgDownloading");
              Update();
              if (!package.Download())
              {
                Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errDownloadFailed"), package.GetDisplayName()));
                return false;
              }
            }
            item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgInstalling");
            Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errInstallFailed"), package.GetDisplayName()));
              return false;
            }
            break;

          case CheckState.NOT_DOWNLOADED:
            item.SubItems[1].Text = Localizer.Instance.GetString("Install_msgDownloading");
            Update();
            if (!package.Download())
            {
              Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errDownloadFailed"), package.GetDisplayName()));
              return false;
            }
            break;
        }
      }
      item.ImageIndex = 0;
      return true;
    }
  }
}
