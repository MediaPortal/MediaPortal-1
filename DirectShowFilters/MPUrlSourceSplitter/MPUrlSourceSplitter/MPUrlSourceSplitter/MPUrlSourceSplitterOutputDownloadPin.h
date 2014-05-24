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

#define OUTPUT_DOWNLOAD_PIN_FLAG_NONE                                 OUTPUT_PIN_FLAG_NONE

#define OUTPUT_SPLITTER_PIN_FLAG_LAST                                 (OUTPUT_PIN_FLAG_LAST + 0)

class CMPUrlSourceSplitterOutputDownloadPin : public CMPUrlSourceSplitterOutputPin
{
public:
  CMPUrlSourceSplitterOutputDownloadPin(CLogger *logger, CMediaTypeCollection *mediaTypes, LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, const wchar_t *downloadFileName);
  virtual ~CMPUrlSourceSplitterOutputDownloadPin(void);

  // finishes download with specified result
  // @param result : the result of download
  void FinishDownload(HRESULT result);

protected:

  // holds download input pin
  CMPUrlSourceSplitterInputDownloadPin *inputPin;
};

#endif