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
#include "ParserRds.h"
#include <algorithm>  // min()
#include <cstddef>    // NULL
#include <cstring>    // memcpy(), memset(), strlen()


// Refer to IEC 62106 annex E tables E1 and E2.
const unsigned short RDS_TO_UTF16[256] =
{
  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,
  NULL, NULL, 0x0a, 0x0b, NULL, 0x0d, NULL, NULL,
  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,
  NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0x1f,
  0x20, 0x21, 0x22, 0x23, 0xa4, 0x25, 0x26, 0x27,
  0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
  0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
  0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
  0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47,
  0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f,
  0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57,
  0x58, 0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x2015, 0x5f,
  0x2016, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67,
  0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f,
  0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77,
  0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x203e, NULL,

  0xe1, 0xe0, 0xe9, 0xe8, 0xed, 0xec, 0xf3, 0xf2,
  0xfa, 0xf9, 0xd1, 0xc7, 0x015e, 0xdf, 0xa1, 0x132,
  0xe2, 0xe4, 0xea, 0xeb, 0xee, 0xef, 0xf4, 0xf6,
  0xfb, 0xfc, 0xf1, 0xe7, 0x015f, 0x011f, 0x0131, 0x0133,
  0xaa, 0x03b1, 0xa9, 0x2030, 0x011e, 0x011b, 0x0148, 0x0151,
  0x03c0, 0x20ac, 0xa3, 0x24, 0x2190, 0x2191, 0x2192, 0x2193,
  0xba, 0xb9, 0xb2, 0xb3, 0xb1, 0x0130, 0x0144, 0x0171,
  0xb5, 0xbf, 0xf7, 0xb0, 0xbc, 0xbd, 0xbe, 0xa7,
  0xc1, 0xc0, 0xc9, 0xc8, 0xcd, 0xcc, 0xd3, 0xd2,
  0xda, 0xd9, 0x0158, 0x010c, 0x0160, 0x017d, 0xd0, 0x013f,
  0xc2, 0xc4, 0xca, 0xcb, 0xce, 0xcf, 0xd4, 0xd6,
  0xdb, 0xdc, 0x0159, 0x010d, 0x0161, 0x017e, 0x0111, 0x0140,
  0xc3, 0xc5, 0xc6, 0x0152, 0x0177, 0xdd, 0xd5, 0xd8,
  0xde, 0x014a, 0x0154, 0x0106, 0x015a, 0x0179, 0x0166, 0xf0,
  0xe3, 0xe5, 0xe6, 0x0153, 0x0175, 0xfd, 0xf5, 0xf8,
  0xfe, 0x014b, 0x0155, 0x0107, 0x015b, 0x017a, 0x0167, NULL
};


extern void LogDebug(const wchar_t* fmt, ...);

CParserRds::CParserRds()
{
  Reset();
}

CParserRds::~CParserRds()
{
  m_callBack = NULL;
}

void CParserRds::SetCallBack(ICallBackRds* callBack)
{
  m_callBack = callBack;
}

HRESULT CParserRds::Receive(unsigned char* data, long dataLength)
{
  if (dataLength != 8)
  {
    return E_UNEXPECTED;
  }

  // The blocks are received in order, however the bytes within each block are
  // reversed. Swap them back.
  for (unsigned char i = 0; i < 8; i += 2)
  {
    unsigned char temp = data[i];
    data[i] = data[i + 1];
    data[i + 1] = temp;
  }

  //unsigned short programmeIdentification = (data[0] << 8) | data[1];
  unsigned char groupType = data[2] >> 4;
  bool isVersionB = (data[2] & 8) != 0;
  //bool trafficProgrammeCode = (data[2] & 4) != 0;
  unsigned char programmeType = ((data[2] & 3) << 3) | data[3] >> 5;

  if (isVersionB)
  {
    //unsigned short programmeIdentification2 = (data[4] << 8) | data[5];
  }

  if (groupType == 0)
  {
    //bool trafficAnnouncementCode = (data[3] & 0x10) != 0;
    //bool isMusic = (data[3] & 8) != 0;    // if not is music then is speech
    unsigned char decoderIdentificationSegmentAddress = data[3] & 3;
    /*if (!isVersionB)
    {
      unsigned char alternativeFrequency1 = data[4];
      unsigned char alternativeFrequency2 = data[5];
    }*/

    unsigned long seenSegmentsBefore = m_seenGroup0Segments;
    HRESULT hr = HandleTextChange(&data[6],
                                  m_programmeServiceName1,
                                  m_programmeServiceName2,
                                  Standard,
                                  L"programme service name",
                                  decoderIdentificationSegmentAddress,
                                  2,
                                  m_seenGroup0Segments,
                                  4);
    if (hr == S_OK && seenSegmentsBefore != m_seenGroup0Segments)
    {
      bool flagValue = (data[3] & 8) != 0;
      if (decoderIdentificationSegmentAddress == 0)
      {
        LogDebug(L"RDS: is dynamic programme type = %d", flagValue);
      }
      else if (decoderIdentificationSegmentAddress == 1)
      {
        LogDebug(L"RDS: is compressed = %d", flagValue);
      }
      else if (decoderIdentificationSegmentAddress == 2)
      {
        LogDebug(L"RDS: is artificial head = %d", flagValue);
      }
      else
      {
        LogDebug(L"RDS: is stereo = %d", flagValue);
      }

      if (m_seenGroup0Segments == 0xf && m_callBack != NULL)
      {
        char* programmeServiceName = NULL;
        RdsTextToString(m_programmeServiceName2,
                        PROGRAMME_SERVICE_NAME_LENGTH,
                        Standard,
                        &programmeServiceName);
        if (programmeServiceName != NULL)
        {
          m_callBack->OnRdsProgrammeServiceNameReceived(programmeServiceName);
          delete[] programmeServiceName;
        }
      }
    }
    return hr;
  }
  else if (groupType == 2)
  {
    bool isTextB = (data[3] & 0x10) != 0;
    unsigned char textSegmentAddress = data[3] & 0xf;
    unsigned char byteCountPerSegment = 4;  // blocks C and D => 4 characters per segment x 16 segments => 64 characters total
    unsigned char dataOffset = 4;
    if (isVersionB)
    {
      byteCountPerSegment = 2;              // only block D => 2 characters per segment x 16 segments => 32 characters total
      dataOffset = 6;
    }

    return HandleTextChange(&data[dataOffset],
                            m_radioText1,
                            m_radioText2,
                            Standard,
                            L"RadioText",
                            textSegmentAddress,
                            byteCountPerSegment,
                            m_seenGroup2Segments,
                            16);
  }
  else if (groupType == 3 && !isVersionB)
  {
    unsigned char applicationGroupType = (data[3] >> 1) & 0xf;
    unsigned short messageBits = (data[4] << 8) | data[5];
    unsigned short applicationIdentification = (data[6] << 8) | data[7];
    if (applicationIdentification == 0x4bd7 && m_radioTextPlusGroupType != applicationGroupType)
    {
      m_radioTextPlusGroupType = applicationGroupType;
      m_isRadioTextPlusForEnhancedRadioText = (messageBits & 0x2000) != 0;
      m_isRadioTextPlusTemplated = (messageBits & 0x1000) != 0;
      LogDebug(L"RDS: RadioText+ application detected, group type = %hhu, is for eRT = %d, is templated = %d",
                applicationGroupType, m_isRadioTextPlusForEnhancedRadioText,
                m_isRadioTextPlusTemplated);
    }
    else if (applicationIdentification == 0x6552 && m_enhancedRadioTextGroupType != applicationGroupType)
    {
      m_enhancedRadioTextGroupType = applicationGroupType;
      m_enhancedRadioTextCharacterTable = (messageBits >> 2) & 7;
      bool isLeftToRight = (messageBits & 2) == 0;
      m_isEnhancedRadioTextUtf8 = (messageBits & 1) != 0;
      LogDebug(L"RDS: eRT application detected, group type = %hhu, character table ID = %hhu, is left to right = %d, is UTF-8 = %d",
                applicationGroupType, m_enhancedRadioTextCharacterTable,
                isLeftToRight, m_isEnhancedRadioTextUtf8);
    }
  }
  else if (groupType == 10 && !isVersionB)
  {
    bool isTextB = (data[3] & 0x10) != 0;
    unsigned char programmeTypeNameSegmentAddress = data[3] & 1;

    return HandleTextChange(&data[4],
                            m_programmeTypeName1,
                            m_programmeTypeName2,
                            Standard,
                            L"programme type name",
                            programmeTypeNameSegmentAddress,
                            4,
                            m_seenGroup10Segments,
                            2);
  }
  else if (
    groupType == m_radioTextPlusGroupType &&
    !isVersionB &&
    !m_isRadioTextPlusTemplated
  )
  {
    bool itemToggleBit = (data[3] & 0x10) != 0;
    bool itemRunningBit = (data[3] & 8) != 0;
    unsigned char contentType1 = ((data[3] & 7) << 3) | (data[4] >> 5);
    unsigned char startMarker1 = ((data[4] & 0x1f) << 1) | (data[5] >> 7);
    unsigned char lengthMarker1 = (data[5] >> 1) & 0x3f;
    unsigned char contentType2 = ((data[5] & 1) << 5) | (data[6] >> 3);
    unsigned char startMarker2 = ((data[6] & 7) << 3) | (data[7] >> 5);
    unsigned char lengthMarker2 = data[7] & 0x1f;
  }
  else if (
    groupType == m_enhancedRadioTextGroupType &&
    !isVersionB &&
    m_enhancedRadioTextCharacterTable == 0   // Only character table 0 is supported. Others are currently reserved.
  )
  {
    unsigned char ertSegmentAddress = data[3] & 0x1f;
    TextType textType = Utf16;
    if (m_isEnhancedRadioTextUtf8)
    {
      textType = Utf8;
    }

    return HandleTextChange(&data[4],
                            m_enhancedRadioText1,
                            m_enhancedRadioText2,
                            textType,
                            L"enhanced RadioText",
                            ertSegmentAddress,
                            4,
                            m_seenEnhancedRadioTextGroupSegments,
                            32);
  }
  return S_OK;
}

void CParserRds::Reset()
{
  m_seenGroup0Segments = 0;
  memset(m_programmeServiceName1, 0, sizeof(m_programmeServiceName1));
  memset(m_programmeServiceName2, 0, sizeof(m_programmeServiceName2));

  m_seenGroup2Segments = 0;
  memset(m_radioText1, 0, sizeof(m_radioText1));
  memset(m_radioText2, 0, sizeof(m_radioText2));
  m_isRadioTextB = false;

  m_radioTextPlusGroupType = 0;
  m_isRadioTextPlusForEnhancedRadioText = false;
  m_isRadioTextPlusTemplated = false;
  m_isRadioTextPlusItemToggleBitSet = false;
  m_isRadioTextPlusItemRunning = false;

  m_enhancedRadioTextGroupType = 0;
  m_seenEnhancedRadioTextGroupSegments = 0;
  memset(m_enhancedRadioText1, 0, sizeof(m_enhancedRadioText1));
  memset(m_enhancedRadioText2, 0, sizeof(m_enhancedRadioText2));
  m_enhancedRadioTextCharacterTable = 0;
  m_isEnhancedRadioTextUtf8 = false;

  m_seenGroup10Segments = 0;
  memset(m_programmeTypeName1, 0, sizeof(m_programmeTypeName1));
  memset(m_programmeTypeName2, 0, sizeof(m_programmeTypeName2));
  m_isProgrammeTypeNameB = false;
}

HRESULT CParserRds::HandleTextChange(unsigned char* data,
                                      unsigned char* text1,
                                      unsigned char* text2,
                                      TextType textType,
                                      const wchar_t* textContent,
                                      unsigned char segmentNumber,
                                      unsigned char byteCountPerSegment,
                                      unsigned long& seenSegmentMask,
                                      unsigned char totalSegmentCount)
{
  // Double-buffering seems to be very effective for minimising errors. The
  // trade-off is a longer delay between tuning and text completion, double the
  // memory usage (insignificant - less than 1 KB) and possibly slightly higher
  // CPU usage.
  bool isConsistent = true;
  unsigned char textOffset = segmentNumber * byteCountPerSegment;
  unsigned char dataOffset = 0;
  for (unsigned char i = textOffset; i < textOffset + byteCountPerSegment; i++)
  {
    if (data[dataOffset] != text1[i])
    {
      text1[i] = data[dataOffset];
      isConsistent = false;
    }
    dataOffset++;
  }
  if (!isConsistent)
  {
    // To disable double-buffering, don't return here. Expect more errors in
    // content if you do this.
    return S_OK;
  }

  isConsistent = true;
  for (unsigned char i = textOffset; i < textOffset + byteCountPerSegment; i++)
  {
    if (text1[i] != text2[i])
    {
      text2[i] = text1[i];
      isConsistent = false;
    }
  }

  unsigned long segmentFlag = 1 << segmentNumber;
  if ((seenSegmentMask & segmentFlag) != 0)
  {
    if (isConsistent)
    {
      return S_OK;
    }

    // In some cases there are text A/B flags. We don't use them as they tend
    // to bounce around.
    LogDebug(L"RDS: %s change", textContent);
    seenSegmentMask = 0;
    if (totalSegmentCount > 4)
    {
      memset(text2, 0, byteCountPerSegment * totalSegmentCount);
      memcpy(&text2[textOffset], data, byteCountPerSegment);
    }
  }
  else if (seenSegmentMask == 0)
  {
    LogDebug(L"RDS: %s seen", textContent);
  }

  seenSegmentMask |= segmentFlag;
  unsigned long completeSegmentMask = (unsigned long)((unsigned long long)1 << totalSegmentCount) - 1;
  bool isChangeComplete = seenSegmentMask == completeSegmentMask;
  if (!isChangeComplete && totalSegmentCount > 4)
  {
    // Change completion can be obvious when all segments are used, but it can
    // also be signalled by a carriage return character when some segments
    // aren't needed.
    unsigned char totalTextByteCount = byteCountPerSegment * totalSegmentCount;
    for (unsigned char i = 0; i < totalTextByteCount; i++)
    {
      char c = text2[i];
      if (textType == Utf16)
      {
        i++;
        if (c != NULL)
        {
          continue;
        }
        c = text2[i + 1];
      }
      if (c == NULL)  // some data not yet received
      {
        break;
      }
      if (c == 0xd)   // carriage return signals completion
      {
        isChangeComplete = true;
        seenSegmentMask = completeSegmentMask;
        break;
      }
    }
  }

  if (seenSegmentMask == completeSegmentMask)
  {
    char* string = NULL;
    RdsTextToString(text2,
                    byteCountPerSegment * totalSegmentCount,
                    textType,
                    &string);
    if (string == NULL)
    {
      LogDebug(L"RDS: %s complete, failed to convert", textContent);
      return E_OUTOFMEMORY;
    }
    LogDebug(L"RDS: %s complete, value = %S", textContent, string);
    delete[] string;
  }
  return S_OK;
}

void CParserRds::RdsTextToString(unsigned char* data,
                                  unsigned long dataLength,
                                  TextType textType,
                                  char** text)
{
  if (
    data == NULL ||
    dataLength == 0 ||
    (textType == Utf16 && dataLength % 2 != 0) ||
    (textType != Utf16 && (data[0] == NULL || data[0] == 0xd)) ||
    (textType == Utf16 && data[0] == NULL && (data[1] == NULL || data[1] == 0xd))
  )
  {
    *text = NULL;
    return;
  }

  // Convert from RDS native encoding (1 byte per character) => UTF-16 => UTF-8.
  unsigned long textBufferSize;
  if (textType == Standard)
  {
    textBufferSize = (dataLength * 3) + 2;
  }
  else if (textType == Utf8)
  {
    textBufferSize = dataLength + 2;
  }
  else if (textType == Utf16)
  {
    textBufferSize = (dataLength / 2) + 2;
  }
  *text = new char[textBufferSize];
  if (*text == NULL)
  {
    return;
  }

  (*text)[0] = 0x15;
  unsigned long textBufferIndex = 1;
  for (unsigned long i = 0; i < dataLength; i++)
  {
    unsigned short rdsChar = data[i];
    if (textType == Utf16)
    {
      i++;
      rdsChar = (rdsChar << 8) | data[i];
    }
    if (rdsChar == NULL || rdsChar == 0xd)
    {
      break;
    }

    if (rdsChar == 0xb)   // end of headline
    {
      rdsChar = 0x20;     // substitute with space
    }
    if (rdsChar == 0x1f)  // soft hyphen (preferred break position)
    {
      continue;
    }

    if (textType == Utf8)
    {
      (*text)[textBufferIndex++] = (char)rdsChar;
      continue;
    }

    unsigned short utf16Char = rdsChar;
    if (textType == Standard)
    {
      utf16Char = RDS_TO_UTF16[rdsChar];
      if (utf16Char == NULL)
      {
        LogDebug(L"muxer: unsupported RDS character, code = %hhu", rdsChar);
        utf16Char = 0x20;
      }
    }

    if (utf16Char < 0x80)
    {
      (*text)[textBufferIndex++] = (char)utf16Char;
    }
    else if (utf16Char < 0x800)
    {
      (*text)[textBufferIndex++] = (char)(((utf16Char >> 6) & 0x1f) | 0xc0);
      (*text)[textBufferIndex++] = (char)((utf16Char & 0x3f) | 0x80);
    }
    else
    {
      (*text)[textBufferIndex++] = (char)(((utf16Char >> 12) & 0xf) | 0xe0);
      (*text)[textBufferIndex++] = (char)(((utf16Char >> 6) & 0x3f) | 0x80);
      (*text)[textBufferIndex++] = (char)((utf16Char & 0x3f) | 0x80);
    }
  }
  (*text)[textBufferIndex] = NULL;
}