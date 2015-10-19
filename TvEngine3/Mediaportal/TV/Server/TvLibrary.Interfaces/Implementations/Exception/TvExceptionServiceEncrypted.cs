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
  /// service's video and/or audio is encrypted.
  /// </summary>
  [Serializable]
  public class TvExceptionServiceEncrypted : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionServiceEncrypted"/> class.
    /// </summary>
    /// <param name="service">The tuning and service details for the service.</param>
    public TvExceptionServiceEncrypted(IChannel service, bool isVideoEncrypted, bool isAudioEncrypted)
      : base("The service's {0} encrypted.{1}{2}", GetStreamDescription(isVideoEncrypted, isAudioEncrypted), Environment.NewLine, service)
    {
    }

    private static string GetStreamDescription(bool isVideoEncrypted, bool isAudioEncrypted)
    {
      if (isVideoEncrypted && isAudioEncrypted)
      {
        return "video and audio streams are";
      }
      if (isVideoEncrypted)
      {
        return "video stream is";
      }
      return "audio stream is";
    }
  }
}