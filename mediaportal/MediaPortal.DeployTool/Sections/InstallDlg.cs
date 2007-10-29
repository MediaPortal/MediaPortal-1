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
using System.IO;
using Microsoft.Win32;

namespace MediaPortal.DeployTool
{
  public partial class InstallDlg : DeployDialog, IDeployDialog
  {
    public InstallDlg()
    {
      InitializeComponent();
      type=DialogType.Installation;
      PopulateListView();
      UpdateUI();
    }

    #region IDeplayDialog interface
    public override void UpdateUI()
    {
      labelHeading.Text = Localizer.Instance.GetString("Install_labelHeading");
      buttonInstall.Text = Localizer.Instance.GetString("Install_buttonInstall");
      listView.Columns[0].Text = Localizer.Instance.GetString("Install_colApplication");
      listView.Columns[1].Text = Localizer.Instance.GetString("Install_colState");
      listView.Columns[2].Text = Localizer.Instance.GetString("Install_colAction");
    }
    public override DeployDialog GetNextDialog()
    {
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Finished);
    }
    public override bool SettingsValid()
    {
      if (!InstallationComplete())
      {
        Utils.ErrorDlg(Localizer.Instance.GetString("Install_errAppsMissing"));
        return false;
      }
      else
        return true;
    }
    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("finished", "yes");
    }
    #endregion

    private bool InstallationComplete()
    {
      bool isComplete = true;
      foreach (ListViewItem item in listView.Items)
      {
        IInstallationPackage package = (IInstallationPackage)item.Tag;
        CheckResult result = package.CheckStatus();
        if (result.state != CheckState.INSTALLED)
        {
          isComplete = false;
          break;
        }
      }
      return isComplete;
    }
    private void AddPackageToListView(IInstallationPackage package)
    {
      ListViewItem item=listView.Items.Add(package.GetDisplayName());
      item.Tag = package;
      CheckResult result = package.CheckStatus();
      switch (result.state)
      {
        case CheckState.INSTALLED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateInstalled"));
          item.SubItems.Add(Localizer.Instance.GetString("Install_actionNothing"));
          item.ForeColor = System.Drawing.Color.Green;
          break;
        case CheckState.NOT_INSTALLED:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateNotInstalled"));
          if (result.needsDownload)
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionDownloadInstall"));
          else
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionInstall"));
          item.ForeColor = System.Drawing.Color.Red;
          break;
        case CheckState.VERSION_MISMATCH:
          item.SubItems.Add(Localizer.Instance.GetString("Install_stateVersionMismatch"));
          if (result.needsDownload)
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionUninstallDownloadInstall"));
          else
            item.SubItems.Add(Localizer.Instance.GetString("Install_actionUninstallInstall"));
          item.ForeColor = System.Drawing.Color.Purple;
          break;
      }
    }
    private void PopulateListView()
    {
      listView.Items.Clear();
      if (InstallationProperties.Instance["InstallType"] == "singleseat")
      {
        AddPackageToListView(new DirectX9Checker());
        AddPackageToListView(new MediaPortalChecker());
        if (InstallationProperties.Instance["DBMSType"] == "mssql")
          AddPackageToListView(new MSSQLExpressChecker());
        else
          AddPackageToListView(new MySQLChecker());
        AddPackageToListView(new TvServerChecker());
        AddPackageToListView(new TvPluginServerChecker());
        AddPackageToListView(new WindowsFirewallChecker());
      }
      else if (InstallationProperties.Instance["InstallType"] == "tvserver_master")
      {
        if (InstallationProperties.Instance["DBMSType"] == "mssql")
          AddPackageToListView(new MSSQLExpressChecker());
        else
          AddPackageToListView(new MySQLChecker());
        AddPackageToListView(new TvServerChecker());
        AddPackageToListView(new WindowsFirewallChecker());
      }
      else if (InstallationProperties.Instance["InstallType"] == "tvserver_slave")
      {
        AddPackageToListView(new TvServerChecker());
        AddPackageToListView(new WindowsFirewallChecker());
      }
      else if (InstallationProperties.Instance["InstallType"] == "client")
      {
        AddPackageToListView(new DirectX9Checker());
        AddPackageToListView(new MediaPortalChecker());
        AddPackageToListView(new TvPluginServerChecker());
      }
      listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
      if (InstallationComplete())
        buttonInstall.Enabled = false;
    }

    private void RequirementsDlg_ParentChanged(object sender, EventArgs e)
    {
      if (Parent != null)
        PopulateListView();
    }

    private bool PerformPackageAction(IInstallationPackage package,ListViewItem item)
    {
      CheckResult result = package.CheckStatus();
      if (result.state != CheckState.INSTALLED)
      {
        switch (result.state)
        {
          case CheckState.NOT_INSTALLED:
            if (result.needsDownload)
            {
              item.SubItems[1].Text=Localizer.Instance.GetString("Install_msgDownloading");
              Update();
              if (!package.Download())
              {
                
                Utils.ErrorDlg(string.Format(Localizer.Instance.GetString("Install_errInstallFailed"),package.GetDisplayName()));
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
        }
      }
      return true;
    }
    private void buttonInstall_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem item in listView.Items)
      {
        IInstallationPackage package = (IInstallationPackage)item.Tag;
        if (!PerformPackageAction(package,item))
          break;
      }
      PopulateListView();
    }
  }
}
