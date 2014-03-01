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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Conexant
{
  /// <summary>
  /// A base class for handling DiSEqC for various Conexant-based tuners including Hauppauge, Geniatech
  /// and DVBSky.
  /// </summary>
  public class Conexant : BaseCustomDevice, IDiseqcDevice
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
      NoReply,              // Expecting no response.
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessageParams
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_TX_MESSAGE_LENGTH)]
      public byte[] DiseqcTransmitMessage;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_RX_MESSAGE_LENGTH)]
      public byte[] DiseqcReceiveMessage;
      public uint DiseqcTransmitMessageLength;
      public uint DiseqcReceiveMessageLength;
      public uint AmplitudeAttenuation;         // range = 3 (max amplitude) - 63 (min amplitude)
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsToneBurstModulated;
      public CxDiseqcVersion DiseqcVersion;
      public CxDiseqcReceiveMode DiseqcReceiveMode;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLastMessage;
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

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 40;
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
      this.LogDebug("Conexant: initialising");

      if (_isConexant)
      {
        this.LogWarn("Conexant: extension already initialised");
        return true;
      }

      IBaseFilter tunerFilter = context as IBaseFilter;
      if (tunerFilter == null)
      {
        this.LogDebug("Conexant: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Conexant: pin is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(_propertySetGuid, (int)BdaExtensionProperty.DiseqcMessage, out support);
      if (hr != (int)HResult.Severity.Success || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Conexant: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
    /// Control whether tone/data burst and 22 kHz legacy tone are used.
    /// </summary>
    /// <remarks>
    /// The Conexant interface does not support directly setting the 22 kHz tone state. The tuning request
    /// LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Conexant: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isConexant)
      {
        this.LogWarn("Conexant: not initialised or interface not supported");
        return false;
      }

      if (toneBurstState == ToneBurst.None)
      {
        this.LogDebug("Conexant: result = success");
        return true;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.DiseqcTransmitMessageLength = 0;
      message.DiseqcReceiveMessageLength = 0;
      message.AmplitudeAttenuation = 3;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        message.IsToneBurstModulated = false;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        message.IsToneBurstModulated = true;
      }
      message.DiseqcVersion = CxDiseqcVersion.Version1;
      message.DiseqcReceiveMode = CxDiseqcReceiveMode.NoReply;
      message.IsLastMessage = true;

      Marshal.StructureToPtr(message, _diseqcBuffer, true);
      //Dump.DumpBinary(_diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE
      );

      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Conexant: result = success");
        return true;
      }

      this.LogError("Conexant: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendDiseqcCommand(byte[] command)
    {
      this.LogDebug("Conexant: send DiSEqC command");

      if (!_isConexant)
      {
        this.LogWarn("Conexant: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogError("Conexant: command not supplied");
        return true;
      }

      int length = command.Length;
      if (length > MAX_DISEQC_TX_MESSAGE_LENGTH)
      {
        this.LogError("Conexant: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.DiseqcTransmitMessage = new byte[MAX_DISEQC_TX_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.DiseqcTransmitMessage, 0, command.Length);
      message.DiseqcTransmitMessageLength = (uint)length;
      message.DiseqcReceiveMessageLength = 0;
      message.AmplitudeAttenuation = 3;
      // We have no choice about sending a tone burst command. If this is a switch command for port A then
      // send a tone burst command ("simple A"), otherwise send a data burst command ("simple B").
      if (length == 4 && ((command[2] == (byte)DiseqcCommand.WriteN0 && (command[3] | 0x0c) == 0) ||
        (command[2] == (byte)DiseqcCommand.WriteN1 && (command[3] | 0x0f) == 0)))
      {
        message.IsToneBurstModulated = false;
      }
      else
      {
        message.IsToneBurstModulated = true;
      }
      message.DiseqcVersion = CxDiseqcVersion.Version1;
      message.DiseqcReceiveMode = CxDiseqcReceiveMode.NoReply;
      message.IsLastMessage = true;

      Marshal.StructureToPtr(message, _diseqcBuffer, true);
      //Dump.DumpBinary(_diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, INSTANCE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_PARAMS_SIZE
      );
      if (hr == (int)HResult.Severity.Success)
      {
        this.LogDebug("Conexant: result = success");
        return true;
      }

      this.LogError("Conexant: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      Release.ComObject("Conexant property set", ref _propertySet);
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