#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;

using System.Runtime.InteropServices;
using System.Windows.Forms;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.Util
{
  [ComVisible(true)]
  [ClassInterface(ClassInterfaceType.None)]
  public class DvrMsImageGrabber
  {
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    unsafe private static extern void GrabBitmaps(string fileName);

    //public static bool GrabFrame(string fileName, string imageFileName, System.Drawing.Imaging.ImageFormat format, int width, int height)
    public static bool GrabFrame(string fileName, string imageFileName)
    {
      try
      {
        if (!System.IO.File.Exists(fileName))
        {
        
          Log.Warn("DvrMsImageGrabber: failed to create thumbnail for {0} because the file was not found! (bogus DB entry?)", fileName);
          return false;
        }

        System.IO.FileInfo file = new System.IO.FileInfo(fileName);
       if(file.Length < 1000000)
       {
         Log.Info("DVRMSImageGrabber, file is too small to grab - {0} bytes. Skipping", file.Length);
         return false;
       }

        Utils.FileDelete("temp.bmp");
        Log.Info("DvrMsImageGrabber: create thumbnails for recorded tv - {0}", fileName);

        // potential danger from relative path here - does also run infinitely on some files...
        GrabBitmaps(fileName);

        if (!System.IO.File.Exists("temp.bmp"))
        {
          Log.Info("DvrMsImageGrabber: failed to create thumbnail for {0}", fileName);
          return false;
        }

        Util.Picture.CreateThumbnail("temp.bmp", imageFileName, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, true);
        string imageFileNameL = Util.Utils.ConvertToLargeCoverArt(imageFileName);
        if (!Util.Picture.CreateThumbnail("temp.bmp", imageFileNameL, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false))
          return false;
        
        return true;

        //using (Image bmp = Image.FromFile("temp.bmp"))
        //{
        //  int arx = bmp.Width;
        //  int ary = bmp.Height;

        //  //keep aspect ratio:-)
        //  float ar = ((float)ary) / ((float)arx);
        //  height = (int)(((float)width) * ar);

        //  Log.Info("dvrms:scale thumbnail {0}x{1}->{2}x{3}", arx, ary, width, height);
        //  using (Bitmap result = new Bitmap(width, height))
        //  {
        //    using (Graphics g = Graphics.FromImage(result))
        //    {
        //      g.DrawImage(bmp, new Rectangle(0, 0, width, height));
        //    }
        //    result.Save(imageFileName, format);
        //    return true;
        //  }
        //}
      }
      catch (Exception ex)
      {
        Log.Error("DvrMsImageGrabber: failed to create thumbnail for {0} {1} {2} {3}", fileName, ex.Message, ex.Source, ex.StackTrace);
        return false;
      }
    }
  }
}