#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace HideVolumeOSD
{
	public class HideVolumeOSDLib
	{
		[DllImport("user32.dll")]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left;        // x position of upper-left corner
			public int top;         // y position of upper-left corner
			public int right;       // x position of lower-right corner
			public int bottom;      // y position of lower-right corner
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		IntPtr hWndInject = IntPtr.Zero;

    public HideVolumeOSDLib(bool IsMuted)
		{
      hWndInject = FindOSDWindow();
      
			if (hWndInject == IntPtr.Zero)
			{
        if (IsMuted)
        {
          keybd_event((byte)Keys.VolumeMute, 0, 0, 0);
          keybd_event((byte)Keys.VolumeMute, 0, 0, 0);
        }
        else
        {
          keybd_event((byte)Keys.VolumeUp, 0, 0, 0);
          keybd_event((byte)Keys.VolumeDown, 0, 0, 0);
        }

				hWndInject = FindOSDWindow();

        if (hWndInject == IntPtr.Zero)
        {
          Log.Error("HideVolumeOSD: VolumeOSD not found.");
        }
			}
		}

		private IntPtr FindOSDWindow()
		{
      IntPtr hwndRet = IntPtr.Zero;
      IntPtr hwndHost = IntPtr.Zero;

      // search for window with class 'NativeHWNDHost'

      int pairCount = 0;

      while ((hwndHost = FindWindowEx(IntPtr.Zero, hwndHost, "NativeHWNDHost", "")) != IntPtr.Zero)
      {
        // if this window has a child with class 'DirectUIHWND' it should be the volume OSD

        if (FindWindowEx(hwndHost, IntPtr.Zero, "DirectUIHWND", "") != IntPtr.Zero)
        {
          if (pairCount == 0)
          {
            hwndRet = hwndHost;
          }

          pairCount++;

          if (pairCount > 1)
          {
            MessageBox.Show("Severe error: Multiple pairs found!", "HideVolumeOSD");
            return IntPtr.Zero;
          }
        }
      }

      if (hwndRet == IntPtr.Zero)
      {
        MessageBox.Show("Severe error: OSD window not found!", "HideVolumeOSD");
      }

      return hwndRet;
		}

		public void HideOSD()
		{
			ShowWindow(hWndInject, 6); // SW_MINIMIZE
		}

		public void ShowOSD()
		{
			ShowWindow(hWndInject, 9); // SW_RESTORE
		}
	}
}
