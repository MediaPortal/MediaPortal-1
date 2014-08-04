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
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftAtscQam
{
  /// <summary>
  /// This class provides an implementation of clear QAM tuning support for Windows XP.
  /// </summary>
  public class MicrosoftAtscQam : BaseCustomDevice
  {
    #region constants

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.

    #endregion

    #region variables

    private bool _isMicrosoftAtscQam = false;
    private IKsPropertySet _propertySet = null;
    private Guid _propertySetGuid = typeof(IBDA_DigitalDemodulator).GUID;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    #region constructors

    /// <summary>
    /// Constructor for <see cref="MicrosoftAtscQam"/> instances.
    /// </summary>
    public MicrosoftAtscQam()
    {
    }

    /// <summary>
    /// Constructor for non-inherited types (eg. <see cref="ViXS"/>).
    /// </summary>
    public MicrosoftAtscQam(Guid propertySetGuid)
    {
      _propertySetGuid = propertySetGuid;
    }

    #endregion

    private IKsPropertySet FindPropertySet(object obj, string objType)
    {
      IKsPropertySet ps = obj as IKsPropertySet;
      if (ps == null)
      {
        this.LogDebug("Microsoft ATSC QAM: {0} is not a property set", objType);
        return null;
      }

      KSPropertySupport support;
      int hr = ps.QuerySupported(_propertySetGuid, (int)BdaDemodulatorProperty.ModulationType, out support);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogDebug("Microsoft ATSC QAM: {0} does not support property set, hr = 0x{1:x}", objType, hr);
        return null;
      }
      return ps;
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This implementation should only be used when more specialised interfaces are not available.
        return 1;
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
        return "Microsoft ATSC QAM";
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
      this.LogDebug("Microsoft ATSC QAM: initialising");

      if (_isMicrosoftAtscQam)
      {
        this.LogWarn("Microsoft ATSC QAM: extension already initialised");
        return true;
      }

      if (tunerType != CardType.Atsc)
      {
        this.LogDebug("Microsoft ATSC QAM: tuner type not supported");
        return false;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Microsoft ATSC QAM: context is not a filter");
        return false;
      }

      _propertySet = FindPropertySet(filter, "filter");
      if (_propertySet == null)
      {
        IPin pin = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
        if (pin == null)
        {
          this.LogError("Microsoft ATSC QAM: failed to find filter output pin");
          return false;
        }

        // Note: this check could be problematic for single tuner/capture filter implementations.
        // Some drivers will not report whether a property set is supported unless the pin is
        // connected. It is okay when we're checking a tuner filter which has a capture filter
        // connected, but if the tuner filter is also the capture filter then the output pin(s)
        // won't be connected yet.
        _propertySet = FindPropertySet(pin, "output pin");

        if (_propertySet == null)
        {
          Release.ComObject("Microsoft ATSC QAM filter output pin", ref pin);
          pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
          if (pin == null)
          {
            this.LogError("Microsoft ATSC QAM: failed to find filter input pin");
            return false;
          }
          _propertySet = FindPropertySet(pin, "input pin");
          if (_propertySet == null)
          {
            Release.ComObject("Microsoft ATSC QAM filter input pin", ref pin);
            this.LogDebug("Microsoft ATSC QAM: property set not supported");
            return false;
          }
        }
      }

      this.LogInfo("Microsoft ATSC QAM: extension supported");
      _isMicrosoftAtscQam = true;
      _paramBuffer = Marshal.AllocCoTaskMem(sizeof(int));
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
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
      this.LogDebug("Microsoft ATSC QAM: on before tune call back");
      action = TunerAction.Default;

      if (!_isMicrosoftAtscQam)
      {
        this.LogWarn("Microsoft ATSC QAM: not initialised or interface not supported");
        return;
      }

      // This is legacy code. I don't know for sure if it is actually needed or why it might be
      // needed. Apparently when tuning ATSC or clear QAM we need to set the modulation directly
      // for compatibility with Windows XP. Timing within the tuning process might be important.
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        return;
      }

      Marshal.WriteInt32(_paramBuffer, (int)atscChannel.ModulationType);
      int hr = _propertySet.Set(_propertySetGuid, (int)BdaDemodulatorProperty.ModulationType, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("Microsoft ATSC QAM: failed to set modulation, hr = 0x{0:x}", hr);
      }
      else
      {
        this.LogDebug("  modulation = {0}", atscChannel.ModulationType);
      }
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      Release.ComObject("Microsoft ATSC QAM property set", ref _propertySet);
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
      _isMicrosoftAtscQam = false;
    }

    #endregion
  }
}