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
#include "ParserEam.h"
#include <cstddef>      // NULL
#include <cstring>      // memcpy()
#include "..\..\shared\EnterCriticalSection.h"
#include "TextUtil.h"
#include "Utils.h"


#define MINIMUM_SECTION_LENGTH 40
#define SEQUENCE_NUMBER_NOT_SET 0xff


extern void LogDebug(const wchar_t* fmt, ...);

CParserEam::CParserEam(unsigned short pid)
{
  m_pid = pid;
  SetCallBack(NULL);
  m_sequenceNumber = SEQUENCE_NUMBER_NOT_SET;
  m_latestRecord = NULL;
}

CParserEam::~CParserEam()
{
  CEnterCriticalSection lock(m_section);
  m_callBack = NULL;
  if (m_latestRecord != NULL)
  {
    delete m_latestRecord;
    m_latestRecord = NULL;
  }
}

void CParserEam::Reset()
{
  LogDebug(L"EAM %hu: reset", m_pid);
  CEnterCriticalSection lock(m_section);
  m_sequenceNumber = SEQUENCE_NUMBER_NOT_SET;
  m_seenEventIds.clear();
  if (m_latestRecord != NULL)
  {
    delete m_latestRecord;
    m_latestRecord = NULL;
  }
  LogDebug(L"EAM %hu: reset done", m_pid);
}

void CParserEam::SetCallBack(ICallBackEam* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBack = callBack;
}

void CParserEam::OnNewSection(CSection& section)
{
  try
  {
    if (
      section.table_id != TABLE_ID_EAM ||
      !section.SectionSyntaxIndicator ||
      section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.section_length > 4093 || section.section_length < MINIMUM_SECTION_LENGTH)
    {
      LogDebug(L"EAM %hu: invalid section, length = %d",
                m_pid, section.section_length);
      return;
    }
    unsigned char protocolVersion = section.Data[8];
    if (protocolVersion != 0)
    {
      LogDebug(L"EAM %hu: unsupported protocol version, protocol version = %hhu",
                m_pid, protocolVersion);
      return;
    }
    if (section.SectionNumber != 0 || section.LastSectionNumber != 0)
    {
      // According to SCTE 18 EAM should only have one section per message.
      LogDebug(L"EAM %hu: unsupported multi-section message, protocol version = %hhu, sequence number = %d, section number = %d, last section number = %d",
                m_pid, protocolVersion, section.version_number,
                section.SectionNumber, section.LastSectionNumber);
      return;
    }

    CEnterCriticalSection lock(m_section);
    if (section.version_number == m_sequenceNumber)
    {
      //LogDebug(L"EAM %hu: previously seen message, protocol version = %hhu, sequence number = %d, section length = %d",
      //          m_pid, protocolVersion, section.version_number,
      //          section.section_length);
      return;
    }
    else if (m_latestRecord == NULL && m_callBack != NULL)
    {
      m_callBack->OnTableSeen(TABLE_ID_EAM);
    }

    CRecordEam* record = new CRecordEam();
    if (record == NULL)
    {
      LogDebug(L"EAM %hu: failed to allocate record, protocol version = %hhu, sequence number = %d",
                m_pid, protocolVersion, section.version_number);
      return;
    }

    unsigned char* data = section.Data;
    record->Id = (data[9] << 8) | data[10];
    record->OriginatorCode = data[11] | (data[12] << 8) | (data[13] << 16);
    unsigned char eventCodeLength = data[14];
    //LogDebug(L"EAM %hu: protocol version = %hhu, sequence number = %d, section length = %d, event ID = %hu, originator code = %S, event code length = %hhu",
    //          m_pid, protocolVersion, section.version_number,
    //          section.section_length, record->Id,
    //          (char*)&(record->OriginatorCode), eventCodeLength);

    unsigned short minimumSectionLength = MINIMUM_SECTION_LENGTH + eventCodeLength;
    if (eventCodeLength > 0)
    {
      if (minimumSectionLength > section.section_length)
      {
        LogDebug(L"EAM %hu: invalid section, event code length = %hhu, section length = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                  m_pid, eventCodeLength, section.section_length,
                  protocolVersion, section.version_number, record->Id);
        delete record;
        return;
      }

      record->EventCode = new char[eventCodeLength + 1];
      if (record->EventCode == NULL)
      {
        LogDebug(L"EAM %hu: failed to allocate %hu byte(s) for an event code, protocol version = %hhu, sequence number = %d, event ID = %hu",
                  m_pid, eventCodeLength + 1, protocolVersion,
                  section.version_number, record->Id);
      }
      else
      {
        memcpy(record->EventCode, &data[15], eventCodeLength);  // ASCII
        record->EventCode[eventCodeLength] = 0;   // NULL terminate
      }
    }

    unsigned short pointer = 15 + eventCodeLength;
    unsigned char natureOfActivationTextLength = data[pointer++];
    //LogDebug(L"EAM %hu: event code = %S, nature of activation text length = %hhu",
    //          m_pid, record->EventCode == NULL ? "" : record->EventCode,
    //          natureOfActivationTextLength);

    minimumSectionLength += natureOfActivationTextLength;
    if (natureOfActivationTextLength > 0)
    {
      if (
        minimumSectionLength > section.section_length ||
        !CTextUtil::AtscScteMultipleStringStructureToStrings(&data[pointer],
                                                              natureOfActivationTextLength,
                                                              record->NatureOfActivationTexts)
      )
      {
        LogDebug(L"EAM %hu: invalid section, nature of activation text length = %hhu, pointer = %hu, event code length = %hhu, section length = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                  m_pid, natureOfActivationTextLength, pointer,
                  eventCodeLength, section.section_length, protocolVersion,
                  section.version_number, record->Id);
        delete record;
        return;
      }

      if (record->NatureOfActivationTexts.size() == 0)
      {
        LogDebug(L"EAM %hu: failed to allocate nature of activation text(s), nature of activation text length = %hhu, pointer = %hu, protocol version = %hhu, sequence number = %d, event ID = %hu, section length = %d",
                  m_pid, natureOfActivationTextLength, pointer,
                  protocolVersion, section.version_number, record->Id,
                  section.section_length);
      }

      pointer += natureOfActivationTextLength;
    }

    record->AlertMessageTimeRemaining = data[pointer++];
    record->EventStartTime = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
    pointer += 4;
    record->EventDuration = (data[pointer] << 8) | data[pointer + 1];
    pointer += 3;   // extra + 1 for reserved byte
    record->AlertPriority = data[pointer++] & 0xf;
    record->DetailsOobSourceId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    record->DetailsMajorChannelNumber = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
    pointer += 2;
    record->DetailsMinorChannelNumber = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
    pointer += 2;

    // Pack one-part channel numbers into the major channel number.
    if (((record->DetailsMajorChannelNumber >> 4) & 0x3f) == 0x3f)
    {
      record->DetailsMajorChannelNumber = ((record->DetailsMajorChannelNumber & 0xf) << 10) | record->DetailsMinorChannelNumber;
      record->DetailsMinorChannelNumber = 0;
    }

    record->AudioOobSourceId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    unsigned short alertTextLength = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    //LogDebug(L"EAM %hu: alert message time remaining = %hhu s, event start time = %lu, event duration = %hu m, alert priority = %hhu, details OOB source ID = %hu, details major channel number = %hu, details minor channel number = %hu, audio OOB source ID = %hu, alert text length = %hu",
    //          m_pid, record->AlertMessageTimeRemaining,
    //          record->EventStartTime, record->EventDuration,
    //          record->AlertPriority, record->DetailsOobSourceId,
    //          record->DetailsMajorChannelNumber,
    //          record->DetailsMinorChannelNumber, record->AudioOobSourceId,
    //          alertTextLength);

    minimumSectionLength += alertTextLength;
    if (alertTextLength > 0)
    {
      if (
        minimumSectionLength > section.section_length ||
        !CTextUtil::AtscScteMultipleStringStructureToStrings(&data[pointer],
                                                              alertTextLength,
                                                              record->AlertTexts)
      )
      {
        LogDebug(L"EAM %hu: invalid section, alert text length = %hu, pointer = %hu, event code length = %hhu, nature of activation text length = %hhu, section length = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                  m_pid, alertTextLength, pointer, eventCodeLength,
                  natureOfActivationTextLength, section.section_length,
                  protocolVersion, section.version_number, record->Id);
        delete record;
        return;
      }

      if (record->AlertTexts.size() == 0)
      {
        LogDebug(L"EAM %hu: failed to allocate alert text(s), alert text length = %hu, pointer = %hu, protocol version = %hhu, sequence number = %d, event ID = %hu, section length = %d",
                  m_pid, alertTextLength, pointer, protocolVersion,
                  section.version_number, record->Id, section.section_length);
      }

      pointer += alertTextLength;
    }

    unsigned char locationCodeCount = data[pointer++];
    //LogDebug(L"EAM %hu: location code count = %hhu",
    //          m_pid, locationCodeCount);

    minimumSectionLength += (locationCodeCount * 3);
    if (minimumSectionLength > section.section_length)
    {
      LogDebug(L"EAM %hu: invalid section, location code count = %hhu, pointer = %hu, event code length = %hhu, nature of activation text length = %hhu, alert text length = %hu, section length = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                m_pid, locationCodeCount, pointer, eventCodeLength,
                natureOfActivationTextLength, alertTextLength,
                section.section_length, protocolVersion,
                section.version_number, record->Id);
      delete record;
      return;
    }
    for (unsigned char lc = 0; lc < locationCodeCount; lc++)
    {
      unsigned char stateCode = data[pointer++];
      unsigned char countySubdivision = data[pointer] >> 4;
      unsigned short countyCode = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
      pointer += 2;

      //LogDebug(L"  state code = %hhu, county sub-division = %hhu, county code = %hu",
      //          stateCode, countySubdivision, countyCode);
      record->LocationCodes.push_back((stateCode << 24) | (countySubdivision << 16) | countyCode);
    }

    unsigned char exceptionCount = data[pointer++];
    //LogDebug(L"EAM %hu: exception count = %hhu", m_pid, exceptionCount);

    minimumSectionLength += (exceptionCount * 5);
    if (minimumSectionLength > section.section_length)
    {
      LogDebug(L"EAM %hu: invalid section, exception count = %hhu, pointer = %hu, event code length = %hhu, nature of activation text length = %hhu, alert text length = %hu, location code count = %hhu, section length = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                m_pid, exceptionCount, pointer, eventCodeLength,
                natureOfActivationTextLength, alertTextLength,
                locationCodeCount, section.section_length, protocolVersion,
                section.version_number, record->Id);
      delete record;
      return;
    }
    for (unsigned char e = 0; e < exceptionCount; e++)
    {
      bool inBandReference = (data[pointer++] & 0x80) != 0;
      if (inBandReference)
      {
        unsigned short exceptionMajorChannelNumber = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
        pointer += 2;
        unsigned short exceptionMinorChannelNumber = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
        pointer += 2;

        // Pack one-part channel numbers into the major channel number.
        if (((exceptionMajorChannelNumber >> 4) & 0x3f) == 0x3f)
        {
          exceptionMajorChannelNumber = ((exceptionMajorChannelNumber & 0xf) << 10) | exceptionMinorChannelNumber;
          exceptionMinorChannelNumber = 0;
        }

        //LogDebug(L"  major channel number = %hu, minor channel number = %hu",
        //          exceptionMajorChannelNumber, exceptionMinorChannelNumber);
        record->Exceptions.push_back((1 << 31) | (exceptionMajorChannelNumber << 16) | exceptionMinorChannelNumber);
      }
      else
      {
        pointer += 2;
        unsigned short exceptionOobSourceId = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;

        //LogDebug(L"  OOB source ID = %hu", exceptionOobSourceId);
        record->Exceptions.push_back(exceptionOobSourceId);
      }
    }

    unsigned short descriptorsLength = ((data[pointer] & 0x3) << 8) | data[pointer + 1];
    pointer += 2;
    if (minimumSectionLength + descriptorsLength > section.section_length)
    {
      LogDebug(L"EAM %hu: invalid section, descriptors length = %hu, pointer = %hu, event code length = %hhu, nature of activation text length = %hhu, alert text length = %hu, location code count = %hhu, exception count = %hhu, section length = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                m_pid, descriptorsLength, pointer, eventCodeLength,
                natureOfActivationTextLength, alertTextLength,
                locationCodeCount, exceptionCount, section.section_length,
                protocolVersion, section.version_number, record->Id);
      delete record;
      return;
    }
    unsigned short endOfDescriptors = pointer + descriptorsLength;
    bool result = true;
    while (pointer + 1 < endOfDescriptors)
    {
      unsigned char tag = data[pointer++];
      unsigned char length = data[pointer++];
      //LogDebug(L"EAM %hu: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          m_pid, tag, length, pointer);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug(L"EAM %hu: invalid section, descriptor length = %hhu, pointer = %hu, end of descriptors = %hu, event code length = %hhu, nature of activation text length = %hhu, alert text length = %hu, location code count = %hhu, exception count = %hhu, section length = %d, tag = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                  m_pid, length, pointer, endOfDescriptors, eventCodeLength,
                  natureOfActivationTextLength, alertTextLength,
                  locationCodeCount, exceptionCount, section.section_length,
                  tag, protocolVersion, section.version_number, record->Id);
        delete record;
        return;
      }

      if (tag == 0)
      {
        result = DecodeInBandDetailsChannelDescriptor(&data[pointer], length,
                                                      record->DetailsRfChannel,
                                                      record->DetailsProgramNumber);
      }
      else if (tag == 1)
      {
        result = DecodeInBandExceptionChannelDescriptor(&data[pointer], length,
                                                        record->AlternativeExceptions);
      }
      else if (tag == 2)
      {
        result = DecodeAudioFileDescriptor(&data[pointer], length);
      }

      if (!result)
      {
        LogDebug(L"EAM %hu: invalid descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptors = %hu, protocol version = %hhu, sequence number = %d, event ID = %hu",
                  m_pid, tag, length, pointer, endOfDescriptors,
                  protocolVersion, section.version_number, record->Id);
        delete record;
        return;
      }
      pointer += length;
    }

    if (pointer != section.section_length - 1)
    {
      LogDebug(L"EAM %hu: section parsing error, pointer = %hu, event code length = %hhu, nature of activation text length = %hhu, alert text length = %hu, location code count = %hhu, exception count = %hhu, descriptors length = %d, section length = %d, protocol version = %hhu, sequence number = %d, event ID = %hu",
                m_pid, pointer, eventCodeLength, natureOfActivationTextLength,
                alertTextLength, locationCodeCount, exceptionCount,
                descriptorsLength, section.section_length, protocolVersion,
                section.version_number, record->Id);
      delete record;
      return;
    }

    bool doCompleteCallBack = true;
    if (m_seenEventIds[record->Id])
    {
      //LogDebug(L"EAM %hu: previously seen message, protocol version = %hhu, sequence number = %d, event ID = %hu",
      //          m_pid, protocolVersion, section.version_number, record->Id);
      doCompleteCallBack = false;
    }
    else if (record->AlertPriority == 0)
    {
      //LogDebug(L"EAM %hu: test and/or sequence establishment message, protocol version = %hhu, sequence number = %d, event ID = %hu",
      //          m_pid, protocolVersion, section.version_number, record->Id);
      m_seenEventIds[record->Id] = true;
      doCompleteCallBack = m_latestRecord == NULL;
    }
    else
    {
      if (m_latestRecord == NULL)
      {
        record->Debug(L"seen");
        record->OnReceived(m_callBack);
      }
      else
      {
        if (m_callBack != NULL)
        {
          m_callBack->OnTableChange(TABLE_ID_EAM);
        }
        record->Debug(L"changed");
        record->OnChanged(m_callBack);
      }
    }

    m_sequenceNumber = section.version_number;
    if (m_latestRecord != NULL)
    {
      delete m_latestRecord;
    }
    m_latestRecord = record;

    if (doCompleteCallBack && m_callBack != NULL)
    {
      m_callBack->OnTableComplete(TABLE_ID_EAM);
    }
  }
  catch (...)
  {
    LogDebug(L"EAM %hu: unhandled exception in OnNewSection()", m_pid);
  }
}

bool CParserEam::GetLatestMessage(unsigned short& id,
                                  unsigned long& originatorCode,
                                  char* eventCode,
                                  unsigned short& eventCodeBufferSize,
                                  unsigned char& alertMessageTimeRemaining,
                                  unsigned long& eventStartTime,
                                  unsigned short& eventDuration,
                                  unsigned char& alertPriority,
                                  unsigned short& detailsOobSourceId,
                                  unsigned short& detailsMajorChannelNumber,
                                  unsigned short& detailsMinorChannelNumber,
                                  unsigned char& detailsRfChannel,
                                  unsigned short& detailsProgramNumber,
                                  unsigned short& audioOobSourceId,
                                  unsigned char& textCount,
                                  unsigned long* locationCodes,
                                  unsigned char& locationCodeCount,
                                  unsigned long* exceptions,
                                  unsigned char& exceptionCount,
                                  unsigned long* alternativeExceptions,
                                  unsigned char& alternativeExceptionCount) const
{
  CEnterCriticalSection lock(m_section);
  if (m_latestRecord == NULL)
  {
    LogDebug(L"EAM %hu: no message available", m_pid);
    return false;
  }

  id = m_latestRecord->Id;
  originatorCode = m_latestRecord->OriginatorCode;
  alertMessageTimeRemaining = m_latestRecord->AlertMessageTimeRemaining;
  eventStartTime = m_latestRecord->EventStartTime;
  eventDuration = m_latestRecord->EventDuration;
  alertPriority = m_latestRecord->AlertPriority;
  detailsOobSourceId = m_latestRecord->DetailsOobSourceId;
  detailsMajorChannelNumber = m_latestRecord->DetailsMajorChannelNumber;
  detailsMinorChannelNumber = m_latestRecord->DetailsMinorChannelNumber;
  detailsRfChannel = m_latestRecord->DetailsRfChannel;
  detailsProgramNumber = m_latestRecord->DetailsProgramNumber;
  audioOobSourceId = m_latestRecord->AudioOobSourceId;
  textCount = m_latestRecord->AlertTexts.size();

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(m_latestRecord->EventCode,
                                  eventCode,
                                  eventCodeBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EAM %hu: insufficient event code buffer size, ID = %hu, required size = %hu, actual size = %hu",
              m_pid, id, requiredBufferSize, eventCodeBufferSize);
  }

  unsigned char requiredCount = 0;
  if (!CUtils::CopyVectorToArray(m_latestRecord->LocationCodes,
                                  locationCodes,
                                  locationCodeCount,
                                  requiredCount))
  {
    LogDebug(L"EAM %hu: insufficient location code array size, ID = %hu, required size = %hhu, actual size = %hhu",
              m_pid, id, requiredCount, locationCodeCount);
  }
  if (!CUtils::CopyVectorToArray(m_latestRecord->Exceptions,
                                  exceptions,
                                  exceptionCount,
                                  requiredCount))
  {
    LogDebug(L"EAM %hu: insufficient exception array size, ID = %hu, required size = %hhu, actual size = %hhu",
              m_pid, id, requiredCount, exceptionCount);
  }
  if (!CUtils::CopyVectorToArray(m_latestRecord->AlternativeExceptions,
                                  alternativeExceptions,
                                  alternativeExceptionCount,
                                  requiredCount))
  {
    LogDebug(L"EAM %hu: insufficient alternative exception array size, ID = %hu, required size = %hhu, actual size = %hhu",
              m_pid, id, requiredCount, alternativeExceptionCount);
  }

  return true;
}

bool CParserEam::GetLatestMessageTextByIndex(unsigned char index,
                                              unsigned long& language,
                                              char* alertText,
                                              unsigned short& alertTextBufferSize,
                                              char* natureOfActivationText,
                                              unsigned short& natureOfActivationTextBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  if (m_latestRecord == NULL)
  {
    LogDebug(L"EAM %hu: no message available", m_pid);
    return false;
  }

  if (index >= m_latestRecord->AlertTexts.size())
  {
    LogDebug(L"EAM %hu: invalid text index, ID = %hu, index = %hhu, count = %llu",
              m_pid, m_latestRecord->Id, index,
              (unsigned long long)m_latestRecord->AlertTexts.size());
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_latestRecord->AlertTexts.begin();
  for ( ; it != m_latestRecord->AlertTexts.end(); it++)
  {
    if (index != 0)
    {
      index--;
      continue;
    }

    language = it->first;
    unsigned short requiredBufferSize = 0;
    if (!CUtils::CopyStringToBuffer(it->second,
                                    alertText,
                                    alertTextBufferSize,
                                    requiredBufferSize))
    {
      LogDebug(L"EAM %hu: insufficient alert text buffer size, ID = %hu, index = %hhu, language = %S, required size = %hu, actual size = %hu",
                m_pid, m_latestRecord->Id, index, (char*)&language,
                requiredBufferSize, alertTextBufferSize);
    }

    it = m_latestRecord->NatureOfActivationTexts.find(language);
    char* temp = NULL;
    if (it != m_latestRecord->NatureOfActivationTexts.end())
    {
      temp = it->second;
    }
    if (!CUtils::CopyStringToBuffer(temp,
                                    natureOfActivationText,
                                    natureOfActivationTextBufferSize,
                                    requiredBufferSize))
    {
      LogDebug(L"EAM %hu: insufficient nature of activation text buffer size, ID = %hu, index = %hhu, language = %S, required size = %hu, actual size = %hu",
                m_pid, m_latestRecord->Id, index, (char*)&language,
                requiredBufferSize, natureOfActivationTextBufferSize);
    }
    return true;
  }
  return false;
}

bool CParserEam::GetLatestMessageTextByLanguage(unsigned long language,
                                                char* alertText,
                                                unsigned short& alertTextBufferSize,
                                                char* natureOfActivationText,
                                                unsigned short& natureOfActivationTextBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  if (m_latestRecord == NULL)
  {
    LogDebug(L"EAM %hu: no message available", m_pid);
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_latestRecord->AlertTexts.find(language);
  if (it == m_latestRecord->AlertTexts.end())
  {
    LogDebug(L"EAM %hu: invalid alert text language, ID = %hu, language = %S",
              m_pid, m_latestRecord->Id, (char*)language);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(it->second,
                                  alertText,
                                  alertTextBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EAM %hu: insufficient alert text buffer size, ID = %hu, language = %S, required size = %hu, actual size = %hu",
              m_pid, m_latestRecord->Id, (char*)&language, requiredBufferSize,
              alertTextBufferSize);
  }

  it = m_latestRecord->NatureOfActivationTexts.find(language);
  char* temp = NULL;
  if (it != m_latestRecord->NatureOfActivationTexts.end())
  {
    temp = it->second;
  }
  if (!CUtils::CopyStringToBuffer(temp,
                                  natureOfActivationText,
                                  natureOfActivationTextBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EAM %hu: insufficient nature of activation text buffer size, ID = %hu, language = %S, required size = %hu, actual size = %hu",
              m_pid, m_latestRecord->Id, (char*)&language,
              requiredBufferSize, natureOfActivationTextBufferSize);
  }
  return true;
}

bool CParserEam::DecodeInBandDetailsChannelDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned char& rfChannel,
                                                      unsigned short& programNumber)
{
  if (dataLength != 3)
  {
    LogDebug(L"EAM: invalid in band details channel descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    rfChannel = data[0];
    programNumber = (data[1] << 8) | data[2];
    //LogDebug(L"EAM: in band details channel descriptor, RF channel = %hhu, program number = %hu",
    //          rfChannel, programNumber);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EAM: unhandled exception in DecodeInBandDetailsChannelDescriptor()");
  }
  return false;
}

bool CParserEam::DecodeInBandExceptionChannelDescriptor(unsigned char* data,
                                                        unsigned char dataLength,
                                                        vector<unsigned long>& channels)
{
  if (dataLength == 0 || (dataLength - 1) % 3 != 0)
  {
    LogDebug(L"EAM: invalid in band exception channel descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char channelCount = data[0];
    //LogDebug(L"EAM: in band exception channel descriptor, channel count = %hhu",
    //          channelCount);
    if (dataLength != (channelCount * 3) + 1)
    {
      LogDebug(L"EAM: invalid in band exception channel descriptor, length = %hhu, channel count = %hhu",
                dataLength, channelCount);
      return false;
    }

    unsigned short pointer = 1;
    for (unsigned char i = 0; i < channelCount && pointer + ((channelCount - i) * 3) - 1 < dataLength; i++)
    {
      unsigned char rfChannel = data[pointer++];
      unsigned short programNumber = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      //LogDebug(L"  RF channel = %hhu, program number = %hu",
      //          rfChannel, programNumber);

      channels.push_back((rfChannel << 16) | programNumber);
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EAM: unhandled exception in DecodeInBandExceptionChannelDescriptor()");
  }
  return false;
}

bool CParserEam::DecodeAudioFileDescriptor(unsigned char* data, unsigned char dataLength)
{
  if (dataLength == 0)
  {
    LogDebug(L"EAM: invalid audio file descriptor, length = %hhu", dataLength);
    return false;
  }
  try
  {
    unsigned char numberOfAudioSources = data[0];
    //LogDebug(L"EAM: audio file descriptor, number of audio sources = %hhu",
    //          numberOfAudioSources);
    if ((numberOfAudioSources * 3) + 1 > dataLength)
    {
      LogDebug(L"EAM: invalid audio file descriptor, length = %hhu, number of audio sources = %hhu",
                dataLength, numberOfAudioSources);
      return false;
    }

    unsigned short pointer = 1;
    for (unsigned char i = 0; i < numberOfAudioSources && pointer + ((numberOfAudioSources - i) * 3) - 1 < dataLength; i++)
    {
      unsigned char loopLength = data[pointer++];
      unsigned short endOfLoop = pointer + loopLength;
      if (endOfLoop > dataLength)
      {
        LogDebug(L"EAM: invalid audio file descriptor, loop length = %hhu, pointer = %hu, descriptor length = %hhu, number of audio sources = %hhu, source index = %hhu",
                  loopLength, pointer, dataLength, numberOfAudioSources, i);
        return false;
      }

      bool fileNamePresent = (data[pointer] & 0x80) != 0;
      unsigned char audioFormat = data[pointer++] & 0x7f;
      char* fileName = NULL;
      if (fileNamePresent)
      {
        unsigned char fileNameLength = data[pointer++];
        if (pointer + fileNameLength + 1 > endOfLoop)
        {
          LogDebug(L"EAM: invalid audio file descriptor, file name length = %hhu, pointer = %hu, end of loop = %hu, number of audio sources = %hhu, source index = %hhu, audio format = %hhu",
                    fileNameLength, pointer, endOfLoop, numberOfAudioSources,
                    i, audioFormat);
          return false;
        }

        fileName = new char[fileNameLength + 1];
        if (fileName == NULL)
        {
          LogDebug(L"EAM: failed to allocate %hu byte(s) for an audio file descriptor file name, audio format = %hhu",
                    fileNameLength + 1, audioFormat);
        }
        else
        {
          memcpy(fileName, &data[pointer], fileNameLength);
          fileName[fileNameLength] = NULL;
        }
        pointer += fileNameLength;
      }

      unsigned char audioSource = data[pointer++];
      //LogDebug(L"  loop length = %hhu, file name = %S, audio format = %hhu, audio source = %hhu",
      //          loopLength, fileName == NULL ? "" : fileName, audioFormat,
      //          audioSource);

      if (
        (audioSource == 1 && pointer + 8 != endOfLoop) ||
        (audioSource == 2 && pointer + 12 != endOfLoop)
      )
      {
        LogDebug(L"EAM: invalid audio file descriptor, pointer = %hu, end of loop = %hu, number of audio sources = %hhu, source index = %hhu, audio format = %hhu, audio source = %hhu",
                  pointer, endOfLoop, numberOfAudioSources, i, audioFormat,
                  audioSource);
        if (fileName != NULL)
        {
          delete[] fileName;
        }
        return false;
      }

      if (audioSource == 1)
      {
        unsigned short programNumber = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        unsigned long carouselId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;
        unsigned short applicationId = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        //LogDebug(L"    program number = %hu, carousel ID = %lu, application ID = %hu",
        //          programNumber, carouselId, applicationId);
      }
      else if (audioSource == 2)
      {
        unsigned short programNumber = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        unsigned long downloadId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;
        unsigned long moduleId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;
        unsigned short applicationId = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        //LogDebug(L"    program number = %hu, download ID = %lu, module ID = %lu, application ID = %hu",
        //          programNumber, downloadId, moduleId, applicationId);
      }

      if (fileName != NULL)
      {
        delete[] fileName;
      }

      pointer = endOfLoop;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EAM: unhandled exception in DecodeAudioFileDescriptor()");
  }
  return false;
}