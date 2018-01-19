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
#include "..\..\shared\CriticalSection.h"
#include "..\..\shared\ISectionDispatcher.h"
#include "..\..\shared\Section.h"
#include "..\..\shared\SectionDecoder.h"
#include "ICallBackSdt.h"
#include "IDefaultAuthorityProvider.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define PID_SDT 0x11
#define TABLE_ID_SDT_ACTUAL 0x42
#define TABLE_ID_SDT_OTHER 0x46


extern void LogDebug(const wchar_t* fmt, ...);

class CParserSdt : public CSectionDecoder, public IDefaultAuthorityProvider
{
  public:
    CParserSdt(ISectionDispatcher* sectionDispatcher);
    virtual ~CParserSdt();

    void Reset(bool enableCrcCheck);
    void SetCallBack(ICallBackSdt* callBack);
    void OnNewSection(const CSection& section);
    bool IsSeenActual() const;
    bool IsSeenOther() const;
    bool IsReadyActual() const;
    bool IsReadyOther() const;

    void GetServiceCount(unsigned short& actualOriginalNetworkId,
                          unsigned short& serviceCount) const;
    bool GetService(unsigned short index,
                    unsigned char& tableId,
                    unsigned short& originalNetworkId,
                    unsigned short& transportStreamId,
                    unsigned short& serviceId,
                    bool& eitScheduleFlag,
                    bool& eitPresentFollowingFlag,
                    unsigned char& runningStatus,
                    bool& freeCaMode,
                    unsigned char& serviceType,
                    unsigned char& serviceNameCount,
                    unsigned short& logicalChannelNumber,
                    unsigned char& dishSubChannelNumber,
                    bool& visibleInGuide,
                    unsigned short& referenceServiceId,
                    bool& isHighDefinition,
                    bool& isStandardDefinition,
                    bool& isThreeDimensional,
                    unsigned short& streamCountVideo,
                    unsigned short& streamCountAudio,
                    unsigned long* audioLanguages,
                    unsigned char& audioLanguageCount,
                    unsigned long* subtitlesLanguages,
                    unsigned char& subtitlesLanguageCount,
                    unsigned char& cyfrowyPolsatChannelCategoryId,
                    unsigned char* openTvChannelCategoryIds,
                    unsigned char& openTvChannelCategoryIdCount,
                    unsigned char& virginMediaChannelCategoryId,
                    unsigned short& dishMarketId,
                    unsigned long* availableInCountries,
                    unsigned char& availableInCountryCount,
                    unsigned long* unavailableInCountries,
                    unsigned char& unavailableInCountryCount,
                    unsigned long* availableInCells,
                    unsigned char& availableInCellCount,
                    unsigned long* unavailableInCells,
                    unsigned char& unavailableInCellCount,
                    unsigned long long* targetRegionIds,
                    unsigned char& targetRegionIdCount,
                    unsigned short& previousOriginalNetworkId,
                    unsigned short& previousTransportStreamId,
                    unsigned short& previousServiceId,
                    unsigned short& epgOriginalNetworkId,
                    unsigned short& epgTransportStreamId,
                    unsigned short& epgServiceId);
    bool GetServiceNameByIndex(unsigned short serviceIndex,
                                unsigned char nameIndex,
                                unsigned long& language,
                                char* providerName,
                                unsigned short& providerNameBufferSize,
                                char* serviceName,
                                unsigned short& serviceNameBufferSize);
    bool GetServiceNameByLanguage(unsigned short serviceIndex,
                                  unsigned long language,
                                  char* providerName,
                                  unsigned short& providerNameBufferSize,
                                  char* serviceName,
                                  unsigned short& serviceNameBufferSize);
    bool GetDefaultAuthority(unsigned short originalNetworkId,
                              unsigned short transportStreamId,
                              unsigned short serviceId,
                              char* defaultAuthority,
                              unsigned short& defaultAuthorityBufferSize) const;

  private:
    class CRecordSdt : public IRecord
    {
      public:
        CRecordSdt()
        {
          TableId = 0;
          OriginalNetworkId = 0;
          TransportStreamId = 0;
          ServiceId = 0;
          EitScheduleFlag = false;
          EitPresentFollowingFlag = false;
          RunningStatus = 0;
          FreeCaMode = false;
          ServiceType = 0;
          LogicalChannelNumber = 0;
          DishSubChannelNumber = 0;
          VisibleInGuide = true;
          ReferenceServiceId = 0;
          IsHighDefinition = false;
          IsStandardDefinition = false;
          IsThreeDimensional = false;
          StreamCountVideo = 0;
          StreamCountAudio = 0;
          CyfrowyPolsatChannelCategoryId = 0xff;
          VirginMediaChannelCategoryId = 0;
          DishMarketId = 0;
          PreviousOriginalNetworkId = 0;
          PreviousTransportStreamId = 0;
          PreviousServiceId = 0;
          EpgOriginalNetworkId = 0;
          EpgTransportStreamId = 0;
          EpgServiceId = 0;
          DefaultAuthority = NULL;
        }

        ~CRecordSdt()
        {
          CUtils::CleanUpStringSet(ProviderNames);
          CUtils::CleanUpStringSet(ServiceNames);

          if (DefaultAuthority != NULL)
          {
            delete[] DefaultAuthority;
            DefaultAuthority = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordSdt* recordSdt = dynamic_cast<const CRecordSdt*>(record);
          if (
            recordSdt == NULL ||
            TableId != recordSdt->TableId ||
            OriginalNetworkId != recordSdt->OriginalNetworkId ||
            TransportStreamId != recordSdt->TransportStreamId ||
            ServiceId != recordSdt->ServiceId ||
            EitScheduleFlag != recordSdt->EitScheduleFlag ||
            EitPresentFollowingFlag != recordSdt->EitPresentFollowingFlag ||
            RunningStatus != recordSdt->RunningStatus ||
            FreeCaMode != recordSdt->FreeCaMode ||
            ServiceType != recordSdt->ServiceType ||
            !CUtils::CompareStringSets(ProviderNames, recordSdt->ProviderNames) ||
            !CUtils::CompareStringSets(ServiceNames, recordSdt->ServiceNames) ||
            LogicalChannelNumber != recordSdt->LogicalChannelNumber ||
            DishSubChannelNumber != recordSdt->DishSubChannelNumber ||
            VisibleInGuide != recordSdt->VisibleInGuide ||
            ReferenceServiceId != recordSdt->ReferenceServiceId ||
            IsHighDefinition != recordSdt->IsHighDefinition ||
            IsStandardDefinition != recordSdt->IsStandardDefinition ||
            IsThreeDimensional != recordSdt->IsThreeDimensional ||
            StreamCountVideo != recordSdt->StreamCountVideo ||
            StreamCountAudio != recordSdt->StreamCountAudio ||
            !CUtils::CompareVectors(AudioLanguages, recordSdt->AudioLanguages) ||
            !CUtils::CompareVectors(SubtitlesLanguages, recordSdt->SubtitlesLanguages) ||
            CyfrowyPolsatChannelCategoryId != recordSdt->CyfrowyPolsatChannelCategoryId ||
            !CUtils::CompareVectors(OpenTvChannelCategoryIds, recordSdt->OpenTvChannelCategoryIds) ||
            VirginMediaChannelCategoryId != recordSdt->VirginMediaChannelCategoryId ||
            DishMarketId != recordSdt->DishMarketId ||
            !CUtils::CompareVectors(AvailableInCountries, recordSdt->AvailableInCountries) ||
            !CUtils::CompareVectors(UnavailableInCountries, recordSdt->UnavailableInCountries) ||
            !CUtils::CompareVectors(AvailableInCells, recordSdt->AvailableInCells) ||
            !CUtils::CompareVectors(UnavailableInCells, recordSdt->UnavailableInCells) ||
            !CUtils::CompareVectors(TargetRegionIds, recordSdt->TargetRegionIds) ||
            PreviousOriginalNetworkId != recordSdt->PreviousOriginalNetworkId ||
            PreviousTransportStreamId != recordSdt->PreviousTransportStreamId ||
            PreviousServiceId != recordSdt->PreviousServiceId ||
            EpgOriginalNetworkId != recordSdt->EpgOriginalNetworkId ||
            EpgTransportStreamId != recordSdt->EpgTransportStreamId ||
            EpgServiceId != recordSdt->EpgServiceId ||
            !CUtils::CompareStrings(DefaultAuthority, recordSdt->DefaultAuthority)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)TableId << 48) | ((unsigned long long)OriginalNetworkId << 32) | (TransportStreamId << 16) | ServiceId;
        }

        unsigned long long GetExpiryKey() const
        {
          return ((unsigned long long)TableId << 32) | (OriginalNetworkId << 16) | TransportStreamId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"SDT: service %s, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, EIT schedule flag = %d, EIT present following flag = %d, running status = %hhu, free CA mode = %d, service type = %hhu, provider name count = %llu, service name count = %llu, LCN = %hu, Dish sub-channel number = %hhu, visible in guide = %d, reference service ID = %hu, is HD = %d, is SD = %d, is 3D = %d, video stream count = %hu, audio stream count = %hu, audio language count = %llu, subtitles language count = %llu, Cyfrowy Polsat channel category ID = %hhu, OpenTV channel category ID count = %llu, Virgin Media channel category ID = %hhu, Dish market ID = %hu, country counts = %llu / %llu, cell counts = %llu / %llu, target region count = %llu, prev. ONID = %hu, prev. TSID = %hu, prev. service ID = %hu, EPG ONID = %hu, EPG TSID = %hu, EPG service ID = %hu, default authority = %S",
                    situation, TableId, OriginalNetworkId, TransportStreamId,
                    ServiceId, EitScheduleFlag, EitPresentFollowingFlag,
                    RunningStatus, FreeCaMode, ServiceType,
                    (unsigned long long)ProviderNames.size(),
                    (unsigned long long)ServiceNames.size(),
                    LogicalChannelNumber, DishSubChannelNumber, VisibleInGuide,
                    ReferenceServiceId, IsHighDefinition, IsStandardDefinition,
                    IsThreeDimensional, StreamCountVideo, StreamCountAudio,
                    (unsigned long long)AudioLanguages.size(),
                    (unsigned long long)SubtitlesLanguages.size(),
                    CyfrowyPolsatChannelCategoryId,
                    (unsigned long long)OpenTvChannelCategoryIds.size(),
                    VirginMediaChannelCategoryId, DishMarketId,
                    (unsigned long long)AvailableInCountries.size(),
                    (unsigned long long)UnavailableInCountries.size(),
                    (unsigned long long)AvailableInCells.size(),
                    (unsigned long long)UnavailableInCells.size(),
                    (unsigned long long)TargetRegionIds.size(),
                    PreviousOriginalNetworkId, PreviousTransportStreamId,
                    PreviousServiceId, EpgOriginalNetworkId,
                    EpgTransportStreamId, EpgServiceId,
                    DefaultAuthority == NULL ? "" : DefaultAuthority);

          CUtils::DebugStringMap(ServiceNames, L"name(s)", L"language", L"name");
          CUtils::DebugStringMap(ProviderNames, L"provider name(s)", L"language", L"name");
          CUtils::DebugVector(AudioLanguages, L"audio language(s)", true);
          CUtils::DebugVector(SubtitlesLanguages, L"subtitles language(s)", true);
          CUtils::DebugVector(OpenTvChannelCategoryIds, L"OpenTV channel category ID(s)", false);
          CUtils::DebugVector(AvailableInCountries, L"available in countries", true);
          CUtils::DebugVector(UnavailableInCountries, L"unavailable in countries", true);
          CUtils::DebugVector(AvailableInCells, L"available in cell(s)", false);
          CUtils::DebugVector(UnavailableInCells, L"unavailable in cell(s)", false);
          CUtils::DebugVector(TargetRegionIds, L"target region ID(s)", false);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackSdt* callBackSdt = static_cast<ICallBackSdt*>(callBack);
          if (callBackSdt != NULL)
          {
            callBackSdt->OnSdtReceived(TableId,
                                        OriginalNetworkId,
                                        TransportStreamId,
                                        ServiceId,
                                        EitScheduleFlag,
                                        EitPresentFollowingFlag,
                                        RunningStatus,
                                        FreeCaMode,
                                        ServiceType,
                                        ProviderNames,
                                        ServiceNames,
                                        LogicalChannelNumber,
                                        DishSubChannelNumber,
                                        VisibleInGuide,
                                        ReferenceServiceId,
                                        IsHighDefinition,
                                        IsStandardDefinition,
                                        IsThreeDimensional,
                                        StreamCountVideo,
                                        StreamCountAudio,
                                        AudioLanguages,
                                        SubtitlesLanguages,
                                        CyfrowyPolsatChannelCategoryId,
                                        OpenTvChannelCategoryIds,
                                        VirginMediaChannelCategoryId,
                                        DishMarketId,
                                        AvailableInCountries,
                                        UnavailableInCountries,
                                        AvailableInCells,
                                        UnavailableInCells,
                                        TargetRegionIds,
                                        PreviousOriginalNetworkId,
                                        PreviousTransportStreamId,
                                        PreviousServiceId,
                                        EpgOriginalNetworkId,
                                        EpgTransportStreamId,
                                        EpgServiceId,
                                        DefaultAuthority);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackSdt* callBackSdt = static_cast<ICallBackSdt*>(callBack);
          if (callBackSdt != NULL)
          {
            callBackSdt->OnSdtChanged(TableId,
                                        OriginalNetworkId,
                                        TransportStreamId,
                                        ServiceId,
                                        EitScheduleFlag,
                                        EitPresentFollowingFlag,
                                        RunningStatus,
                                        FreeCaMode,
                                        ServiceType,
                                        ProviderNames,
                                        ServiceNames,
                                        LogicalChannelNumber,
                                        DishSubChannelNumber,
                                        VisibleInGuide,
                                        ReferenceServiceId,
                                        IsHighDefinition,
                                        IsStandardDefinition,
                                        IsThreeDimensional,
                                        StreamCountVideo,
                                        StreamCountAudio,
                                        AudioLanguages,
                                        SubtitlesLanguages,
                                        CyfrowyPolsatChannelCategoryId,
                                        OpenTvChannelCategoryIds,
                                        VirginMediaChannelCategoryId,
                                        DishMarketId,
                                        AvailableInCountries,
                                        UnavailableInCountries,
                                        AvailableInCells,
                                        UnavailableInCells,
                                        TargetRegionIds,
                                        PreviousOriginalNetworkId,
                                        PreviousTransportStreamId,
                                        PreviousServiceId,
                                        EpgOriginalNetworkId,
                                        EpgTransportStreamId,
                                        EpgServiceId,
                                        DefaultAuthority);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackSdt* callBackSdt = static_cast<ICallBackSdt*>(callBack);
          if (callBackSdt != NULL)
          {
            callBackSdt->OnSdtRemoved(TableId,
                                        OriginalNetworkId,
                                        TransportStreamId,
                                        ServiceId,
                                        EitScheduleFlag,
                                        EitPresentFollowingFlag,
                                        RunningStatus,
                                        FreeCaMode,
                                        ServiceType,
                                        ProviderNames,
                                        ServiceNames,
                                        LogicalChannelNumber,
                                        DishSubChannelNumber,
                                        VisibleInGuide,
                                        ReferenceServiceId,
                                        IsHighDefinition,
                                        IsStandardDefinition,
                                        IsThreeDimensional,
                                        StreamCountVideo,
                                        StreamCountAudio,
                                        AudioLanguages,
                                        SubtitlesLanguages,
                                        CyfrowyPolsatChannelCategoryId,
                                        OpenTvChannelCategoryIds,
                                        VirginMediaChannelCategoryId,
                                        DishMarketId,
                                        AvailableInCountries,
                                        UnavailableInCountries,
                                        AvailableInCells,
                                        UnavailableInCells,
                                        TargetRegionIds,
                                        PreviousOriginalNetworkId,
                                        PreviousTransportStreamId,
                                        PreviousServiceId,
                                        EpgOriginalNetworkId,
                                        EpgTransportStreamId,
                                        EpgServiceId,
                                        DefaultAuthority);
          }
        }

        unsigned char TableId;
        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;
        unsigned short ServiceId;
        bool EitScheduleFlag;
        bool EitPresentFollowingFlag;
        unsigned char RunningStatus;
        bool FreeCaMode;
        unsigned char ServiceType;
        map<unsigned long, char*> ProviderNames;
        map<unsigned long, char*> ServiceNames;
        unsigned short LogicalChannelNumber;
        unsigned char DishSubChannelNumber;
        bool VisibleInGuide;
        unsigned short ReferenceServiceId;
        bool IsHighDefinition;
        bool IsStandardDefinition;
        bool IsThreeDimensional;
        unsigned short StreamCountVideo;
        unsigned short StreamCountAudio;
        vector<unsigned long> AudioLanguages;
        vector<unsigned long> SubtitlesLanguages;
        unsigned char CyfrowyPolsatChannelCategoryId;
        vector<unsigned char> OpenTvChannelCategoryIds;
        unsigned char VirginMediaChannelCategoryId;
        unsigned short DishMarketId;
        vector<unsigned long> AvailableInCountries;
        vector<unsigned long> UnavailableInCountries;
        vector<unsigned long> AvailableInCells;
        vector<unsigned long> UnavailableInCells;
        vector<unsigned long long> TargetRegionIds;
        unsigned short PreviousOriginalNetworkId;
        unsigned short PreviousTransportStreamId;
        unsigned short PreviousServiceId;
        unsigned short EpgOriginalNetworkId;
        unsigned short EpgTransportStreamId;
        unsigned short EpgServiceId;
        char* DefaultAuthority;
    };

    bool SelectServiceRecordByIndex(unsigned short index);

    static bool DecodeServiceRecord(const unsigned char* sectionData,
                                    unsigned short& pointer,
                                    unsigned short endOfSection,
                                    CRecordSdt& record);
    static bool DecodeServiceDescriptors(const unsigned char* sectionData,
                                          unsigned short& pointer,
                                          unsigned short endOfDescriptorLoop,
                                          CRecordSdt& record);

    static bool DecodeServiceDescriptor(const unsigned char* data,
                                        unsigned char dataLength,
                                        unsigned char& serviceType,
                                        char** providerName,
                                        char** serviceName);
    static bool DecodeCountryAvailabilityDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    vector<unsigned long>& availableInCountries,
                                                    vector<unsigned long>& unavailableInCountries);
    static bool DecodeTimeShiftedServiceDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned short& referenceServiceId);
    static bool DecodeComponentDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          bool& isVideo,
                                          bool& isAudio,
                                          bool& isSubtitles,
                                          bool& isHighDefinition,
                                          bool& isStandardDefinition,
                                          bool& isThreeDimensional,
                                          unsigned long& language);
    static bool DecodeMultilingualServiceNameDescriptor(const unsigned char* data,
                                                        unsigned char dataLength,
                                                        map<unsigned long, char*>& serviceNames,
                                                        map<unsigned long, char*>& providerNames);
    static bool DecodePrivateDataSpecifierDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned long& privateDataSpecifier);
    static bool DecodeServiceAvailabilityDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    vector<unsigned long>& availableInCells,
                                                    vector<unsigned long>& unavailableInCells);
    static bool DecodeDefaultAuthorityDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  char** defaultAuthority);
    static bool DecodeTargetRegionDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              vector<unsigned long long>& targetRegionIds);
    static bool DecodeServiceRelocatedDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned short& previousOriginalNetworkId,
                                                  unsigned short& previousTransportStreamId,
                                                  unsigned short& previousServiceId);
    static bool DecodeDishChannelDescriptor(const unsigned char* data,
                                            unsigned char dataLength,
                                            unsigned short& marketId,
                                            unsigned short& logicalChannelNumber);
    static bool DecodeDishEpgLinkDescriptor(const unsigned char* data,
                                            unsigned char dataLength,
                                            unsigned short& originalNetworkId,
                                            unsigned short& transportStreamId,
                                            unsigned short& serviceId);
    static bool DecodeDishSubChannelDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                bool& isHighDefinition,
                                                unsigned short& majorChannelNumber,
                                                unsigned char& minorChannelNumber);
    static bool DecodeOpenTvChannelDescriptionDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          bool isItalianText,
                                                          vector<unsigned char>& channelCategoryIds);
    static bool DecodeOpenTvNvodTimeShiftedServiceNameDescriptor(const unsigned char* data,
                                                                  unsigned char dataLength,
                                                                  char** serviceName);
    static bool DecodeVirginMediaChannelDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned short& logicalChannelNumber,
                                                    bool& visibleInGuide,
                                                    unsigned char& channelCategoryId,
                                                    bool& isHighDefinition);
    static bool DecodeCyfrowyPolsatChannelCategoryDescriptor(const unsigned char* data,
                                                              unsigned char dataLength,
                                                              unsigned char& channelCategoryId);

    CCriticalSection m_section;
    vector<unsigned long long> m_seenSectionsActual;
    vector<unsigned long long> m_unseenSectionsActual;
    vector<unsigned long long> m_seenSectionsOther;
    vector<unsigned long long> m_unseenSectionsOther;
    bool m_isOtherReady;
    clock_t m_otherCompleteTime;
    ICallBackSdt* m_callBack;
    CRecordStore m_records;

    unsigned short m_actualOriginalNetworkId;
    CRecordSdt* m_currentRecord;
    unsigned short m_currentRecordIndex;
    CRecordSdt* m_referenceRecord;
};