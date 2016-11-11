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
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.ProfUsb
{
  /// <summary>
  /// This API was originally used by Turbosight for their QBOX series tuners. Turbosight moved to
  /// a more unified API for their tuners in 2011. This class stays to support the OEM clones from
  /// Prof, Satrade and Omicom which will not receive updated drivers.
  /// </summary>
  public class ProfUsb : BaseTunerExtension, ICustomTuner, IDiseqcDevice, IDisposable, IPowerDevice, IRemoteControlListener
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

    private enum ProfUsbPolarisation : byte
    {
      Horizontal = 0,
      Vertical
    }

    private enum ProfUsbDiseqcPort : byte
    {
      Null = 0,
      PortA,
      PortB,
      PortC,
      PortD
    }

    private enum ProfUsbTone22kState : byte
    {
      Off = 0,
      On
    }

    private enum ProfUsbToneBurst : byte
    {
      ToneBurst = 0,  // simple A
      DataBurst,      // simple B
      Off
    }

    private enum ProfUsbLnbPower : byte
    {
      Off = 0,
      On
    }

    /// <remarks>
    /// My understanding is that Prof tuners are or were OEM TBS tuners. This
    /// is a direct copy from the Turbosight extension.
    /// </remarks>
    private enum ProfUsbRemoteCodeBig : byte
    {
      Recall = 128,       // text [v1]: recall, text [v2]: back
      Up,
      Right,
      Record,
      Power,
      Three,
      Two,
      One,
      Down,
      Six,
      Five,
      Four,
      VolumeDown, // 140  // overlay [v2]: blue
      Nine,
      Eight,
      Seven,
      Left,
      ChannelDown,        // overlay [v2]: yellow
      Zero,
      VolumeUp,           // overlay [v2]: green
      Mute,
      Favourites,         // overlay [v1]: green
      ChannelUp,  // 150  // overlay [v2]: red
      Subtitles,
      Pause,
      Okay,
      Screenshot,
      Mode,
      Epg,
      Zoom,               // overlay [v1]: yellow
      Menu,               // overlay [v1]: red
      Exit, // 159        // overlay [v1]: blue

      Asterix = 209,
      Hash = 210,
      Clear = 212,

      SkipForward = 216,
      SkipBack,
      FastForward,
      Rewind,
      Stop,
      Tv,
      Play  // 222
    }

    /// <remarks>
    /// Image: [none]
    /// Testing: untested, based on old TBS SDK.
    /// </remarks>
    private enum ProfUsbRemoteCodeSmall : byte
    {
      Mute = 1,
      Left,
      Down,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,  // 10

      FullScreen = 12,
      Okay = 15,
      Exit = 18,
      Right = 26,
      Eight = 27,
      Up = 30,
      Nine = 31
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BdaExtensionParams
    {
      public int Frequency;                   // unit = MHz
      // Note that the driver does not automatically enable and disable the 22
      // kHz tone. Further, it is not clear how the driver interprets these
      // parameters. I recommend that the same frequency should be passed in
      // both parameters.
      public int LnbLowBandLof;               // unit = MHz
      public int LnbHighBandLof;              // unit = MHz
      public int SymbolRate;                  // unit = ks/s
      public ProfUsbPolarisation Polarisation;
      public ProfUsbLnbPower LnbPower;        // BdaExtensionProperty.LnbPower
      public ProfUsbTone22kState Tone22kState;// BdaExtensionProperty.Tone
      public ProfUsbToneBurst ToneBurst;      // BdaExtensionProperty.Tone
      public ProfUsbDiseqcPort DiseqcPort;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] DiseqcRawCommand;         // BdaExtensionProperty.Motor
      public byte IrCode;                     // BdaExtensionProperty.Ir
      public byte LockState;                  // BdaExtensionProperty.TunerLock
      public byte SignalStrength;             // BdaExtensionProperty.TunerLock
      public byte SignalQuality;              // BdaExtensionProperty.TunerLock
      public byte FecCodeRate;                // (BinaryConvolutionCodeRate)
      public byte Modulation;                 // (ModulationType)
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      private byte[] Reserved;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 0xaa, 0x87, 0xb5, 0xe1, 0xdc, 0x41, 0x13);

    private static readonly int BDA_EXTENSION_PARAMS_SIZE = Marshal.SizeOf(typeof(BdaExtensionParams));   // 288
    private const int MAX_DISEQC_MESSAGE_LENGTH = 5;
    private const int DEVICE_ID_LENGTH = 8;
    private const int MAC_ADDRESS_LENGTH = 6;

    private static readonly int GENERAL_BUFFER_SIZE = BDA_EXTENSION_PARAMS_SIZE;

    private const byte MIN_BIG_REMOTE_CODE = 0x80;
    private static readonly TimeSpan REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = new TimeSpan(0, 0, 0, 0, 100);

    #endregion

    #region variables

    private bool _isProfUsb = false;
    private IKsPropertySet _propertySet = null;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private bool _isCustomTuningSupported = false;

    private bool _isRemoteControlInterfaceOpen = false;
    private IntPtr _remoteControlBuffer = IntPtr.Zero;
    private Thread _remoteControlListenerThread = null;
    private ManualResetEvent _remoteControlListenerThreadStopEvent = null;

    #endregion

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      this.LogDebug("Prof USB: read device information");

      // Device ID.
      for (int i = 0; i < DEVICE_ID_LENGTH; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DeviceId,
        _generalBuffer, DEVICE_ID_LENGTH,
        _generalBuffer, DEVICE_ID_LENGTH,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != DEVICE_ID_LENGTH)
      {
        this.LogWarn("Prof USB: failed to read device ID, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        // I'm unsure of the meaning of the first, fifth, sixth, seventh and eighth bytes.
        this.LogDebug("  vendor ID   = 0x{0:x4}", Marshal.ReadInt16(_generalBuffer, 1));
        this.LogDebug("  device ID   = 0x{0:x4}", Marshal.ReadInt16(_generalBuffer, 3));
      }

      // MAC address.
      for (int i = 0; i < MAC_ADDRESS_LENGTH; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MacAddress,
        _generalBuffer, MAC_ADDRESS_LENGTH,
        _generalBuffer, MAC_ADDRESS_LENGTH,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != MAC_ADDRESS_LENGTH)
      {
        this.LogWarn("Prof USB: failed to read MAC address, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        byte[] address = new byte[MAC_ADDRESS_LENGTH];
        Marshal.Copy(_generalBuffer, address, 0, MAC_ADDRESS_LENGTH);
        this.LogDebug("  MAC address = {0}", BitConverter.ToString(address).ToLowerInvariant());
      }

      // Check whether custom tuning is supported.
      KSPropertySupport support;
      hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tuner, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("  tuning      = not supported, hr = 0x{0:x}, support = {1}", hr, support);
      }
      else
      {
        this.LogDebug("  tuning      = supported");
        _isCustomTuningSupported = true;
      }
    }

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartRemoteControlListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isRemoteControlInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_remoteControlListenerThread != null && !_remoteControlListenerThread.IsAlive)
      {
        StopRemoteControlListenerThread();
      }
      if (_remoteControlListenerThread == null)
      {
        this.LogDebug("Prof USB: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new ManualResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "Prof USB remote control listener";
        _remoteControlListenerThread.IsBackground = true;
        _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
        _remoteControlListenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopRemoteControlListenerThread()
    {
      if (_remoteControlListenerThread != null)
      {
        if (!_remoteControlListenerThread.IsAlive)
        {
          this.LogWarn("Prof USB: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join((int)REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME.TotalMilliseconds * 2))
          {
            this.LogWarn("Prof USB: failed to join remote control listener thread, aborting thread");
            _remoteControlListenerThread.Abort();
          }
        }
        _remoteControlListenerThread = null;
        if (_remoteControlListenerThreadStopEvent != null)
        {
          _remoteControlListenerThreadStopEvent.Close();
          _remoteControlListenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void RemoteControlListener()
    {
      this.LogDebug("Prof USB: remote control listener thread start polling");
      int hr = (int)NativeMethods.HResult.S_OK;
      int returnedByteCount;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir,
            _remoteControlBuffer, BDA_EXTENSION_PARAMS_SIZE,
            _remoteControlBuffer, BDA_EXTENSION_PARAMS_SIZE,
            out returnedByteCount
          );
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != BDA_EXTENSION_PARAMS_SIZE)
          {
            this.LogError("Prof USB: failed to read remote code, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          }
          else
          {
            BdaExtensionParams data = (BdaExtensionParams)Marshal.PtrToStructure(_remoteControlBuffer, typeof(BdaExtensionParams));
            if (data.IrCode != 0xff)
            {
              if (data.IrCode < MIN_BIG_REMOTE_CODE)
              {
                this.LogDebug("Prof USB: small remote control key press, code = {0}", (ProfUsbRemoteCodeSmall)data.IrCode);
              }
              else
              {
                this.LogDebug("Prof USB: big remote control key press, code = {0}", (ProfUsbRemoteCodeBig)data.IrCode);
              }
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Prof USB: remote control listener thread exception");
        return;
      }
      this.LogDebug("Prof USB: remote control listener thread stop polling");
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 65;
      }
    }

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Prof USB";
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
      this.LogDebug("Prof USB: initialising");

      if (_isProfUsb)
      {
        this.LogWarn("Prof USB: extension already initialised");
        return true;
      }

      // Some Afatech/ITETech DVB-T tuners expose the property set but don't
      // seem to work properly when it is used.
      // http://forum.team-mediaportal.com/threads/testbuilds-for-native-mp2-tv-updated-for-10th-ae-update-1-2014-09-13.114068/page-70#post-1132219
      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Prof USB: tuner type not supported");
        return false;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Prof USB: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Motor, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Prof USB: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      this.LogInfo("Prof USB: extension supported");
      _isProfUsb = true;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
      ReadDeviceInfo();
      return true;
    }

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("Prof USB: set power state, state = {0}", state);

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbPower, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Prof USB: property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      if (state == PowerState.On)
      {
        command.LnbPower = ProfUsbLnbPower.On;
      }
      else
      {
        command.LnbPower = ProfUsbLnbPower.Off;
      }

      Marshal.StructureToPtr(command, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.LnbPower,
        IntPtr.Zero, 0,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to set power state, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region ICustomTuner members

    /// <summary>
    /// Check if the extension implements specialised tuning for a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the extension supports specialised tuning for the channel, otherwise <c>false</c></returns>
    public bool CanTuneChannel(IChannel channel)
    {
      return channel is IChannelSatellite && _isCustomTuningSupported;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("Prof USB: tune to channel");

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null || !_isCustomTuningSupported)
      {
        this.LogError("Prof USB: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
        return false;
      }

      BdaExtensionParams tuningParams = new BdaExtensionParams();
      tuningParams.Frequency = satelliteChannel.Frequency / 1000;

      // See the notes for the struct to understand why we set the same value
      // for both LOFs.
      tuningParams.LnbLowBandLof = SatelliteLnbHandler.GetLocalOscillatorFrequency(satelliteChannel.Frequency) / 1000;  // kHz -> MHz
      tuningParams.LnbHighBandLof = tuningParams.LnbLowBandLof;
      tuningParams.SymbolRate = satelliteChannel.SymbolRate;
      tuningParams.LnbPower = ProfUsbLnbPower.On;

      if (SatelliteLnbHandler.IsHighVoltage(satelliteChannel.Polarisation))
      {
        tuningParams.Polarisation = ProfUsbPolarisation.Horizontal;
      }
      else
      {
        tuningParams.Polarisation = ProfUsbPolarisation.Vertical;
      }

      if (SatelliteLnbHandler.Is22kToneOn(satelliteChannel.Frequency))
      {
        tuningParams.Tone22kState = ProfUsbTone22kState.On;
      }
      else
      {
        tuningParams.Tone22kState = ProfUsbTone22kState.Off;
      }

      // DiSEqC commands are already sent using the raw command interface. No
      // need to resend them and unnecessarily slow down the tune request.
      tuningParams.ToneBurst = ProfUsbToneBurst.Off;
      tuningParams.DiseqcPort = ProfUsbDiseqcPort.Null;

      BinaryConvolutionCodeRate bdaFecCodeRate = BinaryConvolutionCodeRate.RateNotSet;
      switch (satelliteChannel.FecCodeRate)
      {
        case FecCodeRate.Rate1_2:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate1_2;
          break;
        case FecCodeRate.Rate1_3:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate1_3;
          break;
        case FecCodeRate.Rate1_4:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate1_4;
          break;
        case FecCodeRate.Rate2_3:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate2_3;
          break;
        case FecCodeRate.Rate2_5:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate2_5;
          break;
        case FecCodeRate.Rate3_4:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate3_4;
          break;
        case FecCodeRate.Rate3_5:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate3_5;
          break;
        case FecCodeRate.Rate4_5:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate4_5;
          break;
        case FecCodeRate.Rate5_11:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate5_11;
          break;
        case FecCodeRate.Rate5_6:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate5_6;
          break;
        case FecCodeRate.Rate6_7:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate6_7;
          break;
        case FecCodeRate.Rate7_8:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate7_8;
          break;
        case FecCodeRate.Rate8_9:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate8_9;
          break;
        case FecCodeRate.Rate9_10:
          bdaFecCodeRate = BinaryConvolutionCodeRate.Rate9_10;
          break;
        default:
          this.LogWarn("Prof USB: tune request uses unsupported FEC code rate {0}, falling back to automatic", satelliteChannel.FecCodeRate);
          bdaFecCodeRate = BinaryConvolutionCodeRate.RateNotSet;
          break;
      }
      tuningParams.FecCodeRate = (byte)bdaFecCodeRate;

      ModulationType bdaModulation = (ModulationType)0xff;  // not set (-1), limited to 1 byte
      if (channel is ChannelDvbS2)
      {
        switch (satelliteChannel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk4:
            bdaModulation = ModulationType.ModNbcQpsk;
            break;
          case ModulationSchemePsk.Psk8:
            bdaModulation = ModulationType.ModNbc8Psk;
            break;
          case ModulationSchemePsk.Psk16:
            bdaModulation = ModulationType.Mod16Apsk;
            break;
          case ModulationSchemePsk.Psk32:
            bdaModulation = ModulationType.Mod32Apsk;
            break;
          default:
            this.LogWarn("Prof USB: DVB-S2 tune request uses unsupported modulation scheme {0}, falling back to automatic", satelliteChannel.ModulationScheme);
            break;
        }
      }
      else if (channel is ChannelDvbS)
      {
        switch (satelliteChannel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk2:
            bdaModulation = ModulationType.ModBpsk;
            break;
          case ModulationSchemePsk.Psk4:
            bdaModulation = ModulationType.ModQpsk;
            break;
          default:
            this.LogWarn("Prof USB: DVB-S tune request uses unsupported modulation scheme {0}, falling back to automatic", satelliteChannel.ModulationScheme);
            break;
        }
      }
      else
      {
        this.LogWarn("Prof USB: tune request for unsupported satellite standard, using automatic modulation");
      }
      tuningParams.Modulation = (byte)bdaModulation;

      Marshal.StructureToPtr(tuningParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tuner,
        IntPtr.Zero, 0,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to tune, hr = 0x{0:x}{1}{2}", hr, Environment.NewLine, channel);
      return false;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      this.LogDebug("Prof USB: send DiSEqC command");

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Prof USB: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Prof USB: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.DiseqcRawCommand = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, propertyParams.DiseqcRawCommand, 0, command.Length);

      Marshal.StructureToPtr(propertyParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Motor,
        IntPtr.Zero, 0,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Prof USB: send tone burst command, command = {0}", command);

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      if (command == ToneBurst.ToneBurst)
      {
        propertyParams.ToneBurst = ProfUsbToneBurst.ToneBurst;
      }
      else if (command == ToneBurst.DataBurst)
      {
        propertyParams.ToneBurst = ProfUsbToneBurst.DataBurst;
      }
      propertyParams.Tone22kState = ProfUsbTone22kState.Off;

      Marshal.StructureToPtr(propertyParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tone,
        IntPtr.Zero, 0,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("Prof USB: set tone state, state = {0}", state);

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }

      BdaExtensionParams propertyParams = new BdaExtensionParams();
      propertyParams.ToneBurst = ProfUsbToneBurst.Off;
      if (state == Tone22kState.On)
      {
        propertyParams.Tone22kState = ProfUsbTone22kState.On;
      }
      else
      {
        propertyParams.Tone22kState = ProfUsbTone22kState.Off;
      }

      Marshal.StructureToPtr(propertyParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tone,
        IntPtr.Zero, 0,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to set tone state, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      // Not implemented.
      response = null;
      return false;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("Prof USB: open remote control interface");

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Prof USB: remote control interface is already open");
        return true;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Ir, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Prof USB: property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      _remoteControlBuffer = Marshal.AllocCoTaskMem(BDA_EXTENSION_PARAMS_SIZE);
      _isRemoteControlInterfaceOpen = true;
      StartRemoteControlListenerThread();

      this.LogDebug("Prof USB: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Close()
    {
      return CloseRemoteControlListenerInterface(true);
    }

    private bool CloseRemoteControlListenerInterface(bool isDisposing)
    {
      this.LogDebug("Prof USB: close remote control interface");

      if (isDisposing)
      {
        StopRemoteControlListenerThread();
      }

      if (_remoteControlBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_remoteControlBuffer);
        _remoteControlBuffer = IntPtr.Zero;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Prof USB: result = success");
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

    ~ProfUsb()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isProfUsb)
      {
        CloseRemoteControlListenerInterface(isDisposing);
      }
      if (isDisposing)
      {
        _propertySet = null;
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isProfUsb = false;
    }

    #endregion
  }
}