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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  class TvPluginChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal TV-Plugin " + Utils.GetPackageVersion();
    }

    public bool Download()
    {
      string prg = "TvServer";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");
      DialogResult result;
      result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string exe = Application.StartupPath + "\\Deploy\\" + Utils.GetDownloadString("TvServer", "FILE");
      //NSIS installed doesn't want " in parameters (chefkoch)
      //Rember that /D must be the last one         (chefkoch)
      string parameters = "/S /noServer /DeployMode";
      Process setup = Process.Start(exe, parameters);
      try
      {
        setup.WaitForExit();
      }
      catch { }
      return true;
    }
    public bool UnInstall()
    {
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal TV Server");
      string exe = (string)key.GetValue("UninstallString");
      key.Close();
      Process setup = Process.Start(exe, "/S /RemoveAll");
      try
      {
        setup.WaitForExit();
      }
      catch { }
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
#if DEBUG
      MessageBox.Show("TvPlugin - CheckStatus: " + "start");
#endif
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("TvServer", "FILE"));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
#if DEBUG
        MessageBox.Show("TvPlugin - CheckStatus: " + "download_only");
#endif
        if (result.needsDownload == false)
          result.state = CheckState.DOWNLOADED;
        else
          result.state = CheckState.NOT_DOWNLOADED;
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
            if (version == Utils.GetPackageVersion())
              result.state = CheckState.INSTALLED;
            else
              result.state = CheckState.VERSION_MISMATCH;
          }
          else
            result.state = CheckState.NOT_INSTALLED;
        }
      }
      return result;
    }
  }
}
