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


#define PRE_SECTION_BYTE_COUNT 3    // + 1 for table ID, + 2 for section length bytes


extern void LogDebug(const wchar_t* fmt, ...);

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
  SectionLength = 0;
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
  if (BufferPos < PRE_SECTION_BYTE_COUNT)   // is SectionLength set?
  {
    if (BufferPos + dataLength < PRE_SECTION_BYTE_COUNT)
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

  unsigned short copyByteCount = PRE_SECTION_BYTE_COUNT + SectionLength - BufferPos;
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
    if (SectionSyntaxIndicator && BufferPos > 7)
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

bool CSection::IsEmpty() const
{
  return BufferPos == 0;
}

bool CSection::IsComplete() const
{
  return BufferPos >= PRE_SECTION_BYTE_COUNT + SectionLength;
}

bool CSection::IsValid(unsigned short pid) const
{
  if (!IsComplete())
  {
    return false;
  }

  unsigned short expectedMinimumSectionLength = 0;
  if (SectionSyntaxIndicator)
  {
    // These sections include a CRC which can be checked.
    expectedMinimumSectionLength = 9;   // + 5 for table ID extension etc., + 4 for CRC
  }
  else if (
      TableId == 0x73 ||    // DVB TOT
      TableId == 0xc5 ||    // SCTE STT
      TableId == 0xcd       // ATSC STT
  )
  {
    // These sections also include a CRC, even though the section syntax
    // indicator is not set.
    expectedMinimumSectionLength = 4;   // + 4 for CRC
  }
  else
  {
    // No CRC. At this scope we have to assume the section is valid.
    return true;
  }

  if (SectionLength < expectedMinimumSectionLength)
  {
    LogDebug(L"section: invalid section detected by length, PID = %hu", pid);
    Debug();
    return false;
  }

  // Check the CRC.
  unsigned long crc = CalculatCrc32(Data, PRE_SECTION_BYTE_COUNT + SectionLength);
  if (crc == 0)
  {
    return true;
  }

  // It looks like the section might not be valid. However, some providers
  // fill the CRC with zeroes or ones instead of populating it correctly.
  const unsigned char* crcPointer = &(Data[SectionLength - 1]);
  if (
    (*crcPointer == 0 || *crcPointer == 0xff) &&
    *crcPointer == *(crcPointer + 1) &&
    *crcPointer == *(crcPointer + 2) &&
    *crcPointer == *(crcPointer + 3)
  )
  {
    return true;
  }

  LogDebug(L"section: invalid section detected by CRC, PID = %hu, CRC check result = %lu, CRC byte 1 = %hhu, CRC byte 2 = %hhu, CRC byte 3 = %hhu, CRC byte 4 = %hhu",
            pid, crc, *crcPointer, *(crcPointer + 1), *(crcPointer + 2),
            *(crcPointer + 3));
  Debug();
  return false;
}

void CSection::Debug() const
{
  LogDebug(L"  section data, table ID = 0x%hhx, section syntax indicator = %d, private indicator = %d, section length = %hu, table ID extension = %hu, version number = %hhu, current next indicator = %d, section number = %hhu, last section number = %hhu, buffer position = %hu",
            TableId, SectionSyntaxIndicator, PrivateIndicator, SectionLength,
            TableIdExtension, VersionNumber, CurrentNextIndicator,
            SectionNumber, LastSectionNumber, BufferPos);
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