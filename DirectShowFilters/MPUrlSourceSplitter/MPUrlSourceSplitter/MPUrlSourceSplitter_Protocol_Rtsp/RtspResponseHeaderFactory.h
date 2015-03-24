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

#ifndef __RTSP_RESPONSE_HEADER_FACTORY_DEFINED
#define __RTSP_RESPONSE_HEADER_FACTORY_DEFINED

#include "RtspResponseHeader.h"

#define CREATE_SPECIFIC_RESPONSE_HEADER(responseHeaderType, buffer, length, continueParsing, result)                \
                                                                                                                    \
if (SUCCEEDED(continueParsing) && (result == NULL))                                                                 \
{                                                                                                                   \
  responseHeaderType *responseHeader = new responseHeaderType(&continueParsing);                                    \
  CHECK_POINTER_HRESULT(continueParsing, responseHeader, continueParsing, E_OUTOFMEMORY);                           \
                                                                                                                    \
  if (SUCCEEDED(continueParsing))                                                                                   \
  {                                                                                                                 \
    if (responseHeader->Parse(buffer, length))                                                                      \
    {                                                                                                               \
      result = responseHeader;                                                                                      \
    }                                                                                                               \
  }                                                                                                                 \
                                                                                                                    \
  if (result == NULL)                                                                                               \
  {                                                                                                                 \
    FREE_MEM_CLASS(responseHeader);                                                                                 \
  }                                                                                                                 \
}

class CRtspResponseHeaderFactory
{
public:
  // initializes a new instance of CRtspResponseHeaderFactory class
  CRtspResponseHeaderFactory(void);

  // destructor
  virtual ~CRtspResponseHeaderFactory(void);

  // creates response header from buffer
  // @param buffer : buffer with response header data for parsing
  // @param length : the length of data in buffer
  // @return : session tag or NULL if error
  virtual CRtspResponseHeader *CreateResponseHeader(const wchar_t *buffer, unsigned int length);
};

#endif