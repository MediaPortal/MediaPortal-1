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

using System.Collections;
using System.Collections.Generic;
using MediaPortal.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.Radio.Database
{
  public static class RadioDatabase
  {
    private static IRadioDatabase _database = DatabaseFactory.GetRadioDatabase();
    //static IRadioDatabase _database = new RadioDatabaseADO();
    public static void Dispose()
    {
      _database.Dispose();
      _database = null;
    }

    public static void ClearAll()
    {
      _database.ClearAll();
    }

    public static void GetStations(ref ArrayList stations)
    {
      _database.GetStations(ref stations);
    }

    public static bool GetStation(string radioName, out RadioStation station)
    {
      return _database.GetStation(radioName, out station);
    }

    public static void UpdateStation(RadioStation channel)
    {
      _database.UpdateStation(channel);
    }

    public static int AddStation(ref RadioStation channel)
    {
      return _database.AddStation(ref channel);
    }

    public static int GetStationId(string strStation)
    {
      return _database.GetStationId(strStation);
    }

    public static void RemoveStation(string strStationName)
    {
      _database.RemoveStation(strStationName);
    }

    public static void RemoveAllStations()
    {
      _database.RemoveAllStations();
    }

    public static void RemoveLocalRadioStations()
    {
      _database.RemoveLocalRadioStations();
    }

    public static int MapDVBSChannel(int idChannel, int freq, int symrate, int fec, int lnbkhz, int diseqc,
                                     int prognum, int servicetype, string provider, string channel, int eitsched,
                                     int eitprefol, int audpid, int vidpid, int ac3pid, int apid1, int apid2, int apid3,
                                     int teltxtpid, int scrambled, int pol, int lnbfreq, int networkid, int tsid,
                                     int pcrpid, string aLangCode, string aLangCode1, string aLangCode2,
                                     string aLangCode3, int ecmPid, int pmtPid)
    {
      return _database.MapDVBSChannel(idChannel, freq, symrate, fec, lnbkhz, diseqc, prognum, servicetype, provider,
                                      channel, eitsched, eitprefol, audpid, vidpid, ac3pid, apid1, apid2, apid3,
                                      teltxtpid, scrambled, pol, lnbfreq, networkid, tsid, pcrpid, aLangCode, aLangCode1,
                                      aLangCode2, aLangCode3, ecmPid, pmtPid);
    }

    public static int MapDVBTChannel(string channelName, string providerName, int idChannel, int frequency, int ONID,
                                     int TSID, int SID, int audioPid, int pmtPid, int bandWidth, int pcrPid)
    {
      return _database.MapDVBTChannel(channelName, providerName, idChannel, frequency, ONID, TSID, SID, audioPid, pmtPid,
                                      bandWidth, pcrPid);
    }

    public static int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency,
                                     int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID,
                                     int audioPid, int pmtPid, int pcrPid)
    {
      return _database.MapDVBCChannel(channelName, providerName, idChannel, frequency, symbolrate, innerFec, modulation,
                                      ONID, TSID, SID, audioPid, pmtPid, pcrPid);
    }

    public static int MapATSCChannel(string channelName, int physicalChannel, int minorChannel, int majorChannel,
                                     string providerName, int idChannel, int frequency, int symbolrate, int innerFec,
                                     int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid)
    {
      return _database.MapATSCChannel(channelName, physicalChannel, minorChannel, majorChannel, providerName, idChannel,
                                      frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, audioPid, pmtPid,
                                      pcrPid);
    }

    public static void GetDVBTTuneRequest(int idChannel, out string strProvider, out int frequency, out int ONID,
                                          out int TSID, out int SID, out int audioPid, out int pmtPid, out int bandWidth,
                                          out int pcrPid)
    {
      _database.GetDVBTTuneRequest(idChannel, out strProvider, out frequency, out ONID, out TSID, out SID, out audioPid,
                                   out pmtPid, out bandWidth, out pcrPid);
    }

    public static void GetDVBCTuneRequest(int idChannel, out string strProvider, out int frequency, out int symbolrate,
                                          out int innerFec, out int modulation, out int ONID, out int TSID, out int SID,
                                          out int audioPid, out int pmtPid, out int pcrPid)
    {
      _database.GetDVBCTuneRequest(idChannel, out strProvider, out frequency, out symbolrate, out innerFec,
                                   out modulation, out ONID, out TSID, out SID, out audioPid, out pmtPid, out pcrPid);
    }

    public static void GetATSCTuneRequest(int idChannel, out int physicalChannel, out int minorChannel,
                                          out int majorChannel, out string strProvider, out int frequency,
                                          out int symbolrate, out int innerFec, out int modulation, out int ONID,
                                          out int TSID, out int SID, out int audioPid, out int pmtPid, out int pcrPid)
    {
      _database.GetATSCTuneRequest(idChannel, out physicalChannel, out minorChannel, out majorChannel, out strProvider,
                                   out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID,
                                   out SID, out audioPid, out pmtPid, out pcrPid);
    }

    public static bool GetDVBSTuneRequest(int idChannel, int serviceType, ref DVBChannel retChannel)
    {
      return _database.GetDVBSTuneRequest(idChannel, serviceType, ref retChannel);
    }

    public static void MapChannelToCard(int channelId, int card)
    {
      _database.MapChannelToCard(channelId, card);
    }

    public static void DeleteCard(int card)
    {
      _database.DeleteCard(card);
    }

    public static void UnmapChannelFromCard(RadioStation channel, int card)
    {
      _database.UnmapChannelFromCard(channel, card);
    }

    public static bool CanCardTuneToStation(string channelName, int card)
    {
      return _database.CanCardTuneToStation(channelName, card);
    }

    public static void GetStationsForCard(ref ArrayList stations, int card)
    {
      _database.GetStationsForCard(ref stations, card);
    }

    public static RadioStation GetStationByStream(bool atsc, bool dvbt, bool dvbc, bool dvbs, int networkid,
                                                  int transportid, int serviceid, out string provider)
    {
      return _database.GetStationByStream(atsc, dvbt, dvbc, dvbs, networkid, transportid, serviceid, out provider);
    }

    public static void UpdatePids(bool isATSC, bool isDVBC, bool isDVBS, bool isDVBT, DVBChannel channel)
    {
      _database.UpdatePids(isATSC, isDVBC, isDVBS, isDVBT, channel);
    }

    public static int AddProgram(TVProgram prog)
    {
      return _database.AddProgram(prog);
    }

    public static int AddGenre(string strGenre1)
    {
      return _database.AddGenre(strGenre1);
    }

    public static int UpdateProgram(TVProgram prog)
    {
      return _database.UpdateProgram(prog);
    }

    public static void RemoveOldPrograms()
    {
      _database.RemoveOldPrograms();
    }

    public static bool GetGenres(ref List<string> genres)
    {
      return _database.GetGenres(ref genres);
    }

    public static bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime,
                                             ref List<TVProgram> progs)
    {
      return _database.GetProgramsPerChannel(strChannel1, iStartTime, iEndTime, ref progs);
    }
  }
}