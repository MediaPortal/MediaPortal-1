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
    TVController _controller;
    List<PowerEventHandler> _powerEventHandlers;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Service1"/> class.
    /// </summary>
    public Service1()
    {
      /* not needed anymore
      #region QuerySuspendHack the following hack allows to deny suspend queried in NET 2.0
        // Find the initialisation routine - may break in later versions

        MethodInfo init = typeof(ServiceBase).GetMethod("Initialize",
          BindingFlags.NonPublic | BindingFlags.Instance);

        // Call it to set up all members
        init.Invoke(this, new object[] { false });

        // Find the service callback handler
        FieldInfo handlerEx = typeof(ServiceBase).GetField("commandCallbackEx",
          BindingFlags.NonPublic | BindingFlags.Instance);

        // Read the base class provided handler
        _forward = (Delegate)handlerEx.GetValue(this);

        // Create a new delegate to our handler
        Delegate newDelegate= Delegate.CreateDelegate( _forward.GetType(), this, "ServiceCallbackEx" );

        // Install our handler
        handlerEx.SetValue(this, newDelegate );
      #endregion
      */

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
      Log.WriteFile("TV service starting");
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      Process currentProcess = Process.GetCurrentProcess();
      _powerEventThread = new Thread(PowerEventThread);
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
        _controller.Dispose();
        _controller = null;
      }

      if (_powerEventThreadId != 0 )
      {
        Log.Debug("TVService OnStop asking PowerEventThread to exit");
        PostThreadMessage(_powerEventThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        _powerEventThread.Join();
      }
      _powerEventThreadId = 0;
      _powerEventThread = null;

      GC.Collect();
      GC.Collect();
      GC.Collect();
      GC.Collect();
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
    private static extern IntPtr CreateWindowEx( uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y,
      int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
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

    [DllImport("user32", CharSet=CharSet.Auto)]
    private static extern int RegisterClass(ref WNDCLASS wndclass);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam );

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
      if( msg == WM_POWERBROADCAST )
      {
        switch (wParam.ToInt32())
        {
          case PBT_APMQUERYSUSPENDFAILED:
          case PBT_APMQUERYSTANDBYFAILED:
            OnPowerEventDo(PowerBroadcastStatus.QuerySuspendFailed);
            break;
          case PBT_APMQUERYSUSPEND:
          case PBT_APMQUERYSTANDBY:
            if( !OnPowerEventDo(PowerBroadcastStatus.QuerySuspend))
              return new IntPtr(BROADCAST_QUERY_DENY);
            break;
        }
      }
      return DefWindowProc(hWnd,msg,wParam,lParam);
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

        IntPtr handle= CreateWindowEx(0x80, wndclass.lpszClassName, "", 0x80000000, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, wndclass.hInstance, IntPtr.Zero);

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

            Log.Debug("TV service PowerEventThread {0}", msgApi.message);

            TranslateMessage(ref msgApi);
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

    /* not needed anymore!
    #region QuerySuspendHack part 2 of the hack
      Delegate _forward;  // will get the default delegate

      /// <summary>
      ///   This is the hack stub function that allows to deny supend queries.
      /// </summary>
      /// <param name="command"></param>
      /// <param name="eventType"></param>
      /// <param name="eventData"></param>
      /// <param name="eventContext"></param>
      /// <returns></returns>
      private int ServiceCallbackEx(int command, int eventType, IntPtr eventData,
        IntPtr eventContext)
      {
        // Call the base class implementation which is fine for all but power and session management

        if (13 != command) return (int) _forward.DynamicInvoke(command, eventType, eventData, eventContext);

        // Process and forward success code
        if (OnPowerEvent((PowerBroadcastStatus)eventType)) return 0;

        // Abort power operation
        return 0x424d5144;
      }
    #endregion
    */

    /// <summary>
    /// When implemented in a derived class, executes when the computer's power status has changed. This applies to laptop computers when they go into suspended mode, which is not the same as a system shutdown.
    /// </summary>
    /// <param name="powerStatus">A <see cref="T:System.ServiceProcess.PowerBroadcastStatus"></see> that indicates a notification from the system about its power status.</param>
    /// <returns>
    /// When implemented in a derived class, the needs of your application determine what value to return. For example, if a QuerySuspend broadcast status is passed, you could cause your application to reject the query by returning false.
    /// </returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
    {
      if (powerStatus != PowerBroadcastStatus.QuerySuspend && powerStatus != PowerBroadcastStatus.QuerySuspendFailed)
        return OnPowerEventDo(powerStatus);
      return true;
    }
    

    /// <summary>
    /// Handles the power event.
    /// </summary>
    /// <param name="powerStatus"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    protected bool OnPowerEventDo(PowerBroadcastStatus powerStatus)
    {
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
      result = base.OnPowerEvent(powerStatus);
      if (result == false)
        accept = false;
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
        if (powerStatus == PowerBroadcastStatus.QuerySuspend )
        {
          if( result )
            base.OnPowerEvent(PowerBroadcastStatus.QuerySuspendFailed);
          foreach (PowerEventHandler handler in powerEventAllowers)
          {
            handler(PowerBroadcastStatus.QuerySuspendFailed);
          }
        }

        return false;
      }
    }

    private bool OnPowerEventHandler(PowerBroadcastStatus powerStatus)
    {
      switch (powerStatus)
      {
        case PowerBroadcastStatus.QuerySuspend:
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
        case PowerBroadcastStatus.QuerySuspendFailed:
          if (!_controller.EpgGrabberEnabled)
            _controller.EpgGrabberEnabled = true;
          return true;
        case PowerBroadcastStatus.ResumeAutomatic:
        case PowerBroadcastStatus.ResumeCritical:
        case PowerBroadcastStatus.ResumeSuspend:
          //OnStart(null);
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
      Log.WriteFile("Tvservice stopped due to a app domain exception {0}",e.ExceptionObject);
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
