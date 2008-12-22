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
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.TV.Database;

namespace MediaPortal.Radio.Database
{
  public interface IRadioDatabase
  {
    void Dispose();
    void ClearAll();
    void GetStations(ref ArrayList stations);
    bool GetStation(string radioName, out RadioStation station);
    void UpdateStation(RadioStation channel);
    int AddStation(ref RadioStation channel);
    int GetStationId(string strStation);
    void RemoveStation(string strStationName);
    void RemoveAllStations();
    void RemoveLocalRadioStations();
    int MapDVBSChannel(int idChannel, int freq, int symrate, int fec, int lnbkhz, int diseqc,
      int prognum, int servicetype, string provider, string channel, int eitsched,
      int eitprefol, int audpid, int vidpid, int ac3pid, int apid1, int apid2, int apid3,
      int teltxtpid, int scrambled, int pol, int lnbfreq, int networkid, int tsid, int pcrpid, string aLangCode, string aLangCode1, string aLangCode2, string aLangCode3, int ecmPid, int pmtPid);
    int MapDVBTChannel(string channelName, string providerName, int idChannel, int frequency, int ONID, int TSID, int SID, int audioPid, int pmtPid, int bandWidth, int pcrPid);
    int MapDVBCChannel(string channelName, string providerName, int idChannel, int frequency, int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid);
    int MapATSCChannel(string channelName, int physicalChannel, int minorChannel, int majorChannel, string providerName, int idChannel, int frequency, int symbolrate, int innerFec, int modulation, int ONID, int TSID, int SID, int audioPid, int pmtPid, int pcrPid);
    void GetDVBTTuneRequest(int idChannel, out string strProvider, out int frequency, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int bandWidth, out int pcrPid);
    void GetDVBCTuneRequest(int idChannel, out string strProvider, out int frequency, out int symbolrate, out int innerFec, out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int pcrPid);
    void GetATSCTuneRequest(int idChannel, out int physicalChannel, out int minorChannel, out int majorChannel, out string strProvider, out int frequency, out int symbolrate, out int innerFec, out int modulation, out int ONID, out int TSID, out int SID, out int audioPid, out int pmtPid, out int pcrPid);
    bool GetDVBSTuneRequest(int idChannel, int serviceType, ref DVBChannel retChannel);
    void MapChannelToCard(int channelId, int card);
    void DeleteCard(int card);
    void UnmapChannelFromCard(RadioStation channel, int card);
    bool CanCardTuneToStation(string channelName, int card);
    void GetStationsForCard(ref ArrayList stations, int card);
    RadioStation GetStationByStream(bool atsc, bool dvbt, bool dvbc, bool dvbs, int networkid, int transportid, int serviceid, out string provider);
    void UpdatePids(bool isATSC, bool isDVBC, bool isDVBS, bool isDVBT, DVBChannel channel);
    int AddProgram(TVProgram prog);
    int AddGenre(string strGenre1);
    int UpdateProgram(TVProgram prog);
    void RemoveOldPrograms();
    bool GetGenres(ref List<string> genres);
    bool GetProgramsPerChannel(string strChannel1, long iStartTime, long iEndTime, ref List<TVProgram> progs);

  }
}
