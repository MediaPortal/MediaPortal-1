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

#ifndef __STREAM_PACKAGE_DATA_REQUEST_DEFINED
#define __STREAM_PACKAGE_DATA_REQUEST_DEFINED

#include "StreamPackageRequest.h"

#include <stdint.h>

#define STREAM_PACKAGE_DATA_REQUEST_FLAG_NONE                         STREAM_PACKAGE_REQUEST_FLAG_NONE

#define STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH              (1 << (STREAM_PACKAGE_REQUEST_FLAG_LAST + 0))
#define STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH      (1 << (STREAM_PACKAGE_REQUEST_FLAG_LAST + 1))

#define STREAM_PACKAGE_DATA_REQUEST_FLAG_LAST                         (STREAM_PACKAGE_REQUEST_FLAG_LAST + 2)

class CStreamPackageDataRequest : public CStreamPackageRequest
{
public:
  CStreamPackageDataRequest(HRESULT *result);
  virtual ~CStreamPackageDataRequest(void);

  /* get methods */

  // gets request start position
  // @return : request start position
  virtual int64_t GetStart(void);

  // gets request length
  // @return : request length
  virtual unsigned int GetLength(void);

  /* set methods */

  // sets request start position
  // @param start : the requested start position
  virtual void SetStart(int64_t start);

  // sets request length
  // @param length : the requested length
  virtual void SetLength(unsigned int length);

  // sets any data length flag, request will be processed with any available data (even zero length data)
  // @param anyDataLength : true if any data length flag have to be set, false otherwise
  virtual void SetAnyDataLength(bool anyDataLength);

  // sets any non zero data length flag, request will be processed with any non zero available data
  // @param anyNonZeroDataLength : true if any non zero data length flag have to be set, false otherwise
  virtual void SetAnyNonZeroDataLength(bool anyNonZeroDataLength);

  /* other methods */
  
  // tests if any data length flag is set
  // @return : true if any data length is set, false otherwise
  virtual bool IsSetAnyDataLength(void);

  // tests if any non zero data length flag is set
  // @return : true if any non zero data length is set, false otherwise
  virtual bool IsSetAnyNonZeroDataLength(void);

protected:
  // holds request data start
  int64_t start;
  // holds request data length
  unsigned int length;

  /* methods */

  // gets new instance of stream package request
  // @return : new stream package request instance or NULL if error
  virtual CStreamPackageRequest *CreatePackageRequest(void);

  // deeply clones current instance
  // @param item : the stream package request instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CStreamPackageRequest *item);
};

#endif