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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MediaPortal.Ripper
{
  public class DataReadEventArgs : EventArgs
  {
    private byte[] _data;
    private uint _dataSize;

    public DataReadEventArgs(byte[] data, uint size)
    {
      _data = data;
      _dataSize = size;
    }

    public byte[] Data
    {
      get { return _data; }
    }

    public uint DataSize
    {
      get { return _dataSize; }
    }
  }

  public class ReadProgressEventArgs : EventArgs
  {
    private uint m_Bytes2Read;
    private uint m_BytesRead;
    private bool m_CancelRead = false;

    public ReadProgressEventArgs(uint bytes2read, uint bytesread)
    {
      m_Bytes2Read = bytes2read;
      m_BytesRead = bytesread;
    }

    public uint Bytes2Read
    {
      get { return m_Bytes2Read; }
    }

    public uint BytesRead
    {
      get { return m_BytesRead; }
    }

    public bool CancelRead
    {
      get { return m_CancelRead; }
      set { m_CancelRead = value; }
    }
  }

  internal enum DeviceChangeEventType
  {
    MediaInserted,
    MediaRemoved,
    VolumeInserted,
    VolumeRemoved
  } ;

  internal class DeviceChangeEventArgs : EventArgs
  {
    private DeviceChangeEventType m_Type;
    private char m_Drive;

    public DeviceChangeEventArgs(char drive, DeviceChangeEventType type)
    {
      m_Drive = drive;
      m_Type = type;
    }

    public char Drive
    {
      get { return m_Drive; }
    }

    public DeviceChangeEventType ChangeType
    {
      get { return m_Type; }
    }
  }

  public delegate void CdDataReadEventHandler(object sender, DataReadEventArgs ea);

  public delegate void CdReadProgressEventHandler(object sender, ReadProgressEventArgs ea);

  internal delegate void DeviceChangeEventHandler(object sender, DeviceChangeEventArgs ea);

  internal enum DeviceType : uint
  {
    DBT_DEVTYP_OEM = 0x00000000, // oem-defined device type
    DBT_DEVTYP_DEVNODE = 0x00000001, // devnode number
    DBT_DEVTYP_VOLUME = 0x00000002, // logical volume
    DBT_DEVTYP_PORT = 0x00000003, // serial, parallel
    DBT_DEVTYP_NET = 0x00000004 // network resource
  }

  internal enum VolumeChangeFlags : ushort
  {
    DBTF_MEDIA = 0x0001, // media comings and goings
    DBTF_NET = 0x0002 // network volume
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct DEV_BROADCAST_HDR
  {
    public uint dbch_size;
    public DeviceType dbch_devicetype;
    private uint dbch_reserved;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct DEV_BROADCAST_VOLUME
  {
    public uint dbcv_size;
    public DeviceType dbcv_devicetype;
    private uint dbcv_reserved;
    private uint dbcv_unitmask;

    public char[] Drives
    {
      get
      {
        string drvs = "";
        for (char c = 'A'; c <= 'Z'; c++)
        {
          if ((dbcv_unitmask & (1 << (c - 'A'))) != 0)
          {
            drvs += c;
          }
        }
        return drvs.ToCharArray();
      }
    }

    public VolumeChangeFlags dbcv_flags;
  }

  internal class DeviceChangeNotificationWindow : NativeWindow
  {
    public event DeviceChangeEventHandler DeviceChange;

    private const int WS_EX_TOOLWINDOW = 0x80;
    private const int WS_POPUP = unchecked((int)0x80000000);

    private const int WM_DEVICECHANGE = 0x0219;

    private const int DBT_APPYBEGIN = 0x0000;
    private const int DBT_APPYEND = 0x0001;
    private const int DBT_DEVNODES_CHANGED = 0x0007;
    private const int DBT_QUERYCHANGECONFIG = 0x0017;
    private const int DBT_CONFIGCHANGED = 0x0018;
    private const int DBT_CONFIGCHANGECANCELED = 0x0019;
    private const int DBT_MONITORCHANGE = 0x001B;
    private const int DBT_SHELLLOGGEDON = 0x0020;
    private const int DBT_CONFIGMGAPI32 = 0x0022;
    private const int DBT_VXDINITCOMPLETE = 0x0023;
    private const int DBT_VOLLOCKQUERYLOCK = 0x8041;
    private const int DBT_VOLLOCKLOCKTAKEN = 0x8042;
    private const int DBT_VOLLOCKLOCKFAILED = 0x8043;
    private const int DBT_VOLLOCKQUERYUNLOCK = 0x8044;
    private const int DBT_VOLLOCKLOCKRELEASED = 0x8045;
    private const int DBT_VOLLOCKUNLOCKFAILED = 0x8046;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEQUERYREMOVE = 0x8001;
    private const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
    private const int DBT_DEVICEREMOVEPENDING = 0x8003;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private const int DBT_DEVICETYPESPECIFIC = 0x8005;

    public DeviceChangeNotificationWindow()
    {
      CreateParams Params = new CreateParams();
      Params.ExStyle = WS_EX_TOOLWINDOW;
      Params.Style = WS_POPUP;
      CreateHandle(Params);
    }

    private void OnCDChange(DeviceChangeEventArgs ea)
    {
      if (DeviceChange != null)
      {
        DeviceChange(this, ea);
      }
    }

    private void OnDeviceChange(DEV_BROADCAST_VOLUME DevDesc, DeviceChangeEventType EventType)
    {
      if (DeviceChange != null)
      {
        foreach (char ch in DevDesc.Drives)
        {
          DeviceChangeEventArgs a = new DeviceChangeEventArgs(ch, EventType);
          DeviceChange(this, a);
        }
      }
    }

    protected override void WndProc(ref Message m)
    {
      if (m.Msg == WM_DEVICECHANGE)
      {
        DEV_BROADCAST_HDR head;
        switch (m.WParam.ToInt32())
        {
            /*case DBT_DEVNODES_CHANGED :
            break;
          case DBT_CONFIGCHANGED :
            break;*/
          case DBT_DEVICEARRIVAL:
            head = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_HDR));
            if (head.dbch_devicetype == DeviceType.DBT_DEVTYP_VOLUME)
            {
              DEV_BROADCAST_VOLUME DevDesc =
                (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_VOLUME));
              switch (DevDesc.dbcv_flags)
              {
                case VolumeChangeFlags.DBTF_MEDIA:
                  OnDeviceChange(DevDesc, DeviceChangeEventType.MediaInserted);
                  break;

                case VolumeChangeFlags.DBTF_NET:
                  break;

                default:
                  OnDeviceChange(DevDesc, DeviceChangeEventType.VolumeInserted);
                  break;
              }
            }
            break;
            /*case DBT_DEVICEQUERYREMOVE :
            break;
          case DBT_DEVICEQUERYREMOVEFAILED :
            break;
          case DBT_DEVICEREMOVEPENDING :
            break;*/
          case DBT_DEVICEREMOVECOMPLETE:
            head = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_HDR));
            if (head.dbch_devicetype == DeviceType.DBT_DEVTYP_VOLUME)
            {
              DEV_BROADCAST_VOLUME DevDesc =
                (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_VOLUME));
              switch (DevDesc.dbcv_flags)
              {
                case VolumeChangeFlags.DBTF_MEDIA:
                  OnDeviceChange(DevDesc, DeviceChangeEventType.MediaRemoved);
                  break;

                case VolumeChangeFlags.DBTF_NET:
                  break;

                default:
                  OnDeviceChange(DevDesc, DeviceChangeEventType.VolumeRemoved);
                  break;
              }
            }
            break;
            /*case DBT_DEVICETYPESPECIFIC :
            break;*/
        }
      }
      base.WndProc(ref m);
    }
  }
}