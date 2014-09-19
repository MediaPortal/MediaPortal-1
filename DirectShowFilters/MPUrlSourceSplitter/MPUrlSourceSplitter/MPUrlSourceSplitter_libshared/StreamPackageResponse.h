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

#ifndef __STREAM_PACKAGE_RESPONSE_DEFINED
#define __STREAM_PACKAGE_RESPONSE_DEFINED

#include "Flags.h"

#define STREAM_PACKAGE_RESPONSE_FLAG_NONE                             FLAGS_NONE

#define STREAM_PACKAGE_RESPONSE_FLAG_DISCONTINUITY                    (1 << (FLAGS_LAST + 0))
#define STREAM_PACKAGE_RESPONSE_FLAG_NO_MORE_DATA_AVAILABLE           (1 << (FLAGS_LAST + 1))
#define STREAM_PACKAGE_RESPONSE_FLAG_CONNECTION_LOST_CANNOT_REOPEN    (1 << (FLAGS_LAST + 2))

#define STREAM_PACKAGE_RESPONSE_FLAG_LAST                             (FLAGS_LAST + 3)

class CStreamPackageResponse : public CFlags
{
public:
  CStreamPackageResponse(HRESULT *result);
  virtual ~CStreamPackageResponse(void);

  /* get methods */

  /* set methods */

  // sets discontinuity
  // @param discontinuity : true if discontinuity after data, false otherwise
  virtual void SetDiscontinuity(bool discontinuity);

  // sets no more data available
  // @param noMoreDataAvailable : true if no more data available, false otherwise
  virtual void SetNoMoreDataAvailable(bool noMoreDataAvailable);

  // sets connection lost cannot reopen
  // @param connectionLostCannotReopen : true if connection lost cannot reopen, false otherwise
  virtual void SetConnectionLostCannotReopen(bool connectionLostCannotReopen);

  /* other methods */

  // tests if discontinuity is set
  // @return : true if discontinuity is set, false otherwise
  virtual bool IsDiscontinuity(void);

  // tests if no more data available is set
  // @return : true if no more data available is set, false otherwise
  virtual bool IsNoMoreDataAvailable(void);

  // tests if connection lost cannot reopen is set
  // @return : true if connection lost cannot reopen is set, false otherwise
  virtual bool IsConnectionLostCannotReopen(void);

  // deeply clones current instance of stream package response
  // @return : deep clone of current instance or NULL if error
  virtual CStreamPackageResponse *Clone(void);

protected:

  /* methods */

  // gets new instance of stream package response
  // @return : new stream package response instance or NULL if error
  virtual CStreamPackageResponse *CreatePackageResponse(void) = 0;

  // deeply clones current instance
  // @param item : the stream package response instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CStreamPackageResponse *item);
};

#endif