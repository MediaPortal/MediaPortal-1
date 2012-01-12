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
using System.Security.AccessControl;
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
    public static int DefaultInitTimeOut()
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
        try
        {
          using (ServiceController sc = new ServiceController("TvService"))
          {
            return sc.Status == ServiceControllerStatus.Running;
          }
        }
        catch (Exception ex)
        {
          Log.Error(
            "ServiceHelper: Check whether the tvservice is running failed. Please check your installation. \nError: {0}",
            ex.ToString());
          return false;
        }
      }
    }

    /// <summary>
    /// Is TvService fully initialized?
    /// </summary>
    public static bool IsInitialized
    {
      get { return WaitInitialized(0); }
    }

    /// <summary>
    /// Wait until TvService is fully initialized, wait for the default timeout
    /// </summary>
    /// <returns>true if thTvService is initialized</returns>
    public static bool WaitInitialized()
    {
      return WaitInitialized(DefaultInitTimeOut() * 1000);
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
        EventWaitHandle initialized = EventWaitHandle.OpenExisting(RemoteControl.InitializedEventName,
                                                                   EventWaitHandleRights.Synchronize);
        return initialized.WaitOne(millisecondsTimeout);
      }
      catch (Exception ex) // either we have no right, or the event does not exist
      {
        Log.Error("Failed to wait for {0}", RemoteControl.InitializedEventName);
        Log.Write(ex);
      }
      // Fall back: try to call a method on the server (for earlier versions of TvService)
      DateTime expires = millisecondsTimeout == -1
                           ? DateTime.MaxValue
                           : DateTime.Now.AddMilliseconds(millisecondsTimeout);

      // Note if millisecondsTimeout = 0, we never enter the loop and always return false
      // There is no way to determine if TvService is initialized without waiting
      while (DateTime.Now < expires)
      {
        try
        {
          RemoteControl.Clear();
          int cards = RemoteControl.Instance.Cards;
          return true;
        }
        catch (System.Runtime.Remoting.RemotingException)
        {
          Log.Info("ServiceHelper: Waiting for tvserver to initialize. (remoting not initialized)");
        }
        catch (System.Net.Sockets.SocketException)
        {
          Log.Info("ServiceHelper: Waiting for tvserver to initialize. (socket not initialized)");
        }
        catch (Exception ex)
        {
          Log.Error(
            "ServiceHelper: Could not check whether the tvservice is running. Please check your network as well. \nError: {0}",
            ex.ToString());
          break;
        }
        Thread.Sleep(250);
      }
      return false;
    }

    /// <summary>
    /// Is the status of TvService == Stopped
    /// </summary>
    public static bool IsStopped
    {
      get
      {
        try
        {
          using (ServiceController sc = new ServiceController("TvService"))
          {
            return sc.Status == ServiceControllerStatus.Stopped; // should we consider Stopping as stopped?
          }
        }
        catch (Exception ex)
        {
          Log.Error(
            "ServiceHelper: Check whether the tvservice is stopped failed. Please check your installation. \nError: {0}",
            ex.ToString());
          return false;
        }
      }
    }

    /// <summary>
    /// Stop the TvService
    /// </summary>
    /// <returns></returns>
    public static bool Stop()
    {
      try
      {
        using (ServiceController sc = new ServiceController("TvService"))
        {
          switch (sc.Status)
          {
            case ServiceControllerStatus.Running:
              sc.Stop();
              break;
            case ServiceControllerStatus.StopPending:
              break;
            case ServiceControllerStatus.Stopped:
              return true;
            default:
              return false;
          }
          sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
          return sc.Status == ServiceControllerStatus.Stopped;
        }
      }
      catch (Exception ex)
      {
        Log.Error(
          "ServiceHelper: Stopping tvservice failed. Please check your installation. \nError: {0}",
          ex.ToString());
        return false;
      }
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
      try
      {
        using (ServiceController sc = new ServiceController(aServiceName))
        {
          switch (sc.Status)
          {
            case ServiceControllerStatus.Stopped:
              sc.Start();
              break;
            case ServiceControllerStatus.StartPending:
              break;
            case ServiceControllerStatus.Running:
              return true;
            default:
              return false;
          }
          sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
          return sc.Status == ServiceControllerStatus.Running;
        }
      }
      catch (Exception ex)
      {
        Log.Error(
          "ServiceHelper: Starting {0} failed. Please check your installation. \nError: {1}",
          aServiceName, ex.ToString());
        return false;
      }
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
      return Start();
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
            rKey.SetValue("DependOnService", new[] {dependsOnService, "Netman"}, RegistryValueKind.MultiString);
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