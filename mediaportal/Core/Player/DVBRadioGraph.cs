using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using DShowNET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using SQLite.NET;

namespace MediaPortal.Player
{
	/// <summary>
	/// Zusammenfassung für DVBRadioGraph.
	/// </summary>
	public class DVBRadioGraph 
	{
			enum State
			{ 
				None, 
				Created, 
				Listening,
			};
		SQLite.NET.SQLiteClient				m_db=null;
		// iids 0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b
		private static Guid IID_IB2C2AVCTRL2 = new Guid( 0x9c0563ce, 0x2ef7, 0x4568, 0xa2, 0x97, 0x88, 0xc7, 0xbb, 0x82, 0x40, 0x75 );
		private static Guid CLSID_B2C2Adapter = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x0, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);	
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
				[MarshalAs(UnmanagedType.FunctionPtr)] IntPtr vInfo
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
		// the following are needed from dvblib.dll until all interfaces are in dshownet
		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SetPidToPin(IB2C2MPEG2DataCtrl3 dataCtrl,int pin,int pid);		

		[DllImport("dvblib.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DeleteAllPIDs(IB2C2MPEG2DataCtrl3 dataCtrl,int pin);
		
		//
		State							m_graphState=State.None;
		protected IMediaControl			m_mediaControl=null;
		protected int                   m_cardID=-1;
		protected IBaseFilter			m_b2c2Adapter=null;
		protected IPin					m_audioPin=null;
		// def. the interfaces
		protected IB2C2MPEG2DataCtrl3	m_dataCtrl=null;
		protected IB2C2MPEG2TunerCtrl2	m_tunerCtrl=null;
		protected IB2C2MPEG2AVCtrl2		m_avCtrl=null;
		// player graph
		protected IGraphBuilder			m_sourceGraph=null;
		protected int					m_myCookie=0; // for the rot
		protected string				m_strAudioDevice="";
		protected string				m_sCurrentChannel="";
		
		public DVBRadioGraph(string strDevice,string strAudioDevice,string strLineInput)
		{
			m_strAudioDevice=strAudioDevice;
			m_db = new SQLiteClient(@"database\TVDatabaseV11.db");
		}

		public bool Create()
		{
			if (m_graphState != State.None) return false;
			// create graphs
			m_sourceGraph=(IGraphBuilder)  Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.FilterGraph, true ) );
			
			int n=0;
			m_b2c2Adapter=null;
			// create filters & interfaces
			try
			{
				m_b2c2Adapter=(IBaseFilter)Activator.CreateInstance( Type.GetTypeFromCLSID( CLSID_B2C2Adapter, false ) );
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
		private bool TuneCard(int Frequency,int SymbolRate,int FEC,int POL,int LNBKhz,int Diseq,int AudioPID,int VideoPID,int LNBFreq)
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
				DeleteAllPIDs(m_dataCtrl,0);
				hr = m_avCtrl.SetAudioVideoPIDs (AudioPID, 0);
				if (hr!=0)
				{
					return false;	// *** FUNCTION EXIT POINT
				}


			}
			return true;
		}
		//
		public string Channel
		{
			get { return m_sCurrentChannel;}
		}


		/// <summary>
		/// Deletes the current DirectShow graph created with Create()
		/// </summary>
		/// <remarks>
		/// Graph must be created first with CreateGraph()
		/// </remarks>
		public void DeleteGraph()
		{
			if (m_graphState < State.Created) return;
			DirectShowUtil.DebugWrite("DVBRadioGraph:DeleteGraph()");
			if (m_mediaControl != null)
			{
				m_mediaControl.Stop();
				m_mediaControl = null;
			}

			//DsROT.RemoveGraphFromRot(ref m_myCookie);
			
			m_myCookie=0;

			DsUtils.RemoveFilters(m_sourceGraph);
			//
			// release all interfaces and pins
			//
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
		private bool SetVideoAudioPins()
		{
			int hr=0;
			PinInfo pInfo=new PinInfo();

			// audio pin
			hr=DsUtils.GetPin(m_b2c2Adapter,PinDirection.Output,1,out m_audioPin);
			if(hr!=0)
				return false;

			if(m_audioPin==null)
			{
				Log.Write("DVBSS2: pins not found on adapter");
				return false;
			}
			m_audioPin.QueryPinInfo(out pInfo);

			// data pins

			return true;
		}
		//

		//

		//
		//
		public void TuneChannel(int channel)
		{


			int freq=0;int symrate=0;int fec=0;int lnbkhz=0;int diseqc=0;
			int prognum=0;int servicetype=0;string provider="";string schannel="";int eitsched=0;
			int eitprefol=0;int audpid=0;int vidpid=0;int ac3pid=0;int apid1=0;int apid2=0;int apid3=0;
			int teltxtpid=0;int scrambled=0;int pol=0;int lnbfreq=0;int networkid=0;int tsid=0;int pcrpid=0;
	  
	  
			if (m_db==null) return ;
			lock (typeof(DVBRadioGraph))
			{
				try
				{
					string strSQL;
					strSQL=String.Format("select * from satchannels where idChannel={0} and sServiceType=2",(channel>>16));
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count!=1) return ;
					else
					{
						int i=0;
						freq=Int32.Parse(Get(results,i,"sFreq"));
						symrate=Int32.Parse(Get(results,i,"sSymbrate"));
						fec=Int32.Parse(Get(results,i,"sFEC"));
						lnbkhz=Int32.Parse(Get(results,i,"sLNBKhz"));
						diseqc=Int32.Parse(Get(results,i,"sDiseqc"));
						prognum=Int32.Parse(Get(results,i,"sProgramNumber"));
						servicetype=Int32.Parse(Get(results,i,"sServiceType"));
						provider=Get(results,i,"sProviderName");
						schannel=Get(results,i,"sChannelName");
						eitsched=Int32.Parse(Get(results,i,"sEitSched"));
						eitprefol= Int32.Parse(Get(results,i,"sEitPreFol"));
						audpid=Int32.Parse(Get(results,i,"sAudioPid"));
						vidpid=Int32.Parse(Get(results,i,"sVideoPid"));
						ac3pid=Int32.Parse(Get(results,i,"sAC3Pid"));
						apid1= Int32.Parse(Get(results,i,"sAudio1Pid"));
						apid2= Int32.Parse(Get(results,i,"sAudio2Pid"));
						apid3=Int32.Parse(Get(results,i,"sAudio3Pid"));
						teltxtpid=Int32.Parse(Get(results,i,"sTeletextPid"));
						scrambled= Int32.Parse(Get(results,i,"sScrambled"));
						pol=Int32.Parse(Get(results,i,"sPol"));
						lnbfreq=Int32.Parse(Get(results,i,"sLNBFreq"));
						networkid=Int32.Parse(Get(results,i,"sNetworkID"));
						tsid=Int32.Parse(Get(results,i,"sTSID"));
						pcrpid=Int32.Parse(Get(results,i,"sPCRPid"));
						m_sCurrentChannel=schannel;
						
						if(TuneCard(freq,symrate,fec,pol,lnbkhz,diseqc,audpid,0,lnbfreq)==false)
						{
							DeleteGraph();
							return;
						}

					}
				}
				catch(Exception)
				{
					return;
				}
			}
					
			
			if(m_mediaControl!=null)
				m_mediaControl.Run();
			
			
		}
		static string Get(SQLiteResultSet results,int iRecord,string strColum)
		{
			lock (typeof(DVBRadioGraph))
			{
				if (null==results) return "";
				if (results.Rows.Count<iRecord) return "";
				ArrayList arr=(ArrayList)results.Rows[iRecord];
				int iCol=0;
				foreach (string columnName in results.ColumnNames)
				{
					if (strColum==columnName)
					{
						return ((string)arr[iCol]).Trim();
					}
					iCol++;
				}
				return "";
			}
		}

		public bool Tune(int channel)
		{
			if (m_graphState != State.Created) return false;
			TuneChannel(channel);
			int hr=0;

			// render here for listen
			if(m_tunerCtrl.CheckLock()!=0)
				return false;
			try
			{
				hr=m_sourceGraph.Render(m_audioPin);
				if(hr!=0)
					return false;
				int n=0;// 

				m_mediaControl = (IMediaControl)m_sourceGraph;
				//
				n=m_mediaControl.Run();

				m_graphState = State.Listening;
			}
			catch
			{}
			return true;
		}


		//
		public bool SignalPresent()
		{
			return true;
		}
		public int AudioFrequency
		{
			get{return 0;}	
		}

		/// <summary>
		/// This method returns the frequency to which the tv tuner is currently tuned
		/// </summary>
		/// <returns>frequency in Hertz
		/// </returns>

	}
}	  



