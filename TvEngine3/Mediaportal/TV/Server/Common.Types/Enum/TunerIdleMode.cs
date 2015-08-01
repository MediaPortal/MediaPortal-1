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

using System.ComponentModel;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// Tuner idle modes. An idle mode determines the action that will be taken
  /// when a tuner is no longer being actively used.
  /// </summary>
  public enum TunerIdleMode
  {
    /// <summary>
    /// For a tuner that exposes a DirectShow/BDA filter interface, pause the
    /// graph.
    /// - not supported by some tuners
    /// - average power use (tuner driver dependent)
    /// - fast first (re)tune
    /// </summary>
    Pause,
    /// <summary>
    /// For a tuner that exposes a DirectShow/BDA filter interface, stop the
    /// graph.
    /// - highly compatible
    /// - low power use (tuner dependent)
    /// - average/default first (re)tune speed
    /// </summary>
    Stop,
    /// <summary>
    /// For a tuner that exposes a DirectShow/BDA filter interface, dismantle
    /// and dispose the graph.
    /// - ultimate compatibility
    /// - minimal power use
    /// - slowest first (re)tune
    /// </summary>
    Unload,
    /// <summary>
    /// For a tuner that exposes a DirectShow/BDA filter interface, keep the
    /// graph running.
    /// - reasonable compatibility
    /// - highest power use
    /// - fastest possible first (re)tune
    /// </summary>
    [Description("Always On")]
    AlwaysOn
  }
}