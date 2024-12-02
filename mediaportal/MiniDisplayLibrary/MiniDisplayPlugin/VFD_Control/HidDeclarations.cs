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
  internal sealed class HidApiDeclarations
  {
    // API Declarations for communicating with HID-class devices.

    // ******************************************************************************
    // API constants
    // ******************************************************************************

    // from hidpi.h
    // Typedef enum defines a set of integer constants for HidP_Report_Type
    public const ushort HidP_Input = 0;
    public const ushort HidP_Output = 1;
    public const ushort HidP_Feature = 2;

    // ******************************************************************************
    // Structures and classes for API calls, listed alphabetically
    // ******************************************************************************

    // ******************************************************************************
    // API functions, listed alphabetically
    // ******************************************************************************

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_FlushQueue(IntPtr HidDeviceObject);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern int HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetFeature(IntPtr HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetInputReport(IntPtr HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern void HidD_GetHidGuid(out Guid HidGuid);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetNumInputBuffers(IntPtr HidDeviceObject, ref int NumberBuffers);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetFeature(IntPtr HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetNumInputBuffers(IntPtr HidDeviceObject, int NumberBuffers);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetOutputReport(IntPtr HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern int HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern int HidP_GetValueCaps(ushort ReportType, ref byte ValueCaps, ref ushort ValueCapsLength,
                                               IntPtr PreparsedData);

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDD_ATTRIBUTES
    {
      #region Fields

      public int Size;
      public ushort VendorID;
      public ushort ProductID;
      public ushort VersionNumber;

      #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HIDP_CAPS
    {
      #region Fields

      public ushort Usage;
      public ushort UsagePage;
      public ushort InputReportByteLength;
      public ushort OutputReportByteLength;
      public ushort FeatureReportByteLength;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)] public ushort[] Reserved;
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

      #endregion
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct HidP_Range
    {
      public short UsageMin;
      public short UsageMax;
      public short StringMin;
      public short StringMax;
      public short DesignatorMin;
      public short DesignatorMax;
      public short DataIndexMin;
      public short DataIndexMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HidP_NotRange
    {
      public short Usage;
      public short Reserved1;
      public short StringIndex;
      public short Reserved2;
      public short DesignatorIndex;
      public short Reserved3;
      public short DataIndex;
      public short Reserved4;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct HidP_Value_Caps
    {
      [FieldOffset(0)]
      public ushort UsagePage;
      [FieldOffset(2)]
      public byte ReportID;
      [FieldOffset(3), MarshalAs(UnmanagedType.U1)]
      public bool IsAlias;
      [FieldOffset(4)]
      public ushort BitField;
      [FieldOffset(6)]
      public ushort LinkCollection;
      [FieldOffset(8)]
      public ushort LinkUsage;
      [FieldOffset(10)]
      public ushort LinkUsagePage;
      [FieldOffset(12), MarshalAs(UnmanagedType.U1)]
      public bool IsRange; // If IsRange is false, UsageMin is the Usage and UsageMax is unused.
      [FieldOffset(13), MarshalAs(UnmanagedType.U1)]
      public bool IsStringRange; // If IsStringRange is false, StringMin is the string index and StringMax is unused.
      [FieldOffset(14), MarshalAs(UnmanagedType.U1)]
      public bool IsDesignatorRange; // If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.
      [FieldOffset(15), MarshalAs(UnmanagedType.U1)]
      public bool IsAbsolute;
      [FieldOffset(16), MarshalAs(UnmanagedType.U1)]
      public bool HasNull;
      [FieldOffset(17)]
      public byte Reserved;
      [FieldOffset(18)]
      public short BitSize;
      [FieldOffset(20)]
      public short ReportCount;
      [FieldOffset(22)]
      public ushort Reserved2a;
      [FieldOffset(24)]
      public ushort Reserved2b;
      [FieldOffset(26)]
      public ushort Reserved2c;
      [FieldOffset(28)]
      public ushort Reserved2d;
      [FieldOffset(30)]
      public ushort Reserved2e;
      [FieldOffset(32)]
      public int UnitsExp;
      [FieldOffset(36)]
      public int Units;
      [FieldOffset(40)]
      public int LogicalMin;
      [FieldOffset(44)]
      public int LogicalMax;
      [FieldOffset(48)]
      public int PhysicalMin;
      [FieldOffset(52)]
      public int PhysicalMax;

      [FieldOffset(56)]
      public HidP_Range Range;
      [FieldOffset(56)]
      public HidP_NotRange NotRange;

    }
  }
}