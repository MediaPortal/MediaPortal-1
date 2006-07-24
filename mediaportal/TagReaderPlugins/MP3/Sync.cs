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
	/// ID3 SyncSafe
	/// </summary>
	public class Sync
	{
		/// <summary>
		/// Converts from a syncsafe to a integer
		/// </summary>
		/// <param name="val">Litle-endian Sincsafe value</param>
		/// <returns>Litle-endian normal value</returns>
		public static unsafe int Unsafe(int val)
		{
			byte* pVal = (byte*)&val;
			if(pVal[0] > 0x7f || pVal[1] > 0x7f || pVal[2] > 0x7f || pVal[3] > 0x7f)
			{
				throw new Exception("Syncsafe value corrupted");
			}

			int sync = 0;
			byte* pSync = (byte*)&sync;
			pSync[0] = (byte)(((pVal[0]>>0) & 0x7f) | ((pVal[1] & 0x01) << 7)); 
			pSync[1] = (byte)(((pVal[1]>>1) & 0x3f) | ((pVal[2] & 0x03) << 6));
			pSync[2] = (byte)(((pVal[2]>>2) & 0x1f) | ((pVal[3] & 0x07) << 5));
			pSync[3] = (byte)(((pVal[3]>>3) & 0x0f));
			return sync;
		}

		/// <summary>
		/// Converts from a intger to a syncsafe value
		/// </summary>
		/// <param name="val">Bigendian normal value</param>
		/// <returns>Bigendian syncsafe value</returns>
		public static unsafe int Safe(int val)
		{
			if(val > 0x10000000)
			{
				throw new OverflowException("value is too large for a syncsafe integer") ;
			}
			int sync = 0;
			byte* pVal = (byte*)&val;
			byte* pSync = (byte*)&sync;
			pSync[0] = (byte)((pVal[0]>>0) & 0x7f); 
			pSync[1] = (byte)(((pVal[0]>>7) & 0x01) | (pVal[1]<<1) & 0x7f ); 
			pSync[2] = (byte)(((pVal[1]>>6) & 0x03) | (pVal[2]<<2) & 0x7f );
			pSync[3] = (byte)(((pVal[2]>>5) & 0x07) | (pVal[3]<<3) & 0x7f );
			return sync;
		}

		/// <summary>
		/// Converts from a SyncSafe integer value to a numal value
		/// </summary>
		/// <param name="val">Big-endian Sincsafe value</param>
		/// <returns>Big-endian normal value</returns>
		public static unsafe int UnsafeBigEndian(int val)
		{
			byte* pVal = (byte*)&val;
			if(pVal[0] > 0x7f || pVal[1] > 0x7f || pVal[2] > 0x7f || pVal[3] > 0x7f)
			{
				throw new Exception("Syncsafe value corrupted");
			}

			int sync = 0;
			byte* pSync = (byte*)&sync;
			pSync[3] = (byte)(((pVal[3]>>0) & 0x7f) | ((pVal[2] & 0x01) << 7)); 
			pSync[2] = (byte)(((pVal[2]>>1) & 0x3f) | ((pVal[1] & 0x03) << 6));
			pSync[1] = (byte)(((pVal[1]>>2) & 0x1f) | ((pVal[0] & 0x07) << 5));
			pSync[0] = (byte)(((pVal[0]>>3) & 0x0f));
			return sync;
		}

		/// <summary>
		/// Converts from a SyncSafe integer value to a numal value
		/// </summary>
		/// <param name="val">Big-endian normal value</param>
		/// <returns>Big-endian syncsafe value</returns>
		public static unsafe int SafeBigEndian(int val)
		{
			if(val > 0x10000000)
			{
				throw new OverflowException("value is too large for a syncsafe integer") ;
			}
			int sync = 0;
			byte* pVal = (byte*)&val;
			byte* pSync = (byte*)&sync;
			pSync[3] = (byte)((pVal[3]>>0) & 0x7f); 
			pSync[2] = (byte)(((pVal[3]>>7) & 0x01) | (pVal[2]<<1) & 0x7f ); 
			pSync[1] = (byte)(((pVal[2]>>6) & 0x03) | (pVal[1]<<2) & 0x7f );
			pSync[0] = (byte)(((pVal[1]>>5) & 0x07) | (pVal[0]<<3) & 0x7f );
			return sync;
		}

		/// <summary>
		/// Stream adaper, converts from a syncsafe stream to an unsafe stream
		/// </summary>
		/// <param name="src">origin stream</param>
		/// <param name="dst">destination stream</param>
		/// <param name="size">bytes to be proccesed</param>
		/// <returns>number of bytes removed form the original stram</returns>
		public static int Unsafe(Stream src, Stream dst, int size)
		{
			BinaryWriter writer = new BinaryWriter(dst);
			BinaryReader reader = new BinaryReader(src);
			
			byte last = 0;
			int syncs = 0, count = 0;

			while(count < size)
			{
				byte val = reader.ReadByte();
				if (last == 0xFF && val == 0x00)
				{
					syncs++; // skip the sync byte
				}
				else
				{
					writer.Write(val);
				}
				last = val;
				count++;
			}
			if (last == 0xFF)
			{
				writer.Write((byte)0x00);
				syncs++;
			}
			return syncs; //bytes removed form stream
		}

		/// <summary>
		/// Stream adaper, converts from an unsafe stream to a syncsafe stream 
		/// </summary>
		/// <param name="src">origin stream</param>
		/// <param name="dst">destination stream</param>
		/// <param name="size">bytes to be proccesed</param>
		/// <returns>number of bytes added to the original stram</returns>
		public static int Safe(Stream src, Stream dst, int count)
		{
			BinaryWriter writer = new BinaryWriter(dst);
			BinaryReader reader = new BinaryReader(src);
			
			byte last = 0;
			int syncs = 0;

			while(count > 0)
			{
				byte val = reader.ReadByte();
				if (last == 0xFF && (val == 0x00 || val >= 0xE0))
				{
					writer.Write((byte)0x00);
					syncs++;
				}
				last = val;
				writer.Write(val);
				count--;
			}
			if (last == 0xFF)
			{
				writer.Write((byte)0x00);
				syncs++;
			}
			return syncs; // bytes added to the stream
		}
	}
}
