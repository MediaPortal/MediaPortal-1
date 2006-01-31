/* 
 *	Copyright (C) 2005 Team MediaPortal
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

    static public bool GrabFrame(string fileName,string imageFileName, System.Drawing.Imaging.ImageFormat format, int width, int height)
    {
      try
      {
        Utils.FileDelete("temp.bmp");
        GrabBitmaps(fileName);

        if (!System.IO.File.Exists("temp.bmp")) return false;
        using (Image bmp = Image.FromFile("temp.bmp"))
        {
          int arx = bmp.Width;
          int ary = bmp.Height;
          //keep aspect ratio:-)
          float ar = ((float)ary) / ((float)arx);
          height = (int)(((float)width) * ar);
          using (Bitmap result = new Bitmap(width, height))
          {
            using (Graphics g = Graphics.FromImage(result))
            {
              g.DrawImage(bmp, new Rectangle(0, 0, width, height));
            }
            result.Save(imageFileName, format);
            return true;
          }
        }
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}
