// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Text;
using System.IO;

namespace id3
{
	/// <summary>
	/// URL Frame.
	/// </summary>
	public class FrameURL : FrameBase
	{
		TextCode _textEncoding;
		string _contents;
		string _url;

		public FrameURL(string tag):base(tag)
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
			_contents = TextBuilder.ReadText(frame,ref index,_textEncoding);
			_url = TextBuilder.ReadTextEnd(frame,index,_textEncoding);
		}
		public override byte[] Make()
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			writer.Write((byte)_textEncoding);
			writer.Write(TextBuilder.WriteText(_contents,_textEncoding));
			writer.Write(TextBuilder.WriteTextEnd(_url,_textEncoding));
			byte[] frame = new byte[buffer.Length];
			Memory.Copy(buffer.GetBuffer(),frame,(int)buffer.Length);
			return frame;
		}
		public override string ToString()
		{
			return _url;
		}

		public string Contents
		{
			get{return _contents;}
			set{_contents = value;}
		}

		public string URL
		{
			get{return _url;}
			set{_url = value;}
		}
	}
}
