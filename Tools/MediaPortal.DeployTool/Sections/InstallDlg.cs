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
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.DeployTool.InstallationChecks;

namespace MediaPortal.DeployTool.Sections
{
  public partial class InstallDlg : DeployDialog
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

      labelHeading.Text = InstallationProperties.Instance["InstallType"] == "download_only" ? Localizer.GetBestTranslation("Install_labelHeadingDownload") : Localizer.GetBestTranslation("Install_labelHeadingInstall");

      listView.Columns[0].Text = Localizer.GetBestTranslation("Install_colApplication");
      listView.Columns[1].Text = Localizer.GetBestTranslation("Install_colState");
      listView.Columns[2].Text = Localizer.GetBestTranslation("Install_colAction");
      labelSectionHeader.Text = "";
    }
    public override DeployDialog GetNextDialog()
    {
      foreach (ListViewItem item in listView.Items)
      {
        IInstallationPackage package = (IInstallationPackage)item.Tag;
        int action = PerformPackageAction(package, item);
        item.UseItemStyleForSubItems = false;
        item.SubItems[1].Font = new Font(item.SubItems[1].Font, FontStyle.Regular);
        listView.Update();
        if (action == 2)
        {
          break;
        }
        if (action == 1)
        {
          item.SubItems[1].Text = Localizer.GetBestTranslation("Install_stateInstalled");
          item.SubItems[2].Text = Localizer.GetBestTranslation("Install_actionNothing");
          item.ImageIndex = 0;
        }
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
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateInstalled"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_INSTALLED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateNotInstalled"));
          if (result.needsDownload)
            item.SubItems.Add(Localizer.GetBestTranslation("Install_actionDownloadInstall"));
          else
            item.SubItems.Add(Localizer.GetBestTranslation("Install_actionInstall"));
          item.ImageIndex = 1;
          break;
        case CheckState.CONFIGURED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateConfigured"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_CONFIGURED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateNotConfigured"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionConfigure"));
          item.ImageIndex = 1;
          break;
        case CheckState.REMOVED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateRemoved"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_REMOVED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateUninstall"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionRemove"));
          item.ImageIndex = 1;
          break;
        case CheckState.DOWNLOADED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateDownloaded"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionNothing"));
          item.ImageIndex = 0;
          break;
        case CheckState.NOT_DOWNLOADED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateNotDownloaded"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionDownload"));
          item.ImageIndex = 1;
          break;
        case CheckState.VERSION_MISMATCH:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateVersionMismatch"));
          if (result.needsDownload)
            item.SubItems.Add(Localizer.GetBestTranslation("Install_actionUninstallDownloadInstall"));
          else
          {
            if (InstallationProperties.Instance["UpdateMode"] == "yes")
            {
              item.SubItems.Add(Localizer.GetBestTranslation("Install_actionUpgradeInstall"));
            }
            else
            {
              item.SubItems.Add(Localizer.GetBestTranslation("Install_actionUninstallInstall"));
            }
          }

          item.ImageIndex = 2;
          break;
        case CheckState.SKIPPED:
          item.SubItems.Add(Localizer.GetBestTranslation("Install_stateSkipped"));
          item.SubItems.Add(Localizer.GetBestTranslation("Install_actionNothing"));
          item.ImageIndex = 0;
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
      AddPackageToListView(new WindowsMediaPlayerChecker());
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

    private int PerformPackageAction(IInstallationPackage package, ListViewItem item)
    {
      //
      // return 0: nothing to do
      //        1: install successufull
      //        2: install error
      //
      CheckResult result = package.CheckStatus();
      if (result.state != CheckState.INSTALLED &&
          result.state != CheckState.REMOVED &&
          result.state != CheckState.DOWNLOADED &&
          result.state != CheckState.SKIPPED)
      {
        item.UseItemStyleForSubItems = false;
        item.SubItems[1].Font = new Font(item.SubItems[1].Font, FontStyle.Bold);
        switch (result.state)
        {
          case CheckState.NOT_INSTALLED:
            if (result.needsDownload)
            {
              item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgDownloading");
              listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
              listView.Update();
              if (!package.Download())
              {
                Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errInstallFailed"), package.GetDisplayName()));
                return 2;
              }
            }
            item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgInstalling");
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errInstallFailed"), package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.NOT_CONFIGURED:
            item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgConfiguring");
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errConfigureFailed"), package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.NOT_REMOVED:
            item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgUninstalling");
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errRemoveFailed"), package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.VERSION_MISMATCH:
            item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgUninstalling");
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Update();
            if (!package.UnInstall())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errUinstallFailed"), package.GetDisplayName()));
              return 2;
            }
            if (result.needsDownload)
            {
              item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgDownloading");
              listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
              listView.Update();
              if (!package.Download())
              {
                Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errDownloadFailed"), package.GetDisplayName()));
                return 2;
              }
            }
            item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgInstalling");
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errInstallFailed"), package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.NOT_DOWNLOADED:
            item.SubItems[1].Text = Localizer.GetBestTranslation("Install_msgDownloading");
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Update();
            if (!package.Download())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errDownloadFailed"), package.GetDisplayName()));
              return 2;
            }
            break;
        }
        return 1;
      }
      return 0;
    }
  }
}
