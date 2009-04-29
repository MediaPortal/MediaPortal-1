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
#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <windows.h>
#include "PmtParser.h"
#include "channelinfo.h"
#include <cassert>

void LogDebug(const char *fmt, ...) ; 

CPmtParser::CPmtParser()
{
	m_pmtCallback=NULL;
	_isFound=false;
}

CPmtParser::~CPmtParser(void)
{
}

void CPmtParser::SetPmtCallBack(IPmtCallBack* callback)
{
	m_pmtCallback=callback;
}

bool CPmtParser::IsReady()
{
	return _isFound;
}
void CPmtParser::OnNewSection(CSection& section)
{   
	if (section.table_id!=2) return;

	try{

	int program_number = section.table_id_extension;
	int pcr_pid=((section.Data[8]& 0x1F)<<8)+section.Data[9];
	int program_info_length = ((section.Data[10] & 0xF)<<8)+section.Data[11];
	int len2 = program_info_length;
	int pointer = 12;
	int len1 = section.section_length -( 9 + program_info_length +4);
	int x;
	if (!_isFound)
	{
		//LogDebug("got pmt:%x service id:%x", GetPid(), program_number);
		_isFound=true;	
		if (m_pmtCallback!=NULL)
		{
			m_pmtCallback->OnPmtReceived(GetPid());
		}
	}

	// loop 1
	while (len2 > 0)
	{
		int indicator=section.Data[pointer];
		int descriptorLen=section.Data[pointer+1];
		len2 -= (descriptorLen+2);
		pointer += (descriptorLen+2);
	}
	// loop 2
	int stream_type=0;
	int elementary_PID=0;
	int ES_info_length=0;
	int audioToSet=0;
	int subtitleToSet=0;

	m_pidInfo.Reset();
	m_pidInfo.PmtPid=GetPid();
	m_pidInfo.ServiceId=program_number;
	while (len1 > 0)
	{
		//if (start+pointer+4>=sectionLen+9) return ;
		int curAudio=-1;
    int curSubtitle=-1;
		stream_type = section.Data[pointer];
		elementary_PID = ((section.Data[pointer+1]&0x1F)<<8)+section.Data[pointer+2];
		ES_info_length = ((section.Data[pointer+3] & 0xF)<<8)+section.Data[pointer+4];
		// LogDebug("pmt: pid:%x type:%x",elementary_PID, stream_type);
		if(stream_type==SERVICE_TYPE_VIDEO_MPEG1 || stream_type==SERVICE_TYPE_VIDEO_MPEG2)
		{
			//mpeg2 video
			if(m_pidInfo.VideoPid==0)
			{
				m_pidInfo.VideoPid=elementary_PID;
				m_pidInfo.videoServiceType=stream_type;
			}
		}
		if(stream_type==SERVICE_TYPE_VIDEO_MPEG4 || stream_type==SERVICE_TYPE_VIDEO_H264)
		{
			//h.264/mpeg4 video
			if(m_pidInfo.VideoPid==0)
			{
				m_pidInfo.VideoPid=elementary_PID;
				m_pidInfo.videoServiceType=stream_type;
			}
		}
		if(stream_type==SERVICE_TYPE_AUDIO_MPEG1 || stream_type==SERVICE_TYPE_AUDIO_MPEG2 || stream_type==SERVICE_TYPE_AUDIO_AC3 || stream_type==SERVICE_TYPE_AUDIO_AAC || stream_type==SERVICE_TYPE_AUDIO_LATM_AAC)
		{				  
			audioToSet++;
			curAudio=audioToSet;
			switch(curAudio)
			{
			case 1:
				m_pidInfo.AudioPid1=elementary_PID;
				m_pidInfo.AudioServiceType1=stream_type;
				break;
			case 2:
				m_pidInfo.AudioPid2=elementary_PID;
				m_pidInfo.AudioServiceType2=stream_type;
				break;
			case 3:
				m_pidInfo.AudioPid3=elementary_PID;
				m_pidInfo.AudioServiceType3=stream_type;
				break;
			case 4:
				m_pidInfo.AudioPid4=elementary_PID;
				m_pidInfo.AudioServiceType4=stream_type;
				break;
			case 5:
				m_pidInfo.AudioPid5=elementary_PID;
				m_pidInfo.AudioServiceType5=stream_type;
				break;
			case 6:
				m_pidInfo.AudioPid6=elementary_PID;
				m_pidInfo.AudioServiceType6=stream_type;
				break;
			case 7:
				m_pidInfo.AudioPid7=elementary_PID;
				m_pidInfo.AudioServiceType7=stream_type;
				break;
			case 8:
				m_pidInfo.AudioPid8=elementary_PID;
				m_pidInfo.AudioServiceType8=stream_type;
				break;
			}
		}
		m_pidInfo.PcrPid=pcr_pid;

		pointer += 5;
		len1 -= 5;
		len2 = ES_info_length;
		
		while (len2 > 0)
		{
			if (pointer+1>=section.section_length) 
			{
				LogDebug("pmt parser check1");
				return ;
			}
			x = 0;
			
			int indicator=section.Data[pointer];
			x = section.Data[pointer + 1] + 2;
						
			if(indicator==DESCRIPTOR_DVB_AC3)
			{								
				if (curAudio<0) 
				{
					audioToSet++;
					curAudio=audioToSet;
				}
				switch (curAudio)
				{
				case 1:
					m_pidInfo.AudioPid1=elementary_PID;
					m_pidInfo.AudioServiceType1=SERVICE_TYPE_AUDIO_AC3;					
					break;
				case 2:
					m_pidInfo.AudioPid2=elementary_PID;
					m_pidInfo.AudioServiceType2=SERVICE_TYPE_AUDIO_AC3;				
					break;
				case 3:
					m_pidInfo.AudioPid3=elementary_PID;
					m_pidInfo.AudioServiceType3=SERVICE_TYPE_AUDIO_AC3;
					break;
				case 4:				  
					m_pidInfo.AudioPid4=elementary_PID;
					m_pidInfo.AudioServiceType4=SERVICE_TYPE_AUDIO_AC3;
					break;
				case 5:
					m_pidInfo.AudioPid5=elementary_PID;
					m_pidInfo.AudioServiceType5=SERVICE_TYPE_AUDIO_AC3;
					break;
				case 6:
					m_pidInfo.AudioPid6=elementary_PID;
					m_pidInfo.AudioServiceType6=SERVICE_TYPE_AUDIO_AC3;
					break;
				case 7:
					m_pidInfo.AudioPid7=elementary_PID;
					m_pidInfo.AudioServiceType7=SERVICE_TYPE_AUDIO_AC3;
					break;
				case 8:
					m_pidInfo.AudioPid8=elementary_PID;
					m_pidInfo.AudioServiceType8=SERVICE_TYPE_AUDIO_AC3;
					break;
				}
			}
			
			if(indicator==DESCRIPTOR_MPEG_ISO639_Lang)
			{					
				if (pointer+4>=section.section_length) 
				{
					LogDebug("pmt parser check2");
					return ;
				}

				if (curAudio<0) 
				{
					audioToSet++;
					curAudio=audioToSet;
				}

				BYTE d[3];
				d[0]=section.Data[pointer+2];
				d[1]=section.Data[pointer+3];
				d[2]=section.Data[pointer+4];

				if(curAudio==1)
				{
					m_pidInfo.Lang1_1=d[0];
					m_pidInfo.Lang1_2=d[1];
					m_pidInfo.Lang1_3=d[2];
				}
				if(curAudio==2)
				{
					m_pidInfo.Lang2_1=d[0];
					m_pidInfo.Lang2_2=d[1];
					m_pidInfo.Lang2_3=d[2];
				}
				if(curAudio==3)
				{
					m_pidInfo.Lang3_1=d[0];
					m_pidInfo.Lang3_2=d[1];
					m_pidInfo.Lang3_3=d[2];
				}
				if(curAudio==4)
				{
					m_pidInfo.Lang4_1=d[0];
					m_pidInfo.Lang4_2=d[1];
					m_pidInfo.Lang4_3=d[2];
				}
				if(curAudio==5)
				{
					m_pidInfo.Lang5_1=d[0];
					m_pidInfo.Lang5_2=d[1];
					m_pidInfo.Lang5_3=d[2];
				}
				if(curAudio==6)
				{
					m_pidInfo.Lang6_1=d[0];
					m_pidInfo.Lang6_2=d[1];
					m_pidInfo.Lang6_3=d[2];
				}
				if(curAudio==7)
				{
					m_pidInfo.Lang7_1=d[0];
					m_pidInfo.Lang7_2=d[1];
					m_pidInfo.Lang7_3=d[2];
				}
				if(curAudio==8)
				{
					m_pidInfo.Lang8_1=d[0];
					m_pidInfo.Lang8_2=d[1];
					m_pidInfo.Lang8_3=d[2];
				}
			}
			if(indicator==0x46){
				LogDebug("VBI teletext descriptor");
			}
			if(indicator==DESCRIPTOR_DVB_TELETEXT /*&& m_pidInfo.TeletextPid==0*/){
				m_pidInfo.TeletextPid=elementary_PID;
				assert(section.Data[pointer+0] == DESCRIPTOR_DVB_TELETEXT);
				int descriptorLen = section.Data[pointer+1];

				int varBytes = 5; // 4 additional fields for a total of 32 bits (see 6.2.40)
				
				assert(descriptorLen % varBytes == 0); // there shouldnt be any left over bytes :)
				
				int N = descriptorLen / varBytes; 

				//BYTE b = 0x02 << 3;

				//LogDebug("Descriptor length %i, N= %i", descriptorLen, N);
				for(int j = 0; j < N; j++){
					BYTE ISO_639_language_code[3];
					ISO_639_language_code[0] = section.Data[pointer + varBytes*j + 2];
					ISO_639_language_code[1] = section.Data[pointer + varBytes*j + 3];
					ISO_639_language_code[2] = section.Data[pointer + varBytes*j + 4];

					BYTE b3 = section.Data[pointer + varBytes*j + 5];
					BYTE teletext_type = (b3 & 0xF8) >> 3; // 5 first(msb) bits
					
					assert(teletext_type <= 0x05); // 0x06 and upwards reserved for future use and shouldnt appear
					//for(int i = 0; i < 8; i++){
					//	if( ((b3 << i) & 128) != 0) LogDebug("1");	
					//	else LogDebug("0");
					//}

					int teletext_magazine_number = (b3 & 0x07); // last(lsb) 3 bits

					int teletext_page_number = (section.Data[pointer + varBytes*j + 6]);

					int real_page_tens  = (teletext_page_number & 0xF0) >> 4;
					int real_page_units = teletext_page_number & 0x0F;

					int real_page = teletext_magazine_number*100 + real_page_tens*10 + real_page_units;

					//LogDebug("Mag: %i, tens %i, units %i, total ?= %i", teletext_magazine_number,real_page_tens,real_page_units,real_page);

					if(teletext_type == 0x02 || teletext_type == 0x05) { //if its a teletext subtitle service (standard / hard of hearing respectively)

						if(!m_pidInfo.HasTeletextPageInfo(real_page)){
							//LogDebug("Teletext subtitles in PMT: PID %i, mag %i, page %i, prevPage %i", elementary_PID, teletext_magazine_number, real_page, m_pidInfo.TeletextSubPage);
							TeletextServiceInfo info;
							info.page = real_page;
							info.type = teletext_type;
							info.lang[0] = ISO_639_language_code[0];
							info.lang[1] = ISO_639_language_code[1];
							info.lang[2] = ISO_639_language_code[2];
							m_pidInfo.TeletextInfo.push_back(info);
						}
					}
					else{
						//LogDebug("Teletext SI: Page %i Type %X",real_page,teletext_type);
					}
				}

			}
			if(indicator==DESCRIPTOR_DVB_SUBTITLING ) //&& m_pidInfo.SubtitlePid==0)
			{
			  if (stream_type==SERVICE_TYPE_DVB_SUBTITLES2)
        {
          subtitleToSet++;
			    curSubtitle=subtitleToSet;
				  BYTE d[3];
          d[0]=section.Data[pointer+2];
					d[1]=section.Data[pointer+3];
					d[2]=section.Data[pointer+4];

			    switch(curSubtitle)
			    {
			    case 1:
					  m_pidInfo.SubtitlePid1=elementary_PID;
					  m_pidInfo.SubLang1_1=d[0];
					  m_pidInfo.SubLang1_2=d[1];
					  m_pidInfo.SubLang1_3=d[2];
				    break;
			    case 2:
					  m_pidInfo.SubtitlePid2=elementary_PID;
					  m_pidInfo.SubLang2_1=d[0];
					  m_pidInfo.SubLang2_2=d[1];
					  m_pidInfo.SubLang2_3=d[2];
				    break;
			    case 3:
					  m_pidInfo.SubtitlePid3=elementary_PID;
					  m_pidInfo.SubLang3_1=d[0];
					  m_pidInfo.SubLang3_2=d[1];
					  m_pidInfo.SubLang3_3=d[2];
				    break;
			    case 4:
					  m_pidInfo.SubtitlePid4=elementary_PID;
					  m_pidInfo.SubLang4_1=d[0];
					  m_pidInfo.SubLang4_2=d[1];
					  m_pidInfo.SubLang4_3=d[2];
				    break;
          }
        }
			}
			len2 -= x;
			len1 -= x;
			pointer += x;
		}
	}
	if (m_pmtCallback!=NULL)
	{
		//LogDebug("DecodePMT pid:0x%x pcrpid:0x%x videopid:0x%x audiopid:0x%x ac3pid:0x%x sid:%x",
		//  m_pidInfo.PmtPid, m_pidInfo.PcrPid,m_pidInfo.VideoPid,m_pidInfo.AudioPid1,m_pidInfo.AC3Pid,m_pidInfo.ServiceId);
		m_pmtCallback->OnPidsReceived(m_pidInfo);
	}
	} catch (...) { LogDebug("Exception in PmtParser");}
}


CPidTable& CPmtParser::GetPidInfo()
{
	return m_pidInfo;
}