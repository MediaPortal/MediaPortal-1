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
#include "ParserPluginConfiguration.h"
#include "StreamPackage.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "StreamInformationCollection.h"
#include "Parameters.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "TsPacket.h"

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_Mpeg2TSd"
#else
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_Mpeg2TS"
#endif

// 32 KB of data to request at start
#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DATA_LENGTH_DEFAULT             32768

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
  this->continuousStreamRanges = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->continuousStreamRanges = new CContinuousStreamRangeCollection(result);
    CHECK_POINTER_HRESULT(*result, this->continuousStreamRanges, *result, E_OUTOFMEMORY);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Parser_Mpeg2TS::~CMPUrlSourceSplitter_Parser_Mpeg2TS()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->continuousStreamRanges);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CParserPlugin

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::GetParserResult(void)
{
  //if (this->parserResult == PARSER_RESULT_PENDING)
  //{
  //  CStreamInformationCollection *streams = new CStreamInformationCollection(&this->parserResult);
  //  CHECK_POINTER_HRESULT(this->parserResult, streams, this->parserResult, E_OUTOFMEMORY);

  //  CHECK_HRESULT_EXECUTE(this->parserResult, this->protocolHoster->GetStreamInformation(streams));

  //  if (SUCCEEDED(this->parserResult) && (streams->Count() == 1))
  //  {
  //    CStreamPackage *package = new CStreamPackage(&this->parserResult);
  //    CHECK_POINTER_HRESULT(this->parserResult, package, this->parserResult, E_OUTOFMEMORY);

  //    if (SUCCEEDED(this->parserResult))
  //    {
  //      unsigned int requestLength = MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DATA_LENGTH_DEFAULT;
  //      bool receivedSameLength = false;
  //      this->parserResult = PARSER_RESULT_PENDING;

  //      while ((this->parserResult == PARSER_RESULT_PENDING) && (!receivedSameLength))
  //      {
  //        package->Clear();

  //        CStreamPackageDataRequest *request = new CStreamPackageDataRequest(&this->parserResult);
  //        CHECK_POINTER_HRESULT(this->parserResult, request, this->parserResult, E_OUTOFMEMORY);

  //        if (SUCCEEDED(this->parserResult))
  //        {
  //          request->SetStart(0);
  //          request->SetLength(requestLength);
  //          request->SetAnyDataLength(true);

  //          package->SetRequest(request);
  //        }

  //        CHECK_CONDITION_EXECUTE(FAILED(this->parserResult), FREE_MEM_CLASS(request));
  //        CHECK_HRESULT_EXECUTE(this->parserResult, this->protocolHoster->ProcessStreamPackage(package));

  //        if (SUCCEEDED(this->parserResult))
  //        {
  //          this->parserResult = PARSER_RESULT_PENDING;
  //          CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(package->GetResponse());

  //          if (package->IsError())
  //          {
  //            // TO DO: check type of error

  //            this->parserResult = PARSER_RESULT_NOT_KNOWN;
  //          }

  //          if (response != NULL)
  //          {
  //            receivedSameLength = (response->GetBuffer()->GetBufferOccupiedSpace() == this->lastReceivedLength);
  //            if (!receivedSameLength)
  //            {
  //              // try parse data
  //              int res = CTsPacket::FindPacket(response->GetBuffer(), TS_PACKET_MINIMUM_CHECKED_UNSPECIFIED);

  //              switch (res)
  //              {
  //              case TS_PACKET_FIND_RESULT_NOT_FOUND:
  //                this->parserResult = PARSER_RESULT_NOT_KNOWN;
  //                break;
  //              case TS_PACKET_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER:
  //              case TS_PACKET_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS:
  //                this->parserResult = PARSER_RESULT_PENDING;
  //                break;
  //              case TS_PACKET_FIND_RESULT_NOT_ENOUGH_MEMORY:
  //                this->parserResult = E_OUTOFMEMORY;
  //                break;
  //              default:
  //                // we found at least TS_PACKET_MINIMUM_CHECKED MPEG2 TS packets
  //                this->parserResult = PARSER_RESULT_KNOWN;
  //                break;
  //              }

  //              requestLength *= 2;
  //            }

  //            this->lastReceivedLength = response->GetBuffer()->GetBufferOccupiedSpace();
  //          }
  //        }
  //      }
  //    }

  //    FREE_MEM_CLASS(package);
  //  }
  //  else
  //  {
  //    // MPEG2 TS parser doesn't support multiple streams
  //    this->parserResult = PARSER_RESULT_NOT_KNOWN;
  //  }

  //  FREE_MEM_CLASS(streams);
  //}

  this->parserResult = PARSER_RESULT_NOT_KNOWN;

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

// IDemuxerOwner interface

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  //// in rare cases (e.g. HTTP protocol) are MPEG2 TS streams not aligned to MPEG2 TS packet boundary (sync byte)
  //// in that case is FFmpeg (and maybe also TvService) confused
  //// we must align MPEG2 TS stream to MPEG2 TS packet boundary (sync byte)

  //// because we can skip some TS packets, we must maintain position index for requests from filter to protocol
  //// anyway, filter usually asks for 32768 bytes of data, but aligned TS packets are only for 32712 bytes (we have 56 bytes from last MPEG2 TS packet, 132 bytes are missing)
  //// in that case we must handle such TS packet and not align (or better - align after missing 132 bytes)

  //HRESULT result = S_OK;
  //CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  //if (SUCCEEDED(result))
  //{
  //  unsigned int receivedData = 0;
  //  CStreamPackageDataRequest *dataRequest = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
  //  CHECK_POINTER_HRESULT(result, dataRequest, result, E_INVALID_STREAM_PACKAGE_REQUEST);

  //  if (SUCCEEDED(result))
  //  {
  //    // TO DO: get correct start position
  //    int64_t filterStartPosition = dataRequest->GetStart();
  //    int64_t protocolStartPosition = filterStartPosition;
  //    int64_t streamLength = dataRequest->GetLength();

  //    while (SUCCEEDED(result) && SUCCEEDED(streamPackage->GetError()) && (streamLength > 0))
  //    {
  //      // check continuous stream range collection for correct protocol stream range
  //      unsigned int startIndex = 0;
  //      unsigned int endIndex = 0;

  //      // request length can be larger than original streamLength, we align request length to boundary of MPEG2 TS packet
  //      int64_t requestLength = streamLength / 2;

  //      CHECK_CONDITION_HRESULT(result, this->continuousStreamRanges->GetItemInsertPosition(protocolStartPosition, &startIndex, &endIndex), result, E_NOT_VALID_STATE);
  //      if (SUCCEEDED(result))
  //      {
  //        // requested filter stream start position is between startIndex and endIndex
  //        // if startIndex equal to endIndex then it is exactly in startIndex

  //        endIndex = min(endIndex, this->continuousStreamRanges->Count());
  //      }

  //      requestLength = max(TS_PACKET_SIZE, requestLength);
  //      if ((requestLength % TS_PACKET_SIZE) != 0)
  //      {
  //        requestLength = ((requestLength / TS_PACKET_SIZE) + 1) * TS_PACKET_SIZE;
  //      }

  //      // we need to request missing data

  //      CStreamPackage *tempPackage = streamPackage->Clone();
  //      CHECK_POINTER_HRESULT(result, tempPackage, result, E_OUTOFMEMORY);

  //      if (SUCCEEDED(result))
  //      {
  //        CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(tempPackage->GetRequest());
  //        CHECK_POINTER_HRESULT(result, request, result, E_INVALID_STREAM_PACKAGE_REQUEST);

  //        if (SUCCEEDED(result))
  //        {
  //          request->SetStart(protocolStartPosition);
  //          request->SetLength((unsigned int)requestLength);
  //        }

  //        CHECK_HRESULT_EXECUTE(result, this->protocolHoster->ProcessStreamPackage(tempPackage));
  //        CHECK_CONDITION_EXECUTE(FAILED(tempPackage->GetError()), streamPackage->SetCompleted(tempPackage->GetError()));
  //        
  //        if (SUCCEEDED(result) && (SUCCEEDED(tempPackage->GetError())))
  //        {
  //          // process data response
  //          CStreamPackageDataResponse *tempDataResponse = dynamic_cast<CStreamPackageDataResponse *>(tempPackage->GetResponse());
  //          CHECK_CONDITION_HRESULT(result, tempDataResponse, result, E_INVALID_STREAM_PACKAGE_RESPONSE);

  //          if (SUCCEEDED(result))
  //          {
  //            if (streamPackage->GetResponse() == NULL)
  //            {
  //              // create response based on dataResponse
  //              // we don't need data, because we make sync to sync byte

  //              CStreamPackageDataResponse *response = (CStreamPackageDataResponse *)tempDataResponse->Clone();
  //              CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  //              if (SUCCEEDED(result))
  //              {
  //                response->GetBuffer()->ClearBuffer();
  //                streamPackage->SetResponse(response);
  //              }

  //              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  //            }

  //            unsigned int firstPacketPosition = 0;
  //            unsigned int packetSequenceLength = 0;

  //            result = CTsPacket::FindPacketSequence(tempDataResponse->GetBuffer(), &firstPacketPosition, &packetSequenceLength);

  //            if (SUCCEEDED(result))
  //            {
  //              if (packetSequenceLength != 0)
  //              {
  //                // add received protocol stream range to continuous stream range collection
  //                this->continuousStreamRanges->AddContinuousStreamRange(filterStartPosition, protocolStartPosition + (int64_t)firstPacketPosition, (int64_t)packetSequenceLength, 0);

  //                CStreamPackageDataResponse *dataResponse = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

  //                if (((firstPacketPosition == 0) && (dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0)) ||
  //                    (dataResponse->GetBuffer()->GetBufferOccupiedSpace() == 0))
  //                {
  //                  // in case that we have some data in filter respose, we must be aligned to start of request
  //                  // in another case set discontinuity and return

  //                  unsigned int copyDataLength = min(packetSequenceLength, (unsigned int)streamLength);
  //                  CHECK_CONDITION_HRESULT(result, dataResponse->GetBuffer()->AddToBufferWithResize(tempDataResponse->GetBuffer(), firstPacketPosition, copyDataLength) == copyDataLength, result, E_OUTOFMEMORY);

  //                  filterStartPosition += packetSequenceLength;
  //                  protocolStartPosition += firstPacketPosition + packetSequenceLength;
  //                  receivedData += packetSequenceLength;
  //                  streamLength -= packetSequenceLength;
  //                }
  //                else
  //                {
  //                  dataResponse->SetDiscontinuity(true);
  //                  // finish request and response, return to filter
  //                  streamLength = 0;
  //                }
  //              }
  //            }
  //          }
  //        }
  //      }

  //      FREE_MEM_CLASS(tempPackage);
  //    }
  //  }
  //}

  //return result;

  return E_NOTIMPL;
}

// ISimpleProtocol interface

void CMPUrlSourceSplitter_Parser_Mpeg2TS::ClearSession(void)
{
  __super::ClearSession();

  this->lastReceivedLength = 0;
  this->continuousStreamRanges->Clear();
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