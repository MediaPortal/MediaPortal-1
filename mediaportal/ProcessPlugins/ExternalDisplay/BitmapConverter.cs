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

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// Utility class to convert a <see cref="Bitmap"/> to a byte array in the fastest possible way
  /// </summary>
  public class BitmapConverter
  {
    private readonly object m_Obj = new object();
    private byte[] m_Buffer;
    private readonly bool m_ReuseBuffer;

    /// <summary>
    /// Creates a new instance of the <see cref="BitmapConverter"/>
    /// </summary>
    public BitmapConverter() : this(false)
    {}

    /// <summary>
    /// Creates a new instance of the <see cref="BitmapConverter"/>, optionally enabling further optimizations.
    /// </summary>
    /// <param name="reuseBuffer">A <b>bool</b> indicating whether to enable the extra optimizations.</param>
    /// <remarks>
    /// Enabling the extra optimizations greatly improves the speed but it has a serious drawback: 
    /// A second call to the method overwrites the output of the first!</remarks>
    public BitmapConverter(bool reuseBuffer)
    {
      m_ReuseBuffer = reuseBuffer;
    }
    /// <summary>
    /// Converts the given <see cref="Bitmap"/> to a byte array in the fastest possible way (using managed code).
    /// </summary>
    /// <param name="bitmap">The <see cref="Bitmap"/> to convert.</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    /// We try to re-use the same buffer </remarks>
    public byte[] ToByteArray(Bitmap bitmap)
    {
      lock (m_Obj)
      {
        BitmapData data = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        try
        {
          int size = data.Stride*bitmap.Height;
          if (m_Buffer == null || m_Buffer.Length != size || !m_ReuseBuffer)
          {
            m_Buffer = new byte[size];
          }
          Marshal.Copy(data.Scan0, m_Buffer, 0, size);
        }
        finally
        {
          bitmap.UnlockBits(data);
        }
        return m_Buffer;
      }
    }
  }
}