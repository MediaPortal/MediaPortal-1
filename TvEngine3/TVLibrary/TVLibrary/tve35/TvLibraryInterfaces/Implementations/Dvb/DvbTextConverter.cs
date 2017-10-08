#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Runtime.InteropServices;
using System.Text;
using TvLibrary.Log;
using Dump = TvLibrary.Implementations.DVB.DVB_MMI;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb
{
  /// <summary>
  /// This class converts text encoded according to EN 300 468 annex A to Unicode.
  /// </summary>
  public class DvbTextConverter
  {
    /// <summary>
    /// Convert DVB-encoded string in a NULL terminated buffer to Unicode.
    /// </summary>
    /// <param name="buffer">The buffer containing the string to convert.</param>
    /// <param name="encodedByteCount">The number of bytes in <paramref name="buffer"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="buffer"/> at which the string starts.</param>
    /// <returns>the converted string</returns>
    public static string Convert(IntPtr buffer, int encodedByteCount = -1, int offset = 0)
    {
      int decodedByteCount;
      return Convert(buffer, encodedByteCount, offset, out decodedByteCount);
    }

    /// <summary>
    /// Convert DVB-encoded string in a NULL terminated buffer to Unicode.
    /// </summary>
    /// <param name="buffer">The buffer containing the string to convert.</param>
    /// <param name="encodedByteCount">The number of bytes in <paramref name="buffer"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="buffer"/> at which the string starts.</param>
    /// <param name="decodedByteCount">The number of bytes actually read when decoding/converting the string.</param>
    /// <returns>the converted string</returns>
    public static string Convert(IntPtr buffer, int encodedByteCount, int offset, out int decodedByteCount)
    {
      decodedByteCount = 0;
      if (buffer == IntPtr.Zero)
      {
        return string.Empty;
      }

      // Simplify offset handling.
      IntPtr ptr = IntPtr.Add(buffer, offset);

      // If we were told how many bytes were used to encode the string then just decode/convert it.
      byte[] bytes;
      if (encodedByteCount >= 0)
      {
        bytes = new byte[encodedByteCount];
        Marshal.Copy(ptr, bytes, 0, encodedByteCount);
        return Convert(bytes, encodedByteCount, 0, out decodedByteCount);
      }

      // Otherwise we have to carefully figure out how long the string is.
      // This requires us to know the encoding.
      byte b1 = Marshal.ReadByte(ptr, 0);
      byte b2 = 0;
      bool isIso10646 = false;  // 2 byte characters
      if (b1 < 0x20)
      {
        decodedByteCount++;
        if (b1 == 0)
        {
          return string.Empty;
        }
        if (b1 == 0x11)
        {
          isIso10646 = true;
        }
        else if (b1 == 0x10)
        {
          b1 = Marshal.ReadByte(ptr, decodedByteCount++);
          if (b1 == 0)
          {
            b1 = Marshal.ReadByte(ptr, decodedByteCount++);
            if (b1 == 0)
            {
              return string.Empty;
            }
          }
        }
      }

      int nullTerminationByteCount = 0;
      while (true)
      {
        if (isIso10646)
        {
          b1 = Marshal.ReadByte(ptr, decodedByteCount);
          b2 = Marshal.ReadByte(ptr, decodedByteCount + 1);
          if (b1 == 0 && b2 == 0)
          {
            nullTerminationByteCount = 2;
            break;
          }
          decodedByteCount += 2;
        }
        else
        {
          b1 = Marshal.ReadByte(ptr, decodedByteCount);
          if (b1 == 0)
          {
            nullTerminationByteCount = 1;
            break;
          }
          decodedByteCount++;
        }
      }

      bytes = new byte[decodedByteCount];
      Marshal.Copy(ptr, bytes, 0, decodedByteCount);
      string s = Convert(bytes, decodedByteCount, 0, out decodedByteCount);
      // The reported decoded byte count must include the NULL termination byte(s)
      // so that the caller can jump over those bytes when processing NULL
      // terminated strings in inline buffers.
      decodedByteCount += nullTerminationByteCount;
      return s;
    }

    /// <summary>
    /// Convert a DVB-encoded string in a byte array to Unicode.
    /// </summary>
    /// <param name="stringBytes">The byte array containing the string to convert.</param>
    /// <param name="encodedByteCount">The number of bytes in <paramref name="stringBytes"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="stringBytes"/> at which the string starts.</param>
    /// <returns>the converted string</returns>
    public static string Convert(byte[] stringBytes, int encodedByteCount = -1, int offset = 0)
    {
      int decodedByteCount;
      return Convert(stringBytes, encodedByteCount, offset, out decodedByteCount);
    }

    /// <summary>
    /// Convert a DVB-encoded string in a byte array to Unicode.
    /// </summary>
    /// <param name="stringBytes">The byte array containing the string to convert.</param>
    /// <param name="encodedByteCount">The number of bytes in <paramref name="stringBytes"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="stringBytes"/> at which the string starts.</param>
    /// <param name="decodedByteCount">The number of bytes actually read when decoding/converting the string.</param>
    /// <returns>the converted string</returns>
    public static string Convert(byte[] stringBytes, int encodedByteCount, int offset, out int decodedByteCount)
    {
      decodedByteCount = 0;
      if (stringBytes == null || stringBytes.Length == 0)
      {
        return string.Empty;
      }
      else if (stringBytes.Length <= offset || encodedByteCount > stringBytes.Length - offset)
      {
        Log.Error("DVB text: unexpected parameter values, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
        Dump.DumpBinary(stringBytes);
        return string.Empty;
      }

      // Select a compatible Windows code page.
      int stringByteCount = stringBytes.Length;
      int codePage = 20269; // ISO/IEC 6937
      int encodingByteCount = 0;
      byte b1 = stringBytes[offset];
      byte b2 = 0;
      try
      {
        if (b1 < 0x20)
        {
          encodingByteCount++;

          switch (b1)
          {
            case 0x00:
              return string.Empty;
            case 0x10:
              // This code is very intentional. TsWriter removes the second
              // encoding byte to avoid premature NULL termination. However,
              // this function is also used to process DVB text originating
              // from CI/CAMs. Do NOT change unless you know what you are
              // doing!
              if (stringByteCount <= offset + encodingByteCount)
              {
                Log.Error("DVB text: unexpected end of byte sequence after three byte encoding indicator, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
                Dump.DumpBinary(stringBytes);
                return string.Empty;
              }
              b1 = stringBytes[offset + encodingByteCount++];
              if (b1 == 0)
              {
                if (stringByteCount <= offset + encodingByteCount)
                {
                  Log.Error("DVB text: unexpected end of byte sequence after three byte encoding reserved byte, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
                  Dump.DumpBinary(stringBytes);
                  return string.Empty;
                }
                b1 = stringBytes[offset + encodingByteCount++];
              }
              if (b1 == 0)
              {
                Log.Error("DVB text: unexpected end of byte sequence after three byte encoding byte, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
                Dump.DumpBinary(stringBytes);
                return string.Empty;
              }
              else if (b1 < 0x10) // ISO/IEC 8859-1..15
              {
                codePage = 28590 + b1;
              }
              else
              {
                Log.Error("DVB text: unsupported three byte encoding byte 0x{0:x}, byte count = {1}, encoded byte count = {2}, offset = {3}", b1, stringBytes.Length, encodedByteCount, offset);
                Dump.DumpBinary(stringBytes);
                return string.Empty;
              }
              break;
            case 0x11:
              codePage = 1200;  // ISO/IEC 10646-1 little endian
              break;
            case 0x12:
              codePage = 949;   // KSX1001-2004 / KSC5601-1987
              break;
            case 0x13:
              codePage = 936;   // GB-2312-1980
              break;
            case 0x14:
              codePage = 950;   // Big5
              break;
            case 0x15:
              codePage = 65001; // UTF-8
              break;
            default:
              if (b1 > 0xb)
              {
                Log.Error("DVB text: unsupported one byte encoding byte 0x{0:x}, byte count = {1}, encoded byte count = {2}, offset = {3}", b1, stringBytes.Length, encodedByteCount, offset);
                Dump.DumpBinary(stringBytes);
                return string.Empty;
              }
              codePage = 28594 + b1;  // ISO/IEC 8859-5..15
              break;
          }

          if (codePage == 28601)  // ISO/IEC 8859-11
          {
            // Windows doesn't directly support ISO/IEC 8859-11. Use compatible
            // code page 874, which is an extension of TIS-620 (TIS-620 is
            // compatible with ISO/IEC 8859-11).
            codePage = 874;
          }
        }
      }
      finally
      {
        decodedByteCount = encodingByteCount;
      }

      // If necessary, count the number of bytes in the string. Stop at the first NULL termination.
      int nullTerminationByteCount = 0;
      if (encodedByteCount == -1)
      {
        decodedByteCount += offset;
        while (decodedByteCount < stringByteCount)
        {
          if (codePage == 1200)
          {
            if (decodedByteCount >= stringByteCount - 1)
            {
              break;
            }
            b1 = stringBytes[decodedByteCount++];
            b2 = stringBytes[decodedByteCount++];
            if (b1 == 0 && b2 == 0)
            {
              nullTerminationByteCount = 2;
              break;
            }
          }
          else
          {
            b1 = stringBytes[decodedByteCount++];
            if (b1 == 0)
            {
              nullTerminationByteCount = 1;
              break;
            }
          }
        }
        decodedByteCount -= offset;
      }
      else
      {
        // Assumption: the byte count passed in is correct and does not include
        // any NULL termination bytes.
        decodedByteCount = encodedByteCount;
      }

      int bytesToConvertCount = decodedByteCount - encodingByteCount - nullTerminationByteCount;
      switch (codePage)
      {
        case 20269:   // ISO/IEC 6937-1
        case 28600:   // ISO/IEC 8859-10
        case 28604:   // ISO/IEC 8859-14
          byte[] bytes = new byte[bytesToConvertCount];
          Buffer.BlockCopy(stringBytes, offset + encodingByteCount, bytes, 0, bytesToConvertCount);
          if (codePage == 20269)
          {
            return IsoIec6937ToUnicode(bytes);
          }
          if (codePage == 28600)
          {
            return IsoIec8859P10ToUnicode(bytes);
          }
          return IsoIec8859P14ToUnicode(bytes);
        case 28602:   // ISO/IEC 8859-12
          Log.Error("DVB text: selected ISO/IEC 8859-12 encoding does not exist, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
          Dump.DumpBinary(stringBytes);
          return string.Empty;
        default:
          return Encoding.GetEncoding(codePage).GetString(stringBytes, offset + encodingByteCount, bytesToConvertCount);
      }
    }

    /// <summary>
    /// Convert ISO/IEC 8859-10 (Latin/Nordic) to Unicode.
    /// </summary>
    /// <remarks>
    /// Microsoft has not yet implemented a .NET decoder for codepage 28600.
    /// </remarks>
    private static string IsoIec8859P10ToUnicode(byte[] bytes)
    {
      StringBuilder result = new StringBuilder();
      foreach (byte b in bytes)
      {
        if (b == 0x00)
        {
          break;
        }

        switch (b)
        {
          case 0xa0:
            result.Append((char)0x00a0);  // NO-BREAK SPACE
            break;
          case 0xa1:
            result.Append((char)0x0104);  // LATIN CAPITAL LETTER A WITH OGONEK
            break;
          case 0xa2:
            result.Append((char)0x0112);  // LATIN CAPITAL LETTER E WITH MACRON
            break;
          case 0xa3:
            result.Append((char)0x0122);  // LATIN CAPITAL LETTER G WITH CEDILLA
            break;
          case 0xa4:
            result.Append((char)0x012a);  // LATIN CAPITAL LETTER I WITH MACRON
            break;
          case 0xa5:
            result.Append((char)0x0128);  // LATIN CAPITAL LETTER I WITH TILDE
            break;
          case 0xa6:
            result.Append((char)0x0136);  // LATIN CAPITAL LETTER K WITH CEDILLA
            break;
          case 0xa7:
            result.Append((char)0x00a7);  // SECTION SIGN
            break;
          case 0xa8:
            result.Append((char)0x013b);  // LATIN CAPITAL LETTER L WITH CEDILLA
            break;
          case 0xa9:
            result.Append((char)0x0110);  // LATIN CAPITAL LETTER D WITH STROKE
            break;
          case 0xaa:
            result.Append((char)0x0160);  // LATIN CAPITAL LETTER S WITH CARON
            break;
          case 0xab:
            result.Append((char)0x0166);  // LATIN CAPITAL LETTER T WITH STROKE
            break;
          case 0xac:
            result.Append((char)0x017d);  // LATIN CAPITAL LETTER Z WITH CARON
            break;
          case 0xad:
            result.Append((char)0x00ad);  // SOFT HYPHEN
            break;
          case 0xae:
            result.Append((char)0x016a);  // LATIN CAPITAL LETTER U WITH MACRON
            break;
          case 0xaf:
            result.Append((char)0x014a);  // LATIN CAPITAL LETTER ENG
            break;
          case 0xb0:
            result.Append((char)0x00b0);  // DEGREE SIGN
            break;
          case 0xb1:
            result.Append((char)0x0105);  // LATIN SMALL LETTER A WITH OGONEK
            break;
          case 0xb2:
            result.Append((char)0x0113);  // LATIN SMALL LETTER E WITH MACRON
            break;
          case 0xb3:
            result.Append((char)0x0123);  // LATIN SMALL LETTER G WITH CEDILLA
            break;
          case 0xb4:
            result.Append((char)0x012b);  // LATIN SMALL LETTER I WITH MACRON
            break;
          case 0xb5:
            result.Append((char)0x0129);  // LATIN SMALL LETTER I WITH TILDE
            break;
          case 0xb6:
            result.Append((char)0x0137);  // LATIN SMALL LETTER K WITH CEDILLA
            break;
          case 0xb7:
            result.Append((char)0x00b7);  // MIDDLE DOT
            break;
          case 0xb8:
            result.Append((char)0x013c);  // LATIN SMALL LETTER L WITH CEDILLA
            break;
          case 0xb9:
            result.Append((char)0x0111);  // LATIN SMALL LETTER D WITH STROKE
            break;
          case 0xba:
            result.Append((char)0x0161);  // LATIN SMALL LETTER S WITH CARON
            break;
          case 0xbb:
            result.Append((char)0x0167);  // LATIN SMALL LETTER T WITH STROKE
            break;
          case 0xbc:
            result.Append((char)0x017e);  // LATIN SMALL LETTER Z WITH CARON
            break;
          case 0xbd:
            result.Append((char)0x2015);  // HORIZONTAL BAR
            break;
          case 0xbe:
            result.Append((char)0x016b);  // LATIN SMALL LETTER U WITH MACRON
            break;
          case 0xbf:
            result.Append((char)0x014b);  // LATIN SMALL LETTER ENG
            break;

          case 0xc0:
            result.Append((char)0x0100);  // LATIN CAPITAL LETTER A WITH MACRON
            break;
          case 0xc7:
            result.Append((char)0x012e);  // LATIN CAPITAL LETTER I WITH OGONEK
            break;
          case 0xc8:
            result.Append((char)0x010c);  // LATIN CAPITAL LETTER C WITH CARON
            break;
          case 0xca:
            result.Append((char)0x0118);  // LATIN CAPITAL LETTER E WITH OGONEK
            break;
          case 0xcc:
            result.Append((char)0x0116);  // LATIN CAPITAL LETTER E WITH DOT ABOVE
            break;

          case 0xd1:
            result.Append((char)0x0145);  // LATIN CAPITAL LETTER N WITH CEDILLA
            break;
          case 0xd2:
            result.Append((char)0x014c);  // LATIN CAPITAL LETTER O WITH MACRON
            break;
          case 0xd7:
            result.Append((char)0x0168);  // LATIN CAPITAL LETTER U WITH TILDE
            break;
          case 0xd9:
            result.Append((char)0x0172);  // LATIN CAPITAL LETTER U WITH OGONEK
            break;

          case 0xe0:
            result.Append((char)0x0101);  // LATIN SMALL LETTER A WITH MACRON
            break;
          case 0xe7:
            result.Append((char)0x012f);  // LATIN SMALL LETTER I WITH OGONEK
            break;
          case 0xe8:
            result.Append((char)0x010d);  // LATIN SMALL LETTER C WITH CARON
            break;
          case 0xea:
            result.Append((char)0x0119);  // LATIN SMALL LETTER E WITH OGONEK
            break;
          case 0xec:
            result.Append((char)0x0117);  // LATIN SMALL LETTER E WITH DOT ABOVE
            break;

          case 0xf1:
            result.Append((char)0x0146);  // LATIN SMALL LETTER N WITH CEDILLA
            break;
          case 0xf2:
            result.Append((char)0x014d);  // LATIN SMALL LETTER O WITH MACRON
            break;
          case 0xf7:
            result.Append((char)0x0169);  // LATIN SMALL LETTER U WITH TILDE
            break;
          case 0xf9:
            result.Append((char)0x0173);  // LATIN SMALL LETTER U WITH OGONEK
            break;
          default:
            if (0x7f <= b && b <= 0x9f)
            {
              Log.Error("DVB text: unexpected byte/character 0x{0:x} in ISO/IEC 8859-10 string", b);
            }
            else
            {
              result.Append((char)b);
            }
            break;
        }
      }
      return result.ToString();
    }

    /// <summary>
    /// Convert ISO/IEC 8859-14 (Latin/Celtic) to Unicode.
    /// </summary>
    /// <remarks>
    /// Microsoft has not yet implemented a .NET decoder for codepage 28604.
    /// </remarks>
    private static string IsoIec8859P14ToUnicode(byte[] bytes)
    {
      StringBuilder result = new StringBuilder();
      foreach (byte b in bytes)
      {
        if (b == 0x00)
        {
          break;
        }

        switch (b)
        {
          case 0xa0:
            result.Append((char)0x00a0);  // NO-BREAK SPACE
            break;
          case 0xa1:
            result.Append((char)0x1e02);  // LATIN CAPITAL LETTER B WITH DOT ABOVE
            break;
          case 0xa2:
            result.Append((char)0x1e03);  // LATIN SMALL LETTER B WITH DOT ABOVE
            break;
          case 0xa3:
            result.Append((char)0x00a3);  // POUND SIGN
            break;
          case 0xa4:
            result.Append((char)0x010a);  // LATIN CAPITAL LETTER C WITH DOT ABOVE
            break;
          case 0xa5:
            result.Append((char)0x010b);  // LATIN SMALL LETTER C WITH DOT ABOVE
            break;
          case 0xa6:
            result.Append((char)0x1e0a);  // LATIN CAPITAL LETTER D WITH DOT ABOVE
            break;
          case 0xa7:
            result.Append((char)0x00a7);  // SECTION SIGN
            break;
          case 0xa8:
            result.Append((char)0x1e80);  // LATIN CAPITAL LETTER W WITH GRAVE
            break;
          case 0xa9:
            result.Append((char)0x00a9);  // COPYRIGHT SIGN
            break;
          case 0xaa:
            result.Append((char)0x1e82);  // LATIN CAPITAL LETTER W WITH ACUTE
            break;
          case 0xab:
            result.Append((char)0x1e0b);  // LATIN SMALL LETTER D WITH DOT ABOVE
            break;
          case 0xac:
            result.Append((char)0x1ef2);  // LATIN CAPITAL LETTER Y WITH GRAVE
            break;
          case 0xad:
            result.Append((char)0x00ad);  // SOFT HYPHEN
            break;
          case 0xae:
            result.Append((char)0x00ae);  // REGISTERED SIGN
            break;
          case 0xaf:
            result.Append((char)0x0178);  // LATIN CAPITAL LETTER Y WITH DIAERESIS
            break;
          case 0xb0:
            result.Append((char)0x1e1e);  // LATIN CAPITAL LETTER F WITH DOT ABOVE
            break;
          case 0xb1:
            result.Append((char)0x1e1f);  // LATIN SMALL LETTER F WITH DOT ABOVE
            break;
          case 0xb2:
            result.Append((char)0x0120);  // LATIN CAPITAL LETTER G WITH DOT ABOVE
            break;
          case 0xb3:
            result.Append((char)0x0121);  // LATIN SMALL LETTER G WITH DOT ABOVE
            break;
          case 0xb4:
            result.Append((char)0x1e40);  // LATIN CAPITAL LETTER M WITH DOT ABOVE
            break;
          case 0xb5:
            result.Append((char)0x1e41);  // LATIN SMALL LETTER M WITH DOT ABOVE
            break;
          case 0xb6:
            result.Append((char)0x00b6);  // PILCROW SIGN
            break;
          case 0xb7:
            result.Append((char)0x1e56);  // LATIN CAPITAL LETTER P WITH DOT ABOVE
            break;
          case 0xb8:
            result.Append((char)0x1e81);  // LATIN SMALL LETTER W WITH GRAVE
            break;
          case 0xb9:
            result.Append((char)0x1e57);  // LATIN SMALL LETTER P WITH DOT ABOVE
            break;
          case 0xba:
            result.Append((char)0x1e83);  // LATIN SMALL LETTER W WITH ACUTE
            break;
          case 0xbb:
            result.Append((char)0x1e60);  // LATIN CAPITAL LETTER S WITH DOT ABOVE
            break;
          case 0xbc:
            result.Append((char)0x1ef3);  // LATIN SMALL LETTER Y WITH GRAVE
            break;
          case 0xbd:
            result.Append((char)0x1e84);  // LATIN CAPITAL LETTER W WITH DIAERESIS
            break;
          case 0xbe:
            result.Append((char)0x1e85);  // LATIN SMALL LETTER W WITH DIAERESIS
            break;
          case 0xbf:
            result.Append((char)0x1e61);  // LATIN SMALL LETTER S WITH DOT ABOVE
            break;

          case 0xd0:
            result.Append((char)0x0174);  // LATIN CAPITAL LETTER W WITH CIRCUMFLEX
            break;
          case 0xd7:
            result.Append((char)0x1e6a);  // LATIN CAPITAL LETTER T WITH DOT ABOVE
            break;
          case 0xde:
            result.Append((char)0x0176);  // LATIN CAPITAL LETTER Y WITH CIRCUMFLEX
            break;
          case 0xf0:
            result.Append((char)0x0175);  // LATIN SMALL LETTER W WITH CIRCUMFLEX
            break;
          case 0xf7:
            result.Append((char)0x1e6b);  // LATIN SMALL LETTER T WITH DOT ABOVE
            break;
          case 0xfe:
            result.Append((char)0x0177);  // LATIN SMALL LETTER Y WITH CIRCUMFLEX
            break;
          default:
            if (0x7f <= b && b <= 0x9f)
            {
              Log.Error("DVB text: unexpected byte/character 0x{0:x} in ISO/IEC 8859-14 string", b);
            }
            else
            {
              result.Append((char)b);
            }
            break;
        }
      }
      return result.ToString();
    }

    /// <summary>
    /// Convert ISO/IEC 6937-1 to Unicode.
    /// </summary>
    /// <remarks>
    /// The Microsoft .NET ISO/IEC 6937-1 decoder implementation (codepage
    /// 20269) does not convert composite characters correctly. It expects a
    /// base character followed by combining character. ISO/IEC 6937-1 expects
    /// the diacritical sign to precede the base character.
    /// </remarks>
    private static string IsoIec6937ToUnicode(byte[] bytes)
    {
      StringBuilder result = new StringBuilder();
      int offset = 0;
      while (true)
      {
        if (offset >= bytes.Length)
        {
          break;
        }
        byte b = bytes[offset++];
        if (b == 0)
        {
          break;
        }
        char ch = (char)0;
        switch (b)
        {
          #region single byte characters
          case 0xa4:
            ch = (char)0x20ac;  // euro sign - the only difference to ISO/IEC 6937-1
            break;
          case 0xa8:
            ch = (char)0x00a4;
            break;
          case 0xa9:
            ch = (char)0x2018;
            break;
          case 0xaa:
            ch = (char)0x201c;
            break;
          case 0xac:
            ch = (char)0x2190;
            break;
          case 0xad:
            ch = (char)0x2191;
            break;
          case 0xae:
            ch = (char)0x2192;
            break;
          case 0xaf:
            ch = (char)0x2193;
            break;
          case 0xb4:
            ch = (char)0x00d7;
            break;
          case 0xb8:
            ch = (char)0x00f7;
            break;
          case 0xb9:
            ch = (char)0x2019;
            break;
          case 0xba:
            ch = (char)0x201d;
            break;
          case 0xd0:
            ch = (char)0x2015;
            break;
          case 0xd1:
            ch = (char)0xb9;
            break;
          case 0xd2:
            ch = (char)0xae;
            break;
          case 0xd3:
            ch = (char)0xa9;
            break;
          case 0xd4:
            ch = (char)0x2122;
            break;
          case 0xd5:
            ch = (char)0x266a;
            break;
          case 0xd6:
            ch = (char)0xac;
            break;
          case 0xd7:
            ch = (char)0xa6;
            break;
          case 0xdc:
            ch = (char)0x215b;
            break;
          case 0xdd:
            ch = (char)0x215c;
            break;
          case 0xde:
            ch = (char)0x215d;
            break;
          case 0xdf:
            ch = (char)0x215e;
            break;
          case 0xe0:
            ch = (char)0x2126;
            break;
          case 0xe1:
            ch = (char)0xc6;
            break;
          case 0xe2:
            ch = (char)0x0110;
            break;
          case 0xe3:
            ch = (char)0xaa;
            break;
          case 0xe4:
            ch = (char)0x0126;
            break;
          case 0xe6:
            ch = (char)0x0132;
            break;
          case 0xe7:
            ch = (char)0x013f;
            break;
          case 0xe8:
            ch = (char)0x0141;
            break;
          case 0xe9:
            ch = (char)0xd8;
            break;
          case 0xea:
            ch = (char)0x0152;
            break;
          case 0xeb:
            ch = (char)0xba;
            break;
          case 0xec:
            ch = (char)0xde;
            break;
          case 0xed:
            ch = (char)0x0166;
            break;
          case 0xee:
            ch = (char)0x014a;
            break;
          case 0xef:
            ch = (char)0x0149;
            break;
          case 0xf0:
            ch = (char)0x0138;
            break;
          case 0xf1:
            ch = (char)0xe6;
            break;
          case 0xf2:
            ch = (char)0x0111;
            break;
          case 0xf3:
            ch = (char)0xf0;
            break;
          case 0xf4:
            ch = (char)0x0127;
            break;
          case 0xf5:
            ch = (char)0x0131;
            break;
          case 0xf6:
            ch = (char)0x0133;
            break;
          case 0xf7:
            ch = (char)0x0140;
            break;
          case 0xf8:
            ch = (char)0x0142;
            break;
          case 0xf9:
            ch = (char)0xf8;
            break;
          case 0xfa:
            ch = (char)0x0153;
            break;
          case 0xfb:
            ch = (char)0xdf;
            break;
          case 0xfc:
            ch = (char)0xfe;
            break;
          case 0xfd:
            ch = (char)0x0167;
            break;
          case 0xfe:
            ch = (char)0x014b;
            break;
          case 0xff:
            ch = (char)0xad;
            break;
          #endregion

          default:
            if (b >= 0xc1 && b <= 0xcf && b != 0xc9 && b != 0xcc)
            {
              #region multi-byte characters
              byte b1 = b;
              if (offset >= bytes.Length)
              {
                break;
              }
              b = bytes[offset++];
              if (b == 0)
              {
                break;
              }

              switch (b1)
              {
                case 0xc1:
                  #region c1
                  switch (b)
                  {
                    case 0x41:
                      ch = (char)0xc0;
                      break;
                    case 0x45:
                      ch = (char)0xc8;
                      break;
                    case 0x49:
                      ch = (char)0xcc;
                      break;
                    case 0x4f:
                      ch = (char)0xd2;
                      break;
                    case 0x55:
                      ch = (char)0xd9;
                      break;
                    case 0x61:
                      ch = (char)0xe0;
                      break;
                    case 0x65:
                      ch = (char)0xe8;
                      break;
                    case 0x69:
                      ch = (char)0xec;
                      break;
                    case 0x6f:
                      ch = (char)0xf2;
                      break;
                    case 0x75:
                      ch = (char)0xf9;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xc2:
                  #region c2
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0xb4;
                      break;
                    case 0x41:
                      ch = (char)0xc1;
                      break;
                    case 0x43:
                      ch = (char)0x0106;
                      break;
                    case 0x45:
                      ch = (char)0xc9;
                      break;
                    case 0x49:
                      ch = (char)0xcd;
                      break;
                    case 0x4c:
                      ch = (char)0x0139;
                      break;
                    case 0x4e:
                      ch = (char)0x0143;
                      break;
                    case 0x4f:
                      ch = (char)0xd3;
                      break;
                    case 0x52:
                      ch = (char)0x0154;
                      break;
                    case 0x53:
                      ch = (char)0x015a;
                      break;
                    case 0x55:
                      ch = (char)0xda;
                      break;
                    case 0x59:
                      ch = (char)0xdd;
                      break;
                    case 0x5a:
                      ch = (char)0x0179;
                      break;
                    case 0x61:
                      ch = (char)0xe1;
                      break;
                    case 0x63:
                      ch = (char)0x0107;
                      break;
                    case 0x65:
                      ch = (char)0xe9;
                      break;
                    case 0x69:
                      ch = (char)0xed;
                      break;
                    case 0x6c:
                      ch = (char)0x013a;
                      break;
                    case 0x6e:
                      ch = (char)0x0144;
                      break;
                    case 0x6f:
                      ch = (char)0xf3;
                      break;
                    case 0x72:
                      ch = (char)0x0155;
                      break;
                    case 0x73:
                      ch = (char)0x015b;
                      break;
                    case 0x75:
                      ch = (char)0xfa;
                      break;
                    case 0x79:
                      ch = (char)0xfd;
                      break;
                    case 0x7a:
                      ch = (char)0x017a;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xc3:
                  #region c3
                  switch (b)
                  {
                    case 0x41:
                      ch = (char)0xc2;
                      break;
                    case 0x43:
                      ch = (char)0x0108;
                      break;
                    case 0x45:
                      ch = (char)0xca;
                      break;
                    case 0x47:
                      ch = (char)0x011c;
                      break;
                    case 0x48:
                      ch = (char)0x0124;
                      break;
                    case 0x49:
                      ch = (char)0xce;
                      break;
                    case 0x4a:
                      ch = (char)0x0134;
                      break;
                    case 0x4f:
                      ch = (char)0xd4;
                      break;
                    case 0x53:
                      ch = (char)0x015c;
                      break;
                    case 0x55:
                      ch = (char)0xdb;
                      break;
                    case 0x57:
                      ch = (char)0x0174;
                      break;
                    case 0x59:
                      ch = (char)0x0176;
                      break;
                    case 0x61:
                      ch = (char)0xe2;
                      break;
                    case 0x63:
                      ch = (char)0x0109;
                      break;
                    case 0x65:
                      ch = (char)0xea;
                      break;
                    case 0x67:
                      ch = (char)0x011d;
                      break;
                    case 0x68:
                      ch = (char)0x0125;
                      break;
                    case 0x69:
                      ch = (char)0xee;
                      break;
                    case 0x6a:
                      ch = (char)0x0135;
                      break;
                    case 0x6f:
                      ch = (char)0xf4;
                      break;
                    case 0x73:
                      ch = (char)0x015d;
                      break;
                    case 0x75:
                      ch = (char)0xfb;
                      break;
                    case 0x77:
                      ch = (char)0x175;
                      break;
                    case 0x79:
                      ch = (char)0x177;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xc4:
                  #region c4
                  switch (b)
                  {
                    case 0x41:
                      ch = (char)0xc3;
                      break;
                    case 0x49:
                      ch = (char)0x0128;
                      break;
                    case 0x4e:
                      ch = (char)0xd1;
                      break;
                    case 0x4f:
                      ch = (char)0xd5;
                      break;
                    case 0x55:
                      ch = (char)0x0168;
                      break;
                    case 0x61:
                      ch = (char)0xe3;
                      break;
                    case 0x69:
                      ch = (char)0x0129;
                      break;
                    case 0x6e:
                      ch = (char)0xf1;
                      break;
                    case 0x6f:
                      ch = (char)0xf5;
                      break;
                    case 0x75:
                      ch = (char)0x0169;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xc5:
                  #region c5
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0xaf;
                      break;
                    case 0x41:
                      ch = (char)0x100;
                      break;
                    case 0x45:
                      ch = (char)0x112;
                      break;
                    case 0x49:
                      ch = (char)0x012a;
                      break;
                    case 0x4f:
                      ch = (char)0x014c;
                      break;
                    case 0x55:
                      ch = (char)0x016a;
                      break;
                    case 0x61:
                      ch = (char)0x101;
                      break;
                    case 0x65:
                      ch = (char)0x113;
                      break;
                    case 0x69:
                      ch = (char)0x012b;
                      break;
                    case 0x6f:
                      ch = (char)0x014d;
                      break;
                    case 0x75:
                      ch = (char)0x016b;
                      break;
                    // unknown character --> fallback
                    default:
                      ch = (char)b;
                      break;
                  }
                  #endregion
                  break;
                case 0xc6:
                  #region c6
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0x02d8;
                      break;
                    case 0x41:
                      ch = (char)0x102;
                      break;
                    case 0x47:
                      ch = (char)0x011e;
                      break;
                    case 0x55:
                      ch = (char)0x016c;
                      break;
                    case 0x61:
                      ch = (char)0x103;
                      break;
                    case 0x67:
                      ch = (char)0x011f;
                      break;
                    case 0x75:
                      ch = (char)0x016d;
                      break;
                    // unknown character --> fallback
                    default:
                      ch = (char)b;
                      break;
                  }
                  #endregion
                  break;
                case 0xc7:
                  #region c7
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0x02d9;
                      break;
                    case 0x43:
                      ch = (char)0x010a;
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
                    case 0x5a:
                      ch = (char)0x017b;
                      break;
                    case 0x63:
                      ch = (char)0x010b;
                      break;
                    case 0x65:
                      ch = (char)0x117;
                      break;
                    case 0x67:
                      ch = (char)0x121;
                      break;
                    case 0x7a:
                      ch = (char)0x017c;
                      break;
                    // unknown character --> fallback
                    default:
                      ch = (char)b;
                      break;
                  }
                  #endregion
                  break;
                case 0xc8:
                  #region c8
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0xa8;
                      break;
                    case 0x41:
                      ch = (char)0xc4;
                      break;
                    case 0x45:
                      ch = (char)0xcb;
                      break;
                    case 0x49:
                      ch = (char)0xcf;
                      break;
                    case 0x4f:
                      ch = (char)0xd6;
                      break;
                    case 0x55:
                      ch = (char)0xdc;
                      break;
                    case 0x59:
                      ch = (char)0x178;
                      break;
                    case 0x61:
                      ch = (char)0xe4;
                      break;
                    case 0x65:
                      ch = (char)0xeb;
                      break;
                    case 0x69:
                      ch = (char)0xef;
                      break;
                    case 0x6f:
                      ch = (char)0xf6;
                      break;
                    case 0x75:
                      ch = (char)0xfc;
                      break;
                    case 0x79:
                      ch = (char)0xff;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xca:
                  #region ca
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0x02da;
                      break;
                    case 0x41:
                      ch = (char)0xc5;
                      break;
                    case 0x55:
                      ch = (char)0x016e;
                      break;
                    case 0x61:
                      ch = (char)0xe5;
                      break;
                    case 0x75:
                      ch = (char)0x016f;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xcb:
                  #region cb
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0xb8;
                      break;
                    case 0x43:
                      ch = (char)0xc7;
                      break;
                    case 0x47:
                      ch = (char)0x0122;
                      break;
                    case 0x4b:
                      ch = (char)0x136;
                      break;
                    case 0x4c:
                      ch = (char)0x013b;
                      break;
                    case 0x4e:
                      ch = (char)0x0145;
                      break;
                    case 0x52:
                      ch = (char)0x0156;
                      break;
                    case 0x53:
                      ch = (char)0x015e;
                      break;
                    case 0x54:
                      ch = (char)0x0162;
                      break;
                    case 0x63:
                      ch = (char)0xe7;
                      break;
                    case 0x67:
                      ch = (char)0x0123;
                      break;
                    case 0x6b:
                      ch = (char)0x0137;
                      break;
                    case 0x6c:
                      ch = (char)0x013c;
                      break;
                    case 0x6e:
                      ch = (char)0x0146;
                      break;
                    case 0x72:
                      ch = (char)0x0157;
                      break;
                    case 0x73:
                      ch = (char)0x015f;
                      break;
                    case 0x74:
                      ch = (char)0x0163;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xcd:
                  #region cd
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0x02dd;
                      break;
                    case 0x4f:
                      ch = (char)0x0150;
                      break;
                    case 0x55:
                      ch = (char)0x0170;
                      break;
                    case 0x6f:
                      ch = (char)0x0151;
                      break;
                    case 0x75:
                      ch = (char)0x0171;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xce:
                  #region ce
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0x02db;
                      break;
                    case 0x41:
                      ch = (char)0x0104;
                      break;
                    case 0x45:
                      ch = (char)0x0118;
                      break;
                    case 0x49:
                      ch = (char)0x012e;
                      break;
                    case 0x55:
                      ch = (char)0x0172;
                      break;
                    case 0x61:
                      ch = (char)0x0105;
                      break;
                    case 0x65:
                      ch = (char)0x0119;
                      break;
                    case 0x69:
                      ch = (char)0x012f;
                      break;
                    case 0x75:
                      ch = (char)0x0173;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
                case 0xcf:
                  #region cf
                  switch (b)
                  {
                    case 0x20:
                      ch = (char)0x02c7;
                      break;
                    case 0x43:
                      ch = (char)0x010c;
                      break;
                    case 0x44:
                      ch = (char)0x010e;
                      break;
                    case 0x45:
                      ch = (char)0x011a;
                      break;
                    case 0x4c:
                      ch = (char)0x013d;
                      break;
                    case 0x4e:
                      ch = (char)0x0147;
                      break;
                    case 0x52:
                      ch = (char)0x0158;
                      break;
                    case 0x53:
                      ch = (char)0x0160;
                      break;
                    case 0x54:
                      ch = (char)0x0164;
                      break;
                    case 0x5a:
                      ch = (char)0x017d;
                      break;
                    case 0x63:
                      ch = (char)0x010d;
                      break;
                    case 0x64:
                      ch = (char)0x010f;
                      break;
                    case 0x65:
                      ch = (char)0x011b;
                      break;
                    case 0x6c:
                      ch = (char)0x013e;
                      break;
                    case 0x6e:
                      ch = (char)0x0148;
                      break;
                    case 0x72:
                      ch = (char)0x0159;
                      break;
                    case 0x73:
                      ch = (char)0x0161;
                      break;
                    case 0x74:
                      ch = (char)0x0165;
                      break;
                    case 0x7a:
                      ch = (char)0x017e;
                      break;
                    default:
                      ch = (char)b;
                      break; // unknown character --> fallback
                  }
                  #endregion
                  break;
              }
              #endregion
            }
            else
            {
              // Everything else is the same.
              ch = (char)b;
            }
            break;
        }

        if (b == 0 || ch == 0)
        {
          break;
        }
        result.Append(ch);
      }
      return result.ToString();
    }
  }
}