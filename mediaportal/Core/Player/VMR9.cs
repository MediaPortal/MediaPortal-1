#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;
using Action = MediaPortal.GUI.Library.Action;
using Filter = Microsoft.DirectX.Direct3D.Filter;
using Geometry = MediaPortal.GUI.Library.Geometry;

namespace MediaPortal.Player
{
  /// <summary>
  /// General helper class to add the Video Mixing Render9 filter to a graph
  /// , set it to renderless mode and provide it our own allocator/presentor
  /// This will allow us to render the video to a direct3d texture
  /// which we can use to draw the transparent OSD on top of it
  /// Some classes which work together:
  ///  VMR9Util								: general helper class
  ///  AllocatorWrapper.cs		: implements our own allocator/presentor for vmr9 by implementing
  ///                           IVMRSurfaceAllocator9 and IVMRImagePresenter9
  ///  PlaneScene.cs          : class which draws the video texture onscreen and mixes it with the GUI, OSD,...                          
  /// </summary>
  /// // {324FAA1F-7DA6-4778-833B-3993D8FF4151}

  #region IVMR9PresentCallback interface

  [ComVisible(true), ComImport,
   Guid("324FAA1F-7DA6-4778-833B-3993D8FF4151"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IVMR9PresentCallback
  {
    [PreserveSig]
    int PresentImage(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint pImage, uint pTexture);

    //called by EVR presenter, before sample is rendered
    //used to synchronize subtitle's clock
    [PreserveSig]
    void SetSampleTime(Int64 nsSampleTime);

    [PreserveSig]
    int RenderGui(Int16 cx, Int16 cy, Int16 arx, Int16 ary);

    [PreserveSig]
    int RenderOverlay(Int16 cx, Int16 cy, Int16 arx, Int16 ary);

    [PreserveSig]
    void SetRenderTarget(uint target);

    [PreserveSig]
    void SetSubtitleDevice(IntPtr device);

    [PreserveSig]
    void RenderSubtitle(long frameStart, int left, int top, int right, int bottom, int width, int height, int xOffsetInPixels);

    [PreserveSig]
    void RenderFrame(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint pSurface);

    [PreserveSig]
    void ForceOsdUpdate(bool pForce);

    [PreserveSig]
    bool IsFullScreen();

    [PreserveSig]
    bool IsUiVisible();

    [PreserveSig]
    void RestoreDeviceSurface(uint pSurfaceDevice);

    [PreserveSig]
    int ReduceMadvrFrame();
  }

  #endregion

  public class VMR9Util : IDisposable
  {
    #region imports

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe bool Vmr9Init(IVMR9PresentCallback callback, uint dwD3DDevice, IBaseFilter vmr9Filter,
                                               uint monitor);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void Vmr9Deinit();

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void Vmr9SetDeinterlaceMode(Int16 mode);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void Vmr9SetDeinterlacePrefs(uint dwMethod);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe bool EvrInit(IVMR9PresentCallback callback, uint dwD3DDevice, 
                                              ref IBaseFilter vmr9Filter, uint monitor, int monitorIdx,
                                              bool disVsyncCorr, bool disMparCorr);

    //, uint dwWindow);
    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void EvrDeinit();

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void EVRDrawStats(bool enable);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void EVRResetStatCounters();

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void EVRNotifyRateChange(double pRate);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void EVRNotifyDVDMenuState(bool pIsInMenu);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe double EVRGetVideoFPS(int fpsSource);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void EVRUpdateDisplayFPS();

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe bool MadInit(IVMR9PresentCallback callback, int width, int height, uint dwD3DDevice, uint parent, ref IBaseFilter madFilter, IMediaControl mPMediaControl);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadDeinit();

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadStopping();

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadVrPaused(bool paused);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadVrRepeatFrameSend();

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadVr3DRight(int x, int y, int width, int height);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadVr3DLeft(int x, int y, int width, int height);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadVr3DEnable(bool Enable);

    [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void MadVrScreenResizeForce(int x, int y, int width, int height, bool displayChange);

    #endregion

    #region static vars

    public static VMR9Util g_vmr9 = null;
    private static int _instanceCounter = 0;
    public static readonly AutoResetEvent finished = new AutoResetEvent(false);

    #endregion

    #region enums

    private enum Vmr9PlayState
    {
      Playing,
      Repaint
    }

    #endregion

    #region vars

    private PlaneScene _scene = null;
    private bool _useVmr9 = false;
    private bool _inMenu = false;
    private IRender _renderFrame;
    internal IBaseFilter _vmr9Filter = null;
    internal IntPtr m_hWnd;
    private int _videoHeight, _videoWidth;
    private int _videoAspectRatioX, _videoAspectRatioY;
    private IQualProp _qualityInterface = null;
    private int _frameCounter = 0;
    private DateTime _repaintTimer = DateTime.Now;
    private IVMRMixerBitmap9 _vmr9MixerBitmapInterface = null;
    private IGraphBuilder _graphBuilder = null;
    private bool _isVmr9Initialized = false;
    private int _threadId;
    private Vmr9PlayState currentVmr9State = Vmr9PlayState.Playing;
    private string pixelAdaptive = "";
    private string verticalStretch = "";
    private string medianFiltering = "";
    private int _freeframeCounter = 0;
    public Surface MadVrRenderTargetVMR9 = null;
    protected bool UseMadVideoRenderer;      // is madVR used?
    protected bool UseEVRMadVRForTV;
    protected bool UseMadVideoRenderer3D;
    protected internal DateTime playbackTimer;
    protected internal DateTime PlaneSceneMadvrTimer = new DateTime(0);

    #endregion

    #region ctor

    /// <summary>
    /// Constructor
    /// </summary>
    public VMR9Util()
    {
      _useVmr9 = true;

      if (!GUIGraphicsContext.VMR9Allowed)
      {
        Log.Info("VMR9: ctor() - VMR9 not allowed");
        _useVmr9 = false;
        return;
      }
      _renderFrame = GUIGraphicsContext.RenderGUI;
      if (GUIGraphicsContext.DX9Device == null)
      {
        _useVmr9 = false;
        Log.Warn("VMR9: ctor() - DX9Device == null!");
      }
      if (_renderFrame == null)
      {
        _useVmr9 = false;
        Log.Debug("VMR9: ctor() _renderFrame == null");
      }
      if (g_vmr9 != null || GUIGraphicsContext.Vmr9Active)
      {
        _useVmr9 = false;
        Log.Info("VMR9: ctor() VMR9 already active");
      }
    }

    #endregion

    #region properties

    public bool UseVmr9
    {
      get { return _useVmr9; }
    }

    public int FrameCounter
    {
      get { return _frameCounter; }
      set { _frameCounter = value; }
    }

    public int FreeFrameCounter
    {
      get { return _freeframeCounter; }
      set { _freeframeCounter = value; }
    }

    public bool InMenu
    {
      get { return _inMenu; }
    }

    /// <summary>
    /// returns the width of the video
    /// </summary>
    public int VideoWidth
    {
      get { return _videoWidth; }
      set { _videoWidth = value; }
    }

    /// <summary>
    /// returns the height of the video
    /// </summary>
    public int VideoHeight
    {
      get { return _videoHeight; }
      set { _videoHeight = value; }
    }


    /// <summary>
    /// returns the width of the video
    /// </summary>
    public int VideoAspectRatioX
    {
      get { return _videoAspectRatioX; }
      set { _videoAspectRatioX = value; }
    }

    /// <summary>
    /// returns the height of the video
    /// </summary>
    public int VideoAspectRatioY
    {
      get { return _videoAspectRatioY; }
      set { _videoAspectRatioY = value; }
    }

    public IPin PinConnectedTo
    {
      get
      {
        if (!_isVmr9Initialized)
        {
          return null;
        }
        if (_vmr9Filter == null || !_useVmr9)
        {
          return null;
        }

        IPin pinIn, pinConnected;
        pinIn = DsFindPin.ByDirection(_vmr9Filter, PinDirection.Input, 0);
        if (pinIn == null)
        {
          //no input pin found, vmr9 is not possible
          return null;
        }
        pinIn.ConnectedTo(out pinConnected);
        DirectShowUtil.ReleaseComObject(pinIn);
        return pinConnected;
      }
    }

    public IPin PinConnectedInput
    {
      get
      {
        if (!_isVmr9Initialized)
        {
          return null;
        }
        if (_vmr9Filter == null || !_useVmr9)
        {
          return null;
        }

        var pinIn = DsFindPin.ByDirection(_vmr9Filter, PinDirection.Input, 0);
        if (pinIn == null)
        {
          //no input pin found, vmr9 is not possible
          return null;
        }
        return pinIn;
      }
    }

    /// <summary>
    /// This method returns true if VMR9 is enabled AND WORKING!
    /// this allows players to check if if VMR9 is working after setting up the playing graph
    /// by checking if VMR9 is possible they can for example fallback to the overlay device
    /// </summary>
    public bool IsVMR9Connected
    {
      get
      {
        if (!_isVmr9Initialized)
        {
          return false;
        }
        // check if vmr9 is enabled and if initialized
        if (_vmr9Filter == null || !_useVmr9)
        {
          Log.Warn("VMR9: Not used or no filter:{0} {1:x}", _useVmr9, _vmr9Filter);
          return false;
        }

        int hr = 0;
        //get the VMR9 input pin#0 is connected
        for (int i = 0; i < 3; ++i)
        {
          IPin pinIn, pinConnected;
          pinIn = DsFindPin.ByDirection(_vmr9Filter, PinDirection.Input, i);
          if (pinIn == null)
          {
            //no input pin found, vmr9 is not possible
            Log.Warn("VMR9: No input pin {0} found", i);
            continue;
          }

          //check if the input is connected to a video decoder
          hr = pinIn.ConnectedTo(out pinConnected);
          if (pinConnected == null)
          {
            //no pin is not connected so vmr9 is not possible
            Log.Warn("VMR9: Pin: {0} not connected: {1:x}", i, hr);
          }
          else
          {
            //Log.Info("vmr9: pin:{0} is connected",i);
            if (pinIn != null)
            {
              DirectShowUtil.ReleaseComObject(pinIn);
            }
            if (pinConnected != null)
            {
              DirectShowUtil.ReleaseComObject(pinConnected);
            }
            return true;
          }
          if (pinIn != null)
          {
            DirectShowUtil.ReleaseComObject(pinIn);
          }
          if (pinConnected != null)
          {
            DirectShowUtil.ReleaseComObject(pinConnected);
          }
        }
        return false;
      } //get {
    }

    public bool DisableLowLatencyMode
    {
      get { return _scene != null && _scene.DisableLowLatencyMode; }
      set { if (_scene != null) _scene.DisableLowLatencyMode = value; }
    }

    public bool Visible
    {
      get { return _scene != null && _scene.Visible; }
      set { if (_scene != null) _scene.Visible = value; }
    }

    public void SceneMadVr()
    {
      if (_scene != null)
      {
        Size nativeSize = new Size(3, 3);
        _scene.SetVideoWindow(nativeSize);
      }
    }

    //public bool IsVMR9Connected

    #endregion

    #region public members

    /// <summary>
    /// Register madVR WindowsMessageMP
    /// </summary>
    public void WindowsMessageMp()
    {
      // Needed to enable 3D (TODO why is needed ?)
      Log.Debug("VMR9: Delayed OSD Callback");
      RegisterOsd();
      if (VMR9Util.g_vmr9 != null) VMR9Util.g_vmr9.SetMpFullscreenWindow();
    }

    /// <summary>
    /// Register madVR StartMadVrPaused
    /// </summary>
    public void StartMadVrPaused()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        MadVrPaused(g_Player.Paused);
      }
    }

    /// <summary>
    /// Send call to repeat frame for madVR
    /// </summary>
    public void MadVrRepeatFrame()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        MadVrRepeatFrameSend();
      }
    }

    /// <summary>
    /// Send Right 3D for madVR
    /// </summary>
    public void MadVr3DSizeRight(int x, int y, int width, int height)
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        MadVr3DRight(x, y, width, height);
      }
    }

    /// <summary>
    /// Send Left 3D for madVR
    /// </summary>
    public void MadVr3DSizeLeft(int x, int y, int width, int height)
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        MadVr3DLeft(x, y, width, height);
      }
    }

    /// <summary>
    /// Send 3D enable for madVR
    /// </summary>
    public void MadVr3DOnOff(bool Enable)
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        MadVr3DEnable(Enable);
      }
    }

    /// <summary>
    /// Send screen resize for madVR
    /// </summary>
    public void MadVrScreenResize(int x, int y, int width, int height, bool displayChange)
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        MadVrScreenResizeForce(x, y, width, height, displayChange);
      }
    }

    /// <summary>
    /// Register madVR OSD callback
    /// </summary>
    public void RegisterOsd()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        if (UseMadVideoRenderer3D && !g_Player.IsTimeShifting)
        {
          // Sending message to force unfocus/focus for 3D.
          var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_UNFOCUS_FOCUS, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendThreadMessage(msg);
          Log.Debug("VMR9: send message for madVR refresh force");
        }
      }
    }

    /// <summary>
    /// Set MP Window for madVR when using 3D Trick
    /// </summary>
    public void SetMpFullscreenWindow()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        if (UseMadVideoRenderer3D)
        {
          try
          {
            // Sending message to force unfocus/focus for 3D.
            IVideoWindow videoWin = (IVideoWindow)_graphBuilder;
            if (videoWin != null)
            {
              videoWin.put_WindowStyle((WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipChildren + (int)WindowStyle.ClipSiblings));
              videoWin.put_MessageDrain(GUIGraphicsContext.ActiveForm);
            }
            UseMadVideoRenderer3D = false;
          }
          catch (Exception)
          {
            UseMadVideoRenderer3D = false;
          }
          Log.Debug("VMR9: madVR SetMpFullscreenWindow()");
        }
      }
    }

    public void ShutdownMadVr()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        Log.Debug("VMR9: ShutdownMadVr() 1");
        MadDeinit();
        GUIGraphicsContext.MadVrStop = false;
        VMR9Util.finished.Set();
        Log.Debug("VMR9: ShutdownMadVr() 2");
      }
    }

    /// <summary>
    /// Add VMR9 filter to graph and configure it
    /// </summary>
    /// <param name="graphBuilder"></param>
    public bool AddVMR9(IGraphBuilder graphBuilder)
    {
      try
      {
        // Read settings
        using (Settings xmlreader = new MPSettings())
        {
          UseMadVideoRenderer = xmlreader.GetValueAsBool("general", "useMadVideoRenderer", false);
          UseEVRMadVRForTV = xmlreader.GetValueAsBool("general", "useEVRMadVRForTV", false);
          UseMadVideoRenderer3D = xmlreader.GetValueAsBool("general", "useMadVideoRenderer3D", false);
        }
        Log.Debug("VMR9: addvmr9 - thread : {0}", Thread.CurrentThread.Name);
        if (!_useVmr9)
        {
          Log.Debug("VMR9: addvmr9 - vmr9 is deactivated");
          return false;
        }
        if (_isVmr9Initialized)
        {
          Log.Debug("VMR9: addvmr9: vmr9 has already been initialized");
          return false;
        }

        if (_instanceCounter != 0)
        {
          Log.Error("VMR9: Multiple instances of VMR9 running!!!");
          throw new Exception("VMR9Helper: Multiple instances of VMR9 running!!!");
        }

        HResult hr;
        IntPtr hMonitor = Manager.GetAdapterMonitor(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal);
        IntPtr upDevice = DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

        _scene = new PlaneScene(this);

        // Check if need to set EVR for LiveTV when using madVR
        if (UseMadVideoRenderer)
        {
          if (UseEVRMadVRForTV && g_Player.IsTimeShifting)
          {
            GUIGraphicsContext.VideoRenderer = GUIGraphicsContext.VideoRendererType.EVR;
          }
          else
          {
            GUIGraphicsContext.VideoRenderer = GUIGraphicsContext.VideoRendererType.madVR;
          }
        }

        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
        {
          // Process frames to clear D3D dialog window
          GUIWindowManager.MadVrProcess();
          //_scene.MadVrRenderTarget = GUIGraphicsContext.DX9Device.GetRenderTarget(0);
          //MadVrRenderTargetVMR9 = GUIGraphicsContext.DX9Device.GetRenderTarget(0);
        }
        _scene.Init();

        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
        {
          // Fix RDP Screen out of bound (force to use AdapterOrdinal to 0 if adapter number are out of bounds)
          int AdapterOrdinal = GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal;
          if (AdapterOrdinal >= Screen.AllScreens.Length)
          {
            AdapterOrdinal = Screen.AllScreens.Length - 1;
            Log.Info("VMR9: adapter number out of bounds");
          }
          if (GUIGraphicsContext.currentMonitorIdx != -1)
          {
            if ((OSInfo.OSInfo.Win7OrLater() &&
                 Screen.AllScreens[AdapterOrdinal].Primary) || OSInfo.OSInfo.Win8OrLater())
            {
              EvrInit(_scene, (uint) upDevice.ToInt32(), ref _vmr9Filter, (uint) hMonitor.ToInt32(),
                GUIGraphicsContext.currentMonitorIdx, false, false);
            }
            else
            {
              EvrInit(_scene, (uint) upDevice.ToInt32(), ref _vmr9Filter, (uint) hMonitor.ToInt32(),
                GUIGraphicsContext.currentMonitorIdx, true, true);
              Log.Debug("VMR9: force disable vsync and bias correction for Win7 or lower - current primary is : {0}",
                Screen.AllScreens[AdapterOrdinal].Primary);
            }
          }
          else
          {
            if ((OSInfo.OSInfo.Win7OrLater() &&
                 Screen.AllScreens[AdapterOrdinal].Primary) || OSInfo.OSInfo.Win8OrLater())
            {
              EvrInit(_scene, (uint) upDevice.ToInt32(), ref _vmr9Filter, (uint) hMonitor.ToInt32(),
                AdapterOrdinal, false, false);
            }
            else
            {
              EvrInit(_scene, (uint) upDevice.ToInt32(), ref _vmr9Filter, (uint) hMonitor.ToInt32(),
                AdapterOrdinal, true, true);
              Log.Debug("VMR9: force disable vsync and bias correction for Win7 or lower - current primary is : {0}",
                Screen.AllScreens[AdapterOrdinal].Primary);
            }
          }
          hr = new HResult(graphBuilder.AddFilter(_vmr9Filter, "Enhanced Video Renderer"));

          // Adding put_Owner here.
          IVideoWindow videoWin = (IVideoWindow)graphBuilder;
          videoWin.put_Owner(GUIGraphicsContext.ActiveForm);

          Log.Info("VMR9: added EVR Renderer to graph");
        }
        else if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
        {
          GUIGraphicsContext.MadVrOsd = false;
          GUIGraphicsContext.MadVrStop = false;
          GUIGraphicsContext.ForceMadVRFirstStart = true;
          IMediaControl mPMediaControl = (IMediaControl) graphBuilder;
          // Get Client size
          Size client = GUIGraphicsContext.form.ClientSize;
          MadInit(_scene, client.Width, client.Height, (uint)upDevice.ToInt32(),
            (uint)GUIGraphicsContext.ActiveForm.ToInt32(), ref _vmr9Filter, mPMediaControl);
          hr = new HResult(graphBuilder.AddFilter(_vmr9Filter, "madVR"));
          Log.Info("VMR9: added madVR Renderer to graph");
        }
        else
        {
          _vmr9Filter = (IBaseFilter) new VideoMixingRenderer9();
          Log.Info("VMR9: added Video Mixing Renderer 9 to graph");

          Vmr9Init(_scene, (uint) upDevice.ToInt32(), _vmr9Filter, (uint) hMonitor.ToInt32());
          hr = new HResult(graphBuilder.AddFilter(_vmr9Filter, "Video Mixing Renderer 9"));
        }

        if (_vmr9Filter == null)
        {
          Error.SetError("Unable to play movie", "Renderer could not be added");
          Log.Error("VMR9: Renderer not installed / cannot be used!");
          _scene.Stop();
          _scene.Deinit();
          _scene = null;
          return false;
        }

        if (hr != 0)
        {
          if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
          {
            EvrDeinit();
          }
          else if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
          {
            Log.Error("VMR9: MadDeinit - thread : {0}", Thread.CurrentThread.Name);
            GC.Collect();
            MadDeinit();
            GC.Collect();
            DirectShowUtil.FinalReleaseComObject(_vmr9Filter);
            Thread.Sleep(200);
            RestoreGuiForMadVr();
          }
          else
          {
            Vmr9Deinit();
          }

          _scene.Stop();
          _scene.Deinit();
          _scene = null;

          DirectShowUtil.FinalReleaseComObject(_vmr9Filter);
          _vmr9Filter = null;
          Error.SetError("Unable to play movie", "Unable to initialize Renderer");
          Log.Error("VMR9: Failed to add Renderer to filter graph");
          return false;
        }

        _graphBuilder = graphBuilder;
        _instanceCounter++;
        _isVmr9Initialized = true;
        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.VMR9)
        {
          _qualityInterface = _vmr9Filter as IQualProp;
          _vmr9MixerBitmapInterface = _vmr9Filter as IVMRMixerBitmap9;

          Log.Debug("VMR9: SetDeinterlacePrefs() for VMR9 mode");
          SetDeinterlacePrefs();

          IVMRMixerControl9 mixer = _vmr9Filter as IVMRMixerControl9;
          if (mixer != null)
          {
            VMR9MixerPrefs dwPrefs;
            mixer.GetMixingPrefs(out dwPrefs);
            dwPrefs &= ~VMR9MixerPrefs.RenderTargetMask;

            dwPrefs |= VMR9MixerPrefs.RenderTargetYUV;
            // YUV saves graphics bandwith  http://msdn2.microsoft.com/en-us/library/ms788177(VS.85).aspx
            hr.Set(mixer.SetMixingPrefs(dwPrefs));
            Log.Debug("VMR9: Enabled YUV mixing - " + hr.ToDXString());

            using (Settings xmlreader = new MPSettings())
            {
              //Enable nonsquaremixing
              if (xmlreader.GetValueAsBool("general", "nonsquare", true))
              {
                mixer.GetMixingPrefs(out dwPrefs);
                dwPrefs |= VMR9MixerPrefs.NonSquareMixing;
                hr.Set(mixer.SetMixingPrefs(dwPrefs));
                Log.Debug("VRM9: Turning on nonsquare mixing - " + hr.ToDXString());
                hr.Set(mixer.SetMixingPrefs(dwPrefs));
              }

              // Enable DecimateMask - this will effectively use only half of the input width & length
              if (xmlreader.GetValueAsBool("general", "dx9decimatemask", false))
              {
                mixer.GetMixingPrefs(out dwPrefs);
                dwPrefs &= ~VMR9MixerPrefs.DecimateMask;
                dwPrefs |= VMR9MixerPrefs.DecimateOutput;
                hr.Set(mixer.SetMixingPrefs(dwPrefs));
                Log.Debug("VRM9: Enable decimatemask - " + hr.ToDXString());
                hr.Set(mixer.SetMixingPrefs(dwPrefs));
              }

              // see  D3DTEXTUREFILTERTYPE Enumerated Type documents for further information
              // MixerPref9_PointFiltering
              // MixerPref9_BiLinearFiltering
              // MixerPref9_AnisotropicFiltering
              // MixerPref9_PyramidalQuadFiltering
              // MixerPref9_GaussianQuadFiltering

              mixer.SetMixingPrefs(dwPrefs);
              mixer.GetMixingPrefs(out dwPrefs);
              dwPrefs &= ~VMR9MixerPrefs.FilteringMask;
              string filtermode9 = xmlreader.GetValueAsString("general", "dx9filteringmode", "Gaussian Quad Filtering");
              if (filtermode9 == "Point Filtering")
              {
                dwPrefs |= VMR9MixerPrefs.PointFiltering;
              }
              else if (filtermode9 == "Bilinear Filtering")
              {
                dwPrefs |= VMR9MixerPrefs.BiLinearFiltering;
              }
              else if (filtermode9 == "Anisotropic Filtering")
              {
                dwPrefs |= VMR9MixerPrefs.AnisotropicFiltering;
              }
              else if (filtermode9 == "Pyrimidal Quad Filtering")
              {
                dwPrefs |= VMR9MixerPrefs.PyramidalQuadFiltering;
              }
              else
              {
                dwPrefs |= VMR9MixerPrefs.GaussianQuadFiltering;
              }

              hr.Set(mixer.SetMixingPrefs(dwPrefs));
              Log.Debug("VRM9: Set filter mode - " + filtermode9 + " " + hr.ToDXString());
            }
          }
        }
        _threadId = Thread.CurrentThread.ManagedThreadId;
        GUIGraphicsContext.Vmr9Active = true;
        g_vmr9 = this;
        Log.Debug("VMR9: Renderer successfully added");
      }
      catch (Exception)
      {
        _scene.Stop();
        _scene.Deinit();
        _scene = null;
        return false;
      }
      return true;
    }

    /// <summary>
    /// Enables EVR internal stats drawing 
    /// </summary>
    public void EnableEVRStatsDrawing(bool enable)
    {
      EVRDrawStats(enable);
    }

    /// <summary>
    /// Resets EVR internal stats
    /// </summary>
    public void ResetEVRStats()
    {
      EVRResetStatCounters();
    }

    /// Gets EVR frame rate 
    /// Get video FPS - returns FPS from filter graph if 'getReported' is true,
    /// otherwise returns FPS estimated from video timestamps
    ///
    /// FPS_SOURCE_ADAPTIVE = 0
    /// FPS_SOURCE_SAMPLE_TIMESTAMP = 1
    /// FPS_SOURCE_SAMPLE_DURATION= 2
    /// FPS_SOURCE_EVR_MIXER = 3
    /// </summary>
    public double GetEVRVideoFPS(int fpsSource)
    {
      return EVRGetVideoFPS(fpsSource);
    }

    /// <summary>
    /// Gets EVR frame rate 
    /// Get video FPS - returns FPS from filter graph if 'getReported' is true,
    /// otherwise returns FPS estimated from video timestamps
    /// </summary>
    public void UpdateEVRDisplayFPS()
    {
      EVRUpdateDisplayFPS();
    }

    /// <summary>
    /// Notifies EVR presenter if the DVD menu is active
    /// </summary>
    public void EVRSetDVDMenuState(bool isInDVDMenu)
    {
      EVRNotifyDVDMenuState(isInDVDMenu);
      _inMenu = isInDVDMenu;
    }

    /// <summary>
    /// Notifies EVR presenter about the playback rate changes
    /// </summary>
    public void EVRProvidePlaybackRate(double rate)
    {
      EVRNotifyRateChange(rate);
    }

    /// <summary>
    /// repaints the last frame
    /// </summary>
    public void Repaint()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (currentVmr9State == Vmr9PlayState.Playing)
      {
        return;
      }
      if (_scene != null) _scene.Repaint();
    }

    public void SetRepaint()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (!GUIGraphicsContext.Vmr9Active)
      {
        return;
      }
      Log.Debug("VMR9: SetRepaint()");
      FrameCounter = 0;
      _repaintTimer = DateTime.Now;
      currentVmr9State = Vmr9PlayState.Repaint;
      if (_scene != null) _scene.DrawVideo = false;

      //if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR &&
      //    GUIGraphicsContext.InVmr9Render)
      //{
      //  MadVrRepeatFrameSend();
      //  Log.Debug("VMR9: MadVrRepeatFrameSend()");
      //}
    }

    public bool IsRepainting
    {
      get { return (currentVmr9State == Vmr9PlayState.Repaint); }
    }

    public void Process()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (!GUIGraphicsContext.Vmr9Active)
      {
        return;
      }

      if (g_Player.Playing && g_Player.IsDVD && g_Player.IsDVDMenu)
      {
        GUIGraphicsContext.Vmr9FPS = 0f;
        currentVmr9State = Vmr9PlayState.Playing;
        if (_scene != null) _scene.DrawVideo = true;
        _repaintTimer = DateTime.Now;
        return;
      }

      TimeSpan ts = DateTime.Now - _repaintTimer;
      int frames = FrameCounter;
      if (ts.TotalMilliseconds >= 1750
          || (currentVmr9State == Vmr9PlayState.Repaint && FrameCounter > 0)
          || g_Player.Paused) // in paused state frames aren't rendered so we need "force" the GUI drawing here
      {
        _repaintTimer = DateTime.Now;
        GUIGraphicsContext.Vmr9FPS = ((float)(frames * 1000)) / ((float)ts.TotalMilliseconds);
        //Log.Info("VMR9Helper:frames:{0} fps:{1} time:{2}", frames, GUIGraphicsContext.Vmr9FPS,ts.TotalMilliseconds);
        FrameCounter = 0;

        if (_threadId == Thread.CurrentThread.ManagedThreadId)
        {
          if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.VMR9)
          {
            if (_qualityInterface != null)
            {
              VideoRendererStatistics.Update(_qualityInterface);
            }
            else
            {
              Log.Debug("_qualityInterface is null!");
            }
          }
        }
      }

      if (currentVmr9State == Vmr9PlayState.Repaint && frames > 0)
      {
        Log.Debug("VMR9: Repainting -> Playing, Frames: {0}", frames);
        GUIGraphicsContext.Vmr9FPS = 50f;
        currentVmr9State = Vmr9PlayState.Playing;
        if (_scene != null) _scene.DrawVideo = true;
        _repaintTimer = DateTime.Now;
      }
      else if (currentVmr9State == Vmr9PlayState.Playing && GUIGraphicsContext.Vmr9FPS < 2f)
      {
        Log.Debug("VMR9Helper: Playing -> Repainting, Frames {0}", frames);
        GUIGraphicsContext.Vmr9FPS = 0f;
        currentVmr9State = Vmr9PlayState.Repaint;
        if (_scene != null) _scene.DrawVideo = false;
      }
    }

    public void ProcessMadVrOsd()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        TimeSpan tsPlay = DateTime.Now - playbackTimer;
        // Register OSD back 5 seconds after rendering is done on madVR filter.
        if (tsPlay.Seconds >= 5)
        {
          if (GUIGraphicsContext.MadVrOsd)
          {
            GUIGraphicsContext.MadVrOsd = false;
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REGISTER_MADVR_OSD, 0, 0, 0, 0, 0, null);
            GUIWindowManager.SendThreadMessage(msg);
          }
        }
        if (GUIGraphicsContext.ForceMadVRRefresh)
        {
          GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ONDISPLAYMADVRCHANGED, 0, 0, 0, 0, 0, null);
          GUIWindowManager.SendMessage(message);
          GUIGraphicsContext.ForceMadVRFirstStart = false;
          Log.Debug("VMR9:  resize OSD/Screen when resolution change for madVR");
        }
      }
    }

    /// <summary>
    /// returns a IVMRMixerBitmap9 interface
    /// </summary>
    public IVMRMixerBitmap9 MixerBitmapInterface
    {
      get { return _vmr9MixerBitmapInterface; }
    }

    public void SetDeinterlacePrefs()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      Log.Debug("VMR9: SetDeinterlacePrefs()");
      int DeInterlaceMode = 3;
      using (Settings xmlreader = new MPSettings())
      {
        DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
        //None = 0
        if (DeInterlaceMode == 1)
        {
          DeInterlaceMode = 2; //BOB = 0x02
        }
        if (DeInterlaceMode == 2)
        {
          DeInterlaceMode = 4; //Weave = 0x04
        }
        if (DeInterlaceMode == 3)
        {
          DeInterlaceMode = 1; //NextBest = 0x01
        }
      }
      Vmr9SetDeinterlacePrefs((uint)DeInterlaceMode);
    }

    public void SetDeinterlaceMode()
    {
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.VMR9)
      {
        if (!_isVmr9Initialized)
        {
          return;
        }
        Log.Debug("VMR9: SetDeinterlaceMode()");
        IVMRDeinterlaceControl9 deinterlace = (IVMRDeinterlaceControl9)_vmr9Filter;
        IPin InPin = null;
        int hr = _vmr9Filter.FindPin("VMR Input0", out InPin);
        if (hr != 0)
        {
          Log.Error("VMR9: failed finding InPin {0:X}", hr);
        }
        AMMediaType mediatype = new AMMediaType();
        InPin.ConnectionMediaType(mediatype);
        //Start by getting the media type of the video stream.
        //Only VideoInfoHeader2 formats can be interlaced.
        if (mediatype.formatType == FormatType.VideoInfo2)
        {
          Log.Debug("VMR9: SetDeinterlaceMode - FormatType = VideoInfo2");
          int numModes = 0;
          VideoInfoHeader2 VideoHeader2 = new VideoInfoHeader2();
          Marshal.PtrToStructure(mediatype.formatPtr, VideoHeader2);
          VMR9VideoDesc VideoDesc = new VMR9VideoDesc();
          // If the FormatType is VideoInfo2, check the dwInterlaceFlags field for the AMInterlace.IsInterlaced flag.
          //The presence of this flag indicates the video is interlaced.
          if ((VideoHeader2.InterlaceFlags & AMInterlace.IsInterlaced) != 0)
          {
            Log.Debug("VMR9: SetDeinterlaceMode - Interlaced frame detected");
            //Fill in the VMR9VideoDesc structure with a description of the video stream.
            VideoDesc.dwSize = Marshal.SizeOf(VideoDesc); // dwSize: Set this field to sizeof(VMR9VideoDesc).
            VideoDesc.dwSampleWidth = VideoHeader2.BmiHeader.Width; // dwSampleWidth: Set this field to pBMI->biWidth. 
            VideoDesc.dwSampleHeight = VideoHeader2.BmiHeader.Height;
            // dwSampleHeight: Set this field to abs(pBMI->biHeight). 
            //SampleFormat: This field describes the interlace characteristics of the media type.
            //Check the dwInterlaceFlags field in the VIDEOINFOHEADER2 structure, and set SampleFormat equal to the equivalent VMR9_SampleFormat flag.
            if ((VideoHeader2.InterlaceFlags & AMInterlace.IsInterlaced) != 0)
            {
              if ((VideoHeader2.InterlaceFlags & AMInterlace.DisplayModeBobOnly) == 0)
              {
                VideoDesc.SampleFormat = VMR9SampleFormat.ProgressiveFrame;
              }
              if ((VideoHeader2.InterlaceFlags & AMInterlace.OneFieldPerSample) != 0)
              {
                if ((VideoHeader2.InterlaceFlags & AMInterlace.Field1First) != 0)
                {
                  VideoDesc.SampleFormat = VMR9SampleFormat.FieldSingleEven;
                }
                else
                {
                  VideoDesc.SampleFormat = VMR9SampleFormat.FieldSingleOdd;
                }
              }
              if ((VideoHeader2.InterlaceFlags & AMInterlace.Field1First) != 0)
              {
                VideoDesc.SampleFormat = VMR9SampleFormat.FieldInterleavedEvenFirst;
              }
              else
              {
                VideoDesc.SampleFormat = VMR9SampleFormat.FieldInterleavedOddFirst;
              }
            }
            //InputSampleFreq: This field gives the input frequency, which can be calculated from the AvgTimePerFrame field in the VIDEOINFOHEADER2 structure.
            //In the general case, set dwNumerator to 10000000, and set dwDenominator to AvgTimePerFrame. 
            VideoDesc.InputSampleFreq.dwDenominator = 10000000;
            VideoDesc.InputSampleFreq.dwNumerator = (int)VideoHeader2.AvgTimePerFrame;
            //OutputFrameFreq: This field gives the output frequency, which can be calculated from the InputSampleFreq value and the interleaving characteristics of the input stream:
            //Set OutputFrameFreq.dwDenominator equal to InputSampleFreq.dwDenominator.
            //If the input video is interleaved, set OutputFrameFreq.dwNumerator to 2 x InputSampleFreq.dwNumerator. (After deinterlacing, the frame rate is doubled.)
            //Otherwise, set the value to InputSampleFreq.dwNumerator.
            VideoDesc.OutputFrameFreq.dwDenominator = 10000000;
            VideoDesc.OutputFrameFreq.dwNumerator = (int)VideoHeader2.AvgTimePerFrame * 2;
            VideoDesc.dwFourCC = VideoHeader2.BmiHeader.Compression; //dwFourCC: Set this field to pBMI->biCompression.
            //Pass the structure to the IVMRDeinterlaceControl9::GetNumberOfDeinterlaceModes method.
            //Call the method twice. The first call returns the number of deinterlace modes the hardware supports for the specified format.
            hr = deinterlace.GetNumberOfDeinterlaceModes(ref VideoDesc, ref numModes, null);
            if (hr == 0 && numModes != 0)
            {
              Guid[] modes = new Guid[numModes];
              {
                //Allocate an array of GUIDs of this size, and call the method again, passing in the address of the array.
                //The second call fills the array with GUIDs. Each GUID identifies one deinterlacing mode. 
                hr = deinterlace.GetNumberOfDeinterlaceModes(ref VideoDesc, ref numModes, modes);
                for (int i = 0; i < numModes; i++)
                {
                  //To get the capabiltiies of a particular mode, call the IVMRDeinterlaceControl9::GetDeinterlaceModeCaps method.
                  //Pass in the same VMR9VideoDesc structure, along with one of the GUIDs from the array.
                  //The method fills a VMR9DeinterlaceCaps structure with the mode capabilities. 
                  VMR9DeinterlaceCaps caps = new VMR9DeinterlaceCaps();
                  caps.dwSize = Marshal.SizeOf(typeof (VMR9DeinterlaceCaps));
                  hr = deinterlace.GetDeinterlaceModeCaps(modes[i], ref VideoDesc, ref caps);
                  if (hr == 0)
                  {
                    Log.Debug("VMR9: AvailableDeinterlaceMode - {0}: {1}", i, modes[i]);
                    switch (caps.DeinterlaceTechnology)
                    {
                        //The algorithm is unknown or proprietary
                      case VMR9DeinterlaceTech.Unknown:
                        {
                          Log.Info("VMR9: Unknown H/W de-interlace mode");
                          break;
                        }
                        //The algorithm creates each missing line by repeating the line above it or below it.
                        //This method creates jagged artifacts and is not recommended.
                      case VMR9DeinterlaceTech.BOBLineReplicate:
                        {
                          Log.Info("VMR9: BOB Line Replicate capable");
                          break;
                        }
                        //The algorithm creates the missing lines by vertically stretching each video field by a factor of two.
                        //For example, it might average two lines or use a (-1, 9, 9, -1)/16 filter across four lines.
                        //Slight vertical adjustments are made to ensure that the resulting image does not "bob" up and down
                      case VMR9DeinterlaceTech.BOBVerticalStretch:
                        {
                          Log.Info("VMR9: BOB Vertical Stretch capable");
                          verticalStretch = modes[i].ToString();
                          break;
                        }
                        //The algorithm uses median filtering to recreate the pixels in the missing lines.
                      case VMR9DeinterlaceTech.MedianFiltering:
                        {
                          Log.Info("VMR9: Median Filtering capable");
                          medianFiltering = modes[i].ToString();
                          break;
                        }
                        //The algorithm uses an edge filter to create the missing lines.
                        //In this process, spatial directional filters are applied to determine the orientation of edges in the picture content.
                        //Missing pixels are created by filtering along (rather than across) the detected edges.
                      case VMR9DeinterlaceTech.EdgeFiltering:
                        {
                          Log.Info("VMR9: Edge Filtering capable");
                          break;
                        }
                        //The algorithm uses spatial or temporal interpolation, switching between the two on a field-by-field basis, depending on the amount of motion.
                      case VMR9DeinterlaceTech.FieldAdaptive:
                        {
                          Log.Info("VMR9: Field Adaptive capable");
                          break;
                        }
                        //The algorithm uses spatial or temporal interpolation, switching between the two on a pixel-by-pixel basis, depending on the amount of motion.
                      case VMR9DeinterlaceTech.PixelAdaptive:
                        {
                          Log.Info("VMR9: Pixel Adaptive capable");
                          pixelAdaptive = modes[i].ToString();
                          break;
                        }
                        //The algorithm identifies objects within a sequence of video fields.
                        //Before it recreates the missing pixels, it aligns the movement axes of the individual objects in the scene to make them parallel with the time axis.
                      case VMR9DeinterlaceTech.MotionVectorSteered:
                        {
                          Log.Info("VMR9: Motion Vector Steered capable");
                          break;
                        }
                    }
                  }
                }
              }
              //Set the MP preferred h/w de-interlace modes in order of quality
              //pixel adaptive, then median filtering & finally vertical stretch
              if (pixelAdaptive != "")
              {
                Guid DeinterlaceMode = new Guid(pixelAdaptive);
                Log.Debug("VMR9: trying pixel adaptive");
                hr = deinterlace.SetDeinterlaceMode(0, DeinterlaceMode);
                if (hr != 0)
                {
                  Log.Error("VMR9: pixel adaptive failed!");
                }
                else
                {
                  Log.Info("VMR9: setting pixel adaptive succeeded");
                  medianFiltering = "";
                  verticalStretch = "";
                }
              }
              if (medianFiltering != "")
              {
                Guid DeinterlaceMode = new Guid(medianFiltering);
                Log.Debug("VMR9: trying median filtering");
                hr = deinterlace.SetDeinterlaceMode(0, DeinterlaceMode);
                if (hr != 0)
                {
                  Log.Error("VMR9: median filtering failed!");
                }
                else
                {
                  Log.Info("VMR9: setting median filtering succeeded");
                  verticalStretch = "";
                }
              }
              if (verticalStretch != "")
              {
                Guid DeinterlaceMode = new Guid(verticalStretch);
                Log.Debug("VMR9: trying vertical stretch");
                hr = deinterlace.SetDeinterlaceMode(0, DeinterlaceMode);
                if (hr != 0)
                {
                  Log.Error("VMR9: Cannot set H/W de-interlace mode - using VMR9 fallback");
                }
                Log.Info("VMR9: setting vertical stretch succeeded");
              }
            }
            else
            {
              Log.Info("VMR9: No H/W de-interlaced modes supported, using fallback preference");
            }
          }
          else
          {
            Log.Info("VMR9: progressive mode detected - no need to de-interlace");
          }
        }
          //If the format type is VideoInfo, it must be a progressive frame.
        else
        {
          Log.Info("VMR9: no need to de-interlace this video source");
        }
        DsUtils.FreeAMMediaType(mediatype);
        //release the VMR9 pin
        DirectShowUtil.ReleaseComObject(InPin);

        InPin = null;
        mediatype = null;
      }
    }

    public void Enable(bool onOff)
    {
      //Log.Info("Vmr9:Enable:{0}", onOff);
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (_scene != null)
      {
        _scene.Enabled = onOff;
      }
      if (onOff)
      {
        _repaintTimer = DateTime.Now;
        FrameCounter = 50;
      }
    }

    public bool Enabled
    {
      get
      {
        if (!_isVmr9Initialized)
        {
          return true;
        }
        if (_scene == null)
        {
          return true;
        }
        return _scene.Enabled;
      }
    }

    public int StartMediaCtrl(IMediaControl mediaCtrl)
    {
      lock (this)
      {
        if (!UseMadVideoRenderer3D || g_Player.IsTV || g_Player.IsTimeShifting || GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
        {
          IVideoWindow videoWin = (IVideoWindow)_graphBuilder;
          if (videoWin != null)
          {
            videoWin.put_WindowStyle((WindowStyle) ((int) WindowStyle.Child + (int) WindowStyle.ClipChildren + (int) WindowStyle.ClipSiblings));
            videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);
            Log.Debug("VMR9: StartMediaCtrl start put_WindowStyle");
          }
        }

        var hr = mediaCtrl.Run();
        Log.Debug("VMR9: StartMediaCtrl start hr: {0}", hr);
        DsError.ThrowExceptionForHR(hr);
        // S_FALSE from IMediaControl::Run means: The graph is preparing to run, but some filters have not completed the transition to a running state.
        if (hr == 1)
        {
          // wait max. 5 seconds for the graph to transition to the running state
          DateTime startTime = DateTime.Now;
          FilterState filterState;
          do
          {
            Thread.Sleep(10);
            hr = mediaCtrl.GetState(10, out filterState);
            hr = mediaCtrl.Run();
            // check with timeout max. 10 times a second if the state changed
          } while ((hr != 0) && ((DateTime.Now - startTime).TotalSeconds <= 5));
          if (hr != 0) // S_OK
          {
            DsError.ThrowExceptionForHR(hr);
            Log.Debug("VMR9: StartMediaCtrl try to play with hr: 0x{0}", hr.ToString("X8"));
          }
          Log.Debug("VMR9: StartMediaCtrl hr: {0}", hr);
        }
        return hr;
      }
    }

    public void Vmr9MediaCtrl(IMediaControl mediaCtrl)
    {
      // Disable exclusive mode here to avoid madVR window staying on top
      try
      {
        if (mediaCtrl != null)
        {
          Log.Debug("VMR9: mediaCtrl.Stop() 1");
          if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
          {
            //GUIGraphicsContext.MadVrStop = true;
            //finished.WaitOne(5000);

            //// Check if the stop was done on from madVR thread
            //if (GUIGraphicsContext.MadVrStop)
            {
              Log.Debug("VMR9: Vmr9MediaCtrl MadDeinit()");
              MadStopping();
            }
          }
          //else
          {
            var hr = mediaCtrl.Stop();
            DsError.ThrowExceptionForHR(hr);
          }
          Log.Debug("VMR9: mediaCtrl.Stop() 2");

          if (GUIGraphicsContext.InVmr9Render)
          {
            switch (GUIGraphicsContext.VideoRenderer)
            {
              case GUIGraphicsContext.VideoRendererType.madVR:
                GUIGraphicsContext.InVmr9Render = false;
                //if (_vmr9Filter != null) MadvrInterface.EnableExclusiveMode(false, _vmr9Filter);
                break;
              default:
                Log.Error("VMR9: {0} in renderer", g_Player.Player.ToString());
                break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("VMR9: Error while stopping graph or exclusive madVR mode : {0}", ex);
      }
    }

    public void RestoreGuiForMadVr()
    {
      if (MadVrRenderTargetVMR9 != null && !MadVrRenderTargetVMR9.Disposed)
      {
        GUIGraphicsContext.DX9Device.SetRenderTarget(0, MadVrRenderTargetVMR9);
        MadVrRenderTargetVMR9.Dispose();
        MadVrRenderTargetVMR9 = null;

        GUIGraphicsContext.currentScreen = Screen.FromControl(GUIGraphicsContext.form);
        GUIGraphicsContext.form.Location = new Point(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y);

        // Send action message to refresh screen
        Action actionScreenRefresh = new Action(Action.ActionType.ACTION_MADVR_SCREEN_REFRESH, 0, 0);
        GUIGraphicsContext.OnAction(actionScreenRefresh);

        if ((GUIGraphicsContext.form.WindowState != FormWindowState.Minimized))
        {
          // Make MediaPortal window normal ( if minimized )
          Win32API.ShowWindow(GUIGraphicsContext.ActiveForm, Win32API.ShowWindowFlags.ShowNormal);

          // Make Mediaportal window focused
          if (Win32API.SetForegroundWindow(GUIGraphicsContext.ActiveForm, true))
          {
            Log.Info("VMR9: Successfully switched focus.");
          }

          // Bring MP to front
          GUIGraphicsContext.form.BringToFront();
        }
        Log.Debug("VMR9: RestoreGuiForMadVr");
      }
    }

    public bool SaveBitmap(Bitmap bitmap, bool show, bool transparent, float alphaValue)
    {
      if (!_isVmr9Initialized)
      {
        return false;
      }
      if (_vmr9Filter == null)
      {
        return false;
      }

      if (MixerBitmapInterface == null)
      {
        return false;
      }

      if (GUIGraphicsContext.Vmr9Active == false)
      {
        Log.Info("SaveVMR9Bitmap() failed - no VMR9");
        return false;
      }
      int hr = 0;
      // transparent image?
      using (MemoryStream mStr = new MemoryStream())
      {
        if (bitmap != null)
        {
          if (transparent == true)
          {
            bitmap.MakeTransparent(Color.Black);
          }
          bitmap.Save(mStr, ImageFormat.Bmp);
          mStr.Position = 0;
        }

        VMR9AlphaBitmap bmp = new VMR9AlphaBitmap();

        if (show == true)
        {
          // get AR for the bitmap
          Rectangle src, dest;
          g_vmr9.GetVideoWindows(out src, out dest);

          int width = g_vmr9.VideoWidth;
          int height = g_vmr9.VideoHeight;

          float xx = (float)src.X / width;
          float yy = (float)src.Y / height;
          float fx = (float)(src.X + src.Width) / width;
          float fy = (float)(src.Y + src.Height) / height;
          //

          using (
            Surface surface = GUIGraphicsContext.DX9Device.CreateOffscreenPlainSurface(GUIGraphicsContext.Width,
                                                                                       GUIGraphicsContext.Height,
                                                                                       Format.X8R8G8B8,
                                                                                       Pool.SystemMemory))
          {
            SurfaceLoader.FromStream(surface, mStr, Filter.None, 0);
            bmp.dwFlags = (VMR9AlphaBitmapFlags)(4 | 8);
            bmp.clrSrcKey = 0;
            unsafe
            {
              bmp.pDDS = (IntPtr)surface.UnmanagedComPointer;
            }
            bmp.rDest = new NormalizedRect();
            bmp.rDest.top = yy;
            bmp.rDest.left = xx;
            bmp.rDest.bottom = fy;
            bmp.rDest.right = fx;
            bmp.fAlpha = alphaValue;
            //Log.Info("SaveVMR9Bitmap() called");
            if (g_vmr9.MixerBitmapInterface != null) hr = g_vmr9.MixerBitmapInterface.SetAlphaBitmap(ref bmp);
            if (hr != 0)
            {
              //Log.Info("SaveVMR9Bitmap() failed: error {0:X} on SetAlphaBitmap()",hr);
              return false;
            }
          }
        }
        else
        {
          bmp.dwFlags = (VMR9AlphaBitmapFlags)1;
          bmp.clrSrcKey = 0;
          bmp.rDest = new NormalizedRect();
          bmp.rDest.top = 0.0f;
          bmp.rDest.left = 0.0f;
          bmp.rDest.bottom = 1.0f;
          bmp.rDest.right = 1.0f;
          bmp.fAlpha = alphaValue;
          hr = g_vmr9.MixerBitmapInterface.UpdateAlphaBitmapParameters(ref bmp);
          if (hr != 0)
          {
            return false;
          }
        }
      }
      // dispose
      return true;
    }

    // savevmr9bitmap

    public void GetVideoWindows(out Rectangle rSource, out Rectangle rDest)
    {
      Geometry m_geometry = new Geometry();
      // get the window where the video/tv should be shown
      float x = GUIGraphicsContext.VideoWindow.X;
      float y = GUIGraphicsContext.VideoWindow.Y;
      float nw = GUIGraphicsContext.VideoWindow.Width;
      float nh = GUIGraphicsContext.VideoWindow.Height;

      GUIGraphicsContext.Correct(ref x, ref y);

      //sanity checks
      if (nw > GUIGraphicsContext.OverScanWidth)
      {
        nw = GUIGraphicsContext.OverScanWidth;
      }
      if (nh > GUIGraphicsContext.OverScanHeight)
      {
        nh = GUIGraphicsContext.OverScanHeight;
      }

      //are we supposed to show video in fullscreen or in a preview window?
      if (GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.ShowBackground)
      {
        //yes fullscreen, then use the entire screen
        x = GUIGraphicsContext.OverScanLeft;
        y = GUIGraphicsContext.OverScanTop;
        nw = GUIGraphicsContext.OverScanWidth;
        nh = GUIGraphicsContext.OverScanHeight;
      }

      //calculate the video window according to the current aspect ratio settings
      float fVideoWidth = (float)VideoWidth;
      float fVideoHeight = (float)VideoHeight;
      m_geometry.ImageWidth = (int)fVideoWidth;
      m_geometry.ImageHeight = (int)fVideoHeight;
      m_geometry.ScreenWidth = (int)nw;
      m_geometry.ScreenHeight = (int)nh;
      m_geometry.ARType = GUIGraphicsContext.ARType;
      m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
      m_geometry.GetWindow(VideoAspectRatioX, VideoAspectRatioY, out rSource, out rDest);
      rDest.X += (int)x;
      rDest.Y += (int)y;
      m_geometry = null;
    }

    #endregion

    #region IDisposeable

    /// <summary>
    /// removes the vmr9 filter from the graph and free up all unmanaged resources
    /// </summary>
    public void Dispose()
    {
      try
      {
        Log.Debug("VMR9: Dispose");
        if (false == _isVmr9Initialized)
        {
          Log.Debug("VMR9: Dispose 0");
          return;
        }
        if (_threadId != Thread.CurrentThread.ManagedThreadId)
        {
          Log.Error("VMR9: Dispose() from wrong thread");
          //return;
        }
        if (_vmr9Filter == null)
        {
          Log.Error("VMR9: Dispose() no filter");
          return;
        }

        if (_scene != null)
        {
          _scene.Stop();
          _instanceCounter--;
          _scene.Deinit();
          GUIGraphicsContext.Vmr9Active = false;
          GUIGraphicsContext.Vmr9FPS = 0f;
          GUIGraphicsContext.InVmr9Render = false;
          currentVmr9State = Vmr9PlayState.Playing;
          Log.Debug("VMR9: Dispose 1");
        }

        _vmr9MixerBitmapInterface = null;

        _qualityInterface = null;

        if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
        {
          EvrDeinit();
        }
        else if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
        {
          Log.Debug("VMR9: Dispose MadDeinit - thread : {0}", Thread.CurrentThread.Name);
          GC.Collect();
          MadDeinit();
          GC.Collect();
          MadvrInterface.restoreDisplayModeNow(_vmr9Filter);
          DirectShowUtil.FinalReleaseComObject(_vmr9Filter);
          Log.Debug("VMR9: Dispose 2");
        }
        else
        {
          Vmr9Deinit();
        }

        if (_vmr9Filter != null)
        {
          DirectShowUtil.RemoveFilter(_graphBuilder, _vmr9Filter);
          DirectShowUtil.ReleaseComObject(_vmr9Filter);
          Log.Debug("VMR9: Dispose 3");
        }
        g_vmr9.Enable(false);
        _scene = null;
        g_vmr9 = null;
        _isVmr9Initialized = false;
        GUIGraphicsContext.DX9DeviceMadVr = null;
        Log.Debug("VMR9: Dispose 4");
      }
      catch (Exception)
      {
        _vmr9Filter = null;
        _scene = null;
        g_vmr9 = null;
        _isVmr9Initialized = false;
        GUIGraphicsContext.DX9DeviceMadVr = null;
      }
      finally
      {
        RestoreGuiForMadVr();
        DirectShowUtil.TryRelease(ref _vmr9Filter);
        GUIWindowManager.MadVrProcess();
        _vmr9Filter = null;
        Log.Debug("VMR9: Dispose done");
      }
    }

    #endregion
  }
}