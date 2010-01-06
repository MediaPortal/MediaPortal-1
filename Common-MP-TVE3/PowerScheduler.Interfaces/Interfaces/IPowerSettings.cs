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

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{
  /// <summary>
  /// Provides access to PowerScheduler's settings
  /// </summary>
  public interface IPowerSettings
  {
    /// <summary>
    /// Should PowerScheduler actively try to put the system into standby?
    /// </summary>
    bool ShutdownEnabled { get; }

    /// <summary>
    /// Should PowerScheduler check when any plugin wants to wakeup the system?
    /// </summary>
    bool WakeupEnabled { get; }

    /// <summary>
    /// Should the shutdown attemps be forced?
    /// </summary>
    bool ForceShutdown { get; }

    /// <summary>
    /// Should PowerScheduler be verbose when logging?
    /// </summary>
    bool ExtensiveLogging { get; }

    /// <summary>
    /// If ShutdownEnabled, how long (in minutes) to wait before putting the
    /// system into standby
    /// </summary>
    int IdleTimeout { get; }

    /// <summary>
    /// if WakeupEnabled, the time (in seconds) to wakeup the system earlier than
    /// the actual wakeup time
    /// </summary>
    int PreWakeupTime { get; }

    /// <summary>
    /// Controls the granularity of the standby/wakeup checks in seconds
    /// </summary>
    int CheckInterval { get; }

    /// <summary>
    /// How should put the system into standby? suspend/hibernate/stayon
    /// suspend uses S3, hibernate uses S4, stayon is for debugging purposes and
    /// doesn't put the system into standby at all
    /// </summary>
    ShutdownMode ShutdownMode { get; }
  }
}