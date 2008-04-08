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
#include <streams.h>
#include <bdatypes.h>
#include "VirtualChannelTableParser.h"

extern void LogDebug(const char *fmt, ...) ;
CVirtualChannelTableParser::CVirtualChannelTableParser(void)
{
  m_decoder = new CSectionDecoder();
  m_decoder->SetPid(PID_VCT);
  m_decoder->SetCallBack(this);
  m_decoder->Reset();

  m_iVctVersionC8=-1;
  m_iVctVersionC9=-1;
	//LogDebug("vct: ctor");
  m_pCallback=NULL;
}

CVirtualChannelTableParser::~CVirtualChannelTableParser(void)
{
	//LogDebug("vct: dtor");
  delete m_decoder;
}


void CVirtualChannelTableParser::SetCallback(IAtscCallback* callback)
{
  m_pCallback=callback;
}
void CVirtualChannelTableParser::Reset()
{
	//LogDebug("vct: Reset()");
	m_decoder->Reset();
  m_vecChannels.clear();
  m_iVctVersionC8=-1;
  m_iVctVersionC9=-1;
}

void CVirtualChannelTableParser::OnTsPacket(byte* tsPacket)
{
	m_decoder->OnTsPacket(tsPacket);
}

int CVirtualChannelTableParser::Count()
{
  return (int)m_vecChannels.size();
}

bool CVirtualChannelTableParser::GetChannel(int index,CChannelInfo& info)
{
	if (index < 0 || index >= Count()) return false;
	info=m_vecChannels[index];
	return true;
}

bool CVirtualChannelTableParser::GetChannelInfo(int serviceId,CChannelInfo& info)
{
	for (int i=0; i < (int)m_vecChannels.size();++i)
	{
		if (m_vecChannels[i].ServiceId==serviceId)
		{
		 info=m_vecChannels[i];
		 return true;
		}
	}
	return false;
}

void CVirtualChannelTableParser::OnNewSection(int pid, int tableId, CSection& newSection)
{
	if (tableId!=0xC8 && tableId!=0xC9) return;

  byte* buf=newSection.Data;
  
  int startOff=0;
	int table_id =newSection.table_id;

  //dump table!

  int section_syntax_indicator = (buf[1]>>7) & 1;
  int private_indicator = (buf[1]>>6) & 1; 
	int section_length = newSection.section_length;
	int transport_stream_id = newSection.table_id_extension;
  int version_number = ((buf[5]>>1)&0x1F);
	if (table_id==0xc8)
	{
		if (version_number==m_iVctVersionC8) return;
		m_iVctVersionC8=version_number;
	}
	if (table_id==0xc9)
	{
		if (version_number==m_iVctVersionC9) return;
		m_iVctVersionC9=version_number;
	}
	//LogDebug("VCT: received vct table id:%x version:%d", table_id,version_number);
  int current_next_indicator = buf[5] & 1;
  int section_number = buf[6];
  int last_section_number = buf[7];
  int protocol_version = buf[8];
  int num_channels_in_section = buf[9];
	int slen=((buf[1]& 0xF)<<8) + buf[2];
	//LogDebug("VCT:  channels:%d len:%d %d (%d/%d)", num_channels_in_section,section_length,slen,section_number,last_section_number);
  if (num_channels_in_section <= 0) return;
  int start=10;
  for (int i=0; i < num_channels_in_section;i++)
  {
	  char shortName[127];
	  strcpy(shortName,"unknown");
	  try
	  {
		  //shortname 7*16 bits (14 bytes) in UTF-16
		  for (int count=0; count < 7; count++)
		  {
			  shortName[count] = buf[1+start+count*2];
			  shortName[count+1]=0; 
		  }
	  }
	  catch(...)
	  {
	  }
		//LogDebug("VCT:      shortname:%s", shortName);

	  start+= 7*2;
	  // 4---10-- ------10 -------- 8------- 32------ -------- -------- -------- 16------ -------- 16------ -------- 2-111113 --6----- 16------ -------- 6-----10 --------
	  // 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
	  //    112      113      114       115      116    117      118       119     120     121       123      124      125      126      127      128      129      130
	  //     0        1        2         3        4      5        6         7       8       9        10       11       12       13       14       15       16       17 
	  //  ++++++++ ++++++++ --+-++-	
	  // 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
	  int major_channel    		 =((buf[start  ]&0xf)<<8) + (buf[start+1]>>2);
	  int minor_channel    		 =((buf[start+1]&0x3)<<8) + buf[start+2];
	  int modulation_mode  		 = buf[start+3];
	  int carrier_frequency		 = (buf[start+4]<<24) + (buf[start+5]<<16) + (buf[start+6]<<8) + (buf[start+7]);
	  int channel_TSID			 = ((buf[start+8])<<8) + buf[start+9];
	  int program_number			 = ((buf[start+10])<<8) + buf[start+11];
	  int ETM_location			 = ((buf[start+12]>>6)&0x3);
	  int access_controlled		 = ((buf[start+12])&0x20);
	  int hidden          		 = ((buf[start+12]>>3)&0x10);
	  int hide_guide      		 = ((buf[start+12]>>2)&0x2);

	  int service_type             = ((buf[start+13]   )&0x3f);
	  int source_id				 = ((buf[start+14])<<8) + buf[start+15];
	  int descriptors_length		 = ((buf[start+16]&0x3)<<8) + buf[start+17];

		//LogDebug("VCT:      major:%d minor:%d tsid:%x sid:%x mod:%d servicetype:%d desclen:%d", 
		//	major_channel,minor_channel,channel_TSID, program_number,modulation_mode, service_type,descriptors_length);
	  if (major_channel==0 && minor_channel==0 && channel_TSID==0 && service_type==0 )
	  {
			//LogDebug("VCT:      skip, all is 0");
		  return;
	  }
	  if (modulation_mode < 0 || modulation_mode > 5)
	  {
			//LogDebug("VCT:      skip, invalid modulation mode:%d",modulation_mode);
		  return;
	  }
	  CChannelInfo info;
	  strcpy(info.ProviderName,"");
	  strcpy(info.ServiceName,"");
	  strcpy((char*)info.ProviderName,"unknown");
	  strcpy((char*)info.ServiceName,shortName);
	  info.MinorChannel = minor_channel;
	  info.MajorChannel = major_channel;
	  info.Frequency    = carrier_frequency;
	  info.ServiceId= program_number;
	  info.TransportId = channel_TSID;		
		if (access_controlled) info.FreeCAMode=0;
		else info.FreeCAMode=1;

	  if (service_type==1||service_type==2) info.ServiceType=1;//ATSC video
	  if (service_type==3) info.ServiceType=2;//ATSC audio
	  switch (modulation_mode)
	  {
		  case 0: //reserved
			  info.Modulation   = BDA_MOD_NOT_SET;
		  break;
		  case 1: //analog
			  info.Modulation   = BDA_MOD_ANALOG_FREQUENCY;
		  break;
		  case 2: //QAM64
			  info.Modulation   = BDA_MOD_64QAM;
		  break;
		  case 3: //QAM256
			  info.Modulation   = BDA_MOD_256QAM;
		  break;
		  case 4: //8 VSB
			  info.Modulation   = BDA_MOD_8VSB;
		  break;
		  case 5: //16 VSB
			  info.Modulation   = BDA_MOD_16VSB;
		  break;
		  default: //
			  info.Modulation   = BDA_MOD_NOT_SET;
		  break;

	  }

		//LogDebug("VCT:      descriptors_length:%d", descriptors_length);
	  start += 18;
	  int len=0;
	  while (len < descriptors_length && descriptors_length>0)
	  {
		  int descriptor_tag = buf[start+len];
		  int descriptor_len = buf[start+len+1];
		  if (descriptor_len==0 || descriptor_len+start > section_length)
		  {
				//LogDebug("VCT:      end of descriptors");
			  break;
		  }			
			//LogDebug("VCT:      tag:%x len:%d", descriptor_tag,descriptor_len);
		  switch (descriptor_tag)
		  {
			  case 0xa1:
				  DecodeServiceLocationDescriptor( buf,start+len, info);
			  break;
			  case 0xa0:
				  DecodeExtendedChannelNameDescriptor( buf,start+len,info, section_length);
			  break;
		  }
		  len += (descriptor_len+2);
	  }
		start += descriptors_length;
		LogDebug("VCT:  #%d major:%d minor:%d freq:%d sid:%x tsid:%x servicetype:%x name:%s video:%x audio:%x audio:%x audio:%x ac3:%x", 
				i,
				info.MajorChannel,info.MinorChannel,info.Frequency,
				info.ServiceId,info.TransportId,info.ServiceType,
				info.ServiceName,info.PidTable.VideoPid,info.PidTable.AudioPid1,info.PidTable.AudioPid2,info.PidTable.AudioPid3,info.PidTable.AC3Pid);
    m_vecChannels.push_back(info);
  }

  try
  {
    for (int ch1=0; ch1 < (int)m_vecChannels.size();++ch1)
    {
      int nr=1;
      CChannelInfo& infoCh1 = m_vecChannels[ch1];

      for (int ch2=0; ch2 < (int)m_vecChannels.size();++ch2)
      {
        if (ch2==ch1) continue;
        CChannelInfo& infoCh2 = m_vecChannels[ch2];
        if (strcmp(infoCh1.ServiceName, infoCh2.ServiceName)==0)
        {
          if (infoCh1.MajorChannel!=infoCh2.MajorChannel || infoCh1.MinorChannel!=infoCh2.MinorChannel)
          {
            sprintf(infoCh2.ServiceName,"%s#%d", infoCh2.ServiceName,nr++);
          }
        }
      }
    }

    for (int ch1=0; ch1 < (int)m_vecChannels.size();++ch1)
    {
      CChannelInfo& infoCh1 = m_vecChannels[ch1];
      if (m_pCallback!=NULL)
      {
        m_pCallback->OnChannel(infoCh1);
      }
    }
  }
  catch(...)
  {
    LogDebug("VCT: exception");
  }
}

void CVirtualChannelTableParser::DecodeServiceLocationDescriptor( byte* buf,int start,CChannelInfo& channelInfo)
{

	//LogDebug("DecodeServiceLocationDescriptor");
	//  8------ 8------- 3--13--- -------- 8-------       
	// 76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210
	//    0        1        2         3        4       5       6         7        8       9     
	int pcr_pid = ((buf[start+2]&0x1f)<<8) + buf[start+3];
	int number_of_elements = buf[start+4];
	int off=start+5;
	channelInfo.PidTable.PcrPid=pcr_pid;

	if (number_of_elements==0) return;
	
	//LogDebug("  pcrpid:%x, nr of elements:%d",pcr_pid,number_of_elements);
	int audioSet=0;
	for (int i=0; i < number_of_elements;++i)
	{

		//  8------ 3--13--- -------- 24------ -------- --------
		// 76543210|76543210|76543210|76543210|76543210|76543210|
		//    0        1        2         3        4       5     
		int streamtype			  = buf[off];
		int elementary_pid		  = ((buf[off+1]&0x1f)<<8) + buf[off+2];
		int ISO_639_language_code =	(buf[off+3]<<16) +(buf[off+4]<<8) + (buf[off+5]);

		//LogDebug("  nr:%d streamtype:%x pid:%x", i,streamtype,elementary_pid);
		off+=6;
		//pmtData.data=ISO_639_language_code;
		switch (streamtype)
		{
			case 0x1: // video
			case 0x2: // video
				channelInfo.PidTable.VideoPid=elementary_pid;
				break;
			case 0x3: // audio
			case 0x4: // audio
      case 0x0f: // AAC audio
      case 0x11: // LATM AAC audio
				if (audioSet==0)
				{
					channelInfo.PidTable.AudioPid1=elementary_pid;
					audioSet++;
				}
				else if (audioSet==1)
				{
					channelInfo.PidTable.AudioPid2=elementary_pid;
					audioSet++;
				}
				else if (audioSet==2)
				{
					channelInfo.PidTable.AudioPid3=elementary_pid;
					audioSet++;
				}
				break;

			case 0x81: // ac3
				channelInfo.PidTable.AC3Pid=elementary_pid;
				break;

			default:
				break;
		}
	}
}
void CVirtualChannelTableParser::DecodeExtendedChannelNameDescriptor( byte* buf,int start,CChannelInfo& channelInfo, int maxLen)
{
	//LogDebug("  DecodeExtendedChannelNameDescriptor");
	// tid   
	//  8       8------- 8-------
	// 76543210|76543210|76543210
	//    0        1        2    
	int descriptor_tag = buf[start+0];
	int descriptor_len = buf[start+1];
	
	//LogDebug("  tag:%x len:%d",descriptor_tag,descriptor_len);
	char* label = DecodeMultipleStrings(buf,start+2,maxLen);
	if (label==NULL) return ;
	strcpy((char*)channelInfo.ServiceName,label);

	delete [] label;
}

char* CVirtualChannelTableParser::DecodeMultipleStrings(byte* buf, int offset, int maxLen)
{
	int number_of_strings = buf[offset];
	//LogDebug("DecodeMultipleStrings() number_of_strings:%d",number_of_strings);
	

	for (int i=0; i < number_of_strings;++i)
	{
		//LogDebug("  string:%d", i);
		if (offset+4>=maxLen)
		{
			//LogDebug("  eob");
			return "";
		}
		int ISO_639_language_code = (buf[offset+1]<<16)+(buf[offset+2]<<8)+(buf[offset+3]);
		int number_of_segments=buf[offset+4];
		int start=offset+5;
		//LogDebug("  segments:%d", number_of_segments);
		for (int k=0; k < number_of_segments;++k)
		{
			if (start+2>=maxLen)
			{
				//LogDebug("  eob2");
				return "";
			}
			int compression_type = buf[start];
			int mode             = buf[start+1];
			int number_bytes     = buf[start+2];
			
			//LogDebug("  decode segment:%d type:%d mode:%d bytes:%d", k,compression_type,mode,number_bytes);
			//decode text....
			char *label=DecodeString(buf, start+3, compression_type,mode,number_bytes);
			start += (number_bytes+3);
			if (label!=NULL) return label;
		}
	}
	return NULL;
}

char* CVirtualChannelTableParser::DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes)
{
	//LogDebug("DecodeString() compression type:%d numberofbytes:%d",compression_type, mode);
	if (compression_type==0 && mode==0)
	{
		char* label = new char[number_of_bytes+1];
		memcpy(label,&buf[offset],number_of_bytes);
		label[number_of_bytes]=0;
		return (char*)label;
	}
	//string data="";
	//for (int i=0; i < number_of_bytes;++i)
	//	data += String.Format(" {0:X}", buf[offset+i]);

	return NULL;
}
