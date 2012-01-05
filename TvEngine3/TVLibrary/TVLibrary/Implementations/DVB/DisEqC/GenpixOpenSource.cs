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
  /// A class for handling DiSEqC Genpix tuners using the open source BDA
  /// driver available from http://sourceforge.net/projects/genpixskywalker/
  /// and http://sourceforge.net/projects/bdaskywalker/.
  /// </summary>
  public class GenpixOpenSource : IDiSEqCController, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty : int
    {
      Diseqc = 0
    }

    private enum GenpixToneBurst : byte
    {
      ToneBurst = 0,
      DataBurst
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct DiseqcMessage
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
      public byte MessageLength;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0x0b5221eb, 0xf4c4, 0x4976, 0xb9, 0x59, 0xef, 0x74, 0x42, 0x74, 0x64, 0xd9);

    private const int InstanceSize = 32;

    private const int DiseqcMessageSize = 7;
    private const int MaxDiseqcMessageLength = 6;

    #endregion

    #region variables

    private bool _isGenpixOpenSource = false;
    private IntPtr _diseqcBuffer = IntPtr.Zero;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="GenpixOpenSource"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public GenpixOpenSource(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        return;
      }
      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      KSPropertySupport support;
      _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc, out support);
      if ((support & KSPropertySupport.Set) != 0)
      {
        Log.Log.Debug("Genpix (Open Source): supported tuner detected");
        _isGenpixOpenSource = true;
        _diseqcBuffer = Marshal.AllocCoTaskMem(DiseqcMessageSize);
        _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is a Genpix tuner using the open source driver.
    /// </summary>
    /// <value><c>true</c> if this tuner is a Genpix tuner using the open source driver, otherwise <c>false</c></value>
    public bool IsGenpixOpenSource
    {
      get
      {
        return _isGenpixOpenSource;
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
      Log.Log.Debug("Genpix (Open Source): set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (toneBurstState == ToneBurst.Off)
      {
        // The Genpix open source driver doesn't have any explicit way to
        // control the legacy tone state - the state is set based on the
        // 3 LNB parameters.
        return true;
      }

      // The driver interprets sending a DiSEqC message with length one as
      // a tone burst command.
      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = 1;
      message.Message = new byte[MaxDiseqcMessageLength];
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        message.Message[0] = (byte)GenpixToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
      {
        message.Message[0] = (byte)GenpixToneBurst.DataBurst;
      }

      Marshal.StructureToPtr(message, _diseqcBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, InstanceSize,
        _diseqcBuffer, DiseqcMessageSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Genpix (Open Source): result = success");
        return true;
      }

      Log.Log.Debug("Genpix (Open Source): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      bool successDiseqc = true;
      bool isHighBand = BandTypeConverter.IsHiBand(channel, parameters);
      ToneBurst toneBurst = ToneBurst.Off;
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
      Log.Log.Debug("Genpix (Open Source): send DiSEqC command");

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Genpix (Open Source): command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = (byte)command.Length;
      message.Message = new byte[MaxDiseqcMessageLength];
      for (int i = 0; i < command.Length; i++)
      {
        message.Message[i] = command[i];
      }

      Marshal.StructureToPtr(message, _diseqcBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, InstanceSize,
        _diseqcBuffer, DiseqcMessageSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Genpix (Open Source): result = success");
        return true;
      }

      Log.Log.Debug("Genpix (Open Source): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command. This function is untested.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      Log.Log.Debug("Genpix (Open Source): read DiSEqC command");
      // Not supported...
      reply = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (_propertySet != null)
      {
        Release.ComObject(_propertySet);
      }
      if (_isGenpixOpenSource)
      {
        Marshal.FreeCoTaskMem(_diseqcBuffer);
        Marshal.FreeCoTaskMem(_instanceBuffer);
      }
    }

    #endregion
  }
}