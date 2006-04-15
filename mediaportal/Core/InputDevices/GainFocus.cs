#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Diagnostics;
using MediaPortal.GUI.Library;


namespace MediaPortal.InputDevices
{
  static class GainFocus
  {
    #region Interop

    [DllImport("user32")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32")]
    static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32")]
    private static extern bool AttachThreadInput(int nThreadId, int nThreadIdTo, bool bAttach);

    [DllImport("user32")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, int unused);

    [DllImport("user32")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    #endregion

    public static void MainWindow()
    {
      IntPtr window = GUIGraphicsContext.ActiveForm;
      if (window == IntPtr.Zero)
        window = Process.GetCurrentProcess().MainWindowHandle;
      if (window == IntPtr.Zero)
        Log.Write("FOCUS: Invalid window handle for getting focus");
      else
      {
        if (IsIconic(window) || !IsWindowVisible(window))
          ShowWindowAsync(window, 0x9);
        if (GetForegroundWindow() != window)
          SetForegroundWindow(window, true);
        Log.Write("FOCUS: Gained focus");
      }
    }

    static bool SetForegroundWindow(IntPtr window, bool force)
    {
      IntPtr windowForeground = GetForegroundWindow();

      if (window == windowForeground || SetForegroundWindow(window))
        return true;

      if (force == false)
        return false;

      if (windowForeground == IntPtr.Zero)
        return false;

      // if we don't attach successfully to the windows thread then we're out of options
      if (!AttachThreadInput(System.Threading.Thread.CurrentThread.ManagedThreadId, GetWindowThreadProcessId(windowForeground, 0), true))
        return false;

      SetForegroundWindow(window);
      BringWindowToTop(window);

      AttachThreadInput(System.Threading.Thread.CurrentThread.ManagedThreadId, GetWindowThreadProcessId(windowForeground, 0), false);

      // we've done all that we can so base our return value on whether we have succeeded or not
      return (GetForegroundWindow() == window);
    }

  }
}
