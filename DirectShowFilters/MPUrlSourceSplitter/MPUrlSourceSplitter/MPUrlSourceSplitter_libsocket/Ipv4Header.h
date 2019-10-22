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

#ifndef __IPV4_HEADER_DEFINED
#define __IPV4_HEADER_DEFINED

#include "Flags.h"

#define IPV4_HEADER_FLAG_NONE                                                 FLAGS_NONE

#define IPV4_HEADER_FLAG_IDENTIFICATION_SPECIFIC                              (1 << (FLAGS_LAST + 0))
#define IPV4_HEADER_FLAG_IDENTIFICATION_RANDOM                                (1 << (FLAGS_LAST + 1))
#define IPV4_HEADER_FLAG_DONT_FRAGMENT                                        (1 << (FLAGS_LAST + 2))
#define IPV4_HEADER_FLAG_MORE_FRAGMENTS                                       (1 << (FLAGS_LAST + 3))

#define IPV4_HEADER_FLAG_LAST                                                 (FLAGS_LAST + 0)

class CIpv4Header : public CFlags
{
public:
  CIpv4Header(HRESULT *result);
  virtual ~CIpv4Header();

  /* get methods */

  // gets IPV4 header DSCP value
  // @return : IPV4 header DSCP value
  virtual uint8_t GetDscp(void);

  // gets IPV4 header ECN value
  // @return : IPV4 header ECN value
  virtual uint8_t GetEcn(void);

  // gets IPV4 header identification value
  // @return : IPV4 header identification value
  virtual uint16_t GetIdentification(void);

  // gets IPV4 header TTL value
  // @return : IPV4 header TTL value
  virtual uint8_t GetTtl(void);

  // gets IPV4 header protocol value
  // @return : IPV4 header protocol value
  virtual uint8_t GetProtocol(void);

  // gets IPV4 header options value
  // @return : IPV4 header options value
  virtual uint8_t *GetOptions(void);

  // gets IPV4 header options length
  // @return : IPV4 header options length
  virtual uint8_t GetOptionsLength(void);

  /* set methods */

  // sets IPV4 header DSCP field 
  // @param dscp : IPV4 header DSCP value
  virtual void SetDscp(uint8_t dscp);

  // sets IPV4 header ECN field 
  // @param ecn : IPV4 header ECN value
  virtual void SetEcn(uint8_t ecn);

  // sets IPV4 identification fields 
  // @param identification : IPV4 header identification value
  virtual void SetIdentification(uint16_t identification);

  // sets IPV4 dont fragment flag
  // @param flags : IPV4 dont fragment flag value
  virtual void SetDontFragment(bool dontFragment);

  // sets IPV4 more fragments flag value
  // @param flags : IPV4 more fragments flag value
  virtual void SetMoreFragments(bool moreFragments);

  // sets IPV4 header TTL field 
  // @param ttl : IPV4 header TTL value
  virtual void SetTtl(uint8_t ttl);

  // sets IPV4 header protocol field 
  // @param protocol : protocol header value
  virtual void SetProtocol(uint8_t protocol);

  // sets IPV4 header OPTIONS fields
  // @param options : the array of uint8_t representing OPTIONS fields
  // @param optionsLength : the length of options parameter
  // @return : true if successful, false otherwise
  virtual bool SetOptions(uint8_t *options, uint8_t optionsLength);

  /* other methods */

  // tests if don't fragment flag in IPV4 header is set
  // @return : true if don't fragment flag is set, false otherwise
  virtual bool IsDontFragment(void);

  // tests if more fragments flag in IPV4 header is set
  // @return : true if mode fragments flag is set, false otherwise
  virtual bool IsMoreFragments(void);

  // clone current instance
  // @return : new instance of CIpv4Header class or NULL if failed
  virtual CIpv4Header *Clone(void);

protected:

  uint8_t dscp;
  uint8_t ecn;
  uint16_t identification;
  uint8_t ttl;
  uint8_t protocol;
  uint8_t *options;
  uint8_t optionsLength;

  /* methods */
};

#endif