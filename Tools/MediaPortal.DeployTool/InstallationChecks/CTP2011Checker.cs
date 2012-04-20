#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class CTP2011Checker : IInstallationPackage
  {
    public static string prg = "CTP2011";

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    public string GetDisplayName()
    {
      return "Framework CTP 2011";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      string cmdLine = "/qn /i \"" + _fileName + "\"";
      cmdLine += " /msicl ACCEPTEFJUNE2011CTPEULA=1";
      Process setup = Process.Start("msiexec.exe", cmdLine);
      try
      {
        if (setup != null)
        {
          setup.WaitForExit();
        }
      }
      catch
      {
        return false;
      }
      return true;
    }

    public bool UnInstall()
    {
      Utils.UninstallMSI("{B9B90C8C-CB6F-4807-AE96-7493E3B29498}");
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo vcRedistFile = new FileInfo(_fileName);
      bool installDirX86 = true;
      bool installDir = true;

      if (vcRedistFile.Exists && vcRedistFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      if (File.Exists("c:\\vc_force_noinstalled"))
      {
        result.state = CheckState.NOT_INSTALLED;
        return result;
      }

      string InstallDirX86 = Environment.GetEnvironmentVariable("programfiles(x86)") + "\\Microsoft Entity Framework June 2011 CTP\\bin\\.NETFramework\\";
      string InstallDir = Environment.GetEnvironmentVariable("ProgramFiles") + "\\Microsoft Entity Framework June 2011 CTP\\bin\\.NETFramework\\";
      string[] dll = new string[2];
      //CTP2011
      dll[0] = "System.Data.Entity.Design.dll";
      dll[1] = "System.Data.Entity.dll";

      for (int i = 0; i < dll.Length; i++)
      {
        if (!File.Exists(InstallDir + dll[i]))
        {
          installDir = false;
        }
      }

      for (int i = 0; i < dll.Length; i++)
      {
        if (!File.Exists(InstallDirX86 + dll[i]))
        {
          installDirX86 = false;
        }
      }

      if (!installDirX86 && !installDir)
      {
        result.state = CheckState.NOT_INSTALLED;
        return result;
      }
      else
      {
        result.state = CheckState.INSTALLED;
        return result;
      }
    }
  }
}