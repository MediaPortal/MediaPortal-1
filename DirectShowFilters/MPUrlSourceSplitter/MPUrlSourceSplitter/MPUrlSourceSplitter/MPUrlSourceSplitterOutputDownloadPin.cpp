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

#include "MPUrlSourceSplitterOutputDownloadPin.h"

#ifdef _DEBUG
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputDownloadPind"
#else
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputDownloadPin"
#endif

CMPUrlSourceSplitterOutputDownloadPin::CMPUrlSourceSplitterOutputDownloadPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes, const wchar_t *downloadFileName)
  : CMPUrlSourceSplitterOutputPin(pName, pFilter, pLock, phr, logger, parameters, mediaTypes)
{
  this->inputPin = NULL;
  this->downloadResult = S_OK;

  if (phr != NULL)
  {
    if (SUCCEEDED(*phr))
    {
      this->inputPin = new CMPUrlSourceSplitterInputDownloadPin(logger, phr, this->m_pName, downloadFileName, this);
      CHECK_POINTER_HRESULT(*phr, this->inputPin, *phr, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(*phr))
    {
      // increase reference count to input pin to avoid freeing it from memory
      // we free input pin in our destructor
      this->inputPin->AddRef();

      // try to connect output pin with input pin
      *phr = this->Connect(this->inputPin, NULL);
    }
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, this->m_pName));
}

CMPUrlSourceSplitterOutputDownloadPin::~CMPUrlSourceSplitterOutputDownloadPin(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));

  FREE_MEM_CLASS(this->inputPin);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));
}

/* get methods */

HRESULT CMPUrlSourceSplitterOutputDownloadPin::GetDownloadResult(void)
{
  return this->downloadResult;
}

/* set methods */

/* other methods */

void CMPUrlSourceSplitterOutputDownloadPin::FinishDownload(HRESULT result)
{
  this->flags |= MP_URL_SOURCE_SPLITTER_OUTPUT_DOWNLOAD_PIN_FLAG_DOWNLOAD_FINISHED;
  this->downloadResult = result;
}

bool CMPUrlSourceSplitterOutputDownloadPin::IsDownloadFinished(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_DOWNLOAD_PIN_FLAG_DOWNLOAD_FINISHED);
}

/* protected methods */