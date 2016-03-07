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
    // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363244(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEV_BROADCAST_DEVICEINTERFACE
    {
      public int dbcc_size;
      public int dbcc_devicetype;
      public int dbcc_reserved;
      public Guid dbcc_classguid;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
      public string dbcc_name;
    }

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
        var deviceInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(msg.LParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

        // get friendly device name
        string deviceName = String.Empty;
        string[] values = deviceInterface.dbcc_name.Split('#');
        if (values.Length >= 3)
        {
          string deviceType = values[0].Substring(values[0].IndexOf(@"?\", StringComparison.Ordinal) + 2);
          string deviceInstanceID = values[1];
          string deviceUniqueID = values[2];
          string regPath = @"SYSTEM\CurrentControlSet\Enum\" + deviceType + "\\" + deviceInstanceID + "\\" + deviceUniqueID;
          Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath);
          if (regKey != null)
          {
            // use the friendly name if it exists
            object result = regKey.GetValue("FriendlyName");
            if (result != null)
            {
              deviceName = result.ToString();
            }
            // if not use the device description's last part
            else
            {
              result = regKey.GetValue("DeviceDesc");
              if (result != null)
              {
                deviceName = result.ToString().Contains(@"%;") ? result.ToString().Substring(result.ToString().IndexOf(@"%;", StringComparison.Ordinal) + 2) : result.ToString();
              }
            }
          }
        }
        if (!string.IsNullOrEmpty(deviceName) && deviceName.Contains("Microsoft Virtual DVD-ROM"))
        {
          Log.Debug("Ignoring Microsoft Virtual DVD-ROM device change event");

          return true;
        }
      }
      catch
      { }

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
      }
      catch (Exception ex)
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

    /// <summary>
    /// Provides system last mount
    /// </summary>
    public static void SetMountTime(DateTime mountTime)
    {
      _mountTime = mountTime;
    }

    /// <summary>
    /// Provides system last mount time
    /// </summary>
    public static void SetExamineCDTime(DateTime ExamineCDTime)
    {
      _examineCDTime = ExamineCDTime;
    }

    // http://support.microsoft.com/kb/165721
    public static bool EjectMedia(string path, out String message)
    {
      bool driveReady = false;
      bool ejectSuccesfull = false;
      message = string.Empty;
      string sPhysicalDrive = @"\\.\" + path.Substring(0, 1) + ":";

      // Open drive (prepare for eject)
      IntPtr handle = CreateFile(sPhysicalDrive, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE,
        IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

      if (handle.ToInt32() == INVALID_HANDLE_VALUE)
      {
        message = "Media eject failed. Drive not ready or in use!";
        return false;
      }

      while (true)
      {
        // Lock Volume (retry 10 times - 5 seconds)
        for (int i = 0; i < 10; i++)
        {
          int nout = 0;
          driveReady = DeviceIoControl(handle, FSCTL_LOCK_VOLUME, null, 0, null, 0, out nout, IntPtr.Zero);

          if (driveReady)
          {
            break;
          }
          Thread.Sleep(500);
        }

        if (!driveReady)
        {
          message = "Media eject failed. Drive not ready or in use!";
          break;
        }

        // Volume dismount
        int xout = 0;
        driveReady = DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, null, 0, null, 0, out xout, IntPtr.Zero);

        if (!driveReady)
        {
          message = "Media eject failed. Drive not ready or in use!";
          break;
        }

        // Prevent Removal Of Volume
        byte[] flag = new byte[1];
        flag[0] = 0; // 0 = false
        int yout = 0;
        driveReady = DeviceIoControl(handle, IOCTL_STORAGE_MEDIA_REMOVAL, flag, 1, null, 0, out yout, IntPtr.Zero);

        if (!driveReady)
        {
          message = "Media eject failed. Drive not ready or in use!";
          break;
        }

        // Eject Media
        driveReady = DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, null, 0, null, 0, out xout, IntPtr.Zero);
        break;
      }

      // Close Handle
      driveReady = CloseHandle(handle);
      ejectSuccesfull = driveReady;
      return ejectSuccesfull;
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

      _volumeInsertTime = DateTime.Now;
      TimeSpan tsMount = DateTime.Now - _mountTime;
      TimeSpan tsExamineCD = DateTime.Now - _examineCDTime;
      TimeSpan tsVolumeRemoval = DateTime.Now - _volumeRemovalTime;

      if (Utils.IsRemovable(path) || Utils.IsHD(path))
      {
        Log.Debug("Detected new device: {0}", volumeLetter);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ADD_REMOVABLE_DRIVE, 0, 0, 0, 0, 0, 0);
        msg.Label = path;
        msg.Label2 = String.Format("({0}) {1}", path, driveName);
        GUIGraphicsContext.SendMessage(msg);
        return true;
      }
      else if (Utils.IsDVD(path))
      {
        // AnyDVD is causing media removed & inserted events when waking up from S3/S4 
        // We need to filter out those as it could trigger false autoplay event 
        if (tsExamineCD.TotalMilliseconds < _volumeRemovalDelay
          || (tsVolumeRemoval.TotalMilliseconds < _volumeRemovalDelay || tsMount.TotalMilliseconds < _volumeRemovalDelay))
        {
          Log.Debug("Ignoring volume inserted event - drive {0} - timespan mount {1} s",
            volumeLetter, tsMount.TotalMilliseconds / 1000);
          Log.Debug("   _volumeRemovalDelay = {0}", _volumeRemovalDelay);
          return false;
        }

        Log.Debug("Detected new optical media: {0}", volumeLetter);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED, 0, 0, 0, 0, 0, 0);
        msg.Label = path;
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

      if (DaemonTools.GetLastVirtualDrive() == path)
      {
        Log.Debug("Ignoring Microsoft Virtual DVD-ROM device change event. Drive letter: {0}", path);
        return true;
      }

      _volumeRemovalTime = DateTime.Now;
      TimeSpan tsMount = DateTime.Now - _mountTime;
      TimeSpan tsExamineCD = DateTime.Now - _examineCDTime;
      TimeSpan tsVolumeInsert = DateTime.Now - _volumeInsertTime;
            
      if (!Utils.IsDVD(path))
      {
        Log.Debug("Detected device remove: {0}", volumeLetter);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE, 0, 0, 0, 0, 0, 0);
        msg.Label = path;
        msg.Label2 = String.Format("({0})", path);
        GUIGraphicsContext.SendMessage(msg);
        return true;
      }
      else if (Utils.IsDVD(path))
      {

        // AnyDVD is causing media removed & inserted events when Mount/Unmount Volume
        // We need to filter out those as it could trigger false autoplay event
        if (tsExamineCD.TotalMilliseconds < _volumeRemovalDelay || tsMount.TotalMilliseconds < _volumeRemovalDelay || tsVolumeInsert.TotalMilliseconds < _volumeRemovalDelay)
        { 
          Log.Debug("Ignoring volume removed event - drive {0} - time after Mount {1} s",
            volumeLetter, tsMount.TotalMilliseconds / 1000);
          return false;
        }
        Log.Debug("Detected optical media removal: {0}", volumeLetter);
        Log.Debug("  time after ExamineCD {0} s", tsExamineCD.TotalMilliseconds / 1000);
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED, 0, 0, 0, 0, 0, 0);
        msg.Label = path;
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
          Sdn = (STORAGE_DEVICE_NUMBER)Marshal.PtrToStructure(ptrSdn, typeof (STORAGE_DEVICE_NUMBER));
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
        detailData.cbSize = Marshal.SizeOf(typeof (SP_DEVICE_INTERFACE_DETAIL_DATA));
        Marshal.StructureToPtr(detailData, buffer, false);

        if (!SetupDiGetDeviceInterfaceDetail(_deviceInfoSet, interfaceData, buffer, size, ref size, devData))
        {
          Marshal.FreeHGlobal(buffer);
          throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        IntPtr pDevicePath = (IntPtr)((int)buffer + Marshal.SizeOf(typeof (int)));
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
      public int cbSize = Marshal.SizeOf(typeof (SP_DEVICE_INTERFACE_DATA));
      public Guid interfaceClassGuid = Guid.Empty; // temp
      public int flags;
      public int reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private class SP_DEVINFO_DATA
    {
      public int cbSize = Marshal.SizeOf(typeof (SP_DEVINFO_DATA));
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

    // For eject media from drive
    private const int GENERIC_READ = unchecked((int)0x80000000);
    private const int GENERIC_WRITE = unchecked((int)0x40000000);
    private const int FILE_SHARE_READ = unchecked((int)0x00000001);
    private const int FILE_SHARE_WRITE = unchecked((int)0x00000002);
    private const int FSCTL_LOCK_VOLUME = unchecked((int)0x00090018);
    private const int FSCTL_DISMOUNT_VOLUME = unchecked((int)0x00090020);
    private const int IOCTL_STORAGE_EJECT_MEDIA = unchecked((int)0x002D4808);
    private const int IOCTL_STORAGE_MEDIA_REMOVAL = unchecked((int)0x002D4804);

    // For event filtering
    private static DateTime _mountTime = new DateTime();    
    private static DateTime _examineCDTime = new DateTime();
    private static DateTime _volumeRemovalTime = new DateTime();
    private static DateTime _volumeInsertTime = new DateTime();
    private static int _volumeRemovalDelay = 5000; // In milliseconds

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

    
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool DeviceIoControl(
      IntPtr hDevice, 
      int dwIoControlCode, 
      byte[] lpInBuffer,
      int nInBufferSize, 
      byte[] lpOutBuffer, 
      int nOutBufferSize, 
      out int lpBytesReturned, 
      IntPtr lpOverlapped);

    #endregion
  }
}