// TS Mux Splitter Copyright by Agree / Team MediaPortal 2005
//
//
//

#ifndef __DEMUXING_
#define __DEMUXING_

#include <map>
#include <vector>
#include <string>

class Demux;
class SplitterOutputPin;


class Demux
{
public:

	typedef struct PTSTime
	{
		int h;
		int m;
		int s;
		int u;
	};
	
	typedef struct stVideoHeader
	{
    //AudioHeader
		int		vWidth;
		int		vHeight;
		int		vFrameRate;
		int		vFrameType;
		int		vOrgFrameRate;
		__int64 vBitrate;
		char	vAspectRatio[10];
		int		vAspectRatioValue;
		bool	IsGOPStart; 
		bool	IsSeqHeader;

		int		TempSeqNumber;
		int		VBVDelay;
		bool	Done;
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

	HRESULT GetPESHeader(BYTE *data,PESHeader *header);
	HRESULT GetTSHeader(BYTE *data,TSHeader *header);
	
	Demux();
	
	static LONGLONG GetPTS(const BYTE *data);

 	HRESULT Process(const BYTE* pData, long cBytes,void*);

	bool CheckTSFile(IAsyncReader* pRdr);

    // discards all data
    void Empty();

	//
	HRESULT ParseAudioHeader(BYTE *data,AudioHeader *header);
	HRESULT ParseVideoHeader(BYTE *videoPacket,ULONG length,VideoHeader *vh);
	HRESULT ParseAC3Header(BYTE *data,AudioHeader *header);
	void PTSToPTSTime(LONGLONG pts,PTSTime* ptsTime);
	DWORD GetVideoPID(void)
	{
		return m_videoPID;
	}
	DWORD GetAudioPID(void)
	{
		return m_audioPID;
	}
	DWORD GetFileOffset(void)
	{
		return m_StartOffset;
	}
public:
	// for timestamps
	LONGLONG	m_firstPTS;
	LONGLONG	m_lastPTS;
	LONGLONG	m_currentAudioPTS;
	LONGLONG	m_currentVideoPTS;
	ULONGLONG	m_remainingBytes;

	// video and audio stream info
	VideoHeader m_VideoHeader;
	AudioHeader m_AudioHeader;

	// 
	bool	m_isAC3Audio;
	bool	m_checkSyncWord;
	

private:
    enum
    {
        ParseBufferSize = 81780,
    };

    BYTE		m_ParseBuffer[ParseBufferSize];
	BYTE		m_AudioStreamBuffer[65536];
	BYTE		m_VideoStreamBuffer[65536];
	LONGLONG	m_audioPointer;
	LONGLONG	m_videoPointer;
	DWORD		m_currentAudioPackLen;
	DWORD		m_videoPID;
	DWORD		m_audioPID;

	// its the offset to the first ts packet
	DWORD		m_StartOffset;


};
#endif