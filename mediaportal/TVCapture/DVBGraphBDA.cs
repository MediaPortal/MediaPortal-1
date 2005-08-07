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
//#define HW_PID_FILTERING
//#define DUMP
#define USEMTSWRITER
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
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;

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
	/// -radio
	/// </summary>
	public class DVBGraphBDA : MediaPortal.TV.Recording.IGraph
	{
		public  static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid( 0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
		public  static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid( 0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
		public  static Guid MEDIASUBTYPE_DVB_SI= new Guid( 0xe9dd31a3, 0x221d, 0x4adb, 0x85, 0x32, 0x9a, 0xf3, 0x9, 0xc1, 0xa4, 0x8);
		public  static Guid MEDIASUBTYPE_ATSC_SI= new Guid( 0xb3c7397c, 0xd303, 0x414d, 0xb3, 0x3c, 0x4e, 0xd2, 0xc9, 0xd2, 0x97, 0x33);
		enum MediaSampleContent :int 
		{
			TransportPacket,
			ElementaryStream,
			Mpeg2PSI,
			TransportPayload
		} ;
		static byte[] Mpeg2ProgramVideo = 
				{
					0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
					0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
					0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
					0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

					//0x051736=333667-> 10000000/333667 = 29.97fps
					//0x061A80=400000-> 10000000/400000 = 25fps
					0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
					0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
					0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
					0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
					0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
					0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
					0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
					0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
					0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
					0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
					0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
					0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
					0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
					0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
					0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
					//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
					0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
					0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
					0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
					//  .dwSequenceHeader [1]
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
		} ;

		static byte [] MPEG1AudioFormat = 
			{
				0x50, 0x00,             // format type      = 0x0050=WAVE_FORMAT_MPEG
				0x02, 0x00,             // channels
				0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
				0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
				0x01, 0x00,             // nBlockAlign      = 0x0300 = 768
				0x00, 0x00,             // wBitsPerSample   = 16
				0x16, 0x00,             // extra size       = 0x0016 = 22 bytes
				0x02, 0x00,             // fwHeadLayer
				0x00, 0xE8,0x03, 0x00,  // dwHeadBitrate
				0x01, 0x00,             // fwHeadMode
				0x01, 0x00,             // fwHeadModeExt
				0x01, 0x00,             // wHeadEmphasis
				0x16, 0x00,             // fwHeadFlags
				0x00, 0x00, 0x00, 0x00, // dwPTSLow
				0x00, 0x00, 0x00, 0x00  // dwPTSHigh
			} ;
		
		#region imports
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

		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern bool EventMsg(int eventType,[In] IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int SetAppHandle([In] IntPtr hnd/*,[In, MarshalAs(System.Runtime.InteropServices.UnmanagedType.FunctionPtr)] Delegate Callback*/);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern void PidCallback([In] IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int MenuItemClick([In] int ptr);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetMenuHandle([In] long menu);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetPidMap(DShowNET.IPin filter, ref uint pid, ref uint mediasampletype);
		[DllImport("dvblib.dll", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetupDemuxer(IPin pin,int pid,IPin pin1,int pid1,IPin pin2,int pid2);
		[DllImport("dvblib.dll", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetupDemuxerPin(IPin pin,int pid,int elementaryStream, bool unmapOtherPins);

		#endregion

		#region class member variables
		enum State
		{ 
			None, 
			Created, 
			TimeShifting,
			Recording, 
			Viewing,
			Radio
		};
		const int WS_CHILD				= 0x40000000;	
		const int WS_CLIPCHILDREN	= 0x02000000;
		const int WS_CLIPSIBLINGS	= 0x04000000;

		private static Guid CLSID_StreamBufferSink					= new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
		private static Guid CLSID_Mpeg2VideoStreamAnalyzer	= new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
		private static Guid CLSID_StreamBufferConfig				= new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);

		int						m_lastPMTVersion;
		int                     m_cardID								= -1;
		int                     m_iCurrentChannel				= 28;
		int											m_rotCookie							= 0;			// Cookie into the Running Object Table
		int                     m_iPrevChannel					= -1;
		bool                    m_bIsUsingMPEG					= false;
		State                   m_graphState						= State.None;
		DateTime								m_StartTime							= DateTime.Now;


		IPin												m_pinAC3Out				= null;
		IPin												m_pinMPG1Out				= null;
		IPin												m_DemuxVideoPin				= null;
		IPin												m_DemuxAudioPin				= null;
		protected IPin					m_demuxSectionsPin=null;
		IStreamBufferSink						m_IStreamBufferSink		= null;
		IStreamBufferConfigure			m_IStreamBufferConfig	= null;
		IBaseFilter							m_TIF										= null;			// Transport Information Filter
		IBaseFilter             m_NetworkProvider				= null;			// BDA Network Provider
		IBaseFilter             m_TunerDevice						= null;			// BDA Digital Tuner Device
		IBaseFilter							m_CaptureDevice					= null;			// BDA Digital Capture Device
		IBaseFilter							m_MPEG2Demultiplexer		= null;			// Mpeg2 Demultiplexer that connects to Preview pin on Smart Tee (must connect before capture)
		IStreamAnalyzer					m_analyzerInterface			= null;
		IEPGGrabber							m_epgGrabberInterface		= null;
		IMHWGrabber							m_mhwGrabberInterface		= null;
		IATSCGrabber						m_atscGrabberInterface	= null;
		IBaseFilter							m_dvbAnalyzer=null;
		IBaseFilter						  m_tsWriter=null;
		IMPTSWriter							m_tsWriterInterface=null;
		IMPTSRecord						  m_tsRecordInterface=null;
		IBaseFilter							m_smartTee= null;			// Transport Information Filter

		VideoAnalyzer						m_mpeg2Analyzer					= null;
		IGraphBuilder           m_graphBuilder					= null;
		ICaptureGraphBuilder2   m_captureGraphBuilder		= null;
		IVideoWindow            m_videoWindow						= null;
		IBasicVideo2            m_basicVideo						= null;
		IMediaControl						m_mediaControl					= null;
		IBaseFilter							m_sampleGrabber=null;
		ISampleGrabber					m_sampleInterface=null;

		StreamBufferSink				m_StreamBufferSink=null;
		StreamBufferConfig					m_StreamBufferConfig	= null;
		VMR9Util									  Vmr9								  = null; 
		VMR7Util									  Vmr7								  = null; 
		//GuideDataEvent							m_Event               = null;
		//GCHandle										myHandle;
		//int                         adviseCookie;
		ArrayList               m_TunerStatistics       = new ArrayList();
		NetworkType							m_NetworkType=NetworkType.Unknown;
		TVCaptureDevice					m_Card;
		bool												graphRunning=false;
		DVBChannel									currentTuningObject=null;
		TSHelperTools								transportHelper=new TSHelperTools();
		bool												refreshPmtTable=false;
		protected bool							m_pluginsEnabled=false;
		DateTime										updateTimer=DateTime.Now;
		DateTime										timeRetune=DateTime.Now;
		DVBDemuxer									m_streamDemuxer = new DVBDemuxer();
		EpgGrabber									m_epgGrabber = new EpgGrabber();

		
		int m_iVideoWidth=1;
		int m_iVideoHeight=1;
		int m_aspectX=1;
		int m_aspectY=1;
		bool isUsingAC3=false;
		bool m_bOverlayVisible=true;

		DirectShowHelperLib.StreamBufferRecorderClass m_recorder=null;
#if DUMP
		System.IO.FileStream fileout;
#endif
		#endregion

		#region constructor
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
				System.IO.Directory.CreateDirectory("database");
			}
			catch(Exception){}

			try
			{				
				System.IO.Directory.CreateDirectory(@"database\pmt");
			}
			catch(Exception){}
			//create registry keys needed by the streambuffer engine for timeshifting/recording
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");
			}
			catch(Exception){}
			
		}

		#endregion

		#region create/view/timeshift/record
		#region createGraph/DeleteGraph()
		/// <summary>
		/// Creates a new DirectShow graph for the TV capturecard.
		/// This graph can be a DVB-T, DVB-C or DVB-S graph
		/// </summary>
		/// <returns>bool indicating if graph is created or not</returns>
		public bool CreateGraph(int Quality)
		{
			try
			{
				//check if we didnt already create a graph
				if (m_graphState != State.None) 
					return false;
				currentTuningObject=null;
				isUsingAC3=false;

#if DUMP
				fileout = new System.IO.FileStream("audiodump.dat",System.IO.FileMode.OpenOrCreate,System.IO.FileAccess.Write,System.IO.FileShare.None);
#endif
				graphRunning=false;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph(). ");

				using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					m_pluginsEnabled=xmlreader.GetValueAsBool("dvb_ts_cards","enablePlugins",false);
				}

				//no card defined? then we cannot build a graph
				if (m_Card==null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:card is not defined");
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
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:card does not contain filters?");
					return false;
				}

				//check if definition contains <connections> for the tv filter graph
				if (m_Card.TvConnectionDefinitions==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:card does not contain connections for tv?");
					return false;
				}

				//create new instance of VMR9 helper utility
				Vmr9 =new VMR9Util("mytv");
				Vmr7 =new VMR7Util();

				// Make a new filter graph
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:create new filter graph (IGraphBuilder)");
				m_graphBuilder = (IGraphBuilder) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true));

			
				// Get the Capture Graph Builder
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Get the Capture Graph Builder (ICaptureGraphBuilder2)");
				Guid clsid = Clsid.CaptureGraphBuilder2;
				Guid riid = typeof(ICaptureGraphBuilder2).GUID;
				m_captureGraphBuilder = (ICaptureGraphBuilder2) DsBugWO.CreateDsInstance(ref clsid, ref riid);

				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Link the CaptureGraphBuilder to the filter graph (SetFiltergraph)");
				int hr = m_captureGraphBuilder.SetFiltergraph(m_graphBuilder);
				if (hr < 0) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:link FAILED:0x{0:X}",hr);
					return false;
				}
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Add graph to ROT table");
				DsROT.AddGraphToRot(m_graphBuilder, out m_rotCookie);


				//dont use samplegrabber in configuration.exe
				m_sampleGrabber=null;
				m_sampleInterface=null;
				if (GUIGraphicsContext.DX9Device!=null)
				{
					m_sampleGrabber=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.SampleGrabber, true ) );
					m_sampleInterface=(ISampleGrabber) m_sampleGrabber;
					m_graphBuilder.AddFilter(m_sampleGrabber,"Sample Grabber");
				}
				// Loop through configured filters for this card, bind them and add them to the graph
				// Note that while adding filters to a graph, some connections may already be created...
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured filters...");
				foreach (string catName in m_Card.TvFilterDefinitions.Keys)
				{
					FilterDefinition dsFilter = m_Card.TvFilterDefinitions[catName] as FilterDefinition;
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  Adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					dsFilter.DSFilter         = Marshal.BindToMoniker(dsFilter.MonikerDisplayName) as IBaseFilter;
					hr = m_graphBuilder.AddFilter(dsFilter.DSFilter, dsFilter.FriendlyName);
					if (hr == 0)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  Added filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
					}
					else
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:  Error! Failed adding filter <{0}> with moniker <{1}>", dsFilter.FriendlyName, dsFilter.MonikerDisplayName);
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:  Error! Result code = {0}", hr);
					}

					// Support the "legacy" member variables. This could be done different using properties
					// through which the filters are accessable. More implementation independent...
					if (dsFilter.Category == "networkprovider") 
					{
						m_NetworkProvider       = dsFilter.DSFilter;
						// Initialise Tuning Space (using the setupTuningSpace function)
						if(!setupTuningSpace()) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED couldnt create tuning space");
							return false;
						}
					}
					if (dsFilter.Category == "tunerdevice") m_TunerDevice	 							= dsFilter.DSFilter;
					if (dsFilter.Category == "capture")			m_CaptureDevice							= dsFilter.DSFilter;
				}//foreach (string catName in m_Card.TvFilterDefinitions.Keys)

				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured filters...DONE");

				//no network provider specified? then we cannot build the graph
				if(m_NetworkProvider == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED networkprovider filter not found");
					return false;
				}

				//no capture device specified? then we cannot build the graph
				if(m_CaptureDevice == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED capture filter not found");
				}



				FilterDefinition sourceFilter;
				FilterDefinition sinkFilter;
				IPin sourcePin=null;
				IPin sinkPin=null;

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

				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured pin connections...");
				for (int i = 0; i < m_Card.TvConnectionDefinitions.Count; i++)
				{
					//get the source filter for the connection
					sourceFilter = m_Card.TvFilterDefinitions[((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourceCategory] as FilterDefinition;
					if (sourceFilter==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: Cannot find source filter for connection:{0}",i);
						continue;
					}

					//get the destination/sink filter for the connection
					sinkFilter   = m_Card.TvFilterDefinitions[((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkCategory] as FilterDefinition;
					if (sinkFilter==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: Cannot find sink filter for connection:{0}",i);
						continue;
					}

					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  Connecting <{0}>:{1} with <{2}>:{3}", 
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
								Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:   Unable to find sourcePin: <{0}>", strPinName);
							//else
							//	Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sourcePin: <{0}> <{1}>", strPinName, sourcePin.ToString());
						}
					}
					//else
					//	Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sourcePin: <{0}> ", ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SourcePinName);

					//find the pin of the sink filter
					sinkPin      = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);
					if (sinkPin == null)
					{
						String strPinName = ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName;
						if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
						{
							sinkPin = DirectShowUtil.FindPinNr(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
							if (sinkPin==null)
								Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:   Unable to find sinkPin: <{0}>", strPinName);
							//else
							//	Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
						}
					}
					//else
					//	Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sinkPin: <{0}> ", ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);

					//if we have both pins
					if (sourcePin!=null && sinkPin!=null)
					{
						// then connect them
						IPin conPin;
						hr      = sourcePin.ConnectedTo(out conPin);
						if (hr != 0)
							hr = m_graphBuilder.Connect(sourcePin, sinkPin);
						//if (hr == 0)
						//	Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Pins connected...");

						// Give warning and release pin...
						if (conPin != null)
						{
							Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   (Pin was already connected...)");
							Marshal.ReleaseComObject(conPin as Object);
							conPin = null;
							hr     = 0;
						}
					}

					
					
					//log if connection failed
					//if (sourceFilter.Category =="tunerdevice" && sinkFilter.Category=="capture")
					//	hr=1;
					if (hr != 0)
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   unable to connect pins:0x{0:X}",hr);
						if (sourceFilter.Category =="tunerdevice" && sinkFilter.Category=="capture")
						{
							Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   try other instances");
							if (sinkPin!=null)
								Marshal.ReleaseComObject(sinkPin);
							sinkPin=null;
							if (sinkFilter.DSFilter!=null)
							{	
								m_graphBuilder.RemoveFilter(sinkFilter.DSFilter);
								Marshal.ReleaseComObject(sinkFilter.DSFilter);
							}
							sinkFilter.DSFilter=null;
							m_CaptureDevice=null;

							foreach (string key in AvailableFilters.Filters.Keys)
							{
								Filter    filter;
								ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
								filter    = (Filter)al[0];
								if (filter.Name.Equals(sinkFilter.FriendlyName))
								{
									Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   found {0} instances",al.Count);
									for (int filterInstance=0; filterInstance < al.Count;++filterInstance)
									{
										filter    = (Filter)al[filterInstance];
										Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   try:{0}",filter.MonikerString);
										sinkFilter.MonikerDisplayName=filter.MonikerString;
										sinkFilter.DSFilter  = Marshal.BindToMoniker(sinkFilter.MonikerDisplayName) as IBaseFilter;
										hr = m_graphBuilder.AddFilter(sinkFilter.DSFilter, sinkFilter.FriendlyName);
										//find the pin of the sink filter
										sinkPin      = DirectShowUtil.FindPin(sinkFilter.DSFilter, PinDirection.Input, ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName);
										if (sinkPin == null)
										{
											String strPinName = ((ConnectionDefinition)m_Card.TvConnectionDefinitions[i]).SinkPinName;
											if ((strPinName.Length == 1) && (Char.IsDigit(strPinName, 0)))
											{
												sinkPin = DirectShowUtil.FindPinNr(sinkFilter.DSFilter, PinDirection.Input, Convert.ToInt32(strPinName));
												if (sinkPin==null)
													Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:   Unable to find sinkPin: <{0}>", strPinName);
												else
													Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Found sinkPin: <{0}> <{1}>", strPinName, sinkPin.ToString());
											}
										}
										if (sinkPin!=null)
										{
											hr = m_graphBuilder.Connect(sourcePin, sinkPin);
											if (hr == 0)
											{
												Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   Pins connected...");
												m_CaptureDevice	= sinkFilter.DSFilter;
												break;
											}
											else
											{
												Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:   cannot connect pins:0x{0:X}",hr);
												if (sinkPin!=null)
													Marshal.ReleaseComObject(sinkPin);
												sinkPin=null;
												if (sinkFilter.DSFilter!=null)
												{
													m_graphBuilder.RemoveFilter(sinkFilter.DSFilter);
													Marshal.ReleaseComObject(sinkFilter.DSFilter);
													sinkFilter.DSFilter=null;
												}
											}
										}
									}//for (int filterInstance=0; filterInstance < al.Count;++filterInstance)
								}//if (filter.Name.Equals(sinkFilter.FriendlyName))
							}//foreach (string key in AvailableFilters.Filters.Keys)
						}//if (sourceFilter.Category =="tunerdevice" && sinkFilter.Category=="capture")
					}//if (hr != 0)
				}//for (int i = 0; i < m_Card.TvConnectionDefinitions.Count; i++)
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Adding configured pin connections...DONE");

				
				if (sinkPin!=null)
					Marshal.ReleaseComObject(sinkPin);
				sinkPin=null;
				
				if (sourcePin!=null)
					Marshal.ReleaseComObject(sourcePin);
				sourcePin=null;

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
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED interface filter not found");
					return false;
				}

#if USEMTSWRITER
				m_smartTee = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SmartTee, true));
				if(m_smartTee== null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to create SmartTee filter");
					return false;
				}
				m_graphBuilder.AddFilter(m_smartTee, "Smart Tee Filter");
				if (!ConnectFilters(ref lastFilter.DSFilter,ref m_smartTee))
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect capture->smart tee");
					return false;
				}
				if (!ConnectFilters(ref m_smartTee,ref m_sampleGrabber))
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect smart tee->grabber");
					return false;
				}
#else
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() connect interface pin->sample grabber");
				if (GUIGraphicsContext.DX9Device!=null && m_sampleInterface!=null)
				{
					if (!ConnectFilters(ref lastFilter.DSFilter,ref m_sampleGrabber))
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
						return false;
					}
				}
#endif
				//=========================================================================================================
				// add the MPEG-2 Demultiplexer 
				//=========================================================================================================
				// Use CLSID_Mpeg2Demultiplexer to create the filter
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() create MPEG2-Demultiplexer");
				m_MPEG2Demultiplexer = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.Mpeg2Demultiplexer, true));
				if(m_MPEG2Demultiplexer== null) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to create Mpeg2 Demultiplexer");
					return false;
				}


				// Add the Demux to the graph
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() add mpeg2 demuxer to graph");
				m_graphBuilder.AddFilter(m_MPEG2Demultiplexer, "MPEG-2 Demultiplexer");

				//=========================================================================================================
				// add the TIF 
				//=========================================================================================================

				object tmpObject;
				if(!findNamedFilter(FilterCategories.KSCATEGORY_BDA_TRANSPORT_INFORMATION, "BDA MPEG2 Transport Information Filter", out tmpObject)) 
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED Failed to find BDA MPEG2 Transport Information Filter");
					return false;
				}
				m_TIF = (IBaseFilter) tmpObject;
				tmpObject = null;
				if(m_TIF == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:CreateGraph() FAILED BDA MPEG2 Transport Information Filter is null");
					return false;
				}
				m_graphBuilder.AddFilter(m_TIF, "BDA MPEG2 Transport Information Filter");



				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() connect grabber->demuxer");
				if (GUIGraphicsContext.DX9Device!=null &&m_sampleInterface!=null)
				{
					if(!ConnectFilters(ref m_sampleGrabber, ref m_MPEG2Demultiplexer)) 
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
						return false;
					}
				}
				else
				{
					if(!ConnectFilters(ref lastFilter.DSFilter, ref m_MPEG2Demultiplexer)) 
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect samplegrabber filter->mpeg2 demultiplexer");
						return false;
					}
				}				


				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() connect demuxer->tif");
				if(!ConnectFilters(ref m_MPEG2Demultiplexer,ref m_TIF))
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect mpeg2 demultiplexer->TIF");
					return false;
				}
				IMpeg2Demultiplexer   demuxer=m_MPEG2Demultiplexer as IMpeg2Demultiplexer;



				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() add stream analyzer");
				m_dvbAnalyzer=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.MPStreamAnalyzer, true ) );
				m_analyzerInterface=(IStreamAnalyzer)m_dvbAnalyzer;
				m_epgGrabberInterface=m_dvbAnalyzer as IEPGGrabber;
				m_mhwGrabberInterface=m_dvbAnalyzer as IMHWGrabber;
				m_atscGrabberInterface=m_dvbAnalyzer as IATSCGrabber;
				hr=m_graphBuilder.AddFilter(m_dvbAnalyzer,"Stream-Analyzer");
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED to add SectionsFilter 0x{0:X}",hr);
					return false;
				}


				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() find audio/video pins");
				bool connected=false;
				IPin pinAnalyzerIn=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,0);
				IEnumPins pinEnum;
				m_MPEG2Demultiplexer.EnumPins(out pinEnum);
				pinEnum.Reset();
				IPin[] pin= new IPin[1];
				int fetched=0;
				while (pinEnum.Next(1,pin,out fetched)==0)
				{
					if (fetched==1)
					{
						IEnumMediaTypes enumMedia;
						pin[0].EnumMediaTypes(out enumMedia);
						enumMedia.Reset();
						AMMediaTypeClass pinMediaType;
						uint fetchedm=0;
						while (enumMedia.Next(1,out pinMediaType,out fetchedm)==0)
						{
							if (fetchedm==1)
							{
								if (pinMediaType.majorType==MediaType.Audio)
								{
									Log.Write("DVBGraphBDA: found audio pin");
									m_DemuxAudioPin=pin[0];
									break;
								}
								if (pinMediaType.majorType==MediaType.Video)
								{
									Log.Write("DVBGraphBDA: found video pin");
									m_DemuxVideoPin=pin[0];
									break;
								}
								if (pinMediaType.majorType==MEDIATYPE_MPEG2_SECTIONS && !connected)
								{
									IPin pinConnectedTo=null;
									pin[0].ConnectedTo(out pinConnectedTo);
									if (pinConnectedTo==null)
									{
										Log.Write("DVBGraphBDA:connect mpeg2 demux->stream analyzer");
										hr=m_graphBuilder.Connect(pin[0],pinAnalyzerIn);
										if (hr==0)
										{
											connected=true;
											Log.Write("DVBGraphBDA:connected mpeg2 demux->stream analyzer");
										}
										else
										{
											Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect mpeg2 demux->stream analyzer");
										}
									}
									if (pinConnectedTo!=null)
									{
										Marshal.ReleaseComObject(pinConnectedTo);
										pinConnectedTo=null;
									}
								}
							}
						}
						Marshal.ReleaseComObject(enumMedia); enumMedia=null;
					}
				}
				Marshal.ReleaseComObject(pinEnum); pinEnum=null;
				if (pinAnalyzerIn!=null) Marshal.ReleaseComObject(pinAnalyzerIn);pinAnalyzerIn=null;
				//get the video/audio output pins of the mpeg2 demultiplexer
				if (m_DemuxVideoPin==null)
				{
					//video pin not found
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to get pin '{0}' (video out) from MPEG-2 Demultiplexer",m_DemuxVideoPin);
					return false;
				}
				if (m_DemuxAudioPin==null)
				{
					//audio pin not found
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to get pin '{0}' (audio out)  from MPEG-2 Demultiplexer",m_DemuxAudioPin);
					return false;
				}

				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() create ac3/mpg1 pins");
				if (demuxer!=null)
				{
					AMMediaType mpegVideoOut = new AMMediaType();
					mpegVideoOut.majorType = MediaType.Video;
					mpegVideoOut.subType = MediaSubType.MPEG2_Video;

					Size FrameSize=new Size(100,100);
					mpegVideoOut.unkPtr = IntPtr.Zero;
					mpegVideoOut.sampleSize = 0;
					mpegVideoOut.temporalCompression = false;
					mpegVideoOut.fixedSizeSamples = true;

					//Mpeg2ProgramVideo=new byte[Mpeg2ProgramVideo.GetLength(0)];
					mpegVideoOut.formatType = FormatType.Mpeg2Video;
					mpegVideoOut.formatSize = Mpeg2ProgramVideo.GetLength(0) ;
					mpegVideoOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem( mpegVideoOut.formatSize);
					System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo,0,mpegVideoOut.formatPtr,mpegVideoOut.formatSize) ;

					AMMediaType mpegAudioOut = new AMMediaType();
					mpegAudioOut.majorType = MediaType.Audio;
					mpegAudioOut.subType = MediaSubType.MPEG2_Audio;
					mpegAudioOut.sampleSize = 0;
					mpegAudioOut.temporalCompression = false;
					mpegAudioOut.fixedSizeSamples = true;
					mpegAudioOut.unkPtr = IntPtr.Zero;
					mpegAudioOut.formatType = FormatType.WaveEx;
					mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
					mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
					System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);
					hr = demuxer.CreateOutputPin(ref mpegAudioOut, "audio", out m_DemuxAudioPin);
					if (hr != 0)
					{
						Log.WriteFile(Log.LogType.Capture,true, "DVBGraphBDA: FAILED to create audio output pin on demuxer");
						return false;
					}

					hr=demuxer.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out m_DemuxVideoPin);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED to create video output pin on demuxer");
						return  false;
					}

					Log.WriteFile(Log.LogType.Capture,false,"mpeg2: create ac3 pin");
					AMMediaType mediaAC3 = new AMMediaType();
					mediaAC3.majorType = MediaType.Audio;
					mediaAC3.subType = MediaSubType.DolbyAC3;
					mediaAC3.sampleSize = 0;
					mediaAC3.temporalCompression = false;
					mediaAC3.fixedSizeSamples = false;
					mediaAC3.unkPtr = IntPtr.Zero;
					mediaAC3.formatType = FormatType.WaveEx;
					mediaAC3.formatSize = MPEG1AudioFormat.GetLength(0);
					mediaAC3.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaAC3.formatSize);
					System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat,0,mediaAC3.formatPtr,mediaAC3.formatSize) ;

					hr=demuxer.CreateOutputPin(ref mediaAC3/*vidOut*/, "AC3", out m_pinAC3Out);
					if (hr!=0 || m_pinAC3Out==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to create AC3 pin:0x{0:X}",hr);
					}

					Log.WriteFile(Log.LogType.Capture,false,"DVBGraphBDA: create mpg1 audio pin");
					AMMediaType mediaMPG1 = new AMMediaType();
					mediaMPG1.majorType = MediaType.Audio;
					mediaMPG1.subType = MediaSubType.MPEG1AudioPayload;
					mediaMPG1.sampleSize = 0;
					mediaMPG1.temporalCompression = false;
					mediaMPG1.fixedSizeSamples = false;
					mediaMPG1.unkPtr = IntPtr.Zero;
					mediaMPG1.formatType = FormatType.WaveEx;
					mediaMPG1.formatSize = MPEG1AudioFormat.GetLength(0);
					mediaMPG1.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaMPG1.formatSize);
					System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat,0,mediaMPG1.formatPtr,mediaMPG1.formatSize) ;

					hr=demuxer.CreateOutputPin(ref mediaMPG1/*vidOut*/, "audioMpg1", out m_pinMPG1Out);
					if (hr!=0 || m_pinMPG1Out==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to create MPG1 pin:0x{0:X}",hr);
					}

					//create EPG pins
					Log.Write("DVBGraphBDA:Create EPG pin");
					AMMediaType mtEPG = new AMMediaType();
					mtEPG.majorType=MEDIATYPE_MPEG2_SECTIONS;
					mtEPG.subType=MediaSubType.None;
					mtEPG.formatType=FormatType.None;

					IPin pinEPGout, pinMHW1Out,pinMHW2Out;
					hr=demuxer.CreateOutputPin(ref mtEPG, "EPG", out pinEPGout);
					if (hr!=0 || pinEPGout==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to create EPG pin:0x{0:X}",hr);
						return false;
					}
					hr=demuxer.CreateOutputPin(ref mtEPG, "MHW1", out pinMHW1Out);
					if (hr!=0 || pinMHW1Out==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to create MHW1 pin:0x{0:X}",hr);
						return false;
					}
					hr=demuxer.CreateOutputPin(ref mtEPG, "MHW2", out pinMHW2Out);
					if (hr!=0 || pinMHW2Out==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to create MHW2 pin:0x{0:X}",hr);
						return false;
					}

					Log.Write("DVBGraphBDA:Get EPGs pin of analyzer");
					IPin pinMHW1In=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,1);
					if (pinMHW1In==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to get MHW1 pin on MSPA");
						return false;
					}
					IPin pinMHW2In=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,2);
					if (pinMHW2In==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to get MHW2 pin on MSPA");
						return false;
					}
					IPin pinEPGIn=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,3);
					if (pinEPGIn==null)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to get EPG pin on MSPA");
						return false;
					}

					Log.Write("DVBGraphBDA:Connect epg pins");
					hr=m_graphBuilder.Connect(pinEPGout,pinEPGIn);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect EPG pin:0x{0:X}",hr);
						return false;
					}
					hr=m_graphBuilder.Connect(pinMHW1Out,pinMHW1In);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect MHW1 pin:0x{0:X}",hr);
						return false;
					}
					hr=m_graphBuilder.Connect(pinMHW2Out,pinMHW2In);
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect MHW2 pin:0x{0:X}",hr);
						return false;
					}
					Log.Write("DVBGraphBDA:Demuxer is setup");

				}
				else
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:mapped IMPEG2Demultiplexer not found");

				//=========================================================================================================
				// Create the streambuffer engine and mpeg2 video analyzer components since we need them for
				// recording and timeshifting
				//=========================================================================================================
				m_StreamBufferSink  = new StreamBufferSink();
				m_mpeg2Analyzer     = new VideoAnalyzer();
				m_IStreamBufferSink = (IStreamBufferSink) m_StreamBufferSink;
				m_graphState=State.Created;

				GetTunerSignalStatistics();

				// teletext settings
				GUIWindow win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
				if(win!=null)
					win.SetObject(m_streamDemuxer.Teletext);
			
				win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
				if(win!=null)
					win.SetObject(m_streamDemuxer.Teletext);


				//m_streamDemuxer.OnAudioFormatChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnAudioChanged(m_streamDemuxer_OnAudioFormatChanged);
				//m_streamDemuxer.OnPMTIsChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnPMTChanged(m_streamDemuxer_OnPMTIsChanged);
				m_streamDemuxer.SetCardType((int)DVBEPG.EPGCard.BDACards, Network());
				//m_streamDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);

				if(m_sampleInterface!=null)
				{
					AMMediaType mt=new AMMediaType();
					mt.majorType=DShowNET.MediaType.Stream;
					mt.subType=DShowNET.MediaSubType.MPEG2Transport;	
					m_sampleInterface.SetCallback(m_streamDemuxer,1);
					m_sampleInterface.SetMediaType(ref mt);
					m_sampleInterface.SetBufferSamples(false);
				}

				if (Network()==NetworkType.ATSC)
					m_analyzerInterface.UseATSC(1);
				else
					m_analyzerInterface.UseATSC(0);
					
				m_epgGrabber.EPGInterface=m_epgGrabberInterface;
				m_epgGrabber.MHWInterface=m_mhwGrabberInterface;
				m_epgGrabber.ATSCInterface=m_atscGrabberInterface;
				m_epgGrabber.AnalyzerInterface=m_analyzerInterface;
				m_epgGrabber.Network=Network();
			
				return true;
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: Unable to create graph {0} {1}",ex.Message,ex.StackTrace);
				return false;
			}
		}//public bool CreateGraph()

		/// <summary>
		/// Deletes the current DirectShow graph created with CreateGraph()
		/// Frees any (unmanaged) resources
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public void DeleteGraph()
		{
			try
			{
				if (m_graphState < State.Created) 
					return;
				int hr;
				currentTuningObject=null;
				isUsingAC3=false;
				m_iPrevChannel = -1;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:DeleteGraph()");
				StopRecording();
				StopTimeShifting();
				StopViewing();
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: free tuner interfaces");
				
				// to clear buffers for epg and teletext
				if (m_streamDemuxer != null)
				{
					m_streamDemuxer.SetChannelData(0, 0, 0, 0, "",0,0);
				}

				if (m_TunerStatistics!=null)
				{
					for (int i = 0; i < m_TunerStatistics.Count; i++) 
					{
						if (m_TunerStatistics[i] != null)
						{
							IBDA_SignalStatistics stat=(IBDA_SignalStatistics )m_TunerStatistics[i];
							while ((hr=Marshal.ReleaseComObject(stat))>0);
							if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(tunerstat):{0}",hr);
						}
					}
					m_TunerStatistics.Clear();
				}
				Log.Write("DVBGraphBDA:stop graph");
				if (m_mediaControl!=null) m_mediaControl.Stop();
				m_mediaControl = null;
				Log.Write("DVBGraphBDA:graph stopped");

				if (Vmr9!=null)
				{
					Log.Write("DVBGraphBDA:remove vmr9");
					Vmr9.RemoveVMR9();
					Vmr9.Release();
					Vmr9=null;
				}

				if (Vmr7!=null)
				{
					Log.Write("DVBGraphBDA:remove vmr7");
					Vmr7.RemoveVMR7();
					Vmr7=null;
				}
				
				if (m_recorder!=null) 
				{
					Log.Write("DVBGraphBDA:remove recorder");
					try
					{
						m_recorder.Stop();
					}
					catch(Exception){}
					m_recorder=null;
				}
				
				graphRunning=false;
				m_basicVideo = null;
				m_analyzerInterface=null;
				m_epgGrabberInterface=null;
				m_mhwGrabberInterface=null;
				m_tsWriterInterface=null;
				m_tsRecordInterface=null;
				Log.Write("free pins");

				if (m_demuxSectionsPin!=null)
					Marshal.ReleaseComObject(m_demuxSectionsPin);
				m_demuxSectionsPin				= null;

				if ( m_pinAC3Out!=null)
					Marshal.ReleaseComObject(m_pinAC3Out);
				m_pinAC3Out				= null;
				
				if ( m_pinMPG1Out!=null)
					Marshal.ReleaseComObject(m_pinMPG1Out);
				m_pinMPG1Out				= null;

				if ( m_DemuxVideoPin!=null)
					Marshal.ReleaseComObject(m_DemuxVideoPin);
				m_DemuxVideoPin				= null;

				if ( m_DemuxAudioPin!=null)
					Marshal.ReleaseComObject(m_DemuxAudioPin);
				m_DemuxAudioPin				= null;


				if (m_TIF != null)
				{
					while ((hr=Marshal.ReleaseComObject(m_TIF))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_TIF):{0}",hr);
					m_TIF = null;
				}

				if(m_dvbAnalyzer!=null)
				{
					Log.Write("free dvbanalyzer");
					hr=Marshal.ReleaseComObject(m_dvbAnalyzer);
					if (hr!=0) Log.Write("ReleaseComObject(m_dvbAnalyzer):{0}",hr);
					m_dvbAnalyzer=null;
				}

				if (m_tsWriter!=null)
				{
					Log.Write("free MPTSWriter");
					hr=Marshal.ReleaseComObject(m_tsWriter);
					if (hr!=0) Log.Write("ReleaseComObject(m_tsWriter):{0}",hr);
					m_tsWriter=null;
				}
				if (m_smartTee != null)
				{
					while ((hr=Marshal.ReleaseComObject(m_smartTee))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_smartTee):{0}",hr);
					m_smartTee = null;
				}
							
				if (m_videoWindow != null)
				{
					Log.Write("DVBGraphBDA:hide window");
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: hide video window");
					m_videoWindow.put_Visible(DsHlp.OAFALSE);
					//m_videoWindow.put_Owner(IntPtr.Zero);
					m_videoWindow = null;
				}

				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: free other interfaces");
				m_sampleInterface=null;
				if (m_sampleGrabber != null) 
				{
					Log.Write("DVBGraphBDA:free samplegrabber");
					while ((hr=Marshal.ReleaseComObject(m_sampleGrabber))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_sampleGrabber):{0}",hr);
					m_sampleGrabber=null;
				}
				

				m_IStreamBufferConfig=null;
				m_IStreamBufferSink=null;

				if (m_StreamBufferSink!=null) 
				{
					Log.Write("DVBGraphBDA:free streambuffersink");
					while ((hr=Marshal.ReleaseComObject(m_StreamBufferSink))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_StreamBufferSink):{0}",hr);
					m_StreamBufferSink=null;
				}


				if (m_StreamBufferConfig != null) 
				{
					Log.Write("DVBGraphBDA:free streambufferconfig");
					while ((hr=Marshal.ReleaseComObject(m_StreamBufferConfig))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_StreamBufferConfig):{0}",hr);
					m_StreamBufferConfig=null;
				}


				if (m_NetworkProvider != null)
				{	
					Log.Write("DVBGraphBDA:free networkprovider");
					while ((hr=Marshal.ReleaseComObject(m_NetworkProvider))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_NetworkProvider):{0}",hr);
					m_NetworkProvider = null;
				}

				if (m_TunerDevice != null)
				{
					Log.Write("DVBGraphBDA:free tunerdevice");
					while ((hr=Marshal.ReleaseComObject(m_TunerDevice))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_TunerDevice):{0}",hr);
					m_TunerDevice = null;
				}

				if (m_CaptureDevice != null)
				{
					Log.Write("DVBGraphBDA:free capturedevice");
					while ((hr=Marshal.ReleaseComObject(m_CaptureDevice))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_CaptureDevice):{0}",hr);
					m_CaptureDevice = null;
				}
				
				if (m_MPEG2Demultiplexer != null)
				{
					Log.Write("DVBGraphBDA:free demux");
					while ((hr=Marshal.ReleaseComObject(m_MPEG2Demultiplexer))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_MPEG2Demultiplexer):{0}",hr);
					m_MPEG2Demultiplexer = null;
				}

				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: remove filters");
				if (m_graphBuilder!=null)
					DsUtils.RemoveFilters(m_graphBuilder);

				
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: clean filters");
				foreach (string strfName in m_Card.TvFilterDefinitions.Keys)
				{
					FilterDefinition dsFilter = m_Card.TvFilterDefinitions[strfName] as FilterDefinition;
					dsFilter.DSFilter=null;
					((FilterDefinition)m_Card.TvFilterDefinitions[strfName]).DSFilter = null;
					dsFilter = null;
				}

				
				Log.Write("DVBGraphBDA:free remove graph");
				if (m_rotCookie != 0)
					DsROT.RemoveGraphFromRot(ref m_rotCookie);
				m_rotCookie = 0;

				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: remove graph");
				if (m_captureGraphBuilder != null)
				{
					Log.Write("DVBGraphBDA:free remove capturegraphbuilder");
					while ((hr=Marshal.ReleaseComObject(m_captureGraphBuilder))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_captureGraphBuilder):{0}",hr);
					m_captureGraphBuilder = null;
				}

				if (m_graphBuilder != null)
				{
					Log.Write("DVBGraphBDA:free graphbuilder");
					while ((hr=Marshal.ReleaseComObject(m_graphBuilder))>0); 
					if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(m_graphBuilder):{0}",hr);
					m_graphBuilder = null;
				}

#if DUMP
				if (fileout!=null)
				{
					fileout.Close();
					fileout=null;
				}
#endif
				GC.Collect();GC.Collect();GC.Collect();
				m_graphState = State.None;
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: delete graph done");
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: deletegraph() {0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}//public void DeleteGraph()
		
		#endregion
		#region Start/Stop Recording
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
		public bool StartRecording(Hashtable attributes,TVRecording recording,TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
		{
			if (m_graphState != State.TimeShifting) 
				return false;
			
			if (m_StreamBufferSink == null) 
				return false;

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

#if USEMTSWRITER
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRecording()");
			strFileName=System.IO.Path.ChangeExtension(strFileName,".ts");
			int hr=m_tsRecordInterface.SetRecordingFileName(strFileName);
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:unable to set filename:%x", hr);
				return false;
			}
			hr=m_tsRecordInterface.StartRecord(0);
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:unable to start recording:%x", hr);
				return false;
			}
#else
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRecording()");
			uint iRecordingType=0;
			if (bContentRecording) 
				iRecordingType = 0;
			else 
				iRecordingType = 1;										

			try
			{
				m_recorder = new DirectShowHelperLib.StreamBufferRecorderClass();
				m_recorder.Create(m_IStreamBufferSink as DirectShowHelperLib.IBaseFilter,strFileName,iRecordingType);

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
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
							timeProgStart.Hour, timeProgStart.Minute, timeProgStart.Second,
							ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);
																
						lStartTime = (long)ts.TotalSeconds;
					}
					else Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: record entire timeshift buffer");
	      
					TimeSpan tsMaxTimeBack = DateTime.Now - m_StartTime;
					if (lStartTime > tsMaxTimeBack.TotalSeconds)
					{
						lStartTime = (long)tsMaxTimeBack.TotalSeconds;
					}
	        

					lStartTime *= -10000000L;//in reference time 
				}//if (!bContentRecording)
				/*
						foreach (MetadataItem item in attributes.Values)
						{
							try
							{
								if (item.Type == MetadataItemType.String)
									m_recorder.SetAttributeString(item.Name,item.Value.ToString());
								if (item.Type == MetadataItemType.Dword)
									m_recorder.SetAttributeDWORD(item.Name,UInt32.Parse(item.Value.ToString()));
							}
							catch(Exception){}
						}*/
				m_recorder.Start((int)lStartTime);
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to start recording :{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
			finally
			{
			}
#endif
			m_graphState = State.Recording;
			return true;
		}//public bool StartRecording(int country,AnalogVideoStandard standard,int iChannelNr, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
	    
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
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:stop recording...");

			if (m_recorder!=null) 
			{
				try
				{
					m_recorder.Stop();
				}
				catch(Exception){}
				m_recorder=null;
			}
			if (m_tsRecordInterface!=null)
			{
				m_tsRecordInterface.StopRecord(0);
			}


			m_graphState = State.TimeShifting;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:stopped recording...");
		}//public void StopRecording()

		#endregion		
		#region Start/Stop Viewing
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
			if (m_graphState != State.Created) return false;
			
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartViewing()");

			m_bOverlayVisible=true;
			// add VMR9 renderer to graph
			if (Vmr9!=null)
			{
				if (Vmr9.UseVMR9inMYTV)
				{
					//GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
					//GUIWindowManager.SendMessage(msg);
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

			// add the preferred video/audio codecs
			AddPreferredCodecs(true,true);

			// render the video/audio pins of the mpeg2 demultiplexer so they get connected to the video/audio codecs
			if(m_graphBuilder.Render(m_DemuxVideoPin) != 0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render video out pin MPEG-2 Demultiplexer");
				return false;
			}
			
			isUsingAC3=TVDatabase.DoesChannelHaveAC3(channel, Network()==NetworkType.DVBC, Network()==NetworkType.DVBT, Network()==NetworkType.DVBS, Network()==NetworkType.ATSC);
			if (!isUsingAC3)
			{
				if(m_graphBuilder.Render(m_DemuxAudioPin) != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render audio out pin MPEG-2 Demultiplexer");
					return false;
				}

			}
			else
			{
				if(m_graphBuilder.Render(m_pinAC3Out) != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render AC3 pin MPEG-2 Demultiplexer");
					return false;
				}
			}

			//get the IMediaControl interface of the graph
			if(m_mediaControl == null)
				m_mediaControl = (IMediaControl) m_graphBuilder;

			int hr;
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
			//if are using the overlay video renderer
			if (useOverlay)
			{
				//then get the overlay video renderer interfaces
				m_videoWindow = m_graphBuilder as IVideoWindow;
				if (m_videoWindow == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED:Unable to get IVideoWindow");
					return false;
				}

				m_basicVideo = m_graphBuilder as IBasicVideo2;
				if (m_basicVideo == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED:Unable to get IBasicVideo2");
					return false;
				}

				// and set it up
				hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
				if (hr != 0) 
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:set Video window:0x{0:X}",hr);

				hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
				if (hr != 0) 
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:set Video window style:0x{0:X}",hr);

				//show overlay window
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:put_Visible:0x{0:X}",hr);
			}

			//start the graph
			//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: start graph");
			hr=m_mediaControl.Run();
			if (hr<0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
			}

			graphRunning=true;
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


			// tune to the correct channel
			if (channel.Number>=0)
				TuneChannel(channel);

				
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Viewing..");
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
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: StopViewing()");
			if (m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: stop vmr9");
			if (Vmr9!=null)
			{
				Vmr9.Enable(false);
			}

			
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: view stopped");
			graphRunning=false;
			m_graphState = State.Created;
			DeleteGraph();
			//GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
			//GUIWindowManager.SendMessage(msg);
			return true;
		}
				
		#endregion
		#region Start/Stop Timeshifting
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
			if(m_graphState!=State.Created)
				return false;
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
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartTimeShifting()");

			TuneChannel(channel);
			isUsingAC3=TVDatabase.DoesChannelHaveAC3(channel, Network()==NetworkType.DVBC, Network()==NetworkType.DVBT, Network()==NetworkType.DVBS, Network()==NetworkType.ATSC);
			if(CreateSinkSource(strFileName,isUsingAC3))
			{
				if(m_mediaControl == null) 
				{
					m_mediaControl = (IMediaControl) m_graphBuilder;
				}
				//now start the graph
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: start graph");
				int hr=m_mediaControl.Run();
				if (hr<0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
				}
				graphRunning=true;
				m_graphState = State.TimeShifting;
			}
			else 
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Unable to create sinksource()");
				return false;
			}

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:timeshifting started");			
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
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: StopTimeShifting()");
			if (m_mediaControl!=null)
				m_mediaControl.Stop();
			graphRunning=false;
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}//public bool StopTimeShifting()
		
		#endregion
		#endregion

		private bool m_streamDemuxer_AudioHasChanged(MediaPortal.TV.Recording.DVBDemuxer.AudioHeader audioFormat)
		{
			return false;
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
				m_bOverlayVisible=value;
				if (!m_bOverlayVisible)
				{
					if (m_videoWindow!=null)
						m_videoWindow.put_Visible( DsHlp.OAFALSE );

				}
				else
				{
					if (m_videoWindow!=null)
						m_videoWindow.put_Visible( DsHlp.OATRUE );

				}
			}
		}

		#region overrides
		/// <summary>
		/// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
		/// </summary>
		private void GUIGraphicsContext_OnVideoWindowChanged()
		{
			
			if (GUIGraphicsContext.Vmr9Active) return;
			if (m_graphState != State.Viewing) return;
			if (m_basicVideo==null) return;
			if (m_videoWindow==null) return;
			Log.Write("DVBGraphBDA:OnVideoWindowChanged()");
			int iVideoWidth, iVideoHeight;
			int aspectX, aspectY;
			m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);
			m_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			
			m_iVideoWidth=iVideoWidth;
			m_iVideoHeight=iVideoHeight;
			m_aspectX=aspectX;
			m_aspectY=aspectY;
			
				
			if (GUIGraphicsContext.BlankScreen)
			{
				Overlay=false;
			}
			else
			{
				Overlay=true;
			}
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
				m_geometry.GetWindow(aspectX,aspectY,out rSource, out rDest);
				rDest.X += (int)x;
				rDest.Y += (int)y;

				if (rSource.Left< 0 || rSource.Top<0 || rSource.Width<=0 || rSource.Height<=0) return;
				if (rDest.Left <0 || rDest.Top < 0 || rDest.Width<=0 || rDest.Height<=0) return;

				Log.Write("overlay: video WxH  : {0}x{1}",iVideoWidth,iVideoHeight);
				Log.Write("overlay: video AR   : {0}:{1}",aspectX, aspectY);
				Log.Write("overlay: screen WxH : {0}x{1}",nw,nh);
				Log.Write("overlay: AR type    : {0}",GUIGraphicsContext.ARType);
				Log.Write("overlay: PixelRatio : {0}",GUIGraphicsContext.PixelRatio);
				Log.Write("overlay: src        : ({0},{1})-({2},{3})",
					rSource.X,rSource.Y, rSource.X+rSource.Width,rSource.Y+rSource.Height);
				Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
					rDest.X,rDest.Y,rDest.X+rDest.Width,rDest.Y+rDest.Height);


				m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
				m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
				m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
			}
			else
			{
				if ( GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 || 
					GUIGraphicsContext.VideoWindow.Width <=0 || GUIGraphicsContext.VideoWindow.Height <=0) return;
				if (iVideoHeight<=0 || iVideoWidth<=0) return;

				m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
				m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
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
			//check if we switch from an channel with AC3 to a channel without AC3
			//or vice-versa. ifso, graphs should be rebuild
			if (m_graphState != State.Viewing && m_graphState!= State.TimeShifting && m_graphState!=State.Recording) return false; 
			bool useAC3=TVDatabase.DoesChannelHaveAC3(newChannel, Network()==NetworkType.DVBC, Network()==NetworkType.DVBT, Network()==NetworkType.DVBS, Network()==NetworkType.ATSC);
			if (useAC3 != isUsingAC3) return true; 
			return false;
		}

		#region Stream-Audio handling
		public int GetAudioLanguage()
		{
			return currentTuningObject.AudioPid;
		}
		public void SetAudioLanguage(int audioPid)
		{
			if(audioPid!=currentTuningObject.AudioPid)
			{
				int hr;
				if (audioPid==currentTuningObject.AC3Pid)
				{
					hr=SetupDemuxer(m_DemuxVideoPin,currentTuningObject.VideoPid,m_DemuxAudioPin,audioPid,m_pinAC3Out,audioPid);
				}
				else
				{
					hr=SetupDemuxer(m_DemuxVideoPin,currentTuningObject.VideoPid,m_DemuxAudioPin,audioPid,m_pinAC3Out,currentTuningObject.AC3Pid);
				}
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: SetupDemuxer FAILED: errorcode {0}",hr.ToString());
					return;
				}
				else
				{
					currentTuningObject.AudioPid=audioPid;
				}
			}
		}
		public ArrayList GetAudioLanguageList()
		{
			if (currentTuningObject==null) return new ArrayList();
			DVBSections.AudioLanguage al;
			ArrayList audioPidList = new ArrayList();
			if(currentTuningObject.AudioPid!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.AudioPid;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage;
				audioPidList.Add(al);
			}
			if(currentTuningObject.Audio1!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.Audio1;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage1;
				audioPidList.Add(al);
			}
			if(currentTuningObject.Audio2!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.Audio2;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage2;
				audioPidList.Add(al);
			}
			if(currentTuningObject.Audio3!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=currentTuningObject.Audio3;
				al.AudioLanguageCode=currentTuningObject.AudioLanguage3;
				audioPidList.Add(al);
			}
			return audioPidList;
		}
		#endregion

		public bool HasTeletext()
		{
			if (m_graphState!= State.TimeShifting && m_graphState!=State.Recording && m_graphState!=State.Viewing) return false;
			if (currentTuningObject==null) return false;
			if (currentTuningObject.TeletextPid>0) return true;
			return false;
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
		/// Add preferred mpeg video/audio codecs to the graph
		/// the user has can specify these codecs in the setup
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
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
			bool isTunerLocked		= false;
			bool isSignalPresent	= false;
			long signalQuality=0;
			

			for (int i = 0; i < m_TunerStatistics.Count; i++) 
			{
				IBDA_SignalStatistics stat=(IBDA_SignalStatistics)m_TunerStatistics[i];
				bool isLocked=false;
				bool isPresent=false;
				try
				{
					//is the tuner locked?
					stat.get_SignalLocked(ref isLocked);
					isTunerLocked |= isLocked;
				}
				catch (COMException)
				{
				}
				catch (Exception )
				{
				}
				try
				{
					//is a signal present?
					stat.get_SignalPresent(ref isPresent);
					isSignalPresent |= isPresent;
				}
				catch (COMException)
				{
				}
				catch (Exception )
				{
				}
				try
				{
					//is a signal quality ok?
					uint quality=0;
					stat.get_SignalQuality(ref quality); //1-100
					if (quality>0) signalQuality += quality;
				}
				catch (COMException)
				{
				}
				catch (Exception )
				{
				}
			}

			//some devices give different results about signal status
			//on some signalpresent is only true when tuned to a channel
			//on others  signalpresent is true when tuned to a transponder
			//so we just look if any variables returns true
			//	Log.WriteFile(Log.LogType.Capture,"  locked:{0} present:{1} quality:{2}",isTunerLocked ,isSignalPresent ,signalQuality); 

			if (Network()==NetworkType.ATSC)
			{
				if (isSignalPresent ) return true;
				return false;
			}
			if (isTunerLocked || isSignalPresent || (signalQuality>0) )
			{
				return true;
			}
			return false;
		}//public bool SignalPresent()
		public int  SignalQuality()
		{
			if (m_TunerStatistics==null) return -1;
			if (m_TunerStatistics.Count==0) return -1;
			int signalQuality=-1;
			uint quality=0;
			for (int i = 0; i < m_TunerStatistics.Count; i++) 
			{
				IBDA_SignalStatistics stat=(IBDA_SignalStatistics)m_TunerStatistics[i];

				try
				{
					quality = 0;
					stat.get_SignalQuality(ref quality); //1-100
					if (quality>0 && quality>signalQuality) signalQuality = (int)quality;
				}
				catch (COMException )
				{
				}
				catch (Exception )
				{
				}
			}
			return signalQuality;
		}
		public int  SignalStrength()
		{
			if (m_TunerStatistics==null) return -1;
			if (m_TunerStatistics.Count==0) return -1;
			int signalStrength=-1;
			uint strength = 0;

			for (int i = 0; i < m_TunerStatistics.Count; i++) 
			{
				IBDA_SignalStatistics stat=(IBDA_SignalStatistics)m_TunerStatistics[i];
				try
				{
					strength = 0;
					stat.get_SignalStrength(ref strength); //1-100
					if (strength>0 && strength>signalStrength) signalStrength = (int)strength;
				}
				catch (COMException )
				{
				}
				catch (Exception )
				{
				}
			}
			return signalStrength;
		}
		
		
		/// <summary>
		/// not used
		/// </summary>
		/// <returns>-1</returns>
		public long VideoFrequency()
		{
			if (currentTuningObject!=null) return currentTuningObject.Frequency*1000;
			return -1;
		}
		
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
			if (m_NetworkType==NetworkType.Unknown)
			{
				if (m_Card.LoadDefinitions())
				{
					foreach (string catName in m_Card.TvFilterDefinitions.Keys)
					{
						FilterDefinition dsFilter = m_Card.TvFilterDefinitions[catName] as FilterDefinition;
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBC Network Provider") 
						{
							m_NetworkType=NetworkType.DVBC;
							return m_NetworkType;
						}
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBT Network Provider") 
						{
							m_NetworkType=NetworkType.DVBT;
							return m_NetworkType;
						}
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft DVBS Network Provider") 
						{
							m_NetworkType=NetworkType.DVBS;
							return m_NetworkType;
						}
						if (dsFilter.MonikerDisplayName==@"@device:sw:{71985F4B-1CA1-11D3-9CC8-00C04F7971E0}\Microsoft ATSC Network Provider") 
						{
							m_NetworkType=NetworkType.ATSC;
							return m_NetworkType;
						}
					}
				}
			}
			return m_NetworkType;
		}
		
		/// <summary>
		/// Set the LNB settings for a DVB-S tune request
		/// </summary>
		/// <remarks>Only needed for DVB-S</remarks>
		/// <param name="tuneRequest">IDVBTuneRequest tunerequest for a DVB-S channel</param>
		public IBaseFilter Mpeg2DataFilter()
		{
			return null;
		}

		#endregion

		#region graph building helper functions
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
			if (video && strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strVideoCodec);
			if (audio && strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_graphBuilder,strAudioCodec);
			if (audio && strAudioRenderer.Length>0) DirectShowUtil.AddAudioRendererToGraph(m_graphBuilder,strAudioRenderer,false);
			if (video && bAddFFDshow) DirectShowUtil.AddFilterToGraph(m_graphBuilder,"ffdshow raw video filter");
		}//void AddPreferredCodecs()

		/// <summary>
		/// This method gets the IBDA_SignalStatistics interface from the tuner
		/// with this interface we can see if the tuner is locked to a signal
		/// and see what the signal strentgh is
		/// </summary>
		/// <returns>
		/// array of IBDA_SignalStatistics or null
		/// </returns>
		/// <remarks>
		/// Graph should be created
		/// </remarks>
		void GetTunerSignalStatistics()
		{
			//no tuner filter? then return;
			m_TunerStatistics=new ArrayList();
			if (m_TunerDevice==null) 
				return ;
			
			//get the IBDA_Topology from the tuner device
			//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: get IBDA_Topology");
			IBDA_Topology topology = m_TunerDevice as IBDA_Topology;
			if (topology==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: could not get IBDA_Topology from tuner");
				return ;
			}

			//get the NodeTypes from the topology
			//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: GetNodeTypes");
			int nodeTypeCount=0;
			int[] nodeTypes = new int[33];
			Guid[] guidInterfaces = new Guid[33];
			
			int hr=topology.GetNodeTypes(ref nodeTypeCount, 32, nodeTypes);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not get node types from tuner:0x{0:X}",hr);
				return ;
			}
			if (nodeTypeCount==0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not get any node types");
			}
			Guid GuidIBDA_SignalStatistic = new Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338");
			//for each node type
			//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: got {0} node types", nodeTypeCount);
			for (int i=0; i < nodeTypeCount;++i)
			{
				object objectNode;
				int numberOfInterfaces=32;
				hr=topology.GetNodeInterfaces(nodeTypes[i],ref numberOfInterfaces,32,guidInterfaces);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not GetNodeInterfaces for node:{0} 0x:{1:X}",i,hr);
				}

				hr=topology.GetControlNode(0,1, nodeTypes[i], out objectNode);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not GetControlNode for node:{0} 0x:{1:X}",i,hr);
					return ;
				}

				//and get the final IBDA_SignalStatistics
				for (int iface=0; iface < numberOfInterfaces; iface++)
				{
					if ( guidInterfaces[iface] == GuidIBDA_SignalStatistic)
					{
						Log.Write("DVBGraphBDA: got IBDA_SignalStatistics on node:{0} interface:{1}", i,iface);
						m_TunerStatistics.Add ( (IBDA_SignalStatistics) objectNode);
					}
				}

			}//for (int i=0; i < nodeTypeCount;++i)
			Marshal.ReleaseComObject(topology);
			return ;
		}//IBDA_SignalStatistics GetTunerSignalStatistics()

		IBDA_LNBInfo[] GetBDALNBInfoInterface()
		{
			//no tuner filter? then return;
			if (m_TunerDevice==null) 
				return null;
			
			//get the IBDA_Topology from the tuner device
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: get IBDA_Topology");
			IBDA_Topology topology = m_TunerDevice as IBDA_Topology;
			if (topology==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: could not get IBDA_Topology from tuner");
				return null;
			}

			//get the NodeTypes from the topology
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: GetNodeTypes");
			int nodeTypeCount=0;
			int[] nodeTypes = new int[33];
			Guid[] guidInterfaces = new Guid[33];
			
			int hr=topology.GetNodeTypes(ref nodeTypeCount, 32, nodeTypes);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not get node types from tuner");
				return null;
			}
			IBDA_LNBInfo[] signal = new IBDA_LNBInfo[nodeTypeCount];
			//for each node type
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: got {0} node types", nodeTypeCount);
			for (int i=0; i < nodeTypeCount;++i)
			{
				object objectNode;
				hr=topology.GetControlNode(0,1, nodeTypes[i], out objectNode);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED could not GetControlNode for node:{0}",hr);
					return null;
				}
				//and get the final IBDA_LNBInfo
				try
				{
					signal[i] = (IBDA_LNBInfo)objectNode ;
				}
				catch 
				{
					Log.WriteFile(Log.LogType.Capture,"No interface on node {0}", i); 
				}
			}//for (int i=0; i < nodeTypeCount;++i)
			Marshal.ReleaseComObject(topology);
			return signal;
		}//IBDA_LNBInfo[] GetBDALNBInfoInterface()

		void SetupMTSDemuxerPin()
		{
#if USEMTSWRITER
			if (m_tsWriterInterface== null || m_tsWriterInterface==null || currentTuningObject==null) return;
			//m_tsWriterInterface.ResetPids();
			if (currentTuningObject.AC3Pid>0)
				m_tsWriterInterface.SetAC3Pid((ushort)currentTuningObject.AC3Pid);
			else
				m_tsWriterInterface.SetAC3Pid(0);

			if (currentTuningObject.AudioPid>0)
				m_tsWriterInterface.SetAudioPid((ushort)currentTuningObject.AudioPid);
			else
			{
				if (currentTuningObject.Audio1>0)
					m_tsWriterInterface.SetAudioPid((ushort)currentTuningObject.Audio1);
				else
					m_tsWriterInterface.SetAudioPid(0);
			}
			
			if (currentTuningObject.Audio2>0)
				m_tsWriterInterface.SetAudioPid2((ushort)currentTuningObject.Audio2);
			else
				m_tsWriterInterface.SetAudioPid2(0);

			//m_tsWriterInterface.SetSubtitlePid((ushort)currentTuningObject.SubtitlePid);
			if (currentTuningObject.TeletextPid>0)
				m_tsWriterInterface.SetTeletextPid((ushort)currentTuningObject.TeletextPid);
			else
				m_tsWriterInterface.SetTeletextPid(0);

			if (currentTuningObject.VideoPid>0)
				m_tsWriterInterface.SetVideoPid((ushort)currentTuningObject.VideoPid);
			else
				m_tsWriterInterface.SetVideoPid(0);

			if (currentTuningObject.PCRPid>0)
				m_tsWriterInterface.SetPCRPid((ushort)currentTuningObject.PCRPid);
			else
				m_tsWriterInterface.SetPCRPid(0);

			m_tsWriterInterface.SetPMTPid((ushort)currentTuningObject.PMTPid);
#endif
		}

		private bool CreateSinkSource(string fileName, bool useAC3)
		{
			if(m_graphState!=State.Created)
				return false;

#if USEMTSWRITER
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateGraph() add MPTSWriter");
				m_tsWriter=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.MPTSWriter, true ) );
				m_tsWriterInterface = m_tsWriter as IMPTSWriter;
				m_tsRecordInterface = m_tsWriter as IMPTSRecord;

			int hr=m_graphBuilder.AddFilter((IBaseFilter)m_tsWriter,"MPTS Writer");
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot add MPTS Writer:{0:X}",hr);
				return false;
			}			

			IFileSinkFilter fileWriter=m_tsWriter as IFileSinkFilter;
			AMMediaType mt = new AMMediaType();
			mt.majorType=MediaType.Stream;
			mt.subType=MediaSubType.None;
			mt.formatType=FormatType.None;
			hr=fileWriter.SetFileName(fileName, ref mt);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot set filename on MPTS writer:0x{0:X}",hr);
				return false;
			}


			// connect demuxer->mpts writer
			
			if (!ConnectFilters(ref m_smartTee, ref m_tsWriter))
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot demuxer->MPTS writer:0x{0:X}",hr);
				return false;
			}
			SetupMTSDemuxerPin();


			return true;
#else
			int		hr				= 0;
			IPin	pinObj0		= null;
			IPin	pinObj1		= null;
			IPin	pinObj2		= null;
			IPin	pinObj3		= null;
			IPin	outPin		= null;

			try
			{
				int iTimeShiftBuffer=30;
				using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
				{
					iTimeShiftBuffer= xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
					if (iTimeShiftBuffer<5) iTimeShiftBuffer=5;
				}
				iTimeShiftBuffer*=60; //in seconds
				int iFileDuration = iTimeShiftBuffer/6;

				//create StreamBufferSink filter
				//				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateSinkSource()");
				hr=m_graphBuilder.AddFilter((IBaseFilter)m_StreamBufferSink,"StreamBufferSink");
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot add StreamBufferSink:{0:X}",hr);
					return false;
				}			
				//create MPEG2 Analyzer filter
				//				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Add mpeg2 analyzer()");
				hr=m_graphBuilder.AddFilter((IBaseFilter)m_mpeg2Analyzer,"Mpeg2 Analyzer");
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot add mpeg2 analyzer to graph:{0:X}",hr);
					return false;
				}

				//connect mpeg2 demuxer video out->mpeg2 analyzer input pin
				//get input pin of MPEG2 Analyzer filter
				//				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:find mpeg2 analyzer input pin()");
				pinObj0=DirectShowUtil.FindPinNr((IBaseFilter)m_mpeg2Analyzer,PinDirection.Input,0);
				if(pinObj0 == null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find mpeg2 analyzer input pin");
					return false;
				}
				
				//				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect demux video output->mpeg2 analyzer");
				hr=m_graphBuilder.Connect(m_DemuxVideoPin, pinObj0) ;
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect demux video output->mpeg2 analyzer:{0:X}",hr);
					return false;
				}

				//connect MPEG2 analyzer Filter->stream buffer sink pin 0
				//get output pin #0 from MPEG2 analyzer Filter
				//				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:mpeg2 analyzer output->streambuffersink in");
				pinObj1 = DirectShowUtil.FindPinNr((IBaseFilter)m_mpeg2Analyzer, PinDirection.Output, 0);	
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find mpeg2 analyzer output pin:{0:X}",hr);
					return false;
				}
				
				//get input pin #0 from StreamBufferSink Filter
				pinObj2 = DirectShowUtil.FindPinNr((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 0);	
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find SBE input pin:{0:X}",hr);
					return false;
				}

				hr=m_graphBuilder.Connect(pinObj1, pinObj2) ;
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect mpeg2 analyzer->streambuffer sink:{0:X}",hr);
					return false;
				}

				if (!useAC3)
				{
					//connect MPEG2 demuxer audio output ->StreamBufferSink Input #1
					//Get StreamBufferSink InputPin #1
					pinObj3 = DirectShowUtil.FindPinNr((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 1);	
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find SBE input pin#2");
						return false;
					}
					hr=m_graphBuilder.Connect(m_DemuxAudioPin, pinObj3) ;
					if (hr!=0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect mpeg2 demuxer audio out->streambuffer sink in#2:{0:X}",hr);
						return false;
					}
				}
				else
				{
					//connect ac3 pin ->stream buffersink input #2
					if (m_pinAC3Out!=null)
					{
						pinObj3 = DirectShowUtil.FindPinNr((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 1);	
						if (hr!=0)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find SBE input pin#2");
							return false;
						}
						hr=m_graphBuilder.Connect(m_pinAC3Out, pinObj3) ;
						if (hr!=0)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to connect mpeg2 demuxer AC3 out->streambuffer sink in#2:{0:X}",hr);
							return false;
						}
					}
				}
				int ipos=fileName.LastIndexOf(@"\");
				string strDir=fileName.Substring(0,ipos);
				m_StreamBufferConfig	= new StreamBufferConfig();
				m_IStreamBufferConfig	= (IStreamBufferConfigure) m_StreamBufferConfig;
				
				// setting the StreamBufferEngine registry key
				IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
				IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_IStreamBufferConfig;
				IntPtr subKey = IntPtr.Zero;

				RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
				hr=pTemp.SetHKEY(subKey);
				
				//set timeshifting folder
				//				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:set timeshift folder to:{0}", strDir);
				hr = m_IStreamBufferConfig.SetDirectory(strDir);	
				if(hr != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to set timeshift folder to:{0} {1:X}", strDir,hr);
					return false;
				}

				//set number of timeshifting files
				hr = m_IStreamBufferConfig.SetBackingFileCount(6, 8);    //4-6 files
				if(hr != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to set timeshifting files to 6-8 {0:X}",hr);
					return false;
				}
				
				//set duration of each timeshift file
				hr = m_IStreamBufferConfig.SetBackingFileDuration((uint)iFileDuration); // 60sec * 4 files= 4 mins
				if(hr != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to set timeshifting filesduration to {0} {1:X}",iFileDuration,hr);
					return false;
				}

				subKey = IntPtr.Zero;
				HKEY = (IntPtr) unchecked ((int)0x80000002L);
				IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_StreamBufferSink;

				RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
				hr = pConfig.SetHKEY(subKey);
				//set timeshifting filename
				//				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:set timeshift file to:{0}", fileName);
				
				IStreamBufferConfigure2 streamConfig2	= m_StreamBufferConfig as IStreamBufferConfigure2;
				if (streamConfig2!=null)
					streamConfig2.SetFFTransitionRates(8,32);

				// lock on the 'filename' file
				hr=m_IStreamBufferSink.LockProfile(fileName);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED to set timeshift file to:{0} {1:X}", fileName,hr);
					return false;
				}
			}
			finally
			{
				if(pinObj0 != null)
					Marshal.ReleaseComObject(pinObj0);
				if(pinObj1 != null)
					Marshal.ReleaseComObject(pinObj1);
				if(pinObj2 != null)
					Marshal.ReleaseComObject(pinObj2);
				if(pinObj3 != null)
					Marshal.ReleaseComObject(pinObj3);
				if(outPin != null)
					Marshal.ReleaseComObject(outPin);

				//if ( streamBufferInitialize !=null)
				//Marshal.ReleaseComObject(streamBufferInitialize );
				
			}
			//			(m_graphBuilder as IMediaFilter).SetSyncSource(m_MPEG2Demultiplexer as IReferenceClock);
			return true;
#endif			
		}//private bool CreateSinkSource(string fileName)

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
									if(hr == 0) 
									{
										Marshal.ReleaseComObject(dsPin[0]);
										Marshal.ReleaseComObject(outPin[0]);
										Marshal.ReleaseComObject(pinEnum);
										Marshal.ReleaseComObject(downstreamPins);
										return true;
									}
									Marshal.ReleaseComObject(dsPin[0]);
								}
							}//while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
							Marshal.ReleaseComObject(downstreamPins);
						}//if (outputPinCounter == preferredOutputPin)
					}//if (pinDir == PinDirection.Output)
					Marshal.ReleaseComObject(outPin[0]);
				}//while(pinEnum.Next(1, outPin, out ulFetched) == 0) 
				pinEnum.Reset();        // Move back to start of enumerator
			}//if (preferredOutputPin > 0) 
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
							if(hr == 0) 
							{
								Marshal.ReleaseComObject(dsPin[0]);
								Marshal.ReleaseComObject(downstreamPins);
								Marshal.ReleaseComObject(testPin[0]);
								Marshal.ReleaseComObject(pinEnum);
								return true;
							}
						}//if (dsPinDir == PinDirection.Input)
						Marshal.ReleaseComObject(dsPin[0]);
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

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: setupTuningSpace()");
			if(m_NetworkProvider == null) 
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:network provider is null ");
				return false;
			}
			System.Guid classID;
			int hr=m_NetworkProvider.GetClassID(out classID);
			//			if (hr <=0)
			//			{
			//				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:cannot get classid of network provider");
			//				return false;
			//			}

			string strClassID = classID.ToString();
			strClassID = strClassID.ToLower();
			switch (strClassID) 
			{
				case "0dad2fdd-5fd7-11d3-8f50-00c04f7971e2":
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=ATSC");
					m_NetworkType = NetworkType.ATSC;
					break;
				case "dc0c0fe7-0485-4266-b93f-68fbf80ed834":
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=DVB-C");
					m_NetworkType = NetworkType.DVBC;
					break;
				case "fa4b375a-45b4-4d45-8440-263957b11623":
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=DVB-S");
					m_NetworkType = NetworkType.DVBS;
					break;
				case "216c62df-6d7f-4e9a-8571-05f14edb766a":
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Network=DVB-T");
					m_NetworkType = NetworkType.DVBT;
					break;
				default:
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED:unknown network type:{0} ",classID);
					return false;
			}//switch (strClassID) 

			TunerLib.ITuningSpaceContainer TuningSpaceContainer = (TunerLib.ITuningSpaceContainer) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_SystemTuningSpaces, true));
			if(TuningSpaceContainer == null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: Failed to get ITuningSpaceContainer");
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

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: check available tuningspaces");
			TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;

			int Count = 0;
			Count = myTuningSpaces.Count;
			if(Count > 0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: found {0} tuning spaces", Count);
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
								Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: used tuningspace:{0} {1} {2}", counter, tuningSpaceFound.UniqueName,tuningSpaceFound.FriendlyName);
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
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: create new tuningspace");
			switch (m_NetworkType) 
			{
				case NetworkType.ATSC: 
				{
					TuningSpace = (TunerLib.ITuningSpace) Activator.CreateInstance(Type.GetTypeFromCLSID(TuningSpaces.CLSID_ATSCTuningSpace, true));
					TunerLib.IATSCTuningSpace myTuningSpace = (TunerLib.IATSCTuningSpace) TuningSpace;
					myTuningSpace.set__NetworkType(ref NetworkProviders.CLSID_ATSCNetworkProvider);
					myTuningSpace.InputType = TunerLib.tagTunerInputType.TunerInputAntenna;
					myTuningSpace.MaxChannel			= 10000;
					myTuningSpace.MaxMinorChannel		= 1;
					myTuningSpace.MaxPhysicalChannel	= 10000;
					myTuningSpace.MinChannel			= 1;
					myTuningSpace.MinMinorChannel		= 0;
					myTuningSpace.MinPhysicalChannel	= 0;
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
				Marshal.ReleaseComObject(propBag);
				if(String.Compare(Name, FriendlyName,true) == 0) // If found
				{
					object filterObj = null;
					System.Guid filterID = typeof(IBaseFilter).GUID;
					deviceMoniker[0].BindToObject(null, null, ref filterID, out filterObj);
					device = filterObj;
					
					filterObj = null;
					if(device != null) 
					{
						Marshal.ReleaseComObject(deviceMoniker[0]);
						Marshal.ReleaseComObject(enumMoniker);
						return true;
					}
				}//if(String.Compare(Name.ToLower(), FriendlyName.ToLower()) == 0) // If found
				Marshal.ReleaseComObject(deviceMoniker[0]);
			}//while(enumMoniker.Next(1, deviceMoniker, out ulFetched) == 0) // while == S_OK
			Marshal.ReleaseComObject(enumMoniker);
			device = null;
			return false;
		}//private bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device) 

		#endregion
		#region process helper functions
		// send PMT to firedtv device
		bool SendPMT()
		{
			try
			{

				string pmtName=String.Format(@"database\pmt\pmt_{0}_{1}_{2}_{3}_{4}.dat",
					Utils.FilterFileName(currentTuningObject.ServiceName),
					currentTuningObject.NetworkID,
					currentTuningObject.TransportStreamID,
					currentTuningObject.ProgramNumber,
					(int)Network());
				if (!System.IO.File.Exists(pmtName))
				{
					m_lastPMTVersion=-1;
					refreshPmtTable=true;
					return false;
				}
						
				byte[] pmt=null;
				using (System.IO.FileStream stream = new System.IO.FileStream(pmtName,System.IO.FileMode.Open,System.IO.FileAccess.Read,System.IO.FileShare.None))
				{
					long len=stream.Length;
					if (len>6)
					{
						pmt = new byte[len];
						stream.Read(pmt,0,(int)len);
						stream.Close();
					}
				}
				if (pmt==null)
				{
					m_lastPMTVersion=-1;
					refreshPmtTable=true;
					return false;
				}
				if (pmt.Length<6) 
				{
					m_lastPMTVersion=-1;
					refreshPmtTable=true;
					return false;
				}

				DVBSections sections = new DVBSections();
				DVBSections.ChannelInfo info = new DVBSections.ChannelInfo();
				if (!sections.GetChannelInfoFromPMT(pmt, ref info)) 
				{
					m_lastPMTVersion=-1;
					refreshPmtTable=true;
					return false;
				}
				if (info.pid_list!=null)
				{
					
					bool hasAudio=false;
					int audioOptions=0;
					for (int pids =0; pids < info.pid_list.Count;pids++)
					{
						DVBSections.PMTData data=(DVBSections.PMTData) info.pid_list[pids];
						if (data.isVideo)
						{
							currentTuningObject.VideoPid=data.elementary_PID;
						}
					
						if (data.isAudio) 
						{
							switch(audioOptions)
							{
								case 0:
									currentTuningObject.Audio1=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage1=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;
								case 1:
									currentTuningObject.Audio2=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage2=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;
								case 2:
									currentTuningObject.Audio3=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage3=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;

							}

							if (hasAudio==false)
							{
								currentTuningObject.AudioPid=data.elementary_PID;
								if(data.data!=null)
								{
									if(data.data.Length==3)
										currentTuningObject.AudioLanguage=DVBSections.GetLanguageFromCode(data.data);
								}
								hasAudio=true;
							}
						}//if (data.isAudio) 

						if (data.isAC3Audio)
						{
							currentTuningObject.AC3Pid=data.elementary_PID;
						}

						if (data.isTeletext)
						{
							currentTuningObject.TeletextPid=data.elementary_PID;
						}
					}//for (int pids =0; pids < info.pid_list.Count;pids++)

					currentTuningObject.PCRPid=info.pcr_pid;

					if (currentTuningObject.AC3Pid <=0) currentTuningObject.AC3Pid=-1;
					if (currentTuningObject.AudioPid <=0) currentTuningObject.AudioPid=-1;
					if (currentTuningObject.Audio1 <=0) currentTuningObject.Audio1=-1;
					if (currentTuningObject.Audio2 <=0) currentTuningObject.Audio2=-1;
					if (currentTuningObject.Audio2 <=0) currentTuningObject.Audio2=-1;
					//if (currentTuningObject.SubtitlePid <=0) currentTuningObject.SubtitlePid=-1;
					if (currentTuningObject.TeletextPid <=0) currentTuningObject.TeletextPid=-1;
					if (currentTuningObject.VideoPid <=0) currentTuningObject.VideoPid=-1;
					if (currentTuningObject.VideoPid <=0) currentTuningObject.PMTPid=-1;
					try
					{
						if (m_graphState==State.Radio && (currentTuningObject.PCRPid<=0||currentTuningObject.PCRPid>=0x1fff))
						{
							Log.Write("DVBGraphBDA:SendPMT() setup demux:audio pid:{0:X} AC3 pid:{1:X} pcrpid:{2:X}",currentTuningObject.AudioPid,currentTuningObject.AC3Pid,currentTuningObject.PCRPid);
							SetupDemuxer(m_DemuxVideoPin,0,m_DemuxAudioPin,0,m_pinAC3Out,0);
							SetupDemuxerPin(m_pinMPG1Out,currentTuningObject.AudioPid,(int)MediaSampleContent.TransportPayload,true);
							SetupDemuxerPin(m_pinMPG1Out,currentTuningObject.PCRPid,(int)MediaSampleContent.TransportPacket,false);
							
							//setup demuxer MTS pin 
							SetupMTSDemuxerPin();
						}
						else
						{
							Log.Write("DVBGraphBDA:SendPMT() set demux: video pid:{0:X} audio pid:{1:X} AC3 pid:{2:X}",currentTuningObject.VideoPid,currentTuningObject.AudioPid,currentTuningObject.AC3Pid);
							SetupDemuxer(m_DemuxVideoPin,currentTuningObject.VideoPid,m_DemuxAudioPin,currentTuningObject.AudioPid, m_pinAC3Out,currentTuningObject.AC3Pid);
							//setup demuxer MTS pin 
							SetupMTSDemuxerPin();
						}
					}
					catch(Exception)
					{
					}
				}//if (info.pid_list!=null)

				refreshPmtTable=false;
				VideoCaptureProperties props = new VideoCaptureProperties(m_TunerDevice);
				if (!props.IsCISupported()) return true;
				int pmtVersion = ((pmt[5]>>1)&0x1F);

				// send the PMT table to the device
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Process() send PMT version {0} to device",pmtVersion);
				if(props.SendPMT(currentTuningObject.VideoPid,currentTuningObject.AudioPid, pmt, (int)pmt.Length))
				{
					Log.Write("DVBGraphBDA:SendPMT() signal strength:{0} signal quality:{1}",SignalStrength(), SignalQuality() );
					return true;
				}
				else
				{
					refreshPmtTable=true;
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"ERROR: exception while sending pmt {0} {1} {2}",ex.Message,ex.Source,ex.StackTrace);
			}
			Log.Write("DVBGraphBDA:SendPMT() signal strength:{0} signal quality:{1}",SignalStrength(), SignalQuality() );
			return false;
		}//SendPMT()

		void LoadLNBSettings( ref DVBChannel ch, out int lowOsc, out int hiOsc, out int diseqcUsed)
		{
			diseqcUsed=0;
			lowOsc=9750;
			hiOsc=10600;
			try
			{
				string filename=String.Format(@"database\card_{0}.xml",m_Card.FriendlyName);

				int lnbKhz=0;
				int diseqc=0;
				int lnbKind=0;
				// lnb config
				int lnb0MHZ=0;
				int lnb1MHZ=0;
				int lnbswMHZ=0;
				int cbandMHZ=0;
				int circularMHZ=0;

				using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(filename))
				{
					lnb0MHZ=xmlreader.GetValueAsInt("dvbs","LNB0",9750);
					lnb1MHZ=xmlreader.GetValueAsInt("dvbs","LNB1",10600);
					lnbswMHZ=xmlreader.GetValueAsInt("dvbs","Switch",11700);
					cbandMHZ=xmlreader.GetValueAsInt("dvbs","CBand",5150);
					circularMHZ=xmlreader.GetValueAsInt("dvbs","Circular",10750);
					switch(ch.DiSEqC)
					{
						case 1:
							// config a
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind",0);
							Log.Write("DVBGraphBDA: using profile diseqc 1 LNB:{0} KHz diseqc:{1} lnbKind:{2}",lnbKhz,diseqc,lnbKind);
							break;
						case 2:
							// config b
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb2",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc2",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind2",0);
							Log.Write("DVBGraphBDA: using profile diseqc 2 LNB:{0} KHz diseqc:{1} lnbKind:{2}",lnbKhz,diseqc,lnbKind);
							break;
						case 3:
							// config c
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb3",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc3",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind3",0);
							Log.Write("DVBGraphBDA: using profile diseqc 3 LNB:{0} KHz diseqc:{1} lnbKind:{2}",lnbKhz,diseqc,lnbKind);
							break;
							//
						case 4:
							// config d
							lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb4",44);
							diseqc=xmlreader.GetValueAsInt("dvbs","diseqc4",0);
							lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind4",0);
							Log.Write("DVBGraphBDA: using profile diseqc 4 LNB:{0} KHz diseqc:{1} lnbKind:{2}",lnbKhz,diseqc,lnbKind);
							//
							break;
					}// switch(disNo)
				}//using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(m_cardFilename))

				// set values to dvbchannel-object
				// set the lnb parameter 
				if(ch.Frequency>=lnbswMHZ*1000)
				{
					ch.LNBFrequency=lnb1MHZ;
					ch.LNBKHz=lnbKhz;
				}
				else
				{
					ch.LNBFrequency=lnb0MHZ;
					ch.LNBKHz=-1;
				}
				lowOsc=lnb0MHZ;
				hiOsc=lnb1MHZ;
				diseqcUsed=diseqc;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: LNB Settings: freq={0} lnbKhz={1} lnbFreq={2} diseqc={3}",ch.Frequency,ch.LNBKHz,ch.LNBFrequency,ch.DiSEqC); 
			}
			catch(Exception)
			{
			}
		} //void LoadLNBSettings(TunerLib.IDVBTuneRequest tuneRequest)
		
		void SetLNBSettings(int disEqcUsed,TunerLib.IDVBSTuningSpace dvbSpace)
		{
			try
			{
				if (dvbSpace==null) return;
				//
				// A:LOWORD -> LOBYTE -> Bit0 for Position (0-A,1-B)
				// B:LOWORD -> HIBYTE -> Bit0 for 22Khz    (0-Off,1-On)
				// C:HIWORD -> LOBYTE -> Bit0 for Option   (0-A,1-B)
				// D:HIWORD -> HIBYTE -> Bit0 for Burst    (0-Off,1-On)
				// hi         low        hi        low
				// 87654321 | 87654321 | 87654321 | 87654321 | 
				//        D          C          B          A
				long inputRange=0;
				switch (disEqcUsed)
				{
					case 0: //none
						Log.Write("DVBGraphBDA: disEqc:none");
						return; 
					case 1: //simple A
						Log.Write("DVBGraphBDA: disEqc:simple A (not supported)");
						return; 
					case 2: //simple B
						Log.Write("DVBGraphBDA: disEqc:simple B (not supported)");
						return;
					case 3: //Level 1 A/A
						Log.Write("DVBGraphBDA: disEqc:level 1 A/A");
						inputRange=0;
						break;
					case 4: //Level 1 B/A
						Log.Write("DVBGraphBDA: disEqc:level 1 B/A");
						inputRange=1<<16;
						break;
					case 5: //Level 1 A/B
						Log.Write("DVBGraphBDA: disEqc:level 1 A/B");
						inputRange=1;
						break;
					case 6: //Level 1 B/B
						Log.Write("DVBGraphBDA: disEqc:level 1 B/B");
						inputRange=(1<<16)+1;
						break;
				}
				// test with burst on
				//inputRange|=1<<24;

				if (currentTuningObject.LNBKHz==1) // 22khz 
					inputRange |= (1<<8);

				Log.Write("DVBGraphBDA: Set inputrange to:{0:X}",inputRange);
				dvbSpace.InputRange=inputRange.ToString();
			}
			catch(Exception)
			{
			}
		}

		void CheckVideoResolutionChanges()
		{
			if (GUIGraphicsContext.Vmr9Active) return;
			if (m_graphState != State.Viewing) return ;
			if (m_videoWindow==null || m_basicVideo==null) return;
			int aspectX, aspectY;
			int videoWidth=1, videoHeight=1;
			if (m_basicVideo!=null)
			{
				m_basicVideo.GetVideoSize(out videoWidth, out videoHeight);
			}
			aspectX=videoWidth;
			aspectY=videoHeight;
			if (m_basicVideo!=null)
			{
				m_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			if (videoHeight!=m_iVideoHeight || videoWidth != m_iVideoWidth ||
				aspectX != m_aspectX || aspectY != m_aspectY)
			{
				GUIGraphicsContext_OnVideoWindowChanged();
			}
		}
		public void Process()
		{
			if (m_graphState==State.None || m_graphState==State.Created) return;

			if (m_streamDemuxer!=null)
				m_streamDemuxer.Process();
		
			m_epgGrabber.Process();

			TimeSpan ts=DateTime.Now-updateTimer;
			bool reTune=false;

			if (ts.TotalMilliseconds>800)
			{
				if(!GUIGraphicsContext.Vmr9Active && !g_Player.Playing)
				{
					CheckVideoResolutionChanges();
				}
				if(!GUIGraphicsContext.Vmr9Active && Vmr7!=null && m_graphState==State.Viewing)
				{
					Vmr7.Process();
				}
				if(GUIGraphicsContext.Vmr9Active)
				{
					if (GUIGraphicsContext.Vmr9FPS < 1f)
					{
						ts = DateTime.Now-timeRetune;
						if (ts.TotalSeconds>=5)
						{
							reTune=true;
							timeRetune=DateTime.Now;
						}
					}
					else timeRetune=DateTime.Now;
				}
				updateTimer=DateTime.Now;
			}

			if (refreshPmtTable && Network()!=NetworkType.ATSC)
			{
				bool gotPMT=false;
				refreshPmtTable	= false;
				//Log.Write("DVBGRAPHBDA:Get PMT");
				IntPtr pmtMem=Marshal.AllocCoTaskMem(4096);// max. size for pmt
				if(pmtMem!=IntPtr.Zero)
				{
					m_analyzerInterface.SetPMTProgramNumber(currentTuningObject.ProgramNumber);
					int res=m_analyzerInterface.GetPMTData(pmtMem);
					if(res!=-1)
					{
						byte[] pmt=new byte[res];
						int version=-1;
						Marshal.Copy(pmtMem,pmt,0,res);
						version=((pmt[5]>>1)&0x1F);
						int pmtProgramNumber=(pmt[3]<<8)+pmt[4];
						if (pmtProgramNumber==currentTuningObject.ProgramNumber)
						{
							gotPMT=true;
							if(m_lastPMTVersion!=version)
							{
								Log.Write("DVBGRAPHBDA:Got PMT version:{0}",version);
								m_lastPMTVersion=version;
								m_streamDemuxer_OnPMTIsChanged(pmt);
							}
							else 
							{
							//	Log.Write("DVBGRAPHBDA:Got old PMT version:{0} {1}",m_lastPMTVersion,version);
							}
						}
						else 
						{
							ushort chcount=0;
							m_analyzerInterface.GetChannelCount(ref chcount);
							Log.Write("DVBGRAPHBDA:Got wrong PMT:{0} {1} channels:{2}",pmtProgramNumber,currentTuningObject.ProgramNumber, chcount);
							ts = DateTime.Now-timeRetune;
							if (ts.TotalSeconds>5)
							{
								reTune=true;
								timeRetune=DateTime.Now;
							}
						}
						pmt=null;
					}
					else 
					{
						//Log.Write("DVBGRAPHBDA:could not get PMT");
						ts = DateTime.Now-timeRetune;
						if (ts.TotalSeconds>=5)
						{
							reTune=true;
							timeRetune=DateTime.Now;
						}
					}
					Marshal.FreeCoTaskMem(pmtMem);
				}

				if (!gotPMT)
				{
					refreshPmtTable=true;
				}
				else
				{
					SendPMT();
					return;
				}
			}
/*
			if (!reTune) return;
			ushort count=0;
			m_analyzerInterface.GetChannelCount(ref count);
			Log.Write("DVBGraphBDA: no video->retune.. strength:{0} quality:{1} channels:{2}", SignalStrength(), SignalQuality(), count);
			SubmitTuneRequest(currentTuningObject);
			if (m_streamDemuxer != null)
			{
				m_streamDemuxer.ResetGrabber();
				m_streamDemuxer.SetChannelData(currentTuningObject.AudioPid, currentTuningObject.VideoPid, currentTuningObject.TeletextPid, currentTuningObject.Audio3, currentTuningObject.ServiceName,currentTuningObject.PMTPid,currentTuningObject.ProgramNumber);
			}
			m_lastPMTVersion=-1;
			m_analyzerInterface.ResetParser();

			refreshPmtTable =true;*/
		}//public void Process()

		#endregion


		#region Tuning
		/// <summary>
		/// Switches / tunes to another TV channel
		/// </summary>
		/// <param name="iChannel">New channel</param>
		/// <remarks>
		/// Graph should be viewing or timeshifting. 
		/// </remarks>
		public void TuneChannel(TVChannel channel)
		{
			if (m_NetworkProvider==null) return;
			m_lastPMTVersion=-1;
			try
			{
				if (Vmr9!=null) Vmr9.Enable(false);
				m_iPrevChannel		= m_iCurrentChannel;
				m_iCurrentChannel = channel.Number;
				m_StartTime				= DateTime.Now;
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() tune to channel:{0}", channel.ID);

				
				int bandWidth=-1;
				int frequency=-1,ONID=-1,TSID=-1,SID=-1;
				int audioPid=-1, videoPid=-1, teletextPid=-1, pmtPid=-1, pcrPid=-1;
				string providerName;
				int audio1,audio2,audio3,ac3Pid;
				string audioLanguage,audioLanguage1,audioLanguage2,audioLanguage3;
				bool HasEITPresentFollow,HasEITSchedule;
				switch (m_NetworkType)
				{
					case NetworkType.ATSC: 
					{
						//get the ATSC tuning details from the tv database
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get ATSC tuning details");
						int symbolrate=0,innerFec=0,modulation=0,physicalChannel=0;
						int minorChannel=0,majorChannel=0;
						TVDatabase.GetATSCTuneRequest(channel.ID,out physicalChannel,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out minorChannel,out majorChannel,out HasEITPresentFollow,out HasEITSchedule,out pcrPid);
						frequency=0;
						symbolrate=0;
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz physicalChannel:{1} major channel:{2} minor channel:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8} video:0x{9:X} audio:0x{10:X} pcr:0x{11:X}", 
							frequency,physicalChannel,minorChannel, majorChannel, modulation, ONID, TSID, SID,providerName,videoPid,audioPid,pcrPid);
						
						currentTuningObject=new DVBChannel();
						currentTuningObject.PhysicalChannel=physicalChannel;
						currentTuningObject.MinorChannel=minorChannel;
						currentTuningObject.MajorChannel=majorChannel;
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=videoPid;
						currentTuningObject.TeletextPid=teletextPid;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=audioLanguage;
						currentTuningObject.AudioLanguage1=audioLanguage1;
						currentTuningObject.AudioLanguage2=audioLanguage2;
						currentTuningObject.AudioLanguage3=audioLanguage3;
						currentTuningObject.AC3Pid=ac3Pid;
						currentTuningObject.PCRPid=pcrPid;
						currentTuningObject.Audio1=audio1;
						currentTuningObject.Audio2=audio2;
						currentTuningObject.Audio3=audio3;
						currentTuningObject.HasEITSchedule=HasEITSchedule;
						currentTuningObject.HasEITPresentFollow=HasEITPresentFollow;
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() submit tuning request");
						SubmitTuneRequest(currentTuningObject);
					} break;
					
					case NetworkType.DVBC: 
					{
						//get the DVB-C tuning details from the tv database
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get DVBC tuning details");
						int symbolrate=0,innerFec=0,modulation=0;
						TVDatabase.GetDVBCTuneRequest(channel.ID,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3,out HasEITPresentFollow,out HasEITSchedule,out pcrPid);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.ID);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							frequency,symbolrate, innerFec, modulation, ONID, TSID, SID,providerName);
						bool needSwitch=true;
						/*if (currentTuningObject!=null)
						{
							if (currentTuningObject.Frequency==frequency &&
								currentTuningObject.Symbolrate==symbolrate &&
								currentTuningObject.Modulation==innerFec &&
								currentTuningObject.FEC==innerFec &&
								currentTuningObject.NetworkID==ONID &&
								currentTuningObject.TransportStreamID==TSID)
							{
								needSwitch=false;
							}
						}*/
						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=videoPid;
						currentTuningObject.TeletextPid=teletextPid;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=audioLanguage;
						currentTuningObject.AudioLanguage1=audioLanguage1;
						currentTuningObject.AudioLanguage2=audioLanguage2;
						currentTuningObject.AudioLanguage3=audioLanguage3;
						currentTuningObject.AC3Pid=ac3Pid;
						currentTuningObject.Audio1=audio1;
						currentTuningObject.Audio2=audio2;
						currentTuningObject.Audio3=audio3;
						currentTuningObject.PCRPid=pcrPid;
						currentTuningObject.HasEITPresentFollow=HasEITPresentFollow;
						currentTuningObject.HasEITSchedule=HasEITSchedule;
						if (needSwitch)
							SubmitTuneRequest(currentTuningObject);
						else 
							SetPids();

					} break;

					case NetworkType.DVBS: 
					{					
						//get the DVB-S tuning details from the tv database
						//for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get DVBS tuning details");
						DVBChannel ch=new DVBChannel();
						if(TVDatabase.GetSatChannel(channel.ID,1,ref ch)==false)//only television
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.ID);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							ch.Frequency,ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID,ch.ProgramNumber,ch.ServiceProvider);

						bool needSwitch=true;
						/*if (currentTuningObject!=null)
						{
							if (currentTuningObject.Frequency==frequency &&
									currentTuningObject.FEC==ch.FEC &&
									currentTuningObject.Polarity==ch.Polarity &&
									currentTuningObject.LNBFrequency==ch.LNBFrequency &&
									currentTuningObject.LNBKHz==ch.LNBKHz &&
									currentTuningObject.DiSEqC==ch.DiSEqC &&
									currentTuningObject.NetworkID==ch.NetworkID&&
									currentTuningObject.TransportStreamID==ch.TransportStreamID)
							{
								needSwitch=false;
							}
						}*/
						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=ch.Frequency;
						currentTuningObject.Symbolrate=ch.Symbolrate;
						currentTuningObject.FEC=ch.FEC;
						currentTuningObject.Polarity=ch.Polarity;
						currentTuningObject.NetworkID=ch.NetworkID;
						currentTuningObject.TransportStreamID=ch.TransportStreamID;
						currentTuningObject.ProgramNumber=ch.ProgramNumber;
						currentTuningObject.AudioPid=ch.AudioPid;
						currentTuningObject.VideoPid=ch.VideoPid;
						currentTuningObject.TeletextPid=ch.TeletextPid;
						currentTuningObject.PMTPid=ch.PMTPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=ch.AudioLanguage;
						currentTuningObject.AudioLanguage1=ch.AudioLanguage1;
						currentTuningObject.AudioLanguage2=ch.AudioLanguage2;
						currentTuningObject.AudioLanguage3=ch.AudioLanguage3;
						currentTuningObject.AC3Pid=ch.AC3Pid;
						currentTuningObject.PCRPid=ch.PCRPid;
						currentTuningObject.Audio1=ch.Audio1;
						currentTuningObject.Audio2=ch.Audio2;
						currentTuningObject.Audio3=ch.Audio3;
						currentTuningObject.DiSEqC=ch.DiSEqC;
						currentTuningObject.LNBFrequency=ch.LNBFrequency;
						currentTuningObject.LNBKHz=ch.LNBKHz;
						currentTuningObject.HasEITPresentFollow=ch.HasEITPresentFollow;
						currentTuningObject.HasEITSchedule=ch.HasEITSchedule;
						if (needSwitch)
							SubmitTuneRequest(currentTuningObject);
						else 
							SetPids();

					} break;

					case NetworkType.DVBT: 
					{
						//get the DVB-T tuning details from the tv database
						//for DVB-T this is the frequency, ONID , TSID and SID
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get DVBT tuning details");
						TVDatabase.GetDVBTTuneRequest(channel.ID,out providerName,out frequency, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3,out HasEITPresentFollow,out HasEITSchedule,out pcrPid);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.ID);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency, ONID, TSID, SID,providerName);
						//get the IDVBTLocator interface from the new tuning request

						bool needSwitch=true;
						/*if (currentTuningObject!=null)
						{
							if (currentTuningObject.Frequency==frequency &&
									currentTuningObject.NetworkID==ONID &&
									currentTuningObject.TransportStreamID==TSID)
							{
								needSwitch=false;
							}
						}*/
						currentTuningObject=new DVBChannel();
						currentTuningObject.Bandwidth=bandWidth;
						currentTuningObject.Frequency=frequency;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=videoPid;
						currentTuningObject.TeletextPid=teletextPid;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.AudioLanguage=audioLanguage;
						currentTuningObject.AudioLanguage1=audioLanguage1;
						currentTuningObject.AudioLanguage2=audioLanguage2;
						currentTuningObject.AudioLanguage3=audioLanguage3;
						currentTuningObject.AC3Pid=ac3Pid;
						currentTuningObject.PCRPid=pcrPid;
						currentTuningObject.Audio1=audio1;
						currentTuningObject.Audio2=audio2;
						currentTuningObject.Audio3=audio3;
						currentTuningObject.HasEITPresentFollow=HasEITPresentFollow;
						currentTuningObject.HasEITSchedule=HasEITSchedule;
						if (needSwitch)
							SubmitTuneRequest(currentTuningObject);
						else 
							SetPids();

					} break;
				}	//switch (m_NetworkType)
				//submit tune request to the tuner

				if (currentTuningObject.AC3Pid <=0) currentTuningObject.AC3Pid=-1;
				if (currentTuningObject.AudioPid <=0) currentTuningObject.AudioPid=-1;
				if (currentTuningObject.Audio1 <=0) currentTuningObject.Audio1=-1;
				if (currentTuningObject.Audio2 <=0) currentTuningObject.Audio2=-1;
				if (currentTuningObject.Audio2 <=0) currentTuningObject.Audio2=-1;
				//if (currentTuningObject.SubtitlePid <=0) currentTuningObject.SubtitlePid=-1;
				if (currentTuningObject.TeletextPid <=0) currentTuningObject.TeletextPid=-1;
				if (currentTuningObject.VideoPid <=0) currentTuningObject.VideoPid=-1;
				if (currentTuningObject.VideoPid <=0) currentTuningObject.PMTPid=-1;			

				DirectShowUtil.EnableDeInterlace(m_graphBuilder);
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() done freq:{0} ONID:{1} TSID:{2} prog:{3} audio:{4:X} video:{5:X} pmt:{6:X} ac3:{7:X} txt:{8:X}",
																						currentTuningObject.Frequency,
																						currentTuningObject.NetworkID,currentTuningObject.TransportStreamID,
																						currentTuningObject.ProgramNumber,currentTuningObject.Audio1,
																					  currentTuningObject.VideoPid,currentTuningObject.PMTPid,
																						currentTuningObject.AC3Pid,currentTuningObject.TeletextPid);


				timeRetune=DateTime.Now;
				refreshPmtTable	= true;

				if(m_pluginsEnabled==true)
					ExecTuner();
				m_lastPMTVersion=-1;
				m_analyzerInterface.ResetParser();
				if (Network()==NetworkType.ATSC)
					m_epgGrabber.GrabEPG(false);
				SetupMTSDemuxerPin();
			}	
			finally
			{
				if (Vmr9!=null) Vmr9.Enable(true);
			}
		}//public void TuneChannel(AnalogVideoStandard standard,int iChannel,int country)


		public void TuneFrequency(int frequency)
		{
		}


		#region TuneRequest
		void SubmitTuneRequest(DVBChannel ch)
		{
			if (ch==null) return;
			try
			{
				if (m_NetworkProvider==null) return;
				//get the ITuner interface from the network provider filter
				TunerLib.TuneRequest newTuneRequest = null;
				TunerLib.ITuner myTuner = m_NetworkProvider as TunerLib.ITuner;
				if (myTuner==null) return;
				switch (m_NetworkType)
				{
					case NetworkType.ATSC: 
					{
						//get the IATSCTuningSpace from the tuner
						TunerLib.IATSCChannelTuneRequest myATSCTuneRequest=null;
						TunerLib.IATSCTuningSpace myAtscTuningSpace =null;
						myAtscTuningSpace = myTuner.TuningSpace as TunerLib.IATSCTuningSpace;
						if (myAtscTuningSpace ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed SubmitTuneRequest() tuningspace=null");
							return;
						}

						//create a new tuning request
						newTuneRequest = myAtscTuningSpace.CreateTuneRequest();
						if (newTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed SubmitTuneRequest() could not create new tuningrequest");
							return;
						}
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() cast new tuningrequest to IATSCChannelTuneRequest");
						myATSCTuneRequest = newTuneRequest as TunerLib.IATSCChannelTuneRequest;
						if (myATSCTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
							return ;
						}

						//get the IATSCLocator interface from the new tuning request
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() get IATSCLocator interface");
						TunerLib.IATSCLocator myLocator = myATSCTuneRequest.Locator as TunerLib.IATSCLocator;	
						if (myLocator==null)
						{
							myLocator = myAtscTuningSpace.DefaultLocator as TunerLib.IATSCLocator;
						}
							
							
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get IATSCLocator", ch.Frequency);
							return ;
						}
						//set the properties on the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest(ATSC) set tuning properties. Freq:{0} physical:{1} major:{2} minor:{3} SR:{4} mod:{5} tsid:{6}",
																							ch.Frequency,ch.PhysicalChannel,ch.MajorChannel,ch.MinorChannel,ch.Symbolrate,ch.Modulation, ch.TransportStreamID);
						myLocator.CarrierFrequency		= -1;//ch.Frequency;
						myLocator.PhysicalChannel			= ch.PhysicalChannel;
						myLocator.SymbolRate				  = -1;
						myLocator.TSID								= -1;//ch.TransportStreamID;

						myLocator.InnerFEC						= TunerLib.FECMethod.BDA_FEC_METHOD_NOT_SET;
						myLocator.Modulation					= (TunerLib.ModulationType)ch.Modulation;
						myATSCTuneRequest.MinorChannel= ch.MinorChannel;
						myATSCTuneRequest.Channel		  = ch.MajorChannel;
						myATSCTuneRequest.Locator=(TunerLib.Locator)myLocator;
						myTuner.TuneRequest = newTuneRequest;
						//Marshal.ReleaseComObject(myATSCTuneRequest);

					}
						break;

					case NetworkType.DVBC: 
					{
						TunerLib.IDVBTuningSpace2 myTuningSpace =null;
						//get the IDVBTuningSpace2 from the tuner
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() get IDVBTuningSpace2");
						myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
						if (myTuningSpace==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. Invalid tuningspace");
							return ;
						}


						//create a new tuning request
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() create new tuningrequest");
						newTuneRequest = myTuningSpace.CreateTuneRequest();
						if (newTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
							return ;
						}


						TunerLib.IDVBTuneRequest myTuneRequest=null;
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() cast new tuningrequest to IDVBTuneRequest");
						myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
						if (myTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
							return ;
						}

						//get the IDVBCLocator interface from the new tuning request
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() get IDVBCLocator interface");
						TunerLib.IDVBCLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBCLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBCLocator;
						}
							
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz. cannot get locator", ch.Frequency);
							return ;
						}
						//set the properties on the new tuning request
							
							
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() set tuning properties to tuning request");
						myLocator.CarrierFrequency		= ch.Frequency;
						myLocator.SymbolRate				  = ch.Symbolrate;
						myLocator.InnerFEC						= (TunerLib.FECMethod)ch.FEC;
						myLocator.Modulation					= (TunerLib.ModulationType)ch.Modulation;
						myTuneRequest.ONID	= ch.NetworkID;					//original network id
						myTuneRequest.TSID	= ch.TransportStreamID;					//transport stream id
						myTuneRequest.SID		= ch.ProgramNumber;					//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() submit tuning request");
						myTuner.TuneRequest = newTuneRequest;
						//Marshal.ReleaseComObject(myTuneRequest);


					} break;

					case NetworkType.DVBS:
					{
						//get the IDVBSLocator interface
						int lowOsc,hiOsc,diseqcUsed;
						if (ch.DiSEqC<1) ch.DiSEqC=1;
						if (ch.DiSEqC>4) ch.DiSEqC=4;

						LoadLNBSettings(ref ch,out lowOsc, out hiOsc, out diseqcUsed);
						TunerLib.IDVBSTuningSpace dvbSpace =myTuner.TuningSpace as TunerLib.IDVBSTuningSpace;
						if (dvbSpace==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed could not get IDVBSTuningSpace");
							return;
						}

						Log.WriteFile(Log.LogType.Capture,false,"DVBGraphBDA: set LNBSwitch to {0} Khz lowOsc={1} MHz hiOsc={2} Mhz disecq:{3}",ch.LNBKHz, lowOsc, hiOsc, diseqcUsed);
						dvbSpace.LNBSwitch=ch.LNBKHz;
						dvbSpace.SpectralInversion=TunerLib.SpectralInversion.BDA_SPECTRAL_INVERSION_AUTOMATIC;
						dvbSpace.LowOscillator=lowOsc*1000;
						dvbSpace.HighOscillator=hiOsc*1000;
						
						SetLNBSettings(diseqcUsed,dvbSpace);

						newTuneRequest = dvbSpace.CreateTuneRequest();
						if (newTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed SubmitTuneRequest() could not create new tuningrequest");
							return;
						}
						TunerLib.IDVBTuneRequest myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
						if (myTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
							return ;
						}

						TunerLib.IDVBSLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBSLocator;	
						if (myLocator == null)
							myLocator = dvbSpace.DefaultLocator as TunerLib.IDVBSLocator;
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: failed SubmitTuneRequest() could not get IDVBSLocator");
							return;
						}
						//set the properties for the new tuning request.
						myLocator.CarrierFrequency		= ch.Frequency;
						myLocator.InnerFEC						= (TunerLib.FECMethod)ch.FEC;
						if (ch.Polarity==0) 
							myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_H;
						else
							myLocator.SignalPolarisation	= TunerLib.Polarisation.BDA_POLARISATION_LINEAR_V;
						
						myLocator.SymbolRate					= ch.Symbolrate;
						myTuneRequest.ONID						= ch.NetworkID;	//original network id
						myTuneRequest.TSID						= ch.TransportStreamID;	//transport stream id
						myTuneRequest.SID							= ch.ProgramNumber;		//service id
						myTuneRequest.Locator					= (TunerLib.Locator)myLocator;
						//and submit the tune request
						myTuner.TuneRequest  = newTuneRequest;
						//Marshal.ReleaseComObject(myTuneRequest);
					}
						break;

					case NetworkType.DVBT: 
					{
						TunerLib.IDVBTuningSpace2 myTuningSpace =null;
						//get the IDVBTuningSpace2 from the tuner
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() get IDVBTuningSpace2");
						myTuningSpace = myTuner.TuningSpace as TunerLib.IDVBTuningSpace2;
						if (myTuningSpace==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. Invalid tuningspace");
							return ;
						}


						//create a new tuning request
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() create new tuningrequest");
						newTuneRequest = myTuningSpace.CreateTuneRequest();
						if (newTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
							return ;
						}


						TunerLib.IDVBTuneRequest myTuneRequest=null;
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() cast new tuningrequest to IDVBTuneRequest");
						myTuneRequest = newTuneRequest as TunerLib.IDVBTuneRequest;
						if (myTuneRequest ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning. cannot create new tuningrequest");
							return ;
						}

						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() get IDVBTLocator");
						TunerLib.IDVBTLocator myLocator = myTuneRequest.Locator as TunerLib.IDVBTLocator;	
						if (myLocator==null)
						{
							myLocator = myTuningSpace.DefaultLocator as TunerLib.IDVBTLocator;
						}
							
						if (myLocator ==null)
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED tuning to frequency:{0} KHz ONID:{1} TSID:{2}, SID:{3}. cannot get locator", ch.Frequency,ch.NetworkID,ch.TransportStreamID,ch.ProgramNumber);
							return ;
						}
						//set the properties on the new tuning request
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SubmitTuneRequest() frequency:{0} KHz Bandwidth:{1} ONID:{2} TSID:{3}, SID:{4}", 
							ch.Frequency,ch.Bandwidth,ch.NetworkID,ch.TransportStreamID,ch.ProgramNumber);
						myLocator.CarrierFrequency		= ch.Frequency;
						myLocator.Bandwidth=ch.Bandwidth;
						myTuneRequest.ONID	= ch.NetworkID;					//original network id
						myTuneRequest.TSID	= ch.TransportStreamID;					//transport stream id
						myTuneRequest.SID		= ch.ProgramNumber;					//service id
						myTuneRequest.Locator=(TunerLib.Locator)myLocator;
						myTuner.TuneRequest = newTuneRequest;
						//Marshal.ReleaseComObject(myTuneRequest);
					} break;
				}
				SetPids();
				//	Log.Write("DVBGraphBDA: signal strength:{0} signal quality:{1}",SignalStrength(), SignalQuality() );
			}
			catch(Exception ex)
			{
				Log.Write("DVBGraphBDA: SubmitTuneRequest:{0} {1} {2}",ex.Message,ex.Source,ex.StackTrace );
			}
		}

		#endregion
		#region AutoTuning
		/// <summary>
		/// Tune to a specific channel
		/// </summary>
		/// <param name="tuningObject">
		/// DVBChannel object containing the tuning parameter.
		/// </param>
		/// <remarks>
		/// Graph should be created 
		/// </remarks>
		public void Tune(object tuningObject, int disecqNo)
		{
			//if no network provider then return;
			if (m_NetworkProvider==null) return;
			if (tuningObject		 ==null) return;

			//start viewing if we're not yet viewing
			if (!graphRunning)
			{

				Log.Write("Start graph!");
				if(m_mediaControl == null)
					m_mediaControl = (IMediaControl) m_graphBuilder;
				int hr=m_mediaControl.Run();
				if (hr<0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
				}

				graphRunning=true;
			}

			m_analyzerInterface.ResetParser();
			currentTuningObject=(DVBChannel)tuningObject;
			currentTuningObject.DiSEqC=disecqNo;
			SubmitTuneRequest(currentTuningObject);
		}//public void Tune(object tuningObject)
		
		/// <summary>
		/// Store any new tv and/or radio channels found in the tvdatabase
		/// </summary>
		/// <param name="radio">if true:Store radio channels found in the database</param>
		/// <param name="tv">if true:Store tv channels found in the database</param>
		public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels,ref int newRadioChannels, ref int updatedRadioChannels)
		{	
			//it may take a while before signal quality/level is correct
			if (m_dvbAnalyzer==null) return;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: StoreChannels() signal level:{0} signal quality:{1}",SignalStrength(),SignalQuality() );

			//get list of current tv channels present in the database
			ArrayList tvChannels = new ArrayList();
			TVDatabase.GetChannels(ref tvChannels);

			DVBSections.Transponder transp;
			transp.channels=null;
			m_analyzerInterface.ResetParser();

			using (DVBSections sections = new DVBSections())
			{
				ushort count=0;
				sections.DemuxerObject=m_streamDemuxer;
				sections.Timeout=2500;
				sections.GetTablesUsingMicrosoft=true;
				for (int i=0; i < 100; ++i)
				{
					bool allFound=true;
					m_analyzerInterface.GetChannelCount(ref count);
					if(count>0)
					{
						for(int t=0;t<count;t++)
						{
							if(m_analyzerInterface.IsChannelReady(t)!=0)
							{
								allFound=false;
								break;
							}
						}
					}
					else allFound=false;
					if (!allFound) System.Threading.Thread.Sleep(50);
				}

				m_analyzerInterface.GetChannelCount(ref count);
				if(count>0)
				{
					transp.channels=new ArrayList();
					for(int t=0;t<count;t++)
					{
						if(m_analyzerInterface.IsChannelReady(t)==0)
						{
							DVBSections.ChannelInfo chi=new MediaPortal.TV.Recording.DVBSections.ChannelInfo();
							UInt16 len=0;
							int hr=0;
							hr=m_analyzerInterface.GetCISize(ref len);					
							IntPtr mmch=Marshal.AllocCoTaskMem(len);
							hr=m_analyzerInterface.GetChannel((UInt16)t,mmch);
							//byte[] ch=new byte[len];
							//Marshal.Copy(mmch,ch,0,len);
							chi=sections.GetChannelInfo(mmch);
							chi.fec=currentTuningObject.FEC;
							if (Network() != NetworkType.ATSC)
							{
								chi.freq=currentTuningObject.Frequency;
							}
							else
							{
								currentTuningObject.Frequency=0;
								currentTuningObject.Modulation=chi.modulation;
							}
							Marshal.FreeCoTaskMem(mmch);
							transp.channels.Add(chi);
						}
						else Log.Write("DVBGraphBDA:channel {0} is not ready!!!",t);
					}
				}
			}
			if (transp.channels==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: found no channels", transp.channels);
				return;
			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: found {0} channels", transp.channels.Count);
			for (int i=0; i < transp.channels.Count;++i)
			{
				DVBSections.ChannelInfo info=(DVBSections.ChannelInfo)transp.channels[i];
				if (info.service_provider_name==null) info.service_provider_name="";
				if (info.service_name==null) info.service_name="";
				
				info.service_provider_name=info.service_provider_name.Trim();
				info.service_name=info.service_name.Trim();
				if (info.service_provider_name.Length==0 ) 
					info.service_provider_name=Strings.Unknown;
				if (info.service_name.Length==0)
					info.service_name=String.Format("NoName:{0}{1}{2}{3}",info.networkID,info.transportStreamID, info.serviceID,i );


				bool hasAudio=false;
				bool hasVideo=false;
				info.freq=currentTuningObject.Frequency;
				DVBChannel newchannel   = new DVBChannel();

				currentTuningObject.VideoPid=0;
				currentTuningObject.AudioPid=0;
				currentTuningObject.TeletextPid=0;
				currentTuningObject.AC3Pid=0;
				currentTuningObject.Audio1=0;
				currentTuningObject.Audio2=0;
				currentTuningObject.Audio3=0;
				currentTuningObject.AudioLanguage=String.Empty;
				currentTuningObject.AudioLanguage1=String.Empty;
				currentTuningObject.AudioLanguage2=String.Empty;
				currentTuningObject.AudioLanguage3=String.Empty;
				//check if this channel has audio/video streams
				int audioOptions=0;
				if (info.pid_list!=null)
				{
					for (int pids =0; pids < info.pid_list.Count;pids++)
					{
						DVBSections.PMTData data=(DVBSections.PMTData) info.pid_list[pids];
						if (data.isVideo)
						{
							currentTuningObject.VideoPid=data.elementary_PID;
							hasVideo=true;
						}
						
						if (data.isAudio) 
						{
							switch(audioOptions)
							{
								case 0:
									currentTuningObject.Audio1=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage1=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;
								case 1:
									currentTuningObject.Audio2=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage2=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;
								case 2:
									currentTuningObject.Audio3=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											currentTuningObject.AudioLanguage3=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions++;
									break;

							}

							if (hasAudio==false)
							{
								currentTuningObject.AudioPid=data.elementary_PID;
								if(data.data!=null)
								{
									if(data.data.Length==3)
										currentTuningObject.AudioLanguage=DVBSections.GetLanguageFromCode(data.data);
								}
								hasAudio=true;
							}
						}
						if (data.isAC3Audio)
						{
							hasAudio=true;
							currentTuningObject.AC3Pid=data.elementary_PID;
						}

						if (data.isTeletext)
						{
							currentTuningObject.TeletextPid=data.elementary_PID;
						}
					}
				}

				if (info.serviceType!=1 && info.serviceType!=2) 
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:unknown service type: provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:0x{9:X} videopid:0x{10:X} teletextpid:0x{11:X} program:{12} pcr pid:0x{13:X} service type:{14} major:{15} minor:{16}", 
																						info.service_provider_name,
																						info.service_name,
																						info.scrambled,
																						info.freq,
																						info.networkID,
																						info.transportStreamID,
																						info.serviceID,
																						hasVideo, ((!hasVideo) && hasAudio),
																						currentTuningObject.AudioPid,currentTuningObject.VideoPid,currentTuningObject.TeletextPid,
																						info.program_number,
																						info.pcr_pid,
																						info.serviceType,info.majorChannel,info.minorChannel);
					continue;
				}
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Found provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:0x{9:X} videopid:0x{10:X} teletextpid:0x{11:X} program:{12} pcr pid:0x{13:X} ac3 pid:0x{14:X} major:{15} minor:{16}", 
																						info.service_provider_name,
																						info.service_name,
																						info.scrambled,
																						info.freq,
																						info.networkID,
																						info.transportStreamID,
																						info.serviceID,
																						hasVideo, ((!hasVideo) && hasAudio),
																						currentTuningObject.AudioPid,currentTuningObject.VideoPid,currentTuningObject.TeletextPid,
																						info.program_number,
																						info.pcr_pid, currentTuningObject.AC3Pid,info.majorChannel,info.minorChannel);

				if (info.serviceID==0) 
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel#{0} has no service id",i);
					continue;
				}
				bool IsRadio		  = ((!hasVideo) && hasAudio);
				bool IsTv   		  = (hasVideo);//some tv channels dont have an audio stream
		
				newchannel.Frequency = info.freq;
				newchannel.ServiceName  = info.service_name;
				newchannel.ServiceProvider  = info.service_provider_name;
				newchannel.IsScrambled  = info.scrambled;
				newchannel.NetworkID         = info.networkID;
				newchannel.TransportStreamID         = info.transportStreamID;
				newchannel.ProgramNumber          = info.serviceID;
				newchannel.FEC     = info.fec;
				newchannel.Polarity = currentTuningObject.Polarity;
				newchannel.Modulation = currentTuningObject.Modulation;
				newchannel.Symbolrate = currentTuningObject.Symbolrate;
				newchannel.ServiceType=1;//tv
				newchannel.AudioPid=currentTuningObject.AudioPid;
				newchannel.AudioLanguage=currentTuningObject.AudioLanguage;
				newchannel.VideoPid=currentTuningObject.VideoPid;
				newchannel.TeletextPid=currentTuningObject.TeletextPid;
				newchannel.Bandwidth=currentTuningObject.Bandwidth;
				newchannel.PMTPid=info.network_pmt_PID;
				newchannel.Audio1=currentTuningObject.Audio1;
				newchannel.Audio2=currentTuningObject.Audio2;
				newchannel.Audio3=currentTuningObject.Audio3;
				newchannel.AC3Pid=currentTuningObject.AC3Pid;
				newchannel.PCRPid=info.pcr_pid;
				newchannel.AudioLanguage1=currentTuningObject.AudioLanguage1;
				newchannel.AudioLanguage2=currentTuningObject.AudioLanguage2;
				newchannel.AudioLanguage3=currentTuningObject.AudioLanguage3;
				newchannel.DiSEqC=currentTuningObject.DiSEqC;
				newchannel.LNBFrequency=currentTuningObject.LNBFrequency;
				newchannel.LNBKHz=currentTuningObject.LNBKHz;
				newchannel.PhysicalChannel=currentTuningObject.PhysicalChannel;
				newchannel.MinorChannel=info.minorChannel;
				newchannel.MajorChannel=info.majorChannel;
				newchannel.HasEITPresentFollow=info.eitPreFollow;
				newchannel.HasEITSchedule=info.eitSchedule;


				if (info.serviceType==1)//tv
				{
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0} is a tv channel",newchannel.ServiceName);
					//check if this channel already exists in the tv database
					bool isNewChannel=true;
					TVChannel tvChan = new TVChannel();
					tvChan.Name=newchannel.ServiceName;

					int channelId=-1;
					foreach (TVChannel tvchan in tvChannels)
					{
						if (tvchan.Name.Equals(newchannel.ServiceName))
						{
							if (TVDatabase.DoesChannelExist(tvchan.ID, newchannel.TransportStreamID, newchannel.NetworkID))
							{
								//yes already exists
								tvChan=tvchan;
								isNewChannel=false;
								channelId=tvchan.ID;
								break;
							}
						}
					}

					tvChan.Scrambled=newchannel.IsScrambled;
					if (isNewChannel)
					{
						//then add a new channel to the database
						tvChan.Number=TVDatabase.FindFreeTvChannelNumber(newchannel.ProgramNumber);
						tvChan.Sort=newchannel.ProgramNumber;
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: create new tv channel for {0}:{1}",newchannel.ServiceName,tvChan.Number);
						int id=TVDatabase.AddChannel(tvChan);
						channelId=id;
						newChannels++;
					}
					else
					{
						TVDatabase.UpdateChannel(tvChan,tvChan.Sort);
						updatedChannels++;
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0}:{1} already exists",newchannel.ServiceName,tvChan.Number);
					}
				
					if (Network() == NetworkType.DVBT)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBT card:{2}",newchannel.ServiceName,channelId,ID);
						TVDatabase.MapDVBTChannel(newchannel.ServiceName,
							newchannel.ServiceProvider,
							channelId, 
							newchannel.Frequency, 
							newchannel.NetworkID,
							newchannel.TransportStreamID,
							newchannel.ProgramNumber,
							currentTuningObject.AudioPid,
							currentTuningObject.VideoPid, 
							currentTuningObject.TeletextPid,
							newchannel.PMTPid,
							newchannel.Bandwidth,
							newchannel.Audio1,newchannel.Audio2,newchannel.Audio3,newchannel.AC3Pid,newchannel.PCRPid ,
							newchannel.AudioLanguage,newchannel.AudioLanguage1,newchannel.AudioLanguage2,newchannel.AudioLanguage3,
							newchannel.HasEITPresentFollow , newchannel.HasEITSchedule);
					}
					if (Network() == NetworkType.DVBC)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
						TVDatabase.MapDVBCChannel(newchannel.ServiceName,
							newchannel.ServiceProvider,
							channelId, 
							newchannel.Frequency, 
							newchannel.Symbolrate,
							newchannel.FEC,
							newchannel.Modulation,
							newchannel.NetworkID,
							newchannel.TransportStreamID,
							newchannel.ProgramNumber,
							currentTuningObject.AudioPid,
							currentTuningObject.VideoPid, 
							currentTuningObject.TeletextPid,
							newchannel.PMTPid,
							newchannel.Audio1,newchannel.Audio2,newchannel.Audio3,newchannel.AC3Pid,newchannel.PCRPid ,
							newchannel.AudioLanguage,newchannel.AudioLanguage1,newchannel.AudioLanguage2,newchannel.AudioLanguage3,
							newchannel.HasEITPresentFollow , newchannel.HasEITSchedule);

					}
					if (Network() == NetworkType.ATSC)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to ATSC card:{2}",newchannel.ServiceName,channelId,ID);
						TVDatabase.MapATSCChannel(newchannel.ServiceName,
							newchannel.PhysicalChannel, 
							newchannel.MinorChannel,  
							newchannel.MajorChannel, 
							newchannel.ServiceProvider,
							channelId, 
							newchannel.Frequency, 
							newchannel.Symbolrate,
							newchannel.FEC,
							newchannel.Modulation,
							newchannel.NetworkID,
							newchannel.TransportStreamID,
							newchannel.ProgramNumber,
							currentTuningObject.AudioPid,
							currentTuningObject.VideoPid, 
							currentTuningObject.TeletextPid,
							newchannel.PMTPid,
							newchannel.Audio1,newchannel.Audio2,newchannel.Audio3,newchannel.AC3Pid,newchannel.PCRPid ,
							newchannel.AudioLanguage,newchannel.AudioLanguage1,newchannel.AudioLanguage2,newchannel.AudioLanguage3,
							newchannel.HasEITPresentFollow , newchannel.HasEITSchedule);

					}

					if (Network() == NetworkType.DVBS)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
						newchannel.ID=channelId;
						TVDatabase.AddSatChannel(newchannel);
					}
					TVDatabase.MapChannelToCard(channelId,ID);

					
					TVGroup group = new TVGroup();
					if (info.scrambled)
					{
						group.GroupName="Scrambled";
					}
					else
					{
						group.GroupName="Unscrambled";
					}
					int groupid=TVDatabase.AddGroup(group);
					group.ID=groupid;
					TVChannel tvTmp=new TVChannel();
					tvTmp.Name=newchannel.ServiceName;
					tvTmp.Number=tvChan.Number;
					tvTmp.ID=channelId;
					TVDatabase.MapChannelToGroup(group,tvTmp);

					//make group for service provider
					group = new TVGroup();
					group.GroupName=newchannel.ServiceProvider;
					groupid=TVDatabase.AddGroup(group);
					group.ID=groupid;
					tvTmp=new TVChannel();
					tvTmp.Name=newchannel.ServiceName;
					tvTmp.Number=tvChan.Number;
					tvTmp.ID=channelId;
					TVDatabase.MapChannelToGroup(group,tvTmp);

				}
				else if (info.serviceType==2) //radio
				{
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0} is a radio channel",newchannel.ServiceName);
					//check if this channel already exists in the radio database
					bool isNewChannel=true;
					int channelId=-1;
					ArrayList radioStations = new ArrayList();
					RadioDatabase.GetStations(ref radioStations);
					foreach (RadioStation station in radioStations)
					{
						if (station.Name.Equals(newchannel.ServiceName))
						{
							//yes already exists
							isNewChannel=false;
							channelId=station.ID;
							station.Scrambled=info.scrambled;
							RadioDatabase.UpdateStation(station);
							break;
						}
					}

					//if the tv channel found is not yet in the tv database
					if (isNewChannel)
					{
						//then add a new channel to the database
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: create new radio channel for {0}",newchannel.ServiceName);
						RadioStation station = new RadioStation();
						station.Name=newchannel.ServiceName;
						station.Channel=newchannel.ProgramNumber;
						station.Frequency=newchannel.Frequency;
						station.Scrambled=info.scrambled;
						int id=RadioDatabase.AddStation(ref station);
						channelId=id;
						newRadioChannels++;
					}
					else
					{
						updatedRadioChannels++;
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: channel {0} already exists in tv database",newchannel.ServiceName);
					}

					if (Network() == NetworkType.DVBT)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBT card:{2}",newchannel.ServiceName,channelId,ID);
						RadioDatabase.MapDVBTChannel(newchannel.ServiceName,newchannel.ServiceProvider,channelId, newchannel.Frequency, newchannel.NetworkID,newchannel.TransportStreamID,newchannel.ProgramNumber,currentTuningObject.AudioPid,newchannel.PMTPid,newchannel.Bandwidth, newchannel.PCRPid);
					}
					if (Network() == NetworkType.DVBC)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
						RadioDatabase.MapDVBCChannel(newchannel.ServiceName,newchannel.ServiceProvider,channelId, newchannel.Frequency, newchannel.Symbolrate,newchannel.FEC,newchannel.Modulation,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.ProgramNumber,currentTuningObject.AudioPid,newchannel.PMTPid, newchannel.PCRPid);
					}
					if (Network() == NetworkType.ATSC)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
						RadioDatabase.MapATSCChannel(newchannel.ServiceName,newchannel.PhysicalChannel, 
							newchannel.MinorChannel,  
							newchannel.MajorChannel, newchannel.ServiceProvider,channelId, newchannel.Frequency, newchannel.Symbolrate,newchannel.FEC,newchannel.Modulation,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.ProgramNumber,currentTuningObject.AudioPid,newchannel.PMTPid, newchannel.PCRPid);
					}
					if (Network() == NetworkType.DVBS)
					{
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
						newchannel.ID=channelId;

						int scrambled=0;
						if (newchannel.IsScrambled) scrambled=1;
						RadioDatabase.MapDVBSChannel(newchannel.ID,newchannel.Frequency,newchannel.Symbolrate,
							newchannel.FEC,newchannel.LNBKHz,0,newchannel.ProgramNumber,
							0,newchannel.ServiceProvider,newchannel.ServiceName,
							0,0,newchannel.AudioPid,newchannel.VideoPid,newchannel.AC3Pid,
							0,0,0,0,scrambled,
							newchannel.Polarity,newchannel.LNBFrequency
							,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.PCRPid,
							newchannel.AudioLanguage,newchannel.AudioLanguage1,
							newchannel.AudioLanguage2,newchannel.AudioLanguage3,
							newchannel.ECMPid,newchannel.PMTPid);
					}
					RadioDatabase.MapChannelToCard(channelId,ID);
				}
			}//for (int i=0; i < transp.channels.Count;++i)
		}//public void StoreChannels(bool radio, bool tv)
		#endregion

		#endregion

		#region Radio
		public void TuneRadioChannel(RadioStation channel)
		{	
			if (m_NetworkProvider==null) return;

			try
			{	
				m_iPrevChannel		= m_iCurrentChannel;
				m_iCurrentChannel = channel.Channel;
				m_StartTime				= DateTime.Now;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() tune to radio station:{0}", channel.Name);

				
				int frequency=-1,ONID=-1,TSID=-1,SID=-1,  pmtPid=-1,pcrPid=-1;
				int audioPid=-1,bandwidth=8;
				string providerName;
				switch (m_NetworkType)
				{
					case NetworkType.ATSC: 
					{
						//get the ATSC tuning details from the tv database
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneChannel() get ATSC tuning details");
						int symbolrate=0,innerFec=0,modulation=0,physicalChannel=0;
						int minorChannel=0,majorChannel=0;
						RadioDatabase.GetATSCTuneRequest(channel.ID,out physicalChannel, out minorChannel,out majorChannel,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out pmtPid,out pcrPid);

						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz physicalChannel:{1} symbolrate:{2} innerFec:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8}", 
							frequency,physicalChannel,symbolrate, innerFec, modulation, ONID, TSID, SID,providerName);
						currentTuningObject=new DVBChannel();
						currentTuningObject.PhysicalChannel=physicalChannel;
						currentTuningObject.MinorChannel=minorChannel;
						currentTuningObject.MajorChannel=majorChannel;
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.PCRPid=pcrPid;
						currentTuningObject.ServiceName=channel.Name;
						SubmitTuneRequest(currentTuningObject);
					} break;
					
					case NetworkType.DVBC: 
					{
						//get the DVB-C tuning details from the tv database
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get DVBC tuning details");
						int symbolrate=0,innerFec=0,modulation=0;
						RadioDatabase.GetDVBCTuneRequest(channel.ID,out providerName,out frequency, out symbolrate, out innerFec, out modulation,out ONID, out TSID, out SID, out audioPid, out pmtPid, out pcrPid);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.Channel);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							frequency,symbolrate, innerFec, modulation, ONID, TSID, SID,providerName);

						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=frequency;
						currentTuningObject.Symbolrate=symbolrate;
						currentTuningObject.FEC=innerFec;
						currentTuningObject.Modulation=modulation;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.TeletextPid=0;
						currentTuningObject.PMTPid=pmtPid;
						
						currentTuningObject.PCRPid=pcrPid;
						currentTuningObject.ServiceName=channel.Name;
						SubmitTuneRequest(currentTuningObject);

					} break;

					case NetworkType.DVBS: 
					{					
						//get the DVB-S tuning details from the tv database
						//for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get DVBS tuning details");
						DVBChannel ch=new DVBChannel();
						if(RadioDatabase.GetDVBSTuneRequest(channel.ID,0,ref ch)==false)//only radio
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.Channel);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}", 
							ch.Frequency,ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID,ch.ProgramNumber,ch.ServiceProvider);


						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=ch.Frequency;
						currentTuningObject.Symbolrate=ch.Symbolrate;
						currentTuningObject.FEC=ch.FEC;
						currentTuningObject.Polarity=ch.Polarity;
						currentTuningObject.NetworkID=ch.NetworkID;
						currentTuningObject.TransportStreamID=ch.TransportStreamID;
						currentTuningObject.ProgramNumber=ch.ProgramNumber;
						currentTuningObject.AudioPid=ch.AudioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.TeletextPid=0;
						currentTuningObject.PMTPid=ch.PMTPid;
						currentTuningObject.ServiceName=channel.Name;
						currentTuningObject.DiSEqC=ch.DiSEqC;
						currentTuningObject.LNBFrequency=ch.LNBFrequency;
						currentTuningObject.LNBKHz=ch.LNBKHz;
						SubmitTuneRequest(currentTuningObject);

					} break;

					case NetworkType.DVBT: 
					{
						//get the DVB-T tuning details from the tv database
						//for DVB-T this is the frequency, ONID , TSID and SID
						//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() get DVBT tuning details");
						RadioDatabase.GetDVBTTuneRequest(channel.ID,out providerName,out frequency, out ONID, out TSID, out SID, out audioPid,out pmtPid, out bandwidth, out pcrPid);
						if (frequency<=0) 
						{
							Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:database invalid tuning details for channel:{0}", channel.Channel);
							return;
						}
						Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:  tuning details: frequency:{0} KHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency, ONID, TSID, SID,providerName);


						currentTuningObject=new DVBChannel();
						currentTuningObject.Frequency=frequency;
						currentTuningObject.NetworkID=ONID;
						currentTuningObject.TransportStreamID=TSID;
						currentTuningObject.ProgramNumber=SID;
						currentTuningObject.AudioPid=audioPid;
						currentTuningObject.VideoPid=0;
						currentTuningObject.TeletextPid=0;
						currentTuningObject.PMTPid=pmtPid;
						currentTuningObject.PCRPid=pcrPid;
						currentTuningObject.Bandwidth=bandwidth;
						currentTuningObject.ServiceName=channel.Name;
						SubmitTuneRequest(currentTuningObject);
					} break;
				}	//switch (m_NetworkType)
				//submit tune request to the tuner
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:TuneRadioChannel() done");

				if (m_streamDemuxer != null)
				{
					m_streamDemuxer.SetChannelData(currentTuningObject.AudioPid, currentTuningObject.VideoPid, currentTuningObject.TeletextPid, currentTuningObject.Audio3, currentTuningObject.ServiceName,currentTuningObject.PMTPid,currentTuningObject.ProgramNumber);
				}


				timeRetune=DateTime.Now;
				refreshPmtTable=true;
				
				if(m_pluginsEnabled==true)
					ExecTuner();
			}
			finally
			{
			}
		}//public void TuneRadioChannel(AnalogVideoStandard standard,int iChannel,int country)

		public void StartRadio(RadioStation station)
		{
			if (m_graphState != State.Radio) 
			{
				if (m_graphState!=State.Created)  return;
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
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRadio()");

				// add the preferred video/audio codecs
				AddPreferredCodecs(true,false);


				TuneRadioChannel(station);

				if ( currentTuningObject.PCRPid<=0 || currentTuningObject.PCRPid>=0x1fff)
				{
					SetupDemuxer(m_DemuxVideoPin,0,m_DemuxAudioPin,0,m_pinAC3Out,0);
					SetupDemuxerPin(m_pinMPG1Out,currentTuningObject.AudioPid,(int)MediaSampleContent.TransportPayload,true);
					SetupDemuxerPin(m_pinMPG1Out,currentTuningObject.PCRPid,(int)MediaSampleContent.TransportPacket,false);
					//setup demuxer MTS pin
					SetupMTSDemuxerPin();

					IMpeg2Demultiplexer mpeg2Demuxer= m_MPEG2Demultiplexer as IMpeg2Demultiplexer ;
					if (mpeg2Demuxer!=null)
					{
						Log.Write("DVBGraphBDA:MPEG2 demultiplexer PID mapping:");
						uint pid=0,sampletype=0;
						GetPidMap(m_pinMPG1Out,ref pid, ref sampletype);
						Log.Write("DVBGraphBDA:  Pin:mpg1 is mapped to pid:{0:x} content:{1}", pid, sampletype);
					}
					if(m_graphBuilder.Render(m_pinMPG1Out/*m_DemuxAudioPin*/) != 0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render audio out pin MPEG-2 Demultiplexer");
						return;
					}
				}
				else
				{
					//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRadio() render demux output pin");
					if(m_graphBuilder.Render(m_DemuxAudioPin) != 0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to render audio out pin MPEG-2 Demultiplexer");
						return;
					}
				}
				//get the IMediaControl interface of the graph
				if(m_mediaControl == null)
					m_mediaControl = m_graphBuilder as IMediaControl;

				//start the graph
				//Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: start graph");
				if (m_mediaControl!=null)
				{
					int hr=m_mediaControl.Run();
					if (hr<0)
					{
						Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED unable to start graph :0x{0:X}", hr);
					}
				}
				else
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA: FAILED cannot get IMediaControl");
				}

				graphRunning=true;
				m_graphState = State.Radio;
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Listening to radio..");
				return;
			}

			// tune to the correct channel

			TuneRadioChannel(station);
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:Listening to radio..");
		}
		
		public void TuneRadioFrequency(int frequency)
		{
		}
		#endregion
		
		#region plugins
		void ExecTuner()
		{
			DVBGraphSS2.TunerData tu=new DVBGraphSS2.TunerData();
			if (Network()==NetworkType.DVBS) tu.tt = (int) DVBGraphSS2.TunerType.ttSat;
			if (Network()==NetworkType.DVBC) tu.tt = (int) DVBGraphSS2.TunerType.ttCable;
			if (Network()==NetworkType.DVBT) tu.tt = (int) DVBGraphSS2.TunerType.ttTerrestrical;

			tu.Frequency=(UInt32)(currentTuningObject.Frequency);
			tu.SymbolRate=(UInt32)(currentTuningObject.Symbolrate);
			tu.AC3=0;
			tu.AudioPID=(UInt16)currentTuningObject.AudioPid;
			tu.DiseqC=(UInt16)currentTuningObject.DiSEqC;
			tu.PMT=(UInt16)currentTuningObject.PMTPid;
			tu.ECM_0=(UInt16)currentTuningObject.ECMPid;
			tu.FEC=(UInt16)6;
			tu.LNB=(UInt16)currentTuningObject.LNBFrequency;
			tu.LNBSelection=(UInt16)currentTuningObject.LNBKHz;
			tu.NetworkID=(UInt16)currentTuningObject.NetworkID;
			tu.PCRPID=(UInt16)currentTuningObject.PCRPid;
			tu.Polarity=(UInt16)currentTuningObject.Polarity;
			tu.SID=(UInt16)currentTuningObject.ProgramNumber;
			tu.TelePID=(UInt16)currentTuningObject.TeletextPid;
			tu.TransportStreamID=(UInt16)currentTuningObject.TransportStreamID;
			tu.VideoPID=(UInt16)currentTuningObject.VideoPid;
			tu.Reserved1=0;

			IntPtr data=Marshal.AllocHGlobal(50);
			Marshal.StructureToPtr(tu,data,true);

			bool flag=false;
			if(m_pluginsEnabled)
			{
				try
				{
					flag=EventMsg(999, data);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Capture,"Plugins-Exception: {0}",ex.Message);
				}
			}
			Marshal.FreeHGlobal(data);
		}
		#endregion

		private bool m_streamDemuxer_OnAudioFormatChanged(MediaPortal.TV.Recording.DVBDemuxer.AudioHeader audioFormat)
		{/*
			Log.Write("DVBGraphBDA:Audio format changed");
			Log.Write("DVBGraphBDA:  Bitrate:{0}",audioFormat.Bitrate);
			Log.Write("DVBGraphBDA:  Layer:{0}",audioFormat.Layer);
			Log.Write("DVBGraphBDA:  SamplingFreq:{0}",audioFormat.SamplingFreq);
			Log.Write("DVBGraphBDA:  Channel:{0}",audioFormat.Channel);
			Log.Write("DVBGraphBDA:  Bound:{0}",audioFormat.Bound);
			Log.Write("DVBGraphBDA:  Copyright:{0}",audioFormat.Copyright);
			Log.Write("DVBGraphBDA:  Emphasis:{0}",audioFormat.Emphasis);
			Log.Write("DVBGraphBDA:  ID:{0}",audioFormat.ID);
			Log.Write("DVBGraphBDA:  Mode:{0}",audioFormat.Mode);
			Log.Write("DVBGraphBDA:  ModeExtension:{0}",audioFormat.ModeExtension);
			Log.Write("DVBGraphBDA:  Original:{0}",audioFormat.Original);
			Log.Write("DVBGraphBDA:  PaddingBit:{0}",audioFormat.PaddingBit);
			Log.Write("DVBGraphBDA:  PrivateBit:{0}",audioFormat.PrivateBit);
			Log.Write("DVBGraphBDA:  ProtectionBit:{0}",audioFormat.ProtectionBit);
			Log.Write("DVBGraphBDA:  TimeLength:{0}",audioFormat.TimeLength);*/
			return true;
		}

		private void m_streamDemuxer_OnPMTIsChanged(byte[] pmtTable)
		{
			if (m_graphState==State.None||m_graphState==State.Created) return;
			if (pmtTable==null) return;
			if (pmtTable.Length<6) return;
			if (currentTuningObject.NetworkID<0 ||
				currentTuningObject.TransportStreamID<0 ||
				currentTuningObject.ProgramNumber<0) return;
			try
			{
				string pmtName=String.Format(@"database\pmt\pmt_{0}_{1}_{2}_{3}_{4}.dat",
					Utils.FilterFileName(currentTuningObject.ServiceName),
					currentTuningObject.NetworkID,
					currentTuningObject.TransportStreamID,
					currentTuningObject.ProgramNumber,
					(int)Network());
#if COMPARE_PMT
				if (System.IO.File.Exists(pmtName))
				{
					byte[] pmt=null;
					using (System.IO.FileStream stream = new System.IO.FileStream(pmtName,System.IO.FileMode.Open,System.IO.FileAccess.Read,System.IO.FileShare.None))
					{
						long len=stream.Length;
						if (len>6)
						{
							pmt = new byte[len];
							stream.Read(pmt,0,(int)len);
							stream.Close();
							if (pmt.Length==pmtTable.Length)
							{
								bool isSame=true;
								for (int i=0; i < pmt.Length;++i)
								{
									if (pmt[i]!=pmtTable[i]) isSame=false;
								}
								if (isSame) return;
							}
						}
					}
				}
#endif
				Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: OnPMTIsChanged:{0}", pmtName);				
				using (System.IO.FileStream stream = new System.IO.FileStream(pmtName,System.IO.FileMode.Create,System.IO.FileAccess.Write,System.IO.FileShare.None))
				{
					stream.Write(pmtTable,0,pmtTable.Length);
					stream.Close();
				}
				refreshPmtTable=true;
				m_epgGrabber.GrabEPG(currentTuningObject.HasEITSchedule==true);
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"ERROR: exception while creating pmt {0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
		}

		void SetPids()
		{
#if HW_PID_FILTERING
			string pidsText=String.Empty;
			ArrayList pids = new ArrayList();
			pids.Add((ushort)0);
			pids.Add((ushort)1);
			pids.Add((ushort)17);
			pids.Add((ushort)18);
			pids.Add((ushort)0xd3);
			pids.Add((ushort)0xd2);
			//if (currentTuningObject.VideoPid>0) pids.Add((ushort)currentTuningObject.VideoPid);
			if (currentTuningObject.AudioPid>0) pids.Add((ushort)currentTuningObject.AudioPid);
			if (currentTuningObject.AC3Pid>0) pids.Add((ushort)currentTuningObject.AC3Pid);
			if (currentTuningObject.PMTPid>0) pids.Add((ushort)currentTuningObject.PMTPid);
			//if (currentTuningObject.TeletextPid>0) pids.Add((ushort)currentTuningObject.TeletextPid);
			if (currentTuningObject.PCRPid>0) pids.Add((ushort)currentTuningObject.PCRPid);
			if (currentTuningObject.ECMPid>0) pids.Add((ushort)currentTuningObject.ECMPid);
			for (int i=0; i < pids.Count;++i)
				pidsText+=String.Format("{0:X},", (ushort)pids[i]);

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:SetPIDS to:{0}", pidsText);
			VideoCaptureProperties props = new VideoCaptureProperties(m_TunerDevice);
			props.SetPIDS(Network()==NetworkType.DVBC,
										Network()==NetworkType.DVBT,
										Network()==NetworkType.DVBS,
										Network()==NetworkType.ATSC,
										pids);


#endif
		}
	}//public class DVBGraphBDA 
}//namespace MediaPortal.TV.Recording
//end of file
#endif