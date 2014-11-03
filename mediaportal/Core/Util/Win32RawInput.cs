using System;
using System.Runtime.InteropServices;

namespace Win32
{
  public static partial class Function
  {
    [DllImport("User32.dll", SetLastError = true)]
    public static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevice, uint uiNumDevices, uint cbSize);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize,
      uint cbSizeHeader);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern int GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);
  }


  public static partial class Macro
  {
    /// <summary>
    ///   Retrieves the input code from wParam in WM_INPUT.
    ///   See RIM_INPUT and RIM_INPUTSINK.
    /// </summary>
    /// <param name="wParam"></param>
    /// <returns></returns>
    public static int GET_RAWINPUT_CODE_WPARAM(IntPtr wParam)
    {
      return (wParam.ToInt32() & 0xff);
    }

    public static int GET_DEVICE_LPARAM(IntPtr lParam)
    {
      return ((ushort) (HIWORD(lParam.ToInt32()) & Const.FAPPCOMMAND_MASK));
    }

    public static int HIWORD(int val)
    {
      return ((val >> 16) & 0xffff);
    }
  }

  public static partial class Const
  {
    /// <summary>
    ///   Windows Messages
    /// </summary>
    public const int WM_KEYDOWN = 0x0100;

    public const int WM_INPUT = 0x00FF;

    /// <summary>
    ///   GetRawInputDeviceInfo pData points to a string that contains the device name.
    /// </summary>
    public const uint RIDI_DEVICENAME = 0x20000007;

    /// <summary>
    ///   GetRawInputDeviceInfo For this uiCommand only, the value in pcbSize is the character count (not the byte count).
    /// </summary>
    public const uint RIDI_DEVICEINFO = 0x2000000b;

    /// <summary>
    ///   GetRawInputDeviceInfo pData points to an RID_DEVICE_INFO structure.
    /// </summary>
    public const uint RIDI_PREPARSEDDATA = 0x20000005;

    /// <summary>
    ///   Data comes from a mouse.
    /// </summary>
    public const uint RIM_TYPEMOUSE = 0;

    /// <summary>
    ///   Data comes from a keyboard.
    /// </summary>
    public const uint RIM_TYPEKEYBOARD = 1;

    /// <summary>
    ///   Data comes from an HID that is not a keyboard or a mouse.
    /// </summary>
    public const uint RIM_TYPEHID = 2;

    public const int RID_INPUT = 0x10000003;
    public const int RID_HEADER = 0x10000005;

    /// <summary>
    ///   Possible value taken by wParam for WM_INPUT.
    ///   <para />
    ///   Input occurred while the application was in the foreground. The application must call DefWindowProc so the system can
    ///   perform cleanup.
    /// </summary>
    public const int RIM_INPUT = 0;

    /// <summary>
    ///   Possible value taken by wParam for WM_INPUT.
    ///   <para />
    ///   Input occurred while the application was not in the foreground. The application must call DefWindowProc so the system
    ///   can perform the cleanup.
    /// </summary>
    public const int RIM_INPUTSINK = 1;

    /// <summary>
    ///   If set, the application command keys are handled. RIDEV_APPKEYS can be specified only if RIDEV_NOLEGACY is specified
    ///   for a keyboard device.
    /// </summary>
    public const uint RIDEV_APPKEYS = 0x00000400;

    /// <summary>
    ///   If set, the mouse button click does not activate the other window.
    /// </summary>
    public const uint RIDEV_CAPTUREMOUSE = 0x00000200;

    /// <summary>
    ///   If set, this enables the caller to receive WM_INPUT_DEVICE_CHANGE notifications for device arrival and device
    ///   removal.
    ///   Windows XP:  This flag is not supported until Windows Vista
    /// </summary>
    public const uint RIDEV_DEVNOTIFY = 0x00002000;

    /// <summary>
    ///   If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only
    ///   affects a TLC whose usage page is already specified with RIDEV_PAGEONLY.
    /// </summary>
    public const uint RIDEV_EXCLUDE = 0x00000010;

    /// <summary>
    ///   If set, this enables the caller to receive input in the background only if the foreground application does not
    ///   process it. In other words, if the foreground application is not registered for raw input, then the background
    ///   application that is registered will receive the input.
    ///   Windows XP:  This flag is not supported until Windows Vista
    /// </summary>
    public const uint RIDEV_EXINPUTSINK = 0x00001000;

    /// <summary>
    ///   If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that
    ///   hwndTarget must be specified.
    /// </summary>
    public const uint RIDEV_INPUTSINK = 0x00000100;

    /// <summary>
    ///   If set, the application-defined keyboard device hotkeys are not handled. However, the system hotkeys; for example,
    ///   ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. RIDEV_NOHOTKEYS can be
    ///   specified even if RIDEV_NOLEGACY is not specified and hwndTarget is NULL.
    /// </summary>
    public const uint RIDEV_NOHOTKEYS = 0x00000200;

    /// <summary>
    ///   If set, this prevents any devices specified by usUsagePage or usUsage from generating legacy messages. This is only
    ///   for the mouse and keyboard. See Remarks.
    /// </summary>
    public const uint RIDEV_NOLEGACY = 0x00000030;

    /// <summary>
    ///   If set, this specifies all devices whose top level collection is from the specified usUsagePage. Note that usUsage
    ///   must be zero. To exclude a particular top level collection, use RIDEV_EXCLUDE.
    /// </summary>
    public const uint RIDEV_PAGEONLY = 0x00000020;

    /// <summary>
    ///   If set, this removes the top level collection from the inclusion list. This tells the operating system to stop
    ///   reading from a device which matches the top level collection.
    /// </summary>
    public const uint RIDEV_REMOVE = 0x00000001;

    public const int APPCOMMAND_BROWSER_BACKWARD = 1;
    public const int APPCOMMAND_VOLUME_MUTE = 8;
    public const int APPCOMMAND_VOLUME_DOWN = 9;
    public const int APPCOMMAND_VOLUME_UP = 10;
    public const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
    public const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
    public const int APPCOMMAND_MEDIA_STOP = 13;
    public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
    public const int APPCOMMAND_MEDIA_PLAY = 46;
    public const int APPCOMMAND_MEDIA_PAUSE = 47;
    public const int APPCOMMAND_MEDIA_RECORD = 48;
    public const int APPCOMMAND_MEDIA_FAST_FORWARD = 49;
    public const int APPCOMMAND_MEDIA_REWIND = 50;
    public const int APPCOMMAND_MEDIA_CHANNEL_UP = 51;
    public const int APPCOMMAND_MEDIA_CHANNEL_DOWN = 52;
    public const int FAPPCOMMAND_MASK = 0xF000;
    public const int FAPPCOMMAND_MOUSE = 0x8000;
    public const int FAPPCOMMAND_KEY = 0;
    public const int FAPPCOMMAND_OEM = 0x1000;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RAWINPUTDEVICE
  {
    [MarshalAs(UnmanagedType.U2)] public ushort usUsagePage;
    [MarshalAs(UnmanagedType.U2)] public ushort usUsage;
    [MarshalAs(UnmanagedType.U4)] public uint dwFlags;
    public IntPtr hwndTarget;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RAWINPUTHEADER
  {
    [MarshalAs(UnmanagedType.U4)] public int dwType;
    [MarshalAs(UnmanagedType.U4)] public int dwSize;
    public IntPtr hDevice;
    [MarshalAs(UnmanagedType.U4)] public int wParam;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RAWHID
  {
    [MarshalAs(UnmanagedType.U4)] public uint dwSizeHid;
    [MarshalAs(UnmanagedType.U4)] public uint dwCount;
    //
    //BYTE  bRawData[1];
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct BUTTONSSTR
  {
    [MarshalAs(UnmanagedType.U2)] public ushort usButtonFlags;
    [MarshalAs(UnmanagedType.U2)] public ushort usButtonData;
  }


  [StructLayout(LayoutKind.Explicit, Pack = 1)]
  public struct RAWMOUSE
  {
    [MarshalAs(UnmanagedType.U2)] [FieldOffset(0)] public ushort usFlags;
    [MarshalAs(UnmanagedType.U4)] [FieldOffset(4)] public uint ulButtons;
    [FieldOffset(4)] public BUTTONSSTR buttonsStr;
    [MarshalAs(UnmanagedType.U4)] [FieldOffset(8)] public uint ulRawButtons;
    [MarshalAs(UnmanagedType.U4)] [FieldOffset(12)] public int lLastX;
    [MarshalAs(UnmanagedType.U4)] [FieldOffset(16)] public int lLastY;
    [MarshalAs(UnmanagedType.U4)] [FieldOffset(20)] public uint ulExtraInformation;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RAWKEYBOARD
  {
    [MarshalAs(UnmanagedType.U2)] public ushort MakeCode;
    [MarshalAs(UnmanagedType.U2)] public ushort Flags;
    [MarshalAs(UnmanagedType.U2)] public ushort Reserved;
    [MarshalAs(UnmanagedType.U2)] public ushort VKey;
    [MarshalAs(UnmanagedType.U4)] public uint Message;
    [MarshalAs(UnmanagedType.U4)] public uint ExtraInformation;
  }


  [StructLayout(LayoutKind.Explicit, Pack = 1)]
  public struct RAWINPUT
  {
    [FieldOffset(0)] public RAWINPUTHEADER header;
    [FieldOffset(16)] public RAWMOUSE mouse;
    [FieldOffset(16)] public RAWKEYBOARD keyboard;
    [FieldOffset(16)] public RAWHID hid;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RID_DEVICE_INFO_MOUSE
  {
    public uint dwId;
    public uint dwNumberOfButtons;
    public uint dwSampleRate;
    public bool fHasHorizontalWheel;
  }


  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RID_DEVICE_INFO_KEYBOARD
  {
    public uint dwType;
    public uint dwSubType;
    public uint dwKeyboardMode;
    public uint dwNumberOfFunctionKeys;
    public uint dwNumberOfIndicators;
    public uint dwNumberOfKeysTotal;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct RID_DEVICE_INFO_HID
  {
    public uint dwVendorId;
    public uint dwProductId;
    public uint dwVersionNumber;
    public ushort usUsagePage;
    public ushort usUsage;
  }

  [StructLayout(LayoutKind.Explicit, Pack = 1)]
  public struct RID_DEVICE_INFO
  {
    [FieldOffset(0)] public uint cbSize;
    [FieldOffset(4)] public uint dwType;
    [FieldOffset(8)] public RID_DEVICE_INFO_MOUSE mouse;
    [FieldOffset(8)] public RID_DEVICE_INFO_KEYBOARD keyboard;
    [FieldOffset(8)] public RID_DEVICE_INFO_HID hid;
  }
}