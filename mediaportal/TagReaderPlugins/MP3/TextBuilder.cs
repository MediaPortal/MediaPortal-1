/* 
 *	Copyright (C) 2005 Team MediaPortal
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

// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Text;
using System.IO;

namespace id3
{
	// ASCII - ISO-8859-1, UTF16 - Unicode with BOM, UTF16BE - BigEndian Unicode without BOM, UTF8 - encoded Unicode
	public enum TextCode:byte { ASCII = 0x00, UTF16 = 0x01, UTF16BE = 0x02, UTF8 = 0x03 };

	/// <summary>
	/// Summary description for ParseString.
	/// </summary>
	public class TextBuilder
	{
		public static string ReadText(byte[]frame,ref int index,TextCode code)
		{
			switch(code)
			{
				case TextCode.ASCII:
				{
					return ReadASCII(frame,ref index);
				}
				case TextCode.UTF16:
				{
					return ReadUTF16(frame,ref index);
				}
				case TextCode.UTF16BE:
				{
					return ReadUTF16BE(frame,ref index);
				}
				case TextCode.UTF8:
				{
					return ReadUTF8(frame,ref index);
				}
				default:
				{
					throw new Exception("Invalid string type");
				}
			}
		}

		public static string ReadTextEnd(byte[]frame, int index, TextCode code)
		{
			switch(code)
			{
				case TextCode.ASCII:
				{
					return ReadASCIIEnd(frame,index);
				}
				case TextCode.UTF16:
				{
					return ReadUTF16End(frame,index);
				}
				case TextCode.UTF16BE:
				{
					return ReadUTF16BEEnd(frame,index);
				}
				case TextCode.UTF8:
				{
					return ReadUTF8End(frame,index);
				}
				default:
				{
					throw new Exception("Invalid string type");
				}
			}
		}

		public static string ReadASCII(byte[] frame,ref int index)
		{
			string text = null;
			int count = Memory.FindByte(frame,0,index);
			if(count == -1)
			{
				throw new Exception("Invalid Frame");
			}
			if(count > 0)
			{
        Encoding encoding = Encoding.Default;// Encoding.GetEncoding(1252); // Should be ASCII
				text = encoding.GetString(frame,index,count);
				index += count; // add the readed bytes
			}
			index++; // jump an end of line byte
			return text;
		}

		public static string ReadUTF16(byte[] frame,ref int index)
		{
			string text = null;
			UnicodeEncoding encoding = null;
			bool readString = true;
			if(frame[index] == 0xfe && frame[index+1] == 0xff) // Big Endian
			{
				encoding = new UnicodeEncoding(true,false);
			}
			else
			{
				if(frame[index] == 0xff && frame[index+1] == 0xfe) // Litle Endian
				{
					encoding = new UnicodeEncoding(false,false);
				}
				else
				{
					if(frame[index] == 0x00 && frame[index+1] == 0x00)
					{
						readString = false;
					}
					else
					{
						throw new Exception("Invalid Frame");
					}
				}
			}
			index+=2; // skip the BOM or EOL
			if(readString == true)
			{
				int count = Memory.FindShort(frame,0,index);
				if(count == -1)
				{
					throw new Exception("Invalid Frame");
				}
				text = encoding.GetString(frame,index,count);
				index += count; // add the readed bytes
				index += 2; // skip the EOL
			}
			return text;
		}

		public static string ReadUTF16BE(byte[] frame,ref int index)
		{
			string text = null;
			UnicodeEncoding encoding = new UnicodeEncoding(true,false);
			int count = Memory.FindShort(frame,0,index);
			if(count == -1)
			{
				throw new Exception("Invalid Frame");
			}
			if(count > 0)
			{
				text = encoding.GetString(frame,index,count);
				index += count; // add the readed bytes
			}
			index+=2; // jump an end of line unicode char
			return text;
		}

		public static string ReadUTF8(byte[] frame,ref int index)
		{
			string text = null;
			int count = Memory.FindByte(frame,0,index);
			if(count == -1)
			{
				throw new Exception("Invalid Frame");
			}
			if(count > 0)
			{
				text = UTF8Encoding.UTF8.GetString(frame,index,count);
				index += count; // add the readed bytes
			}
			index++; // jump an end of line byte
			return text;
		}

		public static string ReadASCIIEnd(byte[] frame, int index)
		{
      Encoding encoding = Encoding.Default;// Encoding.GetEncoding(1252); // Should be ASCII
			return encoding.GetString(frame,index,frame.Length-index);
		}

		public static string ReadUTF16End(byte[] frame, int index)
		{
			UnicodeEncoding encoding = null;
			if(frame[index] == 0xfe && frame[index+1] == 0xff) // Big Endian
			{
				encoding = new UnicodeEncoding(true,false);
			}
			else
			{
				if(frame[index] == 0xff && frame[index+1] == 0xfe) // Litle Endian
				{
					encoding = new UnicodeEncoding(false,false);
				}
				else
				{
					throw new Exception("Invalid Frame");
				}
			}
			index+=2; // skip the BOM or EOL
			return encoding.GetString(frame,index,frame.Length-index);
		}

		public static string ReadUTF16BEEnd(byte[] frame, int index)
		{
			UnicodeEncoding encoding = new UnicodeEncoding(true,false);
			return encoding.GetString(frame,index,frame.Length-index); 
		}

		public static string ReadUTF8End(byte[] frame,int index)
		{
			return UTF8Encoding.UTF8.GetString(frame,index,frame.Length-index);
		}

		// Write rutines
	
		public static byte[] WriteText(string text, TextCode code)
		{
			switch(code)
			{
				case TextCode.ASCII:
				{
					return WriteASCII(text);
				}
				case TextCode.UTF16:
				{
					return WriteUTF16(text);
				}
				case TextCode.UTF16BE:
				{
					return WriteUTF16BE(text);
				}
				case TextCode.UTF8:
				{
					return WriteUTF8(text);
				}
				default:
				{
					throw new Exception("Invalid string type");
				}
			}
		}

		public static byte[] WriteTextEnd(string text, TextCode code)
		{
			switch(code)
			{
				case TextCode.ASCII:
				{
					return WriteASCIIEnd(text);
				}
				case TextCode.UTF16:
				{
					return WriteUTF16End(text);
				}
				case TextCode.UTF16BE:
				{
					return WriteUTF16BEEnd(text);
				}
				case TextCode.UTF8:
				{
					return WriteUTF8End(text);
				}
				default:
				{
					throw new Exception("Invalid string type");
				}
			}
		}

		public static byte[] WriteASCII(string text)
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
      Encoding encoding = Encoding.Default;//Encoding.GetEncoding(1252); // Should be ASCII
			writer.Write(encoding.GetBytes(text));
			writer.Write((byte)0); //EOL
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public static byte[] WriteUTF16(string text)
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			if(text == null | text == "") //Write a null string
			{
				writer.Write((ushort)0);
				return buffer.GetBuffer();
			}
			writer.Write((byte)0xff); //Litle endian, we have UTF16BE for big endian
			writer.Write((byte)0xfe);
			UnicodeEncoding encoding = new UnicodeEncoding(false,false);
			writer.Write(encoding.GetBytes(text));
			writer.Write((ushort)0);
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public static byte[] WriteUTF16BE(string text)
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			UnicodeEncoding encoding = new UnicodeEncoding(true,false);
			if(text == null | text == "") // Write a null string
			{
				writer.Write((ushort)0);
				return buffer.GetBuffer();
			}
			writer.Write(encoding.GetBytes(text));
			writer.Write((ushort)0);
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public static byte[] WriteUTF8(string text)
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			if(text == null | text == "") // Write a null string
			{
				writer.Write((byte)0);
				return buffer.GetBuffer();
			}
			writer.Write(UTF8Encoding.UTF8.GetBytes(text));
			writer.Write((byte)0);
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public static byte[] WriteASCIIEnd(string text)
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			if(text == null | text == "")
			{
				return buffer.GetBuffer();
			}
      Encoding encoding = Encoding.Default;//Encoding.GetEncoding(1252); // Should be ASCII
			writer.Write(encoding.GetBytes(text));
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public static byte[] WriteUTF16End(string text)
		{
			MemoryStream buffer = new MemoryStream(text.Length+2);
			BinaryWriter writer = new BinaryWriter(buffer);
			if(text == null | text == "")
			{
				return buffer.GetBuffer();
			}
			UnicodeEncoding encoding;
			writer.Write((byte)0xff); // Litle endian
			writer.Write((byte)0xfe);
			encoding = new UnicodeEncoding(false,false);
			writer.Write(encoding.GetBytes(text));
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;

		}

		public static byte[] WriteUTF16BEEnd(string text)
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			if(text == null | text == "")
			{
				return buffer.GetBuffer();
			}
			UnicodeEncoding encoding = new UnicodeEncoding(true,false);
			writer.Write(encoding.GetBytes(text));
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public static byte[] WriteUTF8End(string text)
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			if(text == null | text == "")
			{
				return buffer.GetBuffer();
			}
			writer.Write(UTF8Encoding.UTF8.GetBytes(text));
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}
	}
}
