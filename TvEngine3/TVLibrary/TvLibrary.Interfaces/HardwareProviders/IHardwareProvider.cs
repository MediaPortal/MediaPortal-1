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
using System.Collections.Generic;
using System.Text;
using DirectShowLib;
using TvLibrary.Channels;
using TvLibrary.Implementations;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Hardware
{
  /// <summary>
  /// Capabilities definitions; Bitwise combinations allowed
  /// </summary>
  [Flags]
  public enum CapabilitiesType
  {
    /// No special capabilities
    None = 0,
    /// decoding requires a special CaPMT, not whole PMT
    CA_RequireCaPmt = 1,
    /// support sending multiple channels to decode
    CA_DescrambleMultiple = 2,
    /// when AllowStopGraph is queried CA provider can deny stop if CAM is used (Twinhan)
    CA_DenyStopGraph = 4,
    /// use the 2nd passed filter, not the tuner filter (WinTV CI)
    HW_UseExtraFilter = 8,
    /// device is additional, like WinTV CI USB; there can be more than one devices in current TvCard instance
    HW_MultipleDevice = 16
  }

  /// <summary>
  /// Interface that all hardware specific plugin have to support
  /// </summary>
  public interface IHardwareProvider
  {
    // detection part
    /// <summary>
    /// Init the hardware provider
    /// </summary>
    /// <param name="tunerFilter">the tunerfilter of the card</param>
    void Init(IBaseFilter tunerFilter);

    /// <summary>
    /// Get or set custom device index, if multiple devices need to be addressed
    /// </summary>
    int DeviceIndex { get; set; }

    /// <summary>
    /// Get or set device path, some libraries use it for addressing devices
    /// </summary>
    string DevicePath { get; set; }

    /// <summary>
    /// Loading priority
    /// Loading is done in ascending order, 10 (special) before 50 (generic) before 100 (additional device)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Checks if hardware is supported and open the device
    /// </summary>
    void CheckAndOpen();

    /// <summary>
    /// Returns the name of the provider
    /// </summary>
    String Provider { get; }

    /// <summary>
    /// Returns the result of detection; if false the provider should be disposed
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Returns the Capabilities
    /// </summary>
    CapabilitiesType Capabilities { get; }
  }
}