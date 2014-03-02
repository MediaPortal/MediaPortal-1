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
#include <shlobj.h>
#include <process.h>
#include "..\..\shared\DvbUtil.h"
#include "Hamming.h"

#define STREAM_ID_END_OF_STREAM 0xb9
#define STREAM_ID_PACK 0xba
#define STREAM_ID_SYSTEM_HEADER 0xbb
#define STREAM_ID_PROGRAM_STREAM_MAP 0xbc
#define STREAM_ID_TELETEXT 0xbd   // private stream 1
#define STREAM_ID_PADDING 0xbe
#define STREAM_ID_AUDIO_FIRST 0xc0
#define STREAM_ID_VIDEO_FIRST 0xe0

#define PID_NOT_SET -1
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

#define STREAM_IDLE_TIMEOUT 1000
#define TELETEXT_SERVICE_NAME_TIMEOUT 5000

#define TS_HEADER_LENGTH 4
#define TS_SYNC_BYTE 0x47
#define PTS_LENGTH 5
#define PCR_LENGTH 6

#define VERSION_NOT_SET -1
#define TIME_NOT_SET -1

#define TELETEXT_DATA_IDENTIFIER 0x10     // EBU data
#define TELETEXT_DATA_UNIT_ID 0x02        // EBU teletext non-subtitle data
#define VBI_LINE_LENGTH 43
#define TELETEXT_PES_STUFFING_LENGTH 31

// Table for inverting/reversing bit ordering of teletext/VBI bytes.
const byte REVERSE_BITS[256] =
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

const int AUDIO_BIT_RATES[2][3][15] =
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

const int AUDIO_SAMPLE_RATES[2][3] =
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
static wchar_t currentLogFileName[MAX_PATH];
static WORD currentDay = -1;
static wchar_t logBuffer[2000];

void GetLogFileName(wchar_t* fileName)
{
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  if (currentDay != systemTime.wDay)
  {
    wchar_t folderName[MAX_PATH];
    ::SHGetSpecialFolderPathW(NULL, folderName, CSIDL_COMMON_APPDATA, FALSE);
    swprintf_s(currentLogFileName, L"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\TsMuxer-%04.4d-%02.2d-%02.2d.Log", folderName, systemTime.wYear, systemTime.wMonth, systemTime.wDay);
    currentDay = systemTime.wDay;
  }
  wcscpy(fileName, &currentLogFileName[0]);
}

void LogDebug(const wchar_t* fmt, ...)
{
  va_list ap;
  va_start(ap, fmt);
  vswprintf_s(logBuffer, fmt, ap);
  va_end(ap);

  wchar_t fileName[MAX_PATH];
  GetLogFileName(fileName);
  FILE* file = _wfopen(fileName, L"a+, ccs=UTF-8");
  if (file != NULL)
  {
    SYSTEMTIME systemTime;
    GetLocalTime(&systemTime);
    fwprintf(file, L"%04.4d-%02.2d-%02.2d %02.2d:%02.2d:%02.2d.%03.3d %s\n",
      systemTime.wYear, systemTime.wDay, systemTime.wMonth,
      systemTime.wHour,systemTime.wMinute, systemTime.wSecond, systemTime.wMilliseconds,
      logBuffer);
    fclose(file);

    //::OutputDebugStringW(logBuffer);
    //::OutputDebugStringW(L"\n");
  }
};


//-----------------------------------------------------------------------------
// FILTER CLASS
//-----------------------------------------------------------------------------
CTsMuxerFilter::CTsMuxerFilter(CTsMuxer* tsMuxer, LPUNKNOWN unk, CCritSec* filterLock, CCritSec* receiveLock, HRESULT* hr)
  : CBaseFilter(NAME("MediaPortal TS Muxer"), unk, filterLock, CLSID_TS_MUXER), m_tsMuxer(tsMuxer)
{
  LogDebug(L"CTsMuxerFilter: constructor");
  m_receiveLock = receiveLock;
  m_outputPin = new CTsOutputPin(this, filterLock, hr);
  if (m_outputPin == NULL)
  {
    *hr = E_OUTOFMEMORY;
    return;
  }
  m_streamingMonitorThread = INVALID_HANDLE_VALUE;
  m_streamingMonitorThreadStopEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  if (m_streamingMonitorThreadStopEvent == NULL)
  {
    *hr = GetLastError();
  }

  *hr = AddPin();
  LogDebug(L"CTsMuxerFilter: completed, hr = 0x%x", *hr);
}

CTsMuxerFilter::~CTsMuxerFilter()
{
  LogDebug(L"CTsMuxerFilter: destructor");
  CAutoLock filterLock(m_pLock);
  vector<CMuxInputPin*>::iterator it = m_inputPins.begin();
  while (it != m_inputPins.end())
  {
    delete *it;
    it++;
  }
  m_inputPins.clear();

  if (m_outputPin != NULL)
  {
    delete m_outputPin;
    m_outputPin = NULL;
  }

  if (m_streamingMonitorThreadStopEvent != NULL)
  {
    CloseHandle(m_streamingMonitorThreadStopEvent);
  }
  LogDebug(L"CTsMuxerFilter: completed");
}

CBasePin* CTsMuxerFilter::GetPin(int n)
{
  if (n == 0)
  {
    return m_outputPin;
  }
  n--;

  CAutoLock filterLock(m_pLock);
  if (n < 0 || n >= (int)m_inputPins.size())
  {
    return NULL;
  }
  return m_inputPins[n];
}

HRESULT CTsMuxerFilter::AddPin()
{
  LogDebug(L"CTsMuxerFilter: add pin, current pin count = %d", m_inputPins.size());

  // If any pin is unconnected or the filter is running then don't add a new pin.
  CAutoLock filterLock(m_pLock);
  if (!IsStopped())
  {
    LogDebug(L"CTsMuxerFilter: can't add pin unless filter is stopped");
    return VFW_E_NOT_STOPPED;
  }
  int pinIndex = 0;
  vector<CMuxInputPin*>::iterator it = m_inputPins.begin();
  while (it != m_inputPins.end())
  {
    CMuxInputPin* pin = *it;
    if (!pin->IsConnected())
    {
      LogDebug(L"CTsMuxerFilter: pin %d is available", pinIndex);
      return S_FALSE;
    }
    pinIndex++;
    it++;
  }

  LogDebug(L"CTsMuxerFilter: adding pin %d", pinIndex + 1);
  HRESULT hr = S_OK;
  CMuxInputPin* inputPin = new CMuxInputPin(pinIndex, m_tsMuxer, this, m_pLock, m_receiveLock, &hr);
  if (inputPin == NULL || !SUCCEEDED(hr))
  {
    if (SUCCEEDED(hr))
    {
      hr = E_OUTOFMEMORY;
    }
    LogDebug(L"CTsMuxerFilter: failed to add pin, hr = 0x%x", hr);
    return hr;
  }
  m_inputPins.push_back(inputPin);
  return S_OK;
}

int CTsMuxerFilter::GetPinCount()
{
  CAutoLock filterLock(m_pLock);
  return 1 + m_inputPins.size();
}

HRESULT CTsMuxerFilter::Deliver(PBYTE data, long dataLength)
{
  return m_outputPin->Deliver(data, dataLength);
}

STDMETHODIMP CTsMuxerFilter::Pause()
{
  LogDebug(L"CTsMuxerFilter: pause");
  CAutoLock filterLock(m_pLock);
  LogDebug(L"CTsMuxerFilter: pausing filter...");
  HRESULT hr = CBaseFilter::Pause();
  LogDebug(L"CTsMuxerFilter: completed, hr = 0x%x", hr);
  return hr;
}

STDMETHODIMP CTsMuxerFilter::Run(REFERENCE_TIME startTime)
{
  LogDebug(L"CTsMuxerFilter: run");
  CAutoLock filterLock(m_pLock);

  LogDebug(L"CTsMuxerFilter: starting stream monitor thread...");
  ResetEvent(m_streamingMonitorThreadStopEvent);
  m_streamingMonitorThread = (HANDLE)_beginthread(&CTsMuxerFilter::StreamingMonitorThreadFunction, 0, (void*)this);
  if (m_streamingMonitorThread == INVALID_HANDLE_VALUE)
  {
    return E_POINTER;
  }

  HRESULT hr = m_tsMuxer->Reset();

  LogDebug(L"CTsMuxerFilter: starting filter...");
  hr |= CBaseFilter::Run(startTime);
  LogDebug(L"CTsMuxerFilter: completed, hr = 0x%x", hr);
  return hr;
}

STDMETHODIMP CTsMuxerFilter::Stop()
{
  LogDebug(L"CTsMuxerFilter: stop");
  CAutoLock filterLock(m_pLock);
  LogDebug(L"CTsMuxerFilter: stop receiving...");
  CAutoLock receiveLock(m_receiveLock);

  LogDebug(L"CTsMuxerFilter: stopping stream monitor thread...");
  SetEvent(m_streamingMonitorThreadStopEvent);
  m_streamingMonitorThread = INVALID_HANDLE_VALUE;

  LogDebug(L"CTsMuxerFilter: stopping filter...");
  HRESULT hr = CBaseFilter::Stop();
  LogDebug(L"CTsMuxerFilter: completed, hr = 0x%x", hr);
  return hr;
}

void __cdecl CTsMuxerFilter::StreamingMonitorThreadFunction(void* arg)
{
  LogDebug(L"CTsMuxerFilter: monitor thread started");
  CTsMuxerFilter* filter = (CTsMuxerFilter*)arg;
  IStreamMultiplexer* muxer = filter->m_tsMuxer;
  map<byte, bool> pinStates;
  bool isFirst = true;
  while (true)
  {
    DWORD result = WaitForSingleObject(filter->m_streamingMonitorThreadStopEvent, STREAM_IDLE_TIMEOUT);
    if (result != WAIT_TIMEOUT)
    {
      // event was set
      break;
    }

    if (muxer != NULL && muxer->IsStarted())
    {
      CAutoLock filterLock(filter->m_pLock);
      vector<CMuxInputPin*>::iterator it = filter->m_inputPins.begin();
      if (isFirst)
      {
        while (it != filter->m_inputPins.end())
        {
          CMuxInputPin* pin = *it;
          pinStates[pin->GetId()] = true;
          it++;
        }
        isFirst = false;
      }
      else
      {
        while (it != filter->m_inputPins.end())
        {
          CMuxInputPin* pin = *it;
          byte pinId = pin->GetId();
          bool wasReceiving = pinStates[pinId];
          bool isReceiving = true;
          if (pin->GetReceiveTickCount() == NOT_RECEIVING || GetTickCount() - pin->GetReceiveTickCount() >= STREAM_IDLE_TIMEOUT)
          {
            isReceiving = false;
          }
          if (wasReceiving != isReceiving)
          {
            LogDebug(L"CTsMuxerFilter: pin %d changed receiving state, %d => %d", pinId, wasReceiving, isReceiving);
          }
          pinStates[pinId] = isReceiving;
          it++;
        }
      }
    }
  }
  LogDebug(L"CTsMuxerFilter: monitor thread stopped");
}


//-----------------------------------------------------------------------------
// MULTIPLEXER CLASS
//-----------------------------------------------------------------------------
CTsMuxer::CTsMuxer(LPUNKNOWN unk, HRESULT* hr)
  : CUnknown(NAME("TS Muxer"), unk)
{
  LogDebug(L"CTsMuxer: constructor");

  m_filter = new CTsMuxerFilter(this, GetOwner(), &m_filterLock, &m_receiveLock, hr);
  if (m_filter == NULL)
  {
    *hr = E_OUTOFMEMORY;
    return;
  }

  m_isStarted = false;
  m_isVideoActive = false;
  m_isAudioActive = true;
  m_isTeletextActive = true;

  byte payloadStartFlag = 0x40;

  byte* pointer = &m_patPacket[0];
  *pointer++ = TS_SYNC_BYTE;
  *pointer++ = payloadStartFlag | (PID_PAT >> 8);
  *pointer++ = (PID_PAT & 0xff);
  *pointer++ = 0x10;
  *pointer++ = 0;     // pointer byte
  *pointer++ = TABLE_ID_PAT;
  *pointer++ = 0xb0;
  *pointer++ = 0x0d;  // section length
  *pointer++ = (TRANSPORT_STREAM_ID >> 8);
  *pointer++ = (TRANSPORT_STREAM_ID & 0xff);
  *pointer++ = 0xc1;  // version
  *pointer++ = 0;     // section number
  *pointer++ = 0;     // last section number
  *pointer++ = (SERVICE_ID >> 8);
  *pointer++ = (SERVICE_ID & 0xff);
  *pointer++ = 0xe0 | (PID_PMT_FIRST >> 8);
  *pointer++ = (PID_PMT_FIRST & 0xff);
  DWORD crc = crc32((char*)&m_patPacket[5], 12);
  *pointer++ = (crc >> 24);
  *pointer++ = ((crc >> 16) & 0xff);
  *pointer++ = ((crc >> 8) & 0xff);
  *pointer++ = (crc & 0xff);
  memset(pointer, 0xff, 171);

  pointer = &m_pmtPacket[0];
  *pointer++ = TS_SYNC_BYTE;
  *pointer++ = payloadStartFlag | (PID_PMT_FIRST >> 8);
  *pointer++ = (PID_PMT_FIRST & 0xff);
  *pointer++ = 0x10;
  *pointer++ = 0;     // pointer byte
  *pointer++ = TABLE_ID_PMT;
  *pointer++ = 0xb0;
  *pointer++ = 0;     // section length
  *pointer++ = (SERVICE_ID >> 8);
  *pointer++ = (SERVICE_ID & 0xff);
  *pointer++ = 0xc1;  // version
  *pointer++ = 0;     // section number
  *pointer++ = 0;     // last section number
  *pointer++ = 0xe0;
  *pointer++ = 0;     // PCR PID
  *pointer++ = 0xf0;
  *pointer++ = 0;     // program info length

  pointer = &m_sdtPacket[0];
  *pointer++ = TS_SYNC_BYTE;
  *pointer++ = payloadStartFlag | (PID_SDT >> 8);
  *pointer++ = (PID_SDT & 0xff);
  *pointer++ = 0x10;
  *pointer++ = 0;     // pointer byte
  *pointer++ = TABLE_ID_SDT;
  *pointer++ = 0xf0;
  *pointer++ = 0;     // section length
  *pointer++ = (TRANSPORT_STREAM_ID >> 8);
  *pointer++ = (TRANSPORT_STREAM_ID & 0xff);
  *pointer++ = 0xc1;  // version
  *pointer++ = 0;     // section number
  *pointer++ = 0;     // last section number
  *pointer++ = (ORIGINAL_NETWORK_ID >> 8);
  *pointer++ = (ORIGINAL_NETWORK_ID & 0xff);
  *pointer++ = 0xff;
  *pointer++ = (SERVICE_ID >> 8);
  *pointer++ = (SERVICE_ID & 0xff);
  *pointer++ = 0xfc;  // no EIT
  *pointer++ = 0x80;  // running, not encrypted
  *pointer++ = 0;     // descriptor loop length

  m_patContinuityCounter = 0;

  m_pmtContinuityCounter = 0;
  m_pmtPid = PID_PMT_FIRST;
  m_pmtVersion = 0;

  m_sdtContinuityCounter = 0;
  m_sdtVersion = VERSION_NOT_SET;
  memset(m_serviceName, 0, SERVICE_NAME_LENGTH + 1);
  m_serviceType = SERVICE_TYPE_NOT_SET;
  m_sdtResetTime = TIME_NOT_SET;

  m_packetCounter = 0;
  m_pcrPid = PID_NOT_SET;
  m_nextStreamPid = PID_STREAM_FIRST;
  m_nextVideoStreamId = STREAM_ID_VIDEO_FIRST;
  m_nextAudioStreamId = STREAM_ID_AUDIO_FIRST;

  LogDebug(L"CTsMuxer: completed");
}

CTsMuxer::~CTsMuxer()
{
  LogDebug(L"CTsMuxer: destructor");
  map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    if (sIt->second->pmtDescriptorBytes != NULL)
    {
      delete[] sIt->second->pmtDescriptorBytes;
    }
    delete sIt->second;
    sIt->second = NULL;
    sIt++;
  }
  m_streamInfo.clear();

  map<byte, TransportStreamInfo*>::iterator tsIt = m_transportStreamInfo.begin();
  while (tsIt != m_transportStreamInfo.end())
  {
    delete tsIt->second;
    tsIt->second = NULL;
    tsIt++;
  }
  m_transportStreamInfo.clear();

  map<byte, ProgramStreamInfo*>::iterator psIt = m_programStreamInfo.begin();
  while (psIt != m_programStreamInfo.end())
  {
    delete psIt->second;
    psIt->second = NULL;
    psIt++;
  }
  m_programStreamInfo.clear();

  if (m_filter != NULL)
  {
    delete m_filter;
    m_filter = NULL;
  }
  LogDebug(L"CTsMuxer: completed");
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
  byte pinId = pin->GetId();
  bool removedActiveStream = false;
  map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    if (sIt->second->pinId == pinId)
    {
      removedActiveStream = !(sIt->second->isIgnored) && sIt->second->isCompatible;
      if (sIt->second->pmtDescriptorBytes != NULL)
      {
        delete[] sIt->second->pmtDescriptorBytes;
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
    map<byte, TransportStreamInfo*>::iterator tsIt = m_transportStreamInfo.find(pin->GetId());
    if (tsIt != m_transportStreamInfo.end())
    {
      delete tsIt->second;
      tsIt->second = NULL;
      m_transportStreamInfo.erase(tsIt++);
    }
  }
  else if (pin->GetStreamType() == STREAM_TYPE_MPEG1_SYSTEM_STREAM || pin->GetStreamType() == STREAM_TYPE_MPEG2_PROGRAM_STREAM)
  {
    map<byte, ProgramStreamInfo*>::iterator psIt = m_programStreamInfo.find(pin->GetId());
    if (psIt != m_programStreamInfo.end())
    {
      delete psIt->second;
      psIt->second = NULL;
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

bool CTsMuxer::IsStarted()
{
  return m_isStarted;
}

HRESULT CTsMuxer::Receive(IMuxInputPin* pin, PBYTE data, long dataLength, REFERENCE_TIME dataStartTime)
{
  byte pinId = pin->GetId();
  int streamType = pin->GetStreamType();
  if (streamType == STREAM_TYPE_UNKNOWN)
  {
    LogDebug(L"CTsMuxer: pin %d stream type is not known", pinId);
    return VFW_E_INVALIDMEDIATYPE;
  }

  // Update the SDT if we timed out waiting for the service name.
  if (m_sdtVersion == VERSION_NOT_SET && GetTickCount() - m_sdtResetTime >= TELETEXT_SERVICE_NAME_TIMEOUT)
  {
    UpdateSdt();
  }

  if (streamType == STREAM_TYPE_MPEG2_TRANSPORT_STREAM)
  {
    return ReceiveTransportStream(pin, data, dataLength, dataStartTime);
  }
  else if (streamType == STREAM_TYPE_MPEG1_SYSTEM_STREAM || streamType == STREAM_TYPE_MPEG2_PROGRAM_STREAM)
  {
    return ReceiveProgramOrSystemStream(pin, data, dataLength, dataStartTime);
  }

  // We're processing an elementary stream. Find or create a StreamInfo
  // instance for the pin.
  HRESULT hr = S_OK;
  bool updatePmt = false;
  StreamInfo* info = NULL;
  map<unsigned int, StreamInfo*>::iterator it = m_streamInfo.find(pinId);
  if (it == m_streamInfo.end())
  {
    info = new StreamInfo();
    if (info == NULL)
    {
      return E_OUTOFMEMORY;
    }
    info->pinId = pinId;
    info->streamType = streamType;
    info->pid = m_nextStreamPid++;
    info->continuityCounter = 0;
    info->isCompatible = true;  // default assumption
    info->pmtDescriptorLength = 0;
    info->pmtDescriptorBytes = NULL;
    m_streamInfo[pinId] = info;

    // Log interesting info.
    if (streamType == STREAM_TYPE_AUDIO_MPEG1 || streamType == STREAM_TYPE_AUDIO_MPEG2)
    {
      info->isIgnored = !m_isAudioActive;
      info->streamId = m_nextAudioStreamId++;
      hr = ReadAudioStreamInfo(data, dataLength, info);
    }
    else if (streamType == STREAM_TYPE_VIDEO_MPEG1 || streamType == STREAM_TYPE_VIDEO_MPEG2)
    {
      info->isIgnored = !m_isVideoActive;
      info->streamId = m_nextVideoStreamId++;
      hr = ReadVideoStreamInfo(data, dataLength, info);
    }
    else if (streamType == STREAM_TYPE_TELETEXT)
    {
      LogDebug(L"CTsMuxer: pin %d teletext stream", pinId);
      LogDebug(L"  sample size  = %d bytes", dataLength);
      info->isIgnored = !m_isTeletextActive;
      info->streamId = STREAM_ID_TELETEXT;
      info->pmtDescriptorLength = 2;
      info->pmtDescriptorBytes = new byte[2];
      if (info->pmtDescriptorBytes == NULL)
      {
        return E_OUTOFMEMORY;
      }
      info->pmtDescriptorBytes[0] = DESCRIPTOR_TELETEXT_DVB; // tag
      info->pmtDescriptorBytes[1] = 0;    // length (no page info)
    }
    else
    {
      LogDebug(L"CTsMuxer: pin %d elementary stream with type 0x%x is not supported", pinId, streamType);
      info->isIgnored = false;
      info->isCompatible = false;
      info->streamType = STREAM_TYPE_UNKNOWN;
    }

    // If we fail to parse the stream info or for whatever reason can't be sure
    // that we are able to handle this stream then we won't process it.
    if (!SUCCEEDED(hr))
    {
      info->isCompatible = false;
    }

    if (info->isIgnored && info->isCompatible)
    {
      LogDebug(L"CTsMuxer: pin %d ignoring stream due to active stream type settings", pinId);
    }

    updatePmt = !info->isIgnored;
  }
  else
  {
    info = it->second;
    if (!info->isIgnored &&
      info->isCompatible &&
      (
        info->prevReceiveTickCount == NOT_RECEIVING ||
        GetTickCount() - info->prevReceiveTickCount >= STREAM_IDLE_TIMEOUT
      )
    )
    {
      updatePmt = true;   // stream is restarting
    }
  }
  info->prevReceiveTickCount = GetTickCount();
  if (updatePmt)
  {
    UpdatePmt();
  }

  if (info->isIgnored)
  {
    return S_OK;
  }
  else if (!info->isCompatible)
  {
    // TODO debug
    /*LogDebug(L"debug: pin %d incompatible stream type %d frame", pinId, info->streamType);
    long offset = 0;
    while (offset + 16 < dataLength)
    {
      LogDebug(L"debug: %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x",
        data[offset], data[offset + 1], data[offset + 2], data[offset + 3], data[offset + 4], data[offset + 5], data[offset + 6], data[offset + 7],
        data[offset + 8], data[offset + 9], data[offset + 10], data[offset + 11], data[offset + 12], data[offset + 13], data[offset + 14], data[offset + 15]);
      offset += 16;
    }*/
    return S_OK;
  }

  // Can we start delivering samples? This requires that all pins are receiving
  // and we've seen all the substreams in program, system or transport streams.
  if (!CanDeliver())
  {
    return S_OK;
  }

  // Convert our sample time (10 MHz <=> 100 ns) to a system clock reference
  // (27 MHz <=> ~37 ns).
  REFERENCE_TIME systemClockReference = TIME_NOT_SET;
  REFERENCE_TIME ptsScr = TIME_NOT_SET;
  if (dataStartTime != TIME_NOT_SET)
  {
    systemClockReference = dataStartTime * 27 / 10;
    ptsScr = systemClockReference + 10000000;   // offset PTS from PCR by ~370 ms to give time for demuxing and decoding
  }

  // Wrap the frame into transport stream packets and deliver them.
  PBYTE pesData = NULL;
  long pesDataLength = 0;
  if (info->streamType == STREAM_TYPE_TELETEXT)
  {
    hr = WrapVbiTeletextData(info, data, dataLength, ptsScr, &pesData, &pesDataLength);
  }
  else
  {
    hr = WrapElementaryStreamData(info, data, dataLength, ptsScr, &pesData, &pesDataLength);
  }
  if (SUCCEEDED(hr) && pesDataLength > 0)
  {
    PBYTE tsData = NULL;
    long tsDataLength = 0;
    hr = WrapPacketisedElementaryStreamData(info, pesData, pesDataLength, systemClockReference, &tsData, &tsDataLength);
    if (SUCCEEDED(hr) && tsDataLength > 0)
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
  LogDebug(L"CTsMuxer: reset");
  m_isStarted = false;
  m_sdtVersion = VERSION_NOT_SET;
  map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    sIt->second->prevReceiveTickCount = NOT_RECEIVING;
    sIt++;
  }
  return S_OK;
}

HRESULT CTsMuxer::StreamTypeChange(IMuxInputPin* pin, int oldStreamType, int newStreamType)
{
  // The easiest way to handle a change of stream type is to treat it like
  // disconnecting and reconnecting a pin with a different stream/media type.
  // This will inject a PMT which is almost certainly [temporarily] incorrect.
  // It is not worth the effort to try and avoid this.
  return BreakConnect(pin);
}

STDMETHODIMP CTsMuxer::SetActiveComponents(bool video, bool audio, bool teletext)
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"CTsMuxer: set active components, video = %d, audio = %d, teletext = %d", video, audio, teletext);
  m_isVideoActive = video;
  m_isAudioActive = audio;
  m_isTeletextActive = teletext;
  bool changedActiveStream = false;
  m_serviceType = SERVICE_TYPE_RADIO;
  map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    if (sIt->second->streamType == STREAM_TYPE_VIDEO_MPEG1 || sIt->second->streamType == STREAM_TYPE_VIDEO_MPEG2)
    {
      changedActiveStream = (sIt->second->isIgnored == m_isVideoActive && sIt->second->isCompatible);
      sIt->second->isIgnored = !m_isVideoActive;
      m_serviceType = m_isVideoActive ? SERVICE_TYPE_TELEVISION : SERVICE_TYPE_RADIO;
    }
    else if (sIt->second->streamType == STREAM_TYPE_AUDIO_MPEG1 || sIt->second->streamType == STREAM_TYPE_AUDIO_MPEG2)
    {
      changedActiveStream = (sIt->second->isIgnored == m_isAudioActive && sIt->second->isCompatible);
      sIt->second->isIgnored = !m_isAudioActive;
    }
    else if (sIt->second->streamType == STREAM_TYPE_TELETEXT)
    {
      changedActiveStream = (sIt->second->isIgnored == m_isTeletextActive && sIt->second->isCompatible);
      sIt->second->isIgnored = !m_isTeletextActive;
    }
    sIt++;
  }

  // Clear our SDT details.
  m_sdtVersion = VERSION_NOT_SET;
  memset(m_serviceName, 0, SERVICE_NAME_LENGTH);
  m_sdtResetTime = GetTickCount();

  // Update our PMT PID.
  m_pmtPid++;
  if (m_pmtPid == PID_STREAM_FIRST)
  {
    m_pmtPid = PID_PMT_FIRST;
  }

  // Update the PAT and PMT.
  UpdatePat();
  UpdatePmt();
  return S_OK;
}

STDMETHODIMP CTsMuxer::NonDelegatingQueryInterface(REFIID iid, void** ppv)
{
  /*LogDebug(L"debug: checking for interface %08x-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x",
      iid.Data1, iid.Data2, iid.Data3, iid.Data4[0], iid.Data4[1], iid.Data4[2], iid.Data4[3],
      iid.Data4[4], iid.Data4[5], iid.Data4[6], iid.Data4[7]);*/
  CheckPointer(ppv, E_POINTER);

  if (iid == IID_ITS_MUXER)
  {
    return GetInterface((ITsMuxer*)this, ppv);
  }
  if (iid == IID_IBaseFilter || iid == IID_IMediaFilter || iid == IID_IPersist) {
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
  int pinCount = m_filter->GetPinCount();
  for (int p = 0; p < pinCount; p++)
  {
    CMuxInputPin* pin = (CMuxInputPin*)m_filter->GetPin(p);

    // It doesn't make sense to check if pins that are not connected are
    // receiving.
    IPin* connectedPin;
    int hr = pin->ConnectedTo(&connectedPin);
    if (SUCCEEDED(hr) && connectedPin != NULL)
    {
      connectedPin->Release();
      PIN_DIRECTION direction;
      hr = pin->QueryDirection(&direction);
      if (SUCCEEDED(hr) && direction == PINDIR_INPUT)
      {
        // Check that the pin is receiving. We must be receiving even if the
        // stream is incompatible or we're ignoring it.
        DWORD receiveTickCount = pin->GetReceiveTickCount();
        if (receiveTickCount == NOT_RECEIVING || GetTickCount() - receiveTickCount >= STREAM_IDLE_TIMEOUT)
        {
          return false;
        }

        // Check that we're receiving each substream.
        // How many substreams do we expect from this pin?
        int expectedStreamCount = 0;
        if (pin->GetStreamType() == STREAM_TYPE_MPEG2_TRANSPORT_STREAM)
        {
          map<byte, TransportStreamInfo*>::iterator tsIt = m_transportStreamInfo.find(pin->GetId());
          if (tsIt == m_transportStreamInfo.end())
          {
            // This should never happen.
            return false;
          }

          if (tsIt->second->isCompatible && tsIt->second->streamCount > 0)
          {
            expectedStreamCount = tsIt->second->streamCount;
          }
        }
        else if (pin->GetStreamType() == STREAM_TYPE_MPEG1_SYSTEM_STREAM || pin->GetStreamType() == STREAM_TYPE_MPEG2_PROGRAM_STREAM)
        {
          map<byte, ProgramStreamInfo*>::iterator psIt = m_programStreamInfo.find(pin->GetId());
          if (psIt == m_programStreamInfo.end())
          {
            // This should never happen.
            return false;
          }

          if (psIt->second->isCompatible)
          {
            expectedStreamCount = psIt->second->videoBound + psIt->second->audioBound;
            if (expectedStreamCount < 0)
            {
              // We don't know how many streams are expected because we haven't
              // seen a system header or program stream map yet. This is odd
              // because usually we expect a system header immediately after
              // the first pack. Assume we've seen all streams.
              expectedStreamCount = 0;
            }
          }
        }
        else
        {
          expectedStreamCount = 1;
        }

        // How many substreams are we actually receiving from this pin?
        int receivedStreamCount = 0;
        map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.begin();
        while (sIt != m_streamInfo.end())
        {
          if (sIt->second->pinId == pin->GetId())
          {
            // Any non-ignored compatible stream that we're not receiving is an
            // immediate fail.
            if (!sIt->second->isIgnored &&
              sIt->second->isCompatible &&
              (
                sIt->second->prevReceiveTickCount == NOT_RECEIVING ||
                GetTickCount() - sIt->second->prevReceiveTickCount >= STREAM_IDLE_TIMEOUT
              )
            )
            {
              return false;
            }
            receivedStreamCount++;
          }
          sIt++;
        }

        if (receivedStreamCount < expectedStreamCount)
        {
          return false;
        }
      }
    }
  }

  LogDebug(L"CTsMuxer: starting to deliver...");
  m_isStarted = true;

  // Waiting for SDT?
  if (m_sdtVersion == VERSION_NOT_SET)
  {
    m_sdtResetTime = GetTickCount();
  }
  return true;
}

HRESULT CTsMuxer::ReceiveTransportStream(IMuxInputPin* pin, PBYTE data, long dataLength, REFERENCE_TIME dataStartTime)
{
  // Find or create a TransportStreamInfo instance for the pin.
  byte pinId = pin->GetId();
  bool isFirstReceive = false;
  TransportStreamInfo* tsInfo = NULL;
  map<byte, TransportStreamInfo*>::iterator it = m_transportStreamInfo.find(pinId);
  if (it == m_transportStreamInfo.end())
  {
    tsInfo = new TransportStreamInfo();
    if (tsInfo == NULL)
    {
      return E_OUTOFMEMORY;
    }
    tsInfo->pinId = pinId;
    tsInfo->isCompatible = true;
    tsInfo->pmtPid = PID_NOT_SET;
    tsInfo->patVersion = VERSION_NOT_SET;
    tsInfo->pmtVersion = VERSION_NOT_SET;
    m_transportStreamInfo[pinId] = tsInfo;
  }
  else
  {
    tsInfo = it->second;
  }

  // Skip all processing if we previously determined that this transport stream
  // is incompatible.
  if (isFirstReceive && data[0] != TS_SYNC_BYTE)
  {
    tsInfo->isCompatible = false;
  }
  if (dataLength % TS_PACKET_LENGTH != 0)
  {
    tsInfo->isCompatible = false;
  }
  if (!tsInfo->isCompatible)
  {
    return S_OK;
  }

  byte* outputBuffer = new byte[dataLength];
  if (outputBuffer == NULL)
  {
    return E_OUTOFMEMORY;
  }
  long outputOffset = 0;

  StreamInfo* info = NULL;
  unsigned short previousPid = PID_NOT_SET;
  long inputOffset = 0;
  HRESULT hr = S_OK;
  while (dataLength >= TS_PACKET_LENGTH)
  {
    unsigned short pid = ((data[inputOffset + 1] << 8) & 0x1f) + data[inputOffset + 2];

    if (pid == PID_PAT || (tsInfo->pmtPid != PID_NOT_SET && tsInfo->pmtPid == pid))
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
        hr = ReadProgramAssociationTable(&data[inputOffset + 5], TS_PACKET_LENGTH - 5, tsInfo);
      }
      else
      {
        hr = ReadProgramMapTable(&data[inputOffset + 5], TS_PACKET_LENGTH - 5, tsInfo);
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
        unsigned int streamKey = (pid << 8) + pin->GetId();
        map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.find(streamKey);
        if (sIt != m_streamInfo.end())
        {
          info = sIt->second;
        }
        else
        {
          info = NULL;    // ignore this stream - not part of the service
        }
      }

      if (info != NULL && info->isCompatible && (!info->isIgnored || tsInfo->pcrPid == pid))
      {
        // If we're ignoring the stream that contains PCR then overwrite the
        // content of the packet with padding and inject the packet into our
        // selected PCR stream.
        if (info->isIgnored)
        {
          byte adaptationFieldControl = ((data[inputOffset + 3] >> 4) & 0x03);
          byte adaptationFieldLength = data[inputOffset + TS_HEADER_LENGTH];
          if ((adaptationFieldControl & 0x2) != 0 && adaptationFieldLength > 0 && (data[inputOffset + 5] & 0x10) != 0)
          {
            memcpy(&outputBuffer[outputOffset], &data[inputOffset], TS_PACKET_LENGTH);

            // Fix the PID, adaptation field control and continuity counter.
            outputBuffer[outputOffset + 1] = (m_pcrPid >> 8);
            outputBuffer[outputOffset + 2] = (m_pcrPid & 0xff);
            map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.begin();
            while (sIt != m_streamInfo.end())
            {
              if (sIt->second->pid == m_pcrPid)
              {
                outputBuffer[outputOffset + 3] = 0x20 | sIt->second->continuityCounter;    // adaptation field only, not scrambled
              }
              sIt++;
            }

            // Overwrite the content with padding.
            byte fullAdaptationByteCount = TS_PACKET_LENGTH - TS_HEADER_LENGTH - 1;   // - 1 for adaptation field length
            byte additionalPaddingByteCount = fullAdaptationByteCount - adaptationFieldLength;
            outputBuffer[outputOffset + TS_HEADER_LENGTH] = fullAdaptationByteCount;
            if (additionalPaddingByteCount > 0)
            {
              memset(&outputBuffer[outputOffset + TS_HEADER_LENGTH + adaptationFieldLength], 0xff, additionalPaddingByteCount);
            }
          }
        }
        else
        {
          memcpy(&outputBuffer[outputOffset], &data[inputOffset], TS_PACKET_LENGTH);
        }

        outputOffset += TS_PACKET_LENGTH;
      }

      previousPid = pid;
    }

    dataLength -= TS_PACKET_LENGTH;
    inputOffset += TS_PACKET_LENGTH;
  }

  if (outputOffset != 0)
  {
    hr = DeliverTransportStreamData(outputBuffer, outputOffset);
  }
  delete[] outputBuffer;
  return hr;
}

HRESULT CTsMuxer::ReceiveProgramOrSystemStream(IMuxInputPin* pin, PBYTE data, long dataLength, REFERENCE_TIME dataStartTime)
{
  // Find or create a ProgramStreamInfo instance for the pin.
  byte pinId = pin->GetId();
  bool isFirstReceive = false;
  ProgramStreamInfo* psInfo = NULL;
  map<byte, ProgramStreamInfo*>::iterator it = m_programStreamInfo.find(pinId);
  if (it == m_programStreamInfo.end())
  {
    isFirstReceive = true;
    psInfo = new ProgramStreamInfo();
    if (psInfo == NULL)
    {
      return E_OUTOFMEMORY;
    }
    psInfo->pinId = pinId;
    psInfo->isCompatible = true;
    psInfo->videoBound = -1;
    psInfo->audioBound = -1;
    psInfo->currentMapVersion = VERSION_NOT_SET;
    m_programStreamInfo[pinId] = psInfo;
  }
  else
  {
    psInfo = it->second;
  }

  // Update the SDT if we timed out waiting for the service name.
  if (m_sdtVersion == VERSION_NOT_SET && GetTickCount() - m_sdtResetTime >= TELETEXT_SERVICE_NAME_TIMEOUT)
  {
    UpdateSdt();
  }

  // Skip all processing if we previously determined that this program/system
  // stream is incompatible.
  if (!psInfo->isCompatible)
  {
    return S_OK;
  }

  // Process each program or system stream frame.
  long offset = 0;
  long remainingDataLength = dataLength;
  REFERENCE_TIME systemClockReference = TIME_NOT_SET;
  while (remainingDataLength >= 4)
  {
    // All samples must be aligned to frame boundaries.
    if (data[offset] != 0 || data[offset + 1] != 0 || data[offset + 2] != 1)
    {
      LogDebug(L"CTsMuxer: pin %d program/system stream sample not frame-aligned", pinId);
      psInfo->isCompatible = false;
      return VFW_E_BADALIGN;
    }
    byte streamId = data[offset + 3];
    HRESULT hr = S_OK;

    // What type of frame is this and how do we handle it?
    if (streamId == STREAM_ID_END_OF_STREAM)
    {
      LogDebug(L"CTsMuxer: pin %d received program/system stream end code", pinId);
      offset += 4;
      remainingDataLength -= 4;
    }
    else if (streamId == STREAM_ID_PACK)
    {
      int length = 0;
      hr = ReadProgramOrSystemPack(&data[offset], remainingDataLength, psInfo, isFirstReceive, &length, &systemClockReference);
      offset += length;
      remainingDataLength -= length;
    }
    else
    {
      if (remainingDataLength < 6)
      {
        LogDebug(L"CTsMuxer: pin %d program/system stream packet with length %d is too small to contain a system header, program stream map, padding or data", pinId, remainingDataLength);
        psInfo->isCompatible = false;
        return E_NOT_SUFFICIENT_BUFFER;
      }
      // Ideally we'd have a way to validate that the packet length is correct.
      unsigned short packetLength = (data[offset + 4] << 8) + data[offset + 5];
      if (remainingDataLength < packetLength + 6)
      {
        LogDebug(L"CTsMuxer: pin %d program/system stream appears to spread packets across samples, not compatible", pinId);
        psInfo->isCompatible = false;
        return VFW_E_UNSUPPORTED_STREAM;
      }

      if (streamId == STREAM_ID_SYSTEM_HEADER)
      {
        hr = ReadProgramOrSystemHeader(&data[offset], remainingDataLength, psInfo, isFirstReceive);
      }
      else if (streamId == STREAM_ID_PROGRAM_STREAM_MAP)
      {
        hr = ReadProgramStreamMap(&data[offset], remainingDataLength, psInfo);
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
        unsigned int streamKey = (streamId << 8) + pinId;
        StreamInfo* info = NULL;
        map<unsigned int, StreamInfo*>::iterator it = m_streamInfo.find(streamKey);
        if (it == m_streamInfo.end())
        {
          info = new StreamInfo();
          if (info == NULL)
          {
            return E_OUTOFMEMORY;
          }
          info->pinId = pinId;
          info->pid = m_nextStreamPid++;
          info->continuityCounter = 0;
          info->isCompatible = true;  // default assumption
          info->pmtDescriptorLength = 0;
          info->pmtDescriptorBytes = NULL;
          m_streamInfo[streamKey] = info;

          int offsetToPesData = offset + 9 + data[offset + 8];
          if ((streamId & 0xe0) == STREAM_ID_AUDIO_FIRST)
          {
            info->isIgnored = !m_isAudioActive;
            info->streamId = m_nextAudioStreamId++;
            hr = ReadAudioStreamInfo(&data[offsetToPesData], remainingDataLength - offsetToPesData, info);
          }
          else if ((streamId & 0xf0) == STREAM_ID_VIDEO_FIRST)
          {
            info->isIgnored = !m_isVideoActive;
            info->streamId = m_nextVideoStreamId++;
            hr = ReadVideoStreamInfo(&data[offsetToPesData], remainingDataLength - offsetToPesData, info);
          }
          else
          {
            // other unsupported stream types - private, ECM, EMM, DCMCC, MHEG etc.
            LogDebug(L"CTsMuxer: pin %d program/system stream PES packet with stream ID 0x%x is not supported", pinId, streamId);
            info->isIgnored = false;
            info->isCompatible = false;
          }

          // If we fail to parse the stream info or for whatever reason can't be sure
          // that we are able to handle this stream then we won't process it.
          if (!SUCCEEDED(hr))
          {
            info->isCompatible = false;
          }

          if (info->isIgnored && info->isCompatible)
          {
            LogDebug(L"CTsMuxer: pin %d ignoring substream %d due to active stream type settings", pinId, streamId);
          }

          updatePmt = !info->isIgnored;
        }
        else
        {
          info = it->second;
          if (!info->isIgnored &&
            info->isCompatible &&
            (
              info->prevReceiveTickCount == NOT_RECEIVING ||
              GetTickCount() - info->prevReceiveTickCount >= STREAM_IDLE_TIMEOUT
            )
          )
          {
            updatePmt = true;   // stream is restarting
          }
        }
        info->prevReceiveTickCount = GetTickCount();
        if (updatePmt)
        {
          UpdatePmt();
        }

        // Wrap the PES packet into transport stream packets and deliver them.
        if (!info->isCompatible)
        {
          /*LogDebug(L"debug: pin %d incompatible program/system substream %d frame", pinId, streamId);
          long offset = 0;
          while (offset + 16 < dataLength)
          {
            LogDebug(L"debug: %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x",
              data[offset], data[offset + 1], data[offset + 2], data[offset + 3], data[offset + 4], data[offset + 5], data[offset + 6], data[offset + 7],
              data[offset + 8], data[offset + 9], data[offset + 10], data[offset + 11], data[offset + 12], data[offset + 13], data[offset + 14], data[offset + 15]);
            offset += 16;
          }*/
        }
        else if (!info->isIgnored && CanDeliver())
        {
          PBYTE tsData = NULL;
          long tsDataLength = 0;
          systemClockReference = info->pid == m_pcrPid ? systemClockReference : TIME_NOT_SET;   // only put PCR into the PCR stream
          hr = WrapPacketisedElementaryStreamData(info, &data[offset], packetLength + 6, systemClockReference, &tsData, &tsDataLength);
          if (SUCCEEDED(hr) && tsDataLength > 0)
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
    if (!SUCCEEDED(hr))
    {
      return hr;
    }
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramAssociationTable(PBYTE data, long dataLength, TransportStreamInfo* info)
{
  byte version = ((data[5] >> 1) & 0x1f);
  if (info->patVersion == version)
  {
    return S_OK;
  }
  info->patVersion = version;

  unsigned short transportStreamId = (data[3] << 8) + data[4];
  LogDebug(L"TsMuxer: pin %d new program association table, version = %d, transport stream ID = 0x%x", info->pinId, version, transportStreamId);
  info->transportStreamId = transportStreamId;

  unsigned short sectionLength = ((data[1] & 0x0f) << 8) + data[2];
  unsigned short offset = 8;
  unsigned short endOfServices = offset + (sectionLength - 4 - 5);  // - 4 for CRC, - 5 for other fixed length fields
  byte serviceCount = 0;
  while (offset + 3 < endOfServices)
  {
    unsigned short serviceId = (data[offset] << 8) + data[offset + 1];
    unsigned short pmtPid = ((data[offset + 2] & 0x1f) << 8) + data[offset + 3];
    LogDebug(L"  service, service ID = 0x%x, PMT PID = 0x%x", serviceId, pmtPid);
    if (serviceId != 0)
    {
      serviceCount++;
      // We can only handle transport streams containing one service. Take the
      // first real service (service ID 0 points to the DVB NIT PID).
      if (serviceCount == 1)
      {
        info->serviceId = serviceId;
        info->pmtPid = pmtPid;
      }
    }
    offset += 4;
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramMapTable(PBYTE data, long dataLength, TransportStreamInfo* info)
{
  byte version = ((data[5] >> 1) & 0x1f);
  unsigned short serviceId = (data[3] << 8) + data[4];
  if (info->pmtVersion == version || info->serviceId != serviceId)
  {
    return S_OK;
  }
  info->pmtVersion = version;

  unsigned short pcrPid = ((data[8] & 0x1f) << 8) + data[9];
  LogDebug(L"TsMuxer: pin %d new program map table, version = %d, service ID = 0x%x, PCR PID = 0x%x", info->pinId, version, serviceId, pcrPid);
  info->pcrPid = pcrPid;

  // Set a "marker" on all substreams for this pin so we can remove inactive
  // streams at the end.
  map<unsigned int, StreamInfo*>::iterator sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    if (sIt->second->pinId == info->pinId)
    {
      sIt->second->isIgnored = true;
      sIt->second->streamId = 1;
    }
    sIt++;
  }

  unsigned short sectionLength = ((data[1] & 0x0f) << 8) + data[2];
  unsigned short programInfoLength = ((data[1] & 0x0f) << 8) + data[2];
  long offset = 12;
  long endOfDescriptors = offset + programInfoLength;
  while (offset + 1 < endOfDescriptors)
  {
    byte tag = data[offset++];
    byte length = data[offset++];
    offset += length;
    LogDebug(L"  program descriptor, tag = 0x%x, length = %d", tag, length);
  }

  long endOfStreams = offset + (sectionLength - programInfoLength - 4 - 9);   // - 4 for CRC, - 9 for other fixed length fields
  while (offset + 4 < endOfStreams)
  {
    byte streamType = data[offset++];
    unsigned short elementaryStreamPid = ((data[offset] & 0x1f) << 8) + data[offset + 1];
    offset += 2;

    unsigned int streamKey = (elementaryStreamPid << 8) + info->pinId;
    StreamInfo* streamInfo = NULL;
    sIt = m_streamInfo.find(streamKey);
    if (sIt == m_streamInfo.end())
    {
      streamInfo = new StreamInfo();
      if (streamInfo == NULL)
      {
        return E_OUTOFMEMORY;
      }
      streamInfo->originalPid = elementaryStreamPid;
      streamInfo->continuityCounter = -1;
      streamInfo->isCompatible = true;
      streamInfo->isIgnored = true;
      streamInfo->pid = m_nextStreamPid++;
      streamInfo->pinId = info->pinId;
      streamInfo->prevReceiveTickCount = NOT_RECEIVING;
      m_streamInfo[streamKey] = streamInfo;
    }
    else
    {
      streamInfo = sIt->second;
    }
    streamInfo->streamId = 0;   // unset marker
    streamInfo->streamType = streamType;

    if (streamType == STREAM_TYPE_VIDEO_MPEG1 || streamType == STREAM_TYPE_VIDEO_MPEG2 || streamType == STREAM_TYPE_VIDEO_H264)
    {
      streamInfo->isIgnored = !m_isVideoActive;
    }
    else if (streamType == STREAM_TYPE_AUDIO_MPEG1 || streamType == STREAM_TYPE_AUDIO_MPEG2 || streamType == STREAM_TYPE_AUDIO_AAC || streamType == STREAM_TYPE_AUDIO_LATM_AAC || streamType == STREAM_TYPE_AUDIO_AC3 || streamType == STREAM_TYPE_AUDIO_E_AC3)
    {
      streamInfo->isIgnored = !m_isAudioActive;
    }
    else
    {
      // assume other streams - subtitles, teletext etc. - should be active
      streamInfo->isIgnored = false;
    }
    LogDebug(L"  elementary stream, PID = 0x%x, type = 0x%x, active = %d", elementaryStreamPid, streamType, !streamInfo->isIgnored);

    unsigned short elementaryStreamInfoLength = (data[offset] << 8) + data[offset + 1];
    offset += 2;
    if (streamInfo->pmtDescriptorBytes != NULL)
    {
      delete[] streamInfo->pmtDescriptorBytes;
      streamInfo->pmtDescriptorBytes = NULL;
    }
    if (elementaryStreamInfoLength != 0)
    {
      streamInfo->pmtDescriptorLength = elementaryStreamInfoLength;
      streamInfo->pmtDescriptorBytes = new byte[elementaryStreamInfoLength];
      if (streamInfo->pmtDescriptorBytes == NULL)
      {
        return E_OUTOFMEMORY;
      }
      memcpy(streamInfo->pmtDescriptorBytes, &data[offset], elementaryStreamInfoLength);
    }
    endOfDescriptors = offset + elementaryStreamInfoLength;
    while (offset + 1 < endOfDescriptors)
    {
      byte tag = data[offset++];
      byte length = data[offset++];
      offset += length;
      LogDebug(L"    elementary stream descriptor, tag = 0x%x, length = %d", tag, length);
    }
  }

  // Remove streams that are no longer part of the program.
  sIt = m_streamInfo.begin();
  while (sIt != m_streamInfo.end())
  {
    if (sIt->second->pinId == info->pinId && sIt->second->streamId == 1)
    {
      if (sIt->second->pmtDescriptorBytes != NULL)
      {
        delete[] sIt->second->pmtDescriptorBytes;
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

HRESULT CTsMuxer::ReadProgramOrSystemPack(PBYTE data, long dataLength, ProgramStreamInfo* info, bool isFirstReceive, int* length, REFERENCE_TIME* systemClockReference)
{
  if (isFirstReceive)
  {
    LogDebug(L"CTsMuxer: pin %d program/system stream", info->pinId);
    LogDebug(L"  sample size      = %d bytes", dataLength);
  }

  bool isMpeg2 = false;
  int programMuxRate = 0;
  byte packStuffingLength = 0;
  if ((data[4] & 0xc0) == 0x40)
  {
    // MPEG 2 PS
    if (dataLength < 14)
    {
      LogDebug(L"CTsMuxer: pin %d program stream packet with length %d is too small to contain a pack", info->pinId, dataLength);
      return E_NOT_SUFFICIENT_BUFFER;
    }
    isMpeg2 = true;
    *systemClockReference = ((data[4] & 0x38) << 27);
    *systemClockReference += ((data[4] & 0x03) << 28);
    *systemClockReference += (data[5] << 20);
    *systemClockReference += ((data[6] & 0xf8) << 12);
    *systemClockReference += ((data[6] & 0x03) << 13);
    *systemClockReference += (data[7] << 5);
    *systemClockReference += ((data[8] & 0xf8) >> 3);
    *systemClockReference *= 300;

    *systemClockReference += ((data[8] & 0x03) << 7);
    *systemClockReference += ((data[9] & 0xfe) >> 1);

    programMuxRate = (data[10] << 14);
    programMuxRate += (data[11] << 6);
    programMuxRate += ((data[12] & 0xfc) >> 2);
    programMuxRate *= 50;

    packStuffingLength = (data[13] & 0x07);

    *length = 14 + packStuffingLength;
  }
  else if ((data[4] & 0xf0) == 0x20)
  {
    // MPEG 1 PS
    if (dataLength < 12)
    {
      LogDebug(L"CTsMuxer: pin %d system stream packet with length %d is too small to contain a pack", info->pinId, dataLength);
      return E_NOT_SUFFICIENT_BUFFER;
    }
    isMpeg2 = false;
    *systemClockReference = ((data[4] & 0xe) << 29);
    *systemClockReference += (data[5] << 22);
    *systemClockReference += ((data[6] & 0xfe) << 14);
    *systemClockReference += (data[7] << 7);
    *systemClockReference += ((data[8] & 0xfe) >> 1);
    *systemClockReference *= 300;

    programMuxRate = ((data[9] & 0x7f) << 15);
    programMuxRate += (data[10] << 7);
    programMuxRate += ((data[11] & 0xfe) >> 1);
    programMuxRate *= 50;

    *length = 12;
  }
  else
  {
    LogDebug(L"CTsMuxer: pin %d program/system stream pack format 0x%x is not supported", info->pinId, data[4]);
    return VFW_E_UNSUPPORTED_STREAM;
  }

  if (isFirstReceive)
  {
    LogDebug(L"  is MPEG 2        = %d", isMpeg2);
    LogDebug(L"  program mux rate = %d b/s", programMuxRate);
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramOrSystemHeader(PBYTE data, long dataLength, ProgramStreamInfo* info, bool isFirstReceive)
{
  int rateBound = ((data[6] & 0x7f) << 15);
  rateBound += (data[7] << 7);
  rateBound += ((data[8] & 0xfe) >> 1);
  rateBound *= 50;

  byte audioBound = (data[9] >> 2);
  bool fixedFlag = ((data[9] & 0x02) != 0);
  bool cspsFlag = ((data[9] & 0x01) != 0);
  bool systemAudioLockFlag = ((data[10] & 0x80) != 0);
  bool systemVideoLockFlag = ((data[10] & 0x40) != 0);
  byte videoBound = (data[10] & 0x1f);
  if (isFirstReceive)
  {
    LogDebug(L"  rate bound       = %d b/s", rateBound);
    LogDebug(L"  video bound      = %d", videoBound);
    LogDebug(L"  audio bound      = %d", audioBound);
    LogDebug(L"  fixed            = %d", fixedFlag);
    LogDebug(L"  CSPS             = %d", cspsFlag);
    LogDebug(L"  video lock       = %d", systemVideoLockFlag);
    LogDebug(L"  audio lock       = %d", systemAudioLockFlag);
    info->videoBound = videoBound;
    info->audioBound = audioBound;
  }
  else if (videoBound != info->videoBound || audioBound != info->audioBound)
  {
    if (videoBound != info->videoBound)
    {
      LogDebug(L"CTsMuxer: pin %d program/system video bound changed, %d => %d", info->pinId, info->videoBound, videoBound);
      info->videoBound = videoBound;
    }
    if (audioBound != info->audioBound)
    {
      LogDebug(L"CTsMuxer: pin %d program/system audio bound changed, %d => %d", info->pinId, info->videoBound, videoBound);
      info->audioBound = audioBound;
    }
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadProgramStreamMap(PBYTE data, long dataLength, ProgramStreamInfo* info)
{
  bool currentNextIndicator = ((data[6] & 0x80) != 0);
  byte programStreamMapVersion = (data[6] & 0x1f);
  if (programStreamMapVersion == info->currentMapVersion)
  {
    return S_OK;
  }
  LogDebug(L"CTsMuxer: pin %d program stream map version changed, %d => %d", info->pinId, info->currentMapVersion, programStreamMapVersion);
  info->currentMapVersion = programStreamMapVersion;

  unsigned short programStreamInfoLength = (data[8] << 8) + data[9];
  long offset = 10;
  long endOfDescriptors = offset + programStreamInfoLength;
  while (offset + 1 < endOfDescriptors)
  {
    byte tag = data[offset++];
    byte length = data[offset++];
    offset += length;
    LogDebug(L"  program stream map descriptor, tag = 0x%x, length = %d", tag, length);
  }

  unsigned short elementaryStreamMapLength = (data[offset] << 8) + data[offset + 1];
  offset += 2;
  long endOfStreams = offset + elementaryStreamMapLength;
  while (offset + 3 < endOfStreams)
  {
    byte streamType = data[offset++];
    byte elementaryStreamId = data[offset++];
    LogDebug(L"  elementary stream, stream ID = 0x%x, type = 0x%x", elementaryStreamId, streamType);
    unsigned short elementaryStreamInfoLength = (data[offset] << 8) + data[offset + 1];
    offset += 2;
    endOfDescriptors = offset + elementaryStreamInfoLength;
    while (offset + 1 < endOfDescriptors)
    {
      byte tag = data[offset++];
      byte length = data[offset++];
      offset += length;
      LogDebug(L"    elementary stream descriptor, tag = 0x%x, length = %d", tag, length);
    }
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadVideoStreamInfo(PBYTE data, long dataLength, StreamInfo* info)
{
  LogDebug(L"CTsMuxer: pin %d video stream", info->pinId);
  LogDebug(L"  sample size  = %d bytes", dataLength);
  if (dataLength < 20 || data[0] != 0 || data[1] != 0 || data[2] != 1)
  {
    LogDebug(L"CTsMuxer: pin %d video stream first sample not frame-aligned or unexpected format", info->pinId);
    return VFW_E_BADALIGN;
  }

  if (data[3] == 0xb3)  // sequence header
  {
    int horizontalResolution = (data[4] << 4) + (data[5] >> 4);
    int verticalResolution = ((data[5] & 0xf) << 8) + data[6];
    double aspectRatio = VIDEO_ASPECT_RATIOS[(data[7] >> 4)];
    double frameRate = VIDEO_FRAME_RATES[data[7] & 0xf];
    __int64 bitRate = (data[8] << 10) + (data[9] << 2) + (data[10] >> 6);
    bool isMpeg2 = (data[15] == 0xb5);  // extension start code?
    int profile = 0;
    int level = 0;
    if (isMpeg2 && (data[16] >> 4) == 1) // sequence extension
    {
      profile = (data[16] & 0x7);
      level = (data[17] >> 4);
      horizontalResolution += (((data[17] & 1) + (data[18] >> 7)) << 12);
      verticalResolution += ((data[18] & 0x60) << 7);
      bitRate += (((data[18] & 0x1f) + (data[19] & 0xfe)) << 17);
    }
    bitRate *= 400;
    LogDebug(L"  is MPEG 2    = %d", isMpeg2);
    LogDebug(L"  resolution   = %dx%d", horizontalResolution, verticalResolution);
    LogDebug(L"  aspect ratio = %lf", aspectRatio);
    LogDebug(L"  frame rate   = %lf f/s", frameRate);
    LogDebug(L"  bit rate     = %d b/s", bitRate);
    LogDebug(L"  profile      = %s", profile == 5 ? L"simple" : (profile == 4 ? L"main" : (profile == 3 ? L"SNR scalable" : (profile == 2 ? L"spacially scalable" : L"high"))));
    LogDebug(L"  level        = %s", level == 0x1010 ? L"low" : (level == 0x1000 ? L"main" : (level == 0x0110 ? L"high 1440" : L"high")));

    info->streamType = isMpeg2 ? STREAM_TYPE_VIDEO_MPEG2 : STREAM_TYPE_VIDEO_MPEG1;
  }
  else
  {
    info->streamType = STREAM_TYPE_UNKNOWN;
  }
  return S_OK;
}

HRESULT CTsMuxer::ReadAudioStreamInfo(PBYTE data, long dataLength, StreamInfo* info)
{
  LogDebug(L"CTsMuxer: pin %d audio stream", info->pinId);
  LogDebug(L"  sample size        = %d bytes", dataLength);
  if (dataLength < 4 || data[0] != 0xff || (data[1] & 0xf0) != 0xf0)
  {
    LogDebug(L"CTsMuxer: pin %d audio stream first sample not frame-aligned or unexpected format", info->pinId);
    return VFW_E_BADALIGN;
  }

  bool isMpeg2 = (data[1] & 0x08) == 0;
  int layer = 4 - ((data[1] >> 1) & 0x3);
  int bitRate = AUDIO_BIT_RATES[isMpeg2][layer][data[2] >> 4];
  int samplingFrequency = AUDIO_SAMPLE_RATES[isMpeg2][(data[2] >> 2) & 0x3];
  int mode = (data[3] >> 6);
  LogDebug(L"  is MPEG 2          = %d", isMpeg2);
  LogDebug(L"  layer              = %d", layer);
  LogDebug(L"  bit rate           = %d kb/s", bitRate);
  LogDebug(L"  sampling frequency = %d Hz", samplingFrequency);
  LogDebug(L"  mode               = %s", mode == 0 ? L"stereo" : (mode == 1 ? L"joint" : (mode == 2 ? L"dual channel" : L"mono")));

  info->streamType = isMpeg2 ? STREAM_TYPE_AUDIO_MPEG2 : STREAM_TYPE_AUDIO_MPEG1;
  return S_OK;
}

HRESULT CTsMuxer::UpdatePat()
{
  // Update the PMT PID for our service.
  byte* pointer = &m_patPacket[15];
  *pointer++ = 0xe0 | (m_pmtPid >> 8);
  *pointer++ = (m_pmtPid & 0xff);
  DWORD crc = crc32((char*)&m_patPacket[5], 12);
  *pointer++ = (crc >> 24);
  *pointer++ = ((crc >> 16) & 0xff);
  *pointer++ = ((crc >> 8) & 0xff);
  *pointer++ = (crc & 0xff);

  m_packetCounter = 0;  // trigger PAT to be delivered before the next content
  return S_OK;
}

HRESULT CTsMuxer::UpdatePmt()
{
  int sectionLength = 13;   // size of fixed-length parts of the section (including CRC)

  // Append stream info to the PMT and decide which PID should be the PCR PID.
  byte activeStreamCount = 0;
  m_pcrPid = PID_NOT_SET;
  bool pcrPidIsVideo = false;
  byte* pointer = &m_pmtPacket[17];
  map<unsigned int, StreamInfo*>::iterator it = m_streamInfo.begin();
  while (it != m_streamInfo.end())
  {
    StreamInfo* info = it->second;
    if (info->isIgnored ||
      !info->isCompatible ||
      info->prevReceiveTickCount == NOT_RECEIVING ||
      GetTickCount() - info->prevReceiveTickCount >= STREAM_IDLE_TIMEOUT
    )
    {
      it++;
      continue;
    }
    activeStreamCount++;

    if (info->streamType == STREAM_TYPE_TELETEXT)
    {
      *pointer++ = STREAM_TYPE_PES_PRIVATE_DATA;
    }
    else
    {
      *pointer++ = info->streamType;
    }
    *pointer++ = 0xe0 | (info->pid >> 8);
    *pointer++ = (info->pid & 0xff);
    if (info->pmtDescriptorLength > 0 && info->pmtDescriptorBytes != NULL)
    {
      *pointer++ = 0xf0 | (info->pmtDescriptorLength >> 8);
      *pointer++ = (info->pmtDescriptorLength & 0xff);
      memcpy(pointer, info->pmtDescriptorBytes, info->pmtDescriptorLength);
      pointer += info->pmtDescriptorLength;
      sectionLength += info->pmtDescriptorLength;
    }
    else
    {
      *pointer++ = 0xf0;
      *pointer++ = 0;
    }
    sectionLength += 5;

    // Update the PCR PID. We select the first non-teletext stream by default
    // but we prefer a video stream.
    if ((m_pcrPid == PID_NOT_SET && info->streamType != STREAM_TYPE_TELETEXT) ||
      (!pcrPidIsVideo && (info->streamType == STREAM_TYPE_VIDEO_MPEG1 || info->streamType == STREAM_TYPE_VIDEO_MPEG2))
    )
    {
      m_pcrPid = info->pid;
      pcrPidIsVideo = (info->streamType == STREAM_TYPE_VIDEO_MPEG1 || info->streamType == STREAM_TYPE_VIDEO_MPEG2);
    }

    it++;
  }

  m_pmtPacket[1] = 0x40 | (m_pmtPid >> 8);  // payload start
  m_pmtPacket[2] = (m_pmtPid & 0xff);

  m_pmtPacket[6] = 0xb0 | (sectionLength >> 8);
  m_pmtPacket[7] = (sectionLength & 0xff);

  m_pmtVersion = ((m_pmtVersion + 1) & 0x1f);
  m_pmtPacket[10] = 0xc1 | (m_pmtVersion << 1);

  m_pmtPacket[13] = 0xe0 | (m_pcrPid >> 8);
  m_pmtPacket[14] = (m_pcrPid & 0xff);

  DWORD crc = crc32((char*)&m_pmtPacket[5], sectionLength - 4 + 3); // - 4 for CRC + 3 for table ID and section length
  *pointer++ = (crc >> 24);
  *pointer++ = ((crc >> 16) & 0xff);
  *pointer++ = ((crc >> 8) & 0xff);
  *pointer++ = (crc & 0xff);
  int stuffingLength = TS_PACKET_LENGTH - TS_HEADER_LENGTH - 1 - 3 - sectionLength; // - 1 for pointer byte - 3 for table ID and section length
  if (stuffingLength < 0)
  {
    LogDebug(L"CTsMuxer: PMT requires more than one packet, not supported");
    return E_FAIL;
  }
  memset(pointer, 0xff, stuffingLength);

  byte oldServiceType = m_serviceType;
  m_serviceType = pcrPidIsVideo ? SERVICE_TYPE_TELEVISION : SERVICE_TYPE_RADIO;
  if (oldServiceType != m_serviceType)
  {
    UpdateSdt();
  }

  LogDebug(L"CTsMuxer: updated PMT, PID = 0x%x, active stream count = %d, version = %d, service type = %d", m_pmtPid, activeStreamCount, m_pmtVersion, m_serviceType);
  m_packetCounter = 0;  // trigger PMT to be delivered before the next content
  return S_OK;
}

HRESULT CTsMuxer::UpdateSdt()
{
  byte serviceNameLength = strlen(m_serviceName);
  byte serviceDescriptorLength = 3 + serviceNameLength;
  byte descriptorLoopLength = 2 + serviceDescriptorLength;
  byte sectionLength = 17 + descriptorLoopLength;

  m_sdtPacket[7] = sectionLength;

  m_sdtVersion = ((m_sdtVersion + 1) & 0x1f);
  m_sdtPacket[10] = 0xc1 | (m_sdtVersion << 1);

  PBYTE pointer = &m_sdtPacket[20];
  *pointer++ = descriptorLoopLength;
  *pointer++ = DESCRIPTOR_DVB_SERVICE;
  *pointer++ = serviceDescriptorLength;
  *pointer++ = m_serviceType;
  *pointer++ = 0;     // provider name length
  *pointer++ = serviceNameLength;
  memcpy(pointer, m_serviceName, serviceNameLength);
  pointer += serviceNameLength;

  DWORD crc = crc32((char*)&m_sdtPacket[5], sectionLength - 4 + 3); // - 4 for CRC + 3 for table ID and section length
  *pointer++ = (crc >> 24);
  *pointer++ = ((crc >> 16) & 0xff);
  *pointer++ = ((crc >> 8) & 0xff);
  *pointer++ = (crc & 0xff);

  int stuffingLength = TS_PACKET_LENGTH - TS_HEADER_LENGTH - 1 - 3 - sectionLength; // - 1 for pointer byte - 3 for table ID and section length
  if (stuffingLength < 0)
  {
    LogDebug(L"CTsMuxer: SDT requires more than one packet, not supported");
    return E_FAIL;
  }
  memset(pointer, 0xff, stuffingLength);
  m_packetCounter = 0;  // trigger SDT to be delivered before the next content
  return S_OK;
}

HRESULT CTsMuxer::WrapVbiTeletextData(StreamInfo* info, PBYTE inputData, long inputDataLength, REFERENCE_TIME systemClockReference, PBYTE* outputData, long* outputDataLength)
{
  *outputDataLength = 0;
  *outputData = NULL;
  if (inputDataLength == 0 || systemClockReference == TIME_NOT_SET)   // we must have a PTS for teletext packets, therefore we must have an SCR
  {
    return S_OK;
  }
  if (info == NULL || inputData == NULL)
  {
    return E_POINTER;
  }
  if (inputDataLength % VBI_LINE_LENGTH != 0)
  {
    LogDebug(L"CTsMuxer: pin %d teletext stream input with size %d contains partial lines", info->pinId, inputDataLength);
    return E_UNEXPECTED;
  }

  // Refer to EN 300 472.
  // Convert the raw VBI data into PES data.
  long pesBufferLength = TELETEXT_PES_STUFFING_LENGTH + 1 + ((3 + VBI_LINE_LENGTH) * (inputDataLength / VBI_LINE_LENGTH));  // + 1 for data indentifier, + 3 for field header
  byte* pesBuffer = new byte[pesBufferLength];
  if (pesBuffer == NULL)
  {
    return E_OUTOFMEMORY;
  }

  bool gotRealData = false;
  byte* inputPointer = inputData;
  byte* pesPointer = &pesBuffer[TELETEXT_PES_STUFFING_LENGTH + 1]; // + 1 for data identifier
  while (inputDataLength >= VBI_LINE_LENGTH)
  {
    // If this line has real content...
    if (*inputPointer != 0 || *(inputPointer + 1) != 0 || *(inputPointer + 2) != 0 || *(inputPointer + 3) != 0 || *(inputPointer + 4) != 0)
    {
      gotRealData = true;
      *pesPointer++ = TELETEXT_DATA_UNIT_ID;
      *pesPointer++ = 1 + VBI_LINE_LENGTH;    // data unit length, + 1 for line offset
      *pesPointer++ = 0xc0;                   // undefined line offset
      *pesPointer++ = 0xe4;                   // framing code
      inputPointer++;                         // (skipped framing code)
      byte* linePointer = pesPointer;
      for (byte i = 1; i < VBI_LINE_LENGTH; i++)  // 1.. because we skip/overwrite the framing code
      {
        *pesPointer++ = REVERSE_BITS[*inputPointer++];
      }

      // Side task - search for the channel name for the SDT.
      byte mapa1 = *linePointer;
      byte mapa2 = *(linePointer + 1);
      byte magazineAndPacketAddress = unham(mapa1, mapa2);
      byte magazineNumber = magazineAndPacketAddress & 0x07;
      byte packetNumber = magazineAndPacketAddress >> 3;
      if (magazineNumber == 0 && packetNumber == 30)
      {
        // Use the last SERVICE_NAME_LENGTH bytes (excluding the last byte)
        // from the line as the service name.
        linePointer += (VBI_LINE_LENGTH - SERVICE_NAME_LENGTH - 1);
        char tempName[SERVICE_NAME_LENGTH + 1];
        for (int i = 0; i < SERVICE_NAME_LENGTH; i++)
        {
          tempName[i] = (char)(*linePointer++ & 0x7f);
        }
        tempName[SERVICE_NAME_LENGTH] = '\0';
        if (strcmp(tempName, m_serviceName) != 0)
        {
          LogDebug(L"CTsMuxer: found channel name '%s' in teletext", tempName);
          strcpy(m_serviceName, tempName);
          UpdateSdt();
        }
      }
    }
    else
    {
      inputPointer += VBI_LINE_LENGTH;
    }
    inputDataLength -= VBI_LINE_LENGTH;
  }

  // If we have some real teletext data (ie. not all zeroes)...
  HRESULT hr = S_OK;
  if (gotRealData)
  {
    memset(pesBuffer, 0xff, TELETEXT_PES_STUFFING_LENGTH);
    pesBuffer[TELETEXT_PES_STUFFING_LENGTH] = TELETEXT_DATA_IDENTIFIER;
    hr = WrapElementaryStreamData(info, pesBuffer, pesBufferLength, systemClockReference, outputData, outputDataLength);
    if (SUCCEEDED(hr) && *outputDataLength > 0)
    {
      // Overwrite the PES header length to include the extra stuffing required
      // by EN 300 472.
      (*outputData)[8] = TELETEXT_PES_STUFFING_LENGTH + PTS_LENGTH;
    }
  }
  delete[] pesBuffer;
  return hr;
}

HRESULT CTsMuxer::WrapElementaryStreamData(StreamInfo* info, PBYTE inputData, long inputDataLength, REFERENCE_TIME systemClockReference, PBYTE* outputData, long* outputDataLength)
{
  *outputDataLength = 0;
  *outputData = NULL;
  if (inputDataLength == 0)
  {
    return S_OK;
  }
  if (info == NULL || inputData == NULL)
  {
    return E_POINTER;
  }

  int packetLength = inputDataLength + 3; // + 3 for flags and header length
  byte alignmentFlag = 0;   // We assume that the stream is frame aligned if the sample time is set.
  if (systemClockReference != TIME_NOT_SET)
  {
    packetLength += PTS_LENGTH;
    alignmentFlag = 0x04;   // aligned
  }
  *outputDataLength = packetLength + 6; // + 6 for header overhead (0 0 1 etc.)
  *outputData = new byte[*outputDataLength];
  if (*outputData == NULL)
  {
    return E_OUTOFMEMORY;
  }
  if (packetLength >= 0x10000)
  {
    if (info->streamType == STREAM_TYPE_VIDEO_MPEG1 || info->streamType == STREAM_TYPE_VIDEO_MPEG2)
    {
      packetLength = 0;   // length not specified, only allowed for video PES packets carried in a TS
    }
    else
    {
      LogDebug(L"CTsMuxer: pin %d extended length %d cannot be used for stream type %d", info->pinId, packetLength, info->streamType);
      return E_UNEXPECTED;
    }
  }
  PBYTE pointer = *outputData;
  *pointer++ = 0;
  *pointer++ = 0;
  *pointer++ = 1;
  *pointer++ = info->streamId;
  *pointer++ = (packetLength >> 8);
  *pointer++ = (packetLength & 0xff);
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
    REFERENCE_TIME pts = (systemClockReference / 300) & 0x1ffffffff;
    *pointer++ = 0x20 | ((pts >> 29) & 0x0e) | 1;
    *pointer++ = ((pts >> 22) & 0xff);
    *pointer++ = ((pts >> 14) & 0xfe) | 1;
    *pointer++ = ((pts >> 7) & 0xff);
    *pointer++ = ((pts << 1) & 0xfe) | 1;
  }
  memcpy(pointer, inputData, inputDataLength);
  return S_OK;
}

HRESULT CTsMuxer::WrapPacketisedElementaryStreamData(StreamInfo* info, PBYTE inputData, long inputDataLength, REFERENCE_TIME systemClockReference, PBYTE* outputData, long* outputDataLength)
{
  *outputDataLength = 0;
  *outputData = NULL;
  if (inputDataLength == 0)
  {
    return S_OK;
  }
  if (info == NULL || inputData == NULL)
  {
    return E_POINTER;
  }

  bool writePcr = false;
  long outputBufferSize = inputDataLength;
  if (systemClockReference != TIME_NOT_SET && info->pid == m_pcrPid)
  {
    writePcr = true;
    outputBufferSize += 2 + PCR_LENGTH;   // + 2 for adaptation field length and flags
  }

  // Calculate a rough output buffer size. This must be large enough to hold
  // the largest number of packets that we would generate from the input.
  long packetCount = (outputBufferSize / (TS_PACKET_LENGTH - TS_HEADER_LENGTH)) + 1;  // + 1 for safety
  outputBufferSize = packetCount * TS_PACKET_LENGTH;
  *outputData = new byte[outputBufferSize];
  if (*outputData == NULL)
  {
    return E_OUTOFMEMORY;
  }
  PBYTE inputPointer = inputData;
  PBYTE outputPointer = *outputData;
  *outputDataLength = 0;
  byte firstPacketFlags = 0x40;   // payload start

  // Generate packets until we run out of data. The first packet may have a
  // PCR; the last packet may be padded.
  while (inputDataLength > 0)
  {
    *outputPointer++ = TS_SYNC_BYTE;
    *outputPointer++ = firstPacketFlags | (info->pid >> 8);
    firstPacketFlags = 0;
    *outputPointer++ = (info->pid & 0xff);

    byte dataByteCount = TS_PACKET_LENGTH - TS_HEADER_LENGTH;
    byte stuffingByteCount = 0;

    if (writePcr || inputDataLength < (TS_PACKET_LENGTH - TS_HEADER_LENGTH))
    {
      *outputPointer++ = (0x30 | info->continuityCounter);
      if (writePcr)
      {
        byte maxDataByteCount = TS_PACKET_LENGTH - TS_HEADER_LENGTH - PCR_LENGTH - 2;   // - 2 for adaptation field length and flags
        stuffingByteCount = (byte)max(maxDataByteCount - inputDataLength, 0);
        dataByteCount = maxDataByteCount - stuffingByteCount;
        *outputPointer++ = 1 + PCR_LENGTH + stuffingByteCount;  // adaptation field length, + 1 for flags
        *outputPointer++ = 0x10;                                // flags: PCR present

        // Convert the system clock reference (27 MHz) to a program clock rate.
        REFERENCE_TIME pcrBase = ((systemClockReference / 300) & 0x1ffffffff);  // 90 kHz
        REFERENCE_TIME pcrExt = (systemClockReference & 0x1ff);                 // 27 MHz
        *outputPointer++ = ((pcrBase >> 25) & 0xff);
        *outputPointer++ = ((pcrBase >> 17) & 0xff);
        *outputPointer++ = ((pcrBase >> 9) & 0xff);
        *outputPointer++ = ((pcrBase >> 1) & 0xff);
        *outputPointer++ = (byte)(((pcrBase & 0x01) << 7) | 0x7e | (pcrExt >> 8));
        *outputPointer++ = (pcrExt & 0xff);
        writePcr = false;
      }
      else
      {
        if (inputDataLength == TS_PACKET_LENGTH - TS_HEADER_LENGTH - 1)   // - 1 for adaptation field length
        {
          *outputPointer++ = 0;                                 // adaptation field length
        }
        else
        {
          stuffingByteCount = (byte)(TS_PACKET_LENGTH - TS_HEADER_LENGTH - 2 - inputDataLength);  // - 2 for adaptation field length and flags
          *outputPointer++ = 1 + stuffingByteCount;             // adaptation field length, + 1 for flags
          *outputPointer++ = 0;                                 // flags: (none)
        }
        dataByteCount = (byte)inputDataLength;
      }

      if (stuffingByteCount > 0)
      {
        memset(outputPointer, 0xff, stuffingByteCount);
        outputPointer += stuffingByteCount;
      }
    }
    else
    {
      *outputPointer++ = (0x10 | info->continuityCounter);
    }

    memcpy(outputPointer, inputPointer, dataByteCount);
    outputPointer += dataByteCount;
    inputPointer += dataByteCount;
    inputDataLength -= dataByteCount;

    info->continuityCounter = ((info->continuityCounter + 1) & 0xf);
    *outputDataLength += TS_PACKET_LENGTH;
  }

  return S_OK;
}

HRESULT CTsMuxer::DeliverTransportStreamData(PBYTE inputData, long inputDataLength)
{
  if (inputDataLength == 0)
  {
    return S_OK;
  }
  if (inputData == NULL)
  {
    return E_POINTER;
  }

  HRESULT hr = S_OK;
  while (inputDataLength > 0)
  {
    // Inject a PAT, PMT and SDT once in every X packets.
    if (m_packetCounter == 0)
    {
      m_patContinuityCounter = ((m_patContinuityCounter + 1) & 0xf);
      m_patPacket[3] &= 0xf0;
      m_patPacket[3] |= m_patContinuityCounter;
      hr = m_filter->Deliver(&m_patPacket[0], TS_PACKET_LENGTH);
      if (!SUCCEEDED(hr))
      {
        return hr;
      }

      m_pmtContinuityCounter = ((m_pmtContinuityCounter + 1) & 0xf);
      m_pmtPacket[3] &= 0xf0;
      m_pmtPacket[3] |= m_pmtContinuityCounter;
      hr = m_filter->Deliver(&m_pmtPacket[0], TS_PACKET_LENGTH);
      if (!SUCCEEDED(hr))
      {
        break;
      }

      // Only inject the SDT when we've had the chance to set it.
      if (m_sdtVersion != TIME_NOT_SET)
      {
        m_sdtContinuityCounter = ((m_sdtContinuityCounter + 1) & 0xf);
        m_sdtPacket[3] &= 0xf0;
        m_sdtPacket[3] |= m_sdtContinuityCounter;
        hr = m_filter->Deliver(&m_sdtPacket[0], TS_PACKET_LENGTH);
        if (!SUCCEEDED(hr))
        {
          break;
        }
      }
      m_packetCounter = m_isVideoActive ? 100 : 20;
    }

    // At most we deliver 100 packets per sample.
    int bytesToDeliver = m_packetCounter * TS_PACKET_LENGTH;      // bytes that should be delivered before another PAT and PMT
    int bytesCanDeliver = min(inputDataLength, bytesToDeliver);   // bytes that we have available
    int packetsToDeliver = bytesCanDeliver / TS_PACKET_LENGTH;
    hr = m_filter->Deliver(inputData, bytesCanDeliver);
    if (!SUCCEEDED(hr))
    {
      return hr;
    }
    inputData += bytesCanDeliver;
    inputDataLength -= bytesCanDeliver;
    m_packetCounter -= packetsToDeliver;
  }
  return hr;
}