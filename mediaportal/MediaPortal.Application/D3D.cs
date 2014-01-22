#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

#region usings

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Profile;
using MediaPortal.Properties;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Microsoft.DirectX.Direct3D;
using WPFMediaKit.DirectX;

#endregion

namespace MediaPortal
{
  /// <summary>
  /// The base class for all the graphics (D3D) samples, it derives from windows forms
  /// </summary>
  public class D3D : MPForm
  {
    #region Win32 Imports

    // http://msdn.microsoft.com/en-us/library/windows/desktop/dd145065(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInformation
    {
      public uint Size;
      public Rectangle MonitorRectangle;
      public Rectangle WorkRectangle;
      public uint Flags;
    }

    // http://msdn.microsoft.com/en-us/library/windows/desktop/dd144901(v=vs.85).aspx
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    internal static extern bool GetMonitorInfo(IntPtr hWnd, ref MonitorInformation info);

    // http://msdn.microsoft.com/en-us/library/windows/desktop/ms648396(v=vs.85).aspx
    [DllImport("user32.dll")]
    static extern int ShowCursor(bool bShow);

    // http://msdn.microsoft.com/en-us/library/windows/desktop/ms633505(v=vs.85).aspx
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    #endregion

    #region constants

    // ReSharper disable InconsistentNaming
    private const int D3DSWAPEFFECT_FLIPEX = 5;
    private const int numberOfRetries = 20;
    private const int numberOfRetriesEnum = 20;
    private const int numberOfRetriesAdaptor = 20;
    private const int delayBetweenTries = 5000;
    private static int retries = 0;
    private static bool successful = false;
    private static bool successfulInit = false;
    // ReSharper restore InconsistentNaming

    #endregion

    #region protected structs

    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable InconsistentNaming
    protected struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }
    // ReSharper restore InconsistentNaming

    #endregion

    #region protected attributes

    protected internal static int  ScreenNumberOverride;     // 0 or higher means it is set
    protected internal static bool FullscreenOverride;       // full screen mode overridden by command line argument?
    protected internal static bool WindowedOverride;         // window mode overridden by command line argument?
    protected static string        SkinOverride;             // skin overridden by command line argument
    protected string               FrameStatsLine1;          // 1st string to hold frame stats
    protected string               FrameStatsLine2;          // 2nd string to hold frame stats
    protected bool                 MinimizeOnStartup;        // minimize to tray on startup?
    protected bool                 MinimizeOnGuiExit;        // minimize to tray on GUI exit?
    protected bool                 MinimizeOnFocusLoss;      // minimize to tray when focus in full screen mode is lost?
    protected bool                 ShuttingDown;             // set to true if MP is shutting down
    protected bool                 AutoHideMouse;            // Should the mouse cursor be hidden automatically?
    protected bool                 AppActive;                // set to true while MP is active     
    protected bool                 MouseCursor;              // holds the current mouse cursor state
    protected bool                 Windowed;                 // are we in windowed mode?
    protected bool                 AutoHideTaskbar;          // Should the Task Bar be hidden?
    protected bool                 IsVisible;                // set to true if form is not minimized to tray
    protected bool                 IsInAwayMode;             // indicates if away mode is active, assume no on application launch
    protected bool                 IsDisplayTurnedOn;        // indicates if the display is turned on, assume yes on application launch
    protected bool                 IsUserPresent;            // indicates if a user is present, assume yes on application launch
    protected bool                 UseEnhancedVideoRenderer; // should EVR be used?
    protected bool                 ExitToTray;               //
    protected int                  Frames;                   // number of frames since our last update
    protected static int           Volume;                   // used to save old volume level in case we mute audio
    protected PlayListPlayer       PlaylistPlayer;           // 
    protected DateTime             MouseTimeOutTimer;        // tracks the time of the last mouse activity
    protected RECT                 LastRect;                 // tracks last rectangle size for window resizing
    protected Point                LastCursorPosition;       // tracks last cursor position during window moving
    protected static SplashScreen  SplashScreen;             // splash screen object
    protected GraphicsAdapterInfo  AdapterInfo;              // hold adapter info for the selected display on startup of MP
    protected int MouseTimeOutMP;                            // Mouse activity timeout while in MP in seconds
    protected int MouseTimeOutFullscreen;                    // Mouse activity timeout while in Fullscreen in seconds

    #endregion

    #region private attributes

    private readonly Control           _renderTarget;             // render target object
    private readonly PresentParameters _presentParams;            // D3D presentation parameters
    internal D3DEnumeration            _enumerationSettings;      //
    private readonly bool              _useExclusiveDirectXMode;  // 
    private readonly bool              _disableMouseEvents;       //
    private readonly bool              _doNotWaitForVSync;        // debug setting
    private readonly bool              _showCursorWhenFullscreen; // should the mouse cursor be shown in full screen?
    private readonly bool              _reduceFrameRate;          // reduce frame rate when not in focus?
    private bool                       _miniTvMode;               // 
    private bool                       _isClosing;                //
    private bool                       _isLoaded;                 //
    private bool                       _lastMouseCursor;          // holds the last mouse cursor state to keep state balance
    private bool                       _lostFocus;                // set to true if form lost focus
    private bool                       _wasPlayingVideo;          //
    private bool                       _alwaysOnTop;              // tracks the always on top state
    private bool                       _firstTimeWindowDisplayed; // set to true when MP becomes Active the 1st time
    private bool                       _firstTimeActivated;       // needed to focus on first start
    private int                        _lastActiveWindow;         //
    private long                       _lastTime;                 //
    private double                     _currentPlayerPos;         //
    private string                     _currentFile;              //
    private Rectangle                  _oldClientRectangle;       // last size of the MP window
    private IContainer                 _components;               //
    private MainMenu                   _menuStripMain;            // menu
    private MenuItem                   _menuItemFile;             // sub menu
    private MenuItem                   _menuItemOptions;          // sub menu
    private MenuItem                   _menuItemWizards;          // sub menu
    private MenuItem                   _menuItemExit;             // menu item
    private MenuItem                   _menuItemConfiguration;    // menu item
    private MenuItem                   _menuItemDVD;              // menu item
    private MenuItem                   _menuItemMovies;           // menu item
    private MenuItem                   _menuItemMusic;            // menu item
    private MenuItem                   _menuItemPictures;         // menu item
    private MenuItem                   _menuItemTV;               // menu item
    private MenuItem                   _menuItemContext;          // menu item
    private MenuItem                   _menuItemEmpty;            // menu item
    private MenuItem                   _menuItemFullscreen;       // menu item
    private MenuItem                   _menuItemMiniTv;           // menu item
    private NotifyIcon                 _notifyIcon;               // tray icon object
    private ContextMenu                _contextMenu;              //
    private PlayListType               _currentPlayListType;      //
    private PlayList                   _currentPlayList;          //
    private Win32API.MSG               _msgApi;                   //
    internal static Point              _lastCursorPosition;       // track cursor position of last move move event
    private DeviceType                 _deviceType;               //
    private CreateFlags                _createFlags;              //
    internal static Point              _moveMouseCursorPosition;
    internal static Point              _moveMouseCursorPositionRefresh;
    protected static bool              _firstLoadedScreen;        //

    #endregion

    #region constructor

    /// <summary>
    /// Constructor
    /// </summary>
    protected D3D()
    {
      _firstTimeWindowDisplayed  = true;
      _firstTimeActivated       = true;
      MinimizeOnStartup         = false;
      MinimizeOnGuiExit         = false;
      MinimizeOnFocusLoss       = false;
      ShuttingDown              = false;
      AutoHideMouse             = false;
      MouseCursor               = true;
      Windowed                  = true;
      Volume                    = -1;
      AppActive                 = false;
      KeyPreview                = true;
      Frames                    = 0;
      FrameStatsLine1           = null;
      FrameStatsLine2           = null;
      Text                      = Resources.D3DApp_NotifyIcon_MediaPortal;
      PlaylistPlayer            = PlayListPlayer.SingletonPlayer;
      MouseTimeOutTimer         = DateTime.Now;
      _lastActiveWindow         = -1;
      IsVisible                 = true;
      IsDisplayTurnedOn         = true;
      IsInAwayMode              = false;
      IsUserPresent             = true;
      _lastMouseCursor          = !MouseCursor;
      _showCursorWhenFullscreen = false;
      _currentPlayListType      = PlayListType.PLAYLIST_NONE;
      _enumerationSettings      = new D3DEnumeration();
      _presentParams            = new PresentParameters();
      _renderTarget             = this;

      using (Settings xmlreader = new MPSettings())
      {
        _useExclusiveDirectXMode = xmlreader.GetValueAsBool("general", "exclusivemode", true);
        UseEnhancedVideoRenderer = xmlreader.GetValueAsBool("general", "useEVRenderer", false);
        _disableMouseEvents      = xmlreader.GetValueAsBool("remote", "CentareaJoystickMap", false);
        AutoHideTaskbar          = xmlreader.GetValueAsBool("general", "hidetaskbar", true);
        _alwaysOnTop             = xmlreader.GetValueAsBool("general", "alwaysontop", false);
        _reduceFrameRate         = xmlreader.GetValueAsBool("gui", "reduceframerate", false);
        _doNotWaitForVSync       = xmlreader.GetValueAsBool("debug", "donotwaitforvsync", false);
      }

      _useExclusiveDirectXMode = !UseEnhancedVideoRenderer && _useExclusiveDirectXMode;
      GUIGraphicsContext.IsVMR9Exclusive = _useExclusiveDirectXMode;
      GUIGraphicsContext.IsEvr = UseEnhancedVideoRenderer;
      
      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    public override sealed string Text
    {
      get
      {
        return base.Text;
      }
      set
      {
        base.Text = value;
      }
    }

    #endregion

    #region protected methods
    
    /// <summary>
    /// 
    /// </summary>
    protected virtual void InitializeDeviceObjects() { }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnDeviceLost(Object sender, EventArgs e) { }


    /// <summary>
    /// 
    /// </summary>
    protected virtual void FrameMove() { }


    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnProcess() { }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="timePassed"></param>
    protected virtual void Render(float timePassed) { }


    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnStartup() { }


    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnExit() { }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnEnumeration()
    {
      _enumerationSettings = new D3DEnumeration();
      int enumIntCount = 0;
      bool ConfirmDeviceCheck = false;

      // get display adapter info
      while (ConfirmDeviceCheck != true && enumIntCount < numberOfRetriesEnum)
      {
        try
        {
          Log.Debug("D3D: Starting and find Enumeration Settings - retry {0}", enumIntCount);
          _enumerationSettings.ConfirmDeviceCallback = ConfirmDevice;
          try
          {
            _enumerationSettings.Enumerate();
            ConfirmDeviceCheck = true;
          }
          catch (Exception ex)
          {
            Log.Error("D3D: failed to _enumerationSettings exception {0}", ex);
            _enumerationSettings = new D3DEnumeration();
          }
        }
        catch (Exception ex)
        {
          Log.Error("D3D: Starting and find _enumerationSettings exception {0}", ex);
          _enumerationSettings = new D3DEnumeration();
        }
        enumIntCount++;
      }
    }

    /// <summary>
    /// Init graphics device
    /// </summary>
    /// <returns>true if a good device was found, false otherwise</returns>
    /// 
    protected bool Init()
    {
      Log.Debug("D3D: Init()");

      // Reset Adapter
      AdapterInfo = null;

      // log information about available adapters
      var enumeration = new D3DEnumeration();
      enumeration.Enumerate();
      foreach (GraphicsAdapterInfo ai in enumeration.AdapterInfoList)
      {
        Log.Debug("D3D: Init Adapter #{0}: {1} - Driver: {2} ({3}) - DeviceName: {4}",
          ai.AdapterOrdinal, ai.AdapterDetails.Description, ai.AdapterDetails.DriverName, ai.AdapterDetails.DriverVersion, ai.AdapterDetails.DeviceName);
      }

      // Set up cursor
      _renderTarget.Cursor = Cursors.Default;

      // if our render target is the main window and we haven't said ignore the menus, add our menu
      if (Windowed)
      {
        Menu = _menuStripMain;
      }

      // Initialize the application timer
      DXUtil.Timer(DirectXTimer.Start);

      int adapIntCount = 0;

      // get display adapter info
      OnEnumeration();

      // Reset counter
      adapIntCount = 0;

      while (AdapterInfo == null && adapIntCount < numberOfRetriesAdaptor)
      {
        try
        {
          Log.Debug("D3D: Starting and find Adapter info - retry #{0}", adapIntCount);
          adapIntCount++;
          AdapterInfo = FindAdapterForScreen(GUIGraphicsContext.currentScreen);
          if (AdapterInfo == null)
          {
            OnEnumeration();
          }
          if (AdapterInfo != null)
          {
            Log.Debug("D3D: Starting and find Adapter #{0}: {1} - retry #{2}", AdapterInfo.AdapterOrdinal, AdapterInfo, adapIntCount);
          }
          else
          {
            Log.Debug("D3D: Adapter info is not detected - retry #{0}", adapIntCount);
          }
        }
        catch (Exception ex)
        {
          Log.Error("D3D: Starting and find AdapterInfo exception {0}", ex);
          AdapterInfo = null;
        }
      }
      
      if (!Windowed)
      {
        Log.Info("D3D: Starting in full screen");

        if (AutoHideTaskbar && !MinimizeOnStartup)
        {
          HideTaskBar(true);
        }
        
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox     = false;
        MinimizeBox     = false;
        Menu            = null;

        var newBounds  = GUIGraphicsContext.currentScreen.Bounds;
        Bounds         = newBounds;
        ClientSize     = newBounds.Size;
        Log.Info("D3D: Client size: {0}x{1} - Screen: {2}x{3}",
                 ClientSize.Width, ClientSize.Height,
                 GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
      }

      if (!successful)
      {
        // Initialize D3D Device
        InitializeDevice();
      }

      return true;
    }


    /// <summary>
    /// Hides the task bar
    /// </summary>
    /// <param name="hide">hides taskbar on true, shows it on false</param>
    protected static void HideTaskBar(bool hide)
    {
      Log.Info(hide ? "D3D: Hiding Taskbar" : "D3D: Showing Taskbar");
      Win32API.EnableStartBar(!hide);
      Win32API.ShowStartBar(!hide);
    }


    /// <summary>
    /// 
    /// </summary>
    protected void ToggleFullscreen()
    {
      Log.Debug("D3D: ToggleFullScreen()");

      // disable event handlers
      GUIGraphicsContext.DX9Device.DeviceLost -= OnDeviceLost;

      // Reset DialogMenu to avoid freeze when going to fullscreen/windowed
      var dialogMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dialogMenu != null && (GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_DIALOG_MENU || GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_DIALOG_OK))
      {
        dialogMenu.Dispose();
        GUIWindowManager.UnRoute(); // only unroute if we still the routed window
      }

      // reset device if necessary
      Windowed = !Windowed;
      RecreateSwapChain();
      Windowed = !Windowed;

      // adjust form sizes and properties
      if (Windowed)
      {
        Log.Info("D3D: Switching from windowed mode to full screen");
        
        if (AutoHideTaskbar)
        {
          HideTaskBar(true);
        }

        // exist miniTVMode
        if (_menuItemMiniTv.Checked)
        {
          ToggleMiniTV();
        }

        _oldClientRectangle.Location = Location;
        _oldClientRectangle.Size     = ClientSize;

        WindowState         = FormWindowState.Normal;
        FormBorderStyle     = FormBorderStyle.None;
        MaximizeBox         = false;
        MinimizeBox         = false;
        Menu                = null;
        Windowed            = false;
        Location            = new Point(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y);
        ClientSize          = GUIGraphicsContext.currentScreen.Bounds.Size;
      }
      else
      {
        Log.Info("D3D: Switching from full screen to windowed mode");

        if (AutoHideTaskbar)
        {
          HideTaskBar(false);
        }

        WindowState     = FormWindowState.Normal;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox     = true;
        MinimizeBox     = true;
        Menu            = _menuStripMain;
        Windowed        = true;

        if (_oldClientRectangle.IsEmpty)
        {
          Location   = new Point(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y);
          ClientSize = CalcMaxClientArea();
        }
        else
        {
          Location   = _oldClientRectangle.Location;
          ClientSize = _oldClientRectangle.Size;
        }

        LastRect.top    = Location.Y;
        LastRect.left   = Location.X;
        LastRect.bottom = Size.Height;
        LastRect.right  = Size.Width;
      }

      Update();
      Log.Info("D3D: Client Size: {0}x{1}", ClientSize.Width, ClientSize.Height);
      Log.Info("D3D: Screen size: {0}x{1}", GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);

      // enable event handlers
      GUIGraphicsContext.DX9Device.DeviceLost += OnDeviceLost;
    }


    /// <summary>
    /// reset device if back buffer does not match skin dimensions (e.g. 16:10 display with 16:9 skin, 720p skin on 1080p display etc.)
    /// </summary>
    internal void RecreateSwapChain()
    {
      if (AppActive)
      {
        Log.Debug("Main: RecreateSwapChain()");

        // stop plaback if we are using a a D3D9 device and the device is not lost, meaning we are toggling between fullscreen and windowed mode
        if (!GUIGraphicsContext.IsDirectX9ExUsed() && g_Player.Playing && !RefreshRateChanger.RefreshRateChangePending)
        {
          g_Player.Stop();
          while (GUIGraphicsContext.IsPlaying)
          {
            Thread.Sleep(100);
          }
        }

        // halt rendering
        AppActive = false;
        int activeWin = GUIWindowManager.ActiveWindow;

        // stop window manager and dispose resources
        GUIWindowManager.UnRoute();
        GUIWindowManager.Dispose();
        GUIFontManager.Dispose();
        GUITextureManager.Dispose();
        GUIGraphicsContext.DX9Device.EvictManagedResources();

        // build new D3D presentation parameters and reset device
        BuildPresentParams(Windowed);
        try
        {
          GUIGraphicsContext.DX9Device.Reset(_presentParams);
        }
        catch (InvalidCallException)
        {
          Log.Error("D3D: D3DERR_INVALIDCALL - presentation parametters might contain an invalid value");
        }
        catch (DeviceLostException)
        {
          Log.Error("D3D: D3DERR_DEVICELOST - device is lost but cannot be reset at this time");
        }
        catch (DriverInternalErrorException)
        {
          Log.Error("D3D: D3DERR_DRIVERINTERNALERROR - internal driver error");
        }
        catch (OutOfVideoMemoryException)
        {
          Log.Error("D3D: D3DERR_OUTOFVIDEOMEMORY - not enough available display memory to perform the operation");
        }
        catch (OutOfMemoryException)
        {
          Log.Error("D3D: D3DERR_OUTOFMEMORY - could not allocate sufficient memory to complete the call");
        }

        // load resources
        GUIGraphicsContext.Load();
        GUITextureManager.Init();
        GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
        GUIFontManager.InitializeDeviceObjects();

        // restart window manager
        GUIWindowManager.PreInit();
        GUIWindowManager.ActivateWindow(activeWin);
        GUIWindowManager.OnDeviceRestored();

        // set new device for font manager
        GUIFontManager.SetDevice();

        // continue rendering
        AppActive = true;
      }
    }


    /// <summary>
    /// Called when nothing else needs to be done and it's time to render
    /// </summary>
    protected void FullRender()
    {
      // don't render if form is minimized and not visible to the user
      // TODO: do not render when display is turned off or we are in away mode - needs testing
      //if (!IsVislbe || IsInAwayMode || !IsDisplayTurnedOn)
      if (!IsVisible)
      {
        Thread.Sleep(100);
        return;
      }
      ResumePlayer();
      UpdateMouseCursor();

      // In minitv mode allow to loose focus
      if (ActiveForm != this && _alwaysOnTop && !_miniTvMode && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        Activate();
      }

      // sleep during playback as frames are rendered by other means to avoid hogging a CPU core
      if (GUIGraphicsContext.Vmr9Active)
      {
        Thread.Sleep(1000 / GUIGraphicsContext.MaxFPS);
        return;
      }

      // Render a frame during idle time (no messages are waiting)
      if (AppActive)
      {
        #if !DEBUG
        try
        {
        #endif
          if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST || (ActiveForm != this && _reduceFrameRate)  || GUIGraphicsContext.SaveRenderCycles || !IsVisible)
          {
            Thread.Sleep(100);
          }
          RecoverDevice();
          try
          {
            if (!GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
            {
              lock (GUIGraphicsContext.RenderLock)
              {
                Render(GUIGraphicsContext.TimePassed);
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("D3D: Exception: {0}", ex);
          }
        #if !DEBUG
        } 
	    	catch (Exception ee)
        {
          Log.Info("d3dapp: Exception {0}", ee);
          // ReSharper disable LocalizableElement
          MessageBox.Show("An exception has occurred. MediaPortal has to be closed.\r\n\r\n" + ee, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Information);
         // ReSharper restore LocalizableElement
          Close();
        }
        #endif
      }
      else
      {
        // if form isn't active then don't use all the CPU unless we are visible
        if ((ActiveForm != this && _reduceFrameRate) || GUIGraphicsContext.SaveRenderCycles || !IsVisible)
        {
          Thread.Sleep(100);
        }
      }

      if (_firstTimeActivated && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        Log.Debug("D3D FullRender: MP focus");
        Activate();
        TopMost = true; // important
        TopMost = false; // important
        Focus();
      }
    }


    /// <summary>
    /// 
    /// </summary>
    protected void RecoverDevice()
    {
      // do not try to receover device, when MP does not set a lost state
      if (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.LOST)
      {
        return;
      }

      // disable event handlers
      GUIGraphicsContext.DX9Device.DeviceLost -= OnDeviceLost;

      Log.Debug("D3D: RecoverDevice()");

      // check cooperation level for D3D device only
      if (!GUIGraphicsContext.IsDirectX9ExUsed())
      {
        Log.Debug("D3D: Testing cooperation level of device");
        try
        {
          GUIGraphicsContext.DX9Device.TestCooperativeLevel();
        }
        catch (DeviceLostException)
        {
          Log.Warn("D3D: D3DERR_DEVICELOST - device is lost but cannot be reset at this time");
          return;
        }
        catch (DeviceNotResetException)
        {
          Log.Warn("D3D: D3DERR_DEVICENOTRESET - device is lost but can be reset at this time");
        }
      }

      // lock rendering loop and recreate the backbuffer for the current D3D device
      RecreateSwapChain();

      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;

      // enable handlers
      GUIGraphicsContext.DX9Device.DeviceLost += OnDeviceLost;


      if (RefreshRateChanger.RefreshRateChangePending && RefreshRateChanger.RefreshRateChangeStrFile.Length > 0)
      {
        RefreshRateChanger.RefreshRateChangePending = false;
        if (RefreshRateChanger.RefreshRateChangeMediaType != RefreshRateChanger.MediaType.Unknown)
        {
          var t1 = (int)RefreshRateChanger.RefreshRateChangeMediaType;
          var t2 = (g_Player.MediaType)t1;
          g_Player.Play(RefreshRateChanger.RefreshRateChangeStrFile, t2);
        }
        else
        {
          g_Player.Play(RefreshRateChanger.RefreshRateChangeStrFile);
        }
        if ((g_Player.HasVideo || g_Player.HasViz) && RefreshRateChanger.RefreshRateChangeFullscreenVideo)
        {
          g_Player.ShowFullScreenWindow();
        }
      }
    }


    /// <summary>
    /// Get the  statistics 
    /// </summary>
    protected void GetStats()
    {
      FrameStatsLine1 = String.Format("last {0} fps ({1}x{2}), {3}",
                                      GUIGraphicsContext.CurrentFPS.ToString("f2"),
                                      GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth,
                                      GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight,
                                      GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferFormat
                                      );

      FrameStatsLine2 = String.Format("");

      if (GUIGraphicsContext.Vmr9Active)
      {
        FrameStatsLine2 = String.Format(GUIGraphicsContext.IsEvr ? "EVR {0} " : "VMR9 {0} ", GUIGraphicsContext.Vmr9FPS.ToString("f2"));
      }

      string quality = String.Format("avg fps:{0} sync:{1} drawn:{2} dropped:{3} jitter:{4}",
                                      VideoRendererStatistics.AverageFrameRate.ToString("f2"),
                                      VideoRendererStatistics.AverageSyncOffset,
                                      VideoRendererStatistics.FramesDrawn,
                                      VideoRendererStatistics.FramesDropped,
                                      VideoRendererStatistics.Jitter);
      FrameStatsLine2 += quality;
    }


    /// <summary>
    /// Update the various statistics the simulation keeps track of
    /// </summary>
    protected void UpdateStats()
    {
      long time      = Stopwatch.GetTimestamp();
      float diffTime = (float)(time - _lastTime) / Stopwatch.Frequency;

      // Update the scene stats once per second
      if (diffTime >= 1.0f)
      {
        GUIGraphicsContext.CurrentFPS = Frames / diffTime;
        _lastTime = time;
        Frames = 0;
      }
    }


    /// <summary>
    /// Restores MP from System Tray
    /// </summary>
    protected void RestoreFromTray()
    {
      // do nothing if we are closing
      if (_isClosing)
      {
        return;
      }

      // only restore if not visible
      if (!IsVisible)
      {
        Log.Info("D3D: Restoring from tray");
        IsVisible = true;
        AppActive = true;

        // Display taskbar icon
        Show();

        if (_notifyIcon != null)
        {
          _notifyIcon.Visible = false;
        }
        WindowState = FormWindowState.Normal;
        Activate();

        // Enable _firstTimeActivated to focus on restore
        _firstTimeActivated = true;

        // resume player and restore volume
        if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD)
        {
          // Check and Restore sound if g_Player.Volume equal 0
          if (g_Player.Volume == 0)
          {
            g_Player.Volume = Volume;
          }
          Log.Debug("D3D: Restoring volume from tray {0}", Volume);
          if (g_Player.Paused)
          {
            g_Player.Pause();
          }
        }

        if (!Windowed && AutoHideTaskbar)
        {
          HideTaskBar(true);
        }

        MouseCursor = false;
        MouseTimeOutTimer = DateTime.Now;
        UpdateMouseCursor();

        // Restore GUIGraphicsContext.State when we recover from minimize
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.SUSPENDING)
        {
          GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
        }
      }
    }


    /// <summary>
    /// Minimized MP To System Tray
    /// </summary>
    protected void MinimizeToTray()
    {
      // do nothing if we are closing
      if (_isClosing)
      {
        return;
      }

      // Init for resetting DialogMenu to avoid freeze on minimize
      var dialogMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

      // only minimize if visible and lost focus in windowed mode or if in fullscreen mode or if exiting to tray
      if (IsVisible && ((Windowed && _lostFocus) || (!Windowed && MinimizeOnFocusLoss)) || ExitToTray)
      {
        if (dialogMenu != null && (GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_DIALOG_MENU || GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_DIALOG_OK))
        {
          dialogMenu.Reset();
          dialogMenu.Dispose();
          GUIWindowManager.UnRoute(); // only unroute if we still the routed window
        }

        Log.Info("D3D: Minimizing to tray");
        IsVisible = false;
        AppActive = false;

        // Hide taskbar icon
        Hide();

        if (_notifyIcon != null)
        {
          _notifyIcon.Visible = true;
        }
        WindowState = FormWindowState.Minimized;
 
        // pause player and mute audio
        if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD || g_Player.IsDVDMenu)
        {
          // Only backup sound if g_Player.Volume is different from 0 (to avoid duplicate store and never get sound back)
          if (g_Player.Volume != 0)
          {
            Volume = g_Player.Volume;
            Log.Debug("D3D: backuping volume to tray {0}", Volume);
            g_Player.Volume = 0;
          }
          if (!g_Player.Paused)
          {
            g_Player.Pause();
          }
        }

        if (AutoHideTaskbar)
        {
          HideTaskBar(false);
        }

        // Enable _firstTimeActivated to focus on restore
        _firstTimeActivated = false;

        MouseCursor = true;
        MouseTimeOutTimer = DateTime.Now;
        UpdateMouseCursor();

        // show mouse cursor it will be hidden in the context menu
        if (AutoHideMouse)
        {
          ShowMouseCursor(true);
        }

        // Suspending when we are on minimize (otherwise MP can stay freezed if notification windows show up while minimize)
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
        {
          GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING;
        }

        ExitToTray = false;
      }
    }


    /// <summary>
    /// Message Loop - Handles ANSI and Unicode Messages and dispatch them
    /// </summary>
    protected void HandleMessage()
    {
      try
      {
        while (Win32API.PeekMessage(ref _msgApi, IntPtr.Zero, 0, 0, 0))
        {
          if (_msgApi.hwnd != IntPtr.Zero && Win32API.IsWindowUnicode(new HandleRef(null, _msgApi.hwnd)))
          {
            Win32API.GetMessageW(ref _msgApi, IntPtr.Zero, 0, 0);
            Win32API.TranslateMessage(ref _msgApi);
            Win32API.DispatchMessageW(ref _msgApi);
          }
          else
          {
            Win32API.GetMessageA(ref _msgApi, IntPtr.Zero, 0, 0);
            Win32API.TranslateMessage(ref _msgApi);
            Win32API.DispatchMessageA(ref _msgApi);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Debug("D3D: Exception: {0}", ex.ToString());
      }
    }


    /// <summary>
    /// Calculates the maximum client area if skin is larger than working area
    /// </summary>
    /// <returns></returns>
    protected Size CalcMaxClientArea()
    {
      Size clientArea;
      var border = new Size(Width - ClientSize.Width, Height - ClientSize.Height);
      if (GUIGraphicsContext.SkinSize.Width + border.Width <= GUIGraphicsContext.currentScreen.WorkingArea.Width &&
          GUIGraphicsContext.SkinSize.Height + border.Height <= GUIGraphicsContext.currentScreen.WorkingArea.Height)
      {
        clientArea = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
      }
      else
      {
        double ratio = Math.Min((double)(GUIGraphicsContext.currentScreen.WorkingArea.Width - border.Width) / GUIGraphicsContext.SkinSize.Width,
                                (double)(GUIGraphicsContext.currentScreen.WorkingArea.Height - border.Height) / GUIGraphicsContext.SkinSize.Height);
        clientArea = new Size((int)(GUIGraphicsContext.SkinSize.Width * ratio), (int)(GUIGraphicsContext.SkinSize.Height * ratio));
      }
      return clientArea;
    }

    #endregion

    #region private methods

    /// <summary>
    /// Initialize Menus and Event Handlers
    /// </summary>
    private void InitializeComponent()
    {
      SuspendLayout(); 
      var resources = new ComponentResourceManager(typeof(D3D));
      _components = new Container();
      
      _menuStripMain   = new MainMenu(_components);
      _menuItemFile    = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_File };
      _menuItemOptions = new MenuItem { Index = 1, Text = Resources.D3DApp_menuItem_Options };
      _menuItemWizards = new MenuItem { Index = 2, Text = Resources.D3DApp_menuItem_Wizards };
      _menuStripMain.MenuItems.AddRange(new[] { _menuItemFile, _menuItemOptions, _menuItemWizards });

      _menuItemExit        = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_Exit };
      _menuItemExit.Click += Exit;
      _menuItemFile.MenuItems.AddRange(new[] { _menuItemExit });
     
      _menuItemFullscreen    = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_Fullscreen };
      _menuItemMiniTv        = new MenuItem { Index = 1, Text = Resources.D3DApp_menuItem_MiniTv, Checked = _miniTvMode};
      _menuItemConfiguration = new MenuItem { Index = 2, Text = Resources.D3DApp_menuItem_Configuration, Shortcut = Shortcut.F2, };
      _menuItemFullscreen.Click    += MenuItemFullscreen;
      _menuItemMiniTv.Click        += MenuItemMiniTV;
      _menuItemConfiguration.Click += MenuItemConfiguration;
      _menuItemOptions.MenuItems.AddRange(new[] { _menuItemFullscreen, _menuItemMiniTv, _menuItemConfiguration });
      
      _menuItemDVD      = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_DVD };
      _menuItemMovies   = new MenuItem { Index = 1, Text = Resources.D3DApp_menuItem_Movies };
      _menuItemMusic    = new MenuItem { Index = 2, Text = Resources.D3DApp_menuItem_Music };
      _menuItemPictures = new MenuItem { Index = 3, Text = Resources.D3DApp_menuItem_Pictures };
      _menuItemTV       = new MenuItem { Index = 4, Text = Resources.D3DApp_menuItem_Television };
      _menuItemDVD.Click      += MenuItemDVD;
      _menuItemMovies.Click   += MenuItemMovies;
      _menuItemMusic.Click    += MenuItemMusic;
      _menuItemPictures.Click += MenuItemPictures;
      _menuItemTV.Click       += MenuItemTV;
      _menuItemWizards.MenuItems.AddRange(new[] { _menuItemDVD, _menuItemMovies, _menuItemMusic, _menuItemPictures, _menuItemTV });

      _menuItemContext = new MenuItem { Index = 0, Text = "" };
      _menuItemEmpty   = new MenuItem { Index = 0, Text = "" };
      _menuItemContext.MenuItems.AddRange(new[] { _menuItemEmpty });

      _contextMenu = new ContextMenu();
      _contextMenu.MenuItems.Clear();
      _contextMenu.MenuItems.Add(Resources.D3DApp_NotifyIcon_Restore, NotifyIconRestore);
      _contextMenu.MenuItems.Add(Resources.D3DApp_NotifyIcon_Exit, NotifyIconExit);

      _notifyIcon = new NotifyIcon(_components)
                      {
                        Text = Resources.D3DApp_NotifyIcon_MediaPortal,
                        Icon = ((Icon) (resources.GetObject("_notifyIcon.TrayIcon"))),
                        ContextMenu = _contextMenu
                      };
      _notifyIcon.DoubleClick += NotifyIconRestore;
      
      AutoScaleDimensions = new SizeF(6F, 13F);
      AutoScaleMode = AutoScaleMode.Dpi;
      KeyPreview = true;
      Name = "D3D";
      Load             += OnLoad;
      MouseMove        += OnMouseMove;
      MouseDown        += OnClick;
      MouseDoubleClick += OnMouseDoubleClick;
      KeyPress         += OnKeyPress;
      KeyDown          += OnKeyDown;

      ResumeLayout(false);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="caps"></param>
    /// <param name="vertexProcessingType"></param>
    /// <param name="adapterFormat"></param>
    /// <param name="backBufferFormat"></param>
    /// <returns></returns>
    private static bool ConfirmDevice(Caps caps, VertexProcessingType vertexProcessingType, Format adapterFormat, Format backBufferFormat)
    {
      return true;
    }


    /// <summary>
    /// Finds the adapter that has the specified screen on its primary monitor
    /// </summary>
    /// <returns>The adapter that has the specified screen on its primary monitor</returns>
    private GraphicsAdapterInfo FindAdapterForScreen(Screen screen)
    {
      foreach (GraphicsAdapterInfo adapterInfo in _enumerationSettings.AdapterInfoList)
      {
        var hMon = Manager.GetAdapterMonitor(adapterInfo.AdapterOrdinal);

        var info = new MonitorInformation();
        info.Size = (uint)Marshal.SizeOf(info);
        GetMonitorInfo(hMon, ref info);
        var rect = Screen.FromRectangle(info.MonitorRectangle).Bounds;
        if (rect.Equals(screen.Bounds))
        {
          Log.Debug("D3D: FindAdapterForScreen Adapter #{0}: {1} - Driver: {2} ({3}) - DeviceName: {4}",
          adapterInfo.AdapterOrdinal, adapterInfo.AdapterDetails.Description, adapterInfo.AdapterDetails.DriverName, adapterInfo.AdapterDetails.DriverVersion, adapterInfo.AdapterDetails.DeviceName);
          GUIGraphicsContext.currentStartScreen = GUIGraphicsContext.currentScreen;
          return adapterInfo;
        }
      }
      return null;
    }

    
    /// <summary>
    /// Build D3D presentation parameters
    /// </summary>
    /// <param name="windowed">true for window, false for fullscreen</param>
    protected void BuildPresentParams(bool windowed)
    {
      Log.Debug("D3D: BuildPresentParams()");
      Size size = CalcMaxClientArea();
      _presentParams.BackBufferWidth  = windowed ? size.Width  : GUIGraphicsContext.currentScreen.Bounds.Width;
      _presentParams.BackBufferHeight = windowed ? size.Height : GUIGraphicsContext.currentScreen.Bounds.Height;
      _presentParams.BackBufferFormat = Format.Unknown;
 
      if (OSInfo.OSInfo.Win7OrLater())
      {
        if (!_doNotWaitForVSync)
        {
          // FLIPEX will benefit from more back buffers
          _presentParams.BackBufferCount = 4;
        }
        else
        {
          // Allow performance measurements when DWM is not blocking Present()
          _presentParams.BackBufferCount = 20;
          int hr = DXNative.FontEngineSetMaximumFrameLatency(20);
          if (hr != 0)
          {
            Log.Info("D3D: BuildPresentParams() failed to set maximum frame latency - error: {0}", hr);
          }
        }
      }
      else
      {
        _presentParams.BackBufferCount = 2;
      }

      _presentParams.MultiSample               = MultiSampleType.None;
      _presentParams.MultiSampleQuality        = 0;
      _presentParams.SwapEffect                = OSInfo.OSInfo.Win7OrLater() ? (SwapEffect)D3DSWAPEFFECT_FLIPEX : SwapEffect.Discard;
      _presentParams.DeviceWindow              = _renderTarget;
      _presentParams.Windowed                  = true;
      _presentParams.EnableAutoDepthStencil    = false;
      _presentParams.AutoDepthStencilFormat    = DepthFormat.Unknown;
      _presentParams.PresentFlag               = PresentFlag.Video;
      _presentParams.FullScreenRefreshRateInHz = 0;
      _presentParams.PresentationInterval      = _doNotWaitForVSync ? PresentInterval.Immediate : PresentInterval.One;
      _presentParams.ForceNoMultiThreadedFlag  = false;

      GUIGraphicsContext.DirectXPresentParameters = _presentParams;
      Log.Info("D3D: Back Buffer Size set to: {0}x{1}", _presentParams.BackBufferWidth, _presentParams.BackBufferHeight);
      Windowed = windowed;
    }

    /// <summary>
    /// Initialization of the D3D Device
    /// </summary>
    protected void InitializeDevice()
    {
      Log.Debug("D3D: InitializeDevice()");
      
      // get capabilities of hardware device for current adapter
      Caps capabilities = GetCapabilities();

      if (!successfulInit)
      {
        if (AdapterInfo != null)
        {
          Log.Info("D3D: GPU '{0}' is using driver version '{1}'", AdapterInfo.AdapterDetails.Description.Trim(), AdapterInfo.AdapterDetails.DriverVersion);
        }
        Log.Info("D3D: Vertex shader version: {0}", capabilities.VertexShaderVersion);
        Log.Info("D3D: Pixel shader version: {0}", capabilities.PixelShaderVersion);

        // default to reference rasterizer and software vertex processing for initialization purposes
        _deviceType = DeviceType.Reference;
        _createFlags = CreateFlags.SoftwareVertexProcessing;

        // check if GPU supports rasterization in hardware
        if (capabilities.DeviceCaps.SupportsHardwareRasterization)
        {
          Log.Info("D3D: GPU supports rasterization in hardware");
          _deviceType = DeviceType.Hardware;
        }

        //  check if GPU supports shader model 2.0
        if (capabilities.VertexShaderVersion >= new Version(2, 0) && capabilities.PixelShaderVersion >= new Version(2, 0))
        {
          // check if GPU supports rasterization, transformation, lighting in hardware
          if (capabilities.DeviceCaps.SupportsHardwareTransformAndLight)
          {
            Log.Info("D3D: GPU supports rasterization, transformation, lighting in hardware");
            _createFlags = CreateFlags.HardwareVertexProcessing;
          }
          // check if GPU supports rasterization, transformations, lighting, and shading in hardware
          if (capabilities.DeviceCaps.SupportsPureDevice)
          {
            Log.Info("D3D: GPU supports rasterization, transformations, lighting, and shading in hardware");
            if (OSInfo.OSInfo.VistaOrLater())
            {
              _createFlags |= CreateFlags.PureDevice;
            }
          }
        }

        // Log some interesting capabilities
        if (!capabilities.TextureCaps.SupportsPower2 && !capabilities.TextureCaps.SupportsNonPower2Conditional)
        {
          Log.Info("D3D: GPU unconditionally supports textures with dimensions that are not powers of two");
        }
        else if (capabilities.TextureCaps.SupportsPower2 && capabilities.TextureCaps.SupportsNonPower2Conditional)
        {
          Log.Info("D3D: GPU conditionally supports textures with dimensions that are not powers of two");
        }
        else if (capabilities.TextureCaps.SupportsPower2 && !capabilities.TextureCaps.SupportsNonPower2Conditional)
        {
          Log.Info("D3D: GPU does not support textures with dimensions that are not powers of two");
        }

        // TODO: check for any capabilities that would not allow MP to run

        // read skin information
        GUIControlFactory.LoadReferences(GUIGraphicsContext.GetThemedSkinFile(@"\references.xml"));

        // Set up the presentation parameters
        BuildPresentParams(Windowed);

        // Create the device
        if (GUIGraphicsContext.IsDirectX9ExUsed())
        {
          // Vista or later, use DirectX9Ex device
          Log.Info("D3D: Creating DirectX9Ex device");
          CreateDirectX9ExDevice();
        }
        else
        {
          Log.Info("D3D: Creating DirectX9 device");
          GUIGraphicsContext.DX9Device = new Device(AdapterInfo.AdapterOrdinal,
                                                    _deviceType,
                                                    _renderTarget,
                                                    _createFlags | CreateFlags.MultiThreaded | CreateFlags.FpuPreserve,
                                                    _presentParams);
        }

        // update some magic number use for animations
        GUIGraphicsContext.MaxFPS = Manager.Adapters[AdapterInfo.AdapterOrdinal].CurrentDisplayMode.RefreshRate;

        // set always on top parameter
        TopMost = _alwaysOnTop;

        // Set up the fullscreen cursor
        if (_showCursorWhenFullscreen && !Windowed)
        {
          var ourCursor = Cursor;
          GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
          GUIGraphicsContext.DX9Device.ShowCursor(true);
        }

        // Setup the event handlers for our device
        GUIGraphicsContext.DX9Device.DeviceLost += OnDeviceLost;

        // Initialize the app's device-dependent objects
        try
        {
          InitializeDeviceObjects();
          AppActive = true;
          successfulInit = true;
        }
        catch (Exception ex)
        {
          Log.Error("D3D: InitializeDeviceObjects - Exception: {0}", ex.ToString());
          GUIGraphicsContext.DX9Device.Dispose();
          GUIGraphicsContext.DX9Device = null;
        }
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Caps GetCapabilities()
    {
      Log.Debug("D3D: GetCapabilities()");


      Caps capabilities;
  
      // retry ever 100 ms to get the capabilities for the selected adapter for a maximum time of 100 tries
      if (!successful)
      {
        do
        {
          try
          {
            capabilities = Manager.GetDeviceCaps(AdapterInfo.AdapterOrdinal, DeviceType.Hardware);
            successful = true;
          }
          catch (Exception)
          {
            retries++;
            if (AdapterInfo != null)
            {
              Log.Warn("Main: Failed to get capabilities for adapter #{0}: {1} (retry in {2}ms) try reinit #{3}", AdapterInfo.AdapterOrdinal, AdapterInfo.ToString(), delayBetweenTries, retries);
            }
            else
            {
              Log.Warn("Main: Failed to get capabilities for adapter (retry in {0}ms) try reinit #{1}", delayBetweenTries, retries);
            }
            Thread.Sleep(delayBetweenTries);

            // Restart Init sequence
            if (retries < numberOfRetries)
            {
              Init();
            }
          }
        } while (!successful && retries < numberOfRetries);
      }

      // MP needs a hardware device, or it will be unusable
      if (!successful)
      {
        Log.Error("D3D: Could not create device");
        // ReSharper disable LocalizableElement
        MessageBox.Show("Direct3D device could not be created.", "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        // ReSharper restore LocalizableElement
        try
        {
          Close();
        }
          // ReSharper disable EmptyGeneralCatchClause
        catch {}
        // ReSharper restore EmptyGeneralCatchClause
      }
 
      return capabilities;
    }


    /// <summary>
    /// Creates a DirectX9Ex Device
    /// </summary>
    private void CreateDirectX9ExDevice()
    {
      var param = new D3DPRESENT_PARAMETERS
                    {
                      BackBufferWidth            = (uint)_presentParams.BackBufferWidth,
                      BackBufferHeight           = (uint)_presentParams.BackBufferHeight,
                      BackBufferFormat           = _presentParams.BackBufferFormat,
                      BackBufferCount            = (uint)_presentParams.BackBufferCount,
                      MultiSampleType            = _presentParams.MultiSample,
                      MultiSampleQuality         = _presentParams.MultiSampleQuality,
                      SwapEffect                 = _presentParams.SwapEffect,
                      hDeviceWindow              = _presentParams.DeviceWindow.Handle,
                      Windowed                   = _presentParams.Windowed ? 1 : 0,
                      EnableAutoDepthStencil     = _presentParams.EnableAutoDepthStencil ? 1 : 0,
                      AutoDepthStencilFormat     = _presentParams.AutoDepthStencilFormat,
                      Flags                      = (int)_presentParams.PresentFlag,
                      FullScreen_RefreshRateInHz = (uint)_presentParams.FullScreenRefreshRateInHz,
                      PresentationInterval       = (uint)_presentParams.PresentationInterval,
                    };


      IDirect3D9Ex direct3D9Ex;
      Direct3D.Direct3DCreate9Ex(32, out direct3D9Ex);

      IntPtr dev;
      var hr = direct3D9Ex.CreateDeviceEx(AdapterInfo.AdapterOrdinal,
                                          _deviceType, 
                                          _renderTarget.Handle,
                                          _createFlags | CreateFlags.MultiThreaded | CreateFlags.FpuPreserve,
                                          ref param,
                                          IntPtr.Zero,
                                          out dev);

      if (hr == 0)
      {
        GUIGraphicsContext.DX9Device = new Device(dev);
        GUIGraphicsContext.DX9Device.Reset(_presentParams);
      }
      else
      {
        Log.Error("D3D: Could not create device");
        // ReSharper disable LocalizableElement
        MessageBox.Show("Direct3D device could not be created.", "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        // ReSharper restore LocalizableElement
        try
        {
          Close();
        }
        // ReSharper disable EmptyGeneralCatchClause
        catch {}
        // ReSharper restore EmptyGeneralCatchClause
      }
    }


    /// <summary>
    /// Save player state (when form was resized)
    /// </summary>
    protected void SavePlayerState()
    {
      if (!_wasPlayingVideo && (g_Player.Playing && (g_Player.IsTV || g_Player.IsVideo || g_Player.IsDVD)))
      {
        _wasPlayingVideo = true;

        // Some Audio/video is playing
        _currentPlayerPos = g_Player.CurrentPosition;
        _currentPlayListType = PlaylistPlayer.CurrentPlaylistType;
        _currentPlayList = new PlayList();

        Log.Info("D3D: Saving fullscreen state for resume: {0}", Menu == null);
        var tempList = PlaylistPlayer.GetPlaylist(_currentPlayListType);
        if (tempList.Count == 0 && g_Player.IsDVD)
        {
          // DVD is playing
          var itemDVD = new PlayListItem {FileName = g_Player.CurrentFile, Played = true, Type = PlayListItem.PlayListItemType.DVD};
          tempList.Add(itemDVD);
        }

        foreach (var itemNew in tempList)
        {
          _currentPlayList.Add(itemNew);
        }

        _currentFile = PlaylistPlayer.Get(PlaylistPlayer.CurrentSong);
        if (_currentFile.Equals(string.Empty) && g_Player.IsDVD)
        {
          _currentFile = g_Player.CurrentFile;
        }

        Log.Info("D3D: Stopping media - Current playlist: Type: {0} / Size: {1} / Current item: {2} / Filename: {3} / Position: {4}",
                 _currentPlayListType, _currentPlayList.Count, PlaylistPlayer.CurrentSong, _currentFile, _currentPlayerPos);
        
        g_Player.Stop();

        _lastActiveWindow = GUIWindowManager.ActiveWindow;
      }
    }


    /// <summary>
    /// Restore player from saved state
    /// </summary>
    protected void ResumePlayer()
    {
      if (_wasPlayingVideo) // was any player active at all?
      {
        _wasPlayingVideo = false;

        // we were watching some audio/video
        Log.Info("D3D: RestorePlayers - Resuming: {0}", _currentFile);
        PlaylistPlayer.Init();
        PlaylistPlayer.Reset();
        PlaylistPlayer.CurrentPlaylistType = _currentPlayListType;
        var playlist = PlaylistPlayer.GetPlaylist(_currentPlayListType);
        playlist.Clear();
        if (_currentPlayList != null)
        {
          foreach (var itemNew in _currentPlayList)
          {
            playlist.Add(itemNew);
          }
        }

        if (playlist.Count > 0 && playlist[0].Type.Equals(PlayListItem.PlayListItemType.DVD))
        {
          // we were watching DVD
          var movieDetails = new IMDBMovie();
          var fileName = playlist[0].FileName;
          VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
          var idFile = VideoDatabase.GetFileId(fileName);
          var idMovie = VideoDatabase.GetMovieId(fileName);
          if (idMovie >= 0 && idFile >= 0)
          {
            byte[] resumeData;
            #pragma warning disable 168
            int timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData, g_Player.SetResumeBDTitleState);
            #pragma warning restore 168
            g_Player.PlayDVD(fileName);
            if (g_Player.Playing)
            {
              g_Player.Player.SetResumeState(null);
            }
          }
        }
        else
        {
          PlaylistPlayer.Play(_currentFile); // some standard audio/video
        }

        if (g_Player.Playing)
        {
          g_Player.SeekAbsolute(_currentPlayerPos);
        }

        GUIGraphicsContext.IsFullScreenVideo = Menu == null;
        GUIWindowManager.ReplaceWindow(_lastActiveWindow);
      }
    }

    /// <summary>
    /// Wrapper for shown/hiding cursor
    /// </summary>
    /// <param name="visible"></param>
    public void ShowMouseCursor(bool visible)
    {
      int state = ShowCursor(true);
      Cursor current = Cursor.Current;

      switch (visible)
      {
        case true:
          if (current != null)
          {
            while (state > 1)
            {
              state = ShowCursor(false);
            }
            while (state < 1)
            {
              state = ShowCursor(true);
            }
            Cursor.Show();
          }
          break;
        case false:
          if (current != null)
          {
            while (state < -1)
            {
              state = ShowCursor(true);
            }
            while (state > -1)
            {
              state = ShowCursor(false);
            }
            Cursor.Hide();
          }
          break;
      }
      Log.Debug("D3D: Cursor ShowMouseCursor state {0}", state);
    }

    /// <summary>
    /// Automatically hides or show mouse cursor when over form
    /// </summary>
    private void UpdateMouseCursor()
    {
      if (!AutoHideMouse)
      {
        return;
      }

      // check if we are over the client area of the form
      bool isOverForm;
      try
      {
        isOverForm = ClientRectangle.Contains(PointToClient(MousePosition));
      }
      catch (Exception)
      {
        isOverForm = false;
      }

      // check if we are focused
      bool focused;
      try
      { 
        focused = GetForegroundWindow() == Handle; 
      }
      catch (Exception)
      {
        focused = false;
      }

      // don't update cursor status when not over client area or not focused
      if (!isOverForm && !focused)
      {
        MouseTimeOutTimer = DateTime.Now;
        return;
      }

      // Hide mouse cursor after three seconds of inactivity
      // Set timeout Value
      MouseTimeOutFullscreen = 3;
      MouseTimeOutMP = 6;
      var timeSpam = DateTime.Now - MouseTimeOutTimer;
      int timeSpamValue = GUIGraphicsContext.IsFullScreenVideo ? MouseTimeOutFullscreen : MouseTimeOutMP;
      if (timeSpam.TotalSeconds >= timeSpamValue && MouseCursor)
      {
        MouseCursor = false;
      }

      // update mouse cursor state if necessary
      if (MouseCursor != _lastMouseCursor)
      {
        switch (MouseCursor)
        {
          case true:
            Log.Debug("D3D: Showing mouse cursor");
            ShowMouseCursor(true);
            break;
          case false:
            Log.Debug("D3D: Hiding mouse cursor");
            ShowMouseCursor(false);
            break;
        }
        _lastMouseCursor = MouseCursor;
        Invalidate();
      }
    }


    /// <summary>
    /// Set our variables to not active and not ready
    /// </summary>
    private void CleanupEnvironment()
    {
      Log.Debug("D3D CleanupEnvironment()");
      AppActive = false;
      if (GUIGraphicsContext.DX9Device != null)
      {
        // indicate we are shutting down
        App.IsShuttingDown = true;

        // remove the device lost and reset handlers as application is already closing down
        GUIGraphicsContext.DX9Device.DeviceLost  -= OnDeviceLost;
        GUIGraphicsContext.DX9Device.Dispose();
      }
    }


    /// <summary>
    /// Start Configuration.exe
    /// </summary>
    private void StartConfiguration()
    {
      const string processName = "Configuration.exe";

      if (Process.GetProcesses().Any(process => process.ProcessName.Equals(processName)))
      {
        return;
      }

      Log.Info("D3D: OnSetup - Stopping media");
      g_Player.Stop();

      MinimizeToTray();

      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.Clear();
      }

      Util.Utils.StartProcess(Config.GetFile(Config.Dir.Base, "Configuration.exe"), "", false, false);
    }


    /// <summary>
    /// Exit Form
    /// </summary>
    private void Exit(object sender, EventArgs e)
    {
      Log.Debug("D3D: Exit()");
      if (!MinimizeOnGuiExit && !IsVisible)
      {
        ShuttingDown = true;
        Close();
      }
      else
      {
        Log.Info("D3D: Minimizing to tray on GUI exit");
        ExitToTray = true;
        MinimizeToTray();
      }
    }


    /// <summary>
    /// 
    /// </summary>
    private void ToggleMiniTV()
    {
      if (Windowed)
      {
        _miniTvMode             = !_miniTvMode;
        _menuItemMiniTv.Checked = _miniTvMode;

        Size size;
        if (_miniTvMode)
        {
          FormBorderStyle = FormBorderStyle.SizableToolWindow;
          Menu            = null;
          _alwaysOnTop    = true;
          size = CalcMaxClientArea();
          size.Width  /= 3;
          size.Height /= 3;
        }
        else
        {
          FormBorderStyle = FormBorderStyle.Sizable;
          Menu            = _menuStripMain;
          using (Settings xmlreader = new MPSettings())
          {
            _alwaysOnTop  = xmlreader.GetValueAsBool("general", "alwaysontop", false);
          }
          size  = CalcMaxClientArea();
        }
        ClientSize = size;
        TopMost = _alwaysOnTop;
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected bool ShowLastActiveModule()
    {
      bool showLastActiveModule;
      int lastActiveModule;
      bool lastActiveModuleFullscreen;
      string psClientNextWakeUp;

      using (Settings xmlreader = new MPSettings())
      {
        showLastActiveModule = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
        lastActiveModule = xmlreader.GetValueAsInt("general", "lastactivemodule", -1);
        lastActiveModuleFullscreen = xmlreader.GetValueAsBool("general", "lastactivemodulefullscreen", false);
        psClientNextWakeUp = xmlreader.GetValueAsString("psclientplugin", "nextwakeup", DateTime.MaxValue.ToString(Thread.CurrentThread.CurrentCulture));
      }

      if (!Util.Utils.IsGUISettingsWindow(lastActiveModule) && showLastActiveModule)
      {
        var psClientNextwakeupDate = Convert.ToDateTime(psClientNextWakeUp);
        var now = DateTime.Now;
        var ts = psClientNextwakeupDate - now;

        Log.Debug("ShowLastActiveModule() - psclientplugin nextwakeup {0}", psClientNextWakeUp);
        Log.Debug("ShowLastActiveModule() - timediff in minutes {0}", ts.TotalMinutes);

        if (ts.TotalMinutes < 2 && ts.TotalMinutes > -2)
        {
          Log.Debug("ShowLastActiveModule() - system probably awoken by PSclient, ignoring ShowLastActiveModule");
          showLastActiveModule = false;
        }
        else
        {
          Log.Debug("ShowLastActiveModule() - system probably awoken by user, continuing with ShowLastActiveModule");
        }
      }
      else
      {
        showLastActiveModule = false;
      }
      Log.Debug("D3D: ShowLastActiveModule active : {0}", showLastActiveModule);

      if (showLastActiveModule)
      {
        Log.Debug("D3D: ShowLastActiveModule module : {0}", lastActiveModule);
        Log.Debug("D3D: ShowLastActiveModule fullscreen : {0}", lastActiveModuleFullscreen);
        if (lastActiveModule < 0)
        {
          Log.Error("Error recalling last active module - invalid module name '{0}'", lastActiveModule);
        }
        else
        {
          try
          {
            GUIWindowManager.ActivateWindow(lastActiveModule);
            if (lastActiveModule == (int) GUIWindow.Window.WINDOW_TV && lastActiveModuleFullscreen)
            {
              var tvDelayThread = new Thread(TVDelayThread) {IsBackground = true, Name = "TVDelayThread"};
              tvDelayThread.Start();
            }
          }
          catch (Exception e)
          {
            Log.Error("Error recalling last active module '{0}' - {1}", lastActiveModule, e.Message);
            showLastActiveModule = false;
          }
        }
      }
      return showLastActiveModule;
    }


    /// <summary>
    /// 
    /// </summary>
    private static void TVDelayThread()
    {
      // we have to use a small delay before calling tvfullscreen.                              
      Thread.Sleep(200);
      g_Player.ShowFullScreenWindow();
    }

    #endregion

    #region menu items

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemConfiguration(object sender, EventArgs e)
    {
      StartConfiguration();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemTV(object sender, EventArgs e)
    {
      g_Player.Stop();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.Clear();
      }
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\television.xml");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemPictures(object sender, EventArgs e)
    {
      g_Player.Stop();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.Clear();
      }
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\pictures.xml");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemMusic(object sender, EventArgs e)
    {
      g_Player.Stop();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.Clear();
      }
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\music.xml");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemMovies(object sender, EventArgs e)
    {
      g_Player.Stop();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.Clear();
      }
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\movies.xml");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemDVD(object sender, EventArgs e)
    {
      g_Player.Stop();
      using (Settings xmlreader = new MPSettings())
      {
        xmlreader.Clear();
      }
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\dvd.xml");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemFullscreen(object sender, EventArgs e)
    {
      ToggleFullscreen();

      var dialogNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (dialogNotify != null)
      {
        dialogNotify.SetHeading(1020);
        dialogNotify.SetText(String.Format("{0}\n{1}", GUILocalizeStrings.Get(1021), GUILocalizeStrings.Get(1022)));
        dialogNotify.TimeOut = 6;
        dialogNotify.SetImage(String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin, "dialog_information.png"));
        dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
      }
    }


    /// <summary>
    /// toggles default and minitv mode
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItemMiniTV(object sender, EventArgs e)
    {
      ToggleMiniTV();
    }

    #endregion

    #region notify icon items

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void NotifyIconRestore(Object sender, EventArgs e)
    {
      RestoreFromTray();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NotifyIconExit(Object sender, EventArgs e)
    {
      ShuttingDown = true;
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
    }

    #endregion

    #region event handlers

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLoad(object sender, EventArgs e)
    {
      Log.Debug("D3D: OnLoad()");
      Application.Idle += OnIdle;

      OnStartup();

      try
      {
        // give an external app a change to be notified when the application has reached the final stage of startup

        var handle = EventWaitHandle.OpenExisting("MediaPortalHandleCreated");

        if (handle.SafeWaitHandle.IsInvalid)
        {
          return;
        }

        handle.Set();
        handle.Close();
      }
      // ReSharper disable EmptyGeneralCatchClause
      catch { }
      // ReSharper restore EmptyGeneralCatchClause

      _lastCursorPosition = Cursor.Position;
      ShowLastActiveModule();

      Log.Debug("D3D: Activating main form");
      FrameMove();
      FullRender();
      Activate();

      // Start Minimize and restore to force MP focus
      if (!MinimizeOnStartup)
      {
        // needed to focus on first run.
        // First save current MP screen
        _firstLoadedScreen = true;
        Screen screenfocus = Screen.FromControl(this);
        this.WindowState = FormWindowState.Minimized;
        this.Show();
        this.WindowState = FormWindowState.Normal;
        _firstLoadedScreen = false;
        // Restore previous saved screen
        GUIGraphicsContext.currentScreen = screenfocus;
      }

      _isLoaded = true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
      Log.Debug("D3D: OnPaint()");
      base.OnPaint(e);

      if (_isLoaded)
      {
        // stop the splash screen thread it it is still running
        if (SplashScreen != null)
        {
          Log.Info("D3D: Stopping splash screen thread");
          SplashScreen.Stop();
          do
          {
            Thread.Sleep(20);
          } while (!SplashScreen.IsStopped());
          SplashScreen = null;
          
        }

        if (MinimizeOnStartup && _firstTimeWindowDisplayed)
        {
          Log.Info("D3D: Minimizing to tray on startup");
          ExitToTray = true;
          MinimizeToTray();
          _firstTimeWindowDisplayed = false;
        }
        // Set Cursor.Position to avoid mouse cursor show up itself (for ex on video)
        _lastCursorPosition = Cursor.Position;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnIdle(object sender, EventArgs e)
    {
      do
      {
        OnProcess();
        FrameMove();
        FullRender();
        if (_firstTimeActivated && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
        {
          Log.Debug("D3D OnIdle: MP focus done");
          Activate();
          TopMost = true; // important
          TopMost = false; // important
          Focus();
          BringToFront();
          _firstTimeActivated = false;
        }
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          break;
        }
      } while (!Win32API.PeekMessage(ref _msgApi, IntPtr.Zero, 0, 0, 0));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Control == false && e.Alt && (e.KeyCode == Keys.Return))
      {
        ToggleFullscreen();
        e.Handled = true;
        return;
      }

      if (e.Control && e.Alt && e.KeyCode == Keys.Return)
      {
        ToggleMiniTV();
        e.Handled = true;
        return;
      }

      if (e.KeyCode == Keys.F2)
      {
        StartConfiguration();
      }

      if (e.Handled == false)
      {
        KeyDownEvent(e);
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
      KeyPressEvent(e);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      MouseMoveEvent(e);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClick(object sender, MouseEventArgs e)
    {
      if (ActiveForm != this)
      {
        return;
      }
      MouseClickEvent(e);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (ActiveForm != this)
      {
        return;
      }
      MouseDoubleClickEvent(e);
    }

    #endregion

    #region event handler helpers

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected virtual void KeyDownEvent(KeyEventArgs e)
    {
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected virtual void KeyPressEvent(KeyPressEventArgs e)
    {
    }
    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected virtual void MouseMoveEvent(MouseEventArgs e)
    {
      //Log.Debug("D3D: MouseMoveEvent()");
      // only re-activate mouse cursor when the position really changed between move events
      if (Cursor.Position != _lastCursorPosition || (e.Delta > 0 || e.Delta < 0))
      {
        if ((!_disableMouseEvents && (_lastCursorPosition != _moveMouseCursorPosition) || (e.Delta > 0 || e.Delta < 0)) && (_lastCursorPosition != _moveMouseCursorPositionRefresh))
        {
          MouseCursor = true;
        }
        MouseTimeOutTimer = DateTime.Now;
        UpdateMouseCursor();
        _lastCursorPosition = Cursor.Position;
        _moveMouseCursorPosition = _lastCursorPosition;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected virtual void MouseClickEvent(MouseEventArgs e)
    {
      if (!_disableMouseEvents)
      {
        MouseCursor = true;
      }
      MouseTimeOutTimer = DateTime.Now;
      UpdateMouseCursor();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected virtual void MouseDoubleClickEvent(MouseEventArgs e)
    {
      if (!_disableMouseEvents)
      {
        MouseCursor = true;
      }
      MouseTimeOutTimer = DateTime.Now;
      UpdateMouseCursor();
    }

    #endregion

    #region forms controls

    /// <summary>
    /// Raises the KeyPress event.
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.control.onkeypress.aspx
    /// </summary>
    /// <param name="e">A KeyPressEventArgs that contains the event data.</param>
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      // Allow the control to handle the keystroke now
      if (!e.Handled)
      {
        base.OnKeyPress(e);
      }
    }


    /// <summary>
    /// Raises the MouseMove event.
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.control.onmousemove.aspx
    /// </summary>
    /// <param name="e">A MouseEventArgs that contains the event data.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      if ((GUIGraphicsContext.DX9Device != null) && (!GUIGraphicsContext.DX9Device.Disposed))
      {
        // Move the D3D cursor
        GUIGraphicsContext.DX9Device.SetCursorPosition(e.X, e.Y, false);
      }
      // Let the control handle the mouse now
      base.OnMouseMove(e);
    }


    /// <summary>
    /// Raises the GotFocus event.
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.control.ongotfocus.aspx
    /// </summary>
    /// <param name="e">An EventArgs that contains the event data.</param>
    protected override void OnGotFocus(EventArgs e)
    {
      Log.Debug("D3D: OnGotFocus()");
      if (AutoHideMouse && !Windowed)
      {
        // Set Cursor.Position to avoid mouse cursor show up itself (for ex on video)
        _lastCursorPosition = Cursor.Position;
      }
      _lostFocus = false;
      if (WindowState == FormWindowState.Minimized)
      {
        _firstTimeActivated = true;
      }
      base.OnGotFocus(e);
    }


    /// <summary>
    /// Raises the Lost Focus event.
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.control.onlostfocus.aspx
    /// </summary>
    /// <param name="e">An EventArgs that contains the event data.</param>
    protected override void OnLostFocus(EventArgs e)
    {
      Log.Debug("D3D: OnLostFocus()");
      if (AutoHideMouse && !Windowed)
      {
        // Set Cursor.Position to avoid mouse cursor show up itself (for ex on video)
        _lastCursorPosition = Cursor.Position;
      }
      _lostFocus = true;
      base.OnLostFocus(e);
    }


    /// <summary>
    /// Repaints the current window independent of WM_PAINT messages
    /// </summary>
    protected void OnPaintEvent()
    {
      try
      {
        if (!GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
        {
          lock (GUIGraphicsContext.RenderLock)
          {
            Render(GUIGraphicsContext.TimePassed);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("D3D: Exception: {0}", ex);
      }
    }


    /// <summary>
    ///  http://msdn.microsoft.com/en-us/library/system.windows.forms.form.onformclosing.aspx
    /// </summary>
    /// <param name="formClosingEventArgs"></param>
    protected override void OnFormClosing(FormClosingEventArgs formClosingEventArgs)
    {
      Log.Debug("D3D: OnFormClosing()");
      if (MinimizeOnGuiExit && !ShuttingDown && IsVisible)
      {
        Log.Info("D3D: Minimizing to tray on GUI exit");
        _isClosing = false;
        formClosingEventArgs.Cancel = true;
        ExitToTray = true;
        MinimizeToTray();
      }
      else
      {
        _isClosing = true;
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
        g_Player.Stop();
      }
      base.OnFormClosing(formClosingEventArgs);
    }

    /// <summary>
    /// Clean up any resources
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      CleanupEnvironment();

      if (_notifyIcon != null)
      {
        _notifyIcon.Dispose();
      }

      base.Dispose(disposing);

      if (AutoHideTaskbar)
      {
        HideTaskBar(false);
      }
    }
 
    #endregion
  }
}