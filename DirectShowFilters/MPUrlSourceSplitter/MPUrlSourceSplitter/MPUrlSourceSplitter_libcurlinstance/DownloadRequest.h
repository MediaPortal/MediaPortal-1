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

#ifndef __DOWNLOAD_REQUEST_DEFINED
#define __DOWNLOAD_REQUEST_DEFINED

#include "Flags.h"

#define DOWNLOAD_REQUEST_FLAG_NONE                                    FLAGS_NONE

#define DOWNLOAD_REQUEST_FLAG_LAST                                    (FLAGS_LAST + 0)

class CDownloadRequest : public CFlags
{
public:
  CDownloadRequest(HRESULT *result);
  virtual ~CDownloadRequest(void);

  /* get methods */

  // gets url to download
  // @return : url to download
  virtual const wchar_t *GetUrl(void);

  /* set methods */

  // sets url to download
  // @param url : url to download
  // @return : true if successful, false otherwise
  virtual bool SetUrl(const wchar_t *url);

  /* other methods*/

  // deeply clones current instance
  // @result : deep clone of current instance or NULL if error
  virtual CDownloadRequest *Clone(void);

protected:
  // holds url to download
  wchar_t *url;

  /* methods */

  // creates empty download request
  // @return : download request or NULL if error
  virtual CDownloadRequest *CreateDownloadRequest(void);

  // deeply clones current instance to cloned request
  // @param  clone : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadRequest *clone);
};

#endif