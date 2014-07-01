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
#include "..\shared\PidTable.h"


extern void LogDebug(const wchar_t* fmt, ...);


//-----------------------------------------------------------------------------
// UTILITY FUNCTIONS
//-----------------------------------------------------------------------------
bool IsVideoStream(byte streamType)
{
  if (streamType == STREAM_TYPE_VIDEO_MPEG1 ||
    streamType == STREAM_TYPE_VIDEO_MPEG2 ||
    streamType == STREAM_TYPE_VIDEO_MPEG2_VIEW ||
    streamType == STREAM_TYPE_VIDEO_MPEG4_PART2 ||
    streamType == STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXA ||
    streamType == STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXG ||
    streamType == STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXH ||
    streamType == STREAM_TYPE_VIDEO_MPEG4_PART10_VIEW ||
    streamType == STREAM_TYPE_VIDEO_MPEGH_PART2 ||
    streamType == STREAM_TYPE_VIDEO_AUX ||
    streamType == STREAM_TYPE_VIDEO_JPEG ||
    streamType == STREAM_TYPE_VIDEO_VC1)
  {
    return true;
  }
  return false;
}

bool IsAudioStream(byte streamType)
{
  if (streamType == STREAM_TYPE_AUDIO_MPEG1 ||
    streamType == STREAM_TYPE_AUDIO_MPEG2_PART3 ||
    streamType == STREAM_TYPE_AUDIO_MPEG2_PART7 ||
    streamType == STREAM_TYPE_AUDIO_MPEG4_PART3 ||
    streamType == STREAM_TYPE_AUDIO_MPEG4_PART3_LATM ||
    streamType == STREAM_TYPE_AUDIO_AC3 ||
    // DTS is a logical stream type. The ID clashes with SCTE subtitles, so can't use it directly.
    //streamType == STREAM_TYPE_AUDIO_DTS ||
    streamType == STREAM_TYPE_AUDIO_E_AC3 ||
    streamType == STREAM_TYPE_AUDIO_DTS_HD)
  {
    return true;
  }
  return false;
}

bool IsAudioLogicalStream(byte logicalStreamType)
{
  if (IsAudioStream(logicalStreamType) || logicalStreamType == STREAM_TYPE_AUDIO_DTS)
  {
    return true;
  }
  return false;
}


//-----------------------------------------------------------------------------
// CLASS
//-----------------------------------------------------------------------------
CPidTable::CPidTable(void)
{
  Descriptors = NULL;
  Reset();
}

CPidTable::~CPidTable(void)
{
  Reset();
}

void CPidTable::Reset()
{
  ProgramNumber = -1;
  PmtPid = 0;
  PmtVersion = 0xff;
  PcrPid = 0;

  std::vector<VideoPid*>::iterator vPidIt = VideoPids.begin();
  while (vPidIt != VideoPids.end())
  {
    delete *vPidIt;
    vPidIt++;
  }
  VideoPids.clear();

  std::vector<AudioPid*>::iterator aPidIt = AudioPids.begin();
  while (aPidIt != AudioPids.end())
  {
    delete *aPidIt;
    aPidIt++;
  }
  AudioPids.clear();

  std::vector<SubtitlePid*>::iterator sPidIt = SubtitlePids.begin();
  while (sPidIt != SubtitlePids.end())
  {
    delete *sPidIt;
    sPidIt++;
  }
  SubtitlePids.clear();

  std::vector<TeletextPid*>::iterator tPidIt = TeletextPids.begin();
  while (tPidIt != TeletextPids.end())
  {
    delete *tPidIt;
    tPidIt++;
  }
  TeletextPids.clear();

  DescriptorsLength = 0;
  if (Descriptors != NULL)
  {
    delete[] Descriptors;
    Descriptors = NULL;
  }
}

void CPidTable::LogPids()
{
  LogDebug(L"  program = %d", ProgramNumber);
  LogDebug(L"  PMT PID = %d, version = %d", PmtPid, PmtVersion);
  LogDebug(L"  PCR PID = %d", PcrPid);

  for (unsigned int i = 0; i < VideoPids.size(); i++)
  {
    VideoPid* pid = VideoPids[i];
    LogDebug(L"  video PID = %d, type = %d, logical type = %s", pid->Pid, pid->StreamType, StreamFormatAsString(pid->LogicalStreamType));
  }

  for (unsigned int i = 0; i < AudioPids.size(); i++)
  {
    AudioPid* pid = AudioPids[i];
    LogDebug(L"  audio PID = %d, type = %d, logical type = %s, language = %s", pid->Pid, pid->StreamType, StreamFormatAsString(pid->LogicalStreamType), pid->Lang);
  }
  
  for (unsigned int i = 0; i < SubtitlePids.size(); i++)
  {
    SubtitlePid* pid = SubtitlePids[i];
    LogDebug(L"  subtitle PID = %d, type = %d, logical type = %s, language = %s", pid->Pid, pid->StreamType, StreamFormatAsString(pid->LogicalStreamType), pid->Lang);
  }

  for (unsigned int i = 0; i < TeletextPids.size(); i++)
  {
    TeletextPid* pid = TeletextPids[i];
    LogDebug(L"  teletext PID = %d, type = %d, logical type = %s", pid->Pid, pid->StreamType, StreamFormatAsString(pid->LogicalStreamType));
  }
}

const wchar_t* CPidTable::StreamFormatAsString(byte streamType)
{
  switch (streamType)
  {
    case STREAM_TYPE_VIDEO_MPEG1:
      return L"MPEG 1";
    case STREAM_TYPE_VIDEO_MPEG2:
      return L"MPEG 2";
    case STREAM_TYPE_VIDEO_MPEG4_PART2:
      return L"MPEG 4";
    case STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXA:
      return L"H.264";
    case STREAM_TYPE_VIDEO_AUX:
      return L"ISO 23000-2";
    case STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXG:
      return L"H.264-G SVC";
    case STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXH:
      return L"H.264-H MVC";
    case STREAM_TYPE_VIDEO_JPEG:
      return L"JPEG";
    case STREAM_TYPE_VIDEO_MPEG2_VIEW:
      return L"MPEG 2 view";
    case STREAM_TYPE_VIDEO_MPEG4_PART10_VIEW:
      return L"H.264 view";
    case STREAM_TYPE_VIDEO_MPEGH_PART2:
      return L"H.265";
    case STREAM_TYPE_VIDEO_VC1:
      return L"VC-1";

    case STREAM_TYPE_AUDIO_MPEG1:
      return L"MPEG 1";
    case STREAM_TYPE_AUDIO_MPEG2_PART3:
      return L"MPEG 2";
    case STREAM_TYPE_AUDIO_MPEG2_PART7:
      return L"ADTS AAC";
    case STREAM_TYPE_AUDIO_MPEG4_PART3_LATM:
      return L"LATM AAC";
    case STREAM_TYPE_AUDIO_MPEG4_PART3:
      return L"AAC";
    case STREAM_TYPE_AUDIO_AC3:
      return L"AC3/DD";
    case STREAM_TYPE_AUDIO_DTS:
      return L"DTS";
    case STREAM_TYPE_AUDIO_E_AC3:
      return L"E-AC3/DD+";
    case STREAM_TYPE_AUDIO_DTS_HD:
      return L"DTS HD";

    case STREAM_TYPE_PRIVATE_SECTIONS:
      return L"private sections";
    case STREAM_TYPE_PES_PRIVATE_DATA:
      return L"private data";
    case STREAM_TYPE_TEXT_MPEG4:
      return L"MPEG 4";
    default:
      return L"Unknown";
  }
}