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
using System.Text;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Hardware;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// This API was originally used by Turbosight for their QBOX series tuners.
  /// Turbosight moved to a shared API for their PCIe/PCI and USB tuners. This
  /// class stays to support the QBOX clones from Prof and Omicom which will not
  /// receive updated drivers.
  /// </summary>
  public class ProfUsb : Prof, ICustomTuning, IDiSEqCController, IHardwareProvider, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Tuner = 0,            // For tuning.
      Ir,                   // For retrieving IR codes from the device's IR receiver.
      Tone,                 // For controlling the 22 kHz oscillator.
      Motor,                // For sending raw DiSEqC commands.
      LnbPower,             // For controlling the power supply to the LNB.
      TunerLock,            // For retrieving signal lock, strength and quality information.
      MacAddress,           // For retrieving a device's MAC address.
      DeviceId              // For retrieving a device's ID (name/path???).
    }

    private enum ProfPolarisation : byte
    {
      Horizontal = 0,
      Vertical
    }

    private enum ProfDiseqcPort : byte
    {
      Null = 0,
      PortA,
      PortB,
      PortC,
      PortD
    }

    private enum ProfLnbPower : byte
    {
      Off = 0,
      On
    }

    private enum ProfIrCode : byte
    {
      Recall = 0x80,
      Up1 = 0x81,
      Right1 = 0x82,
      Record = 0x83,
      Power = 0x84,
      Three = 0x85,
      Two = 0x86,
      One = 0x87,
      Down1 = 0x88,
      Six = 0x89,
      Five = 0x8a,
      Four = 0x8b,
      Left1 = 0x8c,
      Nine = 0x8d,
      Eight = 0x8e,
      Seven = 0x8f,
      Left2 = 0x90,
      Up2 = 0x91,
      Zero = 0x92,
      Right2 = 0x93,
      Mute = 0x94,
      Tab = 0x95,
      Down2 = 0x96,
      Epg = 0x97,
      Pause = 0x98,
      Ok = 0x99,
      Snapshot = 0x9a,
      Info = 0x9c,
      Play = 0x9b,
      FullScreen = 0x9d,
      Menu = 0x9e,
      Exit = 0x9f
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BdaExtensionParams
    {
      public UInt32 Frequency;                // The transponder frequency in MHz.
      public UInt32 LnbLowBandLof;            // The LNB's low band local oscillator frequency in MHz.
      public UInt32 LnbHighBandLof;           // The LNB's high band local oscillator frequency in MHz.
      public UInt32 SymbolRate;               // The transponder symbol rate in ks/s.
      public ProfPolarisation Polarisation;    // The transponder polarisation.
      public ProfLnbPower LnbPower;            // Tuner LNB power supply state.
      public Prof22k Tone22k;                  // Tuner 22 kHz oscillator state.
      public ProfToneBurst ToneBurst;          // The "simple" DiSEqC command to send.
      public ProfDiseqcPort DiseqcPort;        // The DiSEqC 1.0 port number.
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
      public byte[] DiseqcRawCommand;         // A raw DiSEqC command for generic DiSEqC device control.
      public ProfIrCode IrCode;                // A button code received from the remote.
      public byte LockState;                  // Tuner/demodulator lock status.
      public byte SignalStrength;             // Tuner/demodulator signal strength measurement.
      public byte SignalQuality;              // Tuner/demodulator signal quality measurement.
      public byte InnerFecRate;               // The transponder inner FEC rate (BinaryConvolutionCodeRate).
      public byte Modulation;                 // The transponder modulation scheme (ModulationType).
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      private byte[] Reserved;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 0xaa, 0x87, 0xb5, 0xe1, 0xdc, 0x41, 0x13);
    private const int BdaExtensionParamsSize = 288;
    private const int MacAddressLength = 6;
    private const int DeviceIdLength = 8;

    #endregion

    #region variables

    private bool _isProfUsb = false;
    private IKsPropertySet _propertySet = null;
    private bool _isCustomTuningSupported = false;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="ProfUsb"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public ProfUsb(IBaseFilter tunerFilter)
      : base (tunerFilter)
    {
      if (!IsProf)
      {
        return;
      }

      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Motor,
                                  out support);
      if (hr != 0)
      {
        Log.Log.Debug("Prof (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }
      if ((support & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Prof (USB): tuner supports the USB interface");
        _isProfUsb = true;

        ReadDeviceInfo();

        SetLnbPowerState(true);
      }
    }

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      Log.Log.Debug("Prof (USB): read device information");

      // Check whether custom tuning is supported.
      Log.Log.Debug("Prof (USB): checking for tuning property support");
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tuner, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Prof (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        if ((support & KSPropertySupport.Set) != 0)
        {
          Log.Log.Debug("Prof (USB): property supported");
          _isCustomTuningSupported = true;
        }
        else
        {
          Log.Log.Debug("Prof (USB): property not supported");
        }
      }

      // MAC address.
      Log.Log.Debug("Prof (USB): reading MAC address");
      for (int i = 0; i < MacAddressLength; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.MacAddress,
        _generalBuffer, MacAddressLength,
        _generalBuffer, MacAddressLength,
        out returnedByteCount
      );
      if (hr != 0)
      {
        Log.Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        String address = String.Empty;
        for (int i = 0; i < returnedByteCount; i++)
        {
          address += String.Format("{0:x2}-", Marshal.ReadByte(_generalBuffer, i));
        }
        Log.Log.Debug("  MAC address = {0}", address.Substring(0, (returnedByteCount * 3) - 1));
      }

      // Device ID.
      Log.Log.Debug("Prof (USB): reading device ID");
      for (int i = 0; i < DeviceIdLength; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DeviceId,
        _generalBuffer, DeviceIdLength,
        _generalBuffer, DeviceIdLength,
        out returnedByteCount
      );
      if (hr != 0)
      {
        Log.Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        Log.Log.Debug("  vendor ID   = {0:x4}", Marshal.ReadInt16(_generalBuffer, 1));
        Log.Log.Debug("  device ID   = {0:x4}", Marshal.ReadInt16(_generalBuffer, 3));
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a Prof USB compatible device.
    /// </summary>
    /// <value><c>true</c> if the hardware supports the Prof USB device DiSEqC interface, otherwise <c>false</c></value>
    public bool IsProfUsb
    {
      get
      {
        return _isProfUsb;
      }
    }

    /// <summary>
    /// Turn the LNB power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn power supply on, otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if the power supply state is set successfully, otherwise <c>false</c></returns>
    public override bool SetLnbPowerState(bool powerOn)
    {
      // (Protect against null reference exception that would be generated when the Turbosight base
      // constructor executes...)
      if (_propertySet == null)
      {
        return false;
      }
      Log.Log.Debug("Prof (USB): set LNB power state, on = {0}", powerOn);
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Prof (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Prof (USB): property not supported");
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      if (powerOn)
      {
        command.LnbPower = ProfLnbPower.On;
      }
      else
      {
        command.LnbPower = ProfLnbPower.Off;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, IntPtr.Zero, 0, _generalBuffer, BdaExtensionParamsSize);
      if (hr == 0)
      {
        Log.Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public override bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Prof (USB): set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tone,
                                  out support);
      if (hr != 0)
      {
        Log.Log.Debug("Prof (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Prof (USB): property not supported");
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      command.ToneBurst = ProfToneBurst.Off;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        command.ToneBurst = ProfToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        command.ToneBurst = ProfToneBurst.DataBurst;
      }
      command.Tone22k = Prof22k.Off;
      if (tone22kState == Tone22k.On)
      {
        command.Tone22k = Prof22k.On;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tone,
        IntPtr.Zero, 0,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #region ICustomTuning members

    /// <summary>
    /// Check if the custom tune method supports tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the custom tune method supports tuning the channel, otherwise <c>false</c></returns>
    public bool SupportsTuningForChannel(IChannel channel)
    {
      if (channel is DVBSChannel)
      {
        return _isCustomTuningSupported;
      }
      return false;
    }

    /// <summary>
    /// Tune to a channel using the custom tune method.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <param name="parameters">The scan parameters.</param>
    /// <returns><c>true</c> if tuning is successful, otherwise <c>false</c></returns>
    public bool CustomTune(IChannel channel, ScanParameters parameters)
    {
      Log.Log.Debug("Prof (USB): tune to channel");
      if (!SupportsTuningForChannel(channel))
      {
        Log.Log.Debug("Prof (USB): custom tuning not supported for this channel");
        return false;
      }

      DVBSChannel dvbsChannel = channel as DVBSChannel;

      int lnbLowLof;
      int lnbHighLof;
      int lnbSwitchFrequency;
      BandTypeConverter.GetDefaultLnbSetup(parameters, dvbsChannel.BandType, out lnbLowLof, out lnbHighLof, out lnbSwitchFrequency);

      ProfPolarisation polarisation = ProfPolarisation.Horizontal;
      if (dvbsChannel.Polarisation == Polarisation.LinearV || dvbsChannel.Polarisation == Polarisation.CircularR)
      {
        polarisation = ProfPolarisation.Vertical;
      }

      bool isHighBand = false;
      uint lnbFrequency = (uint)lnbLowLof;
      if (dvbsChannel.Frequency >= (lnbSwitchFrequency * 1000))
      {
        isHighBand = true;
        lnbFrequency = (uint)lnbHighLof;
      }

      ProfToneBurst toneBurst = ProfToneBurst.Off;
      if (dvbsChannel.DisEqc == DisEqcType.SimpleA)
      {
        toneBurst = ProfToneBurst.ToneBurst;
      }
      else if (dvbsChannel.DisEqc == DisEqcType.SimpleB)
      {
        toneBurst = ProfToneBurst.DataBurst;
      }

      BdaExtensionParams tuningParams = new BdaExtensionParams();
      tuningParams.Frequency = (uint)dvbsChannel.Frequency / 1000;
      tuningParams.LnbLowBandLof = lnbFrequency;
      tuningParams.LnbHighBandLof = lnbFrequency;
      tuningParams.SymbolRate = (uint)dvbsChannel.SymbolRate;
      tuningParams.Polarisation = polarisation;
      tuningParams.LnbPower = ProfLnbPower.On;
      tuningParams.Tone22k = isHighBand ? Prof22k.On : Prof22k.Off;
      tuningParams.ToneBurst = toneBurst;
      // DiSEqC commands are already sent using the raw command interface. No need to resend them and
      // unnecessarily slow down the tune request.
      tuningParams.DiseqcPort = ProfDiseqcPort.Null;
      tuningParams.InnerFecRate = (byte)dvbsChannel.InnerFecRate;
      tuningParams.Modulation = (byte)dvbsChannel.ModulationType;

      Marshal.StructureToPtr(tuningParams, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tuner,
        IntPtr.Zero, 0,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiSEqCController members

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public override bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Prof (USB): send DiSEqC command");

      if (command.Length > 5)
      {
        Log.Log.Debug("Prof (USB): command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.DiseqcRawCommand = new byte[5];
      for (int i = 0; i < command.Length; i++)
      {
        propertyParams.DiseqcRawCommand[i] = command[i];
      }

      Marshal.StructureToPtr(propertyParams, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Motor,
            IntPtr.Zero, 0,
            _generalBuffer, BdaExtensionParamsSize);
      if (hr == 0)
      {
        Log.Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IHardwareProvider members

    /// <summary>
    /// Initialise the hardware provider.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public void Init(IBaseFilter tunerFilter)
    {
      // Not implemented.
    }

    /// <summary>
    /// Get or set a custom device index. Not applicable for Prof USB tuners.
    /// </summary>
    public int DeviceIndex
    {
      get
      {
        return 0;
      }
      set
      {
      }
    }

    /// <summary>
    /// Get or set the tuner device path. Not applicable for Prof USB tuners.
    /// </summary>
    public String DevicePath
    {
      get
      {
        return String.Empty;
      }
      set
      {
      }
    }

    /// <summary>
    /// Get the provider loading priority.
    /// </summary>
    public int Priority
    {
      get
      {
        return 10;
      }
    }

    /// <summary>
    /// Checks if hardware is supported and open the device.
    /// </summary>
    public void CheckAndOpen()
    {
      // Not implemented.
    }

    /// <summary>
    /// Returns the name of the provider.
    /// </summary>
    public String Provider
    {
      get
      {
        return "Prof USB";
      }
    }

    /// <summary>
    /// Returns the result of detection. If false the provider should be disposed.
    /// </summary>
    public bool IsSupported
    {
      get
      {
        return _isProfUsb;
      }
    }

    /// <summary>
    /// Returns the provider capabilities.
    /// </summary>
    public CapabilitiesType Capabilities
    {
      get
      {
        return CapabilitiesType.None;
      }
    }

    #endregion
  }
}
