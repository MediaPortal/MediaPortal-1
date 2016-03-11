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
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class TvPluginChecker : IInstallationPackage
  {
    private readonly string _fileName = Application.StartupPath + "\\deploy\\" +
                                        Utils.GetDownloadString(TvServerChecker.prg, "FILE");

    public string GetDisplayName()
    {
      return "MediaPortal TV-Plugin " + Utils.GetDisplayVersion();
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, TvServerChecker.prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      if (!File.Exists(_fileName))
      {
        return false;
      }

      //NSIS installer need to know if it's a fresh install or an update (chefkoch)
      string UpdateMode = InstallationProperties.Instance["UpdateMode"] == "yes" ? "/UpdateMode" : string.Empty;

      //NSIS installer doesn't want " in parameters (chefkoch)
      //Remember that /D must be the last one         (chefkoch)
      Process setup = Process.Start(_fileName, String.Format("/S /noServer /DeployMode --DeployMode {0}", UpdateMode));

      if (setup != null)
      {
        setup.WaitForExit();
        if (setup.ExitCode == 0)
        {
          string targetDir = InstallationProperties.Instance["MPDir"];
          if (File.Exists(targetDir + "\\reboot"))
          {
            Utils.NotifyReboot(GetDisplayName());
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

      string keyUninstall = Utils.CheckUninstallString("MediaPortal TV Server", true);
      if (keyUninstall != null && File.Exists(keyUninstall))
      {
        Utils.UninstallNSIS(keyUninstall);
      }
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo tvPluginFile = new FileInfo(_fileName);

      result = Utils.CheckNSISUninstallString("Mediaportal TV Server", "MementoSection_SecClient");

      if (tvPluginFile.Exists && tvPluginFile.Length != 0)
      {
        result.needsDownload = false;
      }
      else
      {
        result.needsDownload = true;
      }

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      return result;
    }
  }
}