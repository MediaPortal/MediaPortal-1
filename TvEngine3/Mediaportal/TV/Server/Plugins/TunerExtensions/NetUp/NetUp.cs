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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;
using ITuner = Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.ITuner;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.NetUp
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for NetUP tuners.
  /// </summary>
  public class NetUp : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, IDiseqcDevice, IDisposable
  {
    #region enums

    private enum NetUpIoControl : uint
    {
      Diseqc = 0x100000,

      CiStatus = 0x200000,
      ApplicationInfo = 0x210000,
      ConditionalAccessInfo = 0x220000,
      CiReset = 0x230000,

      MmiEnterMenu = 0x300000,
      MmiGetMenu = 0x310000,
      MmiAnswerMenu = 0x320000,
      MmiClose = 0x330000,
      MmiGetEnquiry = 0x340000,
      MmiPutAnswer = 0x350000,

      PmtListChange = 0x400000
    }

    [Flags]
    private enum NetUpCiState
    {
      Empty = 0,
      CamPresent = 1,
      MmiMenuReady = 2,
      MmiEnquiryReady = 4
    }

    #endregion

    #region structs

    // NetUP and DVBSky structures don't quite match due to different
    // strategies for 64 bit compatibility and struct alignment. Refer to the
    // notes in ExecuteIoctl() for more detail. The structures below represent
    // the original NetUP and current DVBSky structures.

    // Okay.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ApplicationInfo    // NETUP_CAM_APPLICATION_INFO
    {
      public MmiApplicationType ApplicationType;
      public ushort Manufacturer;
      public ushort ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] RootMenuTitle;
    }

    // NetUP 268 vs DVBSky 264 => marshal manually.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CiStateInfo    // NETUP_CAM_STATUS
    {
      public NetUpCiState CiState;      // New NetUP driver: DWORD64 instead of DWORD.

      // These fields don't ever seem to be filled by the NetUP driver. We can
      // query for application info directly.
      public ushort Manufacturer;
      public ushort ManufacturerCode;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] RootMenuTitle;
    }

    // NetUP 520 vs DVBSky 516 => marshal manually.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CaInfo   // NETUP_CAM_INFO
    {
      public uint NumberOfCaSystemIds;  // New NetUP driver: DWORD64 instead of DWORD.
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CA_SYSTEM_IDS)]
      public ushort[] CaSystemIds;
    }

    // Okay.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MmiMenuEntry
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Text;
      #pragma warning restore 0649
    }

    // NetUP 17164 vs DVBSky 17160 => automatic marshaling okay.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MmiMenu    // NETUP_CAM_MENU
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsMenu;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Title;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] SubTitle;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Footer;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CAM_MENU_ENTRIES)]
      public MmiMenuEntry[] Entries;
      public uint EntryCount;           // New NetUP driver: DWORD64 instead of DWORD.
    }

    // Okay.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct MmiEnquiry   // NETUP_CAM_MMI_ENQUIRY
    {
      [MarshalAs(UnmanagedType.Bool)]
      public bool IsBlindAnswer;
      public byte ExpectedAnswerLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STRING_LENGTH)]
      public byte[] Prompt;
    }

    // Okay.
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct MmiAnswer    // NETUP_CAM_MMI_ANSWER
    {
      public byte AnswerLength;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_STRING_LENGTH)]
      public string Answer;
    }

    #endregion

    #region constants

    private static readonly Guid NETUP_BDA_EXTENSION_PROPERTY_SET = new Guid(0x5aa642f2, 0xbf94, 0x4199, 0xa9, 0x8c, 0xc2, 0x22, 0x20, 0x91, 0xe3, 0xc3);

    // Default (DVBSky, TechnoTrend clones)
    private static readonly int DEFAULT_CI_STATE_INFO_SIZE = Marshal.SizeOf(typeof(CiStateInfo));         // 264
    private static readonly int DEFAULT_CA_INFO_SIZE = Marshal.SizeOf(typeof(CaInfo));                    // 516
    private static readonly int DEFAULT_MMI_MENU_SIZE = Marshal.SizeOf(typeof(MmiMenu));                  // 17160

    // NetUP
    private static readonly int NETUP_CI_STATE_INFO_SIZE = DEFAULT_CI_STATE_INFO_SIZE + 4;                // 268
    private static readonly int NETUP_CA_INFO_SIZE = DEFAULT_CA_INFO_SIZE + 4;                            // 520
    private static readonly int NETUP_MMI_MENU_SIZE = DEFAULT_MMI_MENU_SIZE + 4;                          // 17164

    // max
    private static readonly int CI_STATE_INFO_SIZE = Math.Max(DEFAULT_CI_STATE_INFO_SIZE, NETUP_CI_STATE_INFO_SIZE);
    private static readonly int CA_INFO_SIZE = Math.Max(DEFAULT_CA_INFO_SIZE, NETUP_CA_INFO_SIZE);
    private static readonly int MMI_MENU_SIZE = Math.Max(DEFAULT_MMI_MENU_SIZE, NETUP_MMI_MENU_SIZE);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private const int COMMAND_SIZE = 48;
    private static readonly int APPLICATION_INFO_SIZE = Marshal.SizeOf(typeof(ApplicationInfo));  // 261
    private static readonly int MMI_ENQUIRY_SIZE = Marshal.SizeOf(typeof(MmiEnquiry));            // 261
    private static readonly int MMI_ANSWER_SIZE = Marshal.SizeOf(typeof(MmiAnswer));              // 257
    private const int MAX_BUFFER_SIZE = 65536;
    private const int MAX_STRING_LENGTH = 256;
    private const int MAX_CA_SYSTEM_IDS = 256;
    private const int MAX_CAM_MENU_ENTRIES = 64;
    private const int MAX_DISEQC_MESSAGE_LENGTH = 64;         // This is to reduce the _generalBuffer size - the driver limit is MaxBufferSize.

    private const int GENERAL_BUFFER_SIZE = MAX_DISEQC_MESSAGE_LENGTH;
    private static readonly int MMI_BUFFER_SIZE = new int[] {
        APPLICATION_INFO_SIZE, CA_INFO_SIZE, CI_STATE_INFO_SIZE, MMI_ANSWER_SIZE, MMI_ENQUIRY_SIZE, MMI_MENU_SIZE,
    }.Max();
    private const int MMI_HANDLER_THREAD_WAIT_TIME = 500;     // unit = ms

    #endregion

    #region variables

    private bool _isNetUp = false;
    private bool _isCaInterfaceOpen = false;
    private bool _isCamPresent = false;

    // Functions that are called from both the main TV service threads
    // as well as the MMI handler thread use their own local buffer to
    // avoid buffer data corruption. Otherwise functions called exclusively
    // by the MMI handler thread use the MMI buffer and other functions
    // use the general buffer.
    private IntPtr _generalBuffer = IntPtr.Zero;
    private IntPtr _mmiBuffer = IntPtr.Zero;

    private Guid _propertySetGuid = NETUP_BDA_EXTENSION_PROPERTY_SET;
    private IKsPropertySet _propertySet = null;

    private Thread _mmiHandlerThread = null;
    private AutoResetEvent _mmiHandlerThreadStopEvent = null;
    private object _mmiLock = new object();
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();

    private int _ciStateInfoSize = NETUP_CI_STATE_INFO_SIZE;
    private int _caInfoSize = NETUP_CA_INFO_SIZE;
    private int _mmiMenuSize = NETUP_MMI_MENU_SIZE;

    #endregion

    #region constructors

    /// <summary>
    /// Constructor for <see cref="NetUp"/> instances.
    /// </summary>
    public NetUp()
    {
    }

    /// <summary>
    /// Constructor for non-inherited types (eg. <see cref="DvbSky"/>).
    /// </summary>
    public NetUp(Guid propertySetGuid)
    {
      _propertySetGuid = propertySetGuid;
      if (_propertySetGuid != NETUP_BDA_EXTENSION_PROPERTY_SET)
      {
        _ciStateInfoSize = DEFAULT_CI_STATE_INFO_SIZE;
        _caInfoSize = DEFAULT_CA_INFO_SIZE;
        _mmiMenuSize = DEFAULT_MMI_MENU_SIZE;
      }
    }

    #endregion

    /// <summary>
    /// Read the conditional access application information.
    /// </summary>
    /// <returns>an HRESULT indicating whether the application information was successfully retrieved</returns>
    private int ReadApplicationInformation()
    {
      this.LogDebug("NetUP: read application information");

      for (int i = 0; i < APPLICATION_INFO_SIZE; i++)
      {
        Marshal.WriteByte(_mmiBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = GetIoctl(NetUpIoControl.ApplicationInfo, _mmiBuffer, APPLICATION_INFO_SIZE, out returnedByteCount);
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == APPLICATION_INFO_SIZE)
      {
        //Dump.DumpBinary(_mmiBuffer, APPLICATION_INFO_SIZE);
        ApplicationInfo info = (ApplicationInfo)Marshal.PtrToStructure(_mmiBuffer, typeof(ApplicationInfo));
        this.LogDebug("  type         = {0}", info.ApplicationType);
        this.LogDebug("  manufacturer = 0x{0:x4}", info.Manufacturer);
        this.LogDebug("  code         = 0x{0:x4}", info.ManufacturerCode);
        this.LogDebug("  menu title   = {0}", DvbTextConverter.Convert(info.RootMenuTitle));
      }
      else
      {
        this.LogWarn("NetUP: failed to read application information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }

      return hr;
    }

    /// <summary>
    /// Read the conditional access information.
    /// </summary>
    /// <returns>an HRESULT indicating whether the conditional access information was successfully retrieved</returns>
    private int ReadConditionalAccessInformation()
    {
      this.LogDebug("NetUP: read conditional access information");

      for (int i = 0; i < _caInfoSize; i++)
      {
        Marshal.WriteByte(_mmiBuffer, i, 0);
      }

      int returnedByteCount;
      int hr = GetIoctl(NetUpIoControl.ConditionalAccessInfo, _mmiBuffer, _caInfoSize, out returnedByteCount);
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == _caInfoSize)
      {
        // Have to marshal manually due to mismatch between the NetUP and
        // DVBSky structures.
        int numberOfCaSystemIds = Marshal.ReadInt32(_mmiBuffer, 0);
        this.LogDebug("  # CAS IDs = {0}", numberOfCaSystemIds);
        int offset = 4;
        if (_propertySetGuid == NETUP_BDA_EXTENSION_PROPERTY_SET)
        {
          offset += 4;
        }
        for (int i = 0; i < numberOfCaSystemIds; i++)
        {
          this.LogDebug("    {0, -7} = 0x{1:x4}", i + 1, (ushort)Marshal.ReadInt16(_mmiBuffer, offset));
          offset += 2;  // size of Int16
        }
      }
      else
      {
        this.LogWarn("NetUP: failed to read conditional access information, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      }

      return hr;
    }

    /// <summary>
    /// Get the conditional access interface status.
    /// </summary>
    /// <param name="ciState">State of the CI slot.</param>
    /// <returns>an HRESULT indicating whether the CI status was successfully retrieved</returns>
    private int GetCiStatus(out NetUpCiState ciState)
    {
      ciState = NetUpCiState.Empty;

      lock (_mmiLock)
      {
        for (int i = 0; i < _ciStateInfoSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }
        int returnedByteCount;
        int hr = GetIoctl(NetUpIoControl.CiStatus, _mmiBuffer, _ciStateInfoSize, out returnedByteCount);
        if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == _ciStateInfoSize)
        {
          // Have to marshal manually due to mismatch between the NetUP and
          // DVBSky structures.
          ciState = (NetUpCiState)Marshal.ReadInt32(_mmiBuffer, 0);
        }
        return hr;
      }
    }

    #region IOCTL

    private int SetIoctl(NetUpIoControl controlCode, IntPtr inBuffer, int inBufferSize)
    {
      int returnedByteCount;
      return ExecuteIoctl(controlCode, inBuffer, inBufferSize, IntPtr.Zero, 0, out returnedByteCount);
    }

    private int GetIoctl(NetUpIoControl controlCode, IntPtr outBuffer, int outBufferSize, out int returnedByteCount)
    {
      return ExecuteIoctl(controlCode, IntPtr.Zero, 0, outBuffer, outBufferSize, out returnedByteCount);
    }

    private int ExecuteIoctl(NetUpIoControl controlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int returnedByteCount)
    {
      returnedByteCount = 0;
      int hr = (int)NativeMethods.HResult.E_FAIL;
      if (_propertySet == null)
      {
        this.LogError("NetUP: attempted to execute IOCTL when property set is NULL");
        return hr;
      }

      IntPtr instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      IntPtr commandBuffer = Marshal.AllocCoTaskMem(COMMAND_SIZE);
      IntPtr returnedByteCountBuffer = Marshal.AllocCoTaskMem(sizeof(int));
      try
      {
        // Clear buffers. This is probably not actually needed, but better
        // to be safe than sorry!
        for (int i = 0; i < INSTANCE_SIZE; i++)
        {
          Marshal.WriteByte(instanceBuffer, i, 0);
        }
        Marshal.WriteInt32(returnedByteCountBuffer, 0, 0);

        // Originally NetUP's drivers required 32 bit pointers on 32 bit
        // operating systems and 64 bit pointers on 64 bit operating systems.
        // This was somewhat inconvenient as detecting whether you're running
        // under WOW64 can be awkward. I contacted NetUP and asked whether it
        // would be possible to expose a consistent interface. They kindly
        // obliged and padded the command struct pointers in their driver for
        // 32 bit operating systems. At the same time they also changed all
        // DWORDs in structs to DWORD64 (which was unnecessary).
        // ...then I found out DVBSky use the same API but have modified it to
        // use 32 bit pointers even under 64 bit operating systems. <DOH!!!>
        // This class supports both variants of the API.
        // mm1352000, 2013-07-20
        if (_propertySetGuid == NETUP_BDA_EXTENSION_PROPERTY_SET)
        {
          Marshal.WriteInt64(commandBuffer, 0, (long)controlCode);
          Marshal.WriteInt64(commandBuffer, 8, inBuffer.ToInt64());
          Marshal.WriteInt64(commandBuffer, 16, inBufferSize);
          Marshal.WriteInt64(commandBuffer, 24, outBuffer.ToInt64());
          Marshal.WriteInt64(commandBuffer, 32, outBufferSize);
          Marshal.WriteInt64(commandBuffer, 40, returnedByteCountBuffer.ToInt64());
        }
        else
        {
          Marshal.WriteInt32(commandBuffer, 0, (int)controlCode);
          Marshal.WriteInt32(commandBuffer, 4, inBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 8, inBufferSize);
          Marshal.WriteInt32(commandBuffer, 12, outBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 16, outBufferSize);
          Marshal.WriteInt32(commandBuffer, 20, returnedByteCountBuffer.ToInt32());
          Marshal.WriteInt32(commandBuffer, 24, 0);
          Marshal.WriteInt32(commandBuffer, 28, 0);
          Marshal.WriteInt32(commandBuffer, 32, 0);
          Marshal.WriteInt32(commandBuffer, 36, 0);
          Marshal.WriteInt32(commandBuffer, 40, 0);
          Marshal.WriteInt32(commandBuffer, 44, 0);
        }

        hr = _propertySet.Set(_propertySetGuid, 0, instanceBuffer, INSTANCE_SIZE, commandBuffer, COMMAND_SIZE);
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          returnedByteCount = Marshal.ReadInt32(returnedByteCountBuffer, 0);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(instanceBuffer);
        Marshal.FreeCoTaskMem(commandBuffer);
        Marshal.FreeCoTaskMem(returnedByteCountBuffer);
      }
      return hr;
    }

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread to receive MMI messages from the CAM.
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
          this.LogDebug("NetUP: starting new MMI handler thread");
          _mmiHandlerThreadStopEvent = new AutoResetEvent(false);
          _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
          _mmiHandlerThread.Name = "NetUP MMI handler";
          _mmiHandlerThread.IsBackground = true;
          _mmiHandlerThread.Priority = ThreadPriority.Lowest;
          _mmiHandlerThread.Start();
        }
      }
    }

    /// <summary>
    /// Stop the thread that receives MMI messages from the CAM.
    /// </summary>
    private void StopMmiHandlerThread()
    {
      lock (_mmiLock)
      {
        if (_mmiHandlerThread != null)
        {
          if (!_mmiHandlerThread.IsAlive)
          {
            this.LogWarn("NetUP: aborting old MMI handler thread");
            _mmiHandlerThread.Abort();
          }
          else
          {
            _mmiHandlerThreadStopEvent.Set();
            if (!_mmiHandlerThread.Join(MMI_HANDLER_THREAD_WAIT_TIME * 2))
            {
              this.LogWarn("NetUP: failed to join MMI handler thread, aborting thread");
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
    /// Thread function for receiving MMI messages from the CAM.
    /// </summary>
    private void MmiHandler()
    {
      this.LogDebug("NetUP: MMI handler thread start polling");
      NetUpCiState ciState = NetUpCiState.Empty;
      NetUpCiState prevCiState = NetUpCiState.Empty;
      try
      {
        while (!_mmiHandlerThreadStopEvent.WaitOne(MMI_HANDLER_THREAD_WAIT_TIME))
        {
          int hr = GetCiStatus(out ciState);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("NetUP: failed to get CI status, hr = 0x{0:x}", hr);
            continue;
          }

          // Handle CI slot state changes.
          if (ciState != prevCiState)
          {
            this.LogInfo("NetUP: CI state change");
            this.LogInfo("  old state    = {0}", prevCiState);
            this.LogInfo("  new state    = {0}", ciState);

            prevCiState = ciState;
            _isCamPresent = (ciState != NetUpCiState.Empty);
          }

          if (ciState.HasFlag(NetUpCiState.MmiEnquiryReady))
          {
            HandleEnquiry();
          }
          if (ciState.HasFlag(NetUpCiState.MmiMenuReady))
          {
            HandleMenu();
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "NetUP: MMI handler thread exception");
        return;
      }
      this.LogDebug("NetUP: MMI handler thread stop polling");
    }

    /// <summary>
    /// Read an MMI menu object and invoke call backs as appropriate.
    /// </summary>
    /// <returns>an HRESULT indicating whether the menu object was successfully handled</returns>
    private int HandleMenu()
    {
      this.LogDebug("NetUP: read menu");

      MmiMenu mmi;
      lock (_mmiLock)
      {
        for (int i = 0; i < _mmiMenuSize; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }

        int returnedByteCount;
        int hr = GetIoctl(NetUpIoControl.MmiGetMenu, _mmiBuffer, _mmiMenuSize, out returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != _mmiMenuSize)
        {
          this.LogError("NetUP: failed to get menu detail, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return hr;
        }
        mmi = (MmiMenu)Marshal.PtrToStructure(_mmiBuffer, typeof(MmiMenu));
      }

      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack == null)
        {
          this.LogDebug("NetUP: menu call back not set");
        }

        string title = DvbTextConverter.Convert(mmi.Title);
        string subTitle = DvbTextConverter.Convert(mmi.SubTitle);
        string footer = DvbTextConverter.Convert(mmi.Footer);
        this.LogDebug("  is menu   = {0}", mmi.IsMenu);
        this.LogDebug("  title     = {0}", title);
        this.LogDebug("  sub-title = {0}", subTitle);
        this.LogDebug("  footer    = {0}", footer);
        this.LogDebug("  # entries = {0}", mmi.EntryCount);
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiMenu(title, subTitle, footer, (int)mmi.EntryCount);
        }

        for (int i = 0; i < mmi.EntryCount; i++)
        {
          string entry = DvbTextConverter.Convert(mmi.Entries[i].Text);
          this.LogDebug("    {0, -7} = {1}", i + 1, entry);
          if (_caMenuCallBack != null)
          {
            _caMenuCallBack.OnCiMenuChoice(i, entry);
          }
        }
      }
      return (int)NativeMethods.HResult.S_OK;
    }

    /// <summary>
    /// Read an MMI enquiry object and invoke call backs as appropriate.
    /// </summary>
    /// <returns>an HRESULT indicating whether the enquiry object was successfully handled</returns>
    private int HandleEnquiry()
    {
      this.LogDebug("NetUP: read enquiry");

      MmiEnquiry mmi = new MmiEnquiry();
      string prompt = string.Empty;
      lock (_mmiLock)
      {
        for (int i = 0; i < MMI_ENQUIRY_SIZE; i++)
        {
          Marshal.WriteByte(_mmiBuffer, i, 0);
        }

        int returnedByteCount;
        int hr = GetIoctl(NetUpIoControl.MmiGetEnquiry, _mmiBuffer, MMI_ENQUIRY_SIZE, out returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK || returnedByteCount != MMI_ENQUIRY_SIZE)
        {
          this.LogError("NetUP: failed to get enquiry detail, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return hr;
        }

        //Dump.DumpBinary(_mmiBuffer, MMI_ENQUIRY_SIZE);
        mmi = (MmiEnquiry)Marshal.PtrToStructure(_mmiBuffer, typeof(MmiEnquiry));
        prompt = DvbTextConverter.Convert(mmi.Prompt);
      }

      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack == null)
        {
          this.LogDebug("NetUP: menu call back not set");
        }

        this.LogDebug("  prompt = {0}", prompt);
        this.LogDebug("  length = {0}", mmi.ExpectedAnswerLength);
        this.LogDebug("  blind  = {0}", mmi.IsBlindAnswer);
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiRequest(mmi.IsBlindAnswer, mmi.ExpectedAnswerLength, prompt);
        }
      }
      return (int)NativeMethods.HResult.S_OK;
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "NetUP";
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
      this.LogDebug("NetUP: initialising");

      if (_isNetUp)
      {
        this.LogWarn("NetUP: extension already initialised");
        return true;
      }

      IBaseFilter tunerFilter = context as IBaseFilter;
      if (tunerFilter == null)
      {
        this.LogDebug("NetUP: context is not a filter");
        return false;
      }

      IPin pin = null;
      try
      {
        // Find the property set if it exists.
        // The DVBSky and TechnoTrend clones expose the property set on the
        // tuner filter input pin. Try this first because it is easier.
        pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
        _propertySet = CheckPropertySet(pin, "input");
        if (_propertySet != null)
        {
          return true;
        }
        Release.ComObject("NetUP tuner filter input pin", ref pin);

        // NetUP exposes the property set on the tuner filter output pin. Since
        // current NetUP tuners are implemented as a combined filter, the
        // filter output pin is normally going to be unconnected when this
        // function is called. That is a problem because a pin won't correctly
        // report whether it supports a property set unless it is connected to
        // a filter. If this filter pin is currently unconnected, we
        // temporarily connect an infinite tee so that we can check if the pin
        // supports the property set.
        pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
        IPin connected;
        int hr = pin.ConnectedTo(out connected);
        if (hr == (int)NativeMethods.HResult.S_OK && connected != null)
        {
          // We don't need to connect the infinite tee in this case.
          Release.ComObject("NetUP tuner filter connected pin", ref connected);
          _propertySet = CheckPropertySet(pin, "output");
          return _propertySet != null;
        }

        // Get a reference to the filter graph.
        FilterInfo filterInfo;
        hr = tunerFilter.QueryFilterInfo(out filterInfo);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("NetUP: failed to get filter info, hr = 0x{0:x}", hr);
          return false;
        }
        IFilterGraph2 graph = filterInfo.pGraph as IFilterGraph2;
        if (graph == null)
        {
          this.LogDebug("NetUP: filter info graph is null");
          return false;
        }

        // Add an infinite tee.
        IBaseFilter infTee = (IBaseFilter)new InfTee();
        IPin infTeeInputPin = null;
        try
        {
          hr = graph.AddFilter(infTee, "Temp Infinite Tee");
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("NetUP: failed to add infinite tee to graph, hr = 0x{0:x}", hr);
            return false;
          }

          // Connect the infinite tee to the filter.
          infTeeInputPin = DsFindPin.ByDirection(infTee, PinDirection.Input, 0);
          if (infTeeInputPin == null)
          {
            this.LogError("NetUP: failed to find the infinite tee input pin, hr = 0x{0:x}", hr);
            return false;
          }
          hr = graph.ConnectDirect(pin, infTeeInputPin, null);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogError("NetUP: failed to connect infinite tee, hr = 0x{0:x}", hr);
            return false;
          }

          _propertySet = CheckPropertySet(pin, "output");
          return _propertySet != null;
        }
        finally
        {
          pin.Disconnect();
          Release.ComObject("NetUP infinite tee input pin", ref infTeeInputPin);
          graph.RemoveFilter(infTee);
          Release.ComObject("NetUP infinite tee", ref infTee);
          Release.FilterInfo(ref filterInfo);
          graph = null;
        }
      }
      finally
      {
        if (_propertySet != null)
        {
          this.LogInfo("NetUP: extension supported");
          _isNetUp = true;
          _generalBuffer = Marshal.AllocCoTaskMem(GENERAL_BUFFER_SIZE);
        }
        else
        {
          Release.ComObject("NetUP tuner filter output pin", ref pin);
        }
      }
    }

    private IKsPropertySet CheckPropertySet(IPin pin, string pinLogName)
    {
      IKsPropertySet propertySet = pin as IKsPropertySet;
      if (propertySet == null)
      {
        this.LogDebug("NetUP: {0} pin is not a property set", pinLogName);
      }
      else
      {
        KSPropertySupport support;
        int hr = propertySet.QuerySupported(_propertySetGuid, 0, out support);
        if (hr != (int)NativeMethods.HResult.S_OK || !support.HasFlag(KSPropertySupport.Set))
        {
          this.LogDebug("NetUP: property set not supported on {0} pin, hr = 0x{1:x}, support = {2}", pinLogName, hr, support);
          propertySet = null;
        }
      }
      return propertySet;
    }

    #region tuner state change call backs

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

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("NetUP: open conditional access interface");

      if (!_isNetUp)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("NetUP: conditional access interface is already open");
        return true;
      }

      // If the CI state can't be retrieved successfully then there is no CI slot.
      _mmiBuffer = Marshal.AllocCoTaskMem(MMI_BUFFER_SIZE);
      NetUpCiState ciState;
      int hr = GetCiStatus(out ciState);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("NetUP: CI slot not detected, hr = 0x{0:x}", hr);
        Marshal.FreeCoTaskMem(_mmiBuffer);
        _mmiBuffer = IntPtr.Zero;
        return false;
      }

      _isCaInterfaceOpen = true;
      _isCamPresent = ciState.HasFlag(NetUpCiState.CamPresent);
      StartMmiHandlerThread();

      this.LogDebug("NetUP: result = success");
      return true;
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
      this.LogDebug("NetUP: close conditional access interface");

      if (isDisposing)
      {
        StopMmiHandlerThread();
      }

      _isCamPresent = false;
      if (_mmiBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_mmiBuffer);
        _mmiBuffer = IntPtr.Zero;
      }
      _isCaInterfaceOpen = false;

      this.LogDebug("NetUP: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      this.LogDebug("NetUP: reset conditional access interface");

      if (!_isNetUp)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }

      bool success = (this as IConditionalAccessProvider).Close();

      int hr = SetIoctl(NetUpIoControl.CiReset, IntPtr.Zero, 0);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("NetUP: failed to reset conditional access interface, hr = 0x{0:x}", hr);
        success = false;
      }

      return success && (this as IConditionalAccessProvider).Open();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("NetUP: is conditional access interface ready");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }

      // The CAM state is updated by the MMI handler thread. We can only
      // determine whether a CAM is present or not.
      this.LogDebug("NetUP: result = {0}", _isCamPresent);
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
      this.LogDebug("NetUP: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }
      if (!_isCamPresent)
      {
        this.LogError("NetUP: failed to send conditional access command, the CAM is not present");
        return false;
      }
      if (listAction == CaPmtListManagementAction.Add || listAction == CaPmtListManagementAction.Update)
      {
        this.LogWarn("NetUP: conditional access command list action {0} is not supported", listAction);
        return true;
      }
      if (command == CaPmtCommand.NotSelected)
      {
        this.LogError("NetUP: conditional access command type {0} is not supported", command);
        return true;
      }
      if (pmt == null)
      {
        this.LogError("NetUP: failed to send conditional access command, PMT not supplied");
        return true;
      }

      // The NetUP driver accepts standard PMT and converts it to CA PMT internally.
      ReadOnlyCollection<byte> rawPmt = pmt.GetRawPmt();
      NetUpIoControl code = (NetUpIoControl)((uint)NetUpIoControl.PmtListChange | ((byte)listAction << 8) | (uint)command);
      IntPtr buffer = Marshal.AllocCoTaskMem(rawPmt.Count);
      for (int i = 0; i < rawPmt.Count; i++)
      {
        Marshal.WriteByte(buffer, i, rawPmt[i]);
      }
      //Dump.DumpBinary(buffer, rawPmt.Count);
      int hr = SetIoctl(code, buffer, rawPmt.Count);
      Marshal.FreeCoTaskMem(buffer);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogError("NetUP: failed to send conditional access command, hr = 0x{0:x}", hr);
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
      this.LogDebug("NetUP: enter menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("NetUP: failed to enter menu, the CAM is not present");
        return false;
      }

      int hr;
      lock (_mmiLock)
      {
        ReadApplicationInformation();
        ReadConditionalAccessInformation();

        hr = SetIoctl(NetUpIoControl.MmiEnterMenu, IntPtr.Zero, 0);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogError("NetUP: failed to enter menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("NetUP: close menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("NetUP: failed to close menu, the CAM is not present");
        return false;
      }

      int hr;
      lock (_mmiLock)
      {
        hr = SetIoctl(NetUpIoControl.MmiClose, IntPtr.Zero, 0);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogError("NetUP: failed to close menu, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("NetUP: select menu entry, choice = {0}", choice);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("NetUP: failed to select menu entry, the CAM is not present");
        return false;
      }

      int hr;
      lock (_mmiLock)
      {
        NetUpIoControl code = (NetUpIoControl)((uint)NetUpIoControl.MmiAnswerMenu | choice << 8);
        hr = SetIoctl(code, IntPtr.Zero, 0);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogError("NetUP: failed to select menu entry, hr = 0x{0:x}", hr);
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
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("NetUP: answer enquiry, answer = {0}, cancel = {1}", answer, cancel);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();
      if (!_isCamPresent)
      {
        this.LogError("NetUP: failed to answer enquiry, the CAM is not present");
        return false;
      }

      // We have a limit for the answer string length.
      if (answer.Length > MAX_STRING_LENGTH)
      {
        this.LogError("NetUP: answer too long, length = {0}", answer.Length);
        return false;
      }

      MmiAnswer mmi = new MmiAnswer();
      mmi.AnswerLength = (byte)answer.Length;
      mmi.Answer = answer;
      MmiResponseType responseType = MmiResponseType.Answer;
      if (cancel)
      {
        responseType = MmiResponseType.Cancel;
      }

      int hr;
      lock (_mmiLock)
      {
        Marshal.StructureToPtr(mmi, _mmiBuffer, false);
        //Dump.DumpBinary(_mmiBuffer, MMI_ANSWER_SIZE);
        NetUpIoControl code = (NetUpIoControl)((uint)NetUpIoControl.MmiPutAnswer | ((byte)responseType << 8));
        hr = SetIoctl(code, _mmiBuffer, MMI_ANSWER_SIZE);
      }
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogError("NetUP: failed to answer enquiry, hr = 0x{0:x}", hr);
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
      this.LogDebug("NetUP: send DiSEqC command");

      if (!_isNetUp)
      {
        this.LogWarn("NetUP: not initialised or interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        this.LogWarn("NetUP: DiSEqC command not supplied");
        return true;
      }
      if (command.Length > MAX_DISEQC_MESSAGE_LENGTH)
      {
        this.LogError("NetUP: DiSEqC command too long, length = {0}", command.Length);
        return false;
      }

      Marshal.Copy(command, 0, _generalBuffer, command.Length);
      //Dump.DumpBinary(_generalBuffer, command.Length);

      int hr = SetIoctl(NetUpIoControl.Diseqc, _generalBuffer, command.Length);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("NetUP: result = success");
        return true;
      }

      this.LogError("NetUP: failed to send DiSEqC command, hr = 0x{0:x}", hr);
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      // Not supported.
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

    ~NetUp()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isNetUp)
      {
        CloseConditionalAccessInterface(isDisposing);
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      if (isDisposing)
      {
        Release.ComObject("NetUP property set", ref _propertySet);
      }
      _isNetUp = false;
    }

    #endregion
  }
}