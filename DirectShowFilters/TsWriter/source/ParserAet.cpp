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
#include "ParserAet.h"
#include <algorithm>    // find()
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\TimeUtils.h"
#include "GrabberSiAtscScte.h"
#include "ParserSttScte.h"
#include "PidUsage.h"
#include "TextUtil.h"


#define MINIMUM_SECTION_LENGTH 10
#define MINIMUM_RECORD_BYTE_COUNT_AEIT_SOURCE 3
#define MINIMUM_RECORD_BYTE_COUNT_AEIT_EVENT 12
#define MINIMUM_RECORD_BYTE_COUNT_AETT 6


extern void LogDebug(const wchar_t* fmt, ...);

CParserAet::CParserAet(ICallBackPidConsumer* callBack,
                        ISectionDispatcher* sectionDispatcher,
                        ISystemTimeInfoProviderAtscScte* systemTimeInfoProvider,
                        LPUNKNOWN unk,
                        HRESULT* hr)
  : CUnknown(NAME("SCTE EPG Grabber"), unk), m_recordsAeit(600000),
    m_recordsAett(600000)
{
  if (callBack == NULL)
  {
    LogDebug(L"AET: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }
  if (systemTimeInfoProvider == NULL)
  {
    LogDebug(L"AET: system time information provider not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_isGrabbing = false;
  m_isReadyAet = false;
  m_completeTime = 0;

  m_callBackGrabber = NULL;
  m_callBackPidConsumer = callBack;
  m_sectionDispatcher = sectionDispatcher;
  m_systemTimeInfoProvider = systemTimeInfoProvider;
  m_isReadyStt = false;
  m_gpsUtcOffset = 0;
  m_enableCrcCheck = true;

  m_currentRecordAeit = NULL;
  m_currentRecordAett = NULL;
  m_currentRecordIndex = 0xffffffff;
}

CParserAet::~CParserAet()
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = NULL;
  m_callBackPidConsumer = NULL;
  map<unsigned short, CSectionDecoder*>::iterator it = m_decoders.begin();
  for ( ; it != m_decoders.end(); it++)
  {
    if (it->second != NULL)
    {
      delete it->second;
      it->second = NULL;
    }
  }
  m_decoders.clear();
}

STDMETHODIMP CParserAet::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  if (ppv == NULL)
  {
    return E_INVALIDARG;
  }

  if (iid == IID_IGRABBER)
  {
    return GetInterface((IGrabber*)this, ppv);
  }
  if (iid == IID_IGRABBER_EPG_ATSC)
  {
    return GetInterface((IGrabberEpgAtsc*)this, ppv);
  }
  if (iid == IID_IGRABBER_EPG_SCTE)
  {
    return GetInterface((IGrabberEpgScte*)this, ppv);
  }
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

void CParserAet::AddDecoders(const vector<unsigned short>& pids)
{
  LogDebug(L"AET: add decoders...");
  CUtils::DebugVector(pids, L"PIDs", false);
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> newPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CSectionDecoder*>::const_iterator decoderIt = m_decoders.find(pid);
    if (decoderIt != m_decoders.end())
    {
      LogDebug(L"AET: decoder already exists, PID %hu", pid);
      continue;
    }

    CSectionDecoder* decoder = new CSectionDecoder(m_sectionDispatcher);
    if (decoder == NULL)
    {
      LogDebug(L"AET: failed to allocate section decoder for PID %hu", pid);
      return;
    }
    decoder->SetPid(pid);
    decoder->SetCallBack(this);
    decoder->Reset();
    decoder->EnableCrcCheck(m_enableCrcCheck);
    m_decoders[pid] = decoder;
    newPids.push_back(pid);
  }
  if (newPids.size() > 0 && m_isGrabbing && m_callBackPidConsumer != NULL)
  {
    m_callBackPidConsumer->OnPidsRequired(&newPids[0], newPids.size(), Epg);
  }
}

void CParserAet::RemoveDecoders(const vector<unsigned short>& pids)
{
  LogDebug(L"AET: remove decoders...");
  CUtils::DebugVector(pids, L"PIDs", false);
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> oldPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CSectionDecoder*>::iterator decoderIt = m_decoders.find(pid);
    if (decoderIt == m_decoders.end())
    {
      LogDebug(L"AET: decoder does not exist, PID = %hu", pid);
      continue;
    }
    if (decoderIt->second != NULL)
    {
      delete decoderIt->second;
      decoderIt->second = NULL;
      oldPids.push_back(pid);
    }
    m_decoders.erase(decoderIt);
  }
  if (oldPids.size() > 0)
  {
    m_currentRecordAeit = NULL;
    m_currentRecordAett = NULL;
    m_currentRecordIndex = 0xffffffff;

    if (m_isGrabbing && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&oldPids[0], oldPids.size(), Epg);
    }
  }
}

void CParserAet::OnTableSeen(unsigned char tableId)
{
  // Nothing to do. Seeing STT doesn't mean we've seen EPG.
}

void CParserAet::OnTableComplete(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (tableId == TABLE_ID_STT_SCTE && !m_isReadyStt)
  {
    unsigned long systemTime;
    unsigned char gpsUtcOffset;
    bool isDaylightSavingStateKnown;
    bool isDaylightSaving;
    unsigned char daylightSavingDayOfMonth;
    unsigned char daylightSavingHour;
    if (!m_systemTimeInfoProvider->GetSystemTimeDetail(systemTime,
                                                        gpsUtcOffset,
                                                        isDaylightSavingStateKnown,
                                                        isDaylightSaving,
                                                        daylightSavingDayOfMonth,
                                                        daylightSavingHour))
    {
      return;
    }

    // Careful! Seeing the STT complete doesn't mean we've seen EPG.
    m_gpsUtcOffset = gpsUtcOffset;
    m_isReadyStt = true;
    if (m_isReadyAet && m_callBackGrabber != NULL)
    {
      m_callBackGrabber->OnTableComplete(PID_AET_CALL_BACK, TABLE_ID_AET_CALL_BACK);
    }
  }
}

void CParserAet::OnTableChange(unsigned char tableId)
{
  // Careful! Seeing the STT change doesn't mean we've seen EPG.
  CEnterCriticalSection lock(m_section);
  if (tableId == TABLE_ID_STT_SCTE && m_isReadyStt)
  {
    m_isReadyStt = false;
    if (m_isReadyAet && m_callBackGrabber != NULL)
    {
      m_callBackGrabber->OnTableChange(PID_AET_CALL_BACK, TABLE_ID_AET_CALL_BACK);
    }
  }
}

STDMETHODIMP_(void) CParserAet::Start()
{
  LogDebug(L"AET: start");
  CEnterCriticalSection lock(m_section);
  if (!m_isGrabbing)
  {
    m_isGrabbing = true;

    if (m_callBackPidConsumer != NULL)
    {
      vector<unsigned short> pids;
      pids.push_back(PID_SCTE_BASE);    // For the MGT, which gives us the AEIT and AETT PIDs.
      map<unsigned short, CSectionDecoder*>::iterator it = m_decoders.begin();
      for ( ; it != m_decoders.end(); it++)
      {
        pids.push_back(it->first);
      }
      m_callBackPidConsumer->OnPidsRequired(&pids[0], pids.size(), Epg);
    }
  }
}

STDMETHODIMP_(void) CParserAet::Stop()
{
  LogDebug(L"AET: stop");
  CEnterCriticalSection lock(m_section);
  if (m_isGrabbing)
  {
    PrivateReset(false);
    m_isGrabbing = false;

    if (m_callBackPidConsumer != NULL)
    {
      vector<unsigned short> pids;
      pids.push_back(PID_SCTE_BASE);
      map<unsigned short, CSectionDecoder*>::iterator it = m_decoders.begin();
      for ( ; it != m_decoders.end(); it++)
      {
        pids.push_back(it->first);
      }
      m_callBackPidConsumer->OnPidsNotRequired(&pids[0], pids.size(), Epg);
    }
  }
}

void CParserAet::Reset(bool enableCrcCheck)
{
  LogDebug(L"AET: reset");
  m_enableCrcCheck = enableCrcCheck;
  PrivateReset(true);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnReset(PID_AET_CALL_BACK);
  }
  LogDebug(L"AET: reset done");
}

STDMETHODIMP_(void) CParserAet::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
}

bool CParserAet::OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket)
{
  CEnterCriticalSection lock(m_section);
  if (m_isGrabbing)
  {
    map<unsigned short, CSectionDecoder*>::const_iterator it = m_decoders.find(header.Pid);
    if (it != m_decoders.end() && it->second != NULL)
    {
      it->second->OnTsPacket(header, tsPacket);
      return true;
    }
  }
  return false;
}

STDMETHODIMP_(bool) CParserAet::IsSeen()
{
  CEnterCriticalSection lock(m_section);
  return m_seenSections.size() != 0;
}

STDMETHODIMP_(bool) CParserAet::IsReady()
{
  CEnterCriticalSection lock(m_section);
  return m_isReadyAet && m_isReadyStt;
}

STDMETHODIMP_(unsigned long) CParserAet::GetEventCount()
{
  CEnterCriticalSection lock(m_section);
  return m_recordsAeit.GetRecordCount();
}

STDMETHODIMP_(bool) CParserAet::GetEvent(unsigned long index,
                                          unsigned short* sourceId,
                                          unsigned short* eventId,
                                          unsigned long long* startDateTime,
                                          unsigned long* duration,
                                          unsigned char* titleCount,
                                          unsigned long* audioLanguages,
                                          unsigned char* audioLanguageCount,
                                          unsigned long* captionsLanguages,
                                          unsigned char* captionsLanguageCount,
                                          unsigned char* genreIds,
                                          unsigned char* genreIdCount,
                                          unsigned char* vchipRating,
                                          unsigned char* mpaaClassification,
                                          unsigned short* advisories)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(index))
  {
    return false;
  }

  *sourceId = m_currentRecordAeit->SourceId;
  *eventId = m_currentRecordAeit->EventId;

  // Start date/time is a GPS time. To convert to epoch/Unix/POSIX time we
  // must subtract the number of leap seconds between 1980 and now (because
  // GPS time includes leap seconds but epoch/Unix/POSIX time doesn't), and
  // add the number of seconds between the start of epoch/Unix/POSIX time and
  // GPS time.
  *startDateTime = m_currentRecordAeit->StartDateTime + GPS_TIME_START_OFFSET - m_gpsUtcOffset;

  *duration = m_currentRecordAeit->Duration;
  *titleCount = m_currentRecordAeit->Titles.size();
  *vchipRating = m_currentRecordAeit->VchipRating;
  *mpaaClassification = m_currentRecordAeit->MpaaClassification;
  *advisories = m_currentRecordAeit->Advisories;

  unsigned char requiredCount = 0;
  if (!CUtils::CopyVectorToArray(m_currentRecordAeit->AudioLanguages,
                                  audioLanguages,
                                  *audioLanguageCount,
                                  requiredCount))
  {
    LogDebug(L"AET: insufficient audio language array size, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, required size = %hhu, actual size = %hhu",
              index, m_currentRecordAeit->MgtTag, *sourceId, *eventId,
              requiredCount, *audioLanguageCount);
  }
  if (!CUtils::CopyVectorToArray(m_currentRecordAeit->CaptionsLanguages,
                                  captionsLanguages,
                                  *captionsLanguageCount,
                                  requiredCount))
  {
    LogDebug(L"AET: insufficient captions language array size, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, required size = %hhu, actual size = %hhu",
              index, m_currentRecordAeit->MgtTag, *sourceId, *eventId,
              requiredCount, *captionsLanguageCount);
  }
  if (!CUtils::CopyVectorToArray(m_currentRecordAeit->GenreIds,
                                  genreIds,
                                  *genreIdCount,
                                  requiredCount))
  {
    LogDebug(L"AET: insufficient genre ID array size, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, required size = %hhu, actual size = %hhu",
              index, m_currentRecordAeit->MgtTag, *sourceId, *eventId,
              requiredCount, *genreIdCount);
  }

  return true;
}

STDMETHODIMP_(bool) CParserAet::GetEventTextByIndex(unsigned long eventIndex,
                                                    unsigned char textIndex,
                                                    unsigned long* language,
                                                    char* title,
                                                    unsigned short* titleBufferSize,
                                                    char* text,
                                                    unsigned short* textBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(eventIndex))
  {
    return false;
  }

  if (textIndex >= m_currentRecordAeit->Titles.size())
  {
    LogDebug(L"AET: invalid text index, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, text index = %hhu, text count = %llu",
              eventIndex, m_currentRecordAeit->MgtTag,
              m_currentRecordAeit->SourceId, m_currentRecordAeit->EventId,
              textIndex,
              (unsigned long long)m_currentRecordAeit->Titles.size());
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecordAeit->Titles.begin();
  for ( ; it != m_currentRecordAeit->Titles.end(); it++)
  {
    if (textIndex != 0)
    {
      textIndex--;
      continue;
    }

    *language = it->first;
    unsigned short requiredBufferSize = 0;
    if (!CUtils::CopyStringToBuffer(it->second, title, *titleBufferSize, requiredBufferSize))
    {
      LogDebug(L"AET: insufficient title buffer size, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, text index = %hhu, language = %S, required size = %hu, actual size = %hu",
                eventIndex, m_currentRecordAeit->MgtTag,
                m_currentRecordAeit->SourceId, m_currentRecordAeit->EventId,
                textIndex, (char*)language, requiredBufferSize,
                *titleBufferSize);
    }

    char* temp = NULL;
    if (m_currentRecordAett != NULL)
    {
      it = m_currentRecordAett->Texts.find(*language);
      if (it != m_currentRecordAett->Texts.end())
      {
        temp = it->second;
      }
    }

    if (!CUtils::CopyStringToBuffer(temp, text, *textBufferSize, requiredBufferSize))
    {
      LogDebug(L"AET: insufficient text buffer size, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, text index = %hhu, language = %S, required size = %hu, actual size = %hu",
                eventIndex, m_currentRecordAett->MgtTag,
                m_currentRecordAett->SourceId, m_currentRecordAett->EventId,
                textIndex, (char*)language, requiredBufferSize,
                *textBufferSize);
    }
    return true;
  }
  return false;
}

STDMETHODIMP_(bool) CParserAet::GetEventTextByLanguage(unsigned long eventIndex,
                                                        unsigned long language,
                                                        char* title,
                                                        unsigned short* titleBufferSize,
                                                        char* text,
                                                        unsigned short* textBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(eventIndex))
  {
    return false;
  }

  map<unsigned long, char*>::const_iterator it = m_currentRecordAeit->Titles.find(language);
  if (it == m_currentRecordAeit->Titles.end())
  {
    LogDebug(L"AET: invalid text language, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, language = %S",
              eventIndex, m_currentRecordAeit->MgtTag,
              m_currentRecordAeit->SourceId, m_currentRecordAeit->EventId,
              (char*)&language);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(it->second, title, *titleBufferSize, requiredBufferSize))
  {
    LogDebug(L"AET: insufficient title buffer size, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, language = %S, required size = %hu, actual size = %hu",
              eventIndex, m_currentRecordAeit->MgtTag,
              m_currentRecordAeit->SourceId, m_currentRecordAeit->EventId,
              (char*)&language, requiredBufferSize, *titleBufferSize);
  }

  char* temp = NULL;
  if (m_currentRecordAett != NULL)
  {
    it = m_currentRecordAett->Texts.find(language);
    if (it != m_currentRecordAett->Texts.end())
    {
      temp = it->second;
    }
  }

  if (!CUtils::CopyStringToBuffer(temp, text, *textBufferSize, requiredBufferSize))
  {
    LogDebug(L"AET: insufficient text buffer size, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu, language = %S, required size = %hu, actual size = %hu",
              eventIndex, m_currentRecordAett->MgtTag,
              m_currentRecordAett->SourceId, m_currentRecordAett->EventId,
              (char*)&language, requiredBufferSize, *textBufferSize);
  }
  return true;
}

bool CParserAet::SelectEventRecordByIndex(unsigned long index)
{
  if (m_currentRecordAeit != NULL && m_currentRecordIndex == index)
  {
    return true;
  }

  IRecord* record = NULL;
  if (!m_recordsAeit.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"AET: invalid event index, index = %lu, record count = %lu",
              index, m_recordsAeit.GetRecordCount());
    return false;
  }

  m_currentRecordAeit = dynamic_cast<CRecordAeit*>(record);
  if (m_currentRecordAeit == NULL)
  {
    LogDebug(L"AET: invalid event record, index = %lu", index);
    return false;
  }

  if (
    !m_recordsAett.GetRecordByKey(m_currentRecordAeit->GetKey(), &record) ||
    record == NULL
  )
  {
    m_currentRecordAett = NULL;
  }
  else
  {
    m_currentRecordAett = dynamic_cast<CRecordAett*>(record);
    if (m_currentRecordAett == NULL)
    {
      LogDebug(L"AET: invalid text record, event index = %lu, MGT tag = %hhu, source ID = %hu, event ID = %hu",
                index, m_currentRecordAeit->MgtTag,
                m_currentRecordAeit->SourceId, m_currentRecordAeit->EventId);
    }
  }

  m_currentRecordIndex = index;
  return true;
}

void CParserAet::OnNewSection(unsigned short pid, unsigned char tableId, const CSection& section)
{
  try
  {
    if (
      (tableId != TABLE_ID_AEIT && tableId != TABLE_ID_AETT) ||
      !section.SectionSyntaxIndicator ||
      !section.PrivateIndicator ||
      !section.CurrentNextIndicator
    )
    {
      return;
    }
    if (section.SectionLength < MINIMUM_SECTION_LENGTH || section.SectionLength > 4093)
    {
      LogDebug(L"AET: invalid section, length = %hu, table ID = 0x%hhx, PID = %hu",
                section.SectionLength, tableId, pid);
      return;
    }
    unsigned char subtype = section.Data[3];
    if (subtype != 0)
    {
      LogDebug(L"AET: unsupported sub-type, PID = %hu, table ID = 0x%hhx, sub-type = %hhu",
                pid, tableId, subtype);
      return;
    }

    CEnterCriticalSection lock(m_section);
    const unsigned char* data = section.Data;
    unsigned char mgtTag = data[4];
    unsigned char numRecordsInSection = data[8];  // sources or blocks
    //LogDebug(L"AET: PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section length = %hu, section number = %d, last section number = %d, num. records in section = %hhu",
    //          pid, tableId, subtype, mgtTag, section.VersionNumber,
    //          section.SectionLength, section.SectionNumber,
    //          section.LastSectionNumber, numRecordsInSection);

    unsigned char minimumRecordByteCount;
    if (tableId == TABLE_ID_AEIT)
    {
      minimumRecordByteCount = MINIMUM_RECORD_BYTE_COUNT_AEIT_SOURCE;
    }
    else
    {
      minimumRecordByteCount = MINIMUM_RECORD_BYTE_COUNT_AETT;
    }
    if (MINIMUM_SECTION_LENGTH + (numRecordsInSection * minimumRecordByteCount) > section.SectionLength)
    {
      LogDebug(L"AET: invalid section, num. records in section = %hhu, section length = %hu, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d",
                numRecordsInSection, section.SectionLength, pid, tableId,
                subtype, mgtTag, section.VersionNumber,
                section.SectionNumber);
      return;
    }

    // Have we seen this section before?
    unsigned long long sectionKey = ((unsigned long long)pid << 32) | (tableId << 24) | (section.VersionNumber << 16) | (mgtTag << 8) | section.SectionNumber;
    unsigned long long sectionGroupMask = 0xffffffffff00ff00;
    unsigned long long sectionGroupKey = sectionKey & sectionGroupMask;
    vector<unsigned long long>::const_iterator sectionIt = find(m_seenSections.begin(),
                                                                m_seenSections.end(),
                                                                sectionKey);
    if (sectionIt != m_seenSections.end())
    {
      // Yes. We might be ready!
      //LogDebug(L"AET: previously seen section, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, section number = %d",
      //          pid, tableId, subtype, mgtTag, section.SectionNumber);
      if (m_isReadyAet || m_unseenSections.size() != 0)
      {
        return;
      }

      // SCTE 65 annex D specifies minimum and maximum transmission rates for
      // AEIT and AETT. It's a bit tricky to know how those rates would convert
      // to repetition rates. Assume...
      if (CTimeUtils::ElapsedMillis(m_completeTime) >= 120000)
      {
        if (
          m_recordsAeit.RemoveExpiredRecords(NULL) != 0 ||
          m_recordsAett.RemoveExpiredRecords(NULL) != 0
        )
        {
          m_currentRecordAeit = NULL;
          m_currentRecordAett = NULL;
          m_currentRecordIndex = 0xffffffff;
        }

        LogDebug(L"AET: ready, sections parsed = %llu, AEIT record count = %lu, AETT record count = %lu",
                  (unsigned long long)m_seenSections.size(),
                  m_recordsAeit.GetRecordCount(),
                  m_recordsAett.GetRecordCount());
        m_isReadyAet = true;
        // Only notify that we're ready when both AEIT/AETT and STT are ready.
        // We need STT in order to convert time-stamps from GPS to
        // epoch/Unix/POSIX.
        if (m_isReadyStt && m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableComplete(PID_AET_CALL_BACK, TABLE_ID_AET_CALL_BACK);
        }
      }
      return;
    }

    // Were we expecting this section?
    sectionIt = find(m_unseenSections.begin(), m_unseenSections.end(), sectionKey);
    if (sectionIt == m_unseenSections.end())
    {
      // No. Is this a change/update, or just a new section group?
      bool isChange = m_isReadyAet;
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
        LogDebug(L"AET: received, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, last section number = %d",
                  pid, tableId, subtype, mgtTag, section.VersionNumber,
                  section.SectionNumber, section.LastSectionNumber);
        if (m_callBackGrabber != NULL && m_seenSections.size() == 0)
        {
          m_callBackGrabber->OnTableSeen(PID_AET_CALL_BACK, TABLE_ID_AET_CALL_BACK);
        }
      }
      else
      {
        LogDebug(L"AET: changed, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, last section number = %d",
                  pid, tableId, subtype, mgtTag, section.VersionNumber,
                  section.SectionNumber, section.LastSectionNumber);

        if (tableId == TABLE_ID_AEIT)
        {
          m_recordsAeit.MarkExpiredRecords(mgtTag);
        }
        else
        {
          m_recordsAett.MarkExpiredRecords(mgtTag);
        }

        if (m_isReadyAet)
        {
          m_isReadyAet = false;
          // Only notify that we're changing when both AEIT/AETT and STT were
          // ready.
          if (m_isReadyStt && m_callBackGrabber != NULL)
          {
            m_callBackGrabber->OnTableChange(PID_AET_CALL_BACK, TABLE_ID_AET_CALL_BACK);
          }
        }
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
      //LogDebug(L"AET: new section, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d",
      //          pid, tableId, subtype, mgtTag, section.VersionNumber,
      //          section.SectionNumber);
    }

    unsigned short pointer = 9;                               // points to the first byte in the source/block loop
    unsigned short endOfSection = section.SectionLength - 1;  // points to the first byte in the CRC
    for (unsigned char i = 0; i < numRecordsInSection && pointer + ((numRecordsInSection - i) * minimumRecordByteCount) - 1 < endOfSection; i++)
    {
      if (tableId == TABLE_ID_AETT)
      {
        CRecordAett* record = new CRecordAett();
        if (record == NULL)
        {
          LogDebug(L"AET: failed to allocate record, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, num. blocks in section = %hhu, block index = %hhu, source ID = %hu, event ID = %hu",
                    pid, tableId, subtype, mgtTag, section.VersionNumber,
                    section.SectionNumber, numRecordsInSection, i,
                    record->SourceId, record->EventId);
          return;
        }

        record->MgtTag = mgtTag;
        if (!DecodeAettRecord(data, pointer, endOfSection, *record))
        {
          LogDebug(L"AET: invalid section, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, num. blocks in section = %hhu, block index = %hhu, source ID = %hu, event ID = %hu",
                    pid, tableId, subtype, mgtTag, section.VersionNumber,
                    section.SectionNumber, numRecordsInSection, i,
                    record->SourceId, record->EventId);
          delete record;
          return;
        }

        m_recordsAett.AddOrUpdateRecord((IRecord**)&record, NULL);
      }
      else
      {
        unsigned short sourceId = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        unsigned char numEvents = data[pointer++];
        //LogDebug(L"AET: source ID = %hu, num. events = %hhu",
        //          sourceId, numEvents);

        if (pointer + ((numRecordsInSection - 1 - i) * minimumRecordByteCount) + (numEvents * MINIMUM_RECORD_BYTE_COUNT_AEIT_EVENT) > endOfSection)
        {
          LogDebug(L"AET: invalid section, num. events = %hhu, pointer = %hu, end of section = %hu, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, num. sources in section = %hhu, source index = %hhu, source ID = %hu",
                    numEvents, pointer, endOfSection, pid, tableId, subtype,
                    mgtTag, section.VersionNumber, section.SectionNumber,
                    numRecordsInSection, i, sourceId);
          return;
        }

        for (unsigned char j = 0; j < numEvents && pointer + ((numRecordsInSection - 1 - i) * MINIMUM_RECORD_BYTE_COUNT_AEIT_EVENT) + (numEvents * MINIMUM_RECORD_BYTE_COUNT_AEIT_EVENT) - 1 < endOfSection; i++)
        {
          CRecordAeit* record = new CRecordAeit();
          if (record == NULL)
          {
            LogDebug(L"AET: failed to allocate record, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, num. sources in section = %hhu, source index = %hhu, source ID = %hu, num. events = %hhu, event index = %hhu",
                      pid, tableId, subtype, mgtTag, section.VersionNumber,
                      section.SectionNumber, numRecordsInSection, i, sourceId,
                      numEvents, j);
            return;
          }

          record->MgtTag = mgtTag;
          record->SourceId = sourceId;
          if (!DecodeAeitRecord(data, pointer, endOfSection, *record))
          {
            LogDebug(L"AET: invalid section, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, num. sources in section = %hhu, source index = %hhu, source ID = %hu, num. events = %hhu, event index = %hhu, event ID = %hu",
                      pid, tableId, subtype, mgtTag, section.VersionNumber,
                      section.SectionNumber, numRecordsInSection, i, sourceId,
                      numEvents, j, record->EventId);
            delete record;
            return;
          }

          m_recordsAeit.AddOrUpdateRecord((IRecord**)&record, NULL);
        }
      }
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"AET: section parsing error, pointer = %hu, end of section = %hu, PID = %hu, table ID = 0x%hhx, sub-type = %hhu, MGT tag = %hhu, version number = %hhu, section number = %d, num. records in section = %hhu",
                pointer, endOfSection, pid, tableId, subtype, mgtTag,
                section.VersionNumber, section.SectionNumber,
                numRecordsInSection);
      return;
    }

    m_seenSections.push_back(sectionKey);
    m_unseenSections.erase(sectionIt);
    if (m_unseenSections.size() == 0)
    {
      // SCTE 65 section 5.5.1 specifies AEITX and AETTX shall be delivered via
      // a common PID. Further, certain values of X (eg. AEIT0, AEIT1, AETT0
      // and AETT1) shall be delivered via the same PID. Therefore we don't
      // assume that we've seen all sections yet. Sections carrying a different
      // AEIT or AETT may not have been received.
      m_completeTime = clock();
    }
  }
  catch (...)
  {
    LogDebug(L"AET: unhandled exception in OnNewSection()");
  }
}

void CParserAet::PrivateReset(bool removeDecoders)
{
  CEnterCriticalSection lock(m_section);

  if (removeDecoders)
  {
    map<unsigned short, CSectionDecoder*>::iterator it = m_decoders.begin();
    for ( ; it != m_decoders.end(); it++)
    {
      if (it->second != NULL)
      {
        delete it->second;
        it->second = NULL;
      }
    }
    m_decoders.clear();
  }
  else
  {
    map<unsigned short, CSectionDecoder*>::const_iterator it = m_decoders.begin();
    for ( ; it != m_decoders.end(); it++)
    {
      CSectionDecoder* decoder = it->second;
      if (decoder != NULL)
      {
        decoder->EnableCrcCheck(m_enableCrcCheck);
        decoder->Reset();
      }
    }
  }

  m_recordsAeit.RemoveAllRecords();
  m_recordsAett.RemoveAllRecords();
  m_seenSections.clear();
  m_unseenSections.clear();
  m_isReadyAet = false;

  m_isReadyStt = false;
  m_gpsUtcOffset = 0;

  m_currentRecordAeit = NULL;
  m_currentRecordAett = NULL;
  m_currentRecordIndex = 0xffffffff;
}

bool CParserAet::DecodeAeitRecord(const unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  CRecordAeit& record)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT_AEIT_EVENT > endOfSection)
  {
    LogDebug(L"AEIT: invalid AEIT record, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.EventId = ((sectionData[pointer] & 0x3f) << 8) | sectionData[pointer + 1];
    pointer += 2;

    record.StartDateTime = (sectionData[pointer] << 24) | (sectionData[pointer + 1] << 16) | (sectionData[pointer + 2] << 8) | sectionData[pointer + 3];
    pointer += 4;

    record.EtmPresent = (sectionData[pointer] & 0x30) >> 4;
    record.Duration = ((sectionData[pointer] & 0xf) << 16) | (sectionData[pointer + 1] << 8) | sectionData[pointer + 2];
    pointer += 3;

    unsigned char titleLength = sectionData[pointer++];
    //LogDebug(L"AEIT: event ID = %hu, start date/time = %lu, ETM present = %hhu, length in seconds = %lu, title length = %hhu",
    //          record.Id, record.StartDateTime, record.EtmPresent,
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
        LogDebug(L"AEIT: invalid AEIT record, title length = %hhu, pointer = %hu, end of section = %hu",
                  titleLength, pointer, endOfSection);
        return false;
      }

      if (record.Titles.size() == 0)
      {
        LogDebug(L"AEIT: failed to allocate an event's title, title length = %hhu, pointer = %hu, source ID = %hu, event ID = %hu",
                  titleLength, pointer, record.SourceId, record.EventId);
      }

      pointer += titleLength;
    }

    unsigned short descriptorsLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;
    //LogDebug(L"AEIT: descriptors length = %hu", descriptorsLength);

    unsigned short endOfDescriptors = pointer + descriptorsLength;
    if (endOfDescriptors > endOfSection)
    {
      LogDebug(L"AEIT: invalid AEIT record, descriptors length = %hu, pointer = %hu, end of section = %hu",
                descriptorsLength, pointer, endOfSection);
      return false;
    }

    while (pointer + 1 < endOfDescriptors)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      //LogDebug(L"AEIT: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (pointer + length > endOfDescriptors)
      {
        LogDebug(L"AEIT: invalid AEIT record, descriptor length = %hhu, pointer = %hu, end of descriptors = %hu, tag = 0x%hhx, end of section = %hu",
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
        LogDebug(L"AEIT: invalid AEIT record descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptors = %hu",
                  tag, length, pointer, endOfDescriptors);
        return false;
      }

      pointer += length;
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"AEIT: unhandled exception in DecodeAeitRecord()");
  }
  return false;
}

bool CParserAet::DecodeAettRecord(const unsigned char* sectionData,
                                  unsigned short& pointer,
                                  unsigned short endOfSection,
                                  CRecordAett& record)
{
  if (pointer + MINIMUM_RECORD_BYTE_COUNT_AETT > endOfSection)
  {
    LogDebug(L"AETT: invalid AETT record, pointer = %hu, end of section = %hu",
              pointer, endOfSection);
    return false;
  }
  try
  {
    record.SourceId = (sectionData[pointer] << 8) | sectionData[pointer + 1];
    pointer += 2;
    record.EventId = (sectionData[pointer] << 6) | (sectionData[pointer + 1] >> 2);
    pointer += 2;
    unsigned short extendedTextLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;

    //LogDebug(L"AETT: source ID = %hu, event ID = %hu, extended text length = %hu",
    //          record.SourceId, record.EventId, extendedTextLength);
    if (extendedTextLength > 0)
    {
      if (
        pointer + extendedTextLength > endOfSection ||
        !CTextUtil::AtscScteMultipleStringStructureToStrings(&sectionData[pointer],
                                                              extendedTextLength,
                                                              record.Texts)
      )
      {
        LogDebug(L"AETT: invalid AETT record, extended text length = %hu, pointer = %hu, end of section = %hu",
                  extendedTextLength, pointer, endOfSection);
        return false;
      }

      if (record.Texts.size() == 0)
      {
        LogDebug(L"AETT: failed to allocate an event's extended text, extended text length = %hu, pointer = %hu, source ID = %hu, event ID = %hu",
                  extendedTextLength, pointer, record.SourceId,
                  record.EventId);
      }

      pointer += extendedTextLength;
    }

    return true;
  }
  catch (...)
  {
    LogDebug(L"AETT: unhandled exception in DecodeAettRecord()");
  }
  return false;
}

bool CParserAet::DecodeAc3AudioStreamDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                vector<unsigned long>& audioLanguages)
{
  if (dataLength < 3)
  {
    LogDebug(L"AEIT: invalid AC3 audio stream descriptor, length = %hhu",
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
    //LogDebug(L"AEIT: AC3 audio stream descriptor, length = %hhu, sample rate code = %hhu, bsid = %hhu, bit rate code = %hhu, surround mode = %hhu, bsmod. = %hhu, num. channels = %hhu, full svc. = %d",
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
      LogDebug(L"AEIT: invalid AC3 audio stream descriptor, length = %hhu, num. channels = %hhu, text length = %hhu",
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
        LogDebug(L"AEIT: invalid AC3 audio stream descriptor, length = %hhu, num. channels = %hhu, text length = %hhu, language flag = %d, language flag 2 = %d",
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
    LogDebug(L"AEIT: unhandled exception in DecodeAc3AudioStreamDescriptor()");
  }
  return false;
}

bool CParserAet::DecodeCaptionServiceDescriptor(const unsigned char* data,
                                                unsigned char dataLength,
                                                vector<unsigned long>& captionsLanguages)
{
  if (dataLength == 0)
  {
    LogDebug(L"AEIT: invalid caption service descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char numberOfServices = data[0] & 0x1f;
    //LogDebug(L"AEIT: caption service descriptor, number of services = %hhu",
    //          numberOfServices);
    if (1 + (numberOfServices * 6) > dataLength)
    {
      LogDebug(L"AEIT: invalid caption service descriptor, length = %hhu, number of services = %hhu",
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
    LogDebug(L"AEIT: unhandled exception in DecodeCaptionServiceDescriptor()");
  }
  return false;
}

bool CParserAet::DecodeContentAdvisoryDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned char& vchipRating,
                                                  unsigned char& mpaaClassification,
                                                  unsigned short& advisories)
{
  if (dataLength == 0)
  {
    LogDebug(L"AEIT: invalid content advisory descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char ratingRegionCount = data[0] & 0x3f;
    //LogDebug(L"AEIT: content advisory descriptor, rating region count = %hhu",
    //          ratingRegionCount);
    if (1 + (ratingRegionCount * 3) > dataLength)
    {
      LogDebug(L"AEIT: invalid content advisory descriptor, length = %hhu, rating region count = %hhu",
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
        LogDebug(L"AEIT: invalid content advisory descriptor, descriptor length = %hhu, pointer = %hu, rating region count = %hhu, region index = %hhu, region = %hhu, rated dimensions = %hhu",
                  dataLength, pointer, ratingRegionCount, i, ratingRegion,
                  ratedDimensions);
        return false;
      }

      for (unsigned char j = 0; j < ratedDimensions && pointer + ((ratingRegionCount - 1 - i) * 3) + ((ratedDimensions - j) * 2) + 1 - 1 < dataLength; j++)
      {
        unsigned char ratingDimension = data[pointer++];
        unsigned char ratingValue = data[pointer++] & 0xf;
        //LogDebug(L"    dimension = %hhu, value = %hhu", ratingDimension, ratingValue);

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
        LogDebug(L"AEIT: invalid content advisory descriptor, descriptor length = %hhu, pointer = %hu, rating description length = %hhu, rating region count = %hhu, region index = %hhu, region = %hhu, rated dimensions = %hhu",
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
    LogDebug(L"AEIT: unhandled exception in DecodeContentAdvisoryDescriptor()");
  }
  return false;
}

bool CParserAet::DecodeGenreDescriptor(const unsigned char* data,
                                        unsigned char dataLength,
                                        vector<unsigned char>& genreIds)
{
  if (dataLength == 0)
  {
    LogDebug(L"AEIT: invalid genre descriptor, length = %hhu", dataLength);
    return false;
  }
  try
  {
    unsigned char attributeCount = data[0] & 0x1f;
    //LogDebug(L"AEIT: genre descriptor, attribute count = %hhu",
    //          attributeCount);
    if (dataLength != 1 + attributeCount)
    {
      LogDebug(L"AEIT: invalid genre descriptor, length = %hhu, attribute count = %hhu",
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
    LogDebug(L"AEIT: unhandled exception in DecodeGenreDescriptor()");
  }
  return false;
}

bool CParserAet::DecodeEnhancedAc3AudioDescriptor(const unsigned char* data,
                                                  unsigned char dataLength,
                                                  vector<unsigned long>& audioLanguages)
{
  if (dataLength < 2)
  {
    LogDebug(L"AEIT: invalid E-AC3 audio descriptor, length = %hhu",
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
    //LogDebug(L"AEIT: E-AC3 audio descriptor, length = %hhu, bsid flag = %d, main ID flag = %d, ASVC flag = %d, mix info exists = %d, sub-stream 1 flag = %d, sub-stream 2 flag = %d, sub-stream 3 flag = %d, full service flag = %d, service type = %hhu, number of channels = %hhu",
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
        LogDebug(L"AEIT: invalid E-AC3 audio descriptor, length = %hhu, main ID flag = %d, ASVC flag = %d, sub-stream 1 flag = %d, sub-stream 2 flag = %d, sub-stream 3 flag = %d, language flag = %d, language flag 2 = %d",
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
    LogDebug(L"AEIT: unhandled exception in DecodeEnhancedAc3AudioDescriptor()");
  }
  return false;
}