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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <streams.h>

#include "ChannelScan.h"
#include "..\..\shared\ChannelInfo.h"
#include "TsWriter.h"

extern void LogDebug(const char *fmt, ...) ;

CChannelScan::CChannelScan(LPUNKNOWN pUnk, HRESULT *phr, CMpTsFilter* filter) 
  : CUnknown( NAME ("MpTsChannelScan"), pUnk)
{
  m_bIsScanning = false;
  m_bIsScanningNetwork = false;
  m_pFilter = filter;
  m_pCallBack = NULL;
  m_pEncryptionAnalyser = new CEncryptionAnalyser(GetOwner(), phr);
  if (m_pEncryptionAnalyser == NULL)
  {
    *phr = E_OUTOFMEMORY;
  }
}

CChannelScan::~CChannelScan(void)
{
  CleanUp();
  delete m_pEncryptionAnalyser;
  m_pEncryptionAnalyser = NULL;
}

void CChannelScan::CleanUp()
{
  map<int, CChannelInfo*>::iterator it = m_mServices.begin();
  while (it != m_mServices.end())
  {
    CChannelInfo* info = it->second;
    delete info;
    info = NULL;
    it++;
  }
  m_mServices.clear();

  vector<CPmtParser*>::iterator it2 = m_vPmtParsers.begin();
  while (it2 != m_vPmtParsers.end())
  {
    CPmtParser* parser = *it2;
    delete parser;
    parser = NULL;
    it2++;
  }
  m_vPmtParsers.clear();
}

STDMETHODIMP CChannelScan::SetCallBack(IChannelScanCallBack* callBack)
{
  m_pCallBack = callBack;
  return S_OK;
}

STDMETHODIMP CChannelScan::ScanStream(BroadcastStandard broadcastStandard)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    CleanUp();
    m_mPids.clear();

    LogDebug("ChannelScan: start scanning stream, broadcast standard = %d", broadcastStandard);
    m_broadcastStandard = broadcastStandard;
    m_bIsScanningNetwork = false;
    m_bIsScanning = true;
    m_patParser.Reset();
    m_pEncryptionAnalyser->Reset();
    m_sdtParser.Reset(false);
    m_nitParser.Reset();
    m_batParser.Reset();
    m_vctParser.Reset();
    m_patParser.SetCallBack(this);
    m_pEncryptionAnalyser->SetCallBack(this);
    m_sdtParser.SetCallBack(this);
    m_vctParser.SetCallBack(this);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in ScanStream()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::StopStreamScan()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    LogDebug("ChannelScan: stop scanning stream");
    m_bIsScanning = false;
    m_pCallBack = NULL;
    m_patParser.SetCallBack(NULL);
    m_pEncryptionAnalyser->SetCallBack(NULL);
    m_sdtParser.SetCallBack(NULL);
    m_vctParser.SetCallBack(NULL);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in StopStreamScan()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetServiceCount(int* serviceCount)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    *serviceCount = (int)m_mServices.size();
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetServiceCount()");
    *serviceCount = 0;
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetService(int index,
                                       long* networkId,
                                       long* transportStreamId,
                                       long* serviceId,
                                       char** serviceName,
                                       char** providerName,
                                       char** networkNames,
                                       char** logicalChannelNumber,
                                       int* serviceType,
                                       int* hasVideo,
                                       int* hasAudio,
                                       bool* isEncrypted,
                                       int* pmtPid)
{
  static char sServiceName[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
  static char sProviderName[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
  static char sNetworkNames[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
  static char sLogicalChannelNumber[CHANNEL_INFO_MAX_STRING_LENGTH + 1];
  CEnterCriticalSection enter(m_section);
  try
  {
    if (index < 0 || index >= (int)m_mServices.size())
    {
      LogDebug("ChannelScan: attempted to retrieve service details with invalid index %d, service count = %d", index, m_mServices.size());
      return S_FALSE;
    }

    map<int, CChannelInfo*>::iterator it = m_mServices.begin();
    while (index > 0)
    {
      it++;
      index--;
    }

    CChannelInfo* info = it->second;
    *networkId = info->NetworkId;
    *transportStreamId = info->TransportStreamId;
    *serviceId = info->ServiceId;
    strcpy(sServiceName, info->ServiceName);
    *serviceName = sServiceName;
    strcpy(sProviderName, info->ProviderName);
    *providerName = sProviderName;
    if (m_broadcastStandard == Atsc || m_broadcastStandard == Scte)
    {
      strcpy(sNetworkNames, "");
      strcpy(sLogicalChannelNumber, info->LogicalChannelNumber);
    }
    else
    {
      // Concatenate available network and bouquet strings with a ",," separator.
      strcpy(sNetworkNames, "");
      vector<char*>* names = m_nitParser.GetGroupNames(info->NetworkId, info->TransportStreamId, info->ServiceId);
      if (names->size() > 0)
      {
        int offset = 0;
        unsigned int i = 0;
        while (i < names->size())
        {
          int stringLength = strlen((*names)[i]);
          if (offset + stringLength + 2 >= CHANNEL_INFO_MAX_STRING_LENGTH)  // + 2 for the ",," separator
          {
            continue;
          }
          if (offset != 0)
          {
            strcat(sNetworkNames, ",,");  // used to separate strings in the case where multiple network/bouquet names are available
          }
          strcat(sNetworkNames, (*names)[i]);
          i++;
        }
      }

      int tempLcn = m_nitParser.GetLogicialChannelNumber(info->NetworkId, info->TransportStreamId, info->ServiceId);
      if (tempLcn <= 0 || tempLcn == 10000)
      {
        tempLcn = m_batParser.GetLogicialChannelNumber(info->NetworkId, info->TransportStreamId, info->ServiceId);
      }
      sprintf(sLogicalChannelNumber, "%d", tempLcn);
    }
    *networkNames = sNetworkNames;
    *logicalChannelNumber = sLogicalChannelNumber;
    *serviceType = info->ServiceType;
    *hasVideo = info->HasVideo;
    *hasAudio = info->HasAudio;
    *isEncrypted = info->IsEncrypted;
    *pmtPid = info->PmtPid;
    LogDebug("%4d) %-25s provider = %-15s, ONID = 0x%4x, TSID = 0x%4x, SID = 0x%4x, LCN = %7s, type = %3d",
            sServiceName, sProviderName, info->NetworkId, info->TransportStreamId, info->ServiceId, sLogicalChannelNumber, info->ServiceType);
    LogDebug("       has video = %1d, has audio = %1d, is encrypted = %1d, is running = %1d, is other mux = %1d",
            info->HasVideo, info->HasAudio, info->IsEncrypted, info->IsRunning, info->IsOtherMux);
    LogDebug("       is PMT received = %1d, is SDT/VCT received = %1d, is PID received = %1d",
            info->IsPmtReceived, info->IsServiceInfoReceived, info->IsPidReceived);
    LogDebug("       network/bouquet names = %s", sNetworkNames);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetService()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::ScanNetwork()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    CleanUp();
    m_mPids.clear();
    m_vMultiplexes.clear();

    LogDebug("ChannelScan: start scanning network");
    m_bIsScanning = true;
    m_bIsScanningNetwork = true;
    m_broadcastStandard = Dvb;   // Only DVB network scanning is supported. Other standards don't seem to have network information tables.
    m_bIsOtherMuxServiceInfoSeen = false;
    m_patParser.Reset();
    m_pEncryptionAnalyser->Reset();
    m_sdtParser.Reset(true);
    m_nitParser.Reset();
    m_batParser.Reset();
    m_patParser.SetCallBack(this);
    m_pEncryptionAnalyser->SetCallBack(this);
    m_sdtParser.SetCallBack(this);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in ScanNetwork()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::StopNetworkScan(bool* isOtherMuxServiceInfoAvailable)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    LogDebug("ChannelScan: stop scanning network");
    m_bIsScanning = false;
    m_bIsScanningNetwork = false;
    m_pCallBack = NULL;
    m_patParser.SetCallBack(NULL);
    m_pEncryptionAnalyser->SetCallBack(NULL);
    m_sdtParser.SetCallBack(NULL);
    m_vctParser.SetCallBack(NULL);

    *isOtherMuxServiceInfoAvailable = m_bIsOtherMuxServiceInfoSeen;

    // Merge the results from the NIT and BAT scanner. The NIT and BAT result sets are distinct
    // within themselves so we only need to check whether items in the second set are already
    // present in the first set.
    int nitMuxCount = m_nitParser.GetMultiplexCount();
    for (int i = 0; i < nitMuxCount; i++)
    {
      NitMultiplexDetail* mux = m_nitParser.GetMultiplexDetail(i);
      if (mux != NULL)
      {
        m_vMultiplexes.push_back(mux);
      }
    }
    int batMuxCount = m_batParser.GetMultiplexCount();
    for (int i = 0; i < batMuxCount; i++)
    {
      bool alreadyAdded = false;
      NitMultiplexDetail* mux = m_batParser.GetMultiplexDetail(i);
      NitCableMultiplexDetail* cableMux = dynamic_cast<NitCableMultiplexDetail*>(mux);
      if (cableMux != NULL)
      {
        for (int j = 0; j < nitMuxCount; j++)
        {
          if (cableMux->Equals(m_vMultiplexes[j]))
          {
            alreadyAdded = true;
            break;
          }
        }
      }
      else
      {
        NitSatelliteMultiplexDetail* satelliteMux = dynamic_cast<NitSatelliteMultiplexDetail*>(mux);
        if (satelliteMux != NULL)
        {
          for (int j = 0; j < nitMuxCount; j++)
          {
            if (satelliteMux->Equals(m_vMultiplexes[j]))
            {
              alreadyAdded = true;
              break;
            }
          }
        }
        else
        {
          NitTerrestrialMultiplexDetail* terrestrialMux = dynamic_cast<NitTerrestrialMultiplexDetail*>(mux);
          if (terrestrialMux != NULL)
          {
            for (int j = 0; j < nitMuxCount; j++)
            {
              if (terrestrialMux->Equals(m_vMultiplexes[j]))
              {
                alreadyAdded = true;
                break;
              }
            }
          }
          else
          {
            LogDebug("ChannelScan: unhandled multiplex type in StopNetworkScan()");
            alreadyAdded = true;
          }
        }
      }

      if (!alreadyAdded)
      {
        m_vMultiplexes.push_back(mux);
      }
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in StopNetworkScan()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetMultiplexCount(int* multiplexCount)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    *multiplexCount = (int)m_vMultiplexes.size();
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetMultiplexCount()");
    *multiplexCount = 0;
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetMultiplex(int index,
                                          int* networkId,
                                          int* transportStreamId,
                                          int* type,
                                          int* frequency,
                                          int *polarisation,
                                          int* modulation,
                                          int* symbolRate,
                                          int* bandwidth,
                                          int* innerFecRate,
                                          int* rollOff)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (index < 0 || index >= (int)m_vMultiplexes.size())
    {
      LogDebug("ChannelScan: attempted to retrieve multiplex details with invalid index %d, multiplex count = %d", index, m_vMultiplexes.size());
      return S_FALSE;
    }
    NitMultiplexDetail* mux = m_vMultiplexes[index];
    if (mux == NULL)
    {
      LogDebug("ChannelScan: multiplex is NULL, index = %d, multiplex count = %d", index, m_vMultiplexes.size());
      return S_FALSE;
    }

    *networkId = mux->NetworkId;
    *transportStreamId = mux->TransportStreamId;

    NitCableMultiplexDetail* cableMux = dynamic_cast<NitCableMultiplexDetail*>(mux);
    if (cableMux != NULL)
    {
      *frequency = cableMux->Frequency;
      *modulation = cableMux->Modulation;
      *symbolRate = cableMux->SymbolRate;
      *innerFecRate = cableMux->InnerFecRate;
      *type = 0;

      *polarisation = BDA_POLARISATION_NOT_SET;
      *bandwidth = 0;
      *rollOff = BDA_ROLL_OFF_NOT_SET;
      return S_OK;
    }

    NitSatelliteMultiplexDetail* satelliteMux = dynamic_cast<NitSatelliteMultiplexDetail*>(mux);
    if (satelliteMux != NULL)
    {
      *frequency = satelliteMux->Frequency;
      *polarisation = satelliteMux->Polarisation;
      *modulation = satelliteMux->Modulation;
      *symbolRate = satelliteMux->SymbolRate;
      *innerFecRate = satelliteMux->InnerFecRate;
      *rollOff = satelliteMux->RollOff;
      *type = 1;

      *bandwidth = 0;
      return S_OK;
    }

    NitTerrestrialMultiplexDetail* terrestrialMux = dynamic_cast<NitTerrestrialMultiplexDetail*>(mux);
    if (terrestrialMux != NULL)
    {
      *frequency = terrestrialMux->CentreFrequency;
      *bandwidth = terrestrialMux->Bandwidth;
      *type = 2;

      *polarisation = BDA_POLARISATION_NOT_SET;
      *modulation = BDA_MOD_NOT_SET;
      *symbolRate = 0;
      *innerFecRate = BDA_BCC_RATE_NOT_SET;
      *rollOff = BDA_ROLL_OFF_NOT_SET;
      return S_OK;
    }

    LogDebug("ChannelScan: unhandled multiplex type in GetTransponder()");
    return S_FALSE;
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetTransponder()");
    return S_FALSE;
  }
  return S_OK;
}

void CChannelScan::OnTsPacket(byte* tsPacket)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (!m_bIsScanning)
    {
      return;
    }

    // Note: we continue parsing even when we think scanning is complete. The user
    // has the option to set a minimum scan time...
    bool isReady = true;
    if (m_broadcastStandard == Atsc || m_broadcastStandard == Scte)
    {
      isReady = m_vctParser.IsReady();
    }
    else
    {
      isReady = m_nitParser.IsReady() && m_batParser.IsReady() && m_sdtParser.IsReady();
    }
    if (isReady)
    {
      isReady = m_patParser.IsReady();
      if (isReady)
      {
        map<int, CChannelInfo*>::iterator it = m_mServices.begin();
        while (it != m_mServices.end())
        {
          // If the information from the encryption analyser hasn't been received and the
          // service is meant to be in this stream and is apparently running, then we're not ready.
          if (!(*it->second).IsPidReceived && !(*it->second).IsOtherMux && (*it->second).IsRunning)
          {
            isReady = false;
            break;
          }
          it++;
        }
      }
    }
    if (isReady)
    {
      LogDebug("ChannelScan: scanner finished");
      if (m_pCallBack != NULL)
      {
        LogDebug("ChannelScan: triggering callback");
        m_pCallBack->OnScannerDone();
      }
      m_pCallBack = NULL;
    }

    if (m_broadcastStandard == Atsc || m_broadcastStandard == Scte)
    {
      m_vctParser.OnTsPacket(tsPacket);
    }
    else
    {
      m_nitParser.OnTsPacket(tsPacket);
      m_batParser.OnTsPacket(tsPacket);
      m_sdtParser.OnTsPacket(tsPacket);
    }

    m_patParser.OnTsPacket(tsPacket);
    vector<CPmtParser*>::iterator it = m_vPmtParsers.begin();
    while (it != m_vPmtParsers.end())
    {
      CPmtParser* parser = *it;
      parser->OnTsPacket(tsPacket);
      it++;
    }
    m_pEncryptionAnalyser->OnTsPacket(tsPacket);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnTsPacket()");
  }
}

void CChannelScan::OnPatReceived(int serviceId, int pmtPid)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (m_pCallBack == NULL)
    {
      return;
    }

    map<int, CChannelInfo*>::iterator it = m_mServices.find(serviceId);
    if (it == m_mServices.end())
    {
      CChannelInfo* info = new CChannelInfo();
      info->ServiceId = serviceId;
      info->PmtPid = pmtPid;
      m_mServices[serviceId] = info;
    }
    else
    {
      it->second->PmtPid = pmtPid;
    }

    // We're not expecting there to be a PMT parser yet, but double check just in case.
    bool foundParser = false;
    vector<CPmtParser*>::iterator it2 = m_vPmtParsers.begin();
    CPmtParser* parser = NULL;
    while (it2 != m_vPmtParsers.end())
    {
      parser = *it2;
      int sid;
      int pid;
      parser->GetFilter(pid, sid);
      if (pid == pmtPid && sid == serviceId)
      {
        LogDebug("ChannelScan: PMT parser already exists");
        return;
      }
      it++;
    }
    parser = new CPmtParser();
    parser->SetFilter(pmtPid, serviceId);
    parser->SetCallBack(this);
    m_vPmtParsers.push_back(parser);
    LogDebug("ChannelScan: added PMT parser for service 0x%x, PMT PID = 0x%x", serviceId, pmtPid);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnPatReceived()");
  }
}

void CChannelScan::OnSdtReceived(const CChannelInfo& sdtInfo)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    map<int, CChannelInfo*>::iterator it = m_mServices.find(sdtInfo.ServiceId);

    // Do we have a channel with this service ID?
    CChannelInfo* info = NULL;
    if (it != m_mServices.end())
    {
      info = it->second;
      if (info->IsServiceInfoReceived)
      {
        LogDebug("ChannelScan: SDT information for service 0x%x received multiple times", sdtInfo.ServiceId);
      }
    }
    else
    {
      info = new CChannelInfo();
      m_mServices[sdtInfo.ServiceId] = info;
    }

    info->NetworkId = sdtInfo.NetworkId;
    info->TransportStreamId = sdtInfo.TransportStreamId;
    info->ServiceId = sdtInfo.ServiceId;
    info->ServiceType = sdtInfo.ServiceType;
    // We trust running_status and free_ca_mode over PMT being
    // received (or not) and CA descriptors being found in the PMT
    // (or not). However, information from the encryption analyser
    // takes higher precedence.
    if (!info->IsPidReceived)
    {
      info->IsEncrypted = sdtInfo.IsEncrypted;
      info->IsRunning = sdtInfo.IsRunning;
    }
    info->IsOtherMux = sdtInfo.IsOtherMux;
    if (sdtInfo.IsOtherMux)
    {
      m_bIsOtherMuxServiceInfoSeen = true;
    }
    strcpy(info->ProviderName, sdtInfo.ProviderName);
    strcpy(info->ServiceName, sdtInfo.ServiceName);
    info->IsServiceInfoReceived = true;
    LogDebug("ChannelScan: received SDT information for service 0x%x", sdtInfo.ServiceId);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnSdtReceived()");
  }
}

void CChannelScan::OnVctReceived(const CChannelInfo& vctInfo)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    map<int, CChannelInfo*>::iterator it = m_mServices.find(vctInfo.ServiceId);

    // Do we have a channel with this service ID?
    CChannelInfo* info = NULL;
    if (it != m_mServices.end())
    {
      info = it->second;
      if (info->IsServiceInfoReceived)
      {
        LogDebug("ChannelScan: VCT information for service 0x%x received multiple times", vctInfo.ServiceId);
      }
    }
    else
    {
      info = new CChannelInfo();
      m_mServices[vctInfo.ServiceId] = info;
    }

    info->NetworkId = vctInfo.NetworkId;
    info->TransportStreamId = vctInfo.TransportStreamId;
    info->ServiceId = vctInfo.ServiceId;
    info->ServiceType = vctInfo.ServiceType;
    // We trust PMT information over anything else for A/V present
    // status.
    if (!info->IsPmtReceived)
    {
      info->HasVideo = vctInfo.HasVideo;
      info->HasAudio = vctInfo.HasAudio;
    }
    // We trust VCT info and access_controlled over PMT being
    // received (or not) and CA descriptors being found in the PMT
    // (or not). However, information from the encryption analyser
    // takes higher precedence.
    if (!info->IsPidReceived)
    {
      info->IsEncrypted = vctInfo.IsEncrypted;
      info->IsRunning = vctInfo.IsRunning;
    }
    info->IsOtherMux = vctInfo.IsOtherMux;
    if (vctInfo.IsOtherMux)
    {
      m_bIsOtherMuxServiceInfoSeen = true;
    }
    strcpy(info->ProviderName, vctInfo.ProviderName);
    strcpy(info->ServiceName, vctInfo.ServiceName);
    strcpy(info->LogicalChannelNumber, vctInfo.LogicalChannelNumber);
    info->IsServiceInfoReceived = true;
    LogDebug("ChannelScan: received VCT information for service 0x%x", vctInfo.ServiceId);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnVctReceived()");
  }
}

void CChannelScan::OnPmtReceived(const CPidTable& pidTable)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    map<int, CChannelInfo*>::iterator it = m_mServices.find(pidTable.ServiceId);

    // Do we have a channel with this service ID?
    CChannelInfo* info = NULL;
    if (it != m_mServices.end())
    {
      info = it->second;
      if (info->IsPmtReceived)
      {
        LogDebug("ChannelScan: PMT information for service 0x%x received multiple times", pidTable.ServiceId);
      }
    }
    else
    {
      info = new CChannelInfo();
      m_mServices[pidTable.ServiceId] = info;
    }

    info->ServiceId = pidTable.ServiceId;
    // We trust PMT information over anything else for A/V present
    // status.
    info->HasVideo = (int)pidTable.videoPids.size();
    info->HasAudio = (int)pidTable.audioPids.size();
    // We trust service and encryption analyser information over the PMT information
    // for encryption and running status.
    if (!info->IsServiceInfoReceived && !info->IsPidReceived)
    {
      info->IsEncrypted = pidTable.ConditionalAccessDescriptorCount > 0;
      info->IsRunning = true;
    }
    info->IsOtherMux = false;
    info->IsPmtReceived = true;
    LogDebug("ChannelScan: PMT information for service 0x%x received from PID 0x%x", pidTable.ServiceId, pidTable.PmtPid);

    // Add an analyser for each video and audio PID.
    vector<VideoPid>::const_iterator vPidIt = pidTable.videoPids.begin();
    while (vPidIt != pidTable.videoPids.end())
    {
      m_pEncryptionAnalyser->AddPid(vPidIt->Pid);
      m_mPids[vPidIt->Pid] = pidTable.ServiceId;
    }
    vector<AudioPid>::const_iterator aPidIt = pidTable.audioPids.begin();
    while (aPidIt != pidTable.audioPids.end())
    {
      m_pEncryptionAnalyser->AddPid(aPidIt->Pid);
      m_mPids[aPidIt->Pid] = pidTable.ServiceId;
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnPmtReceived()");
  }
}

HRESULT CChannelScan::OnEncryptionStateChange(int pid, EncryptionState encryptionState)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    // Find the service that the PID is associated with (note the limitation that
    // a PID may only be associated with one service).
    map<int, int>::iterator it = m_mPids.find(pid);
    if (it == m_mPids.end())
    {
      LogDebug("ChannelScan: encryption state received for PID 0x%x that we don't know about", pid);
      return S_OK;
    }
    map<int, CChannelInfo*>::iterator it2 = m_mServices.find(it->second);
    if (it2 == m_mServices.end())
    {
      LogDebug("ChannelScan: encryption state received for PID 0x%x associated with service 0x%x that we don't know about", pid, it->second);
      return S_OK;
    }

    CChannelInfo* info = it2->second;
    info->IsRunning = true;
    // Have we already seen one or more of the PIDs for this service?
    // If yes, set the encrypted flag to true if any of the elementary streams are encrypted.
    // If no, assume that the encryption state for this elementary stream reflects the encryption state for the service.
    if (!info->IsPidReceived || !info->IsEncrypted || encryptionState == Encrypted)
    {
      info->IsEncrypted = (encryptionState == Encrypted);
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnEncryptionStateChange()");
  }
  return S_OK;
}