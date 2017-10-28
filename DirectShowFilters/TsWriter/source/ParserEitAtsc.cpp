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
#include "ParserEitAtsc.h"
#include <algorithm>    // find()
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\TimeUtils.h"
#include "TextUtil.h"
#include "Utils.h"


#define MINIMUM_SECTION_LENGTH 11
#define MINIMUM_RECORD_BYTE_COUNT 12


extern void LogDebug(const wchar_t* fmt, ...);

CParserEitAtsc::CParserEitAtsc(unsigned short pid, ISectionDispatcher* sectionDispatcher)
  : CSectionDecoder(sectionDispatcher), m_records(600000)
{
  m_isReady = false;
  m_completeTime = 0;
  SetPid(pid);
  SetCallBack(NULL);
  m_currentRecord = NULL;
  m_currentRecordIndex = 0xffffffff;
}

CParserEitAtsc::~CParserEitAtsc()
{
  SetCallBack(NULL);
}

void CParserEitAtsc::Reset(bool enableCrcCheck)
{
  LogDebug(L"EIT ATSC %d: reset", GetPid());
  CEnterCriticalSection lock(m_section);
  m_records.RemoveAllRecords();
  EnableCrcCheck(enableCrcCheck);
  CSectionDecoder::Reset();
  m_isReady = false;
  m_currentRecord = NULL;
  m_currentRecordIndex = 0xffffffff;
  LogDebug(L"EIT ATSC %d: reset done", GetPid());
}

void CParserEitAtsc::SetCallBack(ICallBackTableParser* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserEitAtsc::OnNewSection(const CSection& section)
{
  try
  {
    if (
      section.TableId != TABLE_ID_EIT_ATSC ||
      !section.SectionSyntaxIndicator ||
      !section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.SectionLength < MINIMUM_SECTION_LENGTH || section.SectionLength > 4093)
    {
      LogDebug(L"EIT ATSC %d: invalid section, length = %hu",
                GetPid(), section.SectionLength);
      return;
    }
    unsigned char protocolVersion = section.Data[8];
    if (protocolVersion != 0)
    {
      LogDebug(L"EIT ATSC %d: unsupported protocol version, protocol version = %hhu",
                GetPid(), protocolVersion);
      return;
    }

    CEnterCriticalSection lock(m_section);
    const unsigned char* data = section.Data;
    unsigned short sourceId = section.TableIdExtension;
    unsigned char numEventsInSection = data[9];
    //LogDebug(L"EIT ATSC %d: source Id = %hu, protocol version = %hhu, version number = %hhu, section length = %hu, section number = %hhu, last section number = %hhu, num. events in section = %hhu",
    //          GetPid(), sourceId, protocolVersion, section.VersionNumber,
    //          section.SectionLength, section.SectionNumber,
    //          section.LastSectionNumber, numEventsInSection);

    if (MINIMUM_SECTION_LENGTH + (numEventsInSection * MINIMUM_RECORD_BYTE_COUNT) > section.SectionLength)
    {
      LogDebug(L"EIT ATSC %d: invalid section, num. events in section = %hhu, section length = %hu, table ID = 0x%hhx, source Id = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu",
                GetPid(), numEventsInSection, section.SectionLength,
                section.TableId, sourceId, protocolVersion,
                section.VersionNumber, section.SectionNumber);
      return;
    }

    // Have we seen this section before?
    unsigned long sectionKey = (section.VersionNumber << 24) | (sourceId << 8) | section.SectionNumber;
    unsigned long sectionGroupMask = 0x00ffff00;
    unsigned long sectionGroupKey = sectionKey & sectionGroupMask;
    vector<unsigned long>::const_iterator sectionIt = find(m_seenSections.begin(),
                                                            m_seenSections.end(),
                                                            sectionKey);
    if (sectionIt != m_seenSections.end())
    {
      // Yes. We might be ready!
      //LogDebug(L"EIT ATSC %d: previously seen section, source Id = %hu, protocol version = %hhu, section number = %hhu",
      //          GetPid(), sourceId, protocolVersion, section.SectionNumber);
      if (m_isReady || m_unseenSections.size() != 0)
      {
        return;
      }

      // ATSC A/69 section 5.1 table 5.1 recommended repetition intervals:
      // - EIT-0 = 500 ms
      // - EIT-1 = 3 s
      // - EIT-2, EIT-3 = 60 s
      // - EIT-X = 60 s
      if (CTimeUtils::ElapsedMillis(m_completeTime) >= 30000)
      {
        if (m_records.RemoveExpiredRecords(NULL) != 0)
        {
          m_currentRecord = NULL;
          m_currentRecordIndex = 0xffffffff;
        }

        LogDebug(L"EIT ATSC %d: ready, sections parsed = %llu, event count = %lu",
                  GetPid(), (unsigned long long)m_seenSections.size(),
                  m_records.GetRecordCount());
        m_isReady = true;
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_EIT_ATSC);
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
        LogDebug(L"EIT ATSC %d: received, source Id = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                  GetPid(), sourceId, protocolVersion, section.VersionNumber,
                  section.SectionNumber, section.LastSectionNumber);
        if (m_callBack != NULL && m_seenSections.size() == 0)
        {
          m_callBack->OnTableSeen(TABLE_ID_EIT_ATSC);
        }
      }
      else
      {
        LogDebug(L"EIT ATSC %d: changed, source Id = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                  GetPid(), sourceId, protocolVersion, section.VersionNumber,
                  section.SectionNumber, section.LastSectionNumber);
        m_records.MarkExpiredRecords(sourceId);
        if (m_isReady)
        {
          m_isReady = false;
          if (m_callBack != NULL)
          {
            m_callBack->OnTableChange(TABLE_ID_EIT_ATSC);
          }
        }
      }

      unsigned long baseKey = sectionKey & 0xffffff00;
      for (unsigned char s = 0; s <= section.LastSectionNumber; s++)
      {
        m_unseenSections.push_back(baseKey + s);
      }
      sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
    }
    else
    {
      //LogDebug(L"EIT ATSC %d: new section, source Id = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu",
      //            GetPid(), sourceId, protocolVersion,
      //            section.VersionNumber, section.SectionNumber);
    }

    unsigned short pointer = 10;                              // points to the first byte in the event loop
    unsigned short endOfSection = section.SectionLength - 1;  // points to the first byte in the CRC
    for (unsigned char i = 0; i < numEventsInSection && pointer + ((numEventsInSection - i) * MINIMUM_RECORD_BYTE_COUNT) - 1 < endOfSection; i++)
    {
      CRecordEit* record = new CRecordEit();
      if (record == NULL)
      {
        LogDebug(L"EIT ATSC %d: failed to allocate record, source Id = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, num. events in section = %hhu, index = %hhu",
                  GetPid(), sourceId, protocolVersion, section.VersionNumber,
                  section.SectionNumber, numEventsInSection, i);
        return;
      }

      record->SourceId = sourceId;
      if (!DecodeEventRecord(data, pointer, endOfSection, *record))
      {
        LogDebug(L"EIT ATSC %d: invalid section, source Id = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, num. events in section = %hhu, index = %hhu, event ID = %hu",
                  GetPid(), sourceId, protocolVersion, section.VersionNumber,
                  section.SectionNumber, numEventsInSection, i,
                  record->EventId);
        delete record;
        return;
      }

      m_records.AddOrUpdateRecord((IRecord**)&record, NULL);
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"EIT ATSC %d: section parsing error, pointer = %hu, end of section = %hu, source Id = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, num. events in section = %hhu",
                GetPid(), pointer, endOfSection, sourceId, protocolVersion,
                section.VersionNumber, section.SectionNumber,
                numEventsInSection);
      return;
    }

    m_seenSections.push_back(sectionKey);
    m_unseenSections.erase(sectionIt);
    if (m_unseenSections.size() == 0)
    {
      // We can't assume that we've seen all sections yet, because sections for
      // another source may not have been received.
      m_completeTime = clock();
    }
  }
  catch (...)
  {
    LogDebug(L"EIT ATSC %d: unhandled exception in OnNewSection()", GetPid());
  }
}

bool CParserEitAtsc::IsSeen() const
{
  CEnterCriticalSection lock(m_section);
  return m_seenSections.size() != 0;
}

bool CParserEitAtsc::IsReady() const
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

unsigned long CParserEitAtsc::GetEventCount() const
{
  CEnterCriticalSection lock(m_section);
  return m_records.GetRecordCount();
}

bool CParserEitAtsc::GetEvent(unsigned long index,
                              unsigned short& sourceId,
                              unsigned short& eventId,
                              unsigned long& startDateTime,
                              unsigned long& duration,
                              unsigned char& titleCount,
                              unsigned long* audioLanguages,
                              unsigned char& audioLanguageCount,
                              unsigned long* captionsLanguages,
                              unsigned char& captionsLanguageCount,
                              unsigned char* genreIds,
                              unsigned char& genreIdCount,
                              unsigned char& vchipRating,
                              unsigned char& mpaaClassification,
                              unsigned short& advisories)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(index))
  {
    return false;
  }

  sourceId = m_currentRecord->SourceId;
  eventId = m_currentRecord->EventId;
  startDateTime = m_currentRecord->StartDateTime;
  duration = m_currentRecord->Duration;
  titleCount = m_currentRecord->Titles.size();
  vchipRating = m_currentRecord->VchipRating;
  mpaaClassification = m_currentRecord->MpaaClassification;
  advisories = m_currentRecord->Advisories;

  unsigned char requiredCount = 0;
  if (!CUtils::CopyVectorToArray(m_currentRecord->AudioLanguages,
                                  audioLanguages,
                                  audioLanguageCount,
                                  requiredCount))
  {
    LogDebug(L"EIT ATSC %d: insufficient audio language array size, event index = %lu, source ID = %hu, event ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, sourceId, eventId, requiredCount,
              audioLanguageCount);
  }
  if (!CUtils::CopyVectorToArray(m_currentRecord->CaptionsLanguages,
                                  captionsLanguages,
                                  captionsLanguageCount,
                                  requiredCount))
  {
    LogDebug(L"EIT ATSC %d: insufficient captions language array size, event index = %lu, source ID = %hu, event ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, sourceId, eventId, requiredCount,
              captionsLanguageCount);
  }
  if (!CUtils::CopyVectorToArray(m_currentRecord->GenreIds,
                                  genreIds,
                                  genreIdCount,
                                  requiredCount))
  {
    LogDebug(L"EIT ATSC %d: insufficient genre ID array size, event index = %lu, source ID = %hu, event ID = %hu, required size = %hhu, actual size = %hhu",
              GetPid(), index, sourceId, eventId, requiredCount, genreIdCount);
  }

  return true;
}

bool CParserEitAtsc::GetEventTitleByIndex(unsigned long eventIndex,
                                          unsigned char titleIndex,
                                          unsigned long& language,
                                          char* title,
                                          unsigned short& titleBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(eventIndex))
  {
    return false;
  }

  if (titleIndex >= m_currentRecord->Titles.size())
  {
    LogDebug(L"EIT ATSC %d: invalid title index, event index = %lu, source ID = %hu, event ID = %hu, title index = %hhu, title count = %llu",
              GetPid(), eventIndex, m_currentRecord->SourceId,
              m_currentRecord->EventId, titleIndex,
              (unsigned long long)m_currentRecord->Titles.size());
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->Titles.begin();
  for ( ; it != m_currentRecord->Titles.end(); it++)
  {
    if (titleIndex != 0)
    {
      titleIndex--;
      continue;
    }

    language = it->first;
    unsigned short requiredBufferSize = 0;
    if (!CUtils::CopyStringToBuffer(it->second, title, titleBufferSize, requiredBufferSize))
    {
      LogDebug(L"EIT ATSC %d: insufficient title buffer size, event index = %lu, source ID = %hu, event ID = %hu, title index = %hhu, language = %S, required size = %hu, actual size = %hu",
                GetPid(), eventIndex, m_currentRecord->SourceId,
                m_currentRecord->EventId, titleIndex, (char*)&language,
                requiredBufferSize, titleBufferSize);
    }
    return true;
  }
  return false;
}

bool CParserEitAtsc::GetEventTitleByLanguage(unsigned long eventIndex,
                                              unsigned long language,
                                              char* title,
                                              unsigned short& titleBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(eventIndex))
  {
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->Titles.find(language);
  if (it == m_currentRecord->Titles.end())
  {
    LogDebug(L"EIT ATSC %d: invalid title language, event index = %lu, source ID = %hu, event ID = %hu, language = %S",
              GetPid(), eventIndex, m_currentRecord->SourceId,
              m_currentRecord->EventId, (char*)&language);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(it->second, title, titleBufferSize, requiredBufferSize))
  {
    LogDebug(L"EIT ATSC %d: insufficient title buffer size, event index = %lu, source ID = %hu, event ID = %hu, language = %S, required size = %hu, actual size = %hu",
              GetPid(), eventIndex, m_currentRecord->SourceId,
              m_currentRecord->EventId, (char*)&language, requiredBufferSize,
              titleBufferSize);
  }
  return true;
}

bool CParserEitAtsc::GetEventIdentifiers(unsigned long index,
                                          unsigned short& sourceId,
                                          unsigned short& eventId)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(index))
  {
    return false;
  }

  sourceId = m_currentRecord->SourceId;
  eventId = m_currentRecord->EventId;
  return true;
}

bool CParserEitAtsc::SelectEventRecordByIndex(unsigned long index)
{
  if (m_currentRecord != NULL && m_currentRecordIndex == index)
  {
    return true;
  }

  IRecord* record = NULL;
  if (!m_records.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"EIT ATSC %d: invalid event index, index = %lu, record count = %lu",
              GetPid(), index, m_records.GetRecordCount());
    return false;
  }

  m_currentRecord = dynamic_cast<CRecordEit*>(record);
  if (m_currentRecord == NULL)
  {
    LogDebug(L"EIT ATSC %d: invalid event record, index = %lu",
              GetPid(), index);
    return false;
  }
  m_currentRecordIndex = index;
  return true;
}

bool CParserEitAtsc::DecodeEventRecord(const unsigned char* sectionData,
                                        unsigned short& pointer,
                                        unsigned short endOfSection,
                                        CRecordEit& record)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT > endOfSection)
  {
    LogDebug(L"EIT ATSC: invalid event record, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.EventId = ((sectionData[pointer] & 0x3f) << 8) | sectionData[pointer + 1];
    pointer += 2;

    record.StartDateTime = (sectionData[pointer] << 24) | (sectionData[pointer + 1] << 16) | (sectionData[pointer + 2] << 8) | sectionData[pointer + 3];
    pointer += 4;

    record.EtmLocation = (sectionData[pointer] & 0x30) >> 4;
    record.Duration = ((sectionData[pointer] & 0xf) << 16) | (sectionData[pointer + 1] << 8) | sectionData[pointer + 2];
    pointer += 3;

    unsigned char titleLength = sectionData[pointer++];
    //LogDebug(L"EIT ATSC: event ID = %hu, start date/time = %lu, ETM location = %hhu, length in seconds = %lu, title length = %hhu",
    //          record.Id, record.StartDateTime, record.EtmLocation,
    //          record.Duration, titleLength);
    if (titleLength > 0)
    {
      if (
        pointer + titleLength + 2 > endOfSection ||
        !CTextUtil::AtscScteMultipleStringStructureToStrings(&sectionData[pointer],
                                                              titleLength,
                                                              record.Titles)
      )
      {
        LogDebug(L"EIT ATSC: invalid event record, title length = %hhu, pointer = %hu, end of section = %hu",
                  titleLength, pointer, endOfSection);
        return false;
      }

      if (record.Titles.size() == 0)
      {
        LogDebug(L"EIT ATSC: failed to allocate an event's title, title length = %hhu, pointer = %hu, source Id = %hu, event ID = %hu",
                  titleLength, pointer, record.SourceId, record.EventId);
      }

      pointer += titleLength;
    }

    unsigned short descriptorsLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;
    //LogDebug(L"EIT ATSC: descriptors length = %hu", descriptorsLength);

    unsigned short endOfDescriptors = pointer + descriptorsLength;
    if (endOfDescriptors > endOfSection)
    {
      LogDebug(L"EIT ATSC: invalid event record, descriptors length = %hu, pointer = %hu, end of section = %hu",
                descriptorsLength, pointer, endOfSection);
      return false;
    }

    while (pointer + 1 < endOfDescriptors)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      //LogDebug(L"EIT ATSC: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug(L"EIT ATSC: invalid event record, descriptor length = %hhu, pointer = %hu, end of descriptors = %hu, tag = 0x%hhx, end of section = %hu",
                  length, pointer, endOfDescriptors, tag, endOfSection);
        return false;
      }

      bool descriptorParseResult = true;
      if (tag == 0x81)  // AC3 audio stream descriptor
      {
        descriptorParseResult = DecodeAc3AudioStreamDescriptor(&sectionData[pointer],
                                                                length,
                                                                record.AudioLanguages);
      }
      else if (tag == 0x86) // caption service descriptor
      {
        descriptorParseResult = DecodeCaptionServiceDescriptor(&sectionData[pointer],
                                                                length,
                                                                record.CaptionsLanguages);
      }
      else if (tag == 0x87) // content advisory descriptor
      {
        descriptorParseResult = DecodeContentAdvisoryDescriptor(&sectionData[pointer],
                                                                length,
                                                                record.VchipRating,
                                                                record.MpaaClassification,
                                                                record.Advisories);
      }
      else if (tag == 0xab) // genre descriptor
      {
        descriptorParseResult = DecodeGenreDescriptor(&sectionData[pointer],
                                                      length,
                                                      record.GenreIds);
      }
      else if (tag == 0xcc)  // E-AC3 audio descriptor
      {
        descriptorParseResult = DecodeEnhancedAc3AudioDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  record.AudioLanguages);
      }

      if (!descriptorParseResult)
      {
        LogDebug(L"EIT ATSC: invalid event record descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptors = %hu",
                  tag, length, pointer, endOfDescriptors);
        return false;
      }

      pointer += length;
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT ATSC: unhandled exception in DecodeEventRecord()");
  }
  return false;
}

bool CParserEitAtsc::DecodeAc3AudioStreamDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    vector<unsigned long>& audioLanguages)
{
  if (dataLength < 3)
  {
    LogDebug(L"EIT ATSC: invalid AC3 audio stream descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char sampleRateCode = data[0] >> 5;
    unsigned char bsid = data[0] & 0x1f;
    unsigned char bitRateCode = data[1] >> 2;
    unsigned char surroundMode = data[1] & 3;
    unsigned char bsmod = data[2] >> 5;
    unsigned char numChannels = (data[2] & 0xf) >> 1;
    bool fullSvc = (data[2] & 1) != 0;
    //LogDebug(L"EIT ATSC: AC3 audio stream descriptor, length = %hhu, sample rate code = %hhu, bsid = %hhu, bit rate code = %hhu, surround mode = %hhu, bsmod. = %hhu, num. channels = %hhu, full svc. = %d",
    //          dataLength, sampleRateCode, bsid, bitRateCode, surroundMode,
    //          bsmod, numChannels, fullSvc);
    if (dataLength == 3)
    {
      return true;
    }

    unsigned char langcod = data[3];
    //LogDebug(L"  langcod = %hhu", langcod);
    if (dataLength == 4)
    {
      return true;
    }

    unsigned short pointer = 4;
    if (numChannels == 0)
    {
      unsigned char langcod2 = data[pointer++];
      //LogDebug(L"  langcod2 = %hhu", langcod2);
      if (pointer == dataLength)
      {
        return true;
      }
    }

    if (bsmod < 2)
    {
      unsigned char mainId = data[pointer] >> 5;
      unsigned char priority = (data[pointer++] & 3) >> 3;
      //LogDebug(L"  main ID = %hhu, priority = %hhu", mainId, priority);
    }
    else
    {
      unsigned char asvcFlags = data[pointer++];
      //LogDebug(L"  ASVC flags = %hhu", asvcFlags);
    }
    if (pointer == dataLength)
    {
      return true;
    }

    unsigned char textLen = data[pointer] >> 1;
    bool textCode = (data[pointer++] & 1) != 0;
    //LogDebug(L"  text length = %hhu, text code = %d", textLen, textCode);
    pointer += textLen;
    if (pointer > dataLength)
    {
      LogDebug(L"EIT ATSC: invalid AC3 audio stream descriptor, length = %hhu, num. channels = %hhu, text length = %hhu",
                numChannels, textLen);
      return false;
    }
    if (pointer == dataLength)
    {
      return true;
    }

    bool languageFlag = (data[pointer] & 0x80) != 0;
    bool languageFlag2 = (data[pointer++] & 0x40) != 0;
    //LogDebug(L"  language flag = %d, language flag 2 = %d",
    //          languageFlag, languageFlag2);

    unsigned char languageCount = 0;
    if (languageFlag)
    {
      languageCount++;
    }
    if (languageFlag2)
    {
      languageCount++;
    }
    for (unsigned char i = 0; i < languageCount; i++)
    {
      if (pointer + 3 > dataLength)
      {
        LogDebug(L"EIT ATSC: invalid AC3 audio stream descriptor, length = %hhu, num. channels = %hhu, text length = %hhu, language flag = %d, language flag 2 = %d",
                  numChannels, textLen, languageFlag, languageFlag2);
        return false;
      }
      unsigned long language = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      //LogDebug(L"  language = %S", (char*)&language);

      vector<unsigned long>::const_iterator it = find(audioLanguages.begin(),
                                                      audioLanguages.end(),
                                                      language);
      if (it == audioLanguages.end())
      {
        audioLanguages.push_back(language);
      }
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT ATSC: unhandled exception in DecodeAc3AudioStreamDescriptor()");
  }
  return false;
}

bool CParserEitAtsc::DecodeCaptionServiceDescriptor(const unsigned char* data,
                                                    unsigned char dataLength,
                                                    vector<unsigned long>& captionsLanguages)
{
  if (dataLength == 0)
  {
    LogDebug(L"EIT ATSC: invalid caption service descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char numberOfServices = data[0] & 0x1f;
    //LogDebug(L"EIT ATSC: caption service descriptor, number of services = %hhu",
    //          numberOfServices);
    if (1 + (numberOfServices * 6) > dataLength)
    {
      LogDebug(L"EIT ATSC: invalid caption service descriptor, length = %hhu, number of services = %hhu",
                dataLength, numberOfServices);
      return false;
    }

    unsigned short pointer = 1;
    for (unsigned char i = 0; i < numberOfServices; i++)
    {
      unsigned long language = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      bool digitalCc = (data[pointer] & 0x80) != 0;
      unsigned char line21Field = 0;
      unsigned char captionServiceNumber = 0;
      if (digitalCc)
      {
        captionServiceNumber = data[pointer++] & 0x3f;
      }
      else
      {
        line21Field = data[pointer++] & 1;
      }
      bool easyReader = (data[pointer] & 0x80) != 0;
      bool wideAspectRatio = (data[pointer] & 0x40) != 0;
      pointer += 2;
      //LogDebug(L"  language = %S, digital CC = %d, line 21 field = %hhu, caption service number = %hhu, easy reader = %d, wide aspect ratio = %d",
      //          language, digitalCc, line21Field, captionServiceNumber,
      //          easyReader, wideAspectRatio);

      if (language != 0)
      {
        vector<unsigned long>::const_iterator it = find(captionsLanguages.begin(),
                                                        captionsLanguages.end(),
                                                        language);
        if (it == captionsLanguages.end())
        {
          captionsLanguages.push_back(language);
        }
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT ATSC: unhandled exception in DecodeCaptionServiceDescriptor()");
  }
  return false;
}

bool CParserEitAtsc::DecodeContentAdvisoryDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned char& vchipRating,
                                                      unsigned char& mpaaClassification,
                                                      unsigned short& advisories)
{
  if (dataLength == 0)
  {
    LogDebug(L"EIT ATSC: invalid content advisory descriptor, length = %huu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char ratingRegionCount = data[0] & 0x3f;
    //LogDebug(L"EIT ATSC: content advisory descriptor, rating region count = %hhu",
    //          ratingRegionCount);
    if (1 + (ratingRegionCount * 3) > dataLength)
    {
      LogDebug(L"EIT ATSC: invalid content advisory descriptor, length = %hhu, rating region count = %hhu",
                dataLength, ratingRegionCount);
      return false;
    }

    unsigned short pointer = 1;
    for (unsigned char i = 0; i < ratingRegionCount && pointer + ((ratingRegionCount - i) * 3) - 1 < dataLength; i++)
    {
      unsigned char ratingRegion = data[pointer++];
      unsigned char ratedDimensions = data[pointer++];
      //LogDebug(L"  region = %hhu, dimensions = %hhu",
      //          ratingRegion, ratedDimensions);

      if (pointer + ((ratingRegionCount - 1 - i) * 3) + (ratedDimensions * 2) + 1 > dataLength)
      {
        LogDebug(L"EIT ATSC: invalid content advisory descriptor, descriptor length = %hhu, pointer = %hu, rating region count = %hhu, region index = %hhu, region = %hhu, rated dimensions = %hhu",
                  dataLength, pointer, ratingRegionCount, i, ratingRegion,
                  ratedDimensions);
        return false;
      }

      for (unsigned char j = 0; j < ratedDimensions && pointer + ((ratingRegionCount - 1 - i) * 3) + ((ratedDimensions - j) * 2) + 1 - 1 < dataLength; j++)
      {
        unsigned char ratingDimension = data[pointer++];
        unsigned char ratingValue = data[pointer++] & 0xf;
        //LogDebug(L"    dimension = %hhu, value = %hhu",
        //          ratingDimension, ratingValue);

        if (ratingRegion == 1)  // US (50 states and possessions)
        {
          // Translate to Dish/BEV encoding. Refer to
          // ParserEitDvb.DecodeDishBevRatingDescriptor() and
          // ParserEitDvb.DecodeDishVchipDescriptor().

          if (ratingDimension == 0)
          {
            // "EntireAudience"
            // 0 =
            // 1 = None
            // 2 = TV-G
            // 3 = TV-PG
            // 4 = TV-14
            // 5 = TV-MA
            if (ratingValue == 1)
            {
              vchipRating = 0;
            }
            else if (ratingValue >= 2 && ratingValue <= 5)
            {
              vchipRating = ratingValue + 1;
            }
          }
          else if (ratingDimension == 1 && ratingValue == 1)
          {
            // "Dialogue"
            // 0 =
            // 1 = D
            advisories |= 0x8000;
          }
          else if (ratingDimension == 2 && ratingValue == 1)
          {
            // "Language"
            // 0 =
            // 1 = L
            advisories |= 0x02;
          }
          else if (ratingDimension == 3 && ratingValue == 1)
          {
            // "Sex"
            // 0 =
            // 1 = S
            advisories |= 0x01;
          }
          else if (ratingDimension == 4 && ratingValue == 1)
          {
            // "Violence"
            // 0 =
            // 1 = V
            advisories |= 0x10;
          }
          else if (ratingDimension == 5)
          {
            // "Children"
            // 0 =
            // 1 = TV-Y
            // 2 = TV-Y7
            if (ratingValue == 1 || ratingValue == 2)
            {
              vchipRating = ratingValue;
            }
          }
          else if (ratingDimension == 6 && ratingValue == 1)
          {
            // "FantasyViolence"
            // 0 =
            // 1 = FV
            advisories |= 0x08;
          }
          else if (ratingDimension == 7)
          {
            // "MPAA"
            // 0 =
            // 1 = N/A ["MPAARatingNotApplicable"]
            // 2 = G ["SuitableforAllAges"]
            // 3 = PG ["ParentalGuidanceSuggested"]
            // 4 = PG-13 ["ParentsStronlyCautioned"]
            // 5 = R ["Restricted,Under17MustBeAccompaniedByAdult"]
            // 6 = NC-17 ["NoOne17AndUnderAdmitted"]
            // 7 = X ["NoOne17AndUnderAdmitted"]
            // 8 = NR ["NotRatedByMPAA"]
            if (ratingValue >= 1 && ratingValue <= 8)
            {
              mpaaClassification = ratingValue - 1;
            }
          }
        }
      }

      unsigned char ratingDescriptionLength = data[pointer++];
      //LogDebug(L"  description length = %hhu", ratingDescriptionLength);
      if (pointer + ratingDescriptionLength > dataLength)
      {
        LogDebug(L"EIT ATSC: invalid content advisory descriptor, descriptor length = %hhu, pointer = %hu, rating description length = %hhu, rating region count = %hhu, region index = %hhu, region = %hhu, rated dimensions = %hhu",
                  dataLength, pointer, ratingDescriptionLength,
                  ratingRegionCount, i, ratingRegion, ratedDimensions);
        return false;
      }
      pointer += ratingDescriptionLength;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT ATSC: unhandled exception in DecodeContentAdvisoryDescriptor()");
  }
  return false;
}

bool CParserEitAtsc::DecodeGenreDescriptor(const unsigned char* data,
                                            unsigned char dataLength,
                                            vector<unsigned char>& genreIds)
{
  if (dataLength == 0)
  {
    LogDebug(L"EIT ATSC: invalid genre descriptor, length = %hhu", dataLength);
    return false;
  }
  try
  {
    unsigned char attributeCount = data[0] & 0x1f;
    //LogDebug(L"EIT ATSC: genre descriptor, attribute count = %hhu",
    //          attributeCount);
    if (dataLength != 1 + attributeCount)
    {
      LogDebug(L"EIT ATSC: invalid genre descriptor, length = %hhu, attribute count = %hhu",
                dataLength, attributeCount);
      return false;
    }

    unsigned short pointer = 1;
    for (unsigned char i = 0; i < attributeCount && pointer + (attributeCount - i) - 1 < dataLength; i++)
    {
      unsigned char attribute = data[pointer++];
      //LogDebug(L"  %hhu", attribute);

      genreIds.push_back(attribute);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT ATSC: unhandled exception in DecodeGenreDescriptor()");
  }
  return false;
}

bool CParserEitAtsc::DecodeEnhancedAc3AudioDescriptor(const unsigned char* data,
                                                      unsigned char dataLength,
                                                      vector<unsigned long>& audioLanguages)
{
  if (dataLength < 2)
  {
    LogDebug(L"EIT ATSC: invalid E-AC3 audio descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    bool bsidFlag = (data[0] & 0x40) != 0;
    bool mainIdFlag = (data[0] & 0x20) != 0;
    bool asvcFlag = (data[0] & 0x10) != 0;
    bool mixInfoExists = (data[0] & 8) != 0;
    bool subStream1Flag = (data[0] & 4) != 0;
    bool subStream2Flag = (data[0] & 2) != 0;
    bool subStream3Flag = (data[0] & 1) != 0;
    bool fullServiceFlag = (data[1] & 0x40) != 0;
    unsigned char serviceType = (data[1] & 7) >> 3;
    unsigned char numberOfChannels = data[1] & 7;
    //LogDebug(L"EIT ATSC: E-AC3 audio descriptor, length = %hhu, bsid flag = %d, main ID flag = %d, ASVC flag = %d, mix info exists = %d, sub-stream 1 flag = %d, sub-stream 2 flag = %d, sub-stream 3 flag = %d, full service flag = %d, service type = %hhu, number of channels = %hhu",
    //          dataLength, bsidFlag, mainIdFlag, asvcFlag, mixInfoExists,
    //          subStream1Flag, subStream2Flag, subStream3Flag,
    //          fullServiceFlag, serviceType, numberOfChannels);
    if (dataLength == 2)
    {
      return true;
    }

    bool languageFlag = (data[2] & 0x80) != 0;
    bool languageFlag2 = (data[2] & 0x40) != 0;
    unsigned char bsid = data[2] & 0x1f;
    //LogDebug(L"  language flag = %d, language flag 2 = %d, bsid = %hhu",
    //          languageFlag, languageFlag2, bsid);
    if (dataLength == 3)
    {
      return true;
    }

    unsigned short pointer = 3;
    if (mainIdFlag)
    {
      unsigned char priority = (data[pointer] & 0x18) >> 3;
      unsigned char mainId = data[pointer++] & 7;
      //LogDebug(L"  priority = %hhu, main ID = %hhu", priority, mainId);
      if (pointer == dataLength)
      {
        return true;
      }
    }

    if (asvcFlag)
    {
      unsigned char asvc = data[pointer++];
      //LogDebug(L"  ASVC = %hhu", asvc);
      if (pointer == dataLength)
      {
        return true;
      }
    }

    unsigned char languageCount = 0;
    if (subStream1Flag)
    {
      unsigned char subStream1 = data[pointer++];
      //LogDebug(L"  sub-stream 1 = %hhu", subStream1);
      if (pointer == dataLength)
      {
        return true;
      }
      languageCount++;
    }

    if (subStream2Flag)
    {
      unsigned char subStream2 = data[pointer++];
      //LogDebug(L"  sub-stream 2 = %hhu", subStream2);
      if (pointer == dataLength)
      {
        return true;
      }
      languageCount++;
    }

    if (subStream3Flag)
    {
      unsigned char subStream3 = data[pointer++];
      //LogDebug(L"  sub-stream 3 = %hhu", subStream3);
      if (pointer == dataLength)
      {
        return true;
      }
      languageCount++;
    }

    if (languageFlag)
    {
      languageCount++;
    }
    if (languageFlag2)
    {
      languageCount++;
    }
    for (unsigned char i = 0; i < languageCount; i++)
    {
      if (pointer + 3 > dataLength)
      {
        LogDebug(L"EIT ATSC: invalid E-AC3 audio descriptor, length = %hhu, main ID flag = %d, ASVC flag = %d, sub-stream 1 flag = %d, sub-stream 2 flag = %d, sub-stream 3 flag = %d, language flag = %d, language flag 2 = %d",
                  dataLength, mainIdFlag, asvcFlag, subStream1Flag,
                  subStream2Flag, subStream3Flag, languageFlag, languageFlag2);
        return false;
      }
      unsigned long language = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      pointer += 3;
      //LogDebug(L"  language = %S", (char*)&language);

      vector<unsigned long>::const_iterator it = find(audioLanguages.begin(),
                                                      audioLanguages.end(),
                                                      language);
      if (it == audioLanguages.end())
      {
        audioLanguages.push_back(language);
      }
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT ATSC: unhandled exception in DecodeEnhancedAc3AudioDescriptor()");
  }
  return false;
}