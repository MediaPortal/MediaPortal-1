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


#define TABLE_ID_STT_SCTE 0xc5


class CParserSttScte : public ISystemTimeInfoProviderAtscScte
{
  public:
    CParserSttScte();
    virtual ~CParserSttScte();

    void Reset();
    void SetCallBack(ICallBackStt* callBack);
    void OnNewSection(CSection& section);

    bool GetSystemTimeDetail(unsigned long& systemTime,
                              unsigned char& gpsUtcOffset,
                              bool& isDaylightSavingStateKnown,
                              bool& isDaylightSaving,
                              unsigned char& daylightSavingDayOfMonth,
                              unsigned char& daylightSavingHour) const;

  private:
    static bool DecodeDaylightSavingsTimeDescriptor(unsigned char* data,
                                                    unsigned char dataLength,
                                                    bool& isDaylightSaving,
                                                    unsigned char& dayOfMonth,
                                                    unsigned char& hour);

    CCriticalSection m_section;
    ICallBackStt* m_callBack;
    unsigned long m_systemTime;     // GPS time
    unsigned char m_gpsUtcOffset;
    bool m_isDaylightSavingStateKnown;
    bool m_isDaylightSaving;
    unsigned char m_daylightSavingDayOfMonth;
    unsigned char m_daylightSavingHour;
};