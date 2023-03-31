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
      if (IntPtr.Size == 8)
        return ((short)HIWORD(lParam.ToInt64()) & ~Const.FAPPCOMMAND_MASK);
      else
        return ((short)HIWORD(lParam.ToInt32()) & ~Const.FAPPCOMMAND_MASK);
    }
  }
}