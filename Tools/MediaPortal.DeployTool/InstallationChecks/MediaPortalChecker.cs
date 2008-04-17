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
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile(prg);
      DialogResult result;
      result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string nsis = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("MediaPortal");
      string targetDir = InstallationProperties.Instance["MPDir"];
      Process setup = Process.Start(nsis, "/S /D=" + targetDir);
      try
      {
        setup.WaitForExit();
      }
      catch { }
      return true;
    }
    public bool UnInstall()
    {
      RegistryKey key;
      Process setup;

      // 1.0.x
      key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal");
      if (key != null)
      {
        key.Close();
        setup = Process.Start((string)key.GetValue("UninstallString"));
        setup.WaitForExit();
      }

      // 0.2.3.0
      key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal 0.2.3.0");
      if (key != null)
      {
        string RegistryFullPathName = key.GetValue("UninstallString").ToString();
        string FileName = Path.GetFileName(RegistryFullPathName);
        string Directory = Path.GetDirectoryName(RegistryFullPathName);
        string TempFullPathName = Environment.GetEnvironmentVariable("TEMP") + "\\" + FileName;
        key.Close();
        File.Copy(RegistryFullPathName, TempFullPathName);
        setup = Process.Start(TempFullPathName, " /S _?=" + Directory);
        setup.WaitForExit();
        File.Delete(TempFullPathName);
      }

      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("MediaPortal"));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        if (result.needsDownload == false)
          result.state = CheckState.DOWNLOADED;
        else
          result.state = CheckState.NOT_DOWNLOADED;
        return result;
      }
      RegistryKey keyold = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal 0.2.3.0");
      RegistryKey keynew = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal");

      if (keyold != null)
      {
        string MpPath = (string)keyold.GetValue("UninstallString");
        string version = (string)keyold.GetValue("DisplayVersion");
        keyold.Close();
        if (MpPath == null | !File.Exists(MpPath))
          result.state = CheckState.NOT_INSTALLED;
        else
          result.state = CheckState.VERSION_MISMATCH;
      }
      else if (keynew != null)
      {
        string MpPath = (string)keynew.GetValue("UninstallString");
        string version = (string)keynew.GetValue("DisplayVersion");
        keynew.Close();
        if (MpPath == null | !File.Exists(MpPath))
        {
          result.state = CheckState.NOT_INSTALLED;
        }
        else
        {
          if (version == Utils.GetPackageVersion())
            result.state = CheckState.INSTALLED;
          else
            result.state = CheckState.VERSION_MISMATCH;
        }
      }
      else
        result.state = CheckState.NOT_INSTALLED;
      return result;
    }
  }
}
