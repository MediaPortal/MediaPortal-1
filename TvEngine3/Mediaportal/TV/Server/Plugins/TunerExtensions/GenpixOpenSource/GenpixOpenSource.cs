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
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.GenpixOpenSource
{
  /// <summary>
  /// A class for handling DiSEqC Genpix tuners using the open source BDA driver available from
  /// http://sourceforge.net/projects/genpixskywalker/ and http://sourceforge.net/projects/bdaskywalker/.
  /// </summary>
  public class GenpixOpenSource : BaseTunerExtension, IDiseqcDevice, IDisposable
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DiseqcMessage
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] Message;
      public byte MessageLength;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0x0b5221eb, 0xf4c4, 0x4976, 0xb9, 0x59, 0xef, 0x74, 0x42, 0x74, 0x64, 0xd9);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.

    private static readonly int DISEQC_MESSAGE_SIZE = Marshal.SizeOf(typeof(DiseqcMessage));  // 7
    private const int MAX_DISEQC_MESSAGE_LENGTH = 6;

    #endregion

    #region variables

    private bool _isGenpixOpenSource = false;
    private IntPtr _diseqcBuffer = IntPtr.Zero;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IKsPropertySet _propertySet = null;

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Genpix open source";
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
      this.LogDebug("Genpix open source: initialising");

      if (_isGenpixOpenSource)
      {
        this.LogWarn("Genpix open source: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) == 0)
      {
        this.LogDebug("Genpix open source: tuner type not supported");
        return false;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("Genpix open source: context is not a filter");
        return false;
      }

      // The driver code suggests there is a separate capture filter, so the
      // pin should be connected and we should be able to check for property
      // set support.
      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Genpix open source: pin is not a property set");
        Release.ComObject("Genpix open source filter output pin", ref pin);
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Genpix open source: property set not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      this.LogInfo("Genpix open source: extension supported");
      _isGenpixOpenSource = true;
      _diseqcBuffer = Marshal.AllocCoTaskMem(DISEQC_MESSAGE_SIZE);
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
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
      this.LogDebug("Genpix open source: send DiSEqC command");

      if (!_isGenpixOpenSource)
      {
        this.LogWarn("Genpix open source: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Genpix open source: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Genpix open source: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = (byte)command.Length;
      message.Message = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);

      Marshal.StructureToPtr(message, _diseqcBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, INSTANCE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Genpix open source: result = success");
        return true;
      }

      this.LogError("Genpix open source: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Genpix open source: send tone burst command, command = {0}", command);

      if (!_isGenpixOpenSource)
      {
        this.LogWarn("Genpix open source: not initialised or interface not supported");
        return false;
      }

      // The driver interprets sending a DiSEqC message with length one as
      // a tone burst command.
      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = 1;
      message.Message = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      if (command == ToneBurst.ToneBurst)
      {
        message.Message[0] = (byte)GenpixToneBurst.ToneBurst;
      }
      else if (command == ToneBurst.DataBurst)
      {
        message.Message[0] = (byte)GenpixToneBurst.DataBurst;
      }

      Marshal.StructureToPtr(message, _diseqcBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.Diseqc,
        _instanceBuffer, INSTANCE_SIZE,
        _diseqcBuffer, DISEQC_MESSAGE_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Genpix open source: result = success");
        return true;
      }

      this.LogError("Genpix open source: failed to send tone burst command, hr = 0x{0:x}", hr);
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
      // Not supported.
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

    ~GenpixOpenSource()
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
        Release.ComObject("Genpix open source property set", ref _propertySet);
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