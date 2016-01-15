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
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace MediaPortal.DeployTool.InstallationChecks
{
  internal class MySQLChecker : IInstallationPackage
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    private static readonly string _arch = Utils.Check64bit() ? "64" : "32";
    private static readonly string prg = "MySQL56" + _arch;
    private static readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    private readonly string _dataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                       "\\MySQL\\MySQL Server 5.6";

    private static bool MySQL51 = false;
    private static string strMySQL = "";
    private static string strMySQLData = "";

    private void PrepareMyIni(string iniFile)
    {
      WritePrivateProfileString("client", "port", "3306", iniFile);
      WritePrivateProfileString("mysql", "default-character-set", "utf8", iniFile);
      WritePrivateProfileString("mysqld", "port", "3306", iniFile);
      WritePrivateProfileString("mysqld", "basedir",
                                "\"" + InstallationProperties.Instance["DBMSDir"].Replace('\\', '/') + "/\"", iniFile);
      WritePrivateProfileString("mysqld", "datadir", "\"" + _dataDir.Replace('\\', '/') + "/Data\"", iniFile);
      WritePrivateProfileString("mysqld", "default-storage-engine", "INNODB", iniFile);
      WritePrivateProfileString("mysqld", "sql-mode",
                                "\"STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION\"", iniFile);
      WritePrivateProfileString("mysqld", "max_connections", "100", iniFile);
      WritePrivateProfileString("mysqld", "query_cache_size", "32M", iniFile);
      WritePrivateProfileString("mysqld", "tmp_table_size", "18M", iniFile);
      WritePrivateProfileString("mysqld", "thread_cache_size", "4", iniFile);
      WritePrivateProfileString("mysqld", "thread_concurrency", "4", iniFile);
      WritePrivateProfileString("mysqld", "myisam_max_sort_file_size", "100M", iniFile);
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
      return "MySQL 5.6";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    private bool IsMySQL51Installed()
    {
      RegistryKey key = null;
      try
      {
        key = Utils.OpenSubKey(Registry.LocalMachine, "SOFTWARE\\MySQL AB\\MySQL Server 5.1", false,
                               Utils.eRegWow64Options.KEY_WOW64_32KEY);
      }
      catch
      {
        // Parent key not open, exception found at opening (probably related to
        // security permissions requested)
      }
      if (key == null)
      {
        key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.1");
      }
      if (key != null)
      {
        strMySQL = key.GetValue("Location").ToString();
        if (Utils.CheckTargetDir(strMySQL) && strMySQL.Contains("MySQL Server 5.1"))
        {
          strMySQLData = key.GetValue("DataLocation").ToString();
          key.Close();
          return true;
        }
      }
      return false;
    }

    public bool BackupDB()
    {
      if (!string.IsNullOrEmpty(strMySQL) && !string.IsNullOrEmpty(strMySQLData))
      {
        const string ServiceName = "MySQL";
        ServiceController ctrl = new ServiceController(ServiceName);
        // Check if MySQL is running and try to start it if not
        if (!ctrl.Status.Equals(ServiceControllerStatus.Running))
        {
          try
          {
            ctrl.Start();
          }
          catch (Exception)
          {
            MessageBox.Show("MySQL - start backup DB service exception");
            return false;
          }
        }

        ctrl.WaitForStatus(ServiceControllerStatus.Running);
        // Service is running, but on slow machines still take some time to answer network queries
        System.Threading.Thread.Sleep(5000);

        string strMySqlDump = null;
        strMySqlDump = "\"" + strMySQL + "bin\\mysqldump.exe" + "\"";
        string cmdLine = "-uroot -p" + InstallationProperties.Instance["DBMSPassword"] +
                         " --all-databases --flush-logs";
        cmdLine += " -r " + "\"" + Path.GetTempPath() + "all_databases.sql" + "\"";
        Process setup = Process.Start(strMySqlDump, cmdLine);
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

        // Try to stop MySQL service
        try
        {
          ctrl.Stop();
        }
        catch (Exception)
        {
          MessageBox.Show("MySQL - stop backup DB service exception");
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
        Process svcInstaller = Process.Start(ff);
#else
        Process svcInstaller = Process.Start(cmdExe, cmdParam);
#endif
        if (svcInstaller != null)
        {
          svcInstaller.WaitForExit();
        }
        return true;
      }
      return false;
    }

    public bool RestoreDB()
    {
      string strMySql = InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql.exe";
      string cmdLine = "--host=localhost --user=root --port=3306 --default-character-set=utf8 -p";
      cmdLine += InstallationProperties.Instance["DBMSPassword"];
      cmdLine += " --comments ";
      cmdLine += "-e " + "\"" + "source " + Path.GetTempPath() + "all_databases.sql" + "\"";
      Process setup = Process.Start(strMySql, cmdLine);
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

    public bool ForceUpdateDB()
    {
      string strMySql = InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql_upgrade.exe";
      string cmdLine = "--host=localhost --user=root -p";
      cmdLine += InstallationProperties.Instance["DBMSPassword"];
      cmdLine += " --force";
      Process setup = Process.Start(strMySql, cmdLine);
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

    public bool Install()
    {
      MySQL51 = IsMySQL51Installed();
      bool IsBackupDB = false;
      if (MySQL51)
      {
        // Backup MySQL 5.1 Database and uninstall current MySQL 5.1
        IsBackupDB = BackupDB();
        Utils.UninstallMSI("{561AB451-B967-475C-80E0-3B6679C38B52}");
        Utils.UninstallMSI("{291D8FE1-ED05-4934-80CE-A5F6B7A8718D}");
      }

      if (!IsBackupDB)
      {
        // Try to stop actual MySQL Service
        const string OldServiceName = "MySQL";
        ServiceController ctrlMySQL = new ServiceController(OldServiceName);
        try
        {
          ctrlMySQL.Stop();
        }
        catch (Exception)
        {
          // Catch if service can't be stopped or didn't exist
        }
      }

      string cmdLine = "/i \"" + _fileName + "\"";
      cmdLine += " INSTALLDIR=\"" + InstallationProperties.Instance["DBMSDir"] + "\"";
      cmdLine += " DATADIR=\"" + _dataDir + "\"";
      cmdLine += " /qn";
      cmdLine += " /L* \"" + Path.GetTempPath() + "\\mysqlinst.log\"";
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
      string inifile = InstallationProperties.Instance["DBMSDir"] + "\\my.ini";
      PrepareMyIni(inifile);
      const string ServiceName = "MySQL";
      string cmdExe = Environment.SystemDirectory + "\\sc.exe";
      string cmdParam = "create " + ServiceName + " start= auto DisplayName= " + ServiceName + " binPath= \"" +
                        InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqld.exe --defaults-file=\\\"" + inifile +
                        "\\\" " + ServiceName + "\"";
#if DEBUG
      string ff = "c:\\mysql-srv.bat";
      StreamWriter a = new StreamWriter(ff);
      a.WriteLine("@echo off");
      a.WriteLine(cmdExe + " " + cmdParam);
      a.Close();
      Process svcInstaller = Process.Start(ff);
#else
      Process svcInstaller = Process.Start(cmdExe, cmdParam);
#endif
      if (svcInstaller != null)
      {
        svcInstaller.WaitForExit();
      }
      ServiceController ctrl = new ServiceController(ServiceName);
      try
      {
        ctrl.Start();
      }
      catch (Exception)
      {
        MessageBox.Show("MySQL - start service exception");
        return false;
      }

      ctrl.WaitForStatus(ServiceControllerStatus.Running);
      // Service is running, but on slow machines still take some time to answer network queries
      System.Threading.Thread.Sleep(5000);
      //
      // mysqladmin.exe is used to set MySQL password
      //
      cmdLine = "-u root password " + InstallationProperties.Instance["DBMSPassword"];

      try
      {
        // Try restore DB here first
        if (MySQL51)
        {
          RestoreDB();
          ForceUpdateDB();
        }

        Process mysqladmin = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqladmin.exe", cmdLine);
        if (mysqladmin != null)
        {
          mysqladmin.WaitForExit();
          if (mysqladmin.ExitCode != 0)
          {
            cmdLine = "-u root --password=" + InstallationProperties.Instance["DBMSPassword"] + " password " + InstallationProperties.Instance["DBMSPassword"];
            mysqladmin = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqladmin.exe", cmdLine);
            if (mysqladmin != null)
            {
              mysqladmin.WaitForExit();
              if (mysqladmin.ExitCode != 0)
              {
                MessageBox.Show("MySQL - set password error: " + mysqladmin.ExitCode);
                return false;
              }
            }
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("MySQL - set password exception");
        return false;
      }
      System.Threading.Thread.Sleep(2000);
      //
      // mysql.exe is used to grant root access from all machines
      //
      cmdLine = "-u root --password=" + InstallationProperties.Instance["DBMSPassword"] +
                " --execute=\"GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '" +
                InstallationProperties.Instance["DBMSPassword"] + "' WITH GRANT OPTION\" mysql";
      Process mysql = Process.Start(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql.exe", cmdLine);
      try
      {
        if (mysql != null)
        {
          mysql.WaitForExit();
          if (mysql.ExitCode != 0)
          {
            MessageBox.Show("MySQL - set privileges error: " + mysql.ExitCode);
            return false;
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("MySQL - set privileges exception");
        return false;
      }
      if (MySQL51)
      {
        RestoreDB();
        ForceUpdateDB();
      }
      return true;
    }

    public bool UnInstall()
    {
      Utils.UninstallMSI("{56DA0CB5-ABD2-4318-BEAB-62FDBC9B12CC}");
      return true;
    }

    public CheckResult CheckStatus()
    {
      RegistryKey key = null;
      CheckResult result;
      result.needsDownload = true;
      FileInfo mySqlFile = new FileInfo(_fileName);

      if (mySqlFile.Exists && mySqlFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      try
      {
        key = Utils.OpenSubKey(Registry.LocalMachine, "SOFTWARE\\MySQL AB\\MySQL Server 5.6", false,
            Utils.eRegWow64Options.KEY_WOW64_32KEY);
      }
      catch
      {
        // Parent key not open, exception found at opening (probably related to
        // security permissions requested)
      }
      if (key == null)
      {
        key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.6");
      }
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

    public static CheckResult CheckStatusMySQL51()
    {
      RegistryKey key = null;
      CheckResult result;
      result.needsDownload = true;
      FileInfo mySqlFile = new FileInfo(_fileName);

      if (mySqlFile.Exists && mySqlFile.Length != 0)
        result.needsDownload = false;

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }
      try
      {
        key = Utils.OpenSubKey(Registry.LocalMachine, "SOFTWARE\\MySQL AB\\MySQL Server 5.1", false,
            Utils.eRegWow64Options.KEY_WOW64_32KEY);
      }
      catch
      {
        // Parent key not open, exception found at opening (probably related to
        // security permissions requested)
      }
      if (key == null)
      {
        key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.1");
      }
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