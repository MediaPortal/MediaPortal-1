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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
  /// <summary>
  /// Removable Drive Detection helper class
  /// </summary>
  public class RemovableDriveHelper
  {

    #region public methods
    /// <summary>
    /// Handles the window message when a drive change is in progress or finished
    /// </summary>
    /// <param name="msg">Window message</param>
    /// <returns>true, if the message was handled</returns>
    public static bool HandleDeviceChangedMessage(Message msg)
    {
      DEV_BROADCAST_HDR hdr = new DEV_BROADCAST_HDR();
      DEV_BROADCAST_VOLUME vol = new DEV_BROADCAST_VOLUME();

      try
      {
        if (msg.WParam.ToInt32() == DBT_DEVICEARRIVAL)
        {
          // new device
          hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(msg.LParam, hdr.GetType());
          if (hdr.devicetype == DBT_DEVTYPE_VOLUME)
          {
            vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(msg.LParam, vol.GetType());
            return DeviceNew(vol);
          }
        }
        else if (msg.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
        {
          // device remove
          hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(msg.LParam, hdr.GetType());
          if (hdr.devicetype == DBT_DEVTYPE_VOLUME)
          {
            vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(msg.LParam, vol.GetType());
            return DeviceRemoved(vol);
          }
        }
      } catch (Exception ex)
      {
        Log.Error("Error in handling device changed message: {0}", ex);
      }
      return false;
    }

    /// <summary>
    /// Ejects a drive. The caller has to ensure that it is a USB drive
    /// </summary>
    /// <param name="path">Path of the drive to eject</param>
    /// <param name="message">Response message</param>
    /// <returns>true, if it was successful</returns>
    public static bool EjectDrive(String path, out String message)
    {
      string sPhysicalDrive = path.Substring(0, 1) + ":";
      message = string.Empty;
      int ans = GetDeviceNumber(@"\\.\" + sPhysicalDrive);
      if (ans != -1)
      {
        int deviceNumber = ans;
        // get the device instance handle of the storage volume by means of a SetupDi enum and matching the device number
        SP_DEVINFO_DATA devData = GetDevInfoForDeviceNumber(deviceNumber);
        if (devData == null)
        {
          message = "Can't find the right device";
          return false;
        }
        int parentDevInst = 0;
        int childDevInst = devData.devInst;
        int hr = CM_Get_Parent(ref parentDevInst, childDevInst, 0);
        if (hr != 0)
        {
          message = "Can't get parent device";
        }
        StringBuilder sb = new StringBuilder(1024);
        PNP_VETO_TYPE veto;
        hr = CM_Request_Device_Eject(parentDevInst, out veto, sb, sb.Capacity, 0);
        if (hr != 0)
        {
          message = "Request failed";
          return false;
        }
        if (veto != PNP_VETO_TYPE.Ok)
        {
          message = veto.ToString();
          return false;
        }
      }
      return true;
    }
    #endregion

    #region private methods

    /// <summary>
    ///  Extract the volume letter from the WIN32 Mask extracted from a windows message
    /// </summary>
    /// <param name="Mask">Win32 MASK</param>
    /// <returns>Drive letter</returns>
    private static char GetVolumeLetter(int Mask)
    {
      int i;
      for (i = 0; i < 26; ++i)
      {
        if ((Mask & 0x1) == 0x1)
          break;
        Mask = Mask >> 1;
      }

      return (char)(Convert.ToChar(i) + 'A');
    }

    /// <summary>
    /// Extracts the Volume letter and sends a gui message with the extracted volume letter so that other plugins can add this drive out of there virtual directory
    /// </summary>
    /// <param name="volumeInformation">Volume Informations</param>
    /// <returns>true, if the message was handled; false otherwise</returns>
    private static bool DeviceNew(DEV_BROADCAST_VOLUME volumeInformation)
    {
      char volumeLetter = GetVolumeLetter(volumeInformation.UnitMask);
      string path = (volumeLetter + @":").ToUpperInvariant();
      string driveName = Utils.GetDriveName(path);
      if (Utils.IsRemovable(path) || Utils.IsHD(path))
      {
        Log.Debug("Detected new device: {0}", volumeLetter);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ADD_REMOVABLE_DRIVE, 0, 0, 0, 0, 0, 0);
        msg.Label = path;
        msg.Label2 = String.Format("({0}) {1}", path, driveName);
        GUIGraphicsContext.SendMessage(msg);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Extracts the Volume letter and sends a gui message with the extracted volume letter so that other plugins can remove this drive out of there virtual directory
    /// </summary>
    /// <param name="volumeInformation">Volume Informations</param>
    /// <returns>true, if the message was handled; false otherwise</returns>
    private static bool DeviceRemoved(DEV_BROADCAST_VOLUME volumeInformation)
    {
      char volumeLetter = GetVolumeLetter(volumeInformation.UnitMask);
      string path = (volumeLetter + @":").ToUpperInvariant();
      if (!Utils.IsDVD(path))
      {
        Log.Debug("Detected device remove: {0}", volumeLetter);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE, 0, 0, 0, 0, 0, 0);
        msg.Label = path;
        msg.Label2 = String.Format("({0})", path);
        GUIGraphicsContext.SendMessage(msg);
        return true;
      }
      return false;
    }
    
    /// <summary>
    /// Determins the device number 
    /// </summary>
    /// <param name="devicePath">Device path</param>
    /// <returns>The Device number</returns>
    private static int GetDeviceNumber(string devicePath)
    {
      int ans = -1;
      IntPtr h = CreateFile(devicePath, 0, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
      if (h.ToInt32() != INVALID_HANDLE_VALUE)
      {
        int requiredSize;
        STORAGE_DEVICE_NUMBER Sdn = new STORAGE_DEVICE_NUMBER();
        int nBytes = Marshal.SizeOf(Sdn);
        IntPtr ptrSdn = Marshal.AllocHGlobal(nBytes);

        if (DeviceIoControl(h, IOCTL_STORAGE_GET_DEVICE_NUMBER, null, 0, ptrSdn, nBytes, out requiredSize,
                            IntPtr.Zero))
        {
          Sdn = (STORAGE_DEVICE_NUMBER)Marshal.PtrToStructure(ptrSdn, typeof(STORAGE_DEVICE_NUMBER));
          // just my way of combining the relevant parts of the
          // STORAGE_DEVICE_NUMBER into a single number
          ans = (Sdn.DeviceType << 8) + Sdn.DeviceNumber;
        }
        Marshal.FreeHGlobal(ptrSdn);
        CloseHandle(h);
      }
      return ans;
    }

    /// <summary>
    /// Get Device data for the given device number
    /// </summary>
    /// <param name="DeviceNumber">Device number</param>
    /// <returns>Deice data</returns>
    private static SP_DEVINFO_DATA GetDevInfoForDeviceNumber(long DeviceNumber)
    {
      SP_DEVINFO_DATA result = null;
      Guid guid = new Guid(GUID_DEVINTERFACE_DISK);

      IntPtr _deviceInfoSet = SetupDiGetClassDevs(ref guid, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

      if (_deviceInfoSet.ToInt32() == INVALID_HANDLE_VALUE)
      {
        return result;
      }
      int index = 0;
      while (true)
      {
        SP_DEVICE_INTERFACE_DATA interfaceData = new SP_DEVICE_INTERFACE_DATA();

        if (!SetupDiEnumDeviceInterfaces(_deviceInfoSet, null, ref guid, index, interfaceData))
        {
          int error = Marshal.GetLastWin32Error();
          if (error != ERROR_NO_MORE_ITEMS)
            throw new Win32Exception(error);
          break;
        }

        SP_DEVINFO_DATA devData = new SP_DEVINFO_DATA();
        int size = 0;
        if (!SetupDiGetDeviceInterfaceDetail(_deviceInfoSet, interfaceData, IntPtr.Zero, 0, ref size, devData))
        {

          int error = Marshal.GetLastWin32Error();
          if (error != ERROR_INSUFFICIENT_BUFFER)
            throw new Win32Exception(error);
        }

        IntPtr buffer = Marshal.AllocHGlobal(size);
        SP_DEVICE_INTERFACE_DETAIL_DATA detailData = new SP_DEVICE_INTERFACE_DETAIL_DATA();
        detailData.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DETAIL_DATA));
        Marshal.StructureToPtr(detailData, buffer, false);

        if (!SetupDiGetDeviceInterfaceDetail(_deviceInfoSet, interfaceData, buffer, size, ref size, devData))
        {
          Marshal.FreeHGlobal(buffer);
          throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        IntPtr pDevicePath = (IntPtr)((int)buffer + Marshal.SizeOf(typeof(int)));
        string devicePath = Marshal.PtrToStringAuto(pDevicePath);
        Marshal.FreeHGlobal(buffer);
        if (GetDeviceNumber(devicePath) == DeviceNumber)
        {
          result = devData;
          break;
        }

        index++;
      }

      return result;
    }
    #endregion

    #region struct
    [StructLayout(LayoutKind.Sequential)]
    private struct DEV_BROADCAST_HDR
    {
      public int size;
      public int devicetype;
      public int reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DEV_BROADCAST_VOLUME
    {
      public DEV_BROADCAST_HDR Header;
      public int UnitMask;
      public short Flags;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct STORAGE_DEVICE_NUMBER
    {
      public int DeviceType;
      public int DeviceNumber;
      public int PartitionNumber;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
      public int cbSize;
      public short devicePath;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private class SP_DEVICE_INTERFACE_DATA
    {
      public int cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA));
      public Guid interfaceClassGuid = Guid.Empty; // temp
      public int flags;
      public int reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private class SP_DEVINFO_DATA
    {
      public int cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
      public Guid classGuid = Guid.Empty; // temp
      public int devInst; // dumy
      public int reserved;
    }
    #endregion

    #region const
    private const int ERROR_NO_MORE_ITEMS = 259;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;
    private const int DIGCF_PRESENT = (0x00000002);
    private const int DIGCF_DEVICEINTERFACE = (0x00000010);
    private const int INVALID_HANDLE_VALUE = -1;
    private const int OPEN_EXISTING = unchecked(3);
    private const int IOCTL_STORAGE_GET_DEVICE_NUMBER = 0x2D1080;
    private const string GUID_DEVINTERFACE_DISK = "53f56307-b6bf-11d0-94f2-00a0c91efb8b";
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private const int DBT_DEVTYPE_VOLUME = 2;
    #endregion

    #region enums
    internal enum PNP_VETO_TYPE
    {
      Ok,
      TypeUnknown,
      LegacyDevice,
      PendingClose,
      WindowsApp,
      WindowsService,
      OutstandingOpen,
      Device,
      Driver,
      IllegalDeviceRequest,
      InsufficientPower,
      NonDisableable,
      LegacyDriver,
    }
    #endregion

    #region Native methods
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool SetupDiEnumDeviceInterfaces(
        IntPtr deviceInfoSet,
        SP_DEVINFO_DATA deviceInfoData,
        ref Guid interfaceClassGuid,
        int memberIndex,
        SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid,
      int enumerator,
      IntPtr hwndParent,
      int flags);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool SetupDiGetDeviceInterfaceDetail(
        IntPtr deviceInfoSet,
        SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
        IntPtr deviceInterfaceDetailData,
        int deviceInterfaceDetailDataSize,
        ref int requiredSize,
        SP_DEVINFO_DATA deviceInfoData);

    // from cfgmgr32.h
    [DllImport("setupapi.dll")]
    private static extern int CM_Get_Parent(
        ref int pdnDevInst,
        int dnDevInst,
        int ulFlags);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
    private static extern int CM_Get_Device_ID(
       IntPtr dnDevInst,
       IntPtr Buffer,
       int BufferLen,
       int ulFlags
    );

    [DllImport("setupapi.dll")]
    private static extern int CM_Request_Device_Eject(
        int dnDevInst,
        out PNP_VETO_TYPE pVetoType,
        StringBuilder pszVetoName,
        int ulNameLength,
        int ulFlags
        );

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFile(
    string lpFileName,
    int dwDesiredAccess,
    int dwShareMode,
    IntPtr lpSecurityAttributes,
    int dwCreationDisposition,
    int dwFlagsAndAttributes,
    IntPtr hTemplateFile);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool DeviceIoControl(
    IntPtr hDevice,
    int dwIoControlCode,
    byte[] lpInBuffer,
    int nInBufferSize,
    IntPtr lpOutBuffer,
    int nOutBufferSize,
    out int lpBytesReturned,
    IntPtr lpOverlapped);

    #endregion
  }
}
