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
using Microsoft.Win32;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
using DShowNET;
using DirectX.Capture;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// 
	/// </summary>
	public class AIWGraph : MediaPortal.TV.Recording.IGraph
	{
		[ComImport, Guid("758C0F02-DF95-11D2-8E75-00104B93CF06")]
			class ATIVideoEncode {};
		[ComImport, Guid("6467DD70-FBD5-11D2-B5B6-444553540000")]
			class ATIAudioEncode {};
		[ComImport, Guid("5CB0B6B4-E857-40C5-92CF-715F05DE33D3")]
			class ATIMCEAudioEncode {};
		[ComImport, Guid("03684DED-3346-4530-8B46-10957AAD5588")]
			class ATIMCEVideoEncode {};

		enum State
		{ 
			None, 
			Created, 
			Recording, 
			Viewing,
			Radio
		};

		int m_iCurrentChannel = 28;
		int m_iCountryCode = 31;
		bool m_bUseCable = false;
		State m_graphState = State.None;
		string m_strVideoCaptureFilter = "";
		string m_strAudioCaptureFilter = "";
		string m_strVideoCompressor = "";
		string m_strAudioCompressor = "";
		string m_strAudioInputPin = "";
		IGraphBuilder m_graphBuilder = null;
		ICaptureGraphBuilder2 m_captureGraphBuilder = null;
		IBaseFilter m_filterCaptureVideo = null;
		IBaseFilter m_filterCaptureAudio = null;
    
		IFileSinkFilter	        m_fileWriterFilter = null; // DShow Filter: file writer
		IBaseFilter		          m_muxFilter = null; // DShow Filter: multiplexor (combine video and audio streams)
		IBaseFilter             m_filterCompressorVideo = null;
		IBaseFilter             m_filterCompressorAudio = null;
		IBaseFilter                m_atiVideoEncode = null;
		IBaseFilter                m_atiAudioEncode = null;
		IPin                       m_videoEncodeOutput = null;
		IPin                       m_audioEncodeOutput = null;
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
		protected string                     cardName;

		const int WS_CHILD = 0x40000000;
		const int WS_CLIPCHILDREN = 0x02000000;
		const int WS_CLIPSIBLINGS = 0x04000000;

		public AIWGraph(int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel, string friendlyName)
		{
			cardName=friendlyName;
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
		}

		/// <summary>
		/// Creates a new DirectShow graph for the TV capturecard
		/// </summary>
		/// <returns>bool indicating if graph is created or not</returns>
		public bool CreateGraph(int Quality)
		{
			if (m_graphState != State.None) return false;
			DirectShowUtil.DebugWrite("AIWGraph:CreateGraph()");

			// find the video capture device
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

			if (filterVideoCaptureDevice == null) 
			{
				DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strVideoCaptureFilter);
				return false;
			}
			if (filterAudioCaptureDevice == null && m_strAudioCaptureFilter.Length > 0) 
			{
				DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() FAILED couldnt find capture device:{0}",m_strAudioCaptureFilter);
				return false;
			}

			// Make a new filter graph
			DirectShowUtil.DebugWrite("AIWGraph:create new filter graph (IGraphBuilder)");
			m_graphBuilder = (IGraphBuilder) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true));

			// Get the Capture Graph Builder
			DirectShowUtil.DebugWrite("AIWGraph:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
			Guid clsid = Clsid.CaptureGraphBuilder2;
			Guid riid = typeof(ICaptureGraphBuilder2).GUID;
			m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref riid);

			DirectShowUtil.DebugWrite("AIWGraph:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
			int hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
			if (hr < 0) 
			{
				DirectShowUtil.DebugWrite("AIWGraph:link FAILED:0x{0:X}",hr);
				return false;
			}
			DirectShowUtil.DebugWrite("AIWGraph:Add graph to ROT table");
			DsROT.AddGraphToRot(m_graphBuilder, out m_rotCookie);

			// Get the video device and add it to the filter graph
			DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() add capture device {0}",m_strVideoCaptureFilter);
			m_filterCaptureVideo = Marshal.BindToMoniker(filterVideoCaptureDevice.MonikerString) as IBaseFilter;
			if (m_filterCaptureVideo != null)
			{
				hr = m_graphBuilder.AddFilter(m_filterCaptureVideo, filterVideoCaptureDevice.Name);
				if (hr < 0) 
				{
					DirectShowUtil.DebugWrite("AIWGraph:FAILED:Add Videodevice to filtergraph:0x{0:X}",hr);
					return false;
				}
			}

			// Get the audio device and add it to the filter graph
			if (filterAudioCaptureDevice != null)
			{
				// Get the audio device and add it to the filter graph
				DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() add capture device {0}",m_strAudioCaptureFilter);
				m_filterCaptureAudio = Marshal.BindToMoniker(filterAudioCaptureDevice.MonikerString) as IBaseFilter;
				if (m_filterCaptureAudio != null)
				{
					hr = m_graphBuilder.AddFilter(m_filterCaptureAudio, filterAudioCaptureDevice.Name);
					if (hr < 0) 
					{
						DirectShowUtil.DebugWrite("AIWGraph:FAILED:Add audiodevice to filtergraph:0x{0:X}",hr);
						return false;
					}
				}
			}

			// Retrieve TV Tuner if available
			DirectShowUtil.DebugWrite("AIWGraph:Find TV Tuner");
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
				DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() FAILED:no tuner found");
			}

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
				for (int ipin=0; ipin < 10; ipin++)
				{
					IPin pin=DirectShowUtil.FindPinNr( (IBaseFilter)m_TVTuner,PinDirection.Output,ipin);
					if (pin!=null)
					{
						IPin pConnectPin=null;
						hr=pin.ConnectedTo(out pConnectPin);  
						if (hr!= 0 || pConnectPin==null)
						{
							//no? then connect all tvtuner outputs
							ConnectTVTunerOutputs();
							break;
						}
					}
					else break;
				}
			}

			m_videoCaptureDevice = new VideoCaptureDevice(m_graphBuilder, m_captureGraphBuilder, m_filterCaptureVideo);


			//set the frame size
			m_videoCaptureDevice.SetFrameSize(m_FrameSize);
			m_videoCaptureDevice.SetFrameRate(m_FrameRate);

			m_IAMAnalogVideoDecoder = m_filterCaptureVideo as IAMAnalogVideoDecoder;

			m_graphState = State.Created;
			return true;
		}

		/// <summary>
		/// The normal overlay mixer does not seem to work for most AIW's
		/// always use OverlayMixer2, this function disconnects and remove the overlay mixer if found
		/// and always connects the OverlayMixer2 to the capture card
		/// </summary>
		/// <param name="captureFilter">The AIW Capture filter</param>
		private bool SwapOverlayMixerForOverlayMixer2(IBaseFilter captureFilter, ICaptureGraphBuilder2 m_captureGraphBuilder, IGraphBuilder m_graphBuilder, bool bTimeShifting)
		{
			int hr;

			// Disconnect the video preview from video port manager, overlay mixer or overlay mixer2
			IPin pin = DirectShowUtil.FindPin(m_filterCaptureVideo, PinDirection.Output, "VP");
			pin.Disconnect();

			// Remove the overlay mixer
			IBaseFilter m_overlayMixer;
			hr = m_graphBuilder.FindFilterByName("Overlay Mixer", out m_overlayMixer);
			if (hr == 0)
			{
				m_graphBuilder.RemoveFilter(m_overlayMixer);
			}
		
			// If the overlay mixer2 exists then reconnect
			// otherwise create and add to graph.
			IBaseFilter m_overlayMixer2;
			hr = m_graphBuilder.FindFilterByName("Overlay Mixer2", out m_overlayMixer2);
	
			if (hr == 0)
			{
				m_graphBuilder.Render(pin);
				DirectShowUtil.RenderOutputPins(m_graphBuilder, m_overlayMixer2);	

				if (bTimeShifting)
				{
					// Remove the video port manager if it exists
					IBaseFilter VideoPortManager = null;
					hr = m_graphBuilder.FindFilterByName("Video Port Manager", out VideoPortManager);
					if (hr == 0)
					{
						m_graphBuilder.RemoveFilter(VideoPortManager);
					}

					IBaseFilter VideoRenderer = null;
					hr = m_graphBuilder.FindFilterByName("Video Renderer", out VideoRenderer);
					if (hr == 0)
					{
						m_graphBuilder.RemoveFilter(VideoRenderer);
					}
				}

				return true;
			} 
			else 
			{
				Filters filters = new Filters();
				foreach(Filter legacyFilter in filters.LegacyFilters)
				{
					if(legacyFilter.Name == "Overlay Mixer2")
					{
						// Get the overlay mixer and add it to the filter graph
						Log.WriteFile(Log.LogType.Capture,"AIWGraph:CreateGraph() add overlay mixer {0}", legacyFilter.Name);
						m_overlayMixer2 = Marshal.BindToMoniker(legacyFilter.MonikerString) as IBaseFilter;
				
						hr = m_graphBuilder.AddFilter(m_overlayMixer2, "Overlay Mixer2");

						if (hr < 0) 
						{
							Log.WriteFile(Log.LogType.Capture,"AIWGraph:FAILED:Add overlay mixer2 to filtergraph:0x{0:X}",hr);
							return false;
						}

						m_graphBuilder.Render(pin);
						DirectShowUtil.RenderOutputPins(m_graphBuilder, m_overlayMixer2);

						if (bTimeShifting)
						{
							// Remove the video port manager if it exists
							IBaseFilter VideoPortManager = null;
							hr = m_graphBuilder.FindFilterByName("Video Port Manager", out VideoPortManager);
							if (hr == 0)
							{
								m_graphBuilder.RemoveFilter(VideoPortManager);
							}

							IBaseFilter VideoRenderer = null;
							hr = m_graphBuilder.FindFilterByName("Video Renderer", out VideoRenderer);
							if (hr == 0)
							{
								m_graphBuilder.RemoveFilter(VideoRenderer);
							}
						}

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// </summary>
		private void AddATIFilters()
		{
			// Try to add the standard ATI MMC Filters
			try
			{
				m_atiVideoEncode = (IBaseFilter) new ATIVideoEncode();
				m_atiAudioEncode = (IBaseFilter) new ATIAudioEncode();
			}
			catch (Exception)
			{
			}

			if (m_atiVideoEncode == null)
			{
				// Try to add the standard ATI MMC Filters
				try
				{
					m_atiVideoEncode = (IBaseFilter) new ATIMCEVideoEncode();
					m_atiAudioEncode = (IBaseFilter) new ATIMCEAudioEncode();
				}
				catch (Exception)
				{
				}
			}

			IBaseFilter m_atiVideoFilter = (IBaseFilter) m_atiVideoEncode;
			if (m_atiVideoFilter == null)
			{
				Log.WriteFile(Log.LogType.Capture,"AIWGraph:FAILED to add ati video encoder");
				return;
			}

			m_graphBuilder.AddFilter(m_atiVideoFilter, "ATI Video Encode");

			// Add the ATI Audio Filter

			IBaseFilter m_atiAudioFilter = (IBaseFilter) m_atiAudioEncode;
			if (m_atiAudioFilter ==null)
			{
				Log.WriteFile(Log.LogType.Capture,"AIWGraph:FAILED to add ati audio encoder");
				return;
			}

			m_graphBuilder.AddFilter(m_atiAudioFilter, "ATI Audio Encode");

			// Find the video capture pin
			IPin videoOutput = null;
			videoOutput = DirectShowUtil.FindPinNr(m_filterCaptureVideo, PinDirection.Output,0);

			// Find the audio capture pin
			IPin audioOutput = null;
			audioOutput = DirectShowUtil.FindPinNr(m_filterCaptureAudio, PinDirection.Output,0);

			// Find the video encoder input pin
			IPin videoEncodeInput= null;
			videoEncodeInput = DirectShowUtil.FindPinNr(m_atiVideoFilter, PinDirection.Input,0);

			// Find the audio encoder input pin
			IPin audioEncodeInput = null;
			audioEncodeInput = DirectShowUtil.FindPinNr(m_atiAudioFilter, PinDirection.Input,0);

			// Find the video encoder output pin (we will use this pin to connect to the
			//StreamBuffer portion engine of the graph. In particular the video analyser
			m_videoEncodeOutput = DirectShowUtil.FindPinNr(m_atiVideoFilter, PinDirection.Output,0);

			// Find the audio encoder output pin (we will use this pin to connect to the
			// StreamBuffer portion engine of the the graph in particular the StreamBufferSink
			m_audioEncodeOutput = DirectShowUtil.FindPinNr(m_atiAudioFilter, PinDirection.Output,0);


			// Connect video capture pin to encoder
			m_graphBuilder.Connect(videoOutput, videoEncodeInput);

			// Connect audio capture pin to encoder
			m_graphBuilder.Connect(audioOutput, audioEncodeInput);


			// Find and render preview pin the ATI capture graph won't work without it!
			IPin previewPin = null;
			previewPin = DirectShowUtil.FindPinNr(m_filterCaptureVideo, PinDirection.Output, 1);
			m_graphBuilder.Render(previewPin);
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

			DirectShowUtil.DebugWrite("AIWGraph:DeleteGraph()");
			StopRecording();
			StopViewing();

			if (m_mediaControl != null)
			{
				m_mediaControl.Stop();
			}
			if (m_videoWindow != null)
			{
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
				//m_videoWindow.put_Owner(IntPtr.Zero);
				m_videoWindow = null;
			}

			if (m_videoCaptureDevice != null)
			{
				m_videoCaptureDevice.CloseInterfaces();
				m_videoCaptureDevice = null;
			}
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
		public bool StartRecording(Hashtable attribtutes,TVRecording recording,TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
		{
			if (m_graphState == State.Recording) return true;
			if (m_graphState != State.Created) return false;
			m_iCountryCode=channel.Country;

			SetRegistryThings();
			int hr;
			DirectShowUtil.DebugWrite("AIWGraph:Start recording...");
			Filters filters = new Filters();
			Filter filterVideoCompressor = null;
			Filter filterAudioCompressor = null;

			bool bRecordWMV = false;
			strFileName = System.IO.Path.ChangeExtension(strFileName, ".avi");
      
			DirectShowUtil.DebugWrite("AIWGraph:find video compressor filter...");
			foreach (Filter filter in filters.VideoCompressors)
			{
				if (filter.Name.Equals(m_strVideoCompressor))
				{
					filterVideoCompressor = filter;
					// check for wmv 7,8,9
					if (filter.MonikerString.Equals("@device:dmo:{3181343B-94A2-4FEB-ADEF-30A1DDE617B4}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") || 
						filter.MonikerString.Equals("@device:dmo:{BA2F0162-CAA4-48FC-89EA-DB0D1DFF40CA}{33D9A760-90C8-11D0-BD43-00A0C911CE86}") || 
						filter.MonikerString.Equals("@device:dmo:{96B57CDD-8966-410C-BB1F-C97EEA765C04}{33D9A760-90C8-11D0-BD43-00A0C911CE86}"))
					{
						bRecordWMV = true;
						strFileName = System.IO.Path.ChangeExtension(strFileName, ".wmv");
					}
					break;
				}
			}
      
			DirectShowUtil.DebugWrite("AIWGraph:find audio compressor filter...");
			foreach (Filter filter in filters.AudioCompressors)
			{
				if (filter.Name.Equals(m_strAudioCompressor))
				{
					filterAudioCompressor = filter;
					break;
				}
			}

			if (filterVideoCompressor == null) 
			{
				DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() FAILED couldnt find video compressor:{0}",m_strVideoCompressor);
				return false;
			}

			if (filterAudioCompressor == null) 
			{
				DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() FAILED couldnt find audio compressor:{0}",m_strAudioCompressor);
				return false;
			}

			// add the video/audio compressor filters
			DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() add video compressor {0}",m_strVideoCompressor);
			m_filterCompressorVideo = Marshal.BindToMoniker(filterVideoCompressor.MonikerString) as IBaseFilter;
			if (m_filterCompressorVideo != null)
			{
				hr = m_graphBuilder.AddFilter(m_filterCompressorVideo, filterVideoCompressor.Name);
				if (hr < 0) 
				{
					DirectShowUtil.DebugWrite("AIWGraph:FAILED:Add video compressor to filtergraph:0x{0:X}",hr);
					return false;
				}
			}
			else
			{
				DirectShowUtil.DebugWrite("AIWGraph:FAILED:Add video compressor to filtergraph");
				return false;
			}

			DirectShowUtil.DebugWrite("AIWGraph:CreateGraph() add audio compressor {0}",m_strAudioCompressor);
			m_filterCompressorAudio = Marshal.BindToMoniker(filterAudioCompressor.MonikerString) as IBaseFilter;
			if (m_filterCompressorAudio != null)
			{
				hr = m_graphBuilder.AddFilter(m_filterCompressorAudio, filterAudioCompressor.Name);
				if (hr < 0) 
				{
					DirectShowUtil.DebugWrite("AIWGraph:FAILED:Add audio compressor to filtergraph");
					return false;
				}
			}
			else
			{
				DirectShowUtil.DebugWrite("AIWGraph:FAILED:Add audio compressor to filtergraph:0x{0:X}",hr);
				return false;
			}

			// select the correct audio input pin to capture
			if (m_filterCaptureAudio != null)
			{
				if (m_strAudioInputPin.Length > 0)
				{
					DirectShowUtil.DebugWrite("AIWGraph:set audio input pin:{0}", m_strAudioInputPin);
					IPin pinInput = DirectShowUtil.FindPin(m_filterCaptureAudio, PinDirection.Input, m_strAudioInputPin);
					if (pinInput == null)
					{
						DirectShowUtil.DebugWrite("AIWGraph:FAILED audio input pin:{0} not found", m_strAudioInputPin);
					}
					else
					{
						IAMAudioInputMixer mixer = pinInput as IAMAudioInputMixer;
						if (mixer != null)
						{
							hr = mixer.put_Enable(true);
							if (hr != 0)
							{
								DirectShowUtil.DebugWrite("AIWGraph:FAILED:to enable audio input pin:0x{0:X}",hr);
							}
							else
							{
								DirectShowUtil.DebugWrite("AIWGraph:enabled audio input pin:{0}",m_strAudioInputPin);
							}

							double fLevel=((double)_RecordingLevel);
							fLevel /= 100.0d;
							hr = mixer.put_MixLevel(fLevel);
							if (hr != 0)
							{
								DirectShowUtil.DebugWrite("AIWGraph:FAILED:to set mixing level to {0}%:0x{1:X}",_RecordingLevel,hr);
							}
							else
							{
								DirectShowUtil.DebugWrite("AIWGraph:set mixing level to {0}% of pin:{1}",_RecordingLevel,m_strAudioInputPin);
							}

						}
						else
						{
							DirectShowUtil.DebugWrite("AIWGraph:FAILED audio input pin:{0} does not expose an IAMAudioInputMixer", m_strAudioInputPin);
						}
					}
				}
			}
    
			// set filename
			DirectShowUtil.DebugWrite("AIWGraph:record to :{0} ", strFileName);

			Guid cat, med;
			Guid mediaSubTypeAvi = MediaSubType.Avi;
			if (bRecordWMV)
				mediaSubTypeAvi = MediaSubType.Asf;

			hr = m_captureGraphBuilder.SetOutputFileName(ref mediaSubTypeAvi, strFileName, out m_muxFilter, out m_fileWriterFilter);
			if (hr != 0)
			{
				DirectShowUtil.DebugWrite("AIWGraph:FAILED:to set output filename to :{0} :0x{1:X}", strFileName, hr);
				return false;
			}

			if (bRecordWMV)
			{
				DirectShowUtil.DebugWrite("AIWGraph:get IConfigAsfWriter");
				IConfigAsfWriter asfwriter = m_fileWriterFilter as IConfigAsfWriter;
				if (asfwriter != null)
				{
					DirectShowUtil.DebugWrite("AIWGraph:IConfigAsfWriter.SetProfile(BestVBRVideo)");
					//Guid WMProfile_V80_HIGHVBRVideo = new Guid( 0xf10d9d3,0x3b04,0x4fb0,0xa3, 0xd3, 0x88, 0xd4, 0xac, 0x85, 0x4a, 0xcc);
					Guid WMProfile_V80_BESTVBRVideo = new Guid(0x48439ba, 0x309c, 0x440e, 0x9c, 0xb4, 0x3d, 0xcc, 0xa3, 0x75, 0x64, 0x23);
					hr = asfwriter.ConfigureFilterUsingProfileGuid(WMProfile_V80_BESTVBRVideo);
					if (hr != 0)
					{
						DirectShowUtil.DebugWrite("AIWGraph:FAILED IConfigAsfWriter.SetProfile() :0x{0:X}", hr);
					}
				}
				else DirectShowUtil.DebugWrite("AIWGraph:FAILED:to get IConfigAsfWriter");
			}

			if (m_videoCaptureDevice.CapturePin != null)
			{
				// NOTE that we try to render the interleaved pin before the video pin, because
				// if BOTH exist, it's a DV filter and the only way to get the audio is to use
				// the interleaved pin.  Using the Video pin on a DV filter is only useful if
				// you don't want the audio.
				DirectShowUtil.DebugWrite("AIWGraph:videocap:connect video capture->compressor (interleaved)");
				cat = PinCategory.Capture;
				med = MediaType.Interleaved;
				hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureVideo, m_filterCompressorVideo, m_muxFilter);
				if (hr != 0)
				{
					DirectShowUtil.DebugWrite("AIWGraph:videocap:connect video capture->compressor (video)");
					cat = PinCategory.Capture;
					med = MediaType.Video;
					hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureVideo, m_filterCompressorVideo, m_muxFilter);
					if (hr != 0)
					{
						DirectShowUtil.DebugWrite("AIWGraph:FAILED:videocap:to connect video capture->compressor :0x{0:X}",hr);
						return false;
					}
				}

				if (m_filterCaptureAudio == null)
				{
					DirectShowUtil.DebugWrite("AIWGraph:videocap:connect audio capture->compressor ");
					cat = PinCategory.Capture;
					med = MediaType.Audio;
					hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureVideo, m_filterCompressorAudio, m_muxFilter);
					if (hr == 0)
					{
						DirectShowUtil.DebugWrite("AIWGraph:videocap:connect audio capture->compressor :succeeded");
					}
				}
			}


			if (m_filterCaptureAudio != null)
			{
				DirectShowUtil.DebugWrite("AIWGraph:audiocap:connect audio capture->compressor ");
				cat = PinCategory.Capture;
				med = MediaType.Audio;
				hr = m_captureGraphBuilder.RenderStream(new Guid[1] { cat}, new Guid[1] { med}, m_filterCaptureAudio, m_filterCompressorAudio, m_muxFilter);
				if (hr != 0)
				{
					DirectShowUtil.DebugWrite("AIWGraph:FAILED:audiocap:to connect audio capture->compressor :0x{0:X}",hr);
					return false;
				}
			} 

			// Set the audio as the masterstream
			if (!bRecordWMV)
			{
				// set avi muxing parameters
				IConfigAviMux ConfigMux = m_muxFilter as IConfigAviMux;
				if (ConfigMux != null)
				{
					DirectShowUtil.DebugWrite("AIWGraph:set audio as masterstream");
					hr = ConfigMux.SetMasterStream(1);
					if (hr != 0)
					{
						DirectShowUtil.DebugWrite("AIWGraph:FAILED:to set audio as masterstream:0x{0:X}",hr);
					}
				}
				else
				{
					DirectShowUtil.DebugWrite("AIWGraph:FAILED:to get IConfigAviMux");
				}

				// Set the avi interleaving mode
				IConfigInterleaving InterleaveMode = m_muxFilter as IConfigInterleaving;
				if (InterleaveMode != null)
				{
					DirectShowUtil.DebugWrite("AIWGraph:set avi interleave mode");
					hr = InterleaveMode.put_Mode(AviInterleaveMode.INTERLEAVE_CAPTURE);
					if (hr != 0)
					{
						DirectShowUtil.DebugWrite("AIWGraph:FAILED:to set avi interleave mode:0x{0:X}",hr);
					}
				}
				else
				{
					DirectShowUtil.DebugWrite("AIWGraph:FAILED:to get IConfigInterleaving");
				}
			}//if (!bRecordWMV)

		

			if (m_mediaControl == null)
				m_mediaControl = (IMediaControl)m_graphBuilder;

			//TuneChannel(standard, iChannelNr);
			StartViewing(channel);
			//m_mediaControl.Run();
			m_graphState = State.Recording;

			DirectShowUtil.DebugWrite("AIWGraph:recording...");
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
			DirectShowUtil.DebugWrite("AIWGraph:stop recording...");
			m_mediaControl.Stop();
			m_graphState = State.Created;
			DeleteGraph();
			DirectShowUtil.DebugWrite("AIWGraph:stopped recording...");
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
			m_iCountryCode=channel.Country;
			m_iCurrentChannel = channel.Number;
			int iChannel=channel.Number;
			int country=channel.Country;
			AnalogVideoStandard standard=channel.TVStandard;

			DirectShowUtil.DebugWrite("AIWGraph:TuneChannel() tune to channel:{0}", iChannel);
			if (iChannel < 1000)
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
					m_TVTuner.get_Channel(out iCurrentChannel, out iVideoSubChannel, out iAudioSubChannel);
					if (iCurrentChannel!=iChannel)
					{
						m_TVTuner.put_Channel(iChannel, DShowNET.AMTunerSubChannel.Default, DShowNET.AMTunerSubChannel.Default);
					}
					int iFreq;
					double dFreq;
					m_TVTuner.get_VideoFrequency(out iFreq);
					dFreq = iFreq / 1000000d;
					DirectShowUtil.DebugWrite("AIWGraph:TuneChannel() tuned to {0} MHz.", dFreq);
				}
				catch (Exception) {} 
			}
			else
			{
				SetVideoStandard(standard);
			}
			DirectShowUtil.DebugWrite("AIWGraph:TuneChannel() tuningspace:0 country:{0} tv standard:{1} cable:{2}",
				m_iCountryCode,standard.ToString(),
				m_bUseCable);

			DsUtils.FixCrossbarRoutingEx(m_graphBuilder,m_captureGraphBuilder, m_filterCaptureVideo, iChannel < (int)ExternalInputs.svhs, (iChannel == (int)ExternalInputs.cvbs1),(iChannel == (int)ExternalInputs.cvbs2),(iChannel == (int)ExternalInputs.svhs),(iChannel == (int)ExternalInputs.rgb),cardName);
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
		public bool StartViewing(TVChannel channel)
		{
			if (m_graphState == State.Viewing)
			{
				m_iCountryCode=channel.Country;
				TuneChannel(channel);
				return true;
			}

			if (m_graphState != State.Created) return false;
			m_iCountryCode=channel.Country;

      SwapOverlayMixerForOverlayMixer2(m_filterCaptureVideo, m_captureGraphBuilder, m_graphBuilder, false);
      
      TuneChannel(channel);

			m_videoCaptureDevice.RenderPreview();

			m_videoWindow = (IVideoWindow) m_graphBuilder as IVideoWindow;
			if (m_videoWindow==null)
			{
				Log.WriteFile(Log.LogType.Capture,"AIWGraph:FAILED:Unable to get IVideoWindow");
				return false;
			}

			m_basicVideo = m_graphBuilder as IBasicVideo2;
			if (m_basicVideo==null)
			{
				Log.WriteFile(Log.LogType.Capture,"AIWGraph:FAILED:Unable to get IBasicVideo2");
				return false;
			}

			m_mediaControl = (IMediaControl)m_graphBuilder;
			int hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
			if (hr != 0) 
				DirectShowUtil.DebugWrite("AIWGraph:FAILED:set Video window:0x{0:X}",hr);

			hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
			if (hr != 0) 
				DirectShowUtil.DebugWrite("AIWGraph:FAILED:set Video window style:0x{0:X}",hr);

			hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
			if (hr != 0) 
				DirectShowUtil.DebugWrite("AIWGraph:FAILED:put_Visible:0x{0:X}",hr);

			DirectShowUtil.DebugWrite("AIWGraph:enable deinterlace");
			DirectShowUtil.EnableDeInterlace(m_graphBuilder);

			m_mediaControl.Run();
      
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
			DirectShowUtil.DebugWrite("AIWGraph:StopViewing()");
			if (m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
			m_mediaControl.Stop();
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}

		/// <summary>
		/// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
		/// </summary>
		private void GUIGraphicsContext_OnVideoWindowChanged()
		{
			if (m_graphState != State.Viewing) return;
			int iVideoWidth, iVideoHeight;
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

				m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
				m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
				m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
				DirectShowUtil.DebugWrite("AIWGraph: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("AIWGraph: source position:({0},{1})-({2},{3})",rSource.Left, rSource.Top, rSource.Right, rSource.Bottom);
				DirectShowUtil.DebugWrite("AIWGraph: dest   position:({0},{1})-({2},{3})",rDest.Left, rDest.Top, rDest.Right, rDest.Bottom);
			}
			else
			{
				m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
				m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				DirectShowUtil.DebugWrite("AIWGraph: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("AIWGraph: source position:({0},{1})-({2},{3})",0, 0, iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("AIWGraph: dest   position:({0},{1})-({2},{3})",GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Right, GUIGraphicsContext.VideoWindow.Bottom);

			}

		}

		void SetRegistryThings()
		{
			//disable xvid status window while encoding
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				RegistryKey subkey = hkcu.OpenSubKey(@"Software\GNU\XviD");
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
		public bool ShouldRebuildGraph(TVChannel newChannel)
		{
			return false;
		}

		/// <summary>
		/// This method returns whether a signal is present. Meaning that the
		/// TV tuner (or video input) is tuned to a channel
		/// </summary>
		/// <returns>true:  tvtuner is tuned to a channel (or video-in has a video signal)
		///          false: tvtuner is not tuned to a channel (or video-in has no video signal)
		/// </returns>
		public bool SignalPresent()
		{
			if (m_TVTuner==null) return true;
			AMTunerSignalStrength strength;
			m_TVTuner.SignalPresent(out strength);
			return ( ( (int)strength ) >=1 );
		}

		public int  SignalQuality()
		{
			if (m_TVTuner==null) return 1;
			AMTunerSignalStrength strength;
			m_TVTuner.SignalPresent(out strength);
			if (strength==AMTunerSignalStrength.SignalPresent) return 100;
			return 1;
		}
		
		public int  SignalStrength()
		{
			if (m_TVTuner==null) return 1;
			AMTunerSignalStrength strength;
			m_TVTuner.SignalPresent(out strength);
			if (strength==AMTunerSignalStrength.SignalPresent) return 100;
			return 1;
		}
		/// <summary>
		/// This method returns the frequency to which the tv tuner is currently tuned
		/// </summary>
		/// <returns>frequency in Hertz
		/// </returns>
		public long VideoFrequency() 
		{
			return 0;
		}

    

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

		void ConnectTVTunerOutputs()
		{
			// AverMedia MCE card has a bug. It will only connect the TV Tuner->crossbar if
			// the crossbar outputs are disconnected
			// same for the winfast pvr 2000
			DirectShowUtil.DebugWrite("AIWGraph:ConnectTVTunerOutputs()");
      
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
				DirectShowUtil.DebugWrite("AIWGraph:no crossbar found");
				return; // no crossbar found?
			}
    
			IAMCrossbar crossbar = o as IAMCrossbar;
			if (crossbar ==null) 
			{
				DirectShowUtil.DebugWrite("AIWGraph:no crossbar found");
				return;
			}


			//disconnect the output pins of the crossbar->video capture filter
			DirectShowUtil.DebugWrite("AIWGraph:disconnect crossbar outputs");
			DirectShowUtil.DisconnectOutputPins(m_graphBuilder,(IBaseFilter)crossbar);

			//connect the output pins of the tvtuner
			DirectShowUtil.DebugWrite("AIWGraph:connect tvtuner outputs");
			bool bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)m_TVTuner);
			if (bAllConnected)
				DirectShowUtil.DebugWrite("AIWGraph:all connected");
			else
				DirectShowUtil.DebugWrite("AIWGraph:FAILED, not all pins connected");

			//reconnect the output pins of the crossbar
			DirectShowUtil.DebugWrite("AIWGraph:reconnect crossbar output pins");

			bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)crossbar);
			if (bAllConnected)
				DirectShowUtil.DebugWrite("AIWGraph:all connected");
			else
				DirectShowUtil.DebugWrite("AIWGraph:FAILED, not all pins connected");
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
					SourceCollection VideoSources = new SourceCollection( m_captureGraphBuilder, m_filterCaptureVideo, true );
					SourceCollection AudioSources = new SourceCollection( m_captureGraphBuilder, m_filterCaptureAudio, true );

					// #MW#, difficult to say if this must be changed, as it depends on the loaded
					// filters. The list below is fixed list however... So???
					propertyPages = new PropertyPageCollection( m_captureGraphBuilder, 
						m_filterCaptureVideo, m_filterCaptureAudio, 
						m_filterCompressorVideo, m_filterCompressorAudio, 
						VideoSources, AudioSources, (IBaseFilter)m_TVTuner );

				}
				catch ( Exception ex ) { Log.Write( "PropertyPages: FAILED to get property pages." + ex.ToString() ); }

				return( propertyPages );
			}
		}
		

		public bool SupportsFrameSize(Size framesize)
		{	
			m_videoCaptureDevice.SetFrameSize(framesize);
			Size newSize=m_videoCaptureDevice.GetFrameSize();
			return (newSize==framesize);
		}
		
		public NetworkType Network()
		{
				return NetworkType.Analog;
		}
		public void TuneFrequency(int frequency)
		{
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
		public void TuneRadioChannel(RadioStation station)
		{
			Log.WriteFile(Log.LogType.Capture,"AIWGraph:tune to {0} {1} hz", station.Name,station.Frequency);
			
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
			Log.WriteFile(Log.LogType.Capture,"AIWGraph:  tuned to {0} hz", frequency);
		}
		public void StartRadio(RadioStation station)
		{
			if (m_graphState != State.Radio)  
			{
        SwapOverlayMixerForOverlayMixer2(m_filterCaptureVideo, m_captureGraphBuilder, m_graphBuilder, true);
        AddATIFilters();
        
        if (m_graphState != State.Created)  return;
				if (m_videoCaptureDevice == null) return ;
				
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

				int hr = m_graphBuilder.Render(m_audioEncodeOutput);
				if (hr == 0) 
					DirectShowUtil.DebugWrite("AIWGraph:demux audio out connected ");
				else
					DirectShowUtil.DebugWrite("AIWGraph:FAILED to render AIWGraphdemux audio out:0x{0:X}",hr);

				if (m_mediaControl == null)
				{
					m_mediaControl = (IMediaControl)m_graphBuilder;
				}
				if (m_mediaControl != null)
				{
					m_mediaControl.Run();
				}

				m_graphState = State.Radio;

				Log.WriteFile(Log.LogType.Capture,"AIWGraph:StartRadio() started");

				return;
			}

			TuneRadioChannel(station);
		}
		public void TuneRadioFrequency(int frequency)
		{
			Log.WriteFile(Log.LogType.Capture,"AIWGraph:tune to {0} hz", frequency);
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
			Log.WriteFile(Log.LogType.Capture,"AIWGraph:  tuned to {0} hz", frequency);
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
		public string TvTimeshiftFileName()
		{
			return String.Empty;
		}
		public string RadioTimeshiftFileName()
		{
			return String.Empty;
		}
		public void GrabTeletext(bool yesNo)
		{
		}
		public IBaseFilter AudiodeviceFilter()
		{
			return m_filterCaptureAudio;
		}


		public bool	IsTimeShifting()
		{
			return false;
		}

    public bool IsEpgDone()
    {
      return true;
    }
    public bool IsEpgGrabbing()
    {
      return false;
    }
    public void GrabEpg(TVChannel chan)
    {
    }

	}
}
