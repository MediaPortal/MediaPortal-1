#include <windows.h>
#include ".\mhwparser.h"

CMHWParser::CMHWParser(void)
{
}

CMHWParser::~CMHWParser(void)
{
}

void CMHWParser::ParseChannels(byte* data, int dataLen)
{
	for(int n=4;n<dataLen;n+=22)
	{
		if(m_vecChannels.size()>=((dataLen-3)/22))
			break;
		MHWChannel ch;
		ch.NetworkID=(data[n]<<8)+data[n+1];
		ch.TransponderID=(data[n+2]<<8)+data[n+3];
		ch.ChannelID=(data[n+4]<<8)+data[n+5];
		ch.ChannelName=(char*)(&data[n+6]);
		m_vecChannels.push_back(ch);
	}// for(int n=0
}