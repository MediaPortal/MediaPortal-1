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

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class BitmapConverter
  {
    private byte[] m_Buffer;
    private readonly object m_Obj;
    private readonly bool m_ReuseBuffer;

    public BitmapConverter()
      : this(false) {}

    public BitmapConverter(bool reuseBuffer)
    {
      this.m_Obj = new object();
      this.m_ReuseBuffer = reuseBuffer;
    }

    public BitmapData ToBitmapData(Bitmap bitmap)
    {
      lock (this.m_Obj)
      {
        BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly,
                                                bitmap.PixelFormat);
        try
        {
          int length = bitmapdata.Stride * bitmap.Height;
          if (((this.m_Buffer == null) || (this.m_Buffer.Length != length)) || !this.m_ReuseBuffer)
          {
            this.m_Buffer = new byte[length];
          }
          Marshal.Copy(bitmapdata.Scan0, this.m_Buffer, 0, length);
        }
        finally
        {
          bitmap.UnlockBits(bitmapdata);
        }
        return bitmapdata;
      }
    }

    public byte[] ToByteArray(Bitmap bitmap)
    {
      lock (this.m_Obj)
      {
        BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly,
                                                bitmap.PixelFormat);
        try
        {
          int length = bitmapdata.Stride * bitmap.Height;
          if (((this.m_Buffer == null) || (this.m_Buffer.Length != length)) || !this.m_ReuseBuffer)
          {
            this.m_Buffer = new byte[length];
          }
          Marshal.Copy(bitmapdata.Scan0, this.m_Buffer, 0, length);
        }
        finally
        {
          bitmap.UnlockBits(bitmapdata);
        }
        return this.m_Buffer;
      }
    }
  }
}