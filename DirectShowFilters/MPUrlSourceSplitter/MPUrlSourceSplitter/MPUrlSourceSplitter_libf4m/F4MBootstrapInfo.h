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

#ifndef __F4M_BOOTSTRAP_INFO_DEFINED
#define __F4M_BOOTSTRAP_INFO_DEFINED

#include "Logger.h"
#include "ParameterCollection.h"

class CF4MBootstrapInfo
{
public:
  // initializes a new instance of CF4MBootstrapInfo class
  CF4MBootstrapInfo(void);

  // destructor
  ~CF4MBootstrapInfo(void);

  /* get methods */

  // gets ID of bootstrap info
  // @return : the ID of bootstrap info
  const wchar_t *GetId(void);

  // gets bootstrap info profile name
  // @return : bootstrap info profile name
  const wchar_t *GetProfile(void);

  // gets bootstrap info URL (if not specified value)
  // @return : bootstrap info URL (can be NULL if value specified)
  const wchar_t *GetUrl(void);

  // gets bootstrap info BASE64 encoded value (if not specified URL)
  // @return : bootstrap info BASE64 encoded value (can be NULL if URL specified)
  const wchar_t *GetValue(void);

  // gets decoding result of BASE64 encoded value
  // @return : E_NOT_VALID_STATE if value is NULL or result from base64_decode() method
  HRESULT GetDecodeResult(void);

  // gets decoded value
  // @return : decoded value or NULL if error
  const unsigned char *GetDecodedValue(void);

  // gets decoded value length
  // @return : decoded value length, UINT_MAX if error
  unsigned int GetDecodedValueLength(void);

  // gets bootstrap info base URL
  // @return : bootstrap info base URL
  const wchar_t *GetBaseUrl(void);

  /* set methods */

  // sets ID of bootstrap info
  // @param id : the ID of bootstrap info to set
  // @return : true if successful, false otherwise
  bool SetId(const wchar_t *id);

  // gets bootstrap info profile name
  // @param profile : bootstrap info profile name to set
  // @return : true if successful, false otherwise
  bool SetProfile(const wchar_t *profile);

  // gets bootstrap info URL (if not specified value)
  // @param url : bootstrap info URL (can be NULL if value specified) to set
  // @return : true if successful, false otherwise
  bool SetUrl(const wchar_t *url);

  // gets bootstrap info BASE64 encoded value (if not specified URL)
  // @param value : bootstrap info BASE64 encoded value (can be NULL if URL specified) to set
  // @return : true if successful, false otherwise
  bool SetValue(const wchar_t *value);

  // sets bootstrap info base URL
  // @param baseUrl : bootstrap info base URL to set
  // @return : true if set, false otherwise
  bool SetBaseUrl(const wchar_t *baseUrl);

  /* other methods */

  // tests if instance is valid
  // @return : true if instance is valid
  bool IsValid(void);

  // tests if for bootstrap info is specified URL
  // @return : true if URL is specified
  bool HasUrl(void);

  // tests if for bootstrap info is specified value
  // @return : true if value is specified
  bool HasValue(void);

  // downloads bootstrap info
  // @param : logger for logging purposes
  // @param protocolName : the protocol name instantiating
  // @param finishTime : time when HTTP request timeout
  // @param referer : referer for HTTP request
  // @param userAgent : user agent for HTTP request
  // @param cookie : cookie for HTTP request
  // @param cookies : cookies for HTTP request, collection of cookies is updated after receving response
  // @param networkInterfaceName : the name of network interface to make HTTP request (can be NULL)
  // @return : S_OK if successfully downloaded, error code otherwise
  HRESULT DownloadBootstrapInfo(CLogger *logger, const wchar_t *protocolName, unsigned int finishTime, const wchar_t *referer, const wchar_t *userAgent, const wchar_t *cookie, CParameterCollection *cookies, const wchar_t *networkInterfaceName);

private:
  // stores bootstrap info ID
  wchar_t *id;
  // stores bootstrap info profile name
  wchar_t *profile;
  // stores bootstrap info URL
  wchar_t *url;
  // stores boostrap info BASE64 encoded value
  wchar_t *value;

  // stores result of BASE64 decoding
  HRESULT decodeResult;
  // stores decoded value
  unsigned char *decodedValue;
  // stores length of decoded value
  unsigned int decodedLength;
  // stores base url for bootstrap info
  wchar_t *baseUrl;
};

#endif