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

#include "MPUrlSourceSplitter_Parser_Default.h"
#include "VersionInfo.h"
#include "Parameters.h"
#include "ParserPluginConfiguration.h"

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                      L"MPUrlSourceSplitter_Parser_Defaultd"
#else
#define PARSER_IMPLEMENTATION_NAME                                      L"MPUrlSourceSplitter_Parser_Default"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Parser_Default(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Parser_Default *parserPlugin = (CMPUrlSourceSplitter_Parser_Default *)plugin;

    delete parserPlugin;
  }
}

CMPUrlSourceSplitter_Parser_Default::CMPUrlSourceSplitter_Parser_Default(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CParserPlugin(result, logger, configuration)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);
    this->parserResult = Known;

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_DEFAULT, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_DEFAULT);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Parser_Default::~CMPUrlSourceSplitter_Parser_Default()
{
}

// CParserPlugin

unsigned int CMPUrlSourceSplitter_Parser_Default::GetParserScore(void)
{
  // return lowest possible score
  return 1;
}

CParserPlugin::Action CMPUrlSourceSplitter_Parser_Default::GetAction(void)
{
  return ParseStream;
}

// CPlugin

const wchar_t *CMPUrlSourceSplitter_Parser_Default::GetName(void)
{
  return PARSER_NAME;
}

GUID CMPUrlSourceSplitter_Parser_Default::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Parser_Default::Initialize(CPluginConfiguration *configuration)
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

unsigned int CMPUrlSourceSplitter_Parser_Default::GetSeekingCapabilities(void)
{
  return this->protocolHoster->GetSeekingCapabilities();
}

int64_t CMPUrlSourceSplitter_Parser_Default::SeekToTime(unsigned int streamId, int64_t time)
{
  return this->protocolHoster->SeekToTime(streamId, time);
}

void CMPUrlSourceSplitter_Parser_Default::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->protocolHoster->SetPauseSeekStopMode(pauseSeekStopMode);
}

// IDemuxerOwner interface

int64_t CMPUrlSourceSplitter_Parser_Default::GetDuration(void)
{
  return this->protocolHoster->GetDuration();
}

HRESULT CMPUrlSourceSplitter_Parser_Default::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  return this->protocolHoster->ProcessStreamPackage(streamPackage);
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Parser_Default::GetOpenConnectionTimeout(void)
{
  return this->protocolHoster->GetOpenConnectionTimeout();
}

unsigned int CMPUrlSourceSplitter_Parser_Default::GetOpenConnectionSleepTime(void)
{
  return this->protocolHoster->GetOpenConnectionSleepTime();
}

unsigned int CMPUrlSourceSplitter_Parser_Default::GetTotalReopenConnectionTimeout(void)
{
  return this->protocolHoster->GetTotalReopenConnectionTimeout();
}

HRESULT CMPUrlSourceSplitter_Parser_Default::StartReceivingData(CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Default::StopReceivingData(void)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Default::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return this->protocolHoster->QueryStreamProgress(streamProgress);
}
  
HRESULT CMPUrlSourceSplitter_Parser_Default::ClearSession(void)
{
  HRESULT result = __super::ClearSession();

  this->parserResult = Known;

  return result;
}

void CMPUrlSourceSplitter_Parser_Default::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  this->protocolHoster->ReportStreamTime(streamTime, streamPosition);
}

HRESULT CMPUrlSourceSplitter_Parser_Default::GetStreamInformation(CStreamInformationCollection *streams)
{
  return this->protocolHoster->GetStreamInformation(streams);
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Parser_Default::GetConnectionState(void)
{
  return None;
}

HRESULT CMPUrlSourceSplitter_Parser_Default::ParseUrl(const CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Default::ReceiveData(CStreamPackage *streamPackage)
{
  return E_NOTIMPL;
}