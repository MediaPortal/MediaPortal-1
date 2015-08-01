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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Conexant
{
  /// <summary>
  /// A base class for handling DiSEqC for various Conexant-based tuners including Hauppauge, Geniatech
  /// and DVBSky.
  /// </summary>
  public class Conexant : BaseTunerExtension, IDiseqcDevice, IDisposable
  {
    #region enums

    /// <summary>
    /// The custom/extended properties supported by Conexant tuners.
    /// </summary>
    private enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For blind scanning.
      ScanFrequency,
      /// For direct/custom tuning.
      ChannelChange,
      /// For retrieving the actual frequency (in kHz) that the tuner is tuned to.
      EffectiveFrequency
    }

    private enum CxDiseqcVersion : uint
    {
      Undefined = 0,        // do not use - results in an error
      Version1,
      Version2,
      EchostarLegacy
    }

    private enum CxDiseqcReceiveMode : uint
    {
      Default = 0,          // Use current setting.
      Interrogation,        // Expecting multiple devices attached.
      QuickReply,           // Expecting one response (receiving is suspended after first response).
      NoReply               // Expecting no response.
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessageParams
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_TX_MESSAGE_LENGTH)]
      public byte[] MessageTransmit;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_RX_MESSAGE_LENGTH)]
      public byte[] MessageReceive;
      public uint MessageTransmitLength;
      public uint MessageReceiveLength;
      public uint AmplitudeAttenuation;         // range = 3 (max amplitude) - 63 (min amplitude)
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsToneBurstModulated;
      public CxDiseqcVersion Version;
      public CxDiseqcReceiveMode ReceiveMode;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLastMessage;                // I think this determines whether a tone burst command will be sent and 22 kHz tone state set.
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private static readonly int DISEQC_MESSAGE_PARAMS_SIZE = Marshal.SizeOf(typeof(DiseqcMessageParams));   // 188
    private const int MAX_DISEQC_TX_MESSAGE_LENGTH = 151;   // 3 bytes per message * 50 messages, plus NULL termination
    private const int MAX_DISEQC_RX_MESSAGE_LENGTH = 9;     // reply first-in-first-out buffer size (hardware limited)

    #endregion

    #region variables

    private bool _isConexant = false;
    private IKsPropertySet _propertySet = null;
    private Guid _propertySetGuid = BDA_EXTENSION_PROPERTY_SET;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _diseqcBuffer = IntPtr.Zero;

    #endregion

    #region constructors

    /// <summary>
    /// Constructor for <see cref="Conexant"/> instances.
    /// </summary>
    public Conexant()
    {
    }

    /// <summary>
    /// Constructor for non-inherited types (eg. <see cref="HauppaugeBda"/>).
    /// </summary>
    public Conexant(Guid propertySetGuid)
    {
      _propertySetGuid = propertySetGuid;
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
        return 40;
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
      this.LogDebug("Conexant: initialising");

      if (_isConexant)
      {
        this.LogWarn("Conexant: extension already initialised");
        return true;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Conexant: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Conexant: pin is not a property set");
        Release.ComObject("Conexant filter input pin", ref pin);
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(_propertySetGuid, (int)BdaExtensionProperty.DiseqcMessage, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Conexant: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        Release.ComObject("Conexant property set", ref _propertySet);
        return false;
      }

      this.LogInfo("Conexant: extension supported");
      _isConexant = true;
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      _diseqcBuffer = Marshal.AllocCoTaskMem(DISEQC_MESSAGE_PARAMS_SIZE);
      return true;
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
      this.LogDebug("Conexant: send DiSEqC command");

      if (!_isConexant)
      {
        this.LogWarn("Conexant: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Conexant: DiSEqC command not supplied");
        return true;
      }

      int length = command.Length;
      if (length > MAX_DISEQC_TX_MESSAGE_LENGTH)
      {
        this.LogError("Conexant: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.MessageTransmit = new byte[MAX_DISEQC_TX_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.MessageTransmit, 0, command.Length);
      message.MessageTransmitLength = (uint)length;
      message.MessageReceiveLength = 0;
      message.AmplitudeAttenuation = 3;

      // Choose a sensible tone burst command in case the understanding of
      // IsLastMessage is incorrect.
      // If this is a switch command for port A then choose tone burst ("simple
      // A", unmodulated), otherwise choose data burst ("simple B", modulated).
      if (
        length == 4 &&
        (
          (command[2] == (byte)DiseqcCommand.WriteN0 && (command[3] | 0x0c) == 0) ||
          (command[2] == (byte)DiseqcCommand.WriteN1 && (command[3] | 0x0f) == 0)
        )
      )
      {
        message.IsToneBurstModulated = false;
      }
      else
      {
        message.IsToneBurstModulated = true;
      }

      message.Version = CxDiseqcVersion.Version1;
      message.ReceiveMode = CxDiseqcReceiveMode.NoReply;
      message.IsLastMessage = false;    // Don't send a tone burst command.

      Marshal.StructureToPtr(message, _diseqcBuffer, false);
      //Dump.DumpBinary(_diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Conexant: result = success");
        return true;
      }

      this.LogError("Conexant: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <remarks>
    /// Don't know whether the driver will send a tone burst command without a
    /// DiSEqC command.
    /// </remarks>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Conexant: send tone burst command, command = {0}", command);

      if (!_isConexant)
      {
        this.LogWarn("Conexant: not initialised or interface not supported");
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.MessageTransmitLength = 0;
      message.MessageReceiveLength = 0;
      message.AmplitudeAttenuation = 3;
      if (command == ToneBurst.ToneBurst)
      {
        message.IsToneBurstModulated = false;
      }
      else if (command == ToneBurst.DataBurst)
      {
        message.IsToneBurstModulated = true;
      }
      message.Version = CxDiseqcVersion.Version1;
      message.ReceiveMode = CxDiseqcReceiveMode.NoReply;
      message.IsLastMessage = true;   // Send a tone burst command.

      Marshal.StructureToPtr(message, _diseqcBuffer, false);
      //Dump.DumpBinary(_diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Conexant: result = success");
        return true;
      }

      this.LogError("Conexant: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      // Set by tune request LNB frequency parameters.
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
      this.LogDebug("Conexant: read DiSEqC response");
      response = null;

      if (!_isConexant)
      {
        this.LogWarn("Conexant: not initialised or interface not supported");
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.MessageTransmitLength = 0;
      message.Version = CxDiseqcVersion.Version2;
      message.ReceiveMode = CxDiseqcReceiveMode.QuickReply;
      Marshal.StructureToPtr(message, _diseqcBuffer, false);
      int returnedByteCount;
      int hr = _propertySet.Get(_propertySetGuid, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != DISEQC_MESSAGE_PARAMS_SIZE)
      {
        this.LogError("Conexant: failed to read DiSEqC response, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return false;
      }

      Dump.DumpBinary(_diseqcBuffer, returnedByteCount);

      message = (DiseqcMessageParams)Marshal.PtrToStructure(_diseqcBuffer, typeof(DiseqcMessageParams));
      if (message.MessageReceiveLength > MAX_DISEQC_RX_MESSAGE_LENGTH)
      {
        this.LogError("Conexant: unexpected number of DiSEqC response message bytes ({0}) returned", message.MessageReceiveLength);
        return false;
      }
      response = new byte[message.MessageReceiveLength];
      Buffer.BlockCopy(message.MessageReceive, 0, response, 0, (int)message.MessageReceiveLength);
      this.LogDebug("Conexant: result = success");
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

    ~Conexant()
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
        Release.ComObject("Conexant property set", ref _propertySet);
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (_diseqcBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_diseqcBuffer);
        _diseqcBuffer = IntPtr.Zero;
      }
      _isConexant = false;
    }

    #endregion
  }
}