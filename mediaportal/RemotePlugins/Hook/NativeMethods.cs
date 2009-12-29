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

namespace MediaPortal.Hooks
{
    public class NativeMethods
    {
        #region Constructors

        private NativeMethods()
        {
        }

        #endregion Constructors

        #region Enums

        public enum ShowWindowFlags
        {
            Hide = 0,
            ShowNormal = 1,
            Normal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11,
            Max = 11,
        }

        #endregion Enums

        #region Imports

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr securityAttributes, bool manualReset, bool initialState, string name);

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr handle, bool enable);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool IsWindowEnabled(IntPtr handle);

        [DllImport("user32")]
        public static extern bool ShowWindow(IntPtr handle, ShowWindowFlags showCommand);

        [DllImport("user32")]
        internal static extern IntPtr SetWindowsHookEx(HookType code, HookDelegate func, IntPtr hInstance, int threadID);

        [DllImport("user32")]
        internal static extern int UnhookWindowsHookEx(IntPtr hhook);

        [DllImport("user32")]
        internal static extern int CallNextHookEx(IntPtr hhook, int code, int wParam, IntPtr lParam);

        [DllImport("user32")]
        private static extern bool SetFocus(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32")]
        private static extern bool AttachThreadInput(int nThreadId, int nThreadIdTo, bool bAttach);

        [DllImport("user32")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, int unused);

        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);

        #endregion Imports

        #region Native Methods

        public static bool SetForegroundWindow(IntPtr window, bool force)
        {
            IntPtr windowForeground = GetForegroundWindow();

            if (window == windowForeground || SetForegroundWindow(window))
                return true;

            if (force == false)
                return false;

            if (windowForeground == IntPtr.Zero)
                return false;

            // if we don't attach successfully to the windows thread then we're out of options
            if (!AttachThreadInput(AppDomain.GetCurrentThreadId(), GetWindowThreadProcessId(windowForeground, 0), true))
                return false;

            SetForegroundWindow(window);
            BringWindowToTop(window);
            SetFocus(window);

            AttachThreadInput(AppDomain.GetCurrentThreadId(), GetWindowThreadProcessId(windowForeground, 0), false);

            // we've done all that we can so base our return value on whether we have succeeded or not
            return (GetForegroundWindow() == window);
        }

        #endregion
    }
}