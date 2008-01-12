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
#include <windows.h>
#include "TeletextServiceInfo.h"
#include <vector>

class CPidTable
{
public:

  CPidTable();
  CPidTable(const CPidTable& pids);
  virtual ~CPidTable();
  void Reset();

  bool HasTeletextPageInfo(int page); // do we have a TeletextServiceInfo entry for that page

  CPidTable& operator = (const CPidTable &pids);
  void Copy(const CPidTable &pids);
	ULONG PcrPid;
	ULONG PmtPid;
	WORD VideoPid;
	WORD AudioPid1;
	WORD AudioServiceType1;
	BYTE Lang1_1;
	BYTE Lang1_2;
	BYTE Lang1_3;
	WORD AudioPid2;
	WORD AudioServiceType2;
	BYTE Lang2_1;
	BYTE Lang2_2;
	BYTE Lang2_3;
  WORD AudioPid3;
	WORD AudioServiceType3;
	BYTE Lang3_1;
	BYTE Lang3_2;
	BYTE Lang3_3;
  WORD AudioPid4;
	WORD AudioServiceType4;
	BYTE Lang4_1;
	BYTE Lang4_2;
	BYTE Lang4_3;
  WORD AudioPid5;
	WORD AudioServiceType5;
	BYTE Lang5_1;
	BYTE Lang5_2;
	BYTE Lang5_3;
  WORD AudioPid6;
	WORD AudioServiceType6;
	BYTE Lang6_1;
	BYTE Lang6_2;
	BYTE Lang6_3;
  WORD AudioPid7;
	WORD AudioServiceType7;
	BYTE Lang7_1;
	BYTE Lang7_2;
	BYTE Lang7_3;
	BYTE Lang8_1;
	BYTE Lang8_2;
	BYTE Lang8_3;
	WORD AudioPid8;
	WORD AudioServiceType8;
	WORD TeletextPid; // which PID contains the teletext data
	//WORD TeletextSubPage; // which page is the primary subtitles on
	//char TeletextSubLang[3]; // which language are they in
	std::vector<TeletextServiceInfo> TeletextInfo;
	WORD SubtitlePid1;
	BYTE SubLang1_1;
	BYTE SubLang1_2;
	BYTE SubLang1_3;
	WORD SubtitlePid2;
	BYTE SubLang2_1;
	BYTE SubLang2_2;
	BYTE SubLang2_3;
	WORD SubtitlePid3;
	BYTE SubLang3_1;
	BYTE SubLang3_2;
	BYTE SubLang3_3;
	WORD SubtitlePid4;
	BYTE SubLang4_1;
	BYTE SubLang4_2;
	BYTE SubLang4_3;
	int  ServiceId;
  int  videoServiceType;
};
