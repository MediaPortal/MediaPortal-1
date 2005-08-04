/*
	MediaPortal TS-SourceFilter by Agree

	
*/



#ifndef __StreamPids
#define __StreamPids

#include <vector>

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
	int VideoPid;
	int AudioPid;
	int AudioPid2;
	int PMTPid;
	int PCRPid;
	int AC3;
	int ac3_2;
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
