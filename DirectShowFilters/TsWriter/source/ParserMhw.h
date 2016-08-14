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
#include <time.h>       // gmtime(), mktime(), time_t, tm
#include <vector>
#include <WinError.h>   // HRESULT
#include "..\..\shared\ISectionCallback.h"
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\TsHeader.h"
#include "CriticalSection.h"
#include "ICallBackGrabber.h"
#include "ICallBackPidConsumer.h"
#include "IGrabberEpgMhw.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define PID_MHW1_EVENTS                     0xd2
#define PID_MHW1_OTHER                      0xd3
#define PID_MHW2_CHANNELS_AND_THEMES        0x231
#define PID_MHW2_EVENTS                     0x234
#define PID_MHW2_DESCRIPTIONS               0x236
#define PID_MHW_CALL_BACK                   PID_MHW1_EVENTS

#define TABLE_ID_MHW1_CHANNELS              0x91
#define TABLE_ID_MHW1_EVENTS                0x90
#define TABLE_ID_MHW1_DESCRIPTIONS          0x90
#define TABLE_ID_MHW1_THEMES                0x92
#define TABLE_ID_MHW2_CHANNELS_AND_THEMES   0xc8
#define TABLE_ID_MHW2_EVENTS                0xe6
#define TABLE_ID_MHW2_DESCRIPTIONS          0x96
#define TABLE_ID_MHW_CALL_BACK              TABLE_ID_MHW1_CHANNELS


extern void LogDebug(const wchar_t* fmt, ...);

class CParserMhw : public CUnknown, public IGrabberEpgMhw, ISectionCallback
{
  public:
    CParserMhw(ICallBackPidConsumer* callBack, LPUNKNOWN unk, HRESULT* hr);
    virtual ~CParserMhw();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    STDMETHODIMP_(void) SetProtocols(bool grabMhw1, bool grabMhw2);
    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(CTsHeader& header, unsigned char* tsPacket);
    STDMETHODIMP_(bool) IsSeen();
    STDMETHODIMP_(bool) IsReady();

    STDMETHODIMP_(unsigned long) GetEventCount();
    STDMETHODIMP_(bool) GetEvent(unsigned long index,
                                  unsigned long long* eventId,
                                  unsigned short* originalNetworkId,
                                  unsigned short* transportStreamId,
                                  unsigned short* serviceId,
                                  char* serviceName,
                                  unsigned short* serviceNameBufferSize,
                                  unsigned long long* startDateTime,
                                  unsigned short* duration,
                                  char* title,
                                  unsigned short* titleBufferSize,
                                  unsigned long* payPerViewId,
                                  char* description,
                                  unsigned short* descriptionBufferSize,
                                  unsigned char* descriptionLineCount,
                                  char* themeName,
                                  unsigned short* themeNameBufferSize,
                                  char* subThemeName,
                                  unsigned short* subThemeNameBufferSize);
    STDMETHODIMP_(bool) GetDescriptionLine(unsigned long long eventId,
                                            unsigned char index,
                                            char* line,
                                            unsigned short* lineBufferSize);

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
            !CUtils::CompareStrings(Name, recordChannel->Name)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (Version << 8) | Id;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: channel %s, version = %hhu, ID = %hhu, ONID = %hu, TSID = %hu, service ID = %hu, name = %S",
                    situation, Version, Id, OriginalNetworkId,
                    TransportStreamId, ServiceId, Name == NULL ? "" : Name);
        }

        unsigned char Version;
        unsigned char Id;
        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;
        unsigned short ServiceId;
        char* Name;
    };

    class CRecordMhwEvent : public IRecord
    {
      public:
        CRecordMhwEvent()
        {
          Version = 1;
          EventId = 0;
          ChannelId = 0;
          ThemeId = 0;
          SubThemeId = 0;
          StartDateTime = 0;
          HasDescription = false;
          Duration = 0;
          Title = NULL;
          PayPerViewId = 0;
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
            ChannelId != recordEvent->ChannelId ||
            ThemeId != recordEvent->ThemeId ||
            SubThemeId != recordEvent->SubThemeId ||
            StartDateTime != recordEvent->StartDateTime ||
            HasDescription != recordEvent->HasDescription ||
            Duration != recordEvent->Duration ||
            !CUtils::CompareStrings(Title, recordEvent->Title) ||
            PayPerViewId != recordEvent->PayPerViewId
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)Version << 32) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: event %s, version = %hhu, event ID = %lu, channel ID = %hhu, theme ID = %hhu, sub-theme ID = %hhu, start date/time = %llu, has description = %d, duration = %hu m, title = %S, PPV ID = %lu",
                    situation, Version, EventId, ChannelId, ThemeId,
                    SubThemeId, StartDateTime, HasDescription, Duration,
                    Title == NULL ? "" : Title, PayPerViewId);
        }

        unsigned char Version;
        unsigned long EventId;
        unsigned char ChannelId;
        unsigned char ThemeId;
        unsigned char SubThemeId;
        unsigned long long StartDateTime; // unit = UTC epoch
        bool HasDescription;
        unsigned short Duration;          // unit = minutes
        char* Title;
        unsigned long PayPerViewId;
    };

    class CRecordMhwDescription : public IRecord
    {
      public:
        CRecordMhwDescription()
        {
          Version = 1;
          EventId = 0;
          Description = NULL;
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
            EventId != recordDescription->EventId ||
            !CUtils::CompareStrings(Description, recordDescription->Description) ||
            Lines.size() != recordDescription->Lines.size()
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
          return ((unsigned long long)Version << 32) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return Version;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"MHW: description %s, version = %hhu, event ID = %lu, description = %S, line count = %llu",
                    situation, Version, EventId,
                    Description == NULL ? "" : Description,
                    (unsigned long long)Lines.size());
          vector<char*>::const_iterator lineIt = Lines.begin();
          for ( ; lineIt != Lines.end(); lineIt++)
          {
            char* line = *lineIt;
            LogDebug(L"  %S", line == NULL ? "" : line);
          }
        }

        unsigned char Version;
        unsigned long EventId;
        char* Description;
        vector<char*> Lines;
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
        unsigned char SubId;
        char* Name;
    };

    void OnNewSection(int pid, int tableId, CSection& section);

    void AddOrResetDecoder(unsigned short pid, bool enableCrcCheck);

    unsigned long DecodeVersion1ChannelSection(unsigned char* data,
                                                unsigned short dataLength);
    unsigned long DecodeVersion1EventSection(unsigned char* data,
                                              unsigned short dataLength);
    unsigned long DecodeVersion1DescriptionSection(unsigned char* data,
                                                    unsigned short dataLength);
    unsigned long DecodeVersion1ThemeSection(unsigned char* data,
                                              unsigned short dataLength);

    unsigned long DecodeVersion2ChannelSection(unsigned char* data,
                                                unsigned short dataLength);
    unsigned long DecodeVersion2EventSection(unsigned char* data,
                                              unsigned short dataLength);
    unsigned long DecodeVersion2DescriptionSection(unsigned char* data,
                                                    unsigned short dataLength);
    unsigned long DecodeVersion2ThemeSection(unsigned char* data,
                                              unsigned short dataLength);

    CCriticalSection m_section;
    unsigned long m_dayOfWeek;
    unsigned long m_yesterdayDayOfWeek;
    time_t m_yesterdayEpoch;
    clock_t m_completeTime;
    bool m_grabMhw1;
    bool m_grabMhw2;
    bool m_isSeen;
    bool m_isReady;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackPidConsumer* m_callBackPidConsumer;
    map<unsigned short, CSectionDecoder*> m_decoders;
    CRecordStore m_recordsChannel;
    CRecordStore m_recordsEvent;
    CRecordStore m_recordsDescription;
    CRecordStore m_recordsTheme;
    bool m_enableCrcCheck;
    clock_t m_lastExpiredRecordCheckTime;
};