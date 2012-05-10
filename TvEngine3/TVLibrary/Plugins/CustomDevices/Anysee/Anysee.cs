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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using TvLibrary;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;
using TvLibrary.Log;

namespace TvEngine
{
  /// <summary>
  /// A class for handling conditional access and DiSEqC for Anysee tuners. Smart card slots are not
  /// supported.
  /// </summary>
  public class Anysee : BaseCustomDevice, IConditionalAccessProvider, ICiMenuActions, IDiseqcController 
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
      ResetHardware = 1107,
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

    [StructLayout(LayoutKind.Sequential)]
    private struct IrData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;
      public bool Enable;
      public Int32 Key;         // bit 8 = repeat flag (0 = repeat), bits 7-0 = key code
    }

    [StructLayout(LayoutKind.Sequential)]
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

    [StructLayout(LayoutKind.Sequential)]
    private struct PlatformInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = KsPropertySize)]
      public byte[] KsProperty;
      public UInt16 Padding1;   // These bits do have a meaning.
      public AnyseePlatform Platform;
      public Int32 Padding2;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ApiString
    {
      #pragma warning disable 0649
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxApiStringLength)]
      public String Text;
      #pragma warning restore 0649
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct CiStateInfo    // tagCIStatus
    {
      public Int32 Size;
      public Int32 DeviceIndex;
      public ApiString Message;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MmiMenu  // MMIStrsBlock
    {
      public Int32 StringCount;
      public Int32 MenuIndex;
      public IntPtr Entries;                // This is a pointer to an array of pointers.
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct ApiCallbacks
    {
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnAnyseeCiState OnCiState;
      [MarshalAs(UnmanagedType.FunctionPtr)]
      public OnAnyseeMmiMessage OnMmiMessage;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PmtData    // DTVCIPMT
    {
      public byte PmtByte6;                     // Byte 6 from the PMT section (PMT version, current next indicator). 
      private byte Padding1;
      public UInt16 PcrPid;
      public UInt16 ServiceId;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDescriptorDataLength)]
      public byte[] ProgramCaDescriptorData;    // The first two bytes should specify the length of the descriptor data.
      private UInt16 Padding2;
      public UInt32 EsCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPmtElementaryStreams)]
      public EsPmtData[] EsPmt;
    }

    [StructLayout(LayoutKind.Sequential)]
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

      [StructLayout(LayoutKind.Sequential)]
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
      private const int ApiAccessThreadSleepTime = 500;   // unit = ms

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
      private IntPtr _libHandle = IntPtr.Zero;

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
        Log.Debug("Anysee: loading API, API index = {0}", _apiIndex);
        if (!File.Exists("Resources\\CIAPI" + _apiIndex + ".dll"))
        {
          try
          {
            File.Copy("Resources\\CIAPI.dll", "Resources\\CIAPI" + _apiIndex + ".dll");
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to copy CIAPI.dll\r\n{0}", ex.ToString());
            return;
          }
        }
        _libHandle = LoadLibrary("Resources\\CIAPI" + _apiIndex + ".dll");
        if (_libHandle == IntPtr.Zero || _libHandle == null)
        {
          Log.Debug("Anysee: failed to load the DLL");
          return;
        }

        try
        {
          IntPtr function = GetProcAddress(_libHandle, "CreateDtvCIAPI");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the CreateDtvCIAPI function");
            return;
          }
          try
          {
            _createApi = (CreateDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(CreateDtvCIAPI));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the CreateDtvCIAPI function\r\n{0}", ex.ToString());
            return;
          }

          function = GetProcAddress(_libHandle, "DestroyDtvCIAPI");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the DestroyDtvCIAPI function");
            return;
          }
          try
          {
            _destroyApi = (DestroyDtvCIAPI)Marshal.GetDelegateForFunctionPointer(function, typeof(DestroyDtvCIAPI));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the DestroyDtvCIAPI function\r\n{0}", ex.ToString());
            return;
          }

          function = GetProcAddress(_libHandle, "GetanyseeNumberofDevicesEx");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the GetanyseeNumberofDevicesEx function");
            return;
          }
          try
          {
            _getAnyseeDeviceCount = (GetanyseeNumberofDevicesEx)Marshal.GetDelegateForFunctionPointer(function, typeof(GetanyseeNumberofDevicesEx));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the GetanyseeNumberofDevicesEx function\r\n", ex.ToString());
            return;
          }

          function = GetProcAddress(_libHandle, "?OpenCILib@CCIAPI@@UAGJPAUHWND__@@H@Z");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the OpenCILib function");
            return;
          }
          try
          {
            _openApi = (OpenCILib)Marshal.GetDelegateForFunctionPointer(function, typeof(OpenCILib));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the OpenCILib function\r\n{0}", ex.ToString());
            return;
          }

          function = GetProcAddress(_libHandle, "?CI_Control@CCIAPI@@UAGJKPAJ0@Z");
          if (function == IntPtr.Zero)
          {
            Log.Debug("Anysee: failed to locate the CI_Control function");
            return;
          }
          try
          {
            _ciControl = (CI_Control)Marshal.GetDelegateForFunctionPointer(function, typeof(CI_Control));
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: failed to load the CI_Control function\r\n{0}", ex.ToString());
            return;
          }

          _dllLoaded = true;
        }
        finally
        {
          if (!_dllLoaded)
          {
            FreeLibrary(_libHandle);
            _libHandle = IntPtr.Zero;
          }
        }
      }

      /// <summary>
      /// Open the API.
      /// </summary>
      /// <param name="tunerDevicePath">The tuner device path.</param>
      /// <returns><c>true</c> if the API is successfully opened, otherwise <c>false</c></returns>
      public bool OpenApi(String tunerDevicePath)
      {
        Log.Debug("Anysee: opening API, API index = {0}", _apiIndex);

        if (!_dllLoaded)
        {
          Log.Debug("Anysee: the CIAPI.dll functions were not successfully loaded");
          return false;
        }
        if (_apiAccessThread != null && _apiAccessThread.IsAlive)
        {
          Log.Debug("Anysee: API access thread is already running");
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
        Log.Debug("Anysee: starting API access thread");
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
          Log.Debug("Anysee: API access thread running");
          return true;
        }
        Log.Debug("Anysee: API access thread self-terminated");
        return false;
      }

      /// <summary>
      /// This is a thread function that actually creates and opens
      /// an Anysee API instance. The API is held open until the thread
      /// is stopped.
      /// </summary>
      private void AccessThread()
      {
        Log.Debug("Anysee: creating new CI API instance");
        int result = _createApi(_ciApiInstance);
        if (result != 1)
        {
          Log.Debug("Anysee: failed to create instance, result = {0}", result);
          return;
        }
        Log.Debug("Anysee: created instance successfully");

        // We have an API instance, but now we need to open it by linking it with hardware.
        Log.Debug("Anysee: determining instance index");
        IntPtr infoBuffer = Marshal.AllocCoTaskMem(CiDeviceInfoSize);
        for (int i = 0; i < CiDeviceInfoSize; i++)
        {
          Marshal.WriteByte(infoBuffer, i, 0);
        }
        int numDevices = _getAnyseeDeviceCount(infoBuffer);
        Log.Debug("Anysee: number of devices = {0}", numDevices);
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
          Log.Debug("Anysee: device {0}", i + 1);
          Log.Debug("  device path  = {0}", captureDevicePath);
          Log.Debug("  index        = {0}", deviceInfo.DevicePathIndices[i]);
          Log.Debug("  Anysee index = {0}", deviceInfo.DeviceIndices[i]);

          if (captureDevicePath.StartsWith(_devicePath))
          {
            Log.Debug("Anysee: found correct instance");
            index = deviceInfo.DeviceIndices[i];
            break;
          }
        }

        // If we have a valid device index then we can attempt to open the CI API.
        if (index != -1)
        {
          Log.Debug("Anysee: opening CI API");
          result = _openApi(_ciApiInstance, _windowHandle, index);
          if (result == 0)
          {
            Log.Debug("Anysee: result = success");
            // Hold the API open until it is no longer needed.
            while (!_stopApiAccessThread)
            {
              Thread.Sleep(ApiAccessThreadSleepTime);
            }
          }
          else
          {
            Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", result, HResult.GetDXErrorString(result));
          }
        }

        // When this thread is stopped, we automatically destroy the API instance.
        Log.Debug("Anysee: destroying CI API instance");
        result = _destroyApi(_ciApiInstance);
        Log.Debug("Anysee: result = {0}", result);
      }

      /// <summary>
      /// Close the API.
      /// </summary>
      /// <returns><c>true</c> if the API is successfully closed, otherwise <c>false</c></returns>
      public bool CloseApi()
      {
        Log.Debug("Anysee: closing API");

        if (!_dllLoaded)
        {
          Log.Debug("Anysee: the CIAPI.dll functions have not been loaded");
          return true;
        }

        // Stop the API access thread.
        if (_apiAccessThread == null)
        {
          Log.Debug("Anysee: API access thread is null");
        }
        else
        {
          Log.Debug("Anysee: API access thread state = {0}", _apiAccessThread.ThreadState);
        }
        _stopApiAccessThread = true;
        // In the worst case scenario it should take approximately
        // twice the thread sleep time to cleanly stop the thread.
        Thread.Sleep(ApiAccessThreadSleepTime * 2);

        // Free memory and close the library.
        if (_ciApiInstance != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(_ciApiInstance);
          _ciApiInstance = IntPtr.Zero;
        }
        if (_windowHandle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(_windowHandle);
          _windowHandle = IntPtr.Zero;
        }
        if (_libHandle != IntPtr.Zero)
        {
          FreeLibrary(_libHandle);
          _libHandle = IntPtr.Zero;
        }
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
        Log.Debug("Anysee: execute API command");

        if (_apiAccessThread == null)
        {
          Log.Debug("Anysee: API access thread is null");
          return false;
        }
        if (!_apiAccessThread.IsAlive)
        {
          Log.Debug("Anysee: the API is not open");
          return false;
        }

        int hr;
        lock (this)
        {
          hr = _ciControl(_ciApiInstance, command, inputParams, outputParams);
        }
        if (hr == 0)
        {
          Log.Debug("Anysee: result = success");
          return true;
        }

        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
    }

    #region callback definitions

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
    private const int ApiCallbackSize = 8;
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
    private IntPtr _callbackBuffer = IntPtr.Zero;
    private IntPtr _pmtBuffer = IntPtr.Zero;

    private String _tunerDevicePath = String.Empty;
    private ApiCallbacks _apiCallbacks;
    private ICiMenuCallbacks _ciMenuCallbacks = null;

    #endregion

    /// <summary>
    /// Attempt to read the device information from the tuner.
    /// </summary>
    private void ReadDeviceInfo()
    {
      Log.Debug("Anysee: read device information");

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
        Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return;
      }

      PlatformInfo info = (PlatformInfo)Marshal.PtrToStructure(_generalBuffer, typeof(PlatformInfo));
      Log.Debug("  platform = {0}", info.Platform);

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
    /// Send key press event information to the CAM. This is the mechanism that
    /// is used for interaction within the CAM menu.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns><c>true</c> if the key code is passed to the CAM successfully, otherwise <c>false</c></returns>
    private bool SendKey(AnyseeCamMenuKey key)
    {
      Log.Debug("Anysee: send key, key = {0}", key);
      if (_ciApi == null)
      {
        Log.Debug("Anysee: the conditional access interface is not open");
        return false;
      }
      if (_isCamReady == false)
      {
        Log.Debug("Anysee: the CAM is not ready");
        return false;
      }

      lock (this)
      {
        Marshal.WriteInt32(_generalBuffer, (Int32)key);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetKey, _generalBuffer, IntPtr.Zero))
        {
          Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

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

      Log.Debug("Anysee: CI state change callback, slot = {0}", slotIndex);

      // Update the CI state variables.
      lock (this)
      {
        Log.Debug("  old state = {0}", _ciState);
        Log.Debug("  new state = {0}", state);
        _ciState = state;
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
      }

      if (String.IsNullOrEmpty(message))
      {
        message = "(no message)";
      }
      Log.Debug("  message   = {0}", message);

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
      Log.Debug("Anysee: MMI message callback, slot = {0}", slotIndex);

      MmiMessage msg = (MmiMessage)Marshal.PtrToStructure(message, typeof(MmiMessage));
      Log.Debug("  device index  = {0}", msg.DeviceIndex);
      Log.Debug("  slot index    = {0}", msg.SlotIndex);
      Log.Debug("  menu title    = {0}", msg.RootMenuTitle.Text);
      Log.Debug("  message type  = {0}", msg.Type);
      MmiMenu menu = (MmiMenu)Marshal.PtrToStructure(msg.Menu, typeof(MmiMenu));
      Log.Debug("  string count  = {0}", menu.StringCount);
      Log.Debug("  menu index    = {0}", menu.MenuIndex);


      // Enquiry
      if (msg.Type == AnyseeMmiMessageType.InputRequest)
      {
        Log.Debug("Anysee: enquiry");
        if (msg.HeaderCount != 1)
        {
          Log.Debug("Anysee: unexpected header count, count = {0}", msg.HeaderCount);
          return 1;
        }
        if (_ciMenuCallbacks == null)
        {
          Log.Debug("Anysee: menu callbacks are not set");
        }

        String prompt = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 0));
        Log.Debug("  prompt    = {0}", prompt);
        Log.Debug("  length    = {0}", msg.ExpectedAnswerLength);
        Log.Debug("  key count = {0}", msg.KeyCount);
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiRequest(false, (uint)msg.ExpectedAnswerLength, prompt);
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: MMI callback enquiry exception\r\n{0}", ex.ToString());
            return 1;
          }
        }
        return 0;
      }

      // Menu or list
      Log.Debug("Anysee: menu");
      if (msg.HeaderCount != 3)
      {
        Log.Debug("Anysee: unexpected header count, count = {0}", msg.HeaderCount);
        return 1;
      }
      if (_ciMenuCallbacks == null)
      {
        Log.Debug("Anysee: menu callbacks are not set");
      }

      String title = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 0));
      String subTitle = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 4));
      String footer = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, 8));
      Log.Debug("  title     = {0}", title);
      Log.Debug("  sub-title = {0}", subTitle);
      Log.Debug("  footer    = {0}", footer);
      Log.Debug("  # entries = {0}", msg.EntryCount);
      if (_ciMenuCallbacks != null)
      {
        try
        {
          _ciMenuCallbacks.OnCiMenu(title, subTitle, footer, msg.EntryCount);
        }
        catch (Exception ex)
        {
          Log.Debug("Anysee: MMI callback header exception\r\n{0}", ex.ToString());
          return 1;
        }
      }

      String entry;
      int offset = 4 * msg.HeaderCount;
      for (int i = 0; i < msg.EntryCount; i++)
      {
        entry = Marshal.PtrToStringAnsi((IntPtr)Marshal.ReadInt32(menu.Entries, offset + (i * 4)));
        Log.Debug("  entry {0,-2}  = {1}", i + 1, entry);
        if (_ciMenuCallbacks != null)
        {
          try
          {
            _ciMenuCallbacks.OnCiMenuChoice(i, entry);
          }
          catch (Exception ex)
          {
            Log.Debug("Anysee: MMI callback entry exception\r\n{0}", ex.ToString());
            return 1;
          }
        }
      }
      return 0;
    }

    #endregion

    #region ICustomDevice members

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
      Log.Debug("Anysee: initialising device");

      if (tunerFilter == null)
      {
        Log.Debug("Anysee: tuner filter is null");
        return false;
      }
      if (String.IsNullOrEmpty(tunerDevicePath))
      {
        Log.Debug("Anysee: tuner device path is not set");
        return false;
      }
      if (_isAnysee)
      {
        Log.Debug("Anysee: device is already initialised");
        return true;
      }

      // We need a reference to the capture filter because that is the filter which
      // actually implements the important property sets.
      IPin captureInputPin;
      IPin tunerOutputPin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
      if (tunerOutputPin == null)
      {
        Log.Debug("Anysee: failed to find the tuner filter output pin");
        return false;
      }
      int hr = tunerOutputPin.ConnectedTo(out captureInputPin);
      DsUtils.ReleaseComObject(tunerOutputPin);
      tunerOutputPin = null;
      if (hr != 0 || captureInputPin == null)
      {
        Log.Debug("Anysee: failed to get the capture filter input pin, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      PinInfo captureInfo;
      hr = captureInputPin.QueryPinInfo(out captureInfo);
      DsUtils.ReleaseComObject(captureInputPin);
      captureInputPin = null;
      if (hr != 0)
      {
        Log.Debug("Anysee: failed to get the capture filter input pin info, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      // Check if the filter supports the property set.
      _propertySet = captureInfo.filter as IKsPropertySet;
      if (_propertySet == null)
      {
        Log.Debug("Anysee: capture filter is not a property set");
        return false;
      }

      KSPropertySupport support;
      hr = _propertySet.QuerySupported(BdaExtensionPropertySet, (int)BdaExtensionProperty.Ir, out support);
      if (hr != 0 || support == 0)
      {
        Log.Debug("Anysee: device does not support the Anysee property set, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        DsUtils.ReleaseComObject(captureInfo.filter);
        captureInfo.filter = null;
        _propertySet = null;
        return false;
      }

      Log.Debug("Anysee: supported device detected");
      _isAnysee = true;
      _tunerDevicePath = tunerDevicePath;
      _generalBuffer = Marshal.AllocCoTaskMem(DiseqcMessageSize);
      return true;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenInterface()
    {
      Log.Debug("Anysee: open conditional access interface");

      if (!_isAnysee)
      {
        Log.Debug("Anysee: device not initialised or interface not supported");
        return false;
      }
      if (_ciApi != null)
      {
        Log.Debug("Anysee: previous interface instance is still open");
        return false;
      }

      // Is a CI slot present? If not, there is no point in opening the interface.
      ReadDeviceInfo();
      if (!_isCiSlotPresent)
      {
        Log.Debug("Anysee: CI slot not present");
        return false;
      }

      _ciApi = new AnyseeCiApi();
      if (!_ciApi.OpenApi(_tunerDevicePath))
      {
        Log.Debug("Anysee: open API failed");
        _ciApi.CloseApi();
        _ciApi = null;
        return false;
      }

      _pmtBuffer = Marshal.AllocCoTaskMem(PmtDataSize);

      Log.Debug("Anysee: setting callbacks");
      // We need to pass the addresses of our callback functions to
      // the API but C# makes that awkward. The workaround is to set
      // up a callback structure instance, marshal the instance into
      // a block of memory, and then read the addresses from the memory.
      _apiCallbacks = new ApiCallbacks();
      _apiCallbacks.OnCiState = OnCiState;
      _apiCallbacks.OnMmiMessage = OnMmiMessage;
      lock (this)
      {
        _callbackBuffer = Marshal.AllocCoTaskMem(ApiCallbackSize);
        Marshal.StructureToPtr(_apiCallbacks, _callbackBuffer, true);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.IsOpenSetCallbacks, (IntPtr)Marshal.ReadInt32(_callbackBuffer, 0), (IntPtr)Marshal.ReadInt32(_callbackBuffer, 4)))
        {
          Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      Log.Debug("Anysee: close conditional access interface");

      bool result = true;
      if (_ciApi != null)
      {
        result = _ciApi.CloseApi();
        _ciApi = null;
      }
      _isCiSlotPresent = false;
      _isCamPresent = false;
      _isCamReady = false;

      if (_pmtBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_pmtBuffer);
        _pmtBuffer = IntPtr.Zero;
      }
      if (_callbackBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_callbackBuffer);
        _callbackBuffer = IntPtr.Zero;
      }

      if (result)
      {
        Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool rebuildGraph)
    {
      rebuildGraph = false;
      return CloseInterface() && OpenInterface();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      Log.Debug("Anysee: is conditional access interface ready");
      if (!_isCiSlotPresent)
      {
        Log.Debug("Anysee: CI slot not present");
        return false;
      }

      // The CAM state is automatically updated in the OnCiState() callback.
      Log.Debug("Anysee: result = {0}", _isCamReady);
      return _isCamReady;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The programme map table entry for the service.</param>
    /// <param name="cat">The conditional access table entry for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, byte[] pmt, byte[] cat)
    {
      Log.Debug("Anysee: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (_ciApi == null)
      {
        Log.Debug("Anysee: the conditional access interface is not open");
        return false;
      }
      if (!_isCamReady)
      {
        Log.Debug("Anysee: the CAM is not ready");
        return false;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        Log.Debug("Anysee: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null || pmt.Length < 12)
      {
        Log.Debug("Anysee: PMT not supplied or too short");
        return true;
      }

      // "Not selected" commands do nothing.
      if (command == CaPmtCommand.NotSelected)
      {
        Log.Debug("Anysee: result = success");
        return true;
      }

      // Anysee tuners only support decrypting one channel at a time. We'll
      // just send this PMT to the CAM regardless of the list management action.
      PmtData pmtData = new PmtData();
      pmtData.PmtByte6 = pmt[5];
      pmtData.PcrPid = (UInt16)((pmt[8] & 0x1f << 8) + pmt[9]);
      pmtData.ServiceId = (UInt16)((pmt[3] << 8) + pmt[4]);

      // Program CA descriptor data
      int programCaDescriptorDataOffset = 2;  // 2 bytes reserved for the data length
      int pmtOffset = 12;
      int pmtProgramInfoEnd = ((pmt[10] & 0x0f) << 8) + pmt[11] + pmtOffset;
      if (pmtProgramInfoEnd > pmt.Length - 4)
      {
        Log.Debug("Anysee: PMT program info length is invalid");
        return false;
      }
      while (pmtOffset + 1 <= pmtProgramInfoEnd)
      {
        int descriptorTag = pmt[pmtOffset];
        int descriptorLength = pmt[pmtOffset + 1];
        if (pmtOffset + 2 + descriptorLength > pmtProgramInfoEnd)
        {
          Log.Debug("Anysee: PMT program descriptor {0} ({1:x}) length is invalid", descriptorTag, descriptorTag);
          return false;
        }
        // We only pass the conditional access descriptors to the interface.
        if (descriptorTag == (byte)DescriptorType.ConditionalAccess)
        {
          if (programCaDescriptorDataOffset + descriptorLength + 2 > MaxDescriptorDataLength)
          {
            Log.Debug("Anysee: PMT program CA data is too long");
            return false;
          }
          Buffer.BlockCopy(pmt, pmtOffset, pmtData.ProgramCaDescriptorData, programCaDescriptorDataOffset, descriptorLength + 2);
          programCaDescriptorDataOffset += descriptorLength + 2;
        }
        pmtOffset += descriptorLength + 2;
      }
      if (pmtOffset != pmtProgramInfoEnd)
      {
        Log.Debug("Anysee: PMT corruption detected");
        return false;
      }

      pmtData.ProgramCaDescriptorData[0] = (byte)((programCaDescriptorDataOffset - 2) & 0xff);
      pmtData.ProgramCaDescriptorData[1] = (byte)(((programCaDescriptorDataOffset - 2) >> 8) & 0xff);

      // Elementary streams
      Log.Debug("Anysee: elementary streams");
      pmtData.EsPmt = new EsPmtData[MaxPmtElementaryStreams];
      int esCount = 0;
      int pmtEnd = ((pmt[1] & 0x0f) << 8) + pmt[2] + 3 - 4;   // = section length + first 3 bytes - CRC length
      if (pmtEnd > pmt.Length - 4)
      {
        Log.Debug("Anysee: PMT section length is invalid");
        return false;
      }
      while (pmtOffset + 4 <= pmtEnd)
      {
        // We want to add each video, audio, subtitle and teletext stream with their corresponding
        // conditional access descriptors. To start with, we don't know what kind of stream this is. If we
        // finish processing the stream type and descriptors and still don't know what type of stream it is
        // then we exclude it.
        pmtData.EsPmt[esCount].EsType = AnyseeEsType.Unknown;
        byte streamType = pmt[pmtOffset];
        int streamPid = ((pmt[pmtOffset + 1] & 0x1f) << 8) + pmt[pmtOffset + 2];
        int esInfoLength = ((pmt[pmtOffset + 3] & 0x0f) << 8) + pmt[pmtOffset + 4];
        pmtOffset += 5;
        if (pmtOffset + esInfoLength > pmtEnd)
        {
          Log.Debug("Anysee: PMT elementary stream info length for PID {0} ({1:x}) is invalid", streamPid, streamPid);
          return false;
        }

        // Can we determine the stream type from the PMT stream type?
        pmtData.EsPmt[esCount].StreamType = streamType;
        if (streamType == (byte)StreamType.Mpeg1Part2Video ||
          streamType == (byte)StreamType.Mpeg2Part2Video ||
          streamType == (byte)StreamType.Mpeg4Part2Video ||
          streamType == (byte)StreamType.Mpeg4Part10Video)
        {
          pmtData.EsPmt[esCount].EsType = AnyseeEsType.Video;
        }
        else if (streamType == (byte)StreamType.Mpeg1Part3Audio ||
          streamType == (byte)StreamType.Mpeg2Part3Audio ||
          streamType == (byte)StreamType.Mpeg2Part7Audio ||
          streamType == (byte)StreamType.Mpeg4Part3Audio ||
          streamType == (byte)StreamType.Ac3Audio ||
          streamType == (byte)StreamType.EnhancedAc3Audio)
        {
          pmtData.EsPmt[esCount].EsType = AnyseeEsType.Audio;
        }

        // Process the elementary stream descriptors.
        pmtData.EsPmt[esCount].DescriptorData = new byte[MaxDescriptorDataLength];
        int esCaDescriptorDataOffset = 2;   // 2 bytes reserved for the data length
        while (esInfoLength >= 2)
        {
          byte descriptorTag = pmt[pmtOffset];
          byte descriptorLength = pmt[pmtOffset + 1];
          if (pmtOffset + descriptorLength + 2 > pmtEnd)
          {
            Log.Debug("Anysee: PMT elementary stream descriptor {0} ({1:x}) length for PID {2} ({3:x}) is invalid", descriptorTag, descriptorTag, streamPid, streamPid);
            return false;
          }

          // If we don't yet know what type of stream this is, check the descriptor type.
          if (pmtData.EsPmt[esCount].EsType == AnyseeEsType.Unknown)
          {
            if (descriptorTag == (byte)DescriptorType.VideoStream ||
              descriptorTag == (byte)DescriptorType.Mpeg4Video ||
              descriptorTag == (byte)DescriptorType.AvcVideo)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Video;
            }
            else if (descriptorTag == (byte)DescriptorType.AudioStream ||
              descriptorTag == (byte)DescriptorType.Mpeg4Audio ||
              descriptorTag == (byte)DescriptorType.Mpeg2AacAudio ||
              descriptorTag == (byte)DescriptorType.Aac ||
              descriptorTag == (byte)DescriptorType.Ac3 ||        // DVB
              descriptorTag == (byte)DescriptorType.Ac3Audio ||   // ATSC
              descriptorTag == (byte)DescriptorType.EnhancedAc3 ||
              descriptorTag == (byte)DescriptorType.Dts)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Audio;
            }
            else if (descriptorTag == (byte)DescriptorType.Subtitling)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Subtitle;
            }
            else if (descriptorTag == (byte)DescriptorType.Teletext)
            {
              pmtData.EsPmt[esCount].EsType = AnyseeEsType.Teletext;
            }
          }

          // We only pass the conditional access descriptors to the interface.
          if (descriptorTag == (byte)DescriptorType.ConditionalAccess)
          {
            if (esCaDescriptorDataOffset + descriptorLength + 2 > MaxDescriptorDataLength)
            {
              Log.Debug("Anysee: PMT elementary stream {0} (0x{1:x}) CA data is too long", streamPid, streamPid);
              return false;
            }
            Buffer.BlockCopy(pmt, pmtOffset, pmtData.EsPmt[esCount].DescriptorData, esCaDescriptorDataOffset, descriptorLength + 2);
            esCaDescriptorDataOffset += descriptorLength + 2;
          }
          esInfoLength -= (descriptorLength + 2);
          pmtOffset += descriptorLength + 2;
        }

        // We finished processing this stream, but do we actually want to keep it?
        if (pmtData.EsPmt[esCount].EsType != AnyseeEsType.Unknown)
        {
          // Yes!
          pmtData.EsPmt[esCount].DescriptorData[0] = (byte)((esCaDescriptorDataOffset - 2) & 0xff);
          pmtData.EsPmt[esCount].DescriptorData[1] = (byte)(((esCaDescriptorDataOffset - 2) >> 8) & 0xff);
          Log.Debug("  including PID {0} (0x{1:x}), stream type = {2} (0x{3:x}), category = {4}", streamPid, streamPid, streamType, streamType, pmtData.EsPmt[esCount].EsType);
          esCount++;
          if (esCount == MaxPmtElementaryStreams)
          {
            Log.Debug("Anysee: reached maximum number of included PIDs");
            break;
          }
        }
        else
        {
          // Nope.
          Log.Debug("  excluding PID {0} (0x{1:x}), stream type = {2} (0x{3:x})", streamPid, streamPid, streamType, streamType);
        }
      }

      Log.Debug("Anysee: total included PIDs = {0}", esCount);
      pmtData.EsCount = (UInt16)esCount;

      lock (this)
      {
        // Pass the PMT structure to the API.
        Marshal.StructureToPtr(pmtData, _pmtBuffer, true);
        //DVB_MMI.DumpBinary(_pmtBuffer, 0, PmtDataSize);
        if (_ciApi.ExecuteCommand(AnyseeCiCommand.SetPmt, _pmtBuffer, IntPtr.Zero))
        {
          Log.Debug("Anysee: result = success");
          return true;
        }
      }

      Log.Debug("Anysee: result = failure");
      return false;
    }

    #endregion

    #region ICiMenuActions members

    /// <summary>
    /// Set the CAM menu callback handler functions.
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
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool EnterCIMenu()
    {
      Log.Debug("Anysee: enter menu");
      bool result = SendKey(AnyseeCamMenuKey.Exit);
      return result && SendKey(AnyseeCamMenuKey.Menu);
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool CloseCIMenu()
    {
      Log.Debug("Anysee: close menu");
      return SendKey(AnyseeCamMenuKey.Exit);
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SelectMenu(byte choice)
    {
      Log.Debug("Anysee: select menu entry, choice = {0}", choice);
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
    /// Send a response from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the request.</param>
    /// <param name="answer">The user's response.</param>
    /// <returns><c>true</c> if the response is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    public bool SendMenuAnswer(bool cancel, String answer)
    {
      if (answer == null)
      {
        answer = String.Empty;
      }
      Log.Debug("Anysee: send menu answer, answer = {0}, cancel = {1}", answer, cancel);

      for (int i = 0; i < answer.Length; i++)
      {
        // We can't send anything other than numbers through the Anysee interface.
        int digit;
        if (!Int32.TryParse(answer[i].ToString(), out digit))
        {
          Log.Debug("Anysee: answer may only contain numeric digits");
          return false;
        }
        if (!SendKey((AnyseeCamMenuKey)(digit + 1)))
        {
          return false;
        }
      }
      return SendKey(AnyseeCamMenuKey.Select);
    }

    #endregion

    #region IDiseqcController members

    /// <summary>
    /// Send a tone/data burst command, and then set the 22 kHz continuous tone state.
    /// </summary>
    /// <remarks>
    /// The Anysee interface does not support directly setting the 22 kHz tone state. The tuning request
    /// LNB frequency parameters can be used to manipulate the tone state appropriately.
    /// </remarks>
    /// <param name="toneBurstState">The tone/data burst command to send, if any.</param>
    /// <param name="tone22kState">The 22 kHz continuous tone state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    public bool SetToneState(ToneBurst toneBurstState, Tone22k tone22kState)
    {
      Log.Debug("Anysee: set tone state, burst = {0}, 22 kHz = {1}", toneBurstState, tone22kState);

      if (!_isAnysee || _propertySet == null)
      {
        Log.Debug("Anysee: device not initialised or interface not supported");
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.MessageLength = 0;
      message.ToneBurst = AnyseeToneBurst.Off;
      if (toneBurstState == ToneBurst.ToneBurst)
      {
        message.ToneBurst = AnyseeToneBurst.ToneBurst;
      }
      else if (toneBurstState == ToneBurst.DataBurst)
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
        Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      return false;
    }

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    public bool SendCommand(byte[] command)
    {
      Log.Debug("Anysee: send DiSEqC command");

      if (!_isAnysee || _propertySet == null)
      {
        Log.Debug("Anysee: interface not supported");
        return false;
      }
      if (command == null || command.Length == 0)
      {
        Log.Debug("Anysee: command not supplied");
        return true;
      }
      if (command.Length > MaxDiseqcMessageLength)
      {
        Log.Debug("Anysee: command too long, length = {0}", command.Length);
        return false;
      }

      DiseqcMessage message = new DiseqcMessage();
      message.Message = new byte[MaxDiseqcMessageLength];
      Buffer.BlockCopy(command, 0, message.Message, 0, command.Length);
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
        Log.Debug("Anysee: result = success");
        return true;
      }

      Log.Debug("Anysee: result = failure, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
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
      CloseInterface();
      if (_propertySet != null)
      {
        DsUtils.ReleaseComObject(_propertySet);
        _propertySet = null;
      }
      if (_generalBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_generalBuffer);
        _generalBuffer = IntPtr.Zero;
      }
      _isAnysee = false;
    }

    #endregion
  }
}
