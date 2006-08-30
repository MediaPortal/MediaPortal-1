/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

namespace SetupTv
{
  public class ServiceHelper
  {

    static public bool IsInstalled
    {
      get
      {
        ServiceController[] services = ServiceController.GetServices();
        foreach (ServiceController service in services)
        {
          if (String.Compare(service.ServiceName, "TvService", true) == 0)
          {
            return true;
          }
        }
        return false;
      }
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
      if (!IsInstalled) return false;

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

  }
}
