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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;

namespace DShowNET.Helper
{
  /// <summary>
  ///  The property page to configure a Video for Windows compliant
  ///  compression codec. Most compressors support this property page
  ///  rather than a DirectShow property page. Also, most compressors
  ///  do not support the IAMVideoCompression interface so this
  ///  property page is the only method to configure a compressor. 
  /// </summary>
  public class VfwCompressorPropertyPage : PropertyPage
  {

    // ---------------- Properties --------------------

    /// <summary> Video for Windows compression dialog interface </summary>
    protected IAMVfwCompressDialogs vfwCompressDialogs = null;

    /// <summary> 
    ///  Get or set the state of the property page. This is used to save
    ///  and restore the user's choices without redisplaying the property page.
    ///  This property will be null if unable to retrieve the property page's
    ///  state.
    /// </summary>
    /// <remarks>
    ///  After showing this property page, read and store the value of 
    ///  this property. At a later time, the user's choices can be 
    ///  reloaded by setting this property with the value stored earlier. 
    ///  Note that some property pages, after setting this property, 
    ///  will not reflect the new state. However, the filter will use the
    ///  new settings.
    /// </remarks>
    public override byte[] State
    {
      get
      {
        byte[] data = null;
        int size = 0;

        int hr = vfwCompressDialogs.GetState(IntPtr.Zero, ref size);
        if ((hr == 0) && (size > 0))
        {
          IntPtr ptrMemory = Marshal.AllocCoTaskMem(size);
          try
          {
            data = new byte[size];
            hr = vfwCompressDialogs.GetState(ptrMemory, ref size);
            if (hr != 0) data = null;
            else
            {
              for (int off = 0; off < size; off++)
                data[off] = Marshal.ReadByte(ptrMemory, off);
            }
          }
          finally
          {
            Marshal.FreeCoTaskMem(ptrMemory);
          }
        }
        return (data);
      }
      set
      {
        IntPtr ptrMemory=Marshal.AllocCoTaskMem(value.Length);
        try
        {
          for (int off = 0; off < value.Length; off++)
            Marshal.WriteByte(ptrMemory, off, value[off]);
          int hr = vfwCompressDialogs.SetState(ptrMemory, value.Length);
          if (hr != 0) Marshal.ThrowExceptionForHR(hr);
        }
        finally
        {
          Marshal.FreeCoTaskMem(ptrMemory);
        }
      }
    }


    // ---------------- Constructors --------------------

    /// <summary> Constructor </summary>
    public VfwCompressorPropertyPage(string name, IAMVfwCompressDialogs compressDialogs)
    {
      Name = name;
      SupportsPersisting = true;
      this.vfwCompressDialogs = compressDialogs;
    }



    // ---------------- Public Methods --------------------

    /// <summary> 
    ///  Show the property page. Some property pages cannot be displayed 
    ///  while previewing and/or capturing. 
    /// </summary>
    public override void Show(Control owner)
    {
      vfwCompressDialogs.ShowDialog(VfwCompressDialogs.Config, owner.Handle);
    }

  }
}
