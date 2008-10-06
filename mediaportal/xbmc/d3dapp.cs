#region Copyright (C) 2005-2008 Team MediaPortal

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
 */

#endregion

//#define PERFCOUNTER
//98472 85932
//enable to following line for profiling
//this will cause Mediaportal to draw the maxium number of FPS. It will use 100% cpu time 
//because it wont do any Sleeps(). This is usefull for the profile so it can see which methods/classes take up the most CPU %
//#define PROFILING 

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using MediaPortal.TV.Recording;
using MediaPortal.Video.Database;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using Caps = Microsoft.DirectX.Direct3D.Caps;
using CreateFlags = Microsoft.DirectX.Direct3D.CreateFlags;
using WPFMediaKit.DirectX;

namespace MediaPortal
{
  /// <summary>
  /// The base class for all the graphics (D3D) samples, it derives from windows forms
  /// </summary>
  public class D3DApp : UserInterface.Controls.MPForm
  {
    private const int MILLI_SECONDS_TIMER = 1;
    protected string m_strSkin = "BlueTwo";
    public static string _strSkinOverride = string.Empty;
    protected string m_strLanguage = "english";

    #region Menu Information

    // The menu items that *all* samples will need
    protected MainMenu menuStripMain;
    protected MenuItem menuItemFile;
    private MenuItem menuItemChangeDevice;
    private MenuItem menuBreakFile;
    protected MenuItem menuItemExit;

    #endregion

    protected bool _minimizeOnStartup = false;  // Minimize to tray on startup and on gui exit
    protected bool _minimizeOnGuiExit = false;
    protected bool _shuttingDown = false;
    protected bool _firstTimeWindowDisplayed = true;
    protected bool _autoHideMouse = false;
    protected DateTime _mouseTimeOutTimer = DateTime.Now;
    private Control ourRenderTarget;  // The window we will render too
    protected bool isUsingMenus = true; // Should we use the default windows
    private float lastTime = 0.0f; // The last time
    protected int frames = 0; // Number of frames since our last update
    protected int m_iVolume = -1;
    private long startFrame = 0;  // fps-limiting stuff
    private long endFrame = 0;
    protected D3DEnumeration enumerationSettings = new D3DEnumeration();  // We need to keep track of our enumeration settings
    protected D3DSettings graphicsSettings = new D3DSettings();
    protected bool isMaximized = false; // Are we maximized?
    private bool isHandlingSizeChanges = true; // Are we handling size changes?
    protected bool isClosing = false; // Are we closing?
    private bool isChangingFormStyle = false; // Are we changing the forms style?
    private bool isWindowActive = true; // Are we waiting for got focus?
    protected bool _showCursor = false;
    protected bool _lastShowCursor = true;
    private bool UseMillisecondTiming = true;
    private static int lastx = -1;
    private static int lasty = -1;
    protected bool windowed;  // Internal variables for the state of the app
    protected bool active;
    protected bool ready;
    protected bool hasFocus;
    protected bool isMultiThreaded = true;
    protected bool _fromTray = false;
    protected bool frameMoving; // Internal variables used for timing
    protected bool singleStep;
    // Main objects used for creating and rendering the 3D scene
    protected PresentParameters presentParams = new PresentParameters(); // Parameters for CreateDevice/Reset
    private Caps graphicsCaps; // Caps for the device
    internal static bool _fullscreenOverride = false;
    internal static bool _windowedOverride = false;
    internal static int _screenNumberOverride = -1;// 0 or higher means it is set

    protected Caps Caps
    {
      get { return graphicsCaps; }
    }

    private CreateFlags behavior; // Indicate sw or hw vertex processing

    protected BehaviorFlags BehaviorFlags
    {
      get { return new BehaviorFlags(behavior); }
    }

    protected Control RenderTarget
    {
      get { return ourRenderTarget; }
      set { ourRenderTarget = value; }
    }

    // Variables for timing
    protected float appTime; // Current time in seconds
    protected float elapsedTime; // Time elapsed since last frame
    protected float framePerSecond = 25; // Instanteous frame rate
    protected string deviceStats; // String to hold D3D device stats
    protected string frameStatsLine1; // 1st string to hold frame stats
    protected string frameStatsLine2; // 2nd string to hold frame stats
    protected bool m_bNeedReset = false;
    // Overridable variables for the app
    private int minDepthBits; // Minimum number of bits needed in depth buffer

    protected int MinDepthBits
    {
      get { return minDepthBits; }
      set
      {
        minDepthBits = value;
        enumerationSettings.AppMinDepthBits = value;
      }
    }

    private int minStencilBits; // Minimum number of bits needed in stencil buffer

    protected int MinStencilBits
    {
      get { return minStencilBits; }
      set
      {
        minStencilBits = value;
        enumerationSettings.AppMinStencilBits = value;
      }
    }

    protected bool showCursorWhenFullscreen; // Whether to show cursor when fullscreen
    protected bool clipCursorWhenFullscreen; // Whether to limit cursor pos when fullscreen
    protected bool startFullscreen = false; // Whether to start up the app in fullscreen mode
    private MenuItem menuItemOptions;
    private MenuItem menuItemConfiguration;
    private IContainer components;
    private MenuItem menuItemWizards;
    private MenuItem menuItemDVD;
    private MenuItem menuItemMovies;
    private MenuItem menuItemMusic;
    private MenuItem menuItemPictures;
    private MenuItem menuItemTelevision;
    private NotifyIcon notifyIcon;
    private ContextMenu contextMenu;
    private MenuItem menuItemContext;
    private MenuItem menuItem5;
    private MenuItem menuItemFullscreen;
    protected Rectangle oldBounds;
    protected PlayListPlayer playlistPlayer;

#if PERFCOUNTER
    PerformanceCounter _perfCounterCpu;
#endif


    // Overridable functions for the 3D scene created by the app
    protected virtual bool ConfirmDevice(Caps caps, VertexProcessingType vertexProcessingType,
                                         Format adapterFormat, Format backBufferFormat)
    {
      return true;
    }

    protected virtual void OneTimeSceneInitialization()
    {
      /* Do Nothing */
    }

    protected virtual void Initialize()
    {
      /* Do Nothing */
    }

    protected virtual void InitializeDeviceObjects()
    {
      /* Do Nothing */
    }

    protected virtual void OnDeviceLost(System.Object sender, System.EventArgs e)
    {
      /* Do Nothing */
    }

    protected virtual void OnDeviceReset(Object sender, EventArgs e)
    {
      /* Do Nothing */
    }

    protected virtual void FrameMove()
    {
      /* Do Nothing */
    }

    protected virtual void OnProcess()
    {
      /* Do Nothing */
    }

    protected virtual void Render(float timePassed)
    {
      /* Do Nothing */
    }

    //protected virtual void OnDeviceDisposing(System.Object sender, System.EventArgs e) { /* Do Nothing */ }

    protected virtual void OnStartup()
    { }

    protected virtual void OnExit()
    { }

    private bool _wasPlayingVideo = false;
    private int _iActiveWindow = -1;
    private double _currentPlayerPos = 0;
    private string _strCurrentFile;
    private PlayListType _currentPlayListType = PlayListType.PLAYLIST_NONE;
    private PlayList _currentPlayList = null;
    private bool _fullscreen = false;
    private bool _wasTV = false;
    private string _tvChannel = string.Empty;
    private bool _tvTimeshift = false;
    private int m_iSleepingTime = 50;
    protected bool autoHideTaskbar = true;
    private bool alwaysOnTop = false;
    protected bool useExclusiveDirectXMode;
    protected bool useEnhancedVideoRenderer;
    private bool _disableMouseEvents = false;

    [DllImport("winmm.dll")]
    internal static extern uint timeBeginPeriod(uint period);

    [DllImport("winmm.dll")]
    internal static extern uint timeEndPeriod(uint period);

    /// Constructor
    /// </summary>
    public D3DApp()
    {
#if PERFCOUNTER
      _perfCounterCpu = new PerformanceCounter();
      _perfCounterCpu.CategoryName = "Processor";
      _perfCounterCpu.CounterName = "% Processor Time";
      _perfCounterCpu.InstanceName = "_Total"; 
#endif
      //GUIGraphicsContext.DX9Device = null;
      try
      {
        int hr = (int)timeBeginPeriod(MILLI_SECONDS_TIMER);
        if (hr != 0)
        {
          UseMillisecondTiming = false;
        }
      }
      catch (Exception)
      {
        UseMillisecondTiming = false;
        Log.Info("Exception");
      }
      active = false;
      ready = false;
      hasFocus = false;
      behavior = 0;
      ourRenderTarget = this;
      frameMoving = true;
      singleStep = false;
      deviceStats = null;
      frameStatsLine1 = null;
      frameStatsLine2 = null;
      this.Text = "D3D9 Sample";
      this.ClientSize = new Size(720, 576);
      this.KeyPreview = true;

      minDepthBits = 16;
      minStencilBits = 0;
      showCursorWhenFullscreen = false;
      bool debugChangeDeviceHack = false;

      OsDetection.OSVersionInfo os = new OsDetection.OperatingSystemVersion();
      int ver = (os.OSMajorVersion * 10) + os.OSMinorVersion;
      if (ver >= 60)
      {
        Log.Debug("Disabling process window ghosting");
        NativeMethods.DisableProcessWindowsGhosting();
      }

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        useExclusiveDirectXMode = xmlreader.GetValueAsBool("general", "exclusivemode", true);
        useEnhancedVideoRenderer = xmlreader.GetValueAsBool("general", "useevr", false);
        if (useEnhancedVideoRenderer)
          useExclusiveDirectXMode = false;
        autoHideTaskbar = xmlreader.GetValueAsBool("general", "hidetaskbar", true);
        alwaysOnTop = xmlreader.GetValueAsBool("general", "alwaysontop", false);
        debugChangeDeviceHack = xmlreader.GetValueAsBool("debug", "changedevicehack", false);
        _disableMouseEvents = xmlreader.GetValueAsBool("remote", "CentareaJoystickMap", false);
      }

      // When clipCursorWhenFullscreen is TRUE, the cursor is limited to
      // the device window when the app goes fullscreen.  This prevents users
      // from accidentally clicking outside the app window on a multimon system.
      // This flag is turned off by default for debug builds, since it makes 
      // multimon debugging difficult.
#if debug
      clipCursorWhenFullscreen = false;
#else
      clipCursorWhenFullscreen = true;
#endif
      InitializeComponent();

      GUIGraphicsContext.IsVMR9Exclusive = useExclusiveDirectXMode;
      GUIGraphicsContext.IsEvr = useEnhancedVideoRenderer;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    protected void SetupCamera2D()
    {
      //Matrix matOrtho;
      //Matrix matIdentity = Matrix.Identity;

      ////Setup the orthogonal projection matrix and the default world/view matrix
      //matOrtho = Matrix.OrthoLH((float)GUIGraphicsContext.Width, (float)GUIGraphicsContext.Height, 0.0f, 1.0f);


      //GUIGraphicsContext.DX9Device.SetTransform(TransformType.Projection, matOrtho);
      //GUIGraphicsContext.DX9Device.SetTransform(TransformType.World, matIdentity);
      //GUIGraphicsContext.DX9Device.SetTransform(TransformType.View, matIdentity);

      //Make sure that the z-buffer and lighting are disabled
      //GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.ZEnable, false);
      //GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.Lighting, false);
    }


    /// <summary>
    /// Picks the best graphics device, and initializes it
    /// </summary>
    /// <returns>true if a good device was found, false otherwise</returns>
    public bool CreateGraphicsSample()
    {
      enumerationSettings.ConfirmDeviceCallback = new D3DEnumeration.ConfirmDeviceCallbackType(this.ConfirmDevice);
      enumerationSettings.Enumerate();

      if (ourRenderTarget.Cursor == null)
      {
        // Set up a default cursor
        ourRenderTarget.Cursor = Cursors.Default;
      }
      // if our render target is the main window and we haven't said 
      // ignore the menus, add our menu
      if ((ourRenderTarget == this) && (isUsingMenus))
      {
        this.Menu = menuStripMain;
      }

      try
      {
        ChooseInitialSettings();
        DXUtil.Timer(DirectXTimer.Start);

        // Initialize the application timer
        //@@@fullscreen
        Screen formOnScreen = Screen.FromRectangle(Bounds);
        if (!formOnScreen.Equals(GUIGraphicsContext.currentScreen))
        {
          Point location = this.Location;

          location.X = location.X - formOnScreen.Bounds.Left + GUIGraphicsContext.currentScreen.Bounds.Left;
          location.Y = location.Y - formOnScreen.Bounds.Top + GUIGraphicsContext.currentScreen.Bounds.Top;

          this.Location = location;
        }
        oldBounds = Bounds;

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          startFullscreen = _fullscreenOverride || xmlreader.GetValueAsBool("general", "startfullscreen", false);
          if (_windowedOverride)
            startFullscreen = false;

          if (startFullscreen)
          {
            if (autoHideTaskbar && !_minimizeOnStartup)
            {
              Win32API.EnableStartBar(false);
              Win32API.ShowStartBar(false);
            }

            Log.Info("D3D: Starting fullscreen");
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Menu = null;
            Rectangle newBounds = GUIGraphicsContext.currentScreen.Bounds;
            this.Bounds = newBounds;
            //GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth=Screen.PrimaryScreen.Bounds.Width;
            //GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight=Screen.PrimaryScreen.Bounds.Height;
            Log.Info("D3D: Client size: {0}x{1} - Screen: {2}x{3}",
                      this.ClientSize.Width, this.ClientSize.Height,
                      GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);

            //m_bNeedReset=true;
            //deviceLost=true;
            isMaximized = true;
          }
        }

        //@@@fullscreen


        // Initialize the 3D environment for the app
        InitializeEnvironment();
        // Initialize the app's custom scene stuff
        OneTimeSceneInitialization();
      }
      catch (SampleException d3de)
      {
        HandleSampleException(d3de, ApplicationMessage.ApplicationMustExit);
        return false;
      }
      catch
      {
        HandleSampleException(new SampleException(), ApplicationMessage.ApplicationMustExit);
        return false;
      }

      // The app is ready to go
      ready = true;


      return true;
    }

    /// <summary>
    /// Finds the adapter that has the specified screen on its primary monitor
    /// </summary>
    /// <returns>The adapter that has the specified screen on its primary monitor</returns>
    public GraphicsAdapterInfo FindAdapterForScreen(Screen screen)
    {
      foreach (GraphicsAdapterInfo adapterInfo in enumerationSettings.AdapterInfoList)
      {
        IntPtr hMon = Manager.GetAdapterMonitor(adapterInfo.AdapterOrdinal);

        NativeMethods.MonitorInformation info = new NativeMethods.MonitorInformation();
        info.Size = (uint)Marshal.SizeOf(info);
        NativeMethods.GetMonitorInfo(hMon, ref info);
        Rectangle rect = Screen.FromRectangle(info.MonitorRectangle).Bounds;

        if (rect.Equals(screen.Bounds))
          return adapterInfo;
      }
      return null;
    }

    /// <summary>
    /// Sets up graphicsSettings with best available windowed mode, subject to 
    /// the doesRequireHardware and doesRequireReference constraints.  
    /// </summary>
    /// <param name="doesRequireHardware">Does the device require hardware support</param>
    /// <param name="doesRequireReference">Does the device require the ref device</param>
    /// <returns>true if a mode is found, false otherwise</returns>
    public bool FindBestWindowedMode(bool doesRequireHardware, bool doesRequireReference)
    {
      // Get display mode of primary adapter (which is assumed to be where the window 
      // will appear)
      DisplayMode primaryDesktopDisplayMode = Manager.Adapters[0].CurrentDisplayMode;

      GraphicsAdapterInfo bestAdapterInfo = null;
      GraphicsDeviceInfo bestDeviceInfo = null;
      DeviceCombo bestDeviceCombo = null;
      foreach (GraphicsAdapterInfo adapterInfoIterate in enumerationSettings.AdapterInfoList)
      {
        GraphicsAdapterInfo adapterInfo = adapterInfoIterate;

        if (GUIGraphicsContext._useScreenSelector)
        {
          adapterInfo = FindAdapterForScreen(GUIGraphicsContext.currentScreen);
          primaryDesktopDisplayMode = Manager.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
          GUI.Library.GUIGraphicsContext.currentScreenNumber = adapterInfo.AdapterOrdinal;
        }
        foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfoList)
        {
          if (doesRequireHardware && deviceInfo.DevType != DeviceType.Hardware)
          {
            continue;
          }
          if (doesRequireReference && deviceInfo.DevType != DeviceType.Reference)
          {
            continue;
          }

          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
          {
            bool adapterMatchesBackBuffer = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);
            if (!deviceCombo.IsWindowed)
            {
              continue;
            }
            if (deviceCombo.AdapterFormat != primaryDesktopDisplayMode.Format)
            {
              continue;
            }

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
                // This windowed device combo looks great -- take it
                goto EndWindowedDeviceComboSearch;
              }
              // Otherwise keep looking for a better windowed device combo
            }
          }
        }
        if (GUIGraphicsContext._useScreenSelector)
          break;// no need to loop again.. result would be the same
      }

      EndWindowedDeviceComboSearch:
      if (bestDeviceCombo == null)
      {
        return false;
      }

      graphicsSettings.WindowedAdapterInfo = bestAdapterInfo;
      graphicsSettings.WindowedDeviceInfo = bestDeviceInfo;
      graphicsSettings.WindowedDeviceCombo = bestDeviceCombo;
      graphicsSettings.IsWindowed = true;
      graphicsSettings.WindowedDisplayMode = primaryDesktopDisplayMode;
      graphicsSettings.WindowedWidth = ourRenderTarget.ClientRectangle.Right - ourRenderTarget.ClientRectangle.Left;
      graphicsSettings.WindowedHeight = ourRenderTarget.ClientRectangle.Bottom - ourRenderTarget.ClientRectangle.Top;
      if (enumerationSettings.AppUsesDepthBuffer)
      {
        graphicsSettings.WindowedDepthStencilBufferFormat = (DepthFormat)bestDeviceCombo.DepthStencilFormatList[0];
      }
      int iQuality = 0; //bestDeviceCombo.MultiSampleTypeList.Count-1;
      graphicsSettings.WindowedMultisampleType = (MultiSampleType)bestDeviceCombo.MultiSampleTypeList[iQuality];
      graphicsSettings.WindowedMultisampleQuality = 0; //(int)bestDeviceCombo.MultiSampleQualityList[iQuality];

      graphicsSettings.WindowedVertexProcessingType = (VertexProcessingType)bestDeviceCombo.VertexProcessingTypeList[0];
      graphicsSettings.WindowedPresentInterval = (PresentInterval)bestDeviceCombo.PresentIntervalList[0];

      return true;
    }


    /// <summary>
    /// Sets up graphicsSettings with best available fullscreen mode, subject to 
    /// the doesRequireHardware and doesRequireReference constraints.  
    /// </summary>
    /// <param name="doesRequireHardware">Does the device require hardware support</param>
    /// <param name="doesRequireReference">Does the device require the ref device</param>
    /// <returns>true if a mode is found, false otherwise</returns>
    public bool FindBestFullscreenMode(bool doesRequireHardware, bool doesRequireReference)
    {
      // For fullscreen, default to first HAL DeviceCombo that supports the current desktop 
      // display mode, or any display mode if HAL is not compatible with the desktop mode, or 
      // non-HAL if no HAL is available
      DisplayMode adapterDesktopDisplayMode = new DisplayMode();
      DisplayMode bestAdapterDesktopDisplayMode = new DisplayMode();
      DisplayMode bestDisplayMode = new DisplayMode();
      bestAdapterDesktopDisplayMode.Width = 0;
      bestAdapterDesktopDisplayMode.Height = 0;
      bestAdapterDesktopDisplayMode.Format = 0;
      bestAdapterDesktopDisplayMode.RefreshRate = 0;

      GraphicsAdapterInfo bestAdapterInfo = null;
      GraphicsDeviceInfo bestDeviceInfo = null;
      DeviceCombo bestDeviceCombo = null;

      foreach (GraphicsAdapterInfo adapterInfoIterate in enumerationSettings.AdapterInfoList)
      {
        GraphicsAdapterInfo adapterInfo = adapterInfoIterate;

        if (GUIGraphicsContext._useScreenSelector)
        {
          adapterInfo = FindAdapterForScreen(GUI.Library.GUIGraphicsContext.currentScreen);
          GUI.Library.GUIGraphicsContext.currentFullscreenAdapterInfo = Manager.Adapters[adapterInfo.AdapterOrdinal];
          GUI.Library.GUIGraphicsContext.currentScreenNumber = adapterInfo.AdapterOrdinal;
        }

        adapterDesktopDisplayMode = Manager.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
        foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfoList)
        {
          if (doesRequireHardware && deviceInfo.DevType != DeviceType.Hardware)
          {
            continue;
          }
          if (doesRequireReference && deviceInfo.DevType != DeviceType.Reference)
          {
            continue;
          }

          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
          {
            bool adapterMatchesBackBuffer = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);
            bool adapterMatchesDesktop = (deviceCombo.AdapterFormat == adapterDesktopDisplayMode.Format);
            if (deviceCombo.IsWindowed)
            {
              continue;
            }

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
              {
                // This fullscreen device combo looks great -- take it
                goto EndFullscreenDeviceComboSearch;
              }
              // Otherwise keep looking for a better fullscreen device combo
            }
          }
        }
        if (GUIGraphicsContext._useScreenSelector)
          break;// no need to loop again.. result would be the same
      }

      EndFullscreenDeviceComboSearch:
      if (bestDeviceCombo == null)
      {
        return false;
      }

      // Need to find a display mode on the best adapter that uses pBestDeviceCombo->AdapterFormat
      // and is as close to bestAdapterDesktopDisplayMode's res as possible
      bestDisplayMode.Width = 0;
      bestDisplayMode.Height = 0;
      bestDisplayMode.Format = 0;
      bestDisplayMode.RefreshRate = 0;
      foreach (DisplayMode displayMode in bestAdapterInfo.DisplayModeList)
      {
        if (displayMode.Format != bestDeviceCombo.AdapterFormat)
        {
          continue;
        }
        if (displayMode.Width == bestAdapterDesktopDisplayMode.Width &&
            displayMode.Height == bestAdapterDesktopDisplayMode.Height &&
            displayMode.RefreshRate == bestAdapterDesktopDisplayMode.RefreshRate)
        {
          // found a perfect match, so stop
          bestDisplayMode = displayMode;
          break;
        }
        else if (displayMode.Width == bestAdapterDesktopDisplayMode.Width &&
                 displayMode.Height == bestAdapterDesktopDisplayMode.Height &&
                 displayMode.RefreshRate > bestDisplayMode.RefreshRate)
        {
          // refresh rate doesn't match, but width/height match, so keep this
          // and keep looking
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

      graphicsSettings.FullscreenAdapterInfo = bestAdapterInfo;
      graphicsSettings.FullscreenDeviceInfo = bestDeviceInfo;
      graphicsSettings.FullscreenDeviceCombo = bestDeviceCombo;
      graphicsSettings.IsWindowed = false;
      graphicsSettings.FullscreenDisplayMode = bestDisplayMode;
      if (enumerationSettings.AppUsesDepthBuffer)
      {
        graphicsSettings.FullscreenDepthStencilBufferFormat = (DepthFormat)bestDeviceCombo.DepthStencilFormatList[0];
      }
      graphicsSettings.FullscreenMultisampleType = (MultiSampleType)bestDeviceCombo.MultiSampleTypeList[0];
      graphicsSettings.FullscreenMultisampleQuality = 0;
      graphicsSettings.FullscreenVertexProcessingType =
        (VertexProcessingType)bestDeviceCombo.VertexProcessingTypeList[0];
      graphicsSettings.FullscreenPresentInterval = PresentInterval.Default;

      return true;
    }


    /// <summary>
    /// Choose the initial settings for the application
    /// </summary>
    /// <returns>true if the settings were initialized</returns>
    public bool ChooseInitialSettings()
    {
      bool foundFullscreenMode = FindBestFullscreenMode(false, false);
      bool foundWindowedMode = FindBestWindowedMode(false, false);
      /*if (/ *startFullscreen* /false && foundFullscreenMode)
      { PIBA startFullscreen was not used and now when set to true would couse flickering visualizations whem playing music
        graphicsSettings.IsWindowed = false;
      }*/

      if (!foundFullscreenMode && !foundWindowedMode)
      {
        throw new NoCompatibleDevicesException();
      }

      return (foundFullscreenMode || foundWindowedMode);
    }


    /// <summary>
    /// Build presentation parameters from the current settings
    /// windowed = true,  normal mode (must use desktop refresh rate)
    /// windowed = false, exclusive mode (can set screen refresh rate)
    /// </summary>
    public void BuildPresentParamsFromSettings(bool bwindowed)
    {
      presentParams.BackBufferCount = 2;
      presentParams.EnableAutoDepthStencil = false;
      presentParams.ForceNoMultiThreadedFlag = false;

      if (bwindowed)
      {
        presentParams.MultiSample = graphicsSettings.WindowedMultisampleType;
        presentParams.MultiSampleQuality = graphicsSettings.WindowedMultisampleQuality;
        presentParams.AutoDepthStencilFormat = graphicsSettings.WindowedDepthStencilBufferFormat;

        presentParams.BackBufferWidth = ourRenderTarget.ClientRectangle.Right - ourRenderTarget.ClientRectangle.Left;
        presentParams.BackBufferHeight = ourRenderTarget.ClientRectangle.Bottom - ourRenderTarget.ClientRectangle.Top;
        presentParams.BackBufferFormat = graphicsSettings.BackBufferFormat;
        presentParams.PresentationInterval = PresentInterval.Default;
        presentParams.FullScreenRefreshRateInHz = 0;
        presentParams.SwapEffect = SwapEffect.Discard;
        presentParams.PresentFlag = PresentFlag.Video; //PresentFlag.LockableBackBuffer;
        presentParams.DeviceWindow = ourRenderTarget;
        presentParams.Windowed = true;
      }
      else
      {                
        graphicsSettings.DisplayMode = Manager.Adapters[GUIGraphicsContext.currentScreenNumber].CurrentDisplayMode;
        
        presentParams.MultiSample = graphicsSettings.FullscreenMultisampleType;
        presentParams.MultiSampleQuality = graphicsSettings.FullscreenMultisampleQuality;
        presentParams.AutoDepthStencilFormat = graphicsSettings.FullscreenDepthStencilBufferFormat;

        presentParams.BackBufferWidth = graphicsSettings.DisplayMode.Width;
        presentParams.BackBufferHeight = graphicsSettings.DisplayMode.Height;
        presentParams.BackBufferFormat = graphicsSettings.DeviceCombo.BackBufferFormat;
        presentParams.PresentationInterval = PresentInterval.Default;
        presentParams.FullScreenRefreshRateInHz = graphicsSettings.DisplayMode.RefreshRate;
        presentParams.SwapEffect = SwapEffect.Discard;
        presentParams.PresentFlag = PresentFlag.Video; //|PresentFlag.LockableBackBuffer;
        presentParams.DeviceWindow = this;
        presentParams.Windowed = false;
        Log.Info("D3D: BuildPresentParamsFromSettings using {0}Hz as RefreshRate", graphicsSettings.DisplayMode.RefreshRate);
      }
      GUIGraphicsContext.DirectXPresentParameters = presentParams;
      windowed = bwindowed;
    }

    /// <summary>
    /// Switch between exclusive mode and windowed depending on parameter
    /// </summary>
    /// 
    public void SwitchFullScreenOrWindowed(bool bWindowed)
    {
      if ((!useExclusiveDirectXMode || useEnhancedVideoRenderer) && !GUIGraphicsContext.IsDirectX9ExUsed())
      {
        return;
      }

      // Temporary remove the handler
      GUIGraphicsContext.DX9Device.DeviceLost -= new EventHandler(this.OnDeviceLost);
      GUIGraphicsContext.DX9Device.DeviceReset -= new EventHandler(this.OnDeviceReset);

      if (bWindowed)
      {
        Log.Debug("D3D: Switch to windowed mode - Playing media: {0}", g_Player.Playing);
      }
      else
      {
        Log.Debug("D3D: Switch to exclusive mode - Playing media: {0}", g_Player.Playing);
      }

      if (GUIGraphicsContext.IsDirectX9ExUsed() && (useEnhancedVideoRenderer || !useExclusiveDirectXMode))
      {
        BuildPresentParamsFromSettings(true);
      }
      else 
      {
        BuildPresentParamsFromSettings(bWindowed);
      }
      try
      {
        GUIGraphicsContext.DX9Device.Reset(presentParams);
        if (GUIGraphicsContext.IsDirectX9ExUsed() && !useEnhancedVideoRenderer && useExclusiveDirectXMode)
        {
          GUIFontManager.LoadFonts(Config.GetFile(Config.Dir.Skin, m_strSkin, "fonts.xml"));
          GUIFontManager.InitializeDeviceObjects();
        }

        if (windowed)
        {
          Log.Debug("D3D: Switched to windowed mode successfully");
        }
        else
        {
          Log.Debug("D3D: Switched to exclusive mode successfully");
        }
      }
      catch (Exception ex)
      {
        if (windowed)
        {
          Log.Warn("D3D: Switch to windowed mode failed - {0}", ex.ToString());
        }
        else
        {
          Log.Warn("D3D: Switch to exclusive mode failed - {0}", ex.ToString());
        }

        if (GUIGraphicsContext.IsDirectX9ExUsed() && (useEnhancedVideoRenderer || !useExclusiveDirectXMode))
        {
          BuildPresentParamsFromSettings(true);
        }
        else
        {
          BuildPresentParamsFromSettings(!bWindowed);
        }
        try
        {
          GUIGraphicsContext.DX9Device.Reset(presentParams);
          if (GUIGraphicsContext.IsDirectX9ExUsed() && !useEnhancedVideoRenderer && useExclusiveDirectXMode)
          {
            GUIFontManager.LoadFonts(Config.GetFile(Config.Dir.Skin, m_strSkin, "fonts.xml"));
            GUIFontManager.InitializeDeviceObjects();
          }
        }
        catch (Exception e)
        {
          Log.Warn("D3D: mode failed - {0}", e.ToString());
        }
      }
      GUIGraphicsContext.DX9Device.DeviceReset += new EventHandler(this.OnDeviceReset);
      GUIGraphicsContext.DX9Device.DeviceLost += new EventHandler(this.OnDeviceLost);

      if (windowed)
      {
        TopMost = alwaysOnTop;
      }
      this.Activate();
    }


    /// <summary>
    /// Initialize the graphics environment
    /// </summary>
    public void InitializeEnvironment()
    {
      GraphicsAdapterInfo adapterInfo = graphicsSettings.AdapterInfo;
      GraphicsDeviceInfo deviceInfo = graphicsSettings.DeviceInfo;

      // Set up the presentation parameters, we start in none exclusive mode
      BuildPresentParamsFromSettings(true);

      if (deviceInfo.Caps.PrimitiveMiscCaps.IsNullReference)
        // Warn user about null ref device that can't render anything
        HandleSampleException(new NullReferenceDeviceException(), ApplicationMessage.None);

      CreateFlags createFlags = new CreateFlags();
      if (graphicsSettings.VertexProcessingType == VertexProcessingType.Software)
        createFlags = CreateFlags.SoftwareVertexProcessing;
      else if (graphicsSettings.VertexProcessingType == VertexProcessingType.Mixed)
        createFlags = CreateFlags.MixedVertexProcessing;
      else if (graphicsSettings.VertexProcessingType == VertexProcessingType.Hardware)
        createFlags = CreateFlags.HardwareVertexProcessing;
      else if (graphicsSettings.VertexProcessingType == VertexProcessingType.PureHardware)
        createFlags = CreateFlags.HardwareVertexProcessing; // | CreateFlags.PureDevice;
      else
        throw new ApplicationException();

      // Make sure to allow multithreaded apps if we need them
      presentParams.ForceNoMultiThreadedFlag = !isMultiThreaded;

      try
      {
        // Create the device
        if (GUIGraphicsContext.IsDirectX9ExUsed())
        {
          // Vista or later, use DirectX9 Ex device
          Log.Info("Creating DirectX9 Ex device");
          CreateDirectX9ExDevice(createFlags);
        }
        else
        {
          Log.Info("Creating DirectX9 device");
          GUIGraphicsContext.DX9Device = new Microsoft.DirectX.Direct3D.Device(graphicsSettings.AdapterOrdinal,
                                                                               graphicsSettings.DevType,
                                                                               windowed ? ourRenderTarget : this,
                                                                               createFlags | CreateFlags.MultiThreaded,
                                                                               presentParams);
        }
        
        // Cache our local objects
        //renderState = GUIGraphicsContext.DX9Device.RenderState;
        //sampleState = GUIGraphicsContext.DX9Device.SamplerState;
        //textureStates = GUIGraphicsContext.DX9Device.TextureState;
        // When moving from fullscreen to windowed mode, it is important to
        // adjust the window size after recreating the device rather than
        // beforehand to ensure that you get the window size you want.  For
        // example, when switching from 640x480 fullscreen to windowed with
        // a 1000x600 window on a 1024x768 desktop, it is impossible to set
        // the window size to 1000x600 until after the display mode has
        // changed to 1024x768, because windows cannot be larger than the
        // desktop.
        if (windowed)
        {
          // Make sure main window isn't topmost, so error message is visible
          Size currentClientSize = this.ClientSize;

          this.Size = this.ClientSize;
          this.SendToBack();
          this.BringToFront();
          this.ClientSize = currentClientSize;
          this.TopMost = alwaysOnTop;
        }

        // Store device Caps
        graphicsCaps = GUIGraphicsContext.DX9Device.DeviceCaps;
        behavior = createFlags;

        StringBuilder sb = new StringBuilder();

        // Store device description
        if (deviceInfo.DevType == DeviceType.Reference)
          sb.Append("REF");
        else if (deviceInfo.DevType == DeviceType.Hardware)
          sb.Append("HAL");
        else if (deviceInfo.DevType == DeviceType.Software)
          sb.Append("SW");

        BehaviorFlags behaviorFlags = new BehaviorFlags(createFlags);
        if ((behaviorFlags.HardwareVertexProcessing) &&
            (behaviorFlags.PureDevice))
          if (deviceInfo.DevType == DeviceType.Hardware)
            sb.Append(" (pure hw vp)");
          else
            sb.Append(" (simulated pure hw vp)");
        else if (behaviorFlags.HardwareVertexProcessing)
          if (deviceInfo.DevType == DeviceType.Hardware)
            sb.Append(" (hw vp)");
          else
            sb.Append(" (simulated hw vp)");
        else if (behaviorFlags.MixedVertexProcessing)
          if (deviceInfo.DevType == DeviceType.Hardware)
            sb.Append(" (mixed vp)");
          else
            sb.Append(" (simulated mixed vp)");
        else if (behaviorFlags.SoftwareVertexProcessing)
          sb.Append(" (sw vp)");

        if (deviceInfo.DevType == DeviceType.Hardware)
        {
          sb.Append(": ");
          sb.Append(adapterInfo.AdapterDetails.Description);
        }

        // Set device stats string
        deviceStats = sb.ToString();

        // Set up the fullscreen cursor
        if (showCursorWhenFullscreen && !windowed)
        {
          Cursor ourCursor = this.Cursor;
          GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
          GUIGraphicsContext.DX9Device.ShowCursor(true);
        }

        // Confine cursor to fullscreen window
        if (clipCursorWhenFullscreen && !windowed)
        {
          Rectangle rcWindow = this.ClientRectangle;
        }

        // Setup the event handlers for our device
        GUIGraphicsContext.DX9Device.DeviceLost += new System.EventHandler(this.OnDeviceLost);
        GUIGraphicsContext.DX9Device.DeviceReset += new EventHandler(this.OnDeviceReset);
        //GUIGraphicsContext.DX9Device.Disposing += new System.EventHandler(this.OnDeviceDisposing);
        //GUIGraphicsContext.DX9Device.DeviceResizing += new System.ComponentModel.CancelEventHandler(this.EnvironmentResized);

        // Initialize the app's device-dependent objects
        try
        {
          InitializeDeviceObjects();
          //OnDeviceReset(null, null);
          active = true;
        }
        catch (Exception ex)
        {
          Log.Error("D3D: InitializeDeviceObjects - Exception: {0}", ex.ToString());
          // Cleanup before we try again
          //OnDeviceLost(null, null);
          //OnDeviceDisposing(null, null);
          GUIGraphicsContext.DX9Device.Dispose();
          GUIGraphicsContext.DX9Device = null;
          if (this.Disposing)
            return;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        // If that failed, fall back to the reference rasterizer
        if (deviceInfo.DevType == DeviceType.Hardware)
          if (FindBestWindowedMode(false, true))
          {
            windowed = true;

            // Make sure main window isn't topmost, so error message is visible
            Size currentClientSize = this.ClientSize;
            this.Size = this.ClientSize;
            this.SendToBack();
            this.BringToFront();
            this.ClientSize = currentClientSize;
            this.TopMost = alwaysOnTop;

            // Let the user know we are switching from HAL to the reference rasterizer
            HandleSampleException(null, ApplicationMessage.WarnSwitchToRef);

            InitializeEnvironment();
          }
      }
    }

    /// <summary>
    /// Creates DirectX9 Ex device in unmanaged code as MDX is not supporting DX9 Ex 
    /// This device type is available only in Vista or later OS
    /// </summary>
    private void CreateDirectX9ExDevice(CreateFlags createFlags)  
    {
      D3DPRESENT_PARAMETERS param = new D3DPRESENT_PARAMETERS();
      param.Windowed = 0;
      if (presentParams.Windowed)
      {
        param.Windowed = 1;
      }

      param.AutoDepthStencilFormat = presentParams.AutoDepthStencilFormat;
      param.BackBufferCount = (uint)presentParams.BackBufferCount;
      param.BackBufferFormat = presentParams.BackBufferFormat;
      param.BackBufferHeight = (uint)presentParams.BackBufferHeight;
      param.BackBufferWidth = (uint)presentParams.BackBufferWidth;
      param.hDeviceWindow = presentParams.DeviceWindow.Handle;

      param.EnableAutoDepthStencil = 0;
      if (presentParams.EnableAutoDepthStencil)
      {
        param.EnableAutoDepthStencil = 1;
      }

      param.FullScreen_RefreshRateInHz = (uint)presentParams.FullScreenRefreshRateInHz;
      param.MultiSampleType = presentParams.MultiSample;
      param.MultiSampleQuality = presentParams.MultiSampleQuality;
      param.PresentationInterval = (uint)presentParams.PresentationInterval;
      param.SwapEffect = presentParams.SwapEffect;

      IDirect3D9Ex m_d3dEx;
      Direct3D.Direct3DCreate9Ex(32, out m_d3dEx);
      IntPtr d3dEx = Marshal.GetIUnknownForObject(m_d3dEx);
      Marshal.Release(d3dEx);
   
      D3DDISPLAYMODEEX displaymodeEx = new D3DDISPLAYMODEEX();

      displaymodeEx.Size = (uint)Marshal.SizeOf(displaymodeEx);
      displaymodeEx.Width = param.BackBufferWidth;
      displaymodeEx.Height = param.BackBufferHeight;
      displaymodeEx.Format = param.BackBufferFormat;
      displaymodeEx.ScanLineOrdering = WPFMediaKit.DirectX.D3DSCANLINEORDERING.D3DSCANLINEORDERING_UNKNOWN;
      IntPtr dev;
      IntPtr prt = Marshal.AllocHGlobal(Marshal.SizeOf(displaymodeEx));
      Marshal.StructureToPtr(displaymodeEx, prt, true);

      int hr = m_d3dEx.CreateDeviceEx(graphicsSettings.AdapterOrdinal, graphicsSettings.DevType, 
        windowed ? ourRenderTarget.Handle : this.Handle, createFlags | CreateFlags.MultiThreaded, ref param,
        windowed ? IntPtr.Zero : prt, out dev);
      GUIGraphicsContext.DX9Device = new Device(dev);
      
      // Reset must be done to get the MDX device internal state up to date
      GUIGraphicsContext.DX9Device.Reset(presentParams);
    }

    /// <summary>
    /// Displays sample exceptions to the user
    /// </summary>
    /// <param name="e">The exception that was thrown</param>
    /// <param name="Type">Extra information on how to handle the exception</param>
    public void HandleSampleException(SampleException e, ApplicationMessage Type)
    {
      try
      {
        if (UseMillisecondTiming)
          timeEndPeriod(MILLI_SECONDS_TIMER);
      }
      catch (Exception)
      { }

      UseMillisecondTiming = false;
      // Build a message to display to the user
      string strMsg = "";
      string strSource = "";
      string strStack = "";
      if (e != null)
      {
        strMsg = e.Message;
        strSource = e.Source;
        strStack = e.StackTrace;
      }
      Log.Error("D3D: Exception: {0} {1} {2}", strMsg, strSource, strStack);
      if (ApplicationMessage.ApplicationMustExit == Type)
      {
        strMsg += "\n\nMediaPortal has to be closed.";
        MessageBox.Show(strMsg, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

        // Close the window, which shuts down the app
        if (this.IsHandleCreated)
          this.Close();
      }
      else
      {
        if (ApplicationMessage.WarnSwitchToRef == Type)
          strMsg = "\n\nSwitching to the reference rasterizer,\n";

        strMsg += "a software device that implements the entire\n";
        strMsg += "Direct3D feature set, but runs very slowly.";
        MessageBox.Show(strMsg, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }


    /// <summary>
    /// Fired when our environment was resized
    /// </summary>
    /// <param name="sender">the device that's resizing our environment</param>
    /// <param name="e">Set the cancel member to true to turn off automatic device reset</param>
    public void EnvironmentResized(object sender, CancelEventArgs e)
    {
      // Check to see if we're closing or changing the form style
      if ((isClosing) || (isChangingFormStyle))
      {
        // We are, cancel our reset, and exit
        e.Cancel = true;
        return;
      }

      // Check to see if we're minimizing and our rendering object
      // is not the form, if so, cancel the resize
      if ((ourRenderTarget != this) && (this.WindowState == FormWindowState.Minimized))
        e.Cancel = true;

      if (!isWindowActive)
        e.Cancel = true;

      // Set up the fullscreen cursor
      if (showCursorWhenFullscreen && !windowed)
      {
        Cursor ourCursor = this.Cursor;
        GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
        GUIGraphicsContext.DX9Device.ShowCursor(true);
      }
    }


    /// <summary>
    /// Save player state (when form was resized)
    /// </summary>
    protected void SavePlayerState()
    {
      // Is App not minimized to tray and is a player active?
      if (WindowState != FormWindowState.Minimized &&
        !_wasPlayingVideo &&
        (g_Player.Playing && (g_Player.IsTV || g_Player.IsVideo || g_Player.IsDVD)))
      {
        _wasPlayingVideo = true;
        _wasTV = Recorder.IsViewing();
        _fullscreen = g_Player.FullScreen;

        if (_wasTV)
        {
          // TV is active
          _tvChannel = Recorder.TVChannelName;
          _tvTimeshift = Recorder.IsTimeShifting();
          Log.Info("D3D: Form resized - Stopping TV - Current channel: {0} / Timeshifting: {1}", _tvChannel, _tvTimeshift);
          Recorder.StopViewing();
        }
        else
        {
          // Some Audio/video is playing
          _currentPlayerPos = g_Player.CurrentPosition;
          _currentPlayListType = playlistPlayer.CurrentPlaylistType;
          _currentPlayList = new PlayList();

          Log.Info("D3D: Saving fullscreen state for resume: {0}", _fullscreen);
          PlayList tempList = playlistPlayer.GetPlaylist(_currentPlayListType);
          if (tempList.Count == 0 && g_Player.IsDVD == true)
          {
            // DVD is playing
            PlayListItem itemDVD = new PlayListItem();
            itemDVD.FileName = g_Player.CurrentFile;
            itemDVD.Played = true;
            itemDVD.Type = PlayListItem.PlayListItemType.DVD;
            tempList.Add(itemDVD);
          }
          if (tempList != null)
          {
            for (int i = 0; i < (int)tempList.Count; ++i)
            {
              PlayListItem itemNew = tempList[i];
              _currentPlayList.Add(itemNew);
            }
          }
          _strCurrentFile = playlistPlayer.Get(playlistPlayer.CurrentSong);
          if (_strCurrentFile.Equals(string.Empty) && g_Player.IsDVD == true)
            _strCurrentFile = g_Player.CurrentFile;
          Log.Info("D3D: Form resized - Stopping media - Current playlist: Type: {0} / Size: {1} / Current item: {2} / Filename: {3} / Position: {4}", _currentPlayListType, _currentPlayList.Count, playlistPlayer.CurrentSong, _strCurrentFile, _currentPlayerPos);
          g_Player.Stop();
        }
        _iActiveWindow = GUIWindowManager.ActiveWindow;
      }
    }

    /// <summary>
    /// Restore player from saved state (after resizing form)
    /// </summary>
    protected void ResumePlayer()
    {
      if (_wasPlayingVideo) // was any player active at all?
      {
        _wasPlayingVideo = false;

        if (_wasTV)
        {
          // we were watching TV
          Log.Info("D3D: RestorePlayers - Resuming: {0}", _tvChannel);
          string _errorMessage = string.Empty;
          bool success = Recorder.StartViewing(_tvChannel, true, _tvTimeshift, true, out _errorMessage);
          if (success)
            if ((_iActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN) ||
              (_iActiveWindow == (int)GUIWindow.Window.WINDOW_TV))
            {
              GUIWindowManager.ActivateWindow(_iActiveWindow);
              Log.Info("D3D: Resumed TV successfully");
            }
            else
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
              Thread tvWaitThread = new Thread(TvWaitThread);
              tvWaitThread.IsBackground = true;
              tvWaitThread.Name = "TvWaitThread";
              tvWaitThread.Start();
            }
          else
            Log.Info("D3D: Error resuming TV: {0}", _errorMessage);
        }
        else
        {
          // we were watching some audio/video
          Log.Info("D3D: RestorePlayers - Resuming: {0}", _strCurrentFile);
          playlistPlayer.Init();
          playlistPlayer.Reset();
          playlistPlayer.CurrentPlaylistType = _currentPlayListType;
          PlayList playlist = playlistPlayer.GetPlaylist(_currentPlayListType);
          playlist.Clear();
          if (_currentPlayList != null)
            for (int i = 0; i < (int)_currentPlayList.Count; ++i)
            {
              PlayListItem itemNew = _currentPlayList[i];
              playlist.Add(itemNew);
            }
          if (playlist.Count > 0 && playlist[0].Type.Equals(PlayListItem.PlayListItemType.DVD))
          {
            // we were watching DVD
            IMDBMovie movieDetails = new IMDBMovie();
            string fileName = playlist[0].FileName;
            VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
            int idFile = VideoDatabase.GetFileId(fileName);
            int idMovie = VideoDatabase.GetMovieId(fileName);
            int timeMovieStopped = 0;
            byte[] resumeData = null;
            if ((idMovie >= 0) && (idFile >= 0))
            {
              timeMovieStopped = VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData);
              g_Player.PlayDVD();
              if (g_Player.Playing)
                g_Player.Player.SetResumeState(resumeData);
            }
          }
          else
            playlistPlayer.Play(_strCurrentFile); // some standard audio/video

          if (g_Player.Playing)
            g_Player.SeekAbsolute(_currentPlayerPos);

          GUIGraphicsContext.IsFullScreenVideo = _fullscreen;
          GUIWindowManager.ReplaceWindow(_iActiveWindow);
        }
      }
    }

    /// <summary>
    /// Called when our sample has nothing else to do, and it's time to render
    /// </summary>
    protected void FullRender()
    {
      if (this.WindowState == FormWindowState.Minimized)
      {
        Thread.Sleep(100);
        return;
      }

      ResumePlayer();

      if (GUIGraphicsContext.Vmr9Active)
      {
        HandleCursor();

        if ((ActiveForm != this) && (alwaysOnTop))
          this.Activate();

        return;
      }
      // Render a frame during idle time (no messages are waiting)
      if (active && ready)
      {
#if DEBUG
#else
        try
        {
#endif
        if ((GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST) || (ActiveForm != this))
        {
          // Yield some CPU time to other processes
#if !PROFILING
          Thread.Sleep(100); // 100 milliseconds
#endif
        }
        Render3DEnvironment();
#if DEBUG
#else
        }
        catch (Exception ee)
        {
          Log.Info("D3D: Exception {0}", ee);
          System.Windows.Forms.MessageBox.Show("An exception has occurred.  MediaPortal has to be closed.\r\n\r\n" + ee.ToString(), "Exception",
            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
          this.Close();
        }
#endif
      }
      else
      {
#if !PROFILING
        // if we dont got the focus, then dont use all the CPU
        if (ActiveForm != this)
          Thread.Sleep(100);
#endif
      }

      HandleCursor();

      if ((ActiveForm != this) && (alwaysOnTop))
        Activate();
    }


    private void DoSleep(int sleepTime)
    {
      if (sleepTime <= 0)
        sleepTime = 5;
#if !PROFILING
      Thread.Sleep(sleepTime);
#endif
    }

    public void RecoverDevice()
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
      {
        if (g_Player.Playing || Recorder.IsViewing())
        {
          g_Player.Stop();
        }

        //Debugger.Launch();
        try
        {
          Log.Debug("d3dapp: RecoverDevice called");
          // Test the cooperative level to see if it's okay to render
          GUIGraphicsContext.DX9Device.TestCooperativeLevel();
        }
        catch (DeviceLostException)
        {
          // If the device was lost, do not render until we get it back
          isHandlingSizeChanges = false;
          isWindowActive = false;
          Log.Debug("d3dapp: DeviceLostException");

          return;          
        }
        catch (DeviceNotResetException)
        {
          Log.Debug("d3dapp: DeviceNotResetException");
          m_bNeedReset = true;
        }
        if (m_bNeedReset)
        {          
          // Check if the device needs to be resized.

          // If we are windowed, read the desktop mode and use the same format for
          // the back buffer
          //Log.Info("app.TestCooperativeLevel()->app.DeviceNotResetException");
          //if (windowed)
          //{
          //  Log.Debug("d3dapp: Windowed while need reset");
          //  GraphicsAdapterInfo adapterInfo = graphicsSettings.AdapterInfo;
          //  graphicsSettings.WindowedDisplayMode = Manager.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
          //  presentParams.BackBufferFormat = graphicsSettings.WindowedDisplayMode.Format;
          //}

          BuildPresentParamsFromSettings(windowed);

          // Reset the device and resize it
          Log.Warn("d3dapp: Resetting DX9 device");
          try
          {
            // For Bav: reset fails with invalid call if we do not clean up all resources
            GUITextureManager.CleanupThumbs();
            GUITextureManager.Dispose();
            GUIFontManager.Dispose();

            // GUIGraphicsContext.DX9Device.EvictManagedResources();

            GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);
            m_bNeedReset = false;
          }
          catch (Exception ex)
          {
            Log.Error("d3dapp: Reset failed - {0}/{1}", ex.Message);
            GUIGraphicsContext.DX9Device.DeviceLost -= new System.EventHandler(this.OnDeviceLost);
            GUIGraphicsContext.DX9Device.DeviceReset -= new EventHandler(this.OnDeviceReset);
            InitializeEnvironment();
            return;
          }

          Log.Debug("d3dapp: EnvironmentResized()");
          EnvironmentResized(GUIGraphicsContext.DX9Device, new CancelEventArgs());
          //InitializeDeviceObjects();
        }
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
      }
    }

    /// <summary>
    /// Draws the scene 
    /// </summary>
    public void Render3DEnvironment()
    {
      RecoverDevice();

      try
      {
        //if (!GUIGraphicsContext.Vmr9Active)
        if (!GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
          Render(GUIGraphicsContext.TimePassed);
      }
      catch (Exception ex)
      {
        Log.Error("d3dapp: Exception: {0}", ex);
      }
    }

    private void TvWaitThread()
    {
      while (!g_Player.Playing)
        Thread.Sleep(100);

      Log.Info("D3D: Resumed TV successfully");
      GUIGraphicsContext.IsFullScreenVideo = _fullscreen;
      GUIWindowManager.ReplaceWindow(_iActiveWindow);
    }

    public void HandleCursor()
    {
      if (!isMaximized)
        return;

      if (_autoHideMouse)
      {
        if (_showCursor != _lastShowCursor)
        {
          if (!_showCursor)
            Cursor.Hide();
          else
            Cursor.Show();
          _lastShowCursor = _showCursor;
        }

        if (_showCursor)
        {
          TimeSpan ts = DateTime.Now - _mouseTimeOutTimer;
          if (ts.TotalSeconds >= 3)
          {
            //hide mouse
            Cursor.Hide();
            _showCursor = false;
            Invalidate(true);
          }
        }
      }
    }

    private bool ShouldUseSleepingTime()
    {
      // Render the scene as normal
      if (GUIGraphicsContext.Vmr9Active)
        if (GUIGraphicsContext.Vmr9FPS > 5f)
          // if we're playing a movie with vmr9 then the player will draw the GUI
          // so we just sleep 50msec here ...
          return false;

      //if we're playing a movie or watching tv (fullscreen)
      if (GUIGraphicsContext.IsFullScreenVideo)
        return false;
      return true;
    }

    private int GetSleepingTime()
    {
      if (!ShouldUseSleepingTime())
        return 100;

      return m_iSleepingTime;
    }


    /// <summary>
    ///Get the  statistics 
    /// </summary>
    public void GetStats()
    {
      string strFmt;
      Format fmtAdapter = graphicsSettings.DisplayMode.Format;
      strFmt = String.Format("backbuf {0}, adapter {1}",
                             GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferFormat.ToString(),
                             fmtAdapter.ToString());

      string strDepthFmt;
      if (enumerationSettings.AppUsesDepthBuffer)
        strDepthFmt = String.Format(" ({0})", graphicsSettings.DepthStencilBufferFormat.ToString());
      else
        // No depth buffer
        strDepthFmt = "";

      string strMultiSample;
      switch (graphicsSettings.MultisampleType)
      {
        case MultiSampleType.NonMaskable:
          strMultiSample = " (NonMaskable Multisample)";
          break;
        case MultiSampleType.TwoSamples:
          strMultiSample = " (2x Multisample)";
          break;
        case MultiSampleType.ThreeSamples:
          strMultiSample = " (3x Multisample)";
          break;
        case MultiSampleType.FourSamples:
          strMultiSample = " (4x Multisample)";
          break;
        case MultiSampleType.FiveSamples:
          strMultiSample = " (5x Multisample)";
          break;
        case MultiSampleType.SixSamples:
          strMultiSample = " (6x Multisample)";
          break;
        case MultiSampleType.SevenSamples:
          strMultiSample = " (7x Multisample)";
          break;
        case MultiSampleType.EightSamples:
          strMultiSample = " (8x Multisample)";
          break;
        case MultiSampleType.NineSamples:
          strMultiSample = " (9x Multisample)";
          break;
        case MultiSampleType.TenSamples:
          strMultiSample = " (10x Multisample)";
          break;
        case MultiSampleType.ElevenSamples:
          strMultiSample = " (11x Multisample)";
          break;
        case MultiSampleType.TwelveSamples:
          strMultiSample = " (12x Multisample)";
          break;
        case MultiSampleType.ThirteenSamples:
          strMultiSample = " (13x Multisample)";
          break;
        case MultiSampleType.FourteenSamples:
          strMultiSample = " (14x Multisample)";
          break;
        case MultiSampleType.FifteenSamples:
          strMultiSample = " (15x Multisample)";
          break;
        case MultiSampleType.SixteenSamples:
          strMultiSample = " (16x Multisample)";
          break;
        default:
          strMultiSample = string.Empty;
          break;
      }

      frameStatsLine1 = String.Format("last {0} fps ({1}x{2}), {3} {4}{5}{6} {7}",
                                 GUIGraphicsContext.CurrentFPS.ToString("f2"),
                                 GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth,
                                 GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight,
                                 GetSleepingTime(), strFmt, strDepthFmt, strMultiSample, ShouldUseSleepingTime());

      frameStatsLine2 = String.Format("");

      if (GUIGraphicsContext.Vmr9Active)
        frameStatsLine2 = String.Format("VMR9 {0} ", GUIGraphicsContext.Vmr9FPS.ToString("f2"));

      string quality = String.Format("avg fps:{0} sync:{1} drawn:{2} dropped:{3} jitter:{4}",
                                     VideoRendererStatistics.AverageFrameRate.ToString("f2"),
                                     VideoRendererStatistics.AverageSyncOffset,
                                     VideoRendererStatistics.FramesDrawn,
                                     VideoRendererStatistics.FramesDropped,
                                     VideoRendererStatistics.Jitter);
#if PERFCOUNTER
        long MBUsed = Process.GetCurrentProcess().PrivateMemorySize;
        MBUsed /= 1024; // KByte;
        MBUsed /= 1024; // MByte;

        quality += String.Format(" Memory:{0} Mb cpu:{1}%",
              MBUsed, _perfCounterCpu.NextValue().ToString("f2"));
#endif
      frameStatsLine2 += quality;
      //long lTotalMemory=GC.GetTotalMemory(false);
      //string memory=String.Format("\nTotal Memory allocated:{0}",Utils.GetSize(lTotalMemory) );

      //frameStatsLine2+=memory;
    }

    /// <summary>
    /// Update the various statistics the simulation keeps track of
    /// </summary>
    public void UpdateStats()
    {
      // Keep track of the frame count
      //if (frames < 10) return;
      float time = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);
      // Update the scene stats once per second
      if (time - lastTime >= 1.0f)
      {
        framePerSecond = frames / (time - lastTime);
        GUIGraphicsContext.CurrentFPS = framePerSecond;
        lastTime = time;
        frames = 0;
      }
    }


    /// <summary>
    /// Set our variables to not active and not ready
    /// </summary>
    public void CleanupEnvironment()
    {
      active = false;
      ready = false;
      if (GUIGraphicsContext.DX9Device != null)
        GUIGraphicsContext.DX9Device.Dispose();
    }

    #region Menu EventHandlers

    public void OnSetup(object sender, EventArgs e)
    {
      string processName = "Configuration.exe";

      foreach (Process process in Process.GetProcesses())
        if (process.ProcessName.Equals(processName))
          return;

      Log.Info("D3D: OnSetup - Stopping media");
      g_Player.Stop();

      if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed)
        SwitchFullScreenOrWindowed(true);

      _autoHideMouse = false;
      Cursor.Show();
      Invalidate(true);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        xmlreader.Clear();

      MediaPortal.Util.Utils.StartProcess(Config.GetFile(Config.Dir.Base, "Configuration.exe"), "", false, false);
    }

    /// <summary>
    /// Will end the simulation
    /// </summary>
    private void ExitSample(object sender, EventArgs e)
    {
      this._shuttingDown = true;
      this.Close();
    }

    #endregion

    #region WinForms Overrides

    /// <summary>
    /// Clean up any resources
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      CleanupEnvironment();

      if (notifyIcon != null)
        //we dispose our tray icon here
        this.notifyIcon.Dispose();

      base.Dispose(disposing);

      if (autoHideTaskbar)
      {
        Win32API.EnableStartBar(true);
        Win32API.ShowStartBar(true);
      }
    }


    /// <summary>
    /// Handle any key presses
    /// </summary>
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      /*
                  // Check for our shortcut keys (Escape to quit)
                  if ((byte)e.KeyChar == (byte)(int)System.Windows.Forms.Keys.Escape)
                  {
                    mnuExit.PerformClick();
                    e.Handled = true;
                  }*/

      // Allow the control to handle the keystroke now
      if (!e.Handled)
        base.OnKeyPress(e);
    }


    private void menuItem2_Click(object sender, EventArgs e)
    {
      OnSetup(sender, e);
    }


    private void D3DApp_Load(object sender, EventArgs e)
    {
      Application.Idle += Application_Idle;

      Initialize();
      OnStartup();

      try
      {
        // give an external app a change to be notified when the appliction has reached the final stage of startup

        EventWaitHandle handle = EventWaitHandle.OpenExisting("MediaPortalHandleCreated");

        if (handle.SafeWaitHandle.IsInvalid)
          return;

        handle.Set();
        handle.Close();
      }
      catch
      { }
      if (GUIGraphicsContext.UseSeparateRenderThread)
      {
        Thread processThread = new Thread(ProcessLoop);
        processThread.Name = "ProcessingThread";
        processThread.IsBackground = true;
        processThread.Start();
      }

      bool result = ShowLastActiveModule();
    }

    private void ProcessLoop()
    {
      while(true)
      {
        OnProcess();
        FrameMove();
        Thread.Sleep(50); 
      }
    }

    private void TvDelayThread()
    {
      //we have to use a small delay before calling tvfullscreen.                              
      Thread.Sleep(200);
      g_Player.ShowFullScreenWindow();
    }
    /*
    private void ModuleDelayThread(int lastActiveModule)
    {
      //we have to use a small delay before calling module.                              
      Thread.Sleep(500);
      GUIWindowManager.ActivateWindow(lastActiveModule);            
    }
    */

    protected bool ShowLastActiveModule()
    {
      bool showLastActiveModule = false;
      int lastActiveModule = -1;
      bool lastActiveModuleFullscreen = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        showLastActiveModule = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
        lastActiveModule = xmlreader.GetValueAsInt("general", "lastactivemodule", -1);
        lastActiveModuleFullscreen = xmlreader.GetValueAsBool("general", "lastactivemodulefullscreen", false);

        // check if system has been awaken by user or psclient.
        // if by psclient, DO NOT resume last active module
        if (showLastActiveModule)
        {
          string psClientNextwakeupStr = xmlreader.GetValueAsString("psclientplugin", "nextwakeup", DateTime.MaxValue.ToString());
          DateTime now = DateTime.Now;
          DateTime psClientNextwakeupDate = Convert.ToDateTime(psClientNextwakeupStr);
          TimeSpan ts = psClientNextwakeupDate - now;

          Log.Debug("ShowLastActiveModule() - psclientplugin nextwakeup {0}", psClientNextwakeupStr);
          Log.Debug("ShowLastActiveModule() - timediff in minutes {0}", ts.TotalMinutes);

          if (ts.TotalMinutes < 2 && ts.TotalMinutes > -2)
          {
            Log.Debug("ShowLastActiveModule() - system probably awoken by PSclient, ignoring ShowLastActiveModule");
            return false;
          }
          else
          {
            Log.Debug("ShowLastActiveModule() - system probably awoken by user, continuing with ShowLastActiveModule");
          }
        }

      }

      Log.Debug("d3dapp: ShowLastActiveModule active : {0}", showLastActiveModule);


      if (showLastActiveModule)
      {
        Log.Debug("d3dapp: ShowLastActiveModule module : {0}", lastActiveModule);
        Log.Debug("d3dapp: ShowLastActiveModule fullscreen : {0}", lastActiveModuleFullscreen);
        if (lastActiveModule < 0)
        {
          Log.Error("Error recalling last active module - invalid module name '{0}'", lastActiveModule);
          //otherwise ignore.
        }
        else
        {
          try
          {
            GUIWindowManager.ActivateWindow(lastActiveModule);
            /*            
            ThreadStart ts = delegate() { ModuleDelayThread(lastActiveModule); };            
            Thread moduleDelayThread = new Thread(ts);            
            moduleDelayThread.Start();
            */

            if (lastActiveModule == (int)GUIWindow.Window.WINDOW_TV && lastActiveModuleFullscreen)
            {
              //GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV);
              //GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);                            

              Thread tvDelayThread = new Thread(TvDelayThread);
              tvDelayThread.IsBackground = true;
              tvDelayThread.Name = "TvDelayThread";
              tvDelayThread.Start();
            }

            return true;
          }
          catch (Exception e)
          {
            Log.Error("Error recalling last active module '{0}' - {1}", lastActiveModule, e.Message);
            //otherwise ignore.
          }
        }
      }
      return false;
    }

    private void D3DApp_Closing(object sender, CancelEventArgs e)
    {
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
      g_Player.Stop();
    }


    private void D3DApp_Click(object sender, MouseEventArgs e)
    {
      if (ActiveForm != this)
        return;

      mouseclick(e);
    }


    private void D3DApp_MouseMove(object sender, MouseEventArgs e)
    {
      if (ActiveForm != this)
        return;

      mousemove(e);
    }


    protected void ToggleFullWindowed()
    {
      Log.Info("D3D: Fullscreen / windowed mode toggled");
      isMaximized = !isMaximized;
      //Force player to stop so as not to crash during toggle
      if (GUIGraphicsContext.Vmr9Active)
      {
        Log.Info("D3D: Vmr9Active - Stopping media");
        g_Player.Stop();
      }
      GUITextureManager.CleanupThumbs();
      GUITextureManager.Dispose();
      GUIFontManager.Dispose();
      if (isMaximized)
      {
        Log.Info("D3D: Switching windowed mode -> fullscreen");
        if (autoHideTaskbar)
        {
          Win32API.EnableStartBar(false);
          Win32API.ShowStartBar(false);
        }

        this.FormBorderStyle = FormBorderStyle.None;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Menu = null;
        oldBounds = this.Bounds;
        Rectangle newBounds = GUIGraphicsContext.currentScreen.Bounds;
        this.Bounds = newBounds;
        this.Update();
        Log.Info("D3D: Switching windowed mode -> fullscreen done - Maximized: {0}", isMaximized);
        Log.Info("D3D: Client size: {0}x{1} - Screen: {2}x{3}",
                  this.ClientSize.Width, this.ClientSize.Height,
                  GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
        SwitchFullScreenOrWindowed(false);
      }
      else
      {
        Log.Info("D3D: Switching fullscreen -> windowed mode");
        if (autoHideTaskbar)
        {
          Win32API.EnableStartBar(true);
          Win32API.ShowStartBar(true);
        }
        this.WindowState = FormWindowState.Normal;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = true;
        this.Menu = menuStripMain;
        Rectangle newBounds = new Rectangle(oldBounds.X, oldBounds.Y, oldBounds.Width, oldBounds.Height);
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          bool autosize = xmlreader.GetValueAsBool("general", "autosize", true);
          if (autosize && !GUIGraphicsContext.Fullscreen)
          {
            newBounds.Height = GUIGraphicsContext.SkinSize.Height;
            newBounds.Width = GUIGraphicsContext.SkinSize.Width;
          }
        }
        this.Bounds = newBounds;
        this.Update();
        Log.Info("D3D: Switching fullscreen -> windowed mode done - Maximized: {0}", isMaximized);
        Log.Info("D3D: Client size: {0}x{1} - Screen: {2}x{3}",
                  this.ClientSize.Width, this.ClientSize.Height,
                  GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
        SwitchFullScreenOrWindowed(true);
      }
      OnDeviceReset(null, null);
    }

    /// <summary>
    /// Handle system keystrokes (ie, alt-enter)
    /// </summary>
    protected void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Control == false && e.Alt == true && (e.KeyCode == Keys.Return))
      {
        ToggleFullWindowed();
        e.Handled = true;
        return;
      }
      else if (e.KeyCode == Keys.F2)
        OnSetup(null, null);
      if (e.Handled == false)
        keydown(e);
    }

    /// <summary>
    /// Winforms generated code for initializing the form
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(D3DApp));
      this.menuStripMain = new System.Windows.Forms.MainMenu(this.components);
      this.menuItemFile = new System.Windows.Forms.MenuItem();
      this.menuItemExit = new System.Windows.Forms.MenuItem();
      this.menuItemOptions = new System.Windows.Forms.MenuItem();
      this.menuItemFullscreen = new System.Windows.Forms.MenuItem();
      this.menuItemConfiguration = new System.Windows.Forms.MenuItem();
      this.menuItemWizards = new System.Windows.Forms.MenuItem();
      this.menuItemDVD = new System.Windows.Forms.MenuItem();
      this.menuItemMovies = new System.Windows.Forms.MenuItem();
      this.menuItemMusic = new System.Windows.Forms.MenuItem();
      this.menuItemPictures = new System.Windows.Forms.MenuItem();
      this.menuItemTelevision = new System.Windows.Forms.MenuItem();
      this.menuItemChangeDevice = new System.Windows.Forms.MenuItem();
      this.menuBreakFile = new System.Windows.Forms.MenuItem();
      this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.contextMenu = new System.Windows.Forms.ContextMenu();
      this.menuItemContext = new System.Windows.Forms.MenuItem();
      this.menuItem5 = new System.Windows.Forms.MenuItem();
      this.SuspendLayout();
      // 
      // menuStripMain
      // 
      this.menuStripMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemFile,
            this.menuItemOptions,
            this.menuItemWizards});
      // 
      // menuItemFile
      // 
      this.menuItemFile.Index = 0;
      this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemExit});
      this.menuItemFile.Text = "&File";
      // 
      // menuItemExit
      // 
      this.menuItemExit.Index = 0;
      this.menuItemExit.Text = "Exit";
      this.menuItemExit.Click += new System.EventHandler(this.ExitSample);
      // 
      // menuItemOptions
      // 
      this.menuItemOptions.Index = 1;
      this.menuItemOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemFullscreen,
            this.menuItemConfiguration});
      this.menuItemOptions.Text = "&Options";
      // 
      // menuItemFullscreen
      // 
      this.menuItemFullscreen.Index = 0;
      this.menuItemFullscreen.Text = "&Fullscreen";
      this.menuItemFullscreen.Click += new System.EventHandler(this.menuItemFullscreen_Click);
      // 
      // menuItemConfiguration
      // 
      this.menuItemConfiguration.Index = 1;
      this.menuItemConfiguration.Shortcut = System.Windows.Forms.Shortcut.F2;
      this.menuItemConfiguration.Text = "&Configuration...";
      this.menuItemConfiguration.Click += new System.EventHandler(this.menuItem2_Click);
      // 
      // menuItemWizards
      // 
      this.menuItemWizards.Index = 2;
      this.menuItemWizards.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemDVD,
            this.menuItemMovies,
            this.menuItemMusic,
            this.menuItemPictures,
            this.menuItemTelevision});
      this.menuItemWizards.Text = "Wizards";
      // 
      // menuItemDVD
      // 
      this.menuItemDVD.Index = 0;
      this.menuItemDVD.Text = "DVD";
      this.menuItemDVD.Click += new System.EventHandler(this.dvdMenuItem_Click);
      // 
      // menuItemMovies
      // 
      this.menuItemMovies.Index = 1;
      this.menuItemMovies.Text = "Movies";
      this.menuItemMovies.Click += new System.EventHandler(this.moviesMenuItem_Click);
      // 
      // menuItemMusic
      // 
      this.menuItemMusic.Index = 2;
      this.menuItemMusic.Text = "Music";
      this.menuItemMusic.Click += new System.EventHandler(this.musicMenuItem_Click);
      // 
      // menuItemPictures
      // 
      this.menuItemPictures.Index = 3;
      this.menuItemPictures.Text = "Pictures";
      this.menuItemPictures.Click += new System.EventHandler(this.picturesMenuItem_Click);
      // 
      // menuItemTelevision
      // 
      this.menuItemTelevision.Index = 4;
      this.menuItemTelevision.Text = "Television";
      this.menuItemTelevision.Click += new System.EventHandler(this.televisionMenuItem_Click);
      // 
      // menuItemChangeDevice
      // 
      this.menuItemChangeDevice.Index = -1;
      this.menuItemChangeDevice.Text = "";
      // 
      // menuBreakFile
      // 
      this.menuBreakFile.Index = -1;
      this.menuBreakFile.Text = "-";
      // 
      // notifyIcon
      // 
      this.notifyIcon.ContextMenu = this.contextMenu;
      this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
      this.notifyIcon.Text = "MediaPortal";
      this.notifyIcon.DoubleClick += new System.EventHandler(this.Restore_OnClick);
      // 
      // contextMenu
      // 
      this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemContext});
      this.contextMenu.Popup += new System.EventHandler(this.contextMenu1_Popup);
      // 
      // menuItemContext
      // 
      this.menuItemContext.Index = 0;
      this.menuItemContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem5});
      this.menuItemContext.Text = "";
      // 
      // menuItem5
      // 
      this.menuItem5.Index = 0;
      this.menuItem5.Text = "";
      // 
      // D3DApp
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(720, 576);
      this.KeyPreview = true;
      this.MinimumSize = new System.Drawing.Size(100, 100);
      this.Name = "D3DApp";
      this.Load += new System.EventHandler(this.D3DApp_Load);
      this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.D3DApp_MouseDoubleClick);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.D3DApp_Click);
      this.Closing += new System.ComponentModel.CancelEventHandler(this.D3DApp_Closing);
      this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.D3DApp_MouseMove);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
      this.ResumeLayout(false);

    }

    /// <summary>
    /// Make sure our graphics cursor (if available) moves with the cursor
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      _mouseTimeOutTimer = DateTime.Now;
      if ((GUIGraphicsContext.DX9Device != null) && (!GUIGraphicsContext.DX9Device.Disposed))
        // Move the D3D cursor
        GUIGraphicsContext.DX9Device.SetCursorPosition(e.X, e.Y, false);
      // Let the control handle the mouse now
      base.OnMouseMove(e);
    }

    /// <summary>
    /// Handle resize events
    /// </summary>
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
        if (notifyIcon != null)
        {
          if (notifyIcon.Visible == false && this.WindowState == FormWindowState.Minimized)
          {
            notifyIcon.Visible = true;
            this.Hide();
            if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD)
            {
              if (g_Player.Volume > 0)
              {
                m_iVolume = g_Player.Volume;
                g_Player.Volume = 0;
              }
              if (g_Player.Paused == false)
              {
                g_Player.Pause();
              }
            }
            return;
          }
          else if (notifyIcon.Visible == true && this.WindowState != FormWindowState.Minimized)
          {
            notifyIcon.Visible = false;
          }
        }
        active = !(this.WindowState == FormWindowState.Minimized);
        base.OnResize(e);
      }
      catch (Exception ex)
      {
        Log.Error("d3dapp: An error occured in OnResize - {0}", ex.Message);
      }
    }

    /// <summary>
    /// Once the form has focus again, we can continue to handle our resize
    /// and resets..
    /// </summary>
    protected override void OnGotFocus(EventArgs e)
    {
      isHandlingSizeChanges = true;
      isWindowActive = true;
      base.OnGotFocus(e);
    }

    /// <summary>
    /// Handle move events
    /// </summary>
    protected override void OnMove(EventArgs e)
    {
      if (isHandlingSizeChanges)
      {
        //storedLocation = this.Location;
      }
      base.OnMove(e);
    }

    /// <summary>
    /// Handle closing event
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
      if (this._minimizeOnGuiExit && !_shuttingDown)
      {
        if (WindowState != FormWindowState.Minimized)
          Log.Info("D3D: Minimizing to tray on GUI exit");
        isClosing = false;
        WindowState = FormWindowState.Minimized;
        Hide();
        e.Cancel = true;

        if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD)
        {
          if (g_Player.Volume > 0)
          {
            m_iVolume = g_Player.Volume;
            g_Player.Volume = 0;
          }
          if (g_Player.Paused == false)
            g_Player.Pause();
        }
        return;
      }
      if (autoHideTaskbar)
      {
        Win32API.EnableStartBar(true);
        Win32API.ShowStartBar(true);
      }
      isClosing = true;
      base.OnClosing(e);
    }
    #endregion

    protected virtual void keypressed(KeyPressEventArgs e)
    {
    }

    protected virtual void keydown(KeyEventArgs e)
    {
    }

    protected virtual void mousemove(MouseEventArgs e)
    {
      if (lastx == -1 || lasty == -1)
      {
        lastx = e.X;
        lasty = e.Y;
      }
      else if (lastx != e.X || lasty != e.Y)
      {
        if (!_disableMouseEvents)
        {
          //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,lastx,lasty);
          lastx = e.X;
          lasty = e.Y;
          Cursor ourCursor = this.Cursor;
          if (!_showCursor)
          {
            Cursor.Show();
            _showCursor = true;
            Invalidate(true);
          }
        }
        _mouseTimeOutTimer = DateTime.Now;
      }
    }

    protected virtual void mouseclick(MouseEventArgs e)
    {
      //this.Text=String.Format("show click");
      Cursor ourCursor = this.Cursor;
      if (!_showCursor)
      {
        Cursor.Show();
        _showCursor = true;
        Invalidate(true);
      }
      _mouseTimeOutTimer = DateTime.Now;
    }

    protected virtual void mousedoubleclick(MouseEventArgs e)
    {
      //this.Text=String.Format("show click");
      Cursor ourCursor = this.Cursor;
      if (!_showCursor)
      {
        Cursor.Show();
        _showCursor = true;
        Invalidate(true);
      }
      _mouseTimeOutTimer = DateTime.Now;
    }

    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
      keypressed(e);
    }

    private void televisionMenuItem_Click(object sender, EventArgs e)
    {
      g_Player.Stop();

      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        xmlreader.Clear();
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\television.xml");
    }

    private void picturesMenuItem_Click(object sender, EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        xmlreader.Clear();
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\pictures.xml");
    }

    private void musicMenuItem_Click(object sender, EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        xmlreader.Clear();
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\music.xml");
    }

    private void moviesMenuItem_Click(object sender, EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        xmlreader.Clear();
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\movies.xml");
    }

    private void dvdMenuItem_Click(object sender, EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true);
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        xmlreader.Clear();
      Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard /section=wizards\dvd.xml");
    }

    private Win32API.MSG msgApi = new Win32API.MSG();
    public void HandleMessage()
    {
      try
      {
        if (!Win32API.PeekMessage(ref msgApi, IntPtr.Zero, 0, 0, 0))
          return;
        if (!Win32API.GetMessageA(ref msgApi, IntPtr.Zero, 0, 0))
          return;
        Win32API.TranslateMessage(ref msgApi);
        Win32API.DispatchMessageA(ref msgApi);
        //	System.Windows.Forms.Application.DoEvents();//SLOW
      }
#if DEBUG
      catch (Exception ex)
      {
        Log.Info("D3D: Exception: {0}", ex.ToString());
#else
      catch (Exception)
      {
#endif
      }
    }

    private void StartFrameClock()
    {
      NativeMethods.QueryPerformanceCounter(ref startFrame);
    }

    private bool ShouldWaitForFrameClock()
    {
      long timeElapsed = 0;
      NativeMethods.QueryPerformanceCounter(ref endFrame);
      timeElapsed = endFrame - startFrame;
      if (timeElapsed < GUIGraphicsContext.DesiredFrameTime)
        return true;
      return false;
    }

    private void WaitForFrameClock()
    {
      //if (GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS>1f) return;
      long milliSecondsLeft;
      long timeElapsed = 0;

      // frame limiting code.
      // sleep as long as there are ticks left for this frame
      NativeMethods.QueryPerformanceCounter(ref endFrame);
      timeElapsed = endFrame - startFrame;
      if (timeElapsed < GUIGraphicsContext.DesiredFrameTime)
      {
        milliSecondsLeft = (((GUIGraphicsContext.DesiredFrameTime - timeElapsed) * 1000) / DXUtil.TicksPerSecond);
        if (milliSecondsLeft > 0)
          DoSleep((int)milliSecondsLeft);
      }
    }

    /*
        /// <summary>
        /// Run the simulation
        /// </summary>
        public void Run()
        {
          // Now we're ready to recieve and process Windows messages.
          System.Windows.Forms.Control mainWindow = this;

          // If the render target is a form and *not* this form, use that form instead,
          // otherwise, use the main form.
          if ((ourRenderTarget is System.Windows.Forms.Form) && (ourRenderTarget != this))
            mainWindow = ourRenderTarget;

          mainWindow.Show();

          // Get the first message		
          storedSize = this.ClientSize;
          storedLocation = this.Location;

          GC.Collect();
          GC.Collect();
          GC.Collect();
          GC.WaitForPendingFinalizers();
          bool useFrameClock=false;
          int  counter=0;
          while (true)
          {
            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
              break;
            try
            {
              if (g_Player.Playing) 
              {
                System.Threading.Thread.Sleep(100);
              //	HandleMessage();
						
              }
              else
              {
                useFrameClock=false;
							
                if (ShouldUseSleepingTime())
                {
                  if (GUIGraphicsContext.IsFullScreenVideo&&  g_Player.Playing && g_Player.IsMusic && g_Player.HasVideo)
                  {
                    //dont sleep
                  }
                  else 
                  {
                    // Do some sleep....
                    useFrameClock=true;
                  }
                }
                else
                {
                  if (GUIGraphicsContext.IsFullScreenVideo && g_Player.Playing && g_Player.IsMusic && g_Player.HasVideo)
                  {
                    //dont sleep
                  }
                  else
                  {
                    GUIGraphicsContext.CurrentFPS = 0f;
                    DoSleep(50);
                  }
                }
                FrameMove();
                if (GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS<1f) 
                {
                  if (VMR9Util.g_vmr9!=null)
                  {
                    VMR9Util.g_vmr9.Process();
                  }
                }
                if (GUIGraphicsContext.IsFullScreenVideo==false||g_Player.Speed!=1) counter=0;
                if (counter==0)
                {
                  OnProcess();
                  if (useFrameClock)
                    StartFrameClock();
                  FullRender();
                  if (useFrameClock)
                    WaitForFrameClock();
                }
                else if (counter==5 || counter==10 || counter==15||counter==20)
                {
                  if (VMR7Util.g_vmr7!=null)
                    FullRender();
                }
                HandleMessage();
                counter++;
                if (counter>25) counter=0;
              }
            }
            catch (Exception ex)
            {
              Log.Error("exception:{0}", ex.ToString());
    #if DEBUG
            throw ex;
    #endif
            }
          }
          OnExit();
          try
          {
            if (UseMillisecondTiming) timeEndPeriod(MILLI_SECONDS_TIMER);
          }
          catch (Exception)
          {
          }
          UseMillisecondTiming = false;
        }

    */

    private void contextMenu1_Popup(object sender, EventArgs e)
    {
      contextMenu.MenuItems.Clear();

      // Add a menu item 
      contextMenu.MenuItems.Add("Restore", new EventHandler(this.Restore_OnClick));
      contextMenu.MenuItems.Add("Exit", new EventHandler(this.Exit_OnClick));
    }

    protected virtual void Restore_OnClick(Object sender, EventArgs e)
    {
      Restore();
    }

    public void Restore()
    {
      Log.Info("D3D: Restoring from tray");
      _fromTray = true;
      Show();
      notifyIcon.Visible = false;
      this.WindowState = FormWindowState.Normal;
      Activate();
      active = true;
      // If the Minimize On Gui Exit option is set and we are restoring to fullscreen
      // we should check whether the autoHideTaskbar option is set...

      bool fullScreenMode = this.Menu == null;
      if (fullScreenMode && autoHideTaskbar)
      {
        Win32API.EnableStartBar(false);
        Win32API.ShowStartBar(false);
        Log.Info("D3D: Hiding taskbar");
      }
    }

    protected void Exit_OnClick(Object sender, EventArgs e)
    {
      _shuttingDown = true;
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
    }


    private void menuItemFullscreen_Click(object sender, EventArgs e)
    {
      ToggleFullWindowed();

      GUIDialogNotify dialogNotify =
        (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (dialogNotify != null)
      {
        dialogNotify.SetHeading(1020);
        dialogNotify.SetText(String.Format("{0}\n{1}", GUILocalizeStrings.Get(1021), GUILocalizeStrings.Get(1022)));
        dialogNotify.TimeOut = 6;
        dialogNotify.SetImage(String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin, "dialog_information.png"));
        dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    private bool AppStillIdle()
    {
      bool result = Win32API.PeekMessage(ref msgApi, IntPtr.Zero, 0, 0, 0);
      if (result)
      {
        //System.Diagnostics.Debug.WriteLine(String.Format("msg :hwnd:{0:X} msg:{1:x} wparm:{2:X} lparm:{3:X}", msg1.hwnd, msg1.message, msg1.wParam, msg1.lParam));
      }
      return !result;
    }

    private static int loopCount = 1;
    private static int sleepCount = 0;

    private void Application_Idle(object sender, EventArgs e)
    {
      do
      {
        if (!GUIGraphicsContext.UseSeparateRenderThread)
        {
          OnProcess();
          FrameMove();
        }
          StartFrameClock();
          FullRender();
          // rtv: trying to unify the FPS-Handling for all modules despite VMR renderer
          //if (g_Player.Playing /*&& !g_Player.IsExternalPlayer*/ && g_Player.IsMusic)         
          //if (g_Player.Playing /*&& !g_Player.IsExternalPlayer*/ && g_Player.IsMusic && !g_Player.HasVideo)
          //if (g_Player.Playing && !g_Player.IsExternalPlayer && !g_Player.IsMusic && !g_Player.IsVideo)
          //{
          //  if (GUIGraphicsContext.CurrentFPS < GUIGraphicsContext.MaxFPS)
          //    loopCount++;
          //  //else if (loopCount > 0)
          //  else if (GUIGraphicsContext.CurrentFPS > GUIGraphicsContext.MaxFPS)
          //    loopCount--;

          //  sleepCount++;
          //  if (sleepCount >= loopCount)
          //  {
          //    WaitForFrameClock();
          //    sleepCount = 0;
          //    UpdateStats();
          //  }
          //}
          //else
          {
            loopCount = 1;
            WaitForFrameClock();
          }
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
          break;
        // Suggested by gibman. Let's see if this works. Can't be bad  
        // disabled by Bavarian: caused mantis bug 1021 => please try to reduce FPS in Setting -> this maybe have the same affect
        //Thread.Sleep(10); // 10 milliseconds - fixes sluggish GUI due to 100% cpu usage when turning TV off in a multiseat client and returning to the home screen or any other screen.
      }
      while (AppStillIdle());
    }

    protected void DoMinimizeOnStartup()
    {
      Log.Info("d3dapp: Minimizing to tray on startup");

      WindowState = FormWindowState.Minimized;
      Hide();
      notifyIcon.Visible = true;
    }

    private void D3DApp_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (ActiveForm != this)
        return;

      mousedoubleclick(e);
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
      bool skipMessage = false;
      Rectangle newBounds = new Rectangle(x, y, width, height);
      if (GUIGraphicsContext._useScreenSelector && isMaximized)
      {
        if (!newBounds.Equals(GUIGraphicsContext.currentScreen.Bounds))
        {
          skipMessage = true;
        }
      }
      if (skipMessage)
      {
        Log.Info("d3dapp: Screenselector: skipped SetBoundsCore {0} does not match {1}", newBounds.ToString(), GUIGraphicsContext.currentScreen.Bounds.ToString());
      }
      else
      {
        base.SetBoundsCore(x, y, width, height, specified);
      }
    }
  }

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

  #region Various SampleExceptions

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
        string strMsg = string.Empty;

        strMsg = "Generic application error. Enable\n";
        strMsg += "debug output for detailed information.";

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
        string strMsg = string.Empty;
        strMsg = "This sample cannot run in a desktop\n";
        strMsg += "window with the current display settings.\n";
        strMsg += "Please change your desktop settings to a\n";
        strMsg += "16- or 32-bit display mode and re-run this\n";
        strMsg += "sample.";

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
        string strMsg = string.Empty;
        strMsg = "Warning: Nothing will be rendered.\n";
        strMsg += "The reference rendering device was selected, but your\n";
        strMsg += "computer only has a reduced-functionality reference device\n";
        strMsg += "installed. Please check if your graphics card and\n";
        strMsg += "drivers meet the minimum system requirements.\n";

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
        string strMsg = string.Empty;
        strMsg = "Could not reset the Direct3D device.";

        return strMsg;
      }
    }
  }


  /// <summary>
  /// The exception thrown when media couldn't be found
  /// </summary>
  public class MediaNotFoundException : SampleException
  {
    private string mediaFile;

    public MediaNotFoundException(string filename)
      : base()
    {
      mediaFile = filename;
    }

    public MediaNotFoundException()
      : base()
    {
      mediaFile = string.Empty;
    }


    /// <summary>
    /// Return information about the exception
    /// </summary>
    public override string Message
    {
      get
      {
        string strMsg = string.Empty;
        strMsg = "Could not load required media.";
        if (mediaFile.Length > 0)
          strMsg += string.Format("\r\nFile: {0}", mediaFile);

        return strMsg;
      }
    }
  }

  #endregion

  #region Native Methods

  /// <summary>
  /// Will hold native methods which are interop'd
  /// </summary>
  public class NativeMethods
  {
    #region Win32 User Messages / Structures

    /// <summary>Show window flags styles</summary>
    public enum ShowWindowFlags : uint
    {
      Hide = 0,
      ShowNormal = 1,
      Normal = 1,
      ShowMinimized = 2,
      ShowMaximized = 3,
      ShowNoActivate = 4,
      Show = 5,
      Minimize = 6,
      ShowMinNoActivate = 7,
      ShowNotActivated = 8,
      Restore = 9,
      ShowDefault = 10,
      ForceMinimize = 11,
    }


    /// <summary>Window styles</summary>
    [Flags]
    public enum WindowStyles : uint
    {
      Overlapped = 0x00000000,
      Popup = 0x80000000,
      Child = 0x40000000,
      Minimize = 0x20000000,
      Visible = 0x10000000,
      Disabled = 0x08000000,
      ClipSiblings = 0x04000000,
      ClipChildren = 0x02000000,
      Maximize = 0x01000000,
      Caption = 0x00C00000, /* WindowStyles.Border | WindowStyles.DialogFrame  */
      Border = 0x00800000,
      DialogFrame = 0x00400000,
      VerticalScroll = 0x00200000,
      HorizontalScroll = 0x00100000,
      SystemMenu = 0x00080000,
      ThickFrame = 0x00040000,
      Group = 0x00020000,
      TabStop = 0x00010000,
      MinimizeBox = 0x00020000,
      MaximizeBox = 0x00010000,
    }


    /// <summary>Peek message flags</summary>
    public enum PeekMessageFlags : uint
    {
      NoRemove = 0,
      Remove = 1,
      NoYield = 2,
    }


    /// <summary>Window messages</summary>
    public enum WindowMessage : uint
    {
      // Misc messages
      Destroy = 0x0002,
      Close = 0x0010,
      Quit = 0x0012,
      Paint = 0x000F,
      SetCursor = 0x0020,
      ActivateApplication = 0x001C,
      EnterMenuLoop = 0x0211,
      ExitMenuLoop = 0x0212,
      NonClientHitTest = 0x0084,
      PowerBroadcast = 0x0218,
      SystemCommand = 0x0112,
      GetMinMax = 0x0024,

      // Keyboard messages
      KeyDown = 0x0100,
      KeyUp = 0x0101,
      Character = 0x0102,
      SystemKeyDown = 0x0104,
      SystemKeyUp = 0x0105,
      SystemCharacter = 0x0106,

      // Mouse messages
      MouseMove = 0x0200,
      LeftButtonDown = 0x0201,
      LeftButtonUp = 0x0202,
      LeftButtonDoubleClick = 0x0203,
      RightButtonDown = 0x0204,
      RightButtonUp = 0x0205,
      RightButtonDoubleClick = 0x0206,
      MiddleButtonDown = 0x0207,
      MiddleButtonUp = 0x0208,
      MiddleButtonDoubleClick = 0x0209,
      MouseWheel = 0x020a,
      XButtonDown = 0x020B,
      XButtonUp = 0x020c,
      XButtonDoubleClick = 0x020d,
      MouseFirst = LeftButtonDown, // Skip mouse move, it happens a lot and there is another message for that
      MouseLast = XButtonDoubleClick,

      // Sizing
      EnterSizeMove = 0x0231,
      ExitSizeMove = 0x0232,
      Size = 0x0005,
    }


    /// <summary>Mouse buttons</summary>
    public enum MouseButtons
    {
      Left = 0x0001,
      Right = 0x0002,
      Middle = 0x0010,
      Side1 = 0x0020,
      Side2 = 0x0040,
    }


    /// <summary>Windows Message</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Message
    {
      public IntPtr hWnd;
      public WindowMessage msg;
      public IntPtr wParam;
      public IntPtr lParam;
      public uint time;
      public Point p;
    }


    /// <summary>MinMax Info structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MinMaxInformation
    {
      public Point reserved;
      public Point MaxSize;
      public Point MaxPosition;
      public Point MinTrackSize;
      public Point MaxTrackSize;
    }


    /// <summary>Monitor Info structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInformation
    {
      public uint Size; // Size of this structure
      public Rectangle MonitorRectangle;
      public Rectangle WorkRectangle;
      public uint Flags; // Possible flags
    }


    /// <summary>Window class structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowClass
    {
      public int Styles;
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public WndProcDelegate WindowsProc;
      private int ExtraClassData;
      private int ExtraWindowData;
      public IntPtr InstanceHandle;
      public IntPtr IconHandle;
      public IntPtr CursorHandle;
      public IntPtr backgroundBrush;
      [MarshalAs(UnmanagedType.LPTStr)]
      public string MenuName;
      [MarshalAs(UnmanagedType.LPTStr)]
      public string ClassName;
    }

    #endregion

    #region Delegates

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

    #endregion

    #region Windows API calls

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern void DisableProcessWindowsGhosting();
    
    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("winmm.dll")]
    public static extern IntPtr timeBeginPeriod(uint period);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax,
                                          PeekMessageFlags flags);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool TranslateMessage(ref Message msg);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool DispatchMessage(ref Message msg);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern void PostQuitMessage(int exitCode);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
#if(_WIN64)
  private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int index, [MarshalAs(UnmanagedType.FunctionPtr)] WndProcDelegate windowCallback);
#else
    private static extern IntPtr SetWindowLong(IntPtr hWnd, int index,
                                                 [MarshalAs(UnmanagedType.FunctionPtr)] WndProcDelegate windowCallback);
#endif

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLongStyle(IntPtr hWnd, int index, WindowStyles style);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
    private static extern WindowStyles GetWindowLongStyle(IntPtr hWnd, int index);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("kernel32")]
    public static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("kernel32")]
    public static extern bool QueryPerformanceCounter(ref long PerformanceCount);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetClientRect(IntPtr hWnd, out Rectangle rect);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rectangle rect);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int x, int y, int w, int h, uint flags);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ScreenToClient(IntPtr hWnd, ref Point rect);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hWnd, ref MonitorInformation info);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint flags);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern short GetAsyncKeyState(uint key);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SetCapture(IntPtr handle);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ReleaseCapture();

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ShowWindow(IntPtr hWnd, ShowWindowFlags flags);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool SetMenu(IntPtr hWnd, IntPtr menuHandle);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool IsIconic(IntPtr hWnd);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool AdjustWindowRect(ref Rectangle rect, WindowStyles style,
                                               [MarshalAs(UnmanagedType.Bool)] bool menu);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr windowHandle, WindowMessage msg, IntPtr w, IntPtr l);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr RegisterClass(ref WindowClass wndClass);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] string className, IntPtr instanceHandle);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateWindow(int exStyle, [MarshalAs(UnmanagedType.LPTStr)] string className,
                                             [MarshalAs(UnmanagedType.LPTStr)] string windowName,
                                             WindowStyles style, int x, int y, int width, int height, IntPtr parent,
                                             IntPtr menuHandle, IntPtr instanceHandle, IntPtr zero);

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int GetCaretBlinkTime();

    #endregion

    #region Class Methods

    private NativeMethods()
    { } // No creation
    /// <summary>Hooks window messages to go through this new callback</summary>
    public static void HookWindowsMessages(IntPtr window, WndProcDelegate callback)
    {
#if(_WIN64)
  SetWindowLongPtr(window, -4, callback);
#else
      SetWindowLong(window, -4, callback);
#endif
    }

    /// <summary>Set new window style</summary>
    public static void SetStyle(IntPtr window, WindowStyles newStyle)
    {
      SetWindowLongStyle(window, -16, newStyle);
    }

    /// <summary>Get new window style</summary>
    public static WindowStyles GetStyle(IntPtr window)
    {
      return GetWindowLongStyle(window, -16);
    }

    /// <summary>Returns the low word</summary>
    public static short LoWord(uint l)
    {
      return (short)(l & 0xffff);
    }

    /// <summary>Returns the high word</summary>
    public static short HiWord(uint l)
    {
      return (short)(l >> 16);
    }

    /// <summary>Makes two shorts into a long</summary>
    public static uint MakeUInt32(short l, short r)
    {
      return (uint)((l & 0xffff) | ((r & 0xffff) << 16));
    }

    /// <summary>Is this key down right now</summary>
    public static bool IsKeyDown(Keys key)
    {
      return (GetAsyncKeyState((int)Keys.ShiftKey) & 0x8000) != 0;
    }

    #endregion
  }

  #endregion
}
