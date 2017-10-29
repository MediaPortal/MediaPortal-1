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
#include "ParserNtt.h"
#include <algorithm>    // find()
#include <cstring>      // strlen(), strncpy()
#include <map>
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\TimeUtils.h"
#include "TextUtil.h"
#include "Utils.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserNtt::CParserNtt()
  : m_recordsTransponderName(600000), m_recordsSatelliteText(600000),
    m_recordsRatingsText(600000), m_recordsRatingSystem(600000),
    m_recordsSourceName(600000), m_recordsMapName(600000),
    m_recordsCurrencySystem(600000)
{
  m_isReady = false;
  m_completeTimeSnt = 0;
  m_tableSubtype6Interpretation = 0;
  SetCallBack(NULL);
}

CParserNtt::~CParserNtt()
{
  SetCallBack(NULL);
}

void CParserNtt::Reset()
{
  LogDebug(L"NTT: reset");
  CEnterCriticalSection lock(m_section);
  m_recordsTransponderName.RemoveAllRecords();
  m_recordsSatelliteText.RemoveAllRecords();
  m_recordsRatingsText.RemoveAllRecords();
  m_recordsRatingSystem.RemoveAllRecords();
  m_recordsSourceName.RemoveAllRecords();
  m_recordsMapName.RemoveAllRecords();
  m_recordsCurrencySystem.RemoveAllRecords();
  m_seenSections.clear();
  m_unseenSections.clear();
  m_isReady = false;
  m_completeTimeSnt = 0;
  m_tableSubtype6Interpretation = 0;
  LogDebug(L"NTT: reset done");
}

void CParserNtt::SetCallBack(ICallBackNtt* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserNtt::OnNewSection(const CSection& section)
{
  try
  {
    unsigned char protocolVersion;
    unsigned long iso639LanguageCode;
    unsigned char transmissionMedium;
    unsigned char tableSubtype;
    unsigned char satelliteId;
    unsigned char ratingRegion;
    vector<CRecordNtt*> records;
    bool seenRevisionDescriptor;
    unsigned char tableVersionNumber;
    unsigned char sectionNumber;
    unsigned char lastSectionNumber;
    CEnterCriticalSection lock(m_section);
    if (!DecodeSection(section,
                        m_tableSubtype6Interpretation,
                        protocolVersion,
                        iso639LanguageCode,
                        transmissionMedium,
                        tableSubtype,
                        satelliteId,
                        ratingRegion,
                        records,
                        seenRevisionDescriptor,
                        tableVersionNumber,
                        sectionNumber,
                        lastSectionNumber))
    {
      if (seenRevisionDescriptor)
      {
        LogDebug(L"NTT: invalid section, protocol version = %hhu, language = %S, transmission medium = %hhu, table sub-type = %hhu, satellite ID = %hhu, rating region = %hhu, version number = %hhu, section number = %hhu",
                  protocolVersion, (char*)&iso639LanguageCode,
                  transmissionMedium, tableSubtype, satelliteId, ratingRegion,
                  tableVersionNumber, sectionNumber);
      }
      else
      {
        LogDebug(L"NTT: invalid section, protocol version = %hhu, language = %S, transmission medium = %hhu, table sub-type = %hhu, satellite ID = %hhu, rating region = %hhu",
                  protocolVersion, (char*)&iso639LanguageCode,
                  transmissionMedium, tableSubtype, satelliteId, ratingRegion);
      }
      return;
    }

    CRecordStore* recordSet;
    if (tableSubtype == 1)
    {
      recordSet = &m_recordsTransponderName;
    }
    else if (tableSubtype == 2)
    {
      recordSet = &m_recordsSatelliteText;
    }
    else if (tableSubtype == 3)
    {
      recordSet = &m_recordsRatingsText;
    }
    else if (tableSubtype == 4)
    {
      recordSet = &m_recordsRatingSystem;
    }
    else if (tableSubtype == 5)
    {
      recordSet = &m_recordsSourceName;
    }
    else if (tableSubtype == 6)
    {
      recordSet = &m_recordsMapName;
    }
    else
    {
      recordSet = &m_recordsCurrencySystem;
    }

    unsigned long sectionGroupKey;
    if (!seenRevisionDescriptor)
    {
      // Assume that there is one section for each sub-table type, except for:
      // - transponder name which has one section for each satellite
      // - ratings text which has one section for each region
      // - source name which can have multiple sections depending on the number
      //    of sources
      if (recordSet->GetRecordCount() == 0)
      {
        LogDebug(L"NTT: received, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu",
                    protocolVersion, transmissionMedium, tableSubtype);
      }
      if (tableSubtype == 1)
      {
        recordSet->MarkExpiredRecords((transmissionMedium << 8) | satelliteId);
      }
      else if (tableSubtype == 3)
      {
        recordSet->MarkExpiredRecords((transmissionMedium << 8) | ratingRegion);
      }
      else if (tableSubtype != 5)
      {
        recordSet->MarkExpiredRecords(transmissionMedium);
      }
    }
    else
    {
      // Have we seen this section before?
      unsigned long sectionKey = (tableSubtype << 16) | (tableVersionNumber << 8) | sectionNumber;
      unsigned long sectionGroupMask = 0xffff0000;
      sectionGroupKey = sectionKey & sectionGroupMask;
      vector<unsigned long>::const_iterator sectionIt = find(m_seenSections.begin(),
                                                              m_seenSections.end(),
                                                              sectionKey);
      if (sectionIt != m_seenSections.end())
      {
        // Yes.
        //LogDebug(L"NTT: previously seen section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, section number = %hhu",
        //          protocolVersion, transmissionMedium, tableSubtype,
        //          sectionNumber);
        CleanUpRecords(records);
        return;
      }

      // Were we expecting this section?
      sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
      if (sectionIt == m_unseenSections.end())
      {
        // No. Is this a change/update, or just a new section group?
        bool isChange = m_isReady;
        vector<unsigned long>::const_iterator tempSectionIt = m_unseenSections.begin();
        while (tempSectionIt != m_unseenSections.end())
        {
          if ((*tempSectionIt & sectionGroupMask) == sectionGroupKey)
          {
            isChange = true;
            tempSectionIt = m_unseenSections.erase(tempSectionIt);
          }
          else
          {
            tempSectionIt++;
          }
        }

        tempSectionIt = m_seenSections.begin();
        while (tempSectionIt != m_seenSections.end())
        {
          if ((*tempSectionIt & sectionGroupMask) == sectionGroupKey)
          {
            isChange = true;
            tempSectionIt = m_seenSections.erase(tempSectionIt);
          }
          else
          {
            tempSectionIt++;
          }
        }

        if (!isChange)
        {
          LogDebug(L"NTT: received, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                    protocolVersion, transmissionMedium, tableSubtype,
                    tableVersionNumber, sectionNumber, lastSectionNumber);
        }
        else
        {
          LogDebug(L"NTT: changed, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                    protocolVersion, transmissionMedium, tableSubtype,
                    tableVersionNumber, sectionNumber, lastSectionNumber);
          if (tableSubtype == 1)
          {
            recordSet->MarkExpiredRecords((transmissionMedium << 8) | satelliteId);
          }
          else if (tableSubtype == 3)
          {
            recordSet->MarkExpiredRecords((transmissionMedium << 8) | ratingRegion);
          }
          else
          {
            recordSet->MarkExpiredRecords(transmissionMedium);
          }
          if (m_isReady)
          {
            m_isReady = false;
            if (m_callBack != NULL)
            {
              m_callBack->OnTableChange(TABLE_ID_NTT);
            }
          }
        }

        unsigned long baseKey = sectionKey & 0xffffff00;
        for (unsigned char s = 0; s <= lastSectionNumber; s++)
        {
          m_unseenSections.push_back(baseKey + s);
        }
        sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
      }
      else
      {
        //LogDebug(L"NTT: new section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, version number = %hhu, section number = %hhu",
        //          protocolVersion, transmissionMedium, tableSubtype,
        //          tableVersionNumber, sectionNumber);
      }

      m_seenSections.push_back(sectionKey);
      m_unseenSections.erase(sectionIt);
    }

    if (
      m_callBack != NULL &&
      m_recordsTransponderName.GetRecordCount() == 0 &&
      m_recordsSatelliteText.GetRecordCount() == 0 &&
      m_recordsRatingsText.GetRecordCount() == 0 &&
      m_recordsRatingSystem.GetRecordCount() == 0 &&
      m_recordsSourceName.GetRecordCount() == 0 &&
      m_recordsMapName.GetRecordCount() == 0 &&
      m_recordsCurrencySystem.GetRecordCount() == 0
    )
    {
      m_callBack->OnTableSeen(TABLE_ID_NTT);
    }

    unsigned long newChangedOrExpiredRecordCount = 0;
    vector<CRecordNtt*>::const_iterator recordIt = records.begin();
    for ( ; recordIt != records.end(); recordIt++)
    {
      CRecordNtt* record = *recordIt;
      if (recordSet->AddOrUpdateRecord((IRecord**)&record, m_callBack))
      {
        newChangedOrExpiredRecordCount++;
      }
    }
    if (m_seenSections.size() == 0 && m_unseenSections.size() == 0)
    {
      if (tableSubtype != 5)
      {
        newChangedOrExpiredRecordCount += recordSet->RemoveExpiredRecords(m_callBack);
      }
      else if (newChangedOrExpiredRecordCount > 0)
      {
        m_completeTimeSnt = clock();
      }
    }

    // Are we ready?
    if (IsReadyPrivate(transmissionMedium))
    {
      if (m_seenSections.size() > 0 || tableSubtype == 5)
      {
        newChangedOrExpiredRecordCount += recordSet->RemoveExpiredRecords(m_callBack);
      }

      // Did something actually change?
      if (m_seenSections.size() > 0 || newChangedOrExpiredRecordCount > 0)
      {
        LogDebug(L"NTT: ready, sections parsed = %llu, transponder name count = %lu, satellite text count = %lu, ratings text count = %lu, rating system count = %lu, source name count = %lu, map name count = %lu, currency system count = %lu",
                  (unsigned long long)m_seenSections.size(),
                  m_recordsTransponderName.GetRecordCount(),
                  m_recordsSatelliteText.GetRecordCount(),
                  m_recordsRatingsText.GetRecordCount(),
                  m_recordsRatingSystem.GetRecordCount(),
                  m_recordsSourceName.GetRecordCount(),
                  m_recordsMapName.GetRecordCount(),
                  m_recordsCurrencySystem.GetRecordCount());
        m_isReady = true;
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_NTT);
        }
      }
    }
    else if (m_isReady)
    {
      LogDebug(L"NTT: changed, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu",
                protocolVersion, transmissionMedium, tableSubtype);
      m_isReady = false;
      if (m_callBack != NULL)
      {
        m_callBack->OnTableChange(TABLE_ID_NTT);
      }
    }
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in OnNewSection()");
  }
}

bool CParserNtt::IsSeen() const
{
  return
    m_seenSections.size() > 0 ||
    m_recordsTransponderName.GetRecordCount() > 0 ||
    m_recordsSatelliteText.GetRecordCount() > 0 ||
    m_recordsRatingsText.GetRecordCount() > 0 ||
    m_recordsRatingSystem.GetRecordCount() > 0 ||
    m_recordsSourceName.GetRecordCount() > 0 ||
    m_recordsMapName.GetRecordCount() > 0 ||
    m_recordsCurrencySystem.GetRecordCount() > 0;
}

bool CParserNtt::IsReady() const
{
  return m_isReady;
}

bool CParserNtt::GetTransponderName(unsigned char transmissionMedium,
                                    unsigned char satelliteId,
                                    unsigned char transponderNumber,
                                    unsigned long& language,
                                    char* name,
                                    unsigned short& nameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsTransponderName.GetRecordByKey((transmissionMedium << 16) | (satelliteId << 8) | transponderNumber, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NTT: invalid transponder name identifiers, transmission medium = %hhu, satellite ID = %hhu, transponder number = %hhu",
              transmissionMedium, satelliteId, transponderNumber);
    return false;
  }

  CRecordNttTransponderName* recordNtt = dynamic_cast<CRecordNttTransponderName*>(record);
  if (recordNtt == NULL)
  {
    LogDebug(L"NTT: invalid transponder name record, transmission medium = %hhu, satellite ID = %hhu, transponder number = %hhu",
              transmissionMedium, satelliteId, transponderNumber);
    return false;
  }

  language = recordNtt->Language;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordNtt->Name, name, nameBufferSize, requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient transponder name buffer size, transmission medium = %hhu, satellite ID = %hhu, transponder number = %hhu, required size = %hu, actual size = %hu",
              transmissionMedium, satelliteId, transponderNumber,
              requiredBufferSize, nameBufferSize);
  }
  return true;
}

bool CParserNtt::GetSatelliteText(unsigned char transmissionMedium,
                                  unsigned char satelliteId,
                                  unsigned long& language,
                                  char* referenceName,
                                  unsigned short& referenceNameBufferSize,
                                  char* fullName,
                                  unsigned short& fullNameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsSatelliteText.GetRecordByKey((transmissionMedium << 8) | satelliteId, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NTT: invalid satellite text identifiers, transmission medium = %hhu, satellite ID = %hhu",
              transmissionMedium, satelliteId);
    return false;
  }

  CRecordNttSatelliteText* recordNtt = dynamic_cast<CRecordNttSatelliteText*>(record);
  if (recordNtt == NULL)
  {
    LogDebug(L"NTT: invalid satellite text record, transmission medium = %hhu, satellite ID = %hhu",
              transmissionMedium, satelliteId);
    return false;
  }

  language = recordNtt->Language;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordNtt->ReferenceName,
                                  referenceName,
                                  referenceNameBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient satellite reference name buffer size, transmission medium = %hhu, satellite ID = %hhu, required size = %hu, actual size = %hu",
              transmissionMedium, satelliteId, requiredBufferSize,
              referenceNameBufferSize);
  }
  if (!CUtils::CopyStringToBuffer(recordNtt->Name,
                                  fullName,
                                  fullNameBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient satellite full name buffer size, transmission medium = %hhu, satellite ID = %hhu, required size = %hu, actual size = %hu",
              transmissionMedium, satelliteId, requiredBufferSize,
              fullNameBufferSize);
  }
  return true;
}

bool CParserNtt::GetRatingsText(unsigned char transmissionMedium,
                                unsigned char ratingRegion,
                                unsigned char dimensionIndex,
                                unsigned char levelIndex,
                                unsigned long& language,
                                char* dimensionName,
                                unsigned short& dimensionNameBufferSize,
                                char* levelName,
                                unsigned short& levelNameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsRatingsText.GetRecordByKey((transmissionMedium << 24) | (ratingRegion << 16) | (dimensionIndex << 8) | levelIndex, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NTT: invalid ratings text identifiers, transmission medium = %hhu, rating region = %hhu, dimension index = %hhu, level index = %hhu",
              transmissionMedium, ratingRegion, dimensionIndex, levelIndex);
    return false;
  }

  CRecordNttRatingsText* recordNtt = dynamic_cast<CRecordNttRatingsText*>(record);
  if (recordNtt == NULL)
  {
    LogDebug(L"NTT: invalid ratings text record, transmission medium = %hhu, rating region = %hhu, dimension index = %hhu, level index = %hhu",
              transmissionMedium, ratingRegion, dimensionIndex, levelIndex);
    return false;
  }

  language = recordNtt->Language;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordNtt->DimensionName,
                                  dimensionName,
                                  dimensionNameBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient ratings dimension name buffer size, transmission medium = %hhu, rating region = %hhu, dimension index = %hhu, level index = %hhu, required size = %hu, actual size = %hu",
              transmissionMedium, ratingRegion, dimensionIndex, levelIndex,
              requiredBufferSize, dimensionNameBufferSize);
  }
  if (!CUtils::CopyStringToBuffer(recordNtt->LevelName,
                                  levelName,
                                  levelNameBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient ratings level name buffer size, transmission medium = %hhu, rating region = %hhu, dimension index = %hhu, level index = %hhu, required size = %hu, actual size = %hu",
              transmissionMedium, ratingRegion, dimensionIndex, levelIndex,
              requiredBufferSize, levelNameBufferSize);
  }
  return true;
}

bool CParserNtt::GetRatingSystem(unsigned char transmissionMedium,
                                  unsigned char ratingRegion,
                                  unsigned long& language,
                                  char* name,
                                  unsigned short& nameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsRatingSystem.GetRecordByKey((transmissionMedium << 8) | ratingRegion, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NTT: invalid rating system identifiers, transmission medium = %hhu, rating region = %hhu",
              transmissionMedium, ratingRegion);
    return false;
  }

  CRecordNttRatingSystem* recordNtt = dynamic_cast<CRecordNttRatingSystem*>(record);
  if (recordNtt == NULL)
  {
    LogDebug(L"NTT: invalid rating system record, transmission medium = %hhu, rating region = %hhu",
              transmissionMedium, ratingRegion);
    return false;
  }

  language = recordNtt->Language;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordNtt->Name, name, nameBufferSize, requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient rating system name buffer size, transmission medium = %hhu, rating region = %hhu, required size = %hu, actual size = %hu",
              transmissionMedium, ratingRegion, requiredBufferSize,
              nameBufferSize);
  }
  return true;
}

bool CParserNtt::GetSourceName(unsigned char transmissionMedium,
                                bool applicationType,
                                unsigned short sourceId,
                                unsigned long& language,
                                char* name,
                                unsigned short& nameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsSourceName.GetRecordByKey((transmissionMedium << 24) | (applicationType << 16) | sourceId, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NTT: invalid source name identifiers, transmission medium = %hhu, application type = %d, source ID = %hu",
              transmissionMedium, applicationType, sourceId);
    return false;
  }

  CRecordNttSourceName* recordNtt = dynamic_cast<CRecordNttSourceName*>(record);
  if (recordNtt == NULL)
  {
    LogDebug(L"NTT: invalid source name record, transmission medium = %hhu, application type = %d, source ID = %hu",
              transmissionMedium, applicationType, sourceId);
    return false;
  }

  language = recordNtt->Language;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordNtt->Name, name, nameBufferSize, requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient source name buffer size, transmission medium = %hhu, application type = %d, source ID = %hu, required size = %hu, actual size = %hu",
              transmissionMedium, applicationType, sourceId,
              requiredBufferSize, nameBufferSize);
  }
  return true;
}

bool CParserNtt::GetMapName(unsigned char transmissionMedium,
                            unsigned short vctId,
                            unsigned long& language,
                            char* name,
                            unsigned short& nameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsMapName.GetRecordByKey((transmissionMedium << 16) | vctId, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NTT: invalid map name identifiers, transmission medium = %hhu, VCT ID = %hu",
              transmissionMedium, vctId);
    return false;
  }

  CRecordNttMapName* recordNtt = dynamic_cast<CRecordNttMapName*>(record);
  if (recordNtt == NULL)
  {
    LogDebug(L"NTT: invalid map name record, transmission medium = %hhu, VCT ID = %hu",
              transmissionMedium, vctId);
    return false;
  }

  language = recordNtt->Language;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordNtt->Name, name, nameBufferSize, requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient map name buffer size, transmission medium = %hhu, VCT ID = %hu, required size = %hu, actual size = %hu",
              transmissionMedium, vctId, requiredBufferSize, nameBufferSize);
  }
  return true;
}

bool CParserNtt::GetCurrencySystem(unsigned char transmissionMedium,
                                    unsigned char currencyRegion,
                                    unsigned long& language,
                                    char* name,
                                    unsigned short& nameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsCurrencySystem.GetRecordByKey((transmissionMedium << 8) | currencyRegion, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NTT: invalid currency system identifiers, transmission medium = %hhu, currency region = %hhu",
              transmissionMedium, currencyRegion);
    return false;
  }

  CRecordNttCurrencySystem* recordNtt = dynamic_cast<CRecordNttCurrencySystem*>(record);
  if (recordNtt == NULL)
  {
    LogDebug(L"NTT: invalid currency system record, transmission medium = %hhu, currency region = %hhu",
              transmissionMedium, currencyRegion);
    return false;
  }

  language = recordNtt->Language;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordNtt->Name, name, nameBufferSize, requiredBufferSize))
  {
    LogDebug(L"NTT: insufficient currency system name buffer size, transmission medium = %hhu, currency region = %hhu, required size = %hu, actual size = %hu",
              transmissionMedium, currencyRegion, requiredBufferSize,
              nameBufferSize);
  }
  return true;
}

bool CParserNtt::IsReadyPrivate(unsigned char transmissionMedium) const
{
  // We always expect SNT, though the number of sections may vary.
  // RTT and RST are optional, but should be correlated if present (one set of
  // RTT per RST region).
  // Same for STT and TNT - one set of TNT per satellite - but we only expect
  // them for satellite broadcasts.
  // MNT and CST are also optional. If present, we only expect one section each.
  unsigned long recordCountSt = m_recordsSatelliteText.GetRecordCount();
  unsigned long recordCountTn = m_recordsTransponderName.GetRecordCount();
  unsigned long recordCountRt = m_recordsRatingsText.GetRecordCount();
  unsigned long recordCountRs = m_recordsRatingSystem.GetRecordCount();
  if (
    m_unseenSections.size() != 0 ||
    m_recordsSourceName.GetRecordCount() == 0 ||
    (recordCountSt == 0 && recordCountTn != 0) ||
    (recordCountSt != 0 && recordCountTn == 0) ||
    (recordCountRt == 0 && recordCountRs != 0) ||
    (recordCountRt != 0 && recordCountRs == 0)
  )
  {
    return false;
  }

  if (m_seenSections.size() > 0)
  {
    return true;
  }

  // Revision detection descriptors not used.
  if (CTimeUtils::ElapsedMillis(m_completeTimeSnt) < 30000)
  {
    // Wait at least 30 seconds for SNT. We have no other way to independently
    // verify that we've seen the entire SNT.
    return false;
  }

  // Check that we have seen all the satellite transponder names that we expect.
  map<unsigned char, bool> satellites;
  for (unsigned long i = 0; i < recordCountSt; i++)
  {
    IRecord* record = NULL;
    if (m_recordsSatelliteText.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordNttSatelliteText* recordNtt = dynamic_cast<CRecordNttSatelliteText*>(record);
      if (recordNtt != NULL && recordNtt->LastSeen != 0)
      {
        satellites[recordNtt->SatelliteId] = false;
      }
    }
  }

  for (unsigned long i = 0; i < recordCountTn; i++)
  {
    IRecord* record = NULL;
    if (m_recordsTransponderName.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordNttTransponderName* recordNtt = dynamic_cast<CRecordNttTransponderName*>(record);
      if (recordNtt != NULL && recordNtt->LastSeen != 0)
      {
        if (satellites.find(recordNtt->SatelliteId) == satellites.end())
        {
          return false;
        }
        satellites[recordNtt->SatelliteId] = true;
      }
    }
  }

  map<unsigned char, bool>::const_iterator it = satellites.begin();
  for ( ; it != satellites.end(); it++)
  {
    if (!it->second)
    {
      return false;
    }
  }

  // Check that we have seen all the rating region names that we expect.
  map<unsigned char, bool> ratingRegions;
  for (unsigned long i = 0; i < recordCountRt; i++)
  {
    IRecord* record = NULL;
    if (m_recordsRatingsText.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordNttRatingsText* recordNtt = dynamic_cast<CRecordNttRatingsText*>(record);
      if (recordNtt != NULL && recordNtt->LastSeen != 0)
      {
        ratingRegions[recordNtt->RatingRegion] = false;
      }
    }
  }

  for (unsigned long i = 0; i < recordCountRs; i++)
  {
    IRecord* record = NULL;
    if (m_recordsRatingSystem.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordNttRatingSystem* recordNtt = dynamic_cast<CRecordNttRatingSystem*>(record);
      if (recordNtt != NULL && recordNtt->LastSeen != 0)
      {
        if (ratingRegions.find(recordNtt->RatingRegion) == ratingRegions.end())
        {
          return false;
        }
        ratingRegions[recordNtt->RatingRegion] = true;
      }
    }
  }

  for (it = ratingRegions.begin(); it != ratingRegions.end(); it++)
  {
    if (!it->second)
    {
      return false;
    }
  }

  return true;
}

template<class T> void CParserNtt::CleanUpRecords(vector<T*>& records)
{
  vector<T*>::iterator it = records.begin();
  for ( ; it != records.end(); it++)
  {
    T* record = *it;
    if (record != NULL)
    {
      delete record;
      *it = NULL;
    }
  }
  records.clear();
}

bool CParserNtt::DecodeSection(const CSection& section,
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
                                unsigned char& lastSectionNumber)
{
  try
  {
    if (
      section.TableId != TABLE_ID_NTT ||
      section.SectionSyntaxIndicator ||
      section.PrivateIndicator
    )
    {
      return false;
    }
    if (section.SectionLength < 9)
    {
      LogDebug(L"NTT: invalid section, length = %hu", section.SectionLength);
      return false;
    }

    const unsigned char* data = section.Data;
    protocolVersion = data[3] & 0x1f;
    if (protocolVersion != 0)
    {
      LogDebug(L"NTT: unsupported protocol version");
      return false;
    }

    iso639LanguageCode = data[4] | (data[5] << 8) | (data[6] << 16);
    transmissionMedium = data[7] >> 4;
    tableSubtype = data[7] & 0xf;
    //LogDebug(L"NTT: protocol version = %hhu, section length = %hu, language = %S, transmission medium = %hhu, table sub-type = %hhu",
    //          protocolVersion, section.SectionLength,
    //          (char*)&iso639LanguageCode, transmissionMedium, tableSubtype);
    if (tableSubtype == 0 || tableSubtype > 7)
    {
      LogDebug(L"NTT: unsupported table sub-type");
      return false;
    }

    // There is a clash between SCTE 57 and 65 for the meaning of table
    // sub-type 6. In SCTE 57 it means "map name sub-type"; in SCTE 65 it means
    // "source name sub-table".
    // How do we resolve this?
    // Well, SCTE 65 is *the* standard for CableCARD tuner out-of-band
    // information. On the other hand, as far as I'm aware SCTE 57 is either
    // not in use or is very rarely used. Therefore we are biased towards the
    // SCTE 65 interpretation.
    // If we see any of the other sub-tables [which aren't defined in SCTE 65]
    // then we switch to the SCTE 57 interpretation.
    // Note this approach may not work if the MNT in an SCTE 57 compliant
    // transport stream is received first.
    if (tableSubtype6Interpretation == 0)
    {
      if (tableSubtype == 6)
      {
        LogDebug(L"NTT: using SCTE 65 interpretation");
        tableSubtype6Interpretation = 5;
      }
      else
      {
        LogDebug(L"NTT: using SCTE 57 interpretation");
        tableSubtype6Interpretation = 6;
      }
    }
    else if (tableSubtype6Interpretation == 5 && tableSubtype != 6)
    {
      LogDebug(L"NTT: switching to SCTE 57 interpretation");
      tableSubtype6Interpretation = 6;
    }

    if (tableSubtype == 6)
    {
      tableSubtype = tableSubtype6Interpretation;
    }

    unsigned short pointer = 8;
    unsigned short endOfSection = section.SectionLength - 1;
    bool result = true;
    switch (tableSubtype)
    {
      case 1:
      {
        vector<CRecordNttTransponderName*> recordsTransponderName;
        result = DecodeTransponderNameSubTable(data,
                                                pointer,
                                                endOfSection,
                                                satelliteId,
                                                recordsTransponderName);
        if (result)
        {
          vector<CRecordNttTransponderName*>::const_iterator recordIt = recordsTransponderName.begin();
          for ( ; recordIt != recordsTransponderName.end(); recordIt++)
          {
            CRecordNttTransponderName* record = *recordIt;
            if (record != NULL)
            {
              record->Language = iso639LanguageCode;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 2:
      {
        vector<CRecordNttSatelliteText*> recordsSatelliteText;
        result = DecodeSatelliteTextSubTable(data, pointer, endOfSection, recordsSatelliteText);
        if (result)
        {
          vector<CRecordNttSatelliteText*>::const_iterator recordIt = recordsSatelliteText.begin();
          for ( ; recordIt != recordsSatelliteText.end(); recordIt++)
          {
            CRecordNttSatelliteText* record = *recordIt;
            if (record != NULL)
            {
              record->Language = iso639LanguageCode;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 3:
      {
        vector<CRecordNttRatingsText*> recordsRatingsText;
        result = DecodeRatingsTextSubTable(data, pointer, endOfSection,
                                            ratingRegion, recordsRatingsText);
        if (result)
        {
          vector<CRecordNttRatingsText*>::const_iterator recordIt = recordsRatingsText.begin();
          for ( ; recordIt != recordsRatingsText.end(); recordIt++)
          {
            CRecordNttRatingsText* record = *recordIt;
            if (record != NULL)
            {
              record->Language = iso639LanguageCode;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 4:
      {
        vector<CRecordNttRatingSystem*> recordsRatingSystem;
        result = DecodeRatingSystemSubTable(data, pointer, endOfSection, recordsRatingSystem);
        if (result)
        {
          vector<CRecordNttRatingSystem*>::const_iterator recordIt = recordsRatingSystem.begin();
          for ( ; recordIt != recordsRatingSystem.end(); recordIt++)
          {
            CRecordNttRatingSystem* record = *recordIt;
            if (record != NULL)
            {
              record->Language = iso639LanguageCode;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 5:
      {
        vector<CRecordNttSourceName*> recordsSourceName;
        result = DecodeSourceNameSubTable(data, pointer, endOfSection, recordsSourceName);
        if (result)
        {
          vector<CRecordNttSourceName*>::const_iterator recordIt = recordsSourceName.begin();
          for ( ; recordIt != recordsSourceName.end(); recordIt++)
          {
            CRecordNttSourceName* record = *recordIt;
            if (record != NULL)
            {
              record->Language = iso639LanguageCode;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 6:
      {
        vector<CRecordNttMapName*> recordsMapName;
        result = DecodeMapNameSubTable(data, pointer, endOfSection, recordsMapName);
        if (result)
        {
          vector<CRecordNttMapName*>::const_iterator recordIt = recordsMapName.begin();
          for ( ; recordIt != recordsMapName.end(); recordIt++)
          {
            CRecordNttMapName* record = *recordIt;
            if (record != NULL)
            {
              record->Language = iso639LanguageCode;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 7:
      {
        vector<CRecordNttCurrencySystem*> recordsCurrencySystem;
        result = DecodeCurrencySystemSubTable(data, pointer, endOfSection, recordsCurrencySystem);
        if (result)
        {
          vector<CRecordNttCurrencySystem*>::const_iterator recordIt = recordsCurrencySystem.begin();
          for ( ; recordIt != recordsCurrencySystem.end(); recordIt++)
          {
            CRecordNttCurrencySystem* record = *recordIt;
            if (record != NULL)
            {
              record->Language = iso639LanguageCode;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
          }
        }
        break;
      }
    }

    seenRevisionDescriptor = false;
    while (pointer + 1 < endOfSection)
    {
      unsigned char tag = data[pointer++];
      unsigned char length = data[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"NTT: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (endOfDescriptor > endOfSection)
      {
        LogDebug(L"NTT: invalid section, descriptor length = %hhu, pointer = %hu, end of section = %hu, tag = 0x%hhx",
                  length, pointer, endOfSection, tag);
        CleanUpRecords(records);
        return false;
      }

      if (tag == 0x93)
      {
        if (!DecodeRevisionDetectionDescriptor(&data[pointer],
                                                length,
                                                tableVersionNumber,
                                                sectionNumber,
                                                lastSectionNumber))
        {
          LogDebug(L"NTT: invalid descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of section = %hu",
                    tag, length, pointer, endOfSection);
          CleanUpRecords(records);
          return false;
        }
        seenRevisionDescriptor = true;
      }

      pointer = endOfDescriptor;
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"NTT: section parsing error, pointer = %hu, end of section = %hu",
                pointer, endOfSection);
      CleanUpRecords(records);
      return false;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeSection()");
  }
  return false;
}

bool CParserNtt::DecodeTransponderNameSubTable(const unsigned char* data,
                                                unsigned short& pointer,
                                                unsigned short endOfSection,
                                                unsigned char& satelliteId,
                                                vector<CRecordNttTransponderName*>& records)
{
  if (pointer + 3 > endOfSection)
  {
    LogDebug(L"NTT: invalid transponder name sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    satelliteId = data[pointer++];
    unsigned char firstIndex = data[pointer++];
    unsigned char numberOfTntRecords = data[pointer++];
    //LogDebug(L"NTT: transponder name sub-table, satellite ID = %hhu, first index = %hhu, number of TNT records = %hhu",
    //          satelliteId, firstIndex, numberOfTntRecords);

    if (pointer + (numberOfTntRecords * 3) > endOfSection)
    {
      LogDebug(L"NTT: invalid transponder name sub-table, number of TNT records = %hhu, pointer = %hu, end of section = %hu, satellite ID = %hhu, first index = %hhu",
                numberOfTntRecords, pointer, endOfSection, satelliteId,
                firstIndex);
      return false;
    }

    for (unsigned char i = 0; i < numberOfTntRecords && pointer + ((numberOfTntRecords - i) * 3) - 1 < endOfSection; i++)
    {
      unsigned char transponderNumber = data[pointer++] & 0x3f;
      unsigned char transponderNameLength = data[pointer++] & 0x1f;
      //LogDebug(L"  transponder number = %hhu, transponder name length = %hhu",
      //          transponderNumber, transponderNameLength);

      CRecordNttTransponderName* record = new CRecordNttTransponderName();
      if (record == NULL)
      {
        LogDebug(L"NTT: failed to allocate transponder name record, satellite ID = %hhu, first index = %hhu, number of TNT records = %hhu, index = %hhu, transponder number = %hhu",
                  satelliteId, firstIndex, numberOfTntRecords, i,
                  transponderNumber);
        CleanUpRecords(records);
        return false;
      }

      record->SatelliteId = satelliteId;
      record->Index = firstIndex + i;
      record->TransponderNumber = transponderNumber;

      if (transponderNameLength > 0)
      {
        if (
          pointer + ((numberOfTntRecords - 1 - i) * 3) + transponderNameLength + 1 > endOfSection ||
          !CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                        transponderNameLength,
                                                        &(record->Name))
        )
        {
          LogDebug(L"NTT: invalid transponder name sub-table, transponder name length = %hhu, pointer = %hu, end of section = %hu, satellite ID = %hhu, first index = %hhu, number of TNT records = %hhu, index = %hhu, transponder number = %hhu",
                    transponderNameLength, pointer, endOfSection,
                    satelliteId, firstIndex, numberOfTntRecords, i,
                    transponderNumber);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        if (record->Name == NULL)
        {
          LogDebug(L"NTT: failed to allocate transponder name, transponder name length = %hhu, satellite ID = %hhu, first index = %hhu, index = %hhu, transponder number = %hhu",
                    transponderNameLength, satelliteId, firstIndex, i,
                    transponderNumber);
        }

        pointer += transponderNameLength;
      }

      unsigned char tntDescriptorsCount = data[pointer++];
      //LogDebug(L"  name = %S, TNT descriptors count = %hhu",
      //          record->Name == NULL ? "" : record->Name, tntDescriptorCount);

      if (pointer + ((numberOfTntRecords - 1 - i) * 3) + (tntDescriptorsCount * 2) > endOfSection)
      {
        LogDebug(L"NTT: invalid transponder name sub-table, TNT descriptors count = %hhu, pointer = %hu, end of section = %hu, satellite ID = %hhu, first index = %hhu, number of TNT records = %hhu, transponder index = %hhu, transponder number = %hhu",
                  tntDescriptorsCount, pointer, endOfSection, satelliteId,
                  firstIndex, numberOfTntRecords, i, transponderNumber);
        delete record;
        CleanUpRecords(records);
        return false;
      }

      for (unsigned char d = 0; d < tntDescriptorsCount && pointer + ((numberOfTntRecords - 1 - i) * 3) + ((tntDescriptorsCount - d) * 2) - 1 < endOfSection; d++)
      {
        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        //LogDebug(L"NTT: transponder name sub-table descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (pointer + ((numberOfTntRecords - 1 - i) * 3) + ((tntDescriptorsCount - 1 - d) * 2) + length > endOfSection)
        {
          LogDebug(L"NTT: invalid transponder name sub-table, descriptor length = %hhu, pointer = %hu, end of section = %hu, satellite ID = %hhu, first index = %hhu, number of TNT records = %hhu, transponder index = %hhu, transponder number = %hhu, TNT descriptors count = %hhu, descriptor index = %hhu, tag = 0x%hhx",
                    length, pointer, endOfSection, satelliteId, firstIndex,
                    numberOfTntRecords, i, transponderNumber,
                    tntDescriptorsCount, d, tag);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        pointer += length;
      }

      records.push_back(record);
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeTransponderNameSubTable()");
  }
  return false;
}

bool CParserNtt::DecodeSatelliteTextSubTable(const unsigned char* data,
                                              unsigned short& pointer,
                                              unsigned short endOfSection,
                                              vector<CRecordNttSatelliteText*>& records)
{
  if (pointer + 2 > endOfSection)
  {
    LogDebug(L"NTT: invalid satellite text sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    unsigned char firstIndex = data[pointer++];
    unsigned char numberOfSttRecords = data[pointer++];
    //LogDebug(L"NTT: satellite text sub-table, first index = %hhu, number of STT records = %hhu",
    //          firstIndex, numberOfSttRecords);

    if (pointer + (numberOfSttRecords * 4) > endOfSection)
    {
      LogDebug(L"NTT: invalid satellite text sub-table, number of STT records = %hhu, pointer = %hu, end of section = %hu, first index = %hhu",
                numberOfSttRecords, pointer, endOfSection, firstIndex);
      return false;
    }

    for (unsigned char i = 0; i < numberOfSttRecords && pointer + ((numberOfSttRecords - i) * 4) - 1 < endOfSection; i++)
    {
      unsigned char satelliteId = data[pointer++];
      unsigned char satReferenceNameLength = data[pointer++] & 0xf;
      //LogDebug(L"  satellite ID = %hhu, sat. reference name length = %hhu",
      //          satelliteId, satReferenceNameLength);

      CRecordNttSatelliteText* record = new CRecordNttSatelliteText();
      if (record == NULL)
      {
        LogDebug(L"NTT: failed to allocate satellite text record, first index = %hhu, number of STT records = %hhu, index = %hhu, satellite ID = %hhu",
                  firstIndex, numberOfSttRecords, i, satelliteId);
        CleanUpRecords(records);
        return false;
      }

      record->Index = firstIndex + i;
      record->SatelliteId = satelliteId;

      if (satReferenceNameLength > 0)
      {
        if (
          pointer + ((numberOfSttRecords - 1 - i) * 4) + satReferenceNameLength + 2 > endOfSection ||
          !CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                        satReferenceNameLength,
                                                        &(record->ReferenceName))
        )
        {
          LogDebug(L"NTT: invalid satellite text sub-table, sat. reference name length = %hhu, pointer = %hu, end of section = %hu, first index = %hhu, number of STT records = %hhu, index = %hhu, satellite ID = %hhu",
                    satReferenceNameLength, pointer, endOfSection, firstIndex,
                    numberOfSttRecords, i, satelliteId);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        if (record->ReferenceName == NULL)
        {
          LogDebug(L"NTT: failed to allocate satellite reference name, sat. reference name length = %hhu, first index = %hhu, index = %hhu, satellite ID = %hhu",
                    satReferenceNameLength, firstIndex, i, satelliteId);
        }

        pointer += satReferenceNameLength;
      }

      unsigned char fullSatelliteNameLength = data[pointer++] & 0x1f;
      //LogDebug(L"  sat. reference name = %S, full satellite name length = %hhu",
      //          record->ReferenceName == NULL ? "" : record->ReferenceName,
      //          fullSatelliteNameLength);

      if (fullSatelliteNameLength > 0)
      {
        if (pointer + ((numberOfSttRecords - 1 - i) * 4) + fullSatelliteNameLength + 1 > endOfSection || !CTextUtil::AtscScteMultilingualTextToString(&data[pointer], fullSatelliteNameLength, &(record->Name)))
        {
          LogDebug(L"NTT: invalid satellite text sub-table, full satellite name length = %hhu, pointer = %hu, end of section = %hu, first index = %hhu, number of STT records = %hhu, index = %hhu, satellite ID = %hhu",
                    fullSatelliteNameLength, pointer, endOfSection, firstIndex,
                    numberOfSttRecords, i, satelliteId);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        if (record->Name == NULL)
        {
          LogDebug(L"NTT: failed to allocate full satellite name, full satellite name length = %hhu, first index = %hhu, index = %hhu, satellite ID = %hhu",
                    fullSatelliteNameLength, firstIndex, i, satelliteId);
        }

        pointer += fullSatelliteNameLength;
      }

      unsigned char sttDescriptorsCount = data[pointer++];
      //LogDebug(L"  full name = %S, STT descriptors count = %hhu",
      //          record->Name == NULL ? "" : record->Name,
      //          sttDescriptorsCount);

      if (pointer + ((numberOfSttRecords - 1 - i) * 4) + (sttDescriptorsCount * 2) > endOfSection)
      {
        LogDebug(L"NTT: invalid satellite text sub-table, STT descriptors count = %hhu, pointer = %hu, end of section = %hu, first index = %hhu, number of STT records = %hhu, index = %hhu, satellite ID = %hhu",
                  sttDescriptorsCount, pointer, endOfSection, firstIndex,
                  numberOfSttRecords, i, satelliteId);
        delete record;
        CleanUpRecords(records);
        return false;
      }

      for (unsigned char d = 0; d < sttDescriptorsCount && pointer + ((numberOfSttRecords - 1 - i) * 4) + ((sttDescriptorsCount - d) * 2) - 1 < endOfSection; d++)
      {
        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        //LogDebug(L"NTT: satellite text sub-table descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (pointer + ((numberOfSttRecords - 1 - i) * 4) + ((sttDescriptorsCount - 1 - d) * 2) + length > endOfSection)
        {
          LogDebug(L"NTT: invalid satellite text sub-table, descriptor length = %hhu, pointer = %hu, end of section = %hu, first index = %hhu, number of STT records = %hhu, satellite index = %hhu, satellite ID = %hhu, STT descriptors count = %hhu, descriptor index = %hhu, tag = 0x%hhx",
                    length, pointer, endOfSection, firstIndex,
                    numberOfSttRecords, i, satelliteId, sttDescriptorsCount, d,
                    tag);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        pointer += length;
      }

      records.push_back(record);
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeSatelliteTextSubTable()");
  }
  return false;
}

bool CParserNtt::DecodeRatingsTextSubTable(const unsigned char* data,
                                            unsigned short& pointer,
                                            unsigned short endOfSection,
                                            unsigned char& ratingRegion,
                                            vector<CRecordNttRatingsText*>& records)
{
  if (pointer + 7 > endOfSection)
  {
    LogDebug(L"NTT: invalid ratings text sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    ratingRegion = data[pointer++];
    //LogDebug(L"NTT: ratings text sub-table, rating region = %hhu",
    //          ratingRegion);

    if (pointer + 6 > endOfSection)
    {
      LogDebug(L"NTT: invalid ratings text sub-table, pointer = %hu, end of section = %hu, rating region = %hhu",
                pointer, endOfSection, ratingRegion);
      return false;
    }

    for (unsigned char i = 0; i < 6 && pointer + 5 - i < endOfSection; i++)
    {
      unsigned char levelsDefined = data[pointer++];
      //LogDebug(L"  levels defined = %hhu", levelsDefined);
      if (levelsDefined == 0)
      {
        continue;
      }

      if (pointer + (5 - i) + 1 + levelsDefined > endOfSection)
      {
        LogDebug(L"NTT: invalid ratings text sub-table, levels defined = %hhu, pointer = %hu, end of section = %hu, rating region = %hhu, dimension index = %hhu",
                  levelsDefined, pointer, endOfSection, ratingRegion, i);
        CleanUpRecords(records);
        return false;
      }

      unsigned char dimensionNameLength = data[pointer++];
      //LogDebug(L"  dimension name length = %hhu", dimensionNameLength);
      char* dimensionName = NULL;
      unsigned short actualDimensionNameLength = 0;
      if (dimensionNameLength > 0)
      {
        if (
          pointer + (5 - i) + dimensionNameLength + levelsDefined > endOfSection ||
          !CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                        dimensionNameLength,
                                                        &dimensionName))
        {
          LogDebug(L"NTT: invalid ratings text sub-table, dimension name length = %hhu, pointer = %hu, end of section = %hu, rating region = %hhu, dimension index = %hhu, levels defined = %hhu",
                    dimensionNameLength, pointer, endOfSection, ratingRegion,
                    i, levelsDefined);
          CleanUpRecords(records);
          return false;
        }

        if (dimensionName == NULL)
        {
          LogDebug(L"NTT: failed to allocate rating dimension name, dimension name length = %hhu, rating region = %hhu, dimension index = %hhu",
                    dimensionNameLength, ratingRegion, i);
        }
        else
        {
          actualDimensionNameLength = strlen(dimensionName) + 1;
        }

        pointer += dimensionNameLength;
      }
      //LogDebug(L"  dimension name = %S",
      //          dimensionName == NULL ? "" : dimensionName);

      for (unsigned char j = 0; j < levelsDefined && pointer + (5 - i) + (levelsDefined - j) - 1 < endOfSection; j++)
      {
        unsigned char ratingNameLength = data[pointer++];
        //LogDebug(L"    level index = %hhu, rating name length = %hhu",
        //          j, ratingNameLength);

        CRecordNttRatingsText* record = new CRecordNttRatingsText();
        if (record == NULL)
        {
          LogDebug(L"NTT: failed to allocate ratings text record, rating region = %hhu, dimension index = %hhu, levels defined = %hhu, level index = %hhu",
                    ratingRegion, i, levelsDefined, j);
          pointer += ratingNameLength;
          continue;
        }

        record->RatingRegion = ratingRegion;
        record->DimensionIndex = i;
        record->LevelIndex = j;

        if (dimensionName != NULL)
        {
          record->DimensionName = new char[actualDimensionNameLength];
          if (record->DimensionName != NULL)
          {
            strncpy(record->DimensionName, dimensionName, actualDimensionNameLength);
            record->DimensionName[actualDimensionNameLength - 1] = NULL;
          }
          else
          {
            LogDebug(L"NTT: failed to allocate rating dimension name, dimension name length = %hhu, rating region = %hhu, dimension index = %hhu, levels defined = %hhu, level index = %hhu",
                      actualDimensionNameLength, ratingRegion, i,
                      levelsDefined, j);
          }
        }

        if (ratingNameLength > 0)
        {
          if (
            pointer + (5 - i) + (levelsDefined - 1 - j) + ratingNameLength > endOfSection ||
            !CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                          ratingNameLength,
                                                          &(record->LevelName))
          )
          {
            LogDebug(L"NTT: invalid ratings text sub-table, rating name length = %hhu, pointer = %hu, end of section = %hu, rating region = %hhu, dimension index = %hhu, levels defined = %hhu, level index = %hhu",
                      ratingNameLength, pointer, endOfSection, ratingRegion, i,
                      levelsDefined, j);
            delete record;
            CleanUpRecords(records);
            return false;
          }

          if (record->LevelName == NULL)
          {
            LogDebug(L"NTT: failed to allocate rating level name, rating name length = %hhu, rating region = %hhu, dimension index = %hhu, levels defined = %hhu, level index = %hhu",
                      ratingNameLength, ratingRegion, i, levelsDefined, j);
          }

          pointer += ratingNameLength;
        }
        //LogDebug(L"    level name = %S",
        //          record->LevelName == NULL ? "" : record->LevelName);

        records.push_back(record);
      }

      if (dimensionName != NULL)
      {
        delete[] dimensionName;
      }
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeRatingsTextSubTable()");
  }
  return false;
}

bool CParserNtt::DecodeRatingSystemSubTable(const unsigned char* data,
                                            unsigned short& pointer,
                                            unsigned short endOfSection,
                                            vector<CRecordNttRatingSystem*>& records)
{
  if (pointer >= endOfSection)
  {
    LogDebug(L"NTT: invalid rating system sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    unsigned char regionsDefined = data[pointer++];
    //LogDebug(L"NTT: rating system sub-table, regions defined = %hhu",
    //          regionsDefined);

    if (pointer + (regionsDefined * 3) > endOfSection)
    {
      LogDebug(L"NTT: invalid rating system sub-table, regions defined = %hhu, pointer = %hu, end of section = %hu",
                regionsDefined, pointer, endOfSection);
      return false;
    }

    for (unsigned char i = 0; i < regionsDefined && pointer + ((regionsDefined - i) * 3) - 1 < endOfSection; i++)
    {
      unsigned char dataLength = data[pointer++];
      unsigned short endOfData = pointer + dataLength;
      unsigned char ratingRegion = data[pointer++];
      unsigned char stringLength = data[pointer++];
      //LogDebug(L"  data length = %hhu, rating region = %hhu, string length = %hhu",
      //          dataLength, ratingRegion, stringLength);

      if (
        endOfData + ((regionsDefined - 1 - i) * 3) > endOfSection ||
        stringLength > dataLength - 2
      )
      {
        LogDebug(L"NTT: invalid rating system sub-table, data length = %hhu, string length = %hhu, pointer = %hu, end of section = %hu, regions defined = %hhu, index = %hhu, rating region = %hhu",
                  dataLength, stringLength, pointer, endOfSection,
                  regionsDefined, i, ratingRegion);
        CleanUpRecords(records);
        return false;
      }

      CRecordNttRatingSystem* record = new CRecordNttRatingSystem();
      if (record == NULL)
      {
        LogDebug(L"NTT: failed to allocate rating system record, rating region = %hhu, regions defined = %hhu, index = %hhu",
                  ratingRegion, regionsDefined, i);
        pointer = endOfData;
        continue;
      }

      record->RatingRegion = ratingRegion;

      if (stringLength > 0)
      {
        if (!CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                          stringLength,
                                                          &(record->Name)))
        {
          LogDebug(L"NTT: invalid rating system sub-table, string length = %hhu, pointer = %hu, end of section = %hu, regions defined = %hhu, index = %hhu, rating region = %hhu, data length = %hhu",
                    stringLength, pointer, endOfSection, regionsDefined, i,
                    ratingRegion, dataLength);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        if (record->Name == NULL)
        {
          LogDebug(L"NTT: failed to allocate rating system text, string length = %hhu, regions defined = %hhu, index = %hhu, rating region = %hhu",
                    stringLength, regionsDefined, i, ratingRegion);
        }

        pointer += stringLength;
      }
      //LogDebug(L"  rating system text = %S",
      //          record->Name == NULL ? "" : record->Name);

      while (pointer + 1 < endOfData)
      {
        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        //LogDebug(L"NTT: rating system sub-table descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (pointer + ((regionsDefined - 1 - i) * 3) + length > endOfSection)
        {
          LogDebug(L"NTT: invalid rating system sub-table, descriptor length = %hhu, pointer = %hu, end of data = %hu, regions defined = %hhu, index = %hhu, rating region = %hhu, tag = 0x%hhx",
                    length, pointer, endOfData, regionsDefined, i,
                    ratingRegion, tag);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        pointer += length;
      }

      if (pointer != endOfData)
      {
        LogDebug(L"NTT: invalid rating system sub-table, pointer = %hu, end of data = %hu, regions defined = %hhu, index = %hhu, rating region = %hhu, data length = %hhu, string length = %hhu",
                  pointer, endOfData, regionsDefined, i, ratingRegion,
                  dataLength, stringLength);
        delete record;
        CleanUpRecords(records);
        return false;
      }

      records.push_back(record);
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeRatingSystemSubTable()");
  }
  return false;
}

bool CParserNtt::DecodeSourceNameSubTable(const unsigned char* data,
                                          unsigned short& pointer,
                                          unsigned short endOfSection,
                                          vector<CRecordNttSourceName*>& records)
{
  if (pointer >= endOfSection)
  {
    LogDebug(L"NTT: invalid source name sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    unsigned char numberOfSntRecords = data[pointer++];
    //LogDebug(L"NTT: source name sub-table, number of SNT records = %hhu",
    //          numberOfSntRecords);

    if (pointer + (numberOfSntRecords * 5) > endOfSection)
    {
      LogDebug(L"NTT: invalid source name sub-table, number of SNT records = %hhu, pointer = %hu, end of section = %hu",
                numberOfSntRecords, pointer, endOfSection);
      return false;
    }

    for (unsigned char i = 0; i < numberOfSntRecords && pointer + ((numberOfSntRecords - i) * 5) - 1 < endOfSection; i++)
    {
      bool applicationType = (data[pointer++] & 0x80) != 0;
      unsigned short sourceId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned char nameLength = data[pointer++];
      //LogDebug(L"  application type = %d, source ID = %hu, name length = %hhu",
      //          applicationType, sourceId, nameLength);

      CRecordNttSourceName* record = new CRecordNttSourceName();
      if (record == NULL)
      {
        LogDebug(L"NTT: failed to allocate source name record, application type = %d, source ID = %hu, number of SNT records = %hhu, index = %hhu",
                  applicationType, sourceId, numberOfSntRecords, i);
        CleanUpRecords(records);
        return false;
      }

      record->ApplicationType = applicationType;
      record->SourceId = sourceId;

      if (nameLength > 0)
      {
        if (
          pointer + ((numberOfSntRecords - 1 - i) * 5) + nameLength + 1 > endOfSection ||
          !CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                        nameLength,
                                                        &(record->Name))
        )
        {
          LogDebug(L"NTT: invalid source name sub-table, name length = %hhu, pointer = %hu, end of section = %hu, number of SNT records = %hhu, index = %hhu, application type = %d, source ID = %hu",
                    nameLength, pointer, endOfSection, numberOfSntRecords, i,
                    applicationType, sourceId);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        if (record->Name == NULL)
        {
          LogDebug(L"NTT: failed to allocate source name name, name length = %hhu, number of SNT records = %hhu, index = %hhu, application type = %d, source ID = %hu",
                    nameLength, numberOfSntRecords, i, applicationType,
                    sourceId);
        }

        pointer += nameLength;
      }

      unsigned char sntDescriptorsCount = data[pointer++];
      //LogDebug(L"  source name = %S, SNT descriptors count = %hhu",
      //          record->Name == NULL ? "" : record->Name, sntDescriptorCount);

      if (pointer + ((numberOfSntRecords - 1 - i) * 5) + (sntDescriptorsCount * 2) > endOfSection)
      {
        LogDebug(L"NTT: invalid source name sub-table, SNT descriptors count = %hhu, pointer = %hu, end of section = %hu, number of SNT records = %hhu, index = %hhu, application type = %d, source ID = %hu",
                  sntDescriptorsCount, pointer, endOfSection,
                  numberOfSntRecords, i, applicationType, sourceId);
        delete record;
        CleanUpRecords(records);
        return false;
      }

      for (unsigned char d = 0; d < sntDescriptorsCount && pointer + ((numberOfSntRecords - 1 - i) * 5) + ((sntDescriptorsCount - d) * 2) - 1 < endOfSection; d++)
      {
        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        //LogDebug(L"NTT: source name sub-table descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (pointer + ((numberOfSntRecords - 1 - i) * 5) + ((sntDescriptorsCount - 1 - d) * 2) + length > endOfSection)
        {
          LogDebug(L"NTT: invalid source name sub-table, descriptor length = %hhu, pointer = %hu, end of section = %hu, number of SNT records = %hhu, index = %hhu, application type = %d, source ID = %hu, SNT descriptors count = %hhu, descriptor index = %hhu, tag = 0x%hhx",
                    length, pointer, endOfSection, numberOfSntRecords, i,
                    applicationType, sourceId, sntDescriptorsCount, d, tag);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        pointer += length;
      }

      records.push_back(record);
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeSourceNameSubTable()");
  }
  return false;
}

bool CParserNtt::DecodeMapNameSubTable(const unsigned char* data,
                                        unsigned short& pointer,
                                        unsigned short endOfSection,
                                        vector<CRecordNttMapName*>& records)
{
  if (pointer >= endOfSection)
  {
    LogDebug(L"NTT: invalid map name sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    unsigned char numberOfMntRecords = data[pointer++];
    //LogDebug(L"NTT: map name sub-table, number of MNT records = %hhu",
    //          numberOfMntRecords);

    if (pointer + (numberOfMntRecords * 4) > endOfSection)
    {
      LogDebug(L"NTT: invalid map name sub-table, number of MNT records = %hhu, pointer = %hu, end of section = %hu",
                numberOfMntRecords, pointer, endOfSection);
      return false;
    }

    for (unsigned char i = 0; i < numberOfMntRecords && pointer + ((numberOfMntRecords - i) * 4) - 1 < endOfSection; i++)
    {
      unsigned short vctId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned char mapNameLength = data[pointer++];
      //LogDebug(L"  VCT ID = %hu, map name length = %hhu",
      //          vctId, mapNameLength);

      CRecordNttMapName* record = new CRecordNttMapName();
      if (record == NULL)
      {
        LogDebug(L"NTT: failed to allocate map name record, VCT ID = %hu, number of MNT records = %hhu, index = %hhu",
                  vctId, numberOfMntRecords, i);
        CleanUpRecords(records);
        return false;
      }

      record->VctId = vctId;

      if (mapNameLength > 0)
      {
        if (
          pointer + ((numberOfMntRecords - 1 - i) * 4) + mapNameLength + 1 > endOfSection ||
          !CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                        mapNameLength,
                                                        &(record->Name))
        )
        {
          LogDebug(L"NTT: invalid map name sub-table, map name length = %hhu, pointer = %hu, end of section = %hu, number of MNT records = %hhu, index = %hhu, VCT ID = %hu",
                    mapNameLength, pointer, endOfSection, numberOfMntRecords,
                    i, vctId);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        if (record->Name == NULL)
        {
          LogDebug(L"NTT: failed to allocate map name name, map name length = %hhu, number of MNT records = %hhu, index = %hhu, VCT ID = %hu",
                    mapNameLength, numberOfMntRecords, i, vctId);
        }

        pointer += mapNameLength;
      }

      unsigned char mntDescriptorsCount = data[pointer++];
      //LogDebug(L"  map name = %S, MNT descriptors count = %hhu",
      //          record->Name == NULL ? "" : record->Name, mntDescriptorsCount);

      if (pointer + ((numberOfMntRecords - 1 - i) * 4) + (mntDescriptorsCount * 2) > endOfSection)
      {
        LogDebug(L"NTT: invalid map name sub-table, MNT descriptors count = %hhu, pointer = %hu, end of section = %hu, number of MNT records = %hhu, index = %hhu, VCT ID = %hu",
                  mntDescriptorsCount, pointer, endOfSection,
                  numberOfMntRecords, i, vctId);
        delete record;
        CleanUpRecords(records);
        return false;
      }

      for (unsigned char d = 0; d < mntDescriptorsCount && pointer + ((numberOfMntRecords - 1 - i) * 4) + ((mntDescriptorsCount - d) * 2) - 1 < endOfSection; d++)
      {
        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        //LogDebug(L"NTT: map name sub-table descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (pointer + ((numberOfMntRecords - 1 - i) * 4) + ((mntDescriptorsCount - 1 - d) * 2) + length > endOfSection)
        {
          LogDebug(L"NTT: invalid map name sub-table, descriptor length = %hhu, pointer = %hu, end of section = %hu, number of MNT records = %hhu, index = %hhu, VCT ID = %hu, MNT descriptors count = %hhu, descriptor index = %hhu, tag = 0x%hhx",
                    length, pointer, endOfSection, numberOfMntRecords, i,
                    vctId, mntDescriptorsCount, d, tag);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        pointer += length;
      }

      records.push_back(record);
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeMapNameSubTable()");
  }
  return false;
}

bool CParserNtt::DecodeCurrencySystemSubTable(const unsigned char* data,
                                              unsigned short& pointer,
                                              unsigned short endOfSection,
                                              vector<CRecordNttCurrencySystem*>& records)
{
  if (pointer >= endOfSection)
  {
    LogDebug(L"NTT: invalid currency system sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    unsigned char regionsDefined = data[pointer++];
    //LogDebug(L"NTT: currency system sub-table, regions defined = %hhu", regionsDefined);

    if (pointer + (regionsDefined * 3) > endOfSection)
    {
      LogDebug(L"NTT: invalid currency system sub-table, regions defined = %hhu, pointer = %hu, end of section = %hu",
                regionsDefined, pointer, endOfSection);
      return false;
    }

    for (unsigned char i = 0; i < regionsDefined && pointer + ((regionsDefined - i) * 3) - 1 < endOfSection; i++)
    {
      unsigned char dataLength = data[pointer++];
      unsigned short endOfData = pointer + dataLength;
      unsigned char currencyRegion = data[pointer++];
      unsigned char stringLength = data[pointer++];
      //LogDebug(L"  data length = %hhu, currency region = %hhu, string length = %hhu",
      //          dataLength, currencyRegion, stringLength);

      if (
        endOfData + ((regionsDefined - 1 - i) * 3) > endOfSection ||
        stringLength > dataLength - 2
      )
      {
        LogDebug(L"NTT: invalid currency system sub-table, data length = %hhu, string length = %hhu, pointer = %hu, end of section = %hu, regions defined = %hhu, index = %hhu, currency region = %hhu",
                  dataLength, stringLength, pointer, endOfSection,
                  regionsDefined, i, currencyRegion);
        CleanUpRecords(records);
        return false;
      }

      CRecordNttCurrencySystem* record = new CRecordNttCurrencySystem();
      if (record == NULL)
      {
        LogDebug(L"NTT: failed to allocate currency system record, currency region = %hhu, regions defined = %hhu, index = %hhu",
                  currencyRegion, regionsDefined, i);
        pointer = endOfData;
        continue;
      }

      record->CurrencyRegion = currencyRegion;

      if (stringLength > 0)
      {
        if (!CTextUtil::AtscScteMultilingualTextToString(&data[pointer],
                                                          stringLength,
                                                          &(record->Name)))
        {
          LogDebug(L"NTT: invalid currency system sub-table, string length = %hhu, pointer = %hu, end of section = %hu, regions defined = %hhu, index = %hhu, currency region = %hhu, data length = %hhu",
                    stringLength, pointer, endOfSection, regionsDefined, i,
                    currencyRegion, dataLength);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        if (record->Name == NULL)
        {
          LogDebug(L"NTT: failed to allocate currency system text, string length = %hhu, regions defined = %hhu, index = %hhu, currency region = %hhu",
                    stringLength, regionsDefined, i, currencyRegion);
        }

        pointer += stringLength;
      }
      //LogDebug(L"  currency system text = %S",
      //          record->Name == NULL ? "" : record->Name);

      while (pointer + 1 < endOfData)
      {
        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        //LogDebug(L"NTT: currency system sub-table descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (pointer + ((regionsDefined - 1 - i) * 3) + length > endOfSection)
        {
          LogDebug(L"NTT: invalid currency system sub-table, descriptor length = %hhu, pointer = %hu, end of data = %hu, regions defined = %hhu, index = %hhu, currency region = %hhu, tag = 0x%hhx",
                    length, pointer, endOfData, regionsDefined, i,
                    currencyRegion, tag);
          delete record;
          CleanUpRecords(records);
          return false;
        }

        pointer += length;
      }

      if (pointer != endOfData)
      {
        LogDebug(L"NTT: invalid currency system sub-table, pointer = %hu, end of data = %hu, regions defined = %hhu, index = %hhu, currency region = %hhu, data length = %hhu, string length = %hhu",
                  pointer, endOfData, regionsDefined, i, currencyRegion,
                  dataLength, stringLength);
        delete record;
        CleanUpRecords(records);
        return false;
      }

      records.push_back(record);
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeCurrencySystemSubTable()");
  }
  return false;
}

bool CParserNtt::DecodeRevisionDetectionDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned char& tableVersionNumber,
                                                    unsigned char& sectionNumber,
                                                    unsigned char& lastSectionNumber)
{
  if (dataLength != 3)
  {
    LogDebug(L"NTT: invalid revision detection descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    tableVersionNumber = data[0] & 0x1f;
    sectionNumber = data[1];
    lastSectionNumber = data[2];
    //LogDebug(L"NTT: revision detection descriptor, table version number = %hhu, section number = %hhu, last section number = %hhu",
    //          tableVersionNumber, sectionNumber, lastSectionNumber);
    return true;
  }
  catch (...)
  {
    LogDebug(L"NTT: unhandled exception in DecodeRevisionDetectionDescriptor()");
  }
  return false;
}