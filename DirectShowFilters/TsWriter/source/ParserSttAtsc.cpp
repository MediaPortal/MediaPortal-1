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
#include "ParserSttAtsc.h"
#include <cstddef>      // NULL
#include "..\..\shared\EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserSttAtsc::CParserSttAtsc()
{
  SetCallBack(NULL);
  m_systemTime = 0;
  m_gpsUtcOffset = 0;
  m_isDaylightSaving = false;
  m_daylightSavingDayOfMonth = 0;
  m_daylightSavingHour = 0;
}

CParserSttAtsc::~CParserSttAtsc()
{
  SetCallBack(NULL);
}

void CParserSttAtsc::Reset()
{
  LogDebug(L"STT ATSC: reset");
  CEnterCriticalSection lock(m_section);
  m_systemTime = 0;
  m_gpsUtcOffset = 0;
  m_isDaylightSaving = false;
  m_daylightSavingDayOfMonth = 0;
  m_daylightSavingHour = 0;
  LogDebug(L"STT ATSC: reset done");
}

void CParserSttAtsc::SetCallBack(ICallBackStt* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserSttAtsc::OnNewSection(const CSection& section)
{
  try
  {
    if (
      section.TableId != TABLE_ID_STT_ATSC ||
      !section.SectionSyntaxIndicator ||
      !section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.SectionLength < 17)
    {
      LogDebug(L"STT ATSC: invalid section, length = %hu",
                section.SectionLength);
      return;
    }
    unsigned char protocolVersion = section.Data[8];
    if (protocolVersion != 0)
    {
      LogDebug(L"STT ATSC: unsupported protocol version, protocol version = %hhu",
                protocolVersion);
      return;
    }
    if (section.SectionNumber != 0 || section.LastSectionNumber != 0)
    {
      // According to ATSC A/65 STT should only have one section.
      LogDebug(L"STT ATSC: unsupported multi-section table, extension ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, protocol version = %hhu",
                section.TableIdExtension, section.VersionNumber,
                section.SectionNumber, section.LastSectionNumber,
                protocolVersion);
      return;
    }

    CEnterCriticalSection lock(m_section);
    unsigned long systemTime = (section.Data[9] << 24) | (section.Data[10] << 16) | (section.Data[11] << 8) | section.Data[12];
    unsigned char gpsUtcOffset = section.Data[13];
    bool isDaylightSaving = (section.Data[14] & 0x80) != 0;
    unsigned char daylightSavingDayOfMonth = section.Data[14] & 0x1f;
    unsigned char daylightSavingHour = section.Data[15];
    //LogDebug(L"STT ATSC: extension ID = %hu, version number = %hhu, section length = %hu, section number = %hhu, protocol version = %hhu, system time = %lu, GPS UTC offset = %hhu, is daylight saving = %d, DS day of month = %hhu, DS hour = %hhu",
    //          section.TableIdExtension, section.VersionNumber,
    //          section.SectionLength, section.SectionNumber, protocolVersion,
    //          systemTime, gpsUtcOffset, isDaylightSaving,
    //          daylightSavingDayOfMonth, daylightSavingHour);

    unsigned short pointer = 16;
    unsigned short endOfSection = section.SectionLength - 1;  // points to the first byte in the CRC
    while (pointer + 1 < endOfSection)
    {
      unsigned char tag = section.Data[pointer++];
      unsigned char length = section.Data[pointer++];
      //LogDebug(L"STT ATSC: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (pointer + length > endOfSection)
      {
        LogDebug(L"STT ATSC: invalid table, descriptor length = %hhu, pointer = %hu, tag = 0x%hhx, end of section = %hu, extension ID = %hu, version number = %hhu, section number = %hhu, protocol version = %hhu",
                  length, pointer, tag, endOfSection, section.TableIdExtension,
                  section.VersionNumber, section.SectionNumber,
                  protocolVersion);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"STT ATSC: section parsing error, pointer = %hu, end of section = %hu, extension ID = %hu, version number = %hhu, section number = %hhu, protocol version = %hhu",
                pointer, endOfSection, section.TableIdExtension,
                section.VersionNumber, section.SectionNumber,
                protocolVersion);
    }

    if (m_systemTime == 0)
    {
      LogDebug(L"STT ATSC: received, system time = %lu, GPS UTC offset = %hhu, is DS = %d, DS day of month = %hhu, DS hour = %hhu",
                systemTime, gpsUtcOffset, isDaylightSaving,
                daylightSavingDayOfMonth, daylightSavingHour);
    }
    else if (
      m_gpsUtcOffset != gpsUtcOffset ||
      m_isDaylightSaving != isDaylightSaving ||
      m_daylightSavingDayOfMonth != daylightSavingDayOfMonth ||
      m_daylightSavingHour != daylightSavingHour
    )
    {
      LogDebug(L"STT ATSC: changed, system time = %lu, GPS UTC offset = %hhu, is DS = %d, DS day of month = %hhu, DS hour = %hhu",
                systemTime, gpsUtcOffset, isDaylightSaving,
                daylightSavingDayOfMonth, daylightSavingHour);
    }

    if (m_callBack != NULL)
    {
      bool isChangeNotified = false;
      if (systemTime != m_systemTime)
      {
        if (m_systemTime == 0)
        {
          m_callBack->OnTableSeen(TABLE_ID_STT_ATSC);
          isChangeNotified = true;
        }
        else
        {
          m_callBack->OnTableChange(TABLE_ID_STT_ATSC);
          isChangeNotified = true;
        }
        m_callBack->OnSystemTime(systemTime);
      }
      if (gpsUtcOffset != m_gpsUtcOffset)
      {
        if (!isChangeNotified)
        {
          m_callBack->OnTableChange(TABLE_ID_STT_ATSC);
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
          m_callBack->OnTableChange(TABLE_ID_STT_ATSC);
          isChangeNotified = true;
        }
        m_callBack->OnDaylightSaving(true,
                                      isDaylightSaving,
                                      daylightSavingDayOfMonth,
                                      daylightSavingHour);
      }
      if (isChangeNotified)
      {
        m_callBack->OnTableComplete(TABLE_ID_STT_ATSC);
      }
    }

    m_systemTime = systemTime;
    m_gpsUtcOffset = gpsUtcOffset;
    m_isDaylightSaving = isDaylightSaving;
    m_daylightSavingDayOfMonth = daylightSavingDayOfMonth;
    m_daylightSavingHour = daylightSavingHour;
  }
  catch (...)
  {
    LogDebug(L"STT ATSC: unhandled exception in OnNewSection()");
  }
}

bool CParserSttAtsc::GetSystemTimeDetail(unsigned long& systemTime,
                                          unsigned char& gpsUtcOffset,
                                          bool& isDaylightSavingStateKnown,
                                          bool& isDaylightSaving,
                                          unsigned char& daylightSavingDayOfMonth,
                                          unsigned char& daylightSavingHour) const
{
  CEnterCriticalSection lock(m_section);
  if (m_systemTime == 0)
  {
    LogDebug(L"STT ATSC: no detail available");
    return false;
  }

  systemTime = m_systemTime;
  gpsUtcOffset = m_gpsUtcOffset;
  isDaylightSavingStateKnown = true;
  isDaylightSaving = m_isDaylightSaving;
  daylightSavingDayOfMonth = m_daylightSavingDayOfMonth;
  daylightSavingHour = m_daylightSavingHour;
  return true;
}