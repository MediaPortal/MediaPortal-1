using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.Plugins.Base;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Services;

namespace Mediaportal.TV.Server.TVLibrary
{
  public class TvServiceThread : IPowerEventHandler
  {


    #region variables

    private Thread _tvServiceThread;
    private bool _priorityApplied;
    private readonly ManualResetEvent _tvServiceThreadEvt = new ManualResetEvent(false);
    private readonly EventWaitHandle _initializedEvent;
    private static bool _started;
    private readonly List<PowerEventHandler> _powerEventHandlers;
    private PluginLoader _plugins;
    private List<ITvServerPlugin> _pluginsStarted = new List<ITvServerPlugin>();
    private readonly string _applicationPath;

    #endregion

    public TvServiceThread(string applicationPath)
    {
      // Initialize hosting environment
      IntegrationProviderHelper.Register();

      // set working dir from application.exe
      _applicationPath = applicationPath;
      applicationPath = System.IO.Path.GetFullPath(applicationPath);
      applicationPath = System.IO.Path.GetDirectoryName(applicationPath);
      System.IO.Directory.SetCurrentDirectory(applicationPath);

      _powerEventHandlers = new List<PowerEventHandler>();
      GlobalServiceProvider.Instance.Add<IPowerEventHandler>(this);

      AddPowerEventHandler(OnPowerEventHandler);
      try
      {
        this.LogDebug("Setting up EventWaitHandle with name: {0}", RemoteControl.InitializedEventName);

        EventWaitHandleAccessRule rule =
          new EventWaitHandleAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                        EventWaitHandleRights.FullControl, AccessControlType.Allow);
        EventWaitHandleSecurity sec = new EventWaitHandleSecurity();
        sec.AddAccessRule(rule);
        bool eventCreated;
        _initializedEvent = new EventWaitHandle(false, EventResetMode.ManualReset, RemoteControl.InitializedEventName,
                                                out eventCreated, sec);
        if (!eventCreated)
        {
          this.LogInfo("{0} was not created", RemoteControl.InitializedEventName);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "");
      }
    }

    public static bool Started
    {
      get { return _started; }
    }

    public EventWaitHandle InitializedEvent
    {
      get { return _initializedEvent; }
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

    #region PowerEvent window handling

    private Thread _powerEventThread;
    private uint _powerEventThreadId;

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
    private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
                                                int x, int y,
                                                int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
                                                IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct WNDCLASS
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

    private IntPtr PowerEventThreadWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
      if (msg == WM_POWERBROADCAST)
      {
        this.LogDebug("TV service PowerEventThread received WM_POWERBROADCAST {0}", wParam.ToInt32());
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
      //this.LogDebug( "Service1.PowerEventThread started" );

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

        IntPtr handle = CreateWindowEx(0x80, wndclass.lpszClassName, "", 0x80000000, 0, 0, 0, 0, IntPtr.Zero,
                                       IntPtr.Zero, wndclass.hInstance, IntPtr.Zero);

        if (handle.Equals(IntPtr.Zero))
        {
          this.LogError("TV service PowerEventThread cannot create window handle, exiting thread");
          return;
        }

        // this thread needs an message loop
        this.LogDebug("TV service PowerEventThread message loop is running");
        while (true)
        {
          try
          {
            MSG msgApi = new MSG();

            if (!GetMessageA(ref msgApi, IntPtr.Zero, 0, 0)) // returns false on WM_QUIT
              return;

            TranslateMessage(ref msgApi);

            this.LogDebug("TV service PowerEventThread {0}", msgApi.message);


            DispatchMessageA(ref msgApi);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "TV service PowerEventThread");
          }
        }
      }
      finally
      {
        Thread.EndThreadAffinity();
        this.LogDebug("TV service PowerEventThread finished");
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
      this.LogDebug("OnPowerEvent: PowerStatus: {0}", powerStatus);

      bool accept = true;
      List<PowerEventHandler> powerEventPreventers = new List<PowerEventHandler>();
      List<PowerEventHandler> powerEventAllowers = new List<PowerEventHandler>();

      // Make a copy of _powerEventHandlers, because the handler might call AddPowerEventHandler
      // or RemovePowerEventHandler when executing thus generating an exception when we iterate.
      List<PowerEventHandler> listCopy = new List<PowerEventHandler>();
      foreach (PowerEventHandler handler in _powerEventHandlers)
      {
        listCopy.Add((PowerEventHandler)handler.Clone());
      }
      // Now iterate the 'copy'
      foreach (PowerEventHandler handler in listCopy)
      {
        bool result = handler(powerStatus);
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
      if (powerEventPreventers.Count > 0)
        foreach (PowerEventHandler handler in powerEventPreventers)
          this.LogDebug("PowerStatus:{0} rejected by {1}", powerStatus, handler.Target.ToString());

      // if query suspend: 
      // everybody that allowed the standby now must receive a deny event
      // since we will not get a QuerySuspendFailed message by the OS when
      // we return false to QuerySuspend
      if (powerStatus == PowerEventType.QuerySuspend ||
          powerStatus == PowerEventType.QueryStandBy)
      {
        foreach (PowerEventHandler handler in powerEventAllowers)
        {
          handler(powerStatus == PowerEventType.QuerySuspend
                    ? PowerEventType.QuerySuspendFailed
                    : PowerEventType.QueryStandByFailed);
        }
      }
      else if (powerStatus == PowerEventType.ResumeAutomatic ||
               powerStatus == PowerEventType.ResumeCritical ||
               powerStatus == PowerEventType.ResumeStandBy ||
               powerStatus == PowerEventType.ResumeSuspend
        )
      {
        ServiceManager.Instance.InternalControllerService.ExecutePendingDeletions();
        // call Recording-folder cleanup, just in case we have empty folders.
      }


      return false;
    }

    private bool OnPowerEventHandler(PowerEventType powerStatus)
    {
      this.LogDebug("OnPowerEventHandler: PowerStatus: {0}", powerStatus);

      switch (powerStatus)
      {
        case PowerEventType.StandBy:
        case PowerEventType.Suspend:
          ServiceManager.Instance.InternalControllerService.OnSuspend();
          return true;
        case PowerEventType.QuerySuspend:
        case PowerEventType.QueryStandBy:
          if (ServiceManager.Instance.InternalControllerService != null)
          {
            if (ServiceManager.Instance.InternalControllerService.CanSuspend)
            {
              //OnStop();
              return true;
            }
            return false;
          }
          return true;
        case PowerEventType.QuerySuspendFailed:
        case PowerEventType.QueryStandByFailed:
          if (!ServiceManager.Instance.InternalControllerService.EpgGrabberEnabled)
            ServiceManager.Instance.InternalControllerService.EpgGrabberEnabled = true;
          return true;
        case PowerEventType.ResumeAutomatic:
        case PowerEventType.ResumeCritical:
        case PowerEventType.ResumeSuspend:
          ServiceManager.Instance.InternalControllerService.OnResume();
          return true;
      }
      return true;
    }


    #endregion

    private void StartPlugins()
    {
      this.LogInfo("TV Service: Load plugins");

      _plugins = new PluginLoader();
      _plugins.Load();

      this.LogInfo("TV Service: Plugins loaded");
      // start plugins
      foreach (ITvServerPlugin plugin in _plugins.Plugins)
      {
        bool setting = SettingsManagement.GetValue(String.Format("plugin{0}", plugin.Name), false);
        if (setting)
        {
          if (plugin is ITvServerPluginCommunciation)
          {
            var tvServerPluginCommunciation = (plugin as ITvServerPluginCommunciation);
            Type interfaceType = tvServerPluginCommunciation.GetServiceInterfaceForContractType;
            object instance = tvServerPluginCommunciation.GetServiceInstance;

            ServiceManager.Instance.AddService(interfaceType, instance);
          }

          this.LogInfo("TV Service: Plugin: {0} started", plugin.Name);
          try
          {
            plugin.Start(ServiceManager.Instance.InternalControllerService);
            _pluginsStarted.Add(plugin);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "TV Service:  Plugin: {0} failed to start", plugin.Name);
          }
        }
        else
        {
          this.LogInfo("TV Service: Plugin: {0} disabled", plugin.Name);
        }
      }

      this.LogInfo("TV Service: Plugins started");

      // fire off startedAll on plugins
      foreach (ITvServerPlugin plugin in _pluginsStarted)
      {
        if (plugin is ITvServerPluginStartedAll)
        {
          this.LogInfo("TV Service: Plugin: {0} started all", plugin.Name);
          try
          {
            (plugin as ITvServerPluginStartedAll).StartedAll();
          }
          catch (Exception ex)
          {
            this.LogError(ex, "TV Service: Plugin: {0} failed to startedAll", plugin.Name);
          }
        }
      }
    }

    private void StopPlugins()
    {
      this.LogInfo("TV Service: Stop plugins");
      if (_pluginsStarted != null)
      {
        foreach (ITvServerPlugin plugin in _pluginsStarted)
        {
          try
          {
            plugin.Stop();
          }
          catch (Exception ex)
          {
            this.LogError(ex, "TV Service: plugin: {0} failed to stop", plugin.Name);
          }
        }
        _pluginsStarted = new List<ITvServerPlugin>();
      }
      this.LogInfo("TV Service: Plugins stopped");
    }



    private void DoStop()
    {
      //if (!Started)
      //  return;
      this.LogDebug("TV Service: stopping");

      if (InitializedEvent != null)
      {
        InitializedEvent.Reset();
      }
      if (ServiceManager.Instance.InternalControllerService != null)
      {
        ServiceManager.Instance.InternalControllerService.DeInit();
      }

      StopPlugins();
      if (_powerEventThreadId != 0)
      {
        this.LogDebug("TV Service: OnStop asking PowerEventThread to exit");
        PostThreadMessage(_powerEventThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        _powerEventThread.Join();
      }
      _powerEventThreadId = 0;
      _powerEventThread = null;
      _started = false;
      this.LogDebug("TV Service: stopped");
    }

    private void ApplyProcessPriority()
    {
      try
      {
        int processPriority = SettingsManagement.GetValue("processPriority", 3);

        switch (processPriority)
        {
          case 0:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            _tvServiceThread.Priority = ThreadPriority.AboveNormal;
            break;
          case 1:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            _tvServiceThread.Priority = ThreadPriority.AboveNormal;
            break;
          case 2:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            _tvServiceThread.Priority = ThreadPriority.AboveNormal;
            break;
          case 3:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            _tvServiceThread.Priority = ThreadPriority.Normal;
            break;
          case 4:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            _tvServiceThread.Priority = ThreadPriority.BelowNormal;
            break;
          case 5:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
            _tvServiceThread.Priority = ThreadPriority.Lowest;
            break;
          default:
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            _tvServiceThread.Priority = ThreadPriority.Normal;
            break;
        }
      }
      catch (Exception ex)
      {
        this.LogError("applyProcessPriority: exception is {0}", ex.StackTrace);
      }
    }

    public void Start(bool allowEnvExit = true)
    {
      var tvServiceThreadStart = new ThreadStart(() => {
          try
          {
            DoStart();
          }
          catch (OSPrerequisites.OSPrerequisites.OperationSystemException ex)
          {
            // Only exit the process if the caller forces this behavior.
            if (allowEnvExit)
              Environment.Exit(ex.ExitCode);
          }
        });
      _tvServiceThread = new Thread(tvServiceThreadStart) { IsBackground = false };


      // apply process priority on initial service start.
      if (!_priorityApplied)
      {
        try
        {
          ApplyProcessPriority();
          _priorityApplied = true;
        }
        catch (Exception ex)
        {
          this.LogError("OnStart: exception applying process priority: {0}", ex.StackTrace);
        }
      }

      _tvServiceThreadEvt.Reset();
      _tvServiceThread.Start();
    }

    public void Stop(int maxWaitMsecs)
    {
      _tvServiceThreadEvt.Set();

      if (_tvServiceThread != null && _tvServiceThread.IsAlive)
      {
        this.LogDebug("waiting for tvService to join...");
        bool joined = _tvServiceThread.Join(maxWaitMsecs);
        if (!joined)
        {
          this.LogDebug("aborting tvService thread.");
          _tvServiceThread.Abort();
          _tvServiceThread.Join();
          this.LogDebug("tvService thread aborted.");
        }
        _tvServiceThread = null;
      }
    }

    private void DoStart()
    {
      try
      {
        if (!Started)
        {
          this.LogInfo("TV service: Starting");

          Thread.CurrentThread.Name = "TVService";

          FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(_applicationPath);

          this.LogDebug("TVService v" + versionInfo.FileVersion + " is starting up on " +
                        OSInfo.OSInfo.GetOSDisplayVersion());

          //Check for unsupported operating systems
          OSPrerequisites.OSPrerequisites.OsCheck(false);

          _powerEventThread = new Thread(PowerEventThread) { Name = "PowerEventThread", IsBackground = true };
          _powerEventThread.Start();
          ServiceManager.Instance.InternalControllerService.Init();
          StartPlugins();

          _started = true;
          if (InitializedEvent != null)
          {
            InitializedEvent.Set();
          }
          this.LogInfo("TV service: Started");
          _tvServiceThreadEvt.WaitOne();
          DoStop();
        }
      }
      catch (Exception ex)
      {
        //wait for thread to exit. eg. when stopping tvservice       
        this.LogError(ex, "TvService OnStart failed");
        //_started = true; // otherwise the onstop code will not complete.
        DoStop();
        throw;
      }
    }
  }
}