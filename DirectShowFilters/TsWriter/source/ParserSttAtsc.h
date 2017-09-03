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
#pragma once
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\Section.h"
#include "ICallBackStt.h"
#include "ISystemTimeInfoProviderAtscScte.h"

using namespace MediaPortal;


#define TABLE_ID_STT_ATSC 0xcd


class CParserSttAtsc : public ISystemTimeInfoProviderAtscScte
{
  public:
    CParserSttAtsc();
    virtual ~CParserSttAtsc();

    void Reset();
    void SetCallBack(ICallBackStt* callBack);
    void OnNewSection(const CSection& section);

    bool GetSystemTimeDetail(unsigned long& systemTime,
                              unsigned char& gpsUtcOffset,
                              bool& isDaylightSavingStateKnown,
                              bool& isDaylightSaving,
                              unsigned char& daylightSavingDayOfMonth,
                              unsigned char& daylightSavingHour) const;

  private:
    CCriticalSection m_section;
    ICallBackStt* m_callBack;
    unsigned long m_systemTime;     // GPS time
    unsigned char m_gpsUtcOffset;
    bool m_isDaylightSaving;
    unsigned char m_daylightSavingDayOfMonth;
    unsigned char m_daylightSavingHour;
};