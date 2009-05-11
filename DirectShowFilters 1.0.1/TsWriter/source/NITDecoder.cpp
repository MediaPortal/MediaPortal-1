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
#include <bdatypes.h>
#include "NITDecoder.h"


extern void LogDebug(const char *fmt, ...) ;
CNITDecoder::CNITDecoder(void)
{
	SetPid(PID_NIT);
  EnableCrcCheck(false);
}

CNITDecoder::~CNITDecoder(void)
{
}

void CNITDecoder::OnNewSection(CSection& sections)
{
	if (sections.table_id!=0x40) return;

	byte* section=sections.Data;
	decodeNITTable(section);
}

bool CNITDecoder::Ready()
{
	DWORD timeSpan=GetTickCount()-m_timer;
	if (timeSpan >=5000) return true;
	return false;
}
void CNITDecoder::Reset()
{
  LogDebug("NIT:Reset");
//	m_vecLCN.clear();
	CSectionDecoder::Reset();
	m_timer=GetTickCount();
  m_nit.satteliteNIT.clear();
  m_nit.cableNIT.clear();
  m_nit.terrestialNIT.clear();
  m_nit.lcnNIT.clear();
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
		//LogDebug("NIT: decode() network:'%s'", m_nit.NetworkName);
		
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
					//LogDebug("check2");
					return;
				}
				int indicator=buf[pointer];
				x = buf[pointer + 1]+2 ;
				//LogDebug("     decode desc:%x len:%d", indicator,x);
				if(indicator==0x43) // sat
				{
					try
					{
						DVB_GetSatDelivSys(&buf[pointer],x);
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
						DVB_GetCableDelivSys(&buf[pointer],x);
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
						DVB_GetTerrestrialDelivSys(&buf[pointer],x);
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
			for (int j=0; j < (int)m_vecLCN.size();++j)
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




void CNITDecoder::DVB_GetSatDelivSys(byte* b,int maxLen)
{
	try
	{
		if(b[0]==0x43 && maxLen>=13)
		{
			int descriptor_tag = b[0];
			int descriptor_length = b[1];
			
			if (descriptor_length>13) 
			{
				LogDebug("DVB_GetSatDelivSys() desclen:%d", descriptor_length);
				return;
			}
			NITSatDescriptor satteliteNIT;
			satteliteNIT.Frequency = (10000000* ((b[2]>>4)&0xf));
			satteliteNIT.Frequency+= (1000000*  ((b[2]&0xf)));
			satteliteNIT.Frequency+= (100000*   ((b[3]>>4)&0xf));
			satteliteNIT.Frequency+= (10000*    ((b[3]&0xf)));
			satteliteNIT.Frequency+= (1000*     ((b[4]>>4)&0xf));
			satteliteNIT.Frequency+= (100*      ((b[4]&0xf)));
			satteliteNIT.Frequency+= ( 10*      ((b[5]>>4)&0xf));
			satteliteNIT.Frequency+= (b[5]&0xf);

			satteliteNIT.OrbitalPosition+= (1000*     ((b[6]>>4)&0xf));
			satteliteNIT.OrbitalPosition+= (100*      ((b[6]&0xf)));
			satteliteNIT.OrbitalPosition+= ( 10*      ((b[7]>>4)&0xf));
			satteliteNIT.OrbitalPosition+= (b[7]&0xf);

			satteliteNIT.WestEastFlag = (b[8] & 0x80)>>7;
			satteliteNIT.Polarisation = (b[8]& 0x60)>>5;
			if(satteliteNIT.Polarisation>1)
				satteliteNIT.Polarisation-=2;
			// polarisation
			// 0 - horizontal/left (linear/circluar)
			// 1 - vertical/right (linear/circluar)
			satteliteNIT.Modulation = (b[8] & 0x1F);
			satteliteNIT.Symbolrate = (1000000* ((b[9]>>4)&0xf));
			satteliteNIT.Symbolrate+= (100000*  ((b[9]&0xf)));
			satteliteNIT.Symbolrate+= (10000*   ((b[10]>>4)&0xf));
			satteliteNIT.Symbolrate+= (1000*    ((b[10]&0xf)));
			satteliteNIT.Symbolrate+= (100*     ((b[11]>>4)&0xf));
			satteliteNIT.Symbolrate+= (10*      ((b[11]&0xf)));
			satteliteNIT.Symbolrate+= (         ((b[12]>>4)&0xf));
			satteliteNIT.FECInner = (b[12] & 0xF);
			

			
			bool alreadyAdded=false;
			for (int i=0; i < m_nit.satteliteNIT.size();++i)
			{
				NITSatDescriptor& nit=m_nit.satteliteNIT[i];
				if (nit.Frequency==satteliteNIT.Frequency)
				{
					LogDebug("Sat nit: frequency:%d Symbolrate:%d Polarisation:%d", 
						satteliteNIT.Frequency,satteliteNIT.Symbolrate,satteliteNIT.Polarisation);
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				m_nit.satteliteNIT.push_back(satteliteNIT);
			}
		}	

	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DVB_GetSatDelivSys()");
	}	
}
void CNITDecoder::DVB_GetTerrestrialDelivSys(byte*b , int maxLen)
{
	try
	{

		if(b[0]==0x5A)
		{
			int descriptor_tag = b[0];
			int descriptor_length = b[1];
			if (descriptor_length>11) 
			{
				LogDebug("DVB_GetTerrestrialDelivSys() desclen:%d", descriptor_length);
				return;
			}
			NITTerrestrialDescriptor terrestialNIT;
			terrestialNIT.CentreFrequency= (b[2]<<24)+(b[3]<<16)+(b[4]<<8)+b[5];
			if (terrestialNIT.CentreFrequency < 40000000 ||
				terrestialNIT.CentreFrequency >900000000) return; // invalid frequency

			terrestialNIT.Bandwidth = (b[6]>>5);
			// bandwith
			// 0- 8 MHz
			// 1- 7 MHz
			// 2- 6 MHz
			if (terrestialNIT.Bandwidth==0) terrestialNIT.Bandwidth=8;
			else if (terrestialNIT.Bandwidth==1) terrestialNIT.Bandwidth=7;
			else if (terrestialNIT.Bandwidth==2) terrestialNIT.Bandwidth=6;
			else terrestialNIT.Bandwidth=8;

			terrestialNIT.Constellation=(b[7]>>6);
			// constellation
			// 0- QPSK
			// 1- 16-QAM
			// 2- 64-QAM
			if (terrestialNIT.Constellation==0) terrestialNIT.Constellation=BDA_MOD_QPSK ;
			else if (terrestialNIT.Constellation==1) terrestialNIT.Constellation=BDA_MOD_16QAM ;
			else if (terrestialNIT.Constellation==2) terrestialNIT.Constellation=BDA_MOD_64QAM ;
			else  terrestialNIT.Constellation=BDA_MOD_NOT_SET;

			terrestialNIT.HierarchyInformation=(b[7]>>3)& 7;
			// 0- non-hierarchical
			// 1- a == 1
			// 2- a == 2
			// 3- a == 4
			switch (terrestialNIT.HierarchyInformation)
			{
				case 0:	terrestialNIT.HierarchyInformation=BDA_HALPHA_NOT_DEFINED ;break;
				case 1:	terrestialNIT.HierarchyInformation=BDA_HALPHA_1 ;break;
				case 2:	terrestialNIT.HierarchyInformation=BDA_HALPHA_2 ;break;
				case 3:	terrestialNIT.HierarchyInformation=BDA_HALPHA_4 ;break;
				default:terrestialNIT.HierarchyInformation=BDA_GUARD_NOT_SET; break;
			}
			terrestialNIT.CoderateHPStream=(b[7] & 7);
			terrestialNIT.CoderateLPStream=(b[8]>>5);
			// coderate (fec)
			// 0- 1/2
			// 1- 2/3
			// 2- 3/4
			// 3- 5/6
			// 4- 7/8
			// Coderate: The code_rate is a 3-bit field specifying the inner FEC scheme used according to table 43. Non-hierarchical
			// channel coding and modulation requires signalling of one code rate. In this case, 3 bits specifying code_rate according
			// to table 44 are followed by another 3 bits of value '000". Two different code rates may be applied to two different levels
			// of modulation with the aim of achieving hierarchy. Transmission then starts with the code rate for the HP level of the
			// modulation and ends with the one for the LP level.
			switch (terrestialNIT.CoderateHPStream)
			{
				case 0:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_1_2;break;
				case 1:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_2_3;break;
				case 2:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_3_4;break;
				case 3:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_5_6;break;
				case 4:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_7_8;break;
				default:terrestialNIT.CoderateHPStream=BDA_BCC_RATE_NOT_SET; break;
			}
			switch (terrestialNIT.CoderateLPStream)
			{
				case 0:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_1_2;break;
				case 1:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_2_3;break;
				case 2:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_3_4;break;
				case 3:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_5_6;break;
				case 4:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_7_8;break;
				default:terrestialNIT.CoderateLPStream=BDA_BCC_RATE_NOT_SET; break;
			}
			terrestialNIT.GuardInterval=(b[8]>>3) & 3;
			// 0 - 1/32
			// 1 - 1/16
			// 2 - 1/8
			// 3 - 1/4
			//
			switch (terrestialNIT.GuardInterval)
			{
				case 0: terrestialNIT.GuardInterval=BDA_GUARD_1_32;break;
				case 1: terrestialNIT.GuardInterval=BDA_GUARD_1_16;break;
				case 2: terrestialNIT.GuardInterval=BDA_GUARD_1_8;break;
				case 3: terrestialNIT.GuardInterval=BDA_GUARD_1_4;break;
				default:terrestialNIT.GuardInterval=BDA_GUARD_NOT_SET;break;
			}
			terrestialNIT.TransmissionMode=(b[8]>>1) & 3;
			// 0 - 2k Mode
			// 1 - 8k Mode
			if (terrestialNIT.TransmissionMode==0) terrestialNIT.TransmissionMode=BDA_XMIT_MODE_2K;
			else if (terrestialNIT.TransmissionMode==1) terrestialNIT.TransmissionMode=BDA_XMIT_MODE_8K;
			else terrestialNIT.TransmissionMode=BDA_XMIT_MODE_NOT_SET;
			
			terrestialNIT.OtherFrequencyFlag=(b[8] & 3);
			// 0 - no other frequency in use
			bool alreadyAdded=false;
			for (int i=0; i < m_nit.terrestialNIT.size();++i)
			{
				NITTerrestrialDescriptor& nit=m_nit.terrestialNIT[i];
				if (nit.CentreFrequency==terrestialNIT.CentreFrequency)
				{
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				LogDebug("NIT: terrestial frequency=%d bandwidth=%d other freqs:%d", terrestialNIT.CentreFrequency,terrestialNIT.Bandwidth,terrestialNIT.OtherFrequencyFlag);
				m_nit.terrestialNIT.push_back(terrestialNIT);
			}
		}

	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DVB_GetTerrestrialDelivSys()");
	}	
}		


void CNITDecoder::DVB_GetCableDelivSys(byte* b, int maxLen)
{
	try
	{
		if(b[0]==0x44 && maxLen>=13)
		{
			int descriptor_tag = b[0];
			int descriptor_length = b[1];
			if (descriptor_length>13) 
			{
				//LogDebug("DVB_GetCableDelivSys() desclen:%d", descriptor_length);
				return;
			}
			NITCableDescriptor cableNIT;
			cableNIT.Frequency = (10000000* ((b[2]>>4)&0xf));
			cableNIT.Frequency+= (1000000*  ((b[2]&0xf)));
			cableNIT.Frequency+= (100000*   ((b[3]>>4)&0xf));
			cableNIT.Frequency+= (10000*    ((b[3]&0xf)));
			cableNIT.Frequency+= (1000*     ((b[4]>>4)&0xf));
			cableNIT.Frequency+= (100*      ((b[4]&0xf)));
			cableNIT.Frequency+= ( 10*      ((b[5]>>4)&0xf));
			cableNIT.Frequency+= (b[5]&0xf);
			//
			cableNIT.FECOuter = (b[7] & 0xF);
			// fec-outer
			// 0- not defined
			// 1- no outer FEC coding
			// 2- RS(204/188)
			// other reserved
			switch (cableNIT.FECOuter)
			{
				case 0:cableNIT.FECOuter=BDA_FEC_METHOD_NOT_SET;break;
				case 1:cableNIT.FECOuter=BDA_FEC_METHOD_NOT_DEFINED;break;
				case 2:cableNIT.FECOuter=BDA_FEC_RS_204_188;break;
				default:cableNIT.FECOuter=BDA_FEC_METHOD_NOT_SET;break;
			}
			cableNIT.Modulation = b[8];
			// modulation
			// 0x00 not defined
			// 0x01 16-QAM
			// 0x02 32-QAM
			// 0x03 64-QAM
			// 0x04 128-QAM
			// 0x05 256-QAM
			switch(cableNIT.Modulation)
			{
				case 0: cableNIT.Modulation=BDA_MOD_NOT_DEFINED; break;
				case 1: cableNIT.Modulation=BDA_MOD_16QAM; break;
				case 2: cableNIT.Modulation=BDA_MOD_32QAM; break;
				case 3: cableNIT.Modulation=BDA_MOD_64QAM; break;
				case 4: cableNIT.Modulation=BDA_MOD_128QAM; break;
				case 5: cableNIT.Modulation=BDA_MOD_256QAM; break;
				default: cableNIT.Modulation=BDA_MOD_NOT_SET; break;
			}
			
			cableNIT.Symbolrate = (1000000* ((b[9]>>4)&0xf));
			cableNIT.Symbolrate+= (100000*  ((b[9]&0xf)));
			cableNIT.Symbolrate+= (10000*   ((b[10]>>4)&0xf));
			cableNIT.Symbolrate+= (1000*    ((b[10]&0xf)));
			cableNIT.Symbolrate+= (100*     ((b[11]>>4)&0xf));
			cableNIT.Symbolrate+= (10*      ((b[11]&0xf)));
			cableNIT.Symbolrate+= (         ((b[12]>>4)&0xf));
			
			

			cableNIT.FECInner = (b[12] & 0xF);
			// fec inner
			// 0- not defined
			// 1- 1/2 conv. code rate
			// 2- 2/3 conv. code rate
			// 3- 3/4 conv. code rate
			// 4- 5/6 conv. code rate
			// 5- 7/8 conv. code rate
			// 6- 8/9 conv. code rate
			// 15- No conv. coding
			switch (cableNIT.FECInner)
			{
				case 0:cableNIT.FECInner=BDA_BCC_RATE_NOT_DEFINED;break;
				case 1:cableNIT.FECInner=BDA_BCC_RATE_1_2;break;
				case 2:cableNIT.FECInner=BDA_BCC_RATE_2_3;break;
				case 3:cableNIT.FECInner=BDA_BCC_RATE_3_4;break;
				case 4:cableNIT.FECInner=BDA_BCC_RATE_5_6;break;
				case 5:cableNIT.FECInner=BDA_BCC_RATE_7_8;break;
				case 6:cableNIT.FECInner=BDA_BCC_RATE_NOT_DEFINED;break;
				case 15:cableNIT.FECInner=BDA_BCC_RATE_NOT_DEFINED;break;
				default:cableNIT.FECInner=BDA_BCC_RATE_NOT_SET;break;
			}
			bool alreadyAdded=false;
			for (int i=0; i < m_nit.cableNIT.size();++i)
			{
				NITCableDescriptor& nit=m_nit.cableNIT[i];
				if (nit.Frequency==cableNIT.Frequency)
				{
					alreadyAdded=true;
					break;
				}
			}
			if (!alreadyAdded)
			{
				m_nit.cableNIT.push_back(cableNIT);
				//LogDebug("NIT: network:%s", cableNIT.NetworkName);
				LogDebug("cable NIT: frequency:%d modulation:%d symbolrate:%d", cableNIT.Frequency, cableNIT.Modulation, cableNIT.Symbolrate);
			}
		}

	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DVB_GetCableDelivSys()");
	}	
}
