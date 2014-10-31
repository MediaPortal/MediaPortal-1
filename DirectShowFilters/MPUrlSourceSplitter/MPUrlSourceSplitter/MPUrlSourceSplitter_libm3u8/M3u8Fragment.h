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

#ifndef __M3U8_FRAGMENT_DEFINED
#define __M3U8_FRAGMENT_DEFINED

#include "Flags.h"

#define M3U8_FRAGMENT_FLAG_NONE                                       FLAGS_NONE

#define M3U8_FRAGMENT_FLAG_DISCONTINUITY                              (1 << (FLAGS_LAST + 0))
#define M3U8_FRAGMENT_FLAG_ENCRYPTED                                  (1 << (FLAGS_LAST + 1))
#define M3U8_FRAGMENT_FLAG_END_OF_STREAM                              (1 << (FLAGS_LAST + 2))

#define M3U8_FRAGMENT_FLAG_LAST                                       (FLAGS_LAST + 3)

#define SEQUENCE_NUMBER_NOT_SPECIFIED                                 UINT_MAX
#define DURATION_NOT_SPECIFIED                                        UINT_MAX

class CM3u8Fragment : public CFlags
{
public:
  CM3u8Fragment(HRESULT *result);
  virtual ~CM3u8Fragment(void);

  /* get methods */

  // gets sequence number
  // @return : sequence number or SEQUENCE_NUMBER_NOT_SPECIFIED if not specified
  unsigned int GetSequenceNumber(void);

  // gets duration in ms
  // @return : duration in ms or DURATION_NOT_SPECIFIED if not specified
  unsigned int GetDuration(void);

  // gets URI
  // @return : URI or NULL if not specified
  const wchar_t *GetUri(void);

  /* set methods */

  // sets sequence number
  // @param sequenceNumber : the sequence number to set
  void SetSequenceNumber(unsigned int sequenceNumber);

  // sets duration in ms
  // @param duration : the duration in ms to set
  void SetDuration(unsigned int duration);

  // sets URI
  // @param uri : the URI to set
  // @return : true if successful, false otherwise
  bool SetUri(const wchar_t *uri);

  // sets if after fragment is discontinuity
  // @param discontinuity : true if after fragment is discontinuity, false otherwise
  void SetDiscontinuity(bool discontinuity);

  // sets if fragment is encrypted
  // @param encrypted : true if after fragment is discontinuity, false otherwise
  void SetEncrypted(bool ecnrypted);

  // sets if fragment is end of stream
  // @param endOfStream : true if after fragment is end of stream, false otherwise
  void SetEndOfStream(bool endOfStream);

  /* other methods */

  // tests if after fragment is discontinuity
  // @return : true if after fragment is discontinuity, false otherwise
  bool IsDiscontinuity(void);

  // tests if fragment is encrypted
  // @return : true if encrypted, false otherwise
  bool IsEncrypted(void);

  // tests if fragment is end of stream
  // @return : true if end of stream, false otherwise
  bool IsEndOfStream(void);

protected:

  // holds sequence number
  unsigned int sequenceNumber;

  // holds duration in ms
  unsigned int duration;

  // holds uri (relative or absolute)
  wchar_t *uri;

  /* methods */
};

#endif