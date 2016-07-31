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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftAtscQam
{
  /// <summary>
  /// This class provides an implementation of clear QAM tuning support for Windows XP.
  /// </summary>
  public class MicrosoftAtscQam : BaseTunerExtension, IDisposable
  {
    #region constants

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.

    #endregion

    #region variables

    private bool _isMicrosoftAtscQam = false;
    private IKsPropertySet _propertySet = null;
    private bool _releasePropertySet = true;
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
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Microsoft ATSC QAM: {0} does not support property set, hr = 0x{1:x}, support = {2}", objType, hr, support);
        return null;
      }
      return ps;
    }

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
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
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Microsoft ATSC QAM";
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Microsoft ATSC QAM: initialising");

      if (_isMicrosoftAtscQam)
      {
        this.LogWarn("Microsoft ATSC QAM: extension already initialised");
        return true;
      }

      if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte))
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

      IPin pin = null;
      try
      {
        _releasePropertySet = true;
        _propertySet = FindPropertySet(filter, "filter");
        if (_propertySet != null)
        {
          _releasePropertySet = false;
          return true;
        }

        pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
        if (pin == null)
        {
          this.LogError("Microsoft ATSC QAM: failed to find filter input pin");
          return false;
        }

        _propertySet = FindPropertySet(pin, "input pin");
        if (_propertySet != null)
        {
          return true;
        }

        Release.ComObject("Microsoft ATSC QAM filter input pin", ref pin);
        pin = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
        if (pin == null)
        {
          this.LogError("Microsoft ATSC QAM: failed to find filter output pin");
          return false;
        }

        IPin connectedPin;
        int hr = pin.ConnectedTo(out connectedPin);
        if (hr == (int)NativeMethods.HResult.S_OK && connectedPin != null)
        {
          Release.ComObject("Microsoft ATSC QAM filter connected pin", ref connectedPin);
          _propertySet = FindPropertySet(pin, "output pin");
          return _propertySet != null;
        }

        // Some drivers will not report whether a property set is supported
        // unless the pin is connected. It is okay when we're checking a tuner
        // filter which has a capture filter connected, but if the tuner filter
        // is also the capture filter then the output pin(s) won't be connected
        // yet.
        FilterInfo filterInfo;
        hr = filter.QueryFilterInfo(out filterInfo);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("Microsoft ATSC QAM: failed to get filter info, hr = 0x{0:x}", hr);
          return false;
        }
        IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
        if (graph == null)
        {
          this.LogDebug("Microsoft ATSC QAM: filter info graph is null");
          return false;
        }

        // Add an infinite tee.
        IBaseFilter infTee = (IBaseFilter)new InfTee();
        IPin infTeeInputPin = null;
        try
        {
          hr = graph.AddFilter(infTee, "Temp Infinite Tee");
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("Microsoft ATSC QAM: failed to add infinite tee to graph, hr = 0x{0:x}", hr);
            return false;
          }

          // Connect the infinite tee to the filter.
          infTeeInputPin = DsFindPin.ByDirection(infTee, PinDirection.Input, 0);
          if (infTeeInputPin == null)
          {
            this.LogError("Microsoft ATSC QAM: failed to find the infinite tee input pin, hr = 0x{0:x}", hr);
            return false;
          }
          hr = graph.ConnectDirect(pin, infTeeInputPin, null);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("Microsoft ATSC QAM: failed to connect infinite tee, hr = 0x{0:x}", hr);
            return false;
          }

          _propertySet = FindPropertySet(pin, "output pin");
          return _propertySet != null;
        }
        finally
        {
          graph.Disconnect(pin);
          Release.ComObject("Microsoft ATSC QAM infinite tee input pin", ref infTeeInputPin);
          graph.RemoveFilter(infTee);
          Release.ComObject("Microsoft ATSC QAM infinite tee", ref infTee);
          Release.FilterInfo(ref filterInfo);
          graph = null;
        }
      }
      finally
      {
        if (_propertySet != null)
        {
          this.LogInfo("Microsoft ATSC QAM: extension supported");
          _isMicrosoftAtscQam = true;
          _paramBuffer = Marshal.AllocCoTaskMem(sizeof(int));
          _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
        }
        else
        {
          Release.ComObject("Microsoft ATSC QAM filter output pin", ref pin);
        }
      }
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Microsoft ATSC QAM: on before tune call back");
      action = TunerAction.Default;

      if (!_isMicrosoftAtscQam)
      {
        this.LogWarn("Microsoft ATSC QAM: not initialised or interface not supported");
        return;
      }

      // This is legacy code. I don't know for sure if it is actually needed or
      // why it might be needed. Apparently when tuning ATSC or clear QAM we
      // need to set the modulation directly for compatibility with Windows XP.
      // Timing within the tuning process might be important, and we might also
      // need to enclose this within IBDA_DeviceControl start/check/commit.
      ModulationType bdaModulation = ModulationType.ModNotSet;
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        switch (atscChannel.ModulationScheme)
        {
          case ModulationSchemeVsb.Vsb8:
            bdaModulation = ModulationType.Mod8Vsb;
            break;
          case ModulationSchemeVsb.Vsb16:
            bdaModulation = ModulationType.Mod16Vsb;
            break;
          default:
            this.LogWarn("Microsoft ATSC QAM: ATSC tune request uses unsupported modulation scheme {0}", atscChannel.ModulationScheme);
            return;
        }
      }
      else
      {
        ChannelScte scteChannel = channel as ChannelScte;
        if (scteChannel == null)
        {
          return;
        }

        switch (scteChannel.ModulationScheme)
        {
          case ModulationSchemeQam.Qam64:
            bdaModulation = ModulationType.Mod64Qam;
            break;
          case ModulationSchemeQam.Qam256:
            bdaModulation = ModulationType.Mod256Qam;
            break;
          default:
            this.LogWarn("Microsoft ATSC QAM: SCTE tune request uses unsupported modulation scheme {0}", scteChannel.ModulationScheme);
            return;
        }
      }

      Marshal.WriteInt32(_paramBuffer, 0, (int)bdaModulation);
      int hr = _propertySet.Set(_propertySetGuid, (int)BdaDemodulatorProperty.ModulationType, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("Microsoft ATSC QAM: failed to set modulation, hr = 0x{0:x}, modulation = {1}", hr, bdaModulation);
      }
      else
      {
        this.LogDebug("  modulation = {0}", bdaModulation);
      }
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~MicrosoftAtscQam()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing && _releasePropertySet)
      {
        Release.ComObject("Microsoft ATSC QAM property set", ref _propertySet);
      }
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