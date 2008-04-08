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
  EnableCrcCheck(false);
  Reset();
  m_pCallback=NULL;
}

CSdtParser::~CSdtParser(void)
{
}

void CSdtParser::Reset()
{        
	CSectionDecoder::Reset();
}

void CSdtParser::SetCallback(ISdtCallBack* callback)
{
  m_pCallback=callback;
}

void  CSdtParser::OnNewSection(CSection& sections)
{
  byte* section=(&sections.Data)[0];
	int sectionLen=sections.section_length;
	if (sections.table_id!=0x42 && sections.table_id!=0x46) return;
  
  long original_network_id = ((section[8])<<8)+section[9];
 // LogDebug("decodeSDTTable len=%d section no:%d last section no:%d cni:%d version:%d si:%d", 
	//	  section_length,section_number,last_section_number,current_next_indicator,version_number,section_syntax_indicator);


  int len1 = sectionLen - 11 - 4;
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
	  service_id = (section[pointer]<<8)+section[pointer+1];
	  EIT_schedule_flag = (section[pointer+2]>>1) & 1;
	  EIT_present_following_flag = section[pointer+2] & 1;
	  running_status = (section[pointer+3]>>5) & 7;
		
	  free_CA_mode = (section[pointer+3]>>4) &1;
	  descriptors_loop_length = ((section[pointer+3] & 0xF)<<8)+section[pointer+4];
	  
	  pointer += 5;
	  len1 -= 5;
	  len2 = descriptors_loop_length;

	  while (len2 > 0)
	  {
		  //if (start+pointer+1 >=4096) return ;
		  int indicator=section[pointer];
		  x = 0;
		  x = section[pointer + 1] + 2;
			//LogDebug("sdt parser: indicator = %x",indicator);
		  if (indicator == 0x48)
		  {					
        CChannelInfo info;
			  DVB_GetService(section+pointer,info);
        info.NetworkId=original_network_id;
				info.TransportId=sections.table_id_extension;
        info.ServiceId=service_id;
        info.FreeCAMode=free_CA_mode;
				info.OtherMux = (sections.table_id==0x46);
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


