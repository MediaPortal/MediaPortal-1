// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Text;
using System.IO;

namespace id3
{
	/// <summary>
	/// Language Comment Text Frame.
	/// Lyrics and Coment frames use this format.
	/// </summary>
	public class FrameLCText : FrameBase
	{
		string _contents;
		string _text;
		string _language;
		TextCode _textEncoding;

		public FrameLCText(string tag):base(tag)
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
			_language = UTF8Encoding.UTF8.GetString(frame,index,3);
			index+=3; // Three language bytes
			_contents = TextBuilder.ReadText(frame,ref index,_textEncoding);
			_text = TextBuilder.ReadTextEnd(frame,index,_textEncoding);
		}

		public override byte[] Make()
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			writer.Write((byte)_textEncoding);
			//TODO: Validate langage field
			writer.Write(TextBuilder.WriteASCII(_language));
			writer.Write(TextBuilder.WriteText(_contents,_textEncoding));
			writer.Write(TextBuilder.WriteTextEnd(_text,_textEncoding));
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}

		public override string ToString()
		{
			return _text;
		}

		public string Contents
		{
			get{return _contents;}
			set{_contents = value;}
		}

		public string Text
		{
			get{return _text;}
			set{_text = value;}
		}

		public string Language
		{
			get{return _language;}
			set{_language = value;}
		}
	}
}
