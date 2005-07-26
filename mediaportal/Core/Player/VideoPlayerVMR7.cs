using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectX.Capture;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;


using MediaPortal.GUI.Library;
using DShowNET;
namespace MediaPortal.Player 
{
  public class VideoPlayerVMR7 : IPlayer
  {
    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }
    protected int 											m_iPositionX=0;
    protected int 											m_iPositionY=0;
    protected int 											m_iWidth=200;
    protected int 											m_iHeight=100;
    protected int                       m_iVideoWidth=100;
    protected int                       m_iVideoHeight=100;
    protected string                    m_strCurrentFile="";
    protected bool											m_bUpdateNeeded=false;
    protected MediaPortal.GUI.Library.Geometry.Type             m_ar=MediaPortal.GUI.Library.Geometry.Type.Normal;
    protected bool											m_bFullScreen=true;
    protected PlayState								  m_state=PlayState.Init;
    protected int                       m_iVolume=100;
    protected IGraphBuilder			        graphBuilder;
    protected long                      m_speedRate = 10000;
		protected double                    m_dCurrentPos;
		protected double                    m_dDuration;
		protected int                       m_aspectX=1;
		protected int                       m_aspectY=1;

    protected bool                      m_bStarted=false;
    protected int		                    rotCookie = 0;

		/// <summary> control interface. </summary>
    protected IMediaControl			        mediaCtrl;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx			        mediaEvt;

    /// <summary> seek interface for positioning in stream. </summary>
    protected IMediaSeeking			        mediaSeek;
    /// <summary> seek interface to set position in stream. </summary>
    protected IMediaPosition			      mediaPos;
    /// <summary> video preview window interface. </summary>
    protected IVideoWindow			        videoWin;
    /// <summary> interface to get information and control video. </summary>
    protected IBasicVideo2			        basicVideo;
    /// <summary> interface to single-step video. </summary>
		protected IBaseFilter								videoCodecFilter=null;
		protected IBaseFilter								audioCodecFilter=null;
		protected IBaseFilter								audioRendererFilter=null;
		protected IBaseFilter								ffdShowFilter=null;
    
    protected IDirectVobSub			        vobSub;
		DateTime  elapsedTimer=DateTime.Now;

    /// <summary> audio interface used to control volume. </summary>
    protected IBasicAudio				basicAudio;
    protected const int WM_GRAPHNOTIFY	= 0x00008001;	// message from graph

    protected const int WS_CHILD			= 0x40000000;	// attributes for video window
    protected const int WS_CLIPCHILDREN	= 0x02000000;
    protected const int WS_CLIPSIBLINGS	= 0x04000000;
    protected bool        m_bVisible=false;
		protected DateTime    updateTimer;
		VMR7Util  vmr7 = null;
		protected struct  FilterStreamInfos
		{
			public int      Id;
			public string   Name;
			public bool      Current;
			public string   Filter;
		};
		protected const int MAX_VIDEOSTREAMS = 20;
		protected const int MAX_AUDIOSTREAMS = 20;
		protected const int MAX_SUBSTREAMS = 20;
		protected int cStreams_Audio=0;
		protected int cStreams_Video=0;
		protected int cStreams_Sub=0;   
		protected int StopSubId=0;   
		protected FilterStreamInfos[] sStreams_Audio;
		protected FilterStreamInfos[] sStreams_Video;
		protected FilterStreamInfos[] sStreams_Sub;

    public VideoPlayerVMR7()
    {
    }


    public override bool Play(string strFile)
    {
			updateTimer=DateTime.Now;
			m_speedRate = 10000;
      m_bVisible=false;
      m_iVolume=100;
      m_state=PlayState.Init;
      m_strCurrentFile=strFile;
      m_bFullScreen=true;
      m_ar=GUIGraphicsContext.ARType;

      m_bUpdateNeeded=true;
      Log.Write("VideoPlayer:play {0}", strFile);
      //lock ( typeof(VideoPlayerVMR7) )
      {
        GC.Collect();
        CloseInterfaces();
        GC.Collect();
        m_bStarted=false;
        if( ! GetInterfaces() )
        {
          m_strCurrentFile="";
          return false;
        }
        int hr = mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero );
        if (hr < 0)
        {
          Error.SetError("Unable to play movie", "Can not set notifications");
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
            Error.SetError("Unable to play movie", "Can not find movie width/height");
						m_strCurrentFile="";
						CloseInterfaces();
						return false;
					}
				}

        /*
        GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target, Color.Black, 1.0f, 0);
        try
        {
          // Show the frame on the primary surface.
          GUIGraphicsContext.DX9Device.Present();
        }
        catch(DeviceLostException)
        {
        }*/

        DirectShowUtil.SetARMode(graphBuilder, AmAspectRatioMode.AM_ARMODE_STRETCHED);
        DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph

       // DsUtils.DumpFilters(graphBuilder);
        hr = mediaCtrl.Run();
        if (hr < 0)
        {
          Error.SetError("Unable to play movie", "Unable to start movie");
          m_strCurrentFile="";
          CloseInterfaces();
          return false;
        }
        GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED,0,0,0,0,0,null);
        msg.Label=strFile;
        GUIWindowManager.SendThreadMessage(msg);
        m_state=PlayState.Playing;
        GUIGraphicsContext.IsFullScreenVideo=true;
        m_iPositionX=GUIGraphicsContext.VideoWindow.X;
        m_iPositionY=GUIGraphicsContext.VideoWindow.Y;
        m_iWidth    =GUIGraphicsContext.VideoWindow.Width;
        m_iHeight   =GUIGraphicsContext.VideoWindow.Height;
        m_ar        =GUIGraphicsContext.ARType;
        m_bUpdateNeeded=true;
        SetVideoWindow();
        mediaPos.get_Duration(out m_dDuration);
        Log.Write("VideoPlayer:Duration:{0}",m_dDuration);
        
				AnalyseStreams(); 

				OnInitialized();
      }
      return true;
    }

    public override void SetVideoWindow()
    {
			if (GUIGraphicsContext.Vmr9Active) 
			{
				m_bUpdateNeeded=false;
				m_bStarted=true;
				return;
			}
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
      //lock ( typeof(VideoPlayerVMR7) )
      {
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          x=m_iPositionX=GUIGraphicsContext.OverScanLeft;
          y=m_iPositionY=GUIGraphicsContext.OverScanTop;
          nw=m_iWidth=GUIGraphicsContext.OverScanWidth;
          nh=m_iHeight=GUIGraphicsContext.OverScanHeight;
				}
				if (x  < 0 || y  < 0) return;
				if (nw <=0 || nh <=0) return;
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
				m_aspectX=aspectX;
				m_aspectY=aspectY;

				
				GUIGraphicsContext.VideoSize=new Size(m_iVideoWidth, m_iVideoHeight);

        System.Drawing.Rectangle rSource,rDest;
        MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
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


				SetSourceDestRectangles(rSource,rDest);
				SetVideoPosition(rDest);
   

        m_SourceRect=rSource;
        m_VideoRect=rDest;
      }
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

    void MovieEnded(bool bManualStop)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      m_strCurrentFile="";
      if (!bManualStop)
      {
        CloseInterfaces();
        m_state=PlayState.Ended;
        GUIGraphicsContext.IsPlaying=false;
      }
    }


    public override void Process()
    {
      if ( !Playing) return;
      if ( !m_bStarted) return;
			if (GUIGraphicsContext.InVmr9Render) return;
			TimeSpan ts=DateTime.Now-updateTimer;
			if (ts.TotalMilliseconds>=800 || m_speedRate!=1) 
			{
				if (mediaPos!=null)
				{
					//mediaPos.get_Duration(out m_dDuration);
					mediaPos.get_CurrentPosition(out m_dCurrentPos);
				}

				if (GUIGraphicsContext.BlankScreen||(GUIGraphicsContext.Overlay==false && GUIGraphicsContext.IsFullScreenVideo==false))
				{
					if (m_bVisible)
					{
						m_bVisible=false;
						if (videoWin!=null) videoWin.put_Visible( DsHlp.OAFALSE );
					}
				}
				else if (!m_bVisible)
				{
					m_bVisible=true;
					if (videoWin!=null) videoWin.put_Visible( DsHlp.OATRUE );
				}      
				CheckVideoResolutionChanges();
				updateTimer=DateTime.Now;
			}
			if (m_speedRate!=1)
			{
				DoFFRW();
			}
			OnProcess();
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
				if (m_state!=PlayState.Init) 
				{
					return m_dDuration;
				}
				return 0.0d;
			}
		}

		public override double CurrentPosition
		{
			get 
			{
				if (m_state!=PlayState.Init) 
				{
					return m_dCurrentPos;
				}
				return 0.0d;
			}
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
        m_speedRate = 10000;
        mediaCtrl.Run();
        m_state=PlayState.Playing;
      }
      else if (m_state==PlayState.Playing) 
      {
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
        Log.Write("VideoPlayer:ended {0}", m_strCurrentFile);
        m_strCurrentFile="";
        CloseInterfaces();
        m_state=PlayState.Init;
        GUIGraphicsContext.IsPlaying=false;

      }
    }
    
    public override int Speed
    {
      get 
      { 
        if (m_state==PlayState.Init) return 1;
        if (mediaSeek==null) return 1;
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
          if (mediaSeek!=null)
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
        Log.Write("VideoPlayer:SetRate to:{0}", m_speedRate);
      }
    }


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
              int iVolume=(int)( (DirectShowVolume.VOLUME_MAX-DirectShowVolume.VOLUME_MIN) *fPercent);
              basicAudio.put_Volume( (iVolume-DirectShowVolume.VOLUME_MIN));
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
        if (mediaCtrl!=null && mediaPos!=null)
        {
          
          double dCurTime;
          mediaPos.get_CurrentPosition(out dCurTime);
          
          dTime=dCurTime+dTime;
          if (dTime<0.0d) dTime=0.0d;
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          if (dTime<0.0d) dTime=0.0d;
          if (dTime < Duration)
          {
						Log.Write("seekabs:{0}",dTime);
						mediaPos.put_CurrentPosition(dTime);
						Log.Write("seekabs:{0} done",dTime);
          }
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          double dCurrentPos;
          mediaPos.get_CurrentPosition(out dCurrentPos);
          double dDuration=Duration;

          double fCurPercent=(dCurrentPos/Duration)*100.0d;
          double fOnePercent=Duration/100.0d;
          fCurPercent=fCurPercent + (double)iPercentage;
          fCurPercent*=fOnePercent;
          if (fCurPercent<0.0d) fCurPercent=0.0d;
          if (fCurPercent<Duration)
          {
            mediaPos.put_CurrentPosition(fCurPercent);
          }
        }
      }
    }


    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          if (iPercentage<0) iPercentage=0;
          if (iPercentage>=100) iPercentage=100;
          double fPercent=Duration/100.0f;
          fPercent*=(double)iPercentage;
          mediaPos.put_CurrentPosition(fPercent);
        }
      }
    }

    
    public override bool HasVideo
    {
      get { return true;}
    }

    public override bool Ended
    {
      get { return m_state==PlayState.Ended;}
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected virtual bool GetInterfaces()
    {
      Type comtype = null;
      object comobj = null;
      try 
      {
        comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
        if( comtype == null )
        {
          Log.WriteFile(Log.LogType.Log,true,"VideoPlayer:DirectX 9 not installed");
            return false;
        }
        comobj = Activator.CreateInstance( comtype );
        graphBuilder = (IGraphBuilder) comobj; comobj = null;
			
				vmr7=new VMR7Util();
				vmr7.AddVMR7(graphBuilder);
				// add preferred video & audio codecs
				string strVideoCodec="";
				string strAudioCodec="";
				string strAudiorenderer="";
        bool   bAddFFDshow=false;
				string defaultLanguage;
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          bAddFFDshow=xmlreader.GetValueAsBool("movieplayer","ffdshow",false);
					strVideoCodec=xmlreader.GetValueAsString("movieplayer","mpeg2videocodec","");
					strAudioCodec=xmlreader.GetValueAsString("movieplayer","mpeg2audiocodec","");
					strAudiorenderer=xmlreader.GetValueAsString("movieplayer","audiorenderer","");
					defaultLanguage= xmlreader.GetValueAsString("subtitles", "language", "English");

				}
				string strExt=System.IO.Path.GetExtension(m_strCurrentFile).ToLower();
				if (strExt.Equals(".dvr-ms") ||strExt.Equals(".mpg") ||strExt.Equals(".mpeg")||strExt.Equals(".bin")||strExt.Equals(".dat"))
				{
					if (strVideoCodec.Length>0) videoCodecFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
					if (strAudioCodec.Length>0) audioCodecFilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
				}
				if (bAddFFDshow) ffdShowFilter= DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");
				if (strAudiorenderer.Length>0) audioRendererFilter= DirectShowUtil.AddAudioRendererToGraph(graphBuilder,strAudiorenderer,false);


        int hr = graphBuilder.RenderFile( m_strCurrentFile, null );
        if( hr < 0 )
          Marshal.ThrowExceptionForHR( hr );

			
        IBaseFilter filter;
        ushort b;
        unchecked
        {
          b=(ushort)0xfffff845;
        }
        Guid classID=new Guid(0x9852a670,b,0x491b,0x9b,0xe6,0xeb,0xd8,0x41,0xb8,0xa6,0x13);
        DsUtils.FindFilterByClassID(graphBuilder,  classID, out filter);

        vobSub = null;
        vobSub = filter as IDirectVobSub;
        if (vobSub!=null)
        {
          using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
          {
            string strTmp="";
            string strFont=xmlreader.GetValueAsString("subtitles","fontface","Arial");
            int    iFontSize=xmlreader.GetValueAsInt("subtitles","fontsize",18);
            bool   bBold=xmlreader.GetValueAsBool("subtitles","bold",true);
            
            strTmp=xmlreader.GetValueAsString("subtitles","color","ffffff");
            long iColor=Convert.ToInt64(strTmp,16);
            int  iShadow=xmlreader.GetValueAsInt("subtitles","shadow",5);
          
            LOGFONT logFont = new LOGFONT();
            int color;
            bool fShadow, fOutLine, fAdvancedRenderer = false;
            int size = Marshal.SizeOf(typeof(LOGFONT));
            vobSub.get_TextSettings(logFont, size,out color, out fShadow, out fOutLine, out fAdvancedRenderer);

            FontStyle fontStyle=FontStyle.Regular;
            if (bBold) fontStyle=FontStyle.Bold;
            System.Drawing.Font Subfont = new System.Drawing.Font(strFont,iFontSize,fontStyle,System.Drawing.GraphicsUnit.Point, 1);
            Subfont.ToLogFont(logFont);
            int R=(int)((iColor>>16)&0xff);
            int G=(int)((iColor>>8)&0xff);
            int B=(int)((iColor)&0xff);
            color=(B<<16)+(G<<8)+R;
            if (iShadow>0) fShadow=true;
            int res = vobSub.put_TextSettings(logFont, size, color,  fShadow, fOutLine, fAdvancedRenderer);
          }

					for (int i=0; i < SubtitleStreams;++i)
					{
						string language=SubtitleLanguage(i);
						if (String.Compare(language,defaultLanguage,true)==0)
						{
							CurrentSubtitleStream=i;
							break;
						}
					}
        }
        if( filter != null )
          Marshal.ReleaseComObject( filter ); filter = null;

        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        mediaSeek	= (IMediaSeeking)  graphBuilder;
        mediaPos	= (IMediaPosition) graphBuilder;

        videoWin	= graphBuilder as IVideoWindow;
        basicVideo	= graphBuilder as IBasicVideo2;
        basicAudio	= graphBuilder as IBasicAudio;
				

        DirectShowUtil.SetARMode(graphBuilder,AmAspectRatioMode.AM_ARMODE_STRETCHED);
        DirectShowUtil.EnableDeInterlace(graphBuilder);
        return true;
      }
      catch( Exception  ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"VideoPlayer:exception while creating DShow graph {0} {1}",ex.Message, ex.StackTrace);
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
			if (graphBuilder==null) return;
      int hr;
      Log.Write("VideoPlayer:cleanup DShow graph");
      try 
      {

        if( mediaCtrl != null )
        {
          hr = mediaCtrl.Stop();
          mediaCtrl = null;
        }

        m_state = PlayState.Init;

				mediaEvt = null;
				if (vmr7!=null)
					vmr7.RemoveVMR7();
				vmr7=null;
        m_bVisible=false;
				videoWin = null;
				mediaSeek = null;
				mediaPos = null;
				basicVideo = null;
				basicAudio = null;
				
				if (videoCodecFilter!=null) Marshal.ReleaseComObject(videoCodecFilter); videoCodecFilter=null;
				if (audioCodecFilter!=null) Marshal.ReleaseComObject(audioCodecFilter); audioCodecFilter=null;
				if (audioRendererFilter!=null) Marshal.ReleaseComObject(audioRendererFilter); audioRendererFilter=null;
				if (ffdShowFilter!=null) Marshal.ReleaseComObject(ffdShowFilter); ffdShowFilter=null;
				if( vobSub != null )
				{
					while((hr=Marshal.ReleaseComObject( vobSub))>0); 
					vobSub = null;
				}

        DsUtils.RemoveFilters(graphBuilder);

        if( rotCookie != 0 )
          DsROT.RemoveGraphFromRot( ref rotCookie );
        rotCookie=0;

				if( graphBuilder != null )
				{
					while((hr=Marshal.ReleaseComObject( graphBuilder ))>0); 
					graphBuilder = null;
				}

        m_state = PlayState.Init;
        GC.Collect();GC.Collect();GC.Collect();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"VideoPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }
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
      if (mediaEvt==null) return;
      int p1, p2, hr = 0;
      DsEvCode code;
      do
      {
        hr = mediaEvt.GetEvent( out code, out p1, out p2, 0 );
        if( hr < 0 )
          break;
        hr = mediaEvt.FreeEventParams( code, p1, p2 );
        if( code == DsEvCode.Complete || code== DsEvCode.ErrorAbort)
        {
          MovieEnded(false);
          return;
        }
      }
      while( hr == 0 );
    }

    protected void DoFFRW()
    {

      if (!Playing) 
        return;
      
      if ((m_speedRate == 10000) || (mediaSeek == null))
        return;

			TimeSpan ts=DateTime.Now-elapsedTimer;
			if (ts.TotalMilliseconds<100) return;
      long earliest, latest, current,  stop, rewind, pStop;
		
      mediaSeek.GetAvailable(out earliest, out latest);
      mediaSeek.GetPositions(out current, out stop);

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
        hr = mediaSeek.SetPositions(ref rewind, (int)SeekingFlags.AbsolutePositioning	,ref pStop, SeekingFlags.NoPositioning);
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
        hr = mediaSeek.SetPositions(ref rewind, (int)SeekingFlags.AbsolutePositioning,ref pStop, SeekingFlags.NoPositioning);
        mediaCtrl.Run();
        return;
      }

      //seek to new moment in time
      //Log.Write(" seek :{0}",rewind/10000000);
      hr = mediaSeek.SetPositions(ref rewind, (int)SeekingFlags.AbsolutePositioning		,ref pStop, SeekingFlags.NoPositioning);
      mediaCtrl.Pause();
    }

		protected virtual void OnInitialized()
		{
		}

		#region subtitle/audio stream selection
		public override int AudioStreams
		{
			get { return cStreams_Audio;}
		}
		public override int CurrentAudioStream
		{
			get
			{
				for (int i=0;i<cStreams_Audio;i++)if (sStreams_Audio[i].Current)return i;
				return 0;
			}
			set
			{
				for (int i=0;i<cStreams_Audio;i++)if (sStreams_Audio[i].Current)sStreams_Audio[i].Current=false;
				sStreams_Audio[value].Current=true;
				EnableStream(sStreams_Audio[value].Id,0,sStreams_Audio[value].Filter);
				EnableStream(sStreams_Audio[value].Id,AMStreamSelectEnableFlags.Enable,sStreams_Audio[value].Filter);
				return;
			}
		}
		public override string AudioLanguage(int iStream)
		{
			return sStreams_Audio[iStream].Name;
		}

   
		public override int SubtitleStreams
		{
			get
			{
				if (this.vobSub != null)
				{
						int ret;
					vobSub.get_LanguageCount(out ret);
					return ret;
				}
				else
					return cStreams_Sub;
			}
		}

		public override int CurrentSubtitleStream
		{
			get
			{
				if (vobSub!=null){int ret=0;vobSub.get_SelectedLanguage(out ret);return ret;}
				for (int i=0;i<cStreams_Sub;i++)if (sStreams_Sub[i].Current)return i;
				return 0;
			}
			set
			{
				if (vobSub!=null){vobSub.put_SelectedLanguage(value);return;}
				for (int i=0;i<cStreams_Sub;i++)sStreams_Sub[i].Current=false;
				sStreams_Sub[value].Current=true;
				EnableStream(sStreams_Sub[value].Id,0,sStreams_Sub[value].Filter);
				EnableStream(sStreams_Sub[value].Id,AMStreamSelectEnableFlags.Enable,sStreams_Sub[value].Filter);
				return;
			}
		}

		public override string SubtitleLanguage(int iStream)
		{
			if (vobSub != null)
			{   
				string ret = Strings.Unknown;
				IntPtr curNamePtr;
				vobSub.get_LanguageName(iStream, out curNamePtr);
				if (curNamePtr != IntPtr.Zero)
				{
					ret = Marshal.PtrToStringUni(curNamePtr);
					Marshal.FreeCoTaskMem(curNamePtr);
				}
				return ret;
			}
			return sStreams_Sub[iStream].Name;
		}
		public override bool EnableSubtitle
		{
			get
			{

				bool ret = false;
				if (this.vobSub != null)
				{
					int hr = vobSub.get_HideSubtitles(out ret);
					if (hr == 0)
					{
						ret = !ret;
					}
				}
				else
				{
					for (int i=0;i<cStreams_Sub;i++)if (sStreams_Sub[i].Current)return true;
				}
				return ret;
			}
			set
			{
				if (this.vobSub != null)
				{
					bool hide = !value;
					int hr = vobSub.put_HideSubtitles(hide);
				}          
				else
				{
					int CurrentSub=0;
					for (int i=0;i<cStreams_Sub;i++)if (sStreams_Sub[i].Current)CurrentSub=i;
             
					if (CurrentSub>=0)
					{
						EnableStream(StopSubId,0,sStreams_Sub[CurrentSub].Filter);              EnableStream(sStreams_Sub[CurrentSub].Id,0,sStreams_Sub[CurrentSub].Filter);
						if (value)
							EnableStream(sStreams_Sub[CurrentSub].Id,AMStreamSelectEnableFlags.Enable,sStreams_Sub[CurrentSub].Filter);
						else
							EnableStream(StopSubId,AMStreamSelectEnableFlags.Enable,sStreams_Sub[CurrentSub].Filter);
					}
				}
			}
		} 

		bool AnalyseStreams()
		{
			cStreams_Audio=0;
			cStreams_Video=0;
			cStreams_Sub=0;
			sStreams_Audio=new FilterStreamInfos[MAX_AUDIOSTREAMS];
			sStreams_Video=new FilterStreamInfos[MAX_VIDEOSTREAMS];
			sStreams_Sub=new FilterStreamInfos[MAX_SUBSTREAMS];   

			IBaseFilter foundfilter=DirectShowUtil.GetFilterByName(graphBuilder,"Ogg Splitter");
			string filter="Ogg Splitter";
			if (foundfilter==null)
			{
				uint fetched=0;
				IEnumFilters enumFilters;
				graphBuilder.EnumFilters(out enumFilters);
				if (enumFilters!=null)
				{
					enumFilters.Reset();
					while (enumFilters.Next(1,out foundfilter,out fetched)==0)
					{
						if (foundfilter!=null && fetched==1)
						{
							IAMStreamSelect pStrm = foundfilter as IAMStreamSelect;
							if (pStrm!=null)
							{
								break;
							}
							Marshal.ReleaseComObject(foundfilter);
						}
					}
					Marshal.ReleaseComObject(enumFilters);
				}
			}
			try
			{
				if (foundfilter!=null)
				{
					int cStreams=0;
					IAMStreamSelect pStrm = foundfilter as IAMStreamSelect;
					if (pStrm!=null)
					{
						pStrm.Count(out cStreams);
						for (int istream=0;istream<cStreams;istream++)
						{
							AMMediaType sType;AMStreamSelectInfoFlags sFlag;
							int sPDWGroup,sPLCid;string sName;
							object pppunk,ppobject;
							pStrm.Info(istream,out sType,out sFlag,out sPLCid,
								out sPDWGroup,out sName,out pppunk,out ppobject);
							if (sPDWGroup==0 && cStreams_Video<MAX_VIDEOSTREAMS)
							{
								sStreams_Video[cStreams_Video].Name=sName;
								sStreams_Video[cStreams_Video].Id=istream;
								sStreams_Video[cStreams_Video].Filter=filter;
								sStreams_Video[cStreams_Video].Current=false;
								if (cStreams_Video==0)
								{
									sStreams_Video[cStreams_Video].Current=true;
									pStrm.Enable(istream,0);
									pStrm.Enable(istream,AMStreamSelectEnableFlags.Enable);
								}
								cStreams_Video++;
							}
							else
								if (sPDWGroup==1 && cStreams_Audio<MAX_AUDIOSTREAMS)
							{
								sStreams_Audio[cStreams_Audio].Name=sName;
								sStreams_Audio[cStreams_Audio].Id=istream;
								sStreams_Audio[cStreams_Audio].Filter=filter;
								sStreams_Audio[cStreams_Audio].Current=false;
								if (cStreams_Audio==0)
								{
									sStreams_Audio[cStreams_Audio].Current=true;
									pStrm.Enable(istream,0);
									pStrm.Enable(istream,AMStreamSelectEnableFlags.Enable);
								}
								cStreams_Audio++;
							}
							else
								if (sPDWGroup==2 && cStreams_Sub<MAX_SUBSTREAMS && sName.LastIndexOf("off")==-1  && sName.LastIndexOf("No ")==-1)
							{
								sStreams_Sub[cStreams_Sub].Name=sName;
								sStreams_Sub[cStreams_Sub].Id=istream;
								sStreams_Sub[cStreams_Sub].Filter=filter;
								sStreams_Sub[cStreams_Sub].Current=false;
								if (cStreams_Sub==0)
								{
									sStreams_Sub[cStreams_Sub].Current=true;
									pStrm.Enable(istream,0);
									pStrm.Enable(istream,AMStreamSelectEnableFlags.Enable);
								}
								cStreams_Sub++;
							}
							else if (sPDWGroup==2 && cStreams_Sub<MAX_SUBSTREAMS && (sName.LastIndexOf("off")!=-1 || sName.LastIndexOf("No ")!=-1))
								StopSubId=istream;
						}
						Marshal.ReleaseComObject(foundfilter);   
					}   
				}
			}
			catch(Exception)
			{
			}
			if (foundfilter!=null)
				Marshal.ReleaseComObject(foundfilter);
			return true;
		}
		bool EnableStream(int Id,AMStreamSelectEnableFlags dwFlags,string Filter)
		{

			IBaseFilter foundfilter=DirectShowUtil.GetFilterByName(graphBuilder,Filter);
			if (foundfilter!=null)
			{
				IAMStreamSelect pStrm = (IAMStreamSelect)foundfilter;
				pStrm.Enable(Id,dwFlags);
				pStrm=null;
				Marshal.ReleaseComObject(foundfilter);
			}
			return true;
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
