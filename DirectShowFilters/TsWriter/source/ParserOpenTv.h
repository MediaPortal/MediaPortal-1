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
#include <vector>
#include <streams.h>    // CUnknown, LPUNKNOWN
#include <WinError.h>   // HRESULT
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\ISectionCallback.h"
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\TsHeader.h"
#include "ICallBackGrabber.h"
#include "ICallBackPidConsumer.h"
#include "IGrabberEpgOpenTv.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


// Usually, in the home/main transport stream:
// [PIDs, table IDs, purpose]
// 0x30 - 0x37  0xa0 - 0xa3  events (1 PID per day, 1 table ID per 6 hour block)
// 0x40 - 0x47  0xa8 - 0xab  descriptions
// 0x50         0xb0         on-demand/PPV events
// 0x51         0xb1         on-demand/PPV descriptions
//
// A subset of the EPG may be available in other transport streams. We derive
// our PIDs from the PMT to minimise PID use.
// I suspect PIDs 0x38 - 0x3f and 0x48 - 0x4f would enable support for a 16 day
// guide, but I've never seen them used.
//
// Other PIDs also contain OpenTV data:
// [PID, table ID]
// 0x52 0xc1
// 0x53 0xc2
// 0x55 0x94 firmware???
// 0x59 0xc0
// 0x60 0xb5
//#define PID_OPENTV_EVENT_START      0x30
//#define PID_OPENTV_EVENT_END        0x37
//#define PID_OPENTV_DESC_START       0x40
//#define PID_OPENTV_DESC_END         0x47
#define PID_OPENTV_CALL_BACK        0x30

#define TABLE_ID_OPENTV_EVENT_START 0xa0
#define TABLE_ID_OPENTV_EVENT_END   0xa3
#define TABLE_ID_OPENTV_DESC_START  0xa8
#define TABLE_ID_OPENTV_DESC_END    0xab
#define TABLE_ID_OPENTV_CALL_BACK   TABLE_ID_OPENTV_EVENT_START


extern void LogDebug(const wchar_t* fmt, ...);

class CParserOpenTv
  : public CUnknown, public IGrabberEpgOpenTv, ISectionCallback
{
  public:
    CParserOpenTv(ICallBackPidConsumer* callBack, LPUNKNOWN unk, HRESULT* hr);
    virtual ~CParserOpenTv();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void SetOriginalNetworkId(unsigned short originalNetworkId);
    void SetPmtPid(unsigned short pid);
    void AddEventDecoders(const vector<unsigned short>& pids);
    void RemoveEventDecoders(const vector<unsigned short>& pids);
    void AddDescriptionDecoders(const vector<unsigned short>& pids);
    void RemoveDescriptionDecoders(const vector<unsigned short>& pids);

    STDMETHODIMP_(void) Start();
    STDMETHODIMP_(void) Stop();

    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(CTsHeader& header, unsigned char* tsPacket);
    STDMETHODIMP_(bool) IsSeen();
    STDMETHODIMP_(bool) IsReady();

    STDMETHODIMP_(unsigned long) GetEventCount();
    STDMETHODIMP_(bool) GetEvent(unsigned long index,
                                  unsigned short* channelId,
                                  unsigned short* eventId,
                                  unsigned long long* startDateTime,
                                  unsigned short* duration,
                                  char* title,
                                  unsigned short* titleBufferSize,
                                  char* shortDescription,
                                  unsigned short* shortDescriptionBufferSize,
                                  char* extendedDescription,
                                  unsigned short* extendedDescriptionBufferSize,
                                  unsigned char* categoryId,
                                  unsigned char* subCategoryId,
                                  bool* isHighDefinition,
                                  bool* hasSubtitles,
                                  unsigned char* parentalRating,
                                  unsigned short* seriesLinkId);

  private:
    class CRecordOpenTvEvent : public IRecord
    {
      public:
        CRecordOpenTvEvent()
        {
          Pid = 0;
          TableId = 0;
          ChannelId = 0;
          EventId = 0;
          StartDateTime = 0;
          Duration = 0;
          Title = NULL;
          CategoryId = 0;
          SubCategoryId = 0;
          IsHighDefinition = false;
          HasSubtitles = false;
          ParentalRating = 0;
        }

        ~CRecordOpenTvEvent()
        {
          if (Title != NULL)
          {
            delete[] Title;
            Title = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordOpenTvEvent* recordEvent = dynamic_cast<const CRecordOpenTvEvent*>(record);
          if (
            recordEvent == NULL ||
            Pid != recordEvent->Pid ||
            TableId != recordEvent->TableId ||
            ChannelId != recordEvent->ChannelId ||
            EventId != recordEvent->EventId ||
            StartDateTime != recordEvent->StartDateTime ||
            Duration != recordEvent->Duration ||
            !CUtils::CompareStrings(Title, recordEvent->Title) ||
            CategoryId != recordEvent->CategoryId ||
            SubCategoryId != recordEvent->SubCategoryId ||
            IsHighDefinition != recordEvent->IsHighDefinition ||
            HasSubtitles != recordEvent->HasSubtitles ||
            ParentalRating != recordEvent->ParentalRating
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Pid << 40) | ((unsigned long long)TableId << 32) | (ChannelId << 16) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return (Pid << 24) | (TableId << 16) | ChannelId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"OpenTV: event %s, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, start date/time = %llu, duration = %hu m, category ID = %hhu, sub-category ID = %hhu, is HD = %d, has subtitles = %d, parental rating = %hhu, title = %S",
                    situation, Pid, TableId, ChannelId, EventId, StartDateTime,
                    Duration, CategoryId, SubCategoryId, IsHighDefinition,
                    HasSubtitles, ParentalRating, Title == NULL ? "" : Title);
        }

        unsigned short Pid;
        unsigned char TableId;
        unsigned short ChannelId;
        unsigned short EventId;
        unsigned long long StartDateTime; // unit = UTC epoch
        unsigned short Duration;          // unit = minutes
        char* Title;
        unsigned char CategoryId;
        unsigned char SubCategoryId;
        bool IsHighDefinition;
        bool HasSubtitles;
        unsigned char ParentalRating;
    };

    class CRecordOpenTvEventDescription : public IRecord
    {
      public:
        CRecordOpenTvEventDescription()
        {
          Pid = 0;
          TableId = 0;
          ChannelId = 0;
          EventId = 0;
          ShortDescription = NULL;
          ExtendedDescription = NULL;
          SeriesLinkId = 0xffff;      // not available
        }

        ~CRecordOpenTvEventDescription()
        {
          if (ShortDescription != NULL)
          {
            delete[] ShortDescription;
            ShortDescription = NULL;
          }
          if (ExtendedDescription != NULL)
          {
            delete[] ExtendedDescription;
            ExtendedDescription = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordOpenTvEventDescription* recordEventDescription = dynamic_cast<const CRecordOpenTvEventDescription*>(record);
          if (
            recordEventDescription == NULL ||
            Pid != recordEventDescription->Pid ||
            TableId != recordEventDescription->TableId ||
            ChannelId != recordEventDescription->ChannelId ||
            EventId != recordEventDescription->EventId ||
            !CUtils::CompareStrings(ShortDescription, recordEventDescription->ShortDescription) ||
            !CUtils::CompareStrings(ExtendedDescription, recordEventDescription->ExtendedDescription) ||
            SeriesLinkId != recordEventDescription->SeriesLinkId
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Pid << 40) | ((unsigned long long)TableId << 32) | (ChannelId << 16) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return ((unsigned long long)Pid << 24) | (TableId << 16) | ChannelId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"OpenTV: event description %s, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, series link ID = %hu, short description = %S, extended description = %S",
                    situation, Pid, TableId, ChannelId, EventId, SeriesLinkId,
                    ShortDescription == NULL ? "" : ShortDescription,
                    ExtendedDescription == NULL ? "" : ExtendedDescription);
        }

        unsigned short Pid;
        unsigned char TableId;
        unsigned short ChannelId;
        unsigned short EventId;
        char* ShortDescription;
        char* ExtendedDescription;
        unsigned short SeriesLinkId;
    };

    void OnNewSection(int pid, int tableId, CSection& section);

    bool AddOrResetDecoder(unsigned short pid, bool enableCrcCheck);
    void CleanUpSections(vector<unsigned short>& keepPids);
    void SwitchToPhase(bool descriptionPhase);

    static bool DecodeEventRecord(unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  bool isItalianText,
                                  bool useAlternativeProgramCategoryHandling,
                                  CRecordOpenTvEvent& record);
    static bool DecodeEventDescriptionRecord(unsigned char* sectionData,
                                              unsigned short& pointer,
                                              unsigned short endOfSection,
                                              bool isItalianText,
                                              CRecordOpenTvEventDescription& record);

    static bool DecodeOpenTvEventDescriptor(unsigned char* data,
                                            unsigned char dataLength,
                                            bool isItalianText,
                                            bool useAlternativeProgramCategoryHandling,
                                            unsigned long& startDateTimeOffset,
                                            unsigned short& duration,
                                            unsigned char& categoryId,
                                            unsigned char& subCategoryId,
                                            bool& isHighDefinition,
                                            bool& hasSubtitles,
                                            unsigned char& parentalRating,
                                            char** title);
    static bool DecodeOpenTvEventDescriptionDescriptor(unsigned char* data,
                                                        unsigned char dataLength,
                                                        bool isItalianText,
                                                        char** description);
    static bool DecodeOpenTvSeriesLinkDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned short& seriesLinkId);

    CCriticalSection m_section;
    vector<unsigned long long> m_seenSections;
    vector<unsigned long long> m_unseenSections;
    bool m_isGrabbing;
    bool m_isItalianText;
    bool m_useAlternativeProgramCategoryHandling;
    bool m_isDescriptionPhase;
    bool m_isReady;
    clock_t m_completeTime;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackPidConsumer* m_callBackPidConsumer;
    unsigned short m_pidPmt;
    vector<unsigned short> m_pidsEvent;
    vector<unsigned short> m_pidsDescription;
    map<unsigned short, CSectionDecoder*> m_decoders;
    CRecordStore m_recordsEvent;
    CRecordStore m_recordsDescription;
    bool m_enableCrcCheck;
};