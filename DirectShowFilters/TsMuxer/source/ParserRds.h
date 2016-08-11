/* 
 *  Copyright (C) 2005-2013 Team MediaPortal
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
#include <WinError.h>   // HRESULT
#include "ICallBackRds.h"

using namespace std;


#define PROGRAMME_SERVICE_NAME_LENGTH 8
#define RADIOTEXT_LENGTH 64
#define PROGRAMME_TYPE_NAME_LENGTH 8
#define ENHANCED_RADIOTEXT_LENGTH 192


class CParserRds
{
  public:
    CParserRds();
    virtual ~CParserRds();

    void SetCallBack(ICallBackRds* callBack);
    HRESULT Receive(unsigned char* data, long dataLength);
    void Reset();

  private:
    enum TextType
    {
      Standard = 0,
      Utf8 = 1,
      Utf16 = 2
    };

    static HRESULT HandleTextChange(unsigned char* data,
                                    unsigned char* text1,
                                    unsigned char* text2,
                                    TextType textType,
                                    const wchar_t* textContent,
                                    unsigned char segmentNumber,
                                    unsigned char byteCountPerSegment,
                                    unsigned long& seenSegmentMask,
                                    unsigned char totalSegmentCount);
    static void RdsTextToString(unsigned char* data,
                                unsigned long dataLength,
                                TextType textType,
                                char** text);

    ICallBackRds* m_callBack;

    unsigned long m_seenGroup0Segments;
    unsigned char m_programmeServiceName1[PROGRAMME_SERVICE_NAME_LENGTH + 1];
    unsigned char m_programmeServiceName2[PROGRAMME_SERVICE_NAME_LENGTH + 1];

    unsigned long m_seenGroup2Segments;
    unsigned char m_radioText1[RADIOTEXT_LENGTH + 1];
    unsigned char m_radioText2[RADIOTEXT_LENGTH + 1];
    bool m_isRadioTextB;

    unsigned char m_radioTextPlusGroupType;
    bool m_isRadioTextPlusForEnhancedRadioText;
    bool m_isRadioTextPlusTemplated;
    bool m_isRadioTextPlusItemToggleBitSet;
    bool m_isRadioTextPlusItemRunning;

    unsigned char m_enhancedRadioTextGroupType;
    unsigned long m_seenEnhancedRadioTextGroupSegments;
    unsigned char m_enhancedRadioText1[ENHANCED_RADIOTEXT_LENGTH + 1];
    unsigned char m_enhancedRadioText2[ENHANCED_RADIOTEXT_LENGTH + 1];
    unsigned char m_enhancedRadioTextCharacterTable;
    bool m_isEnhancedRadioTextUtf8;

    unsigned long m_seenGroup10Segments;
    unsigned char m_programmeTypeName1[PROGRAMME_TYPE_NAME_LENGTH + 1];
    unsigned char m_programmeTypeName2[PROGRAMME_TYPE_NAME_LENGTH + 1];
    bool m_isProgrammeTypeNameB;
};