using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DirectDraw=Microsoft.DirectX.DirectDraw;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;

namespace MediaPortal
{
  /// <summary>
  /// The base class for all the graphics (D3D) samples, it derives from windows forms
  /// </summary>
  public class D3DApp : System.Windows.Forms.Form
  {
    protected string    m_strSkin="CrystalCenter";
    protected string    m_strLanguage="english";

    #region Menu Information
    // The menu items that *all* samples will need
    protected System.Windows.Forms.MainMenu mnuMain;
    protected System.Windows.Forms.MenuItem mnuFile;
    private System.Windows.Forms.MenuItem mnuChange;
    private System.Windows.Forms.MenuItem mnuBreak2;
    protected System.Windows.Forms.MenuItem mnuExit;
    #endregion

    protected bool m_bAutoHideMouse=false;
    protected bool m_bNeedUpdate=false;
    protected DateTime  m_MouseTimeOut=DateTime.Now;
    // The window we will render too
    private System.Windows.Forms.Control ourRenderTarget;
    // Should we use the default windows
    protected bool isUsingMenus = true;
    private float lastTime = 0.0f; // The last time
    private int frames = 0; // Number of frames since our last update

    // We need to keep track of our enumeration settings
    protected D3DEnumeration enumerationSettings = new D3DEnumeration();
    protected D3DSettings graphicsSettings = new D3DSettings();
    private bool isMaximized = false; // Are we maximized?
    private bool isHandlingSizeChanges = true; // Are we handling size changes?
    private bool isClosing = false; // Are we closing?
    private bool isChangingFormStyle = false; // Are we changing the forms style?
    private bool isWindowActive = true; // Are we waiting for got focus?
    protected bool m_bShowCursor=true;

    static int lastx=0;
    static int lasty=0;
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
    protected float framePerSecond;              // Instanteous frame rate
    protected string deviceStats;// String to hold D3D device stats
    protected string frameStats; // String to hold frame stats

    protected bool deviceLost = false;
    protected bool m_bNeedReset=false;
    // Overridable variables for the app
    private int minDepthBits;    // Minimum number of bits needed in depth buffer
    protected int MinDepthBits { get { return minDepthBits; } set { minDepthBits = value;  enumerationSettings.AppMinDepthBits = value;} }
    private int minStencilBits;  // Minimum number of bits needed in stencil buffer
    protected int MinStencilBits { get { return minStencilBits; } set { minStencilBits = value;  enumerationSettings.AppMinStencilBits = value;} }
    protected bool showCursorWhenFullscreen; // Whether to show cursor when fullscreen
    protected bool clipCursorWhenFullscreen; // Whether to limit cursor pos when fullscreen
    protected bool startFullscreen; // Whether to start up the app in fullscreen mode

    protected System.Drawing.Size storedSize;
    protected System.Drawing.Point storedLocation;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MenuItem menuItem2;
    private System.Windows.Forms.Timer timer1;
    private System.ComponentModel.IContainer components;
    private System.Windows.Forms.MenuItem menuItem3;
    protected Rectangle   oldBounds;

    // Overridable functions for the 3D scene created by the app
    protected virtual bool ConfirmDevice(Caps caps, VertexProcessingType vertexProcessingType, 
      Format adapterFormat, Format backBufferFormat) { return true; }
    protected virtual void OneTimeSceneInitialization() { /* Do Nothing */ }
    protected virtual void Initialize() { /* Do Nothing */ }
    protected virtual void InitializeDeviceObjects() { /* Do Nothing */ }
    protected virtual void OnDeviceReset(System.Object sender, System.EventArgs e) { /* Do Nothing */ }
    protected virtual void FrameMove() { /* Do Nothing */ }
    protected virtual void Render() { /* Do Nothing */ }
    protected virtual void OnDeviceLost(System.Object sender, System.EventArgs e) { /* Do Nothing */ }
    protected virtual void OnDeviceDisposing(System.Object sender, System.EventArgs e) { /* Do Nothing */ }

    protected virtual void OnStartup() {}
    protected virtual void OnExit() {}


    bool   m_bWasPlaying=false;
    string m_strCurrentFile="";
    int    m_iActiveWindow=-1;
		bool   m_bRestore=false;
    double m_dCurrentPos=0;

    DirectDraw.Device               m_dddevice=null;
    DirectDraw.SurfaceDescription   m_dddescription=null;
    DirectDraw.Surface              m_ddfront=null;
    DirectDraw.Surface              m_ddback=null;
    /// <summary>
    /// Constructor
    /// </summary>
    public D3DApp()
    {
      //GUIGraphicsContext.DX9Device = null;
      active = false;
      ready = false;
      hasFocus = false;
      behavior = 0;

      ourRenderTarget = this;
      frameMoving = true;
      singleStep = false;
      framePerSecond = 0.0f;
      deviceStats = null;
      frameStats  = null;

      this.Text = "D3D9 Sample";
      this.ClientSize = new System.Drawing.Size(720,576);
      this.KeyPreview = true;

      minDepthBits = 16;
      minStencilBits = 0;
      showCursorWhenFullscreen = false;
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
//@@@fullscreen
/*
       string strStartFull=(string)xmlreader.GetValue("general","startfullscreen");
        if (strStartFull!=null && strStartFull=="yes") startFullscreen=true;
*/        
//@@@fullscreen
      }
      //      startFullscreen=true;
      // When clipCursorWhenFullscreen is TRUE, the cursor is limited to
      // the device window when the app goes fullscreen.  This prevents users
      // from accidentally clicking outside the app window on a multimon system.
      // This flag is turned off by default for debug builds, since it makes 
      // multimon debugging difficult.
      clipCursorWhenFullscreen = false;
      InitializeComponent();
      this.timer1.Interval=300;
      this.timer1.Start();

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
      int iQuality=0;//bestDeviceCombo.MultiSampleTypeList.Count-1;
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
      presentParams.BackBufferCount = 1;
      presentParams.MultiSample = graphicsSettings.MultisampleType;
      presentParams.MultiSampleQuality = graphicsSettings.MultisampleQuality;
      presentParams.EnableAutoDepthStencil = enumerationSettings.AppUsesDepthBuffer;
      presentParams.AutoDepthStencilFormat = graphicsSettings.DepthStencilBufferFormat;
      

      Caps caps=graphicsSettings.DeviceInfo.Caps;
        if (windowed)
        {
            presentParams.EnableAutoDepthStencil = false;
            presentParams.AutoDepthStencilFormat = DepthFormat.D16;
            presentParams.BackBufferWidth  = ourRenderTarget.ClientRectangle.Right - ourRenderTarget.ClientRectangle.Left;
            presentParams.BackBufferHeight = ourRenderTarget.ClientRectangle.Bottom - ourRenderTarget.ClientRectangle.Top;
            presentParams.BackBufferFormat = Format.A8R8G8B8;//graphicsSettings.DeviceCombo.BackBufferFormat;
            presentParams.MultiSample = graphicsSettings.WindowedMultisampleType;
            presentParams.MultiSampleQuality = graphicsSettings.WindowedMultisampleQuality;

            presentParams.PresentationInterval = PresentInterval.One;
           
            presentParams.BackBufferCount = 1;
            presentParams.FullScreenRefreshRateInHz = 0;
            presentParams.SwapEffect=Direct3D.SwapEffect.Copy;
            presentParams.PresentFlag = PresentFlag.Video;
            presentParams.DeviceWindow = ourRenderTarget;
        }
        else
        {
            presentParams.BackBufferWidth  = graphicsSettings.DisplayMode.Width;
            presentParams.BackBufferHeight = graphicsSettings.DisplayMode.Height;
            presentParams.BackBufferFormat = graphicsSettings.DeviceCombo.BackBufferFormat;
            presentParams.FullScreenRefreshRateInHz = graphicsSettings.DisplayMode.RefreshRate;
            presentParams.PresentationInterval = Direct3D.PresentInterval.Default;
            presentParams.SwapEffect=Direct3D.SwapEffect.Flip;
            presentParams.PresentFlag = PresentFlag.Video;
            presentParams.DeviceWindow = this;
      }
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

        GUIGraphicsContext.DX9Device = new Device(graphicsSettings.AdapterOrdinal, graphicsSettings.DevType, 
          windowed ? ourRenderTarget : this , createFlags| CreateFlags.MultiThreaded, presentParams);



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
        GUIGraphicsContext.DX9Device.DeviceLost += new System.EventHandler(this.OnDeviceLost);
        GUIGraphicsContext.DX9Device.DeviceReset += new System.EventHandler(this.OnDeviceReset);
        GUIGraphicsContext.DX9Device.Disposing += new System.EventHandler(this.OnDeviceDisposing);
        GUIGraphicsContext.DX9Device.DeviceResizing += new System.ComponentModel.CancelEventHandler(this.EnvironmentResized);

        // Initialize the app's device-dependent objects
        try
        {
          InitializeDeviceObjects();
          //OnDeviceReset(null, null);
          active = true;
					GC.Collect();
					GC.Collect();
					GC.Collect();
          return;
        }
        catch (Exception )
        {
          // Cleanup before we try again
          OnDeviceLost(null, null);
          OnDeviceDisposing(null, null);
          GUIGraphicsContext.DX9Device.Dispose();
          GUIGraphicsContext.DX9Device = null;
          if (this.Disposing)
            return;
        }
      }
      catch (Exception )
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
      // Build a message to display to the user
      string strMsg = "";
      string strSource = "";
      string strStack = "";
      if (e != null)
      {
        strMsg = e.Message;
        strSource=e.Source;
        strStack=e.StackTrace;
      }
      Log.Write("  exception: {0} {1} {2}", strMsg,strSource,strStack);
      if (ApplicationMessage.ApplicationMustExit == Type)
      {
        strMsg  += "\n\nThis sample will now exit.";
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
            catch(DeviceNotResetException)
            {
              // Device still needs to be Reset. Try again.
              // Yield some CPU time to other processes
              System.Threading.Thread.Sleep(100); // 100 milliseconds
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
        HandleSampleException(e,ApplicationMessage.ApplicationMustExit);
      }
      catch
      {
        HandleSampleException(new SampleException(),ApplicationMessage.ApplicationMustExit);
      }
      ready = true;
    }




    /// <summary>
    /// Called when our sample has nothing else to do, and it's time to render
    /// </summary>
    protected void FullRender()
    {
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
            System.Threading.Thread.Sleep(100); // 100 milliseconds
          }
          // Render a frame during idle time
          if (active)
          {
            Render3DEnvironment();
          }
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
        // if we dont got the focus, then dont use all the CPU
        if (System.Windows.Forms.Form.ActiveForm != this)
          System.Threading.Thread.Sleep(100);
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
      Initialize() ;

      storedSize=this.ClientSize;
      storedLocation=this.Location ;
      oldBounds=new Rectangle(Bounds.X,Bounds.Y,Bounds.Width,Bounds.Height);

//@@@fullscreen

      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strStartFull=(string)xmlreader.GetValue("general","startfullscreen");
        if (strStartFull!=null && strStartFull=="yes")
        {
          Win32API.EnableStartBar(false);
          Win32API.ShowStartBar(false);

          Log.Write("start fullscreen");
          this.FormBorderStyle=FormBorderStyle.None;
          this.MaximizeBox=false;
          this.MinimizeBox=false;
          this.Menu=null;
          this.Location= new System.Drawing.Point(0,0);
          this.Bounds=new Rectangle(0,0,Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
          this.ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
          GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth=Screen.PrimaryScreen.Bounds.Width;
          GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight=Screen.PrimaryScreen.Bounds.Height;
          Log.Write("ClientSize: {0}x{1} screen:{2}x{3}",
                    this.ClientSize.Width,this.ClientSize .Height,
                    Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);

          m_bNeedReset=true;
          deviceLost=true;
          isMaximized=true;
        }
      }
 
//@@@fullscreen
      OnStartup();

      
      while (mainWindow.Created && GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
      {
            
        try
        {
          FrameMove();
        }
        catch (Exception ex)
        {
          Log.Write("Exception: {0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
#if DEBUG
          throw new Exception("exception occured",ex);
#endif
        }
        System.Windows.Forms.Application.DoEvents();
        FullRender();
        System.Windows.Forms.Application.DoEvents();
        
        if (m_bAutoHideMouse)
        {
          if (!m_bShowCursor) 
            Cursor.Hide();
          else 
            Cursor.Show();
          if (m_bShowCursor)
          {
            TimeSpan ts=DateTime.Now-m_MouseTimeOut;
            if (ts.TotalSeconds>=3)
            {
              //hide mouse
              m_bShowCursor=false;
              m_bNeedUpdate=true;
              m_MouseTimeOut=DateTime.Now;
              Invalidate(true);
            }
          }
        }
      }
      OnExit();
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
          m_bNeedReset=true;
        }
        if (m_bNeedReset)
        {
          m_bNeedReset=false;
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
        m_bNeedUpdate=true;
      }
      // Render the scene as normal
      
      if (g_Player.Playing&& g_Player.DoesOwnRendering) 
      {
        AllocatorWrapper.Allocator.RenderVideo();
        System.Windows.Forms.Application.DoEvents();
        //System.Threading.Thread.Sleep(50);
        //System.Windows.Forms.Application.DoEvents();
      }
      else
      {
        System.Windows.Forms.Application.DoEvents();
        if (GUIGraphicsContext.IsFullScreenVideo && g_Player.Playing && g_Player.HasVideo)
        {
          System.Threading.Thread.Sleep(25);  
        }
        System.Windows.Forms.Application.DoEvents();

        try
        {
          System.Windows.Forms.Application.DoEvents();
          Render();
          System.Windows.Forms.Application.DoEvents();
        }
        catch (Exception ex)
        {
          Log.Write("Exception: {0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
#if DEBUG
          throw new Exception("exception occured",ex);
#endif
        }
      }

      if (!deviceLost &&!m_bNeedReset)
      {
				if (m_bRestore)
				{
					m_bRestore=false;
					if (m_bWasPlaying)
					{
						m_bWasPlaying=false;
						g_Player.Play(m_strCurrentFile);
						if (g_Player.Playing)
						{
							g_Player.SeekAbsolute(m_dCurrentPos);
						}
					}
					GUIWindowManager.ActivateWindow(m_iActiveWindow);
				}
      }
      System.Windows.Forms.Application.DoEvents();
      
    }




    /// <summary>
    /// Update the various statistics the simulation keeps track of
    /// </summary>
    public void UpdateStats()
    {
      // Keep track of the frame count
      float time = DXUtil.Timer(DirectXTimer.GetAbsoluteTime);
      ++frames;

      // Update the scene stats once per second
      if (time - lastTime >= 1.0f)
      {
        framePerSecond    = frames / (time - lastTime);
        lastTime = time;
        frames  = 0;

        string strFmt;
        Format fmtAdapter = graphicsSettings.DisplayMode.Format;
        if (fmtAdapter == GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferFormat)
        {
          strFmt = fmtAdapter.ToString();
        }
        else
        {
          strFmt = String.Format("backbuf {0}, adapter {1}", 
            GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferFormat.ToString(), fmtAdapter.ToString());
        }

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
        frameStats = String.Format("{0} fps ({1}x{2}), {3}{4}{5}", framePerSecond.ToString("f2"),
                                    GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth, 
                                    GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight, 
                                    strFmt, strDepthFmt, strMultiSample);
      }
    }





    /// <summary>
    /// Set our variables to not active and not ready
    /// </summary>
    public void CleanupEnvironment()
    {
      active = false;
      ready  = false;
      if (GUIGraphicsContext.DX9Device != null)
        GUIGraphicsContext.DX9Device.Dispose();

    }
    #region Menu EventHandlers



    public void OnSetup(object sender, EventArgs e)
    {
      m_bAutoHideMouse=false;
      Cursor.Show();
      Invalidate(true);
      Utils.StartProcess("Configuration.exe","",true,false);

      string strNewSkin="";
      string strNewLanguage="";
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        strNewSkin=xmlreader.GetValueAsString("skin","name","CrystalCenter");
        strNewLanguage=xmlreader.GetValueAsString("skin","language","English");
        m_bAutoHideMouse=xmlreader.GetValueAsBool("general","autohidemouse",false);
        GUIGraphicsContext.MouseSupport=xmlreader.GetValueAsBool("general","mousesupport",true);
      }
      if (strNewLanguage!=m_strLanguage)
      {
        m_strLanguage=strNewLanguage;
        GUILocalizeStrings.Load(@"language\"+m_strLanguage+ @"\strings.xml");
      }
      if (strNewSkin!=m_strSkin)
      {
        m_strSkin=strNewSkin;
        GUIWindowManager.Clear();
        GUITextureManager.Dispose();
        GUIFontManager.Dispose();
        InitializeDeviceObjects();
      }
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
      catch(SampleException d3de)
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
      base.Dispose(disposing);

      Win32API.EnableStartBar(true);
      Win32API.ShowStartBar(true);
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

    private void menuItem3_Click(object sender, System.EventArgs e)
    {
      PluginForm dlg = new PluginForm();
      dlg.ShowDialog();
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
      OnSetup(sender,e);    
    }

    private void D3DApp_Load(object sender, System.EventArgs e)
    {
    
    }

    private void D3DApp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STOPPING;
      g_Player.Stop();
    }

    private void D3DApp_Resize(object sender, System.EventArgs e)
    {
    }



    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      m_bNeedUpdate=true;
    }


    private void D3DApp_Click(object sender, MouseEventArgs  e)
    {
      if (System.Windows.Forms.Form.ActiveForm != this) return;
      mouseclick(e);
    }

    private void D3DApp_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      if (System.Windows.Forms.Form.ActiveForm != this) return;
      mousemove(e);
    }

    

    /// <summary>
    /// Handle system keystrokes (ie, alt-enter)
    /// </summary>
    protected void OnKeyDown(object sender,System.Windows.Forms.KeyEventArgs e)
    {
      if ((e.Control) && (e.KeyCode == System.Windows.Forms.Keys.F2))
      {
        menuItem3_Click(null,null);
        return ;
      }

      if (e.Control==false && e.Alt==true && (e.KeyCode == System.Windows.Forms.Keys.Return))
      {
				m_bRestore=true;
        m_bWasPlaying =g_Player.Playing;
        m_strCurrentFile=g_Player.CurrentFile;
        m_iActiveWindow=GUIWindowManager.ActiveWindow;
        m_dCurrentPos=g_Player.CurrentPosition;
        g_Player.Stop();

        isMaximized=!isMaximized;
        if (isMaximized)
        {
          Log.Write("windowed->fullscreen");
          Win32API.EnableStartBar(false);
          Win32API.ShowStartBar(false);
          this.FormBorderStyle=FormBorderStyle.None;
          this.MaximizeBox=false;
          this.MinimizeBox=false;
          this.Menu=null;
          this.Location= new System.Drawing.Point(0,0);
          this.Bounds=new Rectangle(0,0,Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
          this.ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);

          deviceLost=true;
          isMaximized=true;
          m_bNeedReset=true;
          GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth=Screen.PrimaryScreen.Bounds.Width;
          GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight=Screen.PrimaryScreen.Bounds.Height;
          Log.Write("windowed->fullscreen done {0}", isMaximized);
          Log.Write("ClientSize: {0}x{1} screen:{2}x{3}",
                      this.ClientSize.Width,this.ClientSize .Height,
                      Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);

                                  
        }
        else
        {
          Log.Write("fullscreen->windowed");
          Win32API.EnableStartBar(true);
          Win32API.ShowStartBar(true);

          this.WindowState = FormWindowState.Normal;
          this.FormBorderStyle=FormBorderStyle.Sizable;
          this.MaximizeBox=true;
          this.MinimizeBox=true;          
          this.Menu=mnuMain;
          this.Location = storedLocation;
          Bounds=new Rectangle(oldBounds.X,oldBounds.Y,oldBounds.Width,oldBounds.Height);
          this.ClientSize = storedSize;
          deviceLost=true;
          m_bNeedReset=true;
          Log.Write("fullscreen->windowed done {0}", isMaximized);
          Log.Write("ClientSize: {0}x{1} screen:{2}x{3}",
                      this.ClientSize.Width,this.ClientSize .Height,
                      Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
        }
        e.Handled=true;
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
        //DoSelectNewDevice();
        OnSetup(null,null);
			}
			else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.F2)
			{
				menuItem3_Click(null,null);
			}

      if (e.Handled==false)
      {
        keydown( e);
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
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			// 
			// mnuMain
			// 
			this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																						this.mnuFile,
																																						this.menuItem1});
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
																																							this.menuItem2,
																																							this.menuItem3});
			this.menuItem1.Text = "&Tools";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Shortcut = System.Windows.Forms.Shortcut.F2;
			this.menuItem2.Text = "&Options...";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.Shortcut = System.Windows.Forms.Shortcut.CtrlF2;
			this.menuItem3.Text = "Plugins...";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
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
      if (isHandlingSizeChanges)
      {
        // Are we maximized?
        isMaximized = (this.WindowState == System.Windows.Forms.FormWindowState.Maximized);
        if (!isMaximized)
        {
          //storedSize = this.ClientSize;
          // storedLocation = this.Location;
        }
      }
      active = !(this.WindowState == System.Windows.Forms.FormWindowState.Minimized || this.Visible == false);
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
      base.OnGotFocus (e);
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
      Win32API.EnableStartBar(true);
      Win32API.ShowStartBar(true);
      isClosing = true;
      base.OnClosing(e);
    }
    #endregion
		

		
    protected virtual void keypressed(System.Windows.Forms.KeyPressEventArgs e)
    {
    }
    protected virtual void keydown( System.Windows.Forms.KeyEventArgs e)
    {
    }
    protected virtual void mousemove(System.Windows.Forms.MouseEventArgs e)
    {
      if (lastx !=e.X || lasty!=e.Y)
      {
        //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,lastx,lasty);
        lastx=e.X;
        lasty=e.Y;
        System.Windows.Forms.Cursor ourCursor = this.Cursor;
        if (!m_bShowCursor)
        {
          m_bShowCursor=true;
          m_bNeedUpdate=true;
          Invalidate(true);
          m_MouseTimeOut=DateTime.Now;
        }
      }
		}
		protected virtual void mouseclick(MouseEventArgs e)
    {
      //this.Text=String.Format("show click");
      System.Windows.Forms.Cursor ourCursor = this.Cursor;
      if (!m_bShowCursor)
      {
        m_bShowCursor=true;
        m_bNeedUpdate=true;
        Invalidate(true);
        m_MouseTimeOut=DateTime.Now;
      }
    }

		private void OnKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			keypressed(e);
		}


    void CreateDirectDrawSurface()
    {
        m_dddescription = new DirectDraw.SurfaceDescription();
      m_dddevice= new DirectDraw.Device();
      m_dddevice.SetCooperativeLevel(this,DirectDraw.CooperativeLevelFlags.Exclusive);
      m_dddevice.SetDisplayMode(presentParams.BackBufferWidth,presentParams.BackBufferHeight,32,0,false);

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
    public MediaNotFoundException(string filename) : base()
    {
      mediaFile = filename;
    }
    public MediaNotFoundException() : base()
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



};