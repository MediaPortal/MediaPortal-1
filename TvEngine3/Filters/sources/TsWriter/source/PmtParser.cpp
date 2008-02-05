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
#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <windows.h>
#include "PmtParser.h"
#include "channelinfo.h"

void LogDebug(const char *fmt, ...) ; 

CPmtParser::CPmtParser()
{
	m_pmtCallback=NULL;
	_isFound=false;
  m_pmtVersion = -1;
	m_serviceId=-1;
	EnableCrcCheck(false);
}

CPmtParser::~CPmtParser(void)
{
}

void CPmtParser::SetFilter(int pid,int serviceId)
{
	SetPid(pid);
	m_serviceId=serviceId;
}

void CPmtParser::SetPmtCallBack(IPmtCallBack* callback)
{
	m_pmtCallback=callback;
}

void CPmtParser::SetPmtCallBack2(IPmtCallBack2* callback)
{
	m_pmtCallback2=callback;
}

bool CPmtParser::IsReady()
{
	return _isFound;
}
void CPmtParser::OnNewSection(CSection& sections)
{ 
	byte* section=sections.Data;
	int sectionLen=sections.section_length;

	int start=0;
	int table_id = sections.table_id;
	if (table_id!=2) return;
	if (m_serviceId!=-1)
		if (sections.table_id_extension!=m_serviceId) return;
	if (sections.version_number==m_pmtVersion) 
	{
		_isFound=true;
		return;
	}

	int section_syntax_indicator = (section[start+1]>>7) & 1;
	int section_length = ((section[start+1]& 0xF)<<8) + section[start+2];
	int program_number = (section[start+3]<<8)+section[start+4];
	int version_number = ((section[start+5]>>1)&0x1F);
	int current_next_indicator = section[start+5] & 1;
	int section_number = section[start+6];
	int last_section_number = section[start+7];
	int pcr_pid=((section[start+8]& 0x1F)<<8)+section[start+9];
	int program_info_length = ((section[start+10] & 0xF)<<8)+section[start+11];
	int len2 = program_info_length;
	int pointer = 12;
	int len1 = section_length -( 9 + program_info_length +4);
	int x;
	
	m_pmtVersion=version_number;
  
	// loop 1
	while (len2 > 0)
	{
		int indicator=section[start+pointer];
		int descriptorLen=section[start+pointer+1];
		len2 -= (descriptorLen+2);
		pointer += (descriptorLen+2);
	}
	// loop 2
	int stream_type=0;
	int elementary_PID=0;
	int ES_info_length=0;
	int audioToSet=0;
	int subtitleToSet=0;

	m_pidInfos2.clear();
	while (len1 > 0)
	{
		//if (start+pointer+4>=sectionLen+9) return ;
    int curSubtitle=-1;
		stream_type = section[start+pointer];
		elementary_PID = ((section[start+pointer+1]&0x1F)<<8)+section[start+pointer+2];
		ES_info_length = ((section[start+pointer+3] & 0xF)<<8)+section[start+pointer+4];

		PidInfo2 pidInfo2;
		pidInfo2.fakePid=-1;
		pidInfo2.elementaryPid=elementary_PID;
		pidInfo2.streamType=stream_type;
		if (pidInfo2.streamType!=SERVICE_TYPE_DVB_SUBTITLES2)
			pidInfo2.logicalStreamType=stream_type;
		else
			pidInfo2.logicalStreamType=-1;
		pidInfo2.rawDescriptorSize=ES_info_length;
		memset(pidInfo2.rawDescriptorData,0xFF,ES_info_length);
		memcpy(pidInfo2.rawDescriptorData,&section[start+pointer+5],ES_info_length);

	  pointer += 5;
	  len1 -= 5;
	  len2 = ES_info_length;
		while (len2 > 0)
		{
			if (pointer+1>=sectionLen) 
			{
				LogDebug("pmt parser check1");
				return ;
			}
			x = 0;
			int indicator=section[start+pointer];
			x = section[start+pointer + 1] + 2;

			if(indicator==DESCRIPTOR_DVB_AC3)
				pidInfo2.logicalStreamType=SERVICE_TYPE_AUDIO_AC3;

			if(indicator==DESCRIPTOR_DVB_TELETEXT)
				pidInfo2.logicalStreamType=DESCRIPTOR_DVB_TELETEXT;

			if(indicator==DESCRIPTOR_DVB_SUBTITLING && stream_type==SERVICE_TYPE_DVB_SUBTITLES2)
				pidInfo2.logicalStreamType=SERVICE_TYPE_DVB_SUBTITLES2;

		  len2 -= x;
		  len1 -= x;
		  pointer += x;
	  }
		m_pidInfos2.push_back(pidInfo2);
  }
  if (m_pmtCallback!=NULL)
    m_pmtCallback->OnPidsReceived(GetPid(),m_serviceId);
	if (m_pmtCallback2!=NULL)
		m_pmtCallback2->OnPmtReceived2(pcr_pid,m_pidInfos2);
}

int CPmtParser::GetPmtVersion()
{
  return m_pmtVersion;
}

