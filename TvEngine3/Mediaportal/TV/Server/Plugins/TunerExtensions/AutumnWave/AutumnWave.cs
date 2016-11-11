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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.AutumnWave
{
  public class AutumnWave : BaseTunerExtension, ICustomTuner, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty
    {
      TunerId = 0,  // get
      SwReset,      // set
      Mode,         // set
      SsInversion,  // set
      SnrRegister   // get
    }

    private enum AutumnWaveHardware
    {
	    Fcv1236d_Lg3302 = 0,  // Sasem
	    LgTdvsH002f_Lg3302,
	    LgTdvsH062f_Lg3303    // Creator?
    }

    private enum AutumnWaveTunerMode
    {
      Qam64 = 0,
      Qam256 = 1,
      Vsb8 = 3
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0x87f6acbc, 0xbe46, 0x4ea5, 0x90, 0x12, 0x1d, 0x21, 0x1c, 0x47, 0x3f, 0x71);

    private const int BUFFER_SIZE = 4;

    #endregion

    #region variables

    private bool _isAutumnWave = false;
    private DsDevice _device = null;
    private IKsPropertySet _propertySet = null;
    private IntPtr _buffer = IntPtr.Zero;

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("AutumnWave: initialising");

      if (_isAutumnWave)
      {
        this.LogWarn("AutumnWave: extension already initialised");
        return true;
      }

      if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte))
      {
        this.LogDebug("AutumnWave: tuner type not supported");
        return false;
      }

      // Find the corresponding BDA source.
      string productInstanceId = null;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      try
      {
        foreach (DsDevice d in devices)
        {
          string devicePath = d.DevicePath;
          if (devicePath != null && tunerExternalId.Contains(devicePath))
          {
            this.LogDebug("AutumnWave: found BDA source");
            this.LogDebug("AutumnWave:   name                = {0}", d.Name);
            this.LogDebug("AutumnWave:   device path         = {0}", devicePath);
            this.LogDebug("AutumnWave:   product instance ID = {0}", d.ProductInstanceIdentifier);
            productInstanceId = d.ProductInstanceIdentifier;
            break;
          }
        }
        if (productInstanceId == null)
        {
          this.LogDebug("AutumnWave: not a BDA source");
          return false;
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          d.Dispose();
        }
      }

      // Find the corresponding WDM analog tuner device.
      devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSTVTuner);
      try
      {
        foreach (DsDevice d in devices)
        {
          if (productInstanceId.Equals(d.ProductInstanceIdentifier))
          {
            string name = d.Name;
            this.LogDebug("AutumnWave: found analog tuner device");
            this.LogDebug("AutumnWave:   name                = {0}", name);
            this.LogDebug("AutumnWave:   device path         = {0}", d.DevicePath);
            _device = d;
            // Name is expected to be "USB HDTV-CR Tuner" or
            // "USB HDTV-GT Tuner". We don't restrict and instead rely on the
            // property set support check below to decide.
            break;
          }
        }
        if (_device == null)
        {
          this.LogDebug("AutumnWave: failed to find corresponding analog tuner, tuner external ID = {0}, product instance ID = {1}", tunerExternalId, productInstanceId);
          return false;
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          if (d != _device)
          {
            d.Dispose();
          }
        }
      }

      try
      {
        // Get the property set and check it is supported.
        object obj = null;
        Guid filterIid = typeof(IBaseFilter).GUID;
        try
        {
          _device.Mon.BindToObject(null, null, ref filterIid, out obj);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "AutumnWave: failed to create analog tuner filter, name = {0}, device path = {1}", _device.Name, _device.DevicePath);
          return false;
        }
        _propertySet = obj as IKsPropertySet;
        if (_propertySet == null)
        {
          this.LogDebug("AutumnWave: filter is not a property set");
          return false;
        }

        KSPropertySupport support;
        int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Mode, out support);
        if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
        {
          this.LogDebug("AutumnWave: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
          return false;
        }

        this.LogInfo("AutumnWave: extension supported");
        _isAutumnWave = true;
        _buffer = Marshal.AllocCoTaskMem(BUFFER_SIZE);
        int returnedByteCount;
        hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.TunerId, _buffer, BUFFER_SIZE, _buffer, BUFFER_SIZE, out returnedByteCount);
        if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == BUFFER_SIZE)
        {
          this.LogDebug("AutumnWave: hardware = {0}", (AutumnWaveHardware)Marshal.ReadInt32(_buffer, 0));
        }
        else
        {
          this.LogWarn("AutumnWave: failed to read tuner ID, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        }
        return true;
      }
      finally
      {
        if (!_isAutumnWave)
        {
          Release.ComObject("AutumnWave property set", ref _propertySet);
          _propertySet = null;
          _device.Dispose();
          _device = null;
        }
      }
    }

    #endregion

    #region ICustomTuner methods

    /// <summary>
    /// Check if the extension implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the extension supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        return atscChannel.ModulationScheme == ModulationSchemeVsb.Vsb8;
      }
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel != null && !scteChannel.IsCableCardNeededToTune())
      {
        return scteChannel.ModulationScheme == ModulationSchemeQam.Qam64 || scteChannel.ModulationScheme == ModulationSchemeQam.Qam256;
      }
      return false;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("AutumnWave: tune to channel");

      if (!_isAutumnWave)
      {
        this.LogWarn("AutumnWave: not initialised or interface not supported");
        return false;
      }

      AutumnWaveTunerMode mode;
      short physicalChannelNumber;
      ChannelAtsc atscChannel = channel as ChannelAtsc;
      if (atscChannel != null)
      {
        if (atscChannel.ModulationScheme != ModulationSchemeVsb.Vsb8)
        {
          this.LogError("AutumnWave: ATSC tune request uses unsupported modulation scheme {0}", atscChannel.ModulationScheme);
          return false;
        }

        mode = AutumnWaveTunerMode.Vsb8;
        physicalChannelNumber = atscChannel.PhysicalChannelNumber;
      }
      else
      {
        ChannelScte scteChannel = channel as ChannelScte;
        if (channel == null)
        {
          this.LogError("AutumnWave: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
          return false;
        }

        switch (scteChannel.ModulationScheme)
        {
          case ModulationSchemeQam.Qam64:
            mode = AutumnWaveTunerMode.Qam64;
            break;
          case ModulationSchemeQam.Qam256:
            mode = AutumnWaveTunerMode.Qam256;
            break;
          default:
            this.LogError("AutumnWave: SCTE tune request uses unsupported modulation scheme {0}", scteChannel.ModulationScheme);
            return false;
        }
        physicalChannelNumber = scteChannel.PhysicalChannelNumber;
      }

      IAMTVTuner tuner = _propertySet as IAMTVTuner;
      int hr = tuner.put_Mode(AMTunerModeType.DTV);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("AutumnWave: failed to set tuner mode, hr = 0x{0:x}", hr);
        return false;
      }

      if (mode != AutumnWaveTunerMode.Vsb8)
      {
        // Try not inverted first. Inversion is not applicable for VSB.
        Marshal.WriteInt32(_buffer, 0, 0);
        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.SsInversion, _buffer, BUFFER_SIZE, _buffer, BUFFER_SIZE);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("AutumnWave: failed to set inversion mode, hr = 0x{0:x}", hr);
          return false;
        }
      }

      Marshal.WriteInt32(_buffer, 0, (int)mode);
      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Mode, _buffer, BUFFER_SIZE, _buffer, BUFFER_SIZE);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("AutumnWave: failed to set mode, hr = 0x{0:x}, mode = {1}", hr, mode);
        return false;
      }

      int foundSignal;
      hr = tuner.AutoTune(physicalChannelNumber, out foundSignal);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("AutumnWave: failed to set physical channel, hr = 0x{0:x}, channel number = {1}", hr, physicalChannelNumber);
        return false;
      }

      if (foundSignal == 0 && mode != AutumnWaveTunerMode.Vsb8)
      {
        // Try inverted.
        Marshal.WriteInt32(_buffer, 0, 1);
        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.SsInversion, _buffer, BUFFER_SIZE, _buffer, BUFFER_SIZE);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("AutumnWave: failed to set inversion mode, hr = 0x{0:x}", hr);
          return false;
        }

        AMTunerSignalStrength strength;
        hr = tuner.SignalPresent(out strength);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("AutumnWave: failed to check signal status, hr = 0x{0:x}", hr);
          return false;
        }
        foundSignal = (int)strength;
      }

      if (foundSignal == 0)
      {
        this.LogWarn("AutumnWave: failed to lock on signal");
      }
      else
      {
        this.LogDebug("AutumnWave: result = success");
      }
      return true;
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

    ~AutumnWave()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        Release.ComObject("AutumnWave property set", ref _propertySet);
        if (_device != null)
        {
          _device.Dispose();
          _device = null;
        }
      }
      if (_buffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_buffer);
        _buffer = IntPtr.Zero;
      }
      _isAutumnWave = false;
    }

    #endregion
  }
}