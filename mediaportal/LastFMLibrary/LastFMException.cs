#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

namespace MediaPortal.LastFM
{
  /// <summary>
  /// Custom exception class to combine errors returned from last.fm and lower level exceptions (eg. unable to reach website)
  /// </summary>
  public class LastFMException : Exception
  {

    /// <summary>
    /// Standard errors as defined by last.fm (http://www.last.fm/api/errorcodes)
    /// </summary>
    public enum LastFMErrorCode
    {
      InvalidService = 2,
      InvalidMethod = 3,
      AuthenticationFailed =4,
      InvalidFormat =5,
      InvalidParameters =6,
      InalidResource = 7,
      OperationFailed = 8,
      InvalidSessionKey = 9,
      InvalidAPIKey = 10,
      ServiceOffline = 11,
      SubscribersOnly = 12,
      InvalidMethodSignature = 13,
      UnauthorisedToken = 14,
      ItemUnavailableForStreaming = 15,
      ServiceUnavailable = 16,
      LoginRequired = 17,
      TrialExpired = 18,
      NotEnoughContent = 20,
      NotEnoughMembers = 21,
      NotEnoughFans = 22,
      NotEnoughNeighbours = 23,
      NoPeakRadio = 24,
      RadioStationNotFound = 25,
      APIKeySuspended = 26,
      DeprecatedRequest = 27,
      RateLimitExceeded = 29,
      UnknownError = 999 // added in addition to last.fm supplied values incase a return does not match
    }

    public LastFMErrorCode LastFMError { get; set; }

    public LastFMException()
    {
    }

    public LastFMException(string message) : base(message)
    {
    }

    public LastFMException(string message, Exception inner) : base(message, inner)
    {
      LastFMError = LastFMErrorCode.UnknownError;
    }

  }
}
