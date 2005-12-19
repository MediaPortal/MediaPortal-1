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
	/// <summary>
	/// Summary description for FrameText.
	/// </summary>
	public class FrameText : FrameBase
	{
		private string _text;
		TextCode _textEncoding;
		public FrameText(string tag):base(tag)
		{
			_textEncoding = TextCode.ASCII;
		}

		public TextCode TextCode
		{
			get { return _textEncoding;}
			set { _textEncoding = value;}
		}

		public override void Parse(byte[] frame)
		{
			int index = 0;
			_textEncoding = (TextCode)frame[index];
			index++;
			_text = TextBuilder.ReadTextEnd(frame,index,_textEncoding);
		}

		public override byte[] Make()
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			writer.Write((byte)_textEncoding);
			writer.Write(TextBuilder.WriteTextEnd(_text,_textEncoding));
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public override string ToString()
		{
			return _text;
		}

		public string Text
		{
			get { return _text;}
			set { _text = value;}
		}

	}
}
