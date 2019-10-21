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

#define UDP_DOWNLOAD_REQUEST_FLAG_NONE                                DOWNLOAD_REQUEST_FLAG_NONE

#define UDP_DOWNLOAD_REQUEST_FLAG_IPV4_DSCP                           (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 0))
#define UDP_DOWNLOAD_REQUEST_FLAG_IPV4_ECN                            (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 1))
#define UDP_DOWNLOAD_REQUEST_FLAG_IPV4_IDENTIFICATION                 (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 2))
#define UDP_DOWNLOAD_REQUEST_FLAG_IPV4_FLAGS                          (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 3))
#define UDP_DOWNLOAD_REQUEST_FLAG_IPV4_TTL                            (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 4))
#define UDP_DOWNLOAD_REQUEST_FLAG_IPV4_PROTOCOL                       (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 5))
#define UDP_DOWNLOAD_REQUEST_FLAG_IPV4_OPTIONS                        (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 6))

#define UDP_DOWNLOAD_REQUEST_FLAG_LAST                                (DOWNLOAD_REQUEST_FLAG_LAST + 7)

class CUdpDownloadRequest : public CDownloadRequest
{
public:
  CUdpDownloadRequest(HRESULT *result);
  virtual ~CUdpDownloadRequest(void);

  /* get methods */

  // gets check interval for incoming data (in ms)
  // @return : check interval for incoming data (in ms)
  virtual unsigned int GetCheckInterval(void);

  // gets IPV4 DSCP value
  // @return : IPV4 DSCP value if UDP_DOWNLOAD_REQUEST_FLAG_IPV4_DSCP is set
  virtual uint8_t GetIpv4Dscp(void);

  // gets IPV4 ECN value
  // @return : IPV4 ECN value if UDP_DOWNLOAD_REQUEST_FLAG_IPV4_ECN is set
  virtual uint8_t GetIpv4Ecn(void);

  // gets IPV4 identification value
  // @return : IPV4 identification value if UDP_DOWNLOAD_REQUEST_FLAG_IPV4_IDENTIFICATION is set
  virtual uint16_t GetIpv4Identification(void);

  // gets IPV4 flags value
  // @return : IPV4 flags value if UDP_DOWNLOAD_REQUEST_FLAG_IPV4_FLAGS is set
  virtual uint8_t GetIpv4Flags(void);

  // gets IPV4 TTL value
  // @return : IPV4 TTL value if UDP_DOWNLOAD_REQUEST_FLAG_IPV4_TTL is set
  virtual uint8_t GetIpv4Ttl(void);

  // gets IPV4 protocol value
  // @return : IPV4 protocol value if UDP_DOWNLOAD_REQUEST_FLAG_IPV4_PROTOCOL is set
  virtual uint8_t GetIpv4Protocol(void);

  // gets IPV4 options value
  // @return : IPV4 options value if UDP_DOWNLOAD_REQUEST_FLAG_IPV4_OPTIONS is set
  virtual uint8_t *GetIpv4Options(void);

  // gets IPV4 options length
  // @return : IPV4 options length
  virtual uint8_t GetIpv4OptionsLength(void);

  /* set methods */

  // sets receive data check interval (in ms)
  // @param checkInterval : the check interval for received data (in ms)
  virtual void SetCheckInterval(unsigned int checkInterval);

  // sets IPV4 DSCP field 
  // @param dscp : DSCP value
  virtual void SetIpv4Dscp(uint8_t dscp);

  // sets IPV4 ECN field 
  // @param ecn : ECN value
  virtual void SetIpv4Ecn(uint8_t ecn);

  // sets IPV4 identification fields 
  // @param identification : identification value
  virtual void SetIpv4Identification(uint16_t identification);

  // sets IPV4 flags field 
  // @param flags : flags value
  virtual void SetIpv4Flags(uint8_t flags);

  // sets IPV4 TTL field 
  // @param ttl : TTL value
  virtual void SetIpv4Ttl(uint8_t ttl);

  // sets IPV4 protocol field 
  // @param protocol : protocol value
  virtual void SetIpv4Protocol(uint8_t protocol);

  // sets IPV4 OPTIONS fields
  // @param options : the array of uint8_t representing OPTIONS fields
  // @param optionsLength : the length of options parameter
  // @return : true if successful, false otherwise
  virtual bool SetIpv4Options(uint8_t *options, uint8_t optionsLength);

  /* other methods */

  // tests if UDP request have to use raw socket
  // @return : true if raw socket have to be used, false otherwise
  virtual bool IsRawSocket(void);

protected:

  // holds check interval for incoming data
  unsigned int checkInterval;

  // specific IPV4 fields

  uint8_t ipv4Dscp;
  uint8_t ipv4Ecn;
  uint16_t ipv4Identification;
  uint8_t ipv4Flags;
  uint8_t ipv4Ttl;
  uint8_t ipv4Protocol;
  uint8_t *ipv4Options;
  uint8_t ipv4OptionsLength;

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