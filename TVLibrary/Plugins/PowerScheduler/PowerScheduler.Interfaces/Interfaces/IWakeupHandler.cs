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
using System.Collections.Generic;
using System.Text;
#endregion

namespace TvEngine.PowerScheduler.Interfaces
{
  /// <summary>
  /// To be implemented by classes who want to control system resume
  /// </summary>
  public interface IWakeupHandler
  {
    /// <summary>
    /// Should return the earliest time the implementation desires to wake up the system.
    /// </summary>
    /// <param name="earliestWakeupTime">indicates the earliest valid wake up time that is considered valid by the PowerScheduler</param>
    /// <returns>earliest time the implementation wants to wake up the system</returns>
    DateTime GetNextWakeupTime(DateTime earliestWakeupTime);

    /// <summary>
    /// Description of the source that want to wake up the system at the given time
    /// </summary>
    string HandlerName { get; }
  }
}
