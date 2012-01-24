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

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling DiSEqC and DVB-S2 tuning for Hauppauge tuners.
  /// </summary>
  public class Hauppauge : ConexantBDA, IDiSEqCController, IDisposable
  {
    #region enums

    private new enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For setting the DVB-S2 pilot parameter value.
      Pilot = 32,
      /// For setting the DVB-S2 roll-off parameter value.
      RollOff = 33
    }

    #endregion

    #region constants

    private static readonly Guid HcwBdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0x00, 0xa0, 0xc9, 0xf2, 0x1f, 0xc7);

    #endregion

    #region variables

    private bool _isHauppauge = false;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Hauppauge"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Hauppauge(IBaseFilter tunerFilter)
      : base(tunerFilter)
    {
      if (!IsConexant)
      {
        return;
      }

      Log.Log.Debug("Hauppauge: supported tuner detected");
      _isHauppauge = true;
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Hauppauge-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Hauppauge-compatible tuner, otherwise <c>false</c></value>
    public bool IsHauppauge
    {
      get
      {
        return _isHauppauge;
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
        return HcwBdaExtensionPropertySet;
      }
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Hauppauge: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      if (ch.ModulationType == ModulationType.ModQpsk || ch.ModulationType == ModulationType.Mod8Psk)
      {
        // I believe this is a workaround for Canal Digital Nordic transponders on Thor 0.8W
        // that was added based on feedback from mylle here:
        // http://forum.team-mediaportal.com/mediaportal-1-1-0-alpha-453/dvb-s-scanning-not-working-hauppauge-s2-hd-symbolrate-30000-a-68010/
        if (ch.SymbolRate == 30000)
        {
          ch.Pilot = Pilot.Off;
        }

        if (ch.ModulationType == ModulationType.ModQpsk)
        {
          ch.ModulationType = ModulationType.ModNbcQpsk;
        }
        else
        {
          ch.ModulationType = ModulationType.ModNbc8Psk;
        }
      }
      Log.Log.WriteFile("  modulation = {0}", ch.ModulationType);
      Log.Log.WriteFile("  pilot      = {0}", ch.Pilot);
      SetPilot(ch.Pilot);
      SetRollOff(ch.Rolloff);
      return ch as DVBBaseChannel;
    }

    /// <summary>
    /// Set the tuner pilot parameter value.
    /// </summary>
    /// <param name="pilot">The pilot parameter value.</param>
    /// <returns><c>true</c> if the setting is successfully applied, otherwise <c>false</c></returns>
    private bool SetPilot(Pilot pilot)
    {
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Pilot, out support);
      if ((support & KSPropertySupport.Set) == 0)
      {
        return true;
      }

      Log.Log.Debug("Hauppauge: set pilot = {0}", pilot);
      Marshal.WriteInt32(_paramBuffer, (Int32)pilot);
      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Pilot,
        _instanceBuffer, InstanceSize,
        _paramBuffer, sizeof(Int32)
      );
      if (hr == 0)
      {
        Log.Log.Debug("Hauppauge: result = success");
        return true;
      }

      Log.Log.Debug("Hauppauge: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set the tuner roll-off parameter value.
    /// </summary>
    /// <param name="rollOff">The roll-off parameter value.</param>
    /// <returns><c>true</c> if the setting is successfully applied, otherwise <c>false</c></returns>
    private bool SetRollOff(RollOff rollOff)
    {
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Pilot, out support);
      if ((support & KSPropertySupport.Set) == 0)
      {
        return true;
      }

      Log.Log.Debug("Hauppauge: set roll-off = {0}", rollOff);
      Marshal.WriteInt32(_paramBuffer, (Int32)rollOff);
      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.RollOff,
        _instanceBuffer, InstanceSize,
        _paramBuffer, sizeof(Int32)
      );
      if (hr == 0)
      {
        Log.Log.Debug("Hauppauge: result = success");
        return true;
      }

      Log.Log.Debug("Hauppauge: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }
  }
}