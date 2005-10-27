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
	class MpegTSFilter 
	{
		public:
			int pid;
			int last_cc; /* last cc code (-1 if first packet) */
			MpegTSState state;
			int data_index;
			int total_size;
			int pes_header_size;
			__int64 pts, dts;
			bool m_scrambled;
			StreamType streamType;
			Mpeg2FrameType frameType;
			byte header[MAX_PES_HEADER_SIZE];
			
	 };

public:
	TsDemux(void);
	virtual ~TsDemux(void);
	bool ParsePacket(byte* tsPacket, bool& isStart);

private:
	__int64 get_pts(const byte *p);
	void DecodePesPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void DecodeVideoPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen);
	void ParsePacket(MpegTSFilter* tss,const byte *buf, int buf_size, int is_start);

	map<int , MpegTSFilter*> m_mapFilters;
	typedef map<int , MpegTSFilter*>::iterator imapFilters;
};
