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
using DirectShowLib;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using SQLite.NET;

namespace MediaPortal.TV.Database
{
  /// <summary>
  /// Singleton class which implements the TVdatabase
  /// The TVDatabase stores and retrieves all information about TV channels, TV shows, scheduled recordings
  /// and Recorded shows
  /// </summary>
  public class TVDatabase
  {
    #region private classes

    private class CachedGenre
    {
      public int idGenre = 0;
      public string strGenre = "";

      public CachedGenre()
      {
      }

      public CachedGenre(int id, string genre)
      {
        idGenre = id;
        strGenre = genre;
      }
    } ;

    private class CachedChannel
    {
      public int idChannel = 0;
      public int iChannelNr = 0;
      public string strChannel = "";

      public CachedChannel()
      {
      }

      public CachedChannel(int id, int number, string name)
      {
        idChannel = id;
        iChannelNr = number;
        strChannel = name;
      }
    } ;

    #endregion

    #region Enums

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

    #endregion

    #region Variables

    public static SQLiteClient m_db = null;
    private static ArrayList m_genreCache = new ArrayList();
    private static ArrayList m_channelCache = new ArrayList();
    private static bool m_bSupressEvents = false;
    private static bool m_bProgramsChanged = false;
    private static bool m_bRecordingsChanged = false;

    public delegate void OnChangedHandler();

    public delegate void OnRecordingChangedHandler(RecordingChange change);

    public static event OnChangedHandler OnProgramsChanged = null;
    public static event OnRecordingChangedHandler OnRecordingsChanged = null;
    public static event OnChangedHandler OnNotifiesChanged = null;
    public static event OnChangedHandler OnChannelsChanged = null;

    #endregion

    #region Constructors/Destructors

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

    #endregion

    #region Public Methods

    public static void UpdateFromPreviousVersion()
    {
      int currentVersion = 9;
      int versionNr = 0;

      DatabaseUtility.AddTable(m_db, "tblversion", "CREATE TABLE tblversion( idVersion integer)");

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
      if (versionNr < 2)
      {
        //version 1->2 : changed start/ending times from text->int in version 2
        try
        {
          m_db.Execute("drop table tblPrograms");
        }
        catch (Exception)
        {
        }
      }
      CreateTables();

      if (versionNr == 0)
      {
        m_db.Execute("update channel set iChannelNr=" + ((int) ExternalInputs.rgb).ToString() +
                     " where strChannel like 'RGB'");
        m_db.Execute("update channel set iChannelNr=" + ((int) ExternalInputs.svhs).ToString() +
                     " where strChannel like 'SVHS'");
        m_db.Execute("update channel set iChannelNr=" + ((int) ExternalInputs.cvbs1).ToString() +
                     " where strChannel like 'Composite #1'");
        m_db.Execute("update channel set iChannelNr=" + ((int) ExternalInputs.cvbs2).ToString() +
                     " where strChannel like 'Composite #2'");

        //version 0->1: add pcr pid column to mapping tables
        m_db.Execute("ALTER TABLE tblDVBCMapping ADD COLUMN pcrPid Integer");
        m_db.Execute("ALTER TABLE tblATSCMapping ADD COLUMN pcrPid Integer");
        m_db.Execute("ALTER TABLE tblDVBTMapping ADD COLUMN pcrPid Integer");
      }
      if (versionNr < 3)
      {
        //version 2->3 : added iSort to tblGroupMapping
        m_db.Execute("ALTER TABLE tblGroupMapping ADD COLUMN iSort Integer");
      }
      if (versionNr < 4)
      {
        //version 3->4 : added keepMethod and keepDate to recorded
        //version 3->4 : added keepMethod and keepDate to recording
        m_db.Execute("ALTER TABLE recorded ADD COLUMN keepMethod Integer");
        m_db.Execute("ALTER TABLE recorded ADD COLUMN keepDate text");
        m_db.Execute("ALTER TABLE recording ADD COLUMN keepMethod Integer");
        m_db.Execute("ALTER TABLE recording ADD COLUMN keepDate text");
        DateTime maxDate = DateTime.MaxValue;
        m_db.Execute(String.Format("update recorded set keepMethod={0}, keepDate='{1}'",
                                   (int) TVRecorded.KeepMethod.Always, Util.Utils.datetolong(maxDate)));
        m_db.Execute(String.Format("update recording set keepMethod={0}, keepDate='{1}'",
                                   (int) TVRecorded.KeepMethod.Always, Util.Utils.datetolong(maxDate)));
      }

      if (versionNr < 5)
      {
        //version 4->5 : added paddingFront and paddingEnd to recording
        m_db.Execute("ALTER TABLE recording ADD COLUMN paddingFront Integer");
        m_db.Execute("ALTER TABLE recording ADD COLUMN paddingEnd Integer");
        DateTime maxDate = DateTime.MaxValue;
        m_db.Execute(String.Format("update recording set paddingFront=-1"));
        m_db.Execute(String.Format("update recording set paddingEnd=-1"));
      }
      if (versionNr < 6)
      {
        m_db.Execute("ALTER TABLE channel ADD COLUMN grabEpg Integer");
        m_db.Execute("update channel set grabEpg=1");
      }
      if (versionNr < 7)
      {
        m_db.Execute("ALTER TABLE channel ADD COLUMN epgHours Integer");
        m_db.Execute("update channel set epgHours=1");
      }
      if (versionNr < 8)
      {
        DateTime dtStart = new DateTime(1971, 11, 6);
        m_db.Execute("ALTER TABLE channel ADD COLUMN epgLastUpdate text");
        m_db.Execute(String.Format("update channel set epglastupdate='{0}'", Util.Utils.datetolong(dtStart)));
      }
      if (versionNr < 9)
      {
        m_db.Execute("ALTER TABLE recorded ADD COLUMN idCard integer");
        m_db.Execute("UPDATE recorded set idCard=1");
      }
      m_db.Execute(String.Format("update tblversion set idVersion={0}", currentVersion));
    }

    /// <summary>
    /// clears the tv channel & genre cache
    /// </summary>
    public static void ClearCache()
    {
      m_channelCache.Clear();
      m_genreCache.Clear();
    }

    public static void ClearAll()
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

    //------------------------------------------------------------------------------------------------
    //b2c2

    public static int AddSatChannel(int idChannel, int freq, int symrate, int fec, int lnbkhz, int diseqc,
                                    int prognum, int servicetype, string provider, string channel, int eitsched,
                                    int eitprefol, int audpid, int vidpid, int ac3pid, int apid1, int apid2, int apid3,
                                    int teltxtpid, int scrambled, int pol, int lnbfreq, int networkid, int tsid,
                                    int pcrpid, string aLangCode, string aLangCode1, string aLangCode2,
                                    string aLangCode3, int ecmPid, int pmtPid)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL = string.Empty;
        try
        {
          DatabaseUtility.RemoveInvalidChars(ref provider);
          DatabaseUtility.RemoveInvalidChars(ref channel);

          SQLiteResultSet results = null;

          strSQL = String.Format("select * from tblDVBSMapping ");
          results = m_db.Execute(strSQL);
          int totalchannels = results.Rows.Count;

          strSQL = String.Format("select * from tblDVBSMapping where idChannel = {0} and sServiceType={1}", idChannel,
                                 servicetype);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            //

            // fields in tblDVBSMapping
            // idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid
            // parameter (8)(9)
            //idChannel,freq,symrate, fec,lnbkhz,diseqc,
            //prognum,servicetype,provider,channel, eitsched,
            //eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
            // teltxtpid,scrambled, pol,lnbfreq,networkid

            strSQL =
              String.Format(
                "insert into tblDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})",
                idChannel, freq, symrate, fec, lnbkhz, diseqc,
                prognum, servicetype, provider.Trim(), channel.Trim(), eitsched,
                eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
                teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid,
                aLangCode.Trim(), aLangCode1.Trim(), aLangCode2.Trim(), aLangCode3.Trim(),
                ecmPid, pmtPid);

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
          Log.Error("TVDatabase exception SQL:{0} err:{1} stack:{2}", strSQL, ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static int AddSatChannel(DVBChannel ch)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL = string.Empty;
        try
        {
          SQLiteResultSet results = null;

          strSQL = String.Format("select * from tblDVBSMapping ");
          results = m_db.Execute(strSQL);
          int totalchannels = results.Rows.Count;

          strSQL = String.Format("select * from tblDVBSMapping where idChannel = {0} and sServiceType={1}", ch.ID,
                                 ch.ServiceType);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            //

            // fields in tblDVBSMapping
            // idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid
            // parameter (8)(9)
            //idChannel,freq,symrate, fec,lnbkhz,diseqc,
            //prognum,servicetype,provider,channel, eitsched,
            //eitprefol, audpid,vidpid,ac3pid,apid1, apid2, apid3,
            // teltxtpid,scrambled, pol,lnbfreq,networkid

            string provider = ch.ServiceProvider;
            string service = ch.ServiceName;

            if (provider == null)
            {
              provider = string.Empty;
            }
            if (service == null)
            {
              service = string.Empty;
            }
            DatabaseUtility.RemoveInvalidChars(ref provider);
            DatabaseUtility.RemoveInvalidChars(ref service);
            string al = ch.AudioLanguage;
            string al1 = ch.AudioLanguage1;
            string al2 = ch.AudioLanguage2;
            string al3 = ch.AudioLanguage3;
            al = (al == null ? "" : al.Trim());
            al1 = (al1 == null ? "" : al1.Trim());
            al2 = (al2 == null ? "" : al2.Trim());
            al3 = (al3 == null ? "" : al3.Trim());
            provider = (provider == null ? "" : provider.Trim());
            service = (service == null ? "" : service.Trim());
            DatabaseUtility.RemoveInvalidChars(ref al);
            DatabaseUtility.RemoveInvalidChars(ref al1);
            DatabaseUtility.RemoveInvalidChars(ref al2);
            DatabaseUtility.RemoveInvalidChars(ref al3);

            strSQL =
              String.Format(
                "insert into tblDVBSMapping (idChannel,sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName,sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID,sTSID,sPCRPid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid) values ( {0}, {1}, {2}, {3}, {4}, {5},{6}, {7}, '{8}' ,'{9}', {10}, {11}, {12}, {13}, {14},{15}, {16}, {17},{18}, {19}, {20},{21}, {22},{23},{24},'{25}','{26}','{27}','{28}',{29},{30})",
                ch.ID, ch.Frequency, ch.Symbolrate, ch.FEC, ch.LnbSwitchFrequency, ch.DiSEqC,
                ch.ProgramNumber, ch.ServiceType, provider, service, (ch.HasEITSchedule == true ? 1 : 0),
                (ch.HasEITPresentFollow == true ? 1 : 0), ch.AudioPid, ch.VideoPid, ch.AC3Pid, ch.Audio1, ch.Audio2,
                ch.Audio3,
                ch.TeletextPid, (ch.IsScrambled == true ? 1 : 0), ch.Polarity, ch.LNBFrequency, ch.NetworkID,
                ch.TransportStreamID, ch.PCRPid, al, al1, al2, al3, ch.ECMPid, ch.PMTPid);

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
          Log.Error("TVDatabase exception {0} err:{1} stack:{2}", strSQL, ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static string GetSatChannelName(int program_number, int id)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        string channelName = "";
        try
        {
          if (null == m_db)
          {
            return "";
          }

          SQLiteResultSet results;
          strSQL = String.Format("select * from tblDVBSMapping where sProgramNumber={0}", program_number);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return "";
          }
          channelName = DatabaseUtility.Get(results, 0, "sChannelName");
          return channelName;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return "";
      }
    }

    public static bool GetSatChannel(int idChannel, int serviceType, ref DVBChannel retChannel)
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
      lock (typeof (TVDatabase))
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
            //chan.ID=DatabaseUtility.GetAsInt(results,i,"idChannel");
            //chan.Number = DatabaseUtility.GetAsInt(results,i,"iChannelNr");
            // sFreq,sSymbrate,sFEC,sLNBKhz,sDiseqc,sProgramNumber,sServiceType,sProviderName,sChannelName
            //sEitSched,sEitPreFol,sAudioPid,sVideoPid,sAC3Pid,sAudio1Pid,sAudio2Pid,sAudio3Pid,
            //sTeletextPid,sScrambled,sPol,sLNBFreq,sNetworkID
            int i = 0;
            freq = DatabaseUtility.GetAsInt(results, i, "sFreq");
            symrate = DatabaseUtility.GetAsInt(results, i, "sSymbrate");
            fec = DatabaseUtility.GetAsInt(results, i, "sFEC");
            lnbkhz = DatabaseUtility.GetAsInt(results, i, "sLNBKhz");
            diseqc = DatabaseUtility.GetAsInt(results, i, "sDiseqc");
            prognum = DatabaseUtility.GetAsInt(results, i, "sProgramNumber");
            servicetype = DatabaseUtility.GetAsInt(results, i, "sServiceType");
            provider = DatabaseUtility.Get(results, i, "sProviderName");
            channel = DatabaseUtility.Get(results, i, "sChannelName");
            eitsched = DatabaseUtility.GetAsInt(results, i, "sEitSched");
            eitprefol = DatabaseUtility.GetAsInt(results, i, "sEitPreFol");
            audpid = DatabaseUtility.GetAsInt(results, i, "sAudioPid");
            vidpid = DatabaseUtility.GetAsInt(results, i, "sVideoPid");
            ac3pid = DatabaseUtility.GetAsInt(results, i, "sAC3Pid");
            apid1 = DatabaseUtility.GetAsInt(results, i, "sAudio1Pid");
            apid2 = DatabaseUtility.GetAsInt(results, i, "sAudio2Pid");
            apid3 = DatabaseUtility.GetAsInt(results, i, "sAudio3Pid");
            teltxtpid = DatabaseUtility.GetAsInt(results, i, "sTeletextPid");
            scrambled = DatabaseUtility.GetAsInt(results, i, "sScrambled");
            pol = DatabaseUtility.GetAsInt(results, i, "sPol");
            lnbfreq = DatabaseUtility.GetAsInt(results, i, "sLNBFreq");
            networkid = DatabaseUtility.GetAsInt(results, i, "sNetworkID");
            tsid = DatabaseUtility.GetAsInt(results, i, "sTSID");
            pcrpid = DatabaseUtility.GetAsInt(results, i, "sPCRPid");
            // sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,sECMPid,sPMTPid
            audioLang = DatabaseUtility.Get(results, i, "sAudioLang");
            audioLang1 = DatabaseUtility.Get(results, i, "sAudioLang1");
            audioLang2 = DatabaseUtility.Get(results, i, "sAudioLang2");
            audioLang3 = DatabaseUtility.Get(results, i, "sAudioLang3");
            ecm = DatabaseUtility.GetAsInt(results, i, "sECMPid");
            pmt = DatabaseUtility.GetAsInt(results, i, "sPMTPid");
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static void UpdateSatChannel(int channelId, DVBChannel ch)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          string strChannel = ch.ServiceName;
          string strProvider = ch.ServiceProvider;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strProvider);

          if (null == m_db)
          {
            return;
          }

          string strSQL =
            String.Format(
              "update tblDVBSMapping set sFreq={0},sSymbrate={1},sFEC={2},sLNBKhz={3},sDiseqc={4},sProgramNumber={5},sServiceType={6},sProviderName='{7}',sChannelName='{8}',sEitSched={9},sEitPreFol={10},sAudioPid={11},sVideoPid={12},sAC3Pid={13},sAudio1Pid={14},sAudio2Pid={15},sAudio3Pid={16},sTeletextPid={17},sScrambled={18},sPol={19},sLNBFreq={20},sNetworkID={21},sTSID={22},sPCRPid={23} where idChannel = {24}",
              ch.Frequency, ch.Symbolrate, ch.FEC, ch.LnbSwitchFrequency, ch.DiSEqC,
              ch.ProgramNumber, ch.ServiceType, strProvider, strChannel, (int) (ch.HasEITSchedule == true ? 1 : 0),
              (int) (ch.HasEITPresentFollow == true ? 1 : 0), ch.AudioPid, ch.VideoPid, ch.AC3Pid, ch.Audio1, ch.Audio2,
              ch.Audio3,
              ch.TeletextPid, (int) (ch.IsScrambled == true ? 1 : 0), ch.Polarity, ch.LNBFrequency, ch.NetworkID,
              ch.TransportStreamID, ch.PCRPid, ch.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static bool GetSatChannels(ref ArrayList channels)
    {
      if (m_db == null)
      {
        return false;
      }
      int idChannel = 0;
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
      string audioLang = "";
      string audioLang1 = "";
      string audioLang2 = "";
      string audioLang3 = "";
      int ecm = 0;
      int pmt = 0;
      lock (typeof (TVDatabase))
      {
        channels.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          DVBChannel dvbChannel = new DVBChannel();
          string strSQL;
          strSQL = String.Format("select * from tblDVBSMapping order by sChannelName");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            idChannel = DatabaseUtility.GetAsInt(results, i, "idChannel");
            freq = DatabaseUtility.GetAsInt(results, i, "sFreq");
            symrate = DatabaseUtility.GetAsInt(results, i, "sSymbrate");
            fec = DatabaseUtility.GetAsInt(results, i, "sFEC");
            lnbkhz = DatabaseUtility.GetAsInt(results, i, "sLNBKhz");
            diseqc = DatabaseUtility.GetAsInt(results, i, "sDiseqc");
            prognum = DatabaseUtility.GetAsInt(results, i, "sProgramNumber");
            servicetype = DatabaseUtility.GetAsInt(results, i, "sServiceType");
            provider = DatabaseUtility.Get(results, i, "sProviderName");
            channel = DatabaseUtility.Get(results, i, "sChannelName");
            eitsched = DatabaseUtility.GetAsInt(results, i, "sEitSched");
            eitprefol = DatabaseUtility.GetAsInt(results, i, "sEitPreFol");
            audpid = DatabaseUtility.GetAsInt(results, i, "sAudioPid");
            vidpid = DatabaseUtility.GetAsInt(results, i, "sVideoPid");
            ac3pid = DatabaseUtility.GetAsInt(results, i, "sAC3Pid");
            apid1 = DatabaseUtility.GetAsInt(results, i, "sAudio1Pid");
            apid2 = DatabaseUtility.GetAsInt(results, i, "sAudio2Pid");
            apid3 = DatabaseUtility.GetAsInt(results, i, "sAudio3Pid");
            teltxtpid = DatabaseUtility.GetAsInt(results, i, "sTeletextPid");
            scrambled = DatabaseUtility.GetAsInt(results, i, "sScrambled");
            pol = DatabaseUtility.GetAsInt(results, i, "sPol");
            lnbfreq = DatabaseUtility.GetAsInt(results, i, "sLNBFreq");
            networkid = DatabaseUtility.GetAsInt(results, i, "sNetworkID");
            tsid = DatabaseUtility.GetAsInt(results, i, "sTSID");
            pcrpid = DatabaseUtility.GetAsInt(results, i, "sPCRPid");
            audioLang = DatabaseUtility.Get(results, i, "sAudioLang");
            audioLang1 = DatabaseUtility.Get(results, i, "sAudioLang1");
            audioLang2 = DatabaseUtility.Get(results, i, "sAudioLang2");
            audioLang3 = DatabaseUtility.Get(results, i, "sAudioLang3");
            ecm = DatabaseUtility.GetAsInt(results, i, "sECMPid");
            pmt = DatabaseUtility.GetAsInt(results, i, "sPMTPid");
            dvbChannel = new DVBChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc,
                                        prognum, servicetype, provider, channel, eitsched,
                                        eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
                                        teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid, audioLang,
                                        audioLang1, audioLang2, audioLang3, ecm, pmt);
            channels.Add(dvbChannel);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static void RemoveSatChannel(DVBChannel ch)
    {
      lock (typeof (TVDatabase))
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
          string strSQL = String.Format("delete from tblDVBSMapping where idChannel={0}", ch.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void RemoveAllSatChannels()
    {
      lock (typeof (TVDatabase))
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
          string strSQL = String.Format("delete from tblDVBSMapping");
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    //--------------------------------------------------------------------------------------------------------
    public static int GetChannelId(string strChannel)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return -1;
          }

          foreach (CachedChannel cache in m_channelCache)
          {
            if (cache.strChannel == strChannel)
            {
              return cache.idChannel;
            }
          }

          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          SQLiteResultSet results;
          strSQL = String.Format("select * from channel where strChannel like '{0}'", strChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return -1;
          }
          int iNewID = DatabaseUtility.GetAsInt(results, 0, "idChannel");

          CachedChannel chan = new CachedChannel();
          chan.idChannel = iNewID;
          chan.strChannel = DatabaseUtility.Get(results, 0, "strChannel");
          chan.iChannelNr = DatabaseUtility.GetAsInt(results, 0, "iChannelNr");
          m_channelCache.Add(chan);
          return iNewID;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static int GetChannelId(int iChannelNr)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return -1;
          }

          foreach (CachedChannel cache in m_channelCache)
          {
            if (cache.iChannelNr == iChannelNr)
            {
              return cache.idChannel;
            }
          }
          SQLiteResultSet results;
          strSQL = String.Format("select * from channel where iChannelNr={0}", iChannelNr);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return -1;
          }
          int iNewID = DatabaseUtility.GetAsInt(results, 0, "idChannel");

          CachedChannel chan = new CachedChannel();
          chan.idChannel = iNewID;
          chan.strChannel = DatabaseUtility.Get(results, 0, "strChannel");
          chan.iChannelNr = iChannelNr;
          m_channelCache.Add(chan);
          return iNewID;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static void SetChannelNumber(string strChannel, int iNumber)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          ClearCache();
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          SQLiteResultSet results;
          strSQL = String.Format("update channel set iChannelNr={0} where strChannel like '{1}'", iNumber, strChannel);
          results = m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void SetChannelFrequency(string strChannel, string strFreq)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          ClearCache();
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          SQLiteResultSet results;
          strSQL = String.Format("update channel set frequency='{0}' where strChannel like '{1}'", strFreq, strChannel);
          results = m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void SetChannelSort(string strChannel, int iPlace)
    {
      if (m_db == null)
      {
        return;
      }
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          ClearCache();
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          SQLiteResultSet results;
          strSQL = String.Format("update channel set iSort={0} where strChannel like '{1}'", iPlace, strChannel);
          results = m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void UpdateChannel(TVChannel channel, int sort)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strChannel = channel.Name;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          if (null == m_db)
          {
            return;
          }
          m_channelCache.Clear();
          string strExternal = channel.ExternalTunerChannel;
          DatabaseUtility.RemoveInvalidChars(ref strExternal);

          int iExternal = 0;
          if (channel.External)
          {
            iExternal = 1;
          }
          int iVisible = 0;
          if (channel.VisibleInGuide)
          {
            iVisible = 1;
          }
          int scrambled = 0;
          if (channel.Scrambled)
          {
            scrambled = 1;
          }
          int grabEpg = 0;
          if (channel.AutoGrabEpg)
          {
            grabEpg = 1;
          }


          strSQL =
            String.Format(
              "update channel set iChannelNr={0}, frequency={1}, iSort={2},bExternal={3}, ExternalChannel='{4}',standard={5}, Visible={6}, Country={7},strChannel='{8}', scrambled={9},grabEpg={10},epgHours={11},epgLastUpdate='{12}' where idChannel like {13}",
              channel.Number, channel.Frequency.ToString(),
              sort, iExternal, strExternal, (int) channel.TVStandard, iVisible, channel.Country,
              strChannel, scrambled, grabEpg, channel.EpgHours,
              Util.Utils.datetolong(channel.LastDateTimeEpgGrabbed),
              channel.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static int AddChannel(TVChannel channel)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL = string.Empty;
        try
        {
          string strChannel = channel.Name;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);

          if (null == m_db)
          {
            return -1;
          }


          //foreach (CachedChannel cache in m_channelCache)
          //{
          //  if (cache.strChannel == strChannel) return cache.idChannel;
          //}
          SQLiteResultSet results;
          strSQL = String.Format("select * from channel ");
          results = m_db.Execute(strSQL);
          int totalchannels = results.Rows.Count;

          strSQL = String.Format("select * from channel where strChannel like '{0}'", strChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            string strExternal = channel.ExternalTunerChannel;
            DatabaseUtility.RemoveInvalidChars(ref strExternal);
            int iExternal = 0;
            if (channel.External)
            {
              iExternal = 1;
            }
            int iVisible = 0;
            if (channel.VisibleInGuide)
            {
              iVisible = 1;
            }
            int scrambled = 0;
            if (channel.Scrambled)
            {
              scrambled = 1;
            }
            int grabepg = 0;
            if (channel.AutoGrabEpg)
            {
              grabepg = 1;
            }

            if (channel.ID > 0)
            {
              strSQL =
                String.Format(
                  "insert into channel (idChannel, strChannel,iChannelNr ,frequency,iSort, bExternal, ExternalChannel,standard, Visible, Country, scrambled,grabEpg,epgHours,epgLastUpdate) values ( {13}, '{0}', {1}, {2}, {3}, {4},'{5}', {6}, {7}, {8}, {9},{10},{11},'{12}' )",
                  strChannel, channel.Number, channel.Frequency.ToString(),
                  totalchannels + 1, iExternal, strExternal, (int) channel.TVStandard, iVisible, channel.Country,
                  scrambled, grabepg, channel.EpgHours,
                  Util.Utils.datetolong(channel.LastDateTimeEpgGrabbed), channel.ID);
            }
            else
            {
              strSQL =
                String.Format(
                  "insert into channel (idChannel, strChannel,iChannelNr ,frequency,iSort, bExternal, ExternalChannel,standard, Visible, Country, scrambled,grabEpg,epgHours,epgLastUpdate) values ( NULL, '{0}', {1}, {2}, {3}, {4},'{5}', {6}, {7}, {8}, {9},{10},{11},'{12}' )",
                  strChannel, channel.Number, channel.Frequency.ToString(),
                  totalchannels + 1, iExternal, strExternal, (int) channel.TVStandard, iVisible, channel.Country,
                  scrambled, grabepg, channel.EpgHours,
                  Util.Utils.datetolong(channel.LastDateTimeEpgGrabbed));
            }
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();

            CachedChannel chan = new CachedChannel(iNewID, channel.Number, strChannel);
            m_channelCache.Add(chan);
            channel.ID = iNewID;
            channel.Sort = totalchannels + 1;
            if (OnChannelsChanged != null)
            {
              OnChannelsChanged();
            }
            return iNewID;
          }
          else
          {
            int iNewID = DatabaseUtility.GetAsInt(results, 0, "idChannel");

            CachedChannel chan = new CachedChannel(iNewID, channel.Number, strChannel);
            m_channelCache.Add(chan);
            channel.ID = iNewID;
            return iNewID;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception sql:{0} err:{1} stack:{2}", strSQL, ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static int AddGenre(string strGenre1)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          foreach (CachedGenre genre in m_genreCache)
          {
            if (genre.strGenre == strGenre1)
            {
              return genre.idGenre;
            }
          }
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

            CachedGenre genre = new CachedGenre(iNewId, strGenre1);
            m_genreCache.Add(genre);
            return iNewId;
          }
          else
          {
            int iID = DatabaseUtility.GetAsInt(results, 0, "idGenre");
            CachedGenre genre = new CachedGenre(iID, strGenre1);
            m_genreCache.Add(genre);
            return iID;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static int AddProgram(TVProgram prog)
    {
      int lRetId = -1;
      lock (typeof (TVDatabase))
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
          int iChannelId = GetChannelId(prog.Channel);
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      ProgramsChanged();
      return lRetId;
    }

    public static int UpdateProgram(TVProgram prog)
    {
      int lRetId = -1;
      lock (typeof (TVDatabase))
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
          int iChannelId = GetChannelId(prog.Channel);
          if (iChannelId < 0)
          {
            return -1;
          }

          //check if program is already in database
          //check if other programs exist between the start - finish time of this program
          //     s------------------e               = prog
          //-------e                                = db 
          //       s--------------e                 = db
          //                      s------------     = db 

          strSQL = String.Format("SELECT * FROM tblPrograms WHERE idChannel={0} AND ", iChannelId);
          strSQL += String.Format("  ( ('{0}' < iEndTime and '{1}' > iEndTime) or  ",
                                  prog.Start.ToString(), prog.End.ToString());
          strSQL += String.Format("    ('{0}' <= iStartTime and '{1}' >= iEndTime) or  ",
                                  prog.Start.ToString(), prog.End.ToString());
          strSQL += String.Format("    ('{0}' < iStartTime and '{1}' > iStartTime ) )",
                                  prog.Start.ToString(), prog.End.ToString());
          //  Log.Info("sql:{0} {1}-{2} {3}", prog.Channel, prog.Start.ToString(), endTime.ToString(), strSQL);
          SQLiteResultSet results2;
          results2 = m_db.Execute(strSQL);
          if (results2.Rows.Count > 0)
          {
            if (results2.Rows.Count == 1)
            {
              string title = DatabaseUtility.Get(results2, 0, "strTitle");
              long start = DatabaseUtility.GetAsInt64(results2, 0, "iStartTime");
              long end = DatabaseUtility.GetAsInt64(results2, 0, "iEndTime");

              if ((prog.Title.Equals(title)) && (prog.Start == start) && (prog.End == end)) // only when equal
              {
                long idProgram = DatabaseUtility.GetAsInt64(results2, 0, "idProgram");
                return (int) idProgram; //program already exists
              }
            }

            //and delete them
            for (int i = 0; i < results2.Rows.Count; ++i)
            {
              long idProgram = DatabaseUtility.GetAsInt64(results2, i, "idProgram");
              //Log.WriteFile(LogType.EPG, "sql: del {0} id:{1} {2}-{3}", i, idProgram, DatabaseUtility.Get(results2, i, "iStartTime"), DatabaseUtility.Get(results2, i, "iEndTime"));
              strSQL = String.Format("DELETE FROM tblPrograms WHERE idProgram={0}", idProgram);
              m_db.Execute(strSQL);
            }
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      ProgramsChanged();
      return lRetId;
    }

    public static bool GetChannelsByProvider(ref ArrayList channels)
    {
      if (m_db == null)
      {
        return false;
      }
      lock (typeof (TVDatabase))
      {
        channels.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = "select channel.idChannel,channel.iChannelNr,channel.frequency,channel.strChannel,";
          strSQL += "channel.bExternal,channel.Visible,channel.scrambled,channel.ExternalChannel,channel.standard,";
          strSQL +=
            "channel.Country,channel.iSort,tblDVBCMapping.strProvider,tblDVBSMapping.sProviderName,tblDVBTMapping.strProvider,tblATSCMapping.strProvider,channel.grabEpg,channel.epgHours,channel.epgLastUpdate ";
          strSQL += "from channel left join tblDVBCMapping on tblDVBCMapping.iLCN=channel.idChannel ";
          strSQL += "left join tblDVBTMapping on tblDVBTMapping.iLCN=channel.idChannel ";
          strSQL += "left join tblDVBSMapping on tblDVBSMapping.idChannel=channel.idChannel ";
          strSQL += "left join tblATSCMapping on tblATSCMapping.iLCN=channel.idChannel ";
          strSQL +=
            "order by tblDVBCMapping.strProvider, tblDVBTMapping.strProvider, tblDVBSMapping.sProviderName, tblATSCMapping.strProvider,channel.strChannel";
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVChannel chan = new TVChannel();
            chan.ID = DatabaseUtility.GetAsInt(results, i, 0);
            chan.Number = DatabaseUtility.GetAsInt(results, i, 1);
            decimal dFreq = 0;
            try
            {
              dFreq = (decimal) DatabaseUtility.GetAsInt64(results, i, "channel.frequency");
            }
            catch (Exception)
            {
              chan.Frequency = 0;
            }
            dFreq /= 1000000M;
            dFreq = Math.Round(dFreq, 3);
            dFreq *= 1000000M;
            chan.Frequency = (long) Math.Round(dFreq, 0);
            chan.Name = DatabaseUtility.Get(results, i, 3);
            int iExternal = DatabaseUtility.GetAsInt(results, i, 4);
            if (iExternal != 0)
            {
              chan.External = true;
            }
            else
            {
              chan.External = false;
            }

            int iVisible = DatabaseUtility.GetAsInt(results, i, 5);
            if (iVisible != 0)
            {
              chan.VisibleInGuide = true;
            }
            else
            {
              chan.VisibleInGuide = false;
            }

            int scrambled = DatabaseUtility.GetAsInt(results, i, 6);
            if (scrambled != 0)
            {
              chan.Scrambled = true;
            }
            else
            {
              chan.Scrambled = false;
            }

            int grabepg = DatabaseUtility.GetAsInt(results, i, 15);
            if (grabepg != 0)
            {
              chan.AutoGrabEpg = true;
            }
            else
            {
              chan.AutoGrabEpg = false;
            }

            chan.EpgHours = DatabaseUtility.GetAsInt(results, i, 16);
            chan.LastDateTimeEpgGrabbed = Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, i, 17));

            chan.ExternalTunerChannel = DatabaseUtility.Get(results, i, 7);
            chan.TVStandard = (AnalogVideoStandard) DatabaseUtility.GetAsInt(results, i, 8);
            chan.Country = DatabaseUtility.GetAsInt(results, i, 9);
            chan.Sort = DatabaseUtility.GetAsInt(results, i, 10);
            chan.ProviderName = DatabaseUtility.Get(results, i, 11);
            if (chan.ProviderName == "")
            {
              chan.ProviderName = DatabaseUtility.Get(results, i, 12);
              if (chan.ProviderName == "")
              {
                chan.ProviderName = DatabaseUtility.Get(results, i, 13);
                if (chan.ProviderName == "")
                {
                  chan.ProviderName = DatabaseUtility.Get(results, i, 14);
                  if (chan.ProviderName == "")
                  {
                    chan.ProviderName = Strings.Unknown;
                  }
                }
              }
            }
            channels.Add(chan);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool GetChannels(ref ArrayList channels)
    {
      if (m_db == null)
      {
        return false;
      }
      lock (typeof (TVDatabase))
      {
        channels.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          m_channelCache.Clear();
          string strSQL;
          strSQL = String.Format("select * from channel order by iSort");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVChannel chan = new TVChannel();
            chan.ID = DatabaseUtility.GetAsInt(results, i, "idChannel");
            chan.Number = DatabaseUtility.GetAsInt(results, i, "iChannelNr");
            decimal dFreq = 0;
            try
            {
              dFreq = (decimal) DatabaseUtility.GetAsInt64(results, i, "frequency");
            }
            catch (Exception)
            {
              chan.Frequency = 0;
            }
            dFreq /= 1000000M;
            dFreq = Math.Round(dFreq, 3);
            dFreq *= 1000000M;
            chan.Frequency = (long) Math.Round(dFreq, 0);
            chan.Name = DatabaseUtility.Get(results, i, "strChannel");
            int iExternal = DatabaseUtility.GetAsInt(results, i, "bExternal");
            if (iExternal != 0)
            {
              chan.External = true;
            }
            else
            {
              chan.External = false;
            }

            int iVisible = DatabaseUtility.GetAsInt(results, i, "Visible");
            if (iVisible != 0)
            {
              chan.VisibleInGuide = true;
            }
            else
            {
              chan.VisibleInGuide = false;
            }


            int scrambled = DatabaseUtility.GetAsInt(results, i, "scrambled");
            if (scrambled != 0)
            {
              chan.Scrambled = true;
            }
            else
            {
              chan.Scrambled = false;
            }

            int grabepg = DatabaseUtility.GetAsInt(results, i, "grabEpg");
            if (grabepg != 0)
            {
              chan.AutoGrabEpg = true;
            }
            else
            {
              chan.AutoGrabEpg = false;
            }

            chan.EpgHours = DatabaseUtility.GetAsInt(results, i, "epgHours");
            chan.LastDateTimeEpgGrabbed = Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, i, "epgLastUpdate"));

            chan.Sort = DatabaseUtility.GetAsInt(results, i, "iSort");

            chan.ExternalTunerChannel = DatabaseUtility.Get(results, i, "ExternalChannel");
            chan.TVStandard = (AnalogVideoStandard) DatabaseUtility.GetAsInt(results, i, "standard");
            chan.Country = DatabaseUtility.GetAsInt(results, i, "Country");

            channels.Add(chan);

            m_channelCache.Add(new CachedChannel(chan.ID, chan.Number, chan.Name));
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static TVChannel GetChannelById(int channelId)
    {
      if (m_db == null)
      {
        return null;
      }
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return null;
          }
          string strSQL;
          strSQL = String.Format("select * from channel where idChannel={0}", channelId);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return null;
          }

          TVChannel chan = new TVChannel();
          chan.ID = DatabaseUtility.GetAsInt(results, 0, "idChannel");
          chan.Number = DatabaseUtility.GetAsInt(results, 0, "iChannelNr");
          decimal dFreq = 0;
          try
          {
            dFreq = (decimal) DatabaseUtility.GetAsInt64(results, 0, "frequency");
          }
          catch (Exception)
          {
            chan.Frequency = 0;
          }
          dFreq /= 1000000M;
          dFreq = Math.Round(dFreq, 3);
          dFreq *= 1000000M;
          chan.Frequency = (long) Math.Round(dFreq, 0);
          chan.Name = DatabaseUtility.Get(results, 0, "strChannel");
          int iExternal = DatabaseUtility.GetAsInt(results, 0, "bExternal");
          if (iExternal != 0)
          {
            chan.External = true;
          }
          else
          {
            chan.External = false;
          }

          int iVisible = DatabaseUtility.GetAsInt(results, 0, "Visible");
          if (iVisible != 0)
          {
            chan.VisibleInGuide = true;
          }
          else
          {
            chan.VisibleInGuide = false;
          }


          int scrambled = DatabaseUtility.GetAsInt(results, 0, "scrambled");
          if (scrambled != 0)
          {
            chan.Scrambled = true;
          }
          else
          {
            chan.Scrambled = false;
          }

          int grabepg = DatabaseUtility.GetAsInt(results, 0, "grabEpg");
          if (grabepg != 0)
          {
            chan.AutoGrabEpg = true;
          }
          else
          {
            chan.AutoGrabEpg = false;
          }

          chan.EpgHours = DatabaseUtility.GetAsInt(results, 0, "epgHours");
          chan.LastDateTimeEpgGrabbed = Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, 0, "epgLastUpdate"));

          chan.Sort = DatabaseUtility.GetAsInt(results, 0, "iSort");

          chan.ExternalTunerChannel = DatabaseUtility.Get(results, 0, "ExternalChannel");
          chan.TVStandard = (AnalogVideoStandard) DatabaseUtility.GetAsInt(results, 0, "standard");
          chan.Country = DatabaseUtility.GetAsInt(results, 0, "Country");

          return chan;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return null;
      }
    }

    public static bool GetGenres(ref ArrayList genres)
    {
      lock (typeof (TVDatabase))
      {
        m_genreCache.Clear();
        genres.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from genre");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            string genre = DatabaseUtility.Get(results, i, "strGenre");
            int idGenre = DatabaseUtility.GetAsInt(results, i, "idGenre");
            genres.Add(genres);
            m_genreCache.Add(new CachedGenre(idGenre, genre));
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static void RemoveChannel(string strChannel)
    {
      lock (typeof (TVDatabase))
      {
        if (null == m_db)
        {
          return;
        }
        int iChannelId = GetChannelId(strChannel);
        if (iChannelId < 0)
        {
          return;
        }

        m_channelCache.Clear();
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("delete from tblPrograms where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);

          strSQL = String.Format("delete from canceledseries where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);

          strSQL = String.Format("delete from recording where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);

          strSQL = String.Format("delete from channel where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);

          strSQL = String.Format("delete from tblGroupMapping where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);

          strSQL = String.Format("delete from tblDVBSMapping where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblDVBCMapping where iLCN={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblDVBTMapping where iLCN={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblATSCMapping where iLCN={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblChannelCard where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblEPGMapping where idChannel={0}", iChannelId);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      m_channelCache.Clear();
      ProgramsChanged();
      RecordingsChanged(RecordingChange.Deleted);

      if (OnChannelsChanged != null)
      {
        OnChannelsChanged();
      }
    }

    public static void RemovePrograms()
    {
      lock (typeof (TVDatabase))
      {
        m_genreCache.Clear();
        m_channelCache.Clear();
        try
        {
          if (null == m_db)
          {
            return;
          }
          DateTime dtStart = new DateTime(1971, 11, 6);
          m_db.Execute("delete from tblPrograms");
          m_db.Execute(String.Format("update channel set epglastupdate='{0}'", Util.Utils.datetolong(dtStart)));
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      ProgramsChanged();
    }

    public static bool SearchProgramsPerGenre(string genre1, ArrayList progs, int SearchKind, string SearchCriteria)
    {
      progs.Clear();
      if (genre1 == null)
      {
        return false;
      }
      string genre = genre1;
      DatabaseUtility.RemoveInvalidChars(ref genre);
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' ",
              genre);
          break;
        case 0:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}%' ",
              genre, SearchCriteria);
          break;

        case 1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}%' ",
              genre, SearchCriteria);
          break;

        case 2:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}' ",
              genre, SearchCriteria);
          break;

        case 3:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}' ",
              genre, SearchCriteria);
          break;
      }
      return GetTVProgramsByGenre(strSQL, strOrder, progs);
    }

    public static bool GetProgramsPerGenre(string genre1, ArrayList progs)
    {
      return SearchProgramsPerGenre(genre1, progs, -1, string.Empty);
    }

    public static bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime, ref ArrayList progs)
    {
      lock (typeof (TVDatabase))
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
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and channel.strChannel like '{0}' ",
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
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
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
          Log.Error("TVDatabase exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, strSQL);
          Open();
        }
        return false;
      }
    }

    public static bool GetProgramsPerChannel(string strChannel1, ref ArrayList progs)
    {
      lock (typeof (TVDatabase))
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
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and channel.strChannel like '{0}' ",
              strChannel);
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

            TVProgram prog = new TVProgram();
            prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
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

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, strSQL);
          Open();
        }
        return false;
      }
    }

    public static bool GetProgramTitles(long iStartTime, long iEndTime, ref ArrayList progs)
    {
      lock (typeof (TVDatabase))
      {
        progs.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL =
            "select distinct strtitle,strchannel from tblprograms,channel where tblprograms.idchannel=channel.idchannel ";

          SQLiteResultSet results;
          string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                       iStartTime, iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime>='{0}' and tblPrograms.iStartTime <= '{1}' ) ", iStartTime,
                                 iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime<='{0}' and tblPrograms.iEndTime >= '{1}') )", iStartTime,
                                 iEndTime);
          strSQL += where;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }

          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVProgram prog = new TVProgram();
            prog.Channel = DatabaseUtility.Get(results, i, "strchannel");
            prog.Title = DatabaseUtility.Get(results, i, "strtitle");
            prog.ID = DatabaseUtility.GetAsInt(results, i, "tblPrograms.idProgram");
            progs.Add(prog);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool SearchPrograms(long iStartTime, long iEndTime, ref ArrayList progs, int SearchKind,
                                      string SearchCriteria, string channel)
    {
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel ");
          break;
        case 0:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}%' ",
              SearchCriteria);
          break;
        case 1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}%' ",
              SearchCriteria);
          break;
        case 2:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}' ",
              SearchCriteria);
          break;
        case 3:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}' ",
              SearchCriteria);
          break;
      }
      if (channel != string.Empty)
      {
        DatabaseUtility.RemoveInvalidChars(ref channel);
        strSQL += String.Format(" and channel.strChannel like '{0}' ", channel);
      }
      return GetTVPrograms(iStartTime, iEndTime, strSQL, strOrder, ref progs);
    }

    public static bool SearchMinimalPrograms(long iStartTime, long iEndTime, ref ArrayList progs, int SearchKind,
                                             string SearchCriteria, string channel)
    {
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms  where tblPrograms.idChannel=channel.idChannel ");
          break;
        case 0:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms  where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}%' ",
              SearchCriteria);
          break;
        case 1:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}%' ",
              SearchCriteria);
          break;
        case 2:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}' ",
              SearchCriteria);
          break;
        case 3:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}' ",
              SearchCriteria);
          break;
      }
      if (channel != string.Empty)
      {
        DatabaseUtility.RemoveInvalidChars(ref channel);
        strSQL += String.Format(" and channel.strChannel like '{0}' ", channel);
      }
      return GetMinimalPrograms(iStartTime, iEndTime, strSQL, strOrder, ref progs);
    }

    public static bool SearchProgramsByDescription(long iStartTime, long iEndTime, ref ArrayList progs, int SearchKind,
                                                   string SearchCriteria)
    {
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel  ");
          break;
        case 0:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}%' ",
              SearchCriteria);
          break;
        case 1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}%' ",
              SearchCriteria);
          break;
        case 2:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}' ",
              SearchCriteria);
          break;
        case 3:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}' ",
              SearchCriteria);
          break;
      }
      return GetTVPrograms(iStartTime, iEndTime, strSQL, strOrder, ref progs);
    }

    public static int GetProgramByDescriptionID(string summaryID, ref TVProgram prog)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        if (null == m_db)
        {
          return -1;
        }
        strSQL =
          String.Format(
            "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription ='{0}'",
            summaryID);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 1)
        {
          prog = new TVProgram();


          long iStart = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iStartTime");
          long iEnd = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iEndTime");
          prog.Channel = DatabaseUtility.Get(results, 0, "channel.strChannel");
          prog.Start = iStart;
          prog.End = iEnd;
          prog.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          prog.Title = DatabaseUtility.Get(results, 0, "tblPrograms.strTitle");
          prog.Description = DatabaseUtility.Get(results, 0, "tblPrograms.strDescription");
          prog.Episode = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeName");
          prog.Repeat = DatabaseUtility.Get(results, 0, "tblPrograms.strRepeat");
          prog.ID = DatabaseUtility.GetAsInt(results, 0, "tblPrograms.idProgram");
          prog.SeriesNum = DatabaseUtility.Get(results, 0, "tblPrograms.strSeriesNum");
          prog.EpisodeNum = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeNum");
          prog.EpisodePart = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodePart");
          prog.Date = DatabaseUtility.Get(results, 0, "tblPrograms.strDate");
          prog.StarRating = DatabaseUtility.Get(results, 0, "tblPrograms.strStarRating");
          prog.Classification = DatabaseUtility.Get(results, 0, "tblPrograms.strClassification");
          return 0;
        }
        else
        {
          return -1;
        }
      }
    }

    public static bool GetPrograms(long iStartTime, long iEndTime, ref ArrayList progs)
    {
      return SearchPrograms(iStartTime, iEndTime, ref progs, -1, string.Empty, string.Empty);
    }

    public static void SetRecordedFileName(TVRecorded rec)
    {
      string strFileName = rec.FileName;
      DatabaseUtility.RemoveInvalidChars(ref strFileName);
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("update recorded set strFileName='{0}' where idRecorded={1}", strFileName,
                                        rec.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void SetRecordedKeep(TVRecorded rec)
    {
      string strFileName = rec.FileName;
      DatabaseUtility.RemoveInvalidChars(ref strFileName);
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("update recorded set keepMethod={0}, keepDate='{1}' where idRecorded={2}",
                                        (int) rec.KeepRecordingMethod, Util.Utils.datetolong(rec.KeepRecordingTill),
                                        rec.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void UpdateRecording(TVRecording recording, RecordingChange change)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strTitle = recording.Title;
          DatabaseUtility.RemoveInvalidChars(ref strTitle);

          if (null == m_db)
          {
            return;
          }
          int iChannelId = GetChannelId(recording.Channel);
          if (iChannelId < 0)
          {
            return;
          }
          if (recording.ID < 0)
          {
            return;
          }

          int iContentRec = 1;
          if (!recording.IsContentRecording)
          {
            iContentRec = 0;
          }

          strSQL =
            String.Format(
              "update recording set idChannel={0},iRecordingType={1},strProgram='{2}',iStartTime='{3}',iEndTime='{4}', iCancelTime='{5}', bContentRecording={6}, quality={7}, priority={8},episodesToKeep={9},keepMethod={10},keepDate='{11}', paddingFront={12}, paddingEnd={13} where idRecording={14}",
              iChannelId,
              (int) recording.RecType,
              strTitle,
              recording.Start.ToString(),
              recording.End.ToString(),
              recording.Canceled.ToString(),
              iContentRec,
              (int) recording.Quality,
              recording.Priority,
              recording.EpisodesToKeep,
              (int) recording.KeepRecordingMethod,
              Util.Utils.datetolong(recording.KeepRecordingTill),
              recording.PaddingFront,
              recording.PaddingEnd,
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      SupressEvents = false;
      RecordingsChanged(change);
    }

    public static void SetRecordingQuality(TVRecording recording)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strTitle = recording.Title;
          DatabaseUtility.RemoveInvalidChars(ref strTitle);

          if (null == m_db)
          {
            return;
          }
          if (recording.ID < 0)
          {
            return;
          }

          strSQL = String.Format("update recording set quality={0} where idRecording={1}",
                                 (int) recording.Quality,
                                 recording.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      SupressEvents = false;
      RecordingsChanged(RecordingChange.QualityChange);
    }

    public static void SetRecordingPriority(TVRecording recording)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strTitle = recording.Title;
          DatabaseUtility.RemoveInvalidChars(ref strTitle);

          if (null == m_db)
          {
            return;
          }
          if (recording.ID < 0)
          {
            return;
          }

          strSQL = String.Format("update recording set priority={0} where idRecording={1}",
                                 (int) recording.Priority,
                                 recording.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      SupressEvents = false;
      RecordingsChanged(RecordingChange.PriorityChange);
    }

    public static void SetRecordingEpisodesToKeep(TVRecording recording)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strTitle = recording.Title;
          DatabaseUtility.RemoveInvalidChars(ref strTitle);

          if (null == m_db)
          {
            return;
          }
          if (recording.ID < 0)
          {
            return;
          }

          strSQL = String.Format("update recording set episodesToKeep={0} where idRecording={1}",
                                 (int) recording.EpisodesToKeep,
                                 recording.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      SupressEvents = false;
      RecordingsChanged(RecordingChange.EpisodesToKeepChange);
    }

    public static int AddRecording(ref TVRecording recording)
    {
      int lNewId = -1;
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strTitle = recording.Title;
          DatabaseUtility.RemoveInvalidChars(ref strTitle);

          if (null == m_db)
          {
            Log.Error("TVDatabase.AddRecording:tvdatabase not opened");
            return -1;
          }
          int iChannelId = GetChannelId(recording.Channel);
          if (iChannelId < 0)
          {
            Log.Error("TVDatabase.AddRecording:invalid channel:{0}", recording.Channel);
            return -1;
          }
          int iContentRec = 1;
          if (!recording.IsContentRecording)
          {
            iContentRec = 0;
          }

          if (recording.ID > 0)
          {
            //mjsystem
            strSQL =
              String.Format(
                "insert into recording (idRecording,idChannel,iRecordingType,strProgram,iStartTime,iEndTime,iCancelTime,bContentRecording,quality,priority,episodesToKeep,keepMethod,keepDate,paddingFront,paddingEnd ) values ( {14}, {0}, {1}, '{2}','{3}', '{4}', '{5}', {6}, {7}, {8},{9},{10},'{11}',{12},{13})",
                iChannelId,
                (int) recording.RecType,
                strTitle,
                recording.Start.ToString(),
                recording.End.ToString(),
                recording.Canceled.ToString(),
                iContentRec,
                (int) recording.Quality,
                recording.Priority,
                recording.EpisodesToKeep,
                (int) recording.KeepRecordingMethod,
                Util.Utils.datetolong(recording.KeepRecordingTill),
                recording.PaddingFront,
                recording.PaddingEnd,
                recording.ID
                );
          }
          else
          {
            strSQL =
              String.Format(
                "insert into recording (idRecording,idChannel,iRecordingType,strProgram,iStartTime,iEndTime,iCancelTime,bContentRecording,quality,priority,episodesToKeep,keepMethod,keepDate,paddingFront,paddingEnd ) values ( NULL, {0}, {1}, '{2}','{3}', '{4}', '{5}', {6}, {7}, {8},{9},{10},'{11}',{12},{13})",
                iChannelId,
                (int) recording.RecType,
                strTitle,
                recording.Start.ToString(),
                recording.End.ToString(),
                recording.Canceled.ToString(),
                iContentRec,
                (int) recording.Quality,
                recording.Priority,
                recording.EpisodesToKeep,
                (int) recording.KeepRecordingMethod,
                Util.Utils.datetolong(recording.KeepRecordingTill),
                recording.PaddingFront,
                recording.PaddingEnd
                );
          }
          m_db.Execute(strSQL);
          lNewId = m_db.LastInsertID();
          recording.ID = lNewId;

          foreach (long datetime in recording.CanceledSeries)
          {
            AddCanceledSerie(recording, datetime);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }

      SupressEvents = false;
      RecordingsChanged(RecordingChange.Added);
      return lNewId;
    }

    public static void RemoveRecording(TVRecording record)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("delete from recording where idRecording={0}", record.ID);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from canceledseries where idRecording={0}", record.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      SupressEvents = false;
      RecordingsChanged(RecordingChange.Deleted);
    }

    public static bool GetRecordingsForChannel(string strChannel1, long iStartTime, long iEndTime,
                                               ref ArrayList recordings)
    {
      lock (typeof (TVDatabase))
      {
        recordings.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strChannel = strChannel1;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);

          string strSQL;
          strSQL =
            String.Format(
              "select * from channel,recording where recording.idChannel=channel.idChannel and channel.strChannel like '{0}' order by iStartTime",
              strChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "recording.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "recording.iEndTime");
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
              TVRecording rec = new TVRecording();
              rec.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
              rec.Start = iStart;
              rec.End = iEnd;
              rec.Canceled = DatabaseUtility.GetAsInt64(results, i, "recording.iCancelTime");
              rec.ID = DatabaseUtility.GetAsInt(results, i, "recording.idRecording");
              rec.Title = DatabaseUtility.Get(results, i, "recording.strProgram");
              rec.RecType = (TVRecording.RecordingType) DatabaseUtility.GetAsInt(results, i, "recording.iRecordingType");
              rec.Quality = (TVRecording.QualityType) DatabaseUtility.GetAsInt(results, i, "recording.quality");
              rec.Priority = DatabaseUtility.GetAsInt(results, i, "recording.priority");
              rec.EpisodesToKeep = DatabaseUtility.GetAsInt(results, i, "recording.episodesToKeep");
              int iContentRec = DatabaseUtility.GetAsInt(results, i, "recording.bContentRecording");
              if (iContentRec == 1)
              {
                rec.IsContentRecording = true;
              }
              else
              {
                rec.IsContentRecording = false;
              }
              rec.KeepRecordingMethod =
                (TVRecorded.KeepMethod) DatabaseUtility.GetAsInt(results, i, "recording.keepMethod");
              long date = DatabaseUtility.GetAsInt64(results, i, "recording.keepDate");
              rec.KeepRecordingTill = Util.Utils.longtodate(date);
              rec.PaddingFront = DatabaseUtility.GetAsInt(results, i, "recording.paddingFront");
              rec.PaddingEnd = DatabaseUtility.GetAsInt(results, i, "recording.paddingEnd");
              GetCanceledRecordings(ref rec);
              recordings.Add(rec);
            }
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool GetRecordings(ref List<TVRecording> recordings)
    {
      return GetRecordings(ref recordings, null);
    }

    public static bool GetRecordings(ref List<TVRecording> recordings, TVRecording recording)
    {
      lock (typeof (TVDatabase))
      {
        recordings.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          if (recording != null)
          {
            String strTitle = recording.Title;
            DatabaseUtility.RemoveInvalidChars(ref strTitle);
            strSQL =
              String.Format(
                "select * from channel,recording where recording.idChannel=channel.idChannel and recording.strProgram='{0}' order by iStartTime",
                strTitle);
          }
          else
          {
            strSQL =
              String.Format(
                "select * from channel,recording where recording.idChannel=channel.idChannel order by iStartTime");
          }
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "recording.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "recording.iEndTime");
            TVRecording rec = new TVRecording();
            rec.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
            rec.Start = iStart;
            rec.End = iEnd;
            rec.Canceled = DatabaseUtility.GetAsInt64(results, i, "recording.iCancelTime");
            rec.ID = DatabaseUtility.GetAsInt(results, i, "recording.idRecording");
            rec.Title = DatabaseUtility.Get(results, i, "recording.strProgram");
            rec.RecType = (TVRecording.RecordingType) DatabaseUtility.GetAsInt(results, i, "recording.iRecordingType");
            rec.Quality = (TVRecording.QualityType) DatabaseUtility.GetAsInt(results, i, "recording.quality");
            rec.Priority = DatabaseUtility.GetAsInt(results, i, "recording.priority");
            rec.EpisodesToKeep = DatabaseUtility.GetAsInt(results, i, "recording.episodesToKeep");

            int iContectRec = DatabaseUtility.GetAsInt(results, i, "recording.bContentRecording");
            if (iContectRec == 1)
            {
              rec.IsContentRecording = true;
            }
            else
            {
              rec.IsContentRecording = false;
            }
            rec.KeepRecordingMethod =
              (TVRecorded.KeepMethod) DatabaseUtility.GetAsInt(results, i, "recording.keepMethod");
            long date = DatabaseUtility.GetAsInt64(results, i, "recording.keepDate");
            rec.KeepRecordingTill = Util.Utils.longtodate(date);
            rec.PaddingFront = DatabaseUtility.GetAsInt(results, i, "recording.paddingFront");
            rec.PaddingEnd = DatabaseUtility.GetAsInt(results, i, "recording.paddingEnd");
            GetCanceledRecordings(ref rec);
            recordings.Add(rec);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool GetRecordings(ref ArrayList recordings)
    {
      List<TVRecording> tmpRecs = new List<TVRecording>();
      bool result = GetRecordings(ref tmpRecs);
      recordings = new ArrayList(tmpRecs);
      return result;
    }


    public static void BeginTransaction()
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          m_db.Execute("begin");
        }
        catch (Exception ex)
        {
          Log.Error("tvdatabase begin transaction failed exception err:{0} ", ex.Message);
          Open();
        }
      }
    }

    public static void CommitTransaction()
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          m_db.Execute("commit");
        }
        catch (Exception ex)
        {
          Log.Error("tvdatabase commit failed exception err:{0} ", ex.Message);
          Open();
        }
      }
    }

    public static void RollbackTransaction()
    {
      try
      {
        m_db.Execute("rollback");
      }
      catch (Exception ex)
      {
        Log.Error("tvdatabase rollback failed exception err:{0} ", ex.Message);
        Open();
      }
    }

    public static void PlayedRecordedTV(TVRecorded record)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("update recorded set iPlayed={0} where idRecorded={1}", record.Played, record.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static int AddRecordedTV(TVRecorded recording)
    {
      int lNewId = -1;
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strTitle = recording.Title;
          string strDescription = recording.Description;
          string strFileName = recording.FileName;
          DatabaseUtility.RemoveInvalidChars(ref strTitle);
          DatabaseUtility.RemoveInvalidChars(ref strDescription);
          DatabaseUtility.RemoveInvalidChars(ref strFileName);

          if (null == m_db)
          {
            return -1;
          }
          int iChannelId = GetChannelId(recording.Channel);
          if (iChannelId < 0)
          {
            return -1;
          }
          int iGenreId = AddGenre(recording.Genre);
          if (iGenreId < 0)
          {
            return -1;
          }

          if (recording.ID > 0) //mjsystem
          {
            strSQL =
              String.Format(
                "insert into recorded (idRecorded,idChannel,idGenre,strProgram,iStartTime,iEndTime,strDescription,strFileName,iPlayed,keepMethod,keepDate, idCard) values ( {10}, {0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', {7}, {8}, '{9}', {11})",
                iChannelId,
                iGenreId,
                strTitle,
                recording.Start.ToString(),
                recording.End.ToString(),
                strDescription,
                strFileName,
                recording.Played,
                (int) recording.KeepRecordingMethod,
                Util.Utils.datetolong(recording.KeepRecordingTill),
                recording.ID,
                recording.RecordedCardIndex);
          }
          else
          {
            strSQL =
              String.Format(
                "insert into recorded (idRecorded,idChannel,idGenre,strProgram,iStartTime,iEndTime,strDescription,strFileName,iPlayed,keepMethod,keepDate, idCard) values ( NULL, {0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', {7}, {8}, '{9}', {10})",
                iChannelId,
                iGenreId,
                strTitle,
                recording.Start.ToString(),
                recording.End.ToString(),
                strDescription,
                strFileName,
                recording.Played,
                (int) recording.KeepRecordingMethod,
                Util.Utils.datetolong(recording.KeepRecordingTill),
                recording.RecordedCardIndex);
          }
          m_db.Execute(strSQL);
          lNewId = m_db.LastInsertID();
          recording.ID = lNewId;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      return lNewId;
    }

    public static void RemoveRecordedTV(TVRecorded record)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL = String.Format("delete from recorded where idRecorded={0}", record.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void RemoveRecordedTVByFileName(string fileName)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string filteredName = fileName;
          DatabaseUtility.RemoveInvalidChars(ref filteredName);
          string strSQL = String.Format("delete from recorded where strFileName like '{0}'", filteredName);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static bool GetRecordedTV(ref ArrayList recordings)
    {
      lock (typeof (TVDatabase))
      {
        recordings.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL =
            String.Format(
              "select * from channel,genre,recorded where recorded.idChannel=channel.idChannel and genre.idGenre=recorded.idGenre order by iStartTime");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "recorded.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "recorded.iEndTime");
            TVRecorded rec = new TVRecorded();
            rec.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
            rec.Start = iStart;
            rec.End = iEnd;
            rec.ID = DatabaseUtility.GetAsInt(results, i, "recorded.idRecorded");
            rec.Title = DatabaseUtility.Get(results, i, "recorded.strProgram");
            rec.Description = DatabaseUtility.Get(results, i, "recorded.strDescription");
            rec.FileName = DatabaseUtility.Get(results, i, "recorded.strFileName");
            rec.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
            rec.Played = DatabaseUtility.GetAsInt(results, i, "recorded.iPlayed");
            rec.KeepRecordingMethod =
              (TVRecorded.KeepMethod) DatabaseUtility.GetAsInt(results, i, "recorded.keepMethod");
            long date = DatabaseUtility.GetAsInt64(results, i, "recorded.keepDate");
            rec.KeepRecordingTill = Util.Utils.longtodate(date);
            rec.RecordedCardIndex = DatabaseUtility.GetAsInt(results, i, "recorded.idCard");
            recordings.Add(rec);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
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
    public static bool GetRecordedTVByFilename(string strFile, ref TVRecorded recording)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          DatabaseUtility.RemoveInvalidChars(ref strFile);
          string strSQL;
          strSQL =
            String.Format(
              "select * from channel,genre,recorded where recorded.idChannel=channel.idChannel and genre.idGenre=recorded.idGenre and recorded.strFileName='{0}'",
              strFile);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          long iStart = DatabaseUtility.GetAsInt64(results, 0, "recorded.iStartTime");
          long iEnd = DatabaseUtility.GetAsInt64(results, 0, "recorded.iEndTime");

          recording.Channel = DatabaseUtility.Get(results, 0, "channel.strChannel");
          recording.Start = iStart;
          recording.End = iEnd;
          recording.ID = DatabaseUtility.GetAsInt(results, 0, "recorded.idRecorded");
          recording.Title = DatabaseUtility.Get(results, 0, "recorded.strProgram");
          recording.Description = DatabaseUtility.Get(results, 0, "recorded.strDescription");
          recording.FileName = DatabaseUtility.Get(results, 0, "recorded.strFileName");
          recording.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          recording.Played = DatabaseUtility.GetAsInt(results, 0, "recorded.iPlayed");
          recording.KeepRecordingMethod =
            (TVRecorded.KeepMethod) DatabaseUtility.GetAsInt(results, 0, "recorded.keepMethod");
          long date = DatabaseUtility.GetAsInt64(results, 0, "recorded.keepDate");
          recording.KeepRecordingTill = Util.Utils.longtodate(date);
          recording.RecordedCardIndex = DatabaseUtility.GetAsInt(results, 0, "recorded.idCard");
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool SupressEvents
    {
      get { return m_bSupressEvents; }
      set
      {
        m_bSupressEvents = value;
        if (!m_bSupressEvents)
        {
          if (m_bProgramsChanged)
          {
            ProgramsChanged();
          }
          if (m_bRecordingsChanged)
          {
            RecordingsChanged(RecordingChange.Added);
          }
        }
      }
    }

    public static TVProgram GetLastProgramForChannel(TVChannel chan)
    {
      TVProgram prog = new TVProgram();
      prog.Start = prog.End = 0;
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return prog;
          }

          SQLiteResultSet results;
          string strSQL = String.Format("select * from tblPrograms where idChannel={0} order by iendtime desc limit 1",
                                        chan.ID);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return prog;
          }
          long iStart = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iStartTime");
          long iEnd = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iEndTime");
          prog.Channel = DatabaseUtility.Get(results, 0, "channel.strChannel");
          prog.Start = iStart;
          prog.End = iEnd;
          prog.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          prog.Title = DatabaseUtility.Get(results, 0, "tblPrograms.strTitle");
          prog.Description = DatabaseUtility.Get(results, 0, "tblPrograms.strDescription");
          prog.Episode = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeName");
          prog.Repeat = DatabaseUtility.Get(results, 0, "tblPrograms.strRepeat");
          prog.ID = DatabaseUtility.GetAsInt(results, 0, "tblPrograms.idProgram");
          prog.SeriesNum = DatabaseUtility.Get(results, 0, "tblPrograms.strSeriesNum");
          prog.EpisodeNum = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeNum");
          prog.EpisodePart = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodePart");
          prog.Date = DatabaseUtility.Get(results, 0, "tblPrograms.strDate");
          prog.StarRating = DatabaseUtility.Get(results, 0, "tblPrograms.strStarRating");
          prog.Classification = DatabaseUtility.Get(results, 0, "tblPrograms.strClassification");
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      return prog;
    }

    public static string GetLastProgramEntry()
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          // for single thread multi day grab establish last guide data in db
          if (m_db == null)
          {
            return "";
          }
          string strSQL;
          strSQL = String.Format("select iendtime from tblPrograms order by iendtime desc limit 1");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return "";
          }

          SQLiteResultSet.Row arr = results.Rows[0];
          return (arr.fields[0]).Trim();
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
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
    public static void RemoveOldPrograms()
    {
      Log.WriteFile(LogType.EPG, "RemoveOldPrograms()");
      if (m_db == null)
      {
        return;
      }
      lock (typeof (TVDatabase))
      {
        //delete programs from database that are more than 1 day old
        string strSQL = string.Empty;
        try
        {
          DateTime yesterday = DateTime.Today.AddDays(-1);
          long longYesterday = Util.Utils.datetolong(yesterday);
          strSQL = String.Format("DELETE FROM tblPrograms WHERE iEndTime < '{0}'", longYesterday);

          Log.WriteFile(LogType.EPG, "sql:{0}", strSQL);
          m_db.Execute(strSQL);
          strSQL = String.Format("DELETE FROM canceledseries where iCancelTime < '{0}'", longYesterday);
          Log.WriteFile(LogType.EPG, "sql:{0}", strSQL);
          m_db.Execute(strSQL);
          Log.WriteFile(LogType.EPG, "RemoveOldPrograms done");

          //DatabaseUtility.CompactDatabase(m_db);
          //Log.WriteFile(LogType.EPG, "vacuum done");
        }
        catch (Exception ex)
        {
          Log.WriteFile(LogType.EPG, true, "TVDatabase exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace,
                        strSQL);
          Open();
        }
        return;
      }
    }

    /// <summary>
    /// GetAllPrograms() returns all tv programs found in the database ordered by channel,starttime
    /// </summary>
    /// <param name="programs">Arraylist containing TVProgram instances</param>
    public static void GetAllPrograms(out ArrayList progs)
    {
      progs = new ArrayList();
      lock (typeof (TVDatabase))
      {
        try
        {
          //get all programs
          if (null == m_db)
          {
            return;
          }

          SQLiteResultSet results;
          results =
            m_db.Execute(
              "select * from tblPrograms,channel,genre where tblPrograms.idGenre=genre.idGenre and tblPrograms.idChannel = channel.idChannel order by tblPrograms.idChannel, tblPrograms.iStartTime");
          if (results.Rows.Count == 0)
          {
            return;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iEndTime");
            TVProgram prog = new TVProgram();
            prog.Start = iStart;
            prog.End = iEnd;
            prog.ID = DatabaseUtility.GetAsInt(results, i, "tblPrograms.idProgram");

            prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
            prog.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
            prog.Title = DatabaseUtility.Get(results, i, "tblPrograms.strTitle");
            prog.Description = DatabaseUtility.Get(results, i, "tblPrograms.strDescription");
            prog.Episode = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeName");
            prog.Repeat = DatabaseUtility.Get(results, i, "tblPrograms.strRepeat");
            prog.SeriesNum = DatabaseUtility.Get(results, i, "tblPrograms.strSeriesNum");
            prog.EpisodeNum = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeNum");
            prog.EpisodePart = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodePart");
            prog.Date = DatabaseUtility.Get(results, i, "tblPrograms.strDate");
            prog.StarRating = DatabaseUtility.Get(results, i, "tblPrograms.strStarRating");
            prog.Classification = DatabaseUtility.Get(results, i, "tblPrograms.strClassification");
            progs.Add(prog);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    /// <summary>
    /// OffsetProgramsByHour() will correct the start/end time of all programs in the tvdatabase
    /// </summary>
    /// <param name="Hours">Number of hours to correct (can be positive or negative)</param>
    public static void OffsetProgramsByMinutes(int Minutes)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          ArrayList progs;
          GetAllPrograms(out progs);

          //correct time offsets
          foreach (TVProgram program in progs)
          {
            DateTime dtStart = program.StartTime;
            DateTime dtEnd = program.EndTime;
            dtStart = dtStart.AddMinutes(Minutes);
            dtEnd = dtEnd.AddMinutes(Minutes);
            program.Start = Util.Utils.datetolong(dtStart);
            program.End = Util.Utils.datetolong(dtEnd);

            string sql = String.Format("update tblPrograms set iStartTime='{0}' , iEndTime='{1}' where idProgram={2}",
                                       program.Start, program.End, program.ID);
            m_db.Execute(sql);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
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
    public static void RemoveOverlappingPrograms()
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          //first get a list of all tv channels
          ArrayList channels = new ArrayList();
          GetChannels(ref channels);

          long endTime = Util.Utils.datetolong(new DateTime(2100, 1, 1, 0, 0, 0, 0));
          foreach (TVChannel channel in channels)
          {
            // for each tv channel get all programs
            ArrayList progs = new ArrayList();
            GetProgramsPerChannel(channel.Name, 0, endTime, ref progs);

            long previousEnd = 0;
            long previousStart = 0;
            foreach (TVProgram program in progs)
            {
              bool overlap = false;
              if (previousEnd > program.Start)
              {
                overlap = true;
              }
              if (overlap)
              {
                //remove this program
                string sql = String.Format("delete from tblPrograms where idProgram={0}", program.ID);
                m_db.Execute(sql);
              }
              else
              {
                previousEnd = program.End;
                previousStart = program.Start;
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void DeleteEPGMappings()
    {
      if (null == m_db)
      {
        return;
      }
      m_db.Execute("delete from tblEPGMapping");
    }

    public static void MapEPGChannel(int idChannel, string channelName, string xmlTvId)
    {
      lock (typeof (TVDatabase))
      {
        if (null == m_db)
        {
          return;
        }
        string strSQL;
        try
        {
          string strChannel = channelName;
          string epgId = xmlTvId;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref epgId);
          strSQL = String.Format("select * from tblEPGMapping where idChannel like {0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            strSQL =
              String.Format("insert into tblEPGMapping (idChannel, strChannel ,xmltvid) Values( {0}, '{1}', '{2}')",
                            idChannel, strChannel, epgId);
            m_db.Execute(strSQL);
            ;
            return;
          }
          else
          {
            strSQL = String.Format(
              "update tblEPGMapping set strChannel='{0}', xmltvid='{1}' where idChannel like '{2}'",
              strChannel, epgId, idChannel);
            m_db.Execute(strSQL);
            return;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return;
      }
    }

    public static bool IsChannelMappedToEPG(int idChannel)
    {
      if (m_db == null)
      {
        return false;
      }
      lock (typeof (TVDatabase))
      {
        try
        {
          string strSQL;
          strSQL = String.Format("select * from tblEPGMapping where idChannel={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count > 0)
          {
            return true;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      return false;
    }

    public static bool GetEPGMapping(string xmlTvId, out int idChannel, out string strChannel)
    {
      strChannel = string.Empty;
      idChannel = -1;
      if (m_db == null)
      {
        return false;
      }
      lock (typeof (TVDatabase))
      {
        try
        {
          string epgId = xmlTvId;
          DatabaseUtility.RemoveInvalidChars(ref epgId);
          string strSQL;
          strSQL = String.Format("select * from tblEPGMapping where xmltvid like '{0}'", epgId);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return false;
          }
          idChannel = DatabaseUtility.GetAsInt(results, 0, "idChannel");
          strChannel = DatabaseUtility.Get(results, 0, "strChannel");
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      return false;
    }

    public static int MapDVBTChannel(string channelName, string providerName, int idChannel, int frequency, int ONID,
                                     int TSID, int SID, int audioPid, int videoPid, int teletextPid, int pmtPid,
                                     int bandWidth, int audio1, int audio2, int audio3, int ac3Pid, int pcrPid,
                                     string audioLanguage, string audioLanguage1, string audioLanguage2,
                                     string audioLanguage3, bool HasEITPresentFollow, bool HasEITSchedule)
    {
      lock (typeof (TVDatabase))
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
          string al = audioLanguage;
          string al1 = audioLanguage1;
          string al2 = audioLanguage2;
          string al3 = audioLanguage3;
          al = (al == null ? "" : al.Trim());
          al1 = (al1 == null ? "" : al1.Trim());
          al2 = (al2 == null ? "" : al2.Trim());
          al3 = (al3 == null ? "" : al3.Trim());
          strChannel = (strChannel == null ? "" : strChannel.Trim());
          strChannel = (strChannel == null ? "" : strChannel.Trim());
          DatabaseUtility.RemoveInvalidChars(ref al);
          DatabaseUtility.RemoveInvalidChars(ref al1);
          DatabaseUtility.RemoveInvalidChars(ref al2);
          DatabaseUtility.RemoveInvalidChars(ref al3);

          SQLiteResultSet results;
          strSQL = String.Format("select * from channel ");
          results = m_db.Execute(strSQL);
          int totalchannels = results.Rows.Count;

          strSQL = String.Format("select * from tblDVBTMapping where iLCN like {0}", idChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            //ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3
            strSQL =
              String.Format(
                "insert into tblDVBTMapping (idChannel, strChannel ,strProvider, iLCN , frequency , bandwidth , ONID , TSID , SID , audioPid,videoPid,teletextPid,pmtPid,ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,HasEITPresentFollow , HasEITSchedule ,Visible,pcrPid) Values( NULL, '{0}', '{1}', {2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},'{16}','{17}','{18}','{19}',{20},{21},1,{22})",
                strChannel.Trim(), strProvider.Trim(), idChannel, frequency, bandWidth, ONID, TSID, SID, audioPid,
                videoPid, teletextPid, pmtPid,
                ac3Pid, audio1, audio2, audio3, al, al1, al2, al3,
                (HasEITPresentFollow == true ? 1 : 0), (HasEITSchedule == true ? 1 : 0), pcrPid
                );
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            return idChannel;
          }
          else
          {
            strSQL =
              String.Format(
                "update tblDVBTMapping set frequency='{0}', ONID={1}, TSID={2}, SID={3}, strChannel='{4}',strProvider='{5}',audioPid={6},videoPid={7},teletextPid={8},pmtPid={9}, bandwidth={10},ac3Pid={11},audio1Pid={12},audio2Pid={13},audio3Pid={14},sAudioLang='{15}',sAudioLang1='{16}',sAudioLang2='{17}',sAudioLang3='{18}',HasEITPresentFollow={19} , HasEITSchedule={20}, pcrPid={21}  where iLCN like '{22}'",
                frequency, ONID, TSID, SID, strChannel.Trim(), strProvider.Trim(), audioPid, videoPid, teletextPid,
                pmtPid, bandWidth,
                ac3Pid, audio1, audio2, audio3, al, al1, al2, al3,
                (HasEITPresentFollow == true ? 1 : 0), (HasEITSchedule == true ? 1 : 0), pcrPid
                , idChannel);
            //	Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            return idChannel;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency,
                                     int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID,
                                     int audioPid, int videoPid, int teletextPid, int pmtPid, int audio1, int audio2,
                                     int audio3, int ac3Pid, int pcrPid, string audioLanguage, string audioLanguage1,
                                     string audioLanguage2, string audioLanguage3, bool HasEITPresentFollow,
                                     bool HasEITSchedule)
    {
      lock (typeof (TVDatabase))
      {
        if (null == m_db)
        {
          return -1;
        }
        string strSQL = string.Empty;
        try
        {
          string strChannel = channelName;
          string strProvider = providerName;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strProvider);
          string al = audioLanguage;
          string al1 = audioLanguage1;
          string al2 = audioLanguage2;
          string al3 = audioLanguage3;
          al = (al == null ? "" : al.Trim());
          al1 = (al1 == null ? "" : al1.Trim());
          al2 = (al2 == null ? "" : al2.Trim());
          al3 = (al3 == null ? "" : al3.Trim());
          strChannel = (strChannel == null ? "" : strChannel.Trim());
          strChannel = (strChannel == null ? "" : strChannel.Trim());
          DatabaseUtility.RemoveInvalidChars(ref al);
          DatabaseUtility.RemoveInvalidChars(ref al1);
          DatabaseUtility.RemoveInvalidChars(ref al2);
          DatabaseUtility.RemoveInvalidChars(ref al3);

          SQLiteResultSet results;
          strSQL = String.Format("select * from channel ");
          results = m_db.Execute(strSQL);
          int totalchannels = results.Rows.Count;

          strSQL = String.Format("select * from tblDVBCMapping where iLCN like {0}", idChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL =
              String.Format(
                "insert into tblDVBCMapping (idChannel, strChannel,strProvider,iLCN,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,videoPid,teletextPid,pmtPid,ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3, HasEITPresentFollow , HasEITSchedule ,Visible,pcrPid) Values( NULL, '{0}', '{1}', {2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},'{18}','{19}','{20}','{21}',{22},{23},1,{24})"
                , strChannel.Trim(), strProvider.Trim(), idChannel, frequency, symbolrate, innerFec, modulation, ONID,
                TSID, SID, audioPid, videoPid, teletextPid, pmtPid,
                ac3Pid, audio1, audio2, audio3, al, al1, al2, al3,
                (HasEITPresentFollow == true ? 1 : 0), (HasEITSchedule == true ? 1 : 0), pcrPid);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            return idChannel;
          }
          else
          {
            strSQL =
              String.Format(
                "update tblDVBCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, videoPid={10}, teletextPid={11}, pmtPid={12},ac3Pid={13},audio1Pid={14},audio2Pid={15},audio3Pid={16},sAudioLang='{17}',sAudioLang1='{18}',sAudioLang2='{19}',sAudioLang3='{20}',HasEITPresentFollow={21} , HasEITSchedule={22},pcrPid={23}  where iLCN like '{24}'",
                frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, strChannel.Trim(), strProvider.Trim(),
                audioPid, videoPid, teletextPid, pmtPid,
                ac3Pid, audio1, audio2, audio3, al, al1, al2, al3,
                (HasEITPresentFollow == true ? 1 : 0), (HasEITSchedule == true ? 1 : 0), pcrPid,
                idChannel);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            return idChannel;
          }
        }
        catch (Exception ex)
        {
          Log.Error("sql:{0}", strSQL);
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static int MapATSCChannel(string channelName, int physicalChannel, int minorChannel, int majorChannel,
                                     string providerName, int idChannel, int frequency, int symbolrate, int innerFec,
                                     int modulation, int ONID, int TSID, int SID, int audioPid, int videoPid,
                                     int teletextPid, int pmtPid, int audio1, int audio2, int audio3, int ac3Pid,
                                     int pcrPid, string audioLanguage, string audioLanguage1, string audioLanguage2,
                                     string audioLanguage3, bool HasEITPresentFollow, bool HasEITSchedule)
    {
      lock (typeof (TVDatabase))
      {
        if (null == m_db)
        {
          return -1;
        }
        string strSQL = string.Empty;
        try
        {
          string strChannel = channelName;
          string strProvider = providerName;
          DatabaseUtility.RemoveInvalidChars(ref strChannel);
          DatabaseUtility.RemoveInvalidChars(ref strProvider);
          string al = audioLanguage;
          string al1 = audioLanguage1;
          string al2 = audioLanguage2;
          string al3 = audioLanguage3;
          al = (al == null ? "" : al.Trim());
          al1 = (al1 == null ? "" : al1.Trim());
          al2 = (al2 == null ? "" : al2.Trim());
          al3 = (al3 == null ? "" : al3.Trim());
          strChannel = (strChannel == null ? "" : strChannel.Trim());
          strChannel = (strChannel == null ? "" : strChannel.Trim());
          DatabaseUtility.RemoveInvalidChars(ref al);
          DatabaseUtility.RemoveInvalidChars(ref al1);
          DatabaseUtility.RemoveInvalidChars(ref al2);
          DatabaseUtility.RemoveInvalidChars(ref al3);

          SQLiteResultSet results;
          strSQL = String.Format("select * from channel ");
          results = m_db.Execute(strSQL);
          int totalchannels = results.Rows.Count;

          strSQL = String.Format("select * from tblATSCMapping where iLCN like {0}", idChannel);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL =
              String.Format(
                "insert into tblATSCMapping (idChannel, strChannel,strProvider,iLCN,frequency,symbolrate,innerFec,modulation,ONID,TSID,SID,audioPid,videoPid,teletextPid,pmtPid,ac3Pid,audio1Pid,audio2Pid,audio3Pid,sAudioLang,sAudioLang1,sAudioLang2,sAudioLang3,channelNumber,minorChannel,majorChannel, HasEITPresentFollow , HasEITSchedule ,Visible,pcrPid) Values( NULL, '{0}', '{1}', {2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},'{18}','{19}','{20}','{21}',{22},{23},{24},{25},{26},1,{27})"
                , strChannel.Trim(), strProvider.Trim(), idChannel, frequency, symbolrate, innerFec, modulation, ONID,
                TSID, SID, audioPid, videoPid, teletextPid, pmtPid,
                ac3Pid, audio1, audio2, audio3, al, al1, al2, al3, physicalChannel, minorChannel, majorChannel,
                (HasEITPresentFollow == true ? 1 : 0), (HasEITSchedule == true ? 1 : 0), pcrPid);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            return idChannel;
          }
          else
          {
            strSQL =
              String.Format(
                "update tblATSCMapping set frequency='{0}', symbolrate={1}, innerFec={2}, modulation={3}, ONID={4}, TSID={5}, SID={6}, strChannel='{7}', strProvider='{8}',audioPid={9}, videoPid={10}, teletextPid={11}, pmtPid={12},ac3Pid={13},audio1Pid={14},audio2Pid={15},audio3Pid={16},sAudioLang='{17}',sAudioLang1='{18}',sAudioLang2='{19}',sAudioLang3='{20}', channelNumber={21}, majorChannel={22}, minorChannel={23},HasEITPresentFollow={24} , HasEITSchedule={25},pcrPid={26}  where iLCN like '{27}'",
                frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, strChannel.Trim(), strProvider.Trim(),
                audioPid, videoPid, teletextPid, pmtPid,
                ac3Pid, audio1, audio2, audio3, al, al1, al2, al3, physicalChannel, majorChannel, minorChannel,
                (HasEITPresentFollow == true ? 1 : 0), (HasEITSchedule == true ? 1 : 0), pcrPid,
                idChannel);
            //Log.Error("sql:{0}", strSQL);
            m_db.Execute(strSQL);
            return idChannel;
          }
        }
        catch (Exception ex)
        {
          Log.Error("sql:{0}", strSQL);
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static bool GetDVBTChannel(int idChannel, ref DVBChannel retChannel)
    {
      int bandwidth = -1;
      int frequency = -1, ONID = -1, TSID = -1, SID = -1;
      int audioPid = -1, videoPid = -1, teletextPid = -1, pmtPid = -1, pcrPid = -1;
      string strProvider;
      int audio1, audio2, audio3, ac3Pid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      bool HasEITPresentFollow, HasEITSchedule;

      HasEITPresentFollow = HasEITSchedule = false;
      audio1 = audio2 = audio3 = ac3Pid = -1;
      audioLanguage = audioLanguage1 = audioLanguage2 = audioLanguage3 = "";
      bandwidth = -1;
      audioPid = videoPid = teletextPid = 0;
      strProvider = "";
      frequency = -1;
      pmtPid = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return false;
      }
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from tblDVBTMapping where iLCN={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return false;
          }
          frequency = DatabaseUtility.GetAsInt(results, 0, "frequency");
          ONID = DatabaseUtility.GetAsInt(results, 0, "ONID");
          TSID = DatabaseUtility.GetAsInt(results, 0, "TSID");
          SID = DatabaseUtility.GetAsInt(results, 0, "SID");
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = DatabaseUtility.GetAsInt(results, 0, "audioPid");
          videoPid = DatabaseUtility.GetAsInt(results, 0, "videoPid");
          teletextPid = DatabaseUtility.GetAsInt(results, 0, "teletextPid");
          pmtPid = DatabaseUtility.GetAsInt(results, 0, "pmtPid");
          bandwidth = DatabaseUtility.GetAsInt(results, 0, "bandwidth");
          audio1 = DatabaseUtility.GetAsInt(results, 0, "audio1Pid");
          audio2 = DatabaseUtility.GetAsInt(results, 0, "audio2Pid");
          audio3 = DatabaseUtility.GetAsInt(results, 0, "audio3Pid");
          ac3Pid = DatabaseUtility.GetAsInt(results, 0, "ac3Pid");
          audioLanguage = DatabaseUtility.Get(results, 0, "sAudioLang");
          audioLanguage1 = DatabaseUtility.Get(results, 0, "sAudioLang1");
          audioLanguage2 = DatabaseUtility.Get(results, 0, "sAudioLang2");
          audioLanguage3 = DatabaseUtility.Get(results, 0, "sAudioLang3");
          HasEITPresentFollow = DatabaseUtility.GetAsInt(results, 0, "HasEITPresentFollow") != 0;
          HasEITSchedule = DatabaseUtility.GetAsInt(results, 0, "HasEITSchedule") != 0;
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");

          retChannel.Frequency = frequency;
          retChannel.NetworkID = ONID;
          retChannel.TransportStreamID = TSID;
          retChannel.ProgramNumber = SID;
          retChannel.ServiceProvider = strProvider;
          retChannel.AudioPid = audioPid;
          retChannel.VideoPid = videoPid;
          retChannel.TeletextPid = teletextPid;
          retChannel.SubtitlePid = 0;
          retChannel.PMTPid = pmtPid;
          retChannel.Bandwidth = bandwidth;
          retChannel.Audio1 = audio1;
          retChannel.Audio2 = audio2;
          retChannel.Audio3 = audio3;
          retChannel.AC3Pid = ac3Pid;
          retChannel.AudioLanguage = audioLanguage;
          retChannel.AudioLanguage1 = audioLanguage1;
          retChannel.AudioLanguage2 = audioLanguage2;
          retChannel.AudioLanguage3 = audioLanguage3;
          retChannel.HasEITPresentFollow = HasEITPresentFollow;
          retChannel.HasEITSchedule = HasEITSchedule;
          retChannel.PCRPid = pcrPid;

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool GetDVBCChannel(int idChannel, ref DVBChannel retChannel)
    {
      int frequency = -1, ONID = -1, TSID = -1, SID = -1, symbolrate = -1, innerFec = -1, modulation = -1;
      int audioPid = -1, videoPid = -1, teletextPid = -1, pmtPid = -1, pcrPid = -1;
      string strProvider;
      int audio1, audio2, audio3, ac3Pid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      bool HasEITPresentFollow, HasEITSchedule;

      HasEITPresentFollow = HasEITSchedule = false;
      audio1 = audio2 = audio3 = ac3Pid = -1;
      audioLanguage = audioLanguage1 = audioLanguage2 = audioLanguage3 = "";
      pmtPid = -1;
      audioPid = videoPid = teletextPid = 0;
      strProvider = "";
      frequency = -1;
      symbolrate = -1;
      innerFec = -1;
      modulation = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return false;
      }
      //Log.Error("GetTuneRequest for iLCN:{0}", iLCN);
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from tblDVBCMapping where iLCN={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return false;
          }
          frequency = DatabaseUtility.GetAsInt(results, 0, "frequency");
          symbolrate = DatabaseUtility.GetAsInt(results, 0, "symbolrate");
          innerFec = DatabaseUtility.GetAsInt(results, 0, "innerFec");
          modulation = DatabaseUtility.GetAsInt(results, 0, "modulation");
          ONID = DatabaseUtility.GetAsInt(results, 0, "ONID");
          TSID = DatabaseUtility.GetAsInt(results, 0, "TSID");
          SID = DatabaseUtility.GetAsInt(results, 0, "SID");
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = DatabaseUtility.GetAsInt(results, 0, "audioPid");
          videoPid = DatabaseUtility.GetAsInt(results, 0, "videoPid");
          teletextPid = DatabaseUtility.GetAsInt(results, 0, "teletextPid");
          pmtPid = DatabaseUtility.GetAsInt(results, 0, "pmtPid");
          audio1 = DatabaseUtility.GetAsInt(results, 0, "audio1Pid");
          audio2 = DatabaseUtility.GetAsInt(results, 0, "audio2Pid");
          audio3 = DatabaseUtility.GetAsInt(results, 0, "audio3Pid");
          ac3Pid = DatabaseUtility.GetAsInt(results, 0, "ac3Pid");
          audioLanguage = DatabaseUtility.Get(results, 0, "sAudioLang");
          audioLanguage1 = DatabaseUtility.Get(results, 0, "sAudioLang1");
          audioLanguage2 = DatabaseUtility.Get(results, 0, "sAudioLang2");
          audioLanguage3 = DatabaseUtility.Get(results, 0, "sAudioLang3");

          HasEITPresentFollow = DatabaseUtility.GetAsInt(results, 0, "HasEITPresentFollow") != 0;
          HasEITSchedule = DatabaseUtility.GetAsInt(results, 0, "HasEITSchedule") != 0;
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");

          retChannel.Frequency = frequency;
          retChannel.Symbolrate = symbolrate;
          retChannel.FEC = innerFec;
          retChannel.Modulation = modulation;
          retChannel.NetworkID = ONID;
          retChannel.TransportStreamID = TSID;
          retChannel.ProgramNumber = SID;
          retChannel.ServiceProvider = strProvider;
          retChannel.AudioPid = audioPid;
          retChannel.VideoPid = videoPid;
          retChannel.TeletextPid = teletextPid;
          retChannel.SubtitlePid = 0;
          retChannel.PMTPid = pmtPid;
          retChannel.Audio1 = audio1;
          retChannel.Audio2 = audio2;
          retChannel.Audio3 = audio3;
          retChannel.AC3Pid = ac3Pid;
          retChannel.AudioLanguage = audioLanguage;
          retChannel.AudioLanguage1 = audioLanguage1;
          retChannel.AudioLanguage2 = audioLanguage2;
          retChannel.AudioLanguage3 = audioLanguage3;
          retChannel.HasEITPresentFollow = HasEITPresentFollow;
          retChannel.HasEITSchedule = HasEITSchedule;
          retChannel.PCRPid = pcrPid;

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool GetATSCChannel(int idChannel, ref DVBChannel retChannel)
    {
      int minorChannel = -1, majorChannel = -1, physicalChannel = -1, symbolrate = -1, innerFec = -1, modulation = -1;
      int frequency = -1, ONID = -1, TSID = -1, SID = -1;
      int audioPid = -1, videoPid = -1, teletextPid = -1, pmtPid = -1, pcrPid = -1;
      string strProvider;
      int audio1, audio2, audio3, ac3Pid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      bool HasEITPresentFollow, HasEITSchedule;

      HasEITPresentFollow = HasEITSchedule = false;
      minorChannel = -1;
      majorChannel = -1;
      audio1 = audio2 = audio3 = ac3Pid = -1;
      physicalChannel = -1;
      audioLanguage = audioLanguage1 = audioLanguage2 = audioLanguage3 = "";
      pmtPid = -1;
      audioPid = videoPid = teletextPid = 0;
      strProvider = "";
      frequency = -1;
      symbolrate = -1;
      innerFec = -1;
      modulation = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return false;
      }
      //Log.Error("GetTuneRequest for iLCN:{0}", iLCN);
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from tblATSCMapping where iLCN={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return false;
          }
          frequency = DatabaseUtility.GetAsInt(results, 0, "frequency");
          symbolrate = DatabaseUtility.GetAsInt(results, 0, "symbolrate");
          innerFec = DatabaseUtility.GetAsInt(results, 0, "innerFec");
          modulation = DatabaseUtility.GetAsInt(results, 0, "modulation");
          ONID = DatabaseUtility.GetAsInt(results, 0, "ONID");
          TSID = DatabaseUtility.GetAsInt(results, 0, "TSID");
          SID = DatabaseUtility.GetAsInt(results, 0, "SID");
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = DatabaseUtility.GetAsInt(results, 0, "audioPid");
          videoPid = DatabaseUtility.GetAsInt(results, 0, "videoPid");
          teletextPid = DatabaseUtility.GetAsInt(results, 0, "teletextPid");
          pmtPid = DatabaseUtility.GetAsInt(results, 0, "pmtPid");
          audio1 = DatabaseUtility.GetAsInt(results, 0, "audio1Pid");
          audio2 = DatabaseUtility.GetAsInt(results, 0, "audio2Pid");
          audio3 = DatabaseUtility.GetAsInt(results, 0, "audio3Pid");
          ac3Pid = DatabaseUtility.GetAsInt(results, 0, "ac3Pid");
          audioLanguage = DatabaseUtility.Get(results, 0, "sAudioLang");
          audioLanguage1 = DatabaseUtility.Get(results, 0, "sAudioLang1");
          audioLanguage2 = DatabaseUtility.Get(results, 0, "sAudioLang2");
          audioLanguage3 = DatabaseUtility.Get(results, 0, "sAudioLang3");
          physicalChannel = DatabaseUtility.GetAsInt(results, 0, "channelNumber");
          minorChannel = DatabaseUtility.GetAsInt(results, 0, "minorChannel");
          majorChannel = DatabaseUtility.GetAsInt(results, 0, "majorChannel");
          HasEITPresentFollow = DatabaseUtility.GetAsInt(results, 0, "HasEITPresentFollow") != 0;
          HasEITSchedule = DatabaseUtility.GetAsInt(results, 0, "HasEITSchedule") != 0;
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");

          retChannel.Frequency = frequency;
          retChannel.Symbolrate = symbolrate;
          retChannel.FEC = innerFec;
          retChannel.Modulation = modulation;
          retChannel.NetworkID = ONID;
          retChannel.TransportStreamID = TSID;
          retChannel.ProgramNumber = SID;
          retChannel.ServiceProvider = strProvider;
          retChannel.AudioPid = audioPid;
          retChannel.VideoPid = videoPid;
          retChannel.TeletextPid = teletextPid;
          retChannel.SubtitlePid = 0;
          retChannel.PMTPid = pmtPid;
          retChannel.Audio1 = audio1;
          retChannel.Audio2 = audio2;
          retChannel.Audio3 = audio3;
          retChannel.AC3Pid = ac3Pid;
          retChannel.AudioLanguage = audioLanguage;
          retChannel.AudioLanguage1 = audioLanguage1;
          retChannel.AudioLanguage2 = audioLanguage2;
          retChannel.AudioLanguage3 = audioLanguage3;
          retChannel.PhysicalChannel = physicalChannel;
          retChannel.MinorChannel = minorChannel;
          retChannel.MajorChannel = majorChannel;
          retChannel.HasEITSchedule = HasEITSchedule;
          retChannel.HasEITPresentFollow = HasEITPresentFollow;
          retChannel.PCRPid = pcrPid;

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool GetDVBTTuneRequest(int idChannel, out string strProvider, out int frequency, out int ONID,
                                          out int TSID, out int SID, out int audioPid, out int videoPid,
                                          out int teletextPid, out int pmtPid, out int bandwidth, out int audio1,
                                          out int audio2, out int audio3, out int ac3Pid, out string audioLanguage,
                                          out string audioLanguage1, out string audioLanguage2,
                                          out string audioLanguage3, out bool HasEITPresentFollow,
                                          out bool HasEITSchedule, out int pcrPid)
    {
      HasEITPresentFollow = HasEITSchedule = false;
      audio1 = audio2 = audio3 = ac3Pid = -1;
      audioLanguage = audioLanguage1 = audioLanguage2 = audioLanguage3 = "";
      bandwidth = -1;
      audioPid = videoPid = teletextPid = 0;
      strProvider = "";
      frequency = -1;
      pmtPid = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return false;
      }
      //Log.Error("GetTuneRequest for iLCN:{0}", iLCN);
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from tblDVBTMapping where iLCN={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return false;
          }
          frequency = DatabaseUtility.GetAsInt(results, 0, "frequency");
          ONID = DatabaseUtility.GetAsInt(results, 0, "ONID");
          TSID = DatabaseUtility.GetAsInt(results, 0, "TSID");
          SID = DatabaseUtility.GetAsInt(results, 0, "SID");
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = DatabaseUtility.GetAsInt(results, 0, "audioPid");
          videoPid = DatabaseUtility.GetAsInt(results, 0, "videoPid");
          teletextPid = DatabaseUtility.GetAsInt(results, 0, "teletextPid");
          pmtPid = DatabaseUtility.GetAsInt(results, 0, "pmtPid");
          bandwidth = DatabaseUtility.GetAsInt(results, 0, "bandwidth");
          audio1 = DatabaseUtility.GetAsInt(results, 0, "audio1Pid");
          audio2 = DatabaseUtility.GetAsInt(results, 0, "audio2Pid");
          audio3 = DatabaseUtility.GetAsInt(results, 0, "audio3Pid");
          ac3Pid = DatabaseUtility.GetAsInt(results, 0, "ac3Pid");
          audioLanguage = DatabaseUtility.Get(results, 0, "sAudioLang");
          audioLanguage1 = DatabaseUtility.Get(results, 0, "sAudioLang1");
          audioLanguage2 = DatabaseUtility.Get(results, 0, "sAudioLang2");
          audioLanguage3 = DatabaseUtility.Get(results, 0, "sAudioLang3");
          HasEITPresentFollow = DatabaseUtility.GetAsInt(results, 0, "HasEITPresentFollow") != 0;
          HasEITSchedule = DatabaseUtility.GetAsInt(results, 0, "HasEITSchedule") != 0;
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      return false;
    }

    public static bool GetDVBCTuneRequest(int idChannel, out string strProvider, out int frequency, out int symbolrate,
                                          out int innerFec, out int modulation, out int ONID, out int TSID, out int SID,
                                          out int audioPid, out int videoPid, out int teletextPid, out int pmtPid,
                                          out int audio1, out int audio2, out int audio3, out int ac3Pid,
                                          out string audioLanguage, out string audioLanguage1, out string audioLanguage2,
                                          out string audioLanguage3, out bool HasEITPresentFollow,
                                          out bool HasEITSchedule, out int pcrPid)
    {
      HasEITPresentFollow = HasEITSchedule = false;
      audio1 = audio2 = audio3 = ac3Pid = -1;
      audioLanguage = audioLanguage1 = audioLanguage2 = audioLanguage3 = "";
      pmtPid = -1;
      audioPid = videoPid = teletextPid = 0;
      strProvider = "";
      frequency = -1;
      symbolrate = -1;
      innerFec = -1;
      modulation = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return false;
      }
      //Log.Error("GetTuneRequest for iLCN:{0}", iLCN);
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from tblDVBCMapping where iLCN={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return false;
          }
          frequency = DatabaseUtility.GetAsInt(results, 0, "frequency");
          symbolrate = DatabaseUtility.GetAsInt(results, 0, "symbolrate");
          innerFec = DatabaseUtility.GetAsInt(results, 0, "innerFec");
          modulation = DatabaseUtility.GetAsInt(results, 0, "modulation");
          ONID = DatabaseUtility.GetAsInt(results, 0, "ONID");
          TSID = DatabaseUtility.GetAsInt(results, 0, "TSID");
          SID = DatabaseUtility.GetAsInt(results, 0, "SID");
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = DatabaseUtility.GetAsInt(results, 0, "audioPid");
          videoPid = DatabaseUtility.GetAsInt(results, 0, "videoPid");
          teletextPid = DatabaseUtility.GetAsInt(results, 0, "teletextPid");
          pmtPid = DatabaseUtility.GetAsInt(results, 0, "pmtPid");
          audio1 = DatabaseUtility.GetAsInt(results, 0, "audio1Pid");
          audio2 = DatabaseUtility.GetAsInt(results, 0, "audio2Pid");
          audio3 = DatabaseUtility.GetAsInt(results, 0, "audio3Pid");
          ac3Pid = DatabaseUtility.GetAsInt(results, 0, "ac3Pid");
          audioLanguage = DatabaseUtility.Get(results, 0, "sAudioLang");
          audioLanguage1 = DatabaseUtility.Get(results, 0, "sAudioLang1");
          audioLanguage2 = DatabaseUtility.Get(results, 0, "sAudioLang2");
          audioLanguage3 = DatabaseUtility.Get(results, 0, "sAudioLang3");

          HasEITPresentFollow = DatabaseUtility.GetAsInt(results, 0, "HasEITPresentFollow") != 0;
          HasEITSchedule = DatabaseUtility.GetAsInt(results, 0, "HasEITSchedule") != 0;
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      return false;
    }

    public static void GetATSCTuneRequest(int idChannel, out int physicalChannel, out string strProvider,
                                          out int frequency, out int symbolrate, out int innerFec, out int modulation,
                                          out int ONID, out int TSID, out int SID, out int audioPid, out int videoPid,
                                          out int teletextPid, out int pmtPid, out int audio1, out int audio2,
                                          out int audio3, out int ac3Pid, out string audioLanguage,
                                          out string audioLanguage1, out string audioLanguage2,
                                          out string audioLanguage3, out int minorChannel, out int majorChannel,
                                          out bool HasEITPresentFollow, out bool HasEITSchedule, out int pcrPid)
    {
      HasEITPresentFollow = HasEITSchedule = false;
      minorChannel = -1;
      majorChannel = -1;
      audio1 = audio2 = audio3 = ac3Pid = -1;
      physicalChannel = -1;
      audioLanguage = audioLanguage1 = audioLanguage2 = audioLanguage3 = "";
      pmtPid = -1;
      audioPid = videoPid = teletextPid = 0;
      strProvider = "";
      frequency = -1;
      symbolrate = -1;
      innerFec = -1;
      modulation = -1;
      ONID = -1;
      TSID = -1;
      SID = -1;
      pcrPid = -1;
      if (m_db == null)
      {
        return;
      }
      //Log.Error("GetTuneRequest for iLCN:{0}", iLCN);
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return;
          }
          string strSQL;
          strSQL = String.Format("select * from tblATSCMapping where iLCN={0}", idChannel);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 1)
          {
            return;
          }
          frequency = DatabaseUtility.GetAsInt(results, 0, "frequency");
          symbolrate = DatabaseUtility.GetAsInt(results, 0, "symbolrate");
          innerFec = DatabaseUtility.GetAsInt(results, 0, "innerFec");
          modulation = DatabaseUtility.GetAsInt(results, 0, "modulation");
          ONID = DatabaseUtility.GetAsInt(results, 0, "ONID");
          TSID = DatabaseUtility.GetAsInt(results, 0, "TSID");
          SID = DatabaseUtility.GetAsInt(results, 0, "SID");
          strProvider = DatabaseUtility.Get(results, 0, "strProvider");
          audioPid = DatabaseUtility.GetAsInt(results, 0, "audioPid");
          videoPid = DatabaseUtility.GetAsInt(results, 0, "videoPid");
          teletextPid = DatabaseUtility.GetAsInt(results, 0, "teletextPid");
          pmtPid = DatabaseUtility.GetAsInt(results, 0, "pmtPid");
          audio1 = DatabaseUtility.GetAsInt(results, 0, "audio1Pid");
          audio2 = DatabaseUtility.GetAsInt(results, 0, "audio2Pid");
          audio3 = DatabaseUtility.GetAsInt(results, 0, "audio3Pid");
          ac3Pid = DatabaseUtility.GetAsInt(results, 0, "ac3Pid");
          audioLanguage = DatabaseUtility.Get(results, 0, "sAudioLang");
          audioLanguage1 = DatabaseUtility.Get(results, 0, "sAudioLang1");
          audioLanguage2 = DatabaseUtility.Get(results, 0, "sAudioLang2");
          audioLanguage3 = DatabaseUtility.Get(results, 0, "sAudioLang3");
          physicalChannel = DatabaseUtility.GetAsInt(results, 0, "channelNumber");
          minorChannel = DatabaseUtility.GetAsInt(results, 0, "minorChannel");
          majorChannel = DatabaseUtility.GetAsInt(results, 0, "majorChannel");
          HasEITPresentFollow = DatabaseUtility.GetAsInt(results, 0, "HasEITPresentFollow") != 0;
          HasEITSchedule = DatabaseUtility.GetAsInt(results, 0, "HasEITSchedule") != 0;
          pcrPid = DatabaseUtility.GetAsInt(results, 0, "pcrPid");
          return;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void GetGroups(ref ArrayList groups)
    {
      groups = new ArrayList();
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;
          strSQL = String.Format("select * from tblGroups order by iSort");
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVGroup group = new TVGroup();
            group.ID = DatabaseUtility.GetAsInt(results, i, "idGroup");
            group.Sort = DatabaseUtility.GetAsInt(results, i, "iSort");
            group.Pincode = DatabaseUtility.GetAsInt(results, i, "Pincode");
            group.GroupName = DatabaseUtility.Get(results, i, "strName");
            groups.Add(group);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void GetTVChannelsForGroup(int groupId, List<TVChannel> groupChannels)
    {
      groupChannels.Clear();
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          List<TVChannel> tvchannels = new List<TVChannel>();
          GetChannels(ref tvchannels);

          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;
          strSQL =
            String.Format(
              "select * from tblGroups,tblGroupMapping where tblGroups.idGroup=tblGroupMapping.idGroup and tblGroups.idGroup={0} order by tblGroupMapping.iSort",
              groupId);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            int channelid = DatabaseUtility.GetAsInt(results, i, "tblGroupMapping.idChannel");
            foreach (TVChannel chan in tvchannels)
            {
              if (chan.ID == channelid)
              {
                groupChannels.Add(chan);
                break;
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void GetCards(ref ArrayList cards)
    {
      cards.Clear();
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;

          strSQL = String.Format("select distinct card from tblChannelCard ");

          results = m_db.Execute(strSQL);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            cards.Add(DatabaseUtility.GetAsInt(results, i, "card"));
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void MapChannelToCard(int channel, int card)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;

          strSQL = String.Format("select * from tblChannelCard where idChannel={0} and card={1}", channel, card);

          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL = String.Format(
              "insert into tblChannelCard (idChannelCard, idChannel,card) values ( NULL, {0}, {1})",
              channel, card);
            m_db.Execute(strSQL);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void DeleteCard(int card)
    {
      lock (typeof (TVDatabase))
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
            int id = DatabaseUtility.GetAsInt(results, i, "idChannelCard");
            int cardnr = DatabaseUtility.GetAsInt(results, i, "card");
            cardnr--;
            strSQL = String.Format("update tblChannelCard set card={0} where idChannelCard={1}",
                                   cardnr, id);
            m_db.Execute(strSQL);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void UnmapChannelFromCard(TVChannel channel, int card)
    {
      lock (typeof (TVDatabase))
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
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
    public static bool CanCardViewTVChannel(string channelName, int card)
    {
      string tvChannelName = channelName;
      DatabaseUtility.RemoveInvalidChars(ref tvChannelName);

      lock (typeof (TVDatabase))
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
              "select * from tblChannelCard,channel where channel.idChannel=tblChannelCard.idChannel and channel.strChannel like '{0}' and tblChannelCard.card={1}",
              tvChannelName, card);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count != 0)
          {
            return true;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      return false;
    }

    public static void MapChannelToGroup(TVGroup group, TVChannel channel)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strgroupName = group.GroupName;
          DatabaseUtility.RemoveInvalidChars(ref strgroupName);

          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;

          strSQL = String.Format("select * from tblGroupMapping where idGroup={0} and idChannel={1}",
                                 group.ID, channel.ID);

          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            strSQL =
              String.Format(
                "insert into tblGroupMapping (idGroupMapping, idGroup,idChannel,iSort) values ( NULL, {0}, {1}, {2})",
                group.ID, channel.ID, channel.Sort);
            m_db.Execute(strSQL);
          }
          else
          {
            strSQL = String.Format("update tblGroupMapping set iSort={0} where idGroup={1} and idChannel={2}",
                                   channel.Sort, group.ID, channel.ID);
            m_db.Execute(strSQL);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void DeleteChannelsFromGroup(TVGroup group)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }

          strSQL = String.Format("delete from tblGroupMapping where idGroup={0} ", group.ID);

          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void UnmapChannelFromGroup(TVGroup group, TVChannel channel)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }

          strSQL = String.Format("delete from tblGroupMapping where idGroup={0} and idChannel={1}",
                                 group.ID, channel.ID);

          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static int AddGroup(TVGroup group)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          string strgroupName = group.GroupName;
          DatabaseUtility.RemoveInvalidChars(ref strgroupName);

          if (null == m_db)
          {
            return -1;
          }
          SQLiteResultSet results;
          strSQL = String.Format("select * from tblGroups");
          results = m_db.Execute(strSQL);
          int totalgroups = results.Rows.Count;

          strSQL = String.Format("select * from tblGroups where strName like '{0}'", strgroupName);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            // doesnt exists, add it
            //mjsystem
            if (group.ID > 0)
            {
              strSQL =
                String.Format("insert into tblGroups (idGroup, strName,iSort ,Pincode) values ( {3}, '{0}', {1}, {2})",
                              strgroupName, totalgroups + 1, group.Pincode, group.ID);
            }
            else
            {
              strSQL =
                String.Format("insert into tblGroups (idGroup, strName,iSort ,Pincode) values ( NULL, '{0}', {1}, {2})",
                              strgroupName, totalgroups + 1, group.Pincode);
            }
            m_db.Execute(strSQL);
            int iNewID = m_db.LastInsertID();
            return iNewID;
          }
          else
          {
            //exists, update it
            int iNewID = DatabaseUtility.GetAsInt(results, 0, "idGroup");
            strSQL = String.Format("update tblGroups set strName='{0}', iSort={1}, Pincode={2} where idGroup={3}",
                                   strgroupName, group.Sort, group.Pincode, iNewID);
            results = m_db.Execute(strSQL);
            return iNewID;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }

        return -1;
      }
    }

    public static int DeleteGroup(TVGroup group)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return -1;
          }

          strSQL = String.Format("delete from tblGroupMapping where idGroup={0}", group.ID);
          m_db.Execute(strSQL);
          strSQL = String.Format("delete from tblGroups where idGroup={0}", group.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return -1;
      }
    }

    public static bool GetChannelsForCard(ref ArrayList channels, int card)
    {
      if (m_db == null)
      {
        return false;
      }
      lock (typeof (TVDatabase))
      {
        channels.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL =
            String.Format(
              "select * from channel,tblChannelCard where channel.idChannel=tblChannelCard.idChannel and tblChannelCard.card={0} order by channel.iSort",
              card);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVChannel chan = new TVChannel();
            chan.ID = DatabaseUtility.GetAsInt(results, i, "channel.idChannel");
            chan.Number = DatabaseUtility.GetAsInt(results, i, "channel.iChannelNr");
            decimal dFreq = 0;
            try
            {
              dFreq = (decimal) DatabaseUtility.GetAsInt64(results, i, "channel.frequency");
            }
            catch (Exception)
            {
              chan.Frequency = 0;
            }
            dFreq /= 1000000M;
            dFreq = Math.Round(dFreq, 3);
            dFreq *= 1000000M;
            chan.Frequency = (long) Math.Round(dFreq, 0);
            chan.Name = DatabaseUtility.Get(results, i, "channel.strChannel");
            int iExternal = DatabaseUtility.GetAsInt(results, i, "channel.bExternal");
            if (iExternal != 0)
            {
              chan.External = true;
            }
            else
            {
              chan.External = false;
            }

            int iVisible = DatabaseUtility.GetAsInt(results, i, "channel.Visible");
            if (iVisible != 0)
            {
              chan.VisibleInGuide = true;
            }
            else
            {
              chan.VisibleInGuide = false;
            }

            int grabepg = DatabaseUtility.GetAsInt(results, i, "channel.grabEpg");
            if (grabepg != 0)
            {
              chan.AutoGrabEpg = true;
            }
            else
            {
              chan.AutoGrabEpg = false;
            }

            chan.EpgHours = DatabaseUtility.GetAsInt(results, i, "channel.epgHours");
            chan.LastDateTimeEpgGrabbed =
              Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, i, "channel.epgLastUpdate"));

            chan.ExternalTunerChannel = DatabaseUtility.Get(results, i, "channel.ExternalChannel");
            chan.TVStandard = (AnalogVideoStandard) DatabaseUtility.GetAsInt(results, i, "channel.standard");
            chan.Country = DatabaseUtility.GetAsInt(results, i, "channel.Country");
            chan.Sort = DatabaseUtility.GetAsInt(results, i, "channel.iSort");
            channels.Add(chan);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static void DeleteCanceledSeries(TVRecording rec)
    {
      try
      {
        string strSQL = String.Format("delete from canceledseries where idRecording={0}", rec.ID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public static void AddCanceledSerie(TVRecording rec, long datetime)
    {
      try
      {
        long idChannel = GetChannelId(rec.Channel);
        string strSQL =
          String.Format("insert into canceledseries (idRecording , idChannel,iCancelTime ) values ( {0}, {1},'{2}' )",
                        rec.ID, idChannel, datetime);
        m_db.Execute(strSQL);
        RecordingsChanged(RecordingChange.CanceledSerie);
      }
      catch (Exception ex)
      {
        Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public static void GetCanceledRecordings(ref TVRecording rec)
    {
      rec.CanceledSeries.Clear();
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;
          strSQL = String.Format("select * from canceledseries where idRecording={0}", rec.ID);
          results = m_db.Execute(strSQL);
          for (int x = 0; x < results.Rows.Count; ++x)
          {
            long datetime = DatabaseUtility.GetAsInt64(results, x, "iCancelTime");
            rec.CanceledSeries.Add(datetime);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    } //public static void GetCanceledRecordings(ref TVRecording rec)

    public static int FindFreeTvChannelNumber(int preferenceNumber)
    {
      ArrayList channels = new ArrayList();
      GetChannels(ref channels);
      bool found = false;
      do
      {
        found = false;
        foreach (TVChannel chan in channels)
        {
          if (chan.Number == preferenceNumber)
          {
            found = true;
            preferenceNumber++;
            break;
          }
        }
      } while (found == true);
      return preferenceNumber;
    }

    public static bool DoesChannelExist(int channelId, int TSID, int ONID)
    {
      //check dvbs
      string strSQL =
        String.Format(
          "select * from channel,tblDVBSMapping where tblDVBSMapping.idChannel = channel.idChannel and sTSID={0} and sNetworkID={1} and channel.idChannel={2}",
          TSID, ONID, channelId);
      SQLiteResultSet results;
      results = m_db.Execute(strSQL);
      if (results.Rows.Count > 0)
      {
        return true;
      }

      //check dvbc
      strSQL =
        String.Format(
          "select * from channel,tblDVBCMapping where tblDVBCMapping.iLCN = channel.idChannel  and TSID={0} and ONID={1} and channel.idChannel={2}",
          TSID, ONID, channelId);
      results = m_db.Execute(strSQL);
      if (results.Rows.Count > 0)
      {
        return true;
      }

      //check DVBT
      strSQL =
        String.Format(
          "select * from channel,tblDVBTMapping where tblDVBTMapping.iLCN = channel.idChannel  and TSID={0} and ONID={1} and channel.idChannel={2}",
          TSID, ONID, channelId);
      results = m_db.Execute(strSQL);
      if (results.Rows.Count > 0)
      {
        return true;
      }

      //check ATSC
      strSQL =
        String.Format(
          "select * from channel,tblATSCMapping where tblATSCMapping.iLCN = channel.idChannel  and TSID={0} and ONID={1} and channel.idChannel={2}",
          TSID, ONID, channelId);
      results = m_db.Execute(strSQL);
      if (results.Rows.Count > 0)
      {
        return true;
      }

      return false;
    }

    public static bool DoesChannelHaveAC3(TVChannel chan, bool checkDVBC, bool checkDVBT, bool checkDVBS, bool checkATSC,
                                          out int serviceType)
    {
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      int freq, ONID, TSID, SID, symbolrate, innerFec, modulation, audioPid, videoPid, teletextPid, pmtPid, bandwidth;
      bool HasEITPresentFollow, HasEITSchedule;

      string provider;
      serviceType = 1;

      if (checkDVBT)
      {
        GetDVBTTuneRequest(chan.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid, out videoPid,
                           out teletextPid, out pmtPid, out bandwidth, out audio1, out audio2, out audio3, out ac3Pid,
                           out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3,
                           out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
        if (ac3Pid > 0)
        {
          return true;
        }
      }
      if (checkDVBC)
      {
        GetDVBCTuneRequest(chan.ID, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID,
                           out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,
                           out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2,
                           out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
        serviceType = 1;
        if (ac3Pid > 0)
        {
          return true;
        }
      }
      if (checkDVBS)
      {
        DVBChannel ch = new DVBChannel();
        serviceType = ch.ServiceType;
        if (!GetSatChannel(chan.ID, 1, ref ch))
        {
          GetSatChannel(chan.ID, 1000, ref ch);
          serviceType = 1000;
        }
        if (ch.AC3Pid > 0)
        {
          return true;
        }
      }
      if (checkATSC)
      {
        int physical, minor, major;
        GetATSCTuneRequest(chan.ID, out physical, out provider, out freq, out symbolrate, out innerFec, out modulation,
                           out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid,
                           out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1,
                           out audioLanguage2, out audioLanguage3, out minor, out major, out HasEITPresentFollow,
                           out HasEITSchedule, out pcrPid);
        serviceType = 1;
        if (ac3Pid > 0)
        {
          return true;
        }
      }
      return false;
    }

    public static void DeleteNotify(TVNotify notify)
    {
      lock (typeof (TVDatabase))
      {
        if (null == m_db)
        {
          return;
        }
        try
        {
          string strSQL = String.Format("delete from tblNotifies where idNotify={0}", notify.ID);
          m_db.Execute(strSQL);
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      if (OnNotifiesChanged != null)
      {
        OnNotifiesChanged();
      }
    }

    public static void AddNotify(TVNotify notify)
    {
      lock (typeof (TVDatabase))
      {
        if (null == m_db)
        {
          return;
        }
        try
        {
          string strSQL = String.Format("insert into tblNotifies (idNotify,idProgram) Values (NULL,{0})",
                                        notify.Program.ID);
          m_db.Execute(strSQL);
          notify.ID = m_db.LastInsertID();
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
      if (OnNotifiesChanged != null)
      {
        OnNotifiesChanged();
      }
    }

    public static void GetNotifies(ArrayList notifies, bool complete)
    {
      notifies.Clear();
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;

          if (complete)
          {
            strSQL =
              String.Format(
                "select * from tblNotifies,tblPrograms,channel,genre where tblPrograms.idProgram=tblNotifies.idProgram and channel.idChannel=tblPrograms.idChannel and tblPrograms.idGenre=genre.idGenre");
          }
          else
          {
            strSQL = String.Format("select * from tblNotifies");
          }

          results = m_db.Execute(strSQL);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVNotify notify = new TVNotify();
            notify.Program = new TVProgram();
            TVProgram prog = new TVProgram();
            if (complete)
            {
              notify.Program.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
              long iStart = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iStartTime");
              long iEnd = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iEndTime");
              notify.Program.Start = iStart;
              notify.Program.End = iEnd;
              notify.Program.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
              notify.Program.Title = DatabaseUtility.Get(results, i, "tblPrograms.strTitle");
              notify.Program.Description = DatabaseUtility.Get(results, i, "tblPrograms.strDescription");
              notify.Program.Episode = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeName");
              notify.Program.Repeat = DatabaseUtility.Get(results, i, "tblPrograms.strRepeat");
              notify.Program.SeriesNum = DatabaseUtility.Get(results, i, "tblPrograms.strSeriesNum");
              notify.Program.EpisodeNum = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeNum");
              notify.Program.EpisodePart = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodePart");
              notify.Program.Date = DatabaseUtility.Get(results, i, "tblPrograms.strDate");
              notify.Program.StarRating = DatabaseUtility.Get(results, i, "tblPrograms.strStarRating");
              notify.Program.Classification = DatabaseUtility.Get(results, i, "tblPrograms.strClassification");
              notify.Program.ID = DatabaseUtility.GetAsInt(results, i, "tblNotifies.idProgram");
              notify.ID = DatabaseUtility.GetAsInt(results, i, "tblNotifies.idNotify");
            }
            else
            {
              notify.Program.ID = DatabaseUtility.GetAsInt(results, i, "idProgram");
              notify.ID = DatabaseUtility.GetAsInt(results, i, "idNotify");
            }
            notifies.Add(notify);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static void GetNotify(TVNotify notify)
    {
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;

          strSQL =
            String.Format(
              "select * from channel,genre,tblPrograms,tblNotifies where channel.idChannel=tblPrograms.idChannel and tblPrograms.idGenre=genre.idGenre and tblPrograms.idProgram=tblNotifies.idProgram and tblNotifies.idNotify={0}",
              notify.ID);
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 1)
          {
            notify.Program = new TVProgram();
            TVProgram prog = new TVProgram();
            notify.Program.Channel = DatabaseUtility.Get(results, 0, "channel.strChannel");
            long iStart = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iEndTime");
            notify.Program.Start = iStart;
            notify.Program.End = iEnd;
            notify.Program.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
            notify.Program.Title = DatabaseUtility.Get(results, 0, "tblPrograms.strTitle");
            notify.Program.Description = DatabaseUtility.Get(results, 0, "tblPrograms.strDescription");
            notify.Program.Episode = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeName");
            notify.Program.Repeat = DatabaseUtility.Get(results, 0, "tblPrograms.strRepeat");
            notify.Program.SeriesNum = DatabaseUtility.Get(results, 0, "tblPrograms.strSeriesNum");
            notify.Program.EpisodeNum = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeNum");
            notify.Program.EpisodePart = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodePart");
            notify.Program.Date = DatabaseUtility.Get(results, 0, "tblPrograms.strDate");
            notify.Program.StarRating = DatabaseUtility.Get(results, 0, "tblPrograms.strStarRating");
            notify.Program.Classification = DatabaseUtility.Get(results, 0, "tblPrograms.strClassification");
            notify.Program.ID = DatabaseUtility.GetAsInt(results, 0, "tblNotifies.idProgram");
            notify.ID = DatabaseUtility.GetAsInt(results, 0, "tblNotifies.idNotify");
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static TVChannel GetTVChannelByStream(bool atsc, bool dvbt, bool dvbc, bool dvbs, int networkid,
                                                 int transportid, int serviceid, out string provider)
    {
      int freq, symbolrate, innerFec, modulation, ONID, TSID, SID;
      int audioPid, videoPid, teletextPid, pmtPid, bandWidth;
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      bool HasEITPresentFollow, HasEITSchedule;
      provider = "";

      DVBChannel ch = new DVBChannel();
      List<TVChannel> channels = new List<TVChannel>();
      GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        if (dvbc)
        {
          GetDVBCTuneRequest(chan.ID, out provider, out freq, out symbolrate, out innerFec, out modulation, out ONID,
                             out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,
                             out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1,
                             out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule,
                             out pcrPid);
          //if (serviceid == SID && transportid == TSID) return chan;
          if (serviceid == SID && transportid == TSID && networkid == ONID)
            //check if networkid has changed during channel update
          {
            return chan;
          }
        }
        if (dvbs)
        {
          GetSatChannel(chan.ID, 1, ref ch);
          if (ch.TransportStreamID == transportid && ch.ProgramNumber == serviceid)
          {
            provider = ch.ServiceProvider;
            return chan;
          }
        }

        if (dvbt)
        {
          GetDVBTTuneRequest(chan.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid, out videoPid,
                             out teletextPid, out pmtPid, out bandWidth, out audio1, out audio2, out audio3, out ac3Pid,
                             out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3,
                             out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
          //if (serviceid == SID && transportid == TSID) return chan;
          if (serviceid == SID && transportid == TSID && networkid == ONID)
            // forums patch: http://forum.team-mediaportal.com/viewtopic.php?t=13551
          {
            return chan;
          }
        }
      }
      provider = "";
      return null;
    }

    public static bool IsMapped(TVChannel channel, bool atsc, bool dvbt, bool dvbc, bool dvbs, out string provider)
    {
      int freq, symbolrate, innerFec, modulation, ONID, TSID, SID;
      int audioPid, videoPid, teletextPid, pmtPid, bandWidth;
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      bool HasEITPresentFollow, HasEITSchedule;
      provider = "";

      DVBChannel ch = new DVBChannel();
      if (dvbc)
      {
        if (GetDVBCTuneRequest(channel.ID, out provider, out freq, out symbolrate, out innerFec, out modulation,
                               out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid,
                               out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1,
                               out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule,
                               out pcrPid))
        {
          return true;
        }
      }
      if (dvbs)
      {
        if (GetSatChannel(channel.ID, 1, ref ch))
        {
          provider = ch.ServiceProvider;
          return true;
        }
      }

      if (dvbt)
      {
        if (GetDVBTTuneRequest(channel.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid,
                               out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1, out audio2,
                               out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2,
                               out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid))
        {
          return true;
        }
      }
      return false;
    }

    public static bool IsDigitalChannel(TVChannel channel)
    {
      int freq, symbolrate, innerFec, modulation, ONID, TSID, SID;
      int audioPid, videoPid, teletextPid, pmtPid, bandWidth;
      int audio1, audio2, audio3, ac3Pid, pcrPid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      bool HasEITPresentFollow, HasEITSchedule;
      string provider = "";
      bool result = GetDVBCTuneRequest(channel.ID, out provider, out freq, out symbolrate, out innerFec, out modulation,
                                       out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid,
                                       out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage,
                                       out audioLanguage1, out audioLanguage2, out audioLanguage3,
                                       out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
      if (result)
      {
        return true;
      }

      DVBChannel ch = new DVBChannel();
      result = GetSatChannel(channel.ID, 1, ref ch);
      if (result)
      {
        return true;
      }

      result = GetDVBTTuneRequest(channel.ID, out provider, out freq, out ONID, out TSID, out SID, out audioPid,
                                  out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1, out audio2,
                                  out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2,
                                  out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
      if (result)
      {
        return true;
      }

      return false;
    }

    public static TVProgram GetProgramByTime(string channelName, DateTime dtTime)
    {
      lock (typeof (TVDatabase))
      {
        int idChannel = GetChannelId(channelName);
        if (idChannel < 0)
        {
          return null;
        }
        string strSQL;
        if (null == m_db)
        {
          return null;
        }
        strSQL =
          String.Format(
            "select * from tblPrograms where tblPrograms.idChannel={0} and '{1}' >= tblPrograms.iStartTime and '{2}' < tblPrograms.iEndTime ",
            idChannel, Util.Utils.datetolong(dtTime), Util.Utils.datetolong(dtTime));
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count >= 1)
        {
          TVProgram prog = new TVProgram();


          long iStart = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iStartTime");
          long iEnd = DatabaseUtility.GetAsInt64(results, 0, "tblPrograms.iEndTime");
          prog.Channel = channelName;
          prog.Start = iStart;
          prog.End = iEnd;
          prog.Genre = GetGenreById(DatabaseUtility.GetAsInt(results, 0, "tblPrograms.idGenre"));
          prog.Title = DatabaseUtility.Get(results, 0, "tblPrograms.strTitle");
          prog.Description = DatabaseUtility.Get(results, 0, "tblPrograms.strDescription");
          prog.Episode = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeName");
          prog.Repeat = DatabaseUtility.Get(results, 0, "tblPrograms.strRepeat");
          prog.ID = DatabaseUtility.GetAsInt(results, 0, "tblPrograms.idProgram");
          prog.SeriesNum = DatabaseUtility.Get(results, 0, "tblPrograms.strSeriesNum");
          prog.EpisodeNum = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodeNum");
          prog.EpisodePart = DatabaseUtility.Get(results, 0, "tblPrograms.strEpisodePart");
          prog.Date = DatabaseUtility.Get(results, 0, "tblPrograms.strDate");
          prog.StarRating = DatabaseUtility.Get(results, 0, "tblPrograms.strStarRating");
          prog.Classification = DatabaseUtility.Get(results, 0, "tblPrograms.strClassification");
          return prog;
        }
        else
        {
          return null;
        }
      }
    }

    public static void ReMapDigitalMapping(int fromChannelId, int toChannelId)
    {
      string sql;
      sql = String.Format("update tblATSCMapping set iLCN={0} where tblATSCMapping.iLCN={1}", toChannelId, fromChannelId);
      m_db.Execute(sql);

      sql = String.Format("update tblDVBTMapping set iLCN={0} where tblDVBTMapping.iLCN={1}", toChannelId, fromChannelId);
      m_db.Execute(sql);

      sql = String.Format("update tblDVBCMapping set iLCN={0} where tblDVBCMapping.iLCN={1}", toChannelId, fromChannelId);
      m_db.Execute(sql);

      sql = String.Format("update tblDVBSMapping set idChannel={0} where tblDVBSMapping.idChannel={1}", toChannelId,
                          fromChannelId);
      m_db.Execute(sql);
    }

    public static void DeleteAllRecordedTv()
    {
      m_db.Execute("delete from recorded");
    }

    public static void UpdatePids(bool isATSC, bool isDVBC, bool isDVBS, bool isDVBT, DVBChannel dvbChannel)
    {
      string provider;
      TVChannel channel = GetTVChannelByStream(isATSC, isDVBT, isDVBC, isDVBS, dvbChannel.NetworkID,
                                               dvbChannel.TransportStreamID, dvbChannel.ProgramNumber, out provider);
      if (channel == null)
      {
        Log.Info("tvdatabase cannot get channel for :{0}", dvbChannel.ServiceName);
        return;
      }
      if (isDVBC)
      {
        Log.Info("update dvbc channel:{0}", channel.Name);
        MapDVBCChannel(channel.Name, dvbChannel.ServiceProvider, channel.ID, dvbChannel.Frequency,
                       dvbChannel.Symbolrate, dvbChannel.FEC, dvbChannel.Modulation, dvbChannel.NetworkID,
                       dvbChannel.TransportStreamID, dvbChannel.ProgramNumber,
                       dvbChannel.AudioPid, dvbChannel.VideoPid, dvbChannel.TeletextPid,
                       dvbChannel.PMTPid, dvbChannel.Audio1, dvbChannel.Audio2, dvbChannel.Audio3,
                       dvbChannel.AC3Pid, dvbChannel.PCRPid, dvbChannel.AudioLanguage,
                       dvbChannel.AudioLanguage1, dvbChannel.AudioLanguage2, dvbChannel.AudioLanguage3,
                       dvbChannel.HasEITPresentFollow, dvbChannel.HasEITSchedule);
      }
      if (isDVBT)
      {
        Log.Info("update dvbt channel:{0}", channel.Name);
        MapDVBTChannel(channel.Name, dvbChannel.ServiceProvider, channel.ID, dvbChannel.Frequency,
                       dvbChannel.NetworkID, dvbChannel.TransportStreamID, dvbChannel.ProgramNumber,
                       dvbChannel.AudioPid, dvbChannel.VideoPid, dvbChannel.TeletextPid,
                       dvbChannel.PMTPid, dvbChannel.Bandwidth, dvbChannel.Audio1, dvbChannel.Audio2,
                       dvbChannel.Audio3, dvbChannel.AC3Pid, dvbChannel.PCRPid, dvbChannel.AudioLanguage,
                       dvbChannel.AudioLanguage1, dvbChannel.AudioLanguage2, dvbChannel.AudioLanguage3,
                       dvbChannel.HasEITPresentFollow, dvbChannel.HasEITSchedule);
      }
      if (isDVBS)
      {
        Log.Info("update dvbs channel:{0}", channel.Name);
        UpdateSatChannel(channel.ID, dvbChannel);
      }
      if (isATSC)
      {
        Log.Info("update atsc channel:{0}", channel.Name);
        MapATSCChannel(channel.Name, dvbChannel.PhysicalChannel, dvbChannel.MinorChannel, dvbChannel.MajorChannel,
                       dvbChannel.ServiceProvider, channel.ID, dvbChannel.Frequency, dvbChannel.Symbolrate,
                       dvbChannel.FEC,
                       dvbChannel.Modulation, dvbChannel.NetworkID, dvbChannel.TransportStreamID,
                       dvbChannel.ProgramNumber,
                       dvbChannel.AudioPid, dvbChannel.VideoPid, dvbChannel.TeletextPid, dvbChannel.PMTPid,
                       dvbChannel.Audio1, dvbChannel.Audio2, dvbChannel.Audio3, dvbChannel.AC3Pid,
                       dvbChannel.PCRPid, dvbChannel.AudioLanguage, dvbChannel.AudioLanguage1, dvbChannel.AudioLanguage2,
                       dvbChannel.AudioLanguage3, dvbChannel.HasEITPresentFollow, dvbChannel.HasEITSchedule);
      }
    }

    #region generics methods

    public static bool GetChannels(ref List<TVChannel> channels)
    {
      if (m_db == null)
      {
        return false;
      }
      lock (typeof (TVDatabase))
      {
        m_channelCache.Clear();
        channels.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL = String.Format("select * from channel order by iSort");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVChannel chan = new TVChannel();
            chan.ID = DatabaseUtility.GetAsInt(results, i, "idChannel");
            chan.Number = DatabaseUtility.GetAsInt(results, i, "iChannelNr");
            decimal dFreq = 0;
            try
            {
              dFreq = (decimal) DatabaseUtility.GetAsInt64(results, i, "frequency");
            }
            catch (Exception)
            {
              chan.Frequency = 0;
            }
            dFreq /= 1000000M;
            dFreq = Math.Round(dFreq, 3);
            dFreq *= 1000000M;
            chan.Frequency = (long) Math.Round(dFreq, 0);
            chan.Name = DatabaseUtility.Get(results, i, "strChannel");
            int iExternal = DatabaseUtility.GetAsInt(results, i, "bExternal");
            if (iExternal != 0)
            {
              chan.External = true;
            }
            else
            {
              chan.External = false;
            }

            int iVisible = DatabaseUtility.GetAsInt(results, i, "Visible");
            if (iVisible != 0)
            {
              chan.VisibleInGuide = true;
            }
            else
            {
              chan.VisibleInGuide = false;
            }


            int scrambled = DatabaseUtility.GetAsInt(results, i, "scrambled");
            if (scrambled != 0)
            {
              chan.Scrambled = true;
            }
            else
            {
              chan.Scrambled = false;
            }
            chan.Sort = DatabaseUtility.GetAsInt(results, i, "iSort");

            int grabepg = DatabaseUtility.GetAsInt(results, i, "grabEpg");
            if (grabepg != 0)
            {
              chan.AutoGrabEpg = true;
            }
            else
            {
              chan.AutoGrabEpg = false;
            }

            chan.EpgHours = DatabaseUtility.GetAsInt(results, i, "epgHours");
            chan.LastDateTimeEpgGrabbed = Util.Utils.longtodate(DatabaseUtility.GetAsInt64(results, i, "epgLastUpdate"));

            chan.ExternalTunerChannel = DatabaseUtility.Get(results, i, "ExternalChannel");
            chan.TVStandard = (AnalogVideoStandard) DatabaseUtility.GetAsInt(results, i, "standard");
            chan.Country = DatabaseUtility.GetAsInt(results, i, "Country");
            channels.Add(chan);

            CachedChannel ch = new CachedChannel(chan.ID, chan.Number, chan.Name);
            m_channelCache.Add(ch);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static void GetNotifies(List<TVNotify> notifies, bool complete)
    {
      notifies.Clear();
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;

          if (complete)
          {
            strSQL =
              String.Format(
                "select * from tblNotifies,tblPrograms,channel,genre where tblPrograms.idProgram=tblNotifies.idProgram and channel.idChannel=tblPrograms.idChannel and tblPrograms.idGenre=genre.idGenre");
          }
          else
          {
            strSQL = String.Format("select * from tblNotifies");
          }

          results = m_db.Execute(strSQL);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVNotify notify = new TVNotify();
            notify.Program = new TVProgram();
            TVProgram prog = new TVProgram();
            if (complete)
            {
              notify.Program.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
              long iStart = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iStartTime");
              long iEnd = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iEndTime");
              notify.Program.Start = iStart;
              notify.Program.End = iEnd;
              notify.Program.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
              notify.Program.Title = DatabaseUtility.Get(results, i, "tblPrograms.strTitle");
              notify.Program.Description = DatabaseUtility.Get(results, i, "tblPrograms.strDescription");
              notify.Program.Episode = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeName");
              notify.Program.Repeat = DatabaseUtility.Get(results, i, "tblPrograms.strRepeat");
              notify.Program.SeriesNum = DatabaseUtility.Get(results, i, "tblPrograms.strSeriesNum");
              notify.Program.EpisodeNum = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodeNum");
              notify.Program.EpisodePart = DatabaseUtility.Get(results, i, "tblPrograms.strEpisodePart");
              notify.Program.Date = DatabaseUtility.Get(results, i, "tblPrograms.strDate");
              notify.Program.StarRating = DatabaseUtility.Get(results, i, "tblPrograms.strStarRating");
              notify.Program.Classification = DatabaseUtility.Get(results, i, "tblPrograms.strClassification");
              notify.Program.ID = DatabaseUtility.GetAsInt(results, i, "tblNotifies.idProgram");
              notify.ID = DatabaseUtility.GetAsInt(results, i, "tblNotifies.idNotify");
            }
            else
            {
              notify.Program.ID = DatabaseUtility.GetAsInt(results, i, "idProgram");
              notify.ID = DatabaseUtility.GetAsInt(results, i, "idNotify");
            }
            notifies.Add(notify);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static bool GetPrograms(long iStartTime, long iEndTime, ref List<TVProgram> progs)
    {
      return SearchPrograms(iStartTime, iEndTime, ref progs, -1, string.Empty, string.Empty);
    }

    public static bool SearchPrograms(long iStartTime, long iEndTime, ref List<TVProgram> progs, int SearchKind,
                                      string SearchCriteria, string channel)
    {
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel ");
          break;
        case 0:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}%' ",
              SearchCriteria);
          break;
        case 1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}%' ",
              SearchCriteria);
          break;
        case 2:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}' ",
              SearchCriteria);
          break;
        case 3:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}' ",
              SearchCriteria);
          break;
      }
      if (channel != string.Empty)
      {
        DatabaseUtility.RemoveInvalidChars(ref channel);
        strSQL += String.Format(" and channel.strChannel like '{0}' ", channel);
      }
      return GetTVPrograms(iStartTime, iEndTime, strSQL, strOrder, ref progs);
    }

    public static bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime,
                                             ref List<TVProgram> progs)
    {
      lock (typeof (TVDatabase))
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
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and channel.strChannel like '{0}' ",
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
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
              prog.Start = iStart;
              prog.End = iEnd;
              prog.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
              prog.Title = DatabaseUtility.Get(results, i, "tblPrograms.strTitle");
              prog.Description = DatabaseUtility.Get(results, i, "tblPrograms.strDescription");
              if (prog.Description == Strings.Unknown)
              {
                prog.Description = string.Empty;
              }
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
          Log.Error("TVDatabase exception err:{0} stack:{1} {2}", ex.Message, ex.StackTrace, strSQL);
          Open();
        }
        return false;
      }
    }

    public static bool GetRecordedTV(ref List<TVRecorded> recordings)
    {
      lock (typeof (TVDatabase))
      {
        recordings.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
          strSQL =
            String.Format(
              "select * from channel,genre,recorded where recorded.idChannel=channel.idChannel and genre.idGenre=recorded.idGenre order by iStartTime");
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "recorded.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "recorded.iEndTime");
            TVRecorded rec = new TVRecorded();
            rec.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
            rec.Start = iStart;
            rec.End = iEnd;
            rec.ID = DatabaseUtility.GetAsInt(results, i, "recorded.idRecorded");
            rec.Title = DatabaseUtility.Get(results, i, "recorded.strProgram");
            rec.Description = DatabaseUtility.Get(results, i, "recorded.strDescription");
            rec.FileName = DatabaseUtility.Get(results, i, "recorded.strFileName");
            rec.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
            rec.Played = DatabaseUtility.GetAsInt(results, i, "recorded.iPlayed");
            rec.KeepRecordingMethod =
              (TVRecorded.KeepMethod) DatabaseUtility.GetAsInt(results, i, "recorded.keepMethod");
            long date = DatabaseUtility.GetAsInt64(results, i, "recorded.keepDate");
            rec.KeepRecordingTill = Util.Utils.longtodate(date);
            rec.RecordedCardIndex = DatabaseUtility.GetAsInt(results, i, "recorded.idCard");
            recordings.Add(rec);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static bool GetGenres(ref List<string> genres)
    {
      lock (typeof (TVDatabase))
      {
        genres.Clear();
        m_genreCache.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL;
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
            m_genreCache.Add(new CachedGenre(id, genre));
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    public static void GetGroups(ref List<TVGroup> groups)
    {
      groups.Clear();
      lock (typeof (TVDatabase))
      {
        string strSQL;
        try
        {
          if (null == m_db)
          {
            return;
          }
          SQLiteResultSet results;
          strSQL = String.Format("select * from tblGroups order by iSort");
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return;
          }
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVGroup group = new TVGroup();
            group.ID = DatabaseUtility.GetAsInt(results, i, "idGroup");
            group.Sort = DatabaseUtility.GetAsInt(results, i, "iSort");
            group.Pincode = DatabaseUtility.GetAsInt(results, i, "Pincode");
            group.GroupName = DatabaseUtility.Get(results, i, "strName");
            groups.Add(group);
          }
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
      }
    }

    public static bool SearchMinimalPrograms(long iStartTime, long iEndTime, ref List<TVProgram> progs, int SearchKind,
                                             string SearchCriteria, string channel)
    {
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms  where tblPrograms.idChannel=channel.idChannel ");
          break;
        case 0:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms  where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}%' ",
              SearchCriteria);
          break;
        case 1:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}%' ",
              SearchCriteria);
          break;
        case 2:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '%{0}' ",
              SearchCriteria);
          break;
        case 3:
          strSQL =
            String.Format(
              "select channel.strChannel,tblPrograms.strTitle,tblPrograms.iStartTime,tblPrograms.iEndTime from channel,tblPrograms where tblPrograms.idChannel=channel.idChannel and tblPrograms.strTitle like '{0}' ",
              SearchCriteria);
          break;
      }
      if (channel != string.Empty)
      {
        DatabaseUtility.RemoveInvalidChars(ref channel);
        strSQL += String.Format(" and channel.strChannel like '{0}' ", channel);
      }
      return GetMinimalPrograms(iStartTime, iEndTime, strSQL, strOrder, ref progs);
    }

    public static bool SearchProgramsByDescription(long iStartTime, long iEndTime, ref List<TVProgram> progs,
                                                   int SearchKind, string SearchCriteria)
    {
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel  ");
          break;
        case 0:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}%' ",
              SearchCriteria);
          break;
        case 1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}%' ",
              SearchCriteria);
          break;
        case 2:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '%{0}' ",
              SearchCriteria);
          break;
        case 3:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and tblPrograms.strDescription like '{0}' ",
              SearchCriteria);
          break;
      }
      return GetTVPrograms(iStartTime, iEndTime, strSQL, strOrder, ref progs);
    }

    public static bool SearchProgramsPerGenre(string genre1, List<TVProgram> progs, int SearchKind,
                                              string SearchCriteria)
    {
      progs.Clear();
      if (genre1 == null)
      {
        return false;
      }
      string genre = genre1;
      DatabaseUtility.RemoveInvalidChars(ref genre);
      DatabaseUtility.RemoveInvalidChars(ref SearchCriteria);
      string strSQL = string.Empty;
      string strOrder = " order by iStartTime";
      switch (SearchKind)
      {
        case -1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' ",
              genre);
          break;
        case 0:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}%' ",
              genre, SearchCriteria);
          break;

        case 1:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}%' ",
              genre, SearchCriteria);
          break;

        case 2:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '%{1}' ",
              genre, SearchCriteria);
          break;

        case 3:
          strSQL =
            String.Format(
              "select * from channel,tblPrograms,genre where genre.idGenre=tblPrograms.idGenre and tblPrograms.idChannel=channel.idChannel and genre.strGenre like '{0}' and tblPrograms.strTitle like '{1}' ",
              genre, SearchCriteria);
          break;
      }
      return GetTVProgramsByGenre(strSQL, strOrder, progs);
    }

    public static bool GetProgramsPerGenre(string genre1, List<TVProgram> progs)
    {
      return SearchProgramsPerGenre(genre1, progs, -1, string.Empty);
    }

    public static bool GetProgramTitles(long iStartTime, long iEndTime, ref List<TVProgram> progs)
    {
      lock (typeof (TVDatabase))
      {
        progs.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }
          string strSQL =
            "select distinct strtitle,strchannel from tblprograms,channel where tblprograms.idchannel=channel.idchannel ";

          SQLiteResultSet results;
          string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                       iStartTime, iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime>='{0}' and tblPrograms.iStartTime <= '{1}' ) ", iStartTime,
                                 iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime<='{0}' and tblPrograms.iEndTime >= '{1}') )", iStartTime,
                                 iEndTime);
          strSQL += where;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }

          for (int i = 0; i < results.Rows.Count; ++i)
          {
            TVProgram prog = new TVProgram();
            prog.Channel = DatabaseUtility.Get(results, i, "strchannel");
            prog.Title = DatabaseUtility.Get(results, i, "strtitle");
            prog.ID = DatabaseUtility.GetAsInt(results, i, "tblPrograms.idProgram");
            progs.Add(prog);
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    #endregion

    #endregion

    #region Private Methods

    private static void Open()
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          // Open database
          Log.Info("opening tvdatabase");

          try
          {
            Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
          }
          catch (Exception)
          {
          }
          //Upgrade();
          m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "TVDatabaseV21.db3"));
          if (m_db != null)
          {
            DatabaseUtility.SetPragmas(m_db);
          }
          UpdateFromPreviousVersion();
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
        Log.Info("tvdatabase opened");
      }
    }

    /// <summary>
    /// Checks if tables already exists in the database, if not creates the missing tables
    /// </summary>
    /// <returns>true : tables created</returns>
    private static bool CreateTables()
    {
      lock (typeof (TVDatabase))
      {
        if (m_db == null)
        {
          return false;
        }
        if (DatabaseUtility.AddTable(m_db, "channel",
                                     "CREATE TABLE channel ( idChannel integer primary key, strChannel text, iChannelNr integer, frequency text, iSort integer, bExternal integer, ExternalChannel text, standard integer, Visible integer, Country integer, scrambled integer)"))
        {
          try
          {
            m_db.Execute("CREATE INDEX idxChannel ON channel(iChannelNr)");
          }
          catch (Exception)
          {
          }
        }

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

        DatabaseUtility.AddTable(m_db, "recording",
                                 "CREATE TABLE recording ( idRecording integer primary key, idChannel integer, iRecordingType integer, strProgram text, iStartTime integer, iEndTime integer, iCancelTime integer, bContentRecording integer, priority integer, quality integer, episodesToKeep integer)");
        DatabaseUtility.AddTable(m_db, "canceledseries",
                                 "CREATE TABLE canceledseries ( idRecording integer, idChannel integer, iCancelTime text)");

        DatabaseUtility.AddTable(m_db, "recorded",
                                 "CREATE TABLE recorded ( idRecorded integer primary key, idChannel integer, idGenre integer, strProgram text, iStartTime integer, iEndTime integer, strDescription text, strFileName text, iPlayed integer, idCard integer)");

        DatabaseUtility.AddTable(m_db, "tblDVBSMapping",
                                 "CREATE TABLE tblDVBSMapping ( idChannel integer,sPCRPid integer,sTSID integer,sFreq integer,sSymbrate integer,sFEC integer,sLNBKhz integer,sDiseqc integer,sProgramNumber integer,sServiceType integer,sProviderName text,sChannelName text,sEitSched integer,sEitPreFol integer,sAudioPid integer,sVideoPid integer,sAC3Pid integer,sAudio1Pid integer,sAudio2Pid integer,sAudio3Pid integer,sTeletextPid integer,sScrambled integer,sPol integer,sLNBFreq integer,sNetworkID integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text,sECMPid integer,sPMTPid integer)");
        DatabaseUtility.AddTable(m_db, "tblDVBCMapping",
                                 "CREATE TABLE tblDVBCMapping ( idChannel integer primary key, strChannel text, strProvider text, iLCN integer, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer, pmtPid integer, ac3Pid integer, audio1Pid integer, audio2Pid integer, audio3Pid integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text, HasEITPresentFollow integer, HasEITSchedule integer)");
        DatabaseUtility.AddTable(m_db, "tblATSCMapping",
                                 "CREATE TABLE tblATSCMapping ( idChannel integer primary key, strChannel text, strProvider text, iLCN integer, frequency text, symbolrate integer, innerFec integer, modulation integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer, pmtPid integer, ac3Pid integer, audio1Pid integer, audio2Pid integer, audio3Pid integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text, channelNumber integer,minorChannel integer, majorChannel integer, HasEITPresentFollow integer, HasEITSchedule integer)");
        DatabaseUtility.AddTable(m_db, "tblDVBTMapping",
                                 "CREATE TABLE tblDVBTMapping ( idChannel integer primary key, strChannel text, strProvider text, iLCN integer, frequency text, bandwidth integer, ONID integer, TSID integer, SID integer, Visible integer, audioPid integer, videoPid integer, teletextPid integer, pmtPid integer, ac3Pid integer, audio1Pid integer, audio2Pid integer, audio3Pid integer,sAudioLang text,sAudioLang1 text,sAudioLang2 text,sAudioLang3 text, HasEITPresentFollow integer, HasEITSchedule integer)");
        DatabaseUtility.AddTable(m_db, "tblGroups",
                                 "CREATE TABLE tblGroups ( idGroup integer primary key, strName text, iSort integer, Pincode integer)");
        DatabaseUtility.AddTable(m_db, "tblGroupMapping",
                                 "CREATE TABLE tblGroupMapping( idGroupMapping integer primary key, idGroup integer, idChannel integer)");

        //following table specifies which channels can be received by which card
        DatabaseUtility.AddTable(m_db, "tblChannelCard",
                                 "CREATE TABLE tblChannelCard( idChannelCard integer primary key, idChannel integer, card integer)");
        DatabaseUtility.AddTable(m_db, "tblNotifies",
                                 "CREATE TABLE tblNotifies( idNotify integer primary key, idProgram integer)");

        //xmltv->tv channel mapping
        DatabaseUtility.AddTable(m_db, "tblEPGMapping",
                                 "CREATE TABLE tblEPGMapping ( idChannel integer primary key, strChannel text, xmltvid text)");

        return true;
      }
    }

    private static string GetGenreById(int idGenre)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return string.Empty;
          }
          foreach (CachedGenre genre in m_genreCache)
          {
            if (genre.idGenre == idGenre)
            {
              return genre.strGenre;
            }
          }
          string strSQL;
          strSQL = String.Format("select * from genre where idGenre={0}", idGenre);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return string.Empty;
          }
          string genreLabel = DatabaseUtility.Get(results, 0, "strGenre");

          m_genreCache.Add(new CachedGenre(idGenre, genreLabel));
          return genreLabel;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return string.Empty;
      }
    }

    private static bool GetTVProgramsByGenre(string strSQL, string strOrder, ArrayList progs)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }

          strSQL += strOrder;
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          long lTimeStart = Util.Utils.datetolong(DateTime.Now);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iEndTime");
            bool bAdd = false;
            if (iEnd >= lTimeStart)
            {
              bAdd = true;
            }
            if (bAdd)
            {
              TVProgram prog = new TVProgram();
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    private static void ProgramsChanged()
    {
      m_bProgramsChanged = true;
      if (!m_bSupressEvents)
      {
        if (OnProgramsChanged != null)
        {
          OnProgramsChanged();
        }
        m_bProgramsChanged = false;
      }
    }

    private static void RecordingsChanged(RecordingChange changeType)
    {
      m_bRecordingsChanged = true;
      if (!m_bSupressEvents)
      {
        if (OnRecordingsChanged != null)
        {
          OnRecordingsChanged(changeType);
        }
        m_bRecordingsChanged = false;
      }
    }

    private static bool GetMinimalPrograms(long iStartTime, long iEndTime, string strSQL, string strOrder,
                                           ref ArrayList progs)
    {
      lock (typeof (TVDatabase))
      {
        progs.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }

          SQLiteResultSet results;
          string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                       iStartTime, iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime>='{0}' and tblPrograms.iStartTime <= '{1}' ) ", iStartTime,
                                 iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime<='{0}' and tblPrograms.iEndTime >= '{1}') )", iStartTime,
                                 iEndTime);
          strSQL += where;
          strSQL += strOrder;
          results = m_db.Execute(strSQL);
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
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
              prog.Start = iStart;
              prog.End = iEnd;
              prog.Title = DatabaseUtility.Get(results, i, "tblPrograms.strTitle");
              progs.Add(prog);
            }
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    private static bool GetTVPrograms(long iStartTime, long iEndTime, string strSQL, string strOrder,
                                      ref ArrayList progs)
    {
      lock (typeof (TVDatabase))
      {
        progs.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }

          SQLiteResultSet results;
          string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                       iStartTime, iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime>='{0}' and tblPrograms.iStartTime <= '{1}' ) ", iStartTime,
                                 iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime<='{0}' and tblPrograms.iEndTime >= '{1}') )", iStartTime,
                                 iEndTime);
          strSQL += where;
          strSQL += strOrder;
          results = m_db.Execute(strSQL);
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
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    #region generics methods

    private static bool GetTVProgramsByGenre(string strSQL, string strOrder, List<TVProgram> progs)
    {
      lock (typeof (TVDatabase))
      {
        try
        {
          if (null == m_db)
          {
            return false;
          }

          strSQL += strOrder;
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          if (results.Rows.Count == 0)
          {
            return false;
          }
          long lTimeStart = Util.Utils.datetolong(DateTime.Now);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            long iStart = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iStartTime");
            long iEnd = DatabaseUtility.GetAsInt64(results, i, "tblPrograms.iEndTime");
            bool bAdd = false;
            if (iEnd >= lTimeStart)
            {
              bAdd = true;
            }
            if (bAdd)
            {
              TVProgram prog = new TVProgram();
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    private static bool GetMinimalPrograms(long iStartTime, long iEndTime, string strSQL, string strOrder,
                                           ref List<TVProgram> progs)
    {
      lock (typeof (TVDatabase))
      {
        progs.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }

          SQLiteResultSet results;
          string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                       iStartTime, iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime>='{0}' and tblPrograms.iStartTime <= '{1}' ) ", iStartTime,
                                 iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime<='{0}' and tblPrograms.iEndTime >= '{1}') )", iStartTime,
                                 iEndTime);
          strSQL += where;
          strSQL += strOrder;
          results = m_db.Execute(strSQL);
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
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
              prog.Start = iStart;
              prog.End = iEnd;
              prog.Title = DatabaseUtility.Get(results, i, "tblPrograms.strTitle");
              progs.Add(prog);
            }
          }

          return true;
        }
        catch (Exception ex)
        {
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    private static bool GetTVPrograms(long iStartTime, long iEndTime, string strSQL, string strOrder,
                                      ref List<TVProgram> progs)
    {
      lock (typeof (TVDatabase))
      {
        progs.Clear();
        try
        {
          if (null == m_db)
          {
            return false;
          }

          SQLiteResultSet results;
          string where = String.Format(" and ( (tblPrograms.iEndTime>='{0}' and tblPrograms.iEndTime <='{1}') ",
                                       iStartTime, iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime>='{0}' and tblPrograms.iStartTime <= '{1}' ) ", iStartTime,
                                 iEndTime);
          where += String.Format(" or (tblPrograms.iStartTime<='{0}' and tblPrograms.iEndTime >= '{1}') )", iStartTime,
                                 iEndTime);
          strSQL += where;
          strSQL += strOrder;
          results = m_db.Execute(strSQL);
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
              prog.Channel = DatabaseUtility.Get(results, i, "channel.strChannel");
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
          Log.Error("TVDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Open();
        }
        return false;
      }
    }

    #endregion

    #endregion
  } //public class TVDatabase
} //namespace MediaPortal.TV.Database