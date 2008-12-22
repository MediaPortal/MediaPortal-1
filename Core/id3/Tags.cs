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
using System.Collections;
using System.IO;
using System.Text;

namespace Roger.ID3
{
	public class Tags
	{
		byte majorVersion;
		byte minorVersion;

		struct FrameEntry
		{
			public string frameId;
			public object frameValue;

			public FrameEntry(string frameId, object frameValue)
			{
				this.frameId = frameId;
				this.frameValue = frameValue;
			}
		}

		ArrayList frames;

		private Tags(byte majorVersion, byte minorVersion, ArrayList frames)
		{
			this.majorVersion = majorVersion;
			this.minorVersion = minorVersion;
			this.frames = frames;
		}

		public static Tags FromFile(string path)
		{
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				Tags tags;
				tags = FromStream(stream);
				stream.Close();

				return tags;
			}
		}

		public static Tags FromStream(Stream stream)
		{
			ArrayList frames = new ArrayList();

			TagReader reader = new TagReader(stream);
			while (reader.Read())
			{
				string frameId = reader.GetKey();
				object frameValue = reader.GetValue();

				frames.Add(new FrameEntry(frameId, frameValue));
			}

			return new Tags(reader.MajorVersion, reader.MinorVersion, frames);
		}

		/// <summary>
		/// Return the ID3 version as a string, e.g. "2.3.0".
		/// </summary>
		public string Version
		{
			get 
			{
				return string.Format("2.{0}.{1}", majorVersion, minorVersion);
			}
		}

		public string Title
		{
			get
			{
				return this["TIT2"];
			}

			set
			{
				this["TIT2"] = value;
			}
		}

		public string Artist
		{
			get
			{
				return this["TPE1"]; 
			}

			set
			{
				this["TPE1"] = value;
			}
		}

		public string Album
		{
			get
			{
				return this["TALB"]; 
			}

			set
			{
				this["TALB"] = value;
			}
		}

		string GetFrameValueById(string index)
		{
			foreach (FrameEntry e in frames)
			{
				if (e.frameId == index)
					return (string)e.frameValue;
			}

			return null;
		}

		void SetFrameValueById(string index, string value)
		{
			// Replace any existing frame
			for (int i = 0; i < frames.Count; ++i)
			{
				if (((FrameEntry)frames[i]).frameId == index)
				{
					frames[i] = new FrameEntry(index, value);
					return;
				}
			}

			// Otherwise, put a new one in.
			frames.Add(new FrameEntry(index, value));
		}

		/// <summary>
		/// Get the first instance of a particular frame type in the array.
		/// </summary>
		public string this[string index]
		{
			get
			{
				return GetFrameValueById(index);
			}

			set
			{
				SetFrameValueById(index, value);
			}
		}

		/// <summary>
		/// Copy the ID3 tags from one file to another, without interpreting them in any way.
		/// </summary>
		/// <param name="source">The name of the source file.</param>
		/// <param name="destination">The name of the destination file.</param>
		public static void Copy(string source, string destination)
		{
			TagCopier.Copy(source, destination);
		}

		public static void Remove(string path)
		{
			TagRemover.Remove(path);
		}

		public void Save(string path)
		{
			TagBuilder builder = new TagBuilder(majorVersion, minorVersion);
			foreach (FrameEntry frame in frames)
				builder.Append(frame.frameId, frame.frameValue);

			// TODO: Exception handler to clean up after any failures.

			// Create a temporary file:
			TemporaryFile tempFile = new TemporaryFile(path);
			FileStream tempStream = new FileStream(tempFile.Path, FileMode.Create, FileAccess.Write, FileShare.None);

			builder.WriteTo(tempStream);

			// Skip the tags in the source file.
			FileStream sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
			long pos0 = sourceStream.Position;
			TagUtil.SkipTags(sourceStream);
			long pos1 = sourceStream.Position;

			StreamCopier.Copy(sourceStream, tempStream);

			sourceStream.Close();
			tempStream.Close();

			// Then we can swap the files over.
			tempFile.Swap();
		}
	}
}
