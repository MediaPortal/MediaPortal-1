using System;

namespace Win32
{
  public static partial class Const
  {
    public const int WM_APPCOMMAND = 0x0319;
  }

  public static partial class Macro
  {
    public static int GET_APPCOMMAND_LPARAM(IntPtr lParam)
    {
      return ((short) HIWORD(lParam.ToInt32()) & ~Const.FAPPCOMMAND_MASK);
    }
  }
}