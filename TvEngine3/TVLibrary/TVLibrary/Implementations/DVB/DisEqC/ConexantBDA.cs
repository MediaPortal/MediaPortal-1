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
using TvLibrary.Channels;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A base class for handling DiSEqC for various Conexant-based tuners
  /// including Hauppauge, Geniatech and DVBSky.
  /// </summary>
  public class ConexantBDA : IDiSEqCController, IDisposable
  {
    #region enums

    /// <summary>
    /// The custom/extended properties supported by the tuner that are
    /// not defined through BDA interfaces.
    /// </summary>
    protected enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For blind scanning.
      ScanFrequency,
      /// For direct/custom tuning.
      ChannelChange,
      /// For retrieving the actual frequency that the tuner is tuned to.
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

    private enum CxModulation
    {
      Undefined = 0,
      DvbsQpsk,
      DvbsBpsk,
      DciiQpskMux,
      DciiQpskSplitI,
      DciiQpskSplitQ
    }

    [Flags]
    private enum CxFecRate
    {
      None = 0,
      Rate1_2 = 1,
      Rate1_3 = 2,
      Rate3_4 = 4,
      Rate4_5 = 8,
      Rate5_6 = 16,
      Rate6_7 = 32,
      Rate7_8 = 64,
      Rate5_11 = 128,       // DCII
      Rate3_5 = 256         // DCII
    }

    private enum CxSpectralInversion
    {
      Undefined = 0,
      Off,                  // Uninverted, check only nominal inversion.
      On,                   // Inverted, check only nominal inversion.
      OnBoth,               // Uninverted. check both inversions.
      OffBoth               // Inverted, check both inversions.
    }

    private enum CxSampleFrequency
    {
      Undefined = 0,
      Nominal,
      DciiNominal,
      External
    }

    private enum CxPolarisation
    {
      Undefined = 0,
      High,                 // 18 V = linear horizontal or circular left
      Low                   // 13 V = linear vertical or circular right
    }

    private enum CxTone22k
    {
      Undefined = 0,
      On,
      Off
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct DiseqcMessageParams
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcTxMessageLength)]
      public byte[] DiseqcTransmitMessage;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcRxMessageLength)]
      public byte[] DiseqcReceiveMessage;
      public UInt32 DiseqcTransmitMessageLength;
      public UInt32 DiseqcReceiveMessageLength;
      public UInt32 AmplitudeAttenuation;       // range = 3 (max amplitude) - 63 (min amplitude)
      public bool IsToneBurstModulated;
      public CxDiseqcVersion DiseqcVersion;
      public CxDiseqcReceiveMode DiseqcReceiveMode;
      public bool IsLastMessage;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1), ComVisible(true)]
    private struct ChannelParams
    {
      public UInt32 Frequency;
      public CxModulation Modulation;
      public CxFecRate FecRate;
      public UInt32 SymbolRate;
      public CxSpectralInversion SpectralInversion;
      public CxSampleFrequency SampleRate;
      public CxPolarisation Polarisation;
      public CxTone22k Tone22k;
      public UInt32 FecRates;                   // CxFecRate values OR'd together.
    }

    #endregion

    #region constants

    private static readonly Guid ConexantBdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);

    private const int DiseqcMessageParamsSize = 188;
    /// <summary>
    /// The size of a property instance (KspNode) parameter.
    /// </summary>
    protected const int InstanceSize = 32;
    private const int MaxDiseqcTxMessageLength = 151;   // 3 bytes per message * 50 messages
    private const int MaxDiseqcRxMessageLength = 9;     // reply fifo size, do not increase (hardware limitation)

    #endregion

    #region variables

    private bool _isConexant = false;

    /// <summary>
    /// Buffer for the instance parameter when setting properties.
    /// </summary>
    protected IntPtr _instanceBuffer = IntPtr.Zero;

    /// <summary>
    /// Buffer for property parameters.
    /// </summary>
    protected IntPtr _paramBuffer = IntPtr.Zero;

    /// <summary>
    /// A reference to the property set instance.
    /// </summary>
    protected IKsPropertySet _propertySet = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="ConexantBDA"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public ConexantBDA(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        return;
      }
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      KSPropertySupport support;
      _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage, out support);
      if ((support & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Conexant: supported tuner detected");
        _isConexant = true;
        _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
        _paramBuffer = Marshal.AllocCoTaskMem(DiseqcMessageParamsSize);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Conexant-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Conexant-compatible tuner, otherwise <c>false</c></value>
    public bool IsConexant
    {
      get
      {
        return _isConexant;
      }
    }

    /// <summary>
    /// Accessor for the property set GUID. This allows other classes with identical
    /// structs but different GUIDs to easily inherit the methods defined in this class.
    /// </summary>
    /// <value>the GUID for the driver's custom tuner property set</value>
    protected virtual Guid BdaExtensionPropertySet
    {
      get
      {
        return ConexantBdaExtensionPropertySet;
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
      // This function needs to be tested. I'm uncertain whether
      // the driver will accept commands with no DiSEqC messages.
      Log.Log.Debug("Conexant: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);
      if (toneBurstState == ToneBurst.Off)
      {
        Log.Log.Debug("Conexant: result = success");
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

      Marshal.StructureToPtr(message, _paramBuffer, true);
      DVB_MMI.DumpBinary(_paramBuffer, 0, DiseqcMessageParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, InstanceSize,
        _paramBuffer, DiseqcMessageParamsSize
      );

      // Note: 22 kHz control is not supported.

      if (hr == 0)
      {
        Log.Log.Debug("Conexant: result = success");
        return true;
      }

      Log.Log.Debug("Conexant: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

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
      Log.Log.Debug("Conexant: send DiSEqC command");

      int length = command.Length;
      if (length > MaxDiseqcTxMessageLength)
      {
        Log.Log.Debug("Conexant: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.DiseqcTransmitMessage = new byte[MaxDiseqcTxMessageLength];
      for (int i = 0; i < length; i++)
      {
        message.DiseqcTransmitMessage[i] = command[i];
      }
      message.DiseqcTransmitMessageLength = (uint)length;
      message.DiseqcReceiveMessageLength = 0;
      message.AmplitudeAttenuation = 3;
      // If this is a switch command for port A then send a tone
      // burst command ("simple A").
      if (length == 4 && ((command[2] == 0x38 && (command[3] | 0x0c) == 0) ||
        (command[2] == 0x39 && (command[3] | 0x0f) == 0)))
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

      Marshal.StructureToPtr(message, _paramBuffer, true);
      DVB_MMI.DumpBinary(_paramBuffer, 0, DiseqcMessageParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, InstanceSize,
        _paramBuffer, DiseqcMessageParamsSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Conexant: result = success");
        return true;
      }

      Log.Log.Debug("Conexant: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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

    #region IDisposable member

    /// <summary>
    /// Free unmanaged memory buffers and release COM objects.
    /// </summary>
    public virtual void Dispose()
    {
      if (_propertySet != null)
      {
        Release.ComObject(_propertySet);
      }
      if (_isConexant)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        Marshal.FreeCoTaskMem(_paramBuffer);
      }
    }

    #endregion
  }
}