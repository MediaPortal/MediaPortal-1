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
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace MediaPortal.DeployTool
{
  class MySQLChecker : IInstallationPackage
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    private void PrepareMyIni(string iniFile)
    {
      WritePrivateProfileString("client", "port", "3306", iniFile);
      WritePrivateProfileString("mysql", "default-character-set", "utf8", iniFile);
      WritePrivateProfileString("mysqld", "port", "3306", iniFile);
      WritePrivateProfileString("mysqld", "basedir", "\"" + InstallationProperties.Instance["DBMSDir"].Replace('\\', '/') + "/\"", iniFile);
      WritePrivateProfileString("mysqld", "datadir", "\"" + InstallationProperties.Instance["DBMSDir"].Replace('\\', '/') + "/Data/\"", iniFile);
      WritePrivateProfileString("mysqld", "default-character-set", "utf8", iniFile);
      WritePrivateProfileString("mysqld", "default-storage-engine", "myisam", iniFile);
      WritePrivateProfileString("mysqld", "sql-mode", "\"STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION\"", iniFile);
      WritePrivateProfileString("mysqld", "max_connections", "100", iniFile);
      WritePrivateProfileString("mysqld", "query_cache_size", "32M", iniFile);
      WritePrivateProfileString("mysqld", "table_cache", "64", iniFile);
      WritePrivateProfileString("mysqld", "tmp_table", "18M", iniFile);
      WritePrivateProfileString("mysqld", "thread_cache_size", "4", iniFile);
      WritePrivateProfileString("mysqld", "thread_concurrency", "4", iniFile);
      WritePrivateProfileString("mysqld", "myisam_max_sort_file_size", "100G", iniFile);
      WritePrivateProfileString("mysqld", "myisam_max_extra_sort_file_size", "100G", iniFile);
      WritePrivateProfileString("mysqld", "myisam_sort_buffer_size", "64M", iniFile);
      WritePrivateProfileString("mysqld", "key_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "read_buffer_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "read_rnd_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "sort_buffer_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_additional_mem_pool_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_flush_log_at_trx_commit", "1", iniFile);
      WritePrivateProfileString("mysqld", "innodb_log_buffer_size", "1M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_buffer_pool_size", "96M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_log_file_size", "50M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_thread_concurrency", "8", iniFile);
    }

    public string GetDisplayName()
    {
      return "MySQL 5";
    }

    public bool Download()
    {
      string prg = "MySQL";
      string FileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");
      DialogResult result;
      result = Utils.RetryDownloadFile(FileName, prg);
      return (result == DialogResult.OK);
    }

    public bool Install()
    {
      string cmdLine = "/i \"" + Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("MySQL", "FILE") + "\"";
      cmdLine += " ADDLOCAL=\"Server,ClientPrograms,MySQLCommandLineShell,MysqlCommandLineUtilsFeature,ServerInstanceConfig\"";
      cmdLine += " INSTALLDIR=\"" + InstallationProperties.Instance["DBMSDir"] + "\"";
      cmdLine += " /qn";
      cmdLine += " /L* \"" + Path.GetTempPath() + "\\mysqlinst.log\"";
      Process setup = Process.Start("msiexec.exe", cmdLine);
      try
      {
        setup.WaitForExit();
      }
      catch
      {
        return false;
      }
      StreamReader sr = new StreamReader(Path.GetTempPath() + "\\mysqlinst.log");
      bool installOk = false;
      while (!sr.EndOfStream)
      {
        string line = sr.ReadLine();
        if (line.Contains("Installation operation completed successfully"))
        {
          installOk = true;
          break;
        }
      }
      sr.Close();
      if (!installOk)
        return false;
      string inifile = InstallationProperties.Instance["DBMSDir"] + "\\my.ini";
      PrepareMyIni(inifile);
      Process svcInstaller = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqld-nt.exe", "--install mysqld --defaults-file=\"" + inifile + "\"");
      svcInstaller.WaitForExit();
      ServiceController ctrl = new ServiceController("mysql");
      try
      {
        ctrl.Start();
      }
      catch (Exception)
      {
        return false;
      }
      for (int i = 1; i < 5; i++)
      {
        if (ctrl.Status == ServiceControllerStatus.Running) break;
        System.Threading.Thread.Sleep(1000 * i);
      }
      // Service is running, but still take some time to answer network queries
      System.Threading.Thread.Sleep(5000);
      //
      // mysqladmin.exe is used to set MySQL password
      //
      cmdLine = "-u root password " + InstallationProperties.Instance["DBMSPassword"];
      Process mysqladmin = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqladmin.exe", cmdLine);
      try
      {
        mysqladmin.WaitForExit();
        if (mysqladmin.ExitCode != 0) return false;
      }
      catch
      {
        return false;
      }
      System.Threading.Thread.Sleep(2000);
      //
      // mysql.exe is used to grant root access from all machines
      //
      cmdLine = "-u root --password=" + InstallationProperties.Instance["DBMSPassword"] + " --execute=\"GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '" + InstallationProperties.Instance["DBMSPassword"] + "' WITH GRANT OPTION\" mysql";
      Process mysql = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql.exe", cmdLine);
      try
      {
        mysql.WaitForExit();
        if (mysql.ExitCode == 0)
          return true;
        else
          return false;
      }
      catch
      {
        return false;
      }
    }

    public bool UnInstall()
    {
      Process mysql = Process.Start("msiexec", "/X {2FEB25F8-C3CB-49A2-AE79-DE17FFAFB5D9}");
      mysql.WaitForExit();
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result;
      result.needsDownload = !File.Exists(Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString("MySQL", "FILE"));
      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        if (result.needsDownload == false)
          result.state = CheckState.DOWNLOADED;
        else
          result.state = CheckState.NOT_DOWNLOADED;
        return result;
      }
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + InstallationProperties.Instance["RegistryKeyAdd"] + "MySQL AB\\MySQL Server 5.0");
      if (key == null)
        result.state = CheckState.NOT_INSTALLED;
      else
      {
        key.Close();
        result.state = CheckState.INSTALLED;
      }
      return result;
    }
  }
}
