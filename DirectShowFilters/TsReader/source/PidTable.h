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

// This class used to store subtitle stream specific information
class SubtitlePid
{
public:

  SubtitlePid()
  {
    Pid=-1; SubtitleServiceType=-1; Lang[0]='U'; Lang[1]='N'; Lang[2]='K'; Lang[3]=0;
  }
  
  bool operator ==(const SubtitlePid& other) const
  {
    if(Pid != other.Pid
      || Lang[0] != other.Lang[0]
      || Lang[1] != other.Lang[1]
      || Lang[2] != other.Lang[2]
      || Lang[3] != other.Lang[3])
      {
        return false;
      }
    else
    {
      return true;
    }
  }
  WORD Pid;
  WORD SubtitleServiceType;
  BYTE Lang[4];
};

// This class used to store audio stream specific information
class AudioPid
{
public:
  AudioPid()
  {
    Pid=-1; AudioServiceType=-1; Lang[0]='U'; Lang[1]='N'; Lang[2]='K'; Lang[3]=0;
  }
  bool operator ==(const AudioPid& other) const
  {
    if(Pid != other.Pid
      || Lang[0] != other.Lang[0]
      || Lang[1] != other.Lang[1]
      || Lang[2] != other.Lang[2]
      || Lang[3] != other.Lang[3]
      || AudioServiceType != other.AudioServiceType)
      {
        return false;
      }
    else
    {
      return true;
    }
  }
  WORD Pid;
  BYTE Lang[4];
  WORD AudioServiceType;
};

// This class used to store video stream specific information
class VideoPid
{
public:
  VideoPid()
  {
    Pid=-1; VideoServiceType=-1; 
  }

  bool operator ==(const VideoPid& other) const
  {
    if(Pid != other.Pid 
      || VideoServiceType != other.VideoServiceType)
    {
      return false;
    }
    else
    {
      return true;
    }
  }

  WORD Pid;
  int VideoServiceType;
};

class TempPid
{
public:

  TempPid()
  {
    Pid=-1; Lang[0]='U'; Lang[1]='N'; Lang[2]='K'; Lang[3]=0;;
  }
  WORD Pid;
  BYTE Lang[4];
};

class CPidTable
{
public:

  CPidTable();
  CPidTable(const CPidTable& pids);
  virtual ~CPidTable();
  void Reset();

  bool HasTeletextPageInfo(int page); // do we have a TeletextServiceInfo entry for that page

  CPidTable& operator = (const CPidTable& pids);
  bool operator==(const CPidTable& other) const;

  void Copy(const CPidTable &pids);

  ULONG PcrPid;
  ULONG PmtPid;
  WORD TeletextPid; // which PID contains the teletext data
  std::vector<TeletextServiceInfo> TeletextInfo;

  int  ServiceId;

  std::vector<VideoPid> videoPids;
  std::vector<AudioPid> audioPids;
  std::vector<SubtitlePid> subtitlePids;
};
