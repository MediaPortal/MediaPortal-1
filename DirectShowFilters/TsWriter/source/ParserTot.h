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
#include <vector>
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\Section.h"
#include "..\..\shared\SectionDecoder.h"
#include "ICallBackTot.h"
#include "IRecord.h"
#include "ISystemTimeInfoProviderDvb.h"
#include "RecordStore.h"

using namespace MediaPortal;
using namespace std;


#define PID_TOT 0x14
#define TABLE_ID_TOT 0x73


extern void LogDebug(const wchar_t* fmt, ...);

class CParserTot : public CSectionDecoder, public ISystemTimeInfoProviderDvb
{
  public:
    CParserTot();
    virtual ~CParserTot();

    void Reset(bool enableCrcCheck);
    void SetCallBack(ICallBackTot* callBack);
    void OnNewSection(CSection& section);

    bool GetSystemTimeDetail(unsigned long long& systemTime,
                              unsigned char& localTimeOffsetCount) const;

    bool GetLocalTimeOffsetByIndex(unsigned char index,
                                    unsigned long& countryCode,
                                    unsigned char& countryRegionId,
                                    long& localTimeOffsetCurrent,
                                    unsigned long long& localTimeOffsetNextChangeDateTime,
                                    long& localTimeOffsetNext) const;
    bool GetLocalTimeOffsetByCountryAndRegion(unsigned long countryCode,
                                              unsigned char countryRegionId,
                                              long& localTimeOffsetCurrent,
                                              unsigned long long& localTimeOffsetNextChangeDateTime,
                                              long& localTimeOffsetNext) const;

  private:
    class CRecordLocalTimeOffset : public IRecord
    {
      public:
        CRecordLocalTimeOffset()
        {
          CountryCode = 0;
          CountryRegionId = 0;
          CurrentOffset = 0;
          NextChangeTime = 0;
          NextOffset = 0;
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordLocalTimeOffset* recordLto = dynamic_cast<const CRecordLocalTimeOffset*>(record);
          if (
            recordLto == NULL ||
            CountryCode != recordLto->CountryCode ||
            CountryRegionId != recordLto->CountryRegionId ||
            CurrentOffset != recordLto->CurrentOffset ||
            NextChangeTime != recordLto->NextChangeTime ||
            NextOffset != recordLto->NextOffset
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)CountryCode << 8) | CountryRegionId;
        }

        unsigned long long GetExpiryKey() const
        {
          return 0;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"TOT: local time offset %s, country code = %S, region ID = %hhu, current = %ld m, next change time = %llu, next = %ld m",
                    situation, (char*)&CountryCode, CountryRegionId,
                    CurrentOffset, NextChangeTime, NextOffset);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackTot* callBackTot = static_cast<ICallBackTot*>(callBack);
          if (callBackTot != NULL)
          {
            callBackTot->OnTotReceived(CountryCode,
                                        CountryRegionId,
                                        CurrentOffset,
                                        NextChangeTime,
                                        NextOffset);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackTot* callBackTot = static_cast<ICallBackTot*>(callBack);
          if (callBackTot != NULL)
          {
            callBackTot->OnTotChanged(CountryCode,
                                        CountryRegionId,
                                        CurrentOffset,
                                        NextChangeTime,
                                        NextOffset);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackTot* callBackTot = static_cast<ICallBackTot*>(callBack);
          if (callBackTot != NULL)
          {
            callBackTot->OnTotRemoved(CountryCode,
                                        CountryRegionId,
                                        CurrentOffset,
                                        NextChangeTime,
                                        NextOffset);
          }
        }

        unsigned long CountryCode;
        unsigned char CountryRegionId;
        long CurrentOffset;                 // unit = minutes
        unsigned long long NextChangeTime;  // epoch/Unix/POSIX time-stamp
        long NextOffset;                    // unit = minutes
    };

    static bool DecodeLocalTimeOffsetDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                vector<CRecordLocalTimeOffset*>& localTimeOffsets);

    CCriticalSection m_section;
    ICallBackTot* m_callBack;
    unsigned long long m_systemTime;
    CRecordStore m_records;
};