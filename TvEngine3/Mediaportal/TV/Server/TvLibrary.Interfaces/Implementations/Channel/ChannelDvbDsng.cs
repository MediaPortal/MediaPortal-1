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
  /// An implementation of <see cref="T:IChannel"/> for DVB-DSNG channels.
  /// </summary>
  [DataContract]
  public class ChannelDvbDsng : ChannelDvbBase, IChannelDvb, IChannelSatellite
  {
    #region variables

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
    private RollOffFactor _rollOffFactor = RollOffFactor.Automatic;

    #endregion

    #region properties

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

    #endregion

    #region IChannel members

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    public override bool IsDifferentTransmitter(IChannel channel)
    {
      ChannelDvbDsng dvbDsngChannel = channel as ChannelDvbDsng;
      if (
        dvbDsngChannel == null ||
        Longitude != dvbDsngChannel.Longitude ||
        Frequency != dvbDsngChannel.Frequency ||
        Polarisation != dvbDsngChannel.Polarisation ||
        ModulationScheme != dvbDsngChannel.ModulationScheme ||
        SymbolRate != dvbDsngChannel.SymbolRate ||
        FecCodeRate != dvbDsngChannel.FecCodeRate ||
        RollOffFactor != dvbDsngChannel.RollOffFactor
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
      ChannelDvbDsng channel = obj as ChannelDvbDsng;
      if (
        channel == null ||
        !base.Equals(obj) ||
        Longitude != Longitude ||
        Frequency != channel.Frequency ||
        Polarisation != channel.Polarisation ||
        ModulationScheme != channel.ModulationScheme ||
        SymbolRate != channel.SymbolRate ||
        FecCodeRate != channel.FecCodeRate ||
        RollOffFactor != channel.RollOffFactor
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
      return base.GetHashCode() ^ Longitude.GetHashCode() ^
              Frequency.GetHashCode() ^ Polarisation.GetHashCode() ^
              ModulationScheme.GetHashCode() ^ SymbolRate.GetHashCode() ^
              FecCodeRate.GetHashCode() ^ RollOffFactor.GetHashCode();
    }

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return string.Format("DVB-DSNG, {0}, satellite ID = {1}, frequency = {2} kHz, polarisation = {3}, modulation scheme = {4}, symbol rate = {5} ks/s, FEC code rate = {6}, roll-off factor = {7}",
                            base.ToString(), Longitude, Frequency,
                            Polarisation, ModulationScheme, SymbolRate,
                            FecCodeRate, RollOffFactor);
    }

    #endregion
  }
}