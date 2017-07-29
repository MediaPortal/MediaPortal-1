/*
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include "ParserTot.h"
#include <cstddef>      // NULL
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\TimeUtils.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserTot::CParserTot() : m_records(600000)
{
  m_systemTime = 0;

  SetPid(PID_TOT);
  SetCallBack(NULL);
}

CParserTot::~CParserTot()
{
  SetCallBack(NULL);
}

void CParserTot::Reset(bool enableCrcCheck)
{
  LogDebug(L"TOT: reset");
  CEnterCriticalSection lock(m_section);
  m_records.RemoveAllRecords();
  EnableCrcCheck(enableCrcCheck);
  CSectionDecoder::Reset();
  m_systemTime = 0;
  LogDebug(L"TOT: reset done");
}

void CParserTot::SetCallBack(ICallBackTot* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserTot::OnNewSection(CSection& section)
{
  try
  {
    if (
      section.table_id != TABLE_ID_TOT ||
      section.SectionSyntaxIndicator ||
      !section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.section_length < 11)
    {
      LogDebug(L"TOT: invalid section, length = %d", section.section_length);
      return;
    }

    CEnterCriticalSection lock(m_section);
    unsigned short systemDateMjd = (section.Data[3] << 8) | section.Data[4];
    unsigned long systemTimeBcd = (section.Data[5] << 16) | (section.Data[6] << 8) | section.Data[7];
    unsigned long long systemTime = CTimeUtils::DecodeMjDateBcdTime(systemDateMjd, systemTimeBcd);
    unsigned short descriptorsLoopLength = ((section.Data[8] & 0xf) << 8) | section.Data[9];
    //LogDebug(L"TOT: section length = %d, system time = %llu, descriptors loop length = %hu",
    //          section.section_length, systemTime, descriptorsLoopLength);

    vector<CRecordLocalTimeOffset*> offsets;
    unsigned short pointer = 10;
    unsigned short endOfSection = pointer + descriptorsLoopLength; // points to the first byte in the CRC
    while (pointer + 1 < endOfSection)
    {
      unsigned char tag = section.Data[pointer++];
      unsigned char length = section.Data[pointer++];
      //LogDebug(L"TOT: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (pointer + length > endOfSection)
      {
        LogDebug(L"TOT: invalid descriptor, descriptor length = %hhu, pointer = %hu, tag = 0x%hhx, descriptors loop length = %hu, end of section = %hu",
                  length, pointer, tag, descriptorsLoopLength, endOfSection);
        return;
      }

      bool result = true;
      if (tag == 0x58)
      {
        result = DecodeLocalTimeOffsetDescriptor(&(section.Data[pointer]),
                                                  length,
                                                  offsets);
      }

      if (!result)
      {
        LogDebug(L"TOT: invalid descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, descriptors loop length = %hu, end of section = %hu",
                  tag, length, pointer, descriptorsLoopLength, endOfSection);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"TOT: section parsing error, pointer = %hu, end of section = %hu, descriptors loop length = %hu",
                pointer, endOfSection, descriptorsLoopLength);
    }

    if (m_callBack != NULL && m_systemTime == 0)
    {
      m_callBack->OnTableSeen(TABLE_ID_TOT);
    }

    bool isChange = false;
    m_records.MarkExpiredRecords(0);
    vector<CRecordLocalTimeOffset*>::const_iterator it = offsets.begin();
    for ( ; it != offsets.end(); it++)
    {
      CRecordLocalTimeOffset* offset = *it;
      isChange |= m_records.AddOrUpdateRecord((IRecord**)&offset, m_callBack);
    }
    isChange |= (m_records.RemoveExpiredRecords(m_callBack) > 0);

    if (m_callBack != NULL)
    {
      if (m_systemTime != 0 && isChange)
      {
        m_callBack->OnTableChange(TABLE_ID_TOT);
      }

      m_callBack->OnTableComplete(TABLE_ID_TOT);
    }

    m_systemTime = systemTime;
  }
  catch (...)
  {
    LogDebug(L"TOT: unhandled exception in OnNewSection()");
  }
}

bool CParserTot::GetSystemTimeDetail(unsigned long long& systemTime,
                                      unsigned char& localTimeOffsetCount) const
{
  CEnterCriticalSection lock(m_section);
  systemTime = m_systemTime;
  localTimeOffsetCount = (unsigned char)m_records.GetRecordCount();
  return m_systemTime != 0;
}

bool CParserTot::GetLocalTimeOffsetByIndex(unsigned char index,
                                            unsigned long& countryCode,
                                            unsigned char& countryRegionId,
                                            long& localTimeOffsetCurrent,
                                            unsigned long long& localTimeOffsetNextChangeDateTime,
                                            long& localTimeOffsetNext) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_records.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"TOT: invalid local time offset index, index = %hhu, offset count = %lu",
              index, m_records.GetRecordCount());
    return false;
  }

  CRecordLocalTimeOffset* offset = dynamic_cast<CRecordLocalTimeOffset*>(record);
  if (offset == NULL)
  {
    LogDebug(L"TOT: invalid local time offset record, index = %hhu", index);
    return false;
  }

  countryCode = offset->CountryCode;
  countryRegionId = offset->CountryRegionId;
  localTimeOffsetCurrent = offset->CurrentOffset;
  localTimeOffsetNextChangeDateTime = offset->NextChangeTime;
  localTimeOffsetNext = offset->NextOffset;
  return true;
}

bool CParserTot::GetLocalTimeOffsetByCountryAndRegion(unsigned long countryCode,
                                                      unsigned char countryRegionId,
                                                      long& localTimeOffsetCurrent,
                                                      unsigned long long& localTimeOffsetNextChangeDateTime,
                                                      long& localTimeOffsetNext) const
{
  CEnterCriticalSection lock(m_section);
  unsigned long long key = ((unsigned long long)countryCode << 8) | countryRegionId;
  IRecord* record = NULL;
  if (!m_records.GetRecordByKey(key, &record) || record == NULL)
  {
    // Not an error. It is entirely possible that we don't have the details for
    // the requested offset.
    return false;
  }

  CRecordLocalTimeOffset* offset = dynamic_cast<CRecordLocalTimeOffset*>(record);
  if (offset == NULL)
  {
    LogDebug(L"TOT: invalid local time offset record, country code = %S, country region ID = %hhu",
              (char*)&countryCode, countryRegionId);
    return false;
  }

  localTimeOffsetCurrent = offset->CurrentOffset;
  localTimeOffsetNextChangeDateTime = offset->NextChangeTime;
  localTimeOffsetNext = offset->NextOffset;
  return true;
}

bool CParserTot::DecodeLocalTimeOffsetDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  vector<CRecordLocalTimeOffset*>& localTimeOffsets)
{
  if (dataLength == 0 || dataLength % 13 != 0)
  {
    LogDebug(L"TOT: invalid local time offset descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    //LogDebug(L"TOT: local time offset descriptor, length = %hhu", dataLength);
    unsigned char pointer = 0;
    while (dataLength - pointer >= 13)
    {
      CRecordLocalTimeOffset* offset = new CRecordLocalTimeOffset();
      if (offset == NULL)
      {
        LogDebug(L"TOT: failed to allocate local time offset record");
        return true;
      }

      offset->CountryCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      offset->CountryRegionId = data[pointer] >> 2;
      bool localTimeOffsetPolarity = (data[pointer++] & 1) != 0;

      // Convert BCD HH:MM to minutes.
      offset->CurrentOffset = 60 * (((data[pointer] >> 4) * 10) + (data[pointer] & 0x0f));
      pointer++;
      offset->CurrentOffset += ((data[pointer] >> 4) * 10) + (data[pointer] & 0x0f);
      pointer++;

      unsigned short dateOfChangeMjd = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned long timeOfChangeBcd = (data[pointer] << 16) | (data[pointer + 1] << 8) | data[pointer + 2];
      pointer += 3;
      offset->NextChangeTime = CTimeUtils::DecodeMjDateBcdTime(dateOfChangeMjd, timeOfChangeBcd);

      // Convert BCD HH:MM to minutes.
      offset->NextOffset = 60 * (((data[pointer] >> 4) * 10) + (data[pointer] & 0x0f));
      pointer++;
      offset->NextOffset += ((data[pointer] >> 4) * 10) + (data[pointer] & 0x0f);
      pointer++;

      if (localTimeOffsetPolarity)
      {
        offset->CurrentOffset *= -1;
        offset->NextOffset *= -1;
      }
      localTimeOffsets.push_back(offset);

      //LogDebug(L"  offset, country code = %S, country region ID = %hhu, local time offset = %ld m, next change time = %llu, next time offset = %ld m",
      //          (char*)&(offset->CountryCode), offset->CountryRegionId,
      //          offset->CurrentOffset, offset->NextChangeTime,
      //          offset->NextOffset);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"TOT: unhandled exception in DecodeLocalTimeOffsetDescriptor()");
  }
  return false;
}