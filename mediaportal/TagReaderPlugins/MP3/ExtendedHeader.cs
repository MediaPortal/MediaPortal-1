// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.IO;

namespace id3
{
	/// <summary>
	/// Summary description for ExtendedHeader.
	/// </summary>
	public class ExtendedHeader
	{
		Header _tagHeader;
		int _size;

		public Header Header
		{
			set{_tagHeader = value;}
		}

		public int Size
		{
			get{return _size;}
		}

		public void Deserialize(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			_size = Swap.Int32(Sync.UnsafeBigEndian(reader.ReadInt32()));
			if(_size < 6)
			{
				throw new Exception("corrupt extended header");
			}
			// TODO: implement the extended header, ignore for now since it's optional
			stream.Seek(_size,SeekOrigin.Current); 
			_size = 0;
		}

		public void Serialize(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			// TODO: implement the extended header
		}
	}
}
