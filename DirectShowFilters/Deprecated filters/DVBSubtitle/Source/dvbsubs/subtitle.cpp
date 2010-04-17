/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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

#include <stdlib.h>
#include <string>
#include "subtitle.h"

extern void LogDebug(const char *fmt, ...);

//
// Constructor
//
CSubtitle::CSubtitle( int width, int height )
{
	m_Bitmap.bmType			  = 0;
	m_Bitmap.bmBitsPixel	= 32;
	m_Bitmap.bmWidth		  = width;
	m_Bitmap.bmHeight		  = height;
	m_Bitmap.bmPlanes		  = 1;
	m_Bitmap.bmWidthBytes	= width * 4;

  m_Data = NULL;
}

//
// Destructor
//
CSubtitle::~CSubtitle()
{
	if( m_Data )
	{
		delete m_Data;
	}
}


//
// RenderBitmap
//
int CSubtitle::RenderBitmap( unsigned char* buffer, unsigned char* my_palette, unsigned char* my_trans,int col_count )
{
	uint8_t colorData( 0 );
	long position( 0 );
  m_FirstScanline = -1;

	for( int i = 0 ; i < m_Bitmap.bmHeight * m_Bitmap.bmWidth; i++ )
	{
		for( int j = 0 ; j < 3 ; j++ )
		{
			if( m_FirstScanline == -1 )
      {
        if( buffer[i] > 0 )
        {
          m_FirstScanline = i / m_Bitmap.bmWidth;
          m_Bitmap.bmHeight -= m_FirstScanline;
        }
      }
    }
  }

  m_Data = new unsigned char[ m_Bitmap.bmHeight * m_Bitmap.bmWidth *4 ];
  ZeroMemory( m_Data, m_Bitmap.bmHeight * m_Bitmap.bmWidth * 4 );

  m_Bitmap.bmBits	= (LPVOID)m_Data;

	for( int i =  m_Bitmap.bmWidth * m_FirstScanline ; i < ( m_Bitmap.bmHeight + m_FirstScanline ) * m_Bitmap.bmWidth; i++ )
	{
		for( int j = 0 ; j < 3 ; j++ )
		{
      colorData = buffer[i];
			
			int value = my_palette[colorData * 3 + j];
      m_Data[position] = value;
  		position++;

			// Add alpha channel 
      if( j == 2 )
      {      
        m_Data[position] = my_trans[colorData];
        position++;
      }
		}
	}
	return 0;
}


//
// GetBitmap
//
BITMAP* CSubtitle::GetBitmap()
{
	if( &m_Bitmap )
		return &m_Bitmap;
	else
		return NULL;
}


//
// Width
//
int CSubtitle::Width()
{
	return m_Bitmap.bmWidth;
}


//
// Height
//
int CSubtitle::Height()
{
	return m_Bitmap.bmHeight;
}


//
// PTS - presentation timestamp
//
uint64_t CSubtitle::PTS()
{
	return m_PTS;
}


//
// SetPTS
//
void CSubtitle::SetPTS( uint64_t PTS )
{
  m_PTS = PTS;
}


//
// Timestamp
//
uint64_t CSubtitle::Timestamp()
{
	return m_timestamp;
}


//
// SetTimestamp
//
void CSubtitle::SetTimestamp( uint64_t timestamp )
{
	m_timestamp = timestamp;
}


//
// Timeout
//
uint64_t CSubtitle::Timeout()
{
  return m_timeout;
}


//
// SetTimeout
//
void CSubtitle::SetTimeout( uint64_t timeout )
{
  m_timeout = timeout;
}


//
// FirstScanline
//
int CSubtitle::FirstScanline()
{
  return m_FirstScanline;
}
