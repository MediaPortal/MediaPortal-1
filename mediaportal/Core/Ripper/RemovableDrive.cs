//
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  yetiicb@hotmail.com
//
//  Copyright (C) 2002-2003 Idael Cardoso. 
//

using System;
using System.Runtime.InteropServices;

namespace MediaPortal.Ripper
{
	/// <summary>
	/// 
	/// </summary>
	public class RemovableDrive: IDisposable
	{
		private DeviceChangeNotificationWindow NotWnd = null;

		public delegate void NotificationHandler(char DriveLetter);
		public event NotificationHandler VolumeInserted;
		public event NotificationHandler VolumeRemoved;
    
		public RemovableDrive()
		{
			NotWnd = new DeviceChangeNotificationWindow();
			NotWnd.DeviceChange +=new DeviceChangeEventHandler(NotWnd_DeviceChange);
		}
	 
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		private void OnVolumeInserted(char DriveLetter)
		{
			if ( VolumeInserted != null )
			{
				VolumeInserted(DriveLetter);
			}
		}

		private void OnVolumeRemoved(char DriveLetter)
		{
			if ( VolumeRemoved != null )
			{
				VolumeRemoved(DriveLetter);
			}
		}

		private void NotWnd_DeviceChange(object sender, DeviceChangeEventArgs ea)
		{
			switch ( ea.ChangeType )
			{
				case DeviceChangeEventType.VolumeInserted :
					OnVolumeInserted(ea.Drive);
					break;
				case DeviceChangeEventType.VolumeRemoved :
					OnVolumeRemoved(ea.Drive);
					break;
			}
		}
	}
}
