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

#ifndef __OUTPUT_PIN_METADATA_BOX_DEFINED
#define __OUTPUT_PIN_METADATA_BOX_DEFINED

#include "Box.h"

#include <streams.h>

#define OUTPUT_PIN_METADATA_BOX_TYPE                                  L"opme"

#define OUTPUT_PIN_METADATA_BOX_FLAG_NONE                             BOX_FLAG_NONE

#define OUTPUT_PIN_METADATA_BOX_FLAG_SYNC_POINT                       (1 << (BOX_FLAG_LAST + 0))
#define OUTPUT_PIN_METADATA_BOX_FLAG_PREROLL                          (1 << (BOX_FLAG_LAST + 1))
#define OUTPUT_PIN_METADATA_BOX_FLAG_DISCONTINUITY                    (1 << (BOX_FLAG_LAST + 2))
#define OUTPUT_PIN_METADATA_BOX_FLAG_TYPE_CHANGED                     (1 << (BOX_FLAG_LAST + 3))
#define OUTPUT_PIN_METADATA_BOX_FLAG_TIME_VALID                       (1 << (BOX_FLAG_LAST + 4))
#define OUTPUT_PIN_METADATA_BOX_FLAG_MEDIA_TIME_VALID                 (1 << (BOX_FLAG_LAST + 5))
#define OUTPUT_PIN_METADATA_BOX_FLAG_TIME_DISCONTINUITY               (1 << (BOX_FLAG_LAST + 6))
#define OUTPUT_PIN_METADATA_BOX_FLAG_STOP_VALID                       (1 << (BOX_FLAG_LAST + 7))

#define OUTPUT_PIN_METADATA_BOX_FLAG_LAST                             (BOX_FLAG_LAST + 8)

class COutputPinMetadataBox : public CBox
{
public:
  COutputPinMetadataBox(HRESULT *result);
  virtual ~COutputPinMetadataBox(void);

  /* get methods */

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  /* set methods */

  // sets media sample to internal fields (except data)
  // @param mediaSample : the media sample to set internal fields
  // @return : true if successful, false otherwise
  virtual bool SetMediaSample(IMediaSample *mediaSample);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:

  //CMediaSample *mediaSample;

  uint32_t mediaSampleFlags;                  /* Flags for this sample */
  //uint32_t mediaSampleTypeSpecificFlags;      /* Media type specific flags */
  int32_t mediaSampleActual;                  /* Length of data in this sample */
  int32_t mediaSampleBufferSize;              /* Size of the buffer */
  int64_t mediaSampleStart;                   /* Start sample time */
  int64_t mediaSampleEnd;                     /* End sample time */
  int64_t mediaSampleMediaStart;              /* Real media start position */
  int32_t mediaSampleMediaEnd;                /* A difference to get the end */
  uint32_t mediaSampleStreamId;               /* Stream id */
  
  /* methods */

  // gets whole box size
  // method is called to determine whole box size for storing box into buffer
  // @return : size of box 
  virtual uint64_t GetBoxSize(void);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed (added to buffer)
  // @return : number of bytes stored into buffer, 0 if error
  virtual uint32_t GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes);
};

#endif