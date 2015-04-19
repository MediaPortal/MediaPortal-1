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

#include "ProtocolPlugin.h"
#include "Parameters.h"

#include <Shlwapi.h>

#pragma warning(pop)

CProtocolPlugin::CProtocolPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CPlugin(result, logger, configuration)
{
  this->logger = NULL;
  this->configuration = NULL;
  this->reportedStreamTime = 0;
  this->reportedStreamPosition = 0;
  this->pauseSeekStopMode = PAUSE_SEEK_STOP_MODE_NONE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger = new CLogger(result, logger);
    this->configuration = new CParameterCollection(result);

    CHECK_POINTER_HRESULT(*result, this->logger, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);
  }
}

CProtocolPlugin::~CProtocolPlugin(void)
{
  FREE_MEM_CLASS(this->configuration);
  FREE_MEM_CLASS(this->logger);
}

// CPlugin

HRESULT CProtocolPlugin::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = ((this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, configuration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(configuration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT) ? PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED : PROTOCOL_PLUGIN_FLAG_NONE;
    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_SPLITTER, true, PARAMETER_NAME_SPLITTER_DEFAULT) ? PLUGIN_FLAG_SPLITTER : PROTOCOL_PLUGIN_FLAG_NONE;
    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_IPTV, true, PARAMETER_NAME_IPTV_DEFAULT) ? PLUGIN_FLAG_IPTV : PROTOCOL_PLUGIN_FLAG_NONE;

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA, true, PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA_DEFAULT) ? PROTOCOL_PLUGIN_FLAG_DUMP_INPUT_DATA : PROTOCOL_PLUGIN_FLAG_NONE;
    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_DUMP_PROTOCOL_OUTPUT_DATA, true, PARAMETER_NAME_DUMP_PROTOCOL_OUTPUT_DATA_DEFAULT) ? PROTOCOL_PLUGIN_FLAG_DUMP_OUTPUT_DATA : PROTOCOL_PLUGIN_FLAG_NONE;
  }

  return result;
}

void CProtocolPlugin::ClearSession(void)
{
  CPlugin::ClearSession();

  this->reportedStreamTime = 0;
  this->reportedStreamPosition = 0;
  this->pauseSeekStopMode = PAUSE_SEEK_STOP_MODE_NONE;
}

// IProtocol interface implementation

// ISimpleProtocol interface implementation

void CProtocolPlugin::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  this->reportedStreamTime = streamTime;
  this->reportedStreamPosition = streamPosition;
}

// ISeeking interface implementation

void CProtocolPlugin::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->pauseSeekStopMode = pauseSeekStopMode;
}

/* get methods */

/* set methods */

/* other methods */

bool CProtocolPlugin::IsLiveStreamSpecified(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED);
}

bool CProtocolPlugin::IsLiveStreamDetected(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED);
}

bool CProtocolPlugin::IsLiveStream(void)
{
  return (this->IsLiveStreamSpecified() || this->IsLiveStreamDetected());
}

bool CProtocolPlugin::IsSetStreamLength(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH);
}

bool CProtocolPlugin::IsStreamLengthEstimated(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED);
}

bool CProtocolPlugin::IsWholeStreamDownloaded(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED);
}

bool CProtocolPlugin::IsEndOfStreamReached(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED);
}

bool CProtocolPlugin::IsConnectionLostCannotReopen(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_CONNECTION_LOST_CANNOT_REOPEN);
}

bool CProtocolPlugin::IsDumpInputData(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_DUMP_INPUT_DATA);
}

bool CProtocolPlugin::IsDumpOutputData(void)
{
  return this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_DUMP_OUTPUT_DATA);
}

/* protected methods */

wchar_t *CProtocolPlugin::GetCacheFile(const wchar_t *extra)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());

    if (guid != NULL)
    {
      if (IsNullOrEmpty(extra))
      {
        result = FormatString(L"%s%s_%s.temp", folder, this->GetStoreFileNamePart(), guid);
      }
      else
      {
        result = FormatString(L"%s%s_%s_%s.temp", folder, this->GetStoreFileNamePart(), guid, extra);
      }
    }

    FREE_MEM(guid);
  }

  return result;
}

wchar_t *CProtocolPlugin::GetDumpFile(const wchar_t *extra)
{
  wchar_t *result = NULL;
  wchar_t *folder = Duplicate(this->configuration->GetValue(PARAMETER_NAME_LOG_FILE_NAME, true, NULL));

  if (folder != NULL)
  {
    PathRemoveFileSpec(folder);

    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      if (IsNullOrEmpty(extra))
      {
        result = FormatString(L"%s\\%s_%s.dump", folder, this->GetStoreFileNamePart(), guid);
      }
      else
      {
        result = FormatString(L"%s\\%s_%s_%s.dump", folder, this->GetStoreFileNamePart(), guid, extra);
      }
    }
    FREE_MEM(guid);
  }

  FREE_MEM(folder);

  return result;
}

wchar_t *CProtocolPlugin::GetDumpFile(void)
{
  return this->GetDumpFile(NULL);
}