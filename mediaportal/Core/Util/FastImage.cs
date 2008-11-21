#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace MediaPortal.Util
{
  /// <summary>
  /// Provides a System.Drawing.Image class with disabled image verification
  /// http://weblogs.asp.net/justin_rogers/archive/2004/03/31/105296.aspx
  /// </summary>
  public class ImageFast
  {
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
    public static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

    private ImageFast()
    {
    }

    private static Type imageType = typeof(Bitmap);

    public static Image FastFromFile(string filename)
    {
      filename = Path.GetFullPath(filename);
      IntPtr loadingImage = IntPtr.Zero;

      // We are not using ICM at all, fudge that, this should be FAAAAAST!
      if (GdipLoadImageFromFile(filename, out loadingImage) != 0)
      {
        throw new ArgumentException("GDI+ threw a status error code");
      }

      return (Bitmap)imageType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { loadingImage });
    }
  }
}