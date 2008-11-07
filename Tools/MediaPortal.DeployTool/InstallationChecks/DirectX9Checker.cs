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
using System.Windows.Forms;
using System.Diagnostics;

namespace MediaPortal.DeployTool.InstallationChecks
{
  class DirectX9Checker : IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "DirectX 9c - August 2008";
    }

    public bool Download()
    {
      const string prg = "DirectX9c";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");
      DialogResult result;
      result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string exe = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("DirectX9c", "FILE");
      Process setup = Process.Start(exe, "/q /t:\"" + Path.GetTempPath() + "\\directx9c\"");
      try
      {
        if (setup != null)
        {
          setup.WaitForExit();
        }
        return true;
      }
      catch
      {
        return false;
      }
    }
    public bool UnInstall()
    {
      //Uninstall not possible. Installer tries an automatic update if older version found
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("DirectX9c", "FILE"));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\DirectX");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        string version = (string)key.GetValue("Version");
        key.Close();
        if (version == "4.09.0000.0904" || version == "4.09.00.0904")
          result.state = CheckState.INSTALLED;
        else
          result.state = CheckState.VERSION_MISMATCH;
      }
      return result;
    }
  }
}