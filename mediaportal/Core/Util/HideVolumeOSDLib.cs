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
			IntPtr childWindow = IntPtr.Zero;

			// search for window with class 'NativeHWNDHost'

			while ((childWindow = FindWindowEx(IntPtr.Zero, childWindow, "NativeHWNDHost", "")) != IntPtr.Zero)
			{
				// if this window has a child with class 'DirectUIHWND' it should be the volume OSD

				if (FindWindowEx(childWindow, IntPtr.Zero, "DirectUIHWND", "") != IntPtr.Zero)
				{
					// as a final check we test position and size range for normal or minimized osd window					

					RECT rect;
					GetWindowRect(childWindow, out rect);

					int width = rect.right - rect.left;
					int height = rect.bottom - rect.top;

					// 50,60 -> Size 65,140 / 69,144 / 160,28

					if (rect.left == 50 && rect.top == 60 &&
						((width >= 60 && width <= 80 && height >= 140 && height <= 150) || // normal
						 (width >= 160 && width <= 170 && height >= 25 && height <= 30)))  // minimized
					{
						// we have it!
						return childWindow;                        
					}
				}
			}

			return IntPtr.Zero;
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
