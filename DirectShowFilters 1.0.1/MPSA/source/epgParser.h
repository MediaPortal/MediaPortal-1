#ifndef EPG_PARSER_H
#define EPG_PARSER_H
#include <map>
#include <vector>
#include <string>
using namespace std;

#include "criticalsection.h"
using namespace Mediaportal;
typedef  struct stEPGLanguage
{
	DWORD language;
	string event;
	string text;
} EPGLanguage;

typedef struct stEPGEvent
{
	unsigned int eventid;
	unsigned long dateMJD;
	unsigned long timeUTC;
	unsigned long duration;
	unsigned int running_status;
	unsigned int free_CA_mode;
	string genre;
	vector<EPGLanguage> vecLanguages;
	typedef vector<EPGLanguage>::iterator ivecLanguages;
}EPGEvent;

typedef struct stEPGChannel
{
	bool allSectionsReceived;
	int last_section_number;
	int original_network_id;
	int transport_id;
	int service_id;
	map<unsigned int,EPGEvent> mapEvents;
	typedef map<unsigned int,EPGEvent>::iterator imapEvents;

	map<int,bool> mapSectionsReceived;
	typedef map<int,bool>::iterator imapSectionsReceived;
}EPGChannel;

class CEPGParser

{

public:
	CEPGParser();
	virtual ~CEPGParser();
	void	ResetEPG();
	void	GrabEPG();
	bool	IsEPGReady();
	bool	IsEPGGrabbing();
	ULONG	GetEPGChannelCount( );
	ULONG	GetEPGEventCount( ULONG channel);
	void	GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	void	GetEPGEvent( ULONG channel,  ULONG event,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre    );
	void    GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription    );
	HRESULT	DecodeEPG(byte* pbData,int len);

private:
	bool GetChannelByindex(ULONG channel, EPGChannel& epgChannel);
	void DecodeShortEventDescriptor(byte* buf,EPGEvent& event);
	void DecodeContentDescription(byte* buf,EPGEvent& event);
	void DecodeExtendedEvent(byte* buf, EPGEvent& event);
	

	map<unsigned long,EPGChannel> m_mapEPG;
	typedef map<unsigned long,EPGChannel>::iterator imapEPG;
	bool	m_bParseEPG;
	bool	m_bEpgDone;
	time_t  m_epgTimeout;
	long	   m_prevChannelIndex;
	long	   m_prevEventIndex;
	EPGChannel m_prevChannel;
	EPGEvent   m_prevEvent;
	CCriticalSection m_critSection;
};

#endif