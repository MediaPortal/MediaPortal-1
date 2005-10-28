#pragma once
#include <map>
using namespace std;
#define AV_NOPTS_VALUE __int64(0x8000000000000000)
#define TS_PACKET_SIZE 188
#define PES_START_SIZE 9
#define MAX_PES_HEADER_SIZE (9 + 255)

class TsDemux
{
private:

	enum MpegTSState {
		MPEGTS_HEADER = 0,
		MPEGTS_PESHEADER_FILL,
		MPEGTS_PAYLOAD,
		MPEGTS_SKIP,
	};
	enum StreamType
	{
		STREAM_VIDEO,
		STREAM_AUDIO,
		STREAM_PRIVATE
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
			int				 pid;
			int				 last_cc; /* last cc code (-1 if first packet) */
			MpegTSState		 state;
			int				 data_index;
			int				 total_size;
			int				 pes_header_size;
			__int64			 pts, dts;			// pts/dts values
			bool			 m_scrambled;		// true if stream is scrambled
			StreamType		 streamType;		// stream type : video, audio or private
			
			//video stream properties
			Mpeg2FrameType	 frameType;			// type of last frame: I,B,P
			__int64			 averageTimePerFrame;
			int				 videoWidth;
			int				 videoHeight;
			Mpeg2AspectRatio videoAspectRatio;
			long			 videoBitRate;
			int				 videoframeRate;
			byte			 header[MAX_PES_HEADER_SIZE];
			byte			 m_videoPacket[100000];
			int				 m_videoPacketLen;
	 };

public:
	TsDemux(void);
	virtual ~TsDemux(void);
	bool ParsePacket(byte* tsPacket, bool& isStart);

	int GetVideoPacket(int videoPid,byte* packet);
private:
	__int64 get_pts(const byte *p);
	void	DecodePesPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void	DecodeVideoPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void	ParsePacket(MpegTSFilter* tss,const byte *buf, int buf_size, int is_start);

	map<int , MpegTSFilter*> m_mapFilters;
	typedef map<int , MpegTSFilter*>::iterator imapFilters;
public:
};
