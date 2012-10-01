#region Copyright (C) 2005-2012 Team MediaPortal

// Copyright (C) 2005-2012 Team MediaPortal
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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
using D3D = Microsoft.DirectX.Direct3D;
using WPFMediaKit.DirectX;

#endregion

namespace MediaPortal
{
  #region D3Dapp

  /// <summary>
  /// The base class for all the graphics (D3D) samples, it derives from windows forms
  /// </summary>
  public class D3DApp : MPForm
  {
    #region internal attributes
  
    internal static int ScreenNumberOverride; // 0 or higher means it is set
    
    #endregion

    #region protected attributes

    protected static bool    FullscreenOverride;       //
    protected static bool    WindowedOverride;         //
    protected static string  SkinOverride;             //
    protected string         FrameStatsLine1;          // 1st string to hold frame stats
    protected string         FrameStatsLine2;          // 2nd string to hold frame stats
    protected bool           MinimizeOnStartup;        // minimize to tray on startup?
    protected bool           MinimizeOnGuiExit;        // minimize to tray on GUI exit?
    protected bool           ShuttingDown;             // 
    protected bool           FirstTimeWindowDisplayed; //
    protected bool           AutoHideMouse;            //
    protected bool           AppActive;                // Is app active?          
    protected bool           ShowCursor;               //
    protected bool           Windowed;                 // Are we in windowed mode?
    protected bool           AutoHideTaskbar;          // 
    protected bool           UseEnhancedVideoRenderer; //
    protected int            Frames;                   // Number of frames since our last update
    protected int            Volume;                   //
    protected PlayListPlayer PlaylistPlayer;           //
    protected DateTime       MouseTimeOutTimer;        //

    #endregion

    #region private attributes

    private readonly Control           _renderTarget;             //
    private readonly PresentParameters _presentParams;            // D3D presentation parameters
    private readonly D3DSettings       _graphicsSettings;         //
    private readonly D3DEnumeration    _enumerationSettings;      // We need to keep track of our enumeration settings
    private readonly bool              _useExclusiveDirectXMode;  // 
    private readonly bool              _alwaysOnTopConfig;        //
    private readonly bool              _disableMouseEvents;       //
    private readonly bool              _showCursorWhenFullscreen; // Whether to show cursor when fullscreen
    private bool                       _miniTvMode;               // minitv means minimum size < 720, always on top, focus may leave
    private bool                       _isClosing;                // closing app?
    private bool                       _lastShowCursor;           //
    private bool                       _fromTray;                 // restoring from tray?
    private bool                       _needReset;                //
    private bool                       _wasPlayingVideo;          //
    private bool                       _alwaysOnTop;              //
    private int                        _lastActiveWindow;         //
    private long                       _lastTime;                 //
    private double                     _currentPlayerPos;         //
    private string                     _currentFile;              //
    private Rectangle                  _oldBounds;                //
    private IContainer                 _components;               //
    private MainMenu                   _menuStripMain;            // 
    private MenuItem                   _menuItemFile;             //
    private MenuItem                   _menuItemExit;             //
    private MenuItem                   _menuItemOptions;          //
    private MenuItem                   _menuItemConfiguration;    //
    private MenuItem                   _menuItemWizards;          //
    private MenuItem                   _menuItemDVD;              //
    private MenuItem                   _menuItemMovies;           //
    private MenuItem                   _menuItemMusic;            //
    private MenuItem                   _menuItemPictures;         //
    private MenuItem                   _menuItemTV;               //
    private MenuItem                   _menuItemContext;          //
    private MenuItem                   _menuItemEmpty;            //
    private MenuItem                   _menuItemFullscreen;       //
    private MenuItem                   _menuItemMiniTv;           //
    private NotifyIcon                 _notifyIcon;               //
    private ContextMenu                _contextMenu;              //
    private PlayListType               _currentPlayListType;      //
    private PlayList                   _currentPlayList;          //
    private Win32API.MSG               _msgApi;                   //
    
    #endregion

    #region constructor

    /// <summary>
    /// Constructor
    /// </summary>
    protected D3DApp()
    {
      SkinOverride              = string.Empty;
      WindowedOverride          = false;
      FullscreenOverride        = false;
      ScreenNumberOverride       = -1;
      FirstTimeWindowDisplayed  = true;
      MinimizeOnStartup         = false;
      MinimizeOnGuiExit         = false;
      ShuttingDown              = false;
      AutoHideMouse             = false;
      ShowCursor                = false;
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
      _lastShowCursor           = true;
      _showCursorWhenFullscreen = false;
      _currentPlayListType      = PlayListType.PLAYLIST_NONE;
      _enumerationSettings      = new D3DEnumeration();
      _graphicsSettings         = new D3DSettings();
      _presentParams            = new PresentParameters();
      _renderTarget             = this;

      using (Settings xmlreader = new MPSettings())
      {
        _useExclusiveDirectXMode = xmlreader.GetValueAsBool("general", "exclusivemode", true);
        UseEnhancedVideoRenderer = xmlreader.GetValueAsBool("general", "useEVRenderer", false);
        _disableMouseEvents      = xmlreader.GetValueAsBool("remote", "CentareaJoystickMap", false);
        AutoHideTaskbar          = xmlreader.GetValueAsBool("general", "hidetaskbar", true);
        _alwaysOnTopConfig       = xmlreader.GetValueAsBool("general", "alwaysontop", false);
      }
      _alwaysOnTop = _alwaysOnTopConfig;

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
    protected virtual void Initialize() { }


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
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnDeviceReset(Object sender, EventArgs e) { }


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
    /// Picks the best graphics device, and initializes it
    /// </summary>
    /// <returns>true if a good device was found, false otherwise</returns>
    /// 
    protected bool CreateGraphicsSample()
    {
      _enumerationSettings.ConfirmDeviceCallback = ConfirmDevice;
      _enumerationSettings.Enumerate();

      if (_renderTarget.Cursor == null)
      {
        // Set up a default cursor
        _renderTarget.Cursor = Cursors.Default;
      }

      // if our render target is the main window and we haven't said ignore the menus, add our menu
      if (_renderTarget == this)
      {
        Menu = _menuStripMain;
      }

      try
      {
        ChooseInitialSettings();
        DXUtil.Timer(DirectXTimer.Start);

        // Initialize the application timer
        var formOnScreen = Screen.FromRectangle(Bounds);
        if (!formOnScreen.Equals(GUIGraphicsContext.currentScreen))
        {
          var location = Location;
          location.X = location.X - formOnScreen.Bounds.Left + GUIGraphicsContext.currentScreen.Bounds.Left;
          location.Y = location.Y - formOnScreen.Bounds.Top + GUIGraphicsContext.currentScreen.Bounds.Top;
          Location = location;
        }
        _oldBounds = Bounds;
        using (Settings xmlreader = new MPSettings())
        {
          var startFullscreen = !WindowedOverride && (FullscreenOverride || xmlreader.GetValueAsBool("general", "startfullscreen", false));
          if (startFullscreen)
          {
            if (AutoHideTaskbar && !MinimizeOnStartup)
            {
              HideTaskBar(true);
            }
            Log.Info("D3D: Starting fullscreen");
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
            Windowed = false;
          }
        }
        
        // Initialize the 3D environment for the app
        // TODO:
        InitializeDevice();
        // Initialize the app's custom scene stuff
        OneTimeSceneInitialization();
      }
      catch (SampleException exception)
      {
        HandleException(exception, ApplicationMessage.ApplicationMustExit);
        return false;
      }
      catch
      {
        HandleException(new SampleException(), ApplicationMessage.ApplicationMustExit);
        return false;
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
    protected virtual void OneTimeSceneInitialization() { }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowed"> </param>
    /// <param name="force"> </param>
    protected void UpdatePresentParams(bool windowed, bool force)
    {
      if (((_useExclusiveDirectXMode && !UseEnhancedVideoRenderer) || GUIGraphicsContext.IsDirectX9ExUsed()) && (windowed != Windowed || force)) 
      {
        GUIGraphicsContext.DX9Device.DeviceLost -= OnDeviceLost;
        GUIGraphicsContext.DX9Device.DeviceReset -= OnDeviceReset;

        BuildPresentParams(windowed);

        try
        {
          GUIGraphicsContext.DX9Device.Reset(_presentParams);
          if (GUIGraphicsContext.IsDirectX9ExUsed() && !UseEnhancedVideoRenderer)
          {
            GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
            GUIFontManager.InitializeDeviceObjects();
          }

          Log.Debug(windowed
                      ? "D3D: Switched to windowed mode successfully"
                      : "D3D: Switched to fullscreen mode successfully");
        }
        catch (Exception ex)
        {
          Log.Warn(windowed
                     ? "D3D: Switch to windowed mode failed - {0}"
                     : "D3D: Switch to fullscreen mode failed - {0}", ex.ToString());
        }

        GUIGraphicsContext.DX9Device.DeviceReset += OnDeviceReset;
        GUIGraphicsContext.DX9Device.DeviceLost += OnDeviceLost;

        if (windowed)
        {
          TopMost = _alwaysOnTop;
        }
        Activate();
      }
    }


    /// <summary>
    /// 
    /// </summary>
    protected void ToggleFullscreen()
    {
      Log.Info("D3D: Fullscreen / windowed mode toggled");
      // Force player to stop so as not to crash during toggle
      if (GUIGraphicsContext.Vmr9Active)
      {
        Log.Info("D3D: Vmr9Active - Stopping media");
        g_Player.Stop();
      }
      GUITextureManager.CleanupThumbs();
      GUITextureManager.Dispose();
      GUIFontManager.Dispose();
      if (Windowed)
      {
        Log.Info("D3D: Switching from windowed mode to fullscreen");
        if (AutoHideTaskbar)
        {
          HideTaskBar(true);
        }

        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Menu = null;
        _oldBounds = Bounds;
        var newBounds = GUIGraphicsContext.currentScreen.Bounds;
        Bounds = newBounds;
        Update();
        Log.Info("D3D: Switching windowed mode to fullscreen done - Windowed: {0}", Windowed);
        Log.Info("D3D: Client size: {0}x{1} - Screen: {2}x{3}", ClientSize.Width, ClientSize.Height,
                 GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
        UpdatePresentParams(false, false);
      }
      else
      {
        Log.Info("D3D: Switching from fullscreen to windowed mode");
        if (AutoHideTaskbar)
        {
          HideTaskBar(false);
        }
        WindowState = FormWindowState.Normal;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        Menu = _menuStripMain;
        var newBounds = new Rectangle(_oldBounds.X, _oldBounds.Y, _oldBounds.Width, _oldBounds.Height);
        using (Settings xmlreader = new MPSettings())
        {
          var autosize = xmlreader.GetValueAsBool("gui", "autosize", true);
          if (autosize && Windowed)
          {
            newBounds.Height = GUIGraphicsContext.SkinSize.Height;
            newBounds.Width = GUIGraphicsContext.SkinSize.Width;
          }
        }
        Bounds = newBounds;
        Update();
        Log.Info("D3D: Switching fullscreen to windowed mode done - Windowed: {0}", Windowed);
        Log.Info("D3D: Client size: {0}x{1} - Screen: {2}x{3}", ClientSize.Width, ClientSize.Height,
                 GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
        UpdatePresentParams(true, false);
      }
      OnDeviceReset(null, null);
    }


    /// <summary>
    /// Called when our sample has nothing else to do, and it's time to render
    /// </summary>
    protected void FullRender()
    {
      // don't render if minimized and restoring is not in progress
      if (WindowState == FormWindowState.Minimized && !_fromTray)
      {
        Thread.Sleep(100);
        return;
      }

      ResumePlayer();
      HandleCursor();

      // In minitv mode allow to loose focus
      if ((ActiveForm != this) && (_alwaysOnTop) && !_miniTvMode && (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING))
      {
        Activate();
      }

      if (GUIGraphicsContext.Vmr9Active)
      {
        return;
      }

      // Render a frame during idle time (no messages are waiting)
      if (AppActive)
      {
#if !DEBUG
        try
        {
#endif
        if (((GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST) || (ActiveForm != this) || (GUIGraphicsContext.SaveRenderCycles)) && !_fromTray)
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
          Log.Error("d3dapp: Exception: {0}", ex);
        }
#if !DEBUG
        } 
		catch (Exception ee)
        {
          Log.Info("d3dapp: Exception {0}", ee);
          MessageBox.Show("An exception has occurred.  MediaPortal has to be closed.\r\n\r\n" + ee,
                          "Exception",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
          Close();
        }
#endif
      }
      else
      {
        // if we don't got the focus, then don't use all the CPU unless we are restoring from tray
        if (ActiveForm != this || (GUIGraphicsContext.SaveRenderCycles) && !_fromTray)
        {
          Thread.Sleep(100);
        }
      }
    }


    /// <summary>
    /// 
    /// </summary>
    protected void RecoverDevice()
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
      {
        if (g_Player.Playing && !RefreshRateChanger.RefreshRateChangePending)
        {
          g_Player.Stop();
        }

        if (!GUIGraphicsContext.IsDirectX9ExUsed())
        {
          try
          {
            Log.Debug("d3dapp: RecoverDevice called");
            // Test the cooperative level to see if it's OK to render
            GUIGraphicsContext.DX9Device.TestCooperativeLevel();
          }
          catch (DeviceLostException)
          {
            // If the device was lost, do not render until we get it back
            AppActive = false;
            Log.Debug("d3dapp: DeviceLostException");
            return;
          }
          catch (DeviceNotResetException)
          {
            Log.Debug("d3dapp: DeviceNotResetException");
            _needReset = true;
          }
        }
        else
        {
          _needReset = true;
        }

        if (_needReset)
        {
          if (!GUIGraphicsContext.IsDirectX9ExUsed())
          {
            BuildPresentParams(Windowed);
          }

          // Reset the device and resize it
          Log.Warn("d3dapp: Resetting DX9 device");
          try
          {
            GUITextureManager.Dispose();
            GUIFontManager.Dispose();

            if (!GUIGraphicsContext.IsDirectX9ExUsed())
            {
              GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);
              _needReset = false;
            }
            else
            {
              Log.Warn("d3dapp: DirectX9Ex is lost or GPU hung --> Reinit of DX9Ex is needed.");
              GUIGraphicsContext.DX9ExRealDeviceLost = true;
              InitializeDevice();
            }
          }
          catch (Exception ex)
          {
            Log.Error("d3dapp: Reset failed - {0}", ex.ToString());
            GUIGraphicsContext.DX9Device.DeviceLost -= OnDeviceLost;
            GUIGraphicsContext.DX9Device.DeviceReset -= OnDeviceReset;
            InitializeDevice();
            return;
          }

          Log.Debug("d3dapp: EnvironmentResized()");
          EnvironmentResized(new CancelEventArgs());
        }
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
      }
    }


    /// <summary>
    /// Get the  statistics 
    /// </summary>
    protected void GetStats()
    {
      var fmtAdapter = _graphicsSettings.DisplayMode.Format;
      var strFmt = String.Format("backbuf {0}, adapter {1}", GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferFormat, fmtAdapter);
      var strDepthFmt = _enumerationSettings.AppUsesDepthBuffer ? String.Format(" ({0})", _graphicsSettings.DepthStencilBufferFormat.ToString()) : "";
      var strMultiSample = string.Empty;
      
      FrameStatsLine1 = String.Format("last {0} fps ({1}x{2}), {3}{4}{5}",
                                      GUIGraphicsContext.CurrentFPS.ToString("f2"),
                                      GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth,
                                      GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight,
                                      strFmt, strDepthFmt, strMultiSample);

      FrameStatsLine2 = String.Format("");

      if (GUIGraphicsContext.Vmr9Active)
      {
        FrameStatsLine2 = String.Format(GUIGraphicsContext.IsEvr ? "EVR {0} " : "VMR9 {0} ", GUIGraphicsContext.Vmr9FPS.ToString("f2"));
      }

      var quality = String.Format("avg fps:{0} sync:{1} drawn:{2} dropped:{3} jitter:{4}",
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
      var time = Stopwatch.GetTimestamp();
      var diffTime = (float)(time - _lastTime) / Stopwatch.Frequency;
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
    // TODO: restore window dimensions
    // TODO: resume player when in fullscreen mode
    protected void RestoreFromTray()
    {
      Log.Info("D3D: Restoring from tray");
      _fromTray = true;
      AppActive = true;
      _notifyIcon.Visible = false;
      Show();
      WindowState = FormWindowState.Normal;
      Activate();
      ResumePlayer();
      if (!Windowed && AutoHideTaskbar)
      {
        HideTaskBar(true);
      }
    }


    /// <summary>
    /// Minimized MP To System Tray
    /// </summary>
    /// <param name="force">set to true for force hiding in window mode</param>
    // TODO: save window dimensions
    // TODO: pause player when in fullscreenmode
    protected void MinimizeToTray(bool force)
    {
      if (!Windowed || force)
      {
        Log.Info("D3D: Minimizing to tray");
        _fromTray = false;
        AppActive = false;
        _notifyIcon.Visible = true;
        Hide();
        WindowState = FormWindowState.Minimized;
        SavePlayerState();
        if (AutoHideTaskbar)
        {
          HideTaskBar(false);
        }
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

    #endregion

    #region private methods

    /// <summary>
    /// Initialize Menus and Event Handlers
    /// </summary>
    private void InitializeComponent()
    {
      SuspendLayout(); 
      var resources = new ComponentResourceManager(typeof(D3DApp));
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
      KeyPreview = true;
      Name = "D3DApp";
      Load             += OnLoad;
      Closing          += OnClosing;
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

        var info = new NativeMethods.MonitorInformation();
        info.Size = (uint)Marshal.SizeOf(info);
        NativeMethods.GetMonitorInfo(hMon, ref info);
        var rect = Screen.FromRectangle(info.MonitorRectangle).Bounds;
        if (rect.Equals(screen.Bounds))
        {
          return adapterInfo;
        }
      }
      return null;
    }


    /// <summary>
    /// Sets up graphicsSettings with best available windowed mode,
    /// </summary>
    /// <returns>true if a mode is found, false otherwise</returns>
    private bool FindBestWindowedMode()
    {
      var primaryDesktopDisplayMode = Manager.Adapters[0].CurrentDisplayMode;
      GraphicsAdapterInfo bestAdapterInfo = null;
      GraphicsDeviceInfo bestDeviceInfo   = null;
      DeviceCombo bestDeviceCombo         = null;

      foreach (GraphicsAdapterInfo graphicsAdapter in _enumerationSettings.AdapterInfoList)
      {
        var adapterInfo = graphicsAdapter;
        if (GUIGraphicsContext._useScreenSelector)
        {
          adapterInfo = FindAdapterForScreen(GUIGraphicsContext.currentScreen);
          primaryDesktopDisplayMode = Manager.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
          GUIGraphicsContext.currentScreenNumber = adapterInfo.AdapterOrdinal;
        }


        foreach (var deviceInfo in adapterInfo.DeviceInfoList.Cast<GraphicsDeviceInfo>().Where(deviceInfo => deviceInfo.DevType == DeviceType.Hardware))
        {
          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
          {
            if (deviceCombo.IsWindowed && deviceCombo.DevType == DeviceType.Hardware && deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat)
            {
              bestAdapterInfo = adapterInfo;
              bestDeviceInfo  = deviceInfo;
              bestDeviceCombo = deviceCombo;
              break;
            }
          }
        }
        // match found, don't continue iterating
        if (bestDeviceCombo != null)
        {
          break;
        }
      }

      // no match found
      if (bestDeviceCombo == null)
      {
        return false;
      }

      _graphicsSettings.WindowedAdapterInfo = bestAdapterInfo;
      _graphicsSettings.WindowedDeviceInfo  = bestDeviceInfo;
      _graphicsSettings.WindowedDeviceCombo = bestDeviceCombo;
      _graphicsSettings.IsWindowed          = true;
      _graphicsSettings.WindowedDisplayMode = primaryDesktopDisplayMode;
      if (_enumerationSettings.AppUsesDepthBuffer)
      {
        _graphicsSettings.WindowedDepthStencilBufferFormat = (DepthFormat)bestDeviceCombo.DepthStencilFormatList[0];
      }
      _graphicsSettings.WindowedMultisampleType      = (MultiSampleType)bestDeviceCombo.MultiSampleTypeList[0];
      _graphicsSettings.WindowedMultisampleQuality   = 0;
      _graphicsSettings.WindowedVertexProcessingType = (VertexProcessingType)bestDeviceCombo.VertexProcessingTypeList[0];
      _graphicsSettings.WindowedPresentInterval      = (PresentInterval)bestDeviceCombo.PresentIntervalList[0];

      return true;
    }


    /// <summary>
    /// Sets up graphicsSettings with best available fullscreen mode
    /// </summary>
    /// <returns>true if a mode is found, false otherwise</returns>
    private bool FindBestFullscreenMode()
    {
      var bestAdapterDesktopDisplayMode   = new DisplayMode {Width = 0, Height = 0, Format = 0, RefreshRate = 0};
      var bestDisplayMode                 = new DisplayMode {Width = 0, Height = 0, Format = 0, RefreshRate = 0};
      GraphicsAdapterInfo bestAdapterInfo = null;
      GraphicsDeviceInfo  bestDeviceInfo  = null;
      DeviceCombo         bestDeviceCombo = null;
      
      foreach (GraphicsAdapterInfo graphicsAdapter in _enumerationSettings.AdapterInfoList)
      {
        var adapterInfo = graphicsAdapter;
        if (GUIGraphicsContext._useScreenSelector)
        {
          adapterInfo = FindAdapterForScreen(GUIGraphicsContext.currentScreen);
          GUIGraphicsContext.currentFullscreenAdapterInfo = Manager.Adapters[adapterInfo.AdapterOrdinal];
          GUIGraphicsContext.currentScreenNumber = adapterInfo.AdapterOrdinal;
        }

        foreach (var deviceInfo in adapterInfo.DeviceInfoList.Cast<GraphicsDeviceInfo>().Where(deviceInfo => deviceInfo.DevType == DeviceType.Hardware))
        {
          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
          {
            if (!deviceCombo.IsWindowed && deviceCombo.DevType == DeviceType.Hardware && deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat)
            {
              bestAdapterInfo = adapterInfo;
              bestDeviceInfo  = deviceInfo;
              bestDeviceCombo = deviceCombo;
              break;
            }
          }
        }
        // match found, don't continue iterating
        if (bestDeviceCombo != null)
        {
          break;
        }
      }

      // no match found
      if (bestDeviceCombo == null)
      {
        return false;
      }

      // Need to find a display mode on the best adapter that uses pBestDeviceCombo->AdapterFormat
      // and is as close to bestAdapterDesktopDisplayMode's res as possible
      foreach (var displayMode in bestAdapterInfo.DisplayModeList.Cast<DisplayMode>().Where(displayMode => displayMode.Format == bestDeviceCombo.AdapterFormat))
      {
        if (displayMode.Width == bestAdapterDesktopDisplayMode.Width &&
            displayMode.Height == bestAdapterDesktopDisplayMode.Height &&
            displayMode.RefreshRate == bestAdapterDesktopDisplayMode.RefreshRate)
        {
          // found a perfect match, so stop
          bestDisplayMode = displayMode;
          break;
        }

        if (displayMode.Width == bestAdapterDesktopDisplayMode.Width &&
            displayMode.Height == bestAdapterDesktopDisplayMode.Height &&
            displayMode.RefreshRate > bestDisplayMode.RefreshRate)
        {
          // refresh rate doesn't match, but width/height match, so keep this and keep looking
          bestDisplayMode = displayMode;
        }
        else if (bestDisplayMode.Width == bestAdapterDesktopDisplayMode.Width)
        {
          // width matches, so keep this and keep looking
          bestDisplayMode = displayMode;
        }
        else if (bestDisplayMode.Width == 0)
        {
          // we don't have anything better yet, so keep this and keep looking
          bestDisplayMode = displayMode;
        }
      }

      _graphicsSettings.FullscreenAdapterInfo = bestAdapterInfo;
      _graphicsSettings.FullscreenDeviceInfo  = bestDeviceInfo;
      _graphicsSettings.FullscreenDeviceCombo = bestDeviceCombo;
      _graphicsSettings.IsWindowed            = false;
      _graphicsSettings.FullscreenDisplayMode = bestDisplayMode;
      if (_enumerationSettings.AppUsesDepthBuffer)
      {
        _graphicsSettings.FullscreenDepthStencilBufferFormat = (DepthFormat)bestDeviceCombo.DepthStencilFormatList[0];
      }
      _graphicsSettings.FullscreenMultisampleType      = (MultiSampleType)bestDeviceCombo.MultiSampleTypeList[0];
      _graphicsSettings.FullscreenMultisampleQuality   = 0;
      _graphicsSettings.FullscreenVertexProcessingType = (VertexProcessingType)bestDeviceCombo.VertexProcessingTypeList[0];
      _graphicsSettings.FullscreenPresentInterval      = PresentInterval.Default;

      return true;
    }


    /// <summary>
    /// Choose the initial settings for the application
    /// </summary>
    private void ChooseInitialSettings()
    {
      var foundFullscreenMode = FindBestFullscreenMode();
      var foundWindowedMode   = FindBestWindowedMode();
      if (!foundFullscreenMode && !foundWindowedMode)
      {
        throw new NoCompatibleDevicesException();
      }
    }


    /// <summary>z
    /// Build D3D presentation parameters
    /// </summary>
    /// <param name="windowed">true for window, false for fullscreen</param>
    protected void BuildPresentParams(bool windowed)
    {
      _graphicsSettings.DisplayMode = GUIGraphicsContext.currentFullscreenAdapterInfo.CurrentDisplayMode;
      // workaround for usage of DX9Device.Viewport elsewhere, in windowed mode width and height should be 0. Backbuffer Size != Client Size
      _presentParams.BackBufferWidth            = windowed ? ClientSize.Width  : _graphicsSettings.DisplayMode.Width;
      _presentParams.BackBufferHeight           = windowed ? ClientSize.Height : _graphicsSettings.DisplayMode.Height;
      Log.Debug(windowed
                  ? "Main: Windowed Presentation Parameter Size updated to: {0}x{1}"
                  : "Main: Fullscreen Presentation Parameter Size updated to: {0}x{1}",
                 _presentParams.BackBufferWidth, _presentParams.BackBufferHeight);
      _presentParams.BackBufferFormat          = windowed ? _graphicsSettings.DisplayMode.Format : Format.X8R8G8B8;
      _presentParams.BackBufferCount           = 2;
      _presentParams.MultiSample               = MultiSampleType.None;
      _presentParams.MultiSampleQuality        = 0;
      _presentParams.SwapEffect                = SwapEffect.Discard;
      _presentParams.DeviceWindow              = windowed ? _renderTarget : this;
      _presentParams.Windowed                  = windowed;
      _presentParams.EnableAutoDepthStencil    = false;
      _presentParams.AutoDepthStencilFormat    = windowed ? _graphicsSettings.WindowedDepthStencilBufferFormat : _graphicsSettings.FullscreenDepthStencilBufferFormat;
      _presentParams.PresentFlag               = PresentFlag.Video;  // | PresentFlag.LockableBackBuffer
      _presentParams.FullScreenRefreshRateInHz = windowed ? 0 : _graphicsSettings.DisplayMode.RefreshRate;
      _presentParams.PresentationInterval      = PresentInterval.Default;
      _presentParams.ForceNoMultiThreadedFlag  = false;
      GUIGraphicsContext.DirectXPresentParameters = _presentParams;
      Windowed = windowed;
    }

    /// <summary>
    /// Initialization of the D3D Device
    /// </summary>
    protected void InitializeDevice()
    {
      var adapterInfo = _graphicsSettings.AdapterInfo;
      var deviceInfo = _graphicsSettings.DeviceInfo;

      try
      {
        Log.Info("d3dapp: Graphic adapter '{0}' is using driver version '{1}'",
                 adapterInfo.AdapterDetails.Description.Trim(), adapterInfo.AdapterDetails.DriverVersion.ToString());
        Log.Info("d3dapp: Pixel shaders supported: {0} (Version: {1}), Vertex shaders supported: {2} (Version: {3})",
                 deviceInfo.Caps.PixelShaderCaps.NumberInstructionSlots, deviceInfo.Caps.PixelShaderVersion.ToString(),
                 deviceInfo.Caps.VertexShaderCaps.NumberTemps, deviceInfo.Caps.VertexShaderVersion.ToString());
      }
      catch (Exception lex)
      {
        Log.Warn("d3dapp: Error logging graphic device details - {0}", lex.Message);
      }

      // Set up the presentation parameters
      BuildPresentParams(Windowed);

      if (deviceInfo.Caps.PrimitiveMiscCaps.IsNullReference)
      {
        // Warn user about null ref device that can't render anything
        HandleException(new NullReferenceDeviceException(), ApplicationMessage.None);
      }

      CreateFlags createFlags;
      switch (_graphicsSettings.VertexProcessingType)
      {
        case VertexProcessingType.Software:
          createFlags = CreateFlags.SoftwareVertexProcessing;
          break;
        case VertexProcessingType.Mixed:
          createFlags = CreateFlags.MixedVertexProcessing;
          break;
        case VertexProcessingType.Hardware:
          createFlags = CreateFlags.HardwareVertexProcessing;
          break;
        case VertexProcessingType.PureHardware:
          createFlags = CreateFlags.HardwareVertexProcessing; //CreateFlags.PureDevice;
          break;
        default:
          throw new ApplicationException();
      }

      try
      {
        // Create the device
        if (GUIGraphicsContext.IsDirectX9ExUsed())
        {
          // Vista or later, use DirectX9Ex device
          Log.Info("Creating DirectX9Ex device");
          CreateDirectX9ExDevice(createFlags);
        }
        else
        {
          Log.Info("Creating DirectX9 device");
          GUIGraphicsContext.DX9Device = new Device(_graphicsSettings.AdapterOrdinal,
                                                    _graphicsSettings.DevType,
                                                    Windowed ? _renderTarget : this,
                                                    createFlags | CreateFlags.MultiThreaded | CreateFlags.FpuPreserve,
                                                    _presentParams);
        }
        if (Windowed)
        {
          // TODO: Fix as ClienzSize does not equal Size
          // Make sure main window isn't topmost, so error message is visible
          var currentClientSize = ClientSize;
          Size = ClientSize;
          SendToBack();
          BringToFront();
          ClientSize = currentClientSize;
          TopMost = _alwaysOnTop;
        }

        // Set up the fullscreen cursor
        if (_showCursorWhenFullscreen && !Windowed)
        {
          var ourCursor = Cursor;
          GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
          GUIGraphicsContext.DX9Device.ShowCursor(true);
        }
        // Setup the event handlers for our device
        GUIGraphicsContext.DX9Device.DeviceLost += OnDeviceLost;
        GUIGraphicsContext.DX9Device.DeviceReset += OnDeviceReset;
        // Initialize the app's device-dependent objects
        try
        {
          InitializeDeviceObjects();
          AppActive = true;
        }
        catch (Exception ex)
        {
          Log.Error("D3D: InitializeDeviceObjects - Exception: {0}", ex.ToString());
          GUIGraphicsContext.DX9Device.Dispose();
          GUIGraphicsContext.DX9Device = null;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        if (FindBestWindowedMode())
        {
          Windowed = true;

          // TODO: Fix as ClienzSize does not equal Size
          // Make sure main window isn't topmost, so error message is visible
          var currentClientSize = ClientSize;
          Size = ClientSize;
          SendToBack();
          BringToFront();
          ClientSize = currentClientSize;
          TopMost = _alwaysOnTop;

          // Let the user know we are switching from HAL to the reference rasterizer
          HandleException(null, ApplicationMessage.WarnSwitchToRef);

          InitializeDevice();
        }
      }
    }


    /// <summary>
    /// Creates a DirectX9Ex Device
    /// </summary>
    /// <param name="vertexProcessing">type of vertex processing</param>
    private void CreateDirectX9ExDevice(CreateFlags vertexProcessing)
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
      var o = Marshal.GetIUnknownForObject(direct3D9Ex);
      Marshal.Release(o);

      var displaymodeEx = new D3DDISPLAYMODEEX();
      displaymodeEx.Size = (uint)Marshal.SizeOf(displaymodeEx);
      displaymodeEx.Width  = param.BackBufferWidth;
      displaymodeEx.Height = param.BackBufferHeight;
      displaymodeEx.Format = param.BackBufferFormat;
      displaymodeEx.RefreshRate = param.FullScreen_RefreshRateInHz;
      displaymodeEx.ScanLineOrdering = D3DSCANLINEORDERING.D3DSCANLINEORDERING_UNKNOWN;
      var prt = Marshal.AllocHGlobal(Marshal.SizeOf(displaymodeEx));
      Marshal.StructureToPtr(displaymodeEx, prt, true);

      IntPtr dev;
      var hr = direct3D9Ex.CreateDeviceEx(_graphicsSettings.AdapterOrdinal,
                                          _graphicsSettings.DevType,
                                          Windowed ? _renderTarget.Handle : Handle,
                                          vertexProcessing | CreateFlags.MultiThreaded | CreateFlags.FpuPreserve,
                                          ref param,
                                          Windowed ? IntPtr.Zero : prt,
                                          out dev);
      if (hr == 0)
      {
        GUIGraphicsContext.DX9Device = new Device(dev);
        GUIGraphicsContext.DX9Device.Reset(_presentParams);
      }
      else
      {
        Log.Error("d3dapp: Could not create device");
      }
    }


    /// <summary>
    /// Displays D3D exceptions to the user
    /// </summary>
    /// <param name="e">The exception that was thrown</param>
    /// <param name="type">Extra information on how to handle the exception</param>
    private void HandleException(SampleException e, ApplicationMessage type)
    {
      if (e != null)
      {
        // Build a message to display to the user
        var strMsg = e.Message;
        var strSource = e.Source;
        var strStack = e.StackTrace;
        Log.Error("D3D: Exception: {0} {1} {2}", strMsg, strSource, strStack);
        switch (type)
        {
          case ApplicationMessage.ApplicationMustExit:
            strMsg += "\n\nMediaPortal has to be closed.";
            MessageBox.Show(strMsg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (IsHandleCreated)
            {
              Close();
            }
            break;
          case ApplicationMessage.WarnSwitchToRef:
            strMsg = "\n\nSwitching to the reference rasterizer,\n";
            strMsg += "a software device that implements the entire\n";
            strMsg += "Direct3D feature set, but runs very slowly.";
            MessageBox.Show(strMsg, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            break;
        }
      }
    }


    /// <summary>
    /// Fired when our environment was resized
    /// </summary>
    /// <param name="e">Set the cancel member to true to turn off automatic device reset</param>
    private void EnvironmentResized(CancelEventArgs e)
    {
      // Check to see if we're closing or changing the form style
      if (_isClosing)
      {
        // We are, cancel our reset, and exit
        e.Cancel = true;
        return;
      }

      // Check to see if we're minimizing and our rendering object
      // is not the form, if so, cancel the resize
      if ((_renderTarget != this) && (WindowState == FormWindowState.Minimized) || !AppActive)
      {
        e.Cancel = true;
      }

      // Set up the fullscreen cursor
      if (_showCursorWhenFullscreen && !Windowed)
      {
        var ourCursor = Cursor;
        GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
        GUIGraphicsContext.DX9Device.ShowCursor(true);
      }
    }


    /// <summary>
    /// Save player state (when form was resized)
    /// </summary>
    private void SavePlayerState()
    {
      // Is App not minimized to tray and is a player active?
      if (WindowState != FormWindowState.Minimized &&
          !_wasPlayingVideo &&
          (g_Player.Playing && (g_Player.IsTV || g_Player.IsVideo || g_Player.IsDVD)))
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
        Log.Info(
          "D3D: Form resized - Stopping media - Current playlist: Type: {0} / Size: {1} / Current item: {2} / Filename: {3} / Position: {4}",
          _currentPlayListType, _currentPlayList.Count, PlaylistPlayer.CurrentSong, _currentFile, _currentPlayerPos);
        g_Player.Stop();

        _lastActiveWindow = GUIWindowManager.ActiveWindow;
      }
    }


    /// <summary>
    /// Restore player from saved state (after resizing form)
    /// </summary>
    private void ResumePlayer()
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
    /// Automatically hides or show mouse cursor
    /// </summary>
    private void HandleCursor()
    {
      if (AutoHideMouse)
      {
        if (ShowCursor != _lastShowCursor)
        {
          if (ShowCursor)
          {
            Cursor.Show();
          }
          else
          {
            Cursor.Hide();
          }
          _lastShowCursor = ShowCursor;
        }

        var ts = DateTime.Now - MouseTimeOutTimer;
        if (ShowCursor && ts.TotalSeconds >= 3)
        {
          Cursor.Hide();
          ShowCursor = false;
          Invalidate(true);
        }
      }
    }


    /// <summary>
    /// Set our variables to not active and not ready
    /// </summary>
    private void CleanupEnvironment()
    {
      AppActive = false;
      if (GUIGraphicsContext.DX9Device != null)
      {
        // indicate we are shutting down
        App.IsShuttingDown = true;
        // remove the device lost and reset handlers as application is already closing down
        GUIGraphicsContext.DX9Device.DeviceLost -= OnDeviceLost;
        GUIGraphicsContext.DX9Device.DeviceReset -= OnDeviceReset;
        GUIGraphicsContext.DX9Device.Dispose();
      }
    }


    /// <summary>
    /// 
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

      if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed)
      {
        UpdatePresentParams(true, false);
      }

      AutoHideMouse = false;
      Cursor.Show();
      Invalidate(true);

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
      ShuttingDown = true;
      Close();
    }


    /// <summary>
    /// 
    /// </summary>
    private void ToggleMiniTV()
    {
      if (Windowed)
      {
        _miniTvMode = !_miniTvMode;
        Size size;
        if (_miniTvMode)
        {
          size = new Size(720/2, 576/2);
          _alwaysOnTop = true;
          FormBorderStyle = FormBorderStyle.SizableToolWindow;
          Menu = null;
        }
        else
        {
          size = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
          _alwaysOnTop = _alwaysOnTopConfig;
          FormBorderStyle = FormBorderStyle.Sizable;
          Menu = _menuStripMain;

          UpdatePresentParams(true, false);
        }

        Size = size;
        TopMost = _alwaysOnTop;

        _menuItemMiniTv.Checked = _miniTvMode;
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected void ShowLastActiveModule()
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
        psClientNextWakeUp = xmlreader.GetValueAsString("psclientplugin", "nextwakeup", DateTime.MaxValue.ToString(CultureInfo.InvariantCulture));
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
      Log.Debug("d3dapp: ShowLastActiveModule active : {0}", showLastActiveModule);

      if (showLastActiveModule)
      {
        Log.Debug("d3dapp: ShowLastActiveModule module : {0}", lastActiveModule);
        Log.Debug("d3dapp: ShowLastActiveModule fullscreen : {0}", lastActiveModuleFullscreen);
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
          }
        }
      }
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

      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
      {
        UpdatePresentParams(true, false);
      }
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
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
      {
        UpdatePresentParams(true, false);
      }
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
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
      {
        UpdatePresentParams(true, false);
      }
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
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
      {
        UpdatePresentParams(true, false);
      }
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
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
      {
        UpdatePresentParams(true, false);
      }
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
      Application.Idle += OnIdle;

      Initialize();
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
      // suppress any errors
      catch { }
      ShowLastActiveModule();
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
    private static void OnClosing(object sender, CancelEventArgs e)
    {
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
      g_Player.Stop();
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
      if (!_disableMouseEvents && !ShowCursor)
      {
        Cursor.Show();
        ShowCursor = true;
        Invalidate(true);
      }
      MouseTimeOutTimer = DateTime.Now;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected virtual void MouseClickEvent(MouseEventArgs e)
    {
      if (!ShowCursor)
      {
        Cursor.Show();
        ShowCursor = true;
        Invalidate(true);
      }
      MouseTimeOutTimer = DateTime.Now;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected virtual void MouseDoubleClickEvent(MouseEventArgs e)
    {
      if (!ShowCursor)
      {
        Cursor.Show();
        ShowCursor = true;
        Invalidate(true);
      }
      MouseTimeOutTimer = DateTime.Now;
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
      AppActive = true;
      base.OnGotFocus(e);
    }


    /// <summary>
    /// Raises the Resize event.
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.control.onresize.aspx
    /// </summary>
    /// <param name="e">An EventArgs that contains the event data.</param>
    protected override void OnResize(EventArgs e)
    {
      try
      {
        if (_fromTray)
        {
          _fromTray = false;
        }
        else
        {
          SavePlayerState();
        }
        if (_notifyIcon != null)
        {
          if (_notifyIcon.Visible == false && WindowState == FormWindowState.Minimized)
          {
            _notifyIcon.Visible = true;
            Hide();
            if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD)
            {
              if (g_Player.Volume > 0)
              {
                Volume = g_Player.Volume;
                g_Player.Volume = 0;
              }
              if (g_Player.Paused == false)
              {
                g_Player.Pause();
              }
            }
            return;
          }
          if (_notifyIcon.Visible && WindowState != FormWindowState.Minimized)
          {
            _notifyIcon.Visible = false;
          }
        }
        AppActive = WindowState != FormWindowState.Minimized;
        base.OnResize(e);
      }
      catch (Exception ex)
      {
        Log.Error("d3dapp: An error occured in OnResize - {0}", ex.ToString());
      }
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
        Log.Error("d3dapp: Exception: {0}", ex);
      }
    }


    /// <summary>
    /// Raises the Closing event.
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.form.onclosing.aspx
    /// </summary>
    /// <param name="e">A CancelEventArgs that contains the event data.</param>
    protected override void OnClosing(CancelEventArgs e)
    {
      if (MinimizeOnGuiExit && !ShuttingDown)
      {
        if (WindowState != FormWindowState.Minimized)
        {
          Log.Info("D3D: Minimizing to tray on GUI exit");
        }
        _isClosing = false;
        WindowState = FormWindowState.Minimized;
        Hide();
        e.Cancel = true;

        if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD)
        {
          if (g_Player.Volume > 0)
          {
            Volume = g_Player.Volume;
            g_Player.Volume = 0;
          }
          if (g_Player.Paused == false)
          {
            g_Player.Pause();
          }
        }
        return;
      }
      if (AutoHideTaskbar)
      {
        HideTaskBar(false);
      }
      _isClosing = true;
      base.OnClosing(e);
    }


    /// <summary>
    /// Performs the work of setting the specified bounds of this control.
    /// http://msdn.microsoft.com/en-us/library/system.windows.forms.control.setboundscore.aspx
    /// </summary>
    /// <param name="x">The new Left property value of the control.</param>
    /// <param name="y">The new Top property value of the control.</param>
    /// <param name="width">The new Width property value of the control.</param>
    /// <param name="height">The new Height property value of the control.</param>
    /// <param name="specified">A bitwise combination of the BoundsSpecified values.</param>
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
      var newBounds = new Rectangle(x, y, width, height);
      if (GUIGraphicsContext._useScreenSelector && !Windowed && !newBounds.Equals(GUIGraphicsContext.currentScreen.Bounds))
      {
        Log.Info("d3dapp: Screenselector: skipped SetBoundsCore {0} does not match {1}", newBounds.ToString(), GUIGraphicsContext.currentScreen.Bounds.ToString());
      }
      else
      {
        base.SetBoundsCore(x, y, width, height, specified);
      }
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

  #endregion
  
  #region Enums for D3D Applications

  /// <summary>
  /// Messages that can be used when displaying an error
  /// </summary>
  public enum ApplicationMessage
  {
    None,
    ApplicationMustExit,
    WarnSwitchToRef
  }

  #endregion

  #region SampleExceptions

  /// <summary>
  /// The default sample exception type
  /// </summary>
  public class SampleException : ApplicationException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        var strMsg = "Generic application error. Enable\n";
        strMsg    += "debug output for detailed information.";
        return strMsg;
      }
    }
  }


  /// <summary>
  /// Exception informing user no compatible devices were found
  /// </summary>
  public class NoCompatibleDevicesException : SampleException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        var strMsg = "This sample cannot run in a desktop\n";
        strMsg    += "window with the current display settings.\n";
        strMsg    += "Please change your desktop settings to a\n";
        strMsg    += "16- or 32-bit display mode and re-run this\n";
        strMsg    += "sample.";
        return strMsg;
      }
    }
  }


  /// <summary>
  /// An exception for when the ReferenceDevice is null
  /// </summary>
  public class NullReferenceDeviceException : SampleException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        var strMsg = "Warning: Nothing will be rendered.\n";
        strMsg    += "The reference rendering device was selected, but your\n";
        strMsg    += "computer only has a reduced-functionality reference device\n";
        strMsg    += "installed. Please check if your graphics card and\n";
        strMsg    += "drivers meet the minimum system requirements.\n";
        return strMsg;
      }
    }
  }


  /// <summary>
  /// An exception for when reset fails
  /// </summary>
  public class ResetFailedException : SampleException
  {
    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        const string strMsg = "Could not reset the Direct3D device.";
        return strMsg;
      }
    }
  }

  #endregion

  #region NativeMethods

  /// <summary>
  /// Holds native methods
  /// </summary>
  public class NativeMethods
  {
    #region Win32 Structures
    
    /// <summary>
    /// Monitor Info structure - http://msdn.microsoft.com/en-us/library/windows/desktop/dd145065(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInformation
    {
      public uint Size;                  // The size of the structure, in bytes.
      public Rectangle MonitorRectangle; // A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. 
      public Rectangle WorkRectangle;    // A RECT structure that specifies the work area rectangle of the display monitor, expressed in virtual-screen coordinates. 
      public uint Flags;                 // A set of flags that represent attributes of the display monitor.
    }

    #endregion

    #region Windows API calls

    // http://msdn.microsoft.com/en-us/library/ms648415(v=vs.85).aspx
    [SuppressUnmanagedCodeSecurity]
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern void DisableProcessWindowsGhosting();

    // http://msdn.microsoft.com/en-us/library/windows/desktop/dd144901(v=vs.85).aspx
    [SuppressUnmanagedCodeSecurity]
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hWnd, ref MonitorInformation info);

    #endregion

  }

  #endregion
}