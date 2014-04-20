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

using System.Collections.Generic;
using System.Text;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp
{
  /// <summary>
  /// A simple class that can be used to serialise RTSP requests.
  /// </summary>
  internal class RtspRequest
  {
    private RtspMethod _method = null;
    private string _uri = null;
    private int _majorVersion = 1;
    private int _minorVersion = 0;
    private IDictionary<string, string> _headers = new Dictionary<string, string>();
    private string _body = string.Empty;

    /// <summary>
    /// Initialise a new instance of the <see cref="RtspRequest"/> class.
    /// </summary>
    /// <param name="method">The request method.</param>
    /// <param name="url">The request URI</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The minor version number.</param>
    public RtspRequest(RtspMethod method, string uri, int majorVersion = 1, int minorVersion = 0)
    {
      _method = method;
      _uri = uri;
      _majorVersion = majorVersion;
      _minorVersion = minorVersion;
    }

    /// <summary>
    /// Get the request method.
    /// </summary>
    public RtspMethod Method
    {
      get
      {
        return _method;
      }
    }

    /// <summary>
    /// Get the request URI.
    /// </summary>
    public string Uri
    {
      get
      {
        return _uri;
      }
    }

    /// <summary>
    /// Get the request major version number.
    /// </summary>
    public int MajorVersion
    {
      get
      {
        return _majorVersion;
      }
    }

    /// <summary>
    /// Get the request minor version number.
    /// </summary>
    public int MinorVersion
    {
      get
      {
        return _minorVersion;
      }
    }

    /// <summary>
    /// Get or set the request headers.
    /// </summary>
    public IDictionary<string, string> Headers
    {
      get
      {
        return _headers;
      }
      set
      {
        _headers = value;
      }
    }

    /// <summary>
    /// Get or set the request body.
    /// </summary>
    public string Body
    {
      get
      {
        return _body;
      }
      set
      {
        _body = value;
      }
    }

    /// <summary>
    /// Serialise this request.
    /// </summary>
    /// <returns>raw request bytes</returns>
    public byte[] Serialise()
    {
      StringBuilder request = new StringBuilder();
      request.AppendFormat("{0} {1} RTSP/{2}.{3}\r\n", _method, _uri, _majorVersion, _minorVersion);
      foreach (KeyValuePair<string, string> header in _headers)
      {
        request.AppendFormat("{0}: {1}\r\n", header.Key, header.Value);
      }
      request.AppendFormat("\r\n{0}", _body);
      return System.Text.Encoding.UTF8.GetBytes(request.ToString());
    }
  }
}
