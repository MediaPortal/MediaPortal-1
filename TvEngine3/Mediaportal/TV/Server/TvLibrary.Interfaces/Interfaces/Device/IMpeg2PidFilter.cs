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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension
{
  /// <summary>
  /// An interface for tuners that deliver MPEG 2 transport streams and are capable of filtering
  /// the stream. Filtering can be used to reduce the required bus (eg. PCI, USB, PCIe, Firewire,
  /// network) bandwidth by supressing unrequired sub-streams.
  /// </summary>
  public interface IMpeg2PidFilter : ICustomDevice
  {
    /// <summary>
    /// Configure the PID filter.
    /// </summary>
    /// <remarks>
    /// The implementation is expected to enable or disable the filter automatically based on the
    /// tuning parameters, number of PIDs or other information. TV Server can also force the filter
    /// to be disabled or enabled if necessary.
    /// To force the filter to be disabled, call the function with an empty or null PID list.
    /// To force the filter to be enabled, call the function with the forceEnable parameter set to <c>true</c>.
    /// </remarks>
    /// <param name="tuningDetail">The current multiplex/transponder tuning parameters.</param>
    /// <param name="pids">The PIDs to allow through the filter.</param>
    /// <param name="forceEnable">Set this parameter to <c>true</c> to force the filter to be enabled.</param>
    /// <returns><c>true</c> if the PID filter is configured successfully, otherwise <c>false</c></returns>
    bool SetFilterState(IChannel tuningDetail, ICollection<ushort> pids, bool forceEnable);
  }
}
