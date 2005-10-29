/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

			/*
			// Get the id3 size, swap and unsync the integer
			uint x=reader.ReadUInt32();
			_id3RawSize = Swap.Int32(Sync.UnsafeBigEndian((int)x));
			if(_id3RawSize == 0)
			{
				throw new Exception("tag size can't be zero");
			}*/

			// read teh size
			// this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
			//Dan Code 
			char[] tagSize = reader.ReadChars(4);    // I use this to read the bytes in from the file
			int[] bytes = new int[4];      // for bit shifting
			ulong newSize = 0;    // for the final number
			// The ID3v2 tag size is encoded with four bytes
			// where the most significant bit (bit 7)
			// is set to zero in every byte,
			// making a total of 28 bits.
			// The zeroed bits are ignored
			//
			// Some bit grinding is necessary.  Hang on.
			

			bytes[3] =  tagSize[3]             | ((tagSize[2] & 1) << 7) ;
			bytes[2] = ((tagSize[2] >> 1) & 63) | ((tagSize[1] & 3) << 6) ;
			bytes[1] = ((tagSize[1] >> 2) & 31) | ((tagSize[0] & 7) << 5) ;
			bytes[0] = ((tagSize[0] >> 3) & 15) ;

			newSize = ((UInt64)10 +	(UInt64)bytes[3] |
											((UInt64)bytes[2] << 8)  |
											((UInt64)bytes[1] << 16) |
											((UInt64)bytes[0] << 24)) ;
			//End Dan Code
			_id3RawSize=(int)newSize;

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
