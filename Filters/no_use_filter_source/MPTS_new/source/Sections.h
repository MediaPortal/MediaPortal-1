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

	typedef struct AdaptionHeader
	{
		bool DiscontinuityIndicator;
		bool RandomAccessIndicator;
		bool ElementaryStreamPriorityIndicator;
		bool PCRFlag;
		bool OPCRFlag;
		bool SplicingPointFlag;
		bool TransportPrivateData;
		bool AdaptationHeaderExtension;
		unsigned short Len;
		ULONGLONG PCRValue;
		WORD PCRCounter;
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
		ULONGLONG h;
		ULONGLONG m;
		ULONGLONG s;
		ULONGLONG u;
	};

private:
	void FindPATPMT();
	bool DecodePMT(byte *pData);
	bool FindVideo();
	string DVB_GetMPEGISO639Lang (byte *b);
public:

	Sections();
	Sections(FileReader *fileReader);
	virtual ~Sections();
	HRESULT GetTSHeader(BYTE *data,TSHeader *header);
	HRESULT GetPESHeader(BYTE *data,PESHeader *header);
	HRESULT GetAdaptionHeader(BYTE *data,AdaptionHeader *header);
	HRESULT CheckStream();
	HRESULT ParseFromFile(void);
	void	GetPTS(BYTE *data,ULONGLONG *pts);
	void	PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime);
	HRESULT CurrentPTS(char* debugTxt, BYTE *pData,ULONGLONG &ptsValue);
	void	ResetBuffers();
public:
	StreamPids pids;
	FileReader *m_pFileReader;

};

#endif
