/* 
 *	Copyright (C) 2006-2010 Team MediaPortal
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
#include "PatParser.h"

void LogDebug(const char *fmt, ...) ;

//*****************************************************************************
CPatParser::CPatParser(void)
{
  SetPid(PID_PAT);
  Reset(true);
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
  itChannels it2=m_mapChannels.begin();
	while (it2!=m_mapChannels.end())
	{
		CChannelInfo* info=it2->second;
		delete info;
		info = NULL;
		++it2;
	}
  m_mapChannels.clear();
  m_bDumped=false;
}

//*****************************************************************************
void  CPatParser::Reset(bool isDvbScan)
{
//	Dump();
	CEnterCriticalSection enter(m_section);
	LogDebug("PatParser::Reset");
	m_bIsDvbScan = isDvbScan;
	CSectionDecoder::Reset();
  CleanUp();

  m_sdtParser.Reset();
	m_nitDecoder.Reset();

	m_sdtParser.SetCallback(this);
	m_bIsReady = false;
	LogDebug("PatParser::Reset done");
}

//*****************************************************************************
BOOL CPatParser::IsReady()
{
	if (m_bIsDvbScan && !m_nitDecoder.Ready())
	{
		//LogDebug("nit not ready");
		return FALSE;
	}

	return m_bIsReady;
}

//*****************************************************************************
int CPatParser::Count()
{
  return (int)m_mapChannels.size();
}

//*****************************************************************************
bool CPatParser::GetChannel(int index, CChannelInfo** info)
{
  if (index < 0 || index >= Count()) 
	{
	  LogDebug("GetChannel:%d invalid", index);
		return false;
	}
  if (index==0 && m_bIsDvbScan) Dump();

  itChannels it=m_mapChannels.begin();
  while (index) 
  {
    it++;
    index--;
  }
  CChannelInfo* toReturn = it->second;
	toReturn->LCN=m_nitDecoder.GetLogicialChannelNumber(toReturn->NetworkId,toReturn->TransportId,toReturn->ServiceId);

  if (toReturn->NetworkId==0)
  {
      for (itChannels it2=m_mapChannels.begin();it2!=m_mapChannels.end();++it2)
      {
        if ( (it2->second)->NetworkId!=0)
        {
          toReturn->NetworkId=(it2->second)->NetworkId;
          break;
        }
    }
  }

  *info = toReturn;
	return true;
}

//*****************************************************************************
void CPatParser::OnSdtReceived(const CChannelInfo& sdtInfo)
{
	itChannels it=m_mapChannels.find(sdtInfo.ServiceId);

	// check if we really have a channel with this sid
  if (it==m_mapChannels.end()) return;

  CChannelInfo* info=it->second;

	// check if we already set the sdt for this channel
	if (info->SdtReceived || (info->TransportId != sdtInfo.TransportId)) return;

	info->NetworkId=sdtInfo.NetworkId;
	info->TransportId=sdtInfo.TransportId;
	info->ServiceId=sdtInfo.ServiceId;
	info->FreeCAMode=sdtInfo.FreeCAMode;
	info->ServiceType=sdtInfo.ServiceType;
	info->OtherMux=sdtInfo.OtherMux;
	info->SdtReceived=true;
	strcpy(info->ProviderName,sdtInfo.ProviderName);
	strcpy(info->ServiceName,sdtInfo.ServiceName);

	int x=0;
	for (itChannels it=m_mapChannels.begin(); it !=m_mapChannels.end();++it)
	{
		CChannelInfo* info=it->second;
		if (!info->PmtReceived || (m_bIsDvbScan && !info->SdtReceived))
		{
			//LogDebug("ch:%d pmt:%d sdt:%d othermux:%d %s onid:%x tsid:%x sid:%x",
			//	x,info->PmtReceived,info->SdtReceived,info->OtherMux,info->ServiceName,
			//	info->NetworkId,info->TransportId,info->ServiceId);
			return;
		}
		x++;
	}
	m_bIsReady = true;
}

void CPatParser::OnPmtReceived2(int pid,int serviceId,int pcrPid,bool hasCaDescriptor,vector<PidInfo2> pidInfo)
{
  itChannels it=m_mapChannels.find(serviceId);
  if (it!=m_mapChannels.end())
  {
    CChannelInfo* info=it->second;
		if (!info->PmtReceived) 
		{
			AnalyzePidInfo(pidInfo,info->hasVideo,info->hasAudio);
			info->hasCaDescriptor = hasCaDescriptor ? 1 : 0;
			info->PmtReceived=true;

			int x=0;
			for (itChannels it=m_mapChannels.begin(); it !=m_mapChannels.end();++it)
			{
				CChannelInfo* info=it->second;
				if (!info->PmtReceived || (m_bIsDvbScan && !info->SdtReceived))
				{
					//LogDebug("ch:%d pmt:%d sdt:%d othermux:%d %s onid:%x tsid:%x sid:%x",
					//	x,info->PmtReceived,info->SdtReceived,info->OtherMux,info->ServiceName,
					//	info->NetworkId,info->TransportId,info->ServiceId);
					return;
				}
				x++;
			}
			m_bIsReady = true;
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
		if (info.logicalStreamType==SERVICE_TYPE_VIDEO_MPEG1 || info.logicalStreamType==SERVICE_TYPE_VIDEO_MPEG2 || info.logicalStreamType==SERVICE_TYPE_VIDEO_MPEG4 || info.logicalStreamType==SERVICE_TYPE_VIDEO_H264 || info.logicalStreamType==SERVICE_TYPE_VIDEO_MPEG2_DCII)
			hasVideo=1;
    if (info.logicalStreamType==SERVICE_TYPE_AUDIO_MPEG1 || info.logicalStreamType==SERVICE_TYPE_AUDIO_MPEG2 || info.logicalStreamType==SERVICE_TYPE_AUDIO_AC3 || info.logicalStreamType==SERVICE_TYPE_AUDIO_E_AC3 || info.logicalStreamType==SERVICE_TYPE_AUDIO_AAC || info.logicalStreamType==SERVICE_TYPE_AUDIO_LATM_AAC)
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
  int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
  if (pid==PID_PAT)
  {
		CSectionDecoder::OnTsPacket(tsPacket);
    return;
  }
  if (pid==PID_NIT) 
  {
    m_nitDecoder.OnTsPacket(tsPacket);
    return;
  }
	if (pid==PID_SDT)
  {
    m_sdtParser.OnTsPacket(tsPacket);
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
				CChannelInfo* info = new CChannelInfo();
				info->TransportId=sections.table_id_extension;
				info->ServiceId=serviceId;
        info->PidTable.PmtPid=pmtPid;
				info->PmtReceived=false;
				info->SdtReceived=false;
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
    }
  }
}

//*****************************************************************************

int CPatParser::PATRequest(CSection& sections, int SID)
{
  CEnterCriticalSection enter(m_section);
  if (sections.table_id!=0) 
  {
    return -1;
  }
  byte* section=sections.Data;
  int section_length=sections.section_length;

  int loop =(section_length - 9) / 4;
  for(int i=0; i < loop; i++)
  {
    int offset = (8 +(i * 4));
    int serviceId=((section[offset] /*& 0x1F*/)<<8) + section[offset+1];
    int pmtPid = ((section[offset+2] & 0x1F)<<8) + section[offset+3];
    if(serviceId==SID)
    {
      if (pmtPid < 0x10 || pmtPid >=0x1fff)
      {
        //invalid pmt pid
        LogDebug("invalid sid:%x pmt:%x", serviceId,pmtPid);
        return -1;
      }
      // pmtPid is good so return it.
      LogDebug("Found PMT Pid %x for ServiceId - %x",pmtPid,SID);
      return pmtPid;
    }
  }
  // SID wasn't found in the PAT so the channel has moved.
  return -1;
}

void CPatParser::Dump()
{
  if (m_bDumped) return;
  m_bDumped=true;
  int i=0;
  itChannels it=m_mapChannels.begin();
  while (it!=m_mapChannels.end()) 
  {
    CChannelInfo* info=it->second;
		LogDebug("%4d)  p:%-15s s:%-25s  onid:%4x tsid:%4x sid:%4x type:%3d pmt:%4x othermux:%d freeca:%d hasVideo:%d hasAudio:%d hasCaDescriptor:%d",i,
            info->ProviderName,info->ServiceName,info->NetworkId,info->TransportId,info->ServiceId,
            info->ServiceType,info->PidTable.PmtPid,info->OtherMux,info->FreeCAMode,info->hasVideo,info->hasAudio,info->hasCaDescriptor);

    it++;
    i++;
  }
}

//*****************************************************************************

