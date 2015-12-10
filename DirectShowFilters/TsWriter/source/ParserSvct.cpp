/*
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include "ParserSvct.h"
#include <algorithm>
#include <map>
#include "..\..\shared\TimeUtils.h"
#include "EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserSvct::CParserSvct(void)
  : m_recordsDefinedChannel(600000),
    m_recordsVirtualChannel(600000),
    m_recordsInverseChannel(600000)
{
  m_isReady = false;
  m_lastExpiredRecordCheckTime = 0;
  SetCallBack(NULL);
}

CParserSvct::~CParserSvct(void)
{
  SetCallBack(NULL);
}

void CParserSvct::Reset()
{
  LogDebug(L"SVCT: reset");
  CEnterCriticalSection lock(m_section);
  m_recordsDefinedChannel.RemoveAllRecords();
  m_recordsVirtualChannel.RemoveAllRecords();
  m_recordsInverseChannel.RemoveAllRecords();
  m_seenSections.clear();
  m_unseenSections.clear();
  m_isReady = false;
  LogDebug(L"SVCT: reset done");
}

void CParserSvct::SetCallBack(ICallBackSvct* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserSvct::OnNewSection(CSection& section)
{
  try
  {
    unsigned char protocolVersion;
    unsigned char transmissionMedium;
    unsigned char tableSubtype;
    unsigned short vctId;
    vector<CRecordSvct*> records;
    bool seenRevisionDescriptor;
    unsigned char tableVersionNumber;
    unsigned char sectionNumber;
    unsigned char lastSectionNumber;
    if (!DecodeSection(section,
                        protocolVersion,
                        transmissionMedium,
                        tableSubtype,
                        vctId,
                        records,
                        seenRevisionDescriptor,
                        tableVersionNumber,
                        sectionNumber,
                        lastSectionNumber))
    {
      if (seenRevisionDescriptor)
      {
        LogDebug(L"SVCT: invalid section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu, version number = %hhu, section number = %hhu",
                  protocolVersion, transmissionMedium, tableSubtype, vctId,
                  tableVersionNumber, sectionNumber);
      }
      else
      {
        LogDebug(L"SVCT: invalid section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu",
                  protocolVersion, transmissionMedium, tableSubtype, vctId);
      }
      return;
    }

    CRecordStore* recordSet;
    if (tableSubtype == 0)
    {
      recordSet = &m_recordsVirtualChannel;
    }
    else if (tableSubtype == 1)
    {
      recordSet = &m_recordsDefinedChannel;
    }
    else
    {
      recordSet = &m_recordsInverseChannel;
    }

    CEnterCriticalSection lock(m_section);
    unsigned long sectionGroupKey;
    if (!seenRevisionDescriptor)
    {
      // Assume that there is more than one section for each sub-table type.
      // Therefore we have to let records expire naturally.
      if (recordSet->GetRecordCount() == 0)
      {
        LogDebug(L"SVCT: received, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu",
                  protocolVersion, transmissionMedium, tableSubtype, vctId);
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
        //LogDebug(L"SVCT: previously seen section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu, section number = %hhu",
        //          protocolVersion, transmissionMedium, tableSubtype, vctId,
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
          LogDebug(L"SVCT: received, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu",
                    protocolVersion, transmissionMedium, tableSubtype, vctId,
                    tableVersionNumber, sectionNumber, lastSectionNumber);
        }
        else
        {
          LogDebug(L"SVCT: changed, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu",
                    protocolVersion, transmissionMedium, tableSubtype, vctId,
                    tableVersionNumber, sectionNumber, lastSectionNumber);
          recordSet->MarkExpiredRecords((transmissionMedium << 16) | vctId);
          if (m_isReady && m_callBack != NULL)
          {
            m_isReady = false;
            m_callBack->OnTableChange(TABLE_ID_SVCT);
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
        //LogDebug(L"SVCT: new section, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu, version number = %hhu, section number = %hhu",
        //            protocolVersion, transmissionMedium, tableSubtype, vctId,
        //            tableVersionNumber, sectionNumber);
      }

      m_seenSections.push_back(sectionKey);
      m_unseenSections.erase(sectionIt);
    }

    if (
      m_callBack != NULL &&
      m_recordsVirtualChannel.GetRecordCount() == 0 &&
      m_recordsDefinedChannel.GetRecordCount() == 0 &&
      m_recordsInverseChannel.GetRecordCount() == 0
    )
    {
      m_callBack->OnTableSeen(TABLE_ID_SVCT);
    }

    unsigned long newOrChangedRecordCount = 0;
    vector<CRecordSvct*>::const_iterator recordIt = records.begin();
    for ( ; recordIt != records.end(); recordIt++)
    {
      CRecordSvct* record = *recordIt;
      if (recordSet->AddOrUpdateRecord((IRecord**)&record, m_callBack))
      {
        newOrChangedRecordCount++;
      }
    }

    // Are we ready?
    if (IsReadyPrivate())
    {
      unsigned long nonCurrentRecordCount = 0;
      if (
        m_seenSections.size() > 0 ||
        (
          m_isReady &&
          (
            newOrChangedRecordCount > 0 ||
            CTimeUtils::ElapsedMillis(m_lastExpiredRecordCheckTime) >= 10000
          )
        )
      )
      {
        nonCurrentRecordCount = m_recordsDefinedChannel.RemoveExpiredRecords(m_callBack);
        nonCurrentRecordCount += m_recordsVirtualChannel.RemoveExpiredRecords(m_callBack);
        nonCurrentRecordCount += m_recordsInverseChannel.RemoveExpiredRecords(m_callBack);
        m_lastExpiredRecordCheckTime = clock();
      }

      // Did something actually change?
      if (m_seenSections.size() > 0 || nonCurrentRecordCount > 0 || newOrChangedRecordCount > 0)
      {
        LogDebug(L"SVCT: ready, sections parsed = %llu, defined channel count = %lu, virtual channel count = %lu, inverse channel count = %lu",
                  (unsigned long long)m_seenSections.size(),
                  m_recordsDefinedChannel.GetRecordCount(),
                  m_recordsVirtualChannel.GetRecordCount(),
                  m_recordsInverseChannel.GetRecordCount());
        m_isReady = true;
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_SVCT);
        }
      }
    }
    else if (m_isReady)
    {
      LogDebug(L"SVCT: changed, protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu",
                protocolVersion, transmissionMedium, tableSubtype, vctId);
      m_isReady = false;
      if (m_callBack != NULL)
      {
        m_callBack->OnTableChange(TABLE_ID_SVCT);
      }
    }
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in OnNewSection()");
  }
}

bool CParserSvct::IsSeen() const
{
  CEnterCriticalSection lock(m_section);
  return
    m_seenSections.size() > 0 ||
    m_recordsDefinedChannel.GetRecordCount() > 0 ||
    m_recordsVirtualChannel.GetRecordCount() > 0 ||
    m_recordsInverseChannel.GetRecordCount() > 0;
}

bool CParserSvct::IsReady() const
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

unsigned short CParserSvct::GetDefinedChannelCount() const
{
  CEnterCriticalSection lock(m_section);
  return (unsigned short)m_recordsDefinedChannel.GetRecordCount();
}

bool CParserSvct::GetDefinedChannel(unsigned short index,
                                    unsigned char& transmissionMedium,
                                    unsigned short& vctId,
                                    unsigned short& virtualChannelNumber) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_recordsDefinedChannel.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"SVCT: invalid defined channel index, index = %hu, record count = %lu",
              index, m_recordsDefinedChannel.GetRecordCount());
    return false;
  }

  CRecordSvctDefinedChannel* recordSvct = dynamic_cast<CRecordSvctDefinedChannel*>(record);
  if (recordSvct == NULL)
  {
    LogDebug(L"SVCT: invalid defined channel record, index = %hu", index);
    return false;
  }
  transmissionMedium = recordSvct->TransmissionMedium;
  vctId = recordSvct->VctId;
  virtualChannelNumber = recordSvct->VirtualChannelNumber;
  return true;
}

unsigned short CParserSvct::GetVirtualChannelCount() const
{
  CEnterCriticalSection lock(m_section);
  return (unsigned short)m_recordsVirtualChannel.GetRecordCount();
}

bool CParserSvct::GetVirtualChannel(unsigned short index,
                                    unsigned char& transmissionMedium,
                                    unsigned short& vctId,
                                    bool& splice,
                                    unsigned long& activationTime,
                                    bool& hdtvChannel,
                                    bool& preferredSource,
                                    unsigned short& virtualChannelNumber,
                                    bool& applicationVirtualChannel,
                                    unsigned char& bitstreamSelect,
                                    unsigned char& pathSelect,
                                    unsigned char& toneSelect,
                                    unsigned char& transportType,
                                    unsigned char& channelType,
                                    unsigned short& sourceId,
                                    unsigned short& nvodChannelBase,
                                    unsigned char& cdtReference,
                                    unsigned short& programNumber,
                                    unsigned char& mmtReference,
                                    bool& scrambled,
                                    unsigned char& videoStandard,
                                    bool& wideBandwidthAudio,
                                    bool& compandedAudio,
                                    unsigned char& matrixMode,
                                    unsigned short& subcarrier2Offset,
                                    unsigned short& subcarrier1Offset,
                                    unsigned char& satelliteId,
                                    unsigned char& transponder,
                                    bool& suppressVideo,
                                    unsigned char& audioSelection,
                                    unsigned long& carrierFrequencyOverride,
                                    unsigned long& symbolRateOverride,
                                    unsigned short& majorChannelNumber,
                                    unsigned short& minorChannelNumber,
                                    unsigned short& transportStreamId,
                                    bool& outOfBand,
                                    bool& hideGuide,
                                    unsigned char& serviceType) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_recordsVirtualChannel.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"SVCT: invalid virtual channel index, index = %hu, record count = %lu",
              index, m_recordsVirtualChannel.GetRecordCount());
    return false;
  }

  CRecordSvctVirtualChannel* recordSvct = dynamic_cast<CRecordSvctVirtualChannel*>(record);
  if (recordSvct == NULL)
  {
    LogDebug(L"SVCT: invalid virtual channel record, index = %hu", index);
    return false;
  }
  transmissionMedium = recordSvct->TransmissionMedium;
  vctId = recordSvct->VctId;
  splice = recordSvct->Splice;
  activationTime = recordSvct->ActivationTime;
  hdtvChannel = recordSvct->HdtvChannel;
  preferredSource = recordSvct->PreferredSource;
  virtualChannelNumber = recordSvct->VirtualChannelNumber;
  applicationVirtualChannel = recordSvct->ApplicationVirtualChannel;
  bitstreamSelect = recordSvct->BitstreamSelect;
  pathSelect = recordSvct->PathSelect;
  toneSelect = recordSvct->ToneSelect;
  transportType = recordSvct->TransportType;
  channelType = recordSvct->ChannelType;
  sourceId = recordSvct->SourceId;
  nvodChannelBase = recordSvct->NvodChannelBase;
  cdtReference = recordSvct->CdtReference;
  programNumber = recordSvct->ProgramNumber;
  mmtReference = recordSvct->MmtReference;
  scrambled = recordSvct->Scrambled;
  videoStandard = recordSvct->VideoStandard;
  wideBandwidthAudio = recordSvct->WideBandwidthAudio;
  compandedAudio = recordSvct->CompandedAudio;
  matrixMode = recordSvct->MatrixMode;
  subcarrier2Offset = recordSvct->Subcarrier2Offset;
  subcarrier1Offset = recordSvct->Subcarrier1Offset;
  satelliteId = recordSvct->SatelliteId;
  transponder = recordSvct->Transponder;
  suppressVideo = recordSvct->SuppressVideo;
  audioSelection = recordSvct->AudioSelection;
  carrierFrequencyOverride = recordSvct->CarrierFrequencyOverride;
  symbolRateOverride = recordSvct->SymbolRateOverride;
  majorChannelNumber = recordSvct->MajorChannelNumber;
  minorChannelNumber = recordSvct->MinorChannelNumber;
  transportStreamId = recordSvct->TransportStreamId;
  outOfBand = recordSvct->OutOfBand;
  hideGuide = recordSvct->HideGuide;
  serviceType = recordSvct->ServiceType;
  return true;
}

unsigned short CParserSvct::GetInverseChannelCount() const
{
  CEnterCriticalSection lock(m_section);
  return (unsigned short)m_recordsInverseChannel.GetRecordCount();
}

bool CParserSvct::GetInverseChannel(unsigned short index,
                                    unsigned char& transmissionMedium,
                                    unsigned short& vctId,
                                    unsigned short& sourceId,
                                    unsigned short& virtualChannelNumber) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_recordsInverseChannel.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"SVCT: invalid inverse channel index, index = %hu, record count = %lu",
              index, m_recordsInverseChannel.GetRecordCount());
    return false;
  }

  CRecordSvctInverseChannel* recordSvct = dynamic_cast<CRecordSvctInverseChannel*>(record);
  if (recordSvct == NULL)
  {
    LogDebug(L"SVCT: invalid inverse channel record, index = %hu", index);
    return false;
  }
  transmissionMedium = recordSvct->TransmissionMedium;
  vctId = recordSvct->VctId;
  sourceId = recordSvct->SourceId;
  virtualChannelNumber = recordSvct->VirtualChannelNumber;
  return true;
}

bool CParserSvct::IsReadyPrivate() const
{
  // The DCM and VCM are mandatory and should be correlated. We don't care
  // the optional ICM.
  unsigned long recordCountVc = m_recordsVirtualChannel.GetRecordCount();
  unsigned long recordCountDc = m_recordsDefinedChannel.GetRecordCount();
  if (
    m_unseenSections.size() > 0 ||
    recordCountDc == 0 ||
    recordCountVc == 0
  )
  {
    return false;
  }

  if (m_seenSections.size() > 0)
  {
    return true;
  }

  // Revision detection descriptors not used.
  // We assume the maps/tables are complete when VCM records have been received
  // for all defined channels. The meaning of "defined" doesn't seem to be
  // specified by standards. We support two interpretations:
  // 1. DCM set matches VCM set.
  // 2. VCM set is a superset of the DCM and hidden VCM sets.
  if (recordCountVc < recordCountDc)
  {
    return false;
  }

  // Create a map of the virtual channel numbers that we know about.
  map<unsigned long long, bool> virtualChannels;
  bool requireDefinedHiddenChannels = recordCountVc == recordCountDc;
  for (unsigned long i = 0; i < recordCountVc; i++)
  {
    IRecord* record = NULL;
    if (m_recordsVirtualChannel.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordSvctVirtualChannel* recordSvct = dynamic_cast<CRecordSvctVirtualChannel*>(record);
      if (recordSvct != NULL && (requireDefinedHiddenChannels || !recordSvct->ChannelType == 1))
      {
        unsigned long long key = ((unsigned long long)recordSvct->TransmissionMedium << 48) | ((unsigned long long)recordSvct->VctId << 32) | recordSvct->VirtualChannelNumber;
        virtualChannels[key] = false;
      }
    }
  }

  // Mark the virtual channel entries that also have defined channel map entries.
  for (unsigned long i = 0; i < recordCountDc; i++)
  {
    IRecord* record = NULL;
    if (m_recordsDefinedChannel.GetRecordByIndex(i, &record) && record != NULL)
    {
      CRecordSvctDefinedChannel* recordSvct = dynamic_cast<CRecordSvctDefinedChannel*>(record);
      if (recordSvct != NULL)
      {
        unsigned long long key = ((unsigned long long)recordSvct->TransmissionMedium << 48) | ((unsigned long long)recordSvct->VctId << 32) | recordSvct->VirtualChannelNumber;
        map<unsigned long long, bool>::iterator it = virtualChannels.find(key);
        if (it == virtualChannels.end())
        {
          // We have a defined channel map entry without a corresponding
          // virtual channel. That means we're not finished.
          return false;
        }
        it->second = true;
      }
    }
  }

  // Finally, check that all virtual channel entries have been marked.
  map<unsigned long long, bool>::const_iterator it = virtualChannels.begin();
  for ( ; it != virtualChannels.end(); it++)
  {
    if (!(it->second))
    {
      return false;
    }
  }
  return true;
}

template<class T> void CParserSvct::CleanUpRecords(vector<T*>& records)
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

bool CParserSvct::DecodeSection(CSection& section,
                                unsigned char& protocolVersion,
                                unsigned char& transmissionMedium,
                                unsigned char& tableSubtype,
                                unsigned short& vctId,
                                vector<CRecordSvct*>& records,
                                bool& seenRevisionDescriptor,
                                unsigned char& tableVersionNumber,
                                unsigned char& sectionNumber,
                                unsigned char& lastSectionNumber)
{
  try
  {
    if (
      section.table_id != TABLE_ID_SVCT ||
      section.SectionSyntaxIndicator ||
      section.PrivateIndicator
    )
    {
      return false;
    }
    if (section.section_length < 8)
    {
      LogDebug(L"SVCT: invalid section, length = %d", section.section_length);
      return false;
    }

    unsigned char* data = section.Data;
    protocolVersion = data[3] & 0x1f;
    if (protocolVersion != 0)
    {
      LogDebug(L"SVCT: unsupported protocol version");
      return false;
    }

    transmissionMedium = data[4] >> 4;
    tableSubtype = data[4] & 0xf;
    vctId = (data[5] << 8) | data[6];
    //LogDebug(L"SVCT: protocol version = %hhu, transmission medium = %hhu, table sub-type = %hhu, VCT ID = %hu, section length = %d",
    //          protocolVersion, transmissionMedium, tableSubtype, vctId,
    //          section.section_length);

    unsigned short pointer = 7;
    unsigned short endOfSection = section.section_length - 1;
    bool result = true;
    switch (tableSubtype)
    {
      case 0:
      {
        vector<CRecordSvctVirtualChannel*> recordsVirtualChannel;
        result = DecodeVirtualChannelMap(&data[pointer],
                                          pointer,
                                          endOfSection,
                                          transmissionMedium,
                                          recordsVirtualChannel);
        if (result)
        {
          vector<CRecordSvctVirtualChannel*>::const_iterator recordIt = recordsVirtualChannel.begin();
          for ( ; recordIt != recordsVirtualChannel.end(); recordIt++)
          {
            CRecordSvctVirtualChannel* record = *recordIt;
            if (record != NULL)
            {
              record->TransmissionMedium = transmissionMedium;
              record->VctId = vctId;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 1:
      {
        vector<CRecordSvctDefinedChannel*> recordsDefinedChannel;
        result = DecodeDefinedChannelMap(&data[pointer],
                                          pointer,
                                          endOfSection,
                                          recordsDefinedChannel);
        if (result)
        {
          vector<CRecordSvctDefinedChannel*>::const_iterator recordIt = recordsDefinedChannel.begin();
          for ( ; recordIt != recordsDefinedChannel.end(); recordIt++)
          {
            CRecordSvctDefinedChannel* record = *recordIt;
            if (record != NULL)
            {
              record->TransmissionMedium = transmissionMedium;
              record->VctId = vctId;
              records.push_back(record);
            }
          }
        }
        break;
      }
      case 2:
      {
        vector<CRecordSvctInverseChannel*> recordsInverseChannel;
        result = DecodeInverseChannelMap(&data[pointer],
                                          pointer,
                                          endOfSection,
                                          recordsInverseChannel);
        if (result)
        {
          vector<CRecordSvctInverseChannel*>::const_iterator recordIt = recordsInverseChannel.begin();
          for ( ; recordIt != recordsInverseChannel.end(); recordIt++)
          {
            CRecordSvctInverseChannel* record = *recordIt;
            if (record != NULL)
            {
              record->TransmissionMedium = transmissionMedium;
              record->VctId = vctId;
              records.push_back(record);
            }
          }
        }
        break;
      }
      default:
        LogDebug(L"SVCT: unsupported table sub-type");
        return false;
    }

    if (!result)
    {
      return false;
    }

    seenRevisionDescriptor = false;
    while (pointer + 1 < endOfSection)
    {
      unsigned char tag = data[pointer++];
      unsigned char length = data[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"SVCT: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (endOfDescriptor > endOfSection)
      {
        LogDebug(L"SVCT: invalid section, descriptor length = %hhu, pointer = %hu, end of section = %hu, tag = 0x%hhx",
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
          LogDebug(L"SVCT: invalid descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of section = %hu",
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
      LogDebug(L"SVCT: section parsing error, pointer = %hu, end of section = %hu",
                pointer, endOfSection);
      CleanUpRecords(records);
      return false;
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeSection()");
  }
  return false;
}

bool CParserSvct::DecodeDefinedChannelMap(unsigned char* data,
                                          unsigned short& pointer,
                                          unsigned short endOfSection,
                                          vector<CRecordSvctDefinedChannel*>& records)
{
  if (pointer + 3 > endOfSection)
  {
    LogDebug(L"SVCT: invalid defined channel map, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    unsigned short firstVirtualChannel = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
    pointer += 2;
    unsigned char dcmDataLength = data[pointer++] & 0x7f;
    //LogDebug(L"SVCT: defined channel map, first virtual channel = %hu, DCM data length = %hhu",
    //          firstVirtualChannel, dcmDataLength);
    if (pointer + dcmDataLength > endOfSection)
    {
      LogDebug(L"SVCT: invalid defined channel map, DCM data length = %hhu, pointer = %hu, end of section = %hu",
                dcmDataLength, pointer, endOfSection);
      return false;
    }

    unsigned short currentChannel = firstVirtualChannel;
    for (unsigned char i = 0; i < dcmDataLength; i++)
    {
      bool rangeDefined = (data[pointer] & 0x80) != 0;
      unsigned char channelsCount = data[pointer++] & 0x7f;
      //LogDebug(L"  range defined = %d, channels count = %hhu", rangeDefined, channelsCount);
      if (rangeDefined)
      {
        for (unsigned char c = 0; c < channelsCount; c++)
        {
          CRecordSvctDefinedChannel* record = new CRecordSvctDefinedChannel();
          if (record == NULL)
          {
            LogDebug(L"SVCT: failed to allocate defined channel record, DCM data length = %hhu, DCM index = %hhu, channels count = %hhu, channel index = %hhu, virtual channel number = %hu",
                      dcmDataLength, i, channelsCount, c, currentChannel);
            currentChannel++;
            continue;
          }
          record->VirtualChannelNumber = currentChannel++;
        }
      }
      else
      {
        currentChannel += channelsCount;
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeDefinedChannelMap()");
  }
  return false;
}

bool CParserSvct::DecodeVirtualChannelMap(unsigned char* data,
                                          unsigned short& pointer,
                                          unsigned short endOfSection,
                                          unsigned char transmissionMedium,
                                          vector<CRecordSvctVirtualChannel*>& records)
{
  if (pointer + 7 > endOfSection)
  {
    LogDebug(L"SVCT: invalid virtual channel map, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    bool freqSpecIncluded = (data[pointer] & 0x80) != 0;      // A/56: yes; SCTE 57: no; SCTE 65: zero
    bool symbolRateIncluded = (data[pointer] & 0x40) != 0;    // A/56: yes; SCTE 57: no; SCTE 65: zero
    bool descriptorsIncluded = (data[pointer++] & 0x20) != 0;
    bool splice = (data[pointer++] & 0x80) != 0;              // A/56: yes; SCTE 57: no; SCTE 65: yes
    unsigned long activationTime = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
    pointer += 4;
    unsigned char numberOfVcRecords = data[pointer++];
    //LogDebug(L"SVCT: virtual channel map, freq. spec. included = %d, symbol rate included = %d, descriptors included = %d, splice = %d, activation time = %lu, number of VC records = %hhu",
    //          freqSpecIncluded, symbolRateIncluded, descriptorsIncluded, splice, activationTime, numberOfVcRecords);

    unsigned char recordByteCount = 9;
    if (transmissionMedium == 3)
    {
      recordByteCount = 10;
    }
    if (transmissionMedium != 4)
    {
      if (freqSpecIncluded)
      {
        recordByteCount += 2;
      }
      if (symbolRateIncluded)
      {
        recordByteCount += 4;
      }
    }
    if (descriptorsIncluded)
    {
      recordByteCount++;
    }
    if (pointer + (numberOfVcRecords * recordByteCount) > endOfSection)
    {
      LogDebug(L"SVCT: invalid virtual channel map, number of VC records = %hhu, transmission medium = %hhu, freq. spec. included = %d, symbol rate included = %d, descriptors included = %d, record byte count = %hhu, pointer = %hu, end of section = %hu",
                numberOfVcRecords, transmissionMedium, freqSpecIncluded,
                symbolRateIncluded, descriptorsIncluded, recordByteCount,
                pointer, endOfSection);
      return false;
    }

    for (unsigned char i = 0; i < numberOfVcRecords && pointer + ((numberOfVcRecords - i) * recordByteCount) - 1 < endOfSection; i++)
    {
      CRecordSvctVirtualChannel* record = new CRecordSvctVirtualChannel();
      if (record == NULL)
      {
        LogDebug(L"SVCT: failed to allocate virtual channel record, number of VC records = %hhu, VC index = %hhu",
                  numberOfVcRecords, i);
        CleanUpRecords(records);
        return false;
      }
      record->Splice = splice;
      record->ActivationTime = activationTime;

      record->HdtvChannel = (data[pointer] & 0x80) != 0;          // A/56: no; SCTE 57: yes [satellite, cable, MMDS]; SCTE 65: zero
      record->PreferredSource = (data[pointer] & 0x10) != 0;      // A/56: no; SCTE 57: yes [cable, MMDS]; SCTE 65: zero
      record->VirtualChannelNumber = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
      pointer += 2;

      record->ApplicationVirtualChannel = (data[pointer] & 0x80) != 0;
      record->BitstreamSelect = (data[pointer] & 0x40) >> 6;      // A/56: yes [satellite, SMATV, cable, MMDS]; SCTE 57: yes [satellite, SMATV, cable, MMDS]; SCTE 65: zero
      record->PathSelect = (data[pointer] & 0x20) >> 5;           // A/56: yes [cable, MMDS]; SCTE 57: yes [cable, MMDS]; SCTE 65: yes
      record->ToneSelect = record->PathSelect;                    // A/56: no; SCTE 57: yes [satellite]; SCTE 65: no
      record->TransportType = (data[pointer] & 0x10) >> 4;
      record->ChannelType = data[pointer++] & 0xf;

      record->SourceId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;

      //LogDebug(L"  HDTV channel = %d, preferred source = %d, virtual channel number = %hu, application virtual channel = %d, bitstream select = %hhu, path select = %hhu, transport type = %hhu, channel type = %hhu, source ID = %hu",
      //          record->HdtvChannel, record->PreferredSource,
      //          record->VirtualChannelNumber,
      //          record->ApplicationVirtualChannel, record->BitstreamSelect,
      //          record->PathSelect, record->TransportType,
      //          record->ChannelType, record->SourceId);

      if (record->ChannelType == 3) // NVOD access
      {
        record->NvodChannelBase = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
        pointer += 2;
        if (transmissionMedium == 3)      // SMATV
        {
          pointer += 3;
        }
        else if (transmissionMedium != 4) // everything else except over the air (broadcast)
        {
          pointer += 2;
        }
        //LogDebug(L"    NVOD channel base = %hu", record->NvodChannelBase);
      }
      else
      {
        switch (transmissionMedium)
        {
          case 0: // cable: A/56, SCTE 57, SCTE 65
          case 2: // MMDS: A/56, SCTE 57
          case 3: // SMATV: A/56, SCTE 57
            record->CdtReference = data[pointer++];
            if (record->TransportType == 0) // MPEG 2
            {
              record->ProgramNumber = (data[pointer] << 8) | data[pointer + 1];
              pointer += 2;
              record->MmtReference = data[pointer++];
              if (transmissionMedium == 3)  // SMATV
              {
                pointer++;
              }
              //LogDebug(L"    CDT reference = %hhu, program number = %hu, MMT reference = %hhu",
              //          record->CdtReference, record->ProgramNumber, record->MmtReference);
            }
            else
            {
              record->Scrambled = (data[pointer] & 0x80) != 0;
              record->VideoStandard = data[pointer++] & 0xf;
              if (transmissionMedium == 3)  // SMATV
              {
                record->WideBandwidthAudio = (data[pointer] & 0x80) != 0;
                record->CompandedAudio = (data[pointer] & 0x40) != 0;
                record->MatrixMode = (data[pointer] >> 4) & 0x3;
                record->Subcarrier2Offset = 10 * (((data[pointer] & 0xf) << 6) | (data[pointer + 1] >> 2));  // kHz
                pointer++;
                record->Subcarrier1Offset = 10 * (((data[pointer] & 0x3) << 8) | data[pointer + 1]);
                pointer += 2;
                //LogDebug(L"    CDT reference = %hhu, scrambled = %d, video standard = %hhu, wide bandwidth audio = %d, companded audio = %d, matrix mode = %hhu, subcarrier 2 offset = %hu kHz, subcarrier 1 offset = %hu kHz",
                //          record->CdtReference, record->Scrambled,
                //          record->VideoStandard, record->WideBandwidthAudio,
                //          record->CompandedAudio, record->MatrixMode,
                //          record->Subcarrier2Offset, record->Subcarrier1Offset);
              }
              else
              {
                pointer += 2;
              }
            }
            break;
          case 1: // satellite: A/56, SCTE 57
            record->SatelliteId = data[pointer++];
            record->Transponder = data[pointer++] & 0x3f;
            if (record->TransportType == 0) // MPEG 2
            {
              record->ProgramNumber = (data[pointer] << 8) | data[pointer + 1];
              pointer += 2;
              //LogDebug(L"    satellite ID = %hhu, transponder = %hhu, program number = %hu",
              //          record->SatelliteId, record->Transponder, record->ProgramNumber);
            }
            else
            {
              record->SuppressVideo = (data[pointer++] & 0x80) != 0;   // A/56: no; SCTE 57: yes
              record->AudioSelection = data[pointer++] & 0x3; // A/56: no; SCTE 57: yes
              //LogDebug(L"    satellite ID = %hhu, transponder = %hhu, suppress video = %d, audio selection = %hhu",
              //          record->SatelliteId, record->Transponder,
              //          record->SuppressVideo, record->AudioSelection);
            }
            break;
          case 4: // over the air: A/56
            if (record->TransportType == 0) // MPEG 2
            {
              record->ProgramNumber = (data[pointer] << 8) | data[pointer + 1];
              pointer += 2;
              //LogDebug(L"    program number = %hu", programNumber);
            }
            else
            {
              record->Scrambled = (data[pointer] & 0x80) != 0;
              record->VideoStandard = data[pointer++] & 0xf;
              pointer++;
              //LogDebug(L"    scrambled = %d, video standard = %hhu",
              //          record->Scrambled, record->VideoStandard);
            }
            break;
        }
      }

      if (freqSpecIncluded || transmissionMedium == 4)  // over the air
      {
        unsigned char frequencyUnit = 10; // kHz
        if ((data[pointer] & 0x80) != 0)
        {
          frequencyUnit = 125;  // kHz
        }
        record->CarrierFrequencyOverride = frequencyUnit * (((data[pointer] & 0x7f) << 8) | data[pointer + 1]);  // kHz
        pointer += 2;
        //LogDebug(L"SVCT: included frequency, unit = %hhu kHz, carrier frequency = %lu kHz",
        //          frequencyUnit, record->CarrierFrequencyOverride);
      }

      if (symbolRateIncluded && transmissionMedium != 4)  // not over the air
      {
        // s/s
        record->SymbolRateOverride = ((data[pointer] & 0xf) << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;
        //LogDebug(L"SVCT: included symbol rate, symbol rate = %lu s/s",
        //          record->SymbolRateOverride);
      }

      if (descriptorsIncluded)
      {
        unsigned char descriptorCount = data[pointer++];

        if (pointer + ((numberOfVcRecords - 1 - i) * recordByteCount) + (descriptorCount * 2) >= endOfSection)
        {
          LogDebug(L"SVCT: invalid virtual channel map, descriptor count = %hhu, pointer = %hu, number of VC records = %hhu, VC index = %hhu, virtual channel number = %hu, source ID = %hu, channel type = %hhu, transport type = %hhu, freq. spec. included = %d, symbol rate included = %d, end of section = %hu",
                    descriptorCount, pointer, numberOfVcRecords, i,
                    record->VirtualChannelNumber, record->SourceId,
                    record->ChannelType, record->TransportType,
                    freqSpecIncluded, symbolRateIncluded, endOfSection);
          CleanUpRecords(records);
          return false;
        }

        for (unsigned char d = 0; d < descriptorCount && pointer + ((numberOfVcRecords - 1 - i) * recordByteCount) + ((descriptorCount - d) * 2) - 1 < endOfSection; d++)
        {
          unsigned char tag = data[pointer++];
          unsigned char length = data[pointer++];
          //LogDebug(L"SVCT: virtual channel map descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
          //          tag, length, pointer);
          if (pointer + length > endOfSection)
          {
            LogDebug(L"SVCT: invalid virtual channel map, descriptor length = %hhu, pointer = %hu, number of VC records = %hhu, VC index = %hhu, virtual channel number = %hu, source ID = %hu, channel type = %hhu, transport type = %hhu, freq. spec. included = %d, symbol rate included = %d, descriptor count = %hhu, descriptor index = %hhu, tag = 0x%hhx, end of section = %hu",
                      length, pointer, numberOfVcRecords, i,
                      record->VirtualChannelNumber, record->SourceId,
                      record->ChannelType, record->TransportType,
                      freqSpecIncluded, symbolRateIncluded, descriptorCount, d,
                      pointer, tag, endOfSection);
            CleanUpRecords(records);
            return false;
          }

          bool descriptorParseResult = true;
          if (tag == 0x90)      // frequency spec. descriptor
          {
            descriptorParseResult = DecodeFrequencySpecDescriptor(&data[pointer],
                                                                  length,
                                                                  record->CarrierFrequencyOverride);
          }
          else if (tag == 0x94) // two part channel number descriptor
          {
            descriptorParseResult = DecodeTwoPartChannelNumberDescriptor(&data[pointer],
                                                                          length,
                                                                          record->MajorChannelNumber,
                                                                          record->MinorChannelNumber);
          }
          else if (tag == 0x95) // channel properties descriptor
          {
            descriptorParseResult = DecodeChannelPropertiesDescriptor(&data[pointer],
                                                                      length,
                                                                      record->TransportStreamId,
                                                                      record->OutOfBand,
                                                                      record->Scrambled,
                                                                      record->HideGuide,
                                                                      record->ServiceType);
          }

          if (!descriptorParseResult)
          {
            LogDebug(L"SVCT: invalid virtual channel map descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, number of VC records = %hhu, VC index = %hhu, virtual channel number = %hu, source ID = %hu, channel type = %hhu, transport type = %hhu, freq. spec. included = %d, symbol rate included = %d, descriptor count = %hhu, descriptor index = %hhu",
                      tag, length, pointer, numberOfVcRecords, i,
                      record->VirtualChannelNumber, record->SourceId,
                      record->ChannelType, record->TransportType,
                      freqSpecIncluded, symbolRateIncluded, descriptorCount, d);
            CleanUpRecords(records);
            return false;
          }

          pointer += length;
        }
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeVirtualChannelMap()");
  }
  return false;
}

bool CParserSvct::DecodeInverseChannelMap(unsigned char* data,
                                          unsigned short& pointer,
                                          unsigned short endOfSection,
                                          vector<CRecordSvctInverseChannel*>& records)
{
  if (pointer + 3 > endOfSection)
  {
    LogDebug(L"SVCT: invalid inverse channel map, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    unsigned short firstMapIndex = ((data[pointer] & 0xf) << 8) | data[pointer + 1];    // A/56: no; SCTE 57: yes; SCTE 65: yes
    pointer += 2;
    unsigned char recordCount = (data[pointer++] & 0x7f);
    //LogDebug(L"SVCT: inverse channel map, first map index = %hu, record count = %hhu",
    //          firstMapIndex, recordCount);

    if (pointer + (recordCount * 4) > endOfSection)
    {
      LogDebug(L"SVCT: invalid inverse channel map, record count = %hhu, pointer = %hu, end of section = %hu",
                recordCount, pointer, endOfSection);
      return false;
    }

    for (unsigned char i = 0; i < recordCount; i++)
    {
      unsigned short sourceId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned short virtualChannelNumber = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
      pointer += 2;
      //LogDebug(L"  source ID = %hu, virtual channel number = %hu",
      //          sourceId, virtualChannelNumber);

      CRecordSvctInverseChannel* record = new CRecordSvctInverseChannel();
      if (record == NULL)
      {
        LogDebug(L"SVCT: failed to allocate inverse channel record, record count = %hhu, record index = %hhu, source ID = %hu, virtual channel number = %hu",
                  recordCount, i, sourceId, virtualChannelNumber);
        continue;
      }
      record->SourceId = sourceId;
      record->VirtualChannelNumber = virtualChannelNumber;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeInverseChannelMap()");
  }
  return false;
}

bool CParserSvct::DecodeFrequencySpecDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                unsigned long& carrierFrequency)
{
  if (dataLength != 2)
  {
    LogDebug(L"SVCT: invalid frequency spec. descriptor, length = %hhu",
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

    //LogDebug(L"SVCT: frequency spec. descriptor, frequency unit = %hhu kHz, carrier frequency = %lu kHz",
    //          frequencyUnit, carrierFrequency);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeFrequencySpecDescriptor()");
  }
  return false;
}

bool CParserSvct::DecodeRevisionDetectionDescriptor(unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned char& tableVersionNumber,
                                                    unsigned char& sectionNumber,
                                                    unsigned char& lastSectionNumber)
{
  if (dataLength != 3)
  {
    LogDebug(L"SVCT: invalid revision detection descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    tableVersionNumber = data[0] & 0x1f;
    sectionNumber = data[1];
    lastSectionNumber = data[2];
    //LogDebug(L"SVCT: revision detection descriptor, table version number = %hhu, section number = %hhu, last section number = %hhu",
    //          tableVersionNumber, sectionNumber, lastSectionNumber);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeRevisionDetectionDescriptor()");
  }
  return false;
}

bool CParserSvct::DecodeTwoPartChannelNumberDescriptor(unsigned char* data,
                                                        unsigned char dataLength,
                                                        unsigned short& majorChannelNumber,
                                                        unsigned short& minorChannelNumber)
{
  if (dataLength != 4)
  {
    LogDebug(L"SVCT: invalid two part channel number descriptor, length = %hhu", dataLength);
    return false;
  }
  try
  {
    majorChannelNumber = ((data[0] & 0x3) << 8) | data[1];
    minorChannelNumber = ((data[2] & 0x3) << 8) | data[3];
    //LogDebug(L"SVCT: two part channel number descriptor, major channel number = %hu, minor channel number = %hu",
    //          majorChannelNumber, minorChannelNumber);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeTwoPartChannelNumberDescriptor()");
  }
  return false;
}

bool CParserSvct::DecodeChannelPropertiesDescriptor(unsigned char* data,
                                                    unsigned char dataLength,
                                                    unsigned short& channelTsid,
                                                    bool& outOfBand,
                                                    bool& accessControlled,
                                                    bool& hideGuide,
                                                    unsigned char& serviceType)
{
  if (dataLength != 4)
  {
    LogDebug(L"SVCT: invalid channel properties descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    channelTsid = ((data[0] & 0x3) << 8) | data[1];
    outOfBand = (data[2] & 0x2) != 0;
    accessControlled = (data[2] & 0x1) != 0;
    hideGuide = (data[3] & 0x80) != 0;
    serviceType = data[3] & 0x3f;
    //LogDebug(L"SVCT: channel properties descriptor, channel TSID = %hu, out of band = %d, access controlled = %d, hide guide = %d, service type = %hhu",
    //          channelTsid, outOfBand, accessControlled, hideGuide,
    //          serviceType);
    return true;
  }
  catch (...)
  {
    LogDebug(L"SVCT: unhandled exception in DecodeChannelPropertiesDescriptor()");
  }
  return false;
}