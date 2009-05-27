#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using SQLite.NET;

namespace MediaPortal.Radio.Database
{
  public class RadioDatabaseSqlLite : IRadioDatabase, IDisposable
  {
    public SQLiteClient m_db = null;

    public RadioDatabaseSqlLite()
    {
      Open();
    }

    public void Dispose()
    {
      if (m_db != null)
      {
        m_db.Close();
        m_db.Dispose();
        m_db = null;
      }
    }

    private void Open()
    {
      try
      {
        // Open database
        Log.Info("open radiodatabase");

        try
        {
          Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception)
        {
        }
        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "RadioDatabase4.db3"));

        if (m_db != null)
        {
          DatabaseUtility.SetPragmas(m_db);
        }
        CreateTables();
        UpdateFromPreviousVersion();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      Log.Info("Radio database opened");
    }

    private bool CreateTables()
    {
      if (m_db == null)
      {
        return false;
      }
      if (DatabaseUtility.AddTable(m_db, "station",
                                   "CREATE TABLE station ( idChannel integer primary key, strName text, iChannelNr integer, frequency text, URL text, genre text, bitrate integer, scrambled integer)"))
      {
        try
        {
          m_db.Execute("CREATE INDEX idxStation ON station(idChannel)");
        }
        catch (Exception)
        {
        }
      }
      DatabaseUtility.AddTable(m_db, "tblDVBSMapping",
                               "CREATE TABLE tblDVBSMapping ( idChannel integer,sPCRPid integer,sTSID integer,sFreq integer,sSymbrate integer,sFEC integer,sLNBKhz integer,sDiseqc integer,sProgramNumber integer,sServiceType integer,sProviderName text,sChannelName text,sEitSched integer,sEitPreFol integer,sAudioPid integer,sVideoPid integer,sAC3Pid integer,sAudio1Pid integer,sAudio2Pid integer,sAudio3Pid integer,sTeletextPid integer,sScrambled integer,sPol integer,sLNBFreq integer,sNetworkID integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text,sECMPid integer,sPMTPid integer)");
      DatabaseUtility.AddTable(m_db, "tblDVBCMapping",
                               "CREATE TABLE tblDVBCMapping ( idChannel integer, strChannel text, strProvider text, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, pmtPid integer)");
      DatabaseUtility.AddTable(m_db, "tblATSCMapping",
                               "CREATE TABLE tblATSCMapping ( idChannel integer, strChannel text, strProvider text, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, pmtPid integer, channelNumber integer, minorChannel integer, majorChannel integer)");
      DatabaseUtility.AddTable(m_db, "tblDVBTMapping",
                               "CREATE TABLE tblDVBTMapping ( idChannel integer, strChannel text, strProvider text, frequency text, bandwidth integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, pmtPid integer)");
      DatabaseUtility.AddTable(m_db, "tblversion", "CREATE TABLE tblversion( idVersion integer)");

      //following table specifies which channels can be received by which card
      DatabaseUtility.AddTable(m_db, "tblChannelCard",
                               "CREATE TABLE tblChannelCard( idChannelCard integer primary key, idChannel integer, card integer)");

      return true;
    }

    public void UpdateFromPreviousVersion()
    {
      int currentVersion = 3;
      int versionNr = 0;
      SQLiteResultSet results;
      results = m_db.Execute("SELECT * FROM tblversion");
      if (results.Rows.Count >= 1)
      {
        SQLiteResultSet.Row row = results.Rows[0];
        versionNr = Int32.Parse(row.fields[0]);
      }
      else
      {
        m_db.Execute(String.Format("insert into tblversion (idVersion) values({0})", currentVersion));
      }
      if (versionNr == 0)
      {
        //add pcr pid column to mapping tables
        m_db.Execute("ALTER TABLE tblDVBCMapping ADD COLUMN pcrPid Integer");
        m_db.Execute("ALTER TABLE tblATSCMapping ADD COLUMN pcrPid Integer");
        m_db.Execute("ALTER TABLE tblDVBTMapping ADD COLUMN pcrPid Integer");
        m_db.Execute(String.Format("update tblversion set idVersion={0}", currentVersion));
      }
      if (versionNr < 2)
      {
        m_db.Execute("ALTER TABLE station ADD COLUMN isort Integer");
        m_db.Execute(String.Format("update station set isort=0"));
      }


      if (versionNr < 3)
      {
        m_db.Execute("ALTER TABLE station ADD COLUMN epgHours Integer");
        m_db.Execute("update station set epgHours=1");

        DateTime dtStart = new DateTime(1971, 11, 6);
        m_db.Execute("ALTER TABLE station ADD COLUMN epgLastUpdate text");
        m_db.Execute(String.Format("update station set epglastupdate='{0}'", Util.Utils.datetolong(dtStart)));

        if (DatabaseUtility.AddTable(m_db, "tblPrograms",
                                     "CREATE TABLE tblPrograms ( idProgram integer primary key, idChannel integer, idGenre integer, strTitle text, iStartTime integer, iEndTime text, strDescription text,strEpisodeName text,strRepeat text,strSeriesNum text,strEpisodeNum text,strEpisodePart text,strDate text,strStarRating text,strClassification text)"))
        {
          try
          {
            m_db.Execute("CREATE INDEX idxProgram ON tblPrograms(idChannel,iStartTime,iEndTime)");
          }
          catch (Exception)
          {
          }
        }

        if (DatabaseUtility.AddTable(m_db, "genre", "CREATE TABLE genre ( idGenre integer primary key, strGenre text)"))
        {
          try
          {
            m_db.Execute("CREATE INDEX idxGenre ON genre(strGenre)");
          }
          catch (Exception)
          {
          }
        }
      }

      m_db.Execute(String.Format("update tblversion set idVersion={0}", currentVersion));
    }

    public void ClearAll()
    {
      m_db.Execute("delete from station");
      m_db.Execute("delete from tblDVBSMapping");
      m_db.Execute("delete from tblDVBCMapping");
      m_db.Execute("delete from tblATSCMapping");
      m_db.Execute("delete from tblDVBTMapping");
      m_db.Execute("delete from tblChannelCard");
    }

    public void GetStations(ref ArrayList stations)
    {
      stations.Clear();
      if (m_db == null)
      {
        return;
      }
      lock (m_db)
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL;
          strSQL = String.Format("select * from station order by isort");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            RadioStation chan = new RadioStation();
            try
            {
              chan.ID = Int32.Parse(DatabaseUtility.Get(results, i, "idChannel"));
            }
            catch (Exception)
            {
            }
            try
            {
              chan.Channel = Int32.Parse(DatabaseUtility.Get(results, i, "iChannelNr"));
            }
            catch (Exception)
            {
            }
            try
            {
              chan.Frequency = Int64.Parse(DatabaseUtility.Get(results, i, "frequency"));
            }
            catch (Exception)
            {
            }

            int scrambled = Int32.Parse(DatabaseUtility.Get(results, i, "scrambled"));
            if (scrambled != 0)
            {
              chan.Scrambled = true;
            }
            else
            {
              chan.Scrambled = false;
            }

            chan.Name = DatabaseUtility.Get(results, i, "strName");
            chan.URL = DatabaseUtility.Get(results, i, "URL");
            if (chan.URL.Equals(Strings.Unknown))
            {
              chan.URL = "";
            }
            try
            {
              chan.BitRate = Int32.Parse(DatabaseUtility.Get(results, i, "bitrate"));
            }
            catch (Exception)
            {
            }
            chan.Sort = Int32.Parse(DatabaseUtility.Get(results, i, "isort"));

            chan.Genre = DatabaseUtility.Get(results, i, "genre");
            chan.EpgHours = DatabaseUtility.GetAsInt(results, i, "epgHours");
            chan.LastDateTimeEpgGrabbed = Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, i, "epgLastUpdate"));
            stations.Add(chan);
          }

          return;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
        return;
      }
    }

    public bool GetStation(string radioName, out RadioStation station)
    {
      station = new RadioStation();
      if (m_db == null)
      {
        return false;
      }
      lock (m_db)
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          string radioNameFiltered = radioName;
          DatabaseUtility.RemoveInvalidChars(ref radioNameFiltered);
          strSQL = String.Format("select * from station where strname like '{0}'", radioNameFiltered);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }

          RadioStation chan = new RadioStation();
          try
          {
            station.ID = Int32.Parse(DatabaseUtility.Get(results, 0, "idChannel"));
          }
          catch (Exception)
          {
          }
          try
          {
            station.Channel = Int32.Parse(DatabaseUtility.Get(results, 0, "iChannelNr"));
          }
          catch (Exception)
          {
          }
          try
          {
            station.Frequency = Int64.Parse(DatabaseUtility.Get(results, 0, "frequency"));
          }
          catch (Exception)
          {
          }

          int scrambled = Int32.Parse(DatabaseUtility.Get(results, 0, "scrambled"));
          if (scrambled != 0)
          {
            chan.Scrambled = true;
          }
          else
          {
            chan.Scrambled = false;
          }
          station.Name = DatabaseUtility.Get(results, 0, "strName");
          station.URL = DatabaseUtility.Get(results, 0, "URL");
          if (station.URL.Equals(Strings.Unknown))
          {
            chan.URL = "";
          }
          try
          {
            station.BitRate = Int32.Parse(DatabaseUtility.Get(results, 0, "bitrate"));
          }
          catch (Exception)
          {
          }

          station.Channel = Int32.Parse(DatabaseUtility.Get(results, 0, "isort"));
          station.Genre = DatabaseUtility.Get(results, 0, "genre");
          station.EpgHours = DatabaseUtility.GetAsInt(results, 0, "epgHours");
          station.LastDateTimeEpgGrabbed = Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, 0, "epgLastUpdate"));
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
          return false;
        }
        return true;
      }
    }

    public void UpdateStation(RadioStation channel)
    {
      lock (m_db)
      {
        string strSQL;
        try
        {
          string strChannel = channel.Name;
          string strURL = channel.URL;
          string strGenre = channel.Genre;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strURL);
          DatabaseUtility.RemoveInvalidChars(ref strGenre);
          int scrambled = 0;
          if (channel.Scrambled)
          {
            scrambled = 1;
          }
          strSQL =
            String.Format(
              "update station set strName='{0}',iChannelNr={1} ,frequency='{2}',URL='{3}',bitrate={4},genre='{5}',scrambled={6},isort={7},epgLastUpdate='{8}',epgHours={9} where idChannel={10}",
              strChannel, channel.Channel, channel.Frequency.ToString(), strURL, channel.BitRate, strGenre, scrambled,
              channel.Sort, Util.Utils.datetolong(channel.LastDateTimeEpgGrabbed), channel.EpgHours, channel.ID);
          //Log.Error(strSQL);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public int AddStation(ref RadioStation channel)
    {
      lock (m_db)
      {
        string strSQL;
        try
        {
          string strChannel = channel.Name;
          string strURL = channel.URL;
          string strGenre = channel.Genre;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strURL);
          DatabaseUtility.RemoveInvalidChars(ref strGenre);

          if (null == m_db)
          {
            return -1;
          }
          SQLiteResultSet results;
          strSQL = String.Format("select * from station where strName like '{0}'", strChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            int scrambled = 0;
            if (channel.Scrambled)
            {
              scrambled = 1;
            }
            strSQL =
              String.Format(
                "insert into station (idChannel, strName,iChannelNr ,frequency,URL,bitrate,genre,scrambled,isort,epgLastUpdate,epgHours) values ( NULL, '{0}', {1}, {2}, '{3}',{4},'{5}',{6},{7},'{8}',{9} )",
                strChannel, channel.Channel, channel.Frequency.ToString(), strURL, channel.BitRate, strGenre, scrambled,
                channel.Sort, Util.Utils.datetolong(channel.LastDateTimeEpgGrabbed), channel.EpgHours);
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            channel.ID = iNewID;
            return iNewID;
          }
          else
          {
            int iNewID = Int32.Parse(DatabaseUtility.Get(results, 0, "idChannel"));
            channel.ID = iNewID;
            return iNewID;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }

        return -1;
      }
    }

    public int GetStationId(string strStation)
    {
      lock (m_db)
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return -1;
          }
          SQLiteResultSet results;
          string radioNameFiltered = strStation;
          DatabaseUtility.RemoveInvalidChars(ref radioNameFiltered);
          strSQL = String.Format("select * from station where strName like '{0}'", radioNameFiltered);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return -1;
          }
          int iNewID = Int32.Parse(DatabaseUtility.Get(results, 0, "idChannel"));
          return iNewID;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }

        return -1;
      }
    }


    public void RemoveStation(string strStationName)
    {
      lock (m_db)
      {
        if (null == m_db)
        {
          return;
        }

        int iChannelId = GetStationId(strStationName);
        if (iChannelId < 0)
        {
          return;
        }

        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("delete from station where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblDVBSMapping where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblDVBCMapping where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblDVBTMapping where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblATSCMapping where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblChannelCard where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public void RemoveAllStations()
    {
      lock (m_db)
      {
        if (null == m_db)
        {
          return;
        }

        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("delete from station ");
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
          strSQL = String.Format("delete from genre");
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblprograms");
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public void RemoveLocalRadioStations()
    {
      lock (m_db)
      {
        if (null == m_db)
        {
          return;
        }

        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("delete from station where frequency>0");
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
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public int MapDVBSChannel(int idChannel, int freq, int symrate, int fec, int lnbkhz, int diseqc,
                              int prognum, int servicetype, string provider, string channel, int eitsched,
                              int eitprefol, int audpid, int vidpid, int ac3pid, int apid1, int apid2, int apid3,
                              int teltxtpid, int scrambled, int pol, int lnbfreq, int networkid, int tsid, int pcrpid,
                              string aLangCode, string aLangCode1, string aLangCode2, string aLangCode3, int ecmPid,
                              int pmtPid)
    {
      lock (typeof (RadioDatabase))
      {
        string strSQL;
        try
        {
          DatabaseUtility.RemoveInvalidChars(ref provider);
          DatabaseUtility.RemoveInvalidChars(ref channel);

          string strChannel = channel;
          SQLiteResultSet results = null;

          strSQL = String.Format("select * from tblDVBSMapping ");
          results = m_db.Execute(strSQL);
          int totalchannels = results.Rows.Count;

          strSQL = String.Format("select * from tblDVBSMapping where idChannel = {0} and sServiceType={1}", idChannel,
                                 servicetype);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            strSQL =
              String.Format(
                "insert into tblDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})",
                idChannel, freq, symrate, fec, lnbkhz, diseqc,
                prognum, servicetype, provider, channel, eitsched,
                eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
                teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid, aLangCode, aLangCode1, aLangCode2,
                aLangCode3, ecmPid, pmtPid);

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
          Log.Error(ex);
          Open();
        }

        return -1;
      }
    }

    public int MapDVBTChannel(string channelName, string providerName, int idChannel, int frequency, int ONID, int TSID,
                              int SID, int audioPid, int pmtPid, int bandWidth, int pcrPid)
    {
      lock (typeof (RadioDatabase))
      {
        if (null == m_db)
        {
          return -1;
        }
        string strSQL;
        try
        {
          string strChannel = channelName;
          string strProvider = providerName;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strProvider);

          SQLiteResultSet results;

          strSQL = String.Format("select * from tblDVBTMapping where idChannel like {0}", idChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL =
              String.Format(
                "insert into tblDVBTMapping (idChannel, strChannel ,strProvider,frequency , bandwidth , ONID , TSID , SID , audioPid,pmtPid,Visible,pcrPid) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},1,{10})",
                idChannel, strChannel, strProvider, frequency, bandWidth, ONID, TSID, SID, audioPid, pmtPid, pcrPid);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            return idChannel;
          }
          else
          {
            strSQL =
              String.Format(
                "update tblDVBTMapping set frequency='{0}', ONID={1}, TSID={2}, SID={3}, strChannel='{4}',strProvider='{5}',audioPid={6}, pmtPid={7}, bandwidth={8},pcrPid={9} where idChannel ={10}",
                frequency, ONID, TSID, SID, strChannel, strProvider, audioPid, pmtPid, bandWidth, pcrPid, idChannel);
            //	Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            return idChannel;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }

        return -1;
      }
    }

    public int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate,
                              int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid,
                              int pcrPid)
    {
      lock (typeof (RadioDatabase))
      {
        if (null == m_db)
        {
          return -1;
        }
        string strSQL;
        try
        {
          string strChannel = channelName;
          string strProvider = providerName;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strProvider);

          SQLiteResultSet results;

          strSQL = String.Format("select * from tblDVBCMapping where idChannel like {0}", idChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL =
              String.Format(
                "insert into tblDVBCMapping (idChannel, strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid,Visible,pcrPid) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},{10},{11},1,{12})"
                , idChannel, strChannel, strProvider, frequency, symbolrate, innerFec, modulation, ONID, TSID, SID,
                audioPid, pmtPid, pcrPid);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            return idChannel;
          }
          else
          {
            strSQL =
              String.Format(
                "update tblDVBCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, pmtPid={10},pcrPid={11} where idChannel like '{12}'",
                frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, strChannel, strProvider, audioPid, pmtPid,
                pcrPid, idChannel);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            return idChannel;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }

        return -1;
      }
    }


    public int MapATSCChannel(string channelName, int physicalChannel, int minorChannel, int majorChannel,
                              string providerName, int idChannel, int frequency, int symbolrate, int innerFec,
                              int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid)
    {
      lock (typeof (RadioDatabase))
      {
        if (null == m_db)
        {
          return -1;
        }
        string strSQL;
        try
        {
          string strChannel = channelName;
          string strProvider = providerName;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strProvider);

          SQLiteResultSet results;

          strSQL = String.Format("select * from tblATSCMapping where idChannel like {0}", idChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL =
              String.Format(
                "insert into tblATSCMapping (idChannel, strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid,channelNumber,minorChannel,majorChannel,Visible,pcrPid) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},1,{15})"
                , idChannel, strChannel, strProvider, frequency, symbolrate, innerFec, modulation, ONID, TSID, SID,
                audioPid, pmtPid, physicalChannel, minorChannel, majorChannel, pcrPid);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            return idChannel;
          }
          else
          {
            strSQL =
              String.Format(
                "update tblATSCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, pmtPid={10}, channelNumber={11},minorChannel={12},majorChannel={13},pcrPid={14} where idChannel like '{15}'",
                frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, strChannel, strProvider, audioPid, pmtPid,
                physicalChannel, minorChannel, majorChannel, pcrPid, idChannel);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            return idChannel;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }

        return -1;
      }
    }


    public void GetDVBTTuneRequest(int idChannel, out string strProvider, out int frequency, out int ONID, out int TSID,
                                   out int SID, out int audioPid, out int pmtPid, out int bandWidth, out int pcrPid)
    {
      pmtPid = -1;
      audioPid = -1;
      strProvider = "";
      frequency = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      bandWidth = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return;
      }
      //Log.Error("GetTuneRequest for idChannel:{0}", idChannel);
      lock (typeof (RadioDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL;
          strSQL = String.Format("select * from tblDVBTMapping where idChannel={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return;
          }
          frequency = Int32.Parse(DatabaseUtility.Get(results, 0, "frequency"));
          ONID = Int32.Parse(DatabaseUtility.Get(results, 0, "ONID"));
          TSID = Int32.Parse(DatabaseUtility.Get(results, 0, "TSID"));
          SID = Int32.Parse(DatabaseUtility.Get(results, 0, "SID"));
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = Int32.Parse(DatabaseUtility.Get(results, 0, "audioPid"));
          pmtPid = Int32.Parse(DatabaseUtility.Get(results, 0, "pmtPid"));
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");
          bandWidth = Int32.Parse(DatabaseUtility.Get(results, 0, "bandwidth"));
          return;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public void GetDVBCTuneRequest(int idChannel, out string strProvider, out int frequency, out int symbolrate,
                                   out int innerFec, out int modulation, out int ONID, out int TSID, out int SID,
                                   out int audioPid, out int pmtPid, out int pcrPid)
    {
      audioPid = 0;
      strProvider = "";
      frequency = -1;
      symbolrate = -1;
      innerFec = -1;
      modulation = -1;
      pmtPid = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return;
      }
      //Log.Error("GetTuneRequest for idChannel:{0}", idChannel);
      lock (typeof (RadioDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL;
          strSQL = String.Format("select * from tblDVBCMapping where idChannel={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return;
          }
          frequency = Int32.Parse(DatabaseUtility.Get(results, 0, "frequency"));
          symbolrate = Int32.Parse(DatabaseUtility.Get(results, 0, "symbolrate"));
          innerFec = Int32.Parse(DatabaseUtility.Get(results, 0, "innerFec"));
          modulation = Int32.Parse(DatabaseUtility.Get(results, 0, "modulation"));
          ONID = Int32.Parse(DatabaseUtility.Get(results, 0, "ONID"));
          TSID = Int32.Parse(DatabaseUtility.Get(results, 0, "TSID"));
          SID = Int32.Parse(DatabaseUtility.Get(results, 0, "SID"));
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = Int32.Parse(DatabaseUtility.Get(results, 0, "audioPid"));
          pmtPid = Int32.Parse(DatabaseUtility.Get(results, 0, "pmtPid"));
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");
          return;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public void GetATSCTuneRequest(int idChannel, out int physicalChannel, out int minorChannel, out int majorChannel,
                                   out string strProvider, out int frequency, out int symbolrate, out int innerFec,
                                   out int modulation, out int ONID, out int TSID, out int SID, out int audioPid,
                                   out int pmtPid, out int pcrPid)
    {
      minorChannel = -1;
      majorChannel = -1;
      audioPid = 0;
      strProvider = "";
      frequency = -1;
      physicalChannel = -1;
      symbolrate = -1;
      innerFec = -1;
      modulation = -1;
      pmtPid = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return;
      }
      //Log.Error("GetTuneRequest for idChannel:{0}", idChannel);
      lock (typeof (RadioDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL;
          strSQL = String.Format("select * from tblATSCMapping where idChannel={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return;
          }
          frequency = Int32.Parse(DatabaseUtility.Get(results, 0, "frequency"));
          symbolrate = Int32.Parse(DatabaseUtility.Get(results, 0, "symbolrate"));
          innerFec = Int32.Parse(DatabaseUtility.Get(results, 0, "innerFec"));
          modulation = Int32.Parse(DatabaseUtility.Get(results, 0, "modulation"));
          ONID = Int32.Parse(DatabaseUtility.Get(results, 0, "ONID"));
          TSID = Int32.Parse(DatabaseUtility.Get(results, 0, "TSID"));
          SID = Int32.Parse(DatabaseUtility.Get(results, 0, "SID"));
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = Int32.Parse(DatabaseUtility.Get(results, 0, "audioPid"));
          pmtPid = Int32.Parse(DatabaseUtility.Get(results, 0, "pmtPid"));
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");
          physicalChannel = Int32.Parse(DatabaseUtility.Get(results, 0, "channelNumber"));
          minorChannel = Int32.Parse(DatabaseUtility.Get(results, 0, "minorChannel"));
          majorChannel = Int32.Parse(DatabaseUtility.Get(results, 0, "majorChannel"));

          return;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public bool GetDVBSTuneRequest(int idChannel, int serviceType, ref DVBChannel retChannel)
    {
      int freq = 0;
      int symrate = 0;
      int fec = 0;
      int lnbkhz = 0;
      int diseqc = 0;
      int prognum = 0;
      int servicetype = 0;
      string provider = "";
      string channel = "";
      int eitsched = 0;
      int eitprefol = 0;
      int audpid = 0;
      int vidpid = 0;
      int ac3pid = 0;
      int apid1 = 0;
      int apid2 = 0;
      int apid3 = 0;
      int teltxtpid = 0;
      int scrambled = 0;
      int pol = 0;
      int lnbfreq = 0;
      int networkid = 0;
      int tsid = 0;
      int pcrpid = 0;
      string audioLang;
      string audioLang1;
      string audioLang2;
      string audioLang3;
      int ecm;
      int pmt;


      if (m_db == null)
      {
        return false;
      }
      lock (typeof (RadioDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from tblDVBSMapping where idChannel={0} and sServiceType={1}", idChannel,
                                 serviceType);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count < 1)
          {
            return false;
          }
          else
          {
            //chan.ID=Int32.Parse(DatabaseUtility.Get(results,i,"idChannel"));
            //chan.Number = Int32.Parse(DatabaseUtility.Get(results,i,"iChannelNr"));
            // sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName
            //sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,
            //sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID
            int i = 0;
            freq = Int32.Parse(DatabaseUtility.Get(results, i, "sFreq"));
            symrate = Int32.Parse(DatabaseUtility.Get(results, i, "sSymbrate"));
            fec = Int32.Parse(DatabaseUtility.Get(results, i, "sFEC"));
            lnbkhz = Int32.Parse(DatabaseUtility.Get(results, i, "sLNBKhz"));
            diseqc = Int32.Parse(DatabaseUtility.Get(results, i, "sDiseqc"));
            prognum = Int32.Parse(DatabaseUtility.Get(results, i, "sProgramNumber"));
            servicetype = Int32.Parse(DatabaseUtility.Get(results, i, "sServiceType"));
            provider = DatabaseUtility.Get(results, i, "sProviderName");
            channel = DatabaseUtility.Get(results, i, "sChannelName");
            eitsched = Int32.Parse(DatabaseUtility.Get(results, i, "sEitSched"));
            eitprefol = Int32.Parse(DatabaseUtility.Get(results, i, "sEitPreFol"));
            audpid = Int32.Parse(DatabaseUtility.Get(results, i, "sAudioPid"));
            vidpid = Int32.Parse(DatabaseUtility.Get(results, i, "sVideoPid"));
            ac3pid = Int32.Parse(DatabaseUtility.Get(results, i, "sAC3Pid"));
            apid1 = Int32.Parse(DatabaseUtility.Get(results, i, "sAudio1Pid"));
            apid2 = Int32.Parse(DatabaseUtility.Get(results, i, "sAudio2Pid"));
            apid3 = Int32.Parse(DatabaseUtility.Get(results, i, "sAudio3Pid"));
            teltxtpid = Int32.Parse(DatabaseUtility.Get(results, i, "sTeletextPid"));
            scrambled = Int32.Parse(DatabaseUtility.Get(results, i, "sScrambled"));
            pol = Int32.Parse(DatabaseUtility.Get(results, i, "sPol"));
            lnbfreq = Int32.Parse(DatabaseUtility.Get(results, i, "sLNBFreq"));
            networkid = Int32.Parse(DatabaseUtility.Get(results, i, "sNetworkID"));
            tsid = Int32.Parse(DatabaseUtility.Get(results, i, "sTSID"));
            pcrpid = Int32.Parse(DatabaseUtility.Get(results, i, "sPCRPid"));
            // sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid
            audioLang = DatabaseUtility.Get(results, i, "sAudioLang");
            audioLang1 = DatabaseUtility.Get(results, i, "sAudioLang1");
            audioLang2 = DatabaseUtility.Get(results, i, "sAudioLang2");
            audioLang3 = DatabaseUtility.Get(results, i, "sAudioLang3");
            ecm = Int32.Parse(DatabaseUtility.Get(results, i, "sECMPid"));
            pmt = Int32.Parse(DatabaseUtility.Get(results, i, "sPMTPid"));
            retChannel = new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
                                        prognum, servicetype, provider, channel, eitsched,
                                        eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
                                        teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid, audioLang,
                                        audioLang1, audioLang2, audioLang3, ecm, pmt);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
        return false;
      }
    }

    public void MapChannelToCard(int channelId, int card)
    {
      lock (typeof (RadioDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;

          strSQL = String.Format("select * from tblChannelCard where idChannel={0} and card={1}", channelId, card);

          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL = String.Format(
              "insert into tblChannelCard (idChannelCard, idChannel,card) values ( NULL, {0}, {1})",
              channelId, card);
            m_db.Execute(strSQL);
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public void DeleteCard(int card)
    {
      lock (typeof (RadioDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          //delete this card
          strSQL = String.Format("delete from tblChannelCard where card={0}", card);
          m_db.Execute(strSQL);

          //adjust the mapping for the other cards
          strSQL = String.Format("select * from tblChannelCard where card > {0}", card);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            int id = Int32.Parse(DatabaseUtility.Get(results, i, "idChannelCard"));
            int cardnr = Int32.Parse(DatabaseUtility.Get(results, i, "card"));
            cardnr--;
            strSQL = String.Format("update tblChannelCard set card={0} where idChannelCard={1}",
                                   cardnr, id);
            m_db.Execute(strSQL);
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
    }

    public void UnmapChannelFromCard(RadioStation channel, int card)
    {
      lock (typeof (RadioDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          strSQL = String.Format("delete from tblChannelCard where idChannel={0} and card={1}",
                                 channel.ID, card);

          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
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
    public bool CanCardTuneToStation(string channelName, int card)
    {
      string stationName = channelName;
      DatabaseUtility.RemoveInvalidChars(ref stationName);

      lock (typeof (RadioDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return false;
          }
          SQLiteResultSet results;

          strSQL =
            String.Format(
              "select * from tblChannelCard,station where station.idChannel=tblChannelCard.idChannel and station.strName like '{0}' and tblChannelCard.card={1}",
              stationName, card);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 0)
          {
            return true;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
      return false;
    }

    public void GetStationsForCard(ref ArrayList stations, int card)
    {
      stations.Clear();
      if (m_db == null)
      {
        return;
      }
      lock (m_db)
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL;
          strSQL =
            String.Format(
              "select * from station,tblChannelCard where station.idChannel=tblChannelCard.idChannel and tblChannelCard.card={0} order by iChannelNr",
              card);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            RadioStation chan = new RadioStation();
            try
            {
              chan.ID = Int32.Parse(DatabaseUtility.Get(results, i, "station.idChannel"));
            }
            catch (Exception)
            {
            }
            try
            {
              chan.Channel = Int32.Parse(DatabaseUtility.Get(results, i, "station.iChannelNr"));
            }
            catch (Exception)
            {
            }
            try
            {
              chan.Frequency = Int64.Parse(DatabaseUtility.Get(results, i, "station.frequency"));
            }
            catch (Exception)
            {
            }

            int scrambled = Int32.Parse(DatabaseUtility.Get(results, i, "station.scrambled"));
            if (scrambled != 0)
            {
              chan.Scrambled = true;
            }
            else
            {
              chan.Scrambled = false;
            }

            chan.Name = DatabaseUtility.Get(results, i, "station.strName");
            chan.URL = DatabaseUtility.Get(results, i, "station.URL");
            if (chan.URL.Equals(Strings.Unknown))
            {
              chan.URL = "";
            }
            try
            {
              chan.BitRate = Int32.Parse(DatabaseUtility.Get(results, i, "station.bitrate"));
            }
            catch (Exception)
            {
            }

            chan.Sort = Int32.Parse(DatabaseUtility.Get(results, i, "station.isort"));
            chan.Genre = DatabaseUtility.Get(results, i, "station.genre");
            chan.EpgHours = DatabaseUtility.GetAsInt(results, i, "epgHours");
            chan.LastDateTimeEpgGrabbed = Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, i, "epgLastUpdate"));
            stations.Add(chan);
          }

          return;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
        return;
      }
    }

    public RadioStation GetStationByStream(bool atsc, bool dvbt, bool dvbc, bool dvbs, int networkid, int transportid,
                                           int serviceid, out string provider)
    {
      int freq, symbolrate, innerFec, modulation, ONID, TSID, SID;
      int audioPid, pmtPid, bandWidth;
      int pcrPid;
      provider = "";

      DVBChannel ch = new DVBChannel();
      ArrayList channels = new ArrayList();
      GetStations(ref channels);
      foreach (RadioStation chan in channels)
      {
        if (dvbc)
        {
          GetDVBCTuneRequest(chan.ID, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID,
                             out TSID, out SID, out audioPid, out pmtPid, out pcrPid);
          if (serviceid == SID && transportid == TSID)
          {
            return chan;
          }
        }
        if (dvbs)
        {
          GetDVBSTuneRequest(chan.ID, 2, ref ch);
          if (ch.TransportStreamID == transportid && ch.ProgramNumber == serviceid)
          {
            return chan;
          }
        }

        if (dvbt)
        {
          GetDVBTTuneRequest(chan.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid, out pmtPid,
                             out bandWidth, out pcrPid);
          if (serviceid == SID && transportid == TSID)
          {
            return chan;
          }
        }
      }
      provider = "";
      return null;
    }

    public void UpdatePids(bool isATSC, bool isDVBC, bool isDVBS, bool isDVBT, DVBChannel channel)
    {
    }

    #region EPG

    public int AddProgram(TVProgram prog)
    {
      int lRetId = -1;
      lock (m_db)
      {
        string strSQL;
        try
        {
          string strGenre = prog.Genre;
          string strTitle = prog.Title;
          string strDescription = prog.Description;
          string strEpisode = prog.Episode;
          string strRepeat = prog.Repeat;
          string strDate = prog.Date;
          string strSeriesNum = prog.SeriesNum;
          string strEpisodeNum = prog.EpisodeNum;
          string strEpisodePart = prog.EpisodePart;
          string strStarRating = prog.StarRating;
          string strClassification = prog.Classification;
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


          if (null == m_db)
          {
            return -1;
          }
          int iGenreId = AddGenre(strGenre);
          int iChannelId = GetStationId(prog.Channel);
          if (iChannelId < 0)
          {
            return -1;
          }

          strSQL =
            String.Format(
              "insert into tblPrograms (idProgram,idChannel,idGenre,strTitle,iStartTime,iEndTime,strDescription,strEpisodeName,strRepeat) values ( NULL, {0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
              iChannelId, iGenreId, strTitle, prog.Start.ToString(),
              prog.End.ToString(), strDescription, strEpisode, strRepeat);
          m_db.Execute(strSQL);
          lRetId = m_db.LastInsertID();
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
      return lRetId;
    }

    public int AddGenre(string strGenre1)
    {
      lock (m_db)
      {
        string strSQL;
        try
        {
          string strGenre = strGenre1;
          DatabaseUtility.RemoveInvalidChars(ref strGenre);
          if (null == m_db)
          {
            return -1;
          }

          SQLiteResultSet results;
          strSQL = String.Format("select * from genre where strGenre like '{0}'", strGenre);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL = String.Format("insert into genre (idGenre, strGenre) values ( NULL, '{0}' )",
                                   strGenre);
            m_db.Execute(strSQL);
            int iNewId = m_db.LastInsertID();

            return iNewId;
          }
          else
          {
            int iID = DatabaseUtility.GetAsInt(results, 0, "idGenre");
            return iID;
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }

        return -1;
      }
    }

    public int UpdateProgram(TVProgram prog)
    {
      int lRetId = -1;
      lock (m_db)
      {
        string strSQL;
        try
        {
          string strGenre = prog.Genre;
          string strTitle = prog.Title;
          string strDescription = prog.Description;
          string strEpisode = prog.Episode;
          string strRepeat = prog.Repeat;
          string strSeriesNum = prog.SeriesNum;
          string strEpisodeNum = prog.EpisodeNum;
          string strEpisodePart = prog.EpisodePart;
          string strDate = prog.Date;
          string strStarRating = prog.StarRating;
          string strClassification = prog.Classification;
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

          if (null == m_db)
          {
            return -1;
          }
          int iGenreId = AddGenre(strGenre);
          int iChannelId = GetStationId(prog.Channel);
          if (iChannelId < 0)
          {
            return -1;
          }

          //check if program is already in database
          //check if other programs exist between the start - finish time of this program
          long endTime = Util.Utils.datetolong(prog.EndTime.AddMinutes(-1));

          strSQL = String.Format("SELECT * FROM tblPrograms WHERE idChannel={0} AND ", iChannelId);
          strSQL += String.Format("  ( ('{0}' <= iStartTime and '{1}' >= iStartTime) or  ",
                                  prog.Start.ToString(), endTime.ToString());
          strSQL += String.Format("    ('{0}' >= iStartTime and '{1}' >= iStartTime and '{2}' < iEndTime) )",
                                  prog.Start.ToString(), endTime.ToString(), prog.Start.ToString());
          //  Log.Info("sql:{0} {1}-{2} {3}", prog.Channel, prog.Start.ToString(), endTime.ToString(), strSQL);
          SQLiteResultSet results2;
          results2 = m_db.Execute(strSQL);
          if (results2.Rows.Count > 0)
          {
            long idProgram = DatabaseUtility.GetAsInt64(results2, 0, "idProgram");
            return (int) idProgram; //program already exists
            /*
            //and delete them
            for (int i = 0; i < results2.Rows.Count; ++i)
            {
               idProgram = DatabaseUtility.GetAsInt64(results2, i, "idProgram");
              //Log.Info("sql: del {0} id:{1} {2}-{3}", i, idProgram,DatabaseUtility.Get(results2, i, "iStartTime"), DatabaseUtility.Get(results2, i, "iEndTime"));
              strSQL = String.Format("DELETE FROM tblPrograms WHERE idProgram={0}", idProgram);
              m_db.Execute(strSQL);
            }*/
          }
          // then add the new shows
          strSQL =
            String.Format(
              "insert into tblPrograms (idProgram,idChannel,idGenre,strTitle,iStartTime,iEndTime,strDescription,strEpisodeName,strRepeat,strSeriesNum,strEpisodeNum,strEpisodePart,strDate,strStarRating,strClassification) values ( NULL, {0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')",
              iChannelId, iGenreId, strTitle, prog.Start.ToString(),
              prog.End.ToString(), strDescription, strEpisode, strRepeat, strSeriesNum, strEpisodeNum, strEpisodePart,
              strDate, strStarRating, strClassification);
          //          Log.WriteFile(LogType.EPG,strSQL);
          m_db.Execute(strSQL);
          lRetId = m_db.LastInsertID();
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
      }
      return lRetId;
    }

    /// <summary>
    /// RemoveOldPrograms()
    /// Deletes all tv programs from the database which ended more then 1 day ago
    /// suppose its now 10 november 2004 11:07 am
    /// then this function will remove all programs which endtime is before 9 november 2004 11:07
    /// </summary>
    public void RemoveOldPrograms()
    {
      Log.WriteFile(LogType.EPG, "RemoveOldPrograms()");
      if (m_db == null)
      {
        return;
      }
      lock (m_db)
      {
        //delete programs from database that are more than 1 day old
        string strSQL = string.Empty;
        try
        {
          DateTime yesterday = DateTime.Today.AddDays(-1);
          long longYesterday = Util.Utils.datetolong(yesterday);
          strSQL = String.Format("DELETE FROM tblPrograms WHERE iEndTime < '{0}'", longYesterday);
          m_db.Execute(strSQL);

          DatabaseUtility.CompactDatabase(m_db);

          Log.WriteFile(LogType.EPG, "vacuum done");
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
        return;
      }
    }


    public bool GetGenres(ref List<string> genres)
    {
      lock (m_db)
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          genres.Clear();
          strSQL = String.Format("select * from genre");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            int id = DatabaseUtility.GetAsInt(results, i, "idGenre");
            string genre = DatabaseUtility.Get(results, i, "strGenre");
            genres.Add(genre);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
        return false;
      }
    }

    public bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime, ref List<TVProgram> progs)
    {
      lock (m_db)
      {
        string strSQL = string.Empty;
        progs.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          if (strChannel1 == null)
          {
            return false;
          }
          string strChannel = strChannel1;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);

          string strOrder = " order by iStartTime";
          strSQL =
            String.Format(
              "select * from station,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=station.idChannel and station.strName like '{0}' ",
              strChannel);
          string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                       iStartTime, iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime>='{0}' and tblPrograms.iStartTime <= '{1}' ) ", iStartTime,
                                 iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime<='{0}' and tblPrograms.iEndTime >= '{1}') )", iStartTime,
                                 iEndTime);
          strSQL += where;
          strSQL += strOrder;


          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results == null)
          {
            return false;
          }
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iEndTime");
            bool bAdd = false;
            if (iEnd >= iStartTime && iEnd <= iEndTime)
            {
              bAdd = true;
            }
            if (iStart >= iStartTime && iStart <= iEndTime)
            {
              bAdd = true;
            }
            if (iStart <= iStartTime && iEnd >= iEndTime)
            {
              bAdd = true;
            }
            if (bAdd)
            {
              TVProgram prog = new TVProgram();
              prog.Channel = DatabaseUtility.Get(results, i, "station.strName");
              prog.Start = iStart;
              prog.End = iEnd;
              prog.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
              prog.Title = DatabaseUtility.Get(results, i, "tblPrograms.strTitle");
              prog.Description = DatabaseUtility.Get(results, i, "tblPrograms.strDescription");
              prog.Episode = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeName");
              prog.Repeat = DatabaseUtility.Get(results, i, "tblPrograms.strRepeat");
              prog.ID = DatabaseUtility.GetAsInt(results, i, "tblPrograms.idProgram");
              prog.SeriesNum = DatabaseUtility.Get(results, i, "tblPrograms.strSeriesNum");
              prog.EpisodeNum = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeNum");
              prog.EpisodePart = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodePart");
              prog.Date = DatabaseUtility.Get(results, i, "tblPrograms.strDate");
              prog.StarRating = DatabaseUtility.Get(results, i, "tblPrograms.strStarRating");
              prog.Classification = DatabaseUtility.Get(results, i, "tblPrograms.strClassification");
              progs.Add(prog);
            }
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Open();
        }
        return false;
      }
    }

    #endregion
  }
}