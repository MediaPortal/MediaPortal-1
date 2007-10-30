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
  class TvServerChecker_SVN: IInstallationPackage
  {
    public string GetDisplayName()
    {
      return "MediaPortal TV-Server Snapshot";
    }

    public bool Download()
    {
      HTTPDownload dlg = new HTTPDownload();
      string url = InstallationProperties.Instance["tve3_downloadurl"];
      string revision = InstallationProperties.Instance["tve3_newestrevision"];
      DialogResult result = dlg.ShowDialog(url, Application.StartupPath + "\\deploy\\tve3_snapshot_"+revision+".zip");
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string msi = Path.GetTempPath() + "\\setup.msi";
      string targetDir = InstallationProperties.Instance["TVServerDir"];
      string revision = InstallationProperties.Instance["tve3_newestrevision"];
      Utils.UnzipFile(Application.StartupPath + "\\Deploy\\tve3_snapshopt_"+revision+".zip", "Setup.msi", msi);
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
      return installOk;
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
      //result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadFile("TvServer"));
      result.needsDownload = true;
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{4B738773-EE07-413D-AFB7-BB0AB04A5488}");
      if (key == null)
      {
        result.state = CheckState.NOT_INSTALLED;
        return result;
      }
      if (InstallationProperties.Instance["tve3_newestrevision"]==null)
      {
        string downloadURL;
        string newestRevision;
        if (!SnapshotLookup.GetSnapshotInfo(SnapshotType.TvServer, out downloadURL, out newestRevision))
        {
          key.Close();
          result.state = CheckState.VERSION_LOOKUP_FAILED;
          return result;
        }
        else
        {
          InstallationProperties.Instance.Set("tv3_downloadurl", downloadURL);
          InstallationProperties.Instance.Set("tv3_newestrevision", newestRevision);
        }
      }
      /*
      string exePath = (string)key.GetValue("Path");
      key.Close();
      StreamReader reader = new StreamReader(exePath + "\\TvService.exe.config");
      for (int i = 0; i < 12; i++)
        reader.ReadLine();
      string revision = reader.ReadLine();
      reader.Close();
      revision = revision.Remove(0, revision.IndexOf("Build") + 6);
      revision = revision.Substring(0, 5);
      if (revision == newestRevision)
        result.state = CheckState.INSTALLED;
      else
        result.state = CheckState.VERSION_MISMATCH;
      */
      result.state = CheckState.VERSION_MISMATCH;
      result.needsDownload = true;
      return result;
    }
  }
}
