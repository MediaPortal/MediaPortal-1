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
