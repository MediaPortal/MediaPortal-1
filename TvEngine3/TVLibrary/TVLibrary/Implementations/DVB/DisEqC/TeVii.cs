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
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Hardware;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Hardware
{
  /// <summary>
  /// A class for handling DiSEqC and tuning for TeVii tuners.
  /// </summary>
  public class TeVii : ICustomTuning, IDiSEqCController, IHardwareProvider, IDisposable
  {
    #region enums

    private enum TeViiPolarisation
    {
      None = 0,
      Vertical,     // also use for circular right
      Horizontal    // also use for circular left
    }

    private enum TeViiModulation
    {
      Auto = 0,
      Qpsk,
      Bpsk,
      Qam16,
      Qam32,
      Qam64,
      Qam128,
      Qam256,
      Vsb8,
      Dvbs2_Qpsk,
      Dvbs2_8Psk,
      Dvbs2_16Apsk,
      Dvbs2_32Apsk,
      TurboQPsk,
      Turbo8Psk,
      Turbo16Psk
    }

    private enum TeViiFecRate
    {
      Auto = 0,
      Rate1_2,
      Rate1_3,
      Rate1_4,
      Rate2_3,
      Rate2_5,
      Rate3_4,
      Rate3_5,
      Rate4_5,
      Rate5_6,
      Rate5_11,
      Rate6_7,
      Rate7_8,
      Rate8_9,
      Rate9_10
    }

    #endregion

    #region Dll imports

    //////////////////////////////////////////////////////////////////////////
    // Information functions
    // these functions don't require opening of device

    /// <summary>
    /// Get the SDK API version number.
    /// </summary>
    /// <returns>the API Version number</returns>
    [DllImport("TeVii.dll", EntryPoint = "GetAPIVersion", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetAPIVersion();

    /// <summary>
    /// Enumerate the TeVii-compatible devices in the system. This function should be
    /// called before any other functions are called. Only the first call will really
    /// enumerate. Subsequent calls will just return the result from the first call.
    /// </summary>
    /// <returns>the number of TeVii-compatible devices connected to the system</returns>
    [DllImport("TeVii.dll", EntryPoint = "FindDevices", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern int FindDevices();

    /// <summary>
    /// Get the friendly name for a specific TeVii device. Note: do not modify or free
    /// the memory associated with the returned pointer!
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <returns>a pointer to a NULL terminated buffer containing the device name, otherwise <c>IntPtr.Zero</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "GetDeviceName", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetDeviceName(int idx);

    /// <summary>
    /// Get the device path for a specific TeVii device. Note: do not modify or free
    /// the memory associated with the returned pointer!
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <returns>a pointer to a NULL terminated buffer containing the device path, otherwise <c>IntPtr.Zero</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "GetDevicePath", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetDevicePath(int idx);

    //////////////////////////////////////////////////////////////////////////
    // Following functions work only after call OpenDevice()
    //

    /// <summary>
    /// Open access to a specific TeVii device. The idx parameter specifies which device will be opened.
    /// It is possible to have access to multiple devices simultaneously.
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <param name="captureCallback">An optional pointer to a function that will be executed when raw stream packets are received.</param>
    /// <param name="context">An optional pointer that will be passed as a paramter to the capture callback.</param>
    /// <returns><c>true</c> if the device access is successfully established, otherwise <c>false</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "OpenDevice", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OpenDevice(int idx, IntPtr captureCallback, IntPtr context);

    /// <summary>
    /// Close access to a specific TeVii device.
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <returns><c>true</c> if the device access is successfully closed, otherwise <c>false</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "CloseDevice", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool CloseDevice(int idx);

    /// <summary>
    /// Tune a TeVii DVB-S/S2 tuner to a specific satellite transponder.
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <param name="frequency">The transponder frequency in kHz (eg. 12450000).</param>
    /// <param name="symbolRate">The transponder symbol rate in sps (eg. 27500000).</param>
    /// <param name="lnbLof">The LNB local oscillator frequency offset in kHz (eg. 9570000).</param>
    /// <param name="polarisation">The transponder polarisation.</param>
    /// <param name="toneOn"><c>True</c> to turn the 22 kHz oscillator on (to force the LNB to high band mode or switch to port 2).</param>
    /// <param name="modulation">The transponder modulation. Note that it's better to avoid using <c>TeViiModulation.Auto</c> for DVB-S2 transponders to minimise lock time.</param>
    /// <param name="fecRate">The transponder FEC rate. Note that it's better to avoid using <c>TeViiFecRate.Auto</c> for DVB-S2 transponders to minimise lock time.</param>
    /// <returns><c>true</c> if the tuner successfully locks on the transponder, otherwise <c>false</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "TuneTransponder", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool TuneTransponder(int idx, Int32 frequency, Int32 symbolRate, Int32 lnbLof, TeViiPolarisation polarisation,
                                               bool toneOn, TeViiModulation modulation, TeViiFecRate fecRate);

    /// <summary>
    /// Get the current signal status for a specific TeVii tuner device.
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <param name="isLocked"><c>True</c> if the tuner/demodulator are locked onto a transponder.</param>
    /// <param name="strength">A signal strength rating ranging between 0 (low strength) and 100 (high strength).</param>
    /// <param name="quality">A signal quality rating ranging between 0 (low quality) and 100 (high quality).</param>
    /// <returns><c>true</c> if the signal status is successfully retrieved, otherwise <c>false</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "GetSignalStatus", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool GetSignalStatus(int idx, out bool isLocked, out Int32 strength, out Int32 quality);

    /// <summary>
    /// Send an arbitrary DiSEqC message.
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <param name="message">The DiSEqC message bytes.</param>
    /// <param name="length">The message length in bytes.</param>
    /// <param name="repeatCount">The number of times to resend the message. Zero means send the message once.</param>
    /// <param name="repeatFlag"><c>True</c> to set the first byte in the message to 0xe1 if/when the message is resent.</param>
    /// <returns><c>true</c> if the message is successfully sent, otherwise <c>false</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "SendDiSEqC", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern bool SendDiSEqC(int idx, byte[] message, Int32 length, Int32 repeatCount, bool repeatFlag);

    /// <summary>
    /// Set the remote control receiver callback function.
    /// </summary>
    /// <param name="idx">The zero-based device index (0 &lt;= idx &lt; FindDevices()).</param>
    /// <param name="remoteKeyCallback">An optional pointer to a function that will be called when remote keypress events are detected.</param>
    /// <param name="context">An optional pointer that will be passed as a paramter to the remote key callback.</param>
    /// <returns><c>true</c> if the callback function is successfully set, otherwise <c>false</c></returns>
    [DllImport("TeVii.dll", EntryPoint = "SetRemoteControl", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl)]
    private static extern Int32 SetRemoteControl(int idx, IntPtr remoteKeyCallback, IntPtr context);

    #endregion

    #region variables

    private int _deviceIndex = -1;
    private bool _isTeVii = false;
    private CardType _tunerType = CardType.Unknown;
    private Tone22k _toneState = Tone22k.Auto;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="TeVii"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The tuner device path.</param>
    public TeVii(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Int32 deviceCount = FindDevices();
      Log.Log.Debug("TeVii: number of devices = {0}, tuner device path = {1}", deviceCount, tunerDevicePath);
      if (deviceCount == 0)
      {
        return;
      }

      String deviceName = String.Empty;
      String devicePath = String.Empty;
      for (int deviceIdx = 0; deviceIdx < deviceCount; deviceIdx++)
      {
        deviceName = Marshal.PtrToStringAnsi(GetDeviceName(deviceIdx));
        devicePath = Marshal.PtrToStringAnsi(GetDevicePath(deviceIdx));

        //Log.Log.Debug("TeVii: compare to {0} {1}", deviceName, devicePath);
        if (devicePath.Equals(tunerDevicePath))
        {
          Log.Log.Debug("TeVii: device recognised, index = {0}, name = {1}, API version = {2}", _deviceIndex, deviceName, GetAPIVersion());
          _deviceIndex = deviceIdx;
          break;
        }
      }

      if (_deviceIndex != -1)
      {
        return;
      }

      if (!OpenDevice(_deviceIndex, IntPtr.Zero, IntPtr.Zero))
      {
        return;
      }
      Log.Log.Debug("TeVii: supported tuner detected");
      _isTeVii = true;
      _tunerType = tunerType;
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a TeVii-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a TeVii-compatible tuner, otherwise <c>false</c></value>
    public bool IsTeVii
    {
      get
      {
        return _isTeVii;
      }
    }

    /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("TeVii: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      // We don't have the ability to send burst commands, and for the continuous
      // tone we just set a variable here that will be passed as part of the next
      // tune request.
      _toneState = tone22kState;
      Log.Log.Debug("TeVii: result = success");
      return true;
    }

    #region ICustomTuning members

    /// <summary>
    /// Check if the custom tune method supports tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the custom tune method supports tuning the channel, otherwise <c>false</c></returns>
    public bool SupportsTuningForChannel(IChannel channel)
    {
      // Tuning of DVB-S/S2 channels is supported with an appropriate tuner.
      if (channel is DVBSChannel && _tunerType == CardType.DvbS)
      {
        return true;
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
      Log.Log.Debug("TeVii: tune to channel");

      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return false;
      }

      int lowLof;
      int highLof;
      int switchFrequency;
      BandTypeConverter.GetDefaultLnbSetup(parameters, ch.BandType, out lowLof, out highLof, out switchFrequency);

      int lnbLof;
      bool toneOn = false;
      if (BandTypeConverter.IsHiBand(ch, parameters))
      {
        lnbLof = highLof * 1000;
        toneOn = true;
      }
      else
      {
        lnbLof = lowLof * 1000;
      }

      // Override the default tone state with the state set in SetToneState().
      if (_toneState == Tone22k.Off)
      {
        toneOn = false;
      }
      else if (_toneState == Tone22k.On)
      {
        toneOn = true;
      }

      bool result = TuneTransponder(_deviceIndex, (int)ch.Frequency, ch.SymbolRate * 1000, lnbLof,
        Translate(ch.Polarisation), toneOn, Translate(ch.ModulationType), Translate(ch.InnerFecRate));
      if (result)
      {
        Log.Log.Debug("TeVii: result = success");
      }
      else
      {
        Log.Log.Debug("TeVii: result = failure");
      }

      // Reset the tone state to auto. SetToneState() must be called again to override
      // the default logic.
      _toneState = Tone22k.Auto;
      return result;
    }

    private TeViiPolarisation Translate(Polarisation pol)
    {
      switch (pol)
      {
        case Polarisation.CircularL:
        case Polarisation.LinearH:
          return TeViiPolarisation.Horizontal;
        case Polarisation.CircularR:
        case Polarisation.LinearV:
          return TeViiPolarisation.Vertical;
        default:
          return TeViiPolarisation.None;
      }
    }

    private TeViiModulation Translate(ModulationType mod)
    {
      switch (mod)
      {
        // DVB-C, DVB-T and North American cable
        case ModulationType.Mod16Qam:
          return TeViiModulation.Qam16;
        case ModulationType.Mod32Qam:
          return TeViiModulation.Qam32;
        case ModulationType.Mod64Qam:
          return TeViiModulation.Qam64;
        case ModulationType.Mod128Qam:
          return TeViiModulation.Qam128;
        case ModulationType.Mod256Qam:
          return TeViiModulation.Qam256;

        // ATSC
        case ModulationType.Mod8Vsb:
          return TeViiModulation.Vsb8;

        // DVB-S/S2
        case ModulationType.ModQpsk:
          return TeViiModulation.TurboQPsk;
        case ModulationType.ModBpsk:
          return TeViiModulation.Bpsk;
        case ModulationType.Mod8Psk:
          return TeViiModulation.Turbo8Psk;
        case ModulationType.Mod16Apsk:
          return TeViiModulation.Turbo16Psk;
        case ModulationType.ModNbcQpsk:
          return TeViiModulation.Dvbs2_Qpsk;
        case ModulationType.ModNbc8Psk:
          return TeViiModulation.Dvbs2_8Psk;

        default:
          return TeViiModulation.Auto;
      }
    }

    private TeViiFecRate Translate(BinaryConvolutionCodeRate fec)
    {
      switch (fec)
      {
        case BinaryConvolutionCodeRate.Rate1_2:
          return TeViiFecRate.Rate1_2;
        case BinaryConvolutionCodeRate.Rate2_3:
          return TeViiFecRate.Rate2_3;
        case BinaryConvolutionCodeRate.Rate3_4:
          return TeViiFecRate.Rate3_4;
        case BinaryConvolutionCodeRate.Rate3_5:
          return TeViiFecRate.Rate3_5;
        case BinaryConvolutionCodeRate.Rate4_5:
          return TeViiFecRate.Rate4_5;
        case BinaryConvolutionCodeRate.Rate5_6:
          return TeViiFecRate.Rate5_6;
        case BinaryConvolutionCodeRate.Rate5_11:
          return TeViiFecRate.Rate5_11;
        case BinaryConvolutionCodeRate.Rate7_8:
          return TeViiFecRate.Rate7_8;
        case BinaryConvolutionCodeRate.Rate1_4:
          return TeViiFecRate.Rate1_4;
        case BinaryConvolutionCodeRate.Rate1_3:
          return TeViiFecRate.Rate1_3;
        case BinaryConvolutionCodeRate.Rate2_5:
          return TeViiFecRate.Rate2_5;
        case BinaryConvolutionCodeRate.Rate6_7:
          return TeViiFecRate.Rate6_7;
        case BinaryConvolutionCodeRate.Rate8_9:
          return TeViiFecRate.Rate8_9;
        case BinaryConvolutionCodeRate.Rate9_10:
          return TeViiFecRate.Rate9_10;
        case BinaryConvolutionCodeRate.RateMax:
        default:
          return TeViiFecRate.Auto;
      }
    }

    #endregion

    #region IDiSEqCController members

    /// <summary>
    /// Send the appropriate DiSEqC 1.0 switch command to switch to a given channel.
    /// </summary>
    /// <param name="parameters">The scan parameters.</param>
    /// <param name="channel">The channel.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(ScanParameters parameters, DVBSChannel channel)
    {
      bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst toneBurst = ToneBurst.Off;
      bool successDiseqc = true;
      if (channel.DisEqc == DisEqcType.SimpleA)
      {
        toneBurst = ToneBurst.ToneBurst;
      }
      else if (channel.DisEqc == DisEqcType.SimpleB)
      {
        toneBurst = ToneBurst.DataBurst;
      }
      else if (channel.DisEqc != DisEqcType.None)
      {
        int antennaNr = BandTypeConverter.GetAntennaNr(channel);
        bool isHorizontal = ((channel.Polarisation == Polarisation.LinearH) ||
                              (channel.Polarisation == Polarisation.CircularL));
        byte command = 0xf0;
        command |= (byte)(isHighBand ? 1 : 0);
        command |= (byte)((isHorizontal) ? 2 : 0);
        command |= (byte)((antennaNr - 1) << 2);
        successDiseqc = SendDiSEqCCommand(new byte[4] { 0xe0, 0x10, 0x38, command });
      }

      Tone22k tone22k = Tone22k.Off;
      if (isHighBand)
      {
        tone22k = Tone22k.On;
      }
      bool successTone = SetToneState(toneBurst, tone22k);

      return (successDiseqc && successTone);
    }

    /// <summary>
    /// Send a DiSEqC command.
    /// </summary>
    /// <param name="command">The DiSEqC command to send.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("TeVii: send DiSEqC command");
      bool result = SendDiSEqC(_deviceIndex, command, command.Length, 0, false);
      if (result)
      {
        Log.Log.Debug("TeVii: result = success");
        return true;
      }

      Log.Log.Debug("TeVii: result = failure");
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      // (Not implemented...)
      reply = null;
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
    /// Get or set a custom device index.
    /// </summary>
    public int DeviceIndex
    {
      get
      {
        return _deviceIndex;
      }
      set
      {
      }
    }

    /// <summary>
    /// Get or set the tuner device path.
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
        return "TeVii";
      }
    }

    /// <summary>
    /// Returns the result of detection. If <c>false</c> the provider should be disposed.
    /// </summary>
    public bool IsSupported
    {
      get
      {
        return _isTeVii;
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

    #region IDisposable member

    /// <summary>
    /// Close the hardware access interface.
    /// </summary>
    public void Dispose()
    {
      if (_isTeVii)
      {
        CloseDevice(_deviceIndex);
      }
    }

    #endregion
  }
}