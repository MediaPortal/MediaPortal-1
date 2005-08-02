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
		ULONG   dateStart;
		ULONG   timeStart;
		bool	Summaries;
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

	int GetTitleCount();
	void GetTitle(int program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	void GetChannel(WORD channelId, WORD* networkId, WORD* transportId, char** channelName);
	void GetSummary(WORD programId, char** summary);
	void GetTheme(WORD themeId, char** theme);

private:
	vector<MHWChannel> m_vecChannels;
	typedef vector<MHWChannel>::iterator imapChannels;

	map<int,MHWSummary> m_mapSummaries;
	typedef map<int,MHWSummary> ::iterator imapSummaries;

	map<int,MHWProgramm> m_mapTitles;
	typedef map<int,MHWProgramm> ::iterator imapTitles;

	vector<MHWTheme> m_vecThemes;
	typedef vector<MHWTheme> ::iterator ivecThemes;
};
