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
		long		NetworkID;
		long		TransponderID;
		long		ChannelID;
		string	ChannelName;
	};
	
	struct MHWSummary
	{
		UINT64		ProgramID;// its the programm-id of epg, not an channel id
		string	Description;
	};
	struct MHWTheme
	{
		long ThemeIndex;
		string ThemeText;
	};
	struct MHWProgramm 
	{
		UINT64		ID;
		long		ChannelID;
		long		ThemeID;
		ULONG		PPV;
		ULONG   dateStart;
		ULONG   timeStart;
		bool	Summaries;
		int		Duration;
		string	Title;
		//long		TransportStreamID;
		//long		NetworkID;
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
	void GetTitle(int program, UINT* id, UINT* transportId, UINT* networkId, UINT* channelId, ULONG* programId, UINT* themeId, UINT* PPV, BYTE* Summaries, UINT* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	void GetChannel(UINT channelNr, UINT* channelId,  UINT* networkId, UINT* transportId, char** channelName);
	void GetSummary(ULONG programId, char** summary);
	void GetTheme(UINT themeId, char** theme);

private:
	CCriticalSection m_critSection;
	vector<MHWChannel> m_vecChannels;
	typedef vector<MHWChannel>::iterator imapChannels;

	map<UINT64,MHWSummary> m_mapSummaries;
	typedef map<UINT64,MHWSummary> ::iterator imapSummaries;

	vector<MHWProgramm> m_vecTitles;
	typedef vector<MHWProgramm> ::iterator ivecTitles;

	map<UINT64,int> m_mapTitles;
	typedef map<UINT64,int> ::iterator imapTitles;

	vector<MHWTheme> m_vecThemes;
	typedef vector<MHWTheme> ::iterator ivecThemes;
};
