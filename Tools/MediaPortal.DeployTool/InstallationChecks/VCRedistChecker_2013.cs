#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
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
  internal class VcRedistChecker2013 : IInstallationPackage
  {
    public static string prg = "VCRedist2013";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    // Microsoft Visual C++ 2013 Redistributable - x86 12.0.40649.5
    private readonly string x86GUID = "{35b83883-40fa-423c-ae73-2aff7e1ea820}";
    // Microsoft Visual C++ 2013 Redistributable - x64 12.0.40649.5
    private readonly string x64GUID = "{5d0723d3-cff7-4e07-8d0b-ada737deb5e6}";

    public string GetDisplayName()
    {
      return "MS Visual C++ 2013";
    }

    public string GetIconName()
    {
      return "VC2013";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      int exitCode = Utils.RunCommandWait(_fileName, "/Q");
      // Return codes:
      // 0               = success, no reboot required
      // 3010            = success, reboot required
      // any other value = failure

      if (exitCode == 3010 || File.Exists("c:\\deploy_force_reboot"))
      {
        Utils.NotifyReboot(GetDisplayName());
      }
      return exitCode == 0;
    }

    public bool UnInstall()
    {
      if (Utils.Is64bit())
      {
        string keySection = Utils.CheckUninstallString(x64GUID, "DisplayName");
        if (!string.IsNullOrEmpty(keySection))
        {
          Utils.UninstallMSI(x64GUID);
        }
      }
      else
      {
        string keySection = Utils.CheckUninstallString(x86GUID, "DisplayName");
        if (!string.IsNullOrEmpty(keySection))
        {
          Utils.UninstallMSI(x86GUID);
        }
      }
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo vcRedistFile = new FileInfo(_fileName);

      if (vcRedistFile.Exists && vcRedistFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      if (File.Exists("c:\\vc_force_noinstalled"))
      {
        result.state = CheckState.NOT_INSTALLED;
        return result;
      }

      if (Utils.Is64bit())
      {
        string keySection = Utils.CheckUninstallString(x64GUID, "DisplayName");
        if (!string.IsNullOrEmpty(keySection))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
      }
      else
      {
        string keySection = Utils.CheckUninstallString(x86GUID, "DisplayName");
        if (!string.IsNullOrEmpty(keySection))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
      }

      string InstallDir = Environment.GetEnvironmentVariable("SystemRoot") + "\\system32\\";
      string[] dll = new string[3];
      //CRT
      dll[0] = "msvcp120.dll";
      //MFC
      dll[1] = "mfc120.dll";
      dll[2] = "mfc120u.dll";

      for (int i = 0; i < dll.Length; i++)
      {
        if (!File.Exists(InstallDir + dll[i]))
        {
          result.state = CheckState.NOT_INSTALLED;
          return result;
        }
      }

      result.state = CheckState.INSTALLED;
      return result;
    }
  }
}