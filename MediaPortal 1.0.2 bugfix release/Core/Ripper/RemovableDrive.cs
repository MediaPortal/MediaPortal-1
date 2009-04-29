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

namespace MediaPortal.Ripper
{
  /// <summary>
  /// 
  /// </summary>
  public class RemovableDrive : IDisposable
  {
    private DeviceChangeNotificationWindow NotWnd = null;

    public delegate void NotificationHandler(char DriveLetter);

    public event NotificationHandler VolumeInserted;
    public event NotificationHandler VolumeRemoved;

    public RemovableDrive()
    {
      NotWnd = new DeviceChangeNotificationWindow();
      NotWnd.DeviceChange += new DeviceChangeEventHandler(NotWnd_DeviceChange);
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    private void OnVolumeInserted(char DriveLetter)
    {
      if (VolumeInserted != null)
      {
        VolumeInserted(DriveLetter);
      }
    }

    private void OnVolumeRemoved(char DriveLetter)
    {
      if (VolumeRemoved != null)
      {
        VolumeRemoved(DriveLetter);
      }
    }

    private void NotWnd_DeviceChange(object sender, DeviceChangeEventArgs ea)
    {
      switch (ea.ChangeType)
      {
        case DeviceChangeEventType.VolumeInserted:
          OnVolumeInserted(ea.Drive);
          break;
        case DeviceChangeEventType.VolumeRemoved:
          OnVolumeRemoved(ea.Drive);
          break;
      }
    }
  }
}