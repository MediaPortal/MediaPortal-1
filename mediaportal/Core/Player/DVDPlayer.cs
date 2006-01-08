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
//using DirectX.Capture;
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
    protected	PlayState				_state;

    /// <summary> current mode of playback (movie/menu/still). </summary>
    protected	MenuMode				_menuMode;

    /// <summary> asynchronous command interface. </summary>
    protected	OptIDvdCmd				_cmdOption = new OptIDvdCmd();
    /// <summary> asynchronous command pending. </summary>
    protected	bool					_pendingCmd;

    /// <summary> dvd graph builder interface. </summary>
    protected IDvdGraphBuilder		_dvdGraph=null;
    /// <summary> dvd control interface. </summary>
    protected IDvdControl2			_dvdCtrl=null;
    /// <summary> dvd information interface. </summary>
    protected IDvdInfo2				_dvdInfo=null;
		protected IBaseFilter _dvdbasefilter=null;
    /// <summary> dvd video playback window interface. </summary>
    protected IVideoWindow			_videoWin=null;
    protected IBasicVideo2			_basicVideo=null; 
    protected IGraphBuilder			_graphBuilder=null;
		protected IAMLine21Decoder	_line21Decoder=null;
    
    protected int                       _videoPref=0;
    protected AmAspectRatioMode arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED;
    /// <summary> control interface. </summary>
    protected IMediaControl			    _mediaCtrl=null;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx			    _mediaEvt=null;

    /// <summary> interface to single-step video. </summary>
    //protected IVideoFrameStep			  videoStep=null;
    protected int                           _volume=100;
    //protected OVTOOLLib.OvMgrClass	m_ovMgr=null;

    protected DvdTimeCode				    _currTime;		// copy of current playback states, see OnDvdEvent()
    protected int						        _currTitle=0;
    protected int						        _currChapter=0;
    protected DvdDomain				      _currDomain;
    protected IBasicAudio				    _basicAudio=null;
    protected IMediaPosition		    _mediaPos=null;
		protected IBaseFilter								_videoCodecFilter=null;
		protected IBaseFilter								_audioCodecFilter=null;
		protected IBaseFilter								_audioRendererFilter=null;
		protected IBaseFilter								_ffdShowFilter=null;

		VMR7Util  _vmr7 = null;
		protected int                   _speed=1;
    protected double                _currentTime=0;
    protected bool                          _visible=true;
    protected bool                          _started=false;
    protected int		                _rotCookie = 0;
    
    protected int 											    _positionX=80;
    protected int 											    _positionY=400;
    protected int 											    _width=200;
    protected int 											    _height=100;
    protected int                           _videoWidth=100;
    protected int                           _videoHeight=100;
    protected bool											    _updateNeeded=false;
    protected bool											    _fullScreen=true;
    protected string                        _audioLanguage="";
    protected string                        _subtitleLanguage="";
    protected bool                          _subtitlesEnabled=true;
    protected bool                          _freeNavigator=false;
    protected int                           _currentAudioStream=-1;
    protected bool                          _menuOn=false;
    protected int                           _UOPs;
    protected string                        _currentFile;
		protected double												_duration;
    protected MediaPortal.GUI.Library.Geometry.Type             _aspectRatio=MediaPortal.GUI.Library.Geometry.Type.Normal;

    protected const int WM_DVD_EVENT		= 0x00008002;	// message from dvd graph
    protected const int WS_CHILD			= 0x40000000;	// attributes for video window
    protected const int WS_CLIPCHILDREN	= 0x02000000;
    protected const int WS_CLIPSIBLINGS	= 0x04000000;
    protected const int WM_MOUSEMOVE    =0x0200;
    protected const int WM_LBUTTONUP    =0x0202;

		ArrayList _mouseMsg ;
    public DVDPlayer()
    {
    }
    public override void WndProc( ref Message m )
    {
			try
			{
				if( m.Msg == WM_DVD_EVENT )
				{
				
					if( _mediaEvt != null )
						OnDvdEvent();
					return;
				}

				if( m.Msg==WM_MOUSEMOVE)
				{
					if (_menuMode!=MenuMode.No)
						_mouseMsg.Add(m);
				}

				if( m.Msg==WM_LBUTTONUP)
				{
					if (_menuMode!=MenuMode.No)
						_mouseMsg.Add(m);
				}
			}
			catch(Exception ex)
			{

				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:WndProc() {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);			
			}
    }

    public override bool Play(string file)
    {
      _UOPs=0;
      _menuOn=false;
      _started=false;
      _visible=false;
      _positionX=80;
      _positionY=400;
      _width=200;
      _height=100;
      _videoWidth=100;
      _videoHeight=100;
      _speed=1;
      _audioLanguage="";
      _subtitleLanguage="";
      _subtitlesEnabled=true;
      _aspectRatio=MediaPortal.GUI.Library.Geometry.Type.Normal;
      _speed=1;
      _currentTime=0;
      _visible=true;
      _started=false;
      _rotCookie = 0;
      _volume=100;
			_mouseMsg  = new ArrayList();


      bool result=FirstPlayDvd(file);
      if (!result) 
      {
        return false;
      }

      _updateNeeded=true;

      _fullScreen=true;
      _updateNeeded=true;

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

    public void SelectAudioLanguage(string language)
    {
      int streamsAvailable=0;
      int currentStream=0;
      Log.Write("DVDPlayer:SelectAudioLanguage:"+ language);
      int hr=_dvdInfo.GetCurrentAudio( out streamsAvailable,out currentStream);
      if (hr<0) 
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetCurrentAudio() failed");
        return;
      }
      Log.Write("DVDPlayer:found {0} audiostreams", streamsAvailable.ToString());
      if (streamsAvailable<=0) return;
      foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures ) )  
      {
        if (String.Compare(ci.EnglishName,language,true)==0) 
        {
          for (int i=0; i < streamsAvailable; ++i)
          {
            int audioLanguage;
            hr=_dvdInfo.GetAudioLanguage(i,out audioLanguage);
            if (hr==0)
            {
              if (ci.LCID==(audioLanguage&0x3ff))
              {
                _audioLanguage=language;
                hr=_dvdCtrl.SelectAudioStream(i, DvdCmdFlags.None,null);
                if (hr==0)
                  Log.Write("DVDPlayer:Selected audio stream:{0}", language);
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
      int streamsAvailable=0;
      int currentStream=0;
      bool isDisabled;
      Log.Write("DVDPlayer:SelectSubtitleLanguage:"+ strSubtitleLanguage);
      int hr=_dvdInfo.GetCurrentSubpicture( out streamsAvailable,out currentStream,out isDisabled);
      if (hr<0) 
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetCurrentSubpicture() failed");
        return;
      }
      Log.Write("DVDPlayer:found {0} subpicture streams", streamsAvailable.ToString());
      if (streamsAvailable<=0) return;
      foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures) )  
      {
        if (String.Compare(ci.EnglishName,strSubtitleLanguage,true)==0) 
        {
          for (int i=0; i < streamsAvailable; ++i)
          {
            int subtitleLanguage;
            hr=_dvdInfo.GetSubpictureLanguage(i,out subtitleLanguage);
            if (hr==0)
            {
              if (ci.LCID==(subtitleLanguage&0x3ff))
              {
                _subtitleLanguage=strSubtitleLanguage;
                hr=_dvdCtrl.SelectSubpictureStream(i, DvdCmdFlags.None,null);
                if (hr==0)
                {
                  hr=_dvdCtrl.SetSubpictureState(_subtitlesEnabled,DvdCmdFlags.None,null);
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
    bool FirstPlayDvd(string file)
    {
      int hr;
  	
	
      try 
      {
        
        _pendingCmd = true; 
        UpdateMenu();
        CloseInterfaces();
        string path=null;
        _currentFile=file;
        if (file!="")
        {
          int ipos=file.LastIndexOf(@"\");
          if (ipos>0)
          {
            path=file.Substring(0,ipos);
          }
        }
        if( ! GetInterfaces(path) )
        {
          Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable getinterfaces()");
          CloseInterfaces();
          return false;
        }

		  using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
		  {
			  _audioLanguage   =xmlreader.GetValueAsString("dvdplayer","audiolanguage","english");
			  _subtitleLanguage=xmlreader.GetValueAsString("dvdplayer","subtitlelanguage","english");
			  _subtitlesEnabled  =xmlreader.GetValueAsBool("dvdplayer","showsubtitles",true);
		  }
      
		  SetDefaultLanguages();
        

			  hr = _mediaEvt.SetNotifyWindow( GUIGraphicsContext.ActiveForm, WM_DVD_EVENT, IntPtr.Zero );
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to SetNotifyWindow 0x{0:X}",hr);
				}

				if (_videoWin!=null)
        {
					if( hr == 0 )
					{
						hr = _videoWin.put_Owner( GUIGraphicsContext.ActiveForm);
						if (hr!=0)
						{
							Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to set window owner 0x{0:X}",hr);
						}
					}
					if( hr == 0 )
					{
							hr = _videoWin.put_WindowStyle( WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN );
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
				if (_basicVideo!=null)
				{
					_basicVideo.SetDefaultSourcePosition();
					_basicVideo.SetDefaultDestinationPosition();
				}
        if (_videoWin!=null)
        {
          _videoWin.SetWindowPosition( 0,0,GUIGraphicsContext.Width,GUIGraphicsContext.Height);
        }
    
        hr = _mediaCtrl.Run();
        if (hr<0 || hr >1)
        {
          Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to start playing() 0x:{0:X}",hr);
          CloseInterfaces();
          return false;
        }
        //DsUtils.DumpFilters(_graphBuilder);
        DvdDiscSide side;
        int titles,numOfVolumes,volume;
        hr=_dvdInfo.GetDVDVolumeInfo(out numOfVolumes, out volume, out side, out titles);
				if (hr < 0) 
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:Unable to get dvdvolumeinfo 0x{0:X}",hr);
					//return false;
				}
				else
				{
					if (titles<=0) 
					{
						Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:DVD does not contain any titles? {0}",titles);
						//return false;
					}
				}
        
        if (_videoWin!=null)
        {
            hr = _videoWin.put_MessageDrain( GUIGraphicsContext.ActiveForm );
        }

        hr=_dvdCtrl.SelectVideoModePreference(_videoPref);
        DvdVideoAttr videoAttr;
        hr=_dvdInfo.GetCurrentVideoAttributes(out videoAttr);
        _videoWidth =videoAttr.sourceResolutionX;
        _videoHeight=videoAttr.sourceResolutionY;

        _state = PlayState.Playing;
        _pendingCmd = false;
        Log.Write("DVDPlayer:Started playing()");
        if (_currentFile==String.Empty)
        {
          for (int i=0; i <= 26;++i)
          {
            string dvd=String.Format("{0}:", (char)('A'+i));
            if (Utils.IsDVD(dvd))
            {
              _currentFile=String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO",(char)('A'+i));
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
			if (_graphBuilder==null) return;
      int hr;
      try 
      {
        Log.Write("DVDPlayer:cleanup DShow graph");			

        if( _mediaCtrl != null )
        {
          hr = _mediaCtrl.Stop();
          _mediaCtrl = null;
        }
        _state = PlayState.Stopped;

				_mediaEvt = null;
        _visible=false;
				_videoWin = null;
//				videoStep	= null;
				_dvdCtrl = null;
				_dvdInfo = null;
				_basicVideo=null;
				_basicAudio=null;
				_mediaPos=null;
				
				if (_vmr7!=null)
					_vmr7.RemoveVMR7();
				_vmr7=null;

				if (_videoCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_videoCodecFilter))>0); 
					_videoCodecFilter=null;
				}
				if (_audioCodecFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_audioCodecFilter))>0); 
					_audioCodecFilter=null;
				}
				
				if (_audioRendererFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_audioRendererFilter))>0); 
					_audioRendererFilter=null;
				}
				
				if (_ffdShowFilter!=null) 
				{
					while ( (hr=Marshal.ReleaseComObject(_ffdShowFilter))>0); 
					_ffdShowFilter=null;
				}

				if( _dvdbasefilter != null )
				{
					while ((hr=Marshal.ReleaseComObject( _dvdbasefilter))>0); 
					_dvdbasefilter = null;              
				}
	
        if( _cmdOption.dvdCmd != null )
          Marshal.ReleaseComObject( _cmdOption.dvdCmd ); 
				_cmdOption.dvdCmd = null;
        _pendingCmd = false;
				if (_line21Decoder!=null)
				{
					while ((hr=Marshal.ReleaseComObject( _line21Decoder))>0); 
					_line21Decoder=null;
				}

        if (_rotCookie !=0) 
					DsROT.RemoveGraphFromRot( ref _rotCookie );		// _graphBuilder capGraph
				_rotCookie=0;

        if (_graphBuilder!=null)
        {
          DsUtils.RemoveFilters(_graphBuilder);
          while ((hr=Marshal.ReleaseComObject( _graphBuilder ))>0); 
					_graphBuilder = null;
        }

				if( _dvdGraph != null )
				{
					while ((hr=Marshal.ReleaseComObject( _dvdGraph ))>0); 
					_dvdGraph = null;
				}
				_state = PlayState.Init;
				GUIGraphicsContext.form.Invalidate(true);          
        GUIGraphicsContext.form.Activate();
      }
      catch( Exception ex)
      {
        Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:exception while cleanuping DShow graph {0} {1}",ex.Message, ex.StackTrace);
      }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected virtual bool GetInterfaces(string path)
    {
      int		            hr;
      Type	            comtype = null;
      object	          comobj = null;
      _freeNavigator=true;
      _dvdInfo=null;
      _dvdCtrl=null;

      string dvdNavigator="";
      string aspectRatioMode="";
      string displayMode="";
      bool  useAC3Filter=false;
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        dvdNavigator=xmlreader.GetValueAsString("dvdplayer","navigator","");
        aspectRatioMode=xmlreader.GetValueAsString("dvdplayer","armode","").ToLower();
        if ( aspectRatioMode=="crop") arMode=AmAspectRatioMode.AM_ARMODE_CROP;
        if ( aspectRatioMode=="letterbox") arMode=AmAspectRatioMode.AM_ARMODE_LETTER_BOX;
        if ( aspectRatioMode=="stretch") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED;
        if ( aspectRatioMode=="follow stream") arMode=AmAspectRatioMode.AM_ARMODE_STRETCHED_AS_PRIMARY;
        useAC3Filter= xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
        displayMode=xmlreader.GetValueAsString("dvdplayer","displaymode","").ToLower();
        if (displayMode=="default") _videoPref=0;
        if (displayMode=="16:9") _videoPref=1;
        if (displayMode=="4:3 pan scan") _videoPref=2;
        if (displayMode=="4:3 letterbox") _videoPref=3;
      }
      try 
      {
        
        comtype = Type.GetTypeFromCLSID( Clsid.DvdGraphBuilder );
        if( comtype == null )
          throw new NotSupportedException( "DirectX (8.1 or higher) not installed?" );
        comobj = Activator.CreateInstance( comtype );
        _dvdGraph = (IDvdGraphBuilder) comobj; comobj = null;

        hr = _dvdGraph.GetFiltergraph( out _graphBuilder );
        if( hr < 0 )
          Marshal.ThrowExceptionForHR( hr );
        DsROT.AddGraphToRot( _graphBuilder, out _rotCookie );		// _graphBuilder capGraph
				_vmr7=new VMR7Util();
				_vmr7.AddVMR7(_graphBuilder);

				
        try
        {

          _dvdbasefilter=DirectShowUtil.AddFilterToGraph(_graphBuilder,dvdNavigator);
          if (_dvdbasefilter!=null)
          {
            IDvdControl2 cntl=(IDvdControl2)_dvdbasefilter;
            if (cntl!=null)
            {
              _dvdInfo = (IDvdInfo2) cntl;
              _dvdCtrl = (IDvdControl2)cntl;
							if (path!=null) 
							{
								if (path.Length!=0)
									cntl.SetDVDDirectory(path);
							}
							_dvdCtrl.SetOption( DvdOptionFlag.HmsfTimeCodeEvt, true );	// use new HMSF timecode format
							_dvdCtrl.SetOption( DvdOptionFlag.ResetOnStop, false );

							AddPreferedCodecs(_graphBuilder);
							DirectShowUtil.RenderOutputPins(_graphBuilder,_dvdbasefilter);

              _videoWin	= _graphBuilder as IVideoWindow;
              _freeNavigator=false;
            }

            //Marshal.ReleaseComObject( _dvdbasefilter); _dvdbasefilter = null;              
          }
        }
        catch(Exception ex)
        {
					string strEx=ex.Message;
        }
				Guid riid ;

        
        if (useAC3Filter)
        {
          string ac3filterMonikerString =@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{A753A1EC-973E-4718-AF8E-A3F554D45C44}";
          Log.Write("DVDPlayer:Adding AC3 filter to graph");
          IBaseFilter filter = Marshal.BindToMoniker( ac3filterMonikerString ) as IBaseFilter;
          if (filter!=null)
          {
            hr = _graphBuilder.AddFilter( filter, "AC3 Filter" );
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
			
        if (_dvdInfo==null)
        {
          riid = typeof( IDvdInfo2 ).GUID;
          hr = _dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          _dvdInfo = (IDvdInfo2) comobj; comobj = null;
        }

        if (_dvdCtrl==null)
        {
          riid = typeof( IDvdControl2 ).GUID;
          hr = _dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          _dvdCtrl = (IDvdControl2) comobj; comobj = null;
        }


        _mediaCtrl	= (IMediaControl)  _graphBuilder;
        _mediaEvt	= (IMediaEventEx)  _graphBuilder;
        _basicAudio	= _graphBuilder as IBasicAudio;
        _mediaPos	= (IMediaPosition) _graphBuilder;
        _basicVideo	= _graphBuilder as IBasicVideo2;
        _videoWin	= _graphBuilder as IVideoWindow;



				// disable Closed Captions!
				IBaseFilter baseFilter;
				_graphBuilder.FindFilterByName("Line 21 Decoder", out baseFilter);
				if (baseFilter==null)
					_graphBuilder.FindFilterByName("Line21 Decoder", out baseFilter);
				if (baseFilter!=null)
				{
					_line21Decoder=(IAMLine21Decoder)baseFilter;
					if (_line21Decoder!=null)
					{
						AMLine21CCState state=AMLine21CCState.Off;
						hr=_line21Decoder.SetServiceState(ref state);
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
        if (_videoWin==null)
        {
          riid = typeof( IVideoWindow ).GUID;
          hr = _dvdGraph.GetDvdInterface( ref riid, out comobj );
          if( hr < 0 )
            Marshal.ThrowExceptionForHR( hr );
          _videoWin = (IVideoWindow) comobj; comobj = null;
        }
  */      
       // GetFrameStepInterface();

        DirectShowUtil.SetARMode(_graphBuilder,arMode);
        DirectShowUtil.EnableDeInterlace(_graphBuilder);
        //m_ovMgr = new OVTOOLLib.OvMgrClass();
        //m_ovMgr.SetGraph(_graphBuilder);

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
					hr = _mediaEvt.GetEvent( out code, out p1, out p2, 0 );
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
								_currTime.bHours	= ati[0];
								_currTime.bMinutes	= ati[1];
								_currTime.bSeconds	= ati[2];
								_currTime.bFrames	= ati[3];
								_currentTime=( (double)_currTime.bHours)* 3600d;
								_currentTime +=( ( (double)_currTime.bMinutes)* 60d );
								_currentTime +=( ( (double)_currTime.bSeconds) );
							}

							break;
						}
					
						case DsEvCode.DvdSubPicStChange:
						{
							Log.Write("EVT:DvdSubPicture Changed to:{0} Enabled:{1}",p1,p2);
							//_subtitleLanguage = p1.ToString();
							//_subtitlesEnabled=(p2!=0);											
						}
							break;

						case DsEvCode.DvdChaptStart:
						{
							Log.Write("EVT:DvdChaptStart:{0}",p1);
							_currChapter = p1;
							SelectSubtitleLanguage(_subtitleLanguage);
							DvdTimeCode totaltime;
							int         ulTimeCodeFlags; 
							_dvdInfo.GetTotalTitleTime( out totaltime, out ulTimeCodeFlags );
          
							_duration=( (double)totaltime.bHours)* 3600d;
							_duration +=( ( (double)totaltime.bMinutes)* 60d );
							_duration +=( ( (double)totaltime.bSeconds) );
							break;
						}
						case DsEvCode.DvdTitleChange:
						{
							Log.Write("EVT:DvdTitleChange:{0}",p1);
							_currTitle = p1;
							SelectSubtitleLanguage(_subtitleLanguage);

							DvdTimeCode totaltime;
							int         ulTimeCodeFlags; 
							_dvdInfo.GetTotalTitleTime( out totaltime, out ulTimeCodeFlags );
          
							_duration=( (double)totaltime.bHours)* 3600d;
							_duration +=( ( (double)totaltime.bMinutes)* 60d );
							_duration +=( ( (double)totaltime.bSeconds) );

							break;
						}
	          
				
						case DsEvCode.DvdCmdStart:
						{
							if( _pendingCmd )
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
								_menuMode = MenuMode.Buttons;
							else
								_menuMode = MenuMode.Still;
							break;
						}
						case DsEvCode.DvdStillOff:
						{
							Log.Write("EVT:DvdStillOff:{0}",p1);
							if( _menuMode == MenuMode.Still )
								_menuMode = MenuMode.No;
							break;
						}
						case DsEvCode.DvdButtonChange:
						{
							Log.Write("EVT:DvdButtonChange: buttons:#{0}",p1);
							if( p1 <= 0 )
								_menuMode = MenuMode.No;
							else
								_menuMode = MenuMode.Buttons;
							break;
						}

						case DsEvCode.DvdNoFpPgc:
						{
							Log.Write("EVT:DvdNoFpPgc:{0}",p1);
							if( _dvdCtrl != null )
								hr = _dvdCtrl.PlayTitle( 1, DvdCmdFlags.None, null );
							break;
						}

						case DsEvCode.DvdAudioStChange:
							// audio stream changed
							Log.Write("EVT:DvdAudioStChange:{0}",p1);
							break;

						case DsEvCode.DvdValidUopsChange:
							Log.Write("EVT:DvdValidUopsChange:0x{0:X}",p1);
							_UOPs=p1;
							break;

						case DsEvCode.DvdDomChange:
						{
							_currDomain = (DvdDomain) p1;
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
									_menuOn=true;
									break;
								case DvdDomain.VideoTitleSetMenu:
									Log.Write("EVT:DVDPlayer:domain=videotitlesetmenu (menu)");
									_menuOn=true;
									break;
								case DvdDomain.Title:
									Log.Write("EVT:DVDPlayer:domain=title (no menu)");
									_menuOn=false;
									break;  
								default:
									Log.Write("EVT:DvdDomChange:{0}",p1);
									break;
							}
							break;
						}
					}

					hr = _mediaEvt.FreeEventParams( code, p1, p2 );
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
				if( (_pendingCmd == false) || (_dvdInfo == null) )
					return;

				IDvdCmd		cmd;
				int hr = _dvdInfo.GetCmdFromEvent( p1, out cmd );
				if( (hr != 0) || (cmd == null) )
				{
					Log.WriteFile(Log.LogType.Log,true, "!!!DVD OnCmdComplete GetCmdFromEvent failed!!!" );
					return;
				}

				if( cmd != _cmdOption.dvdCmd )
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:DVD OnCmdComplete UNKNOWN CMD!!!" );
					Marshal.ReleaseComObject( cmd ); cmd = null;
					return;
				}

				Marshal.ReleaseComObject( cmd ); cmd = null;
				Marshal.ReleaseComObject( _cmdOption.dvdCmd ); _cmdOption.dvdCmd = null;
				_pendingCmd = false;
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
        if (_state!=PlayState.Init) 
        {

          return _duration;
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get 
      {
        if (_state!=PlayState.Init) 
        {
          return _currentTime;
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
        _mediaCtrl.Run();
        _state=PlayState.Playing;
      }
      else if (_state==PlayState.Playing) 
      {
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
			int hr=0;
      if( (_mediaCtrl == null) ||
        ((_state != PlayState.Playing) && (_state != PlayState.Paused) ) )
        return;
			Log.Write("DVDPlayer:Stop()");
			if (_mediaEvt!=null)
				hr = _mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_DVD_EVENT, IntPtr.Zero );
			
			hr = _mediaCtrl.Stop();
			if( hr >= 0 )
      {
        _state = PlayState.Stopped;
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
        return _speed;
      }
      set 
      {
        if (_state!=PlayState.Init)
        {
          try
          {
            _speed=value;
            if (_speed>=1)
            {
              _dvdCtrl.PlayForwards((double)_speed,DvdCmdFlags.None,null);
            }
            else if (_speed<0)
            {
              _dvdCtrl.PlayBackwards((double)-_speed,DvdCmdFlags.None,null);
            }
          }
          catch(Exception )
          {
            _speed=1;
          }
        }
      }
    }

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
              float percent=(float)_volume/100.0f;
              int volume=(int)( (DirectShowVolume.VOLUME_MAX-DirectShowVolume.VOLUME_MIN) *percent);
              _basicAudio.put_Volume( (volume-DirectShowVolume.VOLUME_MIN));

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
        if (_aspectRatio != value)
        {
          _aspectRatio=value;
          _updateNeeded=true;
        }
      }
    }

    public override void SeekRelative(double newTime)
    {
      if (_state!=PlayState.Init)
      {
        if (_mediaCtrl!=null && _mediaPos!=null)
        {
          
          double currentPosition=CurrentPosition;
          newTime=currentPosition+newTime;
          if (newTime<0.0d) newTime=0.0d;
          if (newTime < Duration)
          {
            SeekAbsolute(newTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double newTime)
    {
      if (_state!=PlayState.Init)
      {
        if (_mediaCtrl!=null && _mediaPos!=null)
        {
          if (newTime<0.0d) newTime=0.0d;
          if (newTime < Duration)
          {
            int hours=(int)(newTime/3600d);
            newTime -= (hours*3600);
            int minutes = (int)(newTime / 60d);
            newTime -= (minutes*60);
						int seconds = (int)newTime;
						Log.Write("DVDPlayer:Seek to {0}:{1}:{2}", hours,minutes,seconds);
            DvdTimeCode timeCode;
            timeCode.bHours=(byte)(hours&0xff);
            timeCode.bMinutes=(byte)(minutes&0xff);
            timeCode.bSeconds=(byte)(seconds&0xff);
            timeCode.bFrames=0;
            DvdPlayLocation loc;
            _currTitle=_dvdInfo.GetCurrentLocation(out loc);
						
			
						

						int hr=_dvdCtrl.PlayAtTime(ref timeCode,DvdCmdFlags.Block,null);
            if (hr!=0)
            {
              if ( ((uint)hr)==VFW_E_DVD_OPERATION_INHIBITED) Log.Write("DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) not allowed at this point",hours,minutes,seconds);
              else if ( ((uint)hr)==VFW_E_DVD_INVALIDDOMAIN) Log.Write("DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) invalid domain",hours,minutes,seconds);
              else Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) failed:0x{3:X}",hours,minutes,seconds,hr); 
            }
            //SetDefaultLanguages();
						
						Log.Write("DVDPlayer:Seek to {0}:{1}:{2} done", hours,minutes,seconds);
          }
        }
      }
    }

    public override void SeekRelativePercentage(int percentage)
    {
      if (_state!=PlayState.Init)
      {
        if (_mediaCtrl!=null && _mediaPos!=null)
        {
          double currentPos=CurrentPosition;
          double duration=Duration;

          double curPercent=(currentPos/Duration)*100.0d;
          double onePercent=Duration/100.0d;
          curPercent=curPercent + (double)percentage;
          curPercent*=onePercent;
          if (curPercent<0.0d) curPercent=0.0d;
          if (curPercent<Duration)
          {
            SeekAbsolute(curPercent);
          }
        }
      }
    }


    public override void SeekAsolutePercentage(int percentage)
    {
      if (_state!=PlayState.Init)
      {
        if (_mediaCtrl!=null && _mediaPos!=null)
        {
          if (percentage<0) percentage=0;
          if (percentage>=100) percentage=100;
          double percent=Duration/100.0f;
          percent*=(double)percentage;
          SeekAbsolute(percent);
        }
      }
    }

    
		public override bool GetResumeState(out byte[] resumeData)
		{
      try
      {
        Log.Write("DVDPlayer::GetResumeState() begin");
        resumeData = null;
        IDvdState dvdState;
        int hr = _dvdInfo.GetState(out dvdState);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Log, true, "DVDPlayer:GetResumeState() _dvdInfo.GetState failed");
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
          Log.Write("DVDPlayer::GetResumeState() failed {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }

        Marshal.FreeCoTaskMem(stateData);
        Marshal.ReleaseComObject(dvdStatePersistMemory);
        Marshal.ReleaseComObject(dvdState);
      }
      catch (Exception)
      {
        resumeData = null;
      }
			return true;
		}

    
		public override bool SetResumeState(byte[] resumeData)
		{
			if (resumeData.Length > 0)
			{
				Log.Write("DVDPlayer::SetResumeState() begin");
				IDvdState dvdState;

				int hr = _dvdInfo.GetState(out dvdState);
				if (hr < 0)
				{
					Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:GetResumeState() _dvdInfo.GetState failed");
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
				hr = _dvdCtrl.SetState(dvdState, DShowNET.Dvd.DvdCmdFlags.Block, dvdCmd);
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
			if (!_started) return;
			if (GUIGraphicsContext.InVmr9Render) return;
      OnProcess();
			HandleMouseMessages();
      
    }
		
		void HandleMouseMessages()
		{
			if (!GUIGraphicsContext.IsFullScreenVideo) return;
			if (GUIGraphicsContext.Vmr9Active) return;
			try
			{	

				DsPOINT  pt;
				foreach(Message m in _mouseMsg)
				{
					long lParam=m.LParam.ToInt32();
					if( m.Msg==WM_MOUSEMOVE)
					{
						pt=new DsPOINT();
						pt.X = (int)(lParam  & 0xffff); 
						pt.Y = (int)((lParam>>16)  & 0xffff); 

						// Select the button at the current position, if it exists
						_dvdCtrl.SelectAtPosition(pt);
					}

					if( m.Msg==WM_LBUTTONUP)
					{
						pt=new DsPOINT();
						pt.X = (int)(lParam  & 0xffff); 
						pt.Y = (int)((lParam>>16)  & 0xffff); 

						// Highlight the button at the current position, if it exists
						_dvdCtrl.ActivateAtPosition(pt);
					}
				}
			}
			catch(Exception ex)
			{

				Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:HandleMouseMessages() {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);			
			}
			_mouseMsg.Clear();
		}

    protected virtual void OnProcess()
    {
			
			if (_vmr7!=null)
			{
				_vmr7.Process();
			}
      if (_videoWin!=null)
      {
        if (GUIGraphicsContext.Overlay==false && GUIGraphicsContext.IsFullScreenVideo==false)
        {
          if (_visible)
          {
            _visible=false;
            _videoWin.put_Visible( DsHlp.OAFALSE );
          }
        }
        else if (!_visible)
        {
          _visible=true;
          _videoWin.put_Visible( DsHlp.OATRUE );
        }
      }
    }

    public override void SetVideoWindow()
    {
      if (_videoWin==null) return;
      if (GUIGraphicsContext.IsFullScreenVideo!= _fullScreen)
      {
        _fullScreen=GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded=true;
      }

      if (!_updateNeeded) return;
      
      _started=true;
      _updateNeeded=false;
      float x=_positionX;
      float y=_positionY;
      
      int nw=_width;
      int nh=_height;
      if (nw > GUIGraphicsContext.OverScanWidth)
        nw=GUIGraphicsContext.OverScanWidth;
      if (nh > GUIGraphicsContext.OverScanHeight)
        nh=GUIGraphicsContext.OverScanHeight;
      lock ( typeof(DVDPlayer) )
      {
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          x=_positionX=GUIGraphicsContext.OverScanLeft;
          y=_positionY=GUIGraphicsContext.OverScanTop;
          nw=_width=GUIGraphicsContext.OverScanWidth;
          nh=_height=GUIGraphicsContext.OverScanHeight;
        }
        if (nw <=0 || nh <=0) return;

        System.Drawing.Rectangle source,destination;
				
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

        MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
        m_geometry.ImageWidth  =_videoWidth;
        m_geometry.ImageHeight =_videoHeight;
        m_geometry.ScreenWidth =nw;
        m_geometry.ScreenHeight=nh;
        m_geometry.ARType=GUIGraphicsContext.ARType;
				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
        {
          bool bUseAR=xmlreader.GetValueAsBool("dvdplayer","pixelratiocorrection",false);
          if (bUseAR) m_geometry.PixelRatio=GUIGraphicsContext.PixelRatio;
          else m_geometry.PixelRatio=1.0f;
        }
				m_geometry.GetWindow(aspectX,aspectY,out source, out destination);
				destination.X += (int)x;
        destination.Y += (int)y;
        

				Log.Write("overlay: video WxH  : {0}x{1}",_videoWidth,_videoHeight);
				Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
				Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
				Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
				Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
				Log.Write("overlay: src        : ({0},{1})-({2},{3})",
					source.X,source.Y, source.X+source.Width,source.Y+source.Height);
				Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
					destination.X,destination.Y,destination.X+destination.Width,destination.Y+destination.Height);


        SetSourceDestRectangles(source,destination);
        SetVideoPosition(destination);

				

        //hr=_videoWin.SetWindowPosition( destination.X, destination.Y, destination.Width, destination.Height );
        //hr=_dvdCtrl.SelectVideoModePreference(_videoPref);        
        DirectShowUtil.SetARMode(_graphBuilder,arMode);

        m_SourceRect=source;
        m_VideoRect=destination;
        
      }
    }

    void MovieEnded()
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      if (null!=_videoWin) 
      {
        Log.Write("DVDPlayer: ended");
        _state=PlayState.Init;
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
						if( _menuOn )
						{
							Log.Write("DVDPlayer: move left");
							_dvdCtrl.SelectRelativeButton( DvdRelButton.Left );
							return true;
						}
						break;
					case Action.ActionType.ACTION_MOVE_RIGHT:
						if( _menuOn )
						{
							Log.Write("DVDPlayer: move right");
							_dvdCtrl.SelectRelativeButton( DvdRelButton.Right );
							return true;
						}
						break;
					case Action.ActionType.ACTION_MOVE_UP:
						if( _menuOn )
						{
							
							Log.Write("DVDPlayer: move up");
							_dvdCtrl.SelectRelativeButton( DvdRelButton.Upper );
							return true;
						}
						break;
					case Action.ActionType.ACTION_MOVE_DOWN:
						if( _menuOn )
						{	
							Log.Write("DVDPlayer: move down");
							_dvdCtrl.SelectRelativeButton( DvdRelButton.Lower );
							return true;
						}
						break;
					case Action.ActionType.ACTION_SELECT_ITEM:
						if( (_menuMode == MenuMode.Buttons) && (_dvdCtrl != null) )
						{	
							Log.Write("DVDPlayer: select");
							_dvdCtrl.ActivateButton();
							return true;
						}
						else if( (_menuMode == MenuMode.Still) && (_dvdCtrl != null) )
						{
							Log.Write("DVDPlayer: still off");
							_dvdCtrl.StillOff();
							return true;
						}
						break;

					case Action.ActionType.ACTION_DVD_MENU:

						if( (_state != PlayState.Playing) || (_dvdCtrl == null) )
							return false;
						Speed=1;
						_dvdCtrl.ShowMenu( DvdMenuID.Root, DvdCmdFlags.Block | DvdCmdFlags.Flush, null );
						return true;

					case Action.ActionType.ACTION_NEXT_CHAPTER:
					{
						if( (_state != PlayState.Playing) || (_dvdCtrl == null) )
							return false;

						Speed=1;
						int hr = _dvdCtrl.PlayNextChapter( DvdCmdFlags.SendEvt, _cmdOption );
						if( hr < 0 )
						{
							Log.WriteFile(Log.LogType.Log,true,"!!! PlayNextChapter error : 0x" + hr.ToString("x") );
							return false;
						}

						if( _cmdOption.dvdCmd != null )
						{
							Trace.WriteLine( "PlayNextChapter cmd pending.........." );
							_pendingCmd = true;
						}

						UpdateMenu();
						return true;
					}

					case Action.ActionType.ACTION_PREV_CHAPTER:
					{
						if( (_state != PlayState.Playing) || (_dvdCtrl == null) )
							return false;

						Speed=1;
						int hr = _dvdCtrl.PlayPrevChapter( DvdCmdFlags.SendEvt, _cmdOption );
						if( hr < 0 )
						{
							Log.WriteFile(Log.LogType.Log,true,"DVDPlayer:!!! PlayPrevChapter error : 0x" + hr.ToString("x") );
							return false;
						}

						if( _cmdOption.dvdCmd != null )
						{
							Trace.WriteLine( "PlayPrevChapter cmd pending.........." );
							_pendingCmd = true;
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
		  int setError=0;
		  string errorText = "";
		  int lCID=GetLCID(_audioLanguage);
		  if (lCID>=0)
		  {
			  setError=0;
			  errorText = "";
			  // Flip: Added more detailed message
			  setError=_dvdCtrl.SelectDefaultAudioLanguage(lCID, DvdAudioLangExt.NotSpecified);
			  switch (setError)
			  {
				  case 0:
					  errorText = "Success.";
					  break;
				  case 631:
					  errorText = "The DVD Navigator filter is not in the Stop domain.";
					  break;
				  default:
					  errorText = "Unknown Error. "+setError;
					  break;
			  }


			  Log.Write("DVDPlayer:Set default language:{0} {1} {2}", _audioLanguage,lCID,errorText);
		  }
		  lCID=GetLCID(_subtitleLanguage);
		  if (lCID>=0)
		  {
			  setError=0;
			  errorText = "";
			  setError=_dvdCtrl.SelectDefaultMenuLanguage(lCID);
			  // Flip: Added more detailed message
			  switch (setError)
			  {
				  case 0:
					  errorText = "Success.";
					  break;
				  case 631:
					  errorText = "The DVD Navigator filter is not in a valid domain.";
					  break;
				  default:
					  errorText = "Unknown Error. "+setError;
					  break;
			  }
			  Log.Write("DVDPlayer:Set default menu language:{0} {1} {2}", _subtitleLanguage,lCID,errorText);
		  }
		  lCID=GetLCID(_subtitleLanguage);
		  if (lCID>=0)
		  {
			  setError=0;
			  errorText = "";
			  setError=_dvdCtrl.SelectDefaultSubpictureLanguage(lCID, DvdSubPicLangExt.NotSpecified);
			  // Flip: Added more detailed message
			  switch (setError)
			  {
				  case 0:
					  errorText = "Success.";
					  break;
				  case 631:
					  errorText = "The DVD Navigator filter is not in a valid domain.";
					  break;
				  default:
					  errorText = "Unknown Error. "+setError;
					  break;
			  }
			  Log.Write("DVDPlayer:Set subtitle language:{0} {1} {2}", _subtitleLanguage,lCID,errorText);
		  }

		  _dvdCtrl.SetSubpictureState(_subtitlesEnabled,DvdCmdFlags.None,null);
      
	  }

	  int GetLCID(string language)
	  {
		  if (language==null) return -1;
		  if (language.Length==0) return -1;
		  // Flip: Added to cut off the detailed name info
		  string	cutName;
		  int		start = 0;
		  // Flip: Changed from CultureTypes.NeutralCultures to CultureTypes.SpecificCultures
		  // Flip: CultureTypes.NeutralCultures did not work, provided the wrong CLID
		  foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.SpecificCultures ) )  
		  {
			  // Flip: cut off detailed info, e.g. "English (United States)" -> "English"
			  // Flip: to get correct compare
			  start = ci.EnglishName.IndexOf(" (");
			  if (start>0)
				  cutName = ci.EnglishName.Substring(0,start);
			  else
				  cutName = ci.EnglishName;

			  if (String.Compare(cutName,language,true)==0) 
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
        
        int streamsAvailable,currentStream;
        int hr=_dvdInfo.GetCurrentAudio( out streamsAvailable,out currentStream);
        if (hr==0) return streamsAvailable;
        return 1;
      }
    }
    public override int CurrentAudioStream
    {
      get 
      { 
        int streamsAvailable,currentStream;
        int hr=_dvdInfo.GetCurrentAudio( out streamsAvailable,out currentStream);
        if (hr==0) return currentStream;
        return 0;
      }
      set 
      {
        _dvdCtrl.SelectAudioStream(value, DvdCmdFlags.None,null);
        _audioLanguage=AudioLanguage(value);
        _currentAudioStream=value;
      }
    }

    public override string AudioLanguage(int iStream)
    {
      int audioLanguage;
      int hr=_dvdInfo.GetAudioLanguage(iStream,out audioLanguage);
      if (hr==0)
      {
        foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures ) )  
        {
          if (ci.LCID==(audioLanguage&0x3ff))
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
        int streamsAvailable=0;
        int currentStream=0;
        bool isDisabled;
        int hr=_dvdInfo.GetCurrentSubpicture( out streamsAvailable,out currentStream,out isDisabled);
        if (hr==0) return streamsAvailable;
        return 1;
      }
    }
    public override int CurrentSubtitleStream
    {
      get { 
        int streamsAvailable=0;
        int currentStream=0;
        bool isDisabled;
        int hr=_dvdInfo.GetCurrentSubpicture( out streamsAvailable,out currentStream,out isDisabled);
        if (hr==0) return currentStream;
        return 0;
      }
      set {
        int hr=_dvdCtrl.SelectSubpictureStream(value, DvdCmdFlags.None,null);
        _subtitleLanguage=SubtitleLanguage(value);
      }
    }
    public override string SubtitleLanguage(int iStream)
    {
      int iLanguage;
      int hr=_dvdInfo.GetSubpictureLanguage(iStream,out iLanguage);
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

		public void AddPreferedCodecs(IGraphBuilder _graphBuilder)
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
			if (strVideoCodec.Length>0) _videoCodecFilter= DirectShowUtil.AddFilterToGraph(_graphBuilder,strVideoCodec);
			if (strAudioCodec.Length>0) _audioCodecFilter= DirectShowUtil.AddFilterToGraph(_graphBuilder,strAudioCodec);
			if (strAudiorenderer.Length>0) _audioRendererFilter=DirectShowUtil.AddAudioRendererToGraph(_graphBuilder,strAudiorenderer,false);
      if (bAddFFDshow) _ffdShowFilter=DirectShowUtil.AddFilterToGraph(_graphBuilder,"ffdshow raw video filter");


		}

    public override bool EnableSubtitle
    {
      get 
      {
        return _subtitlesEnabled;
      }
      set 
      {
        _subtitlesEnabled=value;
        _dvdCtrl.SetSubpictureState(_subtitlesEnabled,DvdCmdFlags.None,null);
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

    
    protected virtual void SetVideoPosition(System.Drawing.Rectangle destination)
    {
      if (_videoWin!=null)
      {
        _videoWin.SetWindowPosition(destination.Left,destination.Top,destination.Width,destination.Height);
      }
    }

    protected virtual void  SetSourceDestRectangles(System.Drawing.Rectangle source,System.Drawing.Rectangle destination)
    {
      if (_basicVideo!=null)
      {
        _basicVideo.SetSourcePosition(source.Left,source.Top,source.Width,source.Height);
        _basicVideo.SetDestinationPosition(0,0,destination.Width,destination.Height);
      }
    }

    public override bool CanSeek()
    {
      if ( (_UOPs & UOP_FLAG_Play_Title_Or_AtTime)==0) return true;
      if ( (_UOPs & UOP_FLAG_Play_Chapter_Or_AtTime)==0) return true;
      return false;
    }
  }
}
