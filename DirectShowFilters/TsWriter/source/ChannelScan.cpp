/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "channelscan.h"
#include "..\..\shared\channelinfo.h"
#include "tswriter.h"

extern void LogDebug(const char* fmt, ...);

CChannelScan::CChannelScan(LPUNKNOWN pUnk, HRESULT *phr, CMpTsFilter* filter) 
  : CUnknown( NAME ("MpTsChannelScan"), pUnk), m_atscParser(PID_ATSC_BASE_PID)
{
  m_bIsParsingNIT=false;
	m_bIsParsing=false;
	m_pFilter=filter;
	m_pCallBack=NULL;
}

CChannelScan::~CChannelScan(void)
{
}

STDMETHODIMP CChannelScan::SetCallBack(IChannelScanCallback* callback)
{
	m_pCallBack=callback;
	return S_OK;
}

STDMETHODIMP CChannelScan::Start(TransportStreamStandard tsStandard)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    LogDebug("ChannelScan: start, TS standard = %d", tsStandard);
    m_tsStandard = tsStandard;
    m_bMpeg2PsipMerged = false;
    m_bReceivedOobSection = false;
    m_patParser.Reset(tsStandard == TransportStreamStandard_Default);

    if (tsStandard != TransportStreamStandard_Default)
    {
      m_atscParser.Reset();
      m_atscParser.SetCallBack(this);
      m_scteParser.Reset();
    }

    m_bIsParsing = true;
  }
  catch (...)
  {
    LogDebug("analyzer CChannelScan::Start exception");
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::Stop()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    LogDebug("ChannelScan: stop");
    m_bIsParsing = false;
    m_pCallBack = NULL;
    m_atscParser.SetCallBack(NULL);
  }
  catch (...)
  {
    LogDebug("analyzer CChannelScan::Stop exception");
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetCount(int* channelCount)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (m_tsStandard == TransportStreamStandard_Default)
    {
      *channelCount = m_patParser.Count();
      LogDebug("ChannelScan: channel count = %d", *channelCount);
      return S_OK;
    }
    if (m_bMpeg2PsipMerged || m_bReceivedOobSection)
    {
      *channelCount = m_scteParser.GetChannelCount();
      LogDebug("ChannelScan: channel count = %d", *channelCount);
      return S_OK;
    }

    // For ATSC over-the-air and cable scans with non-CableCARD tuners we have
    // to merge the services found by the VCT parsers with the services found
    // by the PAT parser.
    LogDebug("ChannelScan: merging PMT info with other SI info");
    int patServiceCount = m_patParser.Count();
    int tableId = 0xc8;
    if (m_tsStandard == TransportStreamStandard_Scte)
    {
      tableId = 0xc9;
    }
    for (int s1 = 0; s1 < patServiceCount; s1++)
    {
      CChannelInfo* patInfo = NULL;
      m_patParser.GetChannel(s1, &patInfo);

      CChannelInfo* vctInfo = NULL;
      int scteServiceCount = m_scteParser.GetChannelCount();
      for (int s2 = 0; s2 < scteServiceCount; s2++)
      {
        if (!m_scteParser.GetChannel(s2, &vctInfo))
        {
          vctInfo = NULL;
          break;
        }
        if (vctInfo->ServiceId == patInfo->ServiceId)
        {
          break;
        }
        vctInfo = NULL;
      }

      // Did we find the corresponding VCT record?
      if (vctInfo == NULL)
      {
        // Channel without VCT info. Fake an L-VCT callback to add the channel
        // to the SCTE parser.
        LogDebug("ChannelScan: adding PMT service 0x%x to VCT", patInfo->ServiceId);
        vector<unsigned int> languages;
        m_scteParser.OnLvctReceived(tableId, NULL, 0, 0, 0, 0, patInfo->TransportId, patInfo->ServiceId, 0, patInfo->hasCaDescriptor != 0, false, 0,
          false, false, 0, 0, patInfo->hasVideo, patInfo->hasAudio, languages);
      }
    }
    m_bMpeg2PsipMerged = true;

    *channelCount = m_scteParser.GetChannelCount();
    LogDebug("ChannelScan: channel count = %d", *channelCount);
  }
  catch (...)
  {
    LogDebug("analyzer CChannelScan::GetCount exception");
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::IsReady(BOOL* yesNo) 
{
  CEnterCriticalSection enter(m_section);
  try
  {
    *yesNo = m_bIsReady;
  }
  catch (...)
  {
    LogDebug("analyzer CChannelScan::IsReady exception");
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetChannel(int index,
									 long* networkId,
									 long* transportId,
									 long* serviceId,
									 int* majorChannel,
									 int* minorChannel,
									 int* frequency,
									 int* lcn,
									 int* freeCAMode,
									 int* serviceType,
									 int* modulation,
									 char** providerName,
									 char** serviceName,
									 int* pmtPid,
									 int* hasVideo,
									 int* hasAudio,
									 int* hasCaDescriptor)
{
	static char sServiceName[128];
	static char sProviderName[128];
	CEnterCriticalSection enter(m_section);
	try
	{
    CChannelInfo* info = NULL;
    bool gotInfo = false;
    if (m_tsStandard == TransportStreamStandard_Default)
    {
      gotInfo = m_patParser.GetChannel(index, &info);
    }
    else
    {
      gotInfo = m_scteParser.GetChannel(index, &info);
      LogDebug("%4d) %-25s TSID = 0x%04x, service ID = 0x%04x, source ID = 0x%04x, maj. ch. # = %-4d, min. ch. # = %-4d, access controlled = %d, type = %d, video stream count = %d, audio stream count = %d, frequency = %-6d kHz, modulation = %d",
        index, info->ServiceName, info->TransportId, info->ServiceId, info->NetworkId, info->MajorChannel, info->MinorChannel,
        info->FreeCAMode, info->ServiceType, info->hasVideo, info->hasVideo, info->Frequency, info->Modulation);
    }
    if (!gotInfo)
    {
      info = new CChannelInfo();
    }
    if (info == NULL)
    {
      LogDebug("ChannelScan: failed to retrieve channel info for index %d", index);
      return E_FAIL;
    }

		*networkId=info->NetworkId;
		*transportId=info->TransportId;
		*serviceId=info->ServiceId;
		*majorChannel=info->MajorChannel;
		*minorChannel=info->MinorChannel;
		*frequency=info->Frequency;
		*lcn=info->LCN;
		*freeCAMode=info->FreeCAMode;
		*serviceType=info->ServiceType;
		*modulation=info->Modulation;
		strcpy(sProviderName,info->ProviderName);
		strcpy(sServiceName,info->ServiceName);
		*providerName=sProviderName;
		*serviceName=sServiceName;
		*pmtPid=info->PidTable.PmtPid;
		*hasVideo=info->hasVideo;
		*hasAudio=info->hasAudio;
		*hasCaDescriptor=info->hasCaDescriptor;

    if (!gotInfo)
    {
      delete info;
    }
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::GetChannel exception");
	}
	return S_OK;
}

void CChannelScan::OnTsPacket(byte* tsPacket)
{
  CEnterCriticalSection enter(m_section);

  if (m_bIsParsing)
  {
    bool isReady = false;
    int pid = ((tsPacket[1] & 0x1f) << 8) + tsPacket[2];
    if (pid == PID_ATSC_BASE_PID)
    {
      m_atscParser.OnTsPacket(tsPacket);
      m_bIsReady = m_atscParser.IsReady() && m_patParser.IsReady();
    }
    else if (pid == PID_SCTE_BASE_PID)
    {
      m_scteParser.OnTsPacket(tsPacket);
      m_bIsReady = m_scteParser.IsReady() && m_patParser.IsReady();
    }
    else
    {
      m_patParser.OnTsPacket(tsPacket);
      m_bIsReady = m_patParser.IsReady() && (
        m_tsStandard == TransportStreamStandard_Default ||
        (m_tsStandard == TransportStreamStandard_Atsc && m_atscParser.IsReady()) ||
        (m_tsStandard == TransportStreamStandard_Scte && m_scteParser.IsReady())
      );
    }
    if (m_bIsReady && m_pCallBack != NULL)
    {
      m_pCallBack->OnScannerDone();
      m_pCallBack = NULL;
      m_bIsParsing = false;
    }
  }

  if (m_bIsParsingNIT)
    m_nit.OnTsPacket(tsPacket);
}

void CChannelScan::OnOobSiSection(CSection& section)
{
  CEnterCriticalSection enter(m_section);
  if (!m_bIsParsing || m_tsStandard != TransportStreamStandard_Scte)
  {
    return;
  }

  m_bReceivedOobSection = true;
  m_scteParser.OnNewSection(section);
  if (m_scteParser.IsReady() && m_pCallBack != NULL)
  {
    m_pCallBack->OnScannerDone();
    m_pCallBack = NULL;
    m_bIsParsing = false;
  }
}

void CChannelScan::OnLvctReceived(int tableId, char* name, int majorChannelNumber, int minorChannelNumber, int modulationMode, 
                                  unsigned int carrierFrequency, int channelTsid, int programNumber, int etmLocation,
                                  bool accessControlled, bool hidden, int pathSelect, bool outOfBand, bool hideGuide,
                                  int serviceType, int sourceId, int videoStreamCount, int audioStreamCount,
                                  vector<unsigned int>& languages)
{
  // Pass ATSC over-the-air handling to the SCTE cable parser. Avoids logic duplication.
  m_scteParser.OnLvctReceived(tableId, name, majorChannelNumber, minorChannelNumber, modulationMode, carrierFrequency,
    channelTsid, programNumber, etmLocation, accessControlled, hidden, pathSelect, outOfBand, hideGuide, serviceType,
    sourceId, videoStreamCount, audioStreamCount, languages);
}

STDMETHODIMP CChannelScan::ScanNIT()
{
  m_nit.Reset();
  m_bIsParsingNIT=true;
  return 0;
}

STDMETHODIMP CChannelScan::StopNIT()
{
  m_bIsParsingNIT=false;
  return 0;
}

STDMETHODIMP CChannelScan::GetNITCount(int* transponderCount)
{
  *transponderCount=0;
  if (m_nit.m_nit.satteliteNIT.size()>0) *transponderCount= m_nit.m_nit.satteliteNIT.size();
  else if (m_nit.m_nit.cableNIT.size()>0) *transponderCount= m_nit.m_nit.cableNIT.size();
  else if (m_nit.m_nit.terrestialNIT.size()>0) *transponderCount= m_nit.m_nit.terrestialNIT.size();
  return 0;
}

STDMETHODIMP CChannelScan::GetNITChannel(int channel,int* type,int* frequency,int *polarisation, int* modulation, int* symbolrate, int* bandwidth, int* fecInner, int* rollOff, char** networkName)
{
	static char sNetworkName[128];
	strcpy(sNetworkName,"");
  *frequency=0;
  *polarisation=0;
  *modulation=0;
  *symbolrate=0;
  *bandwidth=0;
  *fecInner=0;
  *type=-1;
  *rollOff = BDA_ROLL_OFF_NOT_SET;
	*networkName=sNetworkName;

  if (m_nit.m_nit.satteliteNIT.size()>0)
  {
    if (channel<0 || channel >=m_nit.m_nit.satteliteNIT.size()) return 0;
    NITSatDescriptor& des = m_nit.m_nit.satteliteNIT[channel];
    *frequency=des.Frequency;
    *polarisation=des.Polarisation;
    *modulation=des.Modulation;
    *symbolrate=des.Symbolrate;
    *fecInner=des.FECInner;
    *rollOff=des.RollOff;
    strcpy(sNetworkName,des.NetworkName.c_str());
	  *networkName=sNetworkName;
    *type=0;
    return 0;
  }
  if (m_nit.m_nit.cableNIT.size()>0)
  {
    if (channel<0 || channel >=m_nit.m_nit.cableNIT.size()) return 0;
    NITCableDescriptor& des = m_nit.m_nit.cableNIT[channel];
    *frequency=des.Frequency;
    *modulation=des.Modulation;
    *symbolrate=des.Symbolrate;
    *fecInner=des.FECInner;
    strcpy(sNetworkName,des.NetworkName.c_str());
	  *networkName=sNetworkName;
    *type=1;
    return 0;
  }
  if (m_nit.m_nit.terrestialNIT.size()>0)
  {
    if (channel<0 || channel >=m_nit.m_nit.terrestialNIT.size()) return 0;
    NITTerrestrialDescriptor& des = m_nit.m_nit.terrestialNIT[channel];
    *frequency=des.CentreFrequency;
    *bandwidth=des.Bandwidth;
    strcpy(sNetworkName,des.NetworkName.c_str());
	  *networkName=sNetworkName;
    *type=2;
    return 0;
  }
  return 0;
}
