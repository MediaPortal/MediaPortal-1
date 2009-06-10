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

    private static readonly string arch = Utils.Check64bit() ? "64" : "32";

    public static string prg = "MSSQL2008Express" + arch;

    private readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    private static void PrepareTemplateINI(string iniFile)
    {
      WritePrivateProfileString("SQLSERVER2008", "INSTANCEID", "SQLExpress", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "ACTION", "Install", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "FEATURES", "SQLENGINE,SSMS", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "SAPWD", InstallationProperties.Instance["DBMSPassword"], iniFile);
      WritePrivateProfileString("SQLSERVER2008", "HELP", "False", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "INDICATEPROGRESS", "False", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "QUIET", "True", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "QUIETSIMPLE", "False", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "X86", "False", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "MEDIASOURCE", Path.GetDirectoryName(Utils.GetDownloadString(prg, "FILE")), iniFile);
      WritePrivateProfileString("SQLSERVER2008", "ERRORREPORTING", "False", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "INSTALLSHAREDDIR", "\"" + InstallationProperties.Instance["DBMSDir"] + "\"", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "INSTANCEDIR", "\"" + InstallationProperties.Instance["DBMSDir"] + "\"", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "SQMREPORTING", "False", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "INSTANCENAME", GetIstanceName(), iniFile);
      WritePrivateProfileString("SQLSERVER2008", "SQLSVCSTARTUPTYPE", "Automatic", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "FILESTREAMLEVEL", "0", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "SQLCOLLATION", "Latin1_General_CI_AS", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "SQLSVCACCOUNT", "\"NT AUTHORITY\\NETWORK SERVICE\"", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "SQLSYSADMINACCOUNTS", "\"Builtin\\Administrators\"", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "SECURITYMODE", "SQL", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "ADDCURRENTUSERASSQLADMIN", "True", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "TCPENABLED", "1", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "NPENABLED", "0", iniFile);
      WritePrivateProfileString("SQLSERVER2008", "BROWSERSVCSTARTUPTYPE", "Automatic", iniFile);
    }

    /*
    private static void FixTcpPort()
    {
      RegistryKey keySql = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL");
      if (keySql == null)
      {
        return;
      }
      string instanceSQL = (string)keySql.GetValue(GetIstanceName());
      keySql.Close();

      keySql = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\" + instanceSQL + "\\MSSQLServer\\SuperSocketNetLib\\Tcp\\IPAll", true);
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
      string[] services = { "MSSQL$" + GetIstanceName(), "SQLBrowser" };
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
    */

    public string GetDisplayName()
    {
      return "MS SQL Express 2008 SP1";
    }

    private static string GetIstanceName()
    {
      return "SQLEXPRESS";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      string iniFile = Path.GetTempPath() + "\\SQL2008CfgFile.ini";

      //Prepare the unattended ini file
      PrepareTemplateINI(iniFile);

      try
      {
        //run the setup
        Process setup = Process.Start(_fileName, "/CONFIGURATIONFILE=\"" + iniFile + "\" /Q /HIDECONSOLE");
        if (setup != null)
        {
          setup.WaitForExit();
          return setup.ExitCode == 0;
        }
        return false;
      }
      catch
      {
        return false;
      }
    }

    public bool UnInstall()
    {
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\\\CurrentVersion\\Uninstall\\Microsoft SQL Server 10 Release"))
      {
        if (key == null)
        {
          return false;
        }
        string uninstaller = (string)key.GetValue("UninstallString");
        key.Close();
        Process setup = Process.Start(uninstaller.Split('"')[0] + "\"", uninstaller.Split(' ')[1]);
        if (setup != null)
        {
          setup.WaitForExit();
          return setup.ExitCode == 0;
        }
      }
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = true;
      FileInfo msSqlFile = new FileInfo(_fileName);

      if (msSqlFile.Exists && msSqlFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server\\SQLEXPRESS\\MSSQLServer\\CurrentVersion"))
      {
        if (key == null)
          result.state = CheckState.NOT_INSTALLED;
        else
        {
          string version = (string)key.GetValue("CurrentVersion");
          key.Close();
          result.state = version.StartsWith("10.0") ? CheckState.INSTALLED : CheckState.VERSION_MISMATCH;
        }
      }
      return result;
    }
  }
}