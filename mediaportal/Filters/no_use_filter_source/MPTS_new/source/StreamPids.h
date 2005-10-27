/*
	MediaPortal TS-SourceFilter by Agree

	
*/



#ifndef __StreamPids
#define __StreamPids
#include <strmif.h>
#include <vector>
#include <string>
using namespace std;
// channel info


class StreamPids
{
public:
	StreamPids();
	virtual ~StreamPids();

	void Clear();
	void AddPid(int pid);
	void CopyFrom(StreamPids *StreamPids);
	void CopyTo(StreamPids *StreamPids);
	int GetPIDCount();
	int CurrentAudioPid;
	int VideoPid;
	int AudioPid1;
	int AudioPid2;
	int AudioPid3;
	string AudioLanguage1;
	string AudioLanguage2;
	string AudioLanguage3;
	string AC3Language;
	int PMTPid;
	int PCRPid;
	int AC3;
	int ac3_2;
	bool MPEG4;
	GUID idMPEG4;
	long ProgramNumber;
	__int64 StartPTS;
	__int64 EndPTS;
	__int64 Duration;
	__int64 DurTime;
	__int64 fileStartPosition;
	int PIDArray[255];
	long bitrate;
private:
	int streamPidsCount;

};

#endif
