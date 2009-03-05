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
  class MediaPortalChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal " + Utils.GetPackageVersion(true);
    }

    public bool Download()
    {
      const string prg = "MediaPortal";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");
      DialogResult result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      string nsis = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("MediaPortal", "FILE");
      if (!File.Exists(nsis)) return false;
      string targetDir = InstallationProperties.Instance["MPDir"];
      //NSIS installed doesn't want " in parameters (chefkoch)
      //Rember that /D must be the last one         (chefkoch)
      Process setup = Process.Start(nsis, "/S /DeployMode /D=" + targetDir);
      if (setup != null)
      {
        setup.WaitForExit();
        if (setup.ExitCode == 0) return true;
      }
      return false;
    }

    public bool UnInstall()
    {
      string[] UninstKeys = {"MediaPortal",             // 1.x
                             "MediaPortal 0.2.3.0"};    // 0.2.3.0

      foreach (string UnistKey in UninstKeys)
      {
        string keyUninstall = Utils.CheckUninstallString(UnistKey, true);
        if (keyUninstall != null && File.Exists(keyUninstall))
        {
          Utils.UninstallNSIS(keyUninstall);
        }
      }
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      string fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("MediaPortal", "FILE");
      FileInfo mpFile = new FileInfo(fileName);

      if (mpFile.Exists && mpFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      result.state = CheckState.NOT_INSTALLED;

      string[] UninstKeys = {"MediaPortal",             // 1.x
                             "MediaPortal 0.2.3.0"};    // 0.2.3.0

      foreach (string UnistKey in UninstKeys)
      {
        RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + UnistKey);

        if (key != null)
        {
          string MpPath = (string)key.GetValue("UninstallString");
          string MpVer = (string)key.GetValue("DisplayVersion");
          key.Close();

#if DEBUG
          MessageBox.Show("Verifying tree " + UnistKey + " (MpPath=" + MpPath + ",version=" + MpVer + ")", "Debug information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif

          if (MpPath != null && File.Exists(MpPath))
          {
            result.state = MpVer == Utils.GetPackageVersion(true) ? CheckState.INSTALLED : CheckState.VERSION_MISMATCH;
          }
        }
      }
      return result;
    }
  }
}