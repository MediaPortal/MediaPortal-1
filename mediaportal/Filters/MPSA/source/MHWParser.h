#pragma once
#include <vector>
#include <map>
#include <string>
using namespace std;

#include "criticalsection.h"
using namespace Mediaportal;
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
		int		TransportStreamID;
		int		NetworkID;
	};
public:
	CMHWParser(void);
	~CMHWParser(void);
	void Reset();
	bool ParseChannels(byte* byData, int nLen);
	bool ParseSummaries(byte* data, int dataLen);
	bool ParseTitles(byte* data, int dataLen);
	bool ParseThemes(byte* data, int dataLen);

	int GetTitleCount();
	void GetTitle(int program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	void GetChannel(WORD channelNr, WORD* channelId,  WORD* networkId, WORD* transportId, char** channelName);
	void GetSummary(WORD programId, char** summary);
	void GetTheme(WORD themeId, char** theme);

private:
	CCriticalSection m_critSection;
	vector<MHWChannel> m_vecChannels;
	typedef vector<MHWChannel>::iterator imapChannels;

	map<int,MHWSummary> m_mapSummaries;
	typedef map<int,MHWSummary> ::iterator imapSummaries;

	vector<MHWProgramm> m_vecTitles;
	typedef vector<MHWProgramm> ::iterator ivecTitles;

	map<ULONG,int> m_mapTitles;
	typedef map<ULONG,int> ::iterator imapTitles;

	vector<MHWTheme> m_vecThemes;
	typedef vector<MHWTheme> ::iterator ivecThemes;
};
