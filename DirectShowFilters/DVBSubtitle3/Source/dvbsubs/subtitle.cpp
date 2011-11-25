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

#define _AFXDLL
#include <afx.h>
#include <afxwin.h>

#include <stdlib.h>
#include <string>
#include "subtitle.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);

static int count = 0;

//
// Constructor
//
CSubtitle::CSubtitle( int subW, int subH, int screenW, int screenH )
{
  m_bitmap.bmType			  = 0;
  m_bitmap.bmBitsPixel	= 32;
  m_bitmap.bmWidth		  = subW;
  m_bitmap.bmHeight		  = subH;
  m_bitmap.bmPlanes		  = 1;
  m_bitmap.bmWidthBytes	= subW * 4; // 32 bits per pixel

  m_screenHeight = screenH;
  m_screenWidth = screenW;

  m_firstScanline = 0;
  m_horizontalPosition = 0;

  count++;
  //LogDebug("CSubtitle:: CREATE count %d width %d height %d", count, width, height);

  m_Data = new unsigned char[ m_bitmap.bmHeight * m_bitmap.bmWidth *4 ];
  ZeroMemory( m_Data, m_bitmap.bmHeight * m_bitmap.bmWidth * 4 );
  m_bitmap.bmBits	= (LPVOID)m_Data;
}

//
// Destructor
//
CSubtitle::~CSubtitle()
{
  count--;
  //LogDebug("CSubtitle::~CSubtitle() count %d", count);
  delete[] m_Data;
}


//
// RenderBitmap
//
int CSubtitle::RenderBitmap( unsigned char* buffer, unsigned char* my_palette, unsigned char* my_trans, int col_count )
{
  uint8_t colorData( 0 );
  long position( 0 );
  m_firstScanline = -1;

  for( int i = 0 ; i < m_bitmap.bmHeight * m_bitmap.bmWidth; i++ )
  {
    for( int j = 0 ; j < 3 ; j++ )
    {
      if( m_firstScanline == -1 )
      {
        if( buffer[i] > 0 )
        {
          m_firstScanline = i / m_bitmap.bmWidth;
          m_bitmap.bmHeight -= m_firstScanline;
        }
      }
    }
  }

  m_bitmap.bmBits	= (LPVOID)m_Data;

  for( int i =  m_bitmap.bmWidth * m_firstScanline ; i < ( m_bitmap.bmHeight + m_firstScanline ) * m_bitmap.bmWidth; i++ )
  {
    if( i < 0 )
    {
      // Could happen when no display definition segment has arrived yet and we have HD subtitles.
      // Just ignore the optimization then.
      i = 0; 
    }
    
    colorData = buffer[i];
    for( int j = 0 ; j < 3 ; j++ )
    {
      m_Data[position] = my_palette[colorData * 3 + j];
      position++;

      // Add alpha channel 
      if( j == 2 )
      {      
        m_Data[position] = my_trans[colorData];
        position++;
      }
    }
  }
/*
  char file_name_tmp[500];

  strcpy( file_name_tmp, "d:\\test_output\\" );
  strncat( file_name_tmp, "test.ppm", 29 );

  FILE* file = fopen( file_name_tmp, "w+" );

  // Create debug PPM image file
  fprintf( file, "P3 %d %d %d\n", m_bitmap.bmWidth, m_bitmap.bmHeight * 4, col_count - 1 );
  
  for( int k = 0 ; k < m_bitmap.bmHeight * m_bitmap.bmWidth ; k++ )
  {
    for( int x = 0 ; x < 4 ; x++ )
    {    
      if( k != 0 && k % ( m_bitmap.bmWidth * 4 ) == 0 )
      {
        fprintf( file, "\n" );
      }
    
      if( x != 3 )
      {
        fprintf( file, "%d ", m_Data[k] );
      }
    }
  }

  fprintf( file, "\n" );	
  fclose(file);
*/
  return 0;
}


//
// ProvideBits
//
void CSubtitle::DrawRect(SHORT nX, SHORT nY, SHORT nCount, int R, int G, int B, int alpha)
{
  long pos = nX*4 + nY*m_bitmap.bmWidthBytes;
  for( int i=0; i<nCount ; i++)
  {
    m_Data[pos+i*4]   = B;
    m_Data[pos+i*4+1] = G;
    m_Data[pos+i*4+2] = R;
    m_Data[pos+i*4+3] = alpha;
  }
}

//
// GetBitmap
//
BITMAP* CSubtitle::GetBitmap()
{
  if( &m_bitmap )
    return &m_bitmap;
  else
    return NULL;
}


//
// Width
//
int CSubtitle::Width()
{
  return m_bitmap.bmWidth;
}


//
// Height
//
int CSubtitle::Height()
{
  return m_bitmap.bmHeight;
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
// SetFirstScanline
//
void CSubtitle::SetFirstScanline( int firstScanLine)
{
  m_firstScanline = firstScanLine;
}


//
// FirstScanline
//
int CSubtitle::FirstScanline()
{
  return m_firstScanline;
}


//
// SetHorizontalPosition
//
void CSubtitle::SetHorizontalPosition( int x )
{
  m_horizontalPosition = x;
}


//
// HorizontalPosition
//
int CSubtitle::HorizontalPosition()
{
  return m_horizontalPosition;
}


//
// ScreenHeight
//
int CSubtitle::ScreenHeight()
{
  return m_screenHeight;
}


//
// ScreenWidth
//
int CSubtitle::ScreenWidth()
{
  return m_screenWidth;
}