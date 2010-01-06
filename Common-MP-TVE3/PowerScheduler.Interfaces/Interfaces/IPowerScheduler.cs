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

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{
  /// <summary>
  /// EvenHandler delegate for PowerScheduler events
  /// </summary>
  /// <param name="args"></param>
  public delegate void PowerSchedulerEventHandler(PowerSchedulerEventArgs args);

  /// <summary>
  /// Interface to PowerScheduler
  /// </summary>
  public interface IPowerScheduler
  {
    /// <summary>
    /// Register to this event to receive status changes from the PowerScheduler
    /// </summary>
    event PowerSchedulerEventHandler OnPowerSchedulerEvent;

    /// <summary>
    /// Registers an IStandbyHandler implementation
    /// </summary>
    /// <param name="handler">implementation to register for handling standby requests</param>
    void Register(IStandbyHandler handler);

    /// <summary>
    /// Registers an IWakeupHandler implementation
    /// </summary>
    /// <param name="handler">implementation to register for handling system resume time</param>
    void Register(IWakeupHandler handler);

    /// <summary>
    /// Unregisters an IStandbyHandler implementation
    /// </summary>
    /// <param name="handler">implementation to unregister for handling standby requests</param>
    void Unregister(IStandbyHandler handler);

    /// <summary>
    /// Registers an IWakeupHandler implementation
    /// </summary>
    /// <param name="handler">implementation to register for handling system resume time</param>
    void Unregister(IWakeupHandler handler);

    /// <summary>
    /// Checks if the given IStandbyHandler is registered
    /// </summary>
    /// <param name="handler">IStandbyHandler to check</param>
    /// <returns>is the given handler registered?</returns>
    bool IsRegistered(IStandbyHandler handler);

    /// <summary>
    /// Checks if the given IWakeupHandler is registered
    /// </summary>
    /// <param name="handler">IWakeupHandler to check</param>
    /// <returns>is the given handler registered?</returns>
    bool IsRegistered(IWakeupHandler handler);

    /// <summary>
    /// Requests suspension of the system
    /// </summary>
    /// <param name="source">description of who wants to suspend the system</param>
    /// <param name="force">force the system to suspend (not recommended)</param>
    void SuspendSystem(string source, bool force);

    /// <summary>
    /// Requests suspension of the system. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="how">How to suspend, see MediaPortal.Util.RestartOptions</param>
    /// <param name="force"></param>
    void SuspendSystem(string source, int how, bool force);

    /// <summary>
    /// Resets the idle timer of the PowerScheduler. When enough time has passed (IdleTimeout), the system
    /// is suspended as soon as possible (no handler disallows shutdown).
    /// Note that the idle timer is automatically reset to now when the user moves the mouse or touchs the keyboard.
    /// </summary>
    void UserActivityDetected(DateTime when);

    /// <summary>
    /// Get the current state. If refresh is true, the state is the most current state, otherwise the state could be some seconds old.
    /// Special case: If shutdown is not allowed because an event is almost due, the handler name is "EVENT-DUE".
    /// </summary>
    /// <param name="refresh"></param>
    /// <param name="disAllowShutdown"></param>
    /// <param name="disAllowShutdownHandler"></param>
    /// <param name="nextWakeupTime"></param>
    /// <param name="nextWakeupHandler"></param>
    void GetCurrentState(bool refresh, out bool unattended, out bool disAllowShutdown,
                         out String disAllowShutdownHandler, out DateTime nextWakeupTime, out String nextWakeupHandler);


    /// <summary>
    /// Checks if a suspend request is in progress
    /// </summary>
    /// <returns>is the system currently trying to suspend?</returns>
    bool IsSuspendInProgress();

    /// <summary>
    /// Provides access to PowerScheduler's settings
    /// </summary>
    PowerSettings Settings { get; }
  }
}