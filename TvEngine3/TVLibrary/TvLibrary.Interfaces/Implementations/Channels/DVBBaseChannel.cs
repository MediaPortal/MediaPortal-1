using System;
using System.Collections.Generic;
using System.Text;

using TvLibrary.Interfaces;
namespace TvLibrary.Channels
{
  /// <summary>
  /// base class for DVB channels
  /// </summary>
  [Serializable]
  public class DVBBaseChannel : IChannel
  {
    #region variables
    string _channelName;
    string _providerName;
    long _channelFrequency;
    int _networkId;
    int _serviceId;
    int _transportId;
    int _pmtPid;
    int _pcrPid;
    int _lcn;
    bool _isRadio;
    bool _isTv;
    bool _freeToAir;
    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    public DVBBaseChannel()
    {
      _channelName = "";
      _providerName = "";
      _pmtPid = -1;
      _pcrPid = -1;
      _networkId = -1;
      _serviceId = -1;
      _transportId = -1;
      _lcn = 10000;
    }

    #region properties
    /// <summary>
    /// gets/set the pid of the PCR
    /// </summary>
    public int LogicalChannelNumber
    {
      get
      {
        return _lcn;
      }
      set
      {
        _lcn = value;
      }
    }
    public int PcrPid
    {
      get
      {
        return _pcrPid;
      }
      set
      {
        _pcrPid = value;
      }
    }
    /// <summary>
    /// gets/set the pid of the Program management table for the channel
    /// </summary>
    public int PmtPid
    {
      get
      {
        return _pmtPid;
      }
      set
      {
        _pmtPid = value;
      }
    }
    /// <summary>
    /// gets/sets the network id of the channel
    /// </summary>
    public int NetworkId
    {
      get
      {
        return _networkId;
      }
      set
      {
        _networkId = value;
      }
    }
    /// <summary>
    /// gets/sets the service id of the channel
    /// </summary>
    public int ServiceId
    {
      get
      {
        return _serviceId;
      }
      set
      {
        _serviceId = value;
      }
    }

    /// <summary>
    /// gets/sets the transport id of the channel
    /// </summary>
    public int TransportId
    {
      get
      {
        return _transportId;
      }
      set
      {
        _transportId = value;
      }
    }

    /// <summary>
    /// gets/sets the channel name
    /// </summary>
    public string Name
    {
      get
      {
        return _channelName;
      }
      set
      {
        _channelName = value;
      }
    }

    /// <summary>
    /// gets/sets the channel provider name
    /// </summary>
    public string Provider
    {
      get
      {
        return _providerName;
      }
      set
      {
        _providerName = value;
      }
    }
    /// <summary>
    /// gets/sets the carrier frequency of the channel
    /// </summary>
    public long Frequency
    {
      get
      {
        return _channelFrequency;
      }
      set
      {
        _channelFrequency = value;
      }
    }
    
    /// <summary>
    /// boolean indication if this is a radio channel
    /// </summary>
    public bool IsRadio
    {
      get
      {
        return _isRadio;
      }
      set
      {
        _isRadio = value;
      }
    }

    /// <summary>
    /// boolean indication if this is a tv channel
    /// </summary>
    public bool IsTv
    {
      get
      {
        return _isTv;
      }
      set
      {
        _isTv = value;
      }
    }
    /// <summary>
    /// boolean indicating if this is a FreeToAir channel or an encrypted channel
    /// </summary>
    public bool FreeToAir
    {
      get
      {
        return _freeToAir;
      }
      set
      {
        _freeToAir = value;
      }
    }

    #endregion

    ///
    public override string ToString()
    {
      string line = "";
      if (IsRadio)
      {
        line = "radio:";
      }
      else
      {
        line = "tv:";
      }
      line += String.Format("{0} {1} Freq:{2} ONID:{3} TSID:{4} SID:{5} PMT:{6:X} FTA:{7} LCN:{8}",
        Provider,Name, Frequency,NetworkId, TransportId, ServiceId, PmtPid, FreeToAir,LogicalChannelNumber);
      return line;
    }


    public override bool Equals(object obj)
    {
      if ((obj as DVBBaseChannel) == null) return false;
      DVBBaseChannel ch = obj as DVBBaseChannel;
      if (ch.FreeToAir != FreeToAir) return false;
      if (ch.Frequency != Frequency) return false;
      if (ch.IsRadio != IsRadio) return false;
      if (ch.IsTv != IsTv) return false;
      if (ch.Name != Name) return false;
      if (ch.NetworkId != NetworkId) return false;
      if (ch.PcrPid != PcrPid) return false;
      if (ch.PmtPid != PmtPid) return false;
      if (ch.Provider != Provider) return false;
      if (ch.ServiceId != ServiceId) return false;
      if (ch.TransportId != TransportId) return false;
      return true;
    }
  }
}
