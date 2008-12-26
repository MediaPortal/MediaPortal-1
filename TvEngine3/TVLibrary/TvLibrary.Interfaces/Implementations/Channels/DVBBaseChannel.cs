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
using System;
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
    int _videoPid;
    int _audioPid;
    bool _isRadio;
    bool _isTv;
    bool _freeToAir;
    #endregion

    /// <summary>
    /// ctor
    /// </summary>
    public DVBBaseChannel(DVBBaseChannel chan)
    {
      _channelName = chan._channelName;
      _providerName = chan._providerName;
      _channelFrequency = chan._channelFrequency;
      _networkId = chan._networkId;
      _serviceId = chan._serviceId;
      _transportId = chan._transportId;
      _pmtPid = chan._pmtPid;
      _pcrPid = chan._pcrPid;
      _lcn = chan._lcn;
      _isRadio = chan._isRadio;
      _isTv = chan._isTv;
      _freeToAir = chan._freeToAir;
      _videoPid = chan._videoPid;
      _audioPid = chan._audioPid;
    }

    ///<summary>
    /// Base constructor
    ///</summary>
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
      _audioPid = -1;
      _videoPid = -1;
    }

    #region properties
    /// <summary>
    /// gets/set the LCN of the channel
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
    /// <summary>
    /// Gets or sets the PCR pid.
    /// </summary>
    /// <value>The PCR pid.</value>
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
    /// <summary>
    /// gets/sets the AudioPid
    /// </summary>
    public int AudioPid
    {
      get
      {
        return _audioPid;
      }
      set
      {
        _audioPid = value;
      }
    }
    /// <summary>
    /// gets/sets the VideoPid
    /// </summary>
    public int VideoPid
    {
      get
      {
        return _videoPid;
      }
      set
      {
        _videoPid = value;
      }
    }
    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      string line = IsRadio ? "radio:" : "tv:";
      line += String.Format("{0} {1} Freq:{2} ONID:{3} TSID:{4} SID:{5} PMT:0x{6:X} FTA:{7} LCN:{8}",
        Provider, Name, Frequency, NetworkId, TransportId, ServiceId, PmtPid, FreeToAir, LogicalChannelNumber);
      return line;
    }


    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      if ((obj as DVBBaseChannel) == null)
        return false;
      DVBBaseChannel ch = obj as DVBBaseChannel;
      if (ch.FreeToAir != FreeToAir)
        return false;
      if (ch.Frequency != Frequency)
        return false;
      if (ch.IsRadio != IsRadio)
        return false;
      if (ch.IsTv != IsTv)
        return false;
      if (ch.Name != Name)
        return false;
      if (ch.NetworkId != NetworkId)
        return false;
      if (ch.PcrPid != PcrPid)
        return false;
      if (ch.PmtPid != PmtPid)
        return false;
      if (ch.Provider != Provider)
        return false;
      if (ch.ServiceId != ServiceId)
        return false;
      if (ch.TransportId != TransportId)
        return false;
      if (ch.VideoPid != VideoPid)
        return false;
      if (ch.AudioPid != AudioPid)
        return false;
      if (ch.LogicalChannelNumber != LogicalChannelNumber)
        return false;
      return true;
    }
    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _channelName.GetHashCode() ^ _providerName.GetHashCode() ^ _pmtPid.GetHashCode() ^ _pcrPid.GetHashCode() ^ _networkId.GetHashCode() ^ _serviceId.GetHashCode() ^ _transportId.GetHashCode() ^ _lcn.GetHashCode() ^ _videoPid.GetHashCode() ^ _audioPid.GetHashCode();
    }
  }
}
