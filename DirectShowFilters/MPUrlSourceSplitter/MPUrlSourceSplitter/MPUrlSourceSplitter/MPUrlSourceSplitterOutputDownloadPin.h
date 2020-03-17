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

#pragma once

#ifndef __MP_URL_SOURCE_SPLITTER_OUTPUT_DOWNLOAD_PIN_DEFINED
#define __MP_URL_SOURCE_SPLITTER_OUTPUT_DOWNLOAD_PIN_DEFINED

#include "MPUrlSourceSplitterOutputPin.h"
#include "MPUrlSourceSplitterInputDownloadPin.h"

#define MP_URL_SOURCE_SPLITTER_OUTPUT_DOWNLOAD_PIN_FLAG_NONE               MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_OUTPUT_DOWNLOAD_PIN_FLAG_DOWNLOAD_FINISHED  (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 0))

#define MP_URL_SOURCE_SPLITTER_OUTPUT_DOWNLOAD_PIN_FLAG_LAST               (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 1)

class CMPUrlSourceSplitterOutputDownloadPin : public CMPUrlSourceSplitterOutputPin
{
public:
  CMPUrlSourceSplitterOutputDownloadPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes, const wchar_t *downloadFileName);
  virtual ~CMPUrlSourceSplitterOutputDownloadPin(void);

  // queues output pin packet
  // @param packet : the packet to queue to output pin
  // @param timeout : the timeout in ms to queue to output pin
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueuePacket(COutputPinPacket *packet, DWORD timeout);

  // queues end of stream
  // @param endOfStreamResult : S_OK if normal end of stream, error code otherwise
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueueEndOfStream(HRESULT endOfStreamResult);

  /* get methods */

  // gets download result
  // @return : S_OK if successful, error code otherwise
  HRESULT GetDownloadResult(void);

  /* set methods */

  /* other methods */

  // finishes download with specified result
  // @param result : the result of download
  void FinishDownload(HRESULT result);

  // tests if download is finished
  // @return : true if download finished, false otherwise
  bool IsDownloadFinished(void);

protected:

  // holds download input pin
  CMPUrlSourceSplitterInputDownloadPin *inputPin;

  // hodls download result
  HRESULT downloadResult;
};

#endif