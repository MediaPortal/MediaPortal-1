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
#pragma warning(disable: 4786)
#pragma warning(disable : 4995)
#include <streams.h>
#include <bdatypes.h>
#include <time.h>
#include "epgdecoder.h"
//#include "crc.h"
#include "autostring.h"
#include "entercriticalsection.h"
#include "DN_EIT_Helper.h"
#include "..\..\shared\dvbutil.h"
#include "FreesatHuffmanTables.h"

extern void LogDebug(const char *fmt, ...) ;

#define S_FINISHED (S_OK+1)
#define PID_FREESAT_EPG 0xBBA
#define PID_FREESAT2_EPG 0xBBB

CEpgDecoder::CEpgDecoder()
{
  ResetEPG();
  m_bParseEPG=false;
  m_bEpgDone=false;
  m_bSorted=FALSE;
  m_epgTimeout=time(NULL);
}
CEpgDecoder::~CEpgDecoder()
{
}

HRESULT CEpgDecoder::DecodeEPG(byte* buf,int len,int PID)
{
	CEnterCriticalSection lock (m_critSection);
	try
	{
		if (!m_bParseEPG) 
      return E_FAIL;
		if (m_bEpgDone) 
      return E_FAIL;
		if (buf==NULL)
      return E_FAIL;

		time_t currentTime=time(NULL);
		time_t timespan=currentTime-m_epgTimeout;
		if (timespan>60)
		{
			m_bParseEPG=false;
			m_bEpgDone=true;
			return S_FINISHED;
		}
		if (len<=14) 
      return E_FAIL;

		int tableid = buf[0];
		//Dish Network also uses table ids from 0x80 to 0xfe. We don't need to check this here either because 
		//of the filter being set in EpgParser.cpp
		//if((tableid < 0x50 || tableid > 0x6f) && tableid != 0x4e && tableid != 0x4f) 
        //     return E_FAIL;
		int section_length = ((buf[1]& 0xF)<<8) + buf[2];
		int service_id = (buf[3]<<8)+buf[4];
		int version_number = (buf[5]>>1) & 0x1f;
		int current_next_indicator = buf[5]&1;
		int section_number=buf[6];
		int last_section_number=buf[7];
		int transport_id=(buf[8]<<8)+buf[9];
		int network_id=(buf[10]<<8)+buf[11];
		int segment_last_section_number=buf[12];
		int last_table_id=buf[13];

		unsigned long lNetworkId=network_id;
		unsigned long lTransport_id=transport_id;
		unsigned long lServiceId=service_id;
		unsigned long key=(unsigned long)(lNetworkId<<32UL);
		key+=(lTransport_id<<16);
		key+=lServiceId;
		imapEPG it=m_mapEPG.find(key);
		if (it==m_mapEPG.end())
		{
			EPGChannel newChannel ;
			newChannel.original_network_id=network_id;
			newChannel.service_id=service_id;
			newChannel.transport_id=transport_id;
			newChannel.allSectionsReceived=false;
			m_mapEPG[key]=newChannel;
			it=m_mapEPG.find(key);
		}	
		if (it==m_mapEPG.end()) 
			return E_FAIL;
		EPGChannel& channel=it->second; 

		//did we already receive this section ?
		key=crc32 ((char*)buf,len);
		EPGChannel::imapSectionsReceived itSec=channel.mapSectionsReceived.find(key);
		if (itSec!=channel.mapSectionsReceived.end())
			return S_FINISHED; //yes
		channel.mapSectionsReceived[key]=true;
		

		m_epgTimeout=time(NULL);
		int start=14;
		while (start+11 <= len+1)
		{
			unsigned int event_id=(buf[start]<<8)+buf[start+1];
			unsigned long dateMJD=(buf[start+2]<<8)+buf[start+3];
			unsigned long timeUTC=(buf[start+4]<<16)+(buf[start+5]<<8)+buf[6];
			unsigned long duration=(buf[start+7]<<16)+(buf[start+8]<<8)+buf[9];
			unsigned int running_status=buf[start+10]>>5;
			unsigned int free_CA_mode=(buf[start+10]>>4) & 0x1;
			int descriptors_len=((buf[start+10]&0xf)<<8) + buf[start+11];

			EPGChannel::imapEvents itEvent=channel.mapEvents.find(event_id);
			if (itEvent==channel.mapEvents.end())
			{
				EPGEvent newEvent;
				newEvent.eventid=event_id;
				newEvent.dateMJD=dateMJD;
				newEvent.timeUTC=timeUTC;
				newEvent.duration=duration;
				newEvent.running_status=running_status;
				newEvent.free_CA_mode=free_CA_mode;
				channel.mapEvents[event_id]=newEvent;
				itEvent=channel.mapEvents.find(event_id);
			}
			EPGEvent& epgEvent=itEvent->second;

			start=start+12;
			int off=0;
			while (off < descriptors_len)
			{
				if (start+off+1>len) 
          return  E_FAIL;
				int descriptor_tag = buf[start+off];
				int descriptor_len = buf[start+off+1];
				if (descriptor_len>0) 
				{
					if (start+off+descriptor_len+2>len) 
					{
						//LogDebug("epg:     DecodeEPG check1 %d %d %d %d",start,off,descriptor_len,len);
						return E_FAIL;
					}
					if (descriptor_tag ==0x4d)
					{
						//					LogDebug("epg:     short event descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeShortEventDescriptor(&buf[start+off],epgEvent,PID);
					}
					else if (descriptor_tag ==0x54)
					{
						//					LogDebug("epg:     genre descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeContentDescription( &buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x4e)
					{
						//					LogDebug("epg:     description descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeExtendedEvent(&buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x55)
					{
											//LogDebug("epg:     parental rating descriptor:0x%x len:%d start:%d %s",descriptor_tag,descriptor_len,start+off,&buf[start+off]);
						DecodeParentalRatingDescriptor(&buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x5f)
					{
						//					LogDebug("epg:     private data descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
					}
					else if (descriptor_tag==0x89)
					{
						DecodeCombinedStarRating_MPAARatingDescriptor(&buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x91)
					{
						//					LogDebug("epg:     dish network short description descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						int tnum=(tableid>0x80 ? 2 : 1);
						DecodeDishShortDescription( &buf[start+off],epgEvent,tnum);
					}
					else if (descriptor_tag ==0x92)
					{
						//					LogDebug("epg:     dish network long description descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						int tnum=(tableid>0x80 ? 2 : 1);
						DecodeDishLongDescription( &buf[start+off],epgEvent,tnum);
					}
					else
					{
						//LogDebug("epg:  len:%d   descriptor:0x%x len:%d start:%d",len,descriptor_tag,descriptor_len,start+off);
					}

				}
				off   +=descriptor_len+2;
			}
			start +=descriptors_len;
		}

	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodeEPG()");
	}	
	return S_OK;
}

void CEpgDecoder::DecodeCombinedStarRating_MPAARatingDescriptor(byte* data,EPGEvent &epgEvent)
{
	/*89: B2 05
	is the Movie rating as per http://www.mpaa.org
	first 3 bits are:
		0x1: *
		0x2: *+
		0x3: **
		0x4: **+
		0x5: ***
		0x6: ***+
		0x7: ****
	next 3 bits are:
		0x0: NA
		0x1: G
    	0x2: PG
		0x3: PG-13
		0x4: R
		0x5: NC-17
		0x6: R
		0x7: NR

	the next bits and bytes describe, but i haven't figured out the order yet:
				SC,L,N,V 
	http://dvbn.happysat.org/viewtopic.php?t=16912&highlight=parental+rating 
	
	According to this thread http://board.mytheatre.ru/viewtopic.php?t=1265&postdays=0&postorder=asc&highlight=star+rating&start=0
	the bits were wrong before */

	//int starRating=(int)data[2]&0x7;
	int starRating=((int)(data[2]&0xE0)>>5); // bits 13-15
	if (starRating>0 && starRating<8)
		epgEvent.starRating=starRating;
	//byte bPRating=data[3]&0x38;
	byte bPRating=((data[2]&0x1C)>>2);		// bits 10-12
	epgEvent.classification= "";
	switch (bPRating)
	{
		case 0:
			epgEvent.classification="NR/AO";
			break;
		case 1:
			epgEvent.classification="G";
			break;
		case 2:
			epgEvent.classification="PG";
			break;
		case 3:
			epgEvent.classification="PG-13";
			break;
		case 4:
			epgEvent.classification="R";
			break;
		case 5:
			epgEvent.classification="NR/AO";
			break;
		case 6:
			epgEvent.classification="";
			break;
		case 7:
			epgEvent.classification="NC-17";
			break;
	}
}

void CEpgDecoder::DecodeParentalRatingDescriptor(byte* data, EPGEvent& epgEvent)
{
	try
	{
		int descriptor_length=data[1];
		if (descriptor_length % 4!=0)
		{
			//LogDebug("*** epg:DecodeParentalRatingDescriptor check 1");
			return; // Invalid length. Must be always a multiply of 4
		}
		int len=0;
		int off=2;
		while (off+2<descriptor_length)
		{
			DWORD language=(data[off]<<16)+(data[off+1]<<8)+data[off+2];
			unsigned int rating=(unsigned int)data[off+3];
			if (rating>0xf)
			{
				// According to ETSI EN 300 468 the rating is
				// 0x00			undefined
				// 0x01-0x0F	minimum=rating+3 years
				// 0x10-0xFF	defined by the broadcaster
				//LogDebug("*** epg:DecodeParentalRatingDescriptor check 2");
			}
			else
			{
				rating=rating+3;
				bool langFound=false;
				EPGEvent::ivecLanguages it = epgEvent.vecLanguages.begin();
				for (it = epgEvent.vecLanguages.begin(); it != epgEvent.vecLanguages.end();++it)
				{
					EPGLanguage& lang=*it;
					if (lang.language==language)
					{
						//found.
						lang.parentalRating=rating;
						//LogDebug("epg grab parental:%s [%d]",langi, rating);
						langFound=true;
						break;
					}
				}
				if (!langFound)
				{
					EPGLanguage lang;
					lang.language=language;
					lang.parentalRating=rating;
					lang.text="";
					lang.event="";
					//LogDebug("epg grab parental: %s [%d]",langi, rating);
					epgEvent.vecLanguages.push_back(lang);
				}
			}
			off=off+4;
		}
	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodeParentalRatingDescriptor()");
	}	
}

HRESULT CEpgDecoder::DecodePremierePrivateEPG(byte* buf,int len)
{
	CEnterCriticalSection lock (m_critSection);
	try
	{
		if (!m_bParseEPG) 
      return E_FAIL;
		if (m_bEpgDone) 
      return E_FAIL;
		if (buf==NULL)
      return E_FAIL;

		time_t currentTime=time(NULL);
		time_t timespan=currentTime-m_epgTimeout;
		if (timespan>60)
		{
			m_bParseEPG=false;
			m_bEpgDone=true;
			return S_FINISHED;
		}
		if (len<=8) 
			return E_FAIL;


		int section_length = ((buf[1]& 0xF)<<8) + buf[2];
		int service_id = (buf[3]<<8)+buf[4];
		int version_number = (buf[5]>>1) & 0x1f;
		int current_next_indicator = buf[5]&1;
		int section_number=buf[6];
		int last_section_number=buf[7];

		//did we already receive this section ?
		unsigned long key=crc32 ((char*)buf,len);
		m_imapSectionsReceived itSec=m_mapSectionsReceived.find(key);
		if (itSec!=m_mapSectionsReceived.end())
			return S_FINISHED; //yes
		m_mapSectionsReceived[key]=true;

		m_epgTimeout=time(NULL);
	    EPGEvent epgEvent;
		int start=8;
		while (start+8 <= len)
		{
			unsigned int event_id=(buf[start]<<24) | (buf[start+1]<<16) | (buf[start+2]<< 8) |  buf[start+3];
			unsigned long duration=(buf[start+4]<<16)+(buf[start+5]<<8)+buf[6];
			int descriptors_len=((buf[start+7]&0xf)<<8) + buf[start+8];
			epgEvent.eventid=event_id;
			epgEvent.duration=duration;

			start=start+9;
			int off=0;
			while (off < descriptors_len)
			{
				if (start+off+1>len) 
					return  E_FAIL;
				int descriptor_tag = buf[start+off];
				int descriptor_len = buf[start+off+1];
				if (descriptor_len>0) 
				{
					if (start+off+descriptor_len+2>len) 
					{
						//LogDebug("epg:     DecodeEPG check1 %d %d %d %d",start,off,descriptor_len,len);
						return E_FAIL;
					}
					if (descriptor_tag ==0x4d)
					{
						//					LogDebug("epg:     short event descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeShortEventDescriptor( &buf[start+off],epgEvent,0);
					}
					else if (descriptor_tag ==0x54)
					{
						//					LogDebug("epg:     genre descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeContentDescription( &buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x4e)
					{
						//					LogDebug("epg:     description descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeExtendedEvent(&buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0xF0)
					{
						//					LogDebug("epg:  premiere content order   descriptor:0x%x len:%d start:%d %s",descriptor_tag,descriptor_len,start+off,&buf[start+off+2]);
					}
					else if (descriptor_tag ==0xF1)
					{
						//					LogDebug("epg:  premiere parental information   descriptor:0x%x len:%d start:%d %s",descriptor_tag,descriptor_len,start+off,&buf[start+off+2]);
					}
					else if (descriptor_tag ==0xF2)
					{
						//					LogDebug("epg:  premiere content transmission   descriptor:0x%x len:%d start:%d %s",descriptor_tag,descriptor_len,start+off,&buf[start+off+2]);
						DecodePremiereContentTransmissionDescriptor(&buf[start+off],epgEvent);
					}
					else
					{
						//					LogDebug("epg:     descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
					}

				}
				off   +=(descriptor_len+2);
			}
			start +=descriptors_len;
		}
	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodePremierePrivateEPG()");
	}	
	return S_OK;
}

void CEpgDecoder::DecodePremiereContentTransmissionDescriptor(byte* data, EPGEvent epgEvent)
{
	int tid=(data[2]<<8)+data[3];
	int nid=(data[4]<<8)+data[5];
	int sid=(data[6]<<8)+data[7];

	unsigned long lNetworkId=nid;
	unsigned long lTransport_id=tid;
	unsigned long lServiceId=sid;
	unsigned long key=(unsigned long)(lNetworkId<<32UL);
	key+=(lTransport_id<<16);
	key+=lServiceId;
	imapEPG it=m_mapEPG.find(key);
	if (it==m_mapEPG.end())
	{
		EPGChannel newChannel;
		newChannel.original_network_id=nid;
		newChannel.service_id=sid;
		newChannel.transport_id=tid;
		newChannel.allSectionsReceived=false;
		m_mapEPG[key]=newChannel;
		it=m_mapEPG.find(key);
	}
	EPGChannel& channel=it->second;
	byte *buf=data+8;
	while ((buf+6) <= (data+2+data[1]))
	{
		unsigned int starttime_no = *(buf+2);
        for (int i=0; i < starttime_no; i+=3)
        {
			unsigned long dateMJD=(buf[0]<<8)+buf[1];
            unsigned long timeUTC=(buf[3+i]<<16)+(buf[4+i]<<8)+data[5+i];
			epgEvent.dateMJD=dateMJD;
			epgEvent.timeUTC=timeUTC;
			//LogDebug("%d %d %s nid %d tid %d sid %d",dateMJD,timeUTC,epgEvent.vecLanguages[0].event.c_str(),nid,tid,sid);
			channel.mapEvents[m_pseudo_event_id]=epgEvent;
			m_pseudo_event_id++;
        }
        buf += 3 + starttime_no;
	}
	//LogDebug("epg: premiere content tid=%d nid=%d sid=%d dateMJD=%d timeUTC=%d",tid,nid,sid,dateMJD,timeUTC);
}

void CEpgDecoder::DecodeDishShortDescription(byte* data, EPGEvent& epgEvent, int tnum)
{
	try
	{
		unsigned char *decompressed=DishDecode::decompress(&data[3], data[1]-1,tnum);
		EPGEvent::ivecLanguages it = epgEvent.vecLanguages.begin();
		for (it = epgEvent.vecLanguages.begin(); it != epgEvent.vecLanguages.end();++it)
		{
			EPGLanguage& lang=*it;
			if (lang.language==langENG)
			{
				lang.event=(char*)decompressed;
				free(decompressed);
				return;
			}
		}
		EPGLanguage lang;
		lang.event=(char*)decompressed;
		free(decompressed);
		lang.parentalRating=0;
		// simulated lang id for "eng"
		lang.language=langENG;
		epgEvent.vecLanguages.push_back(lang);
		//LogDebug("DISH EPG ShortDescription=%s",text.c_str());
	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in DecodeDishShortDescription()");
	}	
}

void CEpgDecoder::DecodeDishLongDescription(byte* data, EPGEvent& epgEvent, int tnum)
{
	try
	{
		unsigned char* decompressed=NULL;
		if((data[3]&0xf8) == 0x80)
			decompressed=DishDecode::decompress(&data[4], data[1]-2,tnum);
		else
		    decompressed=DishDecode::decompress(&data[3], data[1]-1,tnum);
		EPGEvent::ivecLanguages it = epgEvent.vecLanguages.begin();
		for (it = epgEvent.vecLanguages.begin(); it != epgEvent.vecLanguages.end();++it)
		{
			EPGLanguage& lang=*it;
			if (lang.language==langENG)
			{
				lang.text=(char*)decompressed;
				free(decompressed);
				return;
			}
		}
		EPGLanguage lang;
		lang.text=(char*)decompressed;
		free(decompressed);
		lang.language=langENG;
		lang.parentalRating=0;
		epgEvent.vecLanguages.push_back(lang);
	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodeDishLongDescription()");
	}	
}

void CEpgDecoder::DecodeExtendedEvent(byte* data, EPGEvent& epgEvent)
{
	try
	{
		int descriptor_tag;
		int descriptor_length;
		int descriptor_number;
		int last_descriptor_number;
		int text_length;
		int length_of_items;

		string text = "";
		string item = "";
		int pointer = 0;
		int lenB;
		int len1;
		int item_description_length;
		int item_length;

		descriptor_tag = data[0];
		descriptor_length = data[1];
		descriptor_number = (data[2]>>4) & 0xF;
		last_descriptor_number = data[2] & 0xF;
		DWORD language=(data[3]<<16)+(data[4]<<8)+data[5];
		length_of_items = data[6];
		pointer = 7;
		lenB = descriptor_length - 5;
		len1 = length_of_items;

		while (len1 > 0)
		{
			item_description_length = data[pointer];
			if (item_description_length< 0 || pointer+item_description_length > descriptor_length+2) 
			{
				LogDebug("*** epg:DecodeExtendedEvent check 1");
				return;
			}

			pointer += (1 + item_description_length);

			item_length = data[pointer];
			if (item_length < 0 || pointer+item_length>descriptor_length+2) 
			{
				LogDebug("*** epg:DecodeExtendedEvent check 2");
				return;
			}

			if (item_length>0)
			{

				CAutoString buffer2 (item_length*4);
				getString468A(&data[pointer+1], item_length, buffer2.GetBuffer(), item_length*4);
				item = buffer2.GetBuffer();
			}

			pointer += (1 + item_length);
			len1 -= (2 + item_description_length + item_length);
			lenB -= (2 + item_description_length + item_length);
		};

		pointer=7+length_of_items;
		text_length = data[pointer];
		pointer += 1;
		if (text_length< 0 || pointer+text_length>descriptor_length+2) 
		{
			LogDebug("*** epg:DecodeExtendedEvent check 3");
			return;
		}

		if (text_length>0)
		{
			CAutoString buffer (text_length*4);
			getString468A(&data[pointer], text_length, buffer.GetBuffer(), text_length*4);
			text = buffer.GetBuffer();
		}

		//find language...
		EPGEvent::ivecLanguages it = epgEvent.vecLanguages.begin();
		for (it = epgEvent.vecLanguages.begin(); it != epgEvent.vecLanguages.end();++it)
		{
			EPGLanguage& lang=*it;
			if (lang.language==language)
			{
				//found.
				if (item.size()>0 && lang.event.size()<1)
					lang.event+=item;
				if (text.size()>0 && strcmp(item.c_str(),text.c_str())!=0)
				{
					if (lang.text.size()>0)
					{
						if ((BYTE)text[0]<0x20)
						{
							if (!lang.CR_added)
							{
								lang.text+="\n";
								lang.CR_added=true;
							}
							if (text.size()>1)
								lang.text+=text.erase(0,1);
						}
						else
							lang.text+=text;
					}
					else
						lang.text=text;
				}
							//LogDebug("epg grab ext:[%s][%s]", lang.event.c_str(),lang.text.c_str());
				return;
			}
		}
		//add new language...
		EPGLanguage lang;
		lang.CR_added=false;
		lang.language=language;
		if (item.size()>0)
			lang.event=item;
		if (text.size()>0 && strcmp(item.c_str(),text.c_str())!=0)
			lang.text=text;
		lang.parentalRating=0;
			//LogDebug("epg grab ext:[%s][%s]", lang.event.c_str(),lang.text.c_str());
		epgEvent.vecLanguages.push_back(lang);

	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodeExtendedEvent()");
	}	
}

void CEpgDecoder::DecodeShortEventDescriptor(byte* buf, EPGEvent& epgEvent,int PID)
{
	try
	{
		int descriptor_tag = buf[0];
		int descriptor_len = buf[1];
		if(descriptor_tag!=0x4d) 
		{
			LogDebug("*** DecodeShortEventDescriptor: tag !=0x4d");
			return;
		}
		if (descriptor_len<5) 
		{
			LogDebug("*** DecodeShortEventDescriptor: len <4");
			return;
		}
    
		unsigned long ISO_639_language_code=(buf[2]<<16)+(buf[3]<<8)+buf[4];

		//LogDebug("epg grab short:[%s][%s]", lang.event.c_str(),lang.text.c_str());
		if (descriptor_len<6) 
		{
			//LogDebug("*** DecodeShortEventDescriptor: len <6");
		  EPGLanguage lang;
		  lang.language=ISO_639_language_code;
		  lang.event="";
		  lang.text="";
		  epgEvent.vecLanguages.push_back(lang);
			return;
		}

		int event_len = buf[5];
		string eventText="";
		string eventDescription="";
		//LogDebug("DecodeShortEventDescriptor: Lang:0x%x eventlen:0x%x",ISO_639_language_code,event_len);
		if (event_len >0)
		{
			if (6+event_len > descriptor_len+2)
			{
		    EPGLanguage lang;
				lang.CR_added=false;
		    lang.language=ISO_639_language_code;
		    lang.event="";
		    lang.text="";
				lang.parentalRating=0;
		    epgEvent.vecLanguages.push_back(lang);
				LogDebug("*** DecodeShortEventDescriptor: check1: %d %d",event_len,descriptor_len);
				return;
			}

			if(buf[6]==0x1f && (PID==PID_FREESAT_EPG || PID==PID_FREESAT2_EPG))
			{
				eventText=FreesatHuffmanToString(&buf[6],event_len);
			}
			else
			{
				CAutoString buffer(event_len+10);
				getString468A(&buf[6],event_len,buffer.GetBuffer(), event_len*4);
				eventText=buffer.GetBuffer();
			}
			//		LogDebug("  event:%s",eventText.c_str());
		}
		else if (event_len<0)
		{
	    EPGLanguage lang;
			lang.CR_added=false;
	    lang.language=ISO_639_language_code;
	    lang.event="";
	    lang.text="";
		lang.parentalRating=0;
	    epgEvent.vecLanguages.push_back(lang);
			LogDebug("*** DecodeShortEventDescriptor: check1a: %d %d",event_len,descriptor_len);
			return;
		}
		int off=6+event_len;
		int text_len = buf[off];
		//LogDebug("DecodeShortEventDescriptor: text_len:0x%x",text_len);
		if (text_len >0)
		{
			if (off+text_len > descriptor_len+2) 
			{
	      EPGLanguage lang;
				lang.CR_added=false;
	      lang.language=ISO_639_language_code;
	      lang.event="";
	      lang.text="";
				lang.parentalRating=0;
	      epgEvent.vecLanguages.push_back(lang);
				LogDebug("*** DecodeShortEventDescriptor: check2: %d %d",event_len,descriptor_len);
				return;
			}
			// Check if huffman encoded.
			if(buf[off+1]==0x1f && (PID==PID_FREESAT_EPG || PID==PID_FREESAT2_EPG))
			{
				eventDescription=FreesatHuffmanToString(&buf[off+1],text_len);
			}
			else
			{
				CAutoString buffer (text_len+10);
			  getString468A(&buf[off+1],text_len,buffer.GetBuffer(), text_len*4);
				eventDescription=buffer.GetBuffer();
			}

			//		LogDebug("  text:%s",eventDescription.c_str() );
		}
		else if (text_len<0)
		{
	    EPGLanguage lang;
			lang.CR_added=false;
	    lang.language=ISO_639_language_code;
	    lang.event="";
	    lang.text="";
			lang.parentalRating=0;
	    epgEvent.vecLanguages.push_back(lang);
			LogDebug("*** DecodeShortEventDescriptor: check2a: %d %d",event_len,descriptor_len);
			return;
		}
		EPGEvent::ivecLanguages it;
		for (it=epgEvent.vecLanguages.begin(); it != epgEvent.vecLanguages.end();++it)
		{
			EPGLanguage& lang = *it;
			if (lang.language==ISO_639_language_code)
			{
				if (eventText.size()>0)
					lang.event=eventText;
				if (eventDescription.size()>0)
					lang.text=eventDescription;
				//LogDebug("epg grab short:[%s][%s]", lang.event.c_str(),lang.text.c_str());
				return;
			}
		}
		EPGLanguage lang;
		lang.CR_added=false;
		lang.language=ISO_639_language_code;
		if (eventText.size()>0)
			lang.event=eventText;
		if (eventDescription.size()>0)
			lang.text=eventDescription;
		lang.parentalRating=0;
		//LogDebug("epg grab short:[%s][%s]", lang.event.c_str(),lang.text.c_str());
		epgEvent.vecLanguages.push_back(lang);

	}
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodeShortEventDescriptor()");
	}	
}

void CEpgDecoder::DecodeContentDescription(byte* buf,EPGEvent& epgEvent)
{
	try
	{
		int      descriptor_tag;
		int      descriptor_length;		
		int      content_nibble_level_1;
		int      content_nibble_level_2;
		int      user_nibble_1;
		int      user_nibble_2;
		int nibble=0;
		char genreText[1024];
		int           len;

		strcpy(genreText,"");
		descriptor_tag		 = buf[0];
		descriptor_length    = buf[1];
		if(descriptor_tag!=0x54) 
		{
			LogDebug("*** epg:DecodeContentDescription tag!=0x54");
			return;
		}

		len = descriptor_length;
		int pointer=  2;
		while ( len > 0) 
		{
			if (pointer+1>descriptor_length+2) 
			{
				LogDebug("*** epg:DecodeContentDescription check1");
				return;
			}

			content_nibble_level_1	 = (buf[pointer+0]>>4) & 0xF;
			content_nibble_level_2	 = buf[pointer+0] & 0xF;
			user_nibble_1		 = (buf[pointer+1]>>4) & 0xF;
			user_nibble_2		 = buf[pointer+1] & 0xF;

			pointer   += 2;
			len -= 2;
			strcpy(genreText,"");
			nibble=(content_nibble_level_1 << 8) | content_nibble_level_2;
			switch(nibble)
			{
				case 0x0100: strcpy(genreText,"movie/drama (general)" );break;
				case 0x0101: strcpy(genreText,"detective/thriller" );break;
				case 0x0102: strcpy(genreText,"adventure/western/war" );break;
				case 0x0103: strcpy(genreText,"science fiction/fantasy/horror" );break;
				case 0x0104: strcpy(genreText,"comedy" );break;
				case 0x0105: strcpy(genreText,"soap/melodram/folkloric" );break;
				case 0x0106: strcpy(genreText,"romance" );break;
				case 0x0107: strcpy(genreText,"serious/classical/religious/historical movie/drama" );break;
				case 0x0108: strcpy(genreText,"adult movie/drama" );break;

				case 0x010E: strcpy(genreText,"reserved" );break;
				case 0x010F: strcpy(genreText,"user defined" );break;

					// News Current Affairs
				case 0x0200: strcpy(genreText,"news/current affairs (general)" );break;
				case 0x0201: strcpy(genreText,"news/weather report" );break;
				case 0x0202: strcpy(genreText,"news magazine" );break;
				case 0x0203: strcpy(genreText,"documentary" );break;
				case 0x0204: strcpy(genreText,"discussion/interview/debate" );break;
				case 0x020E: strcpy(genreText,"reserved" );break;
				case 0x020F: strcpy(genreText,"user defined" );break;

					// Show Games show
				case 0x0300: strcpy(genreText,"show/game show (general)" );break;
				case 0x0301: strcpy(genreText,"game show/quiz/contest" );break;
				case 0x0302: strcpy(genreText,"variety show" );break;
				case 0x0303: strcpy(genreText,"talk show" );break;
				case 0x030E: strcpy(genreText,"reserved" );break;
				case 0x030F: strcpy(genreText,"user defined" );break;

					// Sports
				case 0x0400: strcpy(genreText,"sports (general)" );break;
				case 0x0401: strcpy(genreText,"special events" );break;
				case 0x0402: strcpy(genreText,"sports magazine" );break;
				case 0x0403: strcpy(genreText,"football/soccer" );break;
				case 0x0404: strcpy(genreText,"tennis/squash" );break;
				case 0x0405: strcpy(genreText,"team sports" );break;
				case 0x0406: strcpy(genreText,"athletics" );break;
				case 0x0407: strcpy(genreText,"motor sport" );break;
				case 0x0408: strcpy(genreText,"water sport" );break;
				case 0x0409: strcpy(genreText,"winter sport" );break;
				case 0x040A: strcpy(genreText,"equestrian" );break;
				case 0x040B: strcpy(genreText,"martial sports" );break;
				case 0x040E: strcpy(genreText,"reserved" );break;
				case 0x040F: strcpy(genreText,"user defined" );break;

					// Children/Youth
				case 0x0500: strcpy(genreText,"childrens's/youth program (general)" );break;
				case 0x0501: strcpy(genreText,"pre-school children's program" );break;
				case 0x0502: strcpy(genreText,"entertainment (6-14 year old)" );break;
				case 0x0503: strcpy(genreText,"entertainment (10-16 year old)" );break;
				case 0x0504: strcpy(genreText,"information/education/school program" );break;
				case 0x0505: strcpy(genreText,"cartoon/puppets" );break;
				case 0x050E: strcpy(genreText,"reserved" );break;
				case 0x050F: strcpy(genreText,"user defined" );break;

				case 0x0600: strcpy(genreText,"music/ballet/dance (general)" );break;
				case 0x0601: strcpy(genreText,"rock/pop" );break;
				case 0x0602: strcpy(genreText,"serious music/classic music" );break;
				case 0x0603: strcpy(genreText,"folk/traditional music" );break;
				case 0x0604: strcpy(genreText,"jazz" );break;
				case 0x0605: strcpy(genreText,"musical/opera" );break;
				case 0x0606: strcpy(genreText,"ballet" );break;
				case 0x060E: strcpy(genreText,"reserved" );break;
				case 0x060F: strcpy(genreText,"user defined" );break;

				case 0x0700: strcpy(genreText,"arts/culture (without music, general)" );break;
				case 0x0701: strcpy(genreText,"performing arts" );break;
				case 0x0702: strcpy(genreText,"fine arts" );break;
				case 0x0703: strcpy(genreText,"religion" );break;
				case 0x0704: strcpy(genreText,"popular culture/traditional arts" );break;
				case 0x0705: strcpy(genreText,"literature" );break;
				case 0x0706: strcpy(genreText,"film/cinema" );break;
				case 0x0707: strcpy(genreText,"experimental film/video" );break;
				case 0x0708: strcpy(genreText,"broadcasting/press" );break;
				case 0x0709: strcpy(genreText,"new media" );break;
				case 0x070A: strcpy(genreText,"arts/culture magazine" );break;
				case 0x070B: strcpy(genreText,"fashion" );break;
				case 0x070E: strcpy(genreText,"reserved" );break;
				case 0x070F: strcpy(genreText,"user defined" );break;

				case 0x0800: strcpy(genreText,"social/political issues/economics (general)" );break;
				case 0x0801: strcpy(genreText,"magazines/reports/documentary" );break;
				case 0x0802: strcpy(genreText,"economics/social advisory" );break;
				case 0x0803: strcpy(genreText,"remarkable people" );break;
				case 0x080E: strcpy(genreText,"reserved" );break;
				case 0x080F: strcpy(genreText,"user defined" );break;

				case 0x0900: strcpy(genreText,"education/science/factual topics (general)" );break;
				case 0x0901: strcpy(genreText,"nature/animals/environment" );break;
				case 0x0902: strcpy(genreText,"technology/natural science" );break;
				case 0x0903: strcpy(genreText,"medicine/physiology/psychology" );break;
				case 0x0904: strcpy(genreText,"foreign countries/expeditions" );break;
				case 0x0905: strcpy(genreText,"social/spiritual science" );break;
				case 0x0906: strcpy(genreText,"further education" );break;
				case 0x0907: strcpy(genreText,"languages" );break;
				case 0x090E: strcpy(genreText,"reserved" );break;
				case 0x090F: strcpy(genreText,"user defined" );break;
				case 0x0A00: strcpy(genreText,"leisure hobbies (general)" );break;
				case 0x0A01: strcpy(genreText,"tourism/travel" );break;
				case 0x0A02: strcpy(genreText,"handicraft" );break;
				case 0x0A03: strcpy(genreText,"motoring" );break;
				case 0x0A04: strcpy(genreText,"fitness & health" );break;
				case 0x0A05: strcpy(genreText,"cooking" );break;
				case 0x0A06: strcpy(genreText,"advertisement/shopping" );break;
				case 0x0A07: strcpy(genreText,"gardening" );break;
				case 0x0A0E: strcpy(genreText,"reserved" );break;
				case 0x0A0F: strcpy(genreText,"user defined" );break;

				case 0x0B00: strcpy(genreText,"original language" );break;
				case 0x0B01: strcpy(genreText,"black & white" );break;
				case 0x0B02: strcpy(genreText,"unpublished" );break;
				case 0x0B03: strcpy(genreText,"live broadcast" );break;
				case 0x0B0E: strcpy(genreText,"reserved" );break;
				case 0x0B0F: strcpy(genreText,"user defined" );break;

				case 0x0E0F: strcpy(genreText,"reserved" );break;
				case 0x0F0F: strcpy(genreText,"user defined" );break;					
			}
			//LogDebug("genre:%s", genreText);
			if (epgEvent.genre.size()==0 && strlen(genreText)>0)
				epgEvent.genre=genreText;
		}
	}

	
	catch(...)
	{
		LogDebug("mpsaa: unhandled exception in Sections::DecodeContentDescription()");
	}	
}

string CEpgDecoder::FreesatHuffmanToString(BYTE *src, int size)
{
  string uncompressed;
  int j,k;
  unsigned char *data;
  int uncompressed_size = 0x102;
  int u;
  int bit;
  short offset;
  unsigned short *base;
  unsigned char *next_node;
  unsigned char node;
  unsigned char prevc;
  unsigned char nextc;

  if (src[1] == 1 || src[1] == 2) 
  {
    if (src[1] == 1) 
    {
      data = raw_huffman_data1;
    }
    else 
    {
      data = raw_huffman_data2;
    }
    src += 2;
    j = 0;
    u = 0;
    prevc = START;
    do
    {
      offset = bitrev16(((unsigned short *)data)[prevc]);
      base = (unsigned short *)&data[offset];
      node = 0;
      do
      {
        bit = (src[j>>3] >> (7-(j&7))) & 1;
        j++;
        next_node = (unsigned char *)&base[node];
        node = next_node[bit];
      }
      while ((next_node[bit] & 0x80) == 0);
      nextc = next_node[bit] ^ 0x80;
      if (nextc == 0x1b)
      {
        do
        {
          nextc = 0;
          for (k=0; k<8; k++)
          {
            bit = (src[j>>3] >> (7-(j&7))) & 1;
            nextc = (nextc <<1) | bit;
            j++;
          }
          if (u >= uncompressed_size)
          {
            return 0;
          }
          uncompressed.append(1,nextc);
        }
        while (nextc & 0x80);
      }
      else
      {
        if (u >= uncompressed_size)
        {
          LogDebug("need realloc, uncompressed_size=%d", uncompressed_size);
          return uncompressed;
        }
        uncompressed.append(1,nextc);
      }
      prevc = nextc;
    }
    while(nextc != STOP);
    prevc = nextc;
    uncompressed.append(1,'\0');
    return uncompressed;
  }
  else
  {
    LogDebug("bad huffman table, %d, only support for 1, 2", src[0]);
    return uncompressed;
  }
  return uncompressed;
}

void CEpgDecoder::ResetEPG()
{
	CEnterCriticalSection lock (m_critSection);
	LogDebug("epg:ResetEPG()");
	EPGChannel ch;
	EPGEvent evt;

	m_prevChannel=ch;
	m_prevChannelIndex=-1;
	m_prevEvent = evt;
	m_prevEventIndex=-1;

	m_mapEPG.clear();
	//m_bParseEPG=false;
	m_bEpgDone=false;
    m_bSorted=false;
	m_epgTimeout=time(NULL);
}

void CEpgDecoder::GrabEPG()
{
	CEnterCriticalSection lock (m_critSection);
	LogDebug("epg:GrabEPG()");
	EPGChannel ch;
	EPGEvent evt;

	m_prevChannel=ch;
	m_prevChannelIndex=-1;
	m_prevEvent = evt;
	m_prevEventIndex=-1;

	m_mapEPG.clear();
	m_bParseEPG=true;
	m_bEpgDone=false;
  m_bSorted=false;
    m_pseudo_event_id=0;
	m_epgTimeout=time(NULL);
}
bool CEpgDecoder::IsEPGGrabbing()
{
	return m_bParseEPG;
}
bool CEpgDecoder::IsEPGReady()
{
	return m_bEpgDone;
}
void CEpgDecoder::Sort()
{
  if (m_bSorted) return;
  map<DWORD,EPGEvent>::iterator itEvent;
  for (imapEPG it =m_mapEPG.begin(); it != m_mapEPG.end();++it)
  {
    EPGChannel& epgChannel =it->second;
    epgChannel.m_sortedEvents.clear();
    for ( itEvent=epgChannel.mapEvents.begin(); itEvent !=epgChannel.mapEvents.end();++itEvent)
    {
      epgChannel.m_sortedEvents.push_back(itEvent->second);
    }
    epgChannel.m_sortedEvents.sort();
  }
  m_bSorted=true;
}
ULONG CEpgDecoder::GetEPGChannelCount( )
{
	CEnterCriticalSection lock (m_critSection);
  Sort();
	return (ULONG)m_mapEPG.size();
}
bool CEpgDecoder::GetChannelByindex(ULONG channelIndex, EPGChannel& epgChannel)
{
	CEnterCriticalSection lock (m_critSection);
  Sort();
	EPGEvent evt;
	m_prevEventIndex=-1;
	m_prevEvent=evt;

	if (channelIndex>=m_mapEPG.size()) 
		return false;
	ULONG count=0;
	imapEPG it =m_mapEPG.begin();
	while (count < channelIndex) 
	{ 
		it++; 
		count++;
	}
	epgChannel=it->second;
	m_prevChannel=epgChannel;
	m_prevChannelIndex=channelIndex;

	return true;
}

ULONG  CEpgDecoder::GetEPGEventCount( ULONG channel)
{
	CEnterCriticalSection lock (m_critSection);
  Sort();
	EPGChannel epgChannel;
	if (!GetChannelByindex(channel,epgChannel)) return 0;
	return (ULONG)epgChannel.m_sortedEvents.size();
}
void CEpgDecoder::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	
	CEnterCriticalSection lock (m_critSection);
  Sort();
	*networkId=0;
	*transportid=0;
	*service_id=0;
	if (channel!=m_prevChannelIndex)
	{
		EPGChannel epgChannel;
		if (!GetChannelByindex(channel,epgChannel)) return ;
		*networkId=epgChannel.original_network_id;
		*transportid=epgChannel.transport_id;
		*service_id=epgChannel.service_id;
	}
	else
	{
		*networkId=m_prevChannel.original_network_id;
		*transportid=m_prevChannel.transport_id;
		*service_id=m_prevChannel.service_id;
	}
}
void CEpgDecoder::GetEPGEvent( ULONG channel,  ULONG eventIndex,ULONG* languageCount, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre ,int* starRating, char** classification, unsigned int* eventid   )
{
	CEnterCriticalSection lock (m_critSection);
  Sort();
	*languageCount=0;
	*dateMJD=0;
	*timeUTC=0;
	*duration=0;
  *eventid=0;
	*genre = (char*)"";
	if (channel!=m_prevChannelIndex)
	{
		EPGChannel epgChannel;
		if (!GetChannelByindex(channel,epgChannel)) return ;
	}
	if (eventIndex >= m_prevChannel.m_sortedEvents.size()) return;

	ULONG count=0;
	EPGChannel::ilistEvents itEvent=m_prevChannel.m_sortedEvents.begin();
	while (count < eventIndex) 
	{ 
		itEvent++; 
		count++;
	}
	EPGEvent& epgEvent=*itEvent;
	m_prevEventIndex=eventIndex;
	m_prevEvent=epgEvent;

	*eventid=epgEvent.eventid;
	*languageCount=(ULONG)epgEvent.vecLanguages.size();
	*dateMJD=epgEvent.dateMJD;
	*timeUTC=epgEvent.timeUTC;
	*duration=epgEvent.duration;
	*genre=(char*)epgEvent.genre.c_str() ;
	*starRating=epgEvent.starRating;
	*classification=(char*)epgEvent.classification.c_str();
}
void CEpgDecoder::GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription, unsigned int* parentalRating  )
{
	CEnterCriticalSection lock (m_critSection);
	*language=0;
	*eventText=(char*)"";
	*eventDescription=(char*)"";
	if (channel!=m_prevChannelIndex)
	{
		EPGChannel epgChannel;
		if (!GetChannelByindex(channel,epgChannel)) return ;
	}

	if (m_prevEventIndex!=eventid)
	{
		if (eventid >= m_prevChannel.m_sortedEvents.size()) return;
		ULONG count=0;
		EPGChannel::ilistEvents itEvent=m_prevChannel.m_sortedEvents.begin();
		while (count < eventid) 
		{ 
			itEvent++; 
			count++;
		}
		EPGEvent& epgEvent=*itEvent;
		m_prevEventIndex=eventid;
		m_prevEvent=epgEvent;
	}
	
	//LogDebug("Lang %d %d", languageIndex, m_prevEvent.vecLanguages.size());
	if (languageIndex >=0 && languageIndex < m_prevEvent.vecLanguages.size())
	{
		EPGLanguage& lang= m_prevEvent.vecLanguages[languageIndex];
		*eventText=(char*)lang.event.c_str();
		*eventDescription=(char*)lang.text.c_str();
		*language=lang.language;
		*parentalRating=lang.parentalRating;
		//LogDebug("epg:get->[%s][%s]", lang.event.c_str(),lang.text.c_str());
	}
}

void CEpgDecoder::AbortGrabbing()
{
	CEnterCriticalSection lock (m_critSection);
	m_bParseEPG=false;
	m_bEpgDone=true;
}

