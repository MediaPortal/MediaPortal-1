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
  /// A class for handling DiSEqC for DVBSky devices.
  /// </summary>
  public class DvbSky : Conexant
  {
    #region enums

    private new enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For receiving remote keypresses.
      RemoteCode,
      /// For passing blindscan parameters and commands.
      BlindscanCommand,
      /// For retrieving blindscan results.
      BlindscanData
    }

    #endregion

    #region constants

    private static readonly Guid DvbSkyBdaExtensionPropertySet = new Guid(0x03cbdcb9, 0x36dc, 0x4072, 0xac, 0x42, 0x2f, 0x94, 0xf4, 0xec, 0xa0, 0x5e);

    #endregion

    #region variables

    private bool _isDvbSky = false;

    #endregion

    /// <summary>
    /// Accessor for the property set GUID. This enables easy inheritence from the Conexant base class.
    /// </summary>
    /// <value>the GUID for the driver's custom property set</value>
    protected override Guid BdaExtensionPropertySet
    {
      get
      {
        return DvbSkyBdaExtensionPropertySet;
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
        // TeVii, Hauppauge, Geniatech, Turbosight, DVBSky, Prof and possibly others all use or implement the
        // same Conexant property set for DiSEqC support, often adding custom extensions. In order to ensure
        // that the full device functionality is available for all hardware we use the following priority
        // hierarchy:
        // TeVii [75] > Hauppauge, DVBSky, Turbosight [70] > Prof (USB) [65] > Prof (PCI, PCIe) [60] > Geniatech [50] > Conexant [40]
        return 70;
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
      Log.Debug("DVBSky: initialising device");

      if (_isDvbSky)
      {
        Log.Debug("DVBSky: device is already initialised");
        return true;
      }

      bool result = base.Initialise(tunerFilter, tunerType, tunerDevicePath);
      if (!result)
      {
        Log.Debug("DVBSky: base Conexant interface not supported");
        return false;
      }

      Log.Debug("DVBSky: supported device detected");
      _isDvbSky = true;
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      _isDvbSky = false;
    }

    #endregion
  }
}
