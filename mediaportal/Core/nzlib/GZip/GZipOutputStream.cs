// GzipOutputStream.cs
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.

using System;
using System.IO;

using NZlib.Checksums;
using NZlib.Compression;
using NZlib.Streams;

namespace NZlib.GZip {
	
	/// <summary>
	/// This filter stream is used to compress a stream into a "GZIP" stream.
	/// The "GZIP" format is described in RFC 1952.
	///
	/// author of the original java version : John Leuner
	/// </summary>
	/// <example> This sample shows how to gzip a file
	/// <code>
	/// using System;
	/// using System.IO;
	/// 
	/// using NZlib.GZip;
	/// 
	/// class MainClass
	/// {
	/// 	public static void Main(string[] args)
	/// 	{
	/// 		Stream s = new GZipOutputStream(File.Create(args[0] + ".gz"));
	/// 		FileStream fs = File.OpenRead(args[0]);
	/// 		byte[] writeData = new byte[fs.Length];
	/// 		fs.Read(writeData, 0, (int)fs.Length);
	/// 		s.Write(writeData, 0, writeData.Length);
	/// 		s.Close();
	/// 	}
	/// }	
	/// </code>
	/// </example>
	public class GZipOutputStream : DeflaterOutputStream
	{
		//Variables
		
		/// <summary>
		/// CRC-32 value for uncompressed data
		/// </summary>
		protected Crc32 crc = new Crc32();
		
		// Constructors
		
		/// <summary>
		/// Creates a GzipOutputStream with the default buffer size
		/// </summary>
		/// <param name="baseOutputStream">
		/// The stream to read data (to be compressed) from
		/// </param>
		public GZipOutputStream(Stream baseOutputStream) : this(baseOutputStream, 4096)
		{
		}
		
		/// <summary>
		/// Creates a GZIPOutputStream with the specified buffer size
		/// </summary>
		/// <param name="baseOutputStream">
		/// The stream to read data (to be compressed) from
		/// </param>
		/// <param name="size">
		/// Size of the buffer to use
		/// </param>
		public GZipOutputStream(Stream baseOutputStream, int size) : base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, true), size)
		{
			// TODO : find out correctness, orgininally this was : (int) (System.currentTimeMillis() / 1000L);
			int mod_time = (int)(DateTime.Now.Ticks / 10000L);  // Ticks give back 100ns intervals
			byte[] gzipHeader = {
				/* The two magic bytes */
				(byte) (GZipConstants.GZIP_MAGIC >> 8), (byte) GZipConstants.GZIP_MAGIC,
				
				/* The compression type */
				(byte) Deflater.DEFLATED,
				
				/* The flags (not set) */
				0,
				
				/* The modification time */
				(byte) mod_time, (byte) (mod_time >> 8),
				(byte) (mod_time >> 16), (byte) (mod_time >> 24),
				
				/* The extra flags */
				0,
				
				/* The OS type (unknown) */
				(byte) 255
			};
			
			baseOutputStream.Write(gzipHeader, 0, gzipHeader.Length);
			//    System.err.println("wrote GZIP header (" + gzipHeader.length + " bytes )");
		}
		
		public override void Write(byte[] buf, int off, int len)
		{
			crc.Update(buf, off, len);
			base.Write(buf, off, len);
		}
		
		/// <summary>
		/// Writes remaining compressed output data to the output stream
		/// and closes it.
		/// </summary>
		public override void Close()
		{
			Finish();
			baseOutputStream.Close();
		}
		
		public override void Finish()
		{
			base.Finish();
			
			int totalin = def.TotalIn;
			int crcval = (int) (crc.Value & 0xffffffff);
			
			//    System.err.println("CRC val is " + Integer.toHexString( crcval ) 		       + " and length " + Integer.toHexString(totalin));
			
			byte[] gzipFooter = {
				(byte) crcval, (byte) (crcval >> 8),
				(byte) (crcval >> 16), (byte) (crcval >> 24),
				
				(byte) totalin, (byte) (totalin >> 8),
				(byte) (totalin >> 16), (byte) (totalin >> 24)
			};
			
			baseOutputStream.Write(gzipFooter, 0, gzipFooter.Length);
			//    System.err.println("wrote GZIP trailer (" + gzipFooter.length + " bytes )");
		}
	}
}
