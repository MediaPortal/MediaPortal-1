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
#include "ParserEtt.h"
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\TimeUtils.h"
#include "TextUtil.h"
#include "Utils.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserEtt::CParserEtt(unsigned short pid,
                        ISectionDispatcher* sectionDispatcher,
                        ICallBackTableParser* callBack)
  : CSectionDecoder(sectionDispatcher), m_records(600000)
{
  m_isSectionDecodingEnabled = true;
  m_isReady = false;
  m_completeTime = 0;

  SetPid(pid);
  m_callBack = callBack;
  m_currentRecord = NULL;
}

CParserEtt::~CParserEtt()
{
  m_callBack = NULL;
}

void CParserEtt::Reset(bool enableCrcCheck)
{
  LogDebug(L"ETT %d: reset", GetPid());
  CEnterCriticalSection lock(m_section);
  m_records.RemoveAllRecords();
  EnableCrcCheck(enableCrcCheck);
  CSectionDecoder::Reset();
  m_seenSections.clear();
  m_isReady = false;
  m_currentRecord = NULL;
  LogDebug(L"ETT %d: reset done", GetPid());
}

void CParserEtt::OnNewSection(const CSection& section)
{
  try
  {
    if (
      section.TableId != TABLE_ID_ETT ||
      !section.SectionSyntaxIndicator ||
      !section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.SectionLength > 4093 || section.SectionLength < 14)
    {
      LogDebug(L"ETT %d: invalid section, length = %hu",
                GetPid(), section.SectionLength);
      return;
    }
    unsigned char protocolVersion = section.Data[8];
    if (protocolVersion != 0)
    {
      LogDebug(L"ETT %d: unsupported protocol version, protocol version = %hhu",
                GetPid(), protocolVersion);
      return;
    }
    if (section.SectionNumber != 0 || section.LastSectionNumber != 0)
    {
      // According to ATSC A/65 ETT should only have one section.
      LogDebug(L"ETT %d: unsupported multi-section table, extension ID = %hu, protocol version = %hhu, version number = %hhu, section number = %hhu, last section number = %hhu",
                GetPid(), section.TableIdExtension, protocolVersion,
                section.VersionNumber, section.SectionNumber,
                section.LastSectionNumber);
      return;
    }

    CEnterCriticalSection lock(m_section);
    if (!m_isSectionDecodingEnabled)
    {
      return;
    }

    // Have we seen this section before?
    unsigned long sectionKey = (section.VersionNumber << 16) | section.TableIdExtension;
    vector<unsigned long>::const_iterator sectionIt = find(m_seenSections.begin(),
                                                            m_seenSections.end(),
                                                            sectionKey);
    if (sectionIt != m_seenSections.end())
    {
      // Yes. We might be ready!
      //LogDebug(L"ETT %d: previously seen section, extension ID = %hu, protocol version = %hhu, section number = %hhu",
      //          GetPid(), section.TableIdExtension, protocolVersion,
      //          section.SectionNumber);
      if (m_isReady)
      {
        return;
      }

      // ATSC A/69 section 5.1 table 5.1 recommended repetition interval for
      // ETT is 60 seconds.
      if (CTimeUtils::ElapsedMillis(m_completeTime) >= 30000)
      {
        if (m_records.RemoveExpiredRecords(NULL) != 0)
        {
          m_currentRecord = NULL;
        }

        LogDebug(L"ETT %d: ready, sections parsed = %llu, event count = %lu",
                  GetPid(), (unsigned long long)m_seenSections.size(),
                  m_records.GetRecordCount());
        m_isReady = true;

        // ***MUST*** release lock before call-back to avoid deadlock.
        lock.Leave();
        if (m_callBack != NULL)
        {
          m_callBack->OnTableComplete(TABLE_ID_ETT);
        }
      }
      return;
    }

    // Is this a change/update, or just a new section?
    if (m_seenSections.size() == 0)
    {
      LogDebug(L"ETT %d: received, protocol version = %hhu, version number = %hhu",
                GetPid(), protocolVersion, section.VersionNumber);

      // ***MUST*** release lock before call-back to avoid deadlock.
      m_isSectionDecodingEnabled = false;
      lock.Leave();
      if (m_callBack != NULL)
      {
        m_callBack->OnTableSeen(TABLE_ID_ETT);
      }
      lock.Enter();
      m_isSectionDecodingEnabled = true;
    }
    else if (m_isReady)
    {
      LogDebug(L"ETT %d: changed, protocol version = %hhu, version number = %hhu",
                GetPid(), protocolVersion, section.VersionNumber);
      m_records.MarkExpiredRecords(0);
      m_seenSections.clear();

      // ***MUST*** release lock before call-back to avoid deadlock.
      m_isSectionDecodingEnabled = false;
      lock.Leave();
      if (m_callBack != NULL)
      {
        m_callBack->OnTableChange(TABLE_ID_ETT);
      }
      lock.Enter();
      m_isSectionDecodingEnabled = true;
    }

    CRecordEtt* record = new CRecordEtt();
    if (record == NULL)
    {
      LogDebug(L"ETT %d: failed to allocate record, extension ID = %hu, protocol version = %hhu, version number = %hhu",
                GetPid(), section.TableIdExtension, protocolVersion,
                section.VersionNumber);
      return;
    }

    const unsigned char* data = section.Data;
    record->Id = section.TableIdExtension;
    record->SourceId = (data[9] << 8) | data[10];
    record->EventId = (data[11] << 6) | (data[12] >> 2);

    //LogDebug(L"ETT %d: extension ID = %hu, protocol version = %hhu, version number = %hhu, section length = %hu, source ID = %hu, event ID = %hu",
    //          GetPid(), section.TableIdExtension, protocolVersion,
    //          section.VersionNumber, section.SectionLength, record->SourceId,
    //          record->EventId);

    if (section.SectionLength - 14 > 0)
    {
      if (!CTextUtil::AtscScteMultipleStringStructureToStrings(&data[13],
                                                                section.SectionLength - 14,
                                                                record->Texts))
      {
        LogDebug(L"ETT %d: invalid section, section length = %hu, extension ID = %hu, protocol version = %hhu, version number = %hhu, source ID = %hu, event ID = %hu",
                  GetPid(), section.SectionLength, section.TableIdExtension,
                  protocolVersion, section.VersionNumber, record->SourceId,
                  record->EventId);
        delete record;
        return;
      }
      if (record->Texts.size() == 0)
      {
        LogDebug(L"ETT %d: failed to allocate event texts, section length = %hu, extension ID = %hu, protocol version = %hhu, version number = %hhu, source ID = %hu, event ID = %hu",
                  GetPid(), section.SectionLength, section.TableIdExtension,
                  protocolVersion, section.VersionNumber, record->SourceId,
                  record->EventId);
        delete record;
        return;
      }
    }

    m_seenSections.push_back(sectionKey);
    if (m_records.AddOrUpdateRecord((IRecord**)&record, NULL))
    {
      m_completeTime = clock();
    }
  }
  catch (...)
  {
    LogDebug(L"ETT %d: unhandled exception in OnNewSection()", GetPid());
  }
}

bool CParserEtt::IsSeen() const
{
  return m_seenSections.size() != 0;
}

bool CParserEtt::IsReady() const
{
  return m_isReady;
}

unsigned char CParserEtt::GetSourceTextCount(unsigned short sourceId)
{
  return GetEventTextCount(sourceId, 0);
}

bool CParserEtt::GetSourceTextByIndex(unsigned short sourceId,
                                      unsigned char index,
                                      unsigned long& language,
                                      char* text,
                                      unsigned short& textBufferSize)
{
  return GetEventTextByIndex(sourceId, 0, index, language, text, textBufferSize);
}

bool CParserEtt::GetSourceTextByLanguage(unsigned short sourceId,
                                          unsigned long language,
                                          char* text,
                                          unsigned short& textBufferSize)
{
  return GetEventTextByLanguage(sourceId, 0, language, text, textBufferSize);
}

unsigned char CParserEtt::GetEventTextCount(unsigned short sourceId,
                                            unsigned short eventId)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectTextRecordByIds(sourceId, eventId, false))
  {
    return 0;
  }
  return m_currentRecord->Texts.size();
}

bool CParserEtt::GetEventTextByIndex(unsigned short sourceId,
                                      unsigned short eventId,
                                      unsigned char index,
                                      unsigned long& language,
                                      char* text,
                                      unsigned short& textBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectTextRecordByIds(sourceId, eventId, true))
  {
    return false;
  }

  if (index >= m_currentRecord->Texts.size())
  {
    LogDebug(L"ETT %d: invalid index, ID = %hu, source ID = %hu, event ID = %hu, index = %hhu, count = %llu",
              GetPid(), m_currentRecord->Id, sourceId, eventId, index,
              (unsigned long long)m_currentRecord->Texts.size());
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->Texts.begin();
  for ( ; it != m_currentRecord->Texts.end(); it++)
  {
    if (index != 0)
    {
      index--;
      continue;
    }

    language = it->first;
    unsigned short requiredBufferSize = 0;
    if (!CUtils::CopyStringToBuffer(it->second, text, textBufferSize, requiredBufferSize))
    {
      LogDebug(L"ETT %d: insufficient buffer size, ID = %hu, source ID = %hu, event ID = %hu, index = %hhu, language = %S, required size = %hu, actual size = %hu",
                GetPid(), m_currentRecord->Id, sourceId, eventId, index,
                (char*)&language, requiredBufferSize, textBufferSize);
    }
    return true;
  }
  return false;
}

bool CParserEtt::GetEventTextByLanguage(unsigned short sourceId,
                                        unsigned short eventId,
                                        unsigned long language,
                                        char* text,
                                        unsigned short& textBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectTextRecordByIds(sourceId, eventId, true))
  {
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecord->Texts.find(language);
  if (it == m_currentRecord->Texts.end())
  {
    LogDebug(L"ETT %d: invalid language, ID = %hu, source ID = %hu, event ID = %hu, language = %S",
              GetPid(), m_currentRecord->Id, sourceId, eventId,
              (char*)&language);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(it->second, text, textBufferSize, requiredBufferSize))
  {
    LogDebug(L"ETT %d: insufficient buffer size, ID = %hu, source ID = %hu, event ID = %hu, language = %S, required size = %hu, actual size = %hu",
              GetPid(), m_currentRecord->Id, sourceId, eventId,
              (char*)&language, requiredBufferSize, textBufferSize);
  }
  return true;
}

bool CParserEtt::SelectTextRecordByIds(unsigned short sourceId,
                                        unsigned short eventId,
                                        bool isExpectedAvailable)
{
  if (
    m_currentRecord != NULL &&
    m_currentRecord->SourceId == sourceId &&
    m_currentRecord->EventId == eventId
  )
  {
    return true;
  }

  IRecord* record = NULL;
  if (!m_records.GetRecordByKey((sourceId << 16) | eventId, &record) || record == NULL)
  {
    if (isExpectedAvailable)
    {
      LogDebug(L"ETT %d: invalid identifiers, source ID = %hu, event ID = %hu",
                GetPid(), sourceId, eventId);
    }
    return false;
  }

  m_currentRecord = dynamic_cast<CRecordEtt*>(record);
  if (m_currentRecord == NULL)
  {
    LogDebug(L"ETT %d: invalid record, source ID = %hu, event ID = %hu",
              GetPid(), sourceId, eventId);
    return false;
  }
  return true;
}