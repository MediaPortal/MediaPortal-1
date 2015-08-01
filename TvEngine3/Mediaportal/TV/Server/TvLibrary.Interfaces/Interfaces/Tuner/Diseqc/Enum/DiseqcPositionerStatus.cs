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

using System;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum
{
  /// <summary>
  /// DiSEqC positioner status flags.
  /// </summary>
  /// <remarks>
  /// Status is retrieved using DiseqcCommand.PositionerStatus.
  /// </remarks>
  [Flags]
  public enum DiseqcPositionerStatus : byte
  {
    /// <summary>
    /// The reference position has been corrupted or lost.
    /// </summary>
    PositionReferenceLost = 0x1,
    /// <summary>
    /// A hardware switch (limit or reference) is activated.
    /// </summary>
    HardwareSwitchActivated = 0x2,
    /// <summary>
    /// Power is not available.
    /// </summary>
    PowerNotAvailable = 0x4,
    /// <summary>
    /// Movement soft-limit has been reached.
    /// </summary>
    SoftwareLimitReached = 0x8,
    /// <summary>
    /// The motor is running.
    /// </summary>
    MotorRunning = 0x10,
    /// <summary>
    /// Current or previous movement direction was West.
    /// </summary>
    DirectionWest = 0x20,
    /// <summary>
    /// Movement soft-limits are enabled.
    /// </summary>
    SoftwareLimitsEnabled = 0x40,
    /// <summary>
    /// The previous movement command has been completed.
    /// </summary>
    CommandCompleted = 0x80
  }
}