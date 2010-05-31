#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
  class TvServerChecker : IInstallationPackage
  {
    public static string prg = "TvServer";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    public string GetDisplayName()
    {
      return "MediaPortal TV-Server " + Utils.GetDisplayVersion();
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      if (!File.Exists(_fileName)) return false;
      string targetDir = InstallationProperties.Instance["TVServerDir"];

      //NSIS installer need to know if it's a fresh install or an update (chefkoch)
      string UpdateMode = InstallationProperties.Instance["UpdateMode"] == "yes" ? "/UpdateMode" : string.Empty;

      //NSIS installer need to know SQL type and password to start SetupTv accordly
      string dbtype = InstallationProperties.Instance["DBMSType"];
      string sqlparam = string.Empty;
      string pwdparam = string.Empty;
      if (!String.IsNullOrEmpty(dbtype))
      {
        sqlparam = dbtype == "msSQL2005" ? "/DeploySql:sqlserver" : "/DeploySql:mysql";
        pwdparam = "/DeployPwd:" + InstallationProperties.Instance["DBMSPassword"];
      }

      //NSIS installer doesn't want " in parameters (chefkoch)
      //Rember that /D must be the last one         (chefkoch)
      Process setup = Process.Start(_fileName, String.Format("/S /noClient /DeployMode {0} {1} {2} /D={3}", UpdateMode, sqlparam, pwdparam, targetDir));

      if (setup == null)
      {
        return false;
      }
      setup.WaitForExit();
      return setup.ExitCode == 0;
    }

    public bool UnInstall()
    {
      if (InstallationProperties.Instance["UpdateMode"] == "yes")
      {
        return true;
      }

      string keyUninstall = Utils.CheckUninstallString("MediaPortal TV Server", true);
      if (keyUninstall != null && File.Exists(keyUninstall))
      {
        Utils.UninstallNSIS(keyUninstall);
      }
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo tvServerFile = new FileInfo(_fileName);

      if (tvServerFile.Exists && tvServerFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      result = Utils.CheckNSISUninstallString("MediaPortal TV Server", "MementoSection_SecServer");
      return result;
    }
  }
}