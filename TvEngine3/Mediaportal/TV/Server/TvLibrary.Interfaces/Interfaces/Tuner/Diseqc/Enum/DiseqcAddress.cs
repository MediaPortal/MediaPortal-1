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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum
{
  /// <summary>
  /// DiSEqC message device class address.
  /// </summary>
  public enum DiseqcAddress : byte
  {
    /// <summary>
    /// Any device.
    /// </summary>
    Any = 0,

    /// <summary>
    /// Any LNB, switcher or SMATV.
    /// </summary>
    AnySwitch = 0x10,
    /// <summary>
    /// LNB.
    /// </summary>
    Lnb = 0x11,
    /// <summary>
    /// LNB with loop-through switching.
    /// </summary>
    LnbWithLoopThrough = 0x12,
    /// <summary>
    /// DC-blocking switcher.
    /// </summary>
    Switcher = 0x14,
    /// <summary>
    /// Switcher with DC loop-through.
    /// </summary>
    SwitcherWithDcLoopThrough = 0x15,
    /// <summary>
    /// SMATV.
    /// </summary>
    Smatv = 0x18,

    /// <summary>
    /// Any polariser.
    /// </summary>
    AnyPolariser = 0x20,
    /// <summary>
    /// Linear (skew) polariser.
    /// </summary>
    LinearPolariser = 0x21,

    /// <summary>
    /// Any positioner.
    /// </summary>
    AnyPositioner = 0x30,
    /// <summary>
    /// Polar/azimuth positioner.
    /// </summary>
    AzimuthPositioner = 0x31,
    /// <summary>
    /// Elevation positioner.
    /// </summary>
    ElevationPositioner = 0x32,

    /// <summary>
    /// Any installer aid.
    /// </summary>
    AnyInstallerAid = 0x40,
    /// <summary>
    /// Analog signal strength indicator.
    /// </summary>
    AnalogSignalStrengthIndicator = 0x41,

    // 0x6x = address reallocations

    /// <summary>
    /// Any intelligent slave.
    /// </summary>
    AnyIntelligentSlave = 0x70,
    /// <summary>
    /// Subscriber controlled headend.
    /// </summary>
    SubscriberControlledHeadend = 0x71

    // 0xf* = OEM extension
  }
}