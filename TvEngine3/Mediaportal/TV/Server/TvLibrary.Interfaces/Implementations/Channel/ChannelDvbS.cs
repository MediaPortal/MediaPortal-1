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

using System.Runtime.Serialization;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel
{
  /// <summary>
  /// An implementation of <see cref="T:IChannel"/> for DVB-S channels.
  /// </summary>
  [DataContract]
  public class ChannelDvbS : ChannelDvbBase, IChannelSatellite
  {
    #region variables

    [DataMember]
    protected int _freesatChannelId = -1;

    #region TODO
    // TODO move these properties to a TunerSatellite class, and replace them with a reference to a satellite.

    [DataMember]
    private int _diseqcPositionerSatelliteIndex = -1;

    [DataMember]
    private DiseqcPort _diseqcSwitchPort = DiseqcPort.None;

    [DataMember]
    private ILnbType _lnbType = null;

    #endregion

    [DataMember]
    private int _frequency = -1;

    [DataMember]
    private Polarisation _polarisation = Polarisation.Automatic;

    [DataMember]
    private ModulationSchemePsk _modulationScheme = ModulationSchemePsk.Automatic;

    [DataMember]
    private int _symbolRate = -1;

    [DataMember]
    private FecCodeRate _fecCodeRate = FecCodeRate.Automatic;

    #endregion

    #region properties

    /// <summary>
    /// Get/set the channel's Freesat channel identifier.
    /// </summary>
    public int FreesatChannelId
    {
      get
      {
        return _freesatChannelId;
      }
      set
      {
        _freesatChannelId = value;
      }
    }

    /// <summary>
    /// Get/set the DiSEqC positioner index of the satellite that the channel is broadcast from.
    /// </summary>
    public int DiseqcPositionerSatelliteIndex
    {
      get
      {
        return _diseqcPositionerSatelliteIndex;
      }
      set
      {
        _diseqcPositionerSatelliteIndex = value;
      }
    }

    /// <summary>
    /// Get/set the DiSEqC switch setting used to select the satellite that the channel is broadcast from.
    /// </summary>
    public DiseqcPort DiseqcSwitchPort
    {
      get
      {
        return _diseqcSwitchPort;
      }
      set
      {
        _diseqcSwitchPort = value;
      }
    }

    /// <summary>
    /// Get/set the type of LNB used to receive the channel.
    /// </summary>
    public ILnbType LnbType
    {
      get
      {
        return _lnbType;
      }
      set
      {
        _lnbType = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's carrier frequency. The frequency unit is kilo-Hertz (kHz).
    /// </summary>
    public int Frequency
    {
      get
      {
        return _frequency;
      }
      set
      {
        _frequency = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's polarisation.
    /// </summary>
    public Polarisation Polarisation
    {
      get
      {
        return _polarisation;
      }
      set
      {
        _polarisation = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's modulation scheme.
    /// </summary>
    public ModulationSchemePsk ModulationScheme
    {
      get
      {
        return _modulationScheme;
      }
      set
      {
        _modulationScheme = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's symbol rate. The symbol rate unit is kilo-symbols per second (ks/s).
    /// </summary>
    public int SymbolRate
    {
      get
      {
        return _symbolRate;
      }
      set
      {
        _symbolRate = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's forward error correction code rate.
    /// </summary>
    public FecCodeRate FecCodeRate
    {
      get
      {
        return _fecCodeRate;
      }
      set
      {
        _fecCodeRate = value;
      }
    }

    #endregion

    #region IChannel members

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public override bool IsDifferentTransmitter(IChannel channel)
    {
      ChannelDvbS dvbsChannel = channel as ChannelDvbS;
      if (
        dvbsChannel == null ||
        DiseqcPositionerSatelliteIndex != dvbsChannel.DiseqcPositionerSatelliteIndex ||
        DiseqcSwitchPort != dvbsChannel.DiseqcSwitchPort ||
        Frequency != dvbsChannel.Frequency ||
        Polarisation != dvbsChannel.Polarisation ||
        ModulationScheme != dvbsChannel.ModulationScheme ||
        SymbolRate != dvbsChannel.SymbolRate ||
        FecCodeRate != dvbsChannel.FecCodeRate
      )
      {
        return true;
      }
      return false;
    }

    #endregion

    #region object overrides

    /// <summary>
    /// Determine whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>, otherwise <c>false</c></returns>
    public override bool Equals(object obj)
    {
      ChannelDvbS channel = obj as ChannelDvbS;
      if (
        channel == null ||
        !base.Equals(obj) ||
        FreesatChannelId != channel.FreesatChannelId ||
        DiseqcPositionerSatelliteIndex != channel.DiseqcPositionerSatelliteIndex ||
        DiseqcSwitchPort != channel.DiseqcSwitchPort ||
        (LnbType == null && channel.LnbType != null) ||
        (LnbType != null && channel.LnbType == null) ||
        (LnbType != null && channel.LnbType != null && LnbType != channel.LnbType) ||
        Frequency != channel.Frequency ||
        Polarisation != channel.Polarisation ||
        ModulationScheme != channel.ModulationScheme ||
        SymbolRate != channel.SymbolRate ||
        FecCodeRate != channel.FecCodeRate
      )
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// A hash function for this type.
    /// </summary>
    /// <returns>a hash code for the current <see cref="T:System.Object"/></returns>
    public override int GetHashCode()
    {
      int hashCode = base.GetHashCode() ^ FreesatChannelId.GetHashCode() ^
        DiseqcPositionerSatelliteIndex.GetHashCode() ^
        DiseqcSwitchPort.GetHashCode() ^ Frequency.GetHashCode() ^
        Polarisation.GetHashCode() ^ ModulationScheme.GetHashCode() ^
        SymbolRate.GetHashCode() ^ FecCodeRate.GetHashCode();
      if (LnbType != null)
      {
        hashCode ^= LnbType.GetHashCode();
      }
      return hashCode;
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("DVB-S, {0}, Freesat CID = {1}, satellite index = {2}, DiSEqC = {3}, LNB type = {4}, frequency = {5} kHz, polarisation = {6}, modulation scheme = {7}, symbol rate = {8} ks/s, FEC code rate = {9}",
                            base.ToString(), FreesatChannelId,
                            DiseqcPositionerSatelliteIndex, DiseqcSwitchPort,
                            LnbType == null ? "[null]" : LnbType.ToString(),
                            Frequency, Polarisation, ModulationScheme,
                            SymbolRate, FecCodeRate);
    }

    #endregion

    #region ICloneable member

    /// <summary>
    /// Clone the channel instance.
    /// </summary>
    /// <returns>a shallow clone of the channel instance</returns>
    public override object Clone()
    {
      ChannelDvbS channel = (ChannelDvbS)MemberwiseClone();
      if (LnbType != null)
      {
        channel.LnbType = (ILnbType)LnbType.Clone();
      }
      return channel;
    }

    #endregion
  }
}