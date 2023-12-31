using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration.Install;

namespace WatchDogService
{
  [ServiceProcessDescriptionAttribute("WatchDogService")]
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
      string opt = null;
      if (args.Length >= 1)
      {
        opt = args[0];
      }

      if (opt != null && opt.ToUpperInvariant() == "/INSTALL")
      {
        TransactedInstaller ti = new TransactedInstaller();
        ProjectInstaller mi = new ProjectInstaller();
        ti.Installers.Add(mi);
        String path = String.Format("/assemblypath={0}",
                                    System.Reflection.Assembly.GetExecutingAssembly().Location);
        String[] cmdline = { path };
        InstallContext ctx = new InstallContext("", cmdline);
        ti.Context = ctx;
        ti.Install(new Hashtable());
        return;
      }

      if (opt != null && opt.ToUpperInvariant() == "/UNINSTALL")
      {
        TransactedInstaller ti = new TransactedInstaller();
        ProjectInstaller mi = new ProjectInstaller();
        ti.Installers.Add(mi);
        String path = String.Format("/assemblypath={0}",
                                    System.Reflection.Assembly.GetExecutingAssembly().Location);
        String[] cmdline = { path };
        InstallContext ctx = new InstallContext("", cmdline);
        ti.Context = ctx;
        ti.Uninstall(null);
        return;
      }
      
      ServiceBase[] ServicesToRun;
      ServicesToRun = new ServiceBase[] 
			{ 
				new WatchDogService() 
			};
      ServiceBase.Run(ServicesToRun);
    }
  }
}
