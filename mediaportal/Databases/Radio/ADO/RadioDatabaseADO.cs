/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.Radio.Database
{
  public class RadioDatabaseADO : IRadioDatabase
  {
    SqlConnection _connection;
    public RadioDatabaseADO()
    {
      string connectionString;
      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings("mediaportal.xml"))
      {
        connectionString = reader.GetValueAsString("database", "connectionstring", SqlServerUtility.DefaultConnectionString);
      }
      _connection = new SqlConnection(connectionString);
      _connection.Open();
      CreateTables();
      UpdateFromPreviousVersion();
    }

    void CreateTables()
    {
      SqlServerUtility.AddTable(_connection, "tblRadioStation", "CREATE TABLE tblRadioStation ( idChannel int IDENTITY(1,1) NOT NULL, strName varchar(2048), iChannelNr int, frequency bigint, URL varchar(2048), genre varchar(2048), bitrate int, scrambled int)");
      SqlServerUtility.AddPrimaryKey(_connection, "tblRadioStation", "idChannel");
      SqlServerUtility.AddIndex(_connection, "idxStation", "CREATE INDEX idxStation ON tblRadioStation(idChannel)");

      SqlServerUtility.AddTable(_connection, "tblRadioDVBSMapping", "CREATE TABLE tblRadioDVBSMapping ( idChannel int,sPCRPid int,sTSID int,sFreq int,sSymbrate int,sFEC int,sLNBKhz int,sDiseqc int,sProgramNumber int,sServiceType int,sProviderName varchar(2048),sChannelName varchar(2048),sEitSched int,sEitPreFol int,sAudioPid int,sVideoPid int,sAC3Pid int,sAudio1Pid int,sAudio2Pid int,sAudio3Pid int,sTeletextPid int,sScrambled int,sPol int,sLNBFreq int,sNetworkID int,sAudioLang varchar(2048),sAudioLang1 varchar(2048),sAudioLang2 varchar(2048),sAudioLang3 varchar(2048),sECMPid int,sPMTPid int)");
      SqlServerUtility.AddTable(_connection, "tblRadioDVBCMapping", "CREATE TABLE tblRadioDVBCMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), symbolrate int, innerFec int, modulation int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int)");
      SqlServerUtility.AddTable(_connection, "tblRadioATSCMapping", "CREATE TABLE tblRadioATSCMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), symbolrate int, innerFec int, modulation int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int, channelNumber int, minorChannel int, majorChannel int)");
      SqlServerUtility.AddTable(_connection, "tblRadioDVBTMapping", "CREATE TABLE tblRadioDVBTMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), bandwidth int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int)");
      SqlServerUtility.AddTable(_connection, "tblRadioVersion", "CREATE TABLE tblRadioVersion( idVersion int)");

      //following table specifies which channels can be received by which card
      SqlServerUtility.AddTable(_connection, "tblRadioChannelCard", "CREATE TABLE tblRadioChannelCard( idChannelCard int primary key, idChannel int, card int)");
    }

    void UpdateFromPreviousVersion()
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
            versionNr = (int)reader[0];
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
        SqlServerUtility.InsertRecord(_connection, String.Format("insert into tblRadioVersion (idVersion) values({0})", currentVersion));
      }
      if (versionNr == 0)
      {
        //add pcr pid column to mapping tables
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioDVBCMapping ADD pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioATSCMapping ADD pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblRadioDVBTMapping ADD pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update tblRadioVersion set idVersion={0}", currentVersion));
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
        SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update tblRadioStation set epglastupdate='{0}'", dtStart));

        SqlServerUtility.AddTable(_connection, "tblRadioPrograms", "CREATE TABLE tblRadioPrograms ( idProgram int IDENTITY(1,1) NOT NULL, idChannel int, idGenre int, strTitle varchar(2048), iStartTime int, iEndTime int, strDescription varchar(2048),strEpisodeName varchar(2048),strRepeat varchar(2048),strSeriesNum varchar(2048),strEpisodeNum varchar(2048),strEpisodePart varchar(2048),strDate datetime,strStarRating varchar(2048),strClassification varchar(2048))");
        SqlServerUtility.AddPrimaryKey(_connection, "tblRadioPrograms", "idProgram");


        SqlServerUtility.AddTable(_connection, "tblRadioGenre", "CREATE TABLE tblRadioGenre ( idGenre int IDENTITY(1,1) NOT NULL, strGenre varchar(2048))");
        SqlServerUtility.AddPrimaryKey(_connection, "tblRadioGenre", "idGenre");
        SqlServerUtility.AddIndex(_connection, "idxStation", "CREATE INDEX idxGenre ON tblRadioGenre(strGenre)");
      }

      SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update tblRadioVersion set idVersion={0}", currentVersion));
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
                chan.ID = (int)reader["idChannel"];
              }
              catch (Exception) { }
              try
              {
                chan.Channel = (int)reader["iChannelNr"];
              }
              catch (Exception) { }
              try
              {
                chan.Frequency = (long)reader["frequency"];
              }
              catch (Exception)
              { }

              int scrambled = (int)reader["scrambled"];
              if (scrambled != 0)
                chan.Scrambled = true;
              else
                chan.Scrambled = false;

              chan.Name = (string)reader["strName"];
              chan.URL = (string)reader["URL"];
              if (chan.URL.Equals(Strings.Unknown)) chan.URL = "";
              try
              {
                chan.BitRate = (int)reader["bitrate"];
              }
              catch (Exception) { }
              chan.Sort = (int)reader["isort"];

              chan.Genre = (string)reader["genre"];
              chan.EpgHours = (int)reader["epgHours"];
              try
              {
                chan.LastDateTimeEpgGrabbed = (DateTime)reader["epgLastUpdate"];
              }
              catch (Exception) { }
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
                station.ID = (int)reader["idChannel"];
              }
              catch (Exception) { }
              try
              {
                station.Channel = (int)reader["iChannelNr"];
              }
              catch (Exception) { }
              try
              {
                station.Frequency = (long)reader["frequency"];
              }
              catch (Exception)
              { }

              int scrambled = (int)reader["scrambled"];
              if (scrambled != 0)
                chan.Scrambled = true;
              else
                chan.Scrambled = false;
              station.Name = (string)reader["strName"];
              station.URL = (string)reader["URL"];
              if (station.URL.Equals(Strings.Unknown)) chan.URL = "";
              try
              {
                station.BitRate = (int)reader["bitrate"];
              }
              catch (Exception) { }

              station.Channel = (int)reader["isort"];
              station.Genre = (string)reader["genre"];
              station.EpgHours = (int)reader["epgHours"];
              try
              {
                station.LastDateTimeEpgGrabbed = (DateTime)reader["epgLastUpdate"];
              }
              catch (Exception)
              { }
            }
          }
        }

      }
      catch (Exception ex)
      {
        Log.Write(ex);
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
        if (channel.Scrambled) scrambled = 1;
        strSQL = String.Format("update tblRadioStation set strName='{0}',iChannelNr={1} ,frequency={2},URL='{3}',bitrate={4},genre='{5}',scrambled={6},isort={7},epgLastUpdate='{8}',epgHours={9} where idChannel={10}",
                              strChannel, channel.Channel, channel.Frequency.ToString(), strURL,
                              channel.BitRate, strGenre, scrambled, channel.Sort,
                              channel.LastDateTimeEpgGrabbed,
                              channel.EpgHours, channel.ID);
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
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
              if (channel.Scrambled) scrambled = 1;
              strSQL = String.Format("insert into tblRadioStation (idChannel, strName,iChannelNr ,frequency,URL,bitrate,genre,scrambled,isort,epgLastUpdate,epgHours) values ( NULL, '{0}', {1}, {2}, '{3}',{4},'{5}',{6},{7},'{8}',{9} )",
                                    strChannel, channel.Channel, channel.Frequency.ToString(),
                                    strURL, channel.BitRate, strGenre, scrambled, channel.Sort,
                                    channel.LastDateTimeEpgGrabbed, channel.EpgHours);
              int iNewID = SqlServerUtility.InsertRecord(_connection, strSQL);
              channel.ID = iNewID;
              return iNewID;
            }
            else
            {
              int iNewID = (int)reader["idChannel"];
              channel.ID = iNewID;
              return iNewID;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
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
            int iNewID = (int)reader["idChannel"];
            return iNewID;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }

      return -1;
    }

    public void RemoveStation(string strStationName)
    {
      int iChannelId = GetStationId(strStationName);
      if (iChannelId < 0) return;

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
        Log.Write(ex);
      }
    }

    public void RemoveAllStations()
    {
      try
      {
        string strSQL = String.Format("delete from tvlRadioStation ");
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
        strSQL = String.Format("delete from tvlRadioGenre");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
        strSQL = String.Format("delete from tblRadioPrograms");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public void RemoveLocalRadioStations()
    {
      try
      {
        string strSQL = String.Format("delete from tvlRadioStation where frequency>0");
        SqlServerUtility.ExecuteNonQuery(_connection, strSQL);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    public int MapDVBSChannel(int idChannel, int freq, int symrate, int fec, int lnbkhz, int diseqc,
      int prognum, int servicetype, string provider, string channel, int eitsched,
      int eitprefol, int audpid, int vidpid, int ac3pid, int apid1, int apid2, int apid3,
      int teltxtpid, int scrambled, int pol, int lnbfreq, int networkid, int tsid, int pcrpid, string aLangCode, string aLangCode1, string aLangCode2, string aLangCode3, int ecmPid, int pmtPid)
    {
      return -1;
    }

    public int MapDVBTChannel(string channelName, string providerName, int idChannel, int frequency, int ONID, int TSID, int SID, int audioPid, int pmtPid, int bandWidth, int pcrPid)
    {
      return -1;
    }

    public int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid)
    {
      return -1;
    }

    public int MapATSCChannel(string channelName, int physicalChannel, int minorChannel, int majorChannel, string providerName, int idChannel, int frequency, int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid)
    {
      return -1;
    }

    public void GetDVBTTuneRequest(int idChannel, out string strProvider, out int frequency, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int bandWidth, out int pcrPid)
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
    }

    public void GetDVBCTuneRequest(int idChannel, out string strProvider, out int frequency, out int symbolrate, out int innerFec, out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int pcrPid)
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
    }

    public void GetATSCTuneRequest(int idChannel, out int physicalChannel, out int minorChannel, out int majorChannel, out string strProvider, out int frequency, out int symbolrate, out int innerFec, out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int pcrPid)
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
    }

    public bool GetDVBSTuneRequest(int idChannel, int serviceType, ref DVBChannel retChannel)
    {
      retChannel = null;
      return false;
    }

    public void MapChannelToCard(int channelId, int card)
    {
    }

    public void DeleteCard(int card)
    {
    }

    public void UnmapChannelFromCard(RadioStation channel, int card)
    {
    }

    public bool CanCardTuneToStation(string channelName, int card)
    {
      return false;
    }

    public void GetStationsForCard(ref ArrayList stations, int card)
    {
    }

    public RadioStation GetStationByStream(bool atsc, bool dvbt, bool dvbc, bool dvbs, int networkid, int transportid, int serviceid, out string provider)
    {
      provider = String.Empty;
      return null;
    }

    public void UpdatePids(bool isATSC, bool isDVBC, bool isDVBS, bool isDVBT, DVBChannel channel)
    {
    }

    public int AddProgram(TVProgram prog)
    {
      return -1;
    }

    public int AddGenre(string strGenre1)
    {
      return -1;
    }

    public int UpdateProgram(TVProgram prog)
    {
      return -1;
    }

    public void RemoveOldPrograms()
    {
    }

    public bool GetGenres(ref List<string> genres)
    {
      return false;
    }

    public bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime, ref List<TVProgram> progs)
    {
      return false;
    }
  }
}
