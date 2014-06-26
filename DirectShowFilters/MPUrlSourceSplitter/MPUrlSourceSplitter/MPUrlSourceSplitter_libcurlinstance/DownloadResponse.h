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

#ifndef __DOWNLOAD_RESPONSE_DEFINED
#define __DOWNLOAD_RESPONSE_DEFINED

#include "LinearBuffer.h"
#include "Flags.h"

#include <curl/curl.h>

#define DOWNLOAD_RESPONSE_FLAG_NONE                                   FLAGS_NONE

#define DOWNLOAD_RESPONSE_FLAG_LAST                                   (FLAGS_LAST + 0)

class CDownloadResponse : public CFlags
{
public:
  CDownloadResponse(HRESULT *result);
  virtual ~CDownloadResponse(void);

  /* get methods */

  // gets received data
  // @return : received data
  virtual CLinearBuffer *GetReceivedData(void);

  // gets result error
  // @return : result error (S_OK if none)
  virtual HRESULT GetResultError(void);

  /* set methods */

  // sets result error
  // @param resultError : result error to set
  virtual void SetResultError(HRESULT resultError);

  /* other methods */

  // deeply clones current instance
  // @result : deep clone of current instance or NULL if error
  virtual CDownloadResponse *Clone(void);

protected:
  // holds received data
  CLinearBuffer *receivedData;

  // holds result error
  HRESULT resultError;

  /* methods */

  // creates download response
  // @return : download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // deeply clones current instance to cloned response
  // @param  clone : cloned response to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadResponse *clone);
};

#endif