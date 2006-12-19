#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

using MediaPortal.GUI.Library;

namespace Tag.OGG
{
  public class OggCRC32
  {
    #region Public Methods
    public static int GetCRCChecksum(byte[] data)
    {
      int chkSum = 0;

      for (int i = 0; i < data.Length; i++)
      {
        chkSum = (chkSum << 8) ^ CRCLookupArray[((chkSum >> 24) & 0xff) ^ (data[i] & 0xff)];
      }

      return chkSum;
    }
    #endregion

    #region Private Methods
    private static int[] CRCLookupArray = new int[256];

    public OggCRC32()
    {
      for (int i = 0; i < CRCLookupArray.Length; i++)
      {
        CRCLookupArray[i] = GetCRCEntry(i);
      }
    }

    private static int GetCRCEntry(int index)
    {
      int r = index << 24;

      for (int i = 0; i < 8; i++)
      {
        if ((r & 0x80000000) != 0)
        {
          r = (r << 1) ^ 0x04c11db7;
        }

        else
        {
          r <<= 1;
        }
      }

      return (r & 0xffff);
    }
    #endregion
  }
}
