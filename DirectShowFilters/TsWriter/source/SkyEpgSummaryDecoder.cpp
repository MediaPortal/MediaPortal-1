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


#include "SkyEpgSummaryDecoder.h"
#include "SkyEpgParser.h"
#include "SkyManager.h"


//	Cstr
CSkyEpgSummaryDecoder::CSkyEpgSummaryDecoder(CSkyEpgParser *parentSkyEpgParser)
{
	skyEpgParser = parentSkyEpgParser;
}

//	Flzr
CSkyEpgSummaryDecoder::~CSkyEpgSummaryDecoder()
{
}

//	Fired when a summary section is received
void CSkyEpgSummaryDecoder::OnSummarySectionReceived(unsigned short pid, CSection *section)
{
	CSkyManager* skyManager = skyEpgParser->GetSkyManager();
	
	Mediaportal::CEnterCriticalSection enter(skyManager->criticalSection);

	//	If the summary data carousel is complete for this pid, we can discard the data as we already have it
	if(skyManager->IsSummaryDataCarouselOnPidComplete(pid))
		return;

	//	Validate table id
	if(!skyManager->DoesTidCarryEpgSummaryData(section->table_id))
		return;

	byte* buffer = section->Data;

	//	Total length of summary data (2 less for this length field)
	int totalLength = (((buffer[1] & 0x0F) << 8) + buffer[2]) - 2;
	
	//	If this section is a valid length (14 absolute minimum with 1 blank summary)
	if(section->section_length < 14)
		return;

	//	Get the channel id that this section's summary data relates to
	unsigned short channelId = (buffer[3] << 8) + buffer[4];
	unsigned short mjdStartDate = (buffer[8] << 8) + buffer[9];
	
	//	Check channel id and start date are valid
	if(channelId == 0 || mjdStartDate == 0)
		return;

	//	Always starts at 10th byte
	int currentSummaryItem = 10;

	int iterationCounter = 0;

	//	Loop while we have more summary data
	while(currentSummaryItem < totalLength)
	{
		if(iterationCounter > 512)
		{
			skyManager->LogError("CSkyEpgTitleDecoder::OnSummarySectionReceived() - Error, infinite loop detected.  Dropping packet.");
			return;
		}

		iterationCounter++;

		//	Extract event id, header type and body length
		unsigned short eventId = (buffer[currentSummaryItem + 0] << 8) + buffer[currentSummaryItem + 1];
		unsigned char headerType = (buffer[currentSummaryItem + 2] & 0xF0 ) >> 4;
		unsigned short bodyLength = ((buffer[currentSummaryItem + 2] & 0x0F) << 8) + buffer[currentSummaryItem + 3];

		//	Build the carousel lookup id
		unsigned int carouselLookupId = (channelId << 16) + eventId;

		//	Notify the parser that a title has been received
		skyManager->OnSummaryReceived(pid, carouselLookupId);

		//	If the summary carousel for this pid is now complete, we can return
		if(skyManager->IsSummaryDataCarouselOnPidComplete(pid))
			return;

		//	Get the epg event we are to populate from the manager
		SkyEpgEvent* epgEvent = skyManager->GetEpgEvent(channelId, eventId);

		//	Check we have the event reference
		if(epgEvent == NULL)
		{
			skyManager->LogError("CSkyEpgSummaryDecoder::OnSummarySectionReceived() - Error, failed to get event reference");
			return;
		}

		int headerLength;

		//	If this is an extended header (0xF) (7 bytes long)
		if(headerType == 0xF)
			headerLength = 7;

		//	Else if normal header (0xB) (4 bytes long)
		else if(headerType == 0xB)
			headerLength = 4;

		//	Else other unknown header (not worked them out yet, at least 4 more)
		//	Think these are only used for box office and adult channels so not really important
		else
		{
			//	Cannot parse the rest of this packet as we dont know the header lengths/format etc
			return;
		}

		
		//	If body length is less than 3, there is no summary data for this event, move to next
		if(bodyLength < 3)
		{
			currentSummaryItem += (headerLength + bodyLength);
			continue;
		}


		//	Move to the body of the summary
		int currentSummaryItemBody = currentSummaryItem + headerLength;

		//	Extract summary signature and huffman buffer length
		unsigned short summaryDescriptor = buffer[currentSummaryItemBody + 0];
		unsigned short encodedBufferLength = buffer[currentSummaryItemBody + 1];
		
		//	If normal summary item (0xB9)
		if(summaryDescriptor == 0xB9)	
		{
			if(epgEvent->summary == NULL)
			{
				//	Decode the summary
				epgEvent->summary = skyManager->DecodeHuffmanData(&buffer[currentSummaryItemBody + 2], encodedBufferLength);

				//	If failed to decode
				if(epgEvent->summary == NULL)
				{
					char* cannotDecodeBuffer = "(Cannot decode)";
					int cannotDecodeBufferLength = strlen(cannotDecodeBuffer) + 1;
					epgEvent->summary = (byte*) malloc(cannotDecodeBufferLength);

					memcpy(epgEvent->summary, cannotDecodeBuffer, cannotDecodeBufferLength);
				}

				else
					//	Notify the manager (for statistics)
					skyManager->OnSummaryDecoded();
			}
		}

		//	Else if (0xBB) - Unknown data item (special box office or adult?)
		//	Seems very rare (1 in every 2000 or so), so not important really
		else if(summaryDescriptor == 0xBB)
		{
			//	Skip
		}

		//	Else other unknown data item, there are a few others that are unknown
		else
		{
			return;
			//skyManager->LogError("CSkyEpgSummaryDecoder::OnSummarySectionReceived() - Error, unrecognised summary descriptor");
		}

		//	Is there any footer information?
		int footerLength = bodyLength - encodedBufferLength - 2;

		if(footerLength >= 4)
		{
			int footerPointer = currentSummaryItemBody + 2 + encodedBufferLength;

			//	Get the descriptor
			short footerDescriptor = buffer[footerPointer + 0];

			//	If series id information (0xC1)
			if(footerDescriptor == 0xC1)
			{
				epgEvent->seriesId = (buffer[footerPointer + 2] << 8) + (buffer[footerPointer + 3] << 0);
			}			
		}		

		//	Move to next summary item
		currentSummaryItem += (bodyLength + headerLength);
	}

	//	Check the packet was parsed correctly - seem to get a few of these.  
	//	Seems to be some extra information tagged onto the end of some summary packets (1 in every 2000 or so)
	//	Not worked this out - possibly box office information
	if(currentSummaryItem != (totalLength + 1))
	{
		//skyManager->LogError("CSkyEpgSummaryDecoder::OnSummarySectionReceived() - Warning, summary packet was not parsed correctly - pointer not in expected place");
		
		return;
	}

}
