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
using System.Collections.Generic;
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
using SharpDX;
using SharpDX.Direct3D9;
//using WPFMediaKit.DirectX;

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
      public System.Drawing.Rectangle MonitorRectangle;
      public System.Drawing.Rectangle WorkRectangle;
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

    protected static readonly Size WINDOWS_NATIVE_RESOLUTION = new Size(1024, 768);
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

    protected internal static int ScreenNumberOverride;     // 0 or higher means it is set
    protected internal static bool FullscreenOverride;       // full screen mode overridden by command line argument?
    protected internal static bool WindowedOverride;         // window mode overridden by command line argument?
    protected static string SkinOverride;             // skin overridden by command line argument
    protected string FrameStatsLine1;          // 1st string to hold frame stats
    protected string FrameStatsLine2;          // 2nd string to hold frame stats
    protected bool MinimizeOnStartup;        // minimize to tray on startup?
    protected bool MinimizeOnGuiExit;        // minimize to tray on GUI exit?
    protected bool MinimizeOnFocusLoss;      // minimize to tray when focus in full screen mode is lost?
    protected bool ShuttingDown;             // set to true if MP is shutting down
    protected bool AutoHideMouse;            // Should the mouse cursor be hidden automatically?
    protected bool AppActive;                // set to true while MP is active
    //protected bool                 NeedRecreateSwapChain;    // set to true if recreate swap chain is needed
    protected bool MouseCursor;              // holds the current mouse cursor state
    protected bool Windowed;                 // are we in windowed mode?
    protected bool AutoHideTaskbar;          // Should the Task Bar be hidden?
    protected bool IsVisible;                // set to true if form is not minimized to tray
    protected bool IsInAwayMode;             // indicates if away mode is active, assume no on application launch
    protected bool IsDisplayTurnedOn;        // indicates if the display is turned on, assume yes on application launch
    protected bool IsUserPresent;            // indicates if a user is present, assume yes on application launch
    protected bool UseEnhancedVideoRenderer; // should EVR be used?
    protected bool UseMadVideoRenderer;      // is madVR used?
    protected bool ExitToTray;               //
    protected int Frames;                   // number of frames since our last update
    protected static int Volume;                   // used to save old volume level in case we mute audio
    protected PlayListPlayer PlaylistPlayer;           // 
    protected DateTime MouseTimeOutTimer;        // tracks the time of the last mouse activity
    protected DateTime KeyEventTimer;            // tracks the time of the last key event activity
    protected DateTime ScreenSaverEventTimer;    // tracks the time of the last key event activity
    protected RECT LastRect;                 // tracks last rectangle size for window resizing
    protected System.Drawing.Point LastCursorPosition;       // tracks last cursor position during window moving
    protected static SplashScreen SplashScreen;             // splash screen object
    protected GraphicsAdapterInfo AdapterInfo;              // hold adapter info for the selected display on startup of MP
    protected int MouseTimeOutMP;           // Mouse activity timeout while in MP in seconds
    protected int MouseTimeOutFullscreen;   // Mouse activity timeout while in Fullscreen in seconds
    protected KeyPressEventArgs PreviousKeyEvent;
    protected bool IsToggleMiniTV;           // madVR check to know if we need to do a resize when Toggle
    protected bool _forceMpAlive;            // workaround to force form to refresh

    #endregion

    #region private attributes

    private readonly Control _renderTarget;             // render target object
    protected D3DConfiguration _currentGraphicsConfiguration = null;
    protected DisplayMode _desktopDisplayMode;
    protected PresentParameters _presentParams;          // D3D presentation parameters
    protected PresentParameters _presentParamsBackup;      // D3D presentation parameters Backup
    internal D3DEnumeration _enumerationSettings;      //
    private readonly bool _useExclusiveDirectXMode;  // 
    private readonly bool _disableMouseEvents;       //
    private readonly bool _doNotWaitForVSync;        // debug setting
    private readonly bool _showCursorWhenFullscreen; // should the mouse cursor be shown in full screen?
    private readonly bool _reduceFrameRate;          // reduce frame rate when not in focus?
    protected readonly bool _useFcuBlackScreenFix;     // workaround for FCU edition to fix blackscreen on resolution change
    private bool _miniTvMode;               // 
    private bool _isClosing;                //
    private bool _isLoaded;                 //
    private bool _lastMouseCursor;          // holds the last mouse cursor state to keep state balance
    private bool _lostFocus;                // set to true if form lost focus
    private bool _wasPlayingVideo;          //
    private bool _alwaysOnTop;              // tracks the always on top state
    private bool _firstTimeWindowDisplayed; // set to true when MP becomes Active the 1st time
    private bool _firstTimeLoadingParams;   // set to true when MP becomes Active the 1st time
    private bool _firstTimeActivated;       // needed to focus on first start
    private int _lastActiveWindow;         //
    private long _lastTime;                 //
    private double _currentPlayerPos;         //
    private string _currentFile;              //
    private System.Drawing.Rectangle _oldClientRectangle;       // last size of the MP window
    private IContainer _components;               //
    private MainMenu _menuStripMain;            // menu
    private MenuItem _menuItemFile;             // sub menu
    private MenuItem _menuItemOptions;          // sub menu
    private MenuItem _menuItemWizards;          // sub menu
    private MenuItem _menuItemExit;             // menu item
    private MenuItem _menuItemConfiguration;    // menu item
    private MenuItem _menuItemDVD;              // menu item
    private MenuItem _menuItemMovies;           // menu item
    private MenuItem _menuItemMusic;            // menu item
    private MenuItem _menuItemPictures;         // menu item
    private MenuItem _menuItemTV;               // menu item
    private MenuItem _menuItemContext;          // menu item
    private MenuItem _menuItemEmpty;            // menu item
    private MenuItem _menuItemFullscreen;       // menu item
    private MenuItem _menuItemMiniTv;           // menu item
    private NotifyIcon _notifyIcon;               // tray icon object
    private ContextMenu _contextMenu;              //
    private PlayListType _currentPlayListType;      //
    private PlayList _currentPlayList;          //
    private Win32API.MSG _msgApi;                   //
    internal static System.Drawing.Point _lastCursorPosition;       // track cursor position of last move move event
    private DeviceType _deviceType;               //
    private CreateFlags _createFlags;              //
    internal static System.Drawing.Point _moveMouseCursorPosition;
    internal static System.Drawing.Point _moveMouseCursorPositionRefresh;
    protected static bool _firstLoadedScreen;        //
    protected static bool _restoreLoadedScreen;      // Restoring correct screen when multi screen in use
    protected static Screen _screenFocus;              // Screen Focus when minimize / restore to systray
    protected static Screen _backupscreen;             // Screen Focus when minimize / restore to systray
    protected static System.Drawing.Rectangle _backupBounds;             // Bounds backup
    private System.Windows.Forms.Timer _DeviceStatusWatchdog;

    #endregion

    #region constructor

    /// <summary>
    /// Constructor
    /// </summary>
    protected D3D()
    {
      _firstTimeLoadingParams = true;
      _firstTimeWindowDisplayed = true;
      _firstTimeActivated = true;
      MinimizeOnStartup = false;
      MinimizeOnGuiExit = false;
      MinimizeOnFocusLoss = false;
      ShuttingDown = false;
      AutoHideMouse = true;
      MouseCursor = true;
      Windowed = true;
      Volume = -1;
      AppActive = false;
      KeyPreview = true;
      Frames = 0;
      FrameStatsLine1 = null;
      FrameStatsLine2 = null;
      Text = Resources.D3DApp_NotifyIcon_MediaPortal;
      PlaylistPlayer = PlayListPlayer.SingletonPlayer;
      MouseTimeOutTimer = DateTime.Now;
      _lastActiveWindow = -1;
      IsVisible = true;
      IsDisplayTurnedOn = true;
      IsInAwayMode = false;
      IsUserPresent = true;
      _lastMouseCursor = !MouseCursor;
      _showCursorWhenFullscreen = false;
      _currentPlayListType = PlayListType.PLAYLIST_NONE;
      _enumerationSettings = new D3DEnumeration();
      _presentParams = new PresentParameters();
      _renderTarget = this;

      using (Settings xmlreader = new MPSettings())
      {
        _useExclusiveDirectXMode = xmlreader.GetValueAsBool("general", "exclusivemode", true);
        UseEnhancedVideoRenderer = xmlreader.GetValueAsBool("general", "useEVRenderer", false);
        UseMadVideoRenderer = xmlreader.GetValueAsBool("general", "useMadVideoRenderer", false);
        _disableMouseEvents = xmlreader.GetValueAsBool("remote", "CentareaJoystickMap", false);
        AutoHideTaskbar = xmlreader.GetValueAsBool("general", "hidetaskbar", true);
        _alwaysOnTop = xmlreader.GetValueAsBool("general", "alwaysontop", false);
        _reduceFrameRate = xmlreader.GetValueAsBool("gui", "reduceframerate", false);
        _doNotWaitForVSync = xmlreader.GetValueAsBool("debug", "donotwaitforvsync", false);
        _useFcuBlackScreenFix = xmlreader.GetValueAsBool("general", "usefcublackscreenfix", false);
      }

      _useExclusiveDirectXMode = !UseEnhancedVideoRenderer && _useExclusiveDirectXMode;
      GUIGraphicsContext.IsVMR9Exclusive = _useExclusiveDirectXMode;

      if (UseEnhancedVideoRenderer)
      {
        GUIGraphicsContext.VideoRenderer = GUIGraphicsContext.VideoRendererType.EVR;
      }
      else if (UseMadVideoRenderer)
      {
        GUIGraphicsContext.VideoRenderer = GUIGraphicsContext.VideoRendererType.madVR;
      }
      else
      {
        GUIGraphicsContext.VideoRenderer = GUIGraphicsContext.VideoRendererType.VMR9;
      }

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

      GUIGraphicsContext.Direct3DLoad();

      // Reset Adapter
      AdapterInfo = null;

      // log information about available adapters
      var enumeration = new D3DEnumeration();
      enumeration.Enumerate();
      foreach (GraphicsAdapterInfo ai in enumeration.AdapterInfoList)
      {
        Log.Debug("D3D: Init Adapter #{0}: {1} - Driver: {2} ({3}) - DeviceName: {4}",
          ai.AdapterOrdinal, ai.AdapterDetails.Description, ai.AdapterDetails.Driver, ai.AdapterDetails.DriverVersion, ai.AdapterDetails.DeviceName);
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
        MaximizeBox = false;
        MinimizeBox = false;
        Menu = null;

        var newBounds = GUIGraphicsContext.currentScreen.Bounds;
        Bounds = newBounds;
        ClientSize = newBounds.Size;
        Log.Info("D3D: Client size: {0}x{1} - Screen: {2}x{3}",
                 ClientSize.Width, ClientSize.Height,
                 GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
      }

      _backupBounds = GUIGraphicsContext.currentScreen.Bounds;
      _backupscreen = GUIGraphicsContext.currentScreen;

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
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DeviceLost -= OnDeviceLost;
      }

      // Suspending GUIGraphicsContext.State
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING;
        Log.Debug("D3D: ToggleFullscreen() - set GUIGraphicsContext.State.SUSPENDING");
      }

      // Reset DialogMenu to avoid freeze when going to fullscreen/windowed
      var dialogMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dialogMenu != null &&
          (GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_DIALOG_MENU ||
           GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_DIALOG_OK))
      {
        dialogMenu.Dispose();
        GUIWindowManager.UnRoute(); // only unroute if we still the routed window
      }

      // reset device if necessary
      Windowed = !Windowed;
      RecreateSwapChain(false);
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
          IsToggleMiniTV = true;
          ToggleMiniTV();
        }

        _oldClientRectangle.Location = Location;
        _oldClientRectangle.Size = ClientSize;

        WindowState = FormWindowState.Normal;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Menu = null;
        Windowed = false;
        Location = new System.Drawing.Point(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y);
        ClientSize = GUIGraphicsContext.currentScreen.Bounds.Size;
      }
      else
      {
        Log.Info("D3D: Switching from full screen to windowed mode");

        if (AutoHideTaskbar)
        {
          HideTaskBar(false);
        }

        WindowState = FormWindowState.Normal;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        Menu = _menuStripMain;
        Windowed = true;

        if (_oldClientRectangle.IsEmpty)
        {
          Location = new System.Drawing.Point(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y);
          ClientSize = CalcMaxClientArea();
        }
        else
        {
          Location = _oldClientRectangle.Location;
          ClientSize = _oldClientRectangle.Size;
        }

        LastRect.top = Location.Y;
        LastRect.left = Location.X;
        LastRect.bottom = Size.Height;
        LastRect.right = Size.Width;
      }

      Update();
      Log.Info("D3D: Client Size: {0}x{1}", ClientSize.Width, ClientSize.Height);
      Log.Info("D3D: Screen size: {0}x{1}", GUIGraphicsContext.currentScreen.Bounds.Width,
        GUIGraphicsContext.currentScreen.Bounds.Height);

      // Needed this double check on first start
      if (GUIGraphicsContext.DX9Device != null)
      {
        // Get Size
        Size client = GUIGraphicsContext.form.ClientSize;
        if ((_presentParams.BackBufferWidth != client.Width ||
            _presentParams.BackBufferHeight != client.Height) && _firstTimeLoadingParams)
        {
          // reset device if necessary
          RecreateSwapChain(false);
          _firstTimeLoadingParams = false;
        }
      }

      // if we do ToggleFullscreen when using madVR (needed to resize OSD)
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        GUIGraphicsContext.ForceMadVRRefresh3D = true;
        GUIGraphicsContext.ForceMadVRRefresh = true;
      }
      // Force OSD resize when no video has started to display OSD GUI correctly
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
               !GUIGraphicsContext.InVmr9Render)
      {
        GUIGraphicsContext.ForceMadVRRefresh = true;
      }

      //// Force a madVR refresh to resize MP window
      //// TODO how to handle it better
      //g_Player.RefreshMadVrVideo();

      // madVR resize OSD/Video window
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
          GUIGraphicsContext.InVmr9Render)
      {
        GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ONDISPLAYMADVRCHANGED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(message);
        GUIGraphicsContext.NeedRecreateSwapChain = true;
      }

      // Restore GUIGraphicsContext.State
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.SUSPENDING)
      {
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
        Log.Debug("D3D: ToggleFullscreen() - set GUIGraphicsContext.State.RUNNING");
      }

      // enable event handlers
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DeviceLost += OnDeviceLost;
        if (this._DeviceStatusWatchdog != null)
          this._DeviceStatusWatchdog.Enabled = true;
      }
    }


    /// <summary>
    /// reset device if back buffer does not match skin dimensions (e.g. 16:10 display with 16:9 skin, 720p skin on 1080p display etc.)
    /// </summary>
    internal void RecreateSwapChain(bool useBackup)
    {
      //lock (GUIFontManager.Renderlock) // Disable for now seems to deadlock
      {
        // Don't need to resize when using madVR
        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
            GUIGraphicsContext.Vmr9Active)
        {
          GUIGraphicsContext.ForceMadVRRefresh3D = true;
          return;
        }

        if (AppActive || GUIGraphicsContext.NeedRecreateSwapChain)
        {
          Log.Debug("D3D: RecreateSwapChain()");

          // Suspending GUIGraphicsContext.State
          if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
          {
            GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING;
            Log.Debug("D3D: RecreateSwapChain() - set GUIGraphicsContext.State.SUSPENDING");
          }

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
          // Don't need to resize when using madVR
          if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR ||
              !GUIGraphicsContext.Vmr9Active)
          {
            if (GUIGraphicsContext.DX9Device != null)
            {
              lock (GUIGraphicsContext.RenderLock)
              {
                GUIGraphicsContext.DX9Device.EvictManagedResources();
              }

              if (useBackup)
              {
                try
                {
                  Log.Debug("D3D: RecreateSwapChain() by restoring startup DirectX values");
                  GUIGraphicsContext.DirectXPresentParameters = _presentParamsBackup;
                  lock (GUIGraphicsContext.RenderLock)
                  {
                    GUIGraphicsContext.DX9Device.Reset(_presentParamsBackup);
                  }
                }
                catch (SharpDXException ex)
                {
                  if (ex.ResultCode == 0x88760868) //D3DERR_DEVICELOST: 0x88760868
                    Log.Error("Main: D3DERR_DEVICELOST - device is lost but cannot be reset at this time {0}", ex.Message);
                  else
                    Log.Error("Main: SharpDXException: {0}", ex.Message);
                }
                //catch (InvalidCallException ex)
                //{
                //  Log.Error("D3D: D3DERR_INVALIDCALL - presentation parameters might contain an invalid value {0}", ex.Message);
                //  Util.Utils.RestartMePo();
                //}
                //catch (DeviceLostException ex)
                //{
                //  Log.Error("D3D: D3DERR_DEVICELOST - device is lost but cannot be reset at this time {0}", ex.Message);
                //}
                //catch (DriverInternalErrorException ex)
                //{
                //  Log.Error("D3D: D3DERR_DRIVERINTERNALERROR - internal driver error {0}", ex.Message);
                //}
                //catch (OutOfVideoMemoryException ex)
                //{
                //  Log.Error(
                //    "D3D: D3DERR_OUTOFVIDEOMEMORY - not enough available display memory to perform the operation {0}", ex.Message);
                //}
                catch (OutOfMemoryException ex)
                {
                  Log.Error("D3D: D3DERR_OUTOFMEMORY - could not allocate sufficient memory to complete the call {0}", ex.Message);
                }
                catch (Exception ex)
                {
                  Log.Error("D3D: RecreateSwapChain exception (useBackup) : {0}", ex);
                }
              }
              else
              {
                // build new D3D presentation parameters and reset device
                Log.Debug("D3D: RecreateSwapChain() by rebuild PresentParams");
                //BuildPresentParams(Windowed);
                BuildPresentParamsFromSettings();
                GUIGraphicsContext.PresentationParameters = _presentParams;
                try
                {
                  lock (GUIGraphicsContext.RenderLock)
                  {
                    GUIGraphicsContext.DX9Device.Reset(_presentParams);
                  }
                }
                catch (SharpDXException ex)
                {
                  if (ex.ResultCode == 0x88760868) //D3DERR_DEVICELOST: 0x88760868
                    Log.Debug("Main: D3DERR_DEVICELOST - device is lost but cannot be reset at this time {0}", ex.Message);
                  else
                    Log.Debug("Main: SharpDXException: {0}", ex.Message);
                }
                //catch (InvalidCallException ex)
                //{
                //  Log.Error("D3D: D3DERR_INVALIDCALL - presentation parametters might contain an invalid value {0}", ex.Message);
                //}
                //catch (DeviceLostException ex)
                //{
                //  // Indicate that the device has been lost
                //  Log.Error("D3D: D3DERR_DEVICELOST - device is lost but cannot be reset at this time {0}", ex.Message);
                //}
                //catch (DriverInternalErrorException ex)
                //{
                //  Log.Error("D3D: D3DERR_DRIVERINTERNALERROR - internal driver error {0}", ex.Message);
                //}
                //catch (OutOfVideoMemoryException ex)
                //{
                //  Log.Error(
                //    "D3D: D3DERR_OUTOFVIDEOMEMORY - not enough available display memory to perform the operation {0}", ex.Message);
                //}
                catch (OutOfMemoryException ex)
                {
                  Log.Error("D3D: D3DERR_OUTOFMEMORY - could not allocate sufficient memory to complete the call {0}", ex.Message);
                }
                catch (Exception ex)
                {
                  Log.Error("D3D: RecreateSwapChain exception : {0}", ex);
                }
              }

              //SharpDX doesn't dispose the previous surface(Microsoft does)
              //We need to change the target now otherwise PlaneScene will keep rendering to old surface
              Surface srf = GUIGraphicsContext.RenderTarget;
              if (srf != null)
                srf.Dispose();

              //Set the new Surface
              GUIGraphicsContext.RenderTarget = GUIGraphicsContext.DX9Device.GetRenderTarget(0);
            }
          }

          // load resources
          GUIGraphicsContext.Load();
          GUITextureManager.Init();
          GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
          GUIFontManager.InitializeDeviceObjects();

          // restart window manager
          GUIWindowManager.PreInit();
          GUIWindowManager.OnResize();
          GUIWindowManager.ActivateWindow(activeWin);
          GUIWindowManager.OnDeviceRestored();

          // set new device for font manager
          GUIFontManager.SetDevice();

          // continue rendering
          AppActive = true;
          GUIGraphicsContext.NeedRecreateSwapChain = false;

          if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
          {
            Log.Debug("D3D: MadVrScreenResize madVR");
            VMR9Util.g_vmr9?.MadVrScreenResize(GUIGraphicsContext.form.Location.X, GUIGraphicsContext.form.Location.Y,
              _presentParams.BackBufferWidth, _presentParams.BackBufferHeight, true);
          }

          if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
          {
            // Reset 3D
            GUIGraphicsContext.NoneDone = false;
            GUIGraphicsContext.TopAndBottomDone = false;
            GUIGraphicsContext.SideBySideDone = false;
            GUIGraphicsContext.SBSLeftDone = false;
            GUIGraphicsContext.SBSRightDone = false;
            GUIGraphicsContext.TABTopDone = false;
            GUIGraphicsContext.TABBottomDone = false;
          }

          // Restore GUIGraphicsContext.State
          if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.SUSPENDING)
          {
            GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
            Log.Debug("D3D: RecreateSwapChain() - set GUIGraphicsContext.State.RUNNING");
          }
        }
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
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST || (ActiveForm != this && _reduceFrameRate) || GUIGraphicsContext.SaveRenderCycles || !IsVisible)
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
        _firstTimeActivated = false;
      }
    }


    /// <summary>
    /// 
    /// </summary>
    protected void RecoverDevice()
    {
      // do not try to recover device, when MP does not set a lost state
      if (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.LOST)
      {
        return;
      }

      // disable event handlers
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DeviceLost -= OnDeviceLost;
      }

      Log.Debug("D3D: RecoverDevice()");

      // check cooperation level for D3D device only
      if (!GUIGraphicsContext.IsDirectX9ExUsed())
      {
        Log.Debug("D3D: Testing cooperation level of device");
        try
        {
          if (GUIGraphicsContext.DX9Device != null)
            GUIGraphicsContext.DX9Device.TestCooperativeLevel();
        }
        //catch (DeviceLostException ex)
        catch (SharpDXException ex)
        {
          if (ex.ResultCode == 0x88760868) //D3DERR_DEVICELOST: 0x88760868
          {
            Log.Warn("D3D: D3DERR_DEVICELOST - device is lost but cannot be reset at this time {0}", ex.Message);
            return;
          }
          else
            Log.Warn("D3D: D3DERR_DEVICENOTRESET - {0}", ex.Message);
        }
        //catch (DeviceNotResetException ex)
        //{
        //  Log.Warn("D3D: D3DERR_DEVICENOTRESET - device is lost but can be reset at this time {0}", ex.Message);
        //}
        catch (Exception ex)
        {
          Log.Warn("D3D: D3DERR_DEVICENOTRESET - {0}", ex.Message);
        }
      }

      while (true)
      {
        try
        {
          if (GUIGraphicsContext.DX9Device != null && !GUIGraphicsContext.DX9Device.IsDisposed)
          {
            // Check device
            GUIGraphicsContext.DX9Device.Present();
            break;
          }
        }
        //catch (DirectXException dex)
        //{
        //  switch (dex.ErrorCode)
        //  {
        //    default:
        //      Log.Error(dex);
        //      Util.Utils.RestartMePo();
        //      break;
        //  }
        //}
        catch (Exception ex)
        {
          Log.Error(ex);
          Util.Utils.RestartMePo();
          break;
        }
      }

      // lock rendering loop and recreate the backbuffer for the current D3D device
      RecreateSwapChain(true);

      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;

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

      // enable handlers
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DeviceLost += OnDeviceLost;
        if (this._DeviceStatusWatchdog != null)
          this._DeviceStatusWatchdog.Enabled = true;
      }
    }

    protected long _lasttime = 0;

    /// <summary>
    /// Get the  statistics 
    /// </summary>
    protected void GetStats()
    {
      if (GUIGraphicsContext.DX9Device != null)
      {
        long time = Stopwatch.GetTimestamp();
        float difftime = (float)(time - _lasttime) / Stopwatch.Frequency * 1000;
        _lasttime = time;

        FrameStatsLine1 = String.Format("last {0} fps ({1}x{2}), {3}, {4:0} ms",
          GUIGraphicsContext.CurrentFPS.ToString("f2"),
          _presentParams.BackBufferWidth,
          _presentParams.BackBufferHeight,
          _presentParams.BackBufferFormat,
          difftime
          );
      }

      FrameStatsLine2 = String.Format("");

      if (GUIGraphicsContext.Vmr9Active)
      {
        string renderer = "VMR9 {0} ";
        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
          renderer = "EVR {0} ";
        else if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
          renderer = "madVR {0} ";

        FrameStatsLine2 = String.Format(renderer, GUIGraphicsContext.Vmr9FPS.ToString("f2"));
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
      long time = Stopwatch.GetTimestamp();
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

        // Restore previous saved screen
        GUIGraphicsContext.currentScreen = _screenFocus;
        _restoreLoadedScreen = true;

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
          Log.Debug("D3D: RestoreFromTray() - set GUIGraphicsContext.State.RUNNING");
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
      if (IsVisible && ((Windowed && (_lostFocus && MinimizeOnFocusLoss) || (!Windowed && MinimizeOnFocusLoss)) || ExitToTray))
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

        _screenFocus = Screen.FromControl(this);
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
          Log.Debug("D3D: MinimizeToTray() - set GUIGraphicsContext.State.SUSPENDING");
        }

        ExitToTray = false;
      }
    }

    /// <summary>
    /// Focus Mediaportal is visible.
    /// </summary>
    protected void ForceMpAlive()
    {
      if (!_forceMpAlive || Windowed)
      {
        return;
      }
      if (_useFcuBlackScreenFix)
      {
        Log.Debug("D3D: ForceMPAlive start.");
        if (GUIGraphicsContext.form != null && GUIGraphicsContext.ActiveForm != IntPtr.Zero)
        {
          try
          {
            // FCU suicide form blackscreen fix
            _forceMpAlive = false;

            // Don't use FixFCU because it didn't always works for all users
            //FixFCU();

            // Instead use this code, it will make a little window appear on top of MP
            using (Form form = new Form())
            {
              form.Text = "FCU Workaround";
              form.Opacity = 5;
              form.Size = new Size(10, 10);
              form.FormBorderStyle = FormBorderStyle.FixedSingle;
              form.Show();
              form.Location = new System.Drawing.Point(GUIGraphicsContext.form.Location.X,
                GUIGraphicsContext.form.Location.Y)
              {
                X = GUIGraphicsContext.form.Location.X,
                Y = GUIGraphicsContext.form.Location.Y
              };
              form.Show();
              form.Close();
            }
            // Make Mediaportal window focused
            if (Win32API.SetForegroundWindow(GUIGraphicsContext.ActiveForm, true))
            {
              Log.Debug("D3D: ForceMpAlive MP Successfully switched focus.");
            }
          }
          catch (Exception ex)
          {
            Log.Debug("D3D: ForceMpAlive {0}", ex.Message);
            // Make MediaPortal window normal ( if minimized )
            if (GUIGraphicsContext.form.WindowState == FormWindowState.Minimized)
            {
              Win32API.ShowWindow(GUIGraphicsContext.ActiveForm, Win32API.ShowWindowFlags.ShowNormal);
              Win32API.ShowWindow(GUIGraphicsContext.ActiveForm, Win32API.ShowWindowFlags.Minimize);
              this.WindowState = FormWindowState.Normal;
              this.WindowState = FormWindowState.Minimized;
              Log.Debug("D3D: ForceMPAlive Minimize.");
            }
            else
            {
              Win32API.ShowWindow(GUIGraphicsContext.ActiveForm, Win32API.ShowWindowFlags.Minimize);
              Win32API.ShowWindow(GUIGraphicsContext.ActiveForm, Win32API.ShowWindowFlags.ShowNormal);
              this.WindowState = FormWindowState.Minimized;
              this.WindowState = FormWindowState.Normal;
              Log.Debug("D3D: ForceMPAlive ShowNormal.");
            }
          }
        }
      }
    }

    // Fix for FCU blackscreen
    protected class SuicideForm : Form
    {
      protected internal SuicideForm()
      {
        Thread.Sleep(500);
        Activated += SuicideFormActivated;
        Opacity = 0;
      }

      protected override void Dispose(bool disposing)
      {
        Activated -= SuicideFormActivated;
        base.Dispose(disposing);
      }

      private void SuicideFormActivated(Object sender, EventArgs e)
      {
        Thread.Sleep(1000);
        Close();
      }
    }

    protected static void KillFormThread()
    {
      try
      {
        var suicideForm = new SuicideForm
        {
          Opacity = 5,
          Size = new Size(10, 10),
          FormBorderStyle = FormBorderStyle.None,
          WindowState = FormWindowState.Normal,
          ShowInTaskbar = false
        };
        suicideForm.Location = new System.Drawing.Point(GUIGraphicsContext.form.Location.X,
          GUIGraphicsContext.form.Location.Y)
        {
          X = GUIGraphicsContext.form.Location.X,
          Y = GUIGraphicsContext.form.Location.Y
        };
        suicideForm.Show();
        suicideForm.Focus();
        //// Make Mediaportal window focused
        //if (Win32API.SetForegroundWindow(GUIGraphicsContext.ActiveForm, true))
        //{
        //  Log.Debug("D3D: KillFormThread MP Successfully switched focus.");
        //}

        //// Bring MP to front
        //GUIGraphicsContext.form.BringToFront();
        Log.Debug("D3D: KillFormThread done.");
      }
      catch (Exception ex)
      {
        Log.Error("D3D: KillFormThread exception {0}", ex);
      }
    }

    protected static void FixFCU()
    {
      try
      {
        Log.Debug("D3D: FixFCU");
        ThreadStart starter = KillFormThread;
        var killFormThread = new Thread(starter) { IsBackground = true };
        killFormThread.Start();
      }
      catch (Exception ex)
      {
        Log.Error("D3D: FixFCU exception {0}", ex);
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
        clientArea = new Size(GUIGraphicsContext.SkinSize.Width + border.Width, GUIGraphicsContext.SkinSize.Height + border.Height);
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

      _menuStripMain = new MainMenu(_components);
      _menuItemFile = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_File };
      _menuItemOptions = new MenuItem { Index = 1, Text = Resources.D3DApp_menuItem_Options };
      _menuItemWizards = new MenuItem { Index = 2, Text = Resources.D3DApp_menuItem_Wizards };
      _menuStripMain.MenuItems.AddRange(new[] { _menuItemFile, _menuItemOptions, _menuItemWizards });

      _menuItemExit = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_Exit };
      _menuItemExit.Click += Exit;
      _menuItemFile.MenuItems.AddRange(new[] { _menuItemExit });

      _menuItemFullscreen = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_Fullscreen };
      _menuItemMiniTv = new MenuItem { Index = 1, Text = Resources.D3DApp_menuItem_MiniTv, Checked = _miniTvMode };
      _menuItemConfiguration = new MenuItem { Index = 2, Text = Resources.D3DApp_menuItem_Configuration, Shortcut = Shortcut.F2, };
      _menuItemFullscreen.Click += MenuItemFullscreen;
      _menuItemMiniTv.Click += MenuItemMiniTV;
      _menuItemConfiguration.Click += MenuItemConfiguration;
      _menuItemOptions.MenuItems.AddRange(new[] { _menuItemFullscreen, _menuItemMiniTv, _menuItemConfiguration });

      _menuItemDVD = new MenuItem { Index = 0, Text = Resources.D3DApp_menuItem_DVD };
      _menuItemMovies = new MenuItem { Index = 1, Text = Resources.D3DApp_menuItem_Movies };
      _menuItemMusic = new MenuItem { Index = 2, Text = Resources.D3DApp_menuItem_Music };
      _menuItemPictures = new MenuItem { Index = 3, Text = Resources.D3DApp_menuItem_Pictures };
      _menuItemTV = new MenuItem { Index = 4, Text = Resources.D3DApp_menuItem_Television };
      _menuItemDVD.Click += MenuItemDVD;
      _menuItemMovies.Click += MenuItemMovies;
      _menuItemMusic.Click += MenuItemMusic;
      _menuItemPictures.Click += MenuItemPictures;
      _menuItemTV.Click += MenuItemTV;
      _menuItemWizards.MenuItems.AddRange(new[] { _menuItemDVD, _menuItemMovies, _menuItemMusic, _menuItemPictures, _menuItemTV });

      _menuItemContext = new MenuItem { Index = 0, Text = "" };
      _menuItemEmpty = new MenuItem { Index = 0, Text = "" };
      _menuItemContext.MenuItems.AddRange(new[] { _menuItemEmpty });

      _contextMenu = new ContextMenu();
      _contextMenu.MenuItems.Clear();
      _contextMenu.MenuItems.Add(Resources.D3DApp_NotifyIcon_Restore, NotifyIconRestore);
      _contextMenu.MenuItems.Add(Resources.D3DApp_NotifyIcon_Exit, NotifyIconExit);

      _notifyIcon = new NotifyIcon(_components)
      {
        Text = Resources.D3DApp_NotifyIcon_MediaPortal,
        Icon = ((Icon)(resources.GetObject("_notifyIcon.TrayIcon"))),
        ContextMenu = _contextMenu
      };
      _notifyIcon.DoubleClick += NotifyIconRestore;

      AutoScaleDimensions = new SizeF(6F, 13F);
      AutoScaleMode = AutoScaleMode.Dpi;
      KeyPreview = true;
      Name = "D3D";
      Load += OnLoad;
      MouseMove += OnMouseMove;
      MouseDown += OnClick;
      MouseDoubleClick += OnMouseDoubleClick;
      KeyPress += OnKeyPress;
      KeyDown += OnKeyDown;

      //this._DeviceStatusWatchdog = new System.Windows.Forms.Timer(_components);
      //this._DeviceStatusWatchdog.Enabled = false;
      //this._DeviceStatusWatchdog.Tick += new System.EventHandler(this.cbDeviceStatusWatchdog);

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
    private static bool ConfirmDevice(Capabilities caps, VertexProcessingType vertexProcessingType, Format adapterFormat, Format backBufferFormat)
    {
      return true;
    }

    /// <summary>
    /// Finds the adapter that has the specified screen on its primary monitor
    /// </summary>
    /// <returns>The adapter that has the specified screen on its primary monitor</returns>
    public GraphicsAdapterInfo FindAdapterForScreen(Screen screen)
    {
      // Get display mode of primary adapter (which is assumed to be where the window will appear)
      DisplayMode primaryDesktopDisplayMode = GUIGraphicsContext.Direct3D.Adapters[0].CurrentDisplayMode;
      GraphicsAdapterInfo bestAdapterInfo = null;
      GraphicsDeviceInfo bestDeviceInfo = null;
      DeviceCombo bestDeviceCombo = null;

      Log.Debug("[FindAdapterForScreen] PrimaryDesktopDisplayMode: Format:{0}", primaryDesktopDisplayMode.Format);

      foreach (GraphicsAdapterInfo adapterInfo in _enumerationSettings.AdapterInfoList)
      {
        Log.Debug("[FindAdapterForScreen] AdapterInfo: {0} - {1}", adapterInfo.AdapterDetails.Description, adapterInfo.AdapterDetails.DeviceName);

        foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfos)
        {
          Log.Debug("[FindAdapterForScreen] DeviceInfo: {0}", deviceInfo.DevType);

          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceCombos)
          {
            Log.Debug("[FindAdapterForScreen] DeviceCombo: BackBufferFormat:{0} AdapterFormat:{1} Windowed:{2}",
              deviceCombo.BackBufferFormat, deviceCombo.AdapterFormat, deviceCombo.IsWindowed);

            if (!deviceCombo.IsWindowed)
              continue;

            if (deviceCombo.AdapterFormat != primaryDesktopDisplayMode.Format)
              continue;

            bool adapterMatchesBackBuffer = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);

            // If we haven't found a compatible DeviceCombo yet, or if this set
            // is better (because it's a HAL, and/or because formats match better),
            // save it
            if (bestDeviceCombo == null ||
                bestDeviceCombo.DevType != DeviceType.Hardware && deviceInfo.DevType == DeviceType.Hardware ||
                deviceCombo.DevType == DeviceType.Hardware && adapterMatchesBackBuffer)
            {
              bestAdapterInfo = adapterInfo;
              bestDeviceInfo = deviceInfo;
              bestDeviceCombo = deviceCombo;
              if (deviceInfo.DevType == DeviceType.Hardware && adapterMatchesBackBuffer)
              {
                Log.Debug("[FindAdapterForScreen] DeviceCombo: Selected");
                // This windowed device combo looks great -- take it
                goto EndWindowedDeviceComboSearch;
              }
              // Otherwise keep looking for a better windowed device combo
              Log.Debug("[FindAdapterForScreen] DeviceCombo: Accepted");
            }
          }
        }
      }

    EndWindowedDeviceComboSearch:
      if (bestDeviceCombo == null)
      {
        Log.Debug("[FindAdapterForScreen] NotFound");
        return null;
      }

      return bestAdapterInfo;
    }

    /// <summary>
    /// Initialization of the D3D Device
    /// </summary>
    protected Device InitializeDevice()
    {
      Log.Debug("D3D: InitializeDevice()");

      // get capabilities of hardware device for current adapter
      Capabilities capabilities = GetCapabilities();

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
        if ((capabilities.DeviceCaps & DeviceCaps.HWRasterization) == DeviceCaps.HWRasterization)
        {
          Log.Info("D3D: GPU supports rasterization in hardware");
          _deviceType = DeviceType.Hardware;
        }

        //  check if GPU supports shader model 2.0
        if (capabilities.VertexShaderVersion >= new Version(2, 0) && capabilities.PixelShaderVersion >= new Version(2, 0))
        {
          // check if GPU supports rasterization, transformation, lighting in hardware
          if ((capabilities.DeviceCaps & DeviceCaps.HWTransformAndLight) == DeviceCaps.HWTransformAndLight)
          {
            Log.Info("D3D: GPU supports rasterization, transformation, lighting in hardware");
            _createFlags = CreateFlags.HardwareVertexProcessing;
          }
          // check if GPU supports rasterization, transformations, lighting, and shading in hardware
          if ((capabilities.DeviceCaps & DeviceCaps.PureDevice) == DeviceCaps.PureDevice)
          {
            Log.Info("D3D: GPU supports rasterization, transformations, lighting, and shading in hardware");
            if (OSInfo.OSInfo.VistaOrLater())
            {
              _createFlags |= CreateFlags.PureDevice;
            }
          }
        }

        // Log some interesting capabilities
        if ((capabilities.TextureCaps & (TextureCaps.Pow2 | TextureCaps.NonPow2Conditional)) == 0)
        {
          Log.Info("D3D: GPU unconditionally supports textures with dimensions that are not powers of two");
        }
        else if ((capabilities.TextureCaps & (TextureCaps.Pow2 | TextureCaps.NonPow2Conditional)) == (TextureCaps.Pow2 | TextureCaps.NonPow2Conditional))
        {
          Log.Info("D3D: GPU conditionally supports textures with dimensions that are not powers of two");
        }
        else if ((capabilities.TextureCaps & (TextureCaps.Pow2 | TextureCaps.NonPow2Conditional)) == TextureCaps.Pow2)
        {
          Log.Info("D3D: GPU does not support textures with dimensions that are not powers of two");
        }

        // TODO: check for any capabilities that would not allow MP to run

        // read skin information
        GUIControlFactory.LoadReferences(GUIGraphicsContext.GetThemedSkinFile(@"\references.xml"));


        try
        {
          //D3DConfiguration configuration = Windowed ? FindBestWindowedMode(false, false) : FindBestFullscreenMode(false, false);
          D3DConfiguration configuration = FindBestWindowedMode(false, false);
          if (configuration == null)
          {
            Log.Error("D3DSetup: Failed to find best windowed display mode.");
            Environment.Exit(0);
          }

          // Initialize the 3D environment for the app
          try
          {
            return CreateDevice(configuration);
          }
          catch (Exception e)
          {
            Log.Error("D3DSetup: Failed to initialize device. Falling back to reference rasterizer.", e);
            if (configuration.DeviceInfo.DevType == DeviceType.Hardware)
            {
              // Let the user know we are switching from HAL to the reference rasterizer
              //HandleException(e, ApplicationMessage.WarnSwitchToRef);

              //configuration = Windowed ? FindBestWindowedMode(false, true) : FindBestFullscreenMode(false, true);
              configuration = FindBestWindowedMode(false, true);

              if (configuration == null)
              {
                Log.Error("D3DSetup: Failed to find display mode for reference rasterizer.");
                Environment.Exit(0);
              }

              return CreateDevice(configuration);
            }
          }
        }
        catch (Exception ex)
        {
          //HandleException(ex, ApplicationMessage.ApplicationMustExit);
          Log.Error("D3DSetup: Failed to initialize device. Falling back to reference rasterizer.", ex);
        }
        Environment.Exit(0);
      }

      return null;
    }


    public Device CreateDevice(D3DConfiguration configuration)
    {
      // Set up the presentation parameters
      //BuildPresentParams(Windowed);
      _presentParams = BuildPresentParamsFromSettings(_currentGraphicsConfiguration = configuration);

      Log.Debug("D3D: CreateDevice() - Info, Adapter: {0}, DevType: {1}, BBW: {2}, BBH: {3}, BBC: {4}, Hz: {5}, PI: {6}, Wind: {7}, Flags: ({8})",
                 AdapterInfo.AdapterOrdinal,
                 _deviceType,
                 _presentParams.BackBufferWidth,
                 _presentParams.BackBufferHeight,
                 _presentParams.BackBufferCount,
                 _presentParams.FullScreenRefreshRateInHz,
                 _presentParams.PresentationInterval,
                 _presentParams.Windowed,
                 (_createFlags | CreateFlags.Multithreaded | CreateFlags.EnablePresentStatistics)
                 );

      // backup _presentParams for later use (standby)
      _presentParamsBackup = _presentParams;
      Log.Debug("D3D: Backup PresentParams with buffer size set to: {0}x{1}", _presentParamsBackup.BackBufferWidth, _presentParamsBackup.BackBufferHeight);



      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DX9Device.Dispose();
        GUIGraphicsContext.DX9Device = null;
      }

      // Create the device
      if (GUIGraphicsContext.IsDirectX9ExUsed())
      {
        // Vista or later, use DirectX9Ex device
        Log.Info("D3D: Creating DirectX9Ex device");
        GUIGraphicsContext.Create(CreateDirectX9ExDevice(), _presentParams);
      }
      else
      {
        Log.Info("D3D: Creating DirectX9 device");
        GUIGraphicsContext.Create(new Device(GUIGraphicsContext.Direct3D, AdapterInfo.AdapterOrdinal,
                                                  _deviceType,
                                                  _renderTarget.Handle,
                                                  _createFlags | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                                                  _presentParams), _presentParams);
      }

      // update some magic number use for animations
      GUIGraphicsContext.MaxFPS = _currentGraphicsConfiguration.DisplayMode.RefreshRate;

      // set always on top parameter
      TopMost = _alwaysOnTop;

      // Set up the fullscreen cursor
      if (_showCursorWhenFullscreen && !Windowed)
      {
        var ourCursor = Cursor;
        if (GUIGraphicsContext.DX9Device != null)
        {
          //GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
          GUIGraphicsContext.DX9Device.ShowCursor = true;
        }
      }

      // Setup the event handlers for our device
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DeviceLost += OnDeviceLost;
        if (this._DeviceStatusWatchdog != null)
          this._DeviceStatusWatchdog.Enabled = true;
      }

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
        if (GUIGraphicsContext.DX9Device != null)
        {
          GUIGraphicsContext.DX9Device.Dispose();
        }
        GUIGraphicsContext.DX9Device = null;
      }

      return GUIGraphicsContext.DX9Device;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Capabilities GetCapabilities()
    {
      Log.Debug("D3D: GetCapabilities()");

      Capabilities capabilities = default(Capabilities);

      // retry ever 100 ms to get the capabilities for the selected adapter for a maximum time of 100 tries
      if (!successful)
      {
        do
        {
          try
          {
            capabilities = GUIGraphicsContext.Direct3D.Adapters[0].GetCaps(DeviceType.Hardware);

            successful = true;
          }
          catch (Exception ex)
          {
            retries++;
            if (AdapterInfo != null)
            {
              Log.Warn("Main: Failed to get capabilities for adapter #{0}: {1} (retry in {2}ms) try reinit #{3} {4}", AdapterInfo.AdapterOrdinal, AdapterInfo.ToString(), delayBetweenTries, retries, ex.Message);
            }
            else
            {
              Log.Warn("Main: Failed to get capabilities for adapter (retry in {0}ms) try reinit #{1} {2}", delayBetweenTries, retries, ex.Message);
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
        catch { }
        // ReSharper restore EmptyGeneralCatchClause
      }

      return capabilities;
    }


    /// <summary>
    /// Creates a DirectX9Ex Device
    /// </summary>
    private DeviceEx CreateDirectX9ExDevice()
    {
      // Create the device
      DeviceEx direct3D9Ex = new DeviceEx((Direct3DEx)GUIGraphicsContext.Direct3D,
          AdapterInfo.AdapterOrdinal,
          _deviceType,
          _renderTarget.Handle,
          _createFlags | CreateFlags.Multithreaded | CreateFlags.EnablePresentStatistics,
          _presentParams);

      Log.Debug("D3D: CreateDirectX9ExDevice(), hr: {0}", direct3D9Ex != null ? "success" : "failed");

      if (direct3D9Ex != null)
      {
        direct3D9Ex.Reset(_presentParams);
        try
        {
          if (AdapterInfo.AdapterOrdinal > -1 && GUIGraphicsContext.Direct3D.Adapters.Count > AdapterInfo.AdapterOrdinal)
          {
            Log.Info("D3D: Current refreshrate = {0}Hz ", _currentGraphicsConfiguration.DisplayMode.RefreshRate);
          }
        }
        catch (Exception e)
        {
          Log.Error("Error determining refreshrate: {0}", e.Message);
        }
      }
      else
      {
        Log.Error("D3D: CreateDirectX9ExDevice(), could not create device, hr: -1");
        // ReSharper disable LocalizableElement
        MessageBox.Show("Direct3D device could not be created.", "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        // ReSharper restore LocalizableElement

        // Reset backup values to sensible values in case this has caused the error
        using (var xmlWriter = new MPSettings())
        {
          Log.Debug("D3D: Reset 'backupsize' values after error");
          var size = CalcMaxClientArea();
          xmlWriter.SetValue("gui", "lastlocationx", 0);
          xmlWriter.SetValue("gui", "lastlocationy", 0);
          xmlWriter.SetValue("gui", "backupsizewidth", size.Width);
          xmlWriter.SetValue("gui", "backupsizeheight", size.Height);
        }

        try
        {
          Close();
        }
        // ReSharper disable EmptyGeneralCatchClause
        catch { }
        // ReSharper restore EmptyGeneralCatchClause
      }

      return direct3D9Ex;
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
          var itemDVD = new PlayListItem { FileName = g_Player.CurrentFile, Played = true, Type = PlayListItem.PlayListItemType.DVD };
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
              // send resume thread async
              var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SET_RESUME_STATE, 0, 0, 0, 0, 0, null);
              GUIWindowManager.SendThreadMessage(msg);
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
      if (IsDisposed)
      {
        return;
      }

      if (!AutoHideMouse)
      {
        return;
      }

      if (Thread.CurrentThread.Name != "MPMain" && Thread.CurrentThread.Name != "Config Main")
      {
        return;
      }

      // check if we are over the client area of the form
      bool isOverForm = false;
      try
      {
        if (MediaPortalApp.ActiveForm != null)
          isOverForm = ClientRectangle.Contains(PointToClient(MousePosition));
      }
      catch (Exception ex)
      {
        Log.Error("D3D: UpdateMouseCursor {0}", ex.Message);
        isOverForm = false;
      }

      // check if we are focused
      bool focused;
      try
      {
        focused = GetForegroundWindow() == Handle;
      }
      catch (Exception ex)
      {
        Log.Error("D3D: UpdateMouseCursor {0}", ex.Message);
        focused = false;
      }

      // don't update cursor status when not over client area or not focused
      if (!isOverForm && !focused && !MouseCursor)
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
        GUIGraphicsContext.DeviceLost -= OnDeviceLost;
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
      // disable event handlers
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DeviceLost -= OnDeviceLost;
      }

      // Restore GUIGraphicsContext.State
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING;
        Log.Debug("D3D: ToggleMiniTV() - set GUIGraphicsContext.State.SUSPENDING");
      }

      if (Windowed)
      {
        _miniTvMode = !_miniTvMode;
        _menuItemMiniTv.Checked = _miniTvMode;

        Size size;
        if (_miniTvMode)
        {
          FormBorderStyle = FormBorderStyle.SizableToolWindow;
          Menu = null;
          _alwaysOnTop = true;
          size = CalcMaxClientArea();
          size.Width /= 3;
          size.Height /= 3;
        }
        else
        {
          FormBorderStyle = FormBorderStyle.Sizable;
          Menu = _menuStripMain;
          using (Settings xmlreader = new MPSettings())
          {
            _alwaysOnTop = xmlreader.GetValueAsBool("general", "alwaysontop", false);
          }
          size = CalcMaxClientArea();
        }
        ClientSize = size;
        TopMost = _alwaysOnTop;

        //// Force a madVR refresh to resize MP window
        //// TODO how to handle it better
        //g_Player.RefreshMadVrVideo();
        GUIGraphicsContext.ForceMadVRRefresh3D = true;
        GUIGraphicsContext.ForceMadVRRefresh = true;

        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
            GUIGraphicsContext.InVmr9Render && !IsToggleMiniTV)
        {
          GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ONDISPLAYMADVRCHANGED, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(message);
          GUIGraphicsContext.NeedRecreateSwapChain = true;
        }
        // for madVR resize
        IsToggleMiniTV = false;
      }

      // Restore GUIGraphicsContext.State
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.SUSPENDING)
      {
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
        Log.Debug("D3D: ToggleMiniTV() - set GUIGraphicsContext.State.RUNNING");
      }

      // disable event handlers
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIGraphicsContext.DeviceLost += OnDeviceLost;
        if (this._DeviceStatusWatchdog != null)
          this._DeviceStatusWatchdog.Enabled = true;
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
            if (lastActiveModule == (int)GUIWindow.Window.WINDOW_TV && lastActiveModuleFullscreen)
            {
              var tvDelayThread = new Thread(TVDelayThread) { IsBackground = true, Name = "TVDelayThread" };
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

      // Set startup bounds
      Bounds = GUIGraphicsContext.currentScreen.Bounds;

      // Start Minimize and restore to force MP focus
      if (!MinimizeOnStartup)
      {
        // needed to focus on first run.
        // First save current MP screen
        _firstLoadedScreen = true;
        Screen screenfocus = Screen.FromControl(this);
        this.WindowState = FormWindowState.Minimized;
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
          MediaPortalApp.ShowStartupWarningDialogs();
        }

        if (MinimizeOnStartup && _firstTimeWindowDisplayed)
        {
          Log.Info("D3D: Minimizing to tray on startup");
          ExitToTray = true;
          MinimizeToTray();
          _firstTimeWindowDisplayed = false;
        }

        if (_useFcuBlackScreenFix && AppActive)
        {
          // Workaround for Win10 FCU and blackscreen
          ForceMpAlive();
        }

        // Set Cursor.Position to avoid mouse cursor show up itself (for ex on video)
        Log.Debug("D3D: Force mouse cursor to false");
        ShowMouseCursor(false);
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
      //TODO: Remove hard coded shortcuts and use actions instead
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
        //SL: Why is it done this way? Surely the derived class (MediaPortal.cs) could register to the event as well rather than using that virtual function.
        // Also once hard coded shortcuts above are removed we probably don't need that event handler here.
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
      //SL: Why is it done this way? Surely the derived class (MediaPortal.cs) could register to the event as well rather than using that virtual function.
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        // Need to use this hack when wndProc receive double event when madVR in use and exclusive mode.
        var timeSpam = DateTime.Now - KeyEventTimer;
        if (e?.KeyChar == PreviousKeyEvent?.KeyChar && timeSpam.TotalMilliseconds < 20)
        {
          return;
        }
        KeyPressEvent(e);
        PreviousKeyEvent = e;
        KeyEventTimer = DateTime.Now;
      }
      else
      {
        KeyPressEvent(e);
      }
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
      if ((GUIGraphicsContext.DX9Device != null) && (!GUIGraphicsContext.DX9Device.IsDisposed))
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

      // Workaround for Win10 FCU and blackscreen
      _forceMpAlive = true;

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

      // Workaround for Win10 FCU and blackscreen
      _forceMpAlive = true;

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
      //else if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
      //         GUIGraphicsContext.Vmr9Active && VMR9Util.g_vmr9 != null)
      //{
      //  // Hack to not close MP while madVR running (need to find why some plugin like OV trigger this)
      //  _isClosing = false;
      //  formClosingEventArgs.Cancel = true;
      //  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
      //  g_Player.Stop();
      //  Log.Debug("D3D: OnFormClosing() avoiding for madVR while running");
      //}
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
      // Store MP Windowed
      using (var xmlWriter = new MPSettings())
      {
        var backupSize = ClientSize;
        var sizeMaxClient = CalcMaxClientArea();
        Log.Debug("D3D: Dispose() ClientSize: {0}x{1}, MaxClientSize: {2}x{3}", backupSize.Width, backupSize.Height, sizeMaxClient.Width, sizeMaxClient.Height);

        if (Windowed)
        {
          if (backupSize.Width < 256 || backupSize.Height < 256 || backupSize.Width > sizeMaxClient.Width ||
              backupSize.Height > sizeMaxClient.Height)
          {
            xmlWriter.SetValue("gui", "lastlocationx", 0);
            xmlWriter.SetValue("gui", "lastlocationy", 0);
            xmlWriter.SetValue("gui", "backupsizewidth", sizeMaxClient.Width);
            xmlWriter.SetValue("gui", "backupsizeheight", sizeMaxClient.Height);
          }
          else
          {
            xmlWriter.SetValue("gui", "lastlocationx", Location.X);
            xmlWriter.SetValue("gui", "lastlocationy", Location.Y);
            xmlWriter.SetValue("gui", "backupsizewidth", backupSize.Width);
            xmlWriter.SetValue("gui", "backupsizeheight", backupSize.Height);
          }
        }
      }

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


    /// <summary>
    /// Build presentation parameters from the current settings.
    /// </summary>
    public void BuildPresentParamsFromSettings()
    {
      _presentParams = BuildPresentParamsFromSettings(_currentGraphicsConfiguration);
    }

    /// <summary>
    /// Build presentation parameters from the given settings.
    /// </summary>
    /// <param name="configuration">Graphics configuration to use.</param>
    public PresentParameters BuildPresentParamsFromSettings(D3DConfiguration configuration)
    {
      int backBufferWidth;
      int backBufferHeight;
      if (Windowed)
      {
        backBufferWidth = _renderTarget.ClientRectangle.Width;
        backBufferHeight = _renderTarget.ClientRectangle.Height;

        Size sizeMaxClient = CalcMaxClientArea();
        Log.Info("D3D: BuildPresentParams CalcMaxClientArea from: {0}x{1}", sizeMaxClient.Width, sizeMaxClient.Height);
        Log.Info("D3D: BuildPresentParams screen WorkingArea from: {0}x{1}", backBufferWidth, backBufferHeight);

        int backupSizeWidth = 0;
        int backupSizeHeight = 0;

        using (Settings xmlreader = new MPSettings())
        {
          backupSizeWidth = xmlreader.GetValueAsInt("gui", "backupsizewidth", 0);
          backupSizeHeight = xmlreader.GetValueAsInt("gui", "backupsizeheight", 0);
        }

        // TODO this part need to be checked because it break at runtime when BuildPresentParams is rebuilded
        if ((backupSizeWidth != 0 && backupSizeHeight != 0) && _firstTimeLoadingParams)
        {
          backBufferWidth = backupSizeWidth;
          backBufferHeight = backupSizeHeight;
        }

        // Sanity check and replace with sensible size values if necessary
        if (backBufferWidth < 256 || backBufferHeight < 256 || backBufferWidth > sizeMaxClient.Width || backBufferHeight > sizeMaxClient.Height)
        {
          Log.Debug("D3D: BuildPresentParams(), invalid size {0} x {1} changed to {2} x {3}", backBufferWidth, backBufferHeight, sizeMaxClient.Width, sizeMaxClient.Height);
          backBufferWidth = sizeMaxClient.Width;
          backBufferHeight = sizeMaxClient.Height;
        }
      }
      else
      {
        backBufferWidth = configuration.DisplayMode.Width;
        backBufferHeight = configuration.DisplayMode.Height;
      }

      Log.Debug("BuildPresentParamsFromSettings: Windowed = {0},  {1} x {2}",
          configuration.DeviceCombo.IsWindowed, backBufferWidth, backBufferHeight);

      PresentParameters result = new PresentParameters();

      //AppSettings settings = ServiceRegistration.Get<ISettingsManager>().Load<AppSettings>();

      DeviceCombo dc = configuration.DeviceCombo;
      //MultisampleType mst = settings.MultisampleType;
      MultisampleType mst = MultisampleType.None;
      mst = dc.MultisampleTypes.ContainsKey(mst) ? mst : MultisampleType.None;
      result.MultiSampleType = mst;
      result.MultiSampleQuality = 0;
      result.EnableAutoDepthStencil = false;
      result.AutoDepthStencilFormat = dc.DepthStencilFormats.FirstOrDefault(dsf =>
          !dc.DepthStencilMultiSampleConflicts.Contains(new DepthStencilMultiSampleConflict { DepthStencilFormat = dsf, MultisampleType = mst }));
      // Note that PresentFlags.Video makes NVidia graphics drivers switch off multisampling antialiasing
      result.PresentFlags = PresentFlags.None;
      // Attention: assigning the Form's handle to DeviceWindowHandle resets its Location!
      System.Drawing.Point location = _renderTarget.Location;
      result.DeviceWindowHandle = _renderTarget.Handle;
      _renderTarget.Location = location;
      result.Windowed = configuration.DeviceCombo.IsWindowed;
      result.BackBufferFormat = configuration.DeviceCombo.BackBufferFormat;
#if PROFILE_PERFORMANCE
      result.BackBufferCount = 20; // Such high backbuffer count is only useful for benchmarking so that rendering is not limited by backbuffer count
      result.PresentationInterval = PresentInterval.One;
#else
      result.BackBufferCount = 4; // 2 to 4 are recommended for FlipEx swap mode
      result.PresentationInterval = PresentInterval.One;
#endif
      result.FullScreenRefreshRateInHz = result.Windowed ? 0 : configuration.DisplayMode.RefreshRate;

      // From http://msdn.microsoft.com/en-us/library/windows/desktop/bb173422%28v=vs.85%29.aspx :
      // To use multisampling, the SwapEffect member of D3DPRESENT_PARAMETER must be set to D3DSWAPEFFECT_DISCARD.
      // SwapEffect must be set to SwapEffect.FlipEx to support the Present property to be Present.ForceImmediate
      // (see http://msdn.microsoft.com/en-us/library/windows/desktop/bb174343%28v=vs.85%29.aspx )
      result.SwapEffect = mst == MultisampleType.None ? SwapEffect.FlipEx : SwapEffect.Discard;

      result.BackBufferWidth = backBufferWidth;
      result.BackBufferHeight = backBufferHeight;

      return result;
    }


    /// <summary>
    /// Returns a settings object with best available windowed mode, according to 
    /// the <paramref name="doesRequireHardware"/> and <paramref name="doesRequireReference"/> constraints.  
    /// </summary>
    /// <param name="doesRequireHardware">The device requires hardware support.</param>
    /// <param name="doesRequireReference">The device requires the ref device.</param>
    /// <returns><c>true</c> if a mode is found, <c>false</c> otherwise.</returns>
    public D3DConfiguration FindBestWindowedMode(bool doesRequireHardware, bool doesRequireReference)
    {
      D3DConfiguration result = new D3DConfiguration();

      // Get display mode of primary adapter (which is assumed to be where the window will appear)
      DisplayMode primaryDesktopDisplayMode = GUIGraphicsContext.Direct3D.Adapters[0].CurrentDisplayMode;
      GraphicsAdapterInfo bestAdapterInfo = null;
      GraphicsDeviceInfo bestDeviceInfo = null;
      DeviceCombo bestDeviceCombo = null;
      foreach (GraphicsAdapterInfo adapterInfo in _enumerationSettings.AdapterInfoList)
      {
        /*
        if (GUIGraphicsContext._useScreenSelector)
        {
          adapterInfo = FindAdapterForScreen(GUI.Library.GUIGraphicsContext.currentScreen);
          primaryDesktopDisplayMode = Direct3D.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
        }*/
        foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfos)
        {
          if (doesRequireHardware && deviceInfo.DevType != DeviceType.Hardware)
            continue;
          if (doesRequireReference && deviceInfo.DevType != DeviceType.Reference)
            continue;

          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceCombos)
          {
            if (!deviceCombo.IsWindowed)
              continue;
            if (deviceCombo.AdapterFormat != primaryDesktopDisplayMode.Format)
              continue;

            bool adapterMatchesBackBuffer = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);

            // If we haven't found a compatible DeviceCombo yet, or if this set
            // is better (because it's a HAL, and/or because formats match better),
            // save it
            if (bestDeviceCombo == null ||
                bestDeviceCombo.DevType != DeviceType.Hardware && deviceInfo.DevType == DeviceType.Hardware ||
                deviceCombo.DevType == DeviceType.Hardware && adapterMatchesBackBuffer)
            {
              bestAdapterInfo = adapterInfo;
              bestDeviceInfo = deviceInfo;
              bestDeviceCombo = deviceCombo;
              if (deviceInfo.DevType == DeviceType.Hardware && adapterMatchesBackBuffer)
                // This windowed device combo looks great -- take it
                goto EndWindowedDeviceComboSearch;
              // Otherwise keep looking for a better windowed device combo
            }
          }
        }
        //if (GUIGraphicsContext._useScreenSelector)
        //  break;// no need to loop again.. result would be the same
      }

    EndWindowedDeviceComboSearch:
      if (bestDeviceCombo == null)
        return null;

      result.AdapterInfo = bestAdapterInfo;
      result.DeviceInfo = bestDeviceInfo;
      result.DeviceCombo = bestDeviceCombo;
      result.DisplayMode = primaryDesktopDisplayMode;

      return result;
    }

    /// <summary>
    /// Returns a settings object with best available fullscreen mode, according to 
    /// the <paramref name="doesRequireHardware"/> and <paramref name="doesRequireReference"/> constraints.  
    /// </summary>
    /// <param name="doesRequireHardware">The device requires hardware support.</param>
    /// <param name="doesRequireReference">The device requires the ref device.</param>
    /// <returns><c>true</c> if a mode is found, <c>false</c> otherwise.</returns>
    public D3DConfiguration FindBestFullscreenMode(bool doesRequireHardware, bool doesRequireReference)
    {
      D3DConfiguration result = new D3DConfiguration();

      // For fullscreen, default to first HAL DeviceCombo that supports the current desktop 
      // display mode, or any display mode if HAL is not compatible with the desktop mode, or 
      // non-HAL if no HAL is available
      DisplayMode bestAdapterDesktopDisplayMode = new DisplayMode();

      GraphicsAdapterInfo bestAdapterInfo = null;
      GraphicsDeviceInfo bestDeviceInfo = null;
      DeviceCombo bestDeviceCombo = null;

      foreach (GraphicsAdapterInfo adapterInfo in _enumerationSettings.AdapterInfoList)
      {
        //if (GUIGraphicsContext._useScreenSelector)
        //  adapterInfo = FindAdapterForScreen(GUI.Library.GUIGraphicsContext.currentScreen);

        DisplayMode adapterDesktopDisplayMode = GUIGraphicsContext.Direct3D.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
        foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfos)
        {
          if (doesRequireHardware && deviceInfo.DevType != DeviceType.Hardware)
            continue;
          if (doesRequireReference && deviceInfo.DevType != DeviceType.Reference)
            continue;

          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceCombos)
          {
            if (deviceCombo.IsWindowed)
              continue;

            bool adapterMatchesBackBuffer = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);
            bool adapterMatchesDesktop = (deviceCombo.AdapterFormat == adapterDesktopDisplayMode.Format);

            // If we haven't found a compatible set yet, or if this set
            // is better (because it's a HAL, and/or because formats match better),
            // save it
            if (bestDeviceCombo == null ||
                bestDeviceCombo.DevType != DeviceType.Hardware && deviceInfo.DevType == DeviceType.Hardware ||
                bestDeviceCombo.DevType == DeviceType.Hardware &&
                bestDeviceCombo.AdapterFormat != adapterDesktopDisplayMode.Format && adapterMatchesDesktop ||
                bestDeviceCombo.DevType == DeviceType.Hardware && adapterMatchesDesktop && adapterMatchesBackBuffer)
            {
              bestAdapterDesktopDisplayMode = adapterDesktopDisplayMode;
              bestAdapterInfo = adapterInfo;
              bestDeviceInfo = deviceInfo;
              bestDeviceCombo = deviceCombo;
              if (deviceInfo.DevType == DeviceType.Hardware && adapterMatchesDesktop && adapterMatchesBackBuffer)
                // This fullscreen device combo looks great -- take it
                goto EndFullscreenDeviceComboSearch;
              // Otherwise keep looking for a better fullscreen device combo
            }
          }
        }
        //if (GUIGraphicsContext._useScreenSelector)
        //  break;// no need to loop again.. result would be the same
      }

    EndFullscreenDeviceComboSearch:
      if (bestDeviceCombo == null)
        return null;

      // Need to find a display mode on the best adapter that uses pBestDeviceCombo->AdapterFormat
      // and is as close to bestAdapterDesktopDisplayMode's res as possible
      foreach (DisplayMode displayMode in bestAdapterInfo.DisplayModes)
      {
        if (displayMode.Format != bestDeviceCombo.AdapterFormat)
          continue;
        if (displayMode.Width == bestAdapterDesktopDisplayMode.Width &&
            displayMode.Height == bestAdapterDesktopDisplayMode.Height &&
            displayMode.RefreshRate == bestAdapterDesktopDisplayMode.RefreshRate)
          _desktopDisplayMode = displayMode;
      }

      result.DisplayMode = bestAdapterDesktopDisplayMode;
      result.AdapterInfo = bestAdapterInfo;
      result.DeviceInfo = bestDeviceInfo;
      result.DeviceCombo = bestDeviceCombo;

      return result;
    }

    /// <summary>
    /// Device status watchdog(timer)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cbDeviceStatusWatchdog(object sender, EventArgs e)
    {
      if (GUIGraphicsContext.DX9Device is DeviceEx)
      {
        if (((DeviceEx)GUIGraphicsContext.DX9Device).CheckDeviceState(_renderTarget.Handle) == DeviceState.DeviceLost)
        {
          //Disable watchdog
          this._DeviceStatusWatchdog.Enabled = false;

          //Rise DeviceLost event
          GUIGraphicsContext.OnDeviceLost(this, EventArgs.Empty);
        }
      }
    }

  }
}