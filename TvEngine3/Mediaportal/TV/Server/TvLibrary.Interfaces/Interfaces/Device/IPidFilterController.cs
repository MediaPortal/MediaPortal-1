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
using System.Collections.Generic;
using DirectShowLib.BDA;

namespace TvLibrary.Interfaces.Device
{
  /// <summary>
  /// An interface for devices that are capable of filtering the MPEG 2 stream to reduce the bandwidth on
  /// the bus (eg. PCI, USB, PCIe, Firewire) that the device is connected to. Filtering may be critical in
  /// order to avoid running out of bandwidth on the bus.
  /// </summary>
  public interface IPidFilterController : ICustomDevice
  {
    /// <summary>
    /// Configure the PID filter.
    /// </summary>
    /// <remarks>
    /// The controller is expected to enable or disable the filter automatically based on the modulation
    /// scheme, number of PIDs or other device information. TV Server can also force the controller to
    /// disable or enable the filter if necessary.
    /// To force the controller to disable the filter, call the function with an empty or null PID list.
    /// To force the controller to enable the filter, call the function with the forceEnable parameter set to <c>true</c>.
    /// </remarks>
    /// <param name="pids">The PIDs to allow through the filter.</param>
    /// <param name="modulation">The current multiplex/transponder modulation scheme.</param>
    /// <param name="forceEnable">Set this parameter to <c>true</c> to force the filter to be enabled.</param>
    /// <returns><c>true</c> if the PID filter is configured successfully, otherwise <c>false</c></returns>
    bool SetFilterPids(HashSet<UInt16> pids, ModulationType modulation, bool forceEnable);
  }
}
