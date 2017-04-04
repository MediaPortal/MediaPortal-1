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
using System.Runtime.InteropServices;
using System.Threading;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player.Subtitles;
using MediaPortal.Profile;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Geometry = MediaPortal.GUI.Library.Geometry;

namespace MediaPortal.Player
{
  /// <summary>
  /// This class will draw a video texture onscreen when using VMR9 renderless
  /// Its controlled by the allocator wrapper 
  /// Example on how to use:
  /// PlaneScene scene = new PlaneScene(GUIGraphicsContext.RenderGUI)
  /// scene.Init()
  /// ... allocate direct3d texture
  /// scene.SetSrcRect(1.0f,1.0f); //change this depending on the texture dimensions
  /// scene.SetSurface(texture); //change this depending on the texture dimensions
  /// while (playingMovie)
  /// {
  ///   scene.Render(GUIGraphicsContext.DX9Device, videoTextre, videoSize) 
  /// }
  /// scene.ReleaseSurface(texture)
  /// scene.Stop();
  /// scene.DeInit();
  /// </summary>
  public class PlaneScene : IVMR9PresentCallback, IRenderLayer
  {
    #region variables

    private bool _stopPainting = false;
    private Surface _renderTarget = null;
    private long _diffuseColor = 0xFFFFFFFF;
    private int _fadeFrameCounter = 0;
    private bool _fadingIn = true;
    private float _uValue = 1.0f;
    private float _vValue = 1.0f;
    private Rectangle _rectPrevious;
    private Rectangle _subsRect;
    private bool _shouldRenderTexture = false;
    private bool _lastOverlayVisible = false;
    private bool _isEnabled = true;
    private Geometry.Type _aspectRatioType;
    private static Rectangle _sourceRect, _destinationRect;
    private Geometry _geometry = new Geometry();
    private VMR9Util _vmr9Util = null;
    private VertexBuffer[] _vertexBuffers;
    private uint _textureAddress;

    private CropSettings _cropSettings;
    private bool updateCrop = false; // indicates that _cropSettings has been updated
    private int _arVideoWidth = 4;
    private int _arVideoHeight = 3;
    private int _prevVideoWidth = 0;
    private int _prevVideoHeight = 0;
    private int _prevArVideoWidth = 0;
    private int _prevArVideoHeight = 0;
    private static bool _reEntrant = false;
    private bool _drawVideoAllowed = true;
    private int _debugStep = 0;
    private GUIImage _blackImage;

    private const int _full3DTABMinHeight = 720 * 2;
    private const int _full3DSBSMinWidth  = 1280 * 2;

    private FrameGrabber grabber = FrameGrabber.GetInstance();

    //Additions for Non-Linear Stretch
    private bool _useNonLinearStretch = false; //Should we do NLS for this AR?

    //These are the partitioning values for the non-linear stretch.
    //TODO: Maybe get these from Geometry class.
    //The horizontal percentage of the source each partition represents (Should be symmetrical and sum up to 100.0)
    private float[] nlsSourcePartitioning = {
                                              6.25f, 9.375f, 12.50f, 6.25f, 12.50f, 6.25f, 12.50f, 6.25f, 12.50f, 9.375f
                                              ,
                                              6.25f
                                            };

    //The horizontal percentage of the destination to fit each partition into (Should be symmetrical and sum up to 100.0)
    private float[] nlsDestPartitioning = {
                                            7.06f, 10.15f, 12.65f, 5.88f, 11.47f, 5.58f, 11.47f, 5.88f, 12.65f, 10.15f,
                                            7.06f
                                          };

    private bool _disableLowLatencyMode = false;
    private bool _visible = false;

    private int _reduceMadvrFrame = 0;
    private bool _useReduceMadvrFrame = false;
    private string _subEngineType = "";
    private readonly object _lockobj = new object();

    private bool UiVisible { get; set; }

    #endregion

    #region ctor

    public PlaneScene(VMR9Util util)
    {
      MadVrRenderTarget = null;
      //	Log.Info("PlaneScene: ctor()");

      _textureAddress = 0;
      _vmr9Util = util;

      // Number of vertex buffers must be same as numer of segments in non-linear stretch
      _vertexBuffers = new VertexBuffer[nlsSourcePartitioning.Length];
      for (int i = 0; i < _vertexBuffers.Length; i++)
      {
        _vertexBuffers[i] = new VertexBuffer(typeof (CustomVertex.TransformedColoredTextured),
                                             4,
                                             GUIGraphicsContext.DX9Device,
                                             0,
                                             CustomVertex.TransformedColoredTextured.Format,
                                             GUIGraphicsContext.GetTexturePoolType());
      }

      _blackImage = new GUIImage(0);
      _blackImage.SetFileName("black.png");
      _blackImage.AllocResources();

      _cropSettings = new CropSettings();
    }

    #endregion

    #region properties

    /// <summary>
    /// Returns a rectangle specifying the part of the video texture which is 
    /// shown
    /// </summary>
    public static Rectangle SourceRect
    {
      get { return _sourceRect; }
    }

    /// <summary>
    /// Returns a rectangle specifying the video window onscreen
    /// </summary>
    public static Rectangle DestRect
    {
      get { return _destinationRect; }
    }

    public bool DrawVideo
    {
      get { return _drawVideoAllowed; }
      set
      {
        _drawVideoAllowed = value;
        //Log.Info("PlaneScene: video draw allowed:{0}", _drawVideoAllowed);
      }
    }

    public bool DisableLowLatencyMode
    {
      get { return _disableLowLatencyMode; }
      set { _disableLowLatencyMode = value; }
    }

    public bool Visible
    {
      get { return _visible; }
      set { _visible = value; }
    }

    public Surface MadVrRenderTarget { get; set; }

    public bool Enabled
    {
      get { return _isEnabled; }
      set
      {
        _isEnabled = value;
        //Log.Info("planescene: enabled:{0}", _isEnabled);
      }
    }

    #endregion

    #region public members

    /// <summary>
    /// Stop VMR9 rendering
    /// this method will restore the DirectX render target since
    /// this might have been changed by the Video Mixing Renderer9
    /// </summary>
    public void Stop()
    {
      //Log.Info("PlaneScene: Stop()");
      DrawVideo = false;
      _stopPainting = true;
    }

    /// <summary>
    /// Set the texture dimensions. Sometimes the video texture is larger then the
    /// video resolution. In this case we should copy only a part from the video texture
    /// Using this function one can set how much of the video texture should be used
    /// </summary>
    /// <param name="fU">(0-1) Specifies the width to used of the video texture</param>
    /// <param name="fV">(0-1) Specifies the height to used of the video texture</param>
    public void SetSrcRect(float fU, float fV)
    {
      _rectPrevious = new Rectangle(0, 0, 0, 0);
      _uValue = fU;
      _vValue = fV;
    }

    /// <summary>
    /// Deinitialize. Release the vertex buffer and the render target resources
    /// This function should be called at last when playing has been stopped
    /// </summary>
    public void Deinit()
    {
      GUIWindowManager.Receivers -= new SendMessageHandler(this.OnMessage);
      GUILayerManager.UnRegisterLayer(this);
      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        //if (MadVrRenderTarget != null)
        //{
        //  //VMR9 changes the directx 9 render target. Thats why we set it back to what it was
        //  if (!MadVrRenderTarget.Disposed)
        //  {
        //    GUIGraphicsContext.DX9Device.SetRenderTarget(0, MadVrRenderTarget);
        //  }
        //  MadVrRenderTarget.Dispose();
        //  MadVrRenderTarget = null;
        //}
      }
      else if (_renderTarget != null)
      {
        //VMR9 changes the directx 9 render target. Thats why we set it back to what it was
        if (!_renderTarget.Disposed)
        {
          GUIGraphicsContext.DX9Device.SetRenderTarget(0, _renderTarget);
        }
        _renderTarget.SafeDispose();
        _renderTarget = null;
      }
      for (int i = 0; i < _vertexBuffers.Length; i++)
      {
        if (_vertexBuffers[i] != null)
        {
          _vertexBuffers[i].SafeDispose();
          _vertexBuffers[i] = null;
        }
      }
      if (_blackImage != null)
      {
        _blackImage.SafeDispose();
        _blackImage = null;
      }

      if (grabber != null) 
      {
        lock (GUIGraphicsContext.RenderModeSwitch)
        {
          grabber.Clean();
        }
      }
      SubtitleRenderer.GetInstance().Clear();

      if (GUIGraphicsContext.LastFrames != null)
      {
        foreach (Texture texture in GUIGraphicsContext.LastFrames)
        {
          texture.Dispose();
        }
        GUIGraphicsContext.LastFrames.Clear();
      }

      if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
      {
        GUIGraphicsContext.InVmr9Render = false;
        if (VMR9Util.g_vmr9 != null)
        {
          VMR9Util.g_vmr9.RestoreGuiForMadVr();
        }
      }
    }

    /// <summary>
    /// Initialize.
    /// This should be called before any other methods.
    /// It allocates resources needed
    /// </summary>
    /// <param name="device">Direct3d devices</param>
    public void Init()
    {
      //Log.Info("PlaneScene: init()");
      if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
      {
        _renderTarget = GUIGraphicsContext.DX9Device.GetRenderTarget(0);

        // Reset 3D
        GUIGraphicsContext.NoneDone = false;
        GUIGraphicsContext.TopAndBottomDone = false;
        GUIGraphicsContext.SideBySideDone = false;
      }
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Video);
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnMessage);
      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          _disableLowLatencyMode = xmlreader.GetValueAsBool("general", "disableLowLatencyMode", true);
          _reduceMadvrFrame = xmlreader.GetValueAsInt("general", "reduceMadvrFrame", 0);
          _useReduceMadvrFrame = xmlreader.GetValueAsBool("general", "useReduceMadvrFrame", false);
          _subEngineType = xmlreader.GetValueAsString("subtitles", "engine", "DirectVobSub");
        }
        catch (Exception)
        {
        }
      }
    }

    /// <summary>
    /// OnMessage.
    /// Handles received GUIMessage's from graphics context.
    /// </summary>
    /// <param name="message">GUIMessage</param>
    private void OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLANESCENE_CROP:
          CropSettings cs = message.Object as CropSettings;
          if (cs != null)
          {
            Crop(cs);
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_REGISTER_MADVR_OSD:
          if (VMR9Util.g_vmr9 != null)
            VMR9Util.g_vmr9.WindowsMessageMp();
          break;
      }
    }

    /// <summary>
    /// Crop.
    /// Crops the current picture..
    /// </summary>
    /// <param name="message">CropSettings</param>
    private void Crop(CropSettings cs)
    {
      lock (this)
      {
        _cropSettings = cs;
        Log.Info("PlaneScene: Crop: top:{0}, bottom:{1}, left:{2}, right:{3}", cs.Top, cs.Bottom, cs.Left, cs.Right);
        updateCrop = true;
      }
    }

    /// <summary>
    /// This method calculates the rectangle on screen where the video should be presented
    /// this depends on if we are in fullscreen mode or preview mode
    /// and on the current aspect ration settings
    /// </summary>
    /// <param name="videoSize">Size of video stream</param>
    /// <returns>
    /// true : video window is visible
    /// false: video window is not visible
    /// </returns>
    public bool SetVideoWindow(Size videoSize)
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        try
        {
          if (!GUIGraphicsContext.IsPlayingVideo && !_vmr9Util.InMenu)
          {
            return false;
          }

          // check if the aspect ratio belongs to a Full-HD 3D format

          GUIGraphicsContext.IsFullHD3DFormat = false;

          if (((double) videoSize.Width/videoSize.Height >= 2.5) && (videoSize.Width >= _full3DSBSMinWidth))
            // we have Full HD SBS 
          {
            GUIGraphicsContext.IsFullHD3DFormat = true;
          }
          else if (((double) videoSize.Width/videoSize.Height <= 1.5) && (videoSize.Height >= _full3DTABMinHeight))
            // we have Full HD TAB
          {
            GUIGraphicsContext.IsFullHD3DFormat = true;
          }

          GUIGraphicsContext.VideoSize = videoSize;
          // get the window where the video/tv should be shown
          float x = GUIGraphicsContext.VideoWindow.X;
          float y = GUIGraphicsContext.VideoWindow.Y;
          int nw = GUIGraphicsContext.VideoWindow.Width;
          int nh = GUIGraphicsContext.VideoWindow.Height;

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

          //sanity check
          if (nw <= 10 || nh <= 10 || x < 0 || y < 0)
          {
            // Need to resize window video for madVR
            if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR) //TODO
              return false;
          }

          GUIGraphicsContext.ScaleVideoWindow(ref nw, ref nh, ref x, ref y);

          GUIGraphicsContext.VideoReceived();

          //did the video window,aspect ratio change? if not
          //then we dont need to recalculate and just return the previous settings
          //add a delta value of -1 or +1 to check
          if (!updateCrop && (int)x == _rectPrevious.X && (int)y == _rectPrevious.Y &&
              nw == _rectPrevious.Width && nh == _rectPrevious.Height &&
              GUIGraphicsContext.ARType == _aspectRatioType &&
              GUIGraphicsContext.Overlay == _lastOverlayVisible && _shouldRenderTexture &&
              (_prevVideoWidth == videoSize.Width || _prevVideoWidth == videoSize.Width + 1 ||
               _prevVideoWidth == videoSize.Width - 1) &&
              (_prevVideoHeight == videoSize.Height || _prevVideoHeight == videoSize.Height + 1 ||
               _prevVideoHeight == videoSize.Height - 1) &&
              (_prevArVideoWidth == _arVideoWidth || _prevArVideoWidth == _arVideoWidth + 1 ||
               _prevArVideoWidth == _arVideoWidth - 1) &&
              (_prevArVideoHeight == _arVideoHeight || _prevArVideoHeight == _arVideoHeight + 1 ||
               _prevArVideoHeight == _arVideoHeight - 1))
          {
            //not changed, return previous settings
            return _shouldRenderTexture;
          }

          //settings (position,size,aspect ratio) changed.
          //Store these settings and start calucating the new video window
          _rectPrevious = new Rectangle((int) x, (int) y, (int) nw, (int) nh);
          _subsRect = _rectPrevious;
          _aspectRatioType = GUIGraphicsContext.ARType;
          _lastOverlayVisible = GUIGraphicsContext.Overlay;
          _prevVideoWidth = videoSize.Width;
          _prevVideoHeight = videoSize.Height;
          _prevArVideoWidth = _arVideoWidth;
          _prevArVideoHeight = _arVideoHeight;

          //calculate the video window according to the current aspect ratio settings
          float fVideoWidth = (float) videoSize.Width;
          float fVideoHeight = (float) videoSize.Height;

          // if we have a Full-HD 3D video we half the width or height in order
          // to provide only the size of one half to the GetWindow call of the
          // Geometry class

          if (((double) videoSize.Width/videoSize.Height >= 2.5) && (videoSize.Width >= _full3DSBSMinWidth))
            // we have Full HD SBS 
          {
            fVideoWidth /= 2;
          }
          else if (((double) videoSize.Width/videoSize.Height <= 1.5) && (videoSize.Height >= _full3DTABMinHeight))
            // we have Full HD TAB
          {
            fVideoHeight /= 2;
          }

          _geometry.ImageWidth = (int) fVideoWidth;
          _geometry.ImageHeight = (int) fVideoHeight;
          _geometry.ScreenWidth = (int) nw;
          _geometry.ScreenHeight = (int) nh;
          _geometry.ARType = GUIGraphicsContext.ARType;
          _geometry.PixelRatio = GUIGraphicsContext.PixelRatio;

          // if the width or height was altered because of a Full-HD 3D format we recalculate
          // the width to allow the GetWindowCall to operate with the correct aspect ratio       

          if (GUIGraphicsContext.IsFullHD3DFormat)
          {
            _arVideoWidth = (int) ((float) _geometry.ImageWidth/_geometry.ImageHeight*_arVideoHeight);
          }

          _geometry.GetWindow(_arVideoWidth, _arVideoHeight, out _sourceRect, out _destinationRect,
            out _useNonLinearStretch, _cropSettings);

          updateCrop = false;
          _destinationRect.X += (int) x;
          _destinationRect.Y += (int) y;

          if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.madVR)
          {
            // Force VideoWindow to be refreshed with madVR when switching from video size like 16:9 to 4:3
            GUIGraphicsContext.UpdateVideoWindow = true;
            GUIGraphicsContext.VideoWindowChanged();
          }
          else
          {
            //sanity check
            if (_destinationRect.Width < 10)
            {
              return false;
            }
            if (_destinationRect.Height < 10)
            {
              return false;
            }
            if (_sourceRect.Width < 10)
            {
              return false;
            }
            if (_sourceRect.Height < 10)
            {
              return false;
            }
          }

          Log.Debug("PlaneScene: crop T, B  : {0}, {1}", _cropSettings.Top, _cropSettings.Bottom);
          Log.Debug("PlaneScene: crop L, R  : {0}, {1}", _cropSettings.Left, _cropSettings.Right);

          Log.Info("PlaneScene: video WxH  : {0}x{1}", videoSize.Width, videoSize.Height);
          Log.Debug("PlaneScene: video AR   : {0}:{1}", _arVideoWidth, _arVideoHeight);
          Log.Info("PlaneScene: screen WxH : {0}x{1}", nw, nh);
          Log.Debug("PlaneScene: AR type    : {0}", GUIGraphicsContext.ARType);
          Log.Debug("PlaneScene: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
          Log.Debug("PlaneScene: src        : ({0},{1})-({2},{3})",
            _sourceRect.X, _sourceRect.Y, _sourceRect.X + _sourceRect.Width, _sourceRect.Y + _sourceRect.Height);
          Log.Debug("PlaneScene: dst        : ({0},{1})-({2},{3})",
            _destinationRect.X, _destinationRect.Y, _destinationRect.X + _destinationRect.Width,
            _destinationRect.Y + _destinationRect.Height);

          if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
          {
            Util.Utils.SwitchFocus();
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          return false;
        }
      }
    }

    public void Repaint()
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING ||
          GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
      {
        return;
      }
      if (!_isEnabled)
      {
        return;
      }
      if (_stopPainting)
      {
        return;
      }
      try
      {
        if (!GUIGraphicsContext.InVmr9Render && GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
        {
          InternalPresentImage(_vmr9Util.VideoWidth, _vmr9Util.VideoHeight, _arVideoWidth, _arVideoHeight, true);
        }
      }
      catch (Exception ex)
      {
        Log.Error("planescene:Unhandled exception in {0} {1} {2}",
                  ex.Message, ex.Source, ex.StackTrace);
      }
    }

    #endregion

    #region IVMR9Callback Members

    public int PresentImage(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, uint pTexture, uint pSurface)
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        try
        {
          // Alert the frame grabber that it has a chance to grab a video frame
          // if it likes (method returns immediately otherwise
          if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
            grabber.OnFrame(width, height, arWidth, arHeight, pSurface, FrameGrabber.FrameSource.Video);

          _textureAddress = pTexture;

          if (pTexture == 0)
          {
            Log.Debug("PlaneScene: PresentImage() dispose surfaces");
            _vmr9Util.VideoWidth = 0;
            _vmr9Util.VideoHeight = 0;
            _vmr9Util.VideoAspectRatioX = 0;
            _vmr9Util.VideoAspectRatioY = 0;
            _arVideoWidth = 0;
            _arVideoHeight = 0;
            return 0;
          }
          if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
          {
            return 0;
          }

          _vmr9Util.FreeFrameCounter++;

          if (!_drawVideoAllowed || !_isEnabled)
          {
            Log.Info("planescene:PresentImage() frame:{0} enabled:{1} allowed:{2}", _vmr9Util.FrameCounter, _isEnabled,
                     _drawVideoAllowed);
            _vmr9Util.FrameCounter++;
            return 0;
          }
          _vmr9Util.FrameCounter++;
          //			Log.Info("vmr9:present image()");
          if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
            InternalPresentImage(width, height, arWidth, arHeight, false);
          //			Log.Info("vmr9:present image() done");
        }
        catch (Exception ex)
        {
          Log.Error("Planescene: Error in PresentImage - {0}", ex.ToString());
        }
      }
      return 0;
    }

    public void RenderFrame(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, uint pSurface)
    {
      IntPtr ptrMadVr = (IntPtr)pSurface;
      Surface surfaceMadVr = new Surface(ptrMadVr);
      try
      {
        unsafe
        {
          lock (GUIGraphicsContext.RenderModeSwitch)
          {
            if (grabber !=null)
              grabber.OnFrame(width, height, arWidth, arHeight, (uint)surfaceMadVr.UnmanagedComPointer,
                FrameGrabber.FrameSource.Video);
          }
        }
        surfaceMadVr.ReleaseGraphics();
        surfaceMadVr.Dispose();
      }
      catch (Exception ex)
      {
        surfaceMadVr.ReleaseGraphics();
        surfaceMadVr.Dispose();
      }
    }

    public int ReduceMadvrFrame()
    {
      if (_useReduceMadvrFrame)
      {
        return _reduceMadvrFrame;
      }
      return 0;
    }

    public bool IsUiVisible()
    {
      return UiVisible;
    }

    public bool IsFullScreen()
    {
      return GUIGraphicsContext.IsFullScreenVideo;
    }

    public int RenderGui(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight)
    {
      return RenderLayers(GUILayers.under, width, height, arWidth, arHeight);
    }

    public int RenderOverlay(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight)
    {
      return RenderLayers(GUILayers.over, width, height, arWidth, arHeight);
    }

    private int RenderLayers(GUILayers layers, Int16 width, Int16 height, Int16 arWidth, Int16 arHeight)
    {
      UiVisible = false;

      //lock (GUIGraphicsContext.RenderMadVrLock)
      {
        try
        {
          if (_reEntrant)
          {
            return -1;
          }

          if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
          {
            //Log.Error("1");
            return -1;
          }

          if (_stopPainting)
          {
            //Log.Error("2");
            return -1;
          }

          if (GUIGraphicsContext.IsSwitchingToNewSkin)
          {
            //Log.Error("3");
            return -1;
          }

          if (GUIWindowManager.IsSwitchingToNewWindow && !_vmr9Util.InMenu)
          {
            //Log.Error("4");
            return -1; // (0) -> S_OK, (1) -> S_FALSE; //dont present video during window transitions
          }

          // Disable for now and added back to g_player
          //if (VMR9Util.g_vmr9 != null)
          //{
          //  VMR9Util.g_vmr9.StartMadVrPaused();
          //}

          _reEntrant = true;
          GUIGraphicsContext.InVmr9Render = true;

          if (VMR9Util.g_vmr9 != null) VMR9Util.g_vmr9.PlaneSceneMadvrTimer = DateTime.Now;

          if (width > 0 && height > 0)
          {
            _vmr9Util.VideoWidth = width;
            _vmr9Util.VideoHeight = height;
            _vmr9Util.VideoAspectRatioX = arWidth;
            _vmr9Util.VideoAspectRatioY = arHeight;
            _arVideoWidth = arWidth;
            _arVideoHeight = arHeight;

            //Log.Debug("PlaneScene width {0}, height {1}", width, height);

            Size nativeSize = new Size(width, height);
            _shouldRenderTexture = SetVideoWindow(nativeSize);
          }

          lock (GUIGraphicsContext.RenderModeSwitch)
          {
            // in case of GUIGraphicsContext.BlankScreen == true always use old method
            // for painting blank screen

            if (GUIGraphicsContext.BlankScreen ||
                GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.None ||
                GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideTo2D ||
                GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottomTo2D ||
                GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideFrom2D)
            {
              if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideFrom2D)
              // convert 2D to 3D
              {
                // left half
                // right half
              }
              else // normal 2D output
              {
                // old output path or force 3D material to 2D by blitting only left/top halp
                GUIGraphicsContext.Render3DModeHalf = GUIGraphicsContext.eRender3DModeHalf.None;

                if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideTo2D)
                  GUIGraphicsContext.Render3DModeHalf = GUIGraphicsContext.eRender3DModeHalf.SBSLeft;

                if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottomTo2D)
                  GUIGraphicsContext.Render3DModeHalf = GUIGraphicsContext.eRender3DModeHalf.TABTop;

                if (!GUIGraphicsContext.NoneDone)
                {
                  // Get Client size
                  Size client = GUIGraphicsContext.form.ClientSize;

                  VMR9Util.g_vmr9?.MadVr3DSizeLeft(0, 0, client.Width, client.Height);
                  VMR9Util.g_vmr9?.MadVr3DSizeRight(0, 0, client.Width, client.Height);

                  VMR9Util.g_vmr9?.MadVr3DOnOff(false);

                  GUIGraphicsContext.NoneDone = true;
                  GUIGraphicsContext.TopAndBottomDone = false;
                  GUIGraphicsContext.SideBySideDone = false;
                }
              }
            }
            else if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide ||
                     GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottom)
            {
              // 3D output either SBS or TAB
              if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide)
              {
                // left half (or right if switched)
                // right half (or left if switched)
                if (!GUIGraphicsContext.SideBySideDone)
                {
                  // Enable 3D
                  VMR9Util.g_vmr9?.MadVr3DOnOff(true);

                  // Get Client size
                  Size client = GUIGraphicsContext.form.ClientSize;

                  // left half (or right if switched)
                  VMR9Util.g_vmr9?.MadVr3DSizeLeft(0, 0, client.Width/2, client.Height);

                  // right half (or left if switched)
                  VMR9Util.g_vmr9?.MadVr3DSizeRight(client.Width/2, 0, client.Width, client.Height);

                  GUIGraphicsContext.NoneDone = false;
                  GUIGraphicsContext.TopAndBottomDone = false;
                  GUIGraphicsContext.SideBySideDone = true;
                }
              }
              else
              {
                // upper half (or lower if switched)
                // lower half (or upper if switched)
                if (!GUIGraphicsContext.TopAndBottomDone)
                {
                  // Enable 3D
                  VMR9Util.g_vmr9?.MadVr3DOnOff(true);

                  // Get Client size
                  Size client = GUIGraphicsContext.form.ClientSize;

                  // upper half (or lower if switched)
                  VMR9Util.g_vmr9?.MadVr3DSizeLeft(0, 0, client.Width, client.Height/2);

                  // lower half (or upper if switched)
                  VMR9Util.g_vmr9?.MadVr3DSizeRight(0, client.Height/2, client.Width, client.Height);

                  GUIGraphicsContext.NoneDone = false;
                  GUIGraphicsContext.TopAndBottomDone = true;
                  GUIGraphicsContext.SideBySideDone = false;
                }
              }
            }
          }

          Device device = GUIGraphicsContext.DX9Device;

          device.Clear(ClearFlags.Target, Color.FromArgb(0, 0, 0, 0), 1.0f, 0);
          device.BeginScene();

          if (layers == GUILayers.over)
          {
            SubtitleRenderer.GetInstance().Render();
            BDOSDRenderer.GetInstance().Render();
          }

          GUIGraphicsContext.RenderGUI.RenderFrame(GUIGraphicsContext.TimePassed, layers, ref _visible);

          GUIFontManager.Present();
          device.EndScene();

          // Present() call is done on C++ side so we are able to use DirectX 9 Ex device
          // which allows us to skip the v-sync wait. We don't want to wait with madVR
          // is it only increases the UI rendering time.
          //return visible ? 0 : 1; // S_OK, S_FALSE
        }
        catch (Exception)
        {
        }
        finally
        {
          if (_visible)
          {
            UiVisible = true;
          }

          if (_disableLowLatencyMode)
          {
            _visible = false;
          }

          _reEntrant = false;

          if (VMR9Util.g_vmr9 != null)
          {
            VMR9Util.g_vmr9.ProcessMadVrOsd();
          }
        }
        return _visible ? 0 : 1; // S_OK, S_FALSE
      }
    }

    public void RestoreDeviceSurface(uint pSurfaceDevice)
    {
      if (GUIGraphicsContext.DX9Device != null)
      {
        Surface surface = new Surface((IntPtr)pSurfaceDevice);
        VMR9Util.g_vmr9.MadVrRenderTargetVMR9 = surface;
      }
    }

    public void SetRenderTarget(uint target)
    {
      lock (_lockobj)
      {
        Surface surface = new Surface((IntPtr) target);
        if (GUIGraphicsContext.DX9Device != null)
        {
          GUIGraphicsContext.DX9Device.SetRenderTarget(0, surface);
        }
        //surface.ReleaseGraphics();
        //surface.Dispose();
      }
    }

    public void SetSubtitleDevice(IntPtr device)
    {
      // Set madVR D3D Device
      GUIGraphicsContext.DX9DeviceMadVr = device != IntPtr.Zero ? new Device(device) : null;
      // No need to set subtitle engine when using XySubFilter and madVR.
      if (!_subEngineType.Equals("XySubFilter"))
      {
        ISubEngine engine = SubEngine.GetInstance(true);
        if (engine != null)
        {
          engine.SetDevice(device);
        }
        Log.Debug("Planescene: Set subtitle device - {0}", device);
      }
    }

    public void RenderSubtitle(long frameStart, int left, int top, int right, int bottom, int width, int height, int xOffsetInPixels)
    {
      ISubEngine engine = SubEngine.GetInstance();
      if (engine != null)
      {
        engine.SetTime(frameStart);
        engine.Render(_subsRect, _destinationRect, xOffsetInPixels);
      }
    }

    public void ForceOsdUpdate(bool pForce)
    {
      // Callback from C++ for madVR.
      if (pForce)
      {
        GUIGraphicsContext.MadVrOsd = true;
        if (_vmr9Util != null) _vmr9Util.playbackTimer = DateTime.Now;
      }
    }

    public static void RenderFor3DMode(GUIGraphicsContext.eRender3DModeHalf renderModeHalf, float timePassed,
      Surface backbuffer, Surface surface, Rectangle targetRect)
    {
      if (GUIGraphicsContext.Render3DMode != GUIGraphicsContext.eRender3DMode.SideBySideFrom2D ||
          GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideFrom2D &&
          renderModeHalf == GUIGraphicsContext.eRender3DModeHalf.SBSLeft)
      {
        GUIGraphicsContext.DX9Device.SetRenderTarget(0, surface);

        GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);

        GUIGraphicsContext.DX9Device.BeginScene();
        GUIGraphicsContext.SetScalingResolution(0, 0, false);

        GUIGraphicsContext.Render3DModeHalf = renderModeHalf;

        try
        {
          if (!GUIGraphicsContext.BlankScreen)
          {
            // Render GUI + Video surface
            GUIGraphicsContext.RenderGUI.RenderFrame(timePassed, GUILayers.all);
            GUIFontManager.Present();
          }
        }
        finally
        {
          GUIGraphicsContext.DX9Device.EndScene();
        }

        GUIGraphicsContext.DX9Device.SetRenderTarget(0, backbuffer);
      }

      if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideFrom2D)
      {
        // render left image for 2D to 3D conversion

        if (renderModeHalf == GUIGraphicsContext.eRender3DModeHalf.SBSLeft)
        {
          GUIGraphicsContext.DX9Device.StretchRectangle(surface,
            new Rectangle(0, 0, backbuffer.Description.Width, backbuffer.Description.Height),
            backbuffer,
            targetRect,
            TextureFilter.Point);

          // if texture for last frame does not exist, then create it

          if (GUIGraphicsContext.LastFrames.Count == 0)
          {
            for (int i = 0; i < 2; i++)
            {
              Texture texture = new Texture(GUIGraphicsContext.DX9Device,
                backbuffer.Description.Width,
                backbuffer.Description.Height, 0, Usage.RenderTarget,
                backbuffer.Description.Format, Pool.Default);

              GUIGraphicsContext.LastFrames.Add(texture);
            }
          }

          // store current image, it will be used as right image for next frame

          Surface surfaceLastFrame = GUIGraphicsContext.LastFrames[GUIGraphicsContext.LastFramesIndex].GetSurfaceLevel(0);

          GUIGraphicsContext.DX9Device.StretchRectangle(surface,
            new Rectangle(0, 0, backbuffer.Description.Width, backbuffer.Description.Height),
            surfaceLastFrame,
            new Rectangle(0, 0, backbuffer.Description.Width, backbuffer.Description.Height),
            TextureFilter.Point);
          surfaceLastFrame.Dispose();
        }
        else
          // render right image of the last frame for 2D to 3D conversion, the difference between 2 frames generates a 3D effect only for moving objects...
        {
          int lastIndex = GUIGraphicsContext.LastFramesIndex - 1;

          if (lastIndex < 0)
            lastIndex = GUIGraphicsContext.LastFrames.Count + lastIndex;

          if (GUIGraphicsContext.LastFrames.Count > 0)
          {
            Surface surfaceLastFrame = GUIGraphicsContext.LastFrames[lastIndex].GetSurfaceLevel(0);

            if (surfaceLastFrame != null)
            {
              // generate additional 3D effect for not moving objects by stretching the right image...

              double xSkewPerLine =
                (double) (GUIGraphicsContext.Convert2Dto3DSkewFactor/1000f*backbuffer.Description.Width)/
                (backbuffer.Description.Height - 1);
              int horzOffset = (int) (xSkewPerLine*backbuffer.Description.Height);

              for (int y = 0; y < backbuffer.Description.Height; y++)
              {
                /*int horzDelta = (int)(xSkewPerLine * (backbuffer.Description.Height - y));

                GUIGraphicsContext.DX9Device.StretchRectangle(surfaceLastFrame,
                              new Rectangle(horzDelta, y, backbuffer.Description.Width - horzDelta * 2, 1),
                              backbuffer,
                              new Rectangle(targetRect.X, y, targetRect.Width, 1),
                              TextureFilter.Point);*/

                int horzDelta = (int) (xSkewPerLine*y);

                GUIGraphicsContext.DX9Device.StretchRectangle(surfaceLastFrame,
                  new Rectangle(horzDelta, y, backbuffer.Description.Width - horzOffset*2 + horzDelta, 1),
                  backbuffer,
                  new Rectangle(targetRect.X, y, targetRect.Width, 1),
                  TextureFilter.Point);
              }

              surfaceLastFrame.Dispose();
            }
          }
        }
      }
      else // render normal 3D movie
      {
        GUIGraphicsContext.DX9Device.StretchRectangle(surface,
          new Rectangle(0, 0, backbuffer.Description.Width, backbuffer.Description.Height),
          backbuffer,
          targetRect,
          TextureFilter.Point);
      }
    }

    private void InternalPresentImage(int width, int height, int arWidth, int arHeight, bool isRepaint)
    {
      if (_reEntrant)
      {
        Log.Error("PlaneScene: re-entrancy in PresentImage");
        return;
      }
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
      {
        return;
      }
      try
      {
        //Direct3D.Surface backBuffer=null;
        _debugStep = 0;
        _reEntrant = true;
        if (GUIGraphicsContext.VideoRenderer != GUIGraphicsContext.VideoRendererType.madVR)
          GUIGraphicsContext.InVmr9Render = true;
        if (width > 0 && height > 0)
        {
          _vmr9Util.VideoWidth = width;
          _vmr9Util.VideoHeight = height;
          _vmr9Util.VideoAspectRatioX = arWidth;
          _vmr9Util.VideoAspectRatioY = arHeight;
          _arVideoWidth = arWidth;
          _arVideoHeight = arHeight;
        }

        //if we're stopping then just return
        float timePassed = GUIGraphicsContext.TimePassed;
        if (_stopPainting)
        {
          return;
        }
        //sanity checks
        if (GUIGraphicsContext.DX9Device == null)
        {
          return;
        }
        if (GUIGraphicsContext.DX9Device.Disposed)
        {
          return;
        }
        if (GUIWindowManager.IsSwitchingToNewWindow && !_vmr9Util.InMenu)
        {
          return; //dont present video during window transitions
        }

        _debugStep = 1;
        if (_renderTarget != null)
        {
          if (!_renderTarget.Disposed)
          {
            GUIGraphicsContext.DX9Device.SetRenderTarget(0, _renderTarget);
          }
        }

        _debugStep = 2;

        //first time, fade in the video in 12 steps
        int iMaxSteps = 12;
        if (_fadeFrameCounter < iMaxSteps)
        {
          if (_vmr9Util.InMenu)
          {
            _diffuseColor = 0xFFffffff;
          }
          else
          {
            // fade in
            int iStep = 0xff/iMaxSteps;
            if (_fadingIn)
            {
              _diffuseColor = iStep*_fadeFrameCounter;
              _diffuseColor <<= 24;
              _diffuseColor |= 0xffffff;
            }
            else
            {
              _diffuseColor = (iMaxSteps - iStep)*_fadeFrameCounter;
              _diffuseColor <<= 24;
              _diffuseColor |= 0xffffff;
            }
          }
          _fadeFrameCounter++;
        }
        else
        {
          //after 12 steps, just present the video texture
          _diffuseColor = 0xFFffffff;
        }

        _debugStep = 3;
        //get desired video window
        if (width > 0 && height > 0 && _textureAddress != 0)
        {
          Size nativeSize = new Size(width, height);
          _shouldRenderTexture = SetVideoWindow(nativeSize);
        }
        else
        {
          _shouldRenderTexture = false;
        }

        //clear screen

        _debugStep = 5;

        lock (GUIGraphicsContext.RenderModeSwitch)
        {
          // in case of GUIGraphicsContext.BlankScreen == true always use old method
          // for painting blank screen

          if (GUIGraphicsContext.BlankScreen ||
              GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.None ||
              GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideTo2D ||
              GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottomTo2D ||
              GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideFrom2D)
          {
            if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideFrom2D)
              // convert 2D to 3D
            {
              Surface backbuffer = GUIGraphicsContext.DX9Device.GetBackBuffer(0, 0, BackBufferType.Mono);

              // create texture/surface for preparation for 3D output 

              Texture auto3DTexture = new Texture(GUIGraphicsContext.DX9Device,
                backbuffer.Description.Width,
                backbuffer.Description.Height, 0, Usage.RenderTarget,
                backbuffer.Description.Format, Pool.Default);

              Surface auto3DSurface = auto3DTexture.GetSurfaceLevel(0);

              // left half

              RenderFor3DMode(GUIGraphicsContext.eRender3DModeHalf.SBSLeft,
                timePassed, backbuffer, auto3DSurface,
                new Rectangle(0, 0, backbuffer.Description.Width/2, backbuffer.Description.Height));

              // right half

              RenderFor3DMode(GUIGraphicsContext.eRender3DModeHalf.SBSRight,
                timePassed, backbuffer, auto3DSurface,
                new Rectangle(backbuffer.Description.Width/2, 0, backbuffer.Description.Width/2,
                  backbuffer.Description.Height));

              if (!GUIGraphicsContext.Render3DSubtitle)
              {
                SubtitleRenderer.GetInstance().Render();
                SubEngine.GetInstance().Render(_subsRect, _destinationRect, 0);
              }

              GUIGraphicsContext.DX9Device.Present();
              backbuffer.Dispose();

              auto3DSurface.Dispose();
              auto3DTexture.Dispose();

              GUIGraphicsContext.LastFramesIndex++;

              if (GUIGraphicsContext.LastFramesIndex > GUIGraphicsContext.LastFrames.Count - 1)
                GUIGraphicsContext.LastFramesIndex = 0;
            }
            else // normal 2D output
            {
              // old output path or force 3D material to 2D by blitting only left/top halp

              // Alert the frame grabber that it has a chance to grab a GUI frame
              // if it likes (method returns immediately otherwise
              grabber.OnFrameGUI();

              GUIGraphicsContext.Render3DModeHalf = GUIGraphicsContext.eRender3DModeHalf.None;

              if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideTo2D)
                GUIGraphicsContext.Render3DModeHalf = GUIGraphicsContext.eRender3DModeHalf.SBSLeft;

              if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottomTo2D)
                GUIGraphicsContext.Render3DModeHalf = GUIGraphicsContext.eRender3DModeHalf.TABTop;

              GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
              GUIGraphicsContext.DX9Device.BeginScene();

              try
              {
                if (!GUIGraphicsContext.BlankScreen)
                {
                  // Render GUI + Video surface
                  GUIGraphicsContext.RenderGUI.RenderFrame(timePassed, GUILayers.all);
                  GUIFontManager.Present();
                }
              }
              finally
              {
                GUIGraphicsContext.DX9Device.EndScene();
              }

              GUIGraphicsContext.DX9Device.Present();
            }
          }
          else if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide ||
                   GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottom)
          {

            // 3D output either SBS or TAB

            Surface backbuffer = GUIGraphicsContext.DX9Device.GetBackBuffer(0, 0, BackBufferType.Mono);

            // create texture/surface for preparation for 3D output
 
            // Alert the frame grabber that it has a chance to grab a GUI frame
            // if it likes (method returns immediately otherwise
            grabber.OnFrameGUI(backbuffer);

            // create texture/surface for preparation for 3D output

            Texture auto3DTexture = new Texture(GUIGraphicsContext.DX9Device,
              backbuffer.Description.Width,
              backbuffer.Description.Height, 0, Usage.RenderTarget,
              backbuffer.Description.Format, Pool.Default);

            Surface auto3DSurface = auto3DTexture.GetSurfaceLevel(0);

            if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide)
            {
              // left half (or right if switched)

              RenderFor3DMode(
                GUIGraphicsContext.Switch3DSides
                  ? GUIGraphicsContext.eRender3DModeHalf.SBSRight
                  : GUIGraphicsContext.eRender3DModeHalf.SBSLeft,
                timePassed, backbuffer, auto3DSurface,
                new Rectangle(0, 0, backbuffer.Description.Width/2, backbuffer.Description.Height));

              // right half (or left if switched)

              RenderFor3DMode(
                GUIGraphicsContext.Switch3DSides
                  ? GUIGraphicsContext.eRender3DModeHalf.SBSLeft
                  : GUIGraphicsContext.eRender3DModeHalf.SBSRight,
                timePassed, backbuffer, auto3DSurface,
                new Rectangle(backbuffer.Description.Width/2, 0, backbuffer.Description.Width/2,
                  backbuffer.Description.Height));
            }
            else
            {
              // upper half (or lower if switched)
              RenderFor3DMode(
                GUIGraphicsContext.Switch3DSides
                  ? GUIGraphicsContext.eRender3DModeHalf.TABBottom
                  : GUIGraphicsContext.eRender3DModeHalf.TABTop,
                timePassed, backbuffer, auto3DSurface,
                new Rectangle(0, 0, backbuffer.Description.Width, backbuffer.Description.Height/2));

              // lower half (or upper if switched)
              RenderFor3DMode(
                GUIGraphicsContext.Switch3DSides
                  ? GUIGraphicsContext.eRender3DModeHalf.TABTop
                  : GUIGraphicsContext.eRender3DModeHalf.TABBottom,
                timePassed, backbuffer, auto3DSurface,
                new Rectangle(0, backbuffer.Description.Height/2, backbuffer.Description.Width,
                  backbuffer.Description.Height/2));
            }

            // for a 3D movie with subtitles generated by a 3D subtitle tool, we render the subtitle here instead of in RenderLayer()

            if (!GUIGraphicsContext.Render3DSubtitle)
            {
              SubtitleRenderer.GetInstance().Render();
              SubEngine.GetInstance().Render(_subsRect, _destinationRect, 0);
            }

            GUIGraphicsContext.DX9Device.Present();
            backbuffer.Dispose();

            auto3DSurface.Dispose();
            auto3DTexture.Dispose();
          }
        }

        _debugStep = 20;
      }
      catch (DeviceLostException)
      {
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
        Log.Warn("Planescene caught DeviceLostException in InternalPresentImage");
      }
      catch (DirectXException dex)
      {
        if (dex.ErrorCode == -2005530508 || // GPU_HUNG
            dex.ErrorCode == -2005530512) // GPU_REMOVED
        {
          Log.Info("Planescene caught GPU_HUNG in InternalPresentImage");
          GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
        }
      }
      catch (Exception ex)
      {
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
        Log.Error("Planescene({0},{1},{2},{3},{4},{5},{6}):Unhandled exception in:",
          width, height, arWidth, arHeight, _textureAddress, isRepaint, _debugStep);
        Log.Error(ex);
      }
      finally
      {
        _reEntrant = false;
        GUIGraphicsContext.InVmr9Render = false;
      }
    }

    private void DrawTextureSegment(VertexBuffer vertexBuffer, float srcX, float srcY, float srcWidth, float srcHeight,
                                    float dstX, float dstY, float dstWidth, float dstHeight, long lColorDiffuse)
    {
      CustomVertex.TransformedColoredTextured[] verts =
        (CustomVertex.TransformedColoredTextured[])vertexBuffer.Lock(0, 0);

      float fVideoWidth = (float)GUIGraphicsContext.VideoSize.Width;
      float fVideoHeight = (float)GUIGraphicsContext.VideoSize.Height;
      float uoff = srcX / fVideoWidth;
      float voff = srcY / fVideoHeight;
      float umax = srcWidth / fVideoWidth;
      float vmax = srcHeight / fVideoHeight;


      // Lock the buffer (which will return our structs)
      // Top right
      verts[0].X = dstX;// - 0.5f;
      verts[0].Y = dstY + dstHeight;// - 0.5f;
      verts[0].Z = 0.0f;
      verts[0].Rhw = 1.0f;
      verts[0].Color = (int)lColorDiffuse;
      verts[0].Tu = uoff;
      verts[0].Tv = voff + vmax;

      // Top Left
      verts[1].X = dstX;// - 0.5f;
      verts[1].Y = dstY;// - 0.5f;
      verts[1].Z = 0.0f;
      verts[1].Rhw = 1.0f;
      verts[1].Color = (int)lColorDiffuse;
      verts[1].Tu = uoff;
      verts[1].Tv = voff;

      // Bottom right
      verts[2].X = dstX + dstWidth;// - 0.5f;
      verts[2].Y = dstY + dstHeight;// - 0.5f;
      verts[2].Z = 0.0f;
      verts[2].Rhw = 1.0f;
      verts[2].Color = (int)lColorDiffuse;
      verts[2].Tu = uoff + umax;
      verts[2].Tv = voff + vmax;

      // Bottom left
      verts[3].X = dstX + dstWidth;// - 0.5f;
      verts[3].Y = dstY;// - 0.5f;
      verts[3].Z = 0.0f;
      verts[3].Rhw = 1.0f;
      verts[3].Color = (int)lColorDiffuse;
      verts[3].Tu = uoff + umax;
      verts[3].Tv = voff;

      // Update vertices to compensate texel/pixel coordinate origins (top left of pixel vs. center of texel)
      // See https://msdn.microsoft.com/en-us/library/bb219690(VS.85).aspx
      for (int i = 0; i < verts.Length; i++)
      {
        verts[i].X -= 0.5f;
        verts[i].Y -= 0.5f;
      }

      vertexBuffer.Unlock();
      unsafe
      {
        GUIGraphicsContext.DX9Device.SetStreamSource(0, vertexBuffer, 0);
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
      }
    }


    private void DrawTexture(uint texAddr, long lColorDiffuse)
    {
      if (texAddr == 0)
      {
        return;
      }
      unsafe
      {
        IntPtr ptr = new IntPtr(texAddr);
        DXNative.FontEngineSetTexture(ptr.ToPointer());

        DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MINFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
        DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MAGFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
        DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MIPFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
        DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_ADDRESSU, (int)D3DTEXTUREADDRESS.D3DTADDRESS_CLAMP);
        DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_ADDRESSV, (int)D3DTEXTUREADDRESS.D3DTADDRESS_CLAMP);

        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;

        DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_ALPHABLENDENABLE, 0);

        if (_useNonLinearStretch)
        {
          //draw/stretch each partition separately according to NLS table

          //top and bottom remain untouched.
          //left and right start from the left of the rect.
          int srcLeft = _sourceRect.Left;
          float srcLeftFloat = (float)_sourceRect.Left;
          int srcRight = srcLeft;
          float srcRightFloat = (float)srcLeft;
          int dstLeft = _destinationRect.Left;
          float dstLeftFloat = _destinationRect.Left;
          int dstRight = dstLeft;
          float dstRightFloat = (float)dstLeft;
          for (int i = 0; i < nlsSourcePartitioning.Length; i++)
          {
            //this left is the previous right
            srcLeft = srcRight;
            dstLeft = dstRight;
            srcLeftFloat = srcRightFloat;
            dstLeftFloat = dstRightFloat;

            //calculate new right
            srcRightFloat = srcLeftFloat + (int)(nlsSourcePartitioning[i] * (float)_sourceRect.Width / 100.0f);
            dstRightFloat = dstLeftFloat + (int)(nlsDestPartitioning[i] * (float)_destinationRect.Width / 100.0f);
            srcRight = (int)srcRightFloat;
            dstRight = (int)dstRightFloat;


            DrawTextureSegment(_vertexBuffers[i],
                               srcLeft,
                               _sourceRect.Top,
                               srcRight - srcLeft,
                               _sourceRect.Height,
                               dstLeft,
                               _destinationRect.Top,
                               dstRight - dstLeft,
                               _destinationRect.Height,
                               lColorDiffuse);
          }
        }
        else
        {
          DrawTextureSegment(_vertexBuffers[0],
                             _sourceRect.Left,
                             _sourceRect.Top,
                             _sourceRect.Width,
                             _sourceRect.Height,
                             _destinationRect.Left,
                             _destinationRect.Top,
                             _destinationRect.Width,
                             _destinationRect.Height,
                             lColorDiffuse);
        }

        // unset the texture and palette or the texture caching crashes because the runtime still has a reference
        GUIGraphicsContext.DX9Device.SetTexture(0, null);
      }
    }

    private void RenderBlackImage(float timePassed)
    {
      // Log.Info("render black");
      try
      {
        if (_blackImage != null)
        {
          //_blackImage.SetPosition((int)_destinationRect.X, (int)_destinationRect.Y);
          //_blackImage.Width = (int)_destinationRect.Width;
          //_blackImage.Height = (int)_destinationRect.Height;
          if (GUIGraphicsContext.IsFullScreenVideo)
          {
            _blackImage.SetPosition(0, 0);
            _blackImage.Width = _geometry.ScreenWidth;
            _blackImage.Height = _geometry.ScreenHeight;
          }
          else
          {
            _blackImage.SetPosition(GUIGraphicsContext.VideoWindow.X, GUIGraphicsContext.VideoWindow.Y);
            _blackImage.Width = GUIGraphicsContext.VideoWindow.Width;
            _blackImage.Height = GUIGraphicsContext.VideoWindow.Height;
          }
          Log.Debug("RenderBlack: x:{0}, y:{1}, w:{2}, h:{3}", _blackImage.XPosition, _blackImage.YPosition,
                    _blackImage.Width, _blackImage.Height);
          if (!GUIGraphicsContext.RenderBlackImage)
          {
            return;
          }

          _blackImage.Render(timePassed);
        }
      }
      finally
      {
        GUIGraphicsContext.BlackImageRendered();
      }
    }

    public void SetSampleTime(long nsSampleTime)
    {
      SubEngine.GetInstance().SetTime(nsSampleTime);
    }

    #endregion

    #region IRenderLayer members

    public void RenderLayer(float timePassed)
    {
      // if (VideoRendererStatistics.VideoState == VideoRendererStatistics.State.VideoPresent)
      // Before , we got a black screen until GUI_MSG_NOTIFY command has finished
      // That we don't want ( do we ?)
      if (VideoRendererStatistics.VideoState == VideoRendererStatistics.State.VideoPresent ||
          VideoRendererStatistics.VideoState == VideoRendererStatistics.State.NoSignal)
      {
        if (GUIGraphicsContext.RenderBlackImage)
        {
          RenderBlackImage(timePassed);
        }

        // Render video texture
        if (_shouldRenderTexture == false)
        {
          BDOSDRenderer.GetInstance().Render();
          return;
        }

        if (_textureAddress != 0)
        {
          Rectangle originalDestination = _destinationRect;
          Rectangle originalSource = _sourceRect;

          if (GUIGraphicsContext.Render3DMode != GUIGraphicsContext.eRender3DMode.None)
          {
            if (originalDestination.Width > 512) // full size mode
            {
              switch (GUIGraphicsContext.Render3DModeHalf)
              {
                case GUIGraphicsContext.eRender3DModeHalf.SBSLeft:

                  if (!GUIGraphicsContext.IsFullHD3DFormat && 
                      GUIGraphicsContext.Render3DMode != GUIGraphicsContext.eRender3DMode.SideBySideFrom2D)
                  {
                    _sourceRect.X = originalSource.X / 2;
                    _sourceRect.Width = originalSource.Width / 2;
                  }
                  break;

                case GUIGraphicsContext.eRender3DModeHalf.SBSRight:

                  if (GUIGraphicsContext.IsFullHD3DFormat)
                  {
                    _sourceRect.X += _geometry.ImageWidth;
                  }
                  else
                  {
                    _sourceRect.X = _geometry.ImageWidth / 2 + originalSource.X / 2;
                    _sourceRect.Width = originalSource.Width / 2;
                  }
                  break;

                case GUIGraphicsContext.eRender3DModeHalf.TABTop:

                  if (!GUIGraphicsContext.IsFullHD3DFormat)
                  {
                    _sourceRect.Y = originalSource.Y;
                    _sourceRect.Height = originalSource.Height / 2;

                    // ViewModeSwitcher crop correction

                    if (GUIGraphicsContext.IsTabWithBlackBars)
                    {
                      _sourceRect.Height -= _cropSettings.Top;
                      _sourceRect.X += _cropSettings.Left;
                      _sourceRect.Width -= (_cropSettings.Left + _cropSettings.Right);
                    }
                  }
                  break;

                case GUIGraphicsContext.eRender3DModeHalf.TABBottom:

                  if (GUIGraphicsContext.IsFullHD3DFormat)
                  {
                    _sourceRect.Y += _geometry.ImageHeight;
                  }
                  else
                  {
                    _sourceRect.Y = _geometry.ImageHeight / 2 + originalSource.Y * 2;
                    _sourceRect.Height = originalSource.Height / 2;

                    // ViewModeSwitcher crop correction

                    if (GUIGraphicsContext.IsTabWithBlackBars)
                    {
                      _sourceRect.Y -= _cropSettings.Top;
                      _sourceRect.Height -= _cropSettings.Bottom;
                      _sourceRect.X += _cropSettings.Left;
                      _sourceRect.Width -= (_cropSettings.Left + _cropSettings.Right);
                    }
                  }
                  break;
              }
            }
            else // assume mini display mode 3D : Is there another way to check if target is mini display?
            {
              switch (GUIGraphicsContext.Render3DModeHalf)
              {
                case GUIGraphicsContext.eRender3DModeHalf.SBSLeft:

                  if (!GUIGraphicsContext.IsFullHD3DFormat)
                    _sourceRect.Width = originalSource.Width/2;
                  break;

                case GUIGraphicsContext.eRender3DModeHalf.SBSRight:

                  if (!GUIGraphicsContext.IsFullHD3DFormat)
                  {
                    _sourceRect.Width = originalSource.Width / 2;
                    _sourceRect.X = originalSource.Width / 2 + _sourceRect.X * 2;
                  }
                  else
                  {
                    _sourceRect.X += _geometry.ImageWidth;
                  }
                  break;

                case GUIGraphicsContext.eRender3DModeHalf.TABTop:

                  if (!GUIGraphicsContext.IsFullHD3DFormat)
                    _sourceRect.Height = originalSource.Height/2;
                  break;

                case GUIGraphicsContext.eRender3DModeHalf.TABBottom:

                  if (!GUIGraphicsContext.IsFullHD3DFormat)
                  {
                    _sourceRect.Height = originalSource.Height / 2;
                    _sourceRect.Y = originalSource.Height / 2 + _sourceRect.Y * 2;
                  }
                  else
                  {
                    _sourceRect.Y += _geometry.ImageHeight;
                  }
                  break;
              }
            }
          }
          else
          {
            if (GUIGraphicsContext.IsFullHD3DFormat)
            {
              if ((double) _prevVideoWidth/_prevVideoHeight >= 2.5) // we have Full HD SBS 
              {
                _sourceRect.Width *= 2;
              }
              else if ((double) _prevVideoWidth/_prevVideoHeight <= 1.5) // we have Full HD TAB
              {
                _sourceRect.Height *= 2;
              }
            }
          }

          DrawTexture(_textureAddress, _diffuseColor);

          _sourceRect = originalSource;
          _destinationRect = originalDestination;
        }
      }
      else
      {
        GUIGraphicsContext.RenderBlackImage = true;
        RenderBlackImage(timePassed);
        GUIGraphicsContext.RenderBlackImage = false;
      }

      SubtitleRenderer.GetInstance().Render();

      if (GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.None)
      {
        // for a 2D movie we render the subtitles here

        SubEngine.GetInstance().Render(_subsRect, _destinationRect, 0);
      }
      else if (((GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.SBSLeft ||
                GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.TABTop) && !GUIGraphicsContext.Switch3DSides) ||
               ((GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.SBSRight ||
                GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.TABBottom) && GUIGraphicsContext.Switch3DSides))
      {
        // for a 3D movie we render the left/top frame subtitle here
        // if Render3DSubtitle is turned off, rendering takes place in InternalPresentImage()
        // this helps to avoid doubling of subtitles that are generated by external tools

        if (GUIGraphicsContext.Render3DSubtitle)
        {
            if (!GUIGraphicsContext.StretchSubtitles)
                SubEngine.GetInstance().Render(_subsRect, _destinationRect, 0);
            else
            {
                Rectangle dstRect = _destinationRect;

                if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide || GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideTo2D)
                    dstRect.Width *= 2;
                else
                    dstRect.Height *= 2;
                
                SubEngine.GetInstance().Render(_subsRect, dstRect, 0);
            }
        }
      }
      else if (((GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.SBSRight ||
               GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.TABBottom) && !GUIGraphicsContext.Switch3DSides) ||
              ((GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.SBSLeft ||
               GUIGraphicsContext.Render3DModeHalf == GUIGraphicsContext.eRender3DModeHalf.TABTop) && GUIGraphicsContext.Switch3DSides))
      {
        // for a 3D movie we render the right/bottom frame subtitle here
        // if Render3DSubtitle is turned off, rendering takes place in InternalPresentImage()
        // this helps to avoid doubling of subtitles that are generated by external tools

        if (GUIGraphicsContext.Render3DSubtitle)
        {
          Rectangle subRect = _subsRect;
          Rectangle dstRect = _destinationRect;

          if (GUIGraphicsContext.StretchSubtitles)
          {
              if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide || GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideTo2D)
                dstRect.Width *= 2;
              else
                dstRect.Height *= 2;
          }

          subRect.X += GUIGraphicsContext.Render3DSubtitleDistance;
          dstRect.X += GUIGraphicsContext.Render3DSubtitleDistance;

          SubEngine.GetInstance().Render(subRect, dstRect, 0);
        }
      }

      BDOSDRenderer.GetInstance().Render();
    }

    public bool ShouldRenderLayer()
    {
      return true;
    }

    #endregion
  }

  //public class PlaneScene 
}

//namespace MediaPortal.Player 