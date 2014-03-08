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
#include <Windows.h>
#include "Section.h"
#include "SectionDecoder.h"
#include "PidTable.h"


#define STREAM_TYPE_VIDEO_MPEG2_DCII  0x80  // DigiCipher II, used for some US cable and satellite streams
#define STREAM_TYPE_AUDIO_AC3_ATSC    0x81  // this is the ISO and ATSC ATSC standard stream type; DVB has a descriptor
#define STREAM_TYPE_SUBTITLES_SCTE    0x82  // SCTE standard stream type defined in SCTE 27
#define STREAM_TYPE_AUDIO_E_AC3_ATSC  0x87

#define DESCRIPTOR_REGISTRATION       0x05
#define DESCRIPTOR_CONDITIONAL_ACCESS 0x09
#define DESCRIPTOR_ISO_639_LANG       0x0a
#define DESCRIPTOR_VBI_TELETEXT_DVB   0x46
#define DESCRIPTOR_TELETEXT_DVB       0x56
#define DESCRIPTOR_SUBTITLING_DVB     0x59
#define DESCRIPTOR_AC3_DVB            0x6a
#define DESCRIPTOR_E_AC3_DVB          0x7a


/*-------------------------------------------------------------------------
CBasePmtParser is the base PMT parser class for both TsReader and TsWriter.
---------------------------------------------------------------------------*/
class CBasePmtParser : public CSectionDecoder
{
  public:
    CBasePmtParser(void);
    virtual ~CBasePmtParser(void);
    void Reset();
    void SetFilter(int pid, int programNumber);
    void GetFilter(int& pid, int& programNumber);
    void OnTsPacket(byte* tsPacket);
    void OnNewSection(CSection& sections);
    bool DecodePmtSection(CSection& section);
    bool IsReady();
    CPidTable& GetPidInfo();

  protected:
    int m_programNumber;
    bool m_isFound;
    CPidTable m_pidInfo;
};