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


#include "SkyChannelParser.h"
#include "SkyManager.h"

//	Cstr
CSkyChannelParser::CSkyChannelParser(CSkyManager *parentSkyManager)
{
	skyManager = parentSkyManager;

	//	Initialize child objects
	skyChannelDecoder = new CSkyChannelDecoder(this);

	//	Initialize the section decoders
	channelNetworkDataSectionDecoder = new CSectionDecoder();

	channelNetworkDataSectionDecoder->SetCallBack(this);
	channelNetworkDataSectionDecoder->SetPid(PID_SKYUKIT_EPG_CHANNEL_INFORMATION);
}

//	Flzr
CSkyChannelParser::~CSkyChannelParser()
{
}

//	Gets the parent sky manager reference
CSkyManager* CSkyChannelParser::GetSkyManager()
{
	return skyManager;
}

//	Fired when a ts packet is received
void CSkyChannelParser::OnTsPacket(CTsHeader& header, byte* tsPacket)
{
	Mediaportal::CEnterCriticalSection enter(skyManager->criticalSection);

	unsigned short pid = header.Pid;

	//	If we are grabbing epg
	if(skyManager->IsEpgGrabbingActive())
	{
		//	If channel network data
		if(skyManager->DoesPidCarryChannelNetworkData(pid))
			channelNetworkDataSectionDecoder->OnTsPacket(header, tsPacket);
	}
}

//	Fired when a new section is received
void CSkyChannelParser::OnNewSection(int pid, int tableId, CSection& section)
{
	Mediaportal::CEnterCriticalSection enter(skyManager->criticalSection);

	//	If we are grabbing epg
	if(skyManager->IsEpgGrabbingActive())
	{
		//	If channel network data
		if(skyManager->DoesPidCarryChannelNetworkData(pid))
			skyChannelDecoder->OnNetworkDataSectionReceived(pid, &section);
	}
}

