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
#include "ParserLvct.h"
#include <algorithm>
#include "..\..\shared\BasePmtParser.h"
#include "..\..\shared\PidTable.h"
#include "..\..\shared\TimeUtils.h"
#include "EnterCriticalSection.h"
#include "TextUtil.h"
#include "Utils.h"


#define MINIMUM_SECTION_LENGTH 13
#define MINIMUM_RECORD_BYTE_COUNT 32


extern void LogDebug(const wchar_t* fmt, ...);

CParserLvct::CParserLvct(unsigned short pid) : m_records(600000)
{
  m_pid = pid;
  m_isSeenCable = false;
  m_isSeenTerrestrial = false;
  m_isReady = false;
  m_completeTime = 0;

  SetCallBack(NULL);
  m_currentRecord = NULL;
  m_currentRecordIndex = 0xffff;
}

CParserLvct::~CParserLvct(void)
{
  SetCallBack(NULL);
}

void CParserLvct::Reset()
{
  LogDebug(L"LVCT %hu: reset", m_pid);
  CEnterCriticalSection lock(m_section);
  m_records.RemoveAllRecords();
  m_seenSections.clear();
  m_unseenSections.clear();
  m_isSeenCable = false;
  m_isSeenTerrestrial = false;
  m_isReady = false;
  m_currentRecord = NULL;
  m_currentRecordIndex = 0xffff;
  LogDebug(L"LVCT %hu: reset done", m_pid);
}

void CParserLvct::SetCallBack(ICallBackLvct* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserLvct::OnNewSection(CSection& section)
{
  try
  {
    if (
      (
        section.table_id != TABLE_ID_LVCT_CABLE &&
        section.table_id != TABLE_ID_LVCT_TERRESTRIAL
      ) ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (
      section.section_length < MINIMUM_SECTION_LENGTH ||
      (section.table_id == TABLE_ID_LVCT_CABLE && section.section_length > 4093) ||
      (section.table_id != TABLE_ID_LVCT_TERRESTRIAL && section.section_length > 1021)
    )
    {
      LogDebug(L"LVCT %hu: invalid section, length = %d, table ID = 0x%x",
                m_pid, section.section_length, section.table_id);
      return;
    }
    unsigned char protocolVersion = section.Data[8];
    if (protocolVersion != 0)
    {
      LogDebug(L"LVCT %hu: unsupported protocol version, protocol version = %hhu, table ID = 0x%x",
                m_pid, protocolVersion, section.table_id);
      return;
    }

    unsigned char* data = section.Data;
    unsigned short transportStreamId = section.table_id_extension;
    unsigned char numChannelsInSection = data[9];
    //LogDebug(L"LVCT %hu: table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section length = %d, section number = %d, last section number = %d, num. channels in section = %hhu",
    //          m_pid, section.table_id, transportStreamId, protocolVersion,
    //          section.version_number, section.section_length,
    //          section.SectionNumber, section.LastSectionNumber,
    //          numChannelsInSection);

    if (MINIMUM_SECTION_LENGTH + (numChannelsInSection * MINIMUM_RECORD_BYTE_COUNT) > section.section_length)
    {
      LogDebug(L"LVCT %hu: invalid section, num. channels in section = %hhu, section length = %d, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d",
                m_pid, numChannelsInSection, section.section_length,
                section.table_id, transportStreamId, protocolVersion,
                section.version_number, section.SectionNumber);
      return;
    }

    // Have we seen this section before?
    unsigned long long sectionKey = ((unsigned long long)section.table_id << 32) | ((unsigned long long)section.version_number << 24) | ((unsigned long long)transportStreamId << 8) | section.SectionNumber;
    unsigned long long sectionGroupMask = 0xffffffff00ffff00;
    unsigned long long sectionGroupKey = sectionKey & sectionGroupMask;
    CEnterCriticalSection lock(m_section);
    vector<unsigned long long>::const_iterator sectionIt = find(m_seenSections.begin(),
                                                                m_seenSections.end(),
                                                                sectionKey);
    if (sectionIt != m_seenSections.end())
    {
      // Yes. We might be ready!
      //LogDebug(L"LVCT %hu: previously seen section, table ID = 0x%x, TSID = %hu, protocol version = %hhu, section number = %d",
      //          m_pid, section.table_id, transportStreamId, protocolVersion,
      //          section.SectionNumber);
      if (m_isReady || m_unseenSections.size() != 0)
      {
        return;
      }

      // ATSC A/65 section 7 and SCTE 54 section 5.7.1.2 specify in band cable
      // and terrestrial LVCT maximum cycle time is 400 ms.
      // SCTE 65 annex D1 specifies out of band cable LVCT maximum cycle time
      // is 2 minutes.
      // We won't attempt to handle OOB cable here, because it seems LVCT is
      // rarely used in that scenario. Also, we assume that in band broadcasts
      // are not compliant... as seems to be the case in practice.
      if (CTimeUtils::ElapsedMillis(m_completeTime) >= 2000)
      {
        m_records.RemoveExpiredRecords(m_callBack);
        LogDebug(L"LVCT %hu: ready, sections parsed = %llu, channel count = %lu",
                  m_pid, (unsigned long long)m_seenSections.size(),
                  m_records.GetRecordCount());
        m_isReady = true;
        if (m_callBack != NULL)
        {
          if (m_isSeenCable)
          {
            m_callBack->OnTableComplete(TABLE_ID_LVCT_CABLE);
          }
          if (m_isSeenTerrestrial)
          {
            m_callBack->OnTableComplete(TABLE_ID_LVCT_TERRESTRIAL);
          }
        }
      }
      return;
    }

    // Were we expecting this section?
    sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
    if (sectionIt == m_unseenSections.end())
    {
      // No. Is this a change/update, or just a new section group?
      bool isChange = m_isReady;
      vector<unsigned long long>::const_iterator tempSectionIt = m_unseenSections.begin();
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
        LogDebug(L"LVCT %hu: received, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d, last section number = %d",
                  m_pid, section.table_id, transportStreamId, protocolVersion,
                  section.version_number, section.SectionNumber,
                  section.LastSectionNumber);
        if (section.table_id == TABLE_ID_LVCT_CABLE)
        {
          m_isSeenCable = true;
        }
        else
        {
          m_isSeenTerrestrial = true;
        }
        if (m_callBack != NULL)
        {
          m_callBack->OnTableSeen(section.table_id);
        }
      }
      else
      {
        LogDebug(L"LVCT %hu: changed, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d, last section number = %d",
                  m_pid, section.table_id, transportStreamId, protocolVersion,
                  section.version_number, section.SectionNumber,
                  section.LastSectionNumber);
        m_records.MarkExpiredRecords((section.table_id << 16) | transportStreamId);
        if (m_isReady && m_callBack != NULL)
        {
          m_isReady = false;
          m_callBack->OnTableChange(section.table_id);
        }
        m_isReady = false;
      }

      unsigned long long baseKey = sectionKey & 0xffffffffffffff00;
      for (unsigned char s = 0; s <= section.LastSectionNumber; s++)
      {
        m_unseenSections.push_back(baseKey + s);
      }
      sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
    }
    else
    {
      //LogDebug(L"LVCT %hu: new section, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d",
      //          m_pid, section.table_id, transportStreamId,
      //          section.version_number, section.SectionNumber);
    }

    unsigned short pointer = 10;                              // points to the first byte in the channel loop
    unsigned short endOfSection = section.section_length - 1; // points to the first byte in the CRC
    for (unsigned char i = 0; i < numChannelsInSection && pointer + ((numChannelsInSection - i) * MINIMUM_RECORD_BYTE_COUNT) - 1 < endOfSection - 2; i++)   // - 2 for the additional descriptors length bytes
    {
      CRecordLvct* record = new CRecordLvct();
      if (record == NULL)
      {
        LogDebug(L"LVCT %hu: failed to allocate record, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d, num. channels in section = %hhu, index = %hhu",
                  m_pid, section.table_id, transportStreamId, protocolVersion,
                  section.version_number, section.SectionNumber,
                  numChannelsInSection, i);
        return;
      }

      record->TableId = section.table_id;
      record->SectionTransportStreamId = transportStreamId;
      if (!DecodeChannelRecord(data, pointer, endOfSection, *record))
      {
        LogDebug(L"LVCT %hu: invalid section, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d, num. channels in section = %hhu, index = %hhu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu",
                  m_pid, section.table_id, transportStreamId, protocolVersion,
                  section.version_number, section.SectionNumber,
                  numChannelsInSection, i,
                  record->TransportStreamId, record->ProgramNumber,
                  record->MajorChannelNumber, record->MinorChannelNumber);
        delete record;
        return;
      }

      m_records.AddOrUpdateRecord((IRecord**)&record, m_callBack);
    }

    // additional descriptors...
    unsigned short additionalDescriptorsLength = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
    pointer += 2;
    //LogDebug(L"LVCT %hu: additional descriptors length = %hu, pointer = %hu",
    //          m_pid, additionalDescriptorsLength, pointer);
    unsigned short endOfAdditionalDescriptors = pointer + additionalDescriptorsLength;
    if (endOfAdditionalDescriptors != endOfSection)
    {
      LogDebug(L"LVCT %hu: invalid section, additional descriptors length = %hu, pointer = %hu, end of section = %hu, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d, num. channels in section = %hhu",
                m_pid, additionalDescriptorsLength, pointer, endOfSection,
                section.table_id, transportStreamId, protocolVersion,
                section.version_number, section.SectionNumber,
                numChannelsInSection);
      return;
    }

    while (pointer + 1 < endOfAdditionalDescriptors)
    {
      unsigned char tag = data[pointer++];
      unsigned char length = data[pointer++];
      //LogDebug(L"LVCT %hu: additional descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          m_pid, tag, length, pointer);
      if (pointer + length > endOfAdditionalDescriptors)
      {
        LogDebug(L"LVCT %hu: invalid section, additional descriptor length = %hhu, pointer = %hu, end of additional descriptors = %hu, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d, num. channels in section = %hhu, tag = 0x%hhx, end of section = %hu",
                  m_pid, length, pointer, endOfAdditionalDescriptors,
                  section.table_id, transportStreamId, protocolVersion,
                  section.version_number, section.SectionNumber,
                  numChannelsInSection, tag, endOfSection);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"LVCT %hu: section parsing error, pointer = %hu, end of section = %hu, table ID = 0x%x, TSID = %hu, protocol version = %hhu, version number = %d, section number = %d, num. channels in section = %hhu",
                m_pid, pointer, endOfSection, section.table_id,
                transportStreamId, protocolVersion, section.version_number,
                section.SectionNumber, numChannelsInSection);
      return;
    }

    m_seenSections.push_back(sectionKey);
    m_unseenSections.erase(sectionIt);
    if (m_unseenSections.size() == 0)
    {
      // We can't assume that we've seen all sections yet, because sections for
      // another transport stream and/or table may not have been received.
      m_completeTime = clock();
    }
  }
  catch (...)
  {
    LogDebug(L"LVCT %hu: unhandled exception in OnNewSection()", m_pid);
  }
}

bool CParserLvct::IsSeen() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSections.size() != 0;
}

bool CParserLvct::IsReady() const
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

unsigned short CParserLvct::GetChannelCount() const
{
  CEnterCriticalSection lock(m_section);
  return (unsigned short)m_records.GetRecordCount();
}

bool CParserLvct::GetChannel(unsigned short index,
                              unsigned char& tableId,
                              unsigned short& sectionTransportStreamId,
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
                              bool& pathSelect,
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
                              unsigned char& captionsLanguageCount)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectChannelRecordByIndex(index))
  {
    return false;
  }

  tableId = m_currentRecord->TableId;
  sectionTransportStreamId = m_currentRecord->SectionTransportStreamId;
  longNameCount = m_currentRecord->LongNames.size();
  majorChannelNumber = m_currentRecord->MajorChannelNumber;
  minorChannelNumber = m_currentRecord->MinorChannelNumber;
  modulationMode = m_currentRecord->ModulationMode;
  carrierFrequency = m_currentRecord->CarrierFrequency;
  transportStreamId = m_currentRecord->TransportStreamId;
  programNumber = m_currentRecord->ProgramNumber;
  etmLocation = m_currentRecord->EtmLocation;
  accessControlled = m_currentRecord->AccessControlled;
  hidden = m_currentRecord->Hidden;
  pathSelect = m_currentRecord->PathSelect;
  outOfBand = m_currentRecord->OutOfBand;
  hideGuide = m_currentRecord->HideGuide;
  serviceType = m_currentRecord->ServiceType;
  sourceId = m_currentRecord->SourceId;
  streamCountVideo = m_currentRecord->StreamCountVideo;
  streamCountAudio = m_currentRecord->StreamCountAudio;
  isThreeDimensional = m_currentRecord->IsThreeDimensional;

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(m_currentRecord->ShortName,
                                  shortName,
                                  shortNameBufferSize,
                                  requiredBufferSize) && shortName != NULL)
  {
    LogDebug(L"LVCT %hu: insufficient short name buffer size, channel index = %hu, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu, required size = %hu, actual size = %hu",
              m_pid, index, tableId, sectionTransportStreamId,
              transportStreamId, programNumber, majorChannelNumber,
              minorChannelNumber, requiredBufferSize, shortNameBufferSize);
  }

  unsigned char requiredCount = 0;
  if (!CUtils::CopyVectorToArray(m_currentRecord->AudioLanguages,
                                  audioLanguages,
                                  audioLanguageCount,
                                  requiredCount) && audioLanguages != NULL)
  {
    LogDebug(L"LVCT %hu: insufficient audio language array size, channel index = %hu, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu, required size = %hhu, actual size = %hhu",
              m_pid, index, tableId, sectionTransportStreamId,
              transportStreamId, programNumber, majorChannelNumber,
              minorChannelNumber, requiredCount, audioLanguageCount);
  }
  if (!CUtils::CopyVectorToArray(m_currentRecord->CaptionsLanguages,
                                  captionsLanguages,
                                  captionsLanguageCount,
                                  requiredCount) && captionsLanguages != NULL)
  {
    LogDebug(L"LVCT %hu: insufficient captions language array size, channel index = %hu, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu, required size = %hhu, actual size = %hhu",
              m_pid, index, tableId, sectionTransportStreamId,
              transportStreamId, programNumber, majorChannelNumber,
              minorChannelNumber, requiredCount, captionsLanguageCount);
  }

  return true;
}

bool CParserLvct::GetChannelLongNameByIndex(unsigned short channelIndex,
                                            unsigned char nameIndex,
                                            unsigned long& language,
                                            char* name,
                                            unsigned short& nameBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectChannelRecordByIndex(channelIndex))
  {
    return false;
  }

  if (nameIndex >= m_currentRecord->LongNames.size())
  {
    LogDebug(L"LVCT %hu: invalid long name index, channel index = %hu, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu, name index = %hhu, name count = %llu",
              m_pid, channelIndex, m_currentRecord->TableId,
              m_currentRecord->SectionTransportStreamId,
              m_currentRecord->TransportStreamId,
              m_currentRecord->ProgramNumber,
              m_currentRecord->MajorChannelNumber,
              m_currentRecord->MinorChannelNumber, nameIndex,
              (unsigned long long)m_currentRecord->LongNames.size());
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->LongNames.begin();
  for ( ; it != m_currentRecord->LongNames.end(); it++)
  {
    if (nameIndex != 0)
    {
      nameIndex--;
      continue;
    }

    language = it->first;
    unsigned short requiredBufferSize = 0;
    if (!CUtils::CopyStringToBuffer(it->second, name, nameBufferSize, requiredBufferSize))
    {
      LogDebug(L"LVCT %hu: insufficient long name buffer size, channel index = %hu, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu, name index = %hhu, language = %S, required size = %hu, actual size = %hu",
                m_pid, channelIndex, m_currentRecord->TableId,
                m_currentRecord->SectionTransportStreamId,
                m_currentRecord->TransportStreamId,
                m_currentRecord->ProgramNumber,
                m_currentRecord->MajorChannelNumber,
                m_currentRecord->MinorChannelNumber, nameIndex,
                (char*)&language, requiredBufferSize, nameBufferSize);
    }
    return true;
  }
  return false;
}

bool CParserLvct::GetChannelLongNameByLanguage(unsigned short channelIndex,
                                                unsigned long language,
                                                char* name,
                                                unsigned short& nameBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectChannelRecordByIndex(channelIndex))
  {
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->LongNames.find(language);
  if (it == m_currentRecord->LongNames.end())
  {
    LogDebug(L"LVCT %hu: invalid long name language, channel index = %hu, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu, language = %S",
              m_pid, channelIndex, m_currentRecord->TableId,
              m_currentRecord->SectionTransportStreamId,
              m_currentRecord->TransportStreamId,
              m_currentRecord->ProgramNumber,
              m_currentRecord->MajorChannelNumber,
              m_currentRecord->MinorChannelNumber, (char*)&language);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(it->second, name, nameBufferSize, requiredBufferSize))
  {
    LogDebug(L"LVCT %hu: insufficient long name buffer size, channel index = %hu, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu, language = %S, required size = %hu, actual size = %hu",
              m_pid, channelIndex, m_currentRecord->TableId,
              m_currentRecord->SectionTransportStreamId,
              m_currentRecord->TransportStreamId,
              m_currentRecord->ProgramNumber,
              m_currentRecord->MajorChannelNumber,
              m_currentRecord->MinorChannelNumber, (char*)&language,
              requiredBufferSize, nameBufferSize);
  }
  return true;
}

bool CParserLvct::SelectChannelRecordByIndex(unsigned short index)
{
  if (m_currentRecord != NULL && m_currentRecordIndex == index)
  {
    return true;
  }

  IRecord* record = NULL;
  if (!m_records.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"LVCT %hu: invalid channel index, index = %hu, record count = %lu",
              m_pid, index, m_records.GetRecordCount());
    return false;
  }

  m_currentRecord = dynamic_cast<CRecordLvct*>(record);
  if (m_currentRecord == NULL)
  {
    LogDebug(L"LVCT %hu: invalid channel record, index = %hu", m_pid, index);
    return false;
  }
  m_currentRecordIndex = index;
  return true;
}

bool CParserLvct::DecodeChannelRecord(unsigned char* sectionData,
                                      unsigned short& pointer,
                                      unsigned short endOfSection,
                                      CRecordLvct& record)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT > endOfSection - 2)
  {
    LogDebug(L"LVCT: invalid channel record, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    // short_name = 7 * 16 bits (14 bytes), UTF-16 encoding
    if (!CTextUtil::IsoIec10646ToString(&sectionData[pointer], 14, &(record.ShortName)))
    {
      LogDebug(L"LVCT: invalid channel record, pointer = %hu, end of section = %hu",
                pointer, endOfSection);
      return false;
    }
    pointer += 14;

    record.MajorChannelNumber = ((sectionData[pointer] & 0xf) << 6) | (sectionData[pointer + 1] >> 2);
    pointer++;
    record.MinorChannelNumber = ((sectionData[pointer] & 0x3) << 8) | sectionData[pointer + 1];
    pointer += 2;

    // Pack one-part channel numbers into the major channel number.
    if ((record.MajorChannelNumber & 0x3f0) == 0x3f0)
    {
      record.MajorChannelNumber = ((record.MajorChannelNumber & 0xf) << 10) | record.MinorChannelNumber;
      record.MinorChannelNumber = 0;
    }

    record.ModulationMode = sectionData[pointer++];
    record.CarrierFrequency = sectionData[pointer++] << 24;
    record.CarrierFrequency |= (sectionData[pointer++] << 16);
    record.CarrierFrequency |= (sectionData[pointer++] << 8);
    record.CarrierFrequency |= sectionData[pointer++];

    record.TransportStreamId = ((sectionData[pointer]) << 8) | sectionData[pointer + 1];
    pointer += 2;

    record.ProgramNumber = ((sectionData[pointer]) << 8) | sectionData[pointer + 1];
    pointer += 2;
    record.EtmLocation = sectionData[pointer] >> 6;
    record.AccessControlled = (sectionData[pointer] & 0x20) != 0;
    record.Hidden = (sectionData[pointer] & 0x10) != 0;

    // cable only
    record.PathSelect = (sectionData[pointer] & 0x8) != 0;
    record.OutOfBand = (sectionData[pointer] & 0x4) != 0;

    record.HideGuide = (sectionData[pointer++] & 0x2) != 0;
    record.ServiceType = sectionData[pointer++] & 0x3f;
    record.SourceId = ((sectionData[pointer]) << 8) | sectionData[pointer + 1];
    pointer += 2;

    if (record.ShortName == NULL)
    {
      LogDebug(L"LVCT: failed to allocate a channel's short name, table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu",
                record.TableId, record.SectionTransportStreamId,
                record.TransportStreamId, record.ProgramNumber,
                record.MajorChannelNumber, record.MinorChannelNumber);
    }

    unsigned short descriptorsLength = ((sectionData[pointer] & 0x3) << 8) | sectionData[pointer + 1];
    pointer += 2;
    //LogDebug(L"LVCT: short name = %S, major channel = %hu, minor channel = %hu, modulation mode = %hhu, carrier frequency = %lu Hz, channel TSID = %hu, program number = %hu, ETM location = %hhu, access controlled = %d, hidden = %d, path select = %d, out of band = %d, hide guide = %d, service type = %hhu, source ID = %hu, descriptors length = %hu",
    //          record.ShortName == NULL ? "" : record.ShortName,
    //          record.MajorChannelNumber, record.MinorChannelNumber,
    //          record.ModulationMode, record.CarrierFrequency,
    //          record.TransportStreamId, record.ProgramNumber,
    //          record.EtmLocation, record.AccessControlled, record.Hidden,
    //          record.PathSelect, record.OutOfBand, record.HideGuide,
    //          record.ServiceType, record.SourceId, descriptorsLength);

    unsigned short endOfDescriptors = pointer + descriptorsLength;
    if (endOfDescriptors > endOfSection - 2)  // - 2 for additional descriptors length
    {
      LogDebug(L"LVCT: invalid channel record, descriptors length = %hu, pointer = %hu, end of section = %hu",
                descriptorsLength, pointer, endOfSection);
      return false;
    }

    unsigned char streamCountVideo;
    unsigned char streamCountAudio;
    bool isThreeDimensional;
    while (pointer + 1 < endOfDescriptors)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      //LogDebug(L"LVCT: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug(L"LVCT: invalid channel record, descriptor length = %hhu, pointer = %hu, end of descriptors = %hu, tag = 0x%hhx, end of section = %hu",
                  length, pointer, endOfDescriptors, tag, endOfSection);
        return false;
      }

      bool descriptorParseResult = true;
      if (tag == 0x8d) // parameterized service descriptor
      {
        descriptorParseResult = DecodeParameterizedServiceDescriptor(&sectionData[pointer],
                                                                      length,
                                                                      isThreeDimensional);
        if (descriptorParseResult)
        {
          record.IsThreeDimensional |= isThreeDimensional;
        }
      }
      else if (tag == 0xa0) // extended channel name descriptor
      {
        descriptorParseResult = CTextUtil::AtscScteMultipleStringStructureToStrings(&sectionData[pointer],
                                                                                    length,
                                                                                    record.LongNames);
        if (descriptorParseResult && record.LongNames.size() == 0)
        {
          LogDebug(L"LVCT: failed to allocate a channel's extended name(s), table ID = 0x%hhx, TSID = %hu, channel TSID = %hu, program number = %hu, major channel number = %hu, minor channel number = %hu",
                    record.TableId, record.SectionTransportStreamId,
                    record.TransportStreamId, record.ProgramNumber,
                    record.MajorChannelNumber, record.MinorChannelNumber);
        }
      }
      else if (tag == 0xa1) // service location descriptor
      {
        descriptorParseResult = DecodeServiceLocationDescriptor(&sectionData[pointer],
                                                                length,
                                                                streamCountVideo,
                                                                streamCountAudio,
                                                                isThreeDimensional,
                                                                record.AudioLanguages,
                                                                record.CaptionsLanguages);
        if (descriptorParseResult)
        {
          if (streamCountVideo > record.StreamCountVideo)
          {
            record.StreamCountVideo = streamCountVideo;
          }
          if (streamCountAudio > record.StreamCountAudio)
          {
            record.StreamCountAudio = streamCountAudio;
          }
          record.IsThreeDimensional |= isThreeDimensional;
        }
      }
      else if (tag = 0xbb)  // component list descriptor
      {
        descriptorParseResult = DecodeComponentListDescriptor(&sectionData[pointer],
                                                              length,
                                                              streamCountVideo,
                                                              streamCountAudio,
                                                              isThreeDimensional);
        if (descriptorParseResult)
        {
          if (streamCountVideo > record.StreamCountVideo)
          {
            record.StreamCountVideo = streamCountVideo;
          }
          if (streamCountAudio > record.StreamCountAudio)
          {
            record.StreamCountAudio = streamCountAudio;
          }
          record.IsThreeDimensional |= isThreeDimensional;
        }
      }

      if (!descriptorParseResult)
      {
        LogDebug(L"LVCT: invalid channel record descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptors = %hu",
                  tag, length, pointer, endOfDescriptors);
        return false;
      }

      pointer += length;
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"LVCT: unhandled exception in DecodeChannelRecord()");
  }
  return false;
}

bool CParserLvct::DecodeParameterizedServiceDescriptor(unsigned char* data,
                                                        unsigned char dataLength,
                                                        bool& isThreeDimensional)
{
  if (dataLength == 0)
  {
    LogDebug(L"LVCT: invalid parameterized service descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char applicationTag = data[0];
    //LogDebug(L"LVCT: parameterized service descriptor, application tag = %hhu",
    //          applicationTag);
    isThreeDimensional = applicationTag == 1; // A/104 part 1 section 5.2

    // For application tag 1:
    //unsigned char channelType = data[1] & 0x1f;
    // 0 = frame compatible side by side
    // 1 = frame compatible top and bottom
    // 3/4/5/6 = service compatible
    return true;
  }
  catch (...)
  {
    LogDebug(L"LVCT: unhandled exception in DecodeParameterizedServiceDescriptor()");
  }
  return false;
}

bool CParserLvct::DecodeServiceLocationDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned char& streamCountVideo,
                                                  unsigned char& streamCountAudio,
                                                  bool& isThreeDimensional,
                                                  vector<unsigned long>& audioLanguages,
                                                  vector<unsigned long>& captionsLanguages)
{
  if (dataLength < 3 || (dataLength - 3) % 6 != 0)
  {
    LogDebug(L"LVCT: invalid service location descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    streamCountVideo = 0;
    streamCountAudio = 0;
    isThreeDimensional = false;

    unsigned short pcrPid = ((data[0] & 0x1f) << 8) | data[1];
    unsigned char numberElements = data[2];
    //LogDebug(L"LVCT: service location descriptor, PCR PID = %hu, element count = %hhu",
    //          pcrPid, numberElements);

    if (dataLength != 3 + (numberElements * 6))
    {
      LogDebug(L"LVCT: invalid service location descriptor, length = %hhu, number elements = %hhu",
                dataLength, numberElements);
      return false;
    }

    unsigned short pointer = 3;
    for (unsigned char i = 0; i < numberElements && pointer + ((numberElements - i) * 6) - 1 < dataLength; i++)
    {
      unsigned char streamType = data[pointer++];
      unsigned short elementaryPid = ((data[pointer] & 0x1f) << 8) | data[pointer + 1];
      pointer += 2;
      unsigned long iso639LanguageCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      //LogDebug(L"  stream type = 0x%hhx, elementary PID = %hu, language = %S",
      //          streamType, elementaryPid, (char*)&iso639LanguageCode);

      if (CPidTable::IsVideoStream(streamType) || streamType == STREAM_TYPE_VIDEO_MPEG2_DCII)
      {
        streamCountVideo++;

        if (
          CPidTable::IsThreeDimensionalVideoStream(streamType) ||
          streamType == STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXH   // technically this could be multi-view, rather than 3D; can't tell without an MVC extension descriptor
        )
        {
          isThreeDimensional = true;  // service compatible plano-stereoscopic
        }
        if (
          iso639LanguageCode != 0 &&
          find(captionsLanguages.begin(), captionsLanguages.end(), iso639LanguageCode) == captionsLanguages.end()
        )
        {
          // Assume closed captions data is present in the video stream.
          captionsLanguages.push_back(iso639LanguageCode);
        }
      }
      else if (CPidTable::IsAudioStream(streamType))
      {
        streamCountAudio++;

        if (
          iso639LanguageCode != 0 &&
          find(audioLanguages.begin(), audioLanguages.end(), iso639LanguageCode) == audioLanguages.end()
        )
        {
          audioLanguages.push_back(iso639LanguageCode);
        }
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"LVCT: unhandled exception in DecodeServiceLocationDescriptor()");
  }
  return false;
}

bool CParserLvct::DecodeComponentListDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                unsigned char& streamCountVideo,
                                                unsigned char& streamCountAudio,
                                                bool& isThreeDimensional)
{
  if (dataLength == 0)
  {
    LogDebug(L"LVCT: invalid component list descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    streamCountVideo = 0;
    streamCountAudio = 0;
    isThreeDimensional = false;

    bool alternate = (data[0] & 0x80) != 0;
    unsigned char componentCount = data[0] & 0x7f;
    //LogDebug(L"LVCT: component list descriptor, alternate = %d, component count = %hhu",
    //          alternate, componentCount);

    if (dataLength < 1 + (componentCount * 6))
    {
      LogDebug(L"LVCT: invalid component list descriptor, length = %hhu, component count = %hhu",
                dataLength, componentCount);
      return false;
    }

    unsigned short pointer = 1;
    for (unsigned char i = 0; i < componentCount && pointer + ((componentCount - i) * 6) - 1 < dataLength; i++)
    {
      unsigned char streamType = data[pointer++];
      unsigned long formatIdentifier = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
      pointer += 4;
      unsigned char lengthOfDetails = data[pointer++];
      //LogDebug(L"  stream type = 0x%hhx, format identifier = %u, length of details = %hhu",
      //          streamType, formatIdentifier, lengthOfDetails);

      if (pointer + ((componentCount - 1 - i) * 6) + lengthOfDetails > dataLength)
      {
        LogDebug(L"LVCT: invalid component list descriptor, length of details = %hhu, pointer = %hu, descriptor length = %hhu, component count = %hhu, component index = %hhu, stream type = 0x%hhx",
                  lengthOfDetails, pointer, dataLength, componentCount, i,
                  streamType);
        return false;
      }

      if (CPidTable::IsVideoStream(streamType) || streamType == STREAM_TYPE_VIDEO_MPEG2_DCII)
      {
        streamCountVideo++;

        if (
          CPidTable::IsThreeDimensionalVideoStream(streamType) ||
          streamType == STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXH   // technically this could be multi-view, rather than 3D; can't tell without an MVC extension descriptor
        )
        {
          isThreeDimensional = true;  // service compatible
        }
      }
      else if (CPidTable::IsAudioStream(streamType))
      {
        streamCountAudio++;
      }

      pointer += lengthOfDetails;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"LVCT: unhandled exception in DecodeComponentListDescriptor()");
  }
  return false;
}