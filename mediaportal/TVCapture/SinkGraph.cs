using System;
using System.Runtime.InteropServices; 
using DShowNET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
	public class SinkGraph
	{
    enum State
    { 
      None,
      Created,
      TimeShifting,
      Recording,
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
    protected int				    m_rotCookie = 0;						// Cookie into the Running Object Table
    State                   m_graphState=State.None;
    int                     m_iChannelNr=-1;
    int                     m_iCountryCode=31;
    bool                    m_bUseCable=false;
    DateTime                m_StartTime=DateTime.Now;

		public SinkGraph(int iCountryCode,bool bCable,string strVideoCaptureFilter)
		{
      m_bUseCable=bCable;
      m_iCountryCode=iCountryCode;
      m_graphState=State.None;
      m_strVideoCaptureFilter=strVideoCaptureFilter;
		}

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
        if (m_TVTuner!=null)
        {
          bool bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)m_TVTuner);
          if (!bAllConnected)
          {
            FixAverMediaBug();
          }
        }
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

    public void DeleteGraph()
    {
      if (m_graphState < State.Created) return;

      DirectShowUtil.DebugWrite("SinkGraph:DeleteGraph()");
      StopRecording();
      StopTimeShifting();

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
				//m_videoCaptureDevice.CloseInterfaces();
				m_videoCaptureDevice=null;
			}
			DsUtils.RemoveFilters(m_graphBuilder);

			//if( m_videoStreamConfig != null )
			//	Marshal.ReleaseComObject( m_videoStreamConfig ); m_videoStreamConfig = null;

			//if( m_videoprocamp != null )
			//	Marshal.ReleaseComObject( m_videoprocamp ); m_videoprocamp = null;


			if( m_TVTuner != null )
				Marshal.ReleaseComObject( m_TVTuner ); m_TVTuner = null;

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

    public bool StartTimeShifting(int iChannelNr, string strFileName)
    {
      if (m_graphState!=State.Created) return false;
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
      TuneChannel(iChannelNr);
      m_mpeg2Demux.StartTimeshifting(strFileName);
      
      m_graphState=State.TimeShifting;
      return true;
    }

    public bool StopTimeShifting()
    {
      if ( m_graphState!=State.TimeShifting) return false;

			DirectShowUtil.DebugWrite("SinkGraph:StopTimeShifting()");
			m_mpeg2Demux.StopTimeShifting();
      m_graphState=State.Created;

      return true;
    }

    public bool StartRecording(string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if ( m_graphState!=State.TimeShifting) return false;

      DirectShowUtil.DebugWrite("SinkGraph:StartRecording({0} {1})",strFileName,bContentRecording);
      m_mpeg2Demux.Record(strFileName, bContentRecording, timeProgStart,m_StartTime);
      m_graphState=State.Recording;
      return true;
    }

    public void StopRecording()
    {
      if ( m_graphState!=State.Recording) return;

      DirectShowUtil.DebugWrite("SinkGraph:StopRecording()");
      m_mpeg2Demux.StopRecording();
      m_graphState=State.TimeShifting;
    }

		public int ChannelNumber
		{
			get { return m_iChannelNr;}
		}

    public void TuneChannel(int iChannel)
    {
      m_iChannelNr=iChannel;
      if (m_TVTuner==null) return;

			DirectShowUtil.DebugWrite("SinkGraph:TuneChannel() tune to channel:{0}", iChannel);
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

			DsUtils.FixCrossbarRouting(m_captureGraphBuilder,m_captureFilter, iChannel<1000, (iChannel==1001), (iChannel==1002), (iChannel==1000) );
      m_StartTime=DateTime.Now;
    }

    void FixAverMediaBug()
    {
/*
      // AverMedia MCE card has a bug. It will only connect the TV Tuner->crossbar if
      // the crossbar outputs are disconnected
      DirectShowUtil.DebugWrite("Sinkgraph:FixAverMediaBug()");
      //find crossbar
      int  hr;
      Guid cat;
      Guid iid;
      object o=null;
      cat = FindDirection.UpstreamOnly;
      iid = typeof(IAMCrossbar).GUID;
      hr=m_captureGraphBuilder.FindInterface(new Guid[1]{cat},null,m_captureFilter, ref iid, out o);
      if (hr !=0 || o == null) 
      {
        DirectShowUtil.DebugWrite("Sinkgraph:no crossbar found");
        return; // no crossbar found?
      }
        
      IAMCrossbar crossbar = o as IAMCrossbar;
      if (crossbar ==null) 
      {
        DirectShowUtil.DebugWrite("Sinkgraph:no crossbar found");
        return;
      }


      //disconnect the output pins of the crossbar
      DirectShowUtil.DebugWrite("Sinkgraph:disconnect crossbar outputs");
      DirectShowUtil.DisconnectOutputPins(m_graphBuilder,(IBaseFilter)m_TVTuner);
      DirectShowUtil.DisconnectOutputPins(m_graphBuilder,(IBaseFilter)crossbar);

      //render the output pins of the tvtuner
      DirectShowUtil.DebugWrite("Sinkgraph:connect tvtuner outputs");
      bool bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)m_TVTuner);
      if (bAllConnected)
        DirectShowUtil.DebugWrite("Sinkgraph:all connected");
      else
        DirectShowUtil.DebugWrite("Sinkgraph:FAILED, not all pins connected");

      //reconnect the output pins of the crossbar
      DirectShowUtil.DebugWrite("Sinkgraph:reconnect crossbar output pins");
    
      bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)crossbar);
      if (bAllConnected)
        DirectShowUtil.DebugWrite("Sinkgraph:all connected");
      else
        DirectShowUtil.DebugWrite("Sinkgraph:FAILED, not all pins connected");

   */
    } 
	}
}
