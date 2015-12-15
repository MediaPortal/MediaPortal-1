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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Enum
{
  /// <summary>
  /// Tuner state.
  /// </summary>
  /// <remarks>
  /// For a tuner that exposes a DirectShow filter interface, state roughly
  /// mirrors the state of the graph.
  /// </remarks>
  internal enum TunerState
  {
    /// <summary>
    /// The tuner is not yet loaded. It must be loaded/initialised before any interaction may occur.
    /// </summary>
    [Description("Not Loaded")]
    NotLoaded,
    /// <summary>
    /// The tuner is being loaded.
    /// </summary>
    Loading,
    /// <summary>
    /// The tuner is loaded but not paused or started.
    /// </summary>
    Stopped,
    /// <summary>
    /// The tuner is paused.
    /// </summary>
    Paused,
    /// <summary>
    /// The tuner is started. Note a tuner may be started and idle if the tuner is configured to
    /// use the always on idle mode.
    /// </summary>
    Started
  }
}