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
// standards from DVB and ATSC.
#define STREAM_TYPE_UNKNOWN                     -1

#define STREAM_TYPE_VIDEO_UNKNOWN               -1
#define STREAM_TYPE_VIDEO_MPEG1                 0x01
#define STREAM_TYPE_VIDEO_MPEG2                 0x02
#define STREAM_TYPE_VIDEO_MPEG4_PART2           0x10
#define STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXA   0x1b
#define STREAM_TYPE_VIDEO_AUX                   0x1e
#define STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXG   0x1f
#define STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXH   0x20
#define STREAM_TYPE_VIDEO_JPEG                  0x21
#define STREAM_TYPE_VIDEO_MPEG2_STEREO_FP       0x22  // frame-packed stereoscopic
#define STREAM_TYPE_VIDEO_MPEG2_STEREO          0x23
#define STREAM_TYPE_VIDEO_MPEG4_PART10_STEREO   0x24
#define STREAM_TYPE_VIDEO_VC1                   0xea  // this is the [SMPTE] standard stream type; DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a registration descriptor

#define STREAM_TYPE_AUDIO_UNKNOWN               -1
#define STREAM_TYPE_AUDIO_MPEG1                 0x03
#define STREAM_TYPE_AUDIO_MPEG2_PART3           0x04
#define STREAM_TYPE_AUDIO_MPEG2_PART7           0x0f
#define STREAM_TYPE_AUDIO_MPEG4_PART3_LATM      0x11
#define STREAM_TYPE_AUDIO_MPEG4_PART3           0x1c  // no transport
#define STREAM_TYPE_AUDIO_AC3                   0x81  // this is the [ATSC] standard stream type; DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor
#define STREAM_TYPE_AUDIO_DTS                   0x82  // defacto standard (ffdshow, libbluray etc.); DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor, not supported by ATSC and SCTE
#define STREAM_TYPE_AUDIO_E_AC3                 0x87  // this is the [ATSC] standard stream type; DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor
#define STREAM_TYPE_AUDIO_DTS_HD                0x88  // defacto standard (ATSC 2.0); DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor, SCTE uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor

#define STREAM_TYPE_PRIVATE_SECTIONS            0x05
#define STREAM_TYPE_PES_PRIVATE_DATA            0x06
#define STREAM_TYPE_TEXT_MPEG4                  0x1d


//-----------------------------------------------------------------------------
// PID CLASSES
//-----------------------------------------------------------------------------
class BasePid
{
  public:
    BasePid()
    {
      Pid = -1;
      StreamType = STREAM_TYPE_UNKNOWN;
      LogicalStreamType = STREAM_TYPE_UNKNOWN;
      DescriptorsLength = 0;
      Descriptors = NULL;
    }

    virtual ~BasePid()
    {
      if (Descriptors != NULL)
      {
        delete[] Descriptors;
        Descriptors = NULL;
      }
    }

    bool operator == (const BasePid& other) const
    {
      if (Pid != other.Pid ||
        StreamType != other.StreamType ||
        LogicalStreamType != other.LogicalStreamType ||
        DescriptorsLength != other.DescriptorsLength)
      {
        return false;
      }
      return memcmp(Descriptors, other.Descriptors, DescriptorsLength) == 0;
    }

    unsigned short Pid;
    byte StreamType;
    byte LogicalStreamType;
    unsigned short DescriptorsLength;
    byte* Descriptors;
};

class VideoPid : public BasePid
{
  public:
    VideoPid() : BasePid()
    {
    }

    bool operator == (const VideoPid& other) const
    {
      return BasePid::operator == (other);
    }
};

class AudioPid : public BasePid
{
  public:
    AudioPid() : BasePid()
    {
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
      if (!BasePid::operator == (other) || strcmp((char*)Lang, (char*)other.Lang) != 0)
      {
        return false;
      }
      return true;
    }

    byte Lang[7];
};

class SubtitlePid : public BasePid
{
  public:
    SubtitlePid() : BasePid()
    {
      Lang[0] = 'U';
      Lang[1] = 'N';
      Lang[2] = 'K';
      Lang[3] = 0;
    }

    bool operator == (const SubtitlePid& other) const
    {
      if (!BasePid::operator == (other) || strcmp((char*)Lang, (char*)other.Lang) != 0)
      {
        return false;
      }
      return true;
    }

    byte Lang[4];
};

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
      if (strcmp((char*)Lang, (char*)other.Lang) != 0 ||
        Type != other.Type ||
        Page != other.Page)
      {
        return false;
      }
      return true;
    }

    byte Lang[4];
    short Type;
    short Page;
};

class TeletextPid : public BasePid
{
  public:
    TeletextPid() : BasePid()
    {
    }

    bool operator == (const TeletextPid& other) const
    {
      if (!BasePid::operator == (other) || Services.size() != other.Services.size())
      {
        return false;
      }
      // Service details not checked.
      return true;
    }

    bool HasTeletextPageInfo(int page)
    {
      std::vector<TeletextServiceInfo>::iterator it = Services.begin();
      while (it != Services.end())
      {
        TeletextServiceInfo& service = *it;
        if (service.Page == page)
        {
          return true;
        }
        it++;
      }
      return false;
    }

    std::vector<TeletextServiceInfo> Services;
};



class CPidTable
{
  public:
    CPidTable();
    virtual ~CPidTable();

    void Reset();
    void LogPids();
    LPCTSTR StreamFormatAsString(byte streamType);

    unsigned short ProgramNumber;
    unsigned short PmtPid;
    byte PmtVersion;
    unsigned short PcrPid;

    unsigned short DescriptorsLength;
    byte* Descriptors;

    std::vector<VideoPid*> VideoPids;
    std::vector<AudioPid*> AudioPids;
    std::vector<SubtitlePid*> SubtitlePids;
    std::vector<TeletextPid*> TeletextPids;
};