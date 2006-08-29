/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#include "sectiondecoder.h"
#include "isectioncallback.h"
#include "mhwdecoder.h"
#include <vector>
using namespace std;

class CMhwParser:public ISectionCallback
{
public:
	CMhwParser(void);
	virtual ~CMhwParser(void);
  void  Reset();
	void  GrabEPG();
	bool  isGrabbing();
	bool  IsEPGReady();
	void GetTitleCount(WORD* count);
	void GetTitle(WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	void GetChannel(WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName);
	void GetSummary(WORD programId, char** summary);
	void GetTheme(WORD themeId, char** theme);

	void	OnTsPacket(byte* tsPacket);
	void  OnNewSection(int pid, int tableId, CSection& section);
private:
  vector<CSectionDecoder*> m_vecDecoders;
	CMhwDecoder m_mhwDecoder;
	time_t      m_TimeOutTimer;
	bool				m_bGrabbing;
	bool				m_bDone;
};

