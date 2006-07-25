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
 */
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
//using DirectX.Capture;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Utils.Services;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;
using DShowNET.TsFileSink;

namespace MediaPortal.Player
{
  public class BaseTStreamBufferPlayer : IPlayer
  {
    [ComImport, Guid("4F8BF30C-3BEB-43A3-8BF2-10096FD28CF2")]
    protected class TsFileSource { }
    #region imports
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxerPin(IPin pin, int pid, int elementaryStream, bool unmapOtherPins);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int DumpMpeg2DemuxerMappings(IBaseFilter filter);
    #endregion

    #region enums
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }
    #endregion

    #region variables
    protected int iSpeed = 1;
    protected TsFileSource _fileSource = null;


    protected int _positionX = 0;
    protected int _positionY = 0;
    protected int _width = 200;
    protected int _height = 100;
    protected int _videoWidth = 100;
    protected int _videoHeight = 100;
    protected string _currentFile = "";
    protected bool _updateNeeded = false;
    protected bool _isFullscreen = true;
    protected PlayState _state = PlayState.Init;
    protected int _volume = 100;
    protected IGraphBuilder _graphBuilder = null;
    protected IMediaSeeking _mediaSeeking = null;
    protected int _speed = 1;
    protected double _currentPos;
    protected double _contentStart;
    protected double _duration = -1d;
    protected bool _isStarted = false;
    protected bool _isLive = false;
    protected double _lastPosition = 0;
    protected bool _isWindowVisible = false;
    protected DsROTEntry _rotEntry = null;
    protected int _aspectX = 1;
    protected int _aspectY = 1;
    protected long _speedRate = 10000;
    bool _isUsingNvidiaCodec = false;
    protected ITSFileSource _interfaceTsFileSource = null;
    protected IBaseFilter _videoCodecFilter = null;
    protected IBaseFilter _audioCodecFilter = null;
    protected IBaseFilter _audioRendererFilter = null;
    protected IBaseFilter _ffdShowFilter = null;
    /// <summary> control interface. </summary>
    protected IMediaControl _mediaCtrl = null;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx _mediaEvt = null;


    /// <summary> video preview window interface. </summary>
    protected IVideoWindow _videoWin = null;

    /// <summary> interface to get information and control video. </summary>
    protected IBasicVideo2 _basicVideo = null;

    /// <summary> audio interface used to control volume. </summary>
    protected IBasicAudio _basicAudio = null;
    protected IBaseFilter _mpegDemux;
    VMR7Util _vmr7 = null;
    DateTime _elapsedTimer = DateTime.Now;

    protected const int WM_GRAPHNOTIFY = 0x00008001;	// message from graph
    protected const int WS_CHILD = 0x40000000;	// attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;
    protected DateTime _updateTimer = DateTime.Now;
    protected MediaPortal.GUI.Library.Geometry.Type _geometry = MediaPortal.GUI.Library.Geometry.Type.Normal;

    protected bool _startingUp;
    protected ILog _log;
    protected bool _isRadio = false;
    #endregion

    #region ctor/dtor
    public BaseTStreamBufferPlayer()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }
    public BaseTStreamBufferPlayer(g_Player.MediaType type)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
      _isRadio = false;
      if (type == g_Player.MediaType.Radio)
      {
        _isRadio = true;
      }
    }
    #endregion

    #region public members

    public override bool SupportsReplay
    {
      get
      {
        return false;
      }
    }
    public override bool Play(string strFile)
    {
      _log.Info("Streambufferplayer play:{0}", strFile);
      if (!System.IO.File.Exists(strFile))
        return false;
      iSpeed = 1;
      _speedRate = 10000;
      _isLive = false;
      _duration = -1d;
      if (strFile.ToLower().IndexOf(".tsbuffer") >= 0)
      {
        _log.Info("Streambufferplayer: live tv");
        _isLive = true;
      }
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;




      _isVisible = false;
      _isWindowVisible = false;
      _volume = 100;
      _state = PlayState.Init;
      _currentFile = strFile;
      _isFullscreen = false;
      _geometry = MediaPortal.GUI.Library.Geometry.Type.Normal;

      _updateNeeded = true;
      //if (_fileSource != null)
      //{
      //  Log.Write("TSStreamBufferPlayer:replay {0}", strFile);
      //  IFileSourceFilter interFaceFile = (IFileSourceFilter)_fileSource;
      //  interFaceFile.Load(strFile, null);
      //}
      //else
      {
        _log.Info("TSStreamBufferPlayer:play {0}", strFile);
        GC.Collect();
        CloseInterfaces();
        GC.Collect();
        _isStarted = false;
        if (!GetInterfaces(strFile))
        {
          _log.Error("TSStreamBufferPlayer:GetInterfaces() failed");
          _currentFile = "";
          return false;
        }
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);


      }

      int hr = _mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
      if (hr < 0)
      {
        _log.Error("TSStreamBufferPlayer:SetNotifyWindow() failed");
        _currentFile = "";
        CloseInterfaces();
        return false;
      }
      if (_videoWin != null)
      {
        _videoWin.put_Owner(GUIGraphicsContext.ActiveForm);
        _videoWin.put_WindowStyle((WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipSiblings + (int)WindowStyle.ClipChildren));
        _videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);

      }
      if (_basicVideo != null)
      {
        hr = _basicVideo.GetVideoSize(out _videoWidth, out _videoHeight);
        if (hr < 0)
        {
          _log.Error("TSStreamBufferPlayer:GetVideoSize() failed");
          _currentFile = "";
          CloseInterfaces();
          return false;
        }
        _log.Info("TSStreamBufferPlayer:VideoSize:{0}x{1}", _videoWidth, _videoHeight);
      }
      GUIGraphicsContext.VideoSize = new Size(_videoWidth, _videoHeight);

      if (_mediaCtrl == null)
      {
        _log.Error("TSStreamBufferPlayer:_mediaCtrl==null");
        _currentFile = "";
        CloseInterfaces();
        return false;
      }

      //DsUtils.DumpFilters(_graphBuilder);

      _positionX = GUIGraphicsContext.VideoWindow.X;
      _positionY = GUIGraphicsContext.VideoWindow.Y;
      _width = GUIGraphicsContext.VideoWindow.Width;
      _height = GUIGraphicsContext.VideoWindow.Height;
      _geometry = GUIGraphicsContext.ARType;
      _updateNeeded = true;
      SetVideoWindow();

      _interfaceTsFileSource = _fileSource as ITSFileSource;

      _startingUp = _isLive;

      DirectShowUtil.EnableDeInterlace(_graphBuilder);
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
      msg.Label = strFile;
      GUIWindowManager.SendThreadMessage(msg);

      _state = PlayState.Playing;

      _log.Info("TsBaseStreamBuffer:start graph");
      _mediaCtrl.Run();

      _log.Info("TsBaseStreamBuffer:playing state");
      OnInitialized();
      _log.Info("TsBaseStreamBuffer:done");
      return true;
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullscreen)
      {
        _isFullscreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }

      if (!_updateNeeded)
        return;

      _updateNeeded = false;
      _isStarted = true;
      float x = _positionX;
      float y = _positionY;

      int nw = _width;
      int nh = _height;
      if (nw > GUIGraphicsContext.OverScanWidth)
        nw = GUIGraphicsContext.OverScanWidth;
      if (nh > GUIGraphicsContext.OverScanHeight)
        nh = GUIGraphicsContext.OverScanHeight;
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        x = _positionX = GUIGraphicsContext.OverScanLeft;
        y = _positionY = GUIGraphicsContext.OverScanTop;
        nw = _width = GUIGraphicsContext.OverScanWidth;
        nh = _height = GUIGraphicsContext.OverScanHeight;
      }
      /*			_log.Info("{0},{1}-{2},{3}  vidwin:{4},{5}-{6},{7} fs:{8}", x,y,nw,nh, 
              GUIGraphicsContext.VideoWindow.Left,
              GUIGraphicsContext.VideoWindow.Top,
              GUIGraphicsContext.VideoWindow.Right,
              GUIGraphicsContext.VideoWindow.Bottom,
              GUIGraphicsContext.IsFullScreenVideo);*/
      if (nw <= 0 || nh <= 0)
        return;
      if (x < 0 || y < 0)
        return;


      int aspectX, aspectY;
      if (_basicVideo != null)
      {
        _basicVideo.GetVideoSize(out _videoWidth, out _videoHeight);
      }
      aspectX = _videoWidth;
      aspectY = _videoHeight;
      if (_basicVideo != null)
      {
        _basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
      }
      GUIGraphicsContext.VideoSize = new Size(_videoWidth, _videoHeight);
      _aspectX = aspectX;
      _aspectY = aspectY;
      MediaPortal.GUI.Library.Geometry m_geometry = new MediaPortal.GUI.Library.Geometry();
      System.Drawing.Rectangle rSource, rDest;
      m_geometry.ImageWidth = _videoWidth;
      m_geometry.ImageHeight = _videoHeight;
      m_geometry.ScreenWidth = nw;
      m_geometry.ScreenHeight = nh;
      m_geometry.ARType = GUIGraphicsContext.ARType;
      m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
      m_geometry.GetWindow(aspectX, aspectY, out rSource, out rDest);

      rDest.X += (int)x;
      rDest.Y += (int)y;

      _log.Info("overlay: video WxH  : {0}x{1}", _videoWidth, _videoHeight);
      _log.Info("overlay: video AR   : {0}:{1}", aspectX, aspectY);
      _log.Info("overlay: screen WxH : {0}x{1}", nw, nh);
      _log.Info("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
      _log.Info("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
      _log.Info("overlay: src        : ({0},{1})-({2},{3})",
        rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
      _log.Info("overlay: dst        : ({0},{1})-({2},{3})",
        rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);


      _log.Info("TSStreamBufferPlayer:Window ({0},{1})-({2},{3}) - ({4},{5})-({6},{7})",
        rSource.X, rSource.Y, rSource.Right, rSource.Bottom,
        rDest.X, rDest.Y, rDest.Right, rDest.Bottom);


      if (rSource.Y == 0)
      {
        rSource.Y += 5;
        rSource.Height -= 10;
      }
      SetSourceDestRectangles(rSource, rDest);
      SetVideoPosition(rDest);
      _sourceRectangle = rSource;
      _videoRectangle = rDest;
    }

    public override bool Ended
    {
      get { return _state == PlayState.Ended; }
    }
    

    public override void Process()
    {
      if (!Playing)
        return;
      if (GUIGraphicsContext.InVmr9Render)
        return;

      //_log.Info("1");
      
      if (_startingUp && _isLive)
      {
        ushort pgmCount=0;
        ushort pgmNumber = 0;
        ushort audioPid=0;
        ushort videoPid = 0;
        ushort pcrPid = 0;
        long duration = 0;
        ITSFileSource tsInterface = _fileSource as ITSFileSource;
        tsInterface.GetAudioPid(ref audioPid);
        tsInterface.GetVideoPid(ref videoPid);
        tsInterface.GetPCRPid(ref pcrPid);
        tsInterface.GetPgmCount(ref pgmCount);
        tsInterface.GetDuration(ref duration);
        if (pgmCount > 0)
        {
          tsInterface.GetPgmNumb(ref pgmNumber);
          _log.Info("programs:{0} duration:{1} current pgm:{2}", pgmCount, duration, pgmNumber);
          _log.Info("video:{0:X} audio:{1:X} pcr:{2:X}", videoPid, audioPid, pcrPid);
          if (pgmNumber != 1)
          {
            tsInterface.SetPgmNumb(1);
            _log.Info("selected prgm number 1");
          }
          _startingUp = false;
          /*
          UpdateDuration();
          if (_duration > 0)
          {
            _log.Info("duration:{0}",_duration);
            double pos = _duration - 2;
            if (pos > 0)
            {
              if (VMR9Util.g_vmr9 != null)
              {
                VMR9Util.g_vmr9.FrameCounter = 123;
                VMR9Util.g_vmr9.Process();
              }
              SeekAbsolute(_duration);

              if (VMR9Util.g_vmr9 != null)
              {
                VMR9Util.g_vmr9.FrameCounter = 123;
                VMR9Util.g_vmr9.Process();
              }
            }
            _startingUp = false;
          }*/
        }
        /*
        IAMStreamSelect control = _fileSource as IAMStreamSelect;
        if (control != null)
        {
          int streamCount = 0;
          control.Count(out streamCount);
          if (streamCount > 0)
          {
            UpdateDuration();
            _log.Info("streams:{0} duration:{1}", streamCount, _duration);
            if (_duration > 1)
            {
              _log.Info("enable stream 1");
              control.Enable(1, AMStreamSelectEnableFlags.EnableAll);
              _log.Info("get duration", streamCount);
              UpdateDuration();
              double dPos = _duration;
              _log.Info("_duration:{0}", _duration);
              if (dPos >= 0 && CurrentPosition < dPos)
              {
                _log.Info("seek:{0}/{0}", dPos, _duration);
                SeekAbsolute(dPos);
                _log.Info("seek:{0}/{0} done", dPos, _duration);
              }
              _startingUp = false;
            }
          }
        }*/
      }
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds >= 800 || iSpeed != 1)
      {
        UpdateCurrentPosition();
        UpdateDuration();

        _updateTimer = DateTime.Now;
      }

      if (IsTimeShifting)
      {
        if (Speed > 1 && CurrentPosition + 5d >= Duration)
        {
          _log.Info("BaseTSStreamBufferPlayer: stop FFWD since end of timeshiftbuffer reached");
          Speed = 1;
          SeekAsolutePercentage(99);
        }
        if (Speed < 0 && CurrentPosition < 5d)
        {
          _log.Info("BaseTSStreamBufferPlayer: stop RWD since begin of timeshiftbuffer reached");
          Speed = 1;
          SeekAsolutePercentage(0);
        }/*
				if (Speed<0 && CurrentPosition > _lastPosition)
				{
					Speed=1;
					SeekAsolutePercentage(0);
				}*/
      }
      _lastPosition = CurrentPosition;


      if (GUIGraphicsContext.VideoWindow.Width <= 10 && GUIGraphicsContext.IsFullScreenVideo == false)
      {
        _isVisible = false;
      }
      if (GUIGraphicsContext.BlankScreen)
      {
        _isVisible = false;
      }
      if (_isWindowVisible && !_isVisible)
      {
        _isWindowVisible = false;
        //_log.Info("TSStreamBufferPlayer:hide window");
        if (_videoWin != null)
          _videoWin.put_Visible(OABool.False);
      }
      else if (!_isWindowVisible && _isVisible)
      {
        _isWindowVisible = true;
        //_log.Info("TSStreamBufferPlayer:show window");
        if (_videoWin != null)
          _videoWin.put_Visible(OABool.True);
      }

      OnProcess();
      CheckVideoResolutionChanges();
      if (_speedRate != 10000)
      {
        DoFFRW();
      }
      //_log.Info("2");
    }
    public override bool IsTV
    {
      get
      {
        return true;
      }
    }
    public override bool IsTimeShifting
    {
      get { return _isLive; }
    }

    public override bool Visible
    {
      get { return _isVisible; }
      set
      {
        if (value == _isVisible)
          return;
        _isVisible = value;
        _updateNeeded = true;
      }
    }

    public override int Speed
    {
      get
      {
        if (_state == PlayState.Init)
          return 1;
        if (_mediaSeeking == null)
          return 1;
        if (_isUsingNvidiaCodec)
          return iSpeed;
        switch (_speedRate)
        {
          case -10000:
            return -1;
          case -15000:
            return -2;
          case -30000:
            return -4;
          case -45000:
            return -8;
          case -60000:
            return -16;
          case -75000:
            return -32;

          case 10000:
            return 1;
          case 15000:
            return 2;
          case 30000:
            return 4;
          case 45000:
            return 8;
          case 60000:
            return 16;
          default:
            return 32;
        }
      }
      set
      {
        if (_state != PlayState.Init)
        {
          if (_isUsingNvidiaCodec)
          {
            if (iSpeed != value)
            {
              iSpeed = value;
              int hr = _mediaSeeking.SetRate((double)iSpeed);
              //_log.Info("VideoPlayer:SetRate to:{0} {1:X}", iSpeed,hr);
              if (hr != 0)
              {
                IMediaSeeking oldMediaSeek = _graphBuilder as IMediaSeeking;
                hr = oldMediaSeek.SetRate((double)iSpeed);
                //_log.Info("VideoPlayer:SetRateOld to:{0} {1:X}", iSpeed,hr);
              }
              if (iSpeed == 1)
              {
                _mediaCtrl.Stop();
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(200);
                System.Windows.Forms.Application.DoEvents();
                FilterState state;
                _mediaCtrl.GetState(100, out state);
                //_log.Info("state:{0}", state.ToString());
                _mediaCtrl.Run();
              }
            }
          }
          else
          {
            switch ((int)value)
            {
              case -1:
                _speedRate = -10000;
                break;
              case -2:
                _speedRate = -15000;
                break;
              case -4:
                _speedRate = -30000;
                break;
              case -8:
                _speedRate = -45000;
                break;
              case -16:
                _speedRate = -60000;
                break;
              case -32:
                _speedRate = -75000;
                break;

              case 1:
                _speedRate = 10000;
                _mediaCtrl.Run();
                break;
              case 2:
                _speedRate = 15000;
                break;
              case 4:
                _speedRate = 30000;
                break;
              case 8:
                _speedRate = 45000;
                break;
              case 16:
                _speedRate = 60000;
                break;
              default:
                _speedRate = 75000;
                break;
            }
          }
        }
      }
    }


    public override void ContinueGraph()
    {
      if (_mediaCtrl == null)
        return;
      _log.Info("TSStreamBufferPlayer:Continue graph");
      _mediaCtrl.Run();
      _mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);

      if (VMR9Util.g_vmr9 != null)
        VMR9Util.g_vmr9.Enable(true);
    }

    public override void PauseGraph()
    {
      if (_mediaCtrl == null)
        return;
      _log.Info("TSStreamBufferPlayer:Pause graph");
      _mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
      _mediaCtrl.Pause();
      if (VMR9Util.g_vmr9 != null)
        VMR9Util.g_vmr9.Enable(false);
    }

    public override void WndProc(ref Message m)
    {
      if (m.Msg == WM_GRAPHNOTIFY)
      {
        if (_mediaEvt != null)
          OnGraphNotify();
        return;
      }
      base.WndProc(ref m);
    }


    public override int PositionX
    {
      get { return _positionX; }
      set
      {
        if (value != _positionX)
        {
          _positionX = value;
          _updateNeeded = true;
        }
      }
    }

    public override int PositionY
    {
      get { return _positionY; }
      set
      {
        if (value != _positionY)
        {
          _positionY = value;
          _updateNeeded = true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return _width; }
      set
      {
        if (value != _width)
        {
          _width = value;
          _updateNeeded = true;
        }
      }
    }
    public override int RenderHeight
    {
      get { return _height; }
      set
      {
        if (value != _height)
        {
          _height = value;
          _updateNeeded = true;
        }
      }
    }

    public override double Duration
    {
      get
      {
        if (_duration < 0)
          Process();
        return _duration;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        return _currentPos;
      }
    }

    public override double ContentStart
    {
      get { return _contentStart; }
    }
    public override bool FullScreen
    {
      get
      {
        return GUIGraphicsContext.IsFullScreenVideo;
      }
      set
      {
        if (value != _isFullscreen)
        {
          _isFullscreen = value;
          _updateNeeded = true;
        }
      }
    }
    public override int Width
    {
      get
      {
        return _videoWidth;
      }
    }

    public override int Height
    {
      get
      {
        return _videoHeight;
      }
    }

    public override void Pause()
    {
      if (_state == PlayState.Paused)
      {
        //_log.Info("BaseTSStreamBufferPlayer:resume");
        Speed = 1;
        _mediaCtrl.Run();
        _state = PlayState.Playing;
      }
      else if (_state == PlayState.Playing)
      {
        //_log.Info("BaseTSStreamBufferPlayer:pause");
        _state = PlayState.Paused;
        _mediaCtrl.Pause();
      }
    }

    public override bool Paused
    {
      get
      {
        return (_state == PlayState.Paused);
      }
    }

    public override bool Playing
    {
      get
      {
        return (_state == PlayState.Playing || _state == PlayState.Paused);
      }
    }

    public override bool Stopped
    {
      get
      {
        return (_state == PlayState.Init);
      }
    }

    public override string CurrentFile
    {
      get { return _currentFile; }
    }

    public override void Stop()
    {
      if (SupportsReplay)
      {
        _log.Info("TSStreamBufferPlayer:stop");
        if (_mediaCtrl == null)
          return;
        _mediaCtrl.Stop();
      }
      else
      {
        CloseInterfaces();
      }
    }


    public override int Volume
    {
      get { return _volume; }
      set
      {
        if (_volume != value)
        {
          _volume = value;
          if (_state != PlayState.Init)
          {
            if (_basicAudio != null)
            {
              // Divide by 100 to get equivalent decibel value. For example, –10,000 is –100 dB. 
              float fPercent = (float)_volume / 100.0f;
              int iVolume = (int)(5000.0f * fPercent);
              _basicAudio.put_Volume((iVolume - 5000));
            }
          }
        }
      }
    }

    public override MediaPortal.GUI.Library.Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType; }
      set
      {
        if (_geometry != value)
        {
          _geometry = value;
          _updateNeeded = true;
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          double dCurTime = this.CurrentPosition;

          dTime = dCurTime + dTime;
          if (dTime < 0.0d)
            dTime = 0.0d;
          if (dTime < Duration)
          {
            SeekAbsolute(dTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double dTimeInSecs)
    {
      _log.Info("SeekAbsolute:seekabs:{0}", dTimeInSecs);


      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          if (dTimeInSecs < 0.0d)
            dTimeInSecs = 0.0d;
          if (dTimeInSecs > Duration)
            dTimeInSecs = Duration;
          dTimeInSecs = Math.Floor(dTimeInSecs);
          _log.Info("TSStreamBufferPlayer: seekabs: {0} duration:{1} current pos:{2}", dTimeInSecs, Duration, CurrentPosition);
          dTimeInSecs *= 10000000d;
          long pStop = 0;
          long lContentStart, lContentEnd;
          double fContentStart, fContentEnd;
          _log.Info("get available");
          _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
          _log.Info("get available done");
          fContentStart = lContentStart;
          fContentEnd = lContentEnd;

          dTimeInSecs += fContentStart;
          long lTime = (long)dTimeInSecs;
          _log.Info("set positions");
          int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          _log.Info("set positions done");
          if (hr != 0)
          {
            _log.Error("seek failed->seek to 0 0x:{0:X}", hr);
          }
        }
        UpdateCurrentPosition();
        _log.Info("TSStreamBufferPlayer: current pos:{0}", CurrentPosition);

      }
    }


    public override void SeekRelativePercentage(int iPercentage)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          double dCurrentPos = this.CurrentPosition;
          double dDuration = Duration;

          double fCurPercent = (dCurrentPos / Duration) * 100.0d;
          double fOnePercent = Duration / 100.0d;
          fCurPercent = fCurPercent + (double)iPercentage;
          fCurPercent *= fOnePercent;
          if (fCurPercent < 0.0d)
            fCurPercent = 0.0d;
          if (fCurPercent < Duration)
          {
            SeekAbsolute(fCurPercent);
          }
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {

          if (iPercentage < 0)
            iPercentage = 0;
          if (iPercentage >= 100)
            iPercentage = 100;
          double fPercent = Duration / 100.0f;
          fPercent *= (double)iPercentage;
          SeekAbsolute(fPercent);
        }
      }
    }


    public override bool IsRadio
    {
      get
      {
        return _isRadio;
      }
    }
    public override bool HasVideo
    {
      get { return (_isRadio==false); }
    }


    #endregion

    #region private/protected members

    protected virtual void SetVideoPosition(System.Drawing.Rectangle rDest)
    {
      if (_videoWin != null)
      {
        if (rDest.Left < 0 || rDest.Top < 0 || rDest.Width <= 0 || rDest.Height <= 0)
          return;
        _videoWin.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
    }

    protected virtual void SetSourceDestRectangles(System.Drawing.Rectangle rSource, System.Drawing.Rectangle rDest)
    {
      if (_basicVideo != null)
      {
        if (rSource.Left < 0 || rSource.Top < 0 || rSource.Width <= 0 || rSource.Height <= 0)
          return;
        if (rDest.Width <= 0 || rDest.Height <= 0)
          return;
        _basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
        _basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
      }
    }

    void MovieEnded()
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped

      //_log.Info("TSStreamBufferPlayer:ended {0}", _currentFile);
      if (!IsTimeShifting)
      {
        CloseInterfaces();
        _state = PlayState.Ended;
      }
      else
      {
        SeekAsolutePercentage(99);
      }
    }
    void CheckVideoResolutionChanges()
    {
      if (_videoWin == null || _basicVideo == null)
        return;
      int aspectX, aspectY;
      int videoWidth = 1, videoHeight = 1;
      if (_basicVideo != null)
      {
        _basicVideo.GetVideoSize(out videoWidth, out videoHeight);
      }
      aspectX = videoWidth;
      aspectY = videoHeight;
      if (_basicVideo != null)
      {
        _basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
      }
      if (videoHeight != _videoHeight || videoWidth != _videoWidth ||
          aspectX != _aspectX || aspectY != _aspectY)
      {
        _updateNeeded = true;
        SetVideoWindow();
      }
    }

    protected virtual void OnProcess()
    {
      if (_vmr7 != null)
      {
        _vmr7.Process();
      }
    }

#if DEBUG
    static DateTime dtStart = DateTime.Now;
#endif
    protected void UpdateCurrentPosition()
    {
      if (_mediaSeeking == null)
        return;
      //GetCurrentPosition(): Returns stream position. 
      //Stream position:The current playback position, relative to the content start
      long lStreamPos;
      double fCurrentPos;
      _mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
      fCurrentPos = lStreamPos;
      fCurrentPos /= 10000000d;

      long lContentStart, lContentEnd;
      double fContentStart, fContentEnd;
      _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
      fContentStart = lContentStart;
      fContentEnd = lContentEnd;
      fContentStart /= 10000000d;
      fContentEnd /= 10000000d;
      double fPos = _currentPos;
      fCurrentPos -= fContentStart;
      _currentPos = fCurrentPos;
      // _log.Info("Position:{0}", _currentPos.ToString("f2"));
      _contentStart = fContentStart;
#if DEBUG
      TimeSpan ts = DateTime.Now - dtStart;
      if ( ts.TotalMilliseconds >= 1000 )
      {
        long lDuration;
        double fDuration;
        _mediaSeeking.GetDuration(out lDuration);
        fDuration = lDuration;
        fDuration /= 10000000d;

        //_log.Info("pos:{0} content:{1}-{2} duration:{3} stream:{4}",_currentPos,fContentStart,fContentEnd,fDuration,fPos);

        dtStart = DateTime.Now;
      }
#endif
    }
    void UpdateDuration()
    {
      if (_mediaSeeking == null)
        return;
      //GetDuration(): Returns (content start – content stop). 
      //content start: The time of the earliest available content. 
      //               For live content, the value starts at zero and increases whenever the 
      //               Stream Buffer Engine deletes an old file. 				
      //content stop : The time of the latest available content. For live content, this value starts at zero 
      //               and increases continuously.

      long lDuration;
      _mediaSeeking.GetDuration(out lDuration);
      _duration = lDuration;
      _duration /= 10000000d;
      //_log.Info("Duration:{0} pos:{1}", _duration.ToString("f2"), CurrentPosition.ToString("f2"));
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected virtual bool GetInterfaces(string filename)
    {
      int hr;
      _log.Info("TSStreamBufferPlayer:GetInterfaces()");
      //Type comtype = null;
      object comobj = null;
      try
      {
        _graphBuilder = (IGraphBuilder)new FilterGraph();

        _vmr7 = new VMR7Util();
        _vmr7.AddVMR7(_graphBuilder);

        _fileSource = new TsFileSource();
        IBaseFilter filter = (IBaseFilter)_fileSource;
        _graphBuilder.AddFilter(filter, "TsFileSource");

        IFileSourceFilter interFaceFile = (IFileSourceFilter)_fileSource;
        interFaceFile.Load(filename, null);

        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strAudioCodec = "";
        string strAudiorenderer = "";
        bool bAddFFDshow = false;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          bAddFFDshow = xmlreader.GetValueAsBool("mytv", "ffdshow", false);
          strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
          strAudiorenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "");
          string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
          if (strValue.Equals("zoom"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
          if (strValue.Equals("stretch"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
          if (strValue.Equals("normal"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
          if (strValue.Equals("original"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
          if (strValue.Equals("letterbox"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
          if (strValue.Equals("panscan"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
          if (strValue.Equals("zoom149"))
            GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom14to9;
        }
        if (strVideoCodec.Length > 0)
          _videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
        if (strAudioCodec.Length > 0)
          _audioCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
        if (strAudiorenderer.Length > 0)
          _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudiorenderer, false);
        if (bAddFFDshow)
          _ffdShowFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, "ffdshow raw video filter");

        if (strVideoCodec.ToLower().IndexOf("nvidia") >= 0)
          _isUsingNvidiaCodec = true;

        //render outputpins of SBE
        DirectShowUtil.RenderOutputPins(_graphBuilder, (IBaseFilter)_fileSource);

        _mediaCtrl = (IMediaControl)_graphBuilder;
        _videoWin = _graphBuilder as IVideoWindow;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _mediaSeeking = _graphBuilder as IMediaSeeking;
        if (_mediaSeeking == null)
        {
          _log.Error("Unable to get IMediaSeeking interface#1");
        }

        if (_audioRendererFilter != null)
        {
          IMediaFilter mp = _graphBuilder as IMediaFilter;
          IReferenceClock clock = _audioRendererFilter as IReferenceClock;
          hr = mp.SetSyncSource(clock);
        }
        _basicVideo = _graphBuilder as IBasicVideo2;
        _basicAudio = _graphBuilder as IBasicAudio;

        //_log.Info("TSStreamBufferPlayer:SetARMode");
        DirectShowUtil.SetARMode(_graphBuilder, AspectRatioMode.Stretched);

        _graphBuilder.SetDefaultSyncSource();
        //_log.Info("TSStreamBufferPlayer: set Deinterlace");

        //_log.Info("TSStreamBufferPlayer: done");
        return true;
      }
      catch (Exception ex)
      {
        _log.Error("TSStreamBufferPlayer:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        return false;
      }
      finally
      {
        if (comobj != null)
          Marshal.ReleaseComObject(comobj);
        comobj = null;
      }
    }



    /// <summary> do cleanup and release DirectShow. </summary>
    protected virtual void CloseInterfaces()
    {
      int hr;
      if (_graphBuilder == null)
      {
        _log.Info("TSStreamBufferPlayer:grapbuilder=null");
        return;
      }

      _log.Info("TSStreamBufferPlayer:cleanup DShow graph");
      try
      {


        if (_mediaCtrl != null)
        {
          hr = _mediaCtrl.Stop();

          _mediaCtrl = null;
        }

        _state = PlayState.Init;

        _mediaEvt = null;
        _isWindowVisible = false;
        _isVisible = false;
        _videoWin = null;
        _mediaSeeking = null;
        _basicAudio = null;
        _basicVideo = null;


        if (_videoCodecFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_videoCodecFilter)) > 0)
            ;
          _videoCodecFilter = null;
        }
        if (_audioCodecFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_audioCodecFilter)) > 0)
            ;
          _audioCodecFilter = null;
        }

        if (_audioRendererFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_audioRendererFilter)) > 0)
            ;
          _audioRendererFilter = null;
        }

        if (_ffdShowFilter != null)
        {
          while ((hr = Marshal.ReleaseComObject(_ffdShowFilter)) > 0)
            ;
          _ffdShowFilter = null;
        }
        if (_fileSource != null)
        {
          while ((hr = Marshal.ReleaseComObject(_fileSource)) > 0)
            ;
          _fileSource = null;
        }

        if (_vmr7 != null)
          _vmr7.RemoveVMR7();
        _vmr7 = null;

        DirectShowUtil.RemoveFilters(_graphBuilder);

        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;

        if (_graphBuilder != null)
        {
          while ((hr = Marshal.ReleaseComObject(_graphBuilder)) > 0)
            ;
        }
        _graphBuilder = null;

        _state = PlayState.Init;
        GUIGraphicsContext.form.Invalidate(true);
        GC.Collect();
        GC.Collect();
        GC.Collect();
      }
      catch (Exception ex)
      {
        _log.Error("TSStreamBufferPlayer:exception while cleanuping DShow graph {0} {1}", ex.Message, ex.StackTrace);
      }
      //_log.Info("TSStreamBufferPlayer:cleanup done");
    }

    void OnGraphNotify()
    {
      int p1, p2, hr = 0;
      EventCode code;
      int counter = 0;
      do
      {
        counter++;
        if (/*Playing && */_mediaEvt != null)
        {
          hr = _mediaEvt.GetEvent(out code, out p1, out p2, 0);
          if (hr == 0)
          {
            hr = _mediaEvt.FreeEventParams(code, p1, p2);
            /*
                        if (code>=EventCode.StreamBufferTimeHole && code <= EventCode.StreamBufferRateChanged)
                        {
                          _log.Info("TSStreamBufferPlayer: event:{0} param1:{1} param2:{2} param1:0x{3:X} param2:0x{4:X}",code.ToString(),p1,p2,p1,p2);
                          long contentStart,contentStop,streamPosition,segmentstop;
                          double fcontentStart,fcontentStop,fstreamPosition,fsegmentstop;
                          _mediaSeeking.GetAvailable(out contentStart, out contentStop);
                          _mediaSeeking.GetCurrentPosition(out streamPosition);
                          _mediaSeeking.GetStopPosition(out segmentstop);
                          fcontentStart=(double)contentStart;
                          fcontentStop=(double)contentStop;
                          fstreamPosition=(double)streamPosition;
                          fsegmentstop=(double)segmentstop;

                          fcontentStart/=10000000d;
                          fcontentStop/=10000000d;
                          fstreamPosition/=10000000d;
                          fsegmentstop/=10000000d;
                          _log.Info("TSStreamBufferPlayer:  content start   :{0} content stop :{1}", fcontentStart.ToString("f2"), fcontentStop.ToString("f2"));
                          _log.Info("TSStreamBufferPlayer:  streamPosition  :{0} segment stop :{1}", fstreamPosition.ToString("f2"), fsegmentstop.ToString("f2"));
                          if (code==EventCode.StreamBufferTimeHole)
                          {
                            //The Stream Buffer Source filter has reached a gap in the content. 
                            //p1 = Time of the start of the gap, in milliseconds, relative to the content start.
                            //param2 = Duration of the gap, in milliseconds.
                            double newpos=(double)p1 + (double)p2 + contentStart;
                            newpos /=1000d;
                            if (newpos <fstreamPosition-1 || newpos > fstreamPosition+1)
                            {
                              _log.Info("TSStreamBufferPlayer:  seek to:{0}", newpos.ToString("f2"));
                              SeekAbsolute(1d+newpos);
                            }
                          }
                          if (code==EventCode.StreamBufferContentBecomingStale)
                          {
                            if (Paused)
                            {
                              _log.Info("TSStreamBufferPlayer:  unpause");
                              Pause();
                            }
                          }
                        }
                        else */
            if (code == EventCode.Complete || code == EventCode.ErrorAbort)
            {
              //_log.Info("TSStreamBufferPlayer: event:{0} param1:{1} param2:{2} param1:0x{3:X} param2:0x{4:X}",code.ToString(),p1,p2,p1,p2);
              MovieEnded();
            }
            //else
            //_log.Info("TSStreamBufferPlayer: event:{0} 0x{1:X} param1:{2} param2:{3} param1:0x{4:X} param2:0x{5:X}",code.ToString(), (int)code,p1,p2,p1,p2);
          }
          else
            break;
        }
        else
          break;
      }
      while (hr == 0 && counter < 20);
    }


    protected virtual void OnInitialized()
    {
    }
    protected void DoFFRW()
    {

      if (!Playing)
        return;

      if ((_speedRate == 10000) || (_mediaSeeking == null))
        return;
      TimeSpan ts = DateTime.Now - _elapsedTimer;
      if (ts.TotalMilliseconds < 100)
        return;
      long earliest, latest, current, stop, rewind, pStop;

      _mediaSeeking.GetAvailable(out earliest, out latest);
      _mediaSeeking.GetPositions(out current, out stop);

      // _log.Info("earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
      //         earliest/10000000,latest/10000000,current/10000000,stop/10000000,_speedRate, (latest-earliest)/10000000);

      //earliest += + 30 * 10000000;

      // new time = current time + 2*timerinterval* (speed)
      long lTimerInterval = (long)ts.TotalMilliseconds;
      if (lTimerInterval > 300)
        lTimerInterval = 300;
      lTimerInterval = 300;
      rewind = (long)(current + (2 * (long)(lTimerInterval) * _speedRate));

      int hr;
      pStop = 0;

      // if we end up before the first moment of time then just
      // start @ the beginning
      if ((rewind < earliest) && (_speedRate < 0))
      {
        _speedRate = 10000;
        rewind = earliest;
        //_log.Info(" seek back:{0}",rewind/10000000);
        hr = _mediaSeeking.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        _mediaCtrl.Run();
        return;
      }
      // if we end up at the end of time then just
      // start @ the end-100msec
      if ((rewind > (latest - 100000)) && (_speedRate > 0))
      {
        _speedRate = 10000;
        rewind = latest - 100000;
        //_log.Info(" seek ff:{0}",rewind/10000000);
        hr = _mediaSeeking.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        _mediaCtrl.Run();
        return;
      }

      //seek to new moment in time
      //_log.Info(" seek :{0}",rewind/10000000);
      hr = _mediaSeeking.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
      _mediaCtrl.Pause();

      _elapsedTimer = DateTime.Now;
    }

    #endregion

    #region IDisposable Members

    public override void Release()
    {
      CloseInterfaces();
    }
    #endregion
  }
}
