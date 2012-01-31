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
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.Channels;
using DirectShowLib.BDA;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for Anysee E7 series tuners.
  /// </summary>
  public class Anysee : IDiSEqCController, IDisposable
  {
    #region enums

    private enum BdaExtensionProperty
    {
      Ir = 4,
      PlatformInfo = 6,
      Diseqc = 24
    }

    private enum AnyseeToneBurst : byte
    {
      Off = 0,
      ToneBurst,
      DataBurst
    }

    private enum AnyseePlatform : ushort
    {
      Pcb508TC = 18,              // DVB-T + DVB-C + Smartcard Interface + CI
      Pcb508S2 = 19,              // DVB-S2 + Smartcard Interface + CI
      Pcb508T2C = 20,             // DVB-T2 + DVB-C + Smartcard Interface + CI
      Pcb508PTC = 21,             // PCI PCB508TC
      Pcb508PS2 = 22,             // PCI PCB508S2
      Pcb508PT2C = 23             // PCI PCB508T2C
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct IrData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;
      public bool Enable;
      public Int32 Key;           // bit 8 = repeat flag (0 = repeat), bits 7-0 = key code
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct DiseqcMessage
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;
      public Int32 MessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
      public AnyseeToneBurst ToneBurst;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct PlatformInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;
      public UInt16 Padding1;     // These bits do contain data, but I don't know what they mean.
      public AnyseePlatform Platform;
      public Int32 Padding2;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xb8e78938, 0x899d, 0x41bd, 0xb5, 0xb4, 0x62, 0x69, 0xf2, 0x80, 0x18, 0x99);
    private const int KsPropertySize = 24;
    private const int DiseqcMessageSize = KsPropertySize + MaxDiseqcMessageLength + 8;
    private const int MaxDiseqcMessageLength = 16;
    private const int PlatformInfoSize = KsPropertySize + 8;

    #endregion

    #region variables

    private bool _isAnysee = false;
    private bool _isCiSlotPresent = false;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Anysee"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    public Anysee(IBaseFilter tunerFilter)
    {
      if (tunerFilter == null)
      {
        return;
      }

      // We need a reference to the capture filter.
      IPin tunerOutputPin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      IPin captureInputPin;
      int hr = tunerOutputPin.ConnectedTo(out captureInputPin);
      Release.ComObject(tunerOutputPin);
      if (hr != 0)
      {
        Log.Log.Debug("Anysee: failed to get the capture filter input pin, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      PinInfo captureInfo;
      hr = captureInputPin.QueryPinInfo(out captureInfo);
      if (hr != 0)
      {
        Log.Log.Debug("Anysee: failed to get the capture filter input pin info, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      // Check if the filter supports the property set.
      _propertySet = captureInfo.filter as IKsPropertySet;
      if (_propertySet == null)
      {
        return;
      }

      KSPropertySupport support;
      hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Ir, out support);
      if (hr != 0)
      {
        Log.Log.Debug("Anysee: failed to query property support, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      Log.Log.Debug("Anysee: supported tuner detected");
      _isAnysee = true;
      _generalBuffer = Marshal.AllocCoTaskMem(DiseqcMessageSize);
      ReadDeviceInfo();
      _isCiSlotPresent = IsCiSlotPresent();
    }

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      Log.Log.Debug("Anysee: read device information");

      for (int i = 0; i < PlatformInfoSize; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BdaExtensionPropertySet, (int)BdaExtensionProperty.PlatformInfo,
        _generalBuffer, PlatformInfoSize,
        _generalBuffer, PlatformInfoSize,
        out returnedByteCount
      );
      if (hr != 0 || returnedByteCount != PlatformInfoSize)
      {
        Log.Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      PlatformInfo info = (PlatformInfo)Marshal.PtrToStructure(_generalBuffer, typeof(PlatformInfo));
      Log.Log.Debug("  platform = {0}", info.Platform);

      if (info.Platform == AnyseePlatform.Pcb508S2 ||
        info.Platform == AnyseePlatform.Pcb508TC ||
        info.Platform == AnyseePlatform.Pcb508T2C ||
        info.Platform == AnyseePlatform.Pcb508PS2 ||
        info.Platform == AnyseePlatform.Pcb508PTC ||
        info.Platform == AnyseePlatform.Pcb508PT2C)
      {
        _isCiSlotPresent = true;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this tuner is an Anysee-compatible tuner.
    /// </summary>
    /// <value><c>true</c> if this tuner is an Anysee-compatible tuner, otherwise <c>false</c></value>
    public bool IsAnysee
    {
      get
      {
        return _isAnysee;
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
      if (toneBurstState == ToneBurst.Off)
      {
        return true;
      }
      Log.Log.Debug("Anysee: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = 0;
      message.ToneBurst = AnyseeToneBurst.ToneBurst;
      if (toneBurstState == ToneBurst.DataBurst)
      {
        message.ToneBurst = AnyseeToneBurst.DataBurst;
      }

      Marshal.StructureToPtr(message, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcMessageSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _generalBuffer, DiseqcMessageSize,
        _generalBuffer, DiseqcMessageSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #region conditional access

    /// <summary>
    /// Determines whether a CI slot is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CI slot is present, otherwise <c>false</c></returns>
    public bool IsCiSlotPresent()
    {
      Log.Log.Debug("Anysee: is CI slot present");

      // Whether a CI slot is present is actually determined in ReadDeviceInfo().
      Log.Log.Debug("Anysee: result = {0}", _isCiSlotPresent);
      return _isCiSlotPresent;
    }

    #endregion

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
      Log.Log.Debug("Anysee: send DiSEqC command");

      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Log.Debug("Anysee: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.Message = new byte[MaxDiseqcMessageLength];
      for (int i = 0; i < command.Length; i++)
      {
        message.Message[i] = command[i];
      }
      message.MessageLength = command.Length;
      message.ToneBurst = AnyseeToneBurst.Off;

      Marshal.StructureToPtr(message, _generalBuffer, true);
      DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcMessageSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _generalBuffer, DiseqcMessageSize,
        _generalBuffer, DiseqcMessageSize
      );
      if (hr == 0)
      {
        Log.Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Get a reply to a previously sent DiSEqC command. This function is untested.
    /// </summary>
    /// <param name="reply">The reply message.</param>
    /// <returns><c>true</c> if a reply is successfully received, otherwise <c>false</c></returns>
    public bool ReadDiSEqCCommand(out byte[] reply)
    {
      // Not implemented.
      reply = null;
      return false;
    }

    #endregion

    #region IDisposable Member

    /// <summary>
    /// Close the conditional access interface and free unmanaged memory buffers.
    /// </summary>
    public void Dispose()
    {
      if (!_isAnysee)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
      }
    }

    #endregion
  }
}
