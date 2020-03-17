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

#ifndef __STREAM_PACKAGE_REQUEST_DEFINED
#define __STREAM_PACKAGE_REQUEST_DEFINED

#include "Flags.h"

#define STREAM_PACKAGE_REQUEST_FLAG_NONE                              FLAGS_NONE

#define STREAM_PACKAGE_REQUEST_FLAG_LAST                              (FLAGS_LAST + 0)

class CStreamPackageRequest : public CFlags
{
public:
  CStreamPackageRequest(HRESULT *result);
  virtual ~CStreamPackageRequest(void);

  /* get methods */

  // gets request ID
  // @return : request ID
  virtual unsigned int GetId(void);

  // gets request stream ID
  // @return : request stream ID
  virtual unsigned int GetStreamId(void);

  // gets start time in ms of processing request
  // @return : start time of processing request in ms
  virtual unsigned int GetStartTime(void);

  /* set methods */

  // sets request ID
  // @param id : the request ID to set
  virtual void SetId(unsigned int id);

  // sets request stream ID
  // @param streamId : the request stream ID to set
  virtual void SetStreamId(unsigned int streamId);

  // sets start time in ms of processing request
  // @param startTime : the start time of starting processing request in ms
  virtual void SetStartTime(unsigned int startTime);

  /* other methods */

  // deeply clones current instance of stream package request
  // @return : deep clone of current instance or NULL if error
  virtual CStreamPackageRequest *Clone(void);

protected:
  // holds request ID
  unsigned int id;
  // holds stream ID for current request
  unsigned int streamId;
  // holds start time in ms
  unsigned int startTime;

  /* methods */

  // gets new instance of stream package request
  // @return : new stream package request instance or NULL if error
  virtual CStreamPackageRequest *CreatePackageRequest(void) = 0;

  // deeply clones current instance
  // @param item : the stream package request instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CStreamPackageRequest *item);
};

#endif