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

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Parser_Mpeg2TS::~CMPUrlSourceSplitter_Parser_Mpeg2TS()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CParserPlugin

#include <stdio.h>

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::GetParserResult(void)
{
  if (this->parserResult == PARSER_RESULT_PENDING)
  {
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

GUID CMPUrlSourceSplitter_Parser_Mpeg2TS::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
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

unsigned int CMPUrlSourceSplitter_Parser_Mpeg2TS::GetSeekingCapabilities(void)
{
  return this->protocolHoster->GetSeekingCapabilities();
}

int64_t CMPUrlSourceSplitter_Parser_Mpeg2TS::SeekToTime(unsigned int streamId, int64_t time)
{
  return this->protocolHoster->SeekToTime(streamId, time);
}

void CMPUrlSourceSplitter_Parser_Mpeg2TS::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->protocolHoster->SetPauseSeekStopMode(pauseSeekStopMode);
}

// IDemuxerOwner interface

int64_t CMPUrlSourceSplitter_Parser_Mpeg2TS::GetDuration(void)
{
  return this->protocolHoster->GetDuration();
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  // in rare cases (e.g. HTTP protocol) are MPEG2 TS streams not aligned to MPEG2 TS packet boundary (sync byte)
  // in that case is FFmpeg (and maybe also TvService) confused
  // we must align MPEG2 TS stream to MPEG2 TS packet boundary (sync byte)


  return this->protocolHoster->ProcessStreamPackage(streamPackage);
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Parser_Mpeg2TS::GetOpenConnectionTimeout(void)
{
  return this->protocolHoster->GetOpenConnectionTimeout();
}

unsigned int CMPUrlSourceSplitter_Parser_Mpeg2TS::GetOpenConnectionSleepTime(void)
{
  return this->protocolHoster->GetOpenConnectionSleepTime();
}

unsigned int CMPUrlSourceSplitter_Parser_Mpeg2TS::GetTotalReopenConnectionTimeout(void)
{
  return this->protocolHoster->GetTotalReopenConnectionTimeout();
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::StartReceivingData(CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::StopReceivingData(void)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return this->protocolHoster->QueryStreamProgress(streamProgress);
}
  
void CMPUrlSourceSplitter_Parser_Mpeg2TS::ClearSession(void)
{
  __super::ClearSession();

  this->lastReceivedLength = 0;
}

void CMPUrlSourceSplitter_Parser_Mpeg2TS::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  this->protocolHoster->ReportStreamTime(streamTime, streamPosition);
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::GetStreamInformation(CStreamInformationCollection *streams)
{
  return this->protocolHoster->GetStreamInformation(streams);
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Parser_Mpeg2TS::GetConnectionState(void)
{
  return None;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::ParseUrl(const CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Mpeg2TS::ReceiveData(CStreamPackage *streamPackage)
{
  return E_NOTIMPL;
}