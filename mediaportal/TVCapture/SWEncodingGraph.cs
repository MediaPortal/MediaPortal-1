using System;
using System.Drawing;
using System.Runtime.InteropServices; 
using DShowNET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
	public class SWEncodingGraph : MediaPortal.TV.Recording.IGraph
	{
    enum State
    { 
      None,
      Created,
      Recording,
      Viewing
    };

    int                     m_iCurrentChannel=28;
    int                     m_iCountryCode=31;
    bool                    m_bUseCable=false;
    State                   m_graphState=State.None;
    string                  m_strVideoCaptureFilter="";
    string                  m_strAudioCaptureFilter="";
    string                  m_strVideoCompressor="";
    string                  m_strAudioCompressor="";

    IGraphBuilder           m_graphBuilder=null;
    ICaptureGraphBuilder2   m_captureGraphBuilder=null;
    IBaseFilter             m_filterCaptureVideo=null;
    IBaseFilter             m_filterCaptureAudio=null;
    
    IBaseFilter             m_filterCompressorVideo=null;
    IBaseFilter             m_filterCompressorAudio=null;
    IAMTVTuner              m_TVTuner=null;
    int				              m_rotCookie = 0;						// Cookie into the Running Object Table
    VideoCaptureDevice      m_videoCaptureDevice=null;
    IVideoWindow            m_videoWindow = null;
    IBasicVideo2            m_basicVideo = null;
    IMediaControl					  m_mediaControl=null;
    const int WS_CHILD			= 0x40000000;	
    const int WS_CLIPCHILDREN	= 0x02000000;
    const int WS_CLIPSIBLINGS	= 0x04000000;

		public SWEncodingGraph(int iCountryCode,bool bCable,string strVideoCaptureFilter,string  strAudioCaptureFilter,string  strVideoCompressor,string  strAudioCompressor)
    {
      m_bUseCable=bCable;
      m_iCountryCode=iCountryCode;
      m_graphState=State.None;
      m_strVideoCaptureFilter=strVideoCaptureFilter;
      m_strAudioCaptureFilter=strAudioCaptureFilter;
      m_strVideoCompressor=strVideoCompressor;
      m_strAudioCompressor=strAudioCompressor;
		}

    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public bool CreateGraph()
    {
      if (m_graphState!=State.None) return false;
      DirectShowUtil.DebugWrite("SWGraph:CreateGraph()");

      // find the video capture device
      Filters filters = new Filters();
      Filter   filterVideoCaptureDevice=null;
      Filter   filterAudioCaptureDevice=null;
      foreach (Filter filter in filters.VideoInputDevices)
      {
        if (filter.Name.Equals(m_strVideoCaptureFilter))
        {
          filterVideoCaptureDevice=filter;
          break;
        }
      }
      // find the audio capture device
      if (m_strAudioCaptureFilter.Length>0)
      {
        foreach (Filter filter in filters.AudioInputDevices)
        {
          if (filter.Name.Equals(m_strAudioCaptureFilter))
          {
            filterAudioCaptureDevice=filter;
            break;
          }
        }
      }

      if (filterVideoCaptureDevice==null) 
      {
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
        return false;
      }
      if (filterAudioCaptureDevice==null && m_strAudioCaptureFilter.Length>0) 
      {
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strAudioCaptureFilter);
        return false;
      }

      // Make a new filter graph
      DirectShowUtil.DebugWrite("SWGraph:create new filter graph (IGraphBuilder)");
      m_graphBuilder = (IGraphBuilder) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) ); 

      // Get the Capture Graph Builder
      DirectShowUtil.DebugWrite("SWGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
      Guid clsid = Clsid.CaptureGraphBuilder2;
      Guid riid = typeof(ICaptureGraphBuilder2).GUID;
      m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance( ref clsid, ref riid ); 

      DirectShowUtil.DebugWrite("SWGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
      int hr = m_captureGraphBuilder.SetFiltergraph( m_graphBuilder );
      if( hr < 0 ) 
      {
        DirectShowUtil.DebugWrite("SWGraph:link FAILED");
        return false;
      }
      DirectShowUtil.DebugWrite("SWGraph:Add graph to ROT table");
      DsROT.AddGraphToRot( m_graphBuilder, out m_rotCookie );

      // Get the video device and add it to the filter graph
      DirectShowUtil.DebugWrite("SWGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
      m_filterCaptureVideo = Marshal.BindToMoniker( filterVideoCaptureDevice.MonikerString ) as IBaseFilter;
      if (m_filterCaptureVideo!=null)
      {
        hr = m_graphBuilder.AddFilter( m_filterCaptureVideo, filterVideoCaptureDevice.Name );
        if( hr < 0 ) 
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED:Add Videodevice to filtergraph");
          return false;
        }
      }

      // Get the audio device and add it to the filter graph
      if (filterAudioCaptureDevice!=null)
      {
        // Get the audio device and add it to the filter graph
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() add capture device {0}",m_strAudioCaptureFilter);
        m_filterCaptureAudio = Marshal.BindToMoniker( filterAudioCaptureDevice.MonikerString ) as IBaseFilter;
        if (m_filterCaptureAudio!=null)
        {
          hr = m_graphBuilder.AddFilter( m_filterCaptureAudio, filterAudioCaptureDevice.Name );
          if( hr < 0 ) 
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:Add audiodevice to filtergraph");
            return false;
          }
        }
      }

      // Retrieve TV Tuner if available
      DirectShowUtil.DebugWrite("SWGraph:Find TV Tuner");
      object o = null;
      Guid cat = FindDirection.UpstreamOnly;
      Guid iid = typeof(IAMTVTuner).GUID;
      hr = m_captureGraphBuilder.FindInterface( new Guid[1]{ cat}, null, m_filterCaptureVideo, ref iid, out o );
      if (hr==0) 
      {
        m_TVTuner = o as IAMTVTuner;
      }
      if (m_TVTuner==null)
      {
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() FAILED:no tuner found");
      }

      m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder,m_captureGraphBuilder, m_filterCaptureVideo);

      m_graphState=State.Created;
      return true;
    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public void DeleteGraph()
    {
      if (m_graphState < State.Created) return;

      DirectShowUtil.DebugWrite("SWGraph:DeleteGraph()");
      StopRecording();
      StopViewing();

      if (m_videoWindow!=null)
      {
        Marshal.ReleaseComObject(m_videoWindow);m_videoWindow=null;
      }
      if (m_basicVideo!=null)
      {
        Marshal.ReleaseComObject(m_basicVideo);m_basicVideo=null;
      }
      if (m_mediaControl!=null)
      {
        Marshal.ReleaseComObject(m_mediaControl);m_mediaControl=null;
      }
      

      if (m_filterCaptureVideo!=null)
        Marshal.ReleaseComObject(m_filterCaptureVideo);m_filterCaptureVideo=null;

      if (m_filterCaptureAudio!=null)
        Marshal.ReleaseComObject(m_filterCaptureAudio);m_filterCaptureAudio=null;      

      if (m_filterCompressorVideo!=null)
        Marshal.ReleaseComObject(m_filterCompressorVideo);m_filterCompressorVideo=null;   

      if (m_filterCompressorAudio!=null)
        Marshal.ReleaseComObject(m_filterCompressorAudio);m_filterCompressorAudio=null;   

      if( m_TVTuner != null )
        Marshal.ReleaseComObject( m_TVTuner ); m_TVTuner = null;
			

      if (m_videoCaptureDevice!=null)
      {
        m_videoCaptureDevice.CloseInterfaces();
        m_videoCaptureDevice=null;
      }

      DsUtils.RemoveFilters(m_graphBuilder);

      if( m_rotCookie != 0 )
        DsROT.RemoveGraphFromRot( ref m_rotCookie);
      m_rotCookie=0;


      if( m_captureGraphBuilder != null )
        Marshal.ReleaseComObject( m_captureGraphBuilder ); m_captureGraphBuilder = null;
	
      if( m_graphBuilder != null )
        Marshal.ReleaseComObject( m_graphBuilder ); m_graphBuilder = null;

      m_graphState=State.None;
      return ;
    }

    /// <summary>
    /// Starts timeshifting the TV channel and stores the timeshifting 
    /// files in the specified filename
    /// </summary>
    /// <param name="iChannelNr">TV channel to which card should be tuned</param>
    /// <param name="strFileName">Filename for the timeshifting buffers</param>
    /// <returns>boolean indicating if timeshifting is running or not</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartTimeShifting(int iChannelNr, string strFileName)
    {
      return true;
    }
    
    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting 
    /// </remarks>
    public bool StopTimeShifting()
    {
      return true;
    }


    /// <summary>
    /// Starts recording live TV to a file
    /// <param name="strFileName">filename for the new recording</param>
    /// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
    /// <param name="timeProgStart">Contains the starttime of the current tv program</param>
    /// </summary>
    /// <returns>boolean indicating if recorded is started or not</returns> 
    /// <remarks>
    /// Graph should be timeshifting. When Recording is started the graph is still 
    /// timeshifting
    /// 
    /// A content recording will start recording from the moment this method is called
    /// and ignores any data left/present in the timeshifting buffer files
    /// 
    /// A reference recording will start recording from the moment this method is called
    /// It will examine the timeshifting files and try to record as much data as is available
    /// from the timeProgStart till the moment recording is stopped again
    /// </remarks>
    public bool StartRecording(string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      ///@@@todo
      return false;
    }
    
    
    /// <summary>
    /// Stops recording 
    /// </summary>
    /// <remarks>
    /// Graph should be recording. When Recording is stopped the graph is still 
    /// timeshifting
    /// </remarks>
    public void StopRecording()
    {
      ///@@@todo
    }



    /// <summary>
    /// Switches / tunes to another TV channel
    /// </summary>
    /// <param name="iChannel">New channel</param>
    /// <remarks>
    /// Graph should be timeshifting. 
    /// </remarks>
    public void TuneChannel(int iChannel)
    {
      m_iCurrentChannel=iChannel;

      DirectShowUtil.DebugWrite("SWGraph:TuneChannel() tune to channel:{0}", iChannel);
      if (iChannel<1000)
      {
        if (m_TVTuner==null) return;
        m_TVTuner.put_TuningSpace(0);
        m_TVTuner.put_CountryCode(m_iCountryCode);
        m_TVTuner.put_Mode(DShowNET.AMTunerModeType.TV);
        if (m_bUseCable)
          m_TVTuner.put_InputType(0,DShowNET.TunerInputType.Cable);
        else
          m_TVTuner.put_InputType(0,DShowNET.TunerInputType.Antenna);
        try
        {
          m_TVTuner.put_Channel(iChannel,DShowNET.AMTunerSubChannel.Default,DShowNET.AMTunerSubChannel.Default);

          int iFreq;
          double dFreq;
          m_TVTuner.get_VideoFrequency(out iFreq);
          dFreq=iFreq/1000000d;
          DirectShowUtil.DebugWrite("SWGraph:TuneChannel() tuned to {0} MHz.", dFreq);
        }
        catch(Exception){} 
      }
      DsUtils.FixCrossbarRouting(m_captureGraphBuilder,m_filterCaptureVideo, iChannel<1000, (iChannel==1001), (iChannel==1002), (iChannel==1000) );
    }

    /// <summary>
    /// Returns the current tv channel
    /// </summary>
    /// <returns>Current channel</returns>
    public int GetChannelNumber()
    {
      return m_iCurrentChannel;
    }

    /// <summary>
    /// Property indiciating if the graph supports timeshifting
    /// </summary>
    /// <returns>boolean indiciating if the graph supports timeshifting</returns>
    public bool SupportsTimeshifting()
    {
      return false;
    }


    /// <summary>
    /// Starts viewing the TV channel 
    /// </summary>
    /// <param name="iChannelNr">TV channel to which card should be tuned</param>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartViewing(int iChannelNr)
    {
      ///@@@todo
      if (m_graphState!=State.Created) return false;
      TuneChannel(iChannelNr);

      m_videoCaptureDevice.RenderPreview();

      m_videoWindow = (IVideoWindow) m_graphBuilder;
      m_basicVideo = (IBasicVideo2) m_graphBuilder;
      m_mediaControl=(IMediaControl)m_graphBuilder;
      int hr = m_videoWindow.put_Owner( GUIGraphicsContext.form.Handle );
      if( hr != 0 ) 
        DirectShowUtil.DebugWrite("mpeg2:FAILED:set Video window");

      hr = m_videoWindow.put_WindowStyle( WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
      if( hr != 0 ) 
        DirectShowUtil.DebugWrite("mpeg2:FAILED:set Video window style");

      hr = m_videoWindow.put_Visible( DsHlp.OATRUE );
      if( hr != 0 ) 
        DirectShowUtil.DebugWrite("mpeg2:FAILED:put_Visible");

      m_mediaControl.Run();
      
      GUIGraphicsContext.OnVideoWindowChanged +=new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      m_graphState=State.Viewing;
      GUIGraphicsContext_OnVideoWindowChanged();
      return true;
    }


    /// <summary>
    /// Stops viewing the TV channel 
    /// </summary>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be viewing first with StartViewing()
    /// </remarks>
    public bool StopViewing()
    {
      if (m_graphState!=State.Viewing) return false;
       
      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      DirectShowUtil.DebugWrite("SWGraph:StopViewing()");
      m_videoWindow.put_Visible( DsHlp.OAFALSE);
      m_mediaControl.Stop();
      m_graphState=State.Created;
      DeleteGraph();
      return true;
    }

    /// <summary>
    /// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
    /// </summary>
    private void GUIGraphicsContext_OnVideoWindowChanged()
    {
      if (m_graphState!=State.Viewing) return ;
      int iVideoWidth,iVideoHeight;
      m_basicVideo.GetVideoSize( out iVideoWidth, out iVideoHeight );
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        float x=GUIGraphicsContext.OverScanLeft;
        float y=GUIGraphicsContext.OverScanTop;
        int  nw=GUIGraphicsContext.OverScanWidth;
        int  nh=GUIGraphicsContext.OverScanHeight;
        if (nw <=0 || nh <=0) return;


        System.Drawing.Rectangle rSource,rDest;
        MediaPortal.GUI.Library.Geometry m_geometry=new MediaPortal.GUI.Library.Geometry();
        m_geometry.ImageWidth=iVideoWidth;
        m_geometry.ImageHeight=iVideoHeight;
        m_geometry.ScreenWidth=nw;
        m_geometry.ScreenHeight=nh;
        m_geometry.ARType=GUIGraphicsContext.ARType;
        m_geometry.PixelRatio=GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;

        m_basicVideo.SetSourcePosition(rSource.Left,rSource.Top,rSource.Width,rSource.Height);
        m_basicVideo.SetDestinationPosition(0,0,rDest.Width,rDest.Height );
        m_videoWindow.SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
      }
      else
      {
        m_basicVideo.SetSourcePosition( 0,0,iVideoWidth,iVideoHeight);
        m_basicVideo.SetDestinationPosition(0,0,GUIGraphicsContext.VideoWindow.Width,GUIGraphicsContext.VideoWindow.Height);
        m_videoWindow.SetWindowPosition( GUIGraphicsContext.VideoWindow.Left,GUIGraphicsContext.VideoWindow.Top,GUIGraphicsContext.VideoWindow.Width,GUIGraphicsContext.VideoWindow.Height);
      }
    }
	}
}
