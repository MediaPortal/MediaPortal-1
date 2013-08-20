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

#ifndef __MSHS_MANIFEST_DEFINED
#define __MSHS_MANIFEST_DEFINED

#include "MSHSSmoothStreamingMedia.h"

class CMSHSManifest
{
public:
  // initializes a new instance of CMSHSManifest class
  CMSHSManifest(void);

  // destructor
  ~CMSHSManifest(void);

  /* get methods */

  // gets smooth streaming media
  // @return : smooth streaming media
  CMSHSSmoothStreamingMedia *GetSmoothStreamingMedia(void);

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

private:
  // stores if data are in XML format
  bool isXml;
  // stores last parse error
  int parseError;

  CMSHSSmoothStreamingMedia *smoothStreamingMedia;
};

#endif