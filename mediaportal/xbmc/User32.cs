using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MediaPortal
{
  internal static class User32
  {
    private const int SW_HIDE = 0;
    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOWMINIMIZED = 2;
    private const int SW_SHOWMAXIMIZED = 3;
    private const int SW_RESTORE = 9;
    private const int WPF_RESTORETOMAXIMIZED = 2;


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


    /// <summary>
    /// Finds the specified window by its name (or caption). 
    /// </summary>
    /// <param name="_windowName">Name of the window to find and activate.</param>
    /// <returns>Window handle or 0 if window not found by that name.</returns>
    private static uint FindWindow(string _windowName)
    {
      // first, try to find the window by its window name or caption.
      uint hWnd = FindWindow(null, _windowName);
      if (hWnd <= 0)
      {
        Debug.Assert(false, "Couldn't find window handle for the specified window name.");
      }
      return hWnd;
    }


    /// <summary> 
    /// Finds the specified window by its name (or caption). Then brings it to 
    /// the foreground. 
    /// </summary> 
    /// <param name="_windowName">Name of the window to find and activate.</param> 
    public static void ActivateWindow(string _windowName)
    {
      // first, try to find the window by its window name or caption. 
      uint hWnd = FindWindow(_windowName);
      ActivateWindowByHandle(hWnd);
    }

    /// <summary> 
    /// Finds the specified window by its Process ID. Then brings it to 
    /// the foreground. 
    /// </summary> 
    /// <param name="_processID">Process ID of the window to find and activate.</param> 
    public static void ActivateWindowByProcID(int _processID)
    {
      uint hWnd = (uint) Process.GetProcessById(_processID).Handle;
      ActivateWindowByHandle(hWnd);
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

      if (windowPlacement.showCmd == SW_HIDE)
      {
        ShowWindow(_hWnd, SW_RESTORE);
      }
      else if (windowPlacement.showCmd == SW_SHOWMINIMIZED)
      {
        // if the window is minimized, then we need to restore it to its 
        // previous size. we also take into account whether it was 
        // previously maximized. 
        int showCmd = (windowPlacement.flags == WPF_RESTORETOMAXIMIZED) ? SW_SHOWMAXIMIZED : SW_SHOWNORMAL;
        ShowWindow(_hWnd, showCmd);
      }
      else
      {
        // if it's not minimized, then we just call SetForegroundWindow to 
        // bring it to the front. 
        SetForegroundWindow(_hWnd);
      }
    }

    #region External WIN32 APIs

    [DllImport("USER32.DLL", SetLastError = true)]
    private static extern uint ShowWindow(uint _hwnd, int _showCommand);

    [DllImport("USER32.DLL", SetLastError = true)]
    private static extern uint FindWindow(string _lpClassName, string _lpWindowName);

    [DllImport("USER32.DLL", SetLastError = true)]
    private static extern uint GetWindowPlacement(uint _hwnd, [Out] out WindowPlacement _lpwndpl);

    [DllImport("USER32.DLL", SetLastError = true)]
    private static extern uint SetForegroundWindow(uint _hwnd);

    #endregion
  }
}