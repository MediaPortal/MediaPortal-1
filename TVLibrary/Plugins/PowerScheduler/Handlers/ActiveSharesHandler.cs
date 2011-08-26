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

#region Usings

using System;
using System.Management;
using System.Collections.Generic;
using System.Text;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvDatabase;
using TvLibrary.Log;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Prevent standby if a (configured) share has open files
  /// </summary>
  public class ActiveSharesHandler : IStandbyHandler
  {
    #region Structs

    internal class ShareMonitor
    {
      internal enum ShareType
      {
        ShareOnly, // If anything is connected to the share, then prevent standby.
        UserOnly, // If a matching user is connected to any share, then prevent standby.
        HostOnly, // If a matching host is connected to any share, then prevent standby.
        HostUsingShare, // If a matching host is connected to the matching share, then prevent standby.
        UserUsingShare, // If a matching user is connected from any host to the matching share, then prevent standby.
        UserFromHostConnected,
        // If a matching user is connected to any share from the define host, then prevent standby.
        UserFromHostUsingShare, // All three fields must match to prevent standby.
        Undefined, // Invalid share configuration. Do not prevent standby.
      } ;

      private string _share;
      private string _host;
      private string _user;

      internal readonly ShareType MonitoringType;

      internal ShareMonitor(string shareName, string hostName, string userName)
      {
        _share = shareName.Trim();
        _host = hostName.Trim();
        _user = userName.Trim();

        if (_share.Equals(string.Empty))
        {
          if (_host.Equals(string.Empty))
          {
            if (_user.Equals(string.Empty))
            {
              MonitoringType = ShareType.Undefined;
            }
            else
            {
              MonitoringType = ShareType.UserOnly;
            }
          }
          else if (_user.Equals(string.Empty))
          {
            MonitoringType = ShareType.HostOnly;
          }
          else
          {
            MonitoringType = ShareType.UserFromHostConnected;
          }
        }
        else if (_host.Equals(string.Empty))
        {
          if (_user.Equals(string.Empty))
          {
            MonitoringType = ShareType.ShareOnly;
          }
          else
          {
            MonitoringType = ShareType.UserUsingShare;
          }
        }
        else if (_user.Equals(string.Empty))
        {
          MonitoringType = ShareType.HostUsingShare;
        }
        else
        {
          MonitoringType = ShareType.UserFromHostUsingShare;
        }
        Log.Debug("ShareMonitor: Monitor user '{0}' from host '{1}' on share '{2}' Type '{3}'", _user, _host, _share,
                  MonitoringType);
      }

      internal bool Equals(ServerConnection serverConnection)
      {
        bool serverConnectionMatches = false;

        switch (MonitoringType)
        {
          case ShareType.ShareOnly:
            if (serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.HostOnly:
            if (serverConnection.ComputerName.Equals(_host, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.UserOnly:
            if (serverConnection.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.HostUsingShare:
            if (serverConnection.ComputerName.Equals(_host, StringComparison.OrdinalIgnoreCase) &&
                serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.UserUsingShare:
            if (serverConnection.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase) &&
                serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.UserFromHostConnected:
            if (serverConnection.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase) &&
                serverConnection.ComputerName.Equals(_host, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.UserFromHostUsingShare:
            if (serverConnection.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase) &&
                serverConnection.ComputerName.Equals(_host, StringComparison.OrdinalIgnoreCase) &&
                serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          default:
            Log.Debug("Invalid share monitoring configuration.");
            break;
        }
        return serverConnectionMatches;
      }
    }

    internal struct ServerConnection
    {
      public string ShareName;
      public string ComputerName;
      public string UserName;
      public int NumberOfFiles;

      public ServerConnection(string shareName, string computerName, string userName, int numFiles)
      {
        ShareName = shareName;
        ComputerName = computerName;
        UserName = userName;
        NumberOfFiles = numFiles;
      }
    }

    #endregion

    #region Variables

    private bool _enabled = false;
    private List<ShareMonitor> _sharesToMonitor = new List<ShareMonitor>();

    private ManagementObjectSearcher _searcher = new ManagementObjectSearcher(
      "SELECT ShareName, UserName, ComputerName, NumberOfFiles  FROM Win32_ServerConnection WHERE NumberOfFiles > 0");

    #endregion

    #region Constructor

    public ActiveSharesHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent +=
          new PowerSchedulerEventHandler(ProcessActiveHandler_OnPowerSchedulerEvent);
    }

    #endregion

    #region private methods

    private void ProcessActiveHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:
          _enabled = LoadSharesToMonitor();
          break;
      }
    }

    /// <summary>
    /// Read the share configuration data.
    /// </summary>
    /// <returns>true if share monitoring is enabled.</returns>
    private bool LoadSharesToMonitor()
    {
      try
      {
        _sharesToMonitor.Clear();

        TvBusinessLayer layer = new TvBusinessLayer();

        // Load share monitoring configuration for standby prevention 
        if (Convert.ToBoolean(layer.GetSetting("PreventStandybyWhenSharesInUse", "false").Value))
        {
          Setting setting = layer.GetSetting("PreventStandybyWhenSpecificSharesInUse", "");

          string[] shares = setting.Value.Split(';');
          foreach (string share in shares)
          {
            string[] shareItem = share.Split(',');
            if ((shareItem.Length.Equals(3)) &&
                ((shareItem[0].Trim().Length > 0) ||
                 (shareItem[1].Trim().Length > 0) ||
                 (shareItem[2].Trim().Length > 0)))
            {
              _sharesToMonitor.Add(new ShareMonitor(shareItem[0], shareItem[1], shareItem[2]));
            }
          }
          Log.Debug("{0}: Share monitoring is enabled.", HandlerName);
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("{0}: Error >{1}< loading shares to monitor", HandlerName, ex.Message);
      }
      Log.Debug("{0}: Share monitoring is disabled.", HandlerName);
      return false;
    }

    private List<ServerConnection> GetConnections(ManagementObjectCollection col)
    {
      List<ServerConnection> connections = new List<ServerConnection>();
      foreach (ManagementObject obj in col)
      {
        connections.Add(
          new ServerConnection(
            obj["ShareName"].ToString(),
            obj["ComputerName"].ToString(),
            obj["UserName"].ToString(),
            Int32.Parse(obj["NumberOfFiles"].ToString())
            )
          );
      }
      return connections;
    }

    #endregion

    #region IStandbyHandler implementation

    public bool DisAllowShutdown
    {
      get
      {
        if (_enabled)
        {
          List<ServerConnection> connections = GetConnections(_searcher.Get());

          // inspect all active server connections against current setup (shares/hostuser combo's)
          foreach (ServerConnection connection in connections)
          {
            foreach (ShareMonitor shareBeingMonitored in _sharesToMonitor)
            {
              if (shareBeingMonitored.Equals(connection))
              {
                Log.Debug("{0}: Standby cancelled due to connection '{1}:{2}' on share '{3}'", HandlerName,
                          connection.UserName, connection.ComputerName, connection.ShareName);
                return true;
              }
            }
          }
          Log.Debug("{0}: have not found any matching connections - will allow standby", HandlerName);
          return false;
        }
        return false;
      }
    }

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get { return "ActiveSharesHandler"; }
    }

    #endregion
  }
}