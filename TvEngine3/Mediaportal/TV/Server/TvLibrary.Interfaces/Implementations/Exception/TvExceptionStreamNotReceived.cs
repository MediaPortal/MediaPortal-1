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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception
{
  /// <summary>
  /// Exception thrown by the TV library when physical tuning succeeds but the
  /// service's video and/or audio streams are not received.
  /// </summary>
  [Serializable]
  public class TvExceptionStreamNotReceived : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionStreamNotReceived"/> class.
    /// </summary>
    /// <param name="service">The tuning and service details for the service.</param>
    /// <param name="isVideoReceived"><c>True</c> if the service's video is being received.</param>
    /// <param name="isAudioReceived"><c>True</c> if the service's audio is being received.</param>
    public TvExceptionStreamNotReceived(IChannel service, bool isVideoReceived, bool isAudioReceived)
      : base("The service's {0} not being received.{1}{2}", GetStreamDescription(isVideoReceived, isAudioReceived), Environment.NewLine, service)
    {
    }

    private static string GetStreamDescription(bool isVideoReceived, bool isAudioReceived)
    {
      if (!isVideoReceived && !isAudioReceived)
      {
        return "video and audio streams are";
      }
      if (!isVideoReceived)
      {
        return "video stream is";
      }
      return "audio stream is";
    }
  }
}