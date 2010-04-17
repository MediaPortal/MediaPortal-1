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
using VistaToolbelt.Interop.Native;

namespace VistaToolbelt.Shell
{
  /// <summary>
  /// Assists in generating thumbnails for shell items.
  /// </summary>
  public static class ThumbnailGenerator
  {
    /// <summary>a
    /// Generates an Explorer-style thumbnail for any file or shell item. Requires Vista or above.
    /// </summary>
    /// <param name="filename">The filename of the item.</param>
    /// <returns>The thumbnail of the item.</returns>
    public static System.Drawing.Image GenerateThumbnail(String filename)
    {
      IShellItem ppsi;
      IntPtr hbitmap = IntPtr.Zero;
      Guid uuid = new Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe");

      // Create an IShellItem from the filename.
      UnsafeNativeMethods.SHCreateItemFromParsingName(filename, IntPtr.Zero, uuid, out ppsi);

      // Convert from GDI HBITMAP to WPF BitmapSource.
      // Get the thumbnail image.
      ((IShellItemImageFactory)ppsi).GetImage(new SIZE(256, 256),
                                              SIIGBF.SIIGBF_BIGGERSIZEOK | SIIGBF.SIIGBF_THUMBNAILONLY, out hbitmap);
      //SIIGBF.SIIGBF_THUMBNAILONLY | SIIGBF.SIIGBF_RESIZETOFIT, out hbitmap);

      System.Drawing.Image source = System.Drawing.Image.FromHbitmap(hbitmap);

      // Release COM stuff to avoid memory leaks.
      Marshal.ReleaseComObject(ppsi);
      Marshal.Release(hbitmap);

      return source;
    }
  }
}