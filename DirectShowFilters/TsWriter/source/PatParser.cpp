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
#include "..\..\shared\dvbutil.h"
#include "PatParser.h"


void LogDebug(const char *fmt, ...) ;

//*****************************************************************************
CPatParser::CPatParser(void)
{
	m_finished=false;
	m_waitForVCT=false;
	m_currentNetworkId=0;
  Reset(NULL,false);
  SetPid(PID_PAT);
}

//*****************************************************************************
CPatParser::~CPatParser(void)
{
  CleanUp();
}

//*****************************************************************************
void  CPatParser::CleanUp()
{
	itPmtParser it=m_pmtParsers.begin();
	while (it!=m_pmtParsers.end())
	{
		CPmtParser* parser=*it;
		delete parser;
		parser=NULL;
		++it;
	}
	m_pmtParsers.clear();
  m_mapChannels.clear();
  m_bDumped=false;
}

//*****************************************************************************
void  CPatParser::Reset(IChannelScanCallback* callback, bool waitForVCT)
{
//	Dump();
	CEnterCriticalSection enter(m_section);
	m_waitForVCT=waitForVCT;
	LogDebug("PatParser:Reset(%d)",m_pCallback);
	CSectionDecoder::Reset();
	SetPid(PID_PAT);
  CleanUp();
  m_vctParser.Reset();
  m_sdtParser.Reset();
	m_nitDecoder.Reset();
	m_currentNetworkId=0;

  m_sdtParser.SetCallback(this);
  m_vctParser.SetCallback(this);
	
	m_pCallback=callback;

  m_tickCount = GetTickCount();
	m_finished=false;
	LogDebug("PatParser::Reset done");
}

//*****************************************************************************
BOOL CPatParser::IsReady()
{
	if (m_nitDecoder.Ready()==false) 
	{
		//LogDebug("nit not ready");
		return FALSE;
	}
  
	int x=0;
	for (itChannels it=m_mapChannels.begin(); it !=m_mapChannels.end();++it)
  {
		CChannelInfo& info=it->second;
		if (!info.PmtReceived || !info.SdtReceived) 
		{
			//LogDebug("ch:%d pmt:%d sdt:%d othermux:%d %s onid:%x tsid:%x sid:%x",
			//	x,info.PmtReceived,info.SdtReceived,info.OtherMux,info.ServiceName,
			//	info.NetworkId,info.TransportId,info.ServiceId);
			return FALSE;
		}
		x++;
	}
	m_finished=true;
	return TRUE;
}

//*****************************************************************************
int CPatParser::Count()
{
  return (int)m_mapChannels.size();
}

//*****************************************************************************
bool CPatParser::GetChannel(int index, CChannelInfo& info)
{
	static CChannelInfo unknownChannel;
  if (index < 0 || index > Count()) 
	{
	  LogDebug("GetChannel:%d invalid", index);
		return false;
	}
  if (index==0) Dump();

  itChannels it=m_mapChannels.begin();
  while (index) 
  {
    it++;
    index--;
  }
  info = it->second;
	info.LCN=m_nitDecoder.GetLogicialChannelNumber(info.NetworkId,info.TransportId,info.ServiceId);

  /*if (info.NetworkId==0)
  {
      for (itChannels it2=m_mapChannels.begin();it2!=m_mapChannels.end();++it2)
      {
        if ( (it2->second).NetworkId!=0)
        {
          info.NetworkId=(it2->second).NetworkId;
          break;
        }
    }
  }*/
	info.NetworkId=m_currentNetworkId;
	if (info.NetworkId==0)
		LogDebug("Unexpected: NID is 0 for SID %d", info.ServiceId);

	return true;
}


//*****************************************************************************
void CPatParser::OnChannel(const CChannelInfo& info)
{
  LogDebug("onch: %s %x %x", info.ServiceName,info.PidTable.VideoPid,info.PidTable.AC3Pid);

	// check if we really have a channel with this sid
	itChannels it=m_mapChannels.find(info.ServiceId);
	if (it==m_mapChannels.end()) return;

	m_mapChannels[info.ServiceId].Frequency=info.Frequency;
	m_mapChannels[info.ServiceId].MajorChannel=info.MajorChannel;
	m_mapChannels[info.ServiceId].MinorChannel=info.MinorChannel;
	strcpy(m_mapChannels[info.ServiceId].ProviderName,info.ProviderName);
	strcpy(m_mapChannels[info.ServiceId].ServiceName,info.ServiceName);
	m_mapChannels[info.ServiceId].FreeCAMode=info.FreeCAMode;
	m_mapChannels[info.ServiceId].Modulation=info.Modulation;
	m_mapChannels[info.ServiceId].ServiceType=info.ServiceType;
	m_mapChannels[info.ServiceId].OtherMux=info.OtherMux;
	m_mapChannels[info.ServiceId].SdtReceived=true;
}

//*****************************************************************************
void CPatParser::OnSdtReceived(const CChannelInfo& sdtInfo)
{
	itChannels it=m_mapChannels.find(sdtInfo.ServiceId);

	// check if we really have a channel with this sid
  if (it==m_mapChannels.end()) return;

  CChannelInfo& info=it->second;

	// check if we already set the sdt for this channel
	if (info.SdtReceived) return;

	m_tickCount = GetTickCount();
	//info.NetworkId=sdtInfo.NetworkId;
	//info.TransportId=sdtInfo.TransportId;
	info.ServiceId=sdtInfo.ServiceId;
	info.FreeCAMode=sdtInfo.FreeCAMode;
	info.ServiceType=sdtInfo.ServiceType;
	info.OtherMux=sdtInfo.OtherMux;
	info.SdtReceived=true;
	strcpy(info.ProviderName,sdtInfo.ProviderName);
     
  int number=0;
  char compareBuffer[255];
  for (itChannels it2 = m_mapChannels.begin();it2!=m_mapChannels.end();++it2)
  {
		CChannelInfo& info2=it2->second;
    if (info2.ServiceId==sdtInfo.ServiceId) continue;
    if (number==0)
			strcpy(compareBuffer,sdtInfo.ServiceName);
    else
      sprintf(compareBuffer,"%s (%d)", sdtInfo.ServiceName,number);
    if (strcmp(info2.ServiceName,compareBuffer)==0) number++;
  }

  if (number==0)
		strcpy(info.ServiceName,sdtInfo.ServiceName);
  else
    sprintf(info.ServiceName,"%s (%d)", sdtInfo.ServiceName,number);
}

void CPatParser::OnPmtReceived2(int pid,int serviceId,int pcrPid,vector<PidInfo2> pidInfo)
{
  itChannels it=m_mapChannels.find(serviceId);
  if (it!=m_mapChannels.end())
  {
    CChannelInfo& info=it->second;
		if (!info.PmtReceived) 
		{
			AnalyzePidInfo(pidInfo,info.hasVideo,info.hasAudio);
			info.PmtReceived=true;
      m_tickCount = GetTickCount();
		}
	}
}

void CPatParser::AnalyzePidInfo(vector<PidInfo2> pidInfo,int &hasVideo, int &hasAudio)
{
	hasVideo=0;
	hasAudio=0;
	ivecPidInfo2 it=pidInfo.begin();
	while (it!=pidInfo.end())
	{
		PidInfo2 info=*it;
    //ITV HD workaround, this enables the channel to be scanned as a TV channel rather than a Radio channel
    if (info.streamType==SERVICE_TYPE_DVB_SUBTITLES2 && info.logicalStreamType==0xffffffff && info.elementaryPid==0xd49)
    {
      info.streamType=SERVICE_TYPE_VIDEO_H264;
      info.logicalStreamType=SERVICE_TYPE_VIDEO_H264;
      LogDebug("AnalyzePidInfo: set ITV HD video stream to H.264");
    }
    //end of workaround
		if (info.logicalStreamType==SERVICE_TYPE_VIDEO_MPEG1 || info.logicalStreamType==SERVICE_TYPE_VIDEO_MPEG2 || info.logicalStreamType==SERVICE_TYPE_VIDEO_MPEG4 || info.logicalStreamType==SERVICE_TYPE_VIDEO_H264)
			hasVideo=1;
    if (info.logicalStreamType==SERVICE_TYPE_AUDIO_MPEG1 || info.logicalStreamType==SERVICE_TYPE_AUDIO_MPEG2 || info.logicalStreamType==SERVICE_TYPE_AUDIO_AC3 || info.logicalStreamType==SERVICE_TYPE_AUDIO_AAC || info.logicalStreamType==SERVICE_TYPE_AUDIO_LATM_AAC)
			hasAudio=1;
		++it;
	}
}

bool CPatParser::PmtParserExists(int pid,int serviceId)
{
	itPmtParser it=m_pmtParsers.begin();
	while (it!=m_pmtParsers.end())
	{
		CPmtParser *parser=*it;
		int fpid; int sid;
		parser->GetFilter(fpid,sid);
		if (pid==fpid && serviceId==sid) 
			return true;
		++it;
	}
	return false;
}

//*****************************************************************************
void CPatParser::OnTsPacket(byte* tsPacket)
{
	CEnterCriticalSection enter(m_section);
	//if (m_finished) return;
  int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];

	if (m_pCallback!=NULL)
	{
		if (IsReady() )
    {
			LogDebug("Scanner finished. Triggering callback");
			m_pCallback->OnScannerDone();
      m_pCallback=NULL;
			return;
    }
	}	
  if (pid==PID_NIT) 
  {
    m_nitDecoder.OnTsPacket(tsPacket);
		m_currentNetworkId=m_nitDecoder.GetLastNetworkId();
    return;
  }
  if (pid==PID_VCT) 
  {
    m_vctParser.OnTsPacket(tsPacket);
    return;
  }
	
	if (pid==PID_SDT)
  {
    m_sdtParser.OnTsPacket(tsPacket);
    return;
  }
  if (pid==PID_PAT)
  {
		CSectionDecoder::OnTsPacket(tsPacket);
    return;
  }
	itPmtParser it=m_pmtParsers.begin();
	while (it!=m_pmtParsers.end())
	{
		CPmtParser *parser=*it;
		parser->OnTsPacket(tsPacket);
		++it;
	}
}

//*****************************************************************************
void CPatParser::OnNewSection(CSection& sections)
{
	CEnterCriticalSection enter(m_section);
	if (sections.table_id!=0) return;

  byte* section=sections.Data;
	int section_length=sections.section_length;

  int pmtcount=0;
  int loop =(section_length - 9) / 4;
  for(int i=0; i < loop; i++)
  {
	  int offset = (8 +(i * 4));
	  int serviceId=((section[offset] /*& 0x1F*/)<<8) + section[offset+1];
	  int pmtPid = ((section[offset+2] & 0x1F)<<8) + section[offset+3];
		
		//if (serviceId==0) pid = network pid
	  //else pid = program map pid

		//LogDebug("sid:%x pmt:%x", serviceId,pmtPid);
	  if (pmtPid < 0x10 || pmtPid >=0x1fff) 
	  {
      //invalid pmt pid
			LogDebug("invalid sid:%x pmt:%x", serviceId,pmtPid);
		  return ;
	  }

	  //some ATSC channels have transport_stream_id==0
		//if (pmtPid>0x12 && transport_stream_id>0 && serviceId>0)
	  if (pmtPid>0x12 && serviceId>0)
    {
			itChannels it =m_mapChannels.find(serviceId);
			if (it==m_mapChannels.end())
			{
				CChannelInfo info;
				info.TransportId=sections.table_id_extension;
				info.ServiceId=serviceId;
        info.PidTable.PmtPid=pmtPid;
				info.PmtReceived=false;
				info.SdtReceived=false;
				m_mapChannels[serviceId]=info;
			//	LogDebug("pat: tsid:%x sid:%x pmt:%x", transport_stream_id,serviceId,pmtPid);
			}
			if (!PmtParserExists(pmtPid,serviceId))
			{
				CPmtParser *parser=new CPmtParser();
				parser->SetFilter(pmtPid,serviceId);
				parser->SetPmtCallBack2(this); 
				m_pmtParsers.push_back(parser);
				LogDebug("PatParser: Added pmt parser for pmt: 0x%x sid: 0x%x",pmtPid,serviceId);
			}
			m_tickCount = GetTickCount();
    }
  }
}

//*****************************************************************************
void CPatParser::Dump()
{
  if (m_bDumped) return;
  m_bDumped=true;
  int i=0;
  itChannels it=m_mapChannels.begin();
  while (it!=m_mapChannels.end()) 
  {
    CChannelInfo& info=it->second;
		LogDebug("%4d)  p:%-15s s:%-25s  onid:%4x tsid:%4x sid:%4x major:%3d minor:%3x freq:%3x type:%3d pmt:%4x othermux:%d freeca:%d hasVideo:%d hasAudio:%d",i,
            info.ProviderName,info.ServiceName,info.NetworkId,info.TransportId,info.ServiceId,info.MajorChannel,info.MinorChannel,info.Frequency,
						info.ServiceType,info.PidTable.PmtPid,info.OtherMux,info.FreeCAMode,info.hasVideo,info.hasAudio);

    it++;
    i++;
  }
}

//*****************************************************************************

