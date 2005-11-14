/*
	MediaPortal TS-SourceFilter by Agree
*/

#ifndef __Sections
#define __Sections

class Sections
{
public:
	typedef struct stVideoHeader
	{
    //AudioHeader
		int vWidth;
		int vHeight;
		int vFrameRate;
		int vFrameType;
		int vOrgFrameRate;
		__int64 vBitrate; 
		bool IsGOPStart; 
		bool IsSeqHeader;
		int TempSeqNumber;
		int VBVDelay;
	}VideoHeader;

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
		int h;
		int m;
		int s;
		int u;
	};

private:

public:

	Sections();
	virtual ~Sections();
	HRESULT GetTSHeader(BYTE *data,TSHeader *header);
	HRESULT GetPESHeader(BYTE *data,PESHeader *header);
	HRESULT GetAdaptionHeader(BYTE *data,AdaptionHeader *header);
	void	GetPTS(BYTE *data,ULONGLONG *pts);
	void	PTSToPTSTime(ULONGLONG pts,PTSTime* ptsTime);
	HRESULT ParseAudioHeader(BYTE *data,AudioHeader *head);

public:

}

#endif
