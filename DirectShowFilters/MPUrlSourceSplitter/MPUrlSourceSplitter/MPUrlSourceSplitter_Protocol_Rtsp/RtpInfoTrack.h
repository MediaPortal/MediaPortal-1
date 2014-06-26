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

#ifndef __RTP_INFO_TRACK_DEFINED
#define __RTP_INFO_TRACK_DEFINED

#include "Flags.h"

#define RTP_INFO_TRACK_FLAG_NONE                                      FLAGS_NONE

#define RTP_INFO_TRACK_FLAG_URL                                       (1 << (FLAGS_LAST + 0))
#define RTP_INFO_TRACK_FLAG_SEQUENCE_NUMBER                           (1 << (FLAGS_LAST + 1))
#define RTP_INFO_TRACK_FLAG_RTP_TIMESTAMP                             (1 << (FLAGS_LAST + 2))

#define RTP_INFO_TRACK_FLAG_LAST                                      (FLAGS_LAST + 3)

#define RTP_INFO_PARAMETER_SEPARATOR                                  L";"
#define RTP_INFO_PARAMETER_SEPARATOR_LENGTH                           1

#define RTP_INFO_PARAMETER_VALUE_SEPARATOR                            L"="
#define RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH                     1

#define RTP_INFO_TRACK_PARAMETER_URL                                  L"url"
#define RTP_INFO_TRACK_PARAMETER_SEQUENCE_NUMBER                      L"seq"
#define RTP_INFO_TRACK_PARAMETER_RTP_TIMESTAMP                        L"rtptime"

#define RTP_INFO_TRACK_PARAMETER_URL_LENGTH                           3
#define RTP_INFO_TRACK_PARAMETER_SEQUENCE_NUMBER_LENGTH               3
#define RTP_INFO_TRACK_PARAMETER_RTP_TIMESTAMP_LENGTH                 7

class CRtpInfoTrack : public CFlags
{
public:
  CRtpInfoTrack(HRESULT *result);
  ~CRtpInfoTrack(void);

  /* get methods */

  // gets RTP info track url
  // @return : RTP info track url or NULL if not specified
  const wchar_t *GetUrl(void);

  // gets RTP info track sequence number
  // @return : RTP info track sequence number
  unsigned int GetSequenceNumber(void);

  // gets RTP info track RTP timestamp
  // @return : RTP info track RTP timestamp
  unsigned int GetRtpTimestamp(void);

  /* set methods */

  /* other methods */

  // tests if url is set
  // @return : true if url is set, false otherwise
  bool IsUrl(void);

  // tests if sequence number is set
  // @return : true if sequence number is set, false otherwise
  bool IsSequenceNumber(void);

  // tests if RTP timestamp is set
  // @return : true if RTP timestamp is set, false otherwise
  bool IsRtpTimestamp(void);

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  CRtpInfoTrack *Clone(void);

  // parses RTP info track
  // @param rtpInfoTrack : RTP info track to parse
  // @param length : the length of RTP info track
  // @return : true if successful, false otherwise
  bool Parse(const wchar_t *rtpInfoTrack, unsigned int length);

protected:

  unsigned int flags;
  wchar_t *url;
  unsigned int sequenceNumber;
  unsigned int rtpTimestamp;
};

#endif