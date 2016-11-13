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
#include <cstddef>    // NULL
#include <vector>

using namespace std;


// The strategy with these stream types is to use ISO standard stream types
// where possible. In many cases these are aligned with the broadcasting
// standards from DVB and ATSC.
#define STREAM_TYPE_UNKNOWN                     0xff

#define STREAM_TYPE_VIDEO_MPEG1                 0x01
#define STREAM_TYPE_VIDEO_MPEG2                 0x02
#define STREAM_TYPE_VIDEO_MPEG4_PART2           0x10
#define STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXA   0x1b  // h.264/AVC
#define STREAM_TYPE_VIDEO_AUX                   0x1e  // ISO/IEC 23002-3; compression specified by descriptor
#define STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXG   0x1f  // SVC sub-bitstream
#define STREAM_TYPE_VIDEO_MPEG4_PART10_ANNEXH   0x20  // MVC sub-bitstream
#define STREAM_TYPE_VIDEO_JPEG                  0x21
#define STREAM_TYPE_VIDEO_MPEG2_VIEW            0x22  // service-compatible stereoscopic 3DTV, MPEG 2 additional view
#define STREAM_TYPE_VIDEO_MPEG4_PART10_VIEW     0x23  // service-compatible stereoscopic 3DTV, h.264/AVC additional view
#define STREAM_TYPE_VIDEO_MPEGH_PART2           0x24  // h.265/HEVC
#define STREAM_TYPE_VIDEO_VC1                   0xea  // this is the [SMPTE, DVB] standard stream type

#define STREAM_TYPE_AUDIO_MPEG1                 0x03
#define STREAM_TYPE_AUDIO_MPEG2_PART3           0x04
#define STREAM_TYPE_AUDIO_MPEG2_PART7           0x0f
#define STREAM_TYPE_AUDIO_MPEG4_PART3_LATM      0x11
#define STREAM_TYPE_AUDIO_MPEG4_PART3           0x1c  // no transport
#define STREAM_TYPE_AUDIO_AC3                   0x81  // this is the [ATSC, SCTE] standard stream type; DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor
#define STREAM_TYPE_AUDIO_DTS                   0x82  // defacto standard (ffdshow, libbluray etc.); DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor, not supported by ATSC and SCTE
#define STREAM_TYPE_AUDIO_E_AC3                 0x87  // this is the [ATSC, SCTE] standard stream type; DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor
#define STREAM_TYPE_AUDIO_DTS_HD                0x88  // this is the [ATSC, SCTE] standard stream type; DVB uses STREAM_TYPE_PES_PRIVATE_DATA with a descriptor
#define STREAM_TYPE_AUDIO_AC4                   0x89  // (place-holder - revise later)

#define STREAM_TYPE_PRIVATE_SECTIONS            0x05
#define STREAM_TYPE_PES_PRIVATE_DATA            0x06
#define STREAM_TYPE_TEXT_MPEG4                  0x1d


//-----------------------------------------------------------------------------
// PID CLASSES
//-----------------------------------------------------------------------------
class BasePid
{
  public:
    BasePid(unsigned short pid, unsigned char streamType)
    {
      Pid = pid;
      StreamType = streamType;
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
      if (
        Pid != other.Pid ||
        StreamType != other.StreamType ||
        LogicalStreamType != other.LogicalStreamType ||
        DescriptorsLength != other.DescriptorsLength
      )
      {
        return false;
      }
      return memcmp(Descriptors, other.Descriptors, DescriptorsLength) == 0;
    }

    unsigned short Pid;
    unsigned char StreamType;
    unsigned char LogicalStreamType;
    unsigned short DescriptorsLength;
    unsigned char* Descriptors;
};

class VideoPid : public BasePid
{
  public:
    VideoPid(unsigned short pid, unsigned char streamType) : BasePid(pid, streamType)
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
    AudioPid(unsigned short pid, unsigned char streamType) : BasePid(pid, streamType)
    {
      Lang[0] = 'U';
      Lang[1] = 'N';
      Lang[2] = 'D';
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

    unsigned char Lang[7];
};

class SubtitlePid : public BasePid
{
  public:
    SubtitlePid(unsigned short pid, unsigned char streamType) : BasePid(pid, streamType)
    {
      Lang[0] = 'U';
      Lang[1] = 'N';
      Lang[2] = 'D';
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

    unsigned char Lang[4];
};

class TeletextPid : public BasePid
{
  public:
    TeletextPid(unsigned short pid, unsigned char streamType) : BasePid(pid, streamType)
    {
    }

    bool operator == (const TeletextPid& other) const
    {
      return BasePid::operator == (other);
    }
};

class VbiPid : public BasePid
{
  public:
    VbiPid(unsigned short pid, unsigned char streamType) : BasePid(pid, streamType)
    {
    }

    bool operator == (const VbiPid& other) const
    {
      return BasePid::operator == (other);
    }
};

class OtherPid : public BasePid
{
  public:
    OtherPid(unsigned short pid, unsigned char streamType) : BasePid(pid, streamType)
    {
    }

    bool operator == (const VbiPid& other) const
    {
      return BasePid::operator == (other);
    }
};


class CPidTable
{
  public:
    CPidTable();
    virtual ~CPidTable();

    void Reset();
    void LogPids();

    static const wchar_t* StreamFormatAsString(unsigned char streamType);
    static bool IsVideoStream(unsigned char streamType);
    static bool IsThreeDimensionalVideoStream(unsigned char streamType);
    static bool IsAudioStream(unsigned char streamType);
    static bool IsAudioLogicalStream(unsigned char logicalStreamType);

    unsigned short ProgramNumber;
    unsigned short PmtPid;
    unsigned char PmtVersion;
    unsigned short PcrPid;

    unsigned short DescriptorsLength;
    unsigned char* Descriptors;

    vector<VideoPid*> VideoPids;
    vector<AudioPid*> AudioPids;
    vector<SubtitlePid*> SubtitlePids;
    vector<TeletextPid*> TeletextPids;
    vector<VbiPid*> VbiPids;
    vector<OtherPid*> OtherPids;

  private:
    template<class T> void ClearPidSet(vector<T*>& pidSet);
};