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
using System.Drawing;
using System.Runtime.InteropServices;

namespace Pabo.MozBar
{
  [StructLayout(LayoutKind.Sequential)]
  public struct DLLVERSIONINFO
  {
    public int cbSize;
    public int dwMajorVersion;
    public int dwMinorVersion;
    public int dwBuildNumber;
    public int dwPlatformID;
  }

  /// <summary>
  /// Summary description for ThemeManager.
  /// </summary>
  public class ThemeManager
  {
    // Declare functions used in uxTheme.dll and ComCtl32.dll

    [DllImport("uxTheme.dll", EntryPoint = "GetThemeColor", ExactSpelling = true, PreserveSig = false,
      CharSet = CharSet.Unicode)]
    private static extern void GetThemeColor(IntPtr hTheme,
                                             int partID,
                                             int stateID,
                                             int propID,
                                             out int color);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr OpenThemeData(IntPtr hwnd, string classes);

    [DllImport("uxtheme.dll", EntryPoint = "CloseThemeData", ExactSpelling = true, PreserveSig = false,
      CharSet = CharSet.Unicode)]
    private static extern int CloseThemeData(IntPtr hwnd);

    [DllImport("uxtheme.dll", EntryPoint = "GetWindowTheme", ExactSpelling = true, PreserveSig = false,
      CharSet = CharSet.Unicode)]
    private static extern int GetWindowTheme(IntPtr hWnd);

    [DllImport("uxtheme.dll", EntryPoint = "IsThemeActive", ExactSpelling = true, PreserveSig = false,
      CharSet = CharSet.Unicode)]
    private static extern bool IsThemeActive();

    [DllImport("Comctl32.dll", EntryPoint = "DllGetVersion", ExactSpelling = true, PreserveSig = false,
      CharSet = CharSet.Unicode)]
    private static extern int DllGetVersion(ref DLLVERSIONINFO s);

    public ThemeManager() {}

    public bool _IsAppThemed()
    {
      try
      {
        // Check which version of ComCtl32 thats in use..
        DLLVERSIONINFO version = new DLLVERSIONINFO();
        version.cbSize = Marshal.SizeOf(typeof (DLLVERSIONINFO));

        int ret = DllGetVersion(ref version);
        // If MajorVersion > 5 themes are allowed.
        if (version.dwMajorVersion >= 6)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception)
      {
        return false;
      }
    }

    public void _CloseThemeData(IntPtr hwnd)
    {
      try
      {
        CloseThemeData(hwnd);
      }
      catch (Exception) {}
    }

    public IntPtr _OpenThemeData(IntPtr hwnd, string classes)
    {
      try
      {
        return OpenThemeData(hwnd, classes);
      }
      catch (Exception)
      {
        return IntPtr.Zero;
      }
    }

    public int _GetWindowTheme(IntPtr hwnd)
    {
      try
      {
        return GetWindowTheme(hwnd);
      }
      catch (Exception)
      {
        return -1;
      }
    }

    public bool _IsThemeActive()
    {
      try
      {
        return IsThemeActive();
      }
      catch (Exception)
      {
        return false;
      }
    }

    public Color _GetThemeColor(IntPtr hTheme, int partID, int stateID, int propID)
    {
      int color;

      try
      {
        GetThemeColor(hTheme, partID, stateID, propID, out color);
        return ColorTranslator.FromWin32(color);
      }
      catch (Exception)
      {
        return Color.Empty;
      }
    }
  }
}