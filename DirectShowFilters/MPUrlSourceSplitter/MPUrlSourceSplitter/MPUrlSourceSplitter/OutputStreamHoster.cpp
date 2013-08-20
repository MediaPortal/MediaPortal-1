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

#include "OutputStreamHoster.h"

COutputStreamHoster::COutputStreamHoster(CLogger *logger, CParameterCollection *configuration, const wchar_t *moduleName, const wchar_t *moduleSearchPattern, IOutputStream *outputStream)
  : CHoster(logger, configuration, moduleName, moduleSearchPattern)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);

  this->outputStream = outputStream;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
}

COutputStreamHoster::~COutputStreamHoster(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);
}

// IOutputStream interface
HRESULT COutputStreamHoster::SetTotalLength(int64_t total, bool estimate)
{
  if (this->outputStream != NULL)
  {
    return this->outputStream->SetTotalLength(total, estimate);
  }

  return E_NOT_VALID_STATE;
}

HRESULT COutputStreamHoster::PushMediaPackets(CMediaPacketCollection *mediaPackets)
{
  if (this->outputStream != NULL)
  {
    return this->outputStream->PushMediaPackets(mediaPackets);
  }

  return E_NOT_VALID_STATE;
}

HRESULT COutputStreamHoster::EndOfStreamReached(int64_t streamPosition)
{
  if (this->outputStream != NULL)
  {
    return this->outputStream->EndOfStreamReached(streamPosition);
  }

  return E_NOT_VALID_STATE;
}