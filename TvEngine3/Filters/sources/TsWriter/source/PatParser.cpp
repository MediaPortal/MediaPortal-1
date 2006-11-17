/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#include "tsheader.h"

extern DWORD crc32 (char *data, int len);
void LogDebug(const char *fmt, ...) ;
CPatParser::CPatParser(void)
{
	//m_pConditionalAccess=NULL;
  Reset();
  SetTableId(0);
  SetPid(0);
}

CPatParser::~CPatParser(void)
{
  CleanUp();
}

void  CPatParser::CleanUp()
{
  for (int i=0; i < (int)m_pmtParsers.size();++i)
  {
    CPmtParser* parser=m_pmtParsers[i];
    delete parser;
  }
  m_pmtParsers.clear();
  m_vecChannels.clear();
}

void  CPatParser::Reset()
{
//	Dump();
	LogDebug("PatParser:Reset()");
	CSectionDecoder::Reset();
  CleanUp();
  m_vctParser.Reset();
  m_sdtParser.Reset();
  m_sdtParserOther.Reset();
	m_nitDecoder.Reset();
	//UpdateHwPids();
  m_sdtParser.SetCallback(this);
  m_sdtParserOther.SetCallback(this);
  m_sdtParserOther.SetTableId(0x46);
  m_vctParser.SetCallback(this);
}

 
BOOL CPatParser::IsReady()
{
 if (m_vctParser.Count() > 0)
  {
    return TRUE;
  }
	if (m_nitDecoder.Ready()==false) return FALSE;
  if (m_pmtParsers.size()==false) return FALSE;
  for (int i=0; i < (int)m_pmtParsers.size();++i)
  {
    CPmtParser* parser=m_pmtParsers[i];
    if (false==parser->IsReady()) return FALSE;
  }
  if (false==m_sdtParser.IsReady()) return FALSE;
  return TRUE;
}

int CPatParser::Count()
{
  if (m_vctParser.Count() > 0)
  {
    return m_vecChannels.size();
  }
  return m_vecChannels.size();
}

bool CPatParser::GetChannel(int index, CChannelInfo& info)
{
	static CChannelInfo unknownChannel;
  if (index < 0 || index > Count()) 
	{
		return false;
	}
  info = m_vecChannels[index];
	info.LCN=m_nitDecoder.GetLogicialChannelNumber(info.NetworkId,info.TransportId,info.ServiceId);
	return true;
}


void CPatParser::OnChannel(CChannelInfo info)
{
  m_vecChannels.push_back(info);
}

void CPatParser::OnSdtReceived(CChannelInfo sdtInfo)
{
  for (int i=0; i < m_vecChannels.size(); ++i)
  {
    CChannelInfo& info=m_vecChannels[i];
    if (info.ServiceId==sdtInfo.ServiceId)
    {
      info.NetworkId=sdtInfo.NetworkId;
      info.TransportId=sdtInfo.TransportId;
      info.ServiceId=sdtInfo.ServiceId;
      info.EIT_schedule_flag=sdtInfo.EIT_schedule_flag;
      info.EIT_present_following_flag=sdtInfo.EIT_present_following_flag;
      info.RunningStatus=sdtInfo.RunningStatus;
      info.FreeCAMode=sdtInfo.FreeCAMode;
      info.ServiceType=sdtInfo.ServiceType;
      strcpy(info.ProviderName,sdtInfo.ProviderName);
      strcpy(info.ServiceName,sdtInfo.ServiceName);
      return;
    }
  }
  CChannelInfo info;
  info.NetworkId=sdtInfo.NetworkId;
  info.TransportId=sdtInfo.TransportId;
  info.ServiceId=sdtInfo.ServiceId;
  info.EIT_schedule_flag=sdtInfo.EIT_schedule_flag;
  info.EIT_present_following_flag=sdtInfo.EIT_present_following_flag;
  info.RunningStatus=sdtInfo.RunningStatus;
  info.FreeCAMode=sdtInfo.FreeCAMode;
  info.ServiceType=sdtInfo.ServiceType;
  strcpy(info.ProviderName,sdtInfo.ProviderName);
  strcpy(info.ServiceName,sdtInfo.ServiceName);
  m_vecChannels.push_back(info);
}
void CPatParser::OnPidsReceived(CPidTable pidTable)
{
  for (int i=0; i < m_vecChannels.size(); ++i)
  {
    CChannelInfo& info=m_vecChannels[i];
    if (info.ServiceId==pidTable.ServiceId)
    {
      info.PidTable=pidTable;
      return;
    }
  }
  CChannelInfo info;
  info.ServiceId=pidTable.ServiceId;
  info.PidTable=pidTable;
  m_vecChannels.push_back(info);
}

void CPatParser::OnTsPacket(byte* tsPacket)
{
	m_nitDecoder.OnTsPacket(tsPacket);
  m_vctParser.OnTsPacket(tsPacket);
  m_sdtParser.OnTsPacket(tsPacket);
  m_sdtParserOther.OnTsPacket(tsPacket);
  for (int i=0; i < (int)m_pmtParsers.size();++i)
  {
    CPmtParser* parser=m_pmtParsers[i];
    parser->OnTsPacket(tsPacket);
  }
  CSectionDecoder::OnTsPacket(tsPacket);
	
}

void CPatParser::OnNewSection(CSection& sections)
{
  byte* section=sections.Data;

  CTsHeader header(section);
  int start=header.PayLoadStart;
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
    int serviceId=((section[start+offset] & 0x1F)<<8) + section[start+offset+1];
	  int pmtPid = ((section[start+offset+2] & 0x1F)<<8) + section[start+offset+3];
	  if (pmtPid < 0x10 || pmtPid >=0x1fff) 
	  {
      //invalid pmt pid
		  return ;
	  }

    bool found=false;
    for (int idx=0; idx < (int)m_vecChannels.size(); idx++)
	  {
      CChannelInfo& info = m_vecChannels[idx];
      if (info.TransportId==transport_stream_id && info.ServiceId==serviceId) 
      {
        found=true;
        break;
      }
    }
    if (!found)
    {
      CChannelInfo info;
      info.TransportId=transport_stream_id;
      info.ServiceId=serviceId;
      m_vecChannels.push_back(info);
    }
	   found=false;
	  for (int idx=0; idx < (int)m_pmtParsers.size(); idx++)
	  {
		  CPmtParser* pmtParser = m_pmtParsers[idx];
		  if (pmtParser->GetPid() == pmtPid)
		  {
			  found=true;
			  break;
		  }
	  }
	  if (!found && pmtPid>0x12)
	  {
		  CPmtParser* pmtParser = new CPmtParser();
		  pmtParser->SetTableId(2);
		  pmtParser->SetPid(pmtPid);
			pmtParser->SetPmtCallBack(this);
		  m_pmtParsers.push_back( pmtParser );
			LogDebug("  add pmt# %d pid: %x",m_pmtParsers.size(), pmtPid);
			newPmtsAdded=true;
	  }
  }
	if (newPmtsAdded)
	{
			//UpdateHwPids();
	}
}

void CPatParser::Dump()
{
  for (int i=0; i < Count();++i)
  {
    CChannelInfo info;
    if (GetChannel( i, info))
    {
      LogDebug("%4d)  p:%-15s s:%-25s  onid:%4x tsid:%4x sid:%4x major:%3d minor:%3x freq:%3x type:%3d pcr:%4x pmt:%4x v:%4x a1:%4x a2:%4x a3:%4x ac3:%4x ttx:%4x sub:%4x",i,
            info.ProviderName,info.ServiceName,info.NetworkId,info.TransportId,info.ServiceId,info.MajorChannel,info.MinorChannel,info.Frequency,
            info.ServiceType,info.PidTable.PcrPid,info.PidTable.PmtPid,info.PidTable.VideoPid,info.PidTable.AudioPid1,info.PidTable.AudioPid2,info.PidTable.AudioPid3,info.PidTable.AC3Pid,info.PidTable.TeletextPid,info.PidTable.SubtitlePid);
    }
    else
    {
      LogDebug("%d) not found",i);
    }
  }
}

void CPatParser::OnPmtReceived(int pid)
{
//	LogDebug("PatParser:  received pmt:%x", pid);
	//if ((m_pmtParsers.size()+5) <=16) return;
	//UpdateHwPids();
}
void CPatParser::UpdateHwPids()
{/*
	//if (m_pConditionalAccess==NULL) return;
	vector<int> pids;
	pids.push_back(0x0); //pat
	pids.push_back(0x10);//NIT
	pids.push_back(0x11);//sdt
	pids.push_back(0x1ffb);//atsc virtual channel table
	pids.push_back(0x1fff);//padding stream..
	for (int i=0; i < (int)m_pmtParsers.size();++i)
	{
		CPmtParser* parser = m_pmtParsers[i];
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