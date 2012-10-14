using System;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVLibrary;

namespace Mediaportal.TV.Server.TVService
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public static void Main(string[] args)
    {
      if (args.Contains("-console"))
      {
        var tvServiceThread = new TvServiceThread(Application.ExecutablePath);
        tvServiceThread.Start();        

        while (Console.ReadKey().KeyChar != 'q')
        {
        }        
        tvServiceThread.Stop(60000);
      }

      else
      {
        var servicesToRun = new ServiceBase[] 
                                                  { 
                                                      new Service1() 
                                                  };
        ServiceBase.Run(servicesToRun);
      }
    }
  }
}
