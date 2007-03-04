#region Copyright (C) 2007 Team MediaPortal
/* 
 *	Copyright (C) 2007 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Management;
using TvControl;
using TvDatabase;
using TvService;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;
using TvEngine;
#endregion

namespace TvEngine.PowerScheduler
{
  /// <summary>
  /// Factory for creating various IStandbyHandlers/IWakeupHandlers
  /// </summary>
  public class PowerSchedulerFactory
  {
    #region Variables
    /// <summary>
    /// List of all standby handlers
    /// </summary>
    List<IStandbyHandler> _standbyHandlers;
    /// <summary>
    /// List of all wakeup handlers
    /// </summary>
    List<IWakeupHandler> _wakeupHandlers;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new PowerSchedulerFactory
    /// </summary>
    /// <param name="controller">Reference to tvservice's TVController</param>
    public PowerSchedulerFactory(IController controller)
    {
      IStandbyHandler standbyHandler;
      IWakeupHandler wakeupHandler;

      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();

      // Add handlers for preventing the system from entering standby
      standbyHandler = new ActiveStreamsHandler(controller);
      _standbyHandlers.Add(standbyHandler);
      standbyHandler = new ControllerActiveHandler(controller);
      _standbyHandlers.Add(standbyHandler);
      //standbyHandler = new EpgGrabbingHandler(controller);
      //_standbyHandlers.Add(standbyHandler);
      standbyHandler = new SetupActiveHandler();
      _standbyHandlers.Add(standbyHandler);

      // Add handlers for resuming from standby
      wakeupHandler = new ScheduledRecordingsHandler();
      _wakeupHandlers.Add(wakeupHandler);
      //wakeupHandler = new TestWakeupHandler();
      //_wakeupHandlers.Add(wakeupHandler);
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Create/register the default set of standby/wakeup handlers
    /// </summary>
    public void CreateDefaultSet()
    {
      IPowerScheduler powerScheduler = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      foreach (IStandbyHandler handler in _standbyHandlers)
        powerScheduler.Register(handler);
      foreach (IWakeupHandler handler in _wakeupHandlers)
        powerScheduler.Register(handler);
    }

    /// <summary>
    /// Unregister the default set of standby/wakeup handlers
    /// </summary>
    public void RemoveDefaultSet()
    {
      IPowerScheduler powerScheduler = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      foreach (IStandbyHandler handler in _standbyHandlers)
        powerScheduler.Unregister(handler);
      foreach (IWakeupHandler handler in _wakeupHandlers)
        powerScheduler.Unregister(handler);
    }
    #endregion

  }

  #region IStandbyHandler implementations

  public class ActiveStreamsHandler : IStandbyHandler
  {
    IController _controller;
    public ActiveStreamsHandler(IController controller)
    {
      _controller = controller;
    }
    public bool DisAllowShutdown
    {
      get { return (_controller.ActiveStreams > 0); }
    }
    public string HandlerName
    {
      get { return "ActiveStreamsHandler"; }
    }
  }

  public class EpgGrabbingHandler : IStandbyHandler
  {
    IController _controller;
    public EpgGrabbingHandler(IController controller)
    {
      _controller = controller;
    }
    public bool DisAllowShutdown
    {
      get
      {
        for (int i = 0; i < _controller.Cards; i++)
        {
          int cardId = _controller.CardId(i);
          if (_controller.IsGrabbingEpg(cardId))
            return true;
        }
        return false;
      }
    }
    public string HandlerName
    {
      get { return "EpgGrabbingHandler"; }
    }
  }

  public class SetupActiveHandler : IStandbyHandler
  {
    public bool DisAllowShutdown
    {
      get
      {
        System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("SetupTv");
        if (processes.Length > 0)
          return true;
        return false;
      }
    }
    public string HandlerName
    {
      get { return "SetupActiveHandler"; }
    }
  }

  public class ControllerActiveHandler : IStandbyHandler
  {
    TVController _controller;
    public ControllerActiveHandler(IController controller)
    {
      _controller = controller as TVController;
    }
    public TVController Controller
    {
      get { return _controller; }
      set { _controller = value; }
    }
    public bool DisAllowShutdown
    {
      get
      {
        if (_controller.CanSuspend)
          return false;
        return true;
      }
    }
    public string HandlerName
    {
      get { return "ControllerActiveHandler"; }
    }
  }

  public class GenericStandbyHandler : IStandbyHandler
  {
    #region Variables
    private int _timeout = 60;
    private bool _disAllowShutdown = false;
    private DateTime _lastUpdate = DateTime.MinValue;
    private string _handlerName = "GenericStandbyHandler";
    #endregion
    #region Constructor
    /// <summary>
    /// Create a new instance of a generic standby handler
    /// </summary>
    /// <param name="standbyIdleTimeout">Configured standby idle timeout</param>
    public GenericStandbyHandler() : this(5) { }
    public GenericStandbyHandler(int standbyIdleTimeout)
    {
      SetIdleTimeout(standbyIdleTimeout);
    }
    #endregion
    #region Public methods
    public void SetIdleTimeout(int standbyIdleTimeout)
    {
      _timeout = standbyIdleTimeout;
    }
    #endregion
    #region IStandbyHandler implementation
    public bool DisAllowShutdown
    {
      get
      {
        // Check if last update + timeout was earlier than
        // the current time; if so, ignore this handler!
        if (_lastUpdate.AddMinutes(_timeout) < DateTime.Now)
        {
          return false;
        }
        else
        {
          return _disAllowShutdown;
        }
      }
      set
      {
        _lastUpdate = DateTime.Now;
        _disAllowShutdown = value;
      }
    }
    public string HandlerName
    {
      get { return _handlerName; }
      set { _handlerName = value; }
    }
    #endregion
  }

  public class ActiveSharesHandler : IStandbyHandler
  {
    #region Structs
    struct HostUserCombo
    {
      public string Host;
      public string User;
      public HostUserCombo(string host) : this(host, String.Empty) { }
      public HostUserCombo(string host, string user)
      {
        User = user;
        Host = host;
      }
    }
    struct ServerConnection
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
    List<string> _shares = new List<string>();
    List<HostUserCombo> _combos = new List<HostUserCombo>();
    ManagementObjectSearcher _searcher = new ManagementObjectSearcher(
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
    public string HandlerName
    {
      get { return "ActiveSharesHandler"; }
    }
    #endregion
  }

  #endregion

  #region IWakeupHandler implementations

  public class ScheduledRecordingsHandler : IWakeupHandler
  {
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      DateTime scheduleWakeupTime;
      DateTime nextWakeuptime = DateTime.MaxValue;
      foreach (Schedule schedule in Schedule.ListAll())
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        scheduleWakeupTime = schedule.StartTime.AddMinutes(-schedule.PreRecordInterval);
        if (scheduleWakeupTime < nextWakeuptime && scheduleWakeupTime >= earliestWakeupTime)
          nextWakeuptime = scheduleWakeupTime;
      }
      return nextWakeuptime;
    }
    public string HandlerName
    {
      get { return "ScheduledRecordingsHandler"; }
    }
  }

  public class TestWakeupHandler : IWakeupHandler
  {
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      return earliestWakeupTime.AddMinutes(5);
    }
    public string HandlerName
    {
      get { return "TestWakeupHandler"; }
    }
  }

  public class GenericWakeupHandler : IWakeupHandler
  {
    #region Variables
    private DateTime _nextWakeupTime = DateTime.MaxValue;
    private string _handlerName = "GenericWakeupHandler";
    #endregion
    #region Public methods
    public void Update(DateTime nextWakeuptime, string handlerName)
    {
      _nextWakeupTime = nextWakeuptime;
      _handlerName = handlerName;
    }
    #endregion
    #region IWakeupHandler implementation
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      return _nextWakeupTime;
    }
    public string HandlerName
    {
      get { return _handlerName; }
    }
    #endregion
  }

  #endregion
}
