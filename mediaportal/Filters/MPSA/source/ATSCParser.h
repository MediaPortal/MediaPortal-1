#pragma once

#include <map>
#include <string>
using namespace std;
#include "section.h"
#include "splittersetup.h"

class ATSCParser
{
public:
	ATSCParser(void);
	~ATSCParser(void);
	void SetDemuxer(SplitterSetup* demuxer);

	//ATSC
	void ATSCDecodeChannelTable(BYTE *buf,ChannelInfo *ch, int* channelsFound);
	void ATSCDecodeMasterGuideTable(byte* buf, int len, int* channelsFound);
	void ATSCDecodeEPG(byte* buf, int len);
	void Reset();
private:
	void DecodeServiceLocationDescriptor( byte* buf,int start,ChannelInfo* channelInfo);
	void DecodeExtendedChannelNameDescriptor( byte* buf,int start,ChannelInfo* channelInfo);
	char* DecodeString(byte* buf, int offset, int compression_type, int mode, int number_of_bytes);
	char* DecodeMultipleStrings(byte* buf, int offset);
	void ATSCDecodeETT(byte* buf, int len);
	void ATSCDecodeEIT(byte* buf, int len);
	void ATSCDecodeRTT(byte* buf, int len);
	void ATSCDecodeChannelEIT(byte* buf, int len);

	bool masterGuideTableDecoded;
	int  mgSectionLength;
	int  mgSectionNumber;
	int  mgLastSectionNumber;
	SplitterSetup* m_demuxer;
};
