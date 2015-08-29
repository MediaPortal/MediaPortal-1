using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace Win32
{
  /// <summary>
  ///   Provide some utility functions for raw input handling.
  /// </summary>
  public static class RawInput
  {
    /// <summary>
    /// </summary>
    /// <param name="aRawInputHandle"></param>
    /// <param name="aRawInput"></param>
    /// <param name="rawInputBuffer">Caller must free up memory on the pointer using Marshal.FreeHGlobal</param>
    /// <returns></returns>
    public static bool GetRawInputData(IntPtr aRawInputHandle, ref RAWINPUT aRawInput, ref IntPtr rawInputBuffer)
    {
      var success = true;
      rawInputBuffer = IntPtr.Zero;

      try
      {
        uint dwSize = 0;
        var sizeOfHeader = (uint) Marshal.SizeOf(typeof (RAWINPUTHEADER));

        //Get the size of our raw input data.
        Function.GetRawInputData(aRawInputHandle, Const.RID_INPUT, IntPtr.Zero, ref dwSize, sizeOfHeader);

        //Allocate a large enough buffer
        rawInputBuffer = Marshal.AllocHGlobal((int) dwSize);

        //Now read our RAWINPUT data
        if (
          Function.GetRawInputData(aRawInputHandle, Const.RID_INPUT, rawInputBuffer, ref dwSize,
            (uint) Marshal.SizeOf(typeof (RAWINPUTHEADER))) != dwSize)
        {
          return false;
        }

        //Cast our buffer
        aRawInput = (RAWINPUT) Marshal.PtrToStructure(rawInputBuffer, typeof (RAWINPUT));
      }
      catch
      {
        Debug.WriteLine("GetRawInputData failed!");
        success = false;
      }

      return success;
    }

    /// <summary>
    /// </summary>
    /// <param name="aRawInputHandle"></param>
    /// <param name="aUsagePage"></param>
    /// <param name="aUsage"></param>
    /// <returns></returns>
    public static bool GetDeviceInfo(IntPtr hDevice, ref RID_DEVICE_INFO deviceInfo)
    {
      var success = true;
      var deviceInfoBuffer = IntPtr.Zero;
      try
      {
        //Get Device Info
        var deviceInfoSize = (uint) Marshal.SizeOf(typeof (RID_DEVICE_INFO));
        deviceInfoBuffer = Marshal.AllocHGlobal((int) deviceInfoSize);

        var res = Function.GetRawInputDeviceInfo(hDevice, Const.RIDI_DEVICEINFO, deviceInfoBuffer, ref deviceInfoSize);
        if (res <= 0)
        {
          Debug.WriteLine("WM_INPUT could not read device info: " + Marshal.GetLastWin32Error());
          return false;
        }

        //Cast our buffer
        deviceInfo = (RID_DEVICE_INFO) Marshal.PtrToStructure(deviceInfoBuffer, typeof (RID_DEVICE_INFO));
      }
      catch
      {
        Debug.WriteLine("GetRawInputData failed!");
        success = false;
      }
      finally
      {
        //Always executes, prevents memory leak
        Marshal.FreeHGlobal(deviceInfoBuffer);
      }


      return success;
    }

    /// <summary>
    ///   Fetch pre-parsed data corresponding to HID descriptor for the given HID device.
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public static IntPtr GetPreParsedData(IntPtr hDevice)
    {
      uint ppDataSize = 256;
      var result = Function.GetRawInputDeviceInfo(hDevice, Const.RIDI_PREPARSEDDATA, IntPtr.Zero, ref ppDataSize);
      if (result != 0)
      {
        Log.Error("Failed to get raw input pre-parsed data size. res: {0} err: {1}", result, Marshal.GetLastWin32Error());
        return IntPtr.Zero;
      }

      var ppData = Marshal.AllocHGlobal((int) ppDataSize);
      result = Function.GetRawInputDeviceInfo(hDevice, Const.RIDI_PREPARSEDDATA, ppData, ref ppDataSize);
      if (result <= 0)
      {
        Log.Error("Failed to get raw input pre-parsed data. res: {0} err: {1}", result, Marshal.GetLastWin32Error());
        return IntPtr.Zero;
      }
      return ppData;
    }

    /// <summary>
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public static string GetDeviceName(IntPtr device)
    {
      uint deviceNameSize = 256;
      var result = Function.GetRawInputDeviceInfo(device, Const.RIDI_DEVICENAME, IntPtr.Zero, ref deviceNameSize);
      if (result != 0)
      {
        return string.Empty;
      }

      var deviceName = Marshal.AllocHGlobal((int) deviceNameSize * 2); // size is the character count not byte count
      try
      {
        result = Function.GetRawInputDeviceInfo(device, Const.RIDI_DEVICENAME, deviceName, ref deviceNameSize);
        if (result > 0)
        {
          return Marshal.PtrToStringAnsi(deviceName, result - 1); // -1 for NULL termination
        }

        return string.Empty;
      }
      finally
      {
        Marshal.FreeHGlobal(deviceName);
      }
    }
  }
}