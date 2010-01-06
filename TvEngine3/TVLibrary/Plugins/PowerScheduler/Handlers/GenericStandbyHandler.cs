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

#region Usings

using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Generic IStandbyHandler implementation, can be reused or inherited
  /// </summary>
  public class GenericStandbyHandler : IStandbyHandler
  {
    #region Variables

    private int _timeout = 5;
    private bool _disAllowShutdown = false;
    private DateTime _lastUpdate = DateTime.MinValue;
    private string _handlerName = "GenericStandbyHandler";

    #endregion

    #region Constructor

    /// <summary>
    /// Create a new instance of a generic standby handler
    /// </summary>
    public GenericStandbyHandler()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<IPowerScheduler>())
        GlobalServiceProvider.Instance.Get<IPowerScheduler>().OnPowerSchedulerEvent +=
          new PowerSchedulerEventHandler(GenericStandbyHandler_OnPowerSchedulerEvent);
    }

    /// <summary>
    /// Handles PowerScheduler event messages.
    /// Used to keep track of changes to the idle timeout
    /// </summary>
    /// <param name="args">PowerSchedulerEventArgs for a specific message</param>
    private void GenericStandbyHandler_OnPowerSchedulerEvent(PowerSchedulerEventArgs args)
    {
      switch (args.EventType)
      {
        case PowerSchedulerEventType.SettingsChanged:
          PowerSettings settings = args.GetData<PowerSettings>();
          if (settings != null)
            _timeout = settings.IdleTimeout;
          break;
      }
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

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get { return _handlerName; }
      set { _handlerName = value; }
    }

    #endregion
  }
}