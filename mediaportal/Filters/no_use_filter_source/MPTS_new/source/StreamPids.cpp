/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
#include "StreamPids.h"
#include <crtdbg.h>
#include <streams.h>

StreamPids::StreamPids()
{
	Clear();
}

StreamPids::~StreamPids()
{
}

void StreamPids::Clear()
{
	AudioLanguage1=AudioLanguage2=AudioLanguage3=AC3Language="";
	CurrentAudioPid=0;
	VideoPid=0;
	AudioPid1=0;
	AudioPid2=0;
	AudioPid3=0;
	PMTPid=0;
	PCRPid=0;
	AC3=0;
	StartPTS=0;
	EndPTS  = 0;
	Duration = 0; // duration in 100 nano sec.
	DurTime=0; // the duration in normal time= h:m:s.h
	ProgramNumber=0;
	fileStartPosition=0;
	streamPidsCount=0;
	for (int i = 0 ; i < 255 ; i++ )
	{
		PIDArray[i] = 0;
	}

}
void StreamPids::AddPid(int pid)
{

	if(streamPidsCount>=255)
		return;
	for (int i = 0 ; i < streamPidsCount ; i++ )
	{
		if(PIDArray[i]==pid)
			return;
	}
	PIDArray[streamPidsCount]=pid;
	streamPidsCount+=1;
}

int StreamPids::GetPIDCount()
{
	return streamPidsCount;
}
