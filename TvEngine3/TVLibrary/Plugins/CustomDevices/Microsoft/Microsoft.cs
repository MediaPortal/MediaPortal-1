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
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;
using TvLibrary.Log;

namespace TvEngine
{
  /// <summary>
  /// This class provides a base implementation of DiSEqC and clear QAM tuning support for devices that
  /// support Microsoft BDA interfaces and de-facto standards.
  /// </summary>
  public class Microsoft : BaseCustomDevice, IDiseqcController
  {
    #region IBDA_DiseqCommand interface

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("F84E2AB0-3C6B-45E3-A0FC-8669D4B81F11"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IBDA_DiseqCommand
    {
      [PreserveSig]
      int put_EnableDiseqCommands([In] byte bEnable);

      [PreserveSig]
      int put_DiseqLNBSource([In] int ulLNBSource);

      [PreserveSig]
      int put_DiseqUseToneBurst([In] byte bUseToneBurst);

      [PreserveSig]
      int put_DiseqRepeats([In] int ulRepeats);

      [PreserveSig]
      int put_DiseqSendCommand([In] int ulRequestId, [In] int ulcbCommandLen, [In] ref byte pbCommand);

      [PreserveSig]
      int get_DiseqResponse([In] int ulRequestId, [In, Out] ref int pulcbResponseLen, [In, Out] ref byte pbResponse);
    }

    #endregion

    #region constants

    private const int InstanceSize = 32;    // The size of a property instance (KspNode) parameter.
    private const int ParamSize = 4;

    #endregion

    #region variables

    private bool _isMicrosoft = false;

    private IBDA_DiseqCommand _w7Interface = null;
    private int _requestId = 0;

    private IBDA_FrequencyFilter _oldInterface = null;
    private IBDA_DeviceControl _deviceControl = null;

    private IKsPropertySet _propertySet = null;
    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _paramBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// The class or property set that provides access to the tuner modulation parameter.
    /// </summary>
    protected virtual Guid ModulationPropertyClass
    {
      get
      {
        return typeof(IBDA_DigitalDemodulator).GUID;
      }
    }

    /// <summary>
    /// Determine if a filter supports the IBDA_DiseqCommand interface.
    /// </summary>
    /// <remarks>
    /// The IBDA_DiseqCommand is only supported on Windows 7 or higher, and only by some tuners.
    /// We can identify the tuners by searching for a node within the filter topology which implements
    /// the interface.
    /// </remarks>
    /// <param name="filter">The filter to check.</param>
    /// <returns>a control node that supports the IBDA_DiseqCommand interface if successful, otherwise <c>null</c></returns>
    private object CheckBdaDiseqcInterface(IBaseFilter filter)
    {
      Log.Debug("Microsoft: check for IBDA_DiseqCommand interface");

      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        Log.Debug("Microsoft: tuner filter is not a topology");
        return null;
      }

      // Get the node types in the filter topology.
      int nodeTypeCount;
      int[] nodeTypes = new int[32];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        Log.Debug("Microsoft: failed to get topology node types, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return null;
      }
      else if (nodeTypeCount == 0)
      {
        Log.Debug("Microsoft: no node types in the topology");
        return null;
      }

      // Check the interface GUIDs on each node.
      Guid[] interfaceGuids = new Guid[32];
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        int interfaceCount;
        hr = topology.GetNodeInterfaces(nodeTypes[i], out interfaceCount, 32, interfaceGuids);
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to get interfaces for node {0}, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
          continue;
        }

        // For each interface implemented by the node...
        for (int j = 0; j < interfaceCount; j++)
        {
          if (interfaceGuids[j].Equals(typeof(IBDA_DiseqCommand).GUID))
          {
            // Found the interface. Now attempt to get a reference.
            Log.Debug("Microsoft: found node that implements the interface");
            object controlNode;
            hr = topology.GetControlNode(0, 1, nodeTypes[i], out controlNode);
            if (hr == 0 && controlNode is IBDA_DiseqCommand)
            {
              return controlNode;
            }

            Log.Debug("Microsoft: failed to get the control interface for node {0}, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
            if (controlNode != null)
            {
              DsUtils.ReleaseComObject(controlNode);
              controlNode = null;
            }
            // If we get to here then we have determined that the node does not actually
            // support the interface, so we move on to the next node.
            break;
          }
        }
      }
      return null;
    }

    /// <summary>
    /// Determine if a filter supports the IBDA_FrequencyFilter interface.
    /// </summary>
    /// <remarks>
    /// The IBDA_FrequencyFilter.put_Range() function was the de-facto "BDA" standard for DiSEqC prior
    /// to the introduction of IBDA_DiseqCommand.
    /// </remarks>
    /// <param name="filter">The filter to check.</param>
    /// <returns>a control node that supports the IBDA_FrequencyFilter interface if successful, otherwise <c>null</c></returns>
    private object CheckFrequencyFilterInterface(IBaseFilter filter)
    {
      Log.Debug("Microsoft: check for IBDA_FrequencyFilter interface");

      IBDA_Topology topology = filter as IBDA_Topology;
      if (topology == null)
      {
        Log.Debug("Microsoft: filter is not a topology");
        return null;
      }

      object controlNode;
      int hr = topology.GetControlNode(0, 1, 0, out controlNode);
      if (hr == 0 && controlNode is IBDA_FrequencyFilter)
      {
        Log.Debug("Microsoft: found node that implements the interface");
        return controlNode;
      }

      Log.Debug("Microsoft: failed to get the control interface, hr = 0x{1:x} ({2})", hr, HResult.GetDXErrorString(hr));
      if (controlNode != null)
      {
        DsUtils.ReleaseComObject(controlNode);
        controlNode = null;
      }
      return null;
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This is the most generic ICustomDevice implementation. It should only be used as a last resort
        // when more specialised interfaces are not suitable.
        return 1;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        if (_w7Interface != null)
        {
          return "Microsoft (BDA DiSEqC)";
        }
        if (_oldInterface != null)
        {
          return "Microsoft (generic DiSEqC)";
        }
        if (_propertySet != null)
        {
          return "Microsoft (generic ATSC/QAM)";
        }
        return base.Name;
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      Log.Debug("Microsoft: initialising device");
      if (tunerType != CardType.DvbS && tunerType != CardType.Atsc)
      {
        Log.Debug("Microsoft: tuner type {0} is not supported", tunerType);
        return false;
      }
      if (_isMicrosoft)
      {
        Log.Debug("Microsoft: device is already initialised");
        return true;
      }

      // First, a check for DVB-S tuners: does the tuner support the IBDA_DiseqCommand interface?
      int hr;
      if (tunerType == CardType.DvbS)
      {
        OperatingSystem os = Environment.OSVersion;
        if (os.Platform == PlatformID.Win32NT && (os.Version.Major > 6 || (os.Version.Major == 6 && os.Version.Minor >= 1)))
        {
          _w7Interface = (IBDA_DiseqCommand)CheckBdaDiseqcInterface(tunerFilter);
          if (_w7Interface != null)
          {
            Log.Debug("Microsoft: supported device detected (IBDA_DiseqCommand interface)");
            _isMicrosoft = true;
            hr = _w7Interface.put_EnableDiseqCommands(1);
            if (hr != 0)
            {
              Log.Debug("Microsoft: failed to enable DiSEqC commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            }
            hr = _w7Interface.put_DiseqRepeats(0);
            if (hr != 0)
            {
              Log.Debug("Microsoft: failed to disable DiSEqC command repeats, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            }
            return true;
          }
        }
      }

      // Now a common check: the remaining two interfaces supported by this class require that the
      // tuner filter supports a particular property set.
      IPin pin = DsFindPin.ByName(tunerFilter, "MPEG2 Transport");
      if (pin == null)
      {
        Log.Debug("Microsoft: failed to find transport pin");
        return false;
      }

      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Debug("Microsoft: pin is not a property set");
        return false;
      }
      KSPropertySupport support;
      hr = _propertySet.QuerySupported(ModulationPropertyClass, 0, out support);
      if (hr != 0 || (support & KSPropertySupport.Set) == 0)
      {
        Log.Debug("Microsoft: property set not supported, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        DsUtils.ReleaseComObject(_propertySet);
        _propertySet = null;
        return false;
      }

      // No further checks required for ATSC tuners.
      if (tunerType == CardType.Atsc)
      {
        Log.Debug("Microsoft: supported device detected (ATSC/QAM)");
        _isMicrosoft = true;
        _instanceBuffer = Marshal.AllocCoTaskMem(InstanceSize);
        _paramBuffer = Marshal.AllocCoTaskMem(ParamSize);
        return true;
      }

      // One further check for DVB-S tuners: does the tuner support the IBDA_FrequencyFilter interface?
      _oldInterface = (IBDA_FrequencyFilter)CheckFrequencyFilterInterface(tunerFilter);
      if (_oldInterface != null)
      {
        Log.Debug("Microsoft: supported device detected (IBDA_FrequencyFilter interface)");
        _isMicrosoft = true;
        _deviceControl = tunerFilter as IBDA_DeviceControl;
        return true;
      }

      return false;
    }

    #region graph state change callbacks

    /// <summary>
    /// This callback is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="forceGraphStart">Ensure that the tuner's BDA graph is running when the tune request is submitted.</param>
    public override void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out bool forceGraphStart)
    {
      Log.Debug("Microsoft: on before tune callback");
      forceGraphStart = false;

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
        return;
      }

      // When tuning a DVB-S channel, we need to translate the modulation value.
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel != null)
      {
        if (dvbsChannel.ModulationType == ModulationType.ModQpsk)
        {
          dvbsChannel.ModulationType = ModulationType.ModNbcQpsk;
        }
        else if (dvbsChannel.ModulationType == ModulationType.Mod8Psk)
        {
          dvbsChannel.ModulationType = ModulationType.ModNbc8Psk;
        }
        else if (dvbsChannel.ModulationType == ModulationType.ModNotSet)
        {
          dvbsChannel.ModulationType = ModulationType.ModQpsk;
        }
        Log.Debug("  modulation = {0}", dvbsChannel.ModulationType);
      }

      // When tuning a clear QAM channel, we need to set the modulation directly.
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel != null && _propertySet != null)
      {
        if (atscChannel.ModulationType == ModulationType.Mod64Qam || atscChannel.ModulationType == ModulationType.Mod256Qam)
        {
          Marshal.WriteInt32(_paramBuffer, (Int32)atscChannel.ModulationType);
          int hr = _propertySet.Set(ModulationPropertyClass, 0, _instanceBuffer, InstanceSize, _paramBuffer, ParamSize);
          if (hr != 0)
          {
            Log.Debug("Microsoft: failed to set QAM modulation, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          }
          else
          {
            Log.Debug("  modulation = {0}", atscChannel.ModulationType);
          }
        }
      }
    }

    #endregion

    #endregion

    #region IDiseqcController members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>The Microsoft interface does not support directly setting the 22 kHz tone state. The tuning
    /// request LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Debug("Microsoft: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (_w7Interface == null)
      {
        Log.Debug("Microsoft: the interface does not support setting the tone state");
        return false;
      }

      int hr;
      try
      {
        if (toneBurstState != ToneBurst.Off)
        {
          // First enable tone burst commands.
          hr = _w7Interface.put_DiseqUseToneBurst(1);
          if (hr != 0)
          {
            Log.Debug("Microsoft: failed to enable tone burst commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }

          int portNumber = 0;
          if (toneBurstState == ToneBurst.DataBurst)
          {
            portNumber = 1;
          }

          // Send a DiSEqC command which sends the appropriate tone burst
          // command as well.
          hr = _w7Interface.put_DiseqLNBSource(portNumber);
          if (hr != 0)
          {
            Log.Debug("Microsoft: failed to send tone burst command, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
            return false;
          }
        }
        Log.Debug("Microsoft: result = success");
        return true;
      }
      finally
      {
        // Finally, disable tone burst commands again.
        hr = _w7Interface.put_DiseqUseToneBurst(0);
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to disable tone burst commands, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        }
      }
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      Log.Debug("Microsoft: send DiSEqC command");

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("Microsoft: command not supplied");
        return true;
      }

      // IBDA_DiseqCommand interface
      if (_w7Interface != null)
      {
        int hr = _w7Interface.put_DiseqSendCommand(_requestId, command.Length, ref command[0]);
        if (hr == 0)
        {
          Log.Debug("Microsoft: result = success");
          _requestId++;
          return true;
        }

        Log.Debug("Microsoft: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      // IBDA_FrequencyFilter interface
      else if (_oldInterface != null)
      {
        // This interface only supports DiSEqC 1.0 switch commands. We have to attempt
        // to translate the raw command back into a supported command.
        if (command.Length != 4 || command[0] != 0xe0 || command[1] != 0x10 || command[2] != 0x38)
        {
          Log.Debug("Microsoft: command not supported");
          return false;
        }
        ulong portNumber = (ulong)((command[3] & 0xc) >> 2);
        if (portNumber > 1)
        {
          portNumber -= 2;
          portNumber |= 0x4;
        }

        int hr = _deviceControl.StartChanges();
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to start device control changes, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        hr = _oldInterface.put_Range(portNumber);
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to put range, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        hr = _deviceControl.CheckChanges();
        if (hr != 0)
        {
          Log.Debug("Microsoft: device control check chanages failed, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        hr = _deviceControl.CommitChanges();
        if (hr != 0)
        {
          Log.Debug("Microsoft: failed to commit device control chanages, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          return false;
        }

        Log.Debug("Microsoft: result = success");
        return true;
      }

      Log.Debug("Microsoft: the interface does not support sending DiSEqC commands");
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
      Log.Debug("Microsoft: read DiSEqC response");
      response = null;

      if (!_isMicrosoft)
      {
        Log.Debug("Microsoft: device not initialised or interface not supported");
        return false;
      }
      if (_w7Interface == null)
      {
        Log.Debug("Microsoft: the interface does not support reading DiSEqC responses");
        return false;
      }

      int responseLength = 0;
      byte[] tempResponse = new byte[32];
      int hr = _w7Interface.get_DiseqResponse(_requestId, ref responseLength, ref tempResponse[0]);
      if (hr == 0 && responseLength > 0 && responseLength < 33)
      {
        Log.Debug("Microsoft: result = success");
        // Copy the response into the return array.
        response = new byte[responseLength];
        Buffer.BlockCopy(tempResponse, 0, response, 0, responseLength);
        return true;
      }

      Log.Debug("Microsoft: result = failure, response length = {0}, hr = 0x{1:x} ({2})", responseLength, hr, HResult.GetDXErrorString(hr));
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      if (_w7Interface != null)
      {
        DsUtils.ReleaseComObject(_w7Interface);
        _w7Interface = null;
      }
      if (_oldInterface != null)
      {
        DsUtils.ReleaseComObject(_oldInterface);
        _oldInterface = null;
      }
      _deviceControl = null;
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
      _isMicrosoft = false;
    }

    #endregion
  }
}
