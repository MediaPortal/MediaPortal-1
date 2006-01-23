/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;

namespace MediaPortal.Player 
{
	public class BaseTStreamBufferPlayer : IPlayer
	{
		[ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
			class MPEG2Demultiplexer {}
//		[ComImport, Guid("A3556F1E-787B-12C4-9100-01AF313AC900")]
//		protected class MPTransportStreamReader {}
		[ComImport, Guid("4F8BF30C-3BEB-43A3-8BF2-10096FD28CF2")]
		protected class TsFileSource{}

		public enum PlayState
		{
			Init,
			Playing,
			Paused,
      Ended
		}
		protected int      iSpeed=1;


		protected int 															_positionX=0;
		protected int 															_positionY=0;
		protected int 															_width=200;
		protected int 															_height=100;
		protected int																_videoWidth=100;
		protected int																_videoHeight=100;
		protected string														_currentFile="";
		protected bool															_updateNeeded=false;
		protected bool															_fullScreen=true;
		protected PlayState													_state=PlayState.Init;
		protected int																_volume=100;
		protected  IGraphBuilder			  			      _graphBuilder =null;
		protected  TsFileSource 										_bufferSource=null ;
		protected  IMediaSeeking									  _mediaSeeking=null;
		protected int                               _speed=1;
		protected double                            _currentPos;
    protected double                            _contentStart;
		protected double                            _duration=-1d;
    protected bool          										_started=false;
    protected bool          										_isLive=false;
    protected double                            _lastPosition=0;
		protected bool                              _windowVisible=false;
		protected DsROTEntry												_rotEntry = null;
		protected int                       _aspectX=1;
		protected int                       _aspectY=1;
		protected long                      _speedRate = 10000;
		bool																_usingNvidiaCodec=false;
		protected bool											_hasVideo=false;
		protected IBaseFilter								_audioRendererFilter;
		/// <summary> control interface. </summary>
		protected IMediaControl							_mediaCtrl =null;

		/// <summary> graph event interface. </summary>
		protected IMediaEventEx							_mediaEvt =null;

    
		/// <summary> video preview window interface. </summary>
		protected IVideoWindow							_videoWin =null;

		/// <summary> interface to get information and control video. </summary>
		protected IBasicVideo2							_basicVideo =null;

		/// <summary> audio interface used to control volume. </summary>
		protected IBasicAudio								_basicAudio=null;
		VMR7Util  vmr7 = null;
		DateTime  elapsedTimer=DateTime.Now;

		protected const int WM_GRAPHNOTIFY	= 0x00008001;	// message from graph
		protected const int WS_CHILD			= 0x40000000;	// attributes for video window
		protected const int WS_CLIPCHILDREN	= 0x02000000;
		protected const int WS_CLIPSIBLINGS	= 0x04000000;
		protected DateTime _updateTimer=DateTime.Now;
		protected MediaPortal.GUI.Library.Geometry.Type             _ar=MediaPortal.GUI.Library.Geometry.Type.Normal;
		protected bool		  _seekToEndFlag=false;
		public BaseTStreamBufferPlayer()
		{
		}


		public override bool Play(string strFile)
		{
      if (!System.IO.File.Exists(strFile)) return false;
			iSpeed=1;
			_speedRate = 10000;
			_isLive=false;
      _duration=-1d;
			_seekToEndFlag=false;
      string strExt=System.IO.Path.GetExtension(strFile).ToLower();
			
      if (strFile.ToLower().IndexOf("live.ts")>=0 ||
				  strFile.ToLower().IndexOf("radio.ts")>=0)
      {
        _isLive=true;
			}
			_hasVideo=true;
			if (strFile.ToLower().IndexOf("radio.ts")>=0)
			{
				_hasVideo=false;
			}

			_isVisible=false;
      _windowVisible=false;
			_volume=100;
			_state=PlayState.Init;
			_currentFile=strFile;
			_fullScreen=false;
			_ar=MediaPortal.GUI.Library.Geometry.Type.Normal;

			_updateNeeded=true;
			Log.Write("TStreamBufferPlayer:play {0}", strFile);
			GC.Collect();
			CloseInterfaces();
			GC.Collect();
			_started=false;
			if( ! GetInterfaces(strFile ))
			{
				Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer:GetInterfaces() failed");
				_currentFile="";
				return false;
			}
      _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);
       
			int hr = _mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
			if (hr < 0)
			{
				Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer:SetNotifyWindow() failed");
				_currentFile="";
				CloseInterfaces();
				return false;
			}
			if (_basicVideo!=null)
			{
				hr = _basicVideo.GetVideoSize( out _videoWidth, out _videoHeight );
				if (hr== 0)
				{
					if (_videoWin!=null)
					{
						_videoWin.put_Owner( GUIGraphicsContext.ActiveForm );
            _videoWin.put_WindowStyle((WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipSiblings + (int)WindowStyle.ClipChildren));
						_videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);

					}
        }
        Log.Write("TStreamBufferPlayer:VideoSize:{0}x{1}",_videoWidth,_videoHeight);	
			}
        
			if (_mediaCtrl==null)
			{
        Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer:_mediaCtrl==null");
        _currentFile="";
				CloseInterfaces();
				return false;
			}

      //DsUtils.DumpFilters(_graphBuilder);

      _positionX=GUIGraphicsContext.VideoWindow.X;
      _positionY=GUIGraphicsContext.VideoWindow.Y;
      _width    =GUIGraphicsContext.VideoWindow.Width;
      _height   =GUIGraphicsContext.VideoWindow.Height;
      _ar        =GUIGraphicsContext.ARType;
      _updateNeeded=true;
      SetVideoWindow();

			//DirectShowUtil.EnableDeInterlace(_graphBuilder);
			hr = _mediaCtrl.Run();
			_mediaSeeking = _graphBuilder as IMediaSeeking;
			if (hr < 0)
			{
				Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer:Rungraph failed");
				_currentFile="";
				CloseInterfaces();
				return false;
			}
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED,0,0,0,0,0,null);
			msg.Label=strFile;
			GUIWindowManager.SendThreadMessage(msg);

			_state=PlayState.Playing;

			UpdateCurrentPosition();
			UpdateDuration();
			if (_isLive)
			{
				SeekAbsolute(0);
			}
			else
			{
				//seek to end of file
				SeekAbsolute(0);
			}


			//Log.Write("TStreamBufferPlayer:playing duration:{0}",Utils.SecondsToHMSString( (int)_duration) );
			_state=PlayState.Playing;
			OnInitialized();
			return true;
		}

		public override void SetVideoWindow()
		{
			if (!_hasVideo) return;
			if (GUIGraphicsContext.IsFullScreenVideo!= _fullScreen)
			{
				_fullScreen=GUIGraphicsContext.IsFullScreenVideo;
				_updateNeeded=true;
			}

			if (!_updateNeeded) return;
      
			_updateNeeded=false;
			_started=true;
			float x=_positionX;
			float y=_positionY;
      
			int nw=_width;
			int nh=_height;
			if (nw > GUIGraphicsContext.OverScanWidth)
				nw=GUIGraphicsContext.OverScanWidth;
			if (nh > GUIGraphicsContext.OverScanHeight)
				nh=GUIGraphicsContext.OverScanHeight;
			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				x=_positionX=GUIGraphicsContext.OverScanLeft;
				y=_positionY=GUIGraphicsContext.OverScanTop;
				nw=_width=GUIGraphicsContext.OverScanWidth;
				nh=_height=GUIGraphicsContext.OverScanHeight;
			}
/*			Log.Write("{0},{1}-{2},{3}  vidwin:{4},{5}-{6},{7} fs:{8}", x,y,nw,nh, 
				GUIGraphicsContext.VideoWindow.Left,
				GUIGraphicsContext.VideoWindow.Top,
				GUIGraphicsContext.VideoWindow.Right,
				GUIGraphicsContext.VideoWindow.Bottom,
				GUIGraphicsContext.IsFullScreenVideo);*/
			if (nw <=0 || nh <=0) return;
			if (x  < 0 || y  < 0) return;

			
			int aspectX, aspectY;
			if (_basicVideo!=null)
			{
				_basicVideo.GetVideoSize(out _videoWidth, out _videoHeight);
			}
			aspectX=_videoWidth;
			aspectY=_videoHeight;
			if (_basicVideo!=null)
			{
				_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			GUIGraphicsContext.VideoSize=new Size(_videoWidth, _videoHeight);
			_aspectX=aspectX;
			_aspectY=aspectY;
			MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
			System.Drawing.Rectangle rSource,rDest;
			m_geometry.ImageWidth=_videoWidth;
			m_geometry.ImageHeight=_videoHeight;
			m_geometry.ScreenWidth=nw;
			m_geometry.ScreenHeight=nh;
			m_geometry.ARType=GUIGraphicsContext.ARType;
			m_geometry.PixelRatio=GUIGraphicsContext.PixelRatio;
			m_geometry.GetWindow(aspectX,aspectY,out rSource, out rDest);

			rDest.X += (int)x;
			rDest.Y += (int)y;

			Log.Write("overlay: video WxH  : {0}x{1}",_videoWidth,_videoHeight);
			Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
			Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
			Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
			Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
			Log.Write("overlay: src        : ({0},{1})-({2},{3})",
				rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
			Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
				rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);

        
			Log.Write("TStreamBufferPlayer:Window ({0},{1})-({2},{3}) - ({4},{5})-({6},{7})", 
				rSource.X,rSource.Y, rSource.Right, rSource.Bottom,
				rDest.X, rDest.Y, rDest.Right, rDest.Bottom);


			SetSourceDestRectangles(rSource,rDest);
			SetVideoPosition(rDest);
			_sourceRectangle=rSource;
			_videoRectangle=rDest;
		}

		protected virtual void SetVideoPosition(System.Drawing.Rectangle rDest)
		{
			if (_hasVideo && _videoWin!=null)
			{
				if (rDest.Left< 0 || rDest.Top<0 || rDest.Width<=0 || rDest.Height<=0) return;
				_videoWin.SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
			}
		}

		protected virtual void  SetSourceDestRectangles(System.Drawing.Rectangle rSource,System.Drawing.Rectangle rDest)
		{
			if (_hasVideo && _basicVideo!=null)
			{
				if (rSource.Left< 0 || rSource.Top<0 || rSource.Width<=0 || rSource.Height<=0) return;
				if (rDest.Width<=0 || rDest.Height<=0) return;
				_basicVideo.SetSourcePosition(rSource.Left,rSource.Top,rSource.Width,rSource.Height);
				_basicVideo.SetDestinationPosition(0,0,rDest.Width,rDest.Height);
			}
		}

		void MovieEnded()
		{
			// this is triggered only if movie has ended
			// ifso, stop the movie which will trigger MovieStopped
			
			Log.Write("TStreamBufferPlayer:ended {0}", _currentFile);
      if (!IsTimeShifting)
      {
        CloseInterfaces();
        _state=PlayState.Ended;
      }
      else
      {
				_seekToEndFlag=true;
      }
		}


    public override bool Ended
    {
      get { return _state==PlayState.Ended;}
    }

		public override void Process()
		{
			if ( !Playing) return;
			if ( !_started) return;
			if (GUIGraphicsContext.InVmr9Render) return;
			OnProcess();
			if (_seekToEndFlag)
			{
				_seekToEndFlag=false;
				if (CurrentPosition+5 <= Duration)
				{
					SeekAbsolute(Duration);
				}
			}
			TimeSpan ts=DateTime.Now-_updateTimer;
			if (ts.TotalMilliseconds>=800 || iSpeed!=1) 
			{
				UpdateCurrentPosition();
					
				UpdateDuration();
				_updateTimer=DateTime.Now;
				Log.Write("pos:{0} duration:{1}", _currentPos.ToString("f2"),_duration.ToString("f2"));
			}

			_lastPosition=CurrentPosition;
							
			if (_hasVideo) 
			{
				if (GUIGraphicsContext.VideoWindow.Width<=10&& GUIGraphicsContext.IsFullScreenVideo==false)
				{
					_isVisible=false;
				}
				if (GUIGraphicsContext.BlankScreen)
				{
					_isVisible=false;
				}
				if (_windowVisible && !_isVisible)
				{
					_windowVisible=false;
					//Log.Write("TStreamBufferPlayer:hide window");
					if (_videoWin!=null) _videoWin.put_Visible( OABool.False);
				}
				else if (!_windowVisible && _isVisible)
				{
					_windowVisible=true;
					//Log.Write("TStreamBufferPlayer:show window");
          if (_videoWin != null) _videoWin.put_Visible(OABool.True);
				}      
			}
			
			CheckVideoResolutionChanges();
			if (_speedRate!=10000)
			{
				DoFFRW();
			}
		}

		void CheckVideoResolutionChanges()
		{
			if (!_hasVideo) return;
			if (_videoWin==null || _basicVideo==null) return;
			int aspectX, aspectY;
			int videoWidth=1, videoHeight=1;
			if (_basicVideo!=null)
			{
				_basicVideo.GetVideoSize(out videoWidth, out videoHeight);
			}
			aspectX=videoWidth;
			aspectY=videoHeight;
			if (_basicVideo!=null)
			{
				_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			if (videoHeight!=_videoHeight || videoWidth != _videoWidth ||
				  aspectX != _aspectX || aspectY != _aspectY)
			{
				_updateNeeded=true;
				SetVideoWindow();
			}
		}

		protected virtual void OnProcess()
		{
			if (vmr7!=null)
			{
				vmr7.Process();
			}
		}
    

		public override int PositionX
		{
			get { return _positionX;}
			set 
			{ 
				if (value != _positionX)
				{
					_positionX=value;
					_updateNeeded=true;
				}
			}
		}

		public override int PositionY
		{
			get { return _positionY;}
			set 
			{
				if (value != _positionY)
				{
					_positionY=value;
					_updateNeeded=true;
				}
			}
		}

		public override int RenderWidth
		{
			get { return _width;}
			set 
			{
				if (value !=_width)
				{
					_width=value;
					_updateNeeded=true;
				}
			}
		}
		public override int RenderHeight
		{
			get { return _height;}
			set 
			{
				if (value != _height)
				{
					_height=value;
					_updateNeeded=true;
				}
			}
		}

		public override double Duration
		{
			get 
			{
				if (_duration<0) Process();
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
      get {return 0;}
    }
		public override bool FullScreen
		{
			get 
			{ 
				return GUIGraphicsContext.IsFullScreenVideo;
			}
			set
			{
				if (value != _fullScreen )
				{
					_fullScreen=value;
					_updateNeeded=true;          
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
			if (_state==PlayState.Paused) 
			{
				Log.Write("BaseTTStreamBufferPlayer:resume");
				Speed=1;
				_mediaCtrl.Run();
				_state=PlayState.Playing;
			}
			else if (_state==PlayState.Playing) 
			{
				Log.Write("BaseTTStreamBufferPlayer:pause");
				_state=PlayState.Paused;
				_mediaCtrl.Pause();
			}
		}

		public override bool Paused
		{
			get 
			{
				return (_state==PlayState.Paused);
			}
		}

		public override bool Playing
		{
			get 
			{ 
				return (_state==PlayState.Playing||_state==PlayState.Paused);
			}
		}

		public override bool Stopped
		{
			get 
			{ 
				return (_state==PlayState.Init);
			}
		}

		public override string CurrentFile
		{
			get { return _currentFile;}
		}

		public override void Stop()
		{
			if (_state!=PlayState.Init)
			{
				Log.Write("TStreamBufferPlayer:stop");

				CloseInterfaces();
			}
		}
/*
		public override int Speed
		{
			get
			{
				//lock (this)
			{
				return _speed;
			}
			}
			set 
			{
				//lock (this)
			{
				if (_speed!=value)
				{
					_speed=value;
					if (_mediaSeeking==null) return ;
					double dRate=value;
					int hr=_mediaSeeking.SetRate(dRate);
					dRate=0;
					_mediaSeeking.GetRate(out dRate);
					Log.Write("TStreamBufferPlayer:SetRate to:{0} hr:{1} {2}", value,hr, dRate);
				}
			}
			}
		}
*/

		public override int Volume
		{
			get { return _volume;}
			set 
			{
				if (_volume!=value)
				{
					_volume=value;
					if (_state!=PlayState.Init)
					{
						if (_basicAudio!=null)
						{
							// Divide by 100 to get equivalent decibel value. For example, –10,000 is –100 dB. 
							float fPercent=(float)_volume/100.0f;
							int iVolume=(int)(5000.0f*fPercent);
							_basicAudio.put_Volume( (iVolume-5000));
						}
					}
				}
			}
		}

		public override MediaPortal.GUI.Library.Geometry.Type ARType
		{
			get { return GUIGraphicsContext.ARType;}
			set 
			{
				if (_ar != value)
				{
					_ar=value;
					_updateNeeded=true;
				}
			}
		}

		public override void SeekRelative(double dTime)
		{
			if (_state!=PlayState.Init)
			{
				if (_mediaCtrl!=null && _mediaSeeking!=null)
				{ 
					double dCurTime=this.CurrentPosition;
            
					dTime=dCurTime+dTime;
					if (dTime<0.0d) dTime=0.0d;
					if (dTime < Duration)
					{
						SeekAbsolute(dTime);
					}
				}
			}
		}

		public override void SeekAbsolute(double dTime)
		{
			if (_state!=PlayState.Init)
			{
				if (_mediaCtrl!=null && _mediaSeeking!=null)
        {
          if (dTime<0.0d) dTime=0.0d;
          if (dTime>Duration) dTime=Duration;
					dTime=Math.Floor(dTime);
          Log.Write("seekabs: {0} duration:{1} current pos:{2}", dTime,Duration, CurrentPosition);
          dTime*=10000000d;
					DsLong pStop=new DsLong(0);

					//long lContentStart,lContentEnd,dmp;
					//double fContentStart,fContentEnd;
					//_mediaSeeking.GetAvailable(out lContentStart, out dmp);
					//_mediaSeeking.GetPositions(out dmp, out lContentEnd);
          //fContentStart=lContentStart;
          //fContentEnd=lContentEnd;

          //dTime+=fContentStart;
          DsLong lTime=new DsLong((long)dTime);

          int hr = _mediaSeeking.SetPositions(lTime, ((AMSeekingSeekingFlags)((int)AMSeekingSeekingFlags.AbsolutePositioning + (int)AMSeekingSeekingFlags.SeekToKeyFrame)), pStop, AMSeekingSeekingFlags.NoPositioning);
          if (hr !=0)
          {
            hr = _mediaSeeking.SetPositions( lTime, AMSeekingSeekingFlags.AbsolutePositioning,  pStop, AMSeekingSeekingFlags.NoPositioning);
						if (hr !=0)
						{
							Log.WriteFile(Log.LogType.Log,true,"seek failed->seek to 0 0x:{0:X}",hr);
						}
          }
				}
        UpdateCurrentPosition();
        //Log.Write("seek->current pos:{0}", CurrentPosition);

			}
		}
#if DEBUG
    static DateTime dtStart=DateTime.Now;
#endif
    void UpdateCurrentPosition()
    {
			if (GUIGraphicsContext.InVmr9Render)
			{
				return;
			}
			int hr;
      if (_mediaSeeking==null) return;
      long lStreamPos;
      double fCurrentPos;
      hr=_mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
      fCurrentPos=lStreamPos;
      fCurrentPos/=10000000d;
			_currentPos=fCurrentPos;
    }
		void UpdateDuration()
		{
			if (GUIGraphicsContext.InVmr9Render)
			{
				return;
			}
			
			long lDuration;
			int hr=_mediaSeeking.GetDuration(out lDuration); 
			_duration=lDuration;
			_duration/=10000000d;
		}

		public override void SeekRelativePercentage(int iPercentage)
		{
			if (_state!=PlayState.Init)
			{
				if (_mediaCtrl!=null && _mediaSeeking!=null)
				{
					double dCurrentPos=this.CurrentPosition;
					double dDuration=Duration;

					double fCurPercent=(dCurrentPos/Duration)*100.0d;
					double fOnePercent=Duration/100.0d;
					fCurPercent=fCurPercent + (double)iPercentage;
					fCurPercent*=fOnePercent;
					if (fCurPercent<0.0d) fCurPercent=0.0d;
					if (fCurPercent<Duration)
					{
            SeekAbsolute(fCurPercent);
					}
				}
			}
		}


		public override void SeekAsolutePercentage(int iPercentage)
		{
			if (_state!=PlayState.Init)
			{
				if (_mediaCtrl!=null && _mediaSeeking!=null)
				{

					if (iPercentage<0) iPercentage=0;
					if (iPercentage>=100) iPercentage=100;
					double fPercent=Duration/100.0f;
					fPercent*=(double)iPercentage;
          SeekAbsolute(fPercent);
				}
			}
		}

    
		public override bool HasVideo
		{
			get { return _hasVideo;}
		}

		/// <summary> create the used COM components and get the interfaces. </summary>
		protected virtual bool GetInterfaces(string filename)
		{

      //Type comtype = null;
      //object comobj = null;
			try 
			{
        _graphBuilder = (IGraphBuilder)new FilterGraph();

				if (_hasVideo)
				{
					vmr7=new VMR7Util();
					vmr7.AddVMR7(_graphBuilder);
				}
				int hr;
				//if (!_hasVideo)
				{
					MPEG2Demultiplexer m_MPEG2Demuxer=null;
					IBaseFilter m_mpeg2Multiplexer=null;
					Log.WriteFile(Log.LogType.Capture,"mpeg2:add new MPEG2 Demultiplexer to graph");
					try
					{
						m_MPEG2Demuxer=new MPEG2Demultiplexer();
						m_mpeg2Multiplexer = (IBaseFilter) m_MPEG2Demuxer;
					}
					catch(Exception){}
					//m_mpeg2Multiplexer = DirectShowUtil.AddFilterToGraph(m__graphBuilder,"MPEG-2 Demultiplexer");
					if (m_mpeg2Multiplexer==null) 
					{
						Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to create mpeg2 demuxer");
						return false;
					}
					hr=_graphBuilder.AddFilter(m_mpeg2Multiplexer,"MPEG-2 Demultiplexer");
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to add mpeg2 demuxer to graph:0x{0:X}",hr);
						return false;
					}
				}

				_bufferSource = new TsFileSource();
				IBaseFilter filter = (IBaseFilter) _bufferSource;
				_graphBuilder.AddFilter(filter, "TsFileSource");
		
				IFileSourceFilter fileSource = (IFileSourceFilter) _bufferSource;
				hr = fileSource.Load(filename, null);

				// add preferred video & audio codecs
				string strVideoCodec="";
				string strAudioCodec="";
				string strAudiorenderer="";
        bool   bAddFFDshow=false;
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
					strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
					strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
					strAudiorenderer=xmlreader.GetValueAsString("mytv","audiorenderer","");
					string strValue=xmlreader.GetValueAsString("mytv","defaultar","normal");
					if (strValue.Equals("zoom")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
					if (strValue.Equals("stretch")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
					if (strValue.Equals("normal")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
					if (strValue.Equals("original")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
					if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
					if (strValue.Equals("panscan")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;
				}
				IBaseFilter videoCodec=null;
				if (_hasVideo)
				{
					if (strVideoCodec.Length>0) 
						videoCodec=DirectShowUtil.AddFilterToGraph(_graphBuilder,strVideoCodec);
				}
				if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(_graphBuilder,strAudioCodec);
				if (strAudiorenderer.Length==0) 
				{
					strAudiorenderer="Default DirectSound Device";					
				}
					_audioRendererFilter=DirectShowUtil.AddAudioRendererToGraph(_graphBuilder,strAudiorenderer,false);
        if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(_graphBuilder,"ffdshow raw video filter");

				if (strVideoCodec.ToLower().IndexOf("nvidia")>=0)
					_usingNvidiaCodec=true;

				//render outputpins of SBE
				DirectShowUtil.RenderOutputPins(_graphBuilder, (IBaseFilter)fileSource);

				_mediaCtrl	= (IMediaControl)  _graphBuilder;
				_mediaEvt	= (IMediaEventEx)  _graphBuilder;
				_mediaSeeking = _graphBuilder as IMediaSeeking;
				_basicAudio	= _graphBuilder as IBasicAudio;
				if (_hasVideo)
				{
					_videoWin	= _graphBuilder as IVideoWindow;
					_basicVideo	= _graphBuilder as IBasicVideo2;
					//Log.Write("TStreamBufferPlayer:SetARMode");
					DirectShowUtil.SetARMode(_graphBuilder,AspectRatioMode.Stretched);

					hr = _basicVideo.GetVideoSize( out _videoWidth, out _videoHeight );
					if (hr!=0)
					{
						if (videoCodec!=null)
							_graphBuilder.RemoveFilter(videoCodec);
						_hasVideo=false;
						if (vmr7!=null)
							vmr7.RemoveVMR7();
						vmr7=null;
						_basicVideo	= null;
						_videoWin=null;
					}
				}
				if (_audioRendererFilter!=null)
				{
					IMediaFilter mp				= _graphBuilder as IMediaFilter;
					IReferenceClock clock = _audioRendererFilter as IReferenceClock;
					hr=mp.SetSyncSource(clock);
				}
				
				//Log.Write("TStreamBufferPlayer: set Deinterlace");

				//Log.Write("TStreamBufferPlayer: done");
				return true;
			}
			catch( Exception  ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
				return false;
			}
		}



		/// <summary> do cleanup and release DirectShow. </summary>
		protected virtual void CloseInterfaces()
		{
			int hr;
			if (_graphBuilder==null) return;
			Log.Write("TStreamBufferPlayer:cleanup DShow graph");
			try 
			{

				if( _mediaCtrl != null )
				{
					hr = _mediaCtrl.Stop();
					_mediaCtrl = null;
				}

				_state = PlayState.Init;

				_mediaEvt = null;
        _windowVisible=false;
				_isVisible=false;
				_videoWin = null;
				_mediaSeeking= null;
				_basicAudio	= null;
				_basicVideo	= null;
				_videoWin=null;

				if (vmr7!=null)
					vmr7.RemoveVMR7();
				vmr7=null;

				while((hr=Marshal.ReleaseComObject( _bufferSource ))>0); 
				_bufferSource	= null;

				if (_audioRendererFilter!=null)
					Marshal.ReleaseComObject( _audioRendererFilter );
				_audioRendererFilter	= null;

        DirectShowUtil.RemoveFilters(_graphBuilder);

        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;

				if( _graphBuilder != null )
				{
					while((hr=Marshal.ReleaseComObject( _graphBuilder ))>0); 
					_graphBuilder = null;
				}

				_state = PlayState.Init;
				GUIGraphicsContext.form.Invalidate(true);
				GC.Collect();GC.Collect();GC.Collect();
			}
			catch( Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"TStreamBufferPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
			}
			//Log.Write("TStreamBufferPlayer:cleanup done");
		}

		public override void WndProc( ref Message m )
		{
			if( m.Msg == WM_GRAPHNOTIFY )
			{
				if( _mediaEvt != null )
					OnGraphNotify();
				return;
			}
			base.WndProc( ref m );
		}

		void OnGraphNotify()
		{
			int p1, p2, hr = 0;
      EventCode code;
			int counter=0;
			do
			{
				counter++;
        if (/*Playing && */_mediaEvt!=null)
        {
          hr = _mediaEvt.GetEvent( out code, out p1, out p2, 0 );
          if( hr < 0 )
            break;
          hr = _mediaEvt.FreeEventParams( code, p1, p2 );
          if (code == EventCode.Complete || code == EventCode.ErrorAbort)
          {
            //Log.Write("TStreamBufferPlayer:on notify complete");
            MovieEnded();
          }
					//Log.Write("BaseTTStreamBufferPlayer: event:{0} {1} {2}",
					//					code.ToString(),p1,p2);
        }
        else
        {
          break;
        }
			}
			while( hr == 0 && counter < 20);
		}

		public override bool IsTV
		{
			get 
			{
				if (_hasVideo)
					return true;
				return false;
			}
		}
		public override bool IsTimeShifting
		{
			get {return _isLive;}
		}      

    public override bool Visible
    {
      get {return _isVisible;}
      set
      { 
        if (value==_isVisible) return;
        _isVisible=value;
        _updateNeeded=true;
      }
    }

		public override int Speed
		{
			get 
			{ 
				if (_state==PlayState.Init) return 1;
				if (_mediaSeeking==null) return 1;
				if (_usingNvidiaCodec) return iSpeed;
				switch ( _speedRate)
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
				if (_state!=PlayState.Init)
				{
					if (_usingNvidiaCodec)
					{
						if (iSpeed!=value)
						{
							iSpeed=value;
							int hr=_mediaSeeking.SetRate((double)iSpeed);
							//Log.Write("VideoPlayer:SetRate to:{0} {1:X}", iSpeed,hr);
							if (hr!=0)
							{
								IMediaSeeking oldMediaSeek=_graphBuilder as IMediaSeeking;
								hr=oldMediaSeek.SetRate((double)iSpeed);
								//Log.Write("VideoPlayer:SetRateOld to:{0} {1:X}", iSpeed,hr);
							}
							if (iSpeed==1)
							{
								_mediaCtrl.Stop();
								System.Windows.Forms.Application.DoEvents();
								System.Threading.Thread.Sleep(200);
								System.Windows.Forms.Application.DoEvents();
								FilterState state;
								_mediaCtrl.GetState(100,out state);
								//Log.Write("state:{0}", state.ToString());
								_mediaCtrl.Run();
							}
						}
					}
					else
					{
						switch ( (int)value)
						{
							case -1:  _speedRate=-10000;break;
							case -2:  _speedRate=-15000;break;
							case -4:  _speedRate=-30000;break;
							case -8:  _speedRate=-45000;break;
							case -16: _speedRate=-60000;break;
							case -32: _speedRate=-75000;break;

							case 1:  
								_speedRate=10000;
								_mediaCtrl.Run();
								break;
							case 2:  _speedRate=15000;break;
							case 4:  _speedRate=30000;break;
							case 8:  _speedRate=45000;break;
							case 16: _speedRate=60000;break;
							default: _speedRate=75000;break;
						}
					}
				}
			}
		}


		public override void ContinueGraph()
		{
			if (_mediaCtrl==null) return;

			Log.Write("TStreamBufferPlayer:continue graph");
			_mediaCtrl.Run();
			_mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
			SeekAbsolute(0);
		}

		public override void PauseGraph()
		{
			Log.Write("TStreamBufferPlayer:Pause graph");
			_mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
			_mediaCtrl.Pause();
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
			TimeSpan ts=DateTime.Now-elapsedTimer;
			if (ts.TotalMilliseconds<100) return;
			long earliest, latest, current,  stop, rewind, pStop,duration;
		
			//_mediaSeeking.GetPositions(out earliest, out latest);
			//_mediaSeeking.GetPositions(out current, out stop);
			_mediaSeeking.GetCurrentPosition(out current);
			_mediaSeeking.GetDuration(out duration);
			earliest=0;
			latest=duration;
			stop=duration;
			
			// Log.Write("earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
			//         earliest/10000000,latest/10000000,current/10000000,stop/10000000,_speedRate, (latest-earliest)/10000000);
      
			//earliest += + 30 * 10000000;

			// new time = current time + 2*timerinterval* (speed)
			long lTimerInterval=(long)ts.TotalMilliseconds;
			if (lTimerInterval > 300) lTimerInterval=300;
			lTimerInterval=300;
			rewind = (long)(current + (2 *(long)(lTimerInterval)* _speedRate)) ;

			int hr; 		
			pStop  = 0;
		
			// if we end up before the first moment of time then just
			// start @ the beginning
			if ((rewind < earliest) && (_speedRate<0))
			{
				_speedRate = 10000;
				rewind = earliest;
				//Log.Write(" seek back:{0}",rewind/10000000);
				hr = _mediaSeeking.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning	,new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
				_mediaCtrl.Run();
				return;
			}
			// if we end up at the end of time then just
			// start @ the end-100msec
			if ((rewind > (latest-100000))  &&(_speedRate>0))
			{
				_speedRate = 10000;
				rewind = latest-100000;
				//Log.Write(" seek ff:{0}",rewind/10000000);
				hr = _mediaSeeking.SetPositions(new DsLong( rewind), AMSeekingSeekingFlags.AbsolutePositioning,new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
				_mediaCtrl.Run();
				return;
			}

			//seek to new moment in time
			//Log.Write(" seek :{0}",rewind/10000000);
			hr = _mediaSeeking.SetPositions(new DsLong( rewind), AMSeekingSeekingFlags.AbsolutePositioning		,new DsLong( pStop), AMSeekingSeekingFlags.NoPositioning);
			_mediaCtrl.Pause();

			elapsedTimer=DateTime.Now;
		}

		public override bool IsRadio
		{
			get
			{
				Log.Write("isradio() video:{0}",_hasVideo);
				if (_hasVideo) return false;
				return true;
			}
		}

		#region IDisposable Members

		public override void Release()
		{
			CloseInterfaces();
		}
		#endregion 
	}
}
