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

#include "StdAfx.h"

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "ParserPlugin.h"
#include "ParserPluginConfiguration.h"

#pragma warning(pop)

CParserPlugin::CParserPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CPlugin(result, logger, configuration)
{
  this->logger = NULL;
  this->configuration = NULL;
  this->protocolHoster = NULL;
  this->parserResult = PARSER_RESULT_PENDING;
  this->connectionParameters = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger = new CLogger(result, logger);
    this->configuration = new CParameterCollection(result);
    this->connectionParameters = new CParameterCollection(result);

    CHECK_POINTER_HRESULT(*result, this->logger, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->connectionParameters, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);
  }
}

CParserPlugin::~CParserPlugin(void)
{
  FREE_MEM_CLASS(this->connectionParameters);
  FREE_MEM_CLASS(this->configuration);
  FREE_MEM_CLASS(this->logger);
}

// CPlugin

GUID CParserPlugin::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CParserPlugin::Initialize(CPluginConfiguration *configuration)
{
  CParserPluginConfiguration *parserConfiguration = dynamic_cast<CParserPluginConfiguration *>(configuration);
  HRESULT result = ((this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, parserConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->protocolHoster = parserConfiguration->GetProtocolHoster();
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(parserConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);
  }

  return result;
}

// IDemuxerOwner interface

int64_t CParserPlugin::GetDuration(void)
{
  return this->protocolHoster->GetDuration();
}

HRESULT CParserPlugin::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  return this->protocolHoster->ProcessStreamPackage(streamPackage);
}

HRESULT CParserPlugin::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return this->protocolHoster->QueryStreamProgress(streamProgress);
}

// IProtocol interface

HRESULT CParserPlugin::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  CHECK_CONDITION_HRESULT(result, parameters->Append(this->connectionParameters), result, E_OUTOFMEMORY);

  return result;
}

ProtocolConnectionState CParserPlugin::GetConnectionState(void)
{
  return None;
}

HRESULT CParserPlugin::ParseUrl(const CParameterCollection *parameters)
{
  return S_OK;
}

HRESULT CParserPlugin::ReceiveData(CStreamPackage *streamPackage)
{
  return S_OK;
}

// ISimpleProtocol interface

unsigned int CParserPlugin::GetOpenConnectionTimeout(void)
{
  return this->protocolHoster->GetOpenConnectionTimeout();
}

unsigned int CParserPlugin::GetOpenConnectionSleepTime(void)
{
  return this->protocolHoster->GetOpenConnectionSleepTime();
}

unsigned int CParserPlugin::GetTotalReopenConnectionTimeout(void)
{
  return this->protocolHoster->GetTotalReopenConnectionTimeout();
}

HRESULT CParserPlugin::StartReceivingData(CParameterCollection *parameters)
{
  return S_OK;
}

HRESULT CParserPlugin::StopReceivingData(void)
{
  return S_OK;
}

void CParserPlugin::ClearSession(void)
{
  CPlugin::ClearSession();

  this->parserResult = PARSER_RESULT_PENDING;
  this->connectionParameters->Clear();
}

void CParserPlugin::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  this->protocolHoster->ReportStreamTime(streamTime, streamPosition);
}

HRESULT CParserPlugin::GetStreamInformation(CStreamInformationCollection *streams)
{
  return this->protocolHoster->GetStreamInformation(streams);
}

// ISeeking interface

unsigned int CParserPlugin::GetSeekingCapabilities(void)
{
  return this->protocolHoster->GetSeekingCapabilities();
}

/* get methods */

HRESULT CParserPlugin::GetParserResult(void)
{
  return this->parserResult;
}

int64_t CParserPlugin::SeekToTime(unsigned int streamId, int64_t time)
{
  return this->protocolHoster->SeekToTime(streamId, time);
}

void CParserPlugin::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->protocolHoster->SetPauseSeekStopMode(pauseSeekStopMode);
}

/* set methods */

HRESULT CParserPlugin::SetConnectionParameters(const CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  CHECK_CONDITION_HRESULT(result, this->connectionParameters->Append((CParameterCollection *)parameters), result, E_OUTOFMEMORY);

  return result;
}

/* other methods */

/* protected methods */
