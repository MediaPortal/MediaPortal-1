/*
 *  Copyright (C) 2006-2010 Team MediaPortal
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
#include "PidTable.h"
#include "Section.h"
#include "SectionDecoder.h"


#define STREAM_TYPE_VIDEO_MPEG2_DCII                0x80  // DigiCipher II video (compatible with MPEG 2 part 2), used for some US cable and satellite streams
#define STREAM_TYPE_SUBTITLES_SCTE                  0x82  // SCTE standard stream type defined in SCTE 27

#define DESCRIPTOR_VIDEO_STREAM                     0x02
#define DESCRIPTOR_REGISTRATION                     0x05
#define DESCRIPTOR_CONDITIONAL_ACCESS               0x09
#define DESCRIPTOR_ISO_639_LANG                     0x0a
#define DESCRIPTOR_AVC_VIDEO                        0x28
#define DESCRIPTOR_MVC_EXTENSION                    0x31
#define DESCRIPTOR_MPEG2_STEREOSCOPIC_VIDEO_FORMAT  0x34
#define DESCRIPTOR_STEREOSCOPIC_PROGRAM_INFO        0x35
#define DESCRIPTOR_STEREOSCOPIC_VIDEO_INFO          0x36
#define DESCRIPTOR_HEVC_VIDEO                       0x38

#define DESCRIPTOR_DVB_VBI_DATA                     0x45
#define DESCRIPTOR_DVB_VBI_TELETEXT                 0x46
#define DESCRIPTOR_DVB_TELETEXT                     0x56
#define DESCRIPTOR_DVB_SUBTITLING                   0x59
#define DESCRIPTOR_DVB_AC3                          0x6a
#define DESCRIPTOR_DVB_E_AC3                        0x7a
#define DESCRIPTOR_DVB_DTS                          0x7b
#define DESCRIPTOR_DVB_EXTENSION                    0x7f
#define DESCRIPTOR_DVB_X_DTS_HD                     0x0e

#define DESCRIPTOR_ATSC_AC3                         0x81
#define DESCRIPTOR_ATSC_CAPTION_SERVICE             0x86
#define DESCRIPTOR_ATSC_E_AC3                       0xcc

#define DESCRIPTOR_SCTE_DTS_HD                      0x7b
#define DESCRIPTOR_SCTE_3D_MPEG2                    0xe8


/*-------------------------------------------------------------------------
CBasePmtParser is the base PMT parser class for TsMuxer and TsWriter.
---------------------------------------------------------------------------*/
class CBasePmtParser : public CSectionDecoder
{
  public:
    CBasePmtParser(void);
    virtual ~CBasePmtParser(void);

    void Reset();
    void SetFilter(unsigned short pid, unsigned short programNumber);
    void GetFilter(unsigned short& pid, unsigned short& programNumber);
    void OnTsPacket(unsigned char* tsPacket);
    virtual void OnNewSection(CSection& section);
    bool DecodePmtSection(const CSection& section);
    bool IsReady();
    CPidTable& GetPidInfo();

  protected:
    unsigned short m_programNumber;
    bool m_isFound;
    CPidTable m_pidInfo;
};