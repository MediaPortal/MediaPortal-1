#pragma once
#include <vector>
#include <map>
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
	
	struct MHWSummary
	{
		int		ProgramID;// its the programm-id of epg, not an channel id
		string	Description;
	};
	struct MHWTheme
	{
		int ThemeIndex;
		string ThemeText;
	};
	struct MHWProgramm 
	{
		int		ID;
		int		ChannelID;
		int		ThemeID;
		int		PPV;
		//DateTime	Time;
		bool		Summaries;
		int		Duration;
		string	Title;
		int		ProgrammID;
		string	ProgrammName;
		int		TransportStreamID;
		int		NetworkID;
	};
public:
	CMHWParser(void);
	~CMHWParser(void);
	void Reset();
	void ParseChannels(byte* byData, int nLen);
	void ParseSummaries(byte* data, int dataLen);
	void ParseTitles(byte* data, int dataLen);
	void ParseThemes(byte* data, int dataLen);
private:
	vector<MHWChannel> m_vecChannels;
	map<int,MHWSummary> m_mapSummaries;
	typedef map<int,MHWSummary> ::iterator imapSummaries;

	map<int,MHWProgramm> m_mapTitles;
	typedef map<int,MHWProgramm> ::iterator imapTitles;

	vector<MHWTheme> m_vecThemes;
};
