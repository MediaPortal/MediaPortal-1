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

#ifndef __F4M_MANIFEST_DEFINED
#define __F4M_MANIFEST_DEFINED

#include "F4MBootstrapInfoCollection.h"
#include "F4MMediaCollection.h"
#include "F4MDeliveryType.h"
#include "F4MBaseUrl.h"
#include "F4MDuration.h"

class CF4MManifest
{
public:
  // initializes a new instance of CF4MManifest class
  CF4MManifest(HRESULT *result);

  // destructor
  ~CF4MManifest(void);

  /* get methods */

  // gets bootstrap info collection from manifest
  // @return : bootstrap info collection from manifest
  CF4MBootstrapInfoCollection *GetBootstrapInfoCollection(void);

  // gets media collection from manifest
  // @return : media collection from manifest
  CF4MMediaCollection *GetMediaCollection(void);

  // gets delivery type from manifest
  // @return : delivery type from manifest
  CF4MDeliveryType *GetDeliveryType(void);

  // gets base URL for all relative (HTTP-based) URLs in the manifest
  // @return : base URL for all relative (HTTP-based) URLs in the manifest
  CF4MBaseUrl *GetBaseUrl(void);

  // gets media duration from manifest
  // @return : media duration from manifest
  CF4MDuration *GetDuration(void);

  // gets last parse error
  // @return : last parse error
  int GetParseError(void);

  /* set methods */

  /* other methods */

  // tests if parsed data is at least in XML format
  // @return : true if data are in XML, false otherwise
  bool IsXml(void);

  // parses F4M manifest
  // @param buffer : buffer with F4M manifest to parse (UTF-8 encoding)
  // @return : true if parsed, false otherwise
  bool Parse(const char *buffer);

  // clears current instance to default state
  void Clear(void);

private:
  // stores if data are in XML format
  bool isXml;
  // stores bootstrap info collection
  CF4MBootstrapInfoCollection *bootstrapInfoCollection;
  // stores media collection
  CF4MMediaCollection *mediaCollection;
  // stores delivery type
  CF4MDeliveryType *deliveryType;
  // stores base url
  CF4MBaseUrl *baseUrl;
  // stores media duration
  CF4MDuration *duration;
  // stores last parse error
  int parseError;
};

#endif