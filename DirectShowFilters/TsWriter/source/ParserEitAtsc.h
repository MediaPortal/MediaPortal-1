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
#include <vector>
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


#define TABLE_ID_EIT_ATSC 0xcb


extern void LogDebug(const wchar_t* fmt, ...);

class CParserEitAtsc : public CSectionDecoder
{
  public:
    CParserEitAtsc(unsigned short pid, ISectionDispatcher* sectionDispatcher);
    virtual ~CParserEitAtsc();

    void Reset(bool enableCrcCheck);
    void SetCallBack(ICallBackTableParser* callBack);
    void OnNewSection(const CSection& section);
    bool IsSeen() const;
    bool IsReady() const;

    unsigned long GetEventCount() const;
    bool GetEvent(unsigned long index,
                  unsigned short& sourceId,
                  unsigned short& eventId,
                  unsigned long& startDateTime,
                  unsigned long& duration,
                  unsigned char& titleCount,
                  unsigned long* audioLanguages,
                  unsigned char& audioLanguageCount,
                  unsigned long* captionsLanguages,
                  unsigned char& captionsLanguageCount,
                  unsigned char* genreIds,
                  unsigned char& genreIdCount,
                  unsigned char& vchipRating,
                  unsigned char& mpaaClassification,
                  unsigned short& advisories);
    bool GetEventTitleByIndex(unsigned long eventIndex,
                              unsigned char titleIndex,
                              unsigned long& language,
                              char* title,
                              unsigned short& titleBufferSize);
    bool GetEventTitleByLanguage(unsigned long eventIndex,
                                  unsigned long language,
                                  char* title,
                                  unsigned short& titleBufferSize);
    bool GetEventIdentifiers(unsigned long index, unsigned short& sourceId, unsigned short& id);

  private:
    class CRecordEit : public IRecord
    {
      public:
        CRecordEit()
        {
          SourceId = 0;
          EventId = 0;
          StartDateTime = 0;
          EtmLocation = 0;
          Duration = 0;
          VchipRating = 0xff;         // default: [not available]
          MpaaClassification = 0xff;  // default: [not available]
          Advisories = 0;             // default: [not available]
        }

        ~CRecordEit()
        {
          CUtils::CleanUpStringSet(Titles);
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordEit* recordEit = dynamic_cast<const CRecordEit*>(record);
          if (
            recordEit == NULL ||
            SourceId != recordEit->SourceId ||
            EventId != recordEit->EventId ||
            StartDateTime != recordEit->StartDateTime ||
            EtmLocation != recordEit->EtmLocation ||
            Duration != recordEit->Duration ||
            !CUtils::CompareStringSets(Titles, recordEit->Titles) ||
            !CUtils::CompareVectors(AudioLanguages, recordEit->AudioLanguages) ||
            !CUtils::CompareVectors(CaptionsLanguages, recordEit->CaptionsLanguages) ||
            !CUtils::CompareVectors(GenreIds, recordEit->GenreIds) ||
            VchipRating != recordEit->VchipRating ||
            MpaaClassification != recordEit->MpaaClassification ||
            Advisories != recordEit->Advisories
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
          return SourceId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"ATSC EIT: event %s, source ID = %hu, event ID = %hu, start date/time = %lu, duration = %lu s, ETM location = %hhu, title count = %llu, audio language count = %llu, captions language count = %llu, genre count = %llu, V-CHIP rating = %hhu, MPAA classification = %hhu, advisories = %hu",
                    situation, SourceId, EventId, StartDateTime, Duration,
                    EtmLocation, (unsigned long long)Titles.size(),
                    (unsigned long long)AudioLanguages.size(),
                    (unsigned long long)CaptionsLanguages.size(),
                    (unsigned long long)GenreIds.size(), VchipRating,
                    MpaaClassification, Advisories);

          CUtils::DebugStringMap(Titles, L"title(s)", L"language", L"title");
          CUtils::DebugVector(AudioLanguages, L"audio language(s)", true);
          CUtils::DebugVector(CaptionsLanguages, L"captions language(s)", true);
          CUtils::DebugVector(GenreIds, L"genre ID(s)", false);
        }

        unsigned short SourceId;
        unsigned short EventId;
        unsigned long StartDateTime;  // GPS time-stamp
        unsigned char EtmLocation;
        unsigned long Duration;       // unit = seconds
        map<unsigned long, char*> Titles;
        vector<unsigned long> AudioLanguages;
        vector<unsigned long> CaptionsLanguages;
        vector<unsigned char> GenreIds;
        unsigned char VchipRating;
        unsigned char MpaaClassification;
        unsigned short Advisories;
    };

    bool SelectEventRecordByIndex(unsigned long index);

    static bool DecodeEventRecord(const unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  CRecordEit& record);

    static bool DecodeAc3AudioStreamDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                vector<unsigned long>& audioLanguages);
    static bool DecodeCaptionServiceDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                vector<unsigned long>& captionsLanguages);
    static bool DecodeContentAdvisoryDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                unsigned char& vchipRating,
                                                unsigned char& mpaaClassification,
                                                unsigned short& advisories);
    static bool DecodeGenreDescriptor(const unsigned char* data,
                                      unsigned char dataLength,
                                      vector<unsigned char>& genreIds);
    static bool DecodeEnhancedAc3AudioDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  vector<unsigned long>& audioLanguages);

    CCriticalSection m_section;
    vector<unsigned long> m_seenSections;
    vector<unsigned long> m_unseenSections;
    bool m_isReady;
    clock_t m_completeTime;
    ICallBackTableParser* m_callBack;
    CRecordStore m_records;

    CRecordEit* m_currentRecord;
    unsigned long m_currentRecordIndex;
};