/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
 * todo:
 *     - settings
 *     - conflict management
 *     - radio?
 *     - disable cards
 *     - hybrid cards
 * test:
 *     - master/slave
 *     - streaming
 *     - atsc
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Xml;
using TvDatabase;
using TvLibrary.Log;
using TvControl;
using TvEngine.Interfaces;
using TvLibrary.Interfaces;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TvService
{
  public partial class Service1 : ServiceBase, IPowerEventHandler
  {
    #region variables
    bool _started = false;
    bool _priorityApplied = false;
    TVController _controller;
    List<PowerEventHandler> _powerEventHandlers;
    private bool _reinitializeController = false;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Service1"/> class.
    /// </summary>
    public Service1()
    {
      string applicationPath = System.Windows.Forms.Application.ExecutablePath;
      applicationPath = System.IO.Path.GetFullPath(applicationPath);
      applicationPath = System.IO.Path.GetDirectoryName(applicationPath);
      System.IO.Directory.SetCurrentDirectory(applicationPath);
      _powerEventHandlers = new List<PowerEventHandler>();
      GlobalServiceProvider.Instance.Add<IPowerEventHandler>(this);
      AddPowerEventHandler(new PowerEventHandler(this.OnPowerEventHandler));
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

      InitializeComponent();
    }

    public void DoStart(string[] args)
    {
      OnStart(args);
    }
    /// <summary>
    /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
    /// </summary>
    /// <param name="args">Data passed by the start command.</param>
    protected override void OnStart(string[] args)
    {
      if (_started)
        return;

      // apply process priority on initial service start.
      if (!_priorityApplied)
      {
        try
        {
          RequestAdditionalTime(60000);  // starting database can be slow so increase default timeout
          applyProcessPriority();
          _priorityApplied = true;
        }
        catch (Exception)
        {
          // applyProcessPriority can generate an exception when we cannot connect to the database
        }
      }
      Thread.CurrentThread.Name = "TVService";

      OsDetection.OSVersionInfo os = new OsDetection.OperatingSystemVersion();
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      string ServicePack = "";
      if (!String.IsNullOrEmpty(os.OSCSDVersion))
        ServicePack = " (" + os.OSCSDVersion + ")";
      Log.WriteFile("TVService v" + versionInfo.FileVersion + " is starting up on " + os.OSVersionString + ServicePack);

      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      Process currentProcess = Process.GetCurrentProcess();
      _powerEventThread = new Thread(PowerEventThread);
      _powerEventThread.Name = "PowerEventThread";
      _powerEventThread.IsBackground = true;
      _powerEventThread.Start();
      //currentProcess.PriorityClass = ProcessPriorityClass.High;
      _controller = new TVController();
      try
      {
        System.IO.Directory.CreateDirectory("pmt");
      }
      catch (Exception)
      {
      }
      StartRemoting();
      Utils.ShutDownMCEServices();
      _started = true;
      Log.WriteFile("TV service started");
    }


    /// <summary>
    /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
    /// </summary>
    protected override void OnStop()
    {
      if (!_started)
        return;

      Log.WriteFile("TV service stopping");

      StopRemoting();
      RemoteControl.Clear();
      if (_controller != null)
      {
        _controller.DeInit();
        //_controller.Dispose(); dispose is already calling deinit, so why bother ?
        _controller = null;
      }

      if (_powerEventThreadId != 0)
      {
        Log.Debug("TVService OnStop asking PowerEventThread to exit");
        PostThreadMessage(_powerEventThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        _powerEventThread.Join();
      }
      _powerEventThreadId = 0;
      _powerEventThread = null;

      if (!Environment.HasShutdownStarted)
      {
        Utils.RestartMCEServices();
        GC.Collect();
        GC.Collect();
        GC.Collect();
        GC.Collect();
      }
      _started = false;
      Log.WriteFile("TV service stopped");
    }

    #region PowerEvent window handling

    Thread _powerEventThread;
    uint _powerEventThreadId;
    
    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
      public IntPtr hwnd;
      public int message;
      public IntPtr wParam;
      public IntPtr lParam;
      public int time;
      public int pt_x;
      public int pt_y;
    }

    [DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern bool GetMessageA([In, Out] ref MSG msg, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern bool TranslateMessage([In, Out] ref MSG msg);

    [DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern IntPtr DispatchMessageA([In] ref MSG msg);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y,
      int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct WNDCLASS
    {
      public uint style;
      public WndProc lpfnWndProc;
      public int cbClsExtra;
      public int cbWndExtra;
      public IntPtr hInstance;
      public IntPtr hIcon;
      public IntPtr hCursor;
      public IntPtr hbrBackground;
      public string lpszMenuName;
      public string lpszClassName;
    }

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int RegisterClass(ref WNDCLASS wndclass);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    #region WndProc message constants
    private const int WM_QUIT = 0x0012;
    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMQUERYSUSPEND = 0x0000;
    private const int PBT_APMQUERYSTANDBY = 0x0001;
    private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
    private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
    private const int PBT_APMSUSPEND = 0x0004;
    private const int PBT_APMSTANDBY = 0x0005;
    private const int PBT_APMRESUMECRITICAL = 0x0006;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    private const int BROADCAST_QUERY_DENY = 0x424D5144;
    #endregion


    IntPtr PowerEventThreadWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
      if (msg == WM_POWERBROADCAST)
      {
        Log.Debug("TV service PowerEventThread received WM_POWERBROADCAST {1}", wParam.ToInt32());
        switch (wParam.ToInt32())
        {
          case PBT_APMQUERYSUSPENDFAILED:
            OnPowerEvent(PowerEventType.QuerySuspendFailed);
            break;
          case PBT_APMQUERYSTANDBYFAILED:
            OnPowerEvent(PowerEventType.QueryStandByFailed);
            break;
          case PBT_APMQUERYSUSPEND:
            if (!OnPowerEvent(PowerEventType.QuerySuspend))
              return new IntPtr(BROADCAST_QUERY_DENY);
            break;
          case PBT_APMQUERYSTANDBY:
            if (!OnPowerEvent(PowerEventType.QueryStandBy))
              return new IntPtr(BROADCAST_QUERY_DENY);
            break;
          case PBT_APMSUSPEND:
            OnPowerEvent(PowerEventType.Suspend);
            break;
          case PBT_APMSTANDBY:
            OnPowerEvent(PowerEventType.StandBy);
            break;
          case PBT_APMRESUMECRITICAL:
            OnPowerEvent(PowerEventType.ResumeCritical);
            break;
          case PBT_APMRESUMESUSPEND:
            OnPowerEvent(PowerEventType.ResumeSuspend);
            break;
          case PBT_APMRESUMESTANDBY:
            OnPowerEvent(PowerEventType.ResumeStandBy);
            break;
          case PBT_APMRESUMEAUTOMATIC:
            OnPowerEvent(PowerEventType.ResumeAutomatic);
            break;
        }
      }
      return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void PowerEventThread()
    {
      //Log.Debug( "Service1.PowerEventThread started" );

      Thread.BeginThreadAffinity();
      try
      {
        _powerEventThreadId = GetCurrentThreadId();

        WNDCLASS wndclass;
        wndclass.style = 0;
        wndclass.lpfnWndProc = PowerEventThreadWndProc;
        wndclass.cbClsExtra = 0;
        wndclass.cbWndExtra = 0;
        wndclass.hInstance = Process.GetCurrentProcess().Handle;
        wndclass.hIcon = IntPtr.Zero;
        wndclass.hCursor = IntPtr.Zero;
        wndclass.hbrBackground = IntPtr.Zero;
        wndclass.lpszMenuName = null;
        wndclass.lpszClassName = "TVServicePowerEventThreadWndClass";

        RegisterClass(ref wndclass);

        IntPtr handle = CreateWindowEx(0x80, wndclass.lpszClassName, "", 0x80000000, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, wndclass.hInstance, IntPtr.Zero);

        if (handle.Equals(IntPtr.Zero))
        {
          Log.Error("TV service PowerEventThread cannot create window handle, exiting thread");
          return;
        }
        else
        {
          //Log.Debug("TV service PowerEventThread window handle created");
        }

        // this thread needs an message loop
        Log.Debug("TV service PowerEventThread message loop is running");
        while (true)
        {
          try
          {
            MSG msgApi = new MSG();

            if (!GetMessageA(ref msgApi, IntPtr.Zero, 0, 0)) // returns false on WM_QUIT
              return;

            TranslateMessage(ref msgApi);

            Log.Debug("TV service PowerEventThread {0}", msgApi.message);


            DispatchMessageA(ref msgApi);
          }
          catch (Exception ex)
          {
            Log.Error("TV service PowerEventThread: Exception: {0}", ex.ToString());
          }
        }
      }
      finally
      {
        Thread.EndThreadAffinity();
        Log.Debug("TV service PowerEventThread finished");
      }
    }
    #endregion


    /// <summary>
    /// Handles the power event.
    /// </summary>
    /// <param name="powerStatus"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    protected bool OnPowerEvent(PowerEventType powerStatus)
    {
      Log.Debug("OnPowerEvent: PowerStatus: {0}", powerStatus);

      bool accept = true;
      bool result;
      List<PowerEventHandler> powerEventPreventers = new List<PowerEventHandler>();
      List<PowerEventHandler> powerEventAllowers = new List<PowerEventHandler>();
      foreach (PowerEventHandler handler in _powerEventHandlers)
      {
        result = handler(powerStatus);
        if (result == false)
        {
          accept = false;
          powerEventPreventers.Add(handler);
        }
        else
          powerEventAllowers.Add(handler);
      }
      if (accept)
        return true;
      else
      {
        if (powerEventPreventers.Count > 0)
          foreach (PowerEventHandler handler in powerEventPreventers)
            Log.Debug("PowerStatus:{0} rejected by {1}", powerStatus, handler.Target.ToString());

        // if query suspend: 
        // everybody that allowed the standby now must receive a deny event
        // since we will not get a QuerySuspendFailed message by the OS when
        // we return false to QuerySuspend
        if (powerStatus == PowerEventType.QuerySuspend ||
          powerStatus == PowerEventType.QueryStandBy)
        {
          foreach (PowerEventHandler handler in powerEventAllowers)
          {
            handler(powerStatus == PowerEventType.QuerySuspend ? PowerEventType.QuerySuspendFailed : PowerEventType.QueryStandByFailed);
          }
        }

        return false;
      }
    }

    void GetDatabaseConnectionString(out string connectionString, out string provider)
    {
      connectionString = "";
      provider = "";
      try
      {
        string fname = String.Format(@"{0}\gentle.config", Log.GetPathName());
        try
        {
          System.IO.File.Copy(fname, "gentle.config", true);
        }
        catch (Exception ex1)
        {
          Log.Write(ex1);
        }
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\gentle.config", Log.GetPathName()));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode nodeConnection = nodeKey.Attributes.GetNamedItem("connectionString"); ;
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name"); ;
        connectionString = nodeConnection.InnerText;
        provider = nodeProvider.InnerText;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    private void applyProcessPriority()
    {
      try
      {
        string connectionString, provider;
        GetDatabaseConnectionString(out connectionString, out provider);
        string ConnectionLog = connectionString.Remove(connectionString.IndexOf(@"Password=") + 8);
        // Log.Info("TVService: using {0} database connection: {1}", provider, ConnectionLog);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);

        TvDatabase.TvBusinessLayer layer = new TvDatabase.TvBusinessLayer();
        int processPriority = Convert.ToInt32(layer.GetSetting("processPriority", "3").Value);

        switch (processPriority)
        {
          case 0:
            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            break;
          case 1:
            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            break;
          case 2:
            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
            break;
          case 3:
            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
          case 4:
            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            break;
          case 5:
            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Idle;
            break;
          default:
            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
        }
      }
      catch (Exception)
      {
      }

    }

    private bool OnPowerEventHandler(PowerEventType powerStatus)
    {
      Log.Debug("OnPowerEventHandler: PowerStatus: {0}", powerStatus);
      
      switch (powerStatus)
      {
        case PowerEventType.StandBy:
        case PowerEventType.Suspend:
          if ((!_reinitializeController) && (_controller != null))
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            bool bSetting = Convert.ToBoolean(layer.GetSetting("PowerSchedulerReinitializeController", "false").Value);
            if (bSetting)
            {
              _reinitializeController = true;
              Log.Debug("OnPowerEventHandler: Stopping Controller");
              _controller.DeInit();
            }
          }
          return true;        
        case PowerEventType.QuerySuspend:
        case PowerEventType.QueryStandBy:
          if (_controller != null)
          {
            if (_controller.CanSuspend)
            {
              //OnStop();
              return true;
            }
            else
            {
              return false;
            }
          }
          return true;
        case PowerEventType.QuerySuspendFailed:
        case PowerEventType.QueryStandByFailed:
          if (!_controller.EpgGrabberEnabled)
            _controller.EpgGrabberEnabled = true;
          return true;
        case PowerEventType.ResumeAutomatic:
        case PowerEventType.ResumeCritical:
        case PowerEventType.ResumeSuspend:
          //OnStart(null);
          if ((_reinitializeController) && (_controller != null))
          {
            _reinitializeController = false;
            Log.Debug("OnPowerEventHandler: Starting Controller");
            _controller.Restart();
          }
        
          return true;
      }
      return true;
    }

    /// <summary>
    /// Starts the remoting interface
    /// </summary>
    void StartRemoting()
    {
      try
      {
        // create the object reference and make the singleton instance available
        ObjRef objref = RemotingServices.Marshal(_controller, "TvControl", typeof(TvControl.IController));
        RemoteControl.Clear();

      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    /// <summary>
    /// Stops the remoting interface
    /// </summary>
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

    /// <summary>
    /// Handles the UnhandledException event of the CurrentDomain control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
    void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Log.WriteFile("Tvservice stopped due to a app domain exception {0}", e.ExceptionObject);
    }

    #region IPowerEventHandler implementation
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void AddPowerEventHandler(PowerEventHandler handler)
    {
      _powerEventHandlers.Add(handler);
    }
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void RemovePowerEventHandler(PowerEventHandler handler)
    {
      lock (_powerEventHandlers)
        _powerEventHandlers.Remove(handler);
    }
    #endregion

  }
}
