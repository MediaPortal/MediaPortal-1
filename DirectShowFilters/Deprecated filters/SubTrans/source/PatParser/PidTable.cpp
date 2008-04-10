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
#include <windows.h>
#include "PidTable.h"

CPidTable::CPidTable(void)
{
  Reset();
}

CPidTable::~CPidTable(void)
{
}

void CPidTable::Reset()
{
  PcrPid=0;
  PmtPid=0;
  VideoPid=0;
  AudioPid1=0;
  Lang1_1=0;
  Lang1_2=0;
  Lang1_3=0;
  AudioPid2=0;
  Lang2_1=0;
  Lang2_2=0;
  Lang2_3=0;
  AudioPid3=0;
  Lang3_1=0;
  Lang3_2=0;
  Lang3_3=0;
  AC3Pid=0;
  TeletextPid=0;
  SubtitlePid=0;
  ServiceId=-1;
	videoServiceType=-1;
}

CPidTable CPidTable::operator = (const CPidTable &pids)
{
  PcrPid=pids.PcrPid;
  PmtPid=pids.PmtPid;
  VideoPid=pids.VideoPid;
  AudioPid1=pids.AudioPid1;
  Lang1_1=pids.Lang1_1;
  Lang1_2=pids.Lang1_2;
  Lang1_3=pids.Lang1_3;
  AudioPid2=pids.AudioPid2;
  Lang2_1=pids.Lang2_1;
  Lang2_2=pids.Lang2_2;
  Lang2_3=pids.Lang2_3;
  AudioPid3=pids.AudioPid3;
  Lang3_1=pids.Lang3_1;
  Lang3_2=pids.Lang3_2;
  Lang3_3=pids.Lang3_3;
  AC3Pid=pids.AC3Pid;
  TeletextPid=pids.TeletextPid;
  SubtitlePid=pids.SubtitlePid;
  ServiceId=pids.ServiceId;
	videoServiceType=pids.videoServiceType;

  return *this;
}