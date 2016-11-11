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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Omicom
{
  /// <summary>
  /// A class for handling DiSEqC and stream selection for Omicom tuners.
  /// </summary>
  public class Omicom : BaseTunerExtension, IDiseqcDevice, IDisposable, IStreamSelector
  {
    #region enums

    private enum BdaExtensionPropertyCustom
    {
      StreamInfo = 5,
      InputStreamFilter
    }

    private enum BdaExtensionPropertyDiseqc
    {
      DiseqcWrite = 0,
      DiseqcRead,
      Tone22kState,
      CableLossCompensation,
      ToneBurst
    }

    private enum OmicomToneBurst : int
    {
      ToneBurst = 0,  // unmodulated, simple A
      DataBurst       // modulated, simple B
    }

    private enum OmicomTone22kState : int
    {
      Off = 0,
      On
    }

    private enum OmicomStreamType : int
    {
      Transport = 0,
      GenericContinuous,
      GenericPacketised,
      Reserved
    }

    private enum OmicomCodingType : int
    {
      Constant = 0,
      Adaptive = 1,
      Variable = 1
    }

    private enum OmicomFrameLength : int
    {
      Long = 0,
      Short
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessage
    {
      public int MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] Message;
      public int RepeatCount;         // Set to zero to send the message once, one => twice, two => three times... etc.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct StreamInfo
    {
      public OmicomStreamType StreamType;
      public OmicomCodingType CodingType;
      public OmicomFrameLength FrameLength;
      public int InputStreamCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_INPUT_STREAM_ID_COUNT)]
      public byte[] InputStreamIds;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsInputStreamSynchActive;
      [MarshalAs(UnmanagedType.Bool)]
      public bool DeleteNullPackets;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET_CUSTOM = new Guid(0x7db2dee6, 0x42b4, 0x423d, 0xa2, 0xf7, 0x19, 0xc3, 0x2e, 0x51, 0xcc, 0xc1);
    private static readonly Guid BDA_EXTENSION_PROPERTY_SET_DISEQC = new Guid(0x7db2deea, 0x42b4, 0x423d, 0xa2, 0xf7, 0x19, 0xc3, 0x2e, 0x51, 0xcc, 0xc1);

    private static readonly int DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(DiseqcMessage));  // 72
    private static readonly int STREAM_INFO_SIZE = Marshal.SizeOf(typeof(StreamInfo));        // 40

    private const int MAX_DISEQC_MESSAGE_LENGTH = 64;
    private const int MAX_INPUT_STREAM_ID_COUNT = 16;

    private static readonly int GENERAL_BUFFER_SIZE = Math.Max(DISEQC_MESSAGE_SIZE, STREAM_INFO_SIZE);

    #endregion

    #region variables

    private bool _isOmicom = false;
    private IKsPropertySet _propertySet = null;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private bool _isInputStreamSelectionSupported = false;

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
      this.LogDebug("Omicom: initialising");

      if (_isOmicom)
      {
        this.LogWarn("Omicom: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Omicom: tuner type not supported");
        return false;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Omicom: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_DISEQC, (int)BdaExtensionPropertyDiseqc.DiseqcWrite, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Omicom: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      this.LogInfo("Omicom: extension supported");
      _isOmicom = true;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);

      // Check for DVB-S2 input stream selection support.
      _propertySet = context as IKsPropertySet;
      hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_CUSTOM, (int)BdaExtensionPropertyCustom.InputStreamFilter, out support);
      if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Get) && support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Omicom: stream selection supported");
        _isInputStreamSelectionSupported = true;
      }
      else
      {
        this.LogDebug("Omicom: stream selection not supported, hr = 0x{0:x}, support = {1}", hr, support);
        _isInputStreamSelectionSupported = false;
      }
      return true;
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
      this.LogDebug("Omicom: on before tune call back");
      action = TunerAction.Default;

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return;
      }

      // We only need to tweak the modulation for DVB-S/S2 channels.
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      ModulationType bdaModulation = ModulationType.ModNotSet;
      if (channel is ChannelDvbS2)
      {
        if (Environment.OSVersion.Version.Major >= 6) // Vista and newer
        {
          if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
          {
            bdaModulation = ModulationType.ModNbcQpsk;
          }
          else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk8)
          {
            bdaModulation = ModulationType.ModNbc8Psk;
          }
        }
        else
        {
          // Assume that the driver can auto-detect the actual scheme.
          bdaModulation = ModulationType.Mod8Psk;
        }
      }
      else if (channel is ChannelDvbS && satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
      {
        bdaModulation = ModulationType.ModQpsk;
      }

      if (bdaModulation != ModulationType.ModNotSet)
      {
        this.LogDebug("  modulation = {0}", bdaModulation);
        satelliteChannel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
      }
    }

    #endregion

    #endregion

    #region IStreamSelector members

    /// <summary>
    /// Get the identifiers for the available streams.
    /// </summary>
    /// <param name="streamIds">The stream identifiers.</param>
    /// <returns><c>true</c> if the stream identifiers are retrieved successfully, otherwise <c>false</c></returns>
    public bool GetAvailableStreamIds(out ICollection<int> streamIds)
    {
      this.LogDebug("Omicom: get available stream IDs");
      streamIds = null;

      if (!_isOmicom || !_isInputStreamSelectionSupported)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET_CUSTOM, (int)BdaExtensionPropertyCustom.StreamInfo, _generalBuffer, STREAM_INFO_SIZE, _generalBuffer, STREAM_INFO_SIZE, out returnedByteCount);
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == STREAM_INFO_SIZE)
      {
        this.LogDebug("Omicom: result = success");
        StreamInfo info = (StreamInfo)Marshal.PtrToStructure(_generalBuffer, typeof(StreamInfo));
        streamIds = new List<int>(info.InputStreamCount);
        for (int i = 0; i < info.InputStreamCount; i++)
        {
          streamIds.Add(info.InputStreamIds[i]);
        }
        return true;
      }

      this.LogError("Omicom: failed to get available stream IDs, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      return false;
    }

    /// <summary>
    /// Select a stream.
    /// </summary>
    /// <param name="streamId">The identifier of the stream to select.</param>
    /// <returns><c>true</c> if the stream is selected successfully, otherwise <c>false</c></returns>
    public bool SelectStream(int streamId)
    {
      this.LogDebug("Omicom: select stream, stream ID = {0}", streamId);

      if (!_isOmicom || !_isInputStreamSelectionSupported)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }

      Marshal.WriteInt32(_generalBuffer, 0, streamId);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_CUSTOM, (int)BdaExtensionPropertyCustom.InputStreamFilter, _generalBuffer, sizeof(int), _generalBuffer, sizeof(int));
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Omicom: result = success");
        return true;
      }

      this.LogError("Omicom: failed to select stream, hr = 0x{0:x}, stream ID = {1}", hr, streamId);
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
      this.LogDebug("Omicom: send DiSEqC command");

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Omicom: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Omicom: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.Message = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);
      message.MessageLength = (byte)command.Length;
      message.RepeatCount = 0;

      Marshal.StructureToPtr(message, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, DISEQC_MESSAGE_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_DISEQC, (int)BdaExtensionPropertyDiseqc.DiseqcWrite,
        _generalBuffer, DISEQC_MESSAGE_SIZE,
        _generalBuffer, DISEQC_MESSAGE_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Omicom: result = success");
        return true;
      }

      this.LogError("Omicom: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Omicom: send tone burst command, command = {0}", command);

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }

      if (command == ToneBurst.ToneBurst)
      {
        Marshal.WriteInt32(_generalBuffer, 0, (int)OmicomToneBurst.ToneBurst);
      }
      else if (command == ToneBurst.DataBurst)
      {
        Marshal.WriteInt32(_generalBuffer, 0, (int)OmicomToneBurst.DataBurst);
      }

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_DISEQC, (int)BdaExtensionPropertyDiseqc.ToneBurst, _generalBuffer, sizeof(int), _generalBuffer, sizeof(int));
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Omicom: result = success");
        return true;
      }

      this.LogError("Omicom: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("Omicom: set tone state, state = {0}", state);

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }

      if (state == Tone22kState.Off)
      {
        Marshal.WriteInt32(_generalBuffer, 0, (int)OmicomTone22kState.Off);
      }
      else if (state == Tone22kState.On)
      {
        Marshal.WriteInt32(_generalBuffer, 0, (int)OmicomTone22kState.On);
      }

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_DISEQC, (int)BdaExtensionPropertyDiseqc.Tone22kState, _generalBuffer, sizeof(int), _generalBuffer, sizeof(int));
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Omicom: result = success");
        return true;
      }

      this.LogError("Omicom: failed to set tone state, hr = 0x{0:x}", hr);
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
      this.LogDebug("Omicom: read DiSEqC response");
      response = null;

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }

      for (int i = 0; i < DISEQC_MESSAGE_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET_DISEQC, (int)BdaExtensionPropertyDiseqc.DiseqcRead,
        _generalBuffer, DISEQC_MESSAGE_SIZE,
        _generalBuffer, DISEQC_MESSAGE_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != DISEQC_MESSAGE_SIZE)
      {
        this.LogError("Omicom: failed to read DiSEqC response, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return false;
      }

      //Dump.DumpBinary(_generalBuffer, DISEQC_MESSAGE_SIZE);
      DiseqcMessage message = (DiseqcMessage)Marshal.PtrToStructure(_generalBuffer, typeof(DiseqcMessage));
      if (message.MessageLength > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Omicom: DiSEqC reply too long, length = {0}", message.MessageLength);
        return false;
      }
      this.LogDebug("Omicom: result = success");
      response = new byte[message.MessageLength];
      Buffer.BlockCopy(message.Message, 0, response, 0, message.MessageLength);
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

    ~Omicom()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (isDisposing)
      {
        _propertySet = null;
      }
      _isOmicom = false;
    }

    #endregion
  }
}