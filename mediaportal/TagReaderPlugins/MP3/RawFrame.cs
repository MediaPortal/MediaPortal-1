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
