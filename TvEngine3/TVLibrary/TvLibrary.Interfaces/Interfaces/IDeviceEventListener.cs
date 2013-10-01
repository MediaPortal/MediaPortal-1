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

using System;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// An interface to be implemented by classes that want to be notified
  /// when device events occur.
  /// </summary>
  public interface IDeviceEventListener
  {
    /// <summary>
    /// This callback is invoked when a device is detected.
    /// </summary>
    /// <param name="device">The device that has been detected.</param>
    void OnDeviceAdded(ITVCard device);

    /// <summary>
    /// This callback is invoked when a device is removed.
    /// </summary>
    /// <param name="deviceIdentifier">The identifier of the device that has been removed.</param>
    void OnDeviceRemoved(String deviceIdentifier);
  }
}
