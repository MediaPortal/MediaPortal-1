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
using System.Collections.Generic;
using System.Text;
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
    /// Requests suspension of the system
    /// </summary>
    /// <param name="source">description of who wants to suspend the system</param>
    /// <param name="force">force the system to suspend (not recommended)</param>
    /// <returns>bool to indicate if the request was honoured</returns>
    bool SuspendSystem(string source, bool force);

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
    /// Indicates whether or not we're connected to the PowerScheduler power control interfaces
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Provides access to PowerScheduler's settings
    /// </summary>
    IPowerSettings PowerSettings { get; }
  }
}
