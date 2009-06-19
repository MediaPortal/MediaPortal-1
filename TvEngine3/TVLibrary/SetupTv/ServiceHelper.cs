#region Copyright (C) 2006-2009 Team MediaPortal
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
using TvLibrary.Log;

namespace SetupTv
{
  /// <summary>
  /// Offers basic control functions for services
  /// </summary>
  public class ServiceHelper
  {
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
          int serverId = TvControl.RemoteControl.Instance.IdServer;
          return true;
        }
        catch (Exception ex)
        {
          if (!(ex is System.Runtime.Remoting.RemotingException || ex is System.Net.Sockets.SocketException))
          {
            Log.Error("ServiceHelper: Could not check whether the tvservice is running. Please check your network as well. \nError: {0}", ex.ToString());
          }

          try
          {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
              if (String.Compare(service.ServiceName, "TvService", true) == 0)
              {
                if (service.Status == ServiceControllerStatus.Running)
                {
                  return true;
                }
                return false;
              }
            }
          }
          catch (Exception ex2)
          {
            Log.Error("ServiceHelper: Fallback to check whether the tvservice is running failed as well. Please check your installation. \nError: {0}", ex2.ToString());
          }
          return false;
        }
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
            if (service.Status == ServiceControllerStatus.Stopped)
              return true;
            return false;
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
            int hackCounter = 0;

            service.Start();

            while (!IsRunning && hackCounter < 60)
            {
              System.Threading.Thread.Sleep(250);
              hackCounter++;
            }
            return (hackCounter == 60) ? false : true;
          }
          if (service.Status == ServiceControllerStatus.Running)
          {
            return true;
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
      int hackCounter = 0;
      if (!IsInstalled(@"TvService"))
        return false;

      Stop();

      while (!IsStopped && hackCounter < 120) // wait a maximum of 30 seconds
      {
        System.Threading.Thread.Sleep(250);
        hackCounter++;
      }
      if (hackCounter == 120)
        return false;

      hackCounter = 0;
      System.Threading.Thread.Sleep(1000);

      Start();
      while (!IsRunning && hackCounter < 60)
      {
        System.Threading.Thread.Sleep(250);
        hackCounter++;
      }
      return (hackCounter == 60) ? false : true;
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
        using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + aServiceName, true))
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
        using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TVService", true))
        {
          if (rKey != null)
          {
            rKey.SetValue("DependOnService", new string[] { dependsOnService, "Netman" }, RegistryValueKind.MultiString);
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
