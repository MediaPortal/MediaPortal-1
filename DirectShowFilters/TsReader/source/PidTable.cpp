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

bool CPidTable::operator ==(const CPidTable& other) const
{
  // Not all members are compared, this is how DeMultiplexer class has 
  // been comparing "PMTs" to detect channel changes.
  if(subtitlePids != other.subtitlePids
    || audioPids != other.audioPids 
    || videoPids != other.videoPids
    || PcrPid != other.PcrPid
    || PmtPid != other.PmtPid)
  {
    return false;
  }
  else
  {
    return true;
  }
}

void CPidTable::Reset()
{
	//LogDebug("Pid table reset");
  PcrPid=0;
  PmtPid=0;
  ServiceId=-1;
  
  videoPids.clear();
  audioPids.clear();
  subtitlePids.clear();

  TeletextPid=0;
  // no reason to reset TeletextSubLang
}

CPidTable& CPidTable::operator = (const CPidTable &pids)
{
  if (&pids==this)
  {
    return *this;
  }
  Copy(pids);
  return *this;
}

void CPidTable::Copy(const CPidTable &pids)
{
  //LogDebug("Pid table copy");
  ServiceId=pids.ServiceId;

  PcrPid=pids.PcrPid;
  PmtPid=pids.PmtPid;

  videoPids=pids.videoPids;
  audioPids=pids.audioPids;
  subtitlePids=pids.subtitlePids;

  TeletextPid=pids.TeletextPid;
  TeletextInfo=pids.TeletextInfo;
}

bool CPidTable::HasTeletextPageInfo(int page)
{
	std::vector<TeletextServiceInfo>::iterator vit = TeletextInfo.begin();
	while(vit != TeletextInfo.end())
  { // is the page already registrered
		TeletextServiceInfo& info = *vit;
		if(info.page == page)
    {
			return true;
			break;
		}
		else vit++;
	}
	return false;
}