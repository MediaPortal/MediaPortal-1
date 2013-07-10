#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using MediaPortal.DeployTool.Sections;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class MediaPortalChecker : IInstallationPackage
  {
    public static string prg = "MediaPortal";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    public string GetDisplayName()
    {
      return "MediaPortal " + Utils.GetDisplayVersion();
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      if (!File.Exists(_fileName))
      {
        return false;
      }

      // If we have a deploy.xml left over from previous installation then attempt to delete it
      var deployXml = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                      @"\Team MediaPortal\MediaPortal\deploy.xml";
      if (File.Exists(deployXml))
      {
        try
        {
          File.Delete(deployXml);
        }
        catch (Exception ex)
        {
          var result = MessageBox.Show(Localizer.GetBestTranslation("MainWindow_AppName"),
                                       Localizer.GetBestTranslation("DeployXmlDelete_Failed"),
                                       MessageBoxButtons.AbortRetryIgnore,
                                       MessageBoxIcon.Error);
          switch (result)
          {
            case DialogResult.Abort:
              Environment.Exit(-2);
              break;
            case DialogResult.Ignore:
              break;
            case DialogResult.Retry:
              return Install();
          }
        }
      }

      //if user has chosen a skin then update deploy.xml so this is picked up by MP
      //if no skin has been chosed (user has selected one click install) then set one
      var chosenSkin = InstallationProperties.Instance.Get("ChosenSkin");
      if (string.IsNullOrEmpty(chosenSkin))
      {
        chosenSkin = "Titan";
      }

      if (InstallationProperties.Instance["UpdateMode"] == "yes")
      {
        if (chosenSkin != "[Existing]")
        {
          Utils.SetDeployXml("skin", "name", chosenSkin);
        }
      }

      string targetDir = InstallationProperties.Instance["MPDir"];

      //NSIS installer need to know if it's a fresh install or an update (chefkoch)
      string UpdateMode = InstallationProperties.Instance["UpdateMode"] == "yes" ? "/UpdateMode" : string.Empty;

      Process setup = null;

      if (UpgradeDlg.reInstallForce)
      {
        setup = Process.Start(_fileName,
                              String.Format("/S /DeployMode --DeployMode /UpdateMode"));
      }
      else
      {
        //NSIS installer doesn't want " in parameters (chefkoch)
        //Remember that /D must be the last one         (chefkoch)
        setup = Process.Start(_fileName,
                                      String.Format("/S /DeployMode --DeployMode {0} /D={1}", UpdateMode, targetDir));
      }
      if (setup != null)
      {
        setup.WaitForExit();
        if (setup.ExitCode == 0)
        {
          if (File.Exists(targetDir + "\\reboot"))
          {
            Utils.NotifyReboot(GetDisplayName());
          }

          // installer backups existing folder so need to write deploy.xml after installation 
          // else it will get backed up
          if (InstallationProperties.Instance["UpdateMode"] != "yes")
          {
            if (chosenSkin != "[Existing]")
            {
              Utils.SetDeployXml("skin", "name", chosenSkin);
            }
          }

          return true;
        }
      }
      return false;
    }

    public bool UnInstall()
    {
      if (InstallationProperties.Instance["UpdateMode"] == "yes")
      {
        return true;
      }

      string[] UninstKeys = {
                              "MediaPortal", // 1.x
                              "MediaPortal 0.2.3.0"
                            }; // 0.2.3.0

      foreach (string UnistKey in UninstKeys)
      {
        string keyUninstall = Utils.CheckUninstallString(UnistKey, true);
        if (keyUninstall != null && File.Exists(keyUninstall))
        {
          Utils.UninstallNSIS(keyUninstall);
        }
      }
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo mpFile = new FileInfo(_fileName);

      if (mpFile.Exists && mpFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      result.state = CheckState.NOT_INSTALLED;

      string[] UninstKeys = {
                              "MediaPortal",        // 1.x
                              "MediaPortal 0.2.3.0" // 0.2.3.0
                            };

      foreach (string UnistKey in UninstKeys)
      {
        string regkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + UnistKey;
        string MpPath = Utils.PathFromRegistry(regkey);
        Version MpVer = Utils.VersionFromRegistry(regkey);

#if DEBUG
        MessageBox.Show("Verifying tree " + UnistKey + " (MpPath=" + MpPath + ",version=" + MpVer + ")",
                        "Debug information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif

        if (MpPath != null && File.Exists(MpPath))
        {
          if (UpgradeDlg.reInstallForce)
          {
            result.state = Utils.IsCurrentPackageUpdatabled(MpVer) ? CheckState.VERSION_MISMATCH : CheckState.INSTALLED;
          }
          else if (UpgradeDlg.freshForce)
          {
            result.state = CheckState.VERSION_MISMATCH;
          }
          else
          {
            result.state = Utils.IsPackageUpdatabled(MpVer) ? CheckState.VERSION_MISMATCH : CheckState.INSTALLED;
          }
        }
      }
      return result;
    }
  }
}