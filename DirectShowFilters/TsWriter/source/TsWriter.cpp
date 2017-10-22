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
#include "TsWriter.h"
#include <algorithm>  // min()
#include <iomanip>    // setfill(), setw()
#include <shlobj.h>   // SHGetSpecialFolderPathW()
#include <sstream>
#include <stdio.h>    // _wfopen(), fclose()
#include <string>
#include <Windows.h>  // MAX_PATH
#include "EncryptionState.h"
#include "ParserMgt.h"
#include "ParserPat.h"
#include "ParserSdt.h"
#include "ParserSttAtsc.h"
#include "Version.h"


//-----------------------------------------------------------------------------
// DIRECTSHOW CONFIGURATION
//-----------------------------------------------------------------------------
const AMOVIESETUP_PIN PIN_TYPE_INFORMATION[] =
{
  {
    L"TS Input",                    // [obsolete] the pin type name
    FALSE,                          // does the filter render this pin type?
    FALSE,                          // is this an output pin?
    FALSE,                          // can the filter have zero instances of this pin type?
    FALSE,                          // can the filter have multiple instances of this pin type?
    &CLSID_NULL,                    // [obsolete] the CLSID of the filter that this pin type connects to
    L"Output",                      // [obsolete] the name of the pin that this pin type connects to
    INPUT_MEDIA_TYPE_COUNT_TS,      // the number of media types supported by this pin type
    INPUT_MEDIA_TYPES_TS            // the media types supported by this pin type
  },
  {
    L"OOB SI Input",                // [obsolete] the pin type name
    FALSE,                          // [not applicable] does the filter render this pin type?
    FALSE,                          // is this an output pin?
    FALSE,                          // can the filter have zero instances of this pin type?
    FALSE,                          // can the filter have multiple instances of this pin type?
    &CLSID_NULL,                    // [obsolete] the CLSID of the filter that this pin type connects to
    L"Output",                      // [obsolete] the name of the pin that this pin type connects to
    INPUT_MEDIA_TYPE_COUNT_OOB_SI,  // the number of media types supported by this pin type
    INPUT_MEDIA_TYPES_OOB_SI        // the media types supported by this pin type
  }
};

const AMOVIESETUP_FILTER FILTER_INFORMATION =
{
  &CLSID_TS_WRITER,             // CLSID
  L"MediaPortal TS Writer",     // name
  MERIT_DO_NOT_USE,             // merit
  2,                            // pin type count
  PIN_TYPE_INFORMATION,         // pin type details
  CLSID_LegacyAmFilterCategory  // category
};

CFactoryTemplate g_Templates[] =
{
  L"MediaPortal TS Writer", &CLSID_TS_WRITER, CTsWriter::CreateInstance, NULL, &FILTER_INFORMATION
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
static wstring g_logFilePath;
static wstring g_logFileName;
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
    wstringstream logFileName;
    logFileName << g_logFilePath << L"\\TsWriter-" << systemTime.wYear <<
                    L"-" << setfill(L'0') << setw(2) << systemTime.wMonth <<
                    L"-" << setw(2) << systemTime.wDay << L".log";
    g_logFileName = logFileName.str();
    g_currentDay = systemTime.wDay;
  }

  FILE* file = _wfopen(g_logFileName.c_str(), L"a+, ccs=UTF-8");
  if (file != NULL)
  {
    va_list ap;
    va_start(ap, fmt);
    vswprintf(g_logBuffer, sizeof(g_logBuffer) / sizeof(g_logBuffer[0]), fmt, ap);
    va_end(ap);
    fwprintf(file, L"%04.4hd-%02.2hd-%02.2hd %02.2hd:%02.2hd:%02.2hd.%03.3hd %s\n",
              systemTime.wYear, systemTime.wMonth, systemTime.wDay,
              systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
              systemTime.wMilliseconds, g_logBuffer);
    fclose(file);
  }

  //::OutputDebugStringW(g_logBuffer);
  //::OutputDebugStringW(L"\n");
};


//-----------------------------------------------------------------------------
// WRITER/ANALYSER CLASS
//-----------------------------------------------------------------------------
CTsWriter::CTsWriter(LPUNKNOWN unk, HRESULT* hr)
  : CUnknown(NAME("TS Writer"), unk)
{
  wchar_t temp[MAX_PATH];
  ::SHGetSpecialFolderPathW(NULL, temp, CSIDL_COMMON_APPDATA, FALSE);
  g_logFilePath = temp;
  g_logFilePath += L"\\Team MediaPortal\\MediaPortal TV Server\\log";

  LogDebug(L"--------------- v%d.%d.%d.0 ---------------", VERSION_TS_WRITER_MAJOR, VERSION_TS_WRITER_MINOR, VERSION_TS_WRITER_MICRO);
  LogDebug(L"TVE 3.5 rewritten version");
  LogDebug(L"writer: constructor");

  m_filter = new CTsWriterFilter(this,
                                  g_logFilePath.c_str(),
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
    LogDebug(L"writer: failed to allocate filter, hr = 0x%x", *hr);
    return;
  }

  // service information grabbers
  m_grabberSiAtsc = new CGrabberSiAtscScte(PID_ATSC_BASE, this, GetOwner(), hr);
  if (m_grabberSiAtsc == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate ATSC SI grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberSiDvb = new CGrabberSiDvb(this, GetOwner(), hr);
  if (m_grabberSiDvb == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate DVB SI grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberSiFreesat = new CGrabberSiDvb(this, GetOwner(), hr);
  if (m_grabberSiFreesat == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate Freesat SI grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberSiMpeg = new CGrabberSiMpeg(this, &m_encryptionAnalyser, GetOwner(), hr);
  if (m_grabberSiMpeg == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate MPEG SI grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberSiScte = new CGrabberSiAtscScte(PID_SCTE_BASE, this, GetOwner(), hr);
  if (m_grabberSiScte == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate SCTE SI grabber, hr = 0x%x", *hr);
    return;
  }

  // electronic programme guide grabbers
  m_grabberEpgAtsc = new CGrabberEpgAtsc(this, m_grabberSiAtsc, GetOwner(), hr);
  if (m_grabberEpgAtsc == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate ATSC EPG grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberEpgDvb = new CParserEitDvb(this, this, GetOwner(), hr);
  if (m_grabberEpgDvb == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate DVB EPG grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberEpgMhw = new CParserMhw(this, m_grabberSiDvb, GetOwner(), hr);
  if (m_grabberEpgMhw == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate MHW EPG grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberEpgOpenTv = new CParserOpenTv(this, GetOwner(), hr);
  if (m_grabberEpgOpenTv == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate OpenTV EPG grabber, hr = 0x%x", *hr);
    return;
  }
  m_grabberEpgScte = new CParserAet(this, m_grabberSiScte, GetOwner(), hr);
  if (m_grabberEpgScte == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"writer: failed to allocate SCTE EPG grabber, hr = 0x%x", *hr);
    return;
  }

  m_grabberSiDvb->SetMediaHighwayChannelInfoProvider(m_grabberEpgMhw);
  m_grabberSiFreesat->SetMediaHighwayChannelInfoProvider(m_grabberEpgMhw);

  m_nextChannelId = 0;

  m_openTvEpgServiceId = 0;
  m_isOpenTvEpgServiceRunning = false;
  m_openTvEpgPmtPid = 0;

  m_checkedIsFreesatTransportStream = false;
  m_isFreesatTransportStream = false;
  m_freesatProgramNumber = 0;

  m_isRunning = false;
  m_checkSectionCrcs = true;
  m_observer = NULL;
  LogDebug(L"writer: completed");

  // To test EPG grabbing.
  /*m_grabberEpgAtsc->Reset(true);
  m_grabberEpgAtsc->Start();
  m_grabberEpgDvb->Reset(true);
  m_grabberEpgDvb->SetProtocols(true, true, true, true, true, true, true, true);
  m_grabberEpgMhw->Reset(true);
  m_grabberEpgMhw->SetProtocols(true, true);
  m_grabberEpgOpenTv->Reset(true);
  m_grabberEpgOpenTv->Start();
  m_grabberEpgScte->Reset(true);
  m_grabberEpgScte->Start();

  m_isRunning = true;*/
}

CTsWriter::~CTsWriter()
{
  LogDebug(L"writer: destructor");
  DeleteAllChannels();

  // electronic programme guide grabbers
  if (m_grabberEpgAtsc != NULL)
  {
    delete m_grabberEpgAtsc;
    m_grabberEpgAtsc = NULL;
  }
  if (m_grabberEpgDvb != NULL)
  {
    delete m_grabberEpgDvb;
    m_grabberEpgDvb = NULL;
  }
  if (m_grabberEpgMhw != NULL)
  {
    delete m_grabberEpgMhw;
    m_grabberEpgMhw = NULL;
  }
  if (m_grabberEpgOpenTv != NULL)
  {
    delete m_grabberEpgOpenTv;
    m_grabberEpgOpenTv = NULL;
  }
  if (m_grabberEpgScte != NULL)
  {
    delete m_grabberEpgScte;
    m_grabberEpgScte = NULL;
  }

  // service information grabbers
  if (m_grabberSiAtsc != NULL)
  {
    delete m_grabberSiAtsc;
    m_grabberSiAtsc = NULL;
  }
  if (m_grabberSiDvb != NULL)
  {
    delete m_grabberSiDvb;
    m_grabberSiDvb = NULL;
  }
  if (m_grabberSiFreesat != NULL)
  {
    delete m_grabberSiFreesat;
    m_grabberSiFreesat = NULL;
  }
  if (m_grabberSiMpeg != NULL)
  {
    delete m_grabberSiMpeg;
    m_grabberSiMpeg = NULL;
  }
  if (m_grabberSiScte != NULL)
  {
    delete m_grabberSiScte;
    m_grabberSiScte = NULL;
  }

  if (m_filter != NULL)
  {
    delete m_filter;
    m_filter = NULL;
  }
  LogDebug(L"writer: completed");
}

CUnknown* WINAPI CTsWriter::CreateInstance(LPUNKNOWN unk, HRESULT* hr)
{
  ASSERT(hr);
  CTsWriter* writer = new CTsWriter(unk, hr);
  if (writer == NULL)
  {
    *hr = E_OUTOFMEMORY;
  }
  return writer;
}

void CTsWriter::AnalyseOobSiSection(const CSection& section)
{
  if (!m_isRunning)
  {
    return;
  }

  if (section.TableId == TABLE_ID_AEIT || section.TableId == TABLE_ID_AETT)
  {
    m_grabberEpgScte->OnNewSection(PID_SCTE_BASE, section.TableId, section);
    return;
  }

  m_grabberSiScte->OnNewSection(PID_SCTE_BASE, section.TableId, section, true);
}

void CTsWriter::AnalyseTsPacket(const unsigned char* tsPacket)
{
  try
  {
    if (!m_isRunning)
    {
      return;
    }

    CTsHeader header;
    header.Decode(tsPacket);

    // These grabbers handle PIDs that should be unique and/or impossible to
    // confuse with program PIDs. If a packet is handled by one of these
    // grabbers, we don't have to pass the packet to other grabbers (ie. the
    // packet is "consumed").
    if (
      // service information
      m_grabberSiMpeg->OnTsPacket(header, tsPacket) ||   // For best compatibility, this must be first.
      m_grabberSiAtsc->OnTsPacket(header, tsPacket) ||
      m_grabberSiDvb->OnTsPacket(header, tsPacket) ||
      (m_isFreesatTransportStream && m_grabberSiFreesat->OnTsPacket(header, tsPacket)) ||
      m_grabberSiScte->OnTsPacket(header, tsPacket) ||

      // electronic programme guide
      m_grabberEpgAtsc->OnTsPacket(header, tsPacket) ||
      m_grabberEpgScte->OnTsPacket(header, tsPacket)
    )
    {
      return;
    }

    // These grabbers handle PIDs that may be used by programs. In other words,
    // the packets could contain EPG data... or they could contain program
    // data. Therefore, we pass all packets through all grabbers.

    // electronic programme guide
    m_grabberEpgDvb->OnTsPacket(header, tsPacket);
    m_grabberEpgMhw->OnTsPacket(header, tsPacket);
    m_grabberEpgOpenTv->OnTsPacket(header, tsPacket);

    // If the packet content is not encrypted...
    unsigned char tsPacket2[TS_PACKET_LEN];
    memcpy(tsPacket2, tsPacket, TS_PACKET_LEN);
    if (!m_encryptionAnalyser.OnTsPacket(header, tsPacket2))
    {
      CAutoLock lock(&m_channelLock);
      vector<CTsChannel*>::const_iterator it = m_channels.begin();
      for ( ; it != m_channels.end(); it++)
      {
        (*it)->OnTsPacket(header, tsPacket2);
      }
    }
  }
  catch (...)
  {
    LogDebug(L"writer: unhandled exception in AnalyseTsPacket()");
  }
}

STDMETHODIMP CTsWriter::ConfigureLogging(wchar_t* path)
{
  LogDebug(L"writer: configure logging, path = %s", path == NULL ? L"" : path);
  if (path == NULL)
  {
    return E_INVALIDARG;
  }
  HRESULT hr = m_filter->SetDumpFilePath(path);
  CAutoLock lock(&g_logFilePathLock);
  g_logFilePath = path;
  return hr;
}

STDMETHODIMP_(void) CTsWriter::DumpInput(bool enableTs, bool enableOobSi)
{
  LogDebug(L"writer: dump input, TS = %d, OOB SI = %d", enableTs, enableOobSi);
  m_filter->DumpInput(enableTs, enableOobSi);
}

STDMETHODIMP_(void) CTsWriter::CheckSectionCrcs(bool enable)
{
  LogDebug(L"writer: check section CRCs, enable = %d", enable);
  m_checkSectionCrcs = enable;
  m_filter->CheckSectionCrcs(enable);
}

STDMETHODIMP_(void) CTsWriter::SetObserver(IObserver* observer)
{
  LogDebug(L"writer: set observer, observer = %d", observer != NULL);
  CAutoLock lock(&m_receiveLock);
  m_observer = observer;
}

STDMETHODIMP_(void) CTsWriter::Start()
{
  LogDebug(L"writer: start");
  CAutoLock lock(&m_receiveLock);
  m_isRunning = true;
}

STDMETHODIMP_(void) CTsWriter::Stop()
{
  LogDebug(L"writer: stop");
  CAutoLock lock(&m_receiveLock);
  m_isRunning = false;

  bool enableCrcCheck = m_checkSectionCrcs && !TsWriterDisableCrcCheck();
  // electronic programme guide grabbers
  m_grabberEpgAtsc->Reset(enableCrcCheck);
  m_grabberEpgDvb->Reset(enableCrcCheck);
  m_grabberEpgMhw->Reset(enableCrcCheck);
  m_grabberEpgOpenTv->Reset(enableCrcCheck);
  m_grabberEpgScte->Reset(enableCrcCheck);

  // service information grabbers
  m_grabberSiAtsc->Reset(enableCrcCheck);
  m_grabberSiDvb->Reset(enableCrcCheck);
  m_grabberSiFreesat->Reset(enableCrcCheck);
  m_grabberSiMpeg->Reset();
  m_grabberSiScte->Reset(enableCrcCheck);

  m_openTvEpgServiceId = 0;
  m_isOpenTvEpgServiceRunning = false;
  m_openTvEpgPmtPid = 0;
  m_openTvEpgPidsEvent.clear();
  m_openTvEpgPidsDescription.clear();

  m_atscEpgPidsEit.clear();
  m_atscEpgPidsEtt.clear();
  m_scteEpgPids.clear();

  m_checkedIsFreesatTransportStream = false;
  m_isFreesatTransportStream = false;
  m_freesatProgramNumber = 0;

  m_encryptionAnalyser.Reset();
}

STDMETHODIMP CTsWriter::AddChannel(IChannelObserver* observer, long* handle)
{
  CAutoLock lock(&m_channelLock);
  LogDebug(L"writer: add channel %ld, channel count = %llu",
            m_nextChannelId, (unsigned long long)m_channels.size());
  CTsChannel* channel = new CTsChannel(m_nextChannelId++);
  if (channel == NULL)
  {
    LogDebug(L"writer: failed to allocate channel");
    m_nextChannelId--;
    return E_OUTOFMEMORY;
  }

  channel->Recorder.SetObserver(observer);
  channel->TimeShifter.SetObserver(observer);
  m_channels.push_back(channel);
  *handle = channel->Id;
  return S_OK;
}

STDMETHODIMP_(void) CTsWriter::GetPidState(unsigned short pid, unsigned long* state)
{
  EncryptionState tempState = m_encryptionAnalyser.GetPidState(pid);
  *state = (unsigned long)tempState;
}

STDMETHODIMP_(void) CTsWriter::DeleteChannel(long handle)
{
  CAutoLock lock(&m_channelLock);
  LogDebug(L"writer: delete channel %ld, channel count = %llu",
            handle, (unsigned long long)m_channels.size());
  vector<CTsChannel*>::iterator it = m_channels.begin();
  for ( ; it != m_channels.end(); it++)
  {
    CTsChannel* channel = *it;
    if (channel != NULL && channel->Id == handle)
    {
      delete channel;
      *it = NULL;
      m_channels.erase(it);
      if (m_channels.size() == 0)
      {
        m_nextChannelId = 0;
      }
      return;
    }
  }
  LogDebug(L"writer: failed to find channel %ld to delete", handle);
}

STDMETHODIMP_(void) CTsWriter::DeleteAllChannels()
{
  CAutoLock lock(&m_channelLock);
  LogDebug(L"writer: delete all channels, channel count = %llu",
            (unsigned long long)m_channels.size());
  vector<CTsChannel*>::iterator it = m_channels.begin();
  for ( ; it != m_channels.end(); it++)
  {
    CTsChannel* channel = *it;
    if (channel != NULL)
    {
      delete channel;
      *it = NULL;
    }
  }
  m_channels.clear();
  m_nextChannelId = 0;
}

STDMETHODIMP CTsWriter::RecorderSetFileName(long handle, wchar_t* fileName)
{
  LogDebug(L"writer: set recorder file name, channel = %ld, name = %s",
            handle, fileName == NULL ? L"" : fileName);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  return channel->Recorder.SetFileName(fileName);
}

STDMETHODIMP CTsWriter::RecorderSetPmt(long handle,
                                        unsigned char* pmt,
                                        unsigned short pmtSize,
                                        bool isDynamicPmtChange)
{
  LogDebug(L"writer: set recorder PMT, channel = %ld, PMT size = %hu, is dynamic change = %d",
            handle, pmtSize, isDynamicPmtChange);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  return channel->Recorder.SetPmt(pmt, pmtSize, isDynamicPmtChange);
}

STDMETHODIMP CTsWriter::RecorderStart(long handle)
{
  LogDebug(L"writer: start recorder, channel = %ld", handle);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  return channel->Recorder.Start();
}

STDMETHODIMP CTsWriter::RecorderPause(long handle, bool isPause)
{
  LogDebug(L"writer: pause/unpause recorder, channel = %ld, is pause = %d",
            handle, isPause);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  channel->Recorder.Pause(isPause);
  return S_OK;
}

STDMETHODIMP CTsWriter::RecorderGetStreamQuality(long handle,
                                                  unsigned long long* countTsPackets,
                                                  unsigned long long* countDiscontinuities,
                                                  unsigned long long* countDroppedBytes)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  channel->Recorder.GetStreamQualityCounters(*countTsPackets,
                                              *countDiscontinuities,
                                              *countDroppedBytes);
  return S_OK;
}

STDMETHODIMP CTsWriter::RecorderStop(long handle)
{
  LogDebug(L"writer: stop recorder, channel = %ld", handle);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  channel->Recorder.Stop();
  return S_OK;
}

STDMETHODIMP CTsWriter::TimeShifterSetFileName(long handle, wchar_t* fileName)
{
  LogDebug(L"writer: set time-shifter file name, channel = %ld, name = %s",
            handle, fileName == NULL ? L"" : fileName);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  return channel->TimeShifter.SetFileName(fileName);
}

STDMETHODIMP CTsWriter::TimeShifterSetParameters(long handle,
                                                  unsigned long fileCountMinimum,
                                                  unsigned long fileCountMaximum,
                                                  unsigned long long fileSizeBytes)
{
  LogDebug(L"writer: set time-shifter parameters, file count minimum = %lu, file count maximum = %lu, file size = %llu bytes",
            fileCountMinimum, fileCountMaximum, fileSizeBytes);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  return channel->TimeShifter.SetTimeShiftingParameters(fileCountMinimum,
                                                        fileCountMaximum,
                                                        fileSizeBytes);
}

STDMETHODIMP CTsWriter::TimeShifterSetPmt(long handle,
                                          unsigned char* pmt,
                                          unsigned short pmtSize,
                                          bool isDynamicPmtChange)
{
  LogDebug(L"writer: set time-shifter PMT, channel = %ld, PMT size = %hu, is dynamic change = %d",
            handle, pmtSize, isDynamicPmtChange);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  return channel->TimeShifter.SetPmt(pmt, pmtSize, isDynamicPmtChange);
}

STDMETHODIMP CTsWriter::TimeShifterStart(long handle)
{
  LogDebug(L"writer: start time-shifter, channel = %ld", handle);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  return channel->TimeShifter.Start();
}

STDMETHODIMP CTsWriter::TimeShifterPause(long handle, bool isPause)
{
  LogDebug(L"writer: pause/unpause time-shifter, channel = %ld, is pause = %d",
            handle, isPause);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  channel->TimeShifter.Pause(isPause);
  return S_OK;
}

STDMETHODIMP CTsWriter::TimeShifterGetStreamQuality(long handle,
                                                    unsigned long long* countTsPackets,
                                                    unsigned long long* countDiscontinuities,
                                                    unsigned long long* countDroppedBytes)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  channel->TimeShifter.GetStreamQualityCounters(*countTsPackets,
                                                *countDiscontinuities,
                                                *countDroppedBytes);
  return S_OK;
}

STDMETHODIMP CTsWriter::TimeShifterGetCurrentFilePosition(long handle,
                                                          unsigned long long* position,
                                                          unsigned long* bufferId)
{
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  channel->TimeShifter.GetTimeShiftingFilePosition(*position, *bufferId);
  return S_OK;
}

STDMETHODIMP CTsWriter::TimeShifterStop(long handle)
{
  LogDebug(L"writer: stop time-shifter, channel = %ld", handle);
  CAutoLock lock(&m_channelLock);
  CTsChannel* channel = GetChannel(handle);
  if (channel == NULL)
  {
    return E_FAIL;
  }
  channel->TimeShifter.Stop();
  return S_OK;
}

STDMETHODIMP CTsWriter::NonDelegatingQueryInterface(REFIID iid, void ** ppv)
{
  if (ppv == NULL)
  {
    return E_POINTER;
  }

  // electronic programme guide
  if (iid == IID_IGRABBER_EPG_ATSC)
  {
    return m_grabberEpgAtsc->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_EPG_DVB)
  {
    return m_grabberEpgDvb->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_EPG_MHW)
  {
    return m_grabberEpgMhw->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_EPG_OPENTV)
  {
    return m_grabberEpgOpenTv->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_EPG_SCTE)
  {
    return m_grabberEpgScte->NonDelegatingQueryInterface(iid, ppv);
  }

  // service information
  if (iid == IID_IGRABBER_SI_ATSC)
  {
    return m_grabberSiAtsc->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_SI_DVB)
  {
    return m_grabberSiDvb->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_SI_FREESAT)
  {
    return m_grabberSiFreesat->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_SI_MPEG)
  {
    return m_grabberSiMpeg->NonDelegatingQueryInterface(iid, ppv);
  }
  if (iid == IID_IGRABBER_SI_SCTE)
  {
    return m_grabberSiScte->NonDelegatingQueryInterface(iid, ppv);
  }

  // other
  if (iid == IID_ITS_WRITER)
  {
    return GetInterface((ITsWriter*)this, ppv);
  }
  if (iid == IID_IBaseFilter || iid == IID_IMediaFilter || iid == IID_IPersist)
  {
    return m_filter->NonDelegatingQueryInterface(iid, ppv);
  }

  return CUnknown::NonDelegatingQueryInterface(iid, ppv);
}

CTsChannel* CTsWriter::GetChannel(long handle)
{
  vector<CTsChannel*>::const_iterator it = m_channels.begin();
  for ( ; it != m_channels.end(); it++)
  {
    CTsChannel* channel = *it;
    if (channel != NULL && channel->Id == handle)
    {
      return channel;
    }
  }
  LogDebug(L"writer: failed to find channel %ld", handle);
  return NULL;
}

bool CTsWriter::GetDefaultAuthority(unsigned short originalNetworkId,
                                    unsigned short transportStreamId,
                                    unsigned short serviceId,
                                    char* defaultAuthority,
                                    unsigned short& defaultAuthorityBufferSize) const
{
  if (
    m_isFreesatTransportStream &&
    m_grabberSiFreesat->GetDefaultAuthority(originalNetworkId,
                                            transportStreamId,
                                            serviceId,
                                            defaultAuthority,
                                            defaultAuthorityBufferSize)
  )
  {
    return true;
  }

  return m_grabberSiDvb->GetDefaultAuthority(originalNetworkId,
                                              transportStreamId,
                                              serviceId,
                                              defaultAuthority,
                                              defaultAuthorityBufferSize);
}

void CTsWriter::OnTableSeen(unsigned char tableId)
{
  if (tableId == TABLE_ID_STT_ATSC)
  {
    m_grabberEpgAtsc->OnTableSeen(tableId);
  }
  else if (tableId == TABLE_ID_STT_SCTE)
  {
    m_grabberEpgScte->OnTableSeen(tableId);
  }
}

void CTsWriter::OnTableComplete(unsigned char tableId)
{
  if (tableId == TABLE_ID_STT_ATSC)
  {
    m_grabberEpgAtsc->OnTableSeen(tableId);
  }
  else if (tableId == TABLE_ID_STT_SCTE)
  {
    m_grabberEpgScte->OnTableSeen(tableId);
  }
  else if (tableId == TABLE_ID_PAT)
  {
    unsigned short transportStreamId;
    unsigned short networkPid;
    unsigned short programCount;
    m_grabberSiMpeg->GetTransportStreamDetail(&transportStreamId,
                                              &networkPid,
                                              &programCount);
    if (m_observer != NULL)
    {
      m_observer->OnProgramAssociationTable(transportStreamId,
                                            networkPid,
                                            programCount);
    }
    if (m_grabberSiDvb->IsReadySdtActual())
    {
      unsigned short originalNetworkId;
      unsigned short serviceCount;
      m_grabberSiDvb->GetServiceCount(&originalNetworkId, &serviceCount);
    }
  }
  else if (tableId == TABLE_ID_SDT_ACTUAL)
  {
    unsigned short originalNetworkId;
    unsigned short serviceCount;
    m_grabberSiDvb->GetServiceCount(&originalNetworkId, &serviceCount);
    m_grabberEpgOpenTv->SetOriginalNetworkId(originalNetworkId);
    if (m_grabberSiMpeg->IsReadyPat())
    {
      unsigned short transportStreamId;
      unsigned short networkPid;
      unsigned short programCount;
      m_grabberSiMpeg->GetTransportStreamDetail(&transportStreamId,
                                                &networkPid,
                                                &programCount);
      m_grabberEpgMhw->SetTransportStream(originalNetworkId,
                                          transportStreamId);
    }
  }

  if (tableId != TABLE_ID_MGT)
  {
    return;
  }

  vector<unsigned short> pidsToRemove;
  unsigned short tableType;
  unsigned short pid;
  unsigned char versionNumber;
  unsigned long numberBytes;

  // Update ATSC EPG grabber PIDs.
  unsigned short tableCount = m_grabberSiAtsc->GetMasterGuideTableCount();
  if (tableCount == 0)
  {
    if (m_atscEpgPidsEit.size() > 0)
    {
      m_grabberEpgAtsc->RemoveEitDecoders(m_atscEpgPidsEit);
      m_atscEpgPidsEit.clear();
    }
    if (m_atscEpgPidsEtt.size() > 0)
    {
      m_grabberEpgAtsc->RemoveEttDecoders(m_atscEpgPidsEtt);
      m_atscEpgPidsEtt.clear();
    }
  }
  else
  {
    vector<unsigned short> pidsToAddEit;
    vector<unsigned short> pidsEit;
    vector<unsigned short> pidsToAddEtt;
    vector<unsigned short> pidsEtt;
    for (unsigned short i = 0; i < tableCount; i++)
    {
      if (!m_grabberSiAtsc->GetMasterGuideTable(i, tableType, pid, versionNumber, numberBytes))
      {
        break;
      }
      if (tableType >= 0x100 && tableType <= 0x17f)       // EIT
      {
        if (find(m_atscEpgPidsEit.begin(), m_atscEpgPidsEit.end(), pid) == m_atscEpgPidsEit.end())
        {
          if (find(pidsToAddEit.begin(), pidsToAddEit.end(), pid) == pidsToAddEit.end())
          {
            pidsToAddEit.push_back(pid);
          }
        }
        else
        {
          pidsEit.push_back(pid);
        }
      }
      else if (tableType >= 0x200 && tableType <= 0x27f)  // ETT
      {
        if (find(m_atscEpgPidsEtt.begin(), m_atscEpgPidsEtt.end(), pid) == m_atscEpgPidsEtt.end())
        {
          if (find(pidsToAddEtt.begin(), pidsToAddEtt.end(), pid) == pidsToAddEtt.end())
          {
            pidsToAddEtt.push_back(pid);
          }
        }
        else
        {
          pidsEtt.push_back(pid);
        }
      }
    }

    vector<unsigned short>::iterator it = m_atscEpgPidsEit.begin();
    while (it != m_atscEpgPidsEit.end())
    {
      if (find(pidsEit.begin(), pidsEit.end(), *it) == pidsEit.end())
      {
        pidsToRemove.push_back(*it);
        it = m_atscEpgPidsEit.erase(it);
      }
      else
      {
        it++;
      }
    }
    if (pidsToRemove.size() > 0)
    {
      m_grabberEpgAtsc->RemoveEitDecoders(pidsToRemove);
      pidsToRemove.clear();
    }
    if (pidsToAddEit.size() > 0)
    {
      m_grabberEpgAtsc->AddEitDecoders(pidsToAddEit);
      it = pidsToAddEit.begin();
      for ( ; it != pidsToAddEit.end(); it++)
      {
        m_atscEpgPidsEit.push_back(*it);
      }
    }

    it = m_atscEpgPidsEtt.begin();
    while (it != m_atscEpgPidsEtt.end())
    {
      if (find(pidsEtt.begin(), pidsEtt.end(), *it) == pidsEtt.end())
      {
        pidsToRemove.push_back(*it);
        it = m_atscEpgPidsEtt.erase(it);
      }
      else
      {
        it++;
      }
    }
    if (pidsToRemove.size() > 0)
    {
      m_grabberEpgAtsc->RemoveEttDecoders(pidsToRemove);
      pidsToRemove.clear();
    }
    if (pidsToAddEtt.size() > 0)
    {
      m_grabberEpgAtsc->AddEttDecoders(pidsToAddEtt);
      it = pidsToAddEtt.begin();
      for ( ; it != pidsToAddEtt.end(); it++)
      {
        m_atscEpgPidsEtt.push_back(*it);
      }
    }
  }

  // Update SCTE EPG grabber PIDs.
  tableCount = m_grabberSiScte->GetMasterGuideTableCount();
  if (tableCount == 0)
  {
    if (m_scteEpgPids.size() > 0)
    {
      m_grabberEpgScte->RemoveDecoders(m_scteEpgPids);
      m_scteEpgPids.clear();
    }
  }
  else
  {
    vector<unsigned short> pidsToAdd;
    vector<unsigned short> pids;
    for (unsigned short i = 0; i < tableCount; i++)
    {
      if (!m_grabberSiScte->GetMasterGuideTable(i, tableType, pid, versionNumber, numberBytes))
      {
        break;
      }
      if (
        (tableType >= 0x1000 && tableType <= 0x10ff) ||
        (tableType >= 0x1100 && tableType <= 0x11ff)
      )
      {
        if (find(m_scteEpgPids.begin(), m_scteEpgPids.end(), pid) == m_scteEpgPids.end())
        {
          if (find(pidsToAdd.begin(), pidsToAdd.end(), pid) == pidsToAdd.end())
          {
            pidsToAdd.push_back(pid);
          }
        }
        else
        {
          pids.push_back(pid);
        }
      }
    }

    vector<unsigned short>::iterator it = m_scteEpgPids.begin();
    while (it != m_scteEpgPids.end())
    {
      if (find(pids.begin(), pids.end(), *it) == pids.end())
      {
        pidsToRemove.push_back(*it);
        it = m_scteEpgPids.erase(it);
      }
      else
      {
        it++;
      }
    }
    if (pidsToRemove.size() > 0)
    {
      m_grabberEpgScte->RemoveDecoders(pidsToRemove);
    }
    if (pidsToAdd.size() > 0)
    {
      m_grabberEpgScte->AddDecoders(pidsToAdd);
      it = pidsToAdd.begin();
      for ( ; it != pidsToAdd.end(); it++)
      {
        m_scteEpgPids.push_back(*it);
      }
    }
  }
}

void CTsWriter::OnTableChange(unsigned char tableId)
{
  if (tableId == TABLE_ID_STT_ATSC)
  {
    m_grabberEpgAtsc->OnTableSeen(tableId);
  }
  else if (tableId == TABLE_ID_STT_SCTE)
  {
    m_grabberEpgScte->OnTableSeen(tableId);
  }
}

void CTsWriter::OnEncryptionStateChange(unsigned short pid,
                                        EncryptionState statePrevious,
                                        EncryptionState stateNew)
{
  if (m_observer != NULL && (statePrevious != EncryptionStateNotSet || stateNew == Encrypted))
  {
    m_observer->OnPidEncryptionStateChange(pid, (unsigned long)stateNew);
  }
}

void CTsWriter::OnPidsRequired(unsigned short* pids, unsigned char pidCount, PidUsage usage)
{
  if (m_observer != NULL)
  {
    m_observer->OnPidsRequired(pids, pidCount, (unsigned long)usage);
  }
}

void CTsWriter::OnPidsNotRequired(unsigned short* pids, unsigned char pidCount, PidUsage usage)
{
  if (m_observer != NULL)
  {
    m_observer->OnPidsNotRequired(pids, pidCount, (unsigned long)usage);
  }
}

void CTsWriter::OnFreesatPids(unsigned short pidEitSchedule,
                              unsigned short pidEitPresentFollowing,
                              unsigned short pidSdt,
                              unsigned short pidBat,
                              unsigned short pidTdt,
                              unsigned short pidTot,
                              unsigned short pidNit)
{
  m_isFreesatTransportStream = true;

  m_grabberSiFreesat->SetPids(pidBat, pidNit, pidSdt, pidTot);
  if (m_observer != NULL)
  {
    unsigned short pids[3];
    unsigned char pidCount = 0;
    if (pidSdt != 0)
    {
      pids[pidCount++] = pidSdt;
    }
    if (pidBat != 0 && pidBat != pidSdt)
    {
      pids[pidCount++] = pidBat;
    }
    if (pidNit != 0 && pidNit != pidSdt && pidNit != pidBat)
    {
      pids[pidCount++] = pidNit;
    }
    // Don't request the TDT/TOT PID. It isn't required for SI grabbing.
    if (pidCount != 0)
    {
      m_observer->OnPidsRequired(pids, pidCount, (unsigned long)Si);
    }
  }

  m_grabberEpgDvb->SetFreesatPids(pidBat, pidEitPresentFollowing, pidEitSchedule, pidNit, pidSdt);
}

void CTsWriter::OnSdtRunningStatus(unsigned short serviceId, unsigned char runningStatus)
{
  // Don't bother notifying unless the service is not running. If the service
  // is running PMT should be received shortly.
  bool isRunning = true;
  if (runningStatus == 1 || runningStatus == 5)
  {
    if (m_observer != NULL)
    {
      m_observer->OnProgramDetail(serviceId, 0, false, NULL, 0);
    }
    isRunning = false;
  }

  if (m_openTvEpgServiceId != serviceId || m_isOpenTvEpgServiceRunning == isRunning)
  {
    return;
  }

  LogDebug(L"writer: OpenTV EPG service running status changed, is running = %d", isRunning);
  if (isRunning)
  {
    if (m_openTvEpgPidsEvent.size() > 0)
    {
      m_grabberEpgOpenTv->AddEventDecoders(m_openTvEpgPidsEvent);
    }
    if (m_openTvEpgPidsDescription.size() > 0)
    {
      m_grabberEpgOpenTv->AddDescriptionDecoders(m_openTvEpgPidsDescription);
    }
  }
  else
  {
    if (m_openTvEpgPidsEvent.size() > 0)
    {
      m_grabberEpgOpenTv->RemoveEventDecoders(m_openTvEpgPidsEvent);
    }
    if (m_openTvEpgPidsDescription.size() > 0)
    {
      m_grabberEpgOpenTv->RemoveDescriptionDecoders(m_openTvEpgPidsDescription);
    }
  }
  m_isOpenTvEpgServiceRunning = isRunning;
}

void CTsWriter::OnOpenTvEpgService(unsigned short serviceId, unsigned short originalNetworkId)
{
  if (m_openTvEpgServiceId == serviceId)
  {
    // Duplicate nofication is odd. We don't ever expect to see 2 OpenTV EPG
    // services in the same transport stream.
    return;
  }

  // If we've received PMT for this service then we can immediately determine
  // whether it's a valid OpenTV EPG service. Otherwise it becomes a candidate
  // and we wait for PMT.
  unsigned short pmtPid;
  bool isOpenTvEpgService = false;
  if (!m_grabberSiMpeg->GetOpenTvEpgPids(serviceId,
                                          pmtPid,
                                          isOpenTvEpgService,
                                          m_openTvEpgPidsEvent,
                                          m_openTvEpgPidsDescription))
  {
    LogDebug(L"writer: OpenTV EPG service candidate identified, service ID = %hu, ONID = %hu, PMT PID = %hu",
              serviceId, originalNetworkId, pmtPid);
    m_openTvEpgServiceId = serviceId;
    if (pmtPid != 0 && m_openTvEpgPmtPid != pmtPid)
    {
      m_openTvEpgPmtPid = pmtPid;
      m_grabberEpgOpenTv->SetPmtPid(pmtPid);
    }
    return;
  }

  if (isOpenTvEpgService)
  {
    LogDebug(L"writer: OpenTV EPG service identified, service ID = %hu, ONID = %hu, PMT PID = %hu",
              serviceId, originalNetworkId, pmtPid);
    m_openTvEpgServiceId = serviceId;
    if (m_openTvEpgPmtPid != pmtPid)
    {
      m_openTvEpgPmtPid = pmtPid;
      m_grabberEpgOpenTv->SetPmtPid(pmtPid);
    }
    if (m_isOpenTvEpgServiceRunning)
    {
      if (m_openTvEpgPidsEvent.size() > 0)
      {
        m_grabberEpgOpenTv->AddEventDecoders(m_openTvEpgPidsEvent);
      }
      if (m_openTvEpgPidsDescription.size() > 0)
      {
        m_grabberEpgOpenTv->AddDescriptionDecoders(m_openTvEpgPidsDescription);
      }
    }
  }
}

void CTsWriter::OnCatReceived(const unsigned char* table, unsigned short tableSize)
{
  if (m_observer != NULL)
  {
    m_observer->OnConditionalAccessTable(table, tableSize);
  }
}

void CTsWriter::OnCatChanged(const unsigned char* table, unsigned short tableSize)
{
  if (m_observer != NULL)
  {
    m_observer->OnConditionalAccessTable(table, tableSize);
  }
}

void CTsWriter::OnEamReceived(unsigned short id,
                              unsigned long originatorCode,
                              const char* eventCode,
                              const map<unsigned long, char*>& NatureOfActivationTexts,
                              unsigned char alertMessageTimeRemaining,
                              unsigned long eventStartTime,
                              unsigned short eventDuration,
                              unsigned char alertPriority,
                              unsigned short detailsOobSourceId,
                              unsigned short detailsMajorChannelNumber,
                              unsigned short detailsMinorChannelNumber,
                              unsigned char detailsRfChannel,
                              unsigned short detailsProgramNumber,
                              unsigned short audioOobSourceId,
                              const map<unsigned long, char*>& alertTexts,
                              const vector<unsigned long>& locationCodes,
                              const vector<unsigned long>& exceptions,
                              const vector<unsigned long>& alternativeExceptions)
{
}

void CTsWriter::OnEamChanged(unsigned short id,
                              unsigned long originatorCode,
                              const char* eventCode,
                              const map<unsigned long, char*>& NatureOfActivationTexts,
                              unsigned char alertMessageTimeRemaining,
                              unsigned long eventStartTime,
                              unsigned short eventDuration,
                              unsigned char alertPriority,
                              unsigned short detailsOobSourceId,
                              unsigned short detailsMajorChannelNumber,
                              unsigned short detailsMinorChannelNumber,
                              unsigned char detailsRfChannel,
                              unsigned short detailsProgramNumber,
                              unsigned short audioOobSourceId,
                              const map<unsigned long, char*>& alertTexts,
                              const vector<unsigned long>& locationCodes,
                              const vector<unsigned long>& exceptions,
                              const vector<unsigned long>& alternativeExceptions)
{
}

void CTsWriter::OnEamRemoved(unsigned short id,
                              unsigned long originatorCode,
                              const char* eventCode,
                              const map<unsigned long, char*>& NatureOfActivationTexts,
                              unsigned char alertMessageTimeRemaining,
                              unsigned long eventStartTime,
                              unsigned short eventDuration,
                              unsigned char alertPriority,
                              unsigned short detailsOobSourceId,
                              unsigned short detailsMajorChannelNumber,
                              unsigned short detailsMinorChannelNumber,
                              unsigned char detailsRfChannel,
                              unsigned short detailsProgramNumber,
                              unsigned short audioOobSourceId,
                              const map<unsigned long, char*>& alertTexts,
                              const vector<unsigned long>& locationCodes,
                              const vector<unsigned long>& exceptions,
                              const vector<unsigned long>& alternativeExceptions)
{
}

void CTsWriter::OnMgtReceived(unsigned short tableType,
                              unsigned short pid,
                              unsigned char versionNumber,
                              unsigned long numberBytes)
{
}

void CTsWriter::OnMgtChanged(unsigned short tableType,
                              unsigned short pid,
                              unsigned char versionNumber,
                              unsigned long numberBytes)
{
}

void CTsWriter::OnMgtRemoved(unsigned short tableType,
                              unsigned short pid,
                              unsigned char versionNumber,
                              unsigned long numberBytes)
{
}

void CTsWriter::OnPatProgramReceived(unsigned short programNumber, unsigned short pmtPid)
{
  if (m_observer != NULL)
  {
    m_observer->OnProgramDetail(programNumber, pmtPid, true, NULL, 0);
  }

  if (m_openTvEpgServiceId == programNumber)
  {
    if (m_openTvEpgPmtPid == 0)
    {
      m_openTvEpgPmtPid = pmtPid;
      m_grabberEpgOpenTv->SetPmtPid(pmtPid);
    }
  }
  else if (!m_checkedIsFreesatTransportStream && m_freesatProgramNumber == 0)
  {
    m_freesatProgramNumber = programNumber;
    m_grabberEpgDvb->SetFreesatPmtPid(pmtPid);
  }
}

void CTsWriter::OnPatProgramChanged(unsigned short programNumber, unsigned short pmtPid)
{
  if (m_observer != NULL)
  {
    m_observer->OnProgramDetail(programNumber, pmtPid, true, NULL, 0);
  }

  if (m_openTvEpgServiceId == programNumber)
  {
    LogDebug(L"writer: OpenTV EPG service PMT PID changed, unsupported");
  }
}

void CTsWriter::OnPatProgramRemoved(unsigned short programNumber, unsigned short pmtPid)
{
  if (m_observer != NULL)
  {
    m_observer->OnProgramDetail(programNumber, pmtPid, false, NULL, 0);
  }

  LogDebug(L"writer: OpenTV EPG service removed");
  m_isOpenTvEpgServiceRunning = false;
  if (m_openTvEpgPidsEvent.size() > 0)
  {
    m_grabberEpgOpenTv->RemoveEventDecoders(m_openTvEpgPidsEvent);
    m_openTvEpgPidsEvent.clear();
  }
  if (m_openTvEpgPidsDescription.size() > 0)
  {
    m_grabberEpgOpenTv->RemoveDescriptionDecoders(m_openTvEpgPidsDescription);
    m_openTvEpgPidsDescription.clear();
  }
}

void CTsWriter::OnPatTsidChanged(unsigned short oldTransportStreamId,
                                  unsigned short newTransportStreamId)
{
  // Currently nothing to do.
}

void CTsWriter::OnPatNetworkPidChanged(unsigned short oldNetworkPid, unsigned short newNetworkPid)
{
  m_grabberSiDvb->SetPids(0, newNetworkPid, 0, 0);

  if (m_observer == NULL)
  {
    return;
  }

  if (oldNetworkPid != 0 && oldNetworkPid != 0xffff)
  {
    m_observer->OnPidsNotRequired(&oldNetworkPid, 1, (unsigned long)Si);
  }
  m_observer->OnPidsRequired(&newNetworkPid, 1, (unsigned long)Si);
}

void CTsWriter::OnPmtReceived(unsigned short programNumber,
                              unsigned short pid,
                              const unsigned char* table,
                              unsigned short tableSize)
{
  if (m_observer != NULL)
  {
    m_observer->OnProgramDetail(programNumber, pid, true, table, tableSize);
  }

  // We only need one PMT for the Freesat program.
  if (programNumber == m_freesatProgramNumber)
  {
    m_checkedIsFreesatTransportStream = true;
    m_freesatProgramNumber = 0;
    m_grabberEpgDvb->SetFreesatPmtPid(0);
  }

  // If this is the PMT for the OpenTV EPG service candidate, we can determine
  // whether the service is really an EPG service or not.
  if (programNumber != m_openTvEpgServiceId)
  {
    return;
  }

  bool isOpenTvEpgService = false;
  if (!m_grabberSiMpeg->GetOpenTvEpgPids(programNumber,
                                          pid,
                                          isOpenTvEpgService,
                                          m_openTvEpgPidsEvent,
                                          m_openTvEpgPidsDescription) || !isOpenTvEpgService)
  {
    LogDebug(L"writer: OpenTV EPG service candidate not valid");
    m_openTvEpgServiceId = 0;
    if (m_openTvEpgPmtPid != 0)
    {
      m_openTvEpgPmtPid = 0;
      m_grabberEpgOpenTv->SetPmtPid(0);
    }
    return;
  }

  LogDebug(L"writer: OpenTV EPG service identified, service ID = %hu, PMT PID = %hu",
            programNumber, pid);
  if (m_openTvEpgPmtPid == 0)
  {
    m_openTvEpgPmtPid = pid;
    m_grabberEpgOpenTv->SetPmtPid(pid);
  }
  if (m_isOpenTvEpgServiceRunning)
  {
    if (m_openTvEpgPidsEvent.size() > 0)
    {
      m_grabberEpgOpenTv->AddEventDecoders(m_openTvEpgPidsEvent);
    }
    if (m_openTvEpgPidsDescription.size() > 0)
    {
      m_grabberEpgOpenTv->AddDescriptionDecoders(m_openTvEpgPidsDescription);
    }
  }
}

void CTsWriter::OnPmtChanged(unsigned short programNumber,
                              unsigned short pid,
                              const unsigned char* table,
                              unsigned short tableSize)
{
  if (m_observer != NULL)
  {
    m_observer->OnProgramDetail(programNumber, pid, true, table, tableSize);
  }

  if (programNumber == m_openTvEpgServiceId)
  {
    // We can't get the new PIDs from here because this call-back is invoked
    // before the grabber's new section data is saved.
    LogDebug(L"writer: OpenTV EPG service PMT changed, unsupported");
  }
}

void CTsWriter::OnPmtRemoved(unsigned short programNumber, unsigned short pid)
{
  if (m_observer != NULL)
  {
    m_observer->OnProgramDetail(programNumber, pid, false, NULL, 0);
  }

  if (programNumber != m_openTvEpgServiceId)
  {
    return;
  }

  LogDebug(L"writer: OpenTV EPG service removed");
  m_isOpenTvEpgServiceRunning = false;
  if (m_openTvEpgPidsEvent.size() > 0)
  {
    m_grabberEpgOpenTv->RemoveEventDecoders(m_openTvEpgPidsEvent);
    m_openTvEpgPidsEvent.clear();
  }
  if (m_openTvEpgPidsDescription.size() > 0)
  {
    m_grabberEpgOpenTv->RemoveDescriptionDecoders(m_openTvEpgPidsDescription);
    m_openTvEpgPidsDescription.clear();
  }
}