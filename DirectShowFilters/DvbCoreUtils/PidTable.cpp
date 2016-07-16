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

  ClearPidSet(VideoPids);
  ClearPidSet(AudioPids);
  ClearPidSet(SubtitlePids);
  ClearPidSet(TeletextPids);
  ClearPidSet(VbiPids);
  ClearPidSet(OtherPids);

  DescriptorsLength = 0;
  if (Descriptors != NULL)
  {
    delete[] Descriptors;
    Descriptors = NULL;
  }
}

void CPidTable::LogPids()
{
  LogDebug(L"  program = %hu", ProgramNumber);
  LogDebug(L"  PMT PID = %hu, version = %hhu", PmtPid, PmtVersion);
  LogDebug(L"  PCR PID = %hu", PcrPid);

  vector<VideoPid*>::const_iterator vPidIt = VideoPids.begin();
  for ( ; vPidIt != VideoPids.end(); vPidIt++)
  {
    VideoPid* pid = *vPidIt;
    if (pid != NULL)
    {
      LogDebug(L"  video PID = %hu, type = %hhu, logical type = %s",
                pid->Pid, pid->StreamType,
                StreamFormatAsString(pid->LogicalStreamType));
    }
  }

  vector<AudioPid*>::const_iterator aPidIt = AudioPids.begin();
  for ( ; aPidIt != AudioPids.end(); aPidIt++)
  {
    AudioPid* pid = *aPidIt;
    if (pid != NULL)
    {
      LogDebug(L"  audio PID = %hu, type = %hhu, logical type = %s, language = %S",
                pid->Pid, pid->StreamType,
                StreamFormatAsString(pid->LogicalStreamType), pid->Lang);
    }
  }
  
  vector<SubtitlePid*>::const_iterator sPidIt = SubtitlePids.begin();
  for ( ; sPidIt != SubtitlePids.end(); sPidIt++)
  {
    SubtitlePid* pid = *sPidIt;
    if (pid != NULL)
    {
      LogDebug(L"  subtitle PID = %hu, type = %hhu, logical type = %s, language = %S",
                pid->Pid, pid->StreamType,
                StreamFormatAsString(pid->LogicalStreamType), pid->Lang);
    }
  }

  vector<TeletextPid*>::const_iterator tPidIt = TeletextPids.begin();
  for ( ; tPidIt != TeletextPids.end(); tPidIt++)
  {
    TeletextPid* pid = *tPidIt;
    if (pid != NULL)
    {
      LogDebug(L"  teletext PID = %hu, type = %hhu, logical type = %s",
                pid->Pid, pid->StreamType,
                StreamFormatAsString(pid->LogicalStreamType));
    }
  }

  vector<VbiPid*>::const_iterator vbiPidIt = VbiPids.begin();
  for ( ; vbiPidIt != VbiPids.end(); vbiPidIt++)
  {
    VbiPid* pid = *vbiPidIt;
    if (pid != NULL)
    {
      LogDebug(L"  VBI PID = %hu, type = %hhu, logical type = %s",
                pid->Pid, pid->StreamType,
                StreamFormatAsString(pid->LogicalStreamType));
    }
  }

  vector<OtherPid*>::const_iterator otherPidIt = OtherPids.begin();
  for ( ; otherPidIt != OtherPids.end(); otherPidIt++)
  {
    OtherPid* pid = *otherPidIt;
    if (pid != NULL)
    {
      LogDebug(L"  other PID = %hu, type = %hhu", pid->Pid, pid->StreamType);
    }
  }
}

const wchar_t* CPidTable::StreamFormatAsString(unsigned char streamType)
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
      return L"H.264/AVC";
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
      return L"H.265/HEVC";
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
      return L"AC-3/DD";
    case STREAM_TYPE_AUDIO_DTS:
      return L"DTS";
    case STREAM_TYPE_AUDIO_E_AC3:
      return L"E-AC-3/DD+";
    case STREAM_TYPE_AUDIO_DTS_HD:
      return L"DTS HD";
    case STREAM_TYPE_AUDIO_AC4:
      return L"AC-4";

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

bool CPidTable::IsVideoStream(unsigned char streamType)
{
  if (
    streamType == STREAM_TYPE_VIDEO_MPEG1 ||
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
    streamType == STREAM_TYPE_VIDEO_VC1
  )
  {
    return true;
  }
  return false;
}

bool CPidTable::IsThreeDimensionalVideoStream(unsigned char streamType)
{
  if (
    streamType == STREAM_TYPE_VIDEO_MPEG2_VIEW ||
    streamType == STREAM_TYPE_VIDEO_MPEG4_PART10_VIEW
  )
  {
    return true;
  }
  return false;
}

bool CPidTable::IsAudioStream(unsigned char streamType)
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
    // AC-4 is a provisional stream type.
    //streamType == STREAM_TYPE_AUDIO_AC4)
  {
    return true;
  }
  return false;
}

bool CPidTable::IsAudioLogicalStream(unsigned char logicalStreamType)
{
  if (IsAudioStream(logicalStreamType) ||
    logicalStreamType == STREAM_TYPE_AUDIO_DTS ||
    logicalStreamType == STREAM_TYPE_AUDIO_AC4)
  {
    return true;
  }
  return false;
}

template<class T> void CPidTable::ClearPidSet(vector<T*>& pidSet)
{
  vector<T*>::iterator it = pidSet.begin();
  for ( ; it != pidSet.end(); it++)
  {
    T* pid = *it;
    if (pid != NULL)
    {
      delete pid;
      *it = NULL;
    }
  }
  pidSet.clear();
}