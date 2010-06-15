using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Windows
{
  static class WindowManagement
  {
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern Boolean EnumChildWindows(int hWndParent, Delegate lpEnumFunc, int lParam);
    
    [DllImport("user32.dll")]
    public static extern Int32 GetWindowText(int hWnd, StringBuilder s, int nMaxCount);
    
    [DllImport("user32.dll")]
    public static extern Int32 GetWindowTextLength(int hwnd);

    [DllImport("User32.Dll")]
    public static extern void GetClassName(int hwnd, StringBuilder s, int nMaxCount);

    [DllImport("User32.Dll")]
    public static extern IntPtr GetWindow(IntPtr hwnd, int wFlag);
    
    public const int SW_HIDE = 0; // Hide the window. 
    public const int SW_MAXIMIZE = 3; // Maximize the window. 
    public const int SW_MINIMIZE = 6; // Minimize the window. 
    public const int SW_RESTORE = 9; // Restore the window (not maximized nor minimized). 
    public const int SW_SHOW = 5; // Show the window. 
    public const int SW_SHOWMAXIMIZED = 3; // Show the window maximized. 
    public const int SW_SHOWMINIMIZED = 2; // Show the window minimized. 
    public const int SW_SHOWMINNOACTIVE = 7; // Show the window minimized but do not activate it. 
    public const int SW_SHOWNA = 8; // Show the window in its current state but do not activate it. 
    public const int SW_SHOWNOACTIVATE = 4; // Show the window in its most recent size and position but do not activate it. 
    public const int SW_SHOWNORMAL = 1; // Show the window and activate it (as usual). 

    private const int GW_HWNDNEXT = 2;

    public static string GetClassName(int hwnd)
    {
      StringBuilder sb = new StringBuilder(1024);
      StringBuilder sbc = new StringBuilder(256);
      GetClassName(hwnd, sbc, sbc.Capacity);

      return sbc.ToString();
    }
    public static bool GetHandleFromPartialCaption(ref IntPtr lWnd, string SCaption)
    {
      IntPtr lhWndP;

      lhWndP = FindWindow(null, null); //PARENT WINDOW

      while (lhWndP != IntPtr.Zero)
      {
        StringBuilder sWindowName = new StringBuilder(256);
        int sWindowNameLength = GetWindowTextLength((int)lhWndP) + 1;
        WindowManagement.GetWindowText((int)lhWndP, sWindowName, sWindowNameLength);
        string tmpString = sWindowName.ToString().Trim();

        if (tmpString.Contains(SCaption))
        {
          lWnd = lhWndP;
          return true;
        }

        lhWndP = GetWindow(lhWndP, GW_HWNDNEXT);
      }
      return false;
    }
  }
}
