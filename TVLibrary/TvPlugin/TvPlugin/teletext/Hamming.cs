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
using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Teletext
{
  public class Hamming
  {
    #region constants
    // Byte offset of the magazin number. Only the first 3 bits are used
    const int MAGZIN_BYTE_OFFSET = 0;

    // Byte offset of the packetnumber. Last bit of byte1 is only used.
    const int PACKETNUMBER_BYTE1_OFFSET = 0;
    const int PACKETNUMBER_BYTE2_OFFSET = 1;

    // Pagenumber. Magazin Number is also needed for the complete pagenumber
    const int PAGENUMBER_BYTE1_OFFSET = 2;
    const int PAGENUMBER_BYTE2_OFFSET = 3;

    // Subpagenumber. Byte1 only 3 bits and Byte3 only 2 Bits are used
    const int SUBPAGENUMBER_BYTE1_OFFSET = 4;
    const int SUBPAGENUMBER_BYTE2_OFFSET = 5;
    const int SUBPAGENUMBER_BYTE3_OFFSET = 6;
    const int SUBPAGENUMBER_BYTE4_OFFSET = 7;

    // C4 Bit in header
    const int ERASE_BYTE_OFFSET = 5;
    const int ERASE_BIT = 8;

    // C6 Bit in header
    const int SUBTITLE_BYTE_OFFSET = 7;
    const int SUBTITLE_BIT = 8;

    // C5 Bit in header
    const int NEWSFLASH_BYTE_OFFSET = 7;
    const int NEWSFLASH_BIT = 4;

    // C8 Bit in header
    const int UPDATE_BYTE_OFFSET = 8;
    const int UPDATE_BIT = 2;

    // C11 Bit in header
    const int SERIAL_BYTE_OFFSET = 9;
    const int SERIAL_BIT = 1;
    #endregion

    #region hamming tables
    /// <summary>
    /// Hamming 8/4 Decoding table with error correction based on http://pdc.ro.nu/hamming.html
    /// </summary>
    static public byte[] Decode = new byte[] {
      0x01, 0xFF, 0x01, 0x01, 0xFF, 0x00, 0x01, 0xFF, 0xFF, 0x02, 0x01, 0xFF, 0x0a, 0xFF, 0xFF, 0x07, 
      0xFF, 0x00, 0x01, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x06, 0xFF, 0xFF, 0x0b, 0xFF, 0x00, 0x03, 0xFF, 
      0xFF, 0x0c, 0x01, 0xFF, 0x04, 0xFF, 0xFF, 0x07, 0x06, 0xFF, 0xFF, 0x07, 0xFF, 0x07, 0x07, 0x07, 
      0x06, 0xFF, 0xFF, 0x05, 0xFF, 0x00, 0x0d, 0xFF, 0x06, 0x06, 0x06, 0xFF, 0x06, 0xFF, 0xFF, 0x07, 
      0xFF, 0x02, 0x01, 0xFF, 0x04, 0xFF, 0xFF, 0x09, 0x02, 0x02, 0xFF, 0x02, 0xFF, 0x02, 0x03, 0xFF, 
      0x08, 0xFF, 0xFF, 0x05, 0xFF, 0x00, 0x03, 0xFF, 0xFF, 0x02, 0x03, 0xFF, 0x03, 0xFF, 0x03, 0x03, 
      0x04, 0xFF, 0xFF, 0x05, 0x04, 0x04, 0x04, 0xFF, 0xFF, 0x02, 0x0f, 0xFF, 0x04, 0xFF, 0xFF, 0x07, 
      0xFF, 0x05, 0x05, 0x05, 0x04, 0xFF, 0xFF, 0x05, 0x06, 0xFF, 0xFF, 0x05, 0xFF, 0x0e, 0x03, 0xFF, 
      0xFF, 0x0c, 0x01, 0xFF, 0x0a, 0xFF, 0xFF, 0x09, 0x0a, 0xFF, 0xFF, 0x0b, 0x0a, 0x0a, 0x0a, 0xFF, 
      0x08, 0xFF, 0xFF, 0x0b, 0xFF, 0x00, 0x0d, 0xFF, 0xFF, 0x0b, 0x0b, 0x0b, 0x0a, 0xFF, 0xFF, 0x0b, 
      0x0c, 0x0c, 0xFF, 0x0c, 0xFF, 0x0c, 0x0d, 0xFF, 0xFF, 0x0c, 0x0f, 0xFF, 0x0a, 0xFF, 0xFF, 0x07, 
      0xFF, 0x0c, 0x0d, 0xFF, 0x0d, 0xFF, 0x0d, 0x0d, 0x06, 0xFF, 0xFF, 0x0b, 0xFF, 0x0e, 0x0d, 0xFF, 
      0x08, 0xFF, 0xFF, 0x09, 0xFF, 0x09, 0x09, 0x09, 0xFF, 0x02, 0x0f, 0xFF, 0x0a, 0xFF, 0xFF, 0x09, 
      0x08, 0x08, 0x08, 0xFF, 0x08, 0xFF, 0xFF, 0x09, 0x08, 0xFF, 0xFF, 0x0b, 0xFF, 0x0e, 0x03, 0xFF, 
      0xFF, 0x0c, 0x0f, 0xFF, 0x04, 0xFF, 0xFF, 0x09, 0x0f, 0xFF, 0x0f, 0x0f, 0xFF, 0x0e, 0x0f, 0xFF, 
      0x08, 0xFF, 0xFF, 0x05, 0xFF, 0x0e, 0x0d, 0xFF, 0xFF, 0x0e, 0x0f, 0xFF, 0x0e, 0x0e, 0xFF, 0x0e
		};

    /// <summary>
    /// Hamming 8/4 Encoding table with error correction based on http://pdc.ro.nu/hamming.html
    /// </summary>
    static public byte[] Encode = new byte[] { 0x15, 0x02, 0x49, 0x5E, 0x64, 0x73, 0x38, 0x2F,
						0xD0, 0xC7, 0x8C, 0x9B, 0xA1, 0xB6, 0xFD, 0xEA};
    #endregion

    #region hamming helper methods

    /// <summary>
    /// Extracts the packetnumber from the teletext header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>PacketNumber of the teletext page row</returns>
    static public int GetPacketNumber(int offset, ref byte[] rowData)
    {
      int magazine = Decode[rowData[offset + PACKETNUMBER_BYTE1_OFFSET]];
      int rowAddress = Decode[rowData[offset + PACKETNUMBER_BYTE2_OFFSET]];
      if (magazine == 0xff || rowAddress == 0xff)
        return -1;
      int packetNumber = (magazine >> 3) + (rowAddress << 1); // Line number in Page 
      return packetNumber;
    }

    /// <summary>
    /// Extracts the pagenumber from the teletext header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>PageNumber of the teletext page</returns>
    static public int GetPageNumber(int offset, ref byte[] rowData)
    {
      // Page number = Magazin PageTens PageUnits
      int magazine = Decode[rowData[offset + MAGZIN_BYTE_OFFSET]];
      if (magazine == 0)
        magazine = 8;

      int pageUnits = Decode[rowData[offset + PAGENUMBER_BYTE1_OFFSET]];
      int pageTens = Decode[rowData[offset + PAGENUMBER_BYTE2_OFFSET]];
      return (magazine * 0x100 + pageTens * 0x10 + pageUnits);
    }

    /// <summary>
    /// Extracts the subpagenumber from the teletext header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>SubPageNumber of the teletext page</returns>
    static public int GetSubPageNumber(int offset, ref byte[] rowData)
    {
      // decode the subpage number
      // SubPageNumber = 4 Bits SubPageNumber_Byte1 - 3 Bits SubPageNumber_Byte2 - 4 Bits SubPageNumber_Byte3 - 2 Bits SubPageNumber_Byte4
      int subPageNumber = 0;
      subPageNumber = ((Decode[rowData[offset + SUBPAGENUMBER_BYTE4_OFFSET]] & 3) << 12) +  //3
                        (Decode[rowData[offset + SUBPAGENUMBER_BYTE3_OFFSET]] << 8) +        //f
                       ((Decode[rowData[offset + SUBPAGENUMBER_BYTE2_OFFSET]] & 7) << 4) +   //7
                        (Decode[rowData[offset + SUBPAGENUMBER_BYTE1_OFFSET]] & 0xf);        //f
      return subPageNumber;
    }

    /// <summary>
    /// Check if the erase bit (C4) is set in the teletext page header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>true, if erase bit is set</returns>
    static public bool IsEraseBitSet(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + ERASE_BYTE_OFFSET]];
      controlByte &= ERASE_BIT;
      return (controlByte != 0);
    }

    /// <summary>
    /// Check if the erase bit (C8) is set in the teletext page header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>true, if erase bit is set</returns>
    static public bool IsUpdateBitSet(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + UPDATE_BYTE_OFFSET]];
      controlByte &= UPDATE_BIT;
      return (controlByte != 0);
    }

    /// <summary>
    /// Check if the newsflash bit (C7) is set in the teletext page header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>true, if newsflash bit is set</returns>
    static public bool IsNewsflash(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + NEWSFLASH_BYTE_OFFSET]];
      controlByte &= NEWSFLASH_BIT;
      return (controlByte != 0);
    }

    /// <summary>
    /// Check if the subtitle bit (C6) is set in the teletext page header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>true, if subtitle bit is set</returns>
    static public bool IsSubtitleBitSet(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + SUBTITLE_BYTE_OFFSET]];
      controlByte &= SUBTITLE_BIT;
      return (controlByte != 0);
    }

    /// <summary>
    /// Check if the magazin serial bit (C11) is set in the teletext page header
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <returns>true, if magazin serial is set</returns>
    static public bool IsSerial(int offset, ref byte[] rowData)
    {
      int controlByte = Decode[rowData[offset + SERIAL_BYTE_OFFSET]];
      controlByte &= SERIAL_BIT;
      return (controlByte != 0);
    }

    /// <summary>
    /// Sets a page number in teletext header into the given data stream
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <param name="hexSubPageNr">PageNumber (Hex representation)</param>
    static public void SetSubPageNumber(int offset, ref byte[] rowData, int hexSubPageNr)
    {
      // max sub = 0x3f7f
      rowData[offset + SUBPAGENUMBER_BYTE1_OFFSET] = Encode[hexSubPageNr & 0xf];
      hexSubPageNr >>= 4;

      rowData[offset + SUBPAGENUMBER_BYTE2_OFFSET] = Encode[hexSubPageNr & 0x7];
      hexSubPageNr >>= 4;

      rowData[offset + SUBPAGENUMBER_BYTE3_OFFSET] = Encode[hexSubPageNr & 0xf];
      hexSubPageNr >>= 4;

      rowData[offset + SUBPAGENUMBER_BYTE4_OFFSET] = Encode[hexSubPageNr & 0x3];
    }
    /// <summary>
    /// Sets a page header into the given data stream
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <param name="pagenr">PageNumber</param>
    /// <param name="subnr">SubPageNumber</param>
    static public void SetHeader(int offset, ref byte[] byData, int pagenr, int subnr)
    {
      int magazine = (pagenr / 256) & 0x7;
      int pageTens = ((pagenr - (magazine * 256)) / 16) & 0xf;
      int pageUnits = (pagenr - (magazine * 256) - (pageTens * 16)) & 0xf;

      if (magazine == 8)
        magazine = 0;

      byData[offset + MAGZIN_BYTE_OFFSET] = Encode[magazine];
      byData[offset + 1] = Encode[0];
      byData[offset + PAGENUMBER_BYTE1_OFFSET] = Encode[pageUnits];
      byData[offset + PAGENUMBER_BYTE2_OFFSET] = Encode[pageTens];

      SetSubPageNumber(0, ref byData, subnr);
      byData[offset + 8] = Encode[0];
      byData[offset + 9] = Encode[0];

      if (magazine == 0)
        magazine = 8; // back to original magazine

      byData[10] = (byte)('0' + magazine);
      if (pageTens > 9)
      {
        // hex A t/m F
        byData[11] = (byte)('A' + (pageTens - 10));
      }
      else
      {
        byData[11] = (byte)('0' + pageTens);
      }
      if (pageUnits > 9)
      {
        byData[12] = (byte)('A' + (pageUnits - 10));
      }
      else
      {
        byData[12] = (byte)('0' + pageUnits);
      }
      for (int x = 13; x < 42; x++)
        byData[x] = 0x20;
    }
    /// <summary>
    /// Sets a packet number in teletext header into the given data stream
    /// </summary>
    /// <param name="offset">Offset in the data stream</param>
    /// <param name="rowData">Teletext data</param>
    /// <param name="pagenr">PageNumber (needed for magazin number)</param>
    /// <param name="packetNumber">PacketNumber</param>
    static public void SetPacketNumber(int offset, ref byte[] byData, int pageNumber, int packetNumber)
    {
      int iMagazine = pageNumber / 0x100;
      if (iMagazine == 8)
        iMagazine = 0;

      if ((packetNumber % 2) == 0)
        byData[offset + 0] = Encode[iMagazine];
      else
        byData[offset + 0] = Encode[iMagazine + 8];
      byData[offset + 1] = Encode[packetNumber / 2];
    }

    #endregion
  }
}
