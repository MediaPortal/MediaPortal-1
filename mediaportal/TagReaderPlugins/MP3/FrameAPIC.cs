// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Text;
using System.IO;

namespace id3
{
	/// <summary>
	/// Summary description for FrameAPIC.
	/// </summary>
	public class FrameAPIC : FrameBase
	{
		TextCode   _textEncoding;
		string _mime;
		byte   _pictureType;
		string _description;
		byte[] _pictureData;

		public FrameAPIC(string tag):base(tag)
		{

		}

		public override void Parse(byte[] frame)
		{
			int index = 0;
			_textEncoding = (TextCode)frame[index];
			index++;
			_mime = TextBuilder.ReadASCII(frame,ref index);
			_pictureType = frame[index];
			index++;
			_description = TextBuilder.ReadText(frame,ref index,_textEncoding);
			_pictureData = new byte[frame.Length - index];
			Memory.Copy(frame,index,_pictureData,0,frame.Length - index);
		}

		public override byte[] Make()
		{
			MemoryStream buffer = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(buffer);
			writer.Write((byte)_textEncoding);
			writer.Write(TextBuilder.WriteASCII(_mime));
			writer.Write(_pictureData);
			return buffer.GetBuffer();
		}

		public TextCode TextEncoding
		{
			get{ return _textEncoding;}
		}

		public string Mime
		{
			get{ return _mime;}
		}

		public byte PictureType
		{
			get{ return _pictureType;}
		}

		public string Description
		{
			get { return _description;}
		}

		public byte[] PictureData
		{
			get {return _pictureData;}
		}

	}
}
