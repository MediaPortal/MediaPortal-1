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

#define RTSP_PAYLOAD_TYPE_FLAG_NONE                                   PAYLOAD_TYPE_FLAG_NONE

// specifies if payload type is container (e.g. avi, mkv, flv, ...)
#define RTSP_PAYLOAD_TYPE_FLAG_CONTAINER                              (1 << (PAYLOAD_TYPE_FLAG_LAST + 0))
// specifies if payload type is packetized stream
#define RTSP_PAYLOAD_TYPE_FLAG_PACKETS                                (1 << (PAYLOAD_TYPE_FLAG_LAST + 1))

#define RTSP_PAYLOAD_TYPE_FLAG_LAST                                   (PAYLOAD_TYPE_FLAG_LAST + 2)

class CRtspPayloadType : public CPayloadType
{
public:
  CRtspPayloadType(HRESULT *result);
  virtual ~CRtspPayloadType(void);

  /* get methods */

  // gets payload stream input format
  // @return : payload stream input format or NULL if not specified
  virtual const wchar_t *GetStreamInputFormat(void);

  /* set methods */

  // sets payload type stream input format
  // @param streamInputFormat : the payload type stream input format to set
  // @return : true if successful, false otherwise
  virtual bool SetStreamInputFormat(const wchar_t *streamInputFormat);

  /* other methods */

  // copy values from specified payload type
  // @param payloadType : the payload type to copy values from
  // @return : true if successful, false otherwise
  virtual bool CopyFromPayloadType(CRtspPayloadType *payloadType);

protected:
  wchar_t *streamInputFormat;

  /* methods */

  // creates payload type
  // @return : payload type or NULL if error
  virtual CPayloadType *CreatePayloadType(void);

  // deeply clones current instance to specified payload type
  // @param payloadType : the payload type to clone current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CPayloadType *payloadType);
};

#endif