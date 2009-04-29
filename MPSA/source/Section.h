/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#ifndef __Sections_
#define __Sections_
#include <map>
#include <vector>
#include <string>
using namespace std;
#define MAX_PAT_TABLES 10
//
// nit structs
typedef  struct stNITLCN
{
	int network_id;
	int transport_id;
	int service_id;
	int LCN;
}NITLCN;

typedef  struct stNITSatDescriptor
{
	int Frequency;
	float OrbitalPosition;
	int WestEastFlag;
	int Polarisation;
	int Modulation;
	int Symbolrate;
	int FECInner;
	string NetworkName;
}NITSatDescriptor;
//
typedef  struct stNITCableDescriptor
{
	int Frequency;
	int FECOuter;
	int Modulation;
	int Symbolrate;
	int FECInner;
	string NetworkName;
}NITCableDescriptor;

//
typedef struct stNITTerrestrialDescriptor
{
	int CentreFrequency;
	int Bandwidth;
	int Constellation;
	int HierarchyInformation;
	int CoderateHPStream;
	int CoderateLPStream;
	int GuardInterval;
	int TransmissionMode; 
	int OtherFrequencyFlag;
	string NetworkName;
}NITTerrestrialDescriptor;

typedef struct stDVBNetworkInfo
{
	vector<NITSatDescriptor>		  satteliteNIT;
	vector<NITCableDescriptor>		  cableNIT;
	vector<NITTerrestrialDescriptor>  terrestialNIT;
	vector<NITLCN>					  lcnNIT;
	string							  NetworkName;

}DVBNetworkInfo;

typedef struct staudioHeader
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
}AudioHeader;

//
//
typedef struct stserviceData
{
	char Provider[255];
	char Name[255];
	WORD ServiceType;
}ServiceData;

//
//
typedef struct stpidData
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
}PidTable;
//
//
typedef struct chInfo
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
	WORD LCN;//580-581
}ChannelInfo;

#pragma warning(disable: 4511 4512 4995)
class Sections
{
public:

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

public:
	Sections();
	virtual ~Sections();

	//reset
	void	ResetPAT();


	//decode
	bool	IsNewPat(BYTE *pData, int len);
	void	decodePAT(BYTE *pData,ChannelInfo *,int *, int len);
	int		decodePMT(BYTE *pData,ChannelInfo *ch, int len);
	int		decodeSDT(BYTE *buf,ChannelInfo *,int, int len);
	void	decodeNITTable(byte* buf,ChannelInfo *channels, int channelCount);
	HRESULT ParseAudioHeader(BYTE *data,AudioHeader *header);
	HRESULT GetLCN(WORD channel,WORD* networkId, WORD* transportId, WORD* serviceID, WORD* LCN);

	//helper
	HRESULT GetTSHeader(BYTE *data,TSHeader *header);
	HRESULT GetPESHeader(BYTE *data,PESHeader *header);
	WORD	CISize();
	ULONG	GetSectionCRCValue(byte* data,int ptr);
	ULONG	GetCRC32(BYTE *pData,WORD len);
	HRESULT CheckStream(void);
	HRESULT ParseFromFile(void);
	void	GetPTS(BYTE *data,ULONGLONG *pts);
	void	PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime);
	HRESULT CurrentPTS(BYTE *pData,ULONGLONG *ptsValue,int *streamType);
	void	DVB_GetService(BYTE *b,ServiceData *sd);
	static void	getString468A(BYTE *b, int l1,char *text);
	void	DVB_GetLogicalChannelNumber(int original_network_id,int transport_stream_id,byte* b,ChannelInfo *channels, int channelCount);
	void	DVB_GetSatDelivSys(byte* b, int maxLen);
	void	DVB_GetTerrestrialDelivSys(byte*b , int maxLen);
	void	DVB_GetCableDelivSys(byte* b, int maxLen);
	//pes
	void GetPES(BYTE *data,ULONG len,BYTE *pes);

private:
	int		m_patsFound;
	int     m_patTableVersion[MAX_PAT_TABLES];
	int     m_patTSID[MAX_PAT_TABLES];
	int     m_patSectionLen[MAX_PAT_TABLES];
	
	DVBNetworkInfo m_nit;

    CCritSec					m_Lock;                // Main renderer critical section	
};

#endif
