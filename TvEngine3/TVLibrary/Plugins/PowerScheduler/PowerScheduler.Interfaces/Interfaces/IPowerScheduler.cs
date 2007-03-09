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
    /// Requests suspension of the system
    /// </summary>
    /// <param name="source">description of who wants to suspend the system</param>
    /// <param name="force">force the system to suspend (not recommended)</param>
    bool SuspendSystem(string source, bool force);

    /// <summary>
    /// Provides access to PowerScheduler's settings
    /// </summary>
    PowerSettings Settings { get; }
  }
}
