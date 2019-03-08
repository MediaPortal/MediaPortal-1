#region Copyright (C) 2014-2018 Team MediaPortal

// Copyright (C) 2014-2018 Team MediaPortal
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

namespace TvLibrary.Interfaces
{
  class ISOTextDecoder
  {
    /// <summary>
    /// implements missing in .NET decoder for codepage 20600 = ISO/IEC 8859-10 (Latin/Nordic).
    /// Taken from ftp://ftp.unicode.org/Public/MAPPINGS/ISO8859/8859-10.TXT
    /// </summary>
    public static string from_ISO_8859_10(byte[] bytes)
    {
      string result = String.Empty;
      foreach(byte b in bytes)
      {
        if (b == 0x00)
          break;
        else if (0x01 <= b && b <= 0x7E)
          result += (char) b;
        else if (0x7F <= b && b <= 0x9F)
          continue;
        else if (0xA0 <= b) 
        {
          switch(b)
          {
            case 0xA0: result += (char)0x00A0; break; //NO-BREAK SPACE
            case 0xA1: result += (char)0x0104; break; //LATIN CAPITAL LETTER A WITH OGONEK
            case 0xA2: result += (char)0x0112; break; //LATIN CAPITAL LETTER E WITH MACRON
            case 0xA3: result += (char)0x0122; break; //LATIN CAPITAL LETTER G WITH CEDILLA
            case 0xA4: result += (char)0x012A; break; //LATIN CAPITAL LETTER I WITH MACRON
            case 0xA5: result += (char)0x0128; break; //LATIN CAPITAL LETTER I WITH TILDE
            case 0xA6: result += (char)0x0136; break; //LATIN CAPITAL LETTER K WITH CEDILLA
            case 0xA7: result += (char)0x00A7; break; //SECTION SIGN
            case 0xA8: result += (char)0x013B; break; //LATIN CAPITAL LETTER L WITH CEDILLA
            case 0xA9: result += (char)0x0110; break; //LATIN CAPITAL LETTER D WITH STROKE
            case 0xAA: result += (char)0x0160; break; //LATIN CAPITAL LETTER S WITH CARON
            case 0xAB: result += (char)0x0166; break; //LATIN CAPITAL LETTER T WITH STROKE
            case 0xAC: result += (char)0x017D; break; //LATIN CAPITAL LETTER Z WITH CARON
            case 0xAD: result += (char)0x00AD; break; //SOFT HYPHEN
            case 0xAE: result += (char)0x016A; break; //LATIN CAPITAL LETTER U WITH MACRON
            case 0xAF: result += (char)0x014A; break; //LATIN CAPITAL LETTER ENG
            case 0xB0: result += (char)0x00B0; break; //DEGREE SIGN
            case 0xB1: result += (char)0x0105; break; //LATIN SMALL LETTER A WITH OGONEK
            case 0xB2: result += (char)0x0113; break; //LATIN SMALL LETTER E WITH MACRON
            case 0xB3: result += (char)0x0123; break; //LATIN SMALL LETTER G WITH CEDILLA
            case 0xB4: result += (char)0x012B; break; //LATIN SMALL LETTER I WITH MACRON
            case 0xB5: result += (char)0x0129; break; //LATIN SMALL LETTER I WITH TILDE
            case 0xB6: result += (char)0x0137; break; //LATIN SMALL LETTER K WITH CEDILLA
            case 0xB7: result += (char)0x00B7; break; //MIDDLE DOT
            case 0xB8: result += (char)0x013C; break; //LATIN SMALL LETTER L WITH CEDILLA
            case 0xB9: result += (char)0x0111; break; //LATIN SMALL LETTER D WITH STROKE
            case 0xBA: result += (char)0x0161; break; //LATIN SMALL LETTER S WITH CARON
            case 0xBB: result += (char)0x0167; break; //LATIN SMALL LETTER T WITH STROKE
            case 0xBC: result += (char)0x017E; break; //LATIN SMALL LETTER Z WITH CARON
            case 0xBD: result += (char)0x2015; break; //HORIZONTAL BAR
            case 0xBE: result += (char)0x016B; break; //LATIN SMALL LETTER U WITH MACRON
            case 0xBF: result += (char)0x014B; break; //LATIN SMALL LETTER ENG
            case 0xC0: result += (char)0x0100; break; //LATIN CAPITAL LETTER A WITH MACRON
            case 0xC1: result += (char)0x00C1; break; //LATIN CAPITAL LETTER A WITH ACUTE
            case 0xC2: result += (char)0x00C2; break; //LATIN CAPITAL LETTER A WITH CIRCUMFLEX
            case 0xC3: result += (char)0x00C3; break; //LATIN CAPITAL LETTER A WITH TILDE
            case 0xC4: result += (char)0x00C4; break; //LATIN CAPITAL LETTER A WITH DIAERESIS
            case 0xC5: result += (char)0x00C5; break; //LATIN CAPITAL LETTER A WITH RING ABOVE
            case 0xC6: result += (char)0x00C6; break; //LATIN CAPITAL LETTER AE
            case 0xC7: result += (char)0x012E; break; //LATIN CAPITAL LETTER I WITH OGONEK
            case 0xC8: result += (char)0x010C; break; //LATIN CAPITAL LETTER C WITH CARON
            case 0xC9: result += (char)0x00C9; break; //LATIN CAPITAL LETTER E WITH ACUTE
            case 0xCA: result += (char)0x0118; break; //LATIN CAPITAL LETTER E WITH OGONEK
            case 0xCB: result += (char)0x00CB; break; //LATIN CAPITAL LETTER E WITH DIAERESIS
            case 0xCC: result += (char)0x0116; break; //LATIN CAPITAL LETTER E WITH DOT ABOVE
            case 0xCD: result += (char)0x00CD; break; //LATIN CAPITAL LETTER I WITH ACUTE
            case 0xCE: result += (char)0x00CE; break; //LATIN CAPITAL LETTER I WITH CIRCUMFLEX
            case 0xCF: result += (char)0x00CF; break; //LATIN CAPITAL LETTER I WITH DIAERESIS
            case 0xD0: result += (char)0x00D0; break; //LATIN CAPITAL LETTER ETH (Icelandic)
            case 0xD1: result += (char)0x0145; break; //LATIN CAPITAL LETTER N WITH CEDILLA
            case 0xD2: result += (char)0x014C; break; //LATIN CAPITAL LETTER O WITH MACRON
            case 0xD3: result += (char)0x00D3; break; //LATIN CAPITAL LETTER O WITH ACUTE
            case 0xD4: result += (char)0x00D4; break; //LATIN CAPITAL LETTER O WITH CIRCUMFLEX
            case 0xD5: result += (char)0x00D5; break; //LATIN CAPITAL LETTER O WITH TILDE
            case 0xD6: result += (char)0x00D6; break; //LATIN CAPITAL LETTER O WITH DIAERESIS
            case 0xD7: result += (char)0x0168; break; //LATIN CAPITAL LETTER U WITH TILDE
            case 0xD8: result += (char)0x00D8; break; //LATIN CAPITAL LETTER O WITH STROKE
            case 0xD9: result += (char)0x0172; break; //LATIN CAPITAL LETTER U WITH OGONEK
            case 0xDA: result += (char)0x00DA; break; //LATIN CAPITAL LETTER U WITH ACUTE
            case 0xDB: result += (char)0x00DB; break; //LATIN CAPITAL LETTER U WITH CIRCUMFLEX
            case 0xDC: result += (char)0x00DC; break; //LATIN CAPITAL LETTER U WITH DIAERESIS
            case 0xDD: result += (char)0x00DD; break; //LATIN CAPITAL LETTER Y WITH ACUTE
            case 0xDE: result += (char)0x00DE; break; //LATIN CAPITAL LETTER THORN (Icelandic)
            case 0xDF: result += (char)0x00DF; break; //LATIN SMALL LETTER SHARP S (German)
            case 0xE0: result += (char)0x0101; break; //LATIN SMALL LETTER A WITH MACRON
            case 0xE1: result += (char)0x00E1; break; //LATIN SMALL LETTER A WITH ACUTE
            case 0xE2: result += (char)0x00E2; break; //LATIN SMALL LETTER A WITH CIRCUMFLEX
            case 0xE3: result += (char)0x00E3; break; //LATIN SMALL LETTER A WITH TILDE
            case 0xE4: result += (char)0x00E4; break; //LATIN SMALL LETTER A WITH DIAERESIS
            case 0xE5: result += (char)0x00E5; break; //LATIN SMALL LETTER A WITH RING ABOVE
            case 0xE6: result += (char)0x00E6; break; //LATIN SMALL LETTER AE
            case 0xE7: result += (char)0x012F; break; //LATIN SMALL LETTER I WITH OGONEK
            case 0xE8: result += (char)0x010D; break; //LATIN SMALL LETTER C WITH CARON
            case 0xE9: result += (char)0x00E9; break; //LATIN SMALL LETTER E WITH ACUTE
            case 0xEA: result += (char)0x0119; break; //LATIN SMALL LETTER E WITH OGONEK
            case 0xEB: result += (char)0x00EB; break; //LATIN SMALL LETTER E WITH DIAERESIS
            case 0xEC: result += (char)0x0117; break; //LATIN SMALL LETTER E WITH DOT ABOVE
            case 0xED: result += (char)0x00ED; break; //LATIN SMALL LETTER I WITH ACUTE
            case 0xEE: result += (char)0x00EE; break; //LATIN SMALL LETTER I WITH CIRCUMFLEX
            case 0xEF: result += (char)0x00EF; break; //LATIN SMALL LETTER I WITH DIAERESIS
            case 0xF0: result += (char)0x00F0; break; //LATIN SMALL LETTER ETH (Icelandic)
            case 0xF1: result += (char)0x0146; break; //LATIN SMALL LETTER N WITH CEDILLA
            case 0xF2: result += (char)0x014D; break; //LATIN SMALL LETTER O WITH MACRON
            case 0xF3: result += (char)0x00F3; break; //LATIN SMALL LETTER O WITH ACUTE
            case 0xF4: result += (char)0x00F4; break; //LATIN SMALL LETTER O WITH CIRCUMFLEX
            case 0xF5: result += (char)0x00F5; break; //LATIN SMALL LETTER O WITH TILDE
            case 0xF6: result += (char)0x00F6; break; //LATIN SMALL LETTER O WITH DIAERESIS
            case 0xF7: result += (char)0x0169; break; //LATIN SMALL LETTER U WITH TILDE
            case 0xF8: result += (char)0x00F8; break; //LATIN SMALL LETTER O WITH STROKE
            case 0xF9: result += (char)0x0173; break; //LATIN SMALL LETTER U WITH OGONEK
            case 0xFA: result += (char)0x00FA; break; //LATIN SMALL LETTER U WITH ACUTE
            case 0xFB: result += (char)0x00FB; break; //LATIN SMALL LETTER U WITH CIRCUMFLEX
            case 0xFC: result += (char)0x00FC; break; //LATIN SMALL LETTER U WITH DIAERESIS
            case 0xFD: result += (char)0x00FD; break; //LATIN SMALL LETTER Y WITH ACUTE
            case 0xFE: result += (char)0x00FE; break; //LATIN SMALL LETTER THORN (Icelandic)
            case 0xFF: result += (char)0x0138; break; //LATIN SMALL LETTER KRA
          }
        }
      }
      return result;
    }

    /// <summary>
    /// implements missing in .NET decoder for codepage 20604 = ISO/IEC 8859-14 ( Latin/Celtic).
    /// Taken from ftp://ftp.unicode.org/Public/MAPPINGS/ISO8859/8859-14.TXT
    /// </summary>
    public static string from_ISO_8859_14(byte[] bytes)
    {
      string result = String.Empty;
      foreach (byte b in bytes) {
        if (b == 0x00)
          break;
        else if (0x01 <= b && b <= 0x7E)
          result += (char)b;
        else if (0x7F <= b && b <= 0x9F)
          continue;
        else if (0xA0 <= b)
        {
          switch (b)
          {
            case 0xA0: result += (char)0x00A0; break; //NO-BREAK SPACE
            case 0xA1: result += (char)0x1E02; break; //LATIN CAPITAL LETTER B WITH DOT ABOVE
            case 0xA2: result += (char)0x1E03; break; //LATIN SMALL LETTER B WITH DOT ABOVE
            case 0xA3: result += (char)0x00A3; break; //POUND SIGN
            case 0xA4: result += (char)0x010A; break; //LATIN CAPITAL LETTER C WITH DOT ABOVE
            case 0xA5: result += (char)0x010B; break; //LATIN SMALL LETTER C WITH DOT ABOVE
            case 0xA6: result += (char)0x1E0A; break; //LATIN CAPITAL LETTER D WITH DOT ABOVE
            case 0xA7: result += (char)0x00A7; break; //SECTION SIGN
            case 0xA8: result += (char)0x1E80; break; //LATIN CAPITAL LETTER W WITH GRAVE
            case 0xA9: result += (char)0x00A9; break; //COPYRIGHT SIGN
            case 0xAA: result += (char)0x1E82; break; //LATIN CAPITAL LETTER W WITH ACUTE
            case 0xAB: result += (char)0x1E0B; break; //LATIN SMALL LETTER D WITH DOT ABOVE
            case 0xAC: result += (char)0x1EF2; break; //LATIN CAPITAL LETTER Y WITH GRAVE
            case 0xAD: result += (char)0x00AD; break; //SOFT HYPHEN
            case 0xAE: result += (char)0x00AE; break; //REGISTERED SIGN
            case 0xAF: result += (char)0x0178; break; //LATIN CAPITAL LETTER Y WITH DIAERESIS
            case 0xB0: result += (char)0x1E1E; break; //LATIN CAPITAL LETTER F WITH DOT ABOVE
            case 0xB1: result += (char)0x1E1F; break; //LATIN SMALL LETTER F WITH DOT ABOVE
            case 0xB2: result += (char)0x0120; break; //LATIN CAPITAL LETTER G WITH DOT ABOVE
            case 0xB3: result += (char)0x0121; break; //LATIN SMALL LETTER G WITH DOT ABOVE
            case 0xB4: result += (char)0x1E40; break; //LATIN CAPITAL LETTER M WITH DOT ABOVE
            case 0xB5: result += (char)0x1E41; break; //LATIN SMALL LETTER M WITH DOT ABOVE
            case 0xB6: result += (char)0x00B6; break; //PILCROW SIGN
            case 0xB7: result += (char)0x1E56; break; //LATIN CAPITAL LETTER P WITH DOT ABOVE
            case 0xB8: result += (char)0x1E81; break; //LATIN SMALL LETTER W WITH GRAVE
            case 0xB9: result += (char)0x1E57; break; //LATIN SMALL LETTER P WITH DOT ABOVE
            case 0xBA: result += (char)0x1E83; break; //LATIN SMALL LETTER W WITH ACUTE
            case 0xBB: result += (char)0x1E60; break; //LATIN CAPITAL LETTER S WITH DOT ABOVE
            case 0xBC: result += (char)0x1EF3; break; //LATIN SMALL LETTER Y WITH GRAVE
            case 0xBD: result += (char)0x1E84; break; //LATIN CAPITAL LETTER W WITH DIAERESIS
            case 0xBE: result += (char)0x1E85; break; //LATIN SMALL LETTER W WITH DIAERESIS
            case 0xBF: result += (char)0x1E61; break; //LATIN SMALL LETTER S WITH DOT ABOVE
            case 0xC0: result += (char)0x00C0; break; //LATIN CAPITAL LETTER A WITH GRAVE
            case 0xC1: result += (char)0x00C1; break; //LATIN CAPITAL LETTER A WITH ACUTE
            case 0xC2: result += (char)0x00C2; break; //LATIN CAPITAL LETTER A WITH CIRCUMFLEX
            case 0xC3: result += (char)0x00C3; break; //LATIN CAPITAL LETTER A WITH TILDE
            case 0xC4: result += (char)0x00C4; break; //LATIN CAPITAL LETTER A WITH DIAERESIS
            case 0xC5: result += (char)0x00C5; break; //LATIN CAPITAL LETTER A WITH RING ABOVE
            case 0xC6: result += (char)0x00C6; break; //LATIN CAPITAL LETTER AE
            case 0xC7: result += (char)0x00C7; break; //LATIN CAPITAL LETTER C WITH CEDILLA
            case 0xC8: result += (char)0x00C8; break; //LATIN CAPITAL LETTER E WITH GRAVE
            case 0xC9: result += (char)0x00C9; break; //LATIN CAPITAL LETTER E WITH ACUTE
            case 0xCA: result += (char)0x00CA; break; //LATIN CAPITAL LETTER E WITH CIRCUMFLEX
            case 0xCB: result += (char)0x00CB; break; //LATIN CAPITAL LETTER E WITH DIAERESIS
            case 0xCC: result += (char)0x00CC; break; //LATIN CAPITAL LETTER I WITH GRAVE
            case 0xCD: result += (char)0x00CD; break; //LATIN CAPITAL LETTER I WITH ACUTE
            case 0xCE: result += (char)0x00CE; break; //LATIN CAPITAL LETTER I WITH CIRCUMFLEX
            case 0xCF: result += (char)0x00CF; break; //LATIN CAPITAL LETTER I WITH DIAERESIS
            case 0xD0: result += (char)0x0174; break; //LATIN CAPITAL LETTER W WITH CIRCUMFLEX
            case 0xD1: result += (char)0x00D1; break; //LATIN CAPITAL LETTER N WITH TILDE
            case 0xD2: result += (char)0x00D2; break; //LATIN CAPITAL LETTER O WITH GRAVE
            case 0xD3: result += (char)0x00D3; break; //LATIN CAPITAL LETTER O WITH ACUTE
            case 0xD4: result += (char)0x00D4; break; //LATIN CAPITAL LETTER O WITH CIRCUMFLEX
            case 0xD5: result += (char)0x00D5; break; //LATIN CAPITAL LETTER O WITH TILDE
            case 0xD6: result += (char)0x00D6; break; //LATIN CAPITAL LETTER O WITH DIAERESIS
            case 0xD7: result += (char)0x1E6A; break; //LATIN CAPITAL LETTER T WITH DOT ABOVE
            case 0xD8: result += (char)0x00D8; break; //LATIN CAPITAL LETTER O WITH STROKE
            case 0xD9: result += (char)0x00D9; break; //LATIN CAPITAL LETTER U WITH GRAVE
            case 0xDA: result += (char)0x00DA; break; //LATIN CAPITAL LETTER U WITH ACUTE
            case 0xDB: result += (char)0x00DB; break; //LATIN CAPITAL LETTER U WITH CIRCUMFLEX
            case 0xDC: result += (char)0x00DC; break; //LATIN CAPITAL LETTER U WITH DIAERESIS
            case 0xDD: result += (char)0x00DD; break; //LATIN CAPITAL LETTER Y WITH ACUTE
            case 0xDE: result += (char)0x0176; break; //LATIN CAPITAL LETTER Y WITH CIRCUMFLEX
            case 0xDF: result += (char)0x00DF; break; //LATIN SMALL LETTER SHARP S
            case 0xE0: result += (char)0x00E0; break; //LATIN SMALL LETTER A WITH GRAVE
            case 0xE1: result += (char)0x00E1; break; //LATIN SMALL LETTER A WITH ACUTE
            case 0xE2: result += (char)0x00E2; break; //LATIN SMALL LETTER A WITH CIRCUMFLEX
            case 0xE3: result += (char)0x00E3; break; //LATIN SMALL LETTER A WITH TILDE
            case 0xE4: result += (char)0x00E4; break; //LATIN SMALL LETTER A WITH DIAERESIS
            case 0xE5: result += (char)0x00E5; break; //LATIN SMALL LETTER A WITH RING ABOVE
            case 0xE6: result += (char)0x00E6; break; //LATIN SMALL LETTER AE
            case 0xE7: result += (char)0x00E7; break; //LATIN SMALL LETTER C WITH CEDILLA
            case 0xE8: result += (char)0x00E8; break; //LATIN SMALL LETTER E WITH GRAVE
            case 0xE9: result += (char)0x00E9; break; //LATIN SMALL LETTER E WITH ACUTE
            case 0xEA: result += (char)0x00EA; break; //LATIN SMALL LETTER E WITH CIRCUMFLEX
            case 0xEB: result += (char)0x00EB; break; //LATIN SMALL LETTER E WITH DIAERESIS
            case 0xEC: result += (char)0x00EC; break; //LATIN SMALL LETTER I WITH GRAVE
            case 0xED: result += (char)0x00ED; break; //LATIN SMALL LETTER I WITH ACUTE
            case 0xEE: result += (char)0x00EE; break; //LATIN SMALL LETTER I WITH CIRCUMFLEX
            case 0xEF: result += (char)0x00EF; break; //LATIN SMALL LETTER I WITH DIAERESIS
            case 0xF0: result += (char)0x0175; break; //LATIN SMALL LETTER W WITH CIRCUMFLEX
            case 0xF1: result += (char)0x00F1; break; //LATIN SMALL LETTER N WITH TILDE
            case 0xF2: result += (char)0x00F2; break; //LATIN SMALL LETTER O WITH GRAVE
            case 0xF3: result += (char)0x00F3; break; //LATIN SMALL LETTER O WITH ACUTE
            case 0xF4: result += (char)0x00F4; break; //LATIN SMALL LETTER O WITH CIRCUMFLEX
            case 0xF5: result += (char)0x00F5; break; //LATIN SMALL LETTER O WITH TILDE
            case 0xF6: result += (char)0x00F6; break; //LATIN SMALL LETTER O WITH DIAERESIS
            case 0xF7: result += (char)0x1E6B; break; //LATIN SMALL LETTER T WITH DOT ABOVE
            case 0xF8: result += (char)0x00F8; break; //LATIN SMALL LETTER O WITH STROKE
            case 0xF9: result += (char)0x00F9; break; //LATIN SMALL LETTER U WITH GRAVE
            case 0xFA: result += (char)0x00FA; break; //LATIN SMALL LETTER U WITH ACUTE
            case 0xFB: result += (char)0x00FB; break; //LATIN SMALL LETTER U WITH CIRCUMFLEX
            case 0xFC: result += (char)0x00FC; break; //LATIN SMALL LETTER U WITH DIAERESIS
            case 0xFD: result += (char)0x00FD; break; //LATIN SMALL LETTER Y WITH ACUTE
            case 0xFE: result += (char)0x0177; break; //LATIN SMALL LETTER Y WITH CIRCUMFLEX
            case 0xFF: result += (char)0x00FF; break; //LATIN SMALL LETTER Y WITH DIAERESIS
          }
        }
      }
      return result;
    }

    /// <summary>
    /// implements decoder for encoding ISO/IEC 6937 with addition of the Euro symbol
    /// according to DVB Standard "ETSI EN 300 468" as "Character code table 00".
    /// Note, that Microsoft .NET implementation of ISO 6937 (as codepage 20269) is wrong,
    /// as it converts composite characters expecting a base character followed by combining character,
    /// and not as specified in the ISO/IEC 6937 standard (i.e. diacritical sign preceeding a base character).
    /// Taken from MP TvEngine3\TVLibrary\TVLibrary\Implementations\DVB\Graphs\Iso6937ToUnicode.cs and modified.
    /// </summary>
    public static string from_ISO_6937_EU(byte[] bytes)
    {
      string result = String.Empty;
      int i = 0;
      byte b = bytes[i++];
      while (b != 0)
      {
        char ch;
        switch (b) {
          #region single byte characters
          case 0xA4: ch = (char)0x20AC; break; //Euro sign in DVB Standard "ETSI EN 300 468" as "Character code table 00" - the only difference to ISO 6937
          case 0xA8: ch = (char)0x00A4; break;
          case 0xA9: ch = (char)0x2018; break;
          case 0xAA: ch = (char)0x201C; break;
          case 0xAC: ch = (char)0x2190; break;
          case 0xAD: ch = (char)0x2191; break;
          case 0xAE: ch = (char)0x2192; break;
          case 0xAF: ch = (char)0x2193; break;
          case 0xB4: ch = (char)0x00D7; break;
          case 0xB8: ch = (char)0x00F7; break;
          case 0xB9: ch = (char)0x2019; break;
          case 0xBA: ch = (char)0x201D; break;
          case 0xD0: ch = (char)0x2015; break;
          case 0xD1: ch = (char)0xB9;   break;
          case 0xD2: ch = (char)0xAE;   break;
          case 0xD3: ch = (char)0xA9;   break;
          case 0xD4: ch = (char)0x2122; break;
          case 0xD5: ch = (char)0x266A; break;
          case 0xD6: ch = (char)0xAC;   break;
          case 0xD7: ch = (char)0xA6;   break;
          case 0xDC: ch = (char)0x215B; break;
          case 0xDD: ch = (char)0x215C; break;
          case 0xDE: ch = (char)0x215D; break;
          case 0xDF: ch = (char)0x215E; break;
          case 0xE0: ch = (char)0x2126; break;
          case 0xE1: ch = (char)0xC6;   break;
          //case 0xE2: ch = (char)0xD0;   break;
          case 0xE2: ch = (char)0x0110; break;
          case 0xE3: ch = (char)0xAA;   break;
          case 0xE4: ch = (char)0x0126; break;
          case 0xE6: ch = (char)0x0132; break;
          case 0xE7: ch = (char)0x013F; break;
          case 0xE8: ch = (char)0x0141; break;
          case 0xE9: ch = (char)0xD8;   break;
          case 0xEA: ch = (char)0x0152; break;
          case 0xEB: ch = (char)0xBA;   break;
          case 0xEC: ch = (char)0xDE;   break;
          case 0xED: ch = (char)0x0166; break;
          case 0xEE: ch = (char)0x014A; break;
          case 0xEF: ch = (char)0x0149; break;
          case 0xF0: ch = (char)0x0138; break;
          case 0xF1: ch = (char)0xE6;   break;
          case 0xF2: ch = (char)0x0111; break;
          case 0xF3: ch = (char)0xF0;   break;
          case 0xF4: ch = (char)0x0127; break;
          case 0xF5: ch = (char)0x0131; break;
          case 0xF6: ch = (char)0x0133; break;
          case 0xF7: ch = (char)0x0140; break;
          case 0xF8: ch = (char)0x0142; break;
          case 0xF9: ch = (char)0xF8;   break;
          case 0xFA: ch = (char)0x0153; break;
          case 0xFB: ch = (char)0xDF;   break;
          case 0xFC: ch = (char)0xFE;   break;
          case 0xFD: ch = (char)0x0167; break;
          case 0xFE: ch = (char)0x014B; break;
          case 0xFF: ch = (char)0xAD;   break;
          #endregion
          #region multibyte
          #region C1
          case 0xC1:
            b = bytes[i++];
            switch (b)
            {
              case 0x41: ch = (char)0xC0; break;
              case 0x45: ch = (char)0xC8; break;
              case 0x49: ch = (char)0xCC; break;
              case 0x4F: ch = (char)0xD2; break;
              case 0x55: ch = (char)0xD9; break;
              case 0x61: ch = (char)0xE0; break;
              case 0x65: ch = (char)0xE8; break;
              case 0x69: ch = (char)0xEC; break;
              case 0x6F: ch = (char)0xF2; break;
              case 0x75: ch = (char)0xF9; break;
              default:   ch = (char)b;    break; // unknown character --> fallback
            }
            break;
          #endregion
          #region C2
          case 0xC2:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0xB4;   break;
              case 0x41: ch = (char)0xC1;   break;
              case 0x43: ch = (char)0x0106; break;
              case 0x45: ch = (char)0xC9;   break;
              case 0x49: ch = (char)0xCD;   break;
              case 0x4C: ch = (char)0x0139; break;
              case 0x4E: ch = (char)0x0143; break;
              case 0x4F: ch = (char)0xD3;   break;
              case 0x52: ch = (char)0x0154; break;
              case 0x53: ch = (char)0x015A; break;
              case 0x55: ch = (char)0xDA;   break;
              case 0x59: ch = (char)0xDD;   break;
              case 0x5A: ch = (char)0x0179; break;
              case 0x61: ch = (char)0xE1;   break;
              case 0x63: ch = (char)0x0107; break;
              case 0x65: ch = (char)0xE9;   break;
              case 0x69: ch = (char)0xED;   break;
              case 0x6C: ch = (char)0x013A; break;
              case 0x6E: ch = (char)0x0144; break;
              case 0x6F: ch = (char)0xF3;   break;
              case 0x72: ch = (char)0x0155; break;
              case 0x73: ch = (char)0x015B; break;
              case 0x75: ch = (char)0xFA;   break;
              case 0x79: ch = (char)0xFD;   break;
              case 0x7A: ch = (char)0x017A; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region C3
          case 0xC3:
            b = bytes[i++];
            switch (b)
            {
              case 0x41: ch = (char)0xC2;   break;
              case 0x43: ch = (char)0x0108; break;
              case 0x45: ch = (char)0xCA;   break;
              case 0x47: ch = (char)0x011C; break;
              case 0x48: ch = (char)0x0124; break;
              case 0x49: ch = (char)0xCE;   break;
              case 0x4A: ch = (char)0x0134; break;
              case 0x4F: ch = (char)0xD4;   break;
              case 0x53: ch = (char)0x015C; break;
              case 0x55: ch = (char)0xDB;   break;
              case 0x57: ch = (char)0x0174; break;
              case 0x59: ch = (char)0x0176; break;
              case 0x61: ch = (char)0xE2;   break;
              case 0x63: ch = (char)0x0109; break;
              case 0x65: ch = (char)0xEA;   break;
              case 0x67: ch = (char)0x011D; break;
              case 0x68: ch = (char)0x0125; break;
              case 0x69: ch = (char)0xEE;   break;
              case 0x6A: ch = (char)0x0135; break;
              case 0x6F: ch = (char)0xF4;   break;
              case 0x73: ch = (char)0x015D; break;
              case 0x75: ch = (char)0xFB;   break;
              case 0x77: ch = (char)0x0175; break;
              case 0x79: ch = (char)0x0177; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region C4
          case 0xC4:
            b = bytes[i++];
            switch (b)
            {
              case 0x41: ch = (char)0x00C3; break;
              case 0x49: ch = (char)0x0128; break;
              case 0x4E: ch = (char)0x00D1; break;
              case 0x4F: ch = (char)0x00D5; break;
              case 0x55: ch = (char)0x0168; break;
              case 0x61: ch = (char)0x00E3; break;
              case 0x69: ch = (char)0x0129; break;
              case 0x6E: ch = (char)0x00F1; break;
              case 0x6F: ch = (char)0x00F5; break;
              case 0x75: ch = (char)0x0169; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region C5
          case 0xC5:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x00AF; break;
              case 0x41: ch = (char)0x0100; break;
              case 0x45: ch = (char)0x0112; break;
              case 0x49: ch = (char)0x012A; break;
              case 0x4F: ch = (char)0x014C; break;
              case 0x55: ch = (char)0x016A; break;
              case 0x61: ch = (char)0x0101; break;
              case 0x65: ch = (char)0x0113; break;
              case 0x69: ch = (char)0x012B; break;
              case 0x6F: ch = (char)0x014D; break;
              case 0x75: ch = (char)0x016B; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region C6
          case 0xC6:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x02D8; break;
              case 0x41: ch = (char)0x0102; break;
              case 0x47: ch = (char)0x011E; break;
              case 0x55: ch = (char)0x016C; break;
              case 0x61: ch = (char)0x0103; break;
              case 0x67: ch = (char)0x011F; break;
              case 0x75: ch = (char)0x016D; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region C7
          case 0xC7:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x02D9; break;
              case 0x43: ch = (char)0x010A; break;
              case 0x45: ch = (char)0x0116; break;
              case 0x47: ch = (char)0x0120; break;
              case 0x49: ch = (char)0x0130; break;
              case 0x5A: ch = (char)0x017B; break;
              case 0x63: ch = (char)0x010B; break;
              case 0x65: ch = (char)0x0117; break;
              case 0x67: ch = (char)0x0121; break;
              case 0x7A: ch = (char)0x017C; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region C8
          case 0xC8:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x00A8; break;
              case 0x41: ch = (char)0x00C4; break;
              case 0x45: ch = (char)0x00CB; break;
              case 0x49: ch = (char)0x00CF; break;
              case 0x4F: ch = (char)0x00D6; break;
              case 0x55: ch = (char)0x00DC; break;
              case 0x59: ch = (char)0x0178; break;
              case 0x61: ch = (char)0x00E4; break;
              case 0x65: ch = (char)0x00EB; break;
              case 0x69: ch = (char)0x00EF; break;
              case 0x6F: ch = (char)0x00F6; break;
              case 0x75: ch = (char)0x00FC; break;
              case 0x79: ch = (char)0x00FF; break;
              default:   ch = (char)b;     break; // unknown character --> fallback
            }
            break;
          #endregion
          #region CA
          case 0xCA:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x02DA; break;
              case 0x41: ch = (char)0xC5;   break;
              case 0x55: ch = (char)0x016E; break;
              case 0x61: ch = (char)0xE5;   break;
              case 0x75: ch = (char)0x016F; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region CB
          case 0xCB:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0xB8;   break;
              case 0x43: ch = (char)0xC7;   break;
              case 0x47: ch = (char)0x0122; break;
              case 0x4B: ch = (char)0x136;  break;
              case 0x4C: ch = (char)0x013B; break;
              case 0x4E: ch = (char)0x0145; break;
              case 0x52: ch = (char)0x0156; break;
              case 0x53: ch = (char)0x015E; break;
              case 0x54: ch = (char)0x0162; break;
              case 0x63: ch = (char)0xE7;   break;
              case 0x67: ch = (char)0x0123; break;
              case 0x6B: ch = (char)0x0137; break;
              case 0x6C: ch = (char)0x013C; break;
              case 0x6E: ch = (char)0x0146; break;
              case 0x72: ch = (char)0x0157; break;
              case 0x73: ch = (char)0x015F; break;
              case 0x74: ch = (char)0x0163; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region CD
          case 0xCD:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x02DD; break;
              case 0x4F: ch = (char)0x0150; break;
              case 0x55: ch = (char)0x0170; break;
              case 0x6F: ch = (char)0x0151; break;
              case 0x75: ch = (char)0x0171; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region CE
          case 0xCE:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x02DB; break;
              case 0x41: ch = (char)0x0104; break;
              case 0x45: ch = (char)0x0118; break;
              case 0x49: ch = (char)0x012E; break;
              case 0x55: ch = (char)0x0172; break;
              case 0x61: ch = (char)0x0105; break;
              case 0x65: ch = (char)0x0119; break;
              case 0x69: ch = (char)0x012F; break;
              case 0x75: ch = (char)0x0173; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #region CF
          case 0xCF:
            b = bytes[i++];
            switch (b)
            {
              case 0x20: ch = (char)0x02C7; break;
              case 0x43: ch = (char)0x010C; break;
              case 0x44: ch = (char)0x010E; break;
              case 0x45: ch = (char)0x011A; break;
              case 0x4C: ch = (char)0x013D; break;
              case 0x4E: ch = (char)0x0147; break;
              case 0x52: ch = (char)0x0158; break;
              case 0x53: ch = (char)0x0160; break;
              case 0x54: ch = (char)0x0164; break;
              case 0x5A: ch = (char)0x017D; break;
              case 0x63: ch = (char)0x010D; break;
              case 0x64: ch = (char)0x010F; break;
              case 0x65: ch = (char)0x011B; break;
              case 0x6C: ch = (char)0x013E; break;
              case 0x6E: ch = (char)0x0148; break;
              case 0x72: ch = (char)0x0159; break;
              case 0x73: ch = (char)0x0161; break;
              case 0x74: ch = (char)0x0165; break;
              case 0x7A: ch = (char)0x017E; break;
              default:   ch = (char)b;      break; // unknown character --> fallback
            }
            break;
          #endregion
          #endregion
          // rest is the same
          default: ch = (char)b; break;
        }
        if (ch != 0)
          result += ch;
        if ((b != 0) && (i < bytes.Length))
          b = bytes[i++];
        else
          break;
      }
      return result;
    }


    public static bool is_ISO_6937(byte[] bytes)
    {
      bool isConsistent = true;
      int i = 0;
      byte b = bytes[i++];
      while (b != 0)
      {
        switch (b) {
          #region check for characters not defined in standard
          case 0x7F:
          case 0x80:
          case 0x81:
          case 0x82:
          case 0x83:
          case 0x84:
          case 0x85:
          case 0x86:
          case 0x87:
          case 0x88:
          case 0x89:
          case 0x8A:
          case 0x8B:
          case 0x8C:
          case 0x8D:
          case 0x8E:
          case 0x8F:
          case 0x90:
          case 0x91:
          case 0x92:
          case 0x93:
          case 0x94:
          case 0x95:
          case 0x96:
          case 0x97:
          case 0x98:
          case 0x99:
          case 0x9A:
          case 0x9B:
          case 0x9C:
          case 0x9D:
          case 0x9E:
          case 0x9F:
          case 0xA6:
          case 0xC0:
          case 0xC9:
          case 0xCC:
          case 0xD8:
          case 0xD9:
          case 0xDA:
          case 0xDB:
          case 0xE5: isConsistent = false; break;
          #endregion
          #region multibyte - check chars that are not allowed after diacritics
          #region C1
          case 0xC1:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x45:
              case 0x49:
              case 0x4F:
              case 0x55:
              case 0x61:
              case 0x65:
              case 0x69:
              case 0x6F:
              case 0x75: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region C2
          case 0xC2:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x43:
              case 0x45:
              case 0x49:
              case 0x4C:
              case 0x4E:
              case 0x4F:
              case 0x52:
              case 0x53:
              case 0x55:
              case 0x59:
              case 0x5A:
              case 0x61:
              case 0x63:
              case 0x65:
              case 0x69:
              case 0x6C:
              case 0x6E:
              case 0x6F:
              case 0x72:
              case 0x73:
              case 0x75:
              case 0x79:
              case 0x7A: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region C3
          case 0xC3:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x43:
              case 0x45:
              case 0x47:
              case 0x48:
              case 0x49:
              case 0x4A:
              case 0x4F:
              case 0x53:
              case 0x55:
              case 0x57:
              case 0x59:
              case 0x61:
              case 0x63:
              case 0x65:
              case 0x67:
              case 0x68:
              case 0x69:
              case 0x6A:
              case 0x6F:
              case 0x73:
              case 0x75:
              case 0x77:
              case 0x79: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region C4
          case 0xC4:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x49:
              case 0x4E:
              case 0x4F:
              case 0x55:
              case 0x61:
              case 0x69:
              case 0x6E:
              case 0x6F:
              case 0x75: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region C5
          case 0xC5:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x45:
              case 0x49:
              case 0x4F:
              case 0x55:
              case 0x61:
              case 0x65:
              case 0x69:
              case 0x6F:
              case 0x75:
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region C6
          case 0xC6:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x47:
              case 0x55:
              case 0x61:
              case 0x67:
              case 0x75:
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region C7
          case 0xC7:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x43:
              case 0x45:
              case 0x47:
              case 0x49:
              case 0x5A:
              case 0x63:
              case 0x65:
              case 0x67:
              case 0x7A: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region C8
          case 0xC8:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x45:
              case 0x49:
              case 0x4F:
              case 0x55:
              case 0x59:
              case 0x61:
              case 0x65:
              case 0x69:
              case 0x6F:
              case 0x75:
              case 0x79: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region CA
          case 0xCA:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x55:
              case 0x61:
              case 0x75: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region CB
          case 0xCB:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x43:
              case 0x47:
              case 0x4B:
              case 0x4C:
              case 0x4E:
              case 0x52:
              case 0x53:
              case 0x54:
              case 0x63:
              case 0x67:
              case 0x6B:
              case 0x6C:
              case 0x6E:
              case 0x72:
              case 0x73:
              case 0x74: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region CD
          case 0xCD:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x4F:
              case 0x55:
              case 0x6F:
              case 0x75: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region CE
          case 0xCE:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x41:
              case 0x45:
              case 0x49:
              case 0x55:
              case 0x61:
              case 0x65:
              case 0x69:
              case 0x75: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #region CF
          case 0xCF:
            if (i == bytes.Length) {
              isConsistent = false;
              break;
            }
            b = bytes[i++];
            switch (b)
            {
              case 0x20:
              case 0x43:
              case 0x44:
              case 0x45:
              case 0x4C:
              case 0x4E:
              case 0x52:
              case 0x53:
              case 0x54:
              case 0x5A:
              case 0x63:
              case 0x64:
              case 0x65:
              case 0x6C:
              case 0x6E:
              case 0x72:
              case 0x73:
              case 0x74:
              case 0x7A: break;
              default:   isConsistent = false; break;
            }
            break;
          #endregion
          #endregion
        }
        if ((b != 0) && (i < bytes.Length) && isConsistent)
            b = bytes[i++];
        else
            break;
      }
      return isConsistent;

    }
  }
}
