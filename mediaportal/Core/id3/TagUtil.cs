#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.IO;
using System.Text;

namespace Roger.ID3
{
	/// <summary>
	/// Summary description for TagUtil.
	/// </summary>
	public class TagUtil
	{
		public static long GetTagEndOffset(Stream stream)
		{
			long returnPos = stream.Position;

			byte[] magicBuffer = new byte[3];
			stream.Read(magicBuffer, 0, 3);

			string magic = Encoding.ASCII.GetString(magicBuffer, 0, 3);
			if (magic != "ID3")
			{
				// It's not got tags: backup a little bit and leave it alone.
				stream.Seek(returnPos, SeekOrigin.Begin);
				return 0;
			}

			// Figure out the version number.
			int majorVersion = stream.ReadByte();
			int minorVersion = stream.ReadByte();

			// Get the flags
			int tagFlags = stream.ReadByte();

			if (tagFlags != 0)
				throw new TagsException("We only know how to deal with tagFlags == 0");

			// TODO: Does 2.3.0 define this value as non-sync-safe?
			// Figure out the length.
			int tagLength = ReadInt28(stream);

			// That tagLength doesn't include the 10 bytes of _this_ header, but does include padding.

			// Back up again.
			stream.Seek(returnPos, SeekOrigin.Begin);
			return tagLength + 10;
		}

		public static void SkipTags(Stream stream)
		{
			long tagEndOffset = GetTagEndOffset(stream);
			stream.Seek(tagEndOffset, SeekOrigin.Current);
		}

		// TODO: Remove duplication.
		/// <summary>
		/// Read a sync-safe integer (i.e. 28 bits stashed in 32-bits).
		/// </summary>
		/// <param name="stream">The stream from which to read the integer.</param>
		/// <returns>The value of the sync-safe integer.</returns>
		public static int ReadInt28(Stream stream)
		{
			byte[] buffer = new byte[4];
			stream.Read(buffer, 0, 4);

			if ((buffer[0] & 0x80) != 0 || (buffer[1] & 0x80) != 0 || (buffer[2] & 0x80) != 0 || (buffer[3] & 0x80) != 0)
				throw new TagsException("Found invalid syncsafe integer");

			int result = (buffer[0] << 21) | (buffer[1] << 14) | (buffer[2] << 7) | buffer[3];
			return result;
		}
	}
}
