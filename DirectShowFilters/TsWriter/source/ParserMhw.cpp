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
#include "ParserMhw.h"
#include "..\..\shared\TimeUtils.h"
#include "EnterCriticalSection.h"
#include "PidUsage.h"
#include "TextUtil.h"
#include "Utils.h"


extern void LogDebug(const wchar_t* fmt, ...);

CParserMhw::CParserMhw(ICallBackPidConsumer* callBack, LPUNKNOWN unk, HRESULT* hr)
  : CUnknown(NAME("MediaHighway EPG Grabber"), unk), m_recordsChannel(600000),
    m_recordsEvent(600000), m_recordsDescription(600000),
    m_recordsTheme(600000)
{
  if (callBack == NULL)
  {
    LogDebug(L"MHW: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_dayOfWeek = 0xffffffff;
  m_yesterdayDayOfWeek = 0;
  m_yesterdayEpoch = 0;

  m_completeTime = 0;
  m_grabMhw1 = false;
  m_grabMhw2 = false;
  m_isSeen = false;
  m_isReady = false;
  m_lastExpiredRecordCheckTime = 0;

  m_callBackGrabber = NULL;
  m_callBackPidConsumer = callBack;
  m_enableCrcCheck = true;
}

CParserMhw::~CParserMhw()
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

STDMETHODIMP CParserMhw::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  if (ppv == NULL)
  {
    return E_INVALIDARG;
  }

  if (iid == IID_IGRABBER)
  {
    return GetInterface((IGrabber*)this, ppv);
  }
  if (iid == IID_IGRABBER_EPG_MHW)
  {
    return GetInterface((IGrabberEpgMhw*)this, ppv);
  }
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

STDMETHODIMP_(void) CParserMhw::SetProtocols(bool grabMhw1, bool grabMhw2)
{
  LogDebug(L"MHW: set protocols, MHW1 = %d, MHW2 = %d", grabMhw1, grabMhw2);
  CEnterCriticalSection lock(m_section);

  vector<unsigned short> pidsAdd;
  vector<unsigned short> pidsRemove;
  if (grabMhw1 != m_grabMhw1)
  {
    if (grabMhw1)
    {
      pidsAdd.push_back(PID_MHW1_EVENTS);
      pidsAdd.push_back(PID_MHW1_OTHER);
    }
    else
    {
      pidsRemove.push_back(PID_MHW1_EVENTS);
      pidsRemove.push_back(PID_MHW1_OTHER);
    }
    m_grabMhw1 = grabMhw1;
  }
  if (grabMhw2 != m_grabMhw2)
  {
    if (grabMhw2)
    {
      pidsAdd.push_back(PID_MHW2_CHANNELS_AND_THEMES);
      pidsAdd.push_back(PID_MHW2_EVENTS);
      pidsAdd.push_back(PID_MHW2_DESCRIPTIONS);
    }
    else
    {
      pidsRemove.push_back(PID_MHW2_CHANNELS_AND_THEMES);
      pidsRemove.push_back(PID_MHW2_EVENTS);
      pidsRemove.push_back(PID_MHW2_DESCRIPTIONS);
    }
    m_grabMhw2 = grabMhw2;
  }

  if (pidsAdd.size() > 0 || pidsRemove.size() > 0)
  {
    Reset(m_enableCrcCheck);
    if (m_callBackPidConsumer != NULL)
    {
      if (pidsRemove.size() > 0)
      {
        m_callBackPidConsumer->OnPidsNotRequired(&pidsRemove[0], pidsRemove.size(), Epg);
      }
      if (pidsAdd.size() > 0)
      {
        m_callBackPidConsumer->OnPidsRequired(&pidsAdd[0], pidsAdd.size(), Epg);
      }
    }
  }
}

void CParserMhw::Reset(bool enableCrcCheck)
{
  LogDebug(L"MHW: reset");
  CEnterCriticalSection lock(m_section);

  m_enableCrcCheck = enableCrcCheck;
  AddOrResetDecoder(PID_MHW1_EVENTS, enableCrcCheck);
  AddOrResetDecoder(PID_MHW1_OTHER, enableCrcCheck);
  AddOrResetDecoder(PID_MHW2_CHANNELS_AND_THEMES, enableCrcCheck);
  AddOrResetDecoder(PID_MHW2_EVENTS, enableCrcCheck);
  AddOrResetDecoder(PID_MHW2_DESCRIPTIONS, enableCrcCheck);

  m_recordsChannel.RemoveAllRecords();
  m_recordsEvent.RemoveAllRecords();
  m_recordsDescription.RemoveAllRecords();
  m_recordsTheme.RemoveAllRecords();

  m_isSeen = false;
  m_isReady = false;
  m_completeTime = clock();
  LogDebug(L"MHW: reset done");
}

STDMETHODIMP_(void) CParserMhw::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
}

bool CParserMhw::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  if (
    (
      !m_grabMhw1 &&
      (
        header.Pid == PID_MHW1_EVENTS ||
        header.Pid == PID_MHW1_OTHER
      )
    ) ||
    (
      !m_grabMhw2 &&
      (
        header.Pid == PID_MHW2_CHANNELS_AND_THEMES ||
        header.Pid == PID_MHW2_EVENTS ||
        header.Pid == PID_MHW2_DESCRIPTIONS
      )
    )
  )
  {
    return false;
  }

  CEnterCriticalSection lock(m_section);
  map<unsigned short, CSectionDecoder*>::const_iterator it = m_decoders.find(header.Pid);
  if (it != m_decoders.end())
  {
    it->second->OnTsPacket(header, tsPacket);
    return true;
  }
  return false;
}

STDMETHODIMP_(bool) CParserMhw::IsSeen()
{
  CEnterCriticalSection lock(m_section);
  return m_isSeen;
}

STDMETHODIMP_(bool) CParserMhw::IsReady()
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

STDMETHODIMP_(unsigned long) CParserMhw::GetEventCount()
{
  CEnterCriticalSection lock(m_section);
  return m_recordsEvent.GetRecordCount();
}

STDMETHODIMP_(bool) CParserMhw::GetEvent(unsigned long index,
                                          unsigned long long* eventId,
                                          unsigned short* originalNetworkId,
                                          unsigned short* transportStreamId,
                                          unsigned short* serviceId,
                                          char* serviceName,
                                          unsigned short* serviceNameBufferSize,
                                          unsigned long long* startDateTime,
                                          unsigned short* duration,
                                          char* title,
                                          unsigned short* titleBufferSize,
                                          unsigned long* payPerViewId,
                                          char* description,
                                          unsigned short* descriptionBufferSize,
                                          unsigned char* descriptionLineCount,
                                          char* themeName,
                                          unsigned short* themeNameBufferSize,
                                          char* subThemeName,
                                          unsigned short* subThemeNameBufferSize)
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_recordsEvent.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"MHW: invalid event index, index = %lu, record count = %lu",
              index, m_recordsEvent.GetRecordCount());
    return false;
  }

  CRecordMhwEvent* recordEvent = dynamic_cast<CRecordMhwEvent*>(record);
  if (recordEvent == NULL)
  {
    LogDebug(L"MHW: invalid event record, index = %lu", index);
    return false;
  }

  *eventId = recordEvent->GetKey();
  *startDateTime = recordEvent->StartDateTime;
  *duration = recordEvent->Duration;
  *payPerViewId = recordEvent->PayPerViewId;
  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordEvent->Title,
                                  title,
                                  *titleBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"MHW: insufficient event title buffer size, index = %lu, version = %hhu, event ID = %lu, required size = %hu, actual size = %hu",
              index, recordEvent->Version, recordEvent->EventId,
              requiredBufferSize, *titleBufferSize);
  }

  *originalNetworkId = 0;
  *transportStreamId = 0;
  *serviceId = 0;
  if (serviceName != NULL)
  {
    serviceName[0] = NULL;
  }
  if (
    m_recordsChannel.GetRecordByKey((recordEvent->Version << 8) | recordEvent->ChannelId, &record) &&
    record != NULL
  )
  {
    CRecordMhwChannel* recordChannel = dynamic_cast<CRecordMhwChannel*>(record);
    if (recordChannel == NULL)
    {
      LogDebug(L"MHW: invalid channel record, index = %lu, version = %hhu, event ID = %lu, channel ID = %hhu",
                index, recordEvent->Version, recordEvent->EventId,
                recordEvent->ChannelId);
    }
    else
    {
      *originalNetworkId = recordChannel->OriginalNetworkId;
      *transportStreamId = recordChannel->TransportStreamId;
      *serviceId = recordChannel->ServiceId;
      if (!CUtils::CopyStringToBuffer(recordChannel->Name,
                                      serviceName,
                                      *serviceNameBufferSize,
                                      requiredBufferSize))
      {
        LogDebug(L"MHW: insufficient service name buffer size, index = %lu, version = %hhu, event ID = %lu, channel ID = %hhu, required size = %hu, actual size = %hu",
                  index, recordEvent->Version, recordEvent->EventId,
                  recordEvent->ChannelId, requiredBufferSize,
                  *serviceNameBufferSize);
      }
    }
  }
  else
  {
    LogDebug(L"MHW: invalid channel identifiers, index = %lu, version = %hhu, event ID = %lu, channel ID = %hhu",
              index, recordEvent->Version, recordEvent->EventId,
              recordEvent->ChannelId);
  }

  *descriptionLineCount = 0;
  if (description != NULL)
  {
    description[0] = NULL;
  }
  if (m_recordsDescription.GetRecordByKey(*eventId, &record) && record != NULL)
  {
    CRecordMhwDescription* recordDescription = dynamic_cast<CRecordMhwDescription*>(record);
    if (recordDescription == NULL)
    {
      LogDebug(L"MHW: invalid description record, index = %lu, version = %hhu, event ID = %lu",
                index, recordEvent->Version, recordEvent->EventId);
    }
    else
    {
      *descriptionLineCount = (unsigned char)(recordDescription->Lines.size());
      if (!CUtils::CopyStringToBuffer(recordDescription->Description,
                                      description,
                                      *descriptionBufferSize,
                                      requiredBufferSize))
      {
        LogDebug(L"MHW: insufficient description buffer size, index = %lu, version = %hhu, event ID = %lu, required size = %hu, actual size = %hu",
                  index, recordEvent->Version, recordEvent->EventId,
                  requiredBufferSize, *descriptionBufferSize);
      }
    }
  }
  else
  {
    LogDebug(L"MHW: invalid description identifiers, index = %lu, version = %hhu, event ID = %lu",
              index, recordEvent->Version, recordEvent->EventId);
  }

  if (themeName != NULL)
  {
    themeName[0] = NULL;
  }
  if (
    m_recordsTheme.GetRecordByKey((recordEvent->Version << 16) | (recordEvent->ThemeId << 8), &record) &&
    record != NULL
  )
  {
    CRecordMhwTheme* recordTheme = dynamic_cast<CRecordMhwTheme*>(record);
    if (recordTheme == NULL)
    {
      LogDebug(L"MHW: invalid theme record, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu",
                index, recordEvent->Version, recordEvent->EventId,
                recordEvent->ThemeId);
    }
    else if (!CUtils::CopyStringToBuffer(recordTheme->Name,
                                          themeName,
                                          *themeNameBufferSize,
                                          requiredBufferSize))
    {
      LogDebug(L"MHW: insufficient theme name buffer size, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, required size = %hu, actual size = %hu",
                index, recordEvent->Version, recordEvent->EventId,
                recordEvent->ThemeId, requiredBufferSize,
                *themeNameBufferSize);
    }
  }
  else
  {
    LogDebug(L"MHW: invalid theme identifiers, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu",
              index, recordEvent->Version, recordEvent->EventId,
              recordEvent->ThemeId);
  }

  if (subThemeName != NULL)
  {
    subThemeName[0] = NULL;
  }
  if (
    m_recordsTheme.GetRecordByKey((recordEvent->Version << 16) | (recordEvent->ThemeId << 8) | recordEvent->SubThemeId, &record) &&
    record != NULL
  )
  {
    CRecordMhwTheme* recordTheme = dynamic_cast<CRecordMhwTheme*>(record);
    if (recordTheme == NULL)
    {
      LogDebug(L"MHW: invalid sub-theme record, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, sub-theme ID = %hhu",
                index, recordEvent->Version, recordEvent->EventId,
                recordEvent->ThemeId, recordEvent->SubThemeId);
    }
    else if (!CUtils::CopyStringToBuffer(recordTheme->Name,
                                          subThemeName,
                                          *subThemeNameBufferSize,
                                          requiredBufferSize))
    {
      LogDebug(L"MHW: insufficient sub-theme name buffer size, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, sub-theme ID = %hhu, required size = %hu, actual size = %hu",
                index, recordEvent->Version, recordEvent->EventId,
                recordEvent->ThemeId, recordEvent->SubThemeId,
                requiredBufferSize, *subThemeNameBufferSize);
    }
  }
  else
  {
    LogDebug(L"MHW: invalid sub-theme identifiers, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, sub-theme ID = %hhu",
              index, recordEvent->Version, recordEvent->EventId,
              recordEvent->ThemeId, recordEvent->SubThemeId);
  }

  return true;
}

STDMETHODIMP_(bool) CParserMhw::GetDescriptionLine(unsigned long long eventId,
                                                    unsigned char index,
                                                    char* line,
                                                    unsigned short* lineBufferSize)
{
  if (line == NULL || *lineBufferSize == 0)
  {
    LogDebug(L"MHW: description line buffer not provided, event ID = %llu, index = %hhu",
              eventId, index);
    return false;
  }

  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (!m_recordsDescription.GetRecordByKey(eventId, &record) || record == NULL)
  {
    LogDebug(L"MHW: invalid description line event ID, event ID = %llu",
              eventId);
    return false;
  }

  CRecordMhwDescription* recordDescription = dynamic_cast<CRecordMhwDescription*>(record);
  if (recordDescription == NULL)
  {
    LogDebug(L"MHW: invalid description line record, event ID = %llu, index = %hhu",
              eventId, index);
    return false;
  }
  if (index >= recordDescription->Lines.size())
  {
    LogDebug(L"MHW: invalid description line index, event ID = %llu, index = %hhu, line count = %llu",
              eventId, index,
              (unsigned long long)recordDescription->Lines.size());
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordDescription->Lines[index],
                                  line,
                                  *lineBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"MHW: insufficient description line buffer size, event ID = %llu, index = %hhu, required size = %hu, actual size = %hu",
              eventId, index, requiredBufferSize, *lineBufferSize);
  }
  return true;
}

void CParserMhw::OnNewSection(int pid, int tableId, CSection& section)
{
  try
  {
    CEnterCriticalSection lock(m_section);

    //LogDebug(L"MHW: PID = %d, table ID = 0x%x, section length = %d",
    //          pid, tableId, section.section_length);
    unsigned long newOrChangedRecordCount = 0;
    unsigned short dataLength = section.section_length - 1;   // + 3 for table ID and section length bytes, - 4 for CRC bytes

    if (pid == PID_MHW1_EVENTS && section.table_id == TABLE_ID_MHW1_EVENTS)
    {
      newOrChangedRecordCount = DecodeVersion1EventSection(section.Data, dataLength);
    }
    else if (pid == PID_MHW1_OTHER)
    {
      if (section.table_id == TABLE_ID_MHW1_CHANNELS)
      {
        newOrChangedRecordCount = DecodeVersion1ChannelSection(section.Data, dataLength);
      }
      else if (section.table_id == TABLE_ID_MHW1_DESCRIPTIONS)
      {
        newOrChangedRecordCount = DecodeVersion1DescriptionSection(section.Data, dataLength);
      }
      else if (section.table_id == TABLE_ID_MHW1_THEMES)
      {
        newOrChangedRecordCount = DecodeVersion1ThemeSection(section.Data, dataLength);
      }
    }
    else if (
      pid == PID_MHW2_CHANNELS_AND_THEMES &&
      section.table_id == TABLE_ID_MHW2_CHANNELS_AND_THEMES
    )
    {
      if (dataLength >= 4)
      {
        if (section.Data[3] == 0)
        {
          newOrChangedRecordCount = DecodeVersion2ChannelSection(section.Data, dataLength);
        }
        else if (section.Data[3] == 1)
        {
          newOrChangedRecordCount = DecodeVersion2ThemeSection(section.Data, dataLength);
        }
      }
    }
    else if (pid == PID_MHW2_EVENTS && section.table_id == TABLE_ID_MHW2_EVENTS)
    {
      newOrChangedRecordCount = DecodeVersion2EventSection(section.Data, dataLength);
    }
    else if (pid == PID_MHW2_DESCRIPTIONS && section.table_id == TABLE_ID_MHW2_DESCRIPTIONS)
    {
      newOrChangedRecordCount = DecodeVersion2DescriptionSection(section.Data, dataLength);
    }

    if (newOrChangedRecordCount > 0)
    {
      if (!m_isSeen)
      {
        LogDebug(L"MHW: received, PID = %d, table ID = 0x%x",
                  pid, section.table_id);
        m_isSeen = true;
        if (m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableSeen(PID_MHW_CALL_BACK, TABLE_ID_MHW_CALL_BACK);
        }
      }
      else if (m_isReady)
      {
        LogDebug(L"MHW: changed, PID = %d, table ID = 0x%x",
                  pid, section.table_id);
        m_isReady = false;
        if (m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableChange(PID_MHW_CALL_BACK, TABLE_ID_MHW_CALL_BACK);
        }
      }
      m_completeTime = clock();
    }
    else
    {
      if (!m_isReady && CTimeUtils::ElapsedMillis(m_completeTime) <= 20000)
      {
        return;
      }

      unsigned long nonCurrentRecordCount = 0;
      if (CTimeUtils::ElapsedMillis(m_lastExpiredRecordCheckTime) >= 10000)
      {
        nonCurrentRecordCount = m_recordsChannel.RemoveExpiredRecords(NULL);
        nonCurrentRecordCount += m_recordsEvent.RemoveExpiredRecords(NULL);
        nonCurrentRecordCount += m_recordsDescription.RemoveExpiredRecords(NULL);
        nonCurrentRecordCount += m_recordsTheme.RemoveExpiredRecords(NULL);
        m_lastExpiredRecordCheckTime = clock();
      }

      // Did something actually change?
      if (!m_isReady || nonCurrentRecordCount > 0)
      {
        LogDebug(L"MHW: ready, channel count = %lu, event count = %lu, description count = %lu, theme count = %lu",
                  m_recordsChannel.GetRecordCount(),
                  m_recordsEvent.GetRecordCount(),
                  m_recordsDescription.GetRecordCount(),
                  m_recordsTheme.GetRecordCount());
        m_isReady = true;
        if (m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableComplete(PID_MHW_CALL_BACK, TABLE_ID_MHW_CALL_BACK);
        }
      }
    }
  }
  catch (...)
  {
    LogDebug(L"MHW: unhandled exception in OnNewSection()");
  }
}

void CParserMhw::AddOrResetDecoder(unsigned short pid, bool enableCrcCheck)
{
  CSectionDecoder* decoder = NULL;
  map<unsigned short, CSectionDecoder*>::const_iterator it = m_decoders.find(pid);
  if (it == m_decoders.end() || it->second == NULL)
  {
    decoder = new CSectionDecoder();
    if (decoder == NULL)
    {
      LogDebug(L"MHW: failed to allocate section decoder for PID %hu", pid);
      return;
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
}

unsigned long CParserMhw::DecodeVersion1ChannelSection(unsigned char* data,
                                                        unsigned short dataLength)
{
  // table ID [0x91] - 1 byte
  // section length - 2 bytes
  // [unknown, channel count???] - 1 byte
  // channels...
  //   original network ID - 2 bytes
  //   transport stream ID - 2 bytes
  //   service ID - 2 bytes
  //   name - 16 bytes
  if (data == NULL || dataLength < 4)
  {
    LogDebug(L"MHW: invalid version 1 channel section, length = %hu",
              dataLength);
    return 0;
  }

  // Assume there is only one channel section.
  m_recordsChannel.MarkExpiredRecords(1);

  unsigned long newOrChangedRecordCount = 0;
  unsigned char index = 0;
  unsigned short pointer = 4;
  while (pointer + 21 < dataLength)
  {
    CRecordMhwChannel* record = new CRecordMhwChannel();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 1 channel record");
    }
    else
    {
      record->Version = 1;
      record->Id = index;
      record->OriginalNetworkId = (data[pointer] << 8) | data[pointer + 1];
      record->TransportStreamId = (data[pointer + 2] << 8) | data[pointer + 3];
      record->ServiceId = (data[pointer + 4] << 8) | data[pointer + 5];

      if (!CTextUtil::DvbTextToString(&data[pointer + 6], 16, &(record->Name)))
      {
        LogDebug(L"MHW: invalid version 1 channel section, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hhu",
                  record->OriginalNetworkId, record->TransportStreamId,
                  record->ServiceId, record->Id);
        return newOrChangedRecordCount;
      }
      if (record->Name == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 1 channel name, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hhu",
                  record->OriginalNetworkId, record->TransportStreamId,
                  record->ServiceId, record->Id);
      }

      //LogDebug(L"MHW: channel, version = 1, ID = %hhu, ONID = %hu, TSID = %hu, service ID = %hu, name = %S",
      //          record->Id, record->OriginalNetworkId, record->TransportStreamId,
      //          record->ServiceId, record->Name);
      if (m_recordsChannel.AddOrUpdateRecord((IRecord**)&record, NULL))
      {
        newOrChangedRecordCount++;
      }
    }

    index++;
    pointer += 22;
  }
  return newOrChangedRecordCount;
}

unsigned long CParserMhw::DecodeVersion1EventSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0x90] - 1 byte
  // section length - 2 bytes
  // channel ID - 1 byte
  // theme ID - 1 byte
  // start date/time...
  //   day of week - 3 bits, encoding = custom binary
  //   hours - 5 bits, encoding = custom binary, offset from start of yesterday [local time]
  //   minutes - 6 bits, encoding = binary, offset from start of yesterday [local time]
  // [unknown] - 1 bit
  // description available - 1 bit
  // [unknown] - 2 bytes
  // duration - 2 bytes, unit = minutes
  // title - 23 bytes
  // pay per view ID - 4 bytes
  // event ID - 4 bytes
  // [unknown] - 4 bytes
  if (data == NULL || dataLength < 42)
  {
    LogDebug(L"MHW: invalid version 1 event section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned long eventId = (data[38] << 24) | (data[39] << 16) | (data[40] << 8) | data[41];
  if (eventId == 0xffffffff)
  {
    LogDebug(L"MHW: invalid version 1 event section, ID = %lu", eventId);
    return 0;
  }

  CRecordMhwEvent* record = new CRecordMhwEvent();
  if (record == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 1 event record, ID = %lu",
              eventId);
    return 0;
  }

  record->Version = 1;
  record->EventId = eventId;
  record->ChannelId = data[3] - 1;
  record->ThemeId = data[4];
  record->SubThemeId = 0;    // not applicable

  unsigned char dayOfWeek = (data[5] & 0xe0) >> 5;
  unsigned char originalDayOfWeek = dayOfWeek;
  unsigned char offsetHours = data[5] & 0x1f;
  unsigned char originalOffsetHours = offsetHours;
  unsigned char offsetMinutes = (data[6] >> 2);

  // I don't understand most of this code. However, apparently it works!
  if (offsetHours > 15)
  {
    offsetHours -= 4;
  }
  else if (offsetHours > 7)
  {
    offsetHours -= 2;
  }
  else
  {
    dayOfWeek++;
  }
  dayOfWeek = dayOfWeek % 7;

  time_t nowTime = time(NULL);
  tm* nowTm = localtime(&nowTime);
  if (m_dayOfWeek != nowTm->tm_wday)  // New day started?
  {
    m_dayOfWeek = nowTm->tm_wday;

    time_t yesterdayTime = nowTime - 86400;         // This is the UTC epoch 24 hours ago.
    tm* tempConversion = gmtime(&yesterdayTime);
    tempConversion->tm_isdst = -1;    // DST status unknown
    yesterdayTime = 2 * yesterdayTime - mktime(tempConversion); // This is the UTC epoch for the local time 24 hours ago.

    tm* yesterdayTm = gmtime(&yesterdayTime);
    m_yesterdayDayOfWeek = yesterdayTm->tm_wday;    // The day of the week for yesterday in the local time zone.

    yesterdayTm->tm_hour = 0;
    yesterdayTm->tm_min = 0;
    yesterdayTm->tm_sec = 0;
    yesterdayTm->tm_isdst = -1;       // DST status unknown
    m_yesterdayEpoch = mktime(yesterdayTm);         // This should be the UTC epoch for the start of yesterday in the local time zone.
  }

  long offsetStartDateTime = ((dayOfWeek - m_yesterdayDayOfWeek) * 86400) + (offsetHours * 3600) + (offsetMinutes * 60);
  if (offsetStartDateTime < 21600)  // Before 6AM yesterday?
  {
    offsetStartDateTime += 604800;  // Increase offset by 1 week.
  }
  record->StartDateTime = m_yesterdayEpoch + offsetStartDateTime;

  record->HasDescription = (data[6] & 0x01) != 0;
  record->Duration = (data[9] << 8) | data[10];   // unit = minutes

  if (!CTextUtil::DvbTextToString(&data[11], 23, &(record->Title)))
  {
    LogDebug(L"MHW: invalid version 1 event section, section length = %hu, event ID = %lu, channel ID = %hhu",
              dataLength, eventId, record->ChannelId);
    delete record;
    return 0;
  }
  if (record->Title == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 1 event title, event ID = %lu, channel ID = %hhu",
              eventId, record->ChannelId);
    delete record;
    return 0;
  }

  record->PayPerViewId = (data[34] << 24) | (data[35] << 16) | (data[36] << 8) | data[37];

  //LogDebug(L"MHW: event, version = 1, ID = %lu, channel ID = %hhu, theme ID = %hhu, day of week = %hhu, hours = %hhu, minutes = %hhu, start date/time = %llu, duration = %hu, has description = %d, title = %S, PPV ID = %lu",
  //          eventId, record->ChannelIndex, record->ThemeId, originalDayOfWeek,
  //          originalOffsetHours, offsetMinutes, record->StartDateTime,
  //          record->Duration, record->HasDescription, record->Title,
  //          record->PayPerViewId);
  if (m_recordsEvent.AddOrUpdateRecord((IRecord**)&record, NULL))
  {
    return 1;
  }
  return 0;
}

unsigned long CParserMhw::DecodeVersion1DescriptionSection(unsigned char* data,
                                                            unsigned short dataLength)
{
  // table ID [0x90] - 1 byte
  // section length - 2 bytes
  // event ID - 4 bytes
  // [unknown] - 3 bytes
  // replay count - 1 byte
  //   channel ID - 1 byte
  //   date/time - 5 bytes, encoding = EIT MJD???
  //   flags - 1 byte
  // description
  if (data == NULL || dataLength < 12)
  {
    LogDebug(L"MHW: invalid version 1 description section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned long eventId = (data[3] << 24) | (data[4] << 16) | (data[5] << 8) | data[6];
  if (eventId == 0xffffffff)
  {
    LogDebug(L"MHW: invalid version 1 description section, event ID = %lu",
              eventId);
    return 0;
  }

  unsigned char replayCount = data[10];
  unsigned short startOfDescription = 11 + (replayCount * 7);
  short descriptionLength = dataLength - startOfDescription;
  char* description = NULL;
  if (
    descriptionLength <= 0 ||
    !CTextUtil::DvbTextToString(&data[startOfDescription], descriptionLength, &description)
  )
  {
    LogDebug(L"MHW: invalid version 1 description section, section length = %hu, replay count = %hhu, event ID = %lu",
              dataLength, replayCount, eventId);
    return 0;
  }
  if (description == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 1 event description, event ID = %lu",
              eventId);
    return 0;
  }

  CRecordMhwDescription* record = new CRecordMhwDescription();
  if (record == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 1 description record, event ID = %lu",
              eventId);
    delete[] description;
    return 0;
  }

  record->Version = 1;
  record->EventId = eventId;
  record->Description = description;
  //LogDebug(L"MHW: description, version = 1, event ID = %lu, description = %S, replay count = %hhu",
  //          eventId, description, replayCount);

  unsigned short pointer = 11;
  for (unsigned char i = 0; i < replayCount; i++)
  {
    unsigned char channelId = data[pointer++];
    unsigned long long startDateTime = ((unsigned long long)data[pointer] << 32) | (data[pointer + 1] << 24) | (data[pointer + 2] << 16) | (data[pointer + 3] << 8) | data[pointer + 4];
    pointer += 5;
    unsigned char flags = data[pointer++];
    //LogDebug(L"  channel ID = %hhu, start date/time = %llu, flags = %hhu",
    //          channelId, startDateTime, flags);
  }

  if (m_recordsDescription.AddOrUpdateRecord((IRecord**)&record, NULL))
  {
    return 1;
  }
  return 0;
}

unsigned long CParserMhw::DecodeVersion1ThemeSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0x92] - 1 byte
  // section length - 2 bytes
  // groups... [x 16]
  //   group ID - 1 byte
  // names... [offset 19 bytes]
  //   name - 15 bytes
  if (data == NULL || dataLength < 34)
  {
    LogDebug(L"MHW: invalid version 1 theme section, length = %hu",
              dataLength);
    return 0;
  }

  // Assume there is only one theme section.
  m_recordsTheme.MarkExpiredRecords(1);

  unsigned long newOrChangedRecordCount = 0;
  unsigned short namePointer = 19;
  unsigned char groupId = 0;
  unsigned char id = 0;
  unsigned char index = 0;
  while (index < 16 && namePointer + 15 <= dataLength)
  {
    if (data[groupId + 3] == index)
    {
      id = groupId * 16;
      groupId++;
    }
    
    CRecordMhwTheme* record = new CRecordMhwTheme();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 1 theme record, ID = %hhu",
                id);
    }
    else
    {
      record->Version = 1;
      record->Id = id;
      record->SubId = 0;    // not applicable

      if (!CTextUtil::DvbTextToString(&data[namePointer], 15, &(record->Name)))
      {
        LogDebug(L"MHW: invalid version 1 theme section, section length = %hu, theme ID = %hhu",
                  dataLength, id);
        delete record;
        return newOrChangedRecordCount;
      }
      if (record->Name == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 1 theme name, ID = %hhu",
                  id);
      }

      //LogDebug(L"MHW: theme, version = 1, ID = %hhu, name = %S",
      //          id, record->Name == NULL ? "" : record->Name);
      if (m_recordsTheme.AddOrUpdateRecord((IRecord**)&record, NULL))
      {
        newOrChangedRecordCount++;
      }
    }

    namePointer += 15;
    id++;
    index++;
  }
  return newOrChangedRecordCount;
}

unsigned long CParserMhw::DecodeVersion2ChannelSection(unsigned char* data,
                                                        unsigned short dataLength)
{
  // table ID [0xc8] - 1 byte
  // section length - 2 bytes
  // data type - 1 byte, 0 = channels, 1 = themes
  // [unknown] - 116 bytes
  // channel count - 1 byte
  // channels...
  //   original network ID - 2 bytes
  //   transport stream ID - 2 bytes
  //   service ID - 2 bytes
  //   [unknown] - 2 bytes
  // channel names...
  //   [unknown] - 2 bits
  //   name length - 6 bits
  //   name - [name length] bytes
  if (data == NULL || dataLength < 121)
  {
    LogDebug(L"MHW: invalid version 2 channel section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned char channelCount = data[120];
  if (dataLength < 121 + (9 * channelCount))
  {
    LogDebug(L"MHW: invalid version 2 channel section, channel count = %hhu, section length = %hu",
              channelCount, dataLength);
    return 0;
  }

  // Assume there is only one channel section.
  m_recordsChannel.MarkExpiredRecords(2);

  unsigned long newOrChangedRecordCount = 0;
  unsigned short pointer = 121;
  unsigned short namePointer = pointer + (8 * channelCount);
  for (unsigned char i = 0; i < channelCount; i++)
  {
    if (namePointer >= dataLength)
    {
      LogDebug(L"MHW: invalid version 2 channel section, pointer = %hu, channel count = %hhu, channel index = %hhu, section length = %hu",
                namePointer, channelCount, i, dataLength);
      return newOrChangedRecordCount;
    }

    unsigned char nameLength = data[namePointer++] & 0x3f;
    if (namePointer + nameLength > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 channel section, name length = %hhu, pointer = %hu, channel count = %hhu, channel index = %hhu, section length = %hu",
                nameLength, namePointer, channelCount, i, dataLength);
      return newOrChangedRecordCount;
    }

    CRecordMhwChannel* record = new CRecordMhwChannel();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 channel record");
      return newOrChangedRecordCount;
    }

    record->Version = 2;
    record->Id = i;
    record->OriginalNetworkId = (data[pointer] << 8) | data[pointer + 1];
    record->TransportStreamId = (data[pointer + 2] << 8) | data[pointer + 3];
    record->ServiceId = (data[pointer + 4] << 8) | data[pointer + 5];
    pointer += 8;

    if (nameLength > 0)
    {
      if (!CTextUtil::DvbTextToString(&data[namePointer], nameLength, &(record->Name)))
      {
        LogDebug(L"MHW: invalid version 2 channel section, name length = %hhu, pointer = %hu, channel count = %hhu, channel index = %hhu, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hhu",
                  nameLength, namePointer, channelCount, i,
                  record->OriginalNetworkId, record->TransportStreamId,
                  record->ServiceId, i);
        return newOrChangedRecordCount;
      }
      if (record->Name == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 2 channel name, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hhu",
                  record->OriginalNetworkId, record->TransportStreamId,
                  record->ServiceId, i);
      }
      namePointer += nameLength;
    }

    //LogDebug(L"MHW: channel, version = 2, ID = %hhu, ONID = %hu, TSID = %hu, service ID = %hu, name = %S",
    //          i, record->OriginalNetworkId, record->TransportStreamId,
    //          record->ServiceId, record->Name == NULL ? "" : record->Name);
    if (m_recordsChannel.AddOrUpdateRecord((IRecord**)&record, NULL))
    {
      newOrChangedRecordCount++;
    }
  }
  return newOrChangedRecordCount;
}

unsigned long CParserMhw::DecodeVersion2EventSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0xe6] - 1 byte
  // section length - 2 bytes
  // [unknown] - 4 bytes
  // [unknown] - 4 bits
  // theme ID - 4 bits
  // [unknown] - 10 bytes
  // titles...
  //   channel ID - 1 byte
  //   [unknown] - 10 bytes
  //   start date/time...
  //     date - 2 bytes, encoding = MJD
  //     hours - 1 byte, encoding = BCD
  //     minutes - 1 byte, encoding = BCD
  //   [unknown] - 1 byte
  //   duration - 2 bytes
  //   [unknown] - 2 bits
  //   title length - 6 bits
  //   title - [title length] bytes
  //   [unknown] - 2 bits
  //   sub-theme ID - 6 bits
  //   event ID - 2 bytes
  if (data == NULL || dataLength < 18)
  {
    LogDebug(L"MHW: invalid version 2 event section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned long newOrChangedRecordCount = 0;
  unsigned short pointer = 18;
  unsigned char themeId = data[7] & 0xf;
  while (pointer + 21 < dataLength)
  {
    unsigned char titleLength = data[pointer + 18] & 0x3f;
    if (pointer + 22 + titleLength > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 event section, title length = %hhu, pointer = %hu, section length = %hu",
                titleLength, dataLength, pointer);
      return newOrChangedRecordCount;
    }

    CRecordMhwEvent* record = new CRecordMhwEvent();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event record");
      return newOrChangedRecordCount;
    }

    record->Version = 2;
    record->EventId = (data[pointer + 20 + titleLength] << 8) | data[pointer + 21 + titleLength];
    record->ChannelId = data[pointer];
    record->ThemeId = themeId;

    // Decode the MJD-encoded start date.
    unsigned short startDateMjd = (data[pointer + 11] << 8) | data[pointer + 12];
    long j = startDateMjd + 2400001 + 68569;
    long c = 4 * j / 146097;
    j = j - (146097 * c + 3) / 4;
    long y = 4000 * (j + 1) / 1461001;
    j = j - 1461 * y / 4 + 31;
    long m = 80 * j / 2447;

    tm startDateTime;
    startDateTime.tm_sec = 0;
    startDateTime.tm_min = ((data[pointer + 14] >> 4) * 10) + (data[pointer + 14] & 0x0f);
    startDateTime.tm_hour = ((data[pointer + 13] >> 4) * 10) + (data[pointer + 13] & 0x0f);
    startDateTime.tm_mday = j - 2447 * m / 80;
    j = m / 11;
    startDateTime.tm_mon = m + 2 - 12 * j - 1;              // 0..11
    startDateTime.tm_year = 100 * (c - 49) + y + j - 1900;  // year - 1900
    startDateTime.tm_isdst = -1;

    // As far as I know the date/time is a local time. mktime() will give us
    // the corresponding UTC epoch, as required.
    record->StartDateTime = mktime(&startDateTime);

    record->Duration = (data[pointer + 16] | data[pointer + 17]) >> 4;

    if (!CTextUtil::DvbTextToString(&data[pointer + 19], titleLength, &(record->Title)))
    {
      LogDebug(L"MHW: invalid version 2 event section, title length = %hhu, pointer = %hu, section length = %hu, event ID = %lu, channel ID = %hhu",
                titleLength, dataLength, pointer, record->EventId,
                record->ChannelId);
      delete record;
      return newOrChangedRecordCount;
    }
    if (record->Title == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event title, event ID = %lu, channel ID = %hhu",
                record->EventId, record->ChannelId);
      delete record;
      return newOrChangedRecordCount;
    }

    record->SubThemeId = data[pointer + 19 + titleLength] & 0x3f;

    // not available, or encoded in an [unknown] field
    record->PayPerViewId = 0;
    record->HasDescription = true;

    //LogDebug(L"MHW: event, version = 2, ID = %lu, channel ID = %hhu, theme ID = %hhu, sub-theme ID = %hhu, start date MJD = %hu, start date/time = %llu, duration = %hu m, title = %S",
    //          record->Id, record->ChannelId, record->ThemeId,
    //          record->SubThemeId, startDateMjd, record->StartDateTime,
    //          record->Duration, record->Title);
    if (m_recordsEvent.AddOrUpdateRecord((IRecord**)&record, NULL))
    {
      newOrChangedRecordCount++;
    }

    pointer += 22 + titleLength;
  }
  return newOrChangedRecordCount;
}

unsigned long CParserMhw::DecodeVersion2DescriptionSection(unsigned char* data,
                                                            unsigned short dataLength)
{
  // table ID [0x96] - 1 byte
  // section length - 2 bytes
  // event ID - 2 bytes
  // [unknown] - 9 bytes
  // description length - 1 byte
  // description - [description length] bytes
  // line count - 1 byte
  // lines...
  //   line length - 1 byte
  //   line - [line length] bytes
  if (data == NULL || dataLength < 15)
  {
    LogDebug(L"MHW: invalid version 2 description section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned short eventId = (data[3] << 8) | data[4];
  if (eventId == 0xffff)
  {
    LogDebug(L"MHW: invalid version 2 description section, event ID = %hu",
              eventId);
    return 0;
  }

  // EPG Collector check, unknown purpose.
  if (data[5] != 0 || (data[6] != 0 && data[6] != 1))
  {
    LogDebug(L"MHW: invalid version 2 description section, length = %hu, byte 6 = %hhu, byte 7 = %hhu, event ID = %hu",
              dataLength, data[5], data[6], eventId);
    return 0;
  }

  unsigned char descriptionLength = data[14];
  char* description = NULL;
  if (
    15 + descriptionLength + 1 > dataLength ||
    !CTextUtil::DvbTextToString(&data[15], descriptionLength, &description)
  )
  {
    LogDebug(L"MHW: invalid version 2 description section, description length = %hhu, section length = %hu, event ID = %hu",
              descriptionLength, dataLength, eventId);
    return 0;
  }
  if (description == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 2 event description, event ID = %hu",
              eventId);
    return 0;
  }

  CRecordMhwDescription* record = new CRecordMhwDescription();
  if (record == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 2 description record, event ID = %hu",
              eventId);
    delete[] description;
    return 0;
  }

  record->Version = 2;
  record->EventId = eventId;
  record->Description = description;

  unsigned short pointer = 15 + descriptionLength;
  unsigned char lineCount = data[pointer++];
  //LogDebug(L"MHW: description, version = 2, event ID = %hu, line count = %hhu, description = %S",
  //          eventId, lineCount, record->Description);
  for (unsigned char i = 0; i < lineCount; i++)
  {
    if (pointer >= dataLength)
    {
      LogDebug(L"MHW: invalid version 2 description section, pointer = %hu, section length = %hu, event ID = %hu, description length = %hhu, line count = %hhu, line index = %hhu",
                pointer, dataLength, eventId, descriptionLength, lineCount, i);
      delete record;
      return 0;
    }

    unsigned char lineLength = data[pointer++];
    char* line = NULL;
    if (
      pointer + lineLength > dataLength ||
      !CTextUtil::DvbTextToString(&data[pointer], lineLength, &line)
    )
    {
      LogDebug(L"MHW: invalid version 2 description section, line length = %hhu, pointer = %hu, section length = %hu, event ID = %hu, description length = %hhu, line count = %hhu, line index = %hhu",
                lineLength, pointer, dataLength, eventId, descriptionLength,
                lineCount, i);
      delete record;
      return 0;
    }

    if (line == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event description line, event ID = %hu, line index = %hhu",
                eventId, i);
    }
    else
    {
      //LogDebug(L"  index = %hhu, line = %S", i, line);
      record->Lines.push_back(line);
    }
    pointer += lineLength;
  }

  if (m_recordsDescription.AddOrUpdateRecord((IRecord**)&record, NULL))
  {
    return 1;
  }
  return 0;
}

unsigned long CParserMhw::DecodeVersion2ThemeSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0xc8] - 1 byte
  // section length - 2 bytes
  // data type - 1 byte, 0 = channels, 1 = themes
  // theme count - 1 byte
  // theme detail pointers...
  //   pointer 1 - 2 bytes
  // theme details... [offset [pointer 1]]
  //   [unknown] - 2 bits
  //   sub-theme count - 6 bits
  //   sub-theme name pointers...
  //     pointer 2 - 2 bytes
  // sub-theme name... [offset [pointer 2]]
  //   [unknown] - 3 bits
  //   sub-theme name length - 5 bits
  //   sub-theme name - [sub-theme name length] bytes
  if (data == NULL || dataLength < 5)
  {
    LogDebug(L"MHW: invalid version 2 theme section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned char themeCount = data[4];
  if (dataLength < 5 + (2 * themeCount))
  {
    LogDebug(L"MHW: invalid version 2 theme section, theme count = %hhu, section length = %hu",
              themeCount, dataLength);
    return 0;
  }

  // Assume there is only one theme section.
  m_recordsTheme.MarkExpiredRecords(2);

  unsigned long newOrChangedRecordCount = 0;
  unsigned short pointer = 5;
  for (unsigned char themeIndex = 0; themeIndex < themeCount; themeIndex++)
  {
    unsigned short themeDetailPointer = (data[pointer] << 8) | data[pointer + 1] + 3;   // + 3 for the table ID and section length bytes
    pointer += 2;

    if (themeDetailPointer >= dataLength)
    {
      LogDebug(L"MHW: invalid version 2 theme section, theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, section length = %hu",
                themeDetailPointer, themeCount, themeIndex, dataLength);
      return newOrChangedRecordCount;
    }
    unsigned char subThemeCount = data[themeDetailPointer++] & 0x3f;
    if (themeDetailPointer + (2 * subThemeCount) > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 theme section, sub-theme count = %hhu, theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, section length = %hu",
                subThemeCount, themeDetailPointer, themeCount, themeIndex,
                dataLength);
      return newOrChangedRecordCount;
    }
    for (unsigned char subThemeIndex = 0; subThemeIndex <= subThemeCount; subThemeIndex++)
    {
      unsigned short subThemeDetailPointer = (data[themeDetailPointer] << 8) | data[themeDetailPointer + 1] + 3;    // + 3 for the table ID and section length bytes
      themeDetailPointer += 2;

      if (subThemeDetailPointer >= dataLength)
      {
        LogDebug(L"MHW: invalid version 2 theme section, sub-theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, theme detail pointer = %hu, sub-theme count = %hhu, sub-theme index = %hhu, section length = %hu",
                  subThemeDetailPointer,
                  themeCount, themeIndex, themeDetailPointer,
                  subThemeCount, subThemeIndex, dataLength);
        return newOrChangedRecordCount;
      }

      unsigned char subThemeNameLength = data[subThemeDetailPointer++];
      char* name = NULL;
      if (
        subThemeDetailPointer + subThemeNameLength > dataLength ||
        !CTextUtil::DvbTextToString(&data[subThemeDetailPointer], subThemeNameLength, &name)
      )
      {
        LogDebug(L"MHW: invalid version 2 theme section, sub-theme name length = %hhu, sub-theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, theme detail pointer = %hu, sub-theme count = %hhu, sub-theme index = %hhu, section length = %hu",
                  subThemeNameLength, subThemeDetailPointer,
                  themeCount, themeIndex, themeDetailPointer,
                  subThemeCount, subThemeIndex, dataLength);
        return newOrChangedRecordCount;
      }

      CRecordMhwTheme* record = new CRecordMhwTheme();
      if (record == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 2 theme record, ID = %hhu, sub ID = %hhu",
                  themeIndex, subThemeIndex);
        if (name != NULL)
        {
          delete[] name;
        }
        return newOrChangedRecordCount;
      }

      record->Version = 2;
      record->Id = themeIndex;
      record->SubId = subThemeIndex;
      if (name == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 2 theme name, ID = %hhu, sub ID = %hhu",
                  themeIndex, subThemeIndex);
      }
      record->Name = name;

      //LogDebug(L"MHW: theme, version = 2, ID = %hhu, sub ID = %hhu, name = %S",
      //          themeIndex, subThemeIndex,
      //          record->Name == NULL ? "" : record->Name);
      if (m_recordsTheme.AddOrUpdateRecord((IRecord**)&record, NULL))
      {
        newOrChangedRecordCount++;
      }
    }
  }
  return newOrChangedRecordCount;
}