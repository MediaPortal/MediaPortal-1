using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class Win32Functions
  {
    public const uint CREATE_ALWAYS = 2;
    public const uint CREATE_NEW = 1;
    public const uint FILE_SHARE_DELETE = 4;
    public const uint FILE_SHARE_READ = 1;
    public const uint FILE_SHARE_WRITE = 2;
    public const uint GENERIC_ALL = 0x10000000;
    public const uint GENERIC_EXECUTE = 0x20000000;
    public const uint GENERIC_READ = 0x80000000;
    public const uint GENERIC_WRITE = 0x40000000;
    public const int HWND_BROADCAST = 0xffff;
    public const uint IOCTL_IMON_DEVICE_ID = 0x22;
    public const uint IOCTL_IMON_FW_VER = 0x222014;
    public const uint IOCTL_IMON_RC_SET = 0x222010;
    public const uint IOCTL_IMON_READ = 0x222008;
    public const uint IOCTL_IMON_READ_RC = 0x222030;
    public const uint IOCTL_IMON_READ2 = 0x222034;
    public const uint IOCTL_IMON_WRITE = 0x222018;
    public const uint OPEN_ALWAYS = 4;
    public const uint OPEN_EXISTING = 3;
    public const int SC_MONITORPOWER = 0xf170;
    public const int SC_SCREENSAVE = 0xf140;
    public const uint SPECIFIC_RIGHTS_ALL = 0xffff;
    public const uint TRUNCATE_EXISTING = 5;
    public const int WM_DESTROY = 2;
    public const int WM_MOUSEMOVE = 0x200;
    public const int WM_PAINT = 15;
    public const int WM_SYSCOMMAND = 0x112;

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int CloseHandle(IntPtr hObject);

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFile(string FileName, uint DesiredAccess, uint ShareMode,
                                           IntPtr lpSecurityAttributes, uint CreationDisposition,
                                           uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int DeviceIoControl(IntPtr hDevice, uint IoControlCode, [In] byte[] InBuffer, uint InBufferSize,
                                             [In, Out] byte[] OutBuffer, uint OutBufferSize, ref uint BytesReturned,
                                             IntPtr Overlapped);

    public static void DisableScreenSaver()
    {
      SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED);
    }

    public static void EnableScreenSaver()
    {
      SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass,
                                              string lpszWindow);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    private static IntPtr GetNotificationAreaHandle()
    {
      IntPtr hwndNotifyArea = IntPtr.Zero;
      IntPtr hwndParent =
        FindWindowEx(
          FindWindowEx(FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null), IntPtr.Zero, "TrayNotifyWnd", null),
          IntPtr.Zero, "SysPager", null);
      if (hwndParent != IntPtr.Zero)
      {
        // Old style search
        hwndNotifyArea = FindWindowEx(hwndParent, IntPtr.Zero, null, "Notification Area");
        // Have we found a valid hwnd?
        if (hwndNotifyArea == IntPtr.Zero)
        {
          // Vista notifyarea
          hwndNotifyArea = FindWindowEx(hwndParent, IntPtr.Zero, "ToolbarWindow32", null);
        }
      }

      return hwndNotifyArea;
    }

    public static void RedrawNotificationArea()
    {
      RECT rect;
      IntPtr notificationAreaHandle = GetNotificationAreaHandle();
      GetClientRect(notificationAreaHandle, out rect);
      for (int i = 0; i < rect.Right; i += 5)
      {
        for (int j = 0; j < rect.Bottom; j += 5)
        {
          SendMessage(notificationAreaHandle, 0x200, 0, (uint)((j << 0x16) + i));
        }
      }
    }

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);

    [DllImport("Kernel32.DLL", CharSet = CharSet.Auto, SetLastError = true)]
    protected static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE state);

    [Flags]
    protected enum EXECUTION_STATE : uint
    {
      ES_CONTINUOUS = 0x80000000,
      ES_DISPLAY_REQUIRED = 2,
      ES_SYSTEM_REQUIRED = 1
    }

    public enum MonitorPowerState
    {
      LowPower = 1,
      off = 2,
      On = -1
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;

      public RECT(int left_, int top_, int right_, int bottom_)
      {
        Left = left_;
        Top = top_;
        Right = right_;
        Bottom = bottom_;
      }

      public int Height
      {
        get { return (Bottom - Top); }
      }

      public int Width
      {
        get { return (Right - Left); }
      }

      public Size Size
      {
        get { return new Size(Width, Height); }
      }

      public Point Location
      {
        get { return new Point(Left, Top); }
      }

      public Rectangle ToRectangle()
      {
        return Rectangle.FromLTRB(Left, Top, Right, Bottom);
      }

      public static RECT FromRectangle(Rectangle rectangle)
      {
        return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
      }

      public override int GetHashCode()
      {
        return (((Left ^ ((Top << 13) | (Top >> 0x13))) ^ ((Width << 0x1a) | (Width >> 6))) ^
                ((Height << 7) | (Height >> 0x19)));
      }

      public static implicit operator Rectangle(RECT rect)
      {
        return rect.ToRectangle();
      }

      public static implicit operator RECT(Rectangle rect)
      {
        return FromRectangle(rect);
      }
    }

    public static class ScreenSaver
    {
      private const uint DESKTOP_READOBJECTS = 1;
      private const uint DESKTOP_WRITEOBJECTS = 0x80;
      private const int SPI_GETSCREENSAVERACTIVE = 0x10;
      private const int SPI_GETSCREENSAVERRUNNING = 0x72;
      private const int SPI_GETSCREENSAVERTIMEOUT = 14;
      private const int SPI_SETSCREENSAVERACTIVE = 0x11;
      private const int SPI_SETSCREENSAVERTIMEOUT = 15;
      private const int SPIF_SENDWININICHANGE = 2;
      private const int WM_CLOSE = 0x10;

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern bool CloseDesktop(IntPtr hDesktop);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsProc callback, IntPtr lParam);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr GetForegroundWindow();

      public static bool GetScreenSaverActive()
      {
        bool lpvParam = false;
        SystemParametersInfo(0x10, 0, ref lpvParam, 0);
        return lpvParam;
      }

      public static bool GetScreenSaverRunning()
      {
        bool lpvParam = false;
        SystemParametersInfo(0x72, 0, ref lpvParam, 0);
        return lpvParam;
      }

      public static int GetScreenSaverTimeout()
      {
        int lpvParam = 0;
        SystemParametersInfo(14, 0, ref lpvParam, 0);
        return lpvParam;
      }

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern bool IsWindowVisible(IntPtr hWnd);

      public static void KillScreenSaver()
      {
        IntPtr hDesktop = OpenDesktop("Screen-saver", 0, false, 0x81);
        if (hDesktop != IntPtr.Zero)
        {
          EnumDesktopWindows(hDesktop, KillScreenSaverFunc, IntPtr.Zero);
          CloseDesktop(hDesktop);
        }
        else
        {
          PostMessage(GetForegroundWindow(), 0x10, 0, 0);
        }
      }

      private static bool KillScreenSaverFunc(IntPtr hWnd, IntPtr lParam)
      {
        if (IsWindowVisible(hWnd))
        {
          PostMessage(hWnd, 0x10, 0, 0);
        }
        return true;
      }

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern IntPtr OpenDesktop(string hDesktop, int Flags, bool Inherit, uint DesiredAccess);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern int PostMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

      public static void SetScreenSaverActive(bool MakeActive)
      {
        int lpvParam = 0;
        if (MakeActive)
        {
          SystemParametersInfo(0x11, 1, ref lpvParam, 0);
        }
        else
        {
          SystemParametersInfo(0x11, 0, ref lpvParam, 0);
        }
      }

      public static void SetScreenSaverTimeout(int Value)
      {
        int lpvParam = 0;
        SystemParametersInfo(15, Value, ref lpvParam, 2);
      }

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern bool SystemParametersInfo(int uAction, int uParam, ref bool lpvParam, int flags);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern bool SystemParametersInfo(int uAction, int uParam, ref int lpvParam, int flags);

      [return: MarshalAs(UnmanagedType.Bool)]
      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern bool SystemParametersInfo(int uiAction, int uParam, ref IntPtr lpvParam, int flags);

      private delegate bool EnumDesktopWindowsProc(IntPtr hDesktop, IntPtr lParam);
    }
  }
}