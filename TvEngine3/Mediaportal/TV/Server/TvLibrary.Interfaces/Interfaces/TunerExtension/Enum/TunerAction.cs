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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum
{
  /// <summary>
  /// Tuner actions. Extensions can specify actions at certain stages of the
  /// tuner lifecycle that optimise compatibility, performance, power use etc.
  /// </summary>
  /// <remarks>
  /// The order specified here is important. When there is a conflict between
  /// extensions, the action with the highest value will be performed. For
  /// example, stop would be performed in preference to pause. The order is
  /// intended to have more compatible actions listed higher than unusual or
  /// less compatible actions.
  /// </remarks>
  public enum TunerAction
  {
    /// <summary>
    /// Default behaviour will continue. No alternate action will be taken.
    /// </summary>
    Default,
    /// <summary>
    /// Start the tuner.
    /// </summary>
    /// <remarks>
    /// For a tuner that exposes a DirectShow/BDA filter interface, run the
    /// graph.
    /// </remarks>
    Start,
    /// <summary>
    /// Pause the tuner.
    /// </summary>
    /// <remarks>
    /// For a tuner that exposes a DirectShow/BDA filter interface, pause the
    /// graph.
    /// </remarks>
    Pause,
    /// <summary>
    /// Stop the tuner.
    /// For a tuner that exposes a DirectShow/BDA filter interface, stop the
    /// graph.
    /// </summary>
    Stop,
    /// <summary>
    /// Retart the tuner.
    /// </summary>
    /// <remarks>
    /// For a tuner that exposes a DirectShow/BDA filter interface, stop then
    /// run the graph.
    /// </remarks>
    Restart,
    /// <summary>
    /// Reset the tuner.
    /// </summary>
    /// <remarks>
    /// For a tuner that exposes a DirectShow/BDA filter interface, rebuild the
    /// graph.
    /// </remarks>
    Reset,
    /// <summary>
    /// Unload the tuner.
    /// </summary>
    /// <remarks>
    /// For a tuner that exposes a DirectShow/BDA filter interface, dismantle
    /// and dispose the graph.
    /// </remarks>
    Unload
  }
}