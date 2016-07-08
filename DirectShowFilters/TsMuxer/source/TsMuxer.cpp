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
#include "TsMuxer.h"
#include <cstddef>    // NULL
#include <cstring>    // memcpy(), memset(), strlen(), strncpy()
#include <iomanip>    // setfill(), setw()
#include <shlobj.h>   // SHGetSpecialFolderPathW()
#include <sstream>
#include <stdio.h>    // _wfopen(), fclose()
#include <Windows.h>  // MAX_PATH
#include "..\..\shared\BasePmtParser.h"
#include "..\..\shared\DvbUtil.h"
#include "..\..\shared\Pcr.h"
#include "..\..\shared\TimeUtils.h"
#include "Hamming.h"
#include "Version.h"

using namespace std;


#define STREAM_ID_END_OF_STREAM 0xb9
#define STREAM_ID_PACK 0xba
#define STREAM_ID_SYSTEM_HEADER 0xbb
#define STREAM_ID_PROGRAM_STREAM_MAP 0xbc
#define STREAM_ID_PRIVATE_STREAM_1 0xbd
#define STREAM_ID_PADDING 0xbe
#define STREAM_ID_AUDIO_FIRST 0xc0
#define STREAM_ID_VIDEO_FIRST 0xe0

#define PID_NOT_SET 0
#define PID_PAT 0
#define PID_PMT_FIRST 0x20
#define PID_SDT 0x11
#define PID_STREAM_FIRST 0x90

#define TABLE_ID_PAT 0
#define TABLE_ID_PMT 2
#define TABLE_ID_SDT 0x42

#define SERVICE_TYPE_NOT_SET -1
#define SERVICE_TYPE_TELEVISION 1
#define SERVICE_TYPE_RADIO 2

#define DESCRIPTOR_DVB_SERVICE 0x48

#define SERVICE_ID 1
#define TRANSPORT_STREAM_ID 1
#define ORIGINAL_NETWORK_ID 1

#define VBI_SERVICE_NAME_TIMEOUT 5000

#define TS_HEADER_LENGTH 4
#define PTS_LENGTH 5                      // ... bytes. Refer to ISO/IEC 13818 part 1 table 2-21.
#define PCR_LENGTH 6                      // ... bytes. Refer to ISO/IEC 13818 part 1 table 2-6.

#define VERSION_NOT_SET 255
#define TIME_NOT_SET -1

#define DATA_IDENTIFIER_EBU_DATA 0x10     // Could be 0x10 to 0x1f or 0x99 to 0x9b. Refer to the notes in EN 301 775 section 4.1 (about data_identifier and compliance with EN 300 472) and 4.4.2 (about data_unit_length).
#define VBI_PES_LINE_LENGTH 46            // ... bytes. Refer to EN 301 775 section 4.4.1. Includes data_unit_id, data_unit_length (fixed length = 0x2c, due to choice of data identifier above), data and stuffing.
#define VBI_PES_HEADER_DATA_LENGTH 36     // ... bytes. Refer to EN 301 775 section 4.3.
#define VBI_DATA_UNIT_LENGTH 0x2c         // ... bytes. Refer to EN 301 775 section 4.4.2. Length is fixed due to the choice of data identifier above.

#define TELETEXT_DATA_UNIT_ID 0x02        // EBU teletext non-subtitle data
#define TELETEXT_LINE_LENGTH 43           // ... bytes. Includes framing code, magazine and packet address, data. Matches sample sizes delivered by the VBI codec.
#define TELETEXT_FIELD_PARITY 0           // Could be 0 or 1. The input doesn't tell us what this should actually be.
#define TELETEXT_LINE_OFFSET 0            // Could be 0 (undefined) or between 7 and 22. The input doesn't tell us what this should actually be.

#define VPS_DATA_UNIT_ID 0xc3             // VPS data
#define VPS_LINE_LENGTH 13                // ... bytes. Excludes run in and start code bytes. Guess based on EN 301 775 section 4.6.1.
#define VPS_FIELD_PARITY 1                // Refer to EN 301 775 section 4.6.2.
#define VPS_LINE_OFFSET 16                // Refer to EN 301 775 section 4.6.2.

#define WSS_DATA_UNIT_ID 0xc4             // WSS data
#define WSS_LINE_LENGTH 2                 // ... bytes. Guess based on EN 301 775 section 4.7.2.
#define WSS_FIELD_PARITY 1                // Refer to EN 301 775 section 4.7.2.
#define WSS_LINE_OFFSET 23                // Refer to EN 301 775 section 4.7.2.

#define DATA_SERVICE_ID_VPS 0x04          // VPS service
#define DATA_SERVICE_ID_WSS 0x05          // WSS service

#define TS_BUFFER_FLUSH_RATE_VIDEO 50
#define TS_BUFFER_FLUSH_RATE_AUDIO 5


// Table for inverting/reversing bit ordering of teletext/VBI bytes.
const unsigned char REVERSE_BITS[256] =
{
  0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0, 0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
  0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8, 0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
  0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4, 0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
  0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec, 0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
  0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2, 0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
  0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea, 0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
  0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6, 0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
  0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee, 0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
  0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1, 0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
  0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9, 0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
  0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5, 0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
  0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed, 0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
  0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3, 0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
  0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb, 0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
  0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7, 0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
  0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef, 0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff
};

// 0 = even, 1 = odd
const unsigned char PARITY[256] =
{
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
  0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0
};


const unsigned short AUDIO_BIT_RATES[2][3][15] =
{
  // MPEG 1 (ISO/IEC 11172-3)
  {
    // layer 1
    { 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448 },
    // layer 2
    { 0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384 },
    // layer 3
    { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320 }
  },
  // MPEG 2 (ISO/IEC 13818-3)
  {
    // layer 1
    { 0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256 },
    // layer 2
    { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160 },
    // layer 3
    { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160 }
  }
};

const long AUDIO_SAMPLE_RATES[2][3] =
{
  // MPEG 1 (ISO/IEC 11172-3)
  { 44100, 48000, 32000 },
  // MPEG 2 (ISO/IEC 13818-3)
  { 22050, 24000, 16000 }
};

const double VIDEO_ASPECT_RATIOS[5] = { 0, 1, 4.3, 16.9, 2.21 };
const double VIDEO_FRAME_RATES[9] = { 0, 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };


//-----------------------------------------------------------------------------
// DIRECTSHOW CONFIGURATION
//-----------------------------------------------------------------------------
const AMOVIESETUP_PIN PIN_TYPE_INFORMATION[] =
{
  {
    L"Input",                   // [obsolete] the pin type name
    FALSE,                      // does the filter render this pin type?
    FALSE,                      // is this an output pin?
    FALSE,                      // can the filter have zero instances of this pin type?
    TRUE,                       // can the filter have multiple instances of this pin type?
    &CLSID_NULL,                // [obsolete] the CLSID of the filter that this pin type connects to
    L"Output",                  // [obsolete] the name of the pin that this pin type connects to
    INPUT_MEDIA_TYPE_COUNT,     // the number of media types supported by this pin type
    INPUT_MEDIA_TYPES           // the media types supported by this pin type
  },
  {
    L"TS Output",               // [obsolete] the pin type name
    FALSE,                      // [not applicable] does the filter render this pin type?
    TRUE,                       // is this an output pin?
    FALSE,                      // can the filter have zero instances of this pin type?
    FALSE,                      // can the filter have multiple instances of this pin type?
    &CLSID_NULL,                // [obsolete] the CLSID of the filter that this pin type connects to
    L"Input",                   // [obsolete] the name of the pin that this pin type connects to
    OUTPUT_MEDIA_TYPE_COUNT,    // the number of media types supported by this pin type
    OUTPUT_MEDIA_TYPES          // the media types supported by this pin type
  }
};

const AMOVIESETUP_FILTER FILTER_INFORMATION =
{
  &CLSID_TS_MUXER,              // CLSID
  L"MediaPortal TS Muxer",      // name
  MERIT_DO_NOT_USE,             // merit
  2,                            // pin type count
  PIN_TYPE_INFORMATION,         // pin type details
  CLSID_LegacyAmFilterCategory  // category
};

CFactoryTemplate g_Templates[] =
{
  L"MediaPortal TS Muxer", &CLSID_TS_MUXER, CTsMuxer::CreateInstance, NULL, &FILTER_INFORMATION
};
int g_cTemplates = 1;

STDAPI DllRegisterServer()
{
  return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2(FALSE);
}

extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE module, DWORD reason, LPVOID reserved)
{
  return DllEntryPoint((HINSTANCE)module, reason, reserved);
}


//-----------------------------------------------------------------------------
// LOGGING
//-----------------------------------------------------------------------------
static CCritSec g_logLock;
static CCritSec g_logFilePathLock;
static wstringstream g_logFilePath;
static wstringstream g_logFileName;
static short g_currentDay = -1;
static wchar_t g_logBuffer[2000];

void LogDebug(const wchar_t* fmt, ...)
{
  CAutoLock lock(&g_logLock);
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  if (g_currentDay != systemTime.wDay)
  {
    CAutoLock lock(&g_logFilePathLock);
    g_logFileName.str(wstring());
    g_logFileName << g_logFilePath.str() << L"\\TsMuxer-" <<
                      systemTime.wYear << L"-" << setfill(L'0') << setw(2) <<
                      systemTime.wMonth << L"-" << setw(2) <<
                      systemTime.wDay << L".log";
    g_currentDay = systemTime.wDay;
  }

  FILE* file = _wfopen(g_logFileName.str().c_str(), L"a+, ccs=UTF-8");
  if (file != NULL)
  {
    va_list ap;
    va_start(ap, fmt);
    vswprintf(g_logBuffer, sizeof(g_logBuffer) / sizeof(g_logBuffer[0]), fmt, ap);
    va_end(ap);
    fwprintf(file, L"%04.4hd-%02.2hd-%02.2hd %02.2hd:%02.2hd:%02.2hd.%03.3hd %s\n",
              systemTime.wYear, systemTime.wDay, systemTime.wMonth,
              systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
              systemTime.wMilliseconds, g_logBuffer);
    fclose(file);
  }

  //::OutputDebugStringW(g_logBuffer);
  //::OutputDebugStringW(L"\n");
};


//-----------------------------------------------------------------------------
// MULTIPLEXER CLASS
//-----------------------------------------------------------------------------
CCniRegister CTsMuxer::m_cniRegister;

CTsMuxer::CTsMuxer(LPUNKNOWN unk, HRESULT* hr)
  : CUnknown(NAME("TS Muxer"), unk)
{
  wchar_t temp[MAX_PATH];
  ::SHGetSpecialFolderPathW(NULL, temp, CSIDL_COMMON_APPDATA, FALSE);
  g_logFilePath << temp << L"\\Team MediaPortal\\MediaPortal TV Server\\log";

  LogDebug(L"--------------- v%d.%d.%d.0 ---------------",
            VERSION_TS_MUXER_MAJOR, VERSION_TS_MUXER_MINOR,
            VERSION_TS_MUXER_MICRO);
  LogDebug(L"initial version");
  LogDebug(L"muxer: constructor");

  m_filter = new CTsMuxerFilter(this,
                                g_logFilePath.str().c_str(),
                                GetOwner(),
                                &m_filterLock,
                                m_receiveLock,
                                hr);
  if (m_filter == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"muxer: failed to allocate filter, hr = 0x%x", *hr);
    return;
  }

  m_isStarted = false;
  m_isVideoActive = true;
  m_isAudioActive = true;
  m_isTeletextActive = true;
  m_isVpsActive = true;
  m_isWssActive = true;

  unsigned char payloadStartFlag = 0x40;

  unsigned char* pointer = &m_patPacket[0];
  *pointer++ = TS_PACKET_SYNC;
  *pointer++ = payloadStartFlag | (PID_PAT >> 8);
  *pointer++ = PID_PAT & 0xff;
  *pointer++ = 0x10;
  *pointer++ = 0;     // pointer byte
  *pointer++ = TABLE_ID_PAT;
  *pointer++ = 0xb0;
  *pointer++ = 0x0d;  // section length
  *pointer++ = TRANSPORT_STREAM_ID >> 8;
  *pointer++ = TRANSPORT_STREAM_ID & 0xff;
  *pointer++ = 0xc1;  // version
  *pointer++ = 0;     // section number
  *pointer++ = 0;     // last section number
  *pointer++ = SERVICE_ID >> 8;
  *pointer++ = SERVICE_ID & 0xff;
  *pointer++ = 0xe0 | (PID_PMT_FIRST >> 8);
  *pointer++ = PID_PMT_FIRST & 0xff;
  unsigned long crc = CalculatCrc32(&m_patPacket[5], 12);
  *pointer++ = crc >> 24;
  *pointer++ = (crc >> 16) & 0xff;
  *pointer++ = (crc >> 8) & 0xff;
  *pointer++ = crc & 0xff;
  memset(pointer, 0xff, 171);

  pointer = &m_pmtPacket[0];
  *pointer++ = TS_PACKET_SYNC;
  *pointer++ = payloadStartFlag | (PID_PMT_FIRST >> 8);
  *pointer++ = PID_PMT_FIRST & 0xff;
  *pointer++ = 0x10;
  *pointer++ = 0;     // pointer byte
  *pointer++ = TABLE_ID_PMT;
  *pointer++ = 0xb0;
  *pointer++ = 0;     // section length
  *pointer++ = SERVICE_ID >> 8;
  *pointer++ = SERVICE_ID & 0xff;
  *pointer++ = 0xc1;  // version
  *pointer++ = 0;     // section number
  *pointer++ = 0;     // last section number
  *pointer++ = 0xe0;
  *pointer++ = 0;     // PCR PID
  *pointer++ = 0xf0;
  *pointer++ = 0;     // program info length

  pointer = &m_sdtPacket[0];
  *pointer++ = TS_PACKET_SYNC;
  *pointer++ = payloadStartFlag | (PID_SDT >> 8);
  *pointer++ = PID_SDT & 0xff;
  *pointer++ = 0x10;
  *pointer++ = 0;     // pointer byte
  *pointer++ = TABLE_ID_SDT;
  *pointer++ = 0xf0;
  *pointer++ = 0;     // section length
  *pointer++ = TRANSPORT_STREAM_ID >> 8;
  *pointer++ = TRANSPORT_STREAM_ID & 0xff;
  *pointer++ = 0xc1;  // version
  *pointer++ = 0;     // section number
  *pointer++ = 0;     // last section number
  *pointer++ = ORIGINAL_NETWORK_ID >> 8;
  *pointer++ = ORIGINAL_NETWORK_ID & 0xff;
  *pointer++ = 0xff;
  *pointer++ = SERVICE_ID >> 8;
  *pointer++ = SERVICE_ID & 0xff;
  *pointer++ = 0xfc;  // no EIT
  *pointer++ = 0x80;  // running, not encrypted
  *pointer++ = 0;     // descriptor loop length

  m_patContinuityCounter = 0;

  m_pmtContinuityCounter = 0;
  m_pmtPid = PID_PMT_FIRST;
  m_pmtVersion = 0;

  ResetSdtInfo();
  m_sdtContinuityCounter = 0;
  m_serviceType = SERVICE_TYPE_NOT_SET;
  m_sdtResetTime = TIME_NOT_SET;

  m_packetCounter = 0;
  m_pcrPid = PID_NOT_SET;
  m_nextStreamPid = PID_STREAM_FIRST;
  m_nextVideoStreamId = STREAM_ID_VIDEO_FIRST;
  m_nextAudioStreamId = STREAM_ID_AUDIO_FIRST;

  LogDebug(L"muxer: completed");
}

CTsMuxer::~CTsMuxer()
{
  LogDebug(L"muxer: destructor");
  map<unsigned long, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  for ( ; sIt != m_streamInfo.end(); sIt++)
  {
    if (sIt->second != NULL)
    {
      unsigned char* bytes = sIt->second->PmtDescriptorBytes;
      if (bytes != NULL)
      {
        delete[] bytes;
        sIt->second->PmtDescriptorBytes = NULL;
      }
      delete sIt->second;
      sIt->second = NULL;
    }
  }
  m_streamInfo.clear();

  map<unsigned char, TransportStreamInfo*>::iterator tsIt = m_transportStreamInfo.begin();
  for ( ; tsIt != m_transportStreamInfo.end(); tsIt++)
  {
    if (tsIt->second != NULL)
    {
      delete tsIt->second;
      tsIt->second = NULL;
    }
  }
  m_transportStreamInfo.clear();

  map<unsigned char, ProgramStreamInfo*>::iterator psIt = m_programStreamInfo.begin();
  for ( ; psIt != m_programStreamInfo.end(); psIt++)
  {
    if (psIt->second != NULL)
    {
      delete psIt->second;
      psIt->second = NULL;
    }
  }
  m_programStreamInfo.clear();

  if (m_filter != NULL)
  {
    delete m_filter;
    m_filter = NULL;
  }
  LogDebug(L"muxer: completed");
}

CUnknown* WINAPI CTsMuxer::CreateInstance(LPUNKNOWN unk, HRESULT* hr)
{
  ASSERT(hr);
  CTsMuxer* muxer = new CTsMuxer(unk, hr);
  if (muxer == NULL)
  {
    *hr = E_OUTOFMEMORY;
  }
  return muxer;
}

HRESULT CTsMuxer::BreakConnect(IMuxInputPin* pin)
{
  // Remove all stream information for this pin.
  unsigned char pinId = pin->GetId();
  bool removedActiveStream = false;
  map<unsigned long, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    if (sIt->second == NULL)
    {
      m_streamInfo.erase(sIt++);
    }
    else if (sIt->second->PinId == pinId)
    {
      removedActiveStream = !(sIt->second->IsIgnored) && sIt->second->IsCompatible;

      unsigned char* bytes = sIt->second->PmtDescriptorBytes;
      if (bytes != NULL)
      {
        delete[] bytes;
        sIt->second->PmtDescriptorBytes = NULL;
      }
      delete sIt->second;
      sIt->second = NULL;
      m_streamInfo.erase(sIt++);
    }
    else
    {
      sIt++;
    }
  }

  // If the pin was carrying a program, system or transport stream, remove the
  // extra info.
  if (pin->GetStreamType() == STREAM_TYPE_MPEG2_TRANSPORT_STREAM)
  {
    map<unsigned char, TransportStreamInfo*>::iterator tsIt = m_transportStreamInfo.find(pin->GetId());
    if (tsIt != m_transportStreamInfo.end())
    {
      if (tsIt->second != NULL)
      {
        delete tsIt->second;
        tsIt->second = NULL;
      }
      m_transportStreamInfo.erase(tsIt++);
    }
  }
  else if (
    pin->GetStreamType() == STREAM_TYPE_MPEG1_SYSTEM_STREAM ||
    pin->GetStreamType() == STREAM_TYPE_MPEG2_PROGRAM_STREAM
  )
  {
    map<unsigned char, ProgramStreamInfo*>::iterator psIt = m_programStreamInfo.find(pin->GetId());
    if (psIt != m_programStreamInfo.end())
    {
      if (psIt->second != NULL)
      {
        delete psIt->second;
        psIt->second = NULL;
      }
      m_programStreamInfo.erase(psIt++);
    }
  }

  // Notify the playback chain if we're running and removed one or more active
  // streams.
  if (removedActiveStream)
  {
    UpdatePmt();
  }
  return S_OK;
}

HRESULT CTsMuxer::CompleteConnect(IMuxInputPin* pin)
{
  return m_filter->AddPin();
}

bool CTsMuxer::IsStarted() const
{
  return m_isStarted;
}

HRESULT CTsMuxer::Receive(IMuxInputPin* pin,
                          unsigned char* data,
                          long dataLength,
                          REFERENCE_TIME dataStartTime)
{
  unsigned char pinId = pin->GetId();
  unsigned char streamType = pin->GetStreamType();
  //LogDebug(L"muxer: receive, pin = %hhu, stream type = 0x%hhx, data length = %ld, start time = %lld",
  //          pinId, streamType, dataLength, dataStartTime);
  if (streamType == STREAM_TYPE_UNKNOWN)
  {
    LogDebug(L"muxer: pin %hhu stream type is not known", pinId);
    return VFW_E_INVALIDMEDIATYPE;
  }

  // Update the SDT if we timed out waiting for the service name.
  if (
    m_sdtVersion == VERSION_NOT_SET &&
    CTimeUtils::ElapsedMillis(m_sdtResetTime) >= VBI_SERVICE_NAME_TIMEOUT
  )
  {
    UpdateSdt();
  }

  if (streamType == STREAM_TYPE_MPEG2_TRANSPORT_STREAM)
  {
    return ReceiveTransportStream(pin, data, dataLength, dataStartTime);
  }
  else if (
    streamType == STREAM_TYPE_MPEG1_SYSTEM_STREAM ||
    streamType == STREAM_TYPE_MPEG2_PROGRAM_STREAM
  )
  {
    return ReceiveProgramOrSystemStream(pin, data, dataLength, dataStartTime);
  }

  // We're processing an elementary stream. Find or create a StreamInfo
  // instance for the pin.
  HRESULT hr = S_OK;
  bool updatePmt = false;
  StreamInfo* info = NULL;
  map<unsigned long, StreamInfo*>::const_iterator it = m_streamInfo.find(pinId);
  if (it == m_streamInfo.end())
  {
    info = new StreamInfo();
    if (info == NULL)
    {
      LogDebug(L"muxer: failed to allocate stream info for pin %hhu", pinId);
      return E_OUTOFMEMORY;
    }
    info->PinId = pinId;
    info->StreamType = streamType;
    info->Pid = m_nextStreamPid++;
    info->ContinuityCounter = 0;
    info->IsCompatible = true;  // default assumption
    info->PmtDescriptorLength = 0;
    info->PmtDescriptorBytes = NULL;
    m_streamInfo[pinId] = info;

    // Log interesting info. Note we only support MPEG 1 and 2 video and audio.
    if (streamType == STREAM_TYPE_AUDIO_MPEG1 || streamType == STREAM_TYPE_AUDIO_MPEG2_PART3)
    {
      info->IsIgnored = !m_isAudioActive;
      info->StreamId = m_nextAudioStreamId++;
      hr = ReadAudioStreamInfo(data, dataLength, *info);
    }
    else if (streamType == STREAM_TYPE_VIDEO_MPEG1 || streamType == STREAM_TYPE_VIDEO_MPEG2)
    {
      info->IsIgnored = !m_isVideoActive;
      info->StreamId = m_nextVideoStreamId++;
      hr = ReadVideoStreamInfo(data, dataLength, *info);
    }
    else if (streamType == STREAM_TYPE_TELETEXT)
    {
      LogDebug(L"muxer: pin %hhu teletext stream", pinId);
      LogDebug(L"  sample size  = %ld bytes", dataLength);
      LogDebug(L"  PID          = %hu", info->Pid);
      info->IsIgnored = !m_isTeletextActive;
      info->StreamId = STREAM_ID_PRIVATE_STREAM_1;
      info->PmtDescriptorLength = 2;
      info->PmtDescriptorBytes = new unsigned char[2];
      if (info->PmtDescriptorBytes == NULL)
      {
        LogDebug(L"muxer: failed to allocate 2 bytes for pin %hhu's PMT descriptor bytes",
                  pinId);
        return E_OUTOFMEMORY;
      }
      info->PmtDescriptorBytes[0] = DESCRIPTOR_DVB_TELETEXT; // tag
      info->PmtDescriptorBytes[1] = 0;    // length (no page info)
    }
    else if (streamType == STREAM_TYPE_VPS || streamType == STREAM_TYPE_WSS)
    {
      unsigned char dataServiceId;
      bool active;
      unsigned char lineOffset;
      if (streamType == STREAM_TYPE_VPS)
      {
        LogDebug(L"muxer: pin %hhu VPS stream", pinId);
        dataServiceId = DATA_SERVICE_ID_VPS;
        active = m_isVpsActive;
        lineOffset = 16;
      }
      else
      {
        LogDebug(L"muxer: pin %hhu WSS stream", pinId);
        dataServiceId = DATA_SERVICE_ID_WSS;
        active = m_isWssActive;
        lineOffset = 23;
      }
      LogDebug(L"  sample size  = %ld bytes", dataLength);
      LogDebug(L"  PID          = %hu", info->Pid);
      info->IsIgnored = !active;
      info->StreamId = STREAM_ID_PRIVATE_STREAM_1;
      info->PmtDescriptorLength = 5;
      info->PmtDescriptorBytes = new unsigned char[2];
      if (info->PmtDescriptorBytes == NULL)
      {
        LogDebug(L"muxer: failed to allocate 2 bytes for pin %hhu's PMT descriptor bytes",
                  pinId);
        return E_OUTOFMEMORY;
      }
      info->PmtDescriptorBytes[0] = DESCRIPTOR_DVB_VBI_DATA; // tag
      info->PmtDescriptorBytes[1] = 3;    // length
      info->PmtDescriptorBytes[2] = dataServiceId;
      info->PmtDescriptorBytes[3] = 1;
      info->PmtDescriptorBytes[4] = 0xe0 | lineOffset;
    }
    else
    {
      LogDebug(L"muxer: pin %hhu elementary stream with type 0x%hhx is not supported",
                pinId, streamType);
      info->IsIgnored = false;
      info->IsCompatible = false;
      info->StreamType = STREAM_TYPE_UNKNOWN;
    }

    // If we fail to parse the stream info or for whatever reason can't be sure
    // that we are able to handle this stream then we won't process it.
    if (FAILED(hr))
    {
      info->IsCompatible = false;
    }

    if (info->IsIgnored && info->IsCompatible)
    {
      LogDebug(L"muxer: pin %hhu ignoring stream due to active stream type settings",
                pinId);
    }

    updatePmt = !info->IsIgnored;
  }
  else
  {
    info = it->second;
    if (!info->IsIgnored &&
      info->IsCompatible &&
      (
        info->PrevReceiveTime == NOT_RECEIVING ||
        CTimeUtils::ElapsedMillis(info->PrevReceiveTime) >= STREAM_IDLE_TIMEOUT
      )
    )
    {
      updatePmt = true;   // stream is restarting
    }
  }
  info->PrevReceiveTime = clock();
  if (updatePmt)
  {
    UpdatePmt();
  }

  // Can we start delivering samples? This requires that all pins are receiving
  // and we've seen all the substreams in program, system or transport streams.
  if (!CanDeliver() || info->IsIgnored || !info->IsCompatible)
  {
    return S_OK;
  }

  // Convert our sample time (10 MHz <=> 100 ns) to a system clock reference
  // (27 MHz <=> ~37 ns).
  REFERENCE_TIME systemClockReference = TIME_NOT_SET;
  long long ptsScr = TIME_NOT_SET;
  if (dataStartTime != TIME_NOT_SET)
  {
    systemClockReference = dataStartTime * 27 / 10;
    ptsScr = systemClockReference + 10000000;   // offset PTS from PCR by ~370 ms to give time for demuxing and decoding
  }

  // Wrap the frame into transport stream packets and deliver them.
  unsigned char* pesData = NULL;
  long pesDataLength = 0;
  if (
    info->StreamType == STREAM_TYPE_TELETEXT ||
    info->StreamType == STREAM_TYPE_VPS ||
    info->StreamType == STREAM_TYPE_WSS
  )
  {
    hr = WrapVbiData(*info, data, dataLength, ptsScr, &pesData, pesDataLength);
  }
  else
  {
    hr = WrapElementaryStreamData(*info, data, dataLength, ptsScr, &pesData, pesDataLength);
  }
  if (SUCCEEDED(hr) && pesData != NULL && pesDataLength > 0)
  {
    unsigned char* tsData = NULL;
    long tsDataLength = 0;
    hr = WrapPacketisedElementaryStreamData(*info, pesData, pesDataLength,
                                            systemClockReference, m_pcrPid,
                                            &tsData, tsDataLength);
    if (SUCCEEDED(hr) && tsData != NULL && tsDataLength > 0)
    {
      hr = DeliverTransportStreamData(tsData, tsDataLength);
      delete[] tsData;
    }
    delete[] pesData;
  }
  return hr;
}

HRESULT CTsMuxer::Reset()
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"muxer: reset");
  m_isStarted = false;
  ResetSdtInfo();
  map<unsigned long, StreamInfo*>::const_iterator sIt = m_streamInfo.begin();
  for ( ; sIt != m_streamInfo.end(); sIt++)
  {
    sIt->second->PrevReceiveTime = NOT_RECEIVING;
  }
  return S_OK;
}

HRESULT CTsMuxer::StreamTypeChange(IMuxInputPin* pin,
                                    unsigned char oldStreamType,
                                    unsigned char newStreamType)
{
  // The easiest way to handle a change of stream type is to treat it like
  // disconnecting and reconnecting a pin with a different stream/media type.
  // This will inject a PMT which is almost certainly [temporarily] incorrect.
  // It is not worth the effort to try and avoid this.
  return BreakConnect(pin);
}

STDMETHODIMP CTsMuxer::ConfigureLogging(wchar_t* path)
{
  LogDebug(L"muxer: configure logging, path = %s", path == NULL ? L"" : path);
  if (path == NULL)
  {
    return E_INVALIDARG;
  }
  HRESULT hr = m_filter->SetDumpFilePath(path);
  CAutoLock lock(&g_logFilePathLock);
  g_logFilePath.str(path);
  return hr;
}

STDMETHODIMP_(void) CTsMuxer::DumpInput(long mask)
{
  LogDebug(L"muxer: dump input, mask = 0x%lx", mask);
  m_filter->DumpInput(mask);
}

STDMETHODIMP_(void) CTsMuxer::DumpOutput(bool enable)
{
  LogDebug(L"muxer: dump output, enable = %d", enable);
  m_filter->DumpOutput(enable);
}

STDMETHODIMP CTsMuxer::SetActiveComponents(bool video, bool audio, bool teletext, bool vps, bool wss)
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"muxer: set active components, video = %d, audio = %d, teletext = %d, VPS = %d, WSS = %d",
            video, audio, teletext, vps, wss);
  m_isVideoActive = video;
  m_isAudioActive = audio;
  m_isTeletextActive = teletext;
  m_isVpsActive = vps;
  m_isWssActive = wss;

  bool haveTeletextStream = false;
  bool haveVpsStream = false;
  m_serviceType = SERVICE_TYPE_RADIO;
  map<unsigned long, StreamInfo*>::const_iterator sIt = m_streamInfo.begin();
  for ( ; sIt != m_streamInfo.end(); sIt++)
  {
    if (CPidTable::IsVideoStream(sIt->second->StreamType))
    {
      sIt->second->IsIgnored = !m_isVideoActive;
      m_serviceType = m_isVideoActive ? SERVICE_TYPE_TELEVISION : SERVICE_TYPE_RADIO;
    }
    else if (CPidTable::IsAudioStream(sIt->second->StreamType))
    {
      sIt->second->IsIgnored = !m_isAudioActive;
    }
    else if (sIt->second->StreamType == STREAM_TYPE_TELETEXT)
    {
      sIt->second->IsIgnored = !m_isTeletextActive;
      haveTeletextStream = true;
    }
    else if (sIt->second->StreamType == STREAM_TYPE_VPS)
    {
      sIt->second->IsIgnored = !m_isVpsActive;
      haveVpsStream = true;
    }
    else if (sIt->second->StreamType == STREAM_TYPE_WSS)
    {
      sIt->second->IsIgnored = !m_isWssActive;
    }
  }

  // Clear our SDT details.
  ResetSdtInfo();
  if ((!m_isTeletextActive || !haveTeletextStream) && (!m_isVpsActive || !haveVpsStream))
  {
    // No point in waiting for teletext to update the SDT if we're ignoring or
    // don't have teletext. Ditto for VPS.
    UpdateSdt();
  }

  // Update our PMT PID.
  m_pmtPid++;
  if (m_pmtPid == PID_STREAM_FIRST)
  {
    m_pmtPid = PID_PMT_FIRST;
  }

  // Update the PAT and PMT.
  UpdatePat();
  return UpdatePmt();
}

STDMETHODIMP CTsMuxer::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  /*LogDebug(L"debug: checking for interface %08x-%04hx-%04hx-%02hhx%02hhx-%02hhx%02hhx%02hhx%02hhx%02hhx%02hhx",
            iid.Data1, iid.Data2, iid.Data3, iid.Data4[0], iid.Data4[1],
            iid.Data4[2], iid.Data4[3], iid.Data4[4], iid.Data4[5],
            iid.Data4[6], iid.Data4[7]);*/
  if (ppv == NULL)
  {
    return E_POINTER;
  }

  if (iid == IID_ITS_MUXER)
  {
    return GetInterface((ITsMuxer*)this, ppv);
  }
  if (iid == IID_IBaseFilter || iid == IID_IMediaFilter || iid == IID_IPersist)
  {
    return m_filter->NonDelegatingQueryInterface(iid, ppv);
  }

  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

bool CTsMuxer::CanDeliver()
{
  if (m_isStarted)
  {
    return true;
  }

  // For each input pin...
  bool haveActiveTeletextOrVpsPin = false;
  int pinCount = m_filter->GetPinCount();
  for (int p = 0; p < pinCount; p++)
  {
    CMuxInputPin* pin = (CMuxInputPin*)m_filter->GetPin(p);
    if (pin == NULL)  // (output pin)
    {
      continue;
    }

    // It doesn't make sense to check if pins that are not connected are
    // receiving.
    IPin* connectedPin;
    HRESULT hr = pin->ConnectedTo(&connectedPin);
    if (FAILED(hr) || connectedPin == NULL)
    {
      continue;
    }

    connectedPin->Release();
    PIN_DIRECTION direction;
    hr = pin->QueryDirection(&direction);
    if (FAILED(hr) || direction != PINDIR_INPUT)
    {
      // This should never happen.
      continue;
    }

    // Check that the pin is receiving. Unless the pin is a VPS or WSS pin, we
    // must be receiving even if the stream is incompatible or we're ignoring
    // it. Unlike teletext, VPS and WSS don't seem to deliver unless data is
    // really present. Therefore we don't check/expect them.
    clock_t receiveTime = pin->GetReceiveTime();
    if (
      receiveTime == NOT_RECEIVING ||
      CTimeUtils::ElapsedMillis(receiveTime) >= STREAM_IDLE_TIMEOUT
    )
    {
      if (pin->GetStreamType() == STREAM_TYPE_VPS || pin->GetStreamType() == STREAM_TYPE_WSS)
      {
        continue;
      }
      return false;
    }

    // Check that we're receiving each substream.
    // How many substreams do we expect from this pin?
    unsigned char expectedStreamCount = 0;
    if (pin->GetStreamType() == STREAM_TYPE_MPEG2_TRANSPORT_STREAM)
    {
      map<unsigned char, TransportStreamInfo*>::const_iterator tsIt = m_transportStreamInfo.find(pin->GetId());
      if (tsIt == m_transportStreamInfo.end())
      {
        // This should never happen.
        return false;
      }

      if (tsIt->second->IsCompatible && tsIt->second->StreamCount > 0)
      {
        expectedStreamCount = tsIt->second->StreamCount;
      }
    }
    else if (
      pin->GetStreamType() == STREAM_TYPE_MPEG1_SYSTEM_STREAM ||
      pin->GetStreamType() == STREAM_TYPE_MPEG2_PROGRAM_STREAM
    )
    {
      map<unsigned char, ProgramStreamInfo*>::const_iterator psIt = m_programStreamInfo.find(pin->GetId());
      if (psIt == m_programStreamInfo.end())
      {
        // This should never happen.
        return false;
      }

      if (psIt->second->IsCompatible)
      {
        expectedStreamCount = psIt->second->VideoBound + psIt->second->AudioBound;
        if (expectedStreamCount < 0)
        {
          // We don't know how many streams are expected because we haven't
          // seen a system header or program stream map yet. This is odd
          // because usually we expect a system header immediately after the
          // first pack. Assume we've seen all streams.
          expectedStreamCount = 0;
        }
      }
    }
    else
    {
      expectedStreamCount = 1;
      if (
        (m_isTeletextActive && pin->GetStreamType() == STREAM_TYPE_TELETEXT) ||
        (m_isVpsActive && pin->GetStreamType() == STREAM_TYPE_VPS)
      )
      {
        haveActiveTeletextOrVpsPin = true;
      }
    }

    // How many substreams are we actually receiving from this pin?
    unsigned char receivedStreamCount = 0;
    map<unsigned long, StreamInfo*>::const_iterator sIt = m_streamInfo.begin();
    for ( ; sIt != m_streamInfo.end(); sIt++)
    {
      if (sIt->second->PinId == pin->GetId())
      {
        // Any non-ignored compatible stream that we're not receiving is an
        // immediate fail.
        if (
          !sIt->second->IsIgnored &&
          sIt->second->IsCompatible &&
          (
            sIt->second->PrevReceiveTime == NOT_RECEIVING ||
            CTimeUtils::ElapsedMillis(sIt->second->PrevReceiveTime) >= STREAM_IDLE_TIMEOUT
          )
        )
        {
          return false;
        }
        receivedStreamCount++;
      }
    }

    if (receivedStreamCount < expectedStreamCount)
    {
      return false;
    }
  }

  LogDebug(L"muxer: starting to deliver...");
  m_isStarted = true;

  // Waiting for SDT?
  if (m_sdtVersion == VERSION_NOT_SET)
  {
    // If we don't have teletext or VPS then we're not going to get a service
    // name, so we may as well start delivering SDT immediately.
    if (!haveActiveTeletextOrVpsPin)
    {
      UpdateSdt();
    }
    else
    {
      m_sdtResetTime = clock();
    }
  }
  return true;
}

HRESULT CTsMuxer::ReceiveTransportStream(IMuxInputPin* pin,
                                          unsigned char* data,
                                          long dataLength,
                                          REFERENCE_TIME dataStartTime)
{
  // Find or create a TransportStreamInfo instance for the pin.
  unsigned char pinId = pin->GetId();
  bool isFirstReceive = false;
  TransportStreamInfo* tsInfo = NULL;
  map<unsigned char, TransportStreamInfo*>::const_iterator it = m_transportStreamInfo.find(pinId);
  if (it == m_transportStreamInfo.end())
  {
    tsInfo = new TransportStreamInfo();
    if (tsInfo == NULL)
    {
      LogDebug(L"muxer: failed to allocate transport stream info for pin %hhu",
                pinId);
      return E_OUTOFMEMORY;
    }
    tsInfo->PinId = pinId;
    tsInfo->IsCompatible = true;
    tsInfo->PmtPid = PID_NOT_SET;
    tsInfo->PatVersion = VERSION_NOT_SET;
    tsInfo->PmtVersion = VERSION_NOT_SET;
    m_transportStreamInfo[pinId] = tsInfo;
  }
  else
  {
    tsInfo = it->second;
  }

  // Skip all processing if this transport stream is incompatible.
  if (!tsInfo->IsCompatible)
  {
    return S_OK;
  }
  else if (dataLength % TS_PACKET_LEN != 0)
  {
    LogDebug(L"muxer: pin %hhu transport stream sample not packet-aligned, data length = %ld",
              pinId, dataLength);
    tsInfo->IsCompatible = false;
    return S_OK;
  }

  unsigned char* outputBuffer = new unsigned char[dataLength];
  if (outputBuffer == NULL)
  {
    LogDebug(L"muxer: failed to allocate %ld byte(s) for a transport stream output buffer",
              dataLength);
    return E_OUTOFMEMORY;
  }
  long outputOffset = 0;

  StreamInfo* info = NULL;
  unsigned short previousPid = PID_NOT_SET;
  long inputOffset = 0;
  HRESULT hr = S_OK;
  while (dataLength >= TS_PACKET_LEN)
  {
    if (data[inputOffset] != TS_PACKET_SYNC)
    {
      LogDebug(L"muxer: pin %hhu transport stream sample not packet-aligned",
                pinId);
      tsInfo->IsCompatible = false;
      return S_OK;
    }

    unsigned short pid = ((data[inputOffset + 1] & 0x1f) << 8) | data[inputOffset + 2];
    if (pid == PID_PAT || (tsInfo->PmtPid != PID_NOT_SET && tsInfo->PmtPid == pid))
    {
      // Flush our buffer in case the PMT changes.
      if (CanDeliver() && outputOffset != 0)
      {
        hr = DeliverTransportStreamData(outputBuffer, outputOffset);
        if (hr != S_OK)
        {
          delete[] outputBuffer;
          return hr;
        }
      }
      outputOffset = 0;

      if (pid == PID_PAT)
      {
        hr = ReadProgramAssociationTable(&data[inputOffset + 5], TS_PACKET_LEN - 5, *tsInfo);
      }
      else
      {
        hr = ReadProgramMapTable(&data[inputOffset + 5], TS_PACKET_LEN - 5, *tsInfo);
        // Change of PMT could have changed the details for any PID, so unset
        // our cached PID details.
        info = NULL;
        previousPid = PID_NOT_SET;
      }

      if (hr != S_OK)
      {
        delete[] outputBuffer;
        return hr;
      }
    }
    else
    {
      if (pid != previousPid)
      {
        unsigned long streamKey = (pid << 8) | pin->GetId();
        map<unsigned long, StreamInfo*>::const_iterator sIt = m_streamInfo.find(streamKey);
        if (sIt != m_streamInfo.end())
        {
          info = sIt->second;
        }
        else
        {
          info = NULL;    // ignore this stream - not part of the service
        }
      }

      if (
        info != NULL &&
        info->IsCompatible &&
        (!info->IsIgnored || tsInfo->PcrPid == pid)
      )
      {
        // If we're ignoring the stream that contains PCR then overwrite the
        // content of the packet with padding and inject the packet into our
        // selected PCR stream.
        if (info->IsIgnored)
        {
          unsigned char adaptationFieldControl = (data[inputOffset + 3] >> 4) & 0x03;
          unsigned char adaptationFieldLength = data[inputOffset + TS_HEADER_LENGTH];
          if (
            (adaptationFieldControl & 0x2) != 0 &&
            adaptationFieldLength > 0 &&
            (data[inputOffset + 5] & 0x10) != 0
          )
          {
            memcpy(&outputBuffer[outputOffset], &data[inputOffset], TS_PACKET_LEN);

            // Fix the PID, adaptation field control and continuity counter.
            outputBuffer[outputOffset + 1] = m_pcrPid >> 8;
            outputBuffer[outputOffset + 2] = m_pcrPid & 0xff;
            map<unsigned long, StreamInfo*>::const_iterator sIt = m_streamInfo.begin();
            for ( ; sIt != m_streamInfo.end(); sIt++)
            {
              if (sIt->second->Pid == m_pcrPid)
              {
                outputBuffer[outputOffset + 3] = 0x20 | sIt->second->ContinuityCounter;     // adaptation field only, not scrambled
              }
            }

            // Overwrite the content with padding.
            unsigned char fullAdaptationByteCount = TS_PACKET_LEN - TS_HEADER_LENGTH - 1;   // - 1 for adaptation field length
            unsigned char additionalPaddingByteCount = fullAdaptationByteCount - adaptationFieldLength;
            outputBuffer[outputOffset + TS_HEADER_LENGTH] = fullAdaptationByteCount;
            if (additionalPaddingByteCount > 0)
            {
              memset(&outputBuffer[outputOffset + TS_HEADER_LENGTH + adaptationFieldLength], 0xff, additionalPaddingByteCount);
            }
          }
        }
        else
        {
          memcpy(&outputBuffer[outputOffset], &data[inputOffset], TS_PACKET_LEN);
        }

        outputOffset += TS_PACKET_LEN;
      }

      previousPid = pid;
    }

    dataLength -= TS_PACKET_LEN;
    inputOffset += TS_PACKET_LEN;
  }

  if (outputOffset != 0)
  {
    hr = DeliverTransportStreamData(outputBuffer, outputOffset);
  }
  delete[] outputBuffer;
  return hr;
}

HRESULT CTsMuxer::ReceiveProgramOrSystemStream(IMuxInputPin* pin,
                                                unsigned char* data,
                                                long dataLength,
                                                REFERENCE_TIME dataStartTime)
{
  // Find or create a ProgramStreamInfo instance for the pin.
  unsigned char pinId = pin->GetId();
  bool isFirstReceive = false;
  ProgramStreamInfo* psInfo = NULL;
  map<unsigned char, ProgramStreamInfo*>::const_iterator it = m_programStreamInfo.find(pinId);
  if (it == m_programStreamInfo.end())
  {
    isFirstReceive = true;
    psInfo = new ProgramStreamInfo();
    if (psInfo == NULL)
    {
      LogDebug(L"muxer: failed to allocate program stream info for pin %hhu",
                pinId);
      return E_OUTOFMEMORY;
    }
    psInfo->PinId = pinId;
    psInfo->IsCompatible = true;
    psInfo->VideoBound = -1;
    psInfo->AudioBound = -1;
    psInfo->CurrentMapVersion = VERSION_NOT_SET;
    m_programStreamInfo[pinId] = psInfo;
  }
  else
  {
    psInfo = it->second;
  }

  // Update the SDT if we timed out waiting for the service name.
  if (
    m_sdtVersion == VERSION_NOT_SET &&
    CTimeUtils::ElapsedMillis(m_sdtResetTime) >= VBI_SERVICE_NAME_TIMEOUT
  )
  {
    UpdateSdt();
  }

  // Skip all processing if we previously determined that this program/system
  // stream is incompatible.
  if (!psInfo->IsCompatible)
  {
    return S_OK;
  }

  // Process each program or system stream frame.
  long offset = 0;
  long remainingDataLength = dataLength;
  long long systemClockReference = TIME_NOT_SET;
  while (remainingDataLength >= 4)
  {
    // All samples must be aligned to frame boundaries.
    if (data[offset] != 0 || data[offset + 1] != 0 || data[offset + 2] != 1)
    {
      LogDebug(L"muxer: pin %hhu program/system stream sample not frame-aligned",
                pinId);
      psInfo->IsCompatible = false;
      return VFW_E_BADALIGN;
    }
    unsigned char streamId = data[offset + 3];
    HRESULT hr = S_OK;

    // What type of frame is this and how do we handle it?
    if (streamId == STREAM_ID_END_OF_STREAM)
    {
      LogDebug(L"muxer: pin %hhu received program/system stream end code",
                pinId);
      offset += 4;
      remainingDataLength -= 4;
    }
    else if (streamId == STREAM_ID_PACK)
    {
      unsigned short length = 0;
      hr = ReadProgramOrSystemPack(&data[offset],
                                    remainingDataLength,
                                    *psInfo,
                                    isFirstReceive,
                                    length,
                                    systemClockReference);
      offset += length;
      remainingDataLength -= length;
    }
    else
    {
      if (remainingDataLength < 6)
      {
        LogDebug(L"muxer: pin %hhu program/system stream packet with length %ld is too small to contain a system header, program stream map, padding or data",
                  pinId, remainingDataLength);
        psInfo->IsCompatible = false;
        return E_NOT_SUFFICIENT_BUFFER;
      }
      // Ideally we'd have a way to validate that the packet length is correct.
      unsigned short packetLength = (data[offset + 4] << 8) | data[offset + 5];
      if (remainingDataLength < packetLength + 6)
      {
        LogDebug(L"muxer: pin %hhu program/system stream appears to spread packets across samples, not compatible",
                  pinId);
        psInfo->IsCompatible = false;
        return VFW_E_UNSUPPORTED_STREAM;
      }

      if (streamId == STREAM_ID_SYSTEM_HEADER)
      {
        hr = ReadProgramOrSystemHeader(&data[offset],
                                        remainingDataLength,
                                        *psInfo,
                                        isFirstReceive);
      }
      else if (streamId == STREAM_ID_PROGRAM_STREAM_MAP)
      {
        hr = ReadProgramStreamMap(&data[offset], remainingDataLength, *psInfo);
      }
      else if (streamId == STREAM_ID_PADDING)
      {
        // (nothing to do)
      }
      // content
      else
      {
        // Find or create a StreamInfo instance for the substream.
        bool updatePmt = false;
        unsigned long streamKey = (streamId << 8) | pinId;
        StreamInfo* info = NULL;
        map<unsigned long, StreamInfo*>::const_iterator it = m_streamInfo.find(streamKey);
        if (it == m_streamInfo.end())
        {
          info = new StreamInfo();
          if (info == NULL)
          {
            LogDebug(L"muxer: failed to allocate stream info for pin %hhu stream 0x%hhx",
                      pinId, streamId);
            return E_OUTOFMEMORY;
          }
          info->PinId = pinId;
          info->Pid = m_nextStreamPid++;
          info->ContinuityCounter = 0;
          info->IsCompatible = true;  // default assumption
          info->PmtDescriptorLength = 0;
          info->PmtDescriptorBytes = NULL;
          m_streamInfo[streamKey] = info;

          long offsetToPesData = offset + 9 + data[offset + 8];
          if ((streamId & 0xe0) == STREAM_ID_AUDIO_FIRST)
          {
            info->IsIgnored = !m_isAudioActive;
            info->StreamId = m_nextAudioStreamId++;
            hr = ReadAudioStreamInfo(&data[offsetToPesData],
                                      remainingDataLength - offsetToPesData,
                                      *info);
          }
          else if ((streamId & 0xf0) == STREAM_ID_VIDEO_FIRST)
          {
            info->IsIgnored = !m_isVideoActive;
            info->StreamId = m_nextVideoStreamId++;
            hr = ReadVideoStreamInfo(&data[offsetToPesData],
                                      remainingDataLength - offsetToPesData,
                                      *info);
          }
          else
          {
            // other unsupported stream types - private, ECM, EMM, DCMCC, MHEG etc.
            LogDebug(L"muxer: pin %hhu program/system stream PES packet with stream ID 0x%hhx is not supported",
                      pinId, streamId);
            info->IsIgnored = false;
            info->IsCompatible = false;
          }

          // If we fail to parse the stream info or for whatever reason can't be sure
          // that we are able to handle this stream then we won't process it.
          if (FAILED(hr))
          {
            info->IsCompatible = false;
          }

          if (info->IsIgnored && info->IsCompatible)
          {
            LogDebug(L"muxer: pin %hhu ignoring substream 0x%hhx due to active stream type settings",
                      pinId, streamId);
          }

          updatePmt = !info->IsIgnored;
        }
        else
        {
          info = it->second;
          if (!info->IsIgnored &&
            info->IsCompatible &&
            (
              info->PrevReceiveTime == NOT_RECEIVING ||
              CTimeUtils::ElapsedMillis(info->PrevReceiveTime) >= STREAM_IDLE_TIMEOUT
            )
          )
          {
            updatePmt = true;   // stream is restarting
          }
        }
        info->PrevReceiveTime = clock();
        if (updatePmt)
        {
          UpdatePmt();
        }

        // Wrap the PES packet into transport stream packets and deliver them.
        if (info->IsCompatible && !info->IsIgnored && CanDeliver())
        {
          unsigned char* tsData = NULL;
          long tsDataLength = 0;
          systemClockReference = info->Pid == m_pcrPid ? systemClockReference : TIME_NOT_SET;   // only put PCR into the PCR stream
          hr = WrapPacketisedElementaryStreamData(*info,
                                                  &data[offset],
                                                  packetLength + 6,
                                                  systemClockReference,
                                                  m_pcrPid,
                                                  &tsData,
                                                  tsDataLength);
          if (SUCCEEDED(hr) && tsData != NULL && tsDataLength > 0)
          {
            hr = DeliverTransportStreamData(tsData, tsDataLength);
            delete[] tsData;
          }
        }
        systemClockReference = TIME_NOT_SET;    // PCR applies to the first frame
      }

      // Move on to the next frame...
      offset += packetLength + 6;
      remainingDataLength -= (packetLength + 6);
    }

    // Get out now if we encountered any error while processing this frame.
    if (FAILED(hr))
    {
      return hr;
    }
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramAssociationTable(unsigned char* data,
                                              long dataLength,
                                              TransportStreamInfo& info)
{
  unsigned char version = (data[5] >> 1) & 0x1f;
  if (info.PatVersion == version)
  {
    return S_OK;
  }
  info.PatVersion = version;

  unsigned short transportStreamId = (data[3] << 8) | data[4];
  LogDebug(L"muxer: pin %hhu new program association table, version = %hhu, transport stream ID = %hu",
            info.PinId, version, transportStreamId);
  info.TransportStreamId = transportStreamId;

  unsigned short sectionLength = ((data[1] & 0x0f) << 8) | data[2];
  unsigned short offset = 8;
  unsigned short endOfPrograms = offset + (sectionLength - 4 - 5);  // - 4 for CRC, - 5 for other fixed length fields
  unsigned char programCount = 0;
  while (offset + 3 < endOfPrograms)
  {
    unsigned short programNumber = (data[offset] << 8) | data[offset + 1];
    unsigned short pmtPid = ((data[offset + 2] & 0x1f) << 8) | data[offset + 3];
    LogDebug(L"  program, program number = %hu, PMT PID = %hu",
              programNumber, pmtPid);
    if (programNumber != 0)
    {
      programCount++;
      // We can only handle transport streams containing one program. Take the
      // first real program (program number 0 points to the network PID).
      if (programCount == 1)
      {
        info.ServiceId = programNumber;
        info.PmtPid = pmtPid;
      }
    }
    offset += 4;
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramMapTable(unsigned char* data,
                                      long dataLength,
                                      TransportStreamInfo& info)
{
  CSection section;
  section.AppendData(data, dataLength);
  if (!section.IsComplete() || section.section_length + 4 > TS_PACKET_LEN) // + 4 for pointer field, table ID, section length bytes
  {
    // Section doesn't fit in one packet.
    LogDebug(L"muxer: pin %hhu larger-than-TS-packet PMT section not supported",
              info.PinId);
    info.IsCompatible = false;
    return S_FALSE;
  }
  if (!section.IsValid())
  {
    LogDebug(L"muxer: pin %hhu PMT section is invalid", info.PinId);
    return S_OK;
  }
  if (info.PmtVersion == section.version_number || info.ServiceId != section.table_id_extension)
  {
    return S_OK;
  }

  CBasePmtParser pmtParser;
  if (!pmtParser.DecodePmtSection(section))
  {
    LogDebug(L"muxer: pin %hhu failed to decode PMT section", info.PinId);
    info.IsCompatible = false;
    return S_FALSE;
  }
  info.PmtVersion = section.version_number;

  CPidTable& pidTable = pmtParser.GetPidInfo();
  LogDebug(L"muxer: pin %hhu new program map table, version = %hhu, program number = %hu, PCR PID = %hu",
            info.PinId, pidTable.PmtVersion, pidTable.ProgramNumber,
            pidTable.PcrPid);
  info.PcrPid = pidTable.PcrPid;

  // Set a "marker" on all substreams for this pin so we can remove inactive
  // streams at the end.
  map<unsigned long, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  for ( ; sIt != m_streamInfo.end(); sIt++)
  {
    if (sIt->second->PinId == info.PinId)
    {
      sIt->second->IsIgnored = true;
      sIt->second->StreamId = 1;
    }
  }

  long offset = 0;
  while (offset + 1 < pidTable.DescriptorsLength)
  {
    unsigned char tag = pidTable.Descriptors[offset++];
    unsigned char length = pidTable.Descriptors[offset++];
    offset += length;
    LogDebug(L"  program descriptor, tag = 0x%hhx, length = %hhu",
              tag, length);
  }

  vector<VideoPid*>::const_iterator vIt = pidTable.VideoPids.begin();
  for ( ; vIt != pidTable.VideoPids.end(); vIt++)
  {
    CreateOrUpdateTsPmtEs(info, *vIt, !m_isVideoActive);
  }
  vector<AudioPid*>::const_iterator aIt = pidTable.AudioPids.begin();
  for ( ; aIt != pidTable.AudioPids.end(); aIt++)
  {
    CreateOrUpdateTsPmtEs(info, *aIt, !m_isAudioActive);
  }
  vector<SubtitlePid*>::const_iterator stIt = pidTable.SubtitlePids.begin();
  for ( ; stIt != pidTable.SubtitlePids.end(); stIt++)
  {
    CreateOrUpdateTsPmtEs(info, *stIt, !m_isVideoActive);
  }
  vector<TeletextPid*>::const_iterator tIt = pidTable.TeletextPids.begin();
  for ( ; tIt != pidTable.TeletextPids.end(); tIt++)
  {
    CreateOrUpdateTsPmtEs(info, *tIt, !m_isTeletextActive);
  }
  vector<VbiPid*>::const_iterator vbiIt = pidTable.VbiPids.begin();
  for ( ; vbiIt != pidTable.VbiPids.end(); vbiIt++)
  {
    CreateOrUpdateTsPmtEs(info, *vbiIt, !m_isWssActive);
  }

  // Remove streams that are no longer part of the program.
  sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    if (sIt->second == NULL)
    {
      m_streamInfo.erase(sIt++);
    }
    else if (sIt->second->PinId == info.PinId && sIt->second->StreamId == 1)
    {
      unsigned char* bytes = sIt->second->PmtDescriptorBytes;
      if (bytes != NULL)
      {
        delete[] bytes;
        sIt->second->PmtDescriptorBytes = NULL;
      }
      delete sIt->second;
      sIt->second = NULL;
      m_streamInfo.erase(sIt++);
    }
    else
    {
      sIt++;
    }
  }

  return UpdatePmt();
}

HRESULT CTsMuxer::CreateOrUpdateTsPmtEs(TransportStreamInfo& info, BasePid* pid, bool isIgnored)
{
  unsigned long streamKey = (pid->Pid << 8) | info.PinId;
  StreamInfo* streamInfo = NULL;
  map<unsigned long, StreamInfo*>::const_iterator sIt = m_streamInfo.find(streamKey);
  if (sIt == m_streamInfo.end())
  {
    streamInfo = new StreamInfo();
    if (streamInfo == NULL)
    {
      LogDebug(L"muxer: failed to allocate stream info for pin %hhu PID %hu",
                info.PinId, pid->Pid);
      return E_OUTOFMEMORY;
    }
    streamInfo->OriginalPid = pid->Pid;
    streamInfo->ContinuityCounter = -1;
    streamInfo->IsCompatible = true;
    streamInfo->Pid = m_nextStreamPid++;
    streamInfo->PinId = info.PinId;
    streamInfo->PrevReceiveTime = NOT_RECEIVING;
    m_streamInfo[streamKey] = streamInfo;
  }
  else
  {
    streamInfo = sIt->second;
  }
  streamInfo->StreamId = 0;   // unset marker
  streamInfo->StreamType = pid->StreamType;
  streamInfo->IsIgnored = isIgnored;
  LogDebug(L"  elementary stream, PID = %hu, type = 0x%hhx, fake PID = %hu, active = %d",
            pid->Pid, pid->StreamType, streamInfo->Pid,
            !streamInfo->IsIgnored);

  if (streamInfo->PmtDescriptorBytes != NULL)
  {
    delete[] streamInfo->PmtDescriptorBytes;
    streamInfo->PmtDescriptorBytes = NULL;
  }
  if (pid->DescriptorsLength != 0)
  {
    streamInfo->PmtDescriptorLength = pid->DescriptorsLength;
    streamInfo->PmtDescriptorBytes = new unsigned char[pid->DescriptorsLength];
    if (streamInfo->PmtDescriptorBytes == NULL)
    {
      LogDebug(L"muxer: failed to allocate %hu byte(s) for pin %hhu PID %hu's PMT descriptors",
                pid->DescriptorsLength, info.PinId, pid->Pid);
      return E_OUTOFMEMORY;
    }
    memcpy(streamInfo->PmtDescriptorBytes, pid->Descriptors, pid->DescriptorsLength);
  }
  unsigned short offset = 0;
  while (offset + 1 < pid->DescriptorsLength)
  {
    unsigned char tag = pid->Descriptors[offset++];
    unsigned char length = pid->Descriptors[offset++];
    offset += length;
    LogDebug(L"    elementary stream descriptor, tag = 0x%hhx, length = %hhu",
              tag, length);
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramOrSystemPack(unsigned char* data,
                                          long dataLength,
                                          const ProgramStreamInfo& info,
                                          bool isFirstReceive,
                                          unsigned short& length,
                                          long long& systemClockReference)
{
  if (isFirstReceive)
  {
    LogDebug(L"muxer: pin %hhu program/system stream", info.PinId);
    LogDebug(L"  sample size      = %ld bytes", dataLength);
  }

  bool isMpeg2 = false;
  long programMuxRate = 0;
  unsigned char packStuffingLength = 0;
  if ((data[4] & 0xc0) == 0x40)
  {
    // MPEG 2 PS
    if (dataLength < 14)
    {
      LogDebug(L"muxer: pin %hhu program stream packet with length %ld is too small to contain a pack",
                info.PinId, dataLength);
      return E_NOT_SUFFICIENT_BUFFER;
    }
    isMpeg2 = true;
    systemClockReference = (data[4] & 0x38) << 27;
    systemClockReference += ((data[4] & 0x03) << 28);
    systemClockReference += (data[5] << 20);
    systemClockReference += ((data[6] & 0xf8) << 12);
    systemClockReference += ((data[6] & 0x03) << 13);
    systemClockReference += (data[7] << 5);
    systemClockReference += ((data[8] & 0xf8) >> 3);
    systemClockReference *= 300;

    systemClockReference += ((data[8] & 0x03) << 7);
    systemClockReference += ((data[9] & 0xfe) >> 1);

    programMuxRate = (data[10] << 14);
    programMuxRate += (data[11] << 6);
    programMuxRate += ((data[12] & 0xfc) >> 2);
    programMuxRate *= 50;

    packStuffingLength = (data[13] & 0x07);

    length = 14 + packStuffingLength;
  }
  else if ((data[4] & 0xf0) == 0x20)
  {
    // MPEG 1 PS
    if (dataLength < 12)
    {
      LogDebug(L"muxer: pin %hhu system stream packet with length %ld is too small to contain a pack",
                info.PinId, dataLength);
      return E_NOT_SUFFICIENT_BUFFER;
    }
    isMpeg2 = false;
    systemClockReference = (data[4] & 0xe) << 29;
    systemClockReference += (data[5] << 22);
    systemClockReference += ((data[6] & 0xfe) << 14);
    systemClockReference += (data[7] << 7);
    systemClockReference += ((data[8] & 0xfe) >> 1);
    systemClockReference *= 300;

    programMuxRate = (data[9] & 0x7f) << 15;
    programMuxRate += (data[10] << 7);
    programMuxRate += ((data[11] & 0xfe) >> 1);
    programMuxRate *= 50;

    length = 12;
  }
  else
  {
    LogDebug(L"muxer: pin %hhu program/system stream pack format 0x%hhx is not supported",
              info.PinId, data[4]);
    return VFW_E_UNSUPPORTED_STREAM;
  }

  if (isFirstReceive)
  {
    LogDebug(L"  is MPEG 2        = %d", isMpeg2);
    LogDebug(L"  program mux rate = %ld b/s", programMuxRate);
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramOrSystemHeader(unsigned char* data,
                                            long dataLength,
                                            ProgramStreamInfo& info,
                                            bool isFirstReceive)
{
  if (dataLength < 11)
  {
    LogDebug(L"muxer: pin %hhu system stream packet with length %ld is too small to contain a header",
              info.PinId, dataLength);
    return E_NOT_SUFFICIENT_BUFFER;
  }

  long rateBound = ((data[6] & 0x7f) << 15);
  rateBound += (data[7] << 7);
  rateBound += ((data[8] & 0xfe) >> 1);
  rateBound *= 50;

  unsigned char audioBound = data[9] >> 2;
  bool fixedFlag = (data[9] & 0x02) != 0;
  bool cspsFlag = (data[9] & 0x01) != 0;
  bool systemAudioLockFlag = (data[10] & 0x80) != 0;
  bool systemVideoLockFlag = (data[10] & 0x40) != 0;
  unsigned char videoBound = data[10] & 0x1f;
  if (isFirstReceive)
  {
    LogDebug(L"  rate bound       = %ld b/s", rateBound);
    LogDebug(L"  video bound      = %hhu", videoBound);
    LogDebug(L"  audio bound      = %hhu", audioBound);
    LogDebug(L"  fixed            = %d", fixedFlag);
    LogDebug(L"  CSPS             = %d", cspsFlag);
    LogDebug(L"  video lock       = %d", systemVideoLockFlag);
    LogDebug(L"  audio lock       = %d", systemAudioLockFlag);
    info.VideoBound = videoBound;
    info.AudioBound = audioBound;
  }
  else if (videoBound != info.VideoBound || audioBound != info.AudioBound)
  {
    if (videoBound != info.VideoBound)
    {
      LogDebug(L"muxer: pin %hhu program/system video bound changed, %hhu => %hhu",
                info.PinId, info.VideoBound, videoBound);
      info.VideoBound = videoBound;
    }
    if (audioBound != info.AudioBound)
    {
      LogDebug(L"muxer: pin %hhu program/system audio bound changed, %hhu => %hhu",
                info.PinId, info.AudioBound, audioBound);
      info.AudioBound = audioBound;
    }
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramStreamMap(unsigned char* data,
                                        long dataLength,
                                        ProgramStreamInfo& info)
{
  if (dataLength < 16)
  {
    LogDebug(L"muxer: pin %hhu system stream packet with length %ld is too small to contain a program stream map",
              info.PinId, dataLength);
    return E_NOT_SUFFICIENT_BUFFER;
  }

  bool currentNextIndicator = (data[6] & 0x80) != 0;
  unsigned char programStreamMapVersion = data[6] & 0x1f;
  if (programStreamMapVersion == info.CurrentMapVersion)
  {
    return S_OK;
  }
  LogDebug(L"muxer: pin %hhu program stream map version changed, %hhu => %hhu",
            info.PinId, info.CurrentMapVersion, programStreamMapVersion);
  info.CurrentMapVersion = programStreamMapVersion;

  unsigned short programStreamInfoLength = (data[8] << 8) | data[9];
  long offset = 10;
  long endOfDescriptors = offset + programStreamInfoLength;
  if (endOfDescriptors + 6 > dataLength)
  {
    LogDebug(L"muxer: pin %hhu program stream map is invalid, data length = %ld, program stream info length = %hu",
              info.PinId, dataLength, programStreamInfoLength);
    return E_NOT_SUFFICIENT_BUFFER;
  }

  while (offset + 1 < endOfDescriptors)
  {
    unsigned char tag = data[offset++];
    unsigned char length = data[offset++];
    offset += length;
    LogDebug(L"  program stream map descriptor, tag = 0x%hhx, length = %hhu",
              tag, length);
  }

  if (offset + 6 > dataLength)
  {
    LogDebug(L"muxer: pin %hhu program stream map is invalid, offset = %ld, data length = %ld, program stream info length = %hu",
              info.PinId, offset, dataLength, programStreamInfoLength);
    return E_NOT_SUFFICIENT_BUFFER;
  }

  unsigned short elementaryStreamMapLength = (data[offset] << 8) | data[offset + 1];
  offset += 2;
  long endOfStreams = offset + elementaryStreamMapLength;

  if (endOfStreams + 4 > dataLength)
  {
    LogDebug(L"muxer: pin %hhu program stream map is invalid, offset = %ld, data length = %ld, program stream info length = %hu, elementary stream map length = %hu",
              info.PinId, offset, dataLength, programStreamInfoLength,
              elementaryStreamMapLength);
    return E_NOT_SUFFICIENT_BUFFER;
  }

  while (offset + 3 < endOfStreams)
  {
    unsigned char streamType = data[offset++];
    unsigned char elementaryStreamId = data[offset++];
    LogDebug(L"  elementary stream, stream ID = 0x%hhx, type = 0x%hhx",
              elementaryStreamId, streamType);
    unsigned short elementaryStreamInfoLength = (data[offset] << 8) | data[offset + 1];
    offset += 2;
    endOfDescriptors = offset + elementaryStreamInfoLength;

    if (endOfDescriptors > endOfStreams)
    {
      LogDebug(L"muxer: pin %hhu program stream map is invalid, offset = %ld, data length = %ld, program stream info length = %hu, elementary stream map length = %hu, elementary stream ID = %hhu, elementary stream info length = %hu",
                info.PinId, offset, dataLength, programStreamInfoLength,
                elementaryStreamMapLength, elementaryStreamId,
                elementaryStreamInfoLength);
      return E_NOT_SUFFICIENT_BUFFER;
    }

    while (offset + 1 < endOfDescriptors)
    {
      unsigned char tag = data[offset++];
      unsigned char length = data[offset++];
      offset += length;
      LogDebug(L"    elementary stream descriptor, tag = 0x%hhx, length = %hhu",
                tag, length);
    }
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadVideoStreamInfo(unsigned char* data,
                                      long dataLength,
                                      StreamInfo& info)
{
  LogDebug(L"muxer: pin %hhu video stream", info.PinId);
  LogDebug(L"  sample size  = %ld bytes", dataLength);
  LogDebug(L"  PID          = %hu", info.Pid);
  LogDebug(L"  stream ID    = 0x%hhx", info.StreamId);
  if (dataLength < 12 || data[0] != 0 || data[1] != 0 || data[2] != 1)
  {
    LogDebug(L"muxer: pin %hhu video stream first sample not frame-aligned or unexpected format",
              info.PinId);
    return VFW_E_BADALIGN;
  }

  if (data[3] != 0xb3)  // sequence header
  {
    info.StreamType = STREAM_TYPE_UNKNOWN;
    return S_OK;
  }

  unsigned short horizontalResolution = (data[4] << 4) | (data[5] >> 4);
  unsigned short verticalResolution = ((data[5] & 0xf) << 8) | data[6];
  double aspectRatio = VIDEO_ASPECT_RATIOS[data[7] >> 4];
  double frameRate = VIDEO_FRAME_RATES[data[7] & 0xf];
  unsigned long long bitRate = (data[8] << 10) | (data[9] << 2) | (data[10] >> 6);
  bool isVariableBitRate = bitRate == (unsigned long long)0x3ffff;

  //bool markerBit = (data[10] & 0x20) != 0;
  //unsigned short vbvBufferSizeValue = ((data[10] & 0x1f) << 5) | (data[11] >> 3);
  //bool constrainedParametersFlag = (data[11] & 0x4) != 0;
  bool loadIntraQuantiserMatrix = (data[11] & 0x2) != 0;

  bool isMpeg2 = false;
  unsigned char profile = 0;
  unsigned char level = 0;
  unsigned short pointer = 11;
  if (loadIntraQuantiserMatrix)
  {
    pointer += 64;
  }
  if (pointer < dataLength)
  {
    bool loadNonIntraQuantiserMatrix = (data[pointer++] & 0x1) != 0;
    if (loadNonIntraQuantiserMatrix)
    {
      pointer += 64;
    }

    if (
      pointer + 3 < dataLength &&
      data[pointer] == 0 &&
      data[pointer + 1] == 0 &&
      data[pointer + 2] == 1 &&
      data[pointer + 3] == 0xb5   // extension start code
    )
    {
      isMpeg2 = true;
      pointer += 4;
      if (pointer + 3 < dataLength && (data[pointer] >> 4) == 1)  // sequence extension
      {
        profile = data[pointer++] & 0x7;
        level = data[pointer] >> 4;
        horizontalResolution += ((((data[pointer] & 1) << 1) | (data[pointer + 1] >> 7)) << 12);
        pointer++;
        verticalResolution += ((data[pointer] & 0x60) << 7);
        bitRate += ((unsigned long long)(((data[pointer] & 0x1f) << 8) | (data[19] & 0xfe)) << 17);
      }
    }
  }

  bitRate *= 400;
  LogDebug(L"  is MPEG 2    = %d", isMpeg2);
  LogDebug(L"  resolution   = %hux%hu",
            horizontalResolution, verticalResolution);
  LogDebug(L"  aspect ratio = %lf", aspectRatio);
  LogDebug(L"  frame rate   = %lf f/s", frameRate);
  if (isVariableBitRate)
  {
    LogDebug(L"  bit rate     = [variable]");
  }
  else
  {
    LogDebug(L"  bit rate     = %llu b/s", bitRate);
  }
  LogDebug(L"  profile      = %s",
            profile == 5 ? L"simple" : (
            profile == 4 ? L"main" : (
            profile == 3 ? L"SNR scalable" : (
            profile == 2 ? L"spacially scalable" : L"high"))));
  LogDebug(L"  level        = %s",
            level == 0x1010 ? L"low" : (
            level == 0x1000 ? L"main" : (
            level == 0x0110 ? L"high 1440" : L"high")));

  info.StreamType = isMpeg2 ? STREAM_TYPE_VIDEO_MPEG2 : STREAM_TYPE_VIDEO_MPEG1;
  return S_OK;
}

HRESULT CTsMuxer::ReadAudioStreamInfo(unsigned char* data,
                                      long dataLength,
                                      StreamInfo& info)
{
  LogDebug(L"muxer: pin %hhu audio stream", info.PinId);
  LogDebug(L"  sample size        = %ld bytes", dataLength);
  LogDebug(L"  PID                = %hu", info.Pid);
  LogDebug(L"  stream ID          = 0x%hhx", info.StreamId);
  if (dataLength < 4 || data[0] != 0xff || (data[1] & 0xf0) != 0xf0)
  {
    LogDebug(L"muxer: pin %hhu audio stream first sample not frame-aligned or unexpected format",
              info.PinId);
    return VFW_E_BADALIGN;
  }

  bool isMpeg2 = (data[1] & 0x08) == 0;
  unsigned char layer = 4 - ((data[1] >> 1) & 0x3);
  unsigned short bitRate = AUDIO_BIT_RATES[isMpeg2][layer][data[2] >> 4];
  long samplingFrequency = AUDIO_SAMPLE_RATES[isMpeg2][(data[2] >> 2) & 0x3];
  unsigned char mode = data[3] >> 6;
  LogDebug(L"  is MPEG 2          = %d", isMpeg2);
  LogDebug(L"  layer              = %hhu", layer);
  LogDebug(L"  bit rate           = %hu kb/s", bitRate);
  LogDebug(L"  sampling frequency = %ld Hz", samplingFrequency);
  LogDebug(L"  mode               = %s",
            mode == 0 ? L"stereo" : (
            mode == 1 ? L"joint" : (
            mode == 2 ? L"dual channel" : L"mono")));

  info.StreamType = isMpeg2 ? STREAM_TYPE_AUDIO_MPEG2_PART3 : STREAM_TYPE_AUDIO_MPEG1;
  return S_OK;
}

void CTsMuxer::UpdatePat()
{
  // Update the PMT PID for our program.
  unsigned char* pointer = &m_patPacket[15];
  *pointer++ = 0xe0 | (m_pmtPid >> 8);
  *pointer++ = m_pmtPid & 0xff;
  unsigned long crc = CalculatCrc32(&m_patPacket[5], 12);
  *pointer++ = crc >> 24;
  *pointer++ = (crc >> 16) & 0xff;
  *pointer++ = (crc >> 8) & 0xff;
  *pointer++ = crc & 0xff;

  m_packetCounter = 0;  // trigger PAT to be delivered before the next content
}

HRESULT CTsMuxer::UpdatePmt()
{
  unsigned short sectionLength = 13;   // size of fixed-length parts of the section (including CRC)

  // Append stream info to the PMT and decide which PID should be the PCR PID.
  unsigned char activeStreamCount = 0;
  m_pcrPid = PID_NOT_SET;
  bool pcrPidIsVideo = false;
  unsigned char* pointer = &m_pmtPacket[17];
  map<unsigned long, StreamInfo*>::const_iterator it = m_streamInfo.begin();
  for ( ; it != m_streamInfo.end(); it++)
  {
    StreamInfo* info = it->second;
    if (
      info->IsIgnored ||
      !info->IsCompatible ||
      info->PrevReceiveTime == NOT_RECEIVING ||
      CTimeUtils::ElapsedMillis(info->PrevReceiveTime) >= STREAM_IDLE_TIMEOUT
    )
    {
      continue;
    }
    activeStreamCount++;

    if (
      info->StreamType == STREAM_TYPE_TELETEXT ||
      info->StreamType == STREAM_TYPE_VPS ||
      info->StreamType == STREAM_TYPE_WSS
    )
    {
      *pointer++ = STREAM_TYPE_PES_PRIVATE_DATA;
    }
    else
    {
      *pointer++ = info->StreamType;
    }
    *pointer++ = 0xe0 | (info->Pid >> 8);
    *pointer++ = info->Pid & 0xff;
    if (info->PmtDescriptorLength > 0 && info->PmtDescriptorBytes != NULL)
    {
      *pointer++ = 0xf0 | (info->PmtDescriptorLength >> 8);
      *pointer++ = info->PmtDescriptorLength & 0xff;
      memcpy(pointer, info->PmtDescriptorBytes, info->PmtDescriptorLength);
      pointer += info->PmtDescriptorLength;
      sectionLength += info->PmtDescriptorLength;
    }
    else
    {
      *pointer++ = 0xf0;
      *pointer++ = 0;
    }
    sectionLength += 5;

    // Update the PCR PID. We select the first non-VBI stream by default
    // but we prefer a video stream.
    bool isVideoPid = CPidTable::IsVideoStream(info->StreamType);
    if (
      (!pcrPidIsVideo && isVideoPid) ||
      (
        m_pcrPid == PID_NOT_SET &&
        info->StreamType != STREAM_TYPE_TELETEXT &&
        info->StreamType != STREAM_TYPE_VPS &&
        info->StreamType != STREAM_TYPE_WSS
      )
    )
    {
      m_pcrPid = info->Pid;
      pcrPidIsVideo = isVideoPid;
    }
  }

  m_pmtPacket[1] = 0x40 | (m_pmtPid >> 8);  // payload start
  m_pmtPacket[2] = m_pmtPid & 0xff;

  m_pmtPacket[6] = 0xb0 | (sectionLength >> 8);
  m_pmtPacket[7] = sectionLength & 0xff;

  m_pmtVersion = (m_pmtVersion + 1) & 0x1f;
  m_pmtPacket[10] = 0xc1 | (m_pmtVersion << 1);

  m_pmtPacket[13] = 0xe0 | (m_pcrPid >> 8);
  m_pmtPacket[14] = m_pcrPid & 0xff;

  unsigned long crc = CalculatCrc32(&m_pmtPacket[5], sectionLength - 4 + 3);  // - 4 for CRC + 3 for table ID and section length
  *pointer++ = crc >> 24;
  *pointer++ = (crc >> 16) & 0xff;
  *pointer++ = (crc >> 8) & 0xff;
  *pointer++ = crc & 0xff;
  unsigned short stuffingLength = TS_PACKET_LEN - TS_HEADER_LENGTH - 1 - 3 - sectionLength;   // - 1 for pointer byte - 3 for table ID and section length
  if (stuffingLength < 0)
  {
    LogDebug(L"muxer: PMT requires more than one packet, not supported");
    return E_FAIL;
  }
  memset(pointer, 0xff, stuffingLength);

  // Check if the service type has changed, and update the SDT that we're
  // delivering if it has. Be careful though: we don't want to trigger SDT
  // delivery from here (hence the SDT version check).
  unsigned char oldServiceType = m_serviceType;
  m_serviceType = pcrPidIsVideo ? SERVICE_TYPE_TELEVISION : SERVICE_TYPE_RADIO;
  if (oldServiceType != m_serviceType && m_sdtVersion != VERSION_NOT_SET)
  {
    UpdateSdt();
  }

  LogDebug(L"muxer: updated PMT, PID = %hu, active stream count = %hhu, version = %hhu, service type = %hhu",
            m_pmtPid, activeStreamCount, m_pmtVersion, m_serviceType);
  m_packetCounter = 0;  // trigger PMT to be delivered before the next content
  return S_OK;
}

void CTsMuxer::ResetSdtInfo()
{
  m_sdtVersion = VERSION_NOT_SET;
  m_isCniName = false;
  memset(m_serviceName, 0, SERVICE_NAME_LENGTH);
  m_sdtResetTime = clock();
}

HRESULT CTsMuxer::UpdateSdt()
{
  unsigned char serviceNameLength = strlen(m_serviceName);
  unsigned char serviceDescriptorLength = 3 + serviceNameLength;
  unsigned char descriptorLoopLength = 2 + serviceDescriptorLength;
  unsigned char sectionLength = 17 + descriptorLoopLength;

  m_sdtPacket[7] = sectionLength;

  m_sdtVersion = (m_sdtVersion + 1) & 0x1f;
  m_sdtPacket[10] = 0xc1 | (m_sdtVersion << 1);

  unsigned char* pointer = &m_sdtPacket[20];
  *pointer++ = descriptorLoopLength;
  *pointer++ = DESCRIPTOR_DVB_SERVICE;
  *pointer++ = serviceDescriptorLength;
  *pointer++ = m_serviceType;
  *pointer++ = 0;     // provider name length
  *pointer++ = serviceNameLength;
  memcpy(pointer, m_serviceName, serviceNameLength);
  pointer += serviceNameLength;

  unsigned long crc = CalculatCrc32(&m_sdtPacket[5], sectionLength - 4 + 3);  // - 4 for CRC + 3 for table ID and section length
  *pointer++ = crc >> 24;
  *pointer++ = (crc >> 16) & 0xff;
  *pointer++ = (crc >> 8) & 0xff;
  *pointer++ = crc & 0xff;

  unsigned short stuffingLength = TS_PACKET_LEN - TS_HEADER_LENGTH - 1 - 3 - sectionLength;   // - 1 for pointer byte - 3 for table ID and section length
  if (stuffingLength < 0)
  {
    LogDebug(L"muxer: SDT requires more than one packet, not supported");
    return E_FAIL;
  }
  memset(pointer, 0xff, stuffingLength);
  m_packetCounter = 0;  // trigger SDT to be delivered before the next content

  if (serviceNameLength > 0)
  {
    LogDebug(L"muxer: updated SDT, version = %hhu, service type = %hhu, service name = %S",
              m_sdtVersion, m_serviceType, m_serviceName);
  }
  else
  {
    LogDebug(L"muxer: updated SDT, version = %hhu, service type = %hhu, service name = [not set]",
              m_sdtVersion, m_serviceType);
  }
  return S_OK;
}

HRESULT CTsMuxer::WrapVbiData(const StreamInfo& info,
                              unsigned char* inputData,
                              long inputDataLength,
                              long long systemClockReference,
                              unsigned char** outputData,
                              long& outputDataLength)
{
  outputDataLength = 0;
  *outputData = NULL;

  // According to EN 301 775 section 4.1 we must have a PTS for VBI packets, therefore we must have an SCR.
  if (inputDataLength == 0 || systemClockReference == TIME_NOT_SET)
  {
    return S_OK;
  }

  if (inputData == NULL)
  {
    return E_INVALIDARG;
  }

  unsigned char dataUnitId;
  unsigned char lineLength;
  unsigned char fieldParity;
  unsigned char lineOffset;
  if (info.StreamType == STREAM_TYPE_TELETEXT)
  {
    dataUnitId = TELETEXT_DATA_UNIT_ID;
    lineLength = TELETEXT_LINE_LENGTH;
    fieldParity = TELETEXT_FIELD_PARITY;
    lineOffset = TELETEXT_LINE_OFFSET;
  }
  else if (info.StreamType == STREAM_TYPE_VPS)
  {
    dataUnitId = VPS_DATA_UNIT_ID;
    lineLength = VPS_LINE_LENGTH;
    fieldParity = VPS_FIELD_PARITY;
    lineOffset = VPS_LINE_OFFSET;
  }
  else if (info.StreamType == STREAM_TYPE_WSS)
  {
    dataUnitId = WSS_DATA_UNIT_ID;
    lineLength = WSS_LINE_LENGTH;
    fieldParity = WSS_FIELD_PARITY;
    lineOffset = WSS_LINE_OFFSET;
  }
  else
  {
    LogDebug(L"muxer: pin %hhu VBI stream type 0x%hhx is not supported",
              info.PinId, info.StreamType);
    return E_UNEXPECTED;
  }

  if (inputDataLength % lineLength != 0)
  {
    LogDebug(L"muxer: pin %hhu VBI stream type 0x%hhx with size %ld contains partial lines",
              info.PinId, info.StreamType, inputDataLength);
    return E_UNEXPECTED;
  }

  // Side task.
  if (info.StreamType == STREAM_TYPE_TELETEXT)
  {
    ReadChannelNameFromVbiTeletextData(inputData, inputDataLength);
  }
  else if (info.StreamType == STREAM_TYPE_VPS)
  {
    ReadChannelNameFromVbiVpsData(inputData, inputDataLength);
  }

  // Convert the raw VBI data into ES data. Refer to EN 301 775, EN 300 472,
  // EN 300 231 and EN 300 294.
  // The number of ES bytes we expect to generate from the input, assuming no
  // empty lines thrown away.
  long esDataLength = VBI_PES_LINE_LENGTH * (inputDataLength / lineLength);

  // Calculate the maximum buffer space required to wrap the input data. This
  // is tricky because VBI ES data must be organised and packed in such a way
  // that it finishes at the end of a TS packet. The PES header + data
  // identifier (fixed length) will give us 46 bytes on top of the 46 bytes
  // from each VBI line (also fixed length, due to the choice of data
  // identifier). If necessary we will add fake/empty VBI lines at the end to
  // make up any difference.
  long esBufferLength = esDataLength;
  while ((esBufferLength + 46) % (TS_PACKET_LEN - TS_HEADER_LENGTH) != 0) // + 46 for the PES header + data identifier
  {
    esBufferLength += VBI_PES_LINE_LENGTH;   // + fake line
  }
  esBufferLength++;  // data identifier

  // The PES header data length is fixed by EN 301 775 (and EN 300 472 before
  // that). We will add PTS, but the rest of the space has to be made up with
  // stuffing, which we add in this function as if it is part of the ES data.
  unsigned short stuffingLength = VBI_PES_HEADER_DATA_LENGTH - PTS_LENGTH;
  esBufferLength += stuffingLength;

  unsigned char* esBuffer = new unsigned char[esBufferLength];
  if (esBuffer == NULL)
  {
    LogDebug(L"muxer: failed to allocate %ld byte(s) for a VBI elementary stream buffer",
              esBufferLength);
    return E_OUTOFMEMORY;
  }

  unsigned char* inputPointer = inputData;
  unsigned char* esPointer = &esBuffer[stuffingLength + 1];   // + 1 for data identifier
  while (inputDataLength >= lineLength)
  {
    // If this line has real content...
    if (*inputPointer != 0 || *(inputPointer + 1) != 0)
    {
      *esPointer++ = dataUnitId;
      *esPointer++ = VBI_DATA_UNIT_LENGTH;
      *esPointer++ = (0xc0 | (fieldParity << 5) | lineOffset);
      if (info.StreamType == STREAM_TYPE_TELETEXT)
      {
        *inputPointer = 0xe4;   // overwrite the framing code
      }
      for (unsigned char i = 0; i < lineLength; i++)
      {
        *esPointer++ = REVERSE_BITS[*inputPointer++];   // Reverse because the TS packet bits must be in transmission order.
      }
      for (unsigned char i = lineLength; i < VBI_DATA_UNIT_LENGTH - 1; i++)
      {
        *esPointer++ = 0xff;    // stuffing
      }
    }
    else
    {
      inputPointer += lineLength;
      esDataLength -= VBI_PES_LINE_LENGTH;
    }
    inputDataLength -= lineLength;
  }

  // If we have some real data (ie. not all zeroes)...
  HRESULT hr = S_OK;
  if (esDataLength > 0)
  {
    memset(esBuffer, 0xff, stuffingLength); 
    esBuffer[stuffingLength] = DATA_IDENTIFIER_EBU_DATA;

    // Stuffing lines.
    while ((esDataLength + 46) % (TS_PACKET_LEN - TS_HEADER_LENGTH) != 0)
    {
      memset(esPointer, 0xff, VBI_PES_LINE_LENGTH);
      *(esPointer + 1) = VBI_DATA_UNIT_LENGTH;
      esDataLength += VBI_PES_LINE_LENGTH;
    }
    hr = WrapElementaryStreamData(info, esBuffer, stuffingLength + 1 + esDataLength,
                                  systemClockReference, outputData, outputDataLength);
    if (SUCCEEDED(hr) && outputDataLength > 0)
    {
      // Overwrite the PES header length to include the stuffing.
      (*outputData)[8] = VBI_PES_HEADER_DATA_LENGTH;
    }
  }
  delete[] esBuffer;
  return hr;
}

HRESULT CTsMuxer::ReadChannelNameFromVbiTeletextData(unsigned char* inputData, long inputDataLength)
{
  if (m_isCniName)
  {
    return S_OK;
  }

  unsigned char* input = inputData;
  HRESULT hr = S_OK;
  while (inputDataLength >= TELETEXT_LINE_LENGTH)
  {
    inputDataLength -= TELETEXT_LINE_LENGTH;
    unsigned char* inputPointer = input;
    input += TELETEXT_LINE_LENGTH;

    // If this line has real content...
    if (
      *inputPointer == 0 && *(inputPointer + 1) == 0 && *(inputPointer + 2) == 0 &&
      *(inputPointer + 3) == 0 && *(inputPointer + 4) == 0
    )
    {
      continue;
    }

    // Search for the channel name for the SDT. Refer to EN 300 706, section
    // 9.8 - Broadcast Service Data Packets.
    unsigned char magazineAndPacketAddress = 0xff;
    if (!UnhamWord(*(inputPointer + 1), *(inputPointer + 2), magazineAndPacketAddress))
    {
      continue;
    }

    unsigned char magazineNumber = magazineAndPacketAddress & 0x07;
    unsigned char packetNumber = magazineAndPacketAddress >> 3;
    if (magazineNumber != 0 || packetNumber != 30)  // magazine 0 is referred to as magazine 8
    {
      continue;
    }

    unsigned char designationCode = 0xff;
    if (!UnhamByte(*(inputPointer + 3), designationCode))
    {
      continue;
    }

    designationCode = (designationCode >> 1);
    if (designationCode == 7 && strlen(m_serviceName) > 0)
    {
      // If we get to here we already have a name from a format 8 (German)
      // line. Assume the name won't be different => no need to continue.
      continue;
    }
    LogDebug(L"muxer: found teletext magazine 8 packet 30, designation code = %hhu",
              designationCode);
    if (designationCode == 7)   // German proprietary format, not preferred
    {
      // 1 byte         framing code - actually seems to be a random value in many cases
      // 2 bytes        magazine and packet numbers, Hamming 8/4 protected
      // 1 byte         designation code and multiplexed flag
      // 6 bytes        unknown
      // 3 bytes        unknown, looks like a hex number encoded in ASCII (eg. FAC) which seems to increase sequentially over time
      // 1 byte         value 0x83
      // 7 to 9 bytes   teletext service name (eg. RTL II, RTL NITRO, ZDFtext), seems to often have "text" on the end
      // 1 byte         value 0x02 or 0x07
      // 8 to 11 bytes  date encoded in ASCII (eg. So 25 Mai, Sa 24.5.)
      // 1 byte         value 0x83
      // 8 or 9 bytes   time encoded in ASCII (eg.  05:59:18, 06:01:50)
      // 1 byte         value 0x64
      inputPointer = inputPointer + 14;
      unsigned char j = 0;
      for (unsigned char i = 0; i < 29; i++)
      {
        char c = *inputPointer++;
        if ((c & 0x7f) < 0x20)
        {
          // End of name, NULL terminate.
          m_serviceName[j] = NULL;
          if (j > 0)
          {
            hr = UpdateSdt();
          }
          break;
        }
        if (PARITY[c] == 0)
        {
          // Error detected => bail and reset name.
          LogDebug(L"muxer: parity error, reset channel name");
          m_serviceName[0] = NULL;
          break;
        }
        if (c != 0x20 || j > 0) // Ignore leadings space characters.
        {
          m_serviceName[j++] = c & 0x7f;
          if (i == 28)
          {
            // No more bytes left.
            m_serviceName[j] = NULL;
            hr = UpdateSdt();
            break;
          }
        }
      }
    }
    else if (designationCode == 0)    // format 1 packet/line (standard - EN 300 706 section 9.8.1), preferred
    {
      // The network identifier field is transmitted MSB first. We
      // have to undo the reversing process performed by the VBI/WST
      // filter.
      unsigned short networkId = (REVERSE_BITS[*(inputPointer + 10)] << 8) | REVERSE_BITS[*(inputPointer + 11)];
      string* name = NULL;
      if (m_cniRegister.GetM8P30Format1NetworkName(networkId, name))
      {
        strncpy(m_serviceName, name->c_str(), sizeof(m_serviceName));
        m_serviceName[sizeof(m_serviceName) - 1] = NULL;
        LogDebug(L"muxer: network ID = %hu", networkId);
        m_isCniName = true;
        return UpdateSdt();
      }

      LogDebug(L"muxer: network ID = %hu, name not registered", networkId);
    }
    else if (designationCode == 1)    // format 2 packet/line (PDC - EN 300 706 section 9.8.2 and EN 300 231 section 8.2.1), preferred
    {
      // The bit manipulation here is really tricky!
      // The bits reach us in reversed transmission order. The Hamming table is
      // designed to work on them in that order, so we unham first.
      // Normally teletext fields are transmitted LSB first, but the CNI field
      // is the opposite. So we have to reverse the unhammed bits to get them
      // back to transmission order, then finally shift them to construct the
      // final identifiers.
      unsigned char byte15;
      unsigned char byte16;
      unsigned char byte21;
      unsigned char byte22;
      unsigned char byte23;
      if (
        UnhamByte(*(inputPointer + 12), byte15) &&   // EN 300 231 byte 15
        UnhamByte(*(inputPointer + 13), byte16) &&
        UnhamByte(*(inputPointer + 18), byte21) &&
        UnhamByte(*(inputPointer + 19), byte22) &&
        UnhamByte(*(inputPointer + 20), byte23)
      )
      {
        unsigned char countryId = REVERSE_BITS[byte15] | ((REVERSE_BITS[byte21] & 0x30) >> 2) | ((REVERSE_BITS[byte22] & 0xc0) >> 6);
        unsigned char networkId = (REVERSE_BITS[byte16] & 0xc0) | (REVERSE_BITS[byte22] & 0x30) | (REVERSE_BITS[byte23] >> 4);
        string* name = NULL;
        if (m_cniRegister.GetM8P30Format2NetworkName(countryId, networkId, name))
        {
          strncpy(m_serviceName, name->c_str(), sizeof(m_serviceName));
          m_serviceName[sizeof(m_serviceName) - 1] = NULL;
          LogDebug(L"muxer: country ID = %hhu, network ID = %hhu",
                    countryId, networkId);
          m_isCniName = true;
          return UpdateSdt();
        }

        LogDebug(L"muxer: country ID = %hhu, network ID = %hhu, name not registered",
                  countryId, networkId);
      }
    }
  }
  return hr;
}

HRESULT CTsMuxer::ReadChannelNameFromVbiVpsData(unsigned char* inputData, long inputDataLength)
{
  if (m_isCniName)
  {
    return S_OK;
  }

  unsigned char* input = inputData;
  HRESULT hr = S_OK;
  while (inputDataLength >= VPS_LINE_LENGTH)
  {
    inputDataLength -= VPS_LINE_LENGTH;
    unsigned char* inputPointer = input;
    input += VPS_LINE_LENGTH;

    // If this line has real content...
    if (
      *inputPointer == 0 && *(inputPointer + 1) == 0 && *(inputPointer + 2) == 0 &&
      *(inputPointer + 3) == 0 && *(inputPointer + 4) == 0
    )
    {
      continue;
    }

    LogDebug(L"muxer: found VPS content");

    // Refer to EN 300 231 section 8.2.2.
    // The bit manipulation here is a bit tricky!
    // The bits reach us in reversed transmission order. Normally teletext
    // fields are transmitted LSB first, but the CNI field is the opposite. So
    // we have to reverse the input bits to get them back to transmission
    // order, then shift them to construct the final identifiers.
    unsigned char byte11 = REVERSE_BITS[*(inputPointer + 12)];
    unsigned char byte13 = REVERSE_BITS[*(inputPointer + 12)];
    unsigned char byte14 = REVERSE_BITS[*(inputPointer + 12)];
    unsigned char countryId = ((byte13 & 0x3) << 2) | ((byte14 & 0xc0) >> 6);
    unsigned char networkId = (byte11 & 0xc0) | (byte14 & 0x3f);
    string* name = NULL;
    if (m_cniRegister.GetVpsNetworkName(countryId, networkId, name))
    {
      strncpy(m_serviceName, name->c_str(), sizeof(m_serviceName));
      m_serviceName[sizeof(m_serviceName) - 1] = NULL;
      LogDebug(L"muxer: country ID = %hhu, network ID = %hhu",
                countryId, networkId);
      m_isCniName = true;
      return UpdateSdt();
    }

    LogDebug(L"muxer: country ID = %hhu, network ID = %hhu, name not registered",
              countryId, networkId);
  }
  return hr;
}

HRESULT CTsMuxer::WrapElementaryStreamData(const StreamInfo& info,
                                            unsigned char* inputData,
                                            long inputDataLength,
                                            long long systemClockReference,
                                            unsigned char** outputData,
                                            long& outputDataLength)
{
  outputDataLength = 0;
  *outputData = NULL;
  if (inputDataLength == 0)
  {
    return S_OK;
  }
  if (inputData == NULL)
  {
    return E_INVALIDARG;
  }

  long packetLength = inputDataLength + 3;  // + 3 for flags and header length
  unsigned char alignmentFlag = 0;          // We assume that the stream is frame aligned if the sample time is set.
  if (systemClockReference != TIME_NOT_SET)
  {
    packetLength += PTS_LENGTH;
    alignmentFlag = 0x04;   // aligned
  }
  outputDataLength = packetLength + 6;      // + 6 for header overhead (0 0 1 etc.)
  *outputData = new unsigned char[outputDataLength];
  if (*outputData == NULL)
  {
    LogDebug(L"muxer: failed to allocate %ld byte(s) for an elementary stream output buffer",
              outputDataLength);
    return E_OUTOFMEMORY;
  }
  if (packetLength >= 0x10000)
  {
    if (CPidTable::IsVideoStream(info.StreamType))
    {
      packetLength = 0;   // length not specified, only allowed for video PES packets carried in a TS
    }
    else
    {
      LogDebug(L"muxer: pin %hhu extended length %ld cannot be used for stream type 0xhhx",
                info.PinId, packetLength, info.StreamType);
      return E_UNEXPECTED;
    }
  }
  unsigned char* pointer = *outputData;
  *pointer++ = 0;
  *pointer++ = 0;
  *pointer++ = 1;
  *pointer++ = info.StreamId;
  *pointer++ = (unsigned char)(packetLength >> 8);
  *pointer++ = packetLength & 0xff;
  *pointer++ = alignmentFlag | 0x81;    // not scrambled, not high priority, not copyright, not copy
  if (systemClockReference == TIME_NOT_SET)
  {
    *pointer++ = 0;           // no extra info
    *pointer++ = 0;           // header length
  }
  else
  {
    *pointer++ = 0x80;        // PTS present, nothing else
    *pointer++ = PTS_LENGTH;  // header data length

    // Convert the system clock reference (27 MHz) to a presentation time stamp
    // (90 kHz).
    long long pts = (systemClockReference / 300) & MAX_PCR_BASE;
    *pointer++ = 0x20 | ((pts >> 29) & 0x0e) | 1;
    *pointer++ = (pts >> 22) & 0xff;
    *pointer++ = ((pts >> 14) & 0xfe) | 1;
    *pointer++ = (pts >> 7) & 0xff;
    *pointer++ = ((pts << 1) & 0xfe) | 1;
  }
  memcpy(pointer, inputData, inputDataLength);
  return S_OK;
}

HRESULT CTsMuxer::WrapPacketisedElementaryStreamData(StreamInfo& info,
                                                      unsigned char* inputData,
                                                      long inputDataLength,
                                                      long long systemClockReference,
                                                      unsigned short pcrPid,
                                                      unsigned char** outputData,
                                                      long& outputDataLength)
{
  outputDataLength = 0;
  *outputData = NULL;
  if (inputDataLength == 0)
  {
    return S_OK;
  }
  if (inputData == NULL)
  {
    return E_INVALIDARG;
  }

  bool writePcr = false;
  long outputBufferSize = inputDataLength;
  if (systemClockReference != TIME_NOT_SET && info.Pid == pcrPid)
  {
    writePcr = true;
    outputBufferSize += 2 + PCR_LENGTH;   // + 2 for adaptation field length and flags
  }

  // Calculate a rough output buffer size. This must be large enough to hold
  // the largest number of packets that we would generate from the input.
  long packetCount = (outputBufferSize / (TS_PACKET_LEN - TS_HEADER_LENGTH)) + 1;   // + 1 for safety
  outputBufferSize = packetCount * TS_PACKET_LEN;
  *outputData = new unsigned char[outputBufferSize];
  if (*outputData == NULL)
  {
    LogDebug(L"muxer: failed to allocate %ld byte(s) for a packetised elementary stream output buffer",
              outputBufferSize);
    return E_OUTOFMEMORY;
  }
  unsigned char* inputPointer = inputData;
  unsigned char* outputPointer = *outputData;
  outputDataLength = 0;
  unsigned char firstPacketFlags = 0x40;   // payload start

  // Generate packets until we run out of data. The first packet may have a
  // PCR; the last packet may be padded.
  while (inputDataLength > 0)
  {
    *outputPointer++ = TS_PACKET_SYNC;
    *outputPointer++ = firstPacketFlags | (info.Pid >> 8);
    firstPacketFlags = 0;
    *outputPointer++ = info.Pid & 0xff;

    unsigned char dataByteCount = TS_PACKET_LEN - TS_HEADER_LENGTH;
    unsigned char stuffingByteCount = 0;

    if (writePcr || inputDataLength < (TS_PACKET_LEN - TS_HEADER_LENGTH))
    {
      *outputPointer++ = 0x30 | info.ContinuityCounter;
      if (writePcr)
      {
        unsigned char maxDataByteCount = TS_PACKET_LEN - TS_HEADER_LENGTH - PCR_LENGTH - 2;    // - 2 for adaptation field length and flags
        stuffingByteCount = (unsigned char)max(maxDataByteCount - inputDataLength, 0);
        dataByteCount = maxDataByteCount - stuffingByteCount;
        *outputPointer++ = 1 + PCR_LENGTH + stuffingByteCount;  // adaptation field length, + 1 for flags
        *outputPointer++ = 0x10;                                // flags: PCR present

        // Convert the system clock reference (27 MHz) to a program clock rate.
        REFERENCE_TIME pcrBase = (systemClockReference / 300) & MAX_PCR_BASE; // 90 kHz
        REFERENCE_TIME pcrExt = systemClockReference & MAX_PCR_EXTENSION;     // 27 MHz
        *outputPointer++ = (pcrBase >> 25) & 0xff;
        *outputPointer++ = (pcrBase >> 17) & 0xff;
        *outputPointer++ = (pcrBase >> 9) & 0xff;
        *outputPointer++ = (pcrBase >> 1) & 0xff;
        *outputPointer++ = (unsigned char)(((pcrBase & 0x01) << 7) | 0x7e | (pcrExt >> 8));
        *outputPointer++ = pcrExt & 0xff;
        writePcr = false;
      }
      else
      {
        if (inputDataLength == TS_PACKET_LEN - TS_HEADER_LENGTH - 1)    // - 1 for adaptation field length
        {
          *outputPointer++ = 0;                                 // adaptation field length
        }
        else
        {
          stuffingByteCount = (unsigned char)(TS_PACKET_LEN - TS_HEADER_LENGTH - 2 - inputDataLength);  // - 2 for adaptation field length and flags
          *outputPointer++ = 1 + stuffingByteCount;             // adaptation field length, + 1 for flags
          *outputPointer++ = 0;                                 // flags: (none)
        }
        dataByteCount = (unsigned char)inputDataLength;
      }

      if (stuffingByteCount > 0)
      {
        memset(outputPointer, 0xff, stuffingByteCount);
        outputPointer += stuffingByteCount;
      }
    }
    else
    {
      *outputPointer++ = 0x10 | info.ContinuityCounter;
    }

    memcpy(outputPointer, inputPointer, dataByteCount);
    outputPointer += dataByteCount;
    inputPointer += dataByteCount;
    inputDataLength -= dataByteCount;

    info.ContinuityCounter = (info.ContinuityCounter + 1) & 0xf;
    outputDataLength += TS_PACKET_LEN;
  }

  return S_OK;
}

HRESULT CTsMuxer::DeliverTransportStreamData(unsigned char* inputData, long inputDataLength)
{
  if (inputDataLength == 0)
  {
    return S_OK;
  }
  if (inputData == NULL)
  {
    return E_INVALIDARG;
  }

  HRESULT hr = S_OK;
  while (inputDataLength > 0)
  {
    // Inject a PAT, PMT and SDT once in every TS_BUFFER_FLUSH_RATE_* packets.
    if (m_packetCounter == 0)
    {
      m_patContinuityCounter = (m_patContinuityCounter + 1) & 0xf;
      m_patPacket[3] &= 0xf0;
      m_patPacket[3] |= m_patContinuityCounter;
      hr = m_filter->Deliver(&m_patPacket[0], TS_PACKET_LEN);
      if (FAILED(hr))
      {
        return hr;
      }

      m_pmtContinuityCounter = (m_pmtContinuityCounter + 1) & 0xf;
      m_pmtPacket[3] &= 0xf0;
      m_pmtPacket[3] |= m_pmtContinuityCounter;
      hr = m_filter->Deliver(&m_pmtPacket[0], TS_PACKET_LEN);
      if (FAILED(hr))
      {
        break;
      }

      // Only inject the SDT when we've had the chance to set it.
      if (m_sdtVersion != VERSION_NOT_SET)
      {
        m_sdtContinuityCounter = (m_sdtContinuityCounter + 1) & 0xf;
        m_sdtPacket[3] &= 0xf0;
        m_sdtPacket[3] |= m_sdtContinuityCounter;
        hr = m_filter->Deliver(&m_sdtPacket[0], TS_PACKET_LEN);
        if (FAILED(hr))
        {
          break;
        }
      }
      m_packetCounter = m_isVideoActive ? TS_BUFFER_FLUSH_RATE_VIDEO : TS_BUFFER_FLUSH_RATE_AUDIO;
    }

    // At most we deliver TS_BUFFER_FLUSH_RATE_VIDEO packets per sample.
    long bytesToDeliver = m_packetCounter * TS_PACKET_LEN;        // bytes that should be delivered before another round of service information (PAT etc.)
    long bytesCanDeliver = min(inputDataLength, bytesToDeliver);  // bytes that we have available
    unsigned short packetsToDeliver = (unsigned short)(bytesCanDeliver / TS_PACKET_LEN);
    hr = m_filter->Deliver(inputData, bytesCanDeliver);
    if (FAILED(hr))
    {
      return hr;
    }
    inputData += bytesCanDeliver;
    inputDataLength -= bytesCanDeliver;
    m_packetCounter -= packetsToDeliver;
  }
  return hr;
}