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
using System.Collections;
using System.Configuration.Install;
#if DEBUG
using System.IO;
#endif
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVLibrary;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVService
{
  public partial class Service1 : ServiceBase
  {
    //private Thread _tvServiceThread = null;
    private static Thread _unhandledExceptionInThread = null;

    const int SERVICE_ACCEPT_PRESHUTDOWN = 0x100;   // not supported on Windows XP
    const int SERVICE_CONTROL_PRESHUTDOWN = 0xf;    // not supported on Windows XP

    /// <summary>
    /// Initializes a new instance of the <see cref="Service1"/> class.
    /// </summary>
    public Service1()
    {
      if (OSInfo.OSInfo.VistaOrLater())
      {
        try
        {
          // Call base Creator
          MethodInfo init = typeof(ServiceBase).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
          init.Invoke(this, new object[] { false });

          // Register the "TvServiceCallbackEx" handler
          Type targetDelegate = typeof(ServiceBase).Assembly.GetType("System.ServiceProcess.NativeMethods+ServiceControlCallbackEx");
          FieldInfo commandCallbackEx = typeof(ServiceBase).GetField("commandCallbackEx", BindingFlags.Instance | BindingFlags.NonPublic);
          commandCallbackEx.SetValue(this, Delegate.CreateDelegate(targetDelegate, this, "TvServiceCallbackEx"));

          // Accept SERVICE_CONTROL_PRESHUTDOWN
          var acceptedCommands = typeof(ServiceBase).GetField("acceptedCommands", BindingFlags.Instance | BindingFlags.NonPublic);
          int accCom = (int)acceptedCommands.GetValue(this);
          acceptedCommands.SetValue(this, accCom | SERVICE_ACCEPT_PRESHUTDOWN);

        }
        catch (Exception ex)
        {
          this.LogError(ex, "Exception on Starting TV Service");
          throw;
        }
      }

      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      InitializeComponent();
    }

    public static bool HasThreadCausedAnUnhandledException(Thread thread)
    {
      bool hasCurrentThreadCausedAnUnhandledException = false;

      if (_unhandledExceptionInThread != null)
      {
        hasCurrentThreadCausedAnUnhandledException = (_unhandledExceptionInThread.ManagedThreadId ==
                                                      thread.ManagedThreadId);
      }

      return hasCurrentThreadCausedAnUnhandledException;
    }

    /// <summary>
    /// Handles the UnhandledException event of the CurrentDomain control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      try
      {
        this.LogDebug("Tvservice stopped due to an unhandled app domain exception {0}", e.ExceptionObject);
        _unhandledExceptionInThread = Thread.CurrentThread;
        ExitCode = -1; //tell windows that the service failed.      
        OnStop(); //cleanup
      }
      finally
      {
        Environment.Exit(-1);
      }            
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void Main(string[] args)
    {
      string opt = null;
      if (args.Length >= 1)
      {
        opt = args[0];
      }

      if (opt != null && opt.ToUpperInvariant() == "-CONSOLE")
      {
        var tvServiceThread = new TvServiceThread(Application.ExecutablePath);
        tvServiceThread.Start();

        while (Console.ReadKey().KeyChar != 'q')
        {
        }
        tvServiceThread.Stop(60000);
      }
      else if (opt != null && opt.ToUpperInvariant() == "/INSTALL")
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
      else if (opt != null && opt.ToUpperInvariant() == "/UNINSTALL")
      {
        using (TransactedInstaller ti = new TransactedInstaller())
        {
          ProjectInstaller mi = new ProjectInstaller();
          ti.Installers.Add(mi);
          String path = String.Format("/assemblypath={0}",
                                      System.Reflection.Assembly.GetExecutingAssembly().Location);
          String[] cmdline = { path };
          InstallContext ctx = new InstallContext("", cmdline);
          ti.Context = ctx;
          ti.Uninstall(null);
        }
        return;
      }
      // When using /DEBUG switch (in visual studio) the TvService is not run as a service
      // Make sure the real TvService is disabled before debugging with /DEBUG
      else if (opt != null && opt.ToUpperInvariant() == "/DEBUG")
      {
        Service1 s = new Service1();
        s.DoStart(new string[] { "/DEBUG" });
        do
        {
          Thread.Sleep(100);
        } while (true);
      }
      // Start TVService
      else if (opt != null && opt.ToUpperInvariant() == "/START")
      {
        try
        {
          using (ServiceController sc = new ServiceController("TVService"))
          {
            switch (sc.Status)
            {
              case ServiceControllerStatus.Running:
                return;
              case ServiceControllerStatus.StopPending:
                return;
              case ServiceControllerStatus.Stopped:
                sc.Start();
                break;
            }
            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            return;
          }
        }
        catch (Exception ex)
        {
          Log.Error(
            "ServiceHelper: Starting tvservice failed. Please check your installation. \nError: {0}",
            ex.ToString());
          return;
        }
      }

      // More than one user Service may run within the same process. To add
      // another service to this process, change the following line to
      // create a second service object. For example,
      //
      //   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
      //
      else
      {
        ServiceBase[] ServicesToRun = new ServiceBase[] { new Service1() };
        ServicesToRun[0].CanShutdown = true;    // Allow OnShutdown()
        ServiceBase.Run(ServicesToRun);
      }
    }

    public void DoStart(string[] args)
    {
      OnStart(args);
    }

    public void DoStop()
    {
      OnStop();
    }


    private TvServiceThread _tvServiceThread;

    /// <summary>
    /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
    /// </summary>
    /// <param name="args">Data passed by the start command.</param>
    protected override void OnStart(string[] args)
    {
      if (_tvServiceThread == null)
      {
#if DEBUG
        if (File.Exists(@"c:\debug_tvservice.txt"))
        {
          System.Diagnostics.Debugger.Launch();        
        }
#endif
        
        if (!(args != null && args.Length > 0 && args[0] == "/DEBUG"))
        {
          RequestAdditionalTime(60000); // starting database can be slow so increase default timeout        
        }

        _tvServiceThread = new TvServiceThread(Application.ExecutablePath);
        _tvServiceThread.Start();
        _tvServiceThread.InitializedEvent.WaitOne();
      }
    }

    private void debug()
    {
      /*var rnd = new Random();
      Program p = ProgramManagement.GetProgram(4971);
      p.starRating = rnd.Next(1, 1000);

      //p.ChangeTracker.ChangeTrackingEnabled = true;

      ProgramManagement.SaveProgram(p);

      var pNew = new Program();

      pNew.idChannel = 123;
      pNew.startTime = DateTime.Now;
      pNew.endTime = DateTime.Now;
      pNew.title = "anything";

      pNew = ProgramManagement.SaveProgram(pNew);
      pNew = ProgramManagement.SaveProgram(pNew);

      ProgramManagement.DeleteProgram(pNew.idProgram);


      */
      /* var channels = ChannelManagement.ListAllChannels().ToList();
      Channel dr1 = channels[0];//ChannelManagement.GetChannel(1);
      Channel dr2 = channels[1]; //ChannelManagement.GetChannel(2);

      Card card1 = Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.CardManagement.GetCard(2);

      ChannelMap map = new ChannelMap();
      map.idChannel = dr1.idChannel;
      map.idCard = card1.idCard;

      dr1.ChannelMaps.AddSubChannelOrUser(map);

      dr1 = ChannelManagement.SaveChannel(dr1);

      ChannelMap map2 = new ChannelMap();
      map2.idChannel = dr2.idChannel;
      map2.idCard = card1.idCard;

      dr2.ChannelMaps.AddSubChannelOrUser(map2);

      dr2 = ChannelManagement.SaveChannel(dr2);
      */

      /*Model mdl = new Model();
      GenericRepository rep = new GenericRepository(mdl);

      rep.UnitOfWork.BeginTransaction();
      try
      {
        throw new Exception("123");
        rep.UnitOfWork.CommitTransaction();
      }
      catch (Exception)
      {        
        rep.UnitOfWork.RollBackTransaction();
      }*/
      /*
      ProgramManagement man = new ProgramManagement();
      IList<Program> prgs = man.GetProgramsByTitleAndStartEndTimes("Troldspejlet", new DateTime(2011, 10, 22, 11, 35, 0),
                                             new DateTime(2011, 10, 22, 12, 0, 0)).ToList();
      */
      /*ImportParams importParams = new ImportParams();
      importParams.ProgamsToDelete = DeleteBeforeImportOption.OverlappingPrograms;


      List<Program> programs = new List<Program>();

      var prg = new Program();
      prg.idChannel = 4;
      prg.startTime = DateTime.Now;
      prg.endTime = DateTime.Now.AddHours(1);
      prg.title = "title";
      prg.description = "description";
      prg.state = (int)ProgramState.None;
      prg.originalAirDate = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
      prg.seriesNum = "seriesNum";
      prg.episodeNum = "episodeNum";
      prg.episodeName = "episodeName";
      prg.episodePart = "episodePart";
      prg.starRating = 1;
      prg.classification = "classification";
      prg.parentalRating = -1;
      prg.previouslyShown = false;


      ProgramCredit credit = new ProgramCredit();
      for (int i = 0; i < 100; i++)
      {
        credit.role += "abc";
        credit.person += "abc";
      }      
      prg.ProgramCredits.AddSubChannelOrUser(credit);

      programs.AddSubChannelOrUser(prg);

      importParams.ProgramList = new ProgramList(programs);

      man.InsertPrograms(importParams);*/
    }



    /// <summary>
    /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
    /// </summary>
    protected override void OnStop()
    {
      RequestAdditionalTime(300000); // we want to make sure all services etc. are closed before tearing down the process.
      this.LogDebug("service.OnStop");
      if (_tvServiceThread != null)
      {
        _tvServiceThread.Stop(60000);
      }
      base.OnStop();
      ExitCode = 0;
    }

    /// <summary>
    /// When implemented in a derived class, executes when the system is shutting down. Specifies what should occur immediately prior to the system shutting down.
    /// </summary>
    protected override void OnShutdown()
    {
      OnStop();
    }

    /// <summary>
    /// Handle service events
    /// </summary>
    /// <param name="control"></param>
    /// <param name="eventType"></param>
    /// <param name="eventData"></param>
    /// <param name="eventContext"></param>
    /// <returns></returns>
    internal virtual int TvServiceCallbackEx(int control, int eventType, IntPtr eventData, IntPtr eventContext)
    {
      var baseCallback = typeof(ServiceBase).GetMethod("ServiceCommandCallbackEx", BindingFlags.Instance | BindingFlags.NonPublic);
      switch (control)
      {
        case SERVICE_CONTROL_PRESHUTDOWN:
          this.LogDebug("TV Service got PRESHUTDOWN event");
          // Pretend shutdown was called
          return (int)baseCallback.Invoke(this, new object[] { 0x00000005, eventType, eventData, eventContext });
        default:
          // Call base
          return (int)baseCallback.Invoke(this, new object[] { control, eventType, eventData, eventContext });
      }
    }
  }
}