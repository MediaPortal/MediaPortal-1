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
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices; 
using DShowNET;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;

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
      Viewing,
			Radio
    };
    protected int                     m_cardID=-1;
    protected string                  m_strVideoCaptureFilter="";
		protected TVCaptureDevice         mCard;
		protected string									m_strVideoCaptureMoniker="";
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
    protected Size                    m_FrameSize;
    protected double                  m_FrameRate;
    protected IAMVideoProcAmp         m_videoprocamp=null;
    protected VideoProcAmp            m_videoAmp=null;
		protected VMR9Util							  Vmr9=null; 
		protected VMR7Util							  Vmr7=null; 
		protected string                     cardName;
		ArrayList						m_audioPidList=new ArrayList();
		int									SelectedLanguage = 11;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="iCountryCode">country code</param>
    /// <param name="bCable">use Cable or antenna</param>
    /// <param name="strVideoCaptureFilter">Filter name of the capture device</param>
		public SinkGraph(int ID,int iCountryCode,bool bCable,string strVideoCaptureFilter, Size frameSize, double frameRate, string friendlyName)
		{
			cardName=friendlyName;
      m_cardID=ID;
      m_bUseCable=bCable;
      m_iCountryCode=iCountryCode;
      m_graphState=State.None;
      m_strVideoCaptureFilter=strVideoCaptureFilter;
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

		/// <summary>
		/// #MW# Added simple call while passing card object
		/// Easier to handle and to extent...
		/// </summary>
		/// <param name="pCard"></param>
		public SinkGraph(TVCaptureDevice pCard)
		{
			mCard                    = pCard;

			// Add legacy code to be compliant to other call, ie fill in membervariables...
			cardName=pCard.FriendlyName;
			m_graphState             = State.None;
			m_cardID                 = mCard.ID;
			m_bUseCable              = mCard.IsCableInput;
			m_iCountryCode           = mCard.DefaultCountryCode;
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
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public virtual bool CreateGraph(int Quality)
    {
      if (m_graphState!=State.None) return false;
			Vmr9 =new VMR9Util("mytv");
			Vmr7 = new VMR7Util();
			Log.WriteFile(Log.LogType.Capture,"SinkGraph:CreateGraph()");
      GUIGraphicsContext.OnGammaContrastBrightnessChanged +=new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);
      m_iPrevChannel=-1;
//      Log.WriteFile(Log.LogType.Capture,"SinkGraph:CreateGraph()");
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
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
				return false;
			}

      // Make a new filter graph
//      Log.WriteFile(Log.LogType.Capture,"SinkGraph:create new filter graph (IGraphBuilder)");
      m_graphBuilder = (IGraphBuilder) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) ); 

      // Get the Capture Graph Builder
//      Log.WriteFile(Log.LogType.Capture,"SinkGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
      Guid clsid = Clsid.CaptureGraphBuilder2;
      Guid riid = typeof(ICaptureGraphBuilder2).GUID;
      m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance( ref clsid, ref riid ); 

//      Log.WriteFile(Log.LogType.Capture,"SinkGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
      hr = m_captureGraphBuilder.SetFiltergraph( m_graphBuilder );
      if( hr != 0 ) 
      {
        Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:link FAILED");
        return false;
      }
//      Log.WriteFile(Log.LogType.Capture,"SinkGraph:Add graph to ROT table");
      DsROT.AddGraphToRot( m_graphBuilder, out m_rotCookie );

      // Get the video device and add it to the filter graph
//			Log.WriteFile(Log.LogType.Capture,"SinkGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
			try
			{
				m_captureFilter = Marshal.BindToMoniker( videoCaptureDeviceFilter.MonikerString ) as IBaseFilter;
				if (m_captureFilter!=null)
				{
					hr = m_graphBuilder.AddFilter( m_captureFilter, "Video Capture Device" );
					if( hr != 0 ) 
					{
						Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:FAILED:Add Videodevice to filtergraph :0x{0:X}",hr);
						return false;
					}
				}
				else
				{
					Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:FAILED:Unable to create video capture device:{0{", videoCaptureDeviceFilter.Name);
					return false;
				}
			}
			catch(Exception)
			{
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:FAILED:Unable to create video capture device:{0{", videoCaptureDeviceFilter.Name);
				return false;
			}

      // Retrieve the stream control interface for the video device
      // FindInterface will also add any required filters
      // (WDM devices in particular may need additional
      // upstream filters to function).
//      Log.WriteFile(Log.LogType.Capture,"SinkGraph:get Video stream control interface (IAMStreamConfig)");
      object o;
      Guid cat ;
      Guid iid ;

      // Retrieve TV Tuner if available
//      Log.WriteFile(Log.LogType.Capture,"SinkGraph:Find TV Tuner");
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
        Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:CreateGraph() FAILED:no tuner found :0x{0:X}",hr);
      }
//			else
//				Log.WriteFile(Log.LogType.Capture,"SinkGraph:CreateGraph() TV tuner found");

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
      m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder,m_captureGraphBuilder, m_captureFilter);
      

      m_FrameSize=m_videoCaptureDevice.GetFrameSize();

      Log.WriteFile(Log.LogType.Capture,"SinkGraph:capturing:{0}x{1}",m_FrameSize.Width,m_FrameSize.Height);
      m_mpeg2Demux=null;
      if (m_videoCaptureDevice.MPEG2)
      {
        m_mpeg2Demux = new MPEG2Demux(ref m_graphBuilder,m_FrameSize);
      }


      

      // Retreive the media control interface (for starting/stopping graph)
      ConnectVideoCaptureToMPEG2Demuxer();
			if (m_mpeg2Demux!=null)
				m_mpeg2Demux.CreateMappings();
      m_videoprocamp=m_captureFilter as IAMVideoProcAmp;
      if (m_videoprocamp!=null)
      {
        m_videoAmp=new VideoProcAmp(m_videoprocamp);
				m_videoAmp.Contrast=m_videoAmp.ContrastDefault;
				m_videoAmp.Brightness=m_videoAmp.BrightnessDefault;
				m_videoAmp.Gamma=m_videoAmp.GammaDefault;
				m_videoAmp.Saturation=m_videoAmp.SaturationDefault;
				m_videoAmp.Sharpness=m_videoAmp.SharpnessDefault;

      }

			SetQuality(3);//high
			VideoCaptureProperties props = new VideoCaptureProperties(m_captureFilter);
			int minKbps, maxKbps;
			bool isVBR;
			props.GetVideoBitRate(out minKbps, out maxKbps,out isVBR);
			Log.WriteFile(Log.LogType.Capture," driver version:{0} min:{1} peak:{2} vbr:{3}", props.VersionInfo,minKbps, maxKbps,isVBR);

      m_graphState=State.Created;
      return true;
    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
		public virtual void DeleteGraph()
    {
			int hr;
      if (m_graphState < State.Created) return;

      m_iPrevChannel=-1;
      Log.WriteFile(Log.LogType.Capture,"SinkGraph:DeleteGraph()");
      StopRecording();
      StopTimeShifting();
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

      m_videoAmp=null;
      m_videoprocamp=null;
			m_IAMAnalogVideoDecoder = null;
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
			{
        while ((hr=Marshal.ReleaseComObject( m_TVTuner ))>0); 
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_TVTuner):{0}",hr);
				m_TVTuner = null;
			}
			if( m_captureFilter != null )
			{
				while ((hr=Marshal.ReleaseComObject( m_captureFilter ))>0); 
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_captureFilter):{0}",hr);
				m_captureFilter = null;
			}
			if (m_graphBuilder!=null)
				DsUtils.RemoveFilters(m_graphBuilder);

			if( m_rotCookie != 0 )
				DsROT.RemoveGraphFromRot( ref m_rotCookie);
			m_rotCookie=0;


			if( m_captureGraphBuilder != null )
			{
				while ((hr=Marshal.ReleaseComObject( m_captureGraphBuilder ))>0); 
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_captureGraphBuilder):{0}",hr);
				m_captureGraphBuilder = null;
			}
			if( m_graphBuilder != null )
			{
				while ((hr=Marshal.ReleaseComObject( m_graphBuilder ))>0); 
				if (hr!=0) Log.Write("Sinkgraph:ReleaseComobject(m_captureGraphBuilder):{0}",hr);
				m_graphBuilder = null;
			}
      GUIGraphicsContext.form.Invalidate(true);
      GC.Collect();GC.Collect();GC.Collect();

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
    public bool StartTimeShifting(TVChannel channel, string strFileName)
    {
      if (m_graphState!=State.Created && m_graphState!= State.TimeShifting) return false;
      if (m_mpeg2Demux==null) return false;
			m_iCountryCode=channel.Country;

      if (m_graphState==State.TimeShifting) 
      {
				if (channel!=null)
				{
					if (channel.Number!=m_iChannelNr)
					{
						TuneChannel(channel);
					}
					return true;
				}
      }

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
			Log.WriteFile(Log.LogType.Capture,"SinkGraph:StartTimeShifting()");
      m_graphState=State.TimeShifting;
      TuneChannel(channel);
      m_mpeg2Demux.StartTimeshifting(strFileName);
      
      return true;
    }

    /// <summary>
    /// Connects the videocapture->MPEG2 Demuxer
    /// </summary>
    protected void ConnectVideoCaptureToMPEG2Demuxer()
		{
//			Log.WriteFile(Log.LogType.Capture,"SinkGraph:Connect VideoCapture device to MPEG2Demuxer filter");
			if (m_captureFilter==null || m_graphBuilder==null) 
			{
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:ConnectVideoCaptureToMPEG2Demuxer() FAILED capture filter=null");
				return;
			}
			if (m_videoCaptureDevice==null) 
			{
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:ConnectVideoCaptureToMPEG2Demuxer() FAILED videocapturedevice filter=null");
				return;
			}
			if (m_mpeg2Demux!=null) 
			{
				if (m_mpeg2Demux.IsRendered) return;
			}

      // connect video capture pin->mpeg2 demux input
      if (!m_videoCaptureDevice.IsMCEDevice && m_mpeg2Demux!=null)
      {
        Guid cat = PinCategory.Capture;
        int hr=m_captureGraphBuilder.RenderStream( new Guid[1]{ cat}, null/*new Guid[1]{ med}*/, m_captureFilter, null, m_mpeg2Demux.BaseFilter); 
        if (hr==0)
        {
          return;
        }
      }

			if (m_mpeg2Demux!=null)
			{
//				Log.WriteFile(Log.LogType.Capture,"SinkGraph:find MPEG2 demuxer input pin");
				IPin pinIn=DirectShowUtil.FindPinNr(m_mpeg2Demux.BaseFilter,PinDirection.Input,0);
				if (pinIn!=null) 
				{
//					Log.WriteFile(Log.LogType.Capture,"SinkGraph:found MPEG2 demuxer input pin");
					int hr=m_graphBuilder.Connect(m_videoCaptureDevice.CapturePin, pinIn);
					if (hr!=0)
						Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:FAILED to connect Encoder->mpeg2 demuxer:{0:x}",hr);

					Marshal.ReleaseComObject(pinIn);
					pinIn=null;
				}
				else
					Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:FAILED could not find mpeg2 demux input pin");
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
			
			Log.WriteFile(Log.LogType.Capture,"SinkGraph:StopTimeShifting()");
			if (m_mpeg2Demux!=null)
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
    public bool StartRecording(Hashtable attribtutes,TVRecording recording,TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if (m_graphState != State.TimeShifting) return false;
			if (m_mpeg2Demux==null) return false;
			m_iCountryCode=channel.Country;
      if (channel.Number!= m_iChannelNr)
      {
        TuneChannel(channel);
      }
			
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
      Log.WriteFile(Log.LogType.Capture,"SinkGraph:StartRecording({0} {1} {2})",strFileName,bContentRecording,recording.Quality);
			if (recording.Quality != TVRecording.QualityType.NotSet)
			{
				
				if (recording.Quality ==TVRecording.QualityType.Portable)
					SetQuality(0);

				if (recording.Quality ==TVRecording.QualityType.Low)
					SetQuality(1);

				if (recording.Quality ==TVRecording.QualityType.Medium)
					SetQuality(2);

				if (recording.Quality ==TVRecording.QualityType.High)
					SetQuality(3);
			}
      m_mpeg2Demux.Record(attribtutes,strFileName, bContentRecording, timeProgStart,m_StartTime);
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

      Log.WriteFile(Log.LogType.Capture,"SinkGraph:StopRecording()");
      if (m_mpeg2Demux!=null) m_mpeg2Demux.StopRecording();
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
    public bool ShouldRebuildGraph(TVChannel newChannel)
    {
			int iChannel=newChannel.Number;
      // if we switch from tuner <-> SVHS/Composite then 
      // we need to rebuild the capture graph
      bool bFixCrossbar=true;
      if (m_iPrevChannel>=0)
      {
        if (m_iPrevChannel< (int)ExternalInputs.svhs  && iChannel < (int)ExternalInputs.svhs) bFixCrossbar=false;
				if (m_iPrevChannel==(int)ExternalInputs.rgb  && iChannel ==(int)ExternalInputs.rgb) bFixCrossbar=false;
        if (m_iPrevChannel==(int)ExternalInputs.svhs  && iChannel ==(int)ExternalInputs.svhs) bFixCrossbar=false;
        if (m_iPrevChannel==(int)ExternalInputs.cvbs1 && iChannel ==(int)ExternalInputs.cvbs1) bFixCrossbar=false;
        if (m_iPrevChannel==(int)ExternalInputs.cvbs2 && iChannel ==(int)ExternalInputs.cvbs2) bFixCrossbar=false;
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
    public void TuneChannel(TVChannel channel)
    {
      if (m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return;
			bool restartGraph=false;
			try
			{
				if (m_graphState==State.TimeShifting)
				{
					string fname=Recorder.GetTimeShiftFileNameByCardId(m_cardID);
					if (g_Player.Playing && g_Player.CurrentFile == fname)
					{
						restartGraph=true;
						g_Player.PauseGraph();
						m_mpeg2Demux.StopTimeShifting();
					}
				}

				m_iChannelNr=channel.Number;
				m_iCountryCode=channel.Country;
	    
				AnalogVideoStandard standard=channel.TVStandard;
				Log.WriteFile(Log.LogType.Capture,"SinkGraph:TuneChannel() tune to channel:{0} country:{1} standard:{2} name:{3}", 
					m_iChannelNr, m_iCountryCode, standard, channel.Name);

				if (m_iChannelNr < (int)ExternalInputs.svhs)
				{
					if (m_TVTuner==null) return;
					try
					{
						InitializeTuner();
						SetVideoStandard(standard);
	        
	        
						Log.WriteFile(Log.LogType.Capture,"SinkGraph:TuneChannel() tuningspace:0 country:{0} tv standard:{1} cable:{2}",
							m_iCountryCode,standard.ToString(),
							m_bUseCable);
						int currentCountry,iCurrentChannel,iVideoSubChannel,iAudioSubChannel;

						m_TVTuner.get_TVFormat(out standard);
						m_TVTuner.get_Channel(out iCurrentChannel, out iVideoSubChannel, out iAudioSubChannel);
						m_TVTuner.get_CountryCode(out currentCountry);
						if (iCurrentChannel!=m_iChannelNr)
						{
							m_TVTuner.put_Channel(channel.Number,DShowNET.AMTunerSubChannel.Default,DShowNET.AMTunerSubChannel.Default);
							DirectShowUtil.EnableDeInterlace(m_graphBuilder);
						}
						int iFreq;
						double dFreq;
						AMTunerSignalStrength signalStrength;
						m_TVTuner.SignalPresent(out signalStrength);
						m_TVTuner.get_VideoFrequency(out iFreq);
						m_TVTuner.get_Channel(out iCurrentChannel, out iVideoSubChannel, out iAudioSubChannel);
						m_TVTuner.get_CountryCode(out currentCountry);
						m_TVTuner.get_TVFormat(out standard);
						dFreq=iFreq/1000000d;
						Log.WriteFile(Log.LogType.Capture,"SinkGraph:TuneChannel() tuned to channel:{0} county:{1} freq:{2} MHz. tvformat:{3} signal:{4}", 
							iCurrentChannel,currentCountry, dFreq,standard.ToString(),signalStrength.ToString());
				
						
					}
					catch(Exception){} 
				}
				else
				{
					SetVideoStandard(channel.TVStandard);
				}

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
						m_captureFilter, 
						channel.Number<(int)ExternalInputs.svhs, 
						(channel.Number==(int)ExternalInputs.cvbs1), 
						(channel.Number==(int)ExternalInputs.cvbs2), 
						(channel.Number==(int)ExternalInputs.svhs) , 
						(channel.Number==(int)ExternalInputs.rgb) ,
						cardName);
				}
			}
			finally
			{
					if (restartGraph)
					{
						string fname=Recorder.GetTimeShiftFileNameByCardId(m_cardID);
						m_mpeg2Demux.StartTimeshifting(fname);
						g_Player.ContinueGraph();
					}
			}
      m_iPrevChannel=channel.Number;
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
		public bool StartViewing(TVChannel channel)
		{

			Log.WriteFile(Log.LogType.Capture,"SinkGraph:StartViewing()");
			if (m_graphState!=State.Created && m_graphState!=State.Viewing) return false;

			m_iCountryCode=channel.Country;
			if (m_mpeg2Demux==null) 
			{
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:StartViewing() FAILED: no mpeg2 demuxer present");
				return false;
			}
			if (m_videoCaptureDevice==null) 
			{
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:StartViewing() FAILED: no video capture device present");
				return false;
			}
			if (m_graphState==State.Viewing) 
			{
				if (channel.Number!=m_iChannelNr)
				{
					TuneChannel(channel);
				}
				return true;
			}

			// add VMR9 renderer to graph
			if (Vmr9!=null)
			{
				if (Vmr9.UseVMR9inMYTV)
				{
					GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
					GUIWindowManager.SendMessage(msg);
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


			AddPreferredCodecs(true,true);
      
			m_graphState=State.Viewing;
			TuneChannel(channel);
			m_mpeg2Demux.StartViewing(GUIGraphicsContext.form.Handle, Vmr9);

			DirectShowUtil.EnableDeInterlace(m_graphBuilder);
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

			GUIGraphicsContext.OnVideoWindowChanged +=new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			GUIGraphicsContext_OnVideoWindowChanged();

			if (Vmr9!=null)
			{
				if (Vmr9.IsVMR9Connected)	
				{
					Vmr9.SetDeinterlaceMode();
				}
				else
				{
					Vmr9.RemoveVMR9();
					Vmr9.Release();
					Vmr9=null;
				}
			}
		      
      Log.WriteFile(Log.LogType.Capture,"SinkGraph:StartViewing() started ");
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
      Log.WriteFile(Log.LogType.Capture,"SinkGraph:StopViewing()");
			if (Vmr9!=null)
			{
				Vmr9.Enable(false);
			}
			if (m_mpeg2Demux!=null)
				m_mpeg2Demux.StopViewing();
			
			m_graphState=State.Created;
      DeleteGraph();
			GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
			GUIWindowManager.SendMessage(msg);

      return true;
    }

    /// <summary>
    /// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
    /// </summary>
    private void GUIGraphicsContext_OnVideoWindowChanged()
		{
			if (!GUIGraphicsContext.VMR9Allowed) return;
			if (GUIGraphicsContext.Vmr9Active) return;
      if (m_graphState!=State.Viewing ) return ;
      if (m_mpeg2Demux==null) return ;
			
			if (GUIGraphicsContext.BlankScreen)
			{
				m_mpeg2Demux.Overlay=false;
			}
			else
			{
				m_mpeg2Demux.Overlay=true;
			}
			int aspectX, aspectY;
      int iVideoWidth,iVideoHeight;
			m_mpeg2Demux.GetVideoSize( out iVideoWidth, out iVideoHeight );
			m_mpeg2Demux.GetPreferredAspectRatio(out aspectX, out aspectY);
			GUIGraphicsContext.VideoSize=new Size(iVideoWidth, iVideoHeight );
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
				m_geometry.GetWindow(aspectX,aspectY,out rSource, out rDest);
				rDest.X += (int)x;
				rDest.Y += (int)y;

				Log.Write("overlay: video WxH  : {0}x{1}",iVideoWidth,iVideoHeight);
				Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
				Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
				Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
				Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
				Log.Write("overlay: src        : ({0},{1})-({2},{3})",
					rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
				Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
					rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);

				if (rSource.Left< 0 || rSource.Top<0 || rSource.Width<=0 || rSource.Height<=0) return;
				if (rDest.Left <0 || rDest.Top < 0 || rDest.Width<=0 || rDest.Height<=0) return;
				m_mpeg2Demux.SetSourcePosition( rSource.Left,rSource.Top,rSource.Width,rSource.Height);
				m_mpeg2Demux.SetDestinationPosition(0,0,rDest.Width,rDest.Height );
				m_mpeg2Demux.SetWindowPosition(rDest.Left,rDest.Top,rDest.Width,rDest.Height);
			}
			else
			{
				if ( GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 || 
					GUIGraphicsContext.VideoWindow.Width <=0 || GUIGraphicsContext.VideoWindow.Height <=0) return;
				if (iVideoHeight<=0 || iVideoWidth<=0) return;
        
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
    void AddPreferredCodecs(bool audio, bool video)
    {				
      // add preferred video & audio codecs
      string strVideoCodec="";
			string strAudioCodec="";
			string strAudioRenderer="";
      bool   bAddFFDshow=false;

      using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        bAddFFDshow=xmlreader.GetValueAsBool("mytv","ffdshow",false);
        strVideoCodec=xmlreader.GetValueAsString("mytv","videocodec","");
				strAudioCodec=xmlreader.GetValueAsString("mytv","audiocodec","");
				strAudioRenderer=xmlreader.GetValueAsString("mytv","audiorenderer","");
      }
      if (video&&strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strVideoCodec);
			if (audio&&strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strAudioCodec);
			if (audio && strAudioRenderer.Length>0) DirectShowUtil.AddAudioRendererToGraph(m_graphBuilder,strAudioRenderer,false);
      if (video&&bAddFFDshow) DirectShowUtil.AddFilterToGraph(m_graphBuilder,"ffdshow raw video filter");
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
      if (m_graphState!=State.Radio&&m_graphState!=State.Recording && m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return false;
      if (m_TVTuner==null) return true;
      AMTunerSignalStrength strength;
      m_TVTuner.SignalPresent(out strength);
      return ( ( (int)strength ) >=1 );
    }

		public int  SignalQuality()
		{
			if (m_graphState!=State.Radio&&m_graphState!=State.Recording && m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return 1;
			if (m_TVTuner==null) return 1;
			AMTunerSignalStrength strength;
			m_TVTuner.SignalPresent(out strength);
			if (strength==AMTunerSignalStrength.SignalPresent) return 100;
			return 1;
		}
		
		public int  SignalStrength()
		{
			if (m_graphState!=State.Radio&&m_graphState!=State.Recording && m_graphState!=State.TimeShifting && m_graphState!=State.Viewing) return 1;
			if (m_TVTuner==null) return 1;
			AMTunerSignalStrength strength;
			m_TVTuner.SignalPresent(out strength);
			if (strength==AMTunerSignalStrength.SignalPresent) return 100;
			return 1;
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
				m_videoAmp.Sharpness = GUIGraphicsContext.Sharpness;
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

		/// <summary>
		/// Sets the tv standard used (pal,ntsc,secam,...) for the video decoder
		/// </summary>
		/// <param name="standard">TVStandard</param>
    protected void SetVideoStandard(AnalogVideoStandard standard)
    {
			VideoCaptureProperties props = new VideoCaptureProperties(m_captureFilter);
			props.SetTvFormat(standard);
      if (standard==AnalogVideoStandard.None) return;

      if (m_IAMAnalogVideoDecoder==null) return;
      AnalogVideoStandard currentStandard;
      int hr=m_IAMAnalogVideoDecoder.get_TVFormat(out currentStandard);
      if (currentStandard==standard) return;

      Log.WriteFile(Log.LogType.Capture,"SinkGraph:Select tvformat:{0}", standard.ToString());
      if (standard==AnalogVideoStandard.None) standard=AnalogVideoStandard.PAL_B;
      hr=m_IAMAnalogVideoDecoder.put_TVFormat(standard);
      if (hr!=0) 
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraph:Unable to select tvformat:{0}", standard.ToString());
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
		
		void UpdateVideoState()
		{
			//check if this card is used for watching tv
			bool isViewing=Recorder.IsCardViewing(m_cardID);
			if (!isViewing) return;

			if (!SignalPresent())
			{
				VideoRendererStatistics.VideoState=VideoRendererStatistics.State.NoSignal;
				return;
			}
			if(GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS < 1f)
			{
				if  ( (g_Player.Playing && !g_Player.Paused) || (!g_Player.Playing) )
				{
					VideoRendererStatistics.VideoState=VideoRendererStatistics.State.NoSignal;
				}
				else
				{
					// todo: check for vmr7 is we are receiving video 
					VideoRendererStatistics.VideoState=VideoRendererStatistics.State.VideoPresent;
				}
			}
			else
				VideoRendererStatistics.VideoState=VideoRendererStatistics.State.VideoPresent;
		}

		public void Process()
		{
			if (!GUIGraphicsContext.VMR9Allowed) return;
			if (m_captureGraphBuilder==null) return;
			if (m_captureFilter==null) return;

			if(!GUIGraphicsContext.Vmr9Active && Vmr7!=null && m_graphState==State.Viewing)
			{
				Vmr7.Process();
			}

			UpdateVideoState();
		}
		
		public PropertyPageCollection PropertyPages()
		{
			
			PropertyPageCollection propertyPages=null;
			{
				if (m_captureGraphBuilder==null) return null;
				if (m_captureFilter==null) return null;
				try 
				{ 
					SourceCollection VideoSources = new SourceCollection( m_captureGraphBuilder, m_captureFilter, true );

					// #MW#, difficult to say if this must be changed, as it depends on the loaded
					// filters. The list below is fixed list however... So???
					propertyPages = new PropertyPageCollection( m_captureGraphBuilder, 
						m_captureFilter, null, 
						null, null, 
						VideoSources, null, null );

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
			return null;
		}

		public bool SupportsFrameSize(Size framesize)
		{	
			return false;
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
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:tune to {0} {1} hz", station.Name,station.Frequency);
			
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
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  tuned to {0} hz", frequency);
		}
		
		public void StartRadio(RadioStation station)
		{
			if (m_graphState!=State.Radio) 
			{
				if (m_graphState!=State.Created)  return;
				if (m_mpeg2Demux==null) return ;
				if (m_videoCaptureDevice==null) return ;
				if (Vmr9!=null)
				{
					Vmr9.RemoveVMR9();
					Vmr9=null;
				}
				if (Vmr7!=null)
				{
					Vmr7.RemoveVMR7();
					Vmr7=null;
				}
				
				AddPreferredCodecs(true,false);

				DsUtils.FixCrossbarRoutingEx(m_graphBuilder,
					m_captureGraphBuilder,
					m_captureFilter, 
					true, 
					false, 
					false, 
					false ,
					false ,
					cardName);
				TuneRadioChannel(station);
				m_mpeg2Demux.StartListening();


				Log.WriteFile(Log.LogType.Capture,"SinkGraph:StartRadio() started");
				m_graphState=State.Radio;
				return;
			}
			TuneRadioChannel(station);
		}
		
		public void TuneRadioFrequency(int frequency)
		{
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:tune to {0} hz", frequency);
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
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  tuned to {0} hz", frequency);
		}

		protected void SetQuality(int Quality)
		{
			string filename=String.Format(@"database\card_{0}.xml",mCard.FriendlyName);
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml(filename))
			{	
				bool enabled=xmlreader.GetValueAsBool("quality","enabled",false);
				if (!enabled) return;

				int portableMinKbps=xmlreader.GetValueAsInt("quality", "portLow", 100);
				int portableMaxKbps=xmlreader.GetValueAsInt("quality", "portMax", 300);
				bool portableVBR=xmlreader.GetValueAsBool("quality", "portVBR", false);

				int lowMinKbps=xmlreader.GetValueAsInt("quality", "LowLow", 500);
				int lowMaxKbps=xmlreader.GetValueAsInt("quality", "LowMax", 1500);
				bool lowVBR=xmlreader.GetValueAsBool("quality", "LowVBR", true);	

				int mediumMinKbps=xmlreader.GetValueAsInt("quality", "MedLow", 2000);
				int mediumMaxKbps=xmlreader.GetValueAsInt("quality", "MedMax", 4000);
				bool mediumVBR=xmlreader.GetValueAsBool("quality", "MedVBR", false);

				int highMinKbps=xmlreader.GetValueAsInt("quality", "HighLow", 8000);
				int highMaxKbps=xmlreader.GetValueAsInt("quality", "HighMax", 12000);
				bool highVBR=xmlreader.GetValueAsBool("quality", "HighVBR", true);

				string comName=this.mCard.CommercialName;
				if (comName.IndexOf("usb")>=0)
				{
					highMinKbps=xmlreader.GetValueAsInt("quality", "HighLow", 768);
					highMaxKbps=xmlreader.GetValueAsInt("quality", "HighMax", 4000);
				}


				VideoCaptureProperties props = new VideoCaptureProperties(m_captureFilter);
				if (Quality>=0)
				{
					switch (Quality)
					{
						case 0://Portable
							Log.WriteFile(Log.LogType.Capture,"SinkGraph:Set quality:portable");
							props.SetVideoBitRate(portableMinKbps,portableMaxKbps,portableVBR);
							break;
						case 1://low
							Log.WriteFile(Log.LogType.Capture,"SinkGraph:Set quality:low");
							props.SetVideoBitRate(lowMinKbps,lowMaxKbps,lowVBR);
							break;
						case 2://medium
							Log.WriteFile(Log.LogType.Capture,"SinkGraph:Set quality:medium");
							props.SetVideoBitRate(mediumMinKbps,mediumMaxKbps,mediumVBR);
							break;
						case 3://hi
							Log.WriteFile(Log.LogType.Capture,"SinkGraph:Set quality:high");
							props.SetVideoBitRate(highMinKbps,highMaxKbps,highVBR);
							break;
								
						default://
							Log.WriteFile(Log.LogType.Capture,"SinkGraph:Set quality to default (medium)");
							props.SetVideoBitRate(mediumMinKbps,mediumMaxKbps,mediumVBR);
							break;
					}
					int minKbps, maxKbps;
					bool isVBR;
					props.GetVideoBitRate(out minKbps, out maxKbps,out isVBR);
					Log.WriteFile(Log.LogType.Capture," driver version:{0} min:{1} peak:{2} vbr:{3}", props.VersionInfo,minKbps, maxKbps,isVBR);
				}
			}//if (Quality>=0)
		}//protected void SetQuality(int Quality)

		public bool HasTeletext()
		{
			return false;
		}
		
		public int GetAudioLanguage()
		{
			return SelectedLanguage;
		}
		public void SetAudioLanguage(int audioPid)
		{
			SelectedLanguage = audioPid;
		}
		public ArrayList GetAudioLanguageList()
		{
#if DEBUG
			DVBSections.AudioLanguage al;
			m_audioPidList.Clear();

			// Add two debug languages
			al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
			al.AudioPid=10;
			al.AudioLanguageCode="eng";
			m_audioPidList.Add(al);

			al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
			al.AudioPid=11;
			al.AudioLanguageCode="dut";
			m_audioPidList.Add(al);

#endif
			return m_audioPidList;
		}
  }
}
