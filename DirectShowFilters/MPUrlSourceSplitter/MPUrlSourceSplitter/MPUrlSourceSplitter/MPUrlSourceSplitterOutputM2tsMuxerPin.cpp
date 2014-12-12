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

#include "MPUrlSourceSplitterOutputM2tsMuxerPin.h"

#ifdef _DEBUG
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputM2tsMuxerPind"
#else
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputM2tsMuxerPin"
#endif

CMPUrlSourceSplitterOutputM2tsMuxerPin::CMPUrlSourceSplitterOutputM2tsMuxerPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes)
  : CMPUrlSourceSplitterOutputPin(pName, pFilter, pLock, phr, logger, parameters, mediaTypes)
{
  if ((phr != NULL) && (SUCCEEDED(*phr)))
  {
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, this->m_pName));
}

CMPUrlSourceSplitterOutputM2tsMuxerPin::~CMPUrlSourceSplitterOutputM2tsMuxerPin(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));

  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));
}

/* get methods */

/* set methods */

HRESULT CMPUrlSourceSplitterOutputM2tsMuxerPin::SetVideoStreams(unsigned int demuxerId, CStreamCollection *streams)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streams);

  return result;
}

HRESULT CMPUrlSourceSplitterOutputM2tsMuxerPin::SetAudioStreams(unsigned int demuxerId, CStreamCollection *streams)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streams);

  return result;
}

HRESULT CMPUrlSourceSplitterOutputM2tsMuxerPin::SetSubtitleStreams(unsigned int demuxerId, CStreamCollection *streams)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streams);

  return result;
}

/* other methods */

HRESULT CMPUrlSourceSplitterOutputM2tsMuxerPin::QueuePacket(COutputPinPacket *packet, DWORD timeout)
{
  FREE_MEM_CLASS(packet);

  return S_OK;
}

HRESULT CMPUrlSourceSplitterOutputM2tsMuxerPin::QueueEndOfStream(HRESULT endOfStreamResult)
{
  //return __super::QueueEndOfStream(endOfStreamResult);

  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME, this->m_pName);

  COutputPinPacket *endOfStream = new COutputPinPacket(&result);
  CHECK_POINTER_HRESULT(result, endOfStream, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    endOfStream->SetEndOfStream(true, endOfStreamResult);

    result = __super::QueuePacket(endOfStream, INFINITE);
  }

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_END_OF_STREAM);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(endOfStream));

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME, this->m_pName, result);
  return result;
}

/* protected methods */