/*
 *      Copyright (C) 2011 Hendrik Leppkes
 *      http://www.1f0.de
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

#include "stdafx.h"
#include "MPUrlSourceSplitterInputPin.h"
#include "Utilities.h"
#include "Parameters.h"

#include "MPUrlSourceSplitter.h"
#include "LockMutex.h"
#include "ErrorCodes.h"

#include <Shlwapi.h>
#include <Shlobj.h>

#define READ_BUFFER_SIZE 32768

#ifdef _DEBUG
#define MODULE_NAME                                               L"MPUrlSourceSplitterInputPind"
#else
#define MODULE_NAME                                               L"MPUrlSourceSplitterInputPin"
#endif

#define METHOD_PARSE_PARAMETERS_NAME                              L"ParseParameters()"
#define METHOD_LOAD_PLUGINS_NAME                                  L"LoadPlugins()"
#define METHOD_LOAD_NAME                                          L"Load()"
#define METHOD_SET_TOTAL_LENGTH_NAME                              L"SetTotalLength()"

#define METHOD_SYNC_READ_NAME                                     L"SyncRead()"
#define METHOD_LENGTH_NAME                                        L"Length()"
#define METHOD_CREATE_ASYNC_REQUEST_PROCESS_WORKER_NAME           L"CreateAsyncRequestProcessWorker()"
#define METHOD_DESTROY_ASYNC_REQUEST_PROCESS_WORKER_NAME          L"DestroyAsyncRequestProcessWorker()"
#define METHOD_ASYNC_REQUEST_PROCESS_WORKER_NAME                  L"AsyncRequestProcessWorker()"

#define METHOD_SEEK_NAME                                          L"Seek()"
#define METHOD_READ_NAME                                          L"Read()"

#define PARAMETER_SEPARATOR                                       L"&"
#define PARAMETER_IDENTIFIER                                      L"####"
#define PARAMETER_ASSIGN                                          L"="

extern "C" char *curl_easy_unescape(void *handle, const char *string, int length, int *olen);
extern "C" void curl_free(void *p);

CMPUrlSourceSplitterInputPin::CMPUrlSourceSplitterInputPin(CLogger *logger, wchar_t *pName, CMPUrlSourceSplitter *pFilter, CCritSec *pLock, HRESULT *phr)
  : CUnknown(pName, NULL)
  /*, m_rtStart(0)
  , m_rtStop(0)
  , m_dRate(1.0)
  , m_rtLastStart(_I64_MIN)
  , m_rtLastStop(_I64_MIN)
  , m_rtCurrent(0)
  , m_bStopValid(FALSE)*/
{
  HRESULT result = S_OK;
  this->configuration = new CParameterCollection();

  this->logger = logger;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
  
  this->filter = pFilter;
  this->allDataReceived = false;
  this->totalLengthReceived = false;
  this->mediaPacketCollection = new CMediaPacketCollection();
  this->totalLength = 0;
  this->estimate = true;
  this->mediaPacketMutex = CreateMutex(NULL, FALSE, NULL);
  this->lastReceivedMediaPacketTime = GetTickCount();
  this->parserHoster = new CParserHoster(this->logger, this->configuration, this);
  if (this->parserHoster != NULL)
  {
    this->parserHoster->LoadPlugins();
  }
  
  if (phr)
  {
    *phr = result;
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);


  /*
  this->configuration = new CParameterCollection();

  this->logger = logger;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
  
  this->createdDemuxer = false;
  this->m_pAVIOContext = NULL;
  this->m_llBufferPosition = 0;
  this->filter = pFilter;
  this->downloadFileName = NULL;
  this->asyncDownloadFinished = false;
  this->allDataReceived = false;
  this->totalLengthReceived = false;
  this->downloadCallbackCalled = false;
  this->asyncDownloadResult = S_OK;
  this->asyncDownloadCallback = NULL;
  this->currentReadRequest = NULL;
  this->mediaPacketCollection = new CMediaPacketCollection();
  this->totalLength = 0;
  this->estimate = true;
  this->asyncRequestProcessingShouldExit = false;
  this->requestId = 0;
  this->requestMutex = CreateMutex(NULL, FALSE, NULL);
  this->mediaPacketMutex = CreateMutex(NULL, FALSE, NULL);
  this->lastReceivedMediaPacketTime = GetTickCount();
  this->parserHoster = new CParserHoster(this->logger, this->configuration, this);
  if (this->parserHoster != NULL)
  {
    this->parserHoster->LoadPlugins();
  }
  
  this->storeFilePath = Duplicate(this->configuration->GetValue(PARAMETER_NAME_DOWNLOAD_FILE_NAME, true, NULL));
  this->downloadingFile = (this->storeFilePath != NULL);
  this->liveStream = false;

  this->hCreateDemuxerWorkerThread = NULL;

  HRESULT result = S_OK;
  if (SUCCEEDED(result))
  {
    result = this->CreateAsyncRequestProcessWorker();
  }

  if (phr)
  {
    *phr = result;
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
  */
}

CMPUrlSourceSplitterInputPin::~CMPUrlSourceSplitterInputPin(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->parserHoster != NULL)
  {
    this->parserHoster->StopReceivingData();
    this->parserHoster->RemoveAllPlugins();
    FREE_MEM_CLASS(this->parserHoster);
  }

  FREE_MEM_CLASS(this->mediaPacketCollection);

  if (this->mediaPacketMutex != NULL)
  {
    CloseHandle(this->mediaPacketMutex);
    this->mediaPacketMutex = NULL;
  }

  FREE_MEM_CLASS(this->configuration);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);


  /*
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);

  this->DestroyDemuxerWorker();
  this->DestroyAsyncRequestProcessWorker();
  this->ReleaseAVIOContext();

  if (this->parserHoster != NULL)
  {
    this->parserHoster->StopReceivingData();
    this->parserHoster->RemoveAllPlugins();
    FREE_MEM_CLASS(this->parserHoster);
  }

  FREE_MEM_CLASS(this->currentReadRequest);
  FREE_MEM_CLASS(this->mediaPacketCollection);

  if (this->requestMutex != NULL)
  {
    CloseHandle(this->requestMutex);
    this->requestMutex = NULL;
  }
  if (this->mediaPacketMutex != NULL)
  {
    CloseHandle(this->mediaPacketMutex);
    this->mediaPacketMutex = NULL;
  }
  if ((!this->downloadingFile) && (this->storeFilePath != NULL))
  {
    DeleteFile(this->storeFilePath);
  }

  FREE_MEM(this->storeFilePath);

  FREE_MEM_CLASS(this->configuration);
  FREE_MEM(this->downloadFileName);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);
  */
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

  return
    __super::NonDelegatingQueryInterface(riid, ppv);
}

// IFileSourceFilter

STDMETHODIMP CMPUrlSourceSplitterInputPin::Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE * pmt)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_LOAD_NAME);

  CHECK_POINTER_HRESULT(result, pszFileName, result, E_INVALIDARG);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

  if (SUCCEEDED(result))
  {
    // stop receiving data
    this->parserHoster->StopReceivingData();
  }

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
        this->configuration->Add(new CParameter(PARAMETER_NAME_URL, url));
      }

      FREE_MEM_CLASS(suppliedParameters);
    }
    else
    {
      // parameters are not supplied, just set current url as only one parameter in configuration
      this->configuration->Clear();
      this->configuration->Add(new CParameter(PARAMETER_NAME_URL, url));
    }
  }

  if (SUCCEEDED(result))
  {
    // loads protocol based on current configuration parameters
    // it also reset all parser and protocol implementations
    result = this->Load();
  }

  FREE_MEM(url);

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LOAD_NAME, result);
  return result;

  /*
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_LOAD_NAME);

  CHECK_POINTER_HRESULT(result, pszFileName, result, E_INVALIDARG);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

  if (SUCCEEDED(result))
  {
    this->DestroyDemuxerWorker();
    // stop receiving data
    this->parserHoster->StopReceivingData();

    // reset all parser and protocol implementations
    this->parserHoster->ClearSession();
  }

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
        this->configuration->Add(new CParameter(PARAMETER_NAME_URL, url));
      }

      FREE_MEM_CLASS(suppliedParameters);
    }
    else
    {
      // parameters are not supplied, just set current url as only one parameter in configuration
      this->configuration->Clear();
      this->configuration->Add(new CParameter(PARAMETER_NAME_URL, url));
    }
  }

  if (SUCCEEDED(result))
  {
    // loads protocol based on current configuration parameters
    result = this->Load();
  }

  if (SUCCEEDED(result) && (!this->downloadingFile))
  {
    // splitter is not needed when downloading file
    result = this->CreateDemuxerWorker();
  }

  FREE_MEM(url);

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LOAD_NAME, result);
  return result;
  */
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetCurFile(LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt)
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

// IAMOpenProgress

STDMETHODIMP CMPUrlSourceSplitterInputPin::QueryProgress(LONGLONG *pllTotal, LONGLONG *pllCurrent)
{
  HRESULT result = E_NOT_VALID_STATE;

  if (this->parserHoster != NULL)
  {
    result = SUCCEEDED(this->parserHoster->GetParserHosterStatus()) ? this->parserHoster->QueryStreamProgress(pllTotal, pllCurrent) : this->parserHoster->GetParserHosterStatus();
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::AbortOperation(void)
{
  //this->DestroyDemuxerWorker();

  HRESULT result = E_NOT_VALID_STATE;

  if (this->parserHoster != NULL)
  {
    this->parserHoster->StopReceivingData();
    result = S_OK;
  }

  return result;
}

// IDownload

STDMETHODIMP CMPUrlSourceSplitterInputPin::Download(LPCOLESTR uri, LPCOLESTR fileName)
{
  HRESULT result = S_OK;
  //this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_NAME);

  //result = this->DownloadAsync(uri, fileName, this);

  //if (SUCCEEDED(result))
  //{
  //  // downloading process is successfully started
  //  // just wait for callback and return to caller
  //  while (!this->asyncDownloadFinished)
  //  {
  //    // just sleep
  //    Sleep(100);
  //  }

  //  result = this->asyncDownloadResult;
  //}

  //this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_NAME, result);
  return result;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::DownloadAsync(LPCOLESTR uri, LPCOLESTR fileName, IDownloadCallback *downloadCallback)
{
  HRESULT result = S_OK;
  //this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_ASYNC_NAME);

  //CHECK_POINTER_DEFAULT_HRESULT(result, uri);
  //CHECK_POINTER_DEFAULT_HRESULT(result, fileName);
  //CHECK_POINTER_DEFAULT_HRESULT(result, downloadCallback);
  //CHECK_POINTER_DEFAULT_HRESULT(result, this->parserHoster);

  //if (SUCCEEDED(result))
  //{
  //  // stop receiving data
  //  this->parserHoster->StopReceivingData();

  //  // reset all parser and protocol implementations
  //  this->parserHoster->ClearSession();

  //  this->asyncDownloadResult = S_OK;
  //  this->asyncDownloadFinished = false;
  //  this->asyncDownloadCallback = downloadCallback;
  //}

  //if (SUCCEEDED(result))
  //{
  //  this->downloadFileName = ConvertToUnicodeW(fileName);

  //  result = (this->downloadFileName == NULL) ? E_CONVERT_STRING_ERROR : S_OK;
  //}

  //if (SUCCEEDED(result))
  //{
  //  CParameterCollection *suppliedParameters = this->ParseParameters(uri);
  //  if (suppliedParameters != NULL)
  //  {
  //    // we have set some parameters
  //    // set them as configuration parameters
  //    this->configuration->Clear();
  //    this->configuration->Append(suppliedParameters);
  //    if (!this->configuration->Contains(PARAMETER_NAME_URL, true))
  //    {
  //      this->configuration->Add(new CParameter(PARAMETER_NAME_URL, uri));
  //    }
  //    this->configuration->Add(new CParameter(PARAMETER_NAME_DOWNLOAD_FILE_NAME, this->downloadFileName));

  //    FREE_MEM_CLASS(suppliedParameters);
  //  }
  //  else
  //  {
  //    // parameters are not supplied, just set current url and download file name as only parameters in configuration
  //    this->configuration->Clear();
  //    this->configuration->Add(new CParameter(PARAMETER_NAME_URL, uri));
  //    this->configuration->Add(new CParameter(PARAMETER_NAME_DOWNLOAD_FILE_NAME, this->downloadFileName));
  //  }
  //}

  //if (SUCCEEDED(result))
  //{
  //  // loads protocol based on current configuration parameters
  //  result = this->Load();
  //}

  //this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_ASYNC_NAME, result);
  return result;
}

// IDownloadCallback

void STDMETHODCALLTYPE CMPUrlSourceSplitterInputPin::OnDownloadCallback(HRESULT downloadResult)
{
  //this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_CALLBACK_NAME);

  //this->asyncDownloadResult = downloadResult;
  //this->asyncDownloadFinished = true;

  //if ((this->asyncDownloadCallback != NULL) && (this->asyncDownloadCallback != this))
  //{
  //  // if download callback is set and it is not current instance (avoid recursion)
  //  this->asyncDownloadCallback->OnDownloadCallback(downloadResult);
  //}

  //this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DOWNLOAD_CALLBACK_NAME);
}

// IOutputStream

HRESULT CMPUrlSourceSplitterInputPin::SetTotalLength(int64_t total, bool estimate)
{
  HRESULT result = E_FAIL;

  {
    CLockMutex lock(this->mediaPacketMutex, INFINITE);

    this->totalLength = total;
    this->estimate = estimate;

    result = S_OK;
  }

  return result;
}

HRESULT CMPUrlSourceSplitterInputPin::PushMediaPackets(CMediaPacketCollection *mediaPackets)
{
  this->logger->Log(LOGGER_DATA, METHOD_START_FORMAT, MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME);
  HRESULT result = S_OK;

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
        this->logger->Log(LOGGER_DATA, L"%s: %s: media packet start: %016llu, length: %08u", MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, start, mediaPacket->GetBuffer()->GetBufferOccupiedSpace());

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
              this->logger->Log(LOGGER_DATA, L"%s: %s: no overlapped region", MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME);

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

              this->logger->Log(LOGGER_DATA, L"%s: %s: overlapped region, start: %016llu, end: %016llu", MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, overlappingRegionStart, overlappingRegionEnd);

              if (SUCCEEDED(result) && (unprocessedMediaPacketStart < overlappingRegionStart))
              {
                // initialize part
                int64_t start = unprocessedMediaPacketStart;
                int64_t end = overlappingRegionStart - 1;
                CMediaPacket *part = unprocessedMediaPacket->CreateMediaPacketBasedOnPacket(start, end);

                this->logger->Log(LOGGER_DATA, L"%s: %s: creating packet, start: %016llu, end: %016llu", MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, start, end);

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

                this->logger->Log(LOGGER_DATA, L"%s: %s: creating packet, start: %016llu, end: %016llu", MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, start, end);

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
          delete unprocessedMediaPacket;
        }
      }

      // media packets collection is not longer needed
      delete unprocessedMediaPackets;
    }
  }

  this->logger->Log(SUCCEEDED(result) ? LOGGER_DATA : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_PUSH_MEDIA_PACKETS_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitterInputPin::EndOfStreamReached(int64_t streamPosition)
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

        if ((!estimate) && (startPosition >= this->totalLength) && (i == 0))
        {
          // we are after end of stream
          // check media packets from start if we don't have gap
          startPosition = 0;
          endPosition = 0;
          mediaPacketIndex = this->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(startPosition);
          this->totalLengthReceived = true;
          this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, L"searching for gap in media packets from beginning");
        }
        else
        {
          // we found some gap
          break;
        }
      }

      if (((!estimate) && (startPosition < this->totalLength)) || (estimate))
      {
        // found part which is not downloaded
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: requesting stream part from: %llu, to: %llu", MODULE_NAME, METHOD_END_OF_STREAM_REACHED_NAME, startPosition, endPosition);
        this->SeekToPosition(startPosition, endPosition);
      }
      else
      {
        // all data received
        this->allDataReceived = true;
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

bool CMPUrlSourceSplitterInputPin::IsDownloading(void)
{
  //return this->downloadingFile;
  return false;
}

void CMPUrlSourceSplitterInputPin::FinishDownload(HRESULT result)
{
  this->OnDownloadCallback(result);
}

// IFilter

CLogger *CMPUrlSourceSplitterInputPin::GetLogger(void)
{
  return this->logger;
}

HRESULT CMPUrlSourceSplitterInputPin::GetTotalLength(int64_t *totalLength)
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

HRESULT CMPUrlSourceSplitterInputPin::GetAvailableLength(int64_t *availableLength)
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

// ISeeking

unsigned int CMPUrlSourceSplitterInputPin::GetSeekingCapabilities(void)
{
  unsigned int capabilities = SEEKING_METHOD_NONE;

  if (this->parserHoster != NULL)
  {
    capabilities = this->parserHoster->GetSeekingCapabilities();
  }

  return capabilities;
}

int64_t CMPUrlSourceSplitterInputPin::SeekToTime(int64_t time)
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

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length to zero, estimate: %d", MODULE_NAME, METHOD_SEEK_TO_TIME_NAME, SUCCEEDED(result) ? 1 : 0);
      this->SetTotalLength(0, SUCCEEDED(result));
    }

    // if correctly seeked than reset flag that all data are received
    // in another case we don't received any other data
    this->allDataReceived = (result < 0);

    // now we are ready to receive data
    // notify protocol that we can receive data
    this->parserHoster->SetSupressData(false);
  }

  return result;

  /*
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
      this->m_llBufferPosition = 0;

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length to zero, estimate: %d", MODULE_NAME, METHOD_SEEK_TO_TIME_NAME, SUCCEEDED(result) ? 1 : 0);
      this->SetTotalLength(0, SUCCEEDED(result));
    }

    // if correctly seeked than reset flag that all data are received
    // in another case we don't received any other data
    this->allDataReceived = (result < 0);

    // now we are ready to receive data
    // notify protocol that we can receive data
    this->parserHoster->SetSupressData(false);
  }

  return result;
  */
}

int64_t CMPUrlSourceSplitterInputPin::SeekToPosition(int64_t start, int64_t end)
{
  int64_t result = E_NOT_VALID_STATE;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->SeekToPosition(start, end);
  }

  return result;
}

void CMPUrlSourceSplitterInputPin::SetSupressData(bool supressData)
{
  if (this->parserHoster != NULL)
  {
    this->parserHoster->SetSupressData(supressData);
  }
}

// IMediaSeeking

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetCapabilities(DWORD* pCapabilities)
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

STDMETHODIMP CMPUrlSourceSplitterInputPin::CheckCapabilities(DWORD* pCapabilities)
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

STDMETHODIMP CMPUrlSourceSplitterInputPin::IsFormatSupported(const GUID* pFormat)
{
  return !pFormat ? E_POINTER : *pFormat == TIME_FORMAT_MEDIA_TIME ? S_OK : S_FALSE;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::QueryPreferredFormat(GUID* pFormat)
{
  return this->GetTimeFormat(pFormat);
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetTimeFormat(GUID* pFormat)
{
  return pFormat ? *pFormat = TIME_FORMAT_MEDIA_TIME, S_OK : E_POINTER;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::IsUsingTimeFormat(const GUID* pFormat)
{
  return this->IsFormatSupported(pFormat);
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::SetTimeFormat(const GUID* pFormat)
{
  return S_OK == this->IsFormatSupported(pFormat) ? S_OK : E_INVALIDARG;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetDuration(LONGLONG* pDuration)
{
  /*CheckPointer(pDuration, E_POINTER);
  CBaseDemuxer *demuxer = this->filter->GetDemuxer();
  CheckPointer(demuxer, E_UNEXPECTED);
  
  *pDuration = demuxer->GetDuration();

  return (*pDuration < 0) ? E_FAIL : S_OK;*/

  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetStopPosition(LONGLONG* pStop)
{
  return this->GetDuration(pStop);
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetCurrentPosition(LONGLONG* pCurrent)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat)
{
  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::SetPositions(LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags)
{
  //return this->SetPositionsInternal(this, pCurrent, dwCurrentFlags, pStop, dwStopFlags);

  // SetPositionsInternal()

  //this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_SET_POSITIONS_INTERNAL_NAME);
  //this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek request; this: %p; caller: %p, current: %I64d; start: %I64d; flags: 0x%08X, stop: %I64d; flags: 0x%08X", MODULE_NAME, METHOD_SET_POSITIONS_INTERNAL_NAME, this, caller, this->m_rtCurrent, pCurrent ? *pCurrent : -1, dwCurrentFlags, pStop ? *pStop : -1, dwStopFlags);

  //CAutoLock cAutoLock(this);
  //HRESULT result = E_FAIL;

  //if ((pCurrent == NULL) && (pStop == NULL)
  //  || (((dwCurrentFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_NoPositioning)
  //    && ((dwStopFlags & AM_SEEKING_PositioningBitsMask) == AM_SEEKING_NoPositioning)))
  //{
  //    result = S_OK;
  //}
  //else
  //{
  //  REFERENCE_TIME rtCurrent = this->m_rtCurrent, rtStop = this->m_rtStop;

  //  if (pCurrent != NULL)
  //  {
  //    switch(dwCurrentFlags & AM_SEEKING_PositioningBitsMask)
  //    {
  //    case AM_SEEKING_NoPositioning:
  //      break;
  //    case AM_SEEKING_AbsolutePositioning:
  //      rtCurrent = *pCurrent;
  //      break;
  //    case AM_SEEKING_RelativePositioning:
  //      rtCurrent = rtCurrent + *pCurrent;
  //      break;
  //    case AM_SEEKING_IncrementalPositioning:
  //      rtCurrent = rtCurrent + *pCurrent;
  //      break;
  //    }
  //  }

  //  if (pStop != NULL)
  //  {
  //    switch(dwStopFlags & AM_SEEKING_PositioningBitsMask)
  //    {
  //    case AM_SEEKING_NoPositioning:
  //      break;
  //    case AM_SEEKING_AbsolutePositioning:
  //      rtStop = *pStop;
  //      this->m_bStopValid = TRUE;
  //      break;
  //    case AM_SEEKING_RelativePositioning:
  //      rtStop += *pStop;
  //      this->m_bStopValid = TRUE;
  //      break;
  //    case AM_SEEKING_IncrementalPositioning:
  //      rtStop = rtCurrent + *pStop;
  //      this->m_bStopValid = TRUE;
  //      break;
  //    }
  //  }

  //  if ((this->m_rtCurrent == rtCurrent) && (this->m_rtStop == rtStop))
  //  {
  //    result = S_OK;
  //  }
  //  else
  //  {
  //    if ((this->m_rtLastStart == rtCurrent) && (this->m_rtLastStop == rtStop) && (this->m_LastSeekers.find(caller) == this->m_LastSeekers.end()))
  //    {
  //      this->m_LastSeekers.insert(caller);
  //      result = S_OK;
  //    }
  //    else
  //    {
  //      this->m_rtLastStart = rtCurrent;
  //      this->m_rtLastStop = rtStop;
  //      this->m_LastSeekers.clear();
  //      this->m_LastSeekers.insert(caller);

  //      this->m_rtNewStart = this->m_rtCurrent = rtCurrent;
  //      this->m_rtNewStop = rtStop;

  //      // perform seek in CLAVSplitter::SetPositionsInternal()
  //      result = S_FALSE;
  //    }
  //  }
  //}
  //this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SET_POSITIONS_INTERNAL_NAME, result);
  //return result;

  return E_NOTIMPL;
}

//STDMETHODIMP CMPUrlSourceSplitterInputPin::SetPositionsInternal(void *caller, LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags)
//{
//  
//}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetPositions(LONGLONG* pCurrent, LONGLONG* pStop)
{
  /*if (pCurrent)
  {
    *pCurrent = m_rtCurrent;
  }
  if (pStop)
  {
    *pStop = m_rtStop;
  }
  return S_OK;*/

  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest)
{
  if (pEarliest)
  {
    *pEarliest = 0;
  }
  return this->GetDuration(pLatest);
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::SetRate(double dRate)
{
  //return dRate > 0 ? m_dRate = dRate, S_OK : E_INVALIDARG;

  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetRate(double* pdRate)
{
  //return pdRate ? *pdRate = m_dRate, S_OK : E_POINTER;

  return E_NOTIMPL;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::GetPreroll(LONGLONG* pllPreroll)
{
  //return pllPreroll ? *pllPreroll = 0, S_OK : E_POINTER;

  return E_NOTIMPL;
}

// other methods */

HRESULT CMPUrlSourceSplitterInputPin::GetParserHosterStatus(void)
{
  if (this->parserHoster != NULL)
  {
    return this->parserHoster->GetParserHosterStatus();
  }

  return E_NOT_VALID_STATE;
}

HRESULT CMPUrlSourceSplitterInputPin::GetNextPacket(COutputPinPacket *packet)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->mediaPacketMutex, INFINITE);

    result = (this->mediaPacketCollection->Count() > 0) ? S_OK : S_FALSE;

    if (result == S_OK)
    {
      CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(0);

      if (SUCCEEDED(result))
      {
        result = packet->CreateBuffer(mediaPacket->GetBuffer()->GetBufferOccupiedSpace()) ? result : E_OUTOFMEMORY;
      }

      if (SUCCEEDED(result))
      {
        unsigned int size = mediaPacket->GetBuffer()->GetBufferOccupiedSpace();
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, size, 0);
        CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          mediaPacket->GetBuffer()->CopyFromBuffer(buffer, size, 0, 0);

          result = (packet->GetBuffer()->AddToBufferWithResize(buffer, size) == size) ? result : E_OUTOFMEMORY;
        }

        FREE_MEM(buffer);
      }

      if (SUCCEEDED(result))
      {
        this->mediaPacketCollection->Remove(0);
      }
    }
  }

  return result;
}

const wchar_t *CMPUrlSourceSplitterInputPin::GetStoreFilePath(void)
{
  return NULL;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::Load()
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

CParameterCollection *CMPUrlSourceSplitterInputPin::ParseParameters(const wchar_t *parameters)
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

                  char *curlValue = ConvertToMultiByte(value);
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

unsigned int CMPUrlSourceSplitterInputPin::GetReceiveDataTimeout(void)
{
  unsigned int result = UINT_MAX;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->GetReceiveDataTimeout();
  }

  return result;
}

STDMETHODIMP CMPUrlSourceSplitterInputPin::Length(LONGLONG *total, LONGLONG *available)
{
  //this->logger->Log((this->filter->GetLastCommand() != CMPUrlSourceSplitter::CMD_PLAY) ? LOGGER_VERBOSE : LOGGER_DATA, METHOD_START_FORMAT, MODULE_NAME, METHOD_LENGTH_NAME);

  //HRESULT result = S_OK;
  //CHECK_POINTER_DEFAULT_HRESULT(result, total);
  //CHECK_POINTER_DEFAULT_HRESULT(result, available);

  //unsigned int mediaPacketCount = 0;
  //{
  //  CLockMutex lock(this->mediaPacketMutex, INFINITE);
  //  mediaPacketCount = this->mediaPacketCollection->Count();
  //}

  //if (SUCCEEDED(result))
  //{
  //  *total = this->totalLength;
  //  *available = this->totalLength;
  //  
  //  CStreamAvailableLength *availableLength = new CStreamAvailableLength();
  //  result = this->QueryStreamAvailableLength(availableLength);
  //  if (SUCCEEDED(result))
  //  {
  //    result = availableLength->GetQueryResult();
  //  }

  //  if (SUCCEEDED(result))
  //  {
  //    *available = availableLength->GetAvailableLength();
  //  }
  //  
  //  if (FAILED(result))
  //  {
  //    // error occured while requesting stream available length
  //    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: cannot query available stream length, result: 0x%08X", MODULE_NAME, METHOD_LENGTH_NAME, result);

  //    CLockMutex lock(this->mediaPacketMutex, INFINITE);
  //    mediaPacketCount = this->mediaPacketCollection->Count();

  //    // return default value = last media packet end
  //    *available = 0;
  //    for (unsigned int i = 0; i < mediaPacketCount; i++)
  //    {
  //      CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(i);
  //      int64_t mediaPacketStart = mediaPacket->GetStart();
  //      int64_t mediaPacketEnd = mediaPacket->GetEnd();

  //      if ((mediaPacketEnd + 1) > (*available))
  //      {
  //        *available = mediaPacketEnd + 1;
  //      }
  //    }

  //    result = S_OK;
  //  }
  //  FREE_MEM_CLASS(availableLength);

  //  result = (this->estimate) ? VFW_S_ESTIMATED : S_OK;
  //  this->logger->Log((this->filter->GetLastCommand() != CMPUrlSourceSplitter::CMD_PLAY) ? LOGGER_VERBOSE : LOGGER_DATA, L"%s: %s: total length: %llu, available length: %llu, estimate: %u, media packets: %u", MODULE_NAME, METHOD_LENGTH_NAME, this->totalLength, *available, (this->estimate) ? 1 : 0, mediaPacketCount);
  //}

  //this->logger->Log((this->filter->GetLastCommand() != CMPUrlSourceSplitter::CMD_PLAY) ? LOGGER_VERBOSE : LOGGER_DATA, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LENGTH_NAME, result);
  //return result;

  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitterInputPin::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
{
  HRESULT result = E_NOTIMPL;

  if (this->parserHoster != NULL)
  {
    result = this->parserHoster->QueryStreamAvailableLength(availableLength);
  }

  return result;
}