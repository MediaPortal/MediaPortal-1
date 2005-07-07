#if (UseCaptureCardDefinitions)
/*
 * This is a modified version of the SinkGraph class.
 * It supports MPEG2 hardware cards using a definition file.
 * 
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices; 
using DShowNET;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using System.Xml;
using TVCapture;
using DirectX.Capture;


namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Implementation of IGraph for all hardware MPEG2 encoder cards (pretentions... ;-) )
	/// As there are the Hauppauge PVR 150MCE, 250, 350 and the WinFast PVR 2000
	/// 
	/// The graphbuilder can build the graph for the following functions:
	/// -tv viewing
	/// -tv recording
	/// -tv timeshifting
	/// </summary>	
	public class SinkGraphEx : MediaPortal.TV.Recording.SinkGraph
	{
		#region Constructors
		
		/// <summary>
		/// Constructor for the graph  for given capture card.
		/// </summary>
		/// <param name="pCard"></param>
		public SinkGraphEx(TVCaptureDevice pCard)
			: base(pCard)
		{
		}
		#endregion

		#region Overrides

		/// <summary>
		/// Creates a new DirectShow graph for the TV capturecard
		/// </summary>
		/// <returns>bool indicating if graph is created or not</returns>
		public override bool CreateGraph(int Quality)
		{
			try
			{
				Vmr9 =new VMR9Util("mytv");
				Vmr7 = new VMR7Util();

				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:CreateGraph() IN");
				if (m_graphState != State.None) return false;		// If doing something already, return...
				if (mCard==null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"SinkGraphEx:card is not defined");
					return false;
				}

				if (!mCard.LoadDefinitions())											// Load configuration for this card
				{
					Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Loading card definitions for card {0} failed", mCard.CaptureName);
					return false;
				}
				if (mCard.TvFilterDefinitions==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"SinkGraphEx:card does not contain filters?");
					return false;
				}
				if (mCard.TvConnectionDefinitions==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"SinkGraphEx:card does not contain connections for tv?");
					return false;
				}

				GUIGraphicsContext.OnGammaContrastBrightnessChanged +=new VideoGammaContrastBrightnessHandler(OnGammaContrastBrightnessChanged);


				// Initialize settings. No channel tuned yet...
				m_iPrevChannel = -1;

				int hr = 0;
							
				// Make a new filter graph
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Create new filter graph (IGraphBuilder)");
				m_graphBuilder = (IGraphBuilder) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true)); 

				// Get the Capture Graph Builder...
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Get the Capture Graph Builder (ICaptureGraphBuilder2)");
				Guid clsid = Clsid.CaptureGraphBuilder2;
				Guid riid  = typeof(ICaptureGraphBuilder2).GUID;
				m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref riid);

				// ...and link the Capture Graph Builder to the Graph Builder
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
				hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
				if( hr < 0 ) 
				{
					Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Error: link FAILED");
					Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:CreateGraph() OUT");
					return false;
				}
				// Add graph to Running Object Table (ROT), so we can connect to the graph using GraphEdit ;)
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Add graph to ROT table");
				DsROT.AddGraphToRot(m_graphBuilder, out m_rotCookie);

				// Loop through configured filters for this card, bind them and add them to the graph
				// Note that while adding filters to a graph, some connections may already be created...
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Adding configured filters...");
				foreach (string catName in mCard.TvFilterDefinitions.Keys)
				{
					FilterDefinition dsFilter = mCard.TvFilterDefinitions[catName] as FilterDefinition;
					Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					dsFilter.DSFilter         = Marshal.BindToMoniker(dsFilter.MonikerDisplayName) as IBaseFilter;
					hr = m_graphBuilder.AddFilter(dsFilter.DSFilter, dsFilter.FriendlyName);
					if (hr == 0)
					{
						Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  Added filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					}
					else
					{
						Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  Error! Failed adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
						Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  Error! Result code = {0}", hr);
					}

					// Support the "legacy" member variables. This could be done different using properties
					// through which the filters are accessable. More implementation independent...
					if (dsFilter.Category == "tvtuner") m_TVTuner       = dsFilter.DSFilter as IAMTVTuner;
					if (dsFilter.Category == "capture") m_captureFilter = dsFilter.DSFilter;
				}
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Adding configured filters...DONE");

				m_IAMAnalogVideoDecoder = m_captureFilter as IAMAnalogVideoDecoder;
				InitializeTuner();
				// All filters, up-to and including the encoder filter have been added using a configuration file.
				// The rest of the filters depends on the fact if we are just viewing TV, or Timeshifting or even
				// recording. This part is however card independent and controlled by software, although this part
				// could also be configured using a definition file. If used, this could lead to the possibility
				// of having building blocks enabling the support of about every card, or combination of cards, ie
				// even (again, pretentions...) the Sigma Designs XCard could be "coupled" to the capture card...

				// Set crossbar routing, default to Tv Tuner + Audio Tuner...
				//	DsUtils.FixCrossbarRouting(m_graphBuilder, m_captureGraphBuilder, m_captureFilter, true, false, false, false,cardName);

				FilterDefinition sourceFilter;
				FilterDefinition sinkFilter;
				IPin sourcePin;
				IPin sinkPin;

				// Create pin connections. These connections are also specified in the definitions file.
				// Note that some connections might fail due to the fact that the connection is already made,
				// probably during the addition of filters to the graph (checked with GraphEdit...)
				//
				// Pin connections can be defined in two ways:
				// 1. Using the name of the pin.
				//		This method does work, but might be language dependent, meaning the connection attempt
				//		will fail because the pin cannot be found...
				// 2.	Using the 0-based index number of the input or output pin.
				//		This method is save. It simply tells to connect output pin #0 to input pin #1 for example.
				//
				// The code assumes method 1 is used. If that fails, method 2 is tried...

				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Adding configured pin connections...");
				for (int i = 0; i < mCard.TvConnectionDefinitions.Count; i++)
				{
					sourceFilter = mCard.TvFilterDefinitions[((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SourceCategory] as FilterDefinition;
					sinkFilter   = mCard.TvFilterDefinitions[((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SinkCategory] as FilterDefinition;
					if (sourceFilter==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"Cannot find source filter for connection:{0}",i);
						continue;
					}
					if (sinkFilter==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"Cannot find sink filter for connection:{0}",i);
						continue;
					}
					Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  Connecting <{0}>:{1} with <{2}>:{3}", 
						sourceFilter.FriendlyName, ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SourcePinName,
						sinkFilter.FriendlyName, ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SinkPinName);
					//sourceFilter.DSFilter.FindPin(((ConnectionDefinition)mCard.ConnectionDefinitions[i]).SourcePinName, out sourcePin);
					sourcePin    = DirectShowUtil.FindPin(sourceFilter.DSFilter, PinDirection.Output, ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SourcePinName);
					if (sourcePin == null)
					{
						String strPinName = ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SourcePinName;
						if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
						{
							sourcePin = DirectShowUtil.FindPinNr(sourceFilter.DSFilter, PinDirection.Output, Convert.ToInt32(strPinName));
							if (sourcePin==null)
								Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   Unable to find sourcePin: <{0}>", strPinName);
							else
								Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   Found sourcePin: <{0}> <{1}>", strPinName, sourcePin.ToString());
						}
					}
					else
						Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   Found sourcePin: <{0}> ", ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SourcePinName);

					//sinkFilter.DSFilter.FindPin(((ConnectionDefinition)mCard.ConnectionDefinitions[i]).SinkPinName, out sinkPin);
					sinkPin      = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SinkPinName);
					if (sinkPin == null)
					{
						String strPinName = ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SinkPinName;
						if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
						{
							sinkPin = DirectShowUtil.FindPinNr(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
							if (sinkPin==null)
								Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   Unable to find sinkPin: <{0}>", strPinName);
							else
								Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
						}
					}
					else
						Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   Found sinkPin: <{0}> ", ((ConnectionDefinition)mCard.TvConnectionDefinitions[i]).SinkPinName);

					if (sourcePin!=null && sinkPin!=null)
					{
						IPin conPin;
						hr      = sourcePin.ConnectedTo(out conPin);
						if (hr != 0)
							hr = m_graphBuilder.Connect(sourcePin, sinkPin);
						if (hr == 0)
							Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   Pins connected...");

						// Give warning and release pin...
						if (conPin != null)
						{
							Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   (Pin was already connected...)");
							Marshal.ReleaseComObject(conPin as Object);
							conPin = null;
							hr     = 0;
						}
					}

					if (hr != 0)
					{
						Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:  Error: Unable to connect Pins 0x{0:X}", hr);
						if (hr == -2147220969)
						{
							Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:   -- Cannot connect: {0} or {1}", sourcePin.ToString(), sinkPin.ToString());
						}
					}
				}
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Adding configured pin connections...DONE");

				// Find out which filter & pin is used as the interface to the rest of the graph.
				// The configuration defines the filter, including the Video, Audio and Mpeg2 pins where applicable
				// We only use the filter, as the software will find the correct pin for now...
				// This should be changed in the future, to allow custom graph endings (mux/no mux) using the
				// video and audio pins to connect to the rest of the graph (SBE, overlay etc.)
				// This might be needed by the ATI AIW cards (waiting for ob2 to release...)
				FilterDefinition lastFilter = mCard.TvFilterDefinitions[mCard.TvInterfaceDefinition.FilterCategory] as FilterDefinition;


				SetQuality(Quality);

				// All filters and connections have been made.
				// Now fix the rest of the graph, add MUX etc.
				m_videoCaptureDevice = new VideoCaptureDevice(
					m_graphBuilder, m_captureGraphBuilder, m_captureFilter, lastFilter.DSFilter, mCard.IsMCECard, mCard.SupportsMPEG2);
	      
				m_FrameSize = m_videoCaptureDevice.GetFrameSize();

				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx: Capturing:{0}x{1}", m_FrameSize.Width, m_FrameSize.Height);
				m_mpeg2Demux = null;
				if (m_videoCaptureDevice.MPEG2)
				{
					// creates the last part of the graph. Depending on timeshifting etc.
					// it will eventually connect to the lastFilter, so this object only does the last part of the graph
					m_mpeg2Demux = new MPEG2Demux(ref m_graphBuilder, m_FrameSize);
				}

				// Connect video capture->mpeg2 demuxer
				ConnectVideoCaptureToMPEG2Demuxer(); // can be done using config, or just leave it this way ;-)
				m_mpeg2Demux.CreateMappings();

				// For just MPEG2 cards, set the videoprocamp...
				if ((mCard.SupportsMPEG2) && (!mCard.IsMCECard))
				{
					m_videoprocamp = m_captureFilter as IAMVideoProcAmp;
					if (m_videoprocamp != null)
					{
						m_videoAmp = new VideoProcAmp(m_videoprocamp);
						m_videoAmp.Contrast=m_videoAmp.ContrastDefault;
						m_videoAmp.Brightness=m_videoAmp.BrightnessDefault;
						m_videoAmp.Gamma=m_videoAmp.GammaDefault;
						m_videoAmp.Saturation=m_videoAmp.SaturationDefault;
						m_videoAmp.Sharpness=m_videoAmp.SharpnessDefault;
					}
				}

				m_graphState = State.Created;
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:CreateGraph() OUT");

				SetQuality(3);

				return true;
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"SinkGraphEx: Unable to create graph:{0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);
				return false;
			}
		}

		public override void DeleteGraph()
		{
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

			if (m_videoprocamp != null)
			{
				m_videoAmp     = null;
				m_videoprocamp = null;
			}
			if (m_mpeg2Demux != null)
			{
				m_mpeg2Demux.CloseInterfaces();
				m_mpeg2Demux = null;
			}

			if (m_videoCaptureDevice != null)
			{
				m_videoCaptureDevice.CloseInterfaces();
				m_videoCaptureDevice = null;
			}

			if (m_TVTuner != null)
				Marshal.ReleaseComObject(m_TVTuner); m_TVTuner = null;
			
			if (m_IAMAnalogVideoDecoder != null)
				Marshal.ReleaseComObject(m_IAMAnalogVideoDecoder); m_IAMAnalogVideoDecoder = null;

			if (m_graphBuilder!=null)
				DsUtils.RemoveFilters(m_graphBuilder);

			if (m_captureFilter != null)
				Marshal.ReleaseComObject(m_captureFilter); m_captureFilter = null;

			if (m_rotCookie != 0)
				DsROT.RemoveGraphFromRot(ref m_rotCookie); m_rotCookie = 0;

			if (m_captureGraphBuilder != null)
				Marshal.ReleaseComObject(m_captureGraphBuilder); m_captureGraphBuilder = null;
	
			if (m_graphBuilder != null)
				Marshal.ReleaseComObject(m_graphBuilder); m_graphBuilder = null;

			foreach (string strfName in mCard.TvFilterDefinitions.Keys)
			{
				FilterDefinition dsFilter = mCard.TvFilterDefinitions[strfName] as FilterDefinition;
				if (dsFilter.DSFilter != null)
					Marshal.ReleaseComObject(dsFilter.DSFilter);
				((FilterDefinition)mCard.TvFilterDefinitions[strfName]).DSFilter = null;
				dsFilter = null;
			}

			m_graphState = State.None;
			return ;
		}
		#endregion Overrides

		#region Obsolete But Probably Needed Again

		// #MW#
		// Not used anymore. Might be needed though to support these cards, or is a generic approach possible?
		// What if the connection order always assures that the TVTuner is connected first, meaning the
		// outputs of the crossbar are NOT connected to any other filter???????
		//
		// Extra Note:
		// Just found out that once you load some of the filters, some connectiona are already made, even
		// without explicit calls, so the crossbar might be connected already...
		void ConnectTVTunerOutputs()
		{
			// AverMedia MCE card has a bug. It will only connect the TV Tuner->crossbar if
			// the crossbar outputs are disconnected
			// same for the winfast pvr 2000
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:ConnectTVTunerOutputs()");
      
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
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:no crossbar found");
				return; // no crossbar found?
			}
    
			IAMCrossbar crossbar = o as IAMCrossbar;
			if (crossbar ==null) 
			{
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:no crossbar found");
				return;
			}

			//disconnect the output pins of the crossbar->video capture filter
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:disconnect crossbar outputs");
			DirectShowUtil.DisconnectOutputPins(m_graphBuilder,(IBaseFilter)crossbar);

			//connect the output pins of the tvtuner
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:connect tvtuner outputs");
			bool bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)m_TVTuner);
			if (bAllConnected)
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:all connected");
			else
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:FAILED, not all pins connected");

			//reconnect the output pins of the crossbar
			Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:reconnect crossbar output pins");

			bAllConnected=DirectShowUtil.RenderOutputPins(m_graphBuilder,(IBaseFilter)crossbar);
			if (bAllConnected)
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:all connected");
			else
				Log.WriteFile(Log.LogType.Capture,"SinkGraphEx:FAILED, not all pins connected");
		}
	
	#endregion

		
	}
}
#endif