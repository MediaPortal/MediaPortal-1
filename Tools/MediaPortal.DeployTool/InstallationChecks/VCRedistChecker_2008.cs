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
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class VCRedistChecker2008 : IInstallationPackage
  {
    public static string prg = "VCRedist2008";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    // Microsoft Visual C++ 2008 SP1 + MFC Security Update (9.0.30729.6161) x86
    private readonly string x86GUID = "{9BE518E6-ECC6-35A9-88E4-87755C07200F}";
    // Microsoft Visual C++ 2008 SP1 + MFC Security Update (9.0.30729.6161) x64
    private readonly string x64GUID = "{5FCE6D76-F5DC-37AB-B2B8-22AB8CEDB1D4}";

    public string GetDisplayName()
    {
      return "MS Visual C++ 2008 SP1\r\n(ATL | MFC Security) Update";
    }

    public string GetIconName()
    {
      return "VC2008SP1";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      Process setup = Process.Start(_fileName, "/Q");
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
      string[] dll = new string[7];
      //CRT
      dll[0] = "msvcp90.dll";
      dll[1] = "msvcr90.dll";
      //MFC
      dll[2] = "mfc90.dll";
      dll[3] = "mfc90u.dll";
      dll[4] = "mfcm90.dll";
      dll[5] = "mfcm90u.dll";
      //ATL
      dll[6] = "atl90.dll";

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