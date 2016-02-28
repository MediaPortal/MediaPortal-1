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
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Channel
{
  /// <summary>
  /// Interface which describes a channel.
  /// </summary>
  public interface IChannel : ICloneable
  {
    /// <summary>
    /// Get/set the channel's name.
    /// </summary>
    string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the channel provider's name.
    /// </summary>
    string Provider
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set the logical number associated with the channel.
    /// </summary>
    string LogicalChannelNumber
    {
      get;
      set;
    }

    /// <summary>
    /// Get the default logical number associated with the channel.
    /// </summary>
    string DefaultLogicalChannelNumber
    {
      get;
    }

    /// <summary>
    /// Get/set the channel's media type.
    /// </summary>
    MediaType MediaType
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set whether the channel is encrypted.
    /// </summary>
    bool IsEncrypted
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set whether the channel's video is high definition (HD).
    /// </summary>
    bool IsHighDefinition
    {
      get;
      set;
    }

    /// <summary>
    /// Get/set whether the channel's video is three dimensional (3D).
    /// </summary>
    bool IsThreeDimensional
    {
      get;
      set;
    }

    /// <summary>
    /// Check if this channel and another channel are broadcast from different transmitters.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the channels are broadcast from different transmitters, otherwise <c>false</c></returns>
    bool IsDifferentTransmitter(IChannel channel);
  }
}