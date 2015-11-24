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
#include "GrabberSiDvb.h"
#include "EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CGrabberSiDvb::CGrabberSiDvb(ICallBackSiDvb* callBack, LPUNKNOWN unk, HRESULT* hr)
  : CUnknown(NAME("DVB SI Grabber"), unk)
{
  if (callBack == NULL)
  {
    LogDebug(L"SI DVB: call back not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_callBackGrabber = NULL;
  m_callBackSiDvb = callBack;
  m_enableCrcCheck = true;
  m_isNitExpected = true;
  m_parserBat.SetCallBack(this);
  m_parserNit.SetCallBack(this);
  m_parserSdt.SetCallBack(this);
}

CGrabberSiDvb::~CGrabberSiDvb(void)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = NULL;
  m_callBackSiDvb = NULL;
}

STDMETHODIMP CGrabberSiDvb::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  if (ppv == NULL)
  {
    return E_INVALIDARG;
  }

  if (iid == IID_IGRABBER)
  {
    return GetInterface((IGrabber*)this, ppv);
  }
  if (iid == IID_IGRABBER_SI_DVB)
  {
    return GetInterface((IGrabberSiDvb*)this, ppv);
  }
  if (iid == IID_IGRABBER_SI_FREESAT)
  {
    return GetInterface((IGrabberSiFreesat*)this, ppv);
  }
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

void CGrabberSiDvb::SetPids(unsigned short pidBat, unsigned short pidNit, unsigned short pidSdt)
{
  if (pidBat == 0)
  {
    // NIT PID notification.
    if (pidNit == 0 && !m_parserNit.IsSeenActual() && !m_parserNit.IsSeenOther())
    {
      LogDebug(L"SI DVB: NIT not expected");
      m_isNitExpected = false;
    }
    return;
  }

  LogDebug(L"SI DVB: set PIDs, BAT = %hu, NIT = %hu, SDT = %hu",
            pidBat, pidNit, pidSdt);
  if (m_parserBat.GetPid() != pidBat)
  {
    m_parserBat.SetPid(pidBat);
    m_parserBat.Reset(m_enableCrcCheck);
  }
  if (m_parserNit.GetPid() != pidNit)
  {
    m_parserNit.SetPid(pidNit);
    m_parserNit.Reset(m_enableCrcCheck);
  }
  if (m_parserSdt.GetPid() != pidSdt)
  {
    m_parserSdt.SetPid(pidSdt);
    m_parserSdt.Reset(m_enableCrcCheck);
  }
}

void CGrabberSiDvb::Reset(bool enableCrcCheck)
{
  m_enableCrcCheck = enableCrcCheck;
  m_isNitExpected = true;
  m_parserBat.Reset(enableCrcCheck);
  m_parserNit.Reset(enableCrcCheck);
  m_parserSdt.Reset(enableCrcCheck);
}

STDMETHODIMP_(void) CGrabberSiDvb::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
}

bool CGrabberSiDvb::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  bool result = false;
  if (header.Pid == m_parserBat.GetPid())
  {
    m_parserBat.OnTsPacket(header, tsPacket);
    result = true;
  }
  if (header.Pid == m_parserNit.GetPid())
  {
    m_parserNit.OnTsPacket(header, tsPacket);
    result = true;
  }
  if (header.Pid == m_parserSdt.GetPid())
  {
    m_parserSdt.OnTsPacket(header, tsPacket);
    result = true;
  }
  return result;
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsSeenBat()
{
  return m_parserBat.IsSeen();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsSeenNitActual()
{
  return m_parserNit.IsSeenActual();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsSeenNitOther()
{
  return m_parserNit.IsSeenOther();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsSeenSdtActual()
{
  return m_parserSdt.IsSeenActual();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsSeenSdtOther()
{
    return m_parserSdt.IsSeenOther();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsReadyBat()
{
  return m_parserBat.IsReady();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsReadyNitActual()
{
  return m_parserNit.IsReadyActual();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsReadyNitOther()
{
  return m_parserNit.IsReadyOther();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsReadySdtActual()
{
  return m_parserSdt.IsReadyActual();
}

STDMETHODIMP_(bool) CGrabberSiDvb::IsReadySdtOther()
{
  return m_parserSdt.IsReadyOther();
}

STDMETHODIMP_(unsigned short) CGrabberSiDvb::GetServiceCount()
{
  return m_parserSdt.GetServiceCount();
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetService(unsigned short index,
                                              unsigned char* tableId,
                                              unsigned short* originalNetworkId,
                                              unsigned short* transportStreamId,
                                              unsigned short* serviceId,
                                              unsigned short* referenceServiceId,
                                              unsigned short* freesatChannelId,
                                              unsigned short* openTvChannelId,
                                              unsigned long long* logicalChannelNumbers,
                                              unsigned short* logicalChannelNumberCount,
                                              unsigned char* dishSubChannelNumber,
                                              bool* eitScheduleFlag,
                                              bool* eitPresentFollowingFlag,
                                              unsigned char* runningStatus,
                                              bool* freeCaMode,
                                              unsigned char* serviceType,
                                              unsigned char* serviceNameCount,
                                              bool* visibleInGuide,
                                              unsigned short* streamCountVideo,
                                              unsigned short* streamCountAudio,
                                              bool* isHighDefinition,
                                              bool* isStandardDefinition,
                                              bool* isThreeDimensional,
                                              unsigned long* audioLanguages,
                                              unsigned char* audioLanguageCount,
                                              unsigned long* subtitlesLanguages,
                                              unsigned char* subtitlesLanguageCount,
                                              unsigned short* networkIds,
                                              unsigned char* networkIdCount,
                                              unsigned short* bouquetIds,
                                              unsigned char* bouquetIdCount,
                                              unsigned long* availableInCountries,
                                              unsigned char* availableInCountryCount,
                                              unsigned long* unavailableInCountries,
                                              unsigned char* unavailableInCountryCount,
                                              unsigned long* availableInCells,
                                              unsigned char* availableInCellCount,
                                              unsigned long* unavailableInCells,
                                              unsigned char* unavailableInCellCount,
                                              unsigned long long* targetRegionIds,
                                              unsigned char* targetRegionIdCount,
                                              unsigned long* freesatRegionIds,
                                              unsigned char* freesatRegionIdCount,
                                              unsigned long* openTvRegionIds,
                                              unsigned char* openTvRegionIdCount,
                                              unsigned short* freesatChannelCategoryIds,
                                              unsigned char* freesatChannelCategoryIdCount,
                                              unsigned char* openTvChannelCategoryId,
                                              unsigned char* virginMediaChannelCategoryId,
                                              unsigned short* dishMarketId,
                                              unsigned char* norDigChannelListIds,
                                              unsigned char* norDigChannelListIdCount,
                                              unsigned short* previousOriginalNetworkId,
                                              unsigned short* previousTransportStreamId,
                                              unsigned short* previousServiceId,
                                              unsigned short* epgOriginalNetworkId,
                                              unsigned short* epgTransportStreamId,
                                              unsigned short* epgServiceId)
{
  unsigned short originalLogicalChannelNumberCount = *logicalChannelNumberCount;
  unsigned char originalAvailableInCountryCount = *availableInCountryCount;
  unsigned char originalUnavailableInCountryCount = *unavailableInCountryCount;
  unsigned char originalAvailableInCellCount = *availableInCellCount;
  unsigned char originalTargetRegionIdCount = *targetRegionIdCount;
  unsigned short sdtLogicalChannelNumber;
  if (!m_parserSdt.GetService(index,
                              *tableId,
                              *originalNetworkId,
                              *transportStreamId,
                              *serviceId,
                              *eitScheduleFlag,
                              *eitPresentFollowingFlag,
                              *runningStatus,
                              *freeCaMode,
                              *serviceType,
                              *serviceNameCount,
                              sdtLogicalChannelNumber,
                              *dishSubChannelNumber,
                              *visibleInGuide,
                              *referenceServiceId,
                              *isHighDefinition,
                              *isStandardDefinition,
                              *isThreeDimensional,
                              *streamCountVideo,
                              *streamCountAudio,
                              audioLanguages,
                              *audioLanguageCount,
                              subtitlesLanguages,
                              *subtitlesLanguageCount,
                              *openTvChannelCategoryId,
                              *virginMediaChannelCategoryId,
                              *dishMarketId,
                              availableInCountries,
                              *availableInCountryCount,
                              unavailableInCountries,
                              *unavailableInCountryCount,
                              availableInCells,
                              *availableInCellCount,
                              unavailableInCells,
                              *unavailableInCellCount,
                              targetRegionIds,
                              *targetRegionIdCount,
                              *previousOriginalNetworkId,
                              *previousTransportStreamId,
                              *previousServiceId,
                              *epgOriginalNetworkId,
                              *epgTransportStreamId,
                              *epgServiceId))
  {
    return false;
  }

  // Supplement the information from the SDT with information from the BAT and
  // NIT.
  bool batVisibleInGuide = true;
  unsigned char tempCount = 0;
  unsigned char batNitTargetRegionIdCount = originalTargetRegionIdCount;
  unsigned long long* batNitTargetRegionIds = new unsigned long long[originalTargetRegionIdCount];
  if (batNitTargetRegionIds == NULL)
  {
    batNitTargetRegionIdCount = 0;
  }
  unsigned char batAvailableInCountryCount = originalAvailableInCountryCount;
  unsigned long* batAvailableInCountries = new unsigned long[originalAvailableInCountryCount];
  if (batAvailableInCountries == NULL)
  {
    batAvailableInCountryCount = 0;
  }
  unsigned char batUnavailableInCountryCount = originalUnavailableInCountryCount;
  unsigned long* batUnavailableInCountries = new unsigned long[originalUnavailableInCountryCount];
  if (batUnavailableInCountries == NULL)
  {
    batUnavailableInCountryCount = 0;
  }
  if (m_parserBat.GetService(*originalNetworkId,
                              *transportStreamId,
                              *referenceServiceId == 0 ? *serviceId : *referenceServiceId,
                              *freesatChannelId,              // BAT only
                              *openTvChannelId,               // BAT only
                              logicalChannelNumbers,          // SDT, BAT, NIT; combine
                              *logicalChannelNumberCount,
                              batVisibleInGuide,              // SDT, BAT, NIT; prefer SDT, then BAT
                              bouquetIds,                     // BAT only
                              *bouquetIdCount,
                              NULL,                           // SDT, NIT; combine [available in cells]
                              tempCount,
                              batNitTargetRegionIds,          // scoped: SDT > BAT > NIT
                              batNitTargetRegionIdCount,
                              freesatRegionIds,               // BAT only
                              *freesatRegionIdCount,
                              openTvRegionIds,                // BAT only
                              *openTvRegionIdCount,
                              freesatChannelCategoryIds,      // BAT only
                              *freesatChannelCategoryIdCount,
                              NULL,                           // NIT only [NorDig channel list IDs]
                              tempCount,
                              batAvailableInCountries,        // SDT, BAT; assume scoped
                              batAvailableInCountryCount,
                              batUnavailableInCountries,      // SDT, BAT; assume scoped
                              batUnavailableInCountryCount))
  {
    // Most of the time we expect to get the LCN and visible in guide flag
    // together from BAT or NIT. Only a few providers put these details in the
    // SDT. For those providers, the details are not available elsewhere.
    if (*logicalChannelNumberCount != 0)
    {
      *visibleInGuide = batVisibleInGuide;
    }

    // Target region descriptors are "scoped" (refer to EN 300 468 section
    // 6.5). Details in the SDT take precedence over details in the BAT, and
    // details in the NIT are the most generic.
    if (*targetRegionIdCount == 0 && batNitTargetRegionIdCount != 0)
    {
      *targetRegionIdCount = originalTargetRegionIdCount;
      CUtils::CopyArrayToArray(batNitTargetRegionIds,
                                batNitTargetRegionIdCount,
                                targetRegionIds,
                                *targetRegionIdCount);
    }

    // It is unclear how country availability should be handled. Assume it is
    // scoped.
    if (
      *availableInCountryCount == 0 &&
      *unavailableInCountryCount == 0 &&
      (batAvailableInCountryCount != 0 || batUnavailableInCountryCount != 0)
    )
    {
      *availableInCountryCount = originalAvailableInCountryCount;
      CUtils::CopyArrayToArray(batAvailableInCountries,
                                batAvailableInCountryCount,
                                availableInCountries,
                                *availableInCountryCount);

      *unavailableInCountryCount = originalUnavailableInCountryCount;
      CUtils::CopyArrayToArray(batUnavailableInCountries,
                                batUnavailableInCountryCount,
                                unavailableInCountries,
                                *unavailableInCountryCount);
    }
  }

  if (batAvailableInCountries != NULL)
  {
    delete[] batAvailableInCountries;
    batAvailableInCountries = NULL;
  }
  if (batUnavailableInCountries != NULL)
  {
    delete[] batUnavailableInCountries;
    batUnavailableInCountries = NULL;
  }

  unsigned short nitFreesatChannelId;
  unsigned short nitOpenTvChannelId;
  unsigned long long* nitLogicalChannelNumbers = logicalChannelNumbers;
  if (logicalChannelNumbers != NULL && *logicalChannelNumberCount < originalLogicalChannelNumberCount)
  {
    nitLogicalChannelNumbers = &logicalChannelNumbers[*logicalChannelNumberCount];
  }
  unsigned short nitLogicalChannelNumberCount = originalLogicalChannelNumberCount - *logicalChannelNumberCount;
  bool nitVisibleInGuide;
  unsigned char nitAvailableInCellCount = originalAvailableInCellCount;
  unsigned long* nitAvailableInCells = new unsigned long[originalAvailableInCellCount];
  if (nitAvailableInCells == NULL)
  {
    nitAvailableInCellCount = 0;
  }
  if (batNitTargetRegionIds != NULL)
  {
    batNitTargetRegionIdCount = originalTargetRegionIdCount;
  }
  if (m_parserNit.GetService(*originalNetworkId,
                              *transportStreamId,
                              *referenceServiceId == 0 ? *serviceId : *referenceServiceId,
                              nitFreesatChannelId,
                              nitOpenTvChannelId,
                              nitLogicalChannelNumbers,
                              nitLogicalChannelNumberCount,
                              nitVisibleInGuide,
                              networkIds,
                              *networkIdCount,
                              nitAvailableInCells,
                              nitAvailableInCellCount,
                              batNitTargetRegionIds,
                              batNitTargetRegionIdCount,
                              NULL,
                              tempCount,
                              NULL,
                              tempCount,
                              NULL,
                              tempCount,
                              norDigChannelListIds,
                              *norDigChannelListIdCount,
                              NULL,
                              tempCount,
                              NULL,
                              tempCount))
  {
    // Use values from the NIT if we didn't get values from the BAT. In most
    // cases these fields are expected to come from the BAT and not be present
    // in the NIT.
    if (*freesatChannelId == 0)
    {
      *freesatChannelId = nitFreesatChannelId;
    }
    else if (nitFreesatChannelId != 0 && *freesatChannelId != nitFreesatChannelId)
    {
      LogDebug(L"SI DVB %d: unexpected Freesat channel ID conflict, ONID = %hu, TSID = %hu, service ID = %hu, BAT Freesat CID = %hu, NIT Freesat CID = %hu",
                m_parserNit.GetPid(), *originalNetworkId, *transportStreamId,
                *serviceId, *freesatChannelId, nitFreesatChannelId);
    }

    if (*openTvChannelId == 0)
    {
      *openTvChannelId = nitOpenTvChannelId;
    }
    else if (nitOpenTvChannelId != 0 && *openTvChannelId != nitOpenTvChannelId)
    {
      LogDebug(L"SI DVB %d: unexpected OpenTV channel ID conflict, ONID = %hu, TSID = %hu, service ID = %hu, BAT OpenTV CID = %hu, NIT OpenTV CID = %hu",
                m_parserNit.GetPid(), *originalNetworkId, *transportStreamId,
                *serviceId, *openTvChannelId, nitOpenTvChannelId);
    }

    if (nitLogicalChannelNumberCount > 0)
    {
      *visibleInGuide = nitVisibleInGuide;
    }

    // Add cell availability. Only add cells that aren't in either of the lists
    // from the SDT.
    if (availableInCells == NULL && nitAvailableInCellCount > 0)
    {
      LogDebug(L"SI DVB %d: insufficient available in cell array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = 0",
                m_parserNit.GetPid(), *originalNetworkId, *transportStreamId,
                *serviceId, nitAvailableInCellCount);
    }
    else
    {
      bool insufficientSize = false;
      for (unsigned char i = 0; i < nitAvailableInCellCount; i++)
      {
        unsigned long cellId = nitAvailableInCells[i];
        if (
          (
            *availableInCellCount == 0 ||
            find(availableInCells, availableInCells + *availableInCellCount, cellId) == availableInCells + *availableInCellCount
          ) &&
          (
            *unavailableInCellCount == 0 ||
            unavailableInCells == NULL ||
            find(unavailableInCells, unavailableInCells + *unavailableInCellCount, cellId) == unavailableInCells + *unavailableInCellCount
          )
        )
        {
          if (*availableInCellCount >= originalAvailableInCellCount)
          {
            (*availableInCellCount)++;
            insufficientSize = true;
          }
          else
          {
            availableInCells[(*availableInCellCount)++] = cellId;
          }
        }
      }
      if (insufficientSize)
      {
        LogDebug(L"SI DVB %d: insufficient available in cell array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hhu, actual size = %hhu",
                  m_parserNit.GetPid(), *originalNetworkId, *transportStreamId,
                  *serviceId, *availableInCellCount,
                  originalAvailableInCellCount);
        *availableInCellCount = originalAvailableInCellCount;
      }
    }

    // As per scoping rules.
    if (*targetRegionIdCount == 0 && batNitTargetRegionIdCount != 0)
    {
      *targetRegionIdCount = originalTargetRegionIdCount;
      CUtils::CopyArrayToArray(batNitTargetRegionIds,
                                batNitTargetRegionIdCount,
                                targetRegionIds,
                                *targetRegionIdCount);
    }
  }

  if (batNitTargetRegionIds != NULL)
  {
    delete[] batNitTargetRegionIds;
  }
  if (nitAvailableInCells != NULL)
  {
    delete[] nitAvailableInCells;
  }

  // Add any LCN from the SDT if the array has room for it.
  if (sdtLogicalChannelNumber != 0)
  {
    if (logicalChannelNumbers == NULL || *logicalChannelNumberCount == originalLogicalChannelNumberCount)
    {
      LogDebug(L"SI DVB %d: insufficient logical channel number array size, ONID = %hu, TSID = %hu, service ID = %hu, required size = %hu, actual size = %hu",
                m_parserSdt.GetPid(), *originalNetworkId, *transportStreamId,
                *serviceId, originalLogicalChannelNumberCount + 1,
                originalLogicalChannelNumberCount);
    }
    else
    {
      logicalChannelNumbers[*logicalChannelNumberCount] = ((unsigned long long)*tableId << 56) | sdtLogicalChannelNumber;
      (*logicalChannelNumberCount)++;
    }
  }

  return true;
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetServiceNameByIndex(unsigned short serviceIndex,
                                                          unsigned char nameIndex,
                                                          unsigned long* language,
                                                          char* providerName,
                                                          unsigned short* providerNameBufferSize,
                                                          char* serviceName,
                                                          unsigned short* serviceNameBufferSize)
{
  return m_parserSdt.GetServiceNameByIndex(serviceIndex,
                                            nameIndex,
                                            *language,
                                            providerName,
                                            *providerNameBufferSize,
                                            serviceName,
                                            *serviceNameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetServiceNameByLanguage(unsigned short serviceIndex,
                                                            unsigned long language,
                                                            char* providerName,
                                                            unsigned short* providerNameBufferSize,
                                                            char* serviceName,
                                                            unsigned short* serviceNameBufferSize)
{
  return m_parserSdt.GetServiceNameByLanguage(serviceIndex,
                                              language,
                                              providerName,
                                              *providerNameBufferSize,
                                              serviceName,
                                              *serviceNameBufferSize);
}

bool CGrabberSiDvb::GetDefaultAuthority(unsigned short originalNetworkId,
                                        unsigned short transportStreamId,
                                        unsigned short serviceId,
                                        char* defaultAuthority,
                                        unsigned short& defaultAuthorityBufferSize) const
{
  // According to TS 102 323, scope precedence should be:
  // SDT > BAT TS = NIT TS > BAT = NIT
  // Also: "The effect of defining a default authority in a BAT that conflicts
  // with a definition of equivalent scope in a NIT is not defined by the
  // present document."

  // In practise we've implemented:
  // SDT > BAT TS > BAT > NIT TS > NIT
  // We're not able to easily prioritise *both* BAT TS and NIT TS over BAT
  // *and* NIT.
  // We know that Freesat UK sometimes puts descriptors in the Freesat BAT TS
  // loops, so we prioritise BAT over NIT.
  if (
    m_parserSdt.GetDefaultAuthority(originalNetworkId, transportStreamId, serviceId, defaultAuthority, defaultAuthorityBufferSize) ||
    m_parserBat.GetDefaultAuthority(originalNetworkId, transportStreamId, serviceId, defaultAuthority, defaultAuthorityBufferSize) ||
    m_parserNit.GetDefaultAuthority(originalNetworkId, transportStreamId, serviceId, defaultAuthority, defaultAuthorityBufferSize)
  )
  {
    return true;
  }
  // Not an error - just not found.
  return false;
}

STDMETHODIMP_(unsigned char) CGrabberSiDvb::GetNetworkNameCount(unsigned short networkId)
{
  return m_parserNit.GetNetworkNameCount(networkId);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetNetworkNameByIndex(unsigned short networkId,
                                                          unsigned char index,
                                                          unsigned long* language,
                                                          char* name,
                                                          unsigned short* nameBufferSize)
{
  return m_parserNit.GetNetworkNameByIndex(networkId, index, *language, name, *nameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetNetworkNameByLanguage(unsigned short networkId,
                                                            unsigned long language,
                                                            char* name,
                                                            unsigned short* nameBufferSize)
{
  return m_parserNit.GetNetworkNameByLanguage(networkId, language, name, *nameBufferSize);
}

STDMETHODIMP_(unsigned char) CGrabberSiDvb::GetBouquetNameCount(unsigned short bouquetId)
{
  return m_parserBat.GetBouquetNameCount(bouquetId);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetBouquetNameByIndex(unsigned short bouquetId,
                                                          unsigned char index,
                                                          unsigned long* language,
                                                          char* name,
                                                          unsigned short* nameBufferSize)
{
  return m_parserBat.GetBouquetNameByIndex(bouquetId, index, *language, name, *nameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetBouquetNameByLanguage(unsigned short bouquetId,
                                                            unsigned long language,
                                                            char* name,
                                                            unsigned short* nameBufferSize)
{
  return m_parserBat.GetBouquetNameByLanguage(bouquetId, language, name, *nameBufferSize);
}

STDMETHODIMP_(unsigned char) CGrabberSiDvb::GetTargetRegionNameCount(unsigned long long regionId)
{
  return m_parserBat.GetTargetRegionNameCount(regionId) + m_parserNit.GetTargetRegionNameCount(regionId);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetTargetRegionNameByIndex(unsigned long long regionId,
                                                              unsigned char index,
                                                              unsigned long* language,
                                                              char* name,
                                                              unsigned short* nameBufferSize)
{
  unsigned char batNameCount = m_parserBat.GetTargetRegionNameCount(regionId);
  if (index < batNameCount)
  {
    return m_parserBat.GetTargetRegionNameByIndex(regionId,
                                                  index,
                                                  *language,
                                                  name,
                                                  *nameBufferSize);
  }
  index -= batNameCount;
  return m_parserNit.GetTargetRegionNameByIndex(regionId, index, *language, name, *nameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetTargetRegionNameByLanguage(unsigned long long regionId,
                                                                  unsigned long language,
                                                                  char* name,
                                                                  unsigned short* nameBufferSize)
{
  if (m_parserBat.GetTargetRegionNameByLanguage(regionId, language, name, *nameBufferSize))
  {
    return true;
  }
  return m_parserNit.GetTargetRegionNameByLanguage(regionId, language, name, *nameBufferSize);
}

STDMETHODIMP_(unsigned char) CGrabberSiDvb::GetFreesatRegionNameCount(unsigned short regionId)
{
  return m_parserBat.GetFreesatRegionNameCount(regionId) + m_parserNit.GetFreesatRegionNameCount(regionId);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetFreesatRegionNameByIndex(unsigned short regionId,
                                                                unsigned char index,
                                                                unsigned long* language,
                                                                char* name,
                                                                unsigned short* nameBufferSize)
{
  unsigned char batNameCount = m_parserBat.GetFreesatRegionNameCount(regionId);
  if (index < batNameCount)
  {
    return m_parserBat.GetFreesatRegionNameByIndex(regionId,
                                                    index,
                                                    *language,
                                                    name,
                                                    *nameBufferSize);
  }
  index -= batNameCount;
  return m_parserNit.GetFreesatRegionNameByIndex(regionId,
                                                  index,
                                                  *language,
                                                  name,
                                                  *nameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetFreesatRegionNameByLanguage(unsigned short regionId,
                                                                  unsigned long language,
                                                                  char* name,
                                                                  unsigned short* nameBufferSize)
{
  if (m_parserBat.GetFreesatRegionNameByLanguage(regionId, language, name, *nameBufferSize))
  {
    return true;
  }
  return m_parserNit.GetFreesatRegionNameByLanguage(regionId, language, name, *nameBufferSize);
}

STDMETHODIMP_(unsigned char) CGrabberSiDvb::GetFreesatChannelCategoryNameCount(unsigned short categoryId)
{
  return m_parserBat.GetFreesatChannelCategoryNameCount(categoryId) + m_parserNit.GetFreesatChannelCategoryNameCount(categoryId);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetFreesatChannelCategoryNameByIndex(unsigned short categoryId,
                                                                        unsigned char index,
                                                                        unsigned long* language,
                                                                        char* name,
                                                                        unsigned short* nameBufferSize)
{
  unsigned char batNameCount = m_parserBat.GetFreesatChannelCategoryNameCount(categoryId);
  if (index < batNameCount)
  {
    return m_parserBat.GetFreesatChannelCategoryNameByIndex(categoryId,
                                                            index,
                                                            *language,
                                                            name,
                                                            *nameBufferSize);
  }
  index -= batNameCount;
  return m_parserNit.GetFreesatChannelCategoryNameByIndex(categoryId,
                                                          index,
                                                          *language,
                                                          name,
                                                          *nameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetFreesatChannelCategoryNameByLanguage(unsigned short categoryId,
                                                                            unsigned long language,
                                                                            char* name,
                                                                            unsigned short* nameBufferSize)
{
  if (m_parserBat.GetFreesatChannelCategoryNameByLanguage(categoryId,
                                                          language,
                                                          name,
                                                          *nameBufferSize))
  {
    return true;
  }
  return m_parserNit.GetFreesatChannelCategoryNameByLanguage(categoryId,
                                                              language,
                                                              name,
                                                              *nameBufferSize);
}

STDMETHODIMP_(unsigned char) CGrabberSiDvb::GetNorDigChannelListNameCount(unsigned char channelListId)
{
  return m_parserBat.GetNorDigChannelListNameCount(channelListId) + m_parserNit.GetNorDigChannelListNameCount(channelListId);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetNorDigChannelListNameByIndex(unsigned char channelListId,
                                                                    unsigned char index,
                                                                    unsigned long* language,
                                                                    char* name,
                                                                    unsigned short* nameBufferSize)
{
  // Prefer NIT in this case because the NorDig specification says logical
  // channel descriptors are carried in the NIT.
  unsigned char nitNameCount = m_parserBat.GetNorDigChannelListNameCount(channelListId);
  if (index < nitNameCount)
  {
    return m_parserNit.GetNorDigChannelListNameByIndex(channelListId,
                                                        index,
                                                        *language,
                                                        name,
                                                        *nameBufferSize);
  }
  index -= nitNameCount;
  return m_parserBat.GetNorDigChannelListNameByIndex(channelListId,
                                                      index,
                                                      *language,
                                                      name,
                                                      *nameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetNorDigChannelListNameByLanguage(unsigned char channelListId,
                                                                      unsigned long language,
                                                                      char* name,
                                                                      unsigned short* nameBufferSize)
{
  if (m_parserBat.GetNorDigChannelListNameByLanguage(channelListId,
                                                      language,
                                                      name,
                                                      *nameBufferSize))
  {
    return true;
  }
  return m_parserNit.GetNorDigChannelListNameByLanguage(channelListId,
                                                        language,
                                                        name,
                                                        *nameBufferSize);
}

STDMETHODIMP_(unsigned short) CGrabberSiDvb::GetTransmitterCount()
{
  return m_parserNit.GetTransmitterCount();
}

STDMETHODIMP_(bool) CGrabberSiDvb::GetTransmitter(unsigned short index,
                                                  unsigned char* tableId,
                                                  unsigned short* networkId,
                                                  unsigned short* originalNetworkId,
                                                  unsigned short* transportStreamId,
                                                  bool* isHomeTransmitter,
                                                  unsigned long* broadcastStandard,
                                                  unsigned long* frequencies,
                                                  unsigned char* frequencyCount,
                                                  unsigned char* polarisation,
                                                  unsigned char* modulation,
                                                  unsigned long* symbolRate,
                                                  unsigned short* bandwidth,
                                                  unsigned char* innerFecRate,
                                                  unsigned char* rollOffFactor,
                                                  short* longitude,
                                                  unsigned short* cellId,
                                                  unsigned char* cellIdExtension,
                                                  unsigned char* plpId)
{
  return m_parserNit.GetTransmitter(index,
                                    *tableId,
                                    *networkId,
                                    *originalNetworkId,
                                    *transportStreamId,
                                    *isHomeTransmitter,
                                    *broadcastStandard,
                                    frequencies,
                                    *frequencyCount,
                                    *polarisation,
                                    *modulation,
                                    *symbolRate,
                                    *bandwidth,
                                    *innerFecRate,
                                    *rollOffFactor,
                                    *longitude,
                                    *cellId,
                                    *cellIdExtension,
                                    *plpId);
}

void CGrabberSiDvb::OnTableSeen(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableSeen(m_parserSdt.GetPid(), tableId);
  }
  if (
    !m_isNitExpected &&
    (tableId == TABLE_ID_NIT_DVB_ACTUAL || tableId == TABLE_ID_NIT_DVB_OTHER)
  )
  {
    LogDebug(L"SI DVB: NIT not expected but still received");
    m_isNitExpected = true;
  }
}

void CGrabberSiDvb::OnTableComplete(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableComplete(m_parserSdt.GetPid(), tableId);
    if (tableId == TABLE_ID_SDT_ACTUAL && !m_isNitExpected)
    {
      m_callBackGrabber->OnTableComplete(m_parserSdt.GetPid(), TABLE_ID_NIT_DVB_ACTUAL);
      m_callBackGrabber->OnTableComplete(m_parserSdt.GetPid(), TABLE_ID_NIT_DVB_OTHER);
    }
  }
}

void CGrabberSiDvb::OnTableChange(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableChange(m_parserSdt.GetPid(), tableId);
  }
}

void CGrabberSiDvb::OnSdtReceived(unsigned char tableId,
                                  unsigned short originalNetworkId,
                                  unsigned short transportStreamId,
                                  unsigned short serviceId,
                                  bool eitScheduleFlag,
                                  bool eitPresentFollowingFlag,
                                  unsigned char runningStatus,
                                  bool freeCaMode,
                                  unsigned char serviceType,
                                  const map<unsigned long, char*>& providerNames,
                                  const map<unsigned long, char*>& serviceNames,
                                  unsigned short logicalChannelNumber,
                                  unsigned char dishSubChannelNumber,
                                  bool visibleInGuide,
                                  unsigned short referenceServiceId,
                                  bool isHighDefinition,
                                  bool isStandardDefinition,
                                  bool isThreeDimensional,
                                  unsigned short streamCountVideo,
                                  unsigned short streamCountAudio,
                                  const vector<unsigned long>& audioLanguages,
                                  const vector<unsigned long>& subtitlesLanguages,
                                  unsigned char openTvCategoryId,
                                  unsigned char virginMediaCategoryId,
                                  unsigned short dishMarketId,
                                  const vector<unsigned long>& availableInCountries,
                                  const vector<unsigned long>& unavailableInCountries,
                                  const vector<unsigned long>& availableInCells,
                                  const vector<unsigned long>& unavailableInCells,
                                  const vector<unsigned long long>& targetRegionIds,
                                  unsigned short previousOriginalNetworkId,
                                  unsigned short previousTransportStreamId,
                                  unsigned short previousServiceId,
                                  unsigned short epgOriginalNetworkId,
                                  unsigned short epgTransportStreamId,
                                  unsigned short epgServiceId,
                                  const char* defaultAuthority)
{
  if (tableId == TABLE_ID_SDT_ACTUAL)
  {
    CEnterCriticalSection lock(m_section);
    if (m_callBackSiDvb != NULL)
    {
      if (serviceType == 0x83)
      {
        m_callBackSiDvb->OnOpenTvEpgService(serviceId, originalNetworkId);
      }
      m_callBackSiDvb->OnSdtRunningStatus(serviceId, runningStatus);
    }
  }
}

void CGrabberSiDvb::OnSdtChanged(unsigned char tableId,
                                  unsigned short originalNetworkId,
                                  unsigned short transportStreamId,
                                  unsigned short serviceId,
                                  bool eitScheduleFlag,
                                  bool eitPresentFollowingFlag,
                                  unsigned char runningStatus,
                                  bool freeCaMode,
                                  unsigned char serviceType,
                                  const map<unsigned long, char*>& providerNames,
                                  const map<unsigned long, char*>& serviceNames,
                                  unsigned short logicalChannelNumber,
                                  unsigned char dishSubChannelNumber,
                                  bool visibleInGuide,
                                  unsigned short referenceServiceId,
                                  bool isHighDefinition,
                                  bool isStandardDefinition,
                                  bool isThreeDimensional,
                                  unsigned short streamCountVideo,
                                  unsigned short streamCountAudio,
                                  const vector<unsigned long>& audioLanguages,
                                  const vector<unsigned long>& subtitlesLanguages,
                                  unsigned char openTvCategoryId,
                                  unsigned char virginMediaCategoryId,
                                  unsigned short dishMarketId,
                                  const vector<unsigned long>& availableInCountries,
                                  const vector<unsigned long>& unavailableInCountries,
                                  const vector<unsigned long>& availableInCells,
                                  const vector<unsigned long>& unavailableInCells,
                                  const vector<unsigned long long>& targetRegionIds,
                                  unsigned short previousOriginalNetworkId,
                                  unsigned short previousTransportStreamId,
                                  unsigned short previousServiceId,
                                  unsigned short epgOriginalNetworkId,
                                  unsigned short epgTransportStreamId,
                                  unsigned short epgServiceId,
                                  const char* defaultAuthority)
{
  if (tableId == TABLE_ID_SDT_ACTUAL)
  {
    CEnterCriticalSection lock(m_section);
    if (m_callBackSiDvb != NULL)
    {
      m_callBackSiDvb->OnSdtRunningStatus(serviceId, runningStatus);
    }
  }
}

void CGrabberSiDvb::OnSdtRemoved(unsigned char tableId,
                                  unsigned short originalNetworkId,
                                  unsigned short transportStreamId,
                                  unsigned short serviceId,
                                  bool eitScheduleFlag,
                                  bool eitPresentFollowingFlag,
                                  unsigned char runningStatus,
                                  bool freeCaMode,
                                  unsigned char serviceType,
                                  const map<unsigned long, char*>& providerNames,
                                  const map<unsigned long, char*>& serviceNames,
                                  unsigned short logicalChannelNumber,
                                  unsigned char dishSubChannelNumber,
                                  bool visibleInGuide,
                                  unsigned short referenceServiceId,
                                  bool isHighDefinition,
                                  bool isStandardDefinition,
                                  bool isThreeDimensional,
                                  unsigned short streamCountVideo,
                                  unsigned short streamCountAudio,
                                  const vector<unsigned long>& audioLanguages,
                                  const vector<unsigned long>& subtitlesLanguages,
                                  unsigned char openTvCategoryId,
                                  unsigned char virginMediaCategoryId,
                                  unsigned short dishMarketId,
                                  const vector<unsigned long>& availableInCountries,
                                  const vector<unsigned long>& unavailableInCountries,
                                  const vector<unsigned long>& availableInCells,
                                  const vector<unsigned long>& unavailableInCells,
                                  const vector<unsigned long long>& targetRegionIds,
                                  unsigned short previousOriginalNetworkId,
                                  unsigned short previousTransportStreamId,
                                  unsigned short previousServiceId,
                                  unsigned short epgOriginalNetworkId,
                                  unsigned short epgTransportStreamId,
                                  unsigned short epgServiceId,
                                  const char* defaultAuthority)
{
  if (tableId == TABLE_ID_SDT_ACTUAL)
  {
    CEnterCriticalSection lock(m_section);
    if (m_callBackSiDvb != NULL)
    {
      m_callBackSiDvb->OnSdtRunningStatus(serviceId, 1);   // not running
    }
  }
}