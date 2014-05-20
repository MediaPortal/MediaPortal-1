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
  this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME, this);

  this->outputStream = outputStream;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
}

COutputStreamHoster::~COutputStreamHoster(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_OUTPUT_STREAM_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);
}

// IOutputStream interface

HRESULT COutputStreamHoster::SetStreamCount(unsigned int streamCount, bool liveStream)
{
  if (this->outputStream != NULL)
  {
    return this->outputStream->SetStreamCount(streamCount, liveStream);
  }

  return E_NOT_VALID_STATE;
}

HRESULT COutputStreamHoster::PushStreamReceiveData(unsigned int streamId, CStreamReceiveData *streamReceiveData)
{
  if (this->outputStream != NULL)
  {
    return this->outputStream->PushStreamReceiveData(streamId, streamReceiveData);
  }

  return E_NOT_VALID_STATE;
}