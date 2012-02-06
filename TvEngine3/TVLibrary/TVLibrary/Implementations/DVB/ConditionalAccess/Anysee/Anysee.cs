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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for Anysee tuners.
  /// Smart card slots are not supported.
  /// </summary>
  public class Anysee : IDiSEqCController, ICiMenuActions, IDisposable
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
      Pcb508TC = 18,            // DVB-T + DVB-C + Smartcard Interface + CI
      Pcb508S2,                 // DVB-S2 + Smartcard Interface + CI
      Pcb508T2C,                // DVB-T2 + DVB-C + Smartcard Interface + CI
      Pcb508PTC,                // PCI PCB508TC
      Pcb508PS2,                // PCI PCB508S2
      Pcb508PT2C                // PCI PCB508T2C
    }

    private enum AnyseeCamMenuKey   // CI_KEY_MAP
    {
      // Numeric keys
      Zero = 1,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine,

      // Navigation keys
      Menu = 20,
      Exit,
      Up,
      Down,
      Left,
      Right,
      Select,

      Clear = 30
    }

    private enum AnyseeCiState    // CI_MESSAGE_COMMAND
    {
      Empty = 2002,             // CI_MSG_EXTRACTED_CAM - CAM not present or not initialised
      Clear = 2100,             // CI_MSG_CLEAR - seems to indicate that there are no encrypted PIDs or that the CAM is not decrypting any channels
      CamInserted = 2101,       // CI_MSG_INITIALIZATION_CAM - CAM initialising
      CamOkay = 2102,           // CI_MSG_INSERT_CAM - CAM initialisation finishing
      SendPmtComplete = 2103,   // CI_SEND_PMT_COMPLET - PMT sent to the CAM
      CamReady = 2105           // CI_USE_MENU - CAM menu can be accessed
    }

    private enum AnyseeCiCommand : uint   // CI_CONTROL_COMMAND
    {
      GetDeviceIndex = 1100,    // CI_CONTROL_GET_DEVICE_NUM - get the Anysee device index
      IsOpen = 1104,            // CI_CONTROL_IS_OPEN - check whether the CI API is open
      SetKey = 1105,            // CI_CONTROL_SET_KEY - send a key press to the CAM
      SetTdt = 1106,            // CI_CONTROL_SET_TDT - send TDT to the CAM
      SetPmt = 1110,            // CI_CONTROL_SET_PMT - send PMT to the CAM
      IsOpenSetCallbacks = 2000 // CI_CONTROL_IS_PLUG_OPEN - check whether the CI API is open and set callback functions
    }

    private enum AnyseeMmiMessageType
    {
      Menu = 0,
      InputRequest,
    }

    private enum AnyseeEsType : byte
    {
      Unknown = 0,
      Audio,
      Video,
      Teletext,
      Subtitle,
      Private
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct IrData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;
      public bool Enable;
      public Int32 Key;         // bit 8 = repeat flag (0 = repeat), bits 7-0 = key code
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
      public UInt16 Padding1;   // These bits do have a meaning.
      public AnyseePlatform Platform;
      public Int32 Padding2;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct ApiString
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxApiStringLength)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct CiStateInfo    // tagCIStatus
    {
      public Int32 Size;
      public Int32 DeviceIndex;
      public ApiString Message;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct MmiMenu  // MMIStrsBlock
    {
      public Int32 StringCount;
      public Int32 MenuIndex;
      public IntPtr Entries;                // This is a pointer to an array of pointers.
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct MmiMessage   // tagCIMsgs
    {
      public Int32 DeviceIndex;
      public Int32 SlotIndex;
      public Int32 HeaderCount;
      public Int32 EntryCount;
      public AnyseeMmiMessageType Type;
      public Int32 ExpectedAnswerLength;
      public Int32 KeyCount;
      public ApiString RootMenuTitle;
      public IntPtr Menu;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), ComVisible(true)]
    private struct ApiCallbacks
    {
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnAnyseeCiState OnCiState;
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnAnyseeMmiMessage OnMmiMessage;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct PmtData    // DTVCIPMT
    {
      public byte PmtByte6;                   // Byte 6 from the PMT section (PMT version, current next indicator). 
      private byte Padding1;
      public UInt16 PcrPid;
      public UInt16 ServiceId;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDescriptorDataLength)]
      public byte[] ProgramDescriptorData;    // The first two bytes should specify the length of the descriptor data.
      private UInt16 Padding2;
      public UInt32 EsCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPmtElementaryStreams)]
      public EsPmtData[] EsPmt;
    }

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    private struct EsPmtData
    {
      public UInt16 Pid;
      public AnyseeEsType EsType;
      public byte StreamType;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDescriptorDataLength)]
      public byte[] DescriptorData;           // The first two bytes should specify the length of the descriptor data.
    }

    #endregion

    /// <summary>
    /// This class is used to "hide" awkward aspects of using the Anysee API
    /// like the requirement for one copy of the CIAPI DLL per device and STA
    /// thread access.
    /// </summary>
    private class AnyseeCiApi
    {
      #region structs

      [StructLayout(LayoutKind.Sequential), ComVisible(true)]
      private struct CiDeviceInfo   // ANYSEECIDEVICESINFO
      {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public ApiString[] DevicePaths;      // A list of the capture device paths for all Anysee devices connected to the system.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public Int32[] DevicePathLengths;     // The length of the corresponding device path in DevicePaths.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public Int32[] DevicePathIndices;     // The index of the corresponding capture device in the set of KSCATEGORY_BDA_RECEIVER_COMPONENT devices returned by the system enumerator.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDeviceCount)]
        public Int32[] DeviceIndices;         // The Anysee device index for the corresponding device.
      }

      #endregion

      #region DLL imports

      /// <summary>
      /// The LoadLibrary function maps the specified executable module into the address space of the calling process.
      /// </summary>
      /// <param name="lpLibFileName">Pointer to a null-terminated string that names the executable module (either a .dll or .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
      /// <returns>If the function succeeds, the return value is a handle to the module.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
      [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA", CharSet = CharSet.Ansi)]
      private static extern IntPtr LoadLibrary(string lpLibFileName);

      /// <summary>
      /// The FreeLibrary function decrements the reference count of the loaded dynamic-link library (DLL). When the reference count reaches zero, the module is unmapped from the address space of the calling process and the handle is no longer valid.
      /// </summary>
      /// <param name="hLibModule">Handle to the loaded DLL module. The LoadLibrary or GetModuleHandle function returns this handle.</param>
      /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
      [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", CharSet = CharSet.Ansi)]
      private static extern int FreeLibrary(IntPtr hLibModule);

      /// <summary>
      /// The GetProcAddress function retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
      /// </summary>
      /// <param name="hModule">Handle to the DLL module that contains the function or variable. The LoadLibrary or GetModuleHandle function returns this handle.</param>
      /// <param name="lpProcName">Pointer to a null-terminated string containing the function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero.</param>
      /// <returns>If the function succeeds, the return value is the address of the exported function or variable.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
      [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi)]
      private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

      #endregion

      #region delegates

      #region static DLL functions

      /// <summary>
      /// Create a new common interface API instance. One instance is required for each Anysee device.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance created. The memory must be allocated by TV Server before calling this function.</param>
      /// <returns><c>one</c> if an instance is successfully created, otherwise <c>zero</c></returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 CreateDtvCIAPI(IntPtr ciApiInstance);

      /// <summary>
      /// Destroy a previously created common interface API instance.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance to destroy. The memory must be released by TV Server after calling this function.</param>
      /// <returns><c>one</c> if the instance is successfully destroyed, otherwise <c>zero</c></returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 DestroyDtvCIAPI(IntPtr ciApiInstance);

      /// <summary>
      /// Get the number of Anysee devices connected to the system with corresponding
      /// device path and index detail to enable opening a common interface API instance.
      /// </summary>
      /// <param name="deviceInfo">A buffer containing device path and index information for all Anysee devices connected to the system.</param>
      /// <returns>the number of Anysee devices connected to the system</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 GetanyseeNumberofDevicesEx(IntPtr deviceInfo);

      #endregion

      #region CIAPI unmanaged class

      /// <summary>
      /// Open the common interface API for a specific Anysee device.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance to open.</param>
      /// <param name="windowHandle">A reference to a window to use as an alternative to callbacks (CIAPI.dll sends custom messages to the window).</param>
      /// <param name="deviceIndex">The Anysee index for the device to open.</param>
      /// <returns>an HRESULT indicating whether the API was successfully opened</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 OpenCILib(IntPtr ciApiInstance, IntPtr windowHandle, Int32 deviceIndex);

      /// <summary>
      /// Execute a command on an open common interface instance.
      /// </summary>
      /// <param name="ciApiInstance">A reference to the instance.</param>
      /// <param name="command">The command to execute.</param>
      /// <param name="inputParams">A reference to a buffer containing the appropriate input parameters for the command.</param>
      /// <param name="outputParams">A reference to a buffer that will be filled with the command's output parameters.</param>
      /// <returns>an HRESULT indicating whether the command was successfully executed</returns>
      [UnmanagedFunctionPointer(CallingConvention.StdCall)]
      private delegate Int32 CI_Control(IntPtr ciApiInstance, AnyseeCiCommand command, IntPtr inputParams, IntPtr outputParams);

      #endregion

      #endregion

      #region constants

      private const int ApiInstanceSize = 76;
      private const int MaxDeviceCount = 32;
      private const int CiDeviceInfoSize = MaxDeviceCount * (MaxApiStringLength + 12);

      #endregion

      #region variables

      // This variable tracks the number of open API instances which corresponds with used DLL indices.
      private static int _apiCount = 0;

      // Delegate instances for each API DLL function.
      private CreateDtvCIAPI _createApi = null;
      private DestroyDtvCIAPI _destroyApi = null;
      private GetanyseeNumberofDevicesEx _getAnyseeDeviceCount = null;
      private OpenCILib _openApi = null;
      private CI_Control _ciControl = null;

      private int _apiIndex = 0;
      private bool _dllLoaded = false;
      private String _devicePath = String.Empty;

      private IntPtr _ciApiInstance = IntPtr.Zero;
      private IntPtr _windowHandle = IntPtr.Zero;

      private Thread _apiAccessThread = null;
      private bool _stopApiAccessThread = false;

      #endregion

      /// <summary>
      /// Create a new CI API instance.
      /// </summary>
      public AnyseeCiApi()
      {
        _apiCount++;
        _apiIndex = _apiCount;
        Log.Log.Debug("Anysee: loading API, API index = {0}", _apiIndex);
        if (!File.Exists("CIAPI" + _apiIndex + ".dll"))
        {
          try
          {
            File.Copy("CIAPI.dll", "CIAPI" + _apiIndex + ".dll");
          }
          catch (Exception ex)
          {
            Log.Log.Debug("Anysee: failed to copy CIAPI.dll: {0}", ex.ToString());
            return;
          }
        }
        IntPtr lib = LoadLibrary("CIAPI" + _apiIndex + ".dll");
        if (lib == IntPtr.Zero || lib == null)
        {
          Log.Log.Debug("Anysee: failed to load the DLL");
          return;
        }

        IntPtr function = GetProcAddress(lib, "CreateDtvCIAPI");
        if (function == IntPtr.Zero)
        {
          Log.Log.Debug("Anysee: failed to locate the CreateDtvCIAPI function");
        }
        try
        {
          _createApi = (CreateDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(CreateDtvCIAPI));
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Anysee: failed to load the CreateDtvCIAPI function: {0}", ex.ToString());
        }

        function = GetProcAddress(lib, "DestroyDtvCIAPI");
        if (function == IntPtr.Zero)
        {
          Log.Log.Debug("Anysee: failed to locate the DestroyDtvCIAPI function");
        }
        try
        {
          _destroyApi = (DestroyDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(DestroyDtvCIAPI));
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Anysee: failed to load the DestroyDtvCIAPI function: {0}", ex.ToString());
        }

        function = GetProcAddress(lib, "GetanyseeNumberofDevicesEx");
        if (function == IntPtr.Zero)
        {
          Log.Log.Debug("Anysee: failed to locate the GetanyseeNumberofDevicesEx function");
        }
        try
        {
          _getAnyseeDeviceCount = (GetanyseeNumberofDevicesEx)Marshal.GetDelegateForFunctionPointer(function, typeof(GetanyseeNumberofDevicesEx));
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Anysee: failed to load the GetanyseeNumberofDevicesEx function: {0}", ex.ToString());
        }

        function = GetProcAddress(lib, "?OpenCILib@CCIAPI@@UAGJPAUHWND__@@H@Z");
        if (function == IntPtr.Zero)
        {
          Log.Log.Debug("Anysee: failed to locate the OpenCILib function");
        }
        try
        {
          _openApi = (OpenCILib)Marshal.GetDelegateForFunctionPointer(function, typeof(OpenCILib));
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Anysee: failed to load the OpenCILib function: {0}", ex.ToString());
        }

        function = GetProcAddress(lib, "?CI_Control@CCIAPI@@UAGJKPAJ0@Z");
        if (function == IntPtr.Zero)
        {
          Log.Log.Debug("Anysee: failed to locate the CI_Control function");
        }
        try
        {
          _ciControl = (CI_Control)Marshal.GetDelegateForFunctionPointer(function, typeof(CI_Control));
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Anysee: failed to load the CI_Control function: {0}", ex.ToString());
        }

        _dllLoaded = true;
      }

      /// <summary>
      /// Open the API.
      /// </summary>
      /// <param name="tunerDevicePath">The tuner device path.</param>
      /// <returns><c>true</c> if the API is successfully opened, otherwise <c>false</c></returns>
      public bool OpenApi(String tunerDevicePath)
      {
        Log.Log.Debug("Anysee: opening API, API index = {0}", _apiIndex);

        if (!_dllLoaded)
        {
          Log.Log.Debug("Anysee: the CIAPI.dll functions were not successfully loaded");
          return false;
        }
        if (_apiAccessThread != null && _apiAccessThread.IsAlive)
        {
          Log.Log.Debug("Anysee: API access thread is already running");
          return false;
        }

        // We only care about the device instance part of the device path
        // because the API provides capture device paths - matching the full
        // device path would not work.
        _devicePath = tunerDevicePath.Split('{')[0];

        _ciApiInstance = Marshal.AllocCoTaskMem(ApiInstanceSize);
        for (int i = 0; i < ApiInstanceSize; i++)
        {
          Marshal.WriteByte(_ciApiInstance, i, 0);
        }
        _windowHandle = Marshal.AllocCoTaskMem(4);

        // Technically all access to the CI API functions should be made
        // from a separate thread because the CIAPI DLL only supports single
        // thread apartment access. TV Server threads use MTA by default.
        // Those two threading models are incompatible. In practise I have
        // determined that it is only necessary to call the "static" DLL
        // functions from an STA thread. That effectively means that we
        // only need an STA thread to open the API and hold it open until
        // it is no longer needed.
        Log.Log.Debug("Anysee: starting API access thread");
        _stopApiAccessThread = false;
        _apiAccessThread = new Thread(new ThreadStart(AccessThread));
        _apiAccessThread.Name = String.Format("Anysee API {0} Access", _apiCount);
        _apiAccessThread.IsBackground = true;
        _apiAccessThread.SetApartmentState(ApartmentState.STA);
        _apiAccessThread.Start();

        Thread.Sleep(500);

        // The thread will terminate almost immediately if there is any
        // problem. If the thread is alive after half a second then
        // everything should be okay.
        if (_apiAccessThread.IsAlive)
        {
          Log.Log.Debug("Anysee: API access thread running");
          return true;
        }
        Log.Log.Debug("Anysee: API access thread self-terminated");
        return false;
      }

      /// <summary>
      /// This is a thread function that actually creates and opens
      /// an Anysee API instance. The API is held open until the thread
      /// is stopped.
      /// </summary>
      private void AccessThread()
      {
        Log.Log.Debug("Anysee: creating new CI API instance");
        int result = _createApi(_ciApiInstance);
        if (result != 1)
        {
          Log.Log.Debug("Anysee: failed to create instance, result = {0}", result);
          return;
        }
        Log.Log.Debug("Anysee: created instance successfully");

        // We have an API instance, but now we need to open it by linking it with hardware.
        Log.Log.Debug("Anysee: determining instance index");
        IntPtr infoBuffer = Marshal.AllocCoTaskMem(CiDeviceInfoSize);
        for (int i = 0; i < CiDeviceInfoSize; i++)
        {
          Marshal.WriteByte(infoBuffer, i, 0);
        }
        int numDevices = _getAnyseeDeviceCount(infoBuffer);
        Log.Log.Debug("Anysee: number of devices = {0}", numDevices);
        if (numDevices == 0)
        {
          Marshal.FreeCoTaskMem(infoBuffer);
          return;
        }
        CiDeviceInfo deviceInfo = (CiDeviceInfo)Marshal.PtrToStructure(infoBuffer, typeof(CiDeviceInfo));
        Marshal.FreeCoTaskMem(infoBuffer);

        String captureDevicePath;
        int index = -1;
        for (int i = 0; i < numDevices; i++)
        {
          captureDevicePath = deviceInfo.DevicePaths[i].Text.Substring(0, deviceInfo.DevicePathLengths[i]);
          Log.Log.Debug("Anysee: device {0}", i + 1);
          Log.Log.Debug("  device path  = {0}", captureDevicePath);
          Log.Log.Debug("  index        = {0}", deviceInfo.DevicePathIndices[i]);
          Log.Log.Debug("  Anysee index = {0}", deviceInfo.DeviceIndices[i]);

          if (captureDevicePath.StartsWith(_devicePath))
          {
            Log.Log.Debug("Anysee: found correct instance");
            index = deviceInfo.DeviceIndices[i];
            break;
          }
        }

        // If we have a valid device index then we can attempt to open the CI API.
        if (index != -1)
        {
          Log.Log.Debug("Anysee: opening CI API");
          result = _openApi(_ciApiInstance, _windowHandle, index);
          if (result == 0)
          {
            Log.Log.Debug("Anysee: result = success");
            // Hold the API open until it is no longer needed.
            while (!_stopApiAccessThread)
            {
              Thread.Sleep(500);
            }
          }
          else
          {
            Log.Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", result, HResult.GetDXErrorString(result));
          }
        }


        Log.Log.Debug("Anysee: destroying CI API instance");
        result = _destroyApi(_ciApiInstance);
        Log.Log.Debug("Anysee: result = {0}", result);
      }

      /// <summary>
      /// Close the API.
      /// </summary>
      /// <returns><c>true</c> if the API is successfully closed, otherwise <c>false</c></returns>
      public bool CloseApi()
      {
        Log.Log.Debug("Anysee: closing API");

        _stopApiAccessThread = true;
        // The access thread wakes every 500 ms, so in the worst case
        // scenario it would take approximately 1 second to cleanly
        // stop the thread.
        Thread.Sleep(1000);

        Marshal.FreeCoTaskMem(_ciApiInstance);
        Marshal.FreeCoTaskMem(_windowHandle);
        return true;
      }

      /// <summary>
      /// Execute a command on an open API instance.
      /// </summary>
      /// <param name="command">The command to execute.</param>
      /// <param name="inputParams">A reference to a buffer containing the appropriate input parameters for the command.</param>
      /// <param name="outputParams">A reference to a buffer that will be filled with the command's output parameters.</param>
      /// <returns><c>true</c> if the command is successfully executed, otherwise <c>false</c></returns>
      public bool ExecuteCommand(AnyseeCiCommand command, IntPtr inputParams, IntPtr outputParams)
      {
        Log.Log.Debug("Anysee: execute API command");
        if (!_apiAccessThread.IsAlive)
        {
          Log.Log.Debug("Anysee: the API is not open");
          return false;
        }
        int hr;
        lock (this)
        {
          hr = _ciControl(_ciApiInstance, command, inputParams, outputParams);
        }
        if (hr == 0)
        {
          Log.Log.Debug("Anysee: result = success");
          return true;
        }

        Log.Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
    }

    #region callbacks

    /// <summary>
    /// Called by the tuner driver when the common interface slot state changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot.</param>
    /// <param name="state">The new CI state.</param>
    /// <param name="message">A short description of the CI state.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate Int32 OnAnyseeCiState(Int32 slotIndex, AnyseeCiState state, [MarshalAs(UnmanagedType.LPStr)] String message);

    /// <summary>
    /// Called by the tuner driver when MMI information is ready to be processed.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="message">The message from the CAM.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully processed</returns>
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    private delegate Int32 OnAnyseeMmiMessage(Int32 slotIndex, IntPtr message);

    #endregion

    #region constants

    private static readonly Guid BdaExtensionPropertySet = new Guid(0xb8e78938, 0x899d, 0x41bd, 0xb5, 0xb4, 0x62, 0x69, 0xf2, 0x80, 0x18, 0x99);

    private const int KsPropertySize = 24;
    private const int DiseqcMessageSize = KsPropertySize + MaxDiseqcMessageLength + 8;
    private const int MaxDiseqcMessageLength = 16;
    private const int PlatformInfoSize = KsPropertySize + 8;
    private const int MaxApiStringLength = 256;
    private const int MaxCamMenuEntries = 32;
    private const int CiStateInfoSize = 8 + MaxApiStringLength;
    private const int MmiMenuSize = (MaxCamMenuEntries * MaxApiStringLength) + 8;
    private const int MmiMessageSize = 32 + MaxApiStringLength;
    private const int MaxDescriptorDataLength = 256;
    private const int MaxPmtElementaryStreams = 50;
    private const int EsPmtDataSize = 260;
    private const int PmtDataSize = 12 + MaxDescriptorDataLength + (MaxPmtElementaryStreams * EsPmtDataSize);

    #endregion

    #region variables

    private bool _isAnysee = false;
    private bool _isCiSlotPresent = false;
    private bool _isCamPresent = false;
    private bool _isCamReady = false;
    private AnyseeCiState _ciState = AnyseeCiState.Empty;

    private IKsPropertySet _propertySet = null;
    private AnyseeCiApi _ciApi = null;

    private IntPtr _generalBuffer = IntPtr.Zero;

    private String _tunerDevicePath = String.Empty;
    private ApiCallbacks _apiCallbacks;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="Anysee"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="tunerDevicePath">The tuner device path.</param>
    public Anysee(IBaseFilter tunerFilter, String tunerDevicePath)
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
      _tunerDevicePath = tunerDevicePath;
      _generalBuffer = Marshal.AllocCoTaskMem(DiseqcMessageSize);
      ReadDeviceInfo();
      _isCiSlotPresent = IsCiSlotPresent();
      // The OnCiState() callback will update these states very quickly
      // after the conditional access interface is opened.
      _isCamPresent = false;
      _isCamReady = false;

      if (_isCiSlotPresent)
      {
        OpenCi();
      }
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
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenCi()
    {
      Log.Log.Debug("Anysee: open conditional access interface");
      _ciApi = new AnyseeCiApi();
      if (!_ciApi.OpenApi(_tunerDevicePath))
      {
        Log.Log.Debug("Anysee: open API failed");
        return false;
      }

      Log.Log.Debug("Anysee: setting callbacks");
      // We need to pass the addresses of our callback functions to
      // the API but C# makes that awkward. The workaround is to set
      // up a callback structure instance, marshal the instance into
      // a block of memory, and then read the addresses from the memory.
      _apiCallbacks = new ApiCallbacks();
      _apiCallbacks.OnCiState = OnCiState;
      _apiCallbacks.OnMmiMessage = OnMmiMessage;
      lock (this)
      {
        Marshal.StructureToPtr(_apiCallbacks, _generalBuffer, true);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.IsOpenSetCallbacks, (IntPtr)Marshal.ReadInt32(_generalBuffer, 0), (IntPtr)Marshal.ReadInt32(_generalBuffer, 4)))
        {
          Log.Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Log.Debug("Anysee: result = failure");
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseCi()
    {
      if (_ciApi == null)
      {
        return true;
      }
      Log.Log.Debug("Anysee: close conditional access interface");
      _isCamPresent = false;
      _isCamReady = false;
      if (_ciApi.CloseApi())
      {
        Log.Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Log.Debug("Anysee: result = failure");
      return false;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph"><c>True</c> if the DirectShow filter graph should be rebuilt after calling this function.</param>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    public bool ResetCi(out bool rebuildGraph)
    {
      rebuildGraph = false;
      return CloseCi() && OpenCi();
    }

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

    /// <summary>
    /// Determines whether a CAM is present or not.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present, otherwise <c>false</c></returns>
    public bool IsCamPresent()
    {
      Log.Log.Debug("Anysee: is CAM present");
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("Anysee: CI slot not present");
        return false;
      }

      // The CAM state is automatically updated in the OnCiState() callback.
      Log.Log.Debug("Anysee: result = {0}", _isCamPresent);
      return _isCamPresent;
    }

    /// <summary>
    /// Determines whether a CAM is present and ready for interaction.
    /// </summary>
    /// <returns><c>true</c> if a CAM is present and ready, otherwise <c>false</c></returns>
    public bool IsCamReady()
    {
      Log.Log.Debug("Anysee: is CAM ready");
      if (!_isCiSlotPresent)
      {
        Log.Log.Debug("Anysee: CI slot not present");
        return false;
      }

      // The CAM state is automatically updated in the OnCiState() callback.
      Log.Log.Debug("Anysee: result = {0}", _isCamReady);
      return _isCamReady;
    }

    /// <summary>
    /// Send PMT to the CAM to request that a service be descrambled.
    /// </summary>
    /// <param name="listAction">A list management action for communication with the CAM.</param>
    /// <param name="command">A decryption command directed to the CAM.</param>
    /// <param name="pmt">The PMT.</param>
    /// <param name="length">The length of the PMT in bytes.</param>
    /// <returns><c>true</c> if the service is successfully descrambled, otherwise <c>false</c></returns>
    public bool SendPmt(ListManagementType listAction, CommandIdType command, byte[] pmt, int length)
    {
      Log.Log.Debug("Anysee: send PMT to CAM, list action = {0}, command = {1}", listAction, command);
      if (!_isCamPresent)
      {
        Log.Log.Debug("Anysee: CAM not available");
        return true;
      }
      if (command == CommandIdType.MMI || command == CommandIdType.Query)
      {
        Log.Log.Debug("Anysee: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null || pmt.Length == 0)
      {
        Log.Log.Debug("Anysee: no PMT");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CommandIdType.NotSelected)
      {
        return true;
      }

      // Anysee tuners only support decrypting one channel at a time. We'll
      // just send this PMT to the CAM regardless of the list management action.
      ChannelInfo info = new ChannelInfo();
      info.DecodePmt(pmt);

      PmtData pmtData = new PmtData();
      pmtData.PmtByte6 = pmt[5];
      pmtData.PcrPid = (UInt16)info.pcrPid;
      pmtData.ServiceId = (UInt16)info.program_number;

      // Descriptor data
      int caDataLength = (UInt16)(((pmt[10] & 0x0f) << 8) + pmt[11]);
      if (caDataLength > MaxDescriptorDataLength - 2)
      {
        Log.Log.Debug("Anysee: program CA data is too long, length = {0}", caDataLength);
        return false;
      }
      pmtData.ProgramDescriptorData = new byte[MaxDescriptorDataLength];
      pmtData.ProgramDescriptorData[0] = pmt[11];
      pmtData.ProgramDescriptorData[1] = (byte)(pmt[10] & 0x0f);
      caDataLength += 2;
      int offset = 12;
      for (int i = 2; i < caDataLength; i++)
      {
        pmtData.ProgramDescriptorData[i] = pmt[offset];
        offset++;
      }

      // Elementary streams
      int esCount = 0;
      pmtData.EsPmt = new EsPmtData[MaxPmtElementaryStreams];
      while (offset < length - 4)
      {
        pmtData.EsPmt[esCount].StreamType = pmt[offset];
        offset++;
        pmtData.EsPmt[esCount].Pid = (UInt16)(((pmt[offset] & 0x1f) << 8) + pmt[offset + 1]);
        offset += 2;
        foreach (PidInfo pid in info.pids)
        {
          if (pid.pid == pmtData.EsPmt[esCount].Pid)
          {
            if (pid.isVideo)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Video;
            }
            else if (pid.isAudio || pid.isAC3Audio || pid.isEAC3Audio)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Audio;
            }
            else if (pid.isDVBSubtitle)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Subtitle;
            }
            else if (pid.isTeletext)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Teletext;
            }
            else
            {
              // We don't differentiate between private and unknown ES types - could
              // this cause problems???
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Unknown;
            }
            break;
          }
        }

        // Descriptor data
        caDataLength = (UInt16)(((pmt[offset] & 0x0f) << 8) + pmt[offset + 1]);
        if (caDataLength > MaxDescriptorDataLength - 2)
        {
          Log.Log.Debug("Anysee: elementary stream {0} CA data is too long, length = {1}", esCount + 1, caDataLength);
          return false;
        }
        pmtData.EsPmt[esCount].DescriptorData = new byte[MaxDescriptorDataLength];
        pmtData.EsPmt[esCount].DescriptorData[0] = pmt[offset + 1];
        pmtData.EsPmt[esCount].DescriptorData[1] = (byte)(pmt[offset] & 0x0f);
        offset += 2;
        caDataLength += 2;
        for (int i = 2; i < caDataLength; i++)
        {
          pmtData.EsPmt[esCount].DescriptorData[i] = pmt[offset];
          offset++;
        }

        esCount++;
      }
      pmtData.EsCount = (UInt16)esCount;

      // Pass the PMT structure to the API.
      IntPtr buffer = Marshal.AllocCoTaskMem(PmtDataSize);
      try
      {
        Marshal.StructureToPtr(pmtData, buffer, true);
        //DVB_MMI.DumpBinary(_mmiMessageBuffer, 0, PmtDataSize);

        if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetPmt, buffer, IntPtr.Zero))
        {
          Log.Log.Debug("Anysee: result = success");
          return true;
        }

        Log.Log.Debug("Anysee: result = failure");
        return false;
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }
    }

    #endregion

    #region callback handlers

    /// <summary>
    /// Called by the tuner driver when the common interface slot state changes.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot.</param>
    /// <param name="state">The new CI state.</param>
    /// <param name="message">A short description of the CI state.</param>
    /// <returns>an HRESULT to indicate whether the state change was successfully handled</returns>
    private Int32 OnCiState(Int32 slotIndex, AnyseeCiState state, String message)
    {
      // If a CAM is inserted the API seems to only invoke this callback when the CAM state
      // changes. However, if a CAM is *not* inserted then this callback is invoked every
      // time the API polls the CI state. We don't want to log the polling - it would swamp
      // the logs.
      if (state == _ciState)
      {
        return 0;
      }

      lock (this)
      {
        Log.Log.Debug("Anysee: CI state change callback, slot = {0}", slotIndex);

        if (state == AnyseeCiState.CamInserted || state == AnyseeCiState.CamOkay)
        {
          _isCamPresent = true;
          _isCamReady = false;
        }
        else if (state == AnyseeCiState.CamReady || state == AnyseeCiState.SendPmtComplete || state == AnyseeCiState.Clear)
        {
          _isCamPresent = true;
          _isCamReady = true;
        }
        else
        {
          _isCamPresent = false;
          _isCamReady = false;
        }
        Log.Log.Debug("  old state = {0}", _ciState);
        Log.Log.Debug("  new state = {0}", state);
        if (String.IsNullOrEmpty(message))
        {
          message = "(no message)";
        }
        Log.Log.Debug("  message   = {0}", message);
        _ciState = state;
      }
      return 0;
    }

    /// <summary>
    /// Called by the tuner driver when MMI information is ready to be processed.
    /// </summary>
    /// <param name="slotIndex">The index of the CI slot containing the CAM.</param>
    /// <param name="message">The message from the CAM.</param>
    /// <returns>an HRESULT to indicate whether the message was successfully processed</returns>
    private Int32 OnMmiMessage(Int32 slotIndex, IntPtr message)
    {
      Log.Log.Debug("Anysee: MMI message callback, slot = {0}", slotIndex);

      MmiMessage msg = (MmiMessage)Marshal.PtrToStructure(message, typeof(MmiMessage));
      Log.Log.Debug("  device index  = {0}", msg.DeviceIndex);
      Log.Log.Debug("  slot index    = {0}", msg.SlotIndex);
      Log.Log.Debug("  menu title    = {0}", msg.RootMenuTitle.Text);
      Log.Log.Debug("  message type  = {0}", msg.Type);
      MmiMenu menu = (MmiMenu)Marshal.PtrToStructure(msg.Menu, typeof(MmiMenu));
      Log.Log.Debug("  string count  = {0}", menu.StringCount);
      Log.Log.Debug("  menu index    = {0}", menu.MenuIndex);


      // Enquiry
      if (msg.Type == AnyseeMmiMessageType.InputRequest)
      {
        Log.Log.Debug("Anysee: enquiry");
        if (msg.HeaderCount != 1)
        {
          Log.Log.Debug("Anysee: unexpected header count, count = {0}", msg.HeaderCount);
          return 1;
        }
        String prompt = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 0));
        Log.Log.Debug("  prompt    = {0}", prompt);
        Log.Log.Debug("  length    = {0}", msg.ExpectedAnswerLength);
        Log.Log.Debug("  key count = {0}", msg.KeyCount);
        try
        {
          if (_ciMenuCallbacks != null)
          {
            _ciMenuCallbacks.OnCiRequest(false, (uint)msg.ExpectedAnswerLength, prompt);
          }
          return 0;
        }
        catch (Exception ex)
        {
          Log.Log.Debug("Anysee: MMI callback enquiry exception: {0}", ex.ToString());
          return 1;
        }
      }

      // Menu or list
      Log.Log.Debug("Anysee: menu");
      if (msg.HeaderCount != 3)
      {
        Log.Log.Debug("Anysee: unexpected header count, count = {0}", msg.HeaderCount);
        return 1;
      }

      String title = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 0));
      String subTitle = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 4));
      String footer = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 8));
      Log.Log.Debug("  title     = {0}", title);
      Log.Log.Debug("  sub-title = {0}", subTitle);
      Log.Log.Debug("  footer    = {0}", footer);
      Log.Log.Debug("  # entries = {0}", msg.EntryCount);
      try
      {
        if (_ciMenuCallbacks != null)
        {
          _ciMenuCallbacks.OnCiMenu(title, subTitle, footer, msg.EntryCount);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Anysee: MMI callback header exception: {0}", ex.ToString());
      }

      try
      {
        String entry;
        int offset = 4 * msg.HeaderCount;
        for (int i = 0; i < msg.EntryCount; i++)
        {
          entry = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, offset + (i * 4)));
          Log.Log.Debug("  entry {0,-2}  = {1}", i + 1, entry);
          if (_ciMenuCallbacks != null)
          {
            _ciMenuCallbacks.OnCiMenuChoice(i, entry);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("Anysee: MMI callback entry exception: {0}", ex.ToString());
      }

      return 0;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Sets the CAM callback handler functions.
    /// </summary>
    /// <param name="ciMenuHandler">A set of callback handler functions.</param>
    /// <returns><c>true</c> if the handlers are set, otherwise <c>false</c></returns>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        _ciMenuCallbacks = ciMenuHandler;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Send key press event information to the CAM. This is the mechanism that
    /// is used for interaction within the CAM menu.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns><c>true</c> if the key code is passed to the CAM successfully, otherwise <c>false</c></returns>
    private bool SendKey(AnyseeCamMenuKey key)
    {
      Log.Log.Debug("Anysee: send key, key = {0}", key);
      if (_ciApi == null)
      {
        Log.Log.Debug("Anysee: the conditional access interface is not open");
        return false;
      }

      lock (this)
      {
        Marshal.WriteInt32(_generalBuffer, (Int32)key);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetKey, _generalBuffer, IntPtr.Zero))
        {
          Log.Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Log.Debug("Anysee: result = failure");
      return false;
    }

    /// <summary>
    /// Sends a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Anysee: enter menu");
      bool result = SendKey(AnyseeCamMenuKey.Exit);
      return result && SendKey(AnyseeCamMenuKey.Menu);
    }

    /// <summary>
    /// Sends a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Anysee: close menu");
      return SendKey(AnyseeCamMenuKey.Exit);
    }

    /// <summary>
    /// Sends a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      Log.Log.Debug("Anysee: select menu entry, choice = {0}", choice);
      if (choice == 0)
      {
        // Going back to the previous menu is not supported.
        return SendKey(AnyseeCamMenuKey.Exit);
      }
      while (choice > 1)
      {
        if (!SendKey(AnyseeCamMenuKey.Down))
        {
          return false;
        }
        choice--;
      }
      return SendKey(AnyseeCamMenuKey.Select);
    }

    /// <summary>
    /// Sends a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (!_isCamPresent)
      {
        return false;
      }
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Log.Debug("Anysee: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      for (int i = 0; i < answer.Length; i++)
      {
        // We can't send anything other than numbers through the Anysee interface.
        int test;
        if (!Int32.TryParse(answer[i].ToString(), out test))
        {
          Log.Log.Debug("Anysee: answer must only contain numeric digits");
          return false;
        }
        if (!SendKey((AnyseeCamMenuKey)test + 1))
        {
          return false;
        }
      }
      return SendKey(AnyseeCamMenuKey.Select);
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

      int hr;
      lock (this)
      {
        Marshal.StructureToPtr(message, _generalBuffer, true);
        DVB_MMI.DumpBinary(_generalBuffer, 0, DiseqcMessageSize);

        hr = _propertySet.Set(BdaExtensionPropertySet, (int)BdaExtensionProperty.Diseqc,
          _generalBuffer, DiseqcMessageSize,
          _generalBuffer, DiseqcMessageSize
        );
      }
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
      if (_isAnysee)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        CloseCi();
      }
    }

    #endregion
  }
}
