using System;
using System.ComponentModel;

namespace MediaPortal.TV.Database
{
	/// <summary>
	/// Zusammenfassung für ChannelClass.
	/// </summary>
	public class DVBChannel
	{
		public DVBChannel(		
			int idChannel,int sFreq ,int sSymbrate,int sFEC,int sLNBKhz,int sDiseqc,int sProgramNumber,int sServiceType,
			string sProviderName,string sChannelName,int sEitSched,
			int sEitPreFol,int sAudioPid,int sVideoPid,int sAC3Pid,int sAudio1Pid,int sAudio2Pid,int sAudio3Pid,int sTeletextPid,
			int sScrambled,int sPol,int sLNBFreq,int sNetworkID,int sTSID,int sPCRPid,string aLangCode,string aLangCode1,string aLangCode2,string aLangCode3,int ecm,int pmt)
		{
			 m_idChannel=idChannel;
			 m_sFreq =sFreq;
			 m_sSymbrate=sSymbrate;
			 m_sFEC=sFEC;
			 m_sLNBKhz=sLNBKhz;
			 m_sDiseqc=sDiseqc;
			 m_sProgramNumber=sProgramNumber;
			 m_sServiceType=sServiceType;
			 m_sProviderName=sProviderName;
			 m_sChannelName=sChannelName;
			 m_sEitSched= sEitSched;
			 m_sEitPreFol= sEitPreFol;
			 m_sAudioPid= sAudioPid;
			 m_sVideoPid= sVideoPid;
			 m_sAC3Pid= sAC3Pid;
			 m_sAudio1Pid=sAudio1Pid ;
			 m_sAudio2Pid= sAudio2Pid;
			 m_sAudio3Pid= sAudio3Pid;
			 m_sTeletextPid=sTeletextPid ;
			 m_sScrambled= sScrambled;
			 m_sPol= sPol;
			 m_sLNBFreq=sLNBFreq ;
			 m_sNetworkID=sNetworkID ;
			 m_sTSID= sTSID;
			 m_sPCRPid= sPCRPid;
			 m_sLangCodeAudio=aLangCode;
			 m_sLangCodeAudio1=aLangCode1;
			 m_sLangCodeAudio2=aLangCode2;
			 m_sLangCodeAudio3=aLangCode3;
			 m_sECMPid=ecm;
			 m_sPMTPid=pmt;
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
		}
		public DVBChannel()
		{
		}
		//
		int m_idChannel;
		int m_sFreq ;
		int m_sSymbrate;
		int m_sFEC;
		int m_sLNBKhz;
		int m_sDiseqc;
		int m_sProgramNumber;
		int m_sServiceType;
		string m_sProviderName;
		string m_sChannelName;
		int m_sEitSched;
		int m_sEitPreFol;
		int m_sAudioPid;
		int m_sVideoPid;
		int m_sAC3Pid;
		int m_sAudio1Pid;
		int m_sAudio2Pid;
		int m_sAudio3Pid;
		int m_sTeletextPid;
		int m_sScrambled;
		int m_sPol;
		int m_sLNBFreq;
		int m_sNetworkID;
		int m_sTSID;
		int m_sPCRPid;
		string m_sLangCodeAudio;
		string m_sLangCodeAudio1;
		string m_sLangCodeAudio2;
		string m_sLangCodeAudio3;
		int m_sECMPid;
		int m_sPMTPid;

		//
		[BrowsableAttribute(true),ReadOnly(true)]
		public int ID
		{
			get
			{return m_idChannel;}
			set
			{m_idChannel=value;}
		}		
		
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]
		public bool HasEITSchedule
		{
			get
			{return m_sEitSched==0?false:true;}
			set
			{m_sEitSched=(int)(value==true?1:0);}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]
		public bool HasEITPresentFollow
		{
			get
			{return m_sEitPreFol==0?false:true;}
			set
			{m_sEitPreFol=(int)(value==true?1:0);}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int PCRPid
		{
			get
			{return m_sPCRPid;}
			set
			{m_sPCRPid=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]

		public int TransportStreamID
		{
			get
			{return m_sTSID;}
			set
			{m_sTSID=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]

		public int NetworkID
		{
			get
			{return m_sNetworkID;}
			set
			{m_sNetworkID=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]

		public int Frequency
		{
			get
			{return m_sFreq;}
			set
			{m_sFreq=value;}
        }
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]
		public int Polarity
		{
			get
			{return m_sPol;}
			set
			{m_sPol=value;}
		}
		[BrowsableAttribute(true),Category("DiSEqC / LNB Config"),
		ReadOnly(false)]

		public int LNBKHz
		{
			get
			{return m_sLNBKhz;}
			set
			{m_sLNBKhz=value;}
		}
		[BrowsableAttribute(true),Category("DiSEqC / LNB Config"),
		ReadOnly(false)]

		public int DiSEqC
		{
			get
			{return m_sDiseqc;}
			set
			{m_sDiseqc=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]

		public int ProgramNumber
		{
			get
			{return m_sProgramNumber;}
			set
			{m_sProgramNumber=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]
		public int ServiceType
		{
			get
			{return m_sServiceType;}
			set
			{m_sServiceType=value;}
		}
		[BrowsableAttribute(true),Category("DiSEqC / LNB Config"),
		ReadOnly(false)]
		public int LNBFrequency
		{
			get
			{return m_sLNBFreq;}
			set
			{m_sLNBFreq=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]
		public bool IsScrambled
		{
			get
			{return m_sScrambled==0?false:true;}
			set
			{m_sScrambled=(int)(value==true?1:0);}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int Audio1
		{
			get
			{return m_sAudio1Pid;}
			set
			{m_sAudio1Pid=value;}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int Audio2
		{
			get
			{return m_sAudio2Pid;}
			set
			{m_sAudio2Pid=value;}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int Audio3
		{
			get
			{return m_sAudio3Pid;}
			set
			{m_sAudio3Pid=value;}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int TeletextPid
		{
			get
			{return m_sTeletextPid;}
			set
			{m_sTeletextPid=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public int Symbolrate
		{
			get
			{return m_sSymbrate;}
			set
			{m_sSymbrate=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public int FEC
		{
			get
			{return m_sFEC;}
			set
			{m_sFEC=value;}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int AudioPid
		{
			get
			{return m_sAudioPid;}
			set
			{m_sAudioPid=value;}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int VideoPid
		{
			get
			{return m_sVideoPid;}
			set
			{m_sVideoPid=value;}
		}
		[BrowsableAttribute(true),Category("Service PIDs"),
		ReadOnly(false)]
		public int AC3Pid
		{
			get
			{return m_sAC3Pid;}
			set
			{m_sAC3Pid=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]
		public string ServiceName
		{
			get
			{return m_sChannelName;}
			set
			{m_sChannelName=value;}
		}
		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(true)]
		public string ServiceProvider
		{
			get
			{return m_sProviderName;}
			set
			{m_sProviderName=value;}
		}

		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public string AudioLanguage
		{
			get
			{return m_sLangCodeAudio;}
			set
			{m_sLangCodeAudio=value;}
		}

		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public string AudioLanguage1
		{
			get
			{return m_sLangCodeAudio1;}
			set
			{m_sLangCodeAudio1=value;}
		}

		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public string AudioLanguage2
		{
			get
			{return m_sLangCodeAudio2;}
			set
			{m_sLangCodeAudio2=value;}
		}

		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public string AudioLanguage3
		{
			get
			{return m_sLangCodeAudio3;}
			set
			{m_sLangCodeAudio3=value;}
		}

		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public int ECMPid
		{
			get
			{return m_sECMPid;}
			set
			{m_sECMPid=value;}
		}

		[BrowsableAttribute(true),Category("Service Data"),
		ReadOnly(false)]
		public int PMTPid
		{
			get
			{return m_sPMTPid;}
			set
			{m_sPMTPid=value;}
		}

		public override string ToString()
		{
			return m_sChannelName;
		}


	}

}
