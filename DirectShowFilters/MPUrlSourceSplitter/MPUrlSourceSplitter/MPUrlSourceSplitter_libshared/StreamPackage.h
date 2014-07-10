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

#ifndef __STREAM_PACKAGE_DEFINED
#define __STREAM_PACKAGE_DEFINED

#include "StreamPackageRequest.h"
#include "StreamPackageResponse.h"

class CStreamPackage
{
public:
  CStreamPackage(HRESULT *result);
  ~CStreamPackage(void);

  enum ProcessingState
  {
    Invalid,
    Created,
    Waiting,
    Completed
  };

  /* get methods */

  // gets stream package processing state
  // @return : stream package processing state
  ProcessingState GetState(void);

  // gets stream package request
  // @return : stream package request or NULL if error or not specified
  CStreamPackageRequest *GetRequest(void);

  // gets stream package response
  // @return : stream package response or NULL if error or not specified
  CStreamPackageResponse *GetResponse(void);

  // gets error code
  // @return : error code
  HRESULT GetError(void);

  /* set methods */

  // sets request for stream package
  // request is not cloned, just set to same pointer, freed in destructor
  // state is changed to Invalid (if request is NULL) or Created (request is not NULL)
  // @param request : the request for stream package
  void SetRequest(CStreamPackageRequest *request);

  // sets response for stream package
  // response is not cloned, just set to same pointer, freed in destructor
  // @param response : the response for stream package
  void SetResponse(CStreamPackageResponse *response);

  // sets stream package processing as completed (in case if not invalid)
  void SetCompleted(HRESULT error);

  // sets stream package processing as waiting
  void SetWaiting(void);

  /* other methods */

  // clears instance
  void Clear(void);

  // tests if response has error
  // @return : true if response has error, false otherwise
  bool IsError(void);

  // deeply clones current instance of stream package
  // @return : deep clone of current instance or NULL if error
  CStreamPackage *Clone(void);

protected:
  // holds processing state
  ProcessingState state;
  // holds stream package request
  CStreamPackageRequest *request;
  // holds stream package response
  CStreamPackageResponse *response;
  // holds error code
  HRESULT errorCode;
};

#endif