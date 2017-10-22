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
#include "ICallBackMhw.h"
#include "ICallBackPidConsumer.h"
#include "IGrabberEpgMhw.h"
#include "IMhwChannelInfoProvider.h"
#include "IRecord.h"
#include "ISystemTimeInfoProviderDvb.h"
#include "MhwProvider.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define PID_MHW1_EVENTS                         0xd2
#define PID_MHW1_OTHER                          0xd3
#define PID_MHW2_CHANNELS_AND_THEMES            0x231
#define PID_MHW2_EVENTS_BY_CHANNEL_TERRESTRIAL  0x232
#define PID_MHW2_EVENTS_BY_CHANNEL_SATELLITE    0x233
#define PID_MHW2_EVENTS_BY_THEME                0x234
#define PID_MHW2_EVENTS_BY_UNKNOWN              0x235
#define PID_MHW2_DESCRIPTIONS                   0x236
#define PID_MHW2_THEME_DESCRIPTIONS             0x26b
#define PID_MHW2_PROGRAMS_AND_SERIES            0x282
#define PID_MHW_CALL_BACK                       PID_MHW1_EVENTS

#define TABLE_ID_MHW1_DESCRIPTIONS              0x90
#define TABLE_ID_MHW1_EVENTS                    0x90
#define TABLE_ID_MHW1_CHANNELS                  0x91
#define TABLE_ID_MHW1_THEMES                    0x92
#define TABLE_ID_MHW2_DESCRIPTIONS              0x96
#define TABLE_ID_MHW2_PROGRAMS                  0x96
#define TABLE_ID_MHW2_SERIES                    0x97
#define TABLE_ID_MHW2_CHANNELS_AND_THEMES       0xc8
#define TABLE_ID_MHW2_EVENTS                    0xdc
#define TABLE_ID_MHW2_THEME_DESCRIPTIONS        0xdd
#define TABLE_ID_MHW2_EVENTS_BY_THEME           0xe6
#define TABLE_ID_MHW2_EVENTS_BY_UNKNOWN_1       0xf0
#define TABLE_ID_MHW2_EVENTS_BY_UNKNOWN_2       0xf1
#define TABLE_ID_MHW2_EVENTS_BY_UNKNOWN_3       0xf2
#define TABLE_ID_MHW2_EVENTS_BY_UNKNOWN_4       0xf3
#define TABLE_ID_MHW_CALL_BACK                  TABLE_ID_MHW1_DESCRIPTIONS


extern void LogDebug(const wchar_t* fmt, ...);

class CParserMhw
  : public CUnknown, ICallBackMhw, public IGrabberEpgMhw,
    public IMhwChannelInfoProvider, ISectionCallback
{
  public:
    CParserMhw(ICallBackPidConsumer* callBack,
                ISystemTimeInfoProviderDvb* systemTimeInfoProvider,
                LPUNKNOWN unk,
                HRESULT* hr);
    virtual ~CParserMhw();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void SetTransportStream(unsigned short originalNetworkId,
                            unsigned short transportStreamId);

    STDMETHODIMP_(void) SetProtocols(bool grabMhw1, bool grabMhw2);
    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket);
    STDMETHODIMP_(bool) IsSeen();
    STDMETHODIMP_(bool) IsReady();

    STDMETHODIMP_(void) GetEventCount(unsigned long* eventCount,
                                      unsigned long* textLanguage);
    STDMETHODIMP_(bool) GetEvent(unsigned long index,
                                  unsigned char* version,
                                  unsigned long* eventId,
                                  unsigned short* originalNetworkId,
                                  unsigned short* transportStreamId,
                                  unsigned short* serviceId,
                                  char* serviceName,
                                  unsigned short* serviceNameBufferSize,
                                  unsigned long long* startDateTime,
                                  unsigned short* duration,
                                  char* title,
                                  unsigned short* titleBufferSize,
                                  char* description,
                                  unsigned short* descriptionBufferSize,
                                  unsigned char* descriptionLineCount,
                                  unsigned long* seriesId,
                                  char* seasonName,
                                  unsigned short* seasonNameBufferSize,
                                  unsigned long* episodeId,
                                  unsigned short* episodeNumber,
                                  char* episodeName,
                                  unsigned short* episodeNameBufferSize,
                                  char* themeName,
                                  unsigned short* themeNameBufferSize,
                                  char* subThemeName,
                                  unsigned short* subThemeNameBufferSize,
                                  unsigned char* classification,
                                  bool* isRecommended,
                                  unsigned long* payPerViewId);
    STDMETHODIMP_(bool) GetDescriptionLine(unsigned long eventIndex,
                                            unsigned char lineIndex,
                                            char* line,
                                            unsigned short* lineBufferSize);

    bool GetService(unsigned short originalNetworkId,
                    unsigned short transportStreamId,
                    unsigned short serviceId,
                    bool* isHighDefinition,
                    bool* isStandardDefinition,
                    unsigned short* categoryIds,
                    unsigned char* categoryIdCount) const;
    bool GetChannelCategoryName(unsigned short categoryId,
                                char* name,
                                unsigned short* nameBufferSize) const;

  private:
    class CRecordMhwChannel : public IRecord
    {
      public:
        CRecordMhwChannel()
        {
          Version = 1;
          Id = 0;
          OriginalNetworkId = 0;
          TransportStreamId = 0;
          ServiceId = 0;
          Name = NULL;
          IsTerrestrial = false;
          IsHighDefinition = false;
          IsStandardDefinition = false;
          IsPayPerView = false;
        }

        ~CRecordMhwChannel()
        {
          if (Name != NULL)
          {
            delete[] Name;
            Name = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwChannel* recordChannel = dynamic_cast<const CRecordMhwChannel*>(record);
          if (
            recordChannel == NULL ||
            Version != recordChannel->Version ||
            Id != recordChannel->Id ||
            OriginalNetworkId != recordChannel->OriginalNetworkId ||
            TransportStreamId != recordChannel->TransportStreamId ||
            ServiceId != recordChannel->ServiceId ||
            !CUtils::CompareStrings(Name, recordChannel->Name) ||
            IsTerrestrial != recordChannel->IsTerrestrial ||
            IsHighDefinition != recordChannel->IsHighDefinition ||
            IsStandardDefinition != recordChannel->IsStandardDefinition ||
            IsPayPerView != recordChannel->IsPayPerView ||
            !CUtils::CompareVectors(CategoryIds, recordChannel->CategoryIds)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (Version << 9) | ((IsTerrestrial ? 1 : 0) << 8) | Id;
        }

        unsigned long long GetExpiryKey() const
        {
          return (Version << 1) | (IsTerrestrial ? 1 : 0);
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: channel %s, version = %hhu, ID = %hhu, ONID = %hu, TSID = %hu, service ID = %hu, is terrestrial = %d, is HD = %d, is SD = %d, is PPV = %d, category ID count = %llu, name = %S",
                    situation, Version, Id, OriginalNetworkId,
                    TransportStreamId, ServiceId, IsTerrestrial,
                    IsHighDefinition, IsStandardDefinition, IsPayPerView,
                    (unsigned long long)CategoryIds.size(),
                    Name == NULL ? "" : Name);

          CUtils::DebugVector(CategoryIds, L"category ID(s)", false);
        }

        unsigned char Version;
        unsigned char Id;
        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;
        unsigned short ServiceId;
        char* Name;

        // v2 only
        bool IsTerrestrial;
        bool IsHighDefinition;
        bool IsStandardDefinition;
        bool IsPayPerView;
        vector<unsigned char> CategoryIds;
    };

    class CRecordMhwChannelCategory : public IRecord
    {
      public:
        CRecordMhwChannelCategory()
        {
          Version = 1;
          Id = 0;
          Name = NULL;
          IsTerrestrial = false;
        }

        ~CRecordMhwChannelCategory()
        {
          if (Name != NULL)
          {
            delete[] Name;
            Name = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwChannelCategory* recordChannelCategory = dynamic_cast<const CRecordMhwChannelCategory*>(record);
          if (
            recordChannelCategory == NULL ||
            Version != recordChannelCategory->Version ||
            Id != recordChannelCategory->Id ||
            !CUtils::CompareStrings(Name, recordChannelCategory->Name) ||
            IsTerrestrial != recordChannelCategory->IsTerrestrial
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (Version << 9) | ((IsTerrestrial ? 1 : 0) << 8) | Id;
        }

        unsigned long long GetExpiryKey() const
        {
          return (Version << 1) | (IsTerrestrial ? 1 : 0);
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: channel category %s, version = %hhu, ID = %hhu, is terrestrial = %d, name = %S",
                    situation, Version, Id, IsTerrestrial,
                    Name == NULL ? "" : Name);
        }

        unsigned char Version;
        unsigned char Id;
        char* Name;
        bool IsTerrestrial;
    };

    class CRecordMhwDescription : public IRecord
    {
      public:
        CRecordMhwDescription()
        {
          Version = 1;
          Id = 0;
          Description = NULL;
          ThemeId = 0;
          SubThemeId = 0;
          ThemeDescriptionId = 0;
          Classification = 0xff;
        }

        ~CRecordMhwDescription()
        {
          if (Description != NULL)
          {
            delete[] Description;
            Description = NULL;
          }

          vector<char*>::const_iterator lineIt = Lines.begin();
          for ( ; lineIt != Lines.end(); lineIt++)
          {
            char* line = *lineIt;
            if (line != NULL)
            {
              delete[] line;
              line = NULL;
            }
          }
          Lines.clear();
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwDescription* recordDescription = dynamic_cast<const CRecordMhwDescription*>(record);
          if (
            recordDescription == NULL ||
            Version != recordDescription->Version ||
            Id != recordDescription->Id ||
            !CUtils::CompareStrings(Description, recordDescription->Description) ||
            Lines.size() != recordDescription->Lines.size() ||
            ThemeId != recordDescription->ThemeId ||
            SubThemeId != recordDescription->SubThemeId ||
            ThemeDescriptionId != recordDescription->ThemeDescriptionId ||
            Classification != recordDescription->Classification
          )
          {
            return false;
          }

          for (unsigned char i = 0; i < Lines.size(); i++)
          {
            if (!CUtils::CompareStrings(Lines[i], recordDescription->Lines[i]))
            {
              return false;
            }
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Version << 32) | Id;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: description %s, version = %hhu, ID = %lu, description = %S, theme ID = %hhu, sub-theme ID = %hhu, theme description ID = %hu, classification = %hhu, line count = %llu",
                    situation, Version, Id,
                    Description == NULL ? "" : Description, ThemeId,
                    SubThemeId, ThemeDescriptionId, Classification,
                    (unsigned long long)Lines.size());

          vector<char*>::const_iterator lineIt = Lines.begin();
          for ( ; lineIt != Lines.end(); lineIt++)
          {
            char* line = *lineIt;
            LogDebug(L"  %S", line == NULL ? "" : line);
          }
        }

        unsigned char Version;
        unsigned long Id;
        char* Description;

        // v2 only
        vector<char*> Lines;
        unsigned char ThemeId;
        unsigned char SubThemeId;
        unsigned short ThemeDescriptionId;

        unsigned char Classification;   // 0 = SC, 1 = TP, 2 = ???, 3 = +18, 4 = X, 5 = ???, 6 = +7, 7 = INF, 8 = +12, 9 = +13 / +16
    };

    class CRecordMhwEvent : public IRecord
    {
      public:
        CRecordMhwEvent()
        {
          Version = 1;
          Segment = 0;
          EventId = 0;
          DescriptionId = 0;
          HasDescription = false;
          ChannelId = 0;
          StartDateTime = 0;
          Duration = 0;
          Title = NULL;
          PayPerViewId = 0;
          IsTerrestrial = false;
          ProgramId = 0xffffffff;
          ShowingId = 0xffffffff;
          ThemeId = 0;
          SubThemeId = 0;
        }

        ~CRecordMhwEvent()
        {
          if (Title != NULL)
          {
            delete[] Title;
            Title = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwEvent* recordEvent = dynamic_cast<const CRecordMhwEvent*>(record);
          if (
            recordEvent == NULL ||
            Version != recordEvent->Version ||
            EventId != recordEvent->EventId ||
            DescriptionId != recordEvent->DescriptionId ||
            HasDescription != recordEvent->HasDescription ||
            ChannelId != recordEvent->ChannelId ||
            StartDateTime != recordEvent->StartDateTime ||
            Duration != recordEvent->Duration ||
            !CUtils::CompareStrings(Title, recordEvent->Title) ||
            PayPerViewId != recordEvent->PayPerViewId ||
            IsTerrestrial != recordEvent->IsTerrestrial ||
            ProgramId != recordEvent->ProgramId ||
            ShowingId != recordEvent->ShowingId ||
            ThemeId != recordEvent->ThemeId ||
            SubThemeId != recordEvent->SubThemeId
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Version << 40) | ((unsigned long long)ChannelId << 32) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return (Version << 8) | Segment;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: event %s, version = %hhu, event ID = %lu, description ID = %lu, has description = %d, channel ID = %hhu, is terrestrial = %d, program ID = %lu, showing ID = %lu, start date/time = %llu, duration = %hu m, theme ID = %hhu, sub-theme ID = %hhu, PPV ID = %lu, title = %S",
                    situation, Version, EventId, DescriptionId, HasDescription,
                    ChannelId, IsTerrestrial, ProgramId, ShowingId,
                    StartDateTime, Duration, ThemeId, SubThemeId, PayPerViewId,
                    Title == NULL ? "" : Title);
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackMhw* callBackMhw = static_cast<ICallBackMhw*>(callBack);
          if (callBackMhw != NULL)
          {
            callBackMhw->OnMhwEventRemoved(Version,
                                            Segment,
                                            EventId,
                                            DescriptionId,
                                            HasDescription,
                                            ChannelId,
                                            StartDateTime,
                                            Duration,
                                            Title,
                                            PayPerViewId,
                                            IsTerrestrial,
                                            ProgramId,
                                            ShowingId,
                                            ThemeId,
                                            SubThemeId);
          }
        }

        unsigned char Version;
        unsigned char Segment;
        unsigned long EventId;
        unsigned long DescriptionId;
        bool HasDescription;
        unsigned char ChannelId;
        unsigned long long StartDateTime; // epoch/Unix/POSIX time-stamp
        unsigned short Duration;          // unit = minutes
        char* Title;                      // for example "Los Simpson (HD) (T19)"

        // v1 only
        unsigned long PayPerViewId;

        // v2 only
        bool IsTerrestrial;
        unsigned long ProgramId;
        unsigned long ShowingId;

        unsigned char ThemeId;            // v1 and v2 event-by-theme
        unsigned char SubThemeId;         // v2 event-by-theme
    };

    class CRecordMhwProgram : public IRecord
    {
      public:
        CRecordMhwProgram()
        {
          Version = 2;
          ProgramId = 0;
          ThemeDescriptionId = 0;
          Classification = 0xff;
          Title = NULL;
          Description = NULL;
        }

        ~CRecordMhwProgram()
        {
          if (Title != NULL)
          {
            delete[] Title;
            Title = NULL;
          }
          if (Description != NULL)
          {
            delete[] Description;
            Description = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwProgram* recordProgram = dynamic_cast<const CRecordMhwProgram*>(record);
          if (
            recordProgram == NULL ||
            Version != recordProgram->Version ||
            ProgramId != recordProgram->ProgramId ||
            ThemeDescriptionId != recordProgram->ThemeDescriptionId ||

            Classification != recordProgram->Classification ||
            !CUtils::CompareStrings(Title, recordProgram->Title) ||
            !CUtils::CompareStrings(Description, recordProgram->Description) ||
            !CUtils::CompareVectors(SeriesIds, recordProgram->SeriesIds) ||
            !CUtils::CompareVectors(ShowingIds, recordProgram->ShowingIds)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Version << 32) | ProgramId;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: program %s, version = %hhu, program ID = %lu, theme description ID = %hu, classification = %hhu, series ID count = %llu, showing ID count = %llu, title = %S, description = %S",
                    situation, Version, ProgramId, ThemeDescriptionId,
                    Classification, (unsigned long long)SeriesIds.size(),
                    (unsigned long long)ShowingIds.size(), Title, Description);

          CUtils::DebugVector(SeriesIds, L"series ID(s)", false);
          CUtils::DebugVector(ShowingIds, L"showing ID(s)", false);
        }

        unsigned char Version;
        unsigned long ProgramId;

        unsigned short ThemeDescriptionId;
        unsigned char Classification;   // 0 = SC, 1 = TP, 2 = ???, 3 = +18, 4 = X, 5 = ???, 6 = +7, 7 = INF, 8 = +12, 9 = +13 / +16

        char* Title;
        char* Description;

        vector<unsigned long> SeriesIds;
        vector<unsigned long> ShowingIds;
    };

    class CRecordMhwProgramShowing : public IRecord
    {
      public:
        CRecordMhwProgramShowing()
        {
          Version = 2;
          ShowingId = 0;
          ProgramId = 0;
          OriginalNetworkId = 0;
          TransportStreamId = 0;
          ServiceId = 0;
          StartDateTime = 0;
          Duration = 0;
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwProgramShowing* recordProgramShowing = dynamic_cast<const CRecordMhwProgramShowing*>(record);
          if (
            recordProgramShowing == NULL ||
            Version != recordProgramShowing->Version ||
            ShowingId != recordProgramShowing->ShowingId ||
            ProgramId != recordProgramShowing->ProgramId ||
            OriginalNetworkId != recordProgramShowing->OriginalNetworkId ||
            TransportStreamId != recordProgramShowing->TransportStreamId ||
            ServiceId != recordProgramShowing->ServiceId ||
            StartDateTime != recordProgramShowing->StartDateTime ||
            Duration != recordProgramShowing->Duration
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Version << 32) | ShowingId;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: program showing %s, version = %hhu, showing ID = %lu, program ID = %lu, ONID = %hu, TSID = %hu, service ID = %hu, start date/time = %llu, duration = %hu m",
                    situation, Version, ShowingId, ProgramId,
                    OriginalNetworkId, TransportStreamId, ServiceId,
                    StartDateTime, Duration);
        }

        unsigned char Version;
        unsigned long ShowingId;
        unsigned long ProgramId;

        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;
        unsigned short ServiceId;
        unsigned long long StartDateTime; // epoch/Unix/POSIX time-stamp
        unsigned short Duration;          // unit = minutes
    };

    class CRecordMhwSeries : public IRecord
    {
      public:
        CRecordMhwSeries()
        {
          Version = 1;
          Id = 0;
          SeriesName = NULL;
          SeasonName = NULL;
          SeasonDescription = NULL;
          IsRecommendation = false;
        }

        ~CRecordMhwSeries()
        {
          if (SeriesName != NULL)
          {
            delete[] SeriesName;
            SeriesName = NULL;
          }
          if (SeasonName != NULL)
          {
            delete[] SeasonName;
            SeasonName = NULL;
          }
          if (SeasonDescription != NULL)
          {
            delete[] SeasonDescription;
            SeasonDescription = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwSeries* recordSeries = dynamic_cast<const CRecordMhwSeries*>(record);
          if (
            recordSeries == NULL ||
            Version != recordSeries->Version ||
            Id != recordSeries->Id ||
            !CUtils::CompareStrings(SeriesName, recordSeries->SeriesName) ||
            !CUtils::CompareStrings(SeasonName, recordSeries->SeasonName) ||
            !CUtils::CompareStrings(SeasonDescription, recordSeries->SeasonDescription) ||
            IsRecommendation != recordSeries->IsRecommendation ||
            !CUtils::CompareMaps(EpisodeNumbers, recordSeries->EpisodeNumbers)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Version << 32) | Id;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: series %s, version = %hhu, ID = %lu, is recommendation = %d, series name = %S, season name = %S, season description = %S",
                    situation, Version, Id, IsRecommendation,
                    SeriesName == NULL ? "" : SeriesName,
                    SeasonName == NULL ? "" : SeasonName,
                    SeasonDescription == NULL ? "" : SeasonDescription);
        }

        unsigned char Version;
        unsigned long Id;
        char* SeriesName;         // for example "Los Simpson", "Juego de tronos" etc. (ie. doesn't include "(HD)" or "(T\d+)" suffixes like the event title)
        char* SeasonName;         // almost always just the season number
        char* SeasonDescription;
        bool IsRecommendation;    // Movistar+ Propone
        map<unsigned long, unsigned short> EpisodeNumbers;    // program ID => episode number
    };

    class CRecordMhwTheme : public IRecord
    {
      public:
        CRecordMhwTheme()
        {
          Version = 1;
          Id = 0;
          SubId = 0;
          Name = NULL;
        }

        ~CRecordMhwTheme()
        {
          if (Name != NULL)
          {
            delete[] Name;
            Name = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwTheme* recordTheme = dynamic_cast<const CRecordMhwTheme*>(record);
          if (
            recordTheme == NULL ||
            Version != recordTheme->Version ||
            Id != recordTheme->Id ||
            SubId != recordTheme->SubId ||
            !CUtils::CompareStrings(Name, recordTheme->Name)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (Version << 16) | (Id << 8) | SubId;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: theme %s, version = %hhu, ID = %hhu, sub ID = %hhu, name = %S",
                    situation, Version, Id, SubId, Name == NULL ? "" : Name);
        }

        unsigned char Version;
        unsigned char Id;
        unsigned char SubId;              // v2 only
        char* Name;
    };

    class CRecordMhwThemeDescription : public IRecord
    {
      public:
        CRecordMhwThemeDescription()
        {
          Version = 1;
          Id = 0;
          Description = NULL;
        }

        ~CRecordMhwThemeDescription()
        {
          if (Description != NULL)
          {
            delete[] Description;
            Description = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordMhwThemeDescription* recordThemeDescription = dynamic_cast<const CRecordMhwThemeDescription*>(record);
          if (
            recordThemeDescription == NULL ||
            Version != recordThemeDescription->Version ||
            Id != recordThemeDescription->Id ||
            !CUtils::CompareStrings(Description, recordThemeDescription->Description)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (Version << 16) | Id;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: theme description %s, version = %hhu, ID = %hu, description = %S",
                    situation, Version, Id,
                    Description == NULL ? "" : Description);
        }

        unsigned char Version;
        unsigned short Id;
        char* Description;
    };

    bool SelectEventRecordByIndex(unsigned long index);

    void OnNewSection(unsigned short pid, unsigned char tableId, const CSection& section);

    void AddOrResetDecoder(unsigned short pid, bool enableCrcCheck);

    unsigned long DecodeVersion1ChannelSection(const unsigned char* data,
                                                unsigned short dataLength);
    unsigned long DecodeVersion1DescriptionSection(const unsigned char* data,
                                                    unsigned short dataLength);
    unsigned long DecodeVersion1EventSection(const unsigned char* data,
                                              unsigned short dataLength);
    unsigned long DecodeVersion1ThemeSection(const unsigned char* data,
                                              unsigned short dataLength);
    bool GetVersion1DateTimeReference(unsigned long long& referenceDateTime,
                                      unsigned char& referenceDayOfWeek);

    unsigned long DecodeVersion2ChannelSection(const unsigned char* data,
                                                unsigned short dataLength);
    unsigned long DecodeVersion2DescriptionSection(const unsigned char* data,
                                                    unsigned short dataLength);
    unsigned long DecodeVersion2EventsByChannelSection(const unsigned char* data,
                                                        unsigned short dataLength,
                                                        bool isTerrestrial);
    unsigned long DecodeVersion2EventsByThemeSection(const unsigned char* data,
                                                      unsigned short dataLength);
    unsigned long DecodeVersion2ProgramSection(const unsigned char* data,
                                                unsigned short dataLength);
    unsigned long DecodeVersion2SeriesSection(const unsigned char* data,
                                              unsigned short dataLength);
    unsigned long DecodeVersion2ThemeSection(const unsigned char* data,
                                              unsigned short dataLength);
    unsigned long DecodeVersion2ThemeDescriptionSection(const unsigned char* data,
                                                        unsigned short dataLength);

    static MhwProvider DetermineProvider(unsigned short originalNetworkId,
                                          unsigned short transportStreamId);
    void CompleteTable(const CSection& section,
                        unsigned char** tableBuffer,
                        unsigned short& tableBufferSize,
                        unsigned char& expectedSectionNumber);

    void OnMhwEventRemoved(unsigned char version,
                            unsigned char segment,
                            unsigned long eventId,
                            unsigned long descriptionId,
                            bool hasDescription,
                            unsigned char channelId,
                            unsigned long long startDateTime,
                            unsigned short duration,
                            const char* title,
                            unsigned long payPerViewId,
                            bool isTerrestrial,
                            unsigned long programId,
                            unsigned long showingId,
                            unsigned char themeId,
                            unsigned char subThemeId);

    CCriticalSection m_section;

    unsigned short m_previousSegmentEventsByChannelSatellite;
    unsigned short m_previousSegmentEventsByChannelTerrestrial;
    unsigned short m_previousSegmentEventsByTheme;
    unsigned short m_previousSegmentEventsVersion1;
    unsigned long m_previousEventIdSatellite;
    unsigned long m_previousEventIdTerrestrial;
    long m_previousDescriptionId;             // version 2
    long m_previousProgramSectionId;
    long m_previousSeriesSectionId;
    unsigned long m_firstDescriptionId;       // version 1

    // Version 1 date/time conversion.
    char m_currentHour;
    unsigned long long m_referenceDateTime;   // epoch/Unix/POSIX time-stamp
    unsigned char m_referenceDayOfWeek;

    clock_t m_completeTime;
    MhwProvider m_provider;
    bool m_grabMhw1;
    bool m_grabMhw2;
    bool m_isSeen;
    bool m_isReady;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackPidConsumer* m_callBackPidConsumer;
    ISystemTimeInfoProviderDvb* m_systemTimeInfoProvider;
    map<unsigned short, CSectionDecoder*> m_decoders;
    CRecordStore m_recordsChannel;
    CRecordStore m_recordsChannelCategory;
    CRecordStore m_recordsDescription;
    CRecordStore m_recordsEventSatellite;
    CRecordStore m_recordsEventTerrestrial;
    CRecordStore m_recordsProgram;
    CRecordStore m_recordsSeries;
    CRecordStore m_recordsShowing;
    CRecordStore m_recordsTheme;
    CRecordStore m_recordsThemeDescription;
    map<unsigned long long, unsigned short> m_channelIdLookup;
    bool m_enableCrcCheck;

    unsigned char* m_descriptionTableBuffer;
    unsigned short m_descriptionTableBufferSize;
    unsigned char m_descriptionSectionNumber;

    unsigned char* m_themeDescriptionTableBuffer;
    unsigned short m_themeDescriptionTableBufferSize;
    unsigned char m_themeDescriptionSectionNumber;

    CRecordMhwEvent* m_currentEvent;
    unsigned long m_currentEventIndex;
    CRecordMhwDescription* m_currentDescription;
};