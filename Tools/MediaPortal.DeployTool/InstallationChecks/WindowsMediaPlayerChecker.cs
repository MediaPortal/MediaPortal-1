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
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;


namespace MediaPortal.DeployTool.InstallationChecks
{
  class WindowsMediaPlayerChecker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "Windows Media Player 11";
    }

    public bool Download()
    {
      const string prg = "WindowsMediaPlayer";
      DialogResult result = Utils.RetryDownloadFile(InstallationProperties.Instance["Wmp11FileName"], prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      try
      {
        Process setup = Process.Start(InstallationProperties.Instance["Wmp11FileName"], "/q");
        if (setup != null)
        {
          setup.WaitForExit();
        }
        return true;
      }
      catch { }
      return false;
    }

    public bool UnInstall()
    {
      //Uninstall not possible. Installer tries an automatic update if older version found
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      const string prg = "WindowsMediaPlayer";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.LocalizeDownloadFile(Utils.GetDownloadString(prg, "FILE"), Utils.GetDownloadString(prg, "TYPE"), prg);
      FileInfo wmpFile = new FileInfo(FileName);
      InstallationProperties.Instance.Set("Wmp11FileName", FileName);

      if (wmpFile.Exists && wmpFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      Version aParamVersion;
      result.state = Utils.CheckFileVersion(Environment.SystemDirectory + "\\wmp.dll", "11.0.0000.0000", out aParamVersion) ? CheckState.INSTALLED : CheckState.NOT_INSTALLED;
      return result;
    }
  }
}
