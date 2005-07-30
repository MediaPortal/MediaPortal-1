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

using System;
using System.IO;
using System.Text;

using MediaPortal.TagReader.MP4.MiscUtil.Conversion;

namespace MediaPortal.TagReader.MP4
{
	/// <summary>
	/// 
	/// </summary>
	public class ParsedAtom
	{
		protected long m_size;
		protected string m_type;

		public ParsedAtom(long size, string type)
		{
			m_size = size;
			m_type = type;
		}

		public long Size 
		{
			get 
			{
				return m_size;
			}
		}

		public string Type
		{
			get
			{
				return m_type;
			}
		}
	}

	public class ParsedContainerAtom: ParsedAtom
	{
		protected ParsedAtom[] m_children;

		public ParsedContainerAtom(long atomSize, string atomType, ParsedAtom[] children) : base (atomSize, atomType)
		{
			m_children = children;
		}

		public override string ToString() 
		{
			return  m_type + " (" + m_size + " bytes) - " + 
				m_children.Length + 
				(m_children.Length == 1 ?
				" child" :
				" children");

		}

		public ParsedAtom[] Children 
		{
			get 
			{
				return m_children;
			}
		}
	}

	public class ParsedLeafAtom: ParsedAtom 
	{
		public ParsedLeafAtom (long size, string type, Stream s) : base(size, type)
		{
			init(s);
		}

		protected virtual void init(Stream s) 
		{
		}

		override public string ToString () 
		{
			return  m_type + " (" + m_size + " bytes) "; 
		}
	}

	public class ParsedHdlrAtom: ParsedLeafAtom
	{
		protected int m_version;
		protected string m_componentType;
		protected string m_componentSubType;
		protected string m_componentManufacturer;
		protected string m_componentName;

		public ParsedHdlrAtom(long size, string type, Stream s) : base(size, type,s)
		{
		}

		protected override void init(Stream s)
		{
			base.init (s);
			// hdlr contains a 1-byte version, 3 bytes of (unused) flags,
			// 4-char component type, 4-char component subtype,
			// 4-byte fields for comp mfgr, comp flags, and flag mask
			// then a pascal string for component name

			byte[] buffy = new byte[4];
			s.Read(buffy, 0, 1);
			m_version = buffy [0];
			// flags are defined as 3 bytes of 0, I just read & forget
			s.Read(buffy, 0, 3);
			// component type and subtype (4-byte strings)
			s.Read(buffy, 0, 4);
			m_componentType = Encoding.Default.GetString(buffy);
			s.Read(buffy, 0, 4);
			m_componentSubType = Encoding.Default.GetString(buffy);
			// component mfgr (4 bytes, apple says "reserved- set to 0")
			s.Read(buffy, 0, 4);
			m_componentManufacturer = Encoding.Default.GetString(buffy);
			// component flags & flag mask 
			// (4 bytes each, apple says "reserved- set to 0", skip for now)
			s.Read(buffy, 0, 4);
			s.Read (buffy, 0, 4);
			// length of pascal string
			s.Read (buffy, 0, 1);
			int compNameLen = buffy[0];
			/* undocumented hack:
			   in .mp4 files (as opposed to .mov's), the component name
			   seems to be a C-style (null-terminated) string rather
			   than Pascal-style (length-byte then run of characters).
			   However, the name is the last thing in this atom, so
			   if the String size is wrong, assume we're in MPEG-4
			   and just read to end of the atom.  In other words, the
			   string length *must* always be atomSize - 33, since there
			   are 33 bytes prior to the string, and it's the last thing
			   in the atom.
			*/
			if (compNameLen != (m_size - 33)) 
			{
				// MPEG-4 case
				compNameLen = (int)m_size - 33;
				// back up one byte (since what we thought was
				// length was actually first char of string)
				s.Seek (-1, SeekOrigin.Current);
			}
			byte[] compNameBuf = new byte[compNameLen];
			s.Read(compNameBuf, 0, compNameLen);
			m_componentName = Encoding.Default.GetString(compNameBuf);
		}

		public int Version 
		{
			get { return m_version; }
		}

		public string ComponentType
		{
			get { return m_componentType; }
		}

		public string ComponentSubType
		{
			get { return m_componentSubType; }
		}

		public string ComponentManufacturer
		{
			get { return m_componentManufacturer; }
		}

		public string ComponentName
		{
			get { return m_componentName; }
		}
	}

	public class ParsedWlocAtom: ParsedLeafAtom 
	{
		protected int m_x;
		protected int m_y;

		public ParsedWlocAtom(long size, string type, Stream s) : base(size, type,s)
		{
		}

		override protected void init (Stream s)
		{
			// WLOC contains 16-bit x,y values
			byte[] value = new byte[4];
			s.Read (value, 0, value.Length);
			m_x = (value[0] << 8) | value[1];
			m_y = (value[2] << 8) | value[3]; 
		}

		public int X 
		{
			get { return m_x; }
		}

		public int Y
		{
			get { return m_y; }
		}
	}

	public class ParsedElstAtom: ParsedLeafAtom
	{
		public class Edit
		{
			long m_trackDuration;
			long m_mediaTime;
			float m_mediaRate;
			public Edit(long d, long t, float r) 
			{
				m_trackDuration = d;
				m_mediaTime = t;
				m_mediaRate = r;
			}
			public long TrackDuration { get {return m_trackDuration; }}
			public long MediaTime { get {return m_mediaTime; }}
			public float MediaRate { get {return m_mediaRate; }}
		}
		int m_version;
		Edit[] m_edits;

		public ParsedElstAtom(long size, string type, Stream s) : base(size, type,s)
		{
		}

		override protected void init (Stream s)
		{
			byte[] buffy = new byte[4];
			s.Read(buffy, 0, 1);
			m_version = buffy [0];
			// flags are defined as 3 bytes of 0, I just read & forget
			s.Read(buffy, 0, 3);
			// how many table entries are there?
			s.Read (buffy, 0, 4);
			int tableCount = EndianBitConverter.Big.ToInt32(buffy, 0);
			m_edits = new Edit[tableCount];
			for (int i=0; i < tableCount; i++) 
			{
				// TODO: also bounds-check that we don't go past size
				// track duration
				s.Read (buffy, 0, 4);
				long trackDuration = EndianBitConverter.Big.ToUInt32(buffy, 0);
				// media time
				s.Read (buffy, 0, 4);
				long mediaTime = EndianBitConverter.Big.ToUInt32(buffy, 0);
				// media rate
				// TODO: wrong, these 4 bytes are a fixed-point
				// float, 16-bits left of decimal, 16 - right
				// I don't get how apple does this, so I'm just reading
				// the integer part
				s.Read (buffy, 0, 2);
				float mediaRate = EndianBitConverter.Big.ToInt16(buffy, 0);
				s.Read (buffy, 0, 2);
				// make an Edit object
				m_edits[i] = new Edit (trackDuration, mediaTime, mediaRate);
			}
		}

		public int Version 
		{
			get { return m_version; }
		}

		public Edit[] Edits
		{
			get { return m_edits; }
		}
	}

	public class ParsedDataAtom: ParsedLeafAtom
	{
		byte[] m_data;

		public ParsedDataAtom(long size, string type, Stream s) : base(size, type,s)
		{
		}

		override protected void init (Stream s)
		{
			int tagSize = ((m_size > 0xffffffffL) ? Int32.MaxValue : (int)m_size) - 16;
			
			m_data = new byte[tagSize];
			s.Seek(8, SeekOrigin.Current);
			s.Read(m_data, 0, tagSize);
		}
		
		public byte[] Data
		{
			get { return m_data; }
		}


	}

	public class AtomFactory 
	{
		public static ParsedAtom createAtomFor(long atomSize, string atomType, Stream s) 
		{
			switch (atomType) 
			{
				case "WLOC":
					return new ParsedWlocAtom(atomSize, atomType, s);
				case "ELST":
					return new ParsedElstAtom(atomSize, atomType, s);
				case "HDLR":
					return new ParsedHdlrAtom(atomSize, atomType, s);
				case "DATA":
					return new ParsedDataAtom(atomSize, atomType, s);
				default:
					return new ParsedLeafAtom(atomSize, atomType, s);
			}
		}
	}
}
