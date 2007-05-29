/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using Microsoft.Win32;

namespace SetupTv
{
  public class ServiceHelper
  {

    static public bool IsInstalled(string serviceToFind)
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

    static public bool IsRunning
    {
      get
      {
        ServiceController[] services = ServiceController.GetServices();
        foreach (ServiceController service in services)
        {
          if (String.Compare(service.ServiceName, "TvService", true) == 0)
          {
            if (service.Status == ServiceControllerStatus.Running) return true;
            return false;
          }
        }
        return false;
      }
    }

    static public bool IsStopped
    {
      get
      {
        ServiceController[] services = ServiceController.GetServices();
        foreach (ServiceController service in services)
        {
          if (String.Compare(service.ServiceName, "TvService", true) == 0)
          {
            if (service.Status == ServiceControllerStatus.Stopped) return true;
            return false;
          }
        }
        return false;
      }
    }

    static public bool Stop()
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
    
    static public bool Start()
    {
      ServiceController[] services = ServiceController.GetServices();
      foreach (ServiceController service in services)
      {
        if (String.Compare(service.ServiceName, "TvService", true) == 0)
        {
          if (service.Status == ServiceControllerStatus.Stopped)
          {
            service.Start();
            return true;
          }
        }
      }
      return false;
    }

    static public bool Restart()
    {
      if (!IsInstalled(@"TvService")) return false;

      Stop();
      while (!IsStopped)
      {
        System.Threading.Thread.Sleep(300);
      }
      System.Threading.Thread.Sleep(2000);
      Start();
      while (!IsRunning)
      {
        System.Threading.Thread.Sleep(300);
      }
      return true;
    }

    /// <summary>
    /// Looks up the database service name for tvengine 3
    /// </summary>
    /// <param name="partOfSvcNameToComplete">Supply a (possibly unique) search term to indentify the service</param>
    /// <returns>true when search was successfull - modifies the search pattern to return the correct full name</returns>
    static public bool GetDBServiceName(ref string partOfSvcNameToComplete)
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
    /// Write dependency info for TvService.exe to registry
    /// </summary>
    /// <param name="dependsOnService">the database service that needs to be started</param>
    /// <returns>true if dependency was added successfully</returns>
    static public bool AddDependencyByName(string dependsOnService)
    {
      try
      {
        using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TVService", true))
        {
          if (rKey != null)
          {
            //dependencyKey = rKey.CreateSubKey()
            rKey.SetValue("DependOnService", new string[] { dependsOnService, "Netman" }, RegistryValueKind.MultiString);
          }
          //else
          //{
          //  MessageBox.Show("TvService is not installed on your system!", MessageBoxButtons.OK, MessageBoxIcon.Error);
          //}
        }

        return true;
      }
      catch (Exception)
      {
        // Log?
        return false;
      }
    }

  }
}
