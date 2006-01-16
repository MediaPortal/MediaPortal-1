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
namespace id3
{
    using System;
	using System.IO;
	using System.Text;
	using System.Diagnostics;

    /// <summary>
    ///    Class representing an ID3v1 tag.
    /// </summary>
	public class ID3v1
	{
		private readonly byte[] _id3 = {0x54,0x41,0x47}; //"TAG"
		protected string _Song;
		protected string _Artist;
		protected string _Album;
		protected string _Year;
		protected string _Comment;
		protected string _Track;
		protected byte _Genre;

		public string Song
		{
			get { return _Song; }
		}

		public string Artist
		{
			get { return _Artist; }
		}

		public string Year
		{
			get { return _Year; }
		}

		public string Album
		{
			get { return _Album; }
		}

		public string Comment
		{
			get { return _Comment; }
		}

		public string Track
		{
			get { return _Track; }
		}

		public byte Genre
		{
			get { return _Genre; }
		}

		public Tags Tags
		{
			get
			{
				Tags tags = new Tags();
				FrameText frameText = new FrameText("TIT2");
				frameText.TextCode = TextCode.ASCII;
				frameText.Text = _Song;
				tags.Add(new RawFrame(UTF8Encoding.UTF8.GetBytes(frameText.Tag),0x0,frameText.Make()));

				frameText = new FrameText("TPE1");
				frameText.TextCode = TextCode.ASCII;
				frameText.Text = _Artist;
				tags.Add(new RawFrame(UTF8Encoding.UTF8.GetBytes(frameText.Tag),0x0,frameText.Make()));

				frameText = new FrameText("TALB");
				frameText.TextCode = TextCode.ASCII;
				frameText.Text = _Album;
				tags.Add(new RawFrame(UTF8Encoding.UTF8.GetBytes(frameText.Tag),0x0,frameText.Make()));

				frameText = new FrameText("TYER");
				frameText.TextCode = TextCode.ASCII;
				frameText.Text = _Year;
				tags.Add(new RawFrame(UTF8Encoding.UTF8.GetBytes(frameText.Tag),0x0,frameText.Make()));

				frameText = new FrameText("TRCK");
				frameText.TextCode = TextCode.ASCII;
				frameText.Text = _Track;
				tags.Add(new RawFrame(UTF8Encoding.UTF8.GetBytes(frameText.Tag),0x0,frameText.Make()));

				FrameLCText frameLCText = new FrameLCText("COMM");
				frameLCText.TextCode = TextCode.ASCII;
				frameLCText.Language = "";
				frameLCText.Contents = "";
				frameLCText.Text = _Comment;
				tags.Add(new RawFrame(UTF8Encoding.UTF8.GetBytes(frameLCText.Tag),0x0,frameLCText.Make()));

				Header header = new Header();
				header.Size = tags.Size;
				header.Version = 4;
				header.Revision = 0;
				header.Unsync = false;
				header.Experimental = false;
				header.Footer = false;
				header.ExtendedHeader = false;
				tags.Header = header;
				return tags;
			}
		}

		public void Deserialize(Stream src)
		{
			BinaryReader reader = new BinaryReader(src);

			// check for ID3v1 tag
      Encoding encoding = Encoding.Default;// Encoding.GetEncoding(1252); // Should be ASCII
			reader.BaseStream.Seek(-128, SeekOrigin.End);

			byte[] idTag = new byte[3];

			// Read the tag identifier
			reader.Read(idTag,0,3);
			// Compare the readed tag
			if(Memory.Compare(_id3,idTag,3) == true)
			{
				// found a ID3 tag
				byte[] tag = new byte[30];

				 reader.Read(tag,0,30);
				_Song = encoding.GetString(tag);//.Trim();
				reader.Read(tag,0,30);
				_Artist = encoding.GetString(tag);//.Trim();
				reader.Read(tag,0,30);
				_Album = encoding.GetString(tag);//.Trim();
				reader.Read(tag,0,4);
				_Year = encoding.GetString(tag,0,4);
				reader.Read(tag,0,30);
				if (tag[28] == 0) // track number is stored at position 29
				{
					_Track = tag[29].ToString();
					_Comment = encoding.GetString(tag, 0, 28);//.Trim();
				}
				else
				{
					_Track = "0";
					_Comment = encoding.GetString(tag);//.Trim();
				}
				_Genre = reader.ReadByte();
			}
			else
			{
				throw new Exception("ID3v1 tag not found");
			}
		}
	}
}
