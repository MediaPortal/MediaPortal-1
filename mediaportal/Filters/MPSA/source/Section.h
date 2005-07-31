/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Sections_
#define __Sections_
#include <map>
#include <string>
using namespace std;

#pragma warning(disable: 4511 4512 4995)
class Sections
{
public:
	struct EPGEvent
	{
		unsigned int eventid;
		unsigned long date;
		unsigned long time;
		unsigned long duration;
		unsigned int running_status;
		unsigned int free_CA_mode;
		string event;
		string text;
		string genre;
	};

	struct EPGChannel
	{
		bool allSectionsReceived;
		int last_section_number;
		int original_network_id;
		int transport_id;
		int service_id;
		map<unsigned int,EPGEvent> mapEvents;
		typedef map<unsigned int,EPGEvent>::iterator imapEvents;

		map<int,bool> mapSectionsReceived;
		typedef map<int,bool>::iterator imapSectionsReceived;
	};

	struct audioHeader
    {
      //AudioHeader
		int ID;
        int Emphasis;
        int Layer;
        int ProtectionBit;
        int Bitrate;
        int SamplingFreq;
        int PaddingBit;
        int PrivateBit;
        int Mode;
        int ModeExtension;
        int Bound;
        int Channel;
        int Copyright;
        int Original;
        int TimeLength;
        int Size;
        int SizeBase;
	};

	typedef audioHeader AudioHeader;
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
		BYTE Lang1_1;//20
		BYTE Lang1_2;//21
		BYTE Lang1_3;//22
		WORD AudioPid2;//24-25
		BYTE Lang2_1;//26
		BYTE Lang2_2;//27
		BYTE Lang2_3;//28
        WORD AudioPid3;//30-31
		BYTE Lang3_1;//32
		BYTE Lang3_2;//33
		BYTE Lang3_3;//34
		WORD AC3;//36-37
		WORD Teletext;//38-39
		WORD Subtitles;//40-41
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
		PidTable Pids;// 16-41
		BYTE ServiceName[255];// 42-296
		BYTE ProviderName[255];// 297-551
		WORD EITPreFollow;// 552-553
		WORD EITSchedule;// 554-555
		WORD Scrambled;// 556-567
		WORD ServiceType;// 568-569
		ULONG NetworkID;// 560-563
		BYTE PMTReady;// 564
		BYTE SDTReady;// 565
		BYTE PhysicalChannel;//566
		WORD MajorChannel;//568-569
		WORD MinorChannel;//570-571
		WORD Modulation;//572-573
		ULONG Frequency;//574-578
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
	bool	m_bParseEPG;
	bool	m_bEpgDone;
	time_t  m_epgTimeout;
public:
	Sections();
	virtual ~Sections();
	void	Reset();
	void	GrabEPG();
	bool	IsEPGReady();
	bool	IsEPGGrabbing();
	ULONG GetEPGChannelCount( );
	ULONG  GetEPGEventCount( ULONG channel);
	void GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  );
	void GetEPGEvent( ULONG channel,  ULONG event, ULONG* date, ULONG* time, ULONG* duration, char** strevent,  char** strtext, char** strgenre    );

	ULONG GetCRC32(BYTE *pData,WORD len);
	HRESULT GetTSHeader(BYTE *data,TSHeader *header);
	HRESULT GetPESHeader(BYTE *data,PESHeader *header);
	HRESULT CheckStream(void);
	HRESULT ParseFromFile(void);
	void GetPTS(BYTE *data,ULONGLONG *pts);
	void PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime);
	HRESULT CurrentPTS(BYTE *pData,ULONGLONG *ptsValue,int *streamType);
	int decodePMT(BYTE *pData,ChannelInfo *ch, int len);
	void decodePAT(BYTE *pData,ChannelInfo *,int *, int len);
	int decodeSDT(BYTE *buf,ChannelInfo *,int, int len);
	void DVB_GetService(BYTE *b,ServiceData *sd);
	void getString468A(BYTE *b, int l1,char *text);
	WORD CISize();

	//ATSC
	void ATSCDecodeChannelTable(BYTE *buf,ChannelInfo *ch, int* channelsFound);
	void Sections::DecodeServiceLocationDescriptor( byte* buf,int start,ChannelInfo* channelInfo);
	void DecodeExtendedChannelNameDescriptor( byte* buf,int start,ChannelInfo* channelInfo);
	char* DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes);
	char* DecodeMultipleStrings(byte* buf, int offset);

	//pes
	void GetPES(BYTE *data,ULONG len,BYTE *pes);
	HRESULT ParseAudioHeader(BYTE *data,AudioHeader *header);
	void DecodeEPG(byte* pbData,int len);
	void DecodeShortEventDescriptor(byte* buf,EPGEvent& event);
	void DecodeContentDescription(byte* buf,EPGEvent& event);

	map<unsigned long,EPGChannel> m_mapEPG;
	typedef map<unsigned long,EPGChannel>::iterator imapEPG;
};

#endif
