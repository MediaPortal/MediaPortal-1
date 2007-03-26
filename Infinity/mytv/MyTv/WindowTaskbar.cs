using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace MyTv
{
  public class WindowTaskbar
  {
    #region imports
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImportAttribute("user32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern int EnableWindow(IntPtr hwnd, int fEnable);
    #endregion

    #region constants
    static int SW_HIDE = 0;
    static int SW_SHOW = 5;
    #endregion

    /// <summary>
    /// Hides the windows taskbar.
    /// </summary>
    static public void Hide()
    {
      IntPtr TaskBarWnd;
      TaskBarWnd = FindWindow("Shell_TrayWnd", null);
      if (IsWindowVisible(TaskBarWnd))
      {
        EnableWindow(TaskBarWnd, 0);
        ShowWindow(TaskBarWnd, SW_HIDE);
      }
    }

    /// <summary>
    /// Shows the windows taskbar.
    /// </summary>
    static public void Show()
    {
      IntPtr TaskBarWnd;
      TaskBarWnd = FindWindow("Shell_TrayWnd", null);
      if (!IsWindowVisible(TaskBarWnd))
      {
        EnableWindow(TaskBarWnd, -1);
        ShowWindow(TaskBarWnd, SW_SHOW);
      }
    }

  }
}
