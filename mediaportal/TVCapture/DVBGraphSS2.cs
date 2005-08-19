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
//#define USEMTSWRITER
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
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;



namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Zusammenfassung für DVBGraphSS2.
	/// </summary>
	/// 
	public class DVBGraphSS2 : IGraph
	
	{

		#region Mpeg2-Arrays
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
		};
		#endregion

		#region Enums
		protected enum State
		{ 
			None,
			Created,
			TimeShifting,
			Recording,
			Viewing,
			Radio,
			EPGGrab
		};

		public enum TunerType
		{
			ttCable=0,
			ttSat,
			ttTerrestrical,
			ttATSC
		
		}
		#endregion

		#region Structs
		public struct TunerData
		{
			public int tt;
			public UInt32 Frequency;
			public UInt32 SymbolRate;
			public UInt16 LNB;           //LNB Frequency, e.g. 9750, 10600
			public UInt16 PMT;           //PMT Pid
			public UInt16 ECM_0; //= 0 if unencrypted
			public byte Reserved1;
			public byte AC3;           //= 1 if audio PID = AC3 private stream
			//= 0 otherwise
			public UInt16 FEC;           //1 = 1/2, 2 = 2/3, 3 = 3/4,
			//4 = 5/6, 5 = 7/8, 6 = Auto
			public UInt16 CAID_0;
			public UInt16 Polarity;      //0 = H, 1 = V
			//or Modulation or GuardUInterval
			public UInt16 ECM_1;
			public UInt16 LNBSelection;  //0 = none, 1 = 22 khz
			public UInt16 CAID_1;
			public UInt16 DiseqC;        //0 = none, 1 = A, 2 = B,
			//3 = A/A, 4 = B/A, 5 = A/B, 6 = B/B
			public UInt16 ECM_2;
			public UInt16 AudioPID;
			public UInt16 CAID_2;
			public UInt16 VideoPID;
			public UInt16 TransportStreamID; //from 2.0 R3 on (?), if scanned channel
			public UInt16 TelePID;
			public UInt16 NetworkID;         //from 2.0 R3 on (?), if scanned channel
			public UInt16 SID;               //Service ID
			public UInt16 PCRPID;

		} 
		#endregion

		#region Imports
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern bool DvrMsCreate(out int id, IBaseFilter streamBufferSink, [In, MarshalAs(UnmanagedType.LPWStr)]string strPath, uint dwRecordingType);
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void DvrMsStart(int id, uint startTime);
		[DllImport("dshowhelper.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void DvrMsStop(int id);

		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern bool EventMsg(int eventType,[In] IntPtr data);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int SetAppHandle([In] IntPtr hnd/*,[In, MarshalAs(System.Runtime.InteropServices.UnmanagedType.FunctionPtr)] Delegate Callback*/);
		[DllImport("SoftCSA.dll",  CallingConvention=CallingConvention.StdCall)]
		public static extern int MenuItemClick([In] int ptr);
		[DllImport("SoftCSA.dll",  CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetMenuHandle([In] long menu);

		[DllImport("dvblib.dll", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.StdCall)]
		public static extern int SetupDemuxer(IPin pin,int pid,IPin pin1,int pid1,IPin pin2,int pid2);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool GetSectionData(DShowNET.IBaseFilter filter,int pid, int tid, ref int secCount,int tabSec,int timeout);

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SetPidToPin(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,int pin,int pid);		

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DeleteAllPIDs(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl,int pin);
			
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int GetSNR(DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2 tunerCtrl,[Out] out int a,[Out] out int b);

		// registry settings
		[DllImport("advapi32", CharSet=CharSet.Auto)]
		private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

		[ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
		class StreamBufferConfig {}
		#endregion

		#region Definitions
		//
		public  static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid( 0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
		public  static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid( 0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
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
		IEPGGrabber							m_epgGrabberInterface		= null;
		IMHWGrabber							m_mhwGrabberInterface		= null;
		IBaseFilter							m_dvbAnalyzer=null;
		IStreamAnalyzer					m_analyzerInterface			= null;

		IBaseFilter						  m_tsWriter=null;
		IMPTSWriter							m_tsWriterInterface=null;
		IMPTSRecord						  m_tsRecordInterface=null;
		IBaseFilter							m_smartTee= null;			

		EpgGrabber							m_epgGrabber = new EpgGrabber();

		protected IMpeg2Demultiplexer	m_demuxInterface=null;
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
		protected IPin					m_demuxSectionsPin=null;
		protected IPin					m_data0=null;
		protected IPin					m_data1=null;
		protected IPin					m_data2=null;
		protected IPin					m_data3=null;
		protected IPin					m_pinAC3Out=null;
		// stream buffer sink filter
		protected IStreamBufferConfigure		m_config=null;
		protected IStreamBufferSink				m_sinkInterface=null;
		protected IBaseFilter					m_sinkFilter=null;
		protected IBaseFilter					m_mpeg2Analyzer=null;
		protected IBaseFilter					m_demux=null;
		// def. the interfaces
		protected DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3		m_dataCtrl=null;
		protected DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2	m_tunerCtrl=null;
		protected DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2		m_avCtrl=null;
        // player graph
		protected IGraphBuilder			m_graphBuilder=null;
		protected bool					m_timeShift=true;
		protected int					m_myCookie=0; // for the rot
		protected DateTime              m_StartTime=DateTime.Now;
		protected int					m_iChannelNr=-1;
		protected bool					m_channelFound=false;
		StreamBufferConfig				m_streamBufferConfig=null;
		protected VMR9Util				Vmr9=null; 
		protected VMR7Util				Vmr7=null;

		protected string				m_filename="";
		protected DVBSections			m_sections=new DVBSections();
		protected bool					m_pluginsEnabled=false;
		int	[]							m_ecmPids=new int[3]{0,0,0};
		int[]							m_ecmIDs=new int[3]{0,0,0};
        DVBDemuxer m_streamDemuxer = new DVBDemuxer();
		string							m_cardType="";
		string							m_cardFilename="";
		DVBChannel						m_currentTuningObject;
		int													m_recorderId=-1;
		int								m_selectedAudioPid=0;
		int m_iVideoWidth=1;
		int m_iVideoHeight=1;
		int m_aspectX=1;
		int m_aspectY=1;
		bool isUsingAC3=false;
		int  m_lastPMTVersion=-1;

		DateTime m_timeDisplayed=DateTime.Now;

		bool m_lastTuneError=false;
		#endregion

		
		public DVBGraphSS2(int cardId,int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel)
		{
			m_cardID=cardId;
			
			using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				m_pluginsEnabled=xmlreader.GetValueAsBool("dvb_ts_cards","enablePlugins",false);
				m_cardType=xmlreader.GetValueAsString("DVBSS2","cardtype","");
				m_cardFilename=xmlreader.GetValueAsString("dvb_ts_cards","filename","");
			}

			// teletext settings
			GUIWindow win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
			if(win!=null)
				win.SetObject(m_streamDemuxer.Teletext);
			
			win=GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
			if(win!=null)
                win.SetObject(m_streamDemuxer.Teletext);

//            m_streamDemuxer.OnAudioFormatChanged += new DVBDemuxer.OnAudioChanged(OnAudioFormatChanged);
			m_streamDemuxer.SetCardType((int)DVBEPG.EPGCard.TechnisatStarCards,NetworkType.DVBS);
//			m_streamDemuxer.OnPMTIsChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnPMTChanged(m_streamDemuxer_OnPMTIsChanged);
//			m_streamDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(m_streamDemuxer_OnGotTable);
			// reg. settings
			try
			{
				RegistryKey hkcu = Registry.CurrentUser;
				hkcu.CreateSubKey(@"Software\MediaPortal");
				RegistryKey hklm = Registry.LocalMachine;
				hklm.CreateSubKey(@"Software\MediaPortal");

			}
			catch(Exception){}
		}

		bool OnAudioFormatChanged(DVBDemuxer.AudioHeader audioFormat)
		{
			// set demuxer
			// release memory

//				AMMediaType mpegAudioOut = new AMMediaType();
//				mpegAudioOut.majorType = MediaType.Audio;
//				mpegAudioOut.subType = MediaSubType.MPEG2_Audio;
//				mpegAudioOut.sampleSize = 0;
//				mpegAudioOut.temporalCompression = false;
//				mpegAudioOut.fixedSizeSamples = true;
//				mpegAudioOut.unkPtr = IntPtr.Zero;
//				mpegAudioOut.formatType = FormatType.WaveEx;
//				mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
//				mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
//				System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);
				
			return true;
		}
		~DVBGraphSS2()
		{
		}
		//
		public static void Message()
		{
		}
		//
		#region Plugin-Handling
		void ExecTuner()
		{
			TunerData tu=new TunerData();
			//tu.TunerType=1;
			tu.tt=(int)TunerType.ttSat;
			tu.Frequency=(UInt32)(m_currentChannel.Frequency);
			tu.SymbolRate=(UInt32)(m_currentChannel.Symbolrate);
			tu.AC3=0;
			tu.AudioPID=(UInt16)m_currentChannel.AudioPid;
			tu.DiseqC=(UInt16)m_currentChannel.DiSEqC;
			tu.PMT=(UInt16)m_currentChannel.PMTPid;
			tu.FEC=(UInt16)6;
			tu.LNB=(UInt16)m_currentChannel.LNBFrequency;
			tu.LNBSelection=(UInt16)m_currentChannel.LNBKHz;
			tu.NetworkID=(UInt16)m_currentChannel.NetworkID;
			tu.PCRPID=(UInt16)m_currentChannel.PCRPid;
			tu.Polarity=(UInt16)m_currentChannel.Polarity;
			tu.SID=(UInt16)m_currentChannel.ProgramNumber;
			tu.TelePID=(UInt16)m_currentChannel.TeletextPid;
			tu.TransportStreamID=(UInt16)m_currentChannel.TransportStreamID;
			tu.VideoPID=(UInt16)m_currentChannel.VideoPid;
			tu.Reserved1=0;
			tu.ECM_0=(UInt16)m_currentChannel.ECMPid;
			tu.ECM_1=(UInt16)m_ecmPids[1];
			tu.ECM_2=(UInt16)m_ecmPids[2];
			tu.CAID_0=(UInt16)m_currentChannel.Audio3;
			tu.CAID_1=(UInt16)m_ecmIDs[1];
			tu.CAID_2=(UInt16)m_ecmIDs[2];

			IntPtr data=Marshal.AllocHGlobal(50);
			
			Marshal.StructureToPtr(tu,data,true);

			bool flag=false;
			if(m_pluginsEnabled)
			{
				try
				{
					flag=EventMsg(999, data/*,out pids*/);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Capture,"Plugins-Exception: {0}",ex.Message);
				}

			}
			Marshal.FreeHGlobal(data);
		}
		#endregion
		//
		/// <summary>
		/// Callback from Card. Sets an information struct with video settings
		/// </summary>

		public bool CreateGraph(int Quality)
		{
			int hr;
			if (m_graphState != State.None) return false;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph()");
			m_myCookie=0;

			// create graphs
			Vmr9 =new VMR9Util("mytv");
			Vmr7=new VMR7Util();
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() create graph");
			m_graphBuilder=(IGraphBuilder)  Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) );
			isUsingAC3=false;

			int n=0;
			m_b2c2Adapter=null;
			// create filters & interfaces
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() create filters");
			try
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:CreateGraph() create B2C2 adapter");
				m_b2c2Adapter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_B2C2Adapter, false ) );

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:CreateGraph() create streambuffer");
				m_sinkFilter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_StreamBufferSink, false ) );

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:CreateGraph() create MPEG2 analyzer");
				m_mpeg2Analyzer=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( DVBSkyStar2Helper.CLSID_Mpeg2VideoStreamAnalyzer, true ) );
				m_sinkInterface=(IStreamBufferSink)m_sinkFilter;

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:CreateGraph() create MPEG2 demultiplexer");
				m_demux=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.Mpeg2Demultiplexer, true ) );
				m_demuxInterface=(IMpeg2Demultiplexer) m_demux;
				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:CreateGraph() create sample grabber");
				m_sampleGrabber=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.SampleGrabber, true ) );
				m_sampleInterface=(ISampleGrabber) m_sampleGrabber;


				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:CreateGraph() create dvbanalyzer");
				m_dvbAnalyzer=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.MPStreamAnalyzer, true ) );
				m_analyzerInterface=(IStreamAnalyzer)m_dvbAnalyzer;
				m_epgGrabberInterface=m_dvbAnalyzer as IEPGGrabber;
				m_mhwGrabberInterface=m_dvbAnalyzer as IMHWGrabber;
				hr=m_graphBuilder.AddFilter(m_dvbAnalyzer,"Stream-Analyzer");
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2: FAILED to add SectionsFilter 0x{0:X}",hr);
					return false;
				}
			}
			
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
				return false;
				//System.Windows.Forms.MessageBox.Show(ex.Message);
			}

			if(m_b2c2Adapter==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() m_b2c2Adapter not found");
				return false;
			}
			try
			{

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() add filters to graph");
				n=m_graphBuilder.AddFilter(m_b2c2Adapter,"B2C2-Source");
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to add B2C2-Adapter");
					return false;
				}
				n=m_graphBuilder.AddFilter(m_sampleGrabber,"GrabberFilter");
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to add SampleGrabber");
					return false;
				}

				n=m_graphBuilder.AddFilter(m_demux,"Demuxer");
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to add Demultiplexer");
					return false;
				}
				// get interfaces
				m_dataCtrl= m_b2c2Adapter as DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3;
				if(m_dataCtrl==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: cannot get IB2C2MPEG2DataCtrl3");
					return false;
				}
				m_tunerCtrl=m_b2c2Adapter as DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2;
				if(m_tunerCtrl==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: cannot get IB2C2MPEG2TunerCtrl2");
					return false;
				}
				m_avCtrl= m_b2c2Adapter as DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2;
				if(m_avCtrl==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: cannot get IB2C2MPEG2AVCtrl2");
					return false;
				}
				// init for tuner

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: Initialize Tuner()");
				n=m_tunerCtrl.Initialize();
				if(n!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: Tuner initialize failed");
					return false;
				}
				// call checklock once, the return value dont matter
	
				n=m_tunerCtrl.CheckLock();
				bool b=false;

				
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: SetVideoAudioPins()");
				b=SetVideoAudioPins();
				if(b==false)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: SetVideoAudioPins() failed");
					return false;
				}

				//create EPG pins
				Log.Write("DVBGraphSS2:Create EPG pin");
				AMMediaType mtEPG = new AMMediaType();
				mtEPG.majorType=MEDIATYPE_MPEG2_SECTIONS;
				mtEPG.subType=MediaSubType.None;
				mtEPG.formatType=FormatType.None;

				AMMediaType mtSections = new AMMediaType();
				mtSections.majorType=MEDIATYPE_MPEG2_SECTIONS;
				mtSections.subType=MediaSubType.None;
				mtSections.formatType=FormatType.None;


				IPin pinEPGout, pinMHW1Out,pinMHW2Out, pinSectionsOut;
				hr=m_demuxInterface.CreateOutputPin(ref mtSections, "sections", out pinSectionsOut);
				if (hr!=0 || pinSectionsOut==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to create sections pin:0x{0:X}",hr);
					return false;
				}
				hr=m_demuxInterface.CreateOutputPin(ref mtEPG, "EPG", out pinEPGout);
				if (hr!=0 || pinEPGout==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to create EPG pin:0x{0:X}",hr);
					return false;
				}
				hr=m_demuxInterface.CreateOutputPin(ref mtEPG, "MHW1", out pinMHW1Out);
				if (hr!=0 || pinMHW1Out==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to create MHW1 pin:0x{0:X}",hr);
					return false;
				}
				hr=m_demuxInterface.CreateOutputPin(ref mtEPG, "MHW2", out pinMHW2Out);
				if (hr!=0 || pinMHW2Out==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to create MHW2 pin:0x{0:X}",hr);
					return false;
				}

				Log.Write("DVBGraphSS2:Get EPGs pin of analyzer");
				IPin pinSectionsIn=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,0);
				if (pinSectionsIn==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to get sections pin on MSPA");
					return false;
				}

				IPin pinMHW1In=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,1);
				if (pinMHW1In==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to get MHW1 pin on MSPA");
					return false;
				}
				IPin pinMHW2In=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,2);
				if (pinMHW2In==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to get MHW2 pin on MSPA");
					return false;
				}
				IPin pinEPGIn=DirectShowUtil.FindPinNr(m_dvbAnalyzer,PinDirection.Input,3);
				if (pinEPGIn==null)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to get EPG pin on MSPA");
					return false;
				}

				Log.Write("DVBGraphSS2:Connect epg pins");
				hr=m_graphBuilder.Connect(pinSectionsOut,pinSectionsIn);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to connect sections pin:0x{0:X}",hr);
					return false;
				}
				hr=m_graphBuilder.Connect(pinEPGout,pinEPGIn);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to connect EPG pin:0x{0:X}",hr);
					return false;
				}
				hr=m_graphBuilder.Connect(pinMHW1Out,pinMHW1In);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to connect MHW1 pin:0x{0:X}",hr);
					return false;
				}
				hr=m_graphBuilder.Connect(pinMHW2Out,pinMHW2In);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:FAILED to connect MHW2 pin:0x{0:X}",hr);
					return false;
				}

				Log.Write("DVBGraphSS2:Demuxer is setup");


				if(m_sampleInterface!=null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: add sample grabber");
					AMMediaType mt=new AMMediaType();
					mt.majorType=DShowNET.MediaType.Stream;
					mt.subType=DShowNET.MediaSubType.MPEG2Transport;	
					//m_sampleInterface.SetOneShot(true);
					m_sampleInterface.SetCallback(m_streamDemuxer,1);
					m_sampleInterface.SetMediaType(ref mt);
					m_sampleInterface.SetBufferSamples(false);
				}
				else
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() SampleGrabber-Interface not found");
					

				m_epgGrabber.EPGInterface=m_epgGrabberInterface;
				m_epgGrabber.MHWInterface=m_mhwGrabberInterface;
				m_epgGrabber.Network=Network();

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: graph created");
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
				return false;
			}

			DsROT.AddGraphToRot(m_graphBuilder,out m_myCookie);
			m_graphState=State.Created;
			return true;
		}

		//
		private bool Tune(int Frequency,int SymbolRate,int FEC,int POL,int LNBKhz,int Diseq,int AudioPID,int VideoPID,int LNBFreq,int ecmPID,int ttxtPID,int pmtPID,int pcrPID,string pidText,int dvbsubPID, int programNumber)
		{
			int hr=0; // the result

			m_lastTuneError=false;
			// clear epg
			if(Frequency>13000)
				Frequency/=1000;

			if(m_tunerCtrl==null || m_dataCtrl==null || m_b2c2Adapter==null || m_avCtrl==null)
				return false;

			// skystar
			if(m_cardType=="" || m_cardType=="skystar")
			{
				hr = m_tunerCtrl.SetFrequency(Frequency);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetFrequency");
					return false;	// *** FUNCTION EXIT POINT
				}
				hr = m_tunerCtrl.SetSymbolRate(SymbolRate);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetSymbolRate");
					return false;	// *** FUNCTION EXIT POINT
				}
				hr = m_tunerCtrl.SetLnbFrequency(LNBFreq);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetLnbFrequency");
					return false;	// *** FUNCTION EXIT POINT
				}
				hr = m_tunerCtrl.SetFec(FEC);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetFec");
					return false;	// *** FUNCTION EXIT POINT
				}
				hr = m_tunerCtrl.SetPolarity(POL);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetPolarity");
					return false;	// *** FUNCTION EXIT POINT
				}
				hr = m_tunerCtrl.SetLnbKHz(LNBKhz);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetLnbKHz");
					return false;	// *** FUNCTION EXIT POINT
				}
				hr = m_tunerCtrl.SetDiseqc(Diseq);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetDiseqc");
					return false;	// *** FUNCTION EXIT POINT
				}
			}
			// cablestar
			
			
			
			// airstar

			// final
			hr = m_tunerCtrl.SetTunerStatus();
			if (hr!=0)	
			{
				Log.WriteFile(Log.LogType.Capture,"Tune for SkyStar2 FAILED: on SetTunerStatus");
				return false;	// *** FUNCTION EXIT POINT
				//
			}
			
			DeleteAllPIDs(m_dataCtrl,0);
			SetPidToPin(m_dataCtrl,0,0x2000);
			m_lastPMTVersion=-1;

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
			if (GUIGraphicsContext.Vmr9Active) return;
			
			if (m_basicVideo==null) return;
			if (m_videoWindow==null) return;
			if (m_graphState!=State.Viewing) return;
			
			if (GUIGraphicsContext.BlankScreen)
			{
				Overlay=false;
			}
			else
			{
				Overlay=true;
			}
			int iVideoWidth=0;
			int iVideoHeight=0;
			int aspectX=4, aspectY=3;
			if (m_basicVideo!=null)
			{
				m_basicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);	
				m_basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
			}
			
			GUIGraphicsContext.VideoSize=new Size(iVideoWidth, iVideoHeight );
			m_iVideoWidth=iVideoWidth;
			m_iVideoHeight=iVideoHeight;
			m_aspectX=aspectX;
			m_aspectY=aspectY;

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

				if(m_basicVideo!=null)
				{
					m_basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
					m_basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
				}
				if(m_videoWindow!=null)
					m_videoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
			}
			else 
			{
				if ( GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 || 
						GUIGraphicsContext.VideoWindow.Width <=0 || GUIGraphicsContext.VideoWindow.Height <=0) return;
				if (iVideoHeight<=0 || iVideoWidth<=0) return;
        
				if(m_basicVideo!=null)
				{
					m_basicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
					m_basicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
				}
				if(m_videoWindow!=null)
					m_videoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);

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
			int hr;
			DirectShowUtil.DebugWrite("DVBGraphSS2:DeleteGraph()");
			isUsingAC3=false;
			m_iChannelNr=-1;
			//m_fileWriter.Close();
			StopRecording();
			StopTimeShifting();
			StopViewing();

			if (m_streamDemuxer != null)
			{
				m_streamDemuxer.SetChannelData(0, 0, 0, 0, "",0,0);
			}

			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}
			if (Vmr7!=null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: free vmr7");
				Vmr7.RemoveVMR7();
				Vmr7=null;
			}
			if (m_recorderId>=0) 
			{
				DvrMsStop(m_recorderId);
				m_recorderId=-1;
			}
				
			
			if (m_mediaControl!=null) m_mediaControl.Stop();	
			m_mediaControl = null;
			m_basicVideo = null;
			m_videoPin=null;
			m_audioPin=null;
			m_demuxVideoPin=null;
			m_demuxAudioPin=null;
			m_demuxSectionsPin=null;
			m_data0=null;
			m_data1=null;
			m_data2=null;
			m_data3=null;
			m_pinAC3Out=null;
			m_sinkInterface=null;
			m_demuxInterface=null;
			m_sampleInterface=null;
			m_tunerCtrl=null;
			m_avCtrl=null;
			m_dataCtrl=null;
			m_config=null;
			m_analyzerInterface=null;
			m_epgGrabberInterface=null;
			m_mhwGrabberInterface=null;

			m_tsWriterInterface=null;
			m_tsRecordInterface=null;
			

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
				m_bOverlayVisible=false;
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
				m_videoWindow = null;
			}	
			if (m_streamBufferConfig != null) 
			{
				while ((hr=Marshal.ReleaseComObject(m_streamBufferConfig))>0); 
				if (hr!=0) Log.Write("ReleaseComObject(m_streamBufferConfig):{0}",hr);
				m_streamBufferConfig=null;
			}
	
			if(m_sampleGrabber!=null)
			{
				while ((hr=Marshal.ReleaseComObject(m_sampleGrabber))>0);
				if (hr!=0) Log.Write("ReleaseComObject(m_sampleGrabber):{0}",hr);
				m_sampleGrabber=null;
			}
			if(m_demux!=null)
			{
				while ((hr=Marshal.ReleaseComObject(m_demux))>0);
				if (hr!=0) Log.Write("ReleaseComObject(m_demux):{0}",hr);
				m_demux=null;
			}			

			if(m_mpeg2Analyzer!=null)
			{
				while ((hr=Marshal.ReleaseComObject(m_mpeg2Analyzer))>0);
				if (hr!=0) Log.Write("ReleaseComObject(m_mpeg2Analyzer):{0}",hr);
				m_mpeg2Analyzer=null;
			}

			if(m_sinkFilter!=null)
			{
				while ((hr=Marshal.ReleaseComObject(m_sinkFilter))>0);
				if (hr!=0) Log.Write("ReleaseComObject(m_sinkFilter):{0}",hr);
				m_sinkFilter=null;
			}

			if(m_b2c2Adapter!=null)
			{
				while ((hr=Marshal.ReleaseComObject(m_b2c2Adapter))>0);
				if (hr!=0) Log.Write("ReleaseComObject(m_b2c2Adapter):{0}",hr);
				m_b2c2Adapter=null;
			}
			DsUtils.RemoveFilters(m_graphBuilder);      

			if (m_myCookie != 0)
				DsROT.RemoveGraphFromRot(ref m_myCookie);
			m_myCookie = 0;

			if (m_graphBuilder != null)
			{
				while ((hr=Marshal.ReleaseComObject(m_graphBuilder))>0); 
				if (hr!=0) Log.Write("ReleaseComObject(m_graphBuilder):{0}",hr);
				m_graphBuilder = null;
			}

			//add collected stuff into programs database
			GUIGraphicsContext.form.Invalidate(true);
			GC.Collect();GC.Collect();GC.Collect();
			m_graphState = State.None;
			return;		
		}
		//
		//

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
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: pins not found on adapter");
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
		private bool CreateMTSWriter(string fileName)
		{
			if(m_graphState!=State.Created)
				return false;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateMTSWriter()");
			//connect capture->sample grabber
			IPin grabberIn;
			grabberIn=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);
			if (grabberIn==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed cannot find input pin of sample grabber");
				return false;
			}
			int hr=m_graphBuilder.Connect(m_data0,grabberIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(grabberIn);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect capture->sample grabber");
				return false;
			}
			Marshal.ReleaseComObject(grabberIn);
			grabberIn=null;

			//connect sample grabber->inf tee
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:add InfTee filter");
			m_smartTee = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.InfTee, true));
			m_graphBuilder.AddFilter(m_smartTee,"Inf Tee");
			
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect sample grabber->inf tee");
			IPin grabberOut, smartTeeIn;
			grabberOut=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);
			smartTeeIn=DirectShowUtil.FindPinNr(m_smartTee,PinDirection.Input,0);	
			if (grabberOut==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find output pin of samplegrabber");
				return false;
			}
			if (smartTeeIn==null)
			{
				Marshal.ReleaseComObject(grabberOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find input pin of inftee");
				return false;
			}

			hr=m_graphBuilder.Connect(grabberOut,smartTeeIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(grabberOut);
				Marshal.ReleaseComObject(smartTeeIn);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot grabber->inftee :0x{0:X}",hr);
				return false;
			}
			Marshal.ReleaseComObject(grabberOut);
			Marshal.ReleaseComObject(smartTeeIn);

			//connect inftee->demuxer
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect inftee->demuxer");
			IPin smartTeeOut, demuxerIn;
			smartTeeOut = DirectShowUtil.FindPinNr(m_smartTee,PinDirection.Output,0);
			demuxerIn   = DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	
			if (smartTeeOut==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find output pin#0 of inftee");
				return false;
			}
			if (demuxerIn==null)
			{
				Marshal.ReleaseComObject(smartTeeOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find input pin of demuxer");
				return false;
			}

			hr=m_graphBuilder.Connect(smartTeeOut,demuxerIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(demuxerIn);
				Marshal.ReleaseComObject(smartTeeOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot inftee->demuxer :0x{0:X}",hr);
				return false;
			}
			Marshal.ReleaseComObject(demuxerIn);
			Marshal.ReleaseComObject(smartTeeOut);

			//add mpts writer
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:add MPTSWriter");
			m_tsWriter=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.MPTSWriter, true ) );
			m_tsWriterInterface = m_tsWriter as IMPTSWriter;
			m_tsRecordInterface = m_tsWriter as IMPTSRecord;

			hr=m_graphBuilder.AddFilter((IBaseFilter)m_tsWriter,"MPTS Writer");
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot add MPTS Writer:{0:X}",hr);
				return false;
			}			

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:set filename on MPTSWriter");
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


			// connect inftee->mpts writer
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect inftee->mpts writer");
			IPin tsWriterIn;
			smartTeeOut=DirectShowUtil.FindPinNr(m_smartTee,PinDirection.Output,1);
			tsWriterIn=DirectShowUtil.FindPinNr(m_tsWriter,PinDirection.Input,0);	
			if (smartTeeOut==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find output pin#1 of inftee");
				return false;
			}
			if (tsWriterIn==null)
			{
				Marshal.ReleaseComObject(smartTeeOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find input pin of tswriter");
				return false;
			}

			hr=m_graphBuilder.Connect(smartTeeOut,tsWriterIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(smartTeeOut);
				Marshal.ReleaseComObject(tsWriterIn);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot inftee->tswriter :0x{0:X}",hr);
				return false;
			}
			Marshal.ReleaseComObject(smartTeeOut);
			Marshal.ReleaseComObject(tsWriterIn);
			SetupMTSDemuxerPin();
			return true;
		}

		void SetupMTSDemuxerPin()
		{
#if USEMTSWRITER
			if (m_tsWriterInterface== null || m_tsWriterInterface==null || m_currentChannel==null) return;
			//m_tsWriterInterface.ResetPids();
			if (m_currentChannel.AC3Pid>0)
				m_tsWriterInterface.SetAC3Pid((ushort)m_currentChannel.AC3Pid);
			else
				m_tsWriterInterface.SetAC3Pid(0);

			if (m_currentChannel.AudioPid>0)
				m_tsWriterInterface.SetAudioPid((ushort)m_currentChannel.AudioPid);
			else
			{
				if (m_currentChannel.Audio1>0)
					m_tsWriterInterface.SetAudioPid((ushort)m_currentChannel.Audio1);
				else
					m_tsWriterInterface.SetAudioPid(0);
			}
			
			if (m_currentChannel.Audio2>0)
				m_tsWriterInterface.SetAudioPid2((ushort)m_currentChannel.Audio2);
			else
				m_tsWriterInterface.SetAudioPid2(0);

			//m_tsWriterInterface.SetSubtitlePid((ushort)m_currentChannel.SubtitlePid);
			if (m_currentChannel.TeletextPid>0)
				m_tsWriterInterface.SetTeletextPid((ushort)m_currentChannel.TeletextPid);
			else
				m_tsWriterInterface.SetTeletextPid(0);

			if (m_currentChannel.VideoPid>0)
				m_tsWriterInterface.SetVideoPid((ushort)m_currentChannel.VideoPid);
			else
				m_tsWriterInterface.SetVideoPid(0);

			if (m_currentChannel.PCRPid>0)
				m_tsWriterInterface.SetPCRPid((ushort)m_currentChannel.PCRPid);
			else
				m_tsWriterInterface.SetPCRPid(0);

			m_tsWriterInterface.SetPMTPid((ushort)m_currentChannel.PMTPid);
#endif
		}

		private bool CreateSinkSource(string fileName,bool useAC3)
		{
			if(m_graphState!=State.Created)
				return false;
			int			hr=0;
			IPin		pinObj0=null;
			IPin		pinObj1=null;
			IPin		outPin=null;


			hr=m_graphBuilder.AddFilter(m_sinkFilter,"StreamBufferSink");
			hr=m_graphBuilder.AddFilter(m_mpeg2Analyzer,"Stream-Analyzer");

			// setup sampleGrabber and demuxer
			IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
			IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	
			hr=m_graphBuilder.Connect(m_data0,samplePin);
			if(hr!=0)
				return false;

			samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);	
			hr=m_graphBuilder.Connect(demuxInPin,samplePin);
			if(hr!=0)
				return false;

			SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid,m_currentChannel.AC3Pid);
				
				
			if(m_demuxVideoPin==null || m_demuxAudioPin==null)
				return false;

			pinObj0=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Input,0);
			if(pinObj0!=null)
			{
				
				hr=m_graphBuilder.Connect(m_demuxVideoPin,pinObj0);
				if(hr==0)
				{
					// render all out pins
					pinObj1=DirectShowUtil.FindPinNr(m_mpeg2Analyzer,PinDirection.Output,0);	
					hr=m_graphBuilder.Render(pinObj1);
					if(hr!=0)
						return false;
					if(!useAC3)
					{
						hr=m_graphBuilder.Render(m_demuxAudioPin);
						if(hr!=0)
							return false;
					}
					else
					{
						hr=m_graphBuilder.Render(m_pinAC3Out);
						if(hr!=0)
							return false;
					}
					if(demuxInPin!=null)
						Marshal.ReleaseComObject(demuxInPin);
					if(samplePin!=null)
						Marshal.ReleaseComObject(samplePin);
					if(pinObj1!=null)
						Marshal.ReleaseComObject(pinObj1);
					if(pinObj0!=null)
						Marshal.ReleaseComObject(pinObj0);

					demuxInPin=null;
					samplePin=null;
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

			IStreamBufferConfigure2 streamConfig2	= m_streamBufferConfig as IStreamBufferConfigure2;
			if (streamConfig2!=null)
				streamConfig2.SetFFTransitionRates(8,32);

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
		public bool StartTimeShifting(TVChannel channel,string fileName)
		{

			if(m_graphState!=State.Created)
				return false;
			int hr=0;
			
			TuneChannel(channel);

			if(m_channelFound==false)
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
			isUsingAC3=TVDatabase.DoesChannelHaveAC3(channel, Network()==NetworkType.DVBC, Network()==NetworkType.DVBT, Network()==NetworkType.DVBS, Network()==NetworkType.ATSC);

#if USEMTSWRITER
			if (CreateMTSWriter(fileName))
			{
				m_mediaControl=(IMediaControl)m_graphBuilder;
				hr=m_mediaControl.Run();
				m_graphState = State.TimeShifting;
			}
			else 
			{
				m_graphState=State.Created;
				return false;
			}
#else
			if(CreateSinkSource(fileName,isUsingAC3)==true)
			{
				m_mediaControl=(IMediaControl)m_graphBuilder;
				hr=m_mediaControl.Run();
				m_graphState = State.TimeShifting;
			}
			else 
			{
				m_graphState=State.Created;
				return false;
			}
#endif
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
			if (m_mediaControl!=null)
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
		public bool StartRecording(Hashtable attribtutes,TVRecording recording,TVChannel channel, ref string strFilename, bool bContentRecording, DateTime timeProgStart)
		{		
			if (m_graphState != State.TimeShifting ) return false;

#if USEMTSWRITER
			if (m_tsRecordInterface==null) 
			{
				return false;
			}

			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRecording()");
			strFilename=System.IO.Path.ChangeExtension(strFilename,".ts");
			int hr=m_tsRecordInterface.SetRecordingFileName(strFilename);
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
			m_graphState=State.Recording;
			return true;
#else
			if (Vmr9!=null)
			{
				Vmr9.RemoveVMR9();
				Vmr9.Release();
				Vmr9=null;
			}
			try
			{
				uint iRecordingType=0;
				if (bContentRecording) iRecordingType=0;
				else iRecordingType=1;										
			 
				bool success=DvrMsCreate(out m_recorderId,(IBaseFilter)m_sinkFilter,strFilename,iRecordingType);
				if (!success)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:StartRecording() FAILED to create recording");
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
				/*
				foreach (MetadataItem item in attribtutes.Values)
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
				DvrMsStart(m_recorderId,(uint)lStartTime);
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:Failed to start recording :{0} {1} {2}",
					ex.Message,ex.Source,ex.StackTrace);
			}
			m_graphState=State.Recording;
#endif
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
			if (m_graphState!=State.Recording)
				return ;
			

			if (m_recorderId>=0) 
			{
				DvrMsStop(m_recorderId);
				m_recorderId=-1;
			}


			if (m_tsRecordInterface!=null)
			{
				m_tsRecordInterface.StopRecord(0);
			}

			m_graphState=State.TimeShifting;
			return ;

		}

		//
		//

		public void TuneChannel(TVChannel channel)
		{
			try
			{
				if (Vmr9!=null) Vmr9.Enable(false);
				if(m_graphState==State.Recording)
					return;

				int channelID=channel.ID;
				m_iChannelNr=channel.Number;
				if(channelID!=-1)
				{
					
					DVBChannel ch=new DVBChannel();
					if(TVDatabase.GetSatChannel(channelID,1,ref ch)==false)//only television
					{
						Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: Tune: channel not found in database (idChannel={0})",channelID);
						m_channelFound=false;
						return;
					}

					if(m_pluginsEnabled==false && ch.IsScrambled==true)
					{
						m_channelFound=false;
						return;
					}
					m_channelFound=true;
					m_currentChannel=ch;
					m_selectedAudioPid=ch.AudioPid;
					if(Tune(ch.Frequency,ch.Symbolrate,6,ch.Polarity,ch.LNBKHz,ch.DiSEqC,ch.AudioPid,ch.VideoPid,ch.LNBFrequency,ch.ECMPid,ch.TeletextPid,ch.PMTPid,ch.PCRPid,ch.AudioLanguage3,ch.Audio3,ch.ProgramNumber)==false)
					{
						m_lastTuneError=true;
						m_channelFound=false;
						return;
					}

					m_lastTuneError=false;
					if (m_streamDemuxer != null)
					{
						m_streamDemuxer.OnTuneNewChannel();
						m_streamDemuxer.SetChannelData(ch.AudioPid, ch.VideoPid, ch.TeletextPid, ch.Audio3, ch.ServiceName,ch.PMTPid,ch.ProgramNumber);
					}
					if(m_pluginsEnabled==true)
						ExecTuner();

					if(m_mediaControl!=null && m_demuxVideoPin!=null && m_demuxAudioPin!=null && m_demux!=null && m_demuxInterface!=null)
					{

						int hr = SetupDemuxer(m_demuxVideoPin, ch.VideoPid,m_demuxAudioPin, ch.AudioPid,m_pinAC3Out,ch.AC3Pid);
						if(hr!=0)
						{
							Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: SetupDemuxer FAILED: errorcode {0}",hr.ToString());
							return;
						}
					}

					//SetMediaType();
					//m_gotAudioFormat=false;
					m_analyzerInterface.ResetParser();
					m_StartTime=DateTime.Now;

					m_epgGrabber.GrabEPG(ch.HasEITSchedule==true);
					VideoRendererStatistics.VideoState=VideoRendererStatistics.State.NoSignal;
					SetupMTSDemuxerPin();
				}
			}
			finally
			{
				if (Vmr9!=null) Vmr9.Enable(true);
			}
		}
		void SetDemux(int audioPid,int videoPid,int ac3Pid)
		{
			
			if(m_demuxInterface==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: SetDemux FAILED: no Demux-Interface");
				return;
			}
			int hr=0;

			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:SetDemux() audio pid:0x{0:X} video pid:0x{1:X}",audioPid,videoPid);
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
            ////IPin pinVideoOut,pinAudioOut;
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

			hr=m_demuxInterface.CreateOutputPin(ref mediaAC3/*vidOut*/, "AC3", out m_pinAC3Out);
			if (hr!=0 || m_pinAC3Out==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"mpeg2:FAILED to create AC3 pin:0x{0:X}",hr);
			}

      hr=m_demuxInterface.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out m_demuxVideoPin);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED to create video output pin on demuxer");
				return;
			}
      hr = m_demuxInterface.CreateOutputPin(ref mpegAudioOut, "audio", out m_demuxAudioPin);
      if (hr != 0)
      {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED to create audio output pin on demuxer");
          return;
      }

			hr=SetupDemuxer(m_demuxVideoPin,videoPid,m_demuxAudioPin,audioPid,m_pinAC3Out,ac3Pid);
			if(hr!=0)//ignore audio pin
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED to config Demuxer");
				return;
			}
			


			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:SetDemux() done:{0}", hr);
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
		public bool StartViewing(TVChannel channel)
		{
			if (m_graphState != State.Created) return false;
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing()");
			TuneChannel(channel);
			int hr=0;
			bool setVisFlag=false;
			
			m_bOverlayVisible=true;
			if(m_channelFound==false)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() channel not found");
				return false;
			}
			AddPreferredCodecs(true,true);
			
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


			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() Using plugins");
			//connect capture->sample grabber
			IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
			if (samplePin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find samplePin");
				return false;
			}

			hr=m_graphBuilder.Connect(m_data0,samplePin);
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot connect data0->samplepin");
				return false;
			}

			//connect sample grabber->demuxer
			IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	
			if (demuxInPin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find demuxInPin");
				return false;
			}

			samplePin=null;
			samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);			
			if(samplePin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find sampleGrabber output pin");
				return false;
			}
			hr=m_graphBuilder.Connect(samplePin,demuxInPin);
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: connect sample->demux");
				return false;
			}

			//setup demuxer
			SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid,m_currentChannel.AC3Pid);
			
			if(m_demuxVideoPin==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find demux video output pin");
				return false;
			}
			if (m_demuxAudioPin == null)
			{
				Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demux audio output pin");
				return false;
			}

			hr=m_graphBuilder.Render(m_demuxVideoPin);
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot render demux video output pin");
				return false;
			}
			isUsingAC3=TVDatabase.DoesChannelHaveAC3(channel, Network()==NetworkType.DVBC, Network()==NetworkType.DVBT, Network()==NetworkType.DVBS, Network()==NetworkType.ATSC);
			if (!isUsingAC3)
			{
				if(m_graphBuilder.Render(m_demuxAudioPin) != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:Failed to render audio out pin MPEG-2 Demultiplexer");
					return false;
				}
			}
			else
			{
				if (m_pinAC3Out == null)
				{
					Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demux audio ac3 output pin");
					return false;
				}
				if(m_graphBuilder.Render(m_pinAC3Out) != 0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraphSS2:Failed to render AC3 pin MPEG-2 Demultiplexer");
					return false;
				}
			}

			//
			if(demuxInPin!=null)
				Marshal.ReleaseComObject(demuxInPin);
			if(samplePin!=null)
				Marshal.ReleaseComObject(samplePin);

			//
			
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

			//
			//
			//
			//
			m_mediaControl = (IMediaControl)m_graphBuilder;
			if (useOverlay)
			{
				m_videoWindow = (IVideoWindow) m_graphBuilder as IVideoWindow;
				if (m_videoWindow==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:FAILED:Unable to get IVideoWindow");
				}

				m_basicVideo = (IBasicVideo2)m_graphBuilder as IBasicVideo2;
				if (m_basicVideo==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:FAILED:Unable to get IBasicVideo2");
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

			m_bOverlayVisible=true;
			GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			m_graphState = State.Viewing;
			GUIGraphicsContext_OnVideoWindowChanged();
			//
			
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() start graph");

			m_mediaControl.Run();
				
			if(setVisFlag)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() show video window");
				hr = m_videoWindow.put_Visible(DsHlp.OATRUE);
				if (hr != 0) 
				{
					DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:put_Visible:0x{0:X}",hr);
				}

			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() startviewing done");
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
			
			GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
			DirectShowUtil.DebugWrite("DVBGraphSS2:StopViewing()");
			if(m_videoWindow!=null)
				m_videoWindow.put_Visible(DsHlp.OAFALSE);
			m_bOverlayVisible=false;

			if (Vmr9!=null)
			{
				Vmr9.Enable(false);
			}
			if (m_mediaControl!=null)
			{
				m_mediaControl.Stop();
				m_mediaControl=null;
			}
			m_graphState = State.Created;
			DeleteGraph();
			GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
			GUIWindowManager.SendMessage(msg);

			return true;
		}

		//
		public bool ShouldRebuildGraph(TVChannel newChannel)
		{
			//check if we switch from an channel with AC3 to a channel without AC3
			//or vice-versa. ifso, graphs should be rebuild
			if (m_graphState != State.Viewing && m_graphState!= State.TimeShifting && m_graphState!=State.Recording) return false; 
			bool useAC3=TVDatabase.DoesChannelHaveAC3(newChannel, Network()==NetworkType.DVBC, Network()==NetworkType.DVBT, Network()==NetworkType.DVBS, Network()==NetworkType.ATSC);
			if (useAC3 != isUsingAC3) return true; 
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
				return (m_lastTuneError==true?false:true);
		}
		
		public int  SignalQuality()
		{
			int level;
			int quality;
			GetSNR(m_tunerCtrl,out level,out quality);
			return quality;
		}
		
		public int  SignalStrength()
		{
			return 100;
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
		void UpdateVideoState()
		{
			//check if this card is used for watching tv
			bool isViewing=Recorder.IsCardViewing(m_cardID);
			if (!isViewing) return;

//			Log.Write("demuxer:{0} signal:{1} fps:{2}",m_streamDemuxer.RecevingPackets,SignalPresent() ,GUIGraphicsContext.Vmr9FPS);

			// do we receive any packets?
			if (!m_streamDemuxer.RecevingPackets || !SignalPresent() )
			{
				//no, then state = no signal
				VideoRendererStatistics.VideoState=VideoRendererStatistics.State.NoSignal;
			}
			else
			{	
				// we receive packets
				// is channel scrambled ?
				if(GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS < 1f)
				{
					if  ( (g_Player.Playing && !g_Player.Paused) || (!g_Player.Playing) )
					{
						VideoRendererStatistics.VideoState=VideoRendererStatistics.State.Scrambled;
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
		}

		public void Process()
		{
			if (m_graphState==State.None || m_graphState==State.Created) return;
			if(!GUIGraphicsContext.Vmr9Active && Vmr7!=null && m_graphState==State.Viewing)
			{
				Vmr7.Process();
			}

			if (m_streamDemuxer!=null) m_streamDemuxer.Process();
			CheckVideoResolutionChanges();
			m_epgGrabber.Process();
			UpdateVideoState();
			
			if (m_currentChannel!=null)
			{
				IntPtr pmtMem=Marshal.AllocCoTaskMem(4096);// max. size for pmt
				if(pmtMem!=IntPtr.Zero)
				{
					//get the PMT
					m_analyzerInterface.SetPMTProgramNumber(m_currentChannel.ProgramNumber);
					int res=m_analyzerInterface.GetPMTData(pmtMem);
					if(res!=-1)
					{
						//check PMT version
						byte[] pmt=new byte[res];
						int version=-1;
						Marshal.Copy(pmtMem,pmt,0,res);
						version=((pmt[5]>>1)&0x1F);
						int pmtProgramNumber=(pmt[3]<<8)+pmt[4];
						if (pmtProgramNumber==m_currentChannel.ProgramNumber)
						{
							if(m_lastPMTVersion!=version)
							{
								//decode pmt
								m_lastPMTVersion=version;
								DVBSections sections = new DVBSections();
								DVBSections.ChannelInfo info = new DVBSections.ChannelInfo();
								if (sections.GetChannelInfoFromPMT(pmt, ref info)) 
								{
									//map pids
									if (info.pid_list!=null)
									{
										DeleteAllPIDs(m_dataCtrl,0);
										SetPidToPin(m_dataCtrl,0,0);
										SetPidToPin(m_dataCtrl,0,0x10);
										SetPidToPin(m_dataCtrl,0,0x11);
										SetPidToPin(m_dataCtrl,0,0xd2);
										SetPidToPin(m_dataCtrl,0,0xd3);
										SetPidToPin(m_dataCtrl,0,m_currentChannel.PMTPid);
										if (m_currentChannel.PCRPid>0)
											SetPidToPin(m_dataCtrl,0,m_currentChannel.PCRPid);
										for (int pids =0; pids < info.pid_list.Count;pids++)
										{
											DVBSections.PMTData data=(DVBSections.PMTData) info.pid_list[pids];
											if (data.elementary_PID>0 && (data.isAC3Audio || data.isAudio||data.isVideo||data.isTeletext) )
											{
												SetPidToPin(m_dataCtrl,0,data.elementary_PID);
											}
										}
									}
								}
							}
						}
					}
				}
				Marshal.FreeCoTaskMem(pmtMem);
			}
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
		//
		public void Tune(object tuningObject, int disecqNo)
		{
			DVBChannel ch=(DVBChannel)tuningObject;
			ch=LoadDiseqcSettings(ch,disecqNo);
			m_currentTuningObject=new DVBChannel();
			if(m_dvbAnalyzer==null)
				return;
			try
			{
				if(m_mediaControl==null)
				{
					m_graphBuilder.Render(m_data0);
					m_mediaControl=m_graphBuilder as IMediaControl;
					m_mediaControl.Run();
				}
			}
			catch{}
			if(Tune(ch.Frequency,ch.Symbolrate,6,ch.Polarity,ch.LNBKHz,ch.DiSEqC,-1,-1,ch.LNBFrequency,0,0,0,0,"",0,0)==false)
			{
				m_lastTuneError=true;
				Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: FAILED to tune channel");
				return;
			}
			else
			{
				m_lastTuneError=false;
				Log.WriteFile(Log.LogType.Capture,"called Tune(object)");
			}
			m_currentTuningObject=ch;
			m_analyzerInterface.ResetParser();
		}
		//
		public void StoreChannels(int ID,bool radio, bool tv, ref int newChannels, ref int updatedChannels,ref int newRadioChannels, ref int updatedRadioChannels)
		{
			Log.WriteFile(Log.LogType.Capture,"called StoreChannels()");
			if (m_dvbAnalyzer==null) return;


			//get list of current tv channels present in the database
			ArrayList tvChannels = new ArrayList();
			TVDatabase.GetChannels(ref tvChannels);

			m_analyzerInterface.ResetParser(); //analyser will map pids needed
			Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: StoreChannels()");
			DVBSections.Transponder transp;
			transp.channels=null;

			Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: get channels()");
			System.Threading.Thread.Sleep(2500);
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
							chi.fec=m_currentTuningObject.FEC;
							if (Network() != NetworkType.ATSC)
							{
								chi.freq=m_currentTuningObject.Frequency;
							}
							else
							{
								m_currentTuningObject.Frequency=0;
								m_currentTuningObject.Modulation=chi.modulation;
							}
							Marshal.FreeCoTaskMem(mmch);
							transp.channels.Add(chi);
						}
						else Log.Write("DVBGraphSS2:channel {0} is not ready!!!",t);
					}
				}
			}

			if (transp.channels==null)
			{
				Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: found no channels", transp.channels);
				return;
			}
			Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: found {0} channels", transp.channels.Count);
			for (int i=0; i < transp.channels.Count;++i)
			{
				m_currentTuningObject.AC3Pid=-1;
				m_currentTuningObject.VideoPid=0;
				m_currentTuningObject.AudioPid=0;
				m_currentTuningObject.TeletextPid=0;
				m_currentTuningObject.Audio1=0;
				m_currentTuningObject.Audio2=0;
				m_currentTuningObject.Audio3=0;
				m_currentTuningObject.AudioLanguage=String.Empty;
				m_currentTuningObject.AudioLanguage1=String.Empty;
				m_currentTuningObject.AudioLanguage2=String.Empty;
				m_currentTuningObject.AudioLanguage3=String.Empty;
				System.Windows.Forms.Application.DoEvents();
				System.Windows.Forms.Application.DoEvents();


				int audioOptions=0;

				DVBSections.ChannelInfo info=(DVBSections.ChannelInfo)transp.channels[i];
				if (info.service_provider_name==null) info.service_provider_name="";
				if (info.service_name==null) info.service_name="";
				
				info.service_provider_name=info.service_provider_name.Trim();
				info.service_name=info.service_name.Trim();
				if (info.service_provider_name.Length==0 ) 
					info.service_provider_name=Strings.Unknown;
				if (info.service_name.Length==0)
					info.service_name=String.Format("NoName:{0}{1}{2}{3}",info.networkID,info.transportStreamID, info.serviceID,i );


				if (info.serviceID==0) 
				{
					Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel#{0} has no service id",i);
					continue;
				}
				bool hasAudio=false;
				bool hasVideo=false;
				info.freq=m_currentTuningObject.Frequency;
				DVBChannel newchannel   = new DVBChannel();

				//check if this channel has audio/video streams
				if (info.pid_list!=null)
				{
					audioOptions=0;
					for (int pids =0; pids < info.pid_list.Count;pids++)
					{
						DVBSections.PMTData data=(DVBSections.PMTData) info.pid_list[pids];
						if(data.isAudio && hasAudio==true && audioOptions<2)
						{
							switch(audioOptions)
							{
								case 0:
									newchannel.Audio1=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											newchannel.AudioLanguage1=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions=1;
									break;
								case 1:
									newchannel.Audio2=data.elementary_PID;
									if(data.data!=null)
									{
										if(data.data.Length==3)
											newchannel.AudioLanguage2=DVBSections.GetLanguageFromCode(data.data);
									}
									audioOptions=2;
									break;

							}
						}
						if (data.isAC3Audio)
						{
							m_currentTuningObject.AC3Pid=data.elementary_PID;
						}
						if (data.isVideo)
						{
							m_currentTuningObject.VideoPid=data.elementary_PID;
							hasVideo=true;
						}
						if (data.isAudio && hasAudio==false)
						{
							m_currentTuningObject.AudioPid=data.elementary_PID;
							if(data.data!=null)
							{
								if(data.data.Length==3)
									newchannel.AudioLanguage=DVBSections.GetLanguageFromCode(data.data);
							}
							hasAudio=true;
						}
						if (data.isTeletext)
						{
							m_currentTuningObject.TeletextPid=data.elementary_PID;
						}
						if(data.isDVBSubtitle)
						{
							m_currentTuningObject.Audio3=data.elementary_PID;
						}
					}
				}
				Log.WriteFile(Log.LogType.Capture,"auto-tune ss2:Found provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:{9} videopid:{10} teletextpid:{11}", 
					info.service_provider_name,
					info.service_name,
					info.scrambled,
					info.freq,
					info.networkID,
					info.transportStreamID,
					info.serviceID,
					hasVideo, ((!hasVideo) && hasAudio),
					m_currentTuningObject.AudioPid,m_currentTuningObject.VideoPid,m_currentTuningObject.TeletextPid);
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
				newchannel.Polarity = m_currentTuningObject.Polarity;
				newchannel.Modulation = m_currentTuningObject.Modulation;
				newchannel.Symbolrate = m_currentTuningObject.Symbolrate;
				newchannel.ServiceType=info.serviceType;//tv
				newchannel.PCRPid=info.pcr_pid;
				newchannel.PMTPid=info.network_pmt_PID;
				newchannel.LNBFrequency=m_currentTuningObject.LNBFrequency;
				newchannel.LNBKHz=m_currentTuningObject.LNBKHz;
				newchannel.DiSEqC=m_currentTuningObject.DiSEqC;
				newchannel.AudioPid=m_currentTuningObject.AudioPid;
				newchannel.VideoPid=m_currentTuningObject.VideoPid;
				newchannel.TeletextPid=m_currentTuningObject.TeletextPid;
				newchannel.AC3Pid=m_currentTuningObject.AC3Pid;
				newchannel.HasEITSchedule=info.eitSchedule;
				newchannel.HasEITPresentFollow=info.eitPreFollow;
				newchannel.AudioLanguage3=info.pidCache;
				newchannel.Audio3=m_currentTuningObject.Audio3;
			

				if (info.serviceType!=1 && info.serviceType!=2) continue;
				if (info.serviceType==1 && tv)
				{
					Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} is a tv channel",newchannel.ServiceName);
					//check if this channel already exists in the tv database
					bool isNewChannel=true;
					int channelId=-1;
					TVChannel tvChan = new TVChannel();
					tvChan.Name=newchannel.ServiceName;
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

					//if the tv channel found is not yet in the tv database
					tvChan.Scrambled=newchannel.IsScrambled;
					if (isNewChannel)
					{
						//then add a new channel to the database
						tvChan.Number=TVDatabase.FindFreeTvChannelNumber(newchannel.ProgramNumber);
						tvChan.Sort=newchannel.ProgramNumber;
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: create new tv channel for {0}",newchannel.ServiceName);
						int id=TVDatabase.AddChannel(tvChan);
						channelId=id;
						newChannels++;
					}
					else
					{
						TVDatabase.UpdateChannel(tvChan,tvChan.Sort);
						updatedChannels++;
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} already exists in tv database",newchannel.ServiceName);
					}
					Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
					newchannel.ID=channelId;
					TVDatabase.AddSatChannel(newchannel);
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
				else
				{
					if(info.serviceType==2)
					{
						//todo: radio channels
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} is a radio channel",newchannel.ServiceName);
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
							Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: create new radio channel for {0}",newchannel.ServiceName);
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
							Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: channel {0} already exists in tv database",newchannel.ServiceName);
						}

						if (Network() == NetworkType.DVBS)
						{
							Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: map channel {0} id:{1} to DVBS card:{2}",newchannel.ServiceName,channelId,ID);
							newchannel.ID=channelId;

							int scrambled=0;
							if (newchannel.IsScrambled) scrambled=1;
							RadioDatabase.MapDVBSChannel(newchannel.ID,newchannel.Frequency,newchannel.Symbolrate,
								newchannel.FEC,newchannel.LNBKHz,newchannel.DiSEqC,newchannel.ProgramNumber,
								0,newchannel.ServiceProvider,newchannel.ServiceName,
								0,0,newchannel.AudioPid,0,newchannel.AC3Pid,
								0,0,0,0,scrambled,
								newchannel.Polarity,newchannel.LNBFrequency
								,newchannel.NetworkID,newchannel.TransportStreamID,newchannel.PCRPid,
								newchannel.AudioLanguage,newchannel.AudioLanguage1,
								newchannel.AudioLanguage2,newchannel.AudioLanguage3,
								newchannel.ECMPid,newchannel.PMTPid);
						}
						RadioDatabase.MapChannelToCard(channelId,ID);
						Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: channel {0} is a radio channel",newchannel.ServiceName);
					}
				}
			}//for (int i=0; i < transp.channels.Count;++i)
			SetLCN();
		}

		void SetLCN()
		{
			Int16 count=0;
			while (true)
			{
				Int16 networkId, transportId, serviceID, LCN;
				string provider;
				m_analyzerInterface.GetLCN(count,out  networkId, out transportId, out serviceID, out LCN);
				if (networkId>0 && transportId>0 && serviceID>=0 && LCN>0)
				{
					TVChannel channel=TVDatabase.GetTVChannelByStream(Network()==NetworkType.ATSC,Network()==NetworkType.DVBT,Network()==NetworkType.DVBC,Network()==NetworkType.DVBS,networkId,transportId,serviceID, out provider);
					if (channel!=null) 
					{
						TVDatabase.SetChannelSort(channel.Name,LCN);
						TVGroup group = new TVGroup();
						if (channel.Scrambled)
						{
							group.GroupName="Scrambled";
						}
						else
						{
							group.GroupName="Unscrambled";
						}
						int groupid=TVDatabase.AddGroup(group);
						group.ID=groupid;
						TVDatabase.MapChannelToGroup(group,channel);
						
						group = new TVGroup();
						group.GroupName=provider;
						groupid=TVDatabase.AddGroup(group);
						group.ID=groupid;
						TVDatabase.MapChannelToGroup(group,channel);

					}
				}
				else 
				{
					return;
				}
				count++;
			}
		}

		public IBaseFilter Mpeg2DataFilter()
		{
			return null;
		}

		DVBChannel LoadDiseqcSettings(DVBChannel ch,int disNo)
		{
			if(m_cardFilename=="")
				return ch;

			int lnbKhz=0;
			int lnbKhzVal=0;
			int diseqc=0;
			int lnbKind=0;
			// lnb config
			int lnb0MHZ=0;
			int lnb1MHZ=0;
			int lnbswMHZ=0;
			int cbandMHZ=0;
			int circularMHZ=0;

			using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(m_cardFilename))
			{
				lnb0MHZ=xmlreader.GetValueAsInt("dvbs","LNB0",9750);
				lnb1MHZ=xmlreader.GetValueAsInt("dvbs","LNB1",10600);
				lnbswMHZ=xmlreader.GetValueAsInt("dvbs","Switch",11700);
				cbandMHZ=xmlreader.GetValueAsInt("dvbs","CBand",5150);
				circularMHZ=xmlreader.GetValueAsInt("dvbs","Circular",10750);
//				bool useLNB1=xmlreader.GetValueAsBool("dvbs","useLNB1",false);
//				bool useLNB2=xmlreader.GetValueAsBool("dvbs","useLNB2",false);
//				bool useLNB3=xmlreader.GetValueAsBool("dvbs","useLNB3",false);
//				bool useLNB4=xmlreader.GetValueAsBool("dvbs","useLNB4",false);
				switch(disNo)
				{
					case 1:
						// config a
						lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb",44);
						diseqc=xmlreader.GetValueAsInt("dvbs","diseqc",0);
						lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind",0);
						break;
					case 2:
						// config b
						lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb2",44);
						diseqc=xmlreader.GetValueAsInt("dvbs","diseqc2",0);
						lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind2",0);
						break;
					case 3:
						// config c
						lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb3",44);
						diseqc=xmlreader.GetValueAsInt("dvbs","diseqc3",0);
						lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind3",0);
						break;
						//
					case 4:
						// config d
						lnbKhz=xmlreader.GetValueAsInt("dvbs","lnb4",44);
						diseqc=xmlreader.GetValueAsInt("dvbs","diseqc4",0);
						lnbKind=xmlreader.GetValueAsInt("dvbs","lnbKind4",0);
						//
						break;
				}// switch(disNo)
				switch (lnbKhz)
				{
					case 0: lnbKhzVal=0;break;
					case 22: lnbKhzVal=1;break;
					case 33: lnbKhzVal=2;break;
					case 44: lnbKhzVal=3;break;
				}


			}//using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(m_cardFilename))

			// set values to dvbchannel-object
			ch.DiSEqC=diseqc;
			// set the lnb parameter 
			if(ch.Frequency>=lnbswMHZ*1000)
			{
				ch.LNBFrequency=lnb1MHZ;
				ch.LNBKHz=lnbKhzVal;
			}
			else
			{
				ch.LNBFrequency=lnb0MHZ;
				ch.LNBKHz=0;
			}
			Log.WriteFile(Log.LogType.Capture,"auto-tune ss2: freq={0} lnbKhz={1} lnbFreq={2} diseqc={3}",ch.Frequency,ch.LNBKHz,ch.LNBFrequency,ch.DiSEqC); 
			return ch;

		}// LoadDiseqcSettings()

		public void TuneRadioChannel(RadioStation station)
		{
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:TuneChannel() get DVBS tuning details");
			DVBChannel ch=new DVBChannel();
			if(RadioDatabase.GetDVBSTuneRequest(station.ID,0,ref ch)==false)//only radio
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:database invalid tuning details for channel:{0}", station.Channel);
				return;
			}
			if(Tune(ch.Frequency,ch.Symbolrate,ch.FEC,ch.Polarity,ch.LNBKHz,ch.DiSEqC,ch.AudioPid,0,ch.LNBFrequency,0,0,ch.PMTPid,ch.PCRPid,ch.AudioLanguage3,0,ch.ProgramNumber)==true)
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: Radio tune ok");
			else
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: FAILED cannot tune");
				return;
			}

			m_currentChannel=ch;
			
			if (m_streamDemuxer != null)
			{
				m_streamDemuxer.OnTuneNewChannel();
				m_streamDemuxer.SetChannelData(ch.AudioPid, ch.VideoPid, ch.TeletextPid, ch.Audio3, ch.ServiceName,ch.PMTPid,ch.ProgramNumber);
			}

			if(m_demuxVideoPin!=null && m_demuxAudioPin!=null)
				SetupDemuxer(m_demuxVideoPin,m_currentChannel.VideoPid,m_demuxAudioPin,m_currentChannel.AudioPid,m_pinAC3Out,m_currentChannel.AC3Pid);

		}

		public void StartRadio(RadioStation station)
		{
			if (m_graphState != State.Radio) 
			{
				if(m_graphState!=State.Created)
					return;

				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2: start radio");

				int hr=0;
				AddPreferredCodecs(true,false);
			
				if (Vmr9!=null)
				{
					Vmr9.RemoveVMR9();
					Vmr9=null;
				}


				Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() Using plugins");
				IPin samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Input,0);	
				IPin demuxInPin=DirectShowUtil.FindPinNr(m_demux,PinDirection.Input,0);	

				if (samplePin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find samplePin");
					return ;
				}
				if (demuxInPin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot find demuxInPin");
					return ;
				}

				hr=m_graphBuilder.Connect(m_data0,samplePin);
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartViewing() FAILED: cannot connect data0->samplepin");
					return ;
				}
				samplePin=null;
				samplePin=DirectShowUtil.FindPinNr(m_sampleGrabber,PinDirection.Output,0);			
				if(samplePin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: cannot find sampleGrabber output pin");
					return ;
				}
				hr=m_graphBuilder.Connect(samplePin,demuxInPin);
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: connect sample->demux");
					return ;
				}

				SetDemux(m_currentChannel.AudioPid,m_currentChannel.VideoPid,m_currentChannel.AC3Pid);
			
				if(m_demuxAudioPin==null)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: cannot find demux audio output pin");
					return ;
				}

				hr=m_graphBuilder.Render(m_demuxAudioPin);
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:StartRadio() FAILED: cannot render demux audio output pin");
					return ;
				}
				//
				if(demuxInPin!=null)
					Marshal.ReleaseComObject(demuxInPin);
				if(samplePin!=null)
					Marshal.ReleaseComObject(samplePin);

				//

			
				//
				//
				m_mediaControl = (IMediaControl)m_graphBuilder;
				m_graphState = State.Radio;
				//
				m_mediaControl.Run();


			}

			// tune to the correct channel
			TuneRadioChannel(station);
			Log.WriteFile(Log.LogType.Capture,"DVBGraphSS2:Listening to radio..");
		}
		public void TuneRadioFrequency(int frequency)
		{
		}
		public bool HasTeletext()
		{
			if (m_graphState!= State.TimeShifting && m_graphState!=State.Recording && m_graphState!=State.Viewing) return false;
			if (m_currentChannel==null) return false;
			if (m_currentChannel.TeletextPid>0) return true;
			return false;
		}
		#region Stream-Audio handling
		public int GetAudioLanguage()
		{
			return m_selectedAudioPid;
		}
		public void SetAudioLanguage(int audioPid)
		{
			if(audioPid!=m_selectedAudioPid)
			{
				int hr=0;
				if (audioPid==m_currentChannel.AC3Pid)
				{
					hr=SetupDemuxer(m_demuxVideoPin,m_currentChannel.VideoPid,m_demuxAudioPin,audioPid,m_pinAC3Out,audioPid);
				}
				else
				{
					hr=SetupDemuxer(m_demuxVideoPin,m_currentChannel.VideoPid,m_demuxAudioPin,audioPid,m_pinAC3Out,m_currentChannel.AC3Pid);
				}
					if (hr != 0)
                {
                    Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: SetupDemuxer FAILED: errorcode {0}", hr.ToString());
                    return;
                }
                else
                {
                    m_selectedAudioPid = audioPid;
                    if(m_streamDemuxer!=null)
                        m_streamDemuxer.SetChannelData(audioPid, m_currentChannel.VideoPid, m_currentChannel.TeletextPid, m_currentChannel.Audio3, m_currentChannel.ServiceName,m_currentChannel.PMTPid,m_currentChannel.ProgramNumber);

                }
			}
		}

		public ArrayList GetAudioLanguageList()
		{
				
			DVBSections.AudioLanguage al;
			ArrayList alList=new ArrayList();
			if(m_currentChannel==null) return alList;
			if(m_currentChannel.AudioPid!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=m_currentChannel.AudioPid;
				al.AudioLanguageCode=m_currentChannel.AudioLanguage;
				alList.Add(al);
			}
			if(m_currentChannel.Audio1!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=m_currentChannel.Audio1;
				al.AudioLanguageCode=m_currentChannel.AudioLanguage1;
				alList.Add(al);
			}
			if(m_currentChannel.Audio2!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=m_currentChannel.Audio2;
				al.AudioLanguageCode=m_currentChannel.AudioLanguage2;
				alList.Add(al);
			}
			if(m_currentChannel.AC3Pid!=0)
			{
				al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
				al.AudioPid=m_currentChannel.AC3Pid;
				al.AudioLanguageCode="AC3";
				alList.Add(al);
			}
			return alList;
		}
		#endregion

		private void m_streamDemuxer_OnPMTIsChanged(byte[] pmtTable)
		{
		}

		private void m_streamDemuxer_OnGotSection(int pid, int tableID, byte[] sectionData)
		{
		}

		private void m_streamDemuxer_OnGotTable(int pid, int tableID, ArrayList tableList)
		{
			if(tableList==null)
				return;
			if(tableList.Count<1)
				return;
		}

	}// class
}// namespace
