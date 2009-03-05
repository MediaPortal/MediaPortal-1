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
  class TvPluginChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal TV-Plugin " + Utils.GetPackageVersion(true);
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
      //NSIS installed doesn't want " in parameters (chefkoch)
      //Rember that /D must be the last one         (chefkoch)
      const string parameters = "/S /noServer /DeployMode";
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
      result.needsDownload = true;
      string filename = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("TvServer", "FILE");
      FileInfo tvPluginFile = new FileInfo(filename);

#if DEBUG
      MessageBox.Show("TvPlugin - CheckStatus: " + "start");
#endif

      if (tvPluginFile.Exists && tvPluginFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
#if DEBUG
        MessageBox.Show("TvPlugin - CheckStatus: " + "download_only");
#endif
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal TV Server");
#if DEBUG
      MessageBox.Show("TvPlugin - CheckStatus: " + "registry open");
#endif
      if (key == null)
      {
        result.state = CheckState.NOT_INSTALLED;
      }
      else
      {
#if DEBUG
        MessageBox.Show("TvPlugin - CheckStatus: " + "registry UninstallString");
#endif
        string TV3Path = (string)key.GetValue("UninstallString", null);
#if DEBUG
        MessageBox.Show("TvPlugin - CheckStatus: " + "registry MementoSection_SecClient");
#endif
        int clientInstalled = (int)key.GetValue("MementoSection_SecClient", 0);
#if DEBUG
        MessageBox.Show("TvPlugin - CheckStatus: " + "registry DisplayVersion");
#endif
        string version = (string)key.GetValue("DisplayVersion", null);
        key.Close();
#if DEBUG
        MessageBox.Show("TvPlugin - CheckStatus: " + "registry close");
#endif
        if (TV3Path == null | !File.Exists(TV3Path))
          result.state = CheckState.NOT_INSTALLED;
        else
        {
#if DEBUG
          MessageBox.Show("TvPlugin - CheckStatus: " + "GetPackageVersion section");
#endif
          if (clientInstalled == 1)
          {
            result.state = version == Utils.GetPackageVersion(true) ? CheckState.INSTALLED : CheckState.VERSION_MISMATCH;
          }
          else
            result.state = CheckState.NOT_INSTALLED;
        }
      }
      return result;
    }
  }
}