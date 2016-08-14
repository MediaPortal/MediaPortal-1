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
#include "..\..\shared\Section.h"
#include "CriticalSection.h"
#include "ICallBackNtt.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_NTT 0xc3


extern void LogDebug(const wchar_t* fmt, ...);

class CParserNtt
{
  public:
    CParserNtt();
    virtual ~CParserNtt();

    void Reset();
    void SetCallBack(ICallBackNtt* callBack);
    void OnNewSection(CSection& section);
    bool IsSeen() const;
    bool IsReady() const;

    bool GetTransponderName(unsigned char transmissionMedium,
                            unsigned char satelliteId,
                            unsigned char transponderNumber,
                            unsigned long& language,
                            char* name,
                            unsigned short& nameBufferSize) const;
    bool GetSatelliteText(unsigned char transmissionMedium,
                          unsigned char satelliteId,
                          unsigned long& language,
                          char* referenceName,
                          unsigned short& referenceNameBufferSize,
                          char* fullName,
                          unsigned short& fullNameBufferSize) const;
    bool GetRatingsText(unsigned char transmissionMedium,
                        unsigned char ratingRegion,
                        unsigned char dimensionIndex,
                        unsigned char levelIndex,
                        unsigned long& language,
                        char* dimensionName,
                        unsigned short& dimensionNameBufferSize,
                        char* levelName,
                        unsigned short& levelNameBufferSize) const;
    bool GetRatingSystem(unsigned char transmissionMedium,
                          unsigned char ratingRegion,
                          unsigned long& language,
                          char* name,
                          unsigned short& nameBufferSize) const;
    bool GetSourceName(unsigned char transmissionMedium,
                        bool applicationType,
                        unsigned short sourceId,
                        unsigned long& language,
                        char* name,
                        unsigned short& nameBufferSize) const;
    bool GetMapName(unsigned char transmissionMedium,
                    unsigned short vctId,
                    unsigned long& language,
                    char* name,
                    unsigned short& nameBufferSize) const;
    bool GetCurrencySystem(unsigned char transmissionMedium,
                            unsigned char currencyRegion,
                            unsigned long& language,
                            char* name,
                            unsigned short& nameBufferSize) const;

  private:
    class CRecordNtt : public IRecord
    {
      public:
        CRecordNtt()
        {
          TransmissionMedium = 0;
          Language = 0;
          Name = NULL;
        }

        virtual ~CRecordNtt()
        {
          if (Name != NULL)
          {
            delete[] Name;
            Name = NULL;
          }
        }

        virtual bool Equals(const IRecord* record) const
        {
          const CRecordNtt* recordNtt = dynamic_cast<const CRecordNtt*>(record);
          if (
            recordNtt == NULL ||
            TransmissionMedium != recordNtt->TransmissionMedium ||
            Language != recordNtt->Language ||
            !CUtils::CompareStrings(Name, recordNtt->Name)
          )
          {
            return false;
          }
          return true;
        }

        virtual unsigned long long GetExpiryKey() const
        {
          return TransmissionMedium;
        }

        unsigned char TransmissionMedium;
        unsigned long Language;
        char* Name;
    };

    class CRecordNttTransponderName : public CRecordNtt
    {
      public:
        CRecordNttTransponderName()
        {
          Index = 0;
          SatelliteId = 0;
          TransponderNumber = 0;
        }

        ~CRecordNttTransponderName()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNtt::Equals(record))
          {
            return false;
          }

          const CRecordNttTransponderName* recordTransponderName = dynamic_cast<const CRecordNttTransponderName*>(record);
          if (
            recordTransponderName == NULL ||
            Index != recordTransponderName->Index ||
            SatelliteId != recordTransponderName->SatelliteId ||
            TransponderNumber != recordTransponderName->TransponderNumber
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 16) | (SatelliteId << 8) | TransponderNumber;
        }

        unsigned long long GetExpiryKey() const
        {
          return (TransmissionMedium << 8) | SatelliteId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NTT: transponder name %s, transmission medium = %hhu, index = %hhu, satellite ID = %hhu, transponder number = %hhu, language = %S, name = %S",
                    situation, TransmissionMedium, Index, SatelliteId,
                    TransponderNumber, (char*)&Language, Name == NULL ? "" : Name);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttTntReceived(TransmissionMedium,
                                          Index,
                                          SatelliteId,
                                          TransponderNumber,
                                          Language,
                                          Name);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttTntChanged(TransmissionMedium,
                                          Index,
                                          SatelliteId,
                                          TransponderNumber,
                                          Language,
                                          Name);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttTntRemoved(TransmissionMedium,
                                          Index,
                                          SatelliteId,
                                          TransponderNumber,
                                          Language,
                                          Name);
          }
        }

        unsigned char Index;
        unsigned char SatelliteId;
        unsigned char TransponderNumber;
    };

    class CRecordNttSatelliteText : public CRecordNtt
    {
      public:
        CRecordNttSatelliteText()
        {
          Index = 0;
          SatelliteId = 0;
          ReferenceName = NULL;
        }

        ~CRecordNttSatelliteText()
        {
          if (ReferenceName != NULL)
          {
            delete[] ReferenceName;
            ReferenceName = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNtt::Equals(record))
          {
            return false;
          }

          const CRecordNttSatelliteText* recordSatelliteText = dynamic_cast<const CRecordNttSatelliteText*>(record);
          if (
            recordSatelliteText == NULL ||
            Index != recordSatelliteText->Index ||
            SatelliteId != recordSatelliteText->SatelliteId ||
            !CUtils::CompareStrings(ReferenceName, recordSatelliteText->ReferenceName)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 8) | SatelliteId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NTT: satellite text %s, transmission medium = %hhu, index = %hhu, satellite ID = %hhu, language = %S, reference name = %S, full name = %S",
                    situation, TransmissionMedium, Index, SatelliteId,
                    (char*)&Language,
                    ReferenceName == NULL ? "" : ReferenceName,
                    Name == NULL ? "" : Name);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttSttReceived(TransmissionMedium,
                                          Index,
                                          SatelliteId,
                                          Language,
                                          ReferenceName,
                                          Name);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttSttChanged(TransmissionMedium,
                                          Index,
                                          SatelliteId,
                                          Language,
                                          ReferenceName,
                                          Name);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttSttRemoved(TransmissionMedium,
                                          Index,
                                          SatelliteId,
                                          Language,
                                          ReferenceName,
                                          Name);
          }
        }

        unsigned char Index;
        unsigned char SatelliteId;
        char* ReferenceName;
    };

    class CRecordNttRatingsText : public CRecordNtt
    {
      public:
        CRecordNttRatingsText()
        {
          RatingRegion = 0;
          DimensionIndex = 0;
          DimensionName = NULL;
          LevelIndex = 0;
          LevelName = NULL;
        }

        ~CRecordNttRatingsText()
        {
          if (DimensionName != NULL)
          {
            delete[] DimensionName;
            DimensionName = NULL;
          }
          if (LevelName != NULL)
          {
            delete[] LevelName;
            LevelName = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNtt::Equals(record))
          {
            return false;
          }

          const CRecordNttRatingsText* recordRatingsText = dynamic_cast<const CRecordNttRatingsText*>(record);
          if (
            recordRatingsText == NULL ||
            RatingRegion != recordRatingsText->RatingRegion ||
            DimensionIndex != recordRatingsText->DimensionIndex ||
            !CUtils::CompareStrings(DimensionName, recordRatingsText->DimensionName) ||
            LevelIndex != recordRatingsText->LevelIndex ||
            !CUtils::CompareStrings(LevelName, recordRatingsText->LevelName)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 24) | (RatingRegion << 16) | (DimensionIndex << 8) | LevelIndex;
        }

        unsigned long long GetExpiryKey() const
        {
          return (TransmissionMedium << 8) | RatingRegion;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NTT: ratings text %s, transmission medium = %hhu, rating region = %hhu, language = %S, dimension index = %hhu, dimension name = %S, level index = %hhu, level name = %S",
                    situation, TransmissionMedium, RatingRegion, (char*)&Language,
                    DimensionIndex, DimensionName == NULL ? "" : DimensionName,
                    LevelIndex, LevelName == NULL ? "" : LevelName);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttRttReceived(TransmissionMedium,
                                          RatingRegion,
                                          Language,
                                          DimensionIndex,
                                          DimensionName,
                                          LevelIndex,
                                          LevelName);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttRttChanged(TransmissionMedium,
                                          RatingRegion,
                                          Language,
                                          DimensionIndex,
                                          DimensionName,
                                          LevelIndex,
                                          LevelName);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttRttRemoved(TransmissionMedium,
                                          RatingRegion,
                                          Language,
                                          DimensionIndex,
                                          DimensionName,
                                          LevelIndex,
                                          LevelName);
          }
        }

        unsigned char RatingRegion;
        unsigned char DimensionIndex;
        char* DimensionName;
        unsigned char LevelIndex;
        char* LevelName;
    };

    class CRecordNttRatingSystem : public CRecordNtt
    {
      public:
        CRecordNttRatingSystem()
        {
          RatingRegion = 0;
        }

        ~CRecordNttRatingSystem()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNtt::Equals(record))
          {
            return false;
          }

          const CRecordNttRatingSystem* recordRatingSystem = dynamic_cast<const CRecordNttRatingSystem*>(record);
          if (
            recordRatingSystem == NULL ||
            RatingRegion != recordRatingSystem->RatingRegion
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 8) | RatingRegion;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NTT: rating system %s, transmission medium = %hhu, rating region = %hhu, language = %S, name = %S",
                    situation, TransmissionMedium, RatingRegion,
                    (char*)&Language, Name == NULL ? "" : Name);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttRstReceived(TransmissionMedium, RatingRegion, Language, Name);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttRstChanged(TransmissionMedium, RatingRegion, Language, Name);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttRstRemoved(TransmissionMedium, RatingRegion, Language, Name);
          }
        }

        unsigned char RatingRegion;
    };

    class CRecordNttSourceName : public CRecordNtt
    {
      public:
        CRecordNttSourceName()
        {
          ApplicationType = false;
          SourceId = 0;
        }

        ~CRecordNttSourceName()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNtt::Equals(record))
          {
            return false;
          }

          const CRecordNttSourceName* recordSourceName = dynamic_cast<const CRecordNttSourceName*>(record);
          if (
            recordSourceName == NULL ||
            ApplicationType != recordSourceName->ApplicationType ||
            SourceId != recordSourceName->SourceId
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 24) | (ApplicationType << 16) | SourceId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NTT: source name %s, transmission medium = %hhu, application type = %d, source ID = %hu, language = %S, name = %S",
                    situation, TransmissionMedium, ApplicationType, SourceId,
                    Language, Name == NULL ? "" : Name);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttSntReceived(TransmissionMedium,
                                          ApplicationType,
                                          SourceId,
                                          Language,
                                          Name);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttSntChanged(TransmissionMedium,
                                          ApplicationType,
                                          SourceId,
                                          Language,
                                          Name);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttSntRemoved(TransmissionMedium,
                                          ApplicationType,
                                          SourceId,
                                          Language,
                                          Name);
          }
        }

        bool ApplicationType;
        unsigned short SourceId;
    };

    class CRecordNttMapName : public CRecordNtt
    {
      public:
        CRecordNttMapName()
        {
          VctId = 0;
        }

        ~CRecordNttMapName()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNtt::Equals(record))
          {
            return false;
          }

          const CRecordNttMapName* recordMapName = dynamic_cast<const CRecordNttMapName*>(record);
          if (
            recordMapName == NULL ||
            VctId != recordMapName->VctId
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 16) | VctId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NTT: map name %s, transmission medium = %hhu, VCT ID = %hu, language = %S, name = %S",
                    situation, TransmissionMedium, VctId, (char*)&Language,
                    Name == NULL ? "" : Name);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttMntReceived(TransmissionMedium, VctId, Language, Name);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttMntChanged(TransmissionMedium, VctId, Language, Name);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttMntRemoved(TransmissionMedium, VctId, Language, Name);
          }
        }

        unsigned short VctId;
    };

    class CRecordNttCurrencySystem : public CRecordNtt
    {
      public:
        CRecordNttCurrencySystem()
        {
          CurrencyRegion = 0;
        }

        ~CRecordNttCurrencySystem()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNtt::Equals(record))
          {
            return false;
          }

          const CRecordNttCurrencySystem* recordCurrencySystem = dynamic_cast<const CRecordNttCurrencySystem*>(record);
          if (
            recordCurrencySystem == NULL ||
            CurrencyRegion != recordCurrencySystem->CurrencyRegion
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return (TransmissionMedium << 8) | CurrencyRegion;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NTT: currency system %s, transmission medium = %hhu, currency region = %hhu, language = %S, name = %S",
                    situation, TransmissionMedium, CurrencyRegion,
                    (char*)&Language, Name == NULL ? "" : Name);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttCstReceived(TransmissionMedium, CurrencyRegion, Language, Name);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttCstChanged(TransmissionMedium, CurrencyRegion, Language, Name);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNtt* callBackNtt = static_cast<ICallBackNtt*>(callBack);
          if (callBackNtt != NULL)
          {
            callBackNtt->OnNttCstRemoved(TransmissionMedium, CurrencyRegion, Language, Name);
          }
        }

        unsigned char CurrencyRegion;
    };

    bool IsReadyPrivate(unsigned char transmissionMedium) const;
    template<class T> static void CleanUpRecords(vector<T*>& records);

    static bool DecodeSection(CSection& section,
                              unsigned char& tableSubtype6Interpretation,
                              unsigned char& protocolVersion,
                              unsigned long& iso639LanguageCode,
                              unsigned char& transmissionMedium,
                              unsigned char& tableSubtype,
                              unsigned char& satelliteId,
                              unsigned char& ratingRegion,
                              vector<CRecordNtt*>& records,
                              bool& seenRevisionDescriptor,
                              unsigned char& tableVersionNumber,
                              unsigned char& sectionNumber,
                              unsigned char& lastSectionNumber);

    static bool DecodeTransponderNameSubTable(unsigned char* data,
                                              unsigned short& pointer,
                                              unsigned short endOfSection,
                                              unsigned char& satelliteId,
                                              vector<CRecordNttTransponderName*>& records);
    static bool DecodeSatelliteTextSubTable(unsigned char* data,
                                            unsigned short& pointer,
                                            unsigned short endOfSection,
                                            vector<CRecordNttSatelliteText*>& records);
    static bool DecodeRatingsTextSubTable(unsigned char* data,
                                          unsigned short& pointer,
                                          unsigned short endOfSection,
                                          unsigned char& ratingRegion,
                                          vector<CRecordNttRatingsText*>& records);
    static bool DecodeRatingSystemSubTable(unsigned char* data,
                                            unsigned short& pointer,
                                            unsigned short endOfSection,
                                            vector<CRecordNttRatingSystem*>& records);
    static bool DecodeSourceNameSubTable(unsigned char* data,
                                          unsigned short& pointer,
                                          unsigned short endOfSection,
                                          vector<CRecordNttSourceName*>& records);
    static bool DecodeMapNameSubTable(unsigned char* data,
                                      unsigned short& pointer,
                                      unsigned short endOfSection,
                                      vector<CRecordNttMapName*>& records);
    static bool DecodeCurrencySystemSubTable(unsigned char* data,
                                              unsigned short& pointer,
                                              unsigned short endOfSection,
                                              vector<CRecordNttCurrencySystem*>& records);

    static bool DecodeRevisionDetectionDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned char& tableVersionNumber,
                                                  unsigned char& sectionNumber,
                                                  unsigned char& lastSectionNumber);

    CCriticalSection m_section;
    vector<unsigned long> m_seenSections;
    vector<unsigned long> m_unseenSections;
    bool m_isReady;
    ICallBackNtt* m_callBack;
    CRecordStore m_recordsTransponderName;
    CRecordStore m_recordsSatelliteText;
    CRecordStore m_recordsRatingsText;
    CRecordStore m_recordsRatingSystem;
    CRecordStore m_recordsSourceName;
    CRecordStore m_recordsMapName;
    CRecordStore m_recordsCurrencySystem;
    clock_t m_completeTimeSnt;
    unsigned char m_tableSubtype6Interpretation;
};