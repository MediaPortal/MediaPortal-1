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

#ifndef __ORIGIN_DEFINED
#define __ORIGIN_DEFINED

#include "SessionTag.h"

#define TAG_ORIGIN                                          L"o"

#define ORIGIN_USER_NAME_NOT_SPECIFIED                      L"-"

#define ORIGIN_VERSION_DEFAULT                              0

#define ORIGIN_NETWORK_TYPE_INTERNET                        L"IN"

#define ORIGIN_ADDRESS_TYPE_IPV4                            L"IP4"
#define ORIGIN_ADDRESS_TYPE_IPV6                            L"IP6"

#define FLAG_ORIGIN_NONE                                    0
#define FLAG_ORIGIN_USER_NAME_NOT_SPECIFIED                 1
#define FLAG_ORIGIN_NETWORK_TYPE_INTERNET                   2
#define FLAG_ORIGIN_ADDRESS_TYPE_IPV4                       4
#define FLAG_ORIGIN_ADDRESS_TYPE_IPV6                       8

class COrigin : public CSessionTag
{
public:
  // initializes a new instance of COrigin class
  COrigin(void);
  virtual ~COrigin(void);

  /* get methods */

  virtual const wchar_t *GetUserName(void);

  virtual const wchar_t *GetSessionId(void);

  virtual unsigned int GetVersion(void);

  virtual const wchar_t *GetNetworkType(void);

  virtual const wchar_t *GetAddressType(void);

  virtual const wchar_t *GetAddress(void);

  /* set methods */

  /* other methods */

  virtual bool IsUserNameNotSpecified(void);

  virtual bool IsNetworkTypeInternet(void);

  virtual bool IsAddressTypeIPV4(void);

  virtual bool IsAddressTypeIPV6(void);

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds user name
  wchar_t *username;

  // holds session ID
  wchar_t *sessionId;

  // holds version of session description
  unsigned int version;

  // holds network type
  wchar_t *networkType;

  // holds address type
  wchar_t *addressType;

  // holds address
  wchar_t *address;

  // holds various flags
  unsigned int flags;
};

#endif