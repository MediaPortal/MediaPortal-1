/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *  Author: tourettes
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

#ifndef _BITMAP_H
#define _BITMAP_H

#include <windows.h>

typedef unsigned __int64 uint64_t;
typedef unsigned __int16 uint16_t;
typedef unsigned __int8 uint8_t;

class CSubtitle
{
public:

	CSubtitle( int width, int height );
	
	~CSubtitle();
	BITMAP m_Bitmap;
	BITMAP* GetBitmap();

	int RenderBitmap( unsigned char* buffer, char *file_name, 
		unsigned char* my_palette, unsigned char* my_trans, int col_count );
	
	int Width();
	
	int Height();

	uint64_t PTS();
	
	void SetPTS( uint64_t PTS );

  int FirstScanline();

	unsigned char* GetData(); 

	int CSubtitle::GetData( int pos );

	unsigned char* m_Data;

  int m_FirstScanline;

private:
	
	uint64_t m_PTS;
};
#endif
