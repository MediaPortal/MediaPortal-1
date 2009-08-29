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
	m_pmtCallback2=NULL;
	_isFound=false;
	m_serviceId=-1;
	EnableCrcCheck(false);
}

CPmtParser::~CPmtParser(void)
{
}

void CPmtParser::Reset()
{
	_isFound=false;
	m_pmtCallback2=NULL;
	CSectionDecoder::Reset();
}

void CPmtParser::SetFilter(int pid,int serviceId)
{
	SetPid(pid);
	m_serviceId=serviceId;
}

void CPmtParser::GetFilter(int &pid,int &serviceId)
{
	pid=GetPid();
	serviceId=m_serviceId;
}

void CPmtParser::SetPmtCallBack2(IPmtCallBack2* callback)
{
	m_pmtCallback2=callback;
}

bool CPmtParser::IsReady()
{
	return _isFound;
}

void CPmtParser::OnTsPacket(byte* tsPacket)
{
	if (_isFound) return;
	CSectionDecoder::OnTsPacket(tsPacket);
}

bool CPmtParser::DecodePmt(CSection sections, int &pcr_pid, vector<PidInfo2>& pidInfos)
{
	byte* section=sections.Data;
	int sectionLen=sections.section_length;

	int table_id = sections.table_id;
	if (table_id!=2) return false;
	if (m_serviceId!=-1)
		if (sections.table_id_extension!=m_serviceId) return false;

	int section_syntax_indicator = (section[1]>>7) & 1;
	int section_length = ((section[1]& 0xF)<<8) + section[2];
	int program_number = (section[3]<<8)+section[4];
	int version_number = ((section[5]>>1)&0x1F);
	int current_next_indicator = section[5] & 1;
	int section_number = section[6];
	int last_section_number = section[7];
	pcr_pid=((section[8]& 0x1F)<<8)+section[9];
	int program_info_length = ((section[10] & 0xF)<<8)+section[11];
	int len2 = program_info_length;
	int pointer = 12;
	int len1 = section_length -( 9 + program_info_length +4);
	int x;
	
	// loop 1
	while (len2 > 0)
	{
		int indicator=section[pointer];
		int descriptorLen=section[pointer+1];
		len2 -= (descriptorLen+2);
		pointer += (descriptorLen+2);
	}
	
	// loop 2
	int stream_type=0;
	int elementary_PID=0;
	int ES_info_length=0;
	int audioToSet=0;
	int subtitleToSet=0;

	pidInfos.clear();
	while (len1 > 0)
	{
		//if (start+pointer+4>=sectionLen+9) return ;
    int curSubtitle=-1;
		stream_type = section[pointer];
		elementary_PID = ((section[pointer+1]&0x1F)<<8)+section[pointer+2];
		ES_info_length = ((section[pointer+3] & 0xF)<<8)+section[pointer+4];

		if (pointer+ES_info_length>=sectionLen) 
		{
			LogDebug("pmt parser check 1");
			return false;
		}

		PidInfo2 pidInfo2;
		pidInfo2.fakePid=-1;
		pidInfo2.elementaryPid=elementary_PID;
		pidInfo2.streamType=stream_type;
    pidInfo2.rawDescriptorSize=ES_info_length;
    if (pidInfo2.streamType!=SERVICE_TYPE_DVB_SUBTITLES2)
      pidInfo2.logicalStreamType=stream_type;
    //ITV HD workaround
    if (pidInfo2.streamType==SERVICE_TYPE_DVB_SUBTITLES2 && program_number==10510)
    {
      if (pidInfo2.logicalStreamType==0xffffffff && pidInfo2.elementaryPid==0xd49)
      {
        pidInfo2.streamType=SERVICE_TYPE_VIDEO_H264;
        pidInfo2.logicalStreamType=SERVICE_TYPE_VIDEO_H264;
        LogDebug("DecodePmt: set ITV HD video stream to H.264");
      }
    }
    //end of workaround
    if (pidInfo2.streamType==SERVICE_TYPE_DVB_SUBTITLES2)
      pidInfo2.logicalStreamType=-1;
		memset(pidInfo2.rawDescriptorData,0xFF,ES_info_length);
		memcpy(pidInfo2.rawDescriptorData,&section[pointer+5],ES_info_length);

	  pointer += 5;
	  len1 -= 5;
	  len2 = ES_info_length;
		while (len2 > 0)
		{
			if (pointer+1>=sectionLen) 
			{
				LogDebug("pmt parser check2");
				return false;
			}
			x = 0;
			int indicator=section[pointer];
			x = section[pointer + 1] + 2;

			if(indicator==DESCRIPTOR_DVB_AC3)
				pidInfo2.logicalStreamType=SERVICE_TYPE_AUDIO_AC3;

			if(indicator==DESCRIPTOR_DVB_E_AC3)
				pidInfo2.logicalStreamType=SERVICE_TYPE_AUDIO_E_AC3;

			if(indicator==DESCRIPTOR_DVB_TELETEXT)
				pidInfo2.logicalStreamType=DESCRIPTOR_DVB_TELETEXT;

			if(indicator==DESCRIPTOR_DVB_SUBTITLING && stream_type==SERVICE_TYPE_DVB_SUBTITLES2)
				pidInfo2.logicalStreamType=SERVICE_TYPE_DVB_SUBTITLES2;

		  len2 -= x;
		  len1 -= x;
		  pointer += x;
	  }
		pidInfos.push_back(pidInfo2);		
  }
	return true;
}

void CPmtParser::OnNewSection(CSection& sections)
{ 
	if (_isFound) return;

	int pcr_pid=0;

	if (!DecodePmt(sections,pcr_pid,m_pidInfos2)) return;

	if (m_pmtCallback2!=NULL)
	{
		m_pmtCallback2->OnPmtReceived2(GetPid(),m_serviceId,pcr_pid,m_pidInfos2);
		_isFound=true;
		m_pmtCallback2=NULL;
	}
}
