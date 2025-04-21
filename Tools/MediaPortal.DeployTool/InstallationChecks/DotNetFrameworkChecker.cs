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
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class DotNetFrameworkChecker : IInstallationPackage
  {
    public static string prg = "DotNetFramework";

    private string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    private static bool dotNet35 = false;
    private static bool dotNet40 = false;

    public string GetDisplayName()
    {
      return ".NET Framework 4";
    }

    public string GetIconName()
    {
      return "DotNetFramework";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      bool result = false;

      // .Net 3.5
      if (!dotNet35)
      {
        int exitCode = Utils.RunCommandWait("DISM.EXE", "/Online /Enable-Feature /FeatureName:NetFx3 /All /Quiet /NoRestart");
        // Return codes:
        // 0               = success, no reboot required
        // 3010            = success, reboot required
        // any other value = failure

        if (exitCode == 3010 || File.Exists("c:\\deploy_force_reboot"))
        {
          Utils.NotifyReboot(GetDisplayName());
        }
        result = result || (exitCode == 0);
      }

      // .Net 4.0
      if (!dotNet40)
      {
        int exitCode = Utils.RunCommandWait(_fileName, "/q /norestart");
        // Return codes:
        // 0               = success, no reboot required
        // 1638            = success, another version of this product is already installed.
        // 3010            = success, reboot required
        // any other value = failure

        if (exitCode == 3010 || File.Exists("c:\\deploy_force_reboot"))
        {
          Utils.NotifyReboot(GetDisplayName());
        }
        result = result || (exitCode == 0 || exitCode == 1638);
      }
      return result;
    }

    public bool UnInstall()
    {
      //Uninstall not possible. Installer tries an automatic update if older version found
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result = new CheckResult();
      result.needsDownload = true;
      FileInfo dxFile = new FileInfo(_fileName);

      if (dxFile.Exists && dxFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      try
      {
        RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v3.5");
        using (key)
        {
          if (key != null)
          {
            key.Close();
            dotNet35 = true;
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Failed to check the .Net Framework 3.5 installation status", "Error", 
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }

      try
      {
        RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4");
        using (key)
        {
          if (key != null)
          {
            key.Close();
            dotNet40 = true;
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Failed to check the .Net Framework 4 installation status", "Error", 
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
      if (!dotNet40)
      {
        try
        {
          RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4.0");
          using (key)
          {
            if (key != null)
            {
              key.Close();
              dotNet40 = true;
            }
          }
        }
        catch (Exception)
        {
          MessageBox.Show("Failed to check the .Net Framework 4.0 installation status", "Error", 
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
        }
      }

      if (dotNet35 && dotNet40)
      {
        result.state = CheckState.INSTALLED;
      }
      else
      {
        result.state = CheckState.NOT_INSTALLED;
      }
      return result;
    }
  }
}
