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

#include "PatParser.h"

extern DWORD crc32 (char *data, int len);
void LogDebug(const char *fmt, ...) ;

//*****************************************************************************
CPatParser::CPatParser(void)
{
	//m_pConditionalAccess=NULL;
  Reset(NULL);
  SetTableId(0);
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
  itPmtParser it=m_mapPmtParsers.begin();
  while (it!=m_mapPmtParsers.end())
  {
    CPmtParser* parser=it->second;
    delete parser;
    it=m_mapPmtParsers.erase(it);
  }
  m_mapChannels.clear();
	m_mapPmtParsers.clear();
  m_bDumped=false;
}

//*****************************************************************************
void  CPatParser::Reset(IChannelScanCallback* callback)
{
	m_pCallback=callback;
//	Dump();
	LogDebug("PatParser:Reset(%d)",m_pCallback);
	CSectionDecoder::Reset();
  CleanUp();
  m_vctParser.Reset();
  m_sdtParser.Reset();
  m_sdtParserOther.Reset();
	m_nitDecoder.Reset();
	//UpdateHwPids();
  m_sdtParser.SetCallback(this);
  m_sdtParser.SetTableId(0x42);
  m_sdtParserOther.SetCallback(this);
  m_sdtParserOther.SetTableId(0x46);
  m_vctParser.SetCallback(this);
  m_tickCount = GetTickCount();
}

 
//*****************************************************************************
BOOL CPatParser::IsReady()
{
  DWORD timeSpan=GetTickCount()-m_tickCount;
  if (timeSpan < 1000) return FALSE;
	DWORD timeSpan1=GetTickCount()-m_tickCount;
	//a1jatt if channels not ready in 30 seconds , it will never be ready, so dump them 
	if (timeSpan1 > 25000) 
	{
		int t=0;
		for (itChannels it1=m_mapChannels.begin(); it1 !=m_mapChannels.end();it1++)
		{
			CChannelInfo& info1=it1->second;
			if (info1.PmtReceived == false || info1.SdtReceived == false) 
			{
					m_mapChannels.erase(t);
			}
				t++;
		}
		m_tickCount=GetTickCount();
		return FALSE;
	}

  if (m_vctParser.Count() > 0)
  {
    return TRUE;
  }
	if (m_nitDecoder.Ready()==false) 
	{
//		LogDebug("nit not ready");
		return FALSE;
	}
  if (m_mapPmtParsers.size()==false) 
	{
		//LogDebug("no pmt parsers ");
		return FALSE;
	}
  if (false==m_sdtParser.IsReady()) 
	{
		//LogDebug("sdt not ready");
		return FALSE;
	}
  
  for (itPmtParser it=m_mapPmtParsers.begin(); it != m_mapPmtParsers.end() ;++it)
  {
    CPmtParser* parser=it->second;
    if (false==parser->IsReady()) 
		{
		  //LogDebug("pmt not ready");
			return FALSE;
		}
  }
	int x=0;
	for (itChannels it=m_mapChannels.begin(); it !=m_mapChannels.end();++it)
  {
		CChannelInfo& info=it->second;
		if (info.PmtReceived == false || info.SdtReceived == false) 
		{
			//LogDebug("ch:%d pmt:%d sdt:%d othermux:%d %s onid:%x tsid:%x sid:%x",
			//	x,info.PmtReceived,info.SdtReceived,info.OtherMux,info.ServiceName,
			//	info.NetworkId,info.TransportId,info.ServiceId);
			return FALSE;
		}
		x++;
	}
  return TRUE;
}

//*****************************************************************************
int CPatParser::Count()
{
  if (m_vctParser.Count() > 0)
  {
    return (int)m_mapChannels.size();
  }
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

  if (info.NetworkId==0)
  {
      for (itChannels it2=m_mapChannels.begin();it2!=m_mapChannels.end();++it2)
      {
        if ( (it2->second).NetworkId!=0)
        {
          info.NetworkId=(it2->second).NetworkId;
          break;
        }
    }
  }

	return true;
}


//*****************************************************************************
void CPatParser::OnChannel(const CChannelInfo& info)
{
  LogDebug("onch: %s %x %x", info.ServiceName,info.PidTable.VideoPid,info.PidTable.AC3Pid);
  CChannelInfo i=info;
  m_mapChannels[info.ServiceId]=i;
}

//*****************************************************************************
void CPatParser::OnSdtReceived(const CChannelInfo& sdtInfo)
{
  if (m_vctParser.Count()!=0) return;
  
	itChannels it=m_mapChannels.find(sdtInfo.ServiceId);
  if (it!=m_mapChannels.end())
  {
    CChannelInfo& info=it->second;
		if (info.SdtReceived==false) 
		{
      m_tickCount = GetTickCount();
			info.NetworkId=sdtInfo.NetworkId;
			info.TransportId=sdtInfo.TransportId;
			info.ServiceId=sdtInfo.ServiceId;
			info.EIT_schedule_flag=sdtInfo.EIT_schedule_flag;
			info.EIT_present_following_flag=sdtInfo.EIT_present_following_flag;
			info.RunningStatus=sdtInfo.RunningStatus;
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
			if (m_pCallback!=NULL)
			{
				if (IsReady() )
        {
					m_pCallback->OnScannerDone();
          m_pCallback=NULL;
        }
			}
		}
	}
	/*
  else
  {
    if (sdtInfo.OtherMux==false)
    { 
      m_tickCount = GetTickCount();
      CChannelInfo info;
			info.NetworkId=sdtInfo.NetworkId;
			info.TransportId=sdtInfo.TransportId;
			info.ServiceId=sdtInfo.ServiceId;
			info.EIT_schedule_flag=sdtInfo.EIT_schedule_flag;
			info.EIT_present_following_flag=sdtInfo.EIT_present_following_flag;
			info.RunningStatus=sdtInfo.RunningStatus;
			info.FreeCAMode=sdtInfo.FreeCAMode;
			info.ServiceType=sdtInfo.ServiceType;
			info.OtherMux=sdtInfo.OtherMux;
			info.SdtReceived=true;
			info.PmtReceived=false;
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

      m_mapChannels[info.ServiceId]=info;
    }
  }*/
  return;
  
}

//*****************************************************************************
void CPatParser::OnPidsReceived(const CPidTable& pidTable)
{
  if (m_vctParser.Count()!=0) return;
  itChannels it=m_mapChannels.find(pidTable.ServiceId);
  if (it!=m_mapChannels.end())
  {
    CChannelInfo& info=it->second;
		if (info.PmtReceived==false) 
		{
      //LogDebug("1OnPidsRecv %x %x %x= %x %x %x", pidTable.VideoPid,pidTable.AudioPid1,pidTable.AC3Pid,info.PidTable.VideoPid,info.PidTable.AudioPid1,info.PidTable.AC3Pid);
      m_tickCount = GetTickCount();
			//LogDebug("PMT: onid:%x tsid:%x nit:%x p:%s s:%s other:%d", info.NetworkId,info.TransportId,info.ServiceId, info.ProviderName,info.ServiceName, info.OtherMux);
			info.PidTable=pidTable;
			info.PmtReceived=true;
      //LogDebug("2OnPidsRecv %x %x %x= %x %x %x", pidTable.VideoPid,pidTable.AudioPid1,pidTable.AC3Pid,info.PidTable.VideoPid,info.PidTable.AudioPid1,info.PidTable.AC3Pid);
			if (m_pCallback!=NULL)
			{
				if (IsReady() )
        {
					m_pCallback->OnScannerDone();
          m_pCallback=NULL;
        }
			}
		}
	}
  return;
  
}

//*****************************************************************************
void CPatParser::OnTsPacket(byte* tsPacket)
{
  int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];

  if (pid==PID_NIT) 
  {
    m_nitDecoder.OnTsPacket(tsPacket);
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
    m_sdtParserOther.OnTsPacket(tsPacket);
    return;
  }

  if (pid==PID_PAT)
  {
    CSectionDecoder::OnTsPacket(tsPacket);
    return;
  }

  for (itPmtParser it=m_mapPmtParsers.begin(); it != m_mapPmtParsers.end() ;++it)
  {
    CPmtParser* parser=it->second;
    if (pid==parser->GetPid())
    {
		  parser->OnTsPacket(tsPacket);
      return;
    }
  }
	if (m_pCallback!=NULL)
	{
		if (IsReady() )
    {
			m_pCallback->OnScannerDone();
      m_pCallback=NULL;
    }
	}
}

//*****************************************************************************
void CPatParser::OnNewSection(CSection& sections)
{
  byte* section=sections.Data;

  m_tsHeader.Decode(section);
  int start=m_tsHeader.PayLoadStart;
  int table_id = section[start];
  if (table_id!=0) return ;
  int section_syntax_indicator = (section[start+1]>>7) & 1;
  int section_length = ((section[start+1]& 0xF)<<8) + section[start+2];
  int transport_stream_id = (section[start+3]<<8)+section[start+4];
  int version_number = ((section[start+5]>>1)&0x1F);
  int current_next_indicator = section[start+5] & 1;
  int section_number = section[start+6];
  int last_section_number = section[start+7];

  
 // DWORD crc= crc32((char*)&section[start],sections.SectionLength+start+3-5);

  int pmtcount=0;
  int loop =(section_length - 9) / 4;
	bool newPmtsAdded=false;
  for(int i=0; i < loop; i++)
  {
	  int offset = (8 +(i * 4));
    int serviceId=((section[start+offset] /*& 0x1F*/)<<8) + section[start+offset+1];
	  int pmtPid = ((section[start+offset+2] & 0x1F)<<8) + section[start+offset+3];
		
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
				info.TransportId=transport_stream_id;
				info.ServiceId=serviceId;
        info.PidTable.PmtPid=pmtPid;
				m_mapChannels[serviceId]=info;
        m_tickCount = GetTickCount();
			//	LogDebug("pat: tsid:%x sid:%x pmt:%x", transport_stream_id,serviceId,pmtPid);
			}

			itPmtParser it2= m_mapPmtParsers.find(pmtPid);
			if (it2==m_mapPmtParsers.end())
			{
		    CPmtParser* pmtParser = new CPmtParser();
		    pmtParser->SetTableId(2);
		    pmtParser->SetPid(pmtPid);
			  pmtParser->SetPmtCallBack(this);
		    m_mapPmtParsers[pmtPid]=pmtParser ;
			  newPmtsAdded=true;
        m_tickCount = GetTickCount();
	    }
    }
  }
	if (newPmtsAdded)
	{
			//UpdateHwPids();
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
		LogDebug("%4d)  p:%-15s s:%-25s  onid:%4x tsid:%4x sid:%4x major:%3d minor:%3x freq:%3x type:%3d pcr:%4x pmt:%4x v:%4x a1:%4x a2:%4x a3:%4x ac3:%4x ttx:%4x sub1:%4x sub2:%4x sub3:%4x sub4:%4x othermux:%d freeca:%d",i,
            info.ProviderName,info.ServiceName,info.NetworkId,info.TransportId,info.ServiceId,info.MajorChannel,info.MinorChannel,info.Frequency,
						info.ServiceType,info.PidTable.PcrPid,info.PidTable.PmtPid,info.PidTable.VideoPid,info.PidTable.AudioPid1,info.PidTable.AudioPid2,
            info.PidTable.AudioPid3,info.PidTable.AC3Pid,info.PidTable.TeletextPid,info.PidTable.SubtitlePid1,info.PidTable.SubtitlePid2,
            info.PidTable.SubtitlePid3,info.PidTable.SubtitlePid4,info.OtherMux,info.FreeCAMode);

    it++;
    i++;
  }
}

//*****************************************************************************
void CPatParser::OnPmtReceived(int pid)
{
	//LogDebug("PatParser:  received pmt:%x", pid);
	//if ((m_mapPmtParsers.size()+5) <=16) return;
	//UpdateHwPids();
}

//*****************************************************************************
void CPatParser::UpdateHwPids()
{/*
	//if (m_pConditionalAccess==NULL) return;
	vector<int> pids;
	pids.push_back(0x0); //pat
	pids.push_back(0x10);//NIT
	pids.push_back(0x11);//sdt
	pids.push_back(0x1ffb);//atsc virtual channel table
	pids.push_back(0x1fff);//padding stream..
	for (int i=0; i < (int)m_mapPmtParsers.size();++i)
	{
		CPmtParser* parser = m_mapPmtParsers[i];
		if (parser->Ready() == false)
		{
			pids.push_back( parser->GetPid() );
		}
		if (pids.size()>=16) break;
	}
	char buf[1024];
	strcpy(buf,"");
	for (int i=0; i < (int)pids.size();++i)
	{
		char tmp[100];
		sprintf(tmp,"%x,", pids[i]);
		strcat(buf,tmp);
	}
	LogDebug("PatParser: filter pids:%s", buf);
	//m_pConditionalAccess->SetPids(pids);*/
}
