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
#include <ctime>
#include <map>
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\ISectionDispatcher.h"
#include "..\..\shared\Section.h"
#include "..\..\shared\SectionDecoder.h"
#include "ICallBackTableParser.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_ETT 0xcc


extern void LogDebug(const wchar_t* fmt, ...);

class CParserEtt : public CSectionDecoder
{
  public:
    CParserEtt(unsigned short pid, ISectionDispatcher* sectionDispatcher);
    virtual ~CParserEtt();

    void Reset(bool enableCrcCheck);
    void SetCallBack(ICallBackTableParser* callBack);
    void OnNewSection(const CSection& section);
    bool IsSeen() const;
    bool IsReady() const;

    unsigned char GetSourceTextCount(unsigned short sourceId);
    bool GetSourceTextByIndex(unsigned short sourceId,
                              unsigned char index,
                              unsigned long& language,
                              char* text,
                              unsigned short& textBufferSize);
    bool GetSourceTextByLanguage(unsigned short sourceId,
                                  unsigned long language,
                                  char* text,
                                  unsigned short& textBufferSize);

    unsigned char GetEventTextCount(unsigned short sourceId,
                                    unsigned short eventId);
    bool GetEventTextByIndex(unsigned short sourceId,
                              unsigned short eventId,
                              unsigned char index,
                              unsigned long& language,
                              char* text,
                              unsigned short& textBufferSize);
    bool GetEventTextByLanguage(unsigned short sourceId,
                                unsigned short eventId,
                                unsigned long language,
                                char* text,
                                unsigned short& textBufferSize);

  private:
    class CRecordEtt : public IRecord
    {
      public:
        CRecordEtt()
        {
          Id = 0;
          SourceId = 0;
          EventId = 0;
        }

        ~CRecordEtt()
        {
          CUtils::CleanUpStringSet(Texts);
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordEtt* recordEtt = dynamic_cast<const CRecordEtt*>(record);
          if (
            recordEtt == NULL ||
            Id != recordEtt->Id ||
            SourceId != recordEtt->SourceId ||
            EventId != recordEtt->EventId ||
            !CUtils::CompareStringSets(Texts, recordEtt->Texts)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (SourceId << 16) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return 0;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"ETT: text %s, ID = %hu, source ID = %hu, event ID = %hu, text count = %llu",
                    situation, Id, SourceId, EventId,
                    (unsigned long long)Texts.size());

          CUtils::DebugStringMap(Texts, L"text(s)", L"language", L"text");
        }

        unsigned short Id;
        unsigned short SourceId;
        unsigned short EventId;
        map<unsigned long, char*> Texts;
    };

    bool SelectTextRecordByIds(unsigned short sourceId, unsigned short eventId);

    CCriticalSection m_section;
    vector<unsigned long> m_seenSections;
    unsigned char m_versionNumber;
    bool m_isReady;
    clock_t m_completeTime;
    ICallBackTableParser* m_callBack;
    CRecordStore m_records;

    CRecordEtt* m_currentRecord;
};