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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Text;

namespace ThumbDBLib
{
  public class ThumbnailCreatorNet : IDisposable
  {
    private int _desiredWidth, _desiredHeight;
    private bool _strechImage, _bevelImage;

    public void SetParams(int width, int height, bool bStretch, bool bBevel)
    {
      // cache parameters
      _desiredWidth = width;
      _desiredHeight = height;
      _strechImage = bStretch;
      _bevelImage = bBevel;
    }
    public void Dispose()
    {
    }
    public Image GetThumbNail(string fileName)
    {
      Bitmap bitmapNew;
      Bitmap bitmapOrg;
      float fx, fy, f;
      int thumbNailWidth, thumbNailHeight;
      float originalWidth, originalHeight;

      using (bitmapOrg = new Bitmap(fileName))
      {
        if (!_strechImage)
        {
          // retain aspect ratio
          originalWidth = bitmapOrg.Width;
          originalHeight = bitmapOrg.Height;
          fx = originalWidth / _desiredWidth;
          fy = originalHeight / _desiredHeight; // subsample factors
          // must fit in thumbnail size
          f = Math.Max(fx, fy);
          if (f < 1) f = 1;
          thumbNailWidth = (int)(originalWidth / f);
          thumbNailHeight = (int)(originalHeight / f);
        }
        else
        {
          thumbNailWidth = _desiredWidth;
          thumbNailHeight = _desiredHeight;
        }

        bitmapNew = (Bitmap)bitmapOrg.GetThumbnailImage(thumbNailWidth, thumbNailHeight, new Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
      }
      if (!_bevelImage)
      {
        return bitmapNew;
      }

      // ---- apply bevel
      int widTh, heTh;
      widTh = bitmapNew.Width;
      heTh = bitmapNew.Height;
      int BevW = 10, LowA = 0, HighA = 180, Dark = 80, Light = 255;
      // hilight color, low and high
      Color clrHi1 = Color.FromArgb(LowA, Light, Light, Light);
      Color clrHi2 = Color.FromArgb(HighA, Light, Light, Light);
      Color clrDark1 = Color.FromArgb(LowA, Dark, Dark, Dark);
      Color clrDark2 = Color.FromArgb(HighA, Dark, Dark, Dark);
      LinearGradientBrush br; 
      Rectangle rectSide;
      using (Graphics newG = Graphics.FromImage(bitmapNew))
      {
        Size szHorz = new Size(widTh, BevW);
        Size szVert = new Size(BevW, heTh);
        // ---- draw dark (shadow) sides first
        // draw bottom-side of bevel
        szHorz += new Size(0, 2);
        szVert += new Size(2, 0);
        rectSide = new Rectangle(new Point(0, heTh - BevW), szHorz);
        using (br = new LinearGradientBrush(rectSide, clrDark1, clrDark2, LinearGradientMode.Vertical))
        {
          rectSide.Inflate(0, -1);
          newG.FillRectangle(br, rectSide);
        }
        // draw right-side of bevel
        rectSide = new Rectangle(new Point(widTh - BevW, 0), szVert);
        using (br = new LinearGradientBrush(rectSide, clrDark1, clrDark2, LinearGradientMode.Horizontal))
        {
          rectSide.Inflate(-1, 0);
          newG.FillRectangle(br, rectSide);
        }
        // ---- draw bright (hilight) sides next
        szHorz -= new Size(0, 2);
        szVert -= new Size(2, 0);
        // draw top-side of bevel
        rectSide = new Rectangle(new Point(0, 0), szHorz);
        using (br = new LinearGradientBrush(rectSide, clrHi2, clrHi1, LinearGradientMode.Vertical))
        {
          newG.FillRectangle(br, rectSide);
          // draw left-side of bevel
        }
        rectSide = new Rectangle(new Point(0, 0), szVert);
        using (br = new LinearGradientBrush(rectSide, clrHi2, clrHi1, LinearGradientMode.Horizontal))
        {
          newG.FillRectangle(br, rectSide);
        }
      }
      return bitmapNew;
    }

    public bool ThumbnailCallback() { return false; }
  }
}
