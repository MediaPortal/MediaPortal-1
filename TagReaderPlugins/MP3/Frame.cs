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
using System.IO;
using System.IO.Compression;

namespace id3
{
	/// <summary>
	/// Summary description for Frame.
	/// </summary>
	public class Frame
	{
		byte _group;
		byte _version;
		byte _revison;
		FrameBase _frameBase;
		
		// flags;
		bool _tagAlter;
		bool _fileAlter;
		bool _readOnly;
		bool _grouping;
		bool _compression;
		bool _encryption;
		bool _unsynchronisation;
		bool _dataLength;

		public Frame(Header tagHeader)
		{
			_version = tagHeader.Version;
			_revison = tagHeader.Revision;
			_frameBase = null;
		}

		public FrameBase FrameBase
		{
			get{ return _frameBase;}
		}

		/// <summary>
		/// Parses the flag to it's bits
		/// </summary>
		public ushort Flags
		{
			set
			{
				switch(_version)
				{
					case 3:
					{
						_tagAlter  = (value & 0x8000) > 0;
						_fileAlter = (value & 0x4000) > 0;;
						_readOnly  = (value & 0x2000) > 0;
						_compression = (value & 0x0080) > 0;
						_encryption = (value & 0x0040) > 0;
						_grouping   = (value & 0x0020) > 0;
						break;
					}
					case 4:
					{
						_tagAlter  = (value & 0x4000) > 0;
						_fileAlter = (value & 0x2000) > 0;;
						_readOnly  = (value & 0x1000) > 0;
						_grouping  = (value & 0x0040) > 0;
						_compression = (value & 0x0008) > 0;
						_encryption  = (value & 0x0004) > 0;;
						_unsynchronisation = (value & 0x0002) > 0;;
						_dataLength  = (value & 0x0001) > 0;
						break;
					}
					default:
					{
						//throw new Exception("Not implemented");
					}
						break;
				}
			}

			get
			{
				ushort flags = 0;
				switch(_version)
				{
					case 3:
					{
						flags = _tagAlter  ?(ushort)(flags | 0x8000):(ushort)(flags & unchecked((ushort)~(0x8000)));
						flags = _fileAlter ?(ushort)(flags | 0x4000):(ushort)(flags & unchecked((ushort)~(0x4000)));
						flags = _readOnly  ?(ushort)(flags | 0x2000):(ushort)(flags & unchecked((ushort)~(0x2000)));
						flags = _compression ?(ushort)(flags | 0x0080):(ushort)(flags & unchecked((ushort)~(0x0080)));
						flags = _encryption  ?(ushort)(flags | 0x0040):(ushort)(flags & unchecked((ushort)~(0x0040)));
						flags = _grouping  ?(ushort)(flags | 0x0020):(ushort)(flags & unchecked((ushort)~(0x0020)));
						break;
					}
					case 4:
					{
						flags = _tagAlter  ?(ushort)(flags | 0x4000):(ushort)(flags & unchecked((ushort)~(0x4000)));
						flags = _fileAlter ?(ushort)(flags | 0x2000):(ushort)(flags & unchecked((ushort)~(0x2000)));
						flags = _readOnly  ?(ushort)(flags | 0x1000):(ushort)(flags & unchecked((ushort)~(0x1000)));
						flags = _grouping  ?(ushort)(flags | 0x0040):(ushort)(flags & unchecked((ushort)~(0x0040)));
						flags = _compression ?(ushort)(flags | 0x0008):(ushort)(flags & unchecked((ushort)~(0x0008)));
						flags = _encryption  ?(ushort)(flags | 0x0004):(ushort)(flags & unchecked((ushort)~(0x0004)));
						flags = _unsynchronisation ?(ushort)(flags | 0x0002):(ushort)(flags & unchecked((ushort)~(0x0002)));
						flags = _dataLength ?(ushort)(flags | 0x0001):(ushort)(flags & unchecked((ushort)~(0x0001)));
						break;
					}
					default:
					{
						//throw new Exception("Not implemented");
					}
						break;
				}
				return flags;
			}
			
		}

		public void Parse(RawFrame rawFrame)
		{
			Flags = rawFrame.Flags;
			_frameBase = FrameBuilder.Build(rawFrame.Tag);

			int index = 0;
			int size = rawFrame.Frame.Length; //buffer length
			Stream stream = new MemoryStream(rawFrame.Frame,false);
			BinaryReader reader = new BinaryReader(stream);
			if(Grouping == true)
			{
				_group = reader.ReadByte();
				index++;
			}
			if(Compression == true)
			{
				switch(_version)
				{
					case 3:
					{
						size = Swap.Int32(reader.ReadInt32());
						break;
					}
					case 4:
					{
						size = Swap.Int32(Sync.UnsafeBigEndian(reader.ReadInt32()));
						break;
					}
					default:
					{
						throw new Exception("Not implemented");
					}
				}
				index=0;
                stream = new DeflateStream(stream, CompressionMode.Decompress);
			}
			if(Encryption == true)
			{
				//TODO: Encription
			//	throw new Exception("Not implemented");
			}
			if(Unsynchronisation == true)
			{
				Stream memStream = new MemoryStream();
				size = Sync.Unsafe(stream,memStream,size);
				index = 0;
				stream = memStream;
			}
			byte[] frameBuffer = new byte[size-index];
			stream.Read(frameBuffer,0,size-index);
			_frameBase.Parse(frameBuffer);
		}

		public override string ToString()
		{
			if(_frameBase == null)
			{
				throw new Exception("Parse the frame first");
			}
			return _frameBase.ToString();	
		}

		/// <summary>
		/// This flag tells the tag parser what to do with this frame if it is
		/// unknown and the tag is altered in any way.																												the frames.
		/// </summary>
		public bool TagAlter
		{
			get{ return _tagAlter ;}
			set{_tagAlter = value;}
		}
		
		/// <summary>
		/// This flag tells the tag parser what to do with this frame if it is
		/// unknown and the file, excluding the tag, is altered.
		/// </summary>
		public bool FileAlter
		{
			get{ return _fileAlter;}
			set{_fileAlter = value;}
		}

		/// <summary>
		/// This flag, if set, tells the software that the contents of this
		/// frame are intended to be read only.
		/// </summary>
		public bool ReadOnly
		{
			get{ return _readOnly;}
			set{_readOnly = value;}
		}

		/// <summary>
		/// This flag indicates whether or not this frame belongs in a group with other frames.
		/// </summary>
		public bool Grouping
		{
			get{ return _grouping;}
			set{_grouping = value;}
		}
	
		/// <summary>
		/// This flag indicates whether or not the frame is compressed.
		/// </summary>
		public bool Compression
		{
			get{ return _compression;}
			set{_compression = value;}
		}

		/// <summary>
		/// This flag indicates whether or not the frame is encrypted.
		/// </summary>
		public bool Encryption
		{
			get{ return _encryption;}
			set{_encryption = value;}
		}

		/// <summary>
		/// This flag indicates whether or not unsynchronisation was applied to this frame.
		/// </summary>
		public bool Unsynchronisation
		{
			get{ return _unsynchronisation;}
			set{_unsynchronisation = value;}
		}

		/// <summary>
		/// This flag indicates that a data length indicator has been added to the frame.
		/// </summary>
		public bool DataLength
		{
			get{ return _dataLength;}
			set{_dataLength = value;}
		}
	}
}