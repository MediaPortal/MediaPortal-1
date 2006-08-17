#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using MediaPortal.Util;
using MediaPortal.Utils.Services;

namespace MediaPortal.RemoteControls.FireDTV
{
  public class FireDTVSourceFilterInfo
  {

    public FireDTVSourceFilterInfo(uint deviceHandle, IntPtr activeWindow)
      : base()
    {
      _windowHandle = activeWindow;
      _handle = deviceHandle;

      StringBuilder displayName = new StringBuilder(256);
      uint returnCode = FireDTVAPI.FS_GetDisplayString(Handle, displayName);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get Device Display Name!");
      _name = displayName.ToString();

      StringBuilder GuidString = new StringBuilder(256);
      returnCode = FireDTVAPI.FS_GetGUIDString(Handle, GuidString);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get GUID String!");
      _GUID = GuidString.ToString();

      string DriverFriend;
      returnCode = FireDTVAPI.FS_GetFriendlyString(Handle, out DriverFriend);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get Device Friendly Name!");
      _friendlyName = DriverFriend;

      FireDTVConstants.FireDTV_DRIVER_VERSION version = new FireDTVConstants.FireDTV_DRIVER_VERSION();
      returnCode = FireDTVAPI.FS_GetDriverVersion(Handle, ref version);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get Driver Version!");
      _driverVersion = System.Text.Encoding.ASCII.GetString(version.DriverVersion);

      returnCode = FireDTVAPI.FS_GetFirmwareVersion(Handle, ref _firmwareVersion);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get Firmware Hardware Version!");

      returnCode = FireDTVAPI.FS_GetSystemInfo(Handle, ref _systemInfo);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get System Information!");


      RegisterNotifications();
    }

    ~FireDTVSourceFilterInfo()
    {
      Close();
    }

    public void Close()
    {
      if (_handle != 0)
      {

        if (_remoteRunning)
          StopFireDTVRemoteControlSupport();

        if (_notificationRegistered)
          UnRegisterNotifications();

        CloseFireDTVHandle();
      }
    }
    #region Private Variables
    private uint _handle;
    private string _name;
    private string _friendlyName;
    private string _GUID;
    private string _driverVersion;
    private IntPtr _windowHandle = (IntPtr)0;
    private bool _remoteRunning = false;
    private bool _notificationRegistered = false;

    private FireDTVConstants.FireDTV_FIRMWARE_VERSION _firmwareVersion = new FireDTVConstants.FireDTV_FIRMWARE_VERSION();
    private FireDTVConstants.FireDTV_SYSTEM_INFO _systemInfo = new FireDTVConstants.FireDTV_SYSTEM_INFO();
    //IBaseFilter		*pFilter;
    #endregion
    #region Properties
    public string Name
    {
      get
      {
        return _name;
      }
    }
    public string GUID
    {
      get
      {
        return _GUID;
      }

    }
    public string Version
    {
      get
      {
        return _driverVersion;
      }
    }
    public uint Handle
    {
      get
      {
        return _handle;
      }
      set
      {
        _handle = value;
      }
    }

    public string FriendlyName
    {
      get
      {
        return _friendlyName;
      }
    }
    public FireDTVConstants.FireDTV_FIRMWARE_VERSION FirmwareVersion
    {
      get
      {
        return _firmwareVersion;
      }
    }
    public FireDTVConstants.FireDTV_SYSTEM_INFO SystemInformation
    {
      get
      {
        return _systemInfo;
      }
    }
    public bool RemoteRunning
    {
      get
      {
        return _remoteRunning;
      }
    }
    public bool NotificationRegistered
    {
      get
      {
        return _notificationRegistered;
      }
    }
    public IntPtr WindowHandle
    {
      get
      {
        if (_windowHandle == (IntPtr)0)
          return (IntPtr)FireDTVAPI.GetActiveWindow();
        else
          return _windowHandle;
      }
    }
    #endregion

    /// <summary>
    /// ToString() for debugging and logging.
    /// </summary>
    /// <returns></returns>
    public override String ToString()
    {
      return String.Format("SourceFilter: handle[{0}],name[{1}],friendly[{2}],GUID[{3}],version[{4}]",
        _handle, _name, _friendlyName, _GUID, _driverVersion);
    }




    #region FireDTV Close Device
    internal void CloseFireDTVHandle()
    {

      uint returnCode = FireDTVAPI.FS_CloseDeviceHandle(Handle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Device Close Failure");
      _handle = 0;
    }
    #endregion
    #region FireDTV Register Notifications
    public void RegisterNotifications()
    {
      uint returnCode = FireDTVAPI.FS_RegisterNotifications(Handle, (int)this.WindowHandle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to Register Notifiations");
      _notificationRegistered = true;
    }
    public void UnRegisterNotifications()
    {
      uint returnCode = FireDTVAPI.FS_UnregisterNotifications(Handle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to unRegister Notifiations");
      _notificationRegistered = false;
    }

    #endregion
    #region Remote Control Management
    public void StartFireDTVRemoteControlSupport()
    {
      uint returnCode = FireDTVAPI.FS_RemoteControl_Start(Handle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to Start RC Support");
      _remoteRunning = true;
    }
    public void StopFireDTVRemoteControlSupport()
    {
      uint returnCode = FireDTVAPI.FS_RemoteControl_Stop(Handle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to Stop RC Support");
      _remoteRunning = true;
    }
    #endregion

  }

  /// <summary>
  /// Stongly typed collection of FireDTVSourceFilters
  /// </summary>
  public class SourceFilterCollection : System.Collections.CollectionBase
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceFilter"></param>
    public void Add(FireDTVSourceFilterInfo sourceFilter)
    {
      foreach (FireDTVSourceFilterInfo srcFilter in List)
      {
        if (srcFilter.Name == sourceFilter.Name)
        {
          sourceFilter.Close();
          return;
        }
      }
      List.Add(sourceFilter);
    }

    /// <summary>
    /// Remove a FilterSource, but first close it.
    /// </summary>
    /// <param name="index">index of the filter</param>
    public void RemoveAt(int index)
    {
      if (index > List.Count - 1 || index < 0)
      {
        throw new IndexOutOfRangeException("Source Filter Index out of Bounds");
      }
      else
      {
        ((FireDTVSourceFilterInfo)List[index]).Close();
        List.RemoveAt(index);
      }
    }

    /// <summary>
    /// Remove a FilterSource, but first close it.
    /// </summary>
    /// <param name="index">index of the filter</param>
    public void RemoveByHandle(uint deviceHandle)
    {
      foreach (FireDTVSourceFilterInfo sourceFilter in List)
      {
        if (sourceFilter.Handle == deviceHandle)
        {
          List.Remove(sourceFilter);
        }
      }
      // TODO <THROW ERROR>
    }

    public FireDTVSourceFilterInfo Item(int index)
    {
      if (index > List.Count - 1 || index < 0)
      {
        throw new IndexOutOfRangeException("Source Filter Index out of Bounds");
      }
      else
      {
        return (FireDTVSourceFilterInfo)List[index];
      }
    }

    public FireDTVSourceFilterInfo ItemByHandle(uint deviceHandle)
    {
      foreach (FireDTVSourceFilterInfo sourceFilter in List)
        if (sourceFilter.Handle == deviceHandle)
          return sourceFilter;
      return null;
    }

    public FireDTVSourceFilterInfo ItemByName(string displayString)
    {
      foreach (FireDTVSourceFilterInfo SourceFilter in List)
        if (SourceFilter.Name == displayString)
          return SourceFilter;
      return null;
    }

    public FireDTVSourceFilterInfo ItemByGUID(string guidString)
    {
      foreach (FireDTVSourceFilterInfo SourceFilter in List)
        if (SourceFilter.GUID == guidString)
          return SourceFilter;
      return null;
    }

    public int IndexByHandle(uint deviceHandle)
    {
      for (int iIndex = 0; iIndex < List.Count; iIndex++)
      {
        FireDTVSourceFilterInfo SourceFilter = Item(iIndex);
        if (SourceFilter.Handle == deviceHandle)
          return iIndex;
      }
      return -1;
    }

  }



  /// <summary>
  /// Summary description for FireDTVControl.
  /// </summary>
  public class FireDTVControl
  {
    /// <summary>
    /// The SetDllDirectory function adds a directory to the search path used to locate DLLs for the application.
    /// http://msdn.microsoft.com/library/en-us/dllproc/base/setdlldirectory.asp
    /// </summary>
    /// <param name="PathName">Pointer to a null-terminated string that specifies the directory to be added to the search path.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    static extern bool SetDllDirectory(
      string PathName);

    #region Constructor / Destructor
    /// <summary>
    /// Try to locate the FireDTV API library and initialise the library.
    /// </summary>
    /// <param name="windowHandle"></param>
    public FireDTVControl(IntPtr windowHandle)
    {
      // get logger
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();

      // try to locate the FireDTV installation directory
      using (RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\DigitalEverywhere\\FireDTV"))
      {
        try
        {
          if (rkey == null)
          {
            _log.Error("FireDTVRemote: Trying to enable FireDTV remote, but software not installed!");
            return;
          }
          else
          {
            string dllPath = rkey.GetValue("InstallFolder").ToString();
            string fullDllPath = string.Format("{0}{1}", dllPath.Substring(0, dllPath.LastIndexOf('\\') + 1), "Tools\\");
            bool _apiFound = File.Exists(fullDllPath + "FiresatApi.dll");
            if (!_apiFound)
            {
              _log.Error("FireDTVRemote: Trying to enable FireDTV remote, but dll not found!");
              return;
            }
            _log.Info("FireDTVRemote: DLL found in directory: {0}", fullDllPath);
            FireDTVControl.SetDllDirectory(fullDllPath);
          }
        }
        catch (Exception ex)
        {
          _log.Error("FireDTVRemote: Trying to enable FireDTV remote, but failed to find dll with error: {0}",ex.Message);
          return;
        }
      }
      _windowHandle = windowHandle;

      // initialise the library
      InitializeLibrary();
      RegisterGeneralNotifications();
    }

    /// <summary>
    /// Default contructer should not be called.
    /// </summary>
    private FireDTVControl()
    {
    }


    ~FireDTVControl()
    {
      CloseDrivers();
    }
    #endregion
    #region Private Methods
    #region Initialization
    [System.Diagnostics.DebuggerStepThrough]
    internal void InitializeLibrary()
    {
      if (!LibrayInitialized)
      {
        try
        {
          uint returnCode = FireDTVAPI.FS_Initialize();
          if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
            throw new FireDTVInitializationException("Initilization Failure (" + returnCode.ToString() + ")");
          LibrayInitialized = true;
          _log.Info("FireDTV: dll initialized");
        }
        catch (Exception e)
        {
          _log.Error("FireDTV: error initializing {0}", e.Message);
        }
      }
    }
    #endregion
    #region FireDTV Open/Close Device
    internal uint OpenWDMDevice(int deviceIndex)
    {
      uint DeviceHandle;
      uint returnCode = FireDTVAPI.FS_OpenWDMDeviceHandle((uint)deviceIndex, out DeviceHandle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVDeviceOpenException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Open WDM Device Error!");
      return DeviceHandle;
    }

    internal uint OpenBDADevice(int deviceIndex)
    {
      uint DeviceHandle;
      uint returnCode = FireDTVAPI.FS_OpenBDADeviceHandle((uint)deviceIndex, out DeviceHandle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
        throw new FireDTVDeviceOpenException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Open BDA Device Error!");
      return DeviceHandle;
    }

    internal void CloseFireDTVHandle(FireDTVSourceFilterInfo currentSourceFilter)
    {
      try
      {
        uint returnCode = FireDTVAPI.FS_CloseDeviceHandle(currentSourceFilter.Handle);
        if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
          throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Device Close Failure");
      }
      catch (Exception) { }
    }
    internal int getWDMCount()
    {
      try
      {
        uint WDMCount;
        uint returnCode = FireDTVAPI.FS_GetNumberOfWDMDevices(out WDMCount);
        if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
          throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to WDM Driver Count");
        return (int)WDMCount;
      }
      catch (Exception) { }
      return 0;
    }

    internal int getBDACount()
    {
      try
      {
        uint BDACount;
        uint returnCode = FireDTVAPI.FS_GetNumberOfBDADevices(out BDACount);
        if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
          throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to BDA Driver Count");
        return (int)BDACount;
      }
      catch (Exception) { }
      return 0;
    }
    #endregion
    #region FireDTV Register Notifications
    internal void UnRegisterNotifications(uint widowHandle)
    {
      if (NotificationsRegistered)
      {
        uint returnCode = FireDTVAPI.FS_UnregisterNotifications(widowHandle);
        if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
          throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to unRegister Notifiations");
        NotificationsRegistered = false;
      }
    }

    [System.Diagnostics.DebuggerStepThrough]
    internal void RegisterGeneralNotifications()
    {
      if (!NotificationsRegistered)
      {
        try
        {
          uint returnCode = FireDTVAPI.FS_RegisterGeneralNotifications((int)WindowsHandle);
          if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
            throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to Register General Notifiations");
          NotificationsRegistered = true;
        }
        catch (Exception) { }
      }
    }
    #endregion
    #endregion
    #region Private Variables
    private ILog _log;
    private bool LibrayInitialized = false;
    private bool NotificationsRegistered = false;
    private IntPtr _windowHandle = (IntPtr)0;
    private SourceFilterCollection _sourceFilterCollection = new SourceFilterCollection();
    #endregion
    #region Properties
    public SourceFilterCollection SourceFilters
    {
      get
      {
        return _sourceFilterCollection;
      }
      set
      {
        _sourceFilterCollection = value;
      }
    }

    /// <summary>
    /// Get the API version of the FireDTV libary
    /// </summary>
    public string APIVersion
    {
      get
      {
        return Marshal.PtrToStringAnsi(FireDTVAPI.FS_GetApiVersion());
      }
    }

    public IntPtr WindowsHandle
    {
      get
      {
        if (_windowHandle == (IntPtr)0)
          return (IntPtr)FireDTVAPI.GetActiveWindow();
        else
          return _windowHandle;
      }
    }
    #endregion
    #region Public Methods

    /// <summary>
    /// Open the communication channels with the FireDTV's.
    /// </summary>
    /// <returns>true if success</returns>
    public bool OpenDrivers()
    {
      if (!LibrayInitialized)
        return false;

      int BDADriverCount = getBDACount();
      int WDMDriverCount = getWDMCount();

      _log.Info("FireDTV: BDA {0}, WMA {1}", BDADriverCount, WDMDriverCount);


      for (int BDACount = 0; BDACount < BDADriverCount; BDACount++)
      {
        FireDTVSourceFilterInfo bdaSourceFilter = new FireDTVSourceFilterInfo(OpenBDADevice(BDACount), _windowHandle);
        if (bdaSourceFilter != null)
          _log.Info("FireDTV: add BDA Source {0}", bdaSourceFilter.ToString());

        _sourceFilterCollection.Add(bdaSourceFilter);
      }

      for (int WDMCount = 0; WDMCount < WDMDriverCount; WDMCount++)
      {
        FireDTVSourceFilterInfo wdmSourceFilter = new FireDTVSourceFilterInfo(OpenWDMDevice(WDMCount), _windowHandle);
        if (wdmSourceFilter != null)
          _log.Info("FireDTV: add WDM Source");
        _sourceFilterCollection.Add(wdmSourceFilter);
      }
      return true;
    }

    public void CloseDrivers()
    {
      for (int DeviceCount = 0; DeviceCount < SourceFilters.Count; DeviceCount++)
      {
        FireDTVSourceFilterInfo SourceFilter = SourceFilters.Item(DeviceCount);
        _sourceFilterCollection.RemoveAt(DeviceCount);
      }
    }

    public bool StopRemoteControlSupport()
    {
      foreach (FireDTVSourceFilterInfo SourceFilter in _sourceFilterCollection)
      {
        if (SourceFilter.RemoteRunning)
        {
          SourceFilter.StopFireDTVRemoteControlSupport();
          return SourceFilter.RemoteRunning;
        }
      }
      return false;

    }
    #endregion
  }
}

