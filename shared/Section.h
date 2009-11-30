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
#pragma once

#define MAX_SECTION_LENGTH 4300
class CSection
{
public:
  CSection(void);
  virtual ~CSection(void);
  void   Reset();
  bool	 DecodeHeader();
	int		 CalcSectionLength(byte* tsPacket, int start);
  bool   SectionComplete();

	int table_id;
	int table_id_extension;
	int section_length;
	int section_number;
	int version_number;
	int section_syntax_indicator;
	int last_section_number;

	int BufferPos;
  byte Data[MAX_SECTION_LENGTH*5];
};
