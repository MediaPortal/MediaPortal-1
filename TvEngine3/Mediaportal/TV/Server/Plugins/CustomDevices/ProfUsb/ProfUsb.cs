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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.ProfUsb
{
  /// <summary>
  /// This API was originally used by Turbosight for their QBOX series devices. Turbosight moved to a unified
  /// API for their PCIe/PCI and USB devices. This class stays to support the QBOX clones from Prof and
  /// Omicom which will not receive updated drivers.
  /// </summary>
  public class ProfUsb : Prof.Prof, ICustomTuner
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
      DeviceId              // For retrieving a device's ID (device path section).
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
      public UInt32 Frequency;                // unit = MHz
      // Note that the driver does not automatically enable and disable the 22 kHz tone. Further, it is not
      // clear how the driver interprets these parameters. I recommend that the same frequency should be
      // passed in both parameters.
      public UInt32 LnbLowBandLof;            // unit = MHz
      public UInt32 LnbHighBandLof;           // unit = MHz
      public UInt32 SymbolRate;               // unit = ks/s
      public ProfPolarisation Polarisation;
      public ProfLnbPower LnbPower;           // BdaExtensionProperty.LnbPower
      public Prof22k Tone22k;                 // BdaExtensionProperty.Tone
      public ProfToneBurst ToneBurst;         // BdaExtensionProperty.Tone
      public ProfDiseqcPort DiseqcPort;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] DiseqcRawCommand;         // BdaExtensionProperty.Motor
      public ProfIrCode IrCode;               // BdaExtensionProperty.Ir
      public byte LockState;                  // BdaExtensionProperty.TunerLock
      public byte SignalStrength;             // BdaExtensionProperty.TunerLock
      public byte SignalQuality;              // BdaExtensionProperty.TunerLock
      public byte InnerFecRate;               // (BinaryConvolutionCodeRate)
      public byte Modulation;                 // (ModulationType)
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      private byte[] Reserved;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 0xaa, 0x87, 0xb5, 0xe1, 0xdc, 0x41, 0x13);
    private const int BdaExtensionParamsSize = 288;
    private const int MaxDiseqcMessageLength = 5;
    private const int MacAddressLength = 6;
    private const int DeviceIdLength = 8;

    #endregion

    #region variables

    private bool _isProfUsb = false;
    private IKsPropertySet _propertySet = null;
    private bool _isCustomTuningSupported = false;

    #endregion

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      Log.Debug("Prof (USB): read device information");

      // Check whether custom tuning is supported.
      Log.Debug("Prof (USB): checking for tuning property support");
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tuner, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Debug("Prof (USB): property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        Log.Debug("Prof (USB): property supported");
        _isCustomTuningSupported = true;
      }

      // MAC address.
      Log.Debug("Prof (USB): reading MAC address");
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
      if (hr != 0 || returnedByteCount != MacAddressLength)
      {
        Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        String address = String.Empty;
        for (int i = 0; i < returnedByteCount; i++)
        {
          address += String.Format("{0:x2}-", Marshal.ReadByte(_generalBuffer, i));
        }
        Log.Debug("  MAC address = {0}", address.Substring(0, (returnedByteCount * 3) - 1));
      }

      // Device ID.
      Log.Debug("Prof (USB): reading device ID");
      for (int i = 0; i < DeviceIdLength; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DeviceId,
        _generalBuffer, DeviceIdLength,
        _generalBuffer, DeviceIdLength,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != DeviceIdLength)
      {
        Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      else
      {
        // I'm unsure of the meaning of the first, fifth, sixth, seventh and eighth bytes.
        Log.Debug("  vendor ID   = {0:x4}", Marshal.ReadInt16(_generalBuffer, 1));
        Log.Debug("  device ID   = {0:x4}", Marshal.ReadInt16(_generalBuffer, 3));
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
        return 65;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return "Prof (USB)";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Log.Debug("Prof (USB): initialising device");

      if (_isProfUsb)
      {
        Log.Debug("Prof (USB): device is already initialised");
        return true;
      }

      if (!base.Initialise(tunerFilter, tunerType, tunerDevicePath))
      {
        Log.Debug("Prof (USB): base Prof interface not supported");
        return false;
      }

      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Debug("Prof (USB): tuner filter is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Motor, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Debug("Prof (USB): device does not support the Prof USB property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      Log.Debug("Prof (USB): tuner supports the USB interface");
      _isProfUsb = true;
      ReadDeviceInfo();
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
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out DeviceAction action)
    {
      Log.Debug("Prof (USB): on before tune callback");
      action = DeviceAction.Default;

      if (!_isProfUsb || _propertySet == null)
      {
        Log.Debug("Prof (USB): device not initialised or interface not supported");
        return;
      }

      // We only need to tweak the modulation for DVB-S/S2 channels.
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }

      if (ch.Frequency > ch.LnbType.SwitchFrequency)
      {
        ch.LnbType.LowBandFrequency = ch.LnbType.HighBandFrequency;
      }
      else
      {
        ch.LnbType.HighBandFrequency = ch.LnbType.LowBandFrequency;
      }
      Log.Debug("  LNB LOF    = {0}", ch.LnbType.LowBandFrequency);

      base.OnBeforeTune(tuner, currentChannel, ref channel, out action);
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Turn the device power supply on or off.
    /// </summary>
    /// <param name="powerOn"><c>True</c> to turn the power supply on; <c>false</c> to turn the power supply off.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public override bool SetPowerState(bool powerOn)
    {
      Log.Debug("Prof (USB): set power state, on = {0}", powerOn);

      if (!_isProfUsb || _propertySet == null)
      {
        Log.Debug("Prof (USB): device not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Debug("Prof (USB): property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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

      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower,
        IntPtr.Zero, 0,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the device implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      if (channel is DVBSChannel)
      {
        return _isCustomTuningSupported;
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
      Log.Debug("Prof (USB): tune to channel");

      if (!_isProfUsb || _propertySet == null)
      {
        Log.Debug("Prof (USB): device not initialised or interface not supported");
        return false;
      }
      if (!CanTuneChannel(channel))
      {
        Log.Debug("Prof (USB): tuning is not supported for this channel");
        return false;
      }

      DVBSChannel dvbsChannel = channel as DVBSChannel;

      ProfPolarisation profPolarisation = ProfPolarisation.Horizontal;
      if (dvbsChannel.Polarisation == Polarisation.LinearV || dvbsChannel.Polarisation == Polarisation.CircularR)
      {
        profPolarisation = ProfPolarisation.Vertical;
      }

      Prof22k tone22k = Prof22k.Off;
      if (dvbsChannel.Frequency > dvbsChannel.LnbType.SwitchFrequency)
      {
        tone22k = Prof22k.On;
      }

      ProfToneBurst toneBurst = ProfToneBurst.Off;
      if (dvbsChannel.Diseqc == DiseqcPort.SimpleA)
      {
        toneBurst = ProfToneBurst.ToneBurst;
      }
      else if (dvbsChannel.Diseqc == DiseqcPort.SimpleB)
      {
        toneBurst = ProfToneBurst.DataBurst;
      }

      BdaExtensionParams tuningParams = new BdaExtensionParams();
      tuningParams.Frequency = (uint)dvbsChannel.Frequency / 1000;
      // See the notes for the struct to understand why we do this. Note that OnBeforeTune() ensures that the low LOF
      // is set appropriately.
      UInt32 lnbLof = (UInt32)dvbsChannel.LnbType.LowBandFrequency / 1000;
      tuningParams.LnbLowBandLof = lnbLof;
      tuningParams.LnbHighBandLof = lnbLof;
      tuningParams.SymbolRate = (uint)dvbsChannel.SymbolRate;
      tuningParams.Polarisation = profPolarisation;
      tuningParams.LnbPower = ProfLnbPower.On;
      tuningParams.Tone22k = tone22k;
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
        Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22k">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public override bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Debug("Prof (USB): set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isProfUsb || _propertySet == null)
      {
        Log.Debug("Prof (USB): device not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tone,
                                  out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Debug("Prof (USB): property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
        Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public override bool SendCommand(byte[] command)
    {
      Log.Debug("Prof (USB): send DiSEqC command");

      if (!_isProfUsb || _propertySet == null)
      {
        Log.Debug("Prof (USB): device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("Prof (USB): command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Debug("Prof (USB): command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.DiseqcRawCommand = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, propertyParams.DiseqcRawCommand, 0, command.Length);

      Marshal.StructureToPtr(propertyParams, _generalBuffer, true);
      //DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Motor,
        IntPtr.Zero, 0,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        Log.Debug("Prof (USB): result = success");
        return true;
      }

      Log.Debug("Prof (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      _propertySet = null;
      _isProfUsb = false;
      base.Dispose();
    }

    #endregion
  }
}
