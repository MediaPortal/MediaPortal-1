#region Copyright (C) 2007-2008 Team MediaPortal

/* 
 *	Copyright (C) 2007-2008 Team MediaPortal
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

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{
  /// <summary>
  /// Public interface to PowerScheduler power control
  /// Intented to be used by singleseat setups
  /// </summary>
  public interface IPowerController
  {
    /// <summary>
    /// Requests suspension of the system. Uses default action.
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
    /// Enables clients on singleseat setups to indicate whether or not the system
    /// is allowed to enter standby
    /// </summary>
    /// <param name="standbyAllowed">is standby allowed?</param>
    /// <param name="handlerName">client handlername which prevents standby</param>
    void SetStandbyAllowed(bool standbyAllowed, string handlerName);

    /// <summary>
    /// Enables clients on singleseat setups to indicate when the next
    /// earliest wakeup time is due
    /// </summary>
    /// <param name="nextWakeupTime">DateTime when to wakeup the system</param>
    /// <param name="handlerName">client handlername which is responsible for this wakeup time</param>
    void SetNextWakeupTime(DateTime nextWakeupTime, string handlerName);

    /// <summary>
    /// Resets the idle timer of the PowerScheduler. When enough time has passed (IdleTimeout), the system
    /// is suspended as soon as possible (no handler disallows shutdown).
    /// Note that the idle timer is automatically reset to now when the user moves the mouse or touchs the keyboard.
    /// </summary>
    void UserActivityDetected(DateTime when);

    /// <summary>
    /// Register remote handlers. If an empty string or null is passed, no handler is registered for
    /// that type. It returns a tag used to unregister the later. The returned tag is always not 0.
    /// </summary>
    /// <param name="standbyHandlerURI"></param>
    /// <param name="wakeupHandlerURI"></param>
    int RegisterRemote(String standbyHandlerURI, String wakeupHandlerURI);

    /// <summary>
    /// Unregister remote handlers.
    /// </summary>
    void UnregisterRemote(int tag);

    /// <summary>
    /// Indicates whether or not we're connected to the PowerScheduler power control interfaces
    /// </summary>
    bool IsConnected { get; }

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
    /// Provides access to PowerScheduler's settings
    /// </summary>
    IPowerSettings PowerSettings { get; }
  }
}