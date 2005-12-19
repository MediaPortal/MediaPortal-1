/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Text;

namespace id3
{
	/// <summary>
	/// Represents the frame in raw a form
	/// </summary>
	public struct RawFrame
	{
		byte[] _tag;  // 4 bytes
		ushort _flags;
		byte[] _frame; // Variable size

		public RawFrame(byte[] tag, ushort flags, byte[] frame)
		{
			_tag = tag;
			_flags = flags;
			_frame = frame;
		}

		public int Size
		{
			get{return _frame.Length + 6;}
		}

		public string Tag
		{
			get
			{   
				return UTF8Encoding.UTF8.GetString(_tag,0,4);
			}
		}

		public byte[] Frame
		{
			get{return _frame;}
			set{_frame = value;}
		}

		public ushort Flags
		{
			get{return _flags;}
			set{_flags = value;}
		}
	}
}
