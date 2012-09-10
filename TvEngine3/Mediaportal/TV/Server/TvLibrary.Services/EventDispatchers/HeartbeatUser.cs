using System;
using System.Net;

namespace Mediaportal.TV.Server.TVLibrary.EventDispatchers
{
  public class HeartbeatUser
  {    
	  private DateTime _lastSeen;
	  private readonly string _name;

    public HeartbeatUser()
    {            
      _name = Dns.GetHostName();
      _lastSeen = DateTime.MinValue;
    }

	  public DateTime LastSeen
	  {
	    get { return _lastSeen; }
	    set { _lastSeen = value; }
	  }	  

	  public string Name
	  {
	    get { return _name; }
	  }
  }
}
