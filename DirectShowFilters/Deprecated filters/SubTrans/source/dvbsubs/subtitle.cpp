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

//#define _AFXDLL
#include <stdlib.h>
#include <string>
//#include <afx.h>
//#include <afxwin.h>
#include "subtitle.h"

extern void Log(const char *fmt, ...);

CSubtitle::CSubtitle( int width, int height )
{
	m_Data = new unsigned char[ height * width * 3 ];

	m_Bitmap.bmBitsPixel	= 24;
	m_Bitmap.bmWidth		= width;
	m_Bitmap.bmHeight		= height;
	m_Bitmap.bmPlanes		= 1;
	m_Bitmap.bmWidthBytes	= height * width;
	m_Bitmap.bmBits			= (LPVOID)m_Data;

	ZeroMemory( m_Data, width * height * 3 );
}

CSubtitle::~CSubtitle()
{
	if( m_Data )
	{
		delete m_Data;
	}
}

BITMAP* CSubtitle::GetBitmap()
{
	if( &m_Bitmap )
		return &m_Bitmap;
	else
		return NULL;
}

unsigned char*  CSubtitle::GetData()
{
	return m_Data;
}

int CSubtitle::GetData( int pos )
{
	return (int)m_Data[pos];
}

int CSubtitle::RenderBitmap( unsigned char* buffer, char *file_name, unsigned char* my_palette, 
	unsigned char* my_trans,int col_count )
{
	uint8_t colorData( 0 );
	long position( 0 );
  m_FirstScanline = -1;
/*	BITMAPINFOHEADER bmi;
	BITMAPFILEHEADER bfi;

	ZeroMemory(&bmi,sizeof(BITMAPINFOHEADER));
	bmi.biSize			= sizeof(BITMAPINFOHEADER);
	bmi.biHeight		= m_Bitmap.bmHeight;
	bmi.biWidth			= m_Bitmap.bmWidth;
	bmi.biSizeImage		= m_Bitmap.bmWidth*m_Bitmap.bmHeight*3;
	bmi.biBitCount		= 24;
	bmi.biCompression	= BI_RGB;
	bmi.biPlanes		= 1;
	
	bfi.bfType			= ((WORD) ('M' << 8) | 'B');
	bfi.bfSize			= sizeof(bfi)+bmi.biSizeImage;
	bfi.bfReserved1		= 0;
	bfi.bfReserved2		= 0;
	bfi.bfOffBits		= (DWORD) (sizeof(bfi)+sizeof(bmi.biSize));
*/	
	for( int i = 0 ; i < m_Bitmap.bmHeight * m_Bitmap.bmWidth; i++ )
	{
		for( int j = 0 ; j < 3 ; j++ )
		{
			if( m_FirstScanline == -1 )
      {
        if( buffer[i] > 0 )
        {
          m_FirstScanline = i / m_Bitmap.bmWidth;
          //Log("Subtitle::RenderBitmap - First scanline that contains subtitle picture %d", m_FirstScanline );
        }
      }

      colorData = buffer[i];
			
			int value = my_palette[colorData * 3 + j];

			// transparent color? Not handled properly yet!
			/*if( my_trans[colorData] == 0 )
			{
				value = 0;			
			}*/ 
				
			m_Data[ position ] = value;
			position++;
		}
	}

		
	//char file_name_tmp[500];

	//strcpy( file_name_tmp, "d:\\test_output\\" );
	//strncat( file_name_tmp, file_name, 29 );

	//FILE* file = fopen( file_name_tmp, "w+" );
	//CFile file;
	
	//if (!file.Open(file_name_tmp,CFile::modeWrite|CFile::modeCreate))
	//if(!file)
	//	return 1;

	//file.Write(&bfi, sizeof(bfi));
	// Create debug PPM image file
	//fprintf( file, "P3 %d %d %d\n", m_Bitmap.bmWidth, m_Bitmap.bmHeight, col_count - 1 );
	
	/*for( int k = 0 ; k < m_Bitmap.bmHeight * m_Bitmap.bmWidth * 3 ; k++ )
	{
		if( k != 0 && k % ( m_Bitmap.bmWidth * 3 ) == 0 )
		{
			fprintf( file, "\n" );
		}

		fprintf( file, "%d ", m_Data[k] );
	}*/
	//file.Write(&bmi, sizeof(bmi));
	//file.Write(m_Data, bmi.biSizeImage);
	//file.Close();
/*
	fprintf( file, "\n" );	
	fclose(file);
*/
	return 0;
}

int CSubtitle::Width()
{
	return m_Bitmap.bmWidth;
}

int CSubtitle::Height()
{
	return m_Bitmap.bmHeight;
}

uint64_t CSubtitle::PTS()
{
	return m_PTS;
}

void CSubtitle::SetPTS( uint64_t PTS )
{
	//Log("Subtitle::SetPTS %lld", PTS );
	m_PTS = PTS;
}

int CSubtitle::FirstScanline()
{
  return m_FirstScanline;
}
