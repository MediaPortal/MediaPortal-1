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
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;


namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class WindowsMediaPlayerChecker : IInstallationPackage
  {
    public static string prg = "WindowsMediaPlayer";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" +
                                        Utils.LocalizeDownloadFile(Utils.GetDownloadString(prg, "FILE"),
                                                                   Utils.GetDownloadString(prg, "TYPE"), prg);

    public string GetDisplayName()
    {
      return "Windows Media Player 11";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      Process setup = Process.Start(_fileName, "/q");
      if (setup != null)
      {
        setup.WaitForExit();
        // Return codes:
        //  0               = success, no reboot required
        //  3010            = success, reboot required
        //  any other value = failure

        if (setup.ExitCode == 3010 || File.Exists("c:\\deploy_force_reboot"))
        {
          Utils.NotifyReboot(GetDisplayName());
        }

        if (setup.ExitCode == 0)
        {
          return true;
        }
      }
      return false;
    }

    public bool UnInstall()
    {
      //Uninstall not possible. Installer tries an automatic update if older version found
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo wmpFile = new FileInfo(_fileName);

      if (wmpFile.Exists && wmpFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      Version aParamVersion;
      result.state = Utils.CheckFileVersion(Environment.SystemDirectory + "\\wmp.dll", "11.0.0000.0000",
                                            out aParamVersion)
                       ? CheckState.INSTALLED
                       : CheckState.NOT_INSTALLED;
      return result;
    }
  }
}