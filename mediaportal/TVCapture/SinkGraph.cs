using System;
using System.Drawing;
using System.Runtime.InteropServices; 
using DShowNET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Implementation of IGraph for cards with an onboard MPEG 2 encoder
  /// like the Hauppauge PVR 250/350/USB2/MCE
  /// A graphbuilder object supports one or more TVCapture cards and
  /// contains all the code/logic necessary todo
  /// -tv viewing
  /// -tv recording
  /// -tv timeshifting
  /// </summary>
	public class SinkGraph : IGraph
	{
    enum State
    { 
      None,
      Created,
      TimeShifting,
      Recording,
      Viewing
    };
    string                  m_strVideoCaptureFilter="";
    IGraphBuilder           m_graphBuilder=null;
    ICaptureGraphBuilder2   m_captureGraphBuilder=null;
    IBaseFilter             m_captureFilter=null;
    IAMTVTuner              m_TVTuner=null;
//	IAMStreamConfig         m_videoStreamConfig=null;
//  IAMVideoProcAmp         m_videoprocamp;
//  IMediaControl           m_mediaControl=null;
		VideoCaptureDevice      m_videoCaptureDevice=null;
    MPEG2Demux              m_mpeg2Demux=null;
    int				              m_rotCookie = 0;						// Cookie into the Running Object Table
    State                   m_graphState=State.None;
    int                     m_iChannelNr=-1;
    int                     m_iCountryCode=31;
    bool                    m_bUseCable=false;
    DateTime                m_StartTime=DateTime.Now;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="iCountryCode">country code</param>
    /// <param name="bCable">use Cable or antenna</param>
    /// <param name="strVideoCaptureFilter">Filter name of the capture device</param>
		public SinkGraph(int iCountryCode,bool bCable,string strVideoCaptureFilter)
		{
      m_bUseCable=bCable;
      m_iCountryCode=iCountryCode;
      m_graphState=State.None;
      m_strVideoCaptureFilter=strVideoCaptureFilter;
		}

    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public bool CreateGraph()
    {
      if (m_graphState!=State.None) return false;
      DirectShowUtil.DebugWrite("SinkGraph:CreateGraph()");
      int hr=0;
      Filters filters = new Filters();
			Filter                  videoCaptureDeviceFilter=null;
      foreach (Filter filter in filters.VideoInputDevices)
      {
        if (filter.Name.Equals(m_strVideoCaptureFilter))
        {
          videoCaptureDeviceFilter=filter;
          break;
        }
      }

			if (videoCaptureDeviceFilter==null) 
			{
				DirectShowUtil.DebugWrite("SinkGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
				return false;
			}

      // Make a new filter graph
      DirectShowUtil.DebugWrite("SinkGraph:create new filter graph (IGraphBuilder)");
      m_graphBuilder = (IGraphBuilder) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) ); 

      // Get the Capture Graph Builder
      DirectShowUtil.DebugWrite("SinkGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
      Guid clsid = Clsid.CaptureGraphBuilder2;
      Guid riid = typeof(ICaptureGraphBuilder2).GUID;
      m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance( ref clsid, ref riid ); 

      DirectShowUtil.DebugWrite("SinkGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
      hr = m_captureGraphBuilder.SetFiltergraph( m_graphBuilder );
      if( hr < 0 ) 
      {
        DirectShowUtil.DebugWrite("SinkGraph:link FAILED");
        return false;
      }
      DirectShowUtil.DebugWrite("SinkGraph:Add graph to ROT table");
      DsROT.AddGraphToRot( m_graphBuilder, out m_rotCookie );

      // Get the video device and add it to the filter graph
			DirectShowUtil.DebugWrite("SinkGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
      m_captureFilter = Marshal.BindToMoniker( videoCaptureDeviceFilter.MonikerString ) as IBaseFilter;
      if (m_captureFilter!=null)
      {
        hr = m_graphBuilder.AddFilter( m_captureFilter, "Video Capture Device" );
        if( hr < 0 ) 
        {
          DirectShowUtil.DebugWrite("SinkGraph:FAILED:Add Videodevice to filtergraph");
          return false;
        }
      }

      // Retrieve the stream control interface for the video device
      // FindInterface will also add any required filters
      // (WDM devices in particular may need additional
      // upstream filters to function).
      DirectShowUtil.DebugWrite("SinkGraph:get Video stream control interface (IAMStreamConfig)");
      object o;
      Guid cat = PinCategory.Capture;
      Guid iid = typeof(IAMStreamConfig).GUID;
/*
      hr = m_captureGraphBuilder.FindInterface(new Guid[1]{cat}, null, m_captureFilter, ref iid, out o );
      if (hr==0)
      {
        m_videoStreamConfig = o as IAMStreamConfig;
      }
*/
      // Retrieve TV Tuner if available
      DirectShowUtil.DebugWrite("SinkGraph:Find TV Tuner");
      o = null;
      cat = FindDirection.UpstreamOnly;
      iid = typeof(IAMTVTuner).GUID;
      hr = m_captureGraphBuilder.FindInterface( new Guid[1]{ cat}, null, m_captureFilter, ref iid, out o );
      if (hr==0) 
      {
        m_TVTuner = o as IAMTVTuner;
      }
			if (m_TVTuner==null)
			{
				DirectShowUtil.DebugWrite("SinkGraph:CreateGraph() FAILED:no tuner found");
			}
      //m_videoprocamp=(IAMVideoProcAmp )m_captureFilter;


      m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder,m_captureGraphBuilder, m_captureFilter);
      m_mpeg2Demux=null;
      if (m_videoCaptureDevice.MPEG2)
      {
        m_mpeg2Demux = new MPEG2Demux(ref m_graphBuilder);
      }


      // Retreive the media control interface (for starting/stopping graph)
      //m_mediaControl = (IMediaControl) m_graphBuilder;

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

      DirectShowUtil.DebugWrite("SinkGraph:DeleteGraph()");
      StopRecording();
      StopTimeShifting();
      StopViewing();

      //if ( m_mediaControl != null )
      //{
      //  m_mediaControl.Stop();
      //}

      if (m_mpeg2Demux!=null)
      {
        m_mpeg2Demux.CloseInterfaces();
        m_mpeg2Demux=null;
      }
      //m_mediaControl=null;

      if (m_videoCaptureDevice!=null)
			{
				m_videoCaptureDevice.CloseInterfaces();
				m_videoCaptureDevice=null;
      }


      if( m_TVTuner != null )
        Marshal.ReleaseComObject( m_TVTuner ); m_TVTuner = null;
			
      DsUtils.RemoveFilters(m_graphBuilder);

			//if( m_videoStreamConfig != null )
			//	Marshal.ReleaseComObject( m_videoStreamConfig ); m_videoStreamConfig = null;

			//if( m_videoprocamp != null )
			//	Marshal.ReleaseComObject( m_videoprocamp ); m_videoprocamp = null;


      if( m_captureFilter != null )
        Marshal.ReleaseComObject( m_captureFilter ); m_captureFilter = null;

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
      if (m_graphState!=State.Created && m_graphState!= State.TimeShifting) return false;
      if (m_mpeg2Demux==null) return false;

      if (m_graphState==State.TimeShifting) 
      {
        if (iChannelNr!=m_iChannelNr)
        {
          TuneChannel(iChannelNr);
        }
        return true;
      }

			DirectShowUtil.DebugWrite("SinkGraph:StartTimeShifting()");
      ConnectEncoder();
      TuneChannel(iChannelNr);
      m_mpeg2Demux.StartTimeshifting(strFileName);
      
      m_graphState=State.TimeShifting;
      return true;
    }

    /// <summary>
    /// Connects the videocapture->WDM Encoder for MCE devices
    /// </summary>
    void ConnectEncoder()
    {
      if (!m_mpeg2Demux.IsRendered) 
      {
        // connect video capture pin->mpeg2 demux input
        if (m_videoCaptureDevice.IsMCEDevice)
        {
          DirectShowUtil.DebugWrite("SinkGraph:find mpeg2 demux input pin");
          IPin pinIn=DirectShowUtil.FindPinNr(m_mpeg2Demux.BaseFilter,PinDirection.Input,0);
          if (pinIn!=null) 
          {
            DirectShowUtil.DebugWrite("SinkGraph:found mpeg2 demux input pin");
            int hr=m_graphBuilder.Connect(m_videoCaptureDevice.CapturePin, pinIn);
            if (hr==0)
              DirectShowUtil.DebugWrite("SinkGraph:connected Encoder->mpeg2 demuxer");
            else
              DirectShowUtil.DebugWrite("SinkGraph:FAILED to connect Encoder->mpeg2 demuxer:{0:x}",hr);
          }
          else
            DirectShowUtil.DebugWrite("SinkGraph:FAILED could not find mpeg2 demux input pin");
        }
        else
        {
          Guid cat = PinCategory.Capture;
          m_captureGraphBuilder.RenderStream( new Guid[1]{ cat}, null/*new Guid[1]{ med}*/, m_captureFilter, null, m_mpeg2Demux.BaseFilter); 
        }
      }
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
      if ( m_graphState!=State.TimeShifting) return false;

			DirectShowUtil.DebugWrite("SinkGraph:StopTimeShifting()");
			m_mpeg2Demux.StopTimeShifting();
      m_graphState=State.Created;

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
      if ( m_graphState!=State.TimeShifting) return false;

      DirectShowUtil.DebugWrite("SinkGraph:StartRecording({0} {1})",strFileName,bContentRecording);
      m_mpeg2Demux.Record(strFileName, bContentRecording, timeProgStart,m_StartTime);
      m_graphState=State.Recording;
      return true;
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
      if ( m_graphState!=State.Recording) return;

      DirectShowUtil.DebugWrite("SinkGraph:StopRecording()");
      m_mpeg2Demux.StopRecording();
      m_graphState=State.TimeShifting;
    }


    /// <summary>
    /// Returns the current tv channel
    /// </summary>
    /// <returns>Current channel</returns>
		public int GetChannelNumber()
		{
			return m_iChannelNr;
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
      m_iChannelNr=iChannel;
      if (m_TVTuner==null) return;

      DirectShowUtil.DebugWrite("SinkGraph:TuneChannel() tune to channel:{0}", iChannel);
      if (iChannel <1000)
      {
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
          DirectShowUtil.DebugWrite("SinkGraph:TuneChannel() tuned to {0} MHz.", dFreq);
        }
        catch(Exception){} 
      }
			DsUtils.FixCrossbarRouting(m_captureGraphBuilder,m_captureFilter, iChannel<1000, (iChannel==1001), (iChannel==1002), (iChannel==1000) );
      m_StartTime=DateTime.Now;
    }


    /// <summary>
    /// Property indiciating if the graph supports timeshifting
    /// </summary>
    /// <returns>boolean in diciating if the graph supports timeshifting</returns>
    public bool SupportsTimeshifting()
    {
      return true;
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
      if (m_graphState!=State.Created && m_graphState!=State.Viewing) return false;
      if (m_mpeg2Demux==null) return false;

      if (m_graphState==State.Viewing) 
      {
        if (iChannelNr!=m_iChannelNr)
        {
          TuneChannel(iChannelNr);
        }
        return true;
      }

      DirectShowUtil.DebugWrite("SinkGraph:StartViewing()");
      ConnectEncoder();
      TuneChannel(iChannelNr);
      m_mpeg2Demux.StartViewing(GUIGraphicsContext.form.Handle);
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
    /// After stopping the graph is deleted
    /// </remarks>
    public bool StopViewing()
    {
      if (m_graphState!=State.Viewing) return false;
       
      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      DirectShowUtil.DebugWrite("SinkGraph:StopViewing()");
      m_mpeg2Demux.StopViewing();
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
      if (m_mpeg2Demux==null) return ;
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        m_mpeg2Demux.SetVideoPosition(new Rectangle(0,0,GUIGraphicsContext.Width,GUIGraphicsContext.Height) );
      }
      else
      {
        m_mpeg2Demux.SetVideoPosition(GUIGraphicsContext.VideoWindow);
      }
    }
  }
}
