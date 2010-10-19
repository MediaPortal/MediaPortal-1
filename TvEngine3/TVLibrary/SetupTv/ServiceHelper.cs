#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.ServiceProcess;
using System.Threading;
using Microsoft.Win32;
using TvDatabase;
using TvControl;
using TvLibrary.Log;

namespace SetupTv
{
  /// <summary>
  /// Offers basic control functions for services
  /// </summary>
  public class ServiceHelper
  {

    /// <summary>
    /// Read from DB card detection delay 
    /// </summary>
    /// <returns>number of seconds</returns>
    public static int DefaultTimeOut()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      return Convert.ToInt16(layer.GetSetting("delayCardDetect", "0").Value) + 10;
    }

    /// <summary>
    /// Does a given service exist
    /// </summary>
    /// <param name="serviceToFind"></param>
    /// <returns></returns>
    public static bool IsInstalled(string serviceToFind)
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (String.Compare(service.ServiceName, serviceToFind, true) == 0)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Is the status of TvService == Running
    /// </summary>
    public static bool IsRunning
    {
      get
      {
        //Patch to be able to use the TvServer Configuration 
        //when running in debug mode
        try
        {
          //Try an call something to see if the server is alive.
          //
          WaitInitialized(DefaultTimeOut() * 1000);
          int serverId = RemoteControl.Instance.IdServer;
          return true;
        }
        catch (Exception ex)
        {
          if (!(ex is System.Runtime.Remoting.RemotingException || ex is System.Net.Sockets.SocketException))
          {
            Log.Error(
              "ServiceHelper: Could not check whether the tvservice is running. Please check your network as well. \nError: {0}",
              ex.ToString());
          }

          try
          {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
              if (String.Compare(service.ServiceName, "TvService", true) == 0)
              {
                return service.Status == ServiceControllerStatus.Running;
              }
            }
          }
          catch (Exception ex2)
          {
            Log.Error(
              "ServiceHelper: Fallback to check whether the tvservice is running failed as well. Please check your installation. \nError: {0}",
              ex2.ToString());
          }
          return false;
        }
      }
    }

    /// <summary>
    /// Is TvService fully initialized?
    /// </summary>
    public static bool IsInitialized
    {
      get
      {
        return WaitInitialized(0);
      }
    }

    /// <summary>
    /// Wait until TvService is fully initialized
    /// </summary>
    /// <param name="millisecondsTimeout">the maximum time to wait in milliseconds</param>
    /// <remarks>If <paramref name="millisecondsTimeout"/> is 0, the current status is immediately returned.
    /// Use <paramref name="millisecondsTimeout"/>=-1 to wait indefinitely</remarks>
    /// <returns>true if thTvService is initialized</returns>
    public static bool WaitInitialized(int millisecondsTimeout)
    {
      try
      {
        EventWaitHandle initialized = EventWaitHandle.OpenExisting(RemoteControl.InitializedEventName);
        return initialized.WaitOne(millisecondsTimeout);
      }
      catch (Exception) // either we have no right, or the event does not exist
      {
        return false;
      }
    }

    /// <summary>
    /// Is the status of TvService == Stopped
    /// </summary>
    public static bool IsStopped
    {
      get
      {
        ServiceController[] services = ServiceController.GetServices();
        foreach (ServiceController service in services)
        {
          if (String.Compare(service.ServiceName, "TvService", true) == 0)
          {
            return service.Status == ServiceControllerStatus.Stopped;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Stop the TvService
    /// </summary>
    /// <returns></returns>
    public static bool Stop()
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (String.Compare(service.ServiceName, "TvService", true) == 0)
        {
          if (service.Status == ServiceControllerStatus.Running)
          {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(DefaultTimeOut()));
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Starts the TvService
    /// </summary>
    /// <returns></returns>
    public static bool Start()
    {
      return Start("TvService");
    }

    public static bool Start(string aServiceName)
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (String.Compare(service.ServiceName, aServiceName, true) == 0)
        {
          if (service.Status == ServiceControllerStatus.Stopped)
          {
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(DefaultTimeOut()));
            // Service is running, but on slow machines still take some time to answer network queries
            return IsRunning;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Start/Stop cycles TvService
    /// </summary>
    /// <returns>Always true</returns>
    public static bool Restart()
    {
      if (!IsInstalled(@"TvService"))
      {
        return false;
      }
      Stop();
      Start();
      return true;
    }

    /// <summary>
    /// Looks up the database service name for tvengine 3
    /// </summary>
    /// <param name="partOfSvcNameToComplete">Supply a (possibly unique) search term to indentify the service</param>
    /// <returns>true when search was successfull - modifies the search pattern to return the correct full name</returns>
    public static bool GetDBServiceName(ref string partOfSvcNameToComplete)
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (service.ServiceName.Contains(partOfSvcNameToComplete))
        {
          partOfSvcNameToComplete = service.ServiceName;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Checks via registry whether a given service is set to autostart on boot
    /// </summary>
    /// <param name="aServiceName">The short name of the service</param>
    /// <param name="aSetEnabled">Enable autostart if needed</param>
    /// <returns>true if the service will start at boot</returns>
    public static bool IsServiceEnabled(string aServiceName, bool aSetEnabled)
    {
      try
      {
        using (
          RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + aServiceName, true)
          )
        {
          if (rKey != null)
          {
            int startMode = (int)rKey.GetValue("Start", 3);
            if (startMode == 2) // autostart
              return true;
            if (aSetEnabled)
            {
              rKey.SetValue("Start", 2, RegistryValueKind.DWord);
              return true;
            }
            return false;
          }
          return false; // probably wrong service name
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Write dependency info for TvService.exe to registry
    /// </summary>
    /// <param name="dependsOnService">the database service that needs to be started</param>
    /// <returns>true if dependency was added successfully</returns>
    public static bool AddDependencyByName(string dependsOnService)
    {
      try
      {
        using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TVService", true)
          )
        {
          if (rKey != null)
          {
            rKey.SetValue("DependOnService", new[] { dependsOnService, "Netman" }, RegistryValueKind.MultiString);
            rKey.SetValue("Start", 2, RegistryValueKind.DWord); // Set TVService to autostart
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("ServiceHelper: Failed to access registry {0}", ex.Message);
        return false;
      }
    }
  }
}