#region Copyright (C) 2005-2025 Team MediaPortal

// Copyright (C) 2005-2025 Team MediaPortal
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
using System.Threading;
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

      // labelHeading.Text = InstallationProperties.Instance["InstallType"] == "download_only"
      //                      ? Localizer.GetBestTranslation("Install_labelHeadingDownload")
      //                      : Localizer.GetBestTranslation("Install_labelHeadingInstall");

      // listView.Columns[0].Text = Localizer.GetBestTranslation("Install_colApplication");
      // listView.Columns[1].Text = Localizer.GetBestTranslation("Install_colState");
      // listView.Columns[2].Text = Localizer.GetBestTranslation("Install_colAction");
      labelSectionHeader.Text = "";
      flpApplication.Update();
    }

    public override DeployDialog GetNextDialog()
    {
      progressInstall.Minimum = 0;
      progressInstall.Maximum = flpApplication.Controls.Count;
      progressInstall.Value = 0;
      progressInstall.Visible = true;
      progressInstall.Update();

      Application.DoEvents();
      Thread.Sleep(1);

      foreach (Control item in flpApplication.Controls)
      {
        IInstallationPackage package = (IInstallationPackage)((ApplicationCtrl)item).Tag;
        int action = PerformPackageAction(package, (ApplicationCtrl)item);
        ((ApplicationCtrl)item).InAction = false;

        item.Update();

        Application.DoEvents();
        Thread.Sleep(1);

        if (action == 2)
        {
          ((ApplicationCtrl)item).StatusName = CheckState.FAILED.ToString();
          break;
        }

        if (action == 1)
        {
          ((ApplicationCtrl)item).State = Localizer.GetBestTranslation("Install_stateInstalled");
          ((ApplicationCtrl)item).Action = Localizer.GetBestTranslation("Install_actionNothing");
          ((ApplicationCtrl)item).StatusName = CheckState.COMPLETE.ToString();
        }

        item.Update();

        progressInstall.Value++;
        progressInstall.Update();

        Application.DoEvents();
        Thread.Sleep(1);
      }
      // PopulateListView();
      progressInstall.Visible = false;
      return DialogFlowHandler.Instance.GetDialogInstance(DialogType.Finished);
    }

    public override bool SettingsValid()
    {
      return true;
    }

    public override void SetProperties()
    {
      InstallationProperties.Instance.Set("finished", "yes");
      InstallationProperties.Instance.Save();
    }

    #endregion

    private void AddPackageToListView(IInstallationPackage package, bool skipIfInstalled = false)
    {
      ApplicationCtrl item = new ApplicationCtrl
      {
        Name = package.GetDisplayName(),
        IconName = package.GetIconName(),
        Tag = package
      };

      CheckResult result = package.CheckStatus();
      if (skipIfInstalled && (result.state == CheckState.INSTALLED || result.state == CheckState.REMOVED))
      {
        return;
      }

      item.StatusName = result.state.ToString();
      switch (result.state)
      {
        case CheckState.INSTALLED:
          item.State = Localizer.GetBestTranslation("Install_stateInstalled");
          item.Action = Localizer.GetBestTranslation("Install_actionNothing");
          break;
        case CheckState.NOT_INSTALLED:
          item.State = Localizer.GetBestTranslation("Install_stateNotInstalled");
          if (result.needsDownload)
            item.Action = Localizer.GetBestTranslation("Install_actionDownloadInstall");
          else
            item.Action = Localizer.GetBestTranslation("Install_actionInstall");
          break;
        case CheckState.CONFIGURED:
          item.State = Localizer.GetBestTranslation("Install_stateConfigured");
          item.Action = Localizer.GetBestTranslation("Install_actionNothing");
          break;
        case CheckState.NOT_CONFIGURED:
          item.State = Localizer.GetBestTranslation("Install_stateNotConfigured");
          item.Action = Localizer.GetBestTranslation("Install_actionConfigure");
          break;
        case CheckState.REMOVED:
          item.State = Localizer.GetBestTranslation("Install_stateRemoved");
          item.Action = Localizer.GetBestTranslation("Install_actionNothing");
          break;
        case CheckState.NOT_REMOVED:
          item.State = Localizer.GetBestTranslation("Install_stateUninstall");
          item.Action = Localizer.GetBestTranslation("Install_actionRemove");
          break;
        case CheckState.DOWNLOADED:
          item.State = Localizer.GetBestTranslation("Install_stateDownloaded");
          item.Action = Localizer.GetBestTranslation("Install_actionNothing");
          break;
        case CheckState.NOT_DOWNLOADED:
          item.State = Localizer.GetBestTranslation("Install_stateNotDownloaded");
          item.Action = Localizer.GetBestTranslation("Install_actionDownload");
          break;
        case CheckState.VERSION_MISMATCH:
          item.State = Localizer.GetBestTranslation("Install_stateVersionMismatch");
          if (result.needsDownload)
            item.Action = Localizer.GetBestTranslation("Install_actionUninstallDownloadInstall");
          else
          {
            if (InstallationProperties.Instance["UpdateMode"] == "yes")
            {
              item.Action = Localizer.GetBestTranslation("Install_actionUpgradeInstall");
            }
            else
            {
              item.Action = Localizer.GetBestTranslation("Install_actionUninstallInstall");
            }
          }
          break;
        case CheckState.SKIPPED:
          item.State = Localizer.GetBestTranslation("Install_stateSkipped");
          item.Action = Localizer.GetBestTranslation("Install_actionNothing");
          break;
      }
      flpApplication.Controls.Add(item);
    }

    private void PopulateListView()
    {
      flpApplication.Controls.Clear();
      if (!Utils.Is64bit() && InstallationProperties.Instance["InstallType"] != "download_only")
      {
        AddPackageToListView(new OldPackageChecker(), true);
      }
      AddPackageToListView(new DotNetFrameworkChecker(), true);
      AddPackageToListView(new DirectX9Checker());
      if (!Utils.Is64bit())
      {
        AddPackageToListView(new VCRedistChecker2008());
      }
      AddPackageToListView(new VCRedistChecker2010());
      if (Utils.Is64bit())
      {
        AddPackageToListView(new VcRedistChecker2013());
      }
      // AddPackageToListView(new VcRedistChecker2015());
      AddPackageToListView(new VcRedistChecker2022());
      AddPackageToListView(new WindowsMediaPlayerChecker());
switch (InstallationProperties.Instance["InstallType"])
      {
        case "singleseat":
          AddPackageToListView(new MediaPortalChecker());
          if (InstallationProperties.Instance["DBMSType"] == "MSSQL")
          {
            AddPackageToListView(new MSSQLExpressChecker());
          }
          if (InstallationProperties.Instance["DBMSType"] == "MySQL")
          {
            if (OSInfo.OSInfo.Win10OrLater() && Utils.Is64bitOS)
            {
              AddPackageToListView(new MySQLChecker());
            }
            else
            {
              AddPackageToListView(new MySQLCheckerOutdated());
            }
          }
          if (InstallationProperties.Instance["DBMSType"] == "MariaDB")
          {
            AddPackageToListView(new MariaDBChecker());
          }

#if NO_TV_SERVER
#else
          AddPackageToListView(new TvServerChecker());
#endif

          AddPackageToListView(new TvPluginChecker());
          break;

        case "tvserver_master":
          if (InstallationProperties.Instance["DBMSType"] == "MSSQL")
          {
            AddPackageToListView(new MSSQLExpressChecker());
          }
          if (InstallationProperties.Instance["DBMSType"] == "MySQL")
          {
            if (OSInfo.OSInfo.Win10OrLater() && Utils.Is64bitOS)
            {
              AddPackageToListView(new MySQLChecker());
            }
            else
            {
              AddPackageToListView(new MySQLCheckerOutdated());
            }
          }
          if (InstallationProperties.Instance["DBMSType"] == "MariaDB")
          {
            AddPackageToListView(new MariaDBChecker());
          }

#if NO_TV_SERVER
#else
          AddPackageToListView(new TvServerChecker());
#endif

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
          // framug: MS SQL is no longer supported
          // AddPackageToListView(new MSSQLExpressChecker());
          if (OSInfo.OSInfo.Win10OrLater() && Utils.Is64bitOS)
          {
            AddPackageToListView(new MySQLChecker());
          }
          else
          {
            AddPackageToListView(new MySQLCheckerOutdated());
          }

#if NO_TV_SERVER
#else
          AddPackageToListView(new TvServerChecker());
#endif

          AddPackageToListView(new TvPluginChecker());
          break;
      }

      if (InstallationProperties.Instance["InstallType"] != "tvserver_master")
      {
        // Extension
        if (InstallationProperties.Instance["ConfigureMediaPortalLAV"] == "1")
        {
          AddPackageToListView(new LAVFilterMPEInstall());
        }
        if (InstallationProperties.Instance["ConfigureMediaPortalTitanExtended"] == "1")
        {
          AddPackageToListView(new TitanExtensionInstall());
        }

        // Skin
        if (InstallationProperties.Instance["ConfigureMediaPortalAresSkin"] == "1")
        {
          AddPackageToListView(new AresSkinExtensionInstall());
        }
      }

      if (InstallationProperties.Instance["InstallType"] != "download_only" &&
          (InstallationProperties.Instance["ConfigureMediaPortalFirewall"] == "1" ||
           InstallationProperties.Instance["ConfigureTVServerFirewall"] == "1"))
      {
        AddPackageToListView(new WindowsFirewallChecker());
      }
    }

    private void RequirementsDlg_ParentChanged(object sender, EventArgs e)
    {
      if (Parent != null)
      {
        PopulateListView();
      }
    }

    private int PerformPackageAction(IInstallationPackage package, ApplicationCtrl item)
    {
      //
      // return 0: nothing to do
      //        1: install successufull
      //        2: install error
      //
      if (package == null)
      {
        return 2;
      }
      CheckResult result = package.CheckStatus();
      if (result.state != CheckState.INSTALLED &&
          result.state != CheckState.REMOVED &&
          result.state != CheckState.DOWNLOADED &&
          result.state != CheckState.SKIPPED)
      {
        item.InAction = true;
        item.StatusName = CheckState.PROGRESS.ToString();
        switch (result.state)
        {
          case CheckState.NOT_INSTALLED:
            if (result.needsDownload)
            {
              item.Action = Localizer.GetBestTranslation("Install_msgDownloading");
              item.Update();
              if (!package.Download())
              {
                Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errInstallFailed"),
                                             package.GetDisplayName()));
                return 2;
              }
            }
            item.Action = Localizer.GetBestTranslation("Install_msgInstalling");
            item.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errInstallFailed"),
                                           package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.NOT_CONFIGURED:
            item.Action = Localizer.GetBestTranslation("Install_msgConfiguring");
            item.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errConfigureFailed"),
                                           package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.NOT_REMOVED:
            item.Action = Localizer.GetBestTranslation("Install_msgUninstalling");
            item.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errRemoveFailed"),
                                           package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.VERSION_MISMATCH:
            item.Action = Localizer.GetBestTranslation("Install_msgUninstalling");
            item.Update();
            if (!package.UnInstall())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errUinstallFailed"),
                                           package.GetDisplayName()));
              return 2;
            }
            if (result.needsDownload)
            {
              item.Action = Localizer.GetBestTranslation("Install_msgDownloading");
              item.Update();
              if (!package.Download())
              {
                Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errDownloadFailed"),
                                             package.GetDisplayName()));
                return 2;
              }
            }
            item.Action = Localizer.GetBestTranslation("Install_msgInstalling");
            item.Update();
            if (!package.Install())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errInstallFailed"),
                                           package.GetDisplayName()));
              return 2;
            }
            break;

          case CheckState.NOT_DOWNLOADED:
            item.Action = Localizer.GetBestTranslation("Install_msgDownloading");
            item.Update();
            if (!package.Download())
            {
              Utils.ErrorDlg(string.Format(Localizer.GetBestTranslation("Install_errDownloadFailed"),
                                           package.GetDisplayName()));
              return 2;
            }
            break;
        }
        return 1;
      }
      item.StatusName = result.state.ToString();
      return 0;
    }
  }
}
