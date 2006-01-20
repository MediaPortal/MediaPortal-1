using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Epg
{
  #region EPGChannel class
  class EPGChannel : IComparer<EPGEvent>
  {
    private TVChannel _tvChannel = null;
    private int _networkId;
    private int _serviceId;
    private int _transportId;
    private NetworkType _networkType;
    List<EPGEvent> _listEvents = new List<EPGEvent>();

    public EPGChannel(NetworkType networkType, int networkId, int serviceId, int transportId)
    {
      _networkType = networkType;
      _networkId = networkId;
      _serviceId = serviceId;
      _transportId = transportId;
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
          if (_tvChannel == null)
            Log.WriteFile(Log.LogType.EPG, "epg-grab: unknown channel: type:{0} network id:{1} service id:{2} transport id:{3}", Network.ToString(), NetworkId, ServiceId, TransportId);
          else
            Log.WriteFile(Log.LogType.EPG, "epg-grab: channel:{0} events:{1}", _tvChannel.Name, _listEvents.Count);
        }
        return _tvChannel;
      }
    }
    public void Sort()
    {
      _listEvents.Sort(this);
    }
    public void AddEvent(EPGEvent epgEvent)
    {
      _listEvents.Add(epgEvent);
    }
    public List<EPGEvent> EpgEvents
    {
      get
      {
        return _listEvents;
      }
    }

    public int Compare(EPGEvent show1, EPGEvent show2)
    {
      if (show1.StartTime < show2.StartTime) return -1;
      if (show1.StartTime > show2.StartTime) return 1;
      return 0;
    }
  }
  #endregion
}
