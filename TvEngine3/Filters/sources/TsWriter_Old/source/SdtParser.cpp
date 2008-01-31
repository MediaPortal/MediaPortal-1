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
#include "sdtParser.h"

void LogDebug(const char *fmt, ...) ;
CSdtParser::CSdtParser(void)
{
  SetPid(PID_SDT);
  SetTableId(0x42);
  EnableCrcCheck(false);
  Reset();
  m_pCallback=NULL;
}

CSdtParser::~CSdtParser(void)
{
}

void CSdtParser::Reset()
{        
  m_bFound=false;

	CSectionDecoder::Reset();
}

bool CSdtParser::IsReady()
{
  return m_bFound;
}
void CSdtParser::SetCallback(ISdtCallBack* callback)
{
  m_pCallback=callback;
}

void  CSdtParser::OnNewSection(CSection& sections)
{
  byte* section=(&sections.Data)[0];
  int sectionLen=sections.SectionLength;
  m_tsHeader.Decode(section);
  int start=m_tsHeader.PayLoadStart;
  int table_id = section[start+0];
	if (table_id!=0x42 && table_id!=0x46) return;
  int section_syntax_indicator = (section[start+1]>>7) & 1;
  int section_length = ((section[start+1]& 0xF)<<8) + section[start+2];
  
  long transport_stream_id = (section[start+3]<<8)+section[start+4];
  int version_number = ((section[start+5]>>1)&0x1F);


  int current_next_indicator = section[start+5] & 1;
  int section_number = section[start+6];
  int last_section_number = section[start+7];
  long original_network_id = ((section[start+8])<<8)+section[start+9];
 // LogDebug("decodeSDTTable len=%d section no:%d last section no:%d cni:%d version:%d si:%d", 
	//	  section_length,section_number,last_section_number,current_next_indicator,version_number,section_syntax_indicator);


    section_length=sectionLen;
  int len1 = section_length - 11 - 4;
  int descriptors_loop_length;
  int len2;
  long service_id;
  int EIT_schedule_flag;
  int free_CA_mode;
  int running_status;
  int EIT_present_following_flag;
  int pointer = 11;
  int x = 0;
  //int channel;	 

  while (len1 > 0)
  {
	  //if (start+pointer+4 >=sectionLen) return ;
	  service_id = (section[start+pointer]<<8)+section[start+pointer+1];
	  EIT_schedule_flag = (section[start+pointer+2]>>1) & 1;
	  EIT_present_following_flag = section[start+pointer+2] & 1;
	  running_status = (section[start+pointer+3]>>5) & 7;
		
	  free_CA_mode = (section[start+pointer+3]>>4) &1;
	  descriptors_loop_length = ((section[start+pointer+3] & 0xF)<<8)+section[start+pointer+4];
	  
	  pointer += 5;
	  len1 -= 5;
	  len2 = descriptors_loop_length;

	  while (len2 > 0)
	  {
		  //if (start+pointer+1 >=4096) return ;
		  int indicator=section[start+pointer];
		  x = 0;
		  x = section[start+pointer + 1] + 2;
			//LogDebug("sdt parser: indicator = %x",indicator);
		  if (indicator == 0x48)
		  {					
        CChannelInfo info;
			  DVB_GetService(section+start+pointer,info);
        info.NetworkId=original_network_id;
        info.TransportId=transport_stream_id;
        info.ServiceId=service_id;
        info.EIT_schedule_flag=EIT_schedule_flag;
        info.EIT_present_following_flag=EIT_present_following_flag;
        info.RunningStatus=running_status;
        info.FreeCAMode=free_CA_mode;
				info.OtherMux = (table_id==0x46);
				m_bFound=true;
        if (m_pCallback!=NULL)
          m_pCallback->OnSdtReceived(info);
		  }
		  else
		  {
			  int st=indicator;
			  if(st!=0x53 && st!=0x64)
				  st=1;
		  }
		  len2 -= x;
		  pointer += x;
		  len1 -= x;
	  }		
  }        

}

void CSdtParser::DVB_GetService(BYTE *b,CChannelInfo& info)
{
	int descriptor_tag;
	int descriptor_length;
	int service_provider_name_length;
	int service_name_length;
	int pointer = 0;
	
	descriptor_tag = b[0];
	descriptor_length = b[1];
	info.ServiceType = b[2];
	service_provider_name_length = b[3];
	pointer = 4;
	getString468A(b+pointer,service_provider_name_length,info.ProviderName);
	pointer += service_provider_name_length;
	service_name_length = b[pointer];
	pointer += 1;
	getString468A(b+pointer, service_name_length,info.ServiceName);
}


