/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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
#pragma once

// structure used to communicate subtitles to MediaPortal's managed code
struct SUBTITLE
{
  // Subtitle bitmap
  LONG        bmType;
  LONG        bmWidth;
  LONG        bmHeight;
  LONG        bmWidthBytes;
  WORD        bmPlanes;
  WORD        bmBitsPixel;
  LPVOID      bmBits;

  LONG        screenWidth;
  LONG        screenHeight;

  unsigned    __int64 timestamp;
  unsigned    __int64 timeOut;
  int         firstScanLine;
};

struct TEXT_SUBTITLE
{
	int character_table;
	LPCSTR language;
	int page;
	LPCSTR text;
	int firstLine;  // can be 0 to (totalLines - 1)
	int totalLines; // for teletext this is 25 lines

	unsigned    __int64 timestamp;
	unsigned    __int64 timeOut;
};

struct DVBLANG
{
	DVBLANG()
  {
	}
	DVBLANG(byte b1, byte b2, byte b3)
  {
		lang[0] = b1;	
		lang[1] = b2;
		lang[2] = b3;
	}

	byte lang[3];
};
