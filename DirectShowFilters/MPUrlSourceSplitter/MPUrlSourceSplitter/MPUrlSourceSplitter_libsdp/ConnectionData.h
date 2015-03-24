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

#ifndef __CONNECTION_DATA_DEFINED
#define __CONNECTION_DATA_DEFINED

#include "SessionTag.h"

#define TAG_CONNECTION_DATA                                           L"c"

#define CONNECTION_DATA_NETWORK_TYPE_INTERNET                         L"IN"
#define CONNECTION_DATA_ADDRESS_TYPE_IPV4                             L"IP4"

#define CONNECTION_DATA_FLAG_NONE                                     SESSION_TAG_FLAG_NONE

#define CONNECTION_DATA_FLAG_NETWORK_TYPE_INTERNET                    (1 << (SESSION_TAG_FLAG_LAST + 0))
#define CONNECTION_DATA_FLAG_ADDRESS_TYPE_IPV4                        (1 << (SESSION_TAG_FLAG_LAST + 1))

#define CONNECTION_DATA_FLAG_LAST                                     (SESSION_TAG_FLAG_LAST + 2)

class CConnectionData : public CSessionTag
{
public:
  // initializes a new instance of CConnectionData class
  CConnectionData(HRESULT *result);
  ~CConnectionData(void);

  /* get methods */

  // gets network type
  // @return : network type
  virtual const wchar_t *GetNetworkType(void);

  // gets address type
  // @return : address type
  virtual const wchar_t *GetAddressType(void);

  // gets connection address
  // @return : connection address
  virtual const wchar_t *GetConnectionAddress(void);

  /* set methods */

  /* other methods */

  // tests if network type is internet
  // @return : true if network type is internet, false otherwise
  virtual bool IsNetworkTypeInternet(void);

  // tests if address type is IPV4
  // @return : true if address type is IPV4, false otherwise
  virtual bool IsAddressTypeIPV4(void);

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds network type
  wchar_t *networkType;

  // holds address type
  wchar_t *addressType;

  // holds connection address
  wchar_t *connectionAddress;
};

#endif