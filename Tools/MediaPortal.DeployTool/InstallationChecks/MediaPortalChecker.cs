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
  class MediaPortalChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal " + Utils.GetPackageVersion();
    }

    public bool Download()
    {
      string prg = "MediaPortal";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");
      DialogResult result;
      result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string nsis = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("MediaPortal", "FILE");
      string targetDir = InstallationProperties.Instance["MPDir"];
      Process setup = Process.Start(nsis, "/S /DeployMode /D=" + targetDir);
      try
      {
        setup.WaitForExit();
        return true;
      }
      catch 
      {
        return false;
      }
    }
    public bool UnInstall()
    {
      RegistryKey key;
      string RegistryFullPathName;

      // 1.0.x
      key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal");
      if (key != null)
      {
        RegistryFullPathName = key.GetValue("UninstallString").ToString();
        key.Close();
        UninstallNSIS(RegistryFullPathName);
      }

      // 0.2.3.0
      key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal 0.2.3.0");
      if (key != null)
      {
        RegistryFullPathName = key.GetValue("UninstallString").ToString();
        key.Close();
        UninstallNSIS(RegistryFullPathName);   
      }

      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("MediaPortal", "FILE"));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        if (result.needsDownload == false)
          result.state = CheckState.DOWNLOADED;
        else
          result.state = CheckState.NOT_DOWNLOADED;
        return result;
      }

      result.state = CheckState.NOT_INSTALLED;

      RegistryKey keyold = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal 0.2.3.0");
      RegistryKey keynew = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal");
      if (keyold != null)
      {
        string MpPath = (string)keyold.GetValue("UninstallString");
        string version = (string)keyold.GetValue("DisplayVersion");

#if DEBUG
        MessageBox.Show("Verifying MP v0.2.3.0 (MpPath=" + MpPath + ",version=" + version + ")", "Debug information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif

        keyold.Close();
        if (MpPath != null && File.Exists(MpPath))
          result.state = CheckState.VERSION_MISMATCH;
      }
      else if (keynew != null)
      {
        string MpPath = (string)keynew.GetValue("UninstallString");
        string version = (string)keynew.GetValue("DisplayVersion");

#if DEBUG
        MessageBox.Show("Verifying MP v1.0 (MpPath=" + MpPath + ",version=" + version + ")", "Debug information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif

        keynew.Close();
        if (MpPath != null && File.Exists(MpPath))
        {
          if (version == Utils.GetPackageVersion())
            result.state = CheckState.INSTALLED;
          else
            result.state = CheckState.VERSION_MISMATCH;
        }
      }
      return result;
    }

    public void UninstallNSIS(string RegistryFullPathName)
    {
      Process setup;
      string FileName = Path.GetFileName(RegistryFullPathName);
      string Directory = Path.GetDirectoryName(RegistryFullPathName);
      string TempFullPathName = Environment.GetEnvironmentVariable("TEMP") + "\\" + FileName;
      File.Copy(RegistryFullPathName, TempFullPathName);
      setup = Process.Start(TempFullPathName, " /S _?=" + Directory);
      setup.WaitForExit();
      File.Delete(TempFullPathName);
    }
  }
}
