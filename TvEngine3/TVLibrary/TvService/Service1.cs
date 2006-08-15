/// general :
///  - get radio to work
///  - comskip support using .txt files
///  - rtsp seeking
///  - re-encoding the stream to use lower bandwidth
///  - analog s/w cards
///  - xmltv loader
/// 
///  tvplugin :
///    - stop timeshifting only when it has started it!!!
///    - remoting setup!!!
///    - verversen van database cache!!!
///    - notifies!!!
///    - messenger!!!
/// 
/// setuptv:
///    - dont allow radio scan for analog cards which dont have a radio tuner
/// 
///  test :
///    - multiple audio streams!!!
///    - hdtv!!!
/// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using TvLibrary.Log;
using TvControl;

namespace TvService
{
  public partial class Service1 : ServiceBase
  {
    #region variables
    TVController _controller;
    #endregion

    public Service1()
    {
      string applicationPath = System.Windows.Forms.Application.ExecutablePath;
      applicationPath = System.IO.Path.GetFullPath(applicationPath);
      applicationPath = System.IO.Path.GetDirectoryName(applicationPath);
      System.IO.Directory.SetCurrentDirectory(applicationPath);
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      Log.WriteFile("TV service started");
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      Process currentProcess = Process.GetCurrentProcess();
      //currentProcess.PriorityClass = ProcessPriorityClass.High;
      _controller = new TVController();
      StartRemoting();

    }

    protected override void OnStop()
    {
      Log.WriteFile("TV service stopping");
      
      StopRemoting();
      RemoteControl.Clear();
      if (_controller != null)
      {
        _controller.Dispose();
        _controller = null;
      }
      GC.Collect();
      GC.Collect();
      GC.Collect();
      GC.Collect();
      Log.WriteFile("TV service stopped");
    }

    void StartRemoting()
    {
      try
      {
        // setup the remoting channels
        try
        {
          string remotingFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
          // process the remoting configuration file
          RemotingConfiguration.Configure(remotingFile, false);
          

        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }

        // create the object reference and make the singleton instance available
        ObjRef objref = RemotingServices.Marshal(_controller, "TvControl", typeof(TvControl.IController));
        RemoteControl.Clear();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    void StopRemoting()
    {
      Log.WriteFile("TV service StopRemoting");
      try
      {
        if (_controller != null)
        {
          RemotingServices.Disconnect(_controller);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      Log.WriteFile("Remoting stopped");
    }
    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Log.WriteFile("Tvservice stopped due to a thread exception");
      Log.Write(e.Exception);
    }

    void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Log.WriteFile("Tvservice stopped due to a app domain exception {0}",e.ExceptionObject);
    }

  }
}
