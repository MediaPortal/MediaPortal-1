#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

#region Usings

using System;
using System.Collections;
using System.Text;

#endregion

namespace MediaPortal.InputDevices.FireDTV
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
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,
                                   "Unable to get Device Display Name!");
      }
      _name = displayName.ToString();

      StringBuilder GuidString = new StringBuilder(256);
      returnCode = FireDTVAPI.FS_GetGUIDString(Handle, GuidString);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get GUID String!");
      }
      _GUID = GuidString.ToString();

      string DriverFriend;
      returnCode = FireDTVAPI.FS_GetFriendlyString(Handle, out DriverFriend);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,
                                   "Unable to get Device Friendly Name!");
      }
      _friendlyName = DriverFriend;

      FireDTVConstants.FireDTV_DRIVER_VERSION version = new FireDTVConstants.FireDTV_DRIVER_VERSION();
      returnCode = FireDTVAPI.FS_GetDriverVersion(Handle, ref version);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get Driver Version!");
      }
      _driverVersion = Encoding.ASCII.GetString(version.DriverVersion);

      returnCode = FireDTVAPI.FS_GetFirmwareVersion(Handle, ref _firmwareVersion);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,
                                   "Unable to get Firmware Hardware Version!");
      }

      returnCode = FireDTVAPI.FS_GetSystemInfo(Handle, ref _systemInfo);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to get System Information!");
      }


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
        {
          StopFireDTVRemoteControlSupport();
        }

        if (_notificationRegistered)
        {
          UnRegisterNotifications();
        }

        CloseFireDTVHandle();
      }
    }

    #region Private variables

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
      get { return _name; }
    }

    public string GUID
    {
      get { return _GUID; }
    }

    public string Version
    {
      get { return _driverVersion; }
    }

    public uint Handle
    {
      get { return _handle; }
      set { _handle = value; }
    }

    public string FriendlyName
    {
      get { return _friendlyName; }
    }

    public FireDTVConstants.FireDTV_FIRMWARE_VERSION FirmwareVersion
    {
      get { return _firmwareVersion; }
    }

    public FireDTVConstants.FireDTV_SYSTEM_INFO SystemInformation
    {
      get { return _systemInfo; }
    }

    public bool RemoteRunning
    {
      get { return _remoteRunning; }
    }

    public bool NotificationRegistered
    {
      get { return _notificationRegistered; }
    }

    public IntPtr WindowHandle
    {
      get
      {
        if (_windowHandle == (IntPtr)0)
        {
          return (IntPtr)FireDTVAPI.GetActiveWindow();
        }
        else
        {
          return _windowHandle;
        }
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
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Device Close Failure");
      }
      _handle = 0;
    }

    #endregion

    #region FireDTV Register Notifications

    public void RegisterNotifications()
    {
      uint returnCode = FireDTVAPI.FS_RegisterNotifications(Handle, (int)this.WindowHandle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to Register Notifiations");
      }
      _notificationRegistered = true;
    }

    public void UnRegisterNotifications()
    {
      uint returnCode = FireDTVAPI.FS_UnregisterNotifications(Handle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to unRegister Notifiations");
      }
      _notificationRegistered = false;
    }

    #endregion

    #region Remote Control Management

    public void StartFireDTVRemoteControlSupport()
    {
      uint returnCode = FireDTVAPI.FS_RemoteControl_Start(Handle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to Start RC Support");
      }
      _remoteRunning = true;
    }

    public void StopFireDTVRemoteControlSupport()
    {
      uint returnCode = FireDTVAPI.FS_RemoteControl_Stop(Handle);
      if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
      {
        throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode, "Unable to Stop RC Support");
      }
      _remoteRunning = true;
    }

    #endregion
  }

  /// <summary>
  /// Strongly typed collection of FireDTVSourceFilters
  /// </summary>
  public class SourceFilterCollection : CollectionBase
  {
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
    /// Remove a FilterSource but first close it.
    /// </summary>
    /// <param name="index">index of the filter</param>
    public new void RemoveAt(int index)
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
      {
        if (sourceFilter.Handle == deviceHandle)
        {
          return sourceFilter;
        }
      }
      return null;
    }

    public FireDTVSourceFilterInfo ItemByName(string displayString)
    {
      foreach (FireDTVSourceFilterInfo SourceFilter in List)
      {
        if (SourceFilter.Name == displayString)
        {
          return SourceFilter;
        }
      }
      return null;
    }

    public FireDTVSourceFilterInfo ItemByGUID(string guidString)
    {
      foreach (FireDTVSourceFilterInfo SourceFilter in List)
      {
        if (SourceFilter.GUID == guidString)
        {
          return SourceFilter;
        }
      }
      return null;
    }

    public int IndexByHandle(uint deviceHandle)
    {
      for (int iIndex = 0; iIndex < List.Count; iIndex++)
      {
        FireDTVSourceFilterInfo SourceFilter = Item(iIndex);
        if (SourceFilter.Handle == deviceHandle)
        {
          return iIndex;
        }
      }
      return -1;
    }
  }
}