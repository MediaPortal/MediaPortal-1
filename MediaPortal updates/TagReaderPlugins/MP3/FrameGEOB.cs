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

// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Text;
using System.IO;

namespace id3
{
	/// <summary>
	/// Summary description for FrameGEOB.
	/// </summary>
	public class FrameGEOB : FrameBase
	{
		TextCode   _textEncoding;
		string _mime;
		string _fileName;
		string _description;
		byte[] _objectData;

		public FrameGEOB(string tag):base(tag)
		{
		}

		public override void Parse(byte[] frame)
		{
			int index = 0;
			_textEncoding = (TextCode)frame[index];
			index++;
			_mime = TextBuilder.ReadASCII(frame,ref index);
			_fileName = TextBuilder.ReadText(frame,ref index,_textEncoding);
			_description = TextBuilder.ReadText(frame,ref index,_textEncoding);
			_objectData = new byte[frame.Length - index];
			Memory.Copy(frame,index,_objectData,0,frame.Length - index);
		}

		public override byte[] Make()
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			writer.Write((byte)_textEncoding);
			writer.Write(TextBuilder.WriteASCII(_mime));
			writer.Write(TextBuilder.WriteText(_fileName,_textEncoding));
			writer.Write(TextBuilder.WriteText(_description,_textEncoding));
			writer.Write(_objectData);
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public TextCode TextEncoding
		{
			get{ return _textEncoding;}
		}

		public string Mime
		{
			get{ return _mime;}
		}


		public string Description
		{
			get { return _description;}
		}

		public byte[] ObjectData
		{
			get {return _objectData;}
		}

	}
}
