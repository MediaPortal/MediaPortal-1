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
using System.Drawing.Imaging;

namespace MediaPortal.Util
{
  public struct PixelData
  {
    public byte blue;
    public byte green;
    public byte red;
    public byte alpha;
  }


  public unsafe class ImageHandler
  {
    public long[] Pixels
    {
      get { return pixels; }

      set { this.pixels = value; }
    }

    public int Width
    {
      get { return width; }
    }

    public int Height
    {
      get { return height; }
    }

    public Bitmap Image
    {
      get { return image; }
    }

    private Bitmap image;
    private long[] pixels;
    private int width;
    private int height;

    public ImageHandler(Bitmap bitmap)
    {
      image = bitmap;
      setSize(bitmap.Width, bitmap.Height);
      Pixels = new long[width * height];
    }

    public ImageHandler(int width, int height)
    {
      Pixels = new long[width * height];
      setSize(width, height);
      image = new Bitmap(width, height);
    }

    private void setSize(int width, int height)
    {
      this.width = width;
      this.height = height;
    }

    private BitmapData bmData;
    private int stride;
    private IntPtr scan;
    private Byte* pBase = null;

    public void lockBitmap()
    {
      // walk through rows
      bmData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite,
                              PixelFormat.Format32bppArgb);
      stride = bmData.Stride;
      scan = bmData.Scan0;
      pBase = (Byte*)bmData.Scan0.ToPointer();
    }

    public void unlockBitmap()
    {
      image.UnlockBits(bmData);
      bmData = null;
      pBase = null;
    }

    public void clear(long col, bool doLockBitmap)
    {
      for (int x = pixels.Length; --x >= 0;)
      {
        pixels[x] = col;
      }

      int size = width * height * 4;

      if (doLockBitmap)
        lockBitmap();
      unsafe
      {
        Byte* p = (Byte*)bmData.Scan0.ToPointer();
        for (int i = 0; i < size; i += 4)
        {
          p[i + 3] = 0xFF;
          p[i] = (byte)((col >> 16) & 0xFF);
          p[i + 1] = (byte)((col >> 8) & 0xFF);
          p[i + 2] = (byte)(col & 0xFF);
        }
      }
      if (doLockBitmap)
        unlockBitmap();
    }

    public bool legit(int x, int y)
    {
      if (x >= 0 && x < width && y >= 0 && y < height)
        return true;
      return false;
    }

    public void setLegitPixel(int x, int y, long color)
    {
      if (legit(x, y))
        setPixel(x, y, color);
    }

    public long getLegitPixel(int x, int y)
    {
      if (legit(x, y))
        return getPixel(x, y);
      return 0;
    }

    public long getLegitPixel2(int x, int y)
    {
      if (legit(x, y))
        return getPixel(x, y);
      return 0xFFFFFFFF;
    }

    public void movePixel(int x, int y, int xx, int yy, long color)
    {
      long c = getPixel(x, y);
      setPixel(x, y, color);
      setPixel(xx, yy, c);
    }

    private unsafe PixelData* PixelAt(int x, int y)
    {
      return (PixelData*)(pBase + y * stride + x * sizeof (PixelData));
    }

    public long getPixel(int x, int y)
    {
      PixelData* p = PixelAt(x, y);
      return (p->red << 16 | p->green << 8 | p->blue);
    }

    public void setPixel(int x, int y, long color)
    {
      PixelData* p = PixelAt(x, y);
      p->alpha = 0xFF; //(byte)((color >> 24) & 0xFF);
      p->red = (byte)((color >> 16) & 0xFF);
      p->green = (byte)((color >> 8) & 0xFF);
      p->blue = (byte)(color & 0xFF);
    }
  }
}