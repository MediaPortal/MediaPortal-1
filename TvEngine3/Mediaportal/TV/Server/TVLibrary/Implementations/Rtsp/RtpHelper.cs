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

using System.Net;
using System.Net.Sockets;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Rtsp
{
  /// <summary>
  /// A simple class that can be used to deserialise RTSP responses.
  /// </summary>
  internal static class RtpHelper
  {
    public static string ConstructUrl(IPAddress localIpAddress, int localPort, string serverAddress, string serverPort)
    {
      // Refer to WinINet IPv6 Support:
      // https://docs.microsoft.com/en-us/windows/desktop/wininet/ip-version-6-support
      // - address literals must be surrounded with square brackets
      // - the percentage character prefix for the scope ID must be encoded using %25
      string localAddress = localIpAddress.ToString();
      if (localIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
      {
        localAddress = string.Format("[{0}]", localAddress.Replace("%", "%25"));
      }

      try
      {
        IPAddress serverIpAddress = IPAddress.Parse(serverAddress);
        if (serverIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
          serverAddress = string.Format("[{0}]", serverIpAddress.ToString().Replace("%", "%25"));
        }
      }
      catch
      {
        // Presumably the server address is a hostname.
      }

      if (string.IsNullOrEmpty(serverPort) || serverPort.Equals("0"))
      {
        return string.Format("rtp://{0}@{1}:{2}", serverAddress, localAddress, localPort);
      }
      return string.Format("rtp://{0}:{1}@{2}:{3}", serverAddress, serverPort, localAddress, localPort);
    }
  }
}