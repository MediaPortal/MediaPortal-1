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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Omicom
{
  /// <summary>
  /// A class for handling DiSEqC for Omicom tuners.
  /// </summary>
  public class Omicom : BaseCustomDevice, IDiseqcDevice
  {
    #region enums

    private enum BdaExtensionProperty
    {
      DiseqcWrite = 0,
      DiseqcRead,
      Tone22k
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

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0x7db2deea, 0x42b4, 0x423d, 0xa2, 0xf7, 0x19, 0xc3, 0x2e, 0x51, 0xcc, 0xc1);

    private static readonly int DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(DiseqcMessage));  // 72
    private const int MAX_DISEQC_MESSAGE_LENGTH = 64;

    #endregion

    #region variables

    private IntPtr _diseqcBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;
    private bool _isOmicom = false;

    #endregion

    #region ICustomDevice members

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
      this.LogDebug("Omicom: initialising");

      if (_isOmicom)
      {
        this.LogWarn("Omicom: extension already initialised");
        return true;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Omicom: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcWrite, out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Omicom: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogInfo("Omicom: extension supported");
      _isOmicom = true;
      _diseqcBuffer = Marshal.AllocCoTaskMem(DISEQC_MESSAGE_SIZE);
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
      this.LogDebug("Omicom: on before tune call back");
      action = TunerAction.Default;

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return;
      }

      // We only need to tweak the modulation for DVB-S2 channels.
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return;
      }

      if (ch.ModulationType == ModulationType.ModQpsk || ch.ModulationType == ModulationType.Mod8Psk)
      {
        ch.ModulationType = ModulationType.Mod8Psk;
      }
      this.LogDebug("  modulation = {0}", ch.ModulationType);
    }

    #endregion

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The Omicom interface does not support sending tone burst commands.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Omicom: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }

      Marshal.WriteInt32(_diseqcBuffer, 0, (int)tone22kState);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Tone22k, _diseqcBuffer, sizeof(int), _diseqcBuffer, sizeof(int));
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Omicom: result = success");
        return true;
      }

      this.LogError("Omicom: failed to set tone state, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
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

      Marshal.StructureToPtr(message, _diseqcBuffer, false);
      //Dump.DumpBinary(_diseqcBuffer, DISEQC_MESSAGE_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcWrite,
        _diseqcBuffer, DISEQC_MESSAGE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_SIZE
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Omicom: result = success");
        return true;
      }

      this.LogError("Omicom: failed to send DiSEqC command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      this.LogDebug("Omicom: read DiSEqC response");
      response = null;

      if (!_isOmicom)
      {
        this.LogWarn("Omicom: not initialised or interface not supported");
        return false;
      }

      for (int i = 0; i < DISEQC_MESSAGE_SIZE; i++)
      {
        Marshal.WriteByte(_diseqcBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.DiseqcRead,
        _diseqcBuffer, DISEQC_MESSAGE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_SIZE,
        out returnedByteCount
      );
      if (hr != (int)HResult.Severity.Success || returnedByteCount != DISEQC_MESSAGE_SIZE)
      {
        this.LogError("Omicom: failed to read DiSEqC response, hr = 0x{0:x} ({1}), byte count = {2}", hr, HResult.GetDXErrorString(hr), returnedByteCount);
        return false;
      }

      //Dump.DumpBinary(_diseqcBuffer, DISEQC_MESSAGE_SIZE);
      DiseqcMessage message = (DiseqcMessage)Marshal.PtrToStructure(_diseqcBuffer, typeof(DiseqcMessage));
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
    public override void Dispose()
    {
      if (_diseqcBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_diseqcBuffer);
        _diseqcBuffer = IntPtr.Zero;
      }
      _propertySet = null;
      _isOmicom = false;
    }

    #endregion
  }
}