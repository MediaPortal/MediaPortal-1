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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper
{
  /// <summary>
  /// This class can be used to assist debugging by providing functions to log the contents of buffers and arrays.
  /// </summary>
  public static class Dump
  {
    /// <summary>
    /// Check if a byte is a printable ASCII character. If not, return underscore.
    /// </summary>
    private static char ToSafeAscii(byte b)
    {
      if (b >= 32 && b <= 126)
      {
        return (char)b;
      }
      return '_';
    }

    /// <summary>
    /// Dump a read-only collection of bytes.
    /// </summary>
    /// <param name="sourceData">The source collection.</param>
    /// <param name="length">The number of bytes to dump. If <c>-1</c>, dump all available bytes starting at <paramref name="offset"/>.</param>
    /// <param name="offset">The offset/position at which to start the dump. Defaults to the first byte.</param>
    public static void DumpBinary(ReadOnlyCollection<byte> sourceData, int length = -1, int offset = 0)
    {
      if (sourceData == null)
      {
        Log.Warn("dump: asked to dump null read-only collection");
        return;
      }
      StringBuilder row = new StringBuilder();
      StringBuilder rowText = new StringBuilder();
      if (length < 0)
      {
        length = sourceData.Count - offset;
      }
      for (int position = offset; position < offset + length; position++)
      {
        if (position == offset || position % 0x10 == 0)
        {
          if (row.Length > 0)
          {
            Log.Debug(string.Format("{0}|{1}", row.ToString().PadRight(55, ' '),
                                            rowText.ToString().PadRight(16, ' ')));
          }
          rowText.Length = 0;
          row.Length = 0;
          row.AppendFormat("{0:X4}|", position);
        }
        row.AppendFormat("{0:X2} ", sourceData[position]); // the hex code
        rowText.Append(ToSafeAscii(sourceData[position])); // the ASCII char
      }
      if (row.Length > 0)
      {
        Log.Debug(string.Format("{0}|{1}", row.ToString().PadRight(55, ' '), rowText.ToString().PadRight(16, ' ')));
      }
    }

    /// <summary>
    /// Dump a byte array.
    /// </summary>
    /// <param name="sourceData">The array.</param>
    /// <param name="length">The number of bytes to dump. If <c>-1</c>, dump all available bytes starting at <paramref name="offset"/>.</param>
    /// <param name="offset">The offset/position at which to start the dump. Defaults to the first byte.</param>
    public static void DumpBinary(byte[] sourceData, int length = -1, int offset = 0)
    {
      if (sourceData == null)
      {
        Log.Warn("dump: asked to dump null array");
        return;
      }
      DumpBinary(new ReadOnlyCollection<byte>(sourceData), length, offset);
    }

    /// <summary>
    /// Dump a buffer.
    /// </summary>
    /// <param name="sourceData">The array.</param>
    /// <param name="length">The number of bytes to dump.</param>
    /// <param name="offset">The offset/position at which to start the dump. Defaults to the first byte.</param>
    public static void DumpBinary(IntPtr sourceData, int length, int offset = 0)
    {
      if (sourceData == IntPtr.Zero)
      {
        Log.Warn("dump: asked to dump unallocated buffer");
        return;
      }
      byte[] tmpBuffer = new byte[length];
      Marshal.Copy(IntPtr.Add(sourceData, offset), tmpBuffer, 0, length);
      DumpBinary(tmpBuffer);
    }
  }
}
