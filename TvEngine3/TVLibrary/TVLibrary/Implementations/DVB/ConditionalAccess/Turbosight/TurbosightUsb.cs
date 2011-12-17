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
  /// Handles sending DiSEqC commands for Turbosight USB DVB-S/S2 devices. In particular
  /// that includes the QBOX series of products and clones from Prof and Omicom.
  /// </summary>
  public class TurbosightUsb : Turbosight, ICustomTuning, IDiSEqCController, ICiMenuActions, IHardwareProvider, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Tuner = 0,            // General purpose extension for tuning.
      Ir,                   // Custom property for retrieving IR codes from the device's IR receiver.
      Tone,                 // Custom property for turning the 22 kHz tone on or off and sending tone burst commands.
      Motor,                // Custom property for sending raw DiSEqC commands.
      LnbPower,             // Custom property for turning power to the LNB on or off.
      TunerLock,            // Custom property for retrieving signal lock, strength and quality information.
      MacAddress,           // Custom property for retrieving a device's MAC address.
      DeviceId,             // Custom property for retrieving a device's ID (name/path???).
      SendPmt,              // Custom property for sending PMT to a CAM.
      ResetCi,              // Custom property for resetting the common interface.
      GetCiStatus           // Custom property for retrieving the common interface status.
    }

    private enum TbsPolarisation : byte
    {
      Horizontal = 0,
      Vertical
    }

    private enum TbsDiseqcPort : byte
    {
      Null = 0,
      PortA,
      PortB,
      PortC,
      PortD
    }

    private enum TbsLnbPower : byte
    {
      Off = 0,
      On
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
      public TbsPolarisation Polarisation;    // The transponder polarisation.
      public TbsLnbPower LnbPower;            // Tuner LNB power supply state.
      public Tbs22k Tone22k;                  // Tuner 22 kHz oscillator state.
      public TbsToneBurst ToneBurst;          // The "simple" DiSEqC command to send.
      public TbsDiseqcPort DiseqcPort;        // The DiSEqC 1.0 port number.
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
      public byte[] DiseqcRawCommand;         // A raw DiSEqC command for generic DiSEqC device control.
      public TbsIrCode IrCode;                // A button code received from the remote.
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

    #endregion

    #region variables

    private bool _isTurbosightUsb = false;
    private IKsPropertySet _propertySet = null;
    private bool _isCustomTuningSupported = false;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="TurbosightUsb"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public TurbosightUsb(IBaseFilter tunerFilter)
      : base (tunerFilter)
    {
      if (!IsTurbosight)
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
        Log.Log.Debug("Turbosight (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }
      if ((support & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Turbosight (USB): tuner supports the USB interface");
        _isTurbosightUsb = true;

        // Check whether custom tuning is supported.
        Log.Log.Debug("Turbosight (USB): checking for tuning property support");
        hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tuner, out support);
        if (hr != 0)
        {
          Log.Log.Debug("Turbosight (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }
        else
        {
          if ((support & KSPropertySupport.Set) != 0)
          {
            Log.Log.Debug("Turbosight (USB): property supported");
            _isCustomTuningSupported = true;
          }
          else
          {
            Log.Log.Debug("Turbosight (USB): property not supported");
          }
        }

        SetLnbPowerState(true);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a Turbosight USB compatible device.
    /// </summary>
    /// <value><c>true</c> if the hardware supports the Turbosight USB device DiSEqC interface, otherwise <c>false</c></value>
    public bool IsTurbosightUsb
    {
      get
      {
        return _isTurbosightUsb;
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
      Log.Log.Debug("Turbosight (USB): set LNB power state, on = {0}", powerOn);
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Turbosight (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Turbosight (USB): property not supported");
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      if (powerOn)
      {
        command.LnbPower = TbsLnbPower.On;
      }
      else
      {
        command.LnbPower = TbsLnbPower.Off;
      }

      Marshal.StructureToPtr(command, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.LnbPower, IntPtr.Zero, 0, _generalBuffer, BdaExtensionParamsSize);
      //Thread.Sleep(100);
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight (USB): result = success");
        return true;
      }

      Log.Log.Debug("Turbosight (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    protected override bool SetToneState(TbsToneBurst toneBurstState, Tbs22k tone22kState)
    {
      Log.Log.Debug("Turbosight (USB): set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tone,
                                  out support);
      if (hr != 0)
      {
        Log.Log.Debug("Turbosight (USB): failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      if ((support & KSPropertySupport.Set) == 0)
      {
        Log.Log.Debug("Turbosight (USB): property not supported");
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      command.ToneBurst = toneBurstState;
      command.Tone22k = tone22kState;

      Marshal.StructureToPtr(command, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tone,
        IntPtr.Zero, 0,
        _generalBuffer, BdaExtensionParamsSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight (USB): result = success");
        return true;
      }

      Log.Log.Debug("Turbosight (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      Log.Log.Debug("Turbosight (USB): tune to channel");
      if (!SupportsTuningForChannel(channel))
      {
        Log.Log.Debug("Turbosight (USB): custom tuning not supported for this channel");
        return false;
      }

      DVBSChannel dvbsChannel = channel as DVBSChannel;

      int lnbLowLof;
      int lnbHighLof;
      int lnbSwitchFrequency;
      BandTypeConverter.GetDefaultLnbSetup(parameters, dvbsChannel.BandType, out lnbLowLof, out lnbHighLof, out lnbSwitchFrequency);

      TbsPolarisation polarisation = TbsPolarisation.Horizontal;
      if (dvbsChannel.Polarisation == Polarisation.LinearV || dvbsChannel.Polarisation == Polarisation.CircularR)
      {
        polarisation = TbsPolarisation.Vertical;
      }

      bool isHighBand = false;
      uint lnbFrequency = (uint)lnbLowLof;
      if (dvbsChannel.Frequency >= (lnbSwitchFrequency * 1000))
      {
        isHighBand = true;
        lnbFrequency = (uint)lnbHighLof;
      }

      int antennaNr = BandTypeConverter.GetAntennaNr(dvbsChannel);
      TbsToneBurst toneBurst = TbsToneBurst.DataBurst;
      if (antennaNr == 1)
      {
        toneBurst = TbsToneBurst.ToneBurst;
      }
      else if (antennaNr == 0)
      {
        toneBurst = TbsToneBurst.Off;
      }

      BdaExtensionParams tuningParams = new BdaExtensionParams();
      tuningParams.Frequency = (uint)dvbsChannel.Frequency / 1000;
      tuningParams.LnbLowBandLof = lnbFrequency;
      tuningParams.LnbHighBandLof = lnbFrequency;
      tuningParams.SymbolRate = (uint)dvbsChannel.SymbolRate;
      tuningParams.Polarisation = polarisation;
      tuningParams.LnbPower = TbsLnbPower.On;
      tuningParams.Tone22k = isHighBand ? Tbs22k.On : Tbs22k.Off;
      tuningParams.ToneBurst = toneBurst;
      tuningParams.DiseqcPort = (TbsDiseqcPort)antennaNr;
      tuningParams.InnerFecRate = (byte)dvbsChannel.InnerFecRate;
      tuningParams.Modulation = (byte)dvbsChannel.ModulationType;

      Marshal.StructureToPtr(tuningParams, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tuner,
        IntPtr.Zero, 0,
        _generalBuffer, BdaExtensionParamsSize
      );
      //Thread.Sleep(300);
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight (USB): result = success");
        return true;
      }

      Log.Log.Debug("Turbosight (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      Log.Log.Debug("Turbosight (USB): send DiSEqC command");

      if (command.Length > 5)
      {
        Log.Log.Debug("Turbosight (USB): command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.DiseqcRawCommand = new byte[5];
      for (int i = 0; i < command.Length; i++)
      {
        propertyParams.DiseqcRawCommand[i] = command[i];
      }

      Marshal.StructureToPtr(propertyParams, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, BdaExtensionParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Motor,
            IntPtr.Zero, 0,
            _generalBuffer, BdaExtensionParamsSize);
      if (hr == 0)
      {
        Log.Log.Debug("Turbosight (USB): result = success");
        return true;
      }

      Log.Log.Debug("Turbosight (USB): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
    /// Get or set a custom device index. Not applicable for Turbosight tuners.
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
    /// Get or set the tuner device path. Not applicable for Turbosight tuners.
    /// </summary>
    public String DevicePath
    {
      get
      {
        return "";
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
        return "Turbosight USB";
      }
    }

    /// <summary>
    /// Returns the result of detection. If false the provider should be disposed.
    /// </summary>
    public bool IsSupported
    {
      get
      {
        return _isTurbosightUsb;
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
