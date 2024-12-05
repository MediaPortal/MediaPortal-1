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

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.VFD_Control
{
  internal sealed class DeviceManagementApiDeclarations
  {
    // API declarations relating to device management (SetupDixxx and
    // RegisterDeviceNotification functions).

    // ******************************************************************************
    // API constants
    // ******************************************************************************

    // from dbt.h
    public const int DBT_DEVICEARRIVAL = 0x8000;
    public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    public const int DBT_DEVTYP_DEVICEINTERFACE = 5;
    public const int DBT_DEVTYP_HANDLE = 6;
    public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
    public const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
    public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
    public const int WM_DEVICECHANGE = 0x219;

    // from setupapi.h
    public const short DIGCF_PRESENT = 0x00000002;
    public const short DIGCF_DEVICEINTERFACE = 0x00000010;

    public const int ERROR_NO_MORE_ITEMS = 259;

    // ******************************************************************************
    // Structures and classes for API calls, listed alphabetically
    // ******************************************************************************

    // There are two declarations for the DEV_BROADCAST_DEVICEINTERFACE structure.

    // Use this in the call to RegisterDeviceNotification() and
    // in checking dbch_devicetype in a DEV_BROADCAST_HDR structure.

    // ******************************************************************************
    // API functions, listed alphabetically
    // ******************************************************************************

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern int SetupDiCreateDeviceInfoList(ref Guid ClassGuid, int hwndParent);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern int SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, int DeviceInfoData,
                                                         ref Guid InterfaceClassGuid, int MemberIndex,
                                                         ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, int hwndParent, int Flags);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet,
                                                              ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                              IntPtr DeviceInterfaceDetailData,
                                                              int DeviceInterfaceDetailDataSize, ref int RequiredSize,
                                                              IntPtr DeviceInfoData);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterDeviceNotification(IntPtr Handle);

    [StructLayout(LayoutKind.Sequential)]
    public class DEV_BROADCAST_DEVICEINTERFACE
    {
      #region Fields

      public int dbcc_size;
      public int dbcc_devicetype;
      public int dbcc_reserved;
      public Guid dbcc_classguid;
      public short dbcc_name;

      #endregion
    }

    // Use this to read the dbcc_name string and classguid.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class DEV_BROADCAST_DEVICEINTERFACE_1
    {
      #region Fields

      public int dbcc_size;
      public int dbcc_devicetype;
      public int dbcc_reserved;

      [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
      public byte[] dbcc_classguid;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
      public char[] dbcc_name;

      #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DEV_BROADCAST_HANDLE
    {
      #region Fields

      public int dbch_size;
      public int dbch_devicetype;
      public int dbch_reserved;
      public IntPtr dbch_handle;
      public IntPtr dbch_hdevnotify;
      public Guid dbch_eventguid;
      public int dbch_nameoffset;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.I1)]
      public byte[] dbch_data;
      #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DEV_BROADCAST_HDR
    {
      #region Fields

      public int dbch_size;
      public int dbch_devicetype;
      public int dbch_reserved;

      #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVICE_INTERFACE_DATA
    {
      #region Fields

      public int cbSize;
      public Guid InterfaceClassGuid;
      public int Flags;
      public IntPtr Reserved;

      #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
      #region Fields

      public int cbSize;
      public string DevicePath;

      #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVINFO_DATA
    {
      #region Fields

      public int cbSize;
      public Guid ClassGuid;
      public int DevInst;
      public IntPtr Reserved;

      #endregion
    }
  }
}