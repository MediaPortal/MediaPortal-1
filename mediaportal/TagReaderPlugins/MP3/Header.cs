// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.IO;

namespace id3
{
	/// <summary>
	/// Summary description for ID3Tag.
	/// </summary>
	public class Header
	{
		private byte _id3Version;
		private byte _id3Revision;
		private byte _id3Flags;
		private int _id3RawSize;
		private readonly byte[] _id3 = {0x49,0x44,0x33}; //"ID3" tag

		public void Serialize(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(_id3);
			writer.Write(_id3Version);
			writer.Write(_id3Revision);
			writer.Write(_id3Flags);
			writer.Write(Swap.Int32(Sync.Safe(_id3RawSize)));
		}

		public void Deserialize(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			byte[] idTag = new byte[3];

			// Read the tag identifier
			reader.Read(idTag,0,3);
			// Compare the readed tag
			if(Memory.Compare(_id3,idTag,3) == false)
			{
				throw new Exception("Tag not found");
			}
			// Get the id3 version byte
			_id3Version = reader.ReadByte();  
			if( _id3Version == 0xff)
			{
				throw new Exception("invalid version");
			}
			// Get the id3 revision byte
			_id3Revision = reader.ReadByte(); 
			if(_id3Revision == 0xff)
			{
				throw new Exception("invalid revision");
			}
			// Get the id3 flag byte, only read what I understad
			_id3Flags = (byte)(0xf0 & reader.ReadByte());
			// Get the id3 size, swap and unsync the integer
			_id3RawSize = Swap.Int32(Sync.UnsafeBigEndian(reader.ReadInt32()));
			if(_id3RawSize == 0)
			{
				throw new Exception("tag size can't be zero");
			}
		}

		public byte Version
		{
			get{return _id3Version;}
			set{_id3Version = value;}
		}

		public int Size
		{
			get{return _id3RawSize;}
			set{_id3RawSize = value;}
		}

		public byte Revision
		{
			get{return _id3Revision;}
			set{_id3Revision = value;}
		}

		public bool Unsync
		{
			get{return (_id3Flags & 0x80) > 0;}
			set
			{
				if(value == true)
				{
					_id3Flags |= 0x80;
				}
				else
				{
					unchecked{_id3Flags &= (byte)~(0x80);}
				}
			}
		}

		public bool ExtendedHeader
		{
			get{return (_id3Flags & 0x40) > 0;}
			set
			{
				if(value == true)
				{
					_id3Flags |= 0x40;
				}
				else
				{
					unchecked{_id3Flags &= (byte)~(0x40);}
				}
			}
		}

		public bool Experimental
		{
			get{return (_id3Flags & 0x20)  > 0;}
			set
			{
				if(value == true)
				{
					_id3Flags |= 0x20;
				}
				else
				{
					unchecked{_id3Flags &= (byte)~(0x20);}
				}
			}
		}

		public bool Footer
		{
			get{return (_id3Flags & 0x10) > 0;}
			set
			{
				if(value == true)
				{
					_id3Flags |= 0x10;
				}
				else
				{
					unchecked{_id3Flags &= (byte)~(0x10);}
				}
			}
		}
	}
}
