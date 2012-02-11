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

#include "SkyChannelDecoder.h"
#include "SkyChannelParser.h"
#include "SkyManager.h"


typedef struct SkyBouquet;


//	Cstr
CSkyChannelDecoder::CSkyChannelDecoder(CSkyChannelParser* parentSkyChannelParser)
{
	skyChannelParser = parentSkyChannelParser;

}

//	Flzr
CSkyChannelDecoder::~CSkyChannelDecoder()
{
}

//	Fired when a new channel network data section is received
void CSkyChannelDecoder::OnNetworkDataSectionReceived(unsigned short pid, CSection* dataSection)
{
	CSkyManager* skyManager = skyChannelParser->GetSkyManager();

	Mediaportal::CEnterCriticalSection enter(skyManager->criticalSection);

	//	If all bouquets are already fully populated, return
	if(skyManager->AreAllBouquetsPopulated())
		return;

	//	Check this table carries channel data
	if(!skyManager->DoesTidCarryChannelNetworkData(dataSection->table_id))
		return;

	byte* buffer = dataSection->Data;
	
	int bouquetId = (buffer[3] << 8) + buffer[4];
	int bouquetDescriptorLength = ((buffer[8] & 0x0F ) << 8) + buffer[9];

	SkyBouquet* skyBouquet = skyManager->GetBouquet(bouquetId);

	if(skyBouquet == NULL)
	{
		skyManager->LogError("CSkyChannelDecoder::OnNetworkDataSectionReceived() - Error, failed to create new or retrieve existing Sky bouquet");
		return;
	}

	//	If the bouquet is not initialized, this is the first time we have seen it
	if(!skyBouquet->isInitialized)
	{
		//	Populate first section number seen
		skyBouquet->firstReceivedSectionNumber = dataSection->section_number;
		skyBouquet->isInitialized = true;
	}

	else
	{
		//	If all the channels are already populated for this bouquet, we can skip this packet
		if(skyBouquet->isPopulated)
			return;
		
		//	If we are back round to the first section we saw for this bouquet, it is now fully populated
		if(dataSection->section_number == skyBouquet->firstReceivedSectionNumber)
		{
			skyBouquet->isPopulated = true;
			skyManager->NotifyBouquetPopulated();

			return;
		}
	}

	int body = 10 + bouquetDescriptorLength;
    int bouquetPayloadLength = ((buffer[body + 0] & 0x0F) << 8) + buffer[body + 1];
	int endOfPacket = body + bouquetPayloadLength + 2;
	int currentTransportGroup = body + 2;

	//	Loop through all transport groups in this packet
	while(currentTransportGroup < endOfPacket)
	{
		//	Extract transport id and network id
		unsigned short int transportId = (buffer[currentTransportGroup + 0] << 8) + buffer[currentTransportGroup + 1];
		unsigned short int networkId = (buffer[currentTransportGroup + 2] << 8) + buffer[currentTransportGroup + 3];

		int transportGroupLength = ((buffer[currentTransportGroup + 4] & 0x0F) << 8) + buffer[currentTransportGroup + 5];
		int currentTransportDescriptor = currentTransportGroup + 6;
		int endOfTransportGroupDescriptors = currentTransportDescriptor + transportGroupLength;

		while(currentTransportDescriptor < endOfTransportGroupDescriptors)
		{
			//	Descriptor type
			byte descriptorType = buffer[currentTransportDescriptor];

			int descriptorLength = buffer[currentTransportDescriptor + 1];
			int currentServiceDescriptor = currentTransportDescriptor + 2;
			int endOfServiceDescriptors = currentServiceDescriptor + descriptorLength - 2;

			//	If the descriptor is channel information (0xB1)
			if(descriptorType == 0xB1)
			{
				while(currentServiceDescriptor < endOfServiceDescriptors)
				{
					//	Extract service id and channel information
					int serviceId = (buffer[currentServiceDescriptor + 2] << 8) + buffer[currentServiceDescriptor + 3];
					int channelId = (buffer[currentServiceDescriptor + 5] << 8) + buffer[currentServiceDescriptor + 6];
					int skyChannelNumber = (buffer[currentServiceDescriptor + 7] << 8) + buffer[currentServiceDescriptor + 8];
					
					SkyChannel* skyChannel = skyManager->GetChannel(channelId);

					//	If the channel information has not been populated, do this now
					if(!skyChannel->isPopulated)
					{
						skyChannel->networkId = networkId;
						skyChannel->transportId = transportId;
						skyChannel->serviceId = serviceId;
						skyChannel->epgChannelNumber = skyChannelNumber;
						skyChannel->isPopulated = true;
					}
				
					currentServiceDescriptor += 9;
				}
			}

			currentTransportDescriptor += descriptorLength + 2;
		}

		//	Move to the next transport stream
		currentTransportGroup += transportGroupLength + 6;
	}

	

	
}
