#region Copyright (C) 2005-2025 Team MediaPortal

// Copyright (C) 2005-2025 Team MediaPortal
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
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class MariaDBChecker : IInstallationPackage
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    private static readonly string prg = "MariaDB10";
    private static readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    private readonly string _dataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                       "\\MariaDB\\MariaDB Server 10.11\\data";

    private static string strMariaDB = "";
    private static string strMariaDBData = "";

    private void PrepareMyIni(string iniFile)
    {
      WritePrivateProfileString("client", "port", "3306", iniFile);

      WritePrivateProfileString("mysql", "default-character-set", "utf8", iniFile);

      WritePrivateProfileString("mysqld", "port", "3306", iniFile);
      WritePrivateProfileString("mysqld", "basedir", "\"" + InstallationProperties.Instance["DBMSDir"].Replace('\\', '/') + "\"", iniFile);
      WritePrivateProfileString("mysqld", "datadir", "\"" + _dataDir.Replace('\\', '/') + "\"", iniFile);
      WritePrivateProfileString("mysqld", "default-storage-engine", "INNODB", iniFile);
      WritePrivateProfileString("mysqld", "sql-mode","\"STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION\"", iniFile);
      WritePrivateProfileString("mysqld", "max_connections", "100", iniFile);
      WritePrivateProfileString("mysqld", "query_cache_size", "32M", iniFile);
      WritePrivateProfileString("mysqld", "tmp_table_size", "18M", iniFile);
      WritePrivateProfileString("mysqld", "thread_cache_size", "4", iniFile);
      WritePrivateProfileString("mysqld", "myisam_max_sort_file_size", "100M", iniFile);
      WritePrivateProfileString("mysqld", "myisam_sort_buffer_size", "64M", iniFile);
      WritePrivateProfileString("mysqld", "key_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "read_buffer_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "read_rnd_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "sort_buffer_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_flush_log_at_trx_commit", "1", iniFile);
      WritePrivateProfileString("mysqld", "innodb_log_buffer_size", "1M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_buffer_pool_size", "96M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_log_file_size", "50M", iniFile);
    }

    public string GetDisplayName()
    {
      return "MariaDB 10.11";
    }

    public string GetIconName()
    {
      return "MariaDB";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    public bool BackupDB()
    {
      if (!string.IsNullOrEmpty(strMariaDB) && !string.IsNullOrEmpty(strMariaDBData))
      {
        const string ServiceName = "MariaDB";
        ServiceController ctrl = new ServiceController(ServiceName);
        // Check if MariaDB is running and try to start it if not
        if (!ctrl.Status.Equals(ServiceControllerStatus.Running))
        {
          try
          {
            ctrl.Start();
          }
          catch (Exception)
          {
            MessageBox.Show("MariaDB - start backup DB service exception");
            return false;
          }
        }

        ctrl.WaitForStatus(ServiceControllerStatus.Running);
        // Service is running, but on slow machines still take some time to answer network queries
        System.Threading.Thread.Sleep(5000);

        string strMariaDBDump = null;
        strMariaDBDump = "\"" + strMariaDB + "bin\\mysqldump.exe" + "\"";
        string cmdLine = "-uroot -p" + InstallationProperties.Instance["DBMSPassword"] +
                         " --all-databases --flush-logs";
        cmdLine += " -r " + "\"" + Path.GetTempPath() + "all_databases.sql" + "\"";
        int exitCode = Utils.RunCommandWait(strMariaDBDump, cmdLine);
        if (exitCode == -1)
        {
          return false;
        }

        // Try to stop MariaDB service
        try
        {
          ctrl.Stop();
        }
        catch (Exception)
        {
          MessageBox.Show("MariaDB - stop backup DB service exception");
          //return false;
        }

        string cmdExe = Environment.SystemDirectory + "\\sc.exe";
        string cmdParam = "delete " + ServiceName; // +"\"";
#if DEBUG
        string ff = "c:\\mysql-srv-delete.bat";
        StreamWriter a = new StreamWriter(ff);
        a.WriteLine("@echo off");
        a.WriteLine(cmdExe + " " + cmdParam);
        a.Close();
        exitCode = Utils.RunCommandWait(ff, string.Empty);
#else
        exitCode = Utils.RunCommandWait(cmdExe, cmdParam);
#endif
        if (exitCode != -1)
        {
          return true;
        }
      }
      return false;
    }

    public bool RestoreDB()
    {
      string strMariaDB = InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql.exe";
      string cmdLine = "--host=localhost --user=root --port=3306 --default-character-set=utf8 -p";
      cmdLine += InstallationProperties.Instance["DBMSPassword"];
      cmdLine += " --comments ";
      cmdLine += "-e " + "\"" + "source " + Path.GetTempPath() + "all_databases.sql" + "\"";
      int exitCode = Utils.RunCommandWait(strMariaDB, cmdLine);
      if (exitCode == -1)
      {
        return false;
      }
      return true;
    }

    public bool ForceUpdateDB()
    {
      string strMariaDB = InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql_upgrade.exe";
      string cmdLine = "--host=localhost --user=root -p";
      cmdLine += InstallationProperties.Instance["DBMSPassword"];
      cmdLine += " --force";
      int exitCode = Utils.RunCommandWait(strMariaDB, cmdLine);
      if (exitCode == -1)
      {
        return false;
      }
      return true;
    }

    public bool Install()
    {
      bool IsBackupDB = false;

      if (!IsBackupDB)
      {
        // Try to stop actual MariaDB Service
        const string OldServiceName = "MariaDB";
        ServiceController ctrlMariaDB = new ServiceController(OldServiceName);
        try
        {
          ctrlMariaDB.Stop();
        }
        catch (Exception)
        {
          // Catch if service can't be stopped or didn't exist
        }
      }

      string strDBMSDir = InstallationProperties.Instance["DBMSDir"];
      string strPassword = InstallationProperties.Instance["DBMSPassword"];

      string cmdLine = "/i \"" + _fileName + "\"";
      cmdLine += " INSTALLDIR=\"" + strDBMSDir + "\"";
      cmdLine += " DATADIR=\"" + _dataDir + "\"";
      cmdLine += " ALLOWREMOTEROOTACCESS=true /qn";
      cmdLine += " /L* \"" + Path.GetTempPath() + "\\mysqlinst.log\"";
      int exitCode = Utils.RunCommandWait("msiexec.exe", cmdLine);
      if (exitCode == -1)
      {
        return false;
      }

      StreamReader sr = new StreamReader(Path.GetTempPath() + "\\mysqlinst.log");
      bool installOk = false;
      while (!sr.EndOfStream)
      {
        string line = sr.ReadLine();
        if (line.Contains("Configuration completed successfully") || line.Contains("Installation completed successfully") || line.Contains("completed successfully"))
        {
          installOk = true;
          //File.Delete(Path.GetTempPath() + "\\mysqlinst.log");
          break;
        }
      }
      sr.Close();
      if (!installOk)
      {
        return false;
      }

      string inifile = strDBMSDir + "\\my.ini";
      PrepareMyIni(inifile);

      const string ServiceName = "MariaDB";
      string cmdExe = Environment.SystemDirectory + "\\sc.exe";
      string cmdParam = "create " + ServiceName + " start= auto DisplayName= " + ServiceName + " binPath= \"" +
                        strDBMSDir + "\\bin\\mysqld.exe --defaults-file=\\\"" + inifile +
                        "\\\" " + ServiceName + "\"";
#if DEBUG
      string ff = "c:\\MariaDB-srv.bat";
      StreamWriter a = new StreamWriter(ff);
      a.WriteLine("@echo off");
      a.WriteLine(cmdExe + " " + cmdParam);
      a.Close();
      exitCode = Utils.RunCommandWait(ff, string.Empty);
#else
      exitCode = Utils.RunCommandWait(cmdExe, cmdParam);
#endif
      if (exitCode == -1)
      {
        return false;
      }

      ServiceController ctrl = new ServiceController(ServiceName);
      try
      {
        ctrl.Start();
      }
      catch (Exception)
      {
        MessageBox.Show("MariaDB - start service exception");
        return false;
      }

      ctrl.WaitForStatus(ServiceControllerStatus.Running);
      // Service is running, but on slow machines still take some time to answer network queries
      System.Threading.Thread.Sleep(5000);

      //
      // mysqladmin.exe is used to set MariaDB password
      //
      cmdLine = "-u root password " + strPassword;
      exitCode = Utils.RunCommandWait(strDBMSDir + "\\bin\\mysqladmin.exe", cmdLine);
      if (exitCode != 0)
      {
        cmdLine = "-u root --password=" + strPassword + " password " + strPassword;
        exitCode = Utils.RunCommandWait(strDBMSDir + "\\bin\\mysqladmin.exe", cmdLine);
        if (exitCode != 0)
        {
          MessageBox.Show("MariaDB - set password error: " + exitCode);
          return false;
        }
      }
      System.Threading.Thread.Sleep(2000);

      //
      // mysql.exe is used to grant root access from all machines
      //
      string strMysqlExe = strDBMSDir + "\\bin\\mysql.exe";
      cmdLine = "-u root --password=" + strPassword +
                " --execute=\"GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '" +
                strPassword + "' WITH GRANT OPTION\" mysql";
      exitCode = Utils.RunCommandWait(strMysqlExe, cmdLine);
      if (exitCode != 0)
      {
        MessageBox.Show("MariaDB - set privileges error: " + exitCode);
        return false;
      }

      //Set password to other root users
      cmdLine = "-u root --password=" + strPassword +
                " --execute=\"" +
                "SET PASSWORD FOR 'root'@'127.0.0.1' = PASSWORD('" + strPassword + "');" +
                "SET PASSWORD FOR 'root'@'" + System.Net.Dns.GetHostName() + "' = PASSWORD('" + strPassword + "');" +
                "SET PASSWORD FOR 'root'@'::1' = PASSWORD('" + strPassword + "');" +
                "FLUSH PRIVILEGES;\" mysql";
      Utils.RunCommandWait(strMysqlExe, cmdLine);

      return true;
    }

    public bool UnInstall()
    {
      Utils.UninstallMSI("{59A1E1C4-0E8C-4FE1-A420-A50181428673}");
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result = default(CheckResult);

      // check if the user does not want MariaDB installed
      if (InstallationProperties.Instance["ConfigureMediaPortalMariaDB"] == "No")
      {
        result.state = CheckState.SKIPPED;
        return result;
      }

      result.needsDownload = true;
      FileInfo MariaDBFile = new FileInfo(_fileName);

      if (MariaDBFile.Exists && MariaDBFile.Length != 0)
      {
        result.needsDownload = false;
      }

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MariaDB 10.11 (x64)");
      if (key == null)
      {
        result.state = CheckState.NOT_INSTALLED;
      }
      else
      {
        key.Close();
        result.state = CheckState.INSTALLED;
      }
      return result;
    }
  }
}