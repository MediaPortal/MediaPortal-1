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
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.ServiceProcess;
using System.Configuration.Install;
using TvLibrary.Log;

namespace TvService
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
      NameValueCollection appSettings = ConfigurationManager.AppSettings;
      appSettings.Set("GentleConfigFile", String.Format(@"{0}\gentle.config", Log.GetPathName()));


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
      if (opt != null && opt.ToUpperInvariant() == "/DEBUG")
      {
        Service1 s = new Service1();
        s.DoStart(null);
        while (true)
        {
          System.Threading.Thread.Sleep(10000);
        }
      }

      // More than one user Service may run within the same process. To add
      // another service to this process, change the following line to
      // create a second service object. For example,
      //
      //   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
      //
      ServiceBase[] ServicesToRun = new ServiceBase[] { new Service1() };

      ServiceBase.Run(ServicesToRun);
    }
  }
}
