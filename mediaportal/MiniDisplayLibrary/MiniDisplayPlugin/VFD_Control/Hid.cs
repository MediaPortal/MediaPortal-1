#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using ProcessPlugins.MiniDisplay.VFD_Control;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.MiniDisplayPlugin.VFD_Control
{
  internal class Hid
  {
    // For communicating with HID-class devices.

    // Used in error messages.
    private const string ModuleName = "Hid";

    #region Fields

    internal HidApiDeclarations.HIDP_CAPS Capabilities;
    internal HidApiDeclarations.HIDD_ATTRIBUTES DeviceAttributes;

    #endregion

    /// <summary>
    /// Remove any Input reports waiting in the buffer.
    /// </summary>
    /// <param name="hidHandle">a handle to a device.</param>
    /// <returns>True on success, False on failure.</returns>
    internal static bool FlushQueue(int hidHandle)
    {
      bool Result = false;

      try
      {
        // ***
        // API function: HidD_FlushQueue
        // Purpose: Removes any Input reports waiting in the buffer.
        // Accepts: a handle to the device.
        // Returns: True on success, False on failure.
        // ***
        Result = HidApiDeclarations.HidD_FlushQueue(hidHandle);

        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Debug(Debugging.ResultOfAPICall("HidD_FlushQueue, ReadHandle"));
          Log.Debug("Result = " + Result);
        }
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return Result;
    }

    /// <summary>
    /// Retrieves a structure with information about a device's capabilities.
    /// </summary>
    /// <param name="hidHandle">a handle to a device.</param>
    /// <returns>An HIDP_CAPS structure.</returns>
    internal HidApiDeclarations.HIDP_CAPS GetDeviceCapabilities(int hidHandle)
    {
      byte[] PreparsedDataBytes = new byte[30];
      IntPtr PreparsedDataPointer = new IntPtr();
      byte[] ValueCaps = new byte[1024]; // (the array size is a guess)

      try
      {
        // ***
        // API function: HidD_GetPreparsedData
        // Purpose: retrieves a pointer to a buffer containing information about the device's capabilities.
        // HidP_GetCaps and other API functions require a pointer to the buffer.
        // Requires:
        // A handle returned by CreateFile.
        // A pointer to a buffer.
        // Returns:
        // True on success, False on failure.
        // ***
        HidApiDeclarations.HidD_GetPreparsedData(hidHandle, ref PreparsedDataPointer);

        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Debug(Debugging.ResultOfAPICall("HidD_GetPreparsedData"));
          Log.Debug("");
        }

        // Copy the data at PreparsedDataPointer into a byte array.
        string PreparsedDataString = Convert.ToBase64String(PreparsedDataBytes);


        // ***
        // API function: HidP_GetCaps
        // Purpose: find out a device's capabilities.
        // For standard devices such as joysticks, you can find out the specific
        // capabilities of the device.
        // For a custom device where the software knows what the device is capable of,
        // this call may be unneeded.
        // Accepts:
        // A pointer returned by HidD_GetPreparsedData
        // A pointer to a HIDP_CAPS structure.
        // Returns: True on success, False on failure.
        // ***
        int Result = HidApiDeclarations.HidP_GetCaps(PreparsedDataPointer, ref Capabilities);
        if (Result != 0)
        {
          //  Debug data:
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("HidP_GetCaps"));
            Log.Debug("Data String: " + PreparsedDataString);
            Log.Debug("  Usage: " + Capabilities.Usage.ToString("x"));
            Log.Debug("  Usage Page: " + Capabilities.UsagePage.ToString("x"));
            Log.Debug("  Input Report Byte Length: " + Capabilities.InputReportByteLength);
            Log.Debug("  Output Report Byte Length: " + Capabilities.OutputReportByteLength);
            Log.Debug("  Feature Report Byte Length: " + Capabilities.FeatureReportByteLength);
            Log.Debug("  Number of Link Collection Nodes: " + Capabilities.NumberLinkCollectionNodes);
            Log.Debug("  Number of Input Button Caps: " + Capabilities.NumberInputButtonCaps);
            Log.Debug("  Number of Input Value Caps: " + Capabilities.NumberInputValueCaps);
            Log.Debug("  Number of Input Data Indices: " + Capabilities.NumberInputDataIndices);
            Log.Debug("  Number of Output Button Caps: " + Capabilities.NumberOutputButtonCaps);
            Log.Debug("  Number of Output Value Caps: " + Capabilities.NumberOutputValueCaps);
            Log.Debug("  Number of Output Data Indices: " + Capabilities.NumberOutputDataIndices);
            Log.Debug("  Number of Feature Button Caps: " + Capabilities.NumberFeatureButtonCaps);
            Log.Debug("  Number of Feature Value Caps: " + Capabilities.NumberFeatureValueCaps);
            Log.Debug("  Number of Feature Data Indices: " + Capabilities.NumberFeatureDataIndices);
          }
          // ***
          // API function: HidP_GetValueCaps
          // Purpose: retrieves a buffer containing an array of HidP_ValueCaps structures.
          // Each structure defines the capabilities of one value.
          // This application doesn't use this data.
          // Accepts:
          // A report type enumerator from hidpi.h,
          // A pointer to a buffer for the returned array,
          // The NumberInputValueCaps member of the device's HidP_Caps structure,
          // A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.
          // Returns: True on success, False on failure.
          // ***
          HidApiDeclarations.HidP_GetValueCaps(HidApiDeclarations.HidP_Input, ref ValueCaps[0],
                                               ref Capabilities.NumberInputValueCaps, PreparsedDataPointer);

          if (Settings.Instance.ExtensiveLogging)
            Log.Debug(Debugging.ResultOfAPICall("HidP_GetValueCaps"));

          // (To use this data, copy the ValueCaps byte array into an array of structures.)
          // ***
          // API function: HidD_FreePreparsedData
          // Purpose: frees the buffer reserved by HidD_GetPreparsedData.
          // Accepts: A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.
          // Returns: True on success, False on failure.
          // ***
          HidApiDeclarations.HidD_FreePreparsedData(ref PreparsedDataPointer);

          if (Settings.Instance.ExtensiveLogging)
            Log.Debug(Debugging.ResultOfAPICall("HidD_FreePreparsedData"));
        }
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return Capabilities;
    }

    /// <summary>
    /// Retrieves the number of Input reports the host can store.
    /// </summary>
    /// <param name="hidDeviceObject">a handle to a device</param>
    /// <param name="numberOfInputBuffers">an integer to hold the returned value.</param>
    /// <returns>True on success, False on failure.</returns>
    internal static bool GetNumberOfInputBuffers(int hidDeviceObject, ref int numberOfInputBuffers)
    {
      bool Success = false;

      try
      {
        if (!(IsWindows98Gold()))
        {
          // ***
          // API function: HidD_GetNumInputBuffers
          // Purpose: retrieves the number of Input reports the host can store.
          // Not supported by Windows 98 Gold.
          // If the buffer is full and another report arrives, the host drops the
          // oldest report.
          // Accepts: a handle to a device and an integer to hold the number of buffers.
          // Returns: True on success, False on failure.
          // ***
          Success = HidApiDeclarations.HidD_GetNumInputBuffers(hidDeviceObject, ref numberOfInputBuffers);
        }
        else
        {
          // Under Windows 98 Gold, the number of buffers is fixed at 2.
          numberOfInputBuffers = 2;
          Success = true;
        }
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return Success;
    }

    /// <summary>
    /// sets the number of input reports the host will store.
    /// Requires Windows XP or later.
    /// </summary>
    /// <param name="hidDeviceObject">a handle to the device.</param>
    /// <param name="numberBuffers">the requested number of input reports.</param>
    /// <returns>True on success. False on failure.</returns>
    internal static bool SetNumberOfInputBuffers(int hidDeviceObject, int numberBuffers)
    {
      bool Success = false;

      try
      {
        if (!(IsWindows98Gold()))
        {
          // ***
          // API function: HidD_SetNumInputBuffers
          // Purpose: Sets the number of Input reports the host can store.
          // If the buffer is full and another report arrives, the host drops the
          // oldest report.
          // Requires:
          // A handle to a HID
          // An integer to hold the number of buffers.
          // Returns: true on success, false on failure.
          // ***
          Success = HidApiDeclarations.HidD_SetNumInputBuffers(hidDeviceObject, numberBuffers);
        }
        else
        {
          // Not supported under Windows 98 Gold.
          Success = false;
        }
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return Success;
    }

    /// <summary>
    /// Find out if the current operating system is Windows XP or later.
    /// (Windows XP or later is required for HidD_GetInputReport and HidD_SetInputReport.
    /// </summary>
    internal static bool IsWindowsXpOrLater()
    {
      bool Success = false;

      try
      {
        OperatingSystem MyEnvironment = Environment.OSVersion;

        // Windows XP is version 5.1.
        Version VersionXP = new Version(5, 1);

        if (MyEnvironment.Version >= VersionXP)
        {
          Debug.Write("The OS is Windows XP or later.");
          Success = true;
        }
        else
        {
          Debug.Write("The OS is earlier than Windows XP.");
          Success = false;
        }
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return Success;
    }

    /// <summary>
    /// Find out if the current operating system is Windows 98 Gold (original version).
    /// Windows 98 Gold does not support the following:
    ///  Interrupt OUT transfers (WriteFile uses control transfers and Set_Report).
    ///   HidD_GetNumInputBuffers and HidD_SetNumInputBuffers
    /// (Not yet tested on a Windows 98 Gold system.)
    /// </summary>
    internal static bool IsWindows98Gold()
    {
      // 
      bool Success = false;

      try
      {
        OperatingSystem MyEnvironment = Environment.OSVersion;

        // Windows 98 Gold is version 4.10 with a build number less than 2183.
        Version Version98SE = new Version(4, 10, 2183);

        if (MyEnvironment.Version >= Version98SE)
        {
          Debug.Write("The OS is Windows 98 Gold.");
          Success = true;
        }
        else
        {
          Debug.Write("The OS is more recent than Windows 98 Gold.");
          Success = false;
        }
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return Success;
    }

    /// <summary>
    /// Provides a central mechanism for exception handling.
    /// logs the message to the MediaPortal Logger
    /// </summary>
    /// <param name="moduleName">the module where the exception occurred.</param>
    /// <param name="e">the exception</param>
    public static void HandleException(string moduleName, Exception e)
    {
      Log.Error("Exception: " + e.Message + Environment.NewLine + "Module: " + moduleName + Environment.NewLine +
                "Method: " + e.TargetSite.Name);
    }

    internal abstract class DeviceReport
    {
      // For reports that the device sends to the host.

      #region Fields

      //internal int HIDHandle;
      //internal bool MyDeviceDetected;
      internal int Result;
      //internal int ReadHandle;

      #endregion

      // Each DeviceReport class defines a ProtectedRead method for reading a type of report.
      // ProtectedRead and Read are declared as Subs rather than as Functions because
      // asynchronous reads use a callback method that can access parameters passed by ByRef
      // but not Function return values.

      protected abstract void ProtectedRead(int readHandle, int hidHandle, ref bool myDeviceDetected,
                                            ref byte[] readBuffer, ref bool success);

      /// <summary>
      /// Calls the overridden ProtectedRead routine.
      /// Enables other classes to override ProtectedRead
      /// while limiting access as Friend.
      /// (Directly declaring Write as Friend MustOverride causes the
      /// compiler warning : "Other languages may permit Friend
      /// Overridable members to be overridden.")
      /// </summary>
      /// <param name="readHandle">a handle for reading from the device.</param>
      /// <param name="hidHandle">a handle for other device communications.</param>
      /// <param name="myDeviceDetected">tells whether the device is currently attached and communicating.</param>
      /// <param name="readBuffer">a byte array to hold the report ID and report data.</param>
      /// <param name="success">read success</param>
      internal void Read(int readHandle, int hidHandle, ref bool myDeviceDetected, ref byte[] readBuffer,
                         ref bool success)
      {
        try
        {
          // Request the report.
          ProtectedRead(readHandle, hidHandle, ref myDeviceDetected, ref readBuffer, ref success);
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }
      }
    }


    /// <summary>
    /// For reading Feature reports.
    /// </summary>
    internal class InFeatureReport : DeviceReport
    {
      /// <summary>
      /// Reads a Feature report from the device.
      /// </summary>
      /// <param name="readHandle">the handle for reading from the device.</param>
      /// <param name="hidHandle">the handle for other device communications.</param>
      /// <param name="myDeviceDetected">tells whether the device is currently attached.</param>
      /// <param name="inFeatureReportBuffer">contains the requested report.</param>
      /// <param name="success">read success</param>
      protected override void ProtectedRead(int readHandle, int hidHandle, ref bool myDeviceDetected,
                                            ref byte[] inFeatureReportBuffer, ref bool success)
      {
        try
        {
          // ***
          // API function: HidD_GetFeature
          // Attempts to read a Feature report from the device.
          // Requires:
          // A handle to a HID
          // A pointer to a buffer containing the report ID and report
          // The size of the buffer.
          // Returns: true on success, false on failure.
          // ***
          success =
            HidApiDeclarations.HidD_GetFeature(hidHandle, ref inFeatureReportBuffer[0], inFeatureReportBuffer.Length);
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("ReadFile"));
            Log.Debug("");
          }
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }
      }
    }

    /// <summary>
    /// For reading Input reports.
    /// </summary>
    internal class InputReport : DeviceReport
    {
      #region Fields

      private bool ReadyForOverlappedTransfer; //  initialize to false

      #endregion

      /// <summary>
      /// closes open handles to a device.
      /// </summary>
      /// <param name="readHandle">the handle for reading from the device.</param>
      /// <param name="hidHandle">the handle for other device communications.</param>
      internal void CancelTransfer(int readHandle, int hidHandle)
      {
        try
        {
          // ***
          // API function: CancelIo
          // Purpose: Cancels a call to ReadFile
          // Accepts: the device handle.
          // Returns: True on success, False on failure.
          // ***
          Result = FileIOApiDeclarations.CancelIo(readHandle);
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug("************ReadFile error*************");
            Log.Debug(Debugging.ResultOfAPICall("CancelIo"));
            Log.Debug("");
          }
          // The failure may have been because the device was removed,
          // so close any open handles and
          // set myDeviceDetected=False to cause the application to
          // look for the device on the next attempt.
          if (hidHandle != 0)
          {
            FileIOApiDeclarations.CloseHandle(hidHandle);
            if (Settings.Instance.ExtensiveLogging)
            {
              Log.Debug(Debugging.ResultOfAPICall("CloseHandle (HIDHandle)"));
              Log.Debug("");
            }
          }

          if (hidHandle != 0)
          {
            FileIOApiDeclarations.CloseHandle(readHandle);
            if (Settings.Instance.ExtensiveLogging)
            {
              Log.Debug(Debugging.ResultOfAPICall("CloseHandle (ReadHandle)"));
            }
          }
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }
      }


      /// <summary>
      /// Creates an event object for the overlapped structure used with ReadFile.
      /// Called before the first call to ReadFile.
      /// </summary>
      /// <param name="hidOverlapped"></param>
      /// <param name="eventObject"></param>
      internal void PrepareForOverlappedTransfer(ref FileIOApiDeclarations.OVERLAPPED hidOverlapped, ref int eventObject)
      {
        FileIOApiDeclarations.SECURITY_ATTRIBUTES Security = new FileIOApiDeclarations.SECURITY_ATTRIBUTES();

        try
        {
          // Values for the SECURITY_ATTRIBUTES structure:
          Security.lpSecurityDescriptor = 0;
          Security.bInheritHandle = Convert.ToInt32(true);
          Security.nLength = Marshal.SizeOf(Security);

          // ***
          // API function: CreateEvent
          // Purpose: Creates an event object for the overlapped structure used with ReadFile.
          // Accepts:
          // A security attributes structure.
          // Manual Reset = False (The system automatically resets the state to nonsignaled
          // after a waiting thread has been released.)
          // Initial state = True (signaled)
          // An event object name (optional)
          // Returns: a handle to the event object
          // ***
          eventObject =
            FileIOApiDeclarations.CreateEvent(ref Security, Convert.ToInt32(false), Convert.ToInt32(true), "");
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("CreateEvent"));
            Log.Debug("");
          }
          // Set the members of the overlapped structure.
          hidOverlapped.Offset = 0;
          hidOverlapped.OffsetHigh = 0;
          hidOverlapped.hEvent = eventObject;
          ReadyForOverlappedTransfer = true;
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }
      }


      /// <summary>
      /// reads an Input report from the device using interrupt transfers.
      /// </summary>
      /// <param name="readHandle">the handle for reading from the device.</param>
      /// <param name="hidHandle">the handle for other device communications.</param>
      /// <param name="myDeviceDetected">tells whether the device is currently attached.</param>
      /// <param name="inputReportBuffer">contains the requested report.</param>
      /// <param name="success">read success</param>
      protected override void ProtectedRead(int readHandle, int hidHandle, ref bool myDeviceDetected,
                                            ref byte[] inputReportBuffer, ref bool success)
      {
        int EventObject = 0;
        FileIOApiDeclarations.OVERLAPPED HIDOverlapped = new FileIOApiDeclarations.OVERLAPPED();
        int NumberOfBytesRead = 0;

        try
        {
          // If it's the first attempt to read, set up the overlapped structure for ReadFile.
          if (!ReadyForOverlappedTransfer)
          {
            PrepareForOverlappedTransfer(ref HIDOverlapped, ref EventObject);
          }

          // ***
          // API function: ReadFile
          // Purpose: Attempts to read an Input report from the device.
          // Accepts:
          // A device handle returned by CreateFile
          // (for overlapped I/O, CreateFile must have been called with FILE_FLAG_OVERLAPPED),
          // A pointer to a buffer for storing the report.
          // The Input report length in bytes returned by HidP_GetCaps,
          // A pointer to a variable that will hold the number of bytes read.
          // An overlapped structure whose hEvent member is set to an event object.
          // Returns: the report in ReadBuffer.
          // The overlapped call returns immediately, even if the data hasn't been received yet.
          // To read multiple reports with one ReadFile, increase the size of ReadBuffer
          // and use NumberOfBytesRead to determine how many reports were returned.
          // Use a larger buffer if the application can't keep up with reading each report
          // individually.
          // ***
          if (Settings.Instance.ExtensiveLogging)
            Log.Debug("input report length = " + inputReportBuffer.Length);

          FileIOApiDeclarations.ReadFile(readHandle, ref inputReportBuffer[0], inputReportBuffer.Length,
                                         ref NumberOfBytesRead, ref HIDOverlapped);
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("ReadFile"));
            Log.Debug("");
            Log.Debug("waiting for ReadFile");
          }
          // API function: WaitForSingleObject
          // Purpose: waits for at least one report or a timeout.
          // Used with overlapped ReadFile.
          // Accepts:
          // An event object created with CreateEvent
          // A timeout value in milliseconds.
          // Returns: A result code.

          Result = FileIOApiDeclarations.WaitForSingleObject(EventObject, 3000);
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("WaitForSingleObject"));
            Log.Debug("");
          }

          // Find out if ReadFile completed or timeout.
          switch (Result)
          {
            case FileIOApiDeclarations.WAIT_OBJECT_0:
              // ReadFile has completed
              success = true;
              if (Settings.Instance.ExtensiveLogging)
                Log.Debug("ReadFile completed successfully.");
              break;

            case FileIOApiDeclarations.WAIT_TIMEOUT:
              // Cancel the operation on timeout
              CancelTransfer(readHandle, hidHandle);
              if (Settings.Instance.ExtensiveLogging)
                Log.Debug("Readfile timeout");
              success = false;
              myDeviceDetected = false;
              break;

            default:
              // Cancel the operation on other error.
              CancelTransfer(readHandle, hidHandle);
              if (Settings.Instance.ExtensiveLogging)
                Log.Debug("Readfile undefined error");
              success = false;
              myDeviceDetected = false;
              break;
          }

          success = (Result == 0) ? true : false;
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }
      }
    }

    internal class InputReportViaControlTransfer : DeviceReport
    {
      /// <summary>
      /// reads an Input report from the device using a control transfer.
      /// </summary>
      /// <param name="readHandle">the handle for reading from the device.</param>
      /// <param name="hidHandle">the handle for other device communications.</param>
      /// <param name="myDeviceDetected">tells whether the device is currently attached.</param>
      /// <param name="inputReportBuffer">contains the requested report.</param>
      /// <param name="success">read success</param>
      protected override void ProtectedRead(int readHandle, int hidHandle, ref bool myDeviceDetected,
                                            ref byte[] inputReportBuffer, ref bool success)
      {
        try
        {
          // ***
          // API function: HidD_GetInputReport
          // Purpose: Attempts to read an Input report from the device using a control transfer.
          // Supported under Windows XP and later only.
          // Requires:
          // A handle to a HID
          // A pointer to a buffer containing the report ID and report
          // The size of the buffer.
          // Returns: true on success, false on failure.
          // ***

          success =
            HidApiDeclarations.HidD_GetInputReport(hidHandle, ref inputReportBuffer[0], inputReportBuffer.Length);

          if (Settings.Instance.ExtensiveLogging)
            Log.Debug(Debugging.ResultOfAPICall("ReadFile"));
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }
      }
    }

    /// <summary>
    /// For reports the host sends to the device.
    /// Each report class defines a ProtectedWrite method for writing a type of report.
    /// </summary>
    internal abstract class HostReport
    {
      protected abstract bool ProtectedWrite(int deviceHandle, byte[] reportBuffer);

      /// <summary>
      /// Calls the overridden ProtectedWrite routine.
      /// This method enables other classes to override ProtectedWrite
      /// while limiting access as Friend.
      /// (Directly declaring Write as Friend MustOverride causes the
      /// compiler(warning) "Other languages may permit Friend
      /// Overridable members to be overridden.")
      /// </summary>
      /// <param name="reportBuffer">contains the report ID and report data.</param>
      /// <param name="deviceHandle">handle to the device.</param>
      /// <returns>True on success. False on failure.</returns>
      internal bool Write(byte[] reportBuffer, int deviceHandle)
      {
        bool Success = false;

        try
        {
          Success = ProtectedWrite(deviceHandle, reportBuffer);
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }
        //Thread.Sleep(20);
        return Success;
      }
    }

    /// <summary>
    /// For Feature reports the host sends to the device.
    /// </summary>
    internal class OutFeatureReport : HostReport
    {
      /// <summary>
      /// writes a Feature report to the device.
      /// </summary>
      /// <param name="hidHandle">a handle to the device.</param>
      /// <param name="outFeatureReportBuffer">contains the report ID and report to send.</param>
      /// <returns>True on success. False on failure.</returns>
      protected override bool ProtectedWrite(int hidHandle, byte[] outFeatureReportBuffer)
      {
        bool Success = false;

        try
        {
          // ***
          // API function: HidD_SetFeature
          // Purpose: Attempts to send a Feature report to the device.
          // Accepts:
          // A handle to a HID
          // A pointer to a buffer containing the report ID and report
          // The size of the buffer.
          // Returns: true on success, false on failure.
          // ***
          Success =
            HidApiDeclarations.HidD_SetFeature(hidHandle, ref outFeatureReportBuffer[0], outFeatureReportBuffer.Length);
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("Hidd_SetFeature"));
            Log.Debug(" FeatureReportByteLength = " + outFeatureReportBuffer.Length);
            Log.Debug(" Report ID: " + outFeatureReportBuffer[0]);
            Log.Debug(" Report Data:");
            for (int i = 0; i < outFeatureReportBuffer.Length; i++)
            {
              Log.Debug(" " + outFeatureReportBuffer[i].ToString("x"));
            }
          }
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }

        return Success;
      }
    }

    /// <summary>
    /// For Output reports the host sends to the device.
    /// Uses interrupt or control transfers depending on the device and OS.
    /// </summary>
    internal class OutputReport : HostReport
    {
      /// <summary>
      /// writes an Output report to the device.
      /// </summary>
      /// <param name="hidHandle">a handle to the device.</param>
      /// <param name="outputReportBuffer">contains the report ID and report to send.</param>
      /// <returns>True on success. False on failure.</returns>
      protected override bool ProtectedWrite(int hidHandle, byte[] outputReportBuffer)
      {
        int NumberOfBytesWritten = 0;
        bool Success = false;

        try
        {
          // The host will use an interrupt transfer if the the HID has an interrupt OUT
          // endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version).
          // Otherwise the the host will use a control transfer.
          // The application doesn't have to know or care which type of transfer is used.

          // ***
          // API function: WriteFile
          // Purpose: writes an Output report to the device.
          // Accepts:
          // A handle returned by CreateFile
          // The output report byte length returned by HidP_GetCaps.
          // An integer to hold the number of bytes written.
          // Returns: True on success, False on failure.
          // ***
          int Result = FileIOApiDeclarations.WriteFile(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length,
                                                       ref NumberOfBytesWritten, 0);
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("WriteFile"));
            Log.Debug("");
            Log.Debug(" OutputReportByteLength = " + outputReportBuffer.Length);
            Log.Debug(" NumberOfBytesWritten = " + NumberOfBytesWritten);
            Log.Debug(" Report ID: " + outputReportBuffer[0]);
            Log.Debug(" Report Data:");

            for (int i = 0; i < outputReportBuffer.Length; i++)
            {
              Log.Debug("   " + outputReportBuffer[i].ToString("x"));
            }
          }
          // Return True on success, False on failure.
          Success = (Result == 0) ? false : true;
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }

        return Success;
      }
    }


    internal class OutputReportViaControlTransfer : HostReport
    {
      /// <summary>
      /// writes an Output report to the device using a control transfer.
      /// </summary>
      /// <param name="hidHandle">a handle to the device.</param>
      /// <param name="outputReportBuffer">contains the report ID and report to send.</param>
      /// <returns>True on success. False on failure.</returns>
      protected override bool ProtectedWrite(int hidHandle, byte[] outputReportBuffer)
      {
        bool Success = false;

        try
        {
          // ***
          // API function: HidD_SetOutputReport
          // Purpose:
          // Attempts to send an Output report to the device using a control transfer.
          // Requires Windows XP or later.
          // Accepts:
          // A handle to a HID
          // A pointer to a buffer containing the report ID and report
          // The size of the buffer.
          // Returns: true on success, false on failure.
          // ***
          Success =
            HidApiDeclarations.HidD_SetOutputReport(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length);
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Debug(Debugging.ResultOfAPICall("Hidd_SetFeature"));
            Log.Debug("");
            Log.Debug(" OutputReportByteLength = " + outputReportBuffer.Length);
            Log.Debug(" Report ID: " + outputReportBuffer[0]);
            Log.Debug(" Report Data:");

            for (int i = 0; i < outputReportBuffer.Length; i++)
            {
              Log.Debug(" " + outputReportBuffer[i].ToString("x"));
            }
          }
        }
        catch (Exception ex)
        {
          HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
        }

        return Success;
      }
    }
  }
}
