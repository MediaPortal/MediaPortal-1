#if (UseCaptureCardDefinitions)
using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using DShowNET;
using DShowNET.Device;
using DShowNET.BDA;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Database;
using TVCapture;
using System.Xml;
using DirectX.Capture;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Implementation of IGraph for digital TV capture cards using the BDA driver architecture
	/// It handles any DVB-T, DVB-C, DVB-S TV Capture card with BDA drivers
	///
	/// A graphbuilder object supports one or more TVCapture cards and
	/// contains all the code/logic necessary for
	/// -tv viewing
	/// -tv recording
	/// -tv timeshifting
	/// 
	/// todo:
	///   -finish support for DVB-S and DVB-C  (things left todo: parsing of IGuiData in Process() )
	///   -when scanning for channels we need to parse IGuideData to see if the channel 
	///    found is a radio or tv channel and if its encrypted or free-2-air
	///   -get EPG data from stream
	///   
	/// for dvb-t scanning we need : frequency
	/// for dvb-s scanning we need : frequency, polarisation, symbolrate, innerFec, SID
	/// for dvb-c scanning we need : frequency, symbolrate, innerFec, SID,modulation
	/// </summary>
	public class DVBGraphBDA : MediaPortal.TV.Recording.IGraph
	{
		public class DVBTChannel
		{
			public int		ONID;				 //original network id
			public int		TSID;				 //transport service id
			public int		SID;				 // service id
			public string ChannelName; //name of channel
			public string NetworkName; //name of network provide
			public int    NetworkType; //type of network
			public bool		IsTv;				 //if true this is a TV channel
			public bool		IsRadio;     //if true this is a radio channel
		}
		
		public class DVBCChannel
		{
			public int		ONID;				 //original network id
			public int		TSID;				 //transport service id
			public int		SID;				 // service id
			public string ChannelName; //name of channel
			public string NetworkName; //name of network provide
			public int    NetworkType; //type of network
			public bool		IsTv;				 //if true this is a TV channel
			public bool		IsRadio;     //if true this is a radio channel
			public int    symbolRate;
			public int    innerFec;
			public int    carrierFrequency;
			public int    modulation;
		}

		public class DVBSChannel
		{
			public int		ONID;				 //original network id
			public int		TSID;				 //transport service id
			public int		SID;				 // service id
			public string ChannelName; //name of channel
			public string NetworkName; //name of network provide
			public int    NetworkType; //type of network
			public bool		IsTv;				 //if true this is a TV channel
			public bool		IsRadio;     //if true this is a radio channel
			public int    polarisation;
			public int    symbolRate;
			public int    innerFec;
			public int    carrierFrequency;
		}

		[ComImport, Guid("6CFAD761-735D-4aa5-8AFC-AF91A7D61EBA")]
			class VideoAnalyzer {};

		[ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
			class MPEG2Demultiplexer {}
    
		[ComImport, Guid("2DB47AE5-CF39-43c2-B4D6-0CD8D90946F4")]
		class StreamBufferSink {};

		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
		class StreamBufferConfig {}
	    
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern bool ConvertStringSidToSid(string pStringSid, ref IntPtr pSID);

		[DllImport("kernel32", CharSet=CharSet.Auto)]
		private static extern IntPtr  LocalFree( IntPtr hMem);

		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);
		const int WS_CHILD				= 0x40000000;	
		const int WS_CLIPCHILDREN	= 0x02000000;
		const int WS_CLIPSIBLINGS	= 0x04000000;

		private static Guid CLSID_StreamBufferSink					= new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
		private static Guid CLSID_Mpeg2VideoStreamAnalyzer	= new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
		private static Guid CLSID_StreamBufferConfig				= new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);

		enum State
		{ 
			None, 
			Created, 
			TimeShifting,
			Recording, 
			Viewing
		};

		int                     m_cardID								= -1;
		int                     m_iCurrentChannel				= 28;
		int											m_rotCookie							= 0;			// Cookie into the Running Object Table
		int                     m_iPrevChannel					= -1;
		bool                    m_bIsUsingMPEG					= false;
		State                   m_graphState						= State.None;
		DateTime								m_StartTime							= DateTime.Now;


		IBaseFilter             m_NetworkProvider				= null;			// BDA Network Provider
		IBaseFilter             m_TunerDevice						= null;			// BDA Digital Tuner Device
		IBaseFilter							m_CaptureDevice					= null;			// BDA Digital Capture Device
		IBaseFilter							m_MPEG2Demultiplexer		= null;			// Mpeg2 Demultiplexer that connects to Preview pin on Smart Tee (must connect before capture)
		IBaseFilter							m_TIF										= null;			// Transport Information Filter
		IBaseFilter							m_SectionsTables				= null;
		VideoAnalyzer						m_mpeg2Analyzer					= null;
		StreamBufferSink				m_StreamBufferSink=null;
		IGraphBuilder           m_graphBuilder					= null;
		ICaptureGraphBuilder2   m_captureGraphBuilder		= null;
		IVideoWindow            m_videoWindow						= null;
		IBasicVideo2            m_basicVideo						= null;
		IMediaControl						m_mediaControl					= null;
		IBDA_SignalStatistics   m_TunerStatistics       = null;
		NetworkType							m_NetworkType;
		
		TVCaptureDevice					m_Card;
		
		//streambuffer interfaces
		IPin												m_DemuxVideoPin				= null;
		IPin												m_DemuxAudioPin				= null;
		IPin												m_pinStreamBufferIn0	= null;
		IPin												m_pinStreamBufferIn1	= null;
		IStreamBufferRecordControl	m_recControl					= null;
		IStreamBufferSink						m_IStreamBufferSink		= null;
		IStreamBufferConfigure			m_IStreamBufferConfig	= null;
		StreamBufferConfig					m_StreamBufferConfig	= null;
		VMR9Util									  Vmr9								  = null; 
		GuideDataEvent							m_Event               = null;
		GCHandle										myHandle;
		int                         adviseCookie;
		bool												graphRunning=false;
		ArrayList										channelList = new ArrayList();
		int													currentFrequency;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pCard">instance of a TVCaptureDevice which contains all details about this card</param>
		public DVBGraphBDA(TVCaptureDevice pCard)	
		{
			m_Card								= pCard;
			m_cardID							= pCard.ID;
			m_bIsUsingMPEG				= true;
			m_graphState					= State.None;

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
		/// Creates a new DirectShow graph for the TV capturecard.
		/// This graph can be a DVB-T, DVB-C or DVB-S graph
		/// </summary>
		/// <returns>bool indicating if graph is created or not</returns>
		public bool CreateGraph()
		{
			try
			{
				//check if we didnt already create a graph
				if (m_graphState != State.None) 
					return false;
		      
				graphRunning=false;
				Log.Write("DVBGraphBDA:CreateGraph(). ");

				//no card defined? then we cannot build a graph
				if (m_Card==null) 
				{
					Log.Write("DVBGraphBDA:card is not defined");
					return false;
				}

				//load card definition from CaptureCardDefinitions.xml
				if (!m_Card.LoadDefinitions())											// Load configuration for this card
				{
					DirectShowUtil.DebugWrite("DVBGraphBDA: Loading card definitions for card {0} failed", m_Card.CaptureName);
					return false;
				}
				
				//check if definition contains a tv filter graph
				if (m_Card.TvFilterDefinitions==null)
				{
					Log.Write("DVBGraphBDA:card does not contain filters?");
					return false;
				}

				//check if definition contains <connections> for the tv filter graph
				if (m_Card.TvConnectionDefinitions==null)
				{
					Log.Write("DVBGraphBDA:card does not contain connections for tv?");
					return false;
				}

				//create new instance of VMR9 helper utility
				Vmr9 =new VMR9Util("mytv");

				// Make a new filter graph
				Log.Write("DVBGraphBDA:create new filter graph (IGraphBuilder)");
				m_graphBuilder = (IGraphBuilder) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true));

				// Get the Capture Graph Builder
				Log.Write("DVBGraphBDA:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
				Guid clsid = Clsid.CaptureGraphBuilder2;
				Guid riid = typeof(ICaptureGraphBuilder2).GUID;
				m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref riid);

				Log.Write("DVBGraphBDA:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
				int hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
				if (hr < 0) 
				{
					Log.Write("DVBGraphBDA:link FAILED:0x{0:X}",hr);
					return false;
				}
				Log.Write("DVBGraphBDA:Add graph to ROT table");
				DsROT.AddGraphToRot(m_graphBuilder, out m_rotCookie);


				// Loop through configured filters for this card, bind them and add them to the graph
				// Note that while adding filters to a graph, some connections may already be created...
				Log.Write("DVBGraphBDA: Adding configured filters...");
				foreach (string catName in m_Card.TvFilterDefinitions.Keys)
				{
					FilterDefinition dsFilter = m_Card.TvFilterDefinitions[catName] as FilterDefinition;
					Log.Write("DVBGraphBDA:  Adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					dsFilter.DSFilter         = Marshal.BindToMoniker(dsFilter.MonikerDisplayName) as IBaseFilter;
					hr = m_graphBuilder.AddFilter(dsFilter.DSFilter, dsFilter.FriendlyName);
					if (hr == 0)
					{
						Log.Write("DVBGraphBDA:  Added filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					}
					else
					{
						Log.Write("DVBGraphBDA:  Error! Failed adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
						Log.Write("DVBGraphBDA:  Error! Result code = {0}", hr);
					}

					// Support the "legacy" member variables. This could be done different using properties
					// through which the filters are accessable. More implementation independent...
					if (dsFilter.Category == "networkprovider") 
					{
						m_NetworkProvider       = dsFilter.DSFilter;
						// Initialise Tuning Space (using the setupTuningSpace function)
						if(!setupTuningSpace()) 
						{
							Log.Write("DVBGraphBDA:CreateGraph() FAILED couldnt create tuning space");
							return false;
						}
					}
					if (dsFilter.Category == "tunerdevice") m_TunerDevice	 							= dsFilter.DSFilter;
					if (dsFilter.Category == "capture")			m_CaptureDevice							= dsFilter.DSFilter;
				}
				Log.Write("DVBGraphBDA: Adding configured filters...DONE");

				//no network provider specified? then we cannot build the graph
				if(m_NetworkProvider == null)
				{
					Log.Write("DVBGraphBDA:CreateGraph() FAILED networkprovider filter not found");
					return false;
				}

				//no capture device specified? then we cannot build the graph
				if(m_CaptureDevice == null)
				{
					Log.Write("DVBGraphBDA:CreateGraph() FAILED capture filter not found");
				}


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

				Log.Write("DVBGraphBDA: Adding configured pin connections...");
				for (int i = 0; i < m_Card.TvConnectionDefinitions.Count; i++)
				{
					//get the source filter for the connection
					sourceFilter = m_Card.TvFilterDefinitions[((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourceCategory] as FilterDefinition;
					if (sourceFilter==null)
					{
						Log.Write("Cannot find source filter for connection:{0}",i);
						continue;
					}

					//get the destination/sink filter for the connection
					sinkFilter   = m_Card.TvFilterDefinitions[((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkCategory] as FilterDefinition;
					if (sinkFilter==null)
					{
						Log.Write("Cannot find sink filter for connection:{0}",i);
						continue;
					}

					Log.Write("DVBGraphBDA:  Connecting <{0}>:{1} with <{2}>:{3}", 
										sourceFilter.FriendlyName, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName,
										sinkFilter.FriendlyName, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);
					
					//find the pin of the source filter
					sourcePin    = DirectShowUtil.FindPin(sourceFilter.DSFilter, PinDirection.Output, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName);
					if (sourcePin == null)
					{
						String strPinName = ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName;
						if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
						{
							sourcePin = DirectShowUtil.FindPinNr(sourceFilter.DSFilter, PinDirection.Output, Convert.ToInt32(strPinName));
							if (sourcePin==null)
								Log.Write("DVBGraphBDA:   Unable to find sourcePin: <{0}>", strPinName);
							else
								Log.Write("DVBGraphBDA:   Found sourcePin: <{0}> <{1}>", strPinName, sourcePin.ToString());
						}
					}
					else
						Log.Write("DVBGraphBDA:   Found sourcePin: <{0}> ", ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName);

					//find the pin of the sink filter
					sinkPin      = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);
					if (sinkPin == null)
					{
						String strPinName = ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName;
						if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
						{
							sinkPin = DirectShowUtil.FindPinNr(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
							if (sinkPin==null)
								Log.Write("DVBGraphBDA:   Unable to find sinkPin: <{0}>", strPinName);
							else
								Log.Write("DVBGraphBDA:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
						}
					}
					else
						Log.Write("DVBGraphBDA:   Found sinkPin: <{0}> ", ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);

					//if we have both pins
					if (sourcePin!=null && sinkPin!=null)
					{
						// then connect them
						IPin conPin;
						hr      = sourcePin.ConnectedTo(out conPin);
						if (hr != 0)
							hr = m_graphBuilder.Connect(sourcePin, sinkPin);
						if (hr == 0)
							Log.Write("DVBGraphBDA:   Pins connected...");

						// Give warning and release pin...
						if (conPin != null)
						{
							Log.Write("DVBGraphBDA:   (Pin was already connected...)");
							Marshal.ReleaseComObject(conPin as Object);
							conPin = null;
							hr     = 0;
						}
					}

					//log if connection failed
					if (hr != 0)
					{
						Log.Write("DVBGraphBDA:  Error: Unable to connect Pins 0x{0:X}", hr);
						if (hr == -2147220969)
						{
							Log.Write("DVBGraphBDA:   -- Cannot connect: {0} or {1}", sourcePin.ToString(), sinkPin.ToString());
						}
					}
				}
				Log.Write("DVBGraphBDA: Adding configured pin connections...DONE");

				// Find out which filter & pin is used as the interface to the rest of the graph.
				// The configuration defines the filter, including the Video, Audio and Mpeg2 pins where applicable
				// We only use the filter, as the software will find the correct pin for now...
				// This should be changed in the future, to allow custom graph endings (mux/no mux) using the
				// video and audio pins to connect to the rest of the graph (SBE, overlay etc.)
				// This might be needed by the ATI AIW cards (waiting for ob2 to release...)
				FilterDefinition lastFilter = m_Card.TvFilterDefinitions[m_Card.TvInterfaceDefinition.FilterCategory] as FilterDefinition;

				// no interface defined or interface not found? then return
				if(lastFilter == null)
				{
					Log.Write("DVBGraphBDA:CreateGraph() FAILED interface filter not found");
					return false;
				}

				//=========================================================================================================
				// add the MPEG-2 Demultiplexer 
				//=========================================================================================================
				// Use CLSID_Mpeg2Demultiplexer to create the filter
				m_MPEG2Demultiplexer = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.Mpeg2Demultiplexer, true));
				if(m_MPEG2Demultiplexer== null) 
				{
					Log.Write("DVBGraphBDA:Failed to create Mpeg2 Demultiplexer");
					return false;
				}

				// Add the Demux to the graph
				m_graphBuilder.AddFilter(m_MPEG2Demultiplexer, "MPEG-2 Demultiplexer");
				
				// connect the interface filter->mpeg2 demultiplexer
				if(!ConnectFilters(ref lastFilter.DSFilter, ref m_MPEG2Demultiplexer)) 
				{
					Log.Write("DVBGraphBDA:Failed to connect interface filter->mpeg2 demultiplexer");
					return false;
				}

				//=========================================================================================================
				// Add the BDA MPEG2 Transport Information Filter
				//=========================================================================================================
				object tmpObject;
				if(!findNamedFilter(FilterCategories.KSCATEGORY_BDA_TRANSPORT_INFORMATION, "BDA MPEG2 Transport Information Filter", out tmpObject)) 
				{
					Log.Write("DVBGraphBDA:CreateGraph() FAILED Failed to find BDA MPEG2 Transport Information Filter");
					return false;
				}
				m_TIF = (IBaseFilter) tmpObject;
				tmpObject = null;
				if(m_TIF == null)
				{
					Log.Write("DVBGraphBDA:CreateGraph() FAILED BDA MPEG2 Transport Information Filter is null");
					return false;
				}
				m_graphBuilder.AddFilter(m_TIF, "BDA MPEG2 Transport Information Filter");

				//connect mpeg2 demultiplexer->BDA MPEG2 Transport Information Filter
				if(!ConnectFilters(ref m_MPEG2Demultiplexer, ref m_TIF)) 
				{
					Log.Write("DVBGraphBDA:Failed to connect MPEG-2 Demultiplexer->BDA MPEG2 Transport Information Filter");
					return false;
				}

				//=========================================================================================================
				// Add the MPEG-2 Sections and Tables filter
				//=========================================================================================================
				if(!findNamedFilter(FilterCategories.KSCATEGORY_BDA_TRANSPORT_INFORMATION, "MPEG-2 Sections and Tables", out tmpObject)) 
				{
					Log.Write("DVBGraphBDA:CreateGraph() FAILED Failed to find MPEG-2 Sections and Tables Filter");
					return false;
				}
				m_SectionsTables = (IBaseFilter) tmpObject;
				tmpObject = null;
				if(m_SectionsTables == null)
				{
					Log.Write("DVBGraphBDA:CreateGraph() FAILED MPEG-2 Sections and Tables Filter is null");
				}

				m_graphBuilder.AddFilter(m_SectionsTables, "MPEG-2 Sections & Tables");

				//connect the mpeg2 demultiplexer->MPEG-2 Sections & Tables
				int iPreferredOutputPin=0;
				try
				{
					//get the preferred mpeg2 demultiplexer pin
					iPreferredOutputPin=Convert.ToInt32(m_Card.TvInterfaceDefinition.SectionsAndTablesPinName);
				}
				catch(Exception){}

				//and connect
				if(!ConnectFilters(ref m_MPEG2Demultiplexer, ref m_SectionsTables, iPreferredOutputPin)) 
				{
					Log.Write("DVBGraphBDA:Failed to connect MPEG-2 Demultiplexer to MPEG-2 Sections and Tables Filter");
					return false;
				}

				//get the video/audio output pins of the mpeg2 demultiplexer
				m_MPEG2Demultiplexer.FindPin(m_Card.TvInterfaceDefinition.VideoPinName, out m_DemuxVideoPin);
				m_MPEG2Demultiplexer.FindPin(m_Card.TvInterfaceDefinition.AudioPinName, out m_DemuxAudioPin);
				if (m_DemuxVideoPin==null)
				{
					//video pin not found
					Log.Write("DVBGraphBDA:Failed to get pin '{0}' (video out) from MPEG-2 Demultiplexer",m_DemuxVideoPin);
					return false;
				}
				if (m_DemuxAudioPin==null)
				{
					//audio pin not found
					Log.Write("DVBGraphBDA:Failed to get pin '{0}' (audio out)  from MPEG-2 Demultiplexer",m_DemuxAudioPin);
					return false;
				}

				//=========================================================================================================
				// Create the streambuffer engine and mpeg2 video analyzer components since we need them for
				// recording and timeshifting
				//=========================================================================================================
				m_StreamBufferSink  = new StreamBufferSink();
				m_mpeg2Analyzer     = new VideoAnalyzer();
				m_IStreamBufferSink = (IStreamBufferSink) m_StreamBufferSink;
				m_graphState=State.Created;

				m_TunerStatistics=GetTunerSignalStatistics();
				AdviseProgramInfo();
				return true;
			}
			catch(Exception)
			{
				Log.Write("DVBGraphBDA: Unable to create graph");
				return false;
			}
		}//public bool CreateGraph()

		/// <summary>
		/// This method asks the Transport Information filter to send us notifies 
		/// about program & service changes which we need for auto-scanning channels
		/// and EPG data
		/// </summary>
		/// <remarks>
		/// assumes that graph is created 
		/// </remarks>
		void AdviseProgramInfo()
		{
			Log.Write("DVBGraphBDA:AdivseProgramInfo()");
			//No TIF? then return;
			if (m_TIF==null) return;

			//creat a new callback object. The TIF will call members on this
			//object when it has new information about services/programs
			m_Event= new GuideDataEvent();
			myHandle = GCHandle.Alloc(m_Event, GCHandleType.Pinned);

			//get IConnectionPointContainer interface from TIF
			IConnectionPointContainer container = m_TIF as IConnectionPointContainer;
			if (container==null)
			{
				//no IConnectionPointContainer ? then return
				Log.Write("DVBGraphBDA:CreateGraph() FAILED couldnt get IConnectionPointContainer");
				return ;
			}
			//Find connection point of the IGuideDataEvent interface
			Guid iid=typeof(IGuideDataEvent).GUID;
			IConnectionPoint    connectPoint;      
			container.FindConnectionPoint( ref iid, out connectPoint);
			if (connectPoint==null)
			{
				// IGuideDataEvent connection point not found? then return
				Log.Write("DVBGraphBDA:CreateGraph() FAILED couldnt get IGuideDataEvent");
				return ;
			}

			//ask the IGuideDataEvent to call the m_Event when it has information about services / programs
			int hr=connectPoint.Advise( m_Event, out adviseCookie);
			if (hr!=0)
			{
				// error
				Log.Write("DVBGraphBDA:CreateGraph() FAILED couldnt set advise");
				return ;
			}
			Log.Write("DVBGraphBDA:AdivseProgramInfo() done");
		}//void AdviseProgramInfo()

		/// <summary>
		///  UnAdviseProgramInfo()
		///  This will ask the TIF to stop giving us information about service/program events
		/// </summary>
		/// <remarks>
		/// assumes that graph is created and AdviseProgramInfo() has been called in the past
		/// </remarks>
		void UnAdviseProgramInfo()
		{
			Log.Write("DVBGraphBDA:UnAdviseProgramInfo()");
			// if not registered then return
			if (adviseCookie==0) return;

			// if no TIF then return
			if (m_TIF==null) return;

			//get the IConnectionPointContainer of the TIF
			IConnectionPointContainer container = m_TIF as IConnectionPointContainer;
			if (container==null)
			{
				Log.Write("DVBGraphBDA:CreateGraph() FAILED couldnt get IConnectionPointContainer");
				return ;
			}

			//Get the IGuideDataEvent connection point
			Guid iid=typeof(IGuideDataEvent).GUID;
			IConnectionPoint    connectPoint;      
			container.FindConnectionPoint( ref iid, out connectPoint);
			if (connectPoint==null)
			{
				Log.Write("DVBGraphBDA:CreateGraph() FAILED couldnt get IGuideDataEvent");
				return ;
			}

			//tell IGuideDataEvent to stop giving info about services/program changes
			int hr=connectPoint.Unadvise(adviseCookie);
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:CreateGraph() FAILED couldnt set advise");
			}
			adviseCookie=0;
			
			//free handle to m_event
			if (myHandle.IsAllocated)
			{
				myHandle.Free();
			}
			m_Event=null;
		}//void UnAdviseProgramInfo()

		/// <summary>
		/// Deletes the current DirectShow graph created with CreateGraph()
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public void DeleteGraph()
		{
			if (m_graphState < State.Created) 
				return;
			m_iPrevChannel = -1;
	
			Log.Write("DVBGraphBDA:DeleteGraph()");
			StopRecording();
			StopViewing();

			if (m_TunerStatistics!=null)
			{
				Marshal.ReleaseComObject(m_TunerStatistics); m_TunerStatistics=null;
			}

			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}

			UnAdviseProgramInfo();
			
			if (m_recControl!=null) 
			{
				m_recControl.Stop(0);
				Marshal.ReleaseComObject(m_recControl); m_recControl=null;
			}
		      
			if (m_StreamBufferSink!=null) 
			{
				Marshal.ReleaseComObject(m_StreamBufferSink); m_StreamBufferSink=null;
			}

			if (m_mediaControl != null)
				m_mediaControl.Stop();
     
			graphRunning=false;

			if (m_videoWindow != null)
			{
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
				//m_videoWindow.put_Owner(IntPtr.Zero);
				m_videoWindow = null;
			}

			if (m_StreamBufferConfig != null) 
				Marshal.ReleaseComObject(m_StreamBufferConfig); m_StreamBufferConfig=null;

			if (m_IStreamBufferConfig != null) 
				Marshal.ReleaseComObject(m_IStreamBufferConfig); m_IStreamBufferConfig=null;

			if (m_pinStreamBufferIn1 != null) 
				Marshal.ReleaseComObject(m_pinStreamBufferIn1); m_pinStreamBufferIn1=null;

			if (m_pinStreamBufferIn0 != null) 
				Marshal.ReleaseComObject(m_pinStreamBufferIn0); m_pinStreamBufferIn0=null;

			if (m_IStreamBufferSink != null) 
				Marshal.ReleaseComObject(m_IStreamBufferSink); m_IStreamBufferSink=null;

			if (m_NetworkProvider != null)
				Marshal.ReleaseComObject(m_NetworkProvider); m_NetworkProvider = null;

			if (m_TunerDevice != null)
				Marshal.ReleaseComObject(m_TunerDevice); m_TunerDevice = null;

			if (m_CaptureDevice != null)
				Marshal.ReleaseComObject(m_CaptureDevice); m_CaptureDevice = null;
			
			if (m_MPEG2Demultiplexer != null)
				Marshal.ReleaseComObject(m_MPEG2Demultiplexer); m_MPEG2Demultiplexer = null;

			if (m_TIF != null)
				Marshal.ReleaseComObject(m_TIF); m_TIF = null;

			if (m_SectionsTables != null)
				Marshal.ReleaseComObject(m_SectionsTables); m_SectionsTables = null;

			m_basicVideo = null;
			m_mediaControl = null;
		      
			DsUtils.RemoveFilters(m_graphBuilder);

			if (m_rotCookie != 0)
				DsROT.RemoveGraphFromRot(ref m_rotCookie);
			m_rotCookie = 0;

			if (m_captureGraphBuilder != null)
				Marshal.ReleaseComObject(m_captureGraphBuilder); m_captureGraphBuilder = null;

			if (m_graphBuilder != null)
				Marshal.ReleaseComObject(m_graphBuilder); m_graphBuilder = null;


			foreach (string strfName in m_Card.TvFilterDefinitions.Keys)
			{
				FilterDefinition dsFilter = m_Card.TvFilterDefinitions[strfName] as FilterDefinition;
				if (dsFilter.DSFilter != null)
					Marshal.ReleaseComObject(dsFilter.DSFilter);
				((FilterDefinition)m_Card.TvFilterDefinitions[strfName]).DSFilter = null;
				dsFilter = null;
			}

			m_graphState = State.None;
			return;
		}//public void DeleteGraph()
		
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
			Log.Write("DVBGraphBDA:stop recording...");

			if (m_recControl!=null) 
			{
				int hr=m_recControl.Stop(0);
				if (hr!=0) 
				{
					Log.Write("DVBGraphBDA: FAILED to stop recording:0x{0:x}",hr );
					return;
				}
				if (m_recControl!=null) Marshal.ReleaseComObject(m_recControl); m_recControl=null;
			}


			Log.Write("DVBGraphBDA: stop graph");
			m_mediaControl.Stop();
			graphRunning=false;
			m_graphState = State.Created;
			DeleteGraph();
			Log.Write("DVBGraphBDA:stopped recording...");
		}//public void StopRecording()



		/// <summary>
		/// Switches / tunes to another TV channel
		/// </summary>
		/// <param name="iChannel">New channel</param>
		/// <remarks>
		/// Graph should be viewing or timeshifting. 
		/// </remarks>
		public void TuneChannel(AnalogVideoStandard standard,int iChannel,int country)
		{
			if (m_NetworkProvider==null) return;
			m_iPrevChannel		= m_iCurrentChannel;
			m_iCurrentChannel = iChannel;
			m_StartTime				= DateTime.Now;

			Log.Write("DVBGraphBDA:TuneChannel() tune to channel:{0}", iChannel);

			//get the ITuner interface from the network provider filter
			TunerLib.TuneRequest newTuneRequest = null;
			TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
			
			if (myTuner==null) return;
			switch (m_NetworkType)
			{
				case NetworkType.ATSC: 
				{
					//todo: add tuning for analog tv cards
				} break;
				
				case NetworkType.DVBC: 
				{
					//get the DVB-C tuning details from the tv database
					int frequency=0,symbolrate=0,innerFec=0, ONID=-1,TSID=-1,SID=-1,modulation=0;
					TVDatabase.GetDVBCTuneRequest(iChannel,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID);
					if (frequency<=0) return;
					Log.Write("DVBGraphBDA:tuning to frequency:{0} KHz ", frequency);

					//get the IDVBTuningSpace2 from the tuner
					TunerLib.IDVBTuningSpace2 myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
					if (myTuningSpace==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz. Invalid tuningspace", frequency);
						return ;
					}

					//create a new tuning request
					newTuneRequest = myTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot create new tuningrequest", frequency);
						return ;
					}

					TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
					if (myTuneRequest ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot create new dvbt tuningrequest", frequency);
						return ;
					}

					//get the IDVBCLocator interface from the new tuning request
					TunerLib.IDVBCLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBCLocator;	
					if (myLocator==null)
					{
						myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBCLocator;
					}
					
					if (myLocator ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0}. cannot get locator", frequency);
						return ;
					}
					//set the properties on the new tuning request
					myLocator.CarrierFrequency		= frequency;
					myLocator.SymbolRate				  = symbolrate;
					myLocator.InnerFEC						= (TunerLib.FECMethod)innerFec;
					myLocator.Modulation					= (TunerLib.ModulationType)modulation;
					myTuneRequest.ONID	= ONID;					//original network id
					myTuneRequest.TSID	= TSID;					//transport stream id
					myTuneRequest.SID		= SID;					//service id
					myTuneRequest.Locator=(TunerLib.Locator)myLocator;

					//submit tune request to the tuner
					myTuner.TuneRequest = newTuneRequest;
					Marshal.ReleaseComObject(myTuneRequest);
					currentFrequency=(int)frequency;
				} break;

				case NetworkType.DVBS: 
				{					
					//get the DVB-S tuning details from the tv database
					//for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
					int frequency=0,polarisation=0,symbolrate=0,innerFec=0, ONID=-1,TSID=-1,SID=-1;
					TVDatabase.GetDVBSTuneRequest(iChannel,out frequency, out polarisation, out symbolrate, out innerFec, out ONID, out TSID, out SID);
					if (frequency<=0) return;
					Log.Write("DVBGraphBDA:tuning to frequency:{0} ", frequency);

					//get the IDVBTuningSpace2 from the tuner
					TunerLib.IDVBTuningSpace2 myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
					if (myTuningSpace==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz. Invalid tuningspace", frequency);
						return ;
					}

					//create a new tuning request
					newTuneRequest = myTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot create new tuningrequest", frequency);
						return ;
					}

					TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
					if (myTuneRequest ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz . cannot create new dvbt tuningrequest", frequency);
						return ;
					}

					//get the IDVBSLocator interface from the new tuning request
					TunerLib.IDVBSLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBSLocator;	
					if (myLocator==null)
					{
						myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBSLocator;
					}
					
					if (myLocator ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get locator", frequency);
						return ;
					}
					//set the properties on the new tuning request
					myLocator.CarrierFrequency		= frequency;
					myLocator.SignalPolarisation  = (TunerLib.Polarisation)polarisation;
					myLocator.SymbolRate				  = symbolrate;
					myLocator.InnerFEC						= (TunerLib.FECMethod)innerFec;
					myTuneRequest.ONID	= ONID;		//original network id
					myTuneRequest.TSID	= TSID;		//transport stream id
					myTuneRequest.SID		= SID;		//service id
					myTuneRequest.Locator=(TunerLib.Locator)myLocator;

					//submit tune request to the tuner
					myTuner.TuneRequest = newTuneRequest;
					Marshal.ReleaseComObject(myTuneRequest);
					currentFrequency=(int)frequency;
				} break;

				case NetworkType.DVBT: 
				{
					//get the DVB-T tuning details from the tv database
					//for DVB-T this is the frequency, ONID , TSID and SID
					int frequency,ONID,TSID,SID;
					TVDatabase.GetDVBTTuneRequest(iChannel,out frequency, out ONID, out TSID, out SID);
					if (frequency<=0) return;
					Log.Write("DVBGraphBDA:tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}", frequency,ONID,TSID,SID);

					//get the IDVBTuningSpace2 from the tuner
					TunerLib.IDVBTuningSpace2 myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
					if (myTuningSpace==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}. Invalid tuningspace", frequency,ONID,TSID,SID);
						return ;
					}

					//create a new tuning request
					newTuneRequest = myTuningSpace.CreateTuneRequest();
					if (newTuneRequest ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}. cannot create new tuningrequest", frequency,ONID,TSID,SID);
						return ;
					}

					TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
					if (myTuneRequest ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}. cannot create new dvbt tuningrequest", frequency,ONID,TSID,SID);
						return ;
					}

					//get the IDVBTLocator interface from the new tuning request
					TunerLib.IDVBTLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBTLocator;	
					if (myLocator==null)
					{
						myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBTLocator;
					}
					
					if (myLocator ==null)
					{
						Log.Write("DVBGraphBDA:FAILED tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}. cannot get locator", frequency,ONID,TSID,SID);
						return ;
					}
					//set the properties on the new tuning request
					myLocator.CarrierFrequency		= frequency;
					myTuneRequest.ONID	= ONID;					//original network id
					myTuneRequest.TSID	= TSID;					//transport stream id
					myTuneRequest.SID		= SID;					//service id
					myTuneRequest.Locator=(TunerLib.Locator)myLocator;

					//submit tune request to the tuner
					myTuner.TuneRequest = newTuneRequest;
					Marshal.ReleaseComObject(myTuneRequest);
					currentFrequency=(int)frequency;
				} break;
			}	//switch (m_NetworkType)
		}//public void TuneChannel(AnalogVideoStandard standard,int iChannel,int country)

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
		/// Add preferred mpeg video/audio codecs to the graph
		/// the user has can specify these codecs in the setup
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
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
		}//void AddPreferredCodecs()
		
		/// <summary>
		/// Starts viewing the TV channel 
		/// </summary>
		/// <param name="iChannelNr">TV channel to which card should be tuned</param>
		/// <returns>boolean indicating if succeed</returns>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public bool StartViewing(AnalogVideoStandard standard, int iChannel,int country)
		{
			if (m_graphState != State.Created) return false;
			
			Log.Write("DVBGraphBDA:StartViewing()");

			// add VMR9 renderer to graph
			Vmr9.AddVMR9(m_graphBuilder);

			// add the preferred video/audio codecs
			AddPreferredCodecs();

			// render the video/audio pins of the mpeg2 demultiplexer so they get connected to the video/audio codecs
			if(m_graphBuilder.Render(m_DemuxVideoPin) != 0)
			{
				Log.Write("DVBGraphBDA:Failed to render video out pin MPEG-2 Demultiplexer");
				return false;
			}

			if(m_graphBuilder.Render(m_DemuxAudioPin) != 0)
			{
				Log.Write("DVBGraphBDA:Failed to render audio out pin MPEG-2 Demultiplexer");
				return false;
			}

			//get the IMediaControl interface of the graph
			if(m_mediaControl == null)
				m_mediaControl = (IMediaControl) m_graphBuilder;

			int hr;
			//if are using the overlay video renderer
			if (!Vmr9.IsVMR9Connected)
			{
				//then get the overlay video renderer interfaces
				m_videoWindow = m_graphBuilder as IVideoWindow;
				if (m_videoWindow == null)
				{
					Log.Write("DVBGraphBDA:FAILED:Unable to get IVideoWindow");
					return false;
				}

				m_basicVideo = m_graphBuilder as IBasicVideo2;
				if (m_basicVideo == null)
				{
					Log.Write("DVBGraphBDA:FAILED:Unable to get IBasicVideo2");
					return false;
				}

				// and set it up
				hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
				if (hr != 0) 
					Log.Write("DVBGraphBDA: FAILED:set Video window:0x{0:X}",hr);

				hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
				if (hr != 0) 
					Log.Write("DVBGraphBDA: FAILED:set Video window style:0x{0:X}",hr);

	      //show overlay window
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
					Log.Write("DVBGraphBDA: FAILED:put_Visible:0x{0:X}",hr);
			}

			//start the graph
			Log.Write("DVBGraphBDA: start graph");
			hr=m_mediaControl.Run();
			if (hr<0)
			{
				Log.Write("DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
			}

			graphRunning=true;
			
			GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			m_graphState = State.Viewing;
			GUIGraphicsContext_OnVideoWindowChanged();


			// tune to the correct channel
			if (iChannel>=0)
				TuneChannel(standard,iChannel,country);

			Log.Write("DVBGraphBDA:Viewing..");
			return true;
		}//public bool StartViewing(AnalogVideoStandard standard, int iChannel,int country)


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
			Log.Write("DVBGraphBDA: StopViewing()");
			if (m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);

			Log.Write("DVBGraphBDA: stop graph");
			if (m_mediaControl!=null)
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
			if (Vmr9.IsVMR9Connected)
			{
				iVideoWidth=Vmr9.VideoWidth;
				iVideoHeight=Vmr9.VideoHeight;
			}
			else
			{
				m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);
			}
			/*if (GUIGraphicsContext.Overlay==false)
			{
				if (Overlay!=false)
				{
					Log.Write("DVBGraphBDA:overlay disabled");
					Overlay=false;
				}
				return;
			}
			else
			{
				if (Overlay!=true)
				{
					Log.Write("DVBGraphBDA:overlay enabled");
					Overlay=true;
				}
			}*/
      
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

				if (!Vmr9.IsVMR9Connected)
				{
					m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
					m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
					m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
					Log.Write("DVBGraphBDA: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
					Log.Write("DVBGraphBDA: source position:({0},{1})-({2},{3})",rSource.Left, rSource.Top, rSource.Right, rSource.Bottom);
					Log.Write("DVBGraphBDA: dest   position:({0},{1})-({2},{3})",rDest.Left, rDest.Top, rDest.Right, rDest.Bottom);
				}
			}
			else
			{
				if (!Vmr9.IsVMR9Connected)
				{
					m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
					m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
					m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
					Log.Write("DVBGraphBDA: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
					Log.Write("DVBGraphBDA: source position:({0},{1})-({2},{3})",0, 0, iVideoWidth, iVideoHeight);
					Log.Write("DVBGraphBDA: dest   position:({0},{1})-({2},{3})",GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Right, GUIGraphicsContext.VideoWindow.Bottom);
				}

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
		public bool ShouldRebuildGraph(int iChannel)
		{
			return false;
		}

		/// <summary>
		/// This method gets the IBDA_SignalStatistics interface from the tuner
		/// with this interface we can see if the tuner is locked to a signal
		/// and see what the signal strentgh is
		/// </summary>
		/// <returns>
		/// IBDA_SignalStatistics or null
		/// </returns>
		/// <remarks>
		/// Graph should be created
		/// </remarks>
		IBDA_SignalStatistics GetTunerSignalStatistics()
		{
			//no tuner filter? then return;
			if (m_TunerDevice==null) 
				return null;
			
			//get the IBDA_Topology from the tuner device
			Log.Write("DVBGraphBDA: get IBDA_Topology");
			IBDA_Topology topology = m_TunerDevice as IBDA_Topology;
			if (topology==null)
			{
				Log.Write("DVBGraphBDA: could not get IBDA_Topology from tuner");
				return null;
			}

			//get the NodeTypes from the topology
			Log.Write("DVBGraphBDA: GetNodeTypes");
			int nodeTypeCount=0;
			int interfaceCount=0;
			int[] nodeTypes = new int[33];
			Guid[] guidInterfaces = new Guid[33];
			int hr=topology.GetNodeTypes(ref nodeTypeCount,32, nodeTypes);
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA: FAILED could not get node types from tuner");
				return null;
			}

			//for each node type
			Log.Write("DVBGraphBDA: got {0} node types", nodeTypeCount);
			for (int i=0; i < nodeTypeCount;++i)
			{
				//get the node interfaces
				Log.Write("DVBGraphBDA: get interfaces for node:{0}", i);
				hr=topology.GetNodeInterfaces(nodeTypes[i],ref interfaceCount,32,guidInterfaces);
				if (hr!=0)
				{
					Log.Write("DVBGraphBDA: FAILED could not GetNodeInterfaces for node:{0}",hr);
					return null;
				}
				//for each interface
				Log.Write("DVBGraphBDA: got {0} interfaces", interfaceCount);
				for (int j=0; j < interfaceCount; ++j)
				{
					//if this interface is IBDA_SignalStatistics
					if ( guidInterfaces[j]== typeof(IBDA_SignalStatistics).GUID)
					{
						//then we found it. get the control node
						Log.Write("DVBGraphBDA: found IBDA_SignalStatistics for interface:{0}. Get Node:{1}", j,i);
						object objectNode;
						hr=topology.GetControlNode(0,1, nodeTypes[i], out objectNode);
						if (hr!=0)
						{
							Log.Write("DVBGraphBDA: FAILED could not GetControlNode for node:{0}",hr);
							return null;
						}
						//and get the final IBDA_SignalStatistics
						Log.Write("DVBGraphBDA: got node");
						IBDA_SignalStatistics signal = objectNode as IBDA_SignalStatistics;
						if (signal==null)
						{
							Log.Write("DVBGraphBDA: FAILED could not IBDA_SignalStatistics for node:{0}",hr);
							return null;
						}
						Marshal.ReleaseComObject(topology);
						return signal;
					}//if ( guidInterfaces[j]== typeof(IBDA_SignalStatistics).GUID)
				}//for (int j=0; j < interfaceCount; ++j)
			}//for (int i=0; i < nodeTypeCount;++i)
			Marshal.ReleaseComObject(topology);
			return null;
		}//IBDA_SignalStatistics GetTunerSignalStatistics()


		/// <summary>
		/// returns true if tuner is locked to a frequency and signalstrength/quality is > 0
		/// </summary>
		/// <returns>
		/// true: tuner has a signal and is locked
		/// false: tuner is not locked
		/// </returns>
		/// <remarks>
		/// Graph should be created and GetTunerSignalStatistics() should be called
		/// </remarks>
		public bool SignalPresent()
		{
			//if we dont have an IBDA_SignalStatistics interface then return
			if (m_TunerStatistics==null) return false;
			int strength=0;
			int quality=0;
			byte locked=0;
			byte present=0;

			//get the signal strength
			int hr=m_TunerStatistics.get_SignalStrength(out strength);
			if (hr!=0) strength=0;
			
			//get the signal quality
			hr=m_TunerStatistics.get_SignalQuality(out quality);
			if (hr!=0) quality=0;
			
			//is the tuner locked?
			hr=m_TunerStatistics.get_SignalLocked(out locked);
			if (hr!=0) locked=0;

			//is a signal present?
			hr=m_TunerStatistics.get_SignalPresent(out present);
			if (hr!=0) present=0;

			//if tuner is locked and quality is ok then return true;
			if (locked>0 || quality>0)
			{
				return true;
			}
			return false;
		}//public bool SignalPresent()

		/// <summary>
		/// not used
		/// </summary>
		/// <returns>-1</returns>
		public long VideoFrequency()
		{
			return -1;
		}
		
		private bool CreateSinkSource(string fileName)
		{
			if(m_graphState!=State.Created)
				return false;
			int		hr				= 0;
			IPin	pinObj0		= null;
			IPin	pinObj1		= null;
			IPin	outPin		= null;

			Log.Write("DVBGraphBDA:CreateSinkSource()");

			Log.Write("DVBGraphBDA:Add streambuffersink()");
			hr=m_graphBuilder.AddFilter((IBaseFilter)m_StreamBufferSink,"StreamBufferSink");
			if(hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED cannot add StreamBufferSink");
				return false;
			}			
			Log.Write("DVBGraphBDA:Add mpeg2 analyzer()");
			hr=m_graphBuilder.AddFilter((IBaseFilter)m_mpeg2Analyzer,"Mpeg2 Analyzer");
			if(hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED cannot add mpeg2 analyzer to graph");
				return false;
			}

			Log.Write("DVBGraphBDA:find mpeg2 analyzer input pin()");
			pinObj0=DirectShowUtil.FindPinNr((IBaseFilter)m_mpeg2Analyzer,PinDirection.Input,0);
			if(pinObj0 == null)
			{
				Log.Write("DVBGraphBDA:FAILED cannot find mpeg2 analyzer input pin");
				return false;
			}
			Log.Write("DVBGraphBDA:connect demux video output->mpeg2 analyzer");
			hr=m_graphBuilder.Connect(m_DemuxVideoPin, pinObj0) ;
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED to connect demux video output->mpeg2 analyzer");
				return false;
			}

			Log.Write("DVBGraphBDA:mpeg2 analyzer output->streambuffersink in");
			pinObj1 = DirectShowUtil.FindPinNr((IBaseFilter)m_mpeg2Analyzer, PinDirection.Output, 0);	
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED cannot find mpeg2 analyzer output pin");
				return false;
			}
			IPin pinObj2 = DirectShowUtil.FindPinNr((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 0);	
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED cannot find SBE input pin");
				return false;
			}
			hr=m_graphBuilder.Connect(pinObj1, pinObj2) ;
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED to connect mpeg2 analyzer->streambuffer sink");
				return false;
			}
			pinObj2 = DirectShowUtil.FindPinNr((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 1);	
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED cannot find SBE input pin#2");
				return false;
			}
			hr=m_graphBuilder.Connect(m_DemuxAudioPin, pinObj2) ;
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:FAILED to connect mpeg2 demuxer audio out->streambuffer sink in#2");
				return false;
			}

			int ipos=fileName.LastIndexOf(@"\");
			string strDir=fileName.Substring(0,ipos);

			m_StreamBufferConfig	= new StreamBufferConfig();
			m_IStreamBufferConfig	= (IStreamBufferConfigure) m_StreamBufferConfig;
			// setting the timeshift behaviors
			IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_IStreamBufferConfig;
			IntPtr subKey = IntPtr.Zero;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr=pTemp.SetHKEY(subKey);
			
			
			Log.Write("DVBGraphBDA:set timeshift folder to:{0}", strDir);
			hr = m_IStreamBufferConfig.SetDirectory(strDir);	
			if(hr != 0)
			{
				Log.Write("DVBGraphBDA:FAILED to set timeshift folder to:{0}", strDir);
				return false;
			}
			hr = m_IStreamBufferConfig.SetBackingFileCount(6, 8);    //4-6 files
			if(hr != 0)
				return false;
			
			hr = m_IStreamBufferConfig.SetBackingFileDuration(300); // 60sec * 4 files= 4 mins
			if(hr != 0)
				return false;

			subKey = IntPtr.Zero;
			HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_StreamBufferSink;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr = pConfig.SetHKEY(subKey);

			
			Log.Write("DVBGraphBDA:set timeshift file to:{0}", fileName);
			// lock on the 'filename' file
			if(m_IStreamBufferSink.LockProfile(fileName) != 0)
			{
				Log.Write("DVBGraphBDA:FAILED to set timeshift file to:{0}", fileName);
				return false;
			}
			if(pinObj0 != null)
				Marshal.ReleaseComObject(pinObj0);
			if(pinObj1 != null)
				Marshal.ReleaseComObject(pinObj1);
			if(pinObj2 != null)
				Marshal.ReleaseComObject(pinObj2);
			if(outPin != null)
				Marshal.ReleaseComObject(outPin);

			return true;
		}//private bool CreateSinkSource(string fileName)
		
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
			if (m_graphState != State.TimeShifting) 
				return false;
			if (m_StreamBufferSink == null) 
				return false;

			Log.Write("DVBGraphBDA:StartRecording()");
			uint iRecordingType=0;
			if (bContentRecording) 
				iRecordingType = 0;
			else 
				iRecordingType = 1;										

			IntPtr recorderObj;
			if (m_IStreamBufferSink.CreateRecorder(strFileName, iRecordingType, out recorderObj) !=0 ) 
				return false;

			object objRecord = Marshal.GetObjectForIUnknown(recorderObj);
			if (objRecord == null) 
				if (m_recControl == null) 
				{
					Log.Write("DVBGraphBDA:FAILED to get IRecorder");
					return false;
				}
      
			Marshal.Release(recorderObj);

			m_recControl = objRecord as IStreamBufferRecordControl;
			if (m_recControl == null) 
			{
				Log.Write("DVBGraphBDA:FAILED to get IStreamBufferRecordControl");
				return false;
			}
			long lStartTime = 0;

			// if we're making a reference recording
			// then record all content from the past as well
			if (!bContentRecording)
			{
				// so set the startttime...
				uint uiSecondsPerFile;
				uint uiMinFiles, uiMaxFiles;
				m_IStreamBufferConfig.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
				m_IStreamBufferConfig.GetBackingFileDuration(out uiSecondsPerFile);
				lStartTime = uiSecondsPerFile;
				lStartTime *= (long)uiMaxFiles;

				// if start of program is given, then use that as our starttime
				if (timeProgStart.Year > 2000)
				{
					TimeSpan ts = DateTime.Now - timeProgStart;
					Log.Write("DVBGraphBDA: Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
						timeProgStart.Hour, timeProgStart.Minute, timeProgStart.Second,
						ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);
															
					lStartTime = (long)ts.TotalSeconds;
				}
				else Log.Write("DVBGraphBDA: record entire timeshift buffer");
      
				TimeSpan tsMaxTimeBack = DateTime.Now - m_StartTime;
				if (lStartTime > tsMaxTimeBack.TotalSeconds)
				{
					lStartTime = (long)tsMaxTimeBack.TotalSeconds;
				}
        

				lStartTime *= -10000000L;//in reference time 
			}//if (!bContentRecording)
			if (m_recControl.Start(ref lStartTime) != 0) 
			{
				//could not start recording...
				if (lStartTime != 0)
				{
					// try recording from livepoint instead from the past
					lStartTime = 0;
					if (m_recControl.Start(ref lStartTime) != 0)
						return false;
				}
				else
					return false;
			}
			m_graphState = State.Recording;
			return true;
		}//public bool StartRecording(int country,AnalogVideoStandard standard,int iChannelNr, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
	    


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
		public bool StartTimeShifting(int country,AnalogVideoStandard standard, int iChannel, string strFileName)
		{
			if(m_graphState!=State.Created)
				return false;
			
			Log.Write("DVBGraphBDA:StartTimeShifting()");

			if(CreateSinkSource(strFileName))
			{
				if(m_mediaControl == null) 
				{
					m_mediaControl = (IMediaControl) m_graphBuilder;
				}
				//now start the graph
				Log.Write("DVBGraphBDA: start graph");
				int hr=m_mediaControl.Run();
				if (hr<0)
				{
					Log.Write("DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
				}
				graphRunning=true;
				m_graphState = State.TimeShifting;
			}
			else 
			{
				Log.Write("DVBGraphBDA:Unable to create sinksource()");
				return false;
			}
			TuneChannel(standard, iChannel,country);

			Log.Write("DVBGraphBDA:timeshifting started");			
			return true;
		}//public bool StartTimeShifting(int country,AnalogVideoStandard standard, int iChannel, string strFileName)
		
		/// <summary>
		/// Stops timeshifting and cleans up the timeshifting files
		/// </summary>
		/// <returns>boolean indicating if timeshifting is stopped or not</returns>
		/// <remarks>
		/// Graph should be timeshifting 
		/// </remarks>
		public bool StopTimeShifting()
		{
			if (m_graphState != State.TimeShifting) return false;
			Log.Write("DVBGraphBDA: StopTimeShifting()");
			m_mediaControl.Stop();
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}//public bool StopTimeShifting()
		
		/// <summary>
		/// Finds and connects pins
		/// </summary>
		/// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
		/// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
		/// <returns>true if succeeded, false if failed</returns>
		private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter) 
		{
			return ConnectFilters(ref UpstreamFilter, ref DownstreamFilter, 0);
		}//bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter) 

		/// <summary>
		/// Finds and connects pins
		/// </summary>
		/// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
		/// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
		/// <param name="preferredOutputPin">The one-based index of the preferred output pin to use on the Upstream filter.  This is tried first. Pin 1 = 1, Pin 2 = 2, etc</param>
		/// <returns>true if succeeded, false if failed</returns>
		private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter, int preferredOutputPin) 
		{
			if (UpstreamFilter == null || DownstreamFilter == null)
				return false;

			int ulFetched = 0;
			int hr = 0;
			IEnumPins pinEnum;

			hr = UpstreamFilter.EnumPins( out pinEnum );
			if((hr < 0) || (pinEnum == null))
				return false;

			#region Attempt to connect preferred output pin first
			if (preferredOutputPin > 0) 
			{
				IPin[] outPin = new IPin[1];
				int outputPinCounter = 0;
				while(pinEnum.Next(1, outPin, out ulFetched) == 0) 
				{    
					PinDirection pinDir;
					outPin[0].QueryDirection(out pinDir);

					if (pinDir == PinDirection.Output)
					{
						outputPinCounter++;
						if (outputPinCounter == preferredOutputPin) // Go and find the input pin.
						{
							IEnumPins downstreamPins;

							DownstreamFilter.EnumPins(out downstreamPins);

							IPin[] dsPin = new IPin[1];
							while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
							{
								PinDirection dsPinDir;
								dsPin[0].QueryDirection(out dsPinDir);
								if (dsPinDir == PinDirection.Input)
								{
									hr = m_graphBuilder.Connect(outPin[0], dsPin[0]);
									if(hr != 0) 
									{
										Marshal.ReleaseComObject(dsPin[0]);
										break;
									} 
									else 
									{
										return true;
									}
								}
							}
							Marshal.ReleaseComObject(downstreamPins);
						}
					}
					Marshal.ReleaseComObject(outPin[0]);
				}
				pinEnum.Reset();        // Move back to start of enumerator
			}
			#endregion

			IPin[] testPin = new IPin[1];
			while(pinEnum.Next(1, testPin, out ulFetched) == 0) 
			{    
				PinDirection pinDir;
				testPin[0].QueryDirection(out pinDir);

				if(pinDir == PinDirection.Output) // Go and find the input pin.
				{
					IEnumPins downstreamPins;

					DownstreamFilter.EnumPins(out downstreamPins);

					IPin[] dsPin = new IPin[1];
					while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
					{
						PinDirection dsPinDir;
						dsPin[0].QueryDirection(out dsPinDir);
						if (dsPinDir == PinDirection.Input)
						{
							hr = m_graphBuilder.Connect(testPin[0], dsPin[0]);
							if(hr != 0) 
							{
								Marshal.ReleaseComObject(dsPin[0]);
								continue;
							} 
							else 
							{
								return true;
							}
						}//if (dsPinDir == PinDirection.Input)
					}//while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
					Marshal.ReleaseComObject(downstreamPins);
				}//if(pinDir == PinDirection.Output) // Go and find the input pin.
				Marshal.ReleaseComObject(testPin[0]);
			}//while(pinEnum.Next(1, testPin, out ulFetched) == 0) 
			Marshal.ReleaseComObject(pinEnum);
			return false;
		}//private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter, int preferredOutputPin) 

		/// <summary>
		/// This is the function for setting up a local tuning space.
		/// </summary>
		/// <returns>true if succeeded, fale if failed</returns>
		private bool setupTuningSpace() 
		{
			//int hr = 0;

			Log.Write("DVBGraphBDA: setupTuningSpace()");
			if(m_NetworkProvider == null) 
			{
				Log.Write("DVBGraphBDA: FAILED:network provider is null ");
				return false;
			}
			System.Guid classID;
			int hr=m_NetworkProvider.GetClassID(out classID);
//			if (hr <=0)
//			{
//				Log.Write("DVBGraphBDA: FAILED:cannot get classid of network provider");
//				return false;
//			}

			string strClassID = classID.ToString();
			strClassID = strClassID.ToLower();
			switch (strClassID) 
			{
				case "0dad2fdd-5fd7-11d3-8f50-00c04f7971e2":
					Log.Write("DVBGraphBDA: Network=ATSC");
					m_NetworkType = NetworkType.ATSC;
					break;
				case "dc0c0fe7-0485-4266-b93f-68fbf80ed834":
					Log.Write("DVBGraphBDA: Network=DVB-C");
					m_NetworkType = NetworkType.DVBC;
					break;
				case "fa4b375a-45b4-4d45-8440-263957b11623":
					Log.Write("DVBGraphBDA: Network=DVB-S");
					m_NetworkType = NetworkType.DVBS;
					break;
				case "216c62df-6d7f-4e9a-8571-05f14edb766a":
					Log.Write("DVBGraphBDA: Network=DVB-T");
					m_NetworkType = NetworkType.DVBT;
					break;
				default:
					Log.Write("DVBGraphBDA: FAILED:unknown network type:{0} ",classID);
					return false;
			}//switch (strClassID) 

			TunerLib.ITuningSpaceContainer TuningSpaceContainer = (TunerLib.ITuningSpaceContainer) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_SystemTuningSpaces, true));
			if(TuningSpaceContainer == null)
			{
				Log.Write("DVBGraphBDA: Failed to get ITuningSpaceContainer");
				return false;
			}

			TunerLib.ITuningSpaces myTuningSpaces = null;
			string uniqueName="";
			switch (m_NetworkType) 
			{
				case NetworkType.ATSC:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_ATSCTuningSpace);
					//ATSCInputType = "Antenna"; // Need to change to allow cable
					uniqueName="Mediaportal ATSC";
				} break;
				case NetworkType.DVBC:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBTuningSpace);
					uniqueName="Mediaportal DVB-C";
				} break;
				case NetworkType.DVBS:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBSTuningSpace);
					uniqueName="Mediaportal DVB-S";
				} break;
				case NetworkType.DVBT:
				{
					myTuningSpaces = TuningSpaceContainer._TuningSpacesForCLSID(ref TuningSpaces.CLSID_DVBTuningSpace);
					uniqueName="Mediaportal DVB-T";
				} break;
			}//switch (m_NetworkType) 

			Log.Write("DVBGraphBDA: check available tuningspaces");
			TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;

			int Count = 0;
			Count = myTuningSpaces.Count;
			if(Count > 0)
			{
				Log.Write("DVBGraphBDA: found {0} tuning spaces", Count);
				TunerLib.IEnumTuningSpaces TuneEnum = myTuningSpaces.EnumTuningSpaces;
				if (TuneEnum !=null)
				{
					uint ulFetched = 0;
					TunerLib.TuningSpace tuningSpaceFound;
					int counter = 0;
					TuneEnum.Reset();
					for (counter=0; counter < Count; counter++)
					{
						TuneEnum.Next(1, out tuningSpaceFound, out ulFetched);
						if (ulFetched==1 )
						{
							if (tuningSpaceFound.UniqueName==uniqueName)
							{
								myTuner.TuningSpace = tuningSpaceFound;
								Log.Write("DVBGraphBDA: used tuningspace:{0} {1} {2}", counter, tuningSpaceFound.UniqueName,tuningSpaceFound.FriendlyName);
								if (myTuningSpaces!=null)
									Marshal.ReleaseComObject(myTuningSpaces);
								if (TuningSpaceContainer!=null)
									Marshal.ReleaseComObject(TuningSpaceContainer);
								return true;
							}//if (tuningSpaceFound.UniqueName==uniqueName)
						}//if (ulFetched==1 )
					}//for (counter=0; counter < Count; counter++)
					if (myTuningSpaces!=null)
						Marshal.ReleaseComObject(myTuningSpaces);
				}//if (TuneEnum !=null)
			}//if(Count > 0)

			TunerLib.ITuningSpace TuningSpace ;
			Log.Write("DVBGraphBDA: create new tuningspace");
			switch (m_NetworkType) 
			{
				case NetworkType.ATSC: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_ATSCTuningSpace, true));
					TunerLib.IATSCTuningSpace myTuningSpace = (TunerLib.IATSCTuningSpace) TuningSpace;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_ATSCNetworkProvider);
					myTuningSpace.InputType = TunerLib.tagTunerInputType.TunerInputAntenna;
					myTuningSpace.MaxChannel			= 99;
					myTuningSpace.MaxMinorChannel		= 999;
					myTuningSpace.MaxPhysicalChannel	= 69;
					myTuningSpace.MinChannel			= 1;
					myTuningSpace.MinMinorChannel		= 0;
					myTuningSpace.MinPhysicalChannel	= 2;
					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;

					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_ATSCLocator, true));
					TunerLib.IATSCLocator myLocator = (TunerLib.IATSCLocator) DefaultLocator;

					myLocator.CarrierFrequency	 = -1;
					myLocator.InnerFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Modulation				= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.OuterFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.PhysicalChannel		= -1;
					myLocator.SymbolRate				= -1;
					myLocator.TSID							= -1;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;
				} break;//case NetworkType.ATSC: 
				
				case NetworkType.DVBC: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_DVBTuningSpace, true));
					TunerLib.IDVBTuningSpace2 myTuningSpace = (TunerLib.IDVBTuningSpace2) TuningSpace;
					myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Cable;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBCNetworkProvider);

					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;
					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_DVBCLocator, true));
					TunerLib.IDVBCLocator myLocator = (TunerLib.IDVBCLocator) DefaultLocator;

					myLocator.CarrierFrequency	= -1;
					myLocator.InnerFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Modulation				= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.OuterFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.SymbolRate				= -1;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;
				} break;//case NetworkType.DVBC: 
				
				case NetworkType.DVBS: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_DVBSTuningSpace, true));
					TunerLib.IDVBSTuningSpace myTuningSpace = (TunerLib.IDVBSTuningSpace) TuningSpace;
					myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Satellite;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBSNetworkProvider);
					myTuningSpace.LNBSwitch = -1;
					myTuningSpace.HighOscillator = -1;
					myTuningSpace.LowOscillator = 11250000;
					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;
					
					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_DVBSLocator, true));
					TunerLib.IDVBSLocator myLocator = (TunerLib.IDVBSLocator) DefaultLocator;
					
					myLocator.CarrierFrequency		= -1;
					myLocator.InnerFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.OuterFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Modulation					= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.SymbolRate					= -1;
					myLocator.Azimuth							= -1;
					myLocator.Elevation						= -1;
					myLocator.OrbitalPosition			= -1;
					myLocator.SignalPolarisation	= (TunerLib.Polarisation) Polarisation.BDA_POLARISATION_NOT_SET;
					myLocator.WestPosition				= false;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;
				} break;//case NetworkType.DVBS: 
				
				case NetworkType.DVBT: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_DVBTuningSpace, true));
					TunerLib.IDVBTuningSpace2 myTuningSpace = (TunerLib.IDVBTuningSpace2) TuningSpace;
					myTuningSpace.SystemType = TunerLib.DVBSystemType.DVB_Terrestrial;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_DVBTNetworkProvider);
					myTuningSpace.FriendlyName=uniqueName;
					myTuningSpace.UniqueName=uniqueName;

					TunerLib.Locator DefaultLocator = (TunerLib.Locator) Activator.CreateInstance(Type.GetTypeFromCLSID(Locators.CLSID_DVBTLocator, true));
					TunerLib.IDVBTLocator myLocator = (TunerLib.IDVBTLocator) DefaultLocator;

					myLocator.CarrierFrequency		= -1;
					myLocator.Bandwidth						= -1;
					myLocator.Guard								= (TunerLib.GuardInterval) GuardInterval.BDA_GUARD_NOT_SET;
					myLocator.HAlpha							= (TunerLib.HierarchyAlpha) HierarchyAlpha.BDA_HALPHA_NOT_SET;
					myLocator.InnerFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.InnerFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.LPInnerFEC					= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.LPInnerFECRate			= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.Mode								= (TunerLib.TransmissionMode) TransmissionMode.BDA_XMIT_MODE_NOT_SET;
					myLocator.Modulation					= (TunerLib.ModulationType) ModulationType.BDA_MOD_NOT_SET;
					myLocator.OtherFrequencyInUse	= false;
					myLocator.OuterFEC						= (TunerLib.FECMethod) FECMethod.BDA_FEC_METHOD_NOT_SET;
					myLocator.OuterFECRate				= (TunerLib.BinaryConvolutionCodeRate) BinaryConvolutionCodeRate.BDA_BCC_RATE_NOT_SET;
					myLocator.SymbolRate					= -1;

					myTuningSpace.DefaultLocator = DefaultLocator;
					TuningSpaceContainer.Add((TunerLib.TuningSpace)myTuningSpace);
					myTuner.TuningSpace=(TunerLib.TuningSpace)TuningSpace;

				} break;//case NetworkType.DVBT: 
			}//switch (m_NetworkType) 
			return true;
		}//private bool setupTuningSpace() 

		/// <summary>
		/// Used to find the Network Provider for addition to the graph.
		/// </summary>
		/// <param name="ClassID">The filter category to enumerate.</param>
		/// <param name="FriendlyName">An identifier based on the DevicePath, used to find the device.</param>
		/// <param name="device">The filter that has been found.</param>
		/// <returns>true of succeeded, false if failed</returns>
		private bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device) 
		{
			int hr;
			ICreateDevEnum		sysDevEnum	= null;
			UCOMIEnumMoniker	enumMoniker	= null;
			
			sysDevEnum = (ICreateDevEnum) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum, true));
			// Enumerate the filter category
			hr = sysDevEnum.CreateClassEnumerator(ref ClassID, out enumMoniker, 0);
			if( hr != 0 )
				throw new NotSupportedException( "No devices in this category" );

			int ulFetched = 0;
			UCOMIMoniker[] deviceMoniker = new UCOMIMoniker[1];
			while(enumMoniker.Next(1, deviceMoniker, out ulFetched) == 0) // while == S_OK
			{
				object bagObj = null;
				Guid bagId = typeof( IPropertyBag ).GUID;
				deviceMoniker[0].BindToStorage(null, null, ref bagId, out bagObj);
				IPropertyBag propBag = (IPropertyBag) bagObj;
				object val = "";
				propBag.Read("FriendlyName", ref val, IntPtr.Zero); 
				string Name = val as string;
				val = "";
				if(String.Compare(Name.ToLower(), FriendlyName.ToLower()) == 0) // If found
				{
					object filterObj = null;
					System.Guid filterID = typeof(IBaseFilter).GUID;
					deviceMoniker[0].BindToObject(null, null, ref filterID, out filterObj);
					device = filterObj;
					
					filterObj = null;
					if(device == null) 
					{
						continue;
					} 
					else 
					{
						return true;
					}
				}//if(String.Compare(Name.ToLower(), FriendlyName.ToLower()) == 0) // If found
				Marshal.ReleaseComObject(deviceMoniker[0]);
			}//while(enumMoniker.Next(1, deviceMoniker, out ulFetched) == 0) // while == S_OK
			device = null;
			return false;
		}//private bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device) 


		
		public void Process()
		{
			if (m_Event==null) return;
			if (m_TIF==null) return;
			if (GuideDataEvent.mutexServiceChanged.WaitOne(1,true))
			{
				IGuideData data= m_TIF as IGuideData;
				if (data==null) return;
				int iFetched;
				IEnumTuneRequests varRequests;
				data.GetServices(out varRequests);
				if (varRequests==null) return;

				DVBTChannel DvbtChannel = new DVBTChannel();
				DVBCChannel DvbcChannel = new DVBCChannel();
				DVBSChannel DvbsChannel = new DVBSChannel();
				int         gotAll=0;
				TunerLib.ITuneRequest[] tunerequests = new TunerLib.ITuneRequest[1];
				while(varRequests.Next(1,  tunerequests, out iFetched) == 0) 
				{
					if (tunerequests[0]==null) return;
					IEnumGuideDataProperties enumProgramProperties;
					data.GetServiceProperties(tunerequests[0],out enumProgramProperties);
					if (enumProgramProperties!=null)
					{
						IGuideDataProperty[] properties = new IGuideDataProperty[1];
						while (enumProgramProperties.Next(1,properties, out iFetched) ==0)
						{
							if (properties[0]==null) return;
							object chanValue;
							int chanLanguage;
							string chanName;
							properties[0].Name( out chanName);
							properties[0].Value(out chanValue);
							properties[0].Language(out chanLanguage);

							if (Network()==NetworkType.DVBT)
							{
								//service name							value						language
								//Description.ID						ONID:TSID:SID			0
								//Description.Name					Nederland 1				0
								//Provider.Name							Digitenne					0
								//Description.ServiceType		1									0

								if (chanName=="Description.ID")
								{
									string[] parts = new string[11];
									string id=(string)chanValue;
									parts=id.Split(new char[]{':'},10);
									if (parts.Length>=3)
									{
										try
										{
											DvbtChannel.ONID = Int32.Parse(parts[0]);
											DvbtChannel.TSID = Int32.Parse(parts[1]);
											DvbtChannel.SID  = Int32.Parse(parts[2]);

											//TODO: determine if channel found is an radio or tv channel
											DvbtChannel.IsTv=true;
											DvbtChannel.IsRadio=true;
											gotAll++;
										}
										catch(Exception)
										{
											Log.Write("DVBGraphBDA: unknown service description:{0}", chanValue);
											gotAll=0;
										}
									}//if (parts.Length>=3)
								}//if (chanName=="Description.ID")

								if (chanName=="Description.Name")
								{
									DvbtChannel.ChannelName=(string)chanValue;
									gotAll++;
								}
								
								if (chanName=="Provider.Name")
								{
									DvbtChannel.NetworkName=(string)chanValue;
									gotAll++;
								}
								
								if (chanName=="Description.ServiceType")
								{
									DvbtChannel.NetworkType=Int32.Parse( chanValue.ToString());
									gotAll++;
								}
								
								if (gotAll==4)
								{
									bool add=true;
									for (int x=0; x < channelList.Count;++x)
									{
										DVBTChannel chan=(DVBTChannel)channelList[x];
										if (chan.ONID==DvbtChannel.ONID && chan.TSID==DvbtChannel.TSID && chan.SID==DvbtChannel.SID)
										{	
											add=false;
											break;
										}
									}
	
									if (add)
									{
										Log.Write("Network:{0} channel:{1} ONID:{2} TSID:{3} SID:{4}",DvbtChannel.NetworkName,DvbtChannel.ChannelName,DvbtChannel.ONID,DvbtChannel.TSID,DvbtChannel.SID);
										channelList.Add(DvbtChannel);
									}
									gotAll=0;
									DvbtChannel=new DVBTChannel();
								}//if (gotAll==4)
							}//if (Network()==NetworkType.DVBT)
							else if (Network()==NetworkType.DVBC)
							{
								//todo: parse guide data for DVB-C
								Log.Write("DVBC:name:{0} value:{1} language:{2}", chanName,chanValue,chanLanguage);

							}
							else if (Network()==NetworkType.DVBS)
							{
								//todo: parse guide data for DVB-S
								Log.Write("DVBS:name:{0} value:{1} language:{2}", chanName,chanValue,chanLanguage);
							}						
							else if (Network()==NetworkType.ATSC)
							{
								//todo: parse guide data for ATSC
								Log.Write("ATSC:name:{0} value:{1} language:{2}", chanName,chanValue,chanLanguage);
							}						
						}//while (enumProgramProperties.Next(1,properties, out iFetched) ==0)
					}//if (enumProgramProperties!=null)
				}//while(varRequests.Next(1,  tunerequests, out iFetched) == 0) 
			}//if (GuideDataEvent.mutexServiceChanged.WaitOne(1,true))
/*
			if (GuideDataEvent.mutexProgramChanged.WaitOne(1,true))
			{
				IGuideData data= m_TIF as IGuideData;
				if (data!=null)
				{
					int iFetched;
					object[] varResults = new object[1];
					string[] channelParams = new string[20];
					IEnumVARIANT varEnum;
					Log.Write("GetGuideProgramIDs");
					data.GetGuideProgramIDs(out varEnum);
					if (varEnum!=null)
					{
						while(varEnum.Next(1,  varResults, out iFetched) == 0) 
						{
							IEnumGuideDataProperties enumProgramProperties;
							data.GetProgramProperties(varResults[0],out enumProgramProperties);
							if (enumProgramProperties!=null)
							{
								IGuideDataProperty[] properties = new IGuideDataProperty[1];
								while (enumProgramProperties.Next(1,properties, out iFetched) ==0)
								{
									object chanValue;
									int chanLanguage;
									string chanName;
									properties[0].Name( out chanName);
									properties[0].Value(out chanValue);
									properties[0].Language(out chanLanguage);
									Log.Write("channel name:{0} value:{1} language:{2}",chanName, chanValue, chanLanguage);
						
									if (Network() == NetworkType.DVBT)
									{
										//name											value											language
										//Description.ID						ONID:TSID:SID							0
										//Description.Title					title of program					0
										//Description.One Sentence	description of program		0
									}
								}
							}
						}
					}
				}
			}
*/
		}//public void Process()

		//not used
		public void TuneFrequency(int frequency)
		{
		}

		
		//not used
		public PropertyPageCollection PropertyPages()
		{
			return null;
		}
		
		//not used
		public IBaseFilter AudiodeviceFilter()
		{
			return null;
		}

		//not used
		public bool SupportsFrameSize(Size framesize)
		{	
			return false;
		}

		/// <summary>
		/// return the network type (DVB-T, DVB-C, DVB-S)
		/// </summary>
		/// <returns>network type</returns>
		public NetworkType Network()
		{
				return m_NetworkType;
		}
		
		/// <summary>
		/// Tune to a specific channel
		/// </summary>
		/// <param name="tuningObject">
		/// tuning object. Differs for dvb-c/dvb-s/dvb-t.
		/// For DVB-T this is simply an int which contains the desired frequency in KHz
		/// For DVB-C this is simply an int which contains the desired frequency in KHz
		/// For DVB-S this is a DVBSChannel object
		/// </param>
		/// <remarks>
		/// Graph should be created 
		/// </remarks>
		public void Tune(object tuningObject)
		{
			//if no network provider then return;
			if (m_NetworkProvider==null) return;
			if (tuningObject==null) return;
			channelList = new ArrayList();

			//start viewing if we're not yet viewing
			if (!graphRunning)
			{
				StartViewing(AnalogVideoStandard.None,-1,-1);
			}

			//for DVB-T
			if (Network() == NetworkType.DVBT)
			{
				int frequency=0;
				try
				{
					frequency=(int)tuningObject;
				}
				catch( Exception )
				{
					return;
				}
				//get the ITuner from the network provider
				TunerLib.TuneRequest newTuneRequest = null;
				TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
				if (myTuner ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() tuner=null");
					return;
				}

				//get the IDVBTuningSpace2 from the tuner
				TunerLib.IDVBTuningSpace2 myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
				if (myTuningSpace ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() tuningspace=null");
					return;
				}

				//create a new tuning request
				newTuneRequest = myTuningSpace.CreateTuneRequest();
				if (newTuneRequest ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not create new tuningrequest");
					return;
				}
				
				TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as  TunerLib.IDVBTuneRequest;
				if (myTuneRequest ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not get IDVBTuneRequest");
					return;
				}
				
				//get the IDVBTLocator interface
				TunerLib.IDVBTLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBTLocator;	
				if (myLocator == null)
					myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBTLocator;
				if (myLocator ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not get IDVBTLocator");
					return;
				}


				//set the properties for the new tuning request. For DVB-T we only set the frequency
				myLocator.CarrierFrequency		= frequency;
				myTuneRequest.ONID						= -1;					//original network id
				myTuneRequest.TSID						= -1;					//transport stream id
				myTuneRequest.SID							= -1;					//service id
				myTuneRequest.Locator					= (TunerLib.Locator)myLocator;

				//and submit the tunre request
				myTuner.TuneRequest  = newTuneRequest;
				Marshal.ReleaseComObject(myTuneRequest);

				currentFrequency=frequency;
			}//if (Network() == NetworkType.DVBT)
			else if (Network() == NetworkType.DVBC)
			{
				//get the ITuner from the network provider
				TunerLib.TuneRequest newTuneRequest = null;
				TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
				if (myTuner ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() tuner=null");
					return;
				}

				//get the IDVBTuningSpace2 from the tuner
				TunerLib.IDVBTuningSpace2 myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
				if (myTuningSpace ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() tuningspace=null");
					return;
				}

				//create a new tuning request
				newTuneRequest = myTuningSpace.CreateTuneRequest();
				if (newTuneRequest ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not create new tuningrequest");
					return;
				}
				
				TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as  TunerLib.IDVBTuneRequest;
				if (myTuneRequest ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not get IDVBTuneRequest");
					return;
				}
				
				//get the IDVBCLocator interface
				TunerLib.IDVBCLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBCLocator;	
				if (myLocator == null)
					myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBCLocator;
				if (myLocator ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not get IDVBCLocator");
					return;
				}

				//set the properties for the new tuning request. For DVB-C we only set the frequency
				DVBCChannel chan=(DVBCChannel)tuningObject;
				myLocator.CarrierFrequency		= chan.carrierFrequency;
				myLocator.InnerFEC						= (TunerLib.FECMethod)chan.innerFec;
				myLocator.SymbolRate					= chan.symbolRate;
				myLocator.Modulation					= (TunerLib.ModulationType)chan.modulation;

				myTuneRequest.ONID						= chan.ONID;	//original network id
				myTuneRequest.TSID						= chan.TSID;	//transport stream id
				myTuneRequest.SID							= chan.SID;		//service id
				myTuneRequest.Locator					= (TunerLib.Locator)myLocator;

				//and submit the tunre request
				myTuner.TuneRequest  = newTuneRequest;
				Marshal.ReleaseComObject(myTuneRequest);
				currentFrequency=(int)tuningObject;
			}
			else if (Network() == NetworkType.DVBS)
			{
				//get the ITuner from the network provider
				TunerLib.TuneRequest newTuneRequest = null;
				TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
				if (myTuner ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() tuner=null");
					return;
				}

				//get the IDVBTuningSpace2 from the tuner
				TunerLib.IDVBTuningSpace2 myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
				if (myTuningSpace ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() tuningspace=null");
					return;
				}

				//create a new tuning request
				newTuneRequest = myTuningSpace.CreateTuneRequest();
				if (newTuneRequest ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not create new tuningrequest");
					return;
				}
				
				TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as  TunerLib.IDVBTuneRequest;
				if (myTuneRequest ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not get IDVBTuneRequest");
					return;
				}
				
				//get the IDVBSLocator interface
				TunerLib.IDVBSLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBSLocator;	
				if (myLocator == null)
					myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBSLocator;
				if (myLocator ==null)
				{
					Log.Write("DVBGraphBDA: failed Tune() could not get IDVBSLocator");
					return;
				}

				DVBSChannel chan=(DVBSChannel)tuningObject;
				//set the properties for the new tuning request. 
				myLocator.CarrierFrequency		= chan.carrierFrequency;
				myLocator.InnerFEC						= (TunerLib.FECMethod)chan.innerFec;
				myLocator.SignalPolarisation	= (TunerLib.Polarisation)chan.polarisation;
				myLocator.SymbolRate					= chan.symbolRate;
				myTuneRequest.ONID						= chan.ONID;	//original network id
				myTuneRequest.TSID						= chan.TSID;	//transport stream id
				myTuneRequest.SID							= chan.SID;		//service id
				myTuneRequest.Locator					= (TunerLib.Locator)myLocator;

				//and submit the tune request
				myTuner.TuneRequest  = newTuneRequest;
				Marshal.ReleaseComObject(myTuneRequest);
				currentFrequency=(int)tuningObject;
			}
			else if (Network() == NetworkType.ATSC)
			{
				//todo: add tuning for ATSC
			}
		}//public void Tune(object tuningObject)
		
		/// <summary>
		/// Store any new tv and/or radio channels found in the tvdatabase
		/// </summary>
		/// <param name="radio">if true:Store radio channels found in the database</param>
		/// <param name="tv">if true:Store tv channels found in the database</param>
		public void StoreChannels(bool radio, bool tv)
		{
			//get list of current tv channels present in the database
			ArrayList tvChannels = new ArrayList();
			TVDatabase.GetChannels(ref tvChannels);

			//for each of the new channels found
			for (int x=0; x < channelList.Count;++x)
			{
				//if this is a DVB-T graph
				if (Network() == NetworkType.DVBT)
				{
					//get the DVBT channel
					DVBTChannel channel=(DVBTChannel)channelList[x];

					//if this is a tv channel and we should store tv channels in the database
					if ( (channel.IsTv && tv))
					{
						//check if this channel already exists in the database
						bool newChannel=true;
						int iChannelNumber=0;
						foreach (TVChannel tvchan in tvChannels)
						{
							if (tvchan.Name.Equals(channel.ChannelName))
							{
								//yes already exists
								iChannelNumber=tvchan.Number;
								newChannel=false;
								break;
							}
						}
						//if the channel found is not yet in the tv database
						if (newChannel)
						{
							//then add a new channel to the database
							TVChannel tvChan = new TVChannel();
							tvChan.Name=channel.ChannelName;
							tvChan.Number=channel.SID;
							tvChan.VisibleInGuide=true;
							iChannelNumber=tvChan.Number;
							TVDatabase.AddChannel(tvChan);
						}

						//set the dvb-t tuning parameters for the tv channel
						TVDatabase.MapDVBTChannel(channel.ChannelName,iChannelNumber, currentFrequency, channel.ONID,channel.TSID,channel.SID);
					}//if ( (channel.IsTv && tv))

					
					//if this is a radio channel and we should store radio channels in the database
					if (channel.IsRadio && radio)
					{
						//todo: store radio channels
					}//if (channel.IsRadio && radio)
				}//if (Network() == NetworkType.DVBT)
				else if (Network() == NetworkType.DVBC)
				{
					//get the DVBC channel
					DVBCChannel channel=(DVBCChannel)channelList[x];

					//if this is a tv channel and we should store tv channels in the database
					if ( (channel.IsTv && tv))
					{
						//check if this channel already exists in the database
						bool newChannel=true;
						int iChannelNumber=0;
						foreach (TVChannel tvchan in tvChannels)
						{
							if (tvchan.Name.Equals(channel.ChannelName))
							{
								//yes already exists
								iChannelNumber=tvchan.Number;
								newChannel=false;
								break;
							}
						}
						//if the channel found is not yet in the tv database
						if (newChannel)
						{
							//then add a new channel to the database
							TVChannel tvChan = new TVChannel();
							tvChan.Name=channel.ChannelName;
							tvChan.Number=channel.SID;
							tvChan.VisibleInGuide=true;
							iChannelNumber=tvChan.Number;
							TVDatabase.AddChannel(tvChan);
						}

						//set the dvb-t tuning parameters for the tv channel
						TVDatabase.MapDVBCChannel(channel.ChannelName,iChannelNumber, currentFrequency, channel.symbolRate,channel.innerFec,channel.modulation,channel.ONID,channel.TSID,channel.SID);
					}//if ( (channel.IsTv && tv))

					
					//if this is a radio channel and we should store radio channels in the database
					if (channel.IsRadio && radio)
					{
						//todo: store radio channels
					}//if (channel.IsRadio && radio)
				}
				else if (Network() == NetworkType.DVBS)
				{
					//get the DVBS channel
					DVBSChannel channel=(DVBSChannel)channelList[x];

					//if this is a tv channel and we should store tv channels in the database
					if ( (channel.IsTv && tv))
					{
						//check if this channel already exists in the database
						bool newChannel=true;
						int iChannelNumber=0;
						foreach (TVChannel tvchan in tvChannels)
						{
							if (tvchan.Name.Equals(channel.ChannelName))
							{
								//yes already exists
								iChannelNumber=tvchan.Number;
								newChannel=false;
								break;
							}
						}
						//if the channel found is not yet in the tv database
						if (newChannel)
						{
							//then add a new channel to the database
							TVChannel tvChan = new TVChannel();
							tvChan.Name=channel.ChannelName;
							tvChan.Number=channel.SID;
							tvChan.VisibleInGuide=true;
							iChannelNumber=tvChan.Number;
							TVDatabase.AddChannel(tvChan);
						}

						//set the dvb-t tuning parameters for the tv channel
						TVDatabase.MapDVBSChannel(channel.ChannelName,iChannelNumber, currentFrequency, channel.symbolRate,channel.innerFec,channel.polarisation,channel.ONID,channel.TSID,channel.SID);
					}//if ( (channel.IsTv && tv))

					
					//if this is a radio channel and we should store radio channels in the database
					if (channel.IsRadio && radio)
					{
						//todo: store radio channels
					}//if (channel.IsRadio && radio)
				}
				else if (Network() == NetworkType.ATSC)
				{
					//todo: store ATSC channels found in tvdatabase
				}
			}//for (int x=0; x < channelList.Count;++x)
		}//public void StoreChannels(bool radio, bool tv)

	}//public class DVBGraphBDA 
}//namespace MediaPortal.TV.Recording
//end of file
#endif