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

#ifndef __F4M_BASE_URL_DEFINED
#define __F4M_BASE_URL_DEFINED

class CF4MBaseUrl
{
public:
  // initializes a new instance of CF4MDeliveryType class
  CF4MBaseUrl(void);

  // destructor
  ~CF4MBaseUrl(void);

  /* get methods */

  // gets base URL for all relative (HTTP-based) URLs in the manifest
  // @return : base URL for all relative (HTTP-based) URLs in the manifest or NULL if not specified
  const wchar_t *GetBaseUrl(void);

  /* set methods */

  // sets base URL for all relative (HTTP-based) URLs in the manifest
  // @param baseUrl : base URL for all relative (HTTP-based) URLs in the manifest or NULL if not specified
  // @return : true if successful, false otherwise
  bool SetBaseUrl(const wchar_t *baseUrl);

  /* other methods */

private:
  // stores base URL for all relative (HTTP-based) URLs in the manifest
  wchar_t *baseUrl;
};

#endif