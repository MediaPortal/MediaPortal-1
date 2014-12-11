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

#ifndef __MP_URL_SOURCE_SPLITTER_OUTPUT_M2TS_MUXER_PIN_DEFINED
#define __MP_URL_SOURCE_SPLITTER_OUTPUT_M2TS_MUXER_PIN_DEFINED

#include "MPUrlSourceSplitterOutputPin.h"

#define MP_URL_SOURCE_SPLITTER_OUTPUT_M2TS_MUXER_PIN_FLAG_NONE                               MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_OUTPUT_M2TS_MUXER_PIN_FLAG_LAST                               (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 0)

class CMPUrlSourceSplitterOutputM2tsMuxerPin : public CMPUrlSourceSplitterOutputPin
{
public:
  CMPUrlSourceSplitterOutputM2tsMuxerPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes);
  virtual ~CMPUrlSourceSplitterOutputM2tsMuxerPin(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // queues output pin packet
  // @param packet : the packet to queue to output pin
  // @param timeout : the timeout in ms to queue to output pin
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueuePacket(COutputPinPacket *packet, DWORD timeout);

  // queues end of stream
  // @param endOfStreamResult : S_OK if normal end of stream, error code otherwise
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueueEndOfStream(HRESULT endOfStreamResult);
 
protected:

  /* methods */
};

#endif