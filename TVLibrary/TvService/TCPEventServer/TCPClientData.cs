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
using System.Net;
using System.Net.Sockets;

namespace TvService
{	
	public class TCPClientData
	{
	  private TcpClient _client;		
	  private DateTime _lastSeen;
	  private string _name;

    public TCPClientData (TcpClient client)
    {
      _client = client;
      IPAddress ipAddress = ((IPEndPoint)(client.Client.RemoteEndPoint)).Address;
      IPHostEntry ipHostEntry = Dns.GetHostEntry(ipAddress);
      _name = ipHostEntry.HostName;
    }

	  public DateTime LastSeen
	  {
	    get { return _lastSeen; }
	    set { _lastSeen = value; }
	  }

	  public TcpClient Client
	  {
	    get { return _client; }
      set { _client = value; }
	  }

	  public string Name
	  {
	    get { return _name; }
	  }
	}
}
