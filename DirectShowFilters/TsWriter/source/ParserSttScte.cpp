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
#include "ParserSttScte.h"
#include <cstddef>      // NULL
#include "..\..\shared\EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserSttScte::CParserSttScte()
{
  SetCallBack(NULL);
  m_systemTime = 0;
  m_gpsUtcOffset = 0;
  m_isDaylightSavingStateKnown = false;
  m_isDaylightSaving = false;
  m_daylightSavingDayOfMonth = 0;
  m_daylightSavingHour = 0;
}

CParserSttScte::~CParserSttScte()
{
  SetCallBack(NULL);
}

void CParserSttScte::Reset()
{
  LogDebug(L"STT SCTE: reset");
  CEnterCriticalSection lock(m_section);
  m_systemTime = 0;
  m_gpsUtcOffset = 0;
  m_isDaylightSavingStateKnown = false;
  m_isDaylightSaving = false;
  m_daylightSavingDayOfMonth = 0;
  m_daylightSavingHour = 0;
  LogDebug(L"STT SCTE: reset done");
}

void CParserSttScte::SetCallBack(ICallBackStt* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserSttScte::OnNewSection(CSection& section)
{
  try
  {
    if (
      section.table_id != TABLE_ID_STT_SCTE ||
      section.SectionSyntaxIndicator ||
      section.PrivateIndicator
    )
    {
      return;
    }
    if (section.section_length < 11)
    {
      LogDebug(L"STT SCTE: invalid section, length = %d",
                section.section_length);
      return;
    }
    unsigned char protocolVersion = section.Data[3] & 0x1f;
    if (protocolVersion != 0)
    {
      LogDebug(L"STT SCTE: unsupported protocol version, protocol version = %hhu",
                protocolVersion);
      return;
    }

    CEnterCriticalSection lock(m_section);
    unsigned long systemTime = (section.Data[4] << 24) | (section.Data[5] << 16) | (section.Data[6] << 8) | section.Data[7];
    unsigned char gpsUtcOffset = section.Data[8];
    //LogDebug(L"STT SCTE: section length = %d, protocol version = %hhu, system time = %lu, GPS UTC offset = %hhu",
    //          section.section_length, protocolVersion, systemTime,
    //          gpsUtcOffset);

    bool isDaylightSavingStateKnown = false;
    bool isDaylightSaving;
    unsigned char daylightSavingDayOfMonth;
    unsigned char daylightSavingHour;
    unsigned short pointer = 16;
    unsigned short endOfSection = section.section_length - 1; // points to the first byte in the CRC
    while (pointer + 1 < endOfSection)
    {
      unsigned char tag = section.Data[pointer++];
      unsigned char length = section.Data[pointer++];
      //LogDebug(L"STT SCTE: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (pointer + length > endOfSection)
      {
        LogDebug(L"STT SCTE: invalid descriptor, descriptor length = %hhu, pointer = %hu, tag = 0x%hhx, end of section = %hu, protocol version = %hhu",
                  length, pointer, tag, endOfSection, protocolVersion);
        return;
      }

      bool result = true;
      if (tag == 0x96)
      {
        isDaylightSavingStateKnown = true;
        result = DecodeDaylightSavingsTimeDescriptor(&(section.Data[pointer]),
                                                      length,
                                                      isDaylightSaving,
                                                      daylightSavingDayOfMonth,
                                                      daylightSavingHour);
      }

      if (!result)
      {
        LogDebug(L"STT SCTE: invalid descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of section = %hu, protocol version = %hhu",
                  tag, length, pointer, endOfSection, protocolVersion);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"STT SCTE: section parsing error, pointer = %hu, end of section = %hu, protocol version = %hhu",
                pointer, endOfSection, protocolVersion);
    }

    if (m_systemTime == 0)
    {
      LogDebug(L"STT SCTE: received, system time = %lu, GPS UTC offset = %hhu, is DS state known = %d, is DS = %d, DS day of month = %hhu, DS hour = %hhu",
                systemTime, gpsUtcOffset, isDaylightSavingStateKnown,
                isDaylightSaving, daylightSavingDayOfMonth,
                daylightSavingHour);
    }
    else if (
      m_gpsUtcOffset != gpsUtcOffset ||
      m_isDaylightSavingStateKnown != isDaylightSavingStateKnown ||
      m_isDaylightSaving != isDaylightSaving ||
      m_daylightSavingDayOfMonth != daylightSavingDayOfMonth ||
      m_daylightSavingHour != daylightSavingHour
    )
    {
      LogDebug(L"STT SCTE: changed, system time = %lu, GPS UTC offset = %hhu, is DS state known = %d, is DS = %d, DS day of month = %hhu, DS hour = %hhu",
                systemTime, gpsUtcOffset, isDaylightSavingStateKnown,
                isDaylightSaving, daylightSavingDayOfMonth,
                daylightSavingHour);
    }

    if (m_callBack != NULL)
    {
      bool isChangeNotified = false;
      if (systemTime != m_systemTime)
      {
        if (m_systemTime == 0)
        {
          m_callBack->OnTableSeen(TABLE_ID_STT_SCTE);
          isChangeNotified = true;
        }
        else
        {
          m_callBack->OnTableChange(TABLE_ID_STT_SCTE);
          isChangeNotified = true;
        }
        m_callBack->OnSystemTime(systemTime);
      }
      if (gpsUtcOffset != m_gpsUtcOffset)
      {
        if (!isChangeNotified)
        {
          m_callBack->OnTableChange(TABLE_ID_STT_SCTE);
          isChangeNotified = true;
        }
        m_callBack->OnGpsUtcOffset(gpsUtcOffset);
      }
      if (
        isDaylightSaving != m_isDaylightSaving ||
        daylightSavingDayOfMonth != m_daylightSavingDayOfMonth ||
        daylightSavingHour != m_daylightSavingHour
      )
      {
        if (!isChangeNotified)
        {
          m_callBack->OnTableChange(TABLE_ID_STT_SCTE);
          isChangeNotified = true;
        }
        m_callBack->OnDaylightSaving(isDaylightSavingStateKnown,
                                      isDaylightSaving,
                                      daylightSavingDayOfMonth,
                                      daylightSavingHour);
      }
      if (isChangeNotified)
      {
        m_callBack->OnTableComplete(TABLE_ID_STT_SCTE);
      }
    }

    m_systemTime = systemTime;
    m_gpsUtcOffset = gpsUtcOffset;
    m_isDaylightSavingStateKnown = isDaylightSavingStateKnown;
    m_isDaylightSaving = isDaylightSaving;
    m_daylightSavingDayOfMonth = daylightSavingDayOfMonth;
    m_daylightSavingHour = daylightSavingHour;
  }
  catch (...)
  {
    LogDebug(L"STT SCTE: unhandled exception in OnNewSection()");
  }
}

bool CParserSttScte::GetSystemTimeDetail(unsigned long& systemTime,
                                          unsigned char& gpsUtcOffset,
                                          bool& isDaylightSavingStateKnown,
                                          bool& isDaylightSaving,
                                          unsigned char& daylightSavingDayOfMonth,
                                          unsigned char& daylightSavingHour) const
{
  CEnterCriticalSection lock(m_section);
  if (m_systemTime == 0)
  {
    LogDebug(L"STT SCTE: no detail available");
    return false;
  }

  systemTime = m_systemTime;
  gpsUtcOffset = m_gpsUtcOffset;
  isDaylightSavingStateKnown = m_isDaylightSavingStateKnown;
  isDaylightSaving = m_isDaylightSaving;
  daylightSavingDayOfMonth = m_daylightSavingDayOfMonth;
  daylightSavingHour = m_daylightSavingHour;
  return true;
}

bool CParserSttScte::DecodeDaylightSavingsTimeDescriptor(unsigned char* data,
                                                          unsigned char dataLength,
                                                          bool& isDaylightSaving,
                                                          unsigned char& dayOfMonth,
                                                          unsigned char& hour)
{
  if (dataLength != 2)
  {
    LogDebug(L"STT SCTE: invalid daylight savings time descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    isDaylightSaving = (data[0] & 0x80) != 0;
    dayOfMonth = data[0] & 0x1f;
    hour = data[1];
    //LogDebug(L"STT SCTE: daylight savings time descriptor, is daylight saving = %d, day of month = %hhu, hour = %hhu",
    //          isDaylightSaving, dayOfMonth, hour);
    return true;
  }
  catch (...)
  {
    LogDebug(L"STT SCTE: unhandled exception in DecodeDaylightSavingsTimeDescriptor()");
  }
  return false;
}