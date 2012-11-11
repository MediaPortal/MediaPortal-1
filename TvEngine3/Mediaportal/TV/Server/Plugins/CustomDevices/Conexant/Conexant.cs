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

namespace Mediaportal.TV.Server.Plugins.CustomDevices.Conexant
{
  /// <summary>
  /// A base class for handling DiSEqC for various Conexant-based devices including Hauppauge, Geniatech
  /// and DVBSky.
  /// </summary>
  public class Conexant : BaseCustomDevice, IDiseqcDevice
  {
 

    #region enums

    /// <summary>
    /// The custom/extended properties supported by Conexant devices.
    /// </summary>
    protected enum BdaExtensionProperty
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
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcTxMessageLength)]
      public byte[] DiseqcTransmitMessage;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDiseqcRxMessageLength)]
      public byte[] DiseqcReceiveMessage;
      public UInt32 DiseqcTransmitMessageLength;
      public UInt32 DiseqcReceiveMessageLength;
      public UInt32 AmplitudeAttenuation;       // range = 3 (max amplitude) - 63 (min amplitude)
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsToneBurstModulated;
      public CxDiseqcVersion DiseqcVersion;
      public CxDiseqcReceiveMode DiseqcReceiveMode;
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsLastMessage;
    }

    #endregion

    #region constants

    private static readonly Guid ConexantBdaExtensionPropertySet = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);

    /// <summary>
    /// The size of a property instance (KSP_NODE) parameter.
    /// </summary>
    protected const int InstanceSize = 32;

    private const int DiseqcMessageParamsSize = 188;
    private const int MaxDiseqcTxMessageLength = 151;   // 3 bytes per message * 50 messages, plus NULL termination
    private const int MaxDiseqcRxMessageLength = 9;     // reply first-in-first-out buffer size (hardware limited)

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

    private Guid _propertySetGuid = Guid.Empty;

    #endregion

    #region constructors

    /// <summary>
    /// Regular constructor for Conexant instances.
    /// </summary>
    public Conexant()
    {
      _propertySetGuid = ConexantBdaExtensionPropertySet;
    }

    /// <summary>
    /// Alternate constructor for Conexant instances. This constructor provides an additional way to use
    /// the functionality defined in this class without directly inheriting from the class.
    /// </summary>
    /// <param name="propertySetGuid">The BDA extension property set GUID to use when setting properties.</param>
    public Conexant(Guid propertySetGuid)
    {
      _propertySetGuid = propertySetGuid;
    }

    #endregion

    /// <summary>
    /// Accessor for the property set GUID. This allows other classes with identical structs but different
    /// GUIDs to easily inherit the methods defined in this class.
    /// </summary>
    /// <value>the GUID for the driver's custom property set</value>
    protected virtual Guid BdaExtensionPropertySet
    {
      get
      {
        // When the default constructor is used, the GUID returned will be the default Conexant GUID. The
        // GUID may be overriden via inheritence (overriding this property) or changed without inheritance
        // using the alternate constructor.
        return _propertySetGuid;
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // TeVii, Hauppauge, Geniatech, Turbosight, DVBSky, Prof and possibly others all use or implement the
        // same Conexant property set for DiSEqC support, often adding custom extensions. In order to ensure
        // that the full device functionality is available for all hardware we use the following priority
        // hierarchy:
        // TeVii [75] > Hauppauge, DVBSky, Turbosight [70] > Prof (USB) [65] > Prof (PCI, PCIe) [60] > Geniatech [50] > Conexant [40]
        return 40;
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
      this.LogDebug("Conexant: initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("Conexant: tuner filter is null");
        return false;
      }
      if (_isConexant)
      {
        this.LogDebug("Conexant: device is already initialised");
        return true;
      }

      IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("Conexant: pin is not a property set");
        return false;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        this.LogDebug("Conexant: device does not support the Conexant property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      this.LogDebug("Conexant: supported device detected");
      _isConexant = true;
      _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
      _paramBuffer = Marshal.AllocCoTaskMem(DiseqcMessageParamsSize);
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

      if (!_isConexant || _propertySet == null)
      {
        this.LogDebug("Conexant: device not initialised or interface not supported");
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

      Marshal.StructureToPtr(message, _paramBuffer, true);
      DVB_MMI.DumpBinary(_paramBuffer, 0, DiseqcMessageParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, InstanceSize,
        _paramBuffer, DiseqcMessageParamsSize
      );

      if (hr == 0)
      {
        this.LogDebug("Conexant: result = success");
        return true;
      }

      this.LogDebug("Conexant: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      this.LogDebug("Conexant: send DiSEqC command");

      if (!_isConexant || _propertySet == null)
      {
        this.LogDebug("Conexant: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogDebug("Conexant: command not supplied");
        return true;
      }

      int length = command.Length;
      if (length > MaxDiseqcTxMessageLength)
      {
        this.LogDebug("Conexant: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessageParams message = new DiseqcMessageParams();
      message.DiseqcTransmitMessage = new byte[MaxDiseqcTxMessageLength];
      Buffer.BlockCopy(command, 0, message.DiseqcTransmitMessage, 0, command.Length);
      message.DiseqcTransmitMessageLength = (UInt32)length;
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

      Marshal.StructureToPtr(message, _paramBuffer, true);
      //DVB_MMI.DumpBinary(_paramBuffer, 0, DiseqcMessageParamsSize);

      int hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.DiseqcMessage,
        _instanceBuffer, InstanceSize,
        _paramBuffer, DiseqcMessageParamsSize
      );
      if (hr == 0)
      {
        this.LogDebug("Conexant: result = success");
        return true;
      }

      this.LogDebug("Conexant: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      // Not implemented.
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
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      if (_paramBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_paramBuffer);
        _paramBuffer = IntPtr.Zero;
      }
      _isConexant = false;
    }

    #endregion
  }
}