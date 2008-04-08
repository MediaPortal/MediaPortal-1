#pragma warning(disable: 4786)
#include <streams.h>
#include <bdatypes.h>
#include <time.h>
#include "epgparser.h"
#include "mpsa.h"
#include "crc.h"
#include "autostring.h"
#include "entercriticalsection.h"

extern void Log(const char *fmt, ...) ;
extern void Dump(const char *fmt, ...) ;

#define S_FINISHED (S_OK+1)
CEPGParser::CEPGParser()
{
  ResetEPG();
	m_bParseEPG=false;
	m_bEpgDone=false;

	m_epgTimeout=time(NULL)+60;
}
CEPGParser::~CEPGParser()
{
}

HRESULT CEPGParser::DecodeEPG(byte* buf,int len)
{
	CEnterCriticalSection lock (m_critSection);
	try
	{
		//30-08-2005 19:54:38 DecodeEPG() check section 0 (50 f0 0f 2e e3 c9 d32ff20)
		//Log("DecodeEPG():%d",len);
		if (!m_bParseEPG) return E_FAIL;
		if (m_bEpgDone) return E_FAIL;
		if (buf==NULL) return E_FAIL;

		time_t currentTime=time(NULL);
		time_t timespan=currentTime-m_epgTimeout;
		if (timespan>10)
		{
			//Log("EPG:timeout ch:%d",m_mapEPG.size());
			m_bParseEPG=false;
			m_bEpgDone=true;
			return S_FINISHED;
		}
		if (len<=14) return E_FAIL;
		int tableid = buf[0];

		if((tableid < 0x50 || tableid > 0x6f) && tableid != 0x4e && tableid != 0x4f) return E_FAIL;
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



		unsigned long key=(network_id<<32)+(transport_id<<16)+service_id;
		//Log("DecodeEPG():key %x",key);
		imapEPG it=m_mapEPG.find(key);
		if (it==m_mapEPG.end())
		{
			//Log("DecodeEPG():new channel");
			EPGChannel newChannel ;
			newChannel.original_network_id=network_id;
			newChannel.service_id=service_id;
			newChannel.transport_id=transport_id;
			newChannel.allSectionsReceived=false;
			m_mapEPG[key]=newChannel;
			it=m_mapEPG.find(key);
			//Log("epg:add new channel table:0x%x onid:0x%x tsid:0x%x sid:0x%x",tableid,network_id,transport_id,service_id);
		}
		if (it==m_mapEPG.end()) return E_FAIL;
		EPGChannel& channel=it->second; 

		//did we already receive this section ?
		key=(section_number);
		//Log("DecodeEPG() check section %x (%02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x %02.2x)",
		//									key, buf[0],buf[1],buf[2],buf[3],buf[4],buf[5],buf[6],buf[7],buf[8],buf[9],buf[10],buf[11],buf[12],buf[13]);
		EPGChannel::imapSectionsReceived itSec=channel.mapSectionsReceived.find(key);
		if (itSec!=channel.mapSectionsReceived.end()) return S_FINISHED; //yes
		channel.mapSectionsReceived[key]=true;

		//	Log("epg: tid:0x%x len:%d %d (%d/%d) sid:0x%x tsid:0x%x onid:0x%x slsn:%d last table id:0x%x cn:%d version:%d", 
		//		buf[0],len,section_length,section_number,last_section_number, 
		//		service_id,transport_id,network_id,segment_last_section_number,last_table_id,
		//		current_next_indicator,version_number);

		m_epgTimeout=time(NULL);
		int start=14;
		while (start+11 <= len)
		{
			//Log("epg:   %d/%d", start,len);
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
			//		Log("epg:    onid:0x%x tsid:0x%x sid:0x%x event:0x%x date:0x%x time:0x%x duration:0x%x running:%d free:%d start:%d desclen:%d",network_id,transport_id,service_id, event_id,dateMJD,timeUTC,duration,running_status,free_CA_mode,start,descriptors_len);
			while (off < descriptors_len)
			{
				if (start+off+1>len) return  E_FAIL;
				int descriptor_tag = buf[start+off];
				int descriptor_len = buf[start+off+1];
				if (descriptor_len>0) 
				{
					if (start+off+descriptor_len+2>len) 
					{
						//Log("epg:     DecodeEPG check1 %d %d %d %d",start,off,descriptor_len,len);
						return E_FAIL;
					}
					if (descriptor_tag ==0x4d)
					{
						//					Log("epg:     short event descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeShortEventDescriptor( &buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x54)
					{
						//					Log("epg:     genre descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeContentDescription( &buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x4e)
					{
						//					Log("epg:     description descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
						DecodeExtendedEvent(&buf[start+off],epgEvent);
					}
					else if (descriptor_tag ==0x55)
					{
						//					Log("epg:     parental rating descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
					}
					else if (descriptor_tag ==0x5f)
					{
						//					Log("epg:     private data descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
					}
					else
					{
						//					Log("epg:     descriptor:0x%x len:%d start:%d",descriptor_tag,descriptor_len,start+off);
					}

				}
				off   +=(descriptor_len+2);
			}
			start +=descriptors_len;
		}

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodeEPG()");
	}	
	return S_OK;
}

void CEPGParser::DecodeExtendedEvent(byte* data, EPGEvent& epgEvent)
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

		//	Log("grabext: tag:%x len:%d lang:%x lengthofitems:%x %d/%d", 
		//		descriptor_tag,descriptor_length,language,length_of_items,descriptor_number,last_descriptor_number);
		while (len1 > 0)
		{
			item_description_length = data[pointer];
			if (item_description_length< 0 || pointer+item_description_length > descriptor_length+2) 
			{
				Log("*** epg:DecodeExtendedEvent check 1");
				return;
			}
			//		Log("  1: %d item_description_length:[%d]",pointer,data[pointer]);
			/*
			if (item_description_length>0)
			{
			CAutoString buffer (item_description_length+10);
			Sections::getString468A(&data[pointer+1], item_description_length,buffer.GetBuffer());
			string testText=buffer.GetBuffer();
			if (testText.size()==0)
			testText="-not avail.-";
			}
			*/

			pointer += (1 + item_description_length);

			//		Log("  1: %d [%s]",pointer,testText.c_str() );

			item_length = data[pointer];
			if (item_length < 0 || pointer+item_length>descriptor_length+2) 
			{
				Log("*** epg:DecodeExtendedEvent check 2");
				return;
			}

			if (item_length>0)
			{
				//		Log("  1: %d item_length:[%d]",pointer,item_length);

				CAutoString buffer2 (item_length+10);
				Sections::getString468A(&data[pointer+1], item_length,buffer2.GetBuffer());
				item = buffer2.GetBuffer();
			}

			//		Log("  1: %d item_length:[%s]",pointer,item.c_str());

			pointer += (1 + item_length);
			len1 -= (2 + item_description_length + item_length);
			lenB -= (2 + item_description_length + item_length);
		};

		pointer=7+length_of_items;
		text_length = data[pointer];
		//	Log("  2: %d text_length:[%d]",pointer,text_length);
		pointer += 1;
		if (text_length< 0 || pointer+text_length>descriptor_length+2) 
		{
			Log("*** epg:DecodeExtendedEvent check 3");
			return;
		}

		if (text_length>0)
		{
			CAutoString buffer (text_length+10);
			Sections::getString468A(&data[pointer], text_length,buffer.GetBuffer());
			text = buffer.GetBuffer();
		}

		//Log("  2: %d [%s]",pointer,text.c_str());

		//BUG HERE?
		//find language...
		EPGEvent::ivecLanguages it = epgEvent.vecLanguages.begin();
		for (it = epgEvent.vecLanguages.begin(); it != epgEvent.vecLanguages.end();++it)
		{
			EPGLanguage& lang=*it;
			if (lang.language==language)
			{
				//found.
				if (item.size()>0)
					lang.event+=item;
				if (text.size()>0)
					lang.text+=text;
				//			Log("epg grab ext:[%s][%s]", lang.event.c_str(),lang.text.c_str());
				return;
			}
		}
		//add new language...
		EPGLanguage lang;
		lang.language=language;
		if (item.size()>0)
			lang.event+=item;
		if (text.size()>0)
			lang.text+=text;
		//	Log("epg grab ext:[%s][%s]", lang.event.c_str(),lang.text.c_str());
		epgEvent.vecLanguages.push_back(lang);

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodeExtendedEvent()");
	}	
}

void CEPGParser::DecodeShortEventDescriptor(byte* buf, EPGEvent& epgEvent)
{
	try
	{
		int descriptor_tag = buf[0];
		int descriptor_len = buf[1];
		if(descriptor_tag!=0x4d) 
		{
			Log("*** DecodeShortEventDescriptor: tag !=0x4d");
			return;
		}
		if (descriptor_len<6) 
		{
			Log("*** DecodeShortEventDescriptor: len <6");
			return;
		}

		unsigned long ISO_639_language_code=(buf[2]<<16)+(buf[3]<<8)+buf[4];
		int event_len = buf[5];

		string eventText="";
		string eventDescription="";
		//Log("DecodeShortEventDescriptor: Lang:0x%x eventlen:0x%x",ISO_639_language_code,event_len);
		if (event_len >0)
		{
			if (6+event_len > descriptor_len+2)
			{
				Log("*** DecodeShortEventDescriptor: check1: %d %d",event_len,descriptor_len);
				return;
			}
			CAutoString buffer(event_len+10);
			Sections::getString468A(&buf[6],event_len,buffer.GetBuffer());
			eventText=buffer.GetBuffer();
			//		Log("  event:%s",eventText.c_str());
		}
		else if (event_len<0)
		{
			Log("*** DecodeShortEventDescriptor: check1a: %d %d",event_len,descriptor_len);
			return;
		}
		int off=6+event_len;
		int text_len = buf[off];
		//Log("DecodeShortEventDescriptor: text_len:0x%x",text_len);
		if (text_len >0)
		{
			if (off+text_len > descriptor_len+2) 
			{
				Log("*** DecodeShortEventDescriptor: check2: %d %d",event_len,descriptor_len);
				return;
			}
			CAutoString buffer (text_len+10);
			Sections::getString468A(&buf[off+1],text_len,buffer.GetBuffer());
			eventDescription=buffer.GetBuffer();
			//		Log("  text:%s",eventDescription.c_str() );
		}
		else if (text_len<0)
		{
			Log("*** DecodeShortEventDescriptor: check2a: %d %d",event_len,descriptor_len);
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
				//Log("epg grab short:[%s][%s]", lang.event.c_str(),lang.text.c_str());
				return;
			}
		}
		EPGLanguage lang;
		lang.language=ISO_639_language_code;
		if (eventText.size()>0)
			lang.event=eventText;
		if (eventDescription.size()>0)
			lang.text=eventDescription;
		//Log("epg grab short:[%s][%s]", lang.event.c_str(),lang.text.c_str());
		epgEvent.vecLanguages.push_back(lang);

	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodeShortEventDescriptor()");
	}	
}

void CEPGParser::DecodeContentDescription(byte* buf,EPGEvent& epgEvent)
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
			Log("*** epg:DecodeContentDescription tag!=0x54");
			return;
		}

		len = descriptor_length;
		int pointer=  2;
		while ( len > 0) 
		{
			if (pointer+1>descriptor_length+2) 
			{
				Log("*** epg:DecodeContentDescription check1");
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
			//Log("genre:%s", genreText);
			if (epgEvent.genre.size()==0 && strlen(genreText)>0)
				epgEvent.genre=genreText;
		}
	}
	catch(...)
	{
		Dump("mpsaa: unhandled exception in Sections::DecodeContentDescription()");
	}	
}

void CEPGParser::ResetEPG()
{
	CEnterCriticalSection lock (m_critSection);
	Log("epg:ResetEPG()");
	EPGChannel ch;
	EPGEvent evt;

	m_prevChannel=ch;
	m_prevChannelIndex-1;
	m_prevEvent = evt;
	m_prevEventIndex=-1;

	m_mapEPG.clear();
	//m_bParseEPG=false;
	m_bEpgDone=false;
	m_epgTimeout=time(NULL)+60;
}

void CEPGParser::GrabEPG()
{
	CEnterCriticalSection lock (m_critSection);
	Log("epg:GrabEPG()");
	EPGChannel ch;
	EPGEvent evt;

	m_prevChannel=ch;
	m_prevChannelIndex=-1;
	m_prevEvent = evt;
	m_prevEventIndex=-1;

	m_mapEPG.clear();
	m_bParseEPG=true;
	m_bEpgDone=false;
	m_epgTimeout=time(NULL);
}
bool CEPGParser::IsEPGGrabbing()
{
	return m_bParseEPG;
}
bool CEPGParser::IsEPGReady()
{
	if (m_bEpgDone)
	{
		Log("CEPGParser::IsEPGReady() ->yes");
	}
	return m_bEpgDone;
}
ULONG CEPGParser::GetEPGChannelCount( )
{
	
	CEnterCriticalSection lock (m_critSection);
	//	Log("GetEPGChannelCount:%d",m_mapEPG.size());
	return m_mapEPG.size();
}
bool CEPGParser::GetChannelByindex(ULONG channelIndex, EPGChannel& epgChannel)
{
	
	CEnterCriticalSection lock (m_critSection);
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

ULONG  CEPGParser::GetEPGEventCount( ULONG channel)
{
	
	CEnterCriticalSection lock (m_critSection);
	EPGChannel epgChannel;
	if (!GetChannelByindex(channel,epgChannel)) return 0;
	return epgChannel.mapEvents.size();
}
void CEPGParser::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	
	CEnterCriticalSection lock (m_critSection);
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
void CEPGParser::GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* languageCount, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre    )
{
	
	CEnterCriticalSection lock (m_critSection);
	*languageCount=0;
	*dateMJD=0;
	*timeUTC=0;
	*duration=0;
	*genre = (char*)"";
	if (channel!=m_prevChannelIndex)
	{
		EPGChannel epgChannel;
		if (!GetChannelByindex(channel,epgChannel)) return ;
	}
	if (eventid >= m_prevChannel.mapEvents.size()) return;

	ULONG count=0;
	EPGChannel::imapEvents itEvent=m_prevChannel.mapEvents.begin();
	while (count < eventid) 
	{ 
		itEvent++; 
		count++;
	}
	EPGEvent& epgEvent=itEvent->second;
	m_prevEventIndex=eventid;
	m_prevEvent=epgEvent;

	*languageCount=epgEvent.vecLanguages.size();
	*dateMJD=epgEvent.dateMJD;
	*timeUTC=epgEvent.timeUTC;
	*duration=epgEvent.duration;
	*genre=(char*)epgEvent.genre.c_str() ;
}
void CEPGParser::GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    )
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
		if (eventid >= m_prevChannel.mapEvents.size()) return;
		ULONG count=0;
		EPGChannel::imapEvents itEvent=m_prevChannel.mapEvents.begin();
		while (count < eventid) 
		{ 
			itEvent++; 
			count++;
		}
		EPGEvent& epgEvent=itEvent->second;
		m_prevEventIndex=eventid;
		m_prevEvent=epgEvent;
	}
	
	//Log("Lang %d %d", languageIndex, m_prevEvent.vecLanguages.size());
	if (languageIndex >=0 && languageIndex < m_prevEvent.vecLanguages.size())
	{
		EPGLanguage& lang= m_prevEvent.vecLanguages[languageIndex];
		*eventText=(char*)lang.event.c_str();
		*eventDescription=(char*)lang.text.c_str();
		*language=lang.language;
		//Log("epg:get->[%s][%s]", lang.event.c_str(),lang.text.c_str());
	}
}

