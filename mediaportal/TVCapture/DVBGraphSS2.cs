using System;
using Microsoft.Win32;
using System.Drawing;
using System.Runtime.InteropServices;
using DShowNET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using MediaPortal.TV.Database;


namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für DVBGraphSS2.
	/// </summary>
	/// 
	public class DVBGraphSS2 : IGraph
	
	{
		struct VideoInfo
		{
			public int	iHSize;			// video data horizontal size in pixels
			public int	iVSize;			// video data vertical size in pixels
			public byte	bAspectRatio;
			public byte	bFrameRate;
		};

	private System.Collections.Hashtable m_epgTable=new System.Collections.Hashtable(); 
	// iids 0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b
	private static Guid IID_IB2C2AVCTRL2 = new Guid( 0x9c0563ce, 0x2ef7, 0x4568, 0xa2, 0x97, 0x88, 0xc7, 0xbb, 0x82, 0x40, 0x75 );
	private static Guid CLSID_B2C2Adapter = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x0, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);	
	private static Guid CLSID_StreamBufferSink = new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
	private static Guid CLSID_Mpeg2VideoStreamAnalyzer= new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
	private static Guid CLSID_StreamBufferConfig = new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);
	private static Guid CLSID_Mpeg2Data = new Guid( 0xC666E115, 0xBB62, 0x4027, 0xA1, 0x13, 0x82, 0xD6, 0x43, 0xFE, 0x2D, 0x99);
	private static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid( 0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
	private static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid( 0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
	//
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
		//

		const int WS_CHILD = 0x40000000;
		const int WS_CLIPCHILDREN = 0x02000000;
		const int WS_CLIPSIBLINGS = 0x04000000;
		//
		// tuner needed
		VideoInfoCallback	m_videoCallback=new VideoInfoCallback(SetVideoInfo);
		static VideoInfo	m_mpegInfo=new VideoInfo();
		protected int		m_iFrequency=0;//12188; // lnb frequency
		protected int		m_iSymbolRate=0;//27500; // symbol rate
		protected int		m_iDiSeqC=0; // diseqc
		protected int		m_iLNBSelect=0; // lnb selection
		protected int		m_iFEC=0; // always set to auto for ss2
		protected int		m_iPolarisation=0; // polarisation 0 - h , 1 - v
		protected int		m_iLNB0=0; // ku band
		protected int		m_iLNB2=0; // 
		protected int		m_iLNBSwitch=0; //
		protected int		m_audioPid=0;
		protected int		m_videoPid=0;
		protected bool      m_bOverlayVisible=false;
		protected int		m_actualTab=0x50;
		protected int		m_videoRender=0;
		//


		//
		protected bool					m_firstTune=false;
		//
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
		// for recording set dump.ax filter containd with directx sdk
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
		
		StreamBufferConfig				m_streamBufferConfig=null;
		//
		public DVBGraphSS2(int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel)
		{
			m_graphState= State.None;
			m_firstTune=true;
			m_iFrequency=12188;
			m_iSymbolRate=27500; // symbol rate
			m_iDiSeqC=1; // diseqc
			m_iLNBSelect=1; // lnb selection
			m_iFEC=6; // always set to auto for ss2
			m_iPolarisation=0; // polarisation 0 - h , 1 - v
			m_iLNB0=9750; // ku band
			m_iLNB2=10600; // 
			m_iLNBSwitch=11700; //
			m_audioPid=104;
			m_videoPid=163;
			
			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_videoRender=xmlreader.GetValueAsInt("mytv","vmr9",0);
			}

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

		/// <summary>
		/// Callback from Card. Sets an information struct with video settings
		/// </summary>
		static int SetVideoInfo(IntPtr data)
		{
			m_mpegInfo=(VideoInfo)Marshal.PtrToStructure(data,typeof(VideoInfo));
			return 0;
		}

		public bool CreateGraph()
		{
			if (m_graphState != State.None) return false;
			// create graphs
			System.Threading.Thread.Sleep(350);
			m_sourceGraph=(IGraphBuilder)  Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) );

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


			}
			catch(Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message);
				return false;
			}
			
			m_graphState=State.Created;
			return true;
		}
		//
		private bool Tune(int Frequency,int SymbolRate,int FEC,int POL,int LNBKhz,int Diseq,int AudioPID,int VideoPID,int LNBFreq)
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
			if(AudioPID!=-1 && VideoPID!=-1)
			{
				// add pmt pid
				bool su=false;
				int res=0;
				su=DeleteDataPids(0);

				res=AddDataPidsToPin(0,18);

				//su=DeleteDataPids(0);
				hr = m_avCtrl.SetAudioVideoPIDs (AudioPID, VideoPID);
				if (hr!=0)
				{
					return false;	// *** FUNCTION EXIT POINT
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
			int iVideoWidth, iVideoHeight;
			m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);
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
				DirectShowUtil.DebugWrite("SWGraph: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("SWGraph: source position:({0},{1})-({2},{3})",rSource.Left, rSource.Top, rSource.Right, rSource.Bottom);
				DirectShowUtil.DebugWrite("SWGraph: dest   position:({0},{1})-({2},{3})",rDest.Left, rDest.Top, rDest.Right, rDest.Bottom);
			}
			else
			{
				m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
				m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				DirectShowUtil.DebugWrite("SWGraph: capture size:{0}x{1}",iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("SWGraph: source position:({0},{1})-({2},{3})",0, 0, iVideoWidth, iVideoHeight);
				DirectShowUtil.DebugWrite("SWGraph: dest   position:({0},{1})-({2},{3})",GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Right, GUIGraphicsContext.VideoWindow.Bottom);

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
			DirectShowUtil.DebugWrite("SWGraph:DeleteGraph()");
			StopRecording();
			StopViewing();
			if (m_mediaControl != null)
			{
				m_mediaControl.Stop();
				m_mediaControl = null;
			}

			//DsROT.RemoveGraphFromRot(ref m_myCookie);
			
			m_myCookie=0;

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

			m_graphState = State.None;
			return;		
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
				Log.Write("DVBSS2: pins not found on adapter");
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
		public bool StartTimeShifting(AnalogVideoStandard xxx,int channel,string fileName)
		{
			if(m_graphState!=State.Created)
				return false;
			int hr=0;
			TuneChannel(xxx,channel);
			if(CreateSinkSource(fileName)==true)
			{
				m_mediaControl=(IMediaControl)m_sourceGraph;
				hr=m_mediaControl.Run();
				m_graphState = State.TimeShifting;
			}
			else return false;

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
			DirectShowUtil.DebugWrite("DVBSS2:StopViewing()");
			m_mediaControl.Stop();
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
		public bool StartRecording(AnalogVideoStandard standard,int channel, ref string strFilename, bool bContentRecording, DateTime timeProgStart)
		{		
			if (m_graphState != State.TimeShifting) return false;

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



		/// <summary>
		/// functions to set / delete pids from data pin
		/// </summary>
		/// <param name="iChannel">New channel</param>
		/// <remarks>
		/// Graph should be timeshifting. 
		/// </remarks>
		/// 

		//
		//
		private int GetServiceID()
		{
			return 12003;
		}
		public void TuneChannel(AnalogVideoStandard xxx,int channel)
		{
			int channelID=TVDatabase.GetChannelId(channel);
		
			m_iChannelNr=channel;
			if(channelID!=-1)
			{
				int freq=0;
				int symrate=0;
				int fec=0;
				int lnbkhz=0;
				int diseqc=0;
				int prognum=0; 
				int servicetype=0;
				string provider=""; 
				string chann="";
				int eitsched=0;
				int eitpol=0; 
				int audpid=0; 
				int vidpid=0; 
				int ac3pid=0;
				int apid1=0; 
				int apid2=0;
				int apid3=0;
				int teltxtpid=0;
				int scrambled=0;
				int pol=0; 
				int lnbfreq=0;
				int networkid=0;
				int pcrpid=0;
				int tsid=0;
				TVDatabase.GetSatChannel(channelID,ref freq,ref symrate,ref  fec,ref lnbkhz,ref diseqc,ref 
					prognum,ref servicetype,ref provider,ref chann,ref  eitsched,ref 
					eitpol,ref  audpid,ref vidpid,ref ac3pid,ref apid1,ref  apid2,ref  apid3,ref 
					teltxtpid,ref scrambled,ref  pol,ref lnbfreq,ref networkid,ref tsid,ref pcrpid);
				Tune(freq,symrate,6,pol,lnbkhz,diseqc,audpid,vidpid,lnbfreq);
			}
			
			m_StartTime=DateTime.Now;
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
		public bool StartViewing(AnalogVideoStandard xxx,int channel)
		{
			if (m_graphState != State.Created) return false;
			TuneChannel(xxx,channel);
			int hr=0;

			AddPreferredCodecs();

			// render here for viewing
			hr=m_sourceGraph.Render(m_videoPin);
			if(hr!=0)
				return false;
			hr=m_sourceGraph.Render(m_audioPin);
			if(hr!=0)
				return false;
			int n=0;// 
			m_mediaControl = (IMediaControl)m_sourceGraph;
			m_videoWindow = (IVideoWindow) m_sourceGraph as IVideoWindow;
			if (m_videoWindow==null)
			{
				Log.Write("DVBSS2:FAILED:Unable to get IVideoWindow");
				return false;
			}

			m_basicVideo = m_sourceGraph as IBasicVideo2;
			if (m_basicVideo==null)
			{
				Log.Write("DVBSS2:FAILED:Unable to get IBasicVideo2");
				return false;
			}
			hr = m_videoWindow.put_Owner(GUIGraphicsContext.form.Handle);
			if (hr != 0) 
				DirectShowUtil.DebugWrite("DVBSS2:FAILED:set Video window:0x{0:X}",hr);

			hr = m_videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
			if (hr != 0) 
				DirectShowUtil.DebugWrite("DVBSS2:FAILED:set Video window style:0x{0:X}",hr);

      
			hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
			if (hr != 0) 
				DirectShowUtil.DebugWrite("DVBSS2:FAILED:put_Visible:0x{0:X}",hr);

            //
			n=m_mediaControl.Run();
			m_bOverlayVisible=true;
			
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
			if (m_graphState != State.Viewing) 
				return false;
			GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			DirectShowUtil.DebugWrite("DVBSS2:StopViewing()");
			m_videoWindow.put_Visible(DsHlp.OAFALSE);
			m_bOverlayVisible=false;
			m_mediaControl.Stop();
			m_mediaControl=null;
			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}

		//
		public bool ShouldRebuildGraph(int iChannel)
		{
			// we always need to rebuild
			// the ds graph, so return always
			// true. the tunning is done in 
			// start viewing/timeshifting/recording methods
			return true;
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
	}
}
