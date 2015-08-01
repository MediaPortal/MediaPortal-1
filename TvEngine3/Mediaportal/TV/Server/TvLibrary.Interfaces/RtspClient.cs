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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// A class that holds the details for a single RTSP stream client.
  /// </summary>
  [Serializable]
  public class RtspClient
  {
    #region variables

    // client properties
    private readonly uint _clientSessionId;
    private readonly string _clientIpAddress;
    private readonly DateTime _clientConnectionDateTime;
    private readonly bool _isClientActive;

    // properties for the stream that the client is connected to
    private readonly string _streamId;
    private readonly string _streamDescription;
    private readonly string _streamUrl;

    #endregion

    #region ctor

    /// <summary>
    /// Initialise a new instance of the <see cref="RtspClient"/> class.
    /// </summary>
    /// <param name="clientSessionId">The identifier for the client's session.</param>
    /// <param name="clientIpAddress">The client's IP address.</param>
    /// <param name="clientConnectionDateTime">The date/time the client connected.</param>
    /// <param name="isClientActive">An indication of whether the client is active.</param>
    /// <param name="streamId">The identifier of the stream that the client is connected to.</param>
    /// <param name="streamDescription">The description of the stream that the client is connected to.</param>
    /// <param name="streamUrl">The URL of the stream that the client is connected to.</param>
    public RtspClient(uint clientSessionId, string clientIpAddress, DateTime clientConnectionDateTime, bool isClientActive, string streamId, string streamDescription, string streamUrl)
    {
      _clientSessionId = clientSessionId;
      _clientIpAddress = clientIpAddress;
      _clientConnectionDateTime = clientConnectionDateTime;
      _isClientActive = isClientActive;
      _streamId = streamId;
      _streamDescription = streamDescription;
      _streamUrl = streamUrl;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get identifier for the client's session.
    /// </summary>
    public uint ClientSessionId
    {
      get { return _clientSessionId; }
    }

    /// <summary>
    /// Get the client's IP address.
    /// </summary>
    public string ClientIpAddress
    {
      get { return _clientIpAddress; }
    }

    /// <summary>
    /// Get the date/time that the client connected to its stream.
    /// </summary>
    public DateTime ClientConnectionDateTime
    {
      get { return _clientConnectionDateTime; }
    }

    /// <summary>
    /// Get an indication of whether the client is active.
    /// </summary>
    public bool IsClientActive
    {
      get { return _isClientActive; }
    }

    /// <summary>
    /// Get the identifier of the stream that the client is connected to.
    /// </summary>
    public string StreamId
    {
      get { return _streamId; }
    }

    /// <summary>
    /// Get the description of the stream that the client is connected to.
    /// </summary>
    public string StreamDescription
    {
      get { return _streamDescription; }
    }

    /// <summary>
    /// Get the URL of the stream that the client is connected to.
    /// </summary>
    public string StreamUrl
    {
      get { return _streamUrl; }
    }

    #endregion
  }
}