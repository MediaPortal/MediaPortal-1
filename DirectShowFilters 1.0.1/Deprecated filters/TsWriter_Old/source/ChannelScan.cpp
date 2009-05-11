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
#include "channelinfo.h"
#include "tswriter.h"

extern void LogDebug(const char *fmt, ...) ;

CChannelScan::CChannelScan(LPUNKNOWN pUnk, HRESULT *phr, CMpTsFilter* filter) 
:CUnknown( NAME ("MpTsChannelScan"), pUnk)
{
  m_bIsParsingNIT=false;
	m_bIsParsing=false;
	m_pFilter=filter;
	m_pConditionalAccess=NULL;
	m_pCallback=NULL;
}
CChannelScan::~CChannelScan(void)
{
}

STDMETHODIMP CChannelScan::SetCallBack(IChannelScanCallback* callback)
{
	m_pCallback=callback;
	return S_OK;
}
STDMETHODIMP CChannelScan::Start()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		if (m_pConditionalAccess!=NULL)
		{
			delete m_pConditionalAccess;
			m_pConditionalAccess=NULL;
		}
	//	m_pConditionalAccess = new CConditionalAccess(m_pFilter->GetFilterGraph());
		//m_patParser.SetConditionalAccess(m_pConditionalAccess);
		m_patParser.Reset(m_pCallback);
		m_bIsParsing=true;
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::Start exception");
	}
	return S_OK;
}
STDMETHODIMP CChannelScan::Stop()
{
	m_bIsParsing=false;
	CEnterCriticalSection enter(m_section);
	try
	{
		m_pCallback=NULL;
		m_patParser.Reset(NULL);
		//m_patParser.SetConditionalAccess(NULL);
		//if (m_pConditionalAccess!=NULL)
		//{
		//	delete m_pConditionalAccess; 
		//}
		m_pConditionalAccess=NULL;
	}
	catch(...)
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
		*channelCount=m_patParser.Count();
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::GetCount exception");
	}
	return S_OK;
}

STDMETHODIMP CChannelScan::IsReady( BOOL* yesNo) 
{
	CEnterCriticalSection enter(m_section);
	try
	{
		*yesNo=m_patParser.IsReady();
		if (*yesNo)
		{
			m_bIsParsing=false;
		}
	}
	catch(...)
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
									 int* EIT_schedule_flag,
									 int* EIT_present_following_flag,
									 int* runningStatus,
									 int* freeCAMode,
									 int* serviceType,
									 int* modulation,
									 char** providerName,
									 char** serviceName,
									 int* pcrPid,
									 int* pmtPid,
									 int* videoPid,
									 int* audio1Pid,
									 int* audio2Pid,
									 int* audio3Pid,
									 int* audio4Pid,
									 int* audio5Pid,
									 int* ac3Pid,
									 char** audioLanguage1,
									 char** audioLanguage2,
									 char** audioLanguage3,
									 char** audioLanguage4,
									 char** audioLanguage5,
									 int* teletextPid,
									 int* subtitlePid1,
                   int* subtitlePid2,
                   int* subtitlePid3,
                   int* subtitlePid4,
									 char** subLanguage1,
                   char** subLanguage2,
                   char** subLanguage3,
                   char** subLanguage4,
									 int* videoStreamType)
{
	static char sServiceName[128];
	static char sProviderName[128];
	static char sAudioLang1[10];
	static char sAudioLang2[10];
	static char sAudioLang3[10];
	static char sAudioLang4[10];
	static char sAudioLang5[10];
	static char ssubLanguage1[10];
  static char ssubLanguage2[10];
  static char ssubLanguage3[10];
  static char ssubLanguage4[10];
	CEnterCriticalSection enter(m_section);
	try
	{
		strcpy(sServiceName,"");
		strcpy(sProviderName,"");
		strcpy(sAudioLang1,"");
		strcpy(sAudioLang2,"");
		strcpy(sAudioLang3,"");
		strcpy(sAudioLang4,"");
		strcpy(sAudioLang5,"");
    strcpy(ssubLanguage1,"");
    strcpy(ssubLanguage2,"");
    strcpy(ssubLanguage3,"");
    strcpy(ssubLanguage4,"");
		*networkId=0;
		*transportId=0;
		*serviceId=0;
		*pmtPid=0;
		*lcn=10000;
		*videoStreamType=0;

		CChannelInfo info;
		info.Reset();
		if ( m_patParser.GetChannel(index, info))
		{
			sAudioLang1[0]=info.PidTable.Lang1_1;
			sAudioLang1[1]=info.PidTable.Lang1_2;
			sAudioLang1[2]=info.PidTable.Lang1_3;
			sAudioLang1[3]=0;

			sAudioLang2[0]=info.PidTable.Lang2_1;
			sAudioLang2[1]=info.PidTable.Lang2_2;
			sAudioLang2[2]=info.PidTable.Lang2_3;
			sAudioLang2[3]=0;
			
			sAudioLang3[0]=info.PidTable.Lang3_1;
			sAudioLang3[1]=info.PidTable.Lang3_2;
			sAudioLang3[2]=info.PidTable.Lang3_3;
			sAudioLang3[3]=0;
			
			sAudioLang4[0]=info.PidTable.Lang4_1;
			sAudioLang4[1]=info.PidTable.Lang4_2;
			sAudioLang4[2]=info.PidTable.Lang4_3;
			sAudioLang4[3]=0;

			sAudioLang5[0]=info.PidTable.Lang5_1;
			sAudioLang5[1]=info.PidTable.Lang5_2;
			sAudioLang5[2]=info.PidTable.Lang5_3;
			sAudioLang5[3]=0;

      ssubLanguage1[0]=info.PidTable.SubLang1_1;
      ssubLanguage1[1]=info.PidTable.SubLang1_2;
      ssubLanguage1[2]=info.PidTable.SubLang1_3;
      ssubLanguage1[3]=0;
			*lcn=info.LCN;
			*networkId=info.NetworkId;
			*transportId=info.TransportId;
			*serviceId=info.ServiceId;
			*majorChannel=info.MajorChannel;
			*minorChannel=info.MinorChannel;
			*EIT_schedule_flag=info.EIT_schedule_flag;
			*EIT_present_following_flag=info.EIT_present_following_flag;
			*runningStatus=info.RunningStatus;
			*freeCAMode=info.FreeCAMode;
			*serviceType=info.ServiceType;
			*modulation=info.Modulation;
			strcpy(sProviderName,info.ProviderName);
			strcpy(sServiceName,info.ServiceName);
			*providerName=sProviderName;
			*serviceName=sServiceName;
			*pcrPid=info.PidTable.PcrPid;
			*pmtPid=info.PidTable.PmtPid;
			*videoPid=info.PidTable.VideoPid;
			*audio1Pid=info.PidTable.AudioPid1;
			*audio2Pid=info.PidTable.AudioPid2;
			*audio3Pid=info.PidTable.AudioPid3;
			*audio4Pid=info.PidTable.AudioPid4;
			*audio5Pid=info.PidTable.AudioPid5;
			*ac3Pid=info.PidTable.AC3Pid;

			*audioLanguage1=sAudioLang1;
			*audioLanguage2=sAudioLang2;
			*audioLanguage3=sAudioLang3;
			*teletextPid=info.PidTable.TeletextPid;
			*subtitlePid1=info.PidTable.SubtitlePid1;
      *subtitlePid2=info.PidTable.SubtitlePid2;
      *subtitlePid3=info.PidTable.SubtitlePid3;
      *subtitlePid4=info.PidTable.SubtitlePid4;
      *subLanguage1=ssubLanguage1;
      *subLanguage2=ssubLanguage2;
      *subLanguage3=ssubLanguage3;
      *subLanguage4=ssubLanguage4;
			*videoStreamType=info.PidTable.videoServiceType;
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
	if (m_bIsParsing)
	{
	  CEnterCriticalSection enter(m_section);
		m_patParser.OnTsPacket(tsPacket);
	}
  if (m_bIsParsingNIT)
  {
    m_nit.OnTsPacket(tsPacket);
  }
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

STDMETHODIMP CChannelScan::GetNITChannel(int channel,int* type,int* frequency,int *polarisation, int* modulation, int* symbolrate, int* bandwidth, int* fecInner, char** networkName)
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
  if (m_nit.m_nit.cableNIT.size()>0)
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
