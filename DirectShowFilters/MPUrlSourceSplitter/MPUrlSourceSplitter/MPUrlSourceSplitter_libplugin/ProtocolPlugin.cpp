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

#include "ProtocolPlugin.h"

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

// IProtocol interface implementation

// ISimpleProtocol interface implementation

HRESULT CProtocolPlugin::ClearSession(void)
{
  this->flags = PROTOCOL_PLUGIN_FLAG_NONE;
  this->reportedStreamTime = 0;
  this->reportedStreamPosition = 0;
  this->pauseSeekStopMode = PAUSE_SEEK_STOP_MODE_NONE;

  return S_OK;
}

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

/* protected methods */
