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
  public class RadioDatabaseADO
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
      SqlServerUtility.AddTable(_connection, "station", "CREATE TABLE station ( idChannel int IDENTITY(1,1) NOT NULL, strName varchar(2048), iChannelNr int, frequency varchar(2048), URL varchar(2048), genre varchar(2048), bitrate int, scrambled int)");
      SqlServerUtility.AddPrimaryKey(_connection, "station", "idChannel");
      SqlServerUtility.AddConstraint(_connection, "idxStation", "CREATE INDEX idxStation ON station(idChannel)");

      SqlServerUtility.AddTable(_connection, "tblDVBSMapping", "CREATE TABLE tblDVBSMapping ( idChannel int,sPCRPid int,sTSID int,sFreq int,sSymbrate int,sFEC int,sLNBKhz int,sDiseqc int,sProgramNumber int,sServiceType int,sProviderName varchar(2048),sChannelName varchar(2048),sEitSched int,sEitPreFol int,sAudioPid int,sVideoPid int,sAC3Pid int,sAudio1Pid int,sAudio2Pid int,sAudio3Pid int,sTeletextPid int,sScrambled int,sPol int,sLNBFreq int,sNetworkID int,sAudioLang varchar(2048),sAudioLang1 varchar(2048),sAudioLang2 varchar(2048),sAudioLang3 varchar(2048),sECMPid int,sPMTPid int)");
      SqlServerUtility.AddTable(_connection, "tblDVBCMapping", "CREATE TABLE tblDVBCMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), symbolrate int, innerFec int, modulation int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int)");
      SqlServerUtility.AddTable(_connection, "tblATSCMapping", "CREATE TABLE tblATSCMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), symbolrate int, innerFec int, modulation int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int, channelNumber int, minorChannel int, majorChannel int)");
      SqlServerUtility.AddTable(_connection, "tblDVBTMapping", "CREATE TABLE tblDVBTMapping ( idChannel int, strChannel varchar(2048), strProvider varchar(2048), frequency varchar(2048), bandwidth int, ONID int, TSID int, SID int, Visible int, audioPid int, pmtPid int)");
      SqlServerUtility.AddTable(_connection, "tblversion", "CREATE TABLE tblversion( idVersion int)");

      //following table specifies which channels can be received by which card
      SqlServerUtility.AddTable(_connection, "tblChannelCard", "CREATE TABLE tblChannelCard( idChannelCard int primary key, idChannel int, card int)");
    }

    void UpdateFromPreviousVersion()
    {
      int currentVersion = 3;
      int versionNr = 0;
      string strSQL = "SELECT * FROM tblversion";
      using (SqlCommand cmd = _connection.CreateCommand())
      {
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = strSQL;
        SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
          versionNr = (int)reader[0];
          reader.Close();
        }
        else
        {
          SqlServerUtility.InsertRecord(_connection, String.Format("insert into tblversion (idVersion) values({0})", currentVersion));
        }
      }

      if (versionNr == 0)
      {
        //add pcr pid column to mapping tables
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblDVBCMapping ADD COLUMN pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblATSCMapping ADD COLUMN pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE tblDVBTMapping ADD COLUMN pcrPid int");
        SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update tblversion set idVersion={0}", currentVersion));
      }
      if (versionNr < 2)
      {
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE station ADD COLUMN isort int");
        SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update station set isort=0"));
      }


      if (versionNr < 3)
      {
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE station ADD COLUMN epgHours int");
        SqlServerUtility.ExecuteNonQuery(_connection, "update station set epgHours=1");

        DateTime dtStart = new DateTime(1971, 11, 6);
        SqlServerUtility.ExecuteNonQuery(_connection, "ALTER TABLE station ADD COLUMN epgLastUpdate varchar(2048)");
        SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update station set epglastupdate='{0}'", Utils.datetolong(dtStart)));

        SqlServerUtility.AddTable(_connection, "tblPrograms", "CREATE TABLE tblPrograms ( idProgram int IDENTITY(1,1) NOT NULL, idChannel int, idGenre int, strTitle varchar(2048), iStartTime int, iEndTime int, strDescription varchar(2048),strEpisodeName varchar(2048),strRepeat varchar(2048),strSeriesNum varchar(2048),strEpisodeNum varchar(2048),strEpisodePart varchar(2048),strDate varchar(2048),strStarRating varchar(2048),strClassification varchar(2048))");
        SqlServerUtility.AddPrimaryKey(_connection, "tblPrograms", "idProgram");


        SqlServerUtility.AddTable(_connection, "genre", "CREATE TABLE genre ( idGenre int IDENTITY(1,1) NOT NULL, strGenre varchar(2048))");
        SqlServerUtility.AddPrimaryKey(_connection, "genre", "idGenre");
        SqlServerUtility.AddConstraint(_connection, "idxStation", "CREATE INDEX idxGenre ON genre(strGenre)");
      }

      SqlServerUtility.ExecuteNonQuery(_connection, String.Format("update tblversion set idVersion={0}", currentVersion));
    }

    public void ClearAll()
    {
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from station");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblDVBSMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblDVBCMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblATSCMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblDVBTMapping");
      SqlServerUtility.ExecuteNonQuery(_connection, "delete from tblChannelCard");
    }

  }
}
