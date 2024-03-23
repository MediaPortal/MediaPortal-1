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
  internal class VcRedistChecker2015 : IInstallationPackage
  {
    public static string prg = "VCRedist2015";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    // Microsoft Visual C++ 2015 Redistributable - x86 14.0.24215.1
    private readonly string x86GUID = "{e2803110-78b3-4664-a479-3611a381656a}";
    // Microsoft Visual C++ 2015 Redistributable - x64 14.0.24215.1
    private readonly string x64GUID = "{d992c12e-cab2-426f-bde3-fb8c53950b0d}";

    public string GetDisplayName()
    {
      return "MS Visual C++ 2015";
    }

    public string GetIconName()
    {
      return "VC2015";
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

      #region Check by uninstall guids
      string[] guids;
      if (Utils.Is64bit())
      {
        guids = new string[]
          {
            //"{282975d8-55fe-4991-bbbb-06a72581ce58}", // Visual C++ 2015-2019 Redistributable x64 (14.24.28127.4)
            "{8bdfe669-9705-4184-9368-db9ce581e0e7}", // Visual C++ 2015-2022 Redistributable x64 (14.36.32532.0)
            "{1CA7421F-A225-4A9C-B320-A36981A2B789}", // Visual C++ 2015-2022 Redistributable x64 (14.38.33130.0)
          };
      }
      else
      {
        guids = new string[]
          {
            //"{e31cb1a4-76b5-46a5-a084-3fa419e82201}", // Visual C++ 2015-2019 Redistributable x86 (14.24.28127.4)
            "{410c0ee1-00bb-41b6-9772-e12c2828b02f}", // Visual C++ 2015-2022 Redistributable x86 (14.36.32532.0)
            "{DF1B52DF-C88E-4DDF-956B-6E7A03327F46}", // Visual C++ 2015-2022 Redistributable x64 (14.38.33130.0)
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