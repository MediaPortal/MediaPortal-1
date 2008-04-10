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
#include "NITDecoder.h"

extern void LogDebug(const char *fmt, ...) ;
CNITDecoder::CNITDecoder(void)
{
	SetPid(0x10);
	SetTableId(0x40);
}

CNITDecoder::~CNITDecoder(void)
{
}

void CNITDecoder::OnNewSection(CSection& sections)
{
	byte* section=&(sections.Data[5]);
	decodeNITTable(section);
}

bool CNITDecoder::Ready()
{
	DWORD timeSpan=GetTickCount()-m_timer;
	if (timeSpan >=1000) return true;
	return false;
}
void CNITDecoder::Reset()
{
//	m_vecLCN.clear();
	CSectionDecoder::Reset();
	m_timer=GetTickCount();
}
int CNITDecoder::GetLogicialChannelNumber(int networkId, int transportId, int serviceId)
{
	for (int i=0; i < (int)m_vecLCN.size();++i)
	{
		NITLCN& lcn=m_vecLCN[i];
		if (lcn.network_id==networkId &&
			  lcn.transport_id==transportId &&
				lcn.service_id==serviceId)
		{
			return lcn.LCN;
		}
	}
	return 10000;
}

void  CNITDecoder::decodeNITTable(byte* buf)
{
	try
	{
		int table_id;
		int section_syntax_indicator;
		int section_length;
		int network_id;
		int version_number;
		int current_next_indicator;
		int section_number;
		int last_section_number;
		int network_descriptor_length;
		int transport_stream_loop_length;
		int transport_stream_id;
		int original_network_id;

		int transport_descriptor_length=0;
		//
		int pointer=0;
		int l1=0;
		int l2=0;

		//  0        1         2        3        4         5       6      7         8         9       10
		//76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|76543210|
		//++++++++ -+--++++ ++++++++ -------- -------- ++-----+ -------- ++++++++ ----++++ ++++++++
		table_id = buf[0];
		//0x40=actual (current) network
		//0x41=other networks
		if (table_id!=0x40 && table_id!=0x41) return;
		
		
		section_syntax_indicator = buf[1] &0x80;
		section_length = ((buf[1] &0xF)<<8)+buf[2];
		network_id = (buf[3]<<8)+buf[4];
		version_number = (buf[5]>>1) &0x1F;
		current_next_indicator = buf[5]&1;
		section_number = buf[6];
		last_section_number = buf[7];
		network_descriptor_length = ((buf[8]&0xF)<<8)+buf[9];

		//LogDebug("NIT section len:%d network_descriptor_length :%d", section_length,network_descriptor_length);
		l1 = network_descriptor_length;
		pointer = 10;
		int x = 0;

		while (l1 > 0)
		{
			int indicator=buf[pointer];
			x = buf[pointer + 1] + 2;
			//LogDebug("decode nit desc1:%x len:%d", indicator,x);
			/*
			if(indicator==0x40)
			{
				CAutoString networkName (x+10);
				strncpy(networkName.GetBuffer() ,(char*)&buf[pointer+2],x-2);
				networkName.GetBuffer()[x-2]=0;
				m_nit.NetworkName=networkName.GetBuffer();
			}*/
			l1 -= x;
			pointer += x;
		}
		pointer=10+network_descriptor_length;

		if (pointer > section_length)
		{
			return;
		}
	//	LogDebug("NIT: decode() network:'%s'", m_nit.NetworkName);
		
		transport_stream_loop_length = ((buf[pointer] &0xF)<<8)+buf[pointer+1];
		l1 = transport_stream_loop_length;
		pointer += 2;
		
		while (l1 > 0)
		{
			//LogDebug("loop1: %d/%d l1:%d",pointer,section_length,l1);
			if (pointer+2 > section_length)
			{
				//LogDebug("check1");
				return;
			}
			transport_stream_id = (buf[pointer]<<8)+buf[pointer+1];
			original_network_id = (buf[pointer+2]<<8)+buf[pointer+3];
			transport_descriptor_length = ((buf[pointer+4] & 0xF)<<8)+buf[pointer+5];
			pointer += 6;
			l1 -= 6;
			l2 = transport_descriptor_length;
			
			//LogDebug("    transport_descriptor_length :%d", transport_descriptor_length);
			while (l2 > 0)
			{
				//LogDebug("    loop2: %d/%d l1:%d",pointer,transport_descriptor_length,l2);
				if (pointer+2 > section_length)
				{
				//	LogDebug("check2");
					return;
				}
				int indicator=buf[pointer];
				x = buf[pointer + 1]+2 ;
				//LogDebug("     decode desc:%x len:%d", indicator,x);
				if(indicator==0x43) // sat
				{
					try
					{
						//DVB_GetSatDelivSys(&buf[pointer],x);
					}
					catch(...)
					{
						LogDebug("exception in DVB_GetSatDelivSys");
					}
				}
				if(indicator==0x44) // cable
				{
					try
					{
						//DVB_GetCableDelivSys(&buf[pointer],x);
					}
					catch(...)
					{
						LogDebug("exception in DVB_GetCableDelivSys");
					}
				}
				if(indicator==0x5A) // terrestrial
				{
					try
					{
						//DVB_GetTerrestrialDelivSys(&buf[pointer],x);
					}
					catch(...)
					{
						LogDebug("exception in DVB_GetTerrestrialDelivSys");
					}
				}
				
				if(indicator==0x83) // lcn
				{
					try
					{
						DVB_GetLogicalChannelNumber(original_network_id,transport_stream_id,&buf[pointer]);
					}
					catch(...)
					{
						LogDebug("exception in DVB_GetLogicalChannelNumber");
					}
				}
				//
				pointer += x;
				l2 -= x;
				l1 -= x;
			}
		}
		
		//LogDebug("NIT: terrestial:%d satellite:%d cable:%d LCN:%d",
		//	m_nit.terrestialNIT.size(),m_nit.satteliteNIT.size(),m_nit.cableNIT.size(),m_vecLCN.size());
	}
	catch(...)
	{
		LogDebug("nit: unhandled exception in Sections::DecodeNIT()");
	}	
}

void CNITDecoder::DVB_GetLogicalChannelNumber(int original_network_id,int transport_stream_id,byte* buf)
{
	try
	{
		// 32 bits per record
		int n = buf[1] / 4;
		if (n < 1)
			return;

		// desc id, desc len, (service id, service number)
		int pointer=2;
		int ServiceID, LCN;
		for (int i = 0; i < n; i++) 
		{
			//service id:16
			//visible_service_flag:1
			//reserved:5
			//logical channel number:10
			ServiceID = 0;
			LCN = 0;
			ServiceID = (buf[pointer+0]<<8) + (buf[pointer+1]&0xff);
			LCN		  = ((buf[pointer+2]&0x03)<<8) + (buf[pointer+3]&0xff);
			if (LCN==0) LCN=10000;//undefined
			else if (LCN>=1000) LCN=10000;//reserved

			pointer+=4;
			bool alreadyAdded=false;
			for (int j=0; j <(int) m_vecLCN.size();++j)
			{
				NITLCN& lcn = m_vecLCN[j];
				if (lcn.network_id==original_network_id && lcn.transport_id==transport_stream_id && lcn.service_id==ServiceID)
				{
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				if (original_network_id>0 && transport_stream_id>0 &&ServiceID>0 && LCN>=0)
				{
					NITLCN lcn;
					lcn.LCN=LCN;
					lcn.network_id=original_network_id;
					lcn.transport_id=transport_stream_id;
					lcn.service_id=ServiceID;
					m_vecLCN.push_back(lcn);
					m_timer=GetTickCount();
					//LogDebug("LCN:%03.3d network id:0x%x transport id:0x%x service id:0x%x (%d)", LCN,original_network_id,transport_stream_id,ServiceID,m_vecLCN.size());
				}
			}
		}
	}
	catch(...)
	{
		LogDebug("nit: unhandled exception in Sections::DVB_GetLogicalChannelNumber()");
	}	
}

