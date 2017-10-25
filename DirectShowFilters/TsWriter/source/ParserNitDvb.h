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
#include "..\..\shared\SectionDecoder.h"
#include "ICallBackNitDvb.h"
#include "IDefaultAuthorityProvider.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define PID_NIT_DVB 0x10
#define TABLE_ID_NIT_DVB_ACTUAL 0x40
#define TABLE_ID_NIT_DVB_OTHER 0x41


extern void LogDebug(const wchar_t* fmt, ...);

class CParserNitDvb
  : public CSectionDecoder, public IDefaultAuthorityProvider, ICallBackNitDvb
{
  public:
    CParserNitDvb();
    virtual ~CParserNitDvb();

    void SetPid(unsigned short pid);
    void Reset(bool enableCrcCheck);
    void SetCallBack(ICallBackNitDvb* callBack);
    void OnNewSection(const CSection& section);
    bool IsSeenActual() const;
    bool IsSeenOther() const;
    bool IsReadyActual() const;
    bool IsReadyOther() const;

    bool GetService(unsigned short originalNetworkId,
                    unsigned short transportStreamId,
                    unsigned short serviceId,
                    unsigned short& freesatChannelId,
                    unsigned short& openTvChannelId,
                    unsigned long long* logicalChannelNumbers,
                    unsigned short& logicalChannelNumberCount,
                    bool& visibleInGuide,
                    unsigned short* groupIds,
                    unsigned char& groupIdCount,
                    unsigned long* availableInCells,
                    unsigned char& availableInCellCount,
                    unsigned long long* targetRegionIds,
                    unsigned char& targetRegionIdCount,
                    unsigned long* freesatRegionIds,
                    unsigned char& freesatRegionIdCount,
                    unsigned long* openTvRegionIds,
                    unsigned char& openTvRegionIdCount,
                    unsigned short* freesatChannelCategoryIds,
                    unsigned char& freesatChannelCategoryIdCount,
                    unsigned char* norDigChannelListIds,
                    unsigned char& norDigChannelListIdCount,
                    unsigned long* availableInCountries,
                    unsigned char& availableInCountryCount,
                    unsigned long* unavailableInCountries,
                    unsigned char& unavailableInCountryCount) const;

    unsigned char GetNetworkNameCount(unsigned short networkId) const;
    bool GetNetworkNameByIndex(unsigned short networkId,
                                unsigned char index,
                                unsigned long& language,
                                char* name,
                                unsigned short& nameBufferSize) const;
    bool GetNetworkNameByLanguage(unsigned short networkId,
                                  unsigned long language,
                                  char* name,
                                  unsigned short& nameBufferSize) const;

    unsigned char GetTargetRegionNameCount(unsigned long long regionId) const;
    bool GetTargetRegionNameByIndex(unsigned long long regionId,
                                    unsigned char index,
                                    unsigned long& language,
                                    char* name,
                                    unsigned short& nameBufferSize) const;
    bool GetTargetRegionNameByLanguage(unsigned long long regionId,
                                        unsigned long language,
                                        char* name,
                                        unsigned short& nameBufferSize) const;

    unsigned char GetCyfrowyPolsatChannelCategoryNameCount(unsigned char categoryId) const;
    bool GetCyfrowyPolsatChannelCategoryNameByIndex(unsigned char categoryId,
                                                    unsigned char index,
                                                    unsigned long& language,
                                                    char* name,
                                                    unsigned short& nameBufferSize) const;
    bool GetCyfrowyPolsatChannelCategoryNameByLanguage(unsigned char categoryId,
                                                        unsigned long language,
                                                        char* name,
                                                        unsigned short& nameBufferSize) const;

    unsigned char GetFreesatRegionNameCount(unsigned short regionId) const;
    bool GetFreesatRegionNameByIndex(unsigned short regionId,
                                      unsigned char index,
                                      unsigned long& language,
                                      char* name,
                                      unsigned short& nameBufferSize) const;
    bool GetFreesatRegionNameByLanguage(unsigned short regionId,
                                        unsigned long language,
                                        char* name,
                                        unsigned short& nameBufferSize) const;

    unsigned char GetFreesatChannelCategoryNameCount(unsigned short categoryId) const;
    bool GetFreesatChannelCategoryNameByIndex(unsigned short categoryId,
                                              unsigned char index,
                                              unsigned long& language,
                                              char* name,
                                              unsigned short& nameBufferSize) const;
    bool GetFreesatChannelCategoryNameByLanguage(unsigned short categoryId,
                                                  unsigned long language,
                                                  char* name,
                                                  unsigned short& nameBufferSize) const;

    unsigned char GetNorDigChannelListNameCount(unsigned char channelListId) const;
    bool GetNorDigChannelListNameByIndex(unsigned char channelListId,
                                          unsigned char index,
                                          unsigned long& language,
                                          char* name,
                                          unsigned short& nameBufferSize) const;
    bool GetNorDigChannelListNameByLanguage(unsigned char channelListId,
                                            unsigned long language,
                                            char* name,
                                            unsigned short& nameBufferSize) const;

    bool GetDefaultAuthority(unsigned short originalNetworkId,
                              unsigned short transportStreamId,
                              unsigned short serviceId,
                              char* defaultAuthority,
                              unsigned short& defaultAuthorityBufferSize) const;

    unsigned short GetTransmitterCount() const;
    bool GetTransmitter(unsigned short index,
                        unsigned char& tableId,
                        unsigned short& networkId,
                        unsigned short& originalNetworkId,
                        unsigned short& transportStreamId,
                        bool& isHomeTransmitter,
                        unsigned long& broadcastStandard,
                        unsigned long* frequencies,
                        unsigned char& frequencyCount,
                        unsigned char& polarisation,
                        unsigned char& modulation,
                        unsigned long& symbolRate,
                        unsigned short& bandwidth,
                        unsigned char& innerFecRate,
                        unsigned char& rollOffFactor,
                        short& longitude,
                        unsigned short& cellId,
                        unsigned char& cellIdExtension,
                        bool& isMultipleInputStream,
                        unsigned char& plpId) const;

  protected:
    void CleanUp();

    CCriticalSection m_section;
    wchar_t m_name[20];
    vector<unsigned char> m_tableIds;

  private:
    enum NameType
    {
      NetworkOrBouquet = 1,
      TargetRegion = 2,
      FreesatRegion = 3,
      FreesatChannelCategory = 4,
      NorDigChannelList = 5,
      CyfrowyPolsatChannelCategory = 6
    };

    class CRecordNitService : public IRecord
    {
      public:
        CRecordNitService(wchar_t* tableName)
        {
          TableName = tableName;
          TableKey = 0;
          TableId = 0;
          TableIdExtension = 0;
          OriginalNetworkId = 0;
          TransportStreamId = 0;
          ServiceId = 0;
          FreesatChannelId = 0;
          OpenTvChannelId = 0;
          VisibleInGuide = true;
          DefaultAuthority = NULL;
        }

        ~CRecordNitService()
        {
          // Do not dispose the table name!
          if (DefaultAuthority != NULL)
          {
            delete[] DefaultAuthority;
            DefaultAuthority = NULL;
          }
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordNitService* recordService = dynamic_cast<const CRecordNitService*>(record);
          if (
            recordService == NULL ||
            TableId != recordService->TableId ||
            TableIdExtension != recordService->TableIdExtension ||
            OriginalNetworkId != recordService->OriginalNetworkId ||
            TransportStreamId != recordService->TransportStreamId ||
            ServiceId != recordService->ServiceId ||
            FreesatChannelId != recordService->FreesatChannelId ||
            OpenTvChannelId != recordService->OpenTvChannelId ||
            VisibleInGuide != recordService->VisibleInGuide ||
            !CUtils::CompareMaps(LogicalChannelNumbers, recordService->LogicalChannelNumbers) ||
            !CUtils::CompareStrings(DefaultAuthority, recordService->DefaultAuthority) ||
            !CUtils::CompareVectors(AvailableInCells, recordService->AvailableInCells) ||
            !CUtils::CompareVectors(TargetRegionIds, recordService->TargetRegionIds) ||
            !CUtils::CompareVectors(FreesatRegionIds, recordService->FreesatRegionIds) ||
            !CUtils::CompareVectors(OpenTvRegionIds, recordService->OpenTvRegionIds) ||
            !CUtils::CompareVectors(FreesatChannelCategoryIds, recordService->FreesatChannelCategoryIds) ||
            !CUtils::CompareVectors(NorDigChannelListIds, recordService->NorDigChannelListIds) ||
            !CUtils::CompareVectors(AvailableInCountries, recordService->AvailableInCountries) ||
            !CUtils::CompareVectors(UnavailableInCountries, recordService->UnavailableInCountries)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          if (OpenTvChannelId > 0)
          {
            return ((unsigned long long)TableKey << 48) | OpenTvChannelId;
          }
          if (FreesatChannelId > 0)
          {
            return ((unsigned long long)TableKey << 48) | (FreesatChannelId << 16);
          }
          return ((unsigned long long)TableKey << 48) | ((unsigned long long)OriginalNetworkId << 32) | (TransportStreamId << 16) | ServiceId;
        }

        unsigned long long GetExpiryKey() const
        {
          return (TableId << 16) | TableIdExtension;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"%s: service %s, table ID = 0x%hhx, extension ID = %hu, ONID = %hu, TSID = %hu, service ID = %hu, Freesat CID = %hu, OpenTV CID = %hu, visible in guide = %d, LCN count = %llu, default authority = %S, available in cells count = %llu, target region count = %llu, Freesat region count = %llu, OpenTV region count = %llu, Freesat channel category count = %llu, NorDig channel list count = %llu, country counts = %llu / %llu",
                    TableName, situation, TableId, TableIdExtension,
                    OriginalNetworkId, TransportStreamId, ServiceId,
                    FreesatChannelId, OpenTvChannelId, VisibleInGuide,
                    (unsigned long long)LogicalChannelNumbers.size(),
                    DefaultAuthority == NULL ? "" : DefaultAuthority,
                    (unsigned long long)AvailableInCells.size(),
                    (unsigned long long)TargetRegionIds.size(),
                    (unsigned long long)FreesatRegionIds.size(),
                    (unsigned long long)OpenTvRegionIds.size(),
                    (unsigned long long)FreesatChannelCategoryIds.size(),
                    (unsigned long long)NorDigChannelListIds.size(),
                    (unsigned long long)AvailableInCountries.size(),
                    (unsigned long long)UnavailableInCountries.size());

          if (LogicalChannelNumbers.size() > 0)
          {
            LogDebug(L"  logical channel number(s)...");
            unsigned char tag;
            unsigned short regionId;
            map<unsigned long, unsigned short>::const_iterator it = LogicalChannelNumbers.begin();
            for ( ; it != LogicalChannelNumbers.end(); it++)
            {
              tag = (unsigned char)(it->first >> 16);
              regionId = it->first & 0xffff;
              LogDebug(L"    tag = 0x%hhx, region ID = %hu, LCN = %hu",
                        tag, regionId, it->second);
            }
          }

          CUtils::DebugVector(AvailableInCells, L"available in cell(s)", false);
          CUtils::DebugVector(TargetRegionIds, L"target region ID(s)", false);
          CUtils::DebugVector(FreesatRegionIds, L"Freesat region ID(s)", false);
          CUtils::DebugVector(OpenTvRegionIds, L"OpenTV region ID(s)", false);
          CUtils::DebugVector(FreesatChannelCategoryIds, L"Freesat channel category ID(s)", false);
          CUtils::DebugVector(NorDigChannelListIds, L"NorDig channel list ID(s)", false);
          CUtils::DebugVector(AvailableInCountries, L"available in countries", true);
          CUtils::DebugVector(UnavailableInCountries, L"unavailable in countries", true);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitServiceReceived(TableId,
                                              TableIdExtension,
                                              OriginalNetworkId,
                                              TransportStreamId,
                                              ServiceId,
                                              FreesatChannelId,
                                              OpenTvChannelId,
                                              VisibleInGuide,
                                              LogicalChannelNumbers,
                                              DefaultAuthority,
                                              AvailableInCells,
                                              TargetRegionIds,
                                              FreesatRegionIds,
                                              OpenTvRegionIds,
                                              FreesatChannelCategoryIds,
                                              NorDigChannelListIds,
                                              AvailableInCountries,
                                              UnavailableInCountries);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitServiceChanged(TableId,
                                              TableIdExtension,
                                              OriginalNetworkId,
                                              TransportStreamId,
                                              ServiceId,
                                              FreesatChannelId,
                                              OpenTvChannelId,
                                              VisibleInGuide,
                                              LogicalChannelNumbers,
                                              DefaultAuthority,
                                              AvailableInCells,
                                              TargetRegionIds,
                                              FreesatRegionIds,
                                              OpenTvRegionIds,
                                              FreesatChannelCategoryIds,
                                              NorDigChannelListIds,
                                              AvailableInCountries,
                                              UnavailableInCountries);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitServiceRemoved(TableId,
                                              TableIdExtension,
                                              OriginalNetworkId,
                                              TransportStreamId,
                                              ServiceId,
                                              FreesatChannelId,
                                              OpenTvChannelId,
                                              VisibleInGuide,
                                              LogicalChannelNumbers,
                                              DefaultAuthority,
                                              AvailableInCells,
                                              TargetRegionIds,
                                              FreesatRegionIds,
                                              OpenTvRegionIds,
                                              FreesatChannelCategoryIds,
                                              NorDigChannelListIds,
                                              AvailableInCountries,
                                              UnavailableInCountries);
          }
        }

        const wchar_t* TableName;
        unsigned short TableKey;                      // reference to a combination of table ID, table ID extension and section number [because transport stream records may be split between sections]; needed to avoid a >64 bit record key
        unsigned char TableId;
        unsigned short TableIdExtension;              // network or bouquet ID

        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;
        unsigned short ServiceId;
        unsigned short FreesatChannelId;
        unsigned short OpenTvChannelId;
        bool VisibleInGuide;
        map<unsigned long, unsigned short> LogicalChannelNumbers;  // is HD bit [1 bit] | region ID [16 bits] => LCN
        char* DefaultAuthority;
        vector<unsigned long> AvailableInCells;       // cell ID [16 bits] | cell ID extension [8 bits]
        vector<unsigned long long> TargetRegionIds;
        vector<unsigned short> FreesatRegionIds;
        vector<unsigned short> OpenTvRegionIds;
        vector<unsigned short> FreesatChannelCategoryIds;
        vector<unsigned char> NorDigChannelListIds;
        vector<unsigned long> AvailableInCountries;
        vector<unsigned long> UnavailableInCountries;
    };

    class CRecordNitTransmitter : public IRecord
    {
      public:
        CRecordNitTransmitter()
        {
          TableKey = 0;
          TableId = 0;
          NetworkId = 0;
          OriginalNetworkId = 0;
          TransportStreamId = 0;
          IsHomeTransmitter = false;
        }

        virtual ~CRecordNitTransmitter()
        {
        }

        virtual bool Equals(const IRecord* record) const
        {
          const CRecordNitTransmitter* recordTransmitter = dynamic_cast<const CRecordNitTransmitter*>(record);
          if (
            recordTransmitter == NULL ||
            recordTransmitter->TableId != TableId ||
            recordTransmitter->NetworkId != NetworkId ||
            recordTransmitter->OriginalNetworkId != OriginalNetworkId ||
            recordTransmitter->TransportStreamId != TransportStreamId ||
            !CUtils::CompareVectors(recordTransmitter->Frequencies, Frequencies) ||
            recordTransmitter->IsHomeTransmitter != IsHomeTransmitter
          )
          {
            return false;
          }
          return true;
        }

        virtual unsigned long long GetKey() const
        {
          return ((unsigned long long)TableKey << 32) | ((unsigned long long)OriginalNetworkId << 16) | TransportStreamId;
        }

        virtual unsigned long long GetExpiryKey() const
        {
          return (TableId << 16) | NetworkId;
        }

        virtual CRecordNitTransmitter* Clone() const
        {
          return NULL;
        }

        virtual bool IsValid() const
        {
          return false;
        }

        unsigned short TableKey;      // reference to a combination of table ID and network ID; needed to avoid a >64 bit record key
        unsigned char TableId;
        unsigned short NetworkId;
        unsigned short OriginalNetworkId;
        unsigned short TransportStreamId;
        vector<unsigned long> Frequencies;  // unit = kHz
        bool IsHomeTransmitter;
    };

    class CRecordNitTransmitterCable : public CRecordNitTransmitter
    {
      public:
        CRecordNitTransmitterCable()
        {
          OuterFecMethod = 0;
          Modulation = 0;
          SymbolRate = 0;
          InnerFecRate = 0;
          IsC2 = false;
          PlpId = 0;
          DataSliceId = 0;
          FrequencyType = 0;
          ActiveOfdmSymbolDuration = 0;
          GuardInterval = 0;
        }

        ~CRecordNitTransmitterCable()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNitTransmitter::Equals(record))
          {
            return false;
          }

          const CRecordNitTransmitterCable* recordCable = dynamic_cast<const CRecordNitTransmitterCable*>(record);
          if (
            recordCable != NULL &&
            recordCable->OuterFecMethod == OuterFecMethod &&
            recordCable->Modulation == Modulation &&
            recordCable->SymbolRate == SymbolRate &&
            recordCable->InnerFecRate == InnerFecRate &&
            recordCable->IsC2 == IsC2 &&
            recordCable->PlpId == PlpId &&
            recordCable->DataSliceId == DataSliceId &&
            recordCable->FrequencyType == FrequencyType &&
            recordCable->ActiveOfdmSymbolDuration == ActiveOfdmSymbolDuration &&
            recordCable->GuardInterval == GuardInterval
          )
          {
            return true;
          }
          return false;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NIT: cable transmitter %s, table ID = 0x%hhx, NID = %hu, ONID = %hu, TSID = %hu, is home transmitter = %d, frequency count = %llu, modulation = %hhu, symbol rate = %lu ks/s, inner FEC rate = %hhu, outer FEC method = %hhu, is DVB-C2 = %d, data slice ID = %hhu, PLP ID = %hhu, frequency type = %hhu, active OFDM symbol duration = %hhu, guard interval = %hhu",
                    situation, TableId, NetworkId, OriginalNetworkId,
                    TransportStreamId, IsHomeTransmitter, Frequencies.size(),
                    Modulation, SymbolRate, InnerFecRate, OuterFecMethod, IsC2,
                    DataSliceId, PlpId, FrequencyType,
                    ActiveOfdmSymbolDuration, GuardInterval);

          CUtils::DebugVector(Frequencies, L"frequencies", false);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterCableReceived(TableId,
                                                        NetworkId,
                                                        OriginalNetworkId,
                                                        TransportStreamId,
                                                        IsHomeTransmitter,
                                                        Frequencies,
                                                        OuterFecMethod,
                                                        Modulation,
                                                        SymbolRate,
                                                        InnerFecRate,
                                                        IsC2,
                                                        PlpId,
                                                        DataSliceId,
                                                        FrequencyType,
                                                        ActiveOfdmSymbolDuration,
                                                        GuardInterval);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterCableChanged(TableId,
                                                        NetworkId,
                                                        OriginalNetworkId,
                                                        TransportStreamId,
                                                        IsHomeTransmitter,
                                                        Frequencies,
                                                        OuterFecMethod,
                                                        Modulation,
                                                        SymbolRate,
                                                        InnerFecRate,
                                                        IsC2,
                                                        PlpId,
                                                        DataSliceId,
                                                        FrequencyType,
                                                        ActiveOfdmSymbolDuration,
                                                        GuardInterval);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterCableRemoved(TableId,
                                                        NetworkId,
                                                        OriginalNetworkId,
                                                        TransportStreamId,
                                                        IsHomeTransmitter,
                                                        Frequencies,
                                                        OuterFecMethod,
                                                        Modulation,
                                                        SymbolRate,
                                                        InnerFecRate,
                                                        IsC2,
                                                        PlpId,
                                                        DataSliceId,
                                                        FrequencyType,
                                                        ActiveOfdmSymbolDuration,
                                                        GuardInterval);
          }
        }

        CRecordNitTransmitter* Clone() const
        {
          CRecordNitTransmitterCable* record = new CRecordNitTransmitterCable();
          if (record != NULL)
          {
            record->TableKey = TableKey;
            record->TableId = TableId;
            record->NetworkId = NetworkId;
            record->OriginalNetworkId = OriginalNetworkId;
            record->TransportStreamId = TransportStreamId;
            record->IsHomeTransmitter = IsHomeTransmitter;
            record->Frequencies = Frequencies;
            record->OuterFecMethod = OuterFecMethod;
            record->Modulation = Modulation;
            record->SymbolRate = SymbolRate;
            record->InnerFecRate = InnerFecRate;
            record->IsC2 = IsC2;
            record->PlpId = PlpId;
            record->DataSliceId = DataSliceId;
            record->FrequencyType = FrequencyType;
            record->ActiveOfdmSymbolDuration = ActiveOfdmSymbolDuration;
            record->GuardInterval = GuardInterval;
          }
          return record;
        }

        unsigned char OuterFecMethod;
        unsigned char Modulation;
        unsigned long SymbolRate;   // unit = ks/s
        unsigned char InnerFecRate;
        bool IsC2;
        unsigned char PlpId;
        unsigned char DataSliceId;
        unsigned char FrequencyType;
        unsigned char ActiveOfdmSymbolDuration;
        unsigned char GuardInterval;
    };

    class CRecordNitTransmitterSatellite : public CRecordNitTransmitter
    {
      public:
        CRecordNitTransmitterSatellite()
        {
          OrbitalPosition = 0;
          WestEastFlag = false;
          Polarisation = 0;
          Modulation = 0;
          SymbolRate = 0;
          InnerFecRate = 0;
          RollOff = 0;
          IsS2 = false;
          MultipleInputStreamFlag = false;
          BackwardsCompatibilityIndicator = false;
          ScramblingSequenceIndex = 0;
          InputStreamIdentifier = 0;
        }

        ~CRecordNitTransmitterSatellite()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNitTransmitter::Equals(record))
          {
            return false;
          }

          const CRecordNitTransmitterSatellite* recordSatellite = dynamic_cast<const CRecordNitTransmitterSatellite*>(record);
          if (
            recordSatellite != NULL &&
            recordSatellite->OrbitalPosition == OrbitalPosition &&
            recordSatellite->WestEastFlag == WestEastFlag &&
            recordSatellite->Polarisation == Polarisation &&
            recordSatellite->Modulation == Modulation &&
            recordSatellite->SymbolRate == SymbolRate &&
            recordSatellite->InnerFecRate == InnerFecRate &&
            recordSatellite->RollOff == RollOff &&
            recordSatellite->IsS2 == IsS2 &&
            recordSatellite->MultipleInputStreamFlag == MultipleInputStreamFlag &&
            recordSatellite->BackwardsCompatibilityIndicator == BackwardsCompatibilityIndicator &&
            recordSatellite->ScramblingSequenceIndex == ScramblingSequenceIndex &&
            recordSatellite->InputStreamIdentifier == InputStreamIdentifier
          )
          {
            return true;
          }
          return false;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NIT: satellite transmitter %s, table ID = 0x%hhx, NID = %hu, ONID = %hu, TSID = %hu, is home transmitter = %d, frequency count = %llu, polarisation = %hhu, modulation = %hhu, symbol rate = %lu ks/s, inner FEC rate = %hhu, orbital position = %hu, is east = %d, is DVB-S2 = %d, roll-off factor = %hhu, multiple input stream flag = %d, backwards compatibility indicator = %d, scrambling sequence index = %lu, input stream identifier = %hhu",
                    situation, TableId, NetworkId, OriginalNetworkId,
                    TransportStreamId, IsHomeTransmitter,
                    (unsigned long long)Frequencies.size(), Polarisation,
                    Modulation, SymbolRate, InnerFecRate, OrbitalPosition,
                    WestEastFlag, IsS2, RollOff, MultipleInputStreamFlag,
                    BackwardsCompatibilityIndicator, ScramblingSequenceIndex,
                    InputStreamIdentifier);

          CUtils::DebugVector(Frequencies, L"frequencies", false);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterSatelliteReceived(TableId,
                                                            NetworkId,
                                                            OriginalNetworkId,
                                                            TransportStreamId,
                                                            IsHomeTransmitter,
                                                            Frequencies,
                                                            OrbitalPosition,
                                                            WestEastFlag,
                                                            Polarisation,
                                                            Modulation,
                                                            SymbolRate,
                                                            InnerFecRate,
                                                            RollOff,
                                                            IsS2,
                                                            MultipleInputStreamFlag,
                                                            BackwardsCompatibilityIndicator,
                                                            ScramblingSequenceIndex,
                                                            InputStreamIdentifier);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterSatelliteChanged(TableId,
                                                            NetworkId,
                                                            OriginalNetworkId,
                                                            TransportStreamId,
                                                            IsHomeTransmitter,
                                                            Frequencies,
                                                            OrbitalPosition,
                                                            WestEastFlag,
                                                            Polarisation,
                                                            Modulation,
                                                            SymbolRate,
                                                            InnerFecRate,
                                                            RollOff,
                                                            IsS2,
                                                            MultipleInputStreamFlag,
                                                            BackwardsCompatibilityIndicator,
                                                            ScramblingSequenceIndex,
                                                            InputStreamIdentifier);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterSatelliteRemoved(TableId,
                                                            NetworkId,
                                                            OriginalNetworkId,
                                                            TransportStreamId,
                                                            IsHomeTransmitter,
                                                            Frequencies,
                                                            OrbitalPosition,
                                                            WestEastFlag,
                                                            Polarisation,
                                                            Modulation,
                                                            SymbolRate,
                                                            InnerFecRate,
                                                            RollOff,
                                                            IsS2,
                                                            MultipleInputStreamFlag,
                                                            BackwardsCompatibilityIndicator,
                                                            ScramblingSequenceIndex,
                                                            InputStreamIdentifier);
          }
        }

        CRecordNitTransmitter* Clone() const
        {
          CRecordNitTransmitterSatellite* record = new CRecordNitTransmitterSatellite();
          if (record != NULL)
          {
            record->TableKey = TableKey;
            record->TableId = TableId;
            record->NetworkId = NetworkId;
            record->OriginalNetworkId = OriginalNetworkId;
            record->TransportStreamId = TransportStreamId;
            record->IsHomeTransmitter = IsHomeTransmitter;
            record->Frequencies = Frequencies;
            record->OrbitalPosition = OrbitalPosition;
            record->WestEastFlag = WestEastFlag;
            record->Polarisation = Polarisation;
            record->Modulation = Modulation;
            record->SymbolRate = SymbolRate;
            record->InnerFecRate = InnerFecRate;
            record->RollOff = RollOff;
            record->IsS2 = IsS2;
            record->MultipleInputStreamFlag = MultipleInputStreamFlag;
            record->BackwardsCompatibilityIndicator = BackwardsCompatibilityIndicator;
            record->ScramblingSequenceIndex = ScramblingSequenceIndex;
            record->InputStreamIdentifier = InputStreamIdentifier;
          }
          return record;
        }

        unsigned short OrbitalPosition; // unit = tenths of a degree
        bool WestEastFlag;
        unsigned char Polarisation;
        unsigned char Modulation;
        unsigned long SymbolRate;       // unit = ks/s
        unsigned char InnerFecRate;
        unsigned char RollOff;
        bool IsS2;
        bool MultipleInputStreamFlag;
        bool BackwardsCompatibilityIndicator;
        unsigned long ScramblingSequenceIndex;
        unsigned char InputStreamIdentifier;
    };

    class CRecordNitTransmitterTerrestrial : public CRecordNitTransmitter
    {
      public:
        CRecordNitTransmitterTerrestrial()
        {
          Bandwidth = 0;
          IsHighPriority = false;
          TimeSlicingIndicator = false;
          MpeFecIndicator = false;
          Constellation = 0;
          IndepthInterleaverUsed = false;
          HierarchyAlpha = 0;
          CodeRateHpStream = 0;
          CodeRateLpStream = 0;
          GuardInterval = 0;
          TransmissionMode = 0;
          OtherFrequencyFlag = false;
          IsT2 = false;
          PlpId = 0;
          T2SystemId = 0;
          MultipleInputStreamFlag = false;
          TimeFrequencySlicingFlag = false;
          CellId = 0;
          CellIdExtension = 0;
        }

        ~CRecordNitTransmitterTerrestrial()
        {
        }

        bool Equals(const IRecord* record) const
        {
          if (!CRecordNitTransmitter::Equals(record))
          {
            return false;
          }

          const CRecordNitTransmitterTerrestrial* recordTerrestrial = dynamic_cast<const CRecordNitTransmitterTerrestrial*>(record);
          if (
            recordTerrestrial != NULL &&
            recordTerrestrial->Bandwidth == Bandwidth &&
            recordTerrestrial->IsHighPriority == IsHighPriority &&
            recordTerrestrial->TimeSlicingIndicator == TimeSlicingIndicator &&
            recordTerrestrial->MpeFecIndicator == MpeFecIndicator &&
            recordTerrestrial->Constellation == Constellation &&
            recordTerrestrial->IndepthInterleaverUsed == IndepthInterleaverUsed &&
            recordTerrestrial->HierarchyAlpha == HierarchyAlpha &&
            recordTerrestrial->CodeRateHpStream == CodeRateHpStream &&
            recordTerrestrial->CodeRateLpStream == CodeRateLpStream &&
            recordTerrestrial->GuardInterval == GuardInterval &&
            recordTerrestrial->TransmissionMode == TransmissionMode &&
            recordTerrestrial->OtherFrequencyFlag == OtherFrequencyFlag &&
            recordTerrestrial->IsT2 == IsT2 &&
            recordTerrestrial->PlpId == PlpId &&
            recordTerrestrial->T2SystemId == T2SystemId &&
            recordTerrestrial->MultipleInputStreamFlag == MultipleInputStreamFlag &&
            recordTerrestrial->TimeFrequencySlicingFlag == TimeFrequencySlicingFlag &&
            recordTerrestrial->CellId == CellId &&
            recordTerrestrial->CellIdExtension == CellIdExtension
          )
          {
            return true;
          }
          return false;
        }

        unsigned long long GetKey() const
        {
          return (CRecordNitTransmitter::GetKey() << 24) | (CellId << 8) | CellIdExtension;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"NIT: terrestrial transmitter %s, table ID = 0x%hhx, NID = %hu, ONID = %hu, TSID = %hu, is home transmitter = %d, frequency count = %llu, bandwidth = %hu kHz, is high priority = %d, time slicing indicator = %d, MPE FEC indicator = %d, constellation = %hhu, indepth interleaver used = %d, hierarchy alpha = %hhu, code rate HP stream = %hhu, code rate LP stream = %hhu, guard interval = %hhu, transmission mode = %hhu, other frequency flag = %d, cell ID = %hu, cell ID extension = %hhu, is DVB-T2 = %d, T2 system ID = %hu, multiple input stream flag = %d, time-frequency slicing flag = %d, PLP ID = %hhu",
                    situation, TableId, NetworkId, OriginalNetworkId,
                    TransportStreamId, IsHomeTransmitter,
                    (unsigned long long)Frequencies.size(), Bandwidth,
                    IsHighPriority, TimeSlicingIndicator, MpeFecIndicator,
                    Constellation, IndepthInterleaverUsed, HierarchyAlpha,
                    CodeRateHpStream, CodeRateLpStream, GuardInterval,
                    TransmissionMode, OtherFrequencyFlag, CellId,
                    CellIdExtension, IsT2, T2SystemId, MultipleInputStreamFlag,
                    TimeFrequencySlicingFlag, PlpId);

          CUtils::DebugVector(Frequencies, L"frequencies", false);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterTerrestrialReceived(TableId,
                                                              NetworkId,
                                                              OriginalNetworkId,
                                                              TransportStreamId,
                                                              IsHomeTransmitter,
                                                              Frequencies,
                                                              Bandwidth,
                                                              IsHighPriority,
                                                              TimeSlicingIndicator,
                                                              MpeFecIndicator,
                                                              Constellation,
                                                              IndepthInterleaverUsed,
                                                              HierarchyAlpha,
                                                              CodeRateHpStream,
                                                              CodeRateLpStream,
                                                              GuardInterval,
                                                              TransmissionMode,
                                                              OtherFrequencyFlag,
                                                              IsT2,
                                                              PlpId,
                                                              T2SystemId,
                                                              MultipleInputStreamFlag,
                                                              TimeFrequencySlicingFlag,
                                                              CellId,
                                                              CellIdExtension);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterTerrestrialChanged(TableId,
                                                              NetworkId,
                                                              OriginalNetworkId,
                                                              TransportStreamId,
                                                              IsHomeTransmitter,
                                                              Frequencies,
                                                              Bandwidth,
                                                              IsHighPriority,
                                                              TimeSlicingIndicator,
                                                              MpeFecIndicator,
                                                              Constellation,
                                                              IndepthInterleaverUsed,
                                                              HierarchyAlpha,
                                                              CodeRateHpStream,
                                                              CodeRateLpStream,
                                                              GuardInterval,
                                                              TransmissionMode,
                                                              OtherFrequencyFlag,
                                                              IsT2,
                                                              PlpId,
                                                              T2SystemId,
                                                              MultipleInputStreamFlag,
                                                              TimeFrequencySlicingFlag,
                                                              CellId,
                                                              CellIdExtension);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackNitDvb* callBackNit = static_cast<ICallBackNitDvb*>(callBack);
          if (callBackNit != NULL)
          {
            callBackNit->OnNitTransmitterTerrestrialRemoved(TableId,
                                                              NetworkId,
                                                              OriginalNetworkId,
                                                              TransportStreamId,
                                                              IsHomeTransmitter,
                                                              Frequencies,
                                                              Bandwidth,
                                                              IsHighPriority,
                                                              TimeSlicingIndicator,
                                                              MpeFecIndicator,
                                                              Constellation,
                                                              IndepthInterleaverUsed,
                                                              HierarchyAlpha,
                                                              CodeRateHpStream,
                                                              CodeRateLpStream,
                                                              GuardInterval,
                                                              TransmissionMode,
                                                              OtherFrequencyFlag,
                                                              IsT2,
                                                              PlpId,
                                                              T2SystemId,
                                                              MultipleInputStreamFlag,
                                                              TimeFrequencySlicingFlag,
                                                              CellId,
                                                              CellIdExtension);
          }
        }

        CRecordNitTransmitter* Clone() const
        {
          CRecordNitTransmitterTerrestrial* record = new CRecordNitTransmitterTerrestrial();
          if (record != NULL)
          {
            record->TableKey = TableKey;
            record->TableId = TableId;
            record->NetworkId = NetworkId;
            record->OriginalNetworkId = OriginalNetworkId;
            record->TransportStreamId = TransportStreamId;
            record->IsHomeTransmitter = IsHomeTransmitter;
            record->Frequencies = Frequencies;
            record->Bandwidth = Bandwidth;
            record->IsHighPriority = IsHighPriority;
            record->TimeSlicingIndicator = TimeSlicingIndicator;
            record->MpeFecIndicator = MpeFecIndicator;
            record->Constellation = Constellation;
            record->IndepthInterleaverUsed = IndepthInterleaverUsed;
            record->HierarchyAlpha = HierarchyAlpha;
            record->CodeRateHpStream = CodeRateHpStream;
            record->CodeRateLpStream = CodeRateLpStream;
            record->GuardInterval = GuardInterval;
            record->TransmissionMode = TransmissionMode;
            record->OtherFrequencyFlag = OtherFrequencyFlag;
            record->IsT2 = IsT2;
            record->PlpId = PlpId;
            record->T2SystemId = T2SystemId;
            record->MultipleInputStreamFlag = MultipleInputStreamFlag;
            record->TimeFrequencySlicingFlag = TimeFrequencySlicingFlag;
            record->CellId = CellId;
            record->CellIdExtension = CellIdExtension;
          }
          return record;
        }

        unsigned short Bandwidth;       // unit = kHz
        bool IsHighPriority;
        bool TimeSlicingIndicator;
        bool MpeFecIndicator;
        unsigned char Constellation;
        bool IndepthInterleaverUsed;
        unsigned char HierarchyAlpha;
        unsigned char CodeRateHpStream;
        unsigned char CodeRateLpStream;
        unsigned char GuardInterval;
        unsigned char TransmissionMode;
        bool OtherFrequencyFlag;
        bool IsT2;
        unsigned char PlpId;
        unsigned short T2SystemId;
        bool MultipleInputStreamFlag;
        bool TimeFrequencySlicingFlag;
        unsigned short CellId;
        unsigned char CellIdExtension;
    };

    template<class T> static void CleanUpNames(map<T, map<unsigned long, char*>*>& names);
    template<class T1, class T2> static void CleanUpMapOfVectors(map<T1, vector<T2>*>& mapOfVectors);
    static void CleanUpMapOfMaps(map<unsigned short, map<unsigned long, unsigned short>*>& mapOfMaps);

    static void ExpandRegionIds(const vector<unsigned short>& regionIds,
                                unsigned short tableIdExtension,
                                vector<unsigned long>& expandedRegionIds);
    template<class T> static void AggregateSet(const vector<T>& values, map<T, bool>& set);
    template<class T1, class T2> static bool GetSetValues(const map<T1, bool>& set,
                                                          T1* keys,
                                                          T2& keyCount);

    void AddGroupNames(unsigned char nameTypeId,
                        unsigned long long groupId,
                        map<unsigned long, char*>& names);
    template<class T> void AddGroupNameSets(unsigned char nameTypeId,
                                            map<T, map<unsigned long, char*>*>& names);
    unsigned char GetNameCount(NameType nameType, unsigned long long nameId) const;
    bool GetNameByIndex(NameType nameType,
                        unsigned long long nameId,
                        unsigned char index,
                        unsigned long& language,
                        char* name,
                        unsigned short& nameBufferSize) const;
    bool GetNameByLanguage(NameType nameType,
                            unsigned long long nameId,
                            unsigned long language,
                            char* name,
                            unsigned short& nameBufferSize) const;

    void AddServices(unsigned char tableId,
                      unsigned short groupId,
                      unsigned char sectionNumber,
                      unsigned short originalNetworkId,
                      unsigned short transportStreamId,
                      vector<unsigned short>& serviceIds,
                      map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                      map<unsigned short, bool>& visibleInGuideFlags,
                      char* defaultAuthority,
                      map<unsigned short, unsigned short>& freesatChannelIds,
                      map<unsigned short, vector<unsigned short>*>& freesatRegionIds,
                      map<unsigned short, vector<unsigned short>*>& freesatChannelCategoryIds,
                      map<unsigned short, unsigned short>& openTvChannelIds,
                      map<unsigned short, vector<unsigned short>*>& openTvRegionIds,
                      map<unsigned short, vector<unsigned char>*>& norDigChannelListIds,
                      map<unsigned long, unsigned long>& cellFrequencies,
                      vector<unsigned long long>& targetRegionIds,
                      vector<unsigned long>& availableInCountries,
                      vector<unsigned long>& unavailableInCountries);

    void AddTransmitters(unsigned char tableId,
                          unsigned short groupId,
                          unsigned short originalNetworkId,
                          unsigned short transportStreamId,
                          CRecordNitTransmitterCable& recordCable,
                          CRecordNitTransmitterSatellite& recordSatellite,
                          CRecordNitTransmitterTerrestrial& recordTerrestrial,
                          map<unsigned long, unsigned long>& cellFrequencies,
                          vector<unsigned long>& frequencies);
    void AddTransmitter(CRecordNitTransmitter* record);

    void AddLogicalChannelNumber(unsigned short serviceId,
                                  unsigned char descriptorTag,
                                  unsigned short regionId,
                                  unsigned short logicalChannelNumber,
                                  const wchar_t* lcnType,
                                  map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const;

    bool DecodeExtensionDescriptors(const unsigned char* sectionData,
                                    unsigned short& pointer,
                                    unsigned short endOfExtensionDescriptors,
                                    map<unsigned long, char*>& names,
                                    vector<unsigned long>& availableInCountries,
                                    vector<unsigned long>& unavailableInCountries,
                                    vector<unsigned long>& homeTransmitterKeys,
                                    unsigned long& privateDataSpecifier,
                                    char** defaultAuthority,
                                    vector<unsigned long long>& targetRegionIds,
                                    map<unsigned long long, map<unsigned long, char*>*>& targetRegionNames,
                                    map<unsigned char, char*>& cyfrowyPolsatChannelCategoryNames,
                                    map<unsigned short, map<unsigned long, char*>*>& freesatRegionNames,
                                    map<unsigned short, vector<unsigned short>*>& freesatChannelCategoryIds,
                                    map<unsigned short, map<unsigned long, char*>*>& freesatChannelCategoryNames) const;
    bool DecodeTransportStreamDescriptors(const unsigned char* sectionData,
                                          unsigned short& pointer,
                                          unsigned short endOfTransportDescriptors,
                                          unsigned long groupPrivateDataSpecifier,
                                          const vector<unsigned short>& bouquetFreesatRegionIds,
                                          vector<unsigned short>& serviceIds,
                                          map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                                          map<unsigned short, bool>& visibleInGuideFlags,
                                          map<unsigned short, vector<unsigned char>*>& norDigChannelListIds,
                                          map<unsigned char, char*>& norDigChannelListNames,
                                          map<unsigned short, unsigned short>& openTvChannelIds,
                                          map<unsigned short, vector<unsigned short>*>& openTvRegionIds,
                                          map<unsigned short, unsigned short>& freesatChannelIds,
                                          map<unsigned short, vector<unsigned short>*>& freesatRegionIds,
                                          vector<unsigned long long>& targetRegionIds,
                                          char** defaultAuthority,
                                          vector<unsigned long>& frequencies,
                                          map<unsigned long, unsigned long>& cellFrequencies,
                                          CRecordNitTransmitterCable& recordCable,
                                          CRecordNitTransmitterSatellite& recordSatellite,
                                          CRecordNitTransmitterTerrestrial& recordTerrestrial) const;

    bool DecodeNameDescriptor(const unsigned char* data,
                              unsigned char dataLength,
                              char** name) const;
    bool DecodeCountryAvailabilityDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              vector<unsigned long>& availableInCountries,
                                              vector<unsigned long>& unavailableInCountries) const;
    bool DecodeLinkageDescriptor(const unsigned char* data,
                                  unsigned char dataLength,
                                  vector<unsigned long>& homeTransmitterKeys) const;
    bool DecodeMultilingualNameDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          map<unsigned long, char*>& names) const;
    bool DecodePrivateDataSpecifierDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned long& privateDataSpecifier) const;
    bool DecodeDefaultAuthorityDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          char** defaultAuthority) const;
    bool DecodeFreesatRegionNameListDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                map<unsigned short, map<unsigned long, char*>*>& names) const;
    bool DecodeFreesatChannelCategoryMappingDescriptor(const unsigned char* data,
                                                        unsigned char dataLength,
                                                        map<unsigned short, vector<unsigned short>*>& channels) const;
    bool DecodeFreesatChannelCategoryNameListDescriptor(const unsigned char* data,
                                                        unsigned char dataLength,
                                                        map<unsigned short, map<unsigned long, char*>*>& names) const;
    bool DecodeCyfrowyPolsatChannelCategoryNameListDescriptor(const unsigned char* data,
                                                              unsigned char dataLength,
                                                              map<unsigned char, char*>& names) const;
    bool DecodeTargetRegionDescriptor(const unsigned char* data,
                                      unsigned char dataLength,
                                      vector<unsigned long long>& targetRegionIds) const;
    bool DecodeTargetRegionNameDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          map<unsigned long long, char*>& names,
                                          unsigned long& language) const;

    bool DecodeServiceListDescriptor(const unsigned char* data,
                                      unsigned char dataLength,
                                      vector<unsigned short>& serviceIds) const;
    bool DecodeSatelliteDeliverySystemDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  CRecordNitTransmitterSatellite& record) const;
    bool DecodeCableDeliverySystemDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              CRecordNitTransmitterCable& record) const;
    bool DecodeTerrestrialDeliverySystemDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    CRecordNitTransmitterTerrestrial& record) const;
    bool DecodeFrequencyListDescriptor(const unsigned char* data,
                                        unsigned char dataLength,
                                        vector<unsigned long>& frequencies) const;
    bool DecodeCellFrequencyLinkDescriptor(const unsigned char* data,
                                            unsigned char dataLength,
                                            map<unsigned long, unsigned long>& cellFrequencies) const;
    bool DecodeS2SatelliteDeliverySystemDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    CRecordNitTransmitterSatellite& record) const;
    bool DecodeAlternativeLogicalChannelNumberDescriptor(const unsigned char* data,
                                                          unsigned char dataLength,
                                                          unsigned char tag,
                                                          map<unsigned short, bool>& visibleInGuideFlags,
                                                          map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const;
    bool DecodeLogicalChannelNumberDescriptor(const unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned char tag,
                                              unsigned long privateDataSpecifier,
                                              map<unsigned short, bool>& visibleInGuideFlags,
                                              map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const;
    bool DecodeServiceAttributeDescriptor(const unsigned char* data,
                                          unsigned char dataLength,
                                          map<unsigned short, bool>& visibleInGuideFlags) const;
    bool DecodeNorDigLogicalChannelDescriptorVersion2(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      map<unsigned char, char*>& channelListNames,
                                                      map<unsigned short, vector<unsigned char>*>& channelListIds,
                                                      map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                                                      map<unsigned short, bool>& visibleInGuideFlags) const;
    bool DecodeOpenTvChannelDescriptor(const unsigned char* data,
                                        unsigned char dataLength,
                                        map<unsigned short, vector<unsigned short>*>& regionIds,
                                        map<unsigned short, unsigned short>& channelIds,
                                        map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers) const;
    bool DecodeFreesatChannelDescriptor(const unsigned char* data,
                                        unsigned char dataLength,
                                        const vector<unsigned short> bouquetRegionIds,
                                        map<unsigned short, bool>& visibleInGuideFlags,
                                        map<unsigned short, unsigned short>& channelIds,
                                        map<unsigned short, map<unsigned long, unsigned short>*>& logicalChannelNumbers,
                                        map<unsigned short, vector<unsigned short>*>& regionIds) const;
    bool DecodeT2TerrestrialDeliverySystemDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      CRecordNitTransmitterTerrestrial& record,
                                                      map<unsigned long, unsigned long>& cellFrequencies) const;
    bool DecodeC2CableDeliverySystemDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                CRecordNitTransmitterCable& record) const;

    static unsigned long DecodeCableFrequency(const unsigned char* data);
    static unsigned long DecodeSatelliteFrequency(const unsigned char* data);
    static unsigned long DecodeTerrestrialFrequency(const unsigned char* data);

    static unsigned long GetLinkageKey(unsigned short originalNetworkId,
                                        unsigned short transportStreamId);

    void OnTableComplete(unsigned char tableId);
    void OnNitServiceRemoved(unsigned char tableId,
                              unsigned short tableIdExtension,
                              unsigned short originalNetworkId,
                              unsigned short transportStreamId,
                              unsigned short serviceId,
                              unsigned short freesatChannelId,
                              unsigned short openTvChannelId,
                              bool visibleInGuide,
                              const map<unsigned long, unsigned short>& logicalChannelNumbers,
                              const char* defaultAuthority,
                              const vector<unsigned long>& availableInCells,
                              const vector<unsigned long long>& targetRegionIds,
                              const vector<unsigned short>& freesatRegionIds,
                              const vector<unsigned short>& openTvRegionIds,
                              const vector<unsigned short>& freesatChannelCategoryIds,
                              const vector<unsigned char>& norDigChannelListIds,
                              const vector<unsigned long>& availableInCountries,
                              const vector<unsigned long>& unavailableInCountries);

    vector<unsigned long long> m_seenSectionsActual;
    vector<unsigned long long> m_unseenSectionsActual;
    vector<unsigned long long> m_seenSectionsOther;
    vector<unsigned long long> m_unseenSectionsOther;
    bool m_isOtherReady;
    clock_t m_otherCompleteTime;
    ICallBackNitDvb* m_callBack;
    bool m_enableCrcCheck;
    bool m_useCompatibilityMode;
    unsigned short m_networkId;

    map<unsigned long long, map<unsigned long, char*>*> m_groupNames;   // name type ID [8 bits] | name ID [56 bits] -> [language -> name]
    map<unsigned long, unsigned short> m_tableKeys;                     // table ID [8 bits] | table ID extension [16 bits] -> table key
    unsigned short m_nextTableKey;
    CRecordStore m_recordsService;
    CRecordStore m_recordsTransmitter;
    map<unsigned long long, vector<CRecordNitService*>*> m_cacheServiceRecordsByDvbId;
};