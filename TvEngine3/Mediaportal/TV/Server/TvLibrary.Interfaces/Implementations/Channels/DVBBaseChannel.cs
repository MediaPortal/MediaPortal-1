#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels
{
  /// <summary>
  /// A base class for holding DVB (and ATSC) channel tuning details.
  /// </summary>
  [DataContract]
  [KnownType(typeof(DVBTChannel))]
  [KnownType(typeof(DVBCChannel))]
  [KnownType(typeof(DVBSChannel))]
  [KnownType(typeof(DVBIPChannel))]
  [KnownType(typeof(ATSCChannel))]
    public abstract class DVBBaseChannel : IChannel
  {
    #region variables

    [DataMember]
    private string _channelName = String.Empty;

    [DataMember]
    private string _providerName = String.Empty;

    [DataMember]
    private long _channelFrequency = -1;

    [DataMember]
    private int _networkId = -1;

    [DataMember]
    private int _transportId = -1;

    [DataMember]
    private int _serviceId = -1;

    [DataMember]
    private int _pmtPid = -1;

    [DataMember]
    private int _lcn = 10000;
        
    [DataMember]
    private bool _freeToAir = true;

    [DataMember]
    private MediaTypeEnum _mediaType;

    #endregion

    #region constructors

    ///<summary>
    /// Base <see cref="DVBBaseChannel"/> constructor.
    ///</summary>
    public DVBBaseChannel()
    {
      _channelName = String.Empty;
      _providerName = String.Empty;
      _channelFrequency = -1;
      _networkId = -1;
      _transportId = -1;
      _serviceId = -1;
      _pmtPid = -1;
      _lcn = 10000;
      _mediaType = MediaTypeEnum.TV;
      _freeToAir = true;
    }

    /// <summary>
    /// Initialise a new instance of a <see cref="DVBBaseChannel"/> derived instance using an
    /// existing instance.
    /// </summary>
    /// <param name="channel">The existing channel instance.</param>
    public DVBBaseChannel(DVBBaseChannel channel)
    {
      _channelName = channel.Name;
      _providerName = channel.Provider;
      _channelFrequency = channel.Frequency;
      _networkId = channel.NetworkId;
      _transportId = channel.TransportId;
      _serviceId = channel.ServiceId;
      _pmtPid = channel.PmtPid;
      _lcn = channel.LogicalChannelNumber;
      _mediaType = channel._mediaType;      
      _freeToAir = channel.FreeToAir;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/set the channel's name.
    /// </summary>
    public string Name
    {
      get { return _channelName; }
      set { _channelName = value; }
    }

    /// <summary>
    /// Get/set the channel provider's name.
    /// </summary>
    public string Provider
    {
      get { return _providerName; }
      set { _providerName = value; }
    }

    /// <summary>
    /// Get/set the carrier frequency for the channel. The frequency unit is kHz.
    /// </summary>
    public long Frequency
    {
      get { return _channelFrequency; }
      set { _channelFrequency = value; }
    }

    /// <summary>
    /// Get/set the network ID for the channel.
    /// </summary>
    public int NetworkId
    {
      get { return _networkId; }
      set { _networkId = value; }
    }

    /// <summary>
    /// Get/set the transport stream ID for the channel.
    /// </summary>
    public int TransportId
    {
      get { return _transportId; }
      set { _transportId = value; }
    }

    /// <summary>
    /// Get/set the service ID for the channel.
    /// </summary>
    public int ServiceId
    {
      get { return _serviceId; }
      set { _serviceId = value; }
    }

    /// <summary>
    /// Get/set the PID of the program map table for this channel
    /// </summary>
    public int PmtPid
    {
      get { return _pmtPid; }
      set { _pmtPid = value; }
    }

    /// <summary>
    /// Get/set the logical channel number for the channel.
    /// </summary>
    public int LogicalChannelNumber
    {
      get { return _lcn; }
      set { _lcn = value; }
    }


    /// <summary>
    /// Get/set whether the channel is a free-to-air or encrypted channel.
    /// </summary>
    public bool FreeToAir
    {
      get { return _freeToAir; }
      set { _freeToAir = value; }
    }

    public MediaTypeEnum MediaType
    {
      get { return _mediaType; }
      set { _mediaType = value; }
    }

    #endregion

    #region object overrides

    /// <summary>
    /// Get a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>
    /// </returns>
    public override string ToString()
    {
      string line = MediaType.ToString();
      line += String.Format("{0} {1} Freq:{2} ONID:{3} TSID:{4} SID:{5} PMT:0x{6:X} FTA:{7} LCN:{8}",
                            Provider, Name, Frequency, NetworkId, TransportId, ServiceId, PmtPid, FreeToAir,
                            LogicalChannelNumber);
      return line;
    }

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>, otherwise <c>false</c>
    /// </returns>
    public override bool Equals(object obj)
    {
      DVBBaseChannel ch = obj as DVBBaseChannel;
      if (ch == null)
      {
        return false;
      }

      if (!ch.Name.Equals(_channelName))
      {
        return false;
      }
      if (!ch.Provider.Equals(_providerName))
      {
        return false;
      }
      if (ch.Frequency != _channelFrequency)
      {
        return false;
      }
      if (ch.NetworkId != _networkId)
      {
        return false;
      }
      if (ch.TransportId != _transportId)
      {
        return false;
      }
      if (ch.ServiceId != _serviceId)
      {
        return false;
      }
      if (ch.PmtPid != _pmtPid)
      {
        return false;
      }
      if (ch.LogicalChannelNumber != _lcn)
      {
        return false;
      }
      if (ch.MediaType != MediaType)
      {
        return false;
      }      
      if (ch.FreeToAir != _freeToAir)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"></see></returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _channelName.GetHashCode() ^ _providerName.GetHashCode() ^
            _channelFrequency.GetHashCode() ^ _networkId.GetHashCode() ^ _transportId.GetHashCode() ^
            _serviceId.GetHashCode() ^ _pmtPid.GetHashCode() ^ _lcn.GetHashCode() ^ _mediaType.GetHashCode() 
             ^ _freeToAir.GetHashCode();
    }

    #endregion

    #region ICloneable member

    /// <summary>
    /// Clone the channel instance.
    /// </summary>
    /// <returns>a shallow clone of the channel instance</returns>
    public virtual object Clone()
    {
      return MemberwiseClone();
    }

    #endregion

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    public virtual bool IsDifferentTransponder(IChannel channel)
    {
      return true;
    }

    /// <summary>
    /// Get a channel instance with properties set to enable tuning of this channel.
    /// </summary>
    /// <returns>a channel instance with parameters adjusted as necessary</returns>
    public virtual IChannel GetTuningChannel()
    {
      // No adjustments required.
      return (IChannel)Clone();
    }
  }
}