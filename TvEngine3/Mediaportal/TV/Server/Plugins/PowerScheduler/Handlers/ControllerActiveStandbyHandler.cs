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
using System.Runtime.CompilerServices;
using System.Threading;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVControl.Events;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVService.Interfaces.CardHandler;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using TvEngine.PowerScheduler.Interfaces;


#endregion

namespace Mediaportal.TV.Server.Plugins.PowerScheduler.Handlers
{
  /// <summary>
  /// Handles standby when the TVController is recording or timeshifting
  /// </summary>
  public class ControllerActiveStandbyHandler : IStandbyHandler, IStandbyHandlerEx
  {
    #region Variables

    /// <summary>
    /// Reference to tvservice's TVController
    /// </summary>
    private IInternalControllerService _controllerService;

    /// <summary>
    /// Handler name to be returned
    /// </summary>
    private string _handlerName;

    #endregion

    #region Constructor

    public ControllerActiveStandbyHandler(IInternalControllerService controllerService)
    {
      // Save controller
      _controllerService = controllerService;

      // Register handler for TV server events
      _controllerService.OnTvServerEvent += OnTvServerEvent;
    }

    ~ControllerActiveStandbyHandler()
    {
      // Unregister handler for TV server events
      _controllerService.OnTvServerEvent -= OnTvServerEvent;
    }

    #endregion

    #region IStandbyHandler Implementation

    public bool DisAllowShutdown
    {
      get
      {
        return (StandbyMode != StandbyMode.StandbyAllowed);
      }
    }

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get { return _handlerName; }
    }

    #endregion

    #region IStandbyHandlerEx Members

    public StandbyMode StandbyMode
    {
      get
      {
        return GetStandbyMode;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Check if any card is recording or timeshifting
    /// </summary>
    private StandbyMode GetStandbyMode
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        _handlerName = "TV Controller";
        bool isTimeShifting = false;

        if (_controllerService == null)
          return StandbyMode.StandbyAllowed;

        // Check all cards
        IEnumerator<KeyValuePair<int, ITvCardHandler>> enumer = _controllerService.CardCollection.GetEnumerator();
        while (enumer.MoveNext())
        {
          ITvCardHandler card = enumer.Current.Value;
          if (card == null)
            continue;

          // Check if user is recording
          if (card.Recorder.IsAnySubChannelRecording)
          {
            _handlerName = "TV Controller (Recording)";
            return StandbyMode.AwayModeRequested;
          }

          // Check if user is timeshifting
          IDictionary<string, IUser> users = card.UserManagement.UsersCopy;
          foreach (IUser user in users.Values)
          {
            if (card.TimeShifter.IsTimeShifting(user))
            {
              _handlerName = "TV Controller (Local timeshifting)";
              isTimeShifting = true;
              if (!PowerManager.IsLocal(user.Name))
              {
                // Timeshifting to a remote client 
                _handlerName = "TV Controller (Remote timeshifting)";
                return StandbyMode.AwayModeRequested;
              }
            }
          }
        }

        // Found local timeshifting?
        return isTimeShifting ? StandbyMode.StandbyPrevented : StandbyMode.StandbyAllowed;
      }
    }

    /// <summary>
    /// Triggers PowerScheduler on recording and timeshifting events to check standby conditions
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnTvServerEvent(object sender, EventArgs eventArgs)
    {
      TvServerEventArgs tvArgs = eventArgs as TvServerEventArgs;
      if (eventArgs == null)
        return;
      if (tvArgs != null)
      {
        switch (tvArgs.EventType)
        {
          case TvServerEventType.RecordingStarted:
          case TvServerEventType.RecordingEnded:
          case TvServerEventType.StartTimeShifting:
          case TvServerEventType.EndTimeShifting:

            // Trigger PowerScheduler's StandbyWakeupThread to check standby conditions
            EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "TvEngine.PowerScheduler.StandbyWakeupTriggered");
            eventWaitHandle.Set();
            break;
        }
      }
    }

    #endregion
  }
}