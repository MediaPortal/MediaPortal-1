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

namespace id3
{
	/// <summary>
	/// Summary description for Swap.
	/// </summary>
	public class Swap
	{
		public static unsafe int Int32(int val)
		{
			byte* pVal = (byte*)&val;
			byte swap = pVal[3];
			pVal[3] = pVal[0];
			pVal[0] = swap;
			swap = pVal[2];
			pVal[2] = pVal[1];
			pVal[1] = swap;
			return val;
		}

		public static unsafe uint UInt32(uint val)
		{
			byte* pVal = (byte*)&val;
			byte swap = pVal[3];
			pVal[3] = pVal[0];
			pVal[0] = swap;
			swap = pVal[2];
			pVal[2] = pVal[1];
			pVal[1] = swap;
			return val;
		}

		public static unsafe short Int16(short val)
		{
			byte* pVal = (byte*)&val;
			byte swap = pVal[1];
			pVal[1] = pVal[0];
			pVal[0] = swap;
			return val;
		}

		public static unsafe ushort UInt16(ushort val)
		{
			byte* pVal = (byte*)&val;
			byte swap = pVal[1];
			pVal[1] = pVal[0];
			pVal[0] = swap;
			return val;
		}
	}
}
