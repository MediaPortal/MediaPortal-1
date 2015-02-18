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

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "MPUrlSourceSplitter_Parser_Mpeg2TS.h"
#include "MPUrlSourceSplitter_Parser_MPEG2TS_Parameters.h"
#include "ParserPluginConfiguration.h"
#include "StreamPackage.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "StreamInformationCollection.h"
#include "Parameters.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "TsPacket.h"
#include "TsPacketConstants.h"
#include "LockMutex.h"
#include "Mpeg2TsDumpBox.h"
#include "ConditionalAccessDescriptor.h"
#include "ConditionalAccessParser.h"

#include <process.h>

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_Mpeg2TSd"
#else
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_Mpeg2TS"
#endif

// 32 KB of data to request at start
#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DATA_LENGTH_DEFAULT             32768

#define SLEEP_MODE_NO                                                         0
#define SLEEP_MODE_SHORT                                                      1
#define SLEEP_MODE_LONG                                                       2

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Parser_Mpeg2TS(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Parser_Mpeg2TS *parserPlugin = (CMPUrlSourceSplitter_Parser_Mpeg2TS *)plugin;

    delete parserPlugin;
  }
}

CMPUrlSourceSplitter_Parser_Mpeg2TS::CMPUrlSourceSplitter_Parser_Mpeg2TS(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CParserPlugin(result, logger, configuration)
{
  this->lastReceivedLength = 0;
  this->receiveDataWorkerShouldExit = false;
  this->receiveDataWorkerThread = NULL;
  this->mutex = NULL;
  this->cacheFile = NULL;
  this->streamFragments = NULL;
  this->lastStoreTime = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;
  this->streamLength = 0;
  this->streamPackage = NULL;
  this->pauseSeekStopMode = PAUSE_SEEK_STOP_MODE_NONE;
  this->positionOffset = 0;
  this->discontinuityParser = NULL;
  this->transportStreamId = MPEG2TS_TRANSPORT_STREAM_ID_DEFAULT;
  this->programNumber = MPEG2TS_PROGRAM_NUMBER_DEFAULT;
  this->programMapPID = MPEG2TS_PROGRAM_MAP_PID_DEFAULT;
  this->programAssociationParserContext = NULL;
  this->transportStreamProgramMapParserContextCollection = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->mutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->streamFragments = new CMpeg2tsStreamFragmentCollection(result);
    this->discontinuityParser = new CDiscontinuityParser(result);
    this->programAssociationParserContext = new CProgramAssociationParserContext(result);
    this->transportStreamProgramMapParserContextCollection = new CTransportStreamProgramMapParserContextCollection(result);

    CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->mutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->discontinuityParser, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->programAssociationParserContext, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->transportStreamProgramMapParserContextCollection, *result, E_OUTOFMEMORY);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Parser_Mpeg2TS::~CMPUrlSourceSplitter_Parser_Mpeg2TS()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->streamFragments);
  FREE_MEM_CLASS(this->discontinuityParser);
  FREE_MEM_CLASS(this->programAssociationParserContext);
  FREE_MEM_CLASS(this->transportStreamProgramMapParserContextCollection);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
    this->mutex = NULL;
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CParserPlugin

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::GetParserResult(void)
{
  if (this->parserResult == PARSER_RESULT_PENDING)
  {
    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_ALIGN_TO_MPEG2TS_PACKET))
    {
      // MPEG2 TS parser is allowed only in case when stream is correctly aligned to MPEG2 TS packet boundaries

      if (this->IsSetAnyOfFlags(
        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_DETECT_DISCONTINUITY |
        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_ALIGN_TO_MPEG2TS_PACKET |
        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID |
        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
      {
        // allowed detection of discontinuity of packets
        // allowed aligning of MPEG2 TS packets

        CStreamInformationCollection *streams = new CStreamInformationCollection(&this->parserResult);
        CHECK_POINTER_HRESULT(this->parserResult, streams, this->parserResult, E_OUTOFMEMORY);

        CHECK_HRESULT_EXECUTE(this->parserResult, this->protocolHoster->GetStreamInformation(streams));

        if (SUCCEEDED(this->parserResult) && (streams->Count() == 1))
        {
          CStreamPackage *package = new CStreamPackage(&this->parserResult);
          CHECK_POINTER_HRESULT(this->parserResult, package, this->parserResult, E_OUTOFMEMORY);

          if (SUCCEEDED(this->parserResult))
          {
            unsigned int requestLength = MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DATA_LENGTH_DEFAULT;
            bool receivedSameLength = false;
            this->parserResult = PARSER_RESULT_PENDING;

            while ((this->parserResult == PARSER_RESULT_PENDING) && (!receivedSameLength))
            {
              package->Clear();

              CStreamPackageDataRequest *request = new CStreamPackageDataRequest(&this->parserResult);
              CHECK_POINTER_HRESULT(this->parserResult, request, this->parserResult, E_OUTOFMEMORY);

              if (SUCCEEDED(this->parserResult))
              {
                request->SetStart(0);
                request->SetLength(requestLength);
                request->SetAnyDataLength(true);

                package->SetRequest(request);
              }

              CHECK_CONDITION_EXECUTE(FAILED(this->parserResult), FREE_MEM_CLASS(request));
              CHECK_HRESULT_EXECUTE(this->parserResult, this->protocolHoster->ProcessStreamPackage(package));

              if (SUCCEEDED(this->parserResult))
              {
                this->parserResult = PARSER_RESULT_PENDING;
                CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(package->GetResponse());

                if (package->IsError())
                {
                  // TO DO: check type of error

                  this->parserResult = PARSER_RESULT_NOT_KNOWN;
                }

                if (response != NULL)
                {
                  receivedSameLength = (response->GetBuffer()->GetBufferOccupiedSpace() == this->lastReceivedLength);
                  if (!receivedSameLength)
                  {
                    // try parse data
                    int res = CTsPacket::FindPacket(response->GetBuffer(), TS_PACKET_MINIMUM_CHECKED_UNSPECIFIED);

                    switch (res)
                    {
                    case TS_PACKET_FIND_RESULT_NOT_FOUND:
                      this->parserResult = PARSER_RESULT_NOT_KNOWN;
                      break;
                    case TS_PACKET_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER:
                    case TS_PACKET_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS:
                      this->parserResult = PARSER_RESULT_PENDING;
                      break;
                    case TS_PACKET_FIND_RESULT_NOT_ENOUGH_MEMORY:
                      this->parserResult = E_OUTOFMEMORY;
                      break;
                    default:
                      // we found at least TS_PACKET_MINIMUM_CHECKED MPEG2 TS packets
                      this->parserResult = PARSER_RESULT_KNOWN;
                      break;
                    }

                    requestLength *= 2;
                  }

                  if (response->IsNoMoreDataAvailable() && (this->parserResult == PARSER_RESULT_PENDING))
                  {
                    this->parserResult = PARSER_RESULT_NOT_KNOWN;
                  }

                  this->lastReceivedLength = response->GetBuffer()->GetBufferOccupiedSpace();
                }
              }
            }
          }

          FREE_MEM_CLASS(package);
        }
        else
        {
          // MPEG2 TS parser doesn't support multiple streams
          this->parserResult = PARSER_RESULT_NOT_KNOWN;
        }

        FREE_MEM_CLASS(streams);
      }
      else
      {
        this->parserResult = PARSER_RESULT_NOT_KNOWN;
      }
    }
    else
    {
      this->parserResult = PARSER_RESULT_NOT_KNOWN;
    }
  }

  return this->parserResult;
}

unsigned int CMPUrlSourceSplitter_Parser_Mpeg2TS::GetParserScore(void)
{
  return 100;
}

CParserPlugin::Action CMPUrlSourceSplitter_Parser_Mpeg2TS::GetAction(void)
{
  return ParseStream;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::SetConnectionParameters(const CParameterCollection *parameters)
{
  HRESULT result = __super::SetConnectionParameters(parameters);

  if (SUCCEEDED(result))
  {
    this->flags &= ~(
      MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_DETECT_DISCONTINUITY |
      MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_ALIGN_TO_MPEG2TS_PACKET |
      MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
      MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
      MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID |
      MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED);

    this->flags |= this->connectionParameters->GetValueBool(PARAMETER_NAME_MPEG2TS_DETECT_DISCONTINUITY, true, MPEG2TS_DETECT_DISCONTINUITY_DEFAULT) ? MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_DETECT_DISCONTINUITY : MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE;
    this->flags |= this->connectionParameters->GetValueBool(PARAMETER_NAME_MPEG2TS_ALIGN_TO_MPEG2TS_PACKET, true, MPEG2TS_ALIGN_TO_MPEG2TS_PACKET) ? MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_ALIGN_TO_MPEG2TS_PACKET : MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE;

    this->transportStreamId = this->connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_MPEG2TS_TRANSPORT_STREAM_ID, true, MPEG2TS_TRANSPORT_STREAM_ID_DEFAULT);
    this->programNumber = this->connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_MPEG2TS_PROGRAM_NUMBER, true, MPEG2TS_PROGRAM_NUMBER_DEFAULT);
    this->programMapPID = this->connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_MPEG2TS_PROGRAM_MAP_PID, true, MPEG2TS_PROGRAM_MAP_PID_DEFAULT);

    this->flags |= (this->transportStreamId != MPEG2TS_TRANSPORT_STREAM_ID_DEFAULT) ? MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID : MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE;
    this->flags |= (this->programNumber != MPEG2TS_PROGRAM_NUMBER_DEFAULT) ? MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER : MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE;
    this->flags |= (this->programMapPID != MPEG2TS_PROGRAM_MAP_PID_DEFAULT) ? MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID : MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE;

    this->flags |= this->connectionParameters->GetValueBool(PARAMETER_NAME_MPEG2TS_SET_NOT_SCRAMBLED, true, MPEG2TS_SET_NOT_SCRAMBLED_DEFAULT) ? MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED : MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE;
  }

  return result;
}

bool CMPUrlSourceSplitter_Parser_Mpeg2TS::IsSetStreamLength(void)
{
  return this->IsSetFlags(PARSER_PLUGIN_FLAG_SET_STREAM_LENGTH);
}

bool CMPUrlSourceSplitter_Parser_Mpeg2TS::IsStreamLengthEstimated(void)
{
  return this->IsSetFlags(PARSER_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED);
}

bool CMPUrlSourceSplitter_Parser_Mpeg2TS::IsWholeStreamDownloaded(void)
{
  return this->IsSetFlags(PARSER_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED);
}

bool CMPUrlSourceSplitter_Parser_Mpeg2TS::IsEndOfStreamReached(void)
{
  return this->IsSetFlags(PARSER_PLUGIN_FLAG_END_OF_STREAM_REACHED);
}

bool CMPUrlSourceSplitter_Parser_Mpeg2TS::IsStreamIptvCompatible(void)
{
  return (this->parserResult == PARSER_RESULT_KNOWN);
}

// CPlugin

const wchar_t *CMPUrlSourceSplitter_Parser_Mpeg2TS::GetName(void)
{
  return PARSER_NAME;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = __super::Initialize(configuration);

  if (SUCCEEDED(result))
  {
    CParserPluginConfiguration *parserConfiguration = (CParserPluginConfiguration *)configuration;
    CHECK_POINTER_HRESULT(result, parserConfiguration, result, E_INVALIDARG);
  }

  if (SUCCEEDED(result))
  {
    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PARSER_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
  }

  return result;
}

// ISeeking interface

void CMPUrlSourceSplitter_Parser_Mpeg2TS::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->protocolHoster->SetPauseSeekStopMode(pauseSeekStopMode);

  LOCK_MUTEX(this->mutex, INFINITE)

  this->pauseSeekStopMode = pauseSeekStopMode;

  UNLOCK_MUTEX(this->mutex)
}

int64_t CMPUrlSourceSplitter_Parser_Mpeg2TS::SeekToTime(unsigned int streamId, int64_t time)
{
  int64_t result = -1;
  LOCK_MUTEX(this->mutex, INFINITE)

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PARSER_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  result = this->protocolHoster->SeekToTime(streamId, time);

  if (result != (-1))
  {
    this->flags &= ~(PARSER_PLUGIN_FLAG_SET_STREAM_LENGTH | PARSER_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PARSER_PLUGIN_FLAG_END_OF_STREAM_REACHED);
    this->flags |= PARSER_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

    this->streamLength = 0;
    this->streamFragments->Clear();
    this->cacheFile->Clear();
    this->streamFragmentDownloading = UINT_MAX;
    this->streamFragmentToDownload = UINT_MAX;
    this->currentProcessedSize = 0;
    this->positionOffset = 0;
    this->reportedStreamTime = 0;
    this->reportedStreamPosition = 0;
    this->discontinuityParser->Clear();

    HRESULT res = S_OK;
    CMpeg2tsStreamFragment *fragment = new CMpeg2tsStreamFragment(&res);
    CHECK_POINTER_HRESULT(res, fragment, res, E_OUTOFMEMORY);

    if (SUCCEEDED(res))
    {
      fragment->SetFragmentStartPosition(0);
      fragment->SetRequestStartPosition(0);
    }

    CHECK_CONDITION_HRESULT(res, this->streamFragments->Add(fragment), res, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(res), FREE_MEM_CLASS(fragment));

    if (SUCCEEDED(res))
    {
      this->streamFragmentToDownload = 0;

      // set start searching index to current processing stream fragment
      this->streamFragments->SetStartSearchingIndex(this->streamFragmentToDownload);
      // set count of fragments to search for specific position
      unsigned int firstNotDownloadedFragmentIndex = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentToDownload);
      this->streamFragments->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? this->streamFragments->Count() : firstNotDownloadedFragmentIndex) - this->streamFragmentToDownload);
    }

    result = SUCCEEDED(res) ? result : (-1);
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);

  UNLOCK_MUTEX(this->mutex)
  return result;
}

// IDemuxerOwner interface

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    LOCK_MUTEX(this->mutex, INFINITE)

    this->streamPackage = streamPackage;

    UNLOCK_MUTEX(this->mutex)

    if (this->streamPackage != NULL)
    {
      if (WaitForSingleObject(this->streamPackage->GetProcessedEventHandle(), INFINITE) == WAIT_OBJECT_0)
      {
        while (this->streamPackage != NULL)
        {
          // lock mutex to get exclussive access to stream package
          // don't wait too long
          LOCK_MUTEX(this->mutex, 20)

          if (streamPackage->GetState() == CStreamPackage::Completed)
          {
            this->streamPackage = NULL;
          }

          UNLOCK_MUTEX(this->mutex)

          if (this->streamPackage != NULL)
          {
            // sleep some time
            Sleep(1);
          }
        }
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    streamProgress->SetTotalLength((this->streamLength == 0) ? 1 : this->streamLength);
    streamProgress->SetCurrentLength((this->streamLength == 0) ? 0 : this->GetBytePosition());

    if (this->IsStreamLengthEstimated())
    {
      result = VFW_S_ESTIMATED;
    }
  }

  return result;
}

// ISimpleProtocol interface

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;

  if (this->streamFragments->Count() == 0)
  {
    CMpeg2tsStreamFragment *fragment = new CMpeg2tsStreamFragment(&result);
    CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      fragment->SetFragmentStartPosition(0);
      fragment->SetRequestStartPosition(0);
    }

    CHECK_CONDITION_HRESULT(result, this->streamFragments->Add(fragment), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));

    if (SUCCEEDED(result))
    {
      this->streamFragmentToDownload = 0;
      this->positionOffset = 0;

      // set start searching index to current processing stream fragment
      this->streamFragments->SetStartSearchingIndex(this->streamFragmentToDownload);
      // set count of fragments to search for specific position
      unsigned int firstNotDownloadedFragmentIndex = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentToDownload);
      this->streamFragments->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? this->streamFragments->Count() : firstNotDownloadedFragmentIndex) - this->streamFragmentToDownload);
    }
  }

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->CreateReceiveDataWorker());

  return result;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::StopReceivingData(void)
{
  return this->DestroyReceiveDataWorker();
}

void CMPUrlSourceSplitter_Parser_Mpeg2TS::ClearSession(void)
{
  // stop receiving data
  this->StopReceivingData();

  __super::ClearSession();

  this->lastReceivedLength = 0;
  this->cacheFile->Clear();
  this->streamFragments->Clear();
  this->lastStoreTime = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;
  this->streamLength = 0;
  this->streamPackage = NULL;
  this->pauseSeekStopMode = PAUSE_SEEK_STOP_MODE_NONE;
  this->positionOffset = 0;
  this->discontinuityParser->Clear();
  this->transportStreamId = MPEG2TS_TRANSPORT_STREAM_ID_DEFAULT;
  this->programNumber = MPEG2TS_PROGRAM_NUMBER_DEFAULT;
  this->programMapPID = MPEG2TS_PROGRAM_MAP_PID_DEFAULT;
  this->programAssociationParserContext->Clear();
  this->transportStreamProgramMapParserContextCollection->Clear();
}

// IProtocol interface

/* protected methods */

wchar_t *CMPUrlSourceSplitter_Parser_Mpeg2TS::GetStoreFile(const wchar_t *extension)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->connectionParameters->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_parser_mpeg2ts_%s.%s", folder, guid, extension);
    }
    FREE_MEM(guid);
  }

  return result;
}

int64_t CMPUrlSourceSplitter_Parser_Mpeg2TS::GetBytePosition(void)
{
  int64_t result = 0;

  LOCK_MUTEX(this->mutex, INFINITE)

  unsigned int first = this->streamFragments->GetStartSearchingIndex();
  unsigned int count = this->streamFragments->GetSearchCount();

  if (count != 0)
  {
    CMpeg2tsStreamFragment *firstFragment = this->streamFragments->GetItem(first);
    CMpeg2tsStreamFragment *lastFragment = this->streamFragments->GetItem(first + count - 1);

    result = lastFragment->GetFragmentStartPosition() + (int64_t)lastFragment->GetLength() - firstFragment->GetFragmentStartPosition() + this->positionOffset;
  }

  UNLOCK_MUTEX(this->mutex)

  return result;
}

/* receive data worker */

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::CreateReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME);

  if (this->receiveDataWorkerThread == NULL)
  {
    this->receiveDataWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CMPUrlSourceSplitter_Parser_Mpeg2TS::ReceiveDataWorker, this, 0, NULL);
  }

  if (this->receiveDataWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CREATE_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::DestroyReceiveDataWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME);

  this->receiveDataWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->receiveDataWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->receiveDataWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->receiveDataWorkerThread, 0);
    }
    CloseHandle(this->receiveDataWorkerThread);
  }

  this->receiveDataWorkerThread = NULL;
  this->receiveDataWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME, result);
  return result;
}

unsigned int WINAPI CMPUrlSourceSplitter_Parser_Mpeg2TS::ReceiveDataWorker(LPVOID lpParam)
{
  CMPUrlSourceSplitter_Parser_Mpeg2TS *caller = (CMPUrlSourceSplitter_Parser_Mpeg2TS *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME);

  HRESULT result = S_OK;
  unsigned int requestId = 0;

  unsigned int sleepMode = SLEEP_MODE_LONG;

  while (!caller->receiveDataWorkerShouldExit)
  {
    sleepMode = SLEEP_MODE_LONG;

    if (SUCCEEDED(result) && (caller->streamFragments->HasReadyForAlignStreamFragments()))
    {
      sleepMode = SLEEP_MODE_SHORT;

      // don't wait too long, we can align stream fragments later
      LOCK_MUTEX(caller->mutex, 20)

      CIndexedMpeg2tsStreamFragmentCollection *indexedReadyForAlignStreamFragments = new CIndexedMpeg2tsStreamFragmentCollection(&result);
      CHECK_POINTER_HRESULT(result, indexedReadyForAlignStreamFragments, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = caller->streamFragments->GetReadyForAlignStreamFragments(indexedReadyForAlignStreamFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedReadyForAlignStreamFragments->Count())); i++)
      {
        CIndexedMpeg2tsStreamFragment *indexedReadyForAlignStreamFragment = indexedReadyForAlignStreamFragments->GetItem(i);
        CMpeg2tsStreamFragment *currentReadyForAlignStreamFragment = indexedReadyForAlignStreamFragment->GetItem();
        CMpeg2tsStreamFragment *nextStreamFragment = currentReadyForAlignStreamFragment->IsDiscontinuity() ? NULL : caller->streamFragments->GetItem(indexedReadyForAlignStreamFragment->GetItemIndex() + 1);

        // we assume that previous stream fragments are aligned
        // if some data will remain in current stream fragment, we add them to start of next stream fragment (or drop)

        CLinearBuffer *processingBuffer = currentReadyForAlignStreamFragment->GetBuffer()->Clone();
        CLinearBuffer *fragmentBuffer = new CLinearBuffer(&result, currentReadyForAlignStreamFragment->GetBuffer()->GetBufferOccupiedSpace());

        CHECK_POINTER_HRESULT(result, processingBuffer, result, E_OUTOFMEMORY);
        CHECK_POINTER_HRESULT(result, fragmentBuffer, result, E_OUTOFMEMORY);

        if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_ALIGN_TO_MPEG2TS_PACKET))
        {
          while (SUCCEEDED(result) && (processingBuffer->GetBufferOccupiedSpace() >= TS_PACKET_SIZE))
          {
            unsigned int firstPacketPosition = 0;
            unsigned int packetSequenceLength = 0;

            result = CTsPacket::FindPacketSequence(processingBuffer, &firstPacketPosition, &packetSequenceLength);

            if (SUCCEEDED(result))
            {
              if (firstPacketPosition != 0)
              {
                processingBuffer->RemoveFromBuffer(firstPacketPosition);

                caller->logger->Log(LOGGER_WARNING, L"%s: %s: invalid data, removing %u bytes", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, firstPacketPosition);
              }

              CHECK_CONDITION_HRESULT(result, fragmentBuffer->AddToBufferWithResize(processingBuffer, 0, packetSequenceLength) == packetSequenceLength, result, E_OUTOFMEMORY);
              processingBuffer->RemoveFromBuffer(packetSequenceLength);
            }
          }

          if (SUCCEEDED(result) && (fragmentBuffer->GetBufferOccupiedSpace() != currentReadyForAlignStreamFragment->GetLength()))
          {
            currentReadyForAlignStreamFragment->GetBuffer()->ClearBuffer();
            CHECK_CONDITION_HRESULT(result, currentReadyForAlignStreamFragment->GetBuffer()->AddToBufferWithResize(fragmentBuffer) == fragmentBuffer->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);

            // move remaining data to next fragment or drop it when next fragment doesn't exist
            if ((nextStreamFragment != NULL) && (processingBuffer->GetBufferOccupiedSpace() != 0))
            {
              unsigned int length = nextStreamFragment->GetLength() + processingBuffer->GetBufferOccupiedSpace();

              ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
              CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                processingBuffer->CopyFromBuffer(buffer, processingBuffer->GetBufferOccupiedSpace());
                nextStreamFragment->GetBuffer()->CopyFromBuffer(buffer + processingBuffer->GetBufferOccupiedSpace(), length - processingBuffer->GetBufferOccupiedSpace());

                nextStreamFragment->GetBuffer()->ClearBuffer();
                CHECK_CONDITION_HRESULT(result, nextStreamFragment->GetBuffer()->AddToBufferWithResize(buffer, length) == length, result, E_OUTOFMEMORY);
              }

              FREE_MEM(buffer);
            }
          }

          FREE_MEM_CLASS(processingBuffer);
          FREE_MEM_CLASS(fragmentBuffer);
        }

        if (SUCCEEDED(result))
        {
          // mark stream fragment as aligned
          currentReadyForAlignStreamFragment->SetReadyForAlign(false, UINT_MAX);
          currentReadyForAlignStreamFragment->SetAligned(true, UINT_MAX);

          caller->streamFragments->UpdateIndexes(indexedReadyForAlignStreamFragment->GetItemIndex(), 1);
          caller->streamFragments->RecalculateAlignedStreamFragmentStartPosition(indexedReadyForAlignStreamFragment->GetItemIndex());
        }
      }

      FREE_MEM_CLASS(indexedReadyForAlignStreamFragments);

      // check if last fragment is processed
      // if yes, then set end of stream reached flag

      CMpeg2tsStreamFragment *lastFragment = caller->streamFragments->GetItem(caller->streamFragments->Count() - 1);

      if ((lastFragment == NULL) || ((lastFragment != NULL) && (lastFragment->IsProcessed())))
      {
        // end of stream reached
        caller->flags |= PARSER_PLUGIN_FLAG_END_OF_STREAM_REACHED;
      }
      
      UNLOCK_MUTEX(caller->mutex)
    }

    if (SUCCEEDED(result) && (caller->streamFragments->HasAlignedStreamFragments()))
    {
      sleepMode = SLEEP_MODE_SHORT;

      // don't wait too long, we can do this later
      LOCK_MUTEX(caller->mutex, 20)

      CIndexedMpeg2tsStreamFragmentCollection *indexedAlignedStreamFragments = new CIndexedMpeg2tsStreamFragmentCollection(&result);
      CHECK_POINTER_HRESULT(result, indexedAlignedStreamFragments, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = caller->streamFragments->GetAlignedStreamFragments(indexedAlignedStreamFragments));

      if (SUCCEEDED(result))
      {
        CTsPacket *nullPacket = CTsPacket::CreateNullPacket();
        CHECK_POINTER_HRESULT(result, nullPacket, result, E_OUTOFMEMORY);

        // create only reference TS packet (we don't copy data from original TS packet from buffer)
        CTsPacket *packet = new CTsPacket(&result, true);
        CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

        for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedAlignedStreamFragments->Count())); i++)
        {
          CIndexedMpeg2tsStreamFragment *indexedAlignedStreamFragment = indexedAlignedStreamFragments->GetItem(i);
          CMpeg2tsStreamFragment *currentAlignedStreamFragment = indexedAlignedStreamFragment->GetItem();

          if (caller->IsSetAnyOfFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_DETECT_DISCONTINUITY | MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
          {
            unsigned int processed = 0;
            unsigned int length = currentAlignedStreamFragment->GetBuffer()->GetBufferOccupiedSpace();
            unsigned char *buffer = currentAlignedStreamFragment->GetBuffer()->GetInternalBuffer();

            while (SUCCEEDED(result) && (processed < length))
            {
              CHECK_CONDITION_HRESULT(result, packet->Parse(buffer + processed, length - processed), result, E_MPEG2TS_CANNOT_PARSE_PACKET);

              if (SUCCEEDED(result) && caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_DETECT_DISCONTINUITY))
              {
                result = caller->discontinuityParser->Parse(packet);

                if (SUCCEEDED(result))
                {
                  CHECK_CONDITION_EXECUTE(caller->discontinuityParser->IsDiscontinuity(), caller->logger->Log(LOGGER_WARNING, L"%s: %s: discontinuity detected, PID: %u (0x%04X), expected counter: %u, packet counter: %u", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, caller->discontinuityParser->GetLastDiscontinuityPid(), caller->discontinuityParser->GetLastDiscontinuityPid(), (unsigned int)caller->discontinuityParser->GetLastExpectedCounter(), (unsigned int)caller->discontinuityParser->GetLastDiscontinuityCounter()));
                }
                else
                {
                  caller->logger->Log(LOGGER_WARNING, L"%s: %s: discontinuity parser returned error: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, result);
                }
              }

              if (SUCCEEDED(result) && caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
              {
                // directly change transport scrambling control flag in buffer
                packet->SetTransportScramblingControl(TS_PACKET_TRANSPORT_SCRAMBLING_CONTROL_NOT_SCRAMBLED);

                if (packet->GetPID() == CONDITIONAL_ACCESS_PARSER_PSI_PACKET_PID)
                {
                  // replace conditional access PSI packet with NULL packet
                  memcpy(buffer + processed, nullPacket->GetPacket(), TS_PACKET_SIZE);
                }
              }

              processed += TS_PACKET_SIZE;
            }
          }

          currentAlignedStreamFragment->SetAligned(false, UINT_MAX);
          currentAlignedStreamFragment->SetDiscontinuityProcessed(true, UINT_MAX);

          caller->streamFragments->UpdateIndexes(indexedAlignedStreamFragment->GetItemIndex(), 1);
        }

        FREE_MEM_CLASS(packet);
        FREE_MEM_CLASS(nullPacket);
      }

      FREE_MEM_CLASS(indexedAlignedStreamFragments);

      UNLOCK_MUTEX(caller->mutex)
    }

    if (SUCCEEDED(result) && caller->streamFragments->HasDiscontinuityProcessedStreamFragments())
    {
      sleepMode = SLEEP_MODE_SHORT;

      // don't wait too long, we can do this later
      LOCK_MUTEX(caller->mutex, 20)

      CIndexedMpeg2tsStreamFragmentCollection *indexedDiscontinuityProcessedStreamFragments = new CIndexedMpeg2tsStreamFragmentCollection(&result);
      CHECK_POINTER_HRESULT(result, indexedDiscontinuityProcessedStreamFragments, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = caller->streamFragments->GetDiscontinuityProcessedStreamFragments(indexedDiscontinuityProcessedStreamFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedDiscontinuityProcessedStreamFragments->Count())); i++)
      {
        CIndexedMpeg2tsStreamFragment *indexedDiscontinuityProcessedStreamFragment = indexedDiscontinuityProcessedStreamFragments->GetItem(i);
        CMpeg2tsStreamFragment *currentDiscontinuityProcessedStreamFragment = indexedDiscontinuityProcessedStreamFragment->GetItem();

        if (caller->IsSetAnyOfFlags(
          MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
          MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
          MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID |
          MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
        {
          // changing stream identification is required or stream should be marked as not scrambled

          unsigned int processed = 0;
          unsigned int length = currentDiscontinuityProcessedStreamFragment->GetBuffer()->GetBufferOccupiedSpace();
          const unsigned char *buffer = currentDiscontinuityProcessedStreamFragment->GetBuffer()->GetInternalBuffer();

          // create only reference TS packet (we don't copy data from original TS packet from buffer)
          CTsPacket *packet = new CTsPacket(&result, true);
          CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

          while (SUCCEEDED(result) && (processed < length))
          {
            CHECK_CONDITION_HRESULT(result, packet->Parse(buffer + processed, length - processed), result, E_MPEG2TS_CANNOT_PARSE_PACKET);

            // process stream fragment for program association section
            if (SUCCEEDED(result) && (packet->GetPID() == PROGRAM_ASSOCIATION_PARSER_PSI_PACKET_PID) && caller->IsSetAnyOfFlags(
              MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
              MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
              MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID |
              MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
            {
              // create only reference program specific information TS packet (we don't copy data from original TS packet from buffer)
              CProgramSpecificInformationPacket *psiPacket = new CProgramSpecificInformationPacket(&result, PROGRAM_ASSOCIATION_PARSER_PSI_PACKET_PID, true);
              CHECK_POINTER_HRESULT(result, psiPacket, result, E_OUTOFMEMORY);

              if (psiPacket->Parse(buffer + processed, length - processed))
              {
                // PSI packet with specified PID
                // program association section PSI packet

                HRESULT res = caller->programAssociationParserContext->GetParser()->Parse(psiPacket);

                if (caller->programAssociationParserContext->GetParser()->IsSectionFound())
                {
                  // found MPEG2 TS packet with program association section (maybe complete, maybe incomplete, maybe with error)

                  if ((caller->programAssociationParserContext->GetSectionContext() == NULL) &&
                    caller->IsSetAnyOfFlags(
                    MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
                    MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
                    MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                  {
                    // create new program association section context
                    result = caller->programAssociationParserContext->CreateSectionContext();
                    CHECK_POINTER_HRESULT(result, caller->programAssociationParserContext->GetSectionContext(), result, E_OUTOFMEMORY);

                    if (SUCCEEDED(result))
                    {
                      caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionEmpty(true);
                      caller->programAssociationParserContext->GetSectionContext()->SetContinuityCounter(psiPacket->GetContinuityCounter());
                    }
                  }

                  bool isOwner = false;
                  switch (res)
                  {
                  case S_OK:
                    // complete program association section
                    {
                      isOwner = true;

                      // check number of programs, we allow only one program (in another case we don't know, which program number and/or program map PID to replace)

                      if (caller->programAssociationParserContext->GetLastSectionCrc32() != caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->GetCrc32())
                      {
                        caller->logger->LogBinary(LOGGER_VERBOSE,
                          caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->GetSection(),
                          caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->GetSectionSize(),
                          L"%s: %s: new program association section detected", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME);

                        caller->programAssociationParserContext->SetLastSectionCrc32(caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->GetCrc32());
                      }

                      if (caller->IsSetAnyOfFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER | MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID | MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
                      {
                        if (caller->IsSetAnyOfFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER | MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                        {
                          CHECK_CONDITION_HRESULT(result, caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->GetPrograms()->Count() == 1, result, E_MPEG2TS_ONLY_ONE_PROGRAM_ALLOWED);
                        }

                        for (unsigned int j = 0; (SUCCEEDED(result) && (j < caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->GetPrograms()->Count())); j++)
                        {
                          CProgramAssociationSectionProgram *program = caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->GetPrograms()->GetItem(j);

                          CTransportStreamProgramMapParserContext *context = caller->transportStreamProgramMapParserContextCollection->GetParserContextByPID(program->GetProgramMapPID());

                          if (context == NULL)
                          {
                            // transport stream program map parser context with specified PID doesn't exist, create new one
                            caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: transport stream program map parser (PID: 0x%04X) does not exist, creating new parser", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, program->GetProgramMapPID());

                            context = new CTransportStreamProgramMapParserContext(&result, (uint16_t)program->GetProgramMapPID());
                            CHECK_POINTER_HRESULT(result, context, result, E_OUTOFMEMORY);

                            CHECK_CONDITION_HRESULT(result, caller->transportStreamProgramMapParserContextCollection->Add(context), result, E_OUTOFMEMORY);
                            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(context));
                          }
                        }
                      }

                      if (SUCCEEDED(result) &&
                        caller->IsSetAnyOfFlags(
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                      {
                        CProgramAssociationSection *section = (CProgramAssociationSection *)caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->Clone();
                        CHECK_POINTER_HRESULT(result, section, result, E_OUTOFMEMORY);

                        CHECK_CONDITION_HRESULT(result, caller->programAssociationParserContext->GetSectionContext()->SetOriginalSection(section), result, E_OUTOFMEMORY);

                        caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->Clear();

                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionEmpty(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIncomplete(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionComplete(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionError(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIsSection(true);

                        caller->programAssociationParserContext->GetSectionContext()->SetPacketCount(caller->programAssociationParserContext->GetSectionContext()->GetPacketCount() + 1);
                      }
                    }
                    break;
                  case S_FALSE:
                    // incomplete program association section
                    {
                      if (caller->IsSetAnyOfFlags(
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                      {
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionEmpty(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIncomplete(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionComplete(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionError(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIsSection(true);

                        caller->programAssociationParserContext->GetSectionContext()->SetPacketCount(caller->programAssociationParserContext->GetSectionContext()->GetPacketCount() + 1);
                      }
                    }
                    break;
                  case E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION:
                    // section is empty and PSI packet with section data
                    {
                      isOwner = true;

                      caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->Clear();

                      if (caller->IsSetAnyOfFlags(
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                      {
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionEmpty(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIncomplete(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionComplete(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionError(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIsSection(true);

                        caller->programAssociationParserContext->GetSectionContext()->SetPacketCount(caller->programAssociationParserContext->GetSectionContext()->GetPacketCount() + 1);
                      }
                    }
                    break;
                  case E_MPEG2TS_INCOMPLETE_SECTION:
                    // section is incomplete
                    {
                      isOwner = true;

                      caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->Clear();

                      if (caller->IsSetAnyOfFlags(
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                      {
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionEmpty(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIncomplete(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionComplete(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionError(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIsSection(true);

                        caller->programAssociationParserContext->GetSectionContext()->SetPacketCount(caller->programAssociationParserContext->GetSectionContext()->GetPacketCount() + 1);
                      }
                    }
                    break;
                  case E_MPEG2TS_SECTION_INVALID_CRC32:
                    // invalid section CRC32 (corrupted section)
                    {
                      isOwner = true;

                      caller->programAssociationParserContext->GetParser()->GetProgramAssociationSection()->Clear();

                      if (caller->IsSetAnyOfFlags(
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
                        MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                      {
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionEmpty(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIncomplete(false);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionComplete(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionError(true);
                        caller->programAssociationParserContext->GetSectionContext()->SetOriginalSectionIsSection(true);

                        caller->programAssociationParserContext->GetSectionContext()->SetPacketCount(caller->programAssociationParserContext->GetSectionContext()->GetPacketCount() + 1);
                      }
                    }
                    break;
                  default:
                    // another error
                    {
                      result = res;
                      caller->logger->Log(LOGGER_ERROR, L"%s: %s: program association parser returned parse error: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, result);
                    }
                    break;
                  }

                  if (SUCCEEDED(result) && caller->IsSetAnyOfFlags(
                    MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID |
                    MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
                    MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
                  {
                    CProgramAssociationSectionPacketContext *packetContext = new CProgramAssociationSectionPacketContext(&result);
                    CHECK_POINTER_HRESULT(result, packetContext, result, E_OUTOFMEMORY);

                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packetContext->SetTsPacketIndex(processed / TS_PACKET_SIZE));
                    CHECK_CONDITION_HRESULT(result, packetContext->SetSectionContext(caller->programAssociationParserContext->GetSectionContext()), result, E_FAIL);

                    CHECK_CONDITION_HRESULT(result, currentDiscontinuityProcessedStreamFragment->GetProgramAssociationSectionPacketContexts()->Add(packetContext), result, E_OUTOFMEMORY);
                    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packetContext));

                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packetContext->SetSectionContextOwner(isOwner));
                  }

                  // program association section context will be released from memory with removing packet context which is owner of program association section context 
                  CHECK_CONDITION_EXECUTE(SUCCEEDED(result) && isOwner, caller->programAssociationParserContext->FreeSectionContext());
                }
                else
                {
                  result = res;
                  caller->logger->Log(LOGGER_ERROR, L"%s: %s: program association parser returned error: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, result);
                }
              }

              FREE_MEM_CLASS(psiPacket);
            }

            // process stream fragment for transport stream program map section
            if (SUCCEEDED(result) && caller->IsSetAnyOfFlags(
              MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER |
              MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID |
              MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
            {
              CTransportStreamProgramMapParserContext *context = caller->transportStreamProgramMapParserContextCollection->GetParserContextByPID(packet->GetPID());

              if (context != NULL)
              {
                bool removeParserContext = false;

                CProgramSpecificInformationPacket *psiPacket = new CProgramSpecificInformationPacket(&result, context->GetParser()->GetTransportStreamProgramMapSectionPID(), true);
                CHECK_POINTER_HRESULT(result, psiPacket, result, E_OUTOFMEMORY);

                if (SUCCEEDED(result) && (psiPacket->Parse(buffer + processed, length - processed)))
                {
                  // PSI packet with specified PID
                  // transport stream program map PSI packet

                  HRESULT res = context->GetParser()->Parse(psiPacket);

                  if (context->GetParser()->IsSectionFound() || (res == E_MPEG2TS_SECTION_INVALID_TABLE_ID))
                  {
                    // found MPEG2 TS packet with transport stream program map section (maybe complete, maybe incomplete, maybe with error)

                    if (context->GetSectionContext() == NULL)
                    {
                      // create new transport stream program map section context
                      result = context->CreateSectionContext();
                      CHECK_POINTER_HRESULT(result, context->GetSectionContext(), result, E_OUTOFMEMORY);

                      if (SUCCEEDED(result))
                      {
                        context->GetSectionContext()->SetOriginalSectionEmpty(true);
                        context->GetSectionContext()->SetContinuityCounter(psiPacket->GetContinuityCounter());
                        context->GetSectionContext()->SetPID(caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID) ? caller->programMapPID : psiPacket->GetPID());
                      }
                    }

                    bool isOwner = false;
                    switch (res)
                    {
                    case S_OK:
                      // complete transport stream program map section
                      {
                        isOwner = true;

                        if (context->GetLastSectionCrc32() != context->GetParser()->GetTransportStreamProgramMapSection()->GetCrc32())
                        {
                          caller->logger->LogBinary(LOGGER_VERBOSE,
                            context->GetParser()->GetTransportStreamProgramMapSection()->GetSection(),
                            context->GetParser()->GetTransportStreamProgramMapSection()->GetSectionSize(),
                            L"%s: %s: new transport stream program map section detected for PID: 0x%04X", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, context->GetParser()->GetTransportStreamProgramMapSectionPID());

                          context->SetLastSectionCrc32(context->GetParser()->GetTransportStreamProgramMapSection()->GetCrc32());
                        }

                        if (SUCCEEDED(result))
                        {
                          CTransportStreamProgramMapSection *section = (CTransportStreamProgramMapSection *)context->GetParser()->GetTransportStreamProgramMapSection()->Clone();
                          CHECK_POINTER_HRESULT(result, section, result, E_OUTOFMEMORY);

                          CHECK_CONDITION_HRESULT(result, context->GetSectionContext()->SetOriginalSection(section), result, E_OUTOFMEMORY);

                          context->GetParser()->GetTransportStreamProgramMapSection()->Clear();

                          context->GetSectionContext()->SetOriginalSectionEmpty(false);
                          context->GetSectionContext()->SetOriginalSectionIncomplete(false);
                          context->GetSectionContext()->SetOriginalSectionComplete(true);
                          context->GetSectionContext()->SetOriginalSectionError(false);
                          context->GetSectionContext()->SetOriginalSectionIsSection(true);

                          context->GetSectionContext()->SetPacketCount(context->GetSectionContext()->GetPacketCount() + 1);
                        }
                      }
                      break;
                    case S_FALSE:
                      // incomplete transport stream program map section
                      {
                        context->GetSectionContext()->SetOriginalSectionEmpty(false);
                        context->GetSectionContext()->SetOriginalSectionIncomplete(true);
                        context->GetSectionContext()->SetOriginalSectionComplete(false);
                        context->GetSectionContext()->SetOriginalSectionError(false);
                        context->GetSectionContext()->SetOriginalSectionIsSection(true);

                        context->GetSectionContext()->SetPacketCount(context->GetSectionContext()->GetPacketCount() + 1);
                      }
                      break;
                    case E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION:
                      // section is empty and PSI packet without section data
                      {
                        isOwner = true;

                        context->GetParser()->GetTransportStreamProgramMapSection()->Clear();

                        context->GetSectionContext()->SetOriginalSectionEmpty(true);
                        context->GetSectionContext()->SetOriginalSectionIncomplete(false);
                        context->GetSectionContext()->SetOriginalSectionComplete(false);
                        context->GetSectionContext()->SetOriginalSectionError(true);
                        context->GetSectionContext()->SetOriginalSectionIsSection(true);

                        context->GetSectionContext()->SetPacketCount(context->GetSectionContext()->GetPacketCount() + 1);
                      }
                      break;
                    case E_MPEG2TS_INCOMPLETE_SECTION:
                      // section is incomplete
                      {
                        isOwner = true;

                        context->GetParser()->GetTransportStreamProgramMapSection()->Clear();

                        context->GetSectionContext()->SetOriginalSectionEmpty(false);
                        context->GetSectionContext()->SetOriginalSectionIncomplete(true);
                        context->GetSectionContext()->SetOriginalSectionComplete(false);
                        context->GetSectionContext()->SetOriginalSectionError(true);
                        context->GetSectionContext()->SetOriginalSectionIsSection(true);

                        context->GetSectionContext()->SetPacketCount(context->GetSectionContext()->GetPacketCount() + 1);
                      }
                      break;
                    case E_MPEG2TS_SECTION_INVALID_CRC32:
                      // invalid section CRC32 (corrupted section)
                      {
                        isOwner = true;

                        context->GetParser()->GetTransportStreamProgramMapSection()->Clear();

                        context->GetSectionContext()->SetOriginalSectionEmpty(false);
                        context->GetSectionContext()->SetOriginalSectionIncomplete(false);
                        context->GetSectionContext()->SetOriginalSectionComplete(true);
                        context->GetSectionContext()->SetOriginalSectionError(true);
                        context->GetSectionContext()->SetOriginalSectionIsSection(true);

                        context->GetSectionContext()->SetPacketCount(context->GetSectionContext()->GetPacketCount() + 1);
                      }
                      break;
                    case E_MPEG2TS_SECTION_INVALID_TABLE_ID:
                      // invalid section table ID (no section)
                      {
                        isOwner = true;

                        context->GetParser()->GetTransportStreamProgramMapSection()->Clear();

                        context->GetSectionContext()->SetOriginalSectionEmpty(false);
                        context->GetSectionContext()->SetOriginalSectionIncomplete(false);
                        context->GetSectionContext()->SetOriginalSectionComplete(false);
                        context->GetSectionContext()->SetOriginalSectionError(true);
                        context->GetSectionContext()->SetOriginalSectionIsSection(false);

                        context->GetSectionContext()->SetPacketCount(context->GetSectionContext()->GetPacketCount() + 1);
                        caller->logger->Log(LOGGER_WARNING, L"%s: %s: transport stream program map parser (PID: 0x%04X), invalid table ID 0x%02X, removing parser", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, (uint32_t)context->GetParser()->GetTransportStreamProgramMapSectionPID(), context->GetParser()->GetTransportStreamProgramMapSection()->GetTableId());

                        removeParserContext = true;
                      }
                      break;
                    default:
                      // another error
                      {
                        result = res;
                        caller->logger->Log(LOGGER_ERROR, L"%s: %s: transport stream program map parser returned parse error: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, result);
                      }
                      break;
                    }

                    if (SUCCEEDED(result))
                    {
                      CTransportStreamProgramMapSectionPacketContext *packetContext = new CTransportStreamProgramMapSectionPacketContext(&result);
                      CHECK_POINTER_HRESULT(result, packetContext, result, E_OUTOFMEMORY);

                      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packetContext->SetTsPacketIndex(processed / TS_PACKET_SIZE));
                      CHECK_CONDITION_HRESULT(result, packetContext->SetSectionContext(context->GetSectionContext()), result, E_FAIL);

                      CHECK_CONDITION_HRESULT(result, currentDiscontinuityProcessedStreamFragment->GetTransportStreamProgramMapSectionPacketContexts()->Add(packetContext), result, E_OUTOFMEMORY);
                      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packetContext));

                      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packetContext->SetSectionContextOwner(isOwner));
                    }

                    // transport stream program map section context will be released from memory with removing packet context which is owner of transport stream program map section context 
                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result) && isOwner, context->FreeSectionContext());

                    if (SUCCEEDED(result) && removeParserContext)
                    {
                      unsigned int parserContextId = caller->transportStreamProgramMapParserContextCollection->GetParserContextIdByPID(packet->GetPID());

                      caller->transportStreamProgramMapParserContextCollection->Remove(parserContextId, 1);
                    }
                  }
                  else
                  {
                    result = res;
                    caller->logger->Log(LOGGER_ERROR, L"%s: %s: transport stream program map parser returned error: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, result);
                  }
                }

                FREE_MEM_CLASS(psiPacket);
              }
            }

            processed += TS_PACKET_SIZE;
          }

          FREE_MEM_CLASS(packet);

          currentDiscontinuityProcessedStreamFragment->SetDiscontinuityProcessed(false, UINT_MAX);

          currentDiscontinuityProcessedStreamFragment->SetProgramAssociationSectionDetectionFinished(currentDiscontinuityProcessedStreamFragment->GetProgramAssociationSectionPacketContexts()->Count() != 0, UINT_MAX);
          currentDiscontinuityProcessedStreamFragment->SetProgramAssociationSectionUpdated(currentDiscontinuityProcessedStreamFragment->GetProgramAssociationSectionPacketContexts()->Count() == 0, UINT_MAX);

          currentDiscontinuityProcessedStreamFragment->SetTransportStreamMapSectionDetectionFinished(currentDiscontinuityProcessedStreamFragment->GetTransportStreamProgramMapSectionPacketContexts()->Count() != 0, UINT_MAX);
          currentDiscontinuityProcessedStreamFragment->SetTransportStreamMapSectionUpdated(currentDiscontinuityProcessedStreamFragment->GetTransportStreamProgramMapSectionPacketContexts()->Count() == 0, UINT_MAX);

          caller->streamFragments->UpdateIndexes(indexedDiscontinuityProcessedStreamFragment->GetItemIndex(), 1);
        }
        else
        {
          currentDiscontinuityProcessedStreamFragment->SetDiscontinuityProcessed(false, UINT_MAX);
          currentDiscontinuityProcessedStreamFragment->SetProgramAssociationSectionUpdated(true, UINT_MAX);
          currentDiscontinuityProcessedStreamFragment->SetTransportStreamMapSectionUpdated(true, UINT_MAX);

          caller->streamFragments->UpdateIndexes(indexedDiscontinuityProcessedStreamFragment->GetItemIndex(), 1);
        }
      }

      FREE_MEM_CLASS(indexedDiscontinuityProcessedStreamFragments);

      UNLOCK_MUTEX(caller->mutex)
    }

    if (SUCCEEDED(result) && caller->streamFragments->HasProgramAssociationSectionDetectionFinishedStreamFragments())
    {
      sleepMode = SLEEP_MODE_SHORT;

      // don't wait too long, we can do this later
      LOCK_MUTEX(caller->mutex, 20)

      CIndexedMpeg2tsStreamFragmentCollection *indexedProgramAssociationSectionDetectionFinishedStreamFragments = new CIndexedMpeg2tsStreamFragmentCollection(&result);
      CHECK_POINTER_HRESULT(result, indexedProgramAssociationSectionDetectionFinishedStreamFragments, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = caller->streamFragments->GetProgramAssociationSectionDetectionFinishedStreamFragments(indexedProgramAssociationSectionDetectionFinishedStreamFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedProgramAssociationSectionDetectionFinishedStreamFragments->Count())); i++)
      {
        CIndexedMpeg2tsStreamFragment *indexedProgramAssociationSectionDetectionFinishedStreamFragment = indexedProgramAssociationSectionDetectionFinishedStreamFragments->GetItem(i);
        CMpeg2tsStreamFragment *currentProgramAssociationSectionDetectionFinishedStreamFragment = indexedProgramAssociationSectionDetectionFinishedStreamFragment->GetItem();

        // create buffer for replacing program association section MPEG2 TS packets
        unsigned int length = currentProgramAssociationSectionDetectionFinishedStreamFragment->GetBuffer()->GetBufferOccupiedSpace();
        unsigned char *buffer = currentProgramAssociationSectionDetectionFinishedStreamFragment->GetBuffer()->GetInternalBuffer();

        unsigned int j = 0;
        while (SUCCEEDED(result) && (j < currentProgramAssociationSectionDetectionFinishedStreamFragment->GetProgramAssociationSectionPacketContexts()->Count()))
        {
          bool contextProcessed = false;
          CProgramAssociationSectionPacketContext *context = currentProgramAssociationSectionDetectionFinishedStreamFragment->GetProgramAssociationSectionPacketContexts()->GetItem(j);

          if (SUCCEEDED(result) && (context->GetSectionContext()->IsOriginalSectionComplete() && (!context->GetSectionContext()->IsOriginalSectionError()) && (context->GetSectionContext()->GetUpdatedSection() == NULL)))
          {
            // create new updated section and update its data

            CHECK_CONDITION_HRESULT(result, context->GetSectionContext()->CreateUpdatedSection(), result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              context->GetSectionContext()->GetUpdatedSection()->ResetSize();

              // replace transport stream ID (if needed)
              if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID))
              {
                context->GetSectionContext()->GetUpdatedSection()->SetTransportStreamId(caller->transportStreamId);
              }

              // replace program number (if needed)
              if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER))
              {
                CProgramAssociationSectionProgram *program = context->GetSectionContext()->GetUpdatedSection()->GetPrograms()->GetItem(0);

                program->SetProgramNumber(caller->programNumber);
              }

              // replace program map PID (if needed)
              if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID))
              {
                CProgramAssociationSectionProgram *program = context->GetSectionContext()->GetUpdatedSection()->GetPrograms()->GetItem(0);

                program->SetProgramMapPID(caller->programMapPID);
              }
            }

            if (SUCCEEDED(result))
            {
              // split section into MPEG2 TS packets
              // the count of MPEG2 TS packets must be same or lower than for original section

              context->GetSectionContext()->GetPackets()->Clear();

              CTsPacketCollection *psiPackets = CProgramSpecificInformationPacket::SplitSectionInProgramSpecificInformationPackets(context->GetSectionContext()->GetUpdatedSection(), PROGRAM_ASSOCIATION_PARSER_PSI_PACKET_PID, context->GetSectionContext()->GetContinuityCounter());
              CHECK_POINTER_HRESULT(result, psiPackets, result, E_MPEG2TS_CANNOT_SPLIT_SECTION_INTO_PSI_PACKETS);
              CHECK_CONDITION_HRESULT(result, psiPackets->Count() <= context->GetSectionContext()->GetPacketCount(), result, E_MPEG2TS_SECTION_BIGGER_THAN_ORIGINAL_SECTION);

              if (SUCCEEDED(result) && (psiPackets->Count() < context->GetSectionContext()->GetPacketCount()))
              {
                // we need to add NULL MPEG2 TS packets to fill gaps
                for (unsigned int k = psiPackets->Count(); (SUCCEEDED(result) && (k < context->GetSectionContext()->GetPacketCount())); k++)
                {
                  CTsPacket *nullPacket = CTsPacket::CreateNullPacket();
                  CHECK_POINTER_HRESULT(result, nullPacket, result, E_OUTOFMEMORY);

                  CHECK_CONDITION_HRESULT(result, psiPackets->Add(nullPacket), result, E_OUTOFMEMORY);
                  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(nullPacket));
                }
              }

              // now we same same count of PSI and NULL MPEG2 TS packets to replace original section MPEG2 TS packets
              CHECK_CONDITION_HRESULT(result, context->GetSectionContext()->GetPackets()->Append(psiPackets), result, E_OUTOFMEMORY);
              FREE_MEM_CLASS(psiPackets);
            }
          }

          if (SUCCEEDED(result) && (context->GetSectionContext()->IsOriginalSectionComplete() && (!context->GetSectionContext()->IsOriginalSectionError()) && (context->GetSectionContext()->GetUpdatedSection() != NULL)))
          {
            // updated section is already created and splitted to MPEG2 TS packets

            CTsPacket *packet = context->GetSectionContext()->GetPackets()->GetItem(0);

            memcpy(buffer + context->GetTsPacketIndex() * TS_PACKET_SIZE, packet->GetPacket(), TS_PACKET_SIZE);

            // remove used MPEG2 TS packet, it is not needed more
            context->GetSectionContext()->GetPackets()->Remove(0);

            contextProcessed = true;
          }

          // in case of end of stream in protocol or connection problems we don't receive any more data, we must replace program association section with NULL MPEG2 TS packets
          if (SUCCEEDED(result) && (context->GetSectionContext()->IsOriginalSectionError() || caller->protocolHoster->IsEndOfStreamReached() || caller->IsConnectionLostCannotReopen()))
          {
            // original section has some error: is empty, is incomplete or bad CRC32
            // in all cases it will be replaced by NULL MPEG2 TS packet to avoid problems

            CTsPacket *packet = CTsPacket::CreateNullPacket();
            CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), memcpy(buffer + context->GetTsPacketIndex() * TS_PACKET_SIZE, packet->GetPacket(), TS_PACKET_SIZE));

            // remove used MPEG2 TS packet, it is not needed more
            FREE_MEM_CLASS(packet);
            contextProcessed = true;
          }

          if (contextProcessed)
          {
            currentProgramAssociationSectionDetectionFinishedStreamFragment->GetProgramAssociationSectionPacketContexts()->Remove(j);
          }
          else
          {
            j++;
          }
        }

        if (currentProgramAssociationSectionDetectionFinishedStreamFragment->GetProgramAssociationSectionPacketContexts()->Count() == 0)
        {
          // all program association sections in MPEG2 TS packet were updated
          currentProgramAssociationSectionDetectionFinishedStreamFragment->SetProgramAssociationSectionDetectionFinished(false, UINT_MAX);
          currentProgramAssociationSectionDetectionFinishedStreamFragment->SetProgramAssociationSectionUpdated(true, UINT_MAX);

          caller->streamFragments->UpdateIndexes(indexedProgramAssociationSectionDetectionFinishedStreamFragment->GetItemIndex(), 1);
        }
      }

      FREE_MEM_CLASS(indexedProgramAssociationSectionDetectionFinishedStreamFragments);
      
      UNLOCK_MUTEX(caller->mutex)
    }

    if (SUCCEEDED(result) && caller->streamFragments->HasTransportStreamMapSectionDetectionFinishedStreamFragments())
    {
      sleepMode = SLEEP_MODE_SHORT;

      // don't wait too long, we can do this later
      LOCK_MUTEX(caller->mutex, 20)

      CIndexedMpeg2tsStreamFragmentCollection *indexedTransportStreamMapSectionDetectionFinishedStreamFragments = new CIndexedMpeg2tsStreamFragmentCollection(&result);
      CHECK_POINTER_HRESULT(result, indexedTransportStreamMapSectionDetectionFinishedStreamFragments, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = caller->streamFragments->GetTransportStreamMapSectionDetectionFinishedStreamFragments(indexedTransportStreamMapSectionDetectionFinishedStreamFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedTransportStreamMapSectionDetectionFinishedStreamFragments->Count())); i++)
      {
        CIndexedMpeg2tsStreamFragment *indexedTransportStreamMapSectionDetectionFinishedStreamFragment = indexedTransportStreamMapSectionDetectionFinishedStreamFragments->GetItem(i);
        CMpeg2tsStreamFragment *currentTransportStreamMapSectionDetectionFinishedStreamFragment = indexedTransportStreamMapSectionDetectionFinishedStreamFragment->GetItem();

        // create buffer for replacing transport stream program map section MPEG2 TS packets
        unsigned int length = currentTransportStreamMapSectionDetectionFinishedStreamFragment->GetBuffer()->GetBufferOccupiedSpace();
        unsigned char *buffer = currentTransportStreamMapSectionDetectionFinishedStreamFragment->GetBuffer()->GetInternalBuffer();

        unsigned int j = 0;
        while (SUCCEEDED(result) && (j < currentTransportStreamMapSectionDetectionFinishedStreamFragment->GetTransportStreamProgramMapSectionPacketContexts()->Count()))
        {
          bool contextProcessed = false;
          CTransportStreamProgramMapSectionPacketContext *context = currentTransportStreamMapSectionDetectionFinishedStreamFragment->GetTransportStreamProgramMapSectionPacketContexts()->GetItem(j);

          if (SUCCEEDED(result) && context->GetSectionContext()->IsOriginalSectionIsSection())
          {
            // in section context is section

            if (SUCCEEDED(result) && (context->GetSectionContext()->IsOriginalSectionComplete() && (!context->GetSectionContext()->IsOriginalSectionError()) && (context->GetSectionContext()->GetUpdatedSection() == NULL)))
            {
              // create new updated section and update its data

              CHECK_CONDITION_HRESULT(result, context->GetSectionContext()->CreateUpdatedSection(), result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                context->GetSectionContext()->GetUpdatedSection()->ResetSize();

                // transport stream ID is replaced in program association section only
                // program map PID is replaced in program association section and also as PID of PSI packet

                // replace program number (if needed)
                if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER))
                {
                  context->GetSectionContext()->GetUpdatedSection()->SetProgramNumber(caller->programNumber);
                }

                // remove CA descriptor (if needed)
                if (caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED))
                {
                  unsigned int k = 0;
                  while (SUCCEEDED(result) && (k < context->GetSectionContext()->GetUpdatedSection()->GetDescriptors()->Count()))
                  {
                    CConditionalAccessDescriptor *caDescriptor = dynamic_cast<CConditionalAccessDescriptor *>(context->GetSectionContext()->GetUpdatedSection()->GetDescriptors()->GetItem(k));

                    if (caDescriptor != NULL)
                    {
                      context->GetSectionContext()->GetUpdatedSection()->GetDescriptors()->Remove(k);
                    }
                    else
                    {
                      k++;
                    }
                  }
                }
              }

              if (SUCCEEDED(result))
              {
                // split section into MPEG2 TS packets
                // the count of MPEG2 TS packets must be same or lower than for original section

                context->GetSectionContext()->GetPackets()->Clear();

                CTsPacketCollection *psiPackets = CProgramSpecificInformationPacket::SplitSectionInProgramSpecificInformationPackets(context->GetSectionContext()->GetUpdatedSection(), context->GetSectionContext()->GetPID(), context->GetSectionContext()->GetContinuityCounter());
                CHECK_POINTER_HRESULT(result, psiPackets, result, E_MPEG2TS_CANNOT_SPLIT_SECTION_INTO_PSI_PACKETS);
                CHECK_CONDITION_HRESULT(result, psiPackets->Count() <= context->GetSectionContext()->GetPacketCount(), result, E_MPEG2TS_SECTION_BIGGER_THAN_ORIGINAL_SECTION);

                if (SUCCEEDED(result) && (psiPackets->Count() < context->GetSectionContext()->GetPacketCount()))
                {
                  // we need to add NULL MPEG2 TS packets to fill gaps
                  for (unsigned int k = psiPackets->Count(); (SUCCEEDED(result) && (k < context->GetSectionContext()->GetPacketCount())); k++)
                  {
                    CTsPacket *nullPacket = CTsPacket::CreateNullPacket();
                    CHECK_POINTER_HRESULT(result, nullPacket, result, E_OUTOFMEMORY);

                    CHECK_CONDITION_HRESULT(result, psiPackets->Add(nullPacket), result, E_OUTOFMEMORY);
                    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(nullPacket));
                  }
                }

                // now we same same count of PSI and NULL MPEG2 TS packets to replace original section MPEG2 TS packets
                CHECK_CONDITION_HRESULT(result, context->GetSectionContext()->GetPackets()->Append(psiPackets), result, E_OUTOFMEMORY);
                FREE_MEM_CLASS(psiPackets);
              }
            }

            if (SUCCEEDED(result) && (context->GetSectionContext()->IsOriginalSectionComplete() && (!context->GetSectionContext()->IsOriginalSectionError()) && (context->GetSectionContext()->GetUpdatedSection() != NULL)))
            {
              // updated section is already created and splitted to MPEG2 TS packets

              CTsPacket *packet = context->GetSectionContext()->GetPackets()->GetItem(0);

              memcpy(buffer + context->GetTsPacketIndex() * TS_PACKET_SIZE, packet->GetPacket(), TS_PACKET_SIZE);

              // remove used MPEG2 TS packet, it is not needed more
              context->GetSectionContext()->GetPackets()->Remove(0);

              contextProcessed = true;
            }

            // in case of end of stream in protocol or connection problems we don't receive any more data, we must replace transport stream program map section with NULL MPEG2 TS packets
            if (SUCCEEDED(result) && (context->GetSectionContext()->IsOriginalSectionError() || caller->protocolHoster->IsEndOfStreamReached() || caller->IsConnectionLostCannotReopen()))
            {
              // original section has some error: is empty, is incomplete or bad CRC32
              // in all cases it will be replaced by NULL MPEG2 TS packet to avoid problems

              CTsPacket *packet = CTsPacket::CreateNullPacket();
              CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), memcpy(buffer + context->GetTsPacketIndex() * TS_PACKET_SIZE, packet->GetPacket(), TS_PACKET_SIZE));

              // remove used MPEG2 TS packet, it is not needed more
              FREE_MEM_CLASS(packet);
              contextProcessed = true;
            }
          }
          else if (SUCCEEDED(result))
          {
            // no section in section context
            // don't change any data in MPEG2 TS packets

            contextProcessed = true;
          }

          if (contextProcessed)
          {
            currentTransportStreamMapSectionDetectionFinishedStreamFragment->GetTransportStreamProgramMapSectionPacketContexts()->Remove(j);
          }
          else
          {
            j++;
          }
        }

        if (currentTransportStreamMapSectionDetectionFinishedStreamFragment->GetTransportStreamProgramMapSectionPacketContexts()->Count() == 0)
        {
          // all transport stream program map sections in MPEG2 TS packet were updated
          currentTransportStreamMapSectionDetectionFinishedStreamFragment->SetTransportStreamMapSectionDetectionFinished(false, UINT_MAX);
          currentTransportStreamMapSectionDetectionFinishedStreamFragment->SetTransportStreamMapSectionUpdated(true, UINT_MAX);

          caller->streamFragments->UpdateIndexes(indexedTransportStreamMapSectionDetectionFinishedStreamFragment->GetItemIndex(), 1);
        }
      }

      FREE_MEM_CLASS(indexedTransportStreamMapSectionDetectionFinishedStreamFragments);
      
      UNLOCK_MUTEX(caller->mutex)
    }

    if (SUCCEEDED(result) && caller->streamFragments->HasBothSectionsUpdatedStreamFragments())
    {
      sleepMode = SLEEP_MODE_SHORT;

      // don't wait too long, we can do this later
      LOCK_MUTEX(caller->mutex, 20)

      CIndexedMpeg2tsStreamFragmentCollection *indexedBothSectionsUpdatedStreamFragments = new CIndexedMpeg2tsStreamFragmentCollection(&result);
      CHECK_POINTER_HRESULT(result, indexedBothSectionsUpdatedStreamFragments, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = caller->streamFragments->GetBothSectionsUpdatedStreamFragments(indexedBothSectionsUpdatedStreamFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedBothSectionsUpdatedStreamFragments->Count())); i++)
      {
        CIndexedMpeg2tsStreamFragment *indexedBothSectionsUpdatedStreamFragment = indexedBothSectionsUpdatedStreamFragments->GetItem(i);
        CMpeg2tsStreamFragment *currentBothSectionsUpdatedStreamFragment = indexedBothSectionsUpdatedStreamFragment->GetItem();

        currentBothSectionsUpdatedStreamFragment->SetProgramAssociationSectionUpdated(false, UINT_MAX);
        currentBothSectionsUpdatedStreamFragment->SetTransportStreamMapSectionUpdated(false, UINT_MAX);
        currentBothSectionsUpdatedStreamFragment->SetProcessed(true, UINT_MAX);
        currentBothSectionsUpdatedStreamFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);

        caller->streamFragments->UpdateIndexes(indexedBothSectionsUpdatedStreamFragment->GetItemIndex(), 1);
      }

      FREE_MEM_CLASS(indexedBothSectionsUpdatedStreamFragments);

      // check if last fragment is processed
      // if yes, then set end of stream reached flag

      CMpeg2tsStreamFragment *lastFragment = caller->streamFragments->GetItem(caller->streamFragments->Count() - 1);

      if ((lastFragment == NULL) || ((lastFragment != NULL) && (lastFragment->IsProcessed())))
      {
        // end of stream reached
        caller->flags |= PARSER_PLUGIN_FLAG_END_OF_STREAM_REACHED;
      }
      
      UNLOCK_MUTEX(caller->mutex)
    }

    if ((!caller->IsSetStreamLength()) && (!(caller->IsWholeStreamDownloaded() || caller->IsEndOfStreamReached() || caller->IsConnectionLostCannotReopen())))
    {
      // adjust total length (if necessary)
      CStreamProgress *streamProgress = new CStreamProgress();
      CHECK_POINTER_HRESULT(result, streamProgress, result, E_OUTOFMEMORY);

      HRESULT res = caller->protocolHoster->QueryStreamProgress(streamProgress);

      if ((res == VFW_S_ESTIMATED) || FAILED(res))
      {
        if (caller->streamLength == 0)
        {
          // stream length not set
          // just make guess

          caller->streamLength = MINIMUM_RECEIVED_DATA_FOR_SPLITTER;
          caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting guess total length: %lld", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, caller->streamLength);
        }
        else if ((caller->GetBytePosition() > (caller->streamLength * 3 / 4)))
        {
          // it is time to adjust stream length, we are approaching to end but still we don't know total length
          caller->streamLength = caller->GetBytePosition() * 2;
          caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting guess total length: %lld", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, caller->streamLength);
        }
      }
      else if (res == S_OK)
      {
        // total length of stream is known, our stream should not be bigger

        if (streamProgress->GetTotalLength() > caller->streamLength)
        {
          caller->streamLength = streamProgress->GetTotalLength();
          caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting guess total length: %lld", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, caller->streamLength);
        }
      }

      FREE_MEM_CLASS(streamProgress);
    }

    if ((!caller->IsSetStreamLength()) && (caller->IsWholeStreamDownloaded() || caller->IsEndOfStreamReached() || caller->IsConnectionLostCannotReopen()))
    {
      // reached end of stream, set stream length

      caller->streamLength = caller->GetBytePosition();
      caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %llu", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, caller->streamLength);

      caller->flags |= PARSER_PLUGIN_FLAG_SET_STREAM_LENGTH;
      caller->flags &= ~PARSER_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
    }

    if (SUCCEEDED(result) && (caller->streamFragmentDownloading == UINT_MAX) && (caller->streamFragmentToDownload != UINT_MAX))
    {
      caller->streamFragmentDownloading = caller->streamFragmentToDownload;
      caller->streamFragmentToDownload = UINT_MAX;

      CMpeg2tsStreamFragment *currentDownloadingFragment = caller->streamFragments->GetItem(caller->streamFragmentDownloading);

      currentDownloadingFragment->GetBuffer()->ClearBuffer();
    }

    if (SUCCEEDED(result) && caller->IsSetFlags(MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_RECEIVE_DATA) && (caller->pauseSeekStopMode != PAUSE_SEEK_STOP_MODE_DISABLE_READING) && (caller->streamFragmentDownloading != UINT_MAX))
    {
      // don't wait too long, we can receive stream fragment later
      LOCK_MUTEX(caller->mutex, 20)

      CMpeg2tsStreamFragment *currentDownloadingFragment = caller->streamFragments->GetItem(caller->streamFragmentDownloading);

      CStreamPackage *package = new CStreamPackage(&result);
      CHECK_POINTER_HRESULT(result, package, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        CStreamPackageDataRequest *request = new CStreamPackageDataRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        requestId++;

        if (SUCCEEDED(result))
        {
          unsigned int length = MINIMUM_RECEIVED_DATA_FOR_SPLITTER;

          if (caller->streamPackage != NULL)
          {
            CStreamPackageDataRequest *streamPackageRequest = dynamic_cast<CStreamPackageDataRequest *>(caller->streamPackage->GetRequest());

            length = streamPackageRequest->GetLength();
          }

          request->SetAnyDataLength(true);
          request->SetId(requestId);
          request->SetStreamId(0);
          request->SetStart(currentDownloadingFragment->GetRequestStartPosition());
          request->SetLength(length);

          package->SetRequest(request);
        }

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
      }

      CHECK_HRESULT_EXECUTE(result, caller->protocolHoster->ProcessStreamPackage(package));
      CHECK_HRESULT_EXECUTE(result, package->GetError());

      if (caller->IsSetFlags(PARSER_PLUGIN_FLAG_DUMP_INPUT_DATA))
      {
        CMpeg2TsDumpBox *dumpBox = new CMpeg2TsDumpBox(&result);
        CHECK_CONDITION_HRESULT(result, dumpBox, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          dumpBox->SetInputData(true);
          dumpBox->SetTimeWithLocalTime();
          dumpBox->SetStreamPackage(package);
        }

        CHECK_CONDITION_HRESULT(result, caller->dumpFile->AddDumpBox(dumpBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dumpBox));
      }

      if (result == E_PAUSE_SEEK_STOP_MODE_DISABLE_READING)
      {
        result = S_OK;
      }
      else if (SUCCEEDED(result))
      {
        // successfully processed stream package request
        CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(package->GetRequest());
        CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(package->GetResponse());

        if (response->IsConnectionLostCannotReopen() || response->IsNoMoreDataAvailable())
        {
          // connection lost, cannot reopen
          caller->flags |= response->IsConnectionLostCannotReopen() ? PARSER_PLUGIN_FLAG_CONNECTION_LOST_CANNOT_REOPEN : PARSER_PLUGIN_FLAG_NONE;
            
          // mark current downloading stream fragment as downloaded or remove it, if it has not any data
          if (currentDownloadingFragment != NULL)
          {
            if (response->GetBuffer() != NULL)
            {
              CHECK_CONDITION_HRESULT(result, currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(response->GetBuffer()) == response->GetBuffer()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);
            }

            if (SUCCEEDED(result))
            {
              if (currentDownloadingFragment->GetLength() == 0)
              {
                caller->streamFragments->Remove(caller->streamFragmentDownloading, 1);

                // set count of fragments to search for specific position
                unsigned int firstNotDownloadedFragmentIndex = caller->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(caller->streamFragments->GetStartSearchingIndex());
                caller->streamFragments->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? caller->streamFragments->Count() : firstNotDownloadedFragmentIndex) - caller->streamFragments->GetStartSearchingIndex());
              }
              else
              {
                currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
                currentDownloadingFragment->SetReadyForAlign(true, UINT_MAX);

                caller->streamFragments->UpdateIndexes(caller->streamFragmentDownloading, 1);
              }
            }

            caller->streamFragmentDownloading = UINT_MAX;
          }

          // check if last fragment is processed
          // if yes, then set end of stream reached flag

          CMpeg2tsStreamFragment *lastFragment = caller->streamFragments->GetItem(caller->streamFragments->Count() - 1);

          if ((lastFragment == NULL) || ((lastFragment != NULL) && (lastFragment->IsProcessed())))
          {
            // end of stream reached
            caller->flags |= PARSER_PLUGIN_FLAG_END_OF_STREAM_REACHED;
          }
        }
        else if (response->GetBuffer()->GetBufferOccupiedSpace() != 0)
        {
          // we received some data

          CHECK_CONDITION_HRESULT(result, currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(response->GetBuffer()) == response->GetBuffer()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            currentDownloadingFragment->SetFlags(currentDownloadingFragment->GetFlags() | (response->IsDiscontinuity() ? STREAM_FRAGMENT_FLAG_DISCONTINUITY : STREAM_FRAGMENT_FLAG_NONE));

            currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
            currentDownloadingFragment->SetReadyForAlign(true, UINT_MAX);

            caller->streamFragments->UpdateIndexes(caller->streamFragmentDownloading, 1);

            // create new stream fragment and set it to download
            CMpeg2tsStreamFragment *fragment = new CMpeg2tsStreamFragment(&result);
            CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              // fragment start position will be set after processing
              fragment->SetRequestStartPosition(currentDownloadingFragment->GetRequestStartPosition() + response->GetBuffer()->GetBufferOccupiedSpace());
            }

            CHECK_CONDITION_HRESULT(result, caller->streamFragments->Insert(caller->streamFragmentDownloading + 1, fragment), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));

            // ??? maybe check caller->streamFragmentToDownload ???
            caller->streamFragmentDownloading++;
          }

        }
      }

      FREE_MEM_CLASS(package);
      
      UNLOCK_MUTEX(caller->mutex)
    }

    if (FAILED(result) || (caller->pauseSeekStopMode == PAUSE_SEEK_STOP_MODE_DISABLE_READING))
    {
      // lock mutex to get exclussive access to stream package
      // don't wait too long
      LOCK_MUTEX(caller->mutex, 20)

      // we must check again caller->pauseSeekStopMode value, because it can changed between test and lock !

      if ((caller->streamPackage != NULL) && (FAILED(result) || (caller->pauseSeekStopMode == PAUSE_SEEK_STOP_MODE_DISABLE_READING)))
      {
        // we have error, for each stream package (if any) return error

        caller->streamPackage->SetCompleted((caller->pauseSeekStopMode == PAUSE_SEEK_STOP_MODE_DISABLE_READING) ? E_PAUSE_SEEK_STOP_MODE_DISABLE_READING : result);
      }

      UNLOCK_MUTEX(caller->mutex)
    }

    if (SUCCEEDED(result) && (caller->pauseSeekStopMode != PAUSE_SEEK_STOP_MODE_DISABLE_READING))
    {
      // lock mutex to get exclussive access to stream package
      // don't wait too long
      LOCK_MUTEX(caller->mutex, 20)

      if (caller->streamPackage != NULL)
      {
        // process stream package (if valid)
        if (caller->streamPackage->GetState() == CStreamPackage::Created)
        {
          HRESULT res = S_OK;
          // stream package is just created, it wasn't processed before
          CStreamPackageDataRequest *dataRequest = dynamic_cast<CStreamPackageDataRequest *>(caller->streamPackage->GetRequest());
          CHECK_CONDITION_HRESULT(res, dataRequest != NULL, res, E_INVALID_STREAM_PACKAGE_REQUEST);

          if (SUCCEEDED(res))
          {
            // set start time of processing request
            // set Waiting state
            // set response

            CStreamPackageDataResponse *response = new CStreamPackageDataResponse(&res);
            CHECK_POINTER_HRESULT(res, response, res, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(res, dataRequest->GetStreamId() == 0, res, E_INVALID_STREAM_ID);

            // allocate memory for response
            CHECK_CONDITION_HRESULT(res, response->GetBuffer()->InitializeBuffer(dataRequest->GetLength()), res, E_OUTOFMEMORY);

            if (SUCCEEDED(res))
            {
              caller->streamPackage->GetRequest()->SetStartTime(GetTickCount());
              caller->streamPackage->SetWaiting();
              caller->streamPackage->SetResponse(response);
            }

            CHECK_CONDITION_EXECUTE(FAILED(res), FREE_MEM_CLASS(response));
          }

          CHECK_CONDITION_EXECUTE(FAILED(res), caller->streamPackage->SetCompleted(res));
        }

        if (caller->streamPackage->GetState() == CStreamPackage::Waiting)
        {
          // in Waiting or WaitingIgnoreTimeout state can be request only if request and response are correctly set
          CStreamPackageDataRequest *dataRequest = dynamic_cast<CStreamPackageDataRequest *>(caller->streamPackage->GetRequest());

          if (dataRequest != NULL)
          {
            CStreamPackageDataResponse *dataResponse = dynamic_cast<CStreamPackageDataResponse *>(caller->streamPackage->GetResponse());

            // don not clear response buffer, we don't have to copy data again from start position
            // first try to find starting stream fragment (stream fragment which have first data)
            unsigned int foundDataLength = dataResponse->GetBuffer()->GetBufferOccupiedSpace();

            int64_t startPosition = dataRequest->GetStart() + foundDataLength - caller->positionOffset;
            unsigned int fragmentIndex = caller->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);

            while (fragmentIndex != UINT_MAX)
            {
              // get stream fragment
              CMpeg2tsStreamFragment *streamFragment = caller->streamFragments->GetItem(fragmentIndex);
              CMpeg2tsStreamFragment *startSearchingStreamFragment = caller->streamFragments->GetItem(caller->streamFragments->GetStartSearchingIndex());

              int64_t streamFragmentRelativeStart = streamFragment->GetFragmentStartPosition() - startSearchingStreamFragment->GetFragmentStartPosition();

              // set copy data start and copy data length
              unsigned int copyDataStart = (startPosition > streamFragmentRelativeStart) ? (unsigned int)(startPosition - streamFragmentRelativeStart) : 0;
              unsigned int copyDataLength = min(streamFragment->GetLength() - copyDataStart, dataRequest->GetLength() - foundDataLength);

              // copy data from stream fragment to response buffer
              if (caller->cacheFile->LoadItems(caller->streamFragments, fragmentIndex, true, UINT_MAX, (caller->lastProcessedSize == 0) ? CACHE_FILE_RELOAD_SIZE : caller->lastProcessedSize))
              {
                // memory is allocated while switching from Created to Waiting state, we can't have problem on next line
                dataResponse->GetBuffer()->AddToBufferWithResize(streamFragment->GetBuffer(), copyDataStart, copyDataLength);

                // update fragment loaded to memory time to avoid its freeing from memory
                streamFragment->SetLoadedToMemoryTime(GetTickCount(), fragmentIndex);
              }
              else
              {
                // we can't copy data, try it later
                break;
              }

              // update length of data
              foundDataLength += copyDataLength;
              caller->currentProcessedSize += copyDataLength;

              if ((streamFragment->IsDiscontinuity()) && ((dataRequest->GetStart() + dataRequest->GetLength()) >= (streamFragmentRelativeStart + streamFragment->GetLength())))
              {
                caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, completing request, request '%u', start '%lld', size '%u', found: '%u', fragment start: %lld, fragment length: %u, start searching fragment start: %u", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), foundDataLength, streamFragment->GetFragmentStartPosition(), streamFragment->GetLength(), startSearchingStreamFragment->GetFragmentStartPosition());

                dataResponse->SetDiscontinuity(true);
                break;
              }
              else if (foundDataLength < dataRequest->GetLength())
              {
                // find another stream fragment after end of this stream fragment
                startPosition += copyDataLength;

                // find another stream fragment after end of this stream fragment
                fragmentIndex = caller->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);
              }
              else
              {
                // do not find any more media packets for this request because we have enough data
                break;
              }
            }

            if (foundDataLength < dataRequest->GetLength())
            {
              // found data length is lower than requested
              // check request flags, maybe we can complete request

              if ((dataRequest->IsSetAnyNonZeroDataLength() || dataRequest->IsSetAnyDataLength()) && (foundDataLength > 0))
              {
                // request can be completed with any length of available data
                caller->streamPackage->SetCompleted(S_OK);
              }
              else if (dataRequest->IsSetAnyDataLength() && (foundDataLength == 0))
              {
                // no data available, check end of stream and connection lost

                if (caller->IsConnectionLostCannotReopen())
                {
                  // connection is lost and we cannot reopen it
                  caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), caller->streamLength);

                  dataResponse->SetConnectionLostCannotReopen(true);
                  caller->streamPackage->SetCompleted(S_OK);
                }
                else if (caller->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= caller->streamLength))
                {
                  // we are not receiving more data, complete request
                  caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), caller->streamLength);

                  dataResponse->SetNoMoreDataAvailable(true);
                  caller->streamPackage->SetCompleted(S_OK);
                }
              }
              else
              {
                if (dataResponse->IsDiscontinuity())
                {
                  caller->streamPackage->SetCompleted(S_OK);
                }
                else if (caller->IsConnectionLostCannotReopen())
                {
                  // connection is lost and we cannot reopen it
                  caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), caller->streamLength);

                  dataResponse->SetConnectionLostCannotReopen(true);
                  caller->streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_CONNECTION_LOST_CANNOT_REOPEN);
                }
                else if (caller->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= caller->streamLength))
                {
                  // we are not receiving more data, complete request
                  caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), caller->streamLength);

                  dataResponse->SetNoMoreDataAvailable(true);
                  caller->streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_NO_MORE_DATA_AVAILABLE);
                }
                //else if (caller->IsLiveStreamDetected() && (caller->connectionState != Opened))
                //{
                //  // we have live stream, we are missing data and we have not opened connection
                //  // we lost some data, report discontinuity

                //  dataResponse->SetDiscontinuity(true);
                //  streamPackage->SetCompleted(S_OK);
                //}
              }

              if (caller->streamPackage->GetState() == CStreamPackage::Waiting)
              {
                caller->flags |= MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_RECEIVE_DATA;
              }

              if ((caller->GetSeekingCapabilities() == SEEKING_METHOD_TIME) && (caller->streamPackage->GetState() == CStreamPackage::Waiting))
              {
                // no seeking by position is available
                // requested position is probably in stream fragment on the end of searchable stream fragments, between this->streamFragments->GetStartSearchingIndex() and this->streamFragments->GetSearchCount()
                // check if fragment is downloading
                // if fragment is not downloading, then schedule it for download

                unsigned int fragmentIndex = caller->streamFragments->GetStartSearchingIndex() + caller->streamFragments->GetSearchCount();
                CMpeg2tsStreamFragment *fragment = caller->streamFragments->GetItem(fragmentIndex);

                if (fragment == NULL)
                {
                  // bad, no such fragment exists, we don't have data

                  caller->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', not found stream fragment", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

                  dataResponse->SetNoMoreDataAvailable(true);
                  caller->streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
                  caller->flags &= ~MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_RECEIVE_DATA;
                }

                if ((fragment != NULL) && (!fragment->IsDownloaded()) && (fragmentIndex != caller->streamFragmentDownloading) && (fragmentIndex != caller->streamFragmentToDownload))
                {
                  // fragment is not downloaded and also is not downloading currently
                  caller->streamFragmentDownloading = UINT_MAX;
                  caller->streamFragmentToDownload = fragmentIndex;

                  caller->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', stream fragment not downloaded and not downloading, scheduled for download", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());
                }
              }

              if ((caller->GetSeekingCapabilities() == SEEKING_METHOD_POSITION) && (caller->streamPackage->GetState() == CStreamPackage::Waiting))
              {
                // no seeking by time is available

                CMpeg2tsStreamFragment *firstFragment = caller->streamFragments->GetItem(0);
                CMpeg2tsStreamFragment *lastFragment = caller->streamFragments->GetItem(caller->streamFragments->Count() - 1);

                if ((firstFragment->GetFragmentStartPosition() > dataRequest->GetStart()) ||
                  (lastFragment->IsProcessed() && ((lastFragment->GetFragmentStartPosition() + dataRequest->GetLength()) < dataRequest->GetStart())) ||
                  ((!lastFragment->IsProcessed()) && ((lastFragment->GetRequestStartPosition() + dataRequest->GetLength()) < dataRequest->GetStart())))
                {
                  caller->logger->Log(LOGGER_INFO, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', not found stream fragment, creating new stream fragment", PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

                  caller->streamFragments->Clear();
                  caller->cacheFile->Clear();

                  CMpeg2tsStreamFragment *fragment = new CMpeg2tsStreamFragment(&result);
                  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

                  if (SUCCEEDED(result))
                  {
                    fragment->SetFragmentStartPosition(dataRequest->GetStart());
                    fragment->SetRequestStartPosition(dataRequest->GetStart());
                  }

                  CHECK_CONDITION_HRESULT(result, caller->streamFragments->Add(fragment), result, E_OUTOFMEMORY);
                  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));

                  if (SUCCEEDED(result))
                  {
                    caller->streamFragmentToDownload = 0;
                    caller->streamFragmentDownloading = UINT_MAX;

                    caller->positionOffset = dataRequest->GetStart();
                    caller->streamLength = caller->GetBytePosition();

                    // set start searching index to current processing stream fragment
                    caller->streamFragments->SetStartSearchingIndex(caller->streamFragmentToDownload);
                    // set count of fragments to search for specific position
                    unsigned int firstNotDownloadedFragmentIndex = caller->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(caller->streamFragmentToDownload);
                    caller->streamFragments->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? caller->streamFragments->Count() : firstNotDownloadedFragmentIndex) - caller->streamFragmentToDownload);

                    caller->flags &= ~(PARSER_PLUGIN_FLAG_SET_STREAM_LENGTH | PARSER_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PARSER_PLUGIN_FLAG_END_OF_STREAM_REACHED);
                    caller->flags |= PARSER_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
                  }
                }
              }
            }
            else if (foundDataLength == dataRequest->GetLength())
            {
              // found data length is equal than requested
              caller->streamPackage->SetCompleted(S_OK);
              caller->flags &= ~MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_RECEIVE_DATA;
            }
          }
        }

        if (caller->IsSetFlags(PARSER_PLUGIN_FLAG_DUMP_OUTPUT_DATA) && (caller->streamPackage->GetState() == CStreamPackage::Completed))
        {
          CMpeg2TsDumpBox *dumpBox = new CMpeg2TsDumpBox(&result);
          CHECK_CONDITION_HRESULT(result, dumpBox, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            dumpBox->SetOutputData(true);
            dumpBox->SetTimeWithLocalTime();
            dumpBox->SetStreamPackage(caller->streamPackage);
          }

          CHECK_CONDITION_HRESULT(result, caller->dumpFile->AddDumpBox(dumpBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dumpBox));
        }
      }

      UNLOCK_MUTEX(caller->mutex)
    }

    // store stream fragments to temporary file
    if ((GetTickCount() - caller->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
    {
      // don't wait too long, we can store received stream fragments later
      LOCK_MUTEX(caller->mutex, 20)

      caller->lastStoreTime = GetTickCount();

      if (caller->currentProcessedSize != 0)
      {
        caller->lastProcessedSize = caller->currentProcessedSize;
      }
      caller->currentProcessedSize = 0;

      if (caller->cacheFile->GetCacheFile() == NULL)
      {
        wchar_t *storeFilePath = caller->GetStoreFile(L"temp");
        CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, caller->cacheFile->SetCacheFile(storeFilePath));
        FREE_MEM(storeFilePath);
      }
        
      // in case of live stream remove all downloaded and processed stream fragments before reported stream time
      if ((caller->IsLiveStream()) && (caller->reportedStreamTime > 0))
      {
        unsigned int fragmentRemoveStart = (caller->streamFragments->GetStartSearchingIndex() == 0) ? 1 : 0;
        unsigned int fragmentRemoveCount = 0;

        while ((fragmentRemoveStart + fragmentRemoveCount) < caller->streamFragments->Count())
        {
          CMpeg2tsStreamFragment *fragment = caller->streamFragments->GetItem(fragmentRemoveStart + fragmentRemoveCount);

          if (((fragmentRemoveStart + fragmentRemoveCount) != caller->streamFragments->GetStartSearchingIndex()) && fragment->IsProcessed() && ((fragment->GetFragmentStartPosition() + (int64_t)fragment->GetLength()) < (int64_t)caller->reportedStreamPosition))
          {
            // fragment will be removed
            fragmentRemoveCount++;
          }
          else
          {
            break;
          }
        }

        if ((fragmentRemoveCount > 0) && (caller->cacheFile->RemoveItems(caller->streamFragments, fragmentRemoveStart, fragmentRemoveCount)))
        {
          unsigned int startSearchIndex = (fragmentRemoveCount > caller->streamFragments->GetStartSearchingIndex()) ? 0 : (caller->streamFragments->GetStartSearchingIndex() - fragmentRemoveCount);
          unsigned int searchCountDecrease = (fragmentRemoveCount > caller->streamFragments->GetStartSearchingIndex()) ? (fragmentRemoveCount - caller->streamFragments->GetStartSearchingIndex()) : 0;

          caller->streamFragments->SetStartSearchingIndex(startSearchIndex);
          caller->streamFragments->SetSearchCount(caller->streamFragments->GetSearchCount() - searchCountDecrease);

          caller->streamFragments->Remove(fragmentRemoveStart, fragmentRemoveCount);

          if (caller->streamFragmentDownloading != UINT_MAX)
          {
            caller->streamFragmentDownloading -= fragmentRemoveCount;
          }

          if (caller->streamFragmentToDownload != UINT_MAX)
          {
            caller->streamFragmentToDownload -= fragmentRemoveCount;
          }
        }
      }

      // store all stream fragments (which are not stored) to file
      if ((caller->cacheFile->GetCacheFile() != NULL) && (caller->streamFragments->Count() != 0) && (caller->streamFragments->GetLoadedToMemorySize() > CACHE_FILE_RELOAD_SIZE))
      {
        caller->cacheFile->StoreItems(caller->streamFragments, caller->lastStoreTime, false, false);
      }
      
      UNLOCK_MUTEX(caller->mutex)
    }

    switch (sleepMode)
    {
    case SLEEP_MODE_SHORT:
      Sleep(1);
      break;
    case SLEEP_MODE_LONG:
      Sleep(20);
      break;
    default:
      break;
    }
  }

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_WORKER_NAME);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}