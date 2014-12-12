/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.


*/

#include "stdafx.h"

#include "MPUrlSourceSplitter.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "Utilities.h"
#include "ParameterCollection.h"
#include "Parameters.h"
#include "LockMutex.h"
#include "StreamProgress.h"
#include "MPUrlSourceSplitterOutputSplitterPin.h"
#include "MPUrlSourceSplitterOutputDownloadPin.h"
#include "MPUrlSourceSplitterOutputM2tsMuxerPin.h"
#include "MPUrlSourceSplitter_Parser_MPEG2TS_Parameters.h"
#include "CurlInstance.h"
#include "conversions.h"
#include "ErrorMessages.h"

#include "DefaultDemuxer.h"
#include "StandardDemuxer.h"
#include "ContainerDemuxer.h"
#include "PacketDemuxer.h"

#include <crtdbg.h>
#include <process.h>

extern "C"
{
#include "config.h"
#include "..\..\ffmpeg\version.h"
#if CONFIG_AVFILTER
#include "libavfilter\version.h"
#include "libavfilter\avfilter.h"
#endif
#if CONFIG_SWSCALE
#include "libswscale\swscale.h"
#endif
#if CONFIG_AVDEVICE
#include "libavdevice\avdevice.h"
#endif
#if CONFIG_SWRESAMPLE
#include "libswresample\swresample.h"
#endif
#if CONFIG_POSTPROC
#include "libpostproc\postprocess.h"
#endif
}

#include <Shlwapi.h>

#ifdef _DEBUG
#define MODULE_NAME                                               L"MPUrlSourceSplitterd"
#else
#define MODULE_NAME                                               L"MPUrlSourceSplitter"
#endif

#define METHOD_LOAD_NAME                                          L"Load()"
#define METHOD_LOAD_ASYNC_NAME                                    L"LoadAsync()"
#define METHOD_THREAD_PROC_NAME                                   L"ThreadProc()"

#define METHOD_STOP_NAME                                          L"Stop()"
#define METHOD_PAUSE_NAME                                         L"Pause()"
#define METHOD_RUN_NAME                                           L"Run()"

#define METHOD_SET_POSITIONS_NAME                                 L"SetPositions()"

#define METHOD_PARSE_PARAMETERS_NAME                              L"ParseParameters()"
#define METHOD_LOAD_PLUGINS_NAME                                  L"LoadPlugins()"
#define METHOD_SET_TOTAL_LENGTH_NAME                              L"SetTotalLength()"

#define METHOD_LENGTH_NAME                                        L"Length()"

#define METHOD_DOWNLOAD_NAME                                      L"Download()"
#define METHOD_DOWNLOAD_ASYNC_NAME                                L"DownloadAsync()"
#define METHOD_DOWNLOAD_CALLBACK_NAME                             L"OnDownloadCallback()"

#define METHOD_ENABLE_NAME                                        L"Enable()"

#define METHOD_CREATE_LOAD_ASYNC_WORKER_NAME                      L"CreateLoadAsyncWorker()"
#define METHOD_DESTROY_LOAD_ASYNC_WORKER_NAME                     L"DestroyLoadAsyncWorker()"
#define METHOD_LOAD_ASYNC_WORKER_NAME                             L"LoadAsyncWorker()"

#define PARAMETER_SEPARATOR                                       L"&"
#define PARAMETER_IDENTIFIER                                      L"####"
#define PARAMETER_ASSIGN                                          L"="

// stock filter backward compatibility
#define PARAMETER_SEPARATOR_STOCK_FILTER                          L"|"
#define PARAMETER_ASSIGN_STOCK_FILTER                             L"="

// stock filter supported parameters
#define PARAMETER_NAME_STOCK_FILTER_MPEG2TS_PROGRAM_NUMBER        L"SidValue"
#define PARAMETER_NAME_STOCK_FILTER_MPEG2TS_PROGRAM_MAP_PID       L"PidValue"
#define PARAMETER_NAME_STOCK_FILTER_INTERFACE                     L"interface"
#define PARAMETER_NAME_STOCK_FILTER_URL                           L"url"

#define PARAMETER_NAME_STOCK_FILTER_TOTAL_SUPPORTED               4
const wchar_t *SUPPORTED_PARAMETER_NAME_STOCK_FILTER[PARAMETER_NAME_STOCK_FILTER_TOTAL_SUPPORTED] = {
                                                                  PARAMETER_NAME_STOCK_FILTER_MPEG2TS_PROGRAM_NUMBER,
                                                                  PARAMETER_NAME_STOCK_FILTER_MPEG2TS_PROGRAM_MAP_PID,
                                                                  PARAMETER_NAME_STOCK_FILTER_INTERFACE,
                                                                  PARAMETER_NAME_STOCK_FILTER_URL };

const wchar_t *REPLACE_PARAMETER_NAME_STOCK_FILTER[PARAMETER_NAME_STOCK_FILTER_TOTAL_SUPPORTED] = {
                                                                  PARAMETER_NAME_MPEG2TS_PROGRAM_NUMBER,
                                                                  PARAMETER_NAME_MPEG2TS_PROGRAM_MAP_PID,
                                                                  PARAMETER_NAME_INTERFACE,
                                                                  PARAMETER_NAME_URL};

#define UNIX_TIMESTAMP_2000_01_01                                 946684800
#define SECONDS_IN_DAY                                            86400

extern "C++" CStaticLogger *staticLogger;

#define SHOW_VERSION                                              2
#define SHOW_CONFIG                                               4

#define GET_LIB_INFO(libInfo, libname, LIBNAME, flags)                                        \
{                                                                                             \
    libInfo = NULL;                                                                           \
    if (CONFIG_##LIBNAME)                                                                     \
    {                                                                                         \
        char *versionStringA = NULL;                                                          \
        char *configurationStringA = NULL;                                                    \
        if (flags & SHOW_VERSION)                                                             \
        {                                                                                     \
            unsigned int version = libname##_version();                                       \
            versionStringA = FormatStringA("lib%-11s %2d.%3d.%3d / %2d.%3d.%3d",              \
                   #libname,                                                                  \
                   LIB##LIBNAME##_VERSION_MAJOR,                                              \
                   LIB##LIBNAME##_VERSION_MINOR,                                              \
                   LIB##LIBNAME##_VERSION_MICRO,                                              \
                   version >> 16, version >> 8 & 0xff, version & 0xff);                       \
        }                                                                                     \
        if (flags & SHOW_CONFIG)                                                              \
        {                                                                                     \
            const char *cfg = libname##_configuration();                                      \
            if (strcmp(FFMPEG_CONFIGURATION, cfg))                                            \
            {                                                                                 \
                configurationStringA = FormatStringA("%-11s configuration: %s",               \
                        #libname, cfg);                                                       \
            }                                                                                 \
        }                                                                                     \
        wchar_t *versionStringW = ConvertToUnicodeA(versionStringA);                          \
        wchar_t *configurationStringW = ConvertToUnicodeA(configurationStringA);              \
        FREE_MEM(versionStringA);                                                             \
        FREE_MEM(configurationStringA);                                                       \
        if ((versionStringW == NULL) && (configurationStringW == NULL))                       \
        {                                                                                     \
            libInfo = NULL;                                                                   \
        }                                                                                     \
        else if (versionStringW == NULL)                                                      \
        {                                                                                     \
            libInfo = Duplicate(configurationStringW);                                        \
        }                                                                                     \
        else if (configurationStringW == NULL)                                                \
        {                                                                                     \
            libInfo = Duplicate(versionStringW);                                              \
        }                                                                                     \
        else                                                                                  \
        {                                                                                     \
            libInfo = FormatStringW(L"%s %s", versionStringW, configurationStringW);          \
        }                                                                                     \
        FREE_MEM(versionStringW);                                                             \
        FREE_MEM(configurationStringW);                                                       \
    }                                                                                         \
}

CMPUrlSourceSplitter::CMPUrlSourceSplitter(LPCSTR pName, LPUNKNOWN pUnk, const IID &clsid, HRESULT *phr)
#pragma warning(push)
#pragma warning(disable:4355)
  // supressed warning 'warning C4355: 'this' : used in base member initializer list'
  // this is not problem, because in CBaseFilter constructor is not called any method of CMPUrlSourceSplitter object
  // only reference is stored to CBaseFilter m_pLock member
  : CBaseFilter(pName, pUnk, this, clsid, phr), CFlags()
#pragma warning(pop)
  , lastCommand(-1)
  , pauseSeekStopRequest(false)
  , logger(NULL)
  , outputPins(NULL)
  , demuxers(NULL)
  , demuxersMutex(NULL)
  , asyncDownloadResult(S_OK)
  , asyncDownloadCallback(NULL)
  , downloadFileName(NULL)
  , demuxStart(0)
  , demuxNewStart(0)
  , loadAsyncWorkerThread(NULL)
  , loadAsyncWorkerShouldExit(false)
  , loadAsyncResult(S_OK)
{
  CParameterCollection *loggerParameters = new CParameterCollection(phr);
  CHECK_POINTER_HRESULT(*phr, loggerParameters, *phr, E_OUTOFMEMORY);

  if (SUCCEEDED(*phr))
  {
#ifdef _DEBUG
    // log file parameter doesn't exist, add default
    wchar_t *logFile = (IsEqualGUID(GUID_MP_IPTV_SOURCE, clsid) != 0) ? GetTvServerFilePath(MP_IPTV_SOURCE_LOG_FILE) : GetMediaPortalFilePath(MP_URL_SOURCE_SPLITTER_LOG_FILE);
    CHECK_POINTER_HRESULT(*phr, logFile, *phr, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*phr, loggerParameters->Add(PARAMETER_NAME_LOG_FILE_NAME, logFile), *phr, E_OUTOFMEMORY);
    FREE_MEM(logFile);
#endif

    this->logger = new CLogger(phr, staticLogger, loggerParameters);
    CHECK_POINTER_HRESULT(*phr, this->logger, *phr, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(*phr))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER, DATE_INFO_MP_URL_SOURCE_SPLITTER);
    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, version);
    FREE_MEM(version);

    wchar_t *result = NULL;

#if CONFIG_AVUTIL
    GET_LIB_INFO(result, avutil,   AVUTIL,   SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif
#if CONFIG_AVCODEC
    GET_LIB_INFO(result, avcodec,  AVCODEC,  SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif
#if CONFIG_AVFORMAT
    GET_LIB_INFO(result, avformat, AVFORMAT, SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif
#if CONFIG_AVDEVICE
    GET_LIB_INFO(result, avdevice, AVDEVICE, SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif
#if CONFIG_AVFILTER
    GET_LIB_INFO(result, avfilter, AVFILTER, SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif
#if CONFIG_SWSCALE
    GET_LIB_INFO(result, swscale,  SWSCALE,  SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif
#if CONFIG_SWRESAMPLE
    GET_LIB_INFO(result, swresample,SWRESAMPLE,  SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif
#if CONFIG_POSTPROC
    GET_LIB_INFO(result, postproc, POSTPROC, SHOW_VERSION);
    if (result != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, result);
    }
    FREE_MEM(result);
#endif

    this->outputPins = new CMPUrlSourceSplitterOutputPinCollection(phr);
    CHECK_POINTER_HRESULT(*phr, this->outputPins, *phr, E_OUTOFMEMORY);

    if (SUCCEEDED(*phr))
    {
      this->configuration = new CParameterCollection(phr);
      CHECK_POINTER_HRESULT(*phr, this->configuration, *phr, E_OUTOFMEMORY);

      this->parserHoster = new CParserHoster(phr, this->logger, loggerParameters);
      CHECK_POINTER_HRESULT(*phr, this->parserHoster, *phr, E_OUTOFMEMORY);

      this->demuxers = new CDemuxerCollection(phr);
      CHECK_POINTER_HRESULT(*phr, this->demuxers, *phr, E_OUTOFMEMORY);

      this->demuxersMutex = CreateMutex(NULL, FALSE, NULL);
      CHECK_POINTER_HRESULT(*phr, this->demuxersMutex != NULL, *phr, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(*phr), this->parserHoster->LoadPlugins(), *phr);
    }

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
  }

  FREE_MEM_CLASS(loggerParameters);
}

CMPUrlSourceSplitter::~CMPUrlSourceSplitter()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);

  // reset all internal properties to default values
  this->ClearSession(false);

  FREE_MEM_CLASS(this->outputPins);
  FREE_MEM_CLASS(this->parserHoster);
  FREE_MEM_CLASS(this->demuxers);

  if (this->demuxersMutex != NULL)
  {
    CloseHandle(this->demuxersMutex);
    this->demuxersMutex = NULL;
  }

  FREE_MEM(this->downloadFileName);
  FREE_MEM_CLASS(this->configuration);

  this->logger->Log(LOGGER_INFO, L"%s: %s: instance reference count: %u", MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_cRef);
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);

  FREE_MEM_CLASS(this->logger);
}

CUnknown * WINAPI CMPUrlSourceSplitter::CreateInstanceIptvSource(LPUNKNOWN lpunk, HRESULT *phr)
{
  *phr = S_OK;
  CMPUrlSourceSplitter *instance = new CMPUrlSourceSplitter("MediaPortal IPTV Source Filter", lpunk, GUID_MP_IPTV_SOURCE, phr);
  CHECK_POINTER_HRESULT(*phr, instance, *phr, E_OUTOFMEMORY);

  if (SUCCEEDED(*phr))
  {
    instance->flags |= MP_URL_SOURCE_SPLITTER_FLAG_AS_IPTV;

    // if in output pin collection isn't any pin, then add new output pin with MPEG2 TS media type
    // in another case filter assume that there is only one output pin with MPEG2 TS media type
    if (instance->outputPins->Count() == 0)
    {
      // create valid MPEG2 TS media type, add it to media types and create output pin
      CMediaTypeCollection *mediaTypes = new CMediaTypeCollection(phr);
      CHECK_POINTER_HRESULT(*phr, mediaTypes, *phr, E_OUTOFMEMORY);

      if (SUCCEEDED(*phr))
      {
        CMediaType *mediaType = new CMediaType();
        CHECK_POINTER_HRESULT(*phr, mediaType, *phr, E_OUTOFMEMORY);

        if (SUCCEEDED(*phr))
        {
          mediaType->SetType(&MEDIATYPE_Stream);
          mediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);

          CHECK_CONDITION_HRESULT(*phr, mediaTypes->Add(mediaType), *phr, E_OUTOFMEMORY);
        }

        CHECK_CONDITION_EXECUTE(FAILED(*phr), FREE_MEM_CLASS(mediaType));
      }

      if (SUCCEEDED(*phr))
      {
        CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputM2tsMuxerPin(L"Output", instance, instance, phr, instance->logger, instance->configuration, mediaTypes);
        CHECK_POINTER_HRESULT(*phr, outputPin, *phr, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(*phr, instance->outputPins->Add(outputPin), *phr, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(*phr), FREE_MEM_CLASS(outputPin));
      }

      FREE_MEM_CLASS(mediaTypes);
    }
  }

  return instance;
}

CUnknown * WINAPI CMPUrlSourceSplitter::CreateInstanceUrlSourceSplitter(LPUNKNOWN lpunk, HRESULT* phr)
{
  *phr = S_OK;
  CMPUrlSourceSplitter *punk = new CMPUrlSourceSplitter("MediaPortal Url Source Splitter", lpunk, GUID_MP_URL_SOURCE_SPLITTER, phr);
  CHECK_POINTER_HRESULT(*phr, punk, *phr, E_OUTOFMEMORY);

  if (SUCCEEDED(*phr))
  {
    punk->flags |= MP_URL_SOURCE_SPLITTER_FLAG_AS_SPLITTER;
  }

  return punk;
}

// IUnknown

STDMETHODIMP CMPUrlSourceSplitter::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

  *ppv = NULL;

  if (this->IsIptv())
  {
    return
      QI(IFileSourceFilter)
      QI(IFilterState)
      QI(IFilterStateEx)
      __super::NonDelegatingQueryInterface(riid, ppv);
  }
  else if (this->IsSplitter())
  {
    return
      QI(IFileSourceFilter)
      QI(IMediaSeeking)
      QI(IAMStreamSelect)
      QI(IAMOpenProgress)
      QI(IDownload)
      QI(IFilterState)
      QI(IFilterStateEx)
      __super::NonDelegatingQueryInterface(riid, ppv);
  }

  return __super::NonDelegatingQueryInterface(riid, ppv);
}

// CBaseFilter

int CMPUrlSourceSplitter::GetPinCount()
{
  return (this->outputPins == NULL) ? 0 : (int)this->outputPins->Count();
}

CBasePin *CMPUrlSourceSplitter::GetPin(int n)
{
  CBasePin *result = NULL;

  if ((n >= 0) && (n <= this->GetPinCount()))
  {
    result = this->outputPins->GetItem((unsigned int)n);
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::GetClassID(CLSID* pClsID)
{
  CheckPointer (pClsID, E_POINTER);

  /*if (m_bFakeASFReader)
  {
    *pClsID = CLSID_WMAsfReader;
    return S_OK;
  }
  else
  {
    return __super::GetClassID(pClsID);
  }*/

  return __super::GetClassID(pClsID);
}

STDMETHODIMP CMPUrlSourceSplitter::GetState(DWORD dwMSecs, __out FILTER_STATE *State)
{
  CheckPointer (State, E_POINTER);

  HRESULT result = __super::GetState(dwMSecs, State);
  result = (SUCCEEDED(result) && (this->m_State == State_Paused)) ? VFW_S_CANT_CUE : result;

  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::Stop()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_STOP_NAME);

  this->flags &= ~(MP_URL_SOURCE_SPLITTER_FLAG_PLAYBACK_STARTED | MP_URL_SOURCE_SPLITTER_FLAG_REPORT_STREAM_TIME);

  this->SetPauseSeekStopRequest(true);
  CAMThread::CallWorker(CMD_EXIT);

  this->DeliverBeginFlush();
  CAMThread::Close();
  this->DeliverEndFlush();

  if (!this->IsEnabledMethodActive())
  {
    // stop async loading
    this->DestroyLoadAsyncWorker();
    // clear all demuxers
    this->demuxers->Clear();
    // if we are not changing streams stop receiving data, data are not needed
    this->parserHoster->StopReceivingData();
    // clear also session, it will no longer be needed
    this->parserHoster->ClearSession();
  }

  HRESULT result = __super::Stop();

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_STOP_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::Pause()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_PAUSE_NAME);

  this->flags &= ~MP_URL_SOURCE_SPLITTER_FLAG_REPORT_STREAM_TIME;

  this->SetPauseSeekStopRequest(true);
  CAMThread::CallWorker(CMD_PAUSE);

  CAutoLock cAutoLock(this);

  FILTER_STATE fs = m_State;
  HRESULT result = __super::Pause();

  // the filter graph will set us to pause before running
  // so if we were stopped before, create demuxing thread
  if (SUCCEEDED(result) && (fs == State_Stopped))
  {
    // create demuxing thread
    result = CAMThread::Create() ? result : E_FAIL;
  }

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_PAUSE_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::Run(REFERENCE_TIME tStart)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_RUN_NAME);

  CAutoLock cAutoLock(this);

  HRESULT result = __super::Run(tStart);

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), CAMThread::CallWorker(CMD_PLAY));
  this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_PLAYBACK_STARTED | MP_URL_SOURCE_SPLITTER_FLAG_REPORT_STREAM_TIME;

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_RUN_NAME, result);
  return S_OK;
}

// IFileSourceFilter

STDMETHODIMP CMPUrlSourceSplitter::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE * pmt)
{
  // reset all internal properties to default values
  this->ClearSession(true);

  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_LOAD_NAME);
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, pszFileName);

  if (SUCCEEDED(result))
  {
    wchar_t *url = ConvertToUnicodeW(pszFileName);
    CHECK_POINTER_HRESULT(result, url, result, E_CONVERT_STRING_ERROR);

    if (SUCCEEDED(result))
    {
      CParameterCollection *suppliedParameters = this->ParseParameters(url);
      if (suppliedParameters != NULL)
      {
        // we have set some parameters
        // set them as configuration parameters
        this->configuration->Clear();
        this->configuration->Append(suppliedParameters);
        if (!this->configuration->Contains(PARAMETER_NAME_URL, true))
        {
          result = this->configuration->Add(PARAMETER_NAME_URL, url) ? result : E_OUTOFMEMORY;
        }

        FREE_MEM_CLASS(suppliedParameters);
      }
      else
      {
        // parameters are not supplied, just set current url as only one parameter in configuration
        this->configuration->Clear();
        result = this->configuration->Add(PARAMETER_NAME_URL, url) ? result : E_OUTOFMEMORY;
      }
    }

    if (SUCCEEDED(result))
    {
      do
      {
        result = this->LoadAsync();

        if (result == S_FALSE)
        {
          Sleep(1);
        }
      }
      while (result == S_FALSE);
    }

    FREE_MEM(url);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LOAD_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::GetCurFile(LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt)
{
  if (!ppszFileName)
  {
    return E_POINTER;
  }

  *ppszFileName = ConvertToUnicode(this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL));
  if ((*ppszFileName) == NULL)
  {
    return E_CONVERT_STRING_ERROR;
  }

  return S_OK;
}

// IMediaSeeking

STDMETHODIMP CMPUrlSourceSplitter::GetCapabilities(DWORD* pCapabilities)
{
  CheckPointer(pCapabilities, E_POINTER);

  *pCapabilities =
    AM_SEEKING_CanGetStopPos   |
    AM_SEEKING_CanGetDuration  |
    AM_SEEKING_CanSeekAbsolute |
    AM_SEEKING_CanSeekForwards |
    AM_SEEKING_CanSeekBackwards;

  return S_OK;
}

STDMETHODIMP CMPUrlSourceSplitter::CheckCapabilities(DWORD* pCapabilities)
{
  CheckPointer(pCapabilities, E_POINTER);
  // capabilities is empty, all is good
  if(*pCapabilities == 0) return S_OK;
  // read caps
  DWORD caps;
  GetCapabilities(&caps);

  // Store the caps that we wanted
  DWORD wantCaps = *pCapabilities;
  // Update pCapabilities with what we have
  *pCapabilities = caps & wantCaps;

  // if nothing matches, its a disaster!
  if(*pCapabilities == 0) return E_FAIL;
  // if all matches, its all good
  if(*pCapabilities == wantCaps) return S_OK;
  // otherwise, a partial match
  return S_FALSE;
}

STDMETHODIMP CMPUrlSourceSplitter::IsFormatSupported(const GUID* pFormat)
{
  return !pFormat ? E_POINTER : *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

STDMETHODIMP CMPUrlSourceSplitter::QueryPreferredFormat(GUID* pFormat)
{
  return this->GetTimeFormat(pFormat);
}

STDMETHODIMP CMPUrlSourceSplitter::GetTimeFormat(GUID* pFormat)
{
  return pFormat ? *pFormat = TIME_FORMAT_MEDIA_TIME, S_OK : E_POINTER;
}

STDMETHODIMP CMPUrlSourceSplitter::IsUsingTimeFormat(const GUID* pFormat)
{
  return this->IsFormatSupported(pFormat);
}

STDMETHODIMP CMPUrlSourceSplitter::SetTimeFormat(const GUID* pFormat)
{
  return SUCCEEDED(this->IsFormatSupported(pFormat)) ? S_OK : E_INVALIDARG;
}

STDMETHODIMP CMPUrlSourceSplitter::GetDuration(LONGLONG* pDuration)
{
  CheckPointer(pDuration, E_POINTER);
  CheckPointer(this->demuxers, E_UNEXPECTED);

  *pDuration = -1;

  CLockMutex lock(this->demuxersMutex, INFINITE);

  for (unsigned int i = 0; i < this->demuxers->Count(); i++)
  {
    CDemuxer *demuxer = this->demuxers->GetItem(i);

    *pDuration = max(*pDuration, demuxer->GetDuration());
  }
  
  return (*pDuration < 0) ? E_FAIL : S_OK;
}

STDMETHODIMP CMPUrlSourceSplitter::GetStopPosition(LONGLONG* pStop)
{
  return this->GetDuration(pStop);
}

STDMETHODIMP CMPUrlSourceSplitter::GetCurrentPosition(LONGLONG* pCurrent)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitter::ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitter::SetPositions(LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_SET_POSITIONS_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek request, start: %I64d, flags: 0x%08X, stop: %I64d, flags: 0x%08X", MODULE_NAME, METHOD_SET_POSITIONS_NAME, pCurrent ? *pCurrent : -1, dwCurrentFlags, pStop ? *pStop : -1, dwStopFlags);

  HRESULT result = E_FAIL;

  if (((pCurrent == NULL) && (pStop == NULL)) ||
    ((dwCurrentFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_NoPositioning))
  {
    result = S_OK;
  }
  else if ((pCurrent != NULL) && ((dwCurrentFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_AbsolutePositioning))
  {
    this->demuxNewStart = *pCurrent;

    // perform seek
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: performing seek to %I64d", MODULE_NAME, METHOD_SET_POSITIONS_NAME, this->demuxNewStart);

    if (ThreadExists())
    {
      this->DeliverBeginFlush();
      this->SetPauseSeekStopRequest(true);
      CallWorker(CMD_SEEK);
      this->DeliverEndFlush();
    }

    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to %I64d finished", MODULE_NAME, METHOD_SET_POSITIONS_NAME, this->demuxNewStart);
    result = S_OK;
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SET_POSITIONS_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::GetPositions(LONGLONG* pCurrent, LONGLONG* pStop)
{
  /*if (pCurrent)
  {
    *pCurrent = this->demuxCurrent;
  }
  if (pStop)
  {
    *pStop = this->demuxStop;
  }
  return S_OK;*/

  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitter::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  if (pEarliest)
  {
    *pEarliest = 0;
  }

  return this->GetDuration(pLatest);
}

STDMETHODIMP CMPUrlSourceSplitter::SetRate(double dRate)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitter::GetRate(double* pdRate)
{
  return (pdRate != NULL) ? *pdRate = 1.0, S_OK : E_POINTER;
}

STDMETHODIMP CMPUrlSourceSplitter::GetPreroll(LONGLONG* pllPreroll)
{
  return pllPreroll ? *pllPreroll = 0, S_OK : E_POINTER;
}

// IAMStreamSelect

STDMETHODIMP CMPUrlSourceSplitter::Count(DWORD *pcStreams)
{
  CheckPointer(pcStreams, E_POINTER);
  CheckPointer(this->demuxers, E_UNEXPECTED);

  *pcStreams = 0;

  CLockMutex lock(this->demuxersMutex, INFINITE);

  for (unsigned int i = 0; i < this->demuxers->Count(); i++)
  {
    CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(this->demuxers->GetItem(i));

    for (unsigned int j = 0; ((demuxer != NULL) && (j < CStream::Unknown)); j++)
    {
      *pcStreams += (DWORD)demuxer->GetStreams((CStream::StreamType)j)->Count();
    }
  }

  return S_OK;
}

STDMETHODIMP CMPUrlSourceSplitter::Enable(long lIndex, DWORD dwFlags)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, this->demuxers, result, E_UNEXPECTED);
  CHECK_CONDITION_HRESULT(result, (dwFlags & AMSTREAMSELECTENABLE_ENABLE) != 0, result, E_NOTIMPL);

  if (SUCCEEDED(result))
  {
    // stream index and stream from demuxer
    unsigned int targetGroup = UINT_MAX;
    unsigned int targetIndex = UINT_MAX;
    CStandardDemuxer *targetDemuxer = NULL;
    CStream *targetStream = NULL;

    int k = 0;
    for (unsigned int i = 0; ((targetStream == NULL) && (i < this->demuxers->Count())); i++)
    {
      CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(this->demuxers->GetItem(i));

      for (unsigned int j = 0; ((targetStream == NULL) && (j < CStream::Unknown)); j++)
      {
        CStreamCollection *streams = demuxer->GetStreams((CStream::StreamType)j);
        int count = (int)streams->Count();

        if ((lIndex >= k) && (lIndex < (k + count)))
        {
          targetIndex = (unsigned int)(lIndex - k);
          targetGroup = j;
          targetStream = streams->GetItem(targetIndex);
          targetDemuxer = demuxer;
        }

        k += count;
      }
    }
    CHECK_POINTER_HRESULT(result, targetStream, result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      // for each group (video, audio, subpic, unknown) is allowed only one active stream
      // go through each stream in found group and enable requested stream

      CMPUrlSourceSplitterOutputPin *groupOutputPin = NULL;
      CStandardDemuxer *groupDemuxer = NULL;
      CStream *groupStream = NULL;

      // find stream from group of streams which is in output pin (it can be only one stream)
      for (unsigned int i = 0; ((groupOutputPin == NULL) && (i < this->outputPins->Count())); i++)
      {
        CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

        CStandardDemuxer *outputPinDemuxer = dynamic_cast<CStandardDemuxer *>(this->demuxers->GetItem(outputPin->GetDemuxerId()));
        CStreamCollection *outputPinDemuxerStreams = outputPinDemuxer->GetStreams((CStream::StreamType)targetGroup);

        for (unsigned int j = 0; ((groupOutputPin == NULL) && (j < outputPinDemuxerStreams->Count())); j++)
        {
          CStream *outputPinDemuxerStream = outputPinDemuxerStreams->GetItem(j);

          if (outputPin->GetStreamPid() == outputPinDemuxerStream->GetPid())
          {
            groupOutputPin = outputPin;
            groupStream = outputPinDemuxerStream;
            groupDemuxer = outputPinDemuxer;
          }
        }
      }
      CHECK_POINTER_HRESULT(result, groupOutputPin, result, E_INVALIDARG);

      if (SUCCEEDED(result))
      {
        if ((targetDemuxer->GetDemuxerId() != groupDemuxer->GetDemuxerId()) || (targetStream->GetPid() != groupStream->GetPid()))
        {
          // the streams are not same, we need to exchange groupStream with targetStream

          this->logger->Log(LOGGER_INFO, L"%s: %s: changing output pin '%s', from '%s' to '%s'", MODULE_NAME, METHOD_ENABLE_NAME, groupOutputPin->Name(), groupStream->GetStreamInfo()->GetStreamDescription(), targetStream->GetStreamInfo()->GetStreamDescription());

          if (groupOutputPin->IsConnected())
          {
            IMediaControl *mediaControl = NULL;
            result = this->m_pGraph->QueryInterface(IID_IMediaControl, (void **)&mediaControl);

            CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot get IMediaControl interface, result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, result));

            this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_ENABLED_METHOD_ACTIVE;

            if (SUCCEEDED(result))
            {
              FILTER_STATE oldState;

              // get the graph state
              // if the graph is in transition, we'll get the next state, not the previous
              result = mediaControl->GetState(10, (OAFilterState *)&oldState);
              CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot get graph state, result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, result));

              // stop the filter graph
              result = mediaControl->Stop();
              CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot stop graph, result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, result));

              if (SUCCEEDED(result))
              {
                // update output pin
                groupOutputPin->SetDemuxerId(targetDemuxer->GetDemuxerId());
                groupOutputPin->SetStreamPid(targetStream->GetPid());
                groupDemuxer->SetActiveStream((CStream::StreamType)targetGroup, ACTIVE_STREAM_NOT_SPECIFIED);

                targetDemuxer->SetActiveStream((CStream::StreamType)targetGroup, targetStream->GetPid());
                result = groupOutputPin->SetNewMediaTypes(targetStream->GetStreamInfo()->GetMediaTypes()) ? result : E_FAIL;
              }

              if (SUCCEEDED(result))
              {
                // audio filters get their connected filter removed
                // this way we make sure that we reconnect to the proper filter
                // other filters just disconnect and try to reconnect later on
                PIN_INFO connectedPinInfo;
                result = groupOutputPin->GetConnected()->QueryPinInfo(&connectedPinInfo);
                CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot query pin info, result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, result));

                if (SUCCEEDED(result))
                {
                  int mediaTypeIndex = -1;
                  for (unsigned int i = 0; i < targetStream->GetStreamInfo()->GetMediaTypes()->Count(); i++)
                  {
                    CMediaType *mediaType = targetStream->GetStreamInfo()->GetMediaTypes()->GetItem(i);

                    if (SUCCEEDED(groupOutputPin->GetConnected()->QueryAccept(mediaType)))
                    {
                      mediaTypeIndex = i;
                      break;
                    }
                  }

                  bool mediaTypeFound = (mediaTypeIndex >= 0);
                  if (!mediaTypeFound)
                  {
                    this->logger->Log(LOGGER_WARNING, L"%s: %s: output pin '%s' does not accept new media types", MODULE_NAME, METHOD_ENABLE_NAME, groupOutputPin->Name());
                    // fallback media type
                    mediaTypeIndex = 0;
                  }

                  CMediaType *mediaType = targetStream->GetStreamInfo()->GetMediaTypes()->GetItem(mediaTypeIndex);
                  CHECK_POINTER_HRESULT(result, mediaType, result, E_FAIL);

                  if (SUCCEEDED(result) && (targetGroup != CStream::Video) && (connectedPinInfo.pFilter != NULL))
                  {
                    bool removeFilter = !mediaTypeFound;

                    if (removeFilter && (targetGroup == CStream::Audio))
                    {
                      result = this->m_pGraph->RemoveFilter(connectedPinInfo.pFilter);

                      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot remove audio filter from graph, result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, result));

                      if (SUCCEEDED(result))
                      {
                        // use IGraphBuilder to rebuild the graph
                        IGraphBuilder *graphBuilder = NULL;
                        result = this->m_pGraph->QueryInterface(__uuidof(IGraphBuilder), (void **)&graphBuilder);
                        CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot get graph builder, result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, result));

                        if(SUCCEEDED(result))
                        {
                          // instruct the graph builder to connect us again
                          result = graphBuilder->Render(groupOutputPin);
                          CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot render output pin '%s', result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, groupOutputPin->Name(), result));
                        }

                        COM_SAFE_RELEASE(graphBuilder);
                      }
                    }
                    else
                    {
                      result = ReconnectPin(groupOutputPin, mediaType);
                      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot reconnect output pin '%s', result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, groupOutputPin->Name(), result));
                    }
                  }
                  else if (SUCCEEDED(result))
                  {
                    result = groupOutputPin->SendMediaType(mediaType);
                    CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot send new media type from output pin '%s', result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, groupOutputPin->Name(), result));
                  }

                  COM_SAFE_RELEASE(connectedPinInfo.pFilter);
                }
              }

              if (SUCCEEDED(result))
              {
                // re-start the graph
                if (oldState == State_Paused)
                {
                  result = mediaControl->Pause();
                }
                else if (oldState == State_Running)
                {
                  result = mediaControl->Run();
                }

                CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = S_OK);
              }
            }

            COM_SAFE_RELEASE(mediaControl);

            this->flags &= ~MP_URL_SOURCE_SPLITTER_FLAG_ENABLED_METHOD_ACTIVE;
          }
          else
          {
            // in normal operation, this won't make much sense
            // however, in graphstudio it is now possible to change the stream before connecting

            groupOutputPin->SetDemuxerId(targetDemuxer->GetDemuxerId());
            groupOutputPin->SetStreamPid(targetStream->GetPid());
            groupDemuxer->SetActiveStream((CStream::StreamType)targetGroup, ACTIVE_STREAM_NOT_SPECIFIED);

            targetDemuxer->SetActiveStream((CStream::StreamType)targetGroup, targetStream->GetPid());
            result = groupOutputPin->SetNewMediaTypes(targetStream->GetStreamInfo()->GetMediaTypes()) ? result : E_FAIL;
          }
        }
      }
    }
  }

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_ENABLE_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::Info(long lIndex, AM_MEDIA_TYPE **ppmt, DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk)
{
  // S_FALSE as return value means that index is out of range
  HRESULT result = S_FALSE;
  CHECK_POINTER_HRESULT(result, this->demuxers, result, E_UNEXPECTED);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->demuxersMutex, INFINITE);
    int k = 0;

    for (unsigned int i = 0; ((result == S_FALSE) && (i < this->demuxers->Count())); i++)
    {
      CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(this->demuxers->GetItem(i));

      for (unsigned int j = 0; ((demuxer != NULL) && (result == S_FALSE) && (j < CStream::Unknown)); j++)
      {
        CStreamCollection *streams = demuxer->GetStreams((CStream::StreamType)j);
        int count = (int)streams->Count();

        if ((lIndex >= k) && (lIndex < (k + count)))
        {
          unsigned int index = (unsigned int)(lIndex - k);

          CStream *stream = streams->GetItem(index);

          CHECK_CONDITION_NOT_NULL_EXECUTE(ppmt, *ppmt = CreateMediaType(stream->GetStreamInfo()->GetMediaTypes()->GetItem(0)));
          CHECK_CONDITION_NOT_NULL_EXECUTE(pdwFlags, *pdwFlags = 0);

          for (unsigned int m = 0; ((pdwFlags != NULL) && (m < this->outputPins->Count())); m++)
          {
            CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(m);

            if ((outputPin->GetDemuxerId() == i) && (outputPin->GetStreamPid() == stream->GetPid()))
            {
              *pdwFlags = AMSTREAMSELECTINFO_ENABLED | AMSTREAMSELECTINFO_EXCLUSIVE;
              break;
            }
          }

          CHECK_CONDITION_NOT_NULL_EXECUTE(pdwGroup, *pdwGroup = j);
          CHECK_CONDITION_NOT_NULL_EXECUTE(ppObject, *ppObject = NULL);
          CHECK_CONDITION_NOT_NULL_EXECUTE(ppUnk, *ppUnk = NULL);
          CHECK_CONDITION_NOT_NULL_EXECUTE(ppszName, *ppszName = Duplicate(stream->GetStreamInfo()->GetStreamDescription()));
          result = S_OK;
        }

        k += count;
      }
    }
  }

  return result;
}

// IAMOpenProgress

STDMETHODIMP CMPUrlSourceSplitter::QueryProgress(LONGLONG *pllTotal, LONGLONG *pllCurrent)
{
  HRESULT result = (this->parserHoster != NULL) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    CStreamProgress *streamProgress = new CStreamProgress();
    CHECK_POINTER_HRESULT(result, streamProgress, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      result = this->parserHoster->QueryStreamProgress(streamProgress);

      if (SUCCEEDED(result))
      {
        *pllTotal = streamProgress->GetTotalLength();
        *pllCurrent = streamProgress->GetCurrentLength();
      }
    }

    FREE_MEM_CLASS(streamProgress);
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::AbortOperation(void)
{
  this->ClearSession(false);

  return S_OK;
}

// IDownload

STDMETHODIMP CMPUrlSourceSplitter::Download(LPCOLESTR uri, LPCOLESTR fileName)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_NAME);

  result = this->DownloadAsync(uri, fileName, this);

  if (SUCCEEDED(result))
  {
    // downloading process is successfully started
    // just wait for callback and return to caller
    while (!this->IsAsyncDownloadFinished())
    {
      // just sleep
      Sleep(100);
    }

    result = this->asyncDownloadResult;
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::DownloadAsync(LPCOLESTR uri, LPCOLESTR fileName, IDownloadCallback *downloadCallback)
{
  HRESULT result = S_OK;

  // reset all internal properties to default values
  this->ClearSession(true);

  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_ASYNC_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, uri);
  CHECK_POINTER_DEFAULT_HRESULT(result, fileName);
  CHECK_POINTER_DEFAULT_HRESULT(result, downloadCallback);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

  if (SUCCEEDED(result))
  {
    this->asyncDownloadResult = S_OK;
    this->flags &= ~MP_URL_SOURCE_SPLITTER_FLAG_ASYNC_DOWNLOAD_FINISHED;
    this->asyncDownloadCallback = downloadCallback;
  }

  if (SUCCEEDED(result))
  {
    this->downloadFileName = ConvertToUnicodeW(fileName);

    result = (this->downloadFileName == NULL) ? E_CONVERT_STRING_ERROR : S_OK;
  }
  
  if (SUCCEEDED(result))
  {
    CParameterCollection *suppliedParameters = this->ParseParameters(uri);
    if (suppliedParameters != NULL)
    {
      // we have set some parameters
      // set them as configuration parameters
      this->configuration->Clear();
      this->configuration->Append(suppliedParameters);
      if (!this->configuration->Contains(PARAMETER_NAME_URL, true))
      {
        CHECK_CONDITION_HRESULT(result, this->configuration->Add(PARAMETER_NAME_URL, uri), result, E_OUTOFMEMORY);
      }
      CHECK_CONDITION_HRESULT(result, this->configuration->Add(PARAMETER_NAME_DOWNLOAD_FILE_NAME, this->downloadFileName), result, E_OUTOFMEMORY);

      FREE_MEM_CLASS(suppliedParameters);
    }
    else
    {
      // parameters are not supplied, just set current url and download file name as only parameters in configuration
      this->configuration->Clear();

      CHECK_CONDITION_HRESULT(result, this->configuration->Add(PARAMETER_NAME_URL, uri), result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(result, this->configuration->Add(PARAMETER_NAME_DOWNLOAD_FILE_NAME, this->downloadFileName), result, E_OUTOFMEMORY);
    }
  }

  if (SUCCEEDED(result))
  {
    // loads protocol based on current configuration parameters
    result = this->LoadAsync();
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_ASYNC_NAME, result);
  return result;
}

// IDownloadCallback

void STDMETHODCALLTYPE CMPUrlSourceSplitter::OnDownloadCallback(HRESULT downloadResult)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_CALLBACK_NAME);

  this->asyncDownloadResult = downloadResult;

  this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_ASYNC_DOWNLOAD_FINISHED;

  if ((this->asyncDownloadCallback != NULL) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOAD_CALLBACK_CALLED)))
  {
    // if download callback is set and it is not current instance (avoid recursion)
    this->asyncDownloadCallback->OnDownloadCallback(downloadResult);
    this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOAD_CALLBACK_CALLED;
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_CALLBACK_NAME);
}

// ISeeking

unsigned int CMPUrlSourceSplitter::GetSeekingCapabilities(void)
{
  unsigned int capabilities = SEEKING_METHOD_NONE;

  if (this->parserHoster != NULL)
  {
    capabilities = this->parserHoster->GetSeekingCapabilities();
  }

  return capabilities;
}

int64_t CMPUrlSourceSplitter::SeekToTime(unsigned int streamId, int64_t time)
{
  int64_t result = -1;

  if (this->parserHoster != NULL)
  {
    // seek to time for specified stream ID
    result = this->parserHoster->SeekToTime(streamId, time);
  }

  return result;
}

void CMPUrlSourceSplitter::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  if (this->parserHoster != NULL)
  {
    this->parserHoster->SetPauseSeekStopMode(pauseSeekStopMode);
  }
}

// IDemuxerOwner

int64_t CMPUrlSourceSplitter::GetDuration(void)
{
  return (this->parserHoster != NULL) ? this->parserHoster->GetDuration() : DURATION_UNSPECIFIED;
}

HRESULT CMPUrlSourceSplitter::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  HRESULT result = E_NOT_VALID_STATE;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->ProcessStreamPackage(streamPackage);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = E_NOTIMPL;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->QueryStreamProgress(streamProgress);
  }

  return result;
}

// IFilterState

HRESULT CMPUrlSourceSplitter::IsFilterReadyToConnectPins(bool *ready)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, ready);

  if (SUCCEEDED(result))
  {
    *ready = false;

    {
      CLockMutex lock(this->demuxersMutex, INFINITE);

      result = this->loadAsyncResult;

      if (SUCCEEDED(result))
      {
        bool createdDemuxers = (this->demuxers->Count() > 0);

        // all demuxers must be created successfully
        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->demuxers->Count())); i++)
        {
          CDemuxer *demuxer = this->demuxers->GetItem(i);

          createdDemuxers &= demuxer->IsCreatedDemuxer();

          // if demuxer is not created and demuxer worker finished its work
          // it throws exception in caller and immediately stops buffering and playback
          result = ((!demuxer->IsCreatedDemuxer()) && demuxer->IsCreateDemuxerWorkerFinished()) ? demuxer->GetCreateDemuxerError() : result;
        }

        *ready = SUCCEEDED(result) ? createdDemuxers : false;
      }
    }
  }
  
  return result;
}

HRESULT CMPUrlSourceSplitter::GetCacheFileName(wchar_t **path)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, path);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->demuxersMutex, INFINITE);

    //const wchar_t *storeFilePath = (this->demuxers->Count() == 1) ? (this->demuxers->GetItem(0)->GetCacheFilePath()) : L"";

    const wchar_t *storeFilePath = L"";
    
    SET_STRING(*path, storeFilePath);
    result = TEST_STRING_WITH_NULL(*path, storeFilePath) ? result : E_OUTOFMEMORY;
  }

  return result;
}

// IFilterStateEx

STDMETHODIMP CMPUrlSourceSplitter::GetVersion(unsigned int *version)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, version);

  if (SUCCEEDED(result))
  {
    uint64_t buildDate = BUILD_INFO_MP_URL_SOURCE_SPLITTER - UNIX_TIMESTAMP_2000_01_01;
    buildDate /= SECONDS_IN_DAY;

    *version = (unsigned int)buildDate;
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::IsFilterError(bool *isFilterError, HRESULT error)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, isFilterError);

  if (SUCCEEDED(result))
  {
    *isFilterError = (IS_OUR_ERROR(error) || IS_CURL_ERROR(error));
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::GetErrorDescription(HRESULT error, wchar_t **description)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, description);

  if (SUCCEEDED(result))
  {
    for (unsigned int i = 0; ; i++)
    {
      ErrorMessage msg = ERROR_MESSAGES[i];

      if (msg.code == error)
      {
        SET_STRING_HRESULT_WITH_NULL(*description, msg.message, result);
        break;
      }

      if (msg.code == 0)
      {
        // the last item without specified message
        SET_STRING_HRESULT_WITH_NULL(*description, L"", result);
        break;
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter::LoadAsync(void)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, this->configuration, result, E_INVALID_CONFIGURATION);
  CHECK_POINTER_HRESULT(result, this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL), result, E_URL_NOT_SPECIFIED);

  if (SUCCEEDED(result))
  {
    result = S_FALSE;

    if (this->loadAsyncWorkerThread == NULL)
    {
      result = this->CreateLoadAsyncWorker();
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = S_FALSE);
    }

    if (SUCCEEDED(result) && (this->loadAsyncWorkerThread != NULL))
    {
      result = (WaitForSingleObject(this->loadAsyncWorkerThread, 0) == WAIT_TIMEOUT) ? S_FALSE : this->loadAsyncResult;
    }

    if (result != S_FALSE)
    {
      // thread finished or error
      this->DestroyLoadAsyncWorker();
    }
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::LoadAsync(const wchar_t *url)
{
  // reset all internal properties to default values
  this->ClearSession(true);

  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_LOAD_ASYNC_NAME);
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, url);

  if (SUCCEEDED(result))
  {
    CParameterCollection *suppliedParameters = this->ParseParameters(url);
    if (suppliedParameters != NULL)
    {
      // we have set some parameters
      // set them as configuration parameters
      this->configuration->Clear();
      this->configuration->Append(suppliedParameters);
      if (!this->configuration->Contains(PARAMETER_NAME_URL, true))
      {
        result = this->configuration->Add(PARAMETER_NAME_URL, url) ? result : E_OUTOFMEMORY;
      }

      FREE_MEM_CLASS(suppliedParameters);
    }
    else
    {
      // parameters are not supplied, just set current url as only one parameter in configuration
      this->configuration->Clear();
      result = this->configuration->Add(PARAMETER_NAME_URL, url) ? result : E_OUTOFMEMORY;
    }

    if (SUCCEEDED(result))
    {
      result = this->LoadAsync();
    }
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LOAD_ASYNC_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::IsStreamOpened(bool *opened)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, opened);

  if (SUCCEEDED(result))
  {
    *opened = false;
    result = SUCCEEDED(this->loadAsyncResult) ? result : this->loadAsyncResult;

    if (SUCCEEDED(result))
    {
      *opened = this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_STREAM_OPENED);
    }
  }
  
  return result;
}

// CAMThread

DWORD CMPUrlSourceSplitter::ThreadProc()
{
  // last command is no command
  this->lastCommand = -1;

  COutputPinPacket *packet = NULL;
  HRESULT seekResult = S_OK;

  for (DWORD cmd = (DWORD)-1; ; cmd = GetRequest())
  {
    this->lastCommand = cmd;
    switch (cmd)
    {
    case CMD_EXIT:
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, L"CMD_EXIT");
      break;
    case CMD_PAUSE:
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, L"CMD_PAUSE");
      break;
    case CMD_SEEK:
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, L"CMD_SEEK");
      break;
    case CMD_PLAY:
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, L"CMD_PLAY");
      break;
    case (DWORD)-1:
      // ignore, it means no command
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, L"no command");
      break;
    default:
      this->logger->Log(LOGGER_INFO, L"%s: %s: unknown command: %d", MODULE_NAME, METHOD_THREAD_PROC_NAME, cmd);
      break;
    }

    if (cmd == CMD_EXIT)
    {
      Reply(S_OK);
      break;
    }

    // it can be CMD_SEEK or no command
    if ((cmd != CMD_PAUSE) && (cmd != CMD_PLAY))
    {
      // clear packet (if any left)
      FREE_MEM_CLASS(packet);

      this->demuxStart = this->demuxNewStart;

      if (this->IsSplitter() && ((this->demuxStart != 0) || this->IsPlaybackStarted()))
      {
        // in case of live stream, seek by position or time (mostly slightly backward in stream)
        // in live stream we can't receive seek request from graph to skip forward or backward
        // we can only seek in case of starting playback (created thread, not CMD_PLAY request) or in case of changing stream (probably audio or subtitle stream)

        for (unsigned int i = 0; (SUCCEEDED(seekResult) && (i < this->demuxers->Count())); i++)
        {
          CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(this->demuxers->GetItem(i));

          seekResult = demuxer->Seek(max(this->demuxStart, 0));
        }
      }

      if (cmd != (DWORD)-1)
      {
        // if any command, than reply
        Reply(S_OK);
      }

      for (unsigned int i = 0; i < this->outputPins->Count(); i++)
      {
        CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

        if (outputPin->IsConnected())
        {
          outputPin->DeliverNewSegment(this->demuxStart, MAXLONGLONG, 1.0);
        }
      }
    }
    else
    {
      if (cmd != (DWORD)-1)
      {
        // if any command, than reply
        Reply(S_OK);
      }
    }

    if ((cmd == CMD_PLAY) || (cmd == CMD_PAUSE))
    {
      // start or continue work of output pins
      for (unsigned int i = 0; i < this->outputPins->Count(); i++)
      {
        CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

        switch (cmd)
        {
        case CMD_PLAY:
          outputPin->DeliverPlay();
          break;
        case CMD_PAUSE:
          outputPin->DeliverPause();
          break;
        }
      }
    }

    if (cmd == CMD_PLAY)
    {
      // start or continue work in demuxers
      for (unsigned int i = 0; i < this->demuxers->Count(); i++)
      {
        this->demuxers->GetItem(i)->SetPauseSeekStopRequest(false);
      }

      this->pauseSeekStopRequest = false;
    }

    this->flags &= ~(MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DISCONTINUITY | MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DELAYING | MP_URL_SOURCE_SPLITTER_FLAG_CORRECTED_TIMESTAMP | MP_URL_SOURCE_SPLITTER_FLAG_ALL_PINS_END_OF_STREAM);

    unsigned int lastReportStreamTime = 0;
    unsigned int lastDemuxerId = 0;

    while (!CheckRequest(&cmd))
    {
      if ((cmd == CMD_PAUSE) || (cmd == CMD_SEEK) || (this->pauseSeekStopRequest))
      {
        Sleep(1);
      }
      else if (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_ALL_PINS_END_OF_STREAM))
      {
        HRESULT result = S_OK;

        // get new packet only when we don't have any packet to send
        if (packet == NULL)
        {
          packet = new COutputPinPacket(&result);
          CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

          if (SUCCEEDED(seekResult))
          {
            result = this->GetNextPacket(packet, lastDemuxerId);
          }
          else
          {
            for (unsigned int i = 0; i < this->outputPins->Count(); i++)
            {
              CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

              if (!outputPin->IsEndOfStream())
              {
                // create end of stream packet for output pin

                packet->SetDemuxerId(outputPin->GetDemuxerId());
                packet->SetStreamPid(outputPin->GetStreamPid());
                packet->SetEndOfStream(true, seekResult);

                result = S_OK;
                break;
              }
            }
          }
        }

        // it can return S_FALSE (no output pin packet) or error code
        if (result == S_OK)
        {
          // in case of IPTV there is only one output pin
          // in case of splitter there can be more than one output pin

          CMPUrlSourceSplitterOutputPin *pin = NULL;

          if (this->IsIptv())
          {
            pin = this->outputPins->GetItem(0);
          }
          else
          {
            for (unsigned int i = 0; i < this->outputPins->Count(); i++)
            {
              CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

              if ((outputPin->GetDemuxerId() == packet->GetDemuxerId()) &&
                  (outputPin->GetStreamPid() == packet->GetStreamPid()))
              {
                pin = outputPin;
                break;
              }
            }

            if ((pin != NULL) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_CORRECTED_TIMESTAMP)))
            {
              // timestamp correction is needed when seek happen
              // from FFmpeg we get timestamp from beginning of stream, but filters need timestamps relative to seek point
              // also in case of unsuccesful adding packet to output pin we must correct timestamps only once

              this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_CORRECTED_TIMESTAMP;

              if (packet->GetStartTime() != COutputPinPacket::INVALID_TIME)
              {
                packet->SetStartTime(packet->GetStartTime() - this->demuxStart);
                packet->SetEndTime(packet->GetEndTime() - this->demuxStart);

                ASSERT(packet->GetStartTime() <= packet->GetEndTime());
              }
            }

            if ((pin != NULL) && (packet->IsDiscontinuity()))
            {
              CHECK_CONDITION_EXECUTE(!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DISCONTINUITY), this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity packet, demuxer: %u, stream ID: %u, start: %016lld, end: %016lld", MODULE_NAME, METHOD_THREAD_PROC_NAME, packet->GetDemuxerId(), packet->GetStreamPid(), packet->GetStartTime(), packet->GetEndTime()));
              this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DISCONTINUITY;

              CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(this->demuxers->GetItem(packet->GetDemuxerId()));
              if (demuxer != NULL)
              {
                CStream *audioStream = demuxer->GetActiveStream(CStream::Audio);
                if (audioStream != NULL)
                {
                  if (packet->GetStreamPid() == audioStream->GetPid())
                  {
                    // we have audio stream discontinuity
                    // we must wait with output pin packet for right time (at max 10 ms before packet start time)

                    CRefTime refTime;
                    if (SUCCEEDED(this->StreamTime(refTime)))
                    {
                      // refTime is measured from demuxStart
                      // packet start time is also measured from demuxStart

                      if (refTime.Millisecs() > 0)
                      {
                        int64_t streamTime = (int64_t)(refTime.Millisecs() * (DSHOW_TIME_BASE / 1000));

                        if (packet->GetStartTime() >= (streamTime + 100000))
                        {
                          CHECK_CONDITION_EXECUTE(!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DELAYING), this->logger->Log(LOGGER_WARNING, L"%s: %s: delaying packet, demuxer: %u, stream ID: %u, start: %016lld, end: %016lld, delay: %016lld, stream time: %lld, demux start: %lld", MODULE_NAME, METHOD_THREAD_PROC_NAME, packet->GetDemuxerId(), packet->GetStreamPid(), packet->GetStartTime(), packet->GetEndTime(), packet->GetStartTime() - (streamTime + 100000), streamTime, this->demuxStart));

                          this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DELAYING;
                          result = E_FAIL;

                          Sleep(1);
                        }
                      }
                    }
                  }
                }
              }
            }
          }

          if (pin != NULL)
          {
            // if pin is not connected, then demuxed packets will only store in its internal output collection and don't go anywhere
            // in this case it only occupy memory and it is never freed (except destroying pin, flushing, etc.)

            if (pin->IsConnected())
            {
              packet->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = pin->QueuePacket(packet, 100));
            }

            if (SUCCEEDED(result))
            {
              // packet was queued to output pin, doesn't need to hold it's reference
              packet = NULL;

              this->flags &= ~(MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DISCONTINUITY | MP_URL_SOURCE_SPLITTER_FLAG_REPORTED_PACKET_DELAYING | MP_URL_SOURCE_SPLITTER_FLAG_CORRECTED_TIMESTAMP);
            }

            if (FAILED(result))
            {
              // timeout or any other error occured while trying to add output packet to output pin
              // hold packet and try later

              result = S_OK;
            }
          }
          else
          {
            // no pin for packet, delete packet
            FREE_MEM_CLASS(packet);
          }
        }
        else if (result == S_FALSE)
        {
          // no output packet get
          FREE_MEM_CLASS(packet);
        }

        // if some error, delete packet
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));

        if (SUCCEEDED(result))
        {
          lastDemuxerId = (++lastDemuxerId) % this->demuxers->Count();

          // set end of stream flag, if all output pins have queued end of stream packets
          this->flags |= (this->outputPins->Count() > 0) ? MP_URL_SOURCE_SPLITTER_FLAG_ALL_PINS_END_OF_STREAM : MP_URL_SOURCE_SPLITTER_FLAG_NONE;

          for (unsigned int i = 0; i < this->outputPins->Count(); i++)
          {
            CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

            CHECK_CONDITION_EXECUTE(!outputPin->IsEndOfStream(), this->flags &= ~MP_URL_SOURCE_SPLITTER_FLAG_ALL_PINS_END_OF_STREAM);
          }

          if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_ALL_PINS_END_OF_STREAM))
          {
            this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, L"all output pins have end of stream");
          }
        }

        if (result == S_FALSE)
        {
          // no output packet, sleep some time
          Sleep(1);
        }
      }
      else
      {
        // no CMD_PAUSE, no CMD_SEEK, no pauseSeekStopRequest, endOfStream is set
        Sleep(1);
      }

      if (this->CanReportStreamTime() && (!this->IsDownloadingFile()) && ((GetTickCount() - lastReportStreamTime) > 1000))
      {
        // report stream time to demuxers, parsers and protocols
        lastReportStreamTime = GetTickCount();

        CRefTime refTime;
        if (SUCCEEDED(this->StreamTime(refTime)))
        {
          if (refTime.Millisecs() > 0)
          {
            uint64_t streamTime = (uint64_t)(this->demuxStart / (DSHOW_TIME_BASE / 1000) + refTime.Millisecs());

            this->parserHoster->ReportStreamTime(streamTime, (this->demuxers->Count() == 1) ? this->demuxers->GetItem(0)->GetPositionForStreamTime(streamTime) : 0);
          }
        }
      }

      if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_ALL_PINS_END_OF_STREAM) && this->IsDownloadingFile())
      {
        // check if downloading isn't finished
        CMPUrlSourceSplitterOutputDownloadPin *outputPin = (CMPUrlSourceSplitterOutputDownloadPin *)this->outputPins->GetItem(0);

        if (outputPin->IsDownloadFinished() && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOAD_CALLBACK_CALLED)))
        {
          this->OnDownloadCallback(outputPin->GetDownloadResult());
          this->flags |= MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOAD_CALLBACK_CALLED;
        }
      }
    }
    
    if (!CheckRequest(&cmd))
    {
      // if we didn't exit by request, deliver end-of-stream

      for (unsigned int i = 0; i < this->outputPins->Count(); i++)
      {
        this->outputPins->GetItem(i)->QueueEndOfStream(S_OK);
      }
    }
  }

  // delete pending packet (if necessary)
  FREE_MEM_CLASS(packet);
  return S_OK;
}

/* other methods */

void CMPUrlSourceSplitter::DeliverBeginFlush()
{
  if (this->outputPins != NULL)
  {
    for (unsigned int i = 0; i < this->outputPins->Count(); i++)
    {
      CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

      outputPin->DeliverBeginFlush();
    }
  }
}

void CMPUrlSourceSplitter::DeliverEndFlush()
{
  if (this->outputPins != NULL)
  {
    for (unsigned int i = 0; i < this->outputPins->Count(); i++)
    {
      CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

      outputPin->DeliverEndFlush();
    }
  }
}

bool CMPUrlSourceSplitter::IsIptv(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_AS_IPTV);
}

bool CMPUrlSourceSplitter::IsSplitter(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_AS_SPLITTER);
}

bool CMPUrlSourceSplitter::IsDownloadingFile(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOADING_FILE);
}

bool CMPUrlSourceSplitter::IsLiveStream(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_LIVE_STREAM);
}

bool CMPUrlSourceSplitter::IsAsyncDownloadFinished(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_ASYNC_DOWNLOAD_FINISHED);
}

bool CMPUrlSourceSplitter::IsDownloadCallbackCalled(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOAD_CALLBACK_CALLED);
}

bool CMPUrlSourceSplitter::IsEnabledMethodActive(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_ENABLED_METHOD_ACTIVE);
}

bool CMPUrlSourceSplitter::IsPlaybackStarted(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_PLAYBACK_STARTED);
}

bool CMPUrlSourceSplitter::CanReportStreamTime(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_REPORT_STREAM_TIME);
}

HRESULT CMPUrlSourceSplitter::GetNextPacket(COutputPinPacket *packet, unsigned int demuxerId)
{
  HRESULT result = S_FALSE;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    // don't wait too long for output packet
    // we can try to get output packet later
    CLockMutex lock(this->demuxersMutex, 20);

    if (lock.IsLocked())
    {
      unsigned int inputDemuxerId = demuxerId;

      while (true)
      {
        CDemuxer *demuxer = this->demuxers->GetItem(demuxerId);

        result = demuxer->GetOutputPinPacket(packet);
        demuxerId = (++demuxerId) % this->demuxers->Count();

        if ((result != S_FALSE) || (inputDemuxerId == demuxerId))
        {
          break;
        }
      }
    }
  }

  return result;
}

// split parameters string by separator
// @param parameters : null-terminated string containing parameters
// @param separator : null-terminated separator string
// @param length : length of first token (without separator)
// @param restOfParameters : reference to rest of parameter string without first token and separator, if NULL then there is no rest of parameters and whole parameters string was processed
// @param separatorMustBeFound : specifies if separator must be found
// @return : true if successful, false otherwise
bool SplitBySeparator(const wchar_t *parameters, const wchar_t *separator, unsigned int *length, wchar_t **restOfParameters, bool separatorMustBeFound)
{
  bool result = false;

  if ((parameters != NULL) && (separator != NULL) && (length != NULL) && (restOfParameters))
  {
    unsigned int parameterLength = wcslen(parameters);

    wchar_t *tempSeparator = NULL;
    wchar_t *tempParameters = (wchar_t *)parameters;

    tempSeparator = (wchar_t *)wcsstr(tempParameters, separator);
    if (tempSeparator == NULL)
    {
      // separator not found
      *length = wcslen(parameters);
      *restOfParameters = NULL;
      result = !separatorMustBeFound;
    }
    else
    {
      // separator found
      if (wcslen(tempSeparator) > 1)
      {
        // we are not on the last character of separator
        // move to end of separator
        tempParameters = tempSeparator + wcslen(separator);
      }
    }

    if (tempSeparator != NULL)
    {
      // we found separator
      // everything before separator is token, everything after separator is rest
      *length = parameterLength - wcslen(tempSeparator);
      *restOfParameters = tempSeparator + wcslen(separator);
      result = true;
    }
  }

  return result;
}

CParameterCollection *CMPUrlSourceSplitter::ParseParameters(const wchar_t *parameters)
{
  HRESULT result = S_OK;
  CParameterCollection *parsedParameters = new CParameterCollection(&result);

  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME);

  CHECK_POINTER_HRESULT(result, parameters, result, E_INVALIDARG);
  CHECK_POINTER_HRESULT(result, parsedParameters, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    this->logger->Log(LOGGER_INFO, L"%s: %s: parameters: %s", MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, parameters);

    // now we have unified string
    // let's parse

    parsedParameters->Clear();

    if (SUCCEEDED(result))
    {
      bool splitted = false;
      unsigned int tokenLength = 0;
      wchar_t *rest = NULL;

      splitted = SplitBySeparator(parameters, PARAMETER_SEPARATOR_STOCK_FILTER, &tokenLength, &rest, true);
      if (splitted)
      {
        // identifier for parameters for MediaPortal Source Filter is found
        parameters = rest;
        splitted = false;

        do
        {
          splitted = SplitBySeparator(parameters, PARAMETER_SEPARATOR_STOCK_FILTER, &tokenLength, &rest, false);
          if (splitted)
          {
            // token length is without terminating null character
            tokenLength++;

            ALLOC_MEM_DEFINE_SET(token, wchar_t, tokenLength, 0);
            CHECK_POINTER_HRESULT(result, token, result, E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_TOKEN);

            if (SUCCEEDED(result))
            {
              // copy token from parameters string
              wcsncpy_s(token, tokenLength, parameters, tokenLength - 1);
              parameters = rest;

              unsigned int nameLength = 0;
              wchar_t *value = NULL;
              bool splittedNameAndValue = SplitBySeparator(token, PARAMETER_ASSIGN, &nameLength, &value, true);

              if ((splittedNameAndValue) && (nameLength != 0))
              {
                // if correctly splitted parameter name and value
                nameLength++;

                ALLOC_MEM_DEFINE_SET(name, wchar_t, nameLength, 0);
                CHECK_POINTER_HRESULT(result, name, result, E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_PARAMETER_NAME);

                if (SUCCEEDED(result))
                {
                  // copy name from token
                  // the value is plain text
                  wcsncpy_s(name, nameLength, token, nameLength - 1);

                  // check parameter name again known stock filter parameter names
                  // if supported, replace them with filter parameter name
                  
                  for (unsigned int i = 0; (SUCCEEDED(result) && (i < PARAMETER_NAME_STOCK_FILTER_TOTAL_SUPPORTED)); i++)
                  {
                    if (_wcsicmp(name, SUPPORTED_PARAMETER_NAME_STOCK_FILTER[i]) == 0)
                    {
                      CHECK_CONDITION_HRESULT(result, parsedParameters->Add(REPLACE_PARAMETER_NAME_STOCK_FILTER[i], value), result, E_OUTOFMEMORY);
                      break;
                    }
                  }
                }

                FREE_MEM(name);
              }
            }

            FREE_MEM(token);
          }
        } while ((splitted) && (rest != NULL) && (SUCCEEDED(result)));
      }
    }

    if (SUCCEEDED(result) && (parsedParameters->Count() == 0))
    {
      bool splitted = false;
      unsigned int tokenLength = 0;
      wchar_t *rest = NULL;

      splitted = SplitBySeparator(parameters, PARAMETER_IDENTIFIER, &tokenLength, &rest, false);
      if (splitted)
      {
        // identifier for parameters for MediaPortal Source Filter is found
        parameters = rest;
        splitted = false;

        do
        {
          splitted = SplitBySeparator(parameters, PARAMETER_SEPARATOR, &tokenLength, &rest, false);
          if (splitted)
          {
            // token length is without terminating null character
            tokenLength++;
            ALLOC_MEM_DEFINE_SET(token, wchar_t, tokenLength, 0);
            CHECK_POINTER_HRESULT(result, token, result, E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_TOKEN);

            if (SUCCEEDED(result))
            {
              // copy token from parameters string
              wcsncpy_s(token, tokenLength, parameters, tokenLength - 1);
              parameters = rest;

              unsigned int nameLength = 0;
              wchar_t *value = NULL;
              bool splittedNameAndValue = SplitBySeparator(token, PARAMETER_ASSIGN, &nameLength, &value, true);

              if ((splittedNameAndValue) && (nameLength != 0))
              {
                // if correctly splitted parameter name and value
                nameLength++;
                ALLOC_MEM_DEFINE_SET(name, wchar_t, nameLength, 0);
                CHECK_POINTER_HRESULT(result, name, result, E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_PARAMETER_NAME);

                if (SUCCEEDED(result))
                {
                  // copy name from token
                  wcsncpy_s(name, nameLength, token, nameLength - 1);

                  // the value is in url encoding (percent encoding)
                  // so it doesn't have doubled separator

                  // CURL library cannot handle wchar_t characters
                  // convert to mutli-byte character set

                  wchar_t *replacedValue = ReplaceString(value, L"+", L"%20");
                  char *curlValue = ConvertToMultiByte(replacedValue);
                  CHECK_POINTER_HRESULT(result, curlValue, result, E_CONVERT_STRING_ERROR);

                  if (SUCCEEDED(result))
                  {
                    char *unescapedCurlValue = curl_easy_unescape(NULL, curlValue, 0, NULL);
                    CHECK_POINTER_HRESULT(result, unescapedCurlValue, result, E_PARSE_PARAMETERS_CANNOT_GET_UNESCAPED_VALUE);
                    
                    if (SUCCEEDED(result))
                    {
                      wchar_t *unescapedValue = ConvertToUnicodeA(unescapedCurlValue);
                      CHECK_POINTER_HRESULT(result, unescapedValue, result, E_CONVERT_STRING_ERROR);

                      if (SUCCEEDED(result))
                      {
                        // we got successfully unescaped parameter value
                        CParameter *parameter = new CParameter(&result, name, unescapedValue);
                        CHECK_POINTER_HRESULT(result, parameter, result, E_OUTOFMEMORY);

                        CHECK_CONDITION_HRESULT(result, parsedParameters->CCollection::Add(parameter), result, E_OUTOFMEMORY);
                        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(parameter));
                      }

                      // free unescaped value
                      FREE_MEM(unescapedValue);

                      // free CURL return value
                      curl_free(unescapedCurlValue);
                    }
                  }

                  FREE_MEM(curlValue);
                  FREE_MEM(replacedValue);
                }

                FREE_MEM(name);
              }
            }

            FREE_MEM(token);
          }
        } while ((splitted) && (rest != NULL) && (SUCCEEDED(result)));
      }
    }

    if (SUCCEEDED(result))
    {
      this->logger->Log(LOGGER_INFO, L"%s: %s: count of parameters: %u", MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, parsedParameters->Count());
      parsedParameters->LogCollection(this->logger, LOGGER_INFO, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME);
    }
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, result);

  if ((FAILED(result)) && (parsedParameters != NULL))
  {
    FREE_MEM_CLASS(parsedParameters);
  }
  
  return parsedParameters;
}

HRESULT CMPUrlSourceSplitter::CreateLoadAsyncWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CREATE_LOAD_ASYNC_WORKER_NAME);

  if (this->loadAsyncWorkerThread == NULL)
  {
    this->loadAsyncWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CMPUrlSourceSplitter::LoadAsyncWorker, this, 0, NULL);
  }

  if (this->loadAsyncWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_LOAD_ASYNC_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_LOAD_ASYNC_WORKER_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter::DestroyLoadAsyncWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTROY_LOAD_ASYNC_WORKER_NAME);

  this->loadAsyncWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->loadAsyncWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->loadAsyncWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_LOAD_ASYNC_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->loadAsyncWorkerThread, 0);
    }
    CloseHandle(this->loadAsyncWorkerThread);
  }

  this->loadAsyncWorkerThread = NULL;
  this->loadAsyncWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_LOAD_ASYNC_WORKER_NAME, result);
  return result;
}

unsigned int WINAPI CMPUrlSourceSplitter::LoadAsyncWorker(LPVOID lpParam)
{
  CMPUrlSourceSplitter *caller = (CMPUrlSourceSplitter *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_LOAD_ASYNC_WORKER_NAME);
  bool fakeIptvUrl = false;

  if (SUCCEEDED(caller->loadAsyncResult))
  {
    if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_AS_IPTV))
    {
      // FAKE for UDP protocol request from MediaPortal
      const wchar_t *url = caller->configuration->GetValue(PARAMETER_NAME_URL, true, NULL);

      if (_wcsnicmp(url, L"udp://@0.0.0.0:1234", 19) != 0)
      {
        CHECK_POINTER_DEFAULT_HRESULT(caller->loadAsyncResult, caller->parserHoster);

        if (SUCCEEDED(caller->loadAsyncResult))
        {
          // IPTV is always live stream, remove live stream flag and create new one
          caller->configuration->Remove(PARAMETER_NAME_LIVE_STREAM, true);
          CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->configuration->Add(PARAMETER_NAME_LIVE_STREAM, L"1"), caller->loadAsyncResult, E_OUTOFMEMORY);
        }
      }
      else
      {
        fakeIptvUrl = true;
      }

      // if in output pin collection isn't any pin, then add new output pin with MPEG2 TS media type
      // in another case filter assume that there is only one output pin with MPEG2 TS media type
      //if (SUCCEEDED(caller->loadAsyncResult) && (caller->outputPins->Count() == 0))
      //{
      //  // create valid MPEG2 TS media type, add it to media types and create output pin
      //  CMediaTypeCollection *mediaTypes = new CMediaTypeCollection(&caller->loadAsyncResult);
      //  CHECK_POINTER_HRESULT(caller->loadAsyncResult, mediaTypes, caller->loadAsyncResult, E_OUTOFMEMORY);

      //  if (SUCCEEDED(caller->loadAsyncResult))
      //  {
      //    CMediaType *mediaType = new CMediaType();
      //    CHECK_POINTER_HRESULT(caller->loadAsyncResult, mediaType, caller->loadAsyncResult, E_OUTOFMEMORY);

      //    if (SUCCEEDED(caller->loadAsyncResult))
      //    {
      //      mediaType->SetType(&MEDIATYPE_Stream);
      //      mediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);
      //    }

      //    CHECK_CONDITION_HRESULT(caller->loadAsyncResult, mediaTypes->Add(mediaType), caller->loadAsyncResult, E_OUTOFMEMORY);
      //    CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(mediaType));
      //  }

      //  if (SUCCEEDED(caller->loadAsyncResult))
      //  {
      //    //CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputPin(L"Output", caller, caller, &caller->loadAsyncResult, caller->logger, caller->configuration, mediaTypes);
      //    CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputM2tsMuxerPin(L"Output", caller, caller, &caller->loadAsyncResult, caller->logger, caller->configuration, mediaTypes);
      //    CHECK_POINTER_HRESULT(caller->loadAsyncResult, outputPin, caller->loadAsyncResult, E_OUTOFMEMORY);

      //    CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->outputPins->Add(outputPin), caller->loadAsyncResult, E_OUTOFMEMORY);
      //    CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(outputPin));
      //  }

      //  FREE_MEM_CLASS(mediaTypes);
      //}

      if (SUCCEEDED(caller->loadAsyncResult))
      {
        caller->outputPins->Clear();

        // create valid MPEG2 TS media type, add it to media types and create output pin
        CMediaTypeCollection *mediaTypes = new CMediaTypeCollection(&caller->loadAsyncResult);
        CHECK_POINTER_HRESULT(caller->loadAsyncResult, mediaTypes, caller->loadAsyncResult, E_OUTOFMEMORY);

        if (SUCCEEDED(caller->loadAsyncResult))
        {
          CMediaType *mediaType = new CMediaType();
          CHECK_POINTER_HRESULT(caller->loadAsyncResult, mediaType, caller->loadAsyncResult, E_OUTOFMEMORY);

          if (SUCCEEDED(caller->loadAsyncResult))
          {
            mediaType->SetType(&MEDIATYPE_Stream);
            mediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);
          }

          CHECK_CONDITION_HRESULT(caller->loadAsyncResult, mediaTypes->Add(mediaType), caller->loadAsyncResult, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(mediaType));
        }

        if (SUCCEEDED(caller->loadAsyncResult))
        {
          CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputM2tsMuxerPin(L"Output", caller, caller, &caller->loadAsyncResult, caller->logger, caller->configuration, mediaTypes);
          CHECK_POINTER_HRESULT(caller->loadAsyncResult, outputPin, caller->loadAsyncResult, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->outputPins->Add(outputPin), caller->loadAsyncResult, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(outputPin));
        }

        FREE_MEM_CLASS(mediaTypes);
      }

      // add IPTV flag to configuration
      CHECK_CONDITION_EXECUTE(SUCCEEDED(caller->loadAsyncResult), caller->configuration->Remove(PARAMETER_NAME_IPTV, true));
      CHECK_CONDITION_EXECUTE(SUCCEEDED(caller->loadAsyncResult), caller->configuration->Remove(PARAMETER_NAME_SPLITTER, true));
      CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->configuration->Add(PARAMETER_NAME_IPTV, L"1"), caller->loadAsyncResult, E_OUTOFMEMORY);
    }
    else if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_AS_SPLITTER))
    {
      CHECK_POINTER_DEFAULT_HRESULT(caller->loadAsyncResult, caller->parserHoster);

      // output pins are created after demuxer is created
      // now we don't know nothing about video/audio/other stream types

      // add splitter flag to configuration
      CHECK_CONDITION_EXECUTE(SUCCEEDED(caller->loadAsyncResult), caller->configuration->Remove(PARAMETER_NAME_IPTV, true));
      CHECK_CONDITION_EXECUTE(SUCCEEDED(caller->loadAsyncResult), caller->configuration->Remove(PARAMETER_NAME_SPLITTER, true));
      CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->configuration->Add(PARAMETER_NAME_SPLITTER, L"1"), caller->loadAsyncResult, E_OUTOFMEMORY);
    }
  }

  if (SUCCEEDED(caller->loadAsyncResult))
  {
    // check log file parameter, if not set, add default
    if (!caller->configuration->Contains(PARAMETER_NAME_LOG_FILE_NAME, true))
    {
      wchar_t *logFile = caller->IsIptv() ? GetTvServerFilePath(MP_IPTV_SOURCE_LOG_FILE) : GetMediaPortalFilePath(MP_URL_SOURCE_SPLITTER_LOG_FILE);
      CHECK_POINTER_HRESULT(caller->loadAsyncResult, logFile, caller->loadAsyncResult, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->configuration->Add(PARAMETER_NAME_LOG_FILE_NAME, logFile), caller->loadAsyncResult, E_OUTOFMEMORY);
      FREE_MEM(logFile);
    }

    // set logger parameters
    caller->logger->SetParameters(caller->configuration);
  }

  if (SUCCEEDED(caller->loadAsyncResult))
  {
    FREE_MEM(caller->downloadFileName);
    caller->downloadFileName = Duplicate(caller->configuration->GetValue(PARAMETER_NAME_DOWNLOAD_FILE_NAME, true, NULL));
    caller->flags |= (caller->downloadFileName != NULL) ? MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOADING_FILE : MP_URL_SOURCE_SPLITTER_FLAG_NONE;
    caller->flags |= (caller->configuration->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT)) ? MP_URL_SOURCE_SPLITTER_FLAG_LIVE_STREAM : MP_URL_SOURCE_SPLITTER_FLAG_NONE;

    wchar_t *folder = GetStoreFilePath(caller->IsIptv() ? L"MPIPTVSource" : L"MPUrlSourceSplitter", caller->configuration);
    if (folder != NULL)
    {
      // replace cache folder in configuration parameters with properly formatted folder
      CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->configuration->Update(PARAMETER_NAME_CACHE_FOLDER, true, folder), caller->loadAsyncResult, E_OUTOFMEMORY);
    }

    FREE_MEM(folder);
  }

  while ((!fakeIptvUrl) && SUCCEEDED(caller->loadAsyncResult) && (!caller->loadAsyncWorkerShouldExit))
  {
    caller->loadAsyncResult = caller->parserHoster->StartReceivingDataAsync(caller->configuration);

    if (caller->loadAsyncResult == S_FALSE)
    {
      // still pending result, wait some time
      Sleep(1);
    }
    else
    {
      // S_OK or error, doesn't matter
      caller->flags |= SUCCEEDED(caller->loadAsyncResult) ? MP_URL_SOURCE_SPLITTER_FLAG_STREAM_OPENED : MP_URL_SOURCE_SPLITTER_FLAG_NONE;
      break;
    }
  }

  if ((!fakeIptvUrl) && SUCCEEDED(caller->loadAsyncResult))
  {
    // create demuxers

    CHECK_POINTER_HRESULT(caller->loadAsyncResult, caller->demuxers, caller->loadAsyncResult, E_OUTOFMEMORY);

    if (SUCCEEDED(caller->loadAsyncResult))
    {
      CLockMutex lock(caller->demuxersMutex, INFINITE);

      CStreamInformationCollection *streams = new CStreamInformationCollection(&caller->loadAsyncResult);
      CHECK_POINTER_HRESULT(caller->loadAsyncResult, streams, caller->loadAsyncResult, E_OUTOFMEMORY);

      if (SUCCEEDED(caller->loadAsyncResult))
      {
        caller->loadAsyncResult = caller->parserHoster->GetStreamInformation(streams);

        if (SUCCEEDED(caller->loadAsyncResult) && (caller->demuxers->Count() != streams->Count()))
        {
          caller->demuxers->Clear();

          for (unsigned int i = 0; (SUCCEEDED(caller->loadAsyncResult) && (i < streams->Count())); i++)
          {
            CDemuxer *demuxer = NULL;

            if (caller->IsSplitter() && (!caller->IsDownloadingFile()))
            {
              if (streams->GetItem(i)->IsContainer())
              {
                demuxer = new CContainerDemuxer(&caller->loadAsyncResult, caller->logger, caller, caller->configuration);
              }
              else if (streams->GetItem(i)->IsPackets())
              {
                demuxer = new CPacketDemuxer(&caller->loadAsyncResult, caller->logger, caller, caller->configuration);
              }

              if (SUCCEEDED(caller->loadAsyncResult))
              {
                CStandardDemuxer *standardDemuxer = dynamic_cast<CStandardDemuxer *>(demuxer);

                standardDemuxer->SetDemuxerId(i);
                caller->loadAsyncResult = standardDemuxer->SetStreamInformation(streams->GetItem(i));
              }
            }
            else
            {
              demuxer = new CDefaultDemuxer(&caller->loadAsyncResult, caller->logger, caller, caller->configuration);

              if (SUCCEEDED(caller->loadAsyncResult))
              {
                demuxer->SetDemuxerId(i);
              }
            }

            CHECK_POINTER_HRESULT(caller->loadAsyncResult, demuxer, caller->loadAsyncResult, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->demuxers->Add(demuxer), caller->loadAsyncResult, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(demuxer));
          }
        }
      }

      FREE_MEM_CLASS(streams);
    }
    
    if (SUCCEEDED(caller->loadAsyncResult))
    {
      // we start to create first demuxer, then next, etc.
      unsigned int activeDemuxer = 0;

      while (SUCCEEDED(caller->loadAsyncResult) && (!caller->loadAsyncWorkerShouldExit))
      {
        CDemuxer *demuxer = caller->demuxers->GetItem(activeDemuxer);

        if (!demuxer->HasStartedCreatingDemuxer())
        {
          caller->loadAsyncResult = demuxer->StartCreatingDemuxer();
        }

        if (demuxer->IsCreatedDemuxer() || demuxer->IsCreateDemuxerWorkerFinished())
        {
          activeDemuxer++;
          caller->loadAsyncResult = demuxer->GetCreateDemuxerError();
        }

        if (activeDemuxer >= caller->demuxers->Count())
        {
          // all demuxers are created, we finished our work
          break;
        }

        Sleep(1);
      }

      if (SUCCEEDED(caller->loadAsyncResult) && caller->IsIptv() && (!caller->loadAsyncWorkerShouldExit) && (activeDemuxer >= caller->demuxers->Count()))
      {
        // all demuxers successfully created
        // initialize output pins

        for (unsigned int i = 0; (SUCCEEDED(caller->loadAsyncResult) && (i < caller->demuxers->Count())); i++)
        {
          CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(caller->demuxers->GetItem(i));

          for (unsigned int j = 0; (SUCCEEDED(caller->loadAsyncResult) && (demuxer != NULL) && (j < caller->outputPins->Count())); j++)
          {
            CMPUrlSourceSplitterOutputPin *outputPin = caller->outputPins->GetItem(j);

            CHECK_HRESULT_EXECUTE(caller->loadAsyncResult, caller->loadAsyncResult = outputPin->SetVideoStreams(demuxer->GetDemuxerId(), demuxer->GetStreams(CStream::Video)));
            CHECK_HRESULT_EXECUTE(caller->loadAsyncResult, caller->loadAsyncResult = outputPin->SetAudioStreams(demuxer->GetDemuxerId(), demuxer->GetStreams(CStream::Audio)));
            CHECK_HRESULT_EXECUTE(caller->loadAsyncResult, caller->loadAsyncResult = outputPin->SetSubtitleStreams(demuxer->GetDemuxerId(), demuxer->GetStreams(CStream::Subpic)));
          }
        }
      }

      if (SUCCEEDED(caller->loadAsyncResult) && (caller->IsSplitter()) && (!caller->IsDownloadingFile()) && (!caller->loadAsyncWorkerShouldExit) && (activeDemuxer >= caller->demuxers->Count()))
      {
        // all demuxers successfully created
        // initialize output pins

        caller->demuxNewStart = 0;
        caller->demuxStart = 0;

        // select video stream
        for (unsigned int i = 0; (SUCCEEDED(caller->loadAsyncResult) && (i < caller->demuxers->Count())); i++)
        {
          CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(caller->demuxers->GetItem(i));
          CStream *videoStream = demuxer->SelectVideoStream();

          if (videoStream != NULL)
          {
            CMPUrlSourceSplitterOutputSplitterPin *outputPin = new CMPUrlSourceSplitterOutputSplitterPin(L"Video", caller, caller, &caller->loadAsyncResult, caller->logger, caller->configuration, videoStream->GetStreamInfo()->GetMediaTypes(), demuxer->GetContainerFormat());
            CHECK_POINTER_HRESULT(caller->loadAsyncResult, outputPin, caller->loadAsyncResult, E_OUTOFMEMORY);

            if (SUCCEEDED(caller->loadAsyncResult))
            {
              outputPin->SetStreamPid(videoStream->GetPid());
              outputPin->SetDemuxerId(i);
              caller->loadAsyncResult = outputPin->SetVideoStreams(demuxer->GetDemuxerId(), demuxer->GetStreams(CStream::Video));

              demuxer->SetActiveStream(CStream::Video, videoStream->GetPid());
            }

            CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->outputPins->Add(outputPin), caller->loadAsyncResult, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(outputPin));
            CHECK_CONDITION_EXECUTE(SUCCEEDED(caller->loadAsyncResult), caller->logger->Log(LOGGER_INFO, L"%s: %s: created video output pin, demuxer: %u, stream ID: %u", MODULE_NAME, METHOD_LOAD_ASYNC_WORKER_NAME, i, videoStream->GetPid()));
            break;
          }
        }

        // select audio stream
        for (unsigned int i = 0; (SUCCEEDED(caller->loadAsyncResult) && (i < caller->demuxers->Count())); i++)
        {
          CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(caller->demuxers->GetItem(i));
          CStream *audioStream = demuxer->SelectAudioStream();

          if (audioStream != NULL)
          {
            CMPUrlSourceSplitterOutputSplitterPin *outputPin = new CMPUrlSourceSplitterOutputSplitterPin(L"Audio", caller, caller, &caller->loadAsyncResult, caller->logger, caller->configuration, audioStream->GetStreamInfo()->GetMediaTypes(), demuxer->GetContainerFormat());
            CHECK_POINTER_HRESULT(caller->loadAsyncResult, outputPin, caller->loadAsyncResult, E_OUTOFMEMORY);

            if (SUCCEEDED(caller->loadAsyncResult))
            {
              outputPin->SetStreamPid(audioStream->GetPid());
              outputPin->SetDemuxerId(i);
              caller->loadAsyncResult = outputPin->SetAudioStreams(demuxer->GetDemuxerId(), demuxer->GetStreams(CStream::Audio));

              demuxer->SetActiveStream(CStream::Audio, audioStream->GetPid());
            }

            CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->outputPins->Add(outputPin), caller->loadAsyncResult, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(outputPin));
            CHECK_CONDITION_EXECUTE(SUCCEEDED(caller->loadAsyncResult), caller->logger->Log(LOGGER_INFO, L"%s: %s: created audio output pin, demuxer: %u, stream ID: %u", MODULE_NAME, METHOD_LOAD_ASYNC_WORKER_NAME, i, audioStream->GetPid()));
            break;
          }
        }

        // select subtitle stream
        for (unsigned int i = 0; (SUCCEEDED(caller->loadAsyncResult) && (i < caller->demuxers->Count())); i++)
        {
          CStandardDemuxer *demuxer = dynamic_cast<CStandardDemuxer *>(caller->demuxers->GetItem(i));

          // if there are some subtitles, just choose first and create output pin
          CStream *subtitleStream = (demuxer->GetStreams(CStream::Subpic)->Count() != 0) ? demuxer->GetStreams(CStream::Subpic)->GetItem(0) : NULL;

          if (subtitleStream != NULL)
          {
            CMPUrlSourceSplitterOutputSplitterPin *outputPin = new CMPUrlSourceSplitterOutputSplitterPin(L"Subtitle", caller, caller, &caller->loadAsyncResult, caller->logger, caller->configuration, subtitleStream->GetStreamInfo()->GetMediaTypes(), demuxer->GetContainerFormat());
            CHECK_POINTER_HRESULT(caller->loadAsyncResult, outputPin, caller->loadAsyncResult, E_OUTOFMEMORY);

            if (SUCCEEDED(caller->loadAsyncResult))
            {
              outputPin->SetStreamPid(subtitleStream->GetPid());
              outputPin->SetDemuxerId(i);
              caller->loadAsyncResult = outputPin->SetSubtitleStreams(demuxer->GetDemuxerId(), demuxer->GetStreams(CStream::Subpic));

              demuxer->SetActiveStream(CStream::Subpic, subtitleStream->GetPid());
            }

            CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->outputPins->Add(outputPin), caller->loadAsyncResult, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(outputPin));
            CHECK_CONDITION_EXECUTE(SUCCEEDED(caller->loadAsyncResult), caller->logger->Log(LOGGER_INFO, L"%s: %s: created subtitle output pin, demuxer: %u, stream ID: %u", MODULE_NAME, METHOD_LOAD_ASYNC_WORKER_NAME, i, subtitleStream->GetPid()));
            break;
          }
        }

        if (SUCCEEDED(caller->loadAsyncResult))
        {
          CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->outputPins->Count() > 0, caller->loadAsyncResult, E_FAIL);

          // if there are no pins, then it is bad
          if (FAILED(caller->loadAsyncResult))
          {
            caller->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_LOAD_ASYNC_WORKER_NAME, L"no output in any of demuxers, no output pin created");
          }
        }
      }
      else if (SUCCEEDED(caller->loadAsyncResult) && (caller->IsSplitter()) && (caller->IsDownloadingFile()) && (!caller->loadAsyncWorkerShouldExit) && (activeDemuxer >= caller->demuxers->Count()))
      {
        // we are downloading file
        // create output pin, which save received data to file

        CMediaTypeCollection *mediaTypes = new CMediaTypeCollection(&caller->loadAsyncResult);
        CHECK_POINTER_HRESULT(caller->loadAsyncResult, mediaTypes, caller->loadAsyncResult, E_OUTOFMEMORY);

        if (SUCCEEDED(caller->loadAsyncResult))
        {
          CMediaType *mediaType = new CMediaType();
          CHECK_POINTER_HRESULT(caller->loadAsyncResult, mediaTypes, caller->loadAsyncResult, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(caller->loadAsyncResult, mediaTypes->Add(mediaType), caller->loadAsyncResult, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(mediaType));
        }

        if (SUCCEEDED(caller->loadAsyncResult))
        {
          CMPUrlSourceSplitterOutputDownloadPin *outputPin = new CMPUrlSourceSplitterOutputDownloadPin(PathFindFileName(caller->downloadFileName), caller, caller, &caller->loadAsyncResult, caller->logger, caller->configuration, mediaTypes, caller->downloadFileName);
          CHECK_POINTER_HRESULT(caller->loadAsyncResult, outputPin, caller->loadAsyncResult, E_OUTOFMEMORY);

          if (SUCCEEDED(caller->loadAsyncResult))
          {
            outputPin->SetStreamPid(caller->demuxers->GetItem(0)->GetDemuxerId());
            outputPin->SetDemuxerId(0);
          }

          CHECK_CONDITION_HRESULT(caller->loadAsyncResult, caller->outputPins->Add(outputPin), caller->loadAsyncResult, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(caller->loadAsyncResult), FREE_MEM_CLASS(outputPin));
        }

        FREE_MEM_CLASS(mediaTypes);
      }

      if (SUCCEEDED(caller->loadAsyncResult))
      {
        // start all demuxers to demux their streams
        for (unsigned int i = 0; (SUCCEEDED(caller->loadAsyncResult) && (i < caller->demuxers->Count())); i++)
        {
          CDemuxer *demuxer = caller->demuxers->GetItem(i);

          // don't demux streams until CMD_PLAY command is received
          demuxer->SetPauseSeekStopRequest(true);
          caller->loadAsyncResult = demuxer->StartDemuxing();
        }
      }

      if (SUCCEEDED(caller->loadAsyncResult) && (caller->IsDownloadingFile()))
      {
        // start downloading and storing file
        caller->Run(0);
      }
    }
  }

  caller->logger->Log(SUCCEEDED(caller->loadAsyncResult) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(caller->loadAsyncResult) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LOAD_ASYNC_WORKER_NAME, caller->loadAsyncResult);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

void CMPUrlSourceSplitter::SetPauseSeekStopRequest(bool pauseSeekStopRequest)
{
  this->pauseSeekStopRequest = pauseSeekStopRequest;

  {
    CLockMutex lock(this->demuxersMutex, INFINITE);

    for (unsigned int i = 0; i < this->demuxers->Count(); i++)
    {
      this->demuxers->GetItem(i)->SetPauseSeekStopRequest(pauseSeekStopRequest);
    }
  }
}

void CMPUrlSourceSplitter::ClearSession(bool withLogger)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CLEAR_SESSION_NAME);

  // stop async loading
  this->DestroyLoadAsyncWorker();

  // clear all demuxers
  this->demuxers->Clear();

  // stops receiving data
  this->Stop();

  // clear all parsers and protocols
  this->parserHoster->ClearSession();

  // clear all flags instead of filter type (IPTV or splitter)
  this->flags &= (MP_URL_SOURCE_SPLITTER_FLAG_AS_IPTV | MP_URL_SOURCE_SPLITTER_FLAG_AS_SPLITTER);

  // in case of splitter delete outputs
  if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_FLAG_AS_SPLITTER))
  {
    if (this->outputPins != NULL)
    {
      for (unsigned int i = 0; i < this->outputPins->Count(); i++)
      {
        CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);
        IPin *connectedPin = outputPin->GetConnected();

        CHECK_CONDITION_NOT_NULL_EXECUTE(connectedPin, connectedPin->Disconnect());
        outputPin->Disconnect();
      }

      this->outputPins->Clear();
    }
  }

  this->lastCommand = -1;
  this->pauseSeekStopRequest = false;
  this->loadAsyncResult = S_OK;

  FREE_MEM(this->downloadFileName);

  this->demuxStart = 0;
  this->demuxNewStart = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CLEAR_SESSION_NAME);

  CHECK_CONDITION_EXECUTE(withLogger, this->logger->Clear());
}