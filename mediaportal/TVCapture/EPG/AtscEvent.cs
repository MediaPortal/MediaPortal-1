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
using System.Collections.Generic;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{

  #region ATSCEvent class

  internal class ATSCEvent : EPGEvent
  {
    private TVChannel _tvChannel = null;
    private int _majorChannel;
    private int _minorChannel;
    private NetworkType _networkType;

    public ATSCEvent(NetworkType networkType, int majorChannel, int minorChannel, string title, string description,
                     string genre, DateTime startTime, DateTime endTime)
      : base(genre, startTime, endTime)
    {
      _networkType = networkType;
      _majorChannel = majorChannel;
      _minorChannel = minorChannel;
      Languages.Add(new EPGLanguage("", title, description));
    }

    public NetworkType Network
    {
      get { return _networkType; }
    }

    public int MajorChannel
    {
      get { return _majorChannel; }
    }

    public int MinorChannel
    {
      get { return _minorChannel; }
    }

    public TVChannel TvChannel
    {
      get
      {
        if (_tvChannel == null)
        {
          List<TVChannel> channels = new List<TVChannel>();
          TVDatabase.GetChannels(ref channels);
          foreach (TVChannel ch in channels)
          {
            int symbolrate = 0, innerFec = 0, modulation = 0, physicalChannel = 0;
            int minorChannel = 0, majorChannel = 0;
            int frequency = -1, ONID = -1, TSID = -1, SID = -1;
            int audioPid = -1, videoPid = -1, teletextPid = -1, pmtPid = -1, pcrPid = -1;
            string providerName;
            int audio1, audio2, audio3, ac3Pid;
            string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
            bool HasEITPresentFollow, HasEITSchedule;
            TVDatabase.GetATSCTuneRequest(ch.ID, out physicalChannel, out providerName, out frequency, out symbolrate,
                                          out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid,
                                          out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3,
                                          out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2,
                                          out audioLanguage3, out minorChannel, out majorChannel,
                                          out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
            if (MajorChannel == majorChannel && MinorChannel == minorChannel)
            {
              _tvChannel = ch;
              return _tvChannel;
            }
          }
        }
        return _tvChannel;
      }
    }
  }

  #endregion
}