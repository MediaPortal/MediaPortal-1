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
using TvEngine.PowerScheduler.Interfaces;


#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Prevent standby if a remote (multi-seat) client is active (used by PowerScheduler client plugin)
  /// </summary>
  public class RemoteClientStandbyHandler : IStandbyHandler, IStandbyHandlerEx
  {
    #region Variables

    /// <summary>
    /// Seconds after which a remote client not signalling activity is considered inactive 
    /// </summary>
    private int _timeout = 120;

    /// <summary>
    /// Indicator if a remote client is active
    /// </summary>
    private StandbyMode _standbyMode = StandbyMode.StandbyAllowed;

    /// <summary>
    /// Last time a remote client signaled activity
    /// </summary>
    private DateTime _lastUpdate = DateTime.MinValue;

    #endregion

    #region Constructor

    public RemoteClientStandbyHandler()
    {
    }

    ~RemoteClientStandbyHandler()
    {
    }

    #endregion

    #region IStandbyHandler implementation

    public bool DisAllowShutdown
    {
      get
      {
        return StandbyMode != StandbyMode.StandbyAllowed;
      }
      set
      {
        StandbyMode = value ? StandbyMode.AwayModeRequested : StandbyMode.StandbyAllowed;
      }
    }

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get { return "Remote Client"; }
    }

    #endregion

    #region IStandbyHandlerEx implementation

    public StandbyMode StandbyMode
    {
      get
      {
        // Check if last update was longer ago than timeout
        // If so, do not prevent standby any longer
        if (_lastUpdate.AddSeconds(_timeout) < DateTime.Now)
        {
          _standbyMode = StandbyMode.StandbyAllowed;
        }
        return _standbyMode;
      }
      set
      {
        _lastUpdate = DateTime.Now;
        _standbyMode = value;
      }
    }
    #endregion
  }
}