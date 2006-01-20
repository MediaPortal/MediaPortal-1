using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{

  #region MHWEvent class
  class MHWEvent : EPGEvent
  {
    private TVChannel _tvChannel = null;
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
      Languages.Add(new EPGLanguage(String.Empty, title, description));
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
