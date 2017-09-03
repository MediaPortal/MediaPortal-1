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
#include <time.h>       // gmtime(), time_t, tm
#include "..\..\shared\EnterCriticalSection.h"
#include "..\..\shared\TimeUtils.h"
#include "ParserTot.h"
#include "PidUsage.h"
#include "TextUtil.h"
#include "Utils.h"


#define COUNTRY_FRA   0x415246
#define COUNTRY_POL   0x4c4f50

#define LANG_FRA 0x617266
#define LANG_POL 0x6c6f70
#define LANG_SPA 0x617073
#define LANG_UND 0x646e75


extern void LogDebug(const wchar_t* fmt, ...);

CParserMhw::CParserMhw(ICallBackPidConsumer* callBack,
                        ISystemTimeInfoProviderDvb* systemTimeInfoProvider,
                        LPUNKNOWN unk,
                        HRESULT* hr)
  : CUnknown(NAME("MediaHighway EPG Grabber"), unk), m_recordsChannel(600000),
    m_recordsChannelCategory(600000), m_recordsDescription(600000),
    m_recordsEventSatellite(600000), m_recordsEventTerrestrial(600000),
    m_recordsProgram(600000), m_recordsSeries(600000),
    m_recordsShowing(600000), m_recordsTheme(600000),
    m_recordsThemeDescription(600000)
{
  if (callBack == NULL)
  {
    LogDebug(L"MHW: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_previousSegmentEventsByChannelSatellite = 0;
  m_previousSegmentEventsByChannelTerrestrial = 0;
  m_previousSegmentEventsByTheme = 0;
  m_previousEventIdSatellite = 0;
  m_previousEventIdTerrestrial = 0;
  m_previousDescriptionId = -1;
  m_previousProgramSectionId = -1;
  m_previousSeriesSectionId = -1;
  m_firstDescriptionId = 0xffffffff;

  m_currentHour = -1;
  m_referenceDateTime = 0;
  m_referenceDayOfWeek = 0;

  m_completeTime = 0;
  m_provider = MhwProviderUnknown;
  m_grabMhw1 = false;
  m_grabMhw2 = false;
  m_isSeen = false;
  m_isReady = false;

  m_callBackGrabber = NULL;
  m_callBackPidConsumer = callBack;
  m_systemTimeInfoProvider = systemTimeInfoProvider;
  m_enableCrcCheck = true;

  m_descriptionTableBuffer = NULL;
  m_descriptionTableBufferSize = 0;
  m_descriptionSectionNumber = 0;

  m_themeDescriptionTableBuffer = NULL;
  m_themeDescriptionTableBufferSize = 0;
  m_themeDescriptionSectionNumber = 0;

  m_currentEvent = NULL;
  m_currentEventIndex = 0xffffffff;
  m_currentDescription = NULL;
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

  if (m_descriptionTableBuffer != NULL)
  {
    delete[] m_descriptionTableBuffer;
    m_descriptionTableBuffer = NULL;
  }
  m_descriptionTableBufferSize = 0;

  if (m_themeDescriptionTableBuffer != NULL)
  {
    delete[] m_themeDescriptionTableBuffer;
    m_themeDescriptionTableBuffer = NULL;
  }
  m_themeDescriptionTableBufferSize = 0;
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

void CParserMhw::SetTransportStream(unsigned short originalNetworkId,
                                    unsigned short transportStreamId)
{
  LogDebug(L"MHW: set transport stream, ONID = %hu, TSID = %hu", originalNetworkId);

  MhwProvider newProvider = DetermineProvider(originalNetworkId, transportStreamId);

  CEnterCriticalSection lock(m_section);
  if (newProvider == m_provider)
  {
    return;
  }

  // Text decoding and time calculations depend on the provider.
  m_provider = newProvider;
  if (
    m_recordsChannel.GetRecordCount() > 0 ||
    m_recordsChannelCategory.GetRecordCount() > 0 ||
    m_recordsDescription.GetRecordCount() > 0 ||
    m_recordsEventSatellite.GetRecordCount() > 0 ||
    m_recordsEventTerrestrial.GetRecordCount() > 0 ||
    m_recordsProgram.GetRecordCount() > 0 ||
    m_recordsSeries.GetRecordCount() > 0 ||
    //m_recordsShowing.GetRecordCount() > 0 ||
    m_recordsTheme.GetRecordCount() > 0 ||
    m_recordsThemeDescription.GetRecordCount() > 0
  )
  {
    m_recordsChannel.RemoveAllRecords();
    m_recordsChannelCategory.RemoveAllRecords();
    m_recordsDescription.RemoveAllRecords();
    m_recordsEventSatellite.RemoveAllRecords();
    m_recordsEventTerrestrial.RemoveAllRecords();
    m_recordsProgram.RemoveAllRecords();
    m_recordsSeries.RemoveAllRecords();
    //m_recordsShowing.RemoveAllRecords();
    m_recordsTheme.RemoveAllRecords();
    m_recordsThemeDescription.RemoveAllRecords();
    m_channelIdLookup.clear();

    m_currentHour = -1;
    m_referenceDateTime = 0;
    m_referenceDayOfWeek = 0;

    m_currentEvent = NULL;
    m_currentEventIndex = 0xffffffff;
    m_currentDescription = NULL;

    m_isSeen = false;
    m_isReady = false;
  }
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
      pidsAdd.push_back(PID_TOT);         // provides required time information for converting local time to UTC
      pidsAdd.push_back(PID_MHW1_EVENTS);
      pidsAdd.push_back(PID_MHW1_OTHER);
    }
    else
    {
      pidsRemove.push_back(PID_TOT);
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
      pidsAdd.push_back(PID_MHW2_DESCRIPTIONS);
      pidsAdd.push_back(PID_MHW2_EVENTS_BY_CHANNEL_SATELLITE);
      pidsAdd.push_back(PID_MHW2_EVENTS_BY_CHANNEL_TERRESTRIAL);
      //pidsAdd.push_back(PID_MHW2_EVENTS_BY_THEME);
      pidsAdd.push_back(PID_MHW2_PROGRAMS_AND_SERIES);
      //pidsAdd.push_back(PID_MHW2_THEME_DESCRIPTIONS);
    }
    else
    {
      pidsRemove.push_back(PID_MHW2_CHANNELS_AND_THEMES);
      pidsRemove.push_back(PID_MHW2_DESCRIPTIONS);
      pidsRemove.push_back(PID_MHW2_EVENTS_BY_CHANNEL_SATELLITE);
      pidsRemove.push_back(PID_MHW2_EVENTS_BY_CHANNEL_TERRESTRIAL);
      //pidsRemove.push_back(PID_MHW2_EVENTS_BY_THEME);
      pidsRemove.push_back(PID_MHW2_PROGRAMS_AND_SERIES);
      //pidsRemove.push_back(PID_MHW2_THEME_DESCRIPTIONS);
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
  AddOrResetDecoder(PID_MHW2_DESCRIPTIONS, enableCrcCheck);
  AddOrResetDecoder(PID_MHW2_EVENTS_BY_CHANNEL_SATELLITE, enableCrcCheck);
  AddOrResetDecoder(PID_MHW2_EVENTS_BY_CHANNEL_TERRESTRIAL, enableCrcCheck);
  //AddOrResetDecoder(PID_MHW2_EVENTS_BY_THEME, enableCrcCheck);
  AddOrResetDecoder(PID_MHW2_PROGRAMS_AND_SERIES, enableCrcCheck);
  //AddOrResetDecoder(PID_MHW2_THEME_DESCRIPTIONS, enableCrcCheck);

  m_recordsChannel.RemoveAllRecords();
  m_recordsChannelCategory.RemoveAllRecords();
  m_recordsDescription.RemoveAllRecords();
  m_recordsEventSatellite.RemoveAllRecords();
  m_recordsEventTerrestrial.RemoveAllRecords();
  m_recordsProgram.RemoveAllRecords();
  m_recordsSeries.RemoveAllRecords();
  m_recordsShowing.RemoveAllRecords();
  m_recordsTheme.RemoveAllRecords();
  m_recordsThemeDescription.RemoveAllRecords();

  m_previousSegmentEventsByChannelSatellite = 0;
  m_previousSegmentEventsByChannelTerrestrial = 0;
  m_previousSegmentEventsByTheme = 0;
  m_previousEventIdSatellite = 0;
  m_previousEventIdTerrestrial = 0;
  m_previousDescriptionId = -1;
  m_previousProgramSectionId = -1;
  m_previousSeriesSectionId = -1;
  m_firstDescriptionId = 0xffffffff;

  m_currentHour = -1;
  m_referenceDateTime = 0;
  m_referenceDayOfWeek = 0;

  m_channelIdLookup.clear();

  if (m_descriptionTableBuffer != NULL)
  {
    delete[] m_descriptionTableBuffer;
    m_descriptionTableBuffer = NULL;
  }
  m_descriptionTableBufferSize = 0;
  m_descriptionSectionNumber = 0;

  if (m_themeDescriptionTableBuffer != NULL)
  {
    delete[] m_themeDescriptionTableBuffer;
    m_themeDescriptionTableBuffer = NULL;
  }
  m_themeDescriptionTableBufferSize = 0;
  m_themeDescriptionSectionNumber = 0;

  m_currentEvent = NULL;
  m_currentEventIndex = 0xffffffff;
  m_currentDescription = NULL;

  m_provider = MhwProviderUnknown;
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
    m_provider == MhwProviderUnknown ||
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
        header.Pid == PID_MHW2_DESCRIPTIONS ||
        header.Pid == PID_MHW2_EVENTS_BY_CHANNEL_SATELLITE ||
        header.Pid == PID_MHW2_EVENTS_BY_CHANNEL_TERRESTRIAL ||
        //header.Pid == PID_MHW2_EVENTS_BY_THEME ||
        header.Pid == PID_MHW2_PROGRAMS_AND_SERIES// ||
        //header.Pid == PID_MHW2_THEME_DESCRIPTIONS
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

STDMETHODIMP_(void) CParserMhw::GetEventCount(unsigned long* eventCount,
                                              unsigned long* textLanguage)
{
  CEnterCriticalSection lock(m_section);
  *eventCount = m_recordsEventSatellite.GetRecordCount() + m_recordsEventTerrestrial.GetRecordCount();
  if (m_provider == CanalsatFrance)
  {
    *textLanguage = LANG_FRA;
  }
  else if (m_provider == CyfraPoland)
  {
    *textLanguage = LANG_POL;
  }
  else if (m_provider == MovistarPlusSpain)
  {
    *textLanguage = LANG_SPA;
  }
  else
  {
    *textLanguage = LANG_UND;
  }
}

STDMETHODIMP_(bool) CParserMhw::GetEvent(unsigned long index,
                                          unsigned char* version,
                                          unsigned long* eventId,
                                          unsigned short* originalNetworkId,
                                          unsigned short* transportStreamId,
                                          unsigned short* serviceId,
                                          char* serviceName,
                                          unsigned short* serviceNameBufferSize,
                                          unsigned long long* startDateTime,
                                          unsigned short* duration,
                                          char* title,
                                          unsigned short* titleBufferSize,
                                          char* description,
                                          unsigned short* descriptionBufferSize,
                                          unsigned char* descriptionLineCount,
                                          unsigned long* seriesId,
                                          unsigned char* seasonNumber,
                                          unsigned long* episodeId,
                                          unsigned short* episodeNumber,
                                          char* episodeName,
                                          unsigned short* episodeNameBufferSize,
                                          char* themeName,
                                          unsigned short* themeNameBufferSize,
                                          char* subThemeName,
                                          unsigned short* subThemeNameBufferSize,
                                          unsigned char* classification,
                                          unsigned long* payPerViewId)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(index))
  {
    return false;
  }

  *version = m_currentEvent->Version;
  *eventId = m_currentEvent->EventId;
  *originalNetworkId = 0;
  *transportStreamId = 0;
  *serviceId = 0;
  *startDateTime = m_currentEvent->StartDateTime;
  *duration = m_currentEvent->Duration;
  *descriptionLineCount = 0;
  *seriesId = 0xffffffff;
  *seasonNumber = 0;
  *episodeId = m_currentEvent->ProgramId;
  *episodeNumber = 0;
  *classification = 0xff;
  *payPerViewId = m_currentEvent->PayPerViewId;

  unsigned short requiredBufferSize = 0;
  IRecord* record = NULL;
  if (
    m_recordsChannel.GetRecordByKey((m_currentEvent->Version << 9) | ((m_currentEvent->IsTerrestrial ? 1 : 0) << 8) | m_currentEvent->ChannelId, &record) &&
    record != NULL
  )
  {
    CRecordMhwChannel* recordChannel = dynamic_cast<CRecordMhwChannel*>(record);
    if (recordChannel == NULL)
    {
      LogDebug(L"MHW: invalid channel record, index = %lu, version = %hhu, event ID = %lu, channel ID = %hhu, is terrestrial = %d",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                m_currentEvent->ChannelId, m_currentEvent->IsTerrestrial);
      CUtils::CopyStringToBuffer(NULL, serviceName, *serviceNameBufferSize, requiredBufferSize);
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
        LogDebug(L"MHW: insufficient service name buffer size, index = %lu, version = %hhu, event ID = %lu, channel ID = %hhu, is terrestrial = %d, required size = %hu, actual size = %hu",
                  index, m_currentEvent->Version, m_currentEvent->EventId,
                  m_currentEvent->ChannelId, m_currentEvent->IsTerrestrial,
                  requiredBufferSize, *serviceNameBufferSize);
      }
    }
  }
  else
  {
    LogDebug(L"MHW: invalid channel identifiers, index = %lu, version = %hhu, event ID = %lu, channel ID = %hhu, is terrestrial = %d",
              index, m_currentEvent->Version, m_currentEvent->EventId,
              m_currentEvent->ChannelId, m_currentEvent->IsTerrestrial);
    CUtils::CopyStringToBuffer(NULL, serviceName, *serviceNameBufferSize, requiredBufferSize);
  }

  if (m_currentDescription == NULL)
  {
    CUtils::CopyStringToBuffer(NULL, description, *descriptionBufferSize, requiredBufferSize);
  }
  else
  {
    *classification = m_currentDescription->Classification;
    *descriptionLineCount = (unsigned char)(m_currentDescription->Lines.size());
    if (!CUtils::CopyStringToBuffer(m_currentDescription->Description,
                                    description,
                                    *descriptionBufferSize,
                                    requiredBufferSize))
    {
      LogDebug(L"MHW: insufficient description buffer size, index = %lu, version = %hhu, event ID = %lu, description ID = %lu, required size = %hu, actual size = %hu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                m_currentEvent->DescriptionId, requiredBufferSize,
                *descriptionBufferSize);
    }
  }

  // Careful with these two! The correct record to use depends on MHW version
  // *and* parsed section.
  unsigned char themeId = m_currentEvent->ThemeId;
  unsigned char subThemeId = m_currentDescription->SubThemeId;
  if (m_currentEvent->Version == 2)
  {
    themeId = m_currentDescription->ThemeId;
  }

  if (
    m_recordsTheme.GetRecordByKey((m_currentEvent->Version << 16) | (themeId << 8), &record) &&
    record != NULL
  )
  {
    CRecordMhwTheme* recordTheme = dynamic_cast<CRecordMhwTheme*>(record);
    if (recordTheme == NULL)
    {
      LogDebug(L"MHW: invalid theme record, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                themeId);
      CUtils::CopyStringToBuffer(NULL, themeName, *themeNameBufferSize, requiredBufferSize);
    }
    else if (!CUtils::CopyStringToBuffer(recordTheme->Name,
                                          themeName,
                                          *themeNameBufferSize,
                                          requiredBufferSize))
    {
      LogDebug(L"MHW: insufficient theme name buffer size, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, required size = %hu, actual size = %hu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                themeId, requiredBufferSize, *themeNameBufferSize);
    }
  }
  else
  {
    LogDebug(L"MHW: invalid theme identifiers, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu",
              index, m_currentEvent->Version, m_currentEvent->EventId,
              themeId);
    CUtils::CopyStringToBuffer(NULL, themeName, *themeNameBufferSize, requiredBufferSize);
  }

  if (
    m_recordsTheme.GetRecordByKey((m_currentEvent->Version << 16) | (themeId << 8) | subThemeId, &record) &&
    record != NULL
  )
  {
    CRecordMhwTheme* recordTheme = dynamic_cast<CRecordMhwTheme*>(record);
    if (recordTheme == NULL)
    {
      LogDebug(L"MHW: invalid sub-theme record, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, sub-theme ID = %hhu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                themeId, subThemeId);
      CUtils::CopyStringToBuffer(NULL, subThemeName, *subThemeNameBufferSize, requiredBufferSize);
    }
    else if (!CUtils::CopyStringToBuffer(recordTheme->Name,
                                          subThemeName,
                                          *subThemeNameBufferSize,
                                          requiredBufferSize))
    {
      LogDebug(L"MHW: insufficient sub-theme name buffer size, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, sub-theme ID = %hhu, required size = %hu, actual size = %hu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                themeId, subThemeId, requiredBufferSize,
                *subThemeNameBufferSize);
    }
  }
  else
  {
    LogDebug(L"MHW: invalid sub-theme identifiers, index = %lu, version = %hhu, event ID = %lu, theme ID = %hhu, sub-theme ID = %hhu",
              index, m_currentEvent->Version, m_currentEvent->EventId, themeId,
              subThemeId);
    CUtils::CopyStringToBuffer(NULL, subThemeName, *subThemeNameBufferSize, requiredBufferSize);
  }

  CRecordMhwProgram* recordProgram = NULL;
  unsigned long tempSeriesId = 0;
  if (m_currentEvent->ProgramId != 0xffffffff)
  {
    if (
      !m_recordsProgram.GetRecordByKey(((unsigned long long)m_currentEvent->Version << 32) | m_currentEvent->ProgramId, &record) ||
      record == NULL
    )
    {
      LogDebug(L"MHW: invalid program identifiers, index = %lu, version = %hhu, event ID = %lu, program ID = %lu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                m_currentEvent->ProgramId);
    }
    else
    {
      recordProgram = dynamic_cast<CRecordMhwProgram*>(record);
      if (recordProgram == NULL)
      {
        LogDebug(L"MHW: invalid program record, index = %lu, version = %hhu, event ID = %lu, program ID = %lu",
                  index, m_currentEvent->Version, m_currentEvent->EventId,
                  m_currentEvent->ProgramId);
      }
    }
  }
  if (recordProgram == NULL)
  {
    CUtils::CopyStringToBuffer(NULL, episodeName, *episodeNameBufferSize, requiredBufferSize);
  }
  else
  {
    *seriesId = recordProgram->SeriesId;
    tempSeriesId = recordProgram->SeriesId;
    *classification = recordProgram->Classification;    // override; preferred over 3-bit-limited description classification
    if (!CUtils::CopyStringToBuffer(recordProgram->Title,
                                    episodeName,
                                    *episodeNameBufferSize,
                                    requiredBufferSize))
    {
      LogDebug(L"MHW: insufficient episode name buffer size, index = %lu, version = %hhu, event ID = %lu, program ID = %lu, required size = %hu, actual size = %hu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                m_currentEvent->ProgramId, requiredBufferSize,
                *episodeNameBufferSize);
    }
  }

  CRecordMhwSeries* recordSeries = NULL;
  bool haveSeriesTitle = false;
  if (tempSeriesId != 0xffffffff)
  {
    if (
      !m_recordsSeries.GetRecordByKey(((unsigned long long)m_currentEvent->Version << 32) | *seriesId, &record) ||
      record == NULL
    )
    {
      LogDebug(L"MHW: invalid series identifiers, index = %lu, version = %hhu, event ID = %lu, series ID = %lu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                tempSeriesId);
    }
    else
    {
      recordSeries = dynamic_cast<CRecordMhwSeries*>(record);
      if (recordSeries == NULL)
      {
        LogDebug(L"MHW: invalid series record, index = %lu, version = %hhu, event ID = %lu, program ID = %lu",
                  index, m_currentEvent->Version, m_currentEvent->EventId,
                  tempSeriesId);
      }
    }
  }
  if (recordSeries != NULL)
  {
    // Season name is usually a numeric string containing the season number.
    if (recordSeries->SeasonName != NULL)
    {
      unsigned char i = 0;
      while (true)
      {
        char c = recordSeries->SeasonName[i];
        if (c == NULL)
        {
          break;
        }
        if (c >= 0x20)
        {
          *seasonNumber = (unsigned char)strtoul(&(recordSeries->SeasonName[i]), NULL, 10);
          break;
        }
        i++;
      }
    }

    // Find and populate the episode number.
    map<unsigned long, unsigned short>::const_iterator it = recordSeries->EpisodeNumbers.find(m_currentEvent->ProgramId);
    if (it == recordSeries->EpisodeNumbers.end())
    {
      LogDebug(L"MHW: missing episode number, index = %lu, version = %hhu, event ID = %lu, series ID = %lu, episode ID = %lu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                tempSeriesId, m_currentEvent->ProgramId);
    }
    if (it->second != 0xffff)
    {
      *episodeNumber = it->second;
    }

    // Prefer series name to event title. This has the effect of excluding
    // suffixes such as "(HD)" and "(T\d+)".
    haveSeriesTitle = recordSeries->SeriesName != NULL;
    if (!CUtils::CopyStringToBuffer(recordSeries->SeriesName,
                                    title,
                                    *titleBufferSize,
                                    requiredBufferSize))
    {
      LogDebug(L"MHW: insufficient event title buffer size, index = %lu, version = %hhu, event ID = %lu, series ID = %lu, required size = %hu, actual size = %hu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                tempSeriesId, requiredBufferSize, *titleBufferSize);
    }
  }

  if (
    !haveSeriesTitle &&
    !CUtils::CopyStringToBuffer(m_currentEvent->Title,
                                title,
                                *titleBufferSize,
                                requiredBufferSize)
  )
  {
    LogDebug(L"MHW: insufficient event title buffer size, index = %lu, version = %hhu, event ID = %lu, required size = %hu, actual size = %hu",
              index, m_currentEvent->Version, m_currentEvent->EventId,
              requiredBufferSize, *titleBufferSize);
  }

  return true;
}

STDMETHODIMP_(bool) CParserMhw::GetDescriptionLine(unsigned long eventIndex,
                                                    unsigned char lineIndex,
                                                    char* line,
                                                    unsigned short* lineBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectEventRecordByIndex(eventIndex) || m_currentDescription == NULL)
  {
    return false;
  }

  if (lineIndex >= m_currentDescription->Lines.size())
  {
    LogDebug(L"MHW: invalid description line index, event index = %lu, version = %hhu, event ID = %lu, line index = %hhu, line count = %llu",
              eventIndex, m_currentEvent->Version, m_currentEvent->EventId,
              lineIndex,
              (unsigned long long)m_currentDescription->Lines.size());
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(m_currentDescription->Lines[lineIndex],
                                  line,
                                  *lineBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"MHW: insufficient description line buffer size, event index = %lu, version = %hhu, event ID = %lu, line index = %hhu, required size = %hu, actual size = %hu",
              eventIndex, m_currentEvent->Version, m_currentEvent->EventId,
              lineIndex, requiredBufferSize, *lineBufferSize);
  }
  return true;
}

bool CParserMhw::GetService(unsigned short originalNetworkId,
                            unsigned short transportStreamId,
                            unsigned short serviceId,
                            bool* isHighDefinition,
                            bool* isStandardDefinition,
                            unsigned short* categoryIds,
                            unsigned char* categoryIdCount) const
{
  unsigned long long key = ((unsigned long long)originalNetworkId << 32) | (transportStreamId << 16) | serviceId;

  CEnterCriticalSection lock(m_section);
  map<unsigned long long, unsigned short>::const_iterator channelIdIt = m_channelIdLookup.find(key);
  if (channelIdIt == m_channelIdLookup.end())
  {
    // Not an error.
    return false;
  }

  IRecord* record = NULL;
  if (
    !m_recordsChannel.GetRecordByKey(channelIdIt->second, &record) ||
    record == NULL
  )
  {
    LogDebug(L"MHW: invalid channel identifier, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hu",
              originalNetworkId, transportStreamId, serviceId,
              channelIdIt->second);
    return false;
  }

  CRecordMhwChannel* recordChannel = dynamic_cast<CRecordMhwChannel*>(record);
  if (recordChannel == NULL)
  {
    LogDebug(L"MHW: invalid channel record, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hu",
              originalNetworkId, transportStreamId, serviceId,
              channelIdIt->second);
    return false;
  }

  *isHighDefinition = recordChannel->IsHighDefinition;
  *isStandardDefinition = recordChannel->IsStandardDefinition;

  vector<unsigned short> tempCategoryIds;
  vector<unsigned char>::const_iterator categoryIdIt = recordChannel->CategoryIds.begin();
  for ( ; categoryIdIt != recordChannel->CategoryIds.end(); categoryIdIt++)
  {
    unsigned short categoryId = (recordChannel->Version << 9) |
                                ((recordChannel->IsTerrestrial ? 1 : 0) << 8) |
                                *categoryIdIt;
    tempCategoryIds.push_back(categoryId);
  }
  unsigned char requiredCount;
  if (!CUtils::CopyVectorToArray(tempCategoryIds, categoryIds, *categoryIdCount, requiredCount))
  {
    LogDebug(L"MHW: insufficient channel category ID array size, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hu, required size = %hhu, actual size = %hhu",
              originalNetworkId, transportStreamId, serviceId,
              channelIdIt->second, requiredCount,
              *categoryIdCount);
  }
  return true;
}

bool CParserMhw::GetChannelCategoryName(unsigned short categoryId,
                                        char* name,
                                        unsigned short* nameBufferSize) const
{
  CEnterCriticalSection lock(m_section);
  IRecord* record = NULL;
  if (
    !m_recordsChannelCategory.GetRecordByKey(categoryId, &record) ||
    record == NULL
  )
  {
    LogDebug(L"MHW: invalid channel category identifier, ID = %hu",
              categoryId);
    return false;
  }

  CRecordMhwChannelCategory* recordChannelCategory = dynamic_cast<CRecordMhwChannelCategory*>(record);
  if (recordChannelCategory == NULL)
  {
    LogDebug(L"MHW: invalid channel category record, ID = %hu", categoryId);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordChannelCategory->Name,
                                  name,
                                  *nameBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"MHW: insufficient channel category name buffer size, ID = %hu, required size = %hu, actual size = %hu",
              categoryId, requiredBufferSize, *nameBufferSize);
  }
  return true;
}

bool CParserMhw::SelectEventRecordByIndex(unsigned long index)
{
  if (m_currentEvent != NULL && m_currentEventIndex == index)
  {
    return true;
  }

  unsigned long eventCountSatellite = m_recordsEventSatellite.GetRecordCount();
  bool gotEvent = false;
  IRecord* record = NULL;
  if (index < eventCountSatellite)
  {
    gotEvent = m_recordsEventSatellite.GetRecordByIndex(index, &record);
  }
  else
  {
    gotEvent = m_recordsEventTerrestrial.GetRecordByIndex(index - eventCountSatellite, &record);
  }
  if (!gotEvent || record == NULL)
  {
    LogDebug(L"MHW: invalid event index, index = %lu, satellite event count = %lu, terrestrial event count = %lu",
              index, eventCountSatellite,
              m_recordsEventTerrestrial.GetRecordCount());
    return false;
  }

  m_currentEvent = dynamic_cast<CRecordMhwEvent*>(record);
  if (m_currentEvent == NULL)
  {
    LogDebug(L"MHW: invalid event record, index = %lu", index);
    return false;
  }
  m_currentEventIndex = index;
  m_currentDescription = NULL;

  if (
    !m_recordsDescription.GetRecordByKey(((unsigned long long)m_currentEvent->Version << 32) | m_currentEvent->DescriptionId, &record) ||
    record == NULL
  )
  {
    if (m_currentEvent->HasDescription)
    {
      LogDebug(L"MHW: invalid description identifiers, index = %lu, version = %hhu, event ID = %lu, description ID = %lu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                m_currentEvent->DescriptionId);
    }
  }
  else
  {
    m_currentDescription = dynamic_cast<CRecordMhwDescription*>(record);
    if (m_currentDescription == NULL)
    {
      LogDebug(L"MHW: invalid description record, index = %lu, version = %hhu, event ID = %lu, description ID = %lu",
                index, m_currentEvent->Version, m_currentEvent->EventId,
                m_currentEvent->DescriptionId);
    }
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
    unsigned long updateCount = 0;
    unsigned short dataLength = section.section_length + 3;   // + 3 for table ID and section length bytes
    if (section.SectionSyntaxIndicator)
    {
      if (!section.CurrentNextIndicator)
      {
        return;
      }
      dataLength -= 4;                                        // - 4 for CRC
    }

    if (pid == PID_MHW1_EVENTS && section.table_id == TABLE_ID_MHW1_EVENTS)
    {
      updateCount = DecodeVersion1EventSection(section.Data, dataLength);
    }
    else if (pid == PID_MHW1_OTHER)
    {
      if (section.table_id == TABLE_ID_MHW1_CHANNELS)
      {
        updateCount = DecodeVersion1ChannelSection(section.Data, dataLength);
      }
      else if (section.table_id == TABLE_ID_MHW1_DESCRIPTIONS)
      {
        updateCount = DecodeVersion1DescriptionSection(section.Data, dataLength);
      }
      else if (section.table_id == TABLE_ID_MHW1_THEMES)
      {
        updateCount = DecodeVersion1ThemeSection(section.Data, dataLength);
      }
    }
    else if (
      pid == PID_MHW2_CHANNELS_AND_THEMES &&
      section.table_id == TABLE_ID_MHW2_CHANNELS_AND_THEMES
    )
    {
      if (dataLength >= 4)
      {
        if (section.Data[3] == 1)
        {
          updateCount = DecodeVersion2ThemeSection(section.Data, dataLength);
        }
        else if (section.Data[3] == 2 || section.Data[3] == 3)    // ignore the SD lineup
        {
          updateCount = DecodeVersion2ChannelSection(section.Data, dataLength);
        }
      }
    }
    else if (
      (
        pid == PID_MHW2_EVENTS_BY_CHANNEL_SATELLITE ||
        pid == PID_MHW2_EVENTS_BY_CHANNEL_TERRESTRIAL
      ) &&
      section.table_id == TABLE_ID_MHW2_EVENTS
    )
    {
      updateCount = DecodeVersion2EventsByChannelSection(section.Data,
                                                        dataLength,
                                                        pid == PID_MHW2_EVENTS_BY_CHANNEL_TERRESTRIAL);
    }
    // Currently not used. We get event details from other PIDs.
    /*else if (pid == PID_MHW2_EVENTS_BY_THEME && section.table_id == TABLE_ID_MHW2_EVENTS_BY_THEME)
    {
      updateCount = DecodeVersion2EventsByThemeSection(section.Data, dataLength);
    }*/
    else if (pid == PID_MHW2_DESCRIPTIONS && section.table_id == TABLE_ID_MHW2_DESCRIPTIONS)
    {
      if (section.LastSectionNumber == 0)
      {
        updateCount = DecodeVersion2DescriptionSection(section.Data, dataLength);
      }
      else
      {
        CompleteTable(section,
                      &m_descriptionTableBuffer,
                      m_descriptionTableBufferSize,
                      m_descriptionSectionNumber);
        if (m_descriptionTableBuffer != NULL && section.SectionNumber == section.LastSectionNumber)
        {
          updateCount = DecodeVersion2DescriptionSection(m_descriptionTableBuffer,
                                                          m_descriptionTableBufferSize);
          delete[] m_descriptionTableBuffer;
          m_descriptionTableBuffer = NULL;
          m_descriptionTableBufferSize = 0;
          m_descriptionSectionNumber = 0;
        }
      }
    }
    // Currently not used.
    /*else if (
      pid == PID_MHW2_THEME_DESCRIPTIONS &&
      section.table_id == TABLE_ID_MHW2_THEME_DESCRIPTIONS
    )
    {
      if (section.LastSectionNumber == 0)
      {
        updateCount = DecodeVersion2ThemeDescriptionSection(section.Data, dataLength);
      }
      else
      {
        CompleteTable(section,
                      &m_themeDescriptionTableBuffer,
                      m_themeDescriptionTableBufferSize,
                      m_themeDescriptionSectionNumber);
        if (
          m_themeDescriptionTableBuffer != NULL &&
          section.SectionNumber == section.LastSectionNumber
        )
        {
          updateCount = DecodeVersion2ThemeDescriptionSection(m_themeDescriptionTableBuffer,
                                                              m_themeDescriptionTableBufferSize);
          delete[] m_themeDescriptionTableBuffer;
          m_themeDescriptionTableBuffer = NULL;
          m_themeDescriptionTableBufferSize = 0;
          m_themeDescriptionSectionNumber = 0;
        }
      }
    }*/
    else if (pid == PID_MHW2_PROGRAMS_AND_SERIES)
    {
      if (section.table_id == TABLE_ID_MHW2_PROGRAMS)
      {
        updateCount = DecodeVersion2ProgramSection(section.Data, dataLength);
      }
      else if (section.table_id == TABLE_ID_MHW2_SERIES)
      {
        updateCount = DecodeVersion2SeriesSection(section.Data, dataLength);
      }
    }

    if (updateCount > 0)
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
    else if (!m_isReady)
    {
      if (CTimeUtils::ElapsedMillis(m_completeTime) <= 20000)
      {
        return;
      }

      LogDebug(L"MHW: ready, channel count = %lu, channel category count = %lu, satellite event count = %lu, terrestrial event count = %lu, description count = %lu, program count = %lu, series count = %lu, showing count = %lu, theme count = %lu, theme description count = %lu",
                m_recordsChannel.GetRecordCount(),
                m_recordsChannelCategory.GetRecordCount(),
                m_recordsEventSatellite.GetRecordCount(),
                m_recordsEventTerrestrial.GetRecordCount(),
                m_recordsDescription.GetRecordCount(),
                m_recordsProgram.GetRecordCount(),
                m_recordsSeries.GetRecordCount(),
                m_recordsShowing.GetRecordCount(),
                m_recordsTheme.GetRecordCount(),
                m_recordsThemeDescription.GetRecordCount());
      m_isReady = true;
      if (m_callBackGrabber != NULL)
      {
        m_callBackGrabber->OnTableComplete(PID_MHW_CALL_BACK, TABLE_ID_MHW_CALL_BACK);
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
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // [unknown] - 1 byte, always seems to be 0xff (Canalsat France, Cyfra+ Poland)
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
  m_recordsChannel.MarkExpiredRecords(1 << 1);

  unsigned char unknown = data[3];
  unsigned long updateCount = 0;
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
      record->Id = index + 1;
      record->OriginalNetworkId = (data[pointer] << 8) | data[pointer + 1];
      record->TransportStreamId = (data[pointer + 2] << 8) | data[pointer + 3];
      record->ServiceId = (data[pointer + 4] << 8) | data[pointer + 5];

      if (!CTextUtil::MhwTextToString(&data[pointer + 6],
                                      16,
                                      m_provider,
                                      &(record->Name)))
      {
        LogDebug(L"MHW: invalid version 1 channel section, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hhu",
                  record->OriginalNetworkId, record->TransportStreamId,
                  record->ServiceId, record->Id);
        return updateCount;
      }
      if (record->Name == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 1 channel name, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hhu",
                  record->OriginalNetworkId, record->TransportStreamId,
                  record->ServiceId, record->Id);
      }

      //LogDebug(L"MHW: channel, version = 1, unknown = %hhu, ID = %hhu, ONID = %hu, TSID = %hu, service ID = %hu, name = %S",
      //          unknown, record->Id, record->OriginalNetworkId,
      //          record->TransportStreamId, record->ServiceId, record->Name);
      if (m_recordsChannel.AddOrUpdateRecord((IRecord**)&record, NULL))
      {
        updateCount++;
      }
    }

    index++;
    pointer += 22;
  }

  updateCount += m_recordsChannel.RemoveExpiredRecords(NULL);
  if (pointer != dataLength)
  {
    LogDebug(L"MHW: version 1 channel section has unexpected trailing bytes, pointer = %hu, length = %hu",
              pointer, dataLength);
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion1DescriptionSection(unsigned char* data,
                                                            unsigned short dataLength)
{
  // table ID [0x90] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // event ID - 4 bytes
  // [unknown] - 3 bytes, always seems to be 0xffffff (Canalsat France, Cyfra+ Poland)
  // replay count - 1 byte
  // replays...
  //   channel ID - 1 byte
  //   start date - 2 bytes, encoding = MJD
  //   start time - 3 bytes, encoding = BCD
  //   flags - 1 byte
  //     no replays for Cyfra+ Poland
  //     Canalsat France
  //       0x80 - last replay???
  //       0x40 - [always 1 in my sample]
  //       0x20
  //       0x10
  //       0x08 - [always 1 in my sample]
  //       0x04 - [always 1 in my sample]
  //       0x02
  //       0x01 - subtitled (EPG Collector)
  // description
  if (data == NULL || dataLength < 11)
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

  unsigned long updateCount = 0;
  if (m_firstDescriptionId == 0xffffffff)
  {
    m_firstDescriptionId = eventId;
  }
  else if (m_firstDescriptionId == eventId)
  {
    updateCount = m_recordsDescription.RemoveExpiredRecords(NULL, 1);
    m_recordsDescription.MarkExpiredRecords(1);
  }

  unsigned long unknown = (data[7] << 16) | (data[8] << 8) | data[9];
  unsigned char replayCount = data[10];
  unsigned short startOfDescription = 11 + (replayCount * 7);
  short descriptionLength = dataLength - startOfDescription;
  if (descriptionLength == 0)
  {
    // This case does actually occur when the section only specifies repeats.
    return 0;
  }

  char* description = NULL;
  if (
    descriptionLength < 0 ||
    !CTextUtil::MhwTextToString(&data[startOfDescription],
                                descriptionLength,
                                m_provider,
                                &description)
  )
  {
    LogDebug(L"MHW: invalid version 1 description section, section length = %hu, unknown = %lu, replay count = %hhu, event ID = %lu",
              dataLength, unknown, replayCount, eventId);
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
  record->Id = eventId;
  record->Description = description;
  //LogDebug(L"MHW: description, version = 1, event ID = %lu, unknown = %lu, description = %S, replay count = %hhu",
  //          eventId, unknown, description, replayCount);

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
    updateCount++;
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion1EventSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0x90] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // channel ID - 1 byte
  // theme ID - 1 byte
  // start day/time (***provider target region [eg. France or Poland] local time***)...
  //   day of week - 3 bits, encoding = binary (0 => [not used], 1 => Monday, 2 => Tuesday etc.)
  //   hour - 5 bits, encoding = custom binary (8 - 13 => 0600 - 1100, 16 - 27 => 1200 - 2300, 0 - 5 => 0000 - 0500 [next day])
  //   minute - 6 bits, encoding = binary
  // [unknown] - 1 bit
  // description available - 1 bit
  // [unknown] - 2 bytes, always seems to be 0xffff (Canalsat France, Cyfra+ Poland)
  // duration - 2 bytes, unit = minutes
  // title - 23 bytes
  // pay per view ID - 4 bytes
  // event ID - 4 bytes, LSB always seems to be 1 (Canalsat France, Cyfra+ Poland) => maybe 31 bits instead of 32
  // [unknown] - 4 bytes, always seems to be 0xffffffff (Canalsat France, Cyfra+ Poland)
  if (data == NULL || dataLength != 46)
  {
    LogDebug(L"MHW: invalid version 1 event section, length = %hu",
              dataLength);
    return 0;
  }

  if (!GetVersion1DateTimeReference(m_referenceDateTime, m_referenceDayOfWeek))
  {
    return 0;
  }

  unsigned long eventId = (data[38] << 24) | (data[39] << 16) | (data[40] << 8) | data[41];
  if (eventId == 0xffffffff)
  {
    // Marker for start of next hour.
    return 0;
  }

  unsigned long updateCount = 0;
  unsigned short segment = (1 << 8) | data[5];
  if (segment != m_previousSegmentEventsVersion1)
  {
    if (m_previousSegmentEventsVersion1 != 0)
    {
      updateCount = m_recordsEventSatellite.RemoveExpiredRecords((ICallBackMhw*)this,
                                                                  m_previousSegmentEventsVersion1);
      m_recordsEventSatellite.MarkExpiredRecords(segment);
    }
    m_previousSegmentEventsVersion1 = segment;
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
  record->DescriptionId = eventId;  // Version 1 descriptions don't have their own ID; link via event ID.
  record->ChannelId = data[3];
  record->ThemeId = data[4];
  record->Segment = data[5];        // Version 1 segments events by day/hour.

  unsigned char dayOfWeek = data[5] >> 5;
  unsigned char originalDayOfWeek = dayOfWeek;
  unsigned char hour = data[5] & 0x1f;
  unsigned char originalHour = hour;
  unsigned char minute = (data[6] >> 2);

  // Hours 6, 7, 14, 15, 28, 29, 30 and 31 are not used, and hours 0 to 5 are
  // associated with the next day (relative to hours 8 to 13 and 16 to 27).
  if (hour > 15)
  {
    hour -= 4;
  }
  else if (hour > 7)
  {
    hour -= 2;
  }
  else
  {
    dayOfWeek++;
  }

  // Match MHW days of the week with C/C++ days of the week.
  if (dayOfWeek == 7)
  {
    dayOfWeek = 0;
  }

  // Use the reference to convert days of the week to actual dates. The
  // inherent assumption is that any obsolete data won't be older than
  // yesterday.
  unsigned char dayOffset = dayOfWeek + 7 - m_referenceDayOfWeek;
  if (dayOffset > 6)
  {
    dayOffset -= 7;
  }

  record->StartDateTime = m_referenceDateTime + (dayOffset * SECONDS_PER_DAY) +
                          (hour * SECONDS_PER_HOUR) + (minute * 60);
  record->HasDescription = (data[6] & 0x01) != 0;
  bool unknown1 = (data[6] & 0x02) != 0;
  unsigned short unknown2 = (data[7] << 8) | data[8];
  record->Duration = (data[9] << 8) | data[10];   // unit = minutes

  if (!CTextUtil::MhwTextToString(&data[11], 23, m_provider, &(record->Title)))
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
  unsigned long unknown3 = (data[42] << 24) | (data[43] << 16) | (data[44] << 8) | data[45];

  //LogDebug(L"MHW: event, version = 1, ID = %lu, channel ID = %hhu, theme ID = %hhu, day of week = %hhu, hour = %hhu, minute = %hhu, start date/time = %llu, duration = %hu, has description = %d, title = %S, PPV ID = %lu, unknown 1 = %d, unknown 2 = %hu, unknown 3 = %lu",
  //          eventId, record->ChannelId, record->ThemeId, originalDayOfWeek,
  //          originalHour, minute, record->StartDateTime, record->Duration,
  //          record->HasDescription, record->Title, record->PayPerViewId,
  //          unknown1, unknown2, unknown3);
  if (m_recordsEventSatellite.AddOrUpdateRecord((IRecord**)&record, NULL))
  {
    updateCount++;
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion1ThemeSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0x92] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // group indices... [x 16]
  //   index of first theme in group - 1 byte
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

  unsigned long updateCount = 0;
  unsigned short namePointer = 19;
  unsigned char groupIndex = 0;
  unsigned char id = 0;
  unsigned char index = 0;
  while (namePointer + 14 < dataLength)
  {
    if (groupIndex < 16 && data[groupIndex + 3] == index)
    {
      id = groupIndex * 16;
      groupIndex++;
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

      if (!CTextUtil::MhwTextToString(&data[namePointer],
                                      15,
                                      m_provider,
                                      &(record->Name)))
      {
        LogDebug(L"MHW: invalid version 1 theme section, section length = %hu, theme ID = %hhu",
                  dataLength, id);
        delete record;
        return updateCount;
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
        updateCount++;
      }
    }

    namePointer += 15;
    if (id == 255)
    {
      break;
    }
    id++;
    index++;
  }

  updateCount += m_recordsTheme.RemoveExpiredRecords(NULL);
  return updateCount;
}

bool CParserMhw::GetVersion1DateTimeReference(unsigned long long& referenceDateTime,
                                              unsigned char& referenceDayOfWeek)
{
  // The questions we have to answer are:
  // 1. What day of the week was yesterday, relative to the current time in the
  // provider's time-zone?
  // 2. What was the epoch/Unix/POSIX time at the reference time (the start of
  // yesterday in the provider's time-zone)?
  unsigned long long systemTime;
  unsigned char localTimeOffsetCount;
  if (!m_systemTimeInfoProvider->GetSystemTimeDetail(systemTime,
                                                      localTimeOffsetCount))
  {
    return false;
  }

  time_t nowTime = (time_t)systemTime;
  tm* nowTm = gmtime(&nowTime);

  // Minimise repeated recalculation. Some time-zones are offset from UTC by a
  // factor of half an hour. Recalculations for those time-zones would be slow,
  // but we don't anticipate that will be a problem for the actual deployments
  // of MediaHighway version 1.
  if (m_currentHour == nowTm->tm_hour)
  {
    return m_referenceDateTime != 0;
  }
  m_currentHour = nowTm->tm_hour;

  unsigned long countryCode = 0;
  unsigned char regionId = 0;
  long localTimeOffsetCurrent = 0;
  unsigned long long localTimeOffsetNextChangeDateTime;
  long localTimeOffsetNext;
  if (m_provider == CanalsatFrance)
  {
    countryCode = COUNTRY_FRA;
    if (
      !m_systemTimeInfoProvider->GetLocalTimeOffsetByCountryAndRegion(COUNTRY_FRA,
                                                                      0,
                                                                      localTimeOffsetCurrent,
                                                                      localTimeOffsetNextChangeDateTime,
                                                                      localTimeOffsetNext)
    )
    {
      LogDebug(L"MHW: failed to get Canalsat France time offset");
    }
  }
  else if (m_provider == CyfraPoland)
  {
    countryCode = COUNTRY_POL;
    if (
      !m_systemTimeInfoProvider->GetLocalTimeOffsetByCountryAndRegion(COUNTRY_POL,
                                                                      0,
                                                                      localTimeOffsetCurrent,
                                                                      localTimeOffsetNextChangeDateTime,
                                                                      localTimeOffsetNext)
    )
    {
      LogDebug(L"MHW: failed to get Cyfra+ Poland time offset");
    }
  }
  if (
    localTimeOffsetCurrent == 0 &&
    !m_systemTimeInfoProvider->GetLocalTimeOffsetByIndex(0,
                                                          countryCode,
                                                          regionId,
                                                          localTimeOffsetCurrent,
                                                          localTimeOffsetNextChangeDateTime,
                                                          localTimeOffsetNext)
  )
  {
    LogDebug(L"MHW: failed to get generic provider time offset");
    localTimeOffsetCurrent = 0;
  }

  long utcDayProgress = (nowTm->tm_hour * 3600) + (nowTm->tm_min * 60) + nowTm->tm_sec;
  LogDebug(L"MHW: reference time inputs, system time = %llu, UTC day of week = %ld, UTC day progress = %ld, country code = %S, region ID = %hhu, local time offset = %ld m, next change time = %llu, next time offset = %ld m",
            systemTime, nowTm->tm_wday, utcDayProgress, (char*)&countryCode,
            regionId, localTimeOffsetCurrent,
            localTimeOffsetNextChangeDateTime, localTimeOffsetNext);

  // unit = minutes => seconds
  localTimeOffsetCurrent *= 60;
  localTimeOffsetNext *= 60;

  long providerTimeOffsetNow = localTimeOffsetCurrent;
  if (localTimeOffsetNextChangeDateTime != 0 && systemTime >= localTimeOffsetNextChangeDateTime)
  {
    providerTimeOffsetNow = localTimeOffsetNext;
  }
  unsigned char providerDayOfWeekNow = nowTm->tm_wday;
  if (providerTimeOffsetNow > 0 && utcDayProgress >= (SECONDS_PER_DAY - providerTimeOffsetNow))
  {
    // It's already the next day of the week in the provider's time-zone.
    if (providerDayOfWeekNow == 6)
    {
      providerDayOfWeekNow = 0;
    }
    else
    {
      providerDayOfWeekNow++;
    }
    referenceDateTime = systemTime - utcDayProgress;
  }
  else if (providerTimeOffsetNow < 0 && utcDayProgress < (providerTimeOffsetNow * -1))
  {
    // It's still the previous day of the week in the provider's time-zone.
    if (providerDayOfWeekNow == 0)
    {
      providerDayOfWeekNow = 6;
    }
    else
    {
      providerDayOfWeekNow--;
    }
    referenceDateTime = systemTime - utcDayProgress - (SECONDS_PER_DAY * 2);
  }
  else
  {
    referenceDateTime = systemTime - utcDayProgress - SECONDS_PER_DAY;
  }

  referenceDayOfWeek = providerDayOfWeekNow;
  if (referenceDayOfWeek == 0)
  {
    referenceDayOfWeek = 6;
  }
  else
  {
    referenceDayOfWeek--;
  }

  // Convert the reference date/time from UTC into the provider's time-zone.
  if (
    localTimeOffsetNextChangeDateTime != 0 &&
    referenceDateTime - localTimeOffsetCurrent >= localTimeOffsetNextChangeDateTime
  )
  {
    referenceDateTime -= localTimeOffsetNext;
  }
  else
  {
    referenceDateTime -= localTimeOffsetCurrent;
  }

  LogDebug(L"MHW: reference time, date/time = %llu, day of week = %hhu",
            referenceDateTime, referenceDayOfWeek);
  return true;
}

unsigned long CParserMhw::DecodeVersion2ChannelSection(unsigned char* data,
                                                        unsigned short dataLength)
{
  // table ID [0xc8] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // data type - 1 byte, 0 = standard definition channels, 1 = themes, 2 = high definition channels, 3 = terrestrial channels
  // [unknown] - 1 byte; constant in each sample; seen values 0xa7, 0xc7
  // channel count pointer - 12 bits
  // channel category count pointer - 12 bits
  // last unknown segment pointer - 12 bits
  // [unknown] - 12 bits; constant in each sample; seen values 0x376, 0x886
  // parts of the day... [x 6 => 96 bytes]
  //   start hour - 1 byte, encoding = binary (eg. 0x16 = 10 PM)
  //   start minute - 1 byte, encoding = binary
  //   length - 2 bytes, encoding = binary, unit = minutes
  //   name - 12 bytes
  // [unknown] - 2 bytes, always seems to be 0x3fc7
  // unknown time-stamp (last change???)...
  //   date - 2 bytes, encoding = MJD
  //   time - 3 bytes, encoding = BCD
  // [unknown] - 2 bytes, always seems to be 0x3fff
  // [unknown] - 4 bytes, always seems to be 0xffffffff
  // channel count - 1 byte
  // channels...
  //   original network ID - 2 bytes
  //   transport stream ID - 2 bytes
  //   service ID - 2 bytes
  //   name pointer - 12 bits
  //   flags - 4 bits
  //     0x8 - is high definition
  //     0x4 - is pay per view
  //     0x2 - [always 1 in my samples]
  //     0x1 - [always 1 in my samples]
  // channel names...
  //   [unknown] - 2 bits, usually 0 but sometimes 1
  //   name length - 6 bits
  //   name - [name length] bytes
  // channel category count - 1 byte
  // channel categories...
  //   [unknown] - 4 bits, always seems to be 0xf
  //   name pointer - 12 bits
  //   channel count - 1 byte
  //   channel category mapped channel IDs...
  //     channel ID - 1 byte
  // channel category names...
  //   [unknown] - 2 bits, always seems to be 0x3
  //   name length - 6 bits
  //   name - [name length] bytes
  // [unknown] - [remaining] bytes
  //   standard definition =
  //     [oldest]
  //     F0 00 C8
  //        10 44 49 47 49 54 41 4C 2B 20 50 52 4F 50 4F 4E 45
  //     F0 00 CA
  //     F0 00 CB
  //     00 0C 37 EA FF 00 00 00 01
  //     [middle, newest]
  //     F0 00 C8
  //   high definition =
  //     [oldest]
  //     F0 00 01
  //        10 44 49 47 49 54 41 4C 2B 20 50 52 4F 50 4F 4E 45
  //     F0 00 02
  //     F0 00 03
  //     00 0C 37 EA FF 00 00 00 01
  //     [newest]
  //     F0 00 01
  //       0E 43 41 4E 41 4C 2B 20 50 52 4F 50 4F 4E 45
  //     F0 00 02
  //       04
  //       F0 00 03 00
  //         0E 1D ED FF
  //         04 43 49 4E 45
  //       F0 00 04 00
  //         0E 1D EF FF
  //         06 53 45 52 49 45 53
  //       F0 00 05 00
  //         0E 1D F1 FF
  //         0C 44 4F 43 55 4D 45 4E 54 41 4C 45 53
  //       F0 00 06 00
  //         0E 1D F3 FF
  //         0D 4F 54 52 4F 53 20 47 C9 4E 45 52 4F 53
  //   terrestrial =
  //     [oldest]
  //     F0 00 01
  //        10 44 49 47 49 54 41 4C 2B 20 50 52 4F 50 4F 4E 45
  //     F0 00 02
  //     F0 00 03
  //     00 0C 37 EA FF 00 00 00 01
  //     [middle]
  //     F0 00 01
  //        10 44 49 47 49 54 41 4C 2B 20 50 52 4F 50 4F 4E 45
  //     F0 00 02
  //     [newest]
  //     F0 00 01
  //       0E 43 41 4E 41 4C 2B 20 50 52 4F 50 4F 4E 45
  //     F0 00 02
  if (data == NULL || dataLength < 26)
  {
    LogDebug(L"MHW: invalid version 2 channel section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned char dataType = data[3];
  unsigned char unknown1 = data[4];

  // + 3 for the table ID and section length bytes
  unsigned short channelCountPointer = ((data[5] << 4) | (data[6] >> 4)) + 3;
  unsigned short channelCategoryCountPointer = (((data[6] & 0xf) << 8) | data[7]) + 3;
  unsigned short lastUnknownSegmentPointer = ((data[8] << 4) | (data[9] >> 4)) + 3;

  unsigned short unknown2 = ((data[9] & 0xf) << 8) | data[10];
  //LogDebug(L"MHW: channel section, version = 2, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu",
  //          dataType, unknown1, channelCountPointer,
  //          channelCategoryCountPointer, lastUnknownSegmentPointer,
  //          unknown2);
  if (
    channelCountPointer >= dataLength  ||
    channelCategoryCountPointer >= dataLength ||
    lastUnknownSegmentPointer >= dataLength
  )
  {
    LogDebug(L"MHW: invalid version 2 channel section, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, section length = %hu, data type = %hhu, unknown 1 = %hhu, unknown 2 = %hu",
              channelCountPointer, channelCategoryCountPointer,
              lastUnknownSegmentPointer, dataLength, dataType, unknown1,
              unknown2);
    return 0;
  }

  // [parts of the day - not required]
  unsigned short pointer = channelCountPointer - 13;
  unsigned short unknown3 = (data[pointer] << 8) | data[pointer + 1];
  pointer += 2;
  unsigned short unknownDateMjd = (data[pointer] << 8) | data[pointer + 1];
  pointer += 2;
  unsigned long unknownTimeBcd = (data[pointer] << 16) | (data[pointer + 1] << 8) | data[pointer + 2];
  pointer += 3;
  unsigned short unknown4 = (data[pointer] << 8) | data[pointer + 1];
  pointer += 2;
  unsigned long unknown5 = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
  pointer += 4;
  unsigned char channelCount = data[pointer++];
  //LogDebug(L"MHW: unknown 3 = %hu, unknown date = %hu, unknown time = %lu, unknown 4 = %hu, unknown 5 = %lu, channel count = %hhu",
  //          unknown3, unknownDateMjd, unknownTimeBcd, unknown4, unknown5,
  //          channelCount);
  if (pointer + (8 * channelCount) > dataLength)
  {
    LogDebug(L"MHW: invalid version 2 channel section, pointer = %hu, channel count = %hhu, section length = %hu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu",
              pointer, channelCount, dataLength, dataType, unknown1,
              channelCountPointer, channelCategoryCountPointer,
              lastUnknownSegmentPointer, unknown2);
    return 0;
  }

  bool isTerrestrial = dataType == 3;
  bool parseSuccess = true;
  m_channelIdLookup.clear();
  map<unsigned char, CRecordMhwChannel*> channels;
  for (unsigned char i = 0; i < channelCount; i++)
  {
    CRecordMhwChannel* record = new CRecordMhwChannel();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 channel record");
      parseSuccess = false;
      break;
    }

    record->Version = 2;
    record->Id = i;
    record->OriginalNetworkId = (data[pointer] << 8) | data[pointer + 1];
    record->TransportStreamId = (data[pointer + 2] << 8) | data[pointer + 3];
    record->ServiceId = (data[pointer + 4] << 8) | data[pointer + 5];
    unsigned short namePointer = (data[pointer + 6] << 4) | (data[pointer + 7] >> 4);
    namePointer += 3;     // + 3 for the table ID and section length bytes
    record->IsTerrestrial = isTerrestrial;
    unsigned char flags = data[pointer + 7] & 0xf;
    record->IsHighDefinition = (flags & 8) != 0;
    if (!isTerrestrial || record->IsHighDefinition)
    {
      record->IsStandardDefinition = !record->IsHighDefinition;
    }
    record->IsPayPerView = (flags & 4) != 0;
    pointer += 8;

    if (namePointer >= dataLength)
    {
      LogDebug(L"MHW: invalid version 2 channel section, name pointer = %hu, section length = %hu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel ID = %hhu, flags = %hhu",
                namePointer, dataLength, dataType, unknown1,
                channelCountPointer, channelCategoryCountPointer,
                lastUnknownSegmentPointer, unknown2, channelCount, i, flags);
      parseSuccess = false;
      break;
    }

    unsigned char nameLength = data[namePointer++];
    unsigned char unknown6 = nameLength >> 6;
    nameLength &= 0x3f;
    if (namePointer + nameLength > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 channel section, name pointer = %hu, name length = %hhu, section length = %hu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel ID = %hhu, flags = %hhu, unknown = %hhu",
                namePointer, nameLength, dataLength, dataType, unknown1,
                channelCountPointer, channelCategoryCountPointer,
                lastUnknownSegmentPointer, unknown2, channelCount, i, flags,
                unknown6);
      parseSuccess = false;
      break;
    }

    if (nameLength > 0)
    {
      if (!CTextUtil::MhwTextToString(&data[namePointer], nameLength, m_provider, &(record->Name)))
      {
        LogDebug(L"MHW: invalid version 2 channel section, name pointer = %hu, name length = %hhu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel ID = %hhu, flags = %hhu, unknown = %hhu",
                  namePointer, nameLength, dataType, unknown1,
                  channelCountPointer, channelCategoryCountPointer,
                  lastUnknownSegmentPointer, unknown2, channelCount, i, flags,
                  unknown6);
        parseSuccess = false;
        break;
      }
      if (record->Name == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 2 channel name, ONID = %hu, TSID = %hu, service ID = %hu, channel ID = %hhu",
                  record->OriginalNetworkId, record->TransportStreamId,
                  record->ServiceId, i);
      }
    }

    //LogDebug(L"MHW: channel, version = 2, ID = %hhu, ONID = %hu, TSID = %hu, service ID = %hu, flags = %hhu, unknown = %hhu, name = %S",
    //          i, record->OriginalNetworkId, record->TransportStreamId,
    //          record->ServiceId, flags, unknown6,
    //          record->Name == NULL ? "" : record->Name);
    unsigned long long dvbChannelId = ((unsigned long long)record->OriginalNetworkId << 32) |
                                      (record->TransportStreamId << 16) |
                                      record->ServiceId;
    m_channelIdLookup[dvbChannelId] = record->Id;
    channels[record->Id] = record;
  }

  unsigned long updateCount = 0;
  if (parseSuccess)
  {
    // Assume there is only one channel (category) section.
    m_recordsChannelCategory.MarkExpiredRecords((2 << 1) | (isTerrestrial ? 1 : 0));

    pointer = channelCategoryCountPointer;
    unsigned char channelCategoryCount = data[pointer++];
    //LogDebug(L"MHW: channel categories, count = %hhu", channelCategoryCount);

    for (unsigned char i = 0; i < channelCategoryCount; i++)
    {
      if (pointer + 2 > dataLength)
      {
        LogDebug(L"MHW: invalid version 2 channel section, pointer = %hu, section length = %hu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel category count = %hhu, category ID = %hhu",
                  pointer, dataLength, dataType, unknown1, channelCountPointer,
                  channelCategoryCountPointer, lastUnknownSegmentPointer,
                  unknown2, channelCount, channelCategoryCount, i);
        parseSuccess = false;
        break;
      }

      unsigned char unknown7 = data[pointer] >> 4;
      unsigned short namePointer = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
      namePointer += 3;     // + 3 for the table ID and section length bytes
      pointer += 2;
      unsigned char categoryChannelCount = data[pointer++];
      if (pointer + categoryChannelCount > dataLength)
      {
        LogDebug(L"MHW: invalid version 2 channel section, pointer = %hu, category channel count = %hhu, section length = %hu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel category count = %hhu, category ID = %hhu, unknown 7 = %hhu",
                  pointer, categoryChannelCount, dataLength, dataType,
                  unknown1, channelCountPointer, channelCategoryCountPointer,
                  lastUnknownSegmentPointer, unknown2, channelCount,
                  channelCategoryCount, i, unknown7);
        parseSuccess = false;
        break;
      }

      if (namePointer >= dataLength)
      {
        LogDebug(L"MHW: invalid version 2 channel section, name pointer = %hu, section length = %hu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel category count = %hhu, category ID = %hhu, unknown 7 = %hhu, category channel count = %hhu",
                  namePointer, dataLength, dataType, unknown1,
                  channelCountPointer, channelCategoryCountPointer,
                  lastUnknownSegmentPointer, unknown2, channelCount,
                  channelCategoryCount, i, unknown7, categoryChannelCount);
        parseSuccess = false;
        break;
      }

      unsigned char nameLength = data[namePointer++];
      unsigned char unknown8 = nameLength >> 6;
      nameLength &= 0x3f;
      if (namePointer + nameLength > dataLength)
      {
        LogDebug(L"MHW: invalid version 2 channel section, name pointer = %hu, name length = %hhu, section length = %hu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel category count = %hhu, category ID = %hhu, unknown 7 = %hhu, category channel count = %hhu, unknown 8 = %hhu",
                  namePointer, nameLength, dataLength, dataType, unknown1,
                  channelCountPointer, channelCategoryCountPointer,
                  lastUnknownSegmentPointer, unknown2, channelCount,
                  channelCategoryCount, i, unknown7, categoryChannelCount,
                  unknown8);
        parseSuccess = false;
        break;
      }

      CRecordMhwChannelCategory* record = new CRecordMhwChannelCategory();
      if (record == NULL)
      {
        LogDebug(L"MHW: failed to allocate version 2 channel category record");
        parseSuccess = false;
        break;
      }
      record->Version = 2;
      record->Id = i;
      record->IsTerrestrial = isTerrestrial;

      if (nameLength > 0)
      {
        if (!CTextUtil::MhwTextToString(&data[namePointer],
                                        nameLength,
                                        m_provider,
                                        &(record->Name)))
        {
          LogDebug(L"MHW: invalid version 2 channel section, name pointer = %hu, name length = %hhu, data type = %hhu, unknown 1 = %hhu, channel count pointer = %hu, channel category count pointer = %hu, last unknown segment pointer = %hu, unknown 2 = %hu, channel count = %hhu, channel category count = %hhu, category ID = %hhu, unknown 7 = %hhu, category channel count = %hhu, unknown 8 = %hhu",
                    namePointer, nameLength, dataType, unknown1,
                    channelCountPointer, channelCategoryCountPointer,
                    lastUnknownSegmentPointer, unknown2, channelCount,
                    channelCategoryCount, i, unknown7, categoryChannelCount,
                    unknown8);
          delete record;
          parseSuccess = false;
          break;
        }
        if (record->Name == NULL)
        {
          LogDebug(L"MHW: failed to allocate version 2 channel category name, category ID = %hhu",
                    i);
        }
      }

      //LogDebug(L"MHW: channel category, version = 2, ID = %hhu, unknown 7 = %hhu, channel count = %hhu, name = %S",
      //          i, unknown7, categoryChannelCount,
      //          record->Name == NULL ? "" : record->Name);
      if (m_recordsChannelCategory.AddOrUpdateRecord((IRecord**)&record, NULL))
      {
        updateCount++;
      }

      for (unsigned char c = 0; c < categoryChannelCount; c++)
      {
        unsigned char channelId = data[pointer++];
        //LogDebug(L"  channel ID = %hhu", channelId);
        map<unsigned char, CRecordMhwChannel*>::iterator channelIt = channels.find(channelId);
        if (channelIt == channels.end())
        {
          LogDebug(L"MHW: version 2 channel category refers to channel that doesn't exist, channel category ID = %hhu, channel ID = %hhu, index = %hhu",
                    i, channelId, c);
        }
        else
        {
          CRecordMhwChannel* recordChannel = channelIt->second;
          recordChannel->CategoryIds.push_back(i);
        }
      }
    }

    updateCount += m_recordsChannelCategory.RemoveExpiredRecords(NULL);
  }

  // Assume there is only one channel section.
  if (parseSuccess)
  {
    m_recordsChannel.MarkExpiredRecords((2 << 1) | (isTerrestrial ? 1 : 0));
  }

  map<unsigned char, CRecordMhwChannel*>::iterator channelIt = channels.begin();
  for ( ; channelIt != channels.end(); channelIt++)
  {
    CRecordMhwChannel* record = channelIt->second;
    if (record != NULL)
    {
      if (!parseSuccess)
      {
        delete record;
      }
      else if (m_recordsChannel.AddOrUpdateRecord((IRecord**)&record, NULL))
      {
        updateCount++;
      }
      channelIt->second = NULL;
    }
  }
  channels.clear();

  if (parseSuccess)
  {
    updateCount += m_recordsChannel.RemoveExpiredRecords(NULL);
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion2DescriptionSection(unsigned char* data,
                                                            unsigned short dataLength)
{
  // table ID [0x96] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 1
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // description ID - 2 bytes
  // reserved - 2 bits
  // version number - 5 bits
  // current/next indicator - 1 bit
  // section number - 1 byte
  // last section number - 1 byte
  // [unknown] - 2 bytes
  //   most significant BCD digit: almost always 0, rarely 1 or 5
  //   all values used, but low values exponentially more common than high values
  //   all values used
  //   least significant BCD digit: always 0xf
  // theme ID - 5 bits
  // sub-theme ID - 6 bits
  // classification - 3 bits; 0 = unclassified / +12, 1 = TP / +16, 2 = +13 / TP/INF, 3 = +18, 4 = X, 5 = SC, 6 = +7, 7 = INF
  // [unknown] - 2 bits, always seems to be 0x3
  // theme description ID - 2 bytes
  // description length - 1 byte
  // description - [description length] bytes
  // [unknown] - 4 bits, always seems to be 0xf
  // line count - 4 bits
  // lines...
  //   line length - 1 byte
  //   line - [line length] bytes
  // [unknown] - 1 byte, always seems to be 0xf0
  // [unknown] - 2 bits, always seems to be 0x3
  // showing count - 6 bits
  // showings...
  //   channel ID - 1 byte
  //   start date - 2 bytes, encoding = MJD
  //   start time - 3 bytes, encoding = BCD
  //   duration - 12 bits, unit = minutes
  //   [unknown] - 4 bits, always seems to be 0x7
  // [unknown] - 1 byte, always seems to be 0xff
  // event count - 1 byte
  // events...
  //   event ID - 2 bytes
  //   [unknown] - 1 byte, values 0x1, 0x2, 0x3, 0x40, 0x41, 0x42, 0x43, 0x50, 0x51, 0x52, 0x53
  //   [unknown] - 1 byte, values 0x1, 0x8, 0x10, 0x18, 0x20, 0x28, 0x30, 0x31, 0x38, 0x39
  // CRC - 4 bytes
  //
  // Note: the data input parameter excludes the CRC.
  if (data == NULL || dataLength < 20)
  {
    LogDebug(L"MHW: invalid version 2 description section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned short descriptionId = (data[3] << 8) | data[4];
  unsigned char versionNumber = (data[5] >> 1) & 0x1f;
  unsigned char sectionNumber = data[6];
  unsigned char lastSectionNumber = data[7];
  unsigned short unknown1 = (data[8] << 8) | data[9];
  unsigned char unknown1a = data[8];
  unsigned char unknown1b = data[9] >> 4;
  unsigned char unknown1c = data[9] & 0xf;    // always f
  unsigned char themeId = data[10] >> 3;
  unsigned char subThemeId = ((data[10] & 7) << 3) | (data[11] >> 5);
  unsigned char classification = (data[11] >> 2) & 7;
  unsigned char unknown2 = data[11] & 3;
  unsigned short themeDescriptionId = (data[12] << 8) | data[13];
  unsigned char descriptionLength = data[14];
  char* description = NULL;
  if (
    15 + descriptionLength + 5 > dataLength ||
    !CTextUtil::MhwTextToString(&data[15], descriptionLength, m_provider, &description)
  )
  {
    LogDebug(L"MHW: invalid version 2 description section, description length = %hhu, section length = %hu, description ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown 1 = %hu, unknown 2 = %hhu",
              descriptionLength, dataLength, descriptionId, versionNumber,
              sectionNumber, lastSectionNumber, unknown1, unknown2);
    return 0;
  }
  if (description == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 2 event description, ID = %hu",
              descriptionId);
    return 0;
  }

  CRecordMhwDescription* record = new CRecordMhwDescription();
  if (record == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 2 description record, ID = %hu",
              descriptionId);
    delete[] description;
    return 0;
  }

  record->Version = 2;
  record->Id = descriptionId;
  record->ThemeId = themeId;
  record->SubThemeId = subThemeId;
  record->ThemeDescriptionId = themeDescriptionId;
  record->Classification = classification;
  record->Description = description;

  unsigned short pointer = 15 + descriptionLength;
  unsigned char lineCount = data[pointer++];
  unsigned char unknown3 = lineCount >> 4;
  lineCount &= 0xf;
  //LogDebug(L"MHW: description, version = 2, ID = %hu, unknown 1a = 0x%hhx, unknown 1b = %hhu, unknown 1c = %hhu, theme ID = %hhu, sub-theme ID = %hhu, classification = %hhu, unknown 2 = %hhu, theme description ID = %hu, line count = %hhu, unknown 3 = %hhu, description = %S",
  //          descriptionId, unknown1a, unknown1b, unknown1c, themeId,
  //          subThemeId, classification, unknown2, themeDescriptionId,
  //          lineCount, unknown3, record->Description);
  for (unsigned char i = 0; i < lineCount; i++)
  {
    if (pointer + 4 > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 description section, pointer = %hu, section length = %hu, description ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown 1 = %hu, unknown 2 = %hhu, description length = %hhu, line count = %hhu, unknown 3 = %hhu, line index = %hhu",
                pointer, dataLength, descriptionId, versionNumber,
                sectionNumber, lastSectionNumber, unknown1, unknown2,
                descriptionLength, lineCount, unknown3, i);
      delete record;
      return 0;
    }

    unsigned char lineLength = data[pointer++];
    char* line = NULL;
    if (
      pointer + lineLength + 4 > dataLength ||
      !CTextUtil::MhwTextToString(&data[pointer], lineLength, m_provider, &line)
    )
    {
      LogDebug(L"MHW: invalid version 2 description section, line length = %hhu, pointer = %hu, section length = %hu, description ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown 1 = %hu, unknown 2 = %hhu, description length = %hhu, line count = %hhu, unknown 3 = %hhu, line index = %hhu",
                lineLength, pointer, dataLength, descriptionId, versionNumber,
                sectionNumber, lastSectionNumber, unknown1, unknown2,
                descriptionLength, lineCount, unknown3, i);
      delete record;
      return 0;
    }

    if (line == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event description line, description ID = %hu, line index = %hhu",
                descriptionId, i);
    }
    else
    {
      //LogDebug(L"  index = %hhu, line = %S", i, line);
      record->Lines.push_back(line);
    }
    pointer += lineLength;
  }

  unsigned char unknown4 = data[pointer++];
  unsigned char showingCount = data[pointer++];
  unsigned char unknown5 = showingCount >> 6;
  showingCount &= 0x3f;
  //LogDebug(L"MHW: description showings, unknown 4 = %hhu, unknown 5 = %hhu, showing count = %hhu",
  //          unknown4, unknown5, showingCount);
  for (unsigned char i = 0; i < showingCount; i++)
  {
    if (pointer + 8 + 2 > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 description section, pointer = %hu, section length = %hu, description ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown 1 = %hu, unknown 2 = %hhu, description length = %hhu, line count = %hhu, unknown 3 = %hhu, unknown 4 = %hhu, unknown 5 = %hhu, showing count = %hhu, showing index = %hhu",
                pointer, dataLength, descriptionId, versionNumber,
                sectionNumber, lastSectionNumber, unknown1, unknown2,
                descriptionLength, lineCount, unknown3, unknown4, unknown5,
                showingCount, i);
      delete record;
      return 0;
    }

    unsigned char channelId = data[pointer++];
    unsigned short startDateMjd = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    unsigned long startTimeBcd = (data[pointer] << 16) | (data[pointer + 1] << 8) | data[pointer + 2];
    pointer += 3;
    unsigned long long startDateTime = CTimeUtils::DecodeMjDateBcdTime(startDateMjd, startTimeBcd);
    unsigned short duration = (data[pointer] << 4) | (data[pointer + 1] >> 4);
    pointer++;
    unsigned char unknown6 = data[pointer++] & 0xf;
    //LogDebug(L"  showing, channel ID = %hhu, start date/time = %llu, duration = %hu m, unknown 6 = %hhu",
    //          channelId, startDateTime, duration, unknown6);
  }

  unsigned char unknown7 = data[pointer++];
  unsigned char eventCount = data[pointer++];
  //LogDebug(L"MHW: description events, unknown 7 = %hhu, event count = %hhu",
  //          unknown7, eventCount);
  for (unsigned char i = 0; i < eventCount; i++)
  {
    if (pointer + 4 > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 description section, pointer = %hu, section length = %hu, description ID = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown 1 = %hu, unknown 2 = %hhu, description length = %hhu, line count = %hhu, unknown 3 = %hhu, unknown 4 = %hhu, unknown 5 = %hhu, showing count = %hhu, unknown 7 = %hhu, event count = %hhu, event index = %hhu",
                pointer, dataLength, descriptionId, versionNumber,
                sectionNumber, lastSectionNumber, unknown1, unknown2,
                descriptionLength, lineCount, unknown3, unknown4, unknown5,
                showingCount, unknown7, eventCount, i);
      delete record;
      return 0;
    }

    unsigned short eventId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    unsigned char unknown8 = data[pointer++];
    unsigned char unknown9 = data[pointer++];
    //LogDebug(L"  event, event ID = %hu, unknown 8 = 0x%hhx, unknown 9 = 0x%hhx",
    //          eventId, unknown8, unknown9);
  }

  if (pointer != dataLength)
  {
    LogDebug(L"MHW: version 2 description section has unexpected trailing bytes, description ID = %hu, pointer = %hu, length = %hu",
              descriptionId, pointer, dataLength);
  }

  // Assumption: records are cycled sequentially.
  unsigned long updateCount = 0;
  if ((long)record->Id < m_previousDescriptionId)
  {
    updateCount = m_recordsDescription.RemoveExpiredRecords(NULL, 2);
    m_recordsDescription.MarkExpiredRecords(2);
  }
  m_previousDescriptionId = record->Id;

  if (m_recordsDescription.AddOrUpdateRecord((IRecord**)&record, NULL))
  {
    updateCount++;
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion2EventsByChannelSection(unsigned char* data,
                                                                unsigned short dataLength,
                                                                bool isTerrestrial)
{
  // table ID [0xdc] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // day bit-mask - 2 bytes; LSB corresponds with today, next bit corresponds with tomorrow etc.
  // section ID - 2 bytes
  // channel ID - 1 byte
  // [unknown] - 2 bytes, always seems to be 0xffff
  // [unknown] - 1 byte; constant in each sample; seen values 0x14, 0x18
  // event count - 1 byte
  // titles...
  //   event ID - 2 bytes
  //   program ID (AKA PRID) - 4 bytes, often 0xffffffff
  //   showing ID (AKA SHID) - 4 bytes; when 0xffffffff, indicates that actual event info is unavailable
  //   start date/time...
  //     date - 2 bytes, encoding = MJD
  //     time - 3 bytes, encoding = BCD
  //   duration - 12 bits, unit = minutes
  //   [unknown] - 3 bits
  //     0  29053
  //     2  263716
  //     3  9800
  //     4  2329
  //     5  70
  //     6  86545
  //   [unknown] - 3 bits, always seems to be 0x7
  //   title length - 6 bits
  //   title - [title length] bytes
  //   description ID - 2 bytes
  //
  // Note: sections containing 0 events have a trailing 00 byte after the event count.
  if (data == NULL || dataLength < 12)
  {
    LogDebug(L"MHW: invalid version 2 events by channel section, is terrestrial = %d, length = %hu",
              isTerrestrial, dataLength);
    return 0;
  }

  unsigned short dayBitMask = (data[3] << 8) | data[4];
  unsigned short sectionId = (data[5] << 8) | data[6];
  unsigned char channelId = data[7];

  unsigned long updateCount = 0;
  unsigned short segment = (2 << 8) | channelId;
  if (!isTerrestrial && segment != m_previousSegmentEventsByChannelSatellite)
  {
    if (m_previousSegmentEventsByChannelSatellite != 0)
    {
      updateCount = m_recordsEventSatellite.RemoveExpiredRecords(NULL,
                                                                  m_previousSegmentEventsByChannelSatellite);
      m_recordsEventSatellite.MarkExpiredRecords(segment);
    }
    m_previousSegmentEventsByChannelSatellite = segment;
  }
  else if (isTerrestrial && segment != m_previousSegmentEventsByChannelTerrestrial)
  {
    if (m_previousSegmentEventsByChannelTerrestrial != 0)
    {
      updateCount = m_recordsEventTerrestrial.RemoveExpiredRecords(NULL,
                                                                    m_previousSegmentEventsByChannelTerrestrial);
      m_recordsEventTerrestrial.MarkExpiredRecords(segment);
    }
    m_previousSegmentEventsByChannelTerrestrial = segment;
  }

  unsigned short unknown1 = (data[8] << 8) | data[9];
  unsigned char unknown2 = data[10];
  unsigned char eventCount = data[11];
  //LogDebug(L"MHW: events by channel section, version = 2, is terrestrial = %d, day bit-mask = 0x%hx, section ID = %hu, channel ID = %hhu, unknown 1 = %hu, unknown 2 = %hhu, event count = %hhu",
  //          isTerrestrial, dayBitMask, sectionId, channelId, unknown1,
  //          unknown2, eventCount);

  unsigned short pointer = 12;
  if (pointer + (20 * eventCount) > dataLength)
  {
    LogDebug(L"MHW: invalid version 2 events by channel section, event count = %hhu, section length = %hu, is terrestrial = %d, day bit-mask = 0x%hx, section ID = %hu, channel ID = %hhu, unknown 1 = %hu, unknown 2 = %hhu, event count = %hhu",
              eventCount, dataLength, isTerrestrial, dayBitMask, sectionId,
              channelId, unknown1, unknown2, eventCount);
    return updateCount;
  }

  for (unsigned char i = 0; i < eventCount; i++)
  {
    if (pointer + 20 > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 events by channel section, pointer = %hu, section length = %hu, is terrestrial = %d, day bit-mask = 0x%hx, section ID = %hu, channel ID = %hhu, unknown 1 = %hu, unknown 2 = %hhu, event count = %hhu, event index = %hhu",
                pointer, dataLength, isTerrestrial, dayBitMask, sectionId,
                channelId, unknown1, unknown2, eventCount, i);
      return updateCount;
    }

    unsigned char titleLength = data[pointer + 17] & 0x3f;
    if (pointer + 20 + titleLength > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 events by channel section, pointer = %hu, title length = %hhu, section length = %hu, is terrestrial = %d, day bit-mask = 0x%hx, section ID = %hu, channel ID = %hhu, unknown 1 = %hu, unknown 2 = %hhu, event count = %hhu, event index = %hhu",
                pointer, titleLength, dataLength, isTerrestrial, dayBitMask,
                sectionId, channelId, unknown1, unknown2, eventCount, i);
      return updateCount;
    }

    CRecordMhwEvent* record = new CRecordMhwEvent();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event record");
      return updateCount;
    }

    record->Version = 2;
    record->Segment = channelId;    // These sections are segmented by channel/day.
    record->ChannelId = channelId;
    record->IsTerrestrial = isTerrestrial;
    record->EventId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    record->ProgramId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
    pointer += 4;
    record->ShowingId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
    pointer += 4;

    unsigned short startDateMjd = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    unsigned long startTimeBcd = (data[pointer] << 16) | (data[pointer + 1] << 8) | data[pointer + 2];
    pointer += 3;
    record->StartDateTime = CTimeUtils::DecodeMjDateBcdTime(startDateMjd, startTimeBcd);

    record->Duration = (data[pointer] << 4) | (data[pointer + 1] >> 4);
    pointer++;
    unsigned char unknown3 = data[pointer++] & 0xf;
    unsigned char unknown4 = data[pointer++] >> 6;

    if (!CTextUtil::MhwTextToString(&data[pointer], titleLength, m_provider, &(record->Title)))
    {
      LogDebug(L"MHW: invalid version 2 events by channel section, pointer = %hu, title length = %hhu, section length = %hu, is terrestrial = %d, day bit-mask = 0x%hx, section ID = %hu, channel ID = %hhu, unknown 1 = %hu, unknown 2 = %hhu, event count = %hhu, event index = %hhu, event ID = %lu, unknown 3 = %hhu, unknown 4 = %hhu",
                pointer, titleLength, dataLength, isTerrestrial, dayBitMask,
                sectionId, channelId, unknown1, unknown2, eventCount, i,
                record->EventId, unknown3, unknown4);
      delete record;
      return updateCount;
    }
    if (record->Title == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event title, event ID = %lu, channel ID = %hhu, is terrestrial = %d",
                record->EventId, record->ChannelId, isTerrestrial);
      delete record;
      return updateCount;
    }
    pointer += titleLength;

    record->DescriptionId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    record->HasDescription = record->DescriptionId != 0xffff;

    //LogDebug(L"MHW: event, version = 2, ID = %lu, channel ID = %hhu, is terrestrial = %d, program ID = %lu, showing ID = %lu, start date/time = %llu, duration = %hu m, unknown 3 = %hhu, unknown 4 = %hhu, description ID = %hu, title = %S",
    //          record->EventId, record->ChannelId, isTerrestrial,
    //          record->ProgramId, record->ShowingId, record->StartDateTime,
    //          record->Duration, unknown3, unknown4, record->DescriptionId,
    //          record->Title);

    if (record->ShowingId == 0xffffffff)
    {
      // Place-holder - actual event information is not available for the
      // associated period.
      delete record;
    }
    else if (!isTerrestrial)
    {
      if (record->EventId == m_previousEventIdSatellite)
      {
        // Events that span days are repeated for each day. Ignore the
        // duplicate instances.
        delete record;
      }
      else
      {
        m_previousEventIdSatellite = record->EventId;
        if (m_recordsEventSatellite.AddOrUpdateRecord((IRecord**)&record, NULL))
        {
          updateCount++;
        }
      }
    }
    else if (isTerrestrial)
    {
      if (record->EventId == m_previousEventIdTerrestrial)
      {
        // Events that span days are repeated for each day. Ignore the
        // duplicate instances.
        delete record;
      }
      else
      {
        m_previousEventIdTerrestrial = record->EventId;
        if (m_recordsEventTerrestrial.AddOrUpdateRecord((IRecord**)&record, NULL))
        {
          updateCount++;
        }
      }
    }
  }

  // Make sure to ignore the trailing 00 byte for sections with 0 events.
  if (pointer != dataLength && (eventCount != 0 || pointer != 12 || dataLength != 13))
  {
    LogDebug(L"MHW: version 2 events by channel section has unexpected trailing bytes, section ID = %hu, pointer = %hu, length = %hu",
              sectionId, pointer, dataLength);
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion2EventsByThemeSection(unsigned char* data,
                                                              unsigned short dataLength)
{
  // table ID [0xe6] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // day bit-mask - 2 bytes; LSB corresponds with today, next bit corresponds with tomorrow etc.
  // section ID - 2 bytes
  // [unknown] - 2 bits, always seems to be 0x3
  // theme ID - 6 bits
  // [unknown] - 1 byte; constant in each sample; seen values 0x14, 0x18
  // [unknown] - 4 bytes, always seems to be 0
  // sub-theme bit mask - 4 bytes; LSB corresponds with sub-theme 0, next bit corresponds with sub-theme 1 etc.
  // event count - 1 byte
  // titles...
  //   channel ID - 1 byte
  //   event ID - 2 bytes
  //   program ID (AKA PRID) - 4 bytes, often 0xffffffff
  //   showing ID (AKA SHID) - 4 bytes; when 0xffffffff, indicates that actual event info is unavailable
  //   start date/time...
  //     date - 2 bytes, encoding = MJD
  //     time - 3 bytes, encoding = BCD
  //   duration - 12 bits, unit = minutes
  //   [unknown] - 3 bits
  //     2  297453
  //     3  12980
  //     4  1549
  //     5  100
  //     6  62343
  //   [unknown] - 3 bits, always seems to be 0x7
  //   title length - 6 bits
  //   title - [title length] bytes
  //   [unknown] - 2 bits, always seems to be 0x3
  //   sub-theme ID - 6 bits
  //   description ID - 2 bytes
  //
  // Note: all fields except table ID, section length, section ID, theme ID,
  // unknown 2 and event count are set to 0xff...f for end-of-theme marker
  // sections.
  if (data == NULL || dataLength < 18)
  {
    LogDebug(L"MHW: invalid version 2 events by theme section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned short dayBitMask = (data[3] << 8) | data[4];
  if (dayBitMask == 0xffff)
  {
    // Marker for end of theme.
    return 0;
  }

  unsigned short sectionId = (data[5] << 8) | data[6];
  unsigned char unknown1 = data[7] >> 6;
  unsigned char themeId = data[7] & 0x3f;
  unsigned short segment = (2 << 8) | themeId;
  unsigned long updateCount = 0;
  if (segment != m_previousSegmentEventsByTheme)
  {
    if (m_previousSegmentEventsByTheme != 0)
    {
      updateCount = m_recordsEventSatellite.RemoveExpiredRecords(NULL,
                                                                  m_previousSegmentEventsByTheme);
      m_recordsEventSatellite.MarkExpiredRecords(segment);
    }
    m_previousSegmentEventsByTheme = segment;
  }

  unsigned char unknown2 = data[8];
  unsigned long unknown3 = (data[9] << 24) | (data[10] << 16) | (data[11] << 8) | data[12];
  unsigned long subThemeBitMask = (data[13] << 24) | (data[14] << 16) | (data[15] << 8) | data[16];
  unsigned char eventCount = data[17];
  LogDebug(L"MHW: events by theme section, version = 2, day bit-mask = 0x%hx, section ID = %hu, unknown 1 = %hhu, theme ID = %hhu, unknown 2 = %hhu, unknown 3 = %lu, sub-theme bit-mask = 0x%lx, event count = %hhu",
            dayBitMask, sectionId, unknown1, themeId, unknown2, unknown3,
            subThemeBitMask, eventCount);

  unsigned short pointer = 18;
  if (pointer + (22 * eventCount) > dataLength)
  {
    LogDebug(L"MHW: invalid version 2 events by theme section, event count = %hhu, section length = %hu, day bit-mask = 0x%hx, section ID = %hu, unknown 1 = %hhu, theme ID = %hhu, unknown 2 = %hhu, unknown 3 = %lu, sub-theme bit-mask = 0x%lx",
              eventCount, dataLength, dayBitMask, sectionId, unknown1, themeId,
              unknown2, unknown3, subThemeBitMask);
    return 0;
  }

  for (unsigned char i = 0; i < eventCount; i++)
  {
    if (pointer + 22 > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 events by theme section, pointer = %hu, section length = %hu, day bit-mask = 0x%hx, section ID = %hu, unknown 1 = %hhu, theme ID = %hhu, unknown 2 = %hhu, unknown 3 = %lu, sub-theme bit mask = 0x%lx, event count = %hhu, event index = %hhu",
                pointer, dataLength, dayBitMask, sectionId, unknown1, themeId,
                unknown2, unknown3, subThemeBitMask, eventCount, i);
      return updateCount;
    }

    unsigned char titleLength = data[pointer + 18] & 0x3f;
    if (pointer + 22 + titleLength > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 events by theme section, pointer = %hu, title length = %hhu, section length = %hu, day bit-mask = 0x%hx, section ID = %hu, unknown 1 = %hhu, theme ID = %hhu, unknown 2 = %hhu, unknown 3 = %lu, sub-theme bit mask = 0x%lx, event count = %hhu, event index = %hhu",
                pointer, titleLength, dataLength, dayBitMask, sectionId,
                unknown1, themeId, unknown2, unknown3, subThemeBitMask,
                eventCount, i);
      return updateCount;
    }

    CRecordMhwEvent* record = new CRecordMhwEvent();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event record");
      return updateCount;
    }

    record->Version = 2;
    record->Segment = themeId;      // These sections are segmented by theme/day.
    record->ThemeId = themeId;
    record->ChannelId = data[pointer++];
    record->EventId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    record->ProgramId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
    pointer += 4;
    record->ShowingId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
    pointer += 4;

    unsigned short startDateMjd = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    unsigned long startTimeBcd = (data[pointer] << 16) | (data[pointer + 1] << 8) | data[pointer + 2];
    pointer += 3;
    record->StartDateTime = CTimeUtils::DecodeMjDateBcdTime(startDateMjd, startTimeBcd);

    record->Duration = (data[pointer] << 4) | (data[pointer + 1] >> 4);
    pointer++;
    unsigned char unknown4 = data[pointer++] & 0xf;
    unsigned char unknown5 = data[pointer++] >> 6;

    if (!CTextUtil::MhwTextToString(&data[pointer], titleLength, m_provider, &(record->Title)))
    {
      LogDebug(L"MHW: invalid version 2 events by theme section, pointer = %hu, title length = %hhu, section length = %hu, day bit-mask = 0x%hx, section ID = %hu, unknown 1 = %hhu, theme ID = %hhu, unknown 2 = %hhu, unknown 3 = %lu, sub-theme bit mask = 0x%lx, event count = %hhu, event index = %hhu, event ID = %lu, unknown 4 = %hhu, unknown 5 = %hhu",
                pointer, titleLength, dataLength, dayBitMask, sectionId,
                unknown1, themeId, unknown2, unknown3, subThemeBitMask,
                eventCount, i, record->EventId, unknown4, unknown5);
      delete record;
      return updateCount;
    }
    if (record->Title == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 event title, event ID = %lu, channel ID = %hhu",
                record->EventId, record->ChannelId);
      delete record;
      return updateCount;
    }
    pointer += titleLength;

    record->SubThemeId = data[pointer++];
    unsigned char unknown6 = record->SubThemeId >> 6;
    record->SubThemeId &= 0x3f;

    record->DescriptionId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    record->HasDescription = record->DescriptionId != 0xffff;

    LogDebug(L"MHW: event, version = 2, ID = %lu, channel ID = %hhu, program ID = %lu, showing ID = %lu, start date/time = %llu, duration = %hu m, unknown 4 = %hhu, unknown 5 = %hhu, theme ID = %hhu, unknown 6 = %hhu, sub-theme ID = %hhu, description ID = %hu, title = %S",
              record->EventId, record->ChannelId, record->ProgramId,
              record->ShowingId, record->StartDateTime, record->Duration,
              unknown4, unknown5, record->ThemeId, unknown6,
              record->SubThemeId, record->DescriptionId, record->Title);
    if (record->ShowingId == 0xffffffff)
    {
      // Place-holder - actual event information is not available for the
      // associated period.
      delete record;
    }
    else if (m_recordsEventSatellite.AddOrUpdateRecord((IRecord**)&record, NULL))
    {
      updateCount++;
    }
  }

  if (pointer != dataLength)
  {
    LogDebug(L"MHW: version 2 events by theme section has unexpected trailing bytes, section ID = %hu, pointer = %hu, length = %hu",
              sectionId, pointer, dataLength);
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion2ProgramSection(unsigned char* data,
                                                        unsigned short dataLength)
{
  // table ID [0x96] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // section ID - 2 bytes
  // program ID (AKA PRID) - 4 bytes
  // [unknown] - 2 bytes, always seems to be 0
  // [unknown] - 4 bits, always seems to be 0xf
  // loop length - 12 bits
  // loop...
  //   tag - 1 byte
  //   length - 2 bytes
  //   content - [length] bytes
  // CRC - 4 bytes
  //
  // tag 1
  // ------
  // theme description ID - 2 bytes
  // classification - 1 byte; 0 = unclassified, 1 = TP, 2 = +13, 3 = +18, 4 = X, 5 = SC, 6 = +7, 7 = INF, 8 = +12, 9 = +16, 10 = TP/INF
  // title length - 1 byte
  // title - [title length] bytes
  // [unknown] - 4 bits, always seems to be 0xf
  // description length - 12 bits
  // description - [description length] bytes
  //
  // tag 2
  // ------
  // loop...
  //   series ID (AKA MSRID) - 4 bytes
  //
  // Note: usually there's only one series ID. A second may be present when the
  // episode/program is suggested/recommended (ie. Canal Propone).
  //
  // tag 5
  // ------
  // loop...
  //   showing ID (AKA SHID) - 4 bytes
  //   original network ID - 2 bytes
  //   transport stream ID - 2 bytes
  //   service ID - 2 bytes
  //   start date/time...
  //     date - 2 bytes, encoding = MJD
  //     time - 3 bytes, encoding = BCD
  //   [unknown] - 4 bits, always seems to be either 0xe or 0xf
  //   duration - 12 bits, unit = minutes
  //   [unknown] - 1 byte
  //   [unknown] - 1 byte
  //
  // Note: the last two unknowns match the values of the two unknowns in the
  // description section event loop.
  if (data == NULL || dataLength < 17)
  {
    LogDebug(L"MHW: invalid version 2 program section, length = %hu",
              dataLength);
    return 0;
  }

  dataLength -= 4;  // exclude the CRC

  // Assumption: records are cycled sequentially.
  unsigned short sectionId = (data[3] << 8) | data[4];
  unsigned long updateCount = 0;
  if ((long)sectionId < m_previousProgramSectionId)
  {
    updateCount = m_recordsProgram.RemoveExpiredRecords(NULL);
    updateCount += m_recordsShowing.RemoveExpiredRecords(NULL);
    m_recordsProgram.MarkExpiredRecords(2);
    m_recordsShowing.MarkExpiredRecords(2);
  }
  m_previousProgramSectionId = sectionId;

  unsigned long programId = (data[5] << 24) | (data[6] << 16) | (data[7] << 8) | data[8];

  CRecordMhwProgram* record = new CRecordMhwProgram();
  if (record == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 2 program record, ID = %lu",
              programId);
    return 0;
  }

  record->Version = 2;
  record->ProgramId = programId;

  unsigned short unknown1 = (data[9] << 8) | data[10];
  unsigned char unknown2 = data[11] >> 4;
  unsigned short loopLength = ((data[11] & 0xf) << 8) | data[12];

  //LogDebug(L"MHW: program section, version = 2, section ID = %hu, program ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu",
  //          sectionId, programId, unknown1, unknown2, loopLength);

  unsigned short pointer = 13;
  if (pointer + loopLength != dataLength)
  {
    LogDebug(L"MHW: invalid version 2 program section, loop length = %hu, section length = %hu, section ID = %hu, program ID = %lu, unknown 1 = %hu, unknown 2 = %hhu",
              loopLength, dataLength, sectionId, programId, unknown1,
              unknown2);
    delete record;
    return 0;
  }

  while (pointer + 2 < dataLength)
  {
    unsigned char tag = data[pointer++];
    unsigned short length = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    //LogDebug(L"MHW: program tag = %hhu, length = %hu", tag, length);
    unsigned short endOfTag = pointer + length;
    if (endOfTag > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 program section, pointer = %hu, tag length = %hu, section length = %hu, section ID = %hu, program ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu, tag = %hhu",
                pointer, length, dataLength, sectionId, programId, unknown1,
                unknown2, loopLength, tag);
      delete record;
      return updateCount;
    }
    else if (
      (tag == 1 && length < 6) ||
      (tag == 2 && (length == 0 || length % 4 != 0)) ||
      (tag == 5 && (length == 0 || length % 19 != 0))
    )
    {
      LogDebug(L"MHW: invalid version 2 program section tag, tag = %hhu, tag length = %hu, pointer = %hu, section length = %hu, section ID = %hu, program ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu",
                tag, length, pointer, dataLength, sectionId, programId,
                unknown1, unknown2, loopLength);
      delete record;
      return updateCount;
    }

    if (tag == 1)
    {
      record->ThemeDescriptionId = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      record->Classification = data[pointer++];

      unsigned char titleLength = data[pointer++];
      if (titleLength > 0)
      {
        if (
          pointer + titleLength > endOfTag ||
          !CTextUtil::MhwTextToString(&data[pointer], titleLength, m_provider, &(record->Title))
        )
        {
          LogDebug(L"MHW: invalid version 2 program section tag 1, title length = %hhu, pointer = %hu, end of tag = %hu, section length = %hu, section ID = %hu, program ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu",
                    titleLength, pointer, endOfTag, dataLength, sectionId,
                    programId, unknown1, unknown2, loopLength);
          delete record;
          return updateCount;
        }
        if (record->Title == NULL)
        {
          LogDebug(L"MHW: failed to allocate version 2 program section tag 1 title, program ID = %hu",
                    programId);
        }

        pointer += titleLength;
      }

      unsigned char unknown3 = data[pointer] >> 4;
      unsigned short descriptionLength = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
      pointer += 2;
      if (descriptionLength > 0)
      {
        if (
          pointer + descriptionLength > endOfTag ||
          !CTextUtil::MhwTextToString(&data[pointer],
                                      descriptionLength,
                                      m_provider,
                                      &(record->Description))
        )
        {
          LogDebug(L"MHW: invalid version 2 program section tag 1, description length = %hu, pointer = %hu, end of tag = %hu, section length = %hu, section ID = %hu, program ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu, title length = %hhu, unknown 3 = %hhu",
                    descriptionLength, pointer, endOfTag, dataLength,
                    sectionId, programId, unknown1, unknown2, loopLength,
                    titleLength, unknown3);
          delete record;
          return updateCount;
        }
        if (record->Description == NULL)
        {
          LogDebug(L"MHW: failed to allocate version 2 program section tag 1 description, program ID = %hu",
                    programId);
        }

        pointer += descriptionLength;
      }

      //LogDebug(L"MHW: program section tag 1, theme description ID = %hu, classification = %hhu, unknown 3 = %hhu, title = %S, description = %S",
      //          record->ThemeDescriptionId, record->Classification, unknown3,
      //          record->Title == NULL ? "" : record->Title,
      //          record->Description == NULL ? "" : record->Description);
    }
    else if (tag == 2)
    {
      // Assumption: real series ID will be first, and other IDs (Canal Propone etc.) will follow.
      bool isFirst = true;
      while (pointer + 4 <= endOfTag)
      {
        unsigned long seriesId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        if (isFirst)
        {
          record->SeriesId = seriesId;
        }
        pointer += 4;
        //LogDebug(L"MHW: program section tag 2, series ID = %lu", seriesId);
      }
    }
    else if (tag == 5)
    {
      while (pointer + 19 <= endOfTag)
      {
        unsigned long showingId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;
        record->ShowingIds.push_back(showingId);

        CRecordMhwProgramShowing* recordShowing = new CRecordMhwProgramShowing();
        if (recordShowing == NULL)
        {
          LogDebug(L"MHW: failed to allocate version 2 program showing record, program ID = %lu, showing ID = %lu",
                    programId, showingId);
        }
        else
        {
          recordShowing->Version = 2;
          recordShowing->ShowingId = showingId;
          recordShowing->OriginalNetworkId = (data[pointer] << 8) | data[pointer + 1];
          pointer += 2;
          recordShowing->TransportStreamId = (data[pointer] << 8) | data[pointer + 1];
          pointer += 2;
          recordShowing->ServiceId = (data[pointer] << 8) | data[pointer + 1];
          pointer += 2;
          unsigned short startDateMjd = (data[pointer] << 8) | data[pointer + 1];
          pointer += 2;
          unsigned long startTimeBcd = (data[pointer] << 16) | (data[pointer + 1] << 8) | data[pointer + 2];
          pointer += 3;
          recordShowing->StartDateTime = CTimeUtils::DecodeMjDateBcdTime(startDateMjd, startTimeBcd);
          unsigned char unknown4 = data[pointer] >> 4;
          recordShowing->Duration = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
          pointer += 2;
          unsigned char unknown5 = data[pointer++];
          unsigned char unknown6 = data[pointer++];

          //LogDebug(L"MHW: program section tag 5, showing ID = %lu, ONID = %hu, TSID = %hu, service ID = %hu, start date/time = %llu, unknown 4 = %hhu, duration = %hu m, unknown 5 = %hhu, unknown 6 = %hhu",
          //          showingId, recordShowing->OriginalNetworkId,
          //          recordShowing->TransportStreamId, recordShowing->ServiceId,
          //          recordShowing->StartDateTime, unknown4,
          //          recordShowing->Duration, unknown5, unknown6);

          if (m_recordsShowing.AddOrUpdateRecord((IRecord**)&recordShowing, NULL))
          {
            updateCount++;
          }
        }
      }
    }
    else
    {
      LogDebug(L"MHW: unsupported program section tag, tag = %hhu, length = %hu",
                tag, length);
      pointer += length;
    }
  }

  if (pointer != dataLength)
  {
    LogDebug(L"MHW: version 2 program section has unexpected trailing bytes, section ID = %hu, pointer = %hu, length = %hu",
              sectionId, pointer, dataLength);
  }

  if (m_recordsProgram.AddOrUpdateRecord((IRecord**)&record, NULL))
  {
    updateCount++;
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion2SeriesSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0x97] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // section ID - 2 bytes
  // series ID (AKA MSRID) - 4 bytes
  // [unknown] - 2 bytes, always seems to be 0
  // [unknown] - 4 bits, always seems to be 0xf
  // loop length - 12 bits
  // loop...
  //   tag - 1 byte
  //   length - 2 bytes
  //   content - [length] bytes
  // CRC - 4 bytes
  //
  // tag 7
  // ------
  // series name length - 1 byte
  // series name - [series name length] bytes (eg. The Simpsons)
  // season name length - 1 byte
  // season name - [season name length] bytes
  // flags - 1 byte
  //   0xff
  //   0xfd
  //   0xfc = suggestion/recommendation (Canal+ Propone)
  // [unknown] - 4 bits, always seems to be 0xf
  // season description length - 12 bits
  // season description - [season description length] bytes
  //
  // tag 8
  // ------
  // loop...
  //   program ID (AKA PRID) - 4 bytes
  //   episode number - 2 bytes; 0xffff when not applicable
  if (data == NULL || dataLength < 17)
  {
    LogDebug(L"MHW: invalid version 2 series section, length = %hu",
              dataLength);
    return 0;
  }

  dataLength -= 4;  // exclude the CRC

  // Assumption: records are cycled sequentially.
  unsigned short sectionId = (data[3] << 8) | data[4];
  unsigned long updateCount = 0;
  if ((long)sectionId < m_previousSeriesSectionId)
  {
    updateCount = m_recordsSeries.RemoveExpiredRecords(NULL);
    m_recordsSeries.MarkExpiredRecords(2);
  }
  m_previousSeriesSectionId = sectionId;

  unsigned long seriesId = (data[5] << 24) | (data[6] << 16) | (data[7] << 8) | data[8];

  CRecordMhwSeries* record = new CRecordMhwSeries();
  if (record == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 2 series record, ID = %lu",
              seriesId);
    return 0;
  }

  record->Version = 2;
  record->Id = seriesId;

  unsigned short unknown1 = (data[9] << 8) | data[10];
  unsigned char unknown2 = data[11] >> 4;
  unsigned short loopLength = ((data[11] & 0xf) << 8) | data[12];
  //LogDebug(L"MHW: series section, version = 2, section ID = %hu, series ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu",
  //          sectionId, seriesId, unknown1, unknown2, loopLength);

  unsigned short pointer = 13;
  if (pointer + loopLength != dataLength)
  {
    LogDebug(L"MHW: invalid version 2 series section, loop length = %hu, section length = %hu, section ID = %hu, series ID = %lu, unknown 1 = %hu, unknown 2 = %hhu",
              loopLength, dataLength, sectionId, seriesId, unknown1, unknown2);
    delete record;
    return 0;
  }

  while (pointer + 2 < dataLength)
  {
    unsigned char tag = data[pointer++];
    unsigned short length = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    //LogDebug(L"MHW: series tag = %hhu, length = %hu", tag, length);
    unsigned short endOfTag = pointer + length;
    if (endOfTag > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 series section, pointer = %hu, tag length = %hu, section length = %hu, section ID = %hu, series ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu, tag = %hhu",
                pointer, length, dataLength, sectionId, seriesId, unknown1,
                unknown2, loopLength, tag);
      delete record;
      return 0;
    }
    else if (
      (tag == 7 && length < 5) ||
      (tag == 8 && (length == 0 || length % 6 != 0))
    )
    {
      LogDebug(L"MHW: invalid version 2 series section tag, tag = %hhu, tag length = %hu, pointer = %hu, section length = %hu, section ID = %hu, series ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu",
                tag, length, pointer, dataLength, sectionId, seriesId,
                unknown1, unknown2, loopLength);
      delete record;
      return 0;
    }

    if (tag == 7)
    {
      unsigned char seriesNameLength = data[pointer++];
      if (seriesNameLength > 0)
      {
        if (
          pointer + seriesNameLength > endOfTag ||
          !CTextUtil::MhwTextToString(&data[pointer],
                                      seriesNameLength,
                                      m_provider,
                                      &(record->SeriesName))
        )
        {
          LogDebug(L"MHW: invalid version 2 series section tag 7, series name length = %hhu, pointer = %hu, end of tag = %hu, section length = %hu, section ID = %hu, series ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu",
                    seriesNameLength, pointer, endOfTag, dataLength, sectionId,
                    seriesId, unknown1, unknown2, loopLength);
          delete record;
          return 0;
        }
        if (record->SeriesName == NULL)
        {
          LogDebug(L"MHW: failed to allocate version 2 series section tag 7 series name, series ID = %lu",
                    seriesId);
        }

        pointer += seriesNameLength;
      }

      unsigned char seasonNameLength = data[pointer++];
      if (seasonNameLength > 0)
      {
        if (
          pointer + seasonNameLength > endOfTag ||
          !CTextUtil::MhwTextToString(&data[pointer],
                                      seasonNameLength,
                                      m_provider,
                                      &(record->SeasonName))
        )
        {
          LogDebug(L"MHW: invalid version 2 series section tag 7, season name length = %hhu, pointer = %hu, end of tag = %hu, section length = %hu, section ID = %hu, series ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu, series name length = %hhu",
                    seasonNameLength, pointer, endOfTag, dataLength, sectionId,
                    seriesId, unknown1, unknown2, loopLength,
                    seriesNameLength);
          delete record;
          return 0;
        }
        if (record->SeasonName == NULL)
        {
          LogDebug(L"MHW: failed to allocate version 2 series section tag 7 season name, series ID = %lu",
                    seriesId);
        }

        pointer += seasonNameLength;
      }

      unsigned char flags = data[pointer++];
      unsigned char unknown4 = data[pointer] >> 4;
      unsigned short seasonDescriptionLength = ((data[pointer] & 0xf) << 8) | data[pointer + 1];
      pointer += 2;
      if (seasonDescriptionLength > 0)
      {
        if (
          pointer + seasonDescriptionLength > endOfTag ||
          !CTextUtil::MhwTextToString(&data[pointer],
                                      seasonDescriptionLength,
                                      m_provider,
                                      &(record->SeasonDescription))
        )
        {
          LogDebug(L"MHW: invalid version 2 series section tag 7, season description length = %hu, pointer = %hu, end of tag = %hu, section length = %hu, section ID = %hu, series ID = %lu, unknown 1 = %hu, unknown 2 = %hhu, loop length = %hu, series name length = %hhu, season name length = %hhu",
                    seasonDescriptionLength, pointer, endOfTag, dataLength,
                    sectionId, seriesId, unknown1, unknown2, loopLength,
                    seriesNameLength, seasonNameLength);
          delete record;
          return 0;
        }
        if (record->SeasonDescription == NULL)
        {
          LogDebug(L"MHW: failed to allocate version 2 series section tag 7 season description, series ID = %lu",
                    seriesId);
        }

        pointer += seasonDescriptionLength;
      }

      //LogDebug(L"MHW: series section tag 7, unknown 3 = %hhu, unknown 4 = %hhu, series name = %S, season name = %S, season description = %S",
      //          flags, unknown4,
      //          record->SeriesName == NULL ? "" : record->SeriesName,
      //          record->SeasonName == NULL ? "" : record->SeasonName,
      //          record->SeasonDescription == NULL ? "" : record->SeasonDescription);
    }
    else if (tag == 8)
    {
      while (pointer + 6 <= endOfTag)
      {
        unsigned long programId = (data[pointer] << 24) | (data[pointer + 1] << 16) | (data[pointer + 2] << 8) | data[pointer + 3];
        pointer += 4;
        unsigned short episodeNumber = (data[pointer] << 8) | data[pointer + 1];
        pointer += 2;
        //LogDebug(L"MHW: series section tag 8, program ID = %lu, episode number = %hu",
        //          programId, episodeNumber);

        record->EpisodeNumbers[programId] = episodeNumber;
      }
    }
    else
    {
      LogDebug(L"MHW: unsupported series section tag, tag = %hhu, length = %hu",
                tag, length);
      pointer += length;
    }
  }

  if (pointer != dataLength)
  {
    LogDebug(L"MHW: version 2 series section has unexpected trailing bytes, section ID = %hu, pointer = %hu, length = %hu",
              sectionId, pointer, dataLength);
  }

  if (m_recordsSeries.AddOrUpdateRecord((IRecord**)&record, NULL))
  {
    updateCount++;
  }
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion2ThemeSection(unsigned char* data,
                                                      unsigned short dataLength)
{
  // table ID [0xc8] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 0
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // data type - 1 byte, 0 = standard definition channels, 1 = themes, 2 = high definition channels, 3 = terrestrial channels
  // theme count - 1 byte
  // theme detail pointers...
  //   pointer 1 - 2 bytes
  // theme details... [offset [pointer 1]]
  //   [unknown] - 2 bits, always seems to be 0x3
  //   sub-theme count - 6 bits
  //   sub-theme name pointers...
  //     pointer 2 - 2 bytes
  // sub-theme name... [offset [pointer 2]]
  //   [unknown] - 3 bits, always seems to be 0x7
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

  unsigned long updateCount = 0;
  unsigned short pointer = 5;
  for (unsigned char themeIndex = 0; themeIndex < themeCount; themeIndex++)
  {
    unsigned short themeDetailPointer = (data[pointer] << 8) | data[pointer + 1];
    themeDetailPointer += 3;    // + 3 for the table ID and section length bytes
    pointer += 2;
    if (themeDetailPointer >= dataLength)
    {
      LogDebug(L"MHW: invalid version 2 theme section, theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, section length = %hu",
                themeDetailPointer, themeCount, themeIndex, dataLength);
      return updateCount;
    }

    unsigned char subThemeCount = data[themeDetailPointer++];
    unsigned char unknown1 = subThemeCount >> 6;
    subThemeCount &= 0x3f;
    if (themeDetailPointer + (2 * (subThemeCount + 1)) > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 theme section, sub-theme count = %hhu, unknown 1 = %hhu, theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, section length = %hu",
                subThemeCount, unknown1, themeDetailPointer, themeCount,
                themeIndex, dataLength);
      return updateCount;
    }

    for (unsigned char subThemeIndex = 0; subThemeIndex <= subThemeCount; subThemeIndex++)
    {
      unsigned short subThemeDetailPointer = (data[themeDetailPointer] << 8) | data[themeDetailPointer + 1];
      subThemeDetailPointer += 3;   // + 3 for the table ID and section length bytes
      themeDetailPointer += 2;
      if (subThemeDetailPointer >= dataLength)
      {
        LogDebug(L"MHW: invalid version 2 theme section, sub-theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, theme detail pointer = %hu, sub-theme count = %hhu, unknown 1 = %hhu, sub-theme index = %hhu, section length = %hu",
                  subThemeDetailPointer, themeCount, themeIndex,
                  themeDetailPointer, subThemeCount, unknown1, subThemeIndex,
                  dataLength);
        return updateCount;
      }

      unsigned char subThemeNameLength = data[subThemeDetailPointer++];
      unsigned char unknown2 = subThemeNameLength >> 5;
      subThemeNameLength &= 0x1f;
      char* name = NULL;
      if (
        subThemeDetailPointer + subThemeNameLength > dataLength ||
        !CTextUtil::MhwTextToString(&data[subThemeDetailPointer],
                                    subThemeNameLength,
                                    m_provider,
                                    &name)
      )
      {
        LogDebug(L"MHW: invalid version 2 theme section, sub-theme name length = %hhu, unknown 2 = %hhu, sub-theme detail pointer = %hu, theme count = %hhu, theme index = %hhu, theme detail pointer = %hu, sub-theme count = %hhu, unknown 1 = %hhu, sub-theme index = %hhu, section length = %hu",
                  subThemeNameLength, unknown2, subThemeDetailPointer,
                  themeCount, themeIndex, themeDetailPointer,
                  subThemeCount, unknown1, subThemeIndex, dataLength);
        return updateCount;
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
        return updateCount;
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
        updateCount++;
      }
    }
  }

  updateCount += m_recordsTheme.RemoveExpiredRecords(NULL);
  return updateCount;
}

unsigned long CParserMhw::DecodeVersion2ThemeDescriptionSection(unsigned char* data,
                                                                unsigned short dataLength)
{
  // table ID [0xdd] - 1 byte
  // section syntax indicator - 1 bit, always seems to be 1
  // private indicator - 1 bit, always seems to be 1
  // reserved - 2 bits, always seems to be 3
  // section length - 12 bits
  // table ID extension - 2 bytes, always seems to be 0
  // reserved - 2 bits
  // version number - 5 bits
  // current/next indicator - 1 bit
  // section number - 1 byte
  // last section number - 1 byte
  // [unknown] - 1 byte, always seems to be 0xff
  // theme description count - 1 byte
  // theme descriptions...
  //   theme description ID - 2 bytes
  //   description length - 1 byte
  //   description - [description length] bytes
  // CRC - 4 bytes
  //
  // Note: the data input parameter excludes the CRC.
  if (data == NULL || dataLength < 10)
  {
    LogDebug(L"MHW: invalid version 2 theme description section, length = %hu",
              dataLength);
    return 0;
  }

  unsigned short tableIdExtension = (data[3] << 8) | data[4];
  unsigned char versionNumber = (data[5] >> 1) & 0x1f;
  unsigned char sectionNumber = data[6];
  unsigned char lastSectionNumber = data[7];
  unsigned char unknown = data[8];
  unsigned char themeDescriptionCount = data[9];

  //LogDebug(L"MHW: theme description section, version = 2, table ID extension = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown = %hhu, theme description count = %hhu",
  //          tableIdExtension, versionNumber, sectionNumber, lastSectionNumber,
  //          unknown, themeDescriptionCount);
  if (dataLength < 10 + (3 * themeDescriptionCount))
  {
    LogDebug(L"MHW: invalid version 2 theme description section, theme description count = %hhu, section length = %hu, table ID extension = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown = %hhu",
              themeDescriptionCount, dataLength, tableIdExtension,
              versionNumber, sectionNumber, lastSectionNumber, unknown);
    return 0;
  }

  // Assume there is only one theme description section.
  m_recordsThemeDescription.MarkExpiredRecords(2);

  unsigned long updateCount = 0;
  unsigned short pointer = 10;
  for (unsigned char i = 0; i < themeDescriptionCount; i++)
  {
    if (pointer + 2 > dataLength)
    {
      LogDebug(L"MHW: invalid version 2 theme description section, pointer = %hu, section length = %hu, table ID extension = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown = %hhu, theme description count = %hhu, theme description index = %hhu",
                pointer, dataLength, tableIdExtension, versionNumber,
                sectionNumber, lastSectionNumber, unknown,
                themeDescriptionCount, i);
      return updateCount;
    }

    unsigned short themeDescriptionId = (data[pointer] << 8) | data[pointer + 1];
    pointer += 2;
    unsigned char descriptionLength = data[pointer++];

    char* description = NULL;
    if (
      pointer + descriptionLength > dataLength ||
      !CTextUtil::MhwTextToString(&data[pointer],
                                  descriptionLength,
                                  m_provider,
                                  &description)
    )
    {
      LogDebug(L"MHW: invalid version 2 theme description section, description length = %hhu, pointer = %hu, section length = %hu, table ID extension = %hu, version number = %hhu, section number = %hhu, last section number = %hhu, unknown = %hhu, theme description count = %hhu, theme description index = %hhu, theme description ID = %hu",
                descriptionLength, pointer, dataLength, tableIdExtension,
                versionNumber, sectionNumber, lastSectionNumber, unknown,
                themeDescriptionCount, i, themeDescriptionId);
      return updateCount;
    }
    if (description == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 theme description, theme description ID = %hu",
                themeDescriptionId);
    }

    pointer += descriptionLength;

    CRecordMhwThemeDescription* record = new CRecordMhwThemeDescription();
    if (record == NULL)
    {
      LogDebug(L"MHW: failed to allocate version 2 theme description record, ID = %hu",
                themeDescriptionId);
      if (description != NULL)
      {
        delete[] description;
      }
      return updateCount;
    }

    record->Version = 2;
    record->Id = themeDescriptionId;
    record->Description = description;

    //LogDebug(L"MHW: theme description, version = 2, ID = %hu, description = %S",
    //          record->Id, description == NULL ? "" : description);
    if (m_recordsThemeDescription.AddOrUpdateRecord((IRecord**)&record, NULL))
    {
      updateCount++;
    }
  }

  if (pointer != dataLength)
  {
    LogDebug(L"MHW: version 2 theme description section has unexpected trailing bytes, pointer = %hu, length = %hu",
              pointer, dataLength);
  }

  updateCount += m_recordsThemeDescription.RemoveExpiredRecords(NULL);
  return updateCount;
}

MhwProvider CParserMhw::DetermineProvider(unsigned short originalNetworkId,
                                          unsigned short transportStreamId)
{
  if (originalNetworkId == 1)
  {
    if (transportStreamId == 1058)
    {
      return MovistarPlusSpain;
    }
    if (transportStreamId == 1098)
    {
      return CanalsatFrance;
    }
  }
  else if (originalNetworkId == 318)
  {
    return CyfraPoland;
  }
  return MhwProviderUnknown;
}

void CParserMhw::CompleteTable(CSection& section,
                                unsigned char** tableBuffer,
                                unsigned short& tableBufferSize,
                                unsigned char& expectedSectionNumber)
{
  // Most tables have only one section. A few have more. Unfortunately the
  // table/section data is not structured in such a way that sections can be
  // parsed independently. Section break(s) may appear absolutely anywhere. The
  // only way to cope is to cache and combine the sections as they're received.
  // Then, when the last section is received, process them together as if they
  // were one section.
  if (section.LastSectionNumber == 0)
  {
    // Don't bother using the buffer if there's only one section in the table.
    return;
  }

  if (section.SectionNumber != expectedSectionNumber)
  {
    if (*tableBuffer != NULL)
    {
      delete[] *tableBuffer;
      *tableBuffer = NULL;
      tableBufferSize = 0;
    }
    expectedSectionNumber = 0;
    return;
  }

  // Cache the section.
  if (*tableBuffer != NULL && section.SectionNumber == 0)
  {
    delete[] *tableBuffer;
    tableBufferSize = 0;
  }

  unsigned short newBufferSize = tableBufferSize;
  unsigned short sectionLength = section.section_length - 9;  // - 9 for the table ID extension, version, section number, last section number and CRC
  if (section.SectionNumber == 0)
  {
    sectionLength = section.section_length - 1;   // + 3 for the table ID and section length, - 4 for the CRC
  }
  newBufferSize += sectionLength;
  
  unsigned char* newBuffer = new unsigned char[newBufferSize];
  if (newBuffer == NULL)
  {
    LogDebug(L"MHW: failed to allocate version 2 table buffer, size = %hu",
              newBufferSize);
    if (*tableBuffer != NULL)
    {
      delete[] *tableBuffer;
      *tableBuffer = NULL;
      tableBufferSize = 0;
    }
    expectedSectionNumber = 0;
    return;
  }

  if (section.SectionNumber == 0)
  {
    memcpy(newBuffer, section.Data, sectionLength);
  }
  else
  {
    memcpy(newBuffer, *tableBuffer, tableBufferSize);
    memcpy(&newBuffer[tableBufferSize], &section.Data[8], sectionLength);
    delete[] *tableBuffer;
  }
  *tableBuffer = newBuffer;
  tableBufferSize = newBufferSize;
  expectedSectionNumber++;
}

void CParserMhw::OnMhwEventRemoved(unsigned char version,
                                    unsigned char segment,
                                    unsigned long eventId,
                                    unsigned long descriptionId,
                                    bool hasDescription,
                                    unsigned char channelId,
                                    unsigned long long startDateTime,
                                    unsigned short duration,
                                    const char* title,
                                    unsigned long payPerViewId,
                                    bool isTerrestrial,
                                    unsigned long programId,
                                    unsigned long showingId,
                                    unsigned char themeId,
                                    unsigned char subThemeId)
{
  if (version == 1 && eventId == m_firstDescriptionId)
  {
    m_firstDescriptionId = 0xffffffff;
  }
}