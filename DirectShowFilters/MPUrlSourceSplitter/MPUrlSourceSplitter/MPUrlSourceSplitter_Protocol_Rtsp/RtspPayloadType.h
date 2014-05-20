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

#ifndef __RTSP_PAYLOAD_TYPE_DEFINED
#define __RTSP_PAYLOAD_TYPE_DEFINED

#include "PayloadType.h"

#define RTSP_PAYLOAD_TYPE_FLAG_NONE                                   0x00000000
// specifies if payload type is container (e.g. avi, mkv, flv, ...)
#define RTSP_PAYLOAD_TYPE_FLAG_CONTAINER                              0x00000001
// specifies if payload type is packetized stream
#define RTSP_PAYLOAD_TYPE_FLAG_PACKETS                                0x00000002

class CRtspPayloadType : public CPayloadType
{
public:
  CRtspPayloadType(void);
  ~CRtspPayloadType(void);

  /* get methods */

  // gets combination of set flags
  // @return : combination of set flags
  unsigned int GetFlags(void);

  // gets payload stream input format
  // @return : payload stream input format or NULL if not specified
  const wchar_t *GetStreamInputFormat(void);

  /* set methods */

  // sets combination of flags
  // @param flags : the combination of flags to set
  void SetFlags(unsigned int flags);

  // sets payload type stream input format
  // @param streamInputFormat : the payload type stream input format to set
  // @return : true if successful, false otherwise
  bool SetStreamInputFormat(const wchar_t *streamInputFormat);

  /* other methods */

  // tests if specific combination of flags is set
  // @param flags : the set of flags to test
  // @return : true if set of flags is set, false otherwise
  bool IsSetFlags(unsigned int flags);

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  CRtspPayloadType *Clone(void);

  // copy values from specified payload type
  // @param payloadType : the payload type to copy values from
  // @return : true if successful, false otherwise
  bool CopyFromPayloadType(CRtspPayloadType *payloadType);

protected:
  unsigned int flags;
  wchar_t *streamInputFormat;

  /* methods */

  // deeply clones current instance to specified payload type
  // @param payloadType : the payload type to clone current instance
  // @result : true if successful, false otherwise
  bool CloneInternal(CRtspPayloadType *payloadType);
};

#endif