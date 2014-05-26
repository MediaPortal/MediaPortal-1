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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.ProfUsb
{
  /// <summary>
  /// This API was originally used by Turbosight for their QBOX series tuners. Turbosight moved to
  /// a more unified API for their tuners in 2011. This class stays to support the OEM clones from
  /// Prof, Satrade and Omicom which will not receive updated drivers.
  /// </summary>
  public class ProfUsb : BaseCustomDevice, IPowerDevice, IDiseqcDevice, ICustomTuner, IRemoteControlListener
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

    private enum ProfUsb22k : byte
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

    // My understanding is that Prof tuners are or were OEM TBS tuners. The
    // codes for this remote should work:
    // http://www.tbsdtv.com/products/images/tbs6981/tbs6981_4.jpg
    private enum ProfUsbBigRemoteCode : byte
    {
      Recall = 0x80,
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
      VolumeDown,
      Nine,
      Eight,
      Seven,
      Left, // 0x90
      ChannelDown,
      Zero,
      VolumeUp,
      Mute,
      Favourites,  // green
      ChannelUp,
      Subtitles,
      Pause,
      Okay,
      Snapshot,
      Mode,
      Epg,
      Zoom,        // yellow
      Menu,        // red
      Exit         // blue
    }

    // Unverified.
    private enum ProfUsbSmallRemoteCode : byte
    {
      Mute = 0x01,
      Left,
      Down,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,  // 0x0a

      FullScreen = 0x0c,
      Okay = 0x0e,
      Exit = 0x12,
      Right = 0x1a,
      Eight = 0x1b,
      Up = 0x1e,
      Nine = 0x1f
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BdaExtensionParams
    {
      public uint Frequency;                  // unit = MHz
      // Note that the driver does not automatically enable and disable the 22 kHz tone. Further, it is not
      // clear how the driver interprets these parameters. I recommend that the same frequency should be
      // passed in both parameters.
      public uint LnbLowBandLof;              // unit = MHz
      public uint LnbHighBandLof;             // unit = MHz
      public uint SymbolRate;                 // unit = ks/s
      public ProfUsbPolarisation Polarisation;
      public ProfUsbLnbPower LnbPower;        // BdaExtensionProperty.LnbPower
      public ProfUsb22k Tone22k;              // BdaExtensionProperty.Tone
      public ProfUsbToneBurst ToneBurst;      // BdaExtensionProperty.Tone
      public ProfUsbDiseqcPort DiseqcPort;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] DiseqcRawCommand;         // BdaExtensionProperty.Motor
      public byte IrCode;                     // BdaExtensionProperty.Ir
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

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 0xaa, 0x87, 0xb5, 0xe1, 0xdc, 0x41, 0x13);

    private static readonly int BDA_EXTENSION_PARAMS_SIZE = Marshal.SizeOf(typeof(BdaExtensionParams));   // 288
    private const int MAX_DISEQC_MESSAGE_LENGTH = 5;
    private const int DEVICE_ID_LENGTH = 8;
    private const int MAC_ADDRESS_LENGTH = 6;

    private static readonly int GENERAL_BUFFER_SIZE = BDA_EXTENSION_PARAMS_SIZE;

    private const byte MIN_BIG_REMOTE_CODE = 0x80;
    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;     // unit = ms

    #endregion

    #region variables

    private bool _isProfUsb = false;
    private IKsPropertySet _propertySet = null;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private bool _isCustomTuningSupported = false;

    private bool _isRemoteControlInterfaceOpen = false;
    private IntPtr _remoteControlBuffer = IntPtr.Zero;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

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
      if (hr != (int)HResult.Severity.Success || returnedByteCount != DEVICE_ID_LENGTH)
      {
        this.LogWarn("Prof USB: failed to read device ID, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
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
      if (hr != (int)HResult.Severity.Success || returnedByteCount != MAC_ADDRESS_LENGTH)
      {
        this.LogWarn("Prof USB: failed to read MAC address, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
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
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("  tuning      = not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
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
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
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
      int hr = (int)HResult.Severity.Success;
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
          if (hr != (int)HResult.Severity.Success || returnedByteCount != BDA_EXTENSION_PARAMS_SIZE)
          {
            this.LogError("Prof USB: failed to read remote code, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
          }
          else
          {
            BdaExtensionParams data = (BdaExtensionParams)Marshal.PtrToStructure(_remoteControlBuffer, typeof(BdaExtensionParams));
            if (data.IrCode != 0xff)
            {
              if (data.IrCode < MIN_BIG_REMOTE_CODE)
              {
                this.LogDebug("Prof USB: small remote control key press = {0}", (ProfUsbSmallRemoteCode)data.IrCode);
              }
              else
              {
                this.LogDebug("Prof USB: big remote control key press = {0}", (ProfUsbBigRemoteCode)data.IrCode);
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

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 65;
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
        return "Prof USB";
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
      this.LogDebug("Prof USB: initialising");

      if (_isProfUsb)
      {
        this.LogWarn("Prof USB: extension already initialised");
        return true;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Prof USB: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Motor, out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Prof USB: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogInfo("Prof USB: extension supported");
      _isProfUsb = true;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
      ReadDeviceInfo();
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
      this.LogDebug("Prof USB: on before tune call back");
      action = TunerAction.Default;

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return;
      }

      // We only need to tweak the parameters for DVB-S/S2 channels.
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
      this.LogDebug("  LNB LOF    = {0}", ch.LnbType.LowBandFrequency);
    }

    #endregion

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
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Prof USB: property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to set power state, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      this.LogDebug("Prof USB: tune to channel");

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }

      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel == null || !_isCustomTuningSupported)
      {
        this.LogError("Prof USB: tuning is not supported for channel");
        return false;
      }

      ProfUsbPolarisation profPolarisation = ProfUsbPolarisation.Horizontal;
      if (dvbsChannel.Polarisation == Polarisation.LinearV || dvbsChannel.Polarisation == Polarisation.CircularR)
      {
        profPolarisation = ProfUsbPolarisation.Vertical;
      }

      ProfUsb22k tone22k = ProfUsb22k.Off;
      if (dvbsChannel.Frequency > dvbsChannel.LnbType.SwitchFrequency)
      {
        tone22k = ProfUsb22k.On;
      }

      ProfUsbToneBurst toneBurst = ProfUsbToneBurst.Off;
      if (dvbsChannel.Diseqc == DiseqcPort.SimpleA)
      {
        toneBurst = ProfUsbToneBurst.ToneBurst;
      }
      else if (dvbsChannel.Diseqc == DiseqcPort.SimpleB)
      {
        toneBurst = ProfUsbToneBurst.DataBurst;
      }

      BdaExtensionParams tuningParams = new BdaExtensionParams();
      tuningParams.Frequency = (uint)dvbsChannel.Frequency / 1000;
      // See the notes for the struct to understand why we do this. Note that OnBeforeTune() ensures that the low LOF
      // is set appropriately.
      uint lnbLof = (uint)dvbsChannel.LnbType.LowBandFrequency / 1000;
      tuningParams.LnbLowBandLof = lnbLof;
      tuningParams.LnbHighBandLof = lnbLof;
      tuningParams.SymbolRate = (uint)dvbsChannel.SymbolRate;
      tuningParams.Polarisation = profPolarisation;
      tuningParams.LnbPower = ProfUsbLnbPower.On;
      tuningParams.Tone22k = tone22k;
      tuningParams.ToneBurst = toneBurst;
      // DiSEqC commands are already sent using the raw command interface. No need to resend them and
      // unnecessarily slow down the tune request.
      tuningParams.DiseqcPort = ProfUsbDiseqcPort.Null;
      tuningParams.InnerFecRate = (byte)dvbsChannel.InnerFecRate;
      tuningParams.Modulation = (byte)dvbsChannel.ModulationType;

      Marshal.StructureToPtr(tuningParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tuner,
        IntPtr.Zero, 0,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to tune, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Prof USB: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isProfUsb)
      {
        this.LogWarn("Prof USB: not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tone,
                                  out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Prof USB: property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      BdaExtensionParams command = new BdaExtensionParams();
      command.ToneBurst = ProfUsbToneBurst.Off;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        command.ToneBurst = ProfUsbToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        command.ToneBurst = ProfUsbToneBurst.DataBurst;
      }
      command.Tone22k = ProfUsb22k.Off;
      if (tone22kState == Tone22k.On)
      {
        command.Tone22k = ProfUsb22k.On;
      }

      Marshal.StructureToPtr(command, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, BDA_EXTENSION_PARAMS_SIZE);

      hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tone,
        IntPtr.Zero, 0,
        _generalBuffer, BDA_EXTENSION_PARAMS_SIZE
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to set tone state, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
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
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Prof USB: result = success");
        return true;
      }

      this.LogError("Prof USB: failed to send DiSEqC command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
    public bool OpenRemoteControlInterface()
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
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Prof USB: property not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
    public bool CloseRemoteControlInterface()
    {
      this.LogDebug("Prof USB: close remote control interface");

      StopRemoteControlListenerThread();
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
    public override void Dispose()
    {
      if (_isProfUsb)
      {
        CloseRemoteControlInterface();
      }
      _propertySet = null;
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