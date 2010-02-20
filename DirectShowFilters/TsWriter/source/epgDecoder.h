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
#ifndef EPG_PARSER_H
#define EPG_PARSER_H
#include <map>
#include <vector>
#include <list>
#include <string>
using namespace std;
#include "..\..\shared\dvbutil.h"
#include "criticalsection.h"
using namespace Mediaportal;

//This is the language code for english. We need this for DISH Network EPG because it doesn't contain one
#define langENG	6647399

typedef  struct stEPGLanguage
{
	DWORD language;
	string event;
	string text;
	unsigned int parentalRating;
	bool CR_added;
} EPGLanguage;

typedef struct stEPGEvent
{
	unsigned int  eventid;
	unsigned long dateMJD;
	unsigned long timeUTC;
	unsigned long duration;
	unsigned int  running_status;
	unsigned int  free_CA_mode;
	string genre;
	string classification;
	int starRating;
	vector<EPGLanguage> vecLanguages;
	typedef vector<EPGLanguage>::iterator ivecLanguages;
    
  bool operator < (const stEPGEvent& rhs)
  {
    if ( dateMJD != rhs.dateMJD) return (dateMJD < rhs.dateMJD);
    return timeUTC < rhs.timeUTC;
  }
}EPGEvent;

typedef struct stEPGChannel
{
	bool    allSectionsReceived;
	int     last_section_number;
	int     original_network_id;
	int     transport_id;
	int     service_id;
	map<DWORD,EPGEvent> mapEvents;
  list<EPGEvent> m_sortedEvents;
	typedef list<EPGEvent>::iterator ilistEvents;
	typedef map<DWORD,EPGEvent>::iterator imapEvents;

	map<int,bool> mapSectionsReceived;
	typedef map<int,bool>::iterator imapSectionsReceived;
}EPGChannel;

class CEpgDecoder : public CDvbUtil

{

public:
	CEpgDecoder();
	virtual ~CEpgDecoder();
	void	ResetEPG();
	void	GrabEPG();
	bool	IsEPGReady();
	bool	IsEPGGrabbing();
	ULONG	GetEPGChannelCount( );
	ULONG	GetEPGEventCount( ULONG channel);
	void	GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	void	GetEPGEvent( ULONG channel,  ULONG event,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre  ,int* starRating, char** classification, unsigned int* eventid   );
	void    GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription,unsigned int* parentalRating  );
	void	AbortGrabbing();
	HRESULT	DecodeEPG(byte* pbData,int len,int PID);
	HRESULT	DecodePremierePrivateEPG(byte* pbData,int len);
	string FreesatHuffmanToString(BYTE *src, int size);
private:
	bool GetChannelByindex(ULONG channel, EPGChannel& epgChannel);
	void DecodeCombinedStarRating_MPAARatingDescriptor(byte* data,EPGEvent &epgEvent);
	void DecodeParentalRatingDescriptor(byte* buf,EPGEvent& event);
	void DecodeShortEventDescriptor(byte* buf,EPGEvent& event,int PID);
	void DecodeContentDescription(byte* buf,EPGEvent& event);
	void DecodeExtendedEvent(byte* buf, EPGEvent& event);
	void DecodeDishShortDescription(byte* data, EPGEvent& epgEvent, int tnum);
	void DecodeDishLongDescription(byte* data, EPGEvent& epgEvent, int tnum);
  void Sort();	

	map<unsigned long,EPGChannel> m_mapEPG;
	typedef map<unsigned long,EPGChannel>::iterator imapEPG;
	bool	m_bParseEPG;
	bool	m_bEpgDone;
	time_t  m_epgTimeout;
	long	   m_prevChannelIndex;
	long	   m_prevEventIndex;
	EPGChannel m_prevChannel;
	EPGEvent   m_prevEvent;
  bool       m_bSorted;
	map<int,bool> m_mapSectionsReceived;
	unsigned long m_pseudo_event_id; // premiere sends one epg event with multiple start times, so we need to make seperate events for this
	typedef map<int,bool>::iterator m_imapSectionsReceived;
	void DecodePremiereContentTransmissionDescriptor(byte* data, EPGEvent epgEvent);
	CCriticalSection m_critSection;
};

#endif
