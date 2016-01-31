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

  // clears current instance to default state
  void Clear(void);

private:
  // stores bootstrap info ID
  wchar_t *id;
  // stores bootstrap info profile name
  wchar_t *profile;
  // stores bootstrap info URL
  wchar_t *url;
  // stores boostrap info BASE64 encoded value
  wchar_t *value;
};

#endif