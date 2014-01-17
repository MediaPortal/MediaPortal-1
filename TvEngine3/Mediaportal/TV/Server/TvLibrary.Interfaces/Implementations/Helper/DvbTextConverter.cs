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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper
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
    /// <param name="encodedByteCount">The number of bytes in <paramref name="stringBytes"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="stringBytes"/> at which the string starts.</param>
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
    /// <param name="encodedByteCount">The number of bytes in <paramref name="stringBytes"/> used to encode the string, if known.</param>
    /// <param name="offset">The offset in <paramref name="stringBytes"/> at which the string starts.</param>
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
      byte c1 = Marshal.ReadByte(ptr, 0);
      byte c2 = 0;
      bool isIso10646 = false;
      if (c1 < 0x20)
      {
        decodedByteCount++;
        if (c1 == 0)
        {
          return string.Empty;
        }
        if (c1 == 0x11)
        {
          isIso10646 = true;
        }
        else if (c1 == 0x10)
        {
          c1 = Marshal.ReadByte(ptr, decodedByteCount++);
          if (c1 == 0)
          {
            c1 = Marshal.ReadByte(ptr, decodedByteCount++);
            if (c1 == 0)
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
          c1 = Marshal.ReadByte(ptr, decodedByteCount);
          c2 = Marshal.ReadByte(ptr, decodedByteCount + 1);
          if (c1 == 0 && c2 == 0)
          {
            nullTerminationByteCount = 2;
            break;
          }
          decodedByteCount += 2;
        }
        else
        {
          c1 = Marshal.ReadByte(ptr, decodedByteCount);
          if (c1 == 0)
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
      if (stringBytes == null)
      {
        return string.Empty;
      }
      else if (stringBytes.Length <= offset || encodedByteCount > stringBytes.Length - offset)
      {
        Log.Warn("DVB text: unexpected parameter values, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
        Dump.DumpBinary(stringBytes);
        return string.Empty;
      }

      // Figure out the encoding used.
      int len = stringBytes.Length;
      int encoding = 20269; // ISO-6937
      int encodingByteCount = 0;
      byte c1 = stringBytes[offset];
      byte c2 = 0;
      try
      {
        if (c1 < 0x20)
        {
          encodingByteCount++;
          switch (c1)
          {
            case 0x00:
              return string.Empty;
            case 0x01:
              encoding = 28595; // ISO-8859-5
              break;
            case 0x02:
              encoding = 28596; // ISO-8859-6
              break;
            case 0x03:
              encoding = 28597; // ISO-8859-7
              break;
            case 0x04:
              encoding = 28598; // ISO-8859-8
              break;
            case 0x05:
              encoding = 28599; // ISO-8859-9
              break;
            //case 0x06: encoding = ; // ISO-8859-10
            //break;
            case 0x07:
              encoding = 874; // ISO-8859-11
              break;
            //case 0x08: encoding = ; // ISO-8859-12
            //break;
            case 0x09:
              encoding = 28603; // ISO-8859-13
              break;
            //case 0x0A: encoding = ; // ISO-8859-14
            //break;
            case 0x0B:
              encoding = 28605; // ISO-8859-15
              break;
            case 0x10:
              {
                // This code is very intentional. TsWriter removes the second
                // encoding byte to avoid premature NULL termination. However,
                // this function is also used to process DVB text originating
                // from CI/CAMs. Do NOT change unless you know what you are
                // doing!
                if (len <= offset + encodingByteCount)
                {
                  Log.Warn("DVB text: unexpected end of byte sequence after three byte encoding indicator, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
                  Dump.DumpBinary(stringBytes);
                  return string.Empty;
                }
                c1 = stringBytes[offset + encodingByteCount++];
                if (c1 == 0)
                {
                  if (len <= offset + encodingByteCount)
                  {
                    Log.Warn("DVB text: unexpected end of byte sequence after three byte encoding reserved byte, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
                    Dump.DumpBinary(stringBytes);
                    return string.Empty;
                  }
                  c1 = stringBytes[offset + encodingByteCount++];
                }
                switch (c1)
                {
                  case 0x00:
                    Log.Warn("DVB text: unexpected end of byte sequence after three byte encoding byte, byte count = {0}, encoded byte count = {1}, offset = {2}", stringBytes.Length, encodedByteCount, offset);
                    Dump.DumpBinary(stringBytes);
                    return string.Empty;
                  case 0x01:
                    encoding = 28591; // ISO-8859-1
                    break;
                  case 0x02:
                    encoding = 28592; // ISO-8859-2
                    break;
                  case 0x03:
                    encoding = 28593; // ISO-8859-3
                    break;
                  case 0x04:
                    encoding = 28594; // ISO-8859-4
                    break;
                  case 0x05:
                    encoding = 28595; // ISO-8859-5
                    break;
                  case 0x06:
                    encoding = 28596; // ISO-8859-6
                    break;
                  case 0x07:
                    encoding = 28597; // ISO-8859-7
                    break;
                  case 0x08:
                    encoding = 28598; // ISO-8859-8
                    break;
                  case 0x09:
                    encoding = 28599; // ISO-8859-9
                    break;
                  //case 0x0A: encoding = ; // ISO-8859-10
                  //break;
                  case 0x0B:
                    encoding = 874; // ISO-8859-11
                    break;
                  //case 0x0C: encoding = ; //ISO-8859-12
                  //break;
                  case 0x0D:
                    encoding = 28591; // ISO-8859-13
                    break;
                  //case 0x0E: encoding = ; // ISO-8859-14
                  //break;
                  case 0x0F:
                    encoding = 28591; // ISO-8859-15
                    break;
                  default:
                    Log.Warn("DVB text: unsupported three byte encoding byte 0x{0:x}, byte count = {1}, encoded byte count = {2}, offset = {3}", c1, stringBytes.Length, encodedByteCount, offset);
                    Dump.DumpBinary(stringBytes);
                    return string.Empty;
                }
                break;
              }
            case 0x11:
              encoding = 1200; // ISO/IEC 10646-1
              break;
            case 0x12:
              encoding = 949; // KSC5601-1987
              break;
            case 0x13:
              encoding = 936; // GB-2312-1980
              break;
            case 0x14:
              encoding = 950; // Big5
              break;
            case 0x15:
              encoding = 65001; // UTF-8
              break;
            default:
              Log.Warn("DVB text: unsupported one byte encoding byte 0x{0:x}, byte count = {1}, encoded byte count = {2}, offset = {3}", c1, stringBytes.Length, encodedByteCount, offset);
              Dump.DumpBinary(stringBytes);
              return string.Empty;
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
        while (decodedByteCount < len)
        {
          if (encoding == 1200)
          {
            if (decodedByteCount >= len - 1)
            {
              break;
            }
            c1 = stringBytes[decodedByteCount++];
            c2 = stringBytes[decodedByteCount++];
            if (c1 == 0 && c2 == 0)
            {
              nullTerminationByteCount = 2;
              break;
            }
          }
          else
          {
            c1 = stringBytes[decodedByteCount++];
            if (c1 == 0)
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

      return Encoding.GetEncoding(encoding).GetString(stringBytes, offset + encodingByteCount, decodedByteCount - encodingByteCount - nullTerminationByteCount);
    }
  }
}