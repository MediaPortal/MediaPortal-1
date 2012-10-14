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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces
{
  /// <summary>
  /// interface which describes a tv/radio channel
  /// </summary>
  public interface IChannel : ICloneable
  {
    /// <summary>
    /// gets/sets the channel name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Check if the given channel and this instance are on different transponders.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>false</c> if the channels are on the same transponder, otherwise <c>true</c></returns>
    bool IsDifferentTransponder(IChannel channel);

    /// <summary>
    /// Get/set whether the channel is a free-to-air or encrypted channel.
    /// </summary>
    bool FreeToAir { get; set; }



    /// <summary>
    /// 
    /// </summary>
    MediaTypeEnum MediaType { get; set; }

    /// <summary>
    /// Get a channel instance with properties set to enable tuning of this channel.
    /// </summary>
    /// <remarks>
    /// For some channel types (especially satellite channels), logical property values must be adjusted or translated
    /// to enable the channel to be successfully tuned. This function is responsible for making those adjustments.
    /// </remarks>
    /// <returns>a channel instance with parameters adjusted as necessary</returns>
    IChannel GetTuningChannel();
  }
}