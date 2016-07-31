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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftStreamSelector
{
  /// <summary>
  /// This class provides a base implementation of stream selection (eg. DVB
  /// PLP or IS selection) for tuners that support Microsoft BDA interfaces and
  /// de-facto standards.
  /// </summary>
  /// <remarks>
  /// If a driver supports this interface, it probably only supports setting
  /// the PLP/IS ***index***, not the actual PLP/IS ID we expect. This is
  /// likely due to the fact that the "get" interface is only able to return a
  /// 4 byte integer. If the "get" interface is supported, we expect it to
  /// return the number of available streams.
  /// </remarks>
  public class MicrosoftStreamSelector : BaseTunerExtension, IDisposable, IStreamSelector
  {
    #region constants

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.

    #endregion

    #region variables

    private bool _isMicrosoftStreamSelector = false;
    private bool _releasePropertySet = true;
    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    private IKsPropertySet FindPropertySet(object obj, string objType)
    {
      IKsPropertySet ps = obj as IKsPropertySet;
      if (ps == null)
      {
        this.LogDebug("Microsoft stream selector: {0} is not a property set", objType);
        return null;
      }

      // Note some implementations may not support "get".
      KSPropertySupport support;
      int hr = ps.QuerySupported(typeof(IBDA_DigitalDemodulator).GUID, (int)BdaDemodulatorProperty.PlpNumber, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Microsoft stream selector: {0} does not support property set, hr = 0x{1:x}, support = {2}", objType, hr, support);
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
        return "Microsoft stream selector";
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
      this.LogDebug("Microsoft stream selector: initialising");

      if (_isMicrosoftStreamSelector)
      {
        this.LogWarn("Microsoft stream selector: extension already initialised");
        return true;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Microsoft stream selector: context is not a filter");
        return false;
      }

      // Find the property set. We expect to find it on the tuner filter output
      // pin, but we check the tuner filter and input pin as well just in case.
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
          this.LogError("Microsoft stream selector: failed to find filter input pin");
          return false;
        }

        _propertySet = FindPropertySet(pin, "input pin");
        if (_propertySet != null)
        {
          return true;
        }

        Release.ComObject("Microsoft stream selector filter input pin", ref pin);
        pin = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
        if (pin == null)
        {
          this.LogError("Microsoft stream selector: failed to find filter output pin");
          return false;
        }

        IPin connectedPin;
        int hr = pin.ConnectedTo(out connectedPin);
        if (hr == (int)NativeMethods.HResult.S_OK && connectedPin != null)
        {
          Release.ComObject("Microsoft stream selector filter connected pin", ref connectedPin);
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
          this.LogError("Microsoft stream selector: failed to get filter info, hr = 0x{0:x}", hr);
          return false;
        }
        IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
        if (graph == null)
        {
          this.LogDebug("Microsoft stream selector: filter info graph is null");
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
            this.LogError("Microsoft stream selector: failed to add infinite tee to graph, hr = 0x{0:x}", hr);
            return false;
          }

          // Connect the infinite tee to the filter.
          infTeeInputPin = DsFindPin.ByDirection(infTee, PinDirection.Input, 0);
          if (infTeeInputPin == null)
          {
            this.LogError("Microsoft stream selector: failed to find the infinite tee input pin, hr = 0x{0:x}", hr);
            return false;
          }
          hr = graph.ConnectDirect(pin, infTeeInputPin, null);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("Microsoft stream selector: failed to connect infinite tee, hr = 0x{0:x}", hr);
            return false;
          }

          _propertySet = FindPropertySet(pin, "output pin");
          return _propertySet != null;
        }
        finally
        {
          graph.Disconnect(pin);
          Release.ComObject("Microsoft stream selector infinite tee input pin", ref infTeeInputPin);
          graph.RemoveFilter(infTee);
          Release.ComObject("Microsoft stream selector infinite tee", ref infTee);
          Release.FilterInfo(ref filterInfo);
          graph = null;
        }
      }
      finally
      {
        if (_propertySet != null)
        {
          this.LogInfo("Microsoft stream selector: extension supported");
          _isMicrosoftStreamSelector = true;
          _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
          _paramBuffer = Marshal.AllocCoTaskMem(sizeof(int));
        }
        else
        {
          Release.ComObject("Microsoft stream selector filter output pin", ref pin);
        }
      }
    }

    #endregion

    #region IStreamSelector members

    /// <summary>
    /// Get the identifiers for the available streams.
    /// </summary>
    /// <param name="streamIds">The stream identifiers.</param>
    /// <returns><c>true</c> if the stream identifiers are retrieved successfully, otherwise <c>false</c></returns>
    public bool GetAvailableStreamIds(out ICollection<int> streamIds)
    {
      this.LogDebug("Microsoft stream selector: get available stream IDs");
      streamIds = null;

      if (!_isMicrosoftStreamSelector)
      {
        this.LogWarn("Microsoft stream selector: not initialised or interface not supported");
        return false;
      }

      int returnedByteCount;
      int hr = _propertySet.Get(typeof(IBDA_DigitalDemodulator).GUID, (int)BdaDemodulatorProperty.PlpNumber,
        _instanceBuffer, INSTANCE_SIZE,
        _paramBuffer, sizeof(int),
        out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == sizeof(int))
      {
        int streamCount = Marshal.ReadInt32(_paramBuffer, 0);
        if (streamCount < 0 || streamCount > 256)
        {
          this.LogError("Microsoft stream selector: stream count {0} is invalid", streamCount);
          return false;
        }

        streamIds = new List<int>(streamCount);
        for (int i = 0; i < streamCount; i++)
        {
          streamIds.Add(i);
        }
        this.LogDebug("Microsoft stream selector: result = success");
        return true;
      }

      this.LogError("Microsoft stream selector: failed to get available stream IDs, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      return false;
    }

    /// <summary>
    /// Select a stream.
    /// </summary>
    /// <param name="streamId">The identifier of the stream to select.</param>
    /// <returns><c>true</c> if the stream is selected successfully, otherwise <c>false</c></returns>
    public bool SelectStream(int streamId)
    {
      this.LogDebug("Microsoft stream selector: select stream, stream ID = {0}", streamId);

      if (!_isMicrosoftStreamSelector)
      {
        this.LogWarn("Microsoft stream selector: not initialised or interface not supported");
        return false;
      }

      Marshal.WriteInt32(_paramBuffer, 0, streamId);
      int hr = _propertySet.Set(typeof(IBDA_DigitalDemodulator).GUID, (int)BdaDemodulatorProperty.PlpNumber, _instanceBuffer, INSTANCE_SIZE, _paramBuffer, sizeof(int));
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Microsoft stream selector: result = success");
        return true;
      }

      this.LogError("Microsoft stream selector: failed to select stream, hr = 0x{0:x}, stream ID = {1}", hr, streamId);
      return false;
    }

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

    ~MicrosoftStreamSelector()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
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
      if (isDisposing && _releasePropertySet)
      {
        Release.ComObject("Microsoft stream selector property set", ref _propertySet);
      }
      _isMicrosoftStreamSelector = false;
    }

    #endregion
  }
}