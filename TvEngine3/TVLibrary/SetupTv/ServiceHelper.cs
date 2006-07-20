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
