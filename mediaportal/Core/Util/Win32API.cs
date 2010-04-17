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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Util
{
  /// <summary>
  /// Summary description for Win32API.
  /// </summary>
  public static class Win32API
  {
    #region Interop declarations

    #region Constants

    private const int WPF_RESTORETOMAXIMIZED = 2;
    public const int WM_SHOWWINDOW = 0x0018;
    private const int SHGFP_TYPE_CURRENT = 0;

    public const int CSIDL_MYMUSIC = 0x000d; // "My Music" folder
    public const int CSIDL_MYVIDEO = 0x000e; // "My Videos" folder
    public const int CSIDL_MYPICTURES = 0x0027; // "My Pictures" folder

    #endregion

    #region Methods

    //    [DllImportAttribute("kernel32", EntryPoint="RtlMoveMemory", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    //    public static extern void CopyMemory(ref KBDLLHOOKSTRUCT Destination, int Source, int Length);

    //   [DllImportAttribute("user32", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
    //   public static extern int GetKeyState(int nVirtKey);

    [DllImport("user32")]
    public static extern IntPtr SetWindowsHookEx(HookType code, HookDelegate func, IntPtr hInstance, int threadID);

    [DllImport("user32")]
    public static extern int UnhookWindowsHookEx(IntPtr hhook);

    [DllImport("user32")]
    public static extern int CallNextHookEx(IntPtr hhook, int code, int wParam, IntPtr lParam);

    [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

    [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
    public static extern IntPtr DeleteDC(IntPtr hDc);

    [DllImportAttribute("user32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref int lParam);

    [DllImportAttribute("user32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern int UnhookWindowsHookEx(int hHook);

    [DllImport("User32", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

    [DllImportAttribute("user32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern IntPtr GetWindow(IntPtr hwnd, ShowWindowFlags wCmd);

    [DllImport("user32", SetLastError = true)]
    private static extern uint GetWindowPlacement(uint _hwnd, [Out] out WindowPlacement _lpwndpl);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool PeekMessage([In, Out] ref MSG msg, IntPtr hwnd, int msgMin, int msgMax, int remove);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern bool GetMessageW([In, Out] ref MSG msg, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern bool GetMessageA([In, Out] ref MSG msg, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern bool TranslateMessage([In, Out] ref MSG msg);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern IntPtr DispatchMessageW([In] ref MSG msg);

    [DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern IntPtr DispatchMessageA([In] ref MSG msg);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetParent(HandleRef hWnd);

    [DllImport("user32", SetLastError = true)]
    public static extern uint ShowWindow(IntPtr hwnd, ShowWindowFlags showCommand);

    [DllImportAttribute("user32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern int EnableWindow(IntPtr hwnd, int fEnable);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr handle);

    [DllImport("user32.dll")]
    public static extern bool IsWindowEnabled(IntPtr handle);

    [DllImport("user32")]
    private static extern bool AttachThreadInput(int nThreadId, int nThreadIdTo, bool bAttach);

    [DllImport("user32")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int unused);

    [DllImport("kernel32.dll")]
    private static extern int GetCurrentThreadId();

    [DllImport("user32")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

    [DllImport("user32", SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr _hwnd);

    [DllImport("user32")]
    private static extern bool SetFocus(IntPtr hWnd);

    [DllImport("user32")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32", SetLastError = true)]
    public static extern bool PostThreadMessage(int idThread, uint Msg, uint wParam, uint lParam);

    [DllImport("wininet.dll")]
    private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);

    // Takes the CSIDL of a folder and returns the pathname.
    [DllImport("shell32.dll")]
    public static extern Int32 SHGetFolderPath(
      IntPtr hwndOwner, // Handle to an owner window.
      Int32 nFolder, // A CSIDL value that identifies the folder whose path is to be retrieved.
      IntPtr hToken, // An access token that can be used to represent a particular user.
      UInt32 dwFlags,
      // Flags to specify which path is to be returned. It is used for cases where the folder associated with a CSIDL may be moved or renamed by the user. 
      StringBuilder pszPath);

    // Pointer to a null-terminated string which will receive the path.

    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWow64Process(
      [In] IntPtr hProcess,
      [Out] out bool lpSystemInfo);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern int FindNextFile(IntPtr hFindFile, ref WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool FindClose(IntPtr hFindFile);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, [In()] ref COPYDATASTRUCT lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessageA(IntPtr hwnd, int wMsg, int wParam, int lParam);

    /// <summary>
    /// The SetDllDirectory function adds a directory to the search path used to locate DLLs for the application.
    /// http://msdn.microsoft.com/library/en-us/dllproc/base/setdlldirectory.asp
    /// </summary>
    /// <param name="PathName">Pointer to a null-terminated string that specifies the directory to be added to the search path.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    public static extern bool SetDllDirectory(string PathName);

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WIN32_FIND_DATA
    {
      public FileAttributes dwFileAttributes;
      public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
      public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
      public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
      public int nFileSizeHigh;
      public int nFileSizeLow;
      public int dwReserved0;
      public int dwReserved1;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string cFileName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
      public string cAlternate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
      public IntPtr hwnd;
      public int message;
      public IntPtr wParam;
      public IntPtr lParam;
      public int time;
      public int pt_x;
      public int pt_y;
    }

    /// <summary>
    /// Point struct used for GetWindowPlacement API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
      public int x;
      public int y;

      public Point(int _x, int _y)
      {
        x = _x;
        y = _y;
      }
    }

    /// <summary>
    /// Rect struct used for GetWindowPlacement API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Rectangle
    {
      public int x;
      public int y;
      public int right;
      public int bottom;

      public Rectangle(int _x, int _y, int _right, int _bottom)
      {
        x = _x;
        y = _y;
        right = _right;
        bottom = _bottom;
      }
    }

    /// <summary>
    /// WindowPlacement struct used for GetWindowPlacement API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPlacement
    {
      public uint length;
      public uint flags;
      public uint showCmd;
      public Point minPosition;
      public Point maxPosition;
      public Rectangle normalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
      public IntPtr dwData;
      public int cbData;
      public IntPtr lpData;
    }

    #endregion

    #region Enums

    public enum ShowWindowFlags
    {
      Hide = 0,
      ShowNormal = 1,
      ShowMinimized = 2,
      ShowMaximized = 3,
      ShowNoActivate = 4,
      Show = 5,
      Minimize = 6,
      ShowMinNoActivate = 7,
      ShowNotActivated = 8,
      Restore = 9,
      ShowDefault = 10,
      ForceMinimize = 11
    }

    public enum HookType
    {
      WH_JOURNALRECORD = 0,
      WH_JOURNALPLAYBACK = 1,
      WH_KEYBOARD = 2,
      WH_GETMESSAGE = 3,
      WH_CALLWNDPROC = 4,
      WH_CBT = 5,
      WH_SYSMSGFILTER = 6,
      WH_MOUSE = 7,
      WH_HARDWARE = 8,
      WH_DEBUG = 9,
      WH_SHELL = 10,
      WH_FOREGROUNDIDLE = 11,
      WH_CALLWNDPROCRET = 12,
      WH_KEYBOARD_LL = 13,
      WH_MOUSE_LL = 14
    }

    #endregion

    public delegate int HookDelegate(int code, int wParam, IntPtr lParam);

    #endregion

    #region Power Saving

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE flags);

    [FlagsAttribute]
    public enum EXECUTION_STATE : uint
    {
      ES_SYSTEM_REQUIRED = 0x00000001,
      ES_DISPLAY_REQUIRED = 0x00000002,
      // Legacy flag, should not be used.
      // ES_USER_PRESENT = 0x00000004,
      ES_CONTINUOUS = 0x80000000
    }

    public static void PreventMonitorPowerdown()
    {
      SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
    }

    public static void AllowMonitorPowerdown()
    {
      SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
    }

    #endregion

    #region x64

    public static bool Check64Bit()
    {
      //IsWow64Process is not supported under Windows2000 ( ver 5.0 )
      int osver = Environment.OSVersion.Version.Major * 10 + Environment.OSVersion.Version.Minor;
      if (osver <= 50) return false;

      Process p = Process.GetCurrentProcess();
      IntPtr handle = p.Handle;
      bool isWow64;
      bool success = IsWow64Process(handle, out isWow64);
      if (!success)
      {
        throw new Win32Exception();
      }
      return isWow64;
    }

    #endregion

    //Checks if the computer is connected to the internet...
    public static bool IsConnectedToInternet()
    {
#if DEBUG
      return true;
#else
      int Desc;
      return InternetGetConnectedState(out Desc, 0);
#endif
    }

    public static bool IsConnectedToInternet(ref int code)
    {
#if DEBUG
      return true;
#else
      return InternetGetConnectedState(out code, 0);
#endif
    }

    public static void Show(string ClassName, string WindowName, bool bVisible)
    {
      IntPtr i = FindWindow(ClassName, WindowName);
      if (bVisible)
      {
        ShowWindow(i, ShowWindowFlags.Show);
      }
      else
      {
        ShowWindow(i, ShowWindowFlags.Hide);
      }
    }

    public static void Enable(string ClassName, string WindowName, bool bEnable)
    {
      IntPtr i = FindWindow(ClassName, WindowName);
      if (bEnable)
      {
        EnableWindow(i, -1);
      }
      else
      {
        EnableWindow(i, 0);
      }
    }

    public static void ShowStartBar(bool bVisible)
    {
      try
      {
        Show("Shell_TrayWnd", "", bVisible);
      }
      catch (Exception) { }
    }

    public static void EnableStartBar(bool bEnable)
    {
      try
      {
        Enable("Shell_TrayWnd", "", bEnable);
      }
      catch (Exception) { }
    }

    /// <summary> 
    /// Finds the specified window by its Process ID. Then brings it to 
    /// the foreground. 
    /// </summary> 
    /// <param name="_hWnd">Handle to the window to find and activate.</param> 
    public static void ActivateWindowByHandle(uint _hWnd)
    {
      WindowPlacement windowPlacement;
      GetWindowPlacement(_hWnd, out windowPlacement);

      switch ((ShowWindowFlags)windowPlacement.showCmd)
      {
        case ShowWindowFlags.Hide: //Window is hidden
          ShowWindow((IntPtr)_hWnd, ShowWindowFlags.Restore);
          break;
        case ShowWindowFlags.ShowMinimized: //Window is minimized
          // if the window is minimized, then we need to restore it to its 
          // previous size. we also take into account whether it was 
          // previously maximized. 
          ShowWindowFlags showCmd = (windowPlacement.flags == WPF_RESTORETOMAXIMIZED) ? ShowWindowFlags.ShowMaximized : ShowWindowFlags.ShowNormal;
          ShowWindow((IntPtr)_hWnd, showCmd);
          break;
        default:
          // if it's not minimized, then we just call SetForegroundWindow to 
          // bring it to the front. 
          SetForegroundWindow((IntPtr)_hWnd);
          break;
      }
    }

    public static void ActivatePreviousInstance()
    {
      //Find the previous instance's process
      List<Process> processes = new List<Process>();
      string processName = Process.GetCurrentProcess().ProcessName;
      if (processName.EndsWith(".vshost"))
      {
        processName = processName.Substring(0, processName.Length - 7);
      }

      processes.AddRange(Process.GetProcessesByName(processName));
      processes.AddRange(Process.GetProcessesByName(processName + ".vshost"));

      foreach (Process process in processes)
      {
        if (process.Id == Process.GetCurrentProcess().Id)
        {
          continue;
        }
        //Instructs the process to go to the foreground 
        SetForeGround(process);
        Environment.Exit(0);
      }
      Log.Info("Main: Could not activate running instance");
      MessageBox.Show("Could not activate running instance.", string.Format("{0} is already running", processName),
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Warning);
      Environment.Exit(0);
    }

    /// <summary>
    /// Brings the previous MP instance to the front.
    /// </summary>
    /// <param name="_process">The <see cref="Process"/> that represents the previous MP instance</param>
    /// <remarks>
    /// We bring the application to the front by sending a WM_SHOWWINDOW message to all of it's threads.
    /// The main thread will detect this message using the <see cref="ThreadMessageFilter"/> and
    /// will instruct it's main window to show and activate itself.
    /// </remarks>
    private static void SetForeGround(Process _process)
    {
      foreach (ProcessThread thread in _process.Threads)
      {
        Win32API.PostThreadMessage(thread.Id, Win32API.WM_SHOWWINDOW, 0, 0);
      }
    }

    public static bool SetForegroundWindow(IntPtr window, bool force)
    {
      IntPtr windowForeground = GetForegroundWindow();

      if (window == windowForeground)
      {
        return true;
      }

      bool setAttempt = SetForegroundWindow(window);

      if (force == false || setAttempt)
      {
        return setAttempt;
      }

      if (windowForeground == IntPtr.Zero)
      {
        return false;
      }

      // if we don't attach successfully to the windows thread then we're out of options
      int processId;
      int fgWindowPID = GetWindowThreadProcessId(windowForeground, out processId);

      if (fgWindowPID == -1)
      {
        return false;
      }

      // If we don't attach successfully to the windows thread then we're out of options
      int curThreadID = GetCurrentThreadId();
      bool attached = AttachThreadInput(curThreadID, fgWindowPID, true);
      int lastError = Marshal.GetLastWin32Error();

      if (!attached)
      {
        return false;
      }

      SetForegroundWindow(window);
      BringWindowToTop(window);
      SetFocus(window);

      AttachThreadInput(curThreadID, fgWindowPID, false);

      // we've done all that we can so base our return value on whether we have succeeded or not
      return (GetForegroundWindow() == window);
    }

    public static string GetFolderPath(int csidl)
    {
      StringBuilder folder = new System.Text.StringBuilder(256);
      SHGetFolderPath(IntPtr.Zero, csidl, IntPtr.Zero, SHGFP_TYPE_CURRENT, folder);
      return folder.ToString();
    }
  }
}