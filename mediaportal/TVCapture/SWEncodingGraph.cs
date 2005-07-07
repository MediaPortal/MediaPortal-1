using System;
using Microsoft.Win32;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
using DShowNET;
using DirectX.Capture;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
	public class SWEncodingGraph : MediaPortal.TV.Recording.IGraph
	{
    [ComImport, Guid("2DB47AE5-CF39-43c2-B4D6-0CD8D90946F4")]
      class StreamBufferSink {};

    [ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
      class StreamBufferConfig {}
    
    [DllImport("advapi32", CharSet=CharSet.Auto)]
    private static extern bool ConvertStringSidToSid(
      string pStringSid, ref IntPtr pSID);

    [DllImport("kernel32", CharSet=CharSet.Auto)]
    private static extern IntPtr  LocalFree( IntPtr hMem);

    [DllImport("advapi32", CharSet=CharSet.Auto)]
    private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);
    const int WS_CHILD			= 0x40000000;	
    const int WS_CLIPCHILDREN	= 0x02000000;
    const int WS_CLIPSIBLINGS	= 0x04000000;


    enum State
    { 
      None, 
      Created, 
      TimeShifting,
      Recording, 
      Viewing,
			Radio
    };
    int                     m_cardID=-1;
    int                     m_iCurrentChannel = 28;
    int                     m_iCountryCode = 31;
    bool                    m_bUseCable = false;
    State                   m_graphState = State.None;
    string                  m_strVideoCaptureFilter = "";
    string                  m_strAudioCaptureFilter = "";
    string                  m_strVideoCompressor = "";
    string                  m_strAudioCompressor = "";
    string                  m_strAudioInputPin = "";
    IGraphBuilder           m_graphBuilder = null;
    ICaptureGraphBuilder2   m_captureGraphBuilder = null;
    IBaseFilter             m_filterCaptureVideo = null;
    IBaseFilter             m_filterCaptureAudio = null;
    
    IFileSinkFilter	        m_fileWriterFilter = null; // DShow Filter: file writer
    IBaseFilter		          m_muxFilter = null; // DShow Filter: multiplexor (combine video and audio streams)
    IBaseFilter             m_filterCompressorVideo = null;
    IBaseFilter             m_filterCompressorAudio = null;
    IAMTVTuner              m_TVTuner = null;
    IAMAnalogVideoDecoder   m_IAMAnalogVideoDecoder=null;
    int				              m_rotCookie = 0; // Cookie into the Running Object Table
    VideoCaptureDevice      m_videoCaptureDevice = null;
    IVideoWindow            m_videoWindow = null;
    IBasicVideo2            m_basicVideo = null;
    IMediaControl					  m_mediaControl = null;
    Size                    m_FrameSize;
    double                  m_FrameRate;
    int                     _RecordingLevel=100;
    bool                    m_bFirstTune = true;
    int                     m_iPrevChannel=-1;
    bool                    m_bRecordWMV = false;
    bool                    m_bIsUsingMPEG = false;
    protected IAMVideoProcAmp         m_videoprocamp=null;
    protected VideoProcAmp            m_videoAmp=null;

    //streambuffer interfaces
    IPin                      m_pinAudioOut=null;
    IPin                      m_pinVideoOut=null;

    IBaseFilter                m_streamBuffer=null;
    IPin                       m_pinStreamBufferIn0=null;
    IPin                       m_pinStreamBufferIn1=null;
    IStreamBufferRecordControl m_recControl=null;
    IStreamBufferSink          m_bSink=null;
    IStreamBufferConfigure     m_pConfig=null;
		IAMAudioInputMixer				 m_mixer=null;
    StreamBufferSink           m_StreamBufferSink=null;
    StreamBufferConfig         m_StreamBufferConfig=null;
    bool                       m_bOverlayVisible=false;
		protected VMR9Util							  Vmr9=null; 
		protected VMR7Util							  Vmr7=null; 
		string                     cardName;

		public SWEncodingGraph(int ID,int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel,string friendlyName)
    {
			cardName=friendlyName;
			m_cardID=ID;
      m_bFirstTune = true;
      m_bUseCable = bCable;
      m_iCountryCode = iCountryCode;
      m_graphState = State.None;
      m_strVideoCaptureFilter = strVideoCaptureFilter;
      m_strAudioCaptureFilter = strAudioCaptureFilter;
      m_strVideoCompressor = strVideoCompressor;
      m_strAudioCompressor = strAudioCompressor;
      m_FrameSize = frameSize;
      m_FrameRate = frameRate;
      if (strAudioInputPin != null && strAudioInputPin.Length > 0)
        m_strAudioInputPin = strAudioInputPin;
      _RecordingLevel = RecordingLevel;
      m_bIsUsingMPEG = false;

      
      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm = Registry.LocalMachine;
        hklm.CreateSubKey(@"Software\MediaPortal");

      }
      catch(Exception){}
		}

    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public bool CreateGraph(int Quality)
    {
      if (m_graphState != State.None) return false;
      m_bIsUsingMPEG = false;
			Vmr9 =new VMR9Util("mytv");
			Vmr7 =new VMR7Util();
      
      Filter filterVideoCompressor ;
      Filter filterAudioCompressor;
      GetCompressors(out filterVideoCompressor , out filterAudioCompressor);

      DirectShowUtil.DebugWrite("SWGraph:CreateGraph()");

      // find the video capture device
      m_iPrevChannel=-1;
      m_bFirstTune = true;
      Filters filters = new Filters();
      Filter filterVideoCaptureDevice = null;
      Filter filterAudioCaptureDevice = null;
      foreach (Filter filter in filters.VideoInputDevices)
      {
        if (filter.Name.Equals(m_strVideoCaptureFilter))
        {
          filterVideoCaptureDevice = filter;
          break;
        }
      }
      // find the audio capture device
      if (m_strAudioCaptureFilter.Length > 0)
      {
        foreach (Filter filter in filters.AudioInputDevices)
        {
          if (filter.Name.Equals(m_strAudioCaptureFilter))
          {
            filterAudioCaptureDevice = filter;
            break;
          }
        }
      }

      //no video capture device? then fail
      if (filterVideoCaptureDevice == null) 
      {
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
        return false;
      }
      
      //no audio capture device? then fail if there should be one
      if (filterAudioCaptureDevice == null && m_strAudioCaptureFilter.Length > 0) 
      {
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strAudioCaptureFilter);
        return false;
      }

      // Make a new filter graph
      DirectShowUtil.DebugWrite("SWGraph:create new filter graph (IGraphBuilder)");
      m_graphBuilder = (IGraphBuilder) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true));

      // Get the Capture Graph Builder
      DirectShowUtil.DebugWrite("SWGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
      Guid clsid = Clsid.CaptureGraphBuilder2;
      Guid riid = typeof(ICaptureGraphBuilder2).GUID;
      m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref riid);

      DirectShowUtil.DebugWrite("SWGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
      int hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
      if (hr != 0) 
      {
        DirectShowUtil.DebugWrite("SWGraph:link FAILED:0x{0:X}",hr);
        return false;
      }
      DirectShowUtil.DebugWrite("SWGraph:Add graph to ROT table");
      DsROT.AddGraphToRot(m_graphBuilder, out m_rotCookie);

      // Get the video capture device and add it to the filter graph
      DirectShowUtil.DebugWrite("SWGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
      m_filterCaptureVideo = Marshal.BindToMoniker(filterVideoCaptureDevice.MonikerString) as IBaseFilter;
      if (m_filterCaptureVideo != null)
      {
        hr = m_graphBuilder.AddFilter(m_filterCaptureVideo, filterVideoCaptureDevice.Name);
        if (hr != 0) 
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED:Add Videodevice to filtergraph:0x{0:X}",hr);
          return false;
        }
      }

      // Get the audio capture device and add it to the filter graph
      if (filterAudioCaptureDevice != null)
      {
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() add capture device {0}",m_strAudioCaptureFilter);
        m_filterCaptureAudio = Marshal.BindToMoniker(filterAudioCaptureDevice.MonikerString) as IBaseFilter;
        if (m_filterCaptureAudio != null)
        {
          hr = m_graphBuilder.AddFilter(m_filterCaptureAudio, filterAudioCaptureDevice.Name);
          if (hr != 0) 
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:Add audiodevice to filtergraph:0x{0:X}",hr);
            return false;
          }
        }
      }

      // Retrieve TV Tuner if available
      DirectShowUtil.DebugWrite("SWGraph:Find TV Tuner");
      object o = null;
      Guid cat = FindDirection.UpstreamOnly;
      Guid iid = typeof(IAMTVTuner).GUID;
      hr = m_captureGraphBuilder.FindInterface(new Guid[1] { cat}, null, m_filterCaptureVideo, ref iid, out o);
      if (hr == 0) 
      {
        m_TVTuner = o as IAMTVTuner;
      }
      if (m_TVTuner == null)
      {
        DirectShowUtil.DebugWrite("SWGraph:CreateGraph() FAILED:no tuner found");
        iid = typeof(IAMTuner).GUID;
        hr = m_captureGraphBuilder.FindInterface(new Guid[1] { cat}, null, m_filterCaptureVideo, ref iid, out o);
      }

      // For some reason, it happens alot that the capture card can NOT be connected (pin 656 for the
      // PRV150MCE) to the encoder because for some reason the videostandard is GONE...
      // So fetch the standard from the TvTuner and define it for the capture card.

      if (m_TVTuner!=null )
      {
        InitializeTuner();

        m_IAMAnalogVideoDecoder = m_filterCaptureVideo as IAMAnalogVideoDecoder;
        if (m_IAMAnalogVideoDecoder!=null)
        {
          AnalogVideoStandard videoStandard;
          m_TVTuner.get_TVFormat(out videoStandard);
          if (videoStandard==AnalogVideoStandard.None) videoStandard=AnalogVideoStandard.PAL_B;
          SetVideoStandard(videoStandard);
        }
      }


      // check if all tvtuner outputs are connected
      if ( m_TVTuner!=null)
      {
				int connected=0;
				int notConnected=0;
        for (int ipin=0; ipin < 10; ipin++)
        {
          IPin pin=DirectShowUtil.FindPinNr( (IBaseFilter)m_TVTuner,PinDirection.Output,ipin);
          if (pin!=null)
          {
            IPin pConnectPin=null;
            hr=pin.ConnectedTo(out pConnectPin);  
						if (pConnectPin==null)
						{
							notConnected++;
						}
						else
						{
							connected++;
						}
          }
          else break;
        }

				if (notConnected>0 && connected<2)
				{
					Log.WriteFile(Log.LogType.Capture,"SWGraph: tuner got {0} pins connected and {1} pins unconnected", connected,notConnected);
					ConnectTVTunerOutputs();
				}
      }

      m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder, m_captureGraphBuilder, m_filterCaptureVideo);



      //set the frame size
      m_videoCaptureDevice.SetFrameSize(m_FrameSize);

      //set the frame rate
      m_videoCaptureDevice.SetFrameRate(m_FrameRate);

      m_IAMAnalogVideoDecoder = m_filterCaptureVideo as IAMAnalogVideoDecoder;

      m_graphState = State.Created;
      m_videoprocamp=m_filterCaptureVideo as IAMVideoProcAmp;
      if (m_videoprocamp!=null)
      {
				m_videoAmp=new VideoProcAmp(m_videoprocamp);
				m_videoAmp.Contrast=m_videoAmp.ContrastDefault;
				m_videoAmp.Brightness=m_videoAmp.BrightnessDefault;
				m_videoAmp.Gamma=m_videoAmp.GammaDefault;
				m_videoAmp.Saturation=m_videoAmp.SaturationDefault;
				m_videoAmp.Sharpness=m_videoAmp.SharpnessDefault;

				
      }

      return true;
    }

		public void SetRecordingLevel(bool enable, int level)
		{
			if (m_mixer!=null)
			{
				try
				{
					double fLevel=((double)level);
					fLevel /= 100.0d;
					m_mixer.put_MixLevel(fLevel);
				}
				catch(Exception){}
				
				try
				{
					m_mixer.put_Enable(enable);
				}
				catch(Exception){}
			}
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
      m_iPrevChannel=-1;

      DirectShowUtil.DebugWrite("SWGraph:DeleteGraph()");
      StopRecording();
      StopViewing();

      GUIGraphicsContext.OnGammaContrastBrightnessChanged -=new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);

      if (Vmr9!=null)
      {
        Vmr9.RemoveVMR9();
				Vmr9.Release();
        Vmr9=null;
      }

			if (Vmr7!=null)
			{
				Vmr7.RemoveVMR7();
				Vmr7=null;
			}

			SetRecordingLevel(false,0);
			m_mixer=null;
			
      if (m_videoprocamp!=null)
      {
        m_videoAmp=null;
        m_videoprocamp=null;
      }

      if (m_recControl!=null) 
      {
        m_recControl.Stop(0);
        Marshal.ReleaseComObject(m_recControl); m_recControl=null;
      }
      
      if (m_streamBuffer!=null) 
      {
        m_streamBuffer.Stop();
        Marshal.ReleaseComObject(m_streamBuffer); m_streamBuffer=null;
      }

      

      if (m_mediaControl != null)
      {
        m_mediaControl.Stop();
      }
      
      if (m_videoWindow != null)
      {
        m_bOverlayVisible=false;
        m_videoWindow.put_Visible(DsHlp.OAFALSE);
        m_videoWindow.put_Owner(IntPtr.Zero);
        m_videoWindow = null;
      }
      if ( m_StreamBufferSink!=null) 
      {
        Marshal.ReleaseComObject(m_StreamBufferSink);
        m_StreamBufferSink=null;
      }
      if ( m_StreamBufferConfig!=null) 
      {
        Marshal.ReleaseComObject(m_StreamBufferConfig);
        m_StreamBufferConfig=null;
      }
      if (m_videoCaptureDevice != null)
      {
        m_videoCaptureDevice.CloseInterfaces();
        m_videoCaptureDevice = null;
      }

      if (m_pConfig!=null) Marshal.ReleaseComObject(m_pConfig); m_pConfig=null;
      if (m_pinStreamBufferIn1!=null) Marshal.ReleaseComObject(m_pinStreamBufferIn1); m_pinStreamBufferIn1=null;
      if (m_pinStreamBufferIn0!=null) Marshal.ReleaseComObject(m_pinStreamBufferIn0); m_pinStreamBufferIn0=null;
      if (m_bSink!=null) Marshal.ReleaseComObject(m_bSink); m_bSink=null;

      if (m_IAMAnalogVideoDecoder!=null)
        Marshal.ReleaseComObject( m_IAMAnalogVideoDecoder ); m_IAMAnalogVideoDecoder = null;

      if (m_filterCaptureAudio != null)
        Marshal.ReleaseComObject(m_filterCaptureAudio); m_filterCaptureAudio = null;

      if (m_filterCompressorVideo != null)
        Marshal.ReleaseComObject(m_filterCompressorVideo); m_filterCompressorVideo = null;

      if (m_filterCompressorAudio != null)
        Marshal.ReleaseComObject(m_filterCompressorAudio); m_filterCompressorAudio = null;

      if (m_muxFilter != null)
        Marshal.ReleaseComObject(m_muxFilter); m_muxFilter = null;
   
      if (m_fileWriterFilter != null)
        Marshal.ReleaseComObject(m_fileWriterFilter); m_fileWriterFilter = null;

      if (m_TVTuner != null)
        Marshal.ReleaseComObject(m_TVTuner); m_TVTuner = null;

      m_basicVideo = null;
      m_mediaControl = null;
      
      if (m_filterCaptureVideo != null)
        Marshal.ReleaseComObject(m_filterCaptureVideo); m_filterCaptureVideo = null;

			if (m_graphBuilder!=null)
				DsUtils.RemoveFilters(m_graphBuilder);

      if (m_rotCookie != 0)
        DsROT.RemoveGraphFromRot(ref m_rotCookie);
      m_rotCookie = 0;



      if (m_captureGraphBuilder != null)
        Marshal.ReleaseComObject(m_captureGraphBuilder); m_captureGraphBuilder = null;

	
      if (m_graphBuilder != null)
        Marshal.ReleaseComObject(m_graphBuilder); m_graphBuilder = null;

      GUIGraphicsContext.form.Invalidate(true);
      GC.Collect();

      m_graphState = State.None;
      return;
    }

    void GetCompressors(out Filter filterVideoCompressor , out Filter filterAudioCompressor)
    {
      Filters filters = new Filters();
      filterVideoCompressor = null;
      filterAudioCompressor = null;
      DirectShowUtil.DebugWrite("SWGraph:GetCompressors() find video compressor filter {0}...", m_strVideoCompressor);
      
      // find video compressor filter
      foreach (Filter filter in filters.VideoCompressors)
      {
        if (filter.Name.Equals(m_strVideoCompressor))
        {
          filterVideoCompressor = filter;
          // check for wmv 7,8,9 DMP
          if (filter.MonikerString.Equals(@"@device:dmo:{3181343B-94A2-4FEB-ADEF-30A1DDE617B4}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") || 
            filter.MonikerString.Equals(@"@device:dmo:{BA2F0162-CAA4-48FC-89EA-DB0D1DFF40CA}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") || 
            filter.MonikerString.Equals(@"@device:dmo:{96B57CDD-8966-410C-BB1F-C97EEA765C04}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") ||
            
            filter.MonikerString.Equals(@"@device:cm:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\wmv3")  )
          {
            m_bRecordWMV = true;
          }
          break;
        }
      }

      // not found?, look in the legacy filters then
      if (filterVideoCompressor==null)
      {
        foreach (Filter filter in filters.LegacyFilters)
        {
          if (filter.Name.Equals(m_strVideoCompressor))
          {
            filterVideoCompressor = filter;
            // check for wmv 7,8,9 DMP
            if (filter.MonikerString.Equals(@"@device:dmo:{3181343B-94A2-4FEB-ADEF-30A1DDE617B4}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") || 
              filter.MonikerString.Equals(@"@device:dmo:{BA2F0162-CAA4-48FC-89EA-DB0D1DFF40CA}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") || 
              filter.MonikerString.Equals(@"@device:dmo:{96B57CDD-8966-410C-BB1F-C97EEA765C04}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") ||
            
              filter.MonikerString.Equals(@"@device:cm:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\wmv3")  )
            {
              m_bRecordWMV = true;
            }
            break;
          }
        }
      }
      
      //find audio compressor filter
      DirectShowUtil.DebugWrite("SWGraph:GetCompressors() find audio compressor filter {0}...", m_strAudioCompressor);
      foreach (Filter filter in filters.AudioCompressors)
      {
        if (filter.Name.Equals(m_strAudioCompressor))
        {
          filterAudioCompressor = filter;
          break;
        }
      }
      
      //not found? then look in the legacy filters
      if (filterAudioCompressor==null)
      {
        foreach (Filter filter in filters.LegacyFilters)
        {
          if (filter.Name.Equals(m_strAudioCompressor))
          {
            filterAudioCompressor = filter;
            break;
          }
        }
      }

      //no video compressor? then fail
      if (filterVideoCompressor == null) 
      {
        DirectShowUtil.DebugWrite("SWGraph:GetCompressors() FAILED couldnt find video compressor:{0}",m_strVideoCompressor);
        return ;
      }

      //no audio compressor? then fail
      if (filterAudioCompressor == null) 
      {
        DirectShowUtil.DebugWrite("SWGraph:GetCompressors() FAILED couldnt find audio compressor:{0}",m_strAudioCompressor);
        return ;
      }

      m_bIsUsingMPEG=IsUsingMPEGCompressors(filterVideoCompressor.MonikerString, filterAudioCompressor.MonikerString);
      DirectShowUtil.DebugWrite("SWGraph:GetCompressors() using mpeg2 compressors:{0}", m_bIsUsingMPEG);
    }

    bool AddCompressors()
    {
      if (m_filterCompressorAudio!=null) return true;
      if (m_filterCompressorVideo!=null) return true;
      int hr;
      Filter filterVideoCompressor ;
      Filter filterAudioCompressor;
      GetCompressors(out filterVideoCompressor , out filterAudioCompressor);
      DirectShowUtil.DebugWrite("SWGraph:AddCompressors()");
      if (filterVideoCompressor ==null || filterAudioCompressor==null) return false;

      // add the video compressor filters to the graph
      DirectShowUtil.DebugWrite("SWGraph:CreateGraph() add video compressor {0}",m_strVideoCompressor);
      m_filterCompressorVideo = Marshal.BindToMoniker(filterVideoCompressor.MonikerString) as IBaseFilter;
      if (m_filterCompressorVideo != null)
      {
        hr = m_graphBuilder.AddFilter(m_filterCompressorVideo, filterVideoCompressor.Name);
        if (hr != 0) 
        {
          DirectShowUtil.DebugWrite("SWGraph:AddCompressors() FAILED:Add video compressor to filtergraph:0x{0:X}",hr);
          return false;
        }
      }
      else
      {
        DirectShowUtil.DebugWrite("SWGraph:FAILED:AddCompressors() Add video compressor to filtergraph");
        return false;
      }

      // add the audio compressor filters to the graph
      DirectShowUtil.DebugWrite("SWGraph:CreateGraph() add audio compressor {0}",m_strAudioCompressor);
      m_filterCompressorAudio = Marshal.BindToMoniker(filterAudioCompressor.MonikerString) as IBaseFilter;
      if (m_filterCompressorAudio != null)
      {
        hr = m_graphBuilder.AddFilter(m_filterCompressorAudio, filterAudioCompressor.Name);
        if (hr != 0) 
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED:Add audio compressor to filtergraph");
          return false;
        }
      }
      else
      {
        DirectShowUtil.DebugWrite("SWGraph:FAILED:Add audio compressor to filtergraph:0x{0:X}",hr);
        return false;
      }

      // select the correct audio input pin to capture
      if (m_filterCaptureAudio != null)
      {
        if (m_strAudioInputPin.Length > 0)
        {
          DirectShowUtil.DebugWrite("SWGraph:set audio input pin:{0}", m_strAudioInputPin);
          IPin pinInput = DirectShowUtil.FindPin(m_filterCaptureAudio, PinDirection.Input, m_strAudioInputPin);
          if (pinInput == null)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED audio input pin:{0} not found", m_strAudioInputPin);
          }
          else
          {
            m_mixer = pinInput as IAMAudioInputMixer;
            if (m_mixer != null)
            {
							SetRecordingLevel(true,_RecordingLevel);
            }
            else
            {
              DirectShowUtil.DebugWrite("SWGraph:FAILED audio input pin:{0} does not expose an IAMAudioInputMixer", m_strAudioInputPin);
            }
          }
        }
      }
      DirectX.Capture.FilterPersist persist=new DirectX.Capture.FilterPersist (m_captureGraphBuilder,m_filterCompressorVideo,m_filterCompressorAudio);
      persist.LoadSettings(m_cardID);
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
      if (m_graphState != State.Recording) return;
      DirectShowUtil.DebugWrite("SWGraph:stop recording...");

      if (m_recControl!=null) 
      {
        int hr=m_recControl.Stop(0);
        if (hr!=0) 
        {
          DirectShowUtil.DebugWrite("mpeg2: FAILED to stop recording:0x{0:x}",hr );
          return;
        }
        if (m_recControl!=null) Marshal.ReleaseComObject(m_recControl); m_recControl=null;
      }


      m_mediaControl.Stop();
      m_graphState = State.Created;
      DeleteGraph();
      DirectShowUtil.DebugWrite("SWGraph:stopped recording...");
    }



    /// <summary>
    /// Switches / tunes to another TV channel
    /// </summary>
    /// <param name="iChannel">New channel</param>
    /// <remarks>
    /// Graph should be timeshifting. 
    /// </remarks>
    public void TuneChannel(TVChannel channel)
    {
			SetRecordingLevel(true,0);
      m_iCurrentChannel = channel.Number;
			m_iCountryCode=channel.Country;
			AnalogVideoStandard standard=channel.TVStandard;

      DirectShowUtil.DebugWrite("SWGraph:TuneChannel() tune to channel:{0}", m_iCurrentChannel);
      if (channel.Number < 10000)
      {
        if (m_TVTuner == null) return;
        if (m_bFirstTune)
        {
          m_bFirstTune = false;
          
          InitializeTuner();

          SetVideoStandard(standard);
          m_TVTuner.get_TVFormat(out standard);
        }
        try
        {
          int iCurrentChannel,iVideoSubChannel,iAudioSubChannel;
          SetVideoStandard(standard);
          m_TVTuner.get_TVFormat(out standard);
          m_TVTuner.get_Channel(out iCurrentChannel, out iVideoSubChannel, out iAudioSubChannel);
          if (iCurrentChannel!=channel.Number)
          {
            m_TVTuner.put_Channel(channel.Number,DShowNET.AMTunerSubChannel.Default,DShowNET.AMTunerSubChannel.Default);
          }
          int iFreq;
          double dFreq;
          m_TVTuner.get_VideoFrequency(out iFreq);
          dFreq = iFreq / 1000000d;
          DirectShowUtil.DebugWrite("SWGraph:TuneChannel() tuned to {0} MHz. tvformat:{1}", dFreq,standard.ToString());
        }
        catch (Exception) {} 
      }
      else
      {
        SetVideoStandard(standard);
      }
      DirectShowUtil.DebugWrite("SWGraph:TuneChannel() tuningspace:0 country:{0} tv standard:{1} cable:{2}",
                                  m_iCountryCode,standard.ToString(),
                                  m_bUseCable);

      bool bFixCrossbar=true;
      if (m_iPrevChannel>=0)
			{
				if (m_iPrevChannel< (int)ExternalInputs.svhs  && channel.Number < (int)ExternalInputs.svhs) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.svhs  && channel.Number ==(int)ExternalInputs.svhs) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.rgb   && channel.Number ==(int)ExternalInputs.rgb) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.cvbs1 && channel.Number ==(int)ExternalInputs.cvbs1) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.cvbs2 && channel.Number ==(int)ExternalInputs.cvbs2) bFixCrossbar=false;
      }
      if (bFixCrossbar)
      {
        DsUtils.FixCrossbarRoutingEx(m_graphBuilder,
																		 m_captureGraphBuilder,
																			m_filterCaptureVideo, 
																			channel.Number<(int)ExternalInputs.svhs, 
																			(channel.Number==(int)ExternalInputs.cvbs1), 
																			(channel.Number==(int)ExternalInputs.cvbs2), 
																			(channel.Number==(int)ExternalInputs.svhs) , 
																			(channel.Number==(int)ExternalInputs.rgb) ,
																			cardName);
      }
			m_iPrevChannel=channel.Number;
			SetRecordingLevel(true,_RecordingLevel);
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
      return m_bIsUsingMPEG;
    }


    /// <summary>
    /// Starts viewing the TV channel 
    /// </summary>
    /// <param name="iChannelNr">TV channel to which card should be tuned</param>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartViewing(TVChannel channel)
    {
			if (m_graphState == State.Viewing)
			{
				m_iCountryCode=channel.Country;
				TuneChannel(channel);
				return true;
			}

      if (m_graphState != State.Created) return false;
      Log.WriteFile(Log.LogType.Capture,"SWGraph:Start viewing");
			m_iCountryCode=channel.Country;
      TuneChannel(channel);

			// add VMR9 renderer to graph
			if (Vmr9!=null)
			{
				if (Vmr9.UseVMR9inMYTV)
				{
					Vmr9.AddVMR9(m_graphBuilder);
					if (Vmr9.VMR9Filter==null)
					{
						Vmr9.RemoveVMR9();
						Vmr9.Release();
						Vmr9=null;
						Vmr7.AddVMR7(m_graphBuilder);
					}
				}
				else Vmr7.AddVMR7(m_graphBuilder);
			}
			else Vmr7.AddVMR7(m_graphBuilder);

			/* disabled. this causes a audio-echo because now both the video capture filter audio out
			 * and the audio capture filter audio out are both connected to a directsound renderer
			Log.WriteFile(Log.LogType.Capture,"SWGraph:FAILED:render audio preview");
			DirectShowUtil.RenderOutputPins(m_graphBuilder, m_filterCaptureAudio,2);
			*/

			Log.WriteFile(Log.LogType.Capture,true,"SWGraph:render video preview");
      m_videoCaptureDevice.RenderPreview();
			m_mediaControl = (IMediaControl)m_graphBuilder;

			bool useOverlay=true;

			if (Vmr9!=null)
			{
				if (Vmr9.IsVMR9Connected)	
				{
					useOverlay=false;
					Vmr9.SetDeinterlaceMode();
				}
				else
				{
					Vmr9.RemoveVMR9();
					Vmr9.Release();
					Vmr9=null;
				}
			}

			if (useOverlay)
			{
				Log.WriteFile(Log.LogType.Capture,"SWGraph:Get overlay interfaces");
				m_videoWindow = m_graphBuilder as IVideoWindow;
				if (m_videoWindow==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"SWGraph:FAILED:Unable to get IVideoWindow");
					return false;
				}

				m_basicVideo = m_graphBuilder as IBasicVideo2;
				if (m_basicVideo==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"SWGraph:FAILED:Unable to get IBasicVideo2");
					return false;
				}
				int hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
				if (hr != 0) 
					DirectShowUtil.DebugWrite("SWGraph:FAILED:set Video window:0x{0:X}",hr);

				hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
				if (hr != 0) 
					DirectShowUtil.DebugWrite("SWGraph:FAILED:set Video window style:0x{0:X}",hr);

	      
				Log.WriteFile(Log.LogType.Capture,"SWGraph:Show overlay");
				m_bOverlayVisible=true;
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
					DirectShowUtil.DebugWrite("SWGraph:FAILED:put_Visible:0x{0:X}",hr);
			}

      DirectShowUtil.DebugWrite("SWGraph:enable deinterlace");
      DirectShowUtil.EnableDeInterlace(m_graphBuilder);

      
      Log.WriteFile(Log.LogType.Capture,"SWGraph:run graph");
      m_mediaControl.Run();
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string strValue=xmlreader.GetValueAsString("mytv","defaultar","normal");
				if (strValue.Equals("zoom")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Zoom;
				if (strValue.Equals("stretch")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
				if (strValue.Equals("normal")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Normal;
				if (strValue.Equals("original")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Original;
				if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
				if (strValue.Equals("panscan")) GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.PanScan43;

			}
  
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      m_graphState = State.Viewing;
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
      if (m_graphState != State.Viewing) return false;
       
      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      DirectShowUtil.DebugWrite("SWGraph:StopViewing()");
			if (m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
			if (Vmr9!=null)
			{
				Vmr9.Enable(false);
			}
			
      m_bOverlayVisible=false;
      m_mediaControl.Stop();
      m_graphState = State.Created;
      DeleteGraph();
      return true;
    }

    
    public bool Overlay
    {
      get 
      {
        return m_bOverlayVisible;
      }
      set 
      {
        if (value==m_bOverlayVisible) return;
				if (m_videoWindow==null) return;
        m_bOverlayVisible=value;
        if (!m_bOverlayVisible)
        {
          Log.WriteFile(Log.LogType.Capture,"SWGraph: hide overlay window");
          if (m_videoWindow!=null)
            m_videoWindow.put_Visible( DsHlp.OAFALSE );

        }
        else
        {
          Log.WriteFile(Log.LogType.Capture,"SWGraph: show overlay window");
          if (m_videoWindow!=null)
            m_videoWindow.put_Visible( DsHlp.OATRUE );

        }
      }
    }

    /// <summary>
    /// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
    /// </summary>
    private void GUIGraphicsContext_OnVideoWindowChanged()
    {
			
			if (GUIGraphicsContext.Vmr9Active) return;
			if (m_graphState != State.Viewing) return;
			if (m_basicVideo==null) return;
			if (m_videoWindow==null) return;
      int iVideoWidth, iVideoHeight;

			if (GUIGraphicsContext.BlankScreen)
			{
				Overlay=false;
			}
			else
			{
				Overlay=true;
			}
			m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);
			
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        float x = GUIGraphicsContext.OverScanLeft;
        float y = GUIGraphicsContext.OverScanTop;
        int nw = GUIGraphicsContext.OverScanWidth;
        int nh = GUIGraphicsContext.OverScanHeight;
        if (nw <= 0 || nh <= 0) return;


        System.Drawing.Rectangle rSource, rDest;
        MediaPortal.GUI.Library.Geometry m_geometry = new MediaPortal.GUI.Library.Geometry();
        m_geometry.ImageWidth = iVideoWidth;
        m_geometry.ImageHeight = iVideoHeight;
        m_geometry.ScreenWidth = nw;
        m_geometry.ScreenHeight = nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;

					
				if (rSource.Left< 0 || rSource.Top<0 || rSource.Width<=0 || rSource.Height<=0) return;
				if (rDest.Left <0 || rDest.Top < 0 || rDest.Width<=0 || rDest.Height<=0) return;
				m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
				m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
				m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
				DirectShowUtil.DebugWrite("SWGraph: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("SWGraph: source position:({0},{1})-({2},{3})",rSource.Left, rSource.Top, rSource.Right, rSource.Bottom);
				DirectShowUtil.DebugWrite("SWGraph: dest   position:({0},{1})-({2},{3})",rDest.Left, rDest.Top, rDest.Right, rDest.Bottom);
      }
      else
      {
				if ( GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 || 
					GUIGraphicsContext.VideoWindow.Width <=0 || GUIGraphicsContext.VideoWindow.Height <=0) return;
				if (iVideoHeight<=0 || iVideoWidth<=0) return;
				m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
				m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				DirectShowUtil.DebugWrite("SWGraph: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("SWGraph: source position:({0},{1})-({2},{3})",0, 0, iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("SWGraph: dest   position:({0},{1})-({2},{3})",GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Right, GUIGraphicsContext.VideoWindow.Bottom);
      }
    }

    void SetRegistryThings()
    {
      //disable xvid status window while encoding
      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        RegistryKey subkey = hkcu.OpenSubKey(@"Software\GNU\XviD",true);
        if (subkey != null)
        {
          long uivalue=0;
          subkey.SetValue("display_status", (int)uivalue);
        }
      }
      catch(Exception)
      {
      }
    }

    
    /// <summary>
    /// This method can be used to ask the graph if it should be rebuild when
    /// we want to tune to the new channel:ichannel
    /// </summary>
    /// <param name="iChannel">new channel to tune to</param>
    /// <returns>true : graph needs to be rebuild for this channel
    ///          false: graph does not need to be rebuild for this channel
    /// </returns>
    public bool ShouldRebuildGraph(TVChannel newChannel)
		{
			int iChannel=newChannel.Number;
      // if we switch from tuner <-> SVHS/Composite then 
      // we need to rebuild the capture graph
      bool bFixCrossbar=true;
      if (m_iPrevChannel>=0)
			{
				if (m_iPrevChannel< (int)ExternalInputs.svhs  && iChannel < (int)ExternalInputs.svhs) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.svhs  && iChannel ==(int)ExternalInputs.svhs) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.rgb   && iChannel ==(int)ExternalInputs.rgb) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.cvbs1 && iChannel ==(int)ExternalInputs.cvbs1) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.cvbs2 && iChannel ==(int)ExternalInputs.cvbs2) bFixCrossbar=false;
      }
      else bFixCrossbar=false;
      return bFixCrossbar;
    }

    public bool SignalPresent()
    {
      if (m_graphState!=State.Recording && m_graphState!=State.Viewing) return false;
      if (m_TVTuner==null) return true;
      AMTunerSignalStrength strength;
      m_TVTuner.SignalPresent(out strength);
      return ( ( (int)strength ) >=1 );
    }

		public int  SignalQuality()
		{
			if (m_graphState!=State.Recording && m_graphState!=State.Viewing) return 1;
			if (m_TVTuner==null) return 1;
			AMTunerSignalStrength strength;
			m_TVTuner.SignalPresent(out strength);
			if (strength==AMTunerSignalStrength.SignalPresent) return 100;
			return 1;
		}
		
		public int  SignalStrength()
		{
			if (m_graphState!=State.Recording && m_graphState!=State.Viewing) return 1;
			if (m_TVTuner==null) return 1;
			AMTunerSignalStrength strength;
			m_TVTuner.SignalPresent(out strength);
			if (strength==AMTunerSignalStrength.SignalPresent) return 100;
			return 1;
		}

    public long VideoFrequency()
    {
      
      if (m_graphState!=State.Recording && m_graphState!=State.Viewing) return 0;
      if (m_TVTuner==null) return 0;
      int lFreq;
      m_TVTuner.get_VideoFrequency(out lFreq);
      return lFreq;
    }

    /// <summary>
    /// This method will start recording and will write all data to strFileName
    /// </summary>
    /// <param name="strFilename">file where recording should b saved</param>
    /// <param name="bContentRecording">
    /// when true it will make a content recording. A content recording writes the data to a new permanent file. 
    /// when false it will make a reference recording. A reference recording creates a stub file that refers to the existing backing files, which are made permanent. Create a reference recording if you want to save data that has already been captured.</param>
    public void Record(string strFilename, bool bContentRecording,DateTime timeProgStart, DateTime timeFirstMoment)
    {
      //      strFilename=@"C:\media\movies\test.dvr-ms";
      DirectShowUtil.DebugWrite("mpeg2: Record : {0} {1}",strFilename,bContentRecording);
      IntPtr recorderObj;
      if (m_bSink==null) 
      {
        DirectShowUtil.DebugWrite("mpeg2: CreateRecorder : no sink!");
        return;
      }
      uint iRecordingType=0;
      if (bContentRecording) iRecordingType=0;
      else iRecordingType=1;										
					 
      int hr=m_bSink.CreateRecorder(strFilename, iRecordingType, out recorderObj);
      if (hr!=0) 
      {
        DirectShowUtil.DebugWrite("mpeg2: CreateRecorder FAILED:0x{0:x}",hr );
        return;
      }
      object objRecord=Marshal.GetObjectForIUnknown(recorderObj);
      if (objRecord==null) 
      {
        DirectShowUtil.DebugWrite("mpeg2: FAILED getting Inknown of recorder");
        return;
      }
      
      Marshal.Release(recorderObj);

      m_recControl=objRecord as IStreamBufferRecordControl;
      if (m_recControl==null) 
      {
        DirectShowUtil.DebugWrite("mpeg2: FAILED getting IStreamBufferRecordControl");
        return;
      }

      long lStartTime=0;

      // if we're making a reference recording
      // then record all content from the past as well
      if (!bContentRecording)
      {
        // so set the startttime...
        uint uiSecondsPerFile;
        uint uiMinFiles, uiMaxFiles;
        m_pConfig.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
        m_pConfig.GetBackingFileDuration(out uiSecondsPerFile);
        lStartTime = uiSecondsPerFile;
        lStartTime*= (long)uiMaxFiles;

        // if start of program is given, then use that as our starttime
        if (timeProgStart.Year>2000)
        {
          TimeSpan ts = DateTime.Now-timeProgStart;
          DirectShowUtil.DebugWrite("mpeg2:Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
            timeProgStart.Hour,timeProgStart.Minute,timeProgStart.Second,
            ts.TotalHours,ts.TotalMinutes,ts.TotalSeconds);
															
          lStartTime = (long)ts.TotalSeconds;
        }
        else DirectShowUtil.DebugWrite("mpeg2:record entire timeshift buffer");
      
        TimeSpan tsMaxTimeBack=DateTime.Now-timeFirstMoment;
        if (lStartTime > tsMaxTimeBack.TotalSeconds )
        {
          lStartTime =(long)tsMaxTimeBack.TotalSeconds;
        }
        

        lStartTime*=-10000000L;//in reference time 
      }

      hr=m_recControl.Start(ref lStartTime);
      if (hr!=0) 
      {
        //could not start recording...
        DirectShowUtil.DebugWrite("mpeg2: FAILED to start recording:0x{0:x}",hr );
        if (lStartTime!=0)
        {
          // try recording from livepoint instead from the past
          lStartTime=0;
          hr=m_recControl.Start(ref lStartTime);
          if (hr!=0)
          {
            //still fails
            DirectShowUtil.DebugWrite("mpeg2: FAILED to start recording now:0x{0:x}",hr );
            return;
          }
          else
          {
            //that worked!
            DirectShowUtil.DebugWrite("mpeg2: FAILED to record now succeeded");
          }
        }
        else
        {
          //record fails
          return;
        }
      }
      lStartTime/=-10000000L;
      long iHour=lStartTime/3600;
      lStartTime -= (iHour*3600);
      long iMin =lStartTime/60;
      lStartTime -= (iMin*60);
      long iSec =lStartTime;
      DirectShowUtil.DebugWrite("mpeg2: recording started from {0:00}:{1:00}:{2:00} ago",iHour,iMin,iSec);

    }

    void CreateSBESink()
    {
      DirectShowUtil.DebugWrite("mpeg2:add streambuffersink");
      m_StreamBufferSink=new StreamBufferSink();
      m_streamBuffer = (IBaseFilter) m_StreamBufferSink;
      if (m_streamBuffer ==null)
      {
        DirectShowUtil.DebugWrite("mpeg2:FAILED to add streambuffer");
        return;
      }

      m_graphBuilder.AddFilter(m_streamBuffer, "SBE SINK");

      IntPtr subKey = IntPtr.Zero;
      
      IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
      IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_streamBuffer;
      //  IntPtr[] sids = new IntPtr[] {pSid};
      //  int result = pConfig.SetSIDs(1, sids);
      RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
      int hr=pConfig.SetHKEY(subKey);
      if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set hkey:0x{0:X}",hr);
      

      m_pinStreamBufferIn0=DirectShowUtil.FindPinNr(m_streamBuffer,PinDirection.Input,0);
      if (m_pinStreamBufferIn0==null) DirectShowUtil.DebugWrite("mpeg2: FAILED to find input pin#0 of streambuffersink");
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
    public bool StartRecording(Hashtable attribtutes,TVRecording recording,TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if (m_graphState == State.Recording) return true;
      if (m_graphState != State.Created) return false;
      m_iCountryCode=channel.Country;

      SetRegistryThings();
      int hr;
      DirectShowUtil.DebugWrite("SWGraph:Start recording...");

      m_bRecordWMV = false;
      strFileName = System.IO.Path.ChangeExtension(strFileName, ".avi");

      // add compressors
      if (m_bIsUsingMPEG)
      {
        if (m_graphState!=State.TimeShifting)
        {
          strFileName=System.IO.Path.ChangeExtension(strFileName, ".tv");
          if (!StartTimeShifting(channel,strFileName)) return false;
          strFileName=System.IO.Path.ChangeExtension(strFileName, ".dvr-ms");
        }
        Record(strFileName,true,timeProgStart,DateTime.Now);
        m_graphState = State.Recording;
        return true;
      }
      else
      {
        if (!AddCompressors()) return false;
      }

      //change the filename extension to .wmv or .avi 
      strFileName = System.IO.Path.ChangeExtension(strFileName, ".avi");
      if (m_bRecordWMV)
      {
        strFileName = System.IO.Path.ChangeExtension(strFileName, ".wmv");
      }
      
      // set filename
      DirectShowUtil.DebugWrite("SWGraph:record to :{0} ", strFileName);

      Guid mediaSubTypeAvi = MediaSubType.Avi;
      if (m_bRecordWMV)
        mediaSubTypeAvi = MediaSubType.Asf;

      hr = m_captureGraphBuilder.SetOutputFileName(ref mediaSubTypeAvi, strFileName, out m_muxFilter, out m_fileWriterFilter);
      if (hr != 0)
      {
        DirectShowUtil.DebugWrite("SWGraph:FAILED:to set output filename to :{0} :0x{1:X}", strFileName, hr);
        return false;
      }

      // if we record to wmv, then set the wmv profile
      if (m_bRecordWMV)
      {
        DirectShowUtil.DebugWrite("SWGraph:get IConfigAsfWriter");
        IConfigAsfWriter asfwriter = m_fileWriterFilter as IConfigAsfWriter;
        if (asfwriter != null)
        {
          DirectShowUtil.DebugWrite("SWGraph:IConfigAsfWriter.SetProfile(BestVBRVideo)");
          //Guid WMProfile_V80_HIGHVBRVideo = new Guid( 0xf10d9d3,0x3b04,0x4fb0,0xa3, 0xd3, 0x88, 0xd4, 0xac, 0x85, 0x4a, 0xcc);
          Guid WMProfile_V80_BESTVBRVideo = new Guid(0x48439ba, 0x309c, 0x440e, 0x9c, 0xb4, 0x3d, 0xcc, 0xa3, 0x75, 0x64, 0x23);
          hr = asfwriter.ConfigureFilterUsingProfileGuid(WMProfile_V80_BESTVBRVideo);
          if (hr != 0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED IConfigAsfWriter.SetProfile() :0x{0:X}", hr);
          }
        }
        else DirectShowUtil.DebugWrite("SWGraph:FAILED:to get IConfigAsfWriter");
      }

      ConnectCompressors(m_muxFilter);
      // for avi files, set the audio as the masterstream
      if (!m_bRecordWMV)
      {
        // set avi muxing parameters
        IConfigAviMux ConfigMux = m_muxFilter as IConfigAviMux;
        if (ConfigMux != null)
        {
          DirectShowUtil.DebugWrite("SWGraph:set audio as masterstream");
          hr = ConfigMux.SetMasterStream(1);
          if (hr != 0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:to set audio as masterstream:0x{0:X}",hr);
          }
        }
        else
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED:to get IConfigAviMux");
        }

        // Set the avi interleaving mode
        IConfigInterleaving InterleaveMode = m_muxFilter as IConfigInterleaving;
        if (InterleaveMode != null)
        {
          DirectShowUtil.DebugWrite("SWGraph:set avi interleave mode");
          hr = InterleaveMode.put_Mode(AviInterleaveMode.INTERLEAVE_CAPTURE);
          if (hr != 0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:to set avi interleave mode:0x{0:X}",hr);
          }
        }
        else
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED:to get IConfigInterleaving");
        }
      }//if (!bRecordWMV)

      if (m_mediaControl == null)
        m_mediaControl = (IMediaControl)m_graphBuilder;

      TuneChannel(channel);

      m_mediaControl.Run();
      m_graphState = State.Recording;

      DirectShowUtil.DebugWrite("SWGraph:recording...");
      return true;
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
    public bool StartTimeShifting(TVChannel channel, string strFileName)
    {
      if (m_graphState!=State.Created) return false;;

			m_iCountryCode=channel.Country;
      if (!AddCompressors()) return false;

      ConnectCompressors(null);

      int hr;
      if (m_StreamBufferSink==null)
      {
        CreateSBESink();
      }
      strFileName=System.IO.Path.ChangeExtension(strFileName,".tv" );
      DirectShowUtil.DebugWrite("mpeg2:StartTimeshifting({0})", strFileName);
      int ipos=strFileName.LastIndexOf(@"\");
      string strDir=strFileName.Substring(0,ipos);
      //if (!m_bRendered) 
      {
        //DeleteOldTimeShiftFiles(strDir);
        DirectShowUtil.DebugWrite("mpeg2:render graph");
        /// [                 ]        [                ]
        /// [mpeg2 video comp ] ->     [#0              ]
        /// [                 ]        [  streambuffer  ]
        /// [mpeg2 audio comp ] -------[#1              ]
				
	      
        DirectShowUtil.DebugWrite("mpeg2:render to :{0}",strFileName);
        
        m_pinVideoOut=  DirectShowUtil.FindPinNr(m_filterCompressorVideo,PinDirection.Output,0);
        m_pinAudioOut=  DirectShowUtil.FindPinNr(m_filterCompressorAudio,PinDirection.Output,0);

        if (m_pinVideoOut==null) return false;
        if (m_pinAudioOut==null) return false;
        if (m_pinStreamBufferIn0==null) return false;
	    
        //mpeg2 video compressor out ->streambuffer in#0
        DirectShowUtil.DebugWrite("mpeg2:video compressor out->stream buffer");
        hr=m_graphBuilder.Connect(m_pinVideoOut, m_pinStreamBufferIn0);
        if (hr==0) 
          DirectShowUtil.DebugWrite("mpeg2:connected to streambuffer");
        else
          DirectShowUtil.DebugWrite("mpeg2:FAILED to connect video compressor  output to streambuffer:0x{0:X}",hr);


        //find streambuffer in#1 pin
        m_pinStreamBufferIn1=DirectShowUtil.FindPinNr(m_streamBuffer,PinDirection.Input,1);
        if (m_pinStreamBufferIn1==null) 
        {
          DirectShowUtil.DebugWrite("mpeg2: FAILED to find input pin#1 of streambuffersink");
          return false; 
        }
        //mpeg2 demux audio out->streambuffer in#1
        DirectShowUtil.DebugWrite("mpeg2:demux audio out->stream buffer");
        hr=m_graphBuilder.Connect(m_pinAudioOut, m_pinStreamBufferIn1);
        if (hr==0) 
          DirectShowUtil.DebugWrite("mpeg2:audio out connected to streambuffer");
        else
          DirectShowUtil.DebugWrite("mpeg2:FAILED to connect audio out to streambuffer:0x{0:X}",hr);

        //set mpeg2 demux as reference clock 
        //(m_graphBuilder as IMediaFilter).SetSyncSource(m_mpeg2Multiplexer as IReferenceClock);

        //set filename
        m_bSink = m_streamBuffer as IStreamBufferSink;
        if (m_bSink ==null) DirectShowUtil.DebugWrite("mpeg2:FAILED to get IStreamBufferSink interface");

        DirectShowUtil.DebugWrite("mpeg2:Set folder:{0} filecount 10-20, fileduration:10mins",strDir);


        // set streambuffer backing file configuration
        m_StreamBufferConfig=new StreamBufferConfig();
        m_pConfig = (IStreamBufferConfigure) m_StreamBufferConfig;

        IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
        IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_pConfig;
        IntPtr subKey = IntPtr.Zero;

        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr=pTemp.SetHKEY(subKey);
        if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set hkey:0x{0:X}",hr);

        hr=m_pConfig.SetDirectory(strDir);
        if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile folder:0x{0:X}",hr);
	      
        hr=m_pConfig.SetBackingFileCount(6, 8);    //6-8 files
        if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile count:0x{0:X}",hr);

        hr=m_pConfig.SetBackingFileDuration( 30*60); // 60sec*30min= 0.5 hours/file
        if (hr!=0) DirectShowUtil.DebugWrite("mpeg2: FAILED to set backingfile duration:0x{0:X}",hr);

				IStreamBufferConfigure2 streamConfig2	= m_StreamBufferConfig as IStreamBufferConfigure2;
				if (streamConfig2!=null)
					streamConfig2.SetFFTransitionRates(8,32);

      }

      // lock profile
      DirectShowUtil.DebugWrite("mpeg2:lock profile");
      hr=m_bSink.LockProfile(strFileName);
      if (hr !=0) DirectShowUtil.DebugWrite("mpeg2:FAILED to set streambuffer filename:0x{0:X}",hr);

      TuneChannel(channel);

      m_mediaControl = (IMediaControl)m_graphBuilder;
      if (m_mediaControl!=null)
      {
        hr=m_mediaControl.Run();
        DirectShowUtil.DebugWrite("mpeg2:StartTimeshifting() started mediactl:{0}",hr);
      }
      m_graphState=State.TimeShifting;
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
      if (m_graphState!=State.TimeShifting) return false;
      try
      {
        int hr;
        DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting()");
        if (m_bSink!=null)
        {
          IStreamBufferSink3 sink3=m_bSink as IStreamBufferSink3;
          if (sink3!=null)
          {
            DirectShowUtil.DebugWrite("mpeg2:unlock profile");
            hr=sink3.UnlockProfile();
            if (hr !=0) DirectShowUtil.DebugWrite("mpeg2:FAILED to set unlock profile:0x{0:X}",hr);
          }
        }
        if (m_mediaControl!=null)
        {
          DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting()  stop mediactl");
          m_mediaControl.Stop(); 
          DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting()  mediactl stopped");
        }
        DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting() stopped");
      }
      catch(Exception ex)
      {
        DirectShowUtil.DebugWrite("mpeg2:StopTimeShifting() exception:"+ex.ToString() );
      }
      
      return true;
    }

    bool ConnectCompressors(IBaseFilter muxer)
    {
      Guid cat,med;
      int hr;
      
      DirectShowUtil.DebugWrite("SWGraph:ConnectCompressors()");
      if (muxer==null)
      {
        // connect video capture->compressor
        IPin videoIn=DirectShowUtil.FindPinNr(m_filterCompressorVideo,PinDirection.Input,0);
        IPin audioIn=DirectShowUtil.FindPinNr(m_filterCompressorAudio,PinDirection.Input,0);
        if (videoIn==null)
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED unable to find video compressor input");
          return false;
        }
        if (audioIn==null)
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED unable to find audio compressor input");
          return false;
        }
        if (m_videoCaptureDevice.CapturePin != null)
        {
          hr=m_graphBuilder.Connect(m_videoCaptureDevice.CapturePin ,videoIn);
          if (hr!=0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED unable to connect videocap:capture->video compressor");
            return false;
          }
        }
        else if (m_videoCaptureDevice.PreviewVideoPin!= null)
        {
          hr=m_graphBuilder.Connect(m_videoCaptureDevice.PreviewVideoPin,videoIn);
          if (hr!=0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED unable to connect videocap:preview->video compressor");
            return false;
          }
        }
        else if (m_videoCaptureDevice.VideoPort!= null)
        {
          hr=m_graphBuilder.Connect(m_videoCaptureDevice.VideoPort ,videoIn);
          if (hr!=0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED unable to connect videocap:vport->video compressor");
            return false;
          }
        }

        // connect audio capture->compressor->muxer
        if (m_filterCaptureAudio != null)
        {
          IPin audioOut=DirectShowUtil.FindPinNr(m_filterCaptureAudio,PinDirection.Output,0);
          if (audioOut==null)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED unable to find audiocap:output ");
            return false;
          }
          hr=m_graphBuilder.Connect(audioOut ,audioIn);
          if (hr!=0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED unable to connect audiocap:output->audio compressor");
            return false;
          }
        }
        return true;
      }


      // connect video capture->compressor->muxer
      if (m_videoCaptureDevice.CapturePin != null)
      {
        // NOTE that we try to render the interleaved pin before the video pin, because
        // if BOTH exist, it's a DV filter and the only way to get the audio is to use
        // the interleaved pin.  Using the Video pin on a DV filter is only useful if
        // you don't want the audio.
        DirectShowUtil.DebugWrite("SWGraph:videocap:connect video capture->compressor (interleaved)");
        cat = PinCategory.Capture;
        med = MediaType.Interleaved;
        hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureVideo, m_filterCompressorVideo, muxer);
        if (hr != 0)
        {
          DirectShowUtil.DebugWrite("SWGraph:videocap:connect video capture->compressor (video)");
          cat = PinCategory.Capture;
          med = MediaType.Video;
          hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureVideo, m_filterCompressorVideo, muxer);
          if (hr != 0)
          {
            DirectShowUtil.DebugWrite("SWGraph:FAILED:videocap:to connect video capture->compressor :0x{0:X}",hr);
            return false;
          }
        }

        // connect audio capture->compressor->muxer
        if (m_filterCaptureAudio == null)
        {
          DirectShowUtil.DebugWrite("SWGraph:videocap:connect audio capture->compressor ");
          cat = PinCategory.Capture;
          med = MediaType.Audio;
          hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureVideo, m_filterCompressorAudio, muxer);
          if (hr == 0)
          {
            DirectShowUtil.DebugWrite("SWGraph:videocap:connect audio capture->compressor :succeeded");
          }
        }
      }


      // connect audio capture->compressor->muxer
      if (m_filterCaptureAudio != null)
      {
        DirectShowUtil.DebugWrite("SWGraph:audiocap:connect audio capture->compressor ");
        cat = PinCategory.Capture;
        med = MediaType.Audio;
        hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureAudio, m_filterCompressorAudio, muxer);
        if (hr != 0)
        {
          DirectShowUtil.DebugWrite("SWGraph:FAILED:audiocap:to connect audio capture->compressor :0x{0:X}",hr);
          return false;
        }
      } 
      return true;
    }

    bool IsUsingMPEGCompressors(string monikerVideo,string monikerAudio)
    {
      bool VideoIsMPEG=false;
      bool AudioIsMPEG=false;

      //Cyberlink MPEG Audio encoder
      if (monikerAudio==@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{A3D70AC0-9023-11D2-8D55-0080C84E9C68}") AudioIsMPEG=true;
      
      //Cyberlink MPEG Video encoder
      if (monikerVideo==@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{36B46E60-D240-11D2-8F3F-0080C84E9806}") VideoIsMPEG=true;

      //YMPEG audio codec
      if (monikerAudio==@"@device:cm:{33D9A761-90C8-11D0-BD43-00A0C911CE86}\1792YMPEG Audio Codec") AudioIsMPEG=true;

      //YMPEG video codec
      if (monikerVideo==@"@device:cm:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\ympg") VideoIsMPEG=true;

      return (VideoIsMPEG==true && AudioIsMPEG==true);
    }

		static bool reentrant=false;
    protected void OnGammaContrastBrightnessChanged()
    {
      if (m_graphState!=State.Recording && m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return ;
      if (m_videoprocamp==null) return;
      if (m_videoAmp==null) return;
			if (reentrant) return;
			reentrant=true;

			//set the changed values
			if (GUIGraphicsContext.Brightness>-1)
			{
				m_videoAmp.Brightness = GUIGraphicsContext.Brightness;
			}
			else
			{
				GUIGraphicsContext.Brightness=m_videoAmp.Brightness ;
			}

			if (GUIGraphicsContext.Contrast>-1)
			{
				m_videoAmp.Contrast = GUIGraphicsContext.Contrast;
			}
			else
			{
				GUIGraphicsContext.Contrast=m_videoAmp.Contrast;
			}

			if (GUIGraphicsContext.Gamma>-1)
			{
				m_videoAmp.Gamma = GUIGraphicsContext.Gamma;
			}
			else
			{
				GUIGraphicsContext.Gamma=m_videoAmp.Gamma;
			}

			if (GUIGraphicsContext.Saturation>-1)
			{
				m_videoAmp.Saturation = GUIGraphicsContext.Saturation;
			}
			else
			{
				GUIGraphicsContext.Saturation=m_videoAmp.Saturation;
			}


			if (GUIGraphicsContext.Sharpness>-1)
			{
				m_videoAmp.Saturation = GUIGraphicsContext.Sharpness;
			}
			else
			{
				GUIGraphicsContext.Sharpness=m_videoAmp.Sharpness;
			}


			//get back the changed values
			GUIGraphicsContext.Brightness= m_videoAmp.Brightness  ;
			GUIGraphicsContext.Contrast  = m_videoAmp.Contrast ;
			GUIGraphicsContext.Gamma     = m_videoAmp.Gamma ;
			GUIGraphicsContext.Saturation= m_videoAmp.Saturation;
			GUIGraphicsContext.Sharpness = m_videoAmp.Sharpness;
			reentrant=false;
		}

    void ConnectTVTunerOutputs()
    {
      // AverMedia MCE card has a bug. It will only connect the TV Tuner->crossbar if
      // the crossbar outputs are disconnected
      // same for the winfast pvr 2000
      DirectShowUtil.DebugWrite("SWGraph:ConnectTVTunerOutputs()");
      
      //find crossbar
      int  hr;
      Guid cat;
      Guid iid;
      object o=null;
      cat = FindDirection.UpstreamOnly;
      iid = typeof(IAMCrossbar).GUID;
      hr=m_captureGraphBuilder.FindInterface(new Guid[1]{cat},null,m_filterCaptureVideo, ref iid, out o);
      if (hr !=0 || o == null) 
      {
        DirectShowUtil.DebugWrite("SWGraph:no crossbar found");
        return; // no crossbar found?
      }
    
      IAMCrossbar crossbar = o as IAMCrossbar;
      if (crossbar ==null) 
      {
        DirectShowUtil.DebugWrite("SWGraph:no crossbar found");
        return;
      }


      //disconnect the output pins of the crossbar->video capture filter
      DirectShowUtil.DebugWrite("SWGraph:disconnect crossbar outputs");
      DirectShowUtil.DisconnectOutputPins(m_graphBuilder,(IBaseFilter)crossbar);


			IPin crossBarAudioTunerIn=null;
			IPin crossBarVideoTunerIn=null;
			int iOutputPinCount, iInputPinCount;
			crossbar.get_PinCounts(out iOutputPinCount, out iInputPinCount);
			for (int i=0; i < iInputPinCount; ++i)
			{
				int iPinIndexRelatedIn;
				PhysicalConnectorType PhysicalTypeIn;			// type of input pin
				crossbar.get_CrossbarPinInfo(true,i,out iPinIndexRelatedIn, out PhysicalTypeIn);
				if (PhysicalTypeIn == PhysicalConnectorType.Audio_Tuner)
				{
					Log.WriteFile(Log.LogType.Capture,"SWGraph:got crossbar audio tuner input");
					crossBarAudioTunerIn=DirectShowUtil.FindPinNr( (IBaseFilter)crossbar, PinDirection.Input,i);
				}
				if (PhysicalTypeIn == PhysicalConnectorType.Video_Tuner)
				{
					Log.WriteFile(Log.LogType.Capture,"SWGraph:got crossbar video tuner input");
					crossBarVideoTunerIn=DirectShowUtil.FindPinNr( (IBaseFilter)crossbar, PinDirection.Input,i);
				}
			}


      //connect the output pins of the tvtuner
			for (int iPinTuner=0; iPinTuner < 10; iPinTuner++)
			{
				IPin pin=DirectShowUtil.FindPinNr( (IBaseFilter)m_TVTuner,PinDirection.Output,iPinTuner);
				if (pin!=null)
				{
					IPin pConnectPin=null;
					hr=pin.ConnectedTo(out pConnectPin);  
					if (pConnectPin==null)
					{
						//this pin is not connected.
						//try to connect it to the crossbar audio/video tuner input 
						hr=-1;
						if (crossBarAudioTunerIn!=null)
							hr=m_graphBuilder.Connect(pin, crossBarAudioTunerIn);
						if (hr!=0)
						{
							if (crossBarVideoTunerIn!=null)
								hr=m_graphBuilder.Connect(pin, crossBarVideoTunerIn);
							if (hr==0)
							{
								Log.WriteFile(Log.LogType.Capture,"SWGraph:connected tuner->crossbar video tuner input");
							}
						}
						else
						{	
							Log.WriteFile(Log.LogType.Capture,"SWGraph:connected tuner->crossbar audio tuner input");
						}
					}
				}
				else break;
			}
			if (crossBarAudioTunerIn!=null)
				Marshal.ReleaseComObject(crossBarAudioTunerIn);
			crossBarAudioTunerIn=null;

			if (crossBarVideoTunerIn!=null)
				Marshal.ReleaseComObject(crossBarVideoTunerIn);
			crossBarVideoTunerIn=null;

      //reconnect the output pins of the crossbar
			IPin crossbarOut1 = DirectShowUtil.FindPinNr( (IBaseFilter)crossbar,PinDirection.Output,0);
			IPin crossbarOut2 = DirectShowUtil.FindPinNr( (IBaseFilter)crossbar,PinDirection.Output,1);

			IPin videoCaptureIn1 = DirectShowUtil.FindPinNr( (IBaseFilter)m_filterCaptureVideo,PinDirection.Input,0);
			IPin videoCaptureIn2 = DirectShowUtil.FindPinNr( (IBaseFilter)m_filterCaptureVideo,PinDirection.Input,1);
			if (crossbarOut1!=null && videoCaptureIn1!=null)
			{
				hr=m_graphBuilder.Connect(crossbarOut1, videoCaptureIn1);
				if (hr==0) Log.WriteFile(Log.LogType.Capture,"SWGraph: connected crossbar:0->capture:0");
				else Log.WriteFile(Log.LogType.Capture,true,"SWGraph: FAILED connected crossbar:0->capture:0 0x{0:X}",hr);
			}
			if (crossbarOut2!=null && videoCaptureIn2!=null)
			{
				hr=m_graphBuilder.Connect(crossbarOut2, videoCaptureIn2);
				if (hr==0) Log.WriteFile(Log.LogType.Capture,"SWGraph: connected crossbar:1->capture:1");
				else Log.WriteFile(Log.LogType.Capture,true,"SWGraph: FAILED connected crossbar:1->capture:1 0x{0:X}",hr);
			}

			if (crossbarOut1!=null) 
				Marshal.ReleaseComObject(crossbarOut1);
			if (crossbarOut2!=null) 
				Marshal.ReleaseComObject(crossbarOut2);
			if (videoCaptureIn1!=null) 
				Marshal.ReleaseComObject(videoCaptureIn1);
			if (videoCaptureIn2!=null) 
				Marshal.ReleaseComObject(videoCaptureIn2);
			crossbarOut1=null;
			crossbarOut2=null;
			videoCaptureIn1=null;
			videoCaptureIn2=null;
		}
    

    protected void SetVideoStandard(AnalogVideoStandard standard)
    {
      if (standard==AnalogVideoStandard.None) return;
      if (m_IAMAnalogVideoDecoder==null) return;
      AnalogVideoStandard currentStandard;
      int hr=m_IAMAnalogVideoDecoder.get_TVFormat(out currentStandard);
      if (currentStandard==standard) return;

			if (standard==AnalogVideoStandard.None) standard=AnalogVideoStandard.PAL_B;
			DirectShowUtil.DebugWrite("SinkGraph:Select tvformat:{0}", standard.ToString());
      hr=m_IAMAnalogVideoDecoder.put_TVFormat(standard);
      if (hr!=0) DirectShowUtil.DebugWrite("SinkGraph:Unable to select tvformat:{0}", standard.ToString());
    }

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
			if (m_captureGraphBuilder==null) return;
			if (m_filterCaptureVideo==null) return;

			if(!GUIGraphicsContext.Vmr9Active && Vmr7!=null && m_graphState==State.Viewing)
			{
				Vmr7.Process();
			}
		}
		
		public PropertyPageCollection PropertyPages()
		{
			if (m_filterCaptureVideo==null) return null;;
			if (m_filterCaptureAudio==null) return null;; 
			PropertyPageCollection propertyPages=null;
			{
				try 
				{ 
					SourceCollection VideoSources = new SourceCollection( m_captureGraphBuilder, m_filterCaptureVideo, true );
					SourceCollection AudioSources = new SourceCollection( m_captureGraphBuilder, m_filterCaptureAudio, true );

					// #MW#, difficult to say if this must be changed, as it depends on the loaded
					// filters. The list below is fixed list however... So???
					propertyPages = new PropertyPageCollection( m_captureGraphBuilder, 
						m_filterCaptureVideo, m_filterCaptureAudio, 
						m_filterCompressorVideo, m_filterCompressorAudio, 
						VideoSources, AudioSources, (IBaseFilter)m_TVTuner );

				}
				catch ( Exception ex ) 
				{ 
					Log.WriteFile(Log.LogType.Capture,true, "PropertyPages: FAILED to get property pages." + ex.ToString() ); 
				}

				return( propertyPages );
			}
		}
		
		public IBaseFilter AudiodeviceFilter()
		{
			return m_filterCaptureAudio;
		}

		public bool SupportsFrameSize(Size framesize)
		{	
			if (m_videoCaptureDevice==null) return false;
			m_videoCaptureDevice.SetFrameSize(framesize);
			Size newSize=m_videoCaptureDevice.GetFrameSize();
			return (newSize==framesize);
		}
		public NetworkType Network()
		{
				return NetworkType.Analog;
		}
		public void Tune(object tuningObject, int disecqNo)
		{
		}
		public void StoreChannels(int ID,bool radio, bool tv, ref int newChannels, ref int updatedChannels,ref int newRadioChannels, ref int updatedRadioChannels)
		{
			newChannels=0;
			updatedChannels=0;
			newRadioChannels=0;
			updatedRadioChannels=0;
		}
		public IBaseFilter Mpeg2DataFilter()
		{
			return null;
		}
		
		public void TuneRadioChannel(RadioStation station)
		{
			if (m_TVTuner==null) return;
			SetRecordingLevel(true,0);
			Log.WriteFile(Log.LogType.Capture,"SWGraph:tune to {0} {1} hz", station.Name,station.Frequency);
			
			m_TVTuner.put_TuningSpace(0);
			m_TVTuner.put_CountryCode(m_iCountryCode);
			m_TVTuner.put_Mode(DShowNET.AMTunerModeType.FMRadio);
			if (m_bUseCable)
			{
				m_TVTuner.put_InputType(0, DShowNET.TunerInputType.Cable);
			}
			else
			{
				m_TVTuner.put_InputType(0, DShowNET.TunerInputType.Antenna);
			}
			m_TVTuner.put_Channel((int)station.Frequency, DShowNET.AMTunerSubChannel.Default, DShowNET.AMTunerSubChannel.Default);
			int frequency;
			m_TVTuner.get_AudioFrequency(out frequency);
			SetRecordingLevel(true,_RecordingLevel);
			Log.WriteFile(Log.LogType.Capture,"SWGraph:  tuned to {0} hz", frequency);
		}
		
		public void StartRadio(RadioStation station)
		{
			if (m_graphState != State.Radio)  
			{
				if (m_graphState != State.Created)  return;
				if (m_videoCaptureDevice == null) return ;

				if (Vmr9!=null)
				{
					Vmr9.RemoveVMR9();
					Vmr9.Release();
					Vmr9=null;
				}
				if (Vmr7!=null)
				{
					Vmr7.RemoveVMR7();
					Vmr7=null;
				}

				Log.WriteFile(Log.LogType.Capture,true,"SWGraph:render video preview");
				m_videoCaptureDevice.RenderPreview();
				m_videoWindow = m_graphBuilder as IVideoWindow;
				if (m_videoWindow!=null)
				{
					m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
					m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
					m_videoWindow.put_Visible(DsHlp.OAFALSE);
					m_videoWindow.SetWindowPosition(0,0,1,1);
				}
				
				DsUtils.FixCrossbarRoutingEx(m_graphBuilder,
																			m_captureGraphBuilder,
																			m_filterCaptureVideo, 
																			true, 
																			false, 
																			false, 
																			false ,
																			false ,
																			cardName);

				TuneRadioChannel(station);


				if (m_mediaControl == null)
				{
					m_mediaControl = (IMediaControl)m_graphBuilder;
				}
				if (m_mediaControl != null)
				{
					m_mediaControl.Run();
				}

				m_graphState = State.Radio;

				Log.WriteFile(Log.LogType.Capture,"SWGraph:StartRadio() started");

				return;
			}

			TuneRadioChannel(station);
		}
		public void TuneRadioFrequency(int frequency)
		{
			if (m_TVTuner==null) return;
			Log.WriteFile(Log.LogType.Capture,"SWGraph:tune to {0} hz", frequency);
			SetRecordingLevel(true,0);
			m_TVTuner.put_TuningSpace(0);
			m_TVTuner.put_CountryCode(m_iCountryCode);
			m_TVTuner.put_Mode(DShowNET.AMTunerModeType.FMRadio);
			if (m_bUseCable)
			{
				m_TVTuner.put_InputType(0, DShowNET.TunerInputType.Cable);
			}
			else
			{
				m_TVTuner.put_InputType(0, DShowNET.TunerInputType.Antenna);
			}
			m_TVTuner.put_Channel(frequency, DShowNET.AMTunerSubChannel.Default, DShowNET.AMTunerSubChannel.Default);
			m_TVTuner.get_AudioFrequency(out frequency);
			Log.WriteFile(Log.LogType.Capture,"SWGraph:  tuned to {0} hz", frequency);
			SetRecordingLevel(true,_RecordingLevel);
		}
		public bool HasTeletext()
		{
			return false;
		}
		public int GetAudioLanguage()
		{
			return 0;
		}
		public void SetAudioLanguage(int audioPid)
		{
		}
		public ArrayList GetAudioLanguageList()
		{
			return new ArrayList();
		}
  }
}
