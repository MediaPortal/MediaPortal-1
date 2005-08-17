/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using DirectX.Capture;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using DShowNET;

namespace MediaPortal.Player 
{
	public class BaseStreamBufferPlayer : IPlayer
	{
		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
		protected class StreamBufferConfig {}
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		protected  static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

		public enum PlayState
		{
			Init,
			Playing,
			Paused,
      Ended
		}
		protected int      iSpeed=1;
		protected IStreamBufferConfigure2 streamConfig2 = null;
		protected StreamBufferConfig m_StreamBufferConfig	=null;


		protected int 															m_iPositionX=0;
		protected int 															m_iPositionY=0;
		protected int 															m_iWidth=200;
		protected int 															m_iHeight=100;
		protected int																m_iVideoWidth=100;
		protected int																m_iVideoHeight=100;
		protected string														m_strCurrentFile="";
		protected bool															m_bUpdateNeeded=false;
		protected bool															m_bFullScreen=true;
		protected PlayState													m_state=PlayState.Init;
		protected int																m_iVolume=100;
		protected  IGraphBuilder			  			      graphBuilder =null;
		protected  IStreamBufferSource 			        bufferSource=null ;
		protected  IStreamBufferMediaSeeking        m_mediaSeeking=null;
		protected  IStreamBufferMediaSeeking2       m_mediaSeeking2=null;
		protected int                               m_iSpeed=1;
		protected double                            m_dCurrentPos;
    protected double                            m_dContentStart;
		protected double                            m_dDuration=-1d;
    protected bool          										m_bStarted=false;
    protected bool          										m_bLive=false;
    protected double                            m_dLastPosition=0;
		protected bool                              m_bWindowVisible=false;
		protected int												rotCookie = 0;
		protected int                       m_aspectX=1;
		protected int                       m_aspectY=1;
		protected long                      m_speedRate = 10000;
		bool      usingNvidiaCodec=false;
		protected IBaseFilter								videoCodecFilter=null;
		protected IBaseFilter								audioCodecFilter=null;
		protected IBaseFilter								audioRendererFilter=null;
		protected IBaseFilter								ffdShowFilter=null;
		/// <summary> control interface. </summary>
		protected IMediaControl							mediaCtrl =null;

		/// <summary> graph event interface. </summary>
		protected IMediaEventEx							mediaEvt =null;

    
		/// <summary> video preview window interface. </summary>
		protected IVideoWindow							videoWin =null;

		/// <summary> interface to get information and control video. </summary>
		protected IBasicVideo2							basicVideo =null;

		/// <summary> audio interface used to control volume. </summary>
		protected IBasicAudio								basicAudio=null;
		protected uint minBackingFiles, maxBackingFiles,backingFileDuration;
		VMR7Util  vmr7 = null;
		DateTime  elapsedTimer=DateTime.Now;

		protected const int WM_GRAPHNOTIFY	= 0x00008001;	// message from graph
		protected const int WS_CHILD			= 0x40000000;	// attributes for video window
		protected const int WS_CLIPCHILDREN	= 0x02000000;
		protected const int WS_CLIPSIBLINGS	= 0x04000000;
		protected DateTime updateTimer=DateTime.Now;
		protected MediaPortal.GUI.Library.Geometry.Type             m_ar=MediaPortal.GUI.Library.Geometry.Type.Normal;

		public BaseStreamBufferPlayer()
		{
		}


		public override bool Play(string strFile)
		{
      if (!System.IO.File.Exists(strFile)) return false;
			iSpeed=1;
			m_speedRate = 10000;
			m_bLive=false;
      m_dDuration=-1d;
      string strExt=System.IO.Path.GetExtension(strFile).ToLower();
      if (strExt.Equals(".tv"))
      {
        m_bLive=true;
      }

			minBackingFiles=6;
			maxBackingFiles=8;
			int iTimeShiftBuffer=30;
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				iTimeShiftBuffer= xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
				if (iTimeShiftBuffer<5) iTimeShiftBuffer=5;
			}
			iTimeShiftBuffer*=60; //in seconds
			backingFileDuration = (uint)(iTimeShiftBuffer/6);

			m_bIsVisible=false;
      m_bWindowVisible=false;
			m_iVolume=100;
			m_state=PlayState.Init;
			m_strCurrentFile=strFile;
			m_bFullScreen=false;
			m_ar=MediaPortal.GUI.Library.Geometry.Type.Normal;

			m_bUpdateNeeded=true;
			Log.Write("StreamBufferPlayer:play {0}", strFile);
			GC.Collect();
			CloseInterfaces();
			GC.Collect();
			m_bStarted=false;
			if( ! GetInterfaces(strFile ))
			{
				Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:GetInterfaces() failed");
				m_strCurrentFile="";
				return false;
			}
			DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph
       
			int hr = mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
			if (hr < 0)
			{
				Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:SetNotifyWindow() failed");
				m_strCurrentFile="";
				CloseInterfaces();
				return false;
			}
			if (videoWin!=null)
			{
				videoWin.put_Owner( GUIGraphicsContext.ActiveForm );
				videoWin.put_WindowStyle( WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN );
        videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);

			}
			if (basicVideo!=null)
			{
				hr = basicVideo.GetVideoSize( out m_iVideoWidth, out m_iVideoHeight );
				if (hr < 0)
				{
					Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:GetVideoSize() failed");
					m_strCurrentFile="";
					CloseInterfaces();
					return false;
        }
        Log.Write("StreamBufferPlayer:VideoSize:{0}x{1}",m_iVideoWidth,m_iVideoHeight);	
			}
			GUIGraphicsContext.VideoSize=new Size(m_iVideoWidth, m_iVideoHeight);
        
			if (mediaCtrl==null)
			{
        Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:mediaCtrl==null");
        m_strCurrentFile="";
				CloseInterfaces();
				return false;
			}

      //DsUtils.DumpFilters(graphBuilder);

      m_iPositionX=GUIGraphicsContext.VideoWindow.X;
      m_iPositionY=GUIGraphicsContext.VideoWindow.Y;
      m_iWidth    =GUIGraphicsContext.VideoWindow.Width;
      m_iHeight   =GUIGraphicsContext.VideoWindow.Height;
      m_ar        =GUIGraphicsContext.ARType;
      m_bUpdateNeeded=true;
      SetVideoWindow();

			DirectShowUtil.EnableDeInterlace(graphBuilder);
			hr = mediaCtrl.Run();
			if (hr < 0)
			{
				Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:Rungraph failed");
				m_strCurrentFile="";
				CloseInterfaces();
				return false;
			}
			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED,0,0,0,0,0,null);
			msg.Label=strFile;
			GUIWindowManager.SendThreadMessage(msg);

			m_state=PlayState.Playing;

			if (m_bLive)
			{
				DateTime dt=DateTime.Now;
				do
				{
					UpdateDuration();
					Application.DoEvents();						
					TimeSpan ts=DateTime.Now-dt;
					if (ts.TotalSeconds>=2) break;
				} while (m_dDuration<1);

				UpdateCurrentPosition();
				double dPos=m_dDuration-5;
				if (dPos>=0 && CurrentPosition<dPos)
				{
					Log.Write("StreamBufferPlayer:Seek to end");
					SeekAbsolute(dPos);
				}
			}
			else
			{
				//seek to end of file
				long lTime=5*60*60;
				lTime*=10000000;
				long pStop=0;
				hr=m_mediaSeeking.SetPositions(ref lTime, SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
				if (hr==0)
				{
					long lStreamPos;
					m_mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
					m_dDuration=lStreamPos;
					m_dDuration/=10000000d;
					SeekAsolutePercentage(0);
				}
			}

			Log.Write("StreamBufferPlayer:playing duration:{0}",Utils.SecondsToHMSString( (int)m_dDuration) );
			m_state=PlayState.Playing;
			OnInitialized();
			return true;
		}

		public override void SetVideoWindow()
		{
			if (GUIGraphicsContext.IsFullScreenVideo!= m_bFullScreen)
			{
				m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
				m_bUpdateNeeded=true;
			}

			if (!m_bUpdateNeeded) return;
      
			m_bUpdateNeeded=false;
			m_bStarted=true;
			float x=m_iPositionX;
			float y=m_iPositionY;
      
			int nw=m_iWidth;
			int nh=m_iHeight;
			if (nw > GUIGraphicsContext.OverScanWidth)
				nw=GUIGraphicsContext.OverScanWidth;
			if (nh > GUIGraphicsContext.OverScanHeight)
				nh=GUIGraphicsContext.OverScanHeight;
			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				x=m_iPositionX=GUIGraphicsContext.OverScanLeft;
				y=m_iPositionY=GUIGraphicsContext.OverScanTop;
				nw=m_iWidth=GUIGraphicsContext.OverScanWidth;
				nh=m_iHeight=GUIGraphicsContext.OverScanHeight;
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
			if (basicVideo!=null)
			{
				basicVideo.GetVideoSize(out m_iVideoWidth, out m_iVideoHeight);
			}
			aspectX=m_iVideoWidth;
			aspectY=m_iVideoHeight;
			if (basicVideo!=null)
			{
				basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			GUIGraphicsContext.VideoSize=new Size(m_iVideoWidth, m_iVideoHeight);
			m_aspectX=aspectX;
			m_aspectY=aspectY;
			MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
			System.Drawing.Rectangle rSource,rDest;
			m_geometry.ImageWidth=m_iVideoWidth;
			m_geometry.ImageHeight=m_iVideoHeight;
			m_geometry.ScreenWidth=nw;
			m_geometry.ScreenHeight=nh;
			m_geometry.ARType=GUIGraphicsContext.ARType;
			m_geometry.PixelRatio=GUIGraphicsContext.PixelRatio;
			m_geometry.GetWindow(aspectX,aspectY,out rSource, out rDest);

			rDest.X += (int)x;
			rDest.Y += (int)y;

			Log.Write("overlay: video WxH  : {0}x{1}",m_iVideoWidth,m_iVideoHeight);
			Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
			Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
			Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
			Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
			Log.Write("overlay: src        : ({0},{1})-({2},{3})",
				rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
			Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
				rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);

        
			Log.Write("StreamBufferPlayer:Window ({0},{1})-({2},{3}) - ({4},{5})-({6},{7})", 
				rSource.X,rSource.Y, rSource.Right, rSource.Bottom,
				rDest.X, rDest.Y, rDest.Right, rDest.Bottom);


			if (rSource.Y==0)
			{
				rSource.Y+=5;
				rSource.Height-=10;
			}
			SetSourceDestRectangles(rSource,rDest);
			SetVideoPosition(rDest);
			m_SourceRect=rSource;
			m_VideoRect=rDest;
		}

		protected virtual void SetVideoPosition(System.Drawing.Rectangle rDest)
		{
			if (videoWin!=null)
			{
				if (rDest.Left< 0 || rDest.Top<0 || rDest.Width<=0 || rDest.Height<=0) return;
				videoWin.SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
			}
		}

		protected virtual void  SetSourceDestRectangles(System.Drawing.Rectangle rSource,System.Drawing.Rectangle rDest)
		{
			if (basicVideo!=null)
			{
				if (rSource.Left< 0 || rSource.Top<0 || rSource.Width<=0 || rSource.Height<=0) return;
				if (rDest.Width<=0 || rDest.Height<=0) return;
				basicVideo.SetSourcePosition(rSource.Left,rSource.Top,rSource.Width,rSource.Height);
				basicVideo.SetDestinationPosition(0,0,rDest.Width,rDest.Height);
			}
		}

		void MovieEnded()
		{
			// this is triggered only if movie has ended
			// ifso, stop the movie which will trigger MovieStopped
			
			Log.Write("StreamBufferPlayer:ended {0}", m_strCurrentFile);
      if (!IsTimeShifting)
      {
        CloseInterfaces();
        m_state=PlayState.Ended;
      }
      else
      {
        SeekAsolutePercentage(99);
      }
		}


    public override bool Ended
    {
      get { return m_state==PlayState.Ended;}
    }

		public override void Process()
		{
			if ( !Playing) return;
			if ( !m_bStarted) return;
			if (GUIGraphicsContext.InVmr9Render) return;
			TimeSpan ts=DateTime.Now-updateTimer;
			if (ts.TotalMilliseconds>=800 || iSpeed!=1) 
			{
				UpdateCurrentPosition();
				UpdateDuration();	
					
				updateTimer=DateTime.Now;
			}

			if (IsTimeShifting)
			{
				if (Speed>1 && CurrentPosition+5d >=Duration) 
				{
					Log.Write("BaseStreambufferPlayer: stop FFWD since end of timeshiftbuffer reached");
					Speed=1;
					SeekAsolutePercentage(99);
				}
				if (Speed<0 && CurrentPosition<5d)
				{
					Log.Write("BaseStreambufferPlayer: stop RWD since begin of timeshiftbuffer reached");
					Speed=1;
					SeekAsolutePercentage(0);
				}/*
				if (Speed<0 && CurrentPosition > m_dLastPosition)
				{
					Speed=1;
					SeekAsolutePercentage(0);
				}*/
			}
			m_dLastPosition=CurrentPosition;
				

			if (GUIGraphicsContext.VideoWindow.Width<=10&& GUIGraphicsContext.IsFullScreenVideo==false)
			{
				m_bIsVisible=false;
			}
			if (GUIGraphicsContext.BlankScreen)
			{
				m_bIsVisible=false;
			}
			if (m_bWindowVisible && !m_bIsVisible)
			{
				m_bWindowVisible=false;
				//Log.Write("StreamBufferPlayer:hide window");
				if (videoWin!=null) videoWin.put_Visible( DsHlp.OAFALSE );
			}
			else if (!m_bWindowVisible && m_bIsVisible)
			{
				m_bWindowVisible=true;
				//Log.Write("StreamBufferPlayer:show window");
				if (videoWin!=null) videoWin.put_Visible( DsHlp.OATRUE );
			}      
			
			OnProcess();
			CheckVideoResolutionChanges();
			if (m_speedRate!=10000)
			{
				DoFFRW();
			}
		}

		void CheckVideoResolutionChanges()
		{
			if (videoWin==null || basicVideo==null) return;
			int aspectX, aspectY;
			int videoWidth=1, videoHeight=1;
			if (basicVideo!=null)
			{
				basicVideo.GetVideoSize(out videoWidth, out videoHeight);
			}
			aspectX=videoWidth;
			aspectY=videoHeight;
			if (basicVideo!=null)
			{
				basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			if (videoHeight!=m_iVideoHeight || videoWidth != m_iVideoWidth ||
				  aspectX != m_aspectX || aspectY != m_aspectY)
			{
				m_bUpdateNeeded=true;
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
			get { return m_iPositionX;}
			set 
			{ 
				if (value != m_iPositionX)
				{
					m_iPositionX=value;
					m_bUpdateNeeded=true;
				}
			}
		}

		public override int PositionY
		{
			get { return m_iPositionY;}
			set 
			{
				if (value != m_iPositionY)
				{
					m_iPositionY=value;
					m_bUpdateNeeded=true;
				}
			}
		}

		public override int RenderWidth
		{
			get { return m_iWidth;}
			set 
			{
				if (value !=m_iWidth)
				{
					m_iWidth=value;
					m_bUpdateNeeded=true;
				}
			}
		}
		public override int RenderHeight
		{
			get { return m_iHeight;}
			set 
			{
				if (value != m_iHeight)
				{
					m_iHeight=value;
					m_bUpdateNeeded=true;
				}
			}
		}

		public override double Duration
		{
			get 
			{
				if (m_dDuration<0) Process();
				return m_dDuration;
			}
		}

		public override double CurrentPosition
		{
			get 
			{
				return m_dCurrentPos;
			}
		}

    public override double ContentStart
    {
      get {return m_dContentStart;}
    }
		public override bool FullScreen
		{
			get 
			{ 
				return GUIGraphicsContext.IsFullScreenVideo;
			}
			set
			{
				if (value != m_bFullScreen )
				{
					m_bFullScreen=value;
					m_bUpdateNeeded=true;          
				}
			}
		}
		public override int Width
		{
			get 
			{ 
				return m_iVideoWidth;
			}
		}

		public override int Height
		{
			get 
			{
				return m_iVideoHeight;
			}
		}

		public override void Pause()
		{
			if (m_state==PlayState.Paused) 
			{
				Log.Write("BaseStreamBufferPlayer:resume");
				Speed=1;
				mediaCtrl.Run();
				m_state=PlayState.Playing;
			}
			else if (m_state==PlayState.Playing) 
			{
				Log.Write("BaseStreamBufferPlayer:pause");
				m_state=PlayState.Paused;
				mediaCtrl.Pause();
			}
		}

		public override bool Paused
		{
			get 
			{
				return (m_state==PlayState.Paused);
			}
		}

		public override bool Playing
		{
			get 
			{ 
				return (m_state==PlayState.Playing||m_state==PlayState.Paused);
			}
		}

		public override bool Stopped
		{
			get 
			{ 
				return (m_state==PlayState.Init);
			}
		}

		public override string CurrentFile
		{
			get { return m_strCurrentFile;}
		}

		public override void Stop()
		{
			if (m_state!=PlayState.Init)
			{
				Log.Write("StreamBufferPlayer:stop");

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
				return m_iSpeed;
			}
			}
			set 
			{
				//lock (this)
			{
				if (m_iSpeed!=value)
				{
					m_iSpeed=value;
					if (m_mediaSeeking==null) return ;
					double dRate=value;
					int hr=m_mediaSeeking.SetRate(dRate);
					dRate=0;
					m_mediaSeeking.GetRate(out dRate);
					Log.Write("StreamBufferPlayer:SetRate to:{0} hr:{1} {2}", value,hr, dRate);
				}
			}
			}
		}
*/

		public override int Volume
		{
			get { return m_iVolume;}
			set 
			{
				if (m_iVolume!=value)
				{
					m_iVolume=value;
					if (m_state!=PlayState.Init)
					{
						if (basicAudio!=null)
						{
							// Divide by 100 to get equivalent decibel value. For example, –10,000 is –100 dB. 
							float fPercent=(float)m_iVolume/100.0f;
							int iVolume=(int)(5000.0f*fPercent);
							basicAudio.put_Volume( (iVolume-5000));
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
				if (m_ar != value)
				{
					m_ar=value;
					m_bUpdateNeeded=true;
				}
			}
		}

		public override void SeekRelative(double dTime)
		{
			if (m_state!=PlayState.Init)
			{
				if (mediaCtrl!=null && m_mediaSeeking!=null)
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

		public override void SeekAbsolute(double dTimeInSecs)
		{
			if (m_state!=PlayState.Init)
			{
				if (mediaCtrl!=null && m_mediaSeeking!=null)
        {
          if (dTimeInSecs<0.0d) dTimeInSecs=0.0d;
          if (dTimeInSecs>Duration) dTimeInSecs=Duration;
					dTimeInSecs=Math.Floor(dTimeInSecs);
          Log.Write("StreamBufferPlayer: seekabs: {0} duration:{1} current pos:{2}", dTimeInSecs,Duration, CurrentPosition);
          dTimeInSecs*=10000000d;
					long pStop=0;
          long lContentStart,lContentEnd;
          double fContentStart,fContentEnd;
          m_mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
          fContentStart=lContentStart;
          fContentEnd=lContentEnd;

          dTimeInSecs+=fContentStart;
          long lTime=(long)dTimeInSecs;
					int hr=m_mediaSeeking.SetPositions(ref lTime, SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
					if (hr !=0)
					{
						Log.WriteFile(Log.LogType.Log,true,"seek failed->seek to 0 0x:{0:X}",hr);
					}
				}
        UpdateCurrentPosition();
        Log.Write("StreamBufferPlayer: current pos:{0}", CurrentPosition);

			}
		}
#if DEBUG
    static DateTime dtStart=DateTime.Now;
#endif
    void UpdateCurrentPosition()
    {
      if (m_mediaSeeking==null) return;
      //GetCurrentPosition(): Returns stream position. 
      //Stream position:The current playback position, relative to the content start
      long lStreamPos;
      double fCurrentPos;
      m_mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
      fCurrentPos=lStreamPos;
      fCurrentPos/=10000000d;

      long lContentStart,lContentEnd;
      double fContentStart,fContentEnd;
      m_mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
      fContentStart=lContentStart;
      fContentEnd=lContentEnd;
      fContentStart/=10000000d;
      fContentEnd/=10000000d;
      double fPos=m_dCurrentPos;
      fCurrentPos-=fContentStart;
      m_dCurrentPos=fCurrentPos;
      m_dContentStart=fContentStart;
#if DEBUG
      TimeSpan ts=DateTime.Now-dtStart;
      if (ts.TotalMilliseconds>=1000)
      {
        long lDuration;
        double fDuration;
        m_mediaSeeking.GetDuration(out lDuration); 
        fDuration=lDuration;
        fDuration/=10000000d;

        //Log.Write("pos:{0} content:{1}-{2} duration:{3} stream:{4}",m_dCurrentPos,fContentStart,fContentEnd,fDuration,fPos);
                  
        dtStart=DateTime.Now;
      }
#endif
    }
		void UpdateDuration()
		{
			//GetDuration(): Returns (content start – content stop). 
			//content start: The time of the earliest available content. 
			//               For live content, the value starts at zero and increases whenever the 
			//               Stream Buffer Engine deletes an old file. 				
			//content stop : The time of the latest available content. For live content, this value starts at zero 
			//               and increases continuously.
			
			long lDuration;
			m_mediaSeeking.GetDuration(out lDuration); 
			m_dDuration=lDuration;
			m_dDuration/=10000000d;
		}

		public override void SeekRelativePercentage(int iPercentage)
		{
			if (m_state!=PlayState.Init)
			{
				if (mediaCtrl!=null && m_mediaSeeking!=null)
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
			if (m_state!=PlayState.Init)
			{
				if (mediaCtrl!=null && m_mediaSeeking!=null)
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
			get { return true;}
		}

		/// <summary> create the used COM components and get the interfaces. </summary>
		protected virtual bool GetInterfaces(string filename)
		{
      
			Type comtype = null;
			object comobj = null;
			try 
			{
				comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
				if( comtype == null )
				{
					Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:DirectX 9 not installed");
					return false;
				}
				comobj = Activator.CreateInstance( comtype );
				graphBuilder = (IGraphBuilder) comobj; comobj = null;

				vmr7=new VMR7Util();
				vmr7.AddVMR7(graphBuilder);

				Guid clsid = Clsid.StreamBufferSource;
				Guid riid = typeof(IStreamBufferSource).GUID;
				Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
				bufferSource = (IStreamBufferSource) comObj; comObj = null;

				int hr;
				m_StreamBufferConfig	= new StreamBufferConfig();
				streamConfig2	= m_StreamBufferConfig as IStreamBufferConfigure2;
				if (streamConfig2!=null)
				{
					// setting the StreamBufferEngine registry key
					IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
					IStreamBufferInitialize pTemp = (IStreamBufferInitialize) streamConfig2;
					IntPtr subKey = IntPtr.Zero;

					RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
					hr=pTemp.SetHKEY(subKey);
					hr=streamConfig2.SetFFTransitionRates(8,32);	
					//Log.Write("set FFTransitionRates:{0:X}",hr);
					
					uint max,maxnon;
					hr=streamConfig2.GetFFTransitionRates(out max,out maxnon);	
					//Log.Write("get FFTransitionRates:{0} {1} {2:X}",max,maxnon,hr);
					streamConfig2.GetBackingFileCount(out minBackingFiles, out maxBackingFiles);
					streamConfig2.GetBackingFileDuration(out backingFileDuration);

				}


		
				IBaseFilter filter = (IBaseFilter) bufferSource;
				graphBuilder.AddFilter(filter, "SBE SOURCE");
		
				IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
				hr = fileSource.Load(filename, IntPtr.Zero);

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
				if (strVideoCodec.Length>0) videoCodecFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
				if (strAudioCodec.Length>0) audioCodecFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				if (strAudiorenderer.Length>0) audioRendererFilter=DirectShowUtil.AddAudioRendererToGraph(graphBuilder,strAudiorenderer,false);
        if (bAddFFDshow) ffdShowFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

				if (strVideoCodec.ToLower().IndexOf("nvidia")>=0)
					usingNvidiaCodec=true;

				//render outputpins of SBE
				DirectShowUtil.RenderOutputPins(graphBuilder, (IBaseFilter)fileSource);

				mediaCtrl	= (IMediaControl)  graphBuilder;
				videoWin	= graphBuilder as IVideoWindow;
				mediaEvt	= (IMediaEventEx)  graphBuilder;
				m_mediaSeeking = bufferSource as IStreamBufferMediaSeeking ;
				m_mediaSeeking2= bufferSource as IStreamBufferMediaSeeking2 ;
				if (m_mediaSeeking==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"Unable to get IMediaSeeking interface#1");
				}
				if (m_mediaSeeking2==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"Unable to get IMediaSeeking interface#2");
				}

				basicVideo	= graphBuilder as IBasicVideo2;
				basicAudio	= graphBuilder as IBasicAudio;
				
				//Log.Write("StreamBufferPlayer:SetARMode");
				DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);

				graphBuilder.SetDefaultSyncSource();
				//Log.Write("StreamBufferPlayer: set Deinterlace");

				//Log.Write("StreamBufferPlayer: done");
				return true;
			}
			catch( Exception  ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
				return false;
			}
			finally
			{
				if( comobj != null )
					Marshal.ReleaseComObject( comobj ); comobj = null;
			}
		}



		/// <summary> do cleanup and release DirectShow. </summary>
		protected virtual void CloseInterfaces()
		{
			int hr;
			if (graphBuilder==null) return;
			Log.Write("StreamBufferPlayer:cleanup DShow graph");
			try 
			{


				if( mediaCtrl != null )
				{
					hr = mediaCtrl.Stop();

					mediaCtrl = null;
				}

				m_state = PlayState.Init;

				mediaEvt = null;			
        m_bWindowVisible=false;
				m_bIsVisible=false;
				videoWin = null;
				m_mediaSeeking= null;
				m_mediaSeeking2= null;
				basicAudio	= null;
				basicVideo	= null;
				bufferSource=null;

				
				if (videoCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(videoCodecFilter))>0); 
					videoCodecFilter=null;
				}
				if (audioCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(audioCodecFilter))>0); 
					audioCodecFilter=null;
				}
				
				if (audioRendererFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(audioRendererFilter))>0); 
					audioRendererFilter=null;
				}
				
				if (ffdShowFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(ffdShowFilter))>0); 
					ffdShowFilter=null;
				}
				if (streamConfig2!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(streamConfig2))>0); 
					streamConfig2=null;
				}
				m_StreamBufferConfig=null;

				if (vmr7!=null)
					vmr7.RemoveVMR7();
				vmr7=null;

        DsUtils.RemoveFilters(graphBuilder);

        if( rotCookie != 0 )
          DsROT.RemoveGraphFromRot( ref rotCookie );
        rotCookie=0;

				if( graphBuilder != null )
				{
					while ( (hr=Marshal.ReleaseComObject( graphBuilder ))>0); 
				}
				graphBuilder = null;

				m_state = PlayState.Init;
				GUIGraphicsContext.form.Invalidate(true);
				GC.Collect();GC.Collect();GC.Collect();
			}
			catch( Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"StreamBufferPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
			}
			//Log.Write("StreamBufferPlayer:cleanup done");
		}

		public override void WndProc( ref Message m )
		{
			if( m.Msg == WM_GRAPHNOTIFY )
			{
				if( mediaEvt != null )
					OnGraphNotify();
				return;
			}
			base.WndProc( ref m );
		}

		void OnGraphNotify()
		{
			int p1, p2, hr = 0;
			DsEvCode code;
			int counter=0;
			do
			{
				counter++;
        if (/*Playing && */mediaEvt!=null)
        {
          hr = mediaEvt.GetEvent( out code, out p1, out p2, 0 );
					if( hr == 0 )
					{
						hr = mediaEvt.FreeEventParams( code, p1, p2 );
						if (code>=DsEvCode.StreamBufferTimeHole && code <= DsEvCode.StreamBufferRateChanged)
						{
							Log.Write("StreamBufferPlayer: event:{0} param1:{1} param2:{2} param1:0x{3:X} param2:0x{4:X}",code.ToString(),p1,p2,p1,p2);
							long contentStart,contentStop,streamPosition,segmentstop;
							double fcontentStart,fcontentStop,fstreamPosition,fsegmentstop;
							m_mediaSeeking.GetAvailable(out contentStart, out contentStop);
							m_mediaSeeking.GetCurrentPosition(out streamPosition);
							m_mediaSeeking.GetStopPosition(out segmentstop);
							fcontentStart=(double)contentStart;
							fcontentStop=(double)contentStop;
							fstreamPosition=(double)streamPosition;
							fsegmentstop=(double)segmentstop;

							fcontentStart/=10000000d;
							fcontentStop/=10000000d;
							fstreamPosition/=10000000d;
							fsegmentstop/=10000000d;
							Log.Write("StreamBufferPlayer:  content start   :{0} content stop :{1}", fcontentStart.ToString("f2"), fcontentStop.ToString("f2"));
							Log.Write("StreamBufferPlayer:  streamPosition  :{0} segment stop :{1}", fstreamPosition.ToString("f2"), fsegmentstop.ToString("f2"));
							if (code==DsEvCode.StreamBufferTimeHole)
							{
								//The Stream Buffer Source filter has reached a gap in the content. 
								//p1 = Time of the start of the gap, in milliseconds, relative to the content start.
								//param2 = Duration of the gap, in milliseconds.
								double newpos=(double)p1 + (double)p2 + contentStart;
								newpos /=1000d;
								if (newpos <fstreamPosition-1 || newpos > fstreamPosition+1)
								{
									Log.Write("StreamBufferPlayer:  seek to:{0}", newpos.ToString("f2"));
									SeekAbsolute(1d+newpos);
								}
							}
							if (code==DsEvCode.StreamBufferContentBecomingStale)
							{
								if (Paused)
								{
									Log.Write("StreamBufferPlayer:  unpause");
									Pause();
								}
							}
						}
						else if( code == DsEvCode.Complete || code== DsEvCode.ErrorAbort)
						{
							Log.Write("StreamBufferPlayer: event:{0} param1:{1} param2:{2} param1:0x{3:X} param2:0x{4:X}",code.ToString(),p1,p2,p1,p2);
							MovieEnded();
						}
						else
							Log.Write("StreamBufferPlayer: event:{0} 0x{1:X} param1:{2} param2:{3} param1:0x{4:X} param2:0x{5:X}",code.ToString(), (int)code,p1,p2,p1,p2);
					}
					else break;
        }
        else  break;
			}
			while( hr == 0 && counter < 20);
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
			get {return m_bLive;}
		}      

    public override bool Visible
    {
      get {return m_bIsVisible;}
      set
      { 
        if (value==m_bIsVisible) return;
        m_bIsVisible=value;
        m_bUpdateNeeded=true;
      }
    }

		public override int Speed
		{
			get 
			{ 
				if (m_state==PlayState.Init) return 1;
				if (m_mediaSeeking==null) return 1;
				if (usingNvidiaCodec) return iSpeed;
				switch ( m_speedRate)
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
				if (m_state!=PlayState.Init)
				{
					if (usingNvidiaCodec)
					{
						if (iSpeed!=value)
						{
							iSpeed=value;
							if (m_mediaSeeking2!=null)
							{
								int hr=m_mediaSeeking2.SetRateEx((double)iSpeed,25);
								//Log.Write("VideoPlayer:SetRateEx to:{0} 25fps {1:X}", iSpeed,hr);
								if (hr!=0)
								{
									IMediaSeeking oldMediaSeek=graphBuilder as IMediaSeeking;
									hr=oldMediaSeek.SetRate((double)iSpeed);
									//Log.Write("VideoPlayer:SetRateOld to:{0} {1:X}", iSpeed,hr);
								}
							}
							else
							{
								int hr=m_mediaSeeking.SetRate((double)iSpeed);
								//Log.Write("VideoPlayer:SetRate to:{0} {1:X}", iSpeed,hr);
								if (hr!=0)
								{
									IMediaSeeking oldMediaSeek=graphBuilder as IMediaSeeking;
									hr=oldMediaSeek.SetRate((double)iSpeed);
									//Log.Write("VideoPlayer:SetRateOld to:{0} {1:X}", iSpeed,hr);
								}
							}
							if (iSpeed==1)
							{
								mediaCtrl.Stop();
								Application.DoEvents();
								System.Threading.Thread.Sleep(200);
								Application.DoEvents();
								FilterState state;
								mediaCtrl.GetState(100,out state);
								//Log.Write("state:{0}", state.ToString());
								mediaCtrl.Run();
							}
						}
					}
					else
					{
						switch ( (int)value)
						{
							case -1:  m_speedRate=-10000;break;
							case -2:  m_speedRate=-15000;break;
							case -4:  m_speedRate=-30000;break;
							case -8:  m_speedRate=-45000;break;
							case -16: m_speedRate=-60000;break;
							case -32: m_speedRate=-75000;break;

							case 1:  
								m_speedRate=10000;
								mediaCtrl.Run();
								break;
							case 2:  m_speedRate=15000;break;
							case 4:  m_speedRate=30000;break;
							case 8:  m_speedRate=45000;break;
							case 16: m_speedRate=60000;break;
							default: m_speedRate=75000;break;
						}
					}
				}
			}
		}


		public override void ContinueGraph()
		{

			if (mediaCtrl==null) return;

			Log.Write("StreamBufferPlayer:ContinueGraph");
			//mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
			//mediaCtrl.Stop();

			Application.DoEvents();
			FilterState state;
			mediaCtrl.GetState(200,out state);
			Application.DoEvents();
			Log.Write("state:{0}", state.ToString());


			IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
			int hr = fileSource.Load(CurrentFile, IntPtr.Zero);
			SeekAbsolute(0);
			
			mediaCtrl.Run();
			mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
			m_state=PlayState.Playing;
			mediaCtrl.GetState(200,out state);
			Application.DoEvents();
			Log.Write("state:{0}", state.ToString());
		}

		public override void PauseGraph()
		{
			Log.Write("StreamBufferPlayer:Pause graph");
			mediaEvt.SetNotifyWindow( IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero );
			mediaCtrl.Stop();
		}
		

		protected virtual void OnInitialized()
		{
		}
		protected void DoFFRW()
		{

			if (!Playing) 
				return;
      
			if ((m_speedRate == 10000) || (m_mediaSeeking == null))
				return;
			TimeSpan ts=DateTime.Now-elapsedTimer;
			if (ts.TotalMilliseconds<100) return;
			long earliest, latest, current,  stop, rewind, pStop;
		
			m_mediaSeeking.GetAvailable(out earliest, out latest);
			m_mediaSeeking.GetPositions(out current, out stop);

			// Log.Write("earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
			//         earliest/10000000,latest/10000000,current/10000000,stop/10000000,m_speedRate, (latest-earliest)/10000000);
      
			//earliest += + 30 * 10000000;

			// new time = current time + 2*timerinterval* (speed)
			long lTimerInterval=(long)ts.TotalMilliseconds;
			if (lTimerInterval > 300) lTimerInterval=300;
			lTimerInterval=300;
			rewind = (long)(current + (2 *(long)(lTimerInterval)* m_speedRate)) ;

			int hr; 		
			pStop  = 0;
		
			// if we end up before the first moment of time then just
			// start @ the beginning
			if ((rewind < earliest) && (m_speedRate<0))
			{
				m_speedRate = 10000;
				rewind = earliest;
				//Log.Write(" seek back:{0}",rewind/10000000);
				hr = m_mediaSeeking.SetPositions(ref rewind, SeekingFlags.AbsolutePositioning	,ref pStop, SeekingFlags.NoPositioning);
				mediaCtrl.Run();
				return;
			}
			// if we end up at the end of time then just
			// start @ the end-100msec
			if ((rewind > (latest-100000))  &&(m_speedRate>0))
			{
				m_speedRate = 10000;
				rewind = latest-100000;
				//Log.Write(" seek ff:{0}",rewind/10000000);
				hr = m_mediaSeeking.SetPositions(ref rewind, SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
				mediaCtrl.Run();
				return;
			}

			//seek to new moment in time
			//Log.Write(" seek :{0}",rewind/10000000);
			hr = m_mediaSeeking.SetPositions(ref rewind, SeekingFlags.AbsolutePositioning		,ref pStop, SeekingFlags.NoPositioning);
			mediaCtrl.Pause();

			elapsedTimer=DateTime.Now;
		}

		#region IDisposable Members

		public override void Release()
		{
			CloseInterfaces();
		}
		#endregion 
	}
}
