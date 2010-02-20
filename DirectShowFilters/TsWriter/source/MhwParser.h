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
#include "..\..\shared\sectiondecoder.h"
#include "..\..\shared\isectioncallback.h"
#include "mhwdecoder.h"
#include "criticalsection.h"
#include "entercriticalsection.h"
#include "..\..\shared\Tsheader.h"
#include <vector>
using namespace std;

#define PID_MHW1  0xd2
#define PID_MHW2  0xd3
class CMhwParser:public ISectionCallback
{
public:
	CMhwParser(void);
	virtual ~CMhwParser(void);
  void  Reset();
	void  GrabEPG();
	bool  isGrabbing();
	bool  IsEPGReady();
	void GetTitleCount(UINT* count);
	void GetTitle(UINT program, UINT* id, UINT* transportId, UINT* networkId, UINT* channelId, ULONG* programId, UINT* themeId, UINT* PPV, BYTE* Summaries, UINT* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName);
	void GetChannel(UINT channelNr, UINT* channelId, UINT* networkId, UINT* transportId, char** channelName);
	void GetSummary(ULONG programId, char** summary);
	void GetTheme(UINT themeId, char** theme);
	void AbortGrabbing();

	void	OnTsPacket(CTsHeader& header,byte* tsPacket);
	void  OnNewSection(int pid, int tableId, CSection& section);
private:
	bool IsSectionWanted(int pid, int table_id);
  vector<CSectionDecoder*> m_vecDecoders;
	CMhwDecoder m_mhwDecoder;
	time_t      m_TimeOutTimer;
	bool				m_bGrabbing;
	bool				m_bDone;
	CCriticalSection m_section;
  CTsHeader             m_tsHeader;
};

