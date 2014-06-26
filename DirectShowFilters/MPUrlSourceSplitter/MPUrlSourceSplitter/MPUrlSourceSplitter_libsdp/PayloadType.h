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

#ifndef __PAYLOAD_TYPE_DEFINED
#define __PAYLOAD_TYPE_DEFINED

#include "Flags.h"

#define PAYLOAD_TYPE_ID_DYNAMIC                                       UINT_MAX
#define PAYLOAD_TYPE_CLOCK_RATE_VARIABLE                              UINT_MAX
#define PAYLOAD_TYPE_CHANNELS_VARIABLE                                UINT_MAX

#define PAYLOAD_TYPE_FLAG_NONE                                        FLAGS_NONE

#define PAYLOAD_TYPE_FLAG_LAST                                        (FLAGS_LAST + 0)

class CPayloadType : public CFlags
{
public:
  enum MediaType { Audio, Video, Both, Unknown };

  CPayloadType(HRESULT *result);
  virtual ~CPayloadType(void);

  /* get methods */

  // gets payload type ID
  // @return : payload type ID or PAYLOAD_TYPE_ID_DYNAMIC if not specified
  virtual unsigned int GetId(void);

  // gets encoding name of payload type
  // @return : encoding name or NULL if not specified
  virtual const wchar_t *GetEncodingName(void);

  // gets media type of payload type
  // @return : media type (Unknown if not specified)
  virtual MediaType GetMediaType(void);

  // gets clock rate in payload type
  // @return : clock rate or PAYLOAD_TYPE_CLOCK_RATE_VARIABLE if not specified
  virtual unsigned int GetClockRate(void);

  // gets channels in payload type
  // @return : channels or PAYLOAD_TYPE_CHANNELS_VARIABLE if not specified
  virtual unsigned int GetChannels(void);

  /* set methods */

  // sets payload type ID
  // @param id : payload type ID or PAYLOAD_TYPE_ID_DYNAMIC if not specified
  virtual void SetId(unsigned int id);

  // sets encoding name of payload type
  // @param encodingName : encoding name or NULL if not specified
  // @return : true if successful, false otherwise
  virtual bool SetEncodingName(const wchar_t *encodingName);

  // sets media type of payload type
  // @param mediaType : media type (Unknown if not specified) to set
  virtual void SetMediaType(MediaType mediaType);

  // sets clock rate in payload type
  // @param clockRate : clock rate or PAYLOAD_TYPE_CLOCK_RATE_VARIABLE if not specified
  virtual void SetClockRate(unsigned int clockRate);

  // sets channels in payload type
  // @param channels : channels or PAYLOAD_TYPE_CHANNELS_VARIABLE if not specified
  virtual void SetChannels(unsigned int channels);

  /* other methods */

  // deep clone of current instance
  // @return : reference to clone of payload type or NULL if error
  virtual CPayloadType *Clone(void);

protected:
  // holds payload type ID
  unsigned int id;

  // holds encoding name
  wchar_t *encodingName;

  // holds media type
  MediaType mediaType;

  // holds clock rate
  unsigned int clockRate;

  // holds channels
  unsigned int channels;

  /* methods */

  // creates payload type
  // @return : payload type or NULL if error
  virtual CPayloadType *CreatePayloadType(void);

  // deeply clones current instance to specified payload type
  // @param payloadType : the payload type to clone current instance
  // @result : true if successful, false otherwise
  virtual bool CloneInternal(CPayloadType *payloadType);
};

#endif