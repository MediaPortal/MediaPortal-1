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
  /// An implementation of <see cref="T:IChannel"/> for DVB-S2 channels.
  /// </summary>
  [DataContract]
  public class ChannelDvbS2 : ChannelDvbBase, IChannelSatellite
  {
    #region variables

    [DataMember]
    protected int _freesatChannelId = -1;

    [DataMember]
    private int _longitude = 0;

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

    [DataMember]
    private PilotTonesState _pilotTonesState = PilotTonesState.Automatic;

    [DataMember]
    private RollOffFactor _rollOffFactor = RollOffFactor.Automatic;

    [DataMember]
    private short _streamId = -1;

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
    /// Get/set the longitude of the satellite that the channel is broadcast from. The longitude unit is tenths of a degree. Negative values are West; positive values are East.
    /// </summary>
    public int Longitude
    {
      get
      {
        return _longitude;
      }
      set
      {
        _longitude = value;
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

    /// <summary>
    /// Get/set the channel transmitter's pilot tones state.
    /// </summary>
    public PilotTonesState PilotTonesState
    {
      get
      {
        return _pilotTonesState;
      }
      set
      {
        _pilotTonesState = value;
      }
    }

    /// <summary>
    /// Get/set the channel transmitter's roll-off factor.
    /// </summary>
    public RollOffFactor RollOffFactor
    {
      get
      {
        return _rollOffFactor;
      }
      set
      {
        _rollOffFactor = value;
      }
    }

    /// <summary>
    /// Get/set the identifier of the input stream that the channel is multiplexed in.
    /// </summary>
    public short StreamId
    {
      get
      {
        return _streamId;
      }
      set
      {
        _streamId = value;
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
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
      if (
        dvbs2Channel == null ||
        Longitude != dvbs2Channel.Longitude ||
        Frequency != dvbs2Channel.Frequency ||
        Polarisation != dvbs2Channel.Polarisation ||
        ModulationScheme != dvbs2Channel.ModulationScheme ||
        SymbolRate != dvbs2Channel.SymbolRate ||
        FecCodeRate != dvbs2Channel.FecCodeRate ||
        PilotTonesState != dvbs2Channel.PilotTonesState ||
        RollOffFactor != dvbs2Channel.RollOffFactor ||
        StreamId != dvbs2Channel.StreamId
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
      ChannelDvbS2 channel = obj as ChannelDvbS2;
      if (
        channel == null ||
        !base.Equals(obj) ||
        FreesatChannelId != channel.FreesatChannelId ||
        Longitude != channel.Longitude ||
        Frequency != channel.Frequency ||
        Polarisation != channel.Polarisation ||
        ModulationScheme != channel.ModulationScheme ||
        SymbolRate != channel.SymbolRate ||
        FecCodeRate != channel.FecCodeRate ||
        PilotTonesState != channel.PilotTonesState ||
        RollOffFactor != channel.RollOffFactor ||
        StreamId != channel.StreamId
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
      return base.GetHashCode() ^ FreesatChannelId.GetHashCode() ^
              Longitude.GetHashCode() ^ Frequency.GetHashCode() ^
              Polarisation.GetHashCode() ^ ModulationScheme.GetHashCode() ^
              SymbolRate.GetHashCode() ^ FecCodeRate.GetHashCode() ^
              PilotTonesState.GetHashCode() ^ RollOffFactor.GetHashCode() ^
              StreamId.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("DVB-S2, {0}, Freesat CID = {1}, satellite ID = {2}, frequency = {3} kHz, polarisation = {4}, modulation scheme = {5}, symbol rate = {6} ks/s, FEC code rate = {7}, pilot tones state = {8}, roll-off factor = {9}, stream ID = {10}",
                            base.ToString(), FreesatChannelId, Longitude,
                            Frequency, Polarisation, ModulationScheme,
                            SymbolRate, FecCodeRate, PilotTonesState,
                            RollOffFactor, StreamId);
    }

    #endregion
  }
}