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

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.VFD_Control
{
  /// <summary>
  /// For detecting devices and receiving device notifications.
  /// </summary>
  internal class DeviceManagement
  {
    // Used in error messages:
    private const string ModuleName = "DeviceManagement";


    /*
		internal bool DeviceNameMatch(Message m, string mydevicePathName)
		{
			
			// Purpose    : Compares two device path names. Used to find out if the device name
			//            : of a recently attached or removed device matches the name of a
			//            : device the application is communicating with.
			
			// Accepts    : m - a WM_DEVICECHANGE message. A call to RegisterDeviceNotification
			//            : causes WM_DEVICECHANGE messages to be passed to an OnDeviceChange routine.
			//            : mydevicePathName - a device pathname returned by SetupDiGetDeviceInterfaceDetail
			//            : in an SP_DEVICE_INTERFACE_DETAIL_DATA structure.
			
			// Returns    : True if the names match, False if not.
			
			try {
				DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE_1 DevBroadcastDeviceInterface = new DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE_1();
				DeviceManagementApiDeclarations.DEV_BROADCAST_HDR DevBroadcastHeader = new DeviceManagementApiDeclarations.DEV_BROADCAST_HDR();
				
				// The LParam parameter of Message is a pointer to a DEV_BROADCAST_HDR structure.
				Marshal.PtrToStructure(m.LParam, DevBroadcastHeader);
				
				if (DevBroadcastHeader.dbch_devicetype == DeviceManagementApiDeclarations.DBT_DEVTYP_DEVICEINTERFACE) {
					
					// The dbch_devicetype parameter indicates that the event applies to a device interface.
					// So the structure in LParam is actually a DEV_BROADCAST_INTERFACE structure,
					// which begins with a DEV_BROADCAST_HDR.
					
					// Obtain the number of characters in dbch_name by subtracting the 28 bytes
					// in the other members of the structure and dividing by 2 because there are
					// 2 bytes per character.
					int StringSize = System.Convert.ToInt32((DevBroadcastHeader.dbch_size - 28) / 2);
					
					// The dbcc_name parameter of DevBroadcastDeviceInterface contains the device name.
					// Trim dbcc_name to match the size of the string.
					DevBroadcastDeviceInterface.dbcc_name = new char[StringSize + 1];
					
					// Marshal data from the unmanaged block pointed to by m.LParam
					// to the managed object DevBroadcastDeviceInterface.
					Marshal.PtrToStructure(m.LParam, DevBroadcastDeviceInterface);
					
					// Store the device name in a String.
					string DeviceNameString = new string(DevBroadcastDeviceInterface.dbcc_name, 0, StringSize);
					
					Debug.WriteLine("Device Name = " + DeviceNameString);
					Debug.WriteLine("");
					
					// Compare the name of the newly attached device with the name of the device
					// the application is accessing (mydevicePathName).
					// Set ignorecase True.
					if (string.Compare(DeviceNameString, mydevicePathName, true) == 0) {
						// The name matches.
						return true;
					}
				}
				
			} catch (Exception ex) {
				HandleException(ModuleName + ":" + System.Reflection.MethodBase.GetCurrentMethod(), ex);
			}

      // It's a different device.
      return false;
		}
		*/

    /// <summary>
    /// Uses SetupDi API functions to retrieve the device path name of an attached device that belongs to an interface class.
    /// </summary>
    /// <param name="myGuid">an interface class GUID.</param>
    /// <param name="devicePathName">a pointer to an array of strings that will contain the device path names of attached devices.</param>
    /// <returns>True if at least one device is found, False if not.</returns>
    internal static bool FindDeviceFromGuid(Guid myGuid, ref string[] devicePathName)
    {
      // int DetailData;
      bool DeviceFound = false;
      bool LastDevice = false;
      int BufferSize = 0;
      // DeviceManagementApiDeclarations.SP_DEVINFO_DATA MyDeviceInfoData;
      DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DETAIL_DATA MyDeviceInterfaceDetailData =
        new DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DETAIL_DATA();
      DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DATA MyDeviceInterfaceData =
        new DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DATA();

      try
      {
        // ***
        // API function: SetupDiGetClassDevs
        // Purpose:
        // Retrieves a device information set for a specified group of devices.
        // SetupDiEnumDeviceInterfaces uses the device information set.
        // Accepts:
        // An interface class GUID
        // Null to retrieve information for all device instances
        // An optional handle to a top-level window (unused here)
        // Flags to limit the returned information to currently present devices
        // and devices that expose interfaces in the class specified by the GUID.
        // Returns:
        // A handle to a device information set for the devices.
        // ***
        IntPtr DeviceInfoSet = DeviceManagementApiDeclarations.SetupDiGetClassDevs(
          ref myGuid,
          null,
          0,
          DeviceManagementApiDeclarations.DIGCF_PRESENT | DeviceManagementApiDeclarations.DIGCF_DEVICEINTERFACE);

        Debug.WriteLine(Debugging.ResultOfAPICall("SetupDiClassDevs"));

        DeviceFound = false;
        int MemberIndex = 0;

        do
        {
          // Begin with 0 and increment through the device information set until
          // no more devices are available.

          // The cbSize element of the MyDeviceInterfaceData structure must be set to
          // the structure's size in bytes. The size is 28 bytes.
          MyDeviceInterfaceData.cbSize = 28;
          //Marshal.SizeOf(MyDeviceInterfaceData);

          // ***
          // API function:
          // SetupDiEnumDeviceInterfaces()
          // Purpose: Retrieves a handle to a SP_DEVICE_INTERFACE_DATA
          // structure for a device.
          // On return, MyDeviceInterfaceData contains the handle to a
          // SP_DEVICE_INTERFACE_DATA structure for a detected device.
          // Accepts:
          // A DeviceInfoSet returned by SetupDiGetClassDevs.
          // An interface class GUID.
          // An index to specify a device in a device information set.
          // A pointer to a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.
          // Returns:
          // Non-zero on success, zero on True.
          // ***

          int Result = DeviceManagementApiDeclarations.SetupDiEnumDeviceInterfaces(
            DeviceInfoSet,
            0,
            ref myGuid,
            MemberIndex,
            ref MyDeviceInterfaceData);

          Debug.WriteLine(Debugging.ResultOfAPICall("SetupDiEnumDeviceInterfaces"));

          // Find out if a device information set was retrieved.

          if (Result == 0)
          {
            LastDevice = true;
          }
          else
          {
            // A device is present.
            Debug.WriteLine("  DeviceInfoSet for device #" + MemberIndex + ": ");
            Debug.WriteLine("  cbSize = " + MyDeviceInterfaceData.cbSize);
            Debug.WriteLine("  InterfaceclassGuid = " + MyDeviceInterfaceData.InterfaceClassGuid);
            Debug.WriteLine("  Flags = " + MyDeviceInterfaceData.Flags.ToString("x"));

            // ***
            // API function:
            // SetupDiGetDeviceInterfaceDetail()
            // Purpose:
            // Retrieves an SP_DEVICE_INTERFACE_DETAIL_DATA structure
            // containing information about a device.
            // To retrieve the information, call this function twice.
            // The first time returns the size of the structure.
            // The second time returns a pointer to the data.
            // Accepts:
            // A DeviceInfoSet returned by SetupDiGetClassDevs
            // An SP_DEVICE_INTERFACE_DATA structure returned by SetupDiEnumDeviceInterfaces
            // A pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA structure to receive information
            // about the specified interface.
            // The size of the SP_DEVICE_INTERFACE_DETAIL_DATA structure.
            // A pointer to a variable that will receive the returned required size of the
            // SP_DEVICE_INTERFACE_DETAIL_DATA structure.
            // A pointer to an SP_DEVINFO_DATA structure to receive information about the device.
            // Returns:
            // Non-zero on success, zero on failure.
            // ***


            DeviceManagementApiDeclarations.SetupDiGetDeviceInterfaceDetail(
              DeviceInfoSet,
              ref MyDeviceInterfaceData,
              IntPtr.Zero, 0, ref BufferSize, IntPtr.Zero);

            Debug.WriteLine(Debugging.ResultOfAPICall("SetupDiGetDeviceInterfaceDetail"));
            Debug.WriteLine("  (OK to say too small)");
            Debug.WriteLine("  Required buffer size for the data: " + BufferSize);

            // Store the structure's size.
            //MyDeviceInterfaceDetailData.cbSize = MyDeviceInterfaceDetailData.ToString().Length;
            MyDeviceInterfaceDetailData.cbSize = Marshal.SizeOf(MyDeviceInterfaceDetailData);

            // Allocate memory for the MyDeviceInterfaceDetailData Structure using the returned buffer size.
            IntPtr DetailDataBuffer = Marshal.AllocHGlobal(BufferSize);

            // Store cbSize in the first 4 bytes of the array
            Marshal.WriteInt32(DetailDataBuffer, 4 + Marshal.SystemDefaultCharSize);
            Debug.WriteLine("cbsize = " + MyDeviceInterfaceDetailData.cbSize);

            // Call SetupDiGetDeviceInterfaceDetail again.
            // This time, pass a pointer to DetailDataBuffer
            // and the returned required buffer size.
            DeviceManagementApiDeclarations.SetupDiGetDeviceInterfaceDetail(
              DeviceInfoSet,
              ref MyDeviceInterfaceData,
              DetailDataBuffer,
              BufferSize,
              ref BufferSize,
              IntPtr.Zero);

            Debug.WriteLine(Debugging.ResultOfAPICall(" Result of second call: "));
            Debug.WriteLine("  MyDeviceInterfaceDetailData.cbSize: " + MyDeviceInterfaceDetailData.cbSize);

            // Skip over cbsize (4 bytes) to get the address of the devicePathName.
            IntPtr pdevicePathName = new IntPtr(DetailDataBuffer.ToInt32() + 4);

            // Get the String containing the devicePathName.
            string SingledevicePathName = Marshal.PtrToStringAuto(pdevicePathName);
            devicePathName[MemberIndex] = SingledevicePathName;

            Debug.WriteLine("Device Path = " + devicePathName[MemberIndex]);
            //Debug.WriteLine("Device Path Length= " + Marshal.SizeOf(devicePathName[MemberIndex]));
            Debug.WriteLine("Device Path Length = " + devicePathName[MemberIndex].Length);

            // Free the memory allocated previously by AllocHGlobal.
            Marshal.FreeHGlobal(DetailDataBuffer);
            DeviceFound = true;
          }
          MemberIndex++;
        } while (!LastDevice);

        // Trim the array to the number of devices found.
        Debug.WriteLine("Number of HIDs found = " + (MemberIndex - 1));

        // ***
        // API function:
        // SetupDiDestroyDeviceInfoList
        // Purpose:
        // Frees the memory reserved for the DeviceInfoSet returned by SetupDiGetClassDevs.
        // Accepts:
        // A DeviceInfoSet returned by SetupDiGetClassDevs.
        // Returns:
        // True on success, False on failure.
        // ***

        DeviceManagementApiDeclarations.SetupDiDestroyDeviceInfoList(DeviceInfoSet);

        Debug.WriteLine(Debugging.ResultOfAPICall("DestroyDeviceInfoList"));
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return DeviceFound;
    }


    /// <summary>
    /// Request to receive a notification when a device is attached or removed.
    /// </summary>
    /// <param name="devicePathName">a handle to a device.</param>
    /// <param name="formHandle">a handle to the window that will receive device events.</param>
    /// <param name="classGuid">an interface class GUID.</param>
    /// <param name="deviceNotificationHandle"></param>
    /// <returns>True on success, False on failure.</returns>
    internal static bool RegisterForDeviceNotifications(string devicePathName, IntPtr formHandle, Guid classGuid,
                                                        ref IntPtr deviceNotificationHandle)
    {
      // Returns    : 
      // A DEV_BROADCAST_DEVICEINTERFACE header holds information about the request.
      DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE DevBroadcastDeviceInterface =
        new DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE();

      try
      {
        // Set the parameters in the DEV_BROADCAST_DEVICEINTERFACE structure.

        // Set the size.
        int size = Marshal.SizeOf(DevBroadcastDeviceInterface);
        DevBroadcastDeviceInterface.dbcc_size = size;

        // Request to receive notifications about a class of devices.
        DevBroadcastDeviceInterface.dbcc_devicetype = DeviceManagementApiDeclarations.DBT_DEVTYP_DEVICEINTERFACE;

        DevBroadcastDeviceInterface.dbcc_reserved = 0;

        // Specify the interface class to receive notifications about.
        DevBroadcastDeviceInterface.dbcc_classguid = classGuid;

        // Allocate memory for the buffer that holds the DEV_BROADCAST_DEVICEINTERFACE structure.
        IntPtr DevBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);

        // Copy the DEV_BROADCAST_DEVICEINTERFACE structure to the buffer.
        // Set fDeleteOld True to prevent memory leaks.
        Marshal.StructureToPtr(DevBroadcastDeviceInterface, DevBroadcastDeviceInterfaceBuffer, true);

        // ***
        // API function:
        // RegisterDeviceNotification
        // Purpose:
        // Request to receive notification messages when a device in an interface class
        // is attached or removed.
        // Accepts:
        // Aa handle to the window that will receive device events
        // A pointer to a DEV_BROADCAST_DEVICEINTERFACE to specify the type of
        // device to send notifications for,
        // DEVICE_NOTIFY_WINDOW_HANDLE to indicate that Handle is a window handle.
        // Returns:
        // A device notification handle or NULL on failure.
        // ***

        deviceNotificationHandle =
          DeviceManagementApiDeclarations.RegisterDeviceNotification(formHandle, DevBroadcastDeviceInterfaceBuffer,
                                                                     DeviceManagementApiDeclarations.
                                                                       DEVICE_NOTIFY_WINDOW_HANDLE);

        // Marshal data from the unmanaged block DevBroadcastDeviceInterfaceBuffer to
        // the managed object DevBroadcastDeviceInterface
        Marshal.PtrToStructure(DevBroadcastDeviceInterfaceBuffer, DevBroadcastDeviceInterface);

        // Free the memory allocated previously by AllocHGlobal.
        Marshal.FreeHGlobal(DevBroadcastDeviceInterfaceBuffer);

        // Find out if RegisterDeviceNotification was successful.
        if (deviceNotificationHandle.ToInt32() == IntPtr.Zero.ToInt32())
        {
          Debug.WriteLine("RegisterDeviceNotification error");
          return false;
        }
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }

      return true;
    }


    /// <summary>
    /// Requests to stop receiving notification messages when a device in an
    /// interface class is attached or removed.
    /// </summary>
    /// <param name="deviceNotificationHandle">a handle returned previously by RegisterDeviceNotification</param>
    internal static void StopReceivingDeviceNotifications(IntPtr deviceNotificationHandle)
    {
      try
      {
        // ***
        // API function: UnregisterDeviceNotification
        // Purpose: Stop receiving notification messages.
        // Accepts: a handle returned previously by RegisterDeviceNotification
        // Returns: True on success, False on failure.
        // ***
        // Ignore failures.
        DeviceManagementApiDeclarations.UnregisterDeviceNotification(deviceNotificationHandle);
      }
      catch (Exception ex)
      {
        HandleException(ModuleName + ":" + MethodBase.GetCurrentMethod(), ex);
      }
    }

    /// <summary>
    /// Provides a central mechanism for exception handling.
    /// Writes the message to the MediaPortal logger.
    /// </summary>
    /// <param name="moduleName">the module where the exception occurred.</param>
    /// <param name="e">the exception</param>
    public static void HandleException(string moduleName, Exception e)
    {
      Log.Error("Exception: " + e.Message + Environment.NewLine + "Module: " + moduleName + Environment.NewLine +
                "Method: " + e.TargetSite.Name);
    }
  }
}
