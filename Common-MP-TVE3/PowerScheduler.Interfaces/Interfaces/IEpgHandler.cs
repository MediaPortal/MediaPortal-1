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
  public delegate void EPGScheduleHandler();

  /// <summary>
  /// Provides access to PowerScheduler's EPG interface for
  /// EPG plugins.
  /// </summary>
  public interface IEpgHandler
  {
    /// <summary>
    /// Allows an external EPG source to prevent standby when it's
    /// grabbing EPG
    /// </summary>
    /// <param name="source">the source preventing standby</param>
    /// <param name="allowed">is standby allowed?</param>
    void SetStandbyAllowed(object source, bool allowed, int timeout);

    /// <summary>
    /// Allows an external EPG source to set a preferred next wakeup time
    /// </summary>
    /// <param name="source">the source that wants to wakeup the system for EPG grabbing</param>
    /// <param name="time">the desired time to wakeup the system</param>
    void SetNextEPGWakeupTime(object source, DateTime time);

    /// <summary>
    /// Allows an external EPG source to retrieve the next wakeup time
    /// for EPG grabbing. This time can be used to start an actual EPG
    /// grabbing process.
    /// </summary>
    /// <returns>next EPG grab wakeup time</returns>
    DateTime GetNextEPGWakeupTime();

    /// <summary>
    /// Event which gets fired when the configured EPG wakeup schedule is due.
    /// </summary>
    event EPGScheduleHandler EPGScheduleDue;
  }
}