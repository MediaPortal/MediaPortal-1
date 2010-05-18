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
#include <windows.h>
#include "Section.h"
#pragma warning(disable : 4995)

CSection::CSection(void)
{
	Data=new byte[MAX_SECTION_LENGTH*5];
  Reset();
}

CSection::~CSection(void)
{
	delete[] Data;
}

void CSection::Reset()
{
  Version=-1;
  SectionNumber=-1;
  SectionLength=-1;
  SectionPos=0;
  BufferPos=0;
  SectionLength=0;
  NetworkId=-1;
  TransportId=0;
  LastSectionNumber=0;
	Length=0;
}
