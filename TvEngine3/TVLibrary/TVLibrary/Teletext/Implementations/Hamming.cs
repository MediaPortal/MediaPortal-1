/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Teletext
{
  public class Hamming
  {
    #region constants
    const int ERASE_BIT_OFFSET = 5;
    const int ERASE_BIT = 8;
    
    const int SUBTITLE_BIT_OFFSET = 7;
    const int SUBTITLE_BIT = 4;

    const int BOXED_BIT_OFFSET = 5;
    const int BOXED_BIT = 12;


    const int SERIAL_BIT_OFFSET = 9;
    const int SERIAL_BIT = 1;
    #endregion

    #region hamming tables
    static public byte[] Decode = new byte[] {
      0xFF, 0xFF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x07, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x06, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x03, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0x05, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0a, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0b, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0x0c, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0d, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x09, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0x08, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0f, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
      0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0e, 0xFF, 0xFF
		};

    static public byte[] Encode = new byte[] { 0x15, 0x02, 0x49, 0x5E, 0x64, 0x73, 0x38, 0x2F,
						0xD0, 0xC7, 0x8C, 0x9B, 0xA1, 0xB6, 0xFD, 0xEA};
    #endregion

    #region hamming helper methods

    static public int GetPacketNumber(int offset, ref byte[] rowData)
    {
      int magazine = Decode[rowData[offset + 0]];
      int rowAddress = Decode[rowData[offset + 1]];
      if (magazine == 0xff || rowAddress == 0xff) return -1;
      int packetNumber = (magazine >> 3) + (rowAddress << 1); // Line number in Page 
      return packetNumber;
    }

    static public int GetPageNumber(int offset, ref byte[] rowData)
    {
      int magazine = Decode[rowData[offset + 0]];
      if (magazine == 0) magazine = 8;

      int pageUnits = Decode[rowData[offset + 2]];
      int pageTens = Decode[rowData[offset + 3]];
      return (magazine * 0x100 + pageTens * 0x10 + pageUnits);
    }

    static public int GetSubPageNumber(int offset, ref byte[] rowData)
    {
      // decode the subpage number
      int subPageNumber = 0;
      subPageNumber = ((Decode[rowData[offset + 7]] & 3) << 12) +  //3
                        (Decode[rowData[offset + 6]] << 8) +        //f
                       ((Decode[rowData[offset + 5]] & 7) << 4) +   //7
                        (Decode[rowData[offset + 4]] & 0xf);        //f
      return subPageNumber;
    }

    static public bool IsEraseBitSet(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + ERASE_BIT_OFFSET]];
      controlByte &= ERASE_BIT;
      return (controlByte != 0);
    }
    static public bool IsSubtitleBitSet(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + SUBTITLE_BIT_OFFSET]];
      controlByte &= SUBTITLE_BIT;
      return (controlByte != 0);
    }

    static public bool IsBoxed(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + BOXED_BIT_OFFSET]];
      controlByte &= BOXED_BIT;
      return (controlByte != 0);
    }

    static public bool IsSerial(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + SERIAL_BIT_OFFSET]];
      controlByte &= SERIAL_BIT;
      return (controlByte != 0);
    }


    static public void SetSubPageNumber(int offset, ref byte[] rowData, int hexSubPageNr)
    {
	    // max sub = 0x3f7f
      rowData[offset + 4] = Encode[hexSubPageNr & 0xf];
      hexSubPageNr >>= 4;

      rowData[offset + 5] = Encode[hexSubPageNr & 0x7];
      hexSubPageNr >>= 4;

      rowData[offset + 6] = Encode[hexSubPageNr & 0xf];
      hexSubPageNr >>= 4;

      rowData[offset + 7] = Encode[hexSubPageNr & 0x3];
    }
    static public void SetHeader(int offset, ref byte[] byData, int pagenr, int subnr)
    {
      int magazine  = ( pagenr / 256 ) & 0x7;
	    int pageTens  = ((pagenr - (magazine*256) ) / 16 ) & 0xf;
	    int pageUnits = ( pagenr - (magazine*256) - (pageTens*16) ) & 0xf;
      
      if (magazine == 8) magazine = 0;

	    byData[offset+0] = Encode[magazine];
      byData[offset + 1] = Encode[0];
      byData[offset + 2] = Encode[pageUnits];
      byData[offset + 3] = Encode[pageTens];

      SetSubPageNumber(0, ref byData, subnr);
      byData[offset + 8] = Encode[0];
      byData[offset + 9] = Encode[0];

      if (magazine == 0) magazine = 8; // back to original magazine
      
	    byData[10] = (byte)('0' + magazine);
      if( pageTens > 9 )
      {
        // hex A t/m F
        byData[11] = (byte)('A' + (pageTens - 10));
      }
      else
      {
        byData[11] = (byte)('0' + pageTens);
      }
      if( pageUnits > 9 )
      {
        byData[12] = (byte)('A' + (pageUnits - 10));
      }
      else
      {
        byData[12] = (byte)('0' + pageUnits);
      }
      for (int x = 13; x < 42; x++) byData[x] = 0x20;
    }
    static public void SetPacketNumber(int offset, ref byte[] byData, int pageNumber, int packetNumber)
    {
      int iMagazine = pageNumber / 0x100;
	    if (iMagazine == 8) iMagazine = 0;

      if ((packetNumber % 2) == 0) byData[offset+0] = Encode[iMagazine];
      else byData[offset+0] = Encode[iMagazine + 8];
      byData[offset + 1] = Encode[packetNumber / 2];
    }

    #endregion
  }
}
