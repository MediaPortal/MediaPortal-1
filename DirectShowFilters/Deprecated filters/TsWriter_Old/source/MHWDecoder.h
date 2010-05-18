/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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
