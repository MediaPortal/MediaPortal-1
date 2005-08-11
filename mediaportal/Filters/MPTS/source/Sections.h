/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Sections
#define __Sections
#include "StreamPids.h"
#include "FileReader.h"

class Sections
{
public:
	typedef struct ChannelInfo
	{
		ULONG TransportStreamID;
		ULONG ProgrammNumber;
		ULONG ProgrammPMTPID;
	};

	typedef struct TSHeader
	{
		BYTE SyncByte			;
		bool TransportError		;
		bool PayloadUnitStart	;
		bool TransportPriority	;
		unsigned short Pid		;
		BYTE TScrambling		;
		BYTE AdaptionControl	;
		BYTE ContinuityCounter	;
	};
	// pes
	typedef struct PESHeader
	{
		BYTE     Reserved				;
		BYTE     ScramblingControl		;
		BYTE     Priority 				;
		BYTE     dataAlignmentIndicator	;
		BYTE     Copyright				;
		BYTE     Original				;
		BYTE     PTSFlags				;
		BYTE     ESCRFlag				;
		BYTE     ESRateFlag				;
		BYTE     DSMTrickModeFlag		;
		BYTE     AdditionalCopyInfoFlag	;
		BYTE     PESCRCFlag				;
		BYTE     PESExtensionFlag		;
		BYTE     PESHeaderDataLength	;
	};
	typedef struct PTSTime
	{
		int h;
		int m;
		int s;
		int u;
	};

private:
	void FindPATPMT();
	bool DecodePMT(byte *pData);
	bool FindVideo();
public:

	Sections(FileReader *fileReader);
	virtual ~Sections();
	HRESULT GetTSHeader(BYTE *data,TSHeader *header);
	HRESULT GetPESHeader(BYTE *data,PESHeader *header);
	HRESULT CheckStream(void);
	HRESULT ParseFromFile(void);
	void GetPTS(BYTE *data,ULONGLONG *pts);
	void PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime);
	HRESULT CurrentPTS(BYTE *pData,ULONGLONG *ptsValue,int* pid);
	void ResetBuffers();
public:

	StreamPids pids;
	FileReader *m_pFileReader;

};

#endif
