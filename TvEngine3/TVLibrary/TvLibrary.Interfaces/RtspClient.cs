#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace TvLibrary.Streaming
{
  /// <summary>
  ///class holding the details about a single rtsp streaming client
  /// </summary>
  [Serializable]
  public class RtspClient
  {
    #region variables

    private readonly bool _isActive;
    private readonly string _ipAdress;
    private readonly string _streamName;
    private readonly DateTime _dateTimeStarted;
    private readonly string _description;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RtspClient"/> class.
    /// </summary>
    public RtspClient() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="RtspClient"/> class.
    /// </summary>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <param name="ipadress">The ipadress.</param>
    /// <param name="streamName">Name of the stream.</param>
    /// <param name="started">The time the client connected.</param>
    /// <param name="description">The description.</param>
    public RtspClient(bool isActive, string ipadress, string streamName, string description, DateTime started)
    {
      _isActive = isActive;
      _ipAdress = ipadress;
      _streamName = streamName;
      _dateTimeStarted = started;
      _description = description;
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description
    {
      get { return _description; }
    }

    /// <summary>
    /// Gets the date and time this client has connected.
    /// </summary>
    /// <value>The date time started.</value>
    public DateTime DateTimeStarted
    {
      get { return _dateTimeStarted; }
    }

    /// <summary>
    /// Gets a value indicating whether user is is active.
    /// </summary>
    /// <value><c>true</c> if this user is active; otherwise, <c>false</c>.</value>
    public bool IsActive
    {
      get { return _isActive; }
    }

    /// <summary>
    /// Gets the ip adress.
    /// </summary>
    /// <value>The ip adress.</value>
    public string IpAdress
    {
      get { return _ipAdress; }
    }

    /// <summary>
    /// Gets the name of the stream.
    /// </summary>
    /// <value>The name of the stream.</value>
    public string StreamName
    {
      get { return _streamName; }
    }

    #endregion
  }
}