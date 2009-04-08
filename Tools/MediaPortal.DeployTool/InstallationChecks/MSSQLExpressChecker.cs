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
using System.ServiceProcess;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MediaPortal.DeployTool.InstallationChecks
{
  class MSSQLExpressChecker : IInstallationPackage
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    private readonly string arch = Utils.Check64bit() ? "64" : "32";

    private static void PrepareTemplateINI(string iniFile)
    {
      WritePrivateProfileString("Options", "USERNAME", "MediaPortal", iniFile);
      WritePrivateProfileString("Options", "COMPANYNAME", "\"Team MediaPortal\"", iniFile);
      WritePrivateProfileString("Options", "INSTALLSQLDIR", "\"" + InstallationProperties.Instance["DBMSDir"] + "\"", iniFile);
      WritePrivateProfileString("Options", "ADDLOCAL", "ALL", iniFile);
      WritePrivateProfileString("Options", "INSTANCENAME", "SQLEXPRESS", iniFile);
      WritePrivateProfileString("Options", "SQLBROWSERAUTOSTART", "1", iniFile);
      WritePrivateProfileString("Options", "SQLAUTOSTART", "1", iniFile);
      WritePrivateProfileString("Options", "SECURITYMODE", "SQL", iniFile);
      WritePrivateProfileString("Options", "SAPWD", InstallationProperties.Instance["DBMSPassword"], iniFile);
      WritePrivateProfileString("Options", "DISABLENETWORKPROTOCOLS", "0", iniFile);
    }

    private static void FixTcpPort()
    {
      RegistryKey keySql =
        Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] +
                                         "\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL");
      if (keySql == null)
      {
        return;
      }
      string instanceSQL = (string)keySql.GetValue("SQLEXPRESS");
      keySql.Close();

      keySql = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] +
                                         "\\Microsoft\\Microsoft SQL Server\\" + instanceSQL + "\\MSSQLServer\\SuperSocketNetLib\\Tcp\\IPAll", true);
      if (keySql == null)
      {
        return;
      }
      keySql.SetValue("TcpPort", "1433");
      keySql.SetValue("TcpDynamicPorts", string.Empty);
      keySql.Close();
    }

    private static void StartStopService(bool start)
    {
      string[] services = { "MSSQL$SQLEXPRESS", "SQLBrowser" };
      foreach (string service in services)
      {
        ServiceController ctrl = new ServiceController(service);
        try
        {
          if (start)
          {
            ctrl.Start();
          }
          else
          {
            ctrl.Stop();
            ctrl.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(String.Format("SQL - service exception during {0}: {1}", start, ex.Message));
          return;
        }
      }
      return;
    }

    public string GetDisplayName()
    {
      return "MS SQL Express 2005";
    }

    public bool Download()
    {
      string prg = "MSSQLExpress" + arch;
      string FileName = InstallationProperties.Instance["Sql2005FileName"];
      DialogResult result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }
    public bool Install()
    {
      string tmpPath = Path.GetTempPath() + "\\SQLEXPRESS";
      //Extract all files
      Process extract = Process.Start(InstallationProperties.Instance["Sql2005FileName"], "/X:\"" + tmpPath + "\" /Q");
      try
      {
        if (extract != null)
        {
          extract.WaitForExit();
        }
      }
      catch
      {
        return false;
      }
      //Prepare the unattended ini file
      PrepareTemplateINI(tmpPath + "\\template.ini");

      try
      {
        //run the setup
        Process setup = Process.Start(tmpPath + "\\setup.exe", "/wait /settings \"" + tmpPath + "\\template.ini\" /qn");
        if (setup != null)
        {
          setup.WaitForExit();
          if (setup.ExitCode == 0)
          {
            Directory.Delete(tmpPath, true);
            StartStopService(false);
            FixTcpPort();
            StartStopService(true);
            return true;
          }
          return false;
        }
      }
      catch
      {
        return false;
      }
      return false;
    }

    public bool UnInstall()
    {
      Utils.UninstallMSI("{2AFFFDD7-ED85-4A90-8C52-5DA9EBDC9B8F}");
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      string prg = "MSSQLExpress" + arch;
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.LocalizeDownloadFile(Utils.GetDownloadString(prg, "FILE"), Utils.GetDownloadString(prg, "TYPE"), prg);
      InstallationProperties.Instance.Set("Sql2005FileName", FileName);
      FileInfo msSqlFile = new FileInfo(FileName);

      if (msSqlFile.Exists && msSqlFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "Microsoft\\Microsoft SQL Server\\SQLEXPRESS\\MSSQLServer\\CurrentVersion"))
      {
        if (key == null)
          result.state = CheckState.NOT_INSTALLED;
        else
        {
          string version = (string)key.GetValue("CurrentVersion");
          key.Close();
          result.state = version.StartsWith("9.0") ? CheckState.INSTALLED : CheckState.VERSION_MISMATCH;
        }
      }
      return result;
    }
  }
}