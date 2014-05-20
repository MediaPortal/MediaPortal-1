#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections.Generic;
using System.Management;
using TvDatabase;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Prevent standby if a (configured) share has open files
  /// </summary>
  public class ActiveSharesStandbyHandler : IStandbyHandler, IStandbyHandlerEx
  {
    #region Structs

    internal class ShareMonitor
    {

      internal enum ShareType
      {
        ShareOnly,                    // If anything is connected to the share, then prevent standby.
        UserOnly,                     // If a matching user is connected to any share, then prevent standby.
        ClientOnly,                     // If a matching client is connected to any share, then prevent standby.
        ClientUsingShare,               // If a matching client is connected to the matching share, then prevent standby.
        UserUsingShare,               // If a matching user is connected from any client to the matching share, then prevent standby.
        UserFromClientConnected,        // If a matching user is connected to any share from the define client, then prevent standby.
        UserFromClientUsingShare,       // All three fields must match to prevent standby.
        Any,                          // Any active share prevents standby.
      };

      string _share;
      string _client;
      string _user;

      internal readonly ShareType MonitoringType;

      internal ShareMonitor(string shareName, string clientName, string userName)
      {
        _share = shareName.Trim();
        _client = clientName.Trim();
        _user = userName.Trim();

        if (_share.Equals(string.Empty))
        {
          if (_client.Equals(string.Empty))
          {
            if (_user.Equals(string.Empty))
            {
              MonitoringType = ShareType.Any;
            }
            else
            {
              MonitoringType = ShareType.UserOnly;
            }
          }
          else if (_user.Equals(string.Empty))
          {
            MonitoringType = ShareType.ClientOnly;
          }
          else
          {
            MonitoringType = ShareType.UserFromClientConnected;
          }
        }
        else if (_client.Equals(string.Empty))
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
          MonitoringType = ShareType.ClientUsingShare;
        }
        else
        {
          MonitoringType = ShareType.UserFromClientUsingShare;
        }
        Log.Debug(LogType.PS, "ActiveSharesHandler: Monitor connections to share '{0}' from client '{1}' by user '{2}' (Type '{3}')", _share, _client, _user, MonitoringType);
      }

      internal bool Equals(ServerConnection serverConnection)
      {
        bool serverConnectionMatches = false;

        switch (MonitoringType)
        {
          case ShareType.Any:
            serverConnectionMatches = true;
            break;
          case ShareType.ShareOnly:
            if (serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.ClientOnly:
            if (serverConnection.ComputerName.Equals(_client, StringComparison.OrdinalIgnoreCase))
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
          case ShareType.ClientUsingShare:
            if (serverConnection.ComputerName.Equals(_client, StringComparison.OrdinalIgnoreCase) && serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.UserUsingShare:
            if (serverConnection.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase) && serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.UserFromClientConnected:
            if (serverConnection.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase) && serverConnection.ComputerName.Equals(_client, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
            break;
          case ShareType.UserFromClientUsingShare:
            if (serverConnection.UserName.Equals(_user, StringComparison.OrdinalIgnoreCase) && serverConnection.ComputerName.Equals(_client, StringComparison.OrdinalIgnoreCase) && serverConnection.ShareName.Equals(_share, StringComparison.OrdinalIgnoreCase))
            {
              serverConnectionMatches = true;
            }
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

    bool _enabled = false;
    List<ShareMonitor> _sharesToMonitor = new List<ShareMonitor>();

    /// <summary>
    /// Use away mode setting
    /// </summary>
    private bool _useAwayMode = false;

    #endregion

    #region Constructor

    public ActiveSharesStandbyHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent += new PowerSchedulerEventHandler(ProcessActiveHandler_OnPowerSchedulerEvent);
    }

    #endregion

    #region private methods

    private void ProcessActiveHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.Started:
        case PowerSchedulerEventType.Elapsed:

          IPowerScheduler ps = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
          if (ps == null)
            return;
          PowerSetting setting;
          TvBusinessLayer layer = new TvBusinessLayer();

          // Load share monitoring configuration for standby prevention 
          setting = ps.Settings.GetSetting("ActiveSharesEnabled");
          _enabled = Convert.ToBoolean(layer.GetSetting("PowerSchedulerActiveSharesEnabled", "false").Value);
          if (setting.Get<bool>() != _enabled)
          {
            setting.Set<bool>(_enabled);
            Log.Debug(LogType.PS, "ActiveSharesHandler: Monitoring active shares {0}", _enabled ? "enabled" : "disabled");
          }

          if (_enabled)
          {
            setting = ps.Settings.GetSetting("ActiveShares");
            string _connections = Convert.ToString(layer.GetSetting("PowerSchedulerActiveShares", "").Value);
            if (setting.Get<string>() != _connections)
            {
              setting.Set<string>(_connections);
              _sharesToMonitor.Clear();
              string[] shares = _connections.Split(';');
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
              if (_sharesToMonitor.Count == 0)
                _sharesToMonitor.Add(new ShareMonitor("", "", ""));
            }
          }

          // Check if away mode should be used
          setting = ps.Settings.GetSetting("ActiveSharesAwayMode");
          _useAwayMode = Convert.ToBoolean(layer.GetSetting("PowerSchedulerActiveSharesAwayMode", "false").Value);
          if (setting.Get<bool>() != _useAwayMode)
          {
            setting.Set<bool>(_useAwayMode);
            Log.Debug(LogType.PS, "ActiveSharesHandler: Use away mode: {0}", _useAwayMode);
          }

          break;
      }
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
        return (StandbyMode != StandbyMode.StandbyAllowed);
      }
    }

    public void UserShutdownNow() { }

    public string HandlerName
    {
      get { return "Active Shares"; }
    }

    #endregion

    #region IStandbyHandlerEx Members

    public StandbyMode StandbyMode
    {
      get
      {
        if (_enabled)
        {
          using (ManagementObjectSearcher searcher = new ManagementObjectSearcher
            ("SELECT ShareName, UserName, ComputerName, NumberOfFiles  FROM Win32_ServerConnection WHERE NumberOfFiles > 0"))
          {
            List<ServerConnection> connections = GetConnections(searcher.Get());

            // Inspect all active server connections against current setup (shares/clientuser combo's)
            foreach (ServerConnection connection in connections)
            {
              foreach (ShareMonitor shareBeingMonitored in _sharesToMonitor)
              {
                if (shareBeingMonitored.Equals(connection))
                {
                  Log.Debug(LogType.PS, "{0}: Standby is not allowed due to connection to share '{1}' from client '{2}' by user '{3}'", HandlerName, connection.ShareName, connection.ComputerName, connection.UserName);
                  return _useAwayMode ? StandbyMode.AwayModeRequested : StandbyMode.StandbyPrevented;
                }
              }
            }
            return StandbyMode.StandbyAllowed;
          }
        }
        return StandbyMode.StandbyAllowed;
      }
    }

    #endregion
  }
}