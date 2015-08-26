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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC, remote controls and
  /// stream selection for Turbosight tuners.
  /// </summary>
  /// <remarks>
  /// Note that some Turbosight drivers seem to still support the original
  /// Conexant, NXP and Cyprus interfaces/structures. However, it is simpler
  /// and definitely more future-proof to stick with the information in the
  /// published SDK.
  /// </remarks>
  public class Turbosight : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, IDiseqcDevice, IDisposable, IPowerDevice, IRemoteControlListener, IStreamSelector
  {
    #region enums

    // PCIe/PCI only.
    private enum BdaExtensionProperty
    {
      Reserved = 0,
      NbcParams = 10,     // Property for setting DVB-S2 parameters that could not initially be set through BDA interfaces. Only supported by some older tuners.
      BlindScan = 11,     // Property for accessing and controlling the hardware blind scan capabilities.
      InputMultiStreamId = 14,  // Property for setting the DVB-S2 input stream identifier. Only supported by TBS6925 and TBS6983.
      CiAccess = 18,      // Property for interacting with the CI slot.
      TbsAccess = 21,     // Property for enabling control of the common properties in the BdaExtensionCommand enum.
      PlpInfo = 22        // Property for setting the DVB-*2 input stream or physical layer pipe identifier.
    }

    // USB (QBOX) only.
    private enum BdaExtensionPropertyUsb
    {
      Reserved = 0,
      Ir = 1,             // Property for retrieving IR codes from the IR receiver.
      CiAccess = 8,       // Property for interacting with the CI slot.
      BlindScan = 9,      // Property for accessing and controlling the hardware blind scan capabilities.
      InputMultiStreamId = 16,  // Property for setting the DVB-S2 input stream identifier. Only supported by TBS5925.
      TbsAccess = 18,     // Property for enabling control of the common properties in the TbsAccessMode enum.
      PlpInfo = 19        // Property for setting the DVB-*2 input stream or physical layer pipe identifier.
    }

    // Common properties that can be controlled on all TBS products.
    private enum TbsAccessMode : uint
    {
      LnbPower = 0,       // Control the LNB power supply.
      Diseqc,             // Send and receive DiSEqC messages.
      Tone                // Control the 22 kHz oscillator state.
    }

    private enum TbsLnbPower : uint
    {
      Off = 0,
      High,               // 18 V - linear horizontal, circular left.
      Low,                // 13 V - linear vertical, circular right.
      On                  // Power on using the previous voltage.
    }

    private enum TbsTone : uint
    {
      Off = 0,
      On,                 // Continuous tone on.
      BurstUnmodulated,   // Simple DiSEqC port A (tone burst).
      BurstModulated      // Simple DiSEqC port B (data burst).
    }

    private enum TbsPilotTonesState : uint
    {
      Off = 0,
      On,
      Unknown               // (Not used...)
    }

    private enum TbsRollOffFactor : uint
    {
      Undefined = 0xff,
      Twenty = 0,           // 0.2
      TwentyFive,           // 0.25
      ThirtyFive            // 0.35
    }

    private enum TbsDvbsStandard : uint
    {
      Auto = 0,
      Dvbs,
      Dvbs2
    }

    private enum TbsMmiMessageType : byte
    {
      Null = 0,
      ApplicationInfo = 0x01,     // PC <-->
      CaInfo = 0x02,              // PC <-->
      //CaPmt = 0x03,               // PC -->
      //CaPmtReply = 0x04,          // PC <--
      DateTimeEnquiry = 0x05,     // PC <--
      //DateTime = 0x06,            // PC -->
      Enquiry = 0x07,             // PC <--
      Answer = 0x08,              // PC -->
      EnterMenu = 0x09,           // PC -->
      Menu = 0x0a,                // PC <--
      MenuAnswer = 0x0b,          // PC -->
      List = 0x0c,                // PC <--
      GetMmi = 0x0d,              // PC <--
      CloseMmi = 0x0e,            // PC -->
      //DateTimeMode = 0x10,        // PC -->
      //SetDateTime = 0x12          // PC <--
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TbsAccessParams
    {
      public TbsAccessMode AccessMode;
      public TbsTone Tone;
      private uint Reserved1;
      public TbsLnbPower LnbPower;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] DiseqcTransmitMessage;
      public uint DiseqcTransmitMessageLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DISEQC_MESSAGE_LENGTH)]
      public byte[] DiseqcReceiveMessage;
      public uint DiseqcReceiveMessageLength;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      private byte[] Reserved2;
    }

    // Used to improve tuning speeds for older Conexant-based tuners.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NbcTuningParams  // BDA_NBC_PARAMS
    {
      public TbsRollOffFactor RollOffFactor;
      public TbsPilotTonesState PilotTonesState;
      public TbsDvbsStandard DvbsStandard;
      public BinaryConvolutionCodeRate FecCodeRate;
      public ModulationType ModulationType;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlpInfo    // PLP_INFO
    {
      public byte PlpId;
      public byte PlpCount;
      private byte Reserved1;
      private byte Reserved2;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] PlpIdList;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MediaInfo  // GETMEDIAINFO
    {
      private int Private1;
      private int Private2;
      private int Private3;
      private int Private4;
      private int Private5;
      private int Private6;
      private int Private7;
      private int Private8;
      private int Private9;
      private int Private10;
      private int Private11;
      private int Private12;
      public int StreamIdentifier;
      private int Private14;
      private int Private15;
      private int Private16;
      private int Private17;
      private int Private18;
    }

    // MP internal message holder - purely for convenience.
    private struct MmiMessage
    {
      public TbsMmiMessageType Type;
      public byte[] Message;

      public MmiMessage(TbsMmiMessageType type, int length)
      {
        Type = type;
        Message = new byte[length];
      }

      public MmiMessage(TbsMmiMessageType type)
      {
        Type = type;
        Message = null;
      }
    }

    #endregion

    #region delegates

    /// <summary>
    /// Open the conditional access interface for a specific Turbosight device.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="deviceName">The corresponding <see cref="DsDevice"/> name.</param>
    /// <param name="deviceIndex">A unique index for the device. This index enables CI support for multiple instances of a product.</param>
    /// <returns>a handle that the DLL can use to identify this device for future function calls</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private delegate IntPtr On_Start_CI(IBaseFilter tunerFilter, [MarshalAs(UnmanagedType.LPWStr)] string deviceName, int deviceIndex);

    /// <summary>
    /// Check whether a CAM is present in the CI slot associated with a specific Turbosight device.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool Camavailable(IntPtr handle);

    /// <summary>
    /// Exchange MMI messages with the CAM.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    /// <param name="command">The MMI command.</param>
    /// <param name="response">The MMI response.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void TBS_ci_MMI_Process(IntPtr handle, IntPtr command, IntPtr response);

    /// <summary>
    /// Send PMT to the CAM.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    /// <param name="pmt">The PMT command.</param>
    /// <param name="pmtLength">The length of the PMT.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void TBS_ci_SendPmt(IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] byte[] pmt, ushort pmtLength);

    /// <summary>
    /// Close the conditional access interface for a specific Turbosight device.
    /// </summary>
    /// <param name="handle">The handle allocated to this device.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void On_Exit_CI(IntPtr handle);

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xfaa8f3e5, 0x31d4, 0x4e41, 0x88, 0xef, 0xd9, 0xeb, 0x71, 0x6f, 0x6e, 0xc9);
    private static readonly Guid BDA_EXTENSION_PROPERTY_SET_USB = new Guid(0xc6efe5eb, 0x855a, 0x4f1b, 0xb7, 0xaa, 0x87, 0xb5, 0xe1, 0xdc, 0x41, 0x13);

    private static readonly int NBC_TUNING_PARAMS_SIZE = Marshal.SizeOf(typeof(NbcTuningParams));   // 20
    private static readonly int MEDIA_INFO_SIZE = Marshal.SizeOf(typeof(MediaInfo));                // 72
    private static readonly int TBS_ACCESS_PARAMS_SIZE = Marshal.SizeOf(typeof(TbsAccessParams));   // 536
    private static readonly int PLP_INFO_SIZE = Marshal.SizeOf(typeof(PlpInfo));                    // 260
    private const int MAX_DISEQC_MESSAGE_LENGTH = 128;

    private const int MMI_MESSAGE_BUFFER_SIZE = 512;
    private const int MMI_RESPONSE_BUFFER_SIZE = 2048;

    private static readonly int GENERAL_BUFFER_SIZE = new int[] { NBC_TUNING_PARAMS_SIZE, MEDIA_INFO_SIZE, TBS_ACCESS_PARAMS_SIZE, PLP_INFO_SIZE }.Max();

    private const int MMI_HANDLER_THREAD_WAIT_TIME = 2000;    // unit = ms

    #endregion

    #region variables

    // This variable tracks the number of open CI API instances which corresponds with used DLL indices.
    private static int _ciApiCount = 0;

    // Conditional access API instance variables.
    private int _ciApiIndex = 0;
    private bool _dllLoaded = false;
    private IntPtr _ciApiLibHandle = IntPtr.Zero;
    private IntPtr _ciHandle = IntPtr.Zero;

    // Delegate instances for each CI API function.
    private On_Start_CI _onStartCi = null;
    private Camavailable _camAvailable = null;
    private TBS_ci_MMI_Process _mmiProcess = null;
    private TBS_ci_SendPmt _sendPmt = null;
    private On_Exit_CI _onExitCi = null;

    // Buffers for use in conditional access related functions.
    private IntPtr _mmiMessageBuffer = IntPtr.Zero;
    private IntPtr _mmiResponseBuffer = IntPtr.Zero;

    // A buffer for general use in synchronised methods.
    private IntPtr _generalBuffer = IntPtr.Zero;

    private IBaseFilter _tunerFilter = null;
    private string _tunerFilterName = null;
    private string _tunerProductInstanceId = null;

    private Guid _propertySetGuid = Guid.Empty;
    private IKsPropertySet _propertySet = null;
    private int _propertyIdInputStreamId = -1;
    private int _propertyIdTbsAccess = -1;
    private int _propertyIdPlpInfo = -1;

    private bool _isTurbosight = false;
    private bool _isCaInterfaceOpen = false;
    private bool _isUsb = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isNbcParamPropertySupported = false;

    private Thread _mmiHandlerThread = null;
    private AutoResetEvent _mmiHandlerThreadStopEvent = null;
    private object _mmiLock = new object();
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();

    // This is a first-in-first-out queue of messages that are ready to be passed to the CAM.
    private List<MmiMessage> _mmiMessageQueue = null;

    private bool _isRemoteControlInterfaceOpen = false;
    private ITurbosightRemoteControl _remoteControlInterface = null;

    #endregion

    /// <summary>
    /// Load a conditional access API instance. This involves obtaining delegate instances for
    /// each of the member functions.
    /// </summary>
    /// <returns><c>true</c> if the instance is successfully loaded, otherwise <c>false</c></returns>
    private bool LoadNewCaApiInstance()
    {
      // Load a new DLL. DLLs should not be reused to avoid issues when resetting interfaces and
      // enable support for multiple tuners with CI slots.
      _ciApiCount++;
      _ciApiIndex = _ciApiCount;
      this.LogDebug("Turbosight: loading API, API index = {0}", _ciApiIndex);
      string resourcesFolder = PathManager.BuildAssemblyRelativePath("Resources");
      string fileNameSource = Path.Combine(resourcesFolder, "tbsCIapi.dll");
      string fileNameTarget = Path.Combine(resourcesFolder, "tbsCIapi" + _ciApiIndex + ".dll");
      if (!File.Exists(fileNameTarget))
      {
        try
        {
          File.Copy(fileNameSource, fileNameTarget);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Turbosight: failed to copy TBS CI API DLL");
          return false;
        }
      }
      _ciApiLibHandle = NativeMethods.LoadLibrary(fileNameTarget);
      if (_ciApiLibHandle == IntPtr.Zero)
      {
        this.LogError("Turbosight: failed to load TBS CI API DLL");
        return false;
      }

      try
      {
        IntPtr function = NativeMethods.GetProcAddress(_ciApiLibHandle, "On_Start_CI");
        if (function == IntPtr.Zero)
        {
          this.LogError("Turbosight: failed to locate the On_Start_CI function");
          return false;
        }
        try
        {
          _onStartCi = (On_Start_CI)Marshal.GetDelegateForFunctionPointer(function, typeof(On_Start_CI));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Turbosight: failed to load the On_Start_CI function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_ciApiLibHandle, "Camavailable");
        if (function == IntPtr.Zero)
        {
          this.LogError("Turbosight: failed to locate the Camavailable function");
          return false;
        }
        try
        {
          _camAvailable = (Camavailable)Marshal.GetDelegateForFunctionPointer(function, typeof(Camavailable));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Turbosight: failed to load the Camavailable function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_ciApiLibHandle, "TBS_ci_MMI_Process");
        if (function == IntPtr.Zero)
        {
          this.LogError("Turbosight: failed to locate the TBS_ci_MMI_Process function");
          return false;
        }
        try
        {
          _mmiProcess = (TBS_ci_MMI_Process)Marshal.GetDelegateForFunctionPointer(function, typeof(TBS_ci_MMI_Process));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Turbosight: failed to load the TBS_ci_MMI_Process function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_ciApiLibHandle, "TBS_ci_SendPmt");
        if (function == IntPtr.Zero)
        {
          this.LogError("Turbosight: failed to locate the TBS_ci_SendPmt function");
          return false;
        }
        try
        {
          _sendPmt = (TBS_ci_SendPmt)Marshal.GetDelegateForFunctionPointer(function, typeof(TBS_ci_SendPmt));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Turbosight: failed to load the TBS_ci_SendPmt function");
          return false;
        }

        function = NativeMethods.GetProcAddress(_ciApiLibHandle, "On_Exit_CI");
        if (function == IntPtr.Zero)
        {
          this.LogError("Turbosight: failed to locate the On_Exit_CI function");
          return false;
        }
        try
        {
          _onExitCi = (On_Exit_CI)Marshal.GetDelegateForFunctionPointer(function, typeof(On_Exit_CI));
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Turbosight: failed to load the On_Exit_CI function");
          return false;
        }

        _dllLoaded = true;
        return true;
      }
      finally
      {
        if (!_dllLoaded)
        {
          NativeMethods.FreeLibrary(_ciApiLibHandle);
          _ciApiLibHandle = IntPtr.Zero;
        }
      }
    }

    /// <summary>
    /// Determine the product instance identifier for the tuner.
    /// </summary>
    /// <param name="tunerExternalId">The tuner's external identifier.</param>
    /// <returns>the tuner's product instance identifier</returns>
    private string GetProductInstanceId(string tunerExternalId)
    {
      string productInstanceId = null;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice device in devices)
      {
        if (productInstanceId == null)
        {
          string devicePath = device.DevicePath;
          if (devicePath != null && tunerExternalId.Contains(devicePath))
          {
            productInstanceId = device.ProductInstanceIdentifier;
          }
        }
        device.Dispose();
      }
      return productInstanceId;
    }

    #region MMI handler thread

    /// <summary>
    /// Start a thread to exchange MMI messages with the CAM.
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isCaInterfaceOpen)
      {
        return;
      }

      lock (_mmiLock)
      {
        // Kill the existing thread if it is in "zombie" state.
        if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
        {
          StopMmiHandlerThread();
        }

        if (_mmiHandlerThread == null)
        {
          this.LogDebug("Turbosight: starting new MMI handler thread");
          // Clear the message queue and buffers in preparation for [first] use.
          _mmiMessageQueue.Clear();
          for (int i = 0; i < MMI_MESSAGE_BUFFER_SIZE; i++)
          {
            Marshal.WriteByte(_mmiMessageBuffer, i, 0);
          }
          for (int i = 0; i < MMI_RESPONSE_BUFFER_SIZE; i++)
          {
            Marshal.WriteByte(_mmiResponseBuffer, i, 0);
          }
          _mmiHandlerThreadStopEvent = new AutoResetEvent(false);
          _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
          _mmiHandlerThread.Name = "Turbosight MMI handler";
          _mmiHandlerThread.IsBackground = true;
          _mmiHandlerThread.Priority = ThreadPriority.Lowest;
          _mmiHandlerThread.Start();
        }
      }
    }

    /// <summary>
    /// Stop the thread that exchanges MMI messages with the CAM.
    /// </summary>
    private void StopMmiHandlerThread()
    {
      lock (_mmiLock)
      {
        if (_mmiHandlerThread != null)
        {
          if (!_mmiHandlerThread.IsAlive)
          {
            this.LogWarn("Turbosight: aborting old MMI handler thread");
            _mmiHandlerThread.Abort();
          }
          else
          {
            _mmiHandlerThreadStopEvent.Set();
            if (!_mmiHandlerThread.Join(MMI_HANDLER_THREAD_WAIT_TIME * 2))
            {
              this.LogWarn("Turbosight: failed to join MMI handler thread, aborting thread");
              _mmiHandlerThread.Abort();
            }
          }
          _mmiHandlerThread = null;
          if (_mmiHandlerThreadStopEvent != null)
          {
            _mmiHandlerThreadStopEvent.Close();
            _mmiHandlerThreadStopEvent = null;
          }
        }
      }
    }

    /// <summary>
    /// Thread function for exchanging MMI messages with the CAM.
    /// </summary>
    private void MmiHandler()
    {
      this.LogDebug("Turbosight: MMI handler thread start polling");
      TbsMmiMessageType message = TbsMmiMessageType.Null;
      ushort sendCount = 0;
      try
      {
        while (!_mmiHandlerThreadStopEvent.WaitOne(MMI_HANDLER_THREAD_WAIT_TIME))
        {
          // Check for CAM state changes.
          bool newState = _camAvailable(_ciHandle);
          if (newState != _isCamPresent)
          {
            _isCamPresent = newState;
            this.LogInfo("Turbosight: CI state change, CAM present = {0}", _isCamPresent);
            // If a CAM has just been inserted then clear the message queue - we consider
            // any old messages as invalid now.
            if (_isCamPresent)
            {
              lock (_mmiLock)
              {
                _mmiMessageQueue.Clear();
              }
              message = TbsMmiMessageType.Null;
            }
          }

          // If there is no CAM then we can't send or receive messages.
          if (!_isCamPresent)
          {
            continue;
          }

          // Are we still trying to get a response?
          if (message == TbsMmiMessageType.Null)
          {
            // No -> do we have a message to send?
            lock (_mmiLock)
            {
              // Yes -> load it into the message buffer.
              if (_mmiMessageQueue.Count > 0)
              {
                message = _mmiMessageQueue[0].Type;
                this.LogDebug("Turbosight: sending message {0}", message);
                Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)message);
                if (_mmiMessageQueue[0].Message != null && _mmiMessageQueue[0].Message.Length > 0)
                {
                  Marshal.Copy(_mmiMessageQueue[0].Message, 0, IntPtr.Add(_mmiMessageBuffer, 1), _mmiMessageQueue[0].Message.Length);
                }
                sendCount = 0;
              }
              // No -> poll for unrequested messages from the CAM.
              else
              {
                Marshal.WriteByte(_mmiMessageBuffer, 0, (byte)TbsMmiMessageType.GetMmi);
              }
            }
          }

          // Send/resend the message.
          _mmiProcess(_ciHandle, _mmiMessageBuffer, _mmiResponseBuffer);

          bool removeMessage = false;
          try
          {
            // Do we expect a response to this message?
            if (message == TbsMmiMessageType.EnterMenu || message == TbsMmiMessageType.MenuAnswer || message == TbsMmiMessageType.Answer || message == TbsMmiMessageType.CloseMmi)
            {
              // No -> remove this message from the queue and move on.
              removeMessage = true;
              continue;
            }

            // Yes, we expect a response -> check for a response.
            TbsMmiMessageType response = (TbsMmiMessageType)Marshal.ReadByte(_mmiResponseBuffer, 4);
            if (response == TbsMmiMessageType.Null)
            {
              // If we are waiting for a response to a message that we sent
              // directly and we haven't received a response after 10 requests
              // then give up and move on.
              if (message != TbsMmiMessageType.Null)
              {
                sendCount++;
                if (sendCount >= 10)
                {
                  this.LogWarn("Turbosight: giving up on message {0}", message);
                  removeMessage = true;
                }
              }
              continue;
            }

            this.LogInfo("Turbosight: received MMI response {0} to message {1}", response, message);
            #region response handling

            // Get the response bytes.
            ushort length = (ushort)Marshal.ReadInt16(_mmiResponseBuffer, 5);
            if (length > MMI_RESPONSE_BUFFER_SIZE - 7)
            {
              this.LogDebug("Turbosight: response too long, length = {0}", length);
              // We know we haven't got the complete response (DLL internal buffer overflow),
              // so wipe the message and response buffers and give up on this message.
              for (int i = 0; i < MMI_RESPONSE_BUFFER_SIZE; i++)
              {
                Marshal.WriteByte(_mmiResponseBuffer, i, 0);
              }
              // If we requested this response directly then remove the request
              // message from the queue.
              removeMessage = (message != TbsMmiMessageType.Null);
              continue;
            }

            this.LogDebug("Turbosight: response length = {0}", length);
            //Dump.DumpBinary(_mmiResponseBuffer, length + 7);
            byte[] responseBytes = new byte[length];
            Marshal.Copy(IntPtr.Add(_mmiResponseBuffer, 7), responseBytes, 0, length);

            bool success = false;
            if (response == TbsMmiMessageType.ApplicationInfo)
            {
              success = HandleApplicationInformation(responseBytes);
            }
            else if (response == TbsMmiMessageType.CaInfo)
            {
              success = HandleCaInformation(responseBytes);
            }
            else if (response == TbsMmiMessageType.Menu || response == TbsMmiMessageType.List)
            {
              success = HandleMenu(responseBytes);
            }
            else if (response == TbsMmiMessageType.Enquiry)
            {
              success = HandleEnquiry(responseBytes);
            }
            else
            {
              this.LogWarn("Turbosight: unhandled response message {0}", response);
            }
            if (!success)
            {
              Dump.DumpBinary(_mmiResponseBuffer, MMI_RESPONSE_BUFFER_SIZE);
            }

            // A message has been handled and now we move on to handling the
            // next message or revert to polling for messages from the CAM.
            for (int i = 0; i < MMI_RESPONSE_BUFFER_SIZE; i++)
            {
              Marshal.WriteByte(_mmiResponseBuffer, i, 0);
            }
            // If we requested this response directly then remove the request
            // message from the queue.
            removeMessage = (message != TbsMmiMessageType.Null);
            #endregion
          }
          finally
          {
            if (removeMessage)
            {
              lock (_mmiLock)
              {
                _mmiMessageQueue.RemoveAt(0);
                message = TbsMmiMessageType.Null;
                if (_mmiMessageQueue.Count == 0)
                {
                  this.LogDebug("Turbosight: resuming polling...");
                }
              }
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Turbosight: MMI handler thread exception");
        return;
      }
      this.LogDebug("Turbosight: MMI handler thread stop polling");
    }

    private bool HandleApplicationInformation(byte[] content)
    {
      this.LogDebug("Turbosight: application information");
      int length = content.Length;
      if (length < 5)
      {
        this.LogError("Turbosight: application information response too short, length = {0}", length);
        return false;
      }
      MmiApplicationType type = (MmiApplicationType)content[0];
      this.LogDebug("  type         = {0}", type);
      this.LogDebug("  manufacturer = 0x{0:x2}{1:x2}", content[1], content[2]);
      this.LogDebug("  code         = 0x{0:x2}{1:x2}", content[3], content[4]);
      this.LogDebug("  menu title   = {0}", DvbTextConverter.Convert(content, length - 5, 5));
      return true;
    }

    private bool HandleCaInformation(byte[] content)
    {
      this.LogDebug("Turbosight: conditional access information");
      int length = content.Length;
      if (length == 0)
      {
        this.LogError("Turbosight: conditional access information response too short");
        return false;
      }
      int numCasIds = content[0];
      this.LogDebug("  # CAS IDs = {0}", numCasIds);
      int i = 1;
      int l = 1;
      while (l + 2 <= length)
      {
        this.LogDebug("    {0, -7} = 0x{1:x2}{2:x2}", i, content[l + 1], content[l]);
        l += 2;
        i++;
      }
      if (length != ((numCasIds * 2) + 1))
      {
        this.LogError("Turbosight: CAS ID count {0} does not match response length {1}", numCasIds, length);
        return false;
      }
      return true;
    }

    private bool HandleMenu(byte[] content)
    {
      this.LogDebug("Turbosight: menu");
      if (content.Length == 0)
      {
        this.LogError("Turbosight: menu response too short");
        return false;
      }
      int entryCount = content[0];
      int expectedStringCount = entryCount + 3;   // + 3 for the title, sub-title and footer

      // Read all the strings into a list. Strings are NULL terminated.
      List<string> strings = new List<string>(expectedStringCount);
      string s;
      int decodedByteCount;
      int totalDecodedByteCount = 1;
      while (totalDecodedByteCount != content.Length)
      {
        s = DvbTextConverter.Convert(content, -1, totalDecodedByteCount, out decodedByteCount);
        if (decodedByteCount == 0)
        {
          this.LogWarn("Turbosight: failed to decode menu string {0} of {1}", strings.Count, expectedStringCount);
          break;
        }
        totalDecodedByteCount += decodedByteCount;
        strings.Add(s);
      }

      if (expectedStringCount != strings.Count)
      {
        this.LogError("Turbosight: actual menu string count {0} does not match expected string count {1}", strings.Count, expectedStringCount);
        return false;
      }

      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack == null)
        {
          this.LogDebug("Turbosight: menu call back not set");
        }

        this.LogDebug("  title     = {0}", strings[0]);
        this.LogDebug("  sub-title = {0}", strings[1]);
        this.LogDebug("  footer    = {0}", strings[2]);
        this.LogDebug("  # entries = {0}", entryCount);
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiMenu(strings[0], strings[1], strings[2], entryCount);
        }
        for (int i = 0; i < entryCount; i++)
        {
          s = strings[i + 3];
          this.LogDebug("    {0, -7} = {1}", i + 1, s);
          if (_caMenuCallBack != null)
          {
            _caMenuCallBack.OnCiMenuChoice(i, s);
          }
        }
      }
      return true;
    }

    private bool HandleEnquiry(byte[] content)
    {
      this.LogDebug("Turbosight: enquiry");
      int length = content.Length;
      if (length < 3)
      {
        this.LogError("Turbosight: enquiry response too short, length = {0}", length);
        return false;
      }
      bool blind = (content[0] != 0);
      uint answerLength = content[1];
      string prompt = DvbTextConverter.Convert(content, length - 2, 2);
      this.LogDebug("  prompt = {0}", prompt);
      this.LogDebug("  length = {0}", answerLength);
      this.LogDebug("  blind  = {0}", blind);
      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiRequest(blind, answerLength, prompt);
        }
        else
        {
          this.LogDebug("Turbosight: menu call back not set");
        }
      }
      return true;
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
        return 70;
      }
    }

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        if (string.IsNullOrEmpty(_tunerFilterName))
        {
          return base.Name;
        }
        return _tunerFilterName;
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
      this.LogDebug("Turbosight: initialising");

      if (_isTurbosight)
      {
        this.LogWarn("Turbosight: extension already initialised");
        return true;
      }

      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskDigital) == 0)
      {
        this.LogDebug("Turbosight: tuner type not supported");
        return false;
      }

      IBaseFilter tunerFilter = context as IBaseFilter;
      if (tunerFilter == null)
      {
        this.LogDebug("Turbosight: context is not a filter");
        return false;
      }

      // Check the tuner filter name first. Other manufacturers that do not support these interfaces
      // use the same GUIDs which makes things a little tricky.
      FilterInfo tunerFilterInfo;
      int hr = tunerFilter.QueryFilterInfo(out tunerFilterInfo);
      _tunerFilterName = tunerFilterInfo.achName;
      Release.FilterInfo(ref tunerFilterInfo);
      if (hr != (int)NativeMethods.HResult.S_OK || _tunerFilterName == null)
      {
        this.LogError("Turbosight: failed to get the tuner filter name, hr = 0x{0:x}", hr);
        return false;
      }
      if (!_tunerFilterName.StartsWith("TBS") && !_tunerFilterName.StartsWith("QBOX"))
      {
        this.LogDebug("Turbosight: tuner filter name does not match");
        return false;
      }
      this.LogDebug("Turbosight: checking tuner named \"{0}\"", _tunerFilterName);

      // Now check for the USB interface first as per TBS SDK recommendations.
      KSPropertySupport support;
      _propertySet = tunerFilter as IKsPropertySet;
      if (_propertySet != null)
      {
        // For DVB-S/S2 tuners...
        hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_USB, (int)BdaExtensionPropertyUsb.TbsAccess, out support);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          // For DVB-C/T/T2 tuners with CI slot...
          hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_USB, (int)BdaExtensionPropertyUsb.CiAccess, out support);
        }
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          // For DVB-T2 tuners without CI slot...
          hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET_USB, (int)BdaExtensionPropertyUsb.PlpInfo, out support);
        }
        if (hr == (int)NativeMethods.HResult.S_OK && support != 0)
        {
          // Okay, we've got a USB tuner here.
          this.LogInfo("Turbosight: extension supported, USB interface");
          _isTurbosight = true;
          _isUsb = true;
          _propertySetGuid = BDA_EXTENSION_PROPERTY_SET_USB;
          _propertyIdInputStreamId = (int)BdaExtensionPropertyUsb.InputMultiStreamId;
          _propertyIdTbsAccess = (int)BdaExtensionPropertyUsb.TbsAccess;
          _propertyIdPlpInfo = (int)BdaExtensionPropertyUsb.PlpInfo;
        }
      }

      // If the tuner doesn't support the USB interface then check for the PCIe/PCI interface.
      if (!_isTurbosight)
      {
        IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
        try
        {
          _propertySet = pin as IKsPropertySet;
          if (_propertySet != null)
          {
            // For DVB-S/S2 tuners...
            hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.TbsAccess, out support);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              // For DVB-C/T/T2 tuners with CI slot...
              hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.CiAccess, out support);
            }
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              // For DVB-T2 tuners without CI slot...
              hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PlpInfo, out support);
            }
            if (hr == (int)NativeMethods.HResult.S_OK && support != 0)
            {
              // Okay, we've got a PCIe or PCI tuner here.
              this.LogInfo("Turbosight: extension supported, PCIe/PCI interface");
              _isTurbosight = true;
              _isUsb = false;
              _propertySetGuid = BDA_EXTENSION_PROPERTY_SET;
              _propertyIdInputStreamId = (int)BdaExtensionProperty.InputMultiStreamId;
              _propertyIdTbsAccess = (int)BdaExtensionProperty.TbsAccess;
              _propertyIdPlpInfo = (int)BdaExtensionProperty.PlpInfo;
            }
          }
        }
        finally
        {
          if (!_isTurbosight)
          {
            Release.ComObject("Turbosight tuner filter input pin", ref pin);
          }
        }
      }

      if (!_isTurbosight)
      {
        this.LogDebug("Turbosight: property sets not supported");
        return false;
      }

      _tunerFilter = tunerFilter;
      _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
      if (tunerExternalId != null)
      {
        _tunerProductInstanceId = GetProductInstanceId(tunerExternalId);
      }

      // This property is probably only supported by older Conexant-based products.
      hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.NbcParams, out support);
      if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Turbosight: NBC tuning parameters property supported");
        _isNbcParamPropertySupported = true;
      }
      else
      {
        this.LogDebug("Turbosight: NBC tuning parameters property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        _isNbcParamPropertySupported = false;
      }

      // This property should be supported by professional DVB-S2, DVB-T2 and future DVB-C2 products.
      hr = _propertySet.QuerySupported(_propertySetGuid, _propertyIdPlpInfo, out support);
      if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Get) && support.HasFlag(KSPropertySupport.Set))
      {
        this.LogDebug("Turbosight: PLP stream selection supported");
        _propertyIdInputStreamId = -1;
      }
      else
      {
        this.LogDebug("Turbosight: PLP stream selection property not supported, hr = 0x{0:x}, support = {1}", hr, support);
        _propertyIdPlpInfo = -1;

        // Try the older MIS property. Only supported by "professional" DVB-S2 tuners (TBS6925, TBS6983, TBS5925).
        if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) != 0)
        {
          hr = _propertySet.QuerySupported(_propertySetGuid, _propertyIdInputStreamId, out support);
          if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Set))
          {
            this.LogDebug("Turbosight: IS stream selection supported");
          }
          else
          {
            this.LogDebug("Turbosight: IS stream selection property not supported, hr = 0x{0:x}, support = {1}", hr, support);
            _propertyIdInputStreamId = -1;
          }
        }
      }

      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Turbosight: on before tune call back");
      action = TunerAction.Default;

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return;
      }

      // Parameters only need to be tweaked and set for satellite tuning.
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      TbsDvbsStandard standard = TbsDvbsStandard.Auto;
      ModulationType bdaModulation = ModulationType.ModNotSet;
      if (channel is ChannelDvbS)
      {
        standard = TbsDvbsStandard.Dvbs;
        if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
        {
          bdaModulation = ModulationType.ModQpsk;
        }
        else
        {
          this.LogWarn("Turbosight: DVB-S tune request uses unsupported modulation scheme {0}", satelliteChannel.ModulationScheme);
        }
      }
      else
      {
        ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
        if (dvbs2Channel == null)
        {
          // Default: tuning with "auto" is slower, so avoid it if possible.
          this.LogWarn("Turbosight: tune request for unsupported satellite standard");
          standard = TbsDvbsStandard.Auto;
        }
        else
        {
          standard = TbsDvbsStandard.Dvbs2;
          switch (satelliteChannel.ModulationScheme)
          {
            case ModulationSchemePsk.Psk4:
              bdaModulation = ModulationType.ModNbcQpsk;
              break;
            case ModulationSchemePsk.Psk8:
              bdaModulation = ModulationType.ModNbc8Psk;
              break;
            case ModulationSchemePsk.Psk16:
              bdaModulation = ModulationType.Mod16Apsk;
              break;
            case ModulationSchemePsk.Psk32:
              bdaModulation = ModulationType.Mod32Apsk;
              break;
            default:
              this.LogWarn("Turbosight: DVB-S2 tune request uses unsupported modulation scheme {0}", satelliteChannel.ModulationScheme);
              break;
          }
        }
      }

      if (bdaModulation != ModulationType.ModNotSet)
      {
        this.LogDebug("  modulation      = {0}", bdaModulation);
        satelliteChannel.ModulationScheme = (ModulationSchemePsk)bdaModulation;
      }

      if (!_isNbcParamPropertySupported)
      {
        return;
      }

      NbcTuningParams command = new NbcTuningParams();
      command.DvbsStandard = standard;
      command.ModulationType = bdaModulation;
      if (command.DvbsStandard != TbsDvbsStandard.Dvbs2)
      {
        command.PilotTonesState = TbsPilotTonesState.Off;
        command.RollOffFactor = TbsRollOffFactor.ThirtyFive;
      }
      else
      {
        ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
        if (dvbs2Channel != null)
        {
          if (dvbs2Channel.PilotTonesState == PilotTonesState.Off)
          {
            command.PilotTonesState = TbsPilotTonesState.Off;
          }
          else if (dvbs2Channel.PilotTonesState == PilotTonesState.On)
          {
            command.PilotTonesState = TbsPilotTonesState.On;
          }
          else
          {
            command.PilotTonesState = TbsPilotTonesState.Unknown;
          }

          switch (dvbs2Channel.RollOffFactor)
          {
            case RollOffFactor.Twenty:
              command.RollOffFactor = TbsRollOffFactor.Twenty;
              break;
            case RollOffFactor.TwentyFive:
              command.RollOffFactor = TbsRollOffFactor.TwentyFive;
              break;
            case RollOffFactor.ThirtyFive:
              command.RollOffFactor = TbsRollOffFactor.ThirtyFive;
              break;
            default:
              this.LogWarn("Turbosight: DVB-S2 tune request uses unsupported roll-off factor {0}", dvbs2Channel.RollOffFactor);
              command.RollOffFactor = TbsRollOffFactor.Undefined;
              break;
          }
        }
      }

      switch (satelliteChannel.FecCodeRate)
      {
        case FecCodeRate.Rate1_2:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate1_2;
          break;
        case FecCodeRate.Rate1_3:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate1_3;
          break;
        case FecCodeRate.Rate1_4:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate1_4;
          break;
        case FecCodeRate.Rate2_3:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate2_3;
          break;
        case FecCodeRate.Rate2_5:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate2_5;
          break;
        case FecCodeRate.Rate3_4:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate3_4;
          break;
        case FecCodeRate.Rate3_5:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate3_5;
          break;
        case FecCodeRate.Rate4_5:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate4_5;
          break;
        case FecCodeRate.Rate5_11:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate5_11;
          break;
        case FecCodeRate.Rate5_6:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate5_6;
          break;
        case FecCodeRate.Rate6_7:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate6_7;
          break;
        case FecCodeRate.Rate7_8:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate7_8;
          break;
        case FecCodeRate.Rate8_9:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate8_9;
          break;
        case FecCodeRate.Rate9_10:
          command.FecCodeRate = BinaryConvolutionCodeRate.Rate9_10;
          break;
        default:
          this.LogWarn("Turbosight: tune request uses unsupported FEC code rate {0}", satelliteChannel.FecCodeRate);
          command.FecCodeRate = BinaryConvolutionCodeRate.RateNotSet;
          command.DvbsStandard = TbsDvbsStandard.Auto;    // old demods can't auto-detect DVB-S2 FEC code rate
          break;
      }

      this.LogDebug("  standard        = {0}", command.DvbsStandard);
      this.LogDebug("  FEC code rate   = {0}", command.FecCodeRate);
      this.LogDebug("  pilot tones     = {0}", command.PilotTonesState);
      this.LogDebug("  roll-off factor = {0}", command.RollOffFactor);

      Marshal.StructureToPtr(command, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, NBC_TUNING_PARAMS_SIZE);

      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.NbcParams,
        _generalBuffer, NBC_TUNING_PARAMS_SIZE,
        _generalBuffer, NBC_TUNING_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Turbosight: result = success");
      }
      else
      {
        this.LogError("Turbosight: failed to set NBC tuning parameters, hr = 0x{0:x}", hr);
      }
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnStarted(ITuner tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("Turbosight: set power state, state = {0}", state);

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.LnbPower;
      if (state == PowerState.On)
      {
        accessParams.LnbPower = TbsLnbPower.On;
      }
      else
      {
        accessParams.LnbPower = TbsLnbPower.Off;
      }

      Marshal.StructureToPtr(accessParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, TBS_ACCESS_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, _propertyIdTbsAccess,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Turbosight: result = success");
        return true;
      }

      this.LogError("Turbosight: failed to set power state, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("Turbosight: open conditional access interface");

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("Turbosight: conditional access interface is already open");
        return true;
      }

      // Check whether a CI slot is present.
      _isCiSlotPresent = false;
      int ciAccessProperty = (int)BdaExtensionProperty.CiAccess;
      if (_isUsb)
      {
        ciAccessProperty = (int)BdaExtensionPropertyUsb.CiAccess;
      }
      KSPropertySupport support;
      int hr = _propertySet.QuerySupported(_propertySetGuid, ciAccessProperty, out support);
      if (hr != (int)NativeMethods.HResult.S_OK || support == 0)
      {
        this.LogDebug("Turbosight: tuner doesn't have a CI slot");
        return false;
      }
      _isCiSlotPresent = true;

      if (!LoadNewCaApiInstance())
      {
        return false;
      }

      // Attempt to initialise the interface.
      _ciHandle = _onStartCi(_tunerFilter, _tunerFilterName, _ciApiIndex);
      if (_ciHandle == IntPtr.Zero || _ciHandle.ToInt64() == -1 || _ciHandle.ToInt32() == -1)
      {
        this.LogWarn("Turbosight: interface handle is null");
        _isCiSlotPresent = false;
        return false;
      }

      _mmiMessageBuffer = Marshal.AllocCoTaskMem(MMI_MESSAGE_BUFFER_SIZE);
      _mmiResponseBuffer = Marshal.AllocCoTaskMem(MMI_RESPONSE_BUFFER_SIZE);
      _mmiMessageQueue = new List<MmiMessage>(10);
      _isCamPresent = _camAvailable(_ciHandle);
      this.LogDebug("Turbosight: CAM available = {0}", _isCamPresent);

      _isCaInterfaceOpen = true;
      StartMmiHandlerThread();

      this.LogDebug("Turbosight: result = success");
      return _isCiSlotPresent;
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
      this.LogDebug("Turbosight: close conditional access interface");

      if (isDisposing)
      {
        StopMmiHandlerThread();
      }

      if (_isCaInterfaceOpen)
      {
        if (_ciHandle != IntPtr.Zero)
        {
          _onExitCi(_ciHandle);
          _ciHandle = IntPtr.Zero;
        }
        else
        {
          this.LogWarn("Turbosight: conditional access interfaces is open but handle is null");
        }
      }

      if (_mmiMessageBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiMessageBuffer);
        _mmiMessageBuffer = IntPtr.Zero;
      }
      if (_mmiResponseBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiResponseBuffer);
        _mmiResponseBuffer = IntPtr.Zero;
      }
      if (_ciApiLibHandle != IntPtr.Zero)
      {
        NativeMethods.FreeLibrary(_ciApiLibHandle);
        _ciApiLibHandle = IntPtr.Zero;
      }

      if (isDisposing)
      {
        _mmiMessageQueue = null;
      }

      _dllLoaded = false;
      _isCiSlotPresent = false;
      _isCamPresent = false;
      _isCaInterfaceOpen = false;

      this.LogDebug("Turbosight: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      // TBS have confirmed that it is currently not possible to call On_Start_CI() multiple times on a
      // filter instance ***even if On_Exit_CI() is called***. The graph must be rebuilt to reset the CI.
      return (this as IConditionalAccessProvider).Close() && (this as IConditionalAccessProvider).Open();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("Turbosight: is conditional access interface ready");
      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }

      // The CAM state is updated by the MMI handler thread. We can only
      // determine whether a CAM is present or not.
      this.LogDebug("Turbosight: result = {0}", _isCamPresent);
      return _isCamPresent;
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
      this.LogDebug("Turbosight: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        this.LogError("Turbosight: failed to send conditional access command, the CAM is not present");
        return false;
      }
      if (pmt == null)
      {
        this.LogError("Turbosight: failed to send conditional access command, PMT not supplied");
        return true;
      }

      // TBS have a short header at the start of the PMT.
      ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
      byte[] pmtBytes = new byte[rawPmt.Count + 2];
      pmtBytes[0] = (byte)listAction;
      pmtBytes[1] = (byte)command;
      rawPmt.CopyTo(pmtBytes, 2);

      _sendPmt(_ciHandle, pmtBytes, (ushort)pmtBytes.Length);

      this.LogDebug("Turbosight: result = success");
      return true;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBack = callBack;
      }
      StartMmiHandlerThread();
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      this.LogDebug("Turbosight: enter menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("Turbosight: failed to enter menu, the CAM is not present");
        return false;
      }

      lock (_mmiLock)
      {
        // Close any existing sessions otherwise the CAM gets confused.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CloseMmi));
        // We send an "application info" message because attempting to enter the menu will fail
        // if you don't get the application information first.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.ApplicationInfo));
        // The CA information is just for information purposes.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CaInfo));
        // The main message.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.EnterMenu));
        // We have to request a response.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
      }
      return true;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("Turbosight: close menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("Turbosight: failed to close menu, the CAM is not present");
        return false;
      }

      lock (_mmiLock)
      {
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.CloseMmi));
      }
      return true;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("Turbosight: select menu entry, choice = {0}", choice);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("Turbosight: failed to select menu entry, the CAM is not present");
        return false;
      }

      lock (_mmiLock)
      {
        MmiMessage selectMessage = new MmiMessage(TbsMmiMessageType.MenuAnswer, 3);
        selectMessage.Message[0] = 0;
        selectMessage.Message[1] = 0;
        selectMessage.Message[2] = choice;
        _mmiMessageQueue.Add(selectMessage);
        // Don't explicitly request a response for a "back" request as that
        // could choke the message queue with a message that the CAM
        // never answers.
        if (choice != 0)
        {
          _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
        }
      }
      return true;
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("Turbosight: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("Turbosight: failed to answer enquiry, the CAM is not present");
        return false;
      }

      if (answer.Length > 254)
      {
        this.LogError("Turbosight: answer too long, length = {0}", answer.Length);
        return false;
      }

      byte responseType = (byte)MmiResponseType.Answer;
      if (cancel)
      {
        responseType = (byte)MmiResponseType.Cancel;
      }
      lock (_mmiLock)
      {
        MmiMessage answerMessage = new MmiMessage(TbsMmiMessageType.Answer, answer.Length + 3);
        answerMessage.Message[0] = (byte)(answer.Length + 1);
        answerMessage.Message[1] = 0;
        answerMessage.Message[2] = responseType;
        int offset = 3;
        for (int i = 0; i < answer.Length; i++)
        {
          answerMessage.Message[offset++] = (byte)answer[i];
        }
        _mmiMessageQueue.Add(answerMessage);
        // We have to request a response.
        _mmiMessageQueue.Add(new MmiMessage(TbsMmiMessageType.GetMmi));
      }
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
      this.LogDebug("Turbosight: send DiSEqC command");

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("Turbosight: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Turbosight: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Diseqc;
      accessParams.DiseqcTransmitMessageLength = (uint)command.Length;
      accessParams.DiseqcTransmitMessage = new byte[MAX_DISEQC_MESSAGE_LENGTH];
      Buffer.BlockCopy(command, 0, accessParams.DiseqcTransmitMessage, 0, command.Length);

      Marshal.StructureToPtr(accessParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, TBS_ACCESS_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, _propertyIdTbsAccess,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Turbosight: result = success");
        return true;
      }

      this.LogError("Turbosight: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      this.LogDebug("Turbosight: send tone burst command, command = {0}", command);

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Tone;
      if (command == ToneBurst.ToneBurst)
      {
        accessParams.Tone = TbsTone.BurstUnmodulated;
      }
      else if (command == ToneBurst.DataBurst)
      {
        accessParams.Tone = TbsTone.BurstModulated;
      }

      Marshal.StructureToPtr(accessParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, TBS_ACCESS_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, _propertyIdTbsAccess,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Turbosight: result = success");
        return true;
      }

      this.LogError("Turbosight: failed to send tone burst command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      this.LogDebug("Turbosight: set tone state, state = {0}", state);

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Tone;
      if (state == Tone22kState.Off)
      {
        accessParams.Tone = TbsTone.Off;
      }
      else if (state == Tone22kState.On)
      {
        accessParams.Tone = TbsTone.On;
      }

      Marshal.StructureToPtr(accessParams, _generalBuffer, false);
      //Dump.DumpBinary(_generalBuffer, TBS_ACCESS_PARAMS_SIZE);

      int hr = _propertySet.Set(_propertySetGuid, _propertyIdTbsAccess,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE
      );
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Turbosight: result = success");
        return true;
      }

      this.LogError("Turbosight: failed to set tone state, hr = 0x{0:x}", hr);
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
      this.LogDebug("Turbosight: read DiSEqC response");
      response = null;

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }

      for (int i = 0; i < TBS_ACCESS_PARAMS_SIZE; i++)
      {
        Marshal.WriteByte(_generalBuffer, i, 0);
      }

      TbsAccessParams accessParams = new TbsAccessParams();
      accessParams.AccessMode = TbsAccessMode.Diseqc;
      int returnedByteCount;
      int hr = _propertySet.Get(_propertySetGuid, _propertyIdTbsAccess,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE,
        _generalBuffer, TBS_ACCESS_PARAMS_SIZE,
        out returnedByteCount
      );
      if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != TBS_ACCESS_PARAMS_SIZE)
      {
        this.LogError("Turbosight: failed to read DiSEqC response, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
        return false;
      }

      Dump.DumpBinary(_generalBuffer, returnedByteCount);

      accessParams = (TbsAccessParams)Marshal.PtrToStructure(_generalBuffer, typeof(TbsAccessParams));
      if (accessParams.DiseqcReceiveMessageLength > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("Turbosight: unexpected number of DiSEqC response message bytes ({0}) returned", accessParams.DiseqcReceiveMessageLength);
        return false;
      }
      response = new byte[accessParams.DiseqcReceiveMessageLength];
      Buffer.BlockCopy(accessParams.DiseqcReceiveMessage, 0, response, 0, (int)accessParams.DiseqcReceiveMessageLength);
      this.LogDebug("Turbosight: result = success");
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IRemoteControlListener.Open()
    {
      this.LogDebug("Turbosight: open remote control interface");

      if (!_isTurbosight)
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Turbosight: remote control interface is already open");
        return true;
      }

      if (_isUsb)
      {
        _remoteControlInterface = new RemoteControlCyprus(_tunerProductInstanceId, _propertySet);
        if (!_remoteControlInterface.Open())
        {
          _remoteControlInterface = null;
          return false;
        }
      }
      else
      {
        _remoteControlInterface = new RemoteControlNxp(_tunerProductInstanceId);
        if (!_remoteControlInterface.Open())
        {
          _remoteControlInterface = new RemoteControlConexant(_tunerProductInstanceId);
          if (!_remoteControlInterface.Open())
          {
            _remoteControlInterface = null;
            return false;
          }
        }
      }

      _isRemoteControlInterfaceOpen = true;
      this.LogDebug("Turbosight: result = success");
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
      this.LogDebug("Turbosight: close remote control interface");

      if (isDisposing && _remoteControlInterface != null)
      {
        _remoteControlInterface.Close();
        _remoteControlInterface = null;
      }

      _isRemoteControlInterfaceOpen = false;
      this.LogDebug("Turbosight: result = success");
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
      this.LogDebug("Turbosight: get available stream IDs");
      streamIds = null;

      if (!_isTurbosight || _propertyIdPlpInfo < 0)
      {
        // As far as I know it is not possible to get the list of DVB-S2 input
        // stream identifiers from the driver.
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }

      int returnedByteCount;
      int hr = _propertySet.Get(_propertySetGuid, _propertyIdPlpInfo,
        _generalBuffer, PLP_INFO_SIZE,
        _generalBuffer, PLP_INFO_SIZE,
        out returnedByteCount
      );
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == PLP_INFO_SIZE)
      {
        this.LogDebug("Turbosight: result = success");
        PlpInfo info = (PlpInfo)Marshal.PtrToStructure(_generalBuffer, typeof(PlpInfo));
        streamIds = new List<int>(info.PlpCount);
        for (int i = 0; i < info.PlpCount; i++)
        {
          streamIds.Add(info.PlpIdList[i]);
        }
        return true;
      }

      this.LogError("Turbosight: failed to get available stream IDs, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      return false;
    }

    /// <summary>
    /// Select a stream.
    /// </summary>
    /// <param name="streamId">The identifier of the stream to select.</param>
    /// <returns><c>true</c> if the stream is selected successfully, otherwise <c>false</c></returns>
    public bool SelectStream(int streamId)
    {
      this.LogDebug("Turbosight: select stream, stream ID = {0}", streamId);

      if (!_isTurbosight || (_propertyIdInputStreamId < 0 && _propertyIdPlpInfo < 0))
      {
        this.LogWarn("Turbosight: not initialised or interface not supported");
        return false;
      }
      if (streamId > 255)
      {
        this.LogError("Turbosight: stream ID is too large, stream ID = {0}", streamId);
        return false;
      }

      int hr;
      if (_propertyIdInputStreamId >= 0)
      {
        MediaInfo info = new MediaInfo();
        info.StreamIdentifier = streamId;
        Marshal.StructureToPtr(info, _generalBuffer, false);
        hr = _propertySet.Set(_propertySetGuid, _propertyIdInputStreamId, _generalBuffer, MEDIA_INFO_SIZE, _generalBuffer, MEDIA_INFO_SIZE);
      }
      else
      {
        PlpInfo info = new PlpInfo();
        info.PlpId = (byte)streamId;
        Marshal.StructureToPtr(info, _generalBuffer, false);
        hr = _propertySet.Set(_propertySetGuid, _propertyIdPlpInfo, _generalBuffer, PLP_INFO_SIZE, _generalBuffer, PLP_INFO_SIZE);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Turbosight: result = success");
        return true;
      }

      this.LogError("Turbosight: failed to select stream, hr = 0x{0:x}, stream ID = {1}", hr, streamId);
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

    ~Turbosight()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isTurbosight)
      {
        CloseConditionalAccessInterface(isDisposing);
        CloseRemoteControlListenerInterface(isDisposing);
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (isDisposing)
      {
        if (_isUsb)
        {
          Release.ComObject("Turbosight property set", ref _propertySet);
        }
        _tunerFilter = null;
        _isTurbosight = false;
      }
    }

    #endregion
  }
}