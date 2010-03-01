#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.GUI.Library;
using MediaPortal.Player.Subtitles;
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
    #region imports


    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawSurface(int fx, int fy, int nw, int nh,
                                                            int dstX, int dstY, int dstWidth, int dstHeight,
                                                            void* surface);

    #endregion

    #region variables

    private bool _stopPainting = false;
    private Surface _renderTarget = null;
    private bool _fadingIn = true;
    private float _uValue = 1.0f;
    private float _vValue = 1.0f;
    private Rectangle _rectPrevious;
    private Rectangle _subsRect;
    private bool _shouldRenderTexture = false;
    private bool _lastOverlayVisible = false;
    private bool _isEnabled = true;
    private Geometry.Type _aspectRatioType;
    private Rectangle _sourceRect, _destinationRect;
    private Geometry _geometry = new Geometry();
    private VMR9Util _vmr9Util = null;
    private uint _surfaceAddress;

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

    #endregion

    #region ctor

    public PlaneScene(VMR9Util util)
    {
      //	Log.Info("PlaneScene: ctor()");

      _surfaceAddress = 0;
      _vmr9Util = util;
      
      _blackImage = new GUIImage(0);
      _blackImage.SetFileName("black.png");
      _blackImage.AllocResources();

      _cropSettings = new CropSettings();
    }

    #endregion

    #region properties

    /// <summary>
    /// Returns a rectangle specifing the part of the video texture which is 
    /// shown
    /// </summary>
    public Rectangle SourceRect
    {
      get { return _sourceRect; }
    }

    /// <summary>
    /// Returns a rectangle specifing the video window onscreen
    /// </summary>
    public Rectangle DestRect
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

    public bool InTv
    {
      get
      {
        int windowId = GUIWindowManager.ActiveWindow;
        GUIWindow window = GUIWindowManager.GetWindow(windowId);
        if (window.IsTv)
        {
          return true;
        }
        if (windowId == (int)GUIWindow.Window.WINDOW_TV ||
            windowId == (int)GUIWindow.Window.WINDOW_TVGUIDE ||
            windowId == (int)GUIWindow.Window.WINDOW_SEARCHTV ||
            windowId == (int)GUIWindow.Window.WINDOW_TELETEXT ||
            windowId == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            windowId == (int)GUIWindow.Window.WINDOW_SCHEDULER ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES ||
            windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTV ||
            windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTVCHANNEL ||
            windowId == (int)GUIWindow.Window.WINDOW_RECORDEDTVGENRE ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_CONFLICTS ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_MAIN ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_AUTO ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_NO_SIGNAL ||
            windowId == (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO)
        {
          return true;
        }
        return false;
      }
    }

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
      if (_renderTarget != null)
      {
        //VMR9 changes the directx 9 render target. Thats why we set it back to what it was
        if (!_renderTarget.Disposed)
        {
          GUIGraphicsContext.DX9Device.SetRenderTarget(0, _renderTarget);
        }
        _renderTarget.Dispose();
        _renderTarget = null;
      }

      if (_blackImage != null)
      {
        _blackImage.FreeResources();
        _blackImage = null;
      }

      grabber.Clean();
      SubtitleRenderer.GetInstance().Clear();
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
      _renderTarget = GUIGraphicsContext.DX9Device.GetRenderTarget(0);
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Video);
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnMessage);
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
      try
      {
        GUIGraphicsContext.VideoSize = videoSize;
        // get the window where the video/tv should be shown
        float x = GUIGraphicsContext.VideoWindow.X;
        float y = GUIGraphicsContext.VideoWindow.Y;
        float nw = GUIGraphicsContext.VideoWindow.Width;
        float nh = GUIGraphicsContext.VideoWindow.Height;

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
        else
        {
          // we're in preview mode. Check if we are in the tv module

          if (!InTv)
          {
            //we are not in the my tv module
            //then check if a VideoWindow is defined or video/tv preview window is enable
            Rectangle rect = GUIGraphicsContext.VideoWindow;
            //BAV: todo -> remove Overlay check -> should no longer be needed
            //if (((rect.Height < 1) && (rect.Width < 1)) || (!GUIGraphicsContext.Overlay)) return false; //not enabled, dont show tv
            if ((rect.Height < 1) && (rect.Width < 1))
            {
              return false;
            }
          }
        }

        //sanity check
        if (nw <= 10 || nh <= 10)
        {
          return false;
        }
        if (x < 0 || y < 0)
        {
          return false;
        }

        GUIGraphicsContext.VideoReceived();

        //did the video window,aspect ratio change? if not
        //then we dont need to recalculate and just return the previous settings
        if (!updateCrop && x == _rectPrevious.X && y == _rectPrevious.Y &&
            nw == _rectPrevious.Width && nh == _rectPrevious.Height &&
            GUIGraphicsContext.ARType == _aspectRatioType &&
            GUIGraphicsContext.Overlay == _lastOverlayVisible && _shouldRenderTexture &&
            _prevVideoWidth == videoSize.Width && _prevVideoHeight == videoSize.Height &&
            _prevArVideoWidth == _arVideoWidth && _prevArVideoHeight == _arVideoHeight)
        {
          //not changed, return previous settings
          return _shouldRenderTexture;
        }

        //settings (position,size,aspect ratio) changed.
        //Store these settings and start calucating the new video window
        _rectPrevious = new Rectangle((int)x, (int)y, (int)nw, (int)nh);
        _subsRect = _rectPrevious;
        _aspectRatioType = GUIGraphicsContext.ARType;
        _lastOverlayVisible = GUIGraphicsContext.Overlay;
        _prevVideoWidth = videoSize.Width;
        _prevVideoHeight = videoSize.Height;
        _prevArVideoWidth = _arVideoWidth;
        _prevArVideoHeight = _arVideoHeight;

        //calculate the video window according to the current aspect ratio settings
        float fVideoWidth = (float)videoSize.Width;
        float fVideoHeight = (float)videoSize.Height;
        _geometry.ImageWidth = (int)fVideoWidth;
        _geometry.ImageHeight = (int)fVideoHeight;
        _geometry.ScreenWidth = (int)nw;
        _geometry.ScreenHeight = (int)nh;
        _geometry.ARType = GUIGraphicsContext.ARType;
        _geometry.PixelRatio = GUIGraphicsContext.PixelRatio;

        _geometry.GetWindow(_arVideoWidth, _arVideoHeight, out _sourceRect, out _destinationRect,
                            out _useNonLinearStretch, _cropSettings);
        updateCrop = false;
        _destinationRect.X += (int)x;
        _destinationRect.Y += (int)y;

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

        return true;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }
    }

    /// <summary>
    /// This method repaints a paused video
    /// </returns>
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
        if (!GUIGraphicsContext.InVmr9Render)
        {
          if (_surfaceAddress != 0)
          {
            InternalPresentImage(_vmr9Util.VideoWidth, _vmr9Util.VideoHeight, _arVideoWidth, _arVideoHeight, true);
          }
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

    /// <summary>
    /// Callback method that is called by the VMR9 /EVR renderer
    /// </returns>
    public int PresentImage(Int16 width, Int16 height, Int16 arWidth, Int16 arHeight, uint pTexture, uint pSurface)
    {
      try
      {
        // Alert the frame grabber that it has a chance to grab a frame
        // if it likes (method returns immediatly otherwise
        grabber.OnFrame(width, height, arWidth, arHeight, pSurface);

        _surfaceAddress = pSurface;

        if (pSurface == 0)
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
        InternalPresentImage(width, height, arWidth, arHeight, false);
        //			Log.Info("vmr9:present image() done");
      }
      catch (Exception ex)
      {
        Log.Error("Planescene: Error in PresentImage - {0}", ex.ToString());
      }
      return 0;
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
        if (GUIWindowManager.IsSwitchingToNewWindow)
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


        _debugStep = 3;
        //get desired video window
        if (width > 0 && height > 0 && _surfaceAddress != 0)
        {
          Size nativeSize = new Size(width, height);
          _shouldRenderTexture = SetVideoWindow(nativeSize);
        }
        else
        {
          _shouldRenderTexture = false;
        }

        //clear screen
        GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);

        _debugStep = 5;
        GUIGraphicsContext.DX9Device.BeginScene();
        try
        {
          if (!GUIGraphicsContext.BlankScreen)
          {
            // Render GUI + Video surface
            GUIGraphicsContext.RenderGUI.RenderFrame(timePassed);
            GUIFontManager.Present();
          }
        }
        finally
        {
          GUIGraphicsContext.DX9Device.EndScene();
        }

        GUIGraphicsContext.DX9Device.Present();
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
                  width, height, arWidth, arHeight, _surfaceAddress, isRepaint, _debugStep);
        Log.Error(ex);
      }
      finally
      {
        _reEntrant = false;
        GUIGraphicsContext.InVmr9Render = false;
      }
    }


    private void DrawSurfaceSegment(uint surfaceAddr, float srcX, float srcY, float srcWidth, float srcHeight,
                                    float dstX, float dstY, float dstWidth, float dstHeight)
    {
      unsafe
      {
        IntPtr ptr = new IntPtr(surfaceAddr);
        FontEngineDrawSurface((int)srcX, (int)srcY, (int)srcWidth, (int)srcHeight, (int)dstX, (int)dstY, (int)dstWidth, (int)dstHeight, ptr.ToPointer());
      }
    }


    private void DrawSurface(uint surfaceAddr)
    {
      if (surfaceAddr == 0)
      {
        return;
      }
      unsafe
      {

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


            DrawSurfaceSegment(surfaceAddr,
                               srcLeft,
                               _sourceRect.Top,
                               srcRight - srcLeft,
                               _sourceRect.Height,
                               dstLeft,
                               _destinationRect.Top,
                               dstRight - dstLeft,
                               _destinationRect.Height);
          }
        }
        else
        {
          DrawSurfaceSegment(surfaceAddr,
                             _sourceRect.Left,
                             _sourceRect.Top,
                             _sourceRect.Width,
                             _sourceRect.Height,
                             _destinationRect.Left,
                             _destinationRect.Top,
                             _destinationRect.Width,
                             _destinationRect.Height);
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

        //Render video texture
        if (_shouldRenderTexture == false)
        {
          return;
        }

        if (_surfaceAddress != 0)
        {
          DrawSurface(_surfaceAddress);
        }
      }
      else
      {
        GUIGraphicsContext.RenderBlackImage = true;
        RenderBlackImage(timePassed);
        GUIGraphicsContext.RenderBlackImage = false;
      }
      SubtitleRenderer.GetInstance().Render();
      SubEngine.GetInstance().Render(_subsRect, _destinationRect);
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