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
#include <windows.h>
#include <time.h>
#include "ChannelLinkageParser.h"
#include "..\..\shared\dvbutil.h"
#pragma warning(disable : 4995)

extern void LogDebug(const char *fmt, ...) ;

CChannelLinkageParser::CChannelLinkageParser(void)
{
	CEnterCriticalSection enter(m_section);
	m_bScanning=false;
	m_bScanningDone=false;
	sectionDecoder=new CSectionDecoder();
	sectionDecoder->SetPid(PID_EPG);
	sectionDecoder->SetCallBack(this);
}

CChannelLinkageParser::~CChannelLinkageParser(void)
{
	CEnterCriticalSection enter(m_section);
	delete sectionDecoder;
}

void CChannelLinkageParser::Start()
{
	CEnterCriticalSection enter(m_section);
	m_mapChannels.clear();
	m_bScanning=true;
	m_scanTimeout=time(NULL);
	m_prevChannelIndex=-1;
}

void CChannelLinkageParser::Reset()
{
	CEnterCriticalSection enter(m_section);
	m_bScanning=false;
	m_bScanningDone=false;
	sectionDecoder->Reset();
	m_mapChannels.clear();
}

bool CChannelLinkageParser::IsScanningDone()
{
	return m_bScanningDone;
}

bool CChannelLinkageParser::GetChannelByindex(ULONG channelIndex, PortalChannel& portalChannel)
{
	CEnterCriticalSection lock (m_section);
	LinkedChannel lChannel;
	m_prevLinkIndex=-1;
	m_prevLink=lChannel;

	if (channelIndex>=m_mapChannels.size()) 
		return false;
	ULONG count=0;
	imapChannels it =m_mapChannels.begin();
	while (count < channelIndex) 
	{ 
		it++; 
		count++;
	}
	portalChannel=it->second;
	m_prevChannel=portalChannel;
	m_prevChannelIndex=channelIndex;

	return true;
}

ULONG CChannelLinkageParser::GetChannelCount()
{
	CEnterCriticalSection lock (m_section);
	return (ULONG)m_mapChannels.size();
}

void CChannelLinkageParser::GetChannel (ULONG channelIndex, WORD* network_id, WORD* transport_id,WORD* service_id  )
{
	CEnterCriticalSection lock (m_section);
	*network_id=0;
	*transport_id=0;
	*service_id=0;
	if (channelIndex!=m_prevChannelIndex)
	{
		PortalChannel pChannel;
		if (!GetChannelByindex(channelIndex,pChannel)) return;
		*network_id=pChannel.original_network_id;
		*transport_id=pChannel.transport_id;
		*service_id=pChannel.service_id;
	}
	else
	{
		*network_id=m_prevChannel.original_network_id;
		*transport_id=m_prevChannel.transport_id;
		*service_id=m_prevChannel.service_id;
	}
}

ULONG CChannelLinkageParser::GetLinkedChannelsCount (ULONG channel)
{
	CEnterCriticalSection lock (m_section);
	PortalChannel pChannel;
	if (!GetChannelByindex(channel,pChannel)) return 0;
	return (ULONG)pChannel.m_linkedChannels.size();
}

void CChannelLinkageParser::GetLinkedChannel (ULONG channelIndex,ULONG linkIndex, WORD* network_id, WORD* transport_id,WORD* service_id, char** channelName  )
{
	CEnterCriticalSection enter(m_section);
	PortalChannel pChannel;
	*network_id=0;
	*transport_id=0;
	*service_id=0;
	*channelName=(char*)"";
	if (channelIndex!=m_prevChannelIndex)
	{
		PortalChannel pChannel;
		if (!GetChannelByindex(channelIndex,pChannel)) return ;
	}
	if (linkIndex >= m_prevChannel.m_linkedChannels.size()) return;

	ULONG count=0;
	PortalChannel::ilinkedChannels itLink=m_prevChannel.m_linkedChannels.begin();
	while (count < linkIndex) 
	{ 
		itLink++; 
		count++;
	}
	LinkedChannel& lChannel=*itLink;
	m_prevLinkIndex=linkIndex;
	m_prevLink=lChannel;

	*network_id=lChannel.network_id;
	*transport_id=lChannel.transport_id;
	*service_id=lChannel.service_id;
	*channelName=(char*)lChannel.name.c_str();
}

void CChannelLinkageParser::OnTsPacket(CTsHeader& header, byte* tsPacket)
{
	if (m_bScanning==false) return;

	CEnterCriticalSection enter(m_section);
	sectionDecoder->OnTsPacket(header,tsPacket);
}

void CChannelLinkageParser::OnNewSection(int pid, int tableId, CSection& section)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		if (section.table_id>=0x4e && section.table_id<0x70)
			DecodeLinkage(section.Data,section.section_length);
	}
	catch(...)
	{
		LogDebug("exception in CChannelLinkageParser::OnNewSection");
	}
}

void CChannelLinkageParser::DecodeLinkage(byte* buf, int len)
{
	CEnterCriticalSection lock (m_section);
	try
	{
		if (!m_bScanning) 
			return;
		if (buf==NULL)
			return;
		time_t currentTime=time(NULL);
		time_t timespan=currentTime-m_scanTimeout;
		if (timespan>8)
		{
			m_bScanning=false;
			m_bScanningDone=true;
			return;
		}
    	if (len<=14) 
			return;
		int tableid = buf[0];
		int service_id = (buf[3]<<8)+buf[4];
		int transport_id=(buf[8]<<8)+buf[9];
		int network_id=(buf[10]<<8)+buf[11];

		unsigned long lNetworkId=network_id;
		unsigned long lTransport_id=transport_id;
		unsigned long lServiceId=service_id;
		unsigned long key=(unsigned long)(lNetworkId<<32UL);
		key+=(lTransport_id<<16);
		key+=lServiceId;
		imapChannels it=m_mapChannels.find(key);
		if (it==m_mapChannels.end())
		{
			PortalChannel newChannel ;
			newChannel.original_network_id=network_id;
			newChannel.service_id=service_id;
			newChannel.transport_id=transport_id;
			newChannel.allSectionsReceived=false;
			m_mapChannels[key]=newChannel;
			it=m_mapChannels.find(key);
		}
		if (it==m_mapChannels.end()) 
			return;
		PortalChannel& channel=it->second; 

		//did we already receive this section ?
		key=crc32 ((char*)buf,len);
		PortalChannel::imapSectionsReceived itSec=channel.mapSectionsReceived.find(key);
		if (itSec!=channel.mapSectionsReceived.end())
			return; //yes
		channel.mapSectionsReceived[key]=true;
		m_scanTimeout=time(NULL);
		int start=14;
		while (start+11 <= len+1)
		{
			int descriptors_len=((buf[start+10]&0xf)<<8) + buf[start+11];
			start=start+12;
			int off=0;
			while (off < descriptors_len)
			{
				if (start+off+1>len) 
					return;
				int descriptor_tag = buf[start+off];
				int descriptor_len = buf[start+off+1];
				if (start+off+descriptor_len+2>len) 
					return;
				if ((descriptor_len>0) && (descriptor_tag==0x4a)) // Linkage descriptor
				{
					LinkedChannel lChannel;
					lChannel.transport_id=(buf[start+off+2]<<8)+buf[start+off+3];
					lChannel.network_id=(buf[start+off+4]<<8)+buf[start+off+5];
					lChannel.service_id=(buf[start+off+6]<<8)+buf[start+off+7];
					char *cname=(char*)malloc(400);
					strncpy(cname,(char*)&buf[start+off+9],descriptor_len-7);
					cname[descriptor_len-7]=0;
					lChannel.name.assign(cname);
					free(cname);
					channel.m_linkedChannels.push_back(lChannel);
					//LogDebug("ChannelLinkageScanner: PortalChannel tsid=%d nid=%d sid=%d",channel.transport_id,channel.original_network_id,channel.service_id);
					//LogDebug("ChannelLinkageScanner: LinkedChannel found len=%d tsid=%d nid=%d sid=%d %s",descriptor_len,lChannel.transport_id,lChannel.network_id,lChannel.service_id,lChannel.name.c_str());
				}
				off   +=descriptor_len+2;
			}
			start +=descriptors_len;
		}

	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodeChannelLinkage()");
	}	
	return;
}
