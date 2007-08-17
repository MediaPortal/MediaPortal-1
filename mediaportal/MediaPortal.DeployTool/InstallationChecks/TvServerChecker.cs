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
  class TvServerChecker: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal TV-Server";
    }

    public bool Download()
    {
      HTTPDownload dlg = new HTTPDownload();
      DialogResult result = dlg.ShowDialog(Utils.GetDownloadURL("TvServer"), Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("TvServer"));
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string msi = Path.GetTempPath() + "\\setup.msi";
      string targetDir = InstallationProperties.Instance["TVServerDir"];
      Utils.UnzipFile(Application.StartupPath + "\\Deploy\\" + Utils.GetDownloadFile("TvServer"), "Setup.msi", msi);
      string parameters = "/i \"" + msi + "\" /qb TARGETDIR=\"" + targetDir + "\" /L* \"" + Path.GetTempPath() + "\\tvserverinst.log\"";
      Process setup = Process.Start("msiexec", parameters);
      setup.WaitForExit();
      StreamReader sr = new StreamReader(Path.GetTempPath() + "\\tvserverinst.log");
      bool installOk = false;
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
        File.Delete(Path.GetTempPath() + "\\tvserverinst.log");
        return true;
      }
      else
        return false;
    }
    public bool UnInstall()
    {
      Process setup = Process.Start("msiexec", "/X {4B738773-EE07-413D-AFB7-BB0AB04A5488}");
      setup.WaitForExit();
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("TvServer"));
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{4B738773-EE07-413D-AFB7-BB0AB04A5488}");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        string version = (string)key.GetValue("DisplayVersion");
        key.Close();
        if (version == "1.0.0")
          result.state = CheckState.INSTALLED;
        else
          result.state = CheckState.VERSION_MISMATCH;
      }
      return result;
    }
  }
}
