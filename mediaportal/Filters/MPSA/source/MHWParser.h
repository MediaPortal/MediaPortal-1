#pragma once
#include <vector>
#include <string>
using namespace std;

class CMHWParser
{

	struct MHWChannel
	{
		int		NetworkID;
		int		TransponderID;
		int		ChannelID;
		string	ChannelName;
	};
public:
	CMHWParser(void);
	~CMHWParser(void);
	void ParseChannels(byte* byData, int nLen);
	void ParseSummaries(byte* data, int dataLen);
	void ParseTitles(byte* data, int dataLen);
	void ParseThemes(byte* data, int dataLen);
private:
	vector<MHWChannel> m_vecChannels;
};
