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
#include "ParserOpenTv.h"
#include <algorithm>
#include <cstring>      // strcmp()
#include <time.h>       // gmtime(), mktime(), time_t, tm
#include "..\..\shared\TimeUtils.h"
#include "EnterCriticalSection.h"
#include "PidUsage.h"
#include "TextUtil.h"
#include "Utils.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserOpenTv::CParserOpenTv(ICallBackPidConsumer* callBack, LPUNKNOWN unk, HRESULT* hr)
  : CUnknown(NAME("OpenTV EPG Grabber"), unk), m_recordsEvent(600000),
    m_recordsDescription(600000)
{
  if (callBack == NULL)
  {
    LogDebug(L"OpenTV: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_isGrabbing = false;
  m_isItalianText = false;
  m_useAlternativeProgramCategoryHandling = false;
  m_isDescriptionPhase = false;
  m_isReady = false;
  m_completeTime = 0;

  m_callBackGrabber = NULL;
  m_callBackPidConsumer = callBack;
  m_pidPmt = 0;
  m_enableCrcCheck = true;
}

CParserOpenTv::~CParserOpenTv(void)
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

STDMETHODIMP CParserOpenTv::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  if (ppv == NULL)
  {
    return E_INVALIDARG;
  }

  if (iid == IID_IGRABBER)
  {
    return GetInterface((IGrabber*)this, ppv);
  }
  if (iid == IID_IGRABBER_EPG_OPENTV)
  {
    return GetInterface((IGrabberEpgOpenTv*)this, ppv);
  }
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

void CParserOpenTv::SetOriginalNetworkId(unsigned short originalNetworkId)
{
  LogDebug(L"OpenTV: set original network ID, ONID = %hu", originalNetworkId);
  bool isItalianText = originalNetworkId == 64511;  // Sky Italia
  bool useAltProgCatHandling = false;
  if (
    originalNetworkId == 47 ||    // Freeview Satellite NZ (TVNZ)
    originalNetworkId == 105 ||   // Foxtel AU (Optus B3)
    originalNetworkId == 168 ||   // Foxtel AU
    originalNetworkId == 169 ||   // Sky NZ
    originalNetworkId == 4096 ||  // Foxtel AU (Optus B3)
    (originalNetworkId >= 4112 && originalNetworkId <= 4127)  // AU broadcasters (ABC, SBS etc.)
  )
  {
    useAltProgCatHandling = true;
  }

  CEnterCriticalSection lock(m_section);
  if (
    isItalianText == m_isItalianText &&
    useAltProgCatHandling == m_useAlternativeProgramCategoryHandling
  )
  {
    return;
  }
  m_isItalianText = isItalianText;
  m_useAlternativeProgramCategoryHandling = useAltProgCatHandling;

  if (m_recordsEvent.GetRecordCount() > 0 || m_recordsDescription.GetRecordCount() > 0)
  {
    m_recordsEvent.RemoveAllRecords();
    m_recordsDescription.RemoveAllRecords();
    m_seenSections.clear();
    m_unseenSections.clear();
    m_isReady = false;
  }
  SwitchToPhase(false);
}

void CParserOpenTv::SetPmtPid(unsigned short pid)
{
  LogDebug(L"OpenTV: set PMT PID, PID = %hu", pid);
  CEnterCriticalSection lock(m_section);
  if (pid == m_pidPmt)
  {
    return;
  }

  if (m_isGrabbing && m_callBackPidConsumer != NULL)
  {
    vector<unsigned short> pids;
    vector<unsigned short>::const_iterator it;
    vector<unsigned short>::const_iterator itEnd;
    if (m_isDescriptionPhase)
    {
      it = m_pidsDescription.begin();
      itEnd = m_pidsDescription.end();
    }
    else
    {
      it = m_pidsEvent.begin();
      itEnd = m_pidsEvent.end();
    }

    if (m_pidPmt != 0)
    {
      pids.push_back(m_pidPmt);
    }
    for ( ; it != itEnd; it++)
    {
      pids.push_back(*it);
    }

    if (pids.size() > 0)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&pids[0], pids.size(), Epg);
    }
    if (pid != 0)
    {
      m_callBackPidConsumer->OnPidsRequired(&pid, 1, Epg);
    }
  }

  bool isGrabbing = m_isGrabbing;
  bool isItalianText = m_isItalianText;
  bool useAlternativeProgramCategoryHandling = m_useAlternativeProgramCategoryHandling;
  Reset(m_enableCrcCheck);
  m_isGrabbing = isGrabbing;
  m_isItalianText = isItalianText;
  m_useAlternativeProgramCategoryHandling = useAlternativeProgramCategoryHandling;

  m_pidPmt = pid;
}

void CParserOpenTv::AddEventDecoders(const vector<unsigned short>& pids)
{
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> newPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    if (m_decoders.find(pid) == m_decoders.end() && AddOrResetDecoder(pid, m_enableCrcCheck))
    {
      newPids.push_back(pid);
      m_pidsEvent.push_back(pid);
    }
  }
  if (newPids.size() > 0)
  {
    LogDebug(L"OpenTV: add event decoders...");
    CUtils::DebugVector(newPids, L"PIDs", false);
    if (m_isGrabbing)
    {
      if (m_isDescriptionPhase)
      {
        SwitchToPhase(false);
      }
      else
      {
        m_callBackPidConsumer->OnPidsRequired(&newPids[0], newPids.size(), Epg);
      }
    }
  }
}

void CParserOpenTv::RemoveEventDecoders(const vector<unsigned short>& pids)
{
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> oldPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CSectionDecoder*>::iterator decoderIt = m_decoders.find(pid);
    if (decoderIt != m_decoders.end())
    {
      if (decoderIt->second != NULL)
      {
        delete decoderIt->second;
        decoderIt->second = NULL;
        oldPids.push_back(pid);

        vector<unsigned short>::iterator it = find(m_pidsEvent.begin(), m_pidsEvent.end(), pid);
        if (it != m_pidsEvent.end())
        {
          m_pidsEvent.erase(it);
        }
      }
      m_decoders.erase(decoderIt);
    }
  }

  if (oldPids.size() > 0)
  {
    LogDebug(L"OpenTV: remove event decoders...");
    CUtils::DebugVector(oldPids, L"PIDs", false);

    m_recordsEvent.RemoveAllRecords();
    CleanUpSections(m_pidsDescription);

    if (m_isGrabbing && !m_isDescriptionPhase && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&oldPids[0], oldPids.size(), Epg);
    }
    else
    {
      SwitchToPhase(false);
    }
  }
}

void CParserOpenTv::AddDescriptionDecoders(const vector<unsigned short>& pids)
{
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> newPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    if (m_decoders.find(pid) == m_decoders.end() && AddOrResetDecoder(pid, m_enableCrcCheck))
    {
      newPids.push_back(pid);
      m_pidsDescription.push_back(pid);
    }
  }
  if (newPids.size() > 0)
  {
    LogDebug(L"OpenTV: add description decoders...");
    CUtils::DebugVector(newPids, L"PIDs", false);
    if (m_isGrabbing && m_isDescriptionPhase && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsRequired(&newPids[0], newPids.size(), Epg);
    }
  }
}

void CParserOpenTv::RemoveDescriptionDecoders(const vector<unsigned short>& pids)
{
  CEnterCriticalSection lock(m_section);
  vector<unsigned short> oldPids;
  vector<unsigned short>::const_iterator pidIt = pids.begin();
  for ( ; pidIt != pids.end(); pidIt++)
  {
    unsigned short pid = *pidIt;
    map<unsigned short, CSectionDecoder*>::iterator decoderIt = m_decoders.find(pid);
    if (decoderIt != m_decoders.end())
    {
      if (decoderIt->second != NULL)
      {
        delete decoderIt->second;
        decoderIt->second = NULL;
        oldPids.push_back(pid);

        vector<unsigned short>::iterator it = find(m_pidsDescription.begin(),
                                                    m_pidsDescription.end(),
                                                    pid);
        if (it != m_pidsDescription.end())
        {
          m_pidsDescription.erase(it);
        }
      }
      m_decoders.erase(decoderIt);
    }
  }

  if (oldPids.size() > 0)
  {
    LogDebug(L"OpenTV: remove description decoders...");
    CUtils::DebugVector(oldPids, L"PIDs", false);
    m_recordsDescription.RemoveAllRecords();
    CleanUpSections(m_pidsEvent);
    if (m_isGrabbing && m_isDescriptionPhase && m_callBackPidConsumer != NULL)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&oldPids[0], oldPids.size(), Epg);
    }
  }
}

STDMETHODIMP_(void) CParserOpenTv::Start()
{
  LogDebug(L"OpenTV: start");
  CEnterCriticalSection lock(m_section);
  if (!m_isGrabbing)
  {
    m_isGrabbing = true;
    m_isDescriptionPhase = false;

    if (m_callBackPidConsumer != NULL)
    {
      vector<unsigned short> pids;
      if (m_pidPmt != 0)
      {
        pids.push_back(m_pidPmt);
      }
      vector<unsigned short>::const_iterator it = m_pidsEvent.begin();
      for ( ; it != m_pidsEvent.end(); it++)
      {
        pids.push_back(*it);
      }
      if (pids.size() > 0)
      {
        m_callBackPidConsumer->OnPidsRequired(&pids[0], pids.size(), Epg);
      }
    }
  }
}

STDMETHODIMP_(void) CParserOpenTv::Stop()
{
  LogDebug(L"OpenTV: stop");
  CEnterCriticalSection lock(m_section);
  if (m_isGrabbing)
  {
    m_isGrabbing = false;
    m_isReady = false;

    map<unsigned short, CSectionDecoder*>::iterator decoderIt = m_decoders.begin();
    for ( ; decoderIt != m_decoders.end(); decoderIt++)
    {
      if (decoderIt->second != NULL)
      {
        decoderIt->second->Reset();
      }
    }

    if (m_callBackPidConsumer != NULL)
    {
      vector<unsigned short> pids;
      vector<unsigned short>::const_iterator it;
      vector<unsigned short>::const_iterator itEnd;
      if (m_isDescriptionPhase)
      {
        it = m_pidsDescription.begin();
        itEnd = m_pidsDescription.end();
      }
      else
      {
        it = m_pidsEvent.begin();
        itEnd = m_pidsEvent.end();
      }

      if (m_pidPmt != 0)
      {
        pids.push_back(m_pidPmt);
      }
      for ( ; it != itEnd; it++)
      {
        pids.push_back(*it);
      }

      if (pids.size() > 0)
      {
        m_callBackPidConsumer->OnPidsNotRequired(&pids[0], pids.size(), Epg);
      }
    }
  }
}

void CParserOpenTv::Reset(bool enableCrcCheck)
{
  LogDebug(L"OpenTV: reset");
  CEnterCriticalSection lock(m_section);

  map<unsigned short, CSectionDecoder*>::iterator decoderIt = m_decoders.begin();
  for ( ; decoderIt != m_decoders.end(); decoderIt++)
  {
    if (decoderIt->second != NULL)
    {
      delete decoderIt->second;
      decoderIt->second = NULL;
    }
  }
  m_decoders.clear();

  m_recordsEvent.RemoveAllRecords();
  m_recordsDescription.RemoveAllRecords();

  m_seenSections.clear();
  m_unseenSections.clear();
  m_pidPmt = 0;
  m_pidsEvent.clear();
  m_pidsDescription.clear();
  m_isGrabbing = false;
  m_isItalianText = false;
  m_useAlternativeProgramCategoryHandling = false;
  m_isDescriptionPhase = false;
  m_isReady = false;
  m_enableCrcCheck = enableCrcCheck;
  LogDebug(L"OpenTV: reset done");
}

STDMETHODIMP_(void) CParserOpenTv::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
}

bool CParserOpenTv::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
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

STDMETHODIMP_(bool) CParserOpenTv::IsSeen()
{
  CEnterCriticalSection lock(m_section);
  return m_seenSections.size() != 0;
}

STDMETHODIMP_(bool) CParserOpenTv::IsReady()
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

STDMETHODIMP_(unsigned long) CParserOpenTv::GetEventCount()
{
  CEnterCriticalSection lock(m_section);
  return m_recordsEvent.GetRecordCount();
}

STDMETHODIMP_(bool) CParserOpenTv::GetEvent(unsigned long index,
                                            unsigned short* channelId,
                                            unsigned short* eventId,
                                            unsigned long long* startDateTime,
                                            unsigned short* duration,
                                            char* title,
                                            unsigned short* titleBufferSize,
                                            char* shortDescription,
                                            unsigned short* shortDescriptionBufferSize,
                                            char* extendedDescription,
                                            unsigned short* extendedDescriptionBufferSize,
                                            unsigned char* categoryId,
                                            unsigned char* subCategoryId,
                                            bool* isHighDefinition,
                                            bool* hasSubtitles,
                                            unsigned char* parentalRating,
                                            unsigned short* seriesLinkId)
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_recordsEvent.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"OpenTV: invalid event index, index = %lu, record count = %lu",
              index, m_recordsEvent.GetRecordCount());
    return false;
  }

  CRecordOpenTvEvent* recordEvent = dynamic_cast<CRecordOpenTvEvent*>(record);
  if (recordEvent == NULL)
  {
    LogDebug(L"OpenTV: invalid event record, index = %lu", index);
    return false;
  }

  *channelId = recordEvent->ChannelId;
  *eventId = recordEvent->EventId;
  *startDateTime = recordEvent->StartDateTime;
  *duration = recordEvent->Duration;
  *categoryId = recordEvent->CategoryId;
  *subCategoryId = recordEvent->SubCategoryId;
  *isHighDefinition = recordEvent->IsHighDefinition;
  *hasSubtitles = recordEvent->HasSubtitles;
  *parentalRating = recordEvent->ParentalRating;

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordEvent->Title,
                                  title,
                                  *titleBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"OpenTV: insufficient title buffer size, event index = %lu, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, required size = %hu, actual size = %hu",
              index, recordEvent->Pid, recordEvent->TableId, *channelId,
              *eventId, requiredBufferSize, *titleBufferSize);
  }

  if (m_recordsDescription.GetRecordByKey(recordEvent->GetKey(), &record) && record != NULL)
  {
    CRecordOpenTvEventDescription* recordDescription = dynamic_cast<CRecordOpenTvEventDescription*>(record);
    if (recordDescription != NULL)
    {
      *seriesLinkId = recordDescription->SeriesLinkId;
      if (!CUtils::CopyStringToBuffer(recordDescription->ShortDescription,
                                      shortDescription,
                                      *shortDescriptionBufferSize,
                                      requiredBufferSize))
      {
        LogDebug(L"OpenTV: insufficient short description buffer size, event index = %lu, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, required size = %hu, actual size = %hu",
                  index, recordEvent->Pid, recordEvent->TableId, *channelId,
                  *eventId, requiredBufferSize, *shortDescriptionBufferSize);
      }
      if (!CUtils::CopyStringToBuffer(recordDescription->ExtendedDescription,
                                      extendedDescription,
                                      *extendedDescriptionBufferSize,
                                      requiredBufferSize))
      {
        LogDebug(L"OpenTV: insufficient short description buffer size, event index = %lu, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, required size = %hu, actual size = %hu",
                  index, recordEvent->Pid, recordEvent->TableId, *channelId,
                  *eventId, requiredBufferSize,
                  *extendedDescriptionBufferSize);
      }
      return true;
    }

    LogDebug(L"OpenTV: invalid description record, event index = %lu, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu",
              index, recordEvent->Pid, recordEvent->TableId, *channelId,
              *eventId);
  }

  *seriesLinkId = 0xffff;
  if (shortDescription != NULL && *shortDescriptionBufferSize != 0)
  {
    shortDescription[0] = NULL;
  }
  *shortDescriptionBufferSize = 0;
  if (extendedDescription != NULL && *extendedDescriptionBufferSize != 0)
  {
    extendedDescription[0] = NULL;
  }
  *extendedDescriptionBufferSize = 0;
  return true;
}

void CParserOpenTv::OnNewSection(int pid, int tableId, CSection& section)
{
  try
  {
    if (!section.SectionSyntaxIndicator || !section.CurrentNextIndicator)
    {
      return;
    }

    bool isEventPid = false;
    CEnterCriticalSection lock(m_section);
    vector<unsigned short>::const_iterator pidIt = find(m_pidsEvent.begin(),
                                                        m_pidsEvent.end(),
                                                        pid);
    if (pidIt != m_pidsEvent.end())
    {
      if (tableId < TABLE_ID_OPENTV_EVENT_START || tableId > TABLE_ID_OPENTV_EVENT_END)
      {
        return;
      }
      isEventPid = true;
    }
    else
    {
      pidIt = find(m_pidsDescription.begin(), m_pidsDescription.end(), pid);
      if (
        pidIt == m_pidsDescription.end() ||
        tableId < TABLE_ID_OPENTV_DESC_START ||
        tableId > TABLE_ID_OPENTV_DESC_END
      )
      {
        return;
      }
    }

    if (section.section_length > 4093 || section.section_length < 11)
    {
      LogDebug(L"OpenTV: invalid section, length = %d, table ID = 0x%x, PID = %d",
                section.section_length, section.table_id, pid);
      return;
    }

    unsigned char* data = section.Data;
    unsigned short channelId = section.table_id_extension;

    // Decode the MJD-encoded start date.
    unsigned short startDateMjd = (data[8] << 8) | data[9];
    long j = startDateMjd + 2400001 + 68569;
    long c = 4 * j / 146097;
    j = j - (146097 * c + 3) / 4;
    long y = 4000 * (j + 1) / 1461001;
    j = j - 1461 * y / 4 + 31;
    long m = 80 * j / 2447;

    tm startDateTime;
    startDateTime.tm_sec = 0;
    startDateTime.tm_min = 0;
    startDateTime.tm_hour = 0;
    startDateTime.tm_mday = j - 2447 * m / 80;
    j = m / 11;
    startDateTime.tm_mon = m + 2 - 12 * j - 1;              // 0..11
    startDateTime.tm_year = 100 * (c - 49) + y + j - 1900;  // year - 1900
    startDateTime.tm_isdst = -1;

    // As far as I know the date/time is a local time. mktime() will give us
    // the corresponding UTC epoch, as required.
    unsigned long long baseStartDateTime = mktime(&startDateTime);

    //LogDebug(L"OpenTV: PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section length = %d, section number = %d, last section number = %d, base start date/time = %llu (%hu)",
    //          pid, tableId, channelId, section.version_number,
    //          section.section_length, section.SectionNumber,
    //          section.LastSectionNumber, baseStartDateTime, startDateMjd);

    // Have we seen this service before?
    unsigned long long sectionKey = ((unsigned long long)pid << 40) | ((unsigned long long)tableId << 32) | (channelId << 16) | (section.version_number << 8) | section.SectionNumber;
    unsigned long long sectionGroupMask = 0xffffffffffff0000;
    unsigned long long sectionGroupKey = sectionKey & sectionGroupMask;
    vector<unsigned long long>::const_iterator sectionIt = find(m_seenSections.begin(),
                                                                m_seenSections.end(),
                                                                sectionKey);
    if (sectionIt != m_seenSections.end())
    {
      // Yes. We might be ready!
      //LogDebug(L"OpenTV: previously seen section, PID = %D, table ID = 0x%x, channel ID = %hu, section number = %d",
      //          pid, tableId, channelId, section.SectionNumber);
      if (m_isReady || m_unseenSections.size() > 0)
      {
        return;
      }

      // Assume repetition every 60 seconds.
      if (CTimeUtils::ElapsedMillis(m_completeTime) >= 30000)
      {
        if (!m_isDescriptionPhase)
        {
          SwitchToPhase(true);
          if (m_isDescriptionPhase)
          {
            return;
          }
        }

        m_recordsEvent.RemoveExpiredRecords(NULL);
        m_recordsDescription.RemoveExpiredRecords(NULL);

        LogDebug(L"OpenTV: ready, sections parsed = %llu, event count = %lu, description count = %lu",
                  (unsigned long long)m_seenSections.size(),
                  m_recordsEvent.GetRecordCount(),
                  m_recordsDescription.GetRecordCount());
        m_isReady = true;

        if (m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableComplete(PID_OPENTV_CALL_BACK, TABLE_ID_OPENTV_CALL_BACK);
        }
        SwitchToPhase(false);   // monitor event changes (start time, title more important)
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

      if (isChange)
      {
        LogDebug(L"OpenTV: changed, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d, last section number = %d",
                  pid, tableId, channelId, section.version_number,
                  section.SectionNumber, section.LastSectionNumber);
        unsigned long expiryKey = ((unsigned long long)pid << 24) | (tableId << 16) | channelId;
        if (isEventPid)
        {
          m_recordsEvent.MarkExpiredRecords(expiryKey);
        }
        else
        {
          m_recordsDescription.MarkExpiredRecords(expiryKey);
        }
        if (m_isReady && m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableChange(PID_OPENTV_CALL_BACK, TABLE_ID_OPENTV_CALL_BACK);
        }
        m_isReady = false;
      }
      else
      {
        LogDebug(L"OpenTV: received, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d, last section number = %d",
                  pid, tableId, channelId, section.version_number,
                  section.SectionNumber, section.LastSectionNumber);
        if (
          m_callBackGrabber != NULL &&
          m_recordsEvent.GetRecordCount() == 0 &&
          m_recordsDescription.GetRecordCount() == 0
        )
        {
          m_callBackGrabber->OnTableSeen(PID_OPENTV_CALL_BACK, TABLE_ID_OPENTV_CALL_BACK);
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
      //LogDebug(L"OpenTV: new section, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d",
      //          pid, tableId, channelId, section.version_number,
      //          section.SectionNumber);
    }

    unsigned short pointer = 10;                              // points to the first byte in the event loop
    unsigned short endOfSection = section.section_length - 1; // points to the first byte in the CRC
    while (pointer + 3 < endOfSection)
    {
      if (isEventPid)
      {
        CRecordOpenTvEvent* recordEvent = new CRecordOpenTvEvent();
        if (recordEvent == NULL)
        {
          LogDebug(L"OpenTV: failed to allocate event record, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d",
                    pid, tableId, channelId, section.version_number,
                    section.SectionNumber);
          return;
        }

        recordEvent->Pid = pid;
        recordEvent->TableId = tableId;
        recordEvent->ChannelId = channelId;
        recordEvent->StartDateTime = baseStartDateTime;
        if (!DecodeEventRecord(data,
                                pointer,
                                endOfSection,
                                m_isItalianText,
                                m_useAlternativeProgramCategoryHandling,
                                *recordEvent))
        {
          LogDebug(L"OpenTV: invalid section, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d, event ID = %hu",
                    pid, tableId, channelId, section.version_number,
                    section.SectionNumber, recordEvent->EventId);
          delete recordEvent;
          return;
        }

        m_recordsEvent.AddOrUpdateRecord((IRecord**)&recordEvent, NULL);
      }
      else
      {
        CRecordOpenTvEventDescription* recordEventDescription = new CRecordOpenTvEventDescription();
        if (recordEventDescription == NULL)
        {
          LogDebug(L"OpenTV: failed to allocate event description record, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d",
                    pid, tableId, channelId, section.version_number,
                    section.SectionNumber);
          return;
        }

        recordEventDescription->Pid = pid;
        recordEventDescription->TableId = tableId;
        recordEventDescription->ChannelId = channelId;
        if (!DecodeEventDescriptionRecord(data,
                                          pointer,
                                          endOfSection,
                                          m_isItalianText,
                                          *recordEventDescription))
        {
          LogDebug(L"OpenTV: invalid section, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d, event ID = %hu",
                    pid, tableId, channelId, section.version_number,
                    section.SectionNumber, recordEventDescription->EventId);
          delete recordEventDescription;
          return;
        }

        m_recordsDescription.AddOrUpdateRecord((IRecord**)&recordEventDescription, NULL);
      }
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"OpenTV: section parsing error, pointer = %hu, end of section = %hu, PID = %d, table ID = 0x%x, channel ID = %hu, version number = %d, section number = %d",
                pointer, endOfSection, pid, tableId, channelId,
                section.version_number, section.SectionNumber);
      return;
    }

    m_seenSections.push_back(sectionKey);
    m_unseenSections.erase(sectionIt);
    if (m_unseenSections.size() == 0)
    {
      // We can't assume that we've seen all sections yet, because sections for
      // another channel, day and/or time block may not have been received.
      m_completeTime = clock();
    }
  }
  catch (...)
  {
    LogDebug(L"OpenTV: unhandled exception in OnNewSection()");
  }
}

bool CParserOpenTv::AddOrResetDecoder(unsigned short pid, bool enableCrcCheck)
{
  CSectionDecoder* decoder = NULL;
  map<unsigned short, CSectionDecoder*>::const_iterator it = m_decoders.find(pid);
  if (it == m_decoders.end() || it->second == NULL)
  {
    decoder = new CSectionDecoder();
    if (decoder == NULL)
    {
      LogDebug(L"OpenTV: failed to allocate section decoder for PID %hu", pid);
      return false;
    }
    decoder->SetPid(pid);
    decoder->SetCallBack(this);
    m_decoders[pid] = decoder;
  }
  else
  {
    decoder = it->second;
  }

  decoder->Reset();
  decoder->EnableCrcCheck(enableCrcCheck);
  return true;
}

void CParserOpenTv::CleanUpSections(vector<unsigned short>& keepPids)
{
  vector<unsigned long long>::iterator sectionIt = m_unseenSections.begin();
  while (sectionIt != m_unseenSections.end())
  {
    unsigned short sectionPid = (unsigned short)((*sectionIt) >> 40);
    vector<unsigned short>::iterator pidIt = find(keepPids.begin(), keepPids.end(), sectionPid);
    if (pidIt == m_pidsEvent.end())
    {
      sectionIt = m_unseenSections.erase(sectionIt);
      continue;
    }
    sectionIt++;
  }
  sectionIt = m_seenSections.begin();
  while (sectionIt != m_seenSections.end())
  {
    unsigned short sectionPid = (unsigned short)((*sectionIt) >> 40);
    vector<unsigned short>::iterator pidIt = find(keepPids.begin(), keepPids.end(), sectionPid);
    if (pidIt == m_pidsEvent.end())
    {
      sectionIt = m_seenSections.erase(sectionIt);
      continue;
    }
    sectionIt++;
  }
}

void CParserOpenTv::SwitchToPhase(bool descriptionPhase)
{
  if (m_isGrabbing)
  {
    if (descriptionPhase && !m_isDescriptionPhase && m_pidsDescription.size() > 0)
    {
      LogDebug(L"OpenTV: switch to description phase");
      m_isDescriptionPhase = true;
      m_completeTime = clock();
      if (m_callBackPidConsumer != NULL)
      {
        if (m_pidsEvent.size() > 0)
        {
          m_callBackPidConsumer->OnPidsNotRequired(&m_pidsEvent[0], m_pidsEvent.size(), Epg);
        }
        m_callBackPidConsumer->OnPidsRequired(&m_pidsDescription[0],
                                              m_pidsDescription.size(),
                                              Epg);
      }
    }
    else if (!descriptionPhase && m_isDescriptionPhase)
    {
      LogDebug(L"OpenTV: switch back to event phase");
      m_isDescriptionPhase = false;
      m_completeTime = clock();
      if (m_callBackPidConsumer != NULL)
      {
        if (m_pidsDescription.size() > 0)
        {
          m_callBackPidConsumer->OnPidsNotRequired(&m_pidsDescription[0],
                                                    m_pidsDescription.size(),
                                                    Epg);
        }
        if (m_pidsEvent.size() > 0)
        {
          m_callBackPidConsumer->OnPidsRequired(&m_pidsEvent[0], m_pidsEvent.size(), Epg);
        }
      }
    }
  }
}

bool CParserOpenTv::DecodeEventRecord(unsigned char* sectionData,
                                      unsigned short& pointer,
                                      unsigned short endOfSection,
                                      bool isItalianText,
                                      bool useAlternativeProgramCategoryHandling,
                                      CRecordOpenTvEvent& record)
{
  try
  {
    record.EventId = (sectionData[pointer] << 8) | sectionData[pointer + 1];
    pointer += 2;
    unsigned short descriptorsLoopLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;
    //LogDebug(L"OpenTV: event, ID = %hu, descriptors loop length = %hu",
    //          record.Id, descriptorsLoopLength);

    unsigned short endOfDescriptorLoop = pointer + descriptorsLoopLength;
    if (endOfDescriptorLoop > endOfSection)
    {
      LogDebug(L"OpenTV: invalid event record, descriptors loop length = %hu, pointer = %hu, end of section = %hu",
                descriptorsLoopLength, pointer, endOfSection);
      return false;
    }

    bool descriptorParseResult = true;
    while (pointer + 1 < endOfDescriptorLoop)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"OpenTV: event descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (endOfDescriptor > endOfDescriptorLoop)
      {
        LogDebug(L"OpenTV: invalid event record, descriptor length = %hhu, pointer = %hu, end of descriptor loop = %hu, tag = 0x%hhx",
                  length, pointer, endOfDescriptorLoop, tag);
        return false;
      }

      if (tag == 0xb5)  // OpenTV event descriptor
      {
        char* title = NULL;
        unsigned long startDateTimeOffset;
        descriptorParseResult = DecodeOpenTvEventDescriptor(&sectionData[pointer],
                                                            length,
                                                            isItalianText,
                                                            useAlternativeProgramCategoryHandling,
                                                            startDateTimeOffset,
                                                            record.Duration,
                                                            record.CategoryId,
                                                            record.SubCategoryId,
                                                            record.IsHighDefinition,
                                                            record.HasSubtitles,
                                                            record.ParentalRating,
                                                            &title);
        record.StartDateTime += startDateTimeOffset;
        if (descriptorParseResult && title != NULL)
        {
          if (record.Title != NULL)
          {
            if (strcmp(record.Title, title) != 0)
            {
              LogDebug(L"OpenTV: replacing title, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, current = %S, new = %S",
                        record.Pid, record.TableId, record.ChannelId,
                        record.EventId, record.Title, title);
            }
            delete[] record.Title;
          }
          record.Title = title;
        }
      }

      if (!descriptorParseResult)
      {
        LogDebug(L"OpenTV: invalid event record descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptor loop = %hu",
                  tag, length, pointer, endOfDescriptorLoop);
        return false;
      }

      pointer = endOfDescriptor;
    }

    pointer = endOfDescriptorLoop;
    return true;
  }
  catch (...)
  {
    LogDebug(L"OpenTV: unhandled exception in DecodeEventRecord()");
  }
  return false;
}

bool CParserOpenTv::DecodeEventDescriptionRecord(unsigned char* sectionData,
                                                  unsigned short& pointer,
                                                  unsigned short endOfSection,
                                                  bool isItalianText,
                                                  CRecordOpenTvEventDescription& record)
{
  try
  {
    record.EventId = (sectionData[pointer] << 8) | sectionData[pointer + 1];
    pointer += 2;
    unsigned short descriptorsLoopLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;
    //LogDebug(L"OpenTV: description, event ID = %hu, descriptors loop length = %hu",
    //          record.Id, descriptorsLoopLength);

    unsigned short endOfDescriptorLoop = pointer + descriptorsLoopLength;
    if (endOfDescriptorLoop > endOfSection)
    {
      LogDebug(L"OpenTV: invalid event description record, descriptors loop length = %hu, pointer = %hu, end of section = %hu",
                descriptorsLoopLength, pointer, endOfSection);
      return false;
    }

    bool descriptorParseResult = true;
    while (pointer + 1 < endOfDescriptorLoop)
    {
      unsigned char tag = sectionData[pointer++];
      unsigned char length = sectionData[pointer++];
      unsigned short endOfDescriptor = pointer + length;
      //LogDebug(L"OpenTV: event description descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
      //          tag, length, pointer);
      if (endOfDescriptor > endOfDescriptorLoop)
      {
        LogDebug(L"OpenTV: invalid event description record, descriptor length = %hhu, pointer = %hu, end of descriptor loop = %hu, tag = 0x%hhx",
                  length, pointer, endOfDescriptorLoop, tag);
        return false;
      }

      if (tag == 0xb9 || tag == 0xbb) // OpenTV short/extended event description descriptor
      {
        char* description = NULL;
        descriptorParseResult = DecodeOpenTvEventDescriptionDescriptor(&sectionData[pointer],
                                                                        length,
                                                                        isItalianText,
                                                                        &description);
        if (descriptorParseResult && description != NULL)
        {
          if (tag == 0xb9)
          {
            if (record.ShortDescription != NULL)
            {
              if (strcmp(record.ShortDescription, description) != 0)
              {
                LogDebug(L"OpenTV: replacing short description, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, current = %S, new = %S",
                          record.Pid, record.TableId, record.ChannelId,
                          record.EventId, record.ShortDescription,
                          description);
              }
              delete[] record.ShortDescription;
            }
            record.ShortDescription = description;
          }
          else
          {
            if (record.ExtendedDescription != NULL)
            {
              if (strcmp(record.ExtendedDescription, description) != 0)
              {
                LogDebug(L"OpenTV: replacing extended description, PID = %hu, table ID = 0x%hhx, channel ID = %hu, event ID = %hu, current = %S, new = %S",
                          record.Pid, record.TableId, record.ChannelId,
                          record.EventId, record.ExtendedDescription,
                          description);
              }
              delete[] record.ExtendedDescription;
            }
            record.ExtendedDescription = description;
          }
        }
      }
      else if (tag == 0xc1) // OpenTV series link descriptor
      {
        descriptorParseResult = DecodeOpenTvSeriesLinkDescriptor(&sectionData[pointer],
                                                                  length,
                                                                  record.SeriesLinkId);
      }

      if (!descriptorParseResult)
      {
        LogDebug(L"OpenTV: invalid event description record descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptor loop = %hu",
                  tag, length, pointer, endOfDescriptorLoop);
        return false;
      }

      pointer = endOfDescriptor;
    }

    pointer = endOfDescriptorLoop;
    return true;
  }
  catch (...)
  {
    LogDebug(L"OpenTV: unhandled exception in DecodeEventDescriptionRecord()");
  }
  return false;
}

bool CParserOpenTv::DecodeOpenTvEventDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                bool isItalianText,
                                                bool useAlternativeProgramCategoryHandling,
                                                unsigned long& startDateTimeOffset,
                                                unsigned short& duration,
                                                unsigned char& categoryId,
                                                unsigned char& subCategoryId,
                                                bool& isHighDefinition,
                                                bool& hasSubtitles,
                                                unsigned char& parentalRating,
                                                char** title)
{
  if (dataLength < 8)
  {
    LogDebug(L"OpenTV: invalid OpenTV event descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    startDateTimeOffset = (data[0] << 9) | (data[1] << 1);            // unit = seconds
    unsigned long durationSeconds = (data[2] << 9) | (data[3] << 1);  // unit = seconds
    duration = (unsigned short)(durationSeconds / 60);                // unit = minutes
    categoryId = data[4] >> 5;                        // interpretation is country-dependent
    subCategoryId = data[4] & 0x1f;                   // interpretation is country-dependent

    unsigned char audioMode = data[5] >> 6;           // 1 = stereo, 2 = surround, 3 = Dolby Digital
    bool unknown1 = (data[5] & 0x20) != 0;
    hasSubtitles = (data[5] & 0x10) != 0;
    bool isWideScreen = (data[5] & 0x8) != 0;
    isHighDefinition = (data[5] & 0x4) != 0;
    bool isCopyProtected = (data[5] & 0x2) != 0;
    bool hasAudioDescription = (data[5] & 0x1) != 0;
    bool isPayPerView = (data[6] & 0x80) != 0;
    bool isSeriesContinuing = (data[6] & 0x40) != 0;
    unsigned char unknown2 = (data[6] & 0x30) >> 4;   // expected to be zero (reserved)
    parentalRating = data[6] & 0xf;                   // interpretation is country-dependent

    if (!CTextUtil::OpenTvTextToString(&data[7], dataLength - 7, isItalianText, title))
    {
      LogDebug(L"OpenTV: invalid OpenTV event descriptor, length = %hhu, is Italian text = %d",
                dataLength, isItalianText);
      return false;
    }
    if (*title == NULL)
    {
      LogDebug(L"OpenTV: failed to allocate OpenTV event title");
    }

    if (useAlternativeProgramCategoryHandling && hasAudioDescription)
    {
      categoryId |= 0x8;
      hasAudioDescription = false;
    }

    //LogDebug(L"OpenTV: OpenTV event descriptor, start date/time offset = %lu s, duration = %lu s, category ID = %hhu, sub-category ID = %hhu, audio mode = %hhu, unknown 1 = %d, has subtitles = %d, is wide screen = %d, is HD = %d, is copy protected = %d, has audio description = %d, is PPV = %d, is series continuing = %d, unknown 2 = %hhu, parental rating = %hhu, title = %S",
    //          startDateTimeOffset, durationSeconds, categoryId, subCategoryId,
    //          audioMode, unknown1, hasSubtitles, isWideScreen,
    //          isHighDefinition, isCopyProtected, hasAudioDescription,
    //          isPayPerView, isSeriesContinuing, unknown2, parentalRating,
    //          *title == NULL ? "" : *title);
    return true;
  }
  catch (...)
  {
    LogDebug(L"OpenTV: unhandled exception in DecodeOpenTvEventDescriptor()");
  }
  return false;
}

bool CParserOpenTv::DecodeOpenTvEventDescriptionDescriptor(unsigned char* data,
                                                            unsigned char dataLength,
                                                            bool isItalianText,
                                                            char** description)
{
  if (dataLength == 0)
  {
    LogDebug(L"OpenTV: invalid OpenTV short/extended event description descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    if (!CTextUtil::OpenTvTextToString(data, dataLength, isItalianText, description))
    {
      LogDebug(L"OpenTV: invalid OpenTV short/extended event description descriptor, length = %hhu, is Italian text = %d",
                dataLength, isItalianText);
      return false;
    }
    if (*description == NULL)
    {
      LogDebug(L"OpenTV: failed to allocate OpenTV event description");
    }

    //LogDebug(L"OpenTV: OpenTV short/extended event description descriptor, text = %S",
    //          *description == NULL ? "" : *description);
    return true;
  }
  catch (...)
  {
    LogDebug(L"OpenTV: unhandled exception in DecodeOpenTvEventDescriptionDescriptor()");
  }
  return false;
}

bool CParserOpenTv::DecodeOpenTvSeriesLinkDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned short& seriesLinkId)
{
  if (dataLength == 0)
  {
    return true;
  }
  if (dataLength != 2)
  {
    LogDebug(L"OpenTV: invalid OpenTV series link descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    seriesLinkId = (data[0] << 8) | data[1];

    //LogDebug(L"OpenTV: OpenTV series link descriptor, series link ID = %hu",
    //          seriesLinkId);
    return true;
  }
  catch (...)
  {
    LogDebug(L"OpenTV %hu: unhandled exception in DecodeOpenTvSeriesLinkDescriptor()");
  }
  return false;
}