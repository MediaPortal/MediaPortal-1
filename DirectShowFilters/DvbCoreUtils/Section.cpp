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
  TableId = 0;
  SectionSyntaxIndicator = true;
  PrivateIndicator = true;
  SectionLength = SECTION_LENGTH_NOT_SET;
  TableIdExtension = 0;
  VersionNumber = 0;
  CurrentNextIndicator = true;
  SectionNumber = -1;
  LastSectionNumber = -1;
  BufferPos = 0;

	table_id = TableId;
  section_length = SectionLength;
  table_id_extension = TableIdExtension;
  version_number = VersionNumber;
}

unsigned short CSection::AppendData(const unsigned char* data, unsigned long dataLength)
{
  if (SectionLength == SECTION_LENGTH_NOT_SET)
  {
    if (BufferPos + dataLength < 3)
    {
      memcpy(&Data[BufferPos], data, dataLength);
      BufferPos += (unsigned short)dataLength;
      return (unsigned short)dataLength;
    }

    if (BufferPos == 0)
    {
      SectionLength = ((data[1] & 0xf) << 8) | data[2];
    }
    else if (BufferPos == 1)
    {
      SectionLength = ((data[0] & 0xf) << 8) | data[1];
    }
    else if (BufferPos == 2)
    {
      SectionLength = ((Data[1] & 0xf) << 8) | data[0];
    }
    section_length = SectionLength;
  }

  unsigned short copyByteCount = SectionLength + 3 - BufferPos;     // + 1 for table ID, + 2 for section length bytes
  if (dataLength < copyByteCount)
  {
    copyByteCount = (unsigned short)dataLength;
  }
  memcpy(&Data[BufferPos], data, copyByteCount);
  BufferPos += copyByteCount;

  if (IsComplete())
  {
    TableId = Data[0];
    SectionSyntaxIndicator = (Data[1] & 0x80) != 0;
    PrivateIndicator = (Data[1] & 0x40) != 0;
    if (SectionSyntaxIndicator)
    {
      TableIdExtension = (Data[3] << 8) | Data[4];
      VersionNumber = (Data[5] >> 1) & 0x1f;
      CurrentNextIndicator = (Data[5] & 1) != 0;
      SectionNumber = Data[6];
      LastSectionNumber = Data[7];
    }

    table_id = TableId;
    table_id_extension = TableIdExtension;
    version_number = VersionNumber;
  }
  return copyByteCount;
}

bool CSection::IsComplete() const
{
  return SectionLength != SECTION_LENGTH_NOT_SET && BufferPos >= section_length + 3;
}

bool CSection::IsValid() const
{
  unsigned long crc = 0;
  // With a few exceptions, only sections with the syntax indicator set have a
  // CRC.
  if (
    SectionSyntaxIndicator ||
    TableId == 0x73 ||   // DVB TOT
    TableId == 0xc5 ||   // SCTE STT
    TableId == 0xcd      // ATSC STT
  )
  {
    // Is the CRC actually populated? Some providers fill the CRC with
    // zeroes or ones instead of setting it correctly.
    const unsigned char* crcPointer = &(Data[SectionLength - 1]);
    if (
      (*crcPointer != 0 && *crcPointer != 0xff) ||
      *crcPointer != *(crcPointer + 1) ||
      *crcPointer != *(crcPointer + 2) ||
      *crcPointer != *(crcPointer + 3)
    )
    {
      crc = CalculatCrc32(Data, SectionLength + 3);   // + 1 for the table ID, + 2 for the section length
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
  TableId = section.TableId;
  SectionSyntaxIndicator = section.SectionSyntaxIndicator;
  PrivateIndicator = section.PrivateIndicator;
  SectionLength = section.SectionLength;
  TableIdExtension = section.TableIdExtension;
  VersionNumber = section.VersionNumber;
  CurrentNextIndicator = section.CurrentNextIndicator;
  SectionNumber = section.SectionNumber;
  LastSectionNumber = section.LastSectionNumber;
  memcpy(Data, section.Data, sizeof(Data));
  BufferPos = section.BufferPos;

	table_id = section.table_id;
  section_length = section.section_length;
  table_id_extension = section.table_id_extension;
  version_number = section.version_number;
}