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

using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc
{
  /// <summary>
  /// An interface for higher level control of DiSEqC devices (<see cref="IDiseqcDevice"/>).
  /// </summary>
  public interface IDiseqcController
  {
    /// <summary>
    /// Reset a device.
    /// </summary>
    void Reset();

    #region positioner (motor) control

    /// <summary>
    /// Stop the movement of a positioner device.
    /// </summary>
    void Stop();

    /// <summary>
    /// Set the Eastward soft-limit of movement for a positioner device.
    /// </summary>
    void SetEastLimit();

    /// <summary>
    /// Set the Westward soft-limit of movement for a positioner device.
    /// </summary>
    void SetWestLimit();

    /// <summary>
    /// Enable/disable the movement soft-limits for a positioner device.
    /// </summary>
    bool ForceLimits
    {
      set;
    }

    /// <summary>
    /// Drive a positioner device in a given direction for a specified period of time.
    /// </summary>
    /// <param name="direction">The direction to move in.</param>
    /// <param name="stepCount">The number of position steps to move.</param>
    void Drive(DiseqcDirection direction, byte stepCount);

    /// <summary>
    /// Store the current position of a positioner device for later use.
    /// </summary>
    /// <param name="position">The identifier to use for the position.</param>
    void StorePosition(byte position);

    /// <summary>
    /// Drive a positioner device to its reference position.
    /// </summary>
    void GoToReferencePosition();

    /// <summary>
    /// Drive a positioner device to a previously stored position.
    /// </summary>
    /// <param name="position">The position to drive to.</param>
    void GoToStoredPosition(byte position);

    /// <summary>
    /// Drive a positioner device to a given longitude.
    /// </summary>
    /// <param name="longitude">The longiude to drive to. Range -180 (180W) to 180 (180E).</param>
    void GoToAngularPosition(double longitude);

    /// <summary>
    /// Get the current position of a positioner device.
    /// </summary>
    /// <param name="position">The stored position identifier corresponding with the current base position.</param>
    /// <param name="longitude">The longitude corresponding with the current base position.</param>
    /// <param name="stepsAzimuth">The number of steps taken from the position on the azmutal axis.</param>
    /// <param name="stepsElevation">The number of steps taken from the position on the vertical (elevation) axis.</param>
    void GetPosition(out int position, out double longitude, out int stepsAzimuth, out int stepsElevation);

    #endregion
  }
}