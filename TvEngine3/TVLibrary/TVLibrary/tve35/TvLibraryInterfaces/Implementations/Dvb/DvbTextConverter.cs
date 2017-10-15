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
      if (buffer == IntPtr.Zero || encodedByteCount == 0)
      {
        return string.Empty;
      }

      // Simplify offset handling.
      IntPtr ptr = IntPtr.Add(buffer, offset);

      // If we were told how many bytes were used to encode the string then
      // just decode/convert it.
      byte[] bytes;
      if (encodedByteCount > 0)
      {
        bytes = new byte[encodedByteCount];
        Marshal.Copy(ptr, bytes, 0, encodedByteCount);
        return Convert(bytes, encodedByteCount, 0, out decodedByteCount);
      }

      // Otherwise we have to carefully figure out how long the string is. This
      // requires us to know the encoding.
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
          // Next byte should be 0x00 [if DVB-compliant and the string source is
          // not TsWriter]. Skip it. It's never the NULL termination.
          decodedByteCount++;
        }
      }

      while (true)
      {
        b1 = Marshal.ReadByte(ptr, decodedByteCount++);
        if (isIso10646)
        {
          b2 = Marshal.ReadByte(ptr, decodedByteCount++);
        }
        if (b1 == 0 && b2 == 0)
        {
          break;
        }
      }

      bytes = new byte[decodedByteCount];
      Marshal.Copy(ptr, bytes, 0, decodedByteCount);
      return Convert(bytes, decodedByteCount, 0, out decodedByteCount);
    }

    /// <summary>
    /// Convert a DVB-encoded string in a byte array to Unicode.
    /// </summary>
    /// <param name="buffer">The byte array containing the string to convert.</param>
    /// <param name="encodedByteCount">The number of bytes in <paramref name="buffer"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="buffer"/> at which the string starts.</param>
    /// <returns>the converted string</returns>
    public static string Convert(byte[] buffer, int encodedByteCount = -1, int offset = 0)
    {
      int decodedByteCount;
      return Convert(buffer, encodedByteCount, offset, out decodedByteCount);
    }

    /// <summary>
    /// Convert a DVB-encoded string in a byte array to Unicode.
    /// </summary>
    /// <param name="buffer">The byte array containing the string to convert.</param>
    /// <param name="encodedByteCount">The number of bytes in <paramref name="buffer"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="buffer"/> at which the string starts.</param>
    /// <param name="decodedByteCount">The number of bytes actually read when decoding/converting the string.</param>
    /// <returns>the converted string</returns>
    public static string Convert(byte[] buffer, int encodedByteCount, int offset, out int decodedByteCount)
    {
      decodedByteCount = 0;
      if (buffer == null || buffer.Length == 0 || encodedByteCount == 0 || buffer.Length - offset == 0)
      {
        return string.Empty;
      }
      else if (buffer.Length <= offset || encodedByteCount > buffer.Length - offset)
      {
        Log.Info("DVB text: unexpected parameter values, buffer length = {0}, encoded byte count = {1}, offset = {2}", buffer.Length, encodedByteCount, offset);
        Dump.DumpBinary(buffer);
        return string.Empty;
      }

      // Select a compatible Windows code page.
      int codePage;
      int encodingIndicatorByteCount;
      string error;
      if (!DetermineCodePage(buffer, offset, out codePage, out encodingIndicatorByteCount, out error))
      {
        Log.Info("DVB text: {0}, buffer length = {1}, encoded byte count = {2}, offset = {3}", error, buffer.Length, encodedByteCount, offset);
        Dump.DumpBinary(buffer);
        return string.Empty;
      }

      bool isUnexpectedByteSequenceSeen = false;
      byte[] content;
      int contentByteCount;
      byte[] emphasizedContent;
      int emphasizedContentByteCount;
      if (encodedByteCount < 0)
      {
        encodedByteCount = buffer.Length - offset - encodingIndicatorByteCount;
      }
      if (codePage == 65001)
      {
        ProcessVariableBytePerCharEncoding(buffer, encodedByteCount, offset + encodingIndicatorByteCount, out decodedByteCount, out isUnexpectedByteSequenceSeen, out content, out contentByteCount, out emphasizedContent, out emphasizedContentByteCount);
      }
      else if (codePage == 1200)
      {
        ProcessTwoBytePerCharEncoding(buffer, encodedByteCount, offset + encodingIndicatorByteCount, out decodedByteCount, out isUnexpectedByteSequenceSeen, out content, out contentByteCount, out emphasizedContent, out emphasizedContentByteCount);
      }
      else
      {
        ProcessOneBytePerCharEncoding(buffer, encodedByteCount, offset + encodingIndicatorByteCount, out decodedByteCount, out isUnexpectedByteSequenceSeen, out content, out contentByteCount, out emphasizedContent, out emphasizedContentByteCount);
      }
      decodedByteCount += encodingIndicatorByteCount;

      try
      {
        switch (codePage)
        {
          case 20269:   // ISO/IEC 6937-1
          case 28600:   // ISO/IEC 8859-10
          case 28604:   // ISO/IEC 8859-14
            if (codePage == 20269)
            {
              return IsoIec6937ToUnicode(content, out isUnexpectedByteSequenceSeen);
            }
            if (codePage == 28600)
            {
              return IsoIec8859P10ToUnicode(content, out isUnexpectedByteSequenceSeen);
            }
            return IsoIec8859P14ToUnicode(content, out isUnexpectedByteSequenceSeen);
          case 28602:   // ISO/IEC 8859-12
            Log.Info("DVB text: selected ISO/IEC 8859-12 encoding does not exist, buffer length = {0}, encoded byte count = {1}, offset = {2}", buffer.Length, encodedByteCount, offset);
            Dump.DumpBinary(buffer);
            return string.Empty;
          default:
            return Encoding.GetEncoding(codePage).GetString(content, 0, contentByteCount);
        }
      }
      finally
      {
        if (isUnexpectedByteSequenceSeen)
        {
          Log.Info("DVB text: unexpected byte sequence seen, buffer length = {0}, encoded byte count = {1}, offset = {2}", buffer.Length, encodedByteCount, offset);
          Dump.DumpBinary(buffer);
        }
      }
    }

    private static bool DetermineCodePage(byte[] buffer, int offset, out int codePage, out int encodingIndicatorByteCount, out string error)
    {
      codePage = 20269;   // ISO/IEC 6937
      encodingIndicatorByteCount = 0;
      error = null;
      byte b = buffer[offset];
      if (b == 0 || b >= 0x20)
      {
        return true;
      }

      encodingIndicatorByteCount = 1;
      switch (b)
      {
        case 0x10:
          // Next byte should be 0x00 [if DVB-compliant and the string source
          // is not TsWriter]. Skip it. After that comes the actual indicator
          // byte.
          if (offset + 2 >= buffer.Length)
          {
            error = "unexpected end of byte sequence after three byte encoding indicator";
            return false;
          }
          encodingIndicatorByteCount = 3;
          b = buffer[offset + 2];
          if (b != 0 && b < 0x10) // ISO/IEC 8859-1..15
          {
            codePage = 28590 + b;
            break;
          }

          error = string.Format("unsupported three byte encoding indicator byte 0x{0:x}", b);
          return false;
        case 0x11:
          codePage = 1200;  // ISO/IEC 10646-1 little endian
          return true;
        case 0x12:
          codePage = 949;   // KSX1001-2004 / KSC5601-1987
          return true;
        case 0x13:
          codePage = 936;   // GB-2312-1980
          return true;
        case 0x14:
          codePage = 950;   // Big5
          return true;
        case 0x15:
          codePage = 65001; // UTF-8
          return true;
        default:
          if (b > 0xb)
          {
            error = string.Format("unsupported one byte encoding indicator byte 0x{0:x}", b);
            return false;
          }
          codePage = 28594 + b;  // ISO/IEC 8859-5..15
          break;
      }

      if (codePage == 28601)  // ISO/IEC 8859-11
      {
        // Windows doesn't directly support ISO/IEC 8859-11. Use compatible
        // code page 874, which is an extension of TIS-620 (TIS-620 is
        // compatible with ISO/IEC 8859-11).
        codePage = 874;
      }
      return true;
    }

    private static void ProcessOneBytePerCharEncoding(byte[] buffer, int encodedByteCount, int offset, out int decodedByteCount, out bool isUnexpectedByteSequenceSeen, out byte[] content, out int contentByteCount, out byte[] emphasizedContent, out int emphasizedContentByteCount)
    {
      decodedByteCount = 0;
      isUnexpectedByteSequenceSeen = false;
      content = new byte[encodedByteCount];
      contentByteCount = 0;
      emphasizedContent = new byte[encodedByteCount];
      emphasizedContentByteCount = 0;

      int initialOffset = offset;
      bool isEmphasisOn = false;
      int stop = offset + encodedByteCount;
      while (offset < stop)
      {
        byte b = buffer[offset++];
        decodedByteCount++;

        if (b == 0)
        {
          return;
        }
        if (b < 0x20 && b != 0xa && b != 0xd)
        {
          Log.Info("DVB text: unexpected byte/character in 1-byte-per-character string, c = 0x{0:x2}, offset = {1}", b, offset - 1 - initialOffset);
          isUnexpectedByteSequenceSeen = true;
          continue;
        }
        else if (b >= 0x80 && b <= 0x9f)
        {
          if (b == 0x86)
          {
            isEmphasisOn = true;
          }
          else if (b == 0x87)
          {
            isEmphasisOn = false;
          }
          else if (b == 0x8a)
          {
            // can't inject both CR and LF; choose LF
            b = 0xa;
          }
          else
          {
            Log.Info("DVB text: unexpected control character in 1-byte-per-character string, c = 0x{0:x2}, offset = {1}", b, offset - 1 - initialOffset);
            isUnexpectedByteSequenceSeen = true;
          }
          continue;
        }

        content[contentByteCount++] = b;
        if (isEmphasisOn)
        {
          emphasizedContent[emphasizedContentByteCount++] = b;
        }
      }
    }

    private static void ProcessTwoBytePerCharEncoding(byte[] buffer, int encodedByteCount, int offset, out int decodedByteCount, out bool isUnexpectedByteSequenceSeen, out byte[] content, out int contentByteCount, out byte[] emphasizedContent, out int emphasizedContentByteCount)
    {
      decodedByteCount = 0;
      isUnexpectedByteSequenceSeen = false;
      content = new byte[encodedByteCount];
      contentByteCount = 0;
      emphasizedContent = new byte[encodedByteCount];
      emphasizedContentByteCount = 0;

      int initialOffset = offset;
      bool isEmphasisOn = false;
      int stop = offset + encodedByteCount;
      while (offset + 1 < stop)
      {
        byte b1 = buffer[offset++];
        byte b2 = buffer[offset++];
        decodedByteCount += 2;
        if (b1 == 0)
        {
          if (b2 == 0)
          {
            return;
          }
          if (b2 < 0x20 && b2 != 0xa && b2 != 0xd)
          {
            Log.Info("DVB text: unexpected byte/character in 2-byte-per-character string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b2, offset - 2 - initialOffset);
            isUnexpectedByteSequenceSeen = true;
            continue;
          }
        }

        if (b1 == 0xe0 && b2 >= 0x80 && b2 <= 0x9f)
        {
          if (b2 == 0x86)
          {
            isEmphasisOn = true;
          }
          else if (b2 == 0x87)
          {
            isEmphasisOn = false;
          }
          else if (b2 == 0x8a)
          {
            b1 = 0xa;
            b2 = 0xd;
          }
          else
          {
            Log.Info("DVB text: unexpected control character in 2-byte-per-character string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b2, offset - 2 - initialOffset);
            isUnexpectedByteSequenceSeen = true;
          }
          continue;
        }

        content[contentByteCount++] = b1;
        content[contentByteCount++] = b2;
        if (isEmphasisOn)
        {
          emphasizedContent[emphasizedContentByteCount++] = b1;
          emphasizedContent[emphasizedContentByteCount++] = b2;
        }
      }
    }

    private static void ProcessVariableBytePerCharEncoding(byte[] buffer, int encodedByteCount, int offset, out int decodedByteCount, out bool isUnexpectedByteSequenceSeen, out byte[] content, out int contentByteCount, out byte[] emphasizedContent, out int emphasizedContentByteCount)
    {
      decodedByteCount = 0;
      isUnexpectedByteSequenceSeen = false;
      content = new byte[encodedByteCount];
      contentByteCount = 0;
      emphasizedContent = new byte[encodedByteCount];
      emphasizedContentByteCount = 0;

      int initialOffset = offset;
      bool isEmphasisOn = false;
      int stop = offset + encodedByteCount;
      byte[] cArray = new byte[4];
      while (offset < stop)
      {
        byte charByteCount = 0;
        byte charIndex = 0;
        uint c = 0;
        while (true)
        {
          if (offset >= stop)
          {
            return;
          }

          byte b = buffer[offset++];
          if (charByteCount == 0)
          {
            if ((b & 0xf8) == 0xf0)
            {
              charByteCount = 4;
            }
            else if ((b & 0xf0) == 0xe0)
            {
              charByteCount = 3;
            }
            else if ((b & 0xe0) == 0xc0)
            {
              charByteCount = 2;
            }
            else
            {
              charByteCount = 1;
            }
          }

          cArray[charIndex++] = b;
          c <<= 8;
          c |= b;
          if (charByteCount == charIndex)
          {
            break;
          }
        }

        decodedByteCount += charByteCount;
        if (c == 0)
        {
          return;
        }
        if (c < 0x20 && c != 0xa && c != 0xd)
        {
          Log.Info("DVB text: unexpected byte/character in variable-byte-per-character string, c = 0x{0:x}, offset = {1}", c, offset - charByteCount - initialOffset);
          isUnexpectedByteSequenceSeen = true;
          continue;
        }
        else if (c >= 0xee8280 && c <= 0xee829f)
        {
          if (c == 0xee8286)
          {
            isEmphasisOn = true;
          }
          else if (c == 0xee8287)
          {
            isEmphasisOn = false;
          }
          else if (c == 0xee828a)
          {
            c = 0x0d0a;
            cArray[0] = 0xd;
            cArray[1] = 0xa;
            charByteCount = 2;
          }
          else
          {
            Log.Info("DVB text: unexpected control character in variable-byte-per-character string, c = 0x{0:x}, offset = {1}", c, offset - charByteCount - initialOffset);
            isUnexpectedByteSequenceSeen = true;
          }
          continue;
        }

        Buffer.BlockCopy(cArray, 0, content, contentByteCount, charByteCount);
        contentByteCount += charByteCount;
        if (isEmphasisOn)
        {
          Buffer.BlockCopy(cArray, 0, emphasizedContent, emphasizedContentByteCount, charByteCount);
          emphasizedContentByteCount += charByteCount;
        }
      }
    }

    /// <summary>
    /// Convert ISO/IEC 8859-10 (Latin/Nordic) to Unicode.
    /// </summary>
    /// <remarks>
    /// Microsoft has not yet implemented a .NET decoder for codepage 28600.
    /// </remarks>
    private static string IsoIec8859P10ToUnicode(byte[] bytes, out bool isUnexpectedByteSequenceSeen)
    {
      isUnexpectedByteSequenceSeen = false;
      int offset = 0;
      StringBuilder result = new StringBuilder();
      foreach (byte b in bytes)
      {
        offset++;
        if (b == 0)
        {
          break;
        }

        char c = (char)0;
        switch (b)
        {
          case 0x0a:
            c = (char)b;        // LINE FEED
            break;
          case 0x0d:
            c = (char)b;        // CARRIAGE RETURN
            break;

          case 0xa0:
            c = (char)0x00a0;   // NO-BREAK SPACE
            break;
          case 0xa1:
            c = (char)0x0104;   // LATIN CAPITAL LETTER A WITH OGONEK
            break;
          case 0xa2:
            c = (char)0x0112;   // LATIN CAPITAL LETTER E WITH MACRON
            break;
          case 0xa3:
            c = (char)0x0122;   // LATIN CAPITAL LETTER G WITH CEDILLA
            break;
          case 0xa4:
            c = (char)0x012a;   // LATIN CAPITAL LETTER I WITH MACRON
            break;
          case 0xa5:
            c = (char)0x0128;   // LATIN CAPITAL LETTER I WITH TILDE
            break;
          case 0xa6:
            c = (char)0x0136;   // LATIN CAPITAL LETTER K WITH CEDILLA
            break;
          case 0xa7:
            c = (char)0x00a7;   // SECTION SIGN
            break;
          case 0xa8:
            c = (char)0x013b;   // LATIN CAPITAL LETTER L WITH CEDILLA
            break;
          case 0xa9:
            c = (char)0x0110;   // LATIN CAPITAL LETTER D WITH STROKE
            break;
          case 0xaa:
            c = (char)0x0160;   // LATIN CAPITAL LETTER S WITH CARON
            break;
          case 0xab:
            c = (char)0x0166;   // LATIN CAPITAL LETTER T WITH STROKE
            break;
          case 0xac:
            c = (char)0x017d;   // LATIN CAPITAL LETTER Z WITH CARON
            break;
          case 0xad:
            c = (char)0x00ad;   // SOFT HYPHEN
            break;
          case 0xae:
            c = (char)0x016a;   // LATIN CAPITAL LETTER U WITH MACRON
            break;
          case 0xaf:
            c = (char)0x014a;   // LATIN CAPITAL LETTER ENG
            break;
          case 0xb0:
            c = (char)0x00b0;   // DEGREE SIGN
            break;
          case 0xb1:
            c = (char)0x0105;   // LATIN SMALL LETTER A WITH OGONEK
            break;
          case 0xb2:
            c = (char)0x0113;   // LATIN SMALL LETTER E WITH MACRON
            break;
          case 0xb3:
            c = (char)0x0123;   // LATIN SMALL LETTER G WITH CEDILLA
            break;
          case 0xb4:
            c = (char)0x012b;   // LATIN SMALL LETTER I WITH MACRON
            break;
          case 0xb5:
            c = (char)0x0129;   // LATIN SMALL LETTER I WITH TILDE
            break;
          case 0xb6:
            c = (char)0x0137;   // LATIN SMALL LETTER K WITH CEDILLA
            break;
          case 0xb7:
            c = (char)0x00b7;   // MIDDLE DOT
            break;
          case 0xb8:
            c = (char)0x013c;   // LATIN SMALL LETTER L WITH CEDILLA
            break;
          case 0xb9:
            c = (char)0x0111;   // LATIN SMALL LETTER D WITH STROKE
            break;
          case 0xba:
            c = (char)0x0161;   // LATIN SMALL LETTER S WITH CARON
            break;
          case 0xbb:
            c = (char)0x0167;   // LATIN SMALL LETTER T WITH STROKE
            break;
          case 0xbc:
            c = (char)0x017e;   // LATIN SMALL LETTER Z WITH CARON
            break;
          case 0xbd:
            c = (char)0x2015;   // HORIZONTAL BAR
            break;
          case 0xbe:
            c = (char)0x016b;   // LATIN SMALL LETTER U WITH MACRON
            break;
          case 0xbf:
            c = (char)0x014b;   // LATIN SMALL LETTER ENG
            break;

          case 0xc0:
            c = (char)0x0100;   // LATIN CAPITAL LETTER A WITH MACRON
            break;
          case 0xc7:
            c = (char)0x012e;   // LATIN CAPITAL LETTER I WITH OGONEK
            break;
          case 0xc8:
            c = (char)0x010c;   // LATIN CAPITAL LETTER C WITH CARON
            break;
          case 0xca:
            c = (char)0x0118;   // LATIN CAPITAL LETTER E WITH OGONEK
            break;
          case 0xcc:
            c = (char)0x0116;   // LATIN CAPITAL LETTER E WITH DOT ABOVE
            break;

          case 0xd1:
            c = (char)0x0145;   // LATIN CAPITAL LETTER N WITH CEDILLA
            break;
          case 0xd2:
            c = (char)0x014c;   // LATIN CAPITAL LETTER O WITH MACRON
            break;
          case 0xd7:
            c = (char)0x0168;   // LATIN CAPITAL LETTER U WITH TILDE
            break;
          case 0xd9:
            c = (char)0x0172;   // LATIN CAPITAL LETTER U WITH OGONEK
            break;

          case 0xe0:
            c = (char)0x0101;   // LATIN SMALL LETTER A WITH MACRON
            break;
          case 0xe7:
            c = (char)0x012f;   // LATIN SMALL LETTER I WITH OGONEK
            break;
          case 0xe8:
            c = (char)0x010d;   // LATIN SMALL LETTER C WITH CARON
            break;
          case 0xea:
            c = (char)0x0119;   // LATIN SMALL LETTER E WITH OGONEK
            break;
          case 0xec:
            c = (char)0x0117;   // LATIN SMALL LETTER E WITH DOT ABOVE
            break;

          case 0xf1:
            c = (char)0x0146;   // LATIN SMALL LETTER N WITH CEDILLA
            break;
          case 0xf2:
            c = (char)0x014d;   // LATIN SMALL LETTER O WITH MACRON
            break;
          case 0xf7:
            c = (char)0x0169;   // LATIN SMALL LETTER U WITH TILDE
            break;
          case 0xf9:
            c = (char)0x0173;   // LATIN SMALL LETTER U WITH OGONEK
            break;
          default:
            if (b < 0x20 || (0x7f <= b && b <= 0x9f))
            {
              Log.Info("DVB text: unexpected byte/character in ISO/IEC 8859-10 string, c = 0x{0:x}, offset = {1}", b, offset);
              isUnexpectedByteSequenceSeen = true;
            }
            else
            {
              c = (char)b;
            }
            break;
        }

        if (c != 0)
        {
          result.Append(c);
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
    private static string IsoIec8859P14ToUnicode(byte[] bytes, out bool isUnexpectedByteSequenceSeen)
    {
      isUnexpectedByteSequenceSeen = false;
      int offset = 0;
      StringBuilder result = new StringBuilder();
      foreach (byte b in bytes)
      {
        offset++;
        if (b == 0)
        {
          break;
        }

        char c = (char)0;
        switch (b)
        {
          case 0x0a:
            c = (char)b;        // LINE FEED
            break;
          case 0x0d:
            c = (char)b;        // CARRIAGE RETURN
            break;

          case 0xa0:
            c = (char)0x00a0;   // NO-BREAK SPACE
            break;
          case 0xa1:
            c = (char)0x1e02;   // LATIN CAPITAL LETTER B WITH DOT ABOVE
            break;
          case 0xa2:
            c = (char)0x1e03;   // LATIN SMALL LETTER B WITH DOT ABOVE
            break;
          case 0xa3:
            c = (char)0x00a3;   // POUND SIGN
            break;
          case 0xa4:
            c = (char)0x010a;   // LATIN CAPITAL LETTER C WITH DOT ABOVE
            break;
          case 0xa5:
            c = (char)0x010b;   // LATIN SMALL LETTER C WITH DOT ABOVE
            break;
          case 0xa6:
            c = (char)0x1e0a;   // LATIN CAPITAL LETTER D WITH DOT ABOVE
            break;
          case 0xa7:
            c = (char)0x00a7;   // SECTION SIGN
            break;
          case 0xa8:
            c = (char)0x1e80;   // LATIN CAPITAL LETTER W WITH GRAVE
            break;
          case 0xa9:
            c = (char)0x00a9;   // COPYRIGHT SIGN
            break;
          case 0xaa:
            c = (char)0x1e82;   // LATIN CAPITAL LETTER W WITH ACUTE
            break;
          case 0xab:
            c = (char)0x1e0b;   // LATIN SMALL LETTER D WITH DOT ABOVE
            break;
          case 0xac:
            c = (char)0x1ef2;   // LATIN CAPITAL LETTER Y WITH GRAVE
            break;
          case 0xad:
            c = (char)0x00ad;   // SOFT HYPHEN
            break;
          case 0xae:
            c = (char)0x00ae;   // REGISTERED SIGN
            break;
          case 0xaf:
            c = (char)0x0178;   // LATIN CAPITAL LETTER Y WITH DIAERESIS
            break;
          case 0xb0:
            c = (char)0x1e1e;   // LATIN CAPITAL LETTER F WITH DOT ABOVE
            break;
          case 0xb1:
            c = (char)0x1e1f;   // LATIN SMALL LETTER F WITH DOT ABOVE
            break;
          case 0xb2:
            c = (char)0x0120;   // LATIN CAPITAL LETTER G WITH DOT ABOVE
            break;
          case 0xb3:
            c = (char)0x0121;   // LATIN SMALL LETTER G WITH DOT ABOVE
            break;
          case 0xb4:
            c = (char)0x1e40;   // LATIN CAPITAL LETTER M WITH DOT ABOVE
            break;
          case 0xb5:
            c = (char)0x1e41;   // LATIN SMALL LETTER M WITH DOT ABOVE
            break;
          case 0xb6:
            c = (char)0x00b6;   // PILCROW SIGN
            break;
          case 0xb7:
            c = (char)0x1e56;   // LATIN CAPITAL LETTER P WITH DOT ABOVE
            break;
          case 0xb8:
            c = (char)0x1e81;   // LATIN SMALL LETTER W WITH GRAVE
            break;
          case 0xb9:
            c = (char)0x1e57;   // LATIN SMALL LETTER P WITH DOT ABOVE
            break;
          case 0xba:
            c = (char)0x1e83;   // LATIN SMALL LETTER W WITH ACUTE
            break;
          case 0xbb:
            c = (char)0x1e60;   // LATIN CAPITAL LETTER S WITH DOT ABOVE
            break;
          case 0xbc:
            c = (char)0x1ef3;   // LATIN SMALL LETTER Y WITH GRAVE
            break;
          case 0xbd:
            c = (char)0x1e84;   // LATIN CAPITAL LETTER W WITH DIAERESIS
            break;
          case 0xbe:
            c = (char)0x1e85;   // LATIN SMALL LETTER W WITH DIAERESIS
            break;
          case 0xbf:
            c = (char)0x1e61;   // LATIN SMALL LETTER S WITH DOT ABOVE
            break;

          case 0xd0:
            c = (char)0x0174;   // LATIN CAPITAL LETTER W WITH CIRCUMFLEX
            break;
          case 0xd7:
            c = (char)0x1e6a;   // LATIN CAPITAL LETTER T WITH DOT ABOVE
            break;
          case 0xde:
            c = (char)0x0176;   // LATIN CAPITAL LETTER Y WITH CIRCUMFLEX
            break;
          case 0xf0:
            c = (char)0x0175;   // LATIN SMALL LETTER W WITH CIRCUMFLEX
            break;
          case 0xf7:
            c = (char)0x1e6b;   // LATIN SMALL LETTER T WITH DOT ABOVE
            break;
          case 0xfe:
            c = (char)0x0177;   // LATIN SMALL LETTER Y WITH CIRCUMFLEX
            break;
          default:
            if (b < 0x20 || (0x7f <= b && b <= 0x9f))
            {
              Log.Info("DVB text: unexpected byte/character in ISO/IEC 8859-14 string, c = 0x{0:x}, offset = {1}", b, offset);
              isUnexpectedByteSequenceSeen = true;
            }
            else
            {
              c = (char)b;
            }
            break;
        }

        if (c != 0)
        {
          result.Append(c);
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
    private static string IsoIec6937ToUnicode(byte[] bytes, out bool isUnexpectedByteSequenceSeen)
    {
      isUnexpectedByteSequenceSeen = false;
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

        char c = (char)0;
        switch (b)
        {
          #region single byte characters
          case 0x0a:
            c = (char)b;
            break;
          case 0x0d:
            c = (char)b;
            break;
          case 0xa4:
            c = (char)0x20ac;  // euro sign - the only difference to ISO/IEC 6937-1
            break;
          case 0xa8:
            c = (char)0x00a4;
            break;
          case 0xa9:
            c = (char)0x2018;
            break;
          case 0xaa:
            c = (char)0x201c;
            break;
          case 0xac:
            c = (char)0x2190;
            break;
          case 0xad:
            c = (char)0x2191;
            break;
          case 0xae:
            c = (char)0x2192;
            break;
          case 0xaf:
            c = (char)0x2193;
            break;
          case 0xb4:
            c = (char)0x00d7;
            break;
          case 0xb8:
            c = (char)0x00f7;
            break;
          case 0xb9:
            c = (char)0x2019;
            break;
          case 0xba:
            c = (char)0x201d;
            break;
          case 0xd0:
            c = (char)0x2015;
            break;
          case 0xd1:
            c = (char)0xb9;
            break;
          case 0xd2:
            c = (char)0xae;
            break;
          case 0xd3:
            c = (char)0xa9;
            break;
          case 0xd4:
            c = (char)0x2122;
            break;
          case 0xd5:
            c = (char)0x266a;
            break;
          case 0xd6:
            c = (char)0xac;
            break;
          case 0xd7:
            c = (char)0xa6;
            break;
          case 0xdc:
            c = (char)0x215b;
            break;
          case 0xdd:
            c = (char)0x215c;
            break;
          case 0xde:
            c = (char)0x215d;
            break;
          case 0xdf:
            c = (char)0x215e;
            break;
          case 0xe0:
            c = (char)0x2126;
            break;
          case 0xe1:
            c = (char)0xc6;
            break;
          case 0xe2:
            c = (char)0x0110;
            break;
          case 0xe3:
            c = (char)0xaa;
            break;
          case 0xe4:
            c = (char)0x0126;
            break;
          case 0xe6:
            c = (char)0x0132;
            break;
          case 0xe7:
            c = (char)0x013f;
            break;
          case 0xe8:
            c = (char)0x0141;
            break;
          case 0xe9:
            c = (char)0xd8;
            break;
          case 0xea:
            c = (char)0x0152;
            break;
          case 0xeb:
            c = (char)0xba;
            break;
          case 0xec:
            c = (char)0xde;
            break;
          case 0xed:
            c = (char)0x0166;
            break;
          case 0xee:
            c = (char)0x014a;
            break;
          case 0xef:
            c = (char)0x0149;
            break;
          case 0xf0:
            c = (char)0x0138;
            break;
          case 0xf1:
            c = (char)0xe6;
            break;
          case 0xf2:
            c = (char)0x0111;
            break;
          case 0xf3:
            c = (char)0xf0;
            break;
          case 0xf4:
            c = (char)0x0127;
            break;
          case 0xf5:
            c = (char)0x0131;
            break;
          case 0xf6:
            c = (char)0x0133;
            break;
          case 0xf7:
            c = (char)0x0140;
            break;
          case 0xf8:
            c = (char)0x0142;
            break;
          case 0xf9:
            c = (char)0xf8;
            break;
          case 0xfa:
            c = (char)0x0153;
            break;
          case 0xfb:
            c = (char)0xdf;
            break;
          case 0xfc:
            c = (char)0xfe;
            break;
          case 0xfd:
            c = (char)0x0167;
            break;
          case 0xfe:
            c = (char)0x014b;
            break;
          case 0xff:
            c = (char)0xad;
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
                      c = (char)0xc0;
                      break;
                    case 0x45:
                      c = (char)0xc8;
                      break;
                    case 0x49:
                      c = (char)0xcc;
                      break;
                    case 0x4f:
                      c = (char)0xd2;
                      break;
                    case 0x55:
                      c = (char)0xd9;
                      break;
                    case 0x61:
                      c = (char)0xe0;
                      break;
                    case 0x65:
                      c = (char)0xe8;
                      break;
                    case 0x69:
                      c = (char)0xec;
                      break;
                    case 0x6f:
                      c = (char)0xf2;
                      break;
                    case 0x75:
                      c = (char)0xf9;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xc2:
                  #region c2
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0xb4;
                      break;
                    case 0x41:
                      c = (char)0xc1;
                      break;
                    case 0x43:
                      c = (char)0x0106;
                      break;
                    case 0x45:
                      c = (char)0xc9;
                      break;
                    case 0x49:
                      c = (char)0xcd;
                      break;
                    case 0x4c:
                      c = (char)0x0139;
                      break;
                    case 0x4e:
                      c = (char)0x0143;
                      break;
                    case 0x4f:
                      c = (char)0xd3;
                      break;
                    case 0x52:
                      c = (char)0x0154;
                      break;
                    case 0x53:
                      c = (char)0x015a;
                      break;
                    case 0x55:
                      c = (char)0xda;
                      break;
                    case 0x59:
                      c = (char)0xdd;
                      break;
                    case 0x5a:
                      c = (char)0x0179;
                      break;
                    case 0x61:
                      c = (char)0xe1;
                      break;
                    case 0x63:
                      c = (char)0x0107;
                      break;
                    case 0x65:
                      c = (char)0xe9;
                      break;
                    case 0x69:
                      c = (char)0xed;
                      break;
                    case 0x6c:
                      c = (char)0x013a;
                      break;
                    case 0x6e:
                      c = (char)0x0144;
                      break;
                    case 0x6f:
                      c = (char)0xf3;
                      break;
                    case 0x72:
                      c = (char)0x0155;
                      break;
                    case 0x73:
                      c = (char)0x015b;
                      break;
                    case 0x75:
                      c = (char)0xfa;
                      break;
                    case 0x79:
                      c = (char)0xfd;
                      break;
                    case 0x7a:
                      c = (char)0x017a;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xc3:
                  #region c3
                  switch (b)
                  {
                    case 0x41:
                      c = (char)0xc2;
                      break;
                    case 0x43:
                      c = (char)0x0108;
                      break;
                    case 0x45:
                      c = (char)0xca;
                      break;
                    case 0x47:
                      c = (char)0x011c;
                      break;
                    case 0x48:
                      c = (char)0x0124;
                      break;
                    case 0x49:
                      c = (char)0xce;
                      break;
                    case 0x4a:
                      c = (char)0x0134;
                      break;
                    case 0x4f:
                      c = (char)0xd4;
                      break;
                    case 0x53:
                      c = (char)0x015c;
                      break;
                    case 0x55:
                      c = (char)0xdb;
                      break;
                    case 0x57:
                      c = (char)0x0174;
                      break;
                    case 0x59:
                      c = (char)0x0176;
                      break;
                    case 0x61:
                      c = (char)0xe2;
                      break;
                    case 0x63:
                      c = (char)0x0109;
                      break;
                    case 0x65:
                      c = (char)0xea;
                      break;
                    case 0x67:
                      c = (char)0x011d;
                      break;
                    case 0x68:
                      c = (char)0x0125;
                      break;
                    case 0x69:
                      c = (char)0xee;
                      break;
                    case 0x6a:
                      c = (char)0x0135;
                      break;
                    case 0x6f:
                      c = (char)0xf4;
                      break;
                    case 0x73:
                      c = (char)0x015d;
                      break;
                    case 0x75:
                      c = (char)0xfb;
                      break;
                    case 0x77:
                      c = (char)0x175;
                      break;
                    case 0x79:
                      c = (char)0x177;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xc4:
                  #region c4
                  switch (b)
                  {
                    case 0x41:
                      c = (char)0xc3;
                      break;
                    case 0x49:
                      c = (char)0x0128;
                      break;
                    case 0x4e:
                      c = (char)0xd1;
                      break;
                    case 0x4f:
                      c = (char)0xd5;
                      break;
                    case 0x55:
                      c = (char)0x0168;
                      break;
                    case 0x61:
                      c = (char)0xe3;
                      break;
                    case 0x69:
                      c = (char)0x0129;
                      break;
                    case 0x6e:
                      c = (char)0xf1;
                      break;
                    case 0x6f:
                      c = (char)0xf5;
                      break;
                    case 0x75:
                      c = (char)0x0169;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xc5:
                  #region c5
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0xaf;
                      break;
                    case 0x41:
                      c = (char)0x100;
                      break;
                    case 0x45:
                      c = (char)0x112;
                      break;
                    case 0x49:
                      c = (char)0x012a;
                      break;
                    case 0x4f:
                      c = (char)0x014c;
                      break;
                    case 0x55:
                      c = (char)0x016a;
                      break;
                    case 0x61:
                      c = (char)0x101;
                      break;
                    case 0x65:
                      c = (char)0x113;
                      break;
                    case 0x69:
                      c = (char)0x012b;
                      break;
                    case 0x6f:
                      c = (char)0x014d;
                      break;
                    case 0x75:
                      c = (char)0x016b;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xc6:
                  #region c6
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0x02d8;
                      break;
                    case 0x41:
                      c = (char)0x102;
                      break;
                    case 0x47:
                      c = (char)0x011e;
                      break;
                    case 0x55:
                      c = (char)0x016c;
                      break;
                    case 0x61:
                      c = (char)0x103;
                      break;
                    case 0x67:
                      c = (char)0x011f;
                      break;
                    case 0x75:
                      c = (char)0x016d;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xc7:
                  #region c7
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0x02d9;
                      break;
                    case 0x43:
                      c = (char)0x010a;
                      break;
                    case 0x45:
                      c = (char)0x116;
                      break;
                    case 0x47:
                      c = (char)0x120;
                      break;
                    case 0x49:
                      c = (char)0x130;
                      break;
                    case 0x5a:
                      c = (char)0x017b;
                      break;
                    case 0x63:
                      c = (char)0x010b;
                      break;
                    case 0x65:
                      c = (char)0x117;
                      break;
                    case 0x67:
                      c = (char)0x121;
                      break;
                    case 0x7a:
                      c = (char)0x017c;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xc8:
                  #region c8
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0xa8;
                      break;
                    case 0x41:
                      c = (char)0xc4;
                      break;
                    case 0x45:
                      c = (char)0xcb;
                      break;
                    case 0x49:
                      c = (char)0xcf;
                      break;
                    case 0x4f:
                      c = (char)0xd6;
                      break;
                    case 0x55:
                      c = (char)0xdc;
                      break;
                    case 0x59:
                      c = (char)0x178;
                      break;
                    case 0x61:
                      c = (char)0xe4;
                      break;
                    case 0x65:
                      c = (char)0xeb;
                      break;
                    case 0x69:
                      c = (char)0xef;
                      break;
                    case 0x6f:
                      c = (char)0xf6;
                      break;
                    case 0x75:
                      c = (char)0xfc;
                      break;
                    case 0x79:
                      c = (char)0xff;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xca:
                  #region ca
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0x02da;
                      break;
                    case 0x41:
                      c = (char)0xc5;
                      break;
                    case 0x55:
                      c = (char)0x016e;
                      break;
                    case 0x61:
                      c = (char)0xe5;
                      break;
                    case 0x75:
                      c = (char)0x016f;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xcb:
                  #region cb
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0xb8;
                      break;
                    case 0x43:
                      c = (char)0xc7;
                      break;
                    case 0x47:
                      c = (char)0x0122;
                      break;
                    case 0x4b:
                      c = (char)0x136;
                      break;
                    case 0x4c:
                      c = (char)0x013b;
                      break;
                    case 0x4e:
                      c = (char)0x0145;
                      break;
                    case 0x52:
                      c = (char)0x0156;
                      break;
                    case 0x53:
                      c = (char)0x015e;
                      break;
                    case 0x54:
                      c = (char)0x0162;
                      break;
                    case 0x63:
                      c = (char)0xe7;
                      break;
                    case 0x67:
                      c = (char)0x0123;
                      break;
                    case 0x6b:
                      c = (char)0x0137;
                      break;
                    case 0x6c:
                      c = (char)0x013c;
                      break;
                    case 0x6e:
                      c = (char)0x0146;
                      break;
                    case 0x72:
                      c = (char)0x0157;
                      break;
                    case 0x73:
                      c = (char)0x015f;
                      break;
                    case 0x74:
                      c = (char)0x0163;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xcd:
                  #region cd
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0x02dd;
                      break;
                    case 0x4f:
                      c = (char)0x0150;
                      break;
                    case 0x55:
                      c = (char)0x0170;
                      break;
                    case 0x6f:
                      c = (char)0x0151;
                      break;
                    case 0x75:
                      c = (char)0x0171;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xce:
                  #region ce
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0x02db;
                      break;
                    case 0x41:
                      c = (char)0x0104;
                      break;
                    case 0x45:
                      c = (char)0x0118;
                      break;
                    case 0x49:
                      c = (char)0x012e;
                      break;
                    case 0x55:
                      c = (char)0x0172;
                      break;
                    case 0x61:
                      c = (char)0x0105;
                      break;
                    case 0x65:
                      c = (char)0x0119;
                      break;
                    case 0x69:
                      c = (char)0x012f;
                      break;
                    case 0x75:
                      c = (char)0x0173;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
                case 0xcf:
                  #region cf
                  switch (b)
                  {
                    case 0x20:
                      c = (char)0x02c7;
                      break;
                    case 0x43:
                      c = (char)0x010c;
                      break;
                    case 0x44:
                      c = (char)0x010e;
                      break;
                    case 0x45:
                      c = (char)0x011a;
                      break;
                    case 0x4c:
                      c = (char)0x013d;
                      break;
                    case 0x4e:
                      c = (char)0x0147;
                      break;
                    case 0x52:
                      c = (char)0x0158;
                      break;
                    case 0x53:
                      c = (char)0x0160;
                      break;
                    case 0x54:
                      c = (char)0x0164;
                      break;
                    case 0x5a:
                      c = (char)0x017d;
                      break;
                    case 0x63:
                      c = (char)0x010d;
                      break;
                    case 0x64:
                      c = (char)0x010f;
                      break;
                    case 0x65:
                      c = (char)0x011b;
                      break;
                    case 0x6c:
                      c = (char)0x013e;
                      break;
                    case 0x6e:
                      c = (char)0x0148;
                      break;
                    case 0x72:
                      c = (char)0x0159;
                      break;
                    case 0x73:
                      c = (char)0x0161;
                      break;
                    case 0x74:
                      c = (char)0x0165;
                      break;
                    case 0x7a:
                      c = (char)0x017e;
                      break;
                    default:
                      Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x2}{1:x2}, offset = {2}", b1, b, offset);
                      isUnexpectedByteSequenceSeen = true;
                      break;
                  }
                  #endregion
                  break;
              }
              #endregion
            }
            else if (b < 0x20 || b > 0x7e)
            {
              Log.Info("DVB text: unexpected byte/character in ISO/IEC 6937-1 string, c = 0x{0:x}, offset = {1}", b, offset);
              isUnexpectedByteSequenceSeen = true;
            }
            else
            {
              c = (char)b;
            }
            break;
        }

        if (c != 0)
        {
          result.Append(c);
        }
      }
      return result.ToString();
    }
  }
}