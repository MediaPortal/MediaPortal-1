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
#include "GrabberSiAtscScte.h"
#include "EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CGrabberSiAtscScte::CGrabberSiAtscScte(unsigned short pid,
                                        ICallBackSiAtscScte* callBack,
                                        LPUNKNOWN unk,
                                        HRESULT* hr)
  : CUnknown(NAME("ATSC/SCTE SI Grabber"), unk), m_parserEam(pid),
    m_parserLvct(pid), m_parserMgt(pid)
{
  if (callBack == NULL)
  {
    LogDebug(L"SI ATSC/SCTE %d: call back not supplied", pid);
    *hr = E_INVALIDARG;
    return;
  }

  m_callBackGrabber = NULL;
  m_callBackSiAtscScte = callBack;
  m_enableCrcCheck = true;

  m_sectionDecoder.SetPid(pid);
  m_sectionDecoder.SetCallBack(this);
  m_parserEam.SetCallBack(this);
  m_parserLvct.SetCallBack(this);
  m_parserMgt.SetCallBack(this);
  m_parserNit.SetCallBack(this);
  m_parserNtt.SetCallBack(this);
  m_parserSvct.SetCallBack(this);
}

CGrabberSiAtscScte::~CGrabberSiAtscScte(void)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = NULL;
  m_callBackSiAtscScte = NULL;
}

STDMETHODIMP CGrabberSiAtscScte::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  if (ppv == NULL)
  {
    return E_INVALIDARG;
  }

  if (iid == IID_IGRABBER)
  {
    return GetInterface((IGrabber*)this, ppv);
  }
  if (iid == IID_IGRABBER_SI_ATSC)
  {
    return GetInterface((IGrabberSiAtsc*)this, ppv);
  }
  if (iid == IID_IGRABBER_SI_SCTE)
  {
    return GetInterface((IGrabberSiScte*)this, ppv);
  }
  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

void CGrabberSiAtscScte::Reset(bool enableCrcCheck)
{
  CEnterCriticalSection lock(m_section);
  m_enableCrcCheck = enableCrcCheck;
  m_sectionDecoder.EnableCrcCheck(m_enableCrcCheck);
  m_sectionDecoder.Reset();
  m_parserEam.Reset();
  m_parserLvct.Reset();
  m_parserMgt.Reset();
  m_parserNit.Reset();
  m_parserNtt.Reset();
  m_parserSvct.Reset();
}

STDMETHODIMP_(void) CGrabberSiAtscScte::SetCallBack(ICallBackGrabber* callBack)
{
  CEnterCriticalSection lock(m_section);
  m_callBackGrabber = callBack;
}

bool CGrabberSiAtscScte::OnTsPacket(CTsHeader& header, unsigned char* tsPacket)
{
  if (header.Pid == m_sectionDecoder.GetPid())
  {
    m_sectionDecoder.OnTsPacket(header, tsPacket);
    return true;
  }
  return false;
}

void CGrabberSiAtscScte::OnNewSection(int pid, int tableId, CSection& section)
{
  OnNewSection(pid, tableId, section, false);
}

void CGrabberSiAtscScte::OnNewSection(int pid, int tableId, CSection& section, bool isOutOfBandSection)
{
  CEnterCriticalSection lock(m_section);
  switch (tableId)
  {
    case TABLE_ID_EAM:
      m_parserEam.OnNewSection(section);
      break;
    case TABLE_ID_LVCT_CABLE:
    case TABLE_ID_LVCT_TERRESTRIAL:
      m_parserLvct.OnNewSection(section, isOutOfBandSection);
      break;
    case TABLE_ID_MGT:
      m_parserMgt.OnNewSection(section);
      break;
    case TABLE_ID_NIT_ATSC:
      m_parserNit.OnNewSection(section);
      break;
    case TABLE_ID_NTT:
      m_parserNtt.OnNewSection(section);
      break;
    case TABLE_ID_SVCT:
      m_parserSvct.OnNewSection(section);
      break;
  }
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::IsSeenLvct()
{
  return m_parserLvct.IsSeen();
}

bool CGrabberSiAtscScte::IsSeenMgt() const
{
  return m_parserMgt.IsSeen();
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::IsSeenSvct()
{
  return m_parserNit.IsSeen() || m_parserNtt.IsSeen() || m_parserSvct.IsSeen();
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::IsReadyLvct()
{
  return m_parserLvct.IsReady();
}

bool CGrabberSiAtscScte::IsReadyMgt() const
{
  return m_parserMgt.IsReady();
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::IsReadySvct()
{
  return m_parserNit.IsReady() && m_parserNtt.IsReady() && m_parserSvct.IsReady();
}

bool CGrabberSiAtscScte::GetLatestEmergencyAlertMessage(unsigned short& id,
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
  return m_parserEam.GetLatestMessage(id,
                                      originatorCode,
                                      eventCode,
                                      eventCodeBufferSize,
                                      alertMessageTimeRemaining,
                                      eventStartTime,
                                      eventDuration,
                                      alertPriority,
                                      detailsOobSourceId,
                                      detailsMajorChannelNumber,
                                      detailsMinorChannelNumber,
                                      detailsRfChannel,
                                      detailsProgramNumber,
                                      audioOobSourceId,
                                      textCount,
                                      locationCodes,
                                      locationCodeCount,
                                      exceptions,
                                      exceptionCount,
                                      alternativeExceptions,
                                      alternativeExceptionCount);
}

bool CGrabberSiAtscScte::GetLatestEmergencyAlertMessageTextByIndex(unsigned char index,
                                                                    unsigned long& language,
                                                                    char* alertText,
                                                                    unsigned short& alertTextBufferSize,
                                                                    char* natureOfActivationText,
                                                                    unsigned short& natureOfActivationTextBufferSize) const
{
  return m_parserEam.GetLatestMessageTextByIndex(index,
                                                  language,
                                                  alertText,
                                                  alertTextBufferSize,
                                                  natureOfActivationText,
                                                  natureOfActivationTextBufferSize);
}

bool CGrabberSiAtscScte::GetLatestEmergencyAlertMessageTextByLanguage(unsigned long language,
                                                                      char* alertText,
                                                                      unsigned short& alertTextBufferSize,
                                                                      char* natureOfActivationText,
                                                                      unsigned short& natureOfActivationTextBufferSize) const
{
  return m_parserEam.GetLatestMessageTextByLanguage(language,
                                                    alertText,
                                                    alertTextBufferSize,
                                                    natureOfActivationText,
                                                    natureOfActivationTextBufferSize);
}

STDMETHODIMP_(unsigned short) CGrabberSiAtscScte::GetLvctChannelCount()
{
  return m_parserLvct.GetChannelCount();
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::GetLvctChannel(unsigned short index,
                                                        unsigned char* tableId,
                                                        unsigned short* sectionTransportStreamId,
                                                        unsigned short* mapId,
                                                        char* shortName,
                                                        unsigned short* shortNameBufferSize,
                                                        unsigned char* longNameCount,
                                                        unsigned short* majorChannelNumber,
                                                        unsigned short* minorChannelNumber,
                                                        unsigned char* modulationMode,
                                                        unsigned long* carrierFrequency,
                                                        unsigned short* transportStreamId,
                                                        unsigned short* programNumber,
                                                        unsigned char* etmLocation,
                                                        bool* accessControlled,
                                                        bool* hidden,
                                                        unsigned char* pathSelect,
                                                        bool* outOfBand,
                                                        bool* hideGuide,
                                                        unsigned char* serviceType,
                                                        unsigned short* sourceId,
                                                        unsigned char* streamCountVideo,
                                                        unsigned char* streamCountAudio,
                                                        bool* isThreeDimensional,
                                                        unsigned long* audioLanguages,
                                                        unsigned char* audioLanguageCount,
                                                        unsigned long* captionsLanguages,
                                                        unsigned char* captionsLanguageCount)
{
  return m_parserLvct.GetChannel(index,
                                  *tableId,
                                  *sectionTransportStreamId,
                                  *mapId,
                                  shortName,
                                  *shortNameBufferSize,
                                  *longNameCount,
                                  *majorChannelNumber,
                                  *minorChannelNumber,
                                  *modulationMode,
                                  *carrierFrequency,
                                  *transportStreamId,
                                  *programNumber,
                                  *etmLocation,
                                  *accessControlled,
                                  *hidden,
                                  *pathSelect,
                                  *outOfBand,
                                  *hideGuide,
                                  *serviceType,
                                  *sourceId,
                                  *streamCountVideo,
                                  *streamCountAudio,
                                  *isThreeDimensional,
                                  audioLanguages,
                                  *audioLanguageCount,
                                  captionsLanguages,
                                  *captionsLanguageCount);
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::GetLvctChannelLongNameByIndex(unsigned short channelIndex,
                                                                      unsigned char nameIndex,
                                                                      unsigned long* language,
                                                                      char* name,
                                                                      unsigned short* nameBufferSize)
{
  return m_parserLvct.GetChannelLongNameByIndex(channelIndex,
                                                nameIndex,
                                                *language,
                                                name,
                                                *nameBufferSize);
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::GetLvctChannelLongNameByLanguage(unsigned short channelIndex,
                                                                          unsigned long language,
                                                                          char* name,
                                                                          unsigned short* nameBufferSize)
{
  return m_parserLvct.GetChannelLongNameByLanguage(channelIndex, language, name, *nameBufferSize);
}

unsigned short CGrabberSiAtscScte::GetMasterGuideTableCount() const
{
  return m_parserMgt.GetTableCount();
}

bool CGrabberSiAtscScte::GetMasterGuideTable(unsigned short index,
                                              unsigned short& tableType,
                                              unsigned short& pid,
                                              unsigned char& versionNumber,
                                              unsigned long& numberBytes) const
{
  return m_parserMgt.GetTable(index, tableType, pid, versionNumber, numberBytes);
}

STDMETHODIMP_(unsigned short) CGrabberSiAtscScte::GetSvctVirtualChannelCount()
{
  return m_parserSvct.GetVirtualChannelCount();
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::GetSvctVirtualChannel(unsigned short index,
                                                              unsigned char* transmissionMedium,
                                                              unsigned short* vctId,
                                                              unsigned long* mapNameLanguage,
                                                              char* mapName,
                                                              unsigned short* mapNameBufferSize,
                                                              bool* splice,
                                                              unsigned long* activationTime,
                                                              bool* hdtvChannel,
                                                              bool* preferredSource,
                                                              bool* applicationVirtualChannel,
                                                              unsigned short* majorChannelNumber,
                                                              unsigned short* minorChannelNumber,
                                                              unsigned short* sourceId,
                                                              unsigned long* sourceNameLanguage,
                                                              char* sourceName,
                                                              unsigned short* sourceNameBufferSize,
                                                              bool* accessControlled,
                                                              bool* hideGuide,
                                                              unsigned char* serviceType,
                                                              bool* outOfBand,
                                                              unsigned char* bitstreamSelect,
                                                              unsigned char* pathSelect,
                                                              unsigned char* channelType,
                                                              unsigned short* nvodChannelBase,
                                                              unsigned char* transportType, 
                                                              bool* wideBandwidthVideo,
                                                              unsigned char* waveformStandard,
                                                              unsigned char* videoStandard,
                                                              bool* wideBandwidthAudio,
                                                              bool* compandedAudio,
                                                              unsigned char* matrixMode,
                                                              unsigned short* subcarrier2Offset,
                                                              unsigned short* subcarrier1Offset,
                                                              bool* suppressVideo,
                                                              unsigned char* audioSelection,
                                                              unsigned short* programNumber,
                                                              unsigned short* transportStreamId, 
                                                              unsigned char* satelliteId,
                                                              unsigned long* satelliteNameLanguage,
                                                              char* satelliteReferenceName,
                                                              unsigned short* satelliteReferenceNameBufferSize,
                                                              char* satelliteFullName,
                                                              unsigned short* satelliteFullNameBufferSize,
                                                              unsigned char* hemisphere,
                                                              unsigned short* orbitalPosition,
                                                              bool* youAreHere,
                                                              unsigned char* frequencyBand,
                                                              bool* outOfService,
                                                              unsigned char* polarisationType,
                                                              unsigned char* transponderNumber,
                                                              unsigned long* transponderNameLanguage,
                                                              char* transponderName,
                                                              unsigned short* transponderNameBufferSize,
                                                              bool* rootTransponder,
                                                              unsigned char* toneSelect,
                                                              unsigned char* polarisation,
                                                              unsigned long* frequency,
                                                              unsigned long* symbolRate,
                                                              unsigned char* transmissionSystem,
                                                              unsigned char* innerCodingMode,
                                                              bool* splitBitstreamMode,
                                                              unsigned char* modulationFormat)
{
  unsigned short virtualChannelNumber;
  unsigned char cdtReference;
  unsigned char mmtReference;
  unsigned long carrierFrequencyOverride = 0;
  unsigned long symbolRateOverride = 0;
  if (!m_parserSvct.GetVirtualChannel(index,
                                      *transmissionMedium,
                                      *vctId,
                                      *splice,
                                      *activationTime,
                                      *hdtvChannel,
                                      *preferredSource,
                                      virtualChannelNumber,
                                      *applicationVirtualChannel,
                                      *bitstreamSelect,
                                      *pathSelect,
                                      *toneSelect,
                                      *transportType,
                                      *channelType,
                                      *sourceId,
                                      *nvodChannelBase,
                                      cdtReference,
                                      *programNumber,
                                      mmtReference,
                                      *accessControlled,
                                      *videoStandard,
                                      *wideBandwidthAudio,
                                      *compandedAudio,
                                      *matrixMode,
                                      *subcarrier2Offset,
                                      *subcarrier1Offset,
                                      *satelliteId,
                                      *transponderNumber,
                                      *suppressVideo,
                                      *audioSelection,
                                      carrierFrequencyOverride,
                                      symbolRateOverride,
                                      *majorChannelNumber,
                                      *minorChannelNumber,
                                      *transportStreamId,
                                      *outOfBand,
                                      *hideGuide,
                                      *serviceType))
  {
    return false;
  }

  if (*majorChannelNumber == 0 && *minorChannelNumber == 0)
  {
    *majorChannelNumber = virtualChannelNumber;
  }

  if (sourceName == NULL)
  {
    *sourceNameBufferSize = 0;
  }
  else if (!m_parserNtt.GetSourceName(*transmissionMedium,
                                      *applicationVirtualChannel,
                                      *sourceId,
                                      *sourceNameLanguage,
                                      sourceName,
                                      *sourceNameBufferSize))
  {
    LogDebug(L"SI ATSC/SCTE %d: missing source name, transmission medium = %hhu, application VC = %d, source ID = %hu",
              m_sectionDecoder.GetPid(), *transmissionMedium,
              *applicationVirtualChannel, *sourceId);
  }

  if (mapName == NULL)
  {
    *mapNameBufferSize = 0;
  }
  else if (!m_parserNtt.GetMapName(*transmissionMedium,
                                    *vctId,
                                    *mapNameLanguage,
                                    mapName,
                                    *mapNameBufferSize) && *vctId != 0)
  {
    LogDebug(L"SI ATSC/SCTE %d: missing VCT/map name, transmission medium = %hhu, VCT ID = %hu",
              m_sectionDecoder.GetPid(), *transmissionMedium, *vctId);
  }

  if (*channelType == 3 || *transmissionMedium != 1)  // NVOD access or non-satellite
  {
    if (transponderName == NULL || *transponderNameBufferSize == 0)
    {
      *transponderNameBufferSize = 0;
    }
    else
    {
      transponderName[0] = NULL;
      *transponderNameBufferSize = 1;
    }
    if (satelliteReferenceName == NULL && *satelliteReferenceNameBufferSize == 0)
    {
      *satelliteReferenceNameBufferSize = 0;
    }
    else
    {
      satelliteReferenceName[0] = NULL;
      *satelliteReferenceNameBufferSize = 1;
    }
    if (satelliteFullName != NULL && *satelliteFullNameBufferSize != 0)
    {
      *satelliteFullNameBufferSize = 0;
    }
    else
    {
      satelliteFullName[0] = NULL;
      *satelliteFullNameBufferSize = 1;
    }

    if (*channelType == 3)
    {
      return true;
    }
  }
  else
  {
    if (transponderName == NULL)
    {
      *transponderNameBufferSize = 0;
    }
    else if (!m_parserNtt.GetTransponderName(*transmissionMedium,
                                              *satelliteId,
                                              *transponderNumber,
                                              *transponderNameLanguage,
                                              transponderName,
                                              *transponderNameBufferSize))
    {
      LogDebug(L"SI ATSC/SCTE %d: missing transponder name, satellite ID = %hhu, transponder number = %hhu",
                m_sectionDecoder.GetPid(), *satelliteId, *transponderNumber);
    }

    if (satelliteReferenceName == NULL && satelliteFullName == NULL)
    {
      *satelliteReferenceNameBufferSize = 0;
      *satelliteFullNameBufferSize = 0;
    }
    else if (!m_parserNtt.GetSatelliteText(*transmissionMedium,
                                            *satelliteId,
                                            *satelliteNameLanguage,
                                            satelliteReferenceName,
                                            *satelliteReferenceNameBufferSize,
                                            satelliteFullName,
                                            *satelliteFullNameBufferSize))
    {
      LogDebug(L"SI ATSC/SCTE %d: missing satellite name, satellite ID = %hhu",
                m_sectionDecoder.GetPid(), *satelliteId);
    }

    unsigned char numberOfTransponders;
    if (!m_parserNit.GetSatelliteInformation(*transmissionMedium,
                                              *satelliteId,
                                              *youAreHere,
                                              *frequencyBand,
                                              *outOfService,
                                              *hemisphere,
                                              *orbitalPosition,
                                              *polarisationType,
                                              numberOfTransponders))
    {
      LogDebug(L"SI ATSC/SCTE %d: missing satellite information, satellite ID = %hhu",
                m_sectionDecoder.GetPid(), *satelliteId);
    }

    unsigned short vctId2 = 0;
    unsigned long carrierFrequencyOverride2 = 0;
    if (!m_parserNit.GetTransponderData(*transmissionMedium,
                                        *satelliteId,
                                        *transponderNumber,
                                        *transportType,
                                        *polarisation,
                                        cdtReference,
                                        mmtReference,
                                        vctId2,
                                        *rootTransponder,
                                        *wideBandwidthVideo,
                                        *waveformStandard,
                                        *wideBandwidthAudio,
                                        *compandedAudio,
                                        *matrixMode,
                                        *subcarrier2Offset,
                                        *subcarrier1Offset,
                                        carrierFrequencyOverride2))
    {
      LogDebug(L"SI ATSC/SCTE %d: missing transponder data, satellite ID = %hhu, transponder number = %hhu",
                m_sectionDecoder.GetPid(), *satelliteId, *transponderNumber);
    }
    else if (carrierFrequencyOverride == 0)
    {
      carrierFrequencyOverride = carrierFrequencyOverride2;
    }
  }

  if (carrierFrequencyOverride != 0)
  {
    *frequency = carrierFrequencyOverride;
  }
  else if (!m_parserNit.GetCarrierDefinition(cdtReference,
                                              *transmissionMedium,
                                              *frequency))
  {
    LogDebug(L"SI ATSC/SCTE %d: missing carrier definition, CDT reference = %hhu, transmission medium = %hhu",
              m_sectionDecoder.GetPid(), cdtReference, *transmissionMedium);
  }

  if (*transportType == 0)  // MPEG 2
  {
    if (!m_parserNit.GetModulationMode(mmtReference,
                                        *transmissionMedium,
                                        *transmissionSystem,
                                        *innerCodingMode,
                                        *splitBitstreamMode,
                                        *modulationFormat,
                                        *symbolRate))
    {
      LogDebug(L"SI ATSC/SCTE %d: missing modulation mode, MMT reference = %hhu, transmission medium = %hhu",
                m_sectionDecoder.GetPid(), mmtReference, *transmissionMedium);
    }
    if (symbolRateOverride != 0)
    {
      *symbolRate = symbolRateOverride;
    }
  }
  return true;
}

STDMETHODIMP_(unsigned short) CGrabberSiAtscScte::GetSvctDefinedChannelCount()
{
  return m_parserSvct.GetDefinedChannelCount();
}

STDMETHODIMP_(bool) CGrabberSiAtscScte::GetSvctDefinedChannel(unsigned short index,
                                                              unsigned char* transmissionMedium,
                                                              unsigned short* vctId,
                                                              unsigned short* virtualChannelNumber)
{
  return m_parserSvct.GetDefinedChannel(index,
                                        *transmissionMedium,
                                        *vctId,
                                        *virtualChannelNumber);
}

STDMETHODIMP_(void) CGrabberSiAtscScte::OnOutOfBandSectionReceived(unsigned char* sectionData,
                                                                    unsigned short sectionDataBufferSize)
{
  CSection s;
  unsigned short pid = (sectionData[0] << 8) | sectionData[1];
  s.AppendData(&sectionData[2], min(sizeof(s.Data), sectionDataBufferSize - 2));
  if (!s.IsComplete())
  {
    LogDebug(L"SI ATSC/SCTE %hu: received incomplete out-of-band section, section data buffer size = %hu, section length = %d",
              pid, sectionDataBufferSize, s.section_length);
    return;
  }
  else if (m_enableCrcCheck && !s.IsValid())
  {
    LogDebug(L"SI ATSC/SCTE %hu: received invalid section", pid);
    return;
  }
  OnNewSection(pid, s.table_id, s, true);
}

void CGrabberSiAtscScte::OnTableSeen(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableSeen(m_sectionDecoder.GetPid(), tableId);
  }
  if (m_callBackSiAtscScte != NULL)
  {
    m_callBackSiAtscScte->OnTableSeen(tableId);
  }
}

void CGrabberSiAtscScte::OnTableComplete(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableComplete(m_sectionDecoder.GetPid(), tableId);
  }
  if (m_callBackSiAtscScte != NULL)
  {
    m_callBackSiAtscScte->OnTableComplete(tableId);
  }
}

void CGrabberSiAtscScte::OnTableChange(unsigned char tableId)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackGrabber != NULL)
  {
    m_callBackGrabber->OnTableChange(m_sectionDecoder.GetPid(), tableId);
  }
  if (m_callBackSiAtscScte != NULL)
  {
    m_callBackSiAtscScte->OnTableChange(tableId);
  }
}

void CGrabberSiAtscScte::OnEamReceived(unsigned short id,
                                        unsigned long originatorCode,
                                        const char* eventCode,
                                        const map<unsigned long, char*>& NatureOfActivationTexts,
                                        unsigned char alertMessageTimeRemaining,
                                        unsigned long eventStartTime,
                                        unsigned short eventDuration,
                                        unsigned char alertPriority,
                                        unsigned short detailsOobSourceId,
                                        unsigned short detailsMajorChannelNumber,
                                        unsigned short detailsMinorChannelNumber,
                                        unsigned char detailsRfChannel,
                                        unsigned short detailsProgramNumber,
                                        unsigned short audioOobSourceId,
                                        const map<unsigned long, char*>& alertTexts,
                                        const vector<unsigned long>& locationCodes,
                                        const vector<unsigned long>& exceptions,
                                        const vector<unsigned long>& alternativeExceptions)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiAtscScte == NULL)
  {
    m_callBackSiAtscScte->OnEamReceived(id,
                                        originatorCode,
                                        eventCode,
                                        NatureOfActivationTexts,
                                        alertMessageTimeRemaining,
                                        eventStartTime,
                                        eventDuration,
                                        alertPriority,
                                        detailsOobSourceId,
                                        detailsMajorChannelNumber,
                                        detailsMinorChannelNumber,
                                        detailsRfChannel,
                                        detailsProgramNumber,
                                        audioOobSourceId,
                                        alertTexts,
                                        locationCodes,
                                        exceptions,
                                        alternativeExceptions);
  }
}

void CGrabberSiAtscScte::OnEamChanged(unsigned short id,
                                        unsigned long originatorCode,
                                        const char* eventCode,
                                        const map<unsigned long, char*>& NatureOfActivationTexts,
                                        unsigned char alertMessageTimeRemaining,
                                        unsigned long eventStartTime,
                                        unsigned short eventDuration,
                                        unsigned char alertPriority,
                                        unsigned short detailsOobSourceId,
                                        unsigned short detailsMajorChannelNumber,
                                        unsigned short detailsMinorChannelNumber,
                                        unsigned char detailsRfChannel,
                                        unsigned short detailsProgramNumber,
                                        unsigned short audioOobSourceId,
                                        const map<unsigned long, char*>& alertTexts,
                                        const vector<unsigned long>& locationCodes,
                                        const vector<unsigned long>& exceptions,
                                        const vector<unsigned long>& alternativeExceptions)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiAtscScte == NULL)
  {
    m_callBackSiAtscScte->OnEamChanged(id,
                                        originatorCode,
                                        eventCode,
                                        NatureOfActivationTexts,
                                        alertMessageTimeRemaining,
                                        eventStartTime,
                                        eventDuration,
                                        alertPriority,
                                        detailsOobSourceId,
                                        detailsMajorChannelNumber,
                                        detailsMinorChannelNumber,
                                        detailsRfChannel,
                                        detailsProgramNumber,
                                        audioOobSourceId,
                                        alertTexts,
                                        locationCodes,
                                        exceptions,
                                        alternativeExceptions);
  }
}

void CGrabberSiAtscScte::OnEamRemoved(unsigned short id,
                                        unsigned long originatorCode,
                                        const char* eventCode,
                                        const map<unsigned long, char*>& NatureOfActivationTexts,
                                        unsigned char alertMessageTimeRemaining,
                                        unsigned long eventStartTime,
                                        unsigned short eventDuration,
                                        unsigned char alertPriority,
                                        unsigned short detailsOobSourceId,
                                        unsigned short detailsMajorChannelNumber,
                                        unsigned short detailsMinorChannelNumber,
                                        unsigned char detailsRfChannel,
                                        unsigned short detailsProgramNumber,
                                        unsigned short audioOobSourceId,
                                        const map<unsigned long, char*>& alertTexts,
                                        const vector<unsigned long>& locationCodes,
                                        const vector<unsigned long>& exceptions,
                                        const vector<unsigned long>& alternativeExceptions)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiAtscScte == NULL)
  {
    m_callBackSiAtscScte->OnEamRemoved(id,
                                        originatorCode,
                                        eventCode,
                                        NatureOfActivationTexts,
                                        alertMessageTimeRemaining,
                                        eventStartTime,
                                        eventDuration,
                                        alertPriority,
                                        detailsOobSourceId,
                                        detailsMajorChannelNumber,
                                        detailsMinorChannelNumber,
                                        detailsRfChannel,
                                        detailsProgramNumber,
                                        audioOobSourceId,
                                        alertTexts,
                                        locationCodes,
                                        exceptions,
                                        alternativeExceptions);
  }
}

void CGrabberSiAtscScte::OnMgtReceived(unsigned short tableType,
                                        unsigned short pid,
                                        unsigned char versionNumber,
                                        unsigned long numberBytes)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiAtscScte == NULL)
  {
    m_callBackSiAtscScte->OnMgtReceived(tableType, pid, versionNumber, numberBytes);
  }
}

void CGrabberSiAtscScte::OnMgtChanged(unsigned short tableType,
                                        unsigned short pid,
                                        unsigned char versionNumber,
                                        unsigned long numberBytes)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiAtscScte == NULL)
  {
    m_callBackSiAtscScte->OnMgtChanged(tableType, pid, versionNumber, numberBytes);
  }
}

void CGrabberSiAtscScte::OnMgtRemoved(unsigned short tableType,
                                        unsigned short pid,
                                        unsigned char versionNumber,
                                        unsigned long numberBytes)
{
  CEnterCriticalSection lock(m_section);
  if (m_callBackSiAtscScte == NULL)
  {
    m_callBackSiAtscScte->OnMgtRemoved(tableType, pid, versionNumber, numberBytes);
  }
}