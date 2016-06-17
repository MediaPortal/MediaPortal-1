#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// This interface defines extensions to the <see cref="IQualityControl"/>
  /// interface that we don't want to publicly expose.
  /// </summary>
  internal interface IQualityControlInternal : IQualityControl
  {
    /// <summary>
    /// Reload the control's configuration.
    /// </summary>
    /// <param name="configuration">The configuration of the associated tuner.</param>
    void ReloadConfiguration(TVDatabase.Entities.Tuner configuration);

    /// <summary>
    /// Notify the control that time-shifting has started.
    /// </summary>
    void OnStartTimeShifting();

    /// <summary>
    /// Notify the control that time-shifting has stopped.
    /// </summary>
    void OnStopTimeShifting();

    /// <summary>
    /// Notify the control that recording has started.
    /// </summary>
    void OnStartRecording();

    /// <summary>
    /// Notify the control that recording has stopped.
    /// </summary>
    void OnStopRecording();
  }
}