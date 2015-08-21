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
using System.Windows.Forms;
using System.Diagnostics;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class DirectX9Checker : IInstallationPackage
  {
    //
    // From 1.2.0 Beta  and on     we need directx_jun2010_redist.exe
    // For  1.2.0 Alpha and before we need directx_mar2009_redist.exe
    // 
    public static string prg = "DirectXRedist2010";

    private string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    public string GetDisplayName()
    {
      return "DirectX 9c - June 2010";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      // Extract package
      Process setup = Process.Start(_fileName, "/q /t:\"" + Path.GetTempPath() + "\\directx9c\"");
      if (setup != null)
      {
        setup.WaitForExit();
      }

      // Install package
      string exe = Path.GetTempPath() + "\\directx9c\\DXSetup.exe";

      setup = Process.Start(exe, "/silent");
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
        RegistryKey key = null;
        key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\DirectX");
        if (key == null)
        {
          try
          {
            key = Utils.OpenSubKey(Registry.LocalMachine, "SOFTWARE\\Microsoft\\DirectX", false,
                Utils.eRegWow64Options.KEY_WOW64_32KEY);
          }
          catch
          {
            // Parent key not open, exception found at opening (probably related to
            // security permissions requested)
          }
        }
        using (key)
        {
          if (key == null)
          {
            result.state = CheckState.NOT_INSTALLED;
          }
          else
          {
            key.Close();
            string[] DllList = {
                                 @"\System32\D3DX9_43.dll",
                                 @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.Direct3D.dll",
                                 @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.DirectDraw.dll",
                                 @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.DirectInput.dll"
                                 ,
                                 @"\microsoft.net\DirectX for Managed Code\1.0.2902.0\Microsoft.DirectX.dll",
                                 @"\microsoft.net\DirectX for Managed Code\1.0.2911.0\Microsoft.DirectX.Direct3DX.dll"
                               };
            string WinDir = Environment.GetEnvironmentVariable("WINDIR");
            foreach (string DllFile in DllList)
            {
              if (!File.Exists(WinDir + "\\" + DllFile))
              {
                // Changed from ".VERSION_MISMATCH" to avoid complaining about "removal of newer DirectX"
                result.state = CheckState.NOT_INSTALLED;
                return result;
              }
            }
            result.state = CheckState.INSTALLED;
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("Failed to check the DirectX installation status", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
      return result;
    }
  }
}