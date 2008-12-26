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

namespace MediaPortal.TV.Epg
{
  /// <summary>
  /// ISO 6937 to unicode converter
  /// </summary>
  public class Iso6937ToUnicode
  {
    /// <summary>
    /// ISO 6937 to Unicode convertor for EPG
    /// </summary>
    public static string Convert(IntPtr ptr)
    {
      int i = 0;
      string output = String.Empty;
      byte b = Marshal.ReadByte(ptr, 0);
      if (b < 0x20)
      {
        // ISO 6937 encoding must start with character between 0x20 and 0xFF
        // otherwise it is dfferent encoding table
        // for example 0x05 means encoding table 8859-9
        // here is just fallback to system ANSI
        return Marshal.PtrToStringAnsi(ptr);
      }
      while (b != 0)
      {
        char ch;
        switch (b)
        {
          // at first single byte characters
          case 0xA8:
            ch = (char)0x00A4;
            break;
          case 0xA9:
            ch = (char)0x2018;
            break;
          case 0xAA:
            ch = (char)0x201C;
            break;
          case 0xAC:
            ch = (char)0x2190;
            break;
          case 0xAD:
            ch = (char)0x2191;
            break;
          case 0xAE:
            ch = (char)0x2192;
            break;
          case 0xAF:
            ch = (char)0x2193;
            break;
          case 0xB4:
            ch = (char)0x00D7;
            break;
          case 0xB8:
            ch = (char)0x00F7;
            break;
          case 0xB9:
            ch = (char)0x2019;
            break;
          case 0xBA:
            ch = (char)0x201D;
            break;
          case 0xD0:
            ch = (char)0x2014;
            break;
          case 0xD1:
            ch = (char)0xB9;
            break;
          case 0xD2:
            ch = (char)0xAE;
            break;
          case 0xD3:
            ch = (char)0xA9;
            break;
          case 0xD4:
            ch = (char)0x2122;
            break;
          case 0xD5:
            ch = (char)0x266A;
            break;
          case 0xD6:
            ch = (char)0xAC;
            break;
          case 0xD7:
            ch = (char)0xA6;
            break;
          case 0xDC:
            ch = (char)0x215B;
            break;
          case 0xDD:
            ch = (char)0x215C;
            break;
          case 0xDE:
            ch = (char)0x215D;
            break;
          case 0xDF:
            ch = (char)0x215E;
            break;
          case 0xE0:
            ch = (char)0x2126;
            break;
          case 0xE1:
            ch = (char)0xC6;
            break;
          case 0xE2:
            ch = (char)0xD0;
            break;
          case 0xE3:
            ch = (char)0xAA;
            break;
          case 0xE4:
            ch = (char)0x126;
            break;
          case 0xE6:
            ch = (char)0x132;
            break;
          case 0xE7:
            ch = (char)0x013F;
            break;
          case 0xE8:
            ch = (char)0x141;
            break;
          case 0xE9:
            ch = (char)0xD8;
            break;
          case 0xEA:
            ch = (char)0x152;
            break;
          case 0xEB:
            ch = (char)0xBA;
            break;
          case 0xEC:
            ch = (char)0xDE;
            break;
          case 0xED:
            ch = (char)0x166;
            break;
          case 0xEE:
            ch = (char)0x014A;
            break;
          case 0xEF:
            ch = (char)0x149;
            break;
          case 0xF0:
            ch = (char)0x138;
            break;
          case 0xF1:
            ch = (char)0xE6;
            break;
          case 0xF2:
            ch = (char)0x111;
            break;
          case 0xF3:
            ch = (char)0xF0;
            break;
          case 0xF4:
            ch = (char)0x127;
            break;
          case 0xF5:
            ch = (char)0x131;
            break;
          case 0xF6:
            ch = (char)0x133;
            break;
          case 0xF7:
            ch = (char)0x140;
            break;
          case 0xF8:
            ch = (char)0x142;
            break;
          case 0xF9:
            ch = (char)0xF8;
            break;
          case 0xFA:
            ch = (char)0x153;
            break;
          case 0xFB:
            ch = (char)0xDF;
            break;
          case 0xFC:
            ch = (char)0xFE;
            break;
          case 0xFD:
            ch = (char)0x167;
            break;
          case 0xFE:
            ch = (char)0x014B;
            break;
          case 0xFF:
            ch = (char)0xAD;
            break;
          // multibyte
          case 0xC1:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x41:
                ch = (char)0xC0;
                break;
              case 0x45:
                ch = (char)0xC8;
                break;
              case 0x49:
                ch = (char)0xCC;
                break;
              case 0x4F:
                ch = (char)0xD2;
                break;
              case 0x55:
                ch = (char)0xD9;
                break;
              case 0x61:
                ch = (char)0xE0;
                break;
              case 0x65:
                ch = (char)0xE8;
                break;
              case 0x69:
                ch = (char)0xEC;
                break;
              case 0x6F:
                ch = (char)0xF2;
                break;
              case 0x75:
                ch = (char)0xF9;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xC2:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0xB4;
                break;
              case 0x41:
                ch = (char)0xC1;
                break;
              case 0x43:
                ch = (char)0x106;
                break;
              case 0x45:
                ch = (char)0xC9;
                break;
              case 0x49:
                ch = (char)0xCD;
                break;
              case 0x4C:
                ch = (char)0x139;
                break;
              case 0x4E:
                ch = (char)0x143;
                break;
              case 0x4F:
                ch = (char)0xD3;
                break;
              case 0x52:
                ch = (char)0x154;
                break;
              case 0x53:
                ch = (char)0x015A;
                break;
              case 0x55:
                ch = (char)0xDA;
                break;
              case 0x59:
                ch = (char)0xDD;
                break;
              case 0x5A:
                ch = (char)0x179;
                break;
              case 0x61:
                ch = (char)0xE1;
                break;
              case 0x63:
                ch = (char)0x107;
                break;
              case 0x65:
                ch = (char)0xE9;
                break;
              case 0x69:
                ch = (char)0xED;
                break;
              case 0x6C:
                ch = (char)0x013A;
                break;
              case 0x6E:
                ch = (char)0x144;
                break;
              case 0x6F:
                ch = (char)0xF3;
                break;
              case 0x72:
                ch = (char)0x155;
                break;
              case 0x73:
                ch = (char)0x015B;
                break;
              case 0x75:
                ch = (char)0xFA;
                break;
              case 0x79:
                ch = (char)0xFD;
                break;
              case 0x7A:
                ch = (char)0x017A;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;

          case 0xC3:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x41:
                ch = (char)0xC2;
                break;
              case 0x43:
                ch = (char)0x108;
                break;
              case 0x45:
                ch = (char)0xCA;
                break;
              case 0x47:
                ch = (char)0x011C;
                break;
              case 0x48:
                ch = (char)0x124;
                break;
              case 0x49:
                ch = (char)0xCE;
                break;
              case 0x4A:
                ch = (char)0x134;
                break;
              case 0x4F:
                ch = (char)0xD4;
                break;
              case 0x53:
                ch = (char)0x015C;
                break;
              case 0x55:
                ch = (char)0xDB;
                break;
              case 0x57:
                ch = (char)0x174;
                break;
              case 0x59:
                ch = (char)0x176;
                break;
              case 0x61:
                ch = (char)0xE2;
                break;
              case 0x63:
                ch = (char)0x109;
                break;
              case 0x65:
                ch = (char)0xEA;
                break;
              case 0x67:
                ch = (char)0x011D;
                break;
              case 0x68:
                ch = (char)0x125;
                break;
              case 0x69:
                ch = (char)0xEE;
                break;
              case 0x6A:
                ch = (char)0x135;
                break;
              case 0x6F:
                ch = (char)0xF4;
                break;
              case 0x73:
                ch = (char)0x015D;
                break;
              case 0x75:
                ch = (char)0xFB;
                break;
              case 0x77:
                ch = (char)0x175;
                break;
              case 0x79:
                ch = (char)0x177;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xC4:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x41:
                ch = (char)0xC3;
                break;
              case 0x49:
                ch = (char)0x128;
                break;
              case 0x4E:
                ch = (char)0xD1;
                break;
              case 0x4F:
                ch = (char)0xD5;
                break;
              case 0x55:
                ch = (char)0x168;
                break;
              case 0x61:
                ch = (char)0xE3;
                break;
              case 0x69:
                ch = (char)0x129;
                break;
              case 0x6E:
                ch = (char)0xF1;
                break;
              case 0x6F:
                ch = (char)0xF5;
                break;
              case 0x75:
                ch = (char)0x169;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xC5:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0xAF;
                break;
              case 0x41:
                ch = (char)0x100;
                break;
              case 0x45:
                ch = (char)0x112;
                break;
              case 0x49:
                ch = (char)0x012A;
                break;
              case 0x4F:
                ch = (char)0x014C;
                break;
              case 0x55:
                ch = (char)0x016A;
                break;
              case 0x61:
                ch = (char)0x101;
                break;
              case 0x65:
                ch = (char)0x113;
                break;
              case 0x69:
                ch = (char)0x012B;
                break;
              case 0x6F:
                ch = (char)0x014D;
                break;
              case 0x75:
                ch = (char)0x016B;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xC6:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0x02D8;
                break;
              case 0x41:
                ch = (char)0x102;
                break;
              case 0x47:
                ch = (char)0x011E;
                break;
              case 0x55:
                ch = (char)0x016C;
                break;
              case 0x61:
                ch = (char)0x103;
                break;
              case 0x67:
                ch = (char)0x011F;
                break;
              case 0x75:
                ch = (char)0x016D;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xC7:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0x02D9;
                break;
              case 0x43:
                ch = (char)0x010A;
                break;
              case 0x45:
                ch = (char)0x116;
                break;
              case 0x47:
                ch = (char)0x120;
                break;
              case 0x49:
                ch = (char)0x130;
                break;
              case 0x5A:
                ch = (char)0x017B;
                break;
              case 0x63:
                ch = (char)0x010B;
                break;
              case 0x65:
                ch = (char)0x117;
                break;
              case 0x67:
                ch = (char)0x121;
                break;
              case 0x7A:
                ch = (char)0x017C;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xC8:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0xA8;
                break;
              case 0x41:
                ch = (char)0xC4;
                break;
              case 0x45:
                ch = (char)0xCB;
                break;
              case 0x49:
                ch = (char)0xCF;
                break;
              case 0x4F:
                ch = (char)0xD6;
                break;
              case 0x55:
                ch = (char)0xDC;
                break;
              case 0x59:
                ch = (char)0x178;
                break;
              case 0x61:
                ch = (char)0xE4;
                break;
              case 0x65:
                ch = (char)0xEB;
                break;
              case 0x69:
                ch = (char)0xEF;
                break;
              case 0x6F:
                ch = (char)0xF6;
                break;
              case 0x75:
                ch = (char)0xFC;
                break;
              case 0x79:
                ch = (char)0xFF;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xCA:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0x02DA;
                break;
              case 0x41:
                ch = (char)0xC5;
                break;
              case 0x55:
                ch = (char)0x016E;
                break;
              case 0x61:
                ch = (char)0xE5;
                break;
              case 0x75:
                ch = (char)0x016F;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xCB:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0xB8;
                break;
              case 0x43:
                ch = (char)0xC7;
                break;
              case 0x47:
                ch = (char)0x122;
                break;
              case 0x4B:
                ch = (char)0x136;
                break;
              case 0x4C:
                ch = (char)0x013B;
                break;
              case 0x4E:
                ch = (char)0x145;
                break;
              case 0x52:
                ch = (char)0x156;
                break;
              case 0x53:
                ch = (char)0x015E;
                break;
              case 0x54:
                ch = (char)0x162;
                break;
              case 0x63:
                ch = (char)0xE7;
                break;
              case 0x67:
                ch = (char)0x123;
                break;
              case 0x6B:
                ch = (char)0x137;
                break;
              case 0x6C:
                ch = (char)0x013C;
                break;
              case 0x6E:
                ch = (char)0x146;
                break;
              case 0x72:
                ch = (char)0x157;
                break;
              case 0x73:
                ch = (char)0x015F;
                break;
              case 0x74:
                ch = (char)0x163;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xCD:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0x02DD;
                break;
              case 0x4F:
                ch = (char)0x150;
                break;
              case 0x55:
                ch = (char)0x170;
                break;
              case 0x6F:
                ch = (char)0x151;
                break;
              case 0x75:
                ch = (char)0x171;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xCE:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0x02DB;
                break;
              case 0x41:
                ch = (char)0x104;
                break;
              case 0x45:
                ch = (char)0x118;
                break;
              case 0x49:
                ch = (char)0x012E;
                break;
              case 0x55:
                ch = (char)0x172;
                break;
              case 0x61:
                ch = (char)0x105;
                break;
              case 0x65:
                ch = (char)0x119;
                break;
              case 0x69:
                ch = (char)0x012F;
                break;
              case 0x75:
                ch = (char)0x173;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          case 0xCF:
            i++;
            b = Marshal.ReadByte(ptr, i);
            switch (b)
            {
              case 0x20:
                ch = (char)0x02C7;
                break;
              case 0x43:
                ch = (char)0x010C;
                break;
              case 0x44:
                ch = (char)0x010E;
                break;
              case 0x45:
                ch = (char)0x011A;
                break;
              case 0x4C:
                ch = (char)0x013D;
                break;
              case 0x4E:
                ch = (char)0x147;
                break;
              case 0x52:
                ch = (char)0x158;
                break;
              case 0x53:
                ch = (char)0x160;
                break;
              case 0x54:
                ch = (char)0x164;
                break;
              case 0x5A:
                ch = (char)0x017D;
                break;
              case 0x63:
                ch = (char)0x010D;
                break;
              case 0x64:
                ch = (char)0x010F;
                break;
              case 0x65:
                ch = (char)0x011B;
                break;
              case 0x6C:
                ch = (char)0x013E;
                break;
              case 0x6E:
                ch = (char)0x148;
                break;
              case 0x72:
                ch = (char)0x159;
                break;
              case 0x73:
                ch = (char)0x161;
                break;
              case 0x74:
                ch = (char)0x165;
                break;
              case 0x7A:
                ch = (char)0x017E;
                break;
              // unknown character --> fallback
              default:
                ch = (char)b;
                break;
            }
            break;
          // rest is the same
          default:
            ch = (char)b;
            break;
        }
        if (b != 0)
        {
          i++;
          b = Marshal.ReadByte(ptr, i);
        }
        if (ch != 0)
        {
          output += ch;
        }
      }
      return output;
    }
  }
}
