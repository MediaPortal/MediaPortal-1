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

#ifndef __RTSP_SUPPORTED_PAYLOAD_TYPE_COLLECTION_DEFINED
#define __RTSP_SUPPORTED_PAYLOAD_TYPE_COLLECTION_DEFINED

#include "RtspPayloadType.h"
#include "Collection.h"

class CRtspSupportedPayloadTypeCollection : public CCollection<CRtspPayloadType>
{
public:
  CRtspSupportedPayloadTypeCollection(HRESULT *result);
  ~CRtspSupportedPayloadTypeCollection(void);

protected:

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CRtspPayloadType *Clone(CRtspPayloadType *item);

  // adds RTSP payload type to supported payload type collection
  // @param payloadType : the payload type number
  // @param name : the payload type name or NULL if not specified
  // @param streamInputFormat : the payload type stream input format or NULL if not specified
  // @param flags : the combination of flags
  // @return : true if successful, false otherwise
  bool AddPayloadType(unsigned int payloadType, const wchar_t *name, const wchar_t *streamInputFormat, uint32_t flags);
};

#endif