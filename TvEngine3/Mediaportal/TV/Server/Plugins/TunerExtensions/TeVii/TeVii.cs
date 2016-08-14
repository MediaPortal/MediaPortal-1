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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.TeVii
{
  /// <summary>
  /// A class for handling DiSEqC, remote controls and tuning for TeVii tuners.
  /// This class is implemented using the official TeVii SDK/API.
  /// </summary>
  public class TeVii : BaseTunerExtension, ICustomTuner, IDiseqcDevice, IDisposable, IRemoteControlListener
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
      TurboQpsk,
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

    /// <remarks>
    /// Image: http://img607.imageshack.us/img607/951/s1057417.jpg
    /// Testing: S480
    /// Comments are labels above the buttons.
    /// There seem to be many variants. The image linked above is the one I tested with.
    /// </remarks>
    private enum TeViiRemoteCode
    {
      Up = 0,
      Down,
      Right,
      Left,
      Record,
      LiveMode,
      ChannelDown,
      PlayMode,
      ChannelUp,
      VolumeUp,
      Power,
      SkipBack,           // timer
      Mute, // 12

      SkipForward = 14,   // open
      VolumeDown,
      Zero,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine,
      Recall,             // event info
      Favourites,         // current/next
      Menu,
      Back,
      Rewind,
      Okay,   // 31

      PlayPause = 64,
      SwitchAb,           // [text: A/B]

      Audio = 67,
      Epg,
      Subtitles,
      Tv,                 // satellite
      Music,  // 71       // provider

      List = 74,
      Info = 76,          // more
      FastForward = 77,

      Enter = 82,         // all
      Monitor = 86,
      FullScreen = 88,
      Home = 90,
      Pictures = 92,      // favourites
      Radio = 94,         // transponder
    }

    #endregion

    #region DLL imports

    #region information functions
    // These functions don't require an open device.

    /// <summary>
    /// Get the SDK API version number.
    /// </summary>
    /// <returns>the API Version number</returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int GetAPIVersion();

    /// <summary>
    /// Enumerate the TeVii-compatible devices in the system.
    /// </summary>
    /// <remarks>
    /// This function should be called before any other functions are called. Only the first call will
    /// really enumerate. Subsequent calls will just return the result from the first call.
    /// </remarks>
    /// <returns>the number of TeVii-compatible devices connected to the system</returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FindDevices();

    /// <summary>
    /// Get the friendly name for a specific TeVii device.
    /// </summary>
    /// <remarks>
    /// The returned pointer points to an ANSI NULL terminated string (UnmanagedType.LPStr). We don't use
    /// automatic marshaling because it would conflict with the stipulation that we must not modify or
    /// free the memory associated with the pointer.
    /// </remarks>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <returns>a pointer to a NULL terminated buffer containing the device name, otherwise <c>IntPtr.Zero</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetDeviceName(int index);

    /// <summary>
    /// Get the device path for a specific TeVii device.
    /// </summary>
    /// <remarks>
    /// The returned pointer points to an ANSI NULL terminated string (UnmanagedType.LPStr). We don't use
    /// automatic marshaling because it would conflict with the stipulation that we must not modify or
    /// free the memory associated with the pointer.
    /// </remarks>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <returns>a pointer to a NULL terminated buffer containing the device path, otherwise <c>IntPtr.Zero</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetDevicePath(int index);

    #endregion

    #region device functions
    // The following functions work only after calling OpenDevice().

    /// <summary>
    /// Open access to a specific TeVii device.
    /// </summary>
    /// <remarks>
    /// The index parameter specifies which device will be opened. It is possible to have access to multiple
    /// devices simultaneously.
    /// </remarks>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <param name="captureCallBack">An optional delegate that will be invoked when raw packet data is received.</param>
    /// <param name="context">An optional pointer that will be passed as a paramter to the capture call-back.</param>
    /// <returns><c>true</c> if the device access is successfully established, otherwise <c>false</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenDevice(int index, OnTeViiCaptureData captureCallBack, IntPtr context);

    /// <summary>
    /// Close access to a specific TeVii device.
    /// </summary>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <returns><c>true</c> if the device access is successfully closed, otherwise <c>false</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseDevice(int index);

    /// <summary>
    /// Tune a TeVii DVB-S/S2 tuner to a specific satellite transponder.
    /// </summary>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <param name="frequency">The transponder frequency. The unit is kilo-Hertz (kHz).</param>
    /// <param name="symbolRate">The transponder symbol rate. The unit is symbols-per-second (s/s).</param>
    /// <param name="lnbLof">The LNB local oscillator frequency offset. The unit is kilo-Hertz (kHz).</param>
    /// <param name="polarisation">The transponder polarisation.</param>
    /// <param name="toneOn"><c>True</c> to turn the 22 kHz oscillator on (to force the LNB to high band mode or switch to port 2).</param>
    /// <param name="modulation">The transponder modulation. Note that it's better to avoid using <c>TeViiModulation.Auto</c> for DVB-S2 transponders to minimise lock time.</param>
    /// <param name="fecRate">The transponder FEC rate. Note that avoiding <c>TeViiFecRate.Auto</c> for DVB-S2 transponders minimises lock time.</param>
    /// <returns><c>true</c> if the tuner successfully locks on the transponder, otherwise <c>false</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TuneTransponder(int index, int frequency, int symbolRate, int lnbLof, TeViiPolarisation polarisation,
                                               [MarshalAs(UnmanagedType.Bool)] bool toneOn, TeViiModulation modulation, TeViiFecRate fecRate);

    /// <summary>
    /// Get the current signal status for a specific TeVii tuner device.
    /// </summary>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <param name="isLocked"><c>True</c> if the tuner/demodulator are locked onto a transponder.</param>
    /// <param name="strength">A signal strength rating ranging between 0 (low strength) and 100 (high strength).</param>
    /// <param name="quality">A signal quality rating ranging between 0 (low quality) and 100 (high quality).</param>
    /// <returns><c>true</c> if the signal status is successfully retrieved, otherwise <c>false</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSignalStatus(int index, [MarshalAs(UnmanagedType.Bool)] out bool isLocked, out int strength, out int quality);

    /// <summary>
    /// Send an arbitrary DiSEqC message.
    /// </summary>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <param name="message">The DiSEqC message bytes.</param>
    /// <param name="length">The message length in bytes.</param>
    /// <param name="repeatCount">The number of times to resend the message. Zero means send the message once.</param>
    /// <param name="repeatFlag"><c>True</c> to set the first byte in the message to 0xe1 if/when the message is resent.</param>
    /// <returns><c>true</c> if the message is successfully sent, otherwise <c>false</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SendDiSEqC(int index, [MarshalAs(UnmanagedType.LPArray)] byte[] message, int length, int repeatCount, [MarshalAs(UnmanagedType.Bool)] bool repeatFlag);

    /// <summary>
    /// Set the remote control receiver call back function.
    /// </summary>
    /// <param name="index">The zero-based device index (0 &lt;= index &lt; FindDevices()).</param>
    /// <param name="remoteKeyPressCallBack">A delegate that will be invoked when remote key-press events are detected.</param>
    /// <param name="context">An optional pointer that will be passed as a paramter to the remote key-press call-back.</param>
    /// <returns><c>true</c> if the call back function is successfully set, otherwise <c>false</c></returns>
    [DllImport("Resources\\TeVii.dll", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetRemoteControl(int index, OnTeViiRemoteControlKeyPress remoteKeyPressCallBack, IntPtr context);

    #endregion

    #endregion

    #region delegate definitions

    /// <summary>
    /// Invoked by the tuner driver when raw packet data is received.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the call back was registered.</param>
    /// <param name="data">The raw data.</param>
    /// <param name="dataLength">The number of bytes of available data.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)]
    private delegate void OnTeViiCaptureData(IntPtr context, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data, int dataLength);

    /// <summary>
    /// Invoked by the tuner driver when a remote control key press is detected.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the call back was registered.</param>
    /// <param name="code">The key code.</param>
    [UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)]
    private delegate void OnTeViiRemoteControlKeyPress(IntPtr context, TeViiRemoteCode code);

    #endregion

    #region constants

    private const string TEVIIRC_PROCESS_NAME = "TeViiRC";

    #endregion

    #region variables

    private int _deviceIndex = -1;
    private bool _isTeVii = false;
    private bool _isCustomTuningSupported = false;
    private Tone22kState _toneState = Tone22kState.Automatic;
    private string _tunerExternalId = string.Empty;
    private bool _restartTeViiRcExe = false;

    private bool _isRemoteControlInterfaceOpen = false;
    private OnTeViiRemoteControlKeyPress _remoteControlKeyPressDelegate = null;

    #endregion

    #region callback handlers

    /// <summary>
    /// Called by the tuner driver when a remote control key press is detected.
    /// </summary>
    /// <param name="context">The optional context passed to the interface when the call back was registered.</param>
    /// <param name="code">The key code.</param>
    private void OnRemoteControlKeyPress(IntPtr context, TeViiRemoteCode code)
    {
      this.LogDebug("TeVii: remote control key press, code = {0}", code);
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
        return 75;
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
      this.LogDebug("TeVii: initialising");

      if (_isTeVii)
      {
        this.LogWarn("TeVii: extension already initialised");
        return true;
      }

      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("TeVii: tuner external identifier is not set");
        return false;
      }

      int deviceCount = FindDevices();
      this.LogDebug("TeVii: number of devices = {0}, tuner external identifier = {1}", deviceCount, tunerExternalId);
      if (deviceCount == 0)
      {
        this.LogDebug("TeVii: TeVii devices not present");
        return false;
      }

      string deviceName = string.Empty;
      string devicePath = string.Empty;
      for (int deviceIndex = 0; deviceIndex < deviceCount; deviceIndex++)
      {
        deviceName = Marshal.PtrToStringAnsi(GetDeviceName(deviceIndex));
        devicePath = Marshal.PtrToStringAnsi(GetDevicePath(deviceIndex));

        //this.LogDebug("TeVii: compare to {0} {1}", deviceName, devicePath);
        if (tunerExternalId.Contains(devicePath))
        {
          this.LogDebug("TeVii: device recognised, index = {0}, name = {1}, API version = {2}", deviceIndex, deviceName, GetAPIVersion());
          _deviceIndex = deviceIndex;
          break;
        }
      }

      if (_deviceIndex == -1)
      {
        this.LogDebug("TeVii: device not recognised as a TeVii device");
        return false;
      }

      if (!OpenDevice(_deviceIndex, null, IntPtr.Zero))
      {
        this.LogError("TeVii: failed to open device");
        return false;
      }
      this.LogInfo("TeVii: extension supported");
      _isTeVii = true;
      _isCustomTuningSupported = (tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) != 0;
      _tunerExternalId = tunerExternalId.ToLowerInvariant();
      return true;
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
      // Tuning of satellite channels is supported with an appropriate tuner.
      return channel is IChannelSatellite && _isCustomTuningSupported;
    }

    /// <summary>
    /// Tune to a given channel using the specialised tuning method.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns><c>true</c> if the channel is successfully tuned, otherwise <c>false</c></returns>
    public bool Tune(IChannel channel)
    {
      this.LogDebug("TeVii: tune to channel");

      if (!_isTeVii)
      {
        this.LogWarn("TeVii: not initialised or interface not supported");
        return false;
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null || !_isCustomTuningSupported)
      {
        this.LogError("TeVii: tuning is not supported for channel{0}{1}", Environment.NewLine, channel);
        return false;
      }

      TeViiPolarisation polarisation = TeViiPolarisation.Vertical;
      if (SatelliteLnbHandler.IsHighVoltage(satelliteChannel.Polarisation))
      {
        polarisation = TeViiPolarisation.Horizontal;
      }

      // Use the state set in SetToneState().
      bool isToneOn = _toneState == Tone22kState.On;

      TeViiModulation modulation = TeViiModulation.Auto;
      if (channel is ChannelDvbS)
      {
        if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk2)
        {
          modulation = TeViiModulation.Bpsk;
        }
        else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
        {
          modulation = TeViiModulation.Qpsk;
        }
      }
      else if (channel is ChannelDvbS2)
      {
        switch (satelliteChannel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk4:
            modulation = TeViiModulation.Dvbs2_Qpsk;
            break;
          case ModulationSchemePsk.Psk8:
            modulation = TeViiModulation.Dvbs2_8Psk;
            break;
          case ModulationSchemePsk.Psk16:
            modulation = TeViiModulation.Dvbs2_16Apsk;
            break;
          case ModulationSchemePsk.Psk32:
            modulation = TeViiModulation.Dvbs2_32Apsk;
            break;
        }
      }
      else if (channel is ChannelSatelliteTurboFec)
      {
        switch (satelliteChannel.ModulationScheme)
        {
          case ModulationSchemePsk.Psk4:
            modulation = TeViiModulation.TurboQpsk;
            break;
          case ModulationSchemePsk.Psk8:
            modulation = TeViiModulation.Turbo8Psk;
            break;
          case ModulationSchemePsk.Psk16:
            modulation = TeViiModulation.Turbo16Psk;
            break;
        }
      }
      if (modulation == TeViiModulation.Auto)
      {
        this.LogWarn("TeVii: tune request uses unsupported modulation scheme {0}, falling back to automatic", satelliteChannel.ModulationScheme);
      }

      TeViiFecRate fecCodeRate = TeViiFecRate.Auto;
      switch (satelliteChannel.FecCodeRate)
      {
        case FecCodeRate.Rate1_2:
          fecCodeRate = TeViiFecRate.Rate1_2;
          break;
        case FecCodeRate.Rate1_3:
          fecCodeRate = TeViiFecRate.Rate1_3;
          break;
        case FecCodeRate.Rate1_4:
          fecCodeRate = TeViiFecRate.Rate1_4;
          break;
        case FecCodeRate.Rate2_3:
          fecCodeRate = TeViiFecRate.Rate2_3;
          break;
        case FecCodeRate.Rate2_5:
          fecCodeRate = TeViiFecRate.Rate2_5;
          break;
        case FecCodeRate.Rate3_4:
          fecCodeRate = TeViiFecRate.Rate3_4;
          break;
        case FecCodeRate.Rate3_5:
          fecCodeRate = TeViiFecRate.Rate3_5;
          break;
        case FecCodeRate.Rate4_5:
          fecCodeRate = TeViiFecRate.Rate4_5;
          break;
        case FecCodeRate.Rate5_11:
          fecCodeRate = TeViiFecRate.Rate5_11;
          break;
        case FecCodeRate.Rate5_6:
          fecCodeRate = TeViiFecRate.Rate5_6;
          break;
        case FecCodeRate.Rate6_7:
          fecCodeRate = TeViiFecRate.Rate6_7;
          break;
        case FecCodeRate.Rate7_8:
          fecCodeRate = TeViiFecRate.Rate7_8;
          break;
        case FecCodeRate.Rate8_9:
          fecCodeRate = TeViiFecRate.Rate8_9;
          break;
        case FecCodeRate.Rate9_10:
          fecCodeRate = TeViiFecRate.Rate9_10;
          break;
        default:
          this.LogWarn("TeVii: tune request uses unsupported FEC code rate {0}, falling back to automatic", satelliteChannel.FecCodeRate);
          break;
      }

      int lnbLof = SatelliteLnbHandler.GetLocalOscillatorFrequency(satelliteChannel.Frequency);
      bool result = TuneTransponder(_deviceIndex, satelliteChannel.Frequency, satelliteChannel.SymbolRate * 1000, lnbLof, polarisation, isToneOn, modulation, fecCodeRate);
      if (result)
      {
        this.LogDebug("TeVii: result = success");
      }
      else
      {
        this.LogError("TeVii: failed to tune{0}{1}", Environment.NewLine, channel);
      }

      // Reset the tone state to auto. SetToneState() must be called again to override the default logic.
      _toneState = Tone22kState.Automatic;
      return result;
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
      this.LogDebug("TeVii: send DiSEqC command");

      if (!_isTeVii)
      {
        this.LogWarn("TeVii: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("TeVii: DiSEqC command not supplied");
        return true;
      }

      bool result = SendDiSEqC(_deviceIndex, command, command.Length, 0, false);
      if (result)
      {
        this.LogDebug("TeVii: result = success");
        return true;
      }

      this.LogError("TeVii: failed to send DiSEqC command");
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      // Not supported.
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("TeVii: set tone state, state = {0}", state);

      if (!_isTeVii)
      {
        this.LogWarn("TeVii: not initialised or interface not supported");
        return false;
      }

      // Cache for next tune request.
      _toneState = state;
      this.LogDebug("TeVii: result = success");
      return true;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      // Not supported.
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
      this.LogDebug("TeVii: open remote control interface");

      if (!_isTeVii)
      {
        this.LogWarn("TeVii: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("TeVii: remote control interface is already open");
        return true;
      }

      if (_tunerExternalId.Contains("vid_9022&pid_d482") || _tunerExternalId.Contains("vid_9022&pid_d484"))
      {
        this.LogDebug("TeVii: detected S480/482 tuner 2, remote control support disabled to avoid double key presses");
        return false;
      }

      _remoteControlKeyPressDelegate = OnRemoteControlKeyPress;
      if (!SetRemoteControl(_deviceIndex, _remoteControlKeyPressDelegate, IntPtr.Zero))
      {
        this.LogError("TeVii: failed to set remote control event/delegate");
        return false;
      }

      _restartTeViiRcExe = false;
      Process[] processes = Process.GetProcessesByName(TEVIIRC_PROCESS_NAME);
      if (processes.Length > 0)
      {
        // TeViiRC is running. We stop it to avoid unnecessary HID keypresses.
        // We'll restart it when we're done.
        this.LogWarn("TeVii: attempt terminate {0} TeVii RC process(es)", processes.Length);
        foreach (Process proc in processes)
        {
          proc.Kill();
        }
        Thread.Sleep(400);
        processes = Process.GetProcessesByName(TEVIIRC_PROCESS_NAME);
        if (processes.Length != 0)
        {
          this.LogWarn("TeVii: failed to terminate TeVii RC, still {0} process(es)", processes.Length);
        }
        else
        {
          _restartTeViiRcExe = true;
        }
      }

      _isRemoteControlInterfaceOpen = true;
      this.LogDebug("TeVii: result = success");
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
      this.LogDebug("TeVii: close remote control interface");

      bool success = SetRemoteControl(_deviceIndex, null, IntPtr.Zero);
      if (!success)
      {
        this.LogError("TeVii: failed to unset remote control event/delegate");
        return false;
      }
      if (isDisposing)
      {
        _remoteControlKeyPressDelegate = null;
      }

      if (_restartTeViiRcExe)
      {
        this.LogDebug("TeVii: restart TeVii RC process");
        try
        {
          int processCount = Process.GetProcessesByName(TEVIIRC_PROCESS_NAME).Length;
          if (processCount == 0)
          {
            Process.Start(@"c:\WINDOWS\TeViiRC.exe");
          }
          else
          {
            this.LogWarn("TeVii: {0} TeVii RC process(es) already running", processCount);
          }
          _restartTeViiRcExe = false;
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "TeVii: failed to restart TeViiRC process");
        }
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("TeVii: result = success");
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

    ~TeVii()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isTeVii && _deviceIndex >= 0)
      {
        CloseRemoteControlListenerInterface(isDisposing);
        CloseDevice(_deviceIndex);
      }
      _deviceIndex = -1;
      _isTeVii = false;
    }

    #endregion
  }
}