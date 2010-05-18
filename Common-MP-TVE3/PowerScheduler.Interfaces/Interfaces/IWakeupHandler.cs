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