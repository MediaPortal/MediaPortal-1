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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.ServiceProcess;
using System.Management;
using WatchDogService.Interface;

namespace WatchDogService
{
  public class WatchDogServer : MarshalByRefObject, WatchDogServiceInterface
  {
    public string StartTVService()
    {
      string result = string.Empty;

      ServiceController sc = new ServiceController();
      sc.ServiceName = "TVService";

      if (sc.Status == ServiceControllerStatus.Running)
      {
        result = "TVService is already running.";
      }
      else if (sc.Status == ServiceControllerStatus.Stopped)
      {
        sc.Start();
        int i = 0;
        while (sc.Status == ServiceControllerStatus.Running)
        {
          i++;
          Thread.Sleep(500);
          sc.Refresh();
          if (i == 60)
          {
            return "Failed to start TVService.";
          }
        }
        result = "TVService started successfully.";
      }

      return result;
    }

    public string StopTVService()
    {
      string result = string.Empty;

      ServiceController sc = new ServiceController();
      sc.ServiceName = "TVService";

      if (sc.Status == ServiceControllerStatus.Stopped)
      {
        result = "TVService already stopped.";
      }
      else if (sc.Status == ServiceControllerStatus.Running)
      {
        sc.Stop();
        int i = 0;
        while (sc.Status == ServiceControllerStatus.Stopped)
        {
          i++;
          Thread.Sleep(500);
          sc.Refresh();
          if (i == 60)
          {
            return "Failed to stop TVService.";
          }
        }
        result = "TVService stopped successfully.";
      }

      return result;
    }

    public void Reboot()
    {
      EventLog _eventLog = new EventLog();
      _eventLog.Source = "WatchDogService";
      _eventLog.WriteEntry("WatchDogService: Reboot command received.", EventLogEntryType.Information);

      ManagementBaseObject mboShutdown = null;
      ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
      mcWin32.Get();

      mcWin32.Scope.Options.EnablePrivileges = true;
      ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

      mboShutdownParams["Flags"] = "6";
      mboShutdownParams["Reserved"] = "0";
      foreach (ManagementObject manObj in mcWin32.GetInstances())
      {
        mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
      }
    }

    public void Shutdown()
    {
      EventLog _eventLog = new EventLog();
      _eventLog.Source = "WatchDogService";
      _eventLog.WriteEntry("WatchDogService: Shutdown command received.", EventLogEntryType.Information);

      ManagementBaseObject mboShutdown = null;
      ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
      mcWin32.Get();

      mcWin32.Scope.Options.EnablePrivileges = true;
      ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

      mboShutdownParams["Flags"] = "5";
      mboShutdownParams["Reserved"] = "0";
      foreach (ManagementObject manObj in mcWin32.GetInstances())
      {
        mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
      }
    }

    public void PowerOff()
    {
      EventLog _eventLog = new EventLog();
      _eventLog.Source = "WatchDogService";
      _eventLog.WriteEntry("WatchDogService: Power Off command received.", EventLogEntryType.Information);

      ManagementBaseObject mboShutdown = null;
      ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
      mcWin32.Get();

      mcWin32.Scope.Options.EnablePrivileges = true;
      ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

      mboShutdownParams["Flags"] = "12";
      mboShutdownParams["Reserved"] = "0";
      foreach (ManagementObject manObj in mcWin32.GetInstances())
      {
        mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
      }
    }

    public Object ReadLog()
    {
      string _tmpDir = Path.GetTempPath() + "\\TvServerLogs";
      string _zipFile = string.Format("{0}\\{1}_TvServerLogs.zip", Path.GetTempPath(), Environment.MachineName);

      ILogCreator TvServerLog = new TvServerLogger();
      ILogCreator TvServerApplicationLog = new EventLogCsvLogger("Application");
      ILogCreator TvServerSystemLog = new EventLogCsvLogger("System");

      if (!Directory.Exists(_tmpDir))
      {
        Directory.CreateDirectory(_tmpDir);
      }

      if (File.Exists(_zipFile))
      {
        File.Delete(_zipFile);
      }

      TvServerLog.CreateLogs(_tmpDir);
      TvServerApplicationLog.CreateLogs(_tmpDir);
      TvServerSystemLog.CreateLogs(_tmpDir);

      using (Archiver archiver = new Archiver())
      {
        archiver.AddDirectory(_tmpDir, _zipFile, false);
      }

      MemoryStream sr = new MemoryStream();
      using (FileStream reader = new FileStream(_zipFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      {
        byte[] buf = new byte[1024 * 1024];
        int bytesRead = reader.Read(buf, 0, 1024 * 1024);

        while (bytesRead > 0)
        {
          sr.Write(buf, 0, bytesRead);
          bytesRead = reader.Read(buf, 0, 1024 * 1024);
        }
        reader.Close();
        sr.Flush();
      }

      File.Delete(_zipFile);
      Directory.Delete(_tmpDir, true);

      return (object)sr;
    }

    public string ClearWindowsEventLogs()
    {
      string result = "Done.";
      string[] logNames = { "Application", "System"};
      foreach (string strLogName in logNames)
      {
        EventLog e = new EventLog(strLogName);
        try
        {
          e.Clear();
        }
        catch (Exception ex) 
        {  
           result = string.Format("Failed: {0}", ex);
        }
      }
      return result;
    }

    public string ClearTVserverLogs()
    {
      string logPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                  "\\Team MediaPortal\\MediaPortal TV Server\\log";
      ClearDir(logPath);

      return "Done.";
    }

    private void ClearDir(string strDir)
    {
      string[] files = Directory.GetFiles(strDir);
      string[] dirs = Directory.GetDirectories(strDir);

      foreach (string file in files)
      {
        if (File.Exists(file))
        {
          try
          {
            File.Delete(file);
          }
          catch (Exception) { }
        }
      }

      foreach (string dir in dirs)
      {
        if (Directory.Exists(dir))
        {
          try
          {
            Directory.Delete(dir, true);
          }
          catch (Exception) { }
        }
      }
      return;
    }

  }
}
