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

using System.IO;
using System.Text;

namespace Roger.ID3
{
  public class TagBuilder
  {
    private byte majorVersion;
    private byte minorVersion;

    private MemoryStream memoryStream;

    public TagBuilder(byte majorVersion, byte minorVersion)
    {
      this.majorVersion = majorVersion;
      this.minorVersion = minorVersion;

      memoryStream = new MemoryStream();
    }

    public void Append(object frameId, object frameValue)
    {
      Append((string) frameId, frameValue);
    }

    public void Append(string frameId, object frameValue)
    {
      byte[] frameIdBuffer = EncodeFrameId(frameId);
      byte[] frameValueBuffer = EncodeFrameValue(frameId, frameValue);

      // Frame ID
      memoryStream.Write(frameIdBuffer, 0, 4);

      // Frame Size
      int frameLength = frameValueBuffer.Length;
      WriteInt28(memoryStream, frameLength);

      // Flags (2 bytes)
      memoryStream.WriteByte(0);
      memoryStream.WriteByte(0);

      // Frame data
      memoryStream.Write(frameValueBuffer, 0, frameValueBuffer.Length);
    }

    private byte[] EncodeFrameId(string frameId)
    {
      byte[] frameIdBuffer = new byte[4];
      Encoding.ASCII.GetBytes(frameId, 0, 4, frameIdBuffer, 0);

      return frameIdBuffer;
    }

    private byte[] EncodeFrameValue(string frameId, object frameValue)
    {
      if (frameId[0] == 'T')
      {
        string valueString = (string) frameValue;

        // For now we'll use Latin1.
        Encoding encoding = Encoding.GetEncoding(1252);
        int bufferSize = 1 + encoding.GetByteCount(valueString);

        byte[] buffer = new byte[bufferSize];
        buffer[0] = 0; // Latin1 encoding.
        Encoding.UTF8.GetBytes(valueString, 0, valueString.Length, buffer, 1);

        return buffer;
      }
      else
      {
        return (byte[]) frameValue;
      }
    }

    public void WriteTo(Stream stream)
    {
      WriteMagic(stream);
      WriteVersion(stream);
      WriteFlags(stream);

      WriteInt28(stream, (int) memoryStream.Length);
      memoryStream.WriteTo(stream);
    }

    private void WriteMagic(Stream stream)
    {
      byte[] magic = Encoding.ASCII.GetBytes("ID3");
      stream.Write(magic, 0, magic.Length);
    }

    private void WriteVersion(Stream stream)
    {
      stream.WriteByte(majorVersion);
      stream.WriteByte(minorVersion);
    }

    private void WriteFlags(Stream stream)
    {
      stream.WriteByte(0);
    }

    private void WriteInt28(Stream stream, int n)
    {
      byte[] buffer = new byte[4];

      buffer[3] = (byte) (n & 0x7F);
      n >>= 7;
      buffer[2] = (byte) (n & 0x7F);
      n >>= 7;
      buffer[1] = (byte) (n & 0x7F);
      n >>= 7;
      buffer[0] = (byte) (n & 0x7F);

      stream.Write(buffer, 0, 4);
    }
  }
}