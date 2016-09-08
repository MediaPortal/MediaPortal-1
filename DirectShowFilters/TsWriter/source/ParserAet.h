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
#include <streams.h>    // CUnknown, LPUNKNOWN
#include <vector>
#include <WinError.h>   // HRESULT
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\ISectionCallback.h"
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\TsHeader.h"
#include "ICallBackGrabber.h"
#include "ICallBackPidConsumer.h"
#include "IGrabberEpgScte.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_AEIT           0xd6
#define TABLE_ID_AETT           0xd7

#define PID_AET_CALL_BACK       TABLE_ID_AEIT
#define TABLE_ID_AET_CALL_BACK  TABLE_ID_AEIT


extern void LogDebug(const wchar_t* fmt, ...);

class CParserAet
  : public CUnknown, public IGrabberEpgScte, public ISectionCallback
{
  public:
    CParserAet(ICallBackPidConsumer* callBack, LPUNKNOWN unk, HRESULT* hr);
    virtual ~CParserAet();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void AddDecoders(const vector<unsigned short>& pids);
    void RemoveDecoders(const vector<unsigned short>& pids);

    STDMETHODIMP_(void) Start();
    STDMETHODIMP_(void) Stop();

    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(CTsHeader& header, unsigned char* tsPacket);
    void OnNewSection(int pid, int tableId, CSection& section);
    STDMETHODIMP_(bool) IsSeen();
    STDMETHODIMP_(bool) IsReady();

    STDMETHODIMP_(unsigned long) GetEventCount();
    STDMETHODIMP_(bool) GetEvent(unsigned long index,
                                  unsigned short* sourceId,
                                  unsigned short* eventId,
                                  unsigned long long* startDateTime,
                                  unsigned short* duration,
                                  unsigned char* textCount,
                                  unsigned long* audioLanguages,
                                  unsigned char* audioLanguageCount,
                                  unsigned long* captionsLanguages,
                                  unsigned char* captionsLanguageCount,
                                  unsigned char* genreIds,
                                  unsigned char* genreIdCount,
                                  unsigned char* vchipRating,
                                  unsigned char* mpaaClassification,
                                  unsigned short* advisories);
    STDMETHODIMP_(bool) GetEventTextByIndex(unsigned long eventIndex,
                                            unsigned char textIndex,
                                            unsigned long* language,
                                            char* title,
                                            unsigned short* titleBufferSize,
                                            char* text,
                                            unsigned short* textBufferSize);
    STDMETHODIMP_(bool) GetEventTextByLanguage(unsigned long eventIndex,
                                                unsigned long language,
                                                char* title,
                                                unsigned short* titleBufferSize,
                                                char* text,
                                                unsigned short* textBufferSize);

  private:
    class CRecordAeit : public IRecord
    {
      public:
        CRecordAeit()
        {
          Pid = 0;
          MgtTag = 0;
          SourceId = 0;
          EventId = 0;
          StartDateTime = 0;
          EtmPresent = 0;
          Duration = 0;
          VchipRating = 0xff;         // default: [not available]
          MpaaClassification = 0xff;  // default: [not available]
          Advisories = 0;               // default: [not available]
        }

        ~CRecordAeit()
        {
          CUtils::CleanUpStringSet(Titles);
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordAeit* recordAeit = dynamic_cast<const CRecordAeit*>(record);
          if (
            recordAeit == NULL ||
            Pid != recordAeit->Pid ||
            MgtTag != recordAeit->MgtTag ||
            SourceId != recordAeit->SourceId ||
            EventId != recordAeit->EventId ||
            StartDateTime != recordAeit->StartDateTime ||
            EtmPresent != recordAeit->EtmPresent ||
            Duration != recordAeit->Duration ||
            !CUtils::CompareStringSets(Titles, recordAeit->Titles) ||
            !CUtils::CompareVectors(AudioLanguages, recordAeit->AudioLanguages) ||
            !CUtils::CompareVectors(CaptionsLanguages, recordAeit->CaptionsLanguages) ||
            !CUtils::CompareVectors(GenreIds, recordAeit->GenreIds) ||
            VchipRating != recordAeit->VchipRating ||
            MpaaClassification != recordAeit->MpaaClassification ||
            Advisories != recordAeit->Advisories
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)MgtTag << 32) | (SourceId << 16) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return MgtTag;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"AEIT: event %s, PID = %hu, MGT tag = %hhu, source ID = %hu, event ID = %hu, start date/time = %llu, duration = %hu m, ETM present = %hhu, title count = %llu, audio language count = %llu, captions language count = %llu, genre count = %llu, V-CHIP rating = %hhu, MPAA classification = %hhu, advisories = %hu",
                    situation, Pid, MgtTag, SourceId, EventId, StartDateTime,
                    Duration, EtmPresent, (unsigned long long)Titles.size(),
                    (unsigned long long)AudioLanguages.size(),
                    (unsigned long long)CaptionsLanguages.size(),
                    (unsigned long long)GenreIds.size(), VchipRating,
                    MpaaClassification, Advisories);

          CUtils::DebugStringMap(Titles, L"title(s)", L"language", L"title");
          CUtils::DebugVector(AudioLanguages, L"audio language(s)", true);
          CUtils::DebugVector(CaptionsLanguages, L"captions language(s)", true);
          CUtils::DebugVector(GenreIds, L"genre ID(s)", false);
        }

        unsigned short Pid;
        unsigned char MgtTag;
        unsigned short SourceId;
        unsigned short EventId;
        unsigned long long StartDateTime; // unit = UTC epoch
        unsigned char EtmPresent;
        unsigned short Duration;          // unit = minutes
        map<unsigned long, char*> Titles;
        vector<unsigned long> AudioLanguages;
        vector<unsigned long> CaptionsLanguages;
        vector<unsigned char> GenreIds;
        unsigned char VchipRating;
        unsigned char MpaaClassification;
        unsigned short Advisories;
    };

    class CRecordAett : public IRecord
    {
      public:
        CRecordAett()
        {
          Pid = 0;
          MgtTag = 0;
          SourceId = 0;
          EventId = 0;
        }

        ~CRecordAett()
        {
          CUtils::CleanUpStringSet(Texts);
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordAett* recordAett = dynamic_cast<const CRecordAett*>(record);
          if (
            recordAett == NULL ||
            Pid != recordAett->Pid ||
            MgtTag != recordAett->MgtTag ||
            SourceId != recordAett->SourceId ||
            EventId != recordAett->EventId ||
            !CUtils::CompareStringSets(Texts, recordAett->Texts)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)MgtTag << 32) | (SourceId << 16) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return MgtTag;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"AETT: text %s, PID = %hu, MGT tag = %hhu, source ID = %hu, event ID = %hu, text count = %llu",
                    situation, Pid, MgtTag, SourceId, EventId,
                    (unsigned long long)Texts.size());

          CUtils::DebugStringMap(Texts, L"text(s)", L"language", L"text");
        }

        unsigned short Pid;
        unsigned char MgtTag;
        unsigned short SourceId;
        unsigned short EventId;
        map<unsigned long, char*> Texts;
    };

    bool SelectEventRecordByIndex(unsigned long index);

    void PrivateReset(bool removeDecoders);

    static bool DecodeAeitRecord(unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  CRecordAeit& record);
    static bool DecodeAettRecord(unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  CRecordAett& record);

    static bool DecodeAc3AudioStreamDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                vector<unsigned long>& audioLanguages);
    static bool DecodeCaptionServiceDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                vector<unsigned long>& captionsLanguages);
    static bool DecodeContentAdvisoryDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                unsigned char& vchipRating,
                                                unsigned char& mpaaClassification,
                                                unsigned short& advisories);
    static bool DecodeGenreDescriptor(unsigned char* data,
                                      unsigned char dataLength,
                                      vector<unsigned char>& genreIds);
    static bool DecodeEnhancedAc3AudioDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  vector<unsigned long>& audioLanguages);

    CCriticalSection m_section;
    bool m_isGrabbing;
    vector<unsigned long long> m_seenSections;
    vector<unsigned long long> m_unseenSections;
    bool m_isReady;
    clock_t m_completeTime;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackPidConsumer* m_callBackPidConsumer;
    map<unsigned short, CSectionDecoder*> m_decoders;
    CRecordStore m_recordsAeit;
    CRecordStore m_recordsAett;
    bool m_enableCrcCheck;

    CRecordAeit* m_currentRecordAeit;
    CRecordAett* m_currentRecordAett;
    unsigned long m_currentRecordIndex;
};