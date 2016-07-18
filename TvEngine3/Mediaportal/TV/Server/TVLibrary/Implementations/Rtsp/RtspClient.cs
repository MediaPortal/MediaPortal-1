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
using System.Net.Sockets;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp
{
  /// <summary>
  /// A simple implementation of an RTSP client.
  /// </summary>
  internal class RtspClient : IDisposable
  {
    #region variables

    private string _serverHost = null;
    private int _serverPort = -1;
    private TcpClient _client = null;
    private int _cseq = 1;
    private object _lockObject = new object();

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="RtspClient"/> class.
    /// </summary>
    /// <param name="serverHost">The RTSP server host name or IP address.</param>
    /// <param name="serverPort">The port on which the RTSP server is listening.</param>
    public RtspClient(string serverHost, int serverPort = 554)
    {
      _serverHost = serverHost;
      _serverPort = serverPort;
    }

    ~RtspClient()
    {
      Dispose(false);
    }

    /// <summary>
    /// Send an RTSP request and retrieve the response.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="response">The response.</param>
    /// <returns>the response status code</returns>
    public RtspStatusCode SendRequest(RtspRequest request, out RtspResponse response)
    {
      response = null;
      lock (_lockObject)
      {
        NetworkStream stream = null;
        int retryCount = 0;
        while (true)
        {
          try
          {
            if (_client == null)
            {
              _client = new TcpClient(_serverHost, _serverPort);
            }
            else if (!_client.Connected)
            {
              _client.Connect(_serverHost, _serverPort);
            }
          }
          catch (Exception ex)
          {
            this.LogError(ex, "RTSP: failed to connect to server, host = {0}, port = {1}", _serverHost, _serverPort);
            return RtspStatusCode.RequestTimeOut;
          }
          try
          {
            stream = _client.GetStream();
            if (stream == null)
            {
              throw new Exception();
            }
            break;
          }
          catch (Exception ex)
          {
            _client.Close();
            _client = null;
            if (retryCount == 1)
            {
              this.LogError(ex, "RTSP: failed to open stream to server, host = {0}, port = {1}", _serverHost, _serverPort);
              return RtspStatusCode.RequestTimeOut;
            }
            retryCount++;
          }
        }

        try
        {
          // Send the request and get the response.
          request.Headers.Add("CSeq", _cseq.ToString());
          _cseq++;
          byte[] requestBytes = request.Serialise();
          stream.Write(requestBytes, 0, requestBytes.Length);
          byte[] responseBytes = new byte[_client.ReceiveBufferSize];
          int byteCount = stream.Read(responseBytes, 0, responseBytes.Length);
          response = RtspResponse.Deserialise(responseBytes, byteCount);

          // Did we get the whole response?
          string contentLengthString;
          int contentLength = 0;
          if (response.Headers.TryGetValue("Content-Length", out contentLengthString))
          {
            contentLength = int.Parse(contentLengthString);
            if ((string.IsNullOrEmpty(response.Body) && contentLength > 0) || response.Body.Length < contentLength)
            {
              if (response.Body == null)
              {
                response.Body = string.Empty;
              }
              while (byteCount > 0 && response.Body.Length < contentLength)
              {
                byteCount = stream.Read(responseBytes, 0, responseBytes.Length);
                response.Body += System.Text.Encoding.UTF8.GetString(responseBytes, 0, byteCount);
              }
            }
          }
        }
        finally
        {
          stream.Close();
          stream.Dispose();
        }

        // Should we close the connection?
        string connectionString;
        if (
          response.Headers.TryGetValue("Connection", out connectionString) &&
          string.Equals("close", connectionString, StringComparison.InvariantCultureIgnoreCase)
        )
        {
          _client.Close();
          _client = null;
        }
        return response.StatusCode;
      }
    }

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        lock (_lockObject)
        {
          if (_client != null)
          {
            _client.Close();
            _client = null;
          }
        }
      }
    }

    #endregion
  }
}