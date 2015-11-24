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
#include <map>
#include <vector>
#include "..\..\shared\Section.h"
#include "CriticalSection.h"
#include "ICallBackEam.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_EAM 0xd8


extern void LogDebug(const wchar_t* fmt, ...);

class CParserEam
{
  public:
    CParserEam(unsigned short pid);
    virtual ~CParserEam(void);

    void Reset();
    void SetCallBack(ICallBackEam* callBack);
    void OnNewSection(CSection& section);

    bool GetLatestMessage(unsigned short& id,
                          unsigned long& originatorCode,
                          char* eventCode,
                          unsigned short& eventCodeBufferSize,
                          unsigned char& alertMessageTimeRemaining,
                          unsigned long& eventStartTime,
                          unsigned short& eventDuration,
                          unsigned char& alertPriority,
                          unsigned short& detailsOobSourceId,
                          unsigned short& detailsMajorChannelNumber,
                          unsigned short& detailsMinorChannelNumber,
                          unsigned char& detailsRfChannel,
                          unsigned short& detailsProgramNumber,
                          unsigned short& audioOobSourceId,
                          unsigned char& textCount,
                          unsigned long* locationCodes,
                          unsigned char& locationCodeCount,
                          unsigned long* exceptions,
                          unsigned char& exceptionCount,
                          unsigned long* alternativeExceptions,
                          unsigned char& alternativeExceptionCount) const;
    bool GetLatestMessageTextByIndex(unsigned char index,
                                      unsigned long& language,
                                      char* alertText,
                                      unsigned short& alertTextBufferSize,
                                      char* natureOfActivationText,
                                      unsigned short& natureOfActivationTextBufferSize) const;
    bool GetLatestMessageTextByLanguage(unsigned long language,
                                        char* alertText,
                                        unsigned short& alertTextBufferSize,
                                        char* natureOfActivationText,
                                        unsigned short& natureOfActivationTextBufferSize) const;

  private:
    class CRecordEam : public IRecord
    {
      public:
        CRecordEam(void)
        {
          Id = 0;
          OriginatorCode = 0;
          EventCode = NULL;
          AlertMessageTimeRemaining = 0;
          EventStartTime = 0;
          EventDuration = 0;
          AlertPriority = 0;
          DetailsOobSourceId = 0;
          DetailsMajorChannelNumber = 0;
          DetailsMinorChannelNumber = 0;
          DetailsRfChannel = 0;
          DetailsProgramNumber = 0;
          AudioOobSourceId = 0;
        }

        ~CRecordEam(void)
        {
          if (EventCode != NULL)
          {
            delete[] EventCode;
            EventCode = NULL;
          }

          CUtils::CleanUpStringSet(NatureOfActivationTexts);
          CUtils::CleanUpStringSet(AlertTexts);
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordEam* recordEam = dynamic_cast<const CRecordEam*>(record);
          if (
            recordEam == NULL ||
            Id != recordEam->Id ||
            OriginatorCode != recordEam->OriginatorCode ||
            !CUtils::CompareStrings(EventCode, recordEam->EventCode) ||
            !CUtils::CompareStringSets(NatureOfActivationTexts, recordEam->NatureOfActivationTexts) ||
            AlertMessageTimeRemaining != recordEam->AlertMessageTimeRemaining ||
            EventStartTime != recordEam->EventStartTime ||
            EventDuration != recordEam->EventDuration ||
            AlertPriority != recordEam->AlertPriority ||
            DetailsOobSourceId != recordEam->DetailsOobSourceId ||
            DetailsMajorChannelNumber != recordEam->DetailsMajorChannelNumber ||
            DetailsMinorChannelNumber != recordEam->DetailsMinorChannelNumber ||
            DetailsRfChannel != recordEam->DetailsRfChannel ||
            DetailsProgramNumber != recordEam->DetailsProgramNumber ||
            AudioOobSourceId != recordEam->AudioOobSourceId ||
            !CUtils::CompareStringSets(AlertTexts, recordEam->AlertTexts) ||
            !CUtils::CompareVectors(LocationCodes, recordEam->LocationCodes) ||
            !CUtils::CompareVectors(Exceptions, recordEam->Exceptions) ||
            !CUtils::CompareVectors(AlternativeExceptions, recordEam->AlternativeExceptions)
          )
          {
            return false;
          }

          return true;
        }

        unsigned long long GetKey() const
        {
          return Id;
        }

        unsigned long long GetExpiryKey() const
        {
          return 0;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"EAM: message %s, ID = %hu, originator code = %S, event code = %S, nature of activation text count = %llu, alert message time remaining = %hhu s, event start time = %lu, event duration = %hu m, alert priority = %hhu, alert text count = %llu, location code count = %llu, exception count = %llu",
                    situation, Id, (char*)&OriginatorCode,
                    EventCode == NULL ? "" : EventCode,
                    (unsigned long long)NatureOfActivationTexts.size(),
                    AlertMessageTimeRemaining, EventStartTime, EventDuration,
                    AlertPriority, (unsigned long long)AlertTexts.size(),
                    (unsigned long long)LocationCodes.size(),
                    (unsigned long long)(Exceptions.size() + AlternativeExceptions.size()));

          LogDebug(L"  details, OOB source ID = %hu, audio OOB source ID = %hu, RF channel = %hhu, program number = %hu, major channel = %hu, minor channel = %hu",
                    DetailsOobSourceId, AudioOobSourceId, DetailsRfChannel,
                    DetailsProgramNumber, DetailsMajorChannelNumber,
                    DetailsMinorChannelNumber);

          CUtils::DebugStringMap(NatureOfActivationTexts, L"nature of activation text(s)", L"language", L"text");
          CUtils::DebugStringMap(AlertTexts, L"alert text(s)", L"language", L"text");

          if (LocationCodes.size() > 0)
          {
            LogDebug(L"  location codes...");
            vector<unsigned long>::const_iterator it = LocationCodes.begin();
            for ( ; it != LocationCodes.end(); it++)
            {
              unsigned long code = *it;
              LogDebug(L"    state code = %hhu, county sub-division = %hhu, county code = %hu",
                        code >> 24, (code >> 16) & 0xff, code & 0xffff);
            }
          }

          if (Exceptions.size() > 0 || AlternativeExceptions.size() > 0)
          {
            LogDebug(L"  exceptions...");

            vector<unsigned long>::const_iterator it = Exceptions.begin();
            for ( ; it != Exceptions.end(); it++)
            {
              unsigned long exception = *it;
              if (exception >> 31 != 0)
              {
                LogDebug(L"    major channel = %hu, minor channel = %hu",
                          (exception >> 16) & 0x7fff, exception & 0xffff);
              }
              else
              {
                LogDebug(L"    OOB source ID = %hu", exception & 0xffff);
              }
            }

            for (it = AlternativeExceptions.begin(); it != AlternativeExceptions.end(); it++)
            {
              unsigned long exception = *it;
              LogDebug(L"    RF channel = %hhu, program number = %hu",
                        exception >> 16, exception & 0xffff);
            }
          }
        }

        void OnReceived(void* callBack)
        {
          ICallBackEam* callBackEam = static_cast<ICallBackEam*>(callBack);
          if (callBackEam != NULL)
          {
            callBackEam->OnEamReceived(Id, OriginatorCode, EventCode,
                                        NatureOfActivationTexts,
                                        AlertMessageTimeRemaining,
                                        EventStartTime, EventDuration,
                                        AlertPriority,
                                        DetailsOobSourceId,
                                        DetailsMajorChannelNumber,
                                        DetailsMinorChannelNumber,
                                        DetailsRfChannel, DetailsProgramNumber,
                                        AudioOobSourceId, AlertTexts,
                                        LocationCodes,
                                        Exceptions, AlternativeExceptions);
          }
        }

        void OnChanged(void* callBack)
        {
          ICallBackEam* callBackEam = static_cast<ICallBackEam*>(callBack);
          if (callBackEam != NULL)
          {
            callBackEam->OnEamChanged(Id, OriginatorCode, EventCode,
                                        NatureOfActivationTexts,
                                        AlertMessageTimeRemaining,
                                        EventStartTime, EventDuration,
                                        AlertPriority,
                                        DetailsOobSourceId,
                                        DetailsMajorChannelNumber,
                                        DetailsMinorChannelNumber,
                                        DetailsRfChannel, DetailsProgramNumber,
                                        AudioOobSourceId, AlertTexts,
                                        LocationCodes,
                                        Exceptions, AlternativeExceptions);
          }
        }

        void OnRemoved(void* callBack)
        {
          ICallBackEam* callBackEam = static_cast<ICallBackEam*>(callBack);
          if (callBackEam != NULL)
          {
            callBackEam->OnEamRemoved(Id, OriginatorCode, EventCode,
                                        NatureOfActivationTexts,
                                        AlertMessageTimeRemaining,
                                        EventStartTime, EventDuration,
                                        AlertPriority,
                                        DetailsOobSourceId,
                                        DetailsMajorChannelNumber,
                                        DetailsMinorChannelNumber,
                                        DetailsRfChannel, DetailsProgramNumber,
                                        AudioOobSourceId, AlertTexts,
                                        LocationCodes,
                                        Exceptions, AlternativeExceptions);
          }
        }

        unsigned short Id;
        unsigned long OriginatorCode;
        char* EventCode;
        map<unsigned long, char*> NatureOfActivationTexts;
        unsigned char AlertMessageTimeRemaining;
        unsigned long EventStartTime;
        unsigned short EventDuration;
        unsigned char AlertPriority;

        unsigned short DetailsOobSourceId;
        unsigned short DetailsMajorChannelNumber;
        unsigned short DetailsMinorChannelNumber;
        unsigned char DetailsRfChannel;
        unsigned short DetailsProgramNumber;
        unsigned short AudioOobSourceId;

        map<unsigned long, char*> AlertTexts;
        vector<unsigned long> LocationCodes;          // state code [8 bits] | county sub-division [8 bits] | county code [16 bits]
        vector<unsigned long> Exceptions;             // in band: major channel number [16 bits] | minor channel number [16 bits]; out of band: source ID [16 bits]
        vector<unsigned long> AlternativeExceptions;  // in band exception channels descriptor: RF channel [8 bits] | program number [16 bits]
    };

    static bool DecodeInBandDetailsChannelDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned char& rfChannel,
                                                      unsigned short& programNumber);
    static bool DecodeInBandExceptionChannelDescriptor(unsigned char* data,
                                                        unsigned char dataLength,
                                                        vector<unsigned long>& channels);
    static bool DecodeAudioFileDescriptor(unsigned char* data, unsigned char dataLength);

    CCriticalSection m_section;
    unsigned short m_pid;
    ICallBackEam* m_callBack;
    unsigned char m_sequenceNumber;
    map<unsigned short, bool> m_seenEventIds;
    CRecordEam* m_latestRecord;
};