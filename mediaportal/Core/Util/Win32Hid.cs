using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Win32
{
  static partial class Function
  {
    [DllImport("hid.dll", CharSet = CharSet.Unicode)]
    public static extern HidStatus HidP_GetUsagesEx(HIDP_REPORT_TYPE ReportType, ushort LinkCollection,
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] USAGE_AND_PAGE[] ButtonList, ref uint UsageLength,
      IntPtr PreparsedData, [MarshalAs(UnmanagedType.LPArray)] byte[] Report, uint ReportLength);

    [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern Boolean HidD_GetManufacturerString(SafeFileHandle HidDeviceObject, StringBuilder Buffer,
      Int32 BufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern Boolean HidD_GetProductString(SafeFileHandle HidDeviceObject, StringBuilder Buffer,
      Int32 BufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern Boolean HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);
  }


  static partial class Macro
  {
  }


  static partial class Const
  {
  }


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

  [StructLayout(LayoutKind.Sequential)]
  public struct HIDD_ATTRIBUTES
  {
    public uint Size;
    public ushort VendorID;
    public ushort ProductID;
    public ushort VersionNumber;
  }
}