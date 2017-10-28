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
#include "GrabberEpgAtsc.h"
#include "..\..\shared\EnterCriticalSection.h"
#include "GrabberSiAtscScte.h"
#include "ParserSttAtsc.h"
#include "PidUsage.h"
#include "Utils.h"


extern void LogDebug(const wchar_t* fmt, ...);

CGrabberEpgAtsc::CGrabberEpgAtsc(ICallBackPidConsumer* callBack,
                                  ISectionDispatcher* sectionDispatcher,
                                  ISystemTimeInfoProviderAtscScte* systemTimeInfoProvider,
                                  LPUNKNOWN unk,
                                  HRESULT* hr)
  : CUnknown(NAME("ATSC EPG Grabber"), unk)
{
  if (callBack == NULL)
  {
    LogDebug(L"EPG ATSC: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }
  if (systemTimeInfoProvider == NULL)
  {
    LogDebug(L"EPG ATSC: system time information provider not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_isGrabbing = false;
  m_isSeen = false;
  m_isReady = false;

  m_callBackGrabber = NULL;
  m_callBackPidConsumer = callBack;
  m_sectionDispatcher = sectionDispatcher;
  m_systemTimeInfoProvider = systemTimeInfoProvider;
  m_gpsUtcOffset = 0;
  m_enableCrcCheck = true;

  m_currentEventParser = NULL;
  m_currentEventIndex = 0xffffffff;
  m_currentEventIndexOffset = 0;
  m_currentEventId = 0;
  m_currentEventSourceId = 0;
}

CGrabberEpgAtsc::~CGrabberEpgAtsc()
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = NULL;
  m_callBackPidConsumer = NULL;

  map<unsigned short, CParserEitAtsc*>::iterator parserEitIt = m_parsersEit.begin();
  for ( ; parserEitIt != m_parsersEit.end(); parserEitIt++)
  {
    if (parserEitIt->second != NULL)
    {
      delete parserEitIt->second;
      parserEitIt->second = NULL;
    }
  }
  m_parsersEit.clear();

  map<unsigned short, CParserEtt*>::iterator parserEttIt = m_parsersEtt.begin();
  for ( ; parserEttIt != m_parsersEtt.end(); parserEttIt++)
  {
    if (parserEttIt->second != NULL)
    {
      delete parserEttIt->second;
      parserEttIt->second = NULL;
    }
  }
  m_parsersEtt.clear();
}

STDMETHODIMP CGrabberEpgAtsc::NonDelegatingQueryInterface(REFIID iid, void** ppv)
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
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

void CGrabberEpgAtsc::AddEitDecoders(const vector<unsigned short>& pids)
{
  LogDebug(L"EPG ATSC: add EIT decoders...");
  CUtils::DebugVector(pids, L"PIDs", false);
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> newPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CParserEitAtsc*>::const_iterator parserEitIt = m_parsersEit.find(pid);
    if (parserEitIt != m_parsersEit.end())
    {
      LogDebug(L"EPG ATSC: EIT decoder already exists, PID %hu", pid);
      continue;
    }
    map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.find(pid);
    if (parserEttIt != m_parsersEtt.end())
    {
      LogDebug(L"EPG ATSC: expecting ETT rather than EIT, PID %hu", pid);
      continue;
    }

    CParserEitAtsc* parser = new CParserEitAtsc(pid, m_sectionDispatcher);
    if (parser == NULL)
    {
      LogDebug(L"EPG ATSC: failed to allocate EIT decoder, PID %hu", pid);
      continue;
    }
    parser->SetCallBack(this);
    parser->Reset(m_enableCrcCheck);
    m_parsersEit[pid] = parser;
    newPids.push_back(pid);
  }
  if (newPids.size() > 0)
  {
    m_currentEventParser = NULL;
    m_currentEventIndex = 0xffffffff;
    m_currentEventIndexOffset = 0;
    m_currentEventId = 0;
    m_currentEventSourceId = 0;

    if (m_isGrabbing && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsRequired(&newPids[0], newPids.size(), Epg);
    }
  }
}

void CGrabberEpgAtsc::RemoveEitDecoders(const vector<unsigned short>& pids)
{
  LogDebug(L"EPG ATSC: remove EIT decoders...");
  CUtils::DebugVector(pids, L"PIDs", false);
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> oldPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CParserEitAtsc*>::iterator parserIt = m_parsersEit.find(pid);
    if (parserIt == m_parsersEit.end())
    {
      LogDebug(L"EPG ATSC: EIT decoder does not exist, PID = %hu", pid);
      continue;
    }
    if (parserIt->second != NULL)
    {
      delete parserIt->second;
      parserIt->second = NULL;
      oldPids.push_back(pid);
    }
    m_parsersEit.erase(parserIt);
  }
  if (oldPids.size() > 0)
  {
    m_currentEventParser = NULL;
    m_currentEventIndex = 0xffffffff;
    m_currentEventIndexOffset = 0;
    m_currentEventId = 0;
    m_currentEventSourceId = 0;

    if (m_isGrabbing && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&oldPids[0], oldPids.size(), Epg);
    }
  }
}

void CGrabberEpgAtsc::AddEttDecoders(const vector<unsigned short>& pids)
{
  LogDebug(L"EPG ATSC: add ETT decoders...");
  CUtils::DebugVector(pids, L"PIDs", false);
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> newPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CParserEitAtsc*>::const_iterator parserEitIt = m_parsersEit.find(pid);
    if (parserEitIt != m_parsersEit.end())
    {
      LogDebug(L"EPG ATSC: ETT decoder already exists, PID %hu", pid);
      continue;
    }
    map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.find(pid);
    if (parserEttIt != m_parsersEtt.end())
    {
      LogDebug(L"EPG ATSC: expecting EIT rather than ETT, PID %hu", pid);
      continue;
    }

    CParserEtt* parser = new CParserEtt(pid, m_sectionDispatcher);
    if (parser == NULL)
    {
      LogDebug(L"EPG ATSC: failed to allocate ETT decoder, PID %hu", pid);
      continue;
    }
    parser->SetCallBack(this);
    parser->Reset(m_enableCrcCheck);
    m_parsersEtt[pid] = parser;
    newPids.push_back(pid);
  }
  if (newPids.size() > 0)
  {
    if (m_isGrabbing && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsRequired(&newPids[0], newPids.size(), Epg);
    }
  }
}

void CGrabberEpgAtsc::RemoveEttDecoders(const vector<unsigned short>& pids)
{
  LogDebug(L"EPG ATSC: remove ETT decoders...");
  CUtils::DebugVector(pids, L"PIDs", false);
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> oldPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CParserEtt*>::iterator parserIt = m_parsersEtt.find(pid);
    if (parserIt == m_parsersEtt.end())
    {
      LogDebug(L"EPG ATSC: ETT decoder does not exist, PID = %hu", pid);
      continue;
    }
    if (parserIt->second != NULL)
    {
      delete parserIt->second;
      parserIt->second = NULL;
      oldPids.push_back(pid);
    }
    m_parsersEtt.erase(parserIt);
  }
  if (oldPids.size() > 0)
  {
    if (m_isGrabbing && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&oldPids[0], oldPids.size(), Epg);
    }
  }
}

void CGrabberEpgAtsc::OnTableSeen(unsigned char tableId)
{
  if (tableId == TABLE_ID_STT_ATSC)
  {
    // Seeing the STT doesn't mean we've seen EPG.
    return;
  }
  CEnterCriticalSection lock(m_section);
  if (!m_isSeen && m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableSeen(PID_EIT_ATSC_CALL_BACK, TABLE_ID_EIT_ATSC_CALL_BACK);
  }
  m_isSeen = true;
  m_isReady = false;
}

void CGrabberEpgAtsc::OnTableComplete(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (!m_isReady)
  {
    if (m_parsersEit.size() == 0 && m_parsersEtt.size() == 0)
    {
      return;
    }

    map<unsigned short, CParserEitAtsc*>::const_iterator parserEitIt = m_parsersEit.begin();
    for ( ; parserEitIt != m_parsersEit.end(); parserEitIt++)
    {
      if (parserEitIt->second != NULL && !parserEitIt->second->IsReady())
      {
        return;
      }
    }

    map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.begin();
    for ( ; parserEttIt != m_parsersEtt.end(); parserEttIt++)
    {
      if (parserEttIt->second != NULL && !parserEttIt->second->IsReady())
      {
        return;
      }
    }

    // System time information must be available too.
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

    m_gpsUtcOffset = gpsUtcOffset;
    m_isReady = true;
    if (m_callBackGrabber != NULL)
    {
      m_callBackGrabber->OnTableComplete(PID_EIT_ATSC_CALL_BACK, TABLE_ID_EIT_ATSC_CALL_BACK);
    }
  }
}

void CGrabberEpgAtsc::OnTableChange(unsigned char tableId)
{
  // Careful! Seeing the STT change doesn't mean we've seen EPG.
  CEnterCriticalSection lock(m_section);
  if (m_isReady)
  {
    if (m_callBackGrabber != NULL)
    {
      m_callBackGrabber->OnTableChange(PID_EIT_ATSC_CALL_BACK, TABLE_ID_EIT_ATSC_CALL_BACK);
    }
    m_isSeen = true;
    m_isReady = false;
  }
}

STDMETHODIMP_(void) CGrabberEpgAtsc::Start()
{
  LogDebug(L"EPG ATSC: start");
  CEnterCriticalSection lock(m_section);
  if (!m_isGrabbing)
  {
    m_isGrabbing = true;

    if (m_callBackPidConsumer != NULL)
    {
      vector<unsigned short> pids;
      pids.push_back(PID_ATSC_BASE);    // For the MGT, which gives us the EIT and ETT PIDs.
      pids.push_back(PID_SCTE_BASE);    // ...in case SCTE transport streams carry in band data for unencrypted channels.
      map<unsigned short, CParserEitAtsc*>::const_iterator parserEitIt = m_parsersEit.begin();
      for ( ; parserEitIt != m_parsersEit.end(); parserEitIt++)
      {
        pids.push_back(parserEitIt->first);
      }
      map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.begin();
      for ( ; parserEttIt != m_parsersEtt.end(); parserEttIt++)
      {
        pids.push_back(parserEttIt->first);
      }
      m_callBackPidConsumer->OnPidsRequired(&pids[0], pids.size(), Epg);
    }
  }
}

STDMETHODIMP_(void) CGrabberEpgAtsc::Stop()
{
  LogDebug(L"EPG ATSC: stop");
  CEnterCriticalSection lock(m_section);
  if (m_isGrabbing)
  {
    m_isGrabbing = false;
    m_isSeen = false;
    m_isReady = false;

    vector<unsigned short> pids;
    pids.push_back(PID_ATSC_BASE);
    pids.push_back(PID_SCTE_BASE);
    map<unsigned short, CParserEitAtsc*>::const_iterator parserEitIt = m_parsersEit.begin();
    for ( ; parserEitIt != m_parsersEit.end(); parserEitIt++)
    {
      if (parserEitIt->second != NULL)
      {
        parserEitIt->second->Reset(m_enableCrcCheck);
      }
      pids.push_back(parserEitIt->first);
    }
    map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.begin();
    for ( ; parserEttIt != m_parsersEtt.end(); parserEttIt++)
    {
      if (parserEttIt->second != NULL)
      {
        parserEttIt->second->Reset(m_enableCrcCheck);
      }
      pids.push_back(parserEttIt->first);
    }

    if (m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&pids[0], pids.size(), Epg);
    }
  }
}

void CGrabberEpgAtsc::Reset(bool enableCrcCheck)
{
  LogDebug(L"EPG ATSC: reset");
  CEnterCriticalSection lock(m_section);

  map<unsigned short, CParserEitAtsc*>::iterator parserEitIt = m_parsersEit.begin();
  for ( ; parserEitIt != m_parsersEit.end(); parserEitIt++)
  {
    if (parserEitIt->second != NULL)
    {
      delete parserEitIt->second;
      parserEitIt->second = NULL;
    }
  }
  m_parsersEit.clear();

  map<unsigned short, CParserEtt*>::iterator parserEttIt = m_parsersEtt.begin();
  for ( ; parserEttIt != m_parsersEtt.end(); parserEttIt++)
  {
    if (parserEttIt->second != NULL)
    {
      delete parserEttIt->second;
      parserEttIt->second = NULL;
    }
  }
  m_parsersEtt.clear();

  m_isSeen = false;
  m_isReady = false;
  m_gpsUtcOffset = 0;
  m_enableCrcCheck = enableCrcCheck;
  m_currentEventParser = NULL;
  m_currentEventIndex = 0xffffffff;
  m_currentEventIndexOffset = 0;
  m_currentEventId = 0;
  m_currentEventSourceId = 0;

  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnReset(PID_EIT_ATSC_CALL_BACK);
  }
  LogDebug(L"EPG ATSC: reset done");
}

STDMETHODIMP_(void) CGrabberEpgAtsc::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
}

bool CGrabberEpgAtsc::OnTsPacket(const CTsHeader& header, const unsigned char* tsPacket)
{
  CEnterCriticalSection lock(m_section);
  if (m_isGrabbing)
  {
    map<unsigned short, CParserEitAtsc*>::const_iterator parserEitIt = m_parsersEit.find(header.Pid);
    if (parserEitIt != m_parsersEit.end())
    {
      if (parserEitIt->second != NULL)
      {
        parserEitIt->second->OnTsPacket(header, tsPacket);
      }
      return true;
    }

    map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.find(header.Pid);
    if (parserEttIt != m_parsersEtt.end())
    {
      if (parserEttIt->second != NULL)
      {
        parserEttIt->second->OnTsPacket(header, tsPacket);
      }
      return true;
    }
  }
  return false;
}

STDMETHODIMP_(bool) CGrabberEpgAtsc::IsSeen()
{
  CEnterCriticalSection lock(m_section);
  return m_isSeen;
}

STDMETHODIMP_(bool) CGrabberEpgAtsc::IsReady()
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

STDMETHODIMP_(unsigned long) CGrabberEpgAtsc::GetEventCount()
{
  CEnterCriticalSection lock(m_section);
  unsigned long eventCount = 0;
  map<unsigned short, CParserEitAtsc*>::const_iterator it = m_parsersEit.begin();
  for ( ; it != m_parsersEit.end(); it++)
  {
    if (it->second != NULL)
    {
      eventCount += it->second->GetEventCount();
    }
  }
  return eventCount;
}

STDMETHODIMP_(bool) CGrabberEpgAtsc::GetEvent(unsigned long index,
                                              unsigned short* sourceId,
                                              unsigned short* eventId,
                                              unsigned long long* startDateTime,
                                              unsigned long* duration,
                                              unsigned char* textCount,
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
  if (!SelectEventByIndex(index))
  {
    return false;
  }

  unsigned long startDateTimeGps;
  bool result = m_currentEventParser->GetEvent(index - m_currentEventIndexOffset,
                                                *sourceId,
                                                *eventId,
                                                startDateTimeGps,
                                                *duration,
                                                *textCount,
                                                audioLanguages,
                                                *audioLanguageCount,
                                                captionsLanguages,
                                                *captionsLanguageCount,
                                                genreIds,
                                                *genreIdCount,
                                                *vchipRating,
                                                *mpaaClassification,
                                                *advisories);
  if (result)
  {
    // Start date/time is a GPS time. To convert to epoch/Unix/POSIX time we
    // must subtract the number of leap seconds between 1980 and now (because
    // GPS time includes leap seconds but epoch/Unix/POSIX time doesn't), and
    // add the number of seconds between the start of epoch/Unix/POSIX time and
    // GPS time.
    *startDateTime = startDateTimeGps + GPS_TIME_START_OFFSET - m_gpsUtcOffset;
  }
  return result;
}

STDMETHODIMP_(bool) CGrabberEpgAtsc::GetEventTextByIndex(unsigned long eventIndex,
                                                          unsigned char titleIndex,
                                                          unsigned long* language,
                                                          char* title,
                                                          unsigned short* titleBufferSize,
                                                          char* text,
                                                          unsigned short* textBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventByIndex(eventIndex))
  {
    return false;
  }

  if (!m_currentEventParser->GetEventTitleByIndex(eventIndex - m_currentEventIndexOffset,
                                                  titleIndex,
                                                  *language,
                                                  title,
                                                  *titleBufferSize))
  {
    return false;
  }

  map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.begin();
  for ( ; parserEttIt != m_parsersEtt.end(); parserEttIt++)
  {
    CParserEtt* parser = parserEttIt->second;
    if (parser != NULL && parser->GetEventTextCount(m_currentEventSourceId, m_currentEventId) != 0)
    {
      if (parser->GetEventTextByLanguage(m_currentEventSourceId,
                                          m_currentEventId,
                                          *language,
                                          text,
                                          *textBufferSize))
      {
        return true;
      }
    }
  }

  LogDebug(L"EPG ATSC: missing event text, source ID = %hu, event ID = %hu, language = %S",
            m_currentEventSourceId, m_currentEventId, (char*)language);
  unsigned short requiredBufferSize;
  CUtils::CopyStringToBuffer(NULL, text, *textBufferSize, requiredBufferSize);
  return true;
}

STDMETHODIMP_(bool) CGrabberEpgAtsc::GetEventTextByLanguage(unsigned long eventIndex,
                                                            unsigned long language,
                                                            char* title,
                                                            unsigned short* titleBufferSize,
                                                            char* text,
                                                            unsigned short* textBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventByIndex(eventIndex))
  {
    return false;
  }

  if (!m_currentEventParser->GetEventTitleByLanguage(eventIndex - m_currentEventIndexOffset,
                                                      language,
                                                      title,
                                                      *titleBufferSize))
  {
    return false;
  }

  map<unsigned short, CParserEtt*>::const_iterator parserEttIt = m_parsersEtt.begin();
  for ( ; parserEttIt != m_parsersEtt.end(); parserEttIt++)
  {
    CParserEtt* parser = parserEttIt->second;
    if (parser != NULL && parser->GetEventTextCount(m_currentEventSourceId, m_currentEventId) != 0)
    {
      if (parser->GetEventTextByLanguage(m_currentEventSourceId,
                                          m_currentEventId,
                                          language,
                                          text,
                                          *textBufferSize))
      {
        return true;
      }
    }
  }

  LogDebug(L"EPG ATSC: missing event text, source ID = %hu, event ID = %hu, language = %S",
            m_currentEventSourceId, m_currentEventId, (char*)&language);
  unsigned short requiredBufferSize;
  CUtils::CopyStringToBuffer(NULL, text, *textBufferSize, requiredBufferSize);
  return true;
}

bool CGrabberEpgAtsc::SelectEventByIndex(unsigned long index)
{
  if (m_currentEventParser != NULL && m_currentEventIndex == index)
  {
    return true;
  }

  unsigned long eventCount = 0;
  map<unsigned short, CParserEitAtsc*>::const_iterator parserEitIt = m_parsersEit.begin();
  for ( ; parserEitIt != m_parsersEit.end(); parserEitIt++)
  {
    CParserEitAtsc* parser = parserEitIt->second;
    if (parser != NULL)
    {
      unsigned long indexOffset = eventCount;
      eventCount += parser->GetEventCount();
      if (index < eventCount)
      {
        m_currentEventParser = parser;
        m_currentEventIndex = index;
        m_currentEventIndexOffset = indexOffset;
        m_currentEventSourceId = 0;
        m_currentEventId = 0;
        if (!parser->GetEventIdentifiers(index - indexOffset,
                                          m_currentEventSourceId,
                                          m_currentEventId))
        {
          return false;
        }
        return true;
      }
    }
  }

  LogDebug(L"EPG ATSC: invalid event index, index = %lu, record count = %lu",
            index, eventCount);
  return false;
}