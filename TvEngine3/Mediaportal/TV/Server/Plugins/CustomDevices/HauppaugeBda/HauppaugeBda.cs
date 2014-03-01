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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBda
{
  /// <summary>
  /// A class for handling DiSEqC and DVB-S2 tuning for Hauppauge BDA tuners.
  /// </summary>
  public class HauppaugeBda : BaseCustomDevice, IDiseqcDevice
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

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0x00, 0xa0, 0xc9, 0xf2, 0x1f, 0xc7);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private const int PARAM_BUFFER_SIZE = 4;

    #endregion

    #region variables

    private bool _isHauppaugeBda = false;
    private Conexant.Conexant _conexantInterface = null;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// Set the tuner pilot parameter value.
    /// </summary>
    /// <param name="pilot">The pilot parameter value.</param>
    /// <returns><c>true</c> if the setting is successfully applied, otherwise <c>false</c></returns>
    private bool SetPilot(Pilot pilot)
    {
      this.LogDebug("Hauppauge BDA: set pilot = {0}", pilot);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Pilot, out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Hauppauge BDA: pilot property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return true;  // This is not an error.
      }

      Marshal.WriteInt32(_paramBuffer, (int)pilot);
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Pilot,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Hauppauge BDA: result = success");
        return true;
      }

      this.LogError("Hauppauge BDA: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Pilot, out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Hauppauge BDA: roll-off property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return true;  // This is not an error.
      }

      Marshal.WriteInt32(_paramBuffer, (int)rollOff);
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.RollOff,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int)
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Hauppauge BDA: result = success");
        return true;
      }

      this.LogError("Hauppauge BDA: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 70;
      }
    }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Hauppauge BDA";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("Hauppauge BDA: initialising");

      if (_isHauppaugeBda)
      {
        this.LogWarn("Hauppauge BDA: extension already initialised");
        return true;
      }

      IBaseFilter tunerFilter = context as IBaseFilter;
      if (tunerFilter == null)
      {
        this.LogDebug("Hauppauge BDA: context is not a filter");
        return false;
      }

      _conexantInterface = new Conexant.Conexant(BDA_EXTENSION_PROPERTY_SET);
      if (!_conexantInterface.Initialise(tunerExternalId, tunerType, context))
      {
        this.LogDebug("Hauppauge BDA: base Conexant interface not supported");
        return false;
      }

      this.LogInfo("Hauppauge BDA: extension supported");
      _isHauppaugeBda = true;
      _propertySet = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0) as IKsPropertySet;
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      _paramBuffer = Marshal.AllocCoTaskMem(PARAM_BUFFER_SIZE);
      return true;
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Hauppauge BDA: on before tune call back");
      action = TunerAction.Default;

      if (!_isHauppaugeBda)
      {
        this.LogWarn("Hauppauge BDA: not initialised or interface not supported");
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

    #region IDiseqcDevice members

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      if (_conexantInterface != null)
      {
        return _conexantInterface.SetToneState(toneBurstState, tone22kState);
      }
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
    {
      if (_conexantInterface != null)
      {
        return _conexantInterface.SendDiseqcCommand(command);
      }
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadDiseqcResponse(out byte[] response)
    {
      if (_conexantInterface != null)
      {
        return _conexantInterface.ReadDiseqcResponse(out response);
      }
      // Not implemented.
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      Release.ComObject("Hauppauge property set", ref _propertySet);
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (_paramBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_paramBuffer);
        _paramBuffer = IntPtr.Zero;
      }
      if (_conexantInterface != null)
      {
        _conexantInterface.Dispose();
        _conexantInterface = null;
      }
      _isHauppaugeBda = false;
    }

    #endregion
  }
}