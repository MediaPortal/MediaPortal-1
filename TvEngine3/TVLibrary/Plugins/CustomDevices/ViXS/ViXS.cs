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
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvEngine
{
  /// <summary>
  /// This class provides clear QAM tuning support for ViXS/Asus ATSC/QAM tuners.
  /// </summary>
  public class ViXS : Microsoft
  {
    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0x02779308, 0x77d8, 0x4914, 0x9f, 0x15, 0x7f, 0xa6, 0xe1, 0x55, 0x84, 0xc7);

    #endregion

    /// <summary>
    /// The class or property set that provides access to the tuner modulation parameter.
    /// </summary>
    protected override Guid ModulationPropertyClass
    {
      get
      {
        // We override the Microsoft implementation of this property. This property set GUID is the only
        // difference between the generic Microsoft clear QAM implementation and the ViXS implementation.
        return BdaExtensionPropertySet;
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return "ViXS";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Log.Debug("ViXS: initialising device");
      if (tunerType != CardType.Atsc)
      {
        Log.Debug("ViXS: tuner type {0} is not supported", tunerType);
        return false;
      }
      return base.Initialise(tunerFilter, tunerType, tunerDevicePath);
    }

    #endregion
  }
}
