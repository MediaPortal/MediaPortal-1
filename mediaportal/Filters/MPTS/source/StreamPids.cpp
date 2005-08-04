/*
	MediaPortal TS-SourceFilter by Agree

	
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
	VideoPid=0;
	AudioPid=0;
	AudioPid2=0;
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