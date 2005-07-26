/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Sections_
#define __Sections_

class Sections
{
public:
	//
	//
	struct serviceData
	{
		char Provider[255];
		char Name[255];
		WORD ServiceType;
	};
	typedef serviceData ServiceData;
	//
	//
	struct pidData
	{
		WORD VideoPid;//16-17
		WORD AudioPid1;//18-19
		BYTE Lang1_1;//20-22
		BYTE Lang1_2;//20-22
		BYTE Lang1_3;//20-22
		WORD AudioPid2;//23-24
		BYTE Lang2_1;//20-22
		BYTE Lang2_2;//20-22
		BYTE Lang2_3;//20-22
        WORD AudioPid3;//28-29
		BYTE Lang3_1;//20-22
		BYTE Lang3_2;//20-22
		BYTE Lang3_3;//20-22
		WORD AC3;//33-34
		WORD Teletext;//35-36
		WORD Subtitles;//37-38
	};
	typedef pidData PidTable;
	//
	//
	struct chInfo
	{
		ULONG TransportStreamID;// 0-3
		ULONG ProgrammNumber;// 4-7
		ULONG ProgrammPMTPID;// 8-11
		ULONG PCRPid;// 12-15
		PidTable Pids;// 16-38
		BYTE ServiceName[255];// 39-293
		BYTE ProviderName[255];// 294-548
		WORD EITPreFollow;// 549-550
		WORD EITSchedule;// 551-552
		WORD Scrambled;// 553-554
		WORD ServiceType;// 555-556
		ULONG NetworkID;
		BYTE PMTReady;// 557-557
		BYTE SDTReady;// 558-558
	};
	typedef chInfo ChannelInfo;
	//
	//
	struct tsheader
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
	typedef tsheader TSHeader;
	// 
	//
	struct pesheader
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
	typedef pesheader PESHeader;
	//
	//
	struct timedata
	{
		int h;
		int m;
		int s;
		int u;
	};
	typedef timedata PTSTime;

private:
public:

	Sections();
	virtual ~Sections();
	ULONG GetCRC32(BYTE *pData,WORD len);
	HRESULT GetTSHeader(BYTE *data,TSHeader *header);
	HRESULT GetPESHeader(BYTE *data,PESHeader *header);
	HRESULT CheckStream(void);
	HRESULT ParseFromFile(void);
	void GetPTS(BYTE *data,ULONGLONG *pts);
	void PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime);
	HRESULT CurrentPTS(BYTE *pData,ULONGLONG *ptsValue,int *streamType);
	int decodePMT(BYTE *pData,ChannelInfo *ch);
	void decodePAT(BYTE *pData,ChannelInfo *,int *);
	int decodeSDT(BYTE *buf,ChannelInfo *,int);
	void DVB_GetService(BYTE *b,ServiceData *sd);
	void getString468A(BYTE *b, int l1,char *text);
	WORD CISize();
};

#endif
