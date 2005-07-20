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

		public enum RecordingChange
		{
			Added,
			Deleted,
			Canceled,
			CanceledSerie,
			Modified,
			QualityChange,
			EpisodesToKeepChange,
			PriorityChange
		}
		static public SQLiteClient m_db=null;
		static ArrayList m_genreCache=new ArrayList();
		static ArrayList m_channelCache=new ArrayList();
		static bool      m_bSupressEvents=false;
		static bool      m_bProgramsChanged=false;
		static bool      m_bRecordingsChanged=false;
		public delegate void OnChangedHandler();
		public delegate void OnRecordingChangedHandler(RecordingChange change);
		static public event OnChangedHandler OnProgramsChanged=null;
		static public event OnRecordingChangedHandler OnRecordingsChanged=null;
		static public event OnChangedHandler OnNotifiesChanged=null;
		static public event OnChangedHandler OnChannelsChanged=null;

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
			Open();
		}
		static void Open()
		{
			lock (typeof(TVDatabase))
			{
				try 
				{
					// Open database
					Log.WriteFile(Log.LogType.Log,false,"opening tvdatabase");

					String strPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.
						GetExecutingAssembly().Location); 
					try
					{
						System.IO.Directory.CreateDirectory(strPath+@"\database");
					}
					catch(Exception){}
					//Upgrade();
					m_db = new SQLiteClient(strPath+@"\database\TVDatabaseV21.db3");
					if (m_db!=null)
					{
						m_db.Execute("PRAGMA cache_size=2000;\n");
						m_db.Execute("PRAGMA synchronous='OFF';\n");
						m_db.Execute("PRAGMA count_changes=1;\n");
						m_db.Execute("PRAGMA full_column_names=0;\n");
						m_db.Execute("PRAGMA short_column_names=0;\n");
					}
					CreateTables();
				
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				}
				Log.WriteFile(Log.LogType.Log,false,"tvdatabase opened");
			}
		}
		static public bool AddTable( string strTable, string strSQL)
		{
			//	lock (typeof(DatabaseUtility))
		
			Log.Write("AddTable: {0}",strTable);
			if (m_db==null) 
			{
				Log.Write("AddTable: database not opened");
				return false;
			}
			if (strSQL==null) 
			{
				Log.Write("AddTable: no sql?");
				return false;
			}
			if (strTable==null) 
			{
				Log.Write("AddTable: No table?");
				return false;
			}
			if (strTable.Length==0) 
			{
				Log.Write("AddTable: empty table?");
				return false;
			}
			if (strSQL.Length==0) 
			{
				Log.Write("AddTable: empty sql?");
				return false;
			}

			//Log.Write("check for  table:{0}", strTable);
			SQLiteResultSet results;
			results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+strTable+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
			if (results!=null)
			{
				Log.Write("  results:{0}", results.Rows.Count);
				if (results.Rows.Count==1) 
				{
					Log.Write(" check result:0");
					ArrayList arr = (ArrayList)results.Rows[0];
					if (arr.Count==1)
					{
						string tableName=((string)arr[0]).Trim();
						if ( String.Compare(tableName,strTable,true)==0) 
						{
							//Log.Write(" table exists");
							return false;
						}
						Log.Write(" table has different name:[{0}[ [{1}]", tableName,strTable);
					}
					else Log.Write(" array contains:{0} items?", arr.Count);
				}
			}

			try 
			{
				//Log.Write("create table:{0}", strSQL);
				m_db.Execute(strSQL);
				//Log.Write("table created");
			}
			catch (SQLiteException ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message,ex.StackTrace,strSQL);
			}
			return true;
		}

		static void UpdateFromPreviousVersion()
		{
			if (null==m_db) return ;
			m_db.Execute("update channel set iChannelNr="+((int)ExternalInputs.rgb).ToString() +" where strChannel like 'RGB'");
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
		static public void ClearAll()
		{
			ClearCache();
			m_db.Execute("delete from channel");
			m_db.Execute("delete from tblPrograms");
			m_db.Execute("delete from genre");
			m_db.Execute("delete from recording");
			m_db.Execute("delete from canceledseries");
			m_db.Execute("delete from recorded");
			m_db.Execute("delete from tblDVBSMapping");
			m_db.Execute("delete from tblDVBCMapping");
			m_db.Execute("delete from tblATSCMapping");
			m_db.Execute("delete from tblDVBTMapping");
			m_db.Execute("delete from tblGroups");
			m_db.Execute("delete from tblGroupMapping");
			m_db.Execute("delete from tblChannelCard");
			m_db.Execute("delete from tblNotifies");
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
				if ( AddTable("channel","CREATE TABLE channel ( idChannel integer primary key, strChannel text, iChannelNr integer, frequency text, iSort integer, bExternal integer, ExternalChannel text, standard integer, Visible integer, Country integer, scrambled integer);\n"))
				{
					try {
						m_db.Execute("CREATE INDEX idxChannel ON channel(iChannelNr);\n");
					}
					catch(Exception){} 
				}
        
        if ( AddTable("tblPrograms","CREATE TABLE tblPrograms ( idProgram integer primary key, idChannel integer, idGenre integer, strTitle text, iStartTime text, iEndTime text, strDescription text,strEpisodeName text,strRepeat text,strSeriesNum text,strEpisodeNum text,strEpisodePart text,strDate text,strStarRating text,strClassification text);\n"))
				{
					try
					{
						m_db.Execute("CREATE INDEX idxProgram ON tblPrograms(idChannel,iStartTime,iEndTime);\n");
					}
					catch(Exception){} 
				}

				if ( AddTable("genre","CREATE TABLE genre ( idGenre integer primary key, strGenre text);\n"))
				{
					try
					{
						m_db.Execute("CREATE INDEX idxGenre ON genre(strGenre);\n");
					}
					catch(Exception){} 
				}

				AddTable("recording","CREATE TABLE recording ( idRecording integer primary key, idChannel integer, iRecordingType integer, strProgram text, iStartTime text, iEndTime text, iCancelTime text, bContentRecording integer, priority integer, quality integer, episodesToKeep integer);\n");
				AddTable("canceledseries","CREATE TABLE canceledseries ( idRecording integer, idChannel integer, iCancelTime text);\n");

				AddTable("recorded","CREATE TABLE recorded ( idRecorded integer primary key, idChannel integer, idGenre integer, strProgram text, iStartTime text, iEndTime text, strDescription text, strFileName text, iPlayed integer);\n");
		
				AddTable("tblDVBSMapping" ,"CREATE TABLE tblDVBSMapping ( idChannel integer,sPCRPid integer,sTSID integer,sFreq integer,sSymbrate integer,sFEC integer,sLNBKhz integer,sDiseqc integer,sProgramNumber integer,sServiceType integer,sProviderName text,sChannelName text,sEitSched integer,sEitPreFol integer,sAudioPid integer,sVideoPid integer,sAC3Pid integer,sAudio1Pid integer,sAudio2Pid integer,sAudio3Pid integer,sTeletextPid integer,sScrambled integer,sPol integer,sLNBFreq integer,sNetworkID integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text,sECMPid integer,sPMTPid integer);\n");
				AddTable("tblDVBCMapping" ,"CREATE TABLE tblDVBCMapping ( idChannel integer primary key, strChannel text, strProvider text, iLCN integer, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer, pmtPid integer, ac3Pid integer, audio1Pid integer, audio2Pid integer, audio3Pid integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text, HasEITPresentFollow integer, HasEITSchedule integer);\n");
				AddTable("tblATSCMapping" ,"CREATE TABLE tblATSCMapping ( idChannel integer primary key, strChannel text, strProvider text, iLCN integer, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer, pmtPid integer, ac3Pid integer, audio1Pid integer, audio2Pid integer, audio3Pid integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text, channelNumber integer,minorChannel integer, majorChannel integer, HasEITPresentFollow integer, HasEITSchedule integer);\n");
				AddTable("tblDVBTMapping" ,"CREATE TABLE tblDVBTMapping ( idChannel integer primary key, strChannel text, strProvider text, iLCN integer, frequency text, bandwidth integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer, pmtPid integer, ac3Pid integer, audio1Pid integer, audio2Pid integer, audio3Pid integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text, HasEITPresentFollow integer, HasEITSchedule integer);\n");
				AddTable("tblGroups"      ,"CREATE TABLE tblGroups ( idGroup integer primary key, strName text, iSort integer, Pincode integer);\n");
				AddTable("tblGroupMapping","CREATE TABLE tblGroupMapping( idGroupMapping integer primary key, idGroup integer, idChannel integer);\n");

				//following table specifies which channels can be received by which card
				AddTable("tblChannelCard" ,"CREATE TABLE tblChannelCard( idChannelCard integer primary key, idChannel integer, card integer);\n");
				AddTable("tblNotifies"    ,"CREATE TABLE tblNotifies( idNotify integer primary key, idProgram integer)");
				return true;
			}

		}
		//------------------------------------------------------------------------------------------------
		//b2c2

		static public int AddSatChannel(int idChannel,int freq,int symrate,int fec,int lnbkhz,int diseqc,
			int prognum,int servicetype,string provider,string channel,int eitsched,
			int eitprefol,int audpid,int vidpid,int ac3pid,int apid1,int apid2,int apid3,
			int teltxtpid,int scrambled,int pol,int lnbfreq,int networkid,int tsid,int pcrpid,string aLangCode,string aLangCode1,string aLangCode2,string aLangCode3,int ecmPid,int pmtPid)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL=String.Empty;
				try
				{
					DatabaseUtility.RemoveInvalidChars(ref provider);
					DatabaseUtility.RemoveInvalidChars(ref channel);

					SQLiteResultSet results=null;

					strSQL=String.Format( "select * from tblDVBSMapping ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBSMapping where idChannel = {0} and sServiceType={1}", idChannel,servicetype);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						//

						// fields in tblDVBSMapping
						// idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid
						// parameter (8)(9)
						//idChannel,freq,symrate, fec,lnbkhz,diseqc,
						//prognum,servicetype,provider,channel, eitsched,
						//eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
						// teltxtpid,scrambled, pol,lnbfreq,networkid

						strSQL=String.Format("insert into tblDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})", 
							idChannel,freq,symrate, fec,lnbkhz,diseqc,
							prognum,servicetype,provider.Trim(),channel.Trim(), eitsched,
							eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
							teltxtpid,scrambled, pol,lnbfreq,networkid,tsid,pcrpid,
							aLangCode.Trim(),aLangCode1.Trim(),aLangCode2.Trim(),aLangCode3.Trim(),
							ecmPid,pmtPid);
					  
						m_db.Execute(strSQL);
						return 0;
					}
					else
					{
						return -1;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception SQL:{0} err:{1} stack:{2}", strSQL,ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}
		static public int AddSatChannel(DVBChannel ch)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL=String.Empty;
				try
				{

					SQLiteResultSet results=null;

					strSQL=String.Format( "select * from tblDVBSMapping ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBSMapping where idChannel = {0} and sServiceType={1}", ch.ID,ch.ServiceType);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						//

						// fields in tblDVBSMapping
						// idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid
						// parameter (8)(9)
						//idChannel,freq,symrate, fec,lnbkhz,diseqc,
						//prognum,servicetype,provider,channel, eitsched,
						//eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
						// teltxtpid,scrambled, pol,lnbfreq,networkid

						string provider=ch.ServiceProvider;
						string service=ch.ServiceName;
						
						if (provider==null)
							provider=String.Empty;
						if (service==null)
							service=String.Empty;
						DatabaseUtility.RemoveInvalidChars(ref provider);
						DatabaseUtility.RemoveInvalidChars(ref service);
						string al=ch.AudioLanguage;
						string al1=ch.AudioLanguage1;
						string al2=ch.AudioLanguage2;
						string al3=ch.AudioLanguage3;
						al=(al==null?"":al.Trim());
						al1=(al1==null?"":al1.Trim());
						al2=(al2==null?"":al2.Trim());
						al3=(al3==null?"":al3.Trim());
						provider=(provider==null?"":provider.Trim());
						service=(service==null?"":service.Trim());

						strSQL=String.Format("insert into tblDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})", 
							ch.ID,ch.Frequency,ch.Symbolrate,ch.FEC,ch.LNBKHz,ch.DiSEqC,
							ch.ProgramNumber,ch.ServiceType,provider,service,(ch.HasEITSchedule==true?1:0),
							(ch.HasEITPresentFollow==true?1:0), ch.AudioPid,ch.VideoPid,ch.AC3Pid,ch.Audio1, ch.Audio2, ch.Audio3,
							ch.TeletextPid,(ch.IsScrambled==true?1:0), ch.Polarity,ch.LNBFrequency,ch.NetworkID,ch.TransportStreamID,ch.PCRPid,al,al1,al2,al3,ch.ECMPid,ch.PMTPid);
					  
						m_db.Execute(strSQL);
						return 0;
					}
					else
					{
						return -1;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception {0} err:{1} stack:{2}", strSQL,ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}
		
		static public string GetSatChannelName(int program_number,int id)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				string channelName="";
				try
				{
					if (null==m_db) return "";

					SQLiteResultSet results;
					strSQL=String.Format( "select * from tblDVBSMapping where sProgramNumber={0}", program_number);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) return "";
					channelName=DatabaseUtility.Get(results,0,"sChannelName");
					return channelName;
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return "";
			}
		}
		
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
					strSQL=String.Format("select * from tblDVBSMapping where idChannel={0} and sServiceType={1}",idChannel,serviceType);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count<1) return false;
					else
					{
						//chan.ID=DatabaseUtility.GetAsInt(results,i,"idChannel");
						//chan.Number = DatabaseUtility.GetAsInt(results,i,"iChannelNr");
						// sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName
						//sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,
						//sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID
						int i=0;
						freq=DatabaseUtility.GetAsInt(results,i,"sFreq");
						symrate=DatabaseUtility.GetAsInt(results,i,"sSymbrate");
						fec=DatabaseUtility.GetAsInt(results,i,"sFEC");
						lnbkhz=DatabaseUtility.GetAsInt(results,i,"sLNBKhz");
						diseqc=DatabaseUtility.GetAsInt(results,i,"sDiseqc");
						prognum=DatabaseUtility.GetAsInt(results,i,"sProgramNumber");
						servicetype=DatabaseUtility.GetAsInt(results,i,"sServiceType");
						provider=DatabaseUtility.Get(results,i,"sProviderName");
						channel=DatabaseUtility.Get(results,i,"sChannelName");
						eitsched=DatabaseUtility.GetAsInt(results,i,"sEitSched");
						eitprefol= DatabaseUtility.GetAsInt(results,i,"sEitPreFol");
						audpid=DatabaseUtility.GetAsInt(results,i,"sAudioPid");
						vidpid=DatabaseUtility.GetAsInt(results,i,"sVideoPid");
						ac3pid=DatabaseUtility.GetAsInt(results,i,"sAC3Pid");
						apid1= DatabaseUtility.GetAsInt(results,i,"sAudio1Pid");
						apid2= DatabaseUtility.GetAsInt(results,i,"sAudio2Pid");
						apid3=DatabaseUtility.GetAsInt(results,i,"sAudio3Pid");
						teltxtpid=DatabaseUtility.GetAsInt(results,i,"sTeletextPid");
						scrambled= DatabaseUtility.GetAsInt(results,i,"sScrambled");
						pol=DatabaseUtility.GetAsInt(results,i,"sPol");
						lnbfreq=DatabaseUtility.GetAsInt(results,i,"sLNBFreq");
						networkid=DatabaseUtility.GetAsInt(results,i,"sNetworkID");
						tsid=DatabaseUtility.GetAsInt(results,i,"sTSID");
						pcrpid=DatabaseUtility.GetAsInt(results,i,"sPCRPid");
						// sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid
						audioLang=DatabaseUtility.Get(results,i,"sAudioLang");
						audioLang1=DatabaseUtility.Get(results,i,"sAudioLang1");
						audioLang2=DatabaseUtility.Get(results,i,"sAudioLang2");
						audioLang3=DatabaseUtility.Get(results,i,"sAudioLang3");
						ecm=DatabaseUtility.GetAsInt(results,i,"sECMPid");
						pmt=DatabaseUtility.GetAsInt(results,i,"sPMTPid");
						retChannel=new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
							prognum, servicetype,provider, channel, eitsched,
							eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
							teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid,audioLang,audioLang1,audioLang2,audioLang3,ecm,pmt);

					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
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
					/*				  strSQL=String.Format( "update tblDVBSMapping set (sFreq={0},sSymbrate={1},sFEC={2},sLNBKhz={3},sDiseqc={4},sProgramNumber={5},sServiceType={6},sProviderName='{7}',sChannelName='{8}',sEitSched={9},sEitPreFol={10},sAudioPid={11},sVideoPid={12},sAC3Pid={13},sAudio1Pid={14},sAudio2Pid={15},sAudio3Pid={16},sTeletextPid={17},sScrambled={18},sPol={19},sLNBFreq={20},sNetworkID={21},sTSID={22},sPCRPid={23}) where idChannel = {24}", 
											ch.Frequency,ch.Symbolrate, ch.FEC,ch.LNBKHz,ch.DiSEqC,
											ch.ProgramNumber,ch.ServiceType,strProvider,strChannel, (int)(ch.HasEITSchedule==true?1:0),
											(int)(ch.HasEITPresentFollow==true?1:0), ch.AudioPid,ch.VideoPid,ch.AC3Pid,ch.Audio1, ch.Audio2, ch.Audio3,
											ch.TeletextPid,(int)(ch.IsScrambled==true?1:0), ch.Polarity,ch.LNBFrequency,ch.NetworkID,ch.TransportStreamID,ch.PCRPid,ch.ID);
									*/
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
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
					strSQL=String.Format("select * from tblDVBSMapping order by sChannelName");
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
					  
						idChannel=DatabaseUtility.GetAsInt(results,i,"idChannel");
						freq=DatabaseUtility.GetAsInt(results,i,"sFreq");
						symrate=DatabaseUtility.GetAsInt(results,i,"sSymbrate");
						fec=DatabaseUtility.GetAsInt(results,i,"sFEC");
						lnbkhz=DatabaseUtility.GetAsInt(results,i,"sLNBKhz");
						diseqc=DatabaseUtility.GetAsInt(results,i,"sDiseqc");
						prognum=DatabaseUtility.GetAsInt(results,i,"sProgramNumber");
						servicetype=DatabaseUtility.GetAsInt(results,i,"sServiceType");
						provider=DatabaseUtility.Get(results,i,"sProviderName");
						channel=DatabaseUtility.Get(results,i,"sChannelName");
						eitsched=DatabaseUtility.GetAsInt(results,i,"sEitSched");
						eitprefol= DatabaseUtility.GetAsInt(results,i,"sEitPreFol");
						audpid=DatabaseUtility.GetAsInt(results,i,"sAudioPid");
						vidpid=DatabaseUtility.GetAsInt(results,i,"sVideoPid");
						ac3pid=DatabaseUtility.GetAsInt(results,i,"sAC3Pid");
						apid1= DatabaseUtility.GetAsInt(results,i,"sAudio1Pid");
						apid2= DatabaseUtility.GetAsInt(results,i,"sAudio2Pid");
						apid3=DatabaseUtility.GetAsInt(results,i,"sAudio3Pid");
						teltxtpid=DatabaseUtility.GetAsInt(results,i,"sTeletextPid");
						scrambled= DatabaseUtility.GetAsInt(results,i,"sScrambled");
						pol=DatabaseUtility.GetAsInt(results,i,"sPol");
						lnbfreq=DatabaseUtility.GetAsInt(results,i,"sLNBFreq");
						networkid=DatabaseUtility.GetAsInt(results,i,"sNetworkID");
						tsid=DatabaseUtility.GetAsInt(results,i,"sTSID");
						pcrpid=DatabaseUtility.GetAsInt(results,i,"sPCRPid");
						audioLang=DatabaseUtility.Get(results,i,"sAudioLang");
						audioLang1=DatabaseUtility.Get(results,i,"sAudioLang1");
						audioLang2=DatabaseUtility.Get(results,i,"sAudioLang2");
						audioLang3=DatabaseUtility.Get(results,i,"sAudioLang3");
						ecm=DatabaseUtility.GetAsInt(results,i,"sECMPid");
						pmt=DatabaseUtility.GetAsInt(results,i,"sPMTPid");
						dvbChannel=new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
							prognum, servicetype,provider, channel, eitsched,
							eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
							teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid,audioLang,audioLang1,audioLang2,audioLang3,ecm,pmt);
						channels.Add(dvbChannel);
					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
			}
		}

		static public void RemoveSatChannel(DVBChannel ch)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return ;
        
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from tblDVBSMapping where idChannel={0}",ch.ID);
					m_db.Execute(strSQL);
				  
			  
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					string strSQL=String.Format("delete from tblDVBSMapping");
					m_db.Execute(strSQL);
				  
			  
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------
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
					int iNewID=DatabaseUtility.GetAsInt(results,0,"idChannel");
					CChannelCache chan=new CChannelCache();
					chan.idChannel=iNewID;
					chan.strChannel=DatabaseUtility.Get(results,0,"strChannel");
					chan.iChannelNr=DatabaseUtility.GetAsInt(results,0,"iChannelNr");
					m_channelCache.Add(chan);
					return iNewID;
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					int iNewID=DatabaseUtility.GetAsInt(results,0,"idChannel");
					CChannelCache chan=new CChannelCache();
					chan.idChannel=iNewID;
					chan.strChannel=DatabaseUtility.Get(results,0,"strChannel");
					chan.iChannelNr=iChannelNr;
					m_channelCache.Add(chan);
					return iNewID;
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					int scrambled=0;
					if (channel.Scrambled) scrambled=1;


					strSQL=String.Format( "update channel set iChannelNr={0}, frequency={1}, iSort={2},bExternal={3}, ExternalChannel='{4}',standard={5}, Visible={6}, Country={7},strChannel='{8}', scrambled={9} where idChannel like {10}", 
						channel.Number,channel.Frequency.ToString(),
						sort,iExternal, strExternal, (int)channel.TVStandard, iVisible, channel.Country,
						strChannel, scrambled,channel.ID);
					m_db.Execute(strSQL);
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		static public int AddChannel(TVChannel channel)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL=String.Empty;
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
						int scrambled=0;
						if (channel.Scrambled) scrambled=1;

						strSQL=String.Format("insert into channel (idChannel, strChannel,iChannelNr ,frequency,iSort, bExternal, ExternalChannel,standard, Visible, Country, scrambled) values ( NULL, '{0}', {1}, {2}, {3}, {4},'{5}', {6}, {7}, {8}, {9} )", 
							strChannel,channel.Number,channel.Frequency.ToString(),
							totalchannels+1,iExternal, strExternal, (int)channel.TVStandard, iVisible,channel.Country,scrambled);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						CChannelCache chan=new CChannelCache();
						chan.idChannel=iNewID;
						chan.strChannel=strChannel;
						chan.iChannelNr=channel.Number;
						m_channelCache.Add(chan);
						channel.ID=iNewID;
						if (OnChannelsChanged!=null) OnChannelsChanged();
						return iNewID;
					}
					else
					{
						int iNewID=DatabaseUtility.GetAsInt(results,0,"idChannel");
						CChannelCache chan=new CChannelCache();
						chan.idChannel=iNewID;
						chan.strChannel=strChannel;
						chan.iChannelNr=channel.Number;
						m_channelCache.Add(chan);
						channel.ID=iNewID;
						return iNewID;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception sql:{0} err:{1} stack:{2}", strSQL,ex.Message,ex.StackTrace);
					Open();
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
						int iID=DatabaseUtility.GetAsInt(results,0,"idGenre");
						CGenreCache genre= new CGenreCache();
						genre.idGenre = iID;
						genre.strGenre = strGenre1;
						m_genreCache.Add(genre);
						return iID;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						//						Log.WriteFile(Log.LogType.Log,true,"Program Exists Ignoring:{0} {1} {2}", strTitle,prog.Start.ToString(),prog.End.ToString());
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
							//							Log.WriteFile(Log.LogType.Log,true,"Removing Program:{0} {1} {2}", DatabaseUtility.Get(results2,i,"strTitle"),iStart.ToString(),iEnd.ToString());
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

			}
			ProgramsChanged();
			return lRetId;
		}
		static public bool GetChannelsByProvider(ref ArrayList channels)
		{
			if (m_db==null) return false;
			lock (typeof(TVDatabase))
			{
				channels.Clear();
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL="select channel.idChannel,channel.iChannelNr,channel.frequency,channel.strChannel,";
					strSQL+="channel.bExternal,channel.Visible,channel.scrambled,channel.ExternalChannel,channel.standard,";
					strSQL+="channel.Country,channel.iSort,tblDVBCMapping.strProvider,tblDVBSMapping.sProviderName,tblDVBTMapping.strProvider,tblATSCMapping.strProvider ";
				  strSQL+="from channel left join tblDVBCMapping on tblDVBCMapping.iLCN=channel.idChannel ";
					strSQL+="left join tblDVBTMapping on tblDVBTMapping.iLCN=channel.idChannel ";
					strSQL+="left join tblDVBSMapping on tblDVBSMapping.idChannel=channel.idChannel ";
					strSQL+="left join tblATSCMapping on tblATSCMapping.iLCN=channel.idChannel ";
					strSQL+="order by tblDVBCMapping.strProvider, tblDVBTMapping.strProvider, tblDVBSMapping.sProviderName, tblATSCMapping.strProvider,channel.strChannel";
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						TVChannel chan=new TVChannel();
						chan.ID=DatabaseUtility.GetAsInt(results,i,0);
						chan.Number = DatabaseUtility.GetAsInt(results,i,1);
						decimal dFreq=0;
						try
						{
							dFreq = (decimal)Int64.Parse(DatabaseUtility.Get(results,i,2));

						}
						catch(Exception)
						{
							chan.Frequency =0;
						}
						dFreq /= 1000000M;
						dFreq=Math.Round(dFreq,3);
						dFreq *=1000000M;
						chan.Frequency = (long)Math.Round(dFreq,0);
						chan.Name = DatabaseUtility.Get(results,i,3);
						int iExternal=DatabaseUtility.GetAsInt(results,i,4);
						if (iExternal!=0) chan.External=true;
						else chan.External=false;

						int iVisible=DatabaseUtility.GetAsInt(results,i,5);
						if (iVisible!=0) chan.VisibleInGuide=true;
						else chan.VisibleInGuide=false;

						int scrambled=DatabaseUtility.GetAsInt(results,i,6);
						if (scrambled!=0) chan.Scrambled=true;
						else chan.Scrambled=false;

						chan.ExternalTunerChannel= DatabaseUtility.Get(results,i,7);
						chan.TVStandard = (AnalogVideoStandard)DatabaseUtility.GetAsInt(results,i,8);
						chan.Country=DatabaseUtility.GetAsInt(results,i,9);
						chan.Sort=DatabaseUtility.GetAsInt(results,i,10);
						chan.ProviderName=DatabaseUtility.Get(results,i,11);
						if (chan.ProviderName=="")
						{
							chan.ProviderName=DatabaseUtility.Get(results,i,12);
							if (chan.ProviderName=="")
							{
								chan.ProviderName=DatabaseUtility.Get(results,i,13);
								if (chan.ProviderName=="")
								{
									chan.ProviderName=DatabaseUtility.Get(results,i,14);
									if (chan.ProviderName=="")
									{
										chan.ProviderName=Strings.Unknown;
									}
								}
							}
						}
						channels.Add(chan);
					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						chan.ID=DatabaseUtility.GetAsInt(results,i,"idChannel");
						chan.Number = DatabaseUtility.GetAsInt(results,i,"iChannelNr");
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
						int iExternal=DatabaseUtility.GetAsInt(results,i,"bExternal");
						if (iExternal!=0) chan.External=true;
						else chan.External=false;

						int iVisible=DatabaseUtility.GetAsInt(results,i,"Visible");
						if (iVisible!=0) chan.VisibleInGuide=true;
						else chan.VisibleInGuide=false;


						int scrambled=DatabaseUtility.GetAsInt(results,i,"scrambled");
						if (scrambled!=0) chan.Scrambled=true;
						else chan.Scrambled=false;
						chan.Sort=DatabaseUtility.GetAsInt(results,i,"iSort");

						chan.ExternalTunerChannel= DatabaseUtility.Get(results,i,"ExternalChannel");
						chan.TVStandard = (AnalogVideoStandard)DatabaseUtility.GetAsInt(results,i,"standard");
						chan.Country=DatabaseUtility.GetAsInt(results,i,"Country");
						channels.Add(chan);
					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
          
					strSQL=String.Format("delete from canceledseries where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);

					strSQL=String.Format("delete from recording where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);
          
					strSQL=String.Format("delete from channel where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);
					
					strSQL=String.Format("delete from tblGroupMapping where idChannel={0}", iChannelId);
					m_db.Execute(strSQL);

					strSQL = String.Format("delete from tblDVBSMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping where iLCN={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping where iLCN={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblATSCMapping where iLCN={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblChannelCard where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			m_channelCache.Clear();
			ProgramsChanged();
			RecordingsChanged(RecordingChange.Deleted);
			
			if (OnChannelsChanged!=null) OnChannelsChanged();
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
			string strOrder=" order by iStartTime";
			switch(SearchKind)
			{
				case -1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' ",genre);
					break;
				case 0:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}%' ",genre,SearchCriteria);
					break;

				case 1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}%' ",genre,SearchCriteria);
					break;
        
				case 2:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}' ",genre,SearchCriteria);
					break;
        
				case 3:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}' ",genre,SearchCriteria);
					break;

			}
			return GetTVProgramsByGenre(strSQL, strOrder,progs);
		}

		static public bool GetProgramsPerGenre(string genre1, ArrayList progs)
		{
			return SearchProgramsPerGenre(genre1, progs, -1, String.Empty);
		}

		static bool GetTVProgramsByGenre(string strSQL, string strOrder, ArrayList progs)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return false;

					strSQL += strOrder;
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
							prog.ID=DatabaseUtility.GetAsInt(results,i,"tblPrograms.idProgram");
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					string strOrder=" order by iStartTime";
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and channel.strChannel like '{0}' ",
						strChannel);
					string where =String.Format(" and ( (tblPrograms.iEndTime>={0} and tblPrograms.iEndTime <={1}) ", iStartTime,iEndTime);
					where+=String.Format(" or (tblPrograms.iStartTime>={0} and tblPrograms.iStartTime <= {1} ) ", iStartTime,iEndTime);
					where+=String.Format(" or (tblPrograms.iStartTime<={0} and tblPrograms.iEndTime >= {1}) )", iStartTime,iEndTime);
					strSQL+=where;
					strSQL+=strOrder;

					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						long iStart=Int64.Parse(DatabaseUtility.Get(results,i,15));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,16));
						bool bAdd=false;
						if (iEnd >=iStartTime && iEnd <= iEndTime) bAdd=true;
						if (iStart >=iStartTime && iStart <= iEndTime) bAdd=true;
						if (iStart <=iStartTime && iEnd>=iEndTime) bAdd=true;
						if (bAdd)
						{
							TVProgram prog=new TVProgram();
							prog.Channel=DatabaseUtility.Get(results,i,1);
							prog.Start=iStart;
							prog.End=iEnd;
							prog.Genre=DatabaseUtility.Get(results,i,27);
							prog.Title=DatabaseUtility.Get(results,i,14);
							prog.Description=DatabaseUtility.Get(results,i,17);
							prog.Episode=DatabaseUtility.Get(results,i,18);
							prog.Repeat=DatabaseUtility.Get(results,i,19);
							prog.ID=DatabaseUtility.GetAsInt(results,i,11);
              prog.SeriesNum=DatabaseUtility.Get(results,i,20);
              prog.EpisodeNum=DatabaseUtility.Get(results,i,21);
              prog.EpisodePart=DatabaseUtility.Get(results,i,22);
              prog.Date=DatabaseUtility.Get(results,i,23);
              prog.StarRating=DatabaseUtility.Get(results,i,24);
              prog.Classification=DatabaseUtility.Get(results,i,25);
							progs.Add(prog);
						}
					}

					return true;

				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
			}
		}

		static bool GetMinimalPrograms(long iStartTime, long iEndTime,string strSQL, string strOrder,ref ArrayList progs)
		{
			lock (typeof(TVDatabase))
			{
				progs.Clear();
				try
				{
					if (null==m_db) return false;

					SQLiteResultSet results;
					string where =String.Format(" and ( (tblPrograms.iEndTime>={0} and tblPrograms.iEndTime <={1}) ", iStartTime,iEndTime);
					where+=String.Format(" or (tblPrograms.iStartTime>={0} and tblPrograms.iStartTime <= {1} ) ", iStartTime,iEndTime);
					where+=String.Format(" or (tblPrograms.iStartTime<={0} and tblPrograms.iEndTime >= {1}) )", iStartTime,iEndTime);
					strSQL+=where;
					strSQL+=strOrder;
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
							prog.Title=DatabaseUtility.Get(results,i,"tblPrograms.strTitle");
							progs.Add(prog);
						}
					}

					return true;

				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
			}
		}

		static bool GetTVPrograms(long iStartTime, long iEndTime,string strSQL, string strOrder,ref ArrayList progs)
		{
			lock (typeof(TVDatabase))
			{
				progs.Clear();
				try
				{
					if (null==m_db) return false;

					SQLiteResultSet results;
					string where =String.Format(" and ( (tblPrograms.iEndTime>={0} and tblPrograms.iEndTime <={1}) ", iStartTime,iEndTime);
					where+=String.Format(" or (tblPrograms.iStartTime>={0} and tblPrograms.iStartTime <= {1} ) ", iStartTime,iEndTime);
					where+=String.Format(" or (tblPrograms.iStartTime<={0} and tblPrograms.iEndTime >= {1}) )", iStartTime,iEndTime);
					strSQL+=where;
					strSQL+=strOrder;
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
							prog.ID=DatabaseUtility.GetAsInt(results,i,"tblPrograms.idProgram");
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
			}
		}

		static public bool SearchPrograms(long iStartTime, long iEndTime,ref ArrayList progs, int SearchKind, string SearchCriteria, string channel)
		{
			DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
			string strSQL=String.Empty;
			string strOrder=" order by iStartTime";
			switch (SearchKind)
			{
				case -1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel ");
					break;
				case 0:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}%' ", SearchCriteria);
					break;
				case 1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}%' ", SearchCriteria);
					break;
				case 2:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}' ", SearchCriteria);
					break;
				case 3:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}' ", SearchCriteria);
					break;
			}
			if (channel!=String.Empty)
			{
				DatabaseUtility.RemoveInvalidChars(ref channel);
				strSQL += String.Format(" and channel.strChannel like '{0}' ",channel);
			}
			return GetTVPrograms(iStartTime, iEndTime, strSQL, strOrder,ref progs);
		}

		static public bool SearchMinimalPrograms(long iStartTime, long iEndTime,ref ArrayList progs, int SearchKind, string SearchCriteria, string channel)
		{
			DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
			string strSQL=String.Empty;
			string strOrder=" order by iStartTime";
			switch (SearchKind)
			{
				case -1:
					strSQL=String.Format("select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms  where tblPrograms.idChannel=channel.idChannel ");
					break;
				case 0:
					strSQL=String.Format("select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms  where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}%' ", SearchCriteria);
					break;
				case 1:
					strSQL=String.Format("select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}%' ", SearchCriteria);
					break;
				case 2:
					strSQL=String.Format("select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}' ", SearchCriteria);
					break;
				case 3:
					strSQL=String.Format("select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}' ", SearchCriteria);
					break;
			}
			if (channel!=String.Empty)
			{
				DatabaseUtility.RemoveInvalidChars(ref channel);
				strSQL += String.Format(" and channel.strChannel like '{0}' ",channel);
			}
			return GetMinimalPrograms(iStartTime, iEndTime, strSQL, strOrder,ref progs);
		}

		static public bool SearchProgramsByDescription(long iStartTime, long iEndTime,ref ArrayList progs, int SearchKind, string SearchCriteria)
		{
			DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
			string strSQL=String.Empty;
			string strOrder=" order by iStartTime";
			switch (SearchKind)
			{
				case -1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel  ");
					break;
				case 0:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}%' ", SearchCriteria);
					break;
				case 1:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}%' ", SearchCriteria);
					break;
				case 2:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}' ", SearchCriteria);
					break;
				case 3:
					strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}' ", SearchCriteria);
					break;
			}
			return GetTVPrograms(iStartTime, iEndTime, strSQL,strOrder, ref progs);
		}
		static public int GetProgramByDescriptionID(string summaryID,ref TVProgram prog)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				if (null==m_db) return -1;
				strSQL=String.Format("select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription ='{0}'", summaryID);
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count==1)
				{
					prog=new TVProgram();
			
					
					long iStart=Int64.Parse(DatabaseUtility.Get(results,0,"tblPrograms.iStartTime"));
					long iEnd=Int64.Parse(DatabaseUtility.Get(results,0,"tblPrograms.iEndTime"));
					prog.Channel=DatabaseUtility.Get(results,0,"channel.strChannel");
					prog.Start=iStart;
					prog.End=iEnd;
					prog.Genre=DatabaseUtility.Get(results,0,"genre.strGenre");
					prog.Title=DatabaseUtility.Get(results,0,"tblPrograms.strTitle");
					prog.Description=DatabaseUtility.Get(results,0,"tblPrograms.strDescription");
					prog.Episode=DatabaseUtility.Get(results,0,"tblPrograms.strEpisodeName");
					prog.Repeat=DatabaseUtility.Get(results,0,"tblPrograms.strRepeat");
					prog.ID=DatabaseUtility.GetAsInt(results,0,"tblPrograms.idProgram");
					prog.SeriesNum=DatabaseUtility.Get(results,0,"tblPrograms.strSeriesNum");
					prog.EpisodeNum=DatabaseUtility.Get(results,0,"tblPrograms.strEpisodeNum");
					prog.EpisodePart=DatabaseUtility.Get(results,0,"tblPrograms.strEpisodePart");
					prog.Date=DatabaseUtility.Get(results,0,"tblPrograms.strDate");
					prog.StarRating=DatabaseUtility.Get(results,0,"tblPrograms.strStarRating");
					prog.Classification=DatabaseUtility.Get(results,0,"tblPrograms.strClassification");
					return 0;
				}
				else
					return -1;

			}

		}
		static public bool GetPrograms(long iStartTime, long iEndTime,ref ArrayList progs)
		{
			return SearchPrograms(iStartTime, iEndTime,ref progs,-1,String.Empty,String.Empty);
		}

		static public void SetRecordedFileName(TVRecorded rec)
		{

			string strFileName=rec.FileName;
			DatabaseUtility.RemoveInvalidChars(ref strFileName);
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("update recorded set strFileName='{0}' where idRecorded={1}",strFileName, rec.ID);
					m_db.Execute(strSQL);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public void UpdateRecording(TVRecording recording, RecordingChange change)
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

					strSQL=String.Format("update recording set idChannel={0},iRecordingType={1},strProgram='{2}',iStartTime='{3}',iEndTime='{4}', iCancelTime='{5}', bContentRecording={6}, quality={7}, priority={8},episodesToKeep={9} where idRecording={10}", 
						iChannelId,
						(int)recording.RecType,
						strTitle,
						recording.Start.ToString(),
						recording.End.ToString(),
						recording.Canceled.ToString(),
						iContentRec,
						(int)recording.Quality,
						recording.Priority,
						recording.EpisodesToKeep,
						recording.ID);
					m_db.Execute(strSQL);
	
					DeleteCanceledSeries(recording);
					foreach (long datetime in recording.CanceledSeries)
					{
						AddCanceledSerie(recording, datetime);
					}

				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			RecordingsChanged(change);
		}
		static public void SetRecordingQuality(TVRecording recording)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strTitle=recording.Title;
					DatabaseUtility.RemoveInvalidChars(ref strTitle);

					if (null==m_db) return ;
					if (recording.ID<0) return ;
          
					strSQL=String.Format("update recording set quality={0} where idRecording={1}", 
						(int)recording.Quality,
						recording.ID);
					m_db.Execute(strSQL);

				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			RecordingsChanged(RecordingChange.QualityChange);
		}
		static public void SetRecordingPriority(TVRecording recording)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strTitle=recording.Title;
					DatabaseUtility.RemoveInvalidChars(ref strTitle);

					if (null==m_db) return ;
					if (recording.ID<0) return ;
          
					strSQL=String.Format("update recording set priority={0} where idRecording={1}", 
						(int)recording.Priority,
						recording.ID);
					m_db.Execute(strSQL);

				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			RecordingsChanged(RecordingChange.PriorityChange);
		}
		static public void SetRecordingEpisodesToKeep(TVRecording recording)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					string strTitle=recording.Title;
					DatabaseUtility.RemoveInvalidChars(ref strTitle);

					if (null==m_db) return ;
					if (recording.ID<0) return ;
          
					strSQL=String.Format("update recording set episodesToKeep={0} where idRecording={1}", 
						(int)recording.EpisodesToKeep,
						recording.ID);
					m_db.Execute(strSQL);

				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			RecordingsChanged(RecordingChange.EpisodesToKeepChange);
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

					if (null==m_db) 
					{
						Log.WriteFile(Log.LogType.Log,true,"TVDatabase.AddRecording:tvdatabase not opened");
						return -1;
					}
					int iChannelId=GetChannelId(recording.Channel);
					if (iChannelId<0) 
					{
						Log.WriteFile(Log.LogType.Log,true,"TVDatabase.AddRecording:invalid channel:{0}",recording.Channel);
						return -1;
					}
					int iContentRec=1;
					if (!recording.IsContentRecording) iContentRec=0;
          
					strSQL=String.Format("insert into recording (idRecording,idChannel,iRecordingType,strProgram,iStartTime,iEndTime,iCancelTime,bContentRecording,quality,priority,episodesToKeep ) values ( NULL, {0}, {1}, '{2}','{3}', '{4}', '{5}', {6}, {7}, {8},{9})", 
						iChannelId,
						(int)recording.RecType,
						strTitle,
						recording.Start.ToString(),
						recording.End.ToString(),
						recording.Canceled.ToString(),
						iContentRec,
						(int)recording.Quality,
						recording.Priority,
						recording.EpisodesToKeep);
					m_db.Execute(strSQL);
					lNewId=m_db.LastInsertID();
					recording.ID=lNewId;
					
					foreach (long datetime in recording.CanceledSeries)
					{
						AddCanceledSerie(recording, datetime);
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			RecordingsChanged(RecordingChange.Added);
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
					strSQL=String.Format("delete from canceledseries where idRecording={0}",record.ID);
					m_db.Execute(strSQL);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			RecordingsChanged(RecordingChange.Deleted);
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
							rec.ID=DatabaseUtility.GetAsInt(results,i,"recording.idRecording");
							rec.Title=DatabaseUtility.Get(results,i,"recording.strProgram");
							rec.RecType=(TVRecording.RecordingType)DatabaseUtility.GetAsInt(results,i,"recording.iRecordingType");
							rec.Quality=(TVRecording.QualityType)DatabaseUtility.GetAsInt(results,i,"recording.quality");
							rec.Priority=DatabaseUtility.GetAsInt(results,i,"recording.priority");
							rec.EpisodesToKeep=DatabaseUtility.GetAsInt(results,i,"recording.episodesToKeep");
							int iContectRec=DatabaseUtility.GetAsInt(results,i,"recording.bContentRecording");
							if (iContectRec==1) rec.IsContentRecording=true;
							else rec.IsContentRecording=false;
							GetCanceledRecordings(ref rec);
							recordings.Add(rec);
						}
					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						rec.ID=DatabaseUtility.GetAsInt(results,i,"recording.idRecording");
						rec.Title=DatabaseUtility.Get(results,i,"recording.strProgram");
						rec.RecType=(TVRecording.RecordingType)DatabaseUtility.GetAsInt(results,i,"recording.iRecordingType");
						rec.Quality=(TVRecording.QualityType)DatabaseUtility.GetAsInt(results,i,"recording.quality");
						rec.Priority=DatabaseUtility.GetAsInt(results,i,"recording.priority");
						rec.EpisodesToKeep=DatabaseUtility.GetAsInt(results,i,"recording.episodesToKeep");

						int iContectRec=DatabaseUtility.GetAsInt(results,i,"recording.bContentRecording");
						if (iContectRec==1) rec.IsContentRecording=true;
						else rec.IsContentRecording=false;
						GetCanceledRecordings(ref rec);
						recordings.Add(rec);
					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					Log.WriteFile(Log.LogType.Log,true,"tvdatabase begin transaction failed exception err:{0} ", ex.Message);		
					Open();
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
					Log.WriteFile(Log.LogType.Log,true,"tvdatabase commit failed exception err:{0} ", ex.Message);		
					Open();
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
				Log.WriteFile(Log.LogType.Log,true,"tvdatabase rollback failed exception err:{0} ", ex.Message);		
				Open();
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		} 

		static public void RemoveRecordedTVByFileName(string fileName)
		{
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null==m_db) return ;
					string filteredName=fileName;
					DatabaseUtility.RemoveInvalidChars(ref filteredName);
					string strSQL=String.Format("delete from recorded where strFileName like '{0}'",filteredName);
					m_db.Execute(strSQL);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						rec.ID=DatabaseUtility.GetAsInt(results,i,"recorded.idRecorded");
						rec.Title=DatabaseUtility.Get(results,i,"recorded.strProgram");
						rec.Description=DatabaseUtility.Get(results,i,"recorded.strDescription");
						rec.FileName=DatabaseUtility.Get(results,i,"recorded.strFileName");
						rec.Genre=DatabaseUtility.Get(results,i,"genre.strGenre");
						rec.Played=DatabaseUtility.GetAsInt(results,i,"recorded.iPlayed");
						recordings.Add(rec);
					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					recording.ID=DatabaseUtility.GetAsInt(results,0,"recorded.idRecorded");
					recording.Title=DatabaseUtility.Get(results,0,"recorded.strProgram");
					recording.Description=DatabaseUtility.Get(results,0,"recorded.strDescription");
					recording.FileName=DatabaseUtility.Get(results,0,"recorded.strFileName");
					recording.Genre=DatabaseUtility.Get(results,0,"genre.strGenre");
					recording.Played=DatabaseUtility.GetAsInt(results,0,"recorded.iPlayed");

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					if (m_bRecordingsChanged)  RecordingsChanged(RecordingChange.Added);
				}
			}
		}
		static void RecordingsChanged(RecordingChange changeType)
		{
			m_bRecordingsChanged=true;
			if (!m_bSupressEvents)
			{
				if (OnRecordingsChanged!=null) OnRecordingsChanged(changeType);
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
					strSQL=String.Format("DELETE FROM canceledseries where iCancelTime < {0}",longYesterday);
					m_db.Execute(strSQL);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						prog.ID=DatabaseUtility.GetAsInt(results,i,"tblPrograms.idProgram");

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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		static public int MapDVBTChannel(string channelName, string providerName,int idChannel, int frequency, int ONID, int TSID, int SID, int audioPid, int videoPid, int teletextPid, int pmtPid, int bandWidth, int audio1,int audio2, int audio3,int ac3Pid, string audioLanguage, string audioLanguage1, string audioLanguage2, string audioLanguage3, bool HasEITPresentFollow , bool HasEITSchedule )
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return -1;
				string strSQL;
				try
				{
					string strChannel=channelName;
					string strProvider=providerName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					DatabaseUtility.RemoveInvalidChars(ref strProvider);

					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBTMapping where iLCN like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						//ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3
						strSQL=String.Format("insert into tblDVBTMapping (idChannel, strChannel ,strProvider, iLCN , frequency , bandwidth , ONID , TSID , SID , audioPid,videoPid,teletextPid,pmtPid,ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,HasEITPresentFollow , HasEITSchedule ,Visible) Values( NULL, '{0}', '{1}', {2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},'{16}','{17}','{18}','{19}',{20},{21},1)",
									strChannel.Trim(),strProvider.Trim(),idChannel,frequency,bandWidth,ONID,TSID,SID,audioPid,videoPid,teletextPid,pmtPid,
									ac3Pid,audio1,audio2,audio3,audioLanguage.Trim(),audioLanguage1.Trim(),audioLanguage2.Trim(),audioLanguage3.Trim(),
								(HasEITPresentFollow==true?1:0) , (HasEITSchedule==true?1:0) 
						);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBTMapping set frequency='{0}', ONID={1}, TSID={2}, SID={3}, strChannel='{4}',strProvider='{5}',audioPid={6},videoPid={7},teletextPid={8},pmtPid={9}, bandwidth={10},ac3Pid={11},audio1Pid={12},audio2Pid={13},audio3Pid={14},sAudioLang='{15}',sAudioLang1='{16}',sAudioLang2='{17}',sAudioLang3='{18}',HasEITPresentFollow={19} , HasEITSchedule={20}  where iLCN like '{21}'", 
							frequency,ONID,TSID,SID,strChannel.Trim(), strProvider.Trim(),audioPid,videoPid,teletextPid, pmtPid,bandWidth,
							ac3Pid,audio1,audio2,audio3,audioLanguage.Trim(),audioLanguage1.Trim(),audioLanguage2.Trim(),audioLanguage3.Trim(),
							(HasEITPresentFollow==true?1:0) , (HasEITSchedule==true?1:0) 
							,idChannel);
						//	Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}
		
		static public int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate,int innerFec, int modulation,int ONID, int TSID, int SID, int audioPid, int videoPid, int teletextPid, int pmtPid, int audio1,int audio2, int audio3, int ac3Pid, string audioLanguage,string audioLanguage1, string audioLanguage2, string audioLanguage3, bool HasEITPresentFollow , bool HasEITSchedule )
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return -1;
				string strSQL=String.Empty;
				try
				{
					string strChannel=channelName;
					string strProvider=providerName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					DatabaseUtility.RemoveInvalidChars(ref strProvider);

					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBCMapping where iLCN like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBCMapping (idChannel, strChannel,strProvider,iLCN,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,videoPid,teletextPid,pmtPid,ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3, HasEITPresentFollow , HasEITSchedule ,Visible) Values( NULL, '{0}', '{1}', {2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},'{18}','{19}','{20}','{21}',{22},{23},1)"
																	,strChannel.Trim(),strProvider.Trim(),idChannel,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,videoPid,teletextPid, pmtPid,
																	ac3Pid,audio1,audio2,audio3,audioLanguage.Trim(),audioLanguage1.Trim(),audioLanguage2.Trim(),audioLanguage3.Trim(),
																	(HasEITPresentFollow==true?1:0) , (HasEITSchedule==true?1:0) );
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, videoPid={10}, teletextPid={11}, pmtPid={12},ac3Pid={13},audio1Pid={14},audio2Pid={15},audio3Pid={16},sAudioLang='{17}',sAudioLang1='{18}',sAudioLang2='{19}',sAudioLang3='{20}',HasEITPresentFollow={21} , HasEITSchedule={22}  where iLCN like '{23}'", 
																	frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,strChannel.Trim(), strProvider.Trim(),audioPid,videoPid,teletextPid,pmtPid,
																	ac3Pid,audio1,audio2,audio3,audioLanguage,audioLanguage1.Trim(),audioLanguage2.Trim(),audioLanguage3.Trim(),
																	(HasEITPresentFollow==true?1:0) , (HasEITSchedule==true?1:0),
																	idChannel);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"sql:{0}",strSQL);
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

		static public int MapATSCChannel(string channelName, int physicalChannel,int minorChannel,int majorChannel,string providerName, int idChannel, int frequency, int symbolrate,int innerFec, int modulation,int ONID, int TSID, int SID, int audioPid, int videoPid, int teletextPid, int pmtPid, int audio1,int audio2, int audio3, int ac3Pid, string audioLanguage,string audioLanguage1, string audioLanguage2, string audioLanguage3, bool HasEITPresentFollow , bool HasEITSchedule )
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return -1;
				string strSQL=String.Empty;
				try
				{
					string strChannel=channelName;
					string strProvider=providerName;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					DatabaseUtility.RemoveInvalidChars(ref strProvider);

					SQLiteResultSet results;
					strSQL=String.Format( "select * from channel ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblATSCMapping where iLCN like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblATSCMapping (idChannel, strChannel,strProvider,iLCN,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,videoPid,teletextPid,pmtPid,ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,channelNumber,minorChannel,majorChannel, HasEITPresentFollow , HasEITSchedule ,Visible) Values( NULL, '{0}', '{1}', {2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},'{18}','{19}','{20}','{21}',{22},{23},{24},{25},{26},1)"
							,strChannel.Trim(),strProvider.Trim(),idChannel,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,videoPid,teletextPid, pmtPid,
							ac3Pid,audio1,audio2,audio3,audioLanguage.Trim(),audioLanguage1.Trim(),audioLanguage2.Trim(),audioLanguage3.Trim(), physicalChannel,minorChannel,majorChannel,
							(HasEITPresentFollow==true?1:0) , (HasEITSchedule==true?1:0));
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblATSCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, videoPid={10}, teletextPid={11}, pmtPid={12},ac3Pid={13},audio1Pid={14},audio2Pid={15},audio3Pid={16},sAudioLang='{17}',sAudioLang1='{18}',sAudioLang2='{19}',sAudioLang3='{20}', channelNumber={21}, majorChannel={22}, minorChannel={23},HasEITPresentFollow={24} , HasEITSchedule={25}  where iLCN like '{26}'", 
							frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,strChannel.Trim(), strProvider.Trim(),audioPid,videoPid,teletextPid,pmtPid,
							ac3Pid,audio1,audio2,audio3,audioLanguage,audioLanguage1.Trim(),audioLanguage2.Trim(),audioLanguage3.Trim(), physicalChannel,minorChannel,majorChannel,
							(HasEITPresentFollow==true?1:0) , (HasEITSchedule==true?1:0),
							idChannel);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"sql:{0}",strSQL);
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

		
		static public void GetDVBTTuneRequest(int idChannel, out string strProvider,out int frequency, out int ONID, out int TSID, out int SID, out int audioPid, out int videoPid, out int teletextPid, out int pmtPid, out int bandwidth, out int audio1,out int audio2,out int audio3,out int ac3Pid, out string audioLanguage, out string audioLanguage1,out string audioLanguage2,out string audioLanguage3, out bool HasEITPresentFollow, out bool HasEITSchedule) 
		{
			HasEITPresentFollow=HasEITSchedule=false;
			audio1=audio2=audio3=ac3Pid=-1;
			audioLanguage=audioLanguage1=audioLanguage2=audioLanguage3="";
			bandwidth=-1;
			audioPid=videoPid=teletextPid=0;
			strProvider="";
			frequency=-1;
			pmtPid=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			//Log.WriteFile(Log.LogType.Log,true,"GetTuneRequest for iLCN:{0}", iLCN);
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBTMapping where iLCN={0}",idChannel);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=DatabaseUtility.GetAsInt(results,0,"frequency");
					ONID=DatabaseUtility.GetAsInt(results,0,"ONID");
					TSID=DatabaseUtility.GetAsInt(results,0,"TSID");
					SID=DatabaseUtility.GetAsInt(results,0,"SID");
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=DatabaseUtility.GetAsInt(results,0,"audioPid");
					videoPid=DatabaseUtility.GetAsInt(results,0,"videoPid");
					teletextPid=DatabaseUtility.GetAsInt(results,0,"teletextPid");
					pmtPid=DatabaseUtility.GetAsInt(results,0,"pmtPid");
					bandwidth=DatabaseUtility.GetAsInt(results,0,"bandwidth");
					audio1=DatabaseUtility.GetAsInt(results,0,"audio1Pid");
					audio2=DatabaseUtility.GetAsInt(results,0,"audio2Pid");
					audio3=DatabaseUtility.GetAsInt(results,0,"audio3Pid");
					ac3Pid=DatabaseUtility.GetAsInt(results,0,"ac3Pid");
					audioLanguage=DatabaseUtility.Get(results,0,"sAudioLang");
					audioLanguage1=DatabaseUtility.Get(results,0,"sAudioLang1");
					audioLanguage2=DatabaseUtility.Get(results,0,"sAudioLang2");
					audioLanguage3=DatabaseUtility.Get(results,0,"sAudioLang3");
					HasEITPresentFollow=DatabaseUtility.GetAsInt(results,0,"HasEITPresentFollow")!=0;
					HasEITSchedule=DatabaseUtility.GetAsInt(results,0,"HasEITSchedule")!=0;
					return ;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public void GetDVBCTuneRequest(int idChannel, out string strProvider,out int frequency,out int symbolrate,out int innerFec,out int modulation, out int ONID, out int TSID, out int SID, out int audioPid,out int videoPid, out int teletextPid, out int pmtPid, out int audio1,out int audio2,out int audio3,out int ac3Pid, out string audioLanguage, out string audioLanguage1,out string audioLanguage2,out string audioLanguage3, out bool HasEITPresentFollow, out bool HasEITSchedule) 
		{
			HasEITPresentFollow=HasEITSchedule=false;
			audio1=audio2=audio3=ac3Pid=-1;
			audioLanguage=audioLanguage1=audioLanguage2=audioLanguage3="";
			pmtPid=-1;
			audioPid=videoPid=teletextPid=0;
			strProvider="";
			frequency=-1;
			symbolrate=-1;
			innerFec=-1;
			modulation=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			//Log.WriteFile(Log.LogType.Log,true,"GetTuneRequest for iLCN:{0}", iLCN);
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBCMapping where iLCN={0}",idChannel);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=DatabaseUtility.GetAsInt(results,0,"frequency");
					symbolrate=DatabaseUtility.GetAsInt(results,0,"symbolrate");
					innerFec=DatabaseUtility.GetAsInt(results,0,"innerFec");
					modulation=DatabaseUtility.GetAsInt(results,0,"modulation");
					ONID=DatabaseUtility.GetAsInt(results,0,"ONID");
					TSID=DatabaseUtility.GetAsInt(results,0,"TSID");
					SID=DatabaseUtility.GetAsInt(results,0,"SID");
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=DatabaseUtility.GetAsInt(results,0,"audioPid");
					videoPid=DatabaseUtility.GetAsInt(results,0,"videoPid");
					teletextPid=DatabaseUtility.GetAsInt(results,0,"teletextPid");
					pmtPid=DatabaseUtility.GetAsInt(results,0,"pmtPid");
					audio1=DatabaseUtility.GetAsInt(results,0,"audio1Pid");
					audio2=DatabaseUtility.GetAsInt(results,0,"audio2Pid");
					audio3=DatabaseUtility.GetAsInt(results,0,"audio3Pid");
					ac3Pid=DatabaseUtility.GetAsInt(results,0,"ac3Pid");
					audioLanguage=DatabaseUtility.Get(results,0,"sAudioLang");
					audioLanguage1=DatabaseUtility.Get(results,0,"sAudioLang1");
					audioLanguage2=DatabaseUtility.Get(results,0,"sAudioLang2");
					audioLanguage3=DatabaseUtility.Get(results,0,"sAudioLang3");
					
					HasEITPresentFollow=DatabaseUtility.GetAsInt(results,0,"HasEITPresentFollow")!=0;
					HasEITSchedule=DatabaseUtility.GetAsInt(results,0,"HasEITSchedule")!=0;
					return ;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		static public void GetATSCTuneRequest(int idChannel, out int physicalChannel,out string strProvider,out int frequency,out int symbolrate,out int innerFec,out int modulation, out int ONID, out int TSID, out int SID, out int audioPid,out int videoPid, out int teletextPid, out int pmtPid, out int audio1,out int audio2,out int audio3,out int ac3Pid, out string audioLanguage, out string audioLanguage1,out string audioLanguage2,out string audioLanguage3, out int minorChannel, out int majorChannel, out bool HasEITPresentFollow, out bool HasEITSchedule) 
		{
			HasEITPresentFollow=HasEITSchedule=false;
			minorChannel=-1; majorChannel=-1;
			audio1=audio2=audio3=ac3Pid=-1;
			physicalChannel=-1;
			audioLanguage=audioLanguage1=audioLanguage2=audioLanguage3="";
			pmtPid=-1;
			audioPid=videoPid=teletextPid=0;
			strProvider="";
			frequency=-1;
			symbolrate=-1;
			innerFec=-1;
			modulation=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			//Log.WriteFile(Log.LogType.Log,true,"GetTuneRequest for iLCN:{0}", iLCN);
			lock (typeof(TVDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblATSCMapping where iLCN={0}",idChannel);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=DatabaseUtility.GetAsInt(results,0,"frequency");
					symbolrate=DatabaseUtility.GetAsInt(results,0,"symbolrate");
					innerFec=DatabaseUtility.GetAsInt(results,0,"innerFec");
					modulation=DatabaseUtility.GetAsInt(results,0,"modulation");
					ONID=DatabaseUtility.GetAsInt(results,0,"ONID");
					TSID=DatabaseUtility.GetAsInt(results,0,"TSID");
					SID=DatabaseUtility.GetAsInt(results,0,"SID");
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=DatabaseUtility.GetAsInt(results,0,"audioPid");
					videoPid=DatabaseUtility.GetAsInt(results,0,"videoPid");
					teletextPid=DatabaseUtility.GetAsInt(results,0,"teletextPid");
					pmtPid=DatabaseUtility.GetAsInt(results,0,"pmtPid");
					audio1=DatabaseUtility.GetAsInt(results,0,"audio1Pid");
					audio2=DatabaseUtility.GetAsInt(results,0,"audio2Pid");
					audio3=DatabaseUtility.GetAsInt(results,0,"audio3Pid");
					ac3Pid=DatabaseUtility.GetAsInt(results,0,"ac3Pid");
					audioLanguage=DatabaseUtility.Get(results,0,"sAudioLang");
					audioLanguage1=DatabaseUtility.Get(results,0,"sAudioLang1");
					audioLanguage2=DatabaseUtility.Get(results,0,"sAudioLang2");
					audioLanguage3=DatabaseUtility.Get(results,0,"sAudioLang3");
					physicalChannel=DatabaseUtility.GetAsInt(results,0,"channelNumber");
					minorChannel=DatabaseUtility.GetAsInt(results,0,"minorChannel");
					majorChannel=DatabaseUtility.GetAsInt(results,0,"majorChannel");
					HasEITPresentFollow=DatabaseUtility.GetAsInt(results,0,"HasEITPresentFollow")!=0;
					HasEITSchedule=DatabaseUtility.GetAsInt(results,0,"HasEITSchedule")!=0;
					return ;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						group.ID      =DatabaseUtility.GetAsInt(results,i,"idGroup");
						group.Sort     =DatabaseUtility.GetAsInt(results,i,"iSort");
						group.Pincode  =DatabaseUtility.GetAsInt(results,i,"Pincode");
						group.GroupName=DatabaseUtility.Get(results,i,"strName");
						groups.Add(group);
					}
					foreach (TVGroup group in groups)
					{
						GetTVChannelsForGroup(group);
					}
				}
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						int channelid=DatabaseUtility.GetAsInt(results,i,"tblGroupMapping.idChannel");
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		
		static public void GetCards(ref ArrayList cards)
		{
			cards.Clear();
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return ;
					SQLiteResultSet results;

					strSQL=String.Format( "select distinct card from tblChannelCard ");

					results=m_db.Execute(strSQL);
					for (int i=0; i < results.Rows.Count;++i)
					{
						cards.Add( DatabaseUtility.GetAsInt(results,i,"card"));
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						int id    =DatabaseUtility.GetAsInt(results,i,"idChannelCard") ;
						int cardnr=DatabaseUtility.GetAsInt(results,i,"card") ;	
						cardnr--;
						strSQL=String.Format( "update tblChannelCard set card={0} where idChannelCard={1}", 
																cardnr, id);
						m_db.Execute(strSQL);
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		/// <summary>
		/// This method returns true if the specified card can receive the specified channel
		/// else it returns false. Since mediaportal can have multiple cards (analog,digital,cable,antenna)
		/// its possible that not all channels can be received by each card
		/// </summary>
		/// <param name="channelName">channelName name of the tv channel</param>
		/// <param name="card">card Id</param>
		/// <returns>
		/// true: card can receive the channel
		/// false: card cannot receive the channel
		/// </returns>
		static public bool CanCardViewTVChannel(string channelName, int card)
		{
			string tvChannelName=channelName;
			DatabaseUtility.RemoveInvalidChars(ref tvChannelName);

			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return false;
					SQLiteResultSet results;

					strSQL=String.Format( "select * from tblChannelCard,channel where channel.idChannel=tblChannelCard.idChannel and channel.strChannel like '{0}' and tblChannelCard.card={1}", tvChannelName,card);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count!=0) 
					{
						return true;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			return false;
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		static public void DeleteChannelsFromGroup(TVGroup group)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{

					if (null==m_db) return ;

					strSQL=String.Format( "delete from tblGroupMapping where idGroup={0} ", group.ID);

					m_db.Execute(strSQL);
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
						int iNewID=DatabaseUtility.GetAsInt(results,0,"idGroup");
						strSQL=String.Format( "update tblGroups set strName='{0}', iSort={1}, Pincode={2} where idGroup={3}", 
																   strgroupName, group.Sort,group.Pincode, iNewID);
						results=m_db.Execute(strSQL);
						return iNewID;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return -1;
			}
		}

		static public bool GetChannelsForCard(ref ArrayList channels, int card)
		{
			if (m_db==null) return false;
			lock (typeof(TVDatabase))
			{
				channels.Clear();
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from channel,tblChannelCard where channel.idChannel=tblChannelCard.idChannel and tblChannelCard.card={0} order by channel.iSort",card);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					for (int i=0; i < results.Rows.Count;++i)
					{
						TVChannel chan=new TVChannel();
						chan.ID=DatabaseUtility.GetAsInt(results,i,"channel.idChannel");
						chan.Number = DatabaseUtility.GetAsInt(results,i,"channel.iChannelNr");
						decimal dFreq=0;
						try
						{
							dFreq = (decimal)Int64.Parse(DatabaseUtility.Get(results,i,"channel.frequency"));

						}
						catch(Exception)
						{
							chan.Frequency =0;
						}
						dFreq /= 1000000M;
						dFreq=Math.Round(dFreq,3);
						dFreq *=1000000M;
						chan.Frequency = (long)Math.Round(dFreq,0);
						chan.Name = DatabaseUtility.Get(results,i,"channel.strChannel");
						int iExternal=DatabaseUtility.GetAsInt(results,i,"channel.bExternal");
						if (iExternal!=0) chan.External=true;
						else chan.External=false;

						int iVisible=DatabaseUtility.GetAsInt(results,i,"channel.Visible");
						if (iVisible!=0) chan.VisibleInGuide=true;
						else chan.VisibleInGuide=false;

						chan.ExternalTunerChannel= DatabaseUtility.Get(results,i,"channel.ExternalChannel");
						chan.TVStandard = (AnalogVideoStandard)DatabaseUtility.GetAsInt(results,i,"channel.standard");
						chan.Country=DatabaseUtility.GetAsInt(results,i,"channel.Country");
						chan.Sort=DatabaseUtility.GetAsInt(results,i,"channel.iSort");
						channels.Add(chan);
					}

					return true;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
			}
		}
		static public void DeleteCanceledSeries(TVRecording rec)
		{
			try
			{
				string strSQL=String.Format("delete from canceledseries where idRecording={0}", rec.ID);
				m_db.Execute(strSQL);
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				Open();
			}
		}

		static public void AddCanceledSerie(TVRecording rec, long datetime)
		{	
			try
			{
				long idChannel=GetChannelId(rec.Channel);
				string strSQL=String.Format("insert into canceledseries (idRecording , idChannel,iCancelTime ) values ( {0}, {1},'{2}' )", 
															rec.ID,idChannel,datetime);
				m_db.Execute(strSQL);
				 RecordingsChanged(RecordingChange.CanceledSerie);
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
				Open();
			}
		}

		static public void GetCanceledRecordings(ref TVRecording rec)
		{
			rec.CanceledSeries.Clear();
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return ;
					SQLiteResultSet results;
					strSQL=String.Format( "select * from canceledseries where idRecording={0}", rec.ID);
					results=m_db.Execute(strSQL);
					for (int x=0; x < results.Rows.Count;++x)
					{
						long datetime=Int64.Parse(DatabaseUtility.Get(results,x,"iCancelTime"));
						rec.CanceledSeries.Add(datetime);
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}		
		}//static public void GetCanceledRecordings(ref TVRecording rec)

		static public int FindFreeTvChannelNumber(int preferenceNumber)
		{
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			bool found=false;
			do
			{
				found=false;
				foreach (TVChannel chan in channels)
				{
					if (chan.Number==preferenceNumber)
					{
						found=true;
						preferenceNumber++;
						break;
					}
				}
			} while (found==true);
			return preferenceNumber;
		}
		static public bool DoesChannelExist(int channelId, int TSID, int ONID)
		{
			//check dvbs
			string strSQL=String.Format( "select * from channel,tblDVBSMapping where tblDVBSMapping.idChannel = channel.idChannel and sTSID={0} and sNetworkID={1} and channel.idChannel={2}", TSID,ONID, channelId);
			SQLiteResultSet results;
			results=m_db.Execute(strSQL);
			if (results.Rows.Count>0) return true;

			//check dvbc
			strSQL=String.Format( "select * from channel,tblDVBCMapping where tblDVBCMapping.iLCN = channel.idChannel  and TSID={0} and ONID={1} and channel.idChannel={2}", TSID,ONID, channelId);
			results=m_db.Execute(strSQL);
			if (results.Rows.Count>0) return true;

			//check DVBT
			strSQL=String.Format( "select * from channel,tblDVBTMapping where tblDVBTMapping.iLCN = channel.idChannel  and TSID={0} and ONID={1} and channel.idChannel={2}", TSID,ONID, channelId);
			results=m_db.Execute(strSQL);
			if (results.Rows.Count>0) return true;

			//check ATSC
			strSQL=String.Format( "select * from channel,tblATSCMapping where tblATSCMapping.iLCN = channel.idChannel  and TSID={0} and ONID={1} and channel.idChannel={2}", TSID,ONID, channelId);
			results=m_db.Execute(strSQL);
			if (results.Rows.Count>0) return true;

			return false;
		}
		static public bool DoesChannelHaveAC3(TVChannel chan, bool checkDVBC, bool checkDVBT, bool checkDVBS, bool checkATSC)
		{
			int audio1, audio2, audio3, ac3Pid;
			string audioLanguage,  audioLanguage1, audioLanguage2, audioLanguage3;
			int freq,ONID,TSID,SID,symbolrate,innerFec,modulation, audioPid,videoPid,teletextPid,pmtPid,bandwidth;
			bool HasEITPresentFollow ,HasEITSchedule ;

			string provider;

			if (checkDVBT)
			{
				GetDVBTTuneRequest(chan.ID,out provider,out freq,out ONID, out TSID,out SID, out audioPid,out videoPid,out teletextPid, out pmtPid, out bandwidth, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out HasEITPresentFollow ,out HasEITSchedule);
				if (ac3Pid>0) return true;
			}
			if (checkDVBC)
			{
				GetDVBCTuneRequest(chan.ID,out provider,out freq, out symbolrate,out innerFec,out modulation,out ONID, out TSID, out SID, out audioPid,out videoPid,out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out HasEITPresentFollow ,out HasEITSchedule );
				if (ac3Pid>0) return true;
			}
			if (checkDVBS)
			{
				DVBChannel ch = new DVBChannel();
				TVDatabase.GetSatChannel(chan.ID,1,ref ch);
				if (ch.AC3Pid>0) return true;
			}
			if (checkATSC)
			{
				int physical,minor,major;
				TVDatabase.GetATSCTuneRequest(chan.ID,out  physical, out provider,out freq, out symbolrate,out innerFec,out modulation,out ONID, out TSID, out SID, out audioPid,out videoPid,out teletextPid, out pmtPid, out audio1,out audio2,out audio3,out ac3Pid, out audioLanguage, out audioLanguage1,out audioLanguage2,out audioLanguage3, out minor, out major, out HasEITPresentFollow ,out HasEITSchedule );
				if (ac3Pid>0) return true;
			}
			return false;
		}


		static public void DeleteNotify(TVNotify notify)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return ;
				try
				{
					string strSQL=String.Format("delete from tblNotifies where idNotify={0}",notify.ID);
					m_db.Execute(strSQL);
				  
			  
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			if (OnNotifiesChanged!=null)
				OnNotifiesChanged();
		}
		static public void AddNotify(TVNotify notify)
		{
			lock (typeof(TVDatabase))
			{
				if (null==m_db) return ;
				try
				{
					string strSQL=String.Format("insert into tblNotifies (idNotify,idProgram) Values (NULL,{0})",notify.Program.ID);
					m_db.Execute(strSQL);
					notify.ID=m_db.LastInsertID();
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			if (OnNotifiesChanged!=null)
				OnNotifiesChanged();
		}

		static public void GetNotifies(ArrayList notifies, bool complete)
		{
			notifies.Clear();
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return ;
					SQLiteResultSet results;

					if (complete)
						strSQL=String.Format( "select * from channel,genre,tblPrograms,tblNotifies where channel.idChannel=tblPrograms.idChannel and tblPrograms.idGenre=genre.idGenre and tblPrograms.idProgram=tblNotifies.idProgram");
					else
						strSQL=String.Format( "select * from tblNotifies");

					results=m_db.Execute(strSQL);
					for (int i=0; i < results.Rows.Count;++i)
					{
						TVNotify notify= new TVNotify();
						notify.Program = new TVProgram();
						TVProgram prog=new TVProgram();
						if (complete)
						{
							notify.Program.Channel=DatabaseUtility.Get(results,i,"channel.strChannel");
							long iStart=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iStartTime"));
							long iEnd=Int64.Parse(DatabaseUtility.Get(results,i,"tblPrograms.iEndTime"));
							notify.Program.Start=iStart;
							notify.Program.End=iEnd;
							notify.Program.Genre=DatabaseUtility.Get(results,i,"genre.strGenre");
							notify.Program.Title=DatabaseUtility.Get(results,i,"tblPrograms.strTitle");
							notify.Program.Description=DatabaseUtility.Get(results,i,"tblPrograms.strDescription");
							notify.Program.Episode=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeName");
							notify.Program.Repeat=DatabaseUtility.Get(results,i,"tblPrograms.strRepeat");
							notify.Program.SeriesNum=DatabaseUtility.Get(results,i,"tblPrograms.strSeriesNum");
							notify.Program.EpisodeNum=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodeNum");
							notify.Program.EpisodePart=DatabaseUtility.Get(results,i,"tblPrograms.strEpisodePart");
							notify.Program.Date=DatabaseUtility.Get(results,i,"tblPrograms.strDate");
							notify.Program.StarRating=DatabaseUtility.Get(results,i,"tblPrograms.strStarRating");
							notify.Program.Classification=DatabaseUtility.Get(results,i,"tblPrograms.strClassification");
							notify.Program.ID=DatabaseUtility.GetAsInt(results,i,"tblNotifies.idProgram");
							notify.ID=DatabaseUtility.GetAsInt(results,i,"tblNotifies.idNotify");
						}
						else
						{
							notify.Program.ID=DatabaseUtility.GetAsInt(results,i,"idProgram");
							notify.ID=DatabaseUtility.GetAsInt(results,i,"idNotify");
						}
						notifies.Add(notify);
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}


		static public void GetNotify(TVNotify notify)
		{
			lock (typeof(TVDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return ;
					SQLiteResultSet results;

					strSQL=String.Format( "select * from channel,genre,tblPrograms,tblNotifies where channel.idChannel=tblPrograms.idChannel and tblPrograms.idGenre=genre.idGenre and tblPrograms.idProgram=tblNotifies.idProgram and tblNotifies.idNotify={0}", notify.ID);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==1)
					{
						notify.Program = new TVProgram();
						TVProgram prog=new TVProgram();
						notify.Program.Channel=DatabaseUtility.Get(results,0,"channel.strChannel");
						long iStart=Int64.Parse(DatabaseUtility.Get(results,0,"tblPrograms.iStartTime"));
						long iEnd=Int64.Parse(DatabaseUtility.Get(results,0,"tblPrograms.iEndTime"));
						notify.Program.Start=iStart;
						notify.Program.End=iEnd;
						notify.Program.Genre=DatabaseUtility.Get(results,0,"genre.strGenre");
						notify.Program.Title=DatabaseUtility.Get(results,0,"tblPrograms.strTitle");
						notify.Program.Description=DatabaseUtility.Get(results,0,"tblPrograms.strDescription");
						notify.Program.Episode=DatabaseUtility.Get(results,0,"tblPrograms.strEpisodeName");
						notify.Program.Repeat=DatabaseUtility.Get(results,0,"tblPrograms.strRepeat");
						notify.Program.SeriesNum=DatabaseUtility.Get(results,0,"tblPrograms.strSeriesNum");
						notify.Program.EpisodeNum=DatabaseUtility.Get(results,0,"tblPrograms.strEpisodeNum");
						notify.Program.EpisodePart=DatabaseUtility.Get(results,0,"tblPrograms.strEpisodePart");
						notify.Program.Date=DatabaseUtility.Get(results,0,"tblPrograms.strDate");
						notify.Program.StarRating=DatabaseUtility.Get(results,0,"tblPrograms.strStarRating");
						notify.Program.Classification=DatabaseUtility.Get(results,0,"tblPrograms.strClassification");
						notify.Program.ID=DatabaseUtility.GetAsInt(results,0,"tblNotifies.idProgram");
						notify.ID=DatabaseUtility.GetAsInt(results,0,"tblNotifies.idNotify");
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"TVDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

	}//public class TVDatabase
}//namespace MediaPortal.TV.Database