#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Data;
using System.Data.SqlClient;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;

namespace MediaPortal.Radio.Database
{
  public class RadioDatabaseADO : IRadioDatabase, IDisposable
  {
    private SqlConnection _connection;

    public RadioDatabaseADO()
    {
      string connectionString;
      using (Settings reader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        connectionString = reader.GetValueAsString("database", "connectionstring",
                                                   SqlServerUtility.DefaultConnectionString);
      }
      _connection = new SqlConnection(connectionString);
      _connection.Open();
      CreateTables();
      UpdateFromPreviousVersion();
    }

    public void Dispose()
    {
      if (_connection != null)
      {
        _connection.Close();
        _connection.Dispose();
        _connection = null;
      }
    }


    private void CreateTables()
    {
      SqlServerUtility.AddTable(_connection, "tblRadioStation",
                                "CREATE TABLE tblRadioStation ( idChannel int IDENTITY(1,1) NOT NULL, strName varchar(2048), iChannelNr int, frequency bigint, URL varchar(2048), genre varchar(2048), bitrate int, scrambled int)");
      SqlServerUtility.AddPrimaryKey(_connection, "tblRadioStation", "idChannel");
      SqlServerUtility.AddIndex(_connection, "idxStation", "CREATE INDEX idxStation ON tblRadioStation(idChannel)");

      SqlServerUtility.AddTable(_connection, "tblRadioDVBSMapping",
                                "CREATE TABLE tblRadioDVBSMapping ( idChannel int,sPCRPid int,sTSID int,sFreq int,sSymbrate int,sFEC int,sLNBKhz int,sDiseqc int,sProgramNumber int,sServiceType int,sProviderName varchar(2048),sChannelName varchar(2048),sEitSched int,sEitPreFol int,sAudioPid int,sVideoPid int,sAC3Pid int,sAudio1Pid int,sAudio2Pid int,sAudio3Pid int,sTeletextPid int,sScrambled int,sPol int,sLNBFreq int,sNetworkID int,sAudioLang varchar(2048),sAudioLang1 varchar(2048),sAudioLang2 varchar(2048),sAudioLang3 varchar(2048),sECMPid int,sPMTPid int)");
      SqlServerUtility.AddTable(_connection, "tblRadioDVBCMapping",
                                "CREATE TABLE tblRadioDVBCMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), symbolrate int, innerFec int, modulation int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int)");
      SqlServerUtility.AddTable(_connection, "tblRadioATSCMapping",
                                "CREATE TABLE tblRadioATSCMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), symbolrate int, innerFec int, modulation int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int, channelNumber int, minorChannel int, majorChannel int)");
      SqlServerUtility.AddTable(_connection, "tblRadioDVBTMapping",
                                "CREATE TABLE tblRadioDVBTMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), bandwidth int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int)");
      SqlServerUtility.AddTable(_connection, "tblRadioVersion", "CREATE TABLE tblRadioVersion( idVersion int)");

      //following table specifies which channels can be received by which card
      SqlServerUtility.AddTable(_connection, "tblRadioChannelCard",
                                "CREATE TABLE tblRadioChannelCard( idChannelCard int primary key, idChannel int, card int)");
    }

    private void UpdateFromPreviousVersion()
    {
      int currentVersion = 3;
      int versionNr = 0;
      bool found = false;
      string strSQL = "SELECT * FROM tblRadioVersion";
      using (SqlCommand cmd = _connection.CreateCommand())
      {
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = strSQL;
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            versionNr = (int) reader[0];
            found = true;
            reader.Close();
          }
          else
          {
            reader.Close();
          }
        }
      }
      if (false == found)
      {
        SqlServerUtility.InsertRecord(_connection,
                                      String.Format("insert into tblRadioVersion (idVersion) values({0})",
                                                    currentVersion));
      }
      if (versionNr == 0)
      {
        //add pcr pid column to mapping tables
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioDVBCMapping ADD pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioATSCMapping ADD pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioDVBTMapping ADD pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection,
                                         String.Format("update tblRadioVersion set idVersion={0}", currentVersion));
      }
      if (versionNr < 2)
      {
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioStation ADD isort int");
        SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update tblRadioStation set isort=0"));
      }


      if (versionNr < 3)
      {
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioStation ADD epgHours int");
        SqlServerUtility.ExecuteNonQuery(_connection, "update tblRadioStation set epgHours=1");

        DateTime dtStart = new DateTime(1971, 11, 6);
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioStation ADD epgLastUpdate datetime");
        SqlServerUtility.ExecuteNonQuery(_connection,
                                         String.Format("update tblRadioStation set epglastupdate='{0}'", dtStart));

        SqlServerUtility.AddTable(_connection, "tblRadioPrograms",
                                  "CREATE TABLE tblRadioPrograms ( idProgram int IDENTITY(1,1) NOT NULL, idChannel int, idGenre int, strTitle varchar(2048), iStartTime int, iEndTime int, strDescription varchar(2048),strEpisodeName varchar(2048),strRepeat varchar(2048),strSeriesNum varchar(2048),strEpisodeNum varchar(2048),strEpisodePart varchar(2048),strDate datetime,strStarRating varchar(2048),strClassification varchar(2048))");
        SqlServerUtility.AddPrimaryKey(_connection, "tblRadioPrograms", "idProgram");


        SqlServerUtility.AddTable(_connection, "tblRadioGenre",
                                  "CREATE TABLE tblRadioGenre ( idGenre int IDENTITY(1,1) NOT NULL, strGenre varchar(2048))");
        SqlServerUtility.AddPrimaryKey(_connection, "tblRadioGenre", "idGenre");
        SqlServerUtility.AddIndex(_connection, "idxStation", "CREATE INDEX idxGenre ON tblRadioGenre(strGenre)");
      }

      SqlServerUtility.ExecuteNonQuery(_connection,
                                       String.Format("update tblRadioVersion set idVersion={0}", currentVersion));
    }

    public void ClearAll()
    {
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioPrograms");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioGenre");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioStation");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioDVBSMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioDVBCMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioATSCMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioDVBTMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblRadioChannelCard");
    }

    public void GetStations(ref ArrayList stations)
    {
      stations.Clear();
      try
      {
        string strSQL;
        strSQL = String.Format("select * from tblRadioStation order by isort");
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandText = strSQL;
          cmd.CommandType = CommandType.Text;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              RadioStation chan = new RadioStation();
              try
              {
                chan.ID = (int) reader["idChannel"];
              }
              catch (Exception)
              {
              }
              try
              {
                chan.Channel = (int) reader["iChannelNr"];
              }
              catch (Exception)
              {
              }
              try
              {
                chan.Frequency = (long) reader["frequency"];
              }
              catch (Exception)
              {
              }

              int scrambled = (int) reader["scrambled"];
              if (scrambled != 0)
              {
                chan.Scrambled = true;
              }
              else
              {
                chan.Scrambled = false;
              }

              chan.Name = (string) reader["strName"];
              chan.URL = (string) reader["URL"];
              if (chan.URL.Equals(Strings.Unknown))
              {
                chan.URL = "";
              }
              try
              {
                chan.BitRate = (int) reader["bitrate"];
              }
              catch (Exception)
              {
              }
              chan.Sort = (int) reader["isort"];

              chan.Genre = (string) reader["genre"];
              chan.EpgHours = (int) reader["epgHours"];
              try
              {
                chan.LastDateTimeEpgGrabbed = (DateTime) reader["epgLastUpdate"];
              }
              catch (Exception)
              {
              }
              stations.Add(chan);
            }
            reader.Close();
          }
        }
      }
      catch (Exception)
      {
      }
      return;
    }


    public bool GetStation(string radioName, out RadioStation station)
    {
      station = new RadioStation();
      try
      {
        string strSQL;
        string radioNameFiltered = radioName;
        DatabaseUtility.RemoveInvalidChars(ref radioNameFiltered);
        strSQL = String.Format("select * from tblRadioStation where strname like '{0}'", radioNameFiltered);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandText = strSQL;
          cmd.CommandType = CommandType.Text;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              RadioStation chan = new RadioStation();
              try
              {
                station.ID = (int) reader["idChannel"];
              }
              catch (Exception)
              {
              }
              try
              {
                station.Channel = (int) reader["iChannelNr"];
              }
              catch (Exception)
              {
              }
              try
              {
                station.Frequency = (long) reader["frequency"];
              }
              catch (Exception)
              {
              }

              int scrambled = (int) reader["scrambled"];
              if (scrambled != 0)
              {
                chan.Scrambled = true;
              }
              else
              {
                chan.Scrambled = false;
              }
              station.Name = (string) reader["strName"];
              station.URL = (string) reader["URL"];
              if (station.URL.Equals(Strings.Unknown))
              {
                chan.URL = "";
              }
              try
              {
                station.BitRate = (int) reader["bitrate"];
              }
              catch (Exception)
              {
              }

              station.Channel = (int) reader["isort"];
              station.Genre = (string) reader["genre"];
              station.EpgHours = (int) reader["epgHours"];
              try
              {
                station.LastDateTimeEpgGrabbed = (DateTime) reader["epgLastUpdate"];
              }
              catch (Exception)
              {
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        return false;
      }
      return true;
    }

    public void UpdateStation(RadioStation channel)
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
            "update tblRadioStation set strName='{0}',iChannelNr={1} ,frequency={2},URL='{3}',bitrate={4},genre='{5}',scrambled={6},isort={7},epgLastUpdate='{8}',epgHours={9} where idChannel={10}",
            strChannel, channel.Channel, channel.Frequency.ToString(), strURL,
            channel.BitRate, strGenre, scrambled, channel.Sort,
            channel.LastDateTimeEpgGrabbed,
            channel.EpgHours, channel.ID);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public int AddStation(ref RadioStation channel)
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

        strSQL = String.Format("select * from tblRadioStation where strName like '{0}'", strChannel);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandText = strSQL;
          cmd.CommandType = CommandType.Text;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (!reader.Read())
            {
              // doesnt exists, add it
              int scrambled = 0;
              if (channel.Scrambled)
              {
                scrambled = 1;
              }
              strSQL =
                String.Format(
                  "insert into tblRadioStation (strName,iChannelNr ,frequency,URL,bitrate,genre,scrambled,isort,epgLastUpdate,epgHours) values ( '{0}', {1}, {2}, '{3}',{4},'{5}',{6},{7},'{8}',{9} )",
                  strChannel, channel.Channel, channel.Frequency.ToString(),
                  strURL, channel.BitRate, strGenre, scrambled, channel.Sort,
                  channel.LastDateTimeEpgGrabbed, channel.EpgHours);
              int iNewID = SqlServerUtility.InsertRecord(_connection, strSQL);
              channel.ID = iNewID;
              return iNewID;
            }
            else
            {
              int iNewID = (int) reader["idChannel"];
              channel.ID = iNewID;
              return iNewID;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return -1;
    }

    public int GetStationId(string strStation)
    {
      string strSQL;
      try
      {
        string radioNameFiltered = strStation;
        DatabaseUtility.RemoveInvalidChars(ref radioNameFiltered);
        strSQL = String.Format("select * from tblRadioStation where strName like '{0}'", radioNameFiltered);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandText = strSQL;
          cmd.CommandType = CommandType.Text;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (!reader.Read())
            {
              return -1;
            }
            int iNewID = (int) reader["idChannel"];
            return iNewID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return -1;
    }

    public void RemoveStation(string strStationName)
    {
      int iChannelId = GetStationId(strStationName);
      if (iChannelId < 0)
      {
        return;
      }

      try
      {
        string strSQL = String.Format("delete from tblRadioStation where idChannel={0}", iChannelId);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioDVBSMapping where idChannel={0}", iChannelId);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioDVBCMapping where idChannel={0}", iChannelId);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioDVBTMapping where idChannel={0}", iChannelId);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioATSCMapping where idChannel={0}", iChannelId);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioChannelCard where idChannel={0}", iChannelId);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void RemoveAllStations()
    {
      try
      {
        string strSQL = String.Format("delete from tblRadioStation ");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioDVBSMapping");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioDVBCMapping");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioDVBTMapping");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioATSCMapping");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioChannelCard");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioGenre");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioPrograms");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void RemoveLocalRadioStations()
    {
      try
      {
        string strSQL = String.Format("delete from tblRadioStation where frequency>0");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public int MapDVBSChannel(int idChannel, int freq, int symrate, int fec, int lnbkhz, int diseqc,
                              int prognum, int servicetype, string provider, string channel, int eitsched,
                              int eitprefol, int audpid, int vidpid, int ac3pid, int apid1, int apid2, int apid3,
                              int teltxtpid, int scrambled, int pol, int lnbfreq, int networkid, int tsid, int pcrpid,
                              string aLangCode, string aLangCode1, string aLangCode2, string aLangCode3, int ecmPid,
                              int pmtPid)
    {
      string strSQL;
      try
      {
        DatabaseUtility.RemoveInvalidChars(ref provider);
        DatabaseUtility.RemoveInvalidChars(ref channel);

        string strChannel = channel;

        strSQL = String.Format("select * from tblRadioDVBSMapping ");
        int totalchannels = SqlServerUtility.GetRowCount(_connection, strSQL);

        strSQL = String.Format("select * from tblRadioDVBSMapping where idChannel = {0} and sServiceType={1}", idChannel,
                               servicetype);
        int rowCount = SqlServerUtility.GetRowCount(_connection, strSQL);
        if (rowCount == 0)
        {
          strSQL =
            String.Format(
              "insert into tblRadioDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})",
              idChannel, freq, symrate, fec, lnbkhz, diseqc,
              prognum, servicetype, provider, channel, eitsched,
              eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
              teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid, aLangCode, aLangCode1, aLangCode2, aLangCode3,
              ecmPid, pmtPid);

          SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
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
      }

      return -1;
    }

    public int MapDVBTChannel(string channelName, string providerName, int idChannel, int frequency, int ONID, int TSID,
                              int SID, int audioPid, int pmtPid, int bandWidth, int pcrPid)
    {
      string strSQL;
      try
      {
        string strChannel = channelName;
        string strProvider = providerName;
        DatabaseUtility.RemoveInvalidChars(ref strChannel);
        DatabaseUtility.RemoveInvalidChars(ref strProvider);


        strSQL = String.Format("select * from tblRadioDVBTMapping where idChannel like {0}", idChannel);
        int rowCount = SqlServerUtility.GetRowCount(_connection, strSQL);
        if (rowCount == 0)
        {
          // doesnt exists, add it
          strSQL =
            String.Format(
              "insert into tblRadioDVBTMapping (idChannel, strChannel ,strProvider,frequency , bandwidth , ONID , TSID , SID , audioPid,pmtPid,Visible,pcrPid) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},1,{10})",
              idChannel, strChannel, strProvider, frequency, bandWidth, ONID, TSID, SID, audioPid, pmtPid, pcrPid);
          SqlServerUtility.InsertRecord(_connection, strSQL);
          return idChannel;
        }
        else
        {
          strSQL =
            String.Format(
              "update tblRadioDVBTMapping set frequency='{0}', ONID={1}, TSID={2}, SID={3}, strChannel='{4}',strProvider='{5}',audioPid={6}, pmtPid={7}, bandwidth={8},pcrPid={9} where idChannel ={10}",
              frequency, ONID, TSID, SID, strChannel, strProvider, audioPid, pmtPid, bandWidth, pcrPid, idChannel);
          //	Log.Error("sql:{0}", strSQL);
          SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
          return idChannel;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return -1;
    }

    public int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate,
                              int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid,
                              int pcrPid)
    {
      string strSQL;
      try
      {
        string strChannel = channelName;
        string strProvider = providerName;
        DatabaseUtility.RemoveInvalidChars(ref strChannel);
        DatabaseUtility.RemoveInvalidChars(ref strProvider);

        strSQL = String.Format("select * from tblRadioDVBCMapping where idChannel like {0}", idChannel);
        int rowCount = SqlServerUtility.GetRowCount(_connection, strSQL);
        if (rowCount == 0)
        {
          // doesnt exists, add it
          strSQL =
            String.Format(
              "insert into tblRadioDVBCMapping (idChannel, strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid,Visible,pcrPid) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},{10},{11},1,{12})"
              , idChannel, strChannel, strProvider, frequency, symbolrate, innerFec, modulation, ONID, TSID, SID,
              audioPid, pmtPid, pcrPid);
          //Log.Error("sql:{0}", strSQL);
          SqlServerUtility.InsertRecord(_connection, strSQL);
          return idChannel;
        }
        else
        {
          strSQL =
            String.Format(
              "update tblRadioDVBCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, pmtPid={10},pcrPid={11} where idChannel like '{12}'",
              frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, strChannel, strProvider, audioPid, pmtPid,
              pcrPid, idChannel);
          //Log.Error("sql:{0}", strSQL);
          SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
          return idChannel;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return -1;
    }

    public int MapATSCChannel(string channelName, int physicalChannel, int minorChannel, int majorChannel,
                              string providerName, int idChannel, int frequency, int symbolrate, int innerFec,
                              int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid)
    {
      string strSQL;
      try
      {
        string strChannel = channelName;
        string strProvider = providerName;
        DatabaseUtility.RemoveInvalidChars(ref strChannel);
        DatabaseUtility.RemoveInvalidChars(ref strProvider);


        strSQL = String.Format("select * from tblRadioATSCMapping where idChannel like {0}", idChannel);
        int rowCount = SqlServerUtility.GetRowCount(_connection, strSQL);
        if (rowCount == 0)
        {
          // doesnt exists, add it
          strSQL =
            String.Format(
              "insert into tblRadioATSCMapping (idChannel, strChannel,strProvider,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,pmtPid,channelNumber,minorChannel,majorChannel,Visible,pcrPid) Values( {0}, '{1}', '{2}', '{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},1,{15})"
              , idChannel, strChannel, strProvider, frequency, symbolrate, innerFec, modulation, ONID, TSID, SID,
              audioPid, pmtPid, physicalChannel, minorChannel, majorChannel, pcrPid);
          //Log.Error("sql:{0}", strSQL);
          SqlServerUtility.InsertRecord(_connection, strSQL);
          return idChannel;
        }
        else
        {
          strSQL =
            String.Format(
              "update tblRadioATSCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, pmtPid={10}, channelNumber={11},minorChannel={12},majorChannel={13},pcrPid={14} where idChannel like '{15}'",
              frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, strChannel, strProvider, audioPid, pmtPid,
              physicalChannel, minorChannel, majorChannel, pcrPid, idChannel);
          //Log.Error("sql:{0}", strSQL);
          SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
          return idChannel;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return -1;
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
      try
      {
        string strSQL;
        strSQL = String.Format("select * from tblRadioDVBTMapping where idChannel={0}", idChannel);

        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              frequency = (int) reader["frequency"];
              ONID = (int) reader["ONID"];
              TSID = (int) reader["TSID"];
              SID = (int) reader["SID"];
              strProvider = (string) reader["strProvider"];
              audioPid = (int) reader["audioPid"];
              pmtPid = (int) reader["pmtPid"];
              pcrPid = (int) reader["pcrPid"];
              bandWidth = (int) reader["bandwidth"];
              return;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
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
      try
      {
        string strSQL;
        strSQL = String.Format("select * from tblRadioDVBCMapping where idChannel={0}", idChannel);

        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              frequency = (int) reader["frequency"];
              symbolrate = (int) reader["symbolrate"];
              innerFec = (int) reader["innerFec"];
              modulation = (int) reader["modulation"];
              ONID = (int) reader["ONID"];
              TSID = (int) reader["TSID"];
              SID = (int) reader["SID"];
              strProvider = (string) reader["strProvider"];
              audioPid = (int) reader["audioPid"];
              pmtPid = (int) reader["pmtPid"];
              pcrPid = (int) reader["pcrPid"];
              return;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
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
      try
      {
        string strSQL;
        strSQL = String.Format("select * from tblRadioATSCMapping where idChannel={0}", idChannel);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              frequency = (int) reader["frequency"];
              symbolrate = (int) reader["symbolrate"];
              innerFec = (int) reader["innerFec"];
              modulation = (int) reader["modulation"];
              ONID = (int) reader["ONID"];
              TSID = (int) reader["TSID"];
              SID = (int) reader["SID"];
              strProvider = (string) reader["strProvider"];
              audioPid = (int) reader["audioPid"];
              pmtPid = (int) reader["pmtPid"];
              pcrPid = (int) reader["pcrPid"];
              physicalChannel = (int) reader["channelNumber"];
              minorChannel = (int) reader["minorChannel"];
              majorChannel = (int) reader["majorChannel"];
            }
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
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


      try
      {
        string strSQL;
        strSQL = String.Format("select * from tblRadioDVBSMapping where idChannel={0} and sServiceType={1}", idChannel,
                               serviceType);

        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              //int i = 0;
              freq = (int) reader["sFreq"];
              symrate = (int) reader["sSymbrate"];
              fec = (int) reader["sFEC"];
              lnbkhz = (int) reader["sLNBKhz"];
              diseqc = (int) reader["sDiseqc"];
              prognum = (int) reader["sProgramNumber"];
              servicetype = (int) reader["sServiceType"];
              provider = (string) reader["sProviderName"];
              channel = (string) reader["sChannelName"];
              eitsched = (int) reader["sEitSched"];
              eitprefol = (int) reader["sEitPreFol"];
              audpid = (int) reader["sAudioPid"];
              vidpid = (int) reader["sVideoPid"];
              ac3pid = (int) reader["sAC3Pid"];
              apid1 = (int) reader["sAudio1Pid"];
              apid2 = (int) reader["sAudio2Pid"];
              apid3 = (int) reader["sAudio3Pid"];
              teltxtpid = (int) reader["sTeletextPid"];
              scrambled = (int) reader["sScrambled"];
              pol = (int) reader["sPol"];
              lnbfreq = (int) reader["sLNBFreq"];
              networkid = (int) reader["sNetworkID"];
              tsid = (int) reader["sTSID"];
              pcrpid = (int) reader["sPCRPid"];
              // sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid
              audioLang = (string) reader["sAudioLang"];
              audioLang1 = (string) reader["sAudioLang1"];
              audioLang2 = (string) reader["sAudioLang2"];
              audioLang3 = (string) reader["sAudioLang3"];
              ecm = (int) reader["sECMPid"];
              pmt = (int) reader["sPMTPid"];
              retChannel = new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
                                          prognum, servicetype, provider, channel, eitsched,
                                          eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
                                          teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid, audioLang,
                                          audioLang1, audioLang2, audioLang3, ecm, pmt);
            }

            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return false;
    }

    public void MapChannelToCard(int channelId, int card)
    {
      string strSQL;
      try
      {
        strSQL = String.Format("select * from tblRadioChannelCard where idChannel={0} and card={1}", channelId, card);

        if (SqlServerUtility.GetRowCount(_connection, strSQL) == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into tblRadioChannelCard (idChannel,card) values ( {0}, {1})", channelId, card);
          SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void DeleteCard(int card)
    {
      string strSQL;
      try
      {
        //delete this card
        strSQL = String.Format("delete from tblRadioChannelCard where card={0}", card);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);

        //adjust the mapping for the other cards
        strSQL = String.Format("select * from tblRadioChannelCard where card > {0}", card);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              int id = (int) reader["idChannelCard"];
              int cardnr = (int) reader["card"];
              cardnr--;
              strSQL = String.Format("update tblRadioChannelCard set card={0} where idChannelCard={1}", cardnr, id);
              SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void UnmapChannelFromCard(RadioStation channel, int card)
    {
      string strSQL;
      try
      {
        strSQL = String.Format("delete from tblRadioChannelCard where idChannel={0} and card={1}", channel.ID, card);

        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public bool CanCardTuneToStation(string channelName, int card)
    {
      string stationName = channelName;
      DatabaseUtility.RemoveInvalidChars(ref stationName);

      string strSQL;
      try
      {
        strSQL =
          String.Format(
            "select * from tblRadioChannelCard,tblRadioStation where tblRadioStation.idChannel=tblRadioChannelCard.idChannel and tblRadioStation.strName like '{0}' and tblRadioChannelCard.card={1}",
            stationName, card);
        int rowCount = SqlServerUtility.GetRowCount(_connection, strSQL);
        if (rowCount != 0)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return false;
    }

    public void GetStationsForCard(ref ArrayList stations, int card)
    {
      stations.Clear();
      try
      {
        string strSQL;
        strSQL =
          String.Format(
            "select * from tblRadioStation,tblRadioChannelCard where tblRadioStation.idChannel=tblRadioChannelCard.idChannel and tblRadioChannelCard.card={0} order by iChannelNr",
            card);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              RadioStation chan = new RadioStation();
              try
              {
                chan.ID = (int) reader["tblRadioStation.idChannel"];
              }
              catch (Exception)
              {
              }
              try
              {
                chan.Channel = (int) reader["tblRadioStation.iChannelNr"];
              }
              catch (Exception)
              {
              }
              try
              {
                chan.Frequency = (int) reader["tblRadioStation.frequency"];
              }
              catch (Exception)
              {
              }

              int scrambled = (int) reader["tblRadioStation.scrambled"];
              if (scrambled != 0)
              {
                chan.Scrambled = true;
              }
              else
              {
                chan.Scrambled = false;
              }

              chan.Name = (string) reader["tblRadioStation.strName"];
              chan.URL = (string) reader["tblRadioStation.URL"];
              if (chan.URL.Equals(Strings.Unknown))
              {
                chan.URL = "";
              }
              try
              {
                chan.BitRate = (int) reader["tblRadioStation.bitrate"];
              }
              catch (Exception)
              {
              }

              chan.Sort = (int) reader["tblRadioStation.isort"];
              chan.Genre = (string) reader["tblRadioStation.genre"];
              chan.EpgHours = (int) reader["epgHours"];
              chan.LastDateTimeEpgGrabbed = (DateTime) reader["epgLastUpdate"];
              stations.Add(chan);
            }
          }
        }

        return;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return;
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

    public int AddProgram(TVProgram prog)
    {
      //int lRetId = -1;
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


        int iGenreId = AddGenre(strGenre);
        int iChannelId = GetStationId(prog.Channel);
        if (iChannelId < 0)
        {
          return -1;
        }

        strSQL =
          String.Format(
            "insert into tblRadioPrograms (idChannel,idGenre,strTitle,iStartTime,iEndTime,strDescription,strEpisodeName,strRepeat) values ( {0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
            iChannelId, iGenreId, strTitle, prog.Start.ToString(),
            prog.End.ToString(), strDescription, strEpisode, strRepeat);
        return SqlServerUtility.InsertRecord(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return -1;
    }

    public int AddGenre(string strGenre1)
    {
      string strSQL;
      try
      {
        string strGenre = strGenre1;
        DatabaseUtility.RemoveInvalidChars(ref strGenre);

        strSQL = String.Format("select * from tblRadioGenre where strGenre like '{0}'", strGenre);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              int iID = (int) reader["idGenre"];
              return iID;
            }
          }
        }

        // doesnt exists, add it
        strSQL = String.Format("insert into tblRadioGenre (strGenre) values ( '{0}' )", strGenre);
        return SqlServerUtility.InsertRecord(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }

      return -1;
    }

    public int UpdateProgram(TVProgram prog)
    {
      //int lRetId = -1;
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

        int iGenreId = AddGenre(strGenre);
        int iChannelId = GetStationId(prog.Channel);
        if (iChannelId < 0)
        {
          return -1;
        }

        //check if program is already in database
        //check if other programs exist between the start - finish time of this program
        long endTime = Util.Utils.datetolong(prog.EndTime.AddMinutes(-1));

        strSQL = String.Format("SELECT * FROM tblRadioPrograms WHERE idChannel={0} AND ", iChannelId);
        strSQL += String.Format("  ( ('{0}' <= iStartTime and '{1}' >= iStartTime) or  ",
                                prog.Start.ToString(), endTime.ToString());
        strSQL += String.Format("    ('{0}' >= iStartTime and '{1}' >= iStartTime and '{2}' < iEndTime) )",
                                prog.Start.ToString(), endTime.ToString(), prog.Start.ToString());
        //  Log.Info("sql:{0} {1}-{2} {3}", prog.Channel, prog.Start.ToString(), endTime.ToString(), strSQL);
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              long idProgram = (int) reader["idProgram"];
              return (int) idProgram; //program already exists
            }
          }
        }
        // then add the new shows
        strSQL =
          String.Format(
            "insert into tblRadioPrograms (idChannel,idGenre,strTitle,iStartTime,iEndTime,strDescription,strEpisodeName,strRepeat,strSeriesNum,strEpisodeNum,strEpisodePart,strDate,strStarRating,strClassification) values ( {0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')",
            iChannelId, iGenreId, strTitle, prog.Start.ToString(),
            prog.End.ToString(), strDescription, strEpisode, strRepeat, strSeriesNum, strEpisodeNum, strEpisodePart,
            strDate, strStarRating, strClassification);
        //          Log.WriteFile(LogType.EPG,strSQL);
        return SqlServerUtility.InsertRecord(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return -1;
    }

    public void RemoveOldPrograms()
    {
      //delete programs from database that are more than 1 day old
      string strSQL = string.Empty;
      try
      {
        DateTime yesterday = DateTime.Today.AddDays(-1);
        long longYesterday = Util.Utils.datetolong(yesterday);
        strSQL = String.Format("DELETE FROM tblRadioPrograms WHERE iEndTime < '{0}'", longYesterday);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return;
    }

    public bool GetGenres(ref List<string> genres)
    {
      try
      {
        string strSQL;
        genres.Clear();
        strSQL = String.Format("select * from tblRadioGenre");
        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              int id = (int) reader["idGenre"];
              string genre = (string) reader["strGenre"];
              genres.Add(genre);
            }
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return false;
    }

    public bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime, ref List<TVProgram> progs)
    {
      string strSQL = string.Empty;
      progs.Clear();
      try
      {
        if (strChannel1 == null)
        {
          return false;
        }
        string strChannel = strChannel1;
        DatabaseUtility.RemoveInvalidChars(ref strChannel);

        string strOrder = " order by iStartTime";
        strSQL =
          String.Format(
            "select * from tblRadiostation,tblRadioPrograms,tblRadiogenre where tblRadiogenre.idGenre=tblRadioPrograms.idGenre and tblRadioPrograms.idChannel=tblRadiostation.idChannel and tblRadiostation.strName like '{0}' ",
            strChannel);
        string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                     iStartTime, iEndTime);
        where += String.Format(" or (tblRadioPrograms.iStartTime>='{0}' and tblRadioPrograms.iStartTime <= '{1}' ) ",
                               iStartTime, iEndTime);
        where += String.Format(" or (tblRadioPrograms.iStartTime<='{0}' and tblRadioPrograms.iEndTime >= '{1}') )",
                               iStartTime, iEndTime);
        strSQL += where;
        strSQL += strOrder;


        using (SqlCommand cmd = _connection.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          cmd.CommandText = strSQL;
          using (SqlDataReader reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              long iStart = (int) reader["tblRadioPrograms.iStartTime"];
              long iEnd = (int) reader["tblRadioPrograms.iEndTime"];
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
                prog.Channel = (string) reader["tblRadiostation.strName"];
                prog.Start = iStart;
                prog.End = iEnd;
                prog.Genre = (string) reader["tblRadiogenre.strGenre"];
                prog.Title = (string) reader["tblRadioPrograms.strTitle"];
                prog.Description = (string) reader["tblRadioPrograms.strDescription"];
                prog.Episode = (string) reader["tblRadioPrograms.strEpisodeName"];
                prog.Repeat = (string) reader["tblRadioPrograms.strRepeat"];
                prog.ID = (int) reader["tblRadioPrograms.idProgram"];
                prog.SeriesNum = (string) reader["tblRadioPrograms.strSeriesNum"];
                prog.EpisodeNum = (string) reader["tblRadioPrograms.strEpisodeNum"];
                prog.EpisodePart = (string) reader["tblRadioPrograms.strEpisodePart"];
                prog.Date = (string) reader["tblRadioPrograms.strDate"];
                prog.StarRating = (string) reader["tblRadioPrograms.strStarRating"];
                prog.Classification = (string) reader["tblRadioPrograms.strClassification"];
                progs.Add(prog);
              }
            }
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return false;
    }
  }
}