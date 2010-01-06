#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

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
    public static extern bool HidD_FlushQueue(int HidDeviceObject);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern int HidD_GetAttributes(int HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetInputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

    [DllImport("hid.dll", PreserveSig = false)]
    public static extern void HidD_GetHidGuid(ref Guid HidGuid);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetNumInputBuffers(int HidDeviceObject, ref int NumberBuffers);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_GetPreparsedData(int HidDeviceObject, ref IntPtr PreparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetFeature(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetNumInputBuffers(int HidDeviceObject, int NumberBuffers);

    [DllImport("hid.dll", SetLastError = true)]
    public static extern bool HidD_SetOutputReport(int HidDeviceObject, ref byte lpReportBuffer, int ReportBufferLength);

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
    public struct HidP_Value_Caps
    {
      #region Fields

      public ushort UsagePage;
      public byte ReportID;
      public int IsAlias;
      public ushort BitField;
      public ushort LinkCollection;
      public ushort LinkUsage;
      public ushort LinkUsagePage;
      public int IsRange; // If IsRange is false, UsageMin is the Usage and UsageMax is unused.
      public int IsStringRange; // If IsStringRange is false, StringMin is the string index and StringMax is unused.

      public int IsDesignatorRange;
      // If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.

      public int IsAbsolute;
      public int HasNull;
      public byte Reserved;
      public ushort BitSize;
      public ushort ReportCount;
      public ushort Reserved2;
      public ushort Reserved3;
      public ushort Reserved4;
      public ushort Reserved5;
      public ushort Reserved6;
      public int LogicalMin;
      public int LogicalMax;
      public int PhysicalMin;
      public int PhysicalMax;
      public ushort UsageMin;
      public ushort UsageMax;
      public ushort StringMin;
      public ushort StringMax;
      public ushort DesignatorMin;
      public ushort DesignatorMax;
      public ushort DataIndexMin;
      public ushort DataIndexMax;

      #endregion
    }
  }
}