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

#ifndef __MULTICAST_UDP_RAW_SERVER_DEFINED
#define __MULTICAST_UDP_RAW_SERVER_DEFINED

#include "MulticastUdpServer.h"

#define MULTICAST_UDP_RAW_SERVER_FLAG_NONE                                    MULTICAST_UDP_SERVER_FLAG_NONE

#define MULTICAST_UDP_RAW_SERVER_FLAG_LAST                                    (MULTICAST_UDP_SERVER_FLAG_LAST + 1)


class CMulticastUdpRawServer : public CMulticastUdpServer
{
public:
  CMulticastUdpRawServer(HRESULT *result);
  virtual ~CMulticastUdpRawServer(void);

  /* get methods */

  /* set methods */

  /* other methods */
};

#endif