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

#ifndef __ASYNC_REQUEST_DEFINED
#define __ASYNC_REQUEST_DEFINED

#include <streams.h>

class CAsyncRequest
{
public:
  CAsyncRequest(void);
  ~CAsyncRequest(void);

  enum AsyncState
  {
    Created,
    Waiting,
    WaitingIgnoreTimeout,
    Completed
  };

private:
  // starting position of requested data
  int64_t position;

  // the length of requested data 
  LONG length;

  // the buffer where to write requested data
  BYTE *buffer;

  // request state
  AsyncState requestState;

  // error code when completed async request
  HRESULT errorCode;

  // specifies an arbitrary value that is returned when the request completes
  DWORD_PTR userData;

  // specifies request ID
  unsigned int requestId;

public:
  // init the parameters for this request
  // @param requestId :
  // @param position :
  // @param length :
  // @param buffer :
  // @return : S_OK if successful, E_POINTER if stream or buffer is NULL
  HRESULT Request(unsigned int requestId, int64_t position, LONG length, BYTE *buffer, DWORD_PTR userData);

  // mark request as completed
  // @param errorCode : the error code of async request
  void Complete(HRESULT errorCode);

  // mark request as waiting for data and ignore timeout
  void WaitAndIgnoreTimeout();

  // gets current state of async request
  // @return : one of AsyncState values
  AsyncState GetState(void);

  // gets buffer length
  // @return : buffer length
  LONG GetBufferLength(void);

  // sets buffer length
  // @param length : the length to set
  void SetBufferLength(LONG length);

  // gets start position
  // @return : start position
  int64_t GetStart(void);

  // returns error code
  // @return : error code
  HRESULT GetErrorCode(void);

  // gets an arbitrary value that is returned when the request completes
  // @return : reference to arbitrary value
  DWORD_PTR GetUserData(void);

  // gets request ID
  // @return : request ID
  unsigned int GetRequestId(void);

  // gets buffer for writing data
  // @return : buffer for writing data
  BYTE *GetBuffer(void);
};

#endif

