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
  class MediaPortalChecker_SVN: IInstallationPackage
  {
    private string newestRevision = "";
    private string downloadURL = "";

    public string GetDisplayName()
    {
      return "MediaPortal Snapshot";
    }

    public bool Download()
    {
      HTTPDownload dlg = new HTTPDownload();

      File.Delete(Application.StartupPath + "\\deploy\\MP_Snapshot.exe");
      DialogResult result = dlg.ShowDialog(downloadURL, Application.StartupPath + "\\deploy\\MP_Snapshot.exe");
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string nsis = Application.StartupPath + "\\deploy\\MP_Snapshot.exe";
      string targetDir = InstallationProperties.Instance["MPDir"];
      Process setup = Process.Start(nsis, "/S /D=" + targetDir);
      setup.WaitForExit();
      return (setup.ExitCode==0);
    }
    public bool UnInstall()
    {
      return true;
    }
    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      result.state = CheckState.INSTALLED;
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal 0.2.3.0");
      if (key == null)
      {
        key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MediaPortal 0.2.3.0 RC3");
        if (key == null)
        {
          result.state = CheckState.NOT_INSTALLED;
          return result;
        }
      }
      if (newestRevision == "")
      {
        if (!SnapshotLookup.GetSnapshotInfo(SnapshotType.MediaPortal, out downloadURL, out newestRevision))
        {
          key.Close();
          downloadURL = "";
          newestRevision = "";
          result.state = CheckState.VERSION_LOOKUP_FAILED;
          return result;
        }
      }
      string exePath=(string)key.GetValue("Path");
      key.Close();
      StreamReader reader=new StreamReader(exePath+"\\MediaPortal.exe.config");
      for (int i=0;i<12;i++)
        reader.ReadLine();
      string revision=reader.ReadLine();
      reader.Close();
      revision = revision.Remove(0, revision.IndexOf("Build")+6);
      revision = revision.Substring(0, 5);
      if (revision==newestRevision)
        result.state = CheckState.INSTALLED;
      else
        result.state = CheckState.VERSION_MISMATCH;
      return result;
    }
  }
}
