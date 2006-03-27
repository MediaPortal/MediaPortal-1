/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;
using System.IO;

namespace MadMouse.FireDTV
{
	public class FireDTVSourceFilterInfo
	{

		public FireDTVSourceFilterInfo(uint deviceHandle, int deviceIndex,int activeWindow) :base()
		{
			_windowHandle = activeWindow;
			_deviceHandle = deviceHandle;
			_deviceIndex  = deviceIndex;

			StringBuilder displayName = new StringBuilder(256);
			uint returnCode = FireDTVAPI.FS_GetDisplayString(DeviceHandle,displayName);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to get Device Display Name!");
			_displayName =	displayName.ToString();

			StringBuilder GuidString = new StringBuilder(256);
			returnCode = FireDTVAPI.FS_GetGUIDString(DeviceHandle,GuidString);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to get GUID String!");
			_driverGUID = GuidString.ToString();

			string DriverFriend;
			returnCode = FireDTVAPI.FS_GetFriendlyString(DeviceHandle,out DriverFriend);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to get Device Friendly Name!");
			_driverFriendlyName =	DriverFriend;

			FireDTVConstants.FireDTV_DRIVER_VERSION version = new FireDTVConstants.FireDTV_DRIVER_VERSION();
			returnCode = FireDTVAPI.FS_GetDriverVersion(DeviceHandle,ref version);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to get Driver Version!");
			_driverVersion = System.Text.Encoding.ASCII.GetString(version.DriverVersion);

			returnCode = FireDTVAPI.FS_GetFirmwareVersion(deviceHandle,ref _firmwareVersion);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to get Firmware Hardware Version!");

			returnCode = FireDTVAPI.FS_GetSystemInfo(deviceHandle,ref _systemInfo);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to get System Information!");

				
			RegisterNotifications();
		}

		~FireDTVSourceFilterInfo()
		{
			Close();
		}

		public void Close()
		{
			if (_deviceHandle != 0)
			{
					
				if (_remoteRunning)
					StopFireDTVRemoteControlSupport();

				if (_ciRunning)
					CloseFireDTVCommonInterfaceSupport();

				if (_notificationRegistered)
					UnRegisterNotifications();

				CloseFireDTVHandle();
			}
		}
		#region Private Variables
		private uint						_deviceHandle;
		private int							_deviceIndex;
		private string						_displayName;
		private string						_driverFriendlyName;
		private string						_driverGUID;
		private string						_driverVersion;
		private int							_windowHandle			= 0;
		private bool						_remoteRunning			= false;
		private bool						_ciRunning				= false;
		private bool						_notificationRegistered = false;

		private FireDTVConstants.FireDTV_FIRMWARE_VERSION	_firmwareVersion = new FireDTVConstants.FireDTV_FIRMWARE_VERSION();
		private FireDTVConstants.FireDTV_SYSTEM_INFO		_systemInfo		 = new FireDTVConstants.FireDTV_SYSTEM_INFO();
		//IBaseFilter		*pFilter;
		#endregion
		#region Properties
		public int						DeviceIndex
		{
			get
			{
				return _deviceIndex;
			}
		}
		public string					DisplayString
		{
			get
			{
				return _displayName;
			}
		}
		public string					GUIDString
		{
			get
			{
				return _driverGUID;
			}

		}
		public string					DriverVersion
		{
			get
			{
				return _driverVersion;
			}
		}
		public uint						DeviceHandle
		{
			get
			{
				return _deviceHandle;
			}
			set
			{
				_deviceHandle = value;
			}
		}

		public string					DeviceFriendlyName
		{
			get
			{
				return _driverFriendlyName;
			}
		}
		public FireDTVConstants.FireDTV_FIRMWARE_VERSION FirmwareVersion	
		{
			get
			{
				return _firmwareVersion;
			}
		}
		public FireDTVConstants.FireDTV_SYSTEM_INFO		SystemInformation
		{
			get
			{
				return _systemInfo;
			}
		}
		public bool						RemoteRunning
		{
			get
			{
				return _remoteRunning;
			}
		}
		public bool						CIRunning
		{
			get
			{
				return _ciRunning;
			}
		}
		public bool						NotificationRegistered
		{
			get
			{
				return _notificationRegistered;
			}
		}
		public int WindowHandle
		{
			get
			{
				if (_windowHandle == 0)
					return FireDTVAPI.GetActiveWindow();
				else
					return _windowHandle;
			}
		}
		#endregion
		#region FireDTV Close Device
		internal void CloseFireDTVHandle()
		{
			
			uint returnCode = FireDTVAPI.FS_CloseDeviceHandle(DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Device Close Failure");
			_deviceHandle = 0;
		}
		#endregion
		#region FireDTV Register Notifications
		public void RegisterNotifications()
		{
			uint returnCode = FireDTVAPI.FS_RegisterNotifications(DeviceHandle,this.WindowHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to Register Notifiations");
			_notificationRegistered = true;
		}
		public void UnRegisterNotifications()
		{
			uint returnCode = FireDTVAPI.FS_UnregisterNotifications(DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to unRegister Notifiations");
			_notificationRegistered = false;
		}

		#endregion
		#region Remote Control Management
		public void StartFireDTVRemoteControlSupport()
		{
			uint returnCode = FireDTVAPI.FS_RemoteControl_Start(DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to Start RC Support");
			_remoteRunning = true;
		}
		public void StopFireDTVRemoteControlSupport()
		{
			uint returnCode = FireDTVAPI.FS_RemoteControl_Stop(DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to Stop RC Support");
			_remoteRunning = true;
		}
		#endregion
		#region Common Interface Management
		public void openFireDTVCommonInterfaceSupport()
		{
			uint returnCode = FireDTVAPI.FS_CI_Open(DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to Start CI Support");
			_ciRunning = true;
		}

		public void CloseFireDTVCommonInterfaceSupport()
		{
			uint returnCode = FireDTVAPI.FS_CI_Close(DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to Stop CI Support");
			_ciRunning = false;
		}

		public void SetFireDTVCommonInterfacePollingInterval(uint commonInterfacePollingInterval)
		{
			uint returnCode = FireDTVAPI.FS_CI_SetPollingIntervall(DeviceHandle,commonInterfacePollingInterval);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to set CI Polling Interval");
		}
		

		#endregion

	}
	public class SourceFilterCollection : System.Collections.CollectionBase
	{
		public void Add(FireDTVSourceFilterInfo sourceFilter)
		{
			foreach(FireDTVSourceFilterInfo srcFilter in List)
			{
				if (srcFilter.DisplayString == sourceFilter.DisplayString)
				{
					sourceFilter.Close();
					return;
				}
			}
			List.Add(sourceFilter);
		}

		public void Remove(int index)
		{
			if (index > Count - 1 || index < 0)
			{
				throw new IndexOutOfRangeException("Source Filter Index out of Bounds");
			}
			else
			{
				((FireDTVSourceFilterInfo)List[index]).Close();
				List.RemoveAt(index); 
			}
		}

		public FireDTVSourceFilterInfo Item(int index)
		{
			if (index > Count - 1 || index < 0)
			{
				throw new IndexOutOfRangeException("Source Filter Index out of Bounds");
			}
			else
			{
				return (FireDTVSourceFilterInfo)List[index];
			}
		}
	}



	/// <summary>
	/// Summary description for FireDTVControl.
	/// </summary>
	public class FireDTVBaseControl
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
		public FireDTVBaseControl(int windowHandle):base()
		{
      RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\DigitalEverywhere\\FireDTV");
      if (rkey != null)
      {
        string dllPath = rkey.GetValue("InstallFolder").ToString();
        string fullDllPath = string.Format("{0}{1}", dllPath.Substring(0, dllPath.LastIndexOf('\\') + 1), "Tools\\");
        bool success = SetDllDirectory(fullDllPath);
        MediaPortal.GUI.Library.Log.Write("FireDTV: Set DLL directory: {0} / {1}", fullDllPath, success);
      }

			_windowHandle = windowHandle;
			InitializeFireDTVLibrary();
			RegisterGeneralNotifications();

		}

		public FireDTVBaseControl():base()
		{
			InitializeFireDTVLibrary();
			RegisterGeneralNotifications();

		}

		~ FireDTVBaseControl()
		{
			CloseDrivers();
		}
		#endregion
		#region Private Methods
		public FireDTVSourceFilterInfo GetSourceFilterByDeviceHandle(uint deviceHandle)
		{
			foreach(FireDTVSourceFilterInfo SourceFilter in sourceFilterCollection)
				if (SourceFilter.DeviceHandle == deviceHandle)
					return SourceFilter;
			return null;
		}

		public FireDTVSourceFilterInfo GetSourceFilterByDisplayString(string displayString)
		{
			foreach(FireDTVSourceFilterInfo SourceFilter in sourceFilterCollection)
				if (SourceFilter.DisplayString == displayString)
					return SourceFilter;
			return null;
		}

		public FireDTVSourceFilterInfo GetSourceFilterByGUIDString(string guidString)
		{
			foreach(FireDTVSourceFilterInfo SourceFilter in sourceFilterCollection)
				if (SourceFilter.GUIDString == guidString)
					return SourceFilter;
			return null;
		}

		public int GetSourceFilterIndexByDeviceHandle(uint deviceHandle)
		{
			for(int iCount = 0; iCount < SourceFilterCollection.Count;iCount++)
			{
				FireDTVSourceFilterInfo SourceFilter = sourceFilterCollection.Item(iCount);
				if (SourceFilter.DeviceHandle == deviceHandle)
					return iCount;
			}
			return -1;
		}
		#region Initialization
    [System.Diagnostics.DebuggerStepThrough]
		internal void InitializeFireDTVLibrary()
		{
			if (!LibrayInitialized)
			{
				try
				{
					uint returnCode = FireDTVAPI.FS_Initialize();
					if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
						throw new FireDTVInitializationException("Initilization Failure (" + returnCode.ToString() + ")");
					LibrayInitialized = true;
				}
				catch(Exception){}
			}
		}
		#endregion
		#region FireDTV Open/Close Device
		internal uint OpenFireDTVWDMDevice(int deviceIndex)
		{
			uint DeviceHandle;
			uint returnCode = FireDTVAPI.FS_OpenWDMDeviceHandle((uint)deviceIndex,out DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVDeviceOpenException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Open WDM Device Error!");				
			return DeviceHandle;
		}

		internal uint OpenFireDTVBDADevice(int deviceIndex)
		{
			uint DeviceHandle;
			uint returnCode = FireDTVAPI.FS_OpenBDADeviceHandle((uint)deviceIndex,out DeviceHandle);
			if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
				throw new FireDTVDeviceOpenException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Open BDA Device Error!");				
			return DeviceHandle;
		}

		internal void CloseFireDTVHandle(FireDTVSourceFilterInfo currentSourceFilter)
		{
			try
			{
				uint returnCode = FireDTVAPI.FS_CloseDeviceHandle(currentSourceFilter.DeviceHandle);
				if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
					throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Device Close Failure");
			}
			catch(Exception){}
		}
		internal int getWDMCount()
		{
			try
			{
				uint WDMCount;
				uint returnCode = FireDTVAPI.FS_GetNumberOfWDMDevices(out WDMCount);
				if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
					throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to WDM Driver Count");
				return (int)WDMCount;
			}
			catch (Exception){}
			return 0;
		}

		internal int getBDACount()
		{
			try
			{
				uint BDACount;
				uint returnCode = FireDTVAPI.FS_GetNumberOfBDADevices(out BDACount);
				if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
					throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to BDA Driver Count");
				return (int)BDACount;
			}
			catch(Exception){}
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
					throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to unRegister Notifiations");
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
					uint returnCode = FireDTVAPI.FS_RegisterGeneralNotifications(this.WindowsHandle);
					if ((FireDTVConstants.FireDTVStatusCodes)returnCode != FireDTVConstants.FireDTVStatusCodes.Success)
						throw new FireDTVException((FireDTVConstants.FireDTVStatusCodes)returnCode,"Unable to Register General Notifiations");
					NotificationsRegistered = true;
				}
				catch(Exception){}
			}
		}
		#endregion
		#endregion
		#region Private Variables
		private bool					LibrayInitialized		= false;
		private bool					NotificationsRegistered	= false;
		private uint					commonInterfacePollingInterval	= 0;
		private int						_windowHandle			= 0;
		private SourceFilterCollection	sourceFilterCollection = new SourceFilterCollection();
		#endregion
		#region Properties
		public SourceFilterCollection SourceFilterCollection
		{
			get
			{
				return sourceFilterCollection;
			}
			set
			{
				sourceFilterCollection = value;
			}
		}

		public string APIVersion
		{
			get
			{
				return Marshal.PtrToStringAnsi(FireDTVAPI.FS_GetApiVersion());
			}
		}
		
		
		public uint CommonInterfacePollingInterval
		{
			get
			{
				return commonInterfacePollingInterval;
			}
			set
			{
				commonInterfacePollingInterval = value;
			}
		}
		public int WindowsHandle
		{
			get
			{
				if (_windowHandle == 0)
					return FireDTVAPI.GetActiveWindow();
				else
					return _windowHandle;
			}
		}
		#endregion
		#region Public Methods
		public void HandleWndProc(ref Message m) 
		{
			// Listen for operating system messages.
			switch ((FireDTVConstants.FireDTVWindowMessages)m.Msg)
			{
				case FireDTVConstants.FireDTVWindowMessages.CIDateTime :
					if(CIDateTime!= null)
						CIDateTime(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					break;

				case FireDTVConstants.FireDTVWindowMessages.CIMMI :
					if(CIMMICommand != null)
						CIMMICommand(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					break;

				case FireDTVConstants.FireDTVWindowMessages.CIModuleInserted :
					if(CIModuleInserted != null)
						CIModuleInserted(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					break;

				case FireDTVConstants.FireDTVWindowMessages.CIModuleReady :
					if(CIModuleReady != null)
						CIModuleReady(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					break;

				case FireDTVConstants.FireDTVWindowMessages.CIModuleRemoved :
					if(CIModuleRemoved != null)
						CIModuleRemoved(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					break;

				case FireDTVConstants.FireDTVWindowMessages.CIPMTReply :
					if(CIPMTReply != null)
						CIPMTReply(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					break;
			
				
				case FireDTVConstants.FireDTVWindowMessages.DeviceAttached :
					if(DeviceAttached != null)
						DeviceAttached(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					OpenDrivers();
					break;

				case FireDTVConstants.FireDTVWindowMessages.DeviceChanged :
					if(DeviceChanged != null)
						DeviceChanged(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
					OpenDrivers();
					break;
				
				case FireDTVConstants.FireDTVWindowMessages.DeviceDetached :
                    FireDTVSourceFilterInfo sourceInfo = GetSourceFilterByDeviceHandle((uint)m.WParam);
					if(DeviceDetached != null)
						DeviceDetached(this,new FireDTVEventArgs(sourceInfo));
					
					SourceFilterCollection.Remove(GetSourceFilterIndexByDeviceHandle(sourceInfo.DeviceHandle));
					break;
				
				case FireDTVConstants.FireDTVWindowMessages.RemoteControlEvent :
					if (RemoteControlCommand != null)
						RemoteControlCommand(this,new FireDTVRemoteControlEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam),m.LParam));
					break;             
			}
			if(Notification != null)
				Notification(this,new FireDTVEventArgs(GetSourceFilterByDeviceHandle((uint)m.WParam)));
			//base.WndProc(ref m);
		}
		
		public void OpenDrivers(IntPtr windowhandle)
		{
			_windowHandle = windowhandle.ToInt32();
			OpenDrivers();
		}
		public void OpenDrivers()
		{
			int BDADriverCount = getBDACount();
			int WDMDriverCount = getWDMCount();
			
			for(int BDACount = 0;BDACount < BDADriverCount; BDACount++)
			{
				FireDTVSourceFilterInfo bdaSourceFilter = new FireDTVSourceFilterInfo(OpenFireDTVBDADevice(BDACount),BDACount,_windowHandle);
				if (bdaSourceFilter != null)
					SourceFilterCollection.Add(bdaSourceFilter);
			}

			for(int WDMCount = 0;WDMCount < WDMDriverCount; WDMCount++)
			{
				FireDTVSourceFilterInfo wdmSourceFilter = new FireDTVSourceFilterInfo(OpenFireDTVWDMDevice(WDMCount),WDMCount,_windowHandle);
				if (wdmSourceFilter != null)
					SourceFilterCollection.Add(wdmSourceFilter);
			}


		}

		public void CloseDrivers()
		{
			for(int DeviceCount= 0;DeviceCount < SourceFilterCollection.Count;DeviceCount++)
			{
				FireDTVSourceFilterInfo SourceFilter = SourceFilterCollection.Item(DeviceCount);
				SourceFilterCollection.Remove(DeviceCount);
			}
		}
		
		public bool StopRemoteControlSupport()
		{
			foreach(FireDTVSourceFilterInfo SourceFilter in sourceFilterCollection)
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
		#region Public Events
		public event FireDTVEventHandler DeviceAttached;
		public event FireDTVEventHandler DeviceDetached;
		public event FireDTVEventHandler DeviceChanged;
		public event FireDTVEventHandler CIModuleInserted;
		public event FireDTVEventHandler CIModuleReady;
		public event FireDTVEventHandler CIModuleRemoved;
		public event FireDTVEventHandler CIMMICommand;
		public event FireDTVEventHandler CIDateTime;
		public event FireDTVEventHandler CIPMTReply;

		public event FireDTVRemoteControlEventHandler RemoteControlCommand;	
		public event FireDTVEventHandler Notification;
		#endregion
	}

	public class FireDTVControl : FireDTVBaseControl
	{
		public FireDTVControl(int windowHandle) : base(windowHandle)
		{
		}
		#region Private Variable
		
		#endregion
	}
}

