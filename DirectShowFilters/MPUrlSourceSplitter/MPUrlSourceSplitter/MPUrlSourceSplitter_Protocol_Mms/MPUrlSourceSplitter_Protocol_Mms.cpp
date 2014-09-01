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

#include "MPUrlSourceSplitter_Protocol_Mms.h"
#include "MPUrlSourceSplitter_Protocol_Mms_Parameters.h"
#include "Parameters.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"

//#include <WinInet.h>
//#include <stdio.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Mmsd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Mms"
#endif

#ifndef AV_RL16
#   define AV_RL16(x)                           \
    ((((const uint8_t*)(x))[1] << 8) |          \
      ((const uint8_t*)(x))[0])
#endif
#ifndef AV_RL32
#   define AV_RL32(x)                                \
    (((uint32_t)((const uint8_t*)(x))[3] << 24) |    \
               (((const uint8_t*)(x))[2] << 16) |    \
               (((const uint8_t*)(x))[1] <<  8) |    \
                ((const uint8_t*)(x))[0])
#endif
#ifndef AV_RL64
#   define AV_RL64(x)                                   \
    (((uint64_t)((const uint8_t*)(x))[7] << 56) |       \
     ((uint64_t)((const uint8_t*)(x))[6] << 48) |       \
     ((uint64_t)((const uint8_t*)(x))[5] << 40) |       \
     ((uint64_t)((const uint8_t*)(x))[4] << 32) |       \
     ((uint64_t)((const uint8_t*)(x))[3] << 24) |       \
     ((uint64_t)((const uint8_t*)(x))[2] << 16) |       \
     ((uint64_t)((const uint8_t*)(x))[1] <<  8) |       \
      (uint64_t)((const uint8_t*)(x))[0])
#endif


CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Mms(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Mms *protocol = (CMPUrlSourceSplitter_Protocol_Mms *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Mms::CMPUrlSourceSplitter_Protocol_Mms(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->lockCurlMutex = NULL;
  this->lockMutex = NULL;
  //this->mainCurlInstance = NULL;
  //this->streamLength = 0;
  this->connectionState = None;
  //this->streamFragments = NULL;
  //this->cacheFile = NULL;
  //this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  //this->streamFragmentDownloading = UINT_MAX;
  //this->streamFragmentProcessing = 0;
  //this->streamFragmentToDownload = UINT_MAX;
  //this->lastFlvPacketTimestamp = 0;
  //this->lastCumulatedFlvTimestamp = 0;
  //this->headerAndMetaPacketSize = 0;
  //this->videoTimestampCorrection = 0;
  //this->audioTimestampCorrection = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    //this->cacheFile = new CCacheFile(result);
    //this->streamFragments = new CRtmpStreamFragmentCollection(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    //CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    //CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    version = CCurlInstance::GetCurlVersion();
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Protocol_Mms::~CMPUrlSourceSplitter_Protocol_Mms()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  /*FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->streamFragments);*/

  if (this->lockMutex != NULL)
  {
    CloseHandle(this->lockMutex);
    this->lockMutex = NULL;
  }

  if (this->lockCurlMutex != NULL)
  {
    CloseHandle(this->lockCurlMutex);
    this->lockCurlMutex = NULL;
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Mms::GetConnectionState(void)
{
  return this->connectionState;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::ParseUrl(const CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  this->ClearSession();

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CProtocolPluginConfiguration *protocolConfiguration = new CProtocolPluginConfiguration(&result, (CParameterCollection *)parameters);
    CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->Initialize(protocolConfiguration));
    FREE_MEM_CLASS(protocolConfiguration);
  }

  const wchar_t *url = this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL);
  CHECK_POINTER_HRESULT(result, url, result, E_URL_NOT_SPECIFIED);

  if (SUCCEEDED(result))
  {
    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
    if (urlComponents == NULL)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'url components'");
      result = E_OUTOFMEMORY;
    }

    if (SUCCEEDED(result))
    {
      ZeroURL(urlComponents);
      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

      this->logger->Log(LOGGER_INFO, L"%s: %s: url: %s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, url);

      if (!InternetCrackUrl(url, 0, 0, urlComponents))
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: InternetCrackUrl() error: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, GetLastError());
        result = E_FAIL;
      }
    }

    if (SUCCEEDED(result))
    {
      int length = urlComponents->dwSchemeLength + 1;
      ALLOC_MEM_DEFINE_SET(protocol, wchar_t, length, 0);
      if (protocol == NULL) 
      {
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'protocol'");
        result = E_OUTOFMEMORY;
      }

      if (SUCCEEDED(result))
      {
        wcsncat_s(protocol, length, urlComponents->lpszScheme, urlComponents->dwSchemeLength);

        bool supportedProtocol = false;
        for (int i = 0; i < TOTAL_SUPPORTED_PROTOCOLS; i++)
        {
          if (_wcsnicmp(urlComponents->lpszScheme, SUPPORTED_PROTOCOLS[i], urlComponents->dwSchemeLength) == 0)
          {
            supportedProtocol = true;
            break;
          }
        }

        if (!supportedProtocol)
        {
          // not supported protocol
          this->logger->Log(LOGGER_INFO, L"%s: %s: unsupported protocol '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
          result = E_FAIL;
        }
      }
      FREE_MEM(protocol);
    }

    FREE_MEM(urlComponents);
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::ReceiveData(CStreamPackage *streamPackage)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Mms::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MMS_OPEN_CONNECTION_TIMEOUT, true, MMS_OPEN_CONNECTION_TIMEOUT_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Mms::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MMS_OPEN_CONNECTION_SLEEP_TIME, true, MMS_OPEN_CONNECTION_SLEEP_TIME_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Mms::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MMS_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, MMS_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT);
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configuration);

  if (SUCCEEDED(result) && (parameters != NULL))
  {
    this->configuration->Append(parameters);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->StopReceivingData());

  this->connectionState = SUCCEEDED(result) ? Initializing : None;

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  //FREE_MEM_CLASS(this->mainCurlInstance);
  //this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA);

  this->connectionState = None;

  //this->streamFragmentDownloading = UINT_MAX;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    //streamProgress->SetTotalLength((this->streamLength == 0) ? 1 : this->streamLength);
    //streamProgress->SetCurrentLength((this->streamLength == 0) ? 0 : this->GetBytePosition());

    if (this->IsStreamLengthEstimated())
    {
      result = VFW_S_ESTIMATED;
    }
  }

  return result;
}

void CMPUrlSourceSplitter_Protocol_Mms::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  __super::ClearSession();
 
  //this->streamLength = 0;
  this->connectionState = None;
  //this->cacheFile->Clear();
  //this->streamFragments->Clear();
  //this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  //this->streamFragmentDownloading = UINT_MAX;
  //this->streamFragmentProcessing = 0;
  //this->streamFragmentToDownload = UINT_MAX;
  //this->lastFlvPacketTimestamp = 0;
  //this->lastCumulatedFlvTimestamp = 0;
  //this->headerAndMetaPacketSize = 0;
  //this->videoTimestampCorrection = 0;
  //this->audioTimestampCorrection = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_Mms::GetDuration(void)
{
  /*uint64_t duration = RTMP_DURATION_UNSPECIFIED;
  if ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetRtmpDownloadResponse()->GetDuration() != RTMP_DURATION_UNSPECIFIED))
  {
    duration = (this->mainCurlInstance->GetRtmpDownloadResponse()->GetDuration() != 0) ? this->mainCurlInstance->GetRtmpDownloadResponse()->GetDuration() : RTMP_DURATION_UNSPECIFIED;
  }

  return this->IsLiveStream() ? DURATION_LIVE_STREAM : ((duration != RTMP_DURATION_UNSPECIFIED) ? (int64_t)duration : DURATION_UNSPECIFIED);*/

  return DURATION_UNSPECIFIED;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::GetStreamInformation(CStreamInformationCollection *streams)
{
  // MMS protocol has always one stream (container)
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streams);

  if (SUCCEEDED(result))
  {
    CStreamInformation *streamInfo = new CStreamInformation(&result);
    CHECK_POINTER_HRESULT(result, streamInfo, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      streamInfo->SetContainer(true);
    }

    CHECK_CONDITION_HRESULT(result, streams->Add(streamInfo), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamInfo));
  }

  return result;
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Mms::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_Mms::SeekToTime(unsigned int streamId, int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  //// find fragment to process
  //// TO DO: implement better and faster seeking algorithm
  //for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
  //{
  //  // first RTMP stream fragment has always zero timestamp
  //  CRtmpStreamFragment *fragment = this->streamFragments->GetItem(i);
  //  CRtmpStreamFragment *nextFragment = this->streamFragments->GetItem(i + 1);

  //  int64_t fragmentTime = fragment->GetFragmentStartTimestamp();
  //  int64_t nextFragmentTime = (nextFragment == NULL) ? INT64_MAX : nextFragment->GetFragmentStartTimestamp();

  //  if ((fragmentTime <= time) && (nextFragmentTime >= time))
  //  {
  //    this->streamFragmentProcessing = i;

  //    result = (result == -1) ? fragmentTime : (min(result, fragmentTime));

  //    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: time %lld, fragment: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, fragmentTime, i);
  //    break;
  //  }
  //}

  //if (result != (-1))
  //{
  //  this->flags &= ~(PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SKIP_HEADER_AND_META);
  //  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

  //  // is set stream fragment to process
  //  // exists at least one stream fragment and cover all possible timestamps (0 - UINT64_MAX)

  //  this->streamLength = 0;

  //  CRtmpStreamFragment *previousFragment = (this->streamFragmentProcessing != 0) ? this->streamFragments->GetItem(this->streamFragmentProcessing - 1) : NULL;
  //  CRtmpStreamFragment *processingFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);

  //  if (processingFragment->ContainsHeaderOrMetaPacket())
  //  {
  //    // we must skip header and meta packet, FFmpeg can't handle them again
  //    this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SKIP_HEADER_AND_META;
  //  }

  //  if (!processingFragment->IsDownloaded())
  //  {
  //    // stream fragment is not downloaded, it is gap
  //    // split stream fragment

  //    CHECK_CONDITION_NOT_NULL_EXECUTE(previousFragment, previousFragment->SetDiscontinuity(true, this->streamFragmentProcessing - 1));
  //    result = time;

  //    // create new fragment within found fragment
  //    // calculate fragment start timestamp

  //    // first fragment start timestamp is always zero, no need to correct timestamp
  //    HRESULT res = S_OK;
  //    CRtmpStreamFragment *fragment = new CRtmpStreamFragment(&res, time, false);
  //    CHECK_POINTER_HRESULT(res, fragment, res, E_OUTOFMEMORY)

  //    result = SUCCEEDED(res) ? result : res;

  //    if (result >= 0)
  //    {
  //      fragment->SetFragmentStartPosition(0);
  //      this->streamFragmentProcessing = this->streamFragmentProcessing + 1;

  //      result = this->streamFragments->Insert(this->streamFragmentProcessing, fragment) ? result : E_OUTOFMEMORY;
  //    }

  //    if (result >= 0)
  //    {
  //      // force to download missing fragment
  //      this->streamFragmentToDownload = this->streamFragmentProcessing;
  //    }
  //    
  //    this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA;
  //  }

  //  // set start searching index to current processing stream fragment
  //  this->streamFragments->SetStartSearchingIndex(this->streamFragmentProcessing);
  //  // set count of fragments to search for specific position
  //  unsigned int firstNotDownloadedFragmentIndex = this->streamFragments->GetFirstNotDownloadedItemIndex(this->streamFragmentProcessing);

  //  if (firstNotDownloadedFragmentIndex == UINT_MAX)
  //  {
  //    this->streamFragments->SetSearchCount(this->streamFragments->Count() - this->streamFragmentProcessing);
  //  }
  //  else
  //  {
  //    // clear end of stream reached flag, because we moved to place with gap between processing stream fragment and last stream fragment
  //    this->flags &= ~PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

  //    this->streamFragments->SetSearchCount(firstNotDownloadedFragmentIndex - this->streamFragmentProcessing);
  //  }
  //}

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Mms::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Mms::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Mms::Initialize(CPluginConfiguration *configuration)
{
  CProtocolPluginConfiguration *protocolConfiguration = (CProtocolPluginConfiguration *)configuration;
  HRESULT result = ((this->lockMutex != NULL) && (this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(protocolConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT) ? PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED : PROTOCOL_PLUGIN_FLAG_NONE;
  }

  return result;
}

/* protected methods */

wchar_t *CMPUrlSourceSplitter_Protocol_Mms::GetStoreFile(const wchar_t *extension)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_mms_%s.%s", folder, guid, extension);
    }
    FREE_MEM(guid);
  }

  return result;
}

/* old implementation */

//CMPUrlSourceSplitter_Protocol_Mms::CMPUrlSourceSplitter_Protocol_Mms(CLogger *logger, CParameterCollection *configuration)
//{
//  this->configurationParameters = new CParameterCollection();
//  if (configuration != NULL)
//  {
//    this->configurationParameters->Append(configuration);
//  }
//
//  this->logger = new CLogger(logger);
//  this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);
//
//  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS);
//  if (version != NULL)
//  {
//    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
//  }
//  FREE_MEM(version);
//
//  version = CCurlInstance::GetCurlVersion();
//  if (version != NULL)
//  {
//    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
//  }
//  FREE_MEM(version);
//  
//  this->receiveDataTimeout = MMS_RECEIVE_DATA_TIMEOUT_DEFAULT;
//  this->streamLength = 0;
//  this->setLength = false;
//  this->lengthCanBeSet = false;
//  this->streamTime = 0;
//  this->bytePosition = 0;
//  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
//  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
//  this->internalExitRequest = false;
//  this->wholeStreamDownloaded = false;
//  this->mainCurlInstance = NULL;
//  this->seekingActive = false;
//  this->supressData = false;
//  this->sequenceNumber = 1;
//  this->receivingData = false;
//  this->mmsContext = NULL;
//  this->streamEndedLogged = false;
//  this->clientGuid = GUID_NULL;
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
//}
//
//CMPUrlSourceSplitter_Protocol_Mms::~CMPUrlSourceSplitter_Protocol_Mms()
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//
//  if (this->IsConnected())
//  {
//    this->StopReceivingData();
//  }
//
//  FREE_MEM_CLASS(this->mainCurlInstance);
//  FREE_MEM_CLASS(this->configurationParameters);
//  FREE_MEM_CLASS(this->mmsContext);
//
//  delete this->configurationParameters;
//
//  if (this->lockMutex != NULL)
//  {
//    CloseHandle(this->lockMutex);
//    this->lockMutex = NULL;
//  }
//  if (this->lockCurlMutex != NULL)
//  {
//    CloseHandle(this->lockCurlMutex);
//    this->lockCurlMutex = NULL;
//  }
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//  FREE_MEM_CLASS(this->logger);
//}
//
//// IProtocol interface
//
//bool CMPUrlSourceSplitter_Protocol_Mms::IsConnected(void)
//{
//  return ((this->mainCurlInstance != NULL) || (this->wholeStreamDownloaded) || (this->seekingActive));
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::ParseUrl(const CParameterCollection *parameters)
//{
//  HRESULT result = S_OK;
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
//
//  this->ClearSession();
//  if (parameters != NULL)
//  {
//    this->configurationParameters->Clear();
//    ALLOC_MEM_DEFINE_SET(protocolConfiguration, ProtocolPluginConfiguration, 1, 0);
//    if (protocolConfiguration != NULL)
//    {
//      protocolConfiguration->configuration = (CParameterCollection *)parameters;
//    }
//    this->Initialize(protocolConfiguration);
//    FREE_MEM(protocolConfiguration);
//  }
//
//  const wchar_t *url = this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL);
//  if (SUCCEEDED(result))
//  {
//    result = (url == NULL) ? E_OUTOFMEMORY : S_OK;
//  }
//
//  if (SUCCEEDED(result))
//  {
//    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
//    if (urlComponents == NULL)
//    {
//      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'url components'");
//      result = E_OUTOFMEMORY;
//    }
//
//    if (SUCCEEDED(result))
//    {
//      ZeroURL(urlComponents);
//      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);
//
//      this->logger->Log(LOGGER_INFO, L"%s: %s: url: %s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, url);
//
//      if (!InternetCrackUrl(url, 0, 0, urlComponents))
//      {
//        this->logger->Log(LOGGER_ERROR, L"%s: %s: InternetCrackUrl() error: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, GetLastError());
//        result = E_FAIL;
//      }
//    }
//
//    if (SUCCEEDED(result))
//    {
//      int length = urlComponents->dwSchemeLength + 1;
//      ALLOC_MEM_DEFINE_SET(protocol, wchar_t, length, 0);
//      if (protocol == NULL) 
//      {
//        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'protocol'");
//        result = E_OUTOFMEMORY;
//      }
//
//      if (SUCCEEDED(result))
//      {
//        wcsncat_s(protocol, length, urlComponents->lpszScheme, urlComponents->dwSchemeLength);
//
//        bool supportedProtocol = false;
//        for (int i = 0; i < TOTAL_SUPPORTED_PROTOCOLS; i++)
//        {
//          if (_wcsnicmp(urlComponents->lpszScheme, SUPPORTED_PROTOCOLS[i], urlComponents->dwSchemeLength) == 0)
//          {
//            supportedProtocol = true;
//            break;
//          }
//        }
//
//        if (!supportedProtocol)
//        {
//          // not supported protocol
//          this->logger->Log(LOGGER_INFO, L"%s: %s: unsupported protocol '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
//          result = E_FAIL;
//        }
//      }
//      FREE_MEM(protocol);
//    }
//
//    FREE_MEM(urlComponents);
//  }
//
//  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::ReceiveData(CReceiveData *receiveData)
//{
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  if (this->internalExitRequest)
//  {
//    // there is internal exit request pending == changed timestamp
//    // close connection
//    this->StopReceivingData();
//
//    // reopen connection
//    // StartReceivingData() reset wholeStreamDownloaded
//    this->StartReceivingData(NULL);
//
//    this->internalExitRequest = false;
//  }
//
//  bool changingStream = false;
//  // MMS has always one stream
//  if ((!this->supressData) && (!this->seekingActive) && (receiveData->SetStreamCount(1)))
//  {
//    if (this->IsConnected())
//    {
//      if (!this->wholeStreamDownloaded)
//      {
//        long responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
//        if ((responseCode != 0) && ((responseCode < 200) || (responseCode >= 400)))
//        {
//          // response code 200 - 299 = OK
//          // response code 300 - 399 = redirect (OK)
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: error response code: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, responseCode);
//        }
//
//        if ((responseCode >= 200) && (responseCode < 400))
//        {
//          if (this->receivingData)
//          {
//            if ((!this->setLength) && (this->lengthCanBeSet))
//            {
//              double streamSize = this->mainCurlInstance->GetDownloadContentLength();
//              if ((streamSize > 0) && (this->streamTime < streamSize))
//              {
//                LONGLONG total = LONGLONG(streamSize);
//                this->streamLength = total;
//                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, total);
//                receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(total, false);
//                this->setLength = true;
//              }
//              else
//              {
//                if ((this->streamLength == 0) || (this->bytePosition > (this->streamLength * 3 / 4)))
//                {
//                  if (this->bytePosition != 0)
//                  {
//                    if (this->streamLength == 0)
//                    {
//                      // error occured or stream duration is not set
//                      // just make guess
//                      this->streamLength = LONGLONG(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
//                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//                      receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, true);
//                    }
//                    else if ((this->bytePosition > (this->streamLength * 3 / 4)))
//                    {
//                      // it is time to adjust stream length, we are approaching to end but still we don't know total length
//                      this->streamLength = this->bytePosition * 2;
//                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: adjusting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//                      receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, true);
//                    }
//                  }
//                }
//              }
//            }
//          }
//
//          {
//            CLockMutex lockData(this->lockCurlMutex, INFINITE);
//
//            unsigned int bytesRead = this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
//            if (bytesRead != 0)
//            {
//              unsigned int bufferSize = this->mmsContext->GetBuffer()->GetBufferSize();
//              unsigned int freeSpace = this->mmsContext->GetBuffer()->GetBufferFreeSpace();
//              unsigned int newBufferSize = max(bufferSize * 2, bufferSize + bytesRead);
//
//              if (freeSpace < bytesRead)
//              {
//                this->logger->Log(LOGGER_INFO, L"%s: %s: not enough free space in buffer for received data, buffer size: %d, free size: %d, received data: %d, new buffer size: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, bytesRead, newBufferSize);
//                if (!this->mmsContext->GetBuffer()->ResizeBuffer(newBufferSize))
//                {
//                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"resizing of buffer unsuccessful");
//                  // it indicates error
//                  bytesRead = 0;
//                }
//              }
//
//              if (bytesRead != 0)
//              {
//                this->mmsContext->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData());
//                this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(bytesRead);
//              }
//            }
//          }
//        }
//
//        if (this->receivingData)
//        {
//          if (this->mmsContext->GetAsfHeaderLength() != 0)
//          {
//            // ASF header was not send to filter
//
//            if (this->sendAsfHeader)
//            {
//              // create media packet
//              // set values of media packet
//              CMediaPacket *mediaPacket = new CMediaPacket();
//              mediaPacket->GetBuffer()->InitializeBuffer(this->mmsContext->GetAsfHeaderLength());
//              mediaPacket->GetBuffer()->AddToBuffer(this->mmsContext->GetAsfHeader(), this->mmsContext->GetAsfHeaderLength());
//              mediaPacket->SetStart(this->bytePosition);
//              mediaPacket->SetEnd(this->bytePosition + this->mmsContext->GetAsfHeaderLength() - 1);
//
//              if (!receiveData->GetStreams()->GetItem(0)->GetMediaPacketCollection()->Add(mediaPacket))
//              {
//                FREE_MEM_CLASS(mediaPacket);
//              }
//
//              this->bytePosition += this->mmsContext->GetAsfHeaderLength();
//            }
//
//            this->mmsContext->ClearAsfHeader();
//          }
//
//          bool finish = false;
//          while (!finish)
//          {
//            MMSChunk *chunk = new MMSChunk();
//            if (chunk != NULL)
//            {
//              if (SUCCEEDED(this->GetMmsChunk(this->mmsContext, chunk)))
//              {
//                switch (chunk->GetChunkType())
//                {
//                case CHUNK_TYPE_END:
//                  {
//                    this->mmsContext->SetChunkSequence(0);
//                    if (!this->streamEndedLogged)
//                    {
//                      this->logger->Log(LOGGER_INFO, L"%s: %s: stream ended", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
//                      this->streamEndedLogged = true;
//                    }
//                  }
//                  break;
//                case CHUNK_TYPE_STREAM_CHANGE:
//                  {
//                    /*this->receivingData = false;
//                    this->mmsContext->SetHeaderParsed(false);
//                    HRESULT result = this->GetMmsHeaderData(this->mmsContext, chunk);
//                    if (FAILED(result))
//                    {
//                      this->logger->Log(LOGGER_ERROR, L"%s: %s: get MMS header data failed with error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, result);
//                      this->StopReceivingData();
//                    }*/
//                    changingStream = true;
//                  }
//                  break;
//                case CHUNK_TYPE_DATA:
//                  {
//                    // create media packet
//                    // set values of media packet
//                    unsigned int packetLength = this->mmsContext->GetAsfPacketLength();
//                    if (packetLength < chunk->GetChunkDataLength())
//                    {
//                      // error : ASF packet length is less than parsed chunk
//                      this->logger->Log(LOGGER_WARNING, L"%s: %s: parsed chunk is bigger than ASF packet length, chunk length: %d, ASF packet length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, chunk->GetChunkDataLength(), packetLength);
//                    }
//                    else
//                    {
//                      // packet must be padded with zeros until packetLength
//                      CMediaPacket *mediaPacket = new CMediaPacket();
//                      mediaPacket->GetBuffer()->InitializeBuffer(packetLength, 0);
//                      mediaPacket->GetBuffer()->AddToBuffer(chunk->GetChunkData(), chunk->GetChunkDataLength());
//
//                      if (packetLength > chunk->GetChunkDataLength())
//                      {
//                        unsigned int paddingLength = packetLength - chunk->GetChunkDataLength();
//                        ALLOC_MEM_DEFINE_SET(paddingBuffer, unsigned char, paddingLength, 0);
//                        if (paddingBuffer != NULL)
//                        {
//                          mediaPacket->GetBuffer()->AddToBuffer(paddingBuffer, paddingLength);
//                        }
//                        else
//                        {
//                          this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot create padding buffer", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
//                        }
//                        FREE_MEM(paddingBuffer);
//                      }
//
//                      mediaPacket->SetStart(this->bytePosition);
//                      mediaPacket->SetEnd(this->bytePosition + packetLength - 1);
//
//                      if (!receiveData->GetStreams()->GetItem(0)->GetMediaPacketCollection()->Add(mediaPacket))
//                      {
//                        FREE_MEM_CLASS(mediaPacket);
//                      }
//
//                      this->bytePosition += packetLength;
//                    }
//                  }
//                  break;
//                default:
//                  // ignore unknown packets
//                  break;
//                }
//              }
//              else
//              {
//                finish = true;
//              }
//
//              FREE_MEM_CLASS(chunk);
//            }
//            else
//            {
//              finish = true;
//            }
//          }
//        }
//
//        if ((changingStream) || (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
//        {
//          if ((changingStream) || (this->mainCurlInstance->GetDownloadResponse()->GetResultCode() == CURLE_OK))
//          {
//            // all data received, we're not receiving data
//            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: received all data", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
//
//            // whole stream downloaded
//            this->wholeStreamDownloaded = true;
//            FREE_MEM_CLASS(this->mainCurlInstance);
//
//            if (!this->seekingActive)
//            {
//              // we are not seeking, so we can set total length
//              if (!this->setLength)
//              {
//                this->streamLength = this->bytePosition;
//                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//                receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, false);
//                this->setLength = true;
//              }
//
//              // notify filter the we reached end of stream
//              receiveData->GetStreams()->GetItem(0)->GetEndOfStreamReached()->SetStreamPosition(max(0, this->bytePosition - 1));
//            }
//
//
//            if (changingStream)
//            {
//              // stop receiving data, we are done with this video
//              this->StopReceivingData();
//            }
//          }
//          else
//          {
//            // error occured while downloading
//            // try to continue with downloading data
//            this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: %d, restarting download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetResultCode());
//            FREE_MEM_CLASS(this->mainCurlInstance);
//          }
//        }
//      }
//      else
//      {
//        // set total length (if not set earlier)
//        if ((!this->setLength) && (this->lengthCanBeSet))
//        {
//          this->streamLength = this->bytePosition;
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//          receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, false);
//          this->setLength = true;
//        }
//      }
//    }
//  }
//
//  return S_OK;
//}
//
//CParameterCollection *CMPUrlSourceSplitter_Protocol_Mms::GetConnectionParameters(void)
//{
//  CParameterCollection *result = new CParameterCollection();
//
//  if (result != NULL)
//  {
//    if (!result->Append(this->configurationParameters))
//    {
//      FREE_MEM_CLASS(result);
//    }
//  }
//  
//  return result;
//}
//
//// ISimpleProtocol interface
//
//unsigned int CMPUrlSourceSplitter_Protocol_Mms::GetReceiveDataTimeout(void)
//{
//  return this->receiveDataTimeout;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::StartReceivingData(CParameterCollection *parameters)
//{
//  HRESULT result = S_OK;
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);
//
//  CHECK_POINTER_DEFAULT_HRESULT(result, this->configurationParameters);
//
//  if (SUCCEEDED(result) && (parameters != NULL))
//  {
//    this->configurationParameters->Append(parameters);
//  }
//
//  unsigned int finishTime = UINT_MAX;
//  if (SUCCEEDED(result))
//  {
//    finishTime = this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, UINT_MAX);
//    if (finishTime != UINT_MAX)
//    {
//      unsigned int currentTime = GetTickCount();
//      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: finish time specified, current time: %u, finish time: %u, diff: %u (ms)", PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, currentTime, finishTime, finishTime - currentTime);
//    }
//  }
//
//  wchar_t *url = NULL;
//  if (SUCCEEDED(result))
//  {
//    url = ReplaceString(this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL), L"mms://", L"http://");
//    if (url == NULL)
//    {
//      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot allocate memory for RTMP url");
//      result = E_OUTOFMEMORY;
//    }
//  }
//
//  wchar_t *clientGuidString = NULL;
//
//  if (SUCCEEDED(result))
//  {
//    result = CoCreateGuid(&this->clientGuid);
//    if (FAILED(result))
//    {
//      this->clientGuid = GUID_NULL;
//    }
//  }
//
//  if (SUCCEEDED(result))
//  {
//    wchar_t *guidTemp = ConvertGuidToString(this->clientGuid);
//    CHECK_POINTER_HRESULT(result, guidTemp, result, E_OUTOFMEMORY);
//
//    if (SUCCEEDED(result))
//    {
//      clientGuidString = FormatString(CLIENTGUID_FORMAT_VALUE, guidTemp);
//      CHECK_POINTER_HRESULT(result, clientGuidString, result, E_OUTOFMEMORY);
//    }
//
//    FREE_MEM(guidTemp);
//  }
//
//  // open connection and send first command
//  if (SUCCEEDED(result))
//  {
//    // lock access to stream
//    CLockMutex lock(this->lockMutex, INFINITE);
//
//    this->wholeStreamDownloaded = false;
//
//    if (SUCCEEDED(result) && (this->mainCurlInstance == NULL))
//    {
//      this->mainCurlInstance = new CHttpCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
//      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);
//    }
//
//    if (SUCCEEDED(result))
//    {
//      CHttpDownloadRequest *request = new CHttpDownloadRequest();
//      CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);
//
//      if (SUCCEEDED(result))
//      {
//        request->SetUrl(url);
//        request->SetReferer(this->configurationParameters->GetValue(PARAMETER_NAME_MMS_REFERER, true, NULL));
//        request->SetUserAgent(this->configurationParameters->GetValue(PARAMETER_NAME_MMS_USER_AGENT, true, NULL));
//        request->SetCookie(this->configurationParameters->GetValue(PARAMETER_NAME_MMS_COOKIE, true, NULL));
//        request->SetHttpVersion(this->configurationParameters->GetValueLong(PARAMETER_NAME_MMS_VERSION, true, HTTP_VERSION_DEFAULT));
//        request->SetIgnoreContentLength((this->configurationParameters->GetValueLong(PARAMETER_NAME_MMS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
//
//        this->sequenceNumber = 1;
//        result = request->GetHeaders()->Add(L"Accept", L"*/*") ? S_OK : E_FAIL;
//        result = request->GetHeaders()->Add(USERAGENT_NAME, USERAGENT_VALUE) ? result : E_FAIL;
//
//        wchar_t *pragma = NULL;
//        if (this->bytePosition == 0)
//        {
//          pragma = FormatString(L"no-cache,rate=1.000000,stream-time=%u,request-context=%u,max-duration=0", this->streamTime, this->sequenceNumber++);
//        }
//        else
//        {
//          pragma = FormatString(L"no-cache,rate=1.000000,stream-offset=%u:%u,request-context=%u,max-duration=0", (unsigned int)(((uint64_t)this->bytePosition) >> 32), (unsigned int)(this->bytePosition & 0xFFFFFFFF), this->sequenceNumber++);
//        }
//        CHECK_POINTER_HRESULT(result, pragma, result, E_OUTOFMEMORY);
//        
//        if (SUCCEEDED(result))
//        {
//          result = request->GetHeaders()->Add(L"Pragma", pragma) ? result : E_FAIL;
//        }
//        FREE_MEM(pragma);
//
//        if (SUCCEEDED(result))
//        {
//          result = request->GetHeaders()->Add(L"Pragma", clientGuidString) ? result : E_FAIL;
//          result = request->GetHeaders()->Add(L"Connection", L"Close") ? result : E_FAIL;
//
//          // set finish time, all methods must return before finish time
//          this->mainCurlInstance->SetFinishTime(finishTime);
//
//          this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
//          this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
//
//          if (SUCCEEDED(result))
//          {
//            result = (this->mainCurlInstance->Initialize(request)) ? S_OK : E_FAIL;
//          }
//        }
//      }
//      FREE_MEM_CLASS(request);
//    }
//
//    if (SUCCEEDED(result))
//    {
//      FREE_MEM_CLASS(this->mmsContext);
//
//      this->mmsContext = new MMSContext();
//      result = (this->mmsContext != NULL) ? S_OK : E_OUTOFMEMORY;
//    }
//
//    if (SUCCEEDED(result))
//    {
//      this->mmsContext->SetFinishTime(finishTime);
//
//      // all parameters set
//      // start receiving data
//
//      result = (this->mainCurlInstance->StartReceivingData()) ? S_OK : E_FAIL;
//    }
//
//    if (SUCCEEDED(result))
//    {
//      // wait for HTTP status code
//
//      long responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
//      while (responseCode == 0)
//      {
//        responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
//        if ((responseCode != 0) && ((responseCode < 200) || (responseCode >= 400)))
//        {
//          // response code 200 - 299 = OK
//          // response code 300 - 399 = redirect (OK)
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: HTTP status code: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, responseCode);
//          result = E_FAIL;
//        }
//
//        if ((responseCode == 0) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
//        {
//          // we received all data, but no response code
//          break;
//        }
//
//        // wait some time
//        Sleep(10);
//      }
//    }
//  }
//
//  if (SUCCEEDED(result))
//  {
//    // get MMS header data and parse it
//    result = this->GetMmsHeaderData(this->mmsContext, NULL);
//
//    if (FAILED(result))
//    {
//      this->logger->Log(LOGGER_ERROR, L"%s: %s: (DESCRIBE) get MMS header data failed with error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
//    }
//    else
//    {
//      {
//        // lock access to stream
//        CLockMutex lock(this->lockMutex, INFINITE);
//
//        this->StopReceivingData();
//      }
//    }
//  }
//
//  if (SUCCEEDED(result))
//  {
//    // send play command
//    {
//      // lock access to stream
//      CLockMutex lock(this->lockMutex, INFINITE);
//
//      this->wholeStreamDownloaded = false;
//
//      this->mainCurlInstance = new CHttpCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
//      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);
//
//      if (SUCCEEDED(result))
//      {
//        CHttpDownloadRequest *request = new CHttpDownloadRequest();
//        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);
//
//        if (SUCCEEDED(result))
//        {
//          request->SetUrl(url);
//          request->SetReferer(this->configurationParameters->GetValue(PARAMETER_NAME_MMS_REFERER, true, NULL));
//          request->SetUserAgent(this->configurationParameters->GetValue(PARAMETER_NAME_MMS_USER_AGENT, true, NULL));
//          request->SetCookie(this->configurationParameters->GetValue(PARAMETER_NAME_MMS_COOKIE, true, NULL));
//          request->SetHttpVersion(this->configurationParameters->GetValueLong(PARAMETER_NAME_MMS_VERSION, true, HTTP_VERSION_DEFAULT));
//          request->SetIgnoreContentLength((this->configurationParameters->GetValueLong(PARAMETER_NAME_MMS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
//
//          this->sequenceNumber = 1;
//          result = request->GetHeaders()->Add(L"Accept", L"*/*") ? S_OK : E_FAIL;
//          result = request->GetHeaders()->Add(USERAGENT_NAME, USERAGENT_VALUE) ? result : E_FAIL;
//
//          wchar_t *pragma = NULL;
//          if (this->bytePosition == 0)
//          {
//            pragma = FormatString(L"no-cache,rate=1.000000,request-context=%u,stream-time=%u", this->sequenceNumber++, this->streamTime);
//          }
//          else
//          {
//            FormatString(L"no-cache,rate=1.000000,request-context=%u,stream-time=%u", this->sequenceNumber++, this->streamTime);
//            pragma = FormatString(L"no-cache,rate=1.000000,stream-offset=%u:%u,request-context=%u,max-duration=0", (unsigned int)(((uint64_t)this->bytePosition) >> 32), (unsigned int)(this->bytePosition & 0xFFFFFFFF), this->sequenceNumber++);
//          }
//          CHECK_POINTER_HRESULT(result, pragma, result, E_OUTOFMEMORY);
//
//          if (SUCCEEDED(result))
//          {
//            result = request->GetHeaders()->Add(L"Pragma", pragma) ? result : E_FAIL;
//          }
//          FREE_MEM(pragma);
//
//          if (SUCCEEDED(result))
//          {
//            result = request->GetHeaders()->Add(L"Pragma", L"xPlayStrm=1") ? result : E_FAIL;
//            result = request->GetHeaders()->Add(L"Pragma", clientGuidString) ? result : E_FAIL;
//
//            pragma = FormatString(L"stream-switch-count=%d", this->mmsContext->GetStreams()->Count());
//            result = request->GetHeaders()->Add(L"Pragma", pragma) ? result : E_FAIL;
//            FREE_MEM(pragma);
//
//            for (unsigned int i = 0; i < this->mmsContext->GetStreams()->Count(); i++)
//            {
//              MMSStream *stream = mmsContext->GetStreams()->GetItem(i);
//              wchar_t *temp = FormatString(L"%sffff:%d:0 ", (pragma == NULL) ? L"stream-switch-entry=" : pragma, stream->GetId());
//              FREE_MEM(pragma);
//              pragma = temp;
//            }
//            result = request->GetHeaders()->Add(L"Pragma", pragma) ? result : E_FAIL;
//          }
//          FREE_MEM(pragma);
//
//          if (SUCCEEDED(result))
//          {
//            result = request->GetHeaders()->Add(L"Connection", L"Close") ? result : E_FAIL;
//
//            // set finish time, all methods must return before finish time
//            this->mainCurlInstance->SetFinishTime(finishTime);
//
//            this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
//            this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
//
//            if (SUCCEEDED(result))
//            {
//              result = (this->mainCurlInstance->Initialize(request)) ? S_OK : E_FAIL;
//            }
//          }
//        }
//        FREE_MEM_CLASS(request);
//      }
//
//      if (SUCCEEDED(result))
//      {
//        // all parameters set
//        // start receiving data
//
//        result = (this->mainCurlInstance->StartReceivingData()) ? S_OK : E_FAIL;
//      }
//
//      if (SUCCEEDED(result))
//      {
//        // wait for HTTP status code
//
//        long responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
//        while (responseCode == 0)
//        {
//          responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
//          if ((responseCode != 0) && ((responseCode < 200) || (responseCode >= 400)))
//          {
//            // response code 200 - 299 = OK
//            // response code 300 - 399 = redirect (OK)
//            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: HTTP status code: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, responseCode);
//            result = E_FAIL;
//          }
//
//          if ((responseCode == 0) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
//          {
//            // we received all data, but no response code
//            break;
//          }
//
//          // wait some time
//          Sleep(10);
//        }
//      }
//    }
//  }
//
//  if (SUCCEEDED(result))
//  {
//    // get MMS header data and parse it
//    result = this->GetMmsHeaderData(this->mmsContext, NULL);
//
//    if (FAILED(result))
//    {
//      this->logger->Log(LOGGER_ERROR, L"%s: %s: (PLAY) get MMS header data failed with error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
//    }
//    else
//    {
//      {
//        // lock access to stream
//        CLockMutex lock(this->lockMutex, INFINITE);
//
//        // set that we are receiving data
//        this->receivingData = true;
//      }
//    }
//  }
//
//  if (FAILED(result))
//  {
//    this->StopReceivingData();
//  }
//
//  FREE_MEM(clientGuidString);
//  FREE_MEM(url);
//
//  // if seeking then not send ASF header
//  //this->sendAsfHeader = !this->seekingActive;
//
//  // if seeking then do not send ASF header
//  // if reopening connection then do not send ASF header (this->bytePosition isn't 0, this->seekingActive is false)
//  this->sendAsfHeader = (this->bytePosition == 0) && (!this->seekingActive);
//
//  // we are definitely not seeking
//  this->seekingActive = false;
//
//  // length can be set now
//  this->lengthCanBeSet = true;
//
//  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::StopReceivingData(void)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
//
//  // lock access to stream
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  FREE_MEM_CLASS(this->mainCurlInstance);
//
//  this->lengthCanBeSet = false;
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
//  return S_OK;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::QueryStreamProgress(CStreamProgress *streamProgress)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
//  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);
//
//  if (SUCCEEDED(result))
//  {
//    streamProgress->SetTotalLength((this->streamLength == 0) ? 1 : this->streamLength);
//    streamProgress->SetCurrentLength((this->streamLength == 0) ? 0 : this->bytePosition);
//
//    if (!this->setLength)
//    {
//      result = VFW_S_ESTIMATED;
//    }
//  }
//
//  return result;
//}
//  
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);
//  CHECK_CONDITION_HRESULT(result, availableLength->GetStreamId() == 0, result, E_INVALIDARG);
//
//  if (SUCCEEDED(result))
//  {
//    availableLength->SetAvailableLength((this->setLength) ? this->streamLength : this->bytePosition);
//  }
//
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::ClearSession(void)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//
//  if (this->IsConnected())
//  {
//    this->StopReceivingData();
//  }
// 
//  this->internalExitRequest = false;
//  this->streamLength = 0;
//  this->setLength = false;
//  this->lengthCanBeSet = false;
//  this->streamTime = 0;
//  this->bytePosition = 0;
//  this->wholeStreamDownloaded = false;
//  this->receiveDataTimeout = MMS_RECEIVE_DATA_TIMEOUT_DEFAULT;
//  this->sequenceNumber = 1;
//  this->receivingData = false;
//  this->streamEndedLogged = false;
//  this->seekingActive = false;
//
//  FREE_MEM_CLASS(this->mmsContext);
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//  return S_OK;
//}
//
//int64_t CMPUrlSourceSplitter_Protocol_Mms::GetDuration(void)
//{
//  return DURATION_UNSPECIFIED;
//}
//
//void CMPUrlSourceSplitter_Protocol_Mms::ReportStreamTime(uint64_t streamTime)
//{
//}
//
//// ISeeking interface
//
//unsigned int CMPUrlSourceSplitter_Protocol_Mms::GetSeekingCapabilities(void)
//{
//  return SEEKING_METHOD_TIME;
//}
//
//int64_t CMPUrlSourceSplitter_Protocol_Mms::SeekToTime(int64_t time)
//{
//  int64_t result = -1;
//
//  {
//    CLockMutex lock(this->lockMutex, INFINITE);
//
//    this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
//    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);
//
//    // this block ReceiveData() method from calling any method
//    this->seekingActive = true;
//
//    // close connection
//    this->StopReceivingData();
//
//    this->bytePosition = 0;
//  }
//
//  // MMS protocol can seek to ms
//  // time is in ms
//
//  // 1 second back
//  this->streamTime = max(0, time - 1000);
//
//  // do not lock mutex, because we need to receive data in StartReceivingData() method
//  // reopen connection
//  // StartReceivingData() reset wholeStreamDownloaded
//
//  if (SUCCEEDED(this->StartReceivingData(NULL)))
//  {
//    result = max(0, time - 1000);
//  }
//
//  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
//  return result;
//}
//
//void CMPUrlSourceSplitter_Protocol_Mms::SetSupressData(bool supressData)
//{
//  this->supressData = supressData;
//}
//
//// IPlugin interface
//
//const wchar_t *CMPUrlSourceSplitter_Protocol_Mms::GetName(void)
//{
//  return PROTOCOL_NAME;
//}
//
//GUID CMPUrlSourceSplitter_Protocol_Mms::GetInstanceId(void)
//{
//  return this->logger->GetLoggerInstanceId();
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::Initialize(PluginConfiguration *configuration)
//{
//  if (configuration == NULL)
//  {
//    return E_POINTER;
//  }
//
//  ProtocolPluginConfiguration *protocolConfiguration = (ProtocolPluginConfiguration *)configuration;
//  this->logger->SetParameters(protocolConfiguration->configuration);
//
//  if (this->lockMutex == NULL)
//  {
//    return E_FAIL;
//  }
//
//  this->configurationParameters->Clear();
//  if (protocolConfiguration->configuration != NULL)
//  {
//    this->configurationParameters->Append(protocolConfiguration->configuration);
//  }
//  this->configurationParameters->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
//
//  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_MMS_RECEIVE_DATA_TIMEOUT, true, MMS_RECEIVE_DATA_TIMEOUT_DEFAULT);
//
//  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? MMS_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
//
//  return S_OK;
//}
//
//// other methods
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::GetMmsChunk(MMSContext *mmsContext, MMSChunk *mmsChunk)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, mmsContext);
//  CHECK_POINTER_DEFAULT_HRESULT(result, mmsChunk);
//
//  if (SUCCEEDED(result))
//  {
//    result = mmsContext->IsValid() ? S_OK : E_INVALIDARG;
//    if (FAILED(result))
//    {
//      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_CHUNK_NAME, L"MMS context is not valid");
//    }
//  }
//
//  if (SUCCEEDED(result))
//  {
//    ALLOC_MEM_DEFINE_SET(chunkHeader, unsigned char, CHUNK_HEADER_LENGTH, 0);
//    ALLOC_MEM_DEFINE_SET(extHeader, unsigned char, EXT_HEADER_LENGTH, 0);
//
//    CHECK_POINTER_HRESULT(result, chunkHeader, S_OK, E_OUTOFMEMORY);
//    CHECK_POINTER_HRESULT(result, extHeader, S_OK, E_OUTOFMEMORY);
//    result = (mmsContext->GetBuffer()->GetBufferOccupiedSpace() >= CHUNK_HEADER_LENGTH) ? result : HRESULT_FROM_WIN32(ERROR_NO_DATA);
//
//    if (SUCCEEDED(result))
//    {
//      unsigned int chunkType = 0;
//      unsigned int chunkDataLength = 0;
//      unsigned int chunkPacketLength = 0;
//      unsigned int extHeaderLength = 0;
//
//      mmsContext->GetBuffer()->CopyFromBuffer(chunkHeader, CHUNK_HEADER_LENGTH);
//
//      chunkType = AV_RL16(chunkHeader);
//      chunkDataLength  = AV_RL16(chunkHeader + 2);
//      chunkPacketLength = chunkDataLength + 4;
//
//      result = (mmsContext->GetBuffer()->GetBufferOccupiedSpace() >= chunkPacketLength) ? result : HRESULT_FROM_WIN32(ERROR_NO_DATA);
//
//      if (SUCCEEDED(result))
//      {
//        switch (chunkType)
//        {
//        case CHUNK_TYPE_END:
//        case CHUNK_TYPE_STREAM_CHANGE:
//          extHeaderLength = 4;
//          break;
//        case CHUNK_TYPE_ASF_HEADER:
//        case CHUNK_TYPE_DATA:
//          extHeaderLength = 8;
//          break;
//        default:
//          this->logger->Log(LOGGER_WARNING, L"%s: %s: unknown chunk type: %d, length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_CHUNK_NAME, chunkType, chunkDataLength);
//          result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//        }
//      }
//
//      if (SUCCEEDED(result))
//      {
//        mmsContext->GetBuffer()->CopyFromBuffer(extHeader, extHeaderLength, CHUNK_HEADER_LENGTH);
//
//        mmsChunk->SetChunkType(chunkType);
//        if (SUCCEEDED(result))
//        {
//          ALLOC_MEM_DEFINE_SET(temp, unsigned char, (chunkDataLength - extHeaderLength), 0);
//          CHECK_POINTER(result, temp, result, E_OUTOFMEMORY);
//          if (SUCCEEDED(result))
//          {
//            mmsContext->GetBuffer()->CopyFromBuffer(temp, chunkDataLength - extHeaderLength, CHUNK_HEADER_LENGTH + extHeaderLength);
//            result = mmsChunk->SetChunkData(temp, chunkDataLength - extHeaderLength) ? result : E_OUTOFMEMORY;
//          }
//          FREE_MEM(temp);
//        }
//        if (SUCCEEDED(result))
//        {
//          ALLOC_MEM_DEFINE_SET(temp, unsigned char, extHeaderLength, 0);
//          CHECK_POINTER(result, temp, result, E_OUTOFMEMORY);
//          if (SUCCEEDED(result))
//          {
//            mmsContext->GetBuffer()->CopyFromBuffer(temp, extHeaderLength, CHUNK_HEADER_LENGTH);
//            result = mmsChunk->SetExtraHeaderData(temp, extHeaderLength) ? result : E_OUTOFMEMORY;
//          }
//          FREE_MEM(temp);
//        }
//
//        if (SUCCEEDED(result))
//        {
//          if ((chunkType == CHUNK_TYPE_END) || (chunkType == CHUNK_TYPE_DATA))
//          {
//            mmsContext->SetChunkSequence(AV_RL32(extHeader));
//          }
//
//          mmsContext->GetBuffer()->RemoveFromBufferAndMove(chunkPacketLength);
//        }
//        else
//        {
//          mmsChunk->Clear();
//        }
//      }
//    }
//
//    FREE_MEM(chunkHeader);
//    FREE_MEM(extHeader);
//  }
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::ParseMmsAsfHeader(MMSContext *mmsContext, MMSChunk *mmsChunk)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, mmsContext);
//  CHECK_POINTER_DEFAULT_HRESULT(result, mmsChunk);
//
//  if (FAILED(result))
//  {
//    return result;
//  }
//
//  mmsContext->GetStreams()->Clear();
//
//  if ((mmsChunk->GetChunkDataLength() < (sizeof(ASF_GUID) * 2 + 22)) ||
//    (memcmp(mmsChunk->GetChunkData(), ASF_HEADER, sizeof(ASF_GUID))))
//  {
//    this->logger->Log(LOGGER_ERROR, L"%s: %s: corrupt stream, invalid ASF header, size: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME, mmsChunk->GetChunkDataLength());
//    return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//  }
//
//  const unsigned char *start = mmsChunk->GetChunkData();
//  const unsigned char *end = mmsChunk->GetChunkData() + mmsChunk->GetChunkDataLength();
//
//  start += sizeof(ASF_GUID) + 14;
//  int flags;
//  int stream_id;
//
//  while ((SUCCEEDED(result)) && ((end - start) >= (sizeof(ASF_GUID) + 8)))
//  {
//    uint64_t chunksize;
//
//    if (!memcmp(start, ASF_DATA_HEADER, sizeof(ASF_GUID)))
//    {
//      chunksize = 50; // see Reference [2] section 5.1
//    }
//    else
//    {
//      chunksize = AV_RL64(start + sizeof(ASF_GUID));
//    }
//
//    if (!chunksize || (chunksize > (end - start)))
//    {
//      this->logger->Log(LOGGER_ERROR, L"%s: %s: header chunk size is invalid: %I64d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME, chunksize);
//      return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//    }
//
//    if (!memcmp(start, ASF_FILE_HEADER, sizeof(ASF_GUID)))
//    {
//      /* read packet size */
//      if ((end - start) > (sizeof(ASF_GUID) * 2 + 68))
//      {
//        mmsContext->SetAsfPacketLength(AV_RL32(start + sizeof(ASF_GUID) * 2 + 64));
//        this->logger->Log(LOGGER_INFO, L"%s: %s: ASF packet length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME, mmsContext->GetAsfPacketLength());
//        if (mmsContext->GetAsfPacketLength() > HEADER_BUFFER_SIZE)
//        {
//          this->logger->Log(LOGGER_ERROR, L"%s: %s: corrupted stream, too large ASF packet length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME, mmsContext->GetAsfPacketLength());
//          return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//        }
//      }
//    }
//    else if (!memcmp(start, ASF_STREAM_HEADER, sizeof(ASF_GUID)))
//    {
//      flags = AV_RL16(start + sizeof(ASF_GUID) * 3 + 24);
//      stream_id = flags & 0x7F;
//
//      if (mmsContext->GetStreams()->Count() < MMS_MAX_STREAMS)
//      {
//        MMSStream *stream = new MMSStream();
//        if (stream == NULL)
//        {
//          this->logger->Log(LOGGER_ERROR, L"%s: %s: not enough memory to create stream", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME);
//          return E_OUTOFMEMORY;
//        }
//
//        stream->SetId(stream_id);
//        if (!mmsContext->GetStreams()->Add(stream))
//        {
//          // cannot add MMS stream to MMS context
//          // delete stream and return error
//          delete stream;
//          stream = NULL;
//          this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot add stream to MMS context", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME);
//          return E_FAIL;
//        }
//        else
//        {
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: added stream with id: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME, stream->GetId());
//        }
//      }
//      else
//      {
//        this->logger->Log(LOGGER_ERROR, L"%s: %s: corrupted stream, too many audio/video streams", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME);
//        return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//      }
//    }
//    else if (!memcmp(start, ASF_EXT_STREAM_HEADER, sizeof(ASF_GUID)))
//    {
//      if ((end - start) >= 88)
//      {
//        int stream_count = AV_RL16(start + 84);
//        int ext_len_count = AV_RL16(start + 86);
//        uint64_t skip_bytes = 88;
//
//        while (stream_count--)
//        {
//          if ((end - start) < (skip_bytes + 4))
//          {
//            this->logger->Log(LOGGER_ERROR, L"%s: %s: corrupted stream, next stream name length is not in the buffer", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME);
//            return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//          }
//          skip_bytes += 4 + AV_RL16(start + skip_bytes + 2);
//        }
//
//        while (ext_len_count--)
//        {
//          if ((end - start) < (skip_bytes + 22))
//          {
//            this->logger->Log(LOGGER_ERROR, L"%s: %s: corrupted stream, next extension system info length is not in the buffer", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME);
//            return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//          }
//          skip_bytes += 22 + AV_RL32(start + skip_bytes + 18);
//        }
//
//        if ((end - start) < skip_bytes)
//        {
//          this->logger->Log(LOGGER_ERROR, L"%s: %s: corrupted stream, the last extension system info length is invalid", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_MMS_ASF_HEADER_NAME);
//          return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//        }
//
//        if (chunksize - skip_bytes > 24)
//        {
//          chunksize = skip_bytes;
//        }
//      }
//    }
//    else if (!memcmp(start, ASF_HEAD1_GUID, sizeof(ASF_GUID)))
//    {
//      chunksize = 46; // see references [2] section 3.4. This should be set 46.
//    }
//
//    start += chunksize;
//  }
//
//  return S_OK;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Mms::GetMmsHeaderData(MMSContext *mmsContext, MMSChunk *mmsChunk)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_HEADER_DATA_NAME);
//
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, mmsContext);
//
//  if (SUCCEEDED(result))
//  {
//    result = mmsContext->IsValid() ? S_OK : E_INVALIDARG;
//  }
//
//  if (SUCCEEDED(result))
//  {
//    bool finish = false;
//    DWORD startTime = GetTickCount();
//    MMSChunk *tempMmsChunk = new MMSChunk(mmsChunk);
//    result = (tempMmsChunk != NULL) ? S_OK : E_OUTOFMEMORY;
//
//    while (SUCCEEDED(result) && (!finish))
//    {
//      // fill data to buffer
//      if (SUCCEEDED(result))
//      {
//        CLockMutex(this->lockCurlMutex, INFINITE);
//
//        unsigned int bytesRead = this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
//        if (bytesRead > 0)
//        {
//          result = (this->mmsContext->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()) == bytesRead) ? S_OK : E_OUTOFMEMORY;
//          this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(bytesRead);
//        }
//      }
//
//      if (tempMmsChunk->IsCleared())
//      {
//        result = this->GetMmsChunk(mmsContext, tempMmsChunk);
//      }
//
//      if (SUCCEEDED(result) || (result == HRESULT_FROM_WIN32(ERROR_INVALID_DATA)))
//      {
//        switch (tempMmsChunk->GetChunkType())
//        {
//        case CHUNK_TYPE_END:
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: chunk type: CHUNK_TYPE_END, length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_HEADER_DATA_NAME, tempMmsChunk->GetChunkDataLength());
//          break;
//        case CHUNK_TYPE_STREAM_CHANGE:
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: chunk type: CHUNK_TYPE_STREAM_CHANGE, length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_HEADER_DATA_NAME, tempMmsChunk->GetChunkDataLength());
//          break;
//        case CHUNK_TYPE_ASF_HEADER:
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: chunk type: CHUNK_TYPE_ASF_HEADER, length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_HEADER_DATA_NAME, tempMmsChunk->GetChunkDataLength());
//          break;
//        case CHUNK_TYPE_DATA:
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: chunk type: CHUNK_TYPE_DATA, length: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_HEADER_DATA_NAME, tempMmsChunk->GetChunkDataLength());
//          break;
//        }
//
//        switch(tempMmsChunk->GetChunkType())
//        {
//        case CHUNK_TYPE_ASF_HEADER:
//          // get asf header and stored it
//          {
//            if (!mmsContext->GetHeaderParsed())
//            {
//              result = this->ParseMmsAsfHeader(this->mmsContext, tempMmsChunk);
//
//              if (SUCCEEDED(result))
//              {
//                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: ASF header successfully parsed", PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_HEADER_DATA_NAME);
//                mmsContext->SetHeaderParsed(true);
//                if (!mmsContext->SetAsfHeader(tempMmsChunk->GetChunkData(), tempMmsChunk->GetChunkDataLength()))
//                {
//                  result = E_OUTOFMEMORY;
//                }
//                finish = true;
//              }
//            }
//            else
//            {
//              finish = true;
//            }
//          }
//          break;
//
//        case CHUNK_TYPE_DATA:
//          finish = true;
//          break;
//
//        default:
//          result = S_OK;
//          break;
//        }
//      }
//
//      tempMmsChunk->Clear();
//
//      // test for HRESULT_FROM_WIN32(ERROR_NO_DATA)
//      if (result == HRESULT_FROM_WIN32(ERROR_NO_DATA))
//      {
//        if (GetTickCount() < mmsContext->GetFinishTime())
//        {
//          // reset error, wait some time
//          result = S_OK;
//          Sleep(1);
//        }
//      }
//    }
//
//    // free MMS chunk
//    FREE_MEM_CLASS(tempMmsChunk);
//  }
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_MMS_HEADER_DATA_NAME, result);
//  return result;
//}