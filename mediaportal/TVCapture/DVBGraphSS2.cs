using System;
using Microsoft.Win32;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;
using DShowNET;
using MediaPortal;
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

		protected enum State
		{ 
			None,
			Created,
			TimeShifting,
			Recording,
			Viewing
		};

		public enum TunerType
		{
			ttCable=0,
			ttSat,
			ttTerrestrical,
			ttATSC
		
		}
		public struct TunerData
		{
			public int tt;
			public UInt32 Frequency;
			public UInt32 SymbolRate;
			public Int16 LNB;           //LNB Frequency, e.g. 9750, 10600
			public Int16 PMT;           //PMT Pid
			public Int16 ECM_0; //= 0 if unencrypted
			public byte Reserved1;
			public byte AC3;           //= 1 if audio PID = AC3 private stream
			//= 0 otherwise
			public Int16 FEC;           //1 = 1/2, 2 = 2/3, 3 = 3/4,
			//4 = 5/6, 5 = 7/8, 6 = Auto
			public Int16 CAID_0;
			public Int16 Polarity;      //0 = H, 1 = V
			//or Modulation or GuardUInterval
			public Int16 ECM_1;
			public Int16 LNBSelection;  //0 = none, 1 = 22 khz
			public Int16 CAID_1;
			public Int16 DiseqC;        //0 = none, 1 = A, 2 = B,
			//3 = A/A, 4 = B/A, 5 = A/B, 6 = B/B
			public Int16 ECM_2;
			public Int16 AudioPID;
			public Int16 CAID_2;
			public Int16 VideoPID;
			public Int16 TransportStreamID; //from 2.0 R3 on (?), if scanned channel
			public Int16 TelePID;
			public Int16 NetworkID;         //from 2.0 R3 on (?), if scanned channel
			public Int16 SID;               //Service ID
			public Int16 PCRPID;

		} 
		//
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int GetGraph([In] DShowNET.IGraphBuilder graph,bool running,IntPtr callback);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern void StopGraph();
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern bool Execute(IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int SetAppHandle([In] IntPtr hnd/*,[In, MarshalAs(System.Runtime.InteropServices.UnmanagedType.FunctionPtr)] Delegate Callback*/);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern void PidCallback([In] IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int MenuItemClick([In] int ptr);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetMenuHandle([In] long menu);

		[DllImport("dvblib.dll", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetupDemuxer(IPin videoPin,IPin audioPin,int audio,int video);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid, int tid, ref int secCount,int tabSec,int timeout);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SetPidToPin(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,int pin,int pid);		

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DeleteAllPIDs(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,int pin);
		
		[DllImport("kernel32.dll", EntryPoint="RtlCopyMemory")]
		public static extern void CopyMemory(IntPtr Destination, IntPtr Source,[MarshalAs(UnmanagedType.U4)] int Length);
		// registry settings
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
		class StreamBufferConfig {}
		// we dont use this callback yet
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
		protected IPin					m_demuxVideoPin=null;
		protected IPin					m_demuxAudioPin=null;
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
		protected IBaseFilter					m_demux=null;
		// def. the interfaces
		protected DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3		m_dataCtrl=null;
		protected DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2	m_tunerCtrl=null;
		protected DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2		m_avCtrl=null;
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
		protected bool					m_pluginsEnabled=false;
		public RebuildFunc				m_rebuildCB=null;
		int	[]							m_ecmPids=new int[3]{0,0,0};
		int[]							m_ecmIDs=new int[3]{0,0,0};
		bool							m_videoDataFound=false;
		bool							m_vmr9Running=false;
		DVBTeletext						m_teleText=new DVBTeletext();
		int								m_retryCount=0;
		bool							m_performRepaint=false;
		bool							m_performCyclePid=false;
		bool							m_performTeletext=false;
		byte[]							m_streamBufferArray=new byte[65535];
		int								m_dataLen=0;
		TSHelperTools					m_tsHelper=new TSHelperTools();


//
		
		//
		public DVBGraphSS2(int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel)
		{
			m_rebuildCB=new RebuildFunc(RebuildTuner);

			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_pluginsEnabled=xmlreader.GetValueAsBool("DVBSS2","enablePlugins",false);
			}
			
			GUIWindow win=GUIWindowManager.GetWindow(7700);
			if(win!=null)
				win.SetObject(m_teleText);

			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");

			}
			catch(Exception){}
		}
		~DVBGraphSS2()
		{
		}
		//
		public void RebuildTuner(IntPtr data)
		{

			TunerData td=new TunerData();
			try
			{
				td=(TunerData)Marshal.PtrToStructure(data,typeof(TunerData));
				m_currentChannel.ECMPid=(int)td.ECM_0;
				SetPidToPin(m_dataCtrl,0,(int)td.ECM_0);
				TVDatabase.UpdateSatChannel(m_currentChannel);
			}
			catch{}
		
		}
		
		//
		void ExecTuner()
		{
			TunerData tu=new TunerData();
			//tu.TunerType=1;
			tu.tt=(int)TunerType.ttSat;
			tu.Frequency=(UInt32)(m_currentChannel.Frequency);
			tu.SymbolRate=(UInt32)(m_currentChannel.Symbolrate);
			tu.AC3=0;
			tu.AudioPID=(Int16)m_currentChannel.AudioPid;
			tu.DiseqC=(Int16)m_currentChannel.DiSEqC;
			tu.PMT=(Int16)m_currentChannel.PMTPid;
			tu.ECM_0=(Int16)m_currentChannel.ECMPid;
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
			tu.Reserved1=0;
			tu.ECM_1=(Int16)m_ecmPids[1];
			tu.ECM_2=(Int16)m_ecmPids[2];
			tu.CAID_0=(Int16)m_currentChannel.Audio3;
			tu.CAID_1=(Int16)m_ecmIDs[1];
			tu.CAID_2=(Int16)m_ecmIDs[2];

			IntPtr data=Marshal.AllocHGlobal(50);
			Marshal.StructureToPtr(tu,data,true);

			bool flag=false;
			if(m_pluginsEnabled)
			{
				try
				{
					flag=Execute(data/*,out pids*/);
				}
				catch(Exception ex)
				{
					Log.Write("Plugins-Exception: {0}",ex.Message);
				}

			}
			Marshal.FreeHGlobal(data);
		}
		//
		public int BufferCB(double time,IntPtr data,int len)
		{
		
			int add=(int)data;
			int end=(add+len);

			if(m_teleText==null)
			{
				if(m_pluginsEnabled==true)
				{
					for(int pointer=add;pointer<end;pointer+=188)
						PidCallback((IntPtr)pointer);
					
				}
			}
			else
			{
				if(m_pluginsEnabled==true)
				{
					for(int pointer=add;pointer<end;pointer+=188)
					{
						PidCallback((IntPtr)pointer);
						m_teleText.SaveData((IntPtr)pointer);
					}
					
				}
			}
		
			//
			// here write code to record raw ts or mp3 etc.
			// the callback needs to return as soon as possible!!
			//

			// the following check should takes care of scrambled video-data
			// and redraw the vmr9 not to hang

			int pid=m_currentChannel.VideoPid;
			for(int pointer=add;pointer<end;pointer+=188)
			{
					
				TSHelperTools.TSHeader header=m_tsHelper.GetHeader((IntPtr)pointer);
				if(header.Pid==pid)
				{
					
					if(header.TransportScrambling!=0) // data is scrambled?
						m_videoDataFound=false;
					else
						m_videoDataFound=true;
						
					break;// stop loop if we got a non-scrambled video-packet 
				}
			}
			//
			if(m_vmr9Running==true  && m_videoDataFound==false)
				m_performRepaint=true;

			// call the plugins tuner
			if(m_videoDataFound==false && m_retryCount>=0)
			{
				m_retryCount++;
				if(m_retryCount>=250)//
					m_performCyclePid=true;
			}
			else m_retryCount=-1;


			if(m_performTeletext==false)
			{
				m_dataLen=len;
				if(m_dataLen>65535)
					m_dataLen=65535;
				Marshal.Copy(data,m_streamBufferArray,0,m_dataLen);
				m_performTeletext=true;
			}
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

		void CyclePid()
		{
			int		currentPid=m_currentChannel.ECMPid;
			int		nextPid=0;
			int		startCheck=1;
			string	pidString=m_currentChannel.AudioLanguage3;

			// get actual pid position
			for(int t=1;t<20;t++)
			{
				if(GetPidNumber(pidString,t+1)==-1)
				{
					startCheck=1;
					break;
				}
				if(GetPidNumber(pidString,t)==currentPid)
				{
					startCheck=t;
					break;
				}
			}
			// get next pid
			for(int t=startCheck;t<20;t++)
			{
				nextPid=GetPidNumber(pidString,t);
				if(nextPid==-1)
					break;
				if(nextPid!=currentPid && nextPid>0)
				{
					m_currentChannel.ECMPid=nextPid;
					TVDatabase.UpdateSatChannel(m_currentChannel);
					break;
				}
			}
			
		}
		//
		//
		public bool CreateGraph()
		{
			if (m_graphState != State.None) return false;
			Log.Write("DVBGraphSS2:creategraph()");
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
				m_b2c2Adapter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_B2C2Adapter, false ) );
				m_sinkFilter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_StreamBufferSink, false ) );
				m_mpeg2Analyzer=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_Mpeg2VideoStreamAnalyzer, true ) );
				m_sinkInterface=(IStreamBufferSink)m_sinkFilter;
				m_mpeg2Data=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_Mpeg2Data, true ) );
				m_demux=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.Mpeg2Demultiplexer, true ) );
				m_demuxInterface=(IMpeg2Demultiplexer) m_demux;
				m_sampleGrabber=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.SampleGrabber, true ) );
				m_sampleInterface=(ISampleGrabber) m_sampleGrabber;
			}
			
			catch(Exception ex)
			{
				Log.Write("DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
				return false;
				//System.Windows.Forms.MessageBox.Show(ex.Message);
			}
			if(m_b2c2Adapter==null)
				return false;
			try
			{
				n=m_sourceGraph.AddFilter(m_b2c2Adapter,"B2C2-Source");
				if(n!=0)
				{
					Log.Write("DVBGraphSS2: FAILED to add B2C2-Adapter");
					return false;
				}
				n=m_sourceGraph.AddFilter(m_sampleGrabber,"GrabberFilter");
				if(n!=0)
				{
					Log.Write("DVBGraphSS2: FAILED to add SampleGrabber");
					return false;
				}

				n=m_sourceGraph.AddFilter(m_demux,"Demuxer");
				if(n!=0)
				{
					Log.Write("DVBGraphSS2: FAILED to add Demultiplexer");
					return false;
				}
				// get interfaces
				m_dataCtrl=(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3) m_b2c2Adapter;
				if(m_dataCtrl==null)
				{
					Log.Write("DVBGraphSS2: cannot get IB2C2MPEG2DataCtrl3");
					return false;
				}
				m_tunerCtrl=(DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2) m_b2c2Adapter;
				if(m_tunerCtrl==null)
				{
					Log.Write("DVBGraphSS2: cannot get IB2C2MPEG2TunerCtrl2");
					return false;
				}
				m_avCtrl=(DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2) m_b2c2Adapter;
				if(m_avCtrl==null)
				{
					Log.Write("DVBGraphSS2: cannot get IB2C2MPEG2AVCtrl2");
					return false;
				}
				// init for tuner
				n=m_tunerCtrl.Initialize();
				if(n!=0)
				{
					Log.Write("DVBGraphSS2: Tuner initialize failed");
					return false;
				}
				// call checklock once, the return value dont matter
	
				n=m_tunerCtrl.CheckLock();
				bool b=false;
				b=SetVideoAudioPins();
				if(b==false)
				{
					Log.Write("DVBGraphSS2: SetVideoAudioPins() failed");
					return false;
				}


				if(m_sampleInterface!=null)
				{
					AMMediaType mt=new AMMediaType();
					mt.majorType=DShowNET.MediaType.Stream;
					mt.subType=DShowNET.MediaSubType.MPEG2Transport;	
					//m_sampleInterface.SetOneShot(true);
					m_sampleInterface.SetCallback(this,1);
					m_sampleInterface.SetMediaType(ref mt);
					m_sampleInterface.SetBufferSamples(false);
				}
				else
					Log.Write("DVBGraphSS2:creategraph() SampleGrabber-Interface not found");
					

			}
			catch(Exception ex)
			{
				Log.Write("DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
				System.Windows.Forms.MessageBox.Show(ex.Message);
				return false;
			}
			
			m_graphState=State.Created;
			return true;
		}

		//
		private bool Tune(int Frequency,int SymbolRate,int FEC,int POL,int LNBKhz,int Diseq,int AudioPID,int VideoPID,int LNBFreq,int ecmPID,int ttxtPID,int pmtPID,int pcrPID,string pidText,int dvbsubPID)
		{
			int hr=0; // the result

			if(Frequency>13000)
				Frequency/=1000;

			if(m_tunerCtrl==null || m_dataCtrl==null || m_b2c2Adapter==null || m_avCtrl==null)
				return false;


			hr = m_tunerCtrl.SetFrequency(Frequency);
			if (hr!=0)
			{
				Log.Write("Tune for SkyStar2 FAILED: on SetFrequency");
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetSymbolRate(SymbolRate);
			if (hr!=0)
			{
				Log.Write("Tune for SkyStar2 FAILED: on SetSymbolRate");
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetLnbFrequency(LNBFreq);
			if (hr!=0)
			{
				Log.Write("Tune for SkyStar2 FAILED: on SetLnbFrequency");
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetFec(FEC);
			if (hr!=0)
			{
				Log.Write("Tune for SkyStar2 FAILED: on SetFec");
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetPolarity(POL);
			if (hr!=0)
			{
				Log.Write("Tune for SkyStar2 FAILED: on SetPolarity");
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetLnbKHz(LNBKhz);
			if (hr!=0)
			{
				Log.Write("Tune for SkyStar2 FAILED: on SetLnbKHz");
				return false;	// *** FUNCTION EXIT POINT
			}
        
			hr = m_tunerCtrl.SetDiseqc(Diseq);
			if (hr!=0)
			{
				Log.Write("Tune for SkyStar2 FAILED: on SetDiseqc");
				return false;	// *** FUNCTION EXIT POINT
			}
			
			hr = m_tunerCtrl.SetTunerStatus();
			if (hr!=0)	
			{
				// some more tries...
				int retryCount=0;
				while(1>0)
				{
					hr=m_tunerCtrl.SetTunerStatus();
					if(hr!=0)
						retryCount++;
					else
						break;

					if(retryCount>=20)
					{
						Log.Write("Tune for SkyStar2 FAILED: on SetTunerStatus (in loop)");
						return false;	// *** FUNCTION EXIT POINT
					}
				}
				//
				
			}

//			hr=m_tunerCtrl.CheckLock();
//			if(hr!=0)
//			{
//				Log.Write("Tune for SkyStar2 FAILED: on CheckLock");
//				return false;
//			}

			if(AudioPID!=-1 && VideoPID!=-1)
			{
				if(m_pluginsEnabled==false)
				{
					DeleteAllPIDs(m_dataCtrl,0);
					SetPidToPin(m_dataCtrl,0,0);
					SetPidToPin(m_dataCtrl,0,1);
					SetPidToPin(m_dataCtrl,0,16);
					SetPidToPin(m_dataCtrl,0,17);
					SetPidToPin(m_dataCtrl,0,18);
					SetPidToPin(m_dataCtrl,0,ttxtPID);
					SetPidToPin(m_dataCtrl,0,AudioPID);
					SetPidToPin(m_dataCtrl,0,VideoPID);
					SetPidToPin(m_dataCtrl,0,pmtPID);
					SetPidToPin(m_dataCtrl,0,dvbsubPID);
					if(pcrPID!=VideoPID)
						SetPidToPin(m_dataCtrl,0,pcrPID);

				}
				else
				{
					int epid=0;

					int eid=0;
					DeleteAllPIDs(m_dataCtrl,0);

					int count=0;
					for(int t=1;t<11;t++)
					{
						epid=GetPidNumber(pidText,t);
						eid=GetPidID(pidText,t);
						if(epid>0)
						{
							if(count<3)
							{
								m_ecmPids[count]=epid;
								m_ecmIDs[count]=eid;
								count++;
							}
							SetPidToPin(m_dataCtrl,0,epid);
						}
					}

					SetPidToPin(m_dataCtrl,0,0);
					SetPidToPin(m_dataCtrl,0,1);
					SetPidToPin(m_dataCtrl,0,16);
					SetPidToPin(m_dataCtrl,0,17);
					SetPidToPin(m_dataCtrl,0,18);
					SetPidToPin(m_dataCtrl,0,ecmPID);
					SetPidToPin(m_dataCtrl,0,ttxtPID);
					SetPidToPin(m_dataCtrl,0,AudioPID);
					SetPidToPin(m_dataCtrl,0,VideoPID);
					SetPidToPin(m_dataCtrl,0,dvbsubPID);
					SetPidToPin(m_dataCtrl,0,pmtPID);
					if(pcrPID!=VideoPID)
						SetPidToPin(m_dataCtrl,0,pcrPID);
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
			//m_fileWriter.Close();
			if(m_pluginsEnabled)
			{
				StopGraph();
				//m_sampleInterface.SetCallback(null,0);
			}

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


				hr=m_sourceGraph.AddFilter(m_sinkFilter,"StreamBufferSink");
				hr=m_sourceGraph.AddFilter(m_mpeg2Analyzer,"Stream-Analyzer");

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
			if (m_graphState != State.TimeShifting ) return false;

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
				if(TVDatabase.GetSatChannel(channelID,1,ref ch)==false)//only television
				{
					Log.Write("DVBGraphSS2: Tune: channel not found in database (idChannel={0})",channelID);
					m_channelFound=false;
					return;
				}

				if(m_pluginsEnabled==false && ch.IsScrambled==true)
				{
					m_channelFound=false;
					return;
				}
				m_channelFound=true;
				
				if(Tune(ch.Frequency,ch.Symbolrate,6,ch.Polarity,ch.LNBKHz,ch.DiSEqC,ch.AudioPid,ch.VideoPid,ch.LNBFrequency,ch.ECMPid,ch.TeletextPid,ch.PMTPid,ch.PCRPid,ch.AudioLanguage3,ch.Audio3)==false)
				{
					m_channelFound=false;
					return;
				}
				m_currentChannel=ch;

				if(m_pluginsEnabled==true)
					ExecTuner();

				if(m_mediaControl!=null && m_demuxVideoPin!=null && m_demuxAudioPin!=null && m_demux!=null && m_demuxInterface!=null)
				{
				
					int hr=SetupDemuxer(m_demuxVideoPin,m_demuxAudioPin,ch.AudioPid,ch.VideoPid);
					if(hr!=0)
					{
						Log.Write("DVBGraphSS2: SetupDemuxer FAILED: errorcode {0}",hr.ToString());
						return;
					}
				}

				m_retryCount=0;
				m_videoDataFound=false;
				m_StartTime=DateTime.Now;


			}
			
		}
		void SetDemux(int audioPid,int videoPid)
		{
			
			if(m_demuxInterface==null)
			{
				Log.Write("DVBGraphSS2: SetDemux FAILED: no Demux-Interface");
				return;
			}
			int hr=0;

			Log.Write("DVBGraphSS2:SetDemux() audio pid:0x{0X} video pid:0x{1:X}",audioPid,videoPid);
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
			System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat,0,mpegAudioOut.formatPtr,mpegAudioOut.formatSize) ;
			
			//IPin pinVideoOut,pinAudioOut;

			hr=m_demuxInterface.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out m_demuxVideoPin);
			if (hr!=0)
			{
				Log.Write("DVBGraphSS2:StartViewing() FAILED to create video output pin on demuxer");
				return;
			}
			hr=m_demuxInterface.CreateOutputPin(ref mpegAudioOut, "audio", out m_demuxAudioPin);
			if (hr!=0)
			{
				Log.Write("DVBGraphSS2:StartViewing() FAILED to create audio output pin on demuxer");
				return;
			}
			hr=SetupDemuxer(m_demuxVideoPin,m_demuxAudioPin,audioPid,videoPid);
			if(hr!=0)
			{
				Log.Write("DVBGraphSS2: FAILED to config Demuxer");
				return;
			}

			Log.Write("DVBGraphSS2:SetDemux() done:{0}", hr);
			//int //=0;
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
		public bool StartViewing(AnalogVideoStandard standard,int channel, int country)
		{
			if (m_graphState != State.Created) return false;
			Log.Write("DVBGraphSS2:StartViewing()");
			TuneChannel(standard,channel,country);
			int hr=0;
			bool setVisFlag=false;
			
			if(m_channelFound==false)
			{
				Log.Write("DVBGraphSS2:StartViewing() channel not found");
				return false;
			}
			AddPreferredCodecs();
			
			if(Vmr9.UseVMR9inMYTV)
			{
				Vmr9.AddVMR9(m_sourceGraph);
			}


				Log.Write("DVBGraphSS2:StartViewing() Using plugins");
				IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
				IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	

				if (samplePin==null)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot find samplePin");
					return false;
				}
				if (demuxInPin==null)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot find demuxInPin");
					return false;
				}

				hr=m_sourceGraph.Connect(m_data0,samplePin);
				if(hr!=0)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot connect data0->samplepin");
					return false;
				}
				samplePin=null;
				samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);			
				if(samplePin==null)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot find sampleGrabber output pin");
					return false;
				}
				hr=m_sourceGraph.Connect(samplePin,demuxInPin);
				if(hr!=0)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: connect sample->demux");
					return false;
				}

				SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid);
			
				IPin dmOutVid=DirectShowUtil.FindPinNr(m_demux,PinDirection.Output,1);
				IPin dmOutAud=DirectShowUtil.FindPinNr(m_demux,PinDirection.Output,0);
				if(dmOutVid==null)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot find demux video output pin");
					return false;
				}
				if(dmOutAud==null)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot find demux audio output pin");
					return false;
				}

				hr=m_sourceGraph.Render(dmOutVid);
				if(hr!=0)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot render demux video output pin");
					return false;
				}
				hr=m_sourceGraph.Render(dmOutAud);
				if(hr!=0)
				{
					Log.Write("DVBGraphSS2:StartViewing() FAILED: cannot render demux audio output pin");
					return false;
				}
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
			//

			
			if(Vmr9.IsVMR9Connected==false && Vmr9.UseVMR9inMYTV==true)// fallback
			{
				if(Vmr9.VMR9Filter!=null)
					m_sourceGraph.RemoveFilter(Vmr9.VMR9Filter);
				Vmr9.RemoveVMR9();
				Vmr9.UseVMR9inMYTV=false;
			}
			//
			//
			if(Vmr9.IsVMR9Connected==true && Vmr9.UseVMR9inMYTV==true)// fallback
			{
				m_vmr9Running=true;
			}
			//
			//
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
				setVisFlag=true;

			}

			m_bOverlayVisible=true;
			GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			m_graphState = State.Viewing;
			GUIGraphicsContext_OnVideoWindowChanged();
			//
			
			Log.Write("DVBGraphSS2:StartViewing() start graph");

			m_mediaControl.Run();
				
			if(setVisFlag)
			{
				Log.Write("DVBGraphSS2:StartViewing() show video window");
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:put_Visible:0x{0:X}",hr);
				}

			}

			Log.Write("DVBGraphSS2:StartViewing() startviewing done");
			return true;
		}

		int GetPidNumber(string pidText,int number)
		{
			if(pidText=="")
				return 0;
			string[] pidSegments;

			pidSegments=pidText.Split(new char[]{';'});
			if(pidSegments.Length-1<number || pidSegments.Length==0)
				return -1;

			string[] pid=pidSegments[number-1].Split(new char[]{'/'});
			if(pid.Length!=2)
				return -1;

			try
			{
				return Convert.ToInt16(pid[0]);
			}
			catch
			{
				return -1;
			}
		}
		int GetPidID(string pidText,int number)
		{
			if(pidText=="")
				return 0;
			string[] pidSegments;

			pidSegments=pidText.Split(new char[]{';'});
			if(pidSegments.Length-1<number || pidSegments.Length==0)
				return 0;

			string[] pid=pidSegments[number-1].Split(new char[]{'/'});
			if(pid.Length!=2)
				return 0;

			try
			{
				return Convert.ToInt16(pid[1]);
			}
			catch
			{
				return 0;
			}
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

			m_graphState = State.Created;
			DeleteGraph();
			return true;
		}

		//
		public bool ShouldRebuildGraph(int iChannel)
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
			//
			if(m_performRepaint==true) // redraw vmr9
			{
				Vmr9.Repaint();
				m_performRepaint=false;
			}
			//
			if(m_performCyclePid==true) // cycle pid
			{
				m_performCyclePid=false;
				m_retryCount=0;
				CyclePid();
				ExecTuner();
				Log.Write("Plugins: recall Tune() with pid={0}",m_currentChannel.ECMPid);
			}
			//
			// actions for streamBufferPtr
//			if(m_performTeletext==true)
//			{
//				for(int pointer=0;pointer<m_dataLen;pointer+=188)
//				{
//					TSHelperTools.TSHeader header=m_tsHelper.GetHeader(m_streamBufferArray,pointer);
//					if(header.Pid==m_currentChannel.TeletextPid && m_teleText!=null)
//					{
//						m_teleText.SaveData(m_streamBufferArray,pointer);
//					}
//				}
//				m_performTeletext=false;
//			}
			//

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
		public void StoreChannels(int ID,bool radio, bool tv)
		{
		}

		public IBaseFilter Mpeg2DataFilter()
		{
			return null;
		}
	}
}
