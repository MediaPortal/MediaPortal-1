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
