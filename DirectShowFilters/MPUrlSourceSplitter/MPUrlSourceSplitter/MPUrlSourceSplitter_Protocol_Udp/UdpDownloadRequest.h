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

#ifndef __UDP_DOWNLOAD_REQUEST_DEFINED
#define __UDP_DOWNLOAD_REQUEST_DEFINED

#include "DownloadRequest.h"
#include "Ipv4Header.h"

#define UDP_DOWNLOAD_REQUEST_FLAG_NONE                                DOWNLOAD_REQUEST_FLAG_NONE

#define UDP_DOWNLOAD_REQUEST_FLAG_LAST                                (DOWNLOAD_REQUEST_FLAG_LAST + 0)

class CUdpDownloadRequest : public CDownloadRequest
{
public:
  CUdpDownloadRequest(HRESULT *result);
  virtual ~CUdpDownloadRequest(void);

  /* get methods */

  // gets check interval for incoming data (in ms)
  // @return : check interval for incoming data (in ms)
  virtual unsigned int GetCheckInterval(void);

  // gets IPV4 header
  // @return : IPV4 header or NULL if not specified
  virtual CIpv4Header *GetIpv4Header(void);

  // gets IGMP packet interval (in ms)
  // @return : IGMP packet interval (in ms)
  virtual unsigned int GetIgmpInterval(void);

  /* set methods */

  // sets receive data check interval (in ms)
  // @param checkInterval : the check interval for received data (in ms)
  virtual void SetCheckInterval(unsigned int checkInterval);

  // sets IPV4 header
  // @param header : the IPV4 header
  // @result : S_OK if successful, error code otherwise
  virtual HRESULT SetIpv4Header(CIpv4Header *header);

  // sets IGMP packet interval (in ms)
  // @param checkInterval : the IGMP packet interval (in ms)
  virtual void SetIgmpInterval(unsigned int igmpInterval);

  /* other methods */

  // tests if UDP request have to use raw socket
  // @return : true if raw socket have to be used, false otherwise
  virtual bool IsRawSocket(void);

protected:

  // holds check interval for incoming data
  unsigned int checkInterval;

  // specific IPV4 fields
  CIpv4Header *ipv4Header;

  // holds IGMP interval
  unsigned int igmpInterval;

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