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

#include "SkyEpgTitleDecoder.h"
#include "SkyEpgParser.h"
#include "SkyManager.h"
#include "SkyThemes.h"


//	Cstr
CSkyEpgTitleDecoder::CSkyEpgTitleDecoder(CSkyEpgParser *parentSkyEpgParser)
{
	skyEpgParser = parentSkyEpgParser;
}

//	Flzr
CSkyEpgTitleDecoder::~CSkyEpgTitleDecoder()
{
}

//	Fired when a title section is received
void CSkyEpgTitleDecoder::OnTitleSectionReceived(unsigned short pid, CSection *section)
{
	CSkyManager* skyManager = skyEpgParser->GetSkyManager();
	
	Mediaportal::CEnterCriticalSection enter(skyManager->criticalSection);

	//	If the title data carousel is complete for this pid, we can discard the data as we already have it
	if(skyManager->IsTitleDataCarouselOnPidComplete(pid))
		return;

	//	Validate table id
	if(!skyManager->DoesTidCarryEpgTitleData(section->table_id))
		return;

	byte* buffer = section->Data;

	//	Total length of title data (2 less for this length field)
	int totalLength = (((buffer[1] & 0x0F) << 8) + buffer[2]) - 2;
	
	//	If this section is a valid length (20 absolute minimum with 1 blank title)
	if(section->section_length < 20)
		return;

	//	Get the channel id that this section's summary data relates to
	unsigned short channelId = (buffer[3] << 8) + buffer[4];
	unsigned short mjdStart = (buffer[8] << 8) + buffer[9];
	
	//	Check channel id and start date are valid
	if(channelId == 0 || mjdStart == 0)
		return;

	//	Always starts at 10th byte
	int currentTitleItem = 10;

	int iterationCounter = 0;

	//	Loop while we have more summary data
	while(currentTitleItem < totalLength)
	{
		if(iterationCounter > 512)
		{
			skyManager->LogError("CSkyEpgTitleDecoder::OnTitleSectionReceived() - Error, infinite loop detected.  Dropping packet.");
			return;
		}

		iterationCounter++;

		//	Extract event id, header type and body length
		unsigned short eventId = (buffer[currentTitleItem + 0] << 8) + buffer[currentTitleItem + 1];
		unsigned char headerType = (buffer[currentTitleItem + 2] & 0xF0 ) >> 4;
		unsigned short bodyLength = ((buffer[currentTitleItem + 2] & 0x0F) << 8) + buffer[currentTitleItem + 3];

		//	Build the carousel lookup id
		unsigned int carouselLookupId = (channelId << 16) + eventId;

		//	Notify the manager that a title has been received
		skyManager->OnTitleReceived(pid, carouselLookupId);

		//	If the title carousel for this pid is now complete, we can return
		if(skyManager->IsTitleDataCarouselOnPidComplete(pid))
			return;

		//	Get the epg event we are to populate from the manager
		SkyEpgEvent* epgEvent = skyManager->GetEpgEvent(channelId, eventId);

		//	Check we have the event reference
		if(epgEvent == NULL)
		{
			skyManager->LogError("CSkyEpgTitleDecoder::OnTitleSectionReceived() - Error, failed to get event reference");
			return;
		}

		//	Set the event id and mjd start 
		epgEvent->mjdStart = mjdStart;
		epgEvent->eventId = eventId;

		int headerLength = 4;

		//	Move to the body of the summary
		int currentTitleItemBody = currentTitleItem + headerLength;

		//	Extract title descriptor and huffman buffer length
		unsigned short titleDescriptor = buffer[currentTitleItemBody + 0];
		unsigned short encodedBufferLength = buffer[currentTitleItemBody + 1] - 7;
		
		//	If normal title item (0xB5)
		if(titleDescriptor == 0xB5)	
		{
			//	Start time stamp (multiplied by 2: << 1) as 86400 seconds in a day and 2 bytes can only store 65535
			//	10800 = 0600
			//	21600 = 12 noon
			epgEvent->startTime = ((buffer[currentTitleItemBody + 2] << 8) + buffer[currentTitleItemBody + 3]) << 1;

			//	Duration (multiplied by 2: << 1) as 86400 seconds in a day and 2 bytes can only store 65535
			//	1800 = 1 hour
			//	900 = 30 mins
			epgEvent->duration = ((buffer[currentTitleItemBody + 4] << 8) + buffer[currentTitleItemBody + 5]) << 1;

			//	Get the theme id
			byte themeId = buffer[currentTitleItemBody + 6];

			//	Set the event theme depending on country
			switch(skyManager->GetCurrentCountryId())
			{
			case SKY_COUNTRY_UK:

				epgEvent->theme = (byte*)ukThemes[themeId];
				break;

			case SKY_COUNTRY_IT:

				epgEvent->theme = (byte*)itThemes[themeId];
				break;
			}

			//	Extract the series termination flag (well its actually a series continuation flag, but we'll invert it to be more concise)
			epgEvent->seriesTermination = ((buffer[currentTitleItemBody + 8] & 0x40) >> 6) ^ 0x01;

			//	If there is no title huffman data, move to next item
			if(encodedBufferLength <= 0)
			{
				currentTitleItem += (headerLength + bodyLength);
				continue;
			}

			//	If this title has not yet been decoded
			if(epgEvent->title == NULL)
			{
				//	Decode the huffman buffer
				epgEvent->title = skyManager->DecodeHuffmanData(&buffer[currentTitleItemBody + 9], encodedBufferLength);

				//	If failed to decode
				if(epgEvent->title == NULL)
				{
					char* cannotDecodeBuffer = "(Cannot decode)";
					int cannotDecodeBufferLength = strlen(cannotDecodeBuffer) + 1;
					epgEvent->title = (byte*) malloc(cannotDecodeBufferLength);

					memcpy(epgEvent->title, cannotDecodeBuffer, cannotDecodeBufferLength);
				}

				else
					//	Notify the manager (for statistics)
					skyManager->OnTitleDecoded();
			}

		}

		//	Else other unknown data item
		else
		{
			return;
			//skyManager->LogError("CSkyEpgTitleDecoder::OnTitleSectionReceived() - Error, unrecognised title descriptor");
		}
		
		//	Move to next summary item
		currentTitleItem += (bodyLength + headerLength);
	}

	//	Check the packet was parsed correctly
	if(currentTitleItem != (totalLength + 1))
	{
		//skyManager->LogError("CSkyEpgSummaryDecoder::OnTitleSectionReceived - Warning, title packet was not parsed correctly - pointer not in expected place");
		return;
	}

}
