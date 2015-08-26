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
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DvbSky
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC, remote controls and
  /// stream selection for DVBSky tuners and clones (eg. certain newer
  /// TechnoTrend and Mystique models).
  /// </summary>
  /// <remarks>
  /// Actually with the exception of the property set GUIDs, the DVBSky
  /// conditional access interface is [almost] identical to the NetUP
  /// conditional access interface, and their DiSEqC interface is identical to
  /// the Conexant interface.
  /// </remarks>
  public class DvbSky : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, IDiseqcDevice, IDisposable, IRemoteControlListener, IStreamSelector
  {
    #region enums

    private enum BdaExtensionProperty
    {
      /// For sending and receiving DiSEqC messages.
      DiseqcMessage = 0,
      /// For receiving remote keypresses.
      RemoteCode,
      /// For passing blind scan parameters and commands.
      BlindScanCommand,
      /// For retrieving blind scan results.
      BlindScanData,
      /// For getting or setting DVB-C2/T2 PLP information.
      DvbPlpId,
      /// For getting the MAC address for tuner 1.
      MacAddressTuner1,
      /// For getting the MAC address for tuner 2.
      MacAddressTuner2
    }

    /// <summary>
    /// Remote control RC-5 system addresses.
    /// </summary>
    private enum DvbSkyRemoteType
    {
      DvbSky = 0,
      TechnoTrend = 21
    }

    /// <summary>
    /// Remote codes for DVBSky products.
    /// </summary>
    /// <remarks>
    /// Image: http://www.dvbshop.net/bilder/produkte/gross/Mystique-SaTiX-S2-Sky-PCI-USALS-DiseqC-12-Win-Linux-Remote_b2.jpg
    /// Testing: untested, based on SDK documentation
    /// </remarks>
    private enum DvbSkyRemoteCodeDvbSky : byte
    {
      Zero = 0,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine,
      Mute, // 10
      Stop,
      Exit,
      Okay,
      Screenshot,
      PictureInPicture,
      Right,
      Left,
      Favourite,
      Info, // 19

      Pause = 22,
      Play,

      Record = 31,
      Up,
      Down,

      Power = 37,
      Rewind,
      SkipForward,

      Recall = 41,

      Menu = 43,
      Epg,
      Zoom
    }

    /// <summary>
    /// Remote codes for TechnoTrend clones.
    /// </summary>
    /// <remarks>
    /// Image: http://www.dvbshop.net/bilder/produkte/gross/TechnoTrend-Budget-S-1500-inkl-CI-Common-Int-TT-Viewer_b2.jpg
    /// Testing: CT2-4500 CI
    /// </remarks>
    private enum DvbSkyRemoteCodeTechnoTrend : byte
    {
      Power = 1,
      Recall,           // icon: two arrows in a circle
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,  // 10
      Nine,
      Zero,
      Up,
      Left,
      Okay,
      Right,
      Down,
      Info,
      Exit,
      Red,    // 20
      Green,
      Yellow,
      Blue,
      Mute,
      Teletext,
      Source, // 26     // text: TV/radio; icon: musical note

      Epg = 34,
      ChannelUp,
      ChannelDown,
      VolumeUp,
      VolumeDown,

      Record = 58,
      Play,
      Stop,
      Rewind,
      Pause,
      FastForward
    }

    private enum DvbSkyPlpType : byte
    {
      Common = 0,
      DataType1,
      DataType2
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlpCommand   // PLP_ID_CMD
    {
      public int PlpId;     // 0..255; >= 256 is auto
      public int PlpCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] PlpIds;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public DvbSkyPlpType[] PlpTypes;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET_GENERAL = new Guid(0x03cbdcb9, 0x36dc, 0x4072, 0xac, 0x42, 0x2f, 0x94, 0xf4, 0xec, 0xa0, 0x5e);
    private static readonly Guid BDA_EXTENSION_PROPERTY_SET_CA = new Guid(0x4fdc5d3a, 0x1543, 0x479e, 0x9f, 0xc3, 0xb7, 0xdb, 0xa4, 0x73, 0xfb, 0x95);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private const int REMOTE_CONTROL_DATA_SIZE = 4;
    private static readonly int PLP_COMMAND_SIZE = Marshal.SizeOf(typeof(PlpCommand));  // 520
    private const int MAC_ADDRESS_LENGTH = 6;

    private const int REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME = 100;                   // unit = ms

    #endregion

    #region variables

    private bool _isDvbSky = false;
    private IDiseqcDevice _diseqcInterface = null;
    private IConditionalAccessProvider _caProviderInterface = null;
    private IConditionalAccessMenuActions _caMenuActionsInterface = null;
    private IKsPropertySet _propertySet = null;

    private IntPtr _instanceBuffer = IntPtr.Zero;
    private IntPtr _plpBuffer = IntPtr.Zero;

    private bool _isRemoteControlInterfaceOpen = false;
    private IntPtr _remoteControlBuffer = IntPtr.Zero;
    private Thread _remoteControlListenerThread = null;
    private AutoResetEvent _remoteControlListenerThreadStopEvent = null;

    #endregion

    private void ReadMacAddresses()
    {
      this.LogDebug("DVBSky: read MAC addresses");

      IntPtr dataBuffer = Marshal.AllocCoTaskMem(MAC_ADDRESS_LENGTH);
      try
      {
        BdaExtensionProperty[] properties = new BdaExtensionProperty[] { BdaExtensionProperty.MacAddressTuner1, BdaExtensionProperty.MacAddressTuner2 };
        string[] propertyNames = new string[] { "tuner 1 MAC address", "tuner 2 MAC address" };

        KSPropertySupport support;
        byte[] previousAddress = null;
        for (int i = 0; i < properties.Length; i++)
        {
          int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_GENERAL, (int)BdaExtensionProperty.MacAddressTuner1, out support);
          if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
          {
            this.LogWarn("DVBSky: {0} property not supported, hr = 0x{1:x}, support = {2}", propertyNames[i], hr, support);
          }
          else
          {
            for (int b = 0; b < MAC_ADDRESS_LENGTH; b++)
            {
              Marshal.WriteByte(dataBuffer, b, 0);
            }
            int returnedByteCount;
            hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET_GENERAL, (int)properties[i], _instanceBuffer, INSTANCE_SIZE, dataBuffer, MAC_ADDRESS_LENGTH, out returnedByteCount);
            if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != MAC_ADDRESS_LENGTH)
            {
              this.LogWarn("DVBSky: failed to read {0}, hr = 0x{1:x}, byte count = {2}", propertyNames[i], hr, returnedByteCount);
            }
            else
            {
              byte[] address = new byte[MAC_ADDRESS_LENGTH];
              Marshal.Copy(dataBuffer, address, 0, MAC_ADDRESS_LENGTH);
              if (previousAddress != null)
              {
                for (int b = 0; b < MAC_ADDRESS_LENGTH; b++)
                {
                  if (address[b] != previousAddress[b])
                  {
                    this.LogDebug("  {0} = {1}", propertyNames[i], BitConverter.ToString(address).ToLowerInvariant());
                    break;
                  }
                }
              }
              else
              {
                this.LogDebug("  {0} = {1}", propertyNames[i], BitConverter.ToString(address).ToLowerInvariant());
              }
              previousAddress = address;
            }
          }
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(dataBuffer);
      }
    }

    #region remote control listener thread

    /// <summary>
    /// Start a thread to listen for remote control commands.
    /// </summary>
    private void StartRemoteControlListenerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isRemoteControlInterfaceOpen)
      {
        return;
      }

      // Kill the existing thread if it is in "zombie" state.
      if (_remoteControlListenerThread != null && !_remoteControlListenerThread.IsAlive)
      {
        StopRemoteControlListenerThread();
      }
      if (_remoteControlListenerThread == null)
      {
        this.LogDebug("DVBSky: starting new remote control listener thread");
        _remoteControlListenerThreadStopEvent = new AutoResetEvent(false);
        _remoteControlListenerThread = new Thread(new ThreadStart(RemoteControlListener));
        _remoteControlListenerThread.Name = "DVBSky remote control listener";
        _remoteControlListenerThread.IsBackground = true;
        _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
        _remoteControlListenerThread.Start();
      }
    }

    /// <summary>
    /// Stop the thread that listens for remote control commands.
    /// </summary>
    private void StopRemoteControlListenerThread()
    {
      if (_remoteControlListenerThread != null)
      {
        if (!_remoteControlListenerThread.IsAlive)
        {
          this.LogWarn("DVBSky: aborting old remote control listener thread");
          _remoteControlListenerThread.Abort();
        }
        else
        {
          _remoteControlListenerThreadStopEvent.Set();
          if (!_remoteControlListenerThread.Join(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME * 2))
          {
            this.LogWarn("DVBSky: failed to join remote control listener thread, aborting thread");
            _remoteControlListenerThread.Abort();
          }
        }
        _remoteControlListenerThread = null;
        if (_remoteControlListenerThreadStopEvent != null)
        {
          _remoteControlListenerThreadStopEvent.Close();
          _remoteControlListenerThreadStopEvent = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving remote control commands.
    /// </summary>
    private void RemoteControlListener()
    {
      this.LogDebug("DVBSky: remote control listener thread start polling");
      int hr;
      int returnedByteCount;
      try
      {
        while (!_remoteControlListenerThreadStopEvent.WaitOne(REMOTE_CONTROL_LISTENER_THREAD_WAIT_TIME))
        {
          for (int i = 0; i < REMOTE_CONTROL_DATA_SIZE; i++)
          {
            Marshal.WriteByte(_remoteControlBuffer, i, 0);
          }
          hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET_GENERAL, (int)BdaExtensionProperty.RemoteCode,
            _instanceBuffer, INSTANCE_SIZE,
            _remoteControlBuffer, REMOTE_CONTROL_DATA_SIZE,
            out returnedByteCount
          );
          if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != REMOTE_CONTROL_DATA_SIZE)
          {
            this.LogError("DVBSky: failed to read remote code, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          }
          else
          {
            int code = Marshal.ReadInt32(_remoteControlBuffer, 0);
            if ((code & 0xff) != 0xff)
            {
              // Standard RC-5 code structure.
              int fieldBit = (code & 0x1000) >> 12;
              int toggleBit = (code & 0x800) >> 11;
              int systemAddress = (code & 0x7c0) >> 6;
              int rc5Code = code & 0x3f;
              string codeName;
              if (systemAddress == (int)DvbSkyRemoteType.TechnoTrend)
              {
                codeName = ((DvbSkyRemoteCodeTechnoTrend)rc5Code).ToString();
              }
              else if (systemAddress == (int)DvbSkyRemoteType.DvbSky)
              {
                codeName = ((DvbSkyRemoteCodeDvbSky)rc5Code).ToString();
              }
              else
              {
                codeName = rc5Code.ToString();
              }
              this.LogDebug("DVBSky: remote control key press, field bit = {0}, toggle bit = {1} system address = {2}, code = {3}", fieldBit, toggleBit, systemAddress, codeName);
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "DVBSky: remote control listener thread exception");
        return;
      }
      this.LogDebug("DVBSky: remote control listener thread stop polling");
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 75;
      }
    }

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "DVBSky";
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
      this.LogDebug("DVBSky: initialising");

      if (_isDvbSky)
      {
        this.LogWarn("DVBSky: extension already initialised");
        return true;
      }

      IBaseFilter filter = context as IBaseFilter;
      if (filter == null)
      {
        this.LogDebug("DVBSky: context is not a filter");
        return false;
      }

      IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
      _propertySet = pin as IKsPropertySet;
      if (_propertySet == null)
      {
        this.LogDebug("DVBSky: pin is not a property set");
        Release.ComObject("DVBSky filter input pin", ref pin);
        return false;
      }

      _diseqcInterface = new Conexant.Conexant(BDA_EXTENSION_PROPERTY_SET_GENERAL);
      if (!_diseqcInterface.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context))
      {
        this.LogDebug("DVBSky: base Conexant interface not supported");
        Release.ComObject("DVBSky filter input pin", ref pin);
        _propertySet = null;
        IDisposable d = _diseqcInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _diseqcInterface = null;
        return false;
      }

      this.LogInfo("DVBSky: extension supported");
      _isDvbSky = true;
      _instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      ReadMacAddresses();

      // This should succeed even if the product does not have a CI.
      _caProviderInterface = new NetUp.NetUp(BDA_EXTENSION_PROPERTY_SET_CA);
      if (!_caProviderInterface.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context))
      {
        this.LogWarn("DVBSky: failed to initialise base NetUP interface");
        IDisposable d = _caProviderInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _caProviderInterface = null;
      }
      _caMenuActionsInterface = _caProviderInterface as IConditionalAccessMenuActions;

      if (tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbC2) || tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT2))
      {
        // Check for DVB-X2 PLP selection support.
        KSPropertySupport support;
        int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_GENERAL, (int)BdaExtensionProperty.DvbPlpId, out support);
        if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Get) && support.HasFlag(KSPropertySupport.Set))
        {
          this.LogDebug("DVBSky: stream selection supported");
          _plpBuffer = Marshal.AllocCoTaskMem(PLP_COMMAND_SIZE);
        }
        else
        {
          this.LogDebug("DVBSky: stream selection property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        }
      }

      return true;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      if (_caProviderInterface != null)
      {
        return _caProviderInterface.Open();
      }
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Close()
    {
      return CloseConditionalAccessInterface(true);
    }

    private bool CloseConditionalAccessInterface(bool isDisposing)
    {
      if (isDisposing && _caProviderInterface != null)
      {
        return _caProviderInterface.Close();
      }
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      if (_caProviderInterface != null)
      {
        return _caProviderInterface.Reset();
      }
      return false;
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      if (_caProviderInterface != null)
      {
        return _caProviderInterface.IsReady();
      }
      return false;
    }

    /// <summary>
    /// Determine whether the conditional access interface requires access to
    /// the MPEG 2 conditional access table in order to successfully decrypt
    /// programs.
    /// </summary>
    /// <returns><c>true</c> if access to the MPEG 2 conditional access table is required in order to successfully decrypt programs, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsConditionalAccessTableRequiredForDecryption()
    {
      return false;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more programs
    ///   simultaneously. This parameter gives the interface an indication of the number of programs that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program's map table.</param>
    /// <param name="cat">The conditional access table for the program's transport stream.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.SendCommand(CaPmtListManagementAction listAction, CaPmtCommand command, TableProgramMap pmt, TableConditionalAccess cat, string programProvider)
    {
      if (_caProviderInterface != null)
      {
        return _caProviderInterface.SendCommand(listAction, command, pmt, cat, programProvider);
      }
      return false;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
    {
      if (_caProviderInterface != null)
      {
        _caMenuActionsInterface.SetCallBack(callBack);
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      if (_caProviderInterface != null)
      {
        return _caMenuActionsInterface.Enter();
      }
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      if (_caProviderInterface != null)
      {
        return _caMenuActionsInterface.Close();
      }
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      if (_caProviderInterface != null)
      {
        return _caMenuActionsInterface.SelectEntry(choice);
      }
      return false;
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
    {
      if (_caProviderInterface != null)
      {
        return _caMenuActionsInterface.AnswerEnquiry(cancel, answer);
      }
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
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.SendCommand(command);
      }
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.SendCommand(command);
      }
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      if (_diseqcInterface != null)
      {
        // Set by tune request LNB frequency parameters.
        return _diseqcInterface.SetToneState(state);
      }
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
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.ReadResponse(out response);
      }
      response = null;
      return false;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("DVBSky: open remote control interface");

      if (!_isDvbSky)
      {
        this.LogWarn("DVBSky: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("DVBSky: conditional access interface is already open");
        return true;
      }

      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_GENERAL, (int)BdaExtensionProperty.RemoteCode, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Get))
      {
        this.LogDebug("DVBSky: property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        return false;
      }

      _remoteControlBuffer = Marshal.AllocCoTaskMem(REMOTE_CONTROL_DATA_SIZE);
      _isRemoteControlInterfaceOpen = true;
      StartRemoteControlListenerThread();

      this.LogDebug("DVBSky: result = success");
      return true;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Close()
    {
      return CloseRemoteControlListenerInterface(true);
    }

    private bool CloseRemoteControlListenerInterface(bool isDisposing)
    {
      this.LogDebug("DVBSky: close remote control interface");

      if (isDisposing)
      {
        StopRemoteControlListenerThread();
      }

      if (_remoteControlBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_remoteControlBuffer);
        _remoteControlBuffer = IntPtr.Zero;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("DVBSky: result = success");
      return true;
    }

    #endregion

    #region IStreamSelector members

    /// <summary>
    /// Get the identifiers for the available streams.
    /// </summary>
    /// <param name="streamIds">The stream identifiers.</param>
    /// <returns><c>true</c> if the stream identifiers are retrieved successfully, otherwise <c>false</c></returns>
    public bool GetAvailableStreamIds(out ICollection<int> streamIds)
    {
      this.LogDebug("DVBSky: get available stream IDs");
      streamIds = null;

      if (!_isDvbSky || _plpBuffer == IntPtr.Zero)
      {
        this.LogWarn("DVBSky: not initialised or interface not supported");
        return false;
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET_GENERAL, (int)BdaExtensionProperty.DvbPlpId,
        _instanceBuffer, INSTANCE_SIZE,
        _plpBuffer, PLP_COMMAND_SIZE,
        out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == PLP_COMMAND_SIZE)
      {
        this.LogDebug("DVBSky: result = success");
        PlpCommand command = (PlpCommand)Marshal.PtrToStructure(_plpBuffer, typeof(PlpCommand));
        streamIds = new List<int>(command.PlpCount);
        for (int i = 0; i < command.PlpCount; i++)
        {
          streamIds.Add(command.PlpIds[i]);
        }
        return true;
      }

      this.LogError("DVBSky: failed to get available stream IDs, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      return false;
    }

    /// <summary>
    /// Select a stream.
    /// </summary>
    /// <param name="streamId">The identifier of the stream to select.</param>
    /// <returns><c>true</c> if the stream is selected successfully, otherwise <c>false</c></returns>
    public bool SelectStream(int streamId)
    {
      this.LogDebug("DVBSky: select stream, stream ID = {0}", streamId);

      if (!_isDvbSky || _plpBuffer == IntPtr.Zero)
      {
        this.LogWarn("DVBSky: not initialised or interface not supported");
        return false;
      }

      PlpCommand command = new PlpCommand();
      command.PlpId = streamId;
      Marshal.StructureToPtr(command, _plpBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET_GENERAL, (int)BdaExtensionProperty.DvbPlpId, _instanceBuffer, INSTANCE_SIZE, _plpBuffer, PLP_COMMAND_SIZE);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("DVBSky: result = success");
        return true;
      }

      this.LogError("DVBSky: failed to select stream, hr = 0x{0:x}, stream ID = {1}", hr, streamId);
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

    ~DvbSky()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isDvbSky)
      {
        CloseConditionalAccessInterface(isDisposing);
        CloseRemoteControlListenerInterface(isDisposing);
      }
      if (isDisposing)
      {
        Release.ComObject("DVBSky property set", ref _propertySet);

        IDisposable d = _diseqcInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _diseqcInterface = null;

        d = _caProviderInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _caProviderInterface = null;
      }
      if (_plpBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_plpBuffer);
        _plpBuffer = IntPtr.Zero;
      }
      if (_instanceBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_instanceBuffer);
        _instanceBuffer = IntPtr.Zero;
      }
      _isDvbSky = false;
    }

    #endregion
  }
}