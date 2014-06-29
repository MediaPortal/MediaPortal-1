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
using System.Text;

namespace MediaPortal.Common.Utils
{
  /// <summary>
  /// This class is a static class containing definitions for system methods that are used in
  /// various places in MediaPortal.
  /// </summary>
  public static class NativeMethods
  {
    #region cfgmgr32.dll

    /// <summary>
    /// The CM_Get_Device_ID function retrieves the device instance ID for a specified device instance on the local machine.
    /// </summary>
    /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
    /// <param name="Buffer">Address of a buffer to receive a device instance ID string. The required buffer size can be obtained by calling CM_Get_Device_ID_Size, then incrementing the received value to allow room for the string's terminating NULL. </param>
    /// <param name="BufferLen">Caller-supplied length, in characters, of the buffer specified by Buffer.</param>
    /// <param name="ulFlags">Not used, must be zero.</param>
    /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in Cfgmgr32.h.</returns>
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    public static extern uint CM_Get_Device_IDW(uint dnDevInst, StringBuilder Buffer, uint BufferLen, uint ulFlags);

    /// <summary>
    /// The CM_Get_Parent function obtains a device instance handle to the parent node of a specified device node (devnode) in the local machine's device tree.
    /// </summary>
    /// <param name="pdnDevInst">Caller-supplied pointer to the device instance handle to the parent node that this function retrieves. The retrieved handle is bound to the local machine.</param>
    /// <param name="dnDevInst">Caller-supplied device instance handle that is bound to the local machine.</param>
    /// <param name="ulFlags">Not used, must be zero.</param>
    /// <returns>If the operation succeeds, the function returns CR_SUCCESS. Otherwise, it returns one of the CR_-prefixed error codes defined in Cfgmgr32.h.</returns>
    [DllImport("cfgmgr32.dll")]
    public static extern uint CM_Get_Parent(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

    #endregion

    #region cfgmgr32.h

    public const uint MAX_DEVICE_ID_LEN = 200;

    #endregion

    #region hid.dll

    [DllImport("hid.dll")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

    [DllImport("hid.dll", CharSet = CharSet.Unicode)]
    public static extern HidStatus HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

    [DllImport("hid.dll", CharSet = CharSet.Unicode)]
    public static extern HidStatus HidP_GetButtonCaps(HIDP_REPORT_TYPE ReportType, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] NativeMethods.HIDP_BUTTON_CAPS[] ButtonCaps, ref ushort ButtonCapsLength, IntPtr PreparsedData);

    [DllImport("hid.dll")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, out IntPtr PreparsedData);

    [DllImport("hid.dll", CharSet = CharSet.Unicode)]
    public static extern HidStatus HidP_GetUsagesEx(HIDP_REPORT_TYPE ReportType, ushort LinkCollection, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] USAGE_AND_PAGE[] ButtonList, ref uint UsageLength, IntPtr PreparsedData, [MarshalAs(UnmanagedType.LPArray)] byte[] Report, uint ReportLength);

    #endregion

    #region Hidpi.h

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDP_BUTTON_CAPS
    {
      public ushort UsagePage;
      public byte ReportID;
      [MarshalAs(UnmanagedType.I1)]
      public bool IsAlias;
      public ushort BitField;
      public ushort LinkCollection;
      public ushort LinkUsage;
      public ushort LinkUsagePage;
      [MarshalAs(UnmanagedType.I1)]
      public bool IsRange;
      [MarshalAs(UnmanagedType.I1)]
      public bool IsStringRange;
      [MarshalAs(UnmanagedType.I1)]
      public bool IsDesignatorRange;
      [MarshalAs(UnmanagedType.I1)]
      public bool IsAbsolute;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
      public uint[] Reserved;

      public ushort UsageMin;
      public ushort UsageMax;
      public ushort StringMin;
      public ushort StringMax;
      public ushort DesignatorMin;
      public ushort DesignatorMax;
      public ushort DataIndexMin;
      public ushort DataIndexMax;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDP_CAPS
    {
      public ushort Usage;
      public ushort UsagePage;
      public ushort InputReportByteLength;
      public ushort OutputReportByteLength;
      public ushort FeatureReportByteLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
      public ushort[] Reserved;
      public ushort NumberLinkCollectionNodes;
      public ushort NumberInputButtonCaps;
      public ushort NumberInputValueCaps;
      public ushort NumberInputDataIndices;
      public ushort NumberOutputButtonCaps;
      public ushort NumberOutputValueCaps;
      public ushort NumberOutputDataIndices;
      public ushort NumberFeatureButtonCaps;
      public ushort NumberFeatureValueCaps;
      public ushort NumberFeatureDataIndices;
    };

    public enum HIDP_REPORT_TYPE : ushort
    {
      HidP_Input = 0,
      HidP_Output,
      HidP_Feature
    }

    public enum HidStatus : uint
    {
      HIDP_STATUS_SUCCESS = 0x110000,
      HIDP_STATUS_NULL = 0x80110001,
      HIDP_STATUS_INVALID_PREPARSED_DATA = 0xc0110001,
      HIDP_STATUS_INVALID_REPORT_TYPE = 0xc0110002,
      HIDP_STATUS_INVALID_REPORT_LENGTH = 0xc0110003,
      HIDP_STATUS_USAGE_NOT_FOUND = 0xc0110004,
      HIDP_STATUS_VALUE_OUT_OF_RANGE = 0xc0110005,
      HIDP_STATUS_BAD_LOG_PHY_VALUES = 0xc0110006,
      HIDP_STATUS_BUFFER_TOO_SMALL = 0xc0110007,
      HIDP_STATUS_INTERNAL_ERROR = 0xc0110008,
      HIDP_STATUS_I8042_TRANS_UNKNOWN = 0xc0110009,
      HIDP_STATUS_INCOMPATIBLE_REPORT_ID = 0xc011000a,
      HIDP_STATUS_NOT_VALUE_ARRAY = 0xc011000b,
      HIDP_STATUS_IS_VALUE_ARRAY = 0xc011000c,
      HIDP_STATUS_DATA_INDEX_NOT_FOUND = 0xc011000d,
      HIDP_STATUS_DATA_INDEX_OUT_OF_RANGE = 0xc011000e,
      HIDP_STATUS_BUTTON_NOT_PRESSED = 0xc011000f,
      HIDP_STATUS_REPORT_DOES_NOT_EXIST = 0xc0110010,
      HIDP_STATUS_NOT_IMPLEMENTED = 0xc0110020,
      HIDP_STATUS_I8242_TRANS_UNKNOWN = 0xc0110009
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct USAGE_AND_PAGE
    {
      public ushort Usage;
      public ushort UsagePage;
    };

    #endregion

    #region kernel32.dll

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
                                               IntPtr lpInBuffer, uint nInBufferSize,
                                               IntPtr lpOutBuffer, uint nOutBufferSize,
                                               out uint lpBytesReturned, IntPtr lpOverlapped);

    /// <summary>
    /// The FreeLibrary function decrements the reference count of the loaded dynamic-link library (DLL). When the reference count reaches zero, the module is unmapped from the address space of the calling process and the handle is no longer valid.
    /// </summary>
    /// <param name="hLibModule">Handle to the loaded DLL module. The LoadLibrary or GetModuleHandle function returns this handle.</param>
    /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hLibModule);

    /// <summary>
    /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
    /// </summary>
    /// <param name="lpModuleName">The name of the loaded module (either a .dll or .exe file).</param>
    /// <returns>If the function succeeds, the return value is a handle to the module.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    /// <summary>
    /// The GetProcAddress function retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
    /// </summary>
    /// <param name="hModule">Handle to the DLL module that contains the function or variable. The LoadLibrary or GetModuleHandle function returns this handle.</param>
    /// <param name="lpProcName">Pointer to a null-terminated string containing the function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero.</param>
    /// <returns>If the function succeeds, the return value is the address of the exported function or variable.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    /// <summary>
    /// Retrieves the thread identifier of the calling thread.
    /// </summary>
    /// <returns>The return value is the thread identifier of the calling thread.</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern uint GetCurrentThreadId();

    /// <summary>
    /// Determines whether the specified process is running under WOW64.
    /// </summary>
    /// <param name="hProcess">A handle to the process. The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right.</param>
    /// <param name="Wow64Process">A pointer to a value that is set to TRUE if the process is running under WOW64. If the process is running under 32-bit Windows, the value is set to FALSE. If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.</param>
    /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

    /// <summary>
    /// The LoadLibrary function maps the specified executable module into the address space of the calling process.
    /// </summary>
    /// <param name="lpLibFileName">Pointer to a null-terminated string that names the executable module (either a .dll or .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
    /// <returns>If the function succeeds, the return value is a handle to the module.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lpLibFileName);

    /// <summary>
    /// The SetDllDirectory function adds a directory to the search path used to locate DLLs for the application.
    /// </summary>
    /// <param name="PathName">Pointer to a null-terminated string that specifies the directory to be added to the search path.</param>
    /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDllDirectory(string PathName);

    #endregion

    #region Ks.h

    public static readonly uint IOCTL_KS_PROPERTY = CTL_CODE(FileDevice.FILE_DEVICE_KS, 0, Method.METHOD_NEITHER, FileAccess.FILE_ANY_ACCESS);
    public static readonly uint IOCTL_KS_ENABLE_EVENT = CTL_CODE(FileDevice.FILE_DEVICE_KS, 1, Method.METHOD_NEITHER, FileAccess.FILE_ANY_ACCESS);
    public static readonly uint IOCTL_KS_DISABLE_EVENT = CTL_CODE(FileDevice.FILE_DEVICE_KS, 2, Method.METHOD_NEITHER, FileAccess.FILE_ANY_ACCESS);
    public static readonly uint IOCTL_KS_METHOD = CTL_CODE(FileDevice.FILE_DEVICE_KS, 3, Method.METHOD_NEITHER, FileAccess.FILE_ANY_ACCESS);
    public static readonly uint IOCTL_KS_WRITE_STREAM = CTL_CODE(FileDevice.FILE_DEVICE_KS, 4, Method.METHOD_NEITHER, FileAccess.FILE_ANY_ACCESS);
    public static readonly uint IOCTL_KS_READ_STREAM = CTL_CODE(FileDevice.FILE_DEVICE_KS, 5, Method.METHOD_NEITHER, FileAccess.FILE_ANY_ACCESS);
    public static readonly uint IOCTL_KS_RESET_STATE = CTL_CODE(FileDevice.FILE_DEVICE_KS, 6, Method.METHOD_NEITHER, FileAccess.FILE_ANY_ACCESS);

    #endregion

    #region ksproxy.ax

    [DllImport("ksproxy.ax")]
    public static extern int KsSynchronousDeviceControl(IntPtr Handle, uint IoControl,
                                                        IntPtr InBuffer, uint InLength,
                                                        IntPtr OutBuffer, uint OutLength, out uint BytesReturned);

    #endregion

    #region SetupAPI.h

    [Flags]
    public enum DiGetClassFlags : uint
    {
      /// <summary>
      /// Return only the device that is associated with the system default device interface, if one is set, for the specified device interface classes.
      /// </summary>
      DIGCF_DEFAULT = 0x00000001,  // only valid with DIGCF_DEVICEINTERFACE
      /// <summary>
      /// Return only devices that are currently present in a system.
      /// </summary>
      DIGCF_PRESENT = 0x00000002,
      /// <summary>
      /// Return a list of installed devices for all device setup classes or all device interface classes.
      /// </summary>
      DIGCF_ALLCLASSES = 0x00000004,
      /// <summary>
      /// Return only devices that are a part of the current hardware profile.
      /// </summary>
      DIGCF_PROFILE = 0x00000008,
      /// <summary>
      /// Return devices that support device interfaces for the specified device interface classes. This flag must be set in the Flags parameter if the Enumerator parameter specifies a device instance ID.
      /// </summary>
      DIGCF_DEVICEINTERFACE = 0x00000010,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SP_DEVINFO_DATA
    {
      public uint cbSize;
      public Guid ClassGuid;
      public uint DevInst;
      public IntPtr Reserved;
    }

    #endregion

    #region setupapi.dll

    /// <summary>
    /// The SetupDiDestroyDeviceInfoList function deletes a device information set and frees all associated memory.
    /// </summary>
    /// <param name="DeviceInfoSet">A handle to the device information set to delete.</param>
    /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the logged error can be retrieved with a call to GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    /// <summary>
    /// The SetupDiEnumDeviceInfo function returns a SP_DEVINFO_DATA structure that specifies a device information element in a device information set.
    /// </summary>
    /// <param name="DeviceInfoSet">A handle to the device information set for which to return an SP_DEVINFO_DATA structure that represents a device information element.</param>
    /// <param name="MemberIndex">A zero-based index of the device information element to retrieve.</param>
    /// <param name="DeviceInfoData">A pointer to an SP_DEVINFO_DATA structure to receive information about an enumerated device information element. The caller must set DeviceInfoData.cbSize to sizeof(SP_DEVINFO_DATA).</param>
    /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the logged error can be retrieved with a call to GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

    /// <summary>
    /// The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local computer.
    /// </summary>
    /// <param name="ClassGuid">A pointer to the GUID for a device setup class or a device interface class. This pointer is optional and can be NULL. For more information about how to set ClassGuid, see the following Remarks section.</param>
    /// <param name="Enumerator">A pointer to a NULL-terminated string that specifies:
    /// - An identifier (ID) of a Plug and Play (PnP) enumerator. This ID can either be the value's globally unique identifier (GUID) or symbolic name. For example, "PCI" can be used to specify the PCI PnP value. Other examples of symbolic names for PnP values include "USB," "PCMCIA," and "SCSI".
    /// - A PnP device instance ID. When specifying a PnP device instance ID, DIGCF_DEVICEINTERFACE must be set in the Flags parameter.
    /// This pointer is optional and can be NULL. If an enumeration value is not used to select devices, set Enumerator to NULL.
    /// For more information about how to set the Enumerator value, see the following Remarks section.</param>
    /// <param name="hwndParent">A handle to the top-level window to be used for a user interface that is associated with installing a device instance in the device information set. This handle is optional and can be NULL.</param>
    /// <param name="Flags">A variable of type DWORD that specifies control options that filter the device information elements that are added to the device information set. This parameter can be a bitwise OR of zero or more of the following flags. For more information about combining these flags, see the following Remarks section.</param>
    /// <returns>If the operation succeeds, SetupDiGetClassDevs returns a handle to a device information set that contains all installed devices that matched the supplied parameters. If the operation fails, the function returns INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr SetupDiGetClassDevsW(ref Guid ClassGuid, string Enumerator, IntPtr hwndParent, DiGetClassFlags Flags);
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr SetupDiGetClassDevsW(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, DiGetClassFlags Flags);

    /// <summary>
    /// The SetupDiGetDeviceInstanceId function retrieves the device instance ID that is associated with a device information element.
    /// </summary>
    /// <param name="DeviceInfoSet">A handle to the device information set that contains the device information element that represents the device for which to retrieve a device instance ID. </param>
    /// <param name="DeviceInfoData">A pointer to an SP_DEVINFO_DATA structure that specifies the device information element in DeviceInfoSet.</param>
    /// <param name="DeviceInstanceId">A pointer to the character buffer that will receive the NULL-terminated device instance ID for the specified device information element. For information about device instance IDs, see Device Identification Strings.</param>
    /// <param name="DeviceInstanceIdSize">The size, in characters, of the DeviceInstanceId buffer.</param>
    /// <param name="RequiredSize">A pointer to the variable that receives the number of characters required to store the device instance ID.</param>
    /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the logged error can be retrieved by making a call to GetLastError.</returns>
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiGetDeviceInstanceIdW(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, uint DeviceInstanceIdSize, out uint RequiredSize);

    #endregion

    #region user32.dll

    #region raw input

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetRawInputData(IntPtr hRawInput, RawInputDataCommand uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetRawInputDeviceInfoW(IntPtr hDevice, RawInputInfoCommand uiCommand, IntPtr pData, ref uint pcbSize);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetRawInputDeviceList([Out] NativeMethods.RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint uiNumDevices, uint cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevice, uint uiNumDevices, uint cbSize);

    #endregion

    #region windows

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr CreateWindowEx(WindowStyleEx dwExStyle, string lpClassName, string lpWindowName, WindowStyle dwStyle,
                                                int x, int y,
                                                int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
                                                IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr DispatchMessage(ref MSG msg);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetMessage(ref MSG msg, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool PostThreadMessage(uint idThread, WindowsMessage Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage(ref MSG msg);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

    #endregion

    #endregion

    #region WinIoCtl.h

    /// <summary>
    /// From CTL_CODE().
    /// </summary>
    public static uint CTL_CODE(FileDevice DeviceType, uint Function, Method Method, FileAccess Access)
    {
      return ((uint)DeviceType << 16) | ((uint)Access << 14) | (Function << 2) | (uint)Method;
    }

    /// <summary>
    /// Define the access check value for any access
    /// </summary>
    /// <remarks>
    /// The FILE_READ_ACCESS and FILE_WRITE_ACCESS constants are also defined in
    /// ntioapi.h as FILE_READ_DATA and FILE_WRITE_DATA. The values for these
    /// constants *MUST* always be in sync.
    /// </remarks>
    public enum FileAccess
    {
      FILE_ANY_ACCESS = 0,
      /// <remarks>
      /// FILE_SPECIAL_ACCESS is checked by the NT I/O system the same as FILE_ANY_ACCESS.
      /// The file systems, however, may add additional access checks for I/O and FS controls
      /// that use this value.
      /// </remarks>
      FILE_SPECIAL_ACCESS = FILE_ANY_ACCESS,
      FILE_READ_ACCESS = 1,
      FILE_WRITE_ACCESS = 2
    }

    public enum FileDevice
    {
      FILE_DEVICE_BEEP = 0x00000001,
      FILE_DEVICE_CD_ROM = 0x00000002,
      FILE_DEVICE_CD_ROM_FILE_SYSTEM = 0x00000003,
      FILE_DEVICE_CONTROLLER = 0x00000004,
      FILE_DEVICE_DATALINK = 0x00000005,
      FILE_DEVICE_DFS = 0x00000006,
      FILE_DEVICE_DISK = 0x00000007,
      FILE_DEVICE_DISK_FILE_SYSTEM = 0x00000008,
      FILE_DEVICE_FILE_SYSTEM = 0x00000009,
      FILE_DEVICE_INPORT_PORT = 0x0000000a,
      FILE_DEVICE_KEYBOARD = 0x0000000b,
      FILE_DEVICE_MAILSLOT = 0x0000000c,
      FILE_DEVICE_MIDI_IN = 0x0000000d,
      FILE_DEVICE_MIDI_OUT = 0x0000000e,
      FILE_DEVICE_MOUSE = 0x0000000f,
      FILE_DEVICE_MULTI_UNC_PROVIDER = 0x00000010,
      FILE_DEVICE_NAMED_PIPE = 0x00000011,
      FILE_DEVICE_NETWORK = 0x00000012,
      FILE_DEVICE_NETWORK_BROWSER = 0x00000013,
      FILE_DEVICE_NETWORK_FILE_SYSTEM = 0x00000014,
      FILE_DEVICE_NULL = 0x00000015,
      FILE_DEVICE_PARALLEL_PORT = 0x00000016,
      FILE_DEVICE_PHYSICAL_NETCARD = 0x00000017,
      FILE_DEVICE_PRINTER = 0x00000018,
      FILE_DEVICE_SCANNER = 0x00000019,
      FILE_DEVICE_SERIAL_MOUSE_PORT = 0x0000001a,
      FILE_DEVICE_SERIAL_PORT = 0x0000001b,
      FILE_DEVICE_SCREEN = 0x0000001c,
      FILE_DEVICE_SOUND = 0x0000001d,
      FILE_DEVICE_STREAMS = 0x0000001e,
      FILE_DEVICE_TAPE = 0x0000001f,
      FILE_DEVICE_TAPE_FILE_SYSTEM = 0x00000020,
      FILE_DEVICE_TRANSPORT = 0x00000021,
      FILE_DEVICE_UNKNOWN = 0x00000022,
      FILE_DEVICE_VIDEO = 0x00000023,
      FILE_DEVICE_VIRTUAL_DISK = 0x00000024,
      FILE_DEVICE_WAVE_IN = 0x00000025,
      FILE_DEVICE_WAVE_OUT = 0x00000026,
      FILE_DEVICE_8042_PORT = 0x00000027,
      FILE_DEVICE_NETWORK_REDIRECTOR = 0x00000028,
      FILE_DEVICE_BATTERY = 0x00000029,
      FILE_DEVICE_BUS_EXTENDER = 0x0000002a,
      FILE_DEVICE_MODEM = 0x0000002b,
      FILE_DEVICE_VDM = 0x0000002c,
      FILE_DEVICE_MASS_STORAGE = 0x0000002d,
      FILE_DEVICE_SMB = 0x0000002e,
      FILE_DEVICE_KS = 0x0000002f,
      FILE_DEVICE_CHANGER = 0x00000030,
      FILE_DEVICE_SMARTCARD = 0x00000031,
      FILE_DEVICE_ACPI = 0x00000032,
      FILE_DEVICE_DVD = 0x00000033,
      FILE_DEVICE_FULLSCREEN_VIDEO = 0x00000034,
      FILE_DEVICE_DFS_FILE_SYSTEM = 0x00000035,
      FILE_DEVICE_DFS_VOLUME = 0x00000036,
      FILE_DEVICE_SERENUM = 0x00000037,
      FILE_DEVICE_TERMSRV = 0x00000038,
      FILE_DEVICE_KSEC = 0x00000039,
      FILE_DEVICE_FIPS = 0x0000003a,
      FILE_DEVICE_INFINIBAND = 0x0000003b,
      FILE_DEVICE_VMBUS = 0x0000003e,
      FILE_DEVICE_CRYPT_PROVIDER = 0x0000003f,
      FILE_DEVICE_WPD = 0x00000040,
      FILE_DEVICE_BLUETOOTH = 0x00000041,
      FILE_DEVICE_MT_COMPOSITE = 0x00000042,
      FILE_DEVICE_MT_TRANSPORT = 0x00000043,
      FILE_DEVICE_BIOMETRIC = 0x00000044,
      FILE_DEVICE_PMI = 0x00000045
    }

    /// <summary>
    /// Define the method codes for how buffers are passed for I/O and FS controls
    /// </summary>
    public enum Method
    {
      METHOD_BUFFERED = 0,
      METHOD_IN_DIRECT,
      METHOD_OUT_DIRECT,
      METHOD_NEITHER,
      METHOD_DIRECT_TO_HARDWARE = METHOD_IN_DIRECT,
      METHOD_DIRECT_FROM_HARDWARE = METHOD_OUT_DIRECT
    }

    #endregion

    #region WinUser.h

    #region raw input data

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUT
    {
      public RAWINPUTHEADER header;
      public RawInputData data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTHEADER
    {
      public RawInputDeviceType dwType;
      public uint dwSize;
      public IntPtr hDevice;
      public IntPtr wParam;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RawInputData
    {
      [FieldOffset(0)]
      public RAWMOUSE mouse;
      [FieldOffset(0)]
      public RAWKEYBOARD keyboard;
      [FieldOffset(0)]
      public RAWHID hid;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RAWMOUSE
    {
      [FieldOffset(0)]
      public RawInputMouseIndicatorFlag usFlags;

      [FieldOffset(4)]
      public uint ulButtons;
      [FieldOffset(4)]
      public RawInputMouseButtonFlag usButtonFlags;
      [FieldOffset(6)]
      public ushort usButtonData;

      [FieldOffset(8)]
      public uint ulRawButtons;
      [FieldOffset(12)]
      public int lLastX;
      [FieldOffset(16)]
      public int lLastY;
      [FieldOffset(20)]
      public uint ulExtraInformation;
    }

    [Flags]
    public enum RawInputMouseIndicatorFlag : ushort
    {
      MOUSE_MOVE_RELATIVE = 0,
      MOUSE_MOVE_ABSOLUTE = 1,
      MOUSE_VIRTUAL_DESKTOP = 2,
      MOUSE_ATTRIBUTES_CHANGED = 4,
      MOUSE_MOVE_NOCOALESCE = 8 // Vista and newer
    }

    [Flags]
    public enum RawInputMouseButtonFlag : ushort
    {
      RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001,
      RI_MOUSE_LEFT_BUTTON_UP = 0x0002,
      RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0040,
      RI_MOUSE_RIGHT_BUTTON_UP = 0x0080,
      RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010,
      RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020,

      RI_MOUSE_BUTTON_1_DOWN = RI_MOUSE_LEFT_BUTTON_DOWN,
      RI_MOUSE_BUTTON_1_UP = RI_MOUSE_LEFT_BUTTON_UP,
      RI_MOUSE_BUTTON_2_DOWN = RI_MOUSE_RIGHT_BUTTON_DOWN,
      RI_MOUSE_BUTTON_2_UP = RI_MOUSE_RIGHT_BUTTON_UP,
      RI_MOUSE_BUTTON_3_DOWN = RI_MOUSE_MIDDLE_BUTTON_DOWN,
      RI_MOUSE_BUTTON_3_UP = RI_MOUSE_MIDDLE_BUTTON_UP,
      RI_MOUSE_BUTTON_4_DOWN = 0x0040,
      RI_MOUSE_BUTTON_4_UP = 0x0080,
      RI_MOUSE_BUTTON_5_DOWN = 0x0100,
      RI_MOUSE_BUTTON_5_UP = 0x0200,

      RI_MOUSE_WHEEL = 0x400
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWKEYBOARD
    {
      public ushort MakeCode;
      public RawInputKeyboardFlag Flags;
      public ushort Reserved;
      public VirtualKey VKey;
      public uint Message;
      public uint ExtraInformation;
    }

    public enum VirtualKey : ushort
    {
      VK_LBUTTON = 0x01,
      VK_RBUTTON = 0x02,
      VK_CANCEL = 0x03,
      VK_MBUTTON = 0x04,

      // >= 2000
      VK_XBUTTON1 = 0x05,
      VK_XBUTTON2 = 0x06,
      // end >= 2000

      VK_BACK = 0x08,
      VK_TAB = 0x09,

      VK_CLEAR = 0x0C,
      VK_RETURN = 0x0D,

      VK_SHIFT = 0x10,
      VK_CONTROL = 0x11,
      VK_MENU = 0x12,
      VK_PAUSE = 0x13,
      VK_CAPITAL = 0x14,

      VK_KANA = 0x15,
      VK_HANGEUL = 0x15,    // old name - compatibility
      VK_HANGUL = 0x15,
      VK_JUNJA = 0x17,
      VK_FINAL = 0x18,
      VK_HANJA = 0x19,
      VK_KANJI = 0x19,

      VK_ESCAPE = 0x1B,

      VK_CONVERT = 0x1C,
      VK_NONCONVERT = 0x1D,
      VK_ACCEPT = 0x1E,
      VK_MODECHANGE = 0x1F,

      VK_SPACE = 0x20,
      VK_PRIOR = 0x21,
      VK_NEXT = 0x22,
      VK_END = 0x23,
      VK_HOME = 0x24,
      VK_LEFT = 0x25,
      VK_UP = 0x26,
      VK_RIGHT = 0x27,
      VK_DOWN = 0x28,
      VK_SELECT = 0x29,
      VK_PRINT = 0x2A,
      VK_EXECUTE = 0x2B,
      VK_SNAPSHOT = 0x2C,
      VK_INSERT = 0x2D,
      VK_DELETE = 0x2E,
      VK_HELP = 0x2F,

      VK_0 = 0x30,
      VK_1 = 0x31,
      VK_2 = 0x32,
      VK_3 = 0x33,
      VK_4 = 0x34,
      VK_5 = 0x35,
      VK_6 = 0x36,
      VK_7 = 0x37,
      VK_8 = 0x38,
      VK_9 = 0x39,

      VK_A = 0x41,
      VK_B = 0x42,
      VK_C = 0x43,
      VK_D = 0x44,
      VK_E = 0x45,
      VK_F = 0x46,
      VK_G = 0x47,
      VK_H = 0x48,
      VK_I = 0x49,
      VK_J = 0x4a,
      VK_K = 0x4b,
      VK_L = 0x4c,
      VK_M = 0x4d,
      VK_N = 0x4e,
      VK_O = 0x4f,
      VK_P = 0x50,
      VK_Q = 0x51,
      VK_R = 0x52,
      VK_S = 0x53,
      VK_T = 0x54,
      VK_U = 0x55,
      VK_V = 0x56,
      VK_W = 0x57,
      VK_X = 0x58,
      VK_Y = 0x59,
      VK_Z = 0x5a,

      VK_LWIN = 0x5B,
      VK_RWIN = 0x5C,
      VK_APPS = 0x5D,

      VK_SLEEP = 0x5F,

      VK_NUMPAD0 = 0x60,
      VK_NUMPAD1 = 0x61,
      VK_NUMPAD2 = 0x62,
      VK_NUMPAD3 = 0x63,
      VK_NUMPAD4 = 0x64,
      VK_NUMPAD5 = 0x65,
      VK_NUMPAD6 = 0x66,
      VK_NUMPAD7 = 0x67,
      VK_NUMPAD8 = 0x68,
      VK_NUMPAD9 = 0x69,
      VK_MULTIPLY = 0x6A,
      VK_ADD = 0x6B,
      VK_SEPARATOR = 0x6C,
      VK_SUBTRACT = 0x6D,
      VK_DECIMAL = 0x6E,
      VK_DIVIDE = 0x6F,
      VK_F1 = 0x70,
      VK_F2 = 0x71,
      VK_F3 = 0x72,
      VK_F4 = 0x73,
      VK_F5 = 0x74,
      VK_F6 = 0x75,
      VK_F7 = 0x76,
      VK_F8 = 0x77,
      VK_F9 = 0x78,
      VK_F10 = 0x79,
      VK_F11 = 0x7A,
      VK_F12 = 0x7B,
      VK_F13 = 0x7C,
      VK_F14 = 0x7D,
      VK_F15 = 0x7E,
      VK_F16 = 0x7F,
      VK_F17 = 0x80,
      VK_F18 = 0x81,
      VK_F19 = 0x82,
      VK_F20 = 0x83,
      VK_F21 = 0x84,
      VK_F22 = 0x85,
      VK_F23 = 0x86,
      VK_F24 = 0x87,

      VK_NUMLOCK = 0x90,
      VK_SCROLL = 0x91,

      // NEC PC-9800 kbd definitions
      VK_OEM_NEC_EQUAL = 0x92,  // '=' key on numpad

      // Fujitsu/OASYS kbd definitions
      VK_OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
      VK_OEM_FJ_MASSHOU = 0x93, // 'Unregister word' key
      VK_OEM_FJ_TOUROKU = 0x94, // 'Register word' key
      VK_OEM_FJ_LOYA = 0x95,    // 'Left OYAYUBI' key
      VK_OEM_FJ_ROYA = 0x96,    // 'Right OYAYUBI' key
      // end Fujitsu/OASYS

      VK_LSHIFT = 0xA0,
      VK_RSHIFT = 0xA1,
      VK_LCONTROL = 0xA2,
      VK_RCONTROL = 0xA3,
      VK_LMENU = 0xA4,
      VK_RMENU = 0xA5,

      // >= 2000
      VK_BROWSER_BACK = 0xA6,
      VK_BROWSER_FORWARD = 0xA7,
      VK_BROWSER_REFRESH = 0xA8,
      VK_BROWSER_STOP = 0xA9,
      VK_BROWSER_SEARCH = 0xAA,
      VK_BROWSER_FAVORITES = 0xAB,
      VK_BROWSER_HOME = 0xAC,

      VK_VOLUME_MUTE = 0xAD,
      VK_VOLUME_DOWN = 0xAE,
      VK_VOLUME_UP = 0xAF,
      VK_MEDIA_NEXT_TRACK = 0xB0,
      VK_MEDIA_PREV_TRACK = 0xB1,
      VK_MEDIA_STOP = 0xB2,
      VK_MEDIA_PLAY_PAUSE = 0xB3,
      VK_LAUNCH_MAIL = 0xB4,
      VK_LAUNCH_MEDIA_SELECT = 0xB5,
      VK_LAUNCH_APP1 = 0xB6,
      VK_LAUNCH_APP2 = 0xB7,
      // end >= 2000

      VK_OEM_1 = 0xBA,          // ';:' for US
      VK_OEM_PLUS = 0xBB,       // '+' any country
      VK_OEM_COMMA = 0xBC,      // ',' any country
      VK_OEM_MINUS = 0xBD,      // '-' any country
      VK_OEM_PERIOD = 0xBE,     // '.' any country
      VK_OEM_2 = 0xBF,          // '/?' for US
      VK_OEM_3 = 0xC0,          // '`~' for US

      VK_OEM_4 = 0xDB,          //  '[{' for US
      VK_OEM_5 = 0xDC,          //  '\|' for US
      VK_OEM_6 = 0xDD,          //  ']}' for US
      VK_OEM_7 = 0xDE,          //  ''"' for US
      VK_OEM_8 = 0xDF,

      // various extended or enhanced keyboards
      VK_OEM_AX = 0xE1,         //  'AX' key on Japanese AX kbd
      VK_OEM_102 = 0xE2,        //  "<>" or "\|" on RT 102-key kbd.
      VK_ICO_HELP = 0xE3,       //  Help key on ICO
      VK_ICO_00 = 0xE4,         //  00 key on ICO

      VK_PROCESSKEY = 0xE5,     // >= 95/NT 4.0
      VK_ICO_CLEAR = 0xE6,
      VK_PACKET = 0xE7,         // >= 2000

      // Nokia/Ericsson definitions
      VK_OEM_RESET = 0xE9,
      VK_OEM_JUMP = 0xEA,
      VK_OEM_PA1 = 0xEB,
      VK_OEM_PA2 = 0xEC,
      VK_OEM_PA3 = 0xED,
      VK_OEM_WSCTRL = 0xEE,
      VK_OEM_CUSEL = 0xEF,
      VK_OEM_ATTN = 0xF0,
      VK_OEM_FINISH = 0xF1,
      VK_OEM_COPY = 0xF2,
      VK_OEM_AUTO = 0xF3,
      VK_OEM_ENLW = 0xF4,
      VK_OEM_BACKTAB = 0xF5,
      // end Nokia/Ericsson

      VK_ATTN = 0xF6,
      VK_CRSEL = 0xF7,
      VK_EXSEL = 0xF8,
      VK_EREOF = 0xF9,
      VK_PLAY = 0xFA,
      VK_ZOOM = 0xFB,
      VK_NONAME = 0xFC,
      VK_PA1 = 0xFD,
      VK_OEM_CLEAR = 0xFE
    }

    [Flags]
    public enum RawInputKeyboardFlag : ushort
    {
      RI_KEY_MAKE = 0,
      RI_KEY_BREAK = 1,
      RI_KEY_E0 = 2,  // left
      RI_KEY_E1 = 4,  // right
      RI_KEY_TERMSRV_SET_LED = 8,
      RI_KEY_TERMSRV_SHADOW = 16
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWHID
    {
      public uint dwSizeHid;
      public uint dwCount;
      // bRawData[1] - Actually a variable length inline byte array, so can't be marshaled automatically.
    }

    #endregion

    #region raw input device info

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTDEVICELIST
    {
      public IntPtr hDevice;
      public RawInputDeviceType dwType;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RID_DEVICE_INFO
    {
      [FieldOffset(0)]
      public uint cbSize;
      [FieldOffset(4)]
      public RawInputDeviceType dwType;

      [FieldOffset(8)]
      public RID_DEVICE_INFO_MOUSE mouse;
      [FieldOffset(8)]
      public RID_DEVICE_INFO_KEYBOARD keyboard;
      [FieldOffset(8)]
      public RID_DEVICE_INFO_HID hid;
    }

    // usage page 1, usage 2
    [StructLayout(LayoutKind.Sequential)]
    public struct RID_DEVICE_INFO_MOUSE
    {
      public uint dwId;
      public uint dwNumberOfButtons;
      public uint dwSampleRate;
      [MarshalAs(UnmanagedType.Bool)]
      public bool fHasHorizontalWheel;    // Vista and newer
    }

    // usage page 1, usage 6
    [StructLayout(LayoutKind.Sequential)]
    public struct RID_DEVICE_INFO_KEYBOARD
    {
      public uint dwType;                 // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724336%28v=vs.85%29.aspx
      public uint dwSubType;              // OEM specific
      public uint dwKeyboardMode;         // scan code mode; should be 1
      public uint dwNumberOfFunctionKeys;
      public uint dwNumberOfIndicators;
      public uint dwNumberOfKeysTotal;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RID_DEVICE_INFO_HID
    {
      public uint dwVendorId;
      public uint dwProductId;
      public uint dwVersionNumber;
      public ushort usUsagePage;
      public ushort usUsage;
    }

    #endregion

    #region raw input general

    public enum RawInputDeviceType : uint
    {
      RIM_TYPEMOUSE = 0,
      RIM_TYPEKEYBOARD,
      RIM_TYPEHID
    }

    public enum RawInputDataCommand : uint
    {
      RID_INPUT = 0x10000003,
      RID_HEADER = 0x10000005
    }

    public enum RawInputInfoCommand : int
    {
      RIDI_PREPARSEDDATA = 0x20000005,
      RIDI_DEVICENAME = 0x20000007,
      RIDI_DEVICEINFO = 0x2000000b
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTDEVICE
    {
      public ushort usUsagePage;
      public ushort usUsage;
      public RawInputDeviceFlag dwFlags;
      public IntPtr hwndTarget;
    }

    [Flags]
    public enum RawInputDeviceFlag : uint
    {
      RIDEV_REMOVE = 0x00000001,
      RIDEV_EXCLUDE = 0x00000010,
      RIDEV_PAGEONLY = 0x00000020,
      RIDEV_NOLEGACY = (RIDEV_EXCLUDE | RIDEV_PAGEONLY),
      RIDEV_INPUTSINK = 0x00000100,
      RIDEV_CAPTUREMOUSE = 0x00000200,    // mouse no-legacy
      RIDEV_NOHOTKEYS = 0x00000200,       // for keyboard
      RIDEV_APPKEYS = 0x00000400,         // for keyboard

      // Vista and newer
      RIDEV_EXINPUTSINK = 0x00001000,
      RIDEV_DEVNOTIFY = 0x00002000
    }

    #endregion

    #region windows

    [Flags]
    public enum ClassStyle : uint
    {
      CS_VREDRAW = 0x0001,
      CS_HREDRAW = 0x0002,
      CS_DBLCLKS = 0x0008,
      CS_OWNDC = 0x0020,
      CS_CLASSDC = 0x0040,
      CS_PARENTDC = 0x0080,
      CS_NOCLOSE = 0x0200,
      CS_SAVEBITS = 0x0800,
      CS_BYTEALIGNCLIENT = 0x1000,
      CS_BYTEALIGNWINDOW = 0x2000,
      CS_GLOBALCLASS = 0x4000,

      CS_IME = 0x00010000,
      CS_DROPSHADOW = 0x00020000  // XP and newer
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MSG
    {
      public IntPtr hwnd;
      public WindowsMessage message;
      public IntPtr wParam;
      public IntPtr lParam;
      public uint time;
      public int pt_x;
      public int pt_y;
    }

    public enum WindowsMessage : uint
    {
      WM_QUIT = 0x0012,
      WM_INPUT = 0x00ff,
      WM_TIMER = 0x0113
    }

    [Flags]
    public enum WindowStyle : uint
    {
      WS_OVERLAPPED = 0x00000000,
      WS_POPUP = 0x80000000,
      WS_CHILD = 0x40000000,
      WS_MINIMIZE = 0x20000000,
      WS_VISIBLE = 0x10000000,
      WS_DISABLED = 0x08000000,
      WS_CLIPSIBLINGS = 0x04000000,
      WS_CLIPCHILDREN = 0x02000000,
      WS_MAXIMIZE = 0x01000000,
      WS_CAPTION = (WS_BORDER | WS_DLGFRAME),
      WS_BORDER = 0x00800000,
      WS_DLGFRAME = 0x00400000,
      WS_VSCROLL = 0x00200000,
      WS_HSCROLL = 0x00100000,
      WS_SYSMENU = 0x00080000,
      WS_THICKFRAME = 0x00040000,
      WS_GROUP = 0x00020000,
      WS_TABSTOP = 0x00010000,

      WS_MINIMIZEBOX = 0x00020000,
      WS_MAXIMIZEBOX = 0x00010000,

      WS_TILED = WS_OVERLAPPED,
      WS_ICONIC = WS_MINIMIZE,
      WS_SIZEBOX = WS_THICKFRAME,
      WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

      // common styles
      WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX),
      WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU),
      WS_CHILDWINDOW = WS_CHILD
    }

    [Flags]
    public enum WindowStyleEx : uint
    {
      WS_EX_DLGMODALFRAME = 0x00000001,
      WS_EX_NOPARENTNOTIFY = 0x00000004,
      WS_EX_TOPMOST = 0x00000008,
      WS_EX_ACCEPTFILES = 0x00000010,
      WS_EX_TRANSPARENT = 0x00000020,

      // >= W95/NT 4.0
      WS_EX_MDICHILD = 0x00000040,
      WS_EX_TOOLWINDOW = 0x00000080,
      WS_EX_WINDOWEDGE = 0x00000100,
      WS_EX_CLIENTEDGE = 0x00000200,
      WS_EX_CONTEXTHELP = 0x00000400,

      WS_EX_RIGHT = 0x00001000,
      WS_EX_LEFT = 0x00000000,
      WS_EX_RTLREADING = 0x00002000,
      WS_EX_LTRREADING = 0x00000000,
      WS_EX_LEFTSCROLLBAR = 0x00004000,
      WS_EX_RIGHTSCROLLBAR = 0x00000000,

      WS_EX_CONTROLPARENT = 0x00010000,
      WS_EX_STATICEDGE = 0x00020000,
      WS_EX_APPWINDOW = 0x00040000,

      WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
      WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
      // end >= W95/NT 4.0

      // >= 2000
      WS_EX_LAYERED = 0x00080000,

      WS_EX_NOINHERITLAYOUT = 0x00100000,
      WS_EX_LAYOUTRTL = 0x00400000,

      WS_EX_NOACTIVATE = 0x08000000,
      // end >= 2000

      WS_EX_COMPOSITED = 0x02000000   // XP and newer
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASS
    {
      public ClassStyle style;
      public WndProc lpfnWndProc;
      public int cbClsExtra;
      public int cbWndExtra;
      public IntPtr hInstance;
      public IntPtr hIcon;
      public IntPtr hCursor;
      public IntPtr hbrBackground;
      public string lpszMenuName;
      public string lpszClassName;
    }

    public delegate IntPtr WndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);

    #endregion

    #endregion
  }
}