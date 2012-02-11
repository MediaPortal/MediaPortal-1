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

#pragma once
#include <vector>
#include <windows.h>
#include "SkyChannelDecoder.h"
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\iSectionCallback.h"
#include "CriticalSection.h"
#include "EnterCriticalSection.h"

using namespace std;

class CSkyManager;


class CSkyChannelParser
	: public ISectionCallback
{
public:

	CSkyChannelParser(CSkyManager* parentSkyManager);
	~CSkyChannelParser();

	//	Gets the parent sky manager reference
	CSkyManager* GetSkyManager();

	//	Fired when a new ts packed is received
	void OnTsPacket(CTsHeader& header, byte* tsPacket);

	//	Fired when a new section is received
	void OnNewSection(int pid, int tableId, CSection& section);

private:

	CSkyManager* skyManager;

	//	Child objects
	CSkyChannelDecoder* skyChannelDecoder;

	CSectionDecoder* channelNetworkDataSectionDecoder;

};
