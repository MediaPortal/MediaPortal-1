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

#ifndef __MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_DEFINED
#define __MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_DEFINED

#include "MPUrlSourceSplitterOutputPin.h"

#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE                                 MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MPEG_TS                    (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 0))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MPEG                       (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 1))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_WTV                        (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 2))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_ASF                        (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 3))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_OGG                        (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 4))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MATROSKA                   (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 5))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_AVI                        (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 6))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MP4                        (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 7))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS           (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 8))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_PGS_DROP_STATE                       (1 << (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 9))

#define MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_LAST                                 (MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST + 10)

class CMPUrlSourceSplitterOutputSplitterPin : public CMPUrlSourceSplitterOutputPin
{
public:
  CMPUrlSourceSplitterOutputSplitterPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes, const wchar_t *containerFormat);
  virtual ~CMPUrlSourceSplitterOutputSplitterPin();

  // CBaseOutputPin

  // Requests the connected input pin to begin a flush operation
  // @return : S_OK if successful, VFW_E_NOT_CONNECTED if pin is not connected, error code otherwise
  virtual HRESULT DeliverBeginFlush();

  // queues output pin packet
  // @param packet : the packet to queue to output pin
  // @param timeout : the timeout in ms to queue to output pin
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueuePacket(COutputPinPacket *packet, DWORD timeout);

  /* get methods */

  /* set methods */

  /* other methods */
 
protected:

  /* data for H264 Annex B stream */

  // holds data for parsing of H264 Annex B stream
  COutputPinPacket *h264Buffer;
  COutputPinPacketCollection *h264PacketCollection;

  /* methods */

  // parses output pin packet
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT Parse(GUID subType, COutputPinPacket *packet);

  // sets if has access unit delimiters (for H264)
  // @param hasAccessUnitDelimiters : true if has access unit delimiters, false otherwise
  virtual void SetHasAccessUnitDelimiters(bool hasAccessUnitDelimiters);

  // sets PGS drop state
  // @param pgsDropState : true if PGS drop state, false otherwise
  virtual void SetPGSDropState(bool pgsDropState);

  // tests if container is MPEG TS
  // @return : true if container if MPEG TS, false otherwise
  virtual bool IsContainerMpegTs(void);

  // tests if container is MPEG
  // @return : true if container if MPEG, false otherwise
  virtual bool IsContainerMpeg(void);

  // tests if container is WTV
  // @return : true if container if WTV, false otherwise
  virtual bool IsContainerWtv(void);

  // tests if container is ASF
  // @return : true if container if ASF, false otherwise
  virtual bool IsContainerAsf(void);

  // tests if container is OGG
  // @return : true if container if OGG, false otherwise
  virtual bool IsContainerOgg(void);

  // tests if container is MATROSKA
  // @return : true if container if MATROSKA, false otherwise
  virtual bool IsContainerMatroska(void);

  // tests if container is AVI
  // @return : true if container if AVI, false otherwise
  virtual bool IsContainerAvi(void);

  // tests if container is MP4
  // @return : true if container if MP4, false otherwise
  virtual bool IsContainerMp4(void);

  // tests if has access unit delimiters (for H264)
  // @return : true if has access unit delimiters, false otherwise
  virtual bool HasAccessUnitDelimiters(void);

  // tests if PGS drop state flag is set
  // @return : true if PGS drop state flag is set, false otherwise
  virtual bool IsPGSDropState(void);
};

#endif