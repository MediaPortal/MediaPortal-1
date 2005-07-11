using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using SQLite.NET;
using MediaPortal.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.Radio.Database
{
  public class RadioDatabase
  {
    static SQLiteClient m_db=null;

    // singleton. Dont allow any instance of this class
    private RadioDatabase()
    {
    }

		static RadioDatabase()
		{
			Open();
		}
		static void Open()
		{
      try 
      {
        // Open database
				Log.WriteFile(Log.LogType.Log,false,"open radiodatabase");

				String strPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.
					GetExecutingAssembly().Location); 
				try
				{
					System.IO.Directory.CreateDirectory(strPath+@"\database");
				}
				catch(Exception){}
				m_db = new SQLiteClient(strPath+@"\database\RadioDatabase4.db3");
        CreateTables();

        if (m_db!=null)
        {
          m_db.Execute("PRAGMA cache_size=8192\n");
          m_db.Execute("PRAGMA synchronous='OFF'\n");
          m_db.Execute("PRAGMA count_changes='OFF'\n");
        }

      } 
      catch (Exception ex) 
      {
        Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
      Log.WriteFile(Log.LogType.Log,false,"Radio database opened");
    }
  
    static bool CreateTables()
    {
      if (m_db==null) return false;
      if ( DatabaseUtility.AddTable(m_db,"station","CREATE TABLE station ( idChannel integer primary key, strName text, iChannelNr integer, frequency text, URL text, genre text, bitrate integer, scrambled integer)\n"))
      {
        m_db.Execute("CREATE INDEX idxStation ON station(idChannel)");
      }
			DatabaseUtility.AddTable(m_db,"tblDVBSMapping" ,"CREATE TABLE tblDVBSMapping ( idChannel integer,sPCRPid integer,sTSID integer,sFreq integer,sSymbrate integer,sFEC integer,sLNBKhz integer,sDiseqc integer,sProgramNumber integer,sServiceType integer,sProviderName text,sChannelName text,sEitSched integer,sEitPreFol integer,sAudioPid integer,sVideoPid integer,sAC3Pid integer,sAudio1Pid integer,sAudio2Pid integer,sAudio3Pid integer,sTeletextPid integer,sScrambled integer,sPol integer,sLNBFreq integer,sNetworkID integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text,sECMPid integer,sPMTPid integer)\n");
			DatabaseUtility.AddTable(m_db,"tblDVBCMapping" ,"CREATE TABLE tblDVBCMapping ( idChannel integer, strChannel text, strProvider text, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, pmtPid integer)\n");
			DatabaseUtility.AddTable(m_db,"tblATSCMapping" ,"CREATE TABLE tblATSCMapping ( idChannel integer, strChannel text, strProvider text, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, pmtPid integer, channelNumber integer, minorChannel integer, majorChannel integer)\n");
			DatabaseUtility.AddTable(m_db,"tblDVBTMapping" ,"CREATE TABLE tblDVBTMapping ( idChannel integer, strChannel text, strProvider text, frequency text, bandwidth integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, pmtPid integer)\n");

			//following table specifies which channels can be received by which card
			DatabaseUtility.AddTable(m_db,"tblChannelCard" ,"CREATE TABLE tblChannelCard( idChannelCard integer primary key, idChannel integer, card integer)\n");      

      return true;
    }
    static public void GetStations(ref ArrayList stations)
    {
      stations.Clear();
      if (m_db==null) return ;
      lock (m_db)
      {
        try
        {
          if (null==m_db) return ;
          string strSQL;
          strSQL=String.Format("select * from station order by iChannelNr");
          SQLiteResultSet results;
          results=m_db.Execute(strSQL);
          if (results.Rows.Count== 0) return ;
          for (int i=0; i < results.Rows.Count;++i)
          {
            RadioStation chan=new RadioStation();
            try
            {
              chan.ID=Int32.Parse(DatabaseUtility.Get(results,i,"idChannel"));
            }catch(Exception){}
            try
            {
              chan.Channel = Int32.Parse(DatabaseUtility.Get(results,i,"iChannelNr"));
            }catch(Exception){}
            try
            {
              chan.Frequency = Int64.Parse(DatabaseUtility.Get(results,i,"frequency"));
            }catch(Exception)
            {}
						
						int scrambled=Int32.Parse( DatabaseUtility.Get(results,i,"scrambled"));
						if (scrambled!=0)
							chan.Scrambled=true;
						else
							chan.Scrambled=false;

            chan.Name = DatabaseUtility.Get(results,i,"strName");
            chan.URL = DatabaseUtility.Get(results,i,"URL");
            if (chan.URL.Equals(Strings.Unknown)) chan.URL ="";
            try
            {
            chan.BitRate=Int32.Parse( DatabaseUtility.Get(results,i,"bitrate") );
            }
            catch(Exception){}

            chan.Genre=DatabaseUtility.Get(results,i,"genre") ;
            stations.Add(chan);
          }

          return ;
        }
        catch(Exception ex)
        {
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }
        return ;
      }
    }
		
		static public bool GetStation(string radioName, out RadioStation station)
		{
			station=new RadioStation();
			if (m_db==null) return false;
			lock (m_db)
			{
				try
				{
					if (null==m_db) return false;
					string strSQL;
					strSQL=String.Format("select * from station where strname like '{0}'",radioName);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return false;
					
					RadioStation chan=new RadioStation();
					try
					{
						station.ID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
					}
					catch(Exception){}
					try
					{
						station.Channel = Int32.Parse(DatabaseUtility.Get(results,0,"iChannelNr"));
					}
					catch(Exception){}
					try
					{
						station.Frequency = Int64.Parse(DatabaseUtility.Get(results,0,"frequency"));
					}
					catch(Exception)
					{}
					
					int scrambled=Int32.Parse( DatabaseUtility.Get(results,0,"scrambled"));
					if (scrambled!=0)
						chan.Scrambled=true;
					else
						chan.Scrambled=false;
					station.Name = DatabaseUtility.Get(results,0,"strName");
					station.URL = DatabaseUtility.Get(results,0,"URL");
					if (station.URL.Equals(Strings.Unknown)) chan.URL ="";
					try
					{
						station.BitRate=Int32.Parse( DatabaseUtility.Get(results,0,"bitrate") );
					}
					catch(Exception){}

					station.Genre=DatabaseUtility.Get(results,0,"genre") ;

				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
					return false;
				}
				return true;
			}
		}

		static public void UpdateStation(RadioStation channel)
		{
			lock (m_db)
			{
				string strSQL;
				try
				{
					string strChannel=channel.Name;
					string strURL    =channel.URL;
					string strGenre    =channel.Genre;
					DatabaseUtility.RemoveInvalidChars(ref strChannel);
					DatabaseUtility.RemoveInvalidChars(ref strURL);
					DatabaseUtility.RemoveInvalidChars(ref strGenre);
					int scrambled=0;
					if (channel.Scrambled) scrambled=1;
					strSQL=String.Format("update station set strName='{0}',iChannelNr={1} ,frequency='{2}',URL='{3}',bitrate={4},genre='{5}',scrambled={6} where idChannel={7}", 
																strChannel,channel.Channel,channel.Frequency.ToString(),strURL, channel.BitRate,strGenre, scrambled, channel.ID);
					//Log.WriteFile(Log.LogType.Log,true,strSQL);
					m_db.Execute(strSQL);
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
    
    static public int AddStation(ref RadioStation channel)
    {
        lock (m_db)
        {
          string strSQL;
          try
          {
            string strChannel=channel.Name;
            string strURL    =channel.URL;
            string strGenre    =channel.Genre;
            DatabaseUtility.RemoveInvalidChars(ref strChannel);
            DatabaseUtility.RemoveInvalidChars(ref strURL);
            DatabaseUtility.RemoveInvalidChars(ref strGenre);

            if (null==m_db) return -1;
            SQLiteResultSet results;
            strSQL=String.Format( "select * from station where strName like '{0}'", strChannel);
            results=m_db.Execute(strSQL);
            if (results.Rows.Count==0) 
            {
              // doesnt exists, add it
							int scrambled=0;
							if (channel.Scrambled) scrambled=1;
              strSQL=String.Format("insert into station (idChannel, strName,iChannelNr ,frequency,URL,bitrate,genre,scrambled) values ( NULL, '{0}', {1}, {2}, '{3}',{4},'{5}',{6} )", 
                                    strChannel,channel.Channel,channel.Frequency.ToString(),strURL, channel.BitRate,strGenre, scrambled);
              m_db.Execute(strSQL);
              int iNewID=m_db.LastInsertID();
              channel.ID=iNewID;
              return iNewID;
            }
            else
            {
              int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
              channel.ID=iNewID;
              return iNewID;
            }
          } 
          catch (Exception ex) 
          {
						Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
						Open();
          }

          return -1;
        }
    }

    static public int GetStationId(string strStation)
    {
      lock (m_db)
      {
        string strSQL;
        try
        {
          if (null==m_db) return -1;
          SQLiteResultSet results;
          strSQL=String.Format( "select * from station where strName like '{0}'", strStation);
          results=m_db.Execute(strSQL);
          if (results.Rows.Count==0) return -1;
          int iNewID=Int32.Parse(DatabaseUtility.Get(results,0,"idChannel"));
          return iNewID;
        } 
        catch (Exception ex) 
        {
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }

        return -1;
      }
    }


    static public void RemoveStation(string strStationName)
    {
      lock (m_db)
      {
        if (null==m_db) return ;

        int iChannelId=GetStationId(strStationName);
        if (iChannelId<0) return ;
        
        try
        {
          if (null==m_db) return ;
          string strSQL=String.Format("delete from station where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBSMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblATSCMapping where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblChannelCard where idChannel={0}",iChannelId);
					m_db.Execute(strSQL);
        }
        catch(Exception ex)
        {
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }
      }
    }   
    
    static public void RemoveAllStations()
    {
      lock (m_db)
      {
        if (null==m_db) return ;
        
        try
        {
          if (null==m_db) return ;
          string strSQL=String.Format("delete from station ");
          m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBSMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblATSCMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblChannelCard");
					m_db.Execute(strSQL);

        }
        catch(Exception ex)
        {
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
        }
      }
		}  
    
		static public void RemoveLocalRadioStations()
		{
			lock (m_db)
			{
				if (null==m_db) return ;
        
				try
				{
					if (null==m_db) return ;
					string strSQL=String.Format("delete from station where frequency>0");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBSMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBCMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblDVBTMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblATSCMapping");
					m_db.Execute(strSQL);
					strSQL = String.Format("delete from tblChannelCard");
					m_db.Execute(strSQL);
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public int MapDVBSChannel(int idChannel,int freq,int symrate,int fec,int lnbkhz,int diseqc,
			int prognum,int servicetype,string provider,string channel,int eitsched,
			int eitprefol,int audpid,int vidpid,int ac3pid,int apid1,int apid2,int apid3,
			int teltxtpid,int scrambled,int pol,int lnbfreq,int networkid,int tsid,int pcrpid,string aLangCode,string aLangCode1,string aLangCode2,string aLangCode3,int ecmPid,int pmtPid)
		{
			lock (typeof(RadioDatabase))
			{
				string strSQL;
				try
				{
					DatabaseUtility.RemoveInvalidChars(ref provider);
					DatabaseUtility.RemoveInvalidChars(ref channel);

					string strChannel=channel;
					SQLiteResultSet results=null;

					strSQL=String.Format( "select * from tblDVBSMapping ");
					results=m_db.Execute(strSQL);
					int totalchannels=results.Rows.Count;

					strSQL=String.Format( "select * from tblDVBSMapping where idChannel = {0} and sServiceType={1}", idChannel,servicetype);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{

						strSQL=String.Format("insert into tblDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})", 
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

		static public int MapDVBTChannel(string channelName, string providerName,int idChannel, int frequency, int ONID, int TSID, int SID, int audioPid, int pmtPid, int bandWidth)
		{
			lock (typeof(RadioDatabase))
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

					strSQL=String.Format( "select * from tblDVBTMapping where idChannel like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBTMapping (idChannel, strChannel ,strProvider,frequency , bandwidth , ONID , TSID , SID , audioPid,pmtPid,Visible) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},1)",
							idChannel,strChannel,strProvider,frequency,bandWidth,ONID,TSID,SID,audioPid,pmtPid);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBTMapping set frequency='{0}', ONID={1}, TSID={2}, SID={3}, strChannel='{4}',strProvider='{5}',audioPid={6}, pmtPid={7}, bandwidth={8} where idChannel ={9}", 
							frequency,ONID,TSID,SID,strChannel, strProvider,audioPid,pmtPid, bandWidth,idChannel);
						//	Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

		static public int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate,int innerFec, int modulation,int ONID, int TSID, int SID, int audioPid, int pmtPid)
		{
			lock (typeof(RadioDatabase))
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

					strSQL=String.Format( "select * from tblDVBCMapping where idChannel like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblDVBCMapping (idChannel, strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid,Visible) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},{10},{11},1)"
							,idChannel,strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblDVBCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, pmtPid={10} where idChannel like '{11}'", 
							frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,strChannel, strProvider,audioPid,pmtPid,idChannel);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}


		static public int MapATSCChannel(string channelName, int physicalChannel,int minorChannel,int majorChannel,string providerName, int idChannel, int frequency, int symbolrate,int innerFec, int modulation,int ONID, int TSID, int SID, int audioPid, int pmtPid)
		{
			lock (typeof(RadioDatabase))
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

					strSQL=String.Format( "select * from tblATSCMapping where idChannel like {0}", idChannel);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblATSCMapping (idChannel, strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid,channelNumber,minorChannel,majorChannel,Visible) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},1)"
							,idChannel,strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid,physicalChannel,minorChannel,majorChannel);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						int iNewID=m_db.LastInsertID();
						return idChannel;
					}
					else
					{
						strSQL=String.Format( "update tblATSCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, pmtPid={10}, channelNumber={11},minorChannel={12},majorChannel={13} where idChannel like '{14}'", 
							frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,strChannel, strProvider,audioPid,pmtPid,physicalChannel,minorChannel,majorChannel,idChannel);
						//Log.WriteFile(Log.LogType.Log,true,"sql:{0}", strSQL);
						m_db.Execute(strSQL);
						return idChannel;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}

				return -1;
			}
		}

	
		static public void GetDVBTTuneRequest(int idChannel, out string strProvider,out int frequency, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int bandWidth) 
		{
			pmtPid=-1;
			audioPid=-1;
			strProvider="";
			frequency=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			bandWidth=-1;
			if (m_db == null) return ;
			//Log.WriteFile(Log.LogType.Log,true,"GetTuneRequest for idChannel:{0}", idChannel);
			lock (typeof(RadioDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBTMapping where idChannel={0}",idChannel);
					SQLiteResultSet results;
					results = m_db.Execute(strSQL);
					if (results.Rows.Count != 1) return ;
					frequency=Int32.Parse(DatabaseUtility.Get(results,0,"frequency"));
					ONID=Int32.Parse(DatabaseUtility.Get(results,0,"ONID"));
					TSID=Int32.Parse(DatabaseUtility.Get(results,0,"TSID"));
					SID=Int32.Parse(DatabaseUtility.Get(results,0,"SID"));
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=Int32.Parse(DatabaseUtility.Get(results,0,"audioPid"));
					pmtPid=Int32.Parse(DatabaseUtility.Get(results,0,"pmtPid"));
					bandWidth=Int32.Parse(DatabaseUtility.Get(results,0,"bandwidth"));
					return ;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public void GetDVBCTuneRequest(int idChannel, out string strProvider,out int frequency,out int symbolrate,out int innerFec,out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid) 
		{
			audioPid=0;
			strProvider="";
			frequency=-1;
			symbolrate=-1;
			innerFec=-1;
			modulation=-1;
			pmtPid=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			//Log.WriteFile(Log.LogType.Log,true,"GetTuneRequest for idChannel:{0}", idChannel);
			lock (typeof(RadioDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblDVBCMapping where idChannel={0}",idChannel);
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
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=Int32.Parse(DatabaseUtility.Get(results,0,"audioPid"));
					pmtPid=Int32.Parse(DatabaseUtility.Get(results,0,"pmtPid"));
					return ;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public void GetATSCTuneRequest(int idChannel, out int physicalChannel,out int minorChannel,out int majorChannel,out string strProvider,out int frequency,out int symbolrate,out int innerFec,out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid) 
		{
			minorChannel=-1;
			majorChannel=-1;
			audioPid=0;
			strProvider="";
			frequency=-1;
			physicalChannel=-1;
			symbolrate=-1;
			innerFec=-1;
			modulation=-1;
			pmtPid=-1;
			ONID=-1;
			TSID=-1;
			SID=-1;
			if (m_db == null) return ;
			//Log.WriteFile(Log.LogType.Log,true,"GetTuneRequest for idChannel:{0}", idChannel);
			lock (typeof(RadioDatabase))
			{
				try
				{
					if (null == m_db) return ;
					string strSQL;
					strSQL = String.Format("select * from tblATSCMapping where idChannel={0}",idChannel);
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
					strProvider=DatabaseUtility.Get(results,0,"strProvider");
					audioPid=Int32.Parse(DatabaseUtility.Get(results,0,"audioPid"));
					pmtPid=Int32.Parse(DatabaseUtility.Get(results,0,"pmtPid"));
					physicalChannel=Int32.Parse(DatabaseUtility.Get(results,0,"channelNumber"));
					minorChannel=Int32.Parse(DatabaseUtility.Get(results,0,"minorChannel"));
					majorChannel=Int32.Parse(DatabaseUtility.Get(results,0,"majorChannel"));
					return ;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		static public bool GetDVBSTuneRequest(int idChannel,int serviceType,ref DVBChannel retChannel)
		{
		  
			int freq=0;int symrate=0;int fec=0;int lnbkhz=0;int diseqc=0;
			int prognum=0;int servicetype=0;string provider="";string channel="";int eitsched=0;
			int eitprefol=0;int audpid=0;int vidpid=0;int ac3pid=0;int apid1=0;int apid2=0;int apid3=0;
			int teltxtpid=0;int scrambled=0;int pol=0;int lnbfreq=0;int networkid=0;int tsid=0;int pcrpid=0;
			string audioLang;string audioLang1;string audioLang2;string audioLang3;int ecm;int pmt;
	  
	  
			if (m_db==null) return false;
			lock (typeof(RadioDatabase))
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
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return false;
			}
		}
		static public void MapChannelToCard(int channelId, int card)
		{
			lock (typeof(RadioDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return ;
					SQLiteResultSet results;

					strSQL=String.Format( "select * from tblChannelCard where idChannel={0} and card={1}", channelId,card);

					results=m_db.Execute(strSQL);
					if (results.Rows.Count==0) 
					{
						// doesnt exists, add it
						strSQL=String.Format("insert into tblChannelCard (idChannelCard, idChannel,card) values ( NULL, {0}, {1})", 
							channelId,card);
						m_db.Execute(strSQL);
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}

		static public void DeleteCard(int card)
		{
			lock (typeof(RadioDatabase))
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
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
		}
		static public void UnmapChannelFromCard(RadioStation channel, int card)
		{
			lock (typeof(RadioDatabase))
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
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
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
		static public bool CanCardTuneToStation(string channelName, int card)
		{
			string stationName=channelName;
			DatabaseUtility.RemoveInvalidChars(ref stationName);

			lock (typeof(RadioDatabase))
			{
				string strSQL;
				try
				{
					if (null==m_db) return false;
					SQLiteResultSet results;

					strSQL=String.Format( "select * from tblChannelCard,station where station.idChannel=tblChannelCard.idChannel and station.strName like '{0}' and tblChannelCard.card={1}", stationName,card);
					results=m_db.Execute(strSQL);
					if (results.Rows.Count!=0) 
					{
						return true;
					}
				} 
				catch (Exception ex) 
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
			}
			return false;
		}

		static public void GetStationsForCard( ref ArrayList stations, int card)
		{
			stations.Clear();
			if (m_db==null) return ;
			lock (m_db)
			{
				try
				{
					if (null==m_db) return ;
					string strSQL;
					strSQL=String.Format("select * from station,tblChannelCard where station.idChannel=tblChannelCard.idChannel and tblChannelCard.card={0} order by iChannelNr",card);
					SQLiteResultSet results;
					results=m_db.Execute(strSQL);
					if (results.Rows.Count== 0) return ;
					for (int i=0; i < results.Rows.Count;++i)
					{
						RadioStation chan=new RadioStation();
						try
						{
							chan.ID=Int32.Parse(DatabaseUtility.Get(results,i,"station.idChannel"));
						}
						catch(Exception){}
						try
						{
							chan.Channel = Int32.Parse(DatabaseUtility.Get(results,i,"station.iChannelNr"));
						}
						catch(Exception){}
						try
						{
							chan.Frequency = Int64.Parse(DatabaseUtility.Get(results,i,"station.frequency"));
						}
						catch(Exception)
						{}
						
						int scrambled=Int32.Parse( DatabaseUtility.Get(results,i,"station.scrambled"));
						if (scrambled!=0)
							chan.Scrambled=true;
						else
							chan.Scrambled=false;

						chan.Name = DatabaseUtility.Get(results,i,"station.strName");
						chan.URL = DatabaseUtility.Get(results,i,"station.URL");
						if (chan.URL.Equals(Strings.Unknown)) chan.URL ="";
						try
						{
							chan.BitRate=Int32.Parse( DatabaseUtility.Get(results,i,"station.bitrate") );
						}
						catch(Exception){}

						chan.Genre=DatabaseUtility.Get(results,i,"station.genre") ;
						stations.Add(chan);
					}

					return ;
				}
				catch(Exception ex)
				{
					Log.WriteFile(Log.LogType.Log,true,"RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
					Open();
				}
				return ;
			}
		}


  }
}
