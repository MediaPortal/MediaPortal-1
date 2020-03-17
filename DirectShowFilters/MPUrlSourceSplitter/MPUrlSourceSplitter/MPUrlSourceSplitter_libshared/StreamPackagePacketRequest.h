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

#ifndef __STREAM_PACKAGE_PACKET_REQUEST_DEFINED
#define __STREAM_PACKAGE_PACKET_REQUEST_DEFINED

#include "StreamPackageRequest.h"

#include <stdint.h>

#define STREAM_PACKAGE_PACKET_REQUEST_FLAG_NONE                       STREAM_PACKAGE_REQUEST_FLAG_NONE

#define STREAM_PACKAGE_PACKET_REQUEST_FLAG_RESET_PACKET_COUNTER       (1 << (STREAM_PACKAGE_REQUEST_FLAG_LAST + 0))

#define STREAM_PACKAGE_PACKET_REQUEST_FLAG_LAST                       (STREAM_PACKAGE_REQUEST_FLAG_LAST + 1)

class CStreamPackagePacketRequest : public CStreamPackageRequest
{
public:
  CStreamPackagePacketRequest(HRESULT *result);
  virtual ~CStreamPackagePacketRequest(void);

  /* get methods */

  /* set methods */

  // sets reset packet counter flag, packet counter is reset before processing request
  // @param resetPacketCounter : true if reset packet counter flag have to be set, false otherwise
  virtual void SetResetPacketCounter(bool resetPacketCounter);

  /* other methods */

  // tests if reset packet counter flag is set
  // @return : true if reset packet counter is set, false otherwise
  virtual bool IsResetPacketCounter(void);
  
protected:
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