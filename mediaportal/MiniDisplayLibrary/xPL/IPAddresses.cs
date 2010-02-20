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

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  internal class IPAddresses
  {
    [DllImport("Iphlpapi.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    public static extern int GetIpAddrTable(IntPtr pAddrTable, ref int pdwSize, bool bOrder);

    public static ArrayList LocalIPAddresses(EventLog ErrorLog)
    {
      IntPtr zero = IntPtr.Zero;
      int pdwSize = 0;
      ArrayList list = new ArrayList(10);
      long newAddress = 0L;
      try
      {
        GetIpAddrTable(zero, ref pdwSize, false);
        zero = Marshal.AllocHGlobal(pdwSize);
        GetIpAddrTable(zero, ref pdwSize, false);
        int num3 = (int)Marshal.PtrToStructure(zero, typeof (int));
        for (int i = 0; i < num3; i++)
        {
          _MIB_IPADDRROW _mib_ipaddrrow =
            (_MIB_IPADDRROW)
            Marshal.PtrToStructure((IntPtr)((zero.ToInt32() + 4) + (i * Marshal.SizeOf(typeof (_MIB_IPADDRROW)))),
                                   typeof (_MIB_IPADDRROW));
          newAddress = long.Parse(_mib_ipaddrrow.dwAddr.ToString());
          list.Add(new IPAddress(newAddress).ToString());
        }
      }
      catch (Exception exception)
      {
        if (ErrorLog != null)
        {
          ErrorLog.WriteEntry("Error looking for local ip address(es): " + exception.Message);
        }
      }
      finally
      {
        if (!zero.Equals(IntPtr.Zero))
        {
          Marshal.FreeHGlobal(zero);
        }
      }
      if (list.Count < 1)
      {
        list.Add(IPAddress.Loopback.ToString());
        return list;
      }
      if ((list.Count > 1) & (list[0].ToString() == IPAddress.Loopback.ToString()))
      {
        list[0] = list[1];
        list[1] = IPAddress.Loopback.ToString();
      }
      return list;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _MIB_IPADDRROW
    {
      public uint dwAddr;
      public uint dwIndex;
      public uint dwMask;
      public uint dwBCastAddr;
      public uint dwReasmSize;
      public short unused1;
      public short unused2;
    }
  }
}