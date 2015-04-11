using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Util
{
  public static class Network
  {
    private static bool? _isSingleSeat;

    /// <summary>
    ///  Checks if the current hostname/IP address is a singleseat setup
    /// </summary>    
    /// <returns>is the current name/address a representation of a singleseat installation</returns>
    public static bool IsSingleSeat()
    {
      if (!_isSingleSeat.HasValue)
      {
        if (Utils.UsingTvServer)
        {
          string serverName;
          using (Settings reader = new MPSettings())
          {
            serverName = reader.GetValueAsString("tvservice", "hostname", String.Empty);
          }

          if (serverName.Equals(Environment.MachineName.ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase))          
          {
            Log.Debug("Common.IsSingleSeat: HostName = {0} / Environment.MachineName = {1}",
                      serverName, Environment.MachineName);
            _isSingleSeat = true;
          }
          else
          {
            Log.Debug("Common.IsSingleSeat: checking if '{0}' is singleseat...", serverName);
            string hostName = Dns.GetHostName();
            var hosts = new[] { "localhost", "127.0.0.1", hostName };
            
            IsHostNameSingleSeat(serverName, hosts);

            if (!_isSingleSeat.GetValueOrDefault(false))
            {
              IPHostEntry hostEntry = Dns.GetHostEntry(hostName);              
              IsHostNameSingleSeat(serverName, hostEntry.AddressList.Select(ipAddress => ipAddress.ToString()).ToList());                        
            }            
          }
        }        
      }
      return _isSingleSeat.GetValueOrDefault(false);
    }

    public static bool IsSingleSeat(string serverName)
    {
      if (!_isSingleSeat.HasValue)
      {
        if (Utils.UsingTvServer)
        {
          if (serverName.Equals(Environment.MachineName.ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase))
          {
            Log.Debug("Common.IsSingleSeat: HostName = {0} / Environment.MachineName = {1}",
                      serverName, Environment.MachineName);
            _isSingleSeat = true;
          }
          else
          {
            Log.Debug("Common.IsSingleSeat: checking if '{0}' is singleseat...", serverName);
            string hostName = Dns.GetHostName();
            var hosts = new[] { "localhost", "127.0.0.1", hostName };

            IsHostNameSingleSeat(serverName, hosts);

            if (!_isSingleSeat.GetValueOrDefault(false))
            {
              IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
              IsHostNameSingleSeat(serverName, hostEntry.AddressList.Select(ipAddress => ipAddress.ToString()).ToList());
            }
          }
        }
      }
      return _isSingleSeat.GetValueOrDefault(false);
    }

    private static void IsHostNameSingleSeat(string serverName, IEnumerable<string> hosts)
    {
      foreach (string name in hosts)
      {                            
        _isSingleSeat = (serverName.Equals(name, StringComparison.CurrentCultureIgnoreCase));              
        Log.Debug("Common.IsSingleSeat:  Checking against {0} - result={1}", name, _isSingleSeat);
        if (_isSingleSeat.GetValueOrDefault(false))
        {
          break;
        }
      }
    }

    public static void Reset()
    {
      _isSingleSeat = null;
    }
  }
}
