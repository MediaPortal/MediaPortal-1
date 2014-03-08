/*
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#pragma once
#include <Windows.h>
#include <vector>


// The strategy with these stream types is to use ISO standard stream types
// where possible. In many cases these are aligned with the broadcasting
// standards from DVB and ATSC. For DVB, stream types that are labelled as
// private data with an identifying descriptor must be translated.
#define STREAM_TYPE_UNKNOWN           -1

#define STREAM_TYPE_VIDEO_UNKNOWN     -1
#define STREAM_TYPE_VIDEO_MPEG1       0x01
#define STREAM_TYPE_VIDEO_MPEG2       0x02
#define STREAM_TYPE_VIDEO_MPEG4       0x10
#define STREAM_TYPE_VIDEO_H264        0x1b

#define STREAM_TYPE_AUDIO_UNKNOWN     -1
#define STREAM_TYPE_AUDIO_MPEG1       0x03
#define STREAM_TYPE_AUDIO_MPEG2       0x04
#define STREAM_TYPE_AUDIO_AAC         0x0f
#define STREAM_TYPE_AUDIO_LATM_AAC    0x11
#define STREAM_TYPE_AUDIO_AC3         0x81  // this is the ISO and ATSC ATSC standard stream type; DVB has a descriptor
#define STREAM_TYPE_AUDIO_E_AC3       0x84  // this is the ISO standard stream type; ATSC uses 0x87 and DVB has a descriptor

#define STREAM_TYPE_PRIVATE_SECTIONS  0x05
#define STREAM_TYPE_PES_PRIVATE_DATA  0x06


class TeletextServiceInfo
{
  public:
    TeletextServiceInfo()
    {
      Lang[0] = 'U';
      Lang[1] = 'N';
      Lang[2] = 'K';
      Lang[3] = 0;
      Type = -1;
      Page = -1;
    }

    bool operator == (const TeletextServiceInfo& other) const
    {
      if (Lang[0] != other.Lang[0] ||
        Lang[1] != other.Lang[1] ||
        Lang[2] != other.Lang[2] ||
        Lang[3] != other.Lang[3] ||
        Type != other.Type ||
        Page != other.Page)
      {
        return false;
      }
      else
      {
        return true;
      }
    }

    BYTE Lang[4];
    short Type;
    short Page;
};

// This class used to store subtitle stream information.
class SubtitlePid
{
  public:
    SubtitlePid()
    {
      Pid = -1;
      StreamType = -1;
      Lang[0] = 'U';
      Lang[1] = 'N';
      Lang[2] = 'K';
      Lang[3] = 0;
    }

    bool operator == (const SubtitlePid& other) const
    {
      if (Pid != other.Pid ||
        StreamType != other.StreamType ||
        Lang[0] != other.Lang[0] ||
        Lang[1] != other.Lang[1] ||
        Lang[2] != other.Lang[2] ||
        Lang[3] != other.Lang[3])
      {
        return false;
      }
      else
      {
        return true;
      }
    }

    WORD Pid;
    WORD StreamType;
    BYTE Lang[4];
};

// This class used to store audio stream information.
class AudioPid
{
  public:
    AudioPid()
    {
      Pid = -1;
      StreamType = -1;
      Lang[0] = 'U';
      Lang[1] = 'N';
      Lang[2] = 'K';
      Lang[3] = 0;
      Lang[4] = 0;
      Lang[5] = 0;
      Lang[6] = 0;
    }

    bool operator == (const AudioPid& other) const
    {
      if (Pid != other.Pid ||
        StreamType != other.StreamType ||
        Lang[0] != other.Lang[0] ||
        Lang[1] != other.Lang[1] ||
        Lang[2] != other.Lang[2] ||
        Lang[3] != other.Lang[3] ||
        Lang[4] != other.Lang[4] ||
        Lang[5] != other.Lang[5] ||
        Lang[6] != other.Lang[6])
      {
        return false;
      }
      else
      {
        return true;
      }
    }

    WORD Pid;
    WORD StreamType;
    BYTE Lang[7];
};

// This class used to store video stream information.
class VideoPid
{
  public:
    VideoPid()
    {
      Pid = -1;
      StreamType = -1;
    }

    bool operator == (const VideoPid& other) const
    {
      if (Pid != other.Pid || StreamType != other.StreamType)
      {
        return false;
      }
      else
      {
        return true;
      }
    }

    WORD Pid;
    WORD StreamType;
};

class CPidTable
{
  public:
    CPidTable();
    CPidTable(const CPidTable& pids);
    virtual ~CPidTable();

    void Reset();
    void LogPids();
    LPCTSTR StreamFormatAsString(int streamType);

    bool HasTeletextPageInfo(int page); // Do we have TeletextServiceInfo for a given page?

    CPidTable& operator = (const CPidTable& other);
    bool operator == (const CPidTable& other) const;
    void Copy(const CPidTable& other);

    int ServiceId;
    ULONG PmtPid;
    byte PmtVersion;
    ULONG PcrPid;

    std::vector<VideoPid> VideoPids;
    std::vector<AudioPid> AudioPids;
    std::vector<SubtitlePid> SubtitlePids;

    WORD TeletextPid;
    std::vector<TeletextServiceInfo> TeletextInfo;

    int ConditionalAccessDescriptorCount;
};