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
#include <sstream>
#include <string>
#include <streams.h>    // CUnknown, LPUNKNOWN
#include <vector>
#include <WinError.h>   // HRESULT
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\ISectionCallback.h"
#include "..\..\shared\ISectionDispatcher.h"
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\TsHeader.h"
#include "ICallBackGrabber.h"
#include "ICallBackPidConsumer.h"
#include "IDefaultAuthorityProvider.h"
#include "IGrabberEpgDvb.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "TextUtil.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define PID_EIT_DVB                     0x12    // DVB standard
#define PID_EIT_VIASAT_SWEDEN           0x39    // Viasat (Sweden satellite); custom PID
#define PID_EIT_DISH                    0x300   // DISH Network (USA satellite) 9 day EPG; custom descriptors
#define PID_EIT_MULTICHOICE             0x3fa   // MultiChoice (South Africa satellite); custom PID
#define PID_EIT_BELL_TV                 0x441   // Bell TV (Canada satellite) 9 day EPG; custom descriptors
#define PID_EIT_PREMIERE_SELECT         0xb09   // Germany, now owned by Sky; custom format
#define PID_EIT_PREMIERE_DIREKT         0xb11   // Germany, now owned by Sky; custom format
#define PID_EIT_PREMIERE_SPORT          0xb12   // Germany, now owned by Sky; custom format
#define PID_EIT_ORBIT_SHOWTIME_NETWORK  0x1012  // Orbit Showtime Network [OSN] (Middle East and North Africa satellite) full EPG; custom PID
#define PID_EIT_DVB_CALL_BACK           PID_EIT_DVB

#define TABLE_ID_EIT_DVB_START          0x4e
#define TABLE_ID_EIT_DVB_END            0x6f
#define TABLE_ID_EIT_DISH_START         0x80
#define TABLE_ID_EIT_DISH_END           0xfe
#define TABLE_ID_EIT_DVB_PF_ACTUAL      0x4e
#define TABLE_ID_EIT_DVB_PF_OTHER       0x4f
#define TABLE_ID_EIT_PREMIERE           0xa0
#define TABLE_ID_EIT_DVB_CALL_BACK      TABLE_ID_EIT_DVB_START

// We don't use this table. It seems to carry duplicate incomplete data.
// Perhaps it is an alternative running status table for accurate recording?
#define TABLE_ID_EIT_FREESAT_PF     0xd1


extern void LogDebug(const wchar_t* fmt, ...);

class CParserEitDvb : public CUnknown, public IGrabberEpgDvb, ISectionCallback
{
  public:
    CParserEitDvb(ICallBackPidConsumer* callBack,
                  ISectionDispatcher* sectionDispatcher,
                  IDefaultAuthorityProvider* authorityProvider,
                  LPUNKNOWN unk,
                  HRESULT* hr);
    virtual ~CParserEitDvb();

    DECLARE_IUNKNOWN

    STDMETHODIMP NonDelegatingQueryInterface(REFIID iid, void** ppv);

    void SetFreesatPmtPid(unsigned short pid);
    void SetFreesatPids(unsigned short pidBat,
                        unsigned short pidEitPf,
                        unsigned short pidEitSchedule,
                        unsigned short pidNit,
                        unsigned short pidSdt);
    STDMETHODIMP_(void) SetProtocols(bool grabDvbEit,
                                      bool grabBellTv,
                                      bool grabDish,
                                      bool grabFreesat,
                                      bool grabMultiChoice,
                                      bool grabOrbitShowtimeNetwork,
                                      bool grabPremiere,
                                      bool grabViasatSweden);
    void Reset(bool enableCrcCheck);
    STDMETHODIMP_(void) SetCallBack(ICallBackGrabber* callBack);
    bool OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket);
    STDMETHODIMP_(bool) IsSeen();
    STDMETHODIMP_(bool) IsReady();

    STDMETHODIMP_(unsigned short) GetServiceCount();
    STDMETHODIMP_(bool) GetService(unsigned short index,
                                    unsigned short* originalNetworkId,
                                    unsigned short* transportStreamId,
                                    unsigned short* serviceId,
                                    unsigned short* eventCount);
    STDMETHODIMP_(bool) GetEvent(unsigned short serviceIndex,
                                  unsigned short eventIndex,
                                  unsigned long long* eventId,
                                  unsigned long long* startDateTime,
                                  unsigned long* duration,
                                  unsigned char* runningStatus,
                                  bool* freeCaMode,
                                  unsigned short* referenceServiceId,
                                  unsigned long long* referenceEventId,
                                  char* seriesId,
                                  unsigned short* seriesIdBufferSize,
                                  char* episodeId,
                                  unsigned short* episodeIdBufferSize,
                                  bool* isHighDefinition,
                                  bool* isStandardDefinition,
                                  bool* isThreeDimensional,
                                  bool* isPreviouslyShown,
                                  unsigned long* audioLanguages,
                                  unsigned char* audioLanguageCount,
                                  unsigned long* subtitlesLanguages,
                                  unsigned char* subtitlesLanguageCount,
                                  unsigned short* dvbContentTypeIds,
                                  unsigned char* dvbContentTypeIdCount,
                                  unsigned long* dvbParentalRatingCountryCodes,
                                  unsigned char* dvbParentalRatings,
                                  unsigned char* dvbParentalRatingCount,
                                  unsigned char* starRating,
                                  unsigned char* mpaaClassification,
                                  unsigned short* dishBevAdvisories,
                                  unsigned char* vchipRating,
                                  unsigned char* textCount);
    STDMETHODIMP_(bool) GetEventText(unsigned short serviceIndex,
                                      unsigned short eventIndex,
                                      unsigned char textIndex,
                                      unsigned long* language,
                                      char* title,
                                      unsigned short* titleBufferSize,
                                      char* shortDescription,
                                      unsigned short* shortDescriptionBufferSize,
                                      char* extendedDescription,
                                      unsigned short* extendedDescriptionBufferSize,
                                      unsigned char* descriptionItemCount);
    STDMETHODIMP_(bool) GetEventDescriptionItem(unsigned short serviceIndex,
                                                unsigned short eventIndex,
                                                unsigned char textIndex,
                                                unsigned char itemIndex,
                                                char* description,
                                                unsigned short* descriptionBufferSize,
                                                char* text,
                                                unsigned short* textBufferSize);

  private:
    class CRecordEitEventDescriptionItem
    {
      public:
        CRecordEitEventDescriptionItem()
        {
          DescriptorNumber = 0;
          Index = 0;
          Description = NULL;
          Text = NULL;
        }

        ~CRecordEitEventDescriptionItem()
        {
          if (Description != NULL)
          {
            delete[] Description;
            Description = NULL;
          }
          if (Text != NULL)
          {
            delete[] Text;
            Text = NULL;
          }
        }

        bool Equals(const CRecordEitEventDescriptionItem* record) const
        {
          if (
            record == NULL ||
            DescriptorNumber != record->DescriptorNumber ||
            Index != record->Index ||
            !CUtils::CompareStrings(Description, record->Description) ||
            !CUtils::CompareStrings(Text, record->Text)
          )
          {
            return false;
          }
          return true;
        }

        unsigned short GetKey() const
        {
          return (DescriptorNumber << 8) | Index;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"EIT DVB: description item %s, descriptor number = %hhu, index = %hhu, description = %S, text = %S",
                    situation, DescriptorNumber, Index,
                    Description == NULL ? "" : Description,
                    Text == NULL ? "" : Text);
        }

        unsigned char DescriptorNumber;
        unsigned char Index;
        char* Description;
        char* Text;
    };

    class CRecordEitEventText
    {
      public:
        CRecordEitEventText()
        {
          Language = 0;
          Title = NULL;
          DescriptionShort = NULL;
          DescriptionExtended = NULL;
        }

        ~CRecordEitEventText()
        {
          if (Title != NULL)
          {
            delete[] Title;
            Title = NULL;
          }
          if (DescriptionShort != NULL)
          {
            delete[] DescriptionShort;
            DescriptionShort = NULL;
          }
          if (DescriptionExtended != NULL)
          {
            delete[] DescriptionExtended;
            DescriptionExtended = NULL;
          }
          map<unsigned short, CRecordEitEventDescriptionItem*>::iterator it = DescriptionItems.begin();
          for ( ; it != DescriptionItems.end(); it++)
          {
            CRecordEitEventDescriptionItem* item = it->second;
            if (item != NULL)
            {
              delete item;
              it->second = NULL;
            }
          }
          DescriptionItems.clear();
        }

        bool Equals(const CRecordEitEventText* record) const
        {
          if (
            record == NULL ||
            Language != record->Language ||
            !CUtils::CompareStrings(Title, record->Title) ||
            !CUtils::CompareStrings(DescriptionShort, record->DescriptionShort) ||
            !CUtils::CompareStrings(DescriptionExtended, record->DescriptionExtended) ||
            DescriptionItems.size() != record->DescriptionItems.size()
          )
          {
            return false;
          }

          map<unsigned short, CRecordEitEventDescriptionItem*>::const_iterator it1 = DescriptionItems.begin();
          for ( ; it1 != DescriptionItems.end(); it1++)
          {
            if (it1->second == NULL)
            {
              continue;
            }
            map<unsigned short, CRecordEitEventDescriptionItem*>::const_iterator it2 = record->DescriptionItems.find(it1->first);
            if (
              it2 == record->DescriptionItems.end() ||
              !it1->second->Equals(it2->second)
            )
            {
              return false;
            }
          }
          return true;
        }

        unsigned long GetKey() const
        {
          return Language;
        }

        void Decompress(char* inputText, char** outputText, const wchar_t* textType) const
        {
          // Dish Network text is compressed to minimise memory usage. It must
          // be decompressed before we can display it.
          if (inputText == NULL || inputText[0] != 0x1f)
          {
            *outputText = inputText;
            return;
          }

          unsigned char tableId = (unsigned char)inputText[1];
          unsigned char dataLength = (unsigned char)inputText[2];
          unsigned char* data = (unsigned char*)&inputText[3];
          if (!CTextUtil::DishTextToString(data, dataLength, tableId, outputText))
          {
            LogDebug(L"EIT DVB: invalid Dish event %s, length = %hhu, table ID = 0x%hhx, byte 1 = %hhu, byte 2 = %hhu",
                      textType, dataLength, tableId, data[0],
                      dataLength > 1 ? data[1] : 0xff);
          }
          else if (*outputText == NULL)
          {
            LogDebug(L"EIT DVB: failed to allocate Dish event %s, length = %hhu, byte 1 = %hhu, byte 2 = %hhu",
                      textType, dataLength, data[0],
                      dataLength > 1 ? data[1] : 0xff);
          }
        }

        void Debug(const wchar_t* situation) const
        {
          // Dish Network text is compressed to minimise memory usage. It must
          // be decompressed before we can display it.
          char* tempTitle = NULL;
          Decompress(Title, &tempTitle, L"name");
          char* tempDescription = NULL;
          Decompress(DescriptionShort, &tempDescription, L"description");

          LogDebug(L"EIT DVB: text %s, code = %S, title = %S, short description = %S, extended description = %S, item count = %llu",
                    situation, (char*)&Language,
                    tempTitle == NULL ? "" : tempTitle,
                    tempDescription == NULL ? "" : tempDescription,
                    DescriptionExtended == NULL ? "" : DescriptionExtended,
                    (unsigned long long)DescriptionItems.size());

          if (tempTitle != NULL && tempTitle != Title)
          {
            delete[] tempTitle;
          }
          if (tempDescription != NULL && tempDescription != DescriptionShort)
          {
            delete[] tempDescription;
          }

          map<unsigned short, CRecordEitEventDescriptionItem*>::const_iterator it = DescriptionItems.begin();
          for ( ; it != DescriptionItems.end(); it++)
          {
            if (it->second != NULL)
            {
              it->second->Debug(situation);
            }
          }
        }

        unsigned long Language;
        char* Title;
        char* DescriptionShort;
        char* DescriptionExtended;
        map<unsigned short, CRecordEitEventDescriptionItem*> DescriptionItems;
    };

    // This implementation is currently used only for Dish Network events.
    // Storing Dish's 9 day guide for ~7250 channels in memory while staying
    // within a 32 bit process memory limit requires extraordinary care.
    class CRecordEitEventMinimal : public IRecord
    {
      public:
        CRecordEitEventMinimal()
        {
          TableId = 0;
          OriginalNetworkId = 0;
          TransportStreamId = 0;
          ServiceId = 0;
          EventId = 0;
          StartDateTime = 0;
          Duration = 0;
          RunningStatus = 0;
          FreeCaMode = false;
          AreSeriesAndEpisodeIdsCrids = false;
          SeriesId = NULL;
          EpisodeId = NULL;
          IsPreviouslyShown = false;
          StarRating = 0;             // default: [not available]
          MpaaClassification = 0xff;  // default: [not available]
          DishBevAdvisories = 0;      // default: [not available]
          VchipRating = 0xff;         // default: [not available]
        }

        virtual ~CRecordEitEventMinimal()
        {
          if (SeriesId != NULL)
          {
            delete[] SeriesId;
            SeriesId = NULL;
          }
          if (EpisodeId != NULL)
          {
            delete[] EpisodeId;
            EpisodeId = NULL;
          }
          map<unsigned long, CRecordEitEventText*>::iterator it = Texts.begin();
          for ( ; it != Texts.end(); it++)
          {
            CRecordEitEventText* text = it->second;
            if (text != NULL)
            {
              delete text;
              it->second = NULL;
            }
          }
          Texts.clear();
        }

        virtual bool Equals(const IRecord* record) const
        {
          const CRecordEitEventMinimal* recordEvent = dynamic_cast<const CRecordEitEventMinimal*>(record);
          if (
            recordEvent == NULL ||
            TableId != recordEvent->TableId ||
            EventId != recordEvent->EventId ||
            StartDateTime != recordEvent->StartDateTime ||
            Duration != recordEvent->Duration ||
            RunningStatus != recordEvent->RunningStatus ||
            FreeCaMode != recordEvent->FreeCaMode ||
            AreSeriesAndEpisodeIdsCrids != recordEvent->AreSeriesAndEpisodeIdsCrids ||
            !CUtils::CompareStrings(SeriesId, recordEvent->SeriesId) ||
            !CUtils::CompareStrings(EpisodeId, recordEvent->EpisodeId) ||
            IsPreviouslyShown != recordEvent->IsPreviouslyShown ||
            !CUtils::CompareVectors(DvbContentTypeIds, recordEvent->DvbContentTypeIds) ||
            StarRating != recordEvent->StarRating ||
            MpaaClassification != recordEvent->MpaaClassification ||
            DishBevAdvisories != recordEvent->DishBevAdvisories ||
            VchipRating != recordEvent->VchipRating ||
            Texts.size() != recordEvent->Texts.size()
          )
          {
            return false;
          }

          map<unsigned long, CRecordEitEventText*>::const_iterator textIt1 = Texts.begin();
          for ( ; textIt1 != Texts.end(); textIt1++)
          {
            if (textIt1->second == NULL)
            {
              continue;
            }
            map<unsigned long, CRecordEitEventText*>::const_iterator textIt2 = recordEvent->Texts.find(textIt1->first);
            if (
              textIt2 == recordEvent->Texts.end() ||
              !textIt1->second->Equals(textIt2->second)
            )
            {
              return false;
            }
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          // Ideally we wouldn't have to include the table ID in the key.
          // However, some providers include the same event in multiple tables
          // at the same time (eg. present/following and schedule). Therefore
          // we must include the table ID to avoid spurious duplicate detection
          // hits.
          return ((unsigned long long)TableId << 48) | EventId;
        }

        unsigned long long GetExpiryKey() const
        {
          return TableId;
        }

        virtual void Debug(const wchar_t* situation) const
        {
          LogDebug(L"EIT DVB: event %s, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, event ID = %llu, start date/time = %llu, duration = %lu s, running status = %hhu, free CA mode = %d, series ID = %S, episode ID = %S, is previously shown = %d, DVB content type count = %llu, star rating = %hhu, MPAA classification = %hhu, Dish/BEV advisories = %hu, V-CHIP rating = %hhu, text count = %llu",
                    situation, TableId, OriginalNetworkId, TransportStreamId,
                    ServiceId, EventId, StartDateTime, Duration, RunningStatus,
                    FreeCaMode, SeriesId == NULL ? "" : SeriesId,
                    EpisodeId == NULL ? "" : EpisodeId, IsPreviouslyShown,
                    (unsigned long long)DvbContentTypeIds.size(),
                    StarRating, MpaaClassification, DishBevAdvisories,
                    VchipRating, (unsigned long long)Texts.size());

          CUtils::DebugVector(DvbContentTypeIds, L"DVB content type ID(s)", false);

          map<unsigned long, CRecordEitEventText*>::const_iterator textIt = Texts.begin();
          for ( ; textIt != Texts.end(); textIt++)
          {
            if (textIt->second != NULL)
            {
              textIt->second->Debug(situation);
            }
          }
        }

        // Ordered to minimise memory usage (8-byte aligned).
        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;

        unsigned long long EventId;
        unsigned long long StartDateTime; // epoch/Unix/POSIX time-stamp

        unsigned long Duration;           // unit = seconds
        unsigned short ServiceId;
        unsigned char TableId;
        unsigned char RunningStatus;

        char* SeriesId;
        char* EpisodeId;

        bool FreeCaMode;
        bool AreSeriesAndEpisodeIdsCrids;
        bool IsPreviouslyShown;
        unsigned char StarRating;
        unsigned char MpaaClassification;
        unsigned char VchipRating;
        unsigned short DishBevAdvisories;

        vector<unsigned short> DvbContentTypeIds;
        map<unsigned long, CRecordEitEventText*> Texts;
    };

    class CRecordEitEvent : public CRecordEitEventMinimal
    {
      public:
        CRecordEitEvent()
        {
          ReferenceServiceId = 0;
          ReferenceEventId = 0;
          IsHighDefinition = false;
          IsStandardDefinition = false;
          IsThreeDimensional = false;
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordEitEventMinimal::Equals(record))
          {
            return false;
          }

          const CRecordEitEvent* recordEvent = dynamic_cast<const CRecordEitEvent*>(record);
          if (
            recordEvent == NULL ||
            ReferenceServiceId != recordEvent->ReferenceServiceId ||
            ReferenceEventId != recordEvent->ReferenceEventId ||
            IsHighDefinition != recordEvent->IsHighDefinition ||
            IsStandardDefinition != recordEvent->IsStandardDefinition ||
            IsThreeDimensional != recordEvent->IsThreeDimensional ||
            !CUtils::CompareVectors(AudioLanguages, recordEvent->AudioLanguages) ||
            !CUtils::CompareVectors(SubtitlesLanguages, recordEvent->SubtitlesLanguages) ||
            DvbParentalRatings.size() != recordEvent->DvbParentalRatings.size()
          )
          {
            return false;
          }

          map<unsigned long, unsigned char>::const_iterator prIt = DvbParentalRatings.begin();
          for ( ; prIt != DvbParentalRatings.end(); prIt++)
          {
            if (recordEvent->DvbParentalRatings.find(prIt->first) == recordEvent->DvbParentalRatings.end())
            {
              return false;
            }
          }
          return true;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"EIT DVB: event %s, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, event ID = %llu, start date/time = %llu, duration = %lu s, running status = %hhu, free CA mode = %d, reference service ID = %hu, reference event ID = %llu, series ID = %S, episode ID = %S, is HD = %d, is SD = %d, is 3D = %d, is previously shown = %d, audio language count = %llu, subtitles language count = %llu, DVB content type count = %llu, DVB parental rating count = %llu, star rating = %hhu, MPAA classification = %hhu, Dish/BEV advisories = %hu, V-CHIP rating = %hhu, text count = %llu",
                    situation, TableId, OriginalNetworkId, TransportStreamId,
                    ServiceId, EventId, StartDateTime, Duration, RunningStatus,
                    FreeCaMode, ReferenceServiceId, ReferenceEventId,
                    SeriesId == NULL ? "" : SeriesId,
                    EpisodeId == NULL ? "" : EpisodeId, IsHighDefinition,
                    IsStandardDefinition, IsThreeDimensional,
                    IsPreviouslyShown,
                    (unsigned long long)AudioLanguages.size(),
                    (unsigned long long)SubtitlesLanguages.size(),
                    (unsigned long long)DvbContentTypeIds.size(),
                    (unsigned long long)DvbParentalRatings.size(), StarRating,
                    MpaaClassification, DishBevAdvisories, VchipRating,
                    (unsigned long long)Texts.size());

          CUtils::DebugVector(AudioLanguages, L"audio language(s)", true);
          CUtils::DebugVector(SubtitlesLanguages, L"subtitles language(s)", true);
          CUtils::DebugVector(DvbContentTypeIds, L"DVB content type ID(s)", false);

          if (DvbParentalRatings.size() > 0)
          {
            wstringstream temp(ios_base::out | ios_base::ate);
            temp.str(L"  DVB parental rating(s) = ");
            bool isFirst = true;
            map<unsigned long, unsigned char>::const_iterator prIt = DvbParentalRatings.begin();
            for ( ; prIt != DvbParentalRatings.end(); prIt++)
            {
              if (!isFirst)
              {
                temp << L", ";
              }
              temp << prIt->second << L" (" << (char*)(&(prIt->first)) << ")";
              isFirst = false;
            }
            wstring s = temp.str();
            LogDebug(s.c_str());
          }

          map<unsigned long, CRecordEitEventText*>::const_iterator textIt = Texts.begin();
          for ( ; textIt != Texts.end(); textIt++)
          {
            if (textIt->second != NULL)
            {
              textIt->second->Debug(situation);
            }
          }
        }

        unsigned short ReferenceServiceId;
        unsigned long long ReferenceEventId;
        bool IsHighDefinition;
        bool IsStandardDefinition;
        bool IsThreeDimensional;
        vector<unsigned long> AudioLanguages;
        vector<unsigned long> SubtitlesLanguages;
        map<unsigned long, unsigned char> DvbParentalRatings;   // country code => rating
    };

    class CRecordEitService
    {
      public:
        CRecordEitService() : Events(600000)
        {
          IsPremiereService = false;
          OriginalNetworkId = 0;
          TransportStreamId = 0;
          ServiceId = 0;
        }

        ~CRecordEitService()
        {
        }

        bool IsPremiereService;
        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;
        unsigned short ServiceId;
        CRecordStore Events;

        vector<unsigned char> SeenTables;
        vector<unsigned char> UnseenTables;
        vector<unsigned long> SeenSegments;
        vector<unsigned long> UnseenSegments;
        vector<unsigned long> SeenSections;
        vector<unsigned long> UnseenSections;
    };

    bool SelectServiceRecordByIndex(unsigned short index);
    bool SelectEventRecordByIndex(unsigned short index);
    bool SelectTextRecordByIndex(unsigned char index);

    void OnNewSection(unsigned short pid, unsigned char tableId, const CSection& section);

    void PrivateReset(bool removeFreesatDecoders);
    bool AddOrResetDecoder(unsigned short pid, bool enableCrcCheck);
    void ResetFreesatGrabState();
    template<class T> static void RemoveExpiredEntries(vector<T>& set,
                                                        bool isTableIdSet,
                                                        unsigned char sectionTableId,
                                                        unsigned char sectionVersionNumber,
                                                        bool isTableFromGroup,
                                                        unsigned char lastValidTableId,
                                                        vector<unsigned char>& erasedTableIds);
    void CreatePremiereEvents(CRecordEitEvent& eventTemplate,
                              map<unsigned long long,
                              vector<unsigned long long>*>& premiereShowings);
    CRecordEitService* GetOrCreateService(bool isPremiereService,
                                          unsigned short originalNetworkId,
                                          unsigned short transportStreamId,
                                          unsigned short serviceId,
                                          bool doNotCreate);
    static CRecordEitEventText* GetOrCreateText(CRecordEitEventMinimal& event, unsigned long language);
    static void CreateDescriptionItem(CRecordEitEventMinimal& event,
                                      unsigned long language,
                                      unsigned char index,
                                      const char* description,
                                      char* text);
    static void CopyString(const char* input, char** output, wchar_t* debug);

    static bool DecodeEventRecord(const unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  CRecordEitEventMinimal& event,
                                  map<unsigned long long, vector<unsigned long long>*>& premiereShowings);
    static bool DecodeEventDescriptors(const unsigned char* sectionData,
                                        unsigned short& pointer,
                                        unsigned short endOfDescriptorLoop,
                                        CRecordEitEventMinimal& event,
                                        map<unsigned long long, vector<unsigned long long>*>& premiereShowings);

    static bool DecodeShortEventDescriptor(const unsigned char* data,
                                            unsigned char dataLength,
                                            unsigned long& language,
                                            char** eventName,
                                            char** text);
    static bool DecodeExtendedEventDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned long& language,
                                              vector<CRecordEitEventDescriptionItem*>& items,
                                              char** text);
    static bool DecodeTimeShiftedEventDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned short& referenceServiceId,
                                                  unsigned short& referenceEventId);
    static bool DecodeComponentDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          bool& isAudio,
                                          bool& isSubtitles,
                                          bool& isHighDefinition,
                                          bool& isStandardDefinition,
                                          bool& isThreeDimensional,
                                          unsigned long& language);
    static bool DecodeContentDescriptor(const unsigned char* data,
                                        unsigned char dataLength,
                                        vector<unsigned short>& contentTypeIds);
    static bool DecodeParentalRatingDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                map<unsigned long, unsigned char>& ratings);
    static bool DecodePrivateDataSpecifierDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned long& privateDataSpecifier);
    static bool DecodeContentIdentifierDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  map<unsigned char, char*>& crids);
    static bool DecodeDishBevRatingDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned char& starRating,
                                              unsigned char& mpaaClassification,
                                              unsigned short& advisories);
    static bool DecodeDishTextDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          unsigned char tableId,
                                          char** text);
    static bool DecodeDishEpisodeInformationDescriptor(const unsigned char* data,
                                                        unsigned char dataLength,
                                                        unsigned char tableId,
                                                        char** information);
    static bool DecodeDishVchipDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          unsigned char& vchipRating,
                                          unsigned short& advisories);
    static bool DecodeDishBevSeriesDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              char** seriesId,
                                              char** episodeId,
                                              bool& isPreviouslyShown);
    static bool DecodePremiereOrderInformationDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          char** orderNumber,
                                                          char** price,
                                                          char** phoneNumber,
                                                          char** smsNumber,
                                                          char** url);
    static bool DecodePremiereParentInformationDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          unsigned char& rating,
                                                          char** text);
    static bool DecodePremiereContentTransmissionDescriptor(const unsigned char* data,
                                                            unsigned char dataLength,
                                                            unsigned short& originalNetworkId,
                                                            unsigned short& transportStreamId,
                                                            unsigned short& serviceId,
                                                            vector<unsigned long long>& showings);

    CCriticalSection m_section;
    map<unsigned short, bool> m_grabPids;
    bool m_grabFreesat;
    unsigned short m_freesatPidBat;
    unsigned short m_freesatPidEitPf;
    unsigned short m_freesatPidEitSchedule;
    unsigned short m_freesatPidNit;
    unsigned short m_freesatPidPmt;
    unsigned short m_freesatPidSdt;
    bool m_isSeen;
    bool m_isReady;
    clock_t m_completeTime;
    unsigned long m_unseenTableCount;
    unsigned long m_unseenSegmentCount;
    unsigned long m_unseenSectionCount;
    ICallBackGrabber* m_callBackGrabber;
    ICallBackPidConsumer* m_callBackPidConsumer;
    ISectionDispatcher* m_sectionDispatcher;
    IDefaultAuthorityProvider* m_defaultAuthorityProvider;
    map<unsigned short, CSectionDecoder*> m_decoders;
    map<unsigned long long, CRecordEitService*> m_services;
    bool m_enableCrcCheck;

    CRecordEitService* m_currentService;
    unsigned short m_currentServiceIndex;
    CRecordEitEventMinimal* m_currentEvent;
    unsigned short m_currentEventIndex;
    CRecordEitEventText* m_currentEventText;
    unsigned char m_currentEventTextIndex;
    CRecordEitEvent* m_referenceEvent;
    unsigned short m_referenceServiceId;
    unsigned long long m_referenceEventId;
};