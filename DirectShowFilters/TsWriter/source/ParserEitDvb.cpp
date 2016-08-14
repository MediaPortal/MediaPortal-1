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
#include "ParserEitDvb.h"
#include <algorithm>    // find()
#include <cstring>      // memcpy(), strcmp(), strlen(), strncpy()
#include <iomanip>
#include <time.h>       // gmtime(), mktime(), time_t, tm
#include <typeinfo>     // bad_cast
#include "..\..\shared\TimeUtils.h"
#include "EnterCriticalSection.h"
#include "ParserBat.h"
#include "ParserNitDvb.h"
#include "ParserSdt.h"
#include "PidUsage.h"
#include "Utils.h"


#define COUNTRY_DEU 0x756564
#define LANG_ENG    0x676e65
#define LANG_GER    0x726567

#define ITEM_INDEX_DISH_EPISODE_INFORMATION     255
#define ITEM_INDEX_PREMIERE_ORDER_NUMBER        254
#define ITEM_INDEX_PREMIERE_ORDER_PRICE         253
#define ITEM_INDEX_PREMIERE_ORDER_PHONE_NUMBER  252
#define ITEM_INDEX_PREMIERE_ORDER_SMS_NUMBER    251
#define ITEM_INDEX_PREMIERE_ORDER_URL           250
#define ITEM_INDEX_PREMIERE_PARENT_TEXT         249

// There is no private data specifier for Dish descriptors, so we have to
// determine scope with ONID. These are the ONIDs for EchoStar networks.
// http://www.dvbservices.com/identifiers/original_network_id&tab=table
#define ORIGINAL_NETWORK_ID_DISH_START          0x1001
#define ORIGINAL_NETWORK_ID_DISH_END            0x100b


extern void LogDebug(const wchar_t* fmt, ...);

CParserEitDvb::CParserEitDvb(ICallBackPidConsumer* callBack,
                              IDefaultAuthorityProvider* authorityProvider,
                              LPUNKNOWN unk,
                              HRESULT* hr)
  : CUnknown(NAME("DVB EPG Grabber"), unk)
{
  if (callBack == NULL)
  {
    LogDebug(L"EIT DVB: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }
  if (authorityProvider == NULL)
  {
    LogDebug(L"EIT DVB: authority provider not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_grabFreesat = false;
  m_freesatPidBat = 0;
  m_freesatPidEitPf = 0;
  m_freesatPidEitSchedule = 0;
  m_freesatPidNit = 0;
  m_freesatPidPmt = 0;
  m_freesatPidSdt = 0;
  m_isSeen = false;
  m_isReady = false;
  m_completeTime = 0;
  m_unseenTableCount = 0;
  m_unseenSegmentCount = 0;
  m_unseenSectionCount = 0;

  m_callBackGrabber = NULL;
  m_callBackPidConsumer = callBack;
  m_defaultAuthorityProvider = authorityProvider;
  m_enableCrcCheck = true;

  m_currentService = NULL;
  m_currentServiceIndex = 0xffff;
  m_currentEvent = NULL;
  m_currentEventIndex = 0;
  m_currentEventText = NULL;
  m_currentEventTextIndex = 0;
  m_referenceEvent = NULL;
  m_referenceServiceId = 0;
  m_referenceEventId = 0;
}

CParserEitDvb::~CParserEitDvb()
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = NULL;
  m_callBackPidConsumer = NULL;
  m_defaultAuthorityProvider = NULL;

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

  map<unsigned long long, CRecordEitService*>::iterator serviceIt = m_services.begin();
  for ( ; serviceIt != m_services.end(); serviceIt++)
  {
    if (serviceIt->second != NULL)
    {
      delete serviceIt->second;
      serviceIt->second = NULL;
    }
  }
  m_services.clear();
}

STDMETHODIMP CParserEitDvb::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  if (ppv == NULL)
  {
    return E_INVALIDARG;
  }

  if (iid == IID_IGRABBER)
  {
    return GetInterface((IGrabber*)this, ppv);
  }
  if (iid == IID_IGRABBER_EPG_DVB)
  {
    return GetInterface((IGrabberEpgDvb*)this, ppv);
  }
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

void CParserEitDvb::SetFreesatPmtPid(unsigned short pid)
{
  LogDebug(L"EIT DVB: set Freesat PMT PID, PID = %hu", pid);
  CEnterCriticalSection lock(m_section);
  if (m_grabFreesat && m_callBackPidConsumer != NULL)
  {
    if (m_freesatPidPmt != 0)
    {
      m_callBackPidConsumer->OnPidsNotRequired(&m_freesatPidPmt, 1, Epg);
    }
    if (pid != 0)
    {
      m_callBackPidConsumer->OnPidsRequired(&pid, 1, Epg);
    }
  }
  m_freesatPidPmt = pid;
}

void CParserEitDvb::SetFreesatPids(unsigned short pidBat,
                                    unsigned short pidEitPf,
                                    unsigned short pidEitSchedule,
                                    unsigned short pidNit,
                                    unsigned short pidSdt)
{
  LogDebug(L"EIT DVB: set Freesat PIDs, BAT = %hu, EIT P/F = %hu, EIT schedule = %hu, NIT = %hu, SDT = %hu",
            pidBat, pidEitPf, pidEitSchedule, pidNit, pidSdt);
  CEnterCriticalSection lock(m_section);
  ResetFreesatGrabState();

  unsigned short pids[5];
  unsigned char pidCount = 0;
  if (pidEitPf != 0 && AddOrResetDecoder(pidEitPf, m_enableCrcCheck))
  {
    m_freesatPidEitPf = pidEitPf;
    m_grabPids[pidEitPf] = m_grabFreesat;
    pids[pidCount++] = pidEitPf;
  }
  if (
    pidEitSchedule != 0 &&
    pidEitSchedule != pidEitPf &&
    AddOrResetDecoder(pidEitSchedule, m_enableCrcCheck)
  )
  {
    m_freesatPidEitSchedule = pidEitSchedule;
    m_grabPids[pidEitSchedule] = m_grabFreesat;
    pids[pidCount++] = pidEitSchedule;
  }

  // BAT, SDT and NIT are needed to get the default authority, which is a
  // prefix for some series and episode CRIDs.
  // Note: BAT and SDT are usually carried on the same PID.
  if (pidBat != 0 && pidBat != pidEitPf && pidBat != pidEitSchedule)
  {
    m_freesatPidBat = pidBat;
    pids[pidCount++] = pidBat;
  }
  if (pidNit != 0 && pidNit != pidBat && pidNit != pidEitPf && pidNit != pidEitSchedule)
  {
    m_freesatPidNit = pidNit;
    pids[pidCount++] = pidNit;
  }
  if (
    pidSdt != 0 &&
    pidSdt != pidBat &&
    pidSdt != pidEitPf &&
    pidSdt != pidEitSchedule &&
    pidSdt != pidNit
  )
  {
    m_freesatPidSdt = pidSdt;
    pids[pidCount++] = pidSdt;
  }

  if (m_grabFreesat && m_callBackPidConsumer != NULL && pidCount > 0)
  {
    m_callBackPidConsumer->OnPidsRequired(pids, pidCount, Epg);
  }

  // Ignore DVB EIT when Freesat EPG has been detected and is being grabbed.
  // Any available DVB EIT data comes from Sky, and will conflict with the
  // Freesat data (since Sky use a different SI system - OpenTV). We prefer
  // the Freesat data, which seems to be richer.
  if (
    m_grabFreesat &&
    (m_freesatPidEitPf > 0 || m_freesatPidEitSchedule > 0) &&
    m_grabPids[PID_EIT_DVB]
  )
  {
    LogDebug(L"EIT DVB: discard conflicting Sky EIT");
    PrivateReset(false);
  }
}

STDMETHODIMP_(void) CParserEitDvb::SetProtocols(bool grabDvbEit,
                                                bool grabBellExpressVu,
                                                bool grabDish,
                                                bool grabFreesat,
                                                bool grabMultiChoice,
                                                bool grabPremiere,
                                                bool grabViasatSweden)
{
  LogDebug(L"EIT DVB: set protocols, DVB EIT = %d, Bell ExpressVu = %d, Dish = %d, Freesat = %d, MultiChoice = %d, Premiere = %d, Viasat Sweden = %d",
            grabDvbEit, grabBellExpressVu, grabDish, grabFreesat,
            grabMultiChoice, grabPremiere, grabViasatSweden);
  CEnterCriticalSection lock(m_section);

  vector<unsigned short> pidsAdd;
  vector<unsigned short> pidsRemove;

  // BAT, SDT and NIT are needed to get the default authority, which is a
  // prefix for some series and episode CRIDs. Note we don't make any
  // assumptions about which providers actually use CRIDs.
  bool currentlyRequireSiPids = false;
  if (
    m_grabPids[PID_EIT_DVB] ||
    m_grabPids[PID_EIT_BELL_EXPRESSVU] ||
    m_grabPids[PID_EIT_DISH] ||
    m_grabPids[PID_EIT_MULTICHOICE] ||
    m_grabPids[PID_EIT_PREMIERE_DIREKT] ||
    m_grabPids[PID_EIT_PREMIERE_SELECT] ||
    m_grabPids[PID_EIT_PREMIERE_SPORT] ||
    m_grabPids[PID_EIT_VIASAT_SWEDEN]
  )
  {
    currentlyRequireSiPids = true;
  }
  bool requireSiPids = false;
  if (
    grabDvbEit ||
    grabBellExpressVu ||
    grabDish ||
    grabMultiChoice ||
    grabPremiere ||
    grabViasatSweden
  )
  {
    requireSiPids = true;
  }
  if (currentlyRequireSiPids != requireSiPids)
  {
    if (requireSiPids)
    {
      pidsAdd.push_back(PID_BAT);
      pidsAdd.push_back(PID_NIT_DVB);
      //pidsAdd.push_back(PID_SDT);     Same as BAT => not required.
    }
    else
    {
      pidsRemove.push_back(PID_BAT);
      pidsRemove.push_back(PID_NIT_DVB);
      //pidsRemove.push_back(PID_SDT);  Same as BAT => not required.
    }
  }

  if (grabDvbEit != m_grabPids[PID_EIT_DVB])
  {
    if (grabDvbEit)
    {
      pidsAdd.push_back(PID_EIT_DVB);
    }
    else
    {
      pidsRemove.push_back(PID_EIT_DVB);
    }
    m_grabPids[PID_EIT_DVB] = grabDvbEit;
  }
  if (grabBellExpressVu != m_grabPids[PID_EIT_BELL_EXPRESSVU])
  {
    if (grabBellExpressVu)
    {
      pidsAdd.push_back(PID_EIT_BELL_EXPRESSVU);
    }
    else
    {
      pidsRemove.push_back(PID_EIT_BELL_EXPRESSVU);
    }
    m_grabPids[PID_EIT_BELL_EXPRESSVU] = grabBellExpressVu;
  }
  if (grabDish != m_grabPids[PID_EIT_DISH])
  {
    if (grabDish)
    {
      pidsAdd.push_back(PID_EIT_DISH);
    }
    else
    {
      pidsRemove.push_back(PID_EIT_DISH);
    }
    m_grabPids[PID_EIT_DISH] = grabDish;
  }
  if (grabMultiChoice != m_grabPids[PID_EIT_MULTICHOICE])
  {
    if (grabMultiChoice)
    {
      pidsAdd.push_back(PID_EIT_MULTICHOICE);
    }
    else
    {
      pidsRemove.push_back(PID_EIT_MULTICHOICE);
    }
    m_grabPids[PID_EIT_MULTICHOICE] = grabMultiChoice;
  }
  if (grabPremiere != m_grabPids[PID_EIT_PREMIERE_DIREKT])
  {
    if (grabPremiere)
    {
      pidsAdd.push_back(PID_EIT_PREMIERE_DIREKT);
      pidsAdd.push_back(PID_EIT_PREMIERE_SELECT);
      pidsAdd.push_back(PID_EIT_PREMIERE_SPORT);
    }
    else
    {
      pidsRemove.push_back(PID_EIT_PREMIERE_DIREKT);
      pidsRemove.push_back(PID_EIT_PREMIERE_SELECT);
      pidsRemove.push_back(PID_EIT_PREMIERE_SPORT);
    }
    m_grabPids[PID_EIT_PREMIERE_DIREKT] = grabPremiere;
    m_grabPids[PID_EIT_PREMIERE_SELECT] = grabPremiere;
    m_grabPids[PID_EIT_PREMIERE_SPORT] = grabPremiere;
  }
  if (grabViasatSweden != m_grabPids[PID_EIT_VIASAT_SWEDEN])
  {
    if (grabViasatSweden)
    {
      pidsAdd.push_back(PID_EIT_VIASAT_SWEDEN);
    }
    else
    {
      pidsRemove.push_back(PID_EIT_VIASAT_SWEDEN);
    }
    m_grabPids[PID_EIT_VIASAT_SWEDEN] = grabViasatSweden;
  }

  if (grabFreesat != m_grabFreesat)
  {
    if (grabFreesat)
    {
      if (m_freesatPidEitPf != 0)
      {
        pidsAdd.push_back(m_freesatPidEitPf);
        m_grabPids[m_freesatPidEitPf] = grabFreesat;
      }
      if (m_freesatPidEitSchedule != 0)
      {
        pidsAdd.push_back(m_freesatPidEitSchedule);
        m_grabPids[m_freesatPidEitSchedule] = grabFreesat;
      }
      if (m_freesatPidBat != 0)
      {
        pidsAdd.push_back(m_freesatPidBat);
      }
      if (m_freesatPidNit != 0)
      {
        pidsAdd.push_back(m_freesatPidNit);
      }
      if (m_freesatPidPmt != 0)
      {
        pidsAdd.push_back(m_freesatPidPmt);
      }
      if (m_freesatPidSdt != 0)
      {
        pidsAdd.push_back(m_freesatPidSdt);
      }
    }
    else
    {
      if (m_freesatPidEitPf != 0)
      {
        pidsRemove.push_back(m_freesatPidEitPf);
        m_grabPids[m_freesatPidEitPf] = grabFreesat;
      }
      if (m_freesatPidEitSchedule != 0)
      {
        pidsRemove.push_back(m_freesatPidEitSchedule);
        m_grabPids[m_freesatPidEitSchedule] = grabFreesat;
      }
      if (m_freesatPidBat != 0)
      {
        pidsRemove.push_back(m_freesatPidBat);
      }
      if (m_freesatPidNit != 0)
      {
        pidsRemove.push_back(m_freesatPidNit);
      }
      if (m_freesatPidPmt != 0)
      {
        pidsRemove.push_back(m_freesatPidPmt);
      }
      if (m_freesatPidSdt != 0)
      {
        pidsRemove.push_back(m_freesatPidSdt);
      }
    }
    m_grabFreesat = grabFreesat;
  }

  if (pidsAdd.size() > 0 || pidsRemove.size() > 0)
  {
    PrivateReset(false);
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

void CParserEitDvb::Reset(bool enableCrcCheck)
{
  LogDebug(L"EIT DVB: reset");
  m_enableCrcCheck = enableCrcCheck;
  PrivateReset(true);
  LogDebug(L"EIT DVB: reset done");
}

STDMETHODIMP_(void) CParserEitDvb::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
}

bool CParserEitDvb::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  CEnterCriticalSection lock(m_section);
  map<unsigned short, bool>::const_iterator it = m_grabPids.find(header.Pid);
  if (it != m_grabPids.end() && it->second)
  {
    map<unsigned short, CSectionDecoder*>::const_iterator it2 = m_decoders.find(header.Pid);
    if (it2 != m_decoders.end() && it2->second != NULL)
    {
      it2->second->OnTsPacket(header, tsPacket);
      return true;
    }
  }
  return false;
}

STDMETHODIMP_(bool) CParserEitDvb::IsSeen()
{
  CEnterCriticalSection lock(m_section);
  return m_isSeen;
}

STDMETHODIMP_(bool) CParserEitDvb::IsReady()
{
  CEnterCriticalSection lock(m_section);
  return m_isReady;
}

STDMETHODIMP_(unsigned short) CParserEitDvb::GetServiceCount()
{
  CEnterCriticalSection lock(m_section);
  return (unsigned short)m_services.size();
}

STDMETHODIMP_(bool) CParserEitDvb::GetService(unsigned short index,
                                              unsigned short* originalNetworkId,
                                              unsigned short* transportStreamId,
                                              unsigned short* serviceId,
                                              unsigned short* eventCount)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectServiceRecordByIndex(index))
  {
    return false;
  }

  *originalNetworkId = m_currentService->OriginalNetworkId;
  *transportStreamId = m_currentService->TransportStreamId;
  *serviceId = m_currentService->ServiceId;
  *eventCount = (unsigned short)m_currentService->Events.GetRecordCount();
  return true;
}

STDMETHODIMP_(bool) CParserEitDvb::GetEvent(unsigned short serviceIndex,
                                            unsigned short eventIndex,
                                            unsigned long long* eventId,
                                            unsigned long long* startDateTime,
                                            unsigned short* duration,
                                            unsigned char* runningStatus,
                                            bool* freeCaMode,
                                            unsigned short* referenceServiceId,
                                            unsigned long long* referenceEventId,
                                            char* seriesId,
                                            unsigned short* seriesIdBufferSize,
                                            char* episodeId,
                                            unsigned short* episodeIdBufferSize,
                                            bool* isHighDefinition,
                                            bool* isStandardDefinition,
                                            bool* isThreeDimensional,
                                            bool* isPreviouslyShown,
                                            unsigned long* audioLanguages,
                                            unsigned char* audioLanguageCount,
                                            unsigned long* subtitlesLanguages,
                                            unsigned char* subtitlesLanguageCount,
                                            unsigned short* dvbContentTypeIds,
                                            unsigned char* dvbContentTypeIdCount,
                                            unsigned long* dvbParentalRatingCountryCodes,
                                            unsigned char* dvbParentalRatings,
                                            unsigned char* dvbParentalRatingCount,
                                            unsigned char* starRating,
                                            unsigned char* mpaaClassification,
                                            unsigned short* dishBevAdvisories,
                                            unsigned char* vchipRating,
                                            unsigned char* textCount)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectServiceRecordByIndex(serviceIndex))
  {
    return false;
  }

  if (!SelectEventRecordByIndex(eventIndex))
  {
    return false;
  }

  *eventId = m_currentEvent->EventId;
  *startDateTime = m_currentEvent->StartDateTime;
  *duration = m_currentEvent->Duration;
  *runningStatus = m_currentEvent->RunningStatus;
  *freeCaMode = m_currentEvent->FreeCaMode;
  *referenceServiceId = m_referenceServiceId;
  *referenceEventId = m_referenceEventId;

  // Assumption: for time-shifted/NVOD services/events, all other details are
  // kept with the reference event.
  CRecordEitEventMinimal* recordEvent = m_currentEvent;
  CRecordEitEvent* recordEventFull = dynamic_cast<CRecordEitEvent*>(m_currentEvent);
  if (m_referenceEvent != NULL)
  {
    recordEvent = m_referenceEvent;
    recordEventFull = dynamic_cast<CRecordEitEvent*>(m_referenceEvent);
  }
  if (recordEventFull != NULL)
  {
    *isHighDefinition = recordEventFull->IsHighDefinition;
    *isStandardDefinition = recordEventFull->IsStandardDefinition;
    *isThreeDimensional = recordEventFull->IsThreeDimensional;
  }
  else
  {
    *isHighDefinition = false;
    *isStandardDefinition = false;
    *isThreeDimensional = false;

    *audioLanguageCount = 0;
    *subtitlesLanguageCount = 0;
    *dvbParentalRatingCount = 0;
  }
  *isPreviouslyShown = recordEvent->IsPreviouslyShown;
  *starRating = recordEvent->StarRating;
  *mpaaClassification = recordEvent->MpaaClassification;
  *dishBevAdvisories = recordEvent->DishBevAdvisories;
  *vchipRating = recordEvent->VchipRating;
  *textCount = (unsigned char)recordEvent->Texts.size();

  // Add default authority as series/episode ID prefix if required.
  char* seriesIdSource = recordEvent->SeriesId;
  char* episodeIdSource = recordEvent->EpisodeId;
  unsigned short requiredBufferSize = 0;
  if (recordEvent->AreSeriesAndEpisodeIdsCrids)
  {
    // Default authorities already have the CRID prefix.
    unsigned short authorityBufferSize = 100;
    char* authority = NULL;
    if (
      m_defaultAuthorityProvider != NULL &&
      (
        (seriesIdSource != NULL && seriesIdSource[0] == '/') ||
        (episodeIdSource != NULL && episodeIdSource[0] == '/')
      )
    )
    {
      authority = new char[authorityBufferSize];
      if (authority == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hu bytes for a CRID authority, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu",
                  authorityBufferSize, serviceIndex,
                  m_currentService->OriginalNetworkId,
                  m_currentService->TransportStreamId,
                  m_currentService->ServiceId, eventIndex,
                  m_currentEvent->EventId, *referenceServiceId,
                  *referenceEventId);
      }
      else if (!m_defaultAuthorityProvider->GetDefaultAuthority(m_currentService->OriginalNetworkId,
                                                                m_currentService->TransportStreamId,
                                                                recordEvent->ServiceId,
                                                                authority,
                                                                authorityBufferSize))
      {
        LogDebug(L"EIT DVB: missing CRID authority, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu",
                  serviceIndex, m_currentService->OriginalNetworkId,
                  m_currentService->TransportStreamId,
                  m_currentService->ServiceId, eventIndex,
                  m_currentEvent->EventId, *referenceServiceId,
                  *referenceEventId);
      }
    }

    unsigned short availableBufferSize = 0;
    if (seriesIdSource != NULL)
    {
      requiredBufferSize = 0;
      if (seriesIdSource[0] == '/')
      {
        if (authority != NULL)
        {
          requiredBufferSize = authorityBufferSize + strlen(seriesIdSource) + 1;
          availableBufferSize = requiredBufferSize - authorityBufferSize;
        }
      }
      else if (
        strncmp(seriesIdSource, "crid://", 7) != 0 &&
        strncmp(seriesIdSource, "CRID://", 7) != 0
      )
      {
        requiredBufferSize = 7 + strlen(seriesIdSource) + 1;
        availableBufferSize = requiredBufferSize - 7;
      }

      if (requiredBufferSize != 0)
      {
        seriesIdSource = new char[requiredBufferSize];
        if (seriesIdSource == NULL)
        {
          LogDebug(L"EIT DVB: failed to allocate %hu bytes for a full series ID, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu",
                    requiredBufferSize, serviceIndex,
                    m_currentService->OriginalNetworkId,
                    m_currentService->TransportStreamId,
                    m_currentService->ServiceId, eventIndex,
                    m_currentEvent->EventId, *referenceServiceId,
                    *referenceEventId);
        }
        else
        {
          if (seriesIdSource[0] == '/')
          {
            strncpy(seriesIdSource, authority, requiredBufferSize);
          }
          else
          {
            strncpy(seriesIdSource, "crid://", requiredBufferSize);
          }
          strncat(seriesIdSource, recordEvent->SeriesId, availableBufferSize);
          seriesIdSource[requiredBufferSize - 1] = NULL;
        }
      }
    }

    if (episodeIdSource != NULL)
    {
      requiredBufferSize = 0;
      if (episodeIdSource[0] == '/')
      {
        if (authority != NULL)
        {
          requiredBufferSize = authorityBufferSize + strlen(episodeIdSource) + 1;
          availableBufferSize = requiredBufferSize - authorityBufferSize;
        }
      }
      else if (
        strncmp(episodeIdSource, "crid://", 7) != 0 &&
        strncmp(episodeIdSource, "CRID://", 7) != 0
      )
      {
        requiredBufferSize = 7 + strlen(episodeIdSource) + 1;
        availableBufferSize = requiredBufferSize - 7;
      }

      if (requiredBufferSize != 0)
      {
        episodeIdSource = new char[requiredBufferSize];
        if (episodeIdSource == NULL)
        {
          LogDebug(L"EIT DVB: failed to allocate %hu bytes for a full episode ID, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu",
                    requiredBufferSize, serviceIndex,
                    m_currentService->OriginalNetworkId,
                    m_currentService->TransportStreamId,
                    m_currentService->ServiceId, eventIndex,
                    m_currentEvent->EventId, *referenceServiceId,
                    *referenceEventId);
        }
        else
        {
          if (episodeIdSource[0] == '/')
          {
            strncpy(episodeIdSource, authority, requiredBufferSize);
          }
          else
          {
            strncpy(episodeIdSource, "crid://", requiredBufferSize);
          }
          strncat(episodeIdSource, recordEvent->EpisodeId, availableBufferSize);
          episodeIdSource[requiredBufferSize - 1] = NULL;
        }
      }
    }

    if (authority != NULL)
    {
      delete[] authority;
      authority = NULL;
    }
  }

  if (!CUtils::CopyStringToBuffer(seriesIdSource,
                                  seriesId,
                                  *seriesIdBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EIT DVB: insufficient series ID buffer size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, required size = %hu, actual size = %hu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, *referenceServiceId,
              *referenceEventId, requiredBufferSize, *seriesIdBufferSize);
  }
  if (seriesIdSource != NULL && seriesIdSource != recordEvent->SeriesId)
  {
    delete[] seriesIdSource;
  }
  if (!CUtils::CopyStringToBuffer(episodeIdSource,
                                  episodeId,
                                  *episodeIdBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EIT DVB: insufficient episode ID buffer size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, required size = %hu, actual size = %hu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, *referenceServiceId,
              *referenceEventId, requiredBufferSize, *episodeIdBufferSize);
  }
  if (episodeIdSource != NULL && episodeIdSource != recordEvent->EpisodeId)
  {
    delete[] episodeIdSource;
  }

  unsigned char requiredCount = 0;
  if (recordEventFull != NULL)
  {
    if (!CUtils::CopyVectorToArray(recordEventFull->AudioLanguages,
                                    audioLanguages,
                                    *audioLanguageCount,
                                    requiredCount))
    {
      LogDebug(L"EIT DVB: insufficient audio language array size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, required size = %hhu, actual size = %hhu",
                serviceIndex, m_currentService->OriginalNetworkId,
                m_currentService->TransportStreamId,
                m_currentService->ServiceId, eventIndex,
                m_currentEvent->EventId, *referenceServiceId,
                *referenceEventId, requiredCount, *audioLanguageCount);
    }
    if (!CUtils::CopyVectorToArray(recordEventFull->SubtitlesLanguages,
                                    subtitlesLanguages,
                                    *subtitlesLanguageCount,
                                    requiredCount))
    {
      LogDebug(L"EIT DVB: insufficient subtitles language array size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, required size = %hhu, actual size = %hhu",
                serviceIndex, m_currentService->OriginalNetworkId,
                m_currentService->TransportStreamId,
                m_currentService->ServiceId, eventIndex,
                m_currentEvent->EventId, *referenceServiceId,
                *referenceEventId, requiredCount, *subtitlesLanguageCount);
    }
  }
  if (!CUtils::CopyVectorToArray(recordEvent->DvbContentTypeIds,
                                  dvbContentTypeIds,
                                  *dvbContentTypeIdCount,
                                  requiredCount))
  {
    LogDebug(L"EIT DVB: insufficient DVB content type ID array size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, required size = %hhu, actual size = %hhu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, *referenceServiceId,
              *referenceEventId, requiredCount, *dvbContentTypeIdCount);
  }

  if (recordEventFull == NULL)
  {
    return true;
  }

  requiredCount = recordEventFull->DvbParentalRatings.size();
  if (requiredCount == 0)
  {
    *dvbParentalRatingCount = 0;
    return true;
  }
  if (
    dvbParentalRatingCountryCodes == NULL ||
    dvbParentalRatings == NULL ||
    *dvbParentalRatingCount < requiredCount
  )
  {
    LogDebug(L"EIT DVB: insufficient DVB parental ratings array size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, required size = %hhu, actual size = %hhu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, *referenceServiceId,
              *referenceEventId, requiredCount, *dvbParentalRatingCount);
    if (dvbParentalRatingCountryCodes == NULL || dvbParentalRatings == NULL)
    {
      *dvbParentalRatingCount = 0;
      return true;
    }
  }
  else
  {
    *dvbParentalRatingCount = requiredCount;
  }
  map<unsigned long, unsigned char>::const_iterator it = recordEventFull->DvbParentalRatings.begin();
  for (unsigned char i = 0; i < *dvbParentalRatingCount; i++, it++)
  {
    dvbParentalRatingCountryCodes[i] = it->first;
    dvbParentalRatings[i] = it->second;
  }
  return true;
}

STDMETHODIMP_(bool) CParserEitDvb::GetEventText(unsigned short serviceIndex,
                                                unsigned short eventIndex,
                                                unsigned char textIndex,
                                                unsigned long* language,
                                                char* title,
                                                unsigned short* titleBufferSize,
                                                char* shortDescription,
                                                unsigned short* shortDescriptionBufferSize,
                                                char* extendedDescription,
                                                unsigned short* extendedDescriptionBufferSize,
                                                unsigned char* descriptionItemCount)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectServiceRecordByIndex(serviceIndex))
  {
    return false;
  }

  if (!SelectEventRecordByIndex(eventIndex))
  {
    return false;
  }

  if (!SelectTextRecordByIndex(textIndex))
  {
    return false;
  }

  *language = m_currentEventText->Language;
  *descriptionItemCount = (unsigned char)m_currentEventText->DescriptionItems.size();

  unsigned short requiredBufferSize = 0;

  char* tempTitle = NULL;
  m_currentEventText->Decompress(m_currentEventText->Title,
                                  &tempTitle,
                                  L"name");
  if (!CUtils::CopyStringToBuffer(tempTitle,
                                  title,
                                  *titleBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EIT DVB: insufficient title buffer size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, language = %S, required size = %hu, actual size = %hu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId, textIndex, (char*)language,
              requiredBufferSize, *titleBufferSize);
  }
  if (tempTitle != NULL && tempTitle != m_currentEventText->Title)
  {
    delete[] tempTitle;
  }

  char* tempDescription = NULL;
  m_currentEventText->Decompress(m_currentEventText->DescriptionShort,
                                  &tempDescription,
                                  L"description");
  if (!CUtils::CopyStringToBuffer(tempDescription,
                                  shortDescription,
                                  *shortDescriptionBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EIT DVB: insufficient short description buffer size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, language = %S, required size = %hu, actual size = %hu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId, textIndex, (char*)language,
              requiredBufferSize, *shortDescriptionBufferSize);
  }
  if (tempDescription != NULL && tempDescription != m_currentEventText->DescriptionShort)
  {
    delete[] tempDescription;
  }

  if (!CUtils::CopyStringToBuffer(m_currentEventText->DescriptionExtended,
                                  extendedDescription,
                                  *extendedDescriptionBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EIT DVB: insufficient extended description buffer size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, language = %S, required size = %hu, actual size = %hu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId, textIndex, (char*)language,
              requiredBufferSize, *extendedDescriptionBufferSize);
  }

  return true;
}

STDMETHODIMP_(bool) CParserEitDvb::GetEventDescriptionItem(unsigned short serviceIndex,
                                                            unsigned short eventIndex,
                                                            unsigned char textIndex,
                                                            unsigned char itemIndex,
                                                            char* description,
                                                            unsigned short* descriptionBufferSize,
                                                            char* text,
                                                            unsigned short* textBufferSize)
{
  CEnterCriticalSection lock(m_section);
  if (!SelectServiceRecordByIndex(serviceIndex))
  {
    return false;
  }

  if (!SelectEventRecordByIndex(eventIndex))
  {
    return false;
  }

  if (!SelectTextRecordByIndex(textIndex))
  {
    return false;
  }

  if (itemIndex >= m_currentEventText->DescriptionItems.size())
  {
    LogDebug(L"EIT DVB: invalid description item index, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, text count = %lu, item index = %hhu, item count = %llu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId, textIndex,
              (char*)&(m_currentEventText->Language), itemIndex,
              (unsigned long long)m_currentEventText->DescriptionItems.size());
    return false;
  }

  unsigned char i = itemIndex;
  map<unsigned short, CRecordEitEventDescriptionItem*>::const_iterator it = m_currentEventText->DescriptionItems.begin();
  for ( ; it != m_currentEventText->DescriptionItems.end(); it++)
  {
    if (i == 0)
    {
      break;
    }
    i--;
  }

  CRecordEitEventDescriptionItem* recordItem = it->second;
  if (recordItem == NULL)
  {
    LogDebug(L"EIT DVB: invalid description item record, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, text count = %lu, item index = %hhu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId, textIndex,
              (char*)&(m_currentEventText->Language), itemIndex);
    return false;
  }

  unsigned short requiredBufferSize = 0;
  if (!CUtils::CopyStringToBuffer(recordItem->Description,
                                  description,
                                  *descriptionBufferSize,
                                  requiredBufferSize))
  {
    LogDebug(L"EIT DVB: insufficient item description buffer size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, language = %S, item index = %hhu, required size = %hu, actual size = %hu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId, textIndex,
              (char*)&(m_currentEventText->Language), itemIndex,
              requiredBufferSize, *descriptionBufferSize);
  }
  if (!CUtils::CopyStringToBuffer(recordItem->Text, text, *textBufferSize, requiredBufferSize))
  {
    LogDebug(L"EIT DVB: insufficient item text buffer size, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, language = %S, item index = %hhu, required size = %hu, actual size = %hu",
              serviceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              eventIndex, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId, textIndex,
              (char*)&(m_currentEventText->Language), itemIndex,
              requiredBufferSize, *textBufferSize);
  }

  return true;
}

bool CParserEitDvb::SelectServiceRecordByIndex(unsigned short index)
{
  if (m_currentService != NULL && m_currentServiceIndex == index)
  {
    return true;
  }

  if (index >= m_services.size())
  {
    LogDebug(L"EIT DVB: invalid service index, index = %hu, record count = %llu",
              index, (unsigned long long)m_services.size());
    return false;
  }

  unsigned short i = index;
  map<unsigned long long, CRecordEitService*>::const_iterator it = m_services.begin();
  for ( ; it != m_services.end(); it++)
  {
    if (i == 0)
    {
      break;
    }
    i--;
  }

  m_currentService = it->second;
  if (m_currentService == NULL)
  {
    LogDebug(L"EIT DVB: invalid service record, index = %hu", index);
    return false;
  }

  m_currentServiceIndex = index;
  m_currentEvent = NULL;
  m_currentEventIndex = 0;
  m_currentEventText = NULL;
  m_currentEventTextIndex = 0;
  m_referenceEvent = NULL;
  m_referenceServiceId = 0;
  m_referenceEventId = 0;
  return true;
}

bool CParserEitDvb::SelectEventRecordByIndex(unsigned short index)
{
  if (m_currentEvent != NULL && m_currentEventIndex == index)
  {
    return true;
  }

  IRecord* record = NULL;
  if (!m_currentService->Events.GetRecordByIndex(index, &record) || record == NULL)
  {
    LogDebug(L"EIT DVB: invalid event index, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event count = %lu",
              m_currentServiceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              index, m_currentService->Events.GetRecordCount());
    return false;
  }
  m_currentEvent = dynamic_cast<CRecordEitEventMinimal*>(record);
  if (m_currentEvent == NULL)
  {
    LogDebug(L"EIT DVB: invalid event record, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu",
              m_currentServiceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              index);
    return false;
  }

  m_currentEventIndex = index;
  m_currentEventText = NULL;
  m_currentEventTextIndex = 0;
  m_referenceEvent = NULL;
  m_referenceServiceId = 0;
  m_referenceEventId = 0;

  CRecordEitEvent* recordEventFull = dynamic_cast<CRecordEitEvent*>(m_currentEvent);
  if (recordEventFull != NULL)
  {
    m_referenceServiceId = recordEventFull->ReferenceServiceId;
    m_referenceEventId = recordEventFull->ReferenceEventId;
  }
  if (m_referenceServiceId == 0 || m_referenceEventId == 0)
  {
    return true;
  }

  CRecordEitService* referenceService = GetOrCreateService(m_currentService->IsPremiereService,
                                                            m_currentService->OriginalNetworkId,
                                                            m_currentService->TransportStreamId,
                                                            m_referenceServiceId,
                                                            true);
  if (referenceService == NULL)
  {
    LogDebug(L"EIT DVB: invalid reference service record, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu",
              m_currentServiceIndex, m_currentEvent->OriginalNetworkId,
              m_currentEvent->TransportStreamId, m_currentEvent->ServiceId,
              index, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId);
    return true;
  }

  if (
    !referenceService->Events.GetRecordByKey(m_referenceEventId, &record) ||
    record == NULL
  )
  {
    LogDebug(L"EIT DVB: invalid reference event key, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu",
              m_currentServiceIndex, m_currentEvent->OriginalNetworkId,
              m_currentEvent->TransportStreamId, m_currentEvent->ServiceId,
              index, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId);
    return true;
  }

  m_referenceEvent = dynamic_cast<CRecordEitEvent*>(record);
  if (m_referenceEvent == NULL)
  {
    LogDebug(L"EIT DVB: invalid reference event record, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu",
              m_currentServiceIndex, m_currentEvent->OriginalNetworkId,
              m_currentEvent->TransportStreamId, m_currentEvent->ServiceId,
              index, m_currentEvent->EventId, m_referenceServiceId,
              m_referenceEventId);
  }
  return true;
}

bool CParserEitDvb::SelectTextRecordByIndex(unsigned char index)
{
  if (m_currentEventText != NULL && m_currentEventTextIndex == index)
  {
    return true;
  }

  CRecordEitEventMinimal* recordEvent = m_currentEvent;
  if (m_referenceEvent != NULL)
  {
    recordEvent = m_referenceEvent;
  }

  if (index >= recordEvent->Texts.size())
  {
    LogDebug(L"EIT DVB: invalid text index, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu, standard event text count = %llu, reference event text count = %hhu",
              m_currentServiceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              m_currentEventIndex, m_currentEvent->EventId,
              m_referenceServiceId, m_referenceEventId, index,
              (unsigned long long)m_currentEvent->Texts.size(),
              (m_referenceEvent == NULL ? 0 : (unsigned char)m_referenceEvent->Texts.size()));
    return false;
  }

  unsigned char i = index;
  map<unsigned long, CRecordEitEventText*>::const_iterator it = recordEvent->Texts.begin();
  for ( ; it != recordEvent->Texts.end(); it++)
  {
    if (i == 0)
    {
      break;
    }
    i--;
  }

  m_currentEventText = it->second;
  if (m_currentEventText == NULL)
  {
    LogDebug(L"EIT DVB: invalid text record, service index = %hu, ONID = %hu, TSID = %hu, service ID = %hu, event index = %hu, event ID = %llu, reference service ID = %hu, reference event ID = %llu, text index = %hhu",
              m_currentServiceIndex, m_currentService->OriginalNetworkId,
              m_currentService->TransportStreamId, m_currentService->ServiceId,
              m_currentEventIndex, m_currentEvent->EventId,
              m_referenceServiceId, m_referenceEventId, index);
    return false;
  }

  m_currentEventTextIndex = index;
  return true;
}

void CParserEitDvb::OnNewSection(int pid, int tableId, CSection& section)
{
  try
  {
    if (!section.SectionSyntaxIndicator || !section.CurrentNextIndicator)
    {
      return;
    }

    bool isPremiereTable = false;
    switch (pid)
    {
      case PID_EIT_DVB:
      case PID_EIT_VIASAT_SWEDEN:
      case PID_EIT_MULTICHOICE:
        if (
          (m_grabFreesat && (m_freesatPidEitPf > 0 || m_freesatPidEitSchedule > 0)) ||
          tableId < TABLE_ID_EIT_DVB_START || tableId > TABLE_ID_EIT_DVB_END
        )
        {
          return;
        }
        break;
      case PID_EIT_DISH:
      {
        if (section.section_length < 12)
        {
          return;
        }
        unsigned short tempOriginalNetworkId = (section.Data[10] << 8) | section.Data[11];
        if (
          tempOriginalNetworkId < ORIGINAL_NETWORK_ID_DISH_START ||
          tempOriginalNetworkId > ORIGINAL_NETWORK_ID_DISH_END
        )
        {
          return;
        }
      }
      case PID_EIT_BELL_EXPRESSVU:
        if (tableId < TABLE_ID_EIT_DISH_START || tableId > TABLE_ID_EIT_DISH_END)
        {
          return;
        }
        break;
      case PID_EIT_PREMIERE_DIREKT:
      case PID_EIT_PREMIERE_SELECT:
      case PID_EIT_PREMIERE_SPORT:
        if (tableId != TABLE_ID_EIT_PREMIERE)
        {
          return;
        }
        isPremiereTable = true;
        break;
      default:
        // Freesat
        if (tableId < TABLE_ID_EIT_DVB_START || tableId > TABLE_ID_EIT_DVB_END)
        {
          return;
        }
        break;
    }

    if (
      section.section_length > 4093 ||
      (isPremiereTable && section.section_length < 9) ||    // Note: Premiere *may* use the least significant 15 (out of 16) bits.
      (!isPremiereTable && section.section_length < 15)
    )
    {
      LogDebug(L"EIT DVB: invalid section, length = %d, table ID = 0x%x, PID = %d",
                section.section_length, tableId, pid);
      return;
    }

    unsigned char* data = section.Data;
    unsigned short serviceId = section.table_id_extension;
    unsigned short transportStreamId = 0;
    unsigned short originalNetworkId = 0;
    unsigned char segmentLastSectionNumber = 0;
    unsigned char lastTableId = 0;
    if (!isPremiereTable)
    {
      transportStreamId = (data[8] << 8) | data[9];
      originalNetworkId = (data[10] << 8) | data[11];
      segmentLastSectionNumber = data[12];
      lastTableId = data[13];
    }

    //LogDebug(L"EIT DVB: PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, version number = %d, section length = %d, section number = %d, last section number = %d, segment last section number = %hhu, last table ID = 0x%hhx",
    //          pid, tableId, serviceId, transportStreamId, originalNetworkId,
    //          section.version_number, section.section_length,
    //          section.SectionNumber, section.LastSectionNumber,
    //          segmentLastSectionNumber, lastTableId);

    CEnterCriticalSection lock(m_section);
    CRecordEitService* service = GetOrCreateService(isPremiereTable,
                                                    originalNetworkId,
                                                    transportStreamId,
                                                    serviceId,
                                                    false);
    if (service == NULL)
    {
      return;
    }

    // Have we seen this section before?
    unsigned long sectionKey = (tableId << 16) | (section.version_number << 8) | section.SectionNumber;
    unsigned long segmentKeyMask = 0xfffffff8;
    unsigned long segmentKey = sectionKey & segmentKeyMask;
    vector<unsigned long>::const_iterator sectionIt = find(service->SeenSections.begin(),
                                                            service->SeenSections.end(),
                                                            sectionKey);
    if (sectionIt != service->SeenSections.end())
    {
      // Yes. We might be ready!
      //LogDebug(L"EIT DVB: previously seen section, PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, section number = %d",
      //          pid, tableId, serviceId, transportStreamId,
      //          originalNetworkId, section.SectionNumber);
      if (
        m_isReady ||
        m_unseenTableCount != 0 ||
        m_unseenSegmentCount != 0 ||
        m_unseenSectionCount != 0
      )
      {
        return;
      }

      // TS 101 211 section 4.4 recommends minimum repetition rates:
      // EIT P/F actual = 2 seconds
      // EIT P/F other = 10 seconds cable, satellite; 20 seconds terrestrial
      // EIT schedule actual first day = 10 seconds
      // EIT schedule other first day = 10 seconds cable, satellite; 60 seconds terrestrial
      // EIT schedule actual next 7 days = 10 seconds cable, satellite; 30 seconds terrestrial
      // EIT schedule other next 7 days = 10 seconds cable, satellite; 300 seconds terrestrial
      // EIT schedule actual beyond 8 days = 30 seconds
      // EIT schedule actual beyond 8 days = 30 seconds cable, satellite; 300 seconds terrestrial
      if (CTimeUtils::ElapsedMillis(m_completeTime) >= 30000)
      {
        LogDebug(L"EIT DVB: ready...");
        unsigned long seenSectionCount = 0;
        unsigned long eventCount = 0;
        map<unsigned long long, CRecordEitService*>::const_iterator serviceIt = m_services.begin();
        for ( ; serviceIt != m_services.end(); serviceIt++)
        {
          service = serviceIt->second;
          if (service == NULL)
          {
            continue;
          }

          service->Events.RemoveExpiredRecords(NULL);
          seenSectionCount += service->SeenSections.size();
          eventCount += service->Events.GetRecordCount();
          LogDebug(L"  ONID = %hu, TSID = %hu, service ID = %hu, sections parsed = %llu, event count = %lu",
                    service->OriginalNetworkId, service->TransportStreamId,
                    service->ServiceId,
                    (unsigned long long)service->SeenSections.size(),
                    service->Events.GetRecordCount());
        }

        LogDebug(L"EIT DVB: totals, sections parsed = %lu, service count = %llu, event count = %lu",
                  seenSectionCount, (unsigned long long)m_services.size(),
                  eventCount);
        m_isReady = true;
        if (m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableComplete(PID_EIT_DVB_CALL_BACK, TABLE_ID_EIT_DVB_CALL_BACK);
        }
      }
      return;
    }

    // Were we expecting this section?
    sectionIt = find(service->UnseenSections.begin(), service->UnseenSections.end(), sectionKey);
    if (sectionIt == service->UnseenSections.end())
    {
      // No. Is this a change/update, or just a new section/segment/table?
      bool isSingleTableGroup = (
        tableId == TABLE_ID_EIT_DVB_PF_ACTUAL ||
        tableId == TABLE_ID_EIT_DVB_PF_OTHER ||
        isPremiereTable
      );
      vector<unsigned char> erasedTableIds;
      RemoveExpiredEntries(service->UnseenSections,
                            false,
                            tableId,
                            section.version_number,
                            !isSingleTableGroup,
                            lastTableId,
                            erasedTableIds);
      m_unseenSectionCount -= erasedTableIds.size();
      RemoveExpiredEntries(service->SeenSections,
                            false,
                            tableId,
                            section.version_number,
                            !isSingleTableGroup,
                            lastTableId,
                            erasedTableIds);
      bool isChange = erasedTableIds.size() > 0;
      erasedTableIds.clear();

      if (!isPremiereTable)
      {
        RemoveExpiredEntries(service->UnseenTables,
                              true,
                              tableId,
                              section.version_number,
                              !isSingleTableGroup,
                              lastTableId,
                              erasedTableIds);
        m_unseenTableCount -= erasedTableIds.size();
        RemoveExpiredEntries(service->SeenTables,
                              true,
                              tableId,
                              section.version_number,
                              !isSingleTableGroup,
                              lastTableId,
                              erasedTableIds);

        if (!isSingleTableGroup)
        {
          vector<unsigned char>::const_iterator tableIt = erasedTableIds.begin();
          for ( ; tableIt != erasedTableIds.end(); tableIt++)
          {
            service->Events.MarkExpiredRecords(*tableIt);
          }
        }
        if (isChange)
        {
          vector<unsigned char>::const_iterator tableIt = find(service->UnseenTables.begin(),
                                                                service->UnseenTables.end(),
                                                                tableId);
          if (tableIt != service->UnseenTables.end())
          {
            service->UnseenTables.erase(tableIt);
            m_unseenTableCount--;
          }
          tableIt = find(service->SeenTables.begin(), service->SeenTables.end(), tableId);
          if (tableIt != service->SeenTables.end())
          {
            service->SeenTables.erase(tableIt);
          }
        }

        RemoveExpiredEntries(service->UnseenSegments,
                              false,
                              tableId,
                              section.version_number,
                              !isSingleTableGroup,
                              lastTableId,
                              erasedTableIds);
        m_unseenSegmentCount -= erasedTableIds.size();
        RemoveExpiredEntries(service->SeenSegments,
                              false,
                              tableId,
                              section.version_number,
                              !isSingleTableGroup,
                              lastTableId,
                              erasedTableIds);
      }

      if (isChange)
      {
        LogDebug(L"EIT DVB: changed, PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, version number = %d, section number = %d, last section number = %d, segment last section number = %hhu, last table ID = 0x%hhx",
                  pid, tableId, serviceId, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber, section.LastSectionNumber,
                  segmentLastSectionNumber, lastTableId);
        service->Events.MarkExpiredRecords(tableId);
        if (m_isReady && m_callBackGrabber != NULL)
        {
          m_callBackGrabber->OnTableChange(PID_EIT_DVB_CALL_BACK, TABLE_ID_EIT_DVB_CALL_BACK);
        }
        m_isReady = false;
      }
      else
      {
        LogDebug(L"EIT DVB: received, PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, version number = %d, section number = %d, last section number = %d, segment last section number = %hhu, last table ID = 0x%hhx",
                  pid, tableId, serviceId, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber, section.LastSectionNumber,
                  segmentLastSectionNumber, lastTableId);
        if (m_callBackGrabber != NULL && !m_isSeen)
        {
          m_callBackGrabber->OnTableSeen(PID_EIT_DVB_CALL_BACK, TABLE_ID_EIT_DVB_CALL_BACK);
        }
        m_isSeen = true;
      }

      if (isPremiereTable)
      {
        unsigned long baseKey = sectionKey & 0xffffff00;
        for (unsigned char s = 0; s <= section.LastSectionNumber; s++)
        {
          service->UnseenSections.push_back(baseKey + s);
          m_unseenSectionCount++;
        }
      }
      else
      {
        vector<unsigned char>::const_iterator tableIt = find(service->SeenTables.begin(),
                                                              service->SeenTables.end(),
                                                              tableId);
        if (tableIt == service->SeenTables.end())
        {
          tableIt = find(service->UnseenTables.begin(), service->UnseenTables.end(), tableId);
          if (tableIt == service->UnseenTables.end())
          {
            unsigned char baseTableId;
            if (isSingleTableGroup)
            {
              baseTableId = tableId;
            }
            else
            {
              baseTableId = tableId & 0xf0;
            }
            for (unsigned char t = baseTableId; t <= lastTableId; t++)
            {
              service->UnseenTables.push_back(t);
              m_unseenTableCount++;
            }
          }

          unsigned long firstSegmentKey = sectionKey & 0xffffff00;
          unsigned long lastSegmentKey = firstSegmentKey | section.LastSectionNumber;
          for (unsigned long s = firstSegmentKey; s <= lastSegmentKey; s += 8)
          {
            vector<unsigned long>::const_iterator segmentIt = find(service->UnseenSegments.begin(),
                                                                    service->UnseenSegments.end(),
                                                                    s);
            if (segmentIt == service->UnseenSegments.end())
            {
              service->UnseenSegments.push_back(s);
              m_unseenSegmentCount++;
            }
          }
        }

        unsigned long lastSectionInSegmentKey = segmentKey | segmentLastSectionNumber;
        for (unsigned long s = segmentKey; s <= lastSectionInSegmentKey; s++)
        {
          service->UnseenSections.push_back(s);
          m_unseenSectionCount++;
        }
      }
      sectionIt = find(service->UnseenSections.begin(), service->UnseenSections.end(), sectionKey);
    }
    else
    {
      //LogDebug(L"EIT DVB: new section, PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, version number = %d, section number = %d",
      //          pid, tableId, serviceId, transportStreamId,
      //          originalNetworkId, section.version_number,
      //          section.SectionNumber);
    }

    unsigned short pointer;                                   // points to the first byte in the event loop
    unsigned short endOfSection = section.section_length - 1; // points to the first byte in the CRC
    unsigned char eventByteCount;
    if (isPremiereTable)
    {
      pointer = 8;
      eventByteCount = 9;
    }
    else
    {
      pointer = 14;
      eventByteCount = 12;
    }
    bool isDishOrBellData = false;
    if (
      pid == PID_EIT_DISH ||
      pid == PID_EIT_BELL_EXPRESSVU ||
      (originalNetworkId >= ORIGINAL_NETWORK_ID_DISH_START && originalNetworkId <= ORIGINAL_NETWORK_ID_DISH_END)
    )
    {
      isDishOrBellData = true;
    }
    while (pointer + eventByteCount - 1 < endOfSection)
    {
      // Create a minimal event for Dish Network and Bell TV; otherwise create
      // a full event. Using smaller events enables us to stay within the 32
      // bit process memory limit.
      CRecordEitEventMinimal* event = NULL;
      if (isDishOrBellData)
      {
        event = new CRecordEitEventMinimal();
      }
      else
      {
        event = new CRecordEitEvent();
      }
      if (event == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate event record, PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, version number = %d, section number = %d",
                  pid, tableId, serviceId, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber);
        return;
      }

      event->TableId = tableId;
      event->OriginalNetworkId = originalNetworkId;
      event->TransportStreamId = transportStreamId;
      event->ServiceId = serviceId;

      map<unsigned long long, vector<unsigned long long>*> premiereShowings;  // ONID [16 bits] | TSID [16 bits] | SID [16 bits] => [ epoch ]
      if (!DecodeEventRecord(data, pointer, endOfSection, *event, premiereShowings))
      {
        LogDebug(L"EIT DVB: invalid section, PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, version number = %d, section number = %d, event ID = %llu",
                  pid, tableId, serviceId, transportStreamId,
                  originalNetworkId, section.version_number,
                  section.SectionNumber, event->EventId);
        delete event;
        return;
      }

      if (isPremiereTable)
      {
        CRecordEitEvent* eventFull = dynamic_cast<CRecordEitEvent*>(event);
        if (eventFull == NULL)
        {
          LogDebug(L"EIT DVB: unexpected Premiere data");
          map<unsigned long long, vector<unsigned long long>*>::iterator premiereEventIt = premiereShowings.begin();
          for ( ; premiereEventIt != premiereShowings.end(); premiereEventIt++)
          {
            if (premiereEventIt->second != NULL)
            {
              delete premiereEventIt->second;
            }
          }
          premiereShowings.clear();
        }
        else
        {
          CreatePremiereEvents(*eventFull, premiereShowings);
        }
        delete event;
      }
      else
      {
        service->Events.AddOrUpdateRecord((IRecord**)&event, NULL);
      }
    }

    if (pointer != endOfSection)
    {
      LogDebug(L"EIT DVB: section parsing error, pointer = %hu, end of section = %hu, PID = %d, table ID = 0x%x, service ID = %hu, TSID = %hu, ONID = %hu, version number = %d, section number = %d",
                pointer, endOfSection, pid, tableId, serviceId,
                transportStreamId, originalNetworkId, section.version_number,
                section.SectionNumber);
      return;
    }

    service->SeenSections.push_back(sectionKey);
    service->UnseenSections.erase(sectionIt);
    m_unseenSectionCount--;
    if (!isPremiereTable)
    {
      vector<unsigned char>::const_iterator tableIt = find(service->UnseenTables.begin(),
                                                            service->UnseenTables.end(),
                                                            tableId);
      if (tableIt != service->UnseenTables.end())
      {
        service->UnseenTables.erase(tableIt);
        m_unseenTableCount--;
        service->SeenTables.push_back(tableId);
      }

      vector<unsigned long>::const_iterator segmentIt = find(service->UnseenSegments.begin(),
                                                              service->UnseenSegments.end(),
                                                              segmentKey);
      if (segmentIt != service->UnseenSegments.end())
      {
        service->UnseenSegments.erase(segmentIt);
        m_unseenSegmentCount--;
        service->SeenSegments.push_back(segmentKey);
      }
    }
    if (m_unseenTableCount == 0 && m_unseenSegmentCount == 0 && m_unseenSectionCount == 0)
    {
      // We can't assume that we've seen all sections yet, because sections for
      // another service, transport stream and/or network may not have been
      // received.
      m_completeTime = clock();
    }
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in OnNewSection()");
  }
}

void CParserEitDvb::PrivateReset(bool removeFreesatDecoders)
{
  CEnterCriticalSection lock(m_section);

  AddOrResetDecoder(PID_EIT_DVB, m_enableCrcCheck);
  AddOrResetDecoder(PID_EIT_VIASAT_SWEDEN, m_enableCrcCheck);
  AddOrResetDecoder(PID_EIT_DISH, m_enableCrcCheck);
  AddOrResetDecoder(PID_EIT_MULTICHOICE, m_enableCrcCheck);
  AddOrResetDecoder(PID_EIT_BELL_EXPRESSVU, m_enableCrcCheck);
  AddOrResetDecoder(PID_EIT_PREMIERE_DIREKT, m_enableCrcCheck);
  AddOrResetDecoder(PID_EIT_PREMIERE_SELECT, m_enableCrcCheck);
  AddOrResetDecoder(PID_EIT_PREMIERE_SPORT, m_enableCrcCheck);
  if (removeFreesatDecoders)
  {
    ResetFreesatGrabState();
  }
  else
  {
    if (m_freesatPidEitPf != 0)
    {
      AddOrResetDecoder(m_freesatPidEitPf, m_enableCrcCheck);
    }
    if (m_freesatPidEitSchedule != 0)
    {
      AddOrResetDecoder(m_freesatPidEitSchedule, m_enableCrcCheck);
    }
  }

  map<unsigned long long, CRecordEitService*>::iterator serviceIt = m_services.begin();
  for ( ; serviceIt != m_services.end(); serviceIt++)
  {
    if (serviceIt->second != NULL)
    {
      delete serviceIt->second;
      serviceIt->second = NULL;
    }
  }
  m_services.clear();

  m_isSeen = false;
  m_isReady = false;
  m_unseenTableCount = 0;
  m_unseenSegmentCount = 0;
  m_unseenSectionCount = 0;

  m_currentService = NULL;
  m_currentServiceIndex = 0xffff;
  m_currentEvent = NULL;
  m_currentEventIndex = 0;
  m_currentEventText = NULL;
  m_currentEventTextIndex = 0;
}

bool CParserEitDvb::AddOrResetDecoder(unsigned short pid, bool enableCrcCheck)
{
  bool added = false;
  CSectionDecoder* decoder = NULL;
  map<unsigned short, CSectionDecoder*>::const_iterator it = m_decoders.find(pid);
  if (it == m_decoders.end() || it->second == NULL)
  {
    decoder = new CSectionDecoder();
    if (decoder == NULL)
    {
      LogDebug(L"EIT DVB: failed to allocate section decoder for PID %hu", pid);
      return false;
    }
    decoder->SetPid(pid);
    decoder->SetCallBack(this);
    m_decoders[pid] = decoder;
    added = true;
  }
  else
  {
    decoder = it->second;
  }

  decoder->Reset();
  decoder->EnableCrcCheck(enableCrcCheck);
  return added;
}

void CParserEitDvb::ResetFreesatGrabState()
{
  LogDebug(L"EIT DVB: reset Freesat grab state");
  CEnterCriticalSection lock(m_section);
  unsigned short activePids[3];
  unsigned char activePidCount = 0;
  if (m_freesatPidEitPf != 0)
  {
    m_grabPids.erase(m_freesatPidEitPf);
    map<unsigned short, CSectionDecoder*>::iterator decoderIt = m_decoders.find(m_freesatPidEitPf);
    if (decoderIt != m_decoders.end())
    {
      if (decoderIt->second != NULL)
      {
        delete decoderIt->second;
        decoderIt->second = NULL;
      }
      m_decoders.erase(decoderIt);
    }
    activePids[activePidCount++] = m_freesatPidEitPf;
  }

  if (m_freesatPidEitSchedule != 0)
  {
    m_grabPids.erase(m_freesatPidEitSchedule);
    map<unsigned short, CSectionDecoder*>::iterator decoderIt = m_decoders.find(m_freesatPidEitSchedule);
    if (decoderIt != m_decoders.end())
    {
      if (decoderIt->second != NULL)
      {
        delete decoderIt->second;
        decoderIt->second = NULL;
      }
      m_decoders.erase(decoderIt);
    }
    activePids[activePidCount++] = m_freesatPidEitSchedule;
  }

  if (m_freesatPidBat != 0)
  {
    activePids[activePidCount++] = m_freesatPidBat;
  }
  if (m_freesatPidNit != 0)
  {
    activePids[activePidCount++] = m_freesatPidNit;
  }
  if (m_freesatPidSdt != 0)
  {
    activePids[activePidCount++] = m_freesatPidSdt;
  }

  if (m_grabFreesat && m_callBackPidConsumer != NULL && activePidCount != 0)
  {
    m_callBackPidConsumer->OnPidsNotRequired(activePids, activePidCount, Epg);
  }

  m_freesatPidBat = 0;
  m_freesatPidEitPf = 0;
  m_freesatPidEitSchedule = 0;
  m_freesatPidNit = 0;
  m_freesatPidSdt = 0;
}

template<class T> void CParserEitDvb::RemoveExpiredEntries(vector<T>& set,
                                                            bool isTableIdSet,
                                                            unsigned char sectionTableId,
                                                            unsigned char sectionVersionNumber,
                                                            bool isTableFromGroup,
                                                            unsigned char lastValidTableId,
                                                            vector<unsigned char>& erasedTableIds)
{
  vector<T>::const_iterator setIt = set.begin();
  while (setIt != set.end())
  {
    unsigned long key = *setIt;
    unsigned char tableId;
    unsigned char versionNumber = 0;
    if (!isTableIdSet)
    {
      tableId = (unsigned char)(key >> 16);
      versionNumber = (unsigned char)((key & 0xff00) >> 8);
    }
    else
    {
      tableId = (unsigned char)key;
    }

    if (
      !isTableIdSet &&
      tableId == sectionTableId &&
      versionNumber != sectionVersionNumber
    )
    {
      setIt = set.erase(setIt);
      erasedTableIds.push_back(tableId);
    }
    else if (
      isTableFromGroup &&
      (tableId & 0xf0) == (sectionTableId & 0xf0) &&
      tableId > lastValidTableId
    )
    {
      setIt = set.erase(setIt);
      erasedTableIds.push_back(tableId);
    }
    else
    {
      setIt++;
    }
  }
}

void CParserEitDvb::CreatePremiereEvents(CRecordEitEvent& eventTemplate,
                                          map<unsigned long long, vector<unsigned long long>*>& premiereShowings)
{
  // Create one event per showing.
  bool isPreviouslyShown = false;
  unsigned char subId = 0;
  map<unsigned long long, vector<unsigned long long>*>::iterator it = premiereShowings.begin();
  for ( ; it != premiereShowings.end(); it++)
  {
    if (it->second == NULL)
    {
      continue;
    }

    unsigned short originalNetworkId = (unsigned short)(it->first >> 32);
    unsigned short transportStreamId = (it->first >> 16) & 0xffff;
    unsigned short serviceId = it->first & 0xffff;
    CRecordEitService* service = GetOrCreateService(true,
                                                    originalNetworkId,
                                                    transportStreamId,
                                                    serviceId,
                                                    false);
    if (service == NULL)
    {
      delete it->second;
      it->second = NULL;
      continue;
    }

    vector<unsigned long long>* serviceShowings = it->second;
    vector<unsigned long long>::const_iterator showingIt = serviceShowings->begin();
    for ( ; showingIt != serviceShowings->end(); showingIt++)
    {
      CRecordEitEvent* e = new CRecordEitEvent();
      if (e == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate Premiere event record, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, index = %hhu",
                  eventTemplate.TableId, serviceId, transportStreamId,
                  originalNetworkId, eventTemplate.EventId, subId);
        subId++;
        continue;
      }

      e->TableId = eventTemplate.TableId;
      e->OriginalNetworkId = originalNetworkId;
      e->TransportStreamId = transportStreamId;
      e->ServiceId = serviceId;
      e->EventId = (eventTemplate.EventId << 8) | subId++;
      e->StartDateTime = *showingIt;
      e->Duration = eventTemplate.Duration;
      e->RunningStatus = eventTemplate.RunningStatus;
      e->FreeCaMode = eventTemplate.FreeCaMode;
      e->ReferenceServiceId = eventTemplate.ReferenceServiceId;
      e->ReferenceEventId = eventTemplate.ReferenceEventId;
      e->IsHighDefinition = eventTemplate.IsHighDefinition;
      e->IsStandardDefinition = eventTemplate.IsStandardDefinition;
      e->IsThreeDimensional = eventTemplate.IsThreeDimensional;
      e->IsPreviouslyShown = isPreviouslyShown;
      e->AudioLanguages = eventTemplate.AudioLanguages;
      e->SubtitlesLanguages = eventTemplate.SubtitlesLanguages;
      e->DvbContentTypeIds = eventTemplate.DvbContentTypeIds;   // vector copy
      e->DvbParentalRatings = eventTemplate.DvbParentalRatings; // map copy
      e->StarRating = eventTemplate.StarRating;
      e->MpaaClassification = eventTemplate.MpaaClassification;
      e->DishBevAdvisories = eventTemplate.DishBevAdvisories;
      e->VchipRating = eventTemplate.VchipRating;

      CopyString(eventTemplate.SeriesId, &(e->SeriesId), L"a Premiere event series ID");
      CopyString(eventTemplate.EpisodeId, &(e->EpisodeId), L"a Premiere event episode ID");

      map<unsigned long, CRecordEitEventText*>::const_iterator textIt = eventTemplate.Texts.begin();
      for ( ; textIt != eventTemplate.Texts.end(); textIt++)
      {
        CRecordEitEventText* text = textIt->second;
        if (text == NULL)
        {
          continue;
        }

        CRecordEitEventText* t = new CRecordEitEventText();
        if (t == NULL)
        {
          LogDebug(L"EIT DVB: failed to allocate Premiere text record, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, event index = %hhu, language = %S",
                    eventTemplate.TableId, serviceId, transportStreamId,
                    originalNetworkId, eventTemplate.EventId, subId,
                    (char*)&(text->Language));
          continue;
        }

        t->Language = text->Language;
        CopyString(text->Title, &(t->Title), L"a Premiere text title");
        CopyString(text->DescriptionShort,
                    &(t->DescriptionShort),
                    L"a Premiere text short description");
        CopyString(text->DescriptionExtended,
                    &(t->DescriptionExtended),
                    L"a Premiere text extended description");

        map<unsigned short, CRecordEitEventDescriptionItem*>::const_iterator itemIt = text->DescriptionItems.begin();
        for ( ; itemIt != text->DescriptionItems.end(); itemIt++)
        {
          CRecordEitEventDescriptionItem* item = itemIt->second;
          if (item == NULL)
          {
            continue;
          }

          CRecordEitEventDescriptionItem* di = new CRecordEitEventDescriptionItem();
          if (di == NULL)
          {
            LogDebug(L"EIT DVB: failed to allocate Premiere description item record, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, event index = %hhu, language = %S, item descriptor number = %hhu, item index = %hhu",
                      eventTemplate.TableId, serviceId, transportStreamId,
                      originalNetworkId, eventTemplate.EventId, subId,
                      (char*)&(text->Language), item->DescriptorNumber,
                      item->Index);
            continue;
          }

          di->DescriptorNumber = item->DescriptorNumber;
          di->Index = item->Index;
          CopyString(item->Description,
                      &(di->Description),
                      L"a Premiere description item description");
          CopyString(item->Text, &(di->Text), L"a Premiere description item");

          t->DescriptionItems[itemIt->first] = di;
        }

        e->Texts[textIt->first] = t;
      }

      service->Events.AddOrUpdateRecord((IRecord**)&e, NULL);
      isPreviouslyShown = true;
    }

    delete serviceShowings;
    it->second = NULL;
  }
  premiereShowings.clear();
}

CParserEitDvb::CRecordEitService* CParserEitDvb::GetOrCreateService(bool isPremiereService,
                                                                    unsigned short originalNetworkId,
                                                                    unsigned short transportStreamId,
                                                                    unsigned short serviceId,
                                                                    bool doNotCreate)
{
  unsigned long long key = 0;
  if (isPremiereService)
  {
    key = ((unsigned long long)1 << 48) | serviceId;  // ONID and TSID are not set yet
  }
  else
  {
    key = ((unsigned long long)originalNetworkId << 32) | (transportStreamId << 16) | serviceId;
  }

  map<unsigned long long, CRecordEitService*>::const_iterator it = m_services.find(key);
  if (it != m_services.end())
  {
    return it->second;
  }
  if (doNotCreate)
  {
    return NULL;
  }

  CRecordEitService* service = new CRecordEitService();
  if (service == NULL)
  {
    LogDebug(L"EIT DVB: failed to allocate service record, is Premiere service = %d, ONID = %hu, TSID = %hu, service ID = %hu",
              isPremiereService, originalNetworkId, transportStreamId,
              serviceId);
    return NULL;
  }

  service->IsPremiereService = isPremiereService;
  service->OriginalNetworkId = originalNetworkId;
  service->TransportStreamId = transportStreamId;
  service->ServiceId = serviceId;
  m_services[key] = service;
  return service;
}

CParserEitDvb::CRecordEitEventText* CParserEitDvb::GetOrCreateText(CRecordEitEventMinimal& event,
                                                                    unsigned long language)
{
  map<unsigned long, CRecordEitEventText*>::const_iterator it = event.Texts.find(language);
  if (it != event.Texts.end() && it->second != NULL)
  {
    return it->second;
  }

  CRecordEitEventText* t = new CRecordEitEventText();
  if (t == NULL)
  {
    LogDebug(L"EIT DVB: failed to allocate text record, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, language = %S",
              event.TableId, event.ServiceId, event.TransportStreamId,
              event.OriginalNetworkId, event.EventId, (char*)&language);
    return NULL;
  }
  t->Language = language;
  event.Texts[language] = t;
  return t;
}

void CParserEitDvb::CreateDescriptionItem(CRecordEitEventMinimal& event,
                                          unsigned long language,
                                          unsigned char index,
                                          const char* description,
                                          char* text)
{
  CRecordEitEventText* eventText = GetOrCreateText(event, language);
  if (eventText == NULL)
  {
    delete[] text;
    return;
  }

  CRecordEitEventDescriptionItem* item = new CRecordEitEventDescriptionItem();
  if (item == NULL)
  {
    LogDebug(L"EIT DVB: failed to allocate description item record, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, language = %S, index = %hhu",
              event.TableId, event.ServiceId, event.TransportStreamId,
              event.OriginalNetworkId, event.EventId, (char*)&language, index);
    delete[] text;
    return;
  }
  item->DescriptorNumber = 0;
  item->Index = index;
  CopyString(description, &(item->Description), L"an event description item description");
  item->Text = text;

  // This should never happen, but just in case: double-check that we haven't
  // got an existing item with the same key.
  map<unsigned short, CRecordEitEventDescriptionItem*>::const_iterator existingItemIt = eventText->DescriptionItems.find(item->GetKey());
  if (existingItemIt != eventText->DescriptionItems.end())
  {
    LogDebug(L"EIT DVB: duplicate event description item, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, language = %S, descriptor number = %hhu, item index = %hhu",
              event.TableId, event.ServiceId, event.TransportStreamId,
              event.OriginalNetworkId, event.EventId, (char*)&language,
              item->DescriptorNumber, item->Index);
    delete item;  // also deletes the text
    return;
  }
  eventText->DescriptionItems[item->GetKey()] = item;
}

void CParserEitDvb::CopyString(const char* input, char** output, wchar_t* debug)
{
  if (input != NULL)
  {
    unsigned long byteCount = strlen(input) + 1;
    *output = new char[byteCount];
    if (*output == NULL)
    {
      LogDebug(L"EIT DVB: failed to allocate %lu byte(s) for %s",
                byteCount, debug);
    }
    else
    {
      strncpy(*output, input, byteCount);
      (*output)[byteCount - 1] = NULL;
    }
  }
}

bool CParserEitDvb::DecodeEventRecord(unsigned char* sectionData,
                                      unsigned short& pointer,
                                      unsigned short endOfSection,
                                      CRecordEitEventMinimal& event,
                                      map<unsigned long long, vector<unsigned long long>*>& premiereShowings)
{
  try
  {
    if (event.TableId == TABLE_ID_EIT_PREMIERE && event.OriginalNetworkId == 0)
    {
      if (pointer + 9 > endOfSection)
      {
        LogDebug(L"EIT DVB: invalid Premiere event record, pointer = %hu, end of section = %hu",
                  pointer, endOfSection);
        return false;
      }

      event.EventId = (sectionData[pointer] << 24) | (sectionData[pointer + 1] << 16) | (sectionData[pointer + 2] << 8) | sectionData[pointer + 3];
      pointer += 4;
    }
    else
    {
      if (pointer + 12 > endOfSection)
      {
        LogDebug(L"EIT DVB: invalid event record, pointer = %hu, end of section = %hu",
                  pointer, endOfSection);
        return false;
      }

      event.EventId = (sectionData[pointer] << 8) | sectionData[pointer + 1];
      pointer += 2;

      unsigned short startDateMjd = (sectionData[pointer] << 8) | sectionData[pointer + 1];
      pointer += 2;
      unsigned long startTimeBcd = (sectionData[pointer] << 16) | (sectionData[pointer + 1] << 8) | sectionData[pointer + 2];
      pointer += 3;
      event.StartDateTime = DecodeDateTime(startDateMjd, startTimeBcd);
    }

    // Convert BCD HH:MM:SS to minutes.
    event.Duration = ((sectionData[pointer] >> 4) * 10) + (sectionData[pointer] & 0x0f);
    pointer++;
    event.Duration *= 60;
    event.Duration += ((sectionData[pointer] >> 4) * 10) + (sectionData[pointer] & 0x0f);
    pointer += 2; // Ignore the seconds byte.

    event.RunningStatus = sectionData[pointer] >> 5;
    event.FreeCaMode = (sectionData[pointer] & 0x10) != 0;

    unsigned short descriptorsLoopLength = ((sectionData[pointer] & 0xf) << 8) | sectionData[pointer + 1];
    pointer += 2;

    //LogDebug(L"EIT DVB: event ID = %llu, start date/time = %llu, duration = %hu m, running status = %hhu, free CA mode = %d, descriptors loop length = %hu",
    //          event.Id, event.StartDateTime, event.Duration,
    //          event.RunningStatus, event.FreeCaMode, descriptorsLoopLength);

    unsigned short endOfDescriptorLoop = pointer + descriptorsLoopLength;
    if (endOfDescriptorLoop > endOfSection)
    {
      LogDebug(L"EIT DVB: invalid event record, descriptors loop length = %hu, pointer = %hu, end of section = %hu",
                descriptorsLoopLength, pointer, endOfSection);
      return false;
    }

    return DecodeEventDescriptors(sectionData,
                                  pointer,
                                  endOfDescriptorLoop,
                                  event,
                                  premiereShowings);
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeEventRecord()");
  }
  return false;
}

bool CParserEitDvb::DecodeEventDescriptors(unsigned char* sectionData,
                                            unsigned short& pointer,
                                            unsigned short endOfDescriptorLoop,
                                            CRecordEitEventMinimal& event,
                                            map<unsigned long long, vector<unsigned long long>*>& premiereShowings)
{
  unsigned long privateDataSpecifier = 0;
  bool result = true;
  while (pointer + 1 < endOfDescriptorLoop)
  {
    unsigned char tag = sectionData[pointer++];
    unsigned char length = sectionData[pointer++];
    unsigned short endOfDescriptor = pointer + length;
    //LogDebug(L"EIT DVB: descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu",
    //          tag, length, pointer);
    if (endOfDescriptor > endOfDescriptorLoop)
    {
      LogDebug(L"EIT DVB: invalid event record, descriptor length = %hhu, pointer = %hu, end of descriptor loop = %hu, tag = 0x%hhx",
                length, pointer, endOfDescriptorLoop, tag);
      result = false;
      break;
    }

    if (tag == 0x4d)  // short event descriptor
    {
      unsigned long language;
      char* title = NULL;
      char* description = NULL;
      result = DecodeShortEventDescriptor(&sectionData[pointer],
                                          length,
                                          language,
                                          &title,
                                          &description);
      if (result)
      {
        CRecordEitEventText* text = GetOrCreateText(event, language);
        if (text == NULL)
        {
          if (title != NULL)
          {
            delete[] title;
          }
          if (description != NULL)
          {
            delete[] description;
          }
          result = false;
          break;
        }

        if (title != NULL)
        {
          if (text->Title != NULL)
          {
            if (strcmp(text->Title, title) != 0)
            {
              LogDebug(L"EIT DVB: replacing title, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, language = %S, current = %S, new = %S",
                        event.TableId, event.ServiceId,
                        event.TransportStreamId, event.OriginalNetworkId,
                        event.EventId, (char*)&language, text->Title, title);
            }
            delete[] text->Title;
          }
          text->Title = title;
        }
        if (description != NULL)
        {
          if (text->DescriptionShort != NULL)
          {
            if (strcmp(text->DescriptionShort, description) != 0)
            {
              LogDebug(L"EIT DVB: replacing short description, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, language = %S, current = %S, new = %S",
                        event.TableId, event.ServiceId,
                        event.TransportStreamId, event.OriginalNetworkId,
                        event.EventId, (char*)&language,
                        text->DescriptionShort, description);
            }
            delete[] text->DescriptionShort;
          }
          text->DescriptionShort = description;
        }
      }
    }
    else if (tag == 0x4e) // extended event descriptor
    {
      unsigned long language;
      char* description = NULL;
      vector<CRecordEitEventDescriptionItem*> items;
      result = DecodeExtendedEventDescriptor(&sectionData[pointer],
                                              length,
                                              language,
                                              items,
                                              &description);
      if (result && false)
      {
        CRecordEitEventText* text = GetOrCreateText(event, language);
        if (text == NULL)
        {
          if (description != NULL)
          {
            delete[] description;
          }
          vector<CRecordEitEventDescriptionItem*>::iterator it = items.begin();
          for ( ; it != items.end(); it++)
          {
            CRecordEitEventDescriptionItem* item = *it;
            if (item != NULL)
            {
              delete item;
              *it = NULL;
            }
          }
          result = false;
          break;
        }

        if (description != NULL)
        {
          if (text->DescriptionExtended == NULL)
          {
            text->DescriptionExtended = description;
          }
          else
          {
            // Count the number of encoding bytes at the start of the
            // description part. These have to be removed.
            unsigned char offset = 0;
            for (unsigned char i = 0; i < 3; i++)
            {
              char c = description[i];
              if (c != 0 && c < 0x20)
              {
                offset++;
                continue;
              }
              break;
            }
            unsigned long byteCount = strlen(description);
            if (offset < byteCount)
            {
              byteCount -= offset;
              byteCount += strlen(text->DescriptionExtended);
              byteCount += 1;  // NULL termination
              char* fullDescription = new char[byteCount];
              if (fullDescription == NULL)
              {
                LogDebug(L"EIT DVB: failed to allocate %lu byte(s) for an extended description combination, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, language = %S",
                          byteCount, event.TableId, event.ServiceId,
                          event.TransportStreamId, event.OriginalNetworkId,
                          event.EventId, (char*)&language);
                delete[] description;
              }
              else
              {
                strncpy(fullDescription, text->DescriptionExtended, byteCount);
                strncat(fullDescription, description, byteCount - strlen(fullDescription));
                fullDescription[byteCount - 1] = NULL;
                delete[] text->DescriptionExtended;
                delete[] description;
                text->DescriptionExtended = fullDescription;
              }
            }
          }
        }

        vector<CRecordEitEventDescriptionItem*>::const_iterator it = items.begin();
        for ( ; it != items.end(); it++)
        {
          CRecordEitEventDescriptionItem* item = *it;
          if (item == NULL)
          {
            continue;
          }
          map<unsigned short, CRecordEitEventDescriptionItem*>::const_iterator existingItemIt = text->DescriptionItems.find(item->GetKey());
          if (existingItemIt != text->DescriptionItems.end())
          {
            LogDebug(L"EIT DVB: duplicate event description item, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, language = %S, descriptor number = %hhu, item index = %hhu",
                      event.TableId, event.ServiceId, event.TransportStreamId,
                      event.OriginalNetworkId, event.EventId, (char*)&language,
                      item->DescriptorNumber, item->Index);
            delete item;
            continue;
          }
          text->DescriptionItems[item->GetKey()] = item;
        }
      }
    }
    else if (tag == 0x4f) // time-shifted event descriptor
    {
      try
      {
        CRecordEitEvent& eventFull = dynamic_cast<CRecordEitEvent&>(event);
        unsigned short referenceEventId;
        result = DecodeTimeShiftedEventDescriptor(&sectionData[pointer],
                                                  length,
                                                  eventFull.ReferenceServiceId,
                                                  referenceEventId);
        eventFull.ReferenceEventId = referenceEventId;
      }
      catch (bad_cast&)
      {
        LogDebug(L"EIT DVB: unexpected time-shifted event descriptor");
      }
    }
    else if (tag == 0x50) // component descriptor
    {
      bool isAudio;
      bool isSubtitles;
      bool isHighDefinition;
      bool isStandardDefinition;
      bool isThreeDimensional;
      unsigned long language;
      result = DecodeComponentDescriptor(&sectionData[pointer],
                                          length,
                                          isAudio,
                                          isSubtitles,
                                          isHighDefinition,
                                          isStandardDefinition,
                                          isThreeDimensional,
                                          language);
      if (result)
      {
        try
        {
          CRecordEitEvent& eventFull = dynamic_cast<CRecordEitEvent&>(event);
          eventFull.IsHighDefinition |= isHighDefinition;
          eventFull.IsStandardDefinition |= isStandardDefinition;
          eventFull.IsThreeDimensional |= isThreeDimensional;
          if (
            isAudio &&
            language != 0 &&
            find(eventFull.AudioLanguages.begin(), eventFull.AudioLanguages.end(), language) == eventFull.AudioLanguages.end()
          )
          {
            eventFull.AudioLanguages.push_back(language);
          }
          else if (
            isSubtitles &&
            language != 0 &&
            find(eventFull.SubtitlesLanguages.begin(), eventFull.SubtitlesLanguages.end(), language) == eventFull.SubtitlesLanguages.end()
          )
          {
            eventFull.SubtitlesLanguages.push_back(language);
          }
        }
        catch (bad_cast&)
        {
          LogDebug(L"EIT DVB: unexpected component descriptor");
        }
      }
    }
    else if (tag == 0x54) // content descriptor
    {
      result = DecodeContentDescriptor(&sectionData[pointer], length, event.DvbContentTypeIds);
    }
    else if (tag == 0x55) // parental rating descriptor
    {
      try
      {
        CRecordEitEvent& eventFull = dynamic_cast<CRecordEitEvent&>(event);
        result = DecodeParentalRatingDescriptor(&sectionData[pointer],
                                                length,
                                                eventFull.DvbParentalRatings);
      }
      catch (bad_cast&)
      {
        LogDebug(L"EIT DVB: unexpected parental rating descriptor");
      }
    }
    else if (tag == 0x5f) // private data specifier descriptor
    {
      result = DecodePrivateDataSpecifierDescriptor(&sectionData[pointer],
                                                    length,
                                                    privateDataSpecifier);
    }
    else if (tag == 0x76) // content identifier descriptor
    {
      map<unsigned char, char*> crids;
      result = DecodeContentIdentifierDescriptor(&sectionData[pointer], length, crids);
      if (result)
      {
        event.AreSeriesAndEpisodeIdsCrids = true;

        map<unsigned char, char*>::const_iterator it = crids.begin();
        for ( ; it != crids.end(); it++)
        {
          if (it->second == NULL)
          {
            continue;
          }
          if (it->first == 1 || it->first == 0x31)
          {
            if (event.EpisodeId != NULL)
            {
              if (strcmp(event.EpisodeId, it->second) != 0)
              {
                LogDebug(L"EIT DVB: replacing episode ID, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, current = %S, new = %S",
                          event.TableId, event.ServiceId,
                          event.TransportStreamId, event.OriginalNetworkId,
                          event.EventId, event.EpisodeId, it->second);
              }
              delete[] event.EpisodeId;
            }
            event.EpisodeId = it->second;
          }
          else if (it->first == 2 || it->first == 0x32)
          {
            if (event.SeriesId != NULL)
            {
              if (strcmp(event.SeriesId, it->second) != 0)
              {
                LogDebug(L"EIT DVB: replacing series ID, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, current = %S, new = %S",
                          event.TableId, event.ServiceId,
                          event.TransportStreamId, event.OriginalNetworkId,
                          event.EventId, event.SeriesId, it->second);
              }
              delete[] event.SeriesId;
            }
            event.SeriesId = it->second;
          }
          else
          {
            delete[] it->second;
          }
        }
      }
    }
    else if (tag == 0x7f) // DVB extended descriptors
    {
      if (length < 1)
      {
        LogDebug(L"EIT DVB: invalid event record, descriptor length = %hhu, pointer = %hu, end of descriptor loop = %hu, tag = 0x%hhx",
                  length, pointer, endOfDescriptorLoop, tag);
        result = false;
      }
      else
      {
        unsigned char tagExtension = sectionData[pointer];
        if (tagExtension == 0x10) // video depth range descriptor
        {
          try
          {
            CRecordEitEvent& eventFull = dynamic_cast<CRecordEitEvent&>(event);
            eventFull.IsThreeDimensional = true;
          }
          catch (bad_cast&)
          {
            LogDebug(L"EIT DVB: unexpected video depth range descriptor");
          }
        }
      }
    }
    // There is no private data specifier for Dish descriptors, so we have to
    // determine scope with ONID. These are the ONIDs for EchoStar networks.
    // http://www.dvbservices.com/identifiers/original_network_id&tab=table
    else if (
      event.OriginalNetworkId >= ORIGINAL_NETWORK_ID_DISH_START &&
      event.OriginalNetworkId <= ORIGINAL_NETWORK_ID_DISH_END
    )
    {
      if (tag == 0x89)  // Dish/BEV rating descriptor
      {
        result = DecodeDishBevRatingDescriptor(&sectionData[pointer],
                                                length,
                                                event.StarRating,
                                                event.MpaaClassification,
                                                event.DishBevAdvisories);
      }
      else if (tag == 0x91 || tag == 0x92)  // Dish event name/description descriptor
      {
        char* text = NULL;
        result = DecodeDishTextDescriptor(&sectionData[pointer], length, sectionData[0], &text);
        if (result && text != NULL)
        {
          CRecordEitEventText* eventText = GetOrCreateText(event, LANG_ENG);
          if (eventText == NULL)
          {
            delete[] text;
            result = false;
            break;
          }

          if (tag == 0x91)
          {
            if (eventText->Title != NULL)
            {
              if (strcmp(eventText->Title, text) != 0)
              {
                LogDebug(L"EIT DVB: replacing title, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, current = %S, new = %S",
                          event.TableId, event.ServiceId,
                          event.TransportStreamId, event.OriginalNetworkId,
                          event.EventId, eventText->Title, text);
              }
              delete[] eventText->Title;
            }
            eventText->Title = text;
          }
          else
          {
            if (eventText->DescriptionShort != NULL)
            {
              if (strcmp(eventText->DescriptionShort, text) != 0)
              {
                LogDebug(L"EIT DVB: replacing short description, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, current = %S, new = %S",
                          event.TableId, event.ServiceId,
                          event.TransportStreamId, event.OriginalNetworkId,
                          event.EventId, eventText->DescriptionShort, text);
              }
              delete[] eventText->DescriptionShort;
            }
            eventText->DescriptionShort = text;
          }
        }
      }
      else if (tag == 0x94) // Dish episode information descriptor
      {
        char* information = NULL;
        result = DecodeDishEpisodeInformationDescriptor(&sectionData[pointer],
                                                        length,
                                                        sectionData[0],
                                                        &information);
        if (result && information != NULL)
        {
          CreateDescriptionItem(event,
                                LANG_ENG,
                                ITEM_INDEX_DISH_EPISODE_INFORMATION,
                                "Dish episode info",
                                information);
        }
      }
      else if (tag == 0x95) // Dish V-Chip descriptor
      {
        result = DecodeDishVchipDescriptor(&sectionData[pointer],
                                            length,
                                            event.VchipRating,
                                            event.DishBevAdvisories);
      }
      else if (tag == 0x96) // Dish/BEV series descriptor
      {
        char* seriesId = NULL;
        char* episodeId = NULL;
        result = DecodeDishBevSeriesDescriptor(&sectionData[pointer],
                                                length,
                                                &seriesId,
                                                &episodeId,
                                                event.IsPreviouslyShown);
        if (result)
        {
          event.AreSeriesAndEpisodeIdsCrids = false;

          if (seriesId != NULL)
          {
            if (event.SeriesId != NULL)
            {
              if (strcmp(event.SeriesId, seriesId) != 0)
              {
                LogDebug(L"EIT DVB: replacing series ID, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, current = %S, new = %S",
                          event.TableId, event.ServiceId,
                          event.TransportStreamId, event.OriginalNetworkId,
                          event.EventId, event.SeriesId, seriesId);
              }
              delete[] event.SeriesId;
            }
            event.SeriesId = seriesId;
          }
          if (episodeId != NULL)
          {
            if (event.EpisodeId != NULL)
            {
              if (strcmp(event.EpisodeId, episodeId) != 0)
              {
                LogDebug(L"EIT DVB: replacing episode ID, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu, current = %S, new = %S",
                          event.TableId, event.ServiceId,
                          event.TransportStreamId, event.OriginalNetworkId,
                          event.EventId, event.EpisodeId, episodeId);
              }
              delete[] event.EpisodeId;
            }
            event.EpisodeId = episodeId;
          }
        }
      }
    }
    else if (privateDataSpecifier == 0xbe)
    {
      if (tag == 0xf0)  // Premiere order information descriptor
      {
        char* orderNumber = NULL;
        char* price = NULL;
        char* phoneNumber = NULL;
        char* smsNumber = NULL;
        char* url = NULL;
        result = DecodePremiereOrderInformationDescriptor(&sectionData[pointer],
                                                          length,
                                                          &orderNumber,
                                                          &price,
                                                          &phoneNumber,
                                                          &smsNumber,
                                                          &url);
        if (result)
        {
          if (orderNumber != NULL)
          {
            CreateDescriptionItem(event,
                                  LANG_GER,
                                  ITEM_INDEX_PREMIERE_ORDER_NUMBER,
                                  "Premiere order number",
                                  orderNumber);
          }
          if (price != NULL)
          {
            CreateDescriptionItem(event,
                                  LANG_GER,
                                  ITEM_INDEX_PREMIERE_ORDER_PRICE,
                                  "Premiere order price",
                                  price);
          }
          if (phoneNumber != NULL)
          {
            CreateDescriptionItem(event,
                                  LANG_GER,
                                  ITEM_INDEX_PREMIERE_ORDER_PHONE_NUMBER,
                                  "Premiere order phone number",
                                  phoneNumber);
          }
          if (smsNumber != NULL)
          {
            CreateDescriptionItem(event,
                                  LANG_GER,
                                  ITEM_INDEX_PREMIERE_ORDER_SMS_NUMBER,
                                  "Premiere order SMS number",
                                  smsNumber);
          }
          if (url != NULL)
          {
            CreateDescriptionItem(event,
                                  LANG_GER,
                                  ITEM_INDEX_PREMIERE_ORDER_URL,
                                  "Premiere order URL",
                                  url);
          }
        }
      }
      else if (tag == 0xf1) // Premiere parent information descriptor
      {
        unsigned char rating;
        char* text = NULL;
        result = DecodePremiereParentInformationDescriptor(&sectionData[pointer],
                                                            length,
                                                            rating,
                                                            &text);
        if (result)
        {
          try
          {
            // The rating is DVB-compatible (age minus 3 years). Assume it
            // applies to Germany, since Premiere is now Sky Deutchland.
            CRecordEitEvent& eventFull = dynamic_cast<CRecordEitEvent&>(event);
            eventFull.DvbParentalRatings[COUNTRY_DEU] = rating;
          }
          catch (bad_cast&)
          {
            LogDebug(L"EIT DVB: unexpected Premiere parent information descriptor");
          }

          if (text != NULL)
          {
            CreateDescriptionItem(event,
                                  LANG_GER,
                                  ITEM_INDEX_PREMIERE_PARENT_TEXT,
                                  "Premiere parent text",
                                  text);
          }
        }
      }
      else if (tag == 0xf2) // Premiere content transmission descriptor
      {
        unsigned short originalNetworkId;
        unsigned short transportStreamId;
        unsigned short serviceId;
        vector<unsigned long long> showings;
        result = DecodePremiereContentTransmissionDescriptor(&sectionData[pointer],
                                                              length,
                                                              originalNetworkId,
                                                              transportStreamId,
                                                              serviceId,
                                                              showings);
        if (result)
        {
          unsigned long long key = ((unsigned long long)originalNetworkId << 32) | ((unsigned long long)transportStreamId << 16) | serviceId;
          map<unsigned long long, vector<unsigned long long>*>::const_iterator it = premiereShowings.find(key);
          vector<unsigned long long>* serviceShowings = NULL;
          if (it == premiereShowings.end())
          {
            serviceShowings = new vector<unsigned long long>();
            if (serviceShowings == NULL)
            {
              LogDebug(L"EIT DVB: failed to allocate Premiere start date/time vector, table ID = 0x%hhx, service ID = %hu, TSID = %hu, ONID = %hu, event ID = %llu",
                        event.TableId, serviceId, transportStreamId,
                        originalNetworkId, event.EventId);
            }
            else
            {
              premiereShowings[key] = serviceShowings;
            }
          }
          else
          {
            serviceShowings = it->second;
          }
          if (serviceShowings != NULL)
          {
            vector<unsigned long long>::const_iterator showingIt = showings.begin();
            for ( ; showingIt != showings.end(); showingIt++)
            {
              serviceShowings->push_back(*showingIt);
            }
          }
        }
      }
    }

    if (!result)
    {
      LogDebug(L"EIT DVB: invalid event record descriptor, tag = 0x%hhx, length = %hhu, pointer = %hu, end of descriptor loop = %hu",
                tag, length, pointer, endOfDescriptorLoop);
      break;
    }

    pointer = endOfDescriptor;
  }

  if (!result)
  {
    map<unsigned long long, vector<unsigned long long>*>::iterator it = premiereShowings.begin();
    for ( ; it != premiereShowings.end(); it++)
    {
      if (it->second != NULL)
      {
        delete it->second;
        it->second = NULL;
      }
    }
    premiereShowings.clear();
  }

  pointer = endOfDescriptorLoop;
  return result;
}

bool CParserEitDvb::DecodeShortEventDescriptor(unsigned char* data,
                                                unsigned char dataLength,
                                                unsigned long& language,
                                                char** eventName,
                                                char** text)
{
  if (dataLength < 5)
  {
    LogDebug(L"EIT DVB: invalid short event descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    language = data[0] | (data[1] << 8) | (data[2] << 16);
    unsigned char eventNameLength = data[3];
    unsigned short pointer = 4;

    if (eventNameLength > 0)
    {
      if (
        pointer + eventNameLength + 1 > dataLength ||
        !CTextUtil::DvbTextToString(&data[pointer], eventNameLength, eventName)
      )
      {
        LogDebug(L"EIT DVB: invalid short event descriptor, descriptor length = %hhu, pointer = %hu, event name length = %hhu",
                  dataLength, pointer, eventNameLength);
        return false;
      }
      if (*eventName == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate a short event descriptor event name");
      }
      pointer += eventNameLength;
    }

    unsigned char textLength = data[pointer++];
    if (textLength > 0)
    {
      if (
        pointer + textLength > dataLength ||
        !CTextUtil::DvbTextToString(&data[pointer], textLength, text)
      )
      {
        LogDebug(L"EIT DVB: invalid short event descriptor, descriptor length = %hhu, pointer = %hu, text length = %hhu",
                  dataLength, pointer, textLength);
        if (*eventName != NULL)
        {
          delete[] *eventName;
          *eventName = NULL;
        }
        return false;
      }
      if (*text == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate a short event descriptor text");
      }
    }

    //LogDebug(L"EIT DVB: short event descriptor, language = %S, event name = %S, text = %S",
    //          (char*)&language, *eventName == NULL ? "" : *eventName,
    //          *text == NULL ? "" : *text);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeShortEventDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeExtendedEventDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned long& language,
                                                  vector<CRecordEitEventDescriptionItem*>& items,
                                                  char** text)
{
  if (dataLength < 6)
  {
    LogDebug(L"EIT DVB: invalid extended event descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char descriptorNumber = data[0] >> 4;
    unsigned char lastDescriptorNumber = data[0] & 0x0f;
    language = data[1] | (data[2] << 8) | (data[3] << 16);
    unsigned char lengthOfItems = data[4];
    //LogDebug(L"EIT DVB: extended event descriptor, descriptor number = %hhu, last descriptor number = %hhu, language = %S, length of items = %hhu",
    //          descriptorNumber, lastDescriptorNumber, (char*)&language,
    //          lengthOfItems);

    unsigned short pointer = 5;
    if (pointer + lengthOfItems + 1 > dataLength)
    {
      LogDebug(L"EIT DVB: invalid extended event descriptor, descriptor length = %hhu, pointer = %hu, length of items = %hhu, descriptor number = %hhu",
                dataLength, pointer, lengthOfItems, descriptorNumber);
      return false;
    }

    unsigned char index = 0;
    unsigned short endOfItems = pointer + lengthOfItems;
    while (pointer + 1 < endOfItems)
    {
      CRecordEitEventDescriptionItem* item = new CRecordEitEventDescriptionItem();
      if (item == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate description item record");
        pointer = endOfItems;
        break;
      }

      item->DescriptorNumber = descriptorNumber;
      item->Index = index++;

      unsigned char itemDescriptionLength = data[pointer++];
      if (itemDescriptionLength > 0)
      {
        if (
          pointer + 1 + itemDescriptionLength > endOfItems ||
          !CTextUtil::DvbTextToString(&data[pointer], itemDescriptionLength, &(item->Description))
        )
        {
          LogDebug(L"EIT DVB: invalid extended event descriptor, descriptor length = %hhu, pointer = %hu, item description length = %hhu, end of items = %hu, descriptor number = %hhu, length of items = %hhu, item index = %hhu",
                    dataLength, pointer, itemDescriptionLength, endOfItems,
                    descriptorNumber, lengthOfItems, items.size());
          delete item;
          vector<CRecordEitEventDescriptionItem*>::iterator it = items.begin();
          for ( ; it != items.end(); it++)
          {
            CRecordEitEventDescriptionItem* i = *it;
            if (i != NULL)
            {
              delete i;
              *it = NULL;
            }
          }
          items.clear();
          return false;
        }

        if (item->Description == NULL)
        {
          LogDebug(L"EIT DVB: failed to allocate extended event descriptor item %llu's description",
                    (unsigned long long)items.size());
        }
        pointer += itemDescriptionLength;
      }

      unsigned char itemLength = data[pointer++];
      if (itemLength > 0)
      {
        if (
          pointer + itemLength > endOfItems ||
          !CTextUtil::DvbTextToString(&data[pointer], itemLength, &(item->Text))
        )
        {
          LogDebug(L"EIT DVB: invalid extended event descriptor, descriptor length = %hhu, pointer = %hu, item length = %hhu, end of items = %hu, descriptor number = %hhu, length of items = %hhu, item index = %hhu",
                    dataLength, pointer, itemLength, endOfItems,
                    descriptorNumber, lengthOfItems, items.size());
          delete item;
          vector<CRecordEitEventDescriptionItem*>::iterator it = items.begin();
          for ( ; it != items.end(); it++)
          {
            CRecordEitEventDescriptionItem* i = *it;
            if (i != NULL)
            {
              delete i;
              *it = NULL;
            }
          }
          items.clear();
          return false;
        }

        if (item->Text == NULL)
        {
          LogDebug(L"EIT DVB: failed to allocate extended event descriptor item %llu's text",
                    (unsigned long long)items.size());
        }
        pointer += itemLength;
      }

      //LogDebug(L"  item, description = %S, text = %S",
      //          item->Description == NULL ? "" : item->Description,
      //          item->Text == NULL ? "" : item->Text);
      items.push_back(item);
    }

    unsigned char textLength = data[pointer];
    if (
      pointer != endOfItems ||
      pointer + 1 + textLength > dataLength ||
      (textLength > 0 && !CTextUtil::DvbTextToString(&data[pointer + 1], textLength, text))
    )
    {
      LogDebug(L"EIT DVB: invalid extended event descriptor, descriptor length = %hhu, pointer = %hu, text length = %hhu, descriptor number = %hhu, length of items = %hhu",
                dataLength, pointer, textLength, descriptorNumber,
                lengthOfItems);
      vector<CRecordEitEventDescriptionItem*>::iterator it = items.begin();
      for ( ; it != items.end(); it++)
      {
        CRecordEitEventDescriptionItem* i = *it;
        if (i != NULL)
        {
          delete i;
          *it = NULL;
        }
      }
      items.clear();
      return false;
    }

    if (textLength > 0 && *text == NULL)
    {
      LogDebug(L"EIT DVB: failed to allocate an extended event descriptor text");
    }
    //LogDebug(L"  %S", *text == NULL ? "" : *text);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeExtendedEventDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeTimeShiftedEventDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      unsigned short& referenceServiceId,
                                                      unsigned short& referenceEventId)
{
  if (dataLength != 4)
  {
    LogDebug(L"EIT DVB: invalid time-shifted event descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    referenceServiceId = (data[0] << 8) | data[1];
    referenceEventId = (data[2] << 8) | data[3];
    //LogDebug(L"EIT DVB: time-shifted event descriptor, reference service ID = %hu, reference event ID = %hu",
    //          referenceServiceId, referenceEventId);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeTimeShiftedEventDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeComponentDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              bool& isAudio,
                                              bool& isSubtitles,
                                              bool& isHighDefinition,
                                              bool& isStandardDefinition,
                                              bool& isThreeDimensional,
                                              unsigned long& language)
{
  if (dataLength < 6)
  {
    LogDebug(L"EIT DVB: invalid component descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    isAudio = false;
    isSubtitles = false;
    isHighDefinition = false;
    isStandardDefinition = false;
    isThreeDimensional = false;

    unsigned char streamContentExt = data[0] >> 4;
    unsigned char streamContent = data[0] & 0x0f;
    unsigned char componentType = data[1];
    unsigned char componentTag = data[2];
    unsigned long iso639LanguageCode = data[3] | (data[4] << 8) | (data[5] << 16);
    // (component description not read)
    //LogDebug(L"EIT DVB: component descriptor, stream content = %hhu, stream content extension = %hhu, component type = %hhu, component tag = %hhu, language = %S",
    //          streamContent, streamContentExt, componentType, componentTag,
    //          (char*)&iso639LanguageCode);

    if (streamContent == 1 || streamContent == 5 || (streamContent == 9 && streamContentExt == 0))
    {
      if (streamContent != 9 && componentType >= 1 && componentType <= 8)
      {
        isStandardDefinition = true;
      }
      else if (streamContent == 9 || (componentType >= 0x09 && componentType <= 0x10))
      {
        isHighDefinition = true;
      }
      if (streamContent == 5)
      {
        if (componentType >= 0x80 && componentType <= 0x83)
        {
          // frame compatible plano-stereoscopic
          // 0x80/0x82 = side by side; 0x81/0x83 = top and bottom
          isHighDefinition = true;
          isThreeDimensional = true;
        }
        else if (componentType == 0x84)
        {
          // service compatible plano-stereoscopic
          isThreeDimensional = true;
        }
      }
    }
    else if (
      streamContent == 2 ||
      streamContent == 4 ||
      (streamContent == 6 && componentType != 0xa0) ||
      streamContent == 7 ||
      (streamContent == 9 && streamContentExt == 1)
    )
    {
      isAudio = true;
      language = iso639LanguageCode;
    }
    else if (
      streamContent == 3 &&
      (
        componentType = 1 ||  // EBU teletext subtitles
        (componentType >= 0x10 && componentType <= 0x15) || // DVB subtitles [normal]
        (componentType >= 0x20 && componentType <= 0x25)    // DVB subtitles [hard of hearing]
      )
    )
    {
      isSubtitles = true;
      language = iso639LanguageCode;
    }
    else if (streamContent == 0xb && streamContentExt == 0xf && componentType == 3)
    {
      // frame compatible plano-stereoscopic top and bottom
      isThreeDimensional = true;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeComponentDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeContentDescriptor(unsigned char* data,
                                            unsigned char dataLength,
                                            vector<unsigned short>& contentTypeIds)
{
  if (dataLength == 0 || dataLength % 2 != 0)
  {
    LogDebug(L"EIT DVB: invalid content descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 1 < dataLength)
    {
      unsigned short contentTypeId = (data[pointer] << 8) | data[pointer + 1];
      contentTypeIds.push_back(contentTypeId);
      //LogDebug(L"EIT DVB: content descriptor, type ID = %hu", contentTypeId);
      pointer += 2;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeContentDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeParentalRatingDescriptor(unsigned char* data,
                                                    unsigned char dataLength,
                                                    map<unsigned long, unsigned char>& ratings)
{
  if (dataLength == 0 || dataLength % 4 != 0)
  {
    LogDebug(L"EIT DVB: invalid parental rating descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 3 < dataLength)
    {
      unsigned long countryCode = data[pointer] | (data[pointer + 1] << 8) | (data[pointer + 2] << 16);
      ratings[countryCode] = data[pointer + 3];
      //LogDebug(L"EIT DVB: parental rating descriptor, country = %S, rating = %hhu",
      //          (char*)&countryCode, data[pointer + 3]);
      pointer += 4;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeParentalRatingDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodePrivateDataSpecifierDescriptor(unsigned char* data,
                                                          unsigned char dataLength,
                                                          unsigned long& privateDataSpecifier)
{
  if (dataLength != 4)
  {
    LogDebug(L"EIT DVB: invalid private data specifier descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    privateDataSpecifier = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
    //LogDebug(L"EIT DVB: private data specifier descriptor, specifier = %lu",
    //          privateDataSpecifier);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodePrivateDataSpecifierDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeContentIdentifierDescriptor(unsigned char* data,
                                                      unsigned char dataLength,
                                                      map<unsigned char, char*>& crids)
{
  // Refer to ETSI TS 102 323.
  if (dataLength == 0)
  {
    LogDebug(L"EIT DVB: invalid content identifier descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned short pointer = 0;
    while (pointer + 1 < dataLength)
    {
      unsigned char cridType = data[pointer] >> 2;
      unsigned char cridLocation = data[pointer++] & 0x3;
      if (cridLocation != 0)  // carried in CIT (not handled) or elsewhere
      {
        LogDebug(L"EIT DVB: content identifier descriptor, descriptor length = %hhu, pointer = %hu, CRID type = %hhu, location = %hhu",
                  dataLength, pointer, cridType, cridLocation);
        if (cridLocation == 1)
        {
          pointer += 2;
        }
        continue;
      }

      // carried within this descriptor
      unsigned char cridLength = data[pointer++];
      if (cridLength == 0)
      {
        continue;
      }

      if (pointer + cridLength > dataLength)
      {
        LogDebug(L"EIT DVB: invalid content identifier descriptor, descriptor length = %hhu, pointer = %hu, CRID length = %hhu, CRID type = %hhu, CRID location = %hhu",
                  dataLength, pointer, cridLength, cridType, cridLocation);
        map<unsigned char, char*>::iterator it = crids.begin();
        for ( ; it != crids.end(); it++)
        {
          if (it->second != NULL)
          {
            delete[] it->second;
            it->second = NULL;
          }
        }
        crids.clear();
        return false;
      }

      char* crid = new char[cridLength + 1];
      if (crid == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hhu byte(s) for a type %hhu CRID",
                  cridLength, cridType);
      }
      else
      {
        memcpy(crid, &data[pointer], cridLength);
        crid[cridLength] = 0;

        char* oldCrid = crids[cridType];
        if (oldCrid != NULL)
        {
          if (strcmp(oldCrid, crid) != 0)
          {
            LogDebug(L"EIT DVB: content identifier descriptor CRID conflict, CRID type = %hhu, old CRID = %S, new CRID = %S",
                      cridType, oldCrid, crid);
          }
          delete[] oldCrid;
        }
        else
        {
          //LogDebug(L"EIT DVB: content identifier descriptor, CRID type = %hhu, CRID = %S",
          //          cridType, crid);
        }
        crids[cridType] = crid;
      }

      pointer += cridLength;
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeContentIdentifierDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeDishBevRatingDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  unsigned char& starRating,
                                                  unsigned char& mpaaClassification,
                                                  unsigned short& advisories)
{
  if (dataLength != 2)
  {
    LogDebug(L"EIT DVB: invalid Dish/BEV rating descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    // 0 = no rating
    // 1 = 1 star
    // 2 = 1.5 stars
    // 3 = 2 stars
    // ...
    // 7 = 4 stars
    starRating = data[0] >> 5;

    // 0 = N/A [not applicable]
    // 1 = G [general]
    // 2 = PG [parental guidance]
    // 3 = PG-13 [parental guidance under 13]
    // 4 = R [restricted]
    // 5 = NC-17 [nobody 17 and under]
    // 6 = NR [not rated]
    // 7 = [not used]
    // Note: there is some uncertainty about values 5, 6 and 7. Our old code differs with MythTV.
    mpaaClassification = (data[0] >> 2) & 7;

    // bit 0 (MSB) = ?
    // bit 1 = ?
    // bit 2 = ?
    // bit 3 = N [nudity]
    // bit 4 = mK [mild peril?]
    // bit 5 = V [violence]
    // bit 6 = FV [fantasy violence]
    // bit 7 = mQ [mild sensuality]
    // bit 8 = L [coarse or crude language]
    // bit 9 (LSB) = S [sexual situations]
    unsigned short dishBevAdvisories = ((data[0] & 3) << 8) | data[1];
    advisories |= dishBevAdvisories;
    //LogDebug(L"EIT DVB: Dish/BEV rating descriptor, star rating = %hhu, MPAA classification = %hhu, advisories = %hu",
    //          starRating, mpaaClassification, dishBevAdvisories);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeDishBevRatingDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeDishTextDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned char tableId,
                                              char** text)
{
  if (dataLength == 0)
  {
    LogDebug(L"EIT DVB: invalid Dish event name/description descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    // Delay decompression until retrieval to minimise memory usage.
    *text = new char[dataLength + 3];
    if (*text == NULL)
    {
      LogDebug(L"EIT DVB: failed to allocate Dish event name or description, descriptor length = %hhu",
                dataLength);
    }
    else
    {
      (*text)[0] = 0x1f;
      (*text)[1] = tableId;
      (*text)[2] = dataLength;
      memcpy(&(*text)[3], data, dataLength);
    }
    return true;

    /*char* temp;
    if (!CTextUtil::DishTextToString(data, dataLength, tableId, &temp))
    {
      LogDebug(L"EIT DVB: invalid Dish event name/description descriptor, length = %hhu, table ID = 0x%hhx, byte 1 = %hhu, byte 2 = %hhu",
                dataLength, tableId, data[0], dataLength > 1 ? data[1] : 0xff);
      return false;
    }
    if (temp == NULL)
    {
      LogDebug(L"EIT DVB: failed to allocate Dish event name or description, descriptor length = %hhu, byte 1 = %hhu, byte 2 = %hhu",
                dataLength, data[0], dataLength > 1 ? data[1] : 0xff);
    }

    LogDebug(L"EIT DVB: Dish event name/description descriptor, text = %S",
              temp == NULL ? "" : temp);

    if (temp != NULL)
    {
      delete[] temp;
    }
    return true;*/
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeDishTextDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeDishEpisodeInformationDescriptor(unsigned char* data,
                                                            unsigned char dataLength,
                                                            unsigned char tableId,
                                                            char** information)
{
  if (dataLength < 2)
  {
    LogDebug(L"EIT DVB: invalid Dish episode information descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    // The first byte always seems to be zero. Skip it. The rest is formatted
    // text.
    if (!CTextUtil::DishTextToString(&data[1], dataLength - 1, tableId, information))
    {
      LogDebug(L"EIT DVB: invalid Dish episode information descriptor, length = %hhu, table ID = 0x%hhx, byte 1 = %hhu, byte 2 = %hhu, byte 3 = %hhu",
                dataLength, tableId, data[0], data[1],
                dataLength > 2 ? data[2] : 0xff);
      return false;
    }
    if (*information == NULL)
    {
      LogDebug(L"EIT DVB: failed to allocate Dish episode information, descriptor length = %hhu, byte 1 = %hhu, byte 2 = %hhu, byte 3 = %hhu",
                dataLength, data[0], data[1], dataLength > 2 ? data[2] : 0xff);
    }

    //LogDebug(L"EIT DVB: Dish episode information descriptor, information = %S",
    //          *information == NULL ? "" : *information);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeDishEpisodeInformationDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeDishVchipDescriptor(unsigned char* data,
                                              unsigned char dataLength,
                                              unsigned char& vchipRating,
                                              unsigned short& advisories)
{
  if (dataLength != 2)
  {
    LogDebug(L"EIT DVB: invalid Dish V-Chip descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    // http://www.tvguidelines.org/ratings.htm
    // 0 = None [not rated/applicable]
    // 1 = TV-Y [all children]
    // 2 = TV-Y7 [children 7 and older]
    // 3 = TV-G [general audience]
    // 4 = TV-PG [parental guidance]
    // 5 = TV-14 [adults 14 and older]
    // 6 = TV-MA [mature audience]
    // 7 = [not used]
    vchipRating = data[0];

    // bit 0 (MSB) = [not used]
    // bit 1 = [not used]
    // bit 2 = [not used]
    // bit 3 = D [suggestive dialogue]
    // bit 4 = L [coarse or crude language]
    // bit 5 = S [sexual situations]
    // bit 6 = V [violence]
    // bit 7 (LSB) = FV [fantasy violence]
    unsigned char vchipAdvisories = data[1];

    // Translate the V-Chip advisories to be compatible with Dish/BEV.
    if (vchipAdvisories & 1)
    {
      advisories |= 0x08;   // FV
    }
    if (vchipAdvisories & 2)
    {
      advisories |= 0x10;   // V
    }
    if (vchipAdvisories & 4)
    {
      advisories |= 0x01;   // S
    }
    if (vchipAdvisories & 8)
    {
      advisories |= 0x02;   // L
    }
    if (vchipAdvisories & 0x10)
    {
      advisories |= 0x8000; // D is unique to V-Chip
    }

    //LogDebug(L"EIT DVB: Dish V-Chip descriptor, rating = %hhu, advisories = %hhu",
    //          vchipRating, vchipAdvisories);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeDishVchipDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodeDishBevSeriesDescriptor(unsigned char* data,
                                                  unsigned char dataLength,
                                                  char** seriesId,
                                                  char** episodeId,
                                                  bool& isPreviouslyShown)
{
  if (dataLength != 8)
  {
    LogDebug(L"EIT DVB: invalid Dish/BEV series descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    // Identifiers are compatible with Tribune Media Services (now owned by
    // Gracenote) data, which is used by many popular websites (eg. Zap2It) and
    // TV providers themselves.
    // http://developer.tmsapi.com/docs/read/data_v1_1/programs/Program_search
    unsigned char entityType = data[0];   // 0x7c = movie, 0x7d = sports, 0x7e = show/series
    unsigned long seriesIdNumber = (data[1] << 18) | (data[2] << 10) | (data[3] << 2) | (data[4] >> 6);
    unsigned short episodeIdNumber = ((data[4] & 0x3f) << 8) | data[5];
    unsigned short originalAirDate = (data[6] << 8) | data[7];  // days since 17 November 1858
    // int seconds = (originalAirDate - 40587) * 86400;                       
    // DateTime utcStartTime = new DateTime(1970, 1, 1).AddSeconds(seconds); 

    string prefix;
    if (entityType == 0x7c)
    {
      prefix = "MV";
    }
    else if (entityType == 0x7d)
    {
      prefix = "SP";
    }
    else if (entityType == 0x7e)
    {
      if (episodeIdNumber == 0)
      {
        prefix = "SH";
      }
      else
      {
        prefix = "EP";
      }
    }

    // Note: episode ID is expected to be 0 except for EP shows.
    stringstream ss;
    ss << prefix << setfill('0') << setw(8) << seriesIdNumber << setw(4) << episodeIdNumber;
    CopyString(ss.str().c_str(), episodeId, L"a Dish episode ID");

    if (entityType == 0x7e)
    {
      ss.str("");
      ss << "EP" << setfill('0') << setw(8) << seriesIdNumber;
      CopyString(ss.str().c_str(), seriesId, L"a Dish series ID");
    }

    isPreviouslyShown |= (originalAirDate != 0);

    //LogDebug(L"EIT DVB: Dish/BEV series descriptor, entity type = %hhu, series ID = %lu, episode ID = %hu, original air date = %hu",
    //          entityType, seriesIdNumber, episodeIdNumber, originalAirDate);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodeDishBevSeriesDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodePremiereOrderInformationDescriptor(unsigned char* data,
                                                              unsigned char dataLength,
                                                              char** orderNumber,
                                                              char** price,
                                                              char** phoneNumber,
                                                              char** smsNumber,
                                                              char** url)
{
  // Note: this text is not DVB-compliant, and is not ASCII either. For
  // example, the pound symbol is encoded as A4 20.
  // order number length - 1 byte
  // order number - [order number length] bytes
  // price length - 1 byte
  // price - [price length] bytes
  // phone number length - 1 byte
  // phone number - [phone number length] bytes
  // SMS number length - 1 byte
  // SMS number - [SMS number length] bytes
  // URL length - 1 byte
  // URL - [URL length] bytes
  if (dataLength == 0)
  {
    LogDebug(L"EIT DVB: invalid Premiere order information descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    unsigned char orderNumberLength = data[0];
    if (orderNumberLength > 0)
    {
      *orderNumber = new char[orderNumberLength + 1];
      if (*orderNumber == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hhu bytes for a Premiere order number",
                  orderNumberLength + 1);
      }
      else
      {
        memcpy(*orderNumber, &data[1], orderNumberLength);
        (*orderNumber)[orderNumberLength] = NULL;
      }
    }

    unsigned short pointer = 1 + orderNumberLength;
    unsigned char priceLength = data[pointer++];
    if (priceLength > 0)
    {
      *price = new char[priceLength + 1];
      if (*price == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hhu bytes for a Premiere price",
                  priceLength + 1);
      }
      else
      {
        memcpy(*price, &data[pointer], priceLength);
        (*price)[priceLength] = NULL;
      }
      pointer += priceLength;
    }

    unsigned char phoneNumberLength = data[pointer++];
    if (phoneNumberLength > 0)
    {
      *phoneNumber = new char[phoneNumberLength + 1];
      if (*phoneNumber == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hhu bytes for a Premiere phone number",
                  phoneNumberLength + 1);
      }
      else
      {
        memcpy(*phoneNumber, &data[pointer], phoneNumberLength);
        (*phoneNumber)[phoneNumberLength] = NULL;
      }
      pointer += phoneNumberLength;
    }

    unsigned char smsNumberLength = data[pointer++];
    if (smsNumberLength > 0)
    {
      *smsNumber = new char[smsNumberLength + 1];
      if (*smsNumber == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hhu bytes for a Premiere SMS number",
                  smsNumberLength + 1);
      }
      else
      {
        memcpy(*smsNumber, &data[pointer], smsNumberLength);
        (*smsNumber)[smsNumberLength] = NULL;
      }
      pointer += smsNumberLength;
    }

    unsigned char urlLength = data[pointer++];
    if (urlLength > 0)
    {
      *url = new char[urlLength + 1];
      if (*url == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hhu bytes for a Premiere URL",
                  urlLength + 1);
      }
      else
      {
        memcpy(*url, &data[pointer], urlLength);
        (*url)[urlLength] = NULL;
      }
      pointer += urlLength;
    }

    //LogDebug(L"EIT DVB: Premiere order information descriptor, order number = %S, price = %S, phone number = %S, SMS number = %S, URL = %S",
    //          *orderNumber == NULL ? "" : *orderNumber,
    //          *price == NULL ? "" : *price,
    //          *phoneNumber == NULL ? "" : *phoneNumber,
    //          *smsNumber == NULL ? "" : *smsNumber,
    //          *url == NULL ? "" : *url);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodePremiereOrderInformationDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodePremiereParentInformationDescriptor(unsigned char* data,
                                                              unsigned char dataLength,
                                                              unsigned char& rating,
                                                              char** text)
{
  // rating - 1 byte; DVB-compatible, age - 3 years
  // control time 1 - 3 bytes; BCD
  // control time 2 - 3 bytes; BCD
  // text length - 1 byte
  // text - [text length] bytes; not DVB-compatible, and not ASCII
  if (dataLength == 0)
  {
    LogDebug(L"EIT DVB: invalid Premiere parent information descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    rating = data[0];
    unsigned long controlTime1 = (data[1] << 24) | (data[2] << 8) | data[3];
    unsigned long controlTime2 = (data[4] << 24) | (data[5] << 8) | data[6];
    unsigned char textLength = data[7];
    if (textLength > 0)
    {
      *text = new char[textLength + 1];
      if (*text == NULL)
      {
        LogDebug(L"EIT DVB: failed to allocate %hhu bytes for Premiere parent information text",
                  textLength + 1);
      }
      else
      {
        memcpy(*text, &data[8], textLength);
        (*text)[textLength] = NULL;
      }
    }

    //LogDebug(L"EIT DVB: Premiere parent information descriptor, rating = %hhu, control time 1 = %lu, control time 2 = %lu, text = %S",
    //          rating, controlTime1, controlTime2, *text == NULL ? "" : *text);
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodePremiereParentInformationDescriptor()");
  }
  return false;
}

bool CParserEitDvb::DecodePremiereContentTransmissionDescriptor(unsigned char* data,
                                                                unsigned char dataLength,
                                                                unsigned short& originalNetworkId,
                                                                unsigned short& transportStreamId,
                                                                unsigned short& serviceId,
                                                                vector<unsigned long long>& showings)
{
  // transport stream ID - 2 bytes
  // original network ID - 2 bytes
  // service ID - 2 bytes
  // <loop>
  //   start date MJD - 2 bytes
  //   times length - 1 byte; multiple of 3 bytes
  //   <loop>
  //     start time BCD - 3 bytes
  //   </loop>
  // </loop>
  if (dataLength < 6)
  {
    LogDebug(L"EIT DVB: invalid Premiere content transmission descriptor, length = %hhu",
              dataLength);
    return false;
  }
  try
  {
    transportStreamId = (data[0] << 8) | data[1];
    originalNetworkId = (data[2] << 8) | data[3];
    serviceId = (data[4] << 8) | data[5];
    //LogDebug(L"EIT DVB: Premiere content transmission descriptor, ONID = %hu, TSID = %hu, service ID = %hu",
    //          originalNetworkId, transportStreamId, serviceId);

    unsigned short pointer = 6;
    while (pointer + 2 < dataLength)
    {
      unsigned short startDateMjd = (data[pointer] << 8) | data[pointer + 1];
      pointer += 2;
      unsigned char timesLength = data[pointer++];
      //LogDebug(L"  start date = %hu, times length = %hhu",
      //          startDateMjd, timesLength);
      if (pointer + timesLength > dataLength || timesLength % 3 != 0)
      {
        LogDebug(L"EIT DVB: invalid Premiere content transmission descriptor, descriptor length = %hhu, pointer = %hu, times length = %hhu, TSID = %hu, ONID = %hu, service ID = %hu, start date MJD = %hu",
                  dataLength, pointer, timesLength, transportStreamId,
                  originalNetworkId, serviceId, startDateMjd);
        return false;
      }

      for (unsigned char i = 0; i < timesLength; i += 3)
      {
        unsigned long startTimeBcd = (data[pointer] << 16) | (data[pointer + 1] << 8) | data[pointer + 2];
        pointer += 3;
        //LogDebug(L"  start time = %lu", startTimeBcd);
        showings.push_back(DecodeDateTime(startDateMjd, startTimeBcd));
      }
    }
    return true;
  }
  catch (...)
  {
    LogDebug(L"EIT DVB: unhandled exception in DecodePremiereContentTransmissionDescriptor()");
  }
  return false;
}

unsigned long long CParserEitDvb::DecodeDateTime(unsigned short dateMjd, unsigned long timeBcd)
{
  if (dateMjd == 0xffff && timeBcd == 0xffffff)
  {
    // NVOD reference service event.
    return 0;
  }

  // Decode the MJD-encoded date. ***The casts are important.***
  tm dateTime;
  dateTime.tm_year = (long)((dateMjd - 15078.2) / 365.25);
  dateTime.tm_mon = (long)((dateMjd - 14956.1 - (long)(dateTime.tm_year * 365.25)) / 30.6001);
  dateTime.tm_mday = (long)(dateMjd - 14956 - (long)(dateTime.tm_year * 365.25) - (long)(dateTime.tm_mon * 30.6001));
  unsigned char adjustment = (dateTime.tm_mon == 14 || dateTime.tm_mon == 15) ? 1 : 0;
  dateTime.tm_year += adjustment;
  dateTime.tm_mon = dateTime.tm_mon - 2 - adjustment * 12;

  dateTime.tm_hour = ((timeBcd >> 20) * 10) + ((timeBcd >> 16) & 0x0f);
  dateTime.tm_min = (((timeBcd >> 12) & 0x0f) * 10) + ((timeBcd >> 8) & 0x0f);
  dateTime.tm_sec = (((timeBcd >> 4) & 0x0f) * 10) + (timeBcd & 0x0f);
  dateTime.tm_isdst = -1;

  // The EIT date/time is UTC. mktime() will give us an epoch as if the
  // time were a local time. We need to convert it back to UTC.
  time_t localDateTime = mktime(&dateTime);
  tm* tempConversion = gmtime(&localDateTime);
  tempConversion->tm_isdst = -1;  // DST status unknown
  return 2 * localDateTime - mktime(tempConversion);
}