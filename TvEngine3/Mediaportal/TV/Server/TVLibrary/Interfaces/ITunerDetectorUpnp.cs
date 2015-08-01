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

using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// An interface for detecting UPnP-based tuners.
  /// </summary>
  internal interface ITunerDetectorUpnp
  {
    /// <summary>
    /// Get the detector's name.
    /// </summary>
    string Name
    {
      get;
    }

    /// <summary>
    /// Detect and instanciate the compatible tuners exposed by a UPnP device.
    /// </summary>
    /// <param name="descriptor">The UPnP device's root descriptor.</param>
    /// <param name="controlPoint">The control point that the device is attached to.</param>
    /// <returns>the compatible tuners exposed by the device</returns>
    ICollection<ITuner> DetectTuners(DeviceDescriptor descriptor, UPnPControlPoint controlPoint);
  }
}