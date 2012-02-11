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

#include <map>
#include <windows.h>
#include "SkyEpgParser.h"
#include "SkyChannelParser.h"
#include "HuffmanTree.h"
#include "CriticalSection.h"
#include "EnterCriticalSection.h"

using namespace std;

//	External logging method
extern void LogDebug(const char *fmt, ...);


#define MAX_LOGGED_ERRORS	20


#define SKY_COUNTRY_UK	0x01
#define SKY_COUNTRY_IT	0x02


//	Sky UK & IT use pid 0x11 for epg channel information
#define PID_SKYUKIT_EPG_CHANNEL_INFORMATION		0x11

//	Sky UK & IT use pids 0x30-0x37 for epg item titles & 0x40-0x47 for normal summaries & 0x50-0x57 for box office and adult summaries
#define PID_SKYUKIT_EPG_TITLES_F     0x30
#define PID_SKYUKIT_EPG_TITLES_T     0x37
#define PID_SKYUKIT_EPG_SUMMARIES_F  0x40
#define PID_SKYUKIT_EPG_SUMMARIES_T  0x47

#define PID_SKYUKIT_EPG_CHANNEL_TABLEID		0x4A
#define PID_SKYUKIT_EPG_TITLE_TABLEID_F		0xA0
#define PID_SKYUKIT_EPG_TITLE_TABLEID_T		0xA3
#define PID_SKYUKIT_EPG_SUMMARY_TABLEID_F	0xA8
#define PID_SKYUKIT_EPG_SUMMARY_TABLEID_T	0xAB


#define HUFFMAN_DECODE_BUFFER_SIZE		8192


typedef struct SkyEpgEvent;

typedef struct SkyBouquet
{
	unsigned short bouquetId;
	byte firstReceivedSectionNumber;
	bool isInitialized;
	bool isPopulated;

	SkyBouquet::SkyBouquet(unsigned short newBouquetId)
	{
		bouquetId = newBouquetId;
		firstReceivedSectionNumber = NULL;
		isInitialized = false;
		isPopulated = false;
	}

}SkyBouquet;

typedef struct SkyChannel
{
	unsigned short channelId;

	unsigned short networkId;
	unsigned short transportId;
	unsigned short serviceId;
	unsigned short epgChannelNumber;

	bool isPopulated;

	map<unsigned short, SkyEpgEvent*>* events;

	SkyChannel::SkyChannel(unsigned short _channelId)
	{
		channelId = _channelId;

		networkId = NULL;
		transportId = NULL;
		serviceId = NULL;
		epgChannelNumber = NULL;

		isPopulated = false;

		events = NULL;
	}

}SkyChannel;

//	Sky epg event
typedef struct SkyEpgEvent
{
	unsigned short eventId;

	unsigned short mjdStart;
	unsigned int startTime;
	unsigned int duration;
	unsigned short seriesId;
	unsigned int seriesTermination;

	byte* title;
	byte* summary;
	byte* theme;

	bool isPopulated;

	SkyEpgEvent::SkyEpgEvent(unsigned short _eventId)
	{
		eventId = _eventId;

		mjdStart = NULL;
		startTime = NULL;
		duration = NULL;
		seriesId = NULL;
		seriesTermination = NULL;

		title = NULL;
		summary = NULL;
		theme = NULL;

		isPopulated = false;
	}
};


class CSkyManager
{
public:
	//	Constr
	CSkyManager();
	
	//	Flzr
	~CSkyManager();

	//	Logs an error a maximum number of times
	void LogError(const char* message);

	//	Resets the manager
	void Reset();

	//	Activates the epg grabber for the specified Sky country
	void ActivateEpgGrabber(unsigned short activateCountryId);

	//	De-activates the epg grabber
	void DeActivateEpgGrabber();

	//	Is the epg grabbing active
	bool IsEpgGrabbingActive();

	//	Is the epg grabbing finished
	bool IsEpgGrabbingFinished();

	//	Has EPG grabbing been aborted through too many errors?
	bool HasEpgGrabbingAborted();
	
	//	Does this pid carry channel network data?
	bool DoesPidCarryChannelNetworkData(int pid);

	//	Does this tid carry channel network data?
	bool DoesTidCarryChannelNetworkData(int tid);

	//	Does this pid carry epg data?
	bool DoesPidCarryEpgData(int pid);

	//	Does the pid carry epg title data?
	bool DoesPidCarryEpgTitleData(int pid);
	
	//	Does the pid carry epg summary data?
	bool DoesPidCarryEpgSummaryData(int pid);

	//	Does the table id carry epg title data?
	bool DoesTidCarryEpgTitleData(int tid);

	//	Does the table id carry
	bool DoesTidCarryEpgSummaryData(int tid);

	//	Fired when a new ts packet is received
	void OnTsPacket(CTsHeader& header, byte* tsPacket);

	//	Notifys the manager that the epg channel network data is fully populated
	void NotifyEpgChannelNetworkDataFullyPopulated();

	//	Gets the bouquet with the specified id
	SkyBouquet* GetBouquet(unsigned short bouquestId);

	//	Gets the channel with the specified id
	SkyChannel* GetChannel(unsigned short channelId);

	//	Gets the epg event with the specified ids
	SkyEpgEvent* GetEpgEvent(unsigned short channelId, unsigned short eventId);
	
	//	Gets the current country id
	unsigned int GetCurrentCountryId();

	//	Are all bouquets populated?
	bool AreAllBouquetsPopulated();

	//	Are all epg titles populated
	bool AreAllTitlesPopulated();

	//	Are all epg summaries populated?
	bool AreAllSummariesPopulated();
	
	//	Notifys that a bouquet is now fully populated
	void NotifyBouquetPopulated();

	//	Gets the channel count
	unsigned int GetChannelCount();

	//	Gets the epg event count
	unsigned int GetEpgEventCount();
	
	//	Notifys the manager that a title event has been received
	void OnTitleReceived(unsigned short pid, unsigned int titleChannelEventUnionId);

	//	Gets if the data carousel for the specified pid is now complete
	bool IsTitleDataCarouselOnPidComplete(unsigned short pid);

	//	Notifys the manager that a summary event has been received
	void OnSummaryReceived(unsigned short pid, unsigned int summaryChannelEventUnionId);

	//	Gets if the data carousel for the specified pid is now complete
	bool IsSummaryDataCarouselOnPidComplete(unsigned short pid);

	//	Notifies the manager that a title has been decoded
	void OnTitleDecoded();

	//	Nofitied the manager that a summary has been decoded
	void OnSummaryDecoded();

	//	Fired when the epg callback is triggered
	void OnEpgCallback();

	//	Decodes a huffman buffer
	byte* DecodeHuffmanData(byte* buffer, int length);

	//	Retrieval epg methods

	//	Resets the retrieval pointers
	void ResetEpgRetrieval();

	//	Gets the next epg channel
	void GetNextSkyEpgChannel(unsigned char* atEnd, unsigned short* channelId, unsigned short* networkId, unsigned short* transportId, unsigned short* serviceId);

	//	Gets the next epg channel event
	void GetNextSkyEpgChannelEvent(unsigned char* atEnd, unsigned short* eventId, unsigned short* mjdStart, unsigned int* startTime, unsigned int* duration, unsigned char** title, unsigned char** summary, unsigned char** theme, unsigned short* seriesId, byte* seriesTermination);

	//	Critical section
	Mediaportal::CCriticalSection criticalSection;

private:

	
	bool isInitialized;
	bool epgFirmwareHuffmanTreesPopulated;
	bool isEpgGrabbingActive;
	bool retrievalReset;
	bool epgGrabbingAbortedThroughTooManyErrors;

	unsigned int numberErrorsLogged;
	unsigned int numberFailedHuffmanDecodes;
	unsigned int numberBouquetsPopulated;

	unsigned int currentCountryId;

	byte huffmanDecodeBuffer[HUFFMAN_DECODE_BUFFER_SIZE];

	CSkyEpgParser* skyEpgParser;
	CSkyChannelParser* skyChannelParser;

	map<unsigned short, CHuffmanTree*> countryFirmwareHuffmanTrees;

	map<unsigned short, SkyBouquet*> bouquets;

	map<unsigned short, SkyChannel*> channels;

	//	Epg retrieval iterators
	map<unsigned short, SkyChannel*>::iterator epgRetrievalChannelsIt;
	map<unsigned short, SkyEpgEvent*>::iterator epgRetrievalChannelEventsIt;
	
	//	Lookup of the first piece of title data lifted of the pid carousel, indexed on pid
	map<unsigned short, unsigned int> titleDataCarouselStartLookup;

	//	Completed title carousels (pid)
	vector<unsigned short> completedTitleDataCarousels;
	
	//	Lookup of the first piece of title data lifted of the pid carousel, indexed on pid
	map<unsigned short, unsigned int> summaryDataCarouselStartLookup;

	//	Completed summary carousels (pid)
	vector<unsigned short> completedSummaryDataCarousels;

	int titlesDecoded;
	int summariesDecoded;

	//	Initializes the epg huffman trees
	bool InitializeEpgFirmwareHuffmanTrees();

};
