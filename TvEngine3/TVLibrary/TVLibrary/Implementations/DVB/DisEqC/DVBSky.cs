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
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling DiSEqC for DVBSky tuners.
  /// </summary>
  public class DvbSky : ConexantBDA, IDiSEqCController, IDisposable
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
    /// Initialises a new instance of the <see cref="DvbSky"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public DvbSky(IBaseFilter tunerFilter)
      : base(tunerFilter)
    {
      if (!IsConexant)
      {
        return;
      }

      Log.Log.Debug("DVBSky: supported tuner detected");
      _isDvbSky = true;
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a DVBSky-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a DVBSky-compatible tuner, otherwise <c>false</c></value>
    public bool IsDvbSky
    {
      get
      {
        return _isDvbSky;
      }
    }

    /// <summary>
    /// Accessor for the property set GUID. This allows easy inheritence from the
    /// Conexant base class.
    /// </summary>
    /// <value>the GUID for the driver's custom tuner property set</value>
    protected override Guid BdaExtensionPropertySet
    {
      get
      {
        return DvbSkyBdaExtensionPropertySet;
      }
    }
  }
}