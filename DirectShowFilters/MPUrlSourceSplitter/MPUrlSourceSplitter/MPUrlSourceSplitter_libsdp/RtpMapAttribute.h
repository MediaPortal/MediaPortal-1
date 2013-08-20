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

#ifndef __RTP_MAP_ATTRIBUTE_DEFINED
#define __RTP_MAP_ATTRIBUTE_DEFINED

#include "Attribute.h"

#define TAG_ATTRIBUTE_RTP_MAP                               L"rtpmap"

#define TAG_ATTRIBUTE_INSTANCE_RTP_MAP                      L"a_rtpmap"

class CRtpMapAttribute : public CAttribute
{
public:
  // initializes a new instance of CRtpMapAttribute class
  CRtpMapAttribute(void);
  virtual ~CRtpMapAttribute(void);

  /* get methods */

  virtual unsigned int GetPayloadType(void);

  virtual const wchar_t *GetEncodingName(void);

  virtual unsigned int GetClockRate(void);

  virtual const wchar_t *GetEncodingParameters(void);

  /* set methods */

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds payload type
  unsigned int payloadType;

  // holds encoding name
  wchar_t *encodingName;

  // holds clock rate
  unsigned int clockRate;

  // holds encoding parameters (optional)
  wchar_t *encodingParameters;
};

#endif