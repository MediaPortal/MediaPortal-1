using System;
using Microsoft.Win32;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
using DShowNET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Player;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für DVBGraphSS2.
	/// </summary>
	/// 
	public class DVBGraphSS2 : IGraph, ISampleGrabberCB
	
	{

	// iids 0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b
	private static Guid IID_IB2C2AVCTRL2 = new Guid( 0x9c0563ce, 0x2ef7, 0x4568, 0xa2, 0x97, 0x88, 0xc7, 0xbb, 0x82, 0x40, 0x75 );
	private static Guid CLSID_B2C2Adapter = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x0, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);	
	private static Guid CLSID_StreamBufferSink = new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
	private static Guid CLSID_Mpeg2VideoStreamAnalyzer= new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
	private static Guid CLSID_StreamBufferConfig = new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);
	private static Guid CLSID_Mpeg2Data = new Guid( 0xC666E115, 0xBB62, 0x4027, 0xA1, 0x13, 0x82, 0xD6, 0x43, 0xFE, 0x2D, 0x99);
	private static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid( 0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
	private static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid( 0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
	private static Guid ColorSpaceConverter = new Guid("CC58E280-8AA1-11D1-B3F1-00AA003761C5");
	//
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
		  0x02, 0x00,             // channels		  = 2
		  0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
		  0x00, 0xEE, 0x02, 0x00, // nAvgBytesPerSec  = 0x00007d00=192000
		  0x04, 0x00,             // nBlockAlign      = 4 (channels*(bitspersample/8))
		  0x10, 0x00,             // wBitsPerSample   = 0
		  0x00, 0x00,             // extra size       = 0x0000 = 0 bytes
		} ;
		//

		#region AVControl
	[ComVisible(true), ComImport,
	Guid("9C0563CE-2EF7-4568-A297-88C7BB824075"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IB2C2MPEG2AVCtrl
	{
				// Argument 1: Audio PID
				// Argument 2: Video PID

		[PreserveSig]
		int SetAudioVideoPIDs (
			int pida,
			int pidb
		);
	};
		#endregion
		#region AVControl2
	// setup interfaces
	[ComVisible(true), ComImport,
	Guid("295950B0-696D-4a04-9EE3-C031A0BFBEDE"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
	public interface IB2C2MPEG2AVCtrl2 : IB2C2MPEG2AVCtrl
	{
		[PreserveSig]
		int SetCallbackForVideoMode (
			[MarshalAs(UnmanagedType.FunctionPtr)] VideoInfoCallback vInfo
		);

		[PreserveSig]
		int DeleteAudioVideoPIDs(  
			int pida,
			int pidv
		);
		[PreserveSig]
		int GetAudioVideoState (
			[Out] out int a,
			[Out] out int b,
			[Out] out int c,
			[Out] out int d,
			[Out] out int e,
			[Out] out int f
		);
	};
		#endregion
		#region DataControl
		[ComVisible(true), ComImport,
			Guid("7F35C560-08B9-11d5-A469-00D0D7B2C2D7"),
			InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
			public interface IB2C2MPEG2DataCtrl		
		{


			// Transport Stream methods
			[PreserveSig]
			int GetMaxPIDCount (
				[Out] out int pidCount
				);

			//this function is obselete, please use IB2C2MPEG2DataCtrl2's AddPIDsToPin function
			[PreserveSig]
			int AddPIDs (
				int count, 
				[In] ref int[] pidArray
				);

			//this function is obselete, please use IB2C2MPEG2DataCtrl2's DeletePIDsFromPin function
			[PreserveSig]
			int DeletePIDs (
				int count, 
				[In] ref int[] pidArray
				);

			// IP methods
			[PreserveSig]
			int GetMaxIpPIDCount (
				[Out] out int maxIpPidCount
				);

			[PreserveSig]
			int AddIpPIDs (
				int count, 
				[In] ref int[] ipPids
				) ;

			[PreserveSig]
			int DeleteIpPIDs (
				int count, 
				[In] ref int[] ipPids
				) ;

			[PreserveSig]
			int GetIpPIDs (
				[Out] out int count, 
				[Out] out int[]  ipPids
				);

			// All protocols

			[PreserveSig]
			int PurgeGlobalPIDs ();

			[PreserveSig]
			int GetMaxGlobalPIDCount ();

			[PreserveSig]
			int GetGlobalPIDs (
				[Out] out int count ,
				[Out] out int[] globalPids
				);


			[PreserveSig]
			int ResetDataReceptionStats ();

			[PreserveSig]
			int GetDataReceptionStats (
				[Out] out int ipQuality , 
				[Out] out int tsQuality 
				);

		};
		#endregion // do NOT use data control interface !!!
		#region DataControl2
		[ComVisible(true), ComImport,
			Guid("B0666B7C-8C7D-4c20-BB9B-4A7FE0F313A8"),
			InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
			public interface IB2C2MPEG2DataCtrl2 : IB2C2MPEG2DataCtrl	
		{
			[PreserveSig]
			int AddPIDsToPin (
				ref int count , 
				[In, MarshalAs(UnmanagedType.LPArray)] int[] pidsArray, 
				int dataPin
			);

			[PreserveSig]
			int DeletePIDsFromPin (
				int count,
				[In, MarshalAs(UnmanagedType.LPArray, SizeConst=39)] int[] pidsArray, 
				int dataPin
			);
		};
		#endregion// do NOT use data control interface !!!
		#region DataControl3
		[ComVisible(true), ComImport,
			Guid("E2857B5B-84E7-48b7-B842-4EF5E175F315"),
			InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
			public interface IB2C2MPEG2DataCtrl3 : IB2C2MPEG2DataCtrl2	
		{
			[PreserveSig]
			int AddTsPIDs (
				int count, 
				[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=39)] int[] pids
				) ;
			[PreserveSig]
			int DeleteTsPIDs (
				int count, 
				[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=39)] int[] pids
				) ;

			[PreserveSig]
			int GetTsState (
				ref Int32 plOpen,
				ref Int32 plRunning,
				ref Int32 plCount,
				ref Int32[] plPIDArray
				) ;

			[PreserveSig]
			int GetIpState (										
				[Out] out int plOpen,
				[Out] out int plRunning,
				[Out] out int plCount,
				[Out] out int[] plPIDArray
				);
			
			[PreserveSig]
			int GetReceivedDataIp (
				IntPtr ptrA,IntPtr ptrB
				);

			[PreserveSig]
			int AddMulticastMacAddress (
				IntPtr  pMacAddrList
				);

			[PreserveSig]
			int GetMulticastMacAddressList (
				IntPtr  pMacAddrList
				);

			[PreserveSig]
			int DeleteMulticastMacAddress (
				IntPtr  pMacAddrList
				);

			[PreserveSig]
			int SetUnicastMacAddress (
				IntPtr  pMacAddr
			);

			[PreserveSig]
			int GetUnicastMacAddress (
				IntPtr pMacAddr
			);

			[PreserveSig]
			int RestoreUnicastMacAddress ();
		};
		#endregion// do NOT use data control interface !!!
		#region TunerControl
		//
		// tuner follows
		//
		[ComVisible(true), ComImport,
			Guid("D875D4A9-0749-4fe8-ADB9-CC13F9B3DD45"),
			InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
			public interface IB2C2MPEG2TunerCtrl
		{
			// Satellite, Cable, Terrestrial (ATSC and DVB)

			[PreserveSig]
			int SetFrequency (
				int frequency	
				) ;

			// Satellite, Cable

			[PreserveSig]
			int SetSymbolRate (
				int symbolRate
				) ;

			// Satellite only

			[PreserveSig]
			int SetLnbFrequency (
				int lnbFrequency
				);

			[PreserveSig]
			int SetFec (
				int fec
				) ;

			[PreserveSig]
			int SetPolarity (
				int polarity
				) ;

			[PreserveSig]
			int SetLnbKHz (
				int lnbKHZ
				) ;
	
			[PreserveSig]
			int SetDiseqc (
				int diseqc
				) ;

			// Cable only

			[PreserveSig]
			int SetModulation (
				int modulation
				) ;
	
			// All tuners

			[PreserveSig]
			int Initialize () ;

			[PreserveSig]
			int SetTunerStatus () ;

			[PreserveSig]
			int CheckLock () ;

			[PreserveSig]
			int GetTunerCapabilities (
				IntPtr tunerCaps, 
				int count
			) ;

			// Terrestrial (ATSC)

			[PreserveSig]
			int GetFrequency (
				[Out] out int freq
				) ;

			[PreserveSig]
			int GetSymbolRate (
				[Out] out int symbRate
				) ;

			[PreserveSig]
			int GetModulation (
				[Out] out int modulation
				) ;

			[PreserveSig]
			int GetSignalStrength (
				[Out] out int signalStrength
				) ;

			[PreserveSig]
			int GetSignalLevel (
				[Out] out float signalLevel
				) ;

			[PreserveSig]
			int GetSNR (
				[Out] out float SNR
				) ;

			[PreserveSig]
			int GetPreErrorCorrectionBER (
				[Out] out float ber, 
				bool flag
				) ;

			[PreserveSig]
			int GetUncorrectedBlocks (
				[Out] out int uncorrectedBlocks
				) ;

			[PreserveSig]
			int GetTotalBlocks (
				[Out] out int correctedBlocks
				) ;

			[PreserveSig]
			int GetChannel (
				[Out] out int  channel
				) ;

			[PreserveSig]
			int SetChannel (
				int channel
				) ;
		};
		#endregion
		#region TunerControl2
		[ComVisible(true), ComImport,
			Guid("CD900832-50DF-4f8f-882D-1C358F90B3F2"),
			InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
			public interface IB2C2MPEG2TunerCtrl2 : IB2C2MPEG2TunerCtrl
		{
			int SetTunerStatusEx (
				int count
				);

			int SetFrequencyKHz (
				long freqKHZ	
				);

			// Terrestrial DVB only

			int SetGuardInterval (
				int interval
				);

			int GetGuardInterval (
				[Out] out int interval
				);

			int GetFec (
				[Out] out int plFec
				);

			int GetPolarity (
										
				[Out] out int plPolarity
				);

			int GetDiseqc (
									  
				[Out] out int plDiseqc
				);

			int GetLnbKHz (
				[Out] out int plLnbKHz
				);

			int GetLnbFrequency (
				[Out] out int plFrequencyMHz
				);

			int GetCorrectedBlocks (
				[Out] out int plCorrectedBlocks
				);

			int GetSignalQuality (
				[Out] out int pdwSignalQuality
				);
		};
		#endregion
		//
		// end interfaces
		protected enum State
		{ 
			None,
			Created,
			TimeShifting,
			Recording,
			Viewing
		};
		public struct TunerData
		{
			public byte TunerType;
			public Int32 Frequency;
			public Int32 SymbolRate;
			public Int16 LNB;           //LNB Frequency, e.g. 9750, 10600
			public Int16 PMT;           //PMT Pid
			public Int16 ECM;           //= 0 if unencrypted
			public byte Reserved1;
			public byte AC3;           //= 1 if audio PID = AC3 private stream
			//= 0 otherwise
			public Int16 FEC;           //1 = 1/2, 2 = 2/3, 3 = 3/4,
			//4 = 5/6, 5 = 7/8, 6 = Auto
			public Int16 Reserved2;
			public Int16 Polarity;      //0 = H, 1 = V
			//or Modulation or GuardInterval
			public Int16 Reserved3;
			public Int16 LNBSelection;  //0 = none, 1 = 22 khz
			public Int16 Reserved4;
			public Int16 DiseqC;        //0 = none, 1 = A, 2 = B,
			//3 = A/A, 4 = B/A, 5 = A/B, 6 = B/B
			public Int16 Reserved5;
			public Int16 AudioPID;
			public Int16 Reserved6;
			public Int16 VideoPID;
			public Int16 TransportStreamID; //from 2.0 R3 on (?), if scanned channel
			public Int16 TelePID;
			public Int16 NetworkID;         //from 2.0 R3 on (?), if scanned channel
			public Int16 SID;               //Service ID
			public Int16 PCRPID;

		} 
		//
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int GetGraph(DShowNET.IGraphBuilder graph,bool running,IntPtr callback);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern void StopGraph();
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern bool Execute([MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)] ref TunerData tunerdata,[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray)] int[] pids);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern bool SetAppHandle(IntPtr hnd,[MarshalAs(System.Runtime.InteropServices.UnmanagedType.FunctionPtr)] Delegate Callback);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern void PidCallback(IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int MenuItemClick(int ptr);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetMenuHandle(long menu);

		[DllImport("dvblib.dll", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetupDemuxer(IPin videoPin,IPin audioPin,int audio,int video);


		// the following are needed from dvblib.dll until all interfaces are in dshownet
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid, int tid, ref int secCount,int tabSec,int timeout);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SetPidToPin(DVBGraphSS2.IB2C2MPEG2DataCtrl3 dataCtrl,int pin,int pid);		

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DeleteAllPIDs(DVBGraphSS2.IB2C2MPEG2DataCtrl3 dataCtrl,int pin);
		
		// registry settings
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
		class StreamBufferConfig {}
		// we dont use this callback yet
		public delegate int VideoInfoCallback(IntPtr data);
		public delegate void RebuildFunc(IntPtr tunerData);
		//

		const int WS_CHILD = 0x40000000;
		const int WS_CLIPCHILDREN = 0x02000000;
		const int WS_CLIPSIBLINGS = 0x04000000;
		//
		// 
		protected bool			m_bOverlayVisible=false;
		protected DVBChannel	m_currentChannel=new DVBChannel();
		//
		protected Hashtable				m_eitPresentFollowTable=null;
		protected Hashtable				m_eitScheduleTable=null;
		protected System.Timers.Timer	m_eitTimer=null;
		//
		protected bool					m_firstTune=false;
		//
		protected IBaseFilter			m_sampleGrabber=null;
		protected ISampleGrabber		m_sampleInterface=null;

		protected IMpeg2Demultiplexer	m_demuxInterface=null;
		protected IBaseFilter			m_mpeg2Data=null; 
		protected IBasicVideo2			m_basicVideo=null;
		protected IVideoWindow			m_videoWindow=null;
		protected State                 m_graphState=State.None;
		protected IMediaControl			m_mediaControl=null;
		protected int                   m_cardID=-1;
		protected IBaseFilter			m_b2c2Adapter=null;
		protected IPin					m_videoPin=null;
		protected IPin					m_audioPin=null;
		protected IPin					m_data0=null;
		protected IPin					m_data1=null;
		protected IPin					m_data2=null;
		protected IPin					m_data3=null;
		// stream buffer sink filter
		protected IStreamBufferInitialize		m_streamBufferInit=null; 
		protected IStreamBufferConfigure		m_config=null;
		protected IStreamBufferRecordControl	m_recControl=null;
		protected IStreamBufferSink				m_sinkInterface=null;
		protected IBaseFilter					m_sinkFilter=null;
		protected IBaseFilter					m_mpeg2Analyzer=null;
		protected IBaseFilter					m_sourceFilter=null;
		protected IBaseFilter					m_smartTee=null;
		protected IBaseFilter					m_demux=null;
		protected IBaseFilter					m_csc=null;
		// def. the interfaces
		protected IB2C2MPEG2DataCtrl3	m_dataCtrl=null;
		protected IB2C2MPEG2TunerCtrl2	m_tunerCtrl=null;
		protected IB2C2MPEG2AVCtrl2		m_avCtrl=null;
        // player graph
		protected IGraphBuilder			m_sourceGraph=null;
		protected bool					m_timeShift=true;
		protected int					m_myCookie=0; // for the rot
		protected DateTime              m_StartTime=DateTime.Now;
		protected int					m_iChannelNr=-1;
		protected bool					m_channelFound=false;
		
		StreamBufferConfig				m_streamBufferConfig=null;
		protected VMR9Util				Vmr9=null; 
		protected string				m_filename="";
		protected DVBSections			m_sections=new DVBSections();
		protected int					m_currentTable=0;
		protected bool					m_grabEPG=false;
		protected bool					m_pluginsEnabled=false;
		public RebuildFunc				m_rebuildCB=null;
		//
		public DVBGraphSS2(int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel)
		{
			int epgInterval=0;
			m_eitPresentFollowTable=new Hashtable();
			m_eitScheduleTable=new Hashtable();
			m_rebuildCB=new RebuildFunc(RebuildTuner);

			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_grabEPG=xmlreader.GetValueAsBool("DVBSS2","grabEPG",false);
				epgInterval=xmlreader.GetValueAsInt("DVBSS2","grabInterval",750);
				m_pluginsEnabled=xmlreader.GetValueAsBool("DVBSS2","enablePlugins",false);
			}
			m_eitTimer=new System.Timers.Timer(epgInterval);
			m_eitTimer.AutoReset=false;
			m_eitTimer.Elapsed+=new System.Timers.ElapsedEventHandler(GetEITData);
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");

			}
			catch(Exception){}
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			// to vs7: don't give me stupid tips :)
		}
		public void RebuildTuner(IntPtr data)
		{
			int channelNr=m_iChannelNr;
			State state=m_graphState;

			TunerData td=new TunerData();
			td=(TunerData)Marshal.PtrToStructure(data,typeof(TunerData));
			m_currentChannel.Audio2=td.ECM;
			TVDatabase.UpdateSatChannel(m_currentChannel);
			//DeleteGraph();
			//CreateGraph();
			//if(state==State.TimeShifting)
			//	StartTimeShifting(0,AnalogVideoStandard.None,channelNr,m_filename);
			//if(state==State.Viewing)
			//	StartViewing(AnalogVideoStandard.None,channelNr,0);
			//m_graphState=state;
			
		}
		

		void ExecTuner()
		{
			TunerData tu=new TunerData();
			tu.PMT=(Int16)m_currentChannel.Audio3;
			tu.TunerType=1;
			tu.Frequency=(Int32)m_currentChannel.Frequency;
			tu.SymbolRate=(Int32)m_currentChannel.Symbolrate*1000;
			tu.AC3=0;
			tu.AudioPID=(Int16)m_currentChannel.AudioPid;
			tu.DiseqC=(Int16)m_currentChannel.DiSEqC;
			tu.ECM=(Int16)m_currentChannel.Audio2;
			tu.FEC=(Int16)6;
			tu.LNB=(Int16)m_currentChannel.LNBFrequency;
			tu.LNBSelection=(Int16)m_currentChannel.LNBKHz;
			tu.NetworkID=(Int16)m_currentChannel.NetworkID;
			tu.PCRPID=(Int16)m_currentChannel.PCRPid;
			tu.Polarity=(Int16)m_currentChannel.Polarity;
			tu.SID=(Int16)m_currentChannel.ProgramNumber;
			tu.TelePID=(Int16)m_currentChannel.TeletextPid;
			tu.TransportStreamID=(Int16)m_currentChannel.TransportStreamID;
			tu.VideoPID=(Int16)m_currentChannel.VideoPid;

			int[] pids=new int[1];

			if(m_pluginsEnabled)
				Execute(ref tu,pids);
		}
		public int BufferCB(double time,IntPtr data,int len)
		{
		
			int add=(int)data;
			int end=(add+len);
			for(int pointer=add;pointer<end;pointer+=188)
				PidCallback((IntPtr)pointer);
		
			// here write code to record raw ts or mp3 etc.
			//			byte[] b=new byte[len];
			//			Marshal.Copy(data,b,0,len);
			//			//m_fileWriter.Write(b,0,len);
			//			m_buffStream.Write(b,0,len);
			//			Log.Write("Written to file: {0} byte",len);
			

			//Log.Write("Plugins: address {1}: written {0} bytes",add,len);
			return 0;
		}

		public int SampleCB(double time,IMediaSample sample)
		{
			return 0;
		
		}
		/// <summary>
		/// Callback from Card. Sets an information struct with video settings
		/// </summary>

		public bool CreateGraph()
		{
			if (m_graphState != State.None) return false;
			// create graphs
			Vmr9 =new VMR9Util("mytv");

			m_sourceGraph=(IGraphBuilder)  Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) );
			
			if(m_pluginsEnabled)
				GetGraph(m_sourceGraph,false,IntPtr.Zero);
			
			int n=0;
			m_b2c2Adapter=null;
			// create filters & interfaces
			try
			{
				m_b2c2Adapter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( CLSID_B2C2Adapter, false ) );
				m_sinkFilter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( CLSID_StreamBufferSink, false ) );
				m_mpeg2Analyzer=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( CLSID_Mpeg2VideoStreamAnalyzer, true ) );
				m_sinkInterface=(IStreamBufferSink)m_sinkFilter;
				// for sections to be read
				m_mpeg2Data=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( CLSID_Mpeg2Data, true ) );
				// the demux is only needed to connect
				// to the mpeg2data sections filter
				// we dont need a muxer, demuxer since we get
				// already mpeg2 from the card
				m_demux=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.Mpeg2Demultiplexer, true ) );
				m_demuxInterface=(IMpeg2Demultiplexer) m_demux;
				m_sampleGrabber=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.SampleGrabber, true ) );
				m_sampleInterface=(ISampleGrabber) m_sampleGrabber;
				m_csc=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( ColorSpaceConverter, false ) );
				// all created ok here
			}
			
			catch(Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message);
			}
			if(m_b2c2Adapter==null)
				return false;
			try
			{
				n=m_sourceGraph.AddFilter(m_b2c2Adapter,"B2C2-Source");
				if(m_pluginsEnabled==true)
				{
					m_sourceGraph.AddFilter(m_sampleGrabber,"GrabberFilter");
					m_sourceGraph.AddFilter(m_demux,"Demuxer");
					//m_sourceGraph.AddFilter(m_csc,"InfinityTEE");
				}
				if(n!=0)
					return false;
				// get interfaces
				m_dataCtrl=(IB2C2MPEG2DataCtrl3) m_b2c2Adapter;
				if(m_dataCtrl==null)
					return false;
				m_tunerCtrl=(IB2C2MPEG2TunerCtrl2) m_b2c2Adapter;
				if(m_tunerCtrl==null)
					return false;
				m_avCtrl=(IB2C2MPEG2AVCtrl2) m_b2c2Adapter;
				if(m_avCtrl==null)
					return false;
				// init for tuner
				n=m_tunerCtrl.Initialize();
				if(n!=0)
					return false;
				// call checklock once, the return value dont matter
	
				n=m_tunerCtrl.CheckLock();
				bool b=false;
				b=SetVideoAudioPins();
				if(b==false)
					return false;

				if(m_pluginsEnabled==true)
				{
					AMMediaType mt=new AMMediaType();
					mt.majorType=DShowNET.MediaType.Stream;
					//mt.subType=DShowNET.MediaSubType.MPEG2Transport;	
					//m_sampleInterface.SetOneShot(true);
					m_sampleInterface.SetCallback(this,1);
					m_sampleInterface.SetMediaType(ref mt);
					m_sampleInterface.SetBufferSamples(false);
				}

			}
			catch(Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message);
				return false;
			}
			
			m_graphState=State.Created;
			return true;
		}
		void GetEITData(object obj,System.Timers.ElapsedEventArgs args)
		{
			ArrayList eitList=new ArrayList();

			if(m_currentTable<0x4E)
				m_currentTable=0x4E;

			eitList=m_sections.GetEITSchedule(m_currentTable,m_mpeg2Data);
			m_eitTimer.Stop();
			try
			{
				foreach(DVBSections.EITDescr eitData in eitList)
				{
					if(m_eitScheduleTable.ContainsKey(eitData))
					{
						m_eitScheduleTable.Remove(eitData);
						m_eitScheduleTable.Add(eitData,null);
					}
					else
						m_eitScheduleTable.Add(eitData,eitData);
				
					GC.Collect();
				}
			}
			catch(Exception ex)
			{string text=ex.Message;}
			// change table

			switch(m_currentTable)
			{
				case 0x50:
					m_currentTable=0x60;
					break;
				case 0x60:
					m_currentTable=0x4e;
					break;
				case 0x4E:
					m_currentTable=0x50;
					break;
			}
			//
			//
			if(m_grabEPG==true)
				m_eitTimer.Start();
		}
		//
		private bool Tune(int Frequency,int SymbolRate,int FEC,int POL,int LNBKhz,int Diseq,int AudioPID,int VideoPID,int LNBFreq,int ecmPID,int ttxtPID,int pmtPID)
		{
			int hr=0; // the result

			if(Frequency>13000)
				Frequency/=1000;

			hr = m_tunerCtrl.SetFrequency(Frequency);
			if (hr!=0)
			{
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetSymbolRate(SymbolRate);
			if (hr!=0)
			{
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetLnbFrequency(LNBFreq);
			if (hr!=0)
			{
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetFec(FEC);
			if (hr!=0)
			{
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetPolarity(POL);
			if (hr!=0)
			{
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetLnbKHz(LNBKhz);
			if (hr!=0)
			{
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetDiseqc(Diseq);
			if (hr!=0)
			{
				return false;	// *** FUNCTION EXIT POINT
			}
			
			hr = m_tunerCtrl.SetTunerStatus();
			if (hr!=0)	
				return false;	// *** FUNCTION EXIT POINT

			hr=m_tunerCtrl.CheckLock();
			if(hr!=0)
				return false;

			if(AudioPID!=-1 && VideoPID!=-1)
			{
				if(m_pluginsEnabled==false)
				{
					DeleteAllPIDs(m_dataCtrl,0);
					SetPidToPin(m_dataCtrl,0,18);
					SetPidToPin(m_dataCtrl,0,0);
					SetPidToPin(m_dataCtrl,0,1);
					hr = m_avCtrl.SetAudioVideoPIDs (AudioPID, VideoPID);
					if (hr!=0)
					{
						return false;	// *** FUNCTION EXIT POINT
					}
				}
				else
				{
					DeleteAllPIDs(m_dataCtrl,0);
					SetPidToPin(m_dataCtrl,0,18);
					SetPidToPin(m_dataCtrl,0,0);
					SetPidToPin(m_dataCtrl,0,1);
					SetPidToPin(m_dataCtrl,0,ecmPID);
					SetPidToPin(m_dataCtrl,0,ttxtPID);
					SetPidToPin(m_dataCtrl,0,AudioPID);
					SetPidToPin(m_dataCtrl,0,VideoPID);
					SetPidToPin(m_dataCtrl,0,pmtPID);
					hr = m_avCtrl.SetAudioVideoPIDs (0, 0);
				}


			}
			return true;
		}
		//
		/// <summary>
		/// Overlay-Controlling
		/// </summary>

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
		/// <summary>
		/// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
		/// </summary>

		private void GUIGraphicsContext_OnVideoWindowChanged()
		{
			if (m_graphState!=State.Viewing && m_graphState!=State.TimeShifting) return;
			
			if (!Vmr9.UseVMR9inMYTV)
			{

				if (GUIGraphicsContext.Overlay==false)
				{
					if(m_graphState!=State.Viewing)
					{
						Overlay=false;
						return;
					}
				}
				else
				{
					Overlay=true;
				}
			}
			int iVideoWidth=0;
			int iVideoHeight=0;
			if (m_basicVideo!=null)
			{
				m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);	
			}
			if (Vmr9.IsVMR9Connected)
			{
				iVideoWidth=Vmr9.VideoWidth;
				iVideoHeight=Vmr9.VideoHeight;
			}
			
			if (GUIGraphicsContext.IsFullScreenVideo || GUIGraphicsContext.ShowBackground==false)
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

				if(m_basicVideo!=null)
				{
					m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
					m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
				}
				if(m_videoWindow!=null)
					m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
				DirectShowUtil.DebugWrite("DVBGraphSS2: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("DVBGraphSS2: source position:({0},{1})-({2},{3})",rSource.Left, rSource.Top, rSource.Right, rSource.Bottom);
				DirectShowUtil.DebugWrite("DVBGraphSS2: dest   position:({0},{1})-({2},{3})",rDest.Left, rDest.Top, rDest.Right, rDest.Bottom);
			}
			else
			{
				if(m_basicVideo!=null)
				{
					m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
					m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				}
				if(m_videoWindow!=null)
					m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				DirectShowUtil.DebugWrite("DVBGraphSS2: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("DVBGraphSS2: source position:({0},{1})-({2},{3})",0, 0, iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("DVBGraphSS2: dest   position:({0},{1})-({2},{3})",GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Right, GUIGraphicsContext.VideoWindow.Bottom);

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
			DirectShowUtil.DebugWrite("DVBGraphSS2:DeleteGraph()");
			
			m_iChannelNr=-1;
			CommitDataToEPG();
			m_sampleInterface.SetCallback(null,0);
			//m_fileWriter.Close();
			if(m_pluginsEnabled)
				StopGraph();

			StopRecording();
			StopTimeShifting();
			StopViewing();

			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9=null;
			}

			if (m_mediaControl != null)
			{
				m_mediaControl.Stop();
				m_mediaControl = null;
			}

			//DsROT.RemoveGraphFromRot(ref m_myCookie);
			
			m_myCookie=0;

			if(m_sampleGrabber!=null)
			{
				Marshal.ReleaseComObject(m_sampleGrabber);
				m_sampleGrabber=null;
			}	
			if(m_sampleInterface!=null)
			{
				Marshal.ReleaseComObject(m_sampleInterface);
				m_sampleInterface=null;
			}	
			if (m_videoWindow != null)
			{
				m_bOverlayVisible=false;
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
				m_videoWindow.put_Owner(IntPtr.Zero);
				m_videoWindow = null;
			}


			if (m_basicVideo != null)
				m_basicVideo = null;
      

			DsUtils.RemoveFilters(m_sourceGraph);
			//
			// release all interfaces and pins
			//
			if(m_demux!=null)
			{
				Marshal.ReleaseComObject(m_demux);
				m_demux=null;
			}			
			if(m_demuxInterface!=null)
			{
				Marshal.ReleaseComObject(m_demuxInterface);
				m_demuxInterface=null;
			}			
			if(m_mpeg2Data!=null)
			{
				Marshal.ReleaseComObject(m_mpeg2Data);
				m_mpeg2Data=null;
			}			
			if(m_streamBufferInit!=null)
			{
				Marshal.ReleaseComObject(m_streamBufferInit);
				m_streamBufferInit=null;
			}
			if(m_config!=null)
			{
				Marshal.ReleaseComObject(m_config);
				m_config=null;
			}
			if(m_recControl!=null)
			{
				Marshal.ReleaseComObject(m_recControl);
				m_recControl=null;
			}
			if(m_sinkInterface!=null)
			{
				Marshal.ReleaseComObject(m_sinkInterface);
				m_sinkInterface=null;
			}
			if(m_videoPin!=null)
			{
				Marshal.ReleaseComObject(m_videoPin);
				m_videoPin=null;
			}
			if(m_data0!=null)
			{
				Marshal.ReleaseComObject(m_data0);
				m_data0=null;
			}
			if(m_data1!=null)
			{
				Marshal.ReleaseComObject(m_data1);
				m_data1=null;
			}
			if(m_data2!=null)
			{
				Marshal.ReleaseComObject(m_data2);
				m_data2=null;
			}
			if(m_data3!=null)
			{
				Marshal.ReleaseComObject(m_data3);
				m_data3=null;
			}
			if(m_audioPin!=null)
			{
				Marshal.ReleaseComObject(m_audioPin);
				m_audioPin=null;
			}
			if(m_tunerCtrl!=null)
			{
				Marshal.ReleaseComObject(m_tunerCtrl);
				m_tunerCtrl=null;
			}
			if(m_sinkFilter!=null)
			{
				Marshal.ReleaseComObject(m_sinkFilter);
				m_sinkFilter=null;
			}
			if(m_mpeg2Analyzer!=null)
			{
				Marshal.ReleaseComObject(m_mpeg2Analyzer);
				m_mpeg2Analyzer=null;
			}
			if(m_sourceFilter!=null)
			{
				Marshal.ReleaseComObject(m_sourceFilter);
				m_sourceFilter=null;
			}
			if(m_avCtrl!=null)
			{
				Marshal.ReleaseComObject(m_avCtrl);
				m_avCtrl=null;
			}
			if(m_dataCtrl!=null)
			{
				Marshal.ReleaseComObject(m_dataCtrl);
				m_dataCtrl=null;
			}
			if(m_b2c2Adapter!=null)
			{
				Marshal.ReleaseComObject(m_b2c2Adapter);
				m_b2c2Adapter=null;
			}
			if (m_sourceGraph != null)
			{
				Marshal.ReleaseComObject(m_sourceGraph); 
				m_sourceGraph = null;
			}
			GUIGraphicsContext.form.Invalidate(true);
			GC.Collect();

			//add collected stuff into programs database

			m_graphState = State.None;
			return;		
		}
		//
		//
		void CommitDataToEPG()
		{
			foreach(DVBSections.EITDescr eit in m_eitScheduleTable.Keys)
			{
				long start=m_sections.GetStartDateFromEIT(eit);
				long end=m_sections.GetEndDateFromEIT(eit);
				ArrayList entrys=new ArrayList();
				TVDatabase.GetPrograms(start,end,ref entrys);
				if(entrys.Count==0)
				{
					m_sections.SetEITToDatabase(eit);
				}
			}
			
			m_eitScheduleTable.Clear();

		}
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
			if (strVideoCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_sourceGraph,strVideoCodec);
			if (strAudioCodec.Length>0) DirectShowUtil.AddFilterToGraph(m_sourceGraph,strAudioCodec);
			if (bAddFFDshow) DirectShowUtil.AddFilterToGraph(m_sourceGraph,"ffdshow raw video filter");
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
		/// 
		private bool SetVideoAudioPins()
		{
			int hr=0;
			PinInfo pInfo=new PinInfo();

			// video pin
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,0,out m_videoPin);
			if(hr!=0)
				return false;

			m_videoPin.QueryPinInfo(out pInfo);
			// audio pin
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,1,out m_audioPin);
			if(hr!=0)
				return false;

			if(m_videoPin==null || m_audioPin==null)
			{
				Log.Write("DVBGraphSS2: pins not found on adapter");
				return false;
			}
			m_audioPin.QueryPinInfo(out pInfo);

			// data pins
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,2,out m_data0);
			if(hr!=0)
				return false;
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,3,out m_data1);
			if(hr!=0)
				return false;
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,4,out m_data2);
			if(hr!=0)
				return false;
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,5,out m_data3);
			if(hr!=0)
				return false;


			return true;
		}
		//
		private bool CreateSinkSource(string fileName)
		{
			if(m_graphState!=State.Created)
				return false;
			int			hr=0;
			IPin		pinObj0=null;
			IPin		pinObj1=null;
			IPin		outPin=null;

			if(m_pluginsEnabled==true)
			{

				hr=m_sourceGraph.AddFilter(m_sinkFilter,"StreamBufferSink");
				hr=m_sourceGraph.AddFilter(m_mpeg2Analyzer,"Stream-Analyzer");

				DsROT.AddGraphToRot(m_sourceGraph,out m_myCookie);
				// setup sampleGrabber and demuxer
				IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
				IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	
				hr=m_sourceGraph.Connect(m_data0,samplePin);
				if(hr!=0)
					return false;

				samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);	
				hr=m_sourceGraph.Connect(demuxInPin,samplePin);
				if(hr!=0)
					return false;

				SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid);
			
				IPin dmOutVid=DirectShowUtil.FindPinNr(m_demux,PinDirection.Output,1);
				IPin dmOutAud=DirectShowUtil.FindPinNr(m_demux,PinDirection.Output,0);
				if(dmOutVid==null || dmOutAud==null)
					return false;

				pinObj0=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Input,0);
				if(pinObj0!=null)
				{
				
					hr=m_sourceGraph.Connect(dmOutVid,pinObj0);
					if(hr==0)
					{
						// render all out pins
						pinObj1=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Output,0);	
						hr=m_sourceGraph.Render(pinObj1);
						if(hr!=0)
							return false;
						hr=m_sourceGraph.Render(dmOutAud);
						if(hr!=0)
							return false;
					
						if(demuxInPin!=null)
							Marshal.ReleaseComObject(demuxInPin);
						if(samplePin!=null)
							Marshal.ReleaseComObject(samplePin);
						if(dmOutVid!=null)
							Marshal.ReleaseComObject(dmOutVid);
						if(dmOutAud!=null)
							Marshal.ReleaseComObject(dmOutAud);
						if(pinObj1!=null)
							Marshal.ReleaseComObject(pinObj1);
						if(pinObj0!=null)
							Marshal.ReleaseComObject(pinObj0);
	
						demuxInPin=null;
						samplePin=null;
						dmOutVid=null;
						dmOutAud=null;
						pinObj1=null;
						pinObj0=null;
					}
				} // render of sink is ready
			}
			if(m_pluginsEnabled==false)
			{
				hr=m_sourceGraph.AddFilter(m_sinkFilter,"StreamBufferSink");
				hr=m_sourceGraph.AddFilter(m_mpeg2Analyzer,"Stream-Analyzer");
				hr=m_sourceGraph.AddFilter(m_mpeg2Data,"Sections-Filter");
				hr=m_sourceGraph.AddFilter(m_demux,"Demuxer-Filter");

				pinObj0=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Input,0);
				if(pinObj0!=null)
				{
				
					hr=m_sourceGraph.Connect(m_videoPin,pinObj0);
					if(hr==0)
					{
						// render all out pins
						hr=-1;
						pinObj1=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Output,0);	
						hr=m_sourceGraph.Render(pinObj1);
						if(hr!=0)
							return false;
						hr=m_sourceGraph.Render(m_audioPin);
						if(hr!=0)
							return false;

					
						// setup data / ts stream
						// set output pin on demux
					
						m_demuxInterface=(IMpeg2Demultiplexer)m_demux;
						if(m_demuxInterface==null)
							return false;
						IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);
						IPin demuxOutPin=null;
						IPin m2dIn=DirectShowUtil.FindPinNr(m_mpeg2Data,PinDirection.Input,0);
						AMMediaType mt=new AMMediaType();
						mt.majorType=MEDIATYPE_MPEG2_SECTIONS;
						mt.subType=MEDIASUBTYPE_MPEG2_DATA;
						hr=m_demuxInterface.CreateOutputPin(ref mt,"MPEG2DATA",out demuxOutPin);
						if(hr!=0)
							return false;
						hr=m_sourceGraph.Connect(m_data0,demuxInPin);
						if(hr!=0)
							return false;
						hr=m_sourceGraph.Connect(demuxOutPin,m2dIn);
						if(hr!=0)
							return false;
						//DsROT.AddGraphToRot(m_sourceGraph,out m_myCookie);
						if(demuxOutPin!=null)
							Marshal.ReleaseComObject(demuxOutPin);
						if(m2dIn!=null)
							Marshal.ReleaseComObject(m2dIn);
						if(demuxInPin!=null)
							Marshal.ReleaseComObject(demuxInPin);
						demuxOutPin=null;
						demuxInPin=null;
						m2dIn=null;
					

										
					}
				} // render of sink is ready
			}

			int ipos=fileName.LastIndexOf(@"\");
			string strDir=fileName.Substring(0,ipos);

			m_streamBufferConfig=new StreamBufferConfig();
			m_config = (IStreamBufferConfigure) m_streamBufferConfig;
			// setting the timeshift behaviors
			IntPtr HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_config;
			IntPtr subKey = IntPtr.Zero;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr=pTemp.SetHKEY(subKey);
			
			hr=m_config.SetDirectory(strDir);	
			if(hr!=0)
				return false;
			hr=m_config.SetBackingFileCount(6, 8);    //4-6 files
			if(hr!=0)
				return false;
			
			hr=m_config.SetBackingFileDuration( 300); // 60sec * 4 files= 4 mins
			if(hr!=0)
				return false;

			subKey = IntPtr.Zero;
      		HKEY = (IntPtr) unchecked ((int)0x80000002L);
			IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_sinkFilter;

			RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
			hr=pConfig.SetHKEY(subKey);
			// lock on the 'filename' file
			hr=m_sinkInterface.LockProfile(fileName);
			m_filename=fileName;
			if(hr!=0)
				return false;

			if(pinObj0!=null)
				Marshal.ReleaseComObject(pinObj0);
			if(pinObj1!=null)
				Marshal.ReleaseComObject(pinObj1);
			if(outPin!=null)
				Marshal.ReleaseComObject(outPin);

			return true;
		}
		//
		bool DeleteDataPids(int pin)
		{
			bool res=false;

			res=DeleteAllPIDs(m_dataCtrl,0);

			return res;

		}
		int AddDataPidsToPin(int pin,int pid)
		{
			int res=0;
			
			res=SetPidToPin(m_dataCtrl,pin,pid);
			
			return res;
		}
		//
		public bool StartTimeShifting(int country,AnalogVideoStandard standard,int channel,string fileName)
		{

			if(m_graphState!=State.Created)
				return false;
			int hr=0;

			TuneChannel(standard,channel,-1);

			if(m_channelFound==false)
				return false;
			

			if(CreateSinkSource(fileName)==true)
			{
				m_mediaControl=(IMediaControl)m_sourceGraph;
				hr=m_mediaControl.Run();
				m_graphState = State.TimeShifting;
				if(m_pluginsEnabled==true)
					SetAppHandle(GUIGraphicsContext.form.Handle,m_rebuildCB);
			}
			else {m_graphState=State.Created;return false;}

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
			if (m_graphState != State.TimeShifting) return false;
			DirectShowUtil.DebugWrite("DVBGraphSS2:StopTimeShifting()");
			m_mediaControl.Stop();
			CommitDataToEPG();
			m_graphState = State.Created;
			DeleteGraph();
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
		public bool StartRecording(int country,AnalogVideoStandard standard,int channel, ref string strFilename, bool bContentRecording, DateTime timeProgStart)
		{		
			if (m_graphState != State.TimeShifting || m_pluginsEnabled==true) return false;

			IntPtr recorderObj;
			if (m_sinkFilter==null) 
			{
				return false;
			}

			uint iRecordingType=0;
			if (bContentRecording) iRecordingType=0;
			else iRecordingType=1;										
		 
			int hr=m_sinkInterface.CreateRecorder(strFilename, iRecordingType, out recorderObj);
			if (hr!=0) 
			{
				return false;
			}
			object objRecord=Marshal.GetObjectForIUnknown(recorderObj);
			if (objRecord==null) 
			{
				return false;
			}
      
			Marshal.Release(recorderObj);

			m_recControl=objRecord as IStreamBufferRecordControl;
			if (m_recControl==null) 
			{
				return false;
			}
			
			long lStartTime=0;

			// if we're making a reference recording
			// then record all content from the past as well
			if (!bContentRecording)
			{
				// so set the startttime...
				uint uiSecondsPerFile;
				uint uiMinFiles, uiMaxFiles;
				m_config.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
				m_config.GetBackingFileDuration(out uiSecondsPerFile);
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
      
				TimeSpan tsMaxTimeBack=DateTime.Now-m_StartTime;
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
				if (lStartTime!=0)
				{
					// try recording from livepoint instead from the past
					lStartTime=0;
					hr=m_recControl.Start(ref lStartTime);
					if (hr!=0)
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
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
			if (m_recControl==null || m_graphState!=State.Recording)
				return ;
			
			int hr=m_recControl.Stop(0);
			if (hr!=0) 
			{
				return ;
			}
			if (m_recControl!=null) 
				Marshal.ReleaseComObject(m_recControl);
			
			m_recControl=null;

			m_graphState=State.TimeShifting;
			return ;

		}

		//
		//
		public void TuneChannel(AnalogVideoStandard standard,int channel, int country)
		{
			if(m_graphState==State.Recording)
				return;

			int channelID=TVDatabase.GetChannelId(channel);
			m_iChannelNr=channel;
			if(channelID!=-1)
			{
				
				DVBChannel ch=new DVBChannel();
				CommitDataToEPG();
				if(TVDatabase.GetSatChannel(channelID,1,ref ch)==false)//only television
				{
					m_channelFound=false;
					return;
				}

				if(m_pluginsEnabled==false && ch.IsScrambled==true)
				{
					m_channelFound=false;
					return;
				}

				//if(m_mediaControl!=null && m_graphState!=State.TimeShifting )
				//	m_mediaControl.Stop();

				
				m_channelFound=true;
				
				if(Tune(ch.Frequency,ch.Symbolrate,6,ch.Polarity,ch.LNBKHz,ch.DiSEqC,ch.AudioPid,ch.VideoPid,ch.LNBFrequency,ch.Audio2,ch.TeletextPid,ch.Audio3)==false)
				{
					m_channelFound=false;
					return;
				}
				m_currentChannel=ch;
				//
				//
				if(m_pluginsEnabled==false && m_grabEPG==true)
					m_eitTimer.Start();
				
				//if(m_mediaControl!=null && m_graphState!=State.TimeShifting )
				//	m_mediaControl.Run();


				if(m_pluginsEnabled==true)
					ExecTuner();

				m_StartTime=DateTime.Now;
			}
			
		}
		void SetDemux(int audioPid,int videoPid)
		{
			AMMediaType mpegVideoOut = new AMMediaType();
			mpegVideoOut.majorType = MediaType.Video;
			mpegVideoOut.subType = MediaSubType.MPEG2_Video;

			Size FrameSize=new Size(720,576);
			mpegVideoOut.unkPtr = IntPtr.Zero;
			mpegVideoOut.sampleSize = 0;
			mpegVideoOut.temporalCompression = false;
			mpegVideoOut.fixedSizeSamples = true;

			Mpeg2ProgramVideo=new byte[Mpeg2ProgramVideo.GetLength(0)];
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
			System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat,0,mpegAudioOut.formatPtr,mpegAudioOut.formatSize) ;
			
			IPin pinVideoOut,pinAudioOut;

			int hr=m_demuxInterface.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out pinVideoOut);
			if (hr!=0)
			{
				return;
			}

			hr=m_demuxInterface.CreateOutputPin(ref mpegAudioOut, "audio", out pinAudioOut);
			if (hr!=0)
			{
				return;
			}

			hr=SetupDemuxer(pinVideoOut,pinAudioOut,audioPid,videoPid);
			int a=0;
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
		/// Property indiciating if the graph supports timeshifting
		/// </summary>
		/// <returns>boolean indiciating if the graph supports timeshifting</returns>
		public bool SupportsTimeshifting()
		{
			if(m_pluginsEnabled==true)
				return false;
			else
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
		public bool StartViewing(AnalogVideoStandard standard,int channel, int country)
		{
			if (m_graphState != State.Created) return false;
			TuneChannel(standard,channel,country);
			int hr=0;
			
			if(m_channelFound==false)
				return false;
			
			AddPreferredCodecs();
			
			if(Vmr9.UseVMR9inMYTV)
			{
				Vmr9.AddVMR9(m_sourceGraph);
			}

			if(m_pluginsEnabled==false)
			{
				// render vid & aud
				hr=m_sourceGraph.Render(m_videoPin);
				if(hr!=0)
					return false;

				hr=m_sourceGraph.Render(m_audioPin);
				if(hr!=0)
					return false;
				hr=m_sourceGraph.AddFilter(m_mpeg2Data,"Sections-Filter");
				hr=m_sourceGraph.AddFilter(m_demux,"Demuxer-Filter");
				m_demuxInterface=(IMpeg2Demultiplexer)m_demux;
				if(m_demuxInterface==null)
					return false;
				IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);
				IPin demuxOutPin=null;
				IPin m2dIn=DirectShowUtil.FindPinNr(m_mpeg2Data,PinDirection.Input,0);
				AMMediaType mt=new AMMediaType();
				mt.majorType=MEDIATYPE_MPEG2_SECTIONS;
				mt.subType=MEDIASUBTYPE_MPEG2_DATA;
				hr=m_demuxInterface.CreateOutputPin(ref mt,"MPEG2DATA",out demuxOutPin);
				if(hr!=0)
					return false;
				hr=m_sourceGraph.Connect(m_data0,demuxInPin);
				if(hr!=0)
					return false;
				hr=m_sourceGraph.Connect(demuxOutPin,m2dIn);
				if(hr!=0)
					return false;
				//DsROT.AddGraphToRot(m_sourceGraph,out m_myCookie);
				if(demuxOutPin!=null)
					Marshal.ReleaseComObject(demuxOutPin);
				if(m2dIn!=null)
					Marshal.ReleaseComObject(m2dIn);
				if(demuxInPin!=null)
					Marshal.ReleaseComObject(demuxInPin);
				demuxOutPin=null;
				demuxInPin=null;
				m2dIn=null;

				int n=0;// 
			}
			else
			{
				IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
				IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	
				hr=m_sourceGraph.Connect(m_data0,samplePin);
				if(hr!=0)
					return false;
				samplePin=null;
				samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);			
				if(samplePin==null)
					return false;
				hr=m_sourceGraph.Connect(samplePin,demuxInPin);
				if(hr!=0)
					return false;

				SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid);
			
				IPin dmOutVid=DirectShowUtil.FindPinNr(m_demux,PinDirection.Output,1);
				IPin dmOutAud=DirectShowUtil.FindPinNr(m_demux,PinDirection.Output,0);
				if(dmOutVid==null || dmOutAud==null)
					return false;
				hr=m_sourceGraph.Render(dmOutVid);
				if(hr!=0)
					return false;
				hr=m_sourceGraph.Render(dmOutAud);
				if(hr!=0)
					return false;
				//
				//DsROT.AddGraphToRot(m_sourceGraph,out m_myCookie);
				if(demuxInPin!=null)
					Marshal.ReleaseComObject(demuxInPin);
				if(samplePin!=null)
					Marshal.ReleaseComObject(samplePin);
				if(dmOutVid!=null)
					Marshal.ReleaseComObject(dmOutVid);
				if(dmOutAud!=null)
					Marshal.ReleaseComObject(dmOutAud);
				//
			}

			//
			// mpeg2data and demux connection

			
			if(Vmr9.IsVMR9Connected==false && Vmr9.UseVMR9inMYTV==true)// fallback
			{
				if(Vmr9.VMR9Filter!=null)
					m_sourceGraph.RemoveFilter(Vmr9.VMR9Filter);
				Vmr9.RemoveVMR9();
				Vmr9.UseVMR9inMYTV=false;

			}

			m_mediaControl = (IMediaControl)m_sourceGraph;
			if (!Vmr9.UseVMR9inMYTV )
			{

				m_videoWindow = (IVideoWindow) m_sourceGraph as IVideoWindow;
				if (m_videoWindow==null)
				{
					Log.Write("DVBGraphSS2:FAILED:Unable to get IVideoWindow");
				}

				m_basicVideo = (IBasicVideo2)m_sourceGraph as IBasicVideo2;
				if (m_basicVideo==null)
				{
					Log.Write("DVBGraphSS2:FAILED:Unable to get IBasicVideo2");
				}
				hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:set Video window:0x{0:X}",hr);
				}
				hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:set Video window style:0x{0:X}",hr);
				}
      
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:put_Visible:0x{0:X}",hr);
				}
			}

			m_bOverlayVisible=true;
			GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			m_graphState = State.Viewing;
			GUIGraphicsContext_OnVideoWindowChanged();
			//
			

			m_mediaControl.Run();
			if(m_pluginsEnabled==true)
				SetAppHandle(GUIGraphicsContext.form.Handle,m_rebuildCB);

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
			if (m_graphState != State.Viewing) 
				return false;
			m_mediaControl.Stop();
			m_mediaControl=null;
			GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			DirectShowUtil.DebugWrite("DVBGraphSS2:StopViewing()");
			if(m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
			m_bOverlayVisible=false;
			CommitDataToEPG();

			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}

		//
		public bool ShouldRebuildGraph(int iChannel)
		{
			//if(m_graphState==State.TimeShifting)
			return true;

			//return false;
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
				return true;
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
		
		public void Process()
		{
		}
		
		public PropertyPageCollection PropertyPages()
		{
			return null;
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
				return NetworkType.DVBS;
		}
		public void Tune(object tuningObject)
		{
		}
		public void StoreChannels(bool radio, bool tv)
		{
		}
	}
}
