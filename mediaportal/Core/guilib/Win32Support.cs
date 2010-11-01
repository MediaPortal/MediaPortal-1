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
using System.Runtime.InteropServices;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Win32Support is a wrapper class that imports all the 
  /// necessary functions that are used in old
  /// double-buffering technique for smooth animation.
  /// </summary>
  public class Win32Support
  {
    /// <summary>
    /// Enumeration to be used for those Win32 function 
    /// that return BOOL
    /// </summary>
    public enum Bool
    {
      False = 0,
      True
    } ;

    /// <summary>
    /// Enumeration for the raster operations used in BitBlt.
    /// In C++ these are actually #define. But to use these
    /// constants with C#, a new enumeration type is defined.
    /// </summary>
    public enum TernaryRasterOperations
    {
      SRCCOPY = 0x00CC0020, // dest = source
      SRCPAINT = 0x00EE0086, // dest = source OR dest
      SRCAND = 0x008800C6, // dest = source AND dest
      SRCINVERT = 0x00660046, // dest = source XOR dest
      SRCERASE = 0x00440328, // dest = source AND (NOT dest)
      NOTSRCCOPY = 0x00330008, // dest = (NOT source)
      NOTSRCERASE = 0x001100A6, // dest = (NOT src) AND (NOT dest)
      MERGECOPY = 0x00C000CA, // dest = (source AND pattern)
      MERGEPAINT = 0x00BB0226, // dest = (NOT source) OR dest
      PATCOPY = 0x00F00021, // dest = pattern
      PATPAINT = 0x00FB0A09, // dest = DPSnoo
      PATINVERT = 0x005A0049, // dest = pattern XOR dest
      DSTINVERT = 0x00550009, // dest = (NOT dest)
      BLACKNESS = 0x00000042, // dest = BLACK
      WHITENESS = 0x00FF0062, // dest = WHITE
    } ;

    /// <summary>
    /// CreateCompatibleDC
    /// </summary>
    [DllImport("gdi32.dll", ExactSpelling = true,
      SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    /// <summary>
    /// DeleteDC
    /// </summary>
    [DllImport("gdi32.dll", ExactSpelling = true,
      SetLastError = true)]
    public static extern Bool DeleteDC(IntPtr hdc);

    /// <summary>
    /// SelectObject
    /// </summary>
    [DllImport("gdi32.dll", ExactSpelling = true)]
    public static extern IntPtr SelectObject(IntPtr hDC,
                                             IntPtr hObject);

    /// <summary>
    /// DeleteObject
    /// </summary>
    [DllImport("gdi32.dll", ExactSpelling = true,
      SetLastError = true)]
    public static extern Bool DeleteObject(IntPtr hObject);

    /// <summary>
    /// CreateCompatibleBitmap
    /// </summary>
    [DllImport("gdi32.dll", ExactSpelling = true,
      SetLastError = true)]
    public static extern IntPtr CreateCompatibleBitmap(
      IntPtr hObject, int width, int height);

    /// <summary>
    /// BitBlt
    /// </summary>
    [DllImport("gdi32.dll", ExactSpelling = true,
      SetLastError = true)]
    public static extern Bool BitBlt(
      IntPtr hObject,
      int nXDest, int nYDest,
      int nWidth, int nHeight,
      IntPtr hObjSource, int nXSrc, int nYSrc,
      TernaryRasterOperations dwRop);
  }
}