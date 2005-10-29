#pragma once
#include "sections.h"

#include <map>
using namespace std;
#define AV_NOPTS_VALUE __int64(0x8000000000000000)
#define TS_PACKET_SIZE 188
#define PES_START_SIZE 9
#define MAX_PES_HEADER_SIZE (9 + 255)

class TsDemux
{
private:

	enum MpegTSState 
	{
		PesHeader = 0,
		PesHeaderFill,
		PesPayLoad,
		PesSkip,
	};
	enum StreamType
	{
		VideoStream,
		AudioStream,
		PrivateStream
	};
	enum Mpeg2FrameType
	{
		IFrame=1,
		PFrame=2,
		BFrame=3,
		DFrame=4
	};
	enum Mpeg2AspectRatio
	{
		forbidden=0,
		Ratio_1_1=1,
		Ratio_4_3=2,
		Ratio_16_9=3,
		Ratio_221_1=4,
		Reserved5=5,
		Reserved6=6,
		Reserved7=7,
		Reserved8=8,
		Reserved9=9,
		Reserved10=10,
		Reserved11=11,
		Reserved12=12,
		Reserved13=13,
		Reserved14=14,
		Reserved15=15,
	};
	class MpegTSFilter 
	{
		public:
			int				 m_pid;
			int				 m_last_cc; /* last cc code (-1 if first packet) */
			MpegTSState		 m_state;
			int				 m_data_index;
			int				 m_total_size;
			int				 m_pes_header_size;
			__int64			 m_pts, m_dts;		// pts/dts values
			bool			 m_scrambled;		// true if stream is scrambled
			StreamType		 m_streamType;		// stream type : video, audio or private
			byte			 m_header[MAX_PES_HEADER_SIZE];
			
			//video stream properties
			Mpeg2FrameType	 m_videoFrameType;			// type of last frame: I,B,P
			__int64			 m_averageTimePerFrame;
			int				 m_videoWidth;
			int				 m_videoHeight;
			Mpeg2AspectRatio m_videoAspectRatio;
			long			 m_videoBitRate;
			int				 m_videoframeRate;
			byte			 m_videoPacket[100000];
			int				 m_videoPacketLen;

			//audiostream properties
			byte			 m_audioPacket[100000];
			int				 m_audioPacketLen;
	 };

public:
	TsDemux(void);
	virtual ~TsDemux(void);
	bool	ParsePacket(byte* tsPacket, bool& isStart);
	int		GetVideoPacket(int videoPid,byte* packet);
	int		GetAudioPacket(int audioPid,byte* packet);
	void 	GetVideoAttributes(int videoPid,int& videoWidth, int &videoHeight,Mpeg2AspectRatio& aspectRatio, __int64& averageTimePerFrame,long& videoBitRate,int& videoframeRate);
	void	GetPCRTime(Sections::PTSTime& time);
	void	GetPCRReferenceTime(REFERENCE_TIME& reftime);
	bool    IsNewPicture();
private:
	__int64 get_pts(const byte *p);
	void	DecodePesPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void	DecodeVideoPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void	DecodeAudioPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void	ParsePacket(MpegTSFilter* tss,const byte *buf, int buf_size, int is_start);

	map<int , MpegTSFilter*> m_mapFilters;
	typedef map<int , MpegTSFilter*>::iterator imapFilters;
	ULONGLONG		 m_pcrNow;
	ULONGLONG		 m_pcrStart;
	ULONGLONG		 m_pcrPrev;
	double  		 m_pcr_incr;
	double  		 m_pcrOff;
	int				 m_packetCount;
	bool			 m_bNewPicture;
};
