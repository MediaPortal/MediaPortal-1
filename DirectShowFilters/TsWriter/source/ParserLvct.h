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
#include "..\..\shared\Section.h"
#include "ICallBackLvct.h"
#include "IRecord.h"
#include "RecordStore.h"
#include "Utils.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_LVCT_CABLE 0xc9
#define TABLE_ID_LVCT_TERRESTRIAL 0xc8


extern void LogDebug(const wchar_t* fmt, ...);

class CParserLvct
{
  public:
    CParserLvct(unsigned short pid);
    virtual ~CParserLvct();

    void Reset();
    void SetCallBack(ICallBackLvct* callBack);
    void OnNewSection(CSection& section, bool isOutOfBandSection);
    bool IsSeen() const;
    bool IsReady() const;

    unsigned short GetChannelCount() const;
    bool GetChannel(unsigned short index,
                    unsigned char& tableId,
                    unsigned short& sectionTransportStreamId,
                    unsigned short& mapId,
                    char* shortName,
                    unsigned short& shortNameBufferSize,
                    unsigned char& longNameCount,
                    unsigned short& majorChannelNumber,
                    unsigned short& minorChannelNumber,
                    unsigned char& modulationMode,
                    unsigned long& carrierFrequency,
                    unsigned short& transportStreamId,
                    unsigned short& programNumber,
                    unsigned char& etmLocation,
                    bool& accessControlled,
                    bool& hidden,
                    unsigned char& pathSelect,
                    bool& outOfBand,
                    bool& hideGuide,
                    unsigned char& serviceType,
                    unsigned short& sourceId,
                    unsigned char& streamCountVideo,
                    unsigned char& streamCountAudio,
                    bool& isThreeDimensional,
                    unsigned long* audioLanguages,
                    unsigned char& audioLanguageCount,
                    unsigned long* captionsLanguages,
                    unsigned char& captionsLanguageCount);
    bool GetChannelLongNameByIndex(unsigned short channelIndex,
                                    unsigned char nameIndex,
                                    unsigned long& language,
                                    char* name,
                                    unsigned short& nameBufferSize);
    bool GetChannelLongNameByLanguage(unsigned short channelIndex,
                                      unsigned long language,
                                      char* name,
                                      unsigned short& nameBufferSize);

  private:
    class CRecordLvct : public IRecord
    {
      public:
        CRecordLvct()
        {
          TableId = 0;
          SectionTransportStreamId = 0;
          MapId = 0;
          ShortName = NULL;
          MajorChannelNumber = 0;
          MinorChannelNumber = 0;
          ModulationMode = 0;
          CarrierFrequency = 0;
          TransportStreamId = 0;
          ProgramNumber = 0;
          EtmLocation = 0;
          AccessControlled = false;
          Hidden = false;
          PathSelect = 0;
          OutOfBand = false;
          HideGuide = false;
          ServiceType = 0;
          SourceId = 0;
          StreamCountVideo = 0;
          StreamCountAudio = 0;
          IsThreeDimensional = false;
        }

        ~CRecordLvct()
        {
          if (ShortName != NULL)
          {
            delete[] ShortName;
            ShortName = NULL;
          }
          CUtils::CleanUpStringSet(LongNames);
        }

        bool Equals(const IRecord* record) const
        {
          const CRecordLvct* recordLvct = dynamic_cast<const CRecordLvct*>(record);
          if (
            recordLvct == NULL ||
            TableId != recordLvct->TableId ||
            SectionTransportStreamId != recordLvct->SectionTransportStreamId ||
            MapId != recordLvct->MapId ||
            !CUtils::CompareStrings(ShortName, recordLvct->ShortName) ||
            !CUtils::CompareStringSets(LongNames, recordLvct->LongNames) ||
            MajorChannelNumber != recordLvct->MajorChannelNumber ||
            MinorChannelNumber != recordLvct->MinorChannelNumber ||
            ModulationMode != recordLvct->ModulationMode ||
            CarrierFrequency != recordLvct->CarrierFrequency ||
            TransportStreamId != recordLvct->TransportStreamId ||
            ProgramNumber != recordLvct->ProgramNumber ||
            EtmLocation != recordLvct->EtmLocation ||
            AccessControlled != recordLvct->AccessControlled ||
            Hidden != recordLvct->Hidden ||
            PathSelect != recordLvct->PathSelect ||
            OutOfBand != recordLvct->OutOfBand ||
            HideGuide != recordLvct->HideGuide ||
            ServiceType != recordLvct->ServiceType ||
            SourceId != recordLvct->SourceId ||
            StreamCountVideo != recordLvct->StreamCountVideo ||
            StreamCountAudio != recordLvct->StreamCountAudio ||
            IsThreeDimensional != recordLvct->IsThreeDimensional ||
            !CUtils::CompareVectors(AudioLanguages, recordLvct->AudioLanguages) ||
            !CUtils::CompareVectors(CaptionsLanguages, recordLvct->CaptionsLanguages)
          )
          {
            return false;
          }
          return true;
        }

        unsigned long long GetKey() const
        {
          return ((unsigned long long)TableId << 32) | (MajorChannelNumber << 16) | MinorChannelNumber;
        }

        unsigned long long GetExpiryKey() const
        {
          return (TableId << 16) | SectionTransportStreamId | MapId;
        }

        void Debug(const wchar_t* situation) const
        {
          LogDebug(L"LVCT: channel %s, table ID = 0x%hhx, section TSID = %hu, map ID = %hu, TSID = %hu, program number = %hu, source ID = %hu, short name = %S, long name count = %llu, major channel = %hu, minor channel = %hu, service type = %hhu, carrier frequency = %lu Hz, modulation mode = %hhu, access controlled = %d, hidden = %d, hide guide = %d, video stream count = %hhu, audio stream count = %hhu, audio language count = %llu, captions language count = %llu, is 3D = %d, ETM location = %hhu, path select = %hhu, out of band = %d",
                    situation, TableId, SectionTransportStreamId, MapId,
                    TransportStreamId, ProgramNumber, SourceId,
                    ShortName == NULL ? "" : ShortName,
                    (unsigned long long)LongNames.size(), MajorChannelNumber,
                    MinorChannelNumber, ServiceType, CarrierFrequency,
                    ModulationMode, AccessControlled, Hidden, HideGuide,
                    StreamCountVideo, StreamCountAudio,
                    (unsigned long long)AudioLanguages.size(),
                    (unsigned long long)CaptionsLanguages.size(),
                    IsThreeDimensional, EtmLocation, PathSelect, OutOfBand);

          CUtils::DebugStringMap(LongNames, L"long name(s)", L"language", L"name");
          CUtils::DebugVector(AudioLanguages, L"audio language(s)", true);
          CUtils::DebugVector(CaptionsLanguages, L"captions language(s)", true);
        }

        void OnReceived(void* callBack) const
        {
          ICallBackLvct* callBackLvct = static_cast<ICallBackLvct*>(callBack);
          if (callBackLvct != NULL)
          {
            callBackLvct->OnLvctReceived(TableId,
                                          SectionTransportStreamId,
                                          MapId,
                                          ShortName,
                                          LongNames,
                                          MajorChannelNumber,
                                          MinorChannelNumber,
                                          ModulationMode,
                                          CarrierFrequency,
                                          TransportStreamId,
                                          ProgramNumber,
                                          EtmLocation,
                                          AccessControlled,
                                          Hidden,
                                          PathSelect,
                                          OutOfBand,
                                          HideGuide,
                                          ServiceType,
                                          SourceId,
                                          StreamCountVideo,
                                          StreamCountAudio,
                                          IsThreeDimensional,
                                          AudioLanguages,
                                          CaptionsLanguages);
          }
        }

        void OnChanged(void* callBack) const
        {
          ICallBackLvct* callBackLvct = static_cast<ICallBackLvct*>(callBack);
          if (callBackLvct != NULL)
          {
            callBackLvct->OnLvctChanged(TableId,
                                          SectionTransportStreamId,
                                          MapId,
                                          ShortName,
                                          LongNames,
                                          MajorChannelNumber,
                                          MinorChannelNumber,
                                          ModulationMode,
                                          CarrierFrequency,
                                          TransportStreamId,
                                          ProgramNumber,
                                          EtmLocation,
                                          AccessControlled,
                                          Hidden,
                                          PathSelect,
                                          OutOfBand,
                                          HideGuide,
                                          ServiceType,
                                          SourceId,
                                          StreamCountVideo,
                                          StreamCountAudio,
                                          IsThreeDimensional,
                                          AudioLanguages,
                                          CaptionsLanguages);
          }
        }

        void OnRemoved(void* callBack) const
        {
          ICallBackLvct* callBackLvct = static_cast<ICallBackLvct*>(callBack);
          if (callBackLvct != NULL)
          {
            callBackLvct->OnLvctRemoved(TableId,
                                          SectionTransportStreamId,
                                          MapId,
                                          ShortName,
                                          LongNames,
                                          MajorChannelNumber,
                                          MinorChannelNumber,
                                          ModulationMode,
                                          CarrierFrequency,
                                          TransportStreamId,
                                          ProgramNumber,
                                          EtmLocation,
                                          AccessControlled,
                                          Hidden,
                                          PathSelect,
                                          OutOfBand,
                                          HideGuide,
                                          ServiceType,
                                          SourceId,
                                          StreamCountVideo,
                                          StreamCountAudio,
                                          IsThreeDimensional,
                                          AudioLanguages,
                                          CaptionsLanguages);

          }
        }

        unsigned char TableId;
        unsigned short SectionTransportStreamId;
        unsigned short MapId;
        char* ShortName;
        map<unsigned long, char*> LongNames;
        unsigned short MajorChannelNumber;
        unsigned short MinorChannelNumber;
        unsigned char ModulationMode;
        unsigned long CarrierFrequency;
        unsigned short TransportStreamId;
        unsigned short ProgramNumber;
        unsigned char EtmLocation;
        bool AccessControlled;
        bool Hidden;
        unsigned char PathSelect;
        bool OutOfBand;
        bool HideGuide;
        unsigned char ServiceType;
        unsigned short SourceId;
        unsigned char StreamCountVideo;
        unsigned char StreamCountAudio;
        bool IsThreeDimensional;
        vector<unsigned long> AudioLanguages;
        vector<unsigned long> CaptionsLanguages;
    };

    bool SelectChannelRecordByIndex(unsigned short index);

    static bool DecodeChannelRecord(unsigned char* sectionData,
                                    unsigned short& pointer,
                                    unsigned short endOfSection,
                                    CRecordLvct& record);

    static bool DecodeParameterizedServiceDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      bool& isThreeDimensional);
    static bool DecodeServiceLocationDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                unsigned char& streamCountVideo,
                                                unsigned char& streamCountAudio,
                                                bool& isThreeDimensional,
                                                vector<unsigned long>& audioLanguages,
                                                vector<unsigned long>& captionsLanguages);
    static bool DecodeComponentListDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned char& streamCountVideo,
                                              unsigned char& streamCountAudio,
                                              bool& isThreeDimensional);

    CCriticalSection m_section;
    unsigned short m_pid;
    vector<unsigned long long> m_seenSections;
    vector<unsigned long long> m_unseenSections;
    bool m_isSeenCable;
    bool m_isSeenTerrestrial;
    bool m_isReady;
    clock_t m_completeTime;
    ICallBackLvct* m_callBack;
    CRecordStore m_records;

    CRecordLvct* m_currentRecord;
    unsigned short m_currentRecordIndex;
};