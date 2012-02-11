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

#include "SkyEpgParser.h"
#include "SkyManager.h"


//	Cstr
CSkyEpgParser::CSkyEpgParser(CSkyManager* parentSkyManager)
{
	Mediaportal::CEnterCriticalSection enter(parentSkyManager->criticalSection);

	try
	{
		skyManager = parentSkyManager;

		//	Create child objects
		skyEpgTitleDecoder = new CSkyEpgTitleDecoder(this);
		skyEpgSummaryDecoder = new CSkyEpgSummaryDecoder(this);

		//	Initialize all title section decoders
		for(unsigned short i = PID_SKYUKIT_EPG_TITLES_F; i <= PID_SKYUKIT_EPG_TITLES_T; i++)
		{
			CSectionDecoder* newTitleDecoder = new CSectionDecoder();

			newTitleDecoder->SetCallBack(this);
			newTitleDecoder->SetPid(i);
			newTitleDecoder->EnableCrcCheck(true);

			epgSectionDecoders.insert(pair<unsigned short, CSectionDecoder*>(i, newTitleDecoder));
		}
		
		//	Initialize all normal summary section decoders
		for(unsigned short i = PID_SKYUKIT_EPG_SUMMARIES_F; i <= PID_SKYUKIT_EPG_SUMMARIES_T; i++)
		{
			CSectionDecoder* newSummaryDecoder = new CSectionDecoder();

			newSummaryDecoder->SetCallBack(this);
			newSummaryDecoder->SetPid(i);
			newSummaryDecoder->EnableCrcCheck(true);

			epgSectionDecoders.insert(pair<unsigned short, CSectionDecoder*>(i, newSummaryDecoder));
		}
		

	}
	catch(...)
	{
		LogDebug("CSkyEpgParser::CSkyEpgParser() - Exception in constructor");
	}
}

//	Flzr
CSkyEpgParser::~CSkyEpgParser()
{
}

//	Gets the parent sky manager reference
CSkyManager* CSkyEpgParser::GetSkyManager()
{
	return skyManager;
}

//	Fired when a new ts packet is received
void CSkyEpgParser::OnTsPacket(CTsHeader& header, byte* tsPacket)
{
	Mediaportal::CEnterCriticalSection enter(skyManager->criticalSection);

	//	If epg grabbing is not active, skip
	if(!skyManager->IsEpgGrabbingActive())
		return;

	unsigned short pid = header.Pid;

	//	If this pid carries epg data
	if(skyManager->DoesPidCarryEpgData(pid))
	{
		//	Try and find the section decoder for this pid
		map<unsigned short, CSectionDecoder*>::iterator epgSectionDecodersIterator = epgSectionDecoders.find(pid);
		
		//	If we couldnt find the decoder for this pid
		if(epgSectionDecodersIterator == epgSectionDecoders.end())
		{
			skyManager->LogError("CSkyEpgParser::OnTsPacket() - Unexpected pid in ts packet");
			return;
		}

		//	Else pass the packet to the section decoder
		epgSectionDecodersIterator->second->OnTsPacket(header, tsPacket);
	}
}

//	Fired when a new section is received
void CSkyEpgParser::OnNewSection(int pid, int tableId, CSection& section)
{
	Mediaportal::CEnterCriticalSection enter(skyManager->criticalSection);

	//	If epg grabbing is not active, skip
	if(!skyManager->IsEpgGrabbingActive())
		return;

	//	If this is a title data section
	if(skyManager->DoesPidCarryEpgTitleData(pid))
	{
		//	Pass to the title decoder
		skyEpgTitleDecoder->OnTitleSectionReceived(pid, &section);
	}

	//	Else if summary data section
	else if(skyManager->DoesPidCarryEpgSummaryData(pid))
	{
		//	Pass to the summary decoder
		skyEpgSummaryDecoder->OnSummarySectionReceived(pid, &section);
	}
	
	else
	{
		skyManager->LogError("CSkyEpgParser::OnNewSection() - Unexpected pid received in section");
		return;
	}
}
