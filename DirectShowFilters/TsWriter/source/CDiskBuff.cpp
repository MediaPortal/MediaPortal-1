/* 
*	Copyright (C) 2006-2016 Team MediaPortal
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

#include <windows.h>
#include "CDiskBuff.h"

///*******************************************
///Class which holds a single disk buffer
///

CDiskBuff::CDiskBuff(int size)
{
  m_iLength=0;
  m_pBuffer = new byte[size];
  m_iSize = size;
}

CDiskBuff::~CDiskBuff()
{
  delete [] m_pBuffer;
  m_pBuffer=NULL;
  m_iLength=0;
}

// Adds data to this buffer
int CDiskBuff::Add(byte* data, int len)
{
  if((m_iSize >= m_iLength + len ) && data) 
  {
    memcpy(&m_pBuffer[m_iLength], data, len);
    m_iLength+=len;
    return 0; //All data written
  }
  else
  {
    return len; //No data written/consumed
  }
}

// returns the length in bytes of the buffer
int CDiskBuff::Length()
{
  return m_iLength;
}

// returns the buffer
byte* CDiskBuff::Data()
{
  return m_pBuffer;
}

