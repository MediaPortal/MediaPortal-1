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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBda
{
  /// <summary>
  /// A class for handling DiSEqC and DVB-S2 tuning for Hauppauge BDA devices.
  /// </summary>
  public class HauppaugeBda : Conexant.Conexant
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

    private static readonly Guid HCW_BDA_EXTENSION_PROPERTY_SET = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0x00, 0xa0, 0xc9, 0xf2, 0x1f, 0xc7);
    private const int PARAM_BUFFER_SIZE = 4;

    #endregion

    #region variables

    private bool _isHauppaugeBda = false;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// Accessor for the property set GUID. This enables easy inheritence from the Conexant base class.
    /// </summary>
    /// <value>the GUID for the driver's custom property set</value>
    protected override Guid BdaExtensionPropertySet
    {
      get
      {
        return HCW_BDA_EXTENSION_PROPERTY_SET;
      }
    }

    /// <summary>
    /// Set the tuner pilot parameter value.
    /// </summary>
    /// <param name="pilot">The pilot parameter value.</param>
    /// <returns><c>true</c> if the setting is successfully applied, otherwise <c>false</c></returns>
    private bool SetPilot(Pilot pilot)
    {
      this.LogDebug("Hauppauge BDA: set pilot = {0}", pilot);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Pilot, out support);
      if (hr != (int)HResult.Severity.Success || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Hauppauge BDA: device does not support pilot property, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return true;  // This is not an error.
      }

      Marshal.WriteInt32(_paramBuffer, (int)pilot);
      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Pilot,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Hauppauge BDA: result = success");
        return true;
      }

      this.LogDebug("Hauppauge BDA: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set the tuner roll-off parameter value.
    /// </summary>
    /// <param name="rollOff">The roll-off parameter value.</param>
    /// <returns><c>true</c> if the setting is successfully applied, otherwise <c>false</c></returns>
    private bool SetRollOff(RollOff rollOff)
    {
      this.LogDebug("Hauppauge BDA: set roll-off = {0}", rollOff);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Pilot, out support);
      if (hr != (int)HResult.Severity.Success || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Hauppauge BDA: device does not support roll-off property, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return true;  // This is not an error.
      }

      Marshal.WriteInt32(_paramBuffer, (int)rollOff);
      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.RollOff,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Hauppauge BDA: result = success");
        return true;
      }

      this.LogDebug("Hauppauge BDA: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 70;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Hauppauge BDA";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerExternalIdentifier">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalIdentifier, CardType tunerType, object context)
    {
      this.LogDebug("Hauppauge BDA: initialising device");

      if (_isHauppaugeBda)
      {
        this.LogDebug("Hauppauge BDA: device is already initialised");
        return true;
      }

      bool result = base.Initialise(tunerExternalIdentifier, tunerType, context);
      if (!result)
      {
        this.LogDebug("Hauppauge BDA: base Conexant interface not supported");
        return false;
      }

      this.LogDebug("Hauppauge BDA: supported device detected");
      _isHauppaugeBda = true;
      _paramBuffer = Marshal.AllocCoTaskMem(PARAM_BUFFER_SIZE);
      return true;
    }

    #region device state change callbacks

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Hauppauge BDA: on before tune callback");
      action = TunerAction.Default;

      if (!_isHauppaugeBda || _propertySet == null)
      {
        this.LogDebug("Hauppauge BDA: device not initialised or interface not supported");
        return;
      }

      // This is important. Hauppauge recommends that the graph be running when tune requests are
      // submitted and DiSEqC commands are sent.
      action = TunerAction.Start;

      // We only have work to do if the channel is a DVB-S2 channel.
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null || (ch.ModulationType != ModulationType.ModQpsk && ch.ModulationType != ModulationType.Mod8Psk))
      {
        return;
      }

      // I believe this is a workaround for Canal Digital Nordic transponders on Thor 0.8W that was
      // added based on feedback from mylle here:
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
      this.LogDebug("  modulation = {0}", ch.ModulationType);

      SetPilot(ch.Pilot);
      SetRollOff(ch.RollOff);
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();

      if (_paramBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_paramBuffer);
        _paramBuffer = IntPtr.Zero;
      }
      _isHauppaugeBda = false;
    }

    #endregion
  }
}