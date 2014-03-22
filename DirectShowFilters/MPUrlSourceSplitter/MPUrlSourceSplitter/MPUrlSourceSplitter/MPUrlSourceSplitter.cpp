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
#define METHOD_THREAD_PROC_NAME                                   L"ThreadProc()"

#define METHOD_STOP_NAME                                          L"Stop()"
#define METHOD_CLOSE_NAME                                         L"Close()"
#define METHOD_PAUSE_NAME                                         L"Pause()"
#define METHOD_RUN_NAME                                           L"Run()"

#define METHOD_SET_POSITIONS_NAME                                 L"SetPositions()"

#define METHOD_PARSE_PARAMETERS_NAME                              L"ParseParameters()"
#define METHOD_LOAD_PLUGINS_NAME                                  L"LoadPlugins()"
#define METHOD_SET_TOTAL_LENGTH_NAME                              L"SetTotalLength()"

#define METHOD_LENGTH_NAME                                        L"Length()"
#define METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME            L"CreateDemuxerReadRequestWorker()"
#define METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME           L"DestroyDemuxerReadRequestWorker()"
#define METHOD_DEMUXER_READ_REQUEST_WORKER_NAME                   L"DemuxerReadRequestWorker()"

#define METHOD_DOWNLOAD_NAME                                      L"Download()"
#define METHOD_DOWNLOAD_ASYNC_NAME                                L"DownloadAsync()"
#define METHOD_DOWNLOAD_CALLBACK_NAME                             L"OnDownloadCallback()"

#define METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME                  L"CreateCreateDemuxerWorker()"
#define METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME                 L"DestroyCreateDemuxerWorker()"
#define METHOD_CREATE_DEMUXER_WORKER_NAME                         L"CreateDemuxerWorker()"

#define METHOD_DEMUXER_SEEK_NAME                                  L"DemuxerSeek()"
#define METHOD_DEMUXER_READ_NAME                                  L"DemuxerRead()"

#define METHOD_ENABLE_NAME                                        L"Enable()"

#define PARAMETER_SEPARATOR                                       L"&"
#define PARAMETER_IDENTIFIER                                      L"####"
#define PARAMETER_ASSIGN                                          L"="

#define DEMUXER_READ_BUFFER_SIZE								  32768

extern "C" char *curl_easy_unescape(void *handle, const char *string, int length, int *olen);
extern "C" void curl_free(void *p);

// if ffmpegLogCallbackSet is true than ffmpeg log callback will not be set
// in that case we don't receive messages from ffmpeg
static volatile bool ffmpegLogCallbackSet = false;

// specifies if FFmpeg was initialized or not
static volatile bool ffmpegInitialized = false;

extern "C++" CLogger *ffmpegLoggerInstance = NULL;
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
  : CBaseFilter(pName, pUnk, this, clsid, phr)
#pragma warning(pop)
  , lastCommand(-1)
  , pauseSeekStopRequest(false)
  , logger(NULL)
  , outputPins(NULL)
  , flags(FLAG_MP_URL_SOURCE_SPLITTER_NONE)
  , demuxerContextBufferPosition(0)
  , demuxerContext(NULL)
  , demuxer(NULL)
  , asyncDownloadResult(S_OK)
  , asyncDownloadCallback(NULL)
  , downloadFileName(NULL)
  , storeFilePath(NULL)
  , createDemuxerWorkerShouldExit(false)
  , createDemuxerWorkerThread(NULL)
  , demuxerReadRequest(NULL)
  , demuxerReadRequestMutex(NULL)
  , demuxerReadRequestWorkerShouldExit(false)
  , demuxerReadRequestId(0)
  , demuxerReadRequestWorkerThread(NULL)
  , demuxStart(0)
  , demuxStop(0)
  , demuxRate(1.0)
  , demuxCurrent(0)
  , demuxNewStart(0)
  , demuxNewStop(0)
  , seekingLastStart(_I64_MIN)
  , seekingLastStop(_I64_MIN)
  , mediaPacketCollection(NULL)
  , mediaPacketMutex(NULL)
{
  CParameterCollection *loggerParameters = new CParameterCollection();
  CHECK_POINTER_HRESULT(*phr, loggerParameters, *phr, E_OUTOFMEMORY);

  if (SUCCEEDED(*phr))
  {
    if (IsEqualGUID(GUID_MP_IPTV_SOURCE, clsid))
    {
      wchar_t *logFile = GetTvServerFilePath(MP_IPTV_SOURCE_LOG_FILE);
      wchar_t *logBackupFile = GetTvServerFilePath(MP_IPTV_SOURCE_LOG_BACKUP_FILE);

      CHECK_CONDITION_EXECUTE(!loggerParameters->Add(PARAMETER_NAME_LOG_FILE_NAME, logFile), *phr = E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(!loggerParameters->Add(PARAMETER_NAME_LOG_BACKUP_FILE_NAME, logBackupFile), *phr = E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(!loggerParameters->Add(PARAMETER_NAME_LOG_GLOBAL_MUTEX_NAME, MP_IPTV_SOURCE_GLOBAL_MUTEX_NAME), *phr = E_OUTOFMEMORY);

      FREE_MEM(logFile);
      FREE_MEM(logBackupFile);
    }
    else if (IsEqualGUID(GUID_MP_URL_SOURCE_SPLITTER, clsid))
    {
      wchar_t *logFile = GetMediaPortalFilePath(MP_URL_SOURCE_SPLITTER_LOG_FILE);
      wchar_t *logBackupFile = GetMediaPortalFilePath(MP_URL_SOURCE_SPLITTER_LOG_BACKUP_FILE);

      CHECK_CONDITION_EXECUTE(!loggerParameters->Add(PARAMETER_NAME_LOG_FILE_NAME, logFile), *phr = E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(!loggerParameters->Add(PARAMETER_NAME_LOG_BACKUP_FILE_NAME, logBackupFile), *phr = E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(!loggerParameters->Add(PARAMETER_NAME_LOG_GLOBAL_MUTEX_NAME, MP_URL_SOURCE_SPLITTER_GLOBAL_MUTEX_NAME), *phr = E_OUTOFMEMORY);

      FREE_MEM(logFile);
      FREE_MEM(logBackupFile);
    }
    else
    {
      *phr = E_INVALIDARG;
    }
  }

  if (SUCCEEDED(*phr))
  {
    this->logger = new CLogger(staticLogger, loggerParameters);
    CHECK_POINTER_HRESULT(*phr, this->logger, *phr, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(*phr))
  {
    this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);

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

    this->outputPins = new CMPUrlSourceSplitterOutputPinCollection();

    CHECK_POINTER_HRESULT(*phr, this->outputPins, *phr, E_OUTOFMEMORY);

    if (SUCCEEDED(*phr))
    {
      this->configuration = new CParameterCollection();
      CHECK_POINTER_HRESULT(*phr, this->configuration, *phr, E_OUTOFMEMORY);

      this->mediaPacketCollection = new CMediaPacketCollection();
      CHECK_POINTER_HRESULT(*phr, this->mediaPacketCollection, *phr, E_OUTOFMEMORY);

      this->totalLength = 0;

      this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_ESTIMATE_TOTAL_LENGTH;

      this->demuxerReadRequestMutex = CreateMutex(NULL, FALSE, NULL);
      this->mediaPacketMutex = CreateMutex(NULL, FALSE, NULL);
      this->lastReceivedMediaPacketTime = GetTickCount();

      CHECK_POINTER_HRESULT(*phr, this->demuxerReadRequestMutex, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->mediaPacketMutex, *phr, E_OUTOFMEMORY);

      this->parserHoster = new CParserHoster(this->logger, loggerParameters, this);
      CHECK_POINTER_HRESULT(*phr, this->parserHoster, *phr, E_OUTOFMEMORY);

      if (SUCCEEDED(*phr))
      {
        this->parserHoster->LoadPlugins();
      }

      if (!ffmpegInitialized)
      {
        // initialize FFmpeg
        av_register_all();

        ffmpegInitialized = true;
      }

      if (!ffmpegLogCallbackSet)
      {
        // callback for ffmpeg log is not set
        av_log_set_callback(FFmpegLogCallback);
        av_log_set_level(AV_LOG_DEBUG);

        // for FFmpeg logger instance we assume that IPTV filter will be used with TvServer and splitter will be used with MediaPortal (different processes)
        // if both will be used in one process than FFmpeg logger instance can write to another log file
        // it depends on which filter will be created first

        // create FFmpeg logger instance
        // logger instance is cleared while unloading DLL (dllmain.cpp)
        ffmpegLoggerInstance = new CLogger(this->logger);

        ffmpegLogCallbackSet = true;
      }
    }

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
  }

  FREE_MEM_CLASS(loggerParameters);
}

CMPUrlSourceSplitter::~CMPUrlSourceSplitter()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);

  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // Close() method finish demuxer thread, disconnects output pins, clears output pin collection,
  // destroys demuxer context and resets demuxer context buffer position
  this->Close();

  // destroy demuxer read request worker
  this->DestroyDemuxerReadRequestWorker();

  FREE_MEM_CLASS(this->outputPins);

  if (this->parserHoster != NULL)
  {
    this->parserHoster->StopReceivingData();
    this->parserHoster->RemoveAllPlugins();
    FREE_MEM_CLASS(this->parserHoster);
  }

  FREE_MEM_CLASS(this->demuxerReadRequest);
  FREE_MEM_CLASS(this->mediaPacketCollection);

  if (this->demuxerReadRequestMutex != NULL)
  {
    CloseHandle(this->demuxerReadRequestMutex);
    this->demuxerReadRequestMutex = NULL;
  }
  
  if (this->mediaPacketMutex != NULL)
  {
    CloseHandle(this->mediaPacketMutex);
    this->mediaPacketMutex = NULL;
  }

  if ((!this->IsDownloadingFile()) && (this->storeFilePath != NULL))
  {
    DeleteFile(this->storeFilePath);
  }

  FREE_MEM(this->storeFilePath);

  FREE_MEM_CLASS(this->configuration);
  FREE_MEM(this->downloadFileName);

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
    instance->flags |= FLAG_MP_URL_SOURCE_SPLITTER_AS_IPTV;

    // if in output pin collection isn't any pin, then add new output pin with MPEG2 TS media type
    // in another case filter assume that there is only one output pin with MPEG2 TS media type
    if (instance->outputPins->Count() == 0)
    {
      // create valid MPEG2 TS media type, add it to media types and create output pin
      CMediaTypeCollection *mediaTypes = new CMediaTypeCollection();
      CHECK_POINTER_HRESULT(*phr, mediaTypes, *phr, E_OUTOFMEMORY);

      if (SUCCEEDED(*phr))
      {
        CMediaType *mediaType = new CMediaType();
        CHECK_POINTER_HRESULT(*phr, mediaType, *phr, E_OUTOFMEMORY);

        if (SUCCEEDED(*phr))
        {
          mediaType->SetType(&MEDIATYPE_Stream);
          mediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);

          *phr = (mediaTypes->Add(mediaType)) ? (*phr) : E_OUTOFMEMORY;
        }

        CHECK_CONDITION_EXECUTE(FAILED(*phr), FREE_MEM_CLASS(mediaType));
      }

      if (SUCCEEDED(*phr))
      {
        CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputPin(mediaTypes, L"Output", instance, instance, phr, L"mpegts");
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
    punk->flags |= FLAG_MP_URL_SOURCE_SPLITTER_AS_SPLITTER;
  }

  return punk;
}

// IUnknown

STDMETHODIMP CMPUrlSourceSplitter::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

  *ppv = NULL;

  /*if (m_pDemuxer && (riid == __uuidof(IKeyFrameInfo) || riid == __uuidof(ITrackInfo) || riid == IID_IAMExtendedSeeking)) {
    return m_pDemuxer->QueryInterface(riid, ppv);
  }*/

  if (this->IsIptv())
  {
    return
      QI(IFileSourceFilter)
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
      //QI2(ISpecifyPropertyPages)
      //QI2(ILAVFSettings)
      //QI2(ILAVFSettingsInternal)
      //QI(IObjectWithSite)
      //QI(IBufferInfo)
      QI(IFilterState)
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

  this->flags &= ~(FLAG_MP_URL_SOURCE_SPLITTER_PLAYBACK_STARTED | FLAG_MP_URL_SOURCE_SPLITTER_REPORT_STREAM_TIME);

  this->pauseSeekStopRequest = true;
  CAMThread::CallWorker(CMD_EXIT);
  this->pauseSeekStopRequest = false;

  this->DeliverBeginFlush();
  CAMThread::Close();
  this->DeliverEndFlush();

  if (!this->IsEnabledMethodActive())
  {
    // if we are not changing streams stop receiving data, data are not needed
    this->parserHoster->StopReceivingData();
  }

  HRESULT result = __super::Stop();

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_STOP_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::Pause()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_PAUSE_NAME);

  this->flags &= ~FLAG_MP_URL_SOURCE_SPLITTER_REPORT_STREAM_TIME;

  this->pauseSeekStopRequest = true;
  CAMThread::CallWorker(CMD_PAUSE);
  this->pauseSeekStopRequest = false;

  CAutoLock cAutoLock(this);

  FILTER_STATE fs = m_State;
  HRESULT result = __super::Pause();

  // the filter graph will set us to pause before running
  // so if we were stopped before, create demuxing thread
  if (SUCCEEDED(result) && (fs == State_Stopped))
  {
    // At this point, the graph is hopefully finished, tell the demuxer about all the cool things
    //m_pDemuxer->SettingsChanged(static_cast<ILAVFSettingsInternal *>(this));

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
  this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_PLAYBACK_STARTED | FLAG_MP_URL_SOURCE_SPLITTER_REPORT_STREAM_TIME;

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_RUN_NAME, result);
  return S_OK;
}

// IFileSourceFilter

STDMETHODIMP CMPUrlSourceSplitter::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE * pmt)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_LOAD_NAME);
  HRESULT result = E_NOT_VALID_STATE;

  if (this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_AS_IPTV))
  {
    result = S_OK;
    CHECK_POINTER_DEFAULT_HRESULT(result, pszFileName);

    // FAKE for UDP protocol request from MediaPortal
    if (_wcsnicmp(pszFileName, L"udp://@0.0.0.0:1234", 19) != 0)
    {
      CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

      if (SUCCEEDED(result))
      {
        // stop receiving data
        this->parserHoster->StopReceivingData();
      }

      this->ClearSession();

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
        // IPTV is always live stream, remove live stream flag and create new one
        this->configuration->Remove(PARAMETER_NAME_LIVE_STREAM, true);
        result = this->configuration->Add(PARAMETER_NAME_LIVE_STREAM, L"1") ? result : E_OUTOFMEMORY;
      }

      if (SUCCEEDED(result))
      {
        // loads protocol based on current configuration parameters
        // it also reset all parser and protocol implementations
        result = this->Load();
      }

      FREE_MEM(url);
    }

    // if in output pin collection isn't any pin, then add new output pin with MPEG2 TS media type
    // in another case filter assume that there is only one output pin with MPEG2 TS media type
    if (SUCCEEDED(result) && (this->outputPins->Count() == 0))
    {
      // create valid MPEG2 TS media type, add it to media types and create output pin
      CMediaTypeCollection *mediaTypes = new CMediaTypeCollection();
      CHECK_POINTER_HRESULT(result, mediaTypes, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        CMediaType *mediaType = new CMediaType();
        CHECK_POINTER_HRESULT(result, mediaType, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          mediaType->SetType(&MEDIATYPE_Stream);
          mediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_TRANSPORT);

          result = (mediaTypes->Add(mediaType)) ? result : E_OUTOFMEMORY;
        }

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaType));
      }

      if (SUCCEEDED(result))
      {
        CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputPin(mediaTypes, L"Output", this, this, &result, L"mpegts");
        CHECK_POINTER_HRESULT(result, outputPin, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->outputPins->Add(outputPin), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(outputPin));
      }

      FREE_MEM_CLASS(mediaTypes);
    }
  }
  else if (this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_AS_SPLITTER))
  {
    result = S_OK;
    CHECK_POINTER_DEFAULT_HRESULT(result, pszFileName);

    // destroy create demuxer worker (if not finished earlier)
    this->DestroyCreateDemuxerWorker();

    // Close() method finish demuxer thread, disconnects output pins, clears output pin collection,
    // destroys demuxer context and resets demuxer context buffer position
    this->Close();

    // destroy demuxer read request worker
    this->DestroyDemuxerReadRequestWorker();

    if (SUCCEEDED(result))
    {
      CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

      if (SUCCEEDED(result))
      {
        this->DestroyCreateDemuxerWorker();

        // stop receiving data
        this->parserHoster->StopReceivingData();
      }

      this->ClearSession();

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
        // loads protocol based on current configuration parameters
        result = this->Load();
      }

      // create demuxer read request worker
      // it is needed also to store file for downloading
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->CreateDemuxerReadRequestWorker());

      if (SUCCEEDED(result) && (!this->IsDownloadingFile()))
      {
        // splitter is not needed when downloading file
        result = this->CreateCreateDemuxerWorker();
      }

      FREE_MEM(url);
    }

    // output pins are created after demuxer is created
    // now we don't know nothing about video/audio/other stream types
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

// IOutputStream

HRESULT CMPUrlSourceSplitter::SetTotalLength(int64_t total, bool estimate)
{
  HRESULT result = E_FAIL;

  {
    CLockMutex lock(this->mediaPacketMutex, INFINITE);

    this->totalLength = total;

    this->flags &= ~FLAG_MP_URL_SOURCE_SPLITTER_ESTIMATE_TOTAL_LENGTH;
    this->flags |= (estimate) ? FLAG_MP_URL_SOURCE_SPLITTER_ESTIMATE_TOTAL_LENGTH : FLAG_MP_URL_SOURCE_SPLITTER_NONE;

    result = S_OK;
  }

  return result;
}

HRESULT CMPUrlSourceSplitter::PushMediaPackets(CMediaPacketCollection *mediaPackets)
{
  HRESULT result = S_OK;

  // in case of splitter we process all media packets
  // in case of IPTV we assume that CMD_PLAY request come ASAP after Load() method is finished
  {
    CLockMutex lock(this->mediaPacketMutex, INFINITE);
    HRESULT result = S_OK;

    // remember last received media packet time
    this->lastReceivedMediaPacketTime = GetTickCount();

    CHECK_POINTER_DEFAULT_HRESULT(result, mediaPackets);

    for (unsigned int i = 0; (SUCCEEDED(result)) && (i < mediaPackets->Count()); i++)
    {
      CMediaPacket *mediaPacket = mediaPackets->GetItem(i);

      CMediaPacketCollection *unprocessedMediaPackets = new CMediaPacketCollection();
      if (unprocessedMediaPackets->Add(mediaPacket->Clone()))
      {
        int64_t start = mediaPacket->GetStart();
        int64_t stop = mediaPacket->GetEnd();

        result = S_OK;
        while ((unprocessedMediaPackets->Count() != 0) && (result == S_OK))
        {
          // there is still some unprocessed media packets
          // get first media packet
          CMediaPacket *unprocessedMediaPacket = unprocessedMediaPackets->GetItem(0)->Clone();

          // remove first unprocessed media packet
          // its clone is going to be processed
          unprocessedMediaPackets->Remove(0);

          int64_t unprocessedMediaPacketStart = unprocessedMediaPacket->GetStart();
          int64_t unprocessedMediaPacketEnd = unprocessedMediaPacket->GetEnd();

          // try to find overlapping region
          CMediaPacket *region = this->mediaPacketCollection->GetOverlappedRegion(unprocessedMediaPacket);
          if (region != NULL)
          {
            if ((region->GetStart() == 0) && (region->GetEnd() == 0))
            {
              // there isn't overlapping media packet
              // whole packet can be added to collection
              result = (this->mediaPacketCollection->Add(unprocessedMediaPacket->Clone())) ? S_OK : E_FAIL;
            }
            else
            {
              // current unprocessed media packet is overlapping some media packet in media packet collection
              // it means that this packet has same data (in overlapping range)
              // there is no need to duplicate data in collection

              int64_t overlappingRegionStart = region->GetStart();
              int64_t overlappingRegionEnd = region->GetEnd();

              if (SUCCEEDED(result) && (unprocessedMediaPacketStart < overlappingRegionStart))
              {
                // initialize part
                int64_t start = unprocessedMediaPacketStart;
                int64_t end = overlappingRegionStart - 1;
                CMediaPacket *part = unprocessedMediaPacket->CreateMediaPacketBasedOnPacket(start, end);

                result = (part != NULL) ? S_OK : E_POINTER;
                if (SUCCEEDED(result))
                {
                  result = (unprocessedMediaPackets->Add(part)) ? S_OK : E_FAIL;
                }
              }

              if (SUCCEEDED(result) && (unprocessedMediaPacketEnd > overlappingRegionEnd))
              {
                // initialize part
                int64_t start = overlappingRegionEnd + 1;
                int64_t end = unprocessedMediaPacketEnd;
                CMediaPacket *part = unprocessedMediaPacket->CreateMediaPacketBasedOnPacket(start, end);

                result = (part != NULL) ? S_OK : E_POINTER;
                if (SUCCEEDED(result))
                {
                  result = (unprocessedMediaPackets->Add(part)) ? S_OK : E_FAIL;
                }
              }
            }
          }
          else
          {
            // there is serious error
            result = E_FAIL;
          }
          FREE_MEM_CLASS(region);

          // delete processed media packet
          FREE_MEM_CLASS(unprocessedMediaPacket);
        }
      }

      // media packets collection is not longer needed
      FREE_MEM_CLASS(unprocessedMediaPackets);
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, result));
  return result;
}

HRESULT CMPUrlSourceSplitter::EndOfStreamReached(int64_t streamPosition)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME);

  HRESULT result = E_FAIL;

  {
    CLockMutex mediaPacketLock(this->mediaPacketMutex, INFINITE);

    if (this->mediaPacketCollection->Count() > 0)
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: media packet count: %u, stream position: %llu", MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, this->mediaPacketCollection->Count(), streamPosition);

      // check media packets from supplied last valid stream position
      int64_t startPosition = 0;
      int64_t endPosition = 0;
      unsigned int mediaPacketIndex = this->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(streamPosition);

      if (mediaPacketIndex != UINT_MAX)
      {
        CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(mediaPacketIndex);
        startPosition = mediaPacket->GetStart();
        endPosition = mediaPacket->GetEnd();
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: for stream position '%llu' found media packet, start: %llu, end: %llu", MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, streamPosition, startPosition, endPosition);
      }

      for (int i = 0; i < 2; i++)
      {
        // because collection is sorted
        // then simple going through all media packets will reveal if there is some empty place
        while (mediaPacketIndex != UINT_MAX)
        {
          CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(mediaPacketIndex);
          int64_t mediaPacketStart = mediaPacket->GetStart();
          int64_t mediaPacketEnd = mediaPacket->GetEnd();

          if (startPosition == mediaPacketStart)
          {
            // next start time is next to end of current media packet
            startPosition = mediaPacketEnd + 1;
            mediaPacketIndex++;

            if (mediaPacketIndex >= this->mediaPacketCollection->Count())
            {
              // stop checking, all media packets checked
              endPosition = startPosition;
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: all media packets checked, start: %llu, end: %llu", MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, startPosition, endPosition);
              mediaPacketIndex = UINT_MAX;
            }
          }
          else
          {
            // we found gap between media packets
            // set end time and stop checking media packets
            endPosition = mediaPacketStart - 1;
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found gap between media packets, start: %llu, end: %llu", MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, startPosition, endPosition);
            mediaPacketIndex = UINT_MAX;
          }
        }

        if ((!this->IsEstimateTotalLength()) && (startPosition >= this->totalLength) && (i == 0))
        {
          // we are after end of stream
          // check media packets from start if we don't have gap
          startPosition = 0;
          endPosition = 0;
          mediaPacketIndex = this->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(startPosition);
          this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_TOTAL_LENGTH_RECEIVED;
          this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, L"searching for gap in media packets from beginning");
        }
        else
        {
          // we found some gap
          break;
        }
      }

      if (((!this->IsEstimateTotalLength()) && (startPosition < this->totalLength)) || (this->IsEstimateTotalLength()))
      {
        // found part which is not downloaded
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: requesting stream part from: %llu, to: %llu", MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, startPosition, endPosition);
        this->SeekToPosition(startPosition, endPosition);
      }
      else
      {
        // all data received
        this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_ALL_DATA_RECEIVED;
        this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, L"all data received");

        // if downloading file, download callback can be called after storing all data to download file
      }
    }

    result = S_OK;
  }
  
  this->logger->Log(LOGGER_VERBOSE, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME);
  return result;
}

// IParserOutputStream

bool CMPUrlSourceSplitter::IsDownloading(void)
{
  return this->IsDownloadingFile();
}

void CMPUrlSourceSplitter::FinishDownload(HRESULT result)
{
  this->OnDownloadCallback(result);
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
  CheckPointer(this->demuxer, E_UNEXPECTED);
  
  *pDuration = (this->IsLiveStream() ? (-1) : this->demuxer->GetDuration());

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
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek request; start: %I64d; flags: 0x%08X, stop: %I64d; flags: 0x%08X", MODULE_NAME, METHOD_SET_POSITIONS_NAME, pCurrent ? *pCurrent : -1, dwCurrentFlags, pStop ? *pStop : -1, dwStopFlags);

  HRESULT result = E_FAIL;

  if (((pCurrent == NULL) && (pStop == NULL)) ||
      (((dwCurrentFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_NoPositioning) && ((dwStopFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_NoPositioning)))
  {
      result = S_OK;
  }
  else
  {
    REFERENCE_TIME rtCurrent = this->demuxCurrent, rtStop = this->demuxStop;

    if (pCurrent != NULL)
    {
      switch(dwCurrentFlags & AM_SEEKING_PositioningBitsMask)
      {
      case AM_SEEKING_NoPositioning:
        break;
      case AM_SEEKING_AbsolutePositioning:
        rtCurrent = *pCurrent;
        break;
      case AM_SEEKING_RelativePositioning:
        rtCurrent = rtCurrent + *pCurrent;
        break;
      case AM_SEEKING_IncrementalPositioning:
        rtCurrent = rtCurrent + *pCurrent;
        break;
      }
    }

    if (pStop != NULL)
    {
      switch(dwStopFlags & AM_SEEKING_PositioningBitsMask)
      {
      case AM_SEEKING_NoPositioning:
        break;
      case AM_SEEKING_AbsolutePositioning:
        rtStop = *pStop;
        break;
      case AM_SEEKING_RelativePositioning:
        rtStop += *pStop;
        break;
      case AM_SEEKING_IncrementalPositioning:
        rtStop = rtCurrent + *pStop;
        break;
      }
    }

    if ((this->demuxCurrent == rtCurrent) && (this->demuxStop == rtStop))
    {
      result = S_OK;
    }
    else
    {
      this->seekingLastStart = rtCurrent;
      this->seekingLastStop = rtStop;

      this->demuxNewStart = this->demuxCurrent = rtCurrent;
      this->demuxStop = rtStop;

      // perform seek
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: performing seek to %I64d", MODULE_NAME, METHOD_SET_POSITIONS_NAME, this->demuxNewStart);

      if (ThreadExists())
      {
        this->DeliverBeginFlush();
        this->pauseSeekStopRequest = true;
        CallWorker(CMD_SEEK);
        this->pauseSeekStopRequest = false;
        this->DeliverEndFlush();
      }

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to %I64d finished", MODULE_NAME, METHOD_SET_POSITIONS_NAME, this->demuxNewStart);
      result = S_OK;
    }
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SET_POSITIONS_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::GetPositions(LONGLONG* pCurrent, LONGLONG* pStop)
{
  if (pCurrent)
  {
    *pCurrent = this->demuxCurrent;
  }
  if (pStop)
  {
    *pStop = this->demuxStop;
  }
  return S_OK;
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
  return (dRate > 0) ? this->demuxRate = dRate, S_OK : E_INVALIDARG;
}

STDMETHODIMP CMPUrlSourceSplitter::GetRate(double* pdRate)
{
  return (pdRate != NULL) ? *pdRate = this->demuxRate, S_OK : E_POINTER;
}

STDMETHODIMP CMPUrlSourceSplitter::GetPreroll(LONGLONG* pllPreroll)
{
  return pllPreroll ? *pllPreroll = 0, S_OK : E_POINTER;
}

// IAMStreamSelect

STDMETHODIMP CMPUrlSourceSplitter::Count(DWORD *pcStreams)
{
  CheckPointer(pcStreams, E_POINTER);
  CheckPointer(this->demuxer, E_UNEXPECTED);

  *pcStreams = 0;
  for (int i = 0; i < CDemuxer::Unknown; i++)
  {
    *pcStreams += (DWORD)this->demuxer->GetStreams((CDemuxer::StreamType)i)->Count();
  }

  return S_OK;
}

STDMETHODIMP CMPUrlSourceSplitter::Enable(long lIndex, DWORD dwFlags)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, this->demuxer, result, E_UNEXPECTED);
  CHECK_CONDITION_HRESULT(result, (dwFlags & AMSTREAMSELECTENABLE_ENABLE) != 0, result, E_NOTIMPL);

  if (SUCCEEDED(result))
  {
    // stream index and stream from demuxer
    unsigned int targetGroup = UINT_MAX;
    unsigned int targetIndex = UINT_MAX;
    CStream *targetStream = NULL;

    int j = 0;
    for (unsigned int i = 0; i < CDemuxer::Unknown; i++)
    {
      CStreamCollection *streams = this->demuxer->GetStreams((CDemuxer::StreamType)i);
      int count = (int)streams->Count();

      if ((lIndex >= j) && (lIndex < (j + count)))
      {
        targetIndex = (unsigned int)(lIndex - j);
        targetGroup = i;
        targetStream = streams->GetItem(targetIndex);
        break;
      }

      j += count;
    }
    CHECK_POINTER_HRESULT(result, targetStream, result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      // for each group (video, audio, subpic, unknown) is allowed only one active stream
      // go through each stream in found group and enable requested stream

      CMPUrlSourceSplitterOutputPin *groupOutputPin = NULL;
      CStream *groupStream = NULL;

      // find stream from group streams which is in output pins (it can be only one stream)
      CStreamCollection *groupStreams = this->demuxer->GetStreams((CDemuxer::StreamType)targetGroup);
      for (unsigned int i = 0; ((groupOutputPin == NULL) && (i < groupStreams->Count())); i++)
      {
        groupStream = groupStreams->GetItem(i);

        for (unsigned int j = 0; j < this->outputPins->Count(); j++)
        {
          CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(j);

          if (outputPin->GetStreamPid() == groupStream->GetPid())
          {
            groupOutputPin = outputPin;
            break;
          }
        }
      }
      CHECK_POINTER_HRESULT(result, groupOutputPin, result, E_INVALIDARG);

      if (SUCCEEDED(result))
      {
        if (targetStream->GetPid() != groupStream->GetPid())
        {
          // the streams are not same, we need to exchange groupStream with targetStream

          this->logger->Log(LOGGER_INFO, L"%s: %s: changing output pin '%s', from '%s' to '%s'", MODULE_NAME, METHOD_ENABLE_NAME, groupOutputPin->Name(), groupStream->GetStreamInfo()->GetStreamDescription(), targetStream->GetStreamInfo()->GetStreamDescription());

          if (groupOutputPin->IsConnected())
          {
            IMediaControl *mediaControl = NULL;
            result = this->m_pGraph->QueryInterface(IID_IMediaControl, (void **)&mediaControl);

            CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot get IMediaControl interface, result: 0x%08X", MODULE_NAME, METHOD_ENABLE_NAME, result));

            this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_ENABLED_METHOD_ACTIVE;

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
                groupOutputPin->SetStreamPid(targetStream->GetPid());
                this->demuxer->SetActiveStream((CDemuxer::StreamType)targetGroup, targetStream->GetPid());
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

                  if (SUCCEEDED(result) && (targetGroup != CDemuxer::Video) && (connectedPinInfo.pFilter != NULL))
                  {
                    bool removeFilter = !mediaTypeFound;

                    if (removeFilter && (targetGroup == CDemuxer::Audio))
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

            this->flags &= ~FLAG_MP_URL_SOURCE_SPLITTER_ENABLED_METHOD_ACTIVE;
          }
          else
          {
            // in normal operation, this won't make much sense
            // however, in graphstudio it is now possible to change the stream before connecting

            groupOutputPin->SetStreamPid(targetStream->GetPid());
            this->demuxer->SetActiveStream((CDemuxer::StreamType)targetGroup, targetStream->GetPid());
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
  CHECK_POINTER_HRESULT(result, this->demuxer, result, E_UNEXPECTED);

  if (SUCCEEDED(result))
  {
    int j = 0;
    for (unsigned int i = 0; i < CDemuxer::Unknown; i++)
    {
      CStreamCollection *streams = this->demuxer->GetStreams((CDemuxer::StreamType)i);
      int count = (int)streams->Count();

      if ((lIndex >= j) && (lIndex < (j + count)))
      {
        unsigned int index = (unsigned int)(lIndex - j);

        CStream *stream = streams->GetItem(index);

        CHECK_CONDITION_NOT_NULL_EXECUTE(ppmt, *ppmt = CreateMediaType(stream->GetStreamInfo()->GetMediaTypes()->GetItem(0)));
        CHECK_CONDITION_NOT_NULL_EXECUTE(pdwFlags, *pdwFlags = 0);

        for (unsigned int k = 0; ((pdwFlags != NULL) && (k < this->outputPins->Count())); k++)
        {
          CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(k);

          if (outputPin->GetStreamPid() == stream->GetPid())
          {
            *pdwFlags = AMSTREAMSELECTINFO_ENABLED | AMSTREAMSELECTINFO_EXCLUSIVE;
            break;
          }
        }

        CHECK_CONDITION_NOT_NULL_EXECUTE(pdwGroup, *pdwGroup = i);
        CHECK_CONDITION_NOT_NULL_EXECUTE(ppObject, *ppObject = NULL);
        CHECK_CONDITION_NOT_NULL_EXECUTE(ppUnk, *ppUnk = NULL);
        CHECK_CONDITION_NOT_NULL_EXECUTE(ppszName, *ppszName = Duplicate(stream->GetStreamInfo()->GetStreamDescription()));
        break;
      }

      j += count;
    }
  }

  return result;
}

// IAMOpenProgress

STDMETHODIMP CMPUrlSourceSplitter::QueryProgress(LONGLONG *pllTotal, LONGLONG *pllCurrent)
{
  HRESULT result = E_NOT_VALID_STATE;

  if (this->parserHoster != NULL)
  {
    result = SUCCEEDED(this->parserHoster->GetParserHosterStatus()) ? this->parserHoster->QueryStreamProgress(pllTotal, pllCurrent) : this->parserHoster->GetParserHosterStatus();
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitter::AbortOperation(void)
{
  this->DestroyCreateDemuxerWorker();

  HRESULT result = E_NOT_VALID_STATE;

  if (this->parserHoster != NULL)
  {
    this->parserHoster->StopReceivingData();
    result = S_OK;
  }

  return result;
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
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_ASYNC_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, uri);
  CHECK_POINTER_DEFAULT_HRESULT(result, fileName);
  CHECK_POINTER_DEFAULT_HRESULT(result, downloadCallback);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

  if (SUCCEEDED(result))
  {
    // stop receiving data
    this->parserHoster->StopReceivingData();

    this->asyncDownloadResult = S_OK;
    this->flags &= ~FLAG_MP_URL_SOURCE_SPLITTER_ASYNC_DOWNLOAD_FINISHED;
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
        this->configuration->Add(new CParameter(PARAMETER_NAME_URL, uri));
      }
      this->configuration->Add(new CParameter(PARAMETER_NAME_DOWNLOAD_FILE_NAME, this->downloadFileName));

      FREE_MEM_CLASS(suppliedParameters);
    }
    else
    {
      // parameters are not supplied, just set current url and download file name as only parameters in configuration
      this->configuration->Clear();
      this->configuration->Add(new CParameter(PARAMETER_NAME_URL, uri));
      this->configuration->Add(new CParameter(PARAMETER_NAME_DOWNLOAD_FILE_NAME, this->downloadFileName));
    }
  }

  if (SUCCEEDED(result))
  {
    // loads protocol based on current configuration parameters
    result = this->Load();
  }

  // create demuxer read request worker
  // it is needed also to store file for downloading
  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->CreateDemuxerReadRequestWorker());

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_ASYNC_NAME, result);
  return result;
}

// IDownloadCallback

void STDMETHODCALLTYPE CMPUrlSourceSplitter::OnDownloadCallback(HRESULT downloadResult)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_CALLBACK_NAME);

  this->asyncDownloadResult = downloadResult;

  this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_ASYNC_DOWNLOAD_FINISHED;

  if ((this->asyncDownloadCallback != NULL) && (this->asyncDownloadCallback != this))
  {
    // if download callback is set and it is not current instance (avoid recursion)
    this->asyncDownloadCallback->OnDownloadCallback(downloadResult);
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

int64_t CMPUrlSourceSplitter::SeekToTime(int64_t time)
{
  int64_t result = -1;

  if (this->parserHoster != NULL)
  {
    // notify protocol that we can't receive any data
    // protocol have to supress sending data and will wait until we are ready
    this->parserHoster->SetSupressData(true);
    result = this->parserHoster->SeekToTime(time);

    {
      // lock access to media packets
      CLockMutex mediaPacketLock(this->mediaPacketMutex, INFINITE);

      // clear media packets, we are starting from beginning
      // delete buffer file and set buffer position to zero
      this->mediaPacketCollection->Clear();
      if (this->storeFilePath != NULL)
      {
        DeleteFile(this->storeFilePath);
      }
      this->demuxerContextBufferPosition = 0;

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length to zero, estimate: %d", MODULE_NAME, METHOD_SEEK_TO_TIME_NAME, SUCCEEDED(result) ? 1 : 0);
      this->SetTotalLength(0, SUCCEEDED(result));
    }

    // if correctly seeked than reset flag that all data are received
    // in another case we don't received any other data
    this->flags &= ~FLAG_MP_URL_SOURCE_SPLITTER_ALL_DATA_RECEIVED;
    this->flags |= (result < 0) ? FLAG_MP_URL_SOURCE_SPLITTER_ALL_DATA_RECEIVED : FLAG_MP_URL_SOURCE_SPLITTER_NONE;

    // now we are ready to receive data
    // notify protocol that we can receive data
    this->parserHoster->SetSupressData(false);
  }

  return result;
}

int64_t CMPUrlSourceSplitter::SeekToPosition(int64_t start, int64_t end)
{
  int64_t result = E_NOT_VALID_STATE;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->SeekToPosition(start, end);
  }

  return result;
}

void CMPUrlSourceSplitter::SetSupressData(bool supressData)
{
  if (this->parserHoster != NULL)
  {
    this->parserHoster->SetSupressData(supressData);
  }
}

// IOutputPinFilter

CParameterCollection *CMPUrlSourceSplitter::GetConfiguration(void)
{
  return this->configuration;
}

// IFilter

CLogger *CMPUrlSourceSplitter::GetLogger(void)
{
  return this->logger;
}

HRESULT CMPUrlSourceSplitter::GetTotalLength(int64_t *totalLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, totalLength);

  if (SUCCEEDED(result))
  {
    int64_t availableLength = 0;
    result = this->Length(totalLength, &availableLength);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter::GetAvailableLength(int64_t *availableLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);

  if (SUCCEEDED(result))
  {
    int64_t totalLength = 0;
    result = this->Length(&totalLength, availableLength);
  }

  return result;
}

int64_t CMPUrlSourceSplitter::GetDuration(void)
{
  return (this->parserHoster != NULL) ? this->parserHoster->GetDuration() : DURATION_UNSPECIFIED;
}

// IFilterState

HRESULT CMPUrlSourceSplitter::IsFilterReadyToConnectPins(bool *ready)
{
  CheckPointer(ready, E_POINTER);

  *ready = (this->IsCreatedDemuxer());

  if (FAILED(this->GetParserHosterStatus()))
  {
    // return parser hoster status, there is error
    return this->GetParserHosterStatus();
  }
  else if ((!(*ready)) && this->IsAllDataReceived() && (!this->IsCreatedDemuxer()) && (this->IsCreateDemuxerWorkerFinished()))
  {
    // if demuxer is not created, all data are received and demuxer worker finished its work
    // it throws exception in OV and immediately stops buffering and playback
    return E_DEMUXER_NOT_CREATED_ALL_DATA_RECEIVED_DEMUXER_WORKER_FINISHED;
  }

  return S_OK;
}

HRESULT CMPUrlSourceSplitter::GetCacheFileName(wchar_t **path)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, path);

  if (SUCCEEDED(result))
  {
    SET_STRING(*path, this->storeFilePath);
    result = TEST_STRING_WITH_NULL(*path, this->storeFilePath) ? result : E_OUTOFMEMORY;
  }

  return result;
}

// CAMThread

DWORD CMPUrlSourceSplitter::ThreadProc()
{
  // last command is no command
  this->lastCommand = -1;

  COutputPinPacket *packet = NULL;

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
      this->demuxStop = this->demuxNewStop;

      if (this->IsSplitter() && ((this->demuxStart != 0) || this->IsPlaybackStarted()))
      {
        // in case of live stream, seek by position or time (mostly slightly backward in stream)
        // in live stream we can't receive seek request from graph to skip forward or backward
        // we can only seek in case of starting playback (created thread, not CMD_PLAY request) or in case of changing stream (probably audio or subtitle stream)

        this->demuxer->Seek(max(this->demuxStart, 0));
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
          outputPin->DeliverNewSegment(this->demuxStart, this->demuxStop, this->demuxRate);
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

    bool endOfStream = false;

    unsigned int lastReportStreamTime = 0;
    while (!CheckRequest(&cmd))
    {
      if ((cmd == CMD_PAUSE) || (cmd == CMD_SEEK) || (this->pauseSeekStopRequest))
      {
        Sleep(1);
      }
      else if (!endOfStream)
      {
        HRESULT result = S_OK;

        // get new packet only when we don't have any packet to send
        if (packet == NULL)
        {
          packet = new COutputPinPacket();
          CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

          result = this->GetNextPacket(packet);
        }

        // it can return S_FALSE (no output pin packet) or error code
        if (result == S_OK)
        {
          ASSERT(packet->GetBuffer() != NULL);

          // in case of IPTV there is only one output pin
          // in case of splitter there can be more than one output pin

          CMPUrlSourceSplitterOutputPin *pin = NULL;

          if (this->IsIptv())
          {
            pin = this->outputPins->GetItem(0);
          }

          if (this->IsSplitter())
          {
            for (unsigned int i = 0; i < this->outputPins->Count(); i++)
            {
              CMPUrlSourceSplitterOutputPin *outputPin = this->outputPins->GetItem(i);

              if (outputPin->GetStreamPid() == packet->GetStreamPid())
              {
                pin = outputPin;
                break;
              }
            }

            if (pin != NULL)
            {
              if (packet->GetStartTime() != COutputPinPacket::INVALID_TIME)
              {
                this->demuxCurrent = packet->GetStartTime();

                packet->SetStartTime(packet->GetStartTime() - this->demuxStart);
                packet->SetEndTime(packet->GetEndTime() - this->demuxStart);

                ASSERT(packet->GetStartTime() <= packet->GetEndTime());

                double playRate = 0;
                result = this->GetRate(&playRate);

                if (SUCCEEDED(result))
                {
                  packet->SetStartTime((REFERENCE_TIME)(packet->GetStartTime() / playRate));
                  packet->SetEndTime((REFERENCE_TIME)(packet->GetEndTime() / playRate));
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
              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = pin->QueuePacket(packet, 100));
            }

            if (SUCCEEDED(result))
            {
              // packet was queued to output pin, doesn't need to hold it's reference
              packet = NULL;
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

        if ((result == HRESULT_FROM_WIN32(ERROR_END_OF_MEDIA)) ||
            (result == E_REQUESTED_DATA_AFTER_TOTAL_LENGTH))
        {
          // special error code for end of stream
          for (unsigned int i = 0; i < this->outputPins->Count(); i++)
          {
            this->outputPins->GetItem(i)->QueueEndOfStream();
          }

          endOfStream = true;
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

      if (this->CanReportStreamTime() && ((GetTickCount() - lastReportStreamTime) > 1000))
      {
        lastReportStreamTime = GetTickCount();

        CRefTime refTime;
        if (SUCCEEDED(this->StreamTime(refTime)))
        {
          this->parserHoster->ReportStreamTime((uint64_t)(this->demuxStart / 10000 + refTime.Millisecs()));
        }
      }
    }
    
    if (!CheckRequest(&cmd))
    {
      // if we didn't exit by request, deliver end-of-stream

      for (unsigned int i = 0; i < this->outputPins->Count(); i++)
      {
        this->outputPins->GetItem(i)->QueueEndOfStream();
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

STDMETHODIMP CMPUrlSourceSplitter::Close()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CLOSE_NAME);

  this->pauseSeekStopRequest = true;
  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();
  this->pauseSeekStopRequest = false;

  this->m_State = State_Stopped;
  this->DeleteOutputs();

  FREE_MEM_CLASS(this->demuxer);

  // release AVIOContext for demuxer

  if (this->demuxerContext != NULL)
  {
    av_free(this->demuxerContext->buffer);
    av_free(this->demuxerContext);
    this->demuxerContext = NULL;
  }

  this->demuxerContextBufferPosition = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CLOSE_NAME);
  return S_OK;
}

STDMETHODIMP CMPUrlSourceSplitter::DeleteOutputs()
{
  HRESULT result = S_OK;

  CHECK_CONDITION_HRESULT(result, (this->m_State != State_Stopped), VFW_E_NOT_STOPPED, result);

  if (SUCCEEDED(result) && (this->outputPins != NULL))
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

  return result;
}

bool CMPUrlSourceSplitter::IsIptv(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_AS_IPTV);
}

bool CMPUrlSourceSplitter::IsSplitter(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_AS_SPLITTER);
}

bool CMPUrlSourceSplitter::IsSetFlag(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

void CMPUrlSourceSplitter::SetFlags(unsigned int flags)
{
  this->flags = flags;
}

bool CMPUrlSourceSplitter::IsEstimateTotalLength(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_ESTIMATE_TOTAL_LENGTH);
}

bool CMPUrlSourceSplitter::IsAllDataReceived(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_ALL_DATA_RECEIVED);
}

bool CMPUrlSourceSplitter::IsTotalLengthReceived(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_TOTAL_LENGTH_RECEIVED);
}

bool CMPUrlSourceSplitter::IsDownloadingFile(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_DOWNLOADING_FILE);
}

bool CMPUrlSourceSplitter::IsLiveStream(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_LIVE_STREAM);
}

bool CMPUrlSourceSplitter::IsCreatedDemuxer(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_CREATED_DEMUXER);
}

bool CMPUrlSourceSplitter::IsAsyncDownloadFinished(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_ASYNC_DOWNLOAD_FINISHED);
}

bool CMPUrlSourceSplitter::IsDownloadCallbackCalled(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_DOWNLOAD_CALLBACK_CALLED);
}

bool CMPUrlSourceSplitter::IsMpegTs(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_MPEG_TS);
}

bool CMPUrlSourceSplitter::IsMpegPs(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_MPEG_PS);
}

bool CMPUrlSourceSplitter::IsAvi(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_AVI);
}

bool CMPUrlSourceSplitter::IsEnabledMethodActive(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_ENABLED_METHOD_ACTIVE);
}

bool CMPUrlSourceSplitter::IsPlaybackStarted(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_PLAYBACK_STARTED);
}

bool CMPUrlSourceSplitter::CanReportStreamTime(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_REPORT_STREAM_TIME);
}

HRESULT CMPUrlSourceSplitter::GetParserHosterStatus(void)
{
  if (this->parserHoster != NULL)
  {
    return this->parserHoster->GetParserHosterStatus();
  }

  return E_NOT_VALID_STATE;
}

bool CMPUrlSourceSplitter::IsCreateDemuxerWorkerFinished(void)
{
  return this->IsSetFlag(FLAG_MP_URL_SOURCE_SPLITTER_CREATE_DEMUXER_WORKER_FINISHED);
}

/* create demuxer worker methods */

unsigned int WINAPI CMPUrlSourceSplitter::CreateDemuxerWorker(LPVOID lpParam)
{
  CMPUrlSourceSplitter *caller = (CMPUrlSourceSplitter *)lpParam;

  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME);

  while ((!caller->createDemuxerWorkerShouldExit) && (!caller->IsCreatedDemuxer()) && (!caller->IsAllDataReceived()) && (caller->GetParserHosterStatus() >= STATUS_NONE))
  {
    if (!caller->IsCreatedDemuxer())
    {
      caller->demuxerContextBufferPosition = 0;
      const wchar_t *url = caller->configuration->GetValue(PARAMETER_NAME_URL, true, NULL);

      HRESULT result = S_OK;
      CHECK_POINTER_DEFAULT_HRESULT(result, url);

      if (SUCCEEDED(result))
      {
        FREE_MEM_CLASS(caller->demuxer);
        CDemuxer *demuxer = new CDemuxer(caller->logger, caller, &result);

        if (caller->demuxerContext == NULL)
        {
          uint8_t *buffer = (uint8_t *)av_mallocz(DEMUXER_READ_BUFFER_SIZE + FF_INPUT_BUFFER_PADDING_SIZE);
          caller->demuxerContext = avio_alloc_context(buffer, DEMUXER_READ_BUFFER_SIZE, 0, caller, DemuxerRead, NULL, DemuxerSeek);
        }

        CHECK_POINTER_HRESULT(result, caller->demuxerContext, result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), caller->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, L"not enough memory to allocate AVIOContext"));

        if (SUCCEEDED(result))
        {
          result = demuxer->OpenStream(caller->demuxerContext, url);

          if (SUCCEEDED(result))
          {
            caller->demuxer = demuxer;
            result = caller->InitDemuxer();
          }
          else
          {
            caller->logger->Log(LOGGER_ERROR, L"%s: %s: OpenInputStream() error: 0x%08X", MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, result);
            FREE_MEM_CLASS(demuxer);
          }
        }
      }

      if (SUCCEEDED(result))
      {
        caller->flags |= FLAG_MP_URL_SOURCE_SPLITTER_CREATED_DEMUXER;
        break;
      }
      else
      {
        if (caller->demuxerContext != NULL)
        {
          av_free(caller->demuxerContext->buffer);
          av_free(caller->demuxerContext);
          caller->demuxerContext = NULL;
          caller->demuxerContextBufferPosition = 0;
        }

        caller->outputPins->Clear();
      }
    }

    Sleep(100);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME);
  caller->flags |= FLAG_MP_URL_SOURCE_SPLITTER_CREATE_DEMUXER_WORKER_FINISHED;

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CMPUrlSourceSplitter::CreateCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;

  this->flags &= ~FLAG_MP_URL_SOURCE_SPLITTER_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME);

  this->createDemuxerWorkerShouldExit = false;

  this->createDemuxerWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CMPUrlSourceSplitter::CreateDemuxerWorker, this, 0, NULL);

  if (this->createDemuxerWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, result);
    this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_CREATE_DEMUXER_WORKER_FINISHED;
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter::DestroyCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME);

  this->createDemuxerWorkerShouldExit = true;

  // wait for the create demuxer worker thread to exit
  if (this->createDemuxerWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->createDemuxerWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->createDemuxerWorkerThread, 0);
    }
    CloseHandle(this->createDemuxerWorkerThread);
  }

  this->createDemuxerWorkerThread = NULL;
  this->createDemuxerWorkerShouldExit = false;
  this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, result);
  return result;
}

int CMPUrlSourceSplitter::DemuxerRead(void *opaque, uint8_t *buf, int buf_size)
{
  CMPUrlSourceSplitter *filter = static_cast<CMPUrlSourceSplitter *>(opaque);

  HRESULT result = S_OK;
  CHECK_CONDITION(result, buf_size >= 0, S_OK, E_INVALIDARG);
  CHECK_POINTER_DEFAULT_HRESULT(result, buf);

  if ((SUCCEEDED(result)) && (buf_size > 0) && (filter->demuxerReadRequest == NULL))
  {
    {
      // lock access to demuxer read request
      CLockMutex lock(filter->demuxerReadRequestMutex, INFINITE);

      filter->demuxerReadRequest = new CAsyncRequest();
      CHECK_POINTER_HRESULT(result, filter->demuxerReadRequest, result, E_OUTOFMEMORY);

      result = filter->demuxerReadRequest->Request(filter->demuxerReadRequestId++, filter->demuxerContextBufferPosition, buf_size, buf, NULL);

      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(filter->demuxerReadRequest));
    }

    if (SUCCEEDED(result))
    {
      DWORD ticks = GetTickCount();
      DWORD timeout = filter->GetReceiveDataTimeout();

      result = (timeout != UINT_MAX) ? S_OK : E_UNEXPECTED;

      if (SUCCEEDED(result))
      {
        // if ranges are not supported than we must wait for data

        result = VFW_E_TIMEOUT;

        // wait until request is completed or cancelled
        while (!filter->demuxerReadRequestWorkerShouldExit)
        {
          unsigned int seekingCapabilities = filter->GetSeekingCapabilities();

          {
            // lock access to demuxer read request
            CLockMutex lock(filter->demuxerReadRequestMutex, INFINITE);

            if ((!filter->IsEstimateTotalLength()) && (filter->demuxerReadRequest->GetStart() >= filter->totalLength))
            {
              // something bad occured
              // graph requests data that are beyond stream (data doesn't exists)
              filter->logger->Log(LOGGER_WARNING, L"%s: %s: graph requests data beyond stream, stream total length: %llu, request start: %llu", MODULE_NAME, METHOD_DEMUXER_READ_NAME, filter->totalLength, filter->demuxerReadRequest->GetStart());
              // complete result with error code
              filter->demuxerReadRequest->Complete(E_REQUESTED_DATA_AFTER_TOTAL_LENGTH);
            }

            if (filter->demuxerReadRequest->GetState() == CAsyncRequest::Completed)
            {
              // request is completed, return error or readed data length
              result = SUCCEEDED(filter->demuxerReadRequest->GetErrorCode()) ? filter->demuxerReadRequest->GetBufferLength() : filter->demuxerReadRequest->GetErrorCode();
              break;
            }
            else if (filter->demuxerReadRequest->GetState() == CAsyncRequest::WaitingIgnoreTimeout)
            {
              // we are waiting for data and we have to ignore timeout
            }
            else
            {
              // common case, not for live stream
              if ((!filter->IsLiveStream()) && (seekingCapabilities != SEEKING_METHOD_NONE) && ((GetTickCount() - ticks) > timeout))
              {
                // if seeking is supported and timeout occured then stop waiting for data and exit with VFW_E_TIMEOUT error
                result = VFW_E_TIMEOUT;
                break;
              }
            }
          }

          // sleep some time
          Sleep(10);
        }
      }

      {
        // lock access to demuxer read request
        CLockMutex lock(filter->demuxerReadRequestMutex, INFINITE);

        FREE_MEM_CLASS(filter->demuxerReadRequest);
      }

      if (FAILED(result))
      {
        filter->logger->Log(LOGGER_WARNING, L"%s: %s: requesting data from position: %llu, length: %lu, request id: %u, result: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_NAME, filter->demuxerContextBufferPosition, buf_size, filter->demuxerReadRequestId, result);
      }
    }
  }
  else if ((SUCCEEDED(result)) && (buf_size > 0) && (filter->demuxerReadRequest != NULL))
  {
    {
      // lock access to demuxer read request
      CLockMutex lock(filter->demuxerReadRequestMutex, INFINITE);

      filter->logger->Log(LOGGER_WARNING, L"%s: %s: current read request is not finished, current read request: position: %llu, length: %lu, new request: position: %llu, length: %lu", MODULE_NAME, METHOD_DEMUXER_READ_NAME, filter->demuxerReadRequest->GetStart(), filter->demuxerReadRequest->GetBufferLength(), filter->demuxerContextBufferPosition, buf_size);
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), filter->logger->Log(LOGGER_ERROR, METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_NAME, result));

  if (SUCCEEDED(result))
  {
    // in case of success is in result is length of returned data
    filter->demuxerContextBufferPosition += result;
  }

  return SUCCEEDED(result) ? result : (-1);
}

int64_t CMPUrlSourceSplitter::DemuxerSeek(void *opaque,  int64_t offset, int whence)
{
  CMPUrlSourceSplitter *filter = static_cast<CMPUrlSourceSplitter *>(opaque);

  CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DEMUXER_SEEK_NAME));

  int64_t pos = 0;
  LONGLONG total = 0;
  LONGLONG available = 0;
  filter->Length(&total, &available);

  int64_t result = 0;
  bool resultSet = false;

  if (whence == SEEK_SET)
  {
	  filter->demuxerContextBufferPosition = offset;
    CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, SEEK_SET", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else if (whence == SEEK_CUR)
  {
    filter->demuxerContextBufferPosition += offset;
    CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, SEEK_CUR", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else if (whence == SEEK_END)
  {
    filter->demuxerContextBufferPosition = total - offset;
    CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, SEEK_END", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else if (whence == AVSEEK_SIZE)
  {
    result = total;
    resultSet = true;
    CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, AVSEEK_SIZE", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else
  {
    result = E_INVALIDARG;
    resultSet = true;
    filter->logger->Log(LOGGER_ERROR, L"%s: %s: offset: %lld, unknown seek value", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset);
  }

  if (!resultSet)
  {
    result = filter->demuxerContextBufferPosition;
    resultSet = true;
  }

  CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: End, result: %lld", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, result));
  return result;
}

unsigned int WINAPI CMPUrlSourceSplitter::DemuxerReadRequestWorker(LPVOID lpParam)
{
  CMPUrlSourceSplitter *caller = (CMPUrlSourceSplitter *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME);

  DWORD lastCheckTime = GetTickCount();
  // holds last waiting request id to avoid multiple message logging
  unsigned int lastWaitingRequestId = 0;

  while (!caller->demuxerReadRequestWorkerShouldExit)
  {
    {
      // lock access to demuxer read requests
      CLockMutex requestLock(caller->demuxerReadRequestMutex, INFINITE);

      if (caller->demuxerReadRequest != NULL)
      {
        CAsyncRequest *request = caller->demuxerReadRequest;

        // check if demuxer worker should be finished
        if (caller->createDemuxerWorkerShouldExit)
        {
          // deny request and report as failed
          request->Complete(E_DEMUXER_WORKER_STOP_REQUEST);
        }

        if (FAILED(caller->GetParserHosterStatus()))
        {
          // there is unrecoverable error while receiving data
          // signalize, that we received all data and no other data come
          request->Complete(caller->GetParserHosterStatus());
        }

        if ((request->GetState() == CAsyncRequest::Waiting) || (request->GetState() == CAsyncRequest::WaitingIgnoreTimeout))
        {
          // process only waiting requests
          // variable to store found data length
          unsigned int foundDataLength = 0;
          HRESULT result = S_OK;
          // current stream position is get only when media packet for request is not found
          int64_t currentStreamPosition = -1;

          // first try to find starting media packet (packet which have first data)
          unsigned int packetIndex = UINT_MAX;
          {
            // lock access to media packets
            CLockMutex mediaPacketLock(caller->mediaPacketMutex, INFINITE);

            int64_t startPosition = request->GetStart();
            packetIndex = caller->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(startPosition);            
            if (packetIndex != UINT_MAX)
            {
              while (packetIndex != UINT_MAX)
              {
                unsigned int mediaPacketDataStart = 0;
                unsigned int mediaPacketDataLength = 0;

                // get media packet
                CMediaPacket *mediaPacket = caller->mediaPacketCollection->GetItem(packetIndex);
                // check packet values against async request values
                result = caller->CheckValues(request, mediaPacket, &mediaPacketDataStart, &mediaPacketDataLength, startPosition);

                if (SUCCEEDED(result))
                {
                  // successfully checked values
                  int64_t positionStart = mediaPacket->GetStart();
                  int64_t positionEnd = mediaPacket->GetEnd();

                  // copy data from media packet to request buffer
                  unsigned char *requestBuffer = request->GetBuffer() + foundDataLength;
                  if (mediaPacket->IsStoredToFile() && (request->GetBuffer() != NULL))
                  {
                    // if media packet is stored to file
                    // than is need to read 'mediaPacketDataLength' bytes
                    // from 'mediaPacket->GetStoreFilePosition()' + 'mediaPacketDataStart' position of file

                    LARGE_INTEGER size;
                    size.QuadPart = 0;

                    // open or create file
                    HANDLE hTempFile = CreateFile(caller->storeFilePath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

                    if (hTempFile != INVALID_HANDLE_VALUE)
                    {
                      bool error = false;

                      LONG distanceToMoveLow = (LONG)(mediaPacket->GetStoreFilePosition() + mediaPacketDataStart);
                      LONG distanceToMoveHigh = (LONG)((mediaPacket->GetStoreFilePosition() + mediaPacketDataStart) >> 32);
                      LONG distanceToMoveHighResult = distanceToMoveHigh;
                      DWORD setFileResult = SetFilePointer(hTempFile, distanceToMoveLow, &distanceToMoveHighResult, FILE_BEGIN);
                      if (setFileResult == INVALID_SET_FILE_POINTER)
                      {
                        DWORD lastError = GetLastError();
                        if (lastError != NO_ERROR)
                        {
                          caller->logger->Log(LOGGER_ERROR, L"%s: %s: error occured while setting position: %lu", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, lastError);
                          error = true;
                        }
                      }

                      if (!error)
                      {
                        DWORD read = 0;
                        if (ReadFile(hTempFile, requestBuffer, mediaPacketDataLength, &read, NULL) == 0)
                        {
                          caller->logger->Log(LOGGER_ERROR, L"%s: %s: error occured reading file: %lu", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, GetLastError());
                        }
                        else if (read != mediaPacketDataLength)
                        {
                          caller->logger->Log(LOGGER_WARNING, L"%s: %s: readed data length not same as requested, requested: %u, readed: %u", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, mediaPacketDataLength, read);
                        }
                      }

                      CloseHandle(hTempFile);
                      hTempFile = INVALID_HANDLE_VALUE;
                    }
                  }
                  else if (request->GetBuffer() != NULL)
                  {
                    // media packet is stored in memory
                    mediaPacket->GetBuffer()->CopyFromBuffer(requestBuffer, mediaPacketDataLength, mediaPacketDataStart);
                  }

                  // update length of data
                  foundDataLength += mediaPacketDataLength;

                  if (foundDataLength < (unsigned int)request->GetBufferLength())
                  {
                    // find another media packet after end of this media packet
                    startPosition = positionEnd + 1;
                    packetIndex = caller->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(startPosition);
                  }
                  else
                  {
                    // do not find any more media packets for this request because we have enough data
                    break;
                  }
                }
                else
                {
                  // some error occured
                  // do not find any more media packets for this request because request failed
                  break;
                }
              }

              if (SUCCEEDED(result))
              {
                if (foundDataLength < (unsigned int)request->GetBufferLength())
                {
                  // found data length is lower than requested
                  DWORD currentTime = GetTickCount();
                  if ((!caller->IsLiveStream()) && (!caller->IsAllDataReceived()) && ((currentTime - caller->lastReceivedMediaPacketTime) > caller->GetReceiveDataTimeout()))
                  {
                    // we don't receive data from protocol at least for specified timeout
                    // finish request with error to avoid freeze
                    caller->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u' doesn't receive data for specified time, current time: %d, last received data time: %d, specified timeout: %d", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), currentTime, caller->lastReceivedMediaPacketTime, caller->GetReceiveDataTimeout());
                    request->Complete(VFW_E_TIMEOUT);
                  }
                  else if ((!caller->IsAllDataReceived()) && (!caller->IsEstimateTotalLength()) && (caller->totalLength > (request->GetStart() + request->GetBufferLength())))
                  {
                    // we are receiving data, wait for all requested data
                  }
                  else if ((caller->pauseSeekStopRequest) || (caller->IsAllDataReceived()) || ((caller->IsTotalLengthReceived()) && (!caller->IsEstimateTotalLength()) && (caller->totalLength <= (request->GetStart() + request->GetBufferLength()))))
                  {
                    // we are not receiving more data
                    // finish request
                    caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%d'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), request->GetStart(), request->GetBufferLength());
                    request->SetBufferLength(foundDataLength);
                    request->Complete(S_OK);
                  }
                }
                else if (foundDataLength == request->GetBufferLength())
                {
                  // found data length is equal than requested, return S_OK
                  request->SetBufferLength(foundDataLength);
                  request->Complete(S_OK);
                }
                else
                {
                  caller->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u' found data length '%u' bigger than requested '%lu'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), foundDataLength, request->GetBufferLength());
                  request->Complete(E_RESULT_DATA_LENGTH_BIGGER_THAN_REQUESTED);
                }
              }
              else
              {
                // some error occured
                // complete async request with error
                // set request is completed with result
                caller->logger->Log(LOGGER_WARNING, L"%s: %s: request '%u' complete status: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), result);
                request->SetBufferLength(foundDataLength);
                request->Complete(result);
              }
            }
          }

          if ((packetIndex == UINT_MAX) && (request->GetState() == CAsyncRequest::Waiting))
          {
            // get current stream position
            LONGLONG total = 0;
            HRESULT queryStreamProgressResult = caller->QueryProgress(&total, &currentStreamPosition);
            if (FAILED(queryStreamProgressResult))
            {
              caller->logger->Log(LOGGER_WARNING, L"%s: %s: failed to get current stream position: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, queryStreamProgressResult);
              currentStreamPosition = -1;
            }
          }

          if ((packetIndex == UINT_MAX) && ((request->GetState() == CAsyncRequest::Waiting) || (request->GetState() == CAsyncRequest::WaitingIgnoreTimeout)))
          {
            if (caller->IsAllDataReceived())
            {
              // if all data received then no more will come and we can fail
              caller->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u' no more data available", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId());
              request->Complete(E_NO_MORE_DATA_AVAILABLE);
            }
          }

          if ((packetIndex == UINT_MAX) && (request->GetState() == CAsyncRequest::Waiting))
          {
            // first check current stream position and request start
            // if request start is just next to current stream position then only wait for data and do not issue seek request
            if (currentStreamPosition != (-1))
            {
              // current stream position has valid value
              if (request->GetStart() > currentStreamPosition)
              {
                // if request start is after current stream position than we have to issue seek request (if supported)
                if (request->GetRequestId() != lastWaitingRequestId)
                {
                  caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: request '%u', start '%llu' (size '%lu') after current stream position '%llu'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), request->GetStart(), request->GetBufferLength(), currentStreamPosition);
                  lastWaitingRequestId = request->GetRequestId();
                }
              }
              else if ((request->GetStart() <= currentStreamPosition) && ((request->GetStart() + request->GetBufferLength()) > currentStreamPosition))
              {
                // current stream position is within current request
                // we are receiving data, do nothing, just wait for all data
                request->WaitAndIgnoreTimeout();
              }
              else
              {
                // if request start is before current stream position than we have to issue seek request
                if (request->GetRequestId() != lastWaitingRequestId)
                {
                  CHECK_CONDITION_EXECUTE(!caller->IsAvi(), caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: request '%u', start '%llu' (size '%lu') before current stream position '%llu'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), request->GetStart(), request->GetBufferLength(), currentStreamPosition));
                  lastWaitingRequestId = request->GetRequestId();
                }
              }
            }

            if (request->GetState() == CAsyncRequest::Waiting)
            {
              // there isn't any packet containg some data for request
              // check if seeking by position is supported

              unsigned int seekingCapabilities = caller->GetSeekingCapabilities();
              if (seekingCapabilities & SEEKING_METHOD_POSITION)
              {
                if (SUCCEEDED(result))
                {
                  // not found start packet and request wasn't requested from filter yet
                  // first found start and end of request

                  int64_t requestStart = request->GetStart();
                  int64_t requestEnd = requestStart;

                  unsigned int startIndex = 0;
                  unsigned int endIndex = 0;
                  {
                    // lock access to media packets
                    CLockMutex mediaPacketLock(caller->mediaPacketMutex, INFINITE);

                    if (caller->mediaPacketCollection->GetItemInsertPosition(request->GetStart(), NULL, &startIndex, &endIndex))
                    {
                      // start and end index found successfully
                      if (startIndex == endIndex)
                      {
                        int64_t endPacketStartPosition = 0;
                        int64_t endPacketStopPosition = 0;
                        unsigned int mediaPacketIndex = caller->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(endPacketStartPosition);

                        // media packet exists in collection
                        while (mediaPacketIndex != UINT_MAX)
                        {
                          CMediaPacket *mediaPacket = caller->mediaPacketCollection->GetItem(mediaPacketIndex);
                          int64_t mediaPacketStart = mediaPacket->GetStart();
                          int64_t mediaPacketEnd = mediaPacket->GetEnd();
                          if (endPacketStartPosition == mediaPacketStart)
                          {
                            // next start time is next to end of current media packet
                            endPacketStartPosition = mediaPacketEnd + 1;
                            mediaPacketIndex++;

                            if (mediaPacketIndex >= caller->mediaPacketCollection->Count())
                            {
                              // stop checking, all media packets checked
                              mediaPacketIndex = UINT_MAX;
                            }
                          }
                          else
                          {
                            endPacketStopPosition = mediaPacketStart - 1;
                            mediaPacketIndex = UINT_MAX;
                          }
                        }

                        requestEnd = endPacketStopPosition;
                      }
                      else if ((startIndex == (caller->mediaPacketCollection->Count() - 1)) && (endIndex == UINT_MAX))
                      {
                        // media packet belongs to end
                        // do nothing, default request is from specific point until end of stream
                      }
                      else if ((startIndex == UINT_MAX) && (endIndex == 0))
                      {
                        // media packet belongs to start
                        CMediaPacket *endMediaPacket = caller->mediaPacketCollection->GetItem(endIndex);
                        if (endMediaPacket != NULL)
                        {
                          // requests data from requestStart until end packet start position
                          requestEnd = endMediaPacket->GetStart() - 1;
                        }
                      }
                      else
                      {
                        // media packet belongs between packets startIndex and endIndex
                        CMediaPacket *endMediaPacket = caller->mediaPacketCollection->GetItem(endIndex);
                        if (endMediaPacket != NULL)
                        {
                          // requests data from requestStart until end packet start position
                          requestEnd = endMediaPacket->GetStart() - 1;
                        }
                      }
                    }
                  }

                  if (requestEnd < requestStart)
                  {
                    CHECK_CONDITION_EXECUTE(!caller->IsAvi(), caller->logger->Log(LOGGER_WARNING, L"%s: %s: request '%u' has start '%llu' after end '%llu', modifying to equal", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), requestStart, requestEnd));
                    requestEnd = requestStart;
                  }

                  // request filter to receive data from request start to end
                  result = (caller->SeekToPosition(requestStart, requestEnd) >= 0) ? S_OK : E_FAIL;
                }

                if (FAILED(result))
                {
                  // if error occured while requesting filter for data
                  caller->logger->Log(LOGGER_WARNING, L"%s: %s: request '%u' error while requesting data, complete status: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, request->GetRequestId(), result);
                  request->Complete(result);
                }
              }
            }
          }
        }
      }
    }

    {
      if (((GetTickCount() - lastCheckTime) > 1000) && ((caller->IsDownloadingFile()) || (!caller->IsLiveStream())))
      {
        lastCheckTime = GetTickCount();

        // lock access to media packets
        CLockMutex mediaPacketLock(caller->mediaPacketMutex, INFINITE);

        if (caller->mediaPacketCollection->Count() > 0)
        {
          // store all media packets (which are not stored) to file
          if (caller->storeFilePath == NULL)
          {
            caller->storeFilePath = caller->GetStoreFile();
          }

          if (caller->storeFilePath != NULL)
          {
            LARGE_INTEGER size;
            size.QuadPart = 0;

            // open or create file
            HANDLE hTempFile = CreateFile(caller->storeFilePath, FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

            if (hTempFile != INVALID_HANDLE_VALUE)
            {
              if (!GetFileSizeEx(hTempFile, &size))
              {
                caller->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, L"error while getting size");
                // error occured while getting file size
                size.QuadPart = -1;
              }

              if (size.QuadPart >= 0)
              {
                unsigned int i = 0;
                bool allMediaPacketsStored = true;
                while (i < caller->mediaPacketCollection->Count())
                {
                  CMediaPacket *mediaPacket = caller->mediaPacketCollection->GetItem(i);

                  if (!mediaPacket->IsStoredToFile())
                  {
                    // if media packet is not stored to file
                    // store it to file
                    int64_t mediaPacketStartPosition = mediaPacket->GetStart();
                    int64_t mediaPacketEndPosition = mediaPacket->GetEnd();
                    unsigned int length = (unsigned int)(mediaPacketEndPosition + 1 - mediaPacketStartPosition);

                    ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
                    if (mediaPacket->GetBuffer()->CopyFromBuffer(buffer, length) == length)
                    {
                      DWORD written = 0;
                      if (WriteFile(hTempFile, buffer, length, &written, NULL))
                      {
                        if (length == written)
                        {
                          // mark as stored
                          mediaPacket->SetStoredToFile(size.QuadPart);
                          size.QuadPart += length;
                        }
                        else
                        {
                          allMediaPacketsStored = false;
                        }
                      }
                      else
                      {
                        allMediaPacketsStored = false;
                        caller->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, L"not written");
                      }
                    }
                    else
                    {
                      allMediaPacketsStored = false;
                    }
                    FREE_MEM(buffer);
                  }

                  i++;
                }

                if (caller->IsDownloadingFile() && caller->IsAllDataReceived() && allMediaPacketsStored && (!caller->IsDownloadCallbackCalled()))
                {
                  // all data received
                  // call download callback method
                  caller->OnDownloadCallback(S_OK);
                  caller->flags |= FLAG_MP_URL_SOURCE_SPLITTER_DOWNLOAD_CALLBACK_CALLED;
                }
              }

              CloseHandle(hTempFile);
              hTempFile = INVALID_HANDLE_VALUE;
            }
            else
            {
              caller->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, L"invalid file handle");
            }
          }
        }
      }

      // remove used media packets
      // in case of live stream they will not be needed (after created demuxer and started playing)
      if ((!caller->IsDownloadingFile()) && (caller->IsLiveStream()) && (caller->IsCreatedDemuxer()) && (caller->lastCommand == CMD_PLAY) && ((GetTickCount() - lastCheckTime) > 1000))
      {
        lastCheckTime = GetTickCount();

        // lock access to media packets
        CLockMutex mediaPacketLock(caller->mediaPacketMutex, INFINITE);

        if (caller->mediaPacketCollection->Count() > 0)
        {
          while (true)
          {
            CMediaPacket *mediaPacket = caller->mediaPacketCollection->GetItem(0);

            if (mediaPacket->GetEnd() < caller->demuxerContextBufferPosition)
            {
              caller->mediaPacketCollection->Remove(0);
            }
            else
            {
              break;
            }
          }
        }
      }
    }

    Sleep(1);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CMPUrlSourceSplitter::CreateDemuxerReadRequestWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME);

  this->demuxerReadRequestWorkerShouldExit = false;

  this->demuxerReadRequestWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CMPUrlSourceSplitter::DemuxerReadRequestWorker, this, 0, NULL);

  if (this->demuxerReadRequestWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter::DestroyDemuxerReadRequestWorker(void)
{
   HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME);

  this->demuxerReadRequestWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->demuxerReadRequestWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->demuxerReadRequestWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->demuxerReadRequestWorkerThread, 0);
    }
    CloseHandle(this->demuxerReadRequestWorkerThread);
  }

  this->demuxerReadRequestWorkerThread = NULL;
  this->demuxerReadRequestWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter::CheckValues(CAsyncRequest *request, CMediaPacket *mediaPacket, unsigned int *mediaPacketDataStart, unsigned int *mediaPacketDataLength, int64_t startPosition)
{
  HRESULT result = S_OK;

  CHECK_POINTER_DEFAULT_HRESULT(result, request);
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacket);
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacketDataStart);
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacketDataLength);

  if (SUCCEEDED(result))
  {
    LONGLONG requestStart = request->GetStart();
    LONGLONG requestEnd = request->GetStart() + request->GetBufferLength();

    CHECK_CONDITION_HRESULT(result, ((startPosition >= requestStart) && (startPosition <= requestEnd)), result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      int64_t mediaPacketStart = mediaPacket->GetStart();
      int64_t mediaPacketEnd = mediaPacket->GetEnd();

      if (SUCCEEDED(result))
      {
        // check if start position is in media packet
        CHECK_CONDITION_HRESULT(result, ((startPosition >= mediaPacketStart) && (startPosition <= mediaPacketEnd)), result, E_INVALIDARG);

        if (SUCCEEDED(result))
        {
          // increase position end because position end is stamp of last byte in buffer
          mediaPacketEnd++;

          // check if async request and media packet are overlapping
          CHECK_CONDITION_HRESULT(result, ((requestStart <= mediaPacketEnd) && (requestEnd >= mediaPacketStart)), result, E_INVALIDARG);
        }
      }

      if (SUCCEEDED(result))
      {
        // check problematic values
        // maximum length of data in media packet can be UINT_MAX - 1
        // async request cannot start after UINT_MAX - 1 because then async request and media packet are not overlapping

        int64_t tempMediaPacketDataStart = ((startPosition - mediaPacketStart) > 0) ? startPosition : mediaPacketStart;
        if ((min(requestEnd, mediaPacketEnd) - tempMediaPacketDataStart) >= UINT_MAX)
        {
          // it's there just for sure
          // problem: length of data is bigger than possible values for copying data
          result = E_OUTOFMEMORY;
        }

        if (SUCCEEDED(result))
        {
          // all values are correct
          *mediaPacketDataStart = (unsigned int)(tempMediaPacketDataStart - mediaPacketStart);
          *mediaPacketDataLength = (unsigned int)(min(requestEnd, mediaPacketEnd) - tempMediaPacketDataStart);
        }
      }
    }
  }

  return result;
}

/* demuxer methods */

HRESULT CMPUrlSourceSplitter::InitDemuxer(void)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->demuxer);

  if (SUCCEEDED(result))
  {
    wchar_t fileName[1024];
    result = (GetModuleFileName(NULL, fileName, 1024) != 0) ? result : E_OUTOFMEMORY;

    const wchar_t *processName = PathFindFileName(fileName);

    // disable subtitles in applications known to fail with them (explorer thumbnail generator, power point, basically all applications using MCI)
    bool noSubtitles = 
      ((_wcsicmp(processName, L"dllhost.exe") == 0) ||
       (_wcsicmp(processName, L"explorer.exe") == 0) ||
       (_wcsicmp(processName, L"powerpnt.exe") == 0) ||
       (_wcsicmp(processName, L"pptview.exe") == 0));

    this->demuxCurrent = 0;
    this->demuxNewStart = 0;
    this->demuxStart = 0;
    this->demuxNewStop = this->demuxer->GetDuration();
    this->demuxStop = this->demuxNewStop;

    this->flags &= ~(FLAG_MP_URL_SOURCE_SPLITTER_MPEG_TS | FLAG_MP_URL_SOURCE_SPLITTER_MPEG_PS | FLAG_MP_URL_SOURCE_SPLITTER_AVI);

    this->flags |= (_wcsicmp(this->demuxer->GetContainerFormat(), L"mpegts") == 0) ? FLAG_MP_URL_SOURCE_SPLITTER_MPEG_TS : FLAG_MP_URL_SOURCE_SPLITTER_NONE;
    this->flags |= (_wcsicmp(this->demuxer->GetContainerFormat(), L"mpeg") == 0) ? FLAG_MP_URL_SOURCE_SPLITTER_MPEG_PS : FLAG_MP_URL_SOURCE_SPLITTER_NONE;
    this->flags |= (_wcsicmp(this->demuxer->GetContainerFormat(), L"avi") == 0) ? FLAG_MP_URL_SOURCE_SPLITTER_AVI : FLAG_MP_URL_SOURCE_SPLITTER_NONE;

    CStream *videoStream = this->demuxer->SelectVideoStream();
    if (videoStream != NULL)
    {
      CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputPin(videoStream->GetStreamInfo()->GetMediaTypes(), L"Video", this, this, &result, this->demuxer->GetContainerFormat());

      if (SUCCEEDED(result))
      {
        outputPin->SetStreamPid(videoStream->GetPid());
        result = (this->outputPins->Add(outputPin)) ? result : E_OUTOFMEMORY;

        this->demuxer->SetActiveStream(CDemuxer::Video, videoStream->GetPid());
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(outputPin));
    }

    CStream *audioStream = this->demuxer->SelectAudioStream();
    if (audioStream != NULL)
    {
      CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputPin(audioStream->GetStreamInfo()->GetMediaTypes(), L"Audio", this, this, &result, this->demuxer->GetContainerFormat());

      if (SUCCEEDED(result))
      {
        outputPin->SetStreamPid(audioStream->GetPid());
        result = (this->outputPins->Add(outputPin)) ? result : E_OUTOFMEMORY;

        this->demuxer->SetActiveStream(CDemuxer::Audio, audioStream->GetPid());
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(outputPin));
    }

    // if there are some subtitles, just choose first and create output pin
    CStream *subtitleStream = (this->demuxer->GetStreams(CDemuxer::Subpic)->Count() != 0) ? this->demuxer->GetStreams(CDemuxer::Subpic)->GetItem(0) : NULL;
    if (subtitleStream != NULL)
    {
      CMPUrlSourceSplitterOutputPin *outputPin = new CMPUrlSourceSplitterOutputPin(subtitleStream->GetStreamInfo()->GetMediaTypes(), L"Subtitle", this, this, &result, this->demuxer->GetContainerFormat());

      if (SUCCEEDED(result))
      {
        outputPin->SetStreamPid(subtitleStream->GetPid());
        result = (this->outputPins->Add(outputPin)) ? result : E_OUTOFMEMORY;

        this->demuxer->SetActiveStream(CDemuxer::Subpic, subtitleStream->GetPid());
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(outputPin));
    }

    // if there are no pins, then it is bad
    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->outputPins->Count() > 0) ? result : E_FAIL);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter::GetNextPacket(COutputPinPacket *packet)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    if (this->IsIptv())
    {
      CLockMutex lock(this->mediaPacketMutex, INFINITE);

      result = (this->mediaPacketCollection->Count() > 0) ? S_OK : S_FALSE;

      if (result == S_OK)
      {
        CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(0);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = packet->CreateBuffer(mediaPacket->GetBuffer()->GetBufferOccupiedSpace()) ? result : E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (packet->GetBuffer()->AddToBufferWithResize(mediaPacket->GetBuffer()) == mediaPacket->GetBuffer()->GetBufferOccupiedSpace()) ? result : E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->mediaPacketCollection->Remove(0));
      }
    }

    if (this->IsSplitter())
    {
      result = this->demuxer->GetNextPacket(packet);
    }
  }

  return result;
}

void InvalidParameterHandler(const wchar_t* expression, const wchar_t* function, const wchar_t* file, unsigned int line, uintptr_t pReserved)
{
  // in release it doesn't output any valuable information
#ifdef _DEBUG
  if (ffmpegLoggerInstance != NULL)
  {
    ffmpegLoggerInstance->Log(LOGGER_VERBOSE, L"%s: %s: invalid parameter detected in function '%s', file '%s', line %d.\nExpression: %s", MODULE_NAME, L"InvalidParameterHandler()", function, file, line, expression);
  }
#endif
}

void CMPUrlSourceSplitter::FFmpegLogCallback(void *ptr, int log_level, const char *format, va_list vl)
{
  // supress error messages while logging messages from ffmpeg
  // error messages are written to log file in debug

  bool isAvi = false;
  bool isMpegTs = false;
  AVFormatContext *formatContext = (AVFormatContext *)ptr;

  CLogger *loggerInstance =  ffmpegLoggerInstance;
  CMPUrlSourceSplitter *filter = NULL;
  if ((formatContext != NULL) && (formatContext->pb != NULL) && (formatContext->pb->opaque != NULL))
  {
    filter = (CMPUrlSourceSplitter *)formatContext->pb->opaque;
    loggerInstance = filter->logger;
    isAvi = filter->IsAvi();
    isMpegTs = filter->IsMpegTs();
  }

  if ((loggerInstance != NULL) && (!isAvi))
  {
    int warnReportMode = _CrtSetReportMode(_CRT_WARN, 0);
    int errorReportMode = _CrtSetReportMode(_CRT_ERROR, 0);
    int assertReportMode = _CrtSetReportMode(_CRT_ASSERT, 0);

    _invalid_parameter_handler previousHandler = _set_invalid_parameter_handler(InvalidParameterHandler);

    int length = _vscprintf(format, vl) + 1;
    ALLOC_MEM_DEFINE_SET(buffer, char, length, 0);
    if (buffer != NULL)
    {
      if (vsprintf_s(buffer, length, format, vl) != (-1))
      {
        char *trimmed = TrimA(buffer);
        if (trimmed != NULL)
        {
          if ((filter != NULL) && (!isAvi) && (!isMpegTs))
          {
            filter->flags |= (strcmp("Format avi probed with size=2048 and score=100", trimmed) == 0) ? FLAG_MP_URL_SOURCE_SPLITTER_AVI : FLAG_MP_URL_SOURCE_SPLITTER_NONE;
            isAvi = filter->IsAvi();
          }

          if ((filter != NULL) && (!isAvi) && (!isMpegTs))
          {
            filter->flags |= (strcmp("Format mpegts probed with size=2048 and score=100", trimmed) == 0) ? FLAG_MP_URL_SOURCE_SPLITTER_MPEG_TS : FLAG_MP_URL_SOURCE_SPLITTER_NONE;
            isMpegTs = filter->IsMpegTs();
          }

          wchar_t *logLine = ConvertToUnicodeA(trimmed);
          if (logLine != NULL)
          {
            if ((!isMpegTs) || (isMpegTs && (strncmp("first_dts", trimmed, 9) != 0)))
            {
              loggerInstance->Log(LOGGER_VERBOSE, L"%s: %s: log level: %d, message: %s", MODULE_NAME, L"ffmpeg_log_callback()", log_level, logLine);
            }
          }

          FREE_MEM(logLine);
        }
        FREE_MEM(trimmed);
      }
    }

    FREE_MEM(buffer);

    // set original values for error messages back
    _set_invalid_parameter_handler(previousHandler);

    _CrtSetReportMode(_CRT_WARN, warnReportMode);
    _CrtSetReportMode(_CRT_ERROR, errorReportMode);
    _CrtSetReportMode(_CRT_ASSERT, assertReportMode);
  }
}

STDMETHODIMP CMPUrlSourceSplitter::Load()
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

  if (this->configuration == NULL)
  {
    result = E_INVALID_CONFIGURATION;
  }

  if (SUCCEEDED(result))
  {
    // set logger parameters
    this->logger->SetParameters(this->configuration);
  }

  if (SUCCEEDED(result))
  {
    result = (this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL) == NULL) ? E_URL_NOT_SPECIFIED : S_OK;
  }

  if (SUCCEEDED(result))
  {
    FREE_MEM(this->storeFilePath);
    this->storeFilePath = Duplicate(this->configuration->GetValue(PARAMETER_NAME_DOWNLOAD_FILE_NAME, true, NULL));
    this->flags |= (this->storeFilePath != NULL) ? FLAG_MP_URL_SOURCE_SPLITTER_DOWNLOADING_FILE : FLAG_MP_URL_SOURCE_SPLITTER_NONE;
    this->flags |= (this->configuration->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT)) ? FLAG_MP_URL_SOURCE_SPLITTER_LIVE_STREAM : FLAG_MP_URL_SOURCE_SPLITTER_NONE;

    wchar_t *folder = GetStoreFilePath(this->IsIptv() ? L"MPIPTVSource" : L"MPUrlSourceSplitter", this->configuration);
    const wchar_t *cacheFolder = configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

    if ((folder != NULL) && (cacheFolder == NULL))
    {
      // cache folder is not specified in configuration parameters
      // add new folder to configuration parameters

      this->configuration->Add(PARAMETER_NAME_CACHE_FOLDER, folder);
    }

    FREE_MEM(folder);
  }

  if (SUCCEEDED(result))
  {
    result = this->parserHoster->StartReceivingData(this->configuration);
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
  CParameterCollection *parsedParameters = new CParameterCollection();

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
            if (token == NULL)
            {
              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, L"not enough memory for token");
              result = E_OUTOFMEMORY;
            }

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
                if (name == NULL)
                {
                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, L"not enough memory for parameter name");
                  result = E_OUTOFMEMORY;
                }

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
                  if (curlValue == NULL)
                  {
                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, L"not enough memory for value for CURL library");
                    result = E_CONVERT_STRING_ERROR;
                  }

                  if (SUCCEEDED(result))
                  {
                    char *unescapedCurlValue = curl_easy_unescape(NULL, curlValue, 0, NULL);

                    if (unescapedCurlValue == NULL)
                    {
                      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, "error occured while getting unescaped value from CURL library");
                      result = E_FAIL;
                    }

                    if (SUCCEEDED(result))
                    {
                      wchar_t *unescapedValue = ConvertToUnicodeA(unescapedCurlValue);

                      if (unescapedValue == NULL)
                      {
                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_PARSE_PARAMETERS_NAME, "not enough memory for unescaped value");
                        result = E_CONVERT_STRING_ERROR;
                      }

                      if (SUCCEEDED(result))
                      {
                        // we got successfully unescaped parameter value
                        CParameter *parameter = new CParameter(name, unescapedValue);
                        parsedParameters->Add(parameter);
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

STDMETHODIMP CMPUrlSourceSplitter::Length(LONGLONG *total, LONGLONG *available)
{
  CHECK_CONDITION_EXECUTE((!this->IsAvi()) && (this->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, MODULE_NAME, METHOD_LENGTH_NAME));

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, total);
  CHECK_POINTER_DEFAULT_HRESULT(result, available);

  unsigned int mediaPacketCount = 0;
  {
    CLockMutex lock(this->mediaPacketMutex, INFINITE);
    mediaPacketCount = this->mediaPacketCollection->Count();
  }

  if (SUCCEEDED(result))
  {
    *total = this->totalLength;
    *available = this->totalLength;
    
    CStreamAvailableLength *availableLength = new CStreamAvailableLength();
    result = this->QueryStreamAvailableLength(availableLength);
    if (SUCCEEDED(result))
    {
      result = availableLength->GetQueryResult();
    }

    if (SUCCEEDED(result))
    {
      *available = availableLength->GetAvailableLength();
    }
    
    if (FAILED(result))
    {
      // error occured while requesting stream available length
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: cannot query available stream length, result: 0x%08X", MODULE_NAME, METHOD_LENGTH_NAME, result);

      CLockMutex lock(this->mediaPacketMutex, INFINITE);
      mediaPacketCount = this->mediaPacketCollection->Count();

      // return default value = last media packet end
      *available = 0;
      for (unsigned int i = 0; i < mediaPacketCount; i++)
      {
        CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(i);
        int64_t mediaPacketStart = mediaPacket->GetStart();
        int64_t mediaPacketEnd = mediaPacket->GetEnd();

        if ((mediaPacketEnd + 1) > (*available))
        {
          *available = mediaPacketEnd + 1;
        }
      }

      result = S_OK;
    }
    FREE_MEM_CLASS(availableLength);

    result = (this->IsEstimateTotalLength()) ? VFW_S_ESTIMATED : S_OK;
    CHECK_CONDITION_EXECUTE((!this->IsAvi()) && (this->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), this->logger->Log(LOGGER_VERBOSE, L"%s: %s: total length: %llu, available length: %llu, estimate: %u, media packets: %u", MODULE_NAME, METHOD_LENGTH_NAME, this->totalLength, *available, (this->IsEstimateTotalLength()) ? 1 : 0, mediaPacketCount));
  }

  CHECK_CONDITION_EXECUTE((!this->IsAvi()) && (this->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), this->logger->Log(LOGGER_VERBOSE, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LENGTH_NAME, result));
  return result;
}

HRESULT CMPUrlSourceSplitter::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
{
  HRESULT result = E_NOTIMPL;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->QueryStreamAvailableLength(availableLength);
  }

  return result;
}

unsigned int CMPUrlSourceSplitter::GetReceiveDataTimeout(void)
{
  unsigned int result = UINT_MAX;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->GetReceiveDataTimeout();
  }

  return result;
}

wchar_t *CMPUrlSourceSplitter::GetStoreFile(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());

    if (guid != NULL)
    {
      result = FormatString(this->IsIptv() ? L"%smpiptvsource_%s.temp" : L"%smpurlsourcesplitter_%s.temp", folder, guid);
    }
  }

  return result;
}

void CMPUrlSourceSplitter::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CLEAR_SESSION_NAME);

  this->lastCommand = -1;
  this->pauseSeekStopRequest = false;

  // clear all flags instead of filter type (IPTV or splitter)
  this->flags &= (FLAG_MP_URL_SOURCE_SPLITTER_AS_IPTV | FLAG_MP_URL_SOURCE_SPLITTER_AS_SPLITTER);

  this->DestroyCreateDemuxerWorker();
  this->DestroyDemuxerReadRequestWorker();
  this->demuxerReadRequestId = 0;

  FREE_MEM_CLASS(this->demuxer);

  // release AVIOContext for demuxer
  if (this->demuxerContext != NULL)
  {
    av_free(this->demuxerContext->buffer);
    av_free(this->demuxerContext);
    this->demuxerContext = NULL;
  }
  this->demuxerContextBufferPosition = 0;

  FREE_MEM(this->storeFilePath);

  this->demuxStart = 0;
  this->demuxStop = 0;
  this->demuxRate = 1.0;
  this->demuxCurrent = 0;
  this->demuxNewStart = 0;
  this->demuxNewStop = 0;
  this->seekingLastStart = _I64_MIN;
  this->seekingLastStop = _I64_MIN;

  this->asyncDownloadResult = S_OK;
  this->asyncDownloadCallback = NULL;

  this->mediaPacketCollection->Clear();
  this->totalLength = 0;
  this->flags |= FLAG_MP_URL_SOURCE_SPLITTER_ESTIMATE_TOTAL_LENGTH;
  this->lastReceivedMediaPacketTime = GetTickCount();
  this->parserHoster->ClearSession();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CLEAR_SESSION_NAME);
}