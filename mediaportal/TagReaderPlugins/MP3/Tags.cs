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
using System.Collections;

namespace id3
{
	/// <summary>
	/// ID3v4 Tag
	/// </summary>
	public class Tags:CollectionBase
	{
		private Header _tagHeader  = new Header();
		private ExtendedHeader _tagExtendedHeader  = new ExtendedHeader();

		public Header Header
		{
			get{ return _tagHeader;}
			set{ _tagHeader = value;}
		}

		public void Add(RawFrame frame)
		{
			List.Add(frame);
		}
	
		public int Size
		{
			get
			{
				int size = 0;
				foreach(RawFrame frame in List)
				{
					size+= frame.Size;
				}
				return size;
			}
		}

		public void Deserialize(Stream src)
		{
			Stream stream = src; 
			_tagHeader.Deserialize(stream); // load the header

			int id3TagSize = _tagHeader.Size;
			if(_tagHeader.Unsync == true)
			{
				MemoryStream memory = new MemoryStream();
				id3TagSize -= Sync.Unsafe(stream,memory,id3TagSize);
				stream = memory; // This is now the stream
				if(id3TagSize<=0)
				{
					throw new Exception("Corrupt Tag");
				}
			}
			int rawSize;
			// load the extended header
			if(_tagHeader.ExtendedHeader == true)
			{
				_tagExtendedHeader.Header = _tagHeader;
				_tagExtendedHeader.Deserialize(stream);
				rawSize = id3TagSize - _tagExtendedHeader.Size;
				if(id3TagSize<=0)
				{
					throw new Exception("Corrupt Tag");
				}
			}
			else
			{
				rawSize = id3TagSize;
			}
			ReadFrames(stream,rawSize);
		}

		private void ReadFrames(Stream stream,int rawSize)
		{
			List.Clear(); // We don't want to mix old and new frames.
			if(rawSize <= 0)
			{
				throw new Exception("No frames are present");
			}
			BinaryReader reader = new BinaryReader(stream);
			// Load the tag frames
			ushort flags;
			int index = 0, frameSize; 
			while(rawSize > index + 10) // repeat while there is at least one complete frame avaliable, 10 is the minimum size of a valid frame
			{
				byte[] tag = new byte[4];
				
				if (_tagHeader.Version==2)
				{
					reader.Read(tag,0,3);
					index+=3; // read 3 bytes
				}
				else
				{
					reader.Read(tag,0,4);
					index+=4; // read 4 bytes
				}
				if(tag[0] == 0)
				{
					break; // We reached the padding area
				}
				byte[] tagSize = new byte[5];    // I use this to read the bytes in from the file
				int[] bytes = new int[5];      // for bit shifting
				ulong newSize = 0;    // for the final number

				if (_tagHeader.Version == 2)
				{
					// only have 3 bytes for size ;


					tagSize = reader.ReadBytes(3);    // I use this to read the bytes in from the file
					bytes = new int[5];      // for bit shifting
					newSize = 0;    // for the final number
					// The ID3v2 tag size is encoded with four bytes
					// where the most significant bit (bit 7)
					// is set to zero in every byte,
					// making a total of 28 bits.
					// The zeroed bits are ignored
					//
					// Some bit grinding is necessary.  Hang on.
			

					bytes[3] =  tagSize[2]             | ((tagSize[1] & 1) << 7) ;
					bytes[2] = ((tagSize[1] >> 1) & 63) | ((tagSize[0] & 3) << 6) ;
					bytes[1] = ((tagSize[0] >> 2) & 31) ;

					newSize  = (((UInt64)bytes[3]) |
											((UInt64)bytes[2] << 8)  |
											((UInt64)bytes[1] << 16));
					//End Dan Code
					index+=3; // read 3 bytes
				}
				else if (_tagHeader.Version == 3 || _tagHeader.Version == 4)
				{
					// version  2.4
					tagSize = reader.ReadBytes(4);    // I use this to read the bytes in from the file
					bytes = new int[4];      // for bit shifting
					newSize = 0;    // for the final number
				
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

					if ( (tagSize[3]&0x80)==0 && (tagSize[2]&0x80)==0 &&  (tagSize[1]&0x80)==0 && (tagSize[0]&0x80)==0)
					{
						newSize  = (((UInt64)bytes[3]) |
							((UInt64)bytes[2] << 8)  |
							((UInt64)bytes[1] << 16) |
							((UInt64)bytes[0] << 24)) ;
						//End Dan Code
					}
					else
						newSize  = (ulong)((tagSize[3])+(tagSize[2]<<8)+(tagSize[1]<<16)+(tagSize[0]<<24));
					index+=4; // read 4 bytes
				}

				//TODO: Validate key valid ranges
				frameSize = (int)newSize;
				
				
				// The size of the frame can't be larger than the avaliable space
				if(frameSize > rawSize - index)
				{
					break;//throw new Exception("Tag Frame corrupt");
				}

				flags=0;
				if (_tagHeader.Version > 2)
				{
					// versions 3+ have frame tags.
					if (_tagHeader.Version == 3)
					{
						flags = Swap.UInt16(reader.ReadUInt16());
						index+=2; // read 2 bytes
					}
					else if (_tagHeader.Version == 4)
					{

						flags = Swap.UInt16(reader.ReadUInt16());
						index+=2; // read 2 bytes
					}
				}

				byte[] frameData = new byte[frameSize];
				reader.Read(frameData,0,frameSize);
				index+=frameSize; // read more bytes
				List.Add(new RawFrame(tag,flags,frameData));
			}
		}

		public void Serialize(Stream stream)
		{
			//TODO:implement write the tags.
			if(List.Count <= 0)
			{
				throw new Exception("Can't serialize an empty tag");
			}
		/*	MemoryStream memory = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);*/
			
		}
		private void WriteFrames(Stream stream,int rawSize)
		{

		}

	}
}
