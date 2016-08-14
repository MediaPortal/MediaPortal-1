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
#include <ctime>
#include <vector>
#include "..\..\shared\BasePmtParser.h"
#include "..\..\shared\Section.h"
#include "..\..\shared\SectionDecoder.h"
#include "..\..\shared\TsHeader.h"
#include "CriticalSection.h"
#include "ICallBackPmt.h"
#include "IEncryptionAnalyser.h"

using namespace MediaPortal;
using namespace std;


#define TABLE_ID_PMT 2


class CGrabberPmt : public CSectionDecoder
{
  public:
    CGrabberPmt(IEncryptionAnalyser* analyser);
    virtual ~CGrabberPmt();

    void Reset();
    void SetFilter(unsigned short pid, unsigned short programNumber);
    void SetCallBack(ICallBackPmt* callBack);
    void OnTsPacket(CTsHeader& header, unsigned char* tsPacket);
    void OnNewSection(CSection& section);
    bool IsReady();

    void GetFilter(unsigned short& pid, unsigned short& programNumber) const;
    bool GetProgramInformation(unsigned short& pid,
                                unsigned short& programNumber,
                                bool& isPmtReceived,
                                unsigned short& streamCountVideo,
                                unsigned short& streamCountAudio,
                                bool& isEncrypted,
                                bool& isEncryptionDetectionAccurate,
                                bool& isThreeDimensional,
                                unsigned long* audioLanguages,
                                unsigned char& audioLanguageCount,
                                unsigned long* subtitlesLanguages,
                                unsigned char& subtitlesLanguageCount);
    bool GetFreesatPids(bool& isFreesatProgram,
                        unsigned short& pidEitSchedule,
                        unsigned short& pidEitPresentFollowing,
                        unsigned short& pidSdt,
                        unsigned short& pidBat,
                        unsigned short& pidNit);
    bool GetOpenTvEpgPids(bool& isOpenTvEpgProgram,
                          vector<unsigned short>& pidsEvent,
                          vector<unsigned short>& pidsDescription);
    bool GetTable(unsigned char* table, unsigned short& tableBufferSize) const;

  private:
    static void CheckDescriptorsPidVideo(unsigned char* descriptors,
                                          unsigned short descriptorsLength,
                                          bool& hasCaDescriptor,
                                          bool& isThreeDimensionalVideo,
                                          vector<unsigned long>& captionsLanguages);
    static void CheckDescriptorsPidAudio(unsigned char* descriptors,
                                          unsigned short descriptorsLength,
                                          bool& hasCaDescriptor);
    static void CheckDescriptorsPidTeletext(unsigned char* descriptors,
                                            unsigned short descriptorsLength,
                                            vector<unsigned long>& subtitlesLanguages);

    CCriticalSection m_section;
    bool m_isReady;
    unsigned char m_version;
    ICallBackPmt* m_callBack;
    IEncryptionAnalyser* m_encryptionAnalyser;
    clock_t m_lastSeenTime;
    unsigned short m_programNumber;
    CSection m_pmtSection;
    CBasePmtParser m_parser;
};