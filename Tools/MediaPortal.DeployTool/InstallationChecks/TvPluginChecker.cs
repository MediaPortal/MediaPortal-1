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
  class TvPluginServerChecker: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal TV-Plugin";
    }

    public bool Download()
    {
      HTTPDownload dlg = new HTTPDownload();
      DialogResult result = dlg.ShowDialog(Utils.GetDownloadURL("TvServer"), Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("TvServer"));
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string msi = Path.GetTempPath() + "\\SetupPlugin.msi";
      Utils.UnzipFile(Application.StartupPath + "\\Deploy\\" + Utils.GetDownloadFile("TvServer"), "SetupPlugin.msi", msi);
      string parameters = "/i \"" + msi + "\" /qb /L* \"" + Path.GetTempPath() + "\\tvplugininst.log\"";
      Process setup = Process.Start("msiexec", parameters);
      setup.WaitForExit();
      StreamReader sr = new StreamReader(Path.GetTempPath() + "\\tvplugininst.log");
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
      return installOk;
    }
    public bool UnInstall()
    {
      Process setup = Process.Start("msiexec", "/X {F7444E89-5BC0-497E-9650-E50539860DE0}");
      setup.WaitForExit();
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("TvServer"));
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{F7444E89-5BC0-497E-9650-E50539860DE0}");
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
