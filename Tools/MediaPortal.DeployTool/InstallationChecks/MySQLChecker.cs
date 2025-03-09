#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
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

    private static readonly string prg = "MySQL83";
    private static readonly string _fileName = Application.StartupPath + "\\deploy\\" + Utils.GetDownloadString(prg, "FILE");

    private readonly string _dataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                       "\\MySQL\\MySQL Server 8.3";

    private static bool MySQL51 = false;
    private static bool MySQL56 = false;
    private static bool MySQL57 = false;

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
                                "\"STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION\"", iniFile);
      WritePrivateProfileString("mysqld", "max_connections", "100", iniFile);
      //WritePrivateProfileString("mysqld", "query_cache_size", "32M", iniFile);
      WritePrivateProfileString("mysqld", "tmp_table_size", "18M", iniFile);
      WritePrivateProfileString("mysqld", "thread_cache_size", "4", iniFile);
      //WritePrivateProfileString("mysqld", "thread_concurrency", "4", iniFile);
      WritePrivateProfileString("mysqld", "myisam_max_sort_file_size", "100M", iniFile);
      WritePrivateProfileString("mysqld", "myisam_sort_buffer_size", "64M", iniFile);
      WritePrivateProfileString("mysqld", "key_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "read_buffer_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "read_rnd_buffer_size", "16M", iniFile);
      WritePrivateProfileString("mysqld", "sort_buffer_size", "2M", iniFile);
      //WritePrivateProfileString("mysqld", "innodb_additional_mem_pool_size", "2M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_flush_log_at_trx_commit", "1", iniFile);
      WritePrivateProfileString("mysqld", "innodb_log_buffer_size", "1M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_buffer_pool_size", "96M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_redo_log_capacity", "50M", iniFile);
      WritePrivateProfileString("mysqld", "innodb_thread_concurrency", "8", iniFile);
    }

    public string GetDisplayName()
    {
      return "MySQL 8.3";
    }

    public string GetIconName()
    {
      return "MySQL";
    }

    public bool Download()
    {
      DialogResult result = Utils.RetryDownloadFile(_fileName, prg);
      return (result == DialogResult.OK);
    }

    private bool IsMySQL51Installed()
    {
      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.1");
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

    private bool IsMySQL56Installed()
    {
      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.6");
      if (key != null)
      {
        strMySQL = key.GetValue("Location").ToString();
        if (Utils.CheckTargetDir(strMySQL) && strMySQL.Contains("MySQL Server 5.6"))
        {
          strMySQLData = key.GetValue("DataLocation").ToString();
          key.Close();
          return true;
        }
      }
      return false;
    }

    private bool IsMySQL57Installed()
    {
      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.7");
      if (key != null)
      {
        strMySQL = key.GetValue("Location").ToString();
        if (Utils.CheckTargetDir(strMySQL) && strMySQL.Contains("MySQL Server 5.7"))
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

        string strMySQLDump = null;
        strMySQLDump = "\"" + strMySQL + "bin\\mysqldump.exe" + "\"";
        string cmdLine = "-uroot -p" + InstallationProperties.Instance["DBMSPassword"] +
                         " --all-databases --flush-logs";
        cmdLine += " -r " + "\"" + Path.GetTempPath() + "all_databases.sql" + "\"";
        int exitCode = Utils.RunCommandWait(strMySQLDump, cmdLine);
        if (exitCode == -1)
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
      string strMySQL = InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql.exe";
      string cmdLine = "--host=localhost --user=root --port=3306 --default-character-set=utf8 -p";
      cmdLine += InstallationProperties.Instance["DBMSPassword"];
      cmdLine += " --comments ";
      cmdLine += "-e " + "\"" + "source " + Path.GetTempPath() + "all_databases.sql" + "\"";
      int exitCode = Utils.RunCommandWait(strMySQL, cmdLine);
      if (exitCode == -1)
      {
        return false;
      }
      return true;
    }

    public bool ForceUpdateDB()
    {
      string strMySQL = InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql_upgrade.exe";
      string cmdLine = "--host=localhost --user=root -p";
      cmdLine += InstallationProperties.Instance["DBMSPassword"];
      cmdLine += " --force";
      int exitCode = Utils.RunCommandWait(strMySQL, cmdLine);
      if (exitCode == -1)
      {
        return false;
      }
      return true;
    }

    public bool Install()
    {
      MySQL51 = IsMySQL51Installed();
      MySQL56 = IsMySQL56Installed();
      MySQL57 = IsMySQL57Installed();

      bool IsBackupDB = false;
      if (MySQL51)
      {
        // Backup MySQL 5.1 Database and uninstall current MySQL 5.1
        IsBackupDB = BackupDB();
        Utils.UninstallMSI("{561AB451-B967-475C-80E0-3B6679C38B52}");
        Utils.UninstallMSI("{291D8FE1-ED05-4934-80CE-A5F6B7A8718D}");
      }
      if (MySQL56)
      {
        // Backup MySQL 5.6 Database and uninstall current MySQL 5.6
        IsBackupDB = BackupDB();
        Utils.UninstallMSI("{56DA0CB5-ABD2-4318-BEAB-62FDBC9B12CC}");
      }
      if (MySQL57)
      {
        // Backup MySQL 5.7 Database and uninstall current MySQL 5.7
        IsBackupDB = BackupDB();
        Utils.UninstallMSI("{F59F931A-0282-45D0-97CD-33F8B43AE3C2}");
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

      string inifile = InstallationProperties.Instance["DBMSDir"] + "\\my.ini";
      PrepareMyIni(inifile);

      //Create data directory if needed
      if (!Directory.Exists(_dataDir))
        Directory.CreateDirectory(_dataDir);

      //Initialize the data folder
      cmdLine = "--defaults-file=\"" + inifile + "\" --initialize-insecure";
      exitCode = Utils.RunCommandWait(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqld.exe", cmdLine);
      if (exitCode != 0)
      {
        MessageBox.Show("MySQL - failed to initialize db: " + exitCode);
        return false;
      }

      const string ServiceName = "MySQL";
      string cmdExe = Environment.SystemDirectory + "\\sc.exe";
      string cmdParam = "create " + ServiceName + " start= auto DisplayName= " + ServiceName + " binPath= \"" +
                        InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqld.exe --defaults-file=\\\"" + inifile +
                        "\\\" " + ServiceName + "\"";
#if DEBUG
      string ff = "c:\\MySQL-srv.bat";
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
        ctrl.WaitForStatus(ServiceControllerStatus.Running);
      }
      catch (Exception ex)
      {
        MessageBox.Show("MySQL - start service exception: " + ex.Message);
        return false;
      }

      // Service is running, but on slow machines still take some time to answer network queries
      System.Threading.Thread.Sleep(5000);
      //
      // mysqladmin.exe is used to set MySQL password
      //
      cmdLine = "-u root password " + InstallationProperties.Instance["DBMSPassword"];

      exitCode = Utils.RunCommandWait(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqladmin.exe", cmdLine);
      if (exitCode != 0)
      {
        cmdLine = "-u root --password=" + InstallationProperties.Instance["DBMSPassword"] + " password " + InstallationProperties.Instance["DBMSPassword"];
        exitCode = Utils.RunCommandWait(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysqladmin.exe", cmdLine);
        if (exitCode != 0)
        {
          MessageBox.Show("MySQL - set password error: " + exitCode);
          return false;
        }
      }
      System.Threading.Thread.Sleep(2000);

      //
      // mysql.exe is used to grant root access from all machines
      //
      //cmdLine = "-u root --password=" + InstallationProperties.Instance["DBMSPassword"] + " --execute=\"UPDATE mysql.user SET host='%' WHERE user='root';\" mysql";
      cmdLine = "-u root --password=" + InstallationProperties.Instance["DBMSPassword"] +
              " --execute=\"CREATE USER 'root'@'%' IDENTIFIED BY '" +
              InstallationProperties.Instance["DBMSPassword"] + "';GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;FLUSH PRIVILEGES;\" mysql";
      exitCode = Utils.RunCommandWait(InstallationProperties.Instance["DBMSDir"] + "\\bin\\mysql.exe", cmdLine);
      if (exitCode != 0)
      {
        MessageBox.Show("MySQL - set root access error: " + exitCode);
        return false;
      }

      if (MySQL51 || MySQL56 || MySQL57)
      {
        RestoreDB();
        ForceUpdateDB();
      }
      return true;
    }

    public bool UnInstall()
    {
      Utils.UninstallMSI("{D1DA7A5D-E358-40A5-8DB0-94B563487A74}");
      return true;
    }

    public CheckResult CheckStatus()
    {
      CheckResult result = default(CheckResult);

      // check if the user does not want MySQL installed
      if (InstallationProperties.Instance["ConfigureMediaPortalMySQL"] == "No")
      {
        result.state = CheckState.SKIPPED;
        return result;
      }

      result.needsDownload = true;
      FileInfo mySqlFile = new FileInfo(_fileName);

      if (mySqlFile.Exists && mySqlFile.Length != 0)
      {
        result.needsDownload = false;
      }

      if (InstallationProperties.Instance["InstallType"] == "download_only")
      {
        result.state = result.needsDownload == false ? CheckState.DOWNLOADED : CheckState.NOT_DOWNLOADED;
        return result;
      }

      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 8.3");
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

      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.1");
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

    public static CheckResult CheckStatusMySQL56()
    {
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

      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.6");
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

    public static CheckResult CheckStatusMySQL57()
    {
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

      RegistryKey key = Utils.LMOpenSubKey("SOFTWARE\\MySQL AB\\MySQL Server 5.7");
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