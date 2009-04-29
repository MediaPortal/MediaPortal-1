#pragma once

#include <map>
#include <string>
using namespace std;
#include "section.h"
#include "splittersetup.h"

class ATSCParser
{
	typedef struct stATSCEvent
	{
		int		source_id;
		int		event_id;
		ULONG	start_time;
		int		ETM_location;
		int		length_in_secs;
		string  title;

	}ATSCEvent;

	typedef struct stETMEvent
	{
		ULONG	ETM_id;
		int		source_id;
		int		event_id;
		int     type;
		string  description;
	}ETMEvent;

public:
	ATSCParser(IPin* pPin);
	~ATSCParser(void);
	void SetDemuxer(SplitterSetup* demuxer);

	//ATSC
	void ATSCDecodeChannelTable(BYTE *buf,ChannelInfo *ch, int* channelsFound, int maxLen);
	void ATSCDecodeMasterGuideTable(byte* buf, int len, int* channelsFound);
	void ATSCDecodeEPG(byte* buf, int len);
	void Reset();
	bool IsReady();
	int GetEPGCount();
	void GetEPGTitle(WORD no, WORD* source_id, ULONG* starttime, WORD* length_in_secs, char** title, char** description);
private:
	void DecodeServiceLocationDescriptor( byte* buf,int start,ChannelInfo* channelInfo);
	void DecodeExtendedChannelNameDescriptor( byte* buf,int start,ChannelInfo* channelInfo, int maxLen);
	char* DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes);
	char* DecodeMultipleStrings(byte* buf, int offset, int maxLen);
	void ATSCDecodeETT(byte* buf, int len);
	void ATSCDecodeEIT(byte* buf, int len);
	void ATSCDecodeRTT(byte* buf, int len);
	void ATSCDecodeChannelEIT(byte* buf, int len);

	bool 			masterGuideTableDecoded;
	int  			mgSectionLength;
	int  			mgSectionNumber;
	int  			mgLastSectionNumber;
	SplitterSetup*	m_demuxer;
	IPin*			m_pPin;
	time_t			m_epgTimeout;
	CCritSec m_Lock;

	map<ULONG,ATSCEvent> m_mapEvents;
	typedef map<ULONG,ATSCEvent>::iterator imapEvents;

	map<ULONG,ETMEvent> m_mapEtm;
	typedef map<ULONG,ETMEvent>::iterator imapEtm;

};
