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
				reader.Read(tag,0,4);
				if(tag[0] == 0)
				{
					break; // We reached the padding area
				}
				index+=4; // read 4 bytes
				//TODO: Validate key valid ranges
				frameSize = Swap.Int32(reader.ReadInt32());
				index+=4; // read 4 bytes
				// ID3v4 now has syncsafe sizes
				if(_tagHeader.Version == 4)
				{
					Sync.Unsafe(frameSize);
				}
				// The size of the frame can't be larger than the avaliable space
				if(frameSize > rawSize - index)
				{
					throw new Exception("Tag Frame corrupt");
				}
				flags = Swap.UInt16(reader.ReadUInt16());
				index+=2; // read 2 bytes
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
