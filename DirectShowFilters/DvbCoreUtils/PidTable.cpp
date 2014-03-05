/* 
 *  Copyright (C) 2006-2009 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include <atlbase.h>
#include "..\shared\PidTable.h"


void LogDebug(const char* fmt, ...);

CPidTable::CPidTable(void)
{
  Reset();
}

CPidTable::CPidTable(const CPidTable& pids)
{
  Copy(pids);
}

CPidTable::~CPidTable(void)
{
}

void CPidTable::Reset()
{
  ServiceId = -1;
  PmtPid = 0;
  PmtVersion = -1;
  PcrPid = 0;

  VideoPids.clear();
  AudioPids.clear();
  SubtitlePids.clear();

  TeletextPid = 0;
  // no reason to reset TeletextSubLang

  ConditionalAccessDescriptorCount = 0;
}

CPidTable& CPidTable::operator = (const CPidTable& other)
{
  if (&other == this)
  {
    return *this;
  }
  Copy(other);
  return *this;
}

bool CPidTable::operator == (const CPidTable& other) const
{
  // Not all members are compared. This is intentional.
  if (ServiceId != other.ServiceId ||
    PmtPid != other.PmtPid ||
    PcrPid != other.PcrPid ||
    VideoPids != other.VideoPids ||
    AudioPids != other.AudioPids ||
    SubtitlePids != other.SubtitlePids)
  {
    return false;
  }
  return true;
}

void CPidTable::Copy(const CPidTable& other)
{
  ServiceId = other.ServiceId;
  PmtPid = other.PmtPid;
  PmtVersion = other.PmtVersion;
  PcrPid = other.PcrPid;

  VideoPids = other.VideoPids;
  AudioPids = other.AudioPids;
  SubtitlePids = other.SubtitlePids;

  TeletextPid = other.TeletextPid;
  //TeletextInfo = other.TeletextInfo;

  ConditionalAccessDescriptorCount = other.ConditionalAccessDescriptorCount;
}

bool CPidTable::HasTeletextPageInfo(int page)
{
  std::vector<TeletextServiceInfo>::iterator it = TeletextInfo.begin();
  while (it != TeletextInfo.end())
  {
    TeletextServiceInfo& info = *it;
    if (info.Page == page)
    {
      return true;
    }
    it++;
  }
  return false;
}

void CPidTable::LogPids()
{
  USES_CONVERSION;
  LogDebug("  program = %d", ServiceId);
  LogDebug("  PMT PID = %d, version = %d", PmtPid, PmtVersion);
  LogDebug("  PCR PID = %d", PcrPid);

  for (unsigned int i = 0; i < VideoPids.size(); i++)
  {
    VideoPid* pid = &VideoPids[i];
    LogDebug("  video PID = %d, type = %s", pid->Pid, T2A(StreamFormatAsString(pid->StreamType)));
  }

  for (unsigned int i = 0; i < AudioPids.size(); i++)
  {
    AudioPid* pid = &AudioPids[i];
    LogDebug("  audio PID = %d, type = %s, language = %s", pid->Pid, T2A(StreamFormatAsString(pid->StreamType)), pid->Lang);
  }
  
  for (unsigned int i = 0; i < SubtitlePids.size(); i++)
  {
    SubtitlePid* pid = &SubtitlePids[i];
    LogDebug("  subtitle PID = %d, type = %s, language = %s", pid->Pid, T2A(StreamFormatAsString(pid->StreamType)), pid->Lang);
  }

  if (TeletextPid > 0)
  {
    LogDebug("  teletext PID = %d", TeletextPid);
  }
}

LPCTSTR CPidTable::StreamFormatAsString(int streamType)
{
  switch (streamType)
  {
    case STREAM_TYPE_VIDEO_MPEG1:
      return _T("MPEG 1");
    case STREAM_TYPE_VIDEO_MPEG2:
      return _T("MPEG 2");
    case STREAM_TYPE_AUDIO_MPEG1:
      return _T("MPEG 1");
    case STREAM_TYPE_AUDIO_MPEG2:
      return _T("MPEG 2");
    case STREAM_TYPE_PRIVATE_SECTIONS:
      return _T("private sections");
    case STREAM_TYPE_PES_PRIVATE_DATA:
      return _T("private data");
    case STREAM_TYPE_AUDIO_AAC:
      return _T("AAC");
    case STREAM_TYPE_VIDEO_MPEG4:
      return _T("MPEG 4");
    case STREAM_TYPE_AUDIO_LATM_AAC:
      return _T("LATM AAC");
    case STREAM_TYPE_VIDEO_H264:
      return _T("H.264");
    case STREAM_TYPE_AUDIO_AC3:
      return _T("AC3");
    case STREAM_TYPE_AUDIO_E_AC3:
      return _T("DD+");
    default:
      return _T("Unknown");
  }
}