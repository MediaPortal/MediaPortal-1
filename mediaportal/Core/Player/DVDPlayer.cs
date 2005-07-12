using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;

using DShowNET;
using DShowNET.Dvd;

// OK: 720x576 Windowed Mode, 
//     Use PixelRatio correction = no
//     ar correction = stretched,
//     display mode  = default
//     s             = 'normal mode'
//
// 
//

namespace MediaPortal.Player
{
	/// <summary>
	/// 
	/// </summary>
  public class DVDPlayer : IPlayer 
  {
    const uint VFW_E_DVD_OPERATION_INHIBITED           = 0x80040276;
    const uint VFW_E_DVD_INVALIDDOMAIN                 = 0x80040277;
    const int UOP_FLAG_Play_Title_Or_AtTime           = 0x00000001;
    const int UOP_FLAG_Play_Chapter_Or_AtTime         = 0x00000020;
    protected enum PlayState
    {
      Init, Playing, Paused, Stopped
    }

    protected enum MenuMode
    {
      No, Buttons, Still
    }
	
    /// <summary> current state of playback (playing/paused/...) </summary>
    protected	PlayState				m_state;

    /// <summary> current mode of playback (movie/menu/still). </summary>
    protected	MenuMode				menuMode;

    /// <summary> asynchronous command interface. </summary>
    protected	OptIDvdCmd				cmdOption = new OptIDvdCmd();
    /// <summary> asynchronous command pending. </summary>
    protected	bool					pendingCmd;

    /// <summary> dvd graph builder interface. </summary>
    protected IDvdGraphBuilder		dvdGraph=null;
    /// <summary> dvd control interface. </summary>
    protected IDvdControl2			dvdCtrl=null;
    /// <summary> dvd information interface. </summary>
    protected IDvdInfo2				dvdInfo=null;
		protected IBaseFilter dvdbasefilter=null;
    /// <summary> dvd video playback window interface. </summary>
    protected IVideoWindow			videoWin=null;
    protected IBasicVideo2			basicVideo=null; 
    protected IGraphBuilder			graphBuilder=null;
		protected IAMLine21Decoder	line21Decoder=null;
    
    protected int                       m_iVideoPref=0;
    protected AmAspectRatioMode arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED;
    /// <summary> control interface. </summary>
    protected IMediaControl			    mediaCtrl=null;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx			    mediaEvt=null;

    /// <summary> interface to single-step video. </summary>
    protected IVideoFrameStep			  videoStep=null;
    protected int                           m_iVolume=100;
    //protected OVTOOLLib.OvMgrClass	m_ovMgr=null;

    protected DvdTimeCode				    currnTime;		// copy of current playback states, see OnDvdEvent()
    protected int						        currnTitle=0;
    protected int						        currnChapter=0;
    protected DvdDomain				      currnDomain;
    protected IBasicAudio				    basicAudio=null;
    protected IMediaPosition		    mediaPos=null;
		VMR7Util  vmr7 = null;
		protected int                   m_iSpeed=1;
    protected double                m_dCurrentTime=0;
    protected bool                          m_bVisible=true;
    protected bool                          m_bStarted=false;
    protected int		                rotCookie = 0;
    
    protected int 											    m_iPositionX=80;
    protected int 											    m_iPositionY=400;
    protected int 											    m_iWidth=200;
    protected int 											    m_iHeight=100;
    protected int                           m_iVideoWidth=100;
    protected int                           m_iVideoHeight=100;
    protected bool											    m_bUpdateNeeded=false;
    protected bool											    m_bFullScreen=true;
    protected string                        m_strAudioLanguage="";
    protected string                        m_strSubtitleLanguage="";
    protected bool                          m_bSubtitlesEnabled=true;
    protected bool                          m_bFreeNavigator=false;
    protected int                           m_iCurrentAudioStream=-1;
    protected bool                          m_bMenuOn=false;
    protected int                           m_iUOPs;
    protected string                        m_strCurrentFile;
		protected double												m_dDuration;
    protected MediaPortal.GUI.Library.Geometry.Type             m_ar=MediaPortal.GUI.Library.Geometry.Type.Normal;

    protected const int WM_DVD_EVENT		= 0x00008002;	// message from dvd graph
    protected const int WS_CHILD			= 0x40000000;	// attributes for video window
    protected const int WS_CLIPCHILDREN	= 0x02000000;
    protected const int WS_CLIPSIBLINGS	= 0x04000000;
    protected const int WM_MOUSEMOVE    =0x0200;
    protected const int WM_LBUTTONUP    =0x0202;

		ArrayList mouseMsg ;
    public DVDPlayer()
    {
    }
    public override void WndProc( ref Message m )
    {
			try
			{
				if( m.Msg == WM_DVD_EVENT )
				{
				
					if( mediaEvt != null )
						OnDvdEvent();
					return;
				}

				if( m.Msg==WM_MOUSEMOVE)
				{
					if (menuMode!=MenuMode.No)
						mouseMsg.Add(m);
				}

				if( m.Msg==WM_LBUTTONUP)
				{
					if (menuMode!=MenuMode.No)
						mouseMsg.Add(m);
				}
			}
			catch(Exception ex)
			{

				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:WndProc() {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);			
			}
    }

    public override bool Play(string strFile)
    {
      m_iUOPs=0;
      m_bMenuOn=false;
      m_bStarted=false;
      m_bVisible=false;
      m_iPositionX=80;
      m_iPositionY=400;
      m_iWidth=200;
      m_iHeight=100;
      m_iVideoWidth=100;
      m_iVideoHeight=100;
      m_iSpeed=1;
      m_strAudioLanguage="";
      m_strSubtitleLanguage="";
      m_bSubtitlesEnabled=true;
      m_ar=MediaPortal.GUI.Library.Geometry.Type.Normal;
      m_iSpeed=1;
      m_dCurrentTime=0;
      m_bVisible=true;
      m_bStarted=false;
      rotCookie = 0;
      m_iVolume=100;
			mouseMsg  = new ArrayList();


      bool bResult=FirstPlayDvd(strFile);
      if (!bResult) 
      {
        return false;
      }

      m_bUpdateNeeded=true;

      m_bFullScreen=true;
      m_bUpdateNeeded=true;

      GUIGraphicsContext.IsFullScreenVideo=true;
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
      SetVideoWindow();

      return true;
    }

    #region IDisposable Members

    public override void Release()
    {
      CloseInterfaces();
			
    }

    #endregion

    public void SelectAudioLanguage(string strLanguage)
    {
      int iStreamsAvailable=0;
      int iCurrentStream=0;
      Log.Write("DVDPlayer:SelectAudioLanguage:"+ strLanguage);
      int hr=dvdInfo.GetCurrentAudio( out iStreamsAvailable,out iCurrentStream);
      if (hr<0) 
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetCurrentAudio() failed");
        return;
      }
      Log.Write("DVDPlayer:found {0} audiostreams", iStreamsAvailable.ToString());
      if (iStreamsAvailable<=0) return;
      foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures ) )  
      {
        if (String.Compare(ci.EnglishName,strLanguage,true)==0) 
        {
          for (int i=0; i < iStreamsAvailable; ++i)
          {
            int iAudioLanguage;
            hr=dvdInfo.GetAudioLanguage(i,out iAudioLanguage);
            if (hr==0)
            {
              if (ci.LCID==(iAudioLanguage&0x3ff))
              {
                m_strAudioLanguage=strLanguage;
                hr=dvdCtrl.SelectAudioStream(i, DvdCmdFlags.None,null);
                if (hr==0)
                  Log.Write("DVDPlayer:Selected audio stream:{0}", strLanguage);
                else
                  Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:SelectAudioStream() failed");
                return;
              }
            }
            else Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetAudioLanguage() failed");
          }
        }
      }
    }

    public void SelectSubtitleLanguage(string strSubtitleLanguage)
    {
      int iStreamsAvailable=0;
      int iCurrentStream=0;
      bool bIsDisabled;
      Log.Write("DVDPlayer:SelectSubtitleLanguage:"+ strSubtitleLanguage);
      int hr=dvdInfo.GetCurrentSubpicture( out iStreamsAvailable,out iCurrentStream,out bIsDisabled);
      if (hr<0) 
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetCurrentSubpicture() failed");
        return;
      }
      Log.Write("DVDPlayer:found {0} subpicture streams", iStreamsAvailable.ToString());
      if (iStreamsAvailable<=0) return;
      foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures) )  
      {
        if (String.Compare(ci.EnglishName,strSubtitleLanguage,true)==0) 
        {
          for (int i=0; i < iStreamsAvailable; ++i)
          {
            int iSubtitleLanguage;
            hr=dvdInfo.GetSubpictureLanguage(i,out iSubtitleLanguage);
            if (hr==0)
            {
              if (ci.LCID==(iSubtitleLanguage&0x3ff))
              {
                m_strSubtitleLanguage=strSubtitleLanguage;
                hr=dvdCtrl.SelectSubpictureStream(i, DvdCmdFlags.None,null);
                if (hr==0)
                {
                  hr=dvdCtrl.SetSubpictureState(m_bSubtitlesEnabled,DvdCmdFlags.None,null);
                  if (hr==0)
                  {
                    Log.Write("DVDPlayer:switched subs to:" + strSubtitleLanguage);
                  }
                  else Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:SetSubpictureState() failed");
                }
                else Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:SelectSubpictureStream() failed");

                return;
              }
            }
            else Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetSubpictureLanguage() failed");
          }
        }
      }
    }

    /// <summary> handling the very first start of dvd playback. </summary>
    bool FirstPlayDvd(string strFile)
    {
      int hr;
  	
	
      try 
      {
        
        pendingCmd = true; 
        UpdateMenu();
        CloseInterfaces();
        string strPath=null;
        m_strCurrentFile=strFile;
        if (strFile!="")
        {
          int ipos=strFile.LastIndexOf(@"\");
          if (ipos>0)
          {
            strPath=strFile.Substring(0,ipos);
          }
        }
        if( ! GetInterfaces(strPath) )
        {
          Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable getinterfaces()");
          CloseInterfaces();
          return false;
        }

		  using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
		  {
			  m_strAudioLanguage   =xmlreader.GetValueAsString("dvdplayer","audiolanguage","english");
			  m_strSubtitleLanguage=xmlreader.GetValueAsString("dvdplayer","subtitlelanguage","english");
			  m_bSubtitlesEnabled  =xmlreader.GetValueAsBool("dvdplayer","showsubtitles",true);
		  }
      
		  SetDefaultLanguages();
        

			  hr = mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_DVD_EVENT, IntPtr.Zero );
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to SetNotifyWindow 0x{0:X}",hr);
				}

				if (videoWin!=null)
        {
					if( hr == 0 )
					{
						hr = videoWin.put_Owner( GUIGraphicsContext.ActiveForm);
						if (hr!=0)
						{
							Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to set window owner 0x{0:X}",hr);
						}
					}
					if( hr == 0 )
					{
							hr = videoWin.put_WindowStyle( WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN );
							if (hr!=0)
								Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to set window style 0x{0:X}",hr);
					}
        }
				
				if( hr != 0 )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to set options()");
					CloseInterfaces();
					return false;
				}
				if (basicVideo!=null)
				{
					basicVideo.SetDefaultSourcePosition();
					basicVideo.SetDefaultDestinationPosition();
				}
        if (videoWin!=null)
        {
          videoWin.SetWindowPosition( 0,0,GUIGraphicsContext.Width,GUIGraphicsContext.Height);
        }
    
        hr = mediaCtrl.Run();
        if (hr<0 || hr >1)
        {
          Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to start playing() 0x:{0:X}",hr);
          CloseInterfaces();
          return false;
        }
        //DsUtils.DumpFilters(graphBuilder);
        DvdDiscSide side;
        int iTitles,iNumOfVolumes,iVolume;
        hr=dvdInfo.GetDVDVolumeInfo(out iNumOfVolumes, out iVolume, out side, out iTitles);
				if (hr < 0) 
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to get dvdvolumeinfo 0x{0:X}",hr);
					//return false;
				}
				else
				{
					if (iTitles<=0) 
					{
						Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:DVD does not contain any titles? {0}",iTitles);
						//return false;
					}
				}
        
        if (videoWin!=null)
        {
            hr = videoWin.put_MessageDrain( GUIGraphicsContext.ActiveForm );
        }

        hr=dvdCtrl.SelectVideoModePreference(m_iVideoPref);
        DvdVideoAttr videoAttr;
        hr=dvdInfo.GetCurrentVideoAttributes(out videoAttr);
        m_iVideoWidth =videoAttr.sourceResolutionX;
        m_iVideoHeight=videoAttr.sourceResolutionY;

        m_state = PlayState.Playing;
        pendingCmd = false;
        Log.Write("DVDPlayer:Started playing()");
        if (m_strCurrentFile==String.Empty)
        {
          for (int i=0; i <= 26;++i)
          {
            string dvd=String.Format("{0}:", (char)('A'+i));
            if (Utils.IsDVD(dvd))
            {
              m_strCurrentFile=String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO",(char)('A'+i));
              break;
            }
          }
        }
        return true;
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Could not start DVD:{0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
        CloseInterfaces();
        return false;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected virtual void CloseInterfaces()
    {
			if (graphBuilder==null) return;
      int hr;
      try 
      {
        Log.Write("DVDPlayer:cleanup DShow graph");			

        if( mediaCtrl != null )
        {
          hr = mediaCtrl.Stop();
          mediaCtrl = null;
        }
        m_state = PlayState.Stopped;

				mediaEvt = null;
        m_bVisible=false;
				videoWin = null;
				videoStep	= null;
				dvdCtrl = null;
				dvdInfo = null;
				basicVideo=null;
				basicAudio=null;
				mediaPos=null;
				
				if (vmr7!=null)
					vmr7.RemoveVMR7();
				vmr7=null;

				if( dvdbasefilter != null )
				{
					while ((hr=Marshal.ReleaseComObject( dvdbasefilter))>0); 
					dvdbasefilter = null;              
				}
	
        if( cmdOption.dvdCmd != null )
          Marshal.ReleaseComObject( cmdOption.dvdCmd ); 
				cmdOption.dvdCmd = null;
        pendingCmd = false;
				if (line21Decoder!=null)
				{
					while ((hr=Marshal.ReleaseComObject( line21Decoder))>0); 
					line21Decoder=null;
				}

        if (rotCookie !=0) 
					DsROT.RemoveGraphFromRot( ref rotCookie );		// graphBuilder capGraph
				rotCookie=0;

        if (graphBuilder!=null)
        {
          DsUtils.RemoveFilters(graphBuilder);
          while ((hr=Marshal.ReleaseComObject( graphBuilder ))>0); 
					graphBuilder = null;
        }

				if( dvdGraph != null )
				{
					while ((hr=Marshal.ReleaseComObject( dvdGraph ))>0); 
					dvdGraph = null;
				}
				m_state = PlayState.Init;
				GUIGraphicsContext.form.Invalidate(true);          
        GUIGraphicsContext.form.Activate();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected virtual bool GetInterfaces(string strPath)
    {
      int		            hr;
      Type	            comtype = null;
      object	          comobj = null;
      m_bFreeNavigator=true;
      dvdInfo=null;
      dvdCtrl=null;

      string strDVDNavigator="";
      string strARMode="";
      string strDisplayMode="";
      bool  bUseAC3Filter=false;
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        strDVDNavigator=xmlreader.GetValueAsString("dvdplayer","navigator","");
        strARMode=xmlreader.GetValueAsString("dvdplayer","armode","").ToLower();
        if ( strARMode=="crop") arMode=AmAspectRatioMode.AM_ARMODE_CROP;
        if ( strARMode=="letterbox") arMode=AmAspectRatioMode.AM_ARMODE_LETTER_BOX;
        if ( strARMode=="stretch") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED;
        if ( strARMode=="follow stream") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED_AS_PRIMARY;
        bUseAC3Filter= xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
        strDisplayMode=xmlreader.GetValueAsString("dvdplayer","displaymode","").ToLower();
        if (strDisplayMode=="default") m_iVideoPref=0;
        if (strDisplayMode=="16:9") m_iVideoPref=1;
        if (strDisplayMode=="4:3 pan scan") m_iVideoPref=2;
        if (strDisplayMode=="4:3 letterbox") m_iVideoPref=3;
      }
      try 
      {
        
        comtype = Type.GetTypeFromCLSID( Clsid.DvdGraphBuilder );
        if( comtype == null )
          throw new NotSupportedException( "DirectX (8.1 or higher) not installed?" );
        comobj = Activator.CreateInstance( comtype );
        dvdGraph = (IDvdGraphBuilder) comobj; comobj = null;

        hr = dvdGraph.GetFiltergraph( out graphBuilder );
        if( hr < 0 )
          Marshal.ThrowExceptionForHR( hr );
        DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph
				vmr7=new VMR7Util();
				vmr7.AddVMR7(graphBuilder);

				
        try
        {

          dvdbasefilter=DirectShowUtil.AddFilterToGraph(graphBuilder,strDVDNavigator);
          if (dvdbasefilter!=null)
          {
            IDvdControl2 cntl=(IDvdControl2)dvdbasefilter;
            if (cntl!=null)
            {
              dvdInfo = (IDvdInfo2) cntl;
              dvdCtrl = (IDvdControl2)cntl;
							if (strPath!=null) 
							{
								if (strPath.Length!=0)
									cntl.SetDVDDirectory(strPath);
							}
							string path;
							int size;
							IntPtr ptrFolder = Marshal.AllocCoTaskMem(256);
							dvdInfo.GetDVDDirectory( ptrFolder,256,out size);
							path=Marshal.PtrToStringAuto(ptrFolder);
							Marshal.FreeCoTaskMem(ptrFolder);
							dvdCtrl.SetOption( DvdOptionFlag.HmsfTimeCodeEvt, true );	// use new HMSF timecode format
							dvdCtrl.SetOption( DvdOptionFlag.ResetOnStop, false );
/*
							if (path!=null && path.Length>0)
							{
								DirectShowHelperLib.DVDClass dvdHelper = new DirectShowHelperLib.DVDClass();
								dvdHelper.Reset(path);
							}
*/
							AddPreferedCodecs(graphBuilder);
							DirectShowUtil.RenderOutputPins(graphBuilder,dvdbasefilter);

              videoWin	= graphBuilder as IVideoWindow;
              m_bFreeNavigator=false;
            }

            //Marshal.ReleaseComObject( dvdbasefilter); dvdbasefilter = null;              
          }
        }
        catch(Exception ex)
        {
					string strEx=ex.Message;
        }
				Guid riid ;

        
        if (bUseAC3Filter)
        {
          string ac3filterMonikerString =@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{A753A1EC-973E-4718-AF8E-A3F554D45C44}";
          Log.Write("DVDPlayer:Adding AC3 filter to graph");
          IBaseFilter filter = Marshal.BindToMoniker( ac3filterMonikerString ) as IBaseFilter;
          if (filter!=null)
          {
            hr = graphBuilder.AddFilter( filter, "AC3 Filter" );
            if( hr < 0 ) 
            {
              DirectShowUtil.DebugWrite("DVDPlayer:FAILED:could not add AC3 filter to graph");
            }
          }
          else
          {
            DirectShowUtil.DebugWrite("DVDPlayer:FAILED:AC3 filter not installed");
          }

        }
			
        if (dvdInfo==null)
        {
          riid = typeof( IDvdInfo2 ).GUID;
          hr = dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          dvdInfo = (IDvdInfo2) comobj; comobj = null;
        }

        if (dvdCtrl==null)
        {
          riid = typeof( IDvdControl2 ).GUID;
          hr = dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          dvdCtrl = (IDvdControl2) comobj; comobj = null;
        }


        mediaCtrl	= (IMediaControl)  graphBuilder;
        mediaEvt	= (IMediaEventEx)  graphBuilder;
        basicAudio	= graphBuilder as IBasicAudio;
        mediaPos	= (IMediaPosition) graphBuilder;
        basicVideo	= graphBuilder as IBasicVideo2;
        videoWin	= graphBuilder as IVideoWindow;



				// disable Closed Captions!
				IBaseFilter basefilter;
				graphBuilder.FindFilterByName("Line 21 Decoder", out basefilter);
				if (basefilter==null)
					graphBuilder.FindFilterByName("Line21 Decoder", out basefilter);
				if (basefilter!=null)
				{
					line21Decoder=(IAMLine21Decoder)basefilter;
					if (line21Decoder!=null)
					{
						AMLine21CCState state=AMLine21CCState.Off;
						hr=line21Decoder.SetServiceState(ref state);
						if (hr==0)
						{
							Log.Write("DVDPlayer:Closed Captions disabled");
						}
						else
						{
							Log.Write("DVDPlayer:failed 2 disable Closed Captions");
						}
					}
				}
/*
				// get video window
        if (videoWin==null)
        {
          riid = typeof( IVideoWindow ).GUID;
          hr = dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          videoWin = (IVideoWindow) comobj; comobj = null;
        }
  */      
        GetFrameStepInterface();

        DirectShowUtil.SetARMode(graphBuilder,arMode);
        DirectShowUtil.EnableDeInterlace(graphBuilder);
        //m_ovMgr = new OVTOOLLib.OvMgrClass();
        //m_ovMgr.SetGraph(graphBuilder);

        return true;
      }
      catch( Exception )
      {
        //MessageBox.Show( this, "Could not get interfaces\r\n" + ee.Message, "DVDPlayer.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop );
        CloseInterfaces();
        return false;
      }
      finally
      {
        if( comobj != null )
          Marshal.ReleaseComObject( comobj ); comobj = null;
      }
    }


    /// <summary> detect if we can single step. </summary>
    bool GetFrameStepInterface()
    {
      videoStep = graphBuilder as IVideoFrameStep;
      if( videoStep == null )
        return false;

      // Check if this decoder can step
      int hr = videoStep.CanStep( 0, null );
      if( hr != 0 )
      {
        videoStep = null;
        return false;
      }
      return true;
    }



    /// <summary> DVD event message handler</summary>
    void OnDvdEvent()
    {
//			Log.Write("OnDvdEvent()");
      int p1, p2, hr = 0;
      DsEvCode code;
			try
			{
				do
				{
					hr = mediaEvt.GetEvent( out code, out p1, out p2, 0 );
					if( hr < 0 )
						break;

//					Log.Write( "DVDPlayer DVD EVT :" + code.ToString() );
				
					switch( code )
					{
						case DsEvCode.DvdWarning:
							Log.Write( "DVDPlayer DVD warning :{0}" ,p1,p2 );
						break;
						case DsEvCode.DvdCurrentHmsfTime:
						{

							byte[] ati = BitConverter.GetBytes( p1 );
							if (ati!=null)
							{
								currnTime.bHours	= ati[0];
								currnTime.bMinutes	= ati[1];
								currnTime.bSeconds	= ati[2];
								currnTime.bFrames	= ati[3];
								m_dCurrentTime=( (double)currnTime.bHours)* 3600d;
								m_dCurrentTime +=( ( (double)currnTime.bMinutes)* 60d );
								m_dCurrentTime +=( ( (double)currnTime.bSeconds) );
							}

							break;
						}
					
						case DsEvCode.DvdSubPicStChange:
						{
							Log.Write("EVT:DvdSubPicture Changed to:{0} Enabled:{1}",p1,p2);
							//m_strSubtitleLanguage = p1.ToString();
							//m_bSubtitlesEnabled=(p2!=0);											
						}
							break;

						case DsEvCode.DvdChaptStart:
						{
							Log.Write("EVT:DvdChaptStart:{0}",p1);
							currnChapter = p1;

							SelectSubtitleLanguage(m_strSubtitleLanguage);
							DvdTimeCode totaltime;
							int         ulTimeCodeFlags; 
							dvdInfo.GetTotalTitleTime( out totaltime, out ulTimeCodeFlags );
          
							m_dDuration=( (double)totaltime.bHours)* 3600d;
							m_dDuration +=( ( (double)totaltime.bMinutes)* 60d );
							m_dDuration +=( ( (double)totaltime.bSeconds) );

							break;
						}
						case DsEvCode.DvdTitleChange:
						{
							Log.Write("EVT:DvdTitleChange:{0}",p1);
							currnTitle = p1;
							SelectSubtitleLanguage(m_strSubtitleLanguage);

							DvdTimeCode totaltime;
							int         ulTimeCodeFlags; 
							dvdInfo.GetTotalTitleTime( out totaltime, out ulTimeCodeFlags );
          
							m_dDuration=( (double)totaltime.bHours)* 3600d;
							m_dDuration +=( ( (double)totaltime.bMinutes)* 60d );
							m_dDuration +=( ( (double)totaltime.bSeconds) );

							break;
						}
	          
				
						case DsEvCode.DvdCmdStart:
						{
							if( pendingCmd )
								Log.Write( "  DvdCmdStart with pending" );
							break;
						}
						case DsEvCode.DvdCmdEnd:
						{
							OnCmdComplete( p1, p2 );
							break;
						}

						case DsEvCode.DvdStillOn:
						{
							Log.Write("EVT:DvdStillOn:{0}",p1);
							if( p1 == 0 )
								menuMode = MenuMode.Buttons;
							else
								menuMode = MenuMode.Still;
							break;
						}
						case DsEvCode.DvdStillOff:
						{
							Log.Write("EVT:DvdStillOff:{0}",p1);
							if( menuMode == MenuMode.Still )
								menuMode = MenuMode.No;
							break;
						}
						case DsEvCode.DvdButtonChange:
						{
							Log.Write("EVT:DvdButtonChange: buttons:#{0}",p1);
							if( p1 <= 0 )
								menuMode = MenuMode.No;
							else
								menuMode = MenuMode.Buttons;
							break;
						}

						case DsEvCode.DvdNoFpPgc:
						{
							Log.Write("EVT:DvdNoFpPgc:{0}",p1);
							if( dvdCtrl != null )
								hr = dvdCtrl.PlayTitle( 1, DvdCmdFlags.None, null );
							break;
						}

						case DsEvCode.DvdAudioStChange:
							// audio stream changed
							Log.Write("EVT:DvdAudioStChange:{0}",p1);
							break;

						case DsEvCode.DvdValidUopsChange:
							Log.Write("EVT:DvdValidUopsChange:0x{0:X}",p1);
							m_iUOPs=p1;
							break;

						case DsEvCode.DvdDomChange:
						{
							currnDomain = (DvdDomain) p1;
							switch ((DvdDomain)p1)
							{
								case DvdDomain.FirstPlay:
									Log.Write("EVT:DVDPlayer:domain=firstplay");
									break;
								case DvdDomain.Stop:
									Log.Write("EVT:DVDPlayer:domain=stop");
									break;
								case DvdDomain.VideoManagerMenu:
									Log.Write("EVT:DVDPlayer:domain=videomanagermenu (menu)");
									m_bMenuOn=true;
									break;
								case DvdDomain.VideoTitleSetMenu:
									Log.Write("EVT:DVDPlayer:domain=videotitlesetmenu (menu)");
									m_bMenuOn=true;
									break;
								case DvdDomain.Title:
									Log.Write("EVT:DVDPlayer:domain=title (no menu)");
									m_bMenuOn=false;
									break;  
								default:
									Log.Write("EVT:DvdDomChange:{0}",p1);
									break;
							}
							break;
						}
					}

					hr = mediaEvt.FreeEventParams( code, p1, p2 );
				}
				while( hr == 0 );
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:OnDvdEvent() {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);
			}
//      Log.Write("DVDEvent done");
    }



    /// <summary> asynchronous command completed </summary>
    void OnCmdComplete( int p1, int hrg )
    {
			try
			{
//				Log.Write( "DVD OnCmdComplete.........." );
				if( (pendingCmd == false) || (dvdInfo == null) )
					return;

				IDvdCmd		cmd;
				int hr = dvdInfo.GetCmdFromEvent( p1, out cmd );
				if( (hr != 0) || (cmd == null) )
				{
					Log.WriteFile(Log.LogType.Log,true, "!!!DVD OnCmdComplete GetCmdFromEvent failed!!!" );
					return;
				}

				if( cmd != cmdOption.dvdCmd )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:DVD OnCmdComplete UNKNOWN CMD!!!" );
					Marshal.ReleaseComObject( cmd ); cmd = null;
					return;
				}

				Marshal.ReleaseComObject( cmd ); cmd = null;
				Marshal.ReleaseComObject( cmdOption.dvdCmd ); cmdOption.dvdCmd = null;
				pendingCmd = false;
//				Log.Write( "DVD OnCmdComplete OK." );
				UpdateMenu();
				
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:OnCmdComplete() {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);
			}
    }

    /// <summary> update menu items to match current playback state </summary>
    protected void UpdateMenu()
    {
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
          return m_dCurrentTime;
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
			int hr=0;
      if( (mediaCtrl == null) ||
        ((m_state != PlayState.Playing) && (m_state != PlayState.Paused) ) )
        return;
			Log.Write("DVDPlayer:Stop()");
			if (mediaEvt!=null)
				hr = mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_DVD_EVENT, IntPtr.Zero );
			
			hr = mediaCtrl.Stop();
			if( hr >= 0 )
      {
        m_state = PlayState.Stopped;
        UpdateMenu();
      }
			CloseInterfaces();
			GUIGraphicsContext.IsFullScreenVideo=false;
      GUIGraphicsContext.IsPlaying=false;
    }
    public override int Speed
    {
      get 
      { 
        return m_iSpeed;
      }
      set 
      {
        if (m_state!=PlayState.Init)
        {
          try
          {
            m_iSpeed=value;
            if (m_iSpeed>=1)
            {
              dvdCtrl.PlayForwards((double)m_iSpeed,DvdCmdFlags.None,null);
            }
            else if (m_iSpeed<0)
            {
              dvdCtrl.PlayBackwards((double)-m_iSpeed,DvdCmdFlags.None,null);
            }
          }
          catch(Exception )
          {
            m_iSpeed=1;
          }
        }
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
          
          double dCurTime=CurrentPosition;
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
      if (m_state!=PlayState.Init)
      {
        if (mediaCtrl!=null && mediaPos!=null)
        {
          if (dTime<0.0d) dTime=0.0d;
          if (dTime < Duration)
          {
            int iHours=(int)(dTime/3600d);
            dTime -= (iHours*3600);
            int iMins = (int)(dTime / 60d);
            dTime -= (iMins*60);
            int iSecs = (int)dTime;
            DvdTimeCode timeCode;
            timeCode.bHours=(byte)(iHours&0xff);
            timeCode.bMinutes=(byte)(iMins&0xff);
            timeCode.bSeconds=(byte)(iSecs&0xff);
            timeCode.bFrames=0;
            DvdPlayLocation loc;
            currnTitle=dvdInfo.GetCurrentLocation(out loc);
            int hr=dvdCtrl.PlayAtTime(ref timeCode,DvdCmdFlags.None,null);
            if (hr!=0)
            {
              if ( ((uint)hr)==VFW_E_DVD_OPERATION_INHIBITED) Log.Write("DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) not allowed at this point",iHours,iMins,iSecs);
              else if ( ((uint)hr)==VFW_E_DVD_INVALIDDOMAIN) Log.Write("DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) invalid domain",iHours,iMins,iSecs);
              else Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) failed:0x{3:X}",iHours,iMins,iSecs,hr); 
            }
            //SetDefaultLanguages();
            //dvdCtrl.Pause(false);
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
          double dCurrentPos=CurrentPosition;
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
        if (mediaCtrl!=null && mediaPos!=null)
        {
          if (iPercentage<0) iPercentage=0;
          if (iPercentage>=100) iPercentage=100;
          double fPercent=Duration/100.0f;
          fPercent*=(double)iPercentage;
          SeekAbsolute(fPercent);
        }
      }
    }

    
		public override bool GetResumeState(out byte[] resumeData)
		{
			Log.Write("DVDPlayer::GetResumeState() begin");
			resumeData = null;
			IDvdState dvdState;
			int hr = dvdInfo.GetState(out dvdState);
			if (hr != 0)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetResumeState() dvdInfo.GetState failed");
				return false;
			}

			IPersistMemory dvdStatePersistMemory = (IPersistMemory)dvdState;
			if (dvdStatePersistMemory == null)
			{
				Log.Write("DVDPlayer::GetResumeState() could not get IPersistMemory");
				Marshal.ReleaseComObject(dvdState);
				return false;
			}
			uint resumeSize = 0;
			dvdStatePersistMemory.GetSizeMax(out resumeSize);
			if (resumeSize <= 0)
			{
				Log.Write("DVDPlayer::GetResumeState() failed resumeSize={0}", resumeSize);
				Marshal.ReleaseComObject(dvdStatePersistMemory);
				Marshal.ReleaseComObject(dvdState);
				return false;
			}
			IntPtr stateData = Marshal.AllocCoTaskMem((int)resumeSize); 

			try
			{
				dvdStatePersistMemory.Save(stateData, true, resumeSize);
				resumeData = new byte[resumeSize];
				Marshal.Copy(stateData, resumeData, 0, (int)resumeSize);
			}
			catch (Exception ex)
			{
				Log.Write("DVDPlayer::GetResumeState() failed {0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
			}
			
			Marshal.FreeCoTaskMem(stateData);
			Marshal.ReleaseComObject(dvdStatePersistMemory);
			Marshal.ReleaseComObject(dvdState);
			return true;
		}

    
		public override bool SetResumeState(byte[] resumeData)
		{
			if (resumeData.Length > 0)
			{
				Log.Write("DVDPlayer::SetResumeState() begin");
				IDvdState dvdState;

				int hr = dvdInfo.GetState(out dvdState);
				if (hr < 0)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetResumeState() dvdInfo.GetState failed");
					return false;
				}
				IPersistMemory dvdStatePersistMemory = (IPersistMemory)dvdState;
				IntPtr stateData = Marshal.AllocHGlobal(resumeData.Length); 
				Marshal.Copy(resumeData, 0, stateData, resumeData.Length);

				try
				{
					dvdStatePersistMemory.Load(stateData, (uint)resumeData.Length);
				}
				catch (Exception e)
				{
					throw e;
				}
				finally
				{
					Marshal.FreeHGlobal(stateData);
				}

				Log.Write("DVDPlayer::SetResumeState() SetState");
				OptIDvdCmd dvdCmd = null;
				hr = dvdCtrl.SetState(dvdState, DShowNET.Dvd.DvdCmdFlags.Block, dvdCmd);
				if (hr == 0)
				{
					Log.Write("DVDPlayer::SetResumeState() end true");
					return true;
				}

				Marshal.ReleaseComObject(dvdState);
			}

			Log.Write("DVDPlayer::SetResumeState() end false");
			return false;
		}
    public override bool HasVideo
    {
      get { return true;}
    }

    public override void Process()
    {
      if ( !Playing) return;
      if (!m_bStarted) return;
      OnProcess();
			HandleMouseMessages();
      
    }
		
		void HandleMouseMessages()
		{
			try
			{				
				if (GUIGraphicsContext.IsFullScreenVideo && !GUIGraphicsContext.Vmr9Active)
				{
					DsPOINT  pt;
					foreach(Message m in mouseMsg)
					{
						long lParam=m.LParam.ToInt32();
						if( m.Msg==WM_MOUSEMOVE)
						{
							pt=new DsPOINT();
							pt.X = (int)(lParam  & 0xffff); 
							pt.Y = (int)((lParam>>16)  & 0xffff); 

							// Select the button at the current position, if it exists
							dvdCtrl.SelectAtPosition(pt);
						}

						if( m.Msg==WM_LBUTTONUP)
						{
							pt=new DsPOINT();
							pt.X = (int)(lParam  & 0xffff); 
							pt.Y = (int)((lParam>>16)  & 0xffff); 

							// Highlight the button at the current position, if it exists
							dvdCtrl.ActivateAtPosition(pt);
						}
					}
				}
			}
			catch(Exception ex)
			{

				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:HandleMouseMessages() {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);			
			}
			mouseMsg.Clear();
		}

    protected virtual void OnProcess()
    {
      if (videoWin!=null)
      {
        if (GUIGraphicsContext.Overlay==false && GUIGraphicsContext.IsFullScreenVideo==false)
        {
          if (m_bVisible)
          {
            m_bVisible=false;
            videoWin.put_Visible( DsHlp.OAFALSE );
          }
        }
        else if (!m_bVisible)
        {
          m_bVisible=true;
          videoWin.put_Visible( DsHlp.OATRUE );
        }
      }
    }

    public override void SetVideoWindow()
    {
      if (videoWin==null) return;
      if (GUIGraphicsContext.IsFullScreenVideo!= m_bFullScreen)
      {
        m_bFullScreen=GUIGraphicsContext.IsFullScreenVideo;
        m_bUpdateNeeded=true;
      }

      if (!m_bUpdateNeeded) return;
      
      m_bStarted=true;
      m_bUpdateNeeded=false;
      float x=m_iPositionX;
      float y=m_iPositionY;
      
      int nw=m_iWidth;
      int nh=m_iHeight;
      if (nw > GUIGraphicsContext.OverScanWidth)
        nw=GUIGraphicsContext.OverScanWidth;
      if (nh > GUIGraphicsContext.OverScanHeight)
        nh=GUIGraphicsContext.OverScanHeight;
      lock ( typeof(DVDPlayer) )
      {
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          x=m_iPositionX=GUIGraphicsContext.OverScanLeft;
          y=m_iPositionY=GUIGraphicsContext.OverScanTop;
          nw=m_iWidth=GUIGraphicsContext.OverScanWidth;
          nh=m_iHeight=GUIGraphicsContext.OverScanHeight;
        }
        if (nw <=0 || nh <=0) return;

        System.Drawing.Rectangle rSource,rDest;
				
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

        MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
        m_geometry.ImageWidth  =m_iVideoWidth;
        m_geometry.ImageHeight =m_iVideoHeight;
        m_geometry.ScreenWidth =nw;
        m_geometry.ScreenHeight=nh;
        m_geometry.ARType=GUIGraphicsContext.ARType;
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          bool bUseAR=xmlreader.GetValueAsBool("dvdplayer","pixelratiocorrection",false);
          if (bUseAR) m_geometry.PixelRatio=GUIGraphicsContext.PixelRatio;
          else m_geometry.PixelRatio=1.0f;
        }
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

				

        //hr=videoWin.SetWindowPosition( rDest.X, rDest.Y, rDest.Width, rDest.Height );
        //hr=dvdCtrl.SelectVideoModePreference(m_iVideoPref);        
        DirectShowUtil.SetARMode(graphBuilder,arMode);

        m_SourceRect=rSource;
        m_VideoRect=rDest;
        
      }
    }

    void MovieEnded()
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      if (null!=videoWin) 
      {
        Log.Write("DVDPlayer: ended");
        m_state=PlayState.Init;
      }
      GUIGraphicsContext.IsFullScreenVideo=false;
      GUIGraphicsContext.IsPlaying=false;
    }

    
    public override  bool IsDVD
    {
      get 
      {
        return true;
      }
    }
    public override bool OnAction(Action action)
    {
			try
			{
				switch (action.wID)
				{
					case Action.ActionType.ACTION_MOVE_LEFT:
						if( m_bMenuOn )
						{
							Log.Write("DVDPlayer: move left");
							dvdCtrl.SelectRelativeButton( DvdRelButton.Left );
							return true;
						}
						break;
					case Action.ActionType.ACTION_MOVE_RIGHT:
						if( m_bMenuOn )
						{
							Log.Write("DVDPlayer: move right");
							dvdCtrl.SelectRelativeButton( DvdRelButton.Right );
							return true;
						}
						break;
					case Action.ActionType.ACTION_MOVE_UP:
						if( m_bMenuOn )
						{
							
							Log.Write("DVDPlayer: move up");
							dvdCtrl.SelectRelativeButton( DvdRelButton.Upper );
							return true;
						}
						break;
					case Action.ActionType.ACTION_MOVE_DOWN:
						if( m_bMenuOn )
						{	
							Log.Write("DVDPlayer: move down");
							dvdCtrl.SelectRelativeButton( DvdRelButton.Lower );
							return true;
						}
						break;
					case Action.ActionType.ACTION_SELECT_ITEM:
						if( (menuMode == MenuMode.Buttons) && (dvdCtrl != null) )
						{	
							Log.Write("DVDPlayer: select");
							dvdCtrl.ActivateButton();
							return true;
						}
						else if( (menuMode == MenuMode.Still) && (dvdCtrl != null) )
						{
							Log.Write("DVDPlayer: still off");
							dvdCtrl.StillOff();
							return true;
						}
						break;

					case Action.ActionType.ACTION_DVD_MENU:

						if( (m_state != PlayState.Playing) || (dvdCtrl == null) )
							return false;
						Speed=1;
						dvdCtrl.ShowMenu( DvdMenuID.Root, DvdCmdFlags.Block | DvdCmdFlags.Flush, null );
						return true;

					case Action.ActionType.ACTION_NEXT_CHAPTER:
					{
						if( (m_state != PlayState.Playing) || (dvdCtrl == null) )
							return false;

						Speed=1;
						int hr = dvdCtrl.PlayNextChapter( DvdCmdFlags.SendEvt, cmdOption );
						if( hr < 0 )
						{
							Log.WriteFile(Log.LogType.Log,true,"!!! PlayNextChapter error : 0x" + hr.ToString("x") );
							return false;
						}

						if( cmdOption.dvdCmd != null )
						{
							Trace.WriteLine( "PlayNextChapter cmd pending.........." );
							pendingCmd = true;
						}

						UpdateMenu();
						return true;
					}

					case Action.ActionType.ACTION_PREV_CHAPTER:
					{
						if( (m_state != PlayState.Playing) || (dvdCtrl == null) )
							return false;

						Speed=1;
						int hr = dvdCtrl.PlayPrevChapter( DvdCmdFlags.SendEvt, cmdOption );
						if( hr < 0 )
						{
							Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:!!! PlayPrevChapter error : 0x" + hr.ToString("x") );
							return false;
						}

						if( cmdOption.dvdCmd != null )
						{
							Trace.WriteLine( "PlayPrevChapter cmd pending.........." );
							pendingCmd = true;
						}

						UpdateMenu();
						return true;
					}
				}				
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:OnAction() {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);
			}
      return false;
    }

	  void SetDefaultLanguages()
	  {
		  Log.Write("SetDefaultLanguages");
		  // Flip: Added more detailed message
		  int iSetError=0;
		  string sError = "";
		  int lCID=GetLCID(m_strAudioLanguage);
		  if (lCID>=0)
		  {
			  iSetError=0;
			  sError = "";
			  // Flip: Added more detailed message
			  iSetError=dvdCtrl.SelectDefaultAudioLanguage(lCID, DvdAudioLangExt.NotSpecified);
			  switch (iSetError)
			  {
				  case 0:
					  sError = "Success.";
					  break;
				  case 631:
					  sError = "The DVD Navigator filter is not in the Stop domain.";
					  break;
				  default:
					  sError = "Unknown Error. "+iSetError;
					  break;
			  }


			  Log.Write("DVDPlayer:Set default language:{0} {1} {2}", m_strAudioLanguage,lCID,sError);
		  }
		  lCID=GetLCID(m_strSubtitleLanguage);
		  if (lCID>=0)
		  {
			  iSetError=0;
			  sError = "";
			  iSetError=dvdCtrl.SelectDefaultMenuLanguage(lCID);
			  // Flip: Added more detailed message
			  switch (iSetError)
			  {
				  case 0:
					  sError = "Success.";
					  break;
				  case 631:
					  sError = "The DVD Navigator filter is not in a valid domain.";
					  break;
				  default:
					  sError = "Unknown Error. "+iSetError;
					  break;
			  }
			  Log.Write("DVDPlayer:Set default menu language:{0} {1} {2}", m_strSubtitleLanguage,lCID,sError);
		  }
		  lCID=GetLCID(m_strSubtitleLanguage);
		  if (lCID>=0)
		  {
			  iSetError=0;
			  sError = "";
			  iSetError=dvdCtrl.SelectDefaultSubpictureLanguage(lCID, DvdSubPicLangExt.NotSpecified);
			  // Flip: Added more detailed message
			  switch (iSetError)
			  {
				  case 0:
					  sError = "Success.";
					  break;
				  case 631:
					  sError = "The DVD Navigator filter is not in a valid domain.";
					  break;
				  default:
					  sError = "Unknown Error. "+iSetError;
					  break;
			  }
			  Log.Write("DVDPlayer:Set subtitle language:{0} {1} {2}", m_strSubtitleLanguage,lCID,sError);
		  }

		  dvdCtrl.SetSubpictureState(m_bSubtitlesEnabled,DvdCmdFlags.None,null);
      
	  }

	  int GetLCID(string strLanguage)
	  {
		  if (strLanguage==null) return -1;
		  if (strLanguage.Length==0) return -1;
		  // Flip: Added to cut off the detailed name info
		  string	strCutName;
		  int		iStart = 0;
		  // Flip: Changed from CultureTypes.NeutralCultures to CultureTypes.SpecificCultures
		  // Flip: CultureTypes.NeutralCultures did not work, provided the wrong CLID
		  foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.SpecificCultures ) )  
		  {
			  // Flip: cut off detailed info, e.g. "English (United States)" -> "English"
			  // Flip: to get correct compare
			  iStart = ci.EnglishName.IndexOf(" (");
			  if (iStart>0)
				  strCutName = ci.EnglishName.Substring(0,iStart);
			  else
				  strCutName = ci.EnglishName;

			  if (String.Compare(strCutName,strLanguage,true)==0) 
			  {
				  return ci.LCID;
			  }
		  }
		  return -1;
	  }

    public override int AudioStreams
    {
      get 
      { 
        
        int iStreamsAvailable,iCurrentStream;
        int hr=dvdInfo.GetCurrentAudio( out iStreamsAvailable,out iCurrentStream);
        if (hr==0) return iStreamsAvailable;
        return 1;
      }
    }
    public override int CurrentAudioStream
    {
      get 
      { 
        int iStreamsAvailable,iCurrentStream;
        int hr=dvdInfo.GetCurrentAudio( out iStreamsAvailable,out iCurrentStream);
        if (hr==0) return iCurrentStream;
        return 0;
      }
      set 
      {
        dvdCtrl.SelectAudioStream(value, DvdCmdFlags.None,null);
        m_strAudioLanguage=AudioLanguage(value);
        m_iCurrentAudioStream=value;
      }
    }

    public override string AudioLanguage(int iStream)
    {
      int iAudioLanguage;
      int hr=dvdInfo.GetAudioLanguage(iStream,out iAudioLanguage);
      if (hr==0)
      {
        foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures ) )  
        {
          if (ci.LCID==(iAudioLanguage&0x3ff))
          {
            return ci.EnglishName;
          }
        }
      }
      return Strings.Unknown;
    }

    public override int SubtitleStreams
    {
      get { 
        int iStreamsAvailable=0;
        int iCurrentStream=0;
        bool bIsDisabled;
        int hr=dvdInfo.GetCurrentSubpicture( out iStreamsAvailable,out iCurrentStream,out bIsDisabled);
        if (hr==0) return iStreamsAvailable;
        return 1;
      }
    }
    public override int CurrentSubtitleStream
    {
      get { 
        int iStreamsAvailable=0;
        int iCurrentStream=0;
        bool bIsDisabled;
        int hr=dvdInfo.GetCurrentSubpicture( out iStreamsAvailable,out iCurrentStream,out bIsDisabled);
        if (hr==0) return iCurrentStream;
        return 0;
      }
      set {
        int hr=dvdCtrl.SelectSubpictureStream(value, DvdCmdFlags.None,null);
        m_strSubtitleLanguage=SubtitleLanguage(value);
      }
    }
    public override string SubtitleLanguage(int iStream)
    {
      int iLanguage;
      int hr=dvdInfo.GetSubpictureLanguage(iStream,out iLanguage);
      if (hr==0)
      {
        foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures ) )  
        {
          if (ci.LCID==(iLanguage&0x3ff) )
          {
            return ci.EnglishName;
          }
        }
      }
      return Strings.Unknown;
    }

		public void AddPreferedCodecs(IGraphBuilder graphBuilder)
		{
			// add preferred video & audio codecs
			string strVideoCodec="";
			string strAudioCodec="";
			string strAudiorenderer="";
      bool   bAddFFDshow=false;
      using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        bAddFFDshow=xmlreader.GetValueAsBool("dvdplayer","ffdshow",false);
				strVideoCodec=xmlreader.GetValueAsString("dvdplayer","videocodec","");
				strAudioCodec=xmlreader.GetValueAsString("dvdplayer","audiocodec","");
				strAudiorenderer=xmlreader.GetValueAsString("dvdplayer","audiorenderer","");
			}
			if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strVideoCodec);
			if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(graphBuilder,strAudioCodec);
			if (strAudiorenderer.Length>0) DirectShowUtil.AddAudioRendererToGraph(graphBuilder,strAudiorenderer,false);
      if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(graphBuilder,"ffdshow raw video filter");

      //Type comtype = Type.GetTypeFromCLSID( Clsid.VideoMixingRenderer9 );
      //object comobj = Activator.CreateInstance( comtype );
      //IBaseFilter VMR9Filter=(IBaseFilter)comobj; comobj=null;
			//graphBuilder.AddFilter(VMR9Filter,"VMR9");

		}

    public override bool EnableSubtitle
    {
      get 
      {
        return m_bSubtitlesEnabled;
      }
      set 
      {
        m_bSubtitlesEnabled=value;
        dvdCtrl.SetSubpictureState(m_bSubtitlesEnabled,DvdCmdFlags.None,null);
      }
    }
    /*
    public override int GetHDC()
    {
      if (m_ovMgr!=null)
      {
        return m_ovMgr.GetHDC();
      }
      return 0;
    }
    public override void ReleaseHDC(int HDC)
    {
      if (m_ovMgr!=null)
      {
        m_ovMgr.ReleaseDC(HDC);
      }
    }*/

    
    protected virtual void SetVideoPosition(System.Drawing.Rectangle rDest)
    {
      if (videoWin!=null)
      {
        videoWin.SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
      }
    }

    protected virtual void  SetSourceDestRectangles(System.Drawing.Rectangle rSource,System.Drawing.Rectangle rDest)
    {
      if (basicVideo!=null)
      {
        basicVideo.SetSourcePosition(rSource.Left,rSource.Top,rSource.Width,rSource.Height);
        basicVideo.SetDestinationPosition(0,0,rDest.Width,rDest.Height);
      }
    }

    public override bool CanSeek()
    {
      if ( (m_iUOPs & UOP_FLAG_Play_Title_Or_AtTime)==0) return true;
      if ( (m_iUOPs & UOP_FLAG_Play_Chapter_Or_AtTime)==0) return true;
      return false;
    }
  }
}
