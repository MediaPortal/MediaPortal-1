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

// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.

using System;

namespace id3
{
	/// <summary>
	/// Raw Memory operations
	/// </summary>
	public class Memory
	{
		public static unsafe bool Compare(byte[] src, byte[] dst, int count)
		{
			if (count < 0 || src.Length < count || dst.Length < count)
			{
				throw new ArgumentException();
			}
			fixed (byte* pSrc = src, pDst = dst)
			{
				byte* ps = pSrc;
				byte* pd = pDst;
				// Loop over the count in blocks of 4 bytes, comparing an
				// integer at a time:
				for (int n = count >> 2; n != 0; n--)
				{
					if(*((int*)pd) != *((int*)ps))
						return false;
					pd += 4;
					ps += 4;
				}
				// Complete the copy by moving any bytes that weren't
				// moved in blocks of 4:
				for (count &= 3; count != 0; count--)
				{
					if(*pd != *ps)
						return false;
					pd++;
					ps++;
				}
			}
			return true;					  
		}

		public static unsafe bool Compare(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
		{
			if (src == null || srcIndex < 0 || dst == null || dstIndex < 0 || count < 0)
			{
				throw new ArgumentException();
			}
			if (src.Length - srcIndex < count || dst.Length - dstIndex < count)
			{
				throw new ArgumentException();
			}
			fixed (byte* pSrc = src, pDst = dst)
			{
				byte* ps = pSrc + srcIndex;
				byte* pd = pDst + dstIndex;
				// Loop over the count in blocks of 4 bytes, comparing an
				// integer at a time:
				for (int n = count >> 2; n != 0; n--)
				{
					if(*((int*)pd) != *((int*)ps))
						return false;
					pd += 4;
					ps += 4;
				}
				// Complete the copy by moving any bytes that weren't
				// moved in blocks of 4:
				for (count &= 3; count != 0; count--)
				{
					if(*pd != *ps)
						return false;
					pd++;
					ps++;
				}
			}
			return true;					  
		}

		public static unsafe bool Copy(byte[] src, byte[] dst, int count)
		{
			if (count < 0 || src.Length < count || dst.Length < count)
			{
				throw new ArgumentException();
			}
			fixed (byte* pSrc = src, pDst = dst)
			{
				byte* ps = pSrc;
				byte* pd = pDst;
				// Loop over the count in blocks of 4 bytes, comparing an
				// integer at a time:
				for (int n = count >> 2; n != 0; n--)
				{
					*((int*)pd) = *((int*)ps);
					pd += 4;
					ps += 4;
				}
				// Complete the copy by moving any bytes that weren't
				// moved in blocks of 4:
				for (count &= 3; count != 0; count--)
				{
					*pd = *ps;
					pd++;
					ps++;
				}
			}
			return true;					  
		}

		public static unsafe void Copy(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
		{
			if (src == null || srcIndex < 0 || dst == null || dstIndex < 0 || count < 0)
			{
				throw new ArgumentException();
			}
			if (src.Length - srcIndex < count || dst.Length - dstIndex < count)
			{
				throw new ArgumentException();
			}
			fixed (byte* pSrc = src, pDst = dst)
			{
				byte* ps = pSrc + srcIndex;
				byte* pd = pDst + dstIndex;
				// Loop over the count in blocks of 4 bytes, copying an
				// integer at a time:
				for (int n = count >> 2; n != 0; n--)
				{
					*((int*)pd) = *((int*)ps);
					pd += 4;
					ps += 4;
				}
				// Complete the copy by moving any bytes that weren't
				// moved in blocks of 4:
				for (count &= 3; count != 0; count--)
				{
					*pd = *ps;
					pd++;
					ps++;
				}
			}
		}

		public static unsafe int FindByte(byte[] src,byte val,int index)
		{
			int n,size = src.Length;

			if(index>size)
			{
				throw new ArgumentException();
			}

			fixed (byte* pSrc = src)
			{
				byte* ps = &pSrc[index];

				for (n = index; n < size; n++)
				{
					if(*ps == val)
					{
						return n-index;
					};
					ps++;
				}
			}
			return -1;
		}

		public static unsafe int FindShort(byte[] src,short val,int index)
		{
			int n,size = src.Length;
			if(index > size)
			{
				throw new ArgumentException();
			}

			fixed (byte* pSrc = src)
			{
				byte* ps = &pSrc[index];

				for (n = (size - index) >> 1; n != 0; n--)
				{
					if(*(short*)ps == val)
					{
						return (((size - index)>>1)-n)*2;
					};
					ps+=2;
				}
			}
			return -1;
		}

	}
}
