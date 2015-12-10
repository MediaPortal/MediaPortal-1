/* 
 *	Copyright (C) 2006-2010 Team MediaPortal
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
#include <cstring>    // memcpy()
#include "..\shared\DvbUtil.h"
#include "..\shared\Section.h"


#define SECTION_LENGTH_NOT_SET 0


CSection::CSection(void)
{
  Reset();
}

CSection::~CSection(void)
{
}

void CSection::Reset()
{
	table_id = -1;
  SectionSyntaxIndicator = true;
  PrivateIndicator = true;
  section_length = SECTION_LENGTH_NOT_SET;
  table_id_extension = -1;
  version_number = -1;
  CurrentNextIndicator = true;
  SectionNumber = -1;
  LastSectionNumber = -1;
  BufferPos = 0;
}

unsigned short CSection::AppendData(const unsigned char* data, unsigned long dataLength)
{
  if (section_length == SECTION_LENGTH_NOT_SET)
  {
    if (BufferPos + dataLength < 3)
    {
      memcpy(&Data[BufferPos], data, dataLength);
      BufferPos += (unsigned short)dataLength;
      return (unsigned short)dataLength;
    }

    if (BufferPos == 0)
    {
      section_length = ((data[1] & 0xf) << 8) | data[2];
    }
    else if (BufferPos == 1)
    {
      section_length = ((data[0] & 0xf) << 8) | data[1];
    }
    else if (BufferPos == 2)
    {
      section_length = ((Data[1] & 0xf) << 8) | data[0];
    }
  }

  unsigned short copyByteCount = section_length + 3 - BufferPos;   // + 1 for table ID, + 2 for section length bytes
  if (dataLength < copyByteCount)
  {
    copyByteCount = (unsigned short)dataLength;
  }
  memcpy(&Data[BufferPos], data, copyByteCount);
  BufferPos += copyByteCount;

  if (IsComplete())
  {
    table_id = Data[0];
    SectionSyntaxIndicator = (Data[1] & 0x80) != 0;
    PrivateIndicator = (Data[1] & 0x40) != 0;
    if (SectionSyntaxIndicator)
    {
      table_id_extension = (Data[3] << 8) | Data[4];
      version_number = (Data[5] >> 1) & 0x1f;
      CurrentNextIndicator = (Data[5] & 1) != 0;
      SectionNumber = Data[6];
      LastSectionNumber = Data[7];
    }
  }
  return copyByteCount;
}

bool CSection::IsComplete()
{
  return section_length != SECTION_LENGTH_NOT_SET && BufferPos >= section_length + 3;
}

bool CSection::IsValid()
{
  unsigned long crc = 0;
  // Only sections with the syntax indicator set have a CRC.
  if (SectionSyntaxIndicator)
  {
    // Is the CRC actually populated? Some providers fill the CRC with
    // zeroes or ones instead of setting it correctly.
    unsigned char* crcPointer = &(Data[section_length - 1]);
    if (
      (*crcPointer != 0 && *crcPointer != 0xff) ||
      *crcPointer != *(crcPointer + 1) ||
      *crcPointer != *(crcPointer + 2) ||
      *crcPointer != *(crcPointer + 3)
    )
    {
      crc = CalculatCrc32(Data, section_length + 3);
    }
  }
  return crc == 0;
}

CSection& CSection::operator = (const CSection& section)
{
  if (&section == this)
  {
    return *this;
  }
  Copy(section);
  return *this;
}

void CSection::Copy(const CSection &section)
{
	table_id = section.table_id;
  SectionSyntaxIndicator = section.SectionSyntaxIndicator;
  PrivateIndicator = section.PrivateIndicator;
  section_length = section.section_length;
  table_id_extension = section.table_id_extension;
  version_number = section.version_number;
  CurrentNextIndicator = section.CurrentNextIndicator;
  SectionNumber = section.SectionNumber;
  LastSectionNumber = section.LastSectionNumber;
  memcpy(Data, section.Data, sizeof(Data));
  BufferPos = section.BufferPos;
}