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
#include "ParserNitAtsc.h"
#include <algorithm>    // find()
#include <map>
#include "EnterCriticalSection.h"


#define MINIMUM_RECORD_BYTE_COUNT_CDS 5
#define MINIMUM_RECORD_BYTE_COUNT_MMS 6
#define MINIMUM_RECORD_BYTE_COUNT_SIS 4
#define MINIMUM_RECORD_BYTE_COUNT_TDS 6


extern void LogDebug(const wchar_t* fmt, ...);

CParserNitAtsc::CParserNitAtsc()
  : m_recordsCarrierDefinition(600000), m_recordsModulationMode(600000),
    m_recordsSatelliteInformation(600000), m_recordsTransponderData(600000)
{
  m_isReady = false;
  SetCallBack(NULL);
}

CParserNitAtsc::~CParserNitAtsc()
{
  SetCallBack(NULL);
}

void CParserNitAtsc::Reset()
{
  LogDebug(L"NIT ATSC: reset");
  CEnterCriticalSection lock(m_section);
  m_recordsCarrierDefinition.RemoveAllRecords();
  m_recordsModulationMode.RemoveAllRecords();
  m_recordsSatelliteInformation.RemoveAllRecords();
  m_recordsTransponderData.RemoveAllRecords();
  m_seenSections.clear();
  m_unseenSections.clear();
  m_isReady = false;
  LogDebug(L"NIT ATSC: reset done");
}

void CParserNitAtsc::SetCallBack(ICallBackNitAtsc* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserNitAtsc::OnNewSection(CSection& section)
{
  try
  {
    unsigned char protocolVersion;
    unsigned char transmissionMedium;
    unsigned char tableSubtype;
    unsigned char satelliteId;
    vector<CRecordNit*> records;
    bool seenRevisionDescriptor;
    unsigned char tableVersionNumber;
    unsigned char sectionNumber;
    unsigned char lastSectionNumber;
    if (!DecodeSection(section, protocolVersion, transmissionMedium,
                        tableSubtype, satelliteId, records,
                        seenRevisionDescriptor, tableVersionNumber,
                        sectionNumber, lastSectionNumber))
    {
      if (seenRevisionDescriptor)
      {
        LogDebug(L"NIT ATSC: invalid section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, satellite ID = %hhu, version number = %hhu, section number = %hhu",
                  protocolVersion, transmissionMedium, tableSubtype, satelliteId,
                  tableVersionNumber, sectionNumber);
      }
      else
      {
        LogDebug(L"NIT ATSC: invalid section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, satellite ID = %hhu",
                  protocolVersion, transmissionMedium, tableSubtype, satelliteId);
      }
      return;
    }

    CRecordStore* recordSet;
    if (tableSubtype == 1)
    {
      recordSet = &m_recordsCarrierDefinition;
    }
    else if (tableSubtype == 2)
    {
      recordSet = &m_recordsModulationMode;
    }
    else if (tableSubtype == 3)
    {
      recordSet = &m_recordsSatelliteInformation;
    }
    else
    {
      recordSet = &m_recordsTransponderData;
    }

    CEnterCriticalSection lock(m_section);
    unsigned long sectionGroupKey;
    if (!seenRevisionDescriptor)
    {
      // Assume that there is one section for each sub-table type, except for
      // transponder data which has one section for each satellite.
      if (recordSet->GetRecordCount() == 0)
      {
        LogDebug(L"NIT ATSC: received, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu",
                  protocolVersion, transmissionMedium, tableSubtype);
      }
      if (tableSubtype == 4)
      {
        recordSet->MarkExpiredRecords((transmissionMedium << 8) | satelliteId);
      }
      else
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
        //LogDebug(L"NIT ATSC: previously seen section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, section number = %hhu",
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
          LogDebug(L"NIT ATSC: received, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                    protocolVersion, transmissionMedium, tableSubtype,
                    tableVersionNumber, sectionNumber, lastSectionNumber);
        }
        else
        {
          LogDebug(L"NIT ATSC: changed, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                    protocolVersion, transmissionMedium, tableSubtype,
                    tableVersionNumber, sectionNumber, lastSectionNumber);
          if (tableSubtype == 4)
          {
            recordSet->MarkExpiredRecords((transmissionMedium << 8) | satelliteId);
          }
          else
          {
            recordSet->MarkExpiredRecords(transmissionMedium);
          }
          if (m_isReady && m_callBack != NULL)
          {
            m_isReady = false;
            m_callBack->OnTableChange(TABLE_ID_NIT_ATSC);
          }
          m_isReady = false;
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
        //LogDebug(L"NIT ATSC: new section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, version number = %hhu, section number = %hhu",
        //            protocolVersion, transmissionMedium, tableSubtype,
        //            tableVersionNumber, sectionNumber);
      }

      m_seenSections.push_back(sectionKey);
      m_unseenSections.erase(sectionIt);
    }

    if (
      m_callBack != NULL &&
      m_recordsCarrierDefinition.GetRecordCount() == 0 &&
      m_recordsModulationMode.GetRecordCount() == 0 &&
      m_recordsSatelliteInformation.GetRecordCount() == 0 &&
      m_recordsTransponderData.GetRecordCount() == 0
    )
    {
      m_callBack->OnTableSeen(TABLE_ID_NIT_ATSC);
    }

    unsigned long newChangedOrExpiredRecordCount = 0;
    vector<CRecordNit*>::const_iterator recordIt = records.begin();
    for ( ; recordIt != records.end(); recordIt++)
    {
      CRecordNit* record = *recordIt;
      if (recordSet->AddOrUpdateRecord((IRecord**)&record, m_callBack))
      {
        newChangedOrExpiredRecordCount++;
      }
    }
    if (m_seenSections.size() == 0 && m_unseenSections.size() == 0)
    {
      newChangedOrExpiredRecordCount += recordSet->RemoveExpiredRecords(m_callBack);
    }

    // Are we ready?
    if (IsReadyPrivate(transmissionMedium))
    {
      if (m_seenSections.size() > 0)
      {
        newChangedOrExpiredRecordCount += recordSet->RemoveExpiredRecords(m_callBack);
      }

      // Did something actually change?
      if (m_seenSections.size() > 0 || newChangedOrExpiredRecordCount > 0)
      {
        LogDebug(L"NIT ATSC: ready, sections parsed = %llu, carrier definition count = %lu, modulation mode count = %lu, satellite information count = %lu, transponder data count = %lu",
                  (unsigned long long)m_seenSections.size(),
                  m_recordsCarrierDefinition.GetRecordCount(),
                  m_recordsModulationMode.GetRecordCount(),
                  m_recordsSatelliteInformation.GetRecordCount(),
                  m_recordsTransponderData.GetRecordCount());
        m_isReady = true;
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_NIT_ATSC);
        }
      }
    }
    else if (m_isReady)
    {
      LogDebug(L"NIT ATSC: changed, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu",
                protocolVersion, transmissionMedium, tableSubtype);
      m_isReady = false;
      if (m_callBack != NULL)
      {
        m_callBack->OnTableChange(TABLE_ID_NIT_ATSC);
      }
    }
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in OnNewSection()");
  }
}

bool CParserNitAtsc::IsSeen() const
{
  CEnterCriticalSection lock(m_section);
  return
    m_seenSections.size() > 0 ||
    m_recordsCarrierDefinition.GetRecordCount() > 0 ||
    m_recordsModulationMode.GetRecordCount() > 0 ||
    m_recordsSatelliteInformation.GetRecordCount() > 0 ||
    m_recordsTransponderData.GetRecordCount() > 0;
}

bool CParserNitAtsc::IsReady() const
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

bool CParserNitAtsc::GetCarrierDefinition(unsigned char index,
                                          unsigned char transmissionMedium,
                                          unsigned long& frequency) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsCarrierDefinition.GetRecordByKey((transmissionMedium << 8) | index, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NIT ATSC: invalid carrier definition identifiers, index = %hhu, transmission medium = %hhu",
              index, transmissionMedium);
    return false;
  }

  CRecordNitCarrierDefinition* recordNit = dynamic_cast<CRecordNitCarrierDefinition*>(record);
  if (recordNit == NULL)
  {
    LogDebug(L"NIT ATSC: invalid carrier definition record, index = %hhu, transmission medium = %hhu",
              index, transmissionMedium);
    return false;
  }
  frequency = recordNit->Frequency;
  return true;
}

bool CParserNitAtsc::GetModulationMode(unsigned char index,
                                        unsigned char transmissionMedium,
                                        unsigned char& transmissionSystem,
                                        unsigned char& innerCodingMode,
                                        bool& splitBitstreamMode,
                                        unsigned char& modulationFormat,
                                        unsigned long& symbolRate) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsModulationMode.GetRecordByKey((transmissionMedium << 8) | index, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NIT ATSC: invalid modulation mode identifiers, index = %hhu, transmission medium = %hhu",
              index, transmissionMedium);
    return false;
  }

  CRecordNitModulationMode* recordNit = dynamic_cast<CRecordNitModulationMode*>(record);
  if (recordNit == NULL)
  {
    LogDebug(L"NIT ATSC: invalid modulation mode record, index = %hhu, transmission medium = %hhu",
              index, transmissionMedium);
    return false;
  }
  transmissionSystem = recordNit->TransmissionSystem;
  innerCodingMode = recordNit->InnerCodingMode;
  splitBitstreamMode = recordNit->SplitBitstreamMode;
  modulationFormat = recordNit->ModulationFormat;
  symbolRate = recordNit->SymbolRate;
  return true;
}

bool CParserNitAtsc::GetSatelliteInformation(unsigned char transmissionMedium,
                                              unsigned char satelliteId,
                                              bool& youAreHere,
                                              unsigned char& frequencyBand,
                                              bool& outOfService,
                                              unsigned char& hemisphere,
                                              unsigned short& orbitalPosition,
                                              unsigned char& polarisationType,
                                              unsigned char& numberOfTransponders) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsSatelliteInformation.GetRecordByKey((transmissionMedium << 8) | satelliteId, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NIT ATSC: invalid satellite information identifiers, transmission medium = %hhu, satellite ID = %hhu",
              transmissionMedium, satelliteId);
    return false;
  }

  CRecordNitSatelliteInformation* recordNit = dynamic_cast<CRecordNitSatelliteInformation*>(record);
  if (recordNit == NULL)
  {
    LogDebug(L"NIT ATSC: invalid satellite information record, transmission medium = %hhu, satellite ID = %hhu",
              transmissionMedium, satelliteId);
    return false;
  }
  youAreHere = recordNit->YouAreHere;
  frequencyBand = recordNit->FrequencyBand;
  outOfService = recordNit->OutOfService;
  hemisphere = recordNit->Hemisphere;
  orbitalPosition = recordNit->OrbitalPosition;
  polarisationType = recordNit->PolarisationType;
  numberOfTransponders = recordNit->NumberOfTransponders;
  return true;
}

bool CParserNitAtsc::GetTransponderData(unsigned char transmissionMedium,
                                        unsigned char satelliteId,
                                        unsigned char transponderNumber,
                                        unsigned char& transportType,
                                        unsigned char& polarisation,
                                        unsigned char& cdtReference,
                                        unsigned char& mmtReference,
                                        unsigned short& vctId,
                                        bool& rootTransponder,
                                        bool& wideBandwidthVideo,
                                        unsigned char& waveformStandard,
                                        bool& wideBandwidthAudio,
                                        bool& compandedAudio,
                                        unsigned char& matrixMode,
                                        unsigned short& subcarrier2Offset,
                                        unsigned short& subcarrier1Offset,
                                        unsigned long& carrierFrequencyOverride) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsTransponderData.GetRecordByKey((transmissionMedium << 16) | (satelliteId << 8) | transponderNumber, &record) ||
    record == NULL
  )
  {
    LogDebug(L"NIT ATSC: invalid transponder data identifiers, transmission medium = %hhu, satellite ID = %hhu, transponder number = %hhu",
              transmissionMedium, satelliteId, transponderNumber);
    return false;
  }

  CRecordNitTransponderData* recordNit = dynamic_cast<CRecordNitTransponderData*>(record);
  if (recordNit == NULL)
  {
    LogDebug(L"NIT ATSC: invalid transponder data record, transmission medium = %hhu, satellite ID = %hhu, transponder number = %hhu",
              transmissionMedium, satelliteId, transponderNumber);
    return false;
  }
  transportType = recordNit->TransportType;
  polarisation = recordNit->Polarisation;
  cdtReference = recordNit->CdtReference;
  mmtReference = recordNit->MmtReference;
  vctId = recordNit->VctId;
  rootTransponder = recordNit->RootTransponder;
  wideBandwidthVideo = recordNit->WideBandwidthVideo;
  waveformStandard = recordNit->WaveformStandard;
  wideBandwidthAudio = recordNit->WideBandwidthAudio;
  compandedAudio = recordNit->CompandedAudio;
  matrixMode = recordNit->MatrixMode;
  subcarrier2Offset = recordNit->Subcarrier2Offset;
  subcarrier1Offset = recordNit->Subcarrier1Offset;
  carrierFrequencyOverride = recordNit->CarrierFrequencyOverride;
  return true;
}

bool CParserNitAtsc::IsReadyPrivate(unsigned char transmissionMedium) const
{
  // We expect one section each for the CDT and MMT. SIT and TDT are only
  // expected for satellite broadcasts, and should be correlated.
  unsigned long recordCountSi = m_recordsSatelliteInformation.GetRecordCount();
  unsigned long recordCountTd = m_recordsTransponderData.GetRecordCount();
  if (
    m_unseenSections.size() != 0 ||
    m_recordsCarrierDefinition.GetRecordCount() == 0 ||
    m_recordsModulationMode.GetRecordCount() == 0 ||
    (
      transmissionMedium == 1 &&  // satellite
      (
        recordCountSi == 0 ||
        recordCountTd == 0
      )
    )
  )
  {
    return false;
  }

  if (m_seenSections.size() > 0)
  {
    return true;
  }

  // Revision detection descriptors not used.
  // Check that we have seen all the satellite transponders that we expect.
  map<unsigned char, short> satelliteTransponderCounts;
  for (unsigned long i = 0; i < recordCountSi; i++)
  {
    IRecord* record = NULL;
    if (m_recordsSatelliteInformation.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordNitSatelliteInformation* recordNit = dynamic_cast<CRecordNitSatelliteInformation*>(record);
      if (recordNit != NULL && recordNit->LastSeen != 0)
      {
        satelliteTransponderCounts[recordNit->SatelliteId] = recordNit->NumberOfTransponders;
      }
    }
  }

  for (unsigned long i = 0; i < recordCountTd; i++)
  {
    IRecord* record = NULL;
    if (m_recordsTransponderData.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordNitTransponderData* recordNit = dynamic_cast<CRecordNitTransponderData*>(record);
      if (recordNit != NULL && recordNit->LastSeen != 0)
      {
        if (satelliteTransponderCounts.find(recordNit->SatelliteId) == satelliteTransponderCounts.end())
        {
          return false;
        }
        satelliteTransponderCounts[recordNit->SatelliteId]--;
      }
    }
  }

  map<unsigned char, short>::const_iterator satelliteIt = satelliteTransponderCounts.begin();
  for ( ; satelliteIt != satelliteTransponderCounts.end(); satelliteIt++)
  {
    // In the normal case we'd expect the final count for each satellite to be
    // zero. Transponder records perfectly cancel out the satellite record
    // transponder count. However, during a transition where the satellite
    // transponder count is reduced, the satellite record transponder count may
    // temporarily be less than the actual transponder record count due to the
    // presence of expired transponder records.
    if (satelliteIt->second > 0)
    {
      return false;
    }
  }
  return true;
}

void CParserNitAtsc::CleanUpRecords(vector<CRecordNit*>& records)
{
  vector<CRecordNit*>::iterator it = records.begin();
  for ( ; it != records.end(); it++)
  {
    CRecordNit* record = *it;
    if (record != NULL)
    {
      delete record;
      *it = NULL;
    }
  }
  records.clear();
}

bool CParserNitAtsc::DecodeSection(CSection& section,
                                    unsigned char& protocolVersion,
                                    unsigned char& transmissionMedium,
                                    unsigned char& tableSubtype,
                                    unsigned char& satelliteId,
                                    vector<CRecordNit*>& records,
                                    bool& seenRevisionDescriptor,
                                    unsigned char& tableVersionNumber,
                                    unsigned char& sectionNumber,
                                    unsigned char& lastSectionNumber)
{
  try
  {
    if (section.table_id != TABLE_ID_NIT_ATSC || section.SectionSyntaxIndicator)
    {
      return false;
    }
    if (section.section_length < 8)
    {
      LogDebug(L"NIT ATSC: invalid section, length = %d",
                section.section_length);
      return false;
    }

    unsigned char* data = section.Data;
    protocolVersion = data[3] & 0x1f;
    if (protocolVersion != 0)
    {
      LogDebug(L"NIT ATSC: unsupported protocol version");
      return false;
    }

    unsigned char firstIndex = data[4];
    unsigned char numberOfRecords = data[5];
    transmissionMedium = data[6] >> 4;
    tableSubtype = data[6] & 0xf;
    //LogDebug(L"NIT ATSC: protocol version = %hhu, section length = %d, number of records = %hhu, first index = %hhu, transmission medium = %hhu, table sub-type = %hhu",
    //          protocolVersion, section.section_length, numberOfRecords,
    //          firstIndex, transmissionMedium, tableSubtype);
    if (tableSubtype == 0 || tableSubtype > 4)
    {
      LogDebug(L"NIT ATSC: unsupported table sub-type");
      return false;
    }

    unsigned short pointer = 7;
    unsigned short endOfSection = section.section_length - 1;
    satelliteId = 0;
    if (tableSubtype == 4)
    {
      if (pointer >= endOfSection)
      {
        LogDebug(L"NIT ATSC: invalid section, section length = %d, pointer = %hu",
                  section.section_length, pointer);
        return false;
      }
      satelliteId = data[pointer++];
      //LogDebug(L"  satellite ID = %hhu", satelliteId);
    }

    unsigned char minimumRecordByteCount;
    if (tableSubtype == 1)
    {
      minimumRecordByteCount = MINIMUM_RECORD_BYTE_COUNT_CDS;
    }
    else if (tableSubtype == 2)
    {
      minimumRecordByteCount = MINIMUM_RECORD_BYTE_COUNT_MMS;
    }
    else if (tableSubtype == 3)
    {
      minimumRecordByteCount = MINIMUM_RECORD_BYTE_COUNT_SIS;
    }
    else
    {
      minimumRecordByteCount = MINIMUM_RECORD_BYTE_COUNT_TDS;
    }
    minimumRecordByteCount++;   // for descriptors_count
    if (pointer + (numberOfRecords * minimumRecordByteCount) > endOfSection)
    {
      LogDebug(L"NIT ATSC: invalid section, number of records = %hhu, pointer = %hu, end of section = %hu",
                numberOfRecords, pointer, endOfSection);
      return false;
    }

    for (unsigned char i = 0; i < numberOfRecords && pointer + ((numberOfRecords - i) * minimumRecordByteCount) - 1 < endOfSection; i++)
    {
      bool result = true;
      switch (tableSubtype)
      {
        case 1:
        {
          unsigned char numberOfCarriers;
          unsigned long frequencySpacing;
          unsigned long firstCarrierFrequency;
          result = DecodeCarrierDefinitionSubTable(data, pointer, endOfSection,
                                                    numberOfCarriers,
                                                    frequencySpacing,
                                                    firstCarrierFrequency);
          if (result)
          {
            for (unsigned char c = 0; c < numberOfCarriers; c++)
            {
              CRecordNitCarrierDefinition* record = new CRecordNitCarrierDefinition();
              if (record == NULL)
              {
                LogDebug(L"NIT ATSC: failed to allocate carrier definition record, number of records = %hhu, first index = %hhu, record index = %hhu, frequency index = %hhu, frequency = %lu kHz",
                          numberOfRecords, firstIndex, i, c,
                          firstCarrierFrequency);
              }
              else
              {
                record->Index = firstIndex + c;
                record->TransmissionMedium = transmissionMedium;
                record->Frequency = firstCarrierFrequency;
                records.push_back(record);
              }
              firstCarrierFrequency += frequencySpacing;
            }
          }
          break;
        }
        case 2:
        {
          CRecordNitModulationMode* record = new CRecordNitModulationMode();
          if (record == NULL)
          {
            LogDebug(L"NIT ATSC: failed to allocate modulation mode record, number of records = %hhu, first index = %hhu, index = %hhu",
                      numberOfRecords, firstIndex, i);
            result = false;
          }
          else
          {
            result = DecodeModulationModeSubTable(data, pointer, endOfSection, *record);
            if (result)
            {
              record->Index = firstIndex + i;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
            else
            {
              delete record;
            }
          }
          break;
        }
        case 3:
        {
          CRecordNitSatelliteInformation* record = new CRecordNitSatelliteInformation();
          if (record == NULL)
          {
            LogDebug(L"NIT ATSC: failed to allocate satellite information record, number of records = %hhu, first index = %hhu, index = %hhu",
                      numberOfRecords, firstIndex, i);
            result = false;
          }
          else
          {
            result = DecodeSatelliteInformationSubTable(data, pointer, endOfSection, *record);
            if (result)
            {
              record->Index = firstIndex + i;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
            else
            {
              delete record;
            }
          }
          break;
        }
        case 4:
        {
          CRecordNitTransponderData* record = new CRecordNitTransponderData();
          if (record == NULL)
          {
            LogDebug(L"NIT ATSC: failed to allocate satellite information record, satellite ID = %hhu, number of records = %hhu, first index = %hhu, index = %hhu",
                      satelliteId, numberOfRecords, firstIndex, i);
            result = false;
          }
          else
          {
            result = DecodeTransponderDataSubTable(data, pointer, endOfSection, *record);
            if (result)
            {
              record->SatelliteId = satelliteId;
              record->Index = firstIndex + i;
              record->TransmissionMedium = transmissionMedium;
              records.push_back(record);
            }
            else
            {
              delete record;
            }
          }
          break;
        }
      }

      if (pointer >= endOfSection)
      {
        LogDebug(L"NIT ATSC: invalid section, pointer = %hu, end of section = %hu, number of records = %hhu, index = %hhu",
                  pointer, endOfSection, numberOfRecords, i);
        result = false;
      }
      if (!result)
      {
        CleanUpRecords(records);
        return false;
      }

      // table descriptors
      unsigned char descriptorCount = data[pointer++];
      //LogDebug(L"NIT ATSC: record index = %hhu, descriptor count = %hhu",
      //          i, descriptorCount);

      if (pointer + ((numberOfRecords - 1 - i) * minimumRecordByteCount) + (descriptorCount * 2) > endOfSection)
      {
        LogDebug(L"NIT ATSC: invalid section, descriptor count = %hhu, pointer = %hu, end of section = %hu, number of records = %hhu, record index = %hhu",
                  descriptorCount, pointer, endOfSection, numberOfRecords, i);
        CleanUpRecords(records);
        return false;
      }

      for (unsigned char d = 0; d < descriptorCount; d++)
      {
        if (pointer + ((numberOfRecords - 1 - i) * minimumRecordByteCount) + ((descriptorCount - d) * 2) > endOfSection)
        {
          LogDebug(L"NIT ATSC: invalid section, pointer = %hu, end of section = %hu, number of records = %hhu, record index = %hhu, descriptor count = %hhu, descriptor index = %hhu",
                    pointer, endOfSection, numberOfRecords, i, descriptorCount, d);
          CleanUpRecords(records);
          return false;
        }

        unsigned char tag = data[pointer++];
        unsigned char length = data[pointer++];
        unsigned short endOfDescriptor = pointer + length;
        //LogDebug(L"NIT ATSC: table descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
        //          tag, length, pointer);
        if (endOfDescriptor > endOfSection)
        {
          LogDebug(L"NIT ATSC: invalid section, table descriptor length = %hhu, pointer = %hu, end of section = %hu, number of records = %hhu, record index = %hhu, descriptor count = %hhu, descriptor index = %hhu, tag = 0x%hhx",
                    length, pointer, endOfSection, numberOfRecords, i,
                    descriptorCount, d, tag);
          CleanUpRecords(records);
          return false;
        }

        if (tag == 0x90)
        {
          unsigned long carrierFrequency;
          if (!DecodeFrequencySpecDescriptor(&data[pointer], length, carrierFrequency))
          {
            LogDebug(L"NIT ATSC: invalid table sub-type descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, number of records = %hhu, record index = %hhu, descriptor count = %hhu, descriptor index = %hhu, end of section = %hu",
                      tag, length, pointer, numberOfRecords, i,
                      descriptorCount, d, endOfSection);
            CleanUpRecords(records);
            return false;
          }
          if (tableSubtype == 4)
          {
            CRecordNitTransponderData* record = dynamic_cast<CRecordNitTransponderData*>(records.back());
            if (record != NULL)
            {
              record->CarrierFrequencyOverride = carrierFrequency;
            }
          }
        }

        pointer = endOfDescriptor;
      }
    }

    seenRevisionDescriptor = false;
    while (pointer + 1 < endOfSection)
    {
      unsigned char tag = data[pointer++];
      unsigned char length = data[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"NIT ATSC: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (endOfDescriptor > endOfSection)
      {
        LogDebug(L"NIT ATSC: invalid section, descriptor length = %hhu, pointer = %hu, end of section = %hu, number of records = %hhu, tag = 0x%hhx",
                  length, pointer, endOfSection, numberOfRecords, tag);
        CleanUpRecords(records);
        return false;
      }

      if (tag == 0x93)
      {
        if (!DecodeRevisionDetectionDescriptor(&data[pointer], length,
                                                tableVersionNumber,
                                                sectionNumber,
                                                lastSectionNumber))
        {
          LogDebug(L"NIT ATSC: invalid descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, number of records = %hhu, end of section = %hu",
                    tag, length, pointer, numberOfRecords, endOfSection);
          CleanUpRecords(records);
          return false;
        }
        seenRevisionDescriptor = true;
      }

      pointer = endOfDescriptor;
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"NIT ATSC: section parsing error, pointer = %hu, end of section = %hu, number of records = %hhu",
                pointer, endOfSection, numberOfRecords);
      CleanUpRecords(records);
      return false;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in DecodeSection()");
  }
  return false;
}

bool CParserNitAtsc::DecodeCarrierDefinitionSubTable(unsigned char* data,
                                                      unsigned short& pointer,
                                                      unsigned short endOfSection,
                                                      unsigned char& numberOfCarriers,
                                                      unsigned long& frequencySpacing,
                                                      unsigned long& firstCarrierFrequency)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT_CDS > endOfSection)
  {
    LogDebug(L"NIT ATSC: invalid carrier definition sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    numberOfCarriers = data[pointer++];

    unsigned char spacingUnit = 10;   // kHz
    if ((data[pointer] & 0x80) != 0)
    {
      spacingUnit = 125;
    }
    frequencySpacing = ((data[pointer] & 0x3f) << 8) | data[pointer + 1];
    pointer += 2;
    frequencySpacing *= spacingUnit;  // => kHz

    unsigned char frequencyUnit = 10; // kHz
    if ((data[pointer] & 0x80) != 0)
    {
      frequencyUnit = 125;
    }
    firstCarrierFrequency = ((data[pointer] & 0x7f) << 8) | data[pointer + 1];
    pointer += 2;
    firstCarrierFrequency *= frequencyUnit;   // => kHz

    //LogDebug(L"NIT ATSC: carrier definition, number of carriers = %hhu, spacing unit = %hhu kHz, frequency spacing = %lu kHz, frequency unit = %hhu kHz, first carrier frequency = %lu kHz",
    //          numberOfCarriers, spacingUnit, frequencySpacing,
    //          frequencyUnit, firstCarrierFrequency);
    return true;
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in DecodeCarrierDefinitionSubTable()");
  }
  return false;
}

bool CParserNitAtsc::DecodeModulationModeSubTable(unsigned char* data,
                                                  unsigned short& pointer,
                                                  unsigned short endOfSection,
                                                  CRecordNitModulationMode& record)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT_MMS > endOfSection)
  {
    LogDebug(L"NIT ATSC: invalid modulation mode sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.TransmissionSystem = data[pointer] >> 4;
    record.InnerCodingMode = data[pointer++] & 0xf;
    record.SplitBitstreamMode = (data[pointer] & 0x80) != 0;
    record.ModulationFormat = data[pointer++] & 0x1f;
    record.SymbolRate = ((data[pointer] | 0xf) << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
    pointer += 4;

    //LogDebug(L"NIT ATSC: modulation mode, transmission system = %hhu, inner coding mode = %hhu, split bitstream mode = %d, modulation format = %hhu, symbol rate = %lu s/s",
    //          record.TransmissionSystem, record.InnerCodingMode,
    //          record.SplitBitstreamMode, record.ModulationFormat,
    //          record.SymbolRate);
    return true;
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in DecodeModulationModeSubTable()");
  }
  return false;
}

bool CParserNitAtsc::DecodeSatelliteInformationSubTable(unsigned char* data,
                                                        unsigned short& pointer,
                                                        unsigned short endOfSection,
                                                        CRecordNitSatelliteInformation& record)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT_SIS > endOfSection)
  {
    LogDebug(L"NIT ATSC: invalid satellite information sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.SatelliteId = data[pointer++];
    record.YouAreHere = (data[pointer] & 0x80) != 0;
    record.FrequencyBand = (data[pointer] >> 5) & 0x3;
    record.OutOfService = (data[pointer] & 0x10) != 0;
    record.Hemisphere = (data[pointer] & 0x8) >> 3;
    record.OrbitalPosition = ((data[pointer] & 0x7) << 8) | data[pointer + 1];
    pointer += 2;
    record.PolarisationType = data[pointer] >> 7;
    record.NumberOfTransponders = (data[pointer++] & 0x1f) + 1;

    //LogDebug(L"NIT ATSC: satellite information, satellite ID = %hhu, you are here = %d, frequency band = %hhu, out of service = %d, hemisphere = %hhu, orbital position = %hu, polarisation type = %hhu, number of transponders = %hhu",
    //          record.SatelliteId, record.YouAreHere, record.FrequencyBand,
    //          record.OutOfService, record.Hemisphere, record.OrbitalPosition,
    //          record.PolarisationType, record.NumberOfTransponders);
    return true;
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in DecodeSatelliteInformationSubTable()");
  }
  return false;
}

bool CParserNitAtsc::DecodeTransponderDataSubTable(unsigned char* data,
                                                    unsigned short& pointer,
                                                    unsigned short endOfSection,
                                                    CRecordNitTransponderData& record)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT_TDS > endOfSection)
  {
    LogDebug(L"NIT ATSC: invalid transport data sub-table, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.TransportType = data[pointer] >> 7;
    record.Polarisation = (data[pointer] & 0x40) >> 6;
    record.TransponderNumber = data[pointer++] & 0x3f;
    record.CdtReference = data[pointer++];

    //LogDebug(L"NIT ATSC: transponder data, transport type = %hhu, polarisation = %hhu, transponder number = %hhu, CDT reference = %hhu",
    //          record.TransportType, record.Polarisation,
    //          record.TransponderNumber, record.CdtReference);
    if (record.TransportType == 0)
    {
      record.MmtReference = data[pointer++];
      record.VctId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      record.RootTransponder = (data[pointer++] & 0x80) != 0;

      //LogDebug(L"  MMT reference = %hhu, VCT ID = %hu, root transponder = %d",
      //          record.MmtReference, record.VctId, record.RootTransponder);
    }
    else
    {
      record.WideBandwidthVideo = (data[pointer] & 0x80) != 0;
      record.WaveformStandard = data[pointer++] & 0x1f;
      record.WideBandwidthAudio = (data[pointer] & 0x80) != 0;
      record.CompandedAudio = (data[pointer] & 0x40) != 0;
      record.MatrixMode = (data[pointer] >> 4) & 0x3;
      record.Subcarrier2Offset = 10 * (((data[pointer] & 0xf) << 6) | (data[pointer + 1] >> 2));
      pointer++;
      record.Subcarrier1Offset = 10 * (((data[pointer] & 0x3) << 8) | data[pointer + 1]);
      pointer += 2;

      //LogDebug(L"  wide bandwidth video = %d, waveform standard = %hhu, widt bandwidth audio = %d, companded audio = %d, matrix mode = %hhu, subcarrier 2 offset = %hu kHz, subcarrier 1 offset = %hu kHz",
      //          record.WideBandwidthVideo, record.WaveformStandard,
      //          record.WideBandwidthAudio, record.CompandedAudio,
      //          record.MatrixMode, record.Subcarrier2Offset,
      //          record.Subcarrier1Offset);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in DecodeTransponderDataSubTable()");
  }
  return false;
}

bool CParserNitAtsc::DecodeFrequencySpecDescriptor(unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned long& carrierFrequency)
{
  if (dataLength != 2)
  {
    LogDebug(L"NIT ATSC: invalid frequency spec. descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char frequencyUnit = 10;   // kHz
    if ((data[0] & 0x80) != 0)
    {
      frequencyUnit = 125;
    }
    carrierFrequency = ((data[0] & 0x7f) << 8) | data[1];
    carrierFrequency *= frequencyUnit;  // => kHz

    //LogDebug(L"NIT ATSC: frequency spec. descriptor, frequency unit = %hhu kHz, carrier frequency = %lu kHz",
    //          frequencyUnit, carrierFrequency);
    return true;
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in DecodeFrequencySpecDescriptor()");
  }
  return false;
}

bool CParserNitAtsc::DecodeRevisionDetectionDescriptor(unsigned char* data,
                                                        unsigned char dataLength,
                                                        unsigned char& tableVersionNumber,
                                                        unsigned char& sectionNumber,
                                                        unsigned char& lastSectionNumber)
{
  if (dataLength != 3)
  {
    LogDebug(L"NIT ATSC: invalid revision detection descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    tableVersionNumber = data[0] & 0x1f;
    sectionNumber = data[1];
    lastSectionNumber = data[2];
    //LogDebug(L"NIT ATSC: revision detection descriptor, table version number = %hhu, section number = %hhu, last section number = %hhu",
    //          tableVersionNumber, sectionNumber, lastSectionNumber);
    return true;
  }
  catch (...)
  {
    LogDebug(L"NIT ATSC: unhandled exception in DecodeRevisionDetectionDescriptor()");
  }
  return false;
}