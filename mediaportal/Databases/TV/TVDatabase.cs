using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;
using DShowNET;
using MediaPortal.Database;

namespace MediaPortal.TV.Database
{
	/// <summary>
	/// Singleton class which implements the TVdatabase
	/// The TVDatabase stores and retrieves all information about TV channels, TV shows, scheduled recordings
	/// and Recorded shows
	/// </summary>
 
	public class TVDatabase
	{
		class CGenreCache
		{
			public int idGenre=0;
			public string strGenre="";
		};
		class CChannelCache
		{
			public int idChannel=0;
			public int iChannelNr=0;
			public string strChannel="";
		};

		static SQLiteClient m_db=null;
		static ArrayList m_genreCache=new ArrayList();
		static ArrayList m_channelCache=new ArrayList();
		static bool      m_bSupressEvents=false;
		static bool      m_bProgramsChanged=false;
		static bool      m_bRecordingsChanged=false;
		public delegate void OnChangedHandler();
		static public event OnChangedHandler OnProgramsChanged=null;
		static public event OnChangedHandler OnRecordingsChanged=null;

		/// <summary>
		/// private constructor to prevent any instance of this class
		/// </summary>
		private TVDatabase()
		{
		}

		/// <summary>
		/// static constructor. Opens or creates the tv database from database\TVDatabaseV6.db
		/// </summary>
		static TVDatabase()
		{
			lock (typeof(TVDatabase))
			{
				try 
				{
					// Open database
					Log.Write("opening tvdatabase");
					try
					{
						System.IO.Directory.CreateDirectory("database");
					}
					catch(Exception){}
					//Upgrade();
					m_db = new SQLiteClient(@"database\TVDatabaseV12.db");
					CreateTables();
					UpdateFromPreviousVersion();
					if (m_db!=null)
					{
						m_db.Execute("PRAGMA cache_size=8192\n");
						m_db.Execute("PRAGMA synchronous='OFF'\n");
						m_db.Execute("PRAGMA count_changes='OFF'\n");
					}

				} 
				catch (Exception ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				Log.Write("tvdatabase opened");
			}
		}

		static void UpdateFromPreviousVersion()
		{
			if (null==m_db) return ;
			m_db.Execute("update channel set iChannelNr="+((int)ExternalInputs.svhs).ToString() +" where strChannel like 'SVHS'");
			m_db.Execute("update channel set iChannelNr="+((int)ExternalInputs.cvbs1).ToString()+" where strChannel like 'Composite #1'");
			m_db.Execute("update channel set iChannelNr="+((int)ExternalInputs.cvbs2).ToString()+" where strChannel like 'Composite #2'");
		}
  
		/// <summary>
		/// clears the tv channel & genre cache
		/// </summary>
		static public void ClearCache()
		{
			m_channelCache.Clear();
			m_genreCache.Clear();
		}
		/// <summary>
		/// Checks if tables already exists in the database, if not creates the missing tables
		/// </summary>
		/// <returns>true : tables created</returns>
		static bool CreateTables()
		{
			lock (typeof(TVDatabase))
			{
				if (m_db==null) return false;
				if ( DatabaseUtility.AddTable(m_db,"channel","CREATE TABLE channel ( idChannel integer primary key, strChannel text, iChannelNr integer, frequency text, iSort integer, bExternal integer, ExternalChannel text, standard integer, Visible integer, Country integer)\n"))
				{
					m_db.Execute("CREATE INDEX idxChannel ON channel(iChannelNr)");
				}
        
        if ( DatabaseUtility.AddTable(m_db,"tblPrograms","CREATE TABLE tblPrograms ( idProgram integer primary key, idChannel integer, idGenre integer, strTitle text, iStartTime text, iEndTime text, strDescription text,strEpisodeName text,strRepeat text,strSeriesNum text,strEpisodeNum text,strEpisodePart text,strDate text,strStarRating text,strClassification text)\n"))
				{
					m_db.Execute("CREATE INDEX idxProgram ON tblPrograms(idChannel,iStartTime,iEndTime)");
				}

				if ( DatabaseUtility.AddTable(m_db,"genre","CREATE TABLE genre ( idGenre integer primary key, strGenre text)\n"))
				{
					m_db.Execute("CREATE INDEX idxGenre ON genre(strGenre)");
				}

				DatabaseUtility.AddTable(m_db,"recording","CREATE TABLE recording ( idRecording integer primary key, idChannel integer, iRecordingType integer, strProgram text, iStartTime text, iEndTime text, iCancelTime text, bContentRecording integer)\n");

				DatabaseUtility.AddTable(m_db,"recorded","CREATE TABLE recorded ( idRecorded integer primary key, idChannel integer, idGenre integer, strProgram text, iStartTime text, iEndTime text, strDescription text, strFileName text, iPlayed integer)\n");
		
				DatabaseUtility.AddTable(m_db,"tblSatChannels" ,"CREATE TABLE tblSatChannels ( idChannel integer,sPCRPid integer,sTSID integer,sFreq integer,sSymbrate integer,sFEC integer,sLNBKhz integer,sDiseqc integer,sProgramNumber integer,sServiceType integer,sProviderName text,sChannelName text,sEitSched integer,sEitPreFol integer,sAudioPid integer,sVideoPid integer,sAC3Pid integer,sAudio1Pid integer,sAudio2Pid integer,sAudio3Pid integer,sTeletextPid integer,sScrambled integer,sPol integer,sLNBFreq integer,sNetworkID integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text,sECMPid integer,sPMTPid integer)\n");
				DatabaseUtility.AddTable(m_db,"tblDVBCMapping" ,"CREATE TABLE tblDVBCMapping ( idChannel integer primary key, strChannel text, iLCN integer, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer)\n");
				DatabaseUtility.AddTable(m_db,"tblDVBSMapping" ,"CREATE TABLE tblDVBSMapping ( idChannel integer primary key, strChannel text, iLCN integer, frequency text, symbolrate integer, innerFec integer, polarisation integer, ONID integer, TSID integer, SID integer, Visible integer)\n");
				DatabaseUtility.AddTable(m_db,"tblDVBTMapping" ,"CREATE TABLE tblDVBTMapping ( idChannel integer primary key, strChannel text, iLCN integer, frequency text, bandwidth integer, ONID integer, TSID integer, SID integer, Visible integer)\n");
				DatabaseUtility.AddTable(m_db,"tblGroups"      ,"CREATE TABLE tblGroups ( idGroup integer primary key, strName text, iSort integer, Pincode integer)\n");
				DatabaseUtility.AddTable(m_db,"tblGroupMapping","CREATE TABLE tblGroupMapping( idGroupMapping integer primary key, idGroup integer, idChannel integer)\n");

				//following table specifies which channels can be received by which card
				DatabaseUtility.AddTable(m_db,"tblChannelCard" ,"CREATE TABLE tblChannelCard( idChannelCard integer primary key, idChannel integer, card integer)\n");
				return true;
			}

		}
		//b2c2

		static public int AddSatChannel(int idChannel,int freq,int symrate,int fec,int lnbkhz,int diseqc,
			int prognum,int servicetype,string provider,string channel,int eitsched,
			int eitprefol,int audpid,int vidpid,int ac3pid,int apid1,int apid2,int apid3,
			int teltxtpid,int scrambled,int pol,int lnbfreq,int networkid,int tsid,int pcrpid,string aLangCode,string aLangCode1,string aLangCode2,string aLangCode3,int ecmPid,int pmtPid)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					DatabaseUtility.RemoveInvalidChars(ref provider);
					DatabaseUtility.RemoveInvalidChars(ref channel);

					string strChannel=channel;
					SQLiteResultSet results=null;

					strSQL=String.Format( "select * from tblSatChannels ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblSatChannels where idChannel = {0} and sServiceType={1}", idChannel,servicetype);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						//

						// fields in tblSatChannels
						// idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid
						// parameter (8)(9)
						//idChannel,freq,symrate, fec,lnbkhz,diseqc,
						//prognum,servicetype,provider,channel, eitsched,
						//eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
						// teltxtpid,scrambled, pol,lnbfreq,networkid

						strSQL=String.Format("insert into tblSatChannels (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})", 
							idChannel,freq,symrate, fec,lnbkhz,diseqc,
							prognum,servicetype,provider,channel, eitsched,
							eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
							teltxtpid,scrambled, pol,lnbfreq,networkid,tsid,pcrpid,aLangCode,aLangCode1,aLangCode2,aLangCode3,ecmPid,pmtPid);
					  
						m_db.Execute(strSQL);
						return 0;
					}
					else
					{
						return -1;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}
		static public int AddSatChannel(DVBChannel ch)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{

					SQLiteResultSet results=null;

					strSQL=String.Format( "select * from tblSatChannels ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblSatChannels where idChannel = {0} and sServiceType={1}", ch.ID,ch.ServiceType);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						//

						// fields in tblSatChannels
						// idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid
						// parameter (8)(9)
						//idChannel,freq,symrate, fec,lnbkhz,diseqc,
						//prognum,servicetype,provider,channel, eitsched,
						//eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
						// teltxtpid,scrambled, pol,lnbfreq,networkid

						strSQL=String.Format("insert into tblSatChannels (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})", 
							ch.ID,ch.Frequency,ch.Symbolrate,ch.FEC,ch.LNBKHz,ch.DiSEqC,
							ch.ProgramNumber,ch.ServiceType,ch.ServiceProvider,ch.ServiceName,(ch.HasEITSchedule==true?1:0),
							(ch.HasEITPresentFollow==true?1:0), ch.AudioPid,ch.VideoPid,ch.AC3Pid,ch.Audio1, ch.Audio2, ch.Audio3,
							ch.TeletextPid,(ch.IsScrambled==true?1:0), ch.Polarity,ch.LNBFrequency,ch.NetworkID,ch.TransportStreamID,ch.PCRPid,ch.AudioLanguage,ch.AudioLanguage1,ch.AudioLanguage2,ch.AudioLanguage3,ch.ECMPid,ch.PMTPid);
					  
						m_db.Execute(strSQL);
						return 0;
					}
					else
					{
						return -1;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}
		//b2c2
		static public string GetSatChannelName(int program_number,int network_id,int ts_id)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				string channelName="";
				try
				{
					if (null==m_db) return "";

					SQLiteResultSet results;
					strSQL=String.Format( "select * from tblSatChannels where sProgramNumber={0} and sNetworkID={1} and sTSID={2}", program_number,network_id,ts_id);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) return "";
					channelName=DatabaseUtility.Get(results,0,"sChannelName");
					return channelName;
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return "";
			}
		}
		//b2c2
		static public bool GetSatChannel(int idChannel,int serviceType,ref DVBChannel retChannel)
		{
		  
			int freq=0;int symrate=0;int fec=0;int lnbkhz=0;int diseqc=0;
			int prognum=0;int servicetype=0;string provider="";string channel="";int eitsched=0;
			int eitprefol=0;int audpid=0;int vidpid=0;int ac3pid=0;int apid1=0;int apid2=0;int apid3=0;
			int teltxtpid=0;int scrambled=0;int pol=0;int lnbfreq=0;int networkid=0;int tsid=0;int pcrpid=0;
			string audioLang;string audioLang1;string audioLang2;string audioLang3;int ecm;int pmt;
	  
	  
			if (m_db==null) return false;
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from tblSatChannels where idChannel={0} and sServiceType={1}",idChannel,serviceType);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count<1) return false;
					else
					{
						//chan.ID=Int32.Parse(DatabaseUtility.Get(results,i,"idChannel"));
						//chan.Number = Int32.Parse(DatabaseUtility.Get(results,i,"iChannelNr"));
						// sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName
						//sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,
						//sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID
						int i=0;
						freq=Int32.Parse(DatabaseUtility.Get(results,i,"sFreq"));
						symrate=Int32.Parse(DatabaseUtility.Get(results,i,"sSymbrate"));
						fec=Int32.Parse(DatabaseUtility.Get(results,i,"sFEC"));
						lnbkhz=Int32.Parse(DatabaseUtility.Get(results,i,"sLNBKhz"));
						diseqc=Int32.Parse(DatabaseUtility.Get(results,i,"sDiseqc"));
						prognum=Int32.Parse(DatabaseUtility.Get(results,i,"sProgramNumber"));
						servicetype=Int32.Parse(DatabaseUtility.Get(results,i,"sServiceType"));
						provider=DatabaseUtility.Get(results,i,"sProviderName");
						channel=DatabaseUtility.Get(results,i,"sChannelName");
						eitsched=Int32.Parse(DatabaseUtility.Get(results,i,"sEitSched"));
						eitprefol= Int32.Parse(DatabaseUtility.Get(results,i,"sEitPreFol"));
						audpid=Int32.Parse(DatabaseUtility.Get(results,i,"sAudioPid"));
						vidpid=Int32.Parse(DatabaseUtility.Get(results,i,"sVideoPid"));
						ac3pid=Int32.Parse(DatabaseUtility.Get(results,i,"sAC3Pid"));
						apid1= Int32.Parse(DatabaseUtility.Get(results,i,"sAudio1Pid"));
						apid2= Int32.Parse(DatabaseUtility.Get(results,i,"sAudio2Pid"));
						apid3=Int32.Parse(DatabaseUtility.Get(results,i,"sAudio3Pid"));
						teltxtpid=Int32.Parse(DatabaseUtility.Get(results,i,"sTeletextPid"));
						scrambled= Int32.Parse(DatabaseUtility.Get(results,i,"sScrambled"));
						pol=Int32.Parse(DatabaseUtility.Get(results,i,"sPol"));
						lnbfreq=Int32.Parse(DatabaseUtility.Get(results,i,"sLNBFreq"));
						networkid=Int32.Parse(DatabaseUtility.Get(results,i,"sNetworkID"));
						tsid=Int32.Parse(DatabaseUtility.Get(results,i,"sTSID"));
						pcrpid=Int32.Parse(DatabaseUtility.Get(results,i,"sPCRPid"));
						// sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid
						audioLang=DatabaseUtility.Get(results,i,"sAudioLang");
						audioLang1=DatabaseUtility.Get(results,i,"sAudioLang1");
						audioLang2=DatabaseUtility.Get(results,i,"sAudioLang2");
						audioLang3=DatabaseUtility.Get(results,i,"sAudioLang3");
						ecm=Int32.Parse(DatabaseUtility.Get(results,i,"sECMPid"));
						pmt=Int32.Parse(DatabaseUtility.Get(results,i,"sPMTPid"));
						retChannel=new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
							prognum, servicetype,provider, channel, eitsched,
							eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
							teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid,audioLang,audioLang1,audioLang2,audioLang3,ecm,pmt);

					}

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}


		static public int GetChannelId(string strChannel)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return -1;

					foreach (CChannelCache cache in m_channelCache)
					{
						if (cache.strChannel==strChannel) return cache.idChannel;
					}
          
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel where strChannel like '{0}'", strChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) return -1;
					int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
					CChannelCache chan=new CChannelCache();
					chan.idChannel=iNewID;
					chan.strChannel=DatabaseUtility.Get(results,0,"strChannel");
					chan.iChannelNr=Int32.Parse(DatabaseUtility.Get(results,0,"iChannelNr"));
					m_channelCache.Add(chan);
					return iNewID;
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}

		static public int GetChannelId(int iChannelNr)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return -1;

					foreach (CChannelCache cache in m_channelCache)
					{
						if (cache.iChannelNr==iChannelNr) return cache.idChannel;
					}
					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel where iChannelNr={0}", iChannelNr);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) return -1;
					int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
					CChannelCache chan=new CChannelCache();
					chan.idChannel=iNewID;
					chan.strChannel=DatabaseUtility.Get(results,0,"strChannel");
					chan.iChannelNr=iChannelNr;
					m_channelCache.Add(chan);
					return iNewID;
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}

		static public void SetChannelNumber(string strChannel,int iNumber)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					ClearCache();
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					SQLiteResultSet results;
					strSQL=String.Format( "update channel set iChannelNr={0} where strChannel like '{1}'", iNumber,strChannel);
					results=m_db.Execute(strSQL);
				}
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void SetChannelFrequency(string strChannel,string strFreq)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					ClearCache();
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					SQLiteResultSet results;
					strSQL=String.Format( "update channel set frequency='{0}' where strChannel like '{1}'", strFreq,strChannel);
					results=m_db.Execute(strSQL);
				}
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public void SetChannelSort(string strChannel,int iPlace)
		{
			if (m_db==null) return;
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					ClearCache();
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					SQLiteResultSet results;
					strSQL=String.Format( "update channel set iSort={0} where strChannel like '{1}'", iPlace,strChannel);
					results=m_db.Execute(strSQL);

				}
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void UpdateSatChannel(DVBChannel ch)
		{

			lock (typeof(TVDatabase))
			{
				try
				{
					string strChannel=ch.ServiceName;
					string strProvider=ch.ServiceProvider;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					DatabaseUtility.RemoveInvalidChars(ref strProvider);

					if (null==m_db) return ;

					RemoveSatChannel(ch);
					AddSatChannel(ch);
					/*				  strSQL=String.Format( "update tblSatChannels set (sFreq={0},sSymbrate={1},sFEC={2},sLNBKhz={3},sDiseqc={4},sProgramNumber={5},sServiceType={6},sProviderName='{7}',sChannelName='{8}',sEitSched={9},sEitPreFol={10},sAudioPid={11},sVideoPid={12},sAC3Pid={13},sAudio1Pid={14},sAudio2Pid={15},sAudio3Pid={16},sTeletextPid={17},sScrambled={18},sPol={19},sLNBFreq={20},sNetworkID={21},sTSID={22},sPCRPid={23}) where idChannel = {24}", 
											ch.Frequency,ch.Symbolrate, ch.FEC,ch.LNBKHz,ch.DiSEqC,
											ch.ProgramNumber,ch.ServiceType,strProvider,strChannel, (int)(ch.HasEITSchedule==true?1:0),
											(int)(ch.HasEITPresentFollow==true?1:0), ch.AudioPid,ch.VideoPid,ch.AC3Pid,ch.Audio1, ch.Audio2, ch.Audio3,
											ch.TeletextPid,(int)(ch.IsScrambled==true?1:0), ch.Polarity,ch.LNBFrequency,ch.NetworkID,ch.TransportStreamID,ch.PCRPid,ch.ID);
									*/
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public void UpdateChannel(TVChannel channel, int sort)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strChannel=channel.Name;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					if (null==m_db) return ;
					m_channelCache.Clear();
					string strExternal=channel.ExternalTunerChannel;
					DatabaseUtility.RemoveInvalidChars(ref strExternal);

					int iExternal=0;
					if (channel.External) iExternal=1;
					int iVisible=0;
					if (channel.VisibleInGuide) iVisible=1;


					strSQL=String.Format( "update channel set iChannelNr={0}, frequency={1}, iSort={2},bExternal={3}, ExternalChannel='{4}',standard={5}, Visible={6}, Country={7} where strChannel like '{8}'", 
						channel.Number,channel.Frequency.ToString(),
						sort,iExternal, strExternal, (int)channel.TVStandard, iVisible, channel.Country,
						strChannel);
					m_db.Execute(strSQL);
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public int AddChannel(TVChannel channel)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strChannel=channel.Name;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);

					if (null==m_db) return -1;

					foreach (CChannelCache cache in m_channelCache)
					{
						if (cache.strChannel==strChannel) return cache.idChannel;
					}
					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from channel where strChannel like '{0}'", strChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						string strExternal=channel.ExternalTunerChannel;
						DatabaseUtility.RemoveInvalidChars(ref strExternal);
						int iExternal=0;
						if (channel.External) iExternal=1;
						int iVisible=0;
						if (channel.VisibleInGuide) iVisible=1;
						strSQL=String.Format("insert into channel (idChannel, strChannel,iChannelNr ,frequency,iSort, bExternal, ExternalChannel,standard, Visible, Country) values ( NULL, '{0}', {1}, {2}, {3}, {4},'{5}', {6}, {7}, {8} )", 
							strChannel,channel.Number,channel.Frequency.ToString(),
							totalchannels+1,iExternal, strExternal, (int)channel.TVStandard, iVisible,channel.Country);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						CChannelCache chan=new CChannelCache();
						chan.idChannel=iNewID;
						chan.strChannel=strChannel;
						chan.iChannelNr=channel.Number;
						m_channelCache.Add(chan);
						channel.ID=iNewID;
						return iNewID;
					}
					else
					{
						int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
						CChannelCache chan=new CChannelCache();
						chan.idChannel=iNewID;
						chan.strChannel=strChannel;
						chan.iChannelNr=channel.Number;
						m_channelCache.Add(chan);
						channel.ID=iNewID;
						return iNewID;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}

		static public int AddGenre(string strGenre1)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					foreach (CGenreCache genre in m_genreCache)
					{
						if (genre.strGenre==strGenre1)
							return genre.idGenre;
					}
					string strGenre=strGenre1;
					DatabaseUtility.RemoveInvalidChars(ref strGenre);
					if (null==m_db) return -1;

					SQLiteResultSet results;
					strSQL=String.Format( "select * from genre where strGenre like '{0}'", strGenre);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into genre (idGenre, strGenre) values ( NULL, '{0}' )", 
							strGenre);
						m_db.Execute(strSQL);
						int iNewId= m_db.LastInsertID();
						CGenreCache genre= new CGenreCache();
						genre.idGenre = iNewId;
						genre.strGenre = strGenre1;
						m_genreCache.Add(genre);
						return iNewId;

					}
					else
					{
						int iID=Int32.Parse(DatabaseUtility.Get(results,0,"idGenre"));
						CGenreCache genre= new CGenreCache();
						genre.idGenre = iID;
						genre.strGenre = strGenre1;
						m_genreCache.Add(genre);
						return iID;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}

		static public int AddProgram(TVProgram prog)
		{
			int lRetId=-1;
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strGenre=prog.Genre;
					string strTitle=prog.Title;
					string strDescription=prog.Description;
					string strEpisode=prog.Episode;
					string strRepeat=prog.Repeat;
          string strDate=prog.Date;
          string strSeriesNum=prog.SeriesNum;
          string strEpisodeNum=prog.EpisodeNum;
          string strEpisodePart=prog.EpisodePart;
          string strStarRating=prog.StarRating;
          string strClassification=prog.Classification;
					DatabaseUtility.RemoveInvalidChars(ref strGenre);
					DatabaseUtility.RemoveInvalidChars(ref strTitle);
					DatabaseUtility.RemoveInvalidChars(ref strDescription);
					DatabaseUtility.RemoveInvalidChars(ref strEpisode);
					DatabaseUtility.RemoveInvalidChars(ref strRepeat);
          DatabaseUtility.RemoveInvalidChars(ref strDate);
          DatabaseUtility.RemoveInvalidChars(ref strSeriesNum);
          DatabaseUtility.RemoveInvalidChars(ref strEpisodeNum);
          DatabaseUtility.RemoveInvalidChars(ref strEpisodePart);
          DatabaseUtility.RemoveInvalidChars(ref strStarRating);
          DatabaseUtility.RemoveInvalidChars(ref strClassification);


					if (null==m_db) return -1;
					int iGenreId=AddGenre(strGenre);
					int iChannelId=GetChannelId(prog.Channel);
					if (iChannelId<0) return -1;
          
					strSQL=String.Format("insert into tblPrograms (idProgram,idChannel,idGenre,strTitle,iStartTime,iEndTime,strDescription,strEpisodeName,strRepeat) values ( NULL, {0}, {1}, '{2}', {3}, '{4}', '{5}', '{6}', '{7}')", 
						iChannelId,iGenreId,strTitle,prog.Start.ToString(),
						prog.End.ToString(), strDescription,strEpisode,strRepeat);
					m_db.Execute(strSQL);
					lRetId=m_db.LastInsertID();
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

			}
			ProgramsChanged();
			return lRetId;
		}
		static public int UpdateProgram(TVProgram prog)
		{
			int lRetId=-1;
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strGenre=prog.Genre;
					string strTitle=prog.Title;
					string strDescription=prog.Description;
					string strEpisode=prog.Episode;
					string strRepeat=prog.Repeat;
          string strSeriesNum=prog.SeriesNum;
          string strEpisodeNum=prog.EpisodeNum;
          string strEpisodePart=prog.EpisodePart;
          string strDate=prog.Date;
          string strStarRating=prog.StarRating;
          string strClassification=prog.Classification;
					DatabaseUtility.RemoveInvalidChars(ref strGenre);
					DatabaseUtility.RemoveInvalidChars(ref strTitle);
					DatabaseUtility.RemoveInvalidChars(ref strDescription);
					DatabaseUtility.RemoveInvalidChars(ref strEpisode);
					DatabaseUtility.RemoveInvalidChars(ref strRepeat);
          DatabaseUtility.RemoveInvalidChars(ref strSeriesNum);
          DatabaseUtility.RemoveInvalidChars(ref strEpisodeNum);
          DatabaseUtility.RemoveInvalidChars(ref strEpisodePart);
          DatabaseUtility.RemoveInvalidChars(ref strDate);
          DatabaseUtility.RemoveInvalidChars(ref strStarRating);
          DatabaseUtility.RemoveInvalidChars(ref strClassification);

					if (null==m_db) return -1;
					int iGenreId=AddGenre(strGenre);
					int iChannelId=GetChannelId(prog.Channel);
					if (iChannelId<0) return -1;
					
					//check if program is already in database
					strSQL=String.Format("SELECT * FROM tblPrograms WHERE idChannel={0} AND idGenre={1} AND strTitle='{2}' AND iStartTime={3} AND iEndTime={4} AND strDescription='{5}'", 
						iChannelId,iGenreId,strTitle,prog.Start.ToString(),
						prog.End.ToString(), strDescription);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count> 0)
					{
						//						Log.Write("Program Exists Ignoring:{0} {1} {2}", strTitle,prog.Start.ToString(),prog.End.ToString());
						return -1; //exit if the same program exists
					}
					//check if other programs exist between the start - finish time of this program
					strSQL=String.Format("SELECT strTitle,iStartTime,iEndTime FROM tblPrograms WHERE idChannel={0} AND iStartTime>={1} AND iEndTime<={2}", 
						iChannelId,prog.Start.ToString(),prog.End.ToString());
					SQLiteResultSet results2;
					results2=m_db.Execute(strSQL);
					if (results2.Rows.Count> 0) //and delete them
						for (int i=0; i < results2.Rows.Count;++i)
						{
							long iStart=Int64.Parse(DatabaseUtility.Get(results2,i,"iStartTime"));
							long iEnd=Int64.Parse(DatabaseUtility.Get(results2,i,"iEndTime"));
							//							Log.Write("Removing Program:{0} {1} {2}", DatabaseUtility.Get(results2,i,"strTitle"),iStart.ToString(),iEnd.ToString());
							strSQL=String.Format("DELETE FROM tblPrograms WHERE idChannel={0} AND iStartTime={1} AND iEndTime={2}",
								iChannelId,iStart,iEnd);
							m_db.Execute(strSQL);
						}

					// then add the new shows
					strSQL=String.Format("insert into tblPrograms (idProgram,idChannel,idGenre,strTitle,iStartTime,iEndTime,strDescription,strEpisodeName,strRepeat,strSeriesNum,strEpisodeNum,strEpisodePart,strDate,strStarRating,strClassification) values ( NULL, {0}, {1}, '{2}', {3}, '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')", 
						iChannelId,iGenreId,strTitle,prog.Start.ToString(),
						prog.End.ToString(), strDescription, strEpisode, strRepeat,strSeriesNum,strEpisodeNum,strEpisodePart,strDate,strStarRating,strClassification);
					m_db.Execute(strSQL);
					lRetId=m_db.LastInsertID();
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

			}
			ProgramsChanged();
			return lRetId;
		}
		static public bool GetSatChannels(ref ArrayList channels)
		{
			if (m_db==null) return false;
			int idChannel=0;int freq=0;int symrate=0;int fec=0;int lnbkhz=0;int diseqc=0;
			int prognum=0;int servicetype=0;string provider="";string channel="";int eitsched=0;
			int eitprefol=0;int audpid=0;int vidpid=0;int ac3pid=0;int apid1=0;int apid2=0;int apid3=0;
			int teltxtpid=0;int scrambled=0;int pol=0;int lnbfreq=0;int networkid=0;int tsid=0;int pcrpid=0;
			string audioLang="";string audioLang1="";string audioLang2="";string audioLang3="";int ecm=0;int pmt=0;
			lock (typeof(TVDatabase))
			{
				channels.Clear();
				try
				{
					if (null==m_db) return false;
					DVBChannel dvbChannel=new DVBChannel();
					string strSQL;
					strSQL=String.Format("select * from tblSatChannels order by sChannelName");
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
					  
						idChannel=Int32.Parse(DatabaseUtility.Get(results,i,"idChannel"));
						freq=Int32.Parse(DatabaseUtility.Get(results,i,"sFreq"));
						symrate=Int32.Parse(DatabaseUtility.Get(results,i,"sSymbrate"));
						fec=Int32.Parse(DatabaseUtility.Get(results,i,"sFEC"));
						lnbkhz=Int32.Parse(DatabaseUtility.Get(results,i,"sLNBKhz"));
						diseqc=Int32.Parse(DatabaseUtility.Get(results,i,"sDiseqc"));
						prognum=Int32.Parse(DatabaseUtility.Get(results,i,"sProgramNumber"));
						servicetype=Int32.Parse(DatabaseUtility.Get(results,i,"sServiceType"));
						provider=DatabaseUtility.Get(results,i,"sProviderName");
						channel=DatabaseUtility.Get(results,i,"sChannelName");
						eitsched=Int32.Parse(DatabaseUtility.Get(results,i,"sEitSched"));
						eitprefol= Int32.Parse(DatabaseUtility.Get(results,i,"sEitPreFol"));
						audpid=Int32.Parse(DatabaseUtility.Get(results,i,"sAudioPid"));
						vidpid=Int32.Parse(DatabaseUtility.Get(results,i,"sVideoPid"));
						ac3pid=Int32.Parse(DatabaseUtility.Get(results,i,"sAC3Pid"));
						apid1= Int32.Parse(DatabaseUtility.Get(results,i,"sAudio1Pid"));
						apid2= Int32.Parse(DatabaseUtility.Get(results,i,"sAudio2Pid"));
						apid3=Int32.Parse(DatabaseUtility.Get(results,i,"sAudio3Pid"));
						teltxtpid=Int32.Parse(DatabaseUtility.Get(results,i,"sTeletextPid"));
						scrambled= Int32.Parse(DatabaseUtility.Get(results,i,"sScrambled"));
						pol=Int32.Parse(DatabaseUtility.Get(results,i,"sPol"));
						lnbfreq=Int32.Parse(DatabaseUtility.Get(results,i,"sLNBFreq"));
						networkid=Int32.Parse(DatabaseUtility.Get(results,i,"sNetworkID"));
						tsid=Int32.Parse(DatabaseUtility.Get(results,i,"sTSID"));
						pcrpid=Int32.Parse(DatabaseUtility.Get(results,i,"sPCRPid"));
						audioLang=DatabaseUtility.Get(results,i,"sAudioLang");
						audioLang1=DatabaseUtility.Get(results,i,"sAudioLang1");
						audioLang2=DatabaseUtility.Get(results,i,"sAudioLang2");
						audioLang3=DatabaseUtility.Get(results,i,"sAudioLang3");
						ecm=Int32.Parse(DatabaseUtility.Get(results,i,"sECMPid"));
						pmt=Int32.Parse(DatabaseUtility.Get(results,i,"sPMTPid"));
						dvbChannel=new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
							prognum, servicetype,provider, channel, eitsched,
							eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
							teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid,audioLang,audioLang1,audioLang2,audioLang3,ecm,pmt);
						channels.Add(dvbChannel);
					}

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}
		static public bool GetChannels(ref ArrayList channels)
		{
			if (m_db==null) return false;
			lock (typeof(TVDatabase))
			{
				channels.Clear();
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from channel order by iSort");
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						TVChannel chan=new TVChannel();
						chan.ID=Int32.Parse(DatabaseUtility.Get(results,i,"idChannel"));
						chan.Number = Int32.Parse(DatabaseUtility.Get(results,i,"iChannelNr"));
						decimal dFreq=0;
						try
						{
							dFreq = (decimal)Int64.Parse(DatabaseUtility.Get(results,i,"frequency"));

						}
						catch(Exception)
						{
							chan.Frequency =0;
						}
						dFreq /= 1000000M;
						dFreq=Math.Round(dFreq,3);
						dFreq *=1000000M;
						chan.Frequency = (long)Math.Round(dFreq,0);
						chan.Name = DatabaseUtility.Get(results,i,"strChannel");
						int iExternal=Int32.Parse(DatabaseUtility.Get(results,i,"bExternal"));
						if (iExternal!=0) chan.External=true;
						else chan.External=false;

						int iVisible=Int32.Parse(DatabaseUtility.Get(results,i,"Visible"));
						if (iVisible!=0) chan.VisibleInGuide=true;
						else chan.VisibleInGuide=false;

						chan.ExternalTunerChannel= DatabaseUtility.Get(results,i,"ExternalChannel");
						chan.TVStandard = (AnalogVideoStandard)Int32.Parse(DatabaseUtility.Get(results,i,"standard"));
						chan.Country=Int32.Parse(DatabaseUtility.Get(results,i,"Country"));
						channels.Add(chan);
					}

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}
    
		static public bool GetGenres(ref ArrayList genres)
		{
			lock (typeof(TVDatabase))
			{
				genres.Clear();
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from genre");
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						genres.Add( DatabaseUtility.Get(results,i,"strGenre") );
					}

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}

		static public void RemoveChannel(string strChannel)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return ;
				int iChannelId=GetChannelId(strChannel);
				if (iChannelId<0) return ;
        
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from tblPrograms where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);
          
					strSQL=String.Format("delete from recording where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);
          
					strSQL=String.Format("delete from channel where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);
					
					strSQL=String.Format("delete from tblGroupMapping where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);

					strSQL = String.Format("delete from tblDVBSMapping where iLCN={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping where iLCN={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping where iLCN={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblChannelCard where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
			m_channelCache.Clear();
			ProgramsChanged();
			RecordingsChanged();
		}
		static public void RemoveSatChannel(DVBChannel ch)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return ;
        
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from tblSatChannels where idChannel={0}",ch.ID);
					m_db.Execute(strSQL);
				  
			  
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void RemoveAllSatChannels()
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return ;
        
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from tblSatChannels");
					m_db.Execute(strSQL);
				  
			  
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void RemovePrograms()
		{
			lock (typeof(TVDatabase))
			{
				m_genreCache.Clear();
				m_channelCache.Clear();
				try
				{
					if (null==m_db) return ;
					m_db.Execute("delete from tblPrograms");
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
			ProgramsChanged();
		}

		static public bool SearchProgramsPerGenre(string genre1, ArrayList progs, int SearchKind, string SearchCriteria)
		{
			progs.Clear();
			if (genre1==null) return false;
			string genre=genre1;
			DatabaseUtility.RemoveInvalidChars(ref genre);
			DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
			string strSQL=String.Empty;
			switch(SearchKind)
			{
				case -1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' order by iStartTime",genre);
					break;
				case 0:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}%' order by iStartTime",genre,SearchCriteria);
					break;

				case 1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}%' order by iStartTime",genre,SearchCriteria);
					break;
        
				case 2:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}' order by iStartTime",genre,SearchCriteria);
					break;
        
				case 3:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}' order by iStartTime",genre,SearchCriteria);
					break;

			}
			return GetTVProgramsByGenre(strSQL, progs);
		}

		static public bool GetProgramsPerGenre(string genre1, ArrayList progs)
		{
			return SearchProgramsPerGenre(genre1, progs, -1, String.Empty);
		}

		static bool GetTVProgramsByGenre(string strSQL, ArrayList progs)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return false;

					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					long lTimeStart=Utils.datetolong(DateTime.Now);
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iEndTime"));
						bool bAdd=false;
						if (iEnd >= lTimeStart) bAdd=true;
						if (bAdd)
						{
							TVProgram prog=new TVProgram();
							prog.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
							prog.Start=iStart;
							prog.End=iEnd;
							prog.Genre=DatabaseUtility.Get(results,i,"genre.strGenre");
							prog.Title=DatabaseUtility.Get(results,i,"tblPrograms.strTitle");
							prog.Description=DatabaseUtility.Get(results,i,"tblPrograms.strDescription");
							prog.Episode=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeName");
							prog.Repeat=DatabaseUtility.Get(results,i,"tblPrograms.strRepeat");
							prog.ID=Int32.Parse(DatabaseUtility.Get(results,i,"tblPrograms.idProgram"));
              prog.SeriesNum=DatabaseUtility.Get(results,i,"tblPrograms.strSeriesNum");
              prog.EpisodeNum=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeNum");
              prog.EpisodePart=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodePart");
              prog.Date=DatabaseUtility.Get(results,i,"tblPrograms.strDate");
              prog.StarRating=DatabaseUtility.Get(results,i,"tblPrograms.strStarRating");
              prog.Classification=DatabaseUtility.Get(results,i,"tblPrograms.strClassification");
							progs.Add(prog);
						}
					}

					return true;

				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}

		static public bool GetProgramsPerChannel(string strChannel1,long iStartTime, long iEndTime,ref ArrayList progs)
		{
			lock (typeof(TVDatabase))
			{
				progs.Clear();
				try
				{
					if (null==m_db) return false;
					if (strChannel1==null) return false;
					string strChannel=strChannel1;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);

					string strSQL;
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and channel.strChannel like '{0}' order by iStartTime",
						strChannel);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iEndTime"));
						bool bAdd=false;
						if (iEnd >=iStartTime && iEnd <= iEndTime) bAdd=true;
						if (iStart >=iStartTime && iStart <= iEndTime) bAdd=true;
						if (iStart <=iStartTime && iEnd>=iEndTime) bAdd=true;
						if (bAdd)
						{
							TVProgram prog=new TVProgram();
							prog.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
							prog.Start=iStart;
							prog.End=iEnd;
							prog.Genre=DatabaseUtility.Get(results,i,"genre.strGenre");
							prog.Title=DatabaseUtility.Get(results,i,"tblPrograms.strTitle");
							prog.Description=DatabaseUtility.Get(results,i,"tblPrograms.strDescription");
							prog.Episode=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeName");
							prog.Repeat=DatabaseUtility.Get(results,i,"tblPrograms.strRepeat");
							prog.ID=Int32.Parse(DatabaseUtility.Get(results,i,"tblPrograms.idProgram"));
              prog.SeriesNum=DatabaseUtility.Get(results,i,"tblPrograms.strSeriesNum");
              prog.EpisodeNum=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeNum");
              prog.EpisodePart=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodePart");
              prog.Date=DatabaseUtility.Get(results,i,"tblPrograms.strDate");
              prog.StarRating=DatabaseUtility.Get(results,i,"tblPrograms.strStarRating");
              prog.Classification=DatabaseUtility.Get(results,i,"tblPrograms.strClassification");
							progs.Add(prog);
						}
					}

					return true;

				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}


		static bool GetTVPrograms(long iStartTime, long iEndTime,string strSQL, ref ArrayList progs)
		{
			lock (typeof(TVDatabase))
			{
				progs.Clear();
				try
				{
					if (null==m_db) return false;

					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iEndTime"));
						bool bAdd=false;
						if (iEnd >=iStartTime && iEnd <= iEndTime) bAdd=true;
						if (iStart >=iStartTime && iStart <= iEndTime) bAdd=true;
						if (iStart <=iStartTime && iEnd>=iEndTime) bAdd=true;
						if (bAdd)
						{
							TVProgram prog=new TVProgram();
							prog.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
							prog.Start=iStart;
							prog.End=iEnd;
							prog.Genre=DatabaseUtility.Get(results,i,"genre.strGenre");
							prog.Title=DatabaseUtility.Get(results,i,"tblPrograms.strTitle");
							prog.Description=DatabaseUtility.Get(results,i,"tblPrograms.strDescription");
							prog.Episode=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeName");
							prog.Repeat=DatabaseUtility.Get(results,i,"tblPrograms.strRepeat");
							prog.ID=Int32.Parse(DatabaseUtility.Get(results,i,"tblPrograms.idProgram"));
              prog.SeriesNum=DatabaseUtility.Get(results,i,"tblPrograms.strSeriesNum");
              prog.EpisodeNum=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeNum");
              prog.EpisodePart=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodePart");
              prog.Date=DatabaseUtility.Get(results,i,"tblPrograms.strDate");
              prog.StarRating=DatabaseUtility.Get(results,i,"tblPrograms.strStarRating");
              prog.Classification=DatabaseUtility.Get(results,i,"tblPrograms.strClassification");
							progs.Add(prog);
						}
					}

					return true;

				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}

		static public bool SearchPrograms(long iStartTime, long iEndTime,ref ArrayList progs, int SearchKind, string SearchCriteria)
		{
			DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
			string strSQL=String.Empty;
			switch (SearchKind)
			{
				case -1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel  order by iStartTime");
					break;
				case 0:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}%' order by iStartTime", SearchCriteria);
					break;
				case 1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}%' order by iStartTime", SearchCriteria);
					break;
				case 2:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}' order by iStartTime", SearchCriteria);
					break;
				case 3:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}' order by iStartTime", SearchCriteria);
					break;
			}
			return GetTVPrograms(iStartTime, iEndTime, strSQL, ref progs);
		}

		static public bool SearchProgramsByDescription(long iStartTime, long iEndTime,ref ArrayList progs, int SearchKind, string SearchCriteria)
		{
			DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
			string strSQL=String.Empty;
			switch (SearchKind)
			{
				case -1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel  order by iStartTime");
					break;
				case 0:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}%' order by iStartTime", SearchCriteria);
					break;
				case 1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}%' order by iStartTime", SearchCriteria);
					break;
				case 2:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}' order by iStartTime", SearchCriteria);
					break;
				case 3:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}' order by iStartTime", SearchCriteria);
					break;
			}
			return GetTVPrograms(iStartTime, iEndTime, strSQL, ref progs);
		}

		static public bool GetPrograms(long iStartTime, long iEndTime,ref ArrayList progs)
		{
			return SearchPrograms(iStartTime, iEndTime,ref progs,-1,String.Empty);
		}

		static public void ChangeRecording(ref TVRecording recording)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strTitle=recording.Title;
					DatabaseUtility.RemoveInvalidChars(ref strTitle);

					if (null==m_db) return ;
					int iChannelId=GetChannelId(recording.Channel);
					if (iChannelId<0) return ;
					if (recording.ID<0) return ;
          
					int iContentRec=1;
					if (!recording.IsContentRecording) iContentRec=0;

					strSQL=String.Format("update recording set idChannel={0},iRecordingType={1},strProgram='{2}',iStartTime='{3}',iEndTime='{4}', iCancelTime='{5}', bContentRecording={6} where idRecording={7}", 
						iChannelId,
						(int)recording.RecType,
						strTitle,
						recording.Start.ToString(),
						recording.End.ToString(),
						recording.Canceled.ToString(),
						iContentRec,
						recording.ID);
					m_db.Execute(strSQL);
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
			RecordingsChanged();
		}

  
		static public int AddRecording(ref TVRecording recording)
		{
			int lNewId=-1;
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strTitle=recording.Title;
					DatabaseUtility.RemoveInvalidChars(ref strTitle);

					if (null==m_db) return -1;
					int iChannelId=GetChannelId(recording.Channel);
					if (iChannelId<0) return -1;
					int iContentRec=1;
					if (!recording.IsContentRecording) iContentRec=0;
          
					strSQL=String.Format("insert into recording (idRecording,idChannel,iRecordingType,strProgram,iStartTime,iEndTime,iCancelTime,bContentRecording ) values ( NULL, {0}, {1}, '{2}','{3}', '{4}', '{5}', {6})", 
						iChannelId,
						(int)recording.RecType,
						strTitle,
						recording.Start.ToString(),
						recording.End.ToString(),
						recording.Canceled.ToString(),
						iContentRec);
					m_db.Execute(strSQL);
					lNewId=m_db.LastInsertID();
					recording.ID=lNewId;
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
			RecordingsChanged();
			return lNewId;
		}

		static public void RemoveRecording(TVRecording record)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from recording where idRecording={0}",record.ID);
					m_db.Execute(strSQL);
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
			RecordingsChanged();
		}

		static public void RemoveRecordingByTime(TVRecording record)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return ;
					int iChannelId=GetChannelId(record.Channel);
					if (iChannelId<0) return ;

					string strSQL=String.Format("delete from recording where iStartTime like {0} and iEndTime like {1} and idChannel={2}",
						record.Start,record.End,iChannelId);
					m_db.Execute(strSQL);
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
			RecordingsChanged();
		}
		static public bool GetRecordingsForChannel(string strChannel1,long iStartTime, long iEndTime,ref ArrayList recordings)
		{
			lock (typeof(TVDatabase))
			{
				recordings.Clear();
				try
				{
					if (null==m_db) return false;
					string strChannel=strChannel1;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);

					string strSQL;
					strSQL=String.Format("select * from channel,recording where recording.idChannel=channel.idChannel and channel.strChannel like '{0}' order by iStartTime",strChannel);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"recording.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"recording.iEndTime"));
						bool bAdd=false;
						if (iEnd >=iStartTime && iEnd <= iEndTime) bAdd=true;
						if (iStart >=iStartTime && iStart <= iEndTime) bAdd=true;
						if (iStart <=iStartTime && iEnd>=iEndTime) bAdd=true;
						if (bAdd)
						{
							TVRecording rec=new TVRecording ();
							rec.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
							rec.Start=iStart;
							rec.End=iEnd;
							rec.Canceled = Int64.Parse(DatabaseUtility.Get(results,i,"recording.iCancelTime"));
							rec.ID=Int32.Parse(DatabaseUtility.Get(results,i,"recording.idRecording"));
							rec.Title=DatabaseUtility.Get(results,i,"recording.strProgram");
							rec.RecType=(TVRecording.RecordingType)Int32.Parse(DatabaseUtility.Get(results,i,"recording.iRecordingType"));
							int iContectRec=Int32.Parse(DatabaseUtility.Get(results,i,"recording.bContentRecording"));
							if (iContectRec==1) rec.IsContentRecording=true;
							else rec.IsContentRecording=false;
							recordings.Add(rec);
						}
					}

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}

		static public bool GetRecordings(ref ArrayList recordings)
		{
			lock (typeof(TVDatabase))
			{
				recordings.Clear();
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from channel,recording where recording.idChannel=channel.idChannel order by iStartTime");
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"recording.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"recording.iEndTime"));
						TVRecording rec=new TVRecording ();
						rec.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
						rec.Start=iStart;
						rec.End=iEnd;
						rec.Canceled = Int64.Parse(DatabaseUtility.Get(results,i,"recording.iCancelTime"));
						rec.ID=Int32.Parse(DatabaseUtility.Get(results,i,"recording.idRecording"));
						rec.Title=DatabaseUtility.Get(results,i,"recording.strProgram");
						rec.RecType=(TVRecording.RecordingType)Int32.Parse(DatabaseUtility.Get(results,i,"recording.iRecordingType"));
						int iContectRec=Int32.Parse(DatabaseUtility.Get(results,i,"recording.bContentRecording"));
						if (iContectRec==1) rec.IsContentRecording=true;
						else rec.IsContentRecording=false;
						recordings.Add(rec);
					}

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}

		static public void BeginTransaction()
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					m_db.Execute("begin");
				}
				catch (Exception ex)
				{
					Log.Write("tvdatabase begin transaction failed exception err:{0} ", ex.Message);		
				}
			}
		}
    
		static public void CommitTransaction()
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					m_db.Execute("commit");
				}
				catch (Exception ex)
				{
					Log.Write("tvdatabase commit failed exception err:{0} ", ex.Message);		
				}
			}
		}
    
		static public void RollbackTransaction()
		{
			try
			{
				m_db.Execute("rollback");
			}
			catch (Exception ex)
			{
				Log.Write("tvdatabase rollback failed exception err:{0} ", ex.Message);		
			}
		}

		static public void PlayedRecordedTV( TVRecorded record)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("update recorded set iPlayed={0} where idRecorded={1}",record.Played, record.ID);
					m_db.Execute(strSQL);
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}

		}
		static public int AddRecordedTV( TVRecorded recording)
		{
			int lNewId=-1;
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strTitle=recording.Title;
					string strDescription=recording.Description;
					string strFileName=recording.FileName;
					DatabaseUtility.RemoveInvalidChars(ref strTitle);
					DatabaseUtility.RemoveInvalidChars(ref strDescription);
					DatabaseUtility.RemoveInvalidChars(ref strFileName);

					if (null==m_db) return -1;
					int iChannelId=GetChannelId(recording.Channel);
					if (iChannelId<0) return -1;
					int iGenreId=AddGenre(recording.Genre);
					if (iGenreId<0) return -1;
          
					strSQL=String.Format("insert into recorded (idRecorded,idChannel,idGenre,strProgram,iStartTime,iEndTime,strDescription,strFileName,iPlayed) values ( NULL, {0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', {7})", 
						iChannelId,
						iGenreId,
						strTitle,
						recording.Start.ToString(),
						recording.End.ToString(),
						strDescription,
						strFileName,
						recording.Played);
					m_db.Execute(strSQL);
					lNewId=m_db.LastInsertID();
					recording.ID=lNewId;
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
			return lNewId;
		}



		static public void RemoveRecordedTV(TVRecorded record)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from recorded where idRecorded={0}",record.ID);
					m_db.Execute(strSQL);
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		} 


		static public bool GetRecordedTV(ref ArrayList recordings)
		{
			lock (typeof(TVDatabase))
			{
				recordings.Clear();
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from channel,genre,recorded where recorded.idChannel=channel.idChannel and genre.idGenre=recorded.idGenre order by iStartTime");
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"recorded.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"recorded.iEndTime"));
						TVRecorded rec=new TVRecorded ();
						rec.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
						rec.Start=iStart;
						rec.End=iEnd;
						rec.ID=Int32.Parse(DatabaseUtility.Get(results,i,"recorded.idRecorded"));
						rec.Title=DatabaseUtility.Get(results,i,"recorded.strProgram");
						rec.Description=DatabaseUtility.Get(results,i,"recorded.strDescription");
						rec.FileName=DatabaseUtility.Get(results,i,"recorded.strFileName");
						rec.Genre=DatabaseUtility.Get(results,i,"genre.strGenre");
						rec.Played=Int32.Parse(DatabaseUtility.Get(results,i,"recorded.iPlayed"));
						recordings.Add(rec);
					}

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}

		/// <summary>
		/// Retrieves the TVRecorded record from the database for the filename of a specific record tvprogram.
		/// </summary>
		/// <param name="strFile">Contains the filename of the recorded tv program.</param>
		/// <param name="recording">Contains the <see>MediaPortal.TV.Database.TVRecorded</see> record for the filename when found</param>
		/// <returns>true if the recording is found in the tvdatabase else false</returns>
		/// <seealso>MediaPortal.TV.Database.TVRecorded</seealso>
		static public bool GetRecordedTVByFilename(string strFile,ref TVRecorded recording)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return false;
					DatabaseUtility.RemoveInvalidChars(ref strFile);
					string strSQL;
					strSQL=String.Format("select * from channel,genre,recorded where recorded.idChannel=channel.idChannel and genre.idGenre=recorded.idGenre and recorded.strFileName='{0}'", strFile);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					long iStart=Int64.Parse(DatabaseUtility.Get(results,0,"recorded.iStartTime"));
					long iEnd=Int64.Parse(DatabaseUtility.Get(results,0,"recorded.iEndTime"));
           
					recording.Channel=DatabaseUtility.Get(results,0,"channel.strChannel");
					recording.Start=iStart;
					recording.End=iEnd;
					recording.ID=Int32.Parse(DatabaseUtility.Get(results,0,"recorded.idRecorded"));
					recording.Title=DatabaseUtility.Get(results,0,"recorded.strProgram");
					recording.Description=DatabaseUtility.Get(results,0,"recorded.strDescription");
					recording.FileName=DatabaseUtility.Get(results,0,"recorded.strFileName");
					recording.Genre=DatabaseUtility.Get(results,0,"genre.strGenre");
					recording.Played=Int32.Parse(DatabaseUtility.Get(results,0,"recorded.iPlayed"));

					return true;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return false;
			}
		}

		static public bool SupressEvents
		{
			get { return m_bSupressEvents;}
			set 
			{ 
				m_bSupressEvents=value;
				if (!m_bSupressEvents)
				{
					if (m_bProgramsChanged)  ProgramsChanged();
					if (m_bRecordingsChanged)  RecordingsChanged();
				}
			}
		}
		static void RecordingsChanged()
		{
			m_bRecordingsChanged=true;
			if (!m_bSupressEvents)
			{
				if (OnRecordingsChanged!=null) OnRecordingsChanged();
				m_bRecordingsChanged=false;
			}
		}

		static void ProgramsChanged()
		{
			m_bProgramsChanged=true;
			if (!m_bSupressEvents)
			{
				if (OnProgramsChanged!=null) OnProgramsChanged();
				m_bProgramsChanged=false;
			}
		}

		static public string GetLastProgramEntry()
		{
			lock (typeof(TVDatabase))
			{
				try
				{	// for single thread multi day grab establish last guide data in db
					if (m_db==null) return "";
					string strSQL;
					strSQL=String.Format("select iendtime from tblPrograms order by iendtime desc limit 1");
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return "";
  					
					ArrayList arr=(ArrayList)results.Rows[0];
					return ((string)arr[0]).Trim();
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return "";
			}
		}

		/// <summary>
		/// RemoveOldPrograms()
		/// Deletes all tv programs from the database which ended more then 1 day ago
		/// suppose its now 10 november 2004 11:07 am
		/// then this function will remove all programs which endtime is before 9 november 2004 11:07
		/// </summary>
		static public void RemoveOldPrograms()
		{
			lock (typeof(TVDatabase))
			{
				//delete programs from database that are more than 1 day old
				try
				{ 
					string strSQL;
					System.DateTime yesterday = System.DateTime.Today.AddDays(-1);
					long longYesterday = Utils.datetolong(yesterday);
					strSQL=String.Format("DELETE FROM tblPrograms WHERE iEndTime < {0}",longYesterday);
					m_db.Execute(strSQL);
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return;
			}
		}

		/// <summary>
		/// GetAllPrograms() returns all tv programs found in the database ordered by channel,starttime
		/// </summary>
		/// <param name="programs">Arraylist containing TVProgram instances</param>
		static public void GetAllPrograms(out ArrayList progs)
		{
			progs = new ArrayList();
			lock (typeof(TVDatabase))
			{
				try
				{
					//get all programs
					if (null==m_db) return ;

					SQLiteResultSet results;
					results=m_db.Execute("select * from tblPrograms,channel,genre where tblPrograms.idGenre=genre.idGenre and tblPrograms.idChannel = channel.idChannel order by tblPrograms.idChannel, tblPrograms.iStartTime");
					if (results.Rows.Count== 0) return ;
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iEndTime"));
						TVProgram prog=new TVProgram();
						prog.Start=iStart;
						prog.End=iEnd;
						prog.ID=Int32.Parse(DatabaseUtility.Get(results,i,"tblPrograms.idProgram"));

						prog.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
						prog.Genre=DatabaseUtility.Get(results,i,"genre.strGenre");
						prog.Title=DatabaseUtility.Get(results,i,"tblPrograms.strTitle");
						prog.Description=DatabaseUtility.Get(results,i,"tblPrograms.strDescription");
						prog.Episode=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeName");
						prog.Repeat=DatabaseUtility.Get(results,i,"tblPrograms.strRepeat");
            prog.SeriesNum=DatabaseUtility.Get(results,i,"tblPrograms.strSeriesNum");
            prog.EpisodeNum=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeNum");
            prog.EpisodePart=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodePart");
            prog.Date=DatabaseUtility.Get(results,i,"tblPrograms.strDate");
            prog.StarRating=DatabaseUtility.Get(results,i,"tblPrograms.strStarRating");
            prog.Classification=DatabaseUtility.Get(results,i,"tblPrograms.strClassification");
						progs.Add(prog);
					}
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		/// <summary>
		/// OffsetProgramsByHour() will correct the start/end time of all programs in the tvdatabase
		/// </summary>
		/// <param name="Hours">Number of hours to correct (can be positive or negative)</param>
		static public void OffsetProgramsByMinutes(int Minutes)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					ArrayList progs;
					GetAllPrograms(out progs);

					//correct time offsets
					foreach (TVProgram program in progs)
					{
						DateTime dtStart=program.StartTime;
						DateTime dtEnd=program.EndTime;
						dtStart=dtStart.AddMinutes(Minutes);
						dtEnd=dtEnd.AddMinutes(Minutes);
						program.Start=Utils.datetolong(dtStart);
						program.End=Utils.datetolong(dtEnd);
          
						string sql=String.Format("update tblPrograms set iStartTime='{0}' , iEndTime='{1}' where idProgram={2}",
							program.Start, program.End, program.ID);
						m_db.Execute(sql);
					}
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		/// <summary>
		/// This function will check all tv programs in the database and
		/// will remove any overlapping programs
		/// An overlapping program is a tv program which overlaps with another tv program in time
		/// for example 
		///   program A on MTV runs from 20.00-21.00 on 1 november 2004
		///   program B on MTV runs from 20.55-22.00 on 1 november 2004
		///   this case, program B will be removed
		/// </summary>
		static public void RemoveOverlappingPrograms()
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					//first get a list of all tv channels
					ArrayList channels = new ArrayList();
					GetChannels(ref channels);

					long endTime=Utils.datetolong( new DateTime(2100,1,1,0,0,0,0));
					foreach (TVChannel channel in channels)
					{
						// for each tv channel get all programs
						ArrayList progs = new ArrayList();
						GetProgramsPerChannel(channel.Name, 0, endTime, ref progs);

						long previousEnd  =0;
						long previousStart=0;
						foreach (TVProgram program in progs)
						{
							bool overlap=false;
							if (previousEnd > program.Start) overlap=true;
							if (overlap)
							{
								//remove this program
								string sql=String.Format("delete from tblPrograms where idProgram={0}",program.ID);
								m_db.Execute(sql);
							}
							else
							{
								previousEnd  = program.End;
								previousStart= program.Start;
							}
						}
					}
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public int MapDVBTChannel(string channelName, int idChannel, int frequency, int ONID, int TSID, int SID)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return -1;
				string strSQL;
				try
				{
					string strChannel=channelName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);

					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBTMapping where iLCN like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBTMapping (idChannel, strChannel , iLCN , frequency , bandwidth , ONID , TSID , SID , Visible) Values( NULL, '{0}', {1}, '{2}',{3},{4},{5},{6},1)",strChannel,idChannel,frequency,0,ONID,TSID,SID);
						//Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBTMapping set frequency='{0}', ONID={1}, TSID={2}, SID={3}, strChannel='{4}' where iLCN like '{5}'", frequency,ONID,TSID,SID,strChannel, idChannel);
						//	Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}
		
		static public int MapDVBCChannel(string channelName, int idChannel, int frequency, int symbolrate,int innerFec, int modulation,int ONID, int TSID, int SID)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return -1;
				string strSQL;
				try
				{
					string strChannel=channelName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);

					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBCMapping where iLCN like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBCMapping (idChannel, strChannel,iLCN,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,Visible) Values( NULL, '{0}', {1}, '{2}',{3},{4},{5},{6},{7},{8},1)"
							,strChannel,idChannel,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID);
						//Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}' where iLCN like '{8}'", 
							frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,strChannel, idChannel);
						//	Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}

		static public int MapDVBSChannel(string channelName, int idChannel, int frequency, int symbolrate,int innerFec, int polarisation,int ONID, int TSID, int SID)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return -1;
				string strSQL;
				try
				{
					string strChannel=channelName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);

					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBSMapping where iLCN like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBSMapping (idChannel, strChannel,iLCN,frequency,symbolrate,innerFec,polarisation,ONID,TSID,SID,Visible) Values( NULL, '{0}', {1}, '{2}',{3},{4},{5},{6},{7},{8},1)"
							,strChannel,idChannel,frequency,symbolrate,innerFec,polarisation,ONID,TSID,SID);
						//Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBSMapping set frequency='{0}', symbolrate={1}, innerFec={2}, polarisation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}' where iLCN like '{8}'", 
							frequency,symbolrate,innerFec,polarisation,ONID,TSID,SID,strChannel, idChannel);
						//	Log.Write("sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}

		static public void GetDVBTTuneRequest(int iLCN, out int frequency, out int ONID, out int TSID, out int SID) 
		{
			frequency=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			Log.Write("GetTuneRequest for iLCN:{0}", iLCN);
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBTMapping where iLCN={0}",iLCN);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=Int32.Parse(DatabaseUtility.Get(results,0,"frequency"));
					ONID=Int32.Parse(DatabaseUtility.Get(results,0,"ONID"));
					TSID=Int32.Parse(DatabaseUtility.Get(results,0,"TSID"));
					SID=Int32.Parse(DatabaseUtility.Get(results,0,"SID"));
					return ;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void GetDVBCTuneRequest(int iLCN, out int frequency,out int symbolrate,out int innerFec,out int modulation, out int ONID, out int TSID, out int SID) 
		{
			frequency=-1;
			symbolrate=-1;
			innerFec=-1;
			modulation=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			Log.Write("GetTuneRequest for iLCN:{0}", iLCN);
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBCMapping where iLCN={0}",iLCN);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=Int32.Parse(DatabaseUtility.Get(results,0,"frequency"));
					symbolrate=Int32.Parse(DatabaseUtility.Get(results,0,"symbolrate"));
					innerFec=Int32.Parse(DatabaseUtility.Get(results,0,"innerFec"));
					modulation=Int32.Parse(DatabaseUtility.Get(results,0,"modulation"));
					ONID=Int32.Parse(DatabaseUtility.Get(results,0,"ONID"));
					TSID=Int32.Parse(DatabaseUtility.Get(results,0,"TSID"));
					SID=Int32.Parse(DatabaseUtility.Get(results,0,"SID"));
					return ;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public void GetDVBSTuneRequest(int iLCN, out int frequency,out int symbolrate,out int innerFec,out int polarisation, out int ONID, out int TSID, out int SID) 
		{
			frequency=-1;
			symbolrate=-1;
			innerFec=-1;
			polarisation=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			Log.Write("GetTuneRequest for iLCN:{0}", iLCN);
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBSMapping where iLCN={0}",iLCN);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=Int32.Parse(DatabaseUtility.Get(results,0,"frequency"));
					symbolrate=Int32.Parse(DatabaseUtility.Get(results,0,"symbolrate"));
					innerFec=Int32.Parse(DatabaseUtility.Get(results,0,"innerFec"));
					polarisation=Int32.Parse(DatabaseUtility.Get(results,0,"polarisation"));
					ONID=Int32.Parse(DatabaseUtility.Get(results,0,"ONID"));
					TSID=Int32.Parse(DatabaseUtility.Get(results,0,"TSID"));
					SID=Int32.Parse(DatabaseUtility.Get(results,0,"SID"));
					return ;
				}
				catch(SQLiteException ex)
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public void GetGroups(ref ArrayList groups)
		{
			groups=new ArrayList();
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return ;
					SQLiteResultSet results;
					strSQL=String.Format( "select * from tblGroups order by iSort");
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return ;
					for (int i=0; i < results.Rows.Count;++i)
					{
						TVGroup group = new TVGroup();
						group.ID      =Int32.Parse(DatabaseUtility.Get(results,i,"idGroup"));
						group.Sort     =Int32.Parse(DatabaseUtility.Get(results,i,"iSort"));
						group.Pincode  =Int32.Parse(DatabaseUtility.Get(results,i,"Pincode"));
						group.GroupName=DatabaseUtility.Get(results,i,"strName");
						groups.Add(group);
					}
					foreach (TVGroup group in groups)
					{
						GetTVChannelsForGroup(group);
					}
				}
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		
		static public void GetTVChannelsForGroup(TVGroup group)
		{
			group.tvChannels.Clear();
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					ArrayList tvchannels = new ArrayList();
					GetChannels(ref tvchannels);

					if (null==m_db) return ;
					SQLiteResultSet results;
					strSQL=String.Format( "select * from tblGroupMapping,tblGroups where tblGroups.idGroup=tblGroupMapping.idGroup and tblGroupMapping.idGroup={0}", group.ID);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return ;
					for (int i=0; i < results.Rows.Count;++i)
					{
						int channelid=Int32.Parse(DatabaseUtility.Get(results,i,"tblGroupMapping.idChannel"));
						foreach (TVChannel chan in tvchannels)
						{
							if (chan.ID==channelid)
							{
								group.tvChannels.Add(chan);
								break;
							}
						}
					}
				}
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void MapChannelToCard(int channel, int card)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return ;
					SQLiteResultSet results;

					strSQL=String.Format( "select * from tblChannelCard where idChannel={0} and card={1}", channel,card);

					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblChannelCard (idChannelCard, idChannel,card) values ( NULL, {0}, {1})", 
																	channel,card);
						m_db.Execute(strSQL);
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void DeleteCard(int card)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{

					if (null==m_db) return ;
					//delete this card
					strSQL=String.Format( "delete from tblChannelCard where card={0}", card);
					m_db.Execute(strSQL);

					//adjust the mapping for the other cards
					strSQL=String.Format( "select * from tblChannelCard where card > {0}", card);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					for (int i=0; i < results.Rows.Count;++i)
					{
						int id    =Int32.Parse(DatabaseUtility.Get(results,i,"idChannelCard") );
						int cardnr=Int32.Parse(DatabaseUtility.Get(results,i,"card") );	
						cardnr--;
						strSQL=String.Format( "update tblChannelCard set card={0} where idChannelCard={1}", 
																cardnr, id);
						m_db.Execute(strSQL);
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}
		static public void UnmapChannelFromCard(TVChannel channel, int card)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{

					if (null==m_db) return ;
					strSQL=String.Format( "delete from tblChannelCard where idChannel={0} and card={1}", 
						channel.ID,card);

					m_db.Execute(strSQL);
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public void MapChannelToGroup(TVGroup group, TVChannel channel)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strgroupName=group.GroupName;
					DatabaseUtility.RemoveInvalidChars(ref strgroupName);

					if (null==m_db) return ;
					SQLiteResultSet results;

					strSQL=String.Format( "select * from tblGroupMapping where idGroup={0} and idChannel={1}", 
																	group.ID,channel.ID);

					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblGroupMapping (idGroupMapping, idGroup,idChannel) values ( NULL, {0}, {1})", 
																		group.ID,channel.ID);
						m_db.Execute(strSQL);
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public void UnmapChannelFromGroup(TVGroup group, TVChannel channel)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{

					if (null==m_db) return ;

					strSQL=String.Format( "delete from tblGroupMapping where idGroup={0} and idChannel={1}", 
						group.ID,channel.ID);

					m_db.Execute(strSQL);
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
			}
		}

		static public int AddGroup(TVGroup group)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strgroupName=group.GroupName;
					DatabaseUtility.RemoveInvalidChars(ref strgroupName);

					if (null==m_db) return -1;
					SQLiteResultSet results;
					strSQL=String.Format( "select * from tblGroups");
					results=m_db.Execute(strSQL);
					int totalgroups=results.Rows.Count;

					strSQL=String.Format( "select * from tblGroups where strName like '{0}'", strgroupName);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblGroups (idGroup, strName,iSort ,Pincode) values ( NULL, '{0}', {1}, {2})", 
																											strgroupName,totalgroups+1,group.Pincode);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return iNewID;
					}
					else
					{
						//exists, update it
						int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idGroup"));
						strSQL=String.Format( "update tblGroups set strName='{0}', iSort={1}, Pincode={2} where idGroup={3}", 
																   strgroupName, group.Sort,group.Pincode, iNewID);
						results=m_db.Execute(strSQL);
						return iNewID;
					}
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}

				return -1;
			}
		}

		static public int DeleteGroup(TVGroup group)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{

					if (null==m_db) return -1;

					strSQL=String.Format( "delete from tblGroupMapping where idGroup={0}", group.ID);
					m_db.Execute(strSQL);
					strSQL=String.Format( "delete from tblGroups where idGroup={0}", group.ID);
					m_db.Execute(strSQL);
				} 
				catch (SQLiteException ex) 
				{
					Log.Write("TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				return -1;
			}
		}
	}
}
