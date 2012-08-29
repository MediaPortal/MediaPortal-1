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
}

CChannelScan::~CChannelScan(void)
{
  CleanUp();
}

void CChannelScan::CleanUp()
{
  vector<CPmtParser*>::iterator it = m_vPmtParsers.begin();
  while (it != m_vPmtParsers.end())
  {
    CPmtParser* parser = *it;
    delete parser;
    parser = NULL;
    it++;
  }
  m_vPmtParsers.clear();
}

STDMETHODIMP CChannelScan::SetCallBack(IChannelScanCallBack* callBack)
{
  m_pCallBack = callBack;
  return S_OK;
}

STDMETHODIMP CChannelScan::ScanStream(TransmissionStandard transmissionStandard)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    CleanUp();
    m_mServices.clear();

    m_transmissionStandard = transmissionStandard;
    m_bIsScanningNetwork = false;
    m_bIsScanning = true;
    m_patParser.SetCallBack(this);
    m_sdtParser.SetCallBack(this);
    m_vctParser.SetCallBack(this);
    m_patParser.Reset();
    m_sdtParser.Reset();
    m_nitParser.Reset();
    m_batParser.Reset();
    m_vctParser.Reset();
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
    m_bIsScanning = false;
    m_pCallBack = NULL;
    m_patParser.SetCallBack(NULL);
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
                                       long* transportId,
                                       long* serviceId,
                                       char** serviceName,
                                       char** providerName,
                                       char** networkNames,
                                       char** logicalChannelNumber,
                                       int* serviceType,
                                       int* hasVideo,
                                       int* hasAudio,
                                       int* isEncrypted,
                                       int* hasCaDescriptor,
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
    *transportId = info->TransportId;
    *serviceId = info->ServiceId;
    strcpy(sServiceName, info->ServiceName);
    *serviceName = sServiceName;
    strcpy(sProviderName, info->ProviderName);
    *providerName = sProviderName;
    if (m_transmissionStandard == Atsc || m_transmissionStandard == Scte)
    {
      strcpy(sNetworkNames, "");
      strcpy(sLogicalChannelNumber, info->LogicalChannelNumber);
    }
    else
    {
      // Concatenate available network and bouquet strings with a ",," separator.
      strcpy(sNetworkNames, "");
      vector<char*>* names = m_nitParser.GetGroupNames(info->NetworkId, info->TransportId, info->ServiceId);
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

      int tempLcn = m_nitParser.GetLogicialChannelNumber(info->NetworkId, info->TransportId, info->ServiceId);
      if (tempLcn <= 0 || tempLcn == 10000)
      {
        tempLcn = m_batParser.GetLogicialChannelNumber(info->NetworkId, info->TransportId, info->ServiceId);
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
    m_vMultiplexes.clear();
    m_bIsScanning = true;
    m_bIsScanningNetwork = true;
    m_transmissionStandard = Dvb;   // Only DVB network scanning is supported. Other standards don't seem to have network information tables.
    m_nitParser.Reset();
    m_batParser.Reset();
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in ScanNetwork()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::StopNetworkScan()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    m_bIsScanning = false;
    m_bIsScanningNetwork = false;

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
    if (m_transmissionStandard == Atsc || m_transmissionStandard == Scte)
    {
      isReady = m_vctParser.IsReady();
    }
    else
    {
      isReady = m_nitParser.IsReady() && !m_batParser.IsReady() && (m_bIsScanningNetwork || m_sdtParser.IsReady());
    }
    if (!m_bIsScanningNetwork)
    {
      if (!m_patParser.IsReady())
      {
        isReady = false;
      }
      if (isReady)
      {
        map<int, CChannelInfo*>::iterator it = m_mServices.begin();
        while (it != m_mServices.end())
        {
          if (!(*it->second).IsPmtReceived && (*it->second).IsRunning)
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

    if (m_transmissionStandard == Atsc || m_transmissionStandard == Scte)
    {
      m_vctParser.OnTsPacket(tsPacket);
    }
    else
    {
      m_nitParser.OnTsPacket(tsPacket);
      m_batParser.OnTsPacket(tsPacket);
      if (!m_bIsScanningNetwork)
      {
        m_sdtParser.OnTsPacket(tsPacket);
      }
    }
    if (!m_bIsScanningNetwork)
    {
      m_patParser.OnTsPacket(tsPacket);
      vector<CPmtParser*>::iterator it = m_vPmtParsers.begin();
      while (it != m_vPmtParsers.end())
      {
        CPmtParser *parser = *it;
        parser->OnTsPacket(tsPacket);
        it++;
      }
    }
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
    map<int, CChannelInfo*>::iterator it = m_mServices.find(serviceId);
    if (it == m_mServices.end())
    {
      CChannelInfo info;
      info.ServiceId = serviceId;
      info.PmtPid = pmtPid;
      m_mServices[serviceId] = &info;
    }
    else
    {
      (*it->second).PmtPid = pmtPid;
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
    bool addService = true;
    CChannelInfo info;
    if (it != m_mServices.end())
    {
      addService = false;
      info = *it->second;
      if (info.IsServiceInfoReceived)
      {
        LogDebug("ChannelScan: SDT information for service 0x%x received multiple times", sdtInfo.ServiceId);
      }
    }

    info.NetworkId = sdtInfo.NetworkId;
    info.TransportId = sdtInfo.TransportId;
    info.ServiceId = sdtInfo.ServiceId;
    info.ServiceType = sdtInfo.ServiceType;
    info.IsEncrypted = sdtInfo.IsEncrypted;  // we trust free_ca_mode over PMT CA descriptors
    info.IsRunning = sdtInfo.IsRunning;      // we trust running_status over PMT having been received or not
    info.IsOtherMux = sdtInfo.IsOtherMux;
    strcpy(info.ProviderName, sdtInfo.ProviderName);
    strcpy(info.ServiceName, sdtInfo.ServiceName);
    info.IsServiceInfoReceived = true;
    LogDebug("ChannelScan: SDT information found for service 0x%x", sdtInfo.ServiceId);
    if (addService)
    {
      m_mServices[info.ServiceId] = &info;
    }
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
    bool addService = true;
    CChannelInfo info;
    if (it != m_mServices.end())
    {
      addService = false;
      info = *it->second;
      if (info.IsServiceInfoReceived)
      {
        LogDebug("ChannelScan: VCT information for service 0x%x received multiple times", vctInfo.ServiceId);
      }
    }

    info.NetworkId = vctInfo.NetworkId;
    info.TransportId = vctInfo.TransportId;
    info.ServiceId = vctInfo.ServiceId;
    info.ServiceType = vctInfo.ServiceType;
    if (!info.IsServiceInfoReceived)
    {
      info.HasVideo = vctInfo.HasVideo;
      info.HasAudio = vctInfo.HasAudio;
    }
    info.IsEncrypted = vctInfo.IsEncrypted;  // we trust access_controlled over PMT CA descriptors
    info.IsRunning = vctInfo.IsRunning;      // we trust VCT info over PMT having been received or not
    info.IsOtherMux = vctInfo.IsOtherMux;
    strcpy(info.ProviderName, vctInfo.ProviderName);
    strcpy(info.ServiceName, vctInfo.ServiceName);
    strcpy(info.LogicalChannelNumber, vctInfo.LogicalChannelNumber);
    info.IsServiceInfoReceived = true;
    LogDebug("ChannelScan: VCT information found for service 0x%x", vctInfo.ServiceId);
    if (addService)
    {
      m_mServices[info.ServiceId] = &info;
    }
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
    bool addService = true;
    CChannelInfo info;
    if (it != m_mServices.end())
    {
      addService = false;
      info = *it->second;
      if (info.IsPmtReceived)
      {
        LogDebug("ChannelScan: PMT information for service 0x%x received multiple times", pidTable.ServiceId);
      }
    }

    info.ServiceId = pidTable.ServiceId;
    info.HasVideo = (int)pidTable.videoPids.size();
    info.HasAudio = (int)pidTable.audioPids.size();
    if (!info.IsServiceInfoReceived)
    {
      info.IsEncrypted = pidTable.ConditionalAccessDescriptorCount > 0;
      info.IsRunning = true;
    }
    info.IsOtherMux = false;
    info.IsPmtReceived = true;
    LogDebug("ChannelScan: PMT information found for service 0x%x received from PID 0x%x", pidTable.ServiceId, pidTable.PmtPid);
    if (addService)
    {
      LogDebug("ChannelScan: received PMT information for service not seen in PAT");
      m_mServices[info.ServiceId] = &info;
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnPmtReceived()");
  }
}

/*

void CPatParser::Dump()
{
  if (m_bDumped) return;
  m_bDumped=true;
  int i=0;
  itChannels it=m_mapChannels.begin();
  while (it!=m_mapChannels.end()) 
  {
    CChannelInfo& info=it->second;
    LogDebug("%4d)  p:%-15s s:%-25s  onid:%4x tsid:%4x sid:%4x major:%3d minor:%3x freq:%3x type:%3d pmt:%4x othermux:%d freeca:%d hasVideo:%d hasAudio:%d hasCaDescriptor:%d",i,
            info.ProviderName,info.ServiceName,info.NetworkId,info.TransportId,info.ServiceId,info.MajorChannel,info.MinorChannel,info.Frequency,
            info.ServiceType,info.PidTable.PmtPid,info.OtherMux,info.FreeCAMode,info.hasVideo,info.hasAudio,info.hasCaDescriptor);

    it++;
    i++;
  }
}

*/