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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.GenpixOpenSource
{
  /// <summary>
  /// A class for handling DiSEqC Genpix devices using the open source BDA driver available from
  /// http://sourceforge.net/projects/genpixskywalker/ and http://sourceforge.net/projects/bdaskywalker/.
  /// </summary>
  public class GenpixOpenSource : BaseCustomDevice, IDiseqcDevice
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

    [StructLayout(LayoutKind.Sequential)]
    private struct DiseqcMessage
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcMessageLength)]
      public byte[] Message;
      public byte MessageLength;
    }

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0x0b5221eb, 0xf4c4, 0x4976, 0xb9, 0x59, 0xef, 0x74, 0x42, 0x74, 0x64, 0xd9);

    private const int InstanceSize = 32;    // The size of a property instance (KSP_NODE) parameter.

    private const int DiseqcMessageSize = 7;
    private const int MaxDiseqcMessageLength = 6;

    #endregion

    #region variables

    private bool _isGenpixOpenSource = false;
    private IntPtr _diseqcBuffer = IntPtr.Zero;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return "Genpix (Open Source)";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      this.LogDebug("Genpix (Open Source): initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("Genpix (Open Source): tuner filter is null");
        return false;
      }
      if (_isGenpixOpenSource)
      {
        this.LogDebug("Genpix (Open Source): device is already initialised");
        return true;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Genpix (Open Source): pin is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Genpix (Open Source): device does not support the Genpix property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("Genpix (Open Source): supported device detected");
      _isGenpixOpenSource = true;
      _diseqcBuffer = Marshal.AllocCoTaskMem(DiseqcMessageSize);
      _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      return true;
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The Genpix open source interface does not support directly setting the 22 kHz tone state. The
    /// tuning request LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      this.LogDebug("Genpix (Open Source): set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isGenpixOpenSource || _propertySet == null)
      {
        this.LogDebug("Genpix (Open Source): device not initialised or interface not supported");
        return false;
      }

      if (toneBurstState == ToneBurst.None)
      {
        this.LogDebug("Genpix (Open Source): result = success");
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
        this.LogDebug("Genpix (Open Source): result = success");
        return true;
      }

      this.LogDebug("Genpix (Open Source): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      this.LogDebug("Genpix (Open Source): send DiSEqC command");

      if (!_isGenpixOpenSource || _propertySet == null)
      {
        this.LogDebug("Genpix (Open Source): device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("Genpix (Open Source): command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        this.LogDebug("Genpix (Open Source): command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = (byte)command.Length;
      message.Message = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);

      Marshal.StructureToPtr(message, _diseqcBuffer, true);
      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, InstanceSize,
        _diseqcBuffer, DiseqcMessageSize
      );
      if (hr == 0)
      {
        this.LogDebug("Genpix (Open Source): result = success");
        return true;
      }

      this.LogDebug("Genpix (Open Source): result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    public bool ReadResponse(out byte[] response)
    {
      // Not supported.
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      if (_propertySet != null)
      {
        DsUtils.ReleaseComObject(_propertySet);
        _propertySet = null;
      }
      if (_diseqcBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_diseqcBuffer);
        _diseqcBuffer = IntPtr.Zero;
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      _isGenpixOpenSource = false;
    }

    #endregion
  }
}