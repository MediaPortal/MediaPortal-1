using System;
using System.Drawing;
using System.Runtime.InteropServices; 
using DShowNET;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using DirectX.Capture;

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
    protected enum State
    { 
      None,
      Created,
      TimeShifting,
      Recording,
      Viewing
    };
    protected int                     m_cardID=-1;
    protected string                  m_strVideoCaptureFilter="";
#if (UseCaptureCardDefinitions)
		protected TVCaptureDevice         mCard;
		protected string									m_strVideoCaptureMoniker="";
#endif
    protected IGraphBuilder           m_graphBuilder=null;
    protected ICaptureGraphBuilder2   m_captureGraphBuilder=null;
    protected IBaseFilter             m_captureFilter=null;
    protected IAMTVTuner              m_TVTuner=null;
    protected IAMAnalogVideoDecoder   m_IAMAnalogVideoDecoder=null;
		protected VideoCaptureDevice      m_videoCaptureDevice=null;
    protected MPEG2Demux              m_mpeg2Demux=null;
    protected int				              m_rotCookie = 0;						// Cookie into the Running Object Table
    protected State                   m_graphState=State.None;
    protected int                     m_iChannelNr=-1;
    protected int                     m_iCountryCode=31;
    protected bool                    m_bUseCable=false;
    protected DateTime                m_StartTime=DateTime.Now;
    protected int                     m_iPrevChannel=-1;
    protected bool                    m_bFirstTune=true;
    protected Size                    m_FrameSize;
    protected double                  m_FrameRate;
    protected IAMVideoProcAmp         m_videoprocamp=null;
    protected VideoProcAmp            m_videoAmp=null;
		protected VMR9Util							  Vmr9=null; 



    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="iCountryCode">country code</param>
    /// <param name="bCable">use Cable or antenna</param>
    /// <param name="strVideoCaptureFilter">Filter name of the capture device</param>
		public SinkGraph(int ID,int iCountryCode,bool bCable,string strVideoCaptureFilter, Size frameSize, double frameRate)
		{
      m_cardID=ID;
      m_bFirstTune=true;
      m_bUseCable=bCable;
      m_iCountryCode=iCountryCode;
      m_graphState=State.None;
			// #MW# Mistake???
			//#if (UseCaptureCardDefinitions)
      m_strVideoCaptureFilter=strVideoCaptureFilter;
			//#endif
      m_FrameSize = frameSize;
      m_FrameRate = frameRate;

      if (m_FrameSize.Width==0 || m_FrameSize.Height==0)
        m_FrameSize=new Size(720,576);

      
      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm = Registry.LocalMachine;
        hklm.CreateSubKey(@"Software\MediaPortal");

      }
      catch(Exception){}
		}

#if (UseCaptureCardDefinitions)
		/// <summary>
		/// #MW# Added simple call while passing card object
		/// Easier to handle and to extent...
		/// </summary>
		/// <param name="pCard"></param>
		public SinkGraph(TVCaptureDevice pCard)
		{
			mCard                    = pCard;

			// Add legacy code to be compliant to other call, ie fill in membervariables...
			m_bFirstTune             = true;
			m_graphState             = State.None;
			m_cardID                 = mCard.ID;
			m_bUseCable              = mCard.IsCableInput;
			m_iCountryCode           = mCard.CountryCode;
			m_strVideoCaptureFilter  = mCard.VideoDevice;
			m_strVideoCaptureMoniker = mCard.VideoDeviceMoniker;
			m_FrameSize              = mCard.FrameSize;
			m_FrameRate							 = mCard.FrameRate;

			if (m_FrameSize.Width==0 || m_FrameSize.Height==0)
				m_FrameSize = new Size(720,576);

			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");
			}
			catch(Exception){}
		}
#endif

#if (UseCaptureCardDefinitions)
		/// <summary>
		/// #MW#, Added moniker name... ie the REAL device!!!
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="iCountryCode"></param>
		/// <param name="bCable"></param>
		/// <param name="strVideoCaptureFilter"></param>
		/// <param name="strVideoCaptureMoniker"></param>
		/// <param name="frameSize"></param>
		/// <param name="frameRate"></param>
		public SinkGraph(int ID,int iCountryCode,bool bCable,string strVideoCaptureFilter, string strVideoCaptureMoniker, Size frameSize, double frameRate)
		{
			m_cardID=ID;
			m_bFirstTune=true;
			m_bUseCable=bCable;
			m_iCountryCode=iCountryCode;
			m_graphState=State.None;
			m_strVideoCaptureFilter=strVideoCaptureFilter;
			// #MW#
			m_strVideoCaptureMoniker=strVideoCaptureMoniker;
			m_FrameSize = frameSize;
			m_FrameRate = frameRate;

			if (m_FrameSize.Width==0 || m_FrameSize.Height==0)
				m_FrameSize=new Size(720,576);

      
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");

			}
			catch(Exception){}
		}
#endif
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public virtual bool CreateGraph()
    {
      if (m_graphState!=State.None) return false;
			Vmr9 =new VMR9Util("mytv");
			DirectShowUtil.DebugWrite("SinkGraph:CreateGraph()");
      GUIGraphicsContext.OnGammaContrastBrightnessChanged +=new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);
      m_iPrevChannel=-1;
      m_bFirstTune=true;
      DirectShowUtil.DebugWrite("SinkGraph:CreateGraph()");
      int hr=0;
      Filters filters = new Filters();
			DShowNET.Filter                  videoCaptureDeviceFilter=null;
      foreach (DShowNET.Filter filter in filters.VideoInputDevices)
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
        if( hr != 0 ) 
        {
          DirectShowUtil.DebugWrite("SinkGraph:FAILED:Add Videodevice to filtergraph :0x{0:X}",hr);
          return false;
        }
      }
      else
      {
        DirectShowUtil.DebugWrite("SinkGraph:FAILED:Unable to create video capture device:{0{", videoCaptureDeviceFilter.Name);
        return false;
      }

      // Retrieve the stream control interface for the video device
      // FindInterface will also add any required filters
      // (WDM devices in particular may need additional
      // upstream filters to function).
      DirectShowUtil.DebugWrite("SinkGraph:get Video stream control interface (IAMStreamConfig)");
      object o;
      Guid cat ;
      Guid iid ;

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
      if (hr!=0||m_TVTuner==null)
      {
        DirectShowUtil.DebugWrite("SinkGraph:CreateGraph() FAILED:no tuner found :0x{0:X}",hr);
      }
			else
				DirectShowUtil.DebugWrite("SinkGraph:CreateGraph() TV tuner found");

      // For some reason, it happens alot that the capture card can NOT be connected (pin 656 for the
      // PRV150MCE) to the encoder because for some reason the videostandard is GONE...
      // So fetch the standard from the TvTuner and define it for the capture card.

      if (m_TVTuner!=null )
      {
          InitializeTuner();
          m_IAMAnalogVideoDecoder = m_captureFilter as IAMAnalogVideoDecoder;
          if (m_IAMAnalogVideoDecoder!=null)
          {
            AnalogVideoStandard videoStandard;
            m_TVTuner.get_TVFormat(out videoStandard);
            SetVideoStandard(videoStandard);
          }
      }
#if DEBUG
      VideoCaptureProperties props = new VideoCaptureProperties(m_captureFilter);
      if (props.SupportsProperties)
      {
        VideoCaptureProperties.versionInfo info = props.VersionInfo;
        VideoCaptureProperties.videoBitRate bitrate = props.VideoBitRate;
        Log.Write(" driver version:{0} fw version:{1}", info.DriverVersion,info.FWVersion);
        Log.Write(" encoding:{0} bitrate:{1} peak:{2}", bitrate.bEncodingMode.ToString(),
          ((float)bitrate.wBitrate)/400.0f,bitrate.dwPeak);
        Log.Write(" gopsize:{0} closedgop:{1} invtelecine:{2} format:{3} size:{4}x{5} output:{6}",
          props.GopSize, props.ClosedGop, props.InverseTelecine,
          props.VideoFormat.ToString(), props.VideoResolution.Width,props.VideoResolution.Height,
          props.StreamOutput.ToString());
      }
#endif
      m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder,m_captureGraphBuilder, m_captureFilter);
      

      m_FrameSize=m_videoCaptureDevice.GetFrameSize();

      DirectShowUtil.DebugWrite("SinkGraph:capturing:{0}x{1}",m_FrameSize.Width,m_FrameSize.Height);
      m_mpeg2Demux=null;
      if (m_videoCaptureDevice.MPEG2)
      {
        m_mpeg2Demux = new MPEG2Demux(ref m_graphBuilder,m_FrameSize);
      }


      

      // Retreive the media control interface (for starting/stopping graph)
      ConnectVideoCaptureToMPEG2Demuxer();
      m_mpeg2Demux.CreateMappings();
      m_videoprocamp=m_captureFilter as IAMVideoProcAmp;
      if (m_videoprocamp!=null)
      {
        m_videoAmp=new VideoProcAmp(m_videoprocamp);
        GUIGraphicsContext.Brightness= m_videoAmp.Brightness  ;
        GUIGraphicsContext.Contrast  = m_videoAmp.Contrast ;
        GUIGraphicsContext.Gamma     = m_videoAmp.Gamma ;
        GUIGraphicsContext.Saturation= m_videoAmp.Saturation;
        GUIGraphicsContext.Sharpness = m_videoAmp.Sharpness;

      }
      m_graphState=State.Created;
      return true;
    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
#if (UseCaptureCardDefinitions)
		public virtual void DeleteGraph()
#else
		public void DeleteGraph()
#endif
    {
      if (m_graphState < State.Created) return;

      m_iPrevChannel=-1;
      DirectShowUtil.DebugWrite("SinkGraph:DeleteGraph()");
      StopRecording();
      StopTimeShifting();
      StopViewing();

      GUIGraphicsContext.OnGammaContrastBrightnessChanged -=new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);

      if (Vmr9!=null)
      {
        Vmr9.RemoveVMR9();
        Vmr9=null;
      }

      if (m_videoprocamp!=null)
      {
        m_videoAmp=null;
        m_videoprocamp=null;
      }
      if (m_mpeg2Demux!=null)
      {
        m_mpeg2Demux.CloseInterfaces();
        m_mpeg2Demux=null;
      }

      if (m_videoCaptureDevice!=null)
			{
				m_videoCaptureDevice.CloseInterfaces();
				m_videoCaptureDevice=null;
      }


      if( m_TVTuner != null )
        Marshal.ReleaseComObject( m_TVTuner ); m_TVTuner = null;
			
      if (m_IAMAnalogVideoDecoder!=null)
        Marshal.ReleaseComObject( m_IAMAnalogVideoDecoder ); m_IAMAnalogVideoDecoder = null;

      DsUtils.RemoveFilters(m_graphBuilder);


      if( m_captureFilter != null )
        Marshal.ReleaseComObject( m_captureFilter ); m_captureFilter = null;

			if( m_rotCookie != 0 )
				DsROT.RemoveGraphFromRot( ref m_rotCookie);
			m_rotCookie=0;


      if( m_captureGraphBuilder != null )
        Marshal.ReleaseComObject( m_captureGraphBuilder ); m_captureGraphBuilder = null;
	
      if( m_graphBuilder != null )
        Marshal.ReleaseComObject( m_graphBuilder ); m_graphBuilder = null;

      GUIGraphicsContext.form.Invalidate(true);
      GC.Collect();

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
    public bool StartTimeShifting(int country, AnalogVideoStandard standard,int iChannelNr, string strFileName)
    {
      if (m_graphState!=State.Created && m_graphState!= State.TimeShifting) return false;
      if (m_mpeg2Demux==null) return false;
			m_iCountryCode=country;

      if (m_graphState==State.TimeShifting) 
      {
        if (iChannelNr!=m_iChannelNr)
        {
          TuneChannel(standard, iChannelNr,m_iCountryCode);
        }
        return true;
      }

			DirectShowUtil.DebugWrite("SinkGraph:StartTimeShifting()");
      m_graphState=State.TimeShifting;
      TuneChannel(standard,iChannelNr, m_iCountryCode);
      m_mpeg2Demux.StartTimeshifting(strFileName);
      
      return true;
    }

    /// <summary>
    /// Connects the videocapture->MPEG2 Demuxer
    /// </summary>
    protected void ConnectVideoCaptureToMPEG2Demuxer()
    {
      if (m_videoCaptureDevice==null) return;
      if (m_mpeg2Demux.IsRendered) return;
			DirectShowUtil.DebugWrite("SinkGraph:Connect VideoCapture device to MPEG2Demuxer filter");

      // connect video capture pin->mpeg2 demux input
      if (!m_videoCaptureDevice.IsMCEDevice)
      {
        Guid cat = PinCategory.Capture;
        int hr=m_captureGraphBuilder.RenderStream( new Guid[1]{ cat}, null/*new Guid[1]{ med}*/, m_captureFilter, null, m_mpeg2Demux.BaseFilter); 
        if (hr==0)
        {
          return;
        }
      }
      DirectShowUtil.DebugWrite("SinkGraph:find MPEG2 demuxer input pin");
      IPin pinIn=DirectShowUtil.FindPinNr(m_mpeg2Demux.BaseFilter,PinDirection.Input,0);
      if (pinIn!=null) 
      {
        DirectShowUtil.DebugWrite("SinkGraph:found MPEG2 demuxer input pin");
        int hr=m_graphBuilder.Connect(m_videoCaptureDevice.CapturePin, pinIn);
        if (hr==0)
          DirectShowUtil.DebugWrite("SinkGraph:connected video capture out->MPEG2 demuxer input");
        else
          DirectShowUtil.DebugWrite("SinkGraph:FAILED to connect Encoder->mpeg2 demuxer:{0:x}",hr);
      }
      else
        DirectShowUtil.DebugWrite("SinkGraph:FAILED could not find mpeg2 demux input pin");
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
    public bool StartRecording(int country,AnalogVideoStandard standard,int iChannelNr, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if (m_graphState != State.TimeShifting) return false;

			m_iCountryCode=country;
      if (iChannelNr != m_iChannelNr)
      {
        TuneChannel(standard,iChannelNr,m_iCountryCode);
      }
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
    /// This method can be used to ask the graph if it should be rebuild when
    /// we want to tune to the new channel:ichannel
    /// </summary>
    /// <param name="iChannel">new channel to tune to</param>
    /// <returns>true : graph needs to be rebuild for this channel
    ///          false: graph does not need to be rebuild for this channel
    /// </returns>
    public bool ShouldRebuildGraph(int iChannel)
    {
      // if we switch from tuner <-> SVHS/Composite then 
      // we need to rebuild the capture graph
      bool bFixCrossbar=true;
      if (m_iPrevChannel>=0)
      {
        // tuner : channel < 1000
        // SVHS/composite : channel >=1000
        if (m_iPrevChannel< 1000 && iChannel < 1000) bFixCrossbar=false;
        if (m_iPrevChannel==1000 && iChannel ==1000) bFixCrossbar=false;
        if (m_iPrevChannel==1001 && iChannel ==1001) bFixCrossbar=false;
        if (m_iPrevChannel==1002 && iChannel ==1002) bFixCrossbar=false;
      }
      else bFixCrossbar=false;
      return bFixCrossbar;
    }

    /// <summary>
    /// Switches / tunes to another TV channel
    /// </summary>
    /// <param name="iChannel">New channel</param>
    /// <remarks>
    /// Graph should be timeshifting. 
    /// </remarks>
    public void TuneChannel(AnalogVideoStandard standard,int iChannel, int country)
    {
      if (m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return;

      m_iChannelNr=iChannel;
			m_iCountryCode=country;
    
      DirectShowUtil.DebugWrite("SinkGraph:TuneChannel() tune to channel:{0}", iChannel);
      if (iChannel <1000)
      {
        if (m_TVTuner==null) return;
        if (m_bFirstTune)
        {
          m_bFirstTune=false;
          InitializeTuner();
          SetVideoStandard(standard);
          
          
          DirectShowUtil.DebugWrite("SinkGraph:TuneChannel() tuningspace:0 country:{0} tv standard:{1} cable:{2}",
            m_iCountryCode,standard.ToString(),
            m_bUseCable);
        }
        try
        {
          int iCurrentChannel,iVideoSubChannel,iAudioSubChannel;
          SetVideoStandard(standard);
          m_TVTuner.get_TVFormat(out standard);
          m_TVTuner.get_Channel(out iCurrentChannel, out iVideoSubChannel, out iAudioSubChannel);
          if (iCurrentChannel!=iChannel)
          {
            m_TVTuner.put_Channel(iChannel,DShowNET.AMTunerSubChannel.Default,DShowNET.AMTunerSubChannel.Default);
          }
          int iFreq;
          double dFreq;
          m_TVTuner.get_VideoFrequency(out iFreq);
          dFreq=iFreq/1000000d;
          DirectShowUtil.DebugWrite("SinkGraph:TuneChannel() tuned to {0} MHz. tvformat:{1}", dFreq,standard.ToString());
        }
        catch(Exception){} 
      }
      else
      {
        SetVideoStandard(standard);
      }

      bool bFixCrossbar=true;
      if (m_iPrevChannel>=0)
      {
        if (m_iPrevChannel< 1000 && iChannel < 1000) bFixCrossbar=false;
        if (m_iPrevChannel==1000 && iChannel ==1000) bFixCrossbar=false;
        if (m_iPrevChannel==1001 && iChannel ==1001) bFixCrossbar=false;
        if (m_iPrevChannel==1002 && iChannel ==1002) bFixCrossbar=false;
      }
      if (bFixCrossbar)
      {
        DsUtils.FixCrossbarRouting(m_graphBuilder,m_captureGraphBuilder,m_captureFilter, iChannel<1000, (iChannel==1001), (iChannel==1002), (iChannel==1000) ,true);
      }
      m_iPrevChannel=iChannel;
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
		public bool StartViewing(AnalogVideoStandard standard,  int iChannelNr,int country)
		{

			DirectShowUtil.DebugWrite("SinkGraph:StartViewing()");
			if (m_graphState!=State.Created && m_graphState!=State.Viewing) return false;

			m_iCountryCode=country;
			if (m_mpeg2Demux==null) return false;
			if (m_videoCaptureDevice==null) return false;

			if (m_graphState==State.Viewing) 
			{
				if (iChannelNr!=m_iChannelNr)
				{
					TuneChannel(standard, iChannelNr,country);
				}
				return true;
			}

			Vmr9.AddVMR9(m_graphBuilder);

			AddPreferredCodecs();
      
#if (!UseCaptureCardDefinitions)
			// #MW# Next call is already done while creating graph, so obsolete?!?!
			ConnectVideoCaptureToMPEG2Demuxer();
#endif
			m_graphState=State.Viewing;
			TuneChannel(standard, iChannelNr, m_iCountryCode);
				m_mpeg2Demux.StartViewing(GUIGraphicsContext.form.Handle, Vmr9.UseVMR9inMYTV);

      
			DirectShowUtil.EnableDeInterlace(m_graphBuilder);

			GUIGraphicsContext.OnVideoWindowChanged +=new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			GUIGraphicsContext_OnVideoWindowChanged();

      
			int iVideoWidth,iVideoHeight;
			if (!Vmr9.UseVMR9inMYTV)
			{
				m_mpeg2Demux.GetVideoSize( out iVideoWidth, out iVideoHeight );
			}
			else
			{
				iVideoWidth=Vmr9.VideoWidth;
				iVideoHeight=Vmr9.VideoHeight;
			}
      DirectShowUtil.DebugWrite("SinkGraph:StartViewing() started {0}x{1}",iVideoWidth, iVideoHeight);
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
      if (m_graphState!=State.Viewing && m_graphState!=State.TimeShifting) return ;
      if (m_mpeg2Demux==null) return ;
			
			if (!Vmr9.UseVMR9inMYTV)
			{
				if (GUIGraphicsContext.Overlay==false)
				{
					if (m_graphState!=State.Viewing)
					{
						m_mpeg2Demux.Overlay=false;
						return;
					}
				}
				else
				{
					m_mpeg2Demux.Overlay=true;
				}
			}
      int iVideoWidth,iVideoHeight;
			if (!Vmr9.UseVMR9inMYTV)
			{
				m_mpeg2Demux.GetVideoSize( out iVideoWidth, out iVideoHeight );
			}
			else
			{
				iVideoWidth=Vmr9.VideoWidth;
				iVideoHeight=Vmr9.VideoHeight;
			}
      if (GUIGraphicsContext.IsFullScreenVideo|| false==GUIGraphicsContext.ShowBackground)
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

        m_mpeg2Demux.SetSourcePosition( rSource.Left,rSource.Top,rSource.Width,rSource.Height);
        m_mpeg2Demux.SetDestinationPosition(0,0,rDest.Width,rDest.Height );
        m_mpeg2Demux.SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
      }
      else
      {
        if (iVideoWidth>0 && iVideoHeight>0)
          m_mpeg2Demux.SetSourcePosition(0,0,iVideoWidth,iVideoHeight);
        
        if (GUIGraphicsContext.VideoWindow.Width>0 && GUIGraphicsContext.VideoWindow.Height>0)
          m_mpeg2Demux.SetDestinationPosition(0,0,GUIGraphicsContext.VideoWindow.Width,GUIGraphicsContext.VideoWindow.Height);
        
        if (GUIGraphicsContext.VideoWindow.Width>0 && GUIGraphicsContext.VideoWindow.Height>0)
          m_mpeg2Demux.SetWindowPosition( GUIGraphicsContext.VideoWindow.Left,GUIGraphicsContext.VideoWindow.Top,GUIGraphicsContext.VideoWindow.Width,GUIGraphicsContext.VideoWindow.Height);
      }
    }

 
		/// <summary>
		/// Add preferred mpeg2 audio/video codecs to the graph
		/// and if wanted add ffdshow postprocessing to the graph
		/// </summary>
    void AddPreferredCodecs()
    {				
      // add preferred video & audio codecs
      string strVideoCodec="";
      string strAudioCodec="";
      bool   bAddFFDshow=false;
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
        strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
        strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
      }
      if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strVideoCodec);
      if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strAudioCodec);
      if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(m_graphBuilder,"ffdshow raw video filter");
    }
    

		/// <summary>
		/// Returns whether the tvtuner is tuned to a tv channel or not
		/// </summary>
		/// <returns>
		/// true: tvtuner is tuned to a tv channel
		/// false: tvtuner is not tuner to a tv channel 
		/// </returns>
    public bool SignalPresent()
    {
      if (m_graphState!=State.Recording && m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return false;
      if (m_TVTuner==null) return true;
      AMTunerSignalStrength strength;
      m_TVTuner.SignalPresent(out strength);
      return strength==AMTunerSignalStrength.SignalPresent;
    }

		/// <summary>
		/// Return video frequency in Hz of current tv channel
		/// </summary>
		/// <returns>video frequency in Hz </returns>
    public long VideoFrequency()
    {
      
      if (m_graphState!=State.Recording && m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return 0;
      if (m_TVTuner==null) return 0;
      int lFreq;
      m_TVTuner.get_VideoFrequency(out lFreq);
      return lFreq;
    }

		/// <summary>
		/// Callback from GUIGraphicsContext
		/// Will be called when the user changed the gamma, brightness,contrast settings
		/// This method takes the new settings and applies them to the video amplifier
		/// </summary>
    protected void OnGammaContrastBrightnessChanged()
    {
      if (m_graphState!=State.Recording && m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return ;
      if (m_videoprocamp==null) return;
      if (m_videoAmp==null) return;

      
      //set the changed values
      m_videoAmp.Brightness = GUIGraphicsContext.Brightness;
      m_videoAmp.Contrast = GUIGraphicsContext.Contrast;
      m_videoAmp.Gamma = GUIGraphicsContext.Gamma;
      m_videoAmp.Saturation=GUIGraphicsContext.Saturation;
      m_videoAmp.Sharpness=GUIGraphicsContext.Sharpness;

      //get back the changed values
      GUIGraphicsContext.Brightness= m_videoAmp.Brightness  ;
      GUIGraphicsContext.Contrast  = m_videoAmp.Contrast ;
      GUIGraphicsContext.Gamma     = m_videoAmp.Gamma ;
      GUIGraphicsContext.Saturation= m_videoAmp.Saturation;
      GUIGraphicsContext.Sharpness = m_videoAmp.Sharpness;
    }

		/// <summary>
		/// Sets the tv standard used (pal,ntsc,secam,...) for the video decoder
		/// </summary>
		/// <param name="standard">TVStandard</param>
    protected void SetVideoStandard(AnalogVideoStandard standard)
    {
      if (standard==AnalogVideoStandard.None) return;
      if (m_IAMAnalogVideoDecoder==null) return;
      AnalogVideoStandard currentStandard;
      int hr=m_IAMAnalogVideoDecoder.get_TVFormat(out currentStandard);
      if (currentStandard==standard) return;

      DirectShowUtil.DebugWrite("SinkGraph:Select tvformat:{0}", standard.ToString());
      if (standard==AnalogVideoStandard.None) standard=AnalogVideoStandard.PAL_B;
      hr=m_IAMAnalogVideoDecoder.put_TVFormat(standard);
      if (hr!=0) DirectShowUtil.DebugWrite("SinkGraph:Unable to select tvformat:{0}", standard.ToString());
    }

		/// <summary>
		/// Initializes the TV tuner with the 
		///		- tuning space
		///		- country (depends on m_iCountryCode)
		///		- Sets tuner mode to TV
		///		- Sets tuner to cable or antenna (depends on m_bUseCable)
		/// </summary>
    protected void InitializeTuner()
    {
      if (m_TVTuner==null) return;
      int iTuningSpace,iCountry;
      DShowNET.AMTunerModeType mode;

      m_TVTuner.get_TuningSpace(out iTuningSpace);
      if (iTuningSpace!=0) m_TVTuner.put_TuningSpace(0);

      m_TVTuner.get_CountryCode(out iCountry);
      if (iCountry!=m_iCountryCode)
        m_TVTuner.put_CountryCode(m_iCountryCode);

      m_TVTuner.get_Mode(out mode);
      if (mode!=DShowNET.AMTunerModeType.TV)
        m_TVTuner.put_Mode(DShowNET.AMTunerModeType.TV);

      DShowNET.TunerInputType inputType;
      m_TVTuner.get_InputType(0, out inputType);
      if (m_bUseCable)
      {
        if (inputType!=DShowNET.TunerInputType.Cable)
          m_TVTuner.put_InputType(0,DShowNET.TunerInputType.Cable);
      }
      else
      {
        if (inputType!=DShowNET.TunerInputType.Antenna)
          m_TVTuner.put_InputType(0,DShowNET.TunerInputType.Antenna);
      }
    }
		
		public void Process()
		{
		}
		
		public PropertyPageCollection PropertyPages()
		{
			
			PropertyPageCollection propertyPages=null;
			{
				try 
				{ 
					SourceCollection VideoSources = new SourceCollection( m_captureGraphBuilder, m_captureFilter, true );

					// #MW#, difficult to say if this must be changed, as it depends on the loaded
					// filters. The list below is fixed list however... So???
					propertyPages = new PropertyPageCollection( m_captureGraphBuilder, 
						m_captureFilter, null, 
						null, null, 
						VideoSources, null, (IBaseFilter)m_TVTuner );

				}
				catch ( Exception ex ) { Log.Write( "PropertyPages: FAILED to get property pages." + ex.ToString() ); }

				return( propertyPages );
			}
		}
		
		public IBaseFilter AudiodeviceFilter()
		{
			return null;
		}

		public bool SupportsFrameSize(Size framesize)
		{	
			return false;
		}
		
		public NetworkType Network()
		{
				return NetworkType.ATSC;
		}
		public void Tune(object tuningObject)
		{
		}
  }
}
