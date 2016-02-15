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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Compro
{
  /// <summary>
  /// A class for handling DiSEqC for Compro tuners.
  /// </summary>
  public class Compro : BaseTunerExtension, IDiseqcDevice, IDisposable, IPowerDevice
  {
    #region enums

    private enum BdaExtensionDiseqcProperty
    {
      DiseqcBasic = 0,
      DiseqcRaw,
      TonePower
    }

    private enum BdaExtensionProperty
    {
      MacAddress = 0
    }

    private enum Compro22k : byte
    {
      Off = 0,
      On
    }

    private enum ComproLnbPower : byte
    {
      Off = 0x02,
      On = 0x03
    }

    private enum ComproSwitchType
    {
      None = 0,
      Single,
      Tone,               // 22 kHz 2 port
      Mini,               // tone burst 2 port
      Diseqc1_0,          // DiSEqC 1.0 4 port (committed)
      Diseqc1_2           // DiSEqC 1.2 16 port (uncommitted)
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_DISEQC_PROPERTY_SET = new Guid(0x0c12bf87, 0x5bc0, 0x4dda, 0x9d, 0x07, 0x21, 0xe5, 0xc2, 0xf3, 0xb9, 0xae);
    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xa1aa3f96, 0x2ea, 0x4ccb, 0xa7, 0x14, 0x0, 0xbc, 0xd3, 0x98, 0xad, 0xb4);

    private static readonly int KS_PROPERTY_SIZE = Marshal.SizeOf(typeof(KsProperty));  // 24;
    private const int MAX_DISEQC_MESSAGE_LENGTH = 8;
    private const int MAC_ADDRESS_LENGTH = 6;
    private static readonly int GENERAL_BUFFER_SIZE = KS_PROPERTY_SIZE + 4;

    #endregion

    #region variables

    private bool _isCompro = false;
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _commandBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

    #endregion

    /// <summary>
    /// Attempt to read the MAC address from the tuner.
    /// </summary>
    private void ReadMacAddress()
    {
      this.LogDebug("Compro: read MAC address");
      for (int i = 0; i < MAC_ADDRESS_LENGTH; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.MacAddress,
        _generalBuffer, MAC_ADDRESS_LENGTH,
        _generalBuffer, MAC_ADDRESS_LENGTH,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != MAC_ADDRESS_LENGTH)
      {
        this.LogWarn("Compro: failed to read MAC address, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }
      else
      {
        byte[] address = new byte[MAC_ADDRESS_LENGTH];
        Marshal.Copy(_generalBuffer, address, 0, MAC_ADDRESS_LENGTH);
        this.LogDebug("  MAC address = {0}", BitConverter.ToString(address).ToLowerInvariant());
      }
    }

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
      this.LogDebug("Compro: initialising");

      if (_isCompro)
      {
        this.LogWarn("Compro: extension already initialised");
        return true;
      }

      _propertySet = context as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Compro: context is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_DISEQC_PROPERTY_SET, (int)BdaExtensionDiseqcProperty.DiseqcRaw, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("Compro: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      this.LogInfo("Compro: extension supported");
      _isCompro = true;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
      _commandBuffer = Marshal.AllocCoTaskMem(MAX_DISEQC_MESSAGE_LENGTH);
      ReadMacAddress();
      return true;
    }

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("Compro: set power state, state = {0}", state);

      if (!_isCompro)
      {
        this.LogWarn("Compro: not initialised or interface not supported");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_DISEQC_PROPERTY_SET, (int)BdaExtensionDiseqcProperty.TonePower, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Compro: property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      if (state == PowerState.On)
      {
        Marshal.WriteByte(_commandBuffer, 0, (byte)ComproLnbPower.On);
      }
      else
      {
        Marshal.WriteByte(_commandBuffer, 0, (byte)ComproLnbPower.Off);
      }

      hr = _propertySet.Set(BDA_EXTENSION_DISEQC_PROPERTY_SET, (int)BdaExtensionDiseqcProperty.TonePower,
        IntPtr.Zero, 0,
        _commandBuffer, sizeof(byte)
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Compro: result = success");
        return true;
      }

      this.LogError("Compro: failed to set power state, hr = 0x{0:x}", hr);
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
      this.LogDebug("Compro: send DiSEqC command");

      if (!_isCompro)
      {
        this.LogWarn("Compro: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Compro: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Compro: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      for (int i = 0; i < GENERAL_BUFFER_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }
      Marshal.WriteInt32(_generalBuffer, 0, command.Length);

      Marshal.Copy(command, 0, _commandBuffer, command.Length);

      int hr = _propertySet.Set(BDA_EXTENSION_DISEQC_PROPERTY_SET, (int)BdaExtensionDiseqcProperty.DiseqcRaw,
        _generalBuffer, GENERAL_BUFFER_SIZE,
        _commandBuffer, MAX_DISEQC_MESSAGE_LENGTH
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Compro: result = success");
        return true;
      }

      this.LogError("Compro: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Compro: send tone burst command, command = {0}", command);

      if (!_isCompro)
      {
        this.LogWarn("Compro: not initialised or interface not supported");
        return false;
      }

      Marshal.WriteInt32(_generalBuffer, 0, (int)ComproSwitchType.Mini);
      if (command == ToneBurst.ToneBurst)
      {
        Marshal.WriteInt32(_commandBuffer, 0, 0);
      }
      else if (command == ToneBurst.DataBurst)
      {
        Marshal.WriteInt32(_commandBuffer, 0, 1);
      }
      int hr = _propertySet.Set(BDA_EXTENSION_DISEQC_PROPERTY_SET, (int)BdaExtensionDiseqcProperty.DiseqcBasic,
        _generalBuffer, sizeof(int),
        _commandBuffer, sizeof(int)
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Compro: result = success");
        return true;
      }

      this.LogError("Compro: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("Compro: set tone state, state = {0}", state);

      if (!_isCompro)
      {
        this.LogWarn("Compro: not initialised or interface not supported");
        return false;
      }

      if (state == Tone22kState.On)
      {
        Marshal.WriteByte(_commandBuffer, 0, (byte)Compro22k.On);
      }
      else
      {
        Marshal.WriteByte(_commandBuffer, 0, (byte)Compro22k.Off);
      }
      int hr = _propertySet.Set(BDA_EXTENSION_DISEQC_PROPERTY_SET, (int)BdaExtensionDiseqcProperty.TonePower,
        IntPtr.Zero, 0,
        _commandBuffer, sizeof(byte)
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Compro: result = success");
        return true;
      }

      this.LogError("Compro: failed to set tone state, hr = 0x{0:x}", hr);
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
      // Not implemented.
      response = null;
      return false;
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

    ~Compro()
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
      if (_commandBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_commandBuffer);
        _commandBuffer = IntPtr.Zero;
      }
      _isCompro = false;
    }

    #endregion
  }
}
