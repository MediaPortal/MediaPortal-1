#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
  class MediaPortalChecker: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal";
    }

    public bool Download()
    {
      HTTPDownload dlg = new HTTPDownload();
      DialogResult result = dlg.ShowDialog(Utils.GetDownloadURL("MediaPortal"), Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("MediaPortal"));
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string msi = Path.GetTempPath() + "\\setup.msi";
      string targetDir=InstallationProperties.Instance["MPDir"];
      Utils.UnzipFile(Application.StartupPath + "\\Deploy\\" + Utils.GetDownloadFile("MediaPortal"), "Setup.msi", msi);
      string parameters="/i \""+msi+"\" /qb TARGETDIR=\""+targetDir+"\" /L* \""+Path.GetTempPath()+"\\mpinst.log\"";
      Process setup=Process.Start("msiexec",parameters);
      setup.WaitForExit();
      StreamReader sr = new StreamReader(Path.GetTempPath() + "\\mpinst.log");
      bool installOk=false;
      while (!sr.EndOfStream)
      {
        string line = sr.ReadLine();
        if (line.Contains("Installation completed successfully"))
        {
          installOk = true;
          break;
        }
      }
      sr.Close();
      if (installOk)
      {
        File.Delete(msi);
        File.Delete(Path.GetTempPath() + "\\mpinst.log");
        return true;
      }
      else
        return false;
    }
    public bool UnInstall()
    {
      Process setup = Process.Start("msiexec", "/X {87819CFA-1786-484D-B0DE-10B5FBF2625D}");
      setup.WaitForExit();
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("MediaPortal"));
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{87819CFA-1786-484D-B0DE-10B5FBF2625D}");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        string version = (string)key.GetValue("DisplayVersion");
        key.Close();
        if (version == "0.2.3")
          result.state = CheckState.INSTALLED;
        else
          result.state = CheckState.VERSION_MISMATCH;
      }
      return result;
    }
  }
}
