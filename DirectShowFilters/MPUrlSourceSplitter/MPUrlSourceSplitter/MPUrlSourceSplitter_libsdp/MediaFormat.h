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

#ifndef __MEDIA_FORMAT_DEFINED
#define __MEDIA_FORMAT_DEFINED

#define MEDIA_FORMAT_PAYLOAD_TYPE_UNSPECIFIED               UINT_MAX
#define MEDIA_FORMAT_CHANNELS_UNSPECIFIED                   UINT_MAX
#define MEDIA_FORMAT_CLOCK_RATE_UNSPECIFIED                 UINT_MAX

#define MEDIA_FORMAT_TYPE_AUDIO                             L"audio"
#define MEDIA_FORMAT_TYPE_VIDEO                             L"video"

class CMediaFormat
{
public:
  // initializes a new instance of CMediaFormat class
  CMediaFormat(HRESULT *result);
  ~CMediaFormat(void);

  /* get methods */

  // gets payload type
  // @return : payload type or MEDIA_FORMAT_PAYLOAD_TYPE_UNSPECIFIED if not specified
  unsigned int GetPayloadType(void);

  // gets media format name
  // @return : media format name or NULL if not specified
  const wchar_t *GetName(void);

  // gets media format type
  // @return : media format type or NULL if not specified
  const wchar_t *GetType(void);

  // gets number of channels
  // @return : number of channel or MEDIA_FORMAT_CHANNELS_UNSPECIFIED if not specified
  unsigned int GetChannels(void);

  // gets clock rate
  // @return : clock rate or MEDIA_FORMAT_CLOCK_RATE_UNSPECIFIED if not specified
  unsigned int GetClockRate(void);

  /* set methods */

  // sets payload type
  // @param payloadType : payload type to set
  void SetPayloadType(unsigned int payloadType);

  // sets media format name
  // @param name : media format name to set
  // @return : true if successful, false otherwise
  bool SetName(const wchar_t *name);

  // sets media format type
  // @param type : media format type to set
  // @return : true if successful, false otherwise
  bool SetType(const wchar_t *type);

  // sets channel
  // @param channels : channels to set
  void SetChannels(unsigned int channels);

  // sets clock rate
  // @param clockRate : clock rate to set
  void SetClockRate(unsigned int clockRate);

  /* other methods */

protected:

  // holds payload type (MEDIA_FORMAT_PAYLOAD_TYPE_UNSPECIFIED if not specified)
  unsigned int payloadType;

  // holds media format name (NULL if not specified)
  wchar_t *name;

  // holds media format type (audio, video, ...), NULL if not specified
  wchar_t *type;

  // holds number of channels (MEDIA_FORMAT_CHANNELS_UNSPECIFIED if not specified)
  unsigned int channels;

  // holds clock rate (MEDIA_FORMAT_CLOCK_RATE_UNSPECIFIED if not specified)
  unsigned int clockRate;
};

#endif