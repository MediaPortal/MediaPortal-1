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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Streaming
{
  /// <summary>
  /// A class describing a single RTSP stream.
  /// </summary>
  public class RtspStream
  {
    #region variables

    private readonly string _id;
    private readonly string _fileName;
    private readonly MediaType _mediaType;
    private readonly ITuner _tuner;
    private readonly int _subChannelId;
    private readonly string _name;

    #endregion

    #region constructors

    /// <summary>
    /// Initialise a new instance of the <see cref="RtspStream"/> class for a time-shifting session.
    /// </summary>
    /// <param name="id">The identifier for the stream.</param>
    /// <param name="fileName">The time-shifting register file name.</param>
    /// <param name="mediaType">The type of the stream.</param>
    /// <param name="tuner">The tuner that the session is associated with.</param>
    /// <param name="subChannelId">The identifier of the tuner sub-channel that the session is associated with.</param>
    public RtspStream(string id, string fileName, MediaType mediaType, ITuner tuner, int subChannelId)
    {
      _id = id ?? string.Empty;
      _fileName = fileName ?? string.Empty;
      _mediaType = mediaType;
      _tuner = tuner;
      _subChannelId = subChannelId;
      _name = string.Empty;
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="RtspStream"/> class for a static file (eg. recording).
    /// </summary>
    /// <param name="id">The identifier for the stream.</param>
    /// <param name="fileName">The name of the file to stream.</param>
    /// <param name="mediaType">The type of the stream.</param>
    /// <param name="name">A human-readable name for the stream.</param>
    public RtspStream(string id, string fileName, MediaType mediaType, string name)
    {
      _id = id ?? string.Empty;
      _fileName = fileName ?? string.Empty;
      _mediaType = mediaType;
      _tuner = null;
      _subChannelId = -1;
      _name = name ?? string.Empty;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get the stream's name.
    /// </summary>
    public string Id
    {
      get { return _id; }
    }

    /// <summary>
    /// Get the name of the stream's source file.
    /// </summary>
    public string FileName
    {
      get { return _fileName; }
    }

    /// <summary>
    /// Get the stream's media type.
    /// </summary>
    public MediaType MediaType
    {
      get { return _mediaType; }
    }

    /// <summary>
    /// Get the stream's name.
    /// </summary>
    public string Name
    {
      get
      {
        if (_tuner != null)
        {
          ISubChannel subChannel = _tuner.GetSubChannel(_subChannelId);
          if (subChannel != null && subChannel.CurrentChannel != null)
          {
            return subChannel.CurrentChannel.Name ?? string.Empty;
          }
        }
        return _name;
      }
    }

    /// <summary>
    /// Is the stream a time-shifting stream?
    /// </summary>
    public bool IsTimeShifting
    {
      get { return _tuner != null; }
    }

    #endregion
  }
}