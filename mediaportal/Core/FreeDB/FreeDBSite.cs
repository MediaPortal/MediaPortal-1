#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;

namespace MediaPortal.Freedb
{
	/// <summary>
	/// Summary description for FreeDBSite.
	/// </summary>
	public class FreeDBSite
	{
    public enum FreeDBProtocol {CDDB, HTTP};

    /// <summary>The hostname of the server. </summary>
    private string m_host;
    /// <summary>The protocol used by the server. </summary>
    private FreeDBProtocol m_proto;
    /// <summary>The port used by the server. </summary>
    private int m_port;
    /// <summary>The URI porting of the cgi if it is http; otherwise, "-". </summary>
    private string m_uri;
    /// <summary>The latitude of the server. </summary>
    private string m_latitude;
    /// <summary>The longitude of the server. </summary>
    private string m_longitude;
    /// <summary>The description of the the server's location. </summary>
    private string m_location;

    public FreeDBSite()
		{
		}

    public FreeDBSite(string host, FreeDBProtocol proto, int port, string uri,
                      string latitude, string longitude, string location)
    {
      m_host = host;
      m_proto = proto;
      m_port = port;
      m_uri = uri;
      m_latitude = latitude;
      m_longitude = longitude;
      m_location = location;
    }

    public string Host
    {
      get
      {
        return m_host;
      }
      set
      {
        m_host = value;
      }
    }

    public FreeDBProtocol Protocol
    {
      get
      {
        return m_proto;
      }
      set
      {
        m_proto = value;
      }
    }

    public int Port
    {
      get
      {
        return m_port;
      }
      set
      {
        m_port = value;
      }
    }
    
    public string URI
    {
      get
      {
        return m_uri;
      }
      set
      {
        m_uri = value;
      }
    }

    public string Latitude
    {
      get
      {
        return m_latitude;
      }
      set
      {
        m_latitude = value;
      }
    }

    public string Longitude
    {
      get
      {
        return m_longitude;
      }
      set
      {
        m_longitude = value;
      }
    }

    public string Location
    {
      get
      {
        return m_location;
      }
      set
      {
        m_location = value;
      }
    }
	}
}
