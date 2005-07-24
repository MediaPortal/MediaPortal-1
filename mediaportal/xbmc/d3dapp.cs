//98472 85932
//enable to following line for profiling
//this will cause Mediaportal to draw the maxium number of FPS. It will use 100% cpu time 
//because it wont do any Sleeps(). This is usefull for the profile so it can see which methods/classes take up the most CPU %
//#define PROFILING 

using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DirectDraw = Microsoft.DirectX.DirectDraw;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Dialogs;
using MediaPortal.TV.Recording;


namespace MediaPortal
{
  /// <summary>
  /// The base class for all the graphics (D3D) samples, it derives from windows forms
  /// </summary>
  public class D3DApp : System.Windows.Forms.Form
  {
		class NativeGameLoop
		{
			public const int PMRemove=1;
			[DllImport("user32.dll", CharSet=CharSet.Auto)]
			public static extern bool PeekMessage([In, Out] ref NativeGameLoop.MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);
 
			[DllImport("user32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
			public static extern bool GetMessageW([In, Out] ref NativeGameLoop.MSG msg, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);
 
			[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
			public static extern bool GetMessageA([In, Out] ref NativeGameLoop.MSG msg, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);
 
			[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
			public static extern bool TranslateMessage([In, Out] ref NativeGameLoop.MSG msg);
 
			[DllImport("user32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
			public static extern IntPtr DispatchMessageW([In] ref NativeGameLoop.MSG msg);

			[DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
			public static extern IntPtr DispatchMessageA([In] ref NativeGameLoop.MSG msg);

			[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
			public static extern IntPtr GetParent(HandleRef hWnd);
 

			public static HandleRef NullHandleRef;


			[StructLayout(LayoutKind.Sequential)]
				public struct MSG
			{
				public IntPtr hwnd;
				public int message;
				public IntPtr wParam;
				public IntPtr lParam;
				public int time;
				public int pt_x;
				public int pt_y;
			}

		}

    const int MILLI_SECONDS_TIMER = 1;
    protected string m_strSkin = "mce";
    protected string m_strLanguage = "english";

    #region Menu Information
    // The menu items that *all* samples will need
    protected System.Windows.Forms.MainMenu mnuMain;
    protected System.Windows.Forms.MenuItem mnuFile;
    private System.Windows.Forms.MenuItem mnuChange;
    private System.Windows.Forms.MenuItem mnuBreak2;
    protected System.Windows.Forms.MenuItem mnuExit;
    #endregion

    protected bool m_bAutoHideMouse = false;
    protected bool m_bNeedUpdate = false;
    protected DateTime m_MouseTimeOut = DateTime.Now;
    // The window we will render too
    private System.Windows.Forms.Control ourRenderTarget;
    // Should we use the default windows
    protected bool isUsingMenus = true;
    private float lastTime = 0.0f; // The last time
    protected int frames = 0; // Number of frames since our last update

    // fps-limiting stuff
    long startFrame = 0;
    long endFrame = 0;
       
    // We need to keep track of our enumeration settings
    protected D3DEnumeration enumerationSettings = new D3DEnumeration();
    protected D3DSettings graphicsSettings = new D3DSettings();
    protected bool isMaximized = false; // Are we maximized?
    private bool isHandlingSizeChanges = true; // Are we handling size changes?
    private bool isClosing = false; // Are we closing?
    private bool isChangingFormStyle = false; // Are we changing the forms style?
    private bool isWindowActive = true; // Are we waiting for got focus?
    protected bool m_bShowCursor = true;
    protected bool m_bLastShowCursor = true;
    bool UseMillisecondTiming = true;

    static int lastx = -1;
    static int lasty = -1;
    // Internal variables for the state of the app
    protected bool windowed;
    protected bool active;
    protected bool ready;
    protected bool hasFocus;
    protected bool isMultiThreaded = true;

    // Internal variables used for timing
    protected bool frameMoving;
    protected bool singleStep;
    // Main objects used for creating and rendering the 3D scene
    protected PresentParameters presentParams = new PresentParameters(); // Parameters for CreateDevice/Reset
    //protected RenderStates renderState;
    //protected SamplerStates sampleState;
    //protected TextureStates textureStates;
    private Caps graphicsCaps;           // Caps for the device
    protected Caps Caps { get { return graphicsCaps; } }
    private CreateFlags behavior;     // Indicate sw or hw vertex processing
    protected BehaviorFlags BehaviorFlags { get { return new BehaviorFlags(behavior); } }
    protected System.Windows.Forms.Control RenderTarget { get { return ourRenderTarget; } set { ourRenderTarget = value; } }

    // Variables for timing
    protected float appTime;             // Current time in seconds
    protected float elapsedTime;      // Time elapsed since last frame
    protected float framePerSecond=25;              // Instanteous frame rate
    protected string deviceStats;// String to hold D3D device stats
    protected string frameStats; // String to hold frame stats

    protected bool deviceLost = false;
    protected bool m_bNeedReset = false;
    // Overridable variables for the app
    private int minDepthBits;    // Minimum number of bits needed in depth buffer
    protected int MinDepthBits { get { return minDepthBits; } set { minDepthBits = value; enumerationSettings.AppMinDepthBits = value; } }
    private int minStencilBits;  // Minimum number of bits needed in stencil buffer
    protected int MinStencilBits { get { return minStencilBits; } set { minStencilBits = value; enumerationSettings.AppMinStencilBits = value; } }
    protected bool showCursorWhenFullscreen; // Whether to show cursor when fullscreen
    protected bool clipCursorWhenFullscreen; // Whether to limit cursor pos when fullscreen
    protected bool startFullscreen; // Whether to start up the app in fullscreen mode

    protected System.Drawing.Size storedSize;
    protected System.Drawing.Point storedLocation;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MenuItem menuItem2;
    private System.Windows.Forms.Timer timer1;
    private System.ComponentModel.IContainer components;
    private System.Windows.Forms.MenuItem menuItem4;
    private System.Windows.Forms.MenuItem dvdMenuItem;
    private System.Windows.Forms.MenuItem moviesMenuItem;
    private System.Windows.Forms.MenuItem musicMenuItem;
    private System.Windows.Forms.MenuItem picturesMenuItem;
    private System.Windows.Forms.MenuItem televisionMenuItem;
    private System.Windows.Forms.NotifyIcon notifyIcon1;
    private System.Windows.Forms.ContextMenu contextMenu1;
    private System.Windows.Forms.MenuItem menuItem3;
    private System.Windows.Forms.MenuItem menuItem5;
    private System.Windows.Forms.MenuItem menuItemFullscreen;
    protected Rectangle oldBounds;

    // Overridable functions for the 3D scene created by the app
    protected virtual bool ConfirmDevice(Caps caps, VertexProcessingType vertexProcessingType,
      Format adapterFormat, Format backBufferFormat) { return true; }
    protected virtual void OneTimeSceneInitialization() { /* Do Nothing */ }
    protected virtual void Initialize() { /* Do Nothing */ }
    protected virtual void InitializeDeviceObjects() { /* Do Nothing */ }
    protected virtual void OnDeviceReset(System.Object sender, System.EventArgs e) { /* Do Nothing */ }
    protected virtual void FrameMove() { /* Do Nothing */ }
    protected virtual void OnProcess() {/* Do Nothing */ }
    protected virtual void Render(float timePassed) { /* Do Nothing */ }
    //protected virtual void OnDeviceLost(System.Object sender, System.EventArgs e) { /* Do Nothing */ }
    //protected virtual void OnDeviceDisposing(System.Object sender, System.EventArgs e) { /* Do Nothing */ }

    protected virtual void OnStartup() { }
    protected virtual void OnExit() { }


    bool m_bWasPlaying = false;
    int m_iActiveWindow = -1;
    bool m_bRestore = false;
    double m_dCurrentPos = 0;
    string m_strCurrentFile;
    PlayListPlayer.PlayListType m_currentPlayList = PlayListPlayer.PlayListType.PLAYLIST_NONE;
    int m_iSleepingTime = 50;
    bool autoHideTaskbar = true;
    bool alwaysOnTop = false;
    DirectDraw.Device m_dddevice = null;
    DirectDraw.SurfaceDescription m_dddescription = null;
    DirectDraw.Surface m_ddfront = null;
    DirectDraw.Surface m_ddback = null;
    bool useExclusiveDirectXMode;

    [DllImport("winmm.dll")]
    internal static extern uint timeBeginPeriod(uint period);

    [DllImport("winmm.dll")]
    internal static extern uint timeEndPeriod(uint period);

    /// Constructor
    /// </summary>
    public D3DApp()
    {
      //GUIGraphicsContext.DX9Device = null;
      try
      {
        int hr = (int)timeBeginPeriod(MILLI_SECONDS_TIMER);
        if (hr != 0)
          UseMillisecondTiming = false;
      }
      catch (Exception)
      {
        UseMillisecondTiming = false;
      }
      active = false;
      ready = false;
      hasFocus = false;
      behavior = 0;

      ourRenderTarget = this;
      frameMoving = true;
      singleStep = false;
      deviceStats = null;
      frameStats = null;

      this.Text = "D3D9 Sample";
      this.ClientSize = new System.Drawing.Size(720, 576);
      this.KeyPreview = true;

      minDepthBits = 16;
      minStencilBits = 0;
      showCursorWhenFullscreen = false;


      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        useExclusiveDirectXMode= xmlreader.GetValueAsBool("general", "exclusivemode", true);
        autoHideTaskbar = xmlreader.GetValueAsBool("general", "hidetaskbar", true);
        alwaysOnTop = xmlreader.GetValueAsBool("general", "alwaysontop", false);
      }
      //      startFullscreen=true;
      // When clipCursorWhenFullscreen is TRUE, the cursor is limited to
      // the device window when the app goes fullscreen.  This prevents users
      // from accidentally clicking outside the app window on a multimon system.
      // This flag is turned off by default for debug builds, since it makes 
      // multimon debugging difficult.
      clipCursorWhenFullscreen = false;
      InitializeComponent();
      this.timer1.Interval = 300;
      this.timer1.Start();
      this.TopMost = alwaysOnTop;

    }

    protected void SetupCamera2D()
    {
      Matrix matOrtho;
      Matrix matIdentity = Matrix.Identity;

      //Setup the orthogonal projection matrix and the default world/view matrix
      matOrtho = Matrix.OrthoLH((float)GUIGraphicsContext.Width, (float)GUIGraphicsContext.Height, 0.0f, 1.0f);


      GUIGraphicsContext.DX9Device.SetTransform(TransformType.Projection, matOrtho);
      GUIGraphicsContext.DX9Device.SetTransform(TransformType.World, matIdentity);
      GUIGraphicsContext.DX9Device.SetTransform(TransformType.View, matIdentity);

      //Make sure that the z-buffer and lighting are disabled
      GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.ZEnable, false);
      GUIGraphicsContext.DX9Device.SetRenderState(RenderStates.Lighting, false);

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
        ourRenderTarget.Cursor = System.Windows.Forms.Cursors.Default;
      }
      // if our render target is the main window and we haven't said 
      // ignore the menus, add our menu
      if ((ourRenderTarget == this) && (isUsingMenus))
        this.Menu = mnuMain;

      try
      {
        ChooseInitialSettings();
        DXUtil.Timer(DirectXTimer.Start);

        // Initialize the application timer
        //@@@fullscreen
        storedSize = this.ClientSize;
        storedLocation = this.Location;
        oldBounds = new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);


        using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          string strStartFull = (string)xmlreader.GetValue("general", "startfullscreen");
          if (strStartFull != null && strStartFull == "yes")
          {
            if (autoHideTaskbar)
            {
              Win32API.EnableStartBar(false);
              Win32API.ShowStartBar(false);
            }

            Log.Write("start fullscreen");
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Menu = null;
            this.Location = new System.Drawing.Point(0, 0);
            this.Bounds = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth=Screen.PrimaryScreen.Bounds.Width;
            //GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight=Screen.PrimaryScreen.Bounds.Height;
            Log.Write("ClientSize: {0}x{1} screen:{2}x{3}",
              this.ClientSize.Width, this.ClientSize.Height,
              Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

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

      foreach (GraphicsAdapterInfo adapterInfo in enumerationSettings.AdapterInfoList)
      {
        foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfoList)
        {
          if (doesRequireHardware && deviceInfo.DevType != DeviceType.Hardware)
            continue;
          if (doesRequireReference && deviceInfo.DevType != DeviceType.Reference)
            continue;

          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
          {
            bool adapterMatchesBackBuffer = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);
            if (!deviceCombo.IsWindowed)
              continue;
            if (deviceCombo.AdapterFormat != primaryDesktopDisplayMode.Format)
              continue;

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
      }

      EndWindowedDeviceComboSearch:
        if (bestDeviceCombo == null)
          return false;

      graphicsSettings.WindowedAdapterInfo = bestAdapterInfo;
      graphicsSettings.WindowedDeviceInfo = bestDeviceInfo;
      graphicsSettings.WindowedDeviceCombo = bestDeviceCombo;
      graphicsSettings.IsWindowed = true;
      graphicsSettings.WindowedDisplayMode = primaryDesktopDisplayMode;
      graphicsSettings.WindowedWidth = ourRenderTarget.ClientRectangle.Right - ourRenderTarget.ClientRectangle.Left;
      graphicsSettings.WindowedHeight = ourRenderTarget.ClientRectangle.Bottom - ourRenderTarget.ClientRectangle.Top;
      if (enumerationSettings.AppUsesDepthBuffer)
        graphicsSettings.WindowedDepthStencilBufferFormat = (DepthFormat)bestDeviceCombo.DepthStencilFormatList[0];
      int iQuality = 0;//bestDeviceCombo.MultiSampleTypeList.Count-1;
      graphicsSettings.WindowedMultisampleType = (MultiSampleType)bestDeviceCombo.MultiSampleTypeList[iQuality];
      graphicsSettings.WindowedMultisampleQuality = 0;//(int)bestDeviceCombo.MultiSampleQualityList[iQuality];

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

      foreach (GraphicsAdapterInfo adapterInfo in enumerationSettings.AdapterInfoList)
      {
        adapterDesktopDisplayMode = Manager.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
        foreach (GraphicsDeviceInfo deviceInfo in adapterInfo.DeviceInfoList)
        {
          if (doesRequireHardware && deviceInfo.DevType != DeviceType.Hardware)
            continue;
          if (doesRequireReference && deviceInfo.DevType != DeviceType.Reference)
            continue;

          foreach (DeviceCombo deviceCombo in deviceInfo.DeviceComboList)
          {
            bool adapterMatchesBackBuffer = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);
            bool adapterMatchesDesktop = (deviceCombo.AdapterFormat == adapterDesktopDisplayMode.Format);
            if (deviceCombo.IsWindowed)
              continue;

            // If we haven't found a compatible set yet, or if this set
            // is better (because it's a HAL, and/or because formats match better),
            // save it
            if (bestDeviceCombo == null ||
              bestDeviceCombo.DevType != DeviceType.Hardware && deviceInfo.DevType == DeviceType.Hardware ||
              bestDeviceCombo.DevType == DeviceType.Hardware && bestDeviceCombo.AdapterFormat != adapterDesktopDisplayMode.Format && adapterMatchesDesktop ||
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
      }

      EndFullscreenDeviceComboSearch:
        if (bestDeviceCombo == null)
          return false;

      // Need to find a display mode on the best adapter that uses pBestDeviceCombo->AdapterFormat
      // and is as close to bestAdapterDesktopDisplayMode's res as possible
      bestDisplayMode.Width = 0;
      bestDisplayMode.Height = 0;
      bestDisplayMode.Format = 0;
      bestDisplayMode.RefreshRate = 0;
      foreach (DisplayMode displayMode in bestAdapterInfo.DisplayModeList)
      {
        if (displayMode.Format != bestDeviceCombo.AdapterFormat)
          continue;
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
        graphicsSettings.FullscreenDepthStencilBufferFormat = (DepthFormat)bestDeviceCombo.DepthStencilFormatList[0];
      graphicsSettings.FullscreenMultisampleType = (MultiSampleType)bestDeviceCombo.MultiSampleTypeList[0];
      graphicsSettings.FullscreenMultisampleQuality = 0;
      graphicsSettings.FullscreenVertexProcessingType = (VertexProcessingType)bestDeviceCombo.VertexProcessingTypeList[0];
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
      if (startFullscreen && foundFullscreenMode)
        graphicsSettings.IsWindowed = false;

      if (!foundFullscreenMode && !foundWindowedMode)
        throw new NoCompatibleDevicesException();

      return (foundFullscreenMode || foundWindowedMode);
    }




    /// <summary>
    /// Build presentation parameters from the current settings
    /// </summary>
    public void BuildPresentParamsFromSettings()
    {
      presentParams.Windowed = graphicsSettings.IsWindowed;
      presentParams.BackBufferCount = 2;
      presentParams.EnableAutoDepthStencil = false;
      presentParams.ForceNoMultiThreadedFlag = false;


      if (windowed)
      {
        presentParams.MultiSample = graphicsSettings.WindowedMultisampleType;
        presentParams.MultiSampleQuality = graphicsSettings.WindowedMultisampleQuality;
        presentParams.AutoDepthStencilFormat = graphicsSettings.WindowedDepthStencilBufferFormat;
        presentParams.BackBufferWidth = ourRenderTarget.ClientRectangle.Right - ourRenderTarget.ClientRectangle.Left;
        presentParams.BackBufferHeight = ourRenderTarget.ClientRectangle.Bottom - ourRenderTarget.ClientRectangle.Top;
        presentParams.BackBufferFormat = graphicsSettings.BackBufferFormat;
        presentParams.PresentationInterval = PresentInterval.Immediate;
        presentParams.FullScreenRefreshRateInHz = 0;
        presentParams.SwapEffect = Direct3D.SwapEffect.Discard;
        presentParams.PresentFlag = PresentFlag.Video;//|PresentFlag.LockableBackBuffer;
        presentParams.DeviceWindow = ourRenderTarget;
        presentParams.Windowed = true;
        //presentParams.PresentationInterval = PresentInterval.Immediate;
      }
      else
      {
        presentParams.MultiSample = graphicsSettings.FullscreenMultisampleType;
        presentParams.MultiSampleQuality = graphicsSettings.FullscreenMultisampleQuality;
        presentParams.AutoDepthStencilFormat = graphicsSettings.FullscreenDepthStencilBufferFormat;

        presentParams.BackBufferWidth = graphicsSettings.DisplayMode.Width;
        presentParams.BackBufferHeight = graphicsSettings.DisplayMode.Height;
        presentParams.BackBufferFormat = graphicsSettings.DeviceCombo.BackBufferFormat;
        presentParams.PresentationInterval = Direct3D.PresentInterval.Default;
        presentParams.FullScreenRefreshRateInHz = graphicsSettings.DisplayMode.RefreshRate;
        presentParams.SwapEffect = Direct3D.SwapEffect.Discard;
        presentParams.PresentFlag = PresentFlag.Video;//|PresentFlag.LockableBackBuffer;
        presentParams.DeviceWindow = this;
        presentParams.Windowed = false;
      }
      GUIGraphicsContext.DirectXPresentParameters=presentParams;
    }

    public bool SwitchFullScreenOrWindowed(bool bWindowed, bool bRemoveHandler)
    {
      if (!useExclusiveDirectXMode) 
        return true;
      if (bRemoveHandler)
        GUIGraphicsContext.DX9Device.DeviceReset -= new System.EventHandler(this.OnDeviceReset);
      if (bWindowed)
        Log.Write("app:Switch to windowed mode ");
      else
        Log.Write("app:Switch to fullscreen mode ");
      windowed = bWindowed;
      BuildPresentParamsFromSettings();
      try
      {
        GUIGraphicsContext.DX9Device.Reset(presentParams);

        if (bRemoveHandler)
          GUIGraphicsContext.DX9Device.DeviceReset += new System.EventHandler(this.OnDeviceReset);
        if (windowed) TopMost = alwaysOnTop;
        this.Activate();
      }
      catch (Exception ex)
      {
        if (windowed)
          Log.Write("app:Switch to windowed mode failed:{0}", ex.ToString());
        else
          Log.Write("app:Switch to fullscreen mode failed:{0}", ex.ToString());
        windowed = !bWindowed;
        BuildPresentParamsFromSettings();
        try
        {
          GUIGraphicsContext.DX9Device.Reset(presentParams);

        }
        catch (Exception) { }

        if (bRemoveHandler)
          GUIGraphicsContext.DX9Device.DeviceReset += new System.EventHandler(this.OnDeviceReset);
        this.Activate();
        return false;
      }
      if (windowed)
        Log.Write("app:Switched to windowed mode");
      else
        Log.Write("app:Switched to fullscreen mode");
      return true;
    }



    /// <summary>
    /// Initialize the graphics environment
    /// </summary>
    public void InitializeEnvironment()
    {
      GraphicsAdapterInfo adapterInfo = graphicsSettings.AdapterInfo;
      GraphicsDeviceInfo deviceInfo = graphicsSettings.DeviceInfo;

      windowed = graphicsSettings.IsWindowed;

      // Set up the presentation parameters
      BuildPresentParamsFromSettings();

      if (deviceInfo.Caps.PrimitiveMiscCaps.IsNullReference)
      {
        // Warn user about null ref device that can't render anything
        HandleSampleException(new NullReferenceDeviceException(), ApplicationMessage.None);
      }

      CreateFlags createFlags = new CreateFlags();
      if (graphicsSettings.VertexProcessingType == VertexProcessingType.Software)
        createFlags = CreateFlags.SoftwareVertexProcessing;
      else if (graphicsSettings.VertexProcessingType == VertexProcessingType.Mixed)
        createFlags = CreateFlags.MixedVertexProcessing;
      else if (graphicsSettings.VertexProcessingType == VertexProcessingType.Hardware)
        createFlags = CreateFlags.HardwareVertexProcessing;
      else if (graphicsSettings.VertexProcessingType == VertexProcessingType.PureHardware)
      {
        createFlags = CreateFlags.HardwareVertexProcessing;// | CreateFlags.PureDevice;
      }
      else
        throw new ApplicationException();

      // Make sure to allow multithreaded apps if we need them
      presentParams.ForceNoMultiThreadedFlag = !isMultiThreaded;

      try
      {
        // Create the device

        GUIGraphicsContext.DX9Device = new Device(graphicsSettings.AdapterOrdinal,
          graphicsSettings.DevType,
          windowed ? ourRenderTarget : this,
          createFlags | CreateFlags.MultiThreaded, presentParams);



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
          System.Drawing.Size currentClientSize = this.ClientSize;


          this.Size = this.ClientSize;
          this.SendToBack();
          this.BringToFront();
          this.ClientSize = currentClientSize;
          this.TopMost = alwaysOnTop;
        }

        // Store device Caps
        graphicsCaps = GUIGraphicsContext.DX9Device.DeviceCaps;
        behavior = createFlags;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

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
        {
          if (deviceInfo.DevType == DeviceType.Hardware)
            sb.Append(" (pure hw vp)");
          else
            sb.Append(" (simulated pure hw vp)");
        }
        else if (behaviorFlags.HardwareVertexProcessing)
        {
          if (deviceInfo.DevType == DeviceType.Hardware)
            sb.Append(" (hw vp)");
          else
            sb.Append(" (simulated hw vp)");
        }
        else if (behaviorFlags.MixedVertexProcessing)
        {
          if (deviceInfo.DevType == DeviceType.Hardware)
            sb.Append(" (mixed vp)");
          else
            sb.Append(" (simulated mixed vp)");
        }
        else if (behaviorFlags.SoftwareVertexProcessing)
        {
          sb.Append(" (sw vp)");
        }

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
          System.Windows.Forms.Cursor ourCursor = this.Cursor;
          GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
          GUIGraphicsContext.DX9Device.ShowCursor(true);
        }

        // Confine cursor to fullscreen window
        if (clipCursorWhenFullscreen)
        {
          if (!windowed)
          {
            System.Drawing.Rectangle rcWindow = this.ClientRectangle;
          }
        }

        // Setup the event handlers for our device
        //GUIGraphicsContext.DX9Device.DeviceLost += new System.EventHandler(this.OnDeviceLost);
        GUIGraphicsContext.DX9Device.DeviceReset += new System.EventHandler(this.OnDeviceReset);
        //GUIGraphicsContext.DX9Device.Disposing += new System.EventHandler(this.OnDeviceDisposing);
        //GUIGraphicsContext.DX9Device.DeviceResizing += new System.ComponentModel.CancelEventHandler(this.EnvironmentResized);

        // Initialize the app's device-dependent objects
        try
        {
          InitializeDeviceObjects();
          //OnDeviceReset(null, null);
          active = true;
          return;
        }
        catch (Exception ex)
        {
          Log.WriteFile(Log.LogType.Log, true, "InitializeDeviceObjects() exception:{0}", ex.ToString());
          // Cleanup before we try again
          //OnDeviceLost(null, null);
          //OnDeviceDisposing(null, null);
          GUIGraphicsContext.DX9Device.Dispose();
          GUIGraphicsContext.DX9Device = null;
          if (this.Disposing)
            return;
        }
      }
      catch (Exception)
      {
        // If that failed, fall back to the reference rasterizer
        if (deviceInfo.DevType == DeviceType.Hardware)
        {
          if (FindBestWindowedMode(false, true))
          {
            windowed = true;

            // Make sure main window isn't topmost, so error message is visible
            System.Drawing.Size currentClientSize = this.ClientSize;
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
      {
      }
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
      Log.WriteFile(Log.LogType.Log, true, "  exception: {0} {1} {2}", strMsg, strSource, strStack);
      if (ApplicationMessage.ApplicationMustExit == Type)
      {
        strMsg += "\n\nThis sample will now exit.";
        System.Windows.Forms.MessageBox.Show(strMsg, this.Text,
          System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

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

        System.Windows.Forms.MessageBox.Show(strMsg, this.Text,
          System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
      }
    }




    /// <summary>
    /// Fired when our environment was resized
    /// </summary>
    /// <param name="sender">the device that's resizing our environment</param>
    /// <param name="e">Set the cancel member to true to turn off automatic device reset</param>
    public void EnvironmentResized(object sender, System.ComponentModel.CancelEventArgs e)
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
      if ((ourRenderTarget != this) && (this.WindowState == System.Windows.Forms.FormWindowState.Minimized))
        e.Cancel = true;

      if (!isWindowActive)
        e.Cancel = true;

      // Set up the fullscreen cursor
      if (showCursorWhenFullscreen && !windowed)
      {
        System.Windows.Forms.Cursor ourCursor = this.Cursor;
        GUIGraphicsContext.DX9Device.SetCursor(ourCursor, true);
        GUIGraphicsContext.DX9Device.ShowCursor(true);
      }

    }




    /// <summary>
    /// Called when user toggles between fullscreen mode and windowed mode
    /// </summary>
    public void ToggleFullscreen()
    {
      int AdapterOrdinalOld = graphicsSettings.AdapterOrdinal;
      DeviceType DevTypeOld = graphicsSettings.DevType;

      isHandlingSizeChanges = false;
      isChangingFormStyle = true;
      ready = false;

      // Toggle the windowed state
      windowed = !windowed;

      // Save our maximized settings..
      if (!windowed && isMaximized)
        this.WindowState = System.Windows.Forms.FormWindowState.Normal;

      graphicsSettings.IsWindowed = windowed;

      // If AdapterOrdinal and DevType are the same, we can just do a Reset().
      // If they've changed, we need to do a complete device teardown/rebuild.
      if (graphicsSettings.AdapterOrdinal == AdapterOrdinalOld &&
        graphicsSettings.DevType == DevTypeOld)
      {
        BuildPresentParamsFromSettings();
        // Resize the 3D device
      RETRY:
        try
        {
          GUIGraphicsContext.DX9Device.Reset(presentParams);
        }
        catch
        {
          if (windowed)
            ForceWindowed();
          else
          {
            // Sit in a loop until the device passes Reset()
            try
            {
              GUIGraphicsContext.DX9Device.TestCooperativeLevel();
            }
            catch (DeviceNotResetException)
            {
              // Device still needs to be Reset. Try again.
              // Yield some CPU time to other processes
#if !PROFILING
              System.Threading.Thread.Sleep(100); // 100 milliseconds
#endif
              goto RETRY;
            }
          }
        }
        EnvironmentResized(GUIGraphicsContext.DX9Device, new System.ComponentModel.CancelEventArgs());

      }
      else
      {
        GUIGraphicsContext.DX9Device.Dispose();
        GUIGraphicsContext.DX9Device = null;
        InitializeEnvironment();
      }

      // When moving from fullscreen to windowed mode, it is important to
      // adjust the window size after resetting the device rather than
      // beforehand to ensure that you get the window size you want.  For
      // example, when switching from 640x480 fullscreen to windowed with
      // a 1000x600 window on a 1024x768 desktop, it is impossible to set
      // the window size to 1000x600 until after the display mode has
      // changed to 1024x768, because windows cannot be larger than the
      // desktop.

      if (windowed)
      {
        // if our render target is the main window and we haven't said 
        // ignore the menus, add our menu
        if ((ourRenderTarget == this) && (isUsingMenus))
          this.Menu = mnuMain;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        isChangingFormStyle = false;

        // We were maximized, restore that state
        if (isMaximized)
        {
          this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }
        this.SendToBack();
        this.BringToFront();
        this.ClientSize = storedSize;
        this.Location = storedLocation;
        this.TopMost = alwaysOnTop;
        //this.FormBorderStyle=FormBorderStyle.None;
      }
      else
      {
        if (this.Menu != null)
          this.Menu = null;

        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        isChangingFormStyle = false;
      }
      isHandlingSizeChanges = true;
      ready = true;
    }




    /// <summary>
    /// Switch to a windowed mode, even if that means picking a new device and/or adapter
    /// </summary>
    public void ForceWindowed()
    {
      if (windowed)
        return;

      if (!FindBestWindowedMode(false, false))
      {
        return;
      }
      windowed = true;

      // Now destroy the current 3D device objects, then reinitialize

      ready = false;

      // Release display objects, so a new device can be created
      GUIGraphicsContext.DX9Device.Dispose();
      GUIGraphicsContext.DX9Device = null;

      // Create the new device
      try
      {
        InitializeEnvironment();
      }
      catch (SampleException e)
      {
        HandleSampleException(e, ApplicationMessage.ApplicationMustExit);
      }
      catch
      {
        HandleSampleException(new SampleException(), ApplicationMessage.ApplicationMustExit);
      }
      ready = true;
    }




    /// <summary>
    /// Called when our sample has nothing else to do, and it's time to render
    /// </summary>
    protected void FullRender()
    {
      if (this.WindowState==FormWindowState.Minimized)
      {
        System.Threading.Thread.Sleep(100);
        return;						
      }

      if (GUIGraphicsContext.Vmr9Active)
      {
        HandleCursor();

        if ((System.Windows.Forms.Form.ActiveForm != this) && (alwaysOnTop))
        {
          this.Activate();
        }
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
          if ((deviceLost) || (System.Windows.Forms.Form.ActiveForm != this))
          {
            // Yield some CPU time to other processes
#if !PROFILING
            System.Threading.Thread.Sleep(100); // 100 milliseconds
#endif
          }
          Render3DEnvironment();
#if DEBUG
#else
        }
        catch (Exception ee)
        {
          System.Windows.Forms.MessageBox.Show("An exception has occurred.  This sample must exit.\r\n\r\n" + ee.ToString(), "Exception",
            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
          this.Close();
        }
#endif
      }
      else
      {
#if !PROFILING
        // if we dont got the focus, then dont use all the CPU
        if (System.Windows.Forms.Form.ActiveForm != this)
          System.Threading.Thread.Sleep(100);
#endif
      }

      HandleCursor();

      if ((System.Windows.Forms.Form.ActiveForm != this) && (alwaysOnTop))
      {
        this.Activate();
      }
    }


    void DoSleep(int sleepTime)
    {
      if (sleepTime <= 0)
        sleepTime = 5;
#if !PROFILING
      System.Threading.Thread.Sleep(sleepTime);
#endif
    }



    /// <summary>
    /// Draws the scene 
    /// </summary>
    public void Render3DEnvironment()
    {
      if (deviceLost)
      {
        try
        {
          // Test the cooperative level to see if it's okay to render
          //Log.Write("app.TestCooperativeLevel()");
          GUIGraphicsContext.DX9Device.TestCooperativeLevel();
          //Log.Write("app.TestCooperativeLevel() succeeded");
          //Log.Write("app.InitializeDeviceObjects()");
        }
        catch (DeviceLostException)
        {
          //Log.Write("app.TestCooperativeLevel()->DeviceLostException");
          // If the device was lost, do not render until we get it back
          isHandlingSizeChanges = false;
          isWindowActive = false;
          //Log.Write("app.DeviceLostException");
          return;
        }
        catch (DeviceNotResetException)
        {
          m_bNeedReset = true;
        }
        if (m_bNeedReset)
        {
          m_bNeedReset = false;
          // Check if the device needs to be resized.

          // If we are windowed, read the desktop mode and use the same format for
          // the back buffer
          //Log.Write("app.TestCooperativeLevel()->app.DeviceNotResetException");
          if (windowed)
          {
            GraphicsAdapterInfo adapterInfo = graphicsSettings.AdapterInfo;
            graphicsSettings.WindowedDisplayMode = Manager.Adapters[adapterInfo.AdapterOrdinal].CurrentDisplayMode;
            presentParams.BackBufferFormat = graphicsSettings.WindowedDisplayMode.Format;
          }

          // Reset the device and resize it

          Log.Write("app.Reset()");

          GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);

          //Log.Write("app.EnvironmentResized()");
          EnvironmentResized(GUIGraphicsContext.DX9Device, new System.ComponentModel.CancelEventArgs());
          InitializeDeviceObjects();
        }
        deviceLost = false;
        m_bNeedUpdate = true;
      }


      try
      {
        if (!GUIGraphicsContext.Vmr9Active)
        {
          Render(GUIGraphicsContext.TimePassed);
        }
      }
      catch (Exception)
      {
        //int x=1;
      }

      if (!deviceLost && !m_bNeedReset)
      {
        if (m_bRestore)
        {
          m_bRestore = false;
          if (m_bWasPlaying)
          {
            Log.Write("App.Render3dEnvironment() play:{0}", m_strCurrentFile);
            m_bWasPlaying = false;

            // If a single file was played, play only that file - don't
            // try to restore the playlist - otherwise restore the PlayListPlayer
            if (m_currentPlayList == PlayListPlayer.PlayListType.PLAYLIST_MUSIC_TEMP
              || m_currentPlayList == PlayListPlayer.PlayListType.PLAYLIST_VIDEO_TEMP)
            {
              g_Player.Play(m_strCurrentFile);
            }
            else
            {
              PlayListPlayer.Init();
              PlayListPlayer.CurrentPlaylist = m_currentPlayList;
              PlayListPlayer.Play(m_strCurrentFile);
            }
            Log.Write("App.Render3dEnvironment() play:{0}", m_strCurrentFile);
            if (g_Player.Playing)
            {
              g_Player.SeekAbsolute(m_dCurrentPos);
            }
          }
          GUIWindowManager.ActivateWindow(m_iActiveWindow);
        }
      }
    }

    public void HideCursor()
    {
      if (m_bShowCursor)
      {
        m_bShowCursor = false;
        m_bLastShowCursor = false;
        Cursor.Hide();
      }
    }

    void HandleCursor()
    {
      if (m_bAutoHideMouse)
      {
        if (m_bShowCursor != m_bLastShowCursor)
        {
          if (!m_bShowCursor)
            Cursor.Hide();
          else
            Cursor.Show();
          m_bLastShowCursor = m_bShowCursor;
        }
        if (m_bShowCursor)
        {
          TimeSpan ts = DateTime.Now - m_MouseTimeOut;
          if (ts.TotalSeconds >= 3)
          {
            //hide mouse
            m_bShowCursor = false;
            m_bNeedUpdate = true;
            Invalidate(true);
          }
        }
      }
    }

    bool ShouldUseSleepingTime()
    {
      // Render the scene as normal
      if (GUIGraphicsContext.Vmr9Active)
      {
        if (GUIGraphicsContext.Vmr9FPS > 5f)
        {
          // if we're playing a movie with vmr9 then the player will draw the GUI
          // so we just sleep 50msec here ...
          return false;
        }
      }
      //if we're playing a movie or watching tv (fullscreen)
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        return false;
      }
      return true;
    }

    int GetSleepingTime()
    {
      if (!ShouldUseSleepingTime()) return 100;
      return m_iSleepingTime;
    }


    /// <summary>
    /// Update the various statistics the simulation keeps track of
    /// </summary>
    public void UpdateStats()
    {
      // Keep track of the frame count
      float time = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);

      // Update the scene stats once per second
      if (time - lastTime >= 1.0f)
      {
        framePerSecond    = frames / (time - lastTime);
        GUIGraphicsContext.CurrentFPS=framePerSecond;
        lastTime = time;
        frames = 0;
        //				if ( !GUIGraphicsContext.Vmr9Active )
        //				{
        //					if (framePerSecond>GUIGraphicsContext.MaxFPS) m_iSleepingTime++;
        //					if (framePerSecond<GUIGraphicsContext.MaxFPS) m_iSleepingTime--;
        //					if (m_iSleepingTime<0) m_iSleepingTime=0;
        //					if (m_iSleepingTime>100) m_iSleepingTime=100;
        //				}

        string strFmt;
        Format fmtAdapter = graphicsSettings.DisplayMode.Format;
        strFmt = String.Format("backbuf {0}, adapter {1}",
          GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferFormat.ToString(), fmtAdapter.ToString());

        string strDepthFmt;
        if (enumerationSettings.AppUsesDepthBuffer)
        {
          strDepthFmt = String.Format(" ({0})",
            graphicsSettings.DepthStencilBufferFormat.ToString());
        }
        else
        {
          // No depth buffer
          strDepthFmt = "";
        }

        string strMultiSample;
        switch (graphicsSettings.MultisampleType)
        {
          case Direct3D.MultiSampleType.NonMaskable: strMultiSample = " (NonMaskable Multisample)"; break;
          case Direct3D.MultiSampleType.TwoSamples: strMultiSample = " (2x Multisample)"; break;
          case Direct3D.MultiSampleType.ThreeSamples: strMultiSample = " (3x Multisample)"; break;
          case Direct3D.MultiSampleType.FourSamples: strMultiSample = " (4x Multisample)"; break;
          case Direct3D.MultiSampleType.FiveSamples: strMultiSample = " (5x Multisample)"; break;
          case Direct3D.MultiSampleType.SixSamples: strMultiSample = " (6x Multisample)"; break;
          case Direct3D.MultiSampleType.SevenSamples: strMultiSample = " (7x Multisample)"; break;
          case Direct3D.MultiSampleType.EightSamples: strMultiSample = " (8x Multisample)"; break;
          case Direct3D.MultiSampleType.NineSamples: strMultiSample = " (9x Multisample)"; break;
          case Direct3D.MultiSampleType.TenSamples: strMultiSample = " (10x Multisample)"; break;
          case Direct3D.MultiSampleType.ElevenSamples: strMultiSample = " (11x Multisample)"; break;
          case Direct3D.MultiSampleType.TwelveSamples: strMultiSample = " (12x Multisample)"; break;
          case Direct3D.MultiSampleType.ThirteenSamples: strMultiSample = " (13x Multisample)"; break;
          case Direct3D.MultiSampleType.FourteenSamples: strMultiSample = " (14x Multisample)"; break;
          case Direct3D.MultiSampleType.FifteenSamples: strMultiSample = " (15x Multisample)"; break;
          case Direct3D.MultiSampleType.SixteenSamples: strMultiSample = " (16x Multisample)"; break;
          default: strMultiSample = string.Empty; break;
        }
        frameStats = String.Format("{0} fps ({1}x{2}), {3} {4}{5}{6} {7}",
          GUIGraphicsContext.CurrentFPS.ToString("f2"),
          GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth,
          GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight,
          GetSleepingTime(), strFmt, strDepthFmt, strMultiSample, ShouldUseSleepingTime());
        if (GUIGraphicsContext.Vmr9Active)
          frameStats += String.Format(" VMR9 {0}", GUIGraphicsContext.Vmr9FPS.ToString("f2"));
        string quality=String.Format("\nfps:{0} sync:{1} drawn:{2} dropped:{3} jitter:{4}",
          VideoRendererStatistics.AverageFrameRate.ToString("f2"),
          VideoRendererStatistics.AverageSyncOffset,
          VideoRendererStatistics.FramesDrawn,
          VideoRendererStatistics.FramesDropped,
          VideoRendererStatistics.Jitter);
        frameStats+=quality;
        //long lTotalMemory=GC.GetTotalMemory(false);
        //string memory=String.Format("\nTotal Memory allocated:{0}",Utils.GetSize(lTotalMemory) );
							
        //frameStats+=memory;
									

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
      {
        if (process.ProcessName.Equals(processName)) return;
      }

      Log.Write("App.OnSetup():stop");
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true, true);
      m_bAutoHideMouse = false;
      Cursor.Show();
      Invalidate(true);

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlreader.Clear();
      }
      Utils.StartProcess("Configuration.exe", "", false, false);
    }
    /// <summary>
    /// Prepares the simulation for a new device being selected
    /// </summary>
    public void UserSelectNewDevice(object sender, EventArgs e)
    {
      // Prompt the user to select a new device or mode
      if (active && ready)
      {
        DoSelectNewDevice();

      }
    }




    /// <summary>
    /// Displays a dialog so the user can select a new adapter, device, or
    /// display mode, and then recreates the 3D environment if needed
    /// </summary>
    private void DoSelectNewDevice()
    {
      isHandlingSizeChanges = false;
      // Can't display dialogs in fullscreen mode
      if (windowed == false)
      {
        try
        {
          ToggleFullscreen();
          isHandlingSizeChanges = false;
        }
        catch
        {
          HandleSampleException(new ResetFailedException(), ApplicationMessage.ApplicationMustExit);
          return;
        }
      }

      // Make sure the main form is in the background
      this.SendToBack();
      D3DSettingsForm settingsForm = new D3DSettingsForm(enumerationSettings, graphicsSettings);
      System.Windows.Forms.DialogResult result = settingsForm.ShowDialog(null);
      if (result != System.Windows.Forms.DialogResult.OK)
      {
        isHandlingSizeChanges = true;
        return;
      }
      graphicsSettings = settingsForm.settings;

      windowed = graphicsSettings.IsWindowed;

      // Release display objects, so a new device can be created
      GUIGraphicsContext.DX9Device.Dispose();
      GUIGraphicsContext.DX9Device = null;

      // Inform the display class of the change. It will internally
      // re-create valid surfaces, a d3ddevice, etc.
      try
      {
        InitializeEnvironment();
      }
      catch (SampleException d3de)
      {
        HandleSampleException(d3de, ApplicationMessage.ApplicationMustExit);
      }
      catch
      {
        HandleSampleException(new SampleException(), ApplicationMessage.ApplicationMustExit);
      }

      isHandlingSizeChanges = true;
    }








    /// <summary>
    /// Will end the simulation
    /// </summary>
    private void ExitSample(object sender, EventArgs e)
    {
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
      if (notifyIcon1 != null)
        this.notifyIcon1.Dispose(); //we dispose our tray icon here
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
    protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
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
      {
        base.OnKeyPress(e);
      }
    }


    public virtual void OnTimer()
    {
    }
    private void timer1_Tick(object sender, System.EventArgs e)
    {
      OnTimer();
    }

    private void menuItem2_Click(object sender, System.EventArgs e)
    {
      OnSetup(sender, e);
    }


    private void D3DApp_Load(object sender, System.EventArgs e)
    {
      Initialize();
      OnStartup();

    }

    private void D3DApp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
      g_Player.Stop();
    }

    private void D3DApp_Resize(object sender, System.EventArgs e)
    {
      if (g_Player.Playing)
      {
        Log.Write("Form resized: stop media");
        m_bRestore = true;
        m_bWasPlaying = g_Player.Playing;
        m_dCurrentPos = g_Player.CurrentPosition;
        m_currentPlayList = PlayListPlayer.CurrentPlaylist;
        m_strCurrentFile = PlayListPlayer.Get(PlayListPlayer.CurrentSong);
        m_iActiveWindow = GUIWindowManager.ActiveWindow;
        g_Player.Stop();
      }
    }



    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      /*
            if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
             return;
            try
            {
              FrameMove();
        
              FullRender();
              int SleepingTime=GetSleepingTime();
              DoSleep(SleepingTime);

              HandleCursor();
            }
            catch(Exception)
            {
            }
            this.Invalidate();*/
    }


    private void D3DApp_Click(object sender, MouseEventArgs e)
    {
      if (System.Windows.Forms.Form.ActiveForm != this) return;
      mouseclick(e);
    }

    private void D3DApp_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      if (System.Windows.Forms.Form.ActiveForm != this) return;
      mousemove(e);
    }


    protected void ToggleFullWindowed()
    {
      Log.Write("App.ToggleFullWindowed()");
      if (g_Player.Playing)
      {
        Log.Write("App.ToggleFullWindowed() stop media");
        m_bRestore = true;
        m_bWasPlaying = g_Player.Playing;
        m_dCurrentPos = g_Player.CurrentPosition;
        m_currentPlayList = PlayListPlayer.CurrentPlaylist;
        m_strCurrentFile = PlayListPlayer.Get(PlayListPlayer.CurrentSong);
        m_iActiveWindow = GUIWindowManager.ActiveWindow;
        g_Player.Stop();
      }

      isMaximized = !isMaximized;
      GUIGraphicsContext.DX9Device.DeviceReset -= new System.EventHandler(this.OnDeviceReset);
      if (isMaximized)
      {
        Log.Write("windowed->fullscreen");
        if (autoHideTaskbar)
        {
          Win32API.EnableStartBar(false);
          Win32API.ShowStartBar(false);
        }
        this.FormBorderStyle = FormBorderStyle.None;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Menu = null;
        this.Location = new System.Drawing.Point(0, 0);
        this.Bounds = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        this.ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        this.Update();
        GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth = Screen.PrimaryScreen.Bounds.Width;
        GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight = Screen.PrimaryScreen.Bounds.Height;
        Log.Write("windowed->fullscreen done {0}", isMaximized);
        Log.Write("ClientSize: {0}x{1} screen:{2}x{3}",
          this.ClientSize.Width, this.ClientSize.Height,
          Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        SwitchFullScreenOrWindowed(windowed, false);
      }
      else
      {
        Log.Write("fullscreen->windowed");
        if (autoHideTaskbar)
        {
          Win32API.EnableStartBar(true);
          Win32API.ShowStartBar(true);
        }

        this.WindowState = FormWindowState.Normal;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = true;
        this.Menu = mnuMain;
        this.Location = storedLocation;
        Bounds = new Rectangle(oldBounds.X, oldBounds.Y, oldBounds.Width, oldBounds.Height);
        this.ClientSize = storedSize;
        Log.Write("fullscreen->windowed done {0}", isMaximized);
        Log.Write("ClientSize: {0}x{1} screen:{2}x{3}",
          this.ClientSize.Width, this.ClientSize.Height,
          Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        SwitchFullScreenOrWindowed(windowed, false);
      }
      GUIGraphicsContext.DX9Device.DeviceReset += new System.EventHandler(this.OnDeviceReset);
      OnDeviceReset(null, null);
    }

    /// <summary>
    /// Handle system keystrokes (ie, alt-enter)
    /// </summary>
    protected void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {


      if (e.Control == false && e.Alt == true && (e.KeyCode == System.Windows.Forms.Keys.Return))
      {
        ToggleFullWindowed();
        e.Handled = true;
        return;

        /*
                // Toggle the fullscreen/window mode
                if (active && ready)
                {

                  try
                  {
                    ToggleFullscreen();                    
                    return;
                  }
                  catch
                  {
                    HandleSampleException(new ResetFailedException(), ApplicationMessage.ApplicationMustExit);
                  }
                  finally
                  {
                    e.Handled = true;
                  }
                }*/
      }
      else if (e.KeyCode == System.Windows.Forms.Keys.F2)
      {
        OnSetup(null, null);
      }

      if (e.Handled == false)
      {
        keydown(e);
      }
    }




    /// <summary>
    /// Winforms generated code for initializing the form
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(D3DApp));
      this.mnuMain = new System.Windows.Forms.MainMenu();
      this.mnuFile = new System.Windows.Forms.MenuItem();
      this.mnuChange = new System.Windows.Forms.MenuItem();
      this.mnuBreak2 = new System.Windows.Forms.MenuItem();
      this.mnuExit = new System.Windows.Forms.MenuItem();
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.menuItemFullscreen = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.menuItem4 = new System.Windows.Forms.MenuItem();
      this.dvdMenuItem = new System.Windows.Forms.MenuItem();
      this.moviesMenuItem = new System.Windows.Forms.MenuItem();
      this.musicMenuItem = new System.Windows.Forms.MenuItem();
      this.picturesMenuItem = new System.Windows.Forms.MenuItem();
      this.televisionMenuItem = new System.Windows.Forms.MenuItem();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
      this.contextMenu1 = new System.Windows.Forms.ContextMenu();
      this.menuItem3 = new System.Windows.Forms.MenuItem();
      this.menuItem5 = new System.Windows.Forms.MenuItem();
      // 
      // mnuMain
      // 
      this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                            this.mnuFile,
                                                                            this.menuItem1,
                                                                            this.menuItem4});
      // 
      // mnuFile
      // 
      this.mnuFile.Index = 0;
      this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                            this.mnuChange,
                                                                            this.mnuBreak2,
                                                                            this.mnuExit});
      this.mnuFile.Text = "&File";
      // 
      // mnuChange
      // 
      this.mnuChange.Index = 0;
      this.mnuChange.Text = "&Change Device...";
      this.mnuChange.Click += new System.EventHandler(this.UserSelectNewDevice);
      // 
      // mnuBreak2
      // 
      this.mnuBreak2.Index = 1;
      this.mnuBreak2.Text = "-";
      // 
      // mnuExit
      // 
      this.mnuExit.Index = 2;
      this.mnuExit.Text = "Exit";
      this.mnuExit.Click += new System.EventHandler(this.ExitSample);
      // 
      // menuItem1
      // 
      this.menuItem1.Index = 1;
      this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.menuItemFullscreen,
                                                                              this.menuItem2});
      this.menuItem1.Text = "&Options";
      // 
      // menuItemFullscreen
      // 
      this.menuItemFullscreen.Index = 0;
      this.menuItemFullscreen.Text = "&Fullscreen";
      this.menuItemFullscreen.Click += new System.EventHandler(this.menuItemFullscreen_Click);
      // 
      // menuItem2
      // 
      this.menuItem2.Index = 1;
      this.menuItem2.Shortcut = System.Windows.Forms.Shortcut.F2;
      this.menuItem2.Text = "&Configuration...";
      this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
      // 
      // menuItem4
      // 
      this.menuItem4.Index = 2;
      this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.dvdMenuItem,
                                                                              this.moviesMenuItem,
                                                                              this.musicMenuItem,
                                                                              this.picturesMenuItem,
                                                                              this.televisionMenuItem});
      this.menuItem4.Text = "Wizards";
      // 
      // dvdMenuItem
      // 
      this.dvdMenuItem.Index = 0;
      this.dvdMenuItem.Text = "DVD";
      this.dvdMenuItem.Click += new System.EventHandler(this.dvdMenuItem_Click);
      // 
      // moviesMenuItem
      // 
      this.moviesMenuItem.Index = 1;
      this.moviesMenuItem.Text = "Movies";
      this.moviesMenuItem.Click += new System.EventHandler(this.moviesMenuItem_Click);
      // 
      // musicMenuItem
      // 
      this.musicMenuItem.Index = 2;
      this.musicMenuItem.Text = "Music";
      this.musicMenuItem.Click += new System.EventHandler(this.musicMenuItem_Click);
      // 
      // picturesMenuItem
      // 
      this.picturesMenuItem.Index = 3;
      this.picturesMenuItem.Text = "Pictures";
      this.picturesMenuItem.Click += new System.EventHandler(this.picturesMenuItem_Click);
      // 
      // televisionMenuItem
      // 
      this.televisionMenuItem.Index = 4;
      this.televisionMenuItem.Text = "Television";
      this.televisionMenuItem.Click += new System.EventHandler(this.televisionMenuItem_Click);
      // 
      // timer1
      // 
      this.timer1.Enabled = true;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // notifyIcon1
      // 
      this.notifyIcon1.ContextMenu = this.contextMenu1;
      this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
      this.notifyIcon1.Text = "MediaPortal";
      this.notifyIcon1.DoubleClick += new System.EventHandler(this.Restore_OnClick);
      // 
      // contextMenu1
      // 
      this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                 this.menuItem3});
      this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
      // 
      // menuItem3
      // 
      this.menuItem3.Index = 0;
      this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.menuItem5});
      this.menuItem3.Text = "";
      // 
      // menuItem5
      // 
      this.menuItem5.Index = 0;
      this.menuItem5.Text = "";
      // 
      // D3DApp
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(720, 576);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.MinimumSize = new System.Drawing.Size(100, 100);
      this.Name = "D3DApp";
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
      this.Resize += new System.EventHandler(this.D3DApp_Resize);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.D3DApp_Click);
      this.Closing += new System.ComponentModel.CancelEventHandler(this.D3DApp_Closing);
      this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
      this.Load += new System.EventHandler(this.D3DApp_Load);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.D3DApp_MouseMove);

    }








    /// <summary>
    /// Make sure our graphics cursor (if available) moves with the cursor
    /// </summary>
    protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
    {
      m_MouseTimeOut = DateTime.Now;
      if ((GUIGraphicsContext.DX9Device != null) && (!GUIGraphicsContext.DX9Device.Disposed))
      {
        // Move the D3D cursor
        GUIGraphicsContext.DX9Device.SetCursorPosition(e.X, e.Y, false);
      }
      // Let the control handle the mouse now
      base.OnMouseMove(e);
    }




    /// <summary>
    /// Handle size changed events
    /// </summary>
    protected override void OnSizeChanged(System.EventArgs e)
    {
      this.OnResize(e);
      base.OnSizeChanged(e);
    }




    /// <summary>
    /// Handle resize events
    /// </summary>
    protected override void OnResize(System.EventArgs e)
    {
      if (notifyIcon1 != null)
      {
        if (notifyIcon1.Visible == false && this.WindowState == FormWindowState.Minimized)
        {
          notifyIcon1.Visible = true;
          this.Hide();
          return;
        }
        else if (notifyIcon1.Visible == true && this.WindowState != FormWindowState.Minimized)
        {
          notifyIcon1.Visible = false;
        }
      }

      active = !(this.WindowState == System.Windows.Forms.FormWindowState.Minimized);
      base.OnResize(e);
    }




    /// <summary>
    /// Once the form has focus again, we can continue to handle our resize
    /// and resets..
    /// </summary>
    protected override void OnGotFocus(System.EventArgs e)
    {
      isHandlingSizeChanges = true;
      isWindowActive = true;
      base.OnGotFocus(e);
    }




    /// <summary>
    /// Handle move events
    /// </summary>
    protected override void OnMove(System.EventArgs e)
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
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      if (autoHideTaskbar)
      {
        Win32API.EnableStartBar(true);
        Win32API.ShowStartBar(true);
      }
      isClosing = true;
      base.OnClosing(e);
    }
    #endregion



    protected virtual void keypressed(System.Windows.Forms.KeyPressEventArgs e)
    {
    }
    protected virtual void keydown(System.Windows.Forms.KeyEventArgs e)
    {
    }
    protected virtual void mousemove(System.Windows.Forms.MouseEventArgs e)
    {
      if (lastx == -1 || lasty == -1)
      {
        lastx = e.X;
        lasty = e.Y;
      }
      else if (lastx != e.X || lasty != e.Y)
      {
        //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,lastx,lasty);
        lastx = e.X;
        lasty = e.Y;
        System.Windows.Forms.Cursor ourCursor = this.Cursor;
        if (!m_bShowCursor)
        {
          m_bShowCursor = true;
          m_bNeedUpdate = true;
          Invalidate(true);
        }
        m_MouseTimeOut = DateTime.Now;
      }
    }
    protected virtual void mouseclick(MouseEventArgs e)
    {
      //this.Text=String.Format("show click");
      System.Windows.Forms.Cursor ourCursor = this.Cursor;
      if (!m_bShowCursor)
      {
        m_bShowCursor = true;
        m_bNeedUpdate = true;
        Invalidate(true);
      }
      m_MouseTimeOut = DateTime.Now;
    }

    private void OnKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      keypressed(e);
    }


    void CreateDirectDrawSurface()
    {
      m_dddescription = new DirectDraw.SurfaceDescription();
      m_dddevice = new DirectDraw.Device();
      m_dddevice.SetCooperativeLevel(this, DirectDraw.CooperativeLevelFlags.Exclusive);
      m_dddevice.SetDisplayMode(presentParams.BackBufferWidth, presentParams.BackBufferHeight, 32, 0, false);

      m_dddescription.SurfaceCaps.PrimarySurface = true;
      m_dddescription.SurfaceCaps.Flip = true;
      m_dddescription.SurfaceCaps.Complex = true;
      m_dddescription.BackBufferCount = 1;
      m_ddfront = new DirectDraw.Surface(m_dddescription, m_dddevice);
      DirectDraw.SurfaceCaps caps = new DirectDraw.SurfaceCaps();
      // Yes, we are using a back buffer
      caps.BackBuffer = true;

      // Associate the front buffer to back buffer with specified caps
      m_ddback = m_ddfront.GetAttachedSurface(caps);

    }

    private void televisionMenuItem_Click(object sender, System.EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true, true);

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlreader.Clear();
      }
      System.Diagnostics.Process.Start("configuration.exe", @"/wizard /section=wizards\television.xml");
    }

    private void picturesMenuItem_Click(object sender, System.EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true, true);
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlreader.Clear();
      }
      System.Diagnostics.Process.Start("configuration.exe", @"/wizard /section=wizards\pictures.xml");
    }

    private void musicMenuItem_Click(object sender, System.EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true, true);
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlreader.Clear();
      }
      System.Diagnostics.Process.Start("configuration.exe", @"/wizard /section=wizards\music.xml");
    }

    private void moviesMenuItem_Click(object sender, System.EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true, true);
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlreader.Clear();
      }

      System.Diagnostics.Process.Start("configuration.exe", @"/wizard /section=wizards\movies.xml");
    }

    private void dvdMenuItem_Click(object sender, System.EventArgs e)
    {
      g_Player.Stop();
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false)
        SwitchFullScreenOrWindowed(true, true);
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlreader.Clear();
      }
      System.Diagnostics.Process.Start("configuration.exe", @"/wizard /section=wizards\dvd.xml");
    }

		public void HandleMessage()
		{
			try
			{
				NativeGameLoop.MSG msg1;
				msg1 = new NativeGameLoop.MSG();

				if (!NativeGameLoop.PeekMessage(ref msg1, IntPtr.Zero, 0, 0, 0))
				{
					return;
				}
				if (!NativeGameLoop.GetMessageA(ref msg1, IntPtr.Zero, 0, 0))
				{
					return;
				}
				NativeGameLoop.TranslateMessage(ref msg1);
				NativeGameLoop.DispatchMessageA(ref msg1);
				//	System.Windows.Forms.Application.DoEvents();//SLOW
			}
#if DEBUG
      catch (Exception ex)
      {
          Log.Write("exception:{0}",ex.ToString());
#else
			catch (Exception)
			{
#endif
			}
		}

    void StartFrameClock()
    {
      NativeMethods.QueryPerformanceCounter(ref startFrame);
    }

    void WaitForFrameClock()
    {
      long milliSecondsLeft;
      long timeElapsed = 0;

      // frame limiting code.
      // sleep as long as there are ticks left for this frame
      NativeMethods.QueryPerformanceCounter(ref endFrame);
      timeElapsed = endFrame - startFrame;
      while(timeElapsed < GUIGraphicsContext.DesiredFrameTime)
      {
        milliSecondsLeft =(((GUIGraphicsContext.DesiredFrameTime - timeElapsed)*1000) / DXUtil.TicksPerSecond);
        if(milliSecondsLeft > 2) 
          DoSleep(1);
        NativeMethods.QueryPerformanceCounter(ref endFrame);
        timeElapsed = endFrame - startFrame;
      }
    }
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
        catch (Exception ex)
        {
          Log.WriteFile(Log.LogType.Log, true, "exception:{0}", ex.ToString());
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

    private void contextMenu1_Popup(object sender, System.EventArgs e)
    {

      contextMenu1.MenuItems.Clear();

      // Add a menu item 
      contextMenu1.MenuItems.Add("Restore", new System.EventHandler(this.Restore_OnClick));
      contextMenu1.MenuItems.Add("Exit", new System.EventHandler(this.Exit_OnClick));
    }

    protected void Restore_OnClick(System.Object sender, System.EventArgs e)
    {
      Show();
      notifyIcon1.Visible = false;
      this.WindowState = FormWindowState.Normal;
      active = true;
    }
    protected void Exit_OnClick(System.Object sender, System.EventArgs e)
    {
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
    }


    private void menuItemFullscreen_Click(object sender, System.EventArgs e)
    {
      ToggleFullWindowed();

      GUIDialogNotify dialogNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (dialogNotify != null)
      {
        dialogNotify.SetHeading(1020);
        dialogNotify.SetText(String.Format("{0}\n{1}", GUILocalizeStrings.Get(1021), GUILocalizeStrings.Get(1022)));
        dialogNotify.TimeOut = 6;
        dialogNotify.SetImage(String.Format(@"{0}\media\{1}", GUIGraphicsContext.Skin, "dialog_information.png"));
        dialogNotify.DoModal(GUIWindowManager.ActiveWindow);
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
  };
  #endregion




  #region Various SampleExceptions
  /// <summary>
  /// The default sample exception type
  /// </summary>
  public class SampleException : System.ApplicationException
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
        strMsg += "installed.  Install the DirectX SDK to get the full\n";
        strMsg += "reference device.\n";

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
      Caption = 0x00C00000,     /* WindowStyles.Border | WindowStyles.DialogFrame  */
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
      public System.Drawing.Point p;
    }

    /// <summary>MinMax Info structure</summary>
    [StructLayout(LayoutKind.Sequential)]
      public struct MinMaxInformation
    {
      public System.Drawing.Point reserved;
      public System.Drawing.Point MaxSize;
      public System.Drawing.Point MaxPosition;
      public System.Drawing.Point MinTrackSize;
      public System.Drawing.Point MaxTrackSize;
    }

    /// <summary>Monitor Info structure</summary>
    [StructLayout(LayoutKind.Sequential)]
      public struct MonitorInformation
    {
      public uint Size; // Size of this structure
      public System.Drawing.Rectangle MonitorRectangle;
      public System.Drawing.Rectangle WorkRectangle;
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
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam);
    #endregion

    #region Windows API calls
    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [System.Runtime.InteropServices.DllImport("winmm.dll")]
    public static extern IntPtr timeBeginPeriod(uint period);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, PeekMessageFlags flags);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool TranslateMessage(ref Message msg);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool DispatchMessage(ref Message msg);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern void PostQuitMessage(int exitCode);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
#if(_WIN64)
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int index, [MarshalAs(UnmanagedType.FunctionPtr)] WndProcDelegate windowCallback);
#else
    private static extern IntPtr SetWindowLong(IntPtr hWnd, int index, [MarshalAs(UnmanagedType.FunctionPtr)] WndProcDelegate windowCallback);
#endif

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLongStyle(IntPtr hWnd, int index, WindowStyles style);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
    private static extern WindowStyles GetWindowLongStyle(IntPtr hWnd, int index);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("kernel32")]
    public static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("kernel32")]
    public static extern bool QueryPerformanceCounter(ref long PerformanceCount);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetClientRect(IntPtr hWnd, out System.Drawing.Rectangle rect);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetWindowRect(IntPtr hWnd, out System.Drawing.Rectangle rect);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int x, int y, int w, int h, uint flags);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ScreenToClient(IntPtr hWnd, ref System.Drawing.Point rect);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hWnd, ref MonitorInformation info);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint flags);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern short GetAsyncKeyState(uint key);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SetCapture(IntPtr handle);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ReleaseCapture();

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool ShowWindow(IntPtr hWnd, ShowWindowFlags flags);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool SetMenu(IntPtr hWnd, IntPtr menuHandle);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool IsIconic(IntPtr hWnd);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool AdjustWindowRect(ref System.Drawing.Rectangle rect, WindowStyles style,
      [MarshalAs(UnmanagedType.Bool)]bool menu);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr windowHandle, WindowMessage msg, IntPtr w, IntPtr l);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr RegisterClass(ref WindowClass wndClass);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] string className, IntPtr instanceHandle);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateWindow(int exStyle, [MarshalAs(UnmanagedType.LPTStr)] string className, [MarshalAs(UnmanagedType.LPTStr)] string windowName,
      WindowStyles style, int x, int y, int width, int height, IntPtr parent, IntPtr menuHandle, IntPtr instanceHandle, IntPtr zero);

    [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int GetCaretBlinkTime();
    #endregion

    #region Class Methods
    private NativeMethods() { } // No creation
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
    public static bool IsKeyDown(System.Windows.Forms.Keys key)
    {
      return (GetAsyncKeyState((int)System.Windows.Forms.Keys.ShiftKey) & 0x8000) != 0;
    }
    #endregion
  }

  #endregion
};