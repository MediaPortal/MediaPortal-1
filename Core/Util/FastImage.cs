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
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Util
{
  /// <summary>
  /// Provides a System.Drawing.Image class with disabled image verification
  /// http://weblogs.asp.net/justin_rogers/archive/2004/03/31/105296.aspx
  /// http://support.microsoft.com/default.aspx?scid=kb%3ben-us%3b831419
  /// </summary>
  public class ImageFast
  {
    #region Internal data types

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupInput
    {
      public int GdiplusVersion;
      public IntPtr DebugEventCallback;
      public bool SuppressBackgroundThread;
      public bool SuppressExternalCodecs;

      public static StartupInput GetDefaultStartupInput()
      {
        StartupInput result = new StartupInput();
        result.GdiplusVersion = 1;
        result.SuppressBackgroundThread = false;
        result.SuppressExternalCodecs = false;
        return result;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupOutput
    {
      public IntPtr Hook;
      public IntPtr Unhook;
    }

    internal enum GdipImageTypeEnum
    {
      Unknown = 0,
      Bitmap = 1,
      Metafile = 2
    }

    #endregion

    #region DLL Imports

    [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

    [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdiplusStartup(out IntPtr token, ref StartupInput input, out StartupOutput output);

    [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdiplusShutdown(IntPtr token);

    [DllImport("gdiplus.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int GdipGetImageType(IntPtr image, out GdipImageTypeEnum type);

    private static IntPtr gdipToken = IntPtr.Zero;
    private static Type bmpType = typeof(System.Drawing.Bitmap);
    private static Type emfType = typeof(System.Drawing.Imaging.Metafile);

    #endregion

    #region Constructors

    private ImageFast()
    { 
    }

    static ImageFast()
    {
      if (gdipToken == IntPtr.Zero)
      {
        StartupInput input = StartupInput.GetDefaultStartupInput();
        StartupOutput output;

        int status = GdiplusStartup(out gdipToken, ref input, out output);
        if (status == 0)
          AppDomain.CurrentDomain.ProcessExit += new EventHandler(Cleanup_Gdiplus);
      }
    }

    #endregion

    private static void Cleanup_Gdiplus(object sender, EventArgs e)
    {
      if (gdipToken != IntPtr.Zero)
      {
        Log.Debug("FastImage: Cleaning up GDI+ before shutting down");
        GdiplusShutdown(gdipToken);
      }
    }    

    public static Image FromFile(string filename)
    {
      filename = Path.GetFullPath(filename);
      IntPtr loadingImage = IntPtr.Zero;

      // We are not using ICM at all, fudge that, this should be FAAAAAST!
      if (GdipLoadImageFromFile(filename, out loadingImage) != 0)
      {
        throw new ArgumentException("GDI+ threw a status error code.");
      }

      GdipImageTypeEnum imageType;
      if (GdipGetImageType(loadingImage, out imageType) != 0)
      {
        throw new ArgumentException("GDI+ couldn't get the image type");
      }

      switch (imageType)
      {
        case GdipImageTypeEnum.Bitmap:
          return (Bitmap)bmpType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { loadingImage });
        case GdipImageTypeEnum.Metafile:
          return (Metafile)emfType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { loadingImage });
      }

      throw new ArgumentException("Couldn't convert underlying GDI+ object to managed object");
    }
  }

}
