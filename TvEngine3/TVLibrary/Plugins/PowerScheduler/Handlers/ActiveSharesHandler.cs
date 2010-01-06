#region Copyright (C) 2007-2009 Team MediaPortal

/* 
 *	Copyright (C) 2007-2009 Team MediaPortal
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

#region Usings

using System;
using System.Management;
using System.Collections.Generic;
using System.Text;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Prevent standby if a (configured) share has open files
  /// </summary>
  public class ActiveSharesHandler : IStandbyHandler
  {
    #region Structs

    private struct HostUserCombo
    {
      public string Host;
      public string User;
      public HostUserCombo(string host) : this(host, String.Empty) {}

      public HostUserCombo(string host, string user)
      {
        User = user;
        Host = host;
      }
    }

    private struct ServerConnection
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

    private List<string> _shares = new List<string>();
    private List<HostUserCombo> _combos = new List<HostUserCombo>();

    private ManagementObjectSearcher _searcher = new ManagementObjectSearcher(
      "SELECT ShareName, UserName, ComputerName, NumberOfFiles  FROM Win32_ServerConnection WHERE NumberOfFiles > 0");

    #endregion

    #region Public methods

    /// <summary>
    /// Adds a valid sharename to watch. If no shares have been added, all shares will be monitored.
    /// </summary>
    /// <param name="shareName">sharename to watch</param>
    public void AddShare(string shareName)
    {
      if (!_shares.Contains(shareName))
        _shares.Add(shareName);
    }

    /// <summary>
    /// Adds a host which can keep the server alive when it has open files on the server.
    /// If no hosts or host/user combinations have been added, all users can keep the server alive
    /// with open files.
    /// </summary>
    /// <param name="host">host to add</param>
    public void AddHost(string host)
    {
      AddHostUserCombo(host, String.Empty);
    }

    /// <summary>
    /// Adds a host/user combination which can keep the server alive if it has open files on the
    /// server. For example: if you do AddHostUserCombo("Bar", "Foo"); then only user "Foo" on
    /// the computer named "Bar" can keep the server alive if they have open files via a network
    /// share.
    /// </summary>
    /// <param name="host">host to add</param>
    /// <param name="user">user to add</param>
    public void AddHostUserCombo(string host, string user)
    {
      _combos.Add(new HostUserCombo(host, user));
    }

    /// <summary>
    /// Clears both the share list and host/user list. After this call all sessions which have
    /// open files can keep the server from entering standby.
    /// </summary>
    public void Clear()
    {
      _shares.Clear();
      _combos.Clear();
    }

    #endregion

    #region private methods

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
        bool activeServerConnections = false;
        bool allowed;
        List<ServerConnection> connections = GetConnections(_searcher.Get());
        // Check if there are no sessions at all with NumberOfFiles > 0
        if (connections.Count == 0)
          return false;
        // inspect all active server connections against current setup (shares/hostuser combo's)
        foreach (ServerConnection connection in connections)
        {
          // check if we should filter by sharename
          if (_shares.Count > 0)
          {
            allowed = false;
            foreach (string share in _shares)
              if (connection.ShareName.Equals(share, StringComparison.CurrentCultureIgnoreCase))
              {
                allowed = true;
                break;
              }
            if (!allowed) continue;
          }
          // check if we should filter by host/user combo
          if (_combos.Count > 0)
          {
            allowed = false;
            foreach (HostUserCombo combo in _combos)
              if (connection.ComputerName.Equals(combo.Host, StringComparison.CurrentCultureIgnoreCase))
              {
                if (combo.User.Equals(String.Empty))
                {
                  allowed = true;
                  break;
                }
                else if (connection.UserName.Equals(combo.User, StringComparison.CurrentCultureIgnoreCase))
                {
                  allowed = true;
                  break;
                }
              }
            if (!allowed) continue;
          }
          // if we got here then we should regard this ServerConnection as active
          activeServerConnections = true;
        }
        return activeServerConnections;
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