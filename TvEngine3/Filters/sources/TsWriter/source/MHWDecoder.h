#pragma once
#include <vector>
#include <map>
#include <string>
using namespace std;

#include "criticalsection.h"
using namespace Mediaportal;
class CMhwDecoder
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
		ULONG		ProgramID;// its the programm-id of epg, not an channel id
		string	Description;
	};
	struct MHWTheme
	{
		int ThemeIndex;
		string ThemeText;
	};
	struct MHWProgramm 
	{
		ULONG		ID;
		int		ChannelID;
		int		ThemeID;
		ULONG		PPV;
		ULONG   dateStart;
		ULONG   timeStart;
		bool	Summaries;
		int		Duration;
		string	Title;
		int		TransportStreamID;
		int		NetworkID;
	};
public:
	CMhwDecoder(void);
	~CMhwDecoder(void);
	void Reset();
	bool ParseChannels(byte* byData, int nLen);
	bool ParseSummaries(byte* data, int dataLen);
	bool ParseTitles(byte* data, int dataLen);
	bool ParseThemes(byte* data, int dataLen);

	int GetTitleCount();
	void GetTitle(int program, UINT* id, WORD* transportId, WORD* networkId, WORD* channelId, UINT* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	void GetChannel(WORD channelNr, WORD* channelId,  WORD* networkId, WORD* transportId, char** channelName);
	void GetSummary(UINT programId, char** summary);
	void GetTheme(WORD themeId, char** theme);

private:
	CCriticalSection m_critSection;
	vector<MHWChannel> m_vecChannels;
	typedef vector<MHWChannel>::iterator imapChannels;

	map<ULONG,MHWSummary> m_mapSummaries;
	typedef map<ULONG,MHWSummary> ::iterator imapSummaries;

	vector<MHWProgramm> m_vecTitles;
	typedef vector<MHWProgramm> ::iterator ivecTitles;

	map<ULONG,int> m_mapTitles;
	typedef map<ULONG,int> ::iterator imapTitles;

	vector<MHWTheme> m_vecThemes;
	typedef vector<MHWTheme> ::iterator ivecThemes;
};
