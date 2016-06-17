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

using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Channel
{
  /// <summary>
  /// Interface which describes a satellite channel.
  /// </summary>
  public interface IChannelSatellite : IChannel, IChannelPhysical
  {
    /// <summary>
    /// Get/set the longitude of the satellite that the channel is broadcast from. The longitude unit is tenths of a degree. Negative values are West; positive values are East.
    /// </summary>
    int Longitude
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the channel transmitter's polarisation.
    /// </summary>
    Polarisation Polarisation
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the channel transmitter's modulation scheme.
    /// </summary>
    ModulationSchemePsk ModulationScheme
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the channel transmitter's symbol rate. The symbol rate unit is kilo-symbols per second (ks/s).
    /// </summary>
    int SymbolRate
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the channel transmitter's forward error correction code rate.
    /// </summary>
    FecCodeRate FecCodeRate
    {
      get;
      set;
    }
  }
}