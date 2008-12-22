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
using System.Text;
using MediaPortal.TV.Database;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{

  #region MHWEvent class
  class MHWEvent : EPGEvent
  {
    private TVChannel _tvChannel = null;
    private RadioStation _station = null;
    private int _networkId;
    private int _serviceId;
    private int _transportId;
    private NetworkType _networkType;

    public MHWEvent(NetworkType networkType, int networkId, int serviceId, int transportId, string title, string description, string genre, DateTime startTime, DateTime endTime)
      : base(genre, startTime, endTime)
    {
      _networkType = networkType;
      _networkId = networkId;
      _serviceId = serviceId;
      _transportId = transportId;
      Languages.Add(new EPGLanguage(string.Empty, title, description));
    }
    public NetworkType Network
    {
      get { return _networkType; }
    }
    public int NetworkId
    {
      get { return _networkId; }
    }
    public int ServiceId
    {
      get { return _serviceId; }
    }
    public int TransportId
    {
      get { return _transportId; }
    }
    public TVChannel TvChannel
    {
      get
      {
        if (_tvChannel == null)
        {
          string provider;
          _tvChannel = TVDatabase.GetTVChannelByStream(Network == NetworkType.ATSC, Network == NetworkType.DVBT, Network == NetworkType.DVBC, Network == NetworkType.DVBS, _networkId, _transportId, _serviceId, out provider);
        }
        return _tvChannel;
      }
    }
    public RadioStation RadioStation
    {
      get
      {
        if (_station == null)
        {
          string provider;
          _station = RadioDatabase.GetStationByStream(Network == NetworkType.ATSC, Network == NetworkType.DVBT, Network == NetworkType.DVBC, Network == NetworkType.DVBS, _networkId, _transportId, _serviceId, out provider);
        }
        return _station;
      }
    }
  }

  class MHWEventComparer : IComparer<MHWEvent>
  {
    public int Compare(MHWEvent show1, MHWEvent show2)
    {
      if (show1.NetworkId < show2.NetworkId) return -1;
      if (show1.NetworkId > show2.NetworkId) return 1;

      if (show1.ServiceId < show2.ServiceId) return -1;
      if (show1.ServiceId > show2.ServiceId) return 1;

      if (show1.TransportId < show2.TransportId) return -1;
      if (show1.TransportId > show2.TransportId) return 1;

      if (show1.StartTime < show2.StartTime) return -1;
      if (show1.StartTime > show2.StartTime) return 1;
      return 0;
    }
  }
  #endregion

}
