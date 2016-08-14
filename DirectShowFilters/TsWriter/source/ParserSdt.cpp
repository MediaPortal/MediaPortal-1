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
#include "ParserSdt.h"
#include <algorithm>    // find()
#include <cstring>      // strcmp(), strncmp(), strncpy()
#include "..\..\shared\TimeUtils.h"
#include "EnterCriticalSection.h"
#include "TextUtil.h"
#include "Utils.h"


#define LANG_UND 0x646e75

// There is no private data specifier for Dish descriptors, so we have to
// determine scope with ONID. These are the ONIDs for EchoStar networks.
// http://www.dvbservices.com/identifiers/original_network_id&tab=table
#define ORIGINAL_NETWORK_ID_DISH_START  0x1001
#define ORIGINAL_NETWORK_ID_DISH_END    0x100b


extern void LogDebug(const wchar_t* fmt, ...);

CParserSdt::CParserSdt() : m_records(600000)
{
  m_isOtherReady = false;
  m_otherCompleteTime = 0;
  m_actualOriginalNetworkId = 0;

  m_currentRecord = NULL;
  m_currentRecordIndex = 0xffff;
  m_referenceRecord = NULL;

  SetPid(PID_SDT);
  SetCallBack(NULL);
}

CParserSdt::~CParserSdt()
{
  SetCallBack(NULL);
}

void CParserSdt::Reset(bool enableCrcCheck)
{
  LogDebug(L"SDT %d: reset", GetPid());
  CEnterCriticalSection lock(m_section);
  m_records.RemoveAllRecords();
  EnableCrcCheck(enableCrcCheck);
  CSectionDecoder::Reset();
  m_seenSectionsActual.clear();
  m_unseenSectionsActual.clear();
  m_seenSectionsOther.clear();
  m_unseenSectionsOther.clear();
  m_isOtherReady = false;
  m_actualOriginalNetworkId = 0;
  m_currentRecord = NULL;
  m_currentRecordIndex = 0xffff;
  m_referenceRecord = NULL;
  LogDebug(L"SDT %d: reset done", GetPid());
}

void CParserSdt::SetCallBack(ICallBackSdt* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserSdt::OnNewSection(CSection& section)
{
  try
  {
    if (
      (section.table_id != TABLE_ID_SDT_ACTUAL && section.table_id != TABLE_ID_SDT_OTHER) ||
      !section.SectionSyntaxIndicator ||
      !section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.section_length > 1021 || section.section_length < 12)
    {
      LogDebug(L"SDT %d: invalid section, length = %d, table ID = 0x%x",
                GetPid(), section.section_length, section.table_id);
      return;
    }

    unsigned char* data = section.Data;
    unsigned short transportStreamId = section.table_id_extension;
    unsigned short originalNetworkId = (data[8] << 8) | data[9];
    //LogDebug(L"SDT %d: table ID = 0x%x, TSID = %hu, ONID = %hu, version number = %d, section length = %d, section number = %d, last section number = %d",
    //          GetPid(), section.table_id, transportStreamId,
    //          originalNetworkId, section.version_number,
    //          section.section_length, section.SectionNumber,
    //          section.LastSectionNumber);

    CEnterCriticalSection lock(m_section);
    vector<unsigned long long>* seenSections;
    vector<unsigned long long>* unseenSections;
    if (section.table_id == TABLE_ID_SDT_ACTUAL)
    {
      m_actualOriginalNetworkId = originalNetworkId;
      seenSections = &m_seenSectionsActual;
      unseenSections = &m_unseenSectionsActual;
    }
    else
    {
      seenSections = &m_seenSectionsOther;
      unseenSections = &m_unseenSectionsOther;
    }

    // Have we seen this section before?
    unsigned long long sectionKey = ((unsigned long long)section.table_id << 48) | ((unsigned long long)section.version_number << 40) | ((unsigned long long)originalNetworkId << 24) | ((unsigned long long)transportStreamId << 8) | section.SectionNumber;
    unsigned long long sectionGroupMask = 0xffff00ffffffff00;
    unsigned long long sectionGroupKey = sectionKey & sectionGroupMask;
    vector<unsigned long long>::const_iterator sectionIt = find(seenSections->begin(),
                                                                seenSections->end(),
                                                                sectionKey);
    if (sectionIt != seenSections->end())
    {
      // Yes. We might be ready!
      //LogDebug(L"SDT %d: previously seen section, table ID = 0x%x, TSID = %hu, ONID = %hu, section number = %d",
      //          GetPid(), section.table_id, transportStreamId,
      //          originalNetworkId, section.SectionNumber);
      if (m_isOtherReady || m_unseenSectionsOther.size() != 0)
      {
        return;
      }

      // TS 101 211 section 4.4 recommends minimum repetition rates:
      // SDT actual = 2 seconds
      // SDT other = 10 seconds
      // This code only handles other time out. Actual should only have one
      // combination of ONID + TSID, which makes completion deterministic.
      if (CTimeUtils::ElapsedMillis(m_otherCompleteTime) >= 5000)
      {
        if (m_unseenSectionsActual.size() == 0)
        {
          m_records.RemoveExpiredRecords(m_callBack);
        }
        LogDebug(L"SDT %d: other ready, sections parsed = %llu, service count = %lu",
                  GetPid(), (unsigned long long)m_seenSectionsOther.size(),
                  m_records.GetRecordCount());
        m_isOtherReady = true;
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_SDT_OTHER);
        }
      }
      return;
    }

    // Were we expecting this section?
    sectionIt = find(unseenSections->begin(), unseenSections->end(), sectionKey);
    if (sectionIt == unseenSections->end())
    {
      // No. Is this a change/update, or just a new section group?
      bool isChange = false;
      if (section.table_id == TABLE_ID_SDT_ACTUAL)
      {
        isChange = m_unseenSectionsActual.size() != 0 || m_seenSectionsActual.size() != 0;
      }
      else
      {
        isChange = m_isOtherReady;
        vector<unsigned long long>::const_iterator tempSectionIt = unseenSections->begin();
        while (tempSectionIt != unseenSections->end())
        {
          if ((*tempSectionIt & sectionGroupMask) == sectionGroupKey)
          {
            isChange = true;
            tempSectionIt = unseenSections->erase(tempSectionIt);
          }
          else
          {
            tempSectionIt++;
          }
        }

        tempSectionIt = seenSections->begin();
        while (tempSectionIt != seenSections->end())
        {
          if ((*tempSectionIt & sectionGroupMask) == sectionGroupKey)
          {
            isChange = true;
            tempSectionIt = seenSections->erase(tempSectionIt);
          }
          else
          {
            tempSectionIt++;
          }
        }
      }

      if (isChange)
      {
        LogDebug(L"SDT %d: changed, table ID = 0x%x, TSID = %hu, ONID = %hu, version number = %d, section number = %d, last section number = %d",
                  GetPid(), section.table_id, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber, section.LastSectionNumber);
        m_records.MarkExpiredRecords(((unsigned long long)section.table_id << 32) | (originalNetworkId << 16) | transportStreamId);

        if (section.table_id == TABLE_ID_SDT_ACTUAL)
        {
          seenSections->clear();
          unseenSections->clear();
          if (m_callBack != NULL)
          {
            m_callBack->OnTableChange(TABLE_ID_SDT_ACTUAL);
          }
        }
        else if (m_isOtherReady)
        {
          m_isOtherReady = false;
          if (m_callBack != NULL)
          {
            m_callBack->OnTableChange(TABLE_ID_SDT_OTHER);
          }
        }
      }
      else
      {
        LogDebug(L"SDT %d: received, table ID = 0x%x, TSID = %hu, ONID = %hu, version number = %d, section number = %d, last section number = %d",
                  GetPid(), section.table_id, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber, section.LastSectionNumber);
        if (
          m_callBack != NULL &&
          (
            (section.table_id == TABLE_ID_SDT_ACTUAL && m_seenSectionsActual.size() == 0) ||
            (section.table_id == TABLE_ID_SDT_OTHER && m_seenSectionsOther.size() == 0)
          )
        )
        {
          m_callBack->OnTableSeen(section.table_id);
        }
      }

      unsigned long long baseKey = sectionKey & 0xffffffffffffff00;
      for (unsigned char s = 0; s <= section.LastSectionNumber; s++)
      {
        unseenSections->push_back(baseKey + s);
      }
      sectionIt = find(unseenSections->begin(), unseenSections->end(), sectionKey);
    }
    else
    {
      //LogDebug(L"SDT %d: new section, table ID = 0x%x, TSID = %hu, ONID = %hu, version number = %d, section number = %d",
      //          GetPid(), section.table_id, transportStreamId,
      //          originalNetworkId, section.version_number,
      //          section.SectionNumber);
    }

    unsigned short pointer = 11;                              // points to the first byte in the service loop
    unsigned short endOfSection = section.section_length - 1; // points to the first byte in the CRC
    while (pointer + 4 < endOfSection)
    {
      CRecordSdt* record = new CRecordSdt();
      if (record == NULL)
      {
        LogDebug(L"SDT %d: failed to allocate record, table ID = 0x%x, TSID = %hu, ONID = %hu, version number = %d, section number = %d",
                  GetPid(), section.table_id, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber);
        return;
      }

      record->TableId = section.table_id;
      record->OriginalNetworkId = originalNetworkId;
      record->TransportStreamId = transportStreamId;
      if (!DecodeServiceRecord(data, pointer, endOfSection, *record))
      {
        LogDebug(L"SDT %d: invalid section, table ID = 0x%x, TSID = %hu, ONID = %hu, version number = %d, section number = %d, service ID = %hu",
                  GetPid(), section.table_id, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber, record->ServiceId);
        delete record;
        return;
      }

      m_records.AddOrUpdateRecord((IRecord**)&record, m_callBack);
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"SDT %d: section parsing error, pointer = %hu, end of section = %hu, table ID = 0x%x, TSID = %hu, ONID = %hu, version number = %d, section number = %d",
                GetPid(), pointer, endOfSection, section.table_id,
                transportStreamId, originalNetworkId,
                section.version_number, section.SectionNumber);
      return;
    }

    seenSections->push_back(sectionKey);
    unseenSections->erase(sectionIt);
    if (unseenSections->size() == 0)
    {
      if (section.table_id == TABLE_ID_SDT_ACTUAL)
      {
        if (m_isOtherReady)
        {
          m_records.RemoveExpiredRecords(m_callBack);
        }
        LogDebug(L"SDT %d: actual ready, sections parsed = %llu, record count = %lu",
                  GetPid(), (unsigned long long)m_seenSectionsActual.size(),
                  m_records.GetRecordCount());
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_SDT_ACTUAL);
        }
      }
      else
      {
        // We can't assume that we've seen all sections yet, because sections
        // for another transport stream and/or network may not have been
        // received.
        m_otherCompleteTime = clock();
      }
    }
  }
  catch (...)
  {
    LogDebug(L"SDT %d: unhandled exception in OnNewSection()", GetPid());
  }
}

bool CParserSdt::IsSeenActual() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSectionsActual.size() != 0;
}

bool CParserSdt::IsSeenOther() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSectionsOther.size() != 0;
}

bool CParserSdt::IsReadyActual() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSectionsActual.size() != 0 && m_unseenSectionsActual.size() == 0;
}

bool CParserSdt::IsReadyOther() const
{
  CEnterCriticalSection lock(m_section);
  return m_isOtherReady;
}

void CParserSdt::GetServiceCount(unsigned short& actualOriginalNetworkId,
                                  unsigned short& serviceCount) const
{
  CEnterCriticalSection lock(m_section);
  actualOriginalNetworkId = m_actualOriginalNetworkId;
  serviceCount = (unsigned short)m_records.GetRecordCount();
}

bool CParserSdt::GetService(unsigned short index,
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
                            unsigned char& openTvCategoryId,
                            unsigned char& virginMediaCategoryId,
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
                            unsigned short& epgServiceId)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectServiceRecordByIndex(index))
  {
    return false;
  }

  tableId = m_currentRecord->TableId;
  originalNetworkId = m_currentRecord->OriginalNetworkId;
  transportStreamId = m_currentRecord->TransportStreamId;
  serviceId = m_currentRecord->ServiceId;
  eitScheduleFlag = m_currentRecord->EitScheduleFlag;
  eitPresentFollowingFlag = m_currentRecord->EitPresentFollowingFlag;
  runningStatus = m_currentRecord->RunningStatus;
  freeCaMode = m_currentRecord->FreeCaMode;

  if (m_referenceRecord == NULL)
  {
    serviceType = m_currentRecord->ServiceType;
    serviceNameCount = m_currentRecord->ServiceNames.size();
  }
  else
  {
    serviceType = m_referenceRecord->ServiceType;
    // Sometimes [with Sky NZ] the reference service has no name but the
    // time-shifted service does.
    if (m_referenceRecord->ServiceNames.size() > 0)
    {
      serviceNameCount = m_referenceRecord->ServiceNames.size();
    }
    else
    {
      serviceNameCount = m_currentRecord->ServiceNames.size();
    }
  }

  logicalChannelNumber = m_currentRecord->LogicalChannelNumber;
  dishSubChannelNumber = m_currentRecord->DishSubChannelNumber;
  visibleInGuide = m_currentRecord->VisibleInGuide;
  referenceServiceId = m_currentRecord->ReferenceServiceId;
  isHighDefinition = m_currentRecord->IsHighDefinition;
  isStandardDefinition = m_currentRecord->IsStandardDefinition;
  isThreeDimensional = m_currentRecord->IsThreeDimensional;
  streamCountVideo = m_currentRecord->StreamCountVideo;
  streamCountAudio = m_currentRecord->StreamCountAudio;
  openTvCategoryId = m_currentRecord->OpenTvCategoryId;
  virginMediaCategoryId = m_currentRecord->VirginMediaCategoryId;
  dishMarketId = m_currentRecord->DishMarketId;
  previousOriginalNetworkId = m_currentRecord->PreviousOriginalNetworkId;
  previousTransportStreamId = m_currentRecord->PreviousTransportStreamId;
  previousServiceId = m_currentRecord->PreviousServiceId;
  epgOriginalNetworkId = m_currentRecord->EpgOriginalNetworkId;
  epgTransportStreamId = m_currentRecord->EpgTransportStreamId;
  epgServiceId = m_currentRecord->EpgServiceId;

  unsigned char requiredCount = 0;
  if (!CUtils::CopyVectorToArray(m_currentRecord->AudioLanguages,
                                  audioLanguages,
                                  audioLanguageCount,
                                  requiredCount) && audioLanguages != NULL)
  {
    LogDebug(L"SDT %d: insufficient audio language array size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, tableId, originalNetworkId, transportStreamId,
              serviceId, referenceServiceId, requiredCount,
              audioLanguageCount);
  }
  if (!CUtils::CopyVectorToArray(m_currentRecord->SubtitlesLanguages,
                                  subtitlesLanguages,
                                  subtitlesLanguageCount,
                                  requiredCount) && subtitlesLanguages != NULL)
  {
    LogDebug(L"SDT %d: insufficient subtitles language array size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, tableId, originalNetworkId, transportStreamId,
              serviceId, referenceServiceId, requiredCount,
              subtitlesLanguageCount);
  }

  // Assumption: for time-shifted/NVOD services, the following details are only
  // kept with the reference service.
  CRecordSdt* recordSdt = m_currentRecord;
  if (m_referenceRecord != NULL)
  {
    recordSdt = m_referenceRecord;
  }
  if (!CUtils::CopyVectorToArray(recordSdt->AvailableInCountries,
                                  availableInCountries,
                                  availableInCountryCount,
                                  requiredCount) && availableInCountries != NULL)
  {
    LogDebug(L"SDT %d: insufficient available in country array size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, tableId, originalNetworkId, transportStreamId,
              serviceId, referenceServiceId, requiredCount,
              availableInCountryCount);
  }
  if (!CUtils::CopyVectorToArray(recordSdt->UnavailableInCountries,
                                  unavailableInCountries,
                                  unavailableInCountryCount,
                                  requiredCount) && unavailableInCountries != NULL)
  {
    LogDebug(L"SDT %d: insufficient unavailable in country array size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, tableId, originalNetworkId, transportStreamId,
              serviceId, referenceServiceId, requiredCount,
              unavailableInCountryCount);
  }
  if (!CUtils::CopyVectorToArray(recordSdt->AvailableInCells,
                                  availableInCells,
                                  availableInCellCount,
                                  requiredCount) && availableInCells != NULL)
  {
    LogDebug(L"SDT %d: insufficient available in cell array size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, tableId, originalNetworkId, transportStreamId,
              serviceId, referenceServiceId, requiredCount,
              availableInCellCount);
  }
  if (!CUtils::CopyVectorToArray(recordSdt->UnavailableInCells,
                                  unavailableInCells,
                                  unavailableInCellCount,
                                  requiredCount) && unavailableInCells != NULL)
  {
    LogDebug(L"SDT %d: insufficient unavailable in cell array size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, tableId, originalNetworkId, transportStreamId,
              serviceId, referenceServiceId, requiredCount,
              unavailableInCellCount);
  }
  if (!CUtils::CopyVectorToArray(recordSdt->TargetRegionIds,
                                  targetRegionIds,
                                  targetRegionIdCount,
                                  requiredCount) && targetRegionIds != NULL)
  {
    LogDebug(L"SDT %d: insufficient target region ID array size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, tableId, originalNetworkId, transportStreamId,
              serviceId, referenceServiceId, requiredCount,
              targetRegionIdCount);
  }

  return true;
}

bool CParserSdt::GetServiceNameByIndex(unsigned short serviceIndex,
                                        unsigned char nameIndex,
                                        unsigned long& language,
                                        char* providerName,
                                        unsigned short& providerNameBufferSize,
                                        char* serviceName,
                                        unsigned short& serviceNameBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectServiceRecordByIndex(serviceIndex))
  {
    return false;
  }

  CRecordSdt* recordSdt = m_currentRecord;
  if (m_referenceRecord != NULL && m_referenceRecord->ServiceNames.size() > 0)
  {
    recordSdt = m_referenceRecord;
  }

  if (nameIndex >= recordSdt->ServiceNames.size())
  {
    LogDebug(L"SDT %d: invalid service name index, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, name index = %hhu, name count = %llu",
              GetPid(), serviceIndex, recordSdt->TableId,
              recordSdt->OriginalNetworkId, recordSdt->TransportStreamId,
              m_currentRecord->ServiceId, m_currentRecord->ReferenceServiceId,
              nameIndex, (unsigned long long)recordSdt->ServiceNames.size());
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->ServiceNames.begin();
  for ( ; it != m_currentRecord->ServiceNames.end(); it++)
  {
    if (nameIndex != 0)
    {
      nameIndex--;
      continue;
    }

    language = it->first;
    unsigned short requiredBufferSize = 0;
    if (!CUtils::CopyStringToBuffer(it->second,
                                    serviceName,
                                    serviceNameBufferSize,
                                    requiredBufferSize))
    {
      LogDebug(L"SDT %d: insufficient service name buffer size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, name index = %hhu, language = %S, required size = %hu, actual size = %hu",
                GetPid(), serviceIndex, recordSdt->TableId,
                recordSdt->OriginalNetworkId, recordSdt->TransportStreamId,
                m_currentRecord->ServiceId,
                m_currentRecord->ReferenceServiceId, nameIndex,
                (char*)&language, requiredBufferSize, serviceNameBufferSize);
    }

    it = m_currentRecord->ProviderNames.find(language);
    char* temp = NULL;
    if (it != m_currentRecord->ProviderNames.end())
    {
      temp = it->second;
    }
    if (!CUtils::CopyStringToBuffer(temp,
                                    providerName,
                                    providerNameBufferSize,
                                    requiredBufferSize))
    {
      LogDebug(L"SDT %d: insufficient provider name buffer size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, name index = %hhu, language = %S, required size = %hu, actual size = %hu",
                GetPid(), serviceIndex, recordSdt->TableId,
                recordSdt->OriginalNetworkId, recordSdt->TransportStreamId,
                m_currentRecord->ServiceId,
                m_currentRecord->ReferenceServiceId, nameIndex,
                (char*)&language, requiredBufferSize, providerNameBufferSize);
    }
    return true;
  }
  return false;
}

bool CParserSdt::GetServiceNameByLanguage(unsigned short serviceIndex,
                                          unsigned long language,
                                          char* providerName,
                                          unsigned short& providerNameBufferSize,
                                          char* serviceName,
                                          unsigned short& serviceNameBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectServiceRecordByIndex(serviceIndex))
  {
    return false;
  }

  CRecordSdt* recordSdt = m_currentRecord;
  if (m_referenceRecord != NULL && m_referenceRecord->ServiceNames.size() > 0)
  {
    recordSdt = m_referenceRecord;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->ServiceNames.find(language);
  if (it == m_currentRecord->ServiceNames.end())
  {
    LogDebug(L"SDT %d: invalid service name language, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, language = %S",
              GetPid(), serviceIndex, recordSdt->TableId,
              recordSdt->OriginalNetworkId, recordSdt->TransportStreamId,
              m_currentRecord->ServiceId, m_currentRecord->ReferenceServiceId,
              (char*)&language);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(it->second,
                                  serviceName,
                                  serviceNameBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"SDT %d: insufficient service name buffer size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, language = %S, required size = %hu, actual size = %hu",
              GetPid(), serviceIndex, recordSdt->TableId,
              recordSdt->OriginalNetworkId, recordSdt->TransportStreamId,
              m_currentRecord->ServiceId, m_currentRecord->ReferenceServiceId,
              (char*)&language, requiredBufferSize, serviceNameBufferSize);
  }

  it = m_currentRecord->ProviderNames.find(language);
  char* temp = NULL;
  if (it != m_currentRecord->ProviderNames.end())
  {
    temp = it->second;
  }
  if (!CUtils::CopyStringToBuffer(temp,
                                  providerName,
                                  providerNameBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"SDT %d: insufficient provider name buffer size, service index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu, language = %S, required size = %hu, actual size = %hu",
              GetPid(), serviceIndex, recordSdt->TableId,
              recordSdt->OriginalNetworkId, recordSdt->TransportStreamId,
              m_currentRecord->ServiceId, m_currentRecord->ReferenceServiceId,
              (char*)&language, requiredBufferSize, providerNameBufferSize);
  }
  return true;
}

bool CParserSdt::GetDefaultAuthority(unsigned short originalNetworkId,
                                      unsigned short transportStreamId,
                                      unsigned short serviceId,
                                      char* defaultAuthority,
                                      unsigned short& defaultAuthorityBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  unsigned char tableId = TABLE_ID_SDT_ACTUAL;
  unsigned long long key = ((unsigned long long)tableId << 48) | ((unsigned long long)originalNetworkId << 32) | (transportStreamId << 16) | serviceId;
  IRecord* record = NULL;
  if (!m_records.GetRecordByKey(key, &record) || record == NULL)
  {
    // Not an error. It is entirely possible that we don't have the details for
    // the requested service.
    tableId = TABLE_ID_SDT_OTHER;
    key = ((unsigned long long)tableId << 48) | ((unsigned long long)originalNetworkId << 32) | (transportStreamId << 16) | serviceId;
    if (!m_records.GetRecordByKey(key, &record) || record == NULL)
    {
      // Not an error, as above.
      return false;
    }
  }

  CRecordSdt* serviceRecord = dynamic_cast<CRecordSdt*>(record);
  if (serviceRecord == NULL)
  {
    LogDebug(L"SDT %d: invalid service record, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu",
              GetPid(), tableId, originalNetworkId, transportStreamId,
              serviceId);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(serviceRecord->DefaultAuthority,
                                  defaultAuthority,
                                  defaultAuthorityBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"SDT %d: insufficient default authority buffer size, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hu, actual size = %hu",
              GetPid(), tableId, originalNetworkId, transportStreamId,
              serviceId, requiredBufferSize, defaultAuthorityBufferSize);
  }
  return true;
}

bool CParserSdt::SelectServiceRecordByIndex(unsigned short index)
{
  if (m_currentRecord != NULL && m_currentRecordIndex == index)
  {
    return true;
  }

  IRecord* record = NULL;
  if (!m_records.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"SDT %d: invalid service index, index = %hu, record count = %lu",
              GetPid(), index, m_records.GetRecordCount());
    return false;
  }

  m_currentRecord = dynamic_cast<CRecordSdt*>(record);
  if (m_currentRecord == NULL)
  {
    LogDebug(L"SDT %d: invalid service record, index = %hu", GetPid(), index);
    return false;
  }
  m_currentRecordIndex = index;
  m_referenceRecord = NULL;

  if (m_currentRecord->ReferenceServiceId == 0)
  {
    return true;
  }

  unsigned long long key = m_currentRecord->GetKey() & 0xffffffffffff0000;
  key |= m_currentRecord->ReferenceServiceId;
  if (!m_records.GetRecordByKey(key, &record) || record == NULL)
  {
    LogDebug(L"SDT %d: invalid reference service key, index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu",
              GetPid(), index, m_currentRecord->TableId,
              m_currentRecord->OriginalNetworkId,
              m_currentRecord->TransportStreamId, m_currentRecord->ServiceId,
              m_currentRecord->ReferenceServiceId);
    return true;
  }

  m_referenceRecord = dynamic_cast<CRecordSdt*>(record);
  if (m_referenceRecord == NULL)
  {
    LogDebug(L"SDT %d: invalid reference service record, index = %hu, table ID = 0x%hhx, ONID = %hu, TSID = %hu, service ID = %hu, reference service ID = %hu",
              GetPid(), index, m_currentRecord->TableId,
              m_currentRecord->OriginalNetworkId,
              m_currentRecord->TransportStreamId, m_currentRecord->ServiceId,
              m_currentRecord->ReferenceServiceId);
  }
  return true;
}

bool CParserSdt::DecodeServiceRecord(unsigned char* sectionData,
                                      unsigned short& pointer,
                                      unsigned short endOfSection,
                                      CRecordSdt& record)
{
  if (pointer + 5 > endOfSection)
  {
    LogDebug(L"SDT: invalid service record, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.ServiceId = (sectionData[pointer] << 8) | sectionData[pointer + 1];
    pointer += 2;
    record.EitScheduleFlag = (sectionData[pointer] & 2) != 0;
    record.EitPresentFollowingFlag = (sectionData[pointer] & 1) != 0;
    pointer++;
    record.RunningStatus = (sectionData[pointer] >> 5) & 7;
    record.FreeCaMode = (sectionData[pointer] & 0x10) != 0;
    unsigned short descriptorsLoopLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;

    //LogDebug(L"SDT: service ID = %hu, EIT schedule flag = %d, EIT present following flag = %d, running status = %hhu, free CA mode = %d, descriptors loop length = %hu",
    //          record.ServiceId, record.EitScheduleFlag,
    //          record.EitPresentFollowingFlag, record.RunningStatus,
    //          record.FreeCaMode, descriptorsLoopLength);

    if (
      record.OriginalNetworkId >= ORIGINAL_NETWORK_ID_DISH_START &&
      record.OriginalNetworkId <= ORIGINAL_NETWORK_ID_DISH_END
    )
    {
      record.LogicalChannelNumber = record.ServiceId;
    }

    unsigned short endOfDescriptorLoop = pointer + descriptorsLoopLength;
    if (endOfDescriptorLoop > endOfSection)
    {
      LogDebug(L"SDT: invalid service record, descriptors loop length = %hu, pointer = %hu, end of section = %hu",
                descriptorsLoopLength, pointer, endOfSection);
      return false;
    }

    return DecodeServiceDescriptors(sectionData, pointer, endOfDescriptorLoop, record);
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeServiceRecord()");
  }
  return false;
}

bool CParserSdt::DecodeServiceDescriptors(unsigned char* sectionData,
                                          unsigned short& pointer,
                                          unsigned short endOfDescriptorLoop,
                                          CRecordSdt& record)
{
  unsigned long privateDataSpecifier = 0;
  while (pointer + 1 < endOfDescriptorLoop)
  {
    unsigned char tag = sectionData[pointer++];
    unsigned char length = sectionData[pointer++];
    unsigned short endOfDescriptor = pointer + length;
    //LogDebug(L"SDT: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, private data specifier = %lu",
    //          tag, length, pointer, privateDataSpecifier);
    if (endOfDescriptor > endOfDescriptorLoop)
    {
      LogDebug(L"SDT: invalid service record, descriptor length = %hhu, pointer = %hu, end of descriptor loop = %hu, tag = 0x%hhx, private data specifier = %lu",
                length, pointer, endOfDescriptorLoop, tag,
                privateDataSpecifier);
      return false;
    }

    bool descriptorParseResult = true;
    if (tag == 0x48) // service descriptor
    {
      char* providerName = NULL;
      char* serviceName = NULL;
      descriptorParseResult = DecodeServiceDescriptor(&sectionData[pointer],
                                                      length,
                                                      record.ServiceType,
                                                      &providerName,
                                                      &serviceName);
      if (descriptorParseResult)
      {
        if (
          record.ServiceType == 0x11 ||
          (record.ServiceType >= 0x19 && record.ServiceType <= 0x1f)
        )
        {
          record.IsHighDefinition = true;
          if (record.ServiceType >= 0x1c && record.ServiceType <= 0x1e)   // Sky UK: 0x80 - 0x84 = 3D [too generic]
          {
            record.IsThreeDimensional = true;
          }
        }

        if (providerName != NULL)
        {
          char* existingName = record.ProviderNames[LANG_UND];
          if (existingName != NULL)
          {
            if (strcmp(existingName, providerName) != 0)
            {
              LogDebug(L"SDT: replacing provider name, table ID = 0x%hhx, TSID = %hu, ONID = %hu, service ID = %hu, current = %S, new = %S",
                        record.TableId, record.TransportStreamId,
                        record.OriginalNetworkId, record.ServiceId,
                        existingName, providerName);
            }
            delete[] existingName;
          }
          record.ProviderNames[LANG_UND] = providerName;
        }
        if (serviceName != NULL)
        {
          char* existingName = record.ServiceNames[LANG_UND];
          if (existingName != NULL)
          {
            if (strcmp(existingName, serviceName) != 0)
            {
              LogDebug(L"SDT: replacing service name, table ID = 0x%hhx, TSID = %hu, ONID = %hu, service ID = %hu, current = %S, new = %S",
                        record.TableId, record.TransportStreamId,
                        record.OriginalNetworkId, record.ServiceId,
                        existingName, serviceName);
            }
            delete[] existingName;
          }
          record.ServiceNames[LANG_UND] = serviceName;
        }
      }
    }
    else if (tag == 0x49) // country availability descriptor
    {
      descriptorParseResult = DecodeCountryAvailabilityDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  record.AvailableInCountries,
                                                                  record.UnavailableInCountries);
    }
    else if (tag == 0x4c) // time-shifted service descriptor
    {
      descriptorParseResult = DecodeTimeShiftedServiceDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  record.ReferenceServiceId);
    }
    else if (tag == 0x50) // component descriptor
    {
      bool isVideo;
      bool isAudio;
      bool isSubtitles;
      bool isHighDefinition;
      bool isStandardDefinition;
      bool isThreeDimensional;
      unsigned long language;
      descriptorParseResult = DecodeComponentDescriptor(&sectionData[pointer],
                                                        length,
                                                        isVideo,
                                                        isAudio,
                                                        isSubtitles,
                                                        isHighDefinition,
                                                        isStandardDefinition,
                                                        isThreeDimensional,
                                                        language);
      if (descriptorParseResult)
      {
        if (isVideo)
        {
          record.StreamCountVideo++;
        }
        else if (isAudio)
        {
          record.StreamCountAudio++;
          if (
            language != 0 &&
            find(record.AudioLanguages.begin(), record.AudioLanguages.end(), language) == record.AudioLanguages.end()
          )
          {
            record.AudioLanguages.push_back(language);
          }
        }
        else if (
          isSubtitles &&
          language != 0 &&
          find(record.SubtitlesLanguages.begin(), record.SubtitlesLanguages.end(), language) == record.SubtitlesLanguages.end()
        )
        {
          record.SubtitlesLanguages.push_back(language);
        }
        record.IsHighDefinition |= isHighDefinition;
        record.IsStandardDefinition |= isStandardDefinition;
        record.IsThreeDimensional |= isThreeDimensional;
      }
    }
    else if (tag == 0x5d) // multilingual service name descriptor
    {
      descriptorParseResult = DecodeMultilingualServiceNameDescriptor(&sectionData[pointer],
                                                                      length,
                                                                      record.ServiceNames,
                                                                      record.ProviderNames);
    }
    else if (tag == 0x5f) // private data specifier descriptor
    {
      descriptorParseResult = DecodePrivateDataSpecifierDescriptor(&sectionData[pointer],
                                                                    length,
                                                                    privateDataSpecifier);
    }
    else if (tag == 0x72) // service availability descriptor
    {
      descriptorParseResult = DecodeServiceAvailabilityDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  record.AvailableInCells,
                                                                  record.UnavailableInCells);
    }
    else if (tag == 0x73) // default authority descriptor
    {
      char* defaultAuthority = NULL;
      descriptorParseResult = DecodeDefaultAuthorityDescriptor(&sectionData[pointer],
                                                                length,
                                                                &defaultAuthority);
      if (descriptorParseResult && defaultAuthority != NULL)
      {
        if (record.DefaultAuthority != NULL)
        {
          if (strcmp(record.DefaultAuthority, defaultAuthority) != 0)
          {
            LogDebug(L"SDT: replacing default authority, table ID = 0x%hhx, TSID = %hu, ONID = %hu, service ID = %hu, current = %S, new = %S",
                      record.TableId, record.TransportStreamId,
                      record.OriginalNetworkId, record.ServiceId,
                      record.DefaultAuthority, defaultAuthority);
          }
          delete[] record.DefaultAuthority;
        }
        record.DefaultAuthority = defaultAuthority;
      }
    }
    else if (tag == 0x7f) // DVB extended descriptors
    {
      if (length < 1)
      {
        LogDebug(L"SDT: invalid service record, extended descriptor length = %hhu, pointer = %hu, end of descriptor loop = %hu, tag = 0x%hhx",
                  length, pointer, endOfDescriptorLoop, tag);
        return false;
      }

      unsigned char tagExtension = sectionData[pointer];
      if (tagExtension == 0x09)  // target region descriptor
      {
        descriptorParseResult = DecodeTargetRegionDescriptor(&sectionData[pointer],
                                                              length,
                                                              record.TargetRegionIds);
      }
      else if (tagExtension == 0x0b) // service relocated descriptor
      {
        descriptorParseResult = DecodeServiceRelocatedDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  record.PreviousOriginalNetworkId,
                                                                  record.PreviousTransportStreamId,
                                                                  record.PreviousServiceId);
      }
      else if (tagExtension == 0x10)  // video depth range descriptor
      {
        record.IsThreeDimensional = true;
      }
    }
    // There is no private data specifier for Dish descriptors, so we have to
    // determine scope with ONID. These are the ONIDs for EchoStar networks.
    // http://www.dvbservices.com/identifiers/original_network_id&tab=table
    else if (
      record.OriginalNetworkId >= ORIGINAL_NETWORK_ID_DISH_START &&
      record.OriginalNetworkId <= ORIGINAL_NETWORK_ID_DISH_END
    )
    {
      if (tag == 0x80)  // Dish Network channel descriptor
      {
        // This LCN overrides the service ID and is overriden by the
        // sub-channel descriptor.
        if (record.DishSubChannelNumber == 0)
        {
          descriptorParseResult = DecodeDishChannelDescriptor(&sectionData[pointer],
                                                              length,
                                                              record.DishMarketId,
                                                              record.LogicalChannelNumber);
        }
      }
      else if (tag == 0x9e) // Dish Network EPG Link descriptor
      {
        descriptorParseResult = DecodeDishEpgLinkDescriptor(&sectionData[pointer],
                                                            length,
                                                            record.EpgOriginalNetworkId,
                                                            record.EpgTransportStreamId,
                                                            record.EpgServiceId);
      }
      else if (tag == 0xa1) // Dish Network sub-channel descriptor
      {
        // This LCN always overrides all other Dish LCN sources.
        bool isHighDefinition = false;
        descriptorParseResult = DecodeDishSubChannelDescriptor(&sectionData[pointer],
                                                                length,
                                                                isHighDefinition,
                                                                record.LogicalChannelNumber,
                                                                record.DishSubChannelNumber);
        if (descriptorParseResult)
        {
          record.IsHighDefinition = isHighDefinition;
          record.IsStandardDefinition = !isHighDefinition;
        }
      }
    }
    else if (privateDataSpecifier == 2)
    {
      if (tag == 0xb2)  // OpenTV channel description descriptor
      {
        descriptorParseResult = DecodeOpenTvChannelDescriptionDescriptor(&sectionData[pointer],
                                                                          length,
                                                                          record.OpenTvCategoryId);
      }
      else if (tag == 0xc0) // OpenTV NVOD time-shifted service name descriptor
      {
        char* serviceName = NULL;
        descriptorParseResult = DecodeOpenTvNvodTimeShiftedServiceNameDescriptor(&sectionData[pointer],
                                                                                  length,
                                                                                  &serviceName);
        if (serviceName != NULL)
        {
          char* existingName = record.ServiceNames[LANG_UND];
          if (existingName != NULL)
          {
            if (strcmp(existingName, serviceName) != 0)
            {
              LogDebug(L"SDT: replacing NVOD time-shifted service name, table ID = 0x%hhx, TSID = %hu, ONID = %hu, service ID = %hu, current = %S, new = %S",
                        record.TableId, record.TransportStreamId,
                        record.OriginalNetworkId, record.ServiceId,
                        existingName, serviceName);
            }
            delete[] existingName;
          }
          record.ServiceNames[LANG_UND] = serviceName;
        }
      }
    }
    else if (tag == 0xca) // Virgin Media channel descriptor
    {
      // There is no private data specifier. Even the ONID may be generic
      // (assigned to "Cable and Wireless Communications". Take care.
      // http://www.dvbservices.com/identifiers/original_network_id&tab=table
      if (record.OriginalNetworkId == 0xf020)
      {
        bool isHighDefinition = false;
        descriptorParseResult = DecodeVirginMediaChannelDescriptor(&sectionData[pointer],
                                                                    length,
                                                                    record.LogicalChannelNumber,
                                                                    record.VisibleInGuide,
                                                                    record.VirginMediaCategoryId,
                                                                    isHighDefinition);
        if (descriptorParseResult && isHighDefinition)
        {
          record.IsHighDefinition = true;
        }
      }
    }

    if (!descriptorParseResult)
    {
      LogDebug(L"SDT: invalid service record descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptor loop = %hu, private data specifier = %lu",
                tag, length, pointer, endOfDescriptorLoop,
                privateDataSpecifier);
      return false;
    }

    pointer = endOfDescriptor;
  }

  pointer = endOfDescriptorLoop;
  return true;
}

bool CParserSdt::DecodeServiceDescriptor(unsigned char* data,
                                          unsigned char dataLength,
                                          unsigned char& serviceType,
                                          char** providerName,
                                          char** serviceName)
{
  if (dataLength < 3)
  {
    LogDebug(L"SDT: invalid service descriptor, length = %hhu", dataLength);
    return false;
  }
  try
  {
    serviceType = data[0];
    unsigned char serviceProviderNameLength = data[1];
    unsigned short pointer = 2;
    if (serviceProviderNameLength > 0)
    {
      if (
        pointer + serviceProviderNameLength + 1 > dataLength ||
        !CTextUtil::DvbTextToString(&data[pointer], serviceProviderNameLength, providerName)
      )
      {
        LogDebug(L"SDT: invalid service descriptor, descriptor length = %hhu, pointer = %hu, provider name length = %hhu",
                  dataLength, pointer, serviceProviderNameLength);
        return false;
      }
      if (*providerName == NULL)
      {
        LogDebug(L"SDT: failed to allocate a provider name");
      }
      pointer += serviceProviderNameLength;
    }

    unsigned char serviceNameLength = data[pointer++];
    if (serviceNameLength > 0)
    {
      if (
        pointer + serviceNameLength > dataLength ||
        !CTextUtil::DvbTextToString(&data[pointer], serviceNameLength, serviceName)
      )
      {
        LogDebug(L"SDT: invalid service descriptor, descriptor length = %hhu, pointer = %hu, provider name length = %hhu, name length = %hhu",
                  dataLength, pointer, serviceProviderNameLength,
                  serviceNameLength);
        if (*providerName != NULL)
        {
          delete[] *providerName;
          *providerName = NULL;
        }
        return false;
      }
      if (*serviceName == NULL)
      {
        LogDebug(L"SDT: failed to allocate a service name");
      }
    }

    //LogDebug(L"SDT: service descriptor, type = %hhu, provider = %S, name = %S",
    //          serviceType, *providerName == NULL ? "" : *providerName,
    //          *serviceName == NULL ? "" : *serviceName);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeServiceDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeCountryAvailabilityDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      vector<unsigned long>& availableInCountries,
                                                      vector<unsigned long>& unavailableInCountries)
{
  if (dataLength == 0 || (dataLength - 1) % 3 != 0)
  {
    LogDebug(L"SDT: invalid country availability descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    bool countryAvailabilityFlag = (data[pointer++] & 0x80) != 0;
    //LogDebug(L"SDT: country availability descriptor, availability flag = %d",
    //          countryAvailabilityFlag);
    while (pointer + 2 < dataLength)
    {
      unsigned long countryCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      //LogDebug(L"  %S", (char*)&countryCode);
      if (countryAvailabilityFlag)
      {
        availableInCountries.push_back(countryCode);
      }
      else
      {
        unavailableInCountries.push_back(countryCode);
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeCountryAvailabilityDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeTimeShiftedServiceDescriptor(unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned short& referenceServiceId)
{
  if (dataLength != 2)
  {
    LogDebug(L"SDT: invalid time-shifted service descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    referenceServiceId = (data[0] << 8) | data[1];
    //LogDebug(L"SDT: time-shifted service descriptor, reference service ID = %hu",
    //          referenceServiceId);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeTimeShiftedServiceDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeComponentDescriptor(unsigned char* data,
                                            unsigned char dataLength,
                                            bool& isVideo,
                                            bool& isAudio,
                                            bool& isSubtitles,
                                            bool& isHighDefinition,
                                            bool& isStandardDefinition,
                                            bool& isThreeDimensional,
                                            unsigned long& language)
{
  if (dataLength < 6)
  {
    LogDebug(L"SDT: invalid component descriptor, length = %hhu", dataLength);
    return false;
  }
  try
  {
    isVideo = false;
    isAudio = false;
    isSubtitles = false;
    isHighDefinition = false;
    isStandardDefinition = false;
    isThreeDimensional = false;

    unsigned char streamContentExt = data[0] >> 4;
    unsigned char streamContent = data[0] & 0x0f;
    unsigned char componentType = data[1];
    unsigned char componentTag = data[2];
    unsigned long iso639LanguageCode = data[3] | (data[4] << 8) | (data[5] << 16);
    // (component description not read)
    //LogDebug(L"SDT: component descriptor, stream content = %hhu, stream content extension = %hhu, component type = %hhu, component tag = %hhu, language = %S",
    //          streamContent, streamContentExt, componentType, componentTag,
    //          (char*)&iso639LanguageCode);

    if (streamContent == 1 || streamContent == 5 || (streamContent == 9 && streamContentExt == 0))
    {
      isVideo = true;
      if (streamContent != 9 && componentType >= 1 && componentType <= 8)
      {
        isStandardDefinition = true;
      }
      else if (streamContent == 9 || (componentType >= 0x09 && componentType <= 0x10))
      {
        isHighDefinition = true;
      }
      if (streamContent == 5)
      {
        if (componentType >= 0x80 && componentType <= 0x83)
        {
          // frame compatible plano-stereoscopic
          // 0x80/0x82 = side by side; 0x81/0x83 = top and bottom
          isHighDefinition = true;
          isThreeDimensional = true;
        }
        else if (componentType == 0x84)
        {
          // service compatible plano-stereoscopic
          isThreeDimensional = true;
        }
      }
    }
    else if (
      streamContent == 2 ||
      streamContent == 4 ||
      (streamContent == 6 && componentType != 0xa0) ||
      streamContent == 7 ||
      (streamContent == 9 && streamContentExt == 1)
    )
    {
      isAudio = true;
      language = iso639LanguageCode;
    }
    else if (
      streamContent == 3 &&
      (
        componentType = 1 ||  // EBU teletext subtitles
        (componentType >= 0x10 && componentType <= 0x15) || // DVB subtitles [normal]
        (componentType >= 0x20 && componentType <= 0x25)    // DVB subtitles [hard of hearing]
      )
    )
    {
      isSubtitles = true;
      language = iso639LanguageCode;
    }
    else if (streamContent == 0xb && streamContentExt == 0xf && componentType == 3)
    {
      // frame compatible plano-stereoscopic top and bottom
      isThreeDimensional = true;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeComponentDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeMultilingualServiceNameDescriptor(unsigned char* data,
                                                          unsigned char dataLength,
                                                          map<unsigned long, char*> serviceNames,
                                                          map<unsigned long, char*> providerNames)
{
  if (dataLength == 0)
  {
    LogDebug(L"SDT: invalid multilingual service name descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 4 < dataLength)
    {
      unsigned long iso639LanguageCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      unsigned char serviceProviderNameLength = data[pointer++];
      char* providerName = NULL;
      if (serviceProviderNameLength > 0)
      {
        if (
          pointer + serviceProviderNameLength + 1 > dataLength ||
          !CTextUtil::DvbTextToString(&data[pointer], serviceProviderNameLength, &providerName)
        )
        {
          LogDebug(L"SDT: invalid multilingual service name descriptor, descriptor length = %hhu, pointer = %hu, language = %S, provider name length = %hhu",
                    dataLength, pointer, (char*)&iso639LanguageCode,
                    serviceProviderNameLength);
          CUtils::CleanUpStringSet(providerNames);
          CUtils::CleanUpStringSet(serviceNames);
          return false;
        }
        if (providerName == NULL)
        {
          LogDebug(L"SDT: failed to allocate the %S provider name",
                    (char*)&iso639LanguageCode);
        }
        else
        {
          char* existingName = providerNames[iso639LanguageCode];
          if (existingName != NULL)
          {
            if (strcmp(existingName, providerName) != 0)
            {
              LogDebug(L"SDT: multilingual provider name conflict, language = %S, name = %S, alternative name = %S",
                        (char*)&iso639LanguageCode, existingName,
                        providerName);
            }
            delete[] providerName;
            providerName = NULL;
          }
          else
          {
            providerNames[iso639LanguageCode] = providerName;
          }
        }
        pointer += serviceProviderNameLength;
      }

      unsigned char serviceNameLength = data[pointer++];
      char* serviceName = NULL;
      if (serviceNameLength > 0)
      {
        if (
          pointer + serviceNameLength > dataLength ||
          !CTextUtil::DvbTextToString(&data[pointer], serviceNameLength, &serviceName)
        )
        {
          LogDebug(L"SDT: invalid multilingual service name descriptor, descriptor length = %hhu, pointer = %hu, language = %S, provider name length = %hhu, name length = %hhu",
                    dataLength, pointer, (char*)&iso639LanguageCode,
                    serviceProviderNameLength, serviceNameLength);
          if (providerName != NULL)
          {
            delete[] providerName;
            providerName = NULL;
          }
          CUtils::CleanUpStringSet(providerNames);
          CUtils::CleanUpStringSet(serviceNames);
          return false;
        }
        if (serviceName == NULL)
        {
          LogDebug(L"SDT: failed to allocate the %S service name",
                    (char*)&iso639LanguageCode);
        }
        else
        {
          char* existingName = serviceNames[iso639LanguageCode];
          if (existingName != NULL)
          {
            if (strcmp(existingName, serviceName) != 0)
            {
              LogDebug(L"SDT: multilingual service name conflict, language = %S, name = %S, alternative name = %S",
                        (char*)&iso639LanguageCode, existingName, serviceName);
            }
            delete[] serviceName;
            serviceName = NULL;
          }
          else
          {
            serviceNames[iso639LanguageCode] = serviceName;
          }
        }
        pointer += serviceNameLength;
      }

      //LogDebug(L"SDT: multilingual service name descriptor, language = %S, provider = %S, name = %S",
      //          iso639LanguageCode, providerName == NULL ? "" : providerName,
      //          serviceName == NULL ? "" : serviceName);
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeMultilingualServiceNameDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodePrivateDataSpecifierDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned long& privateDataSpecifier)
{
  if (dataLength != 4)
  {
    LogDebug(L"SDT: invalid private data specifier descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    privateDataSpecifier = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
    //LogDebug(L"SDT: private data specifier descriptor, specifier = %lu",
    //          privateDataSpecifier);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodePrivateDataSpecifierDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeServiceAvailabilityDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      vector<unsigned long>& availableInCells,
                                                      vector<unsigned long>& unavailableInCells)
{
  if (dataLength == 0 || (dataLength - 1) % 2 != 0)
  {
    LogDebug(L"SDT: invalid service availability descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    bool availabilityFlag = (data[pointer++] & 0x80) != 0;
    //LogDebug(L"SDT: service availability descriptor, availability flag = %d",
    //          availabilityFlag);
    while (pointer + 1 < dataLength)
    {
      unsigned long cellId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      //LogDebug(L"  %lu", cellId);
      if (availabilityFlag)
      {
        availableInCells.push_back(cellId << 8);  // << 8 to leave room for an extension ID, matches NIT
      }
      else
      {
        unavailableInCells.push_back(cellId << 8);
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeServiceAvailabilityDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeDefaultAuthorityDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  char** defaultAuthority)
{
  if (dataLength == 0)
  {
    LogDebug(L"SDT: invalid default authority descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    if (!CTextUtil::DvbTextToString(data, dataLength, defaultAuthority))
    {
      LogDebug(L"SDT: invalid default authority descriptor, descriptor length = %hhu",
                dataLength);
      return false;
    }
    if (*defaultAuthority == NULL)
    {
      LogDebug(L"SDT: failed to allocate a default authority");
    }
    else if (
      strncmp(*defaultAuthority, "crid://", 7) != 0 &&
      strncmp(*defaultAuthority, "CRID://", 7) != 0
    )
    {
      // Prepend the "crid://" part if necessary.
      unsigned char byteCount = 7 + strlen(*defaultAuthority) + 1;
      char* temp = new char[7 + strlen(*defaultAuthority) + 1];
      if (temp == NULL)
      {
        LogDebug(L"SDT: failed to allocate %hhu bytes for a fully qualified default authority",
                  byteCount);
      }
      else
      {
        strncpy(temp, "crid://", 8);
        strncpy(&temp[7], *defaultAuthority, byteCount - 7);
      }
      delete[] *defaultAuthority;
      *defaultAuthority = temp;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeDefaultAuthorityDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeTargetRegionDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              vector<unsigned long long>& targetRegionIds)
{
  if (dataLength < 4)
  {
    LogDebug(L"SDT: invalid target region descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned long countryCode = data[1] | (data[2] << 8) | (data[3] << 16);
    if (dataLength == 4)
    {
      //LogDebug(L"SDT: target region descriptor, country code = %S",
      //          (char*)&countryCode);
      targetRegionIds.push_back((unsigned long long)countryCode << 32);
      return true;
    }

    unsigned short pointer = 4;
    while (pointer < dataLength)
    {
      bool countryCodeFlag = (data[pointer] & 0x04) != 0;
      unsigned char regionDepth = data[pointer++] & 0x03;

      // How many bytes are we expecting in this loop?
      unsigned char byteCount = 0;
      if (countryCodeFlag)
      {
        byteCount += 3;
      }
      byteCount += regionDepth;
      if (regionDepth == 3)
      {
        byteCount++;
      }

      if (pointer + byteCount > dataLength)
      {
        LogDebug(L"SDT: invalid target region descriptor, length = %hhu, pointer = %hu, country code flag = %d, region depth = %hhu, country code = %S",
                  dataLength, pointer, countryCodeFlag, regionDepth,
                  (char*)&countryCode);
        return false;
      }

      if (countryCodeFlag)
      {
        countryCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
        pointer += 3;
      }

      unsigned long long targetRegionId = (unsigned long long)countryCode << 32;
      if (regionDepth > 0)
      {
        targetRegionId |= (data[pointer++] << 24);
        if (regionDepth > 1)
        {
          targetRegionId |= (data[pointer++] << 16);
          if (regionDepth > 2)
          {
            targetRegionId |= (data[pointer] << 8) | data[pointer + 1];
            pointer += 2;
          }
        }
      }

      //LogDebug(L"SDT: target region descriptor, country code = %S, ID = %llu",
      //          (char*)&countryCode, targetRegionId);
      targetRegionIds.push_back(targetRegionId);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeTargetRegionDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeServiceRelocatedDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned short& previousOriginalNetworkId,
                                                  unsigned short& previousTransportStreamId,
                                                  unsigned short& previousServiceId)
{
  if (dataLength != 7)
  {
    LogDebug(L"SDT: invalid service relocated descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    previousOriginalNetworkId = (data[1] << 8) | data[2];
    previousTransportStreamId = (data[3] << 8) | data[4];
    previousServiceId = (data[5] << 8) | data[6];
    //LogDebug(L"SDT: service relocated descriptor, ONID = %hu, TSID = %hu, service ID = %hu",
    //          previousOriginalNetworkId, previousTransportStreamId,
    //          previousServiceId);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeServiceRelocatedDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeDishChannelDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned short& marketId,
                                              unsigned short& logicalChannelNumber)
{
  // market ID - 2 bytes; 0x0ffa = standard channel (non-regional)
  // logical channel number - 2 bytes
  // if (???)
  //   last byte - 1 byte; relates to "mapdowns"
  if (dataLength < 4 || dataLength > 5)
  {
    LogDebug(L"SDT: invalid Dish channel description descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    marketId = (data[0] << 8) | data[1];
    logicalChannelNumber = (data[2] << 8) | data[3];
    unsigned char lastByte = 0;
    if (dataLength == 5)
    {
      lastByte = data[4];
    }

    //LogDebug(L"SDT: Dish channel descriptor, market ID = %hu, LCN = %hu, last byte = %hhu",
    //          marketId, logicalChannelNumber, lastByte);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeDishChannelDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeDishEpgLinkDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned short& originalNetworkId,
                                              unsigned short& transportStreamId,
                                              unsigned short& serviceId)
{
  // original network ID - 2 bytes
  // transport stream ID - 2 bytes
  // service ID - 2 bytes
  // identifier - 2 bytes; links to first 2 bytes in 0x93 descriptor, usually 0
  if (dataLength != 8)
  {
    LogDebug(L"SDT: invalid Dish EPG link descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    originalNetworkId = (data[0] << 8) | data[1];
    transportStreamId = (data[2] << 8) | data[3];
    serviceId = (data[4] << 8) | data[5];
    unsigned short identifier = (data[6] << 8) | data[7];

    //LogDebug(L"SDT: Dish EPG link descriptor, ONID = %hu, TSID = %hu, service ID = %hu, identifier = %hu",
    //          originalNetworkId, transportStreamId, serviceId, identifier);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeDishEpgLinkDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeDishSubChannelDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                bool& isHighDefinition,
                                                unsigned short& majorChannelNumber,
                                                unsigned char& minorChannelNumber)
{
  // is high definition - 1 bit
  // major channel number - 15 bits
  // minor channel number - 7 bits
  // reserved - 9 bits; always 0x1FF
  if (dataLength != 4)
  {
    LogDebug(L"SDT: invalid Dish sub-channel descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    isHighDefinition = (data[0] & 0x80) != 0;
    majorChannelNumber = ((data[0] & 0x7f) << 8) | data[1];
    minorChannelNumber = data[2] >> 1;
    unsigned short reserved = ((data[2] & 1) << 8) | data[3];

    //LogDebug(L"SDT: Dish sub-channel descriptor, is HD = %d, major channel number = %hu, minor channel number = %hhu, reserved = %hu",
    //          isHighDefinition, majorChannelNumber, minorChannelNumber,
    //          reserved);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeDishSubChannelDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeOpenTvChannelDescriptionDescriptor(unsigned char* data,
                                                          unsigned char dataLength,
                                                          unsigned char& categoryId)
{
  // channel type - 1 byte
  // flags - 1 byte; AU, NZ = always 1, UK = varies, pattern not obvious
  // category ID / unknown - 4 bits
  // following byte count - 4 bits
  // if (following byte count != 0) {
  //   category ID - 1 byte; country-specific interpretation
  //   unknown - [following byte count - 1] bytes
  // }
  // channel description - [remaining] bytes; Huffman encoded
  if (dataLength < 3)
  {
    LogDebug(L"SDT: invalid OpenTV channel description descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char channelType = data[0];
    unsigned char flags = data[1];
    unsigned char categoryIdOrUnknown = data[2];
    unsigned char followingByteCount = categoryIdOrUnknown & 0xf;
    if (followingByteCount == 0)
    {
      categoryId = categoryIdOrUnknown;
    }
    else
    {
      if (dataLength < 3 + followingByteCount)
      {
        LogDebug(L"SDT: invalid OpenTV channel description descriptor, length = %hhu, channel type = %hhu, flags = 0x%hhx, category ID/unknown = %hhu",
                  dataLength, channelType, flags, categoryIdOrUnknown);
        return false;
      }
      categoryId = data[3];
    }

    //LogDebug(L"SDT: OpenTV channel description descriptor, channel type = %hhu, flags = 0x%hhx, category ID/unknown = %hhu, category ID = %hhu",
    //          channelType, flags, categoryIdOrUnknown, categoryId);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeOpenTvChannelDescriptionDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeOpenTvNvodTimeShiftedServiceNameDescriptor(unsigned char* data,
                                                                  unsigned char dataLength,
                                                                  char** serviceName)
{
  if (dataLength == 0)
  {
    LogDebug(L"SDT: invalid OpenTV NVOD time-shifted service name descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    if (!CTextUtil::DvbTextToString(data, dataLength, serviceName))
    {
      LogDebug(L"SDT: invalid OpenTV NVOD time-shifted service name descriptor, length = %hhu",
                dataLength);
      return false;
    }
    if (*serviceName == NULL)
    {
      LogDebug(L"SDT: failed to allocate an NVOD time-shifted service name");
    }

    //LogDebug(L"SDT: OpenTV NVOD time-shifted service name descriptor, name = %S",
    //          *serviceName == NULL ? "" : *serviceName);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeOpenTvNvodTimeShiftedServiceNameDescriptor()");
  }
  return false;
}

bool CParserSdt::DecodeVirginMediaChannelDescriptor(unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned short& logicalChannelNumber,
                                                    bool& visibleInGuide,
                                                    unsigned char& categoryId,
                                                    bool& isHighDefinition)
{
  // logical channel number - 2 bytes
  // name length - 1 byte
  // name - [name length] bytes; ASCII, abbreviated???
  // category ID = 1 byte
  // flags 2 - 1 byte
  // unique - 2 bytes; channel ID???
  // zero - 1 byte
  // flags 2 - 1 bytes
  if (dataLength < 9)
  {
    LogDebug(L"SDT: invalid Virgin Media channel descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    logicalChannelNumber = (data[0] << 8) | data[1];
    unsigned char nameLength = data[2];
    if (dataLength < 9 + nameLength)
    {
      LogDebug(L"SDT: invalid Virgin Media channel descriptor, descriptor length = %hhu, name length = %hhu",
                dataLength, nameLength);
      return false;
    }

    unsigned short pointer = nameLength + 2;

    // 0 = no category
    // 1 = factual
    // 2 = entertainment
    // 3 = international
    // 4 = radio
    // 5 = kids
    // 6 = lifestyle
    // 7 = movies
    // 8 = music
    // 9 = news
    // 10 = sport
    // 11 = "live events channel"
    // 12 = adult
    // 13 = shopping
    // 14 = "Virgin iD"
    // 15 = "Top Left 4KTV test", "Bot Left 4KTV test"
    categoryId = data[pointer++];

    // b0 (MSB) = [always zero]
    // b1 = [always zero]
    // b2 = is NVOD time-shifted
    // b3 = is not NVOD time-shifted
    // b4 = hide in guide
    // b5 = is Sky channel???
    // b6 = [always zero]
    // b7 (LSB) = is high definition
    unsigned char flags1 = data[pointer++];
    visibleInGuide = (flags1 & 8) == 0;
    isHighDefinition = (flags1 & 1) != 0;

    unsigned short channelId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    unsigned char zero = data[pointer++];
    unsigned char flags2 = data[pointer++];

    //LogDebug(L"SDT: Virgin Media channel descriptor, LCN = %hu, name length = %hhu, category ID = %hhu, flags 1 = 0x%hhx, channel ID = %hu, zero = %hhu, flags 2 = 0x%hhx",
    //          logicalChannelNumber, nameLength, categoryId, flags1, channelId,
    //          zero, flags2);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SDT: unhandled exception in DecodeVirginMediaChannelDescriptor()");
  }
  return false;
}