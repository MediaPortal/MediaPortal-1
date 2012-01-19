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

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling DiSEqC for Omicom tuners.
  /// </summary>
  public class Omicom : IDiSEqCController, IDisposable
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
      public Int32 MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
      public Int32 RepeatCount;       // Set to zero to send the message once, one => twice, two => three times... etc.
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0x7db2deea, 0x42b4, 0x423d, 0xa2, 0xf7, 0x19, 0xc3, 0x2e, 0x51, 0xcc, 0xc1);

    private const int DiseqcMessageSize = 72;
    private const int MaxDiseqcMessageLength = 64;

    #endregion

    #region variables

    private IntPtr _diseqcBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;
    private bool _isOmicom = false;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Omicom"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Omicom(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        return;
      }
      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcWrite,
                                  out support);
      if (hr != 0)
      {
        Log.Log.Debug("Omicom: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }
      if ((support & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Omicom: supported tuner detected");
        _isOmicom = true;
        _diseqcBuffer = Marshal.AllocCoTaskMem(DiseqcMessageSize);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Omicom-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Omicom-compatible tuner, otherwise <c>false</c></value>
    public bool IsOmicom
    {
      get
      {
        return _isOmicom;
      }
    }

     /// <summary>
    /// Control whether tone/data burst and 22 kHz legacy tone are used. This function is untested and does
    /// not currently support setting the tone burst state.
    /// </summary>
    /// <param name="toneBurstState">The tone/data burst state.</param>
    /// <param name="tone22kState">The 22 kHz legacy tone state.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Log.Debug("Omicom: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      Marshal.WriteInt32(_diseqcBuffer, 0, (Int32)tone22kState);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Tone22k, _diseqcBuffer, sizeof(Int32), _diseqcBuffer, sizeof(Int32));
      if (hr == 0)
      {
        Log.Log.Debug("Omicom: result = success");
        return true;
      }

      Log.Log.Debug("Omicom: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces.
    /// </summary>
    /// <param name="channel">The channel to tune.</param>
    /// <returns>The channel with parameters adjusted as necessary.</returns>
    public DVBBaseChannel SetTuningParameters(DVBBaseChannel channel)
    {
      Log.Log.Debug("Omicom: set tuning parameters");
      DVBSChannel ch = channel as DVBSChannel;
      if (ch == null)
      {
        return channel;
      }

      if (ch.ModulationType == ModulationType.ModQpsk || ch.ModulationType == ModulationType.Mod8Psk)
      {
        // Note: using 8 VSB forces the driver to auto-detect the correct
        // modulation. It may be better to use 8 PSK.
        ch.ModulationType = ModulationType.Mod8Vsb;
      }
      Log.Log.Debug("  modulation = {0}", ch.ModulationType);
      return ch as DVBBaseChannel;
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
    public virtual bool SendDiSEqCCommand(byte[] command)
    {
      Log.Log.Debug("Omicom: send DiSEqC command");

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Omicom: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.Message = new byte[MaxDiseqcMessageLength];
      for (int i = 0; i < command.Length; i++)
      {
        message.Message[i] = command[i];
      }
      message.MessageLength = (byte)command.Length;
      message.RepeatCount = 0;

      Marshal.StructureToPtr(message, _diseqcBuffer, true);
      //DVB_MMI.DumpBinary(_diseqcBuffer, 0, DiseqcMessageSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcWrite,
        _diseqcBuffer, DiseqcMessageSize,
        _diseqcBuffer, DiseqcMessageSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Omicom: result = success");
        // Note: sleep may be necessary here.
        return true;
      }

      Log.Log.Debug("Omicom: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command. This function is untested.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      Log.Log.Debug("Omicom: read DiSEqC command");
      reply = new byte[1];
      reply[0] = 0;

      for (int i = 0; i < DiseqcMessageSize; i++)
      {
        Marshal.WriteByte(_diseqcBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcRead,
        _diseqcBuffer, DiseqcMessageSize,
        _diseqcBuffer, DiseqcMessageSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != DiseqcMessageSize)
      {
        Log.Log.Debug("Omicom: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      //DVB_MMI.DumpBinary(_diseqcBuffer, 0, DiseqcMessageSize);
      DiseqcMessage message = (DiseqcMessage)Marshal.PtrToStructure(_diseqcBuffer, typeof(DiseqcMessage));
      if (message.MessageLength > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Omicom: reply too long, length = {0}", message.MessageLength);
        return false;
      }
      Log.Log.Debug("Omicom: result = success");
      reply = new byte[message.MessageLength];
      for (int i = 0; i < message.MessageLength; i++)
      {
        reply[i] = message.Message[i];
      }
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (_isOmicom)
      {
        Marshal.FreeCoTaskMem(_diseqcBuffer);
      }
    }

    #endregion
  }
}

