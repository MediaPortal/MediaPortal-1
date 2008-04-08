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
#pragma warning(disable : 4995)
#include <windows.h>
#include "PidTable.h"

void LogDebug(const char *fmt, ...) ; 

CPidTable::CPidTable(const CPidTable& pids)
{
  Copy(pids);
}

CPidTable::CPidTable(void)
{
  Reset();
}

CPidTable::~CPidTable(void)
{
}

void CPidTable::Reset()
{
	//LogDebug("Pid table reset");
  PcrPid=0;
  PmtPid=0;
  VideoPid=0;
  AudioPid1=0;
  Lang1_1=0;
  Lang1_2=0;
  Lang1_3=0;
  AudioServiceType1=0;
  AudioPid2=0;
  Lang2_1=0;
  Lang2_2=0;
  Lang2_3=0;
  AudioServiceType2=0;
  AudioPid3=0;
  Lang3_1=0;
  Lang3_2=0;
  Lang3_3=0;
  AudioServiceType3=0;
  AudioPid4=0;
  Lang4_1=0;
  Lang4_2=0;
  Lang4_3=0;
  AudioServiceType4=0;
  AudioPid5=0;
  Lang5_1=0;
  Lang5_2=0;
  Lang5_3=0;
  AudioServiceType5=0;
  AudioPid6=0;
  Lang6_1=0;
  Lang6_2=0;
  Lang6_3=0;
  AudioServiceType6=0;
  AudioPid7=0;
  Lang7_1=0;
  Lang7_2=0;
  Lang7_3=0;
  AudioServiceType7=0;
  AudioPid8=0;
  Lang8_1=0;
  Lang8_2=0;
  Lang8_3=0;
  AudioServiceType8=0;
  TeletextPid=0;
  // no reason to reset TeletextSubLang
  
  SubtitlePid1=0;
  SubtitlePid2=0;
  SubtitlePid3=0;
  SubtitlePid4=0;
	BYTE SubLang1_1;
	BYTE SubLang1_2;
	BYTE SubLang1_3;
	BYTE SubLang2_1;
	BYTE SubLang2_2;
	BYTE SubLang2_3;
	BYTE SubLang3_1;
	BYTE SubLang3_2;
	BYTE SubLang3_3;
	BYTE SubLang4_1;
	BYTE SubLang4_2;
	BYTE SubLang4_3;

  ServiceId=-1;
	videoServiceType=-1;
}

CPidTable& CPidTable::operator = (const CPidTable &pids)
{
  if (&pids==this) return *this;
  Copy(pids);
  return *this;
}

void CPidTable::Copy(const CPidTable &pids)
{
  //LogDebug("Pid table copy");
  PcrPid=pids.PcrPid;
  PmtPid=pids.PmtPid;
  VideoPid=pids.VideoPid;
  AudioPid1=pids.AudioPid1;
  Lang1_1=pids.Lang1_1;
  Lang1_2=pids.Lang1_2;
  Lang1_3=pids.Lang1_3;
  AudioServiceType1=pids.AudioServiceType1;
  AudioPid2=pids.AudioPid2;
  Lang2_1=pids.Lang2_1;
  Lang2_2=pids.Lang2_2;
  Lang2_3=pids.Lang2_3;
  AudioServiceType2=pids.AudioServiceType2;
  AudioPid3=pids.AudioPid3;
  Lang3_1=pids.Lang3_1;
  Lang3_2=pids.Lang3_2;
  Lang3_3=pids.Lang3_3;
  AudioServiceType3=pids.AudioServiceType3;
  AudioPid4=pids.AudioPid4;
  Lang4_1=pids.Lang4_1;
  Lang4_2=pids.Lang4_2;
  Lang4_3=pids.Lang4_3;
  AudioServiceType4=pids.AudioServiceType4;
  AudioPid5=pids.AudioPid5;
  Lang5_1=pids.Lang5_1;
  Lang5_2=pids.Lang5_2;
  Lang5_3=pids.Lang5_3;
  AudioServiceType5=pids.AudioServiceType5;
  AudioPid6=pids.AudioPid6;
  Lang6_1=pids.Lang6_1;
  Lang6_2=pids.Lang6_2;
  Lang6_3=pids.Lang6_3;
  AudioServiceType6=pids.AudioServiceType6;
  AudioPid7=pids.AudioPid7;
  Lang7_1=pids.Lang7_1;
  Lang7_2=pids.Lang7_2;
  Lang7_3=pids.Lang7_3;
  AudioServiceType7=pids.AudioServiceType7;
  AudioPid8=pids.AudioPid8;
  Lang8_1=pids.Lang8_1;
  Lang8_2=pids.Lang8_2;
  Lang8_3=pids.Lang8_3;
  AudioServiceType8=pids.AudioServiceType8;
  TeletextPid=pids.TeletextPid;
  TeletextInfo=pids.TeletextInfo;

  SubtitlePid1=pids.SubtitlePid1;
  SubLang1_1=pids.SubLang1_1;
  SubLang1_2=pids.SubLang1_2;
  SubLang1_3=pids.SubLang1_3;
  SubtitlePid2=pids.SubtitlePid2;
  SubLang2_1=pids.SubLang2_1;
  SubLang2_2=pids.SubLang2_2;
  SubLang2_3=pids.SubLang2_3;
  SubtitlePid3=pids.SubtitlePid3;
  SubLang3_1=pids.SubLang3_1;
  SubLang3_2=pids.SubLang3_2;
  SubLang3_3=pids.SubLang3_3;
  SubtitlePid4=pids.SubtitlePid4;
  SubLang4_1=pids.SubLang4_1;
  SubLang4_2=pids.SubLang4_2;
  SubLang4_3=pids.SubLang4_3;

  ServiceId=pids.ServiceId;
  videoServiceType=pids.videoServiceType;
}

bool CPidTable::HasTeletextPageInfo(int page){
	std::vector<TeletextServiceInfo>::iterator vit = TeletextInfo.begin();
	while(vit != TeletextInfo.end()){ // is the page already registrered
		TeletextServiceInfo& info = *vit;
		if(info.page == page){
			return true;
			break;
		}
		else vit++;
	}
	return false;
}