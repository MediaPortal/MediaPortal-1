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
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class VcRedistChecker2022 : IInstallationPackage
  {
    public static string prg = "VCRedist2022";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    // Microsoft Visual C++ 2015-2022 Redistributable x86 (14.44.35208.0)
    private readonly string x86GUID = "{e90abaf0-d749-437b-ba99-cda1c84b6754}";
    // Microsoft Visual C++ 2015-2022 Redistributable x64 (14.44.35208.0)
    private readonly string x64GUID = "{9387bec2-2f2b-48d1-a0ce-692c5df7042d}";

    public string GetDisplayName()
    {
      return "MS Visual C++ 2015-2022";
    }

    public string GetIconName()
    {
      return "VC2022";
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

      #region Check by uninstall guids
      string[] guids;
      if (Utils.Is64bit())
      {
        guids = new string[]
          {
            "{9387bec2-2f2b-48d1-a0ce-692c5df7042d}", // Visual C++ 2015-2022 Redistributable x64 (14.44.35208.0)
          };
      }
      else
      {
        guids = new string[]
          {
            "{e90abaf0-d749-437b-ba99-cda1c84b6754}", // Visual C++ 2015-2022 Redistributable x86 (14.44.35208.0)
          };

      }

      for (int i = 0; i < guids.Length; i++)
      {
        string keySection = Utils.CheckUninstallString(guids[i], "DisplayName");
        if (!string.IsNullOrEmpty(keySection))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
      }
      #endregion

      #region Check by system files
      string InstallDir = Environment.GetEnvironmentVariable("SystemRoot") + "\\system32\\";
      string[] dll = new string[]
        {
          //CRT
          "msvcp140.dll",
          "msvcp140_1.dll",
          //MFC
          "mfc140.dll",
          "mfc140u.dll",
          "mfcm140.dll",
          "mfcm140u.dll",
          //RUNTIME
          "vcruntime140.dll",
          "vcruntime140_1.dll"  //x64 only
        };

      for (int i = 0; i < dll.Length; i++)
      {
        //Skip x64 only files
        if (i + 1 == dll.Length && !Utils.Is64bit())
          break;

        if (!File.Exists(InstallDir + dll[i]))
        {
          result.state = CheckState.NOT_INSTALLED;
          return result;
        }
      }
      #endregion

      result.state = CheckState.INSTALLED;
      return result;
    }
  }
}