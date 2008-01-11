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



namespace WaveLib

{

	public class WaveStream : Stream, IDisposable

	{

		private Stream m_Stream;

		private long _dataPos;

		private long m_Length;



		private WaveFormat m_Format;



		public WaveFormat Format

		{

			get { return m_Format; }

		}



		private string ReadChunk(BinaryReader reader)

		{

			byte[] ch = new byte[4];

			reader.Read(ch, 0, ch.Length);

			return System.Text.Encoding.ASCII.GetString(ch);

		}



		private void ReadHeader()

		{

			BinaryReader Reader = new BinaryReader(m_Stream);

			if (ReadChunk(Reader) != "RIFF")

				throw new Exception("Invalid file format");



			Reader.ReadInt32(); // File length minus first 8 bytes of RIFF description, we don't use it



			if (ReadChunk(Reader) != "WAVE")

				throw new Exception("Invalid file format");



			if (ReadChunk(Reader) != "fmt ")

				throw new Exception("Invalid file format");



			int FormatLength = Reader.ReadInt32();

      if ( FormatLength < 16) // bad format chunk length

				throw new Exception("Invalid file format");



			m_Format = new WaveFormat(22050, 16, 2); // initialize to any format

			m_Format.wFormatTag = Reader.ReadInt16();

			m_Format.nChannels = Reader.ReadInt16();

			m_Format.nSamplesPerSec = Reader.ReadInt32();

			m_Format.nAvgBytesPerSec = Reader.ReadInt32();

			m_Format.nBlockAlign = Reader.ReadInt16();

			m_Format.wBitsPerSample = Reader.ReadInt16(); 

      if ( FormatLength > 16)

      {

        m_Stream.Position += (FormatLength-16);

      }

			// assume the data chunk is aligned

			while(m_Stream.Position < m_Stream.Length && ReadChunk(Reader) != "data")

				;



			if (m_Stream.Position >= m_Stream.Length)

				throw new Exception("Invalid file format");



			m_Length = Reader.ReadInt32();

			_dataPos = m_Stream.Position;



			Position = 0;

		}



		public WaveStream(string fileName) : this(new FileStream(fileName, FileMode.Open))

		{

		}

		public WaveStream(Stream S)

		{

			m_Stream = S;

			ReadHeader();

		}

		~WaveStream()

		{

			Dispose();

		}

		public new void Dispose()
		{
      base.Dispose();
			if (m_Stream != null)

				m_Stream.Close();

			GC.SuppressFinalize(this);
		}



		public override bool CanRead

		{

			get { return true; }

		}

		public override bool CanSeek

		{

			get { return true; }

		}

		public override bool CanWrite

		{

			get { return false; }

		}

		public override long Length

		{

			get { return m_Length; }

		}

		public override long Position

		{

			get { return m_Stream.Position - _dataPos; }

			set { Seek(value, SeekOrigin.Begin); }

		}

		public override void Close()

		{

			Dispose();

		}

		public override void Flush()

		{

		}

		public override void SetLength(long len)

		{

			throw new InvalidOperationException();

		}

		public override long Seek(long pos, SeekOrigin o)

		{

			switch(o)

			{

				case SeekOrigin.Begin:

					m_Stream.Position = pos + _dataPos;

					break;

				case SeekOrigin.Current:

					m_Stream.Seek(pos, SeekOrigin.Current);

					break;

				case SeekOrigin.End:

					m_Stream.Position = _dataPos + m_Length - pos;

					break;

			}

			return this.Position;

		}

		public override int Read(byte[] buf, int ofs, int count)

		{

			int toread = (int)Math.Min(count, m_Length - Position);

			return m_Stream.Read(buf, ofs, toread);

		}

		public override void Write(byte[] buf, int ofs, int count)

		{

			throw new InvalidOperationException();

		}

	}

}

