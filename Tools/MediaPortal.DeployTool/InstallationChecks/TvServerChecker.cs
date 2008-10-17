#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.InstallationChecks
{
  class TvServerChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal TV-Server " + Utils.GetPackageVersion();
    }

    public bool Download()
    {
      const string prg = "TvServer";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");
      DialogResult result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string nsis = Application.StartupPath + "\\Deploy\\" + Utils.GetDownloadString("TvServer", "FILE");
      if (!File.Exists(nsis)) return false;
      string targetDir = InstallationProperties.Instance["TVServerDir"];
      //NSIS installed doesn't want " in parameters (chefkoch)
      //Rember that /D must be the last one         (chefkoch)
      string parameters = "/S /noClient /DeployMode /D=" + targetDir;
      Process setup = Process.Start(nsis, parameters);
      if (setup != null)
      {
        setup.WaitForExit();
        if (setup.ExitCode == 0) return true;
      }
      return false;
    }

    public bool UnInstall()
    {
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
#if DEBUG
      MessageBox.Show("TvSever - CheckStatus: " + "start");
#endif
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("TvServer", "FILE"));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
#if DEBUG
        MessageBox.Show("TvSever - CheckStatus: " + "download_only");
#endif
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal TV Server");
      if (key == null)
      {
        result.state = CheckState.NOT_INSTALLED;
      }
      else
      {
#if DEBUG
        MessageBox.Show("TvServer - CheckStatus: " + "registry UninstallString");
#endif
        string TV3Path = (string)key.GetValue("UninstallString", null);
#if DEBUG
        MessageBox.Show("TvServer - CheckStatus: " + "registry MementoSection_SecServer");
#endif
        int serverInstalled = (int)key.GetValue("MementoSection_SecServer", 0);
#if DEBUG
        MessageBox.Show("TvServer - CheckStatus: " + "registry DisplayVersion");
#endif
        string version = (string)key.GetValue("DisplayVersion", null);
        key.Close();
#if DEBUG
        MessageBox.Show("TvServer - CheckStatus: " + "registry close");
#endif
        if (TV3Path == null | !File.Exists(TV3Path))
          result.state = CheckState.NOT_INSTALLED;
        else
        {
#if DEBUG
          MessageBox.Show("TvServer - CheckStatus: " + "GetPackageVersion section");
#endif
          if (serverInstalled == 1)
          {
            result.state = version == Utils.GetPackageVersion() ? CheckState.INSTALLED : CheckState.VERSION_MISMATCH;
          }
          else
            result.state = CheckState.NOT_INSTALLED;
        }
      }
      return result;
    }
  }
}