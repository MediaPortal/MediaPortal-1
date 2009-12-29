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
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using Microsoft.Win32.SafeHandles;

#pragma warning disable 618

namespace MediaPortal.Hardware
{
  public class Remote : Device
  {
    #region Constructor

    static Remote()
    {
      _deviceSingleton = new Remote();
      _deviceSingleton.Init();
    }

    #endregion Constructor

    #region Implementation

    private void Init()
    {
      try
      {
        _deviceClass = HidGuid;
        _doubleClickTime = GetDoubleClickTime();

        _deviceBuffer = new byte[256];

        _deviceWatcher = new DeviceWatcher();
        _deviceWatcher.Create();
        _deviceWatcher.Class = _deviceClass;
        _deviceWatcher.DeviceArrival += new DeviceEventHandler(OnDeviceArrival);
        _deviceWatcher.DeviceRemoval += new DeviceEventHandler(OnDeviceRemoval);
        _deviceWatcher.SettingsChanged += new SettingsChanged(OnSettingsChanged);
        _deviceWatcher.RegisterDeviceArrival();

        Open();
      }
      catch (Exception e)
      {
        Log.Info("Remote.Init: {0}", e.Message);
      }
    }

    protected override void Open()
    {
      string devicePath = FindDevice(_deviceClass);

      if (devicePath == null)
      {
        return;
      }
      if (LogVerbose)
      {
        Log.Info("MCE: Using: {0}", devicePath);
      }

      SafeFileHandle deviceHandle = CreateFile(devicePath, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open,
                                               FileFlag.Overlapped, 0);

      if (deviceHandle.IsInvalid)
      {
        throw new Exception(string.Format("Failed to open remote ({0})", GetLastError()));
      }

      _deviceWatcher.RegisterDeviceRemoval(deviceHandle);

      // open a stream from the device and begin an asynchronous read
      _deviceStream = new FileStream(deviceHandle, FileAccess.Read, 128, true);
      _deviceStream.BeginRead(_deviceBuffer, 0, _deviceBuffer.Length, new AsyncCallback(OnReadComplete), null);
    }

    private void OnReadComplete(IAsyncResult asyncResult)
    {
      try
      {
        int valEndRead;
        int devBufIndex;
        if (OSInfo.OSInfo.GetOSName() == OSInfo.OSInfo.OSList.Windows7)
        {
          valEndRead = 25;
          devBufIndex = 9;
        }
        else
        {
          valEndRead = 13;
          devBufIndex = 5;
        }

        if (_deviceStream.EndRead(asyncResult) == valEndRead && _deviceBuffer[1] == 1)
        {
          if (_deviceBuffer[devBufIndex] == (int)_doubleClickButton &&
              Environment.TickCount - _doubleClickTick <= _doubleClickTime)
          {
            if (DoubleClick != null)
            {
              DoubleClick(this, new RemoteEventArgs(_doubleClickButton));
            }
          }
          else
          {
            _doubleClickButton = (RemoteButton)_deviceBuffer[devBufIndex];
            _doubleClickTick = Environment.TickCount;

            if (Click != null)
            {
              Click(this, new RemoteEventArgs(_doubleClickButton));
            }
          }
        }
        // begin another asynchronous read from the device
        _deviceStream.BeginRead(_deviceBuffer, 0, _deviceBuffer.Length, new AsyncCallback(OnReadComplete), null);
      }
      catch (Exception)
      {
      }
    }

    private void OnSettingsChanged()
    {
      _doubleClickTime = GetDoubleClickTime();
    }

    #endregion Implementation

    #region Interop

    [DllImport("user32")]
    private static extern int GetDoubleClickTime();

    #endregion Interop

    #region Events

    public static RemoteEventHandler Click = null;
    public static RemoteEventHandler DoubleClick = null;

    #endregion Events

    #region Members

    private static Remote _deviceSingleton;
    private int _doubleClickTime = -1;
    private int _doubleClickTick = 0;
    private RemoteButton _doubleClickButton;

    #endregion Members
  }
}