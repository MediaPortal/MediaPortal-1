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

        // MS Visual C++ 2008 SP1 + ATL Update (9.0.30729.4148) x86
        keySection = Utils.CheckUninstallString("{1F1C2DFC-2D24-3E06-BCB8-725134ADF989}", "DisplayName");
        if (!string.IsNullOrEmpty(keySection))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
      }

      string ManifestDir = Environment.GetEnvironmentVariable("SystemRoot") + "\\winsxs\\Manifests\\";
      if (Utils.Is64bit())
      {
        // Manifests for Windows10/Windows11
        // MS Visual C++ 2008 SP1 + MFC Security Update (9.0.30729.9635)
        const string ManifestCRT_Win10 = "amd64_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.9635_none_08e2c157a83ed5da.manifest";
        if (File.Exists(ManifestDir + ManifestCRT_Win10))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
      }
      else
      {
        // Manifests for Windows10/Windows11
        // MS Visual C++ 2008 SP1 + MFC Security Update (9.0.30729.9635)
        const string ManifestCRT_Win10 = "x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.9635_none_508ff82ebcbafee0.manifest";
        // Manifests for Windows7/Windows10
        // MS Visual C++ 2008 SP1 + MFC Security Update (9.0.30729.6161)
        const string ManifestCRT_Win7 = "x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.6161_none_50934f2ebcb7eb57.manifest";
        const string ManifestMFC_Win7 = "x86_microsoft.vc90.mfc_1fc8b3b9a1e18e3b_9.0.30729.6161_none_4bf7e3e2bf9ada4c.manifest";
        const string ManifestATL_Win7 = "x86_microsoft.vc90.atl_1fc8b3b9a1e18e3b_9.0.30729.6161_none_51cd0a7abbe4e19b.manifest";
        // Manifests from MS11-025
        const string ManifestCRT_7_10 = "x86_Microsoft.VC90.CRT_1fc8b3b9a1e18e3b_9.0.30729.6161_x-ww_31a54e43.manifest";
        const string ManifestMFC_7_10 = "x86_Microsoft.VC90.MFC_1fc8b3b9a1e18e3b_9.0.30729.6161_x-ww_028bc148.manifest";
        const string ManifestATL_7_10 = "x86_Microsoft.VC90.ATL_1fc8b3b9a1e18e3b_9.0.30729.6161_x-ww_92453bb7.manifest";
        // Manifests for Vista/2008
        // MS Visual C++ 2008 SP1 + ATL Update (9.0.30729.4148)
        const string ManifestCRT_Vista = "x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.4148_none_5090ab56bcba71c2.manifest";
        const string ManifestMFC_Vista = "x86_microsoft.vc90.mfc_1fc8b3b9a1e18e3b_9.0.30729.4148_none_4bf5400abf9d60b7.manifest";
        const string ManifestATL_Vista = "x86_microsoft.vc90.atl_1fc8b3b9a1e18e3b_9.0.30729.4148_none_51ca66a2bbe76806.manifest";
        // MS Visual C++ 2008 SP1 + ATL Update (9.0.30729.4148)
        const string ManifestCRT_XP = "x86_Microsoft.VC90.CRT_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_d495ac4e.manifest";
        const string ManifestMFC_XP = "x86_Microsoft.VC90.MFC_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_a57c1f53.manifest";
        const string ManifestATL_XP = "x86_Microsoft.VC90.ATL_1fc8b3b9a1e18e3b_9.0.30729.4148_x-ww_353599c2.manifest";

        if (File.Exists(ManifestDir + ManifestCRT_Win10))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
        else if (File.Exists(ManifestDir + ManifestCRT_Win7) && File.Exists(ManifestDir + ManifestMFC_Win7) && File.Exists(ManifestDir + ManifestATL_Win7))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
        else if (File.Exists(ManifestDir + ManifestCRT_7_10) && File.Exists(ManifestDir + ManifestMFC_7_10) && File.Exists(ManifestDir + ManifestATL_7_10))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
        else if (File.Exists(ManifestDir + ManifestCRT_Vista) && File.Exists(ManifestDir + ManifestMFC_Vista) && File.Exists(ManifestDir + ManifestATL_Vista))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
        else if (File.Exists(ManifestDir + ManifestCRT_XP) && File.Exists(ManifestDir + ManifestMFC_XP) && File.Exists(ManifestDir + ManifestATL_XP))
        {
          result.state = CheckState.INSTALLED;
          return result;
        }
        else
        {
          result.state = CheckState.NOT_INSTALLED;
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