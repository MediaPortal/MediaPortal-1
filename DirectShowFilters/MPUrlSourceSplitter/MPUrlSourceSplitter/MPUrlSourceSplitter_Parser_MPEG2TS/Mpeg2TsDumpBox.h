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

#ifndef __MPEG2TS_DUMP_BOX_DEFINED
#define __MPEG2TS_DUMP_BOX_DEFINED

#include "DumpBox.h"
#include "StreamPackage.h"

#define MPEG2TS_DUMP_BOX_TYPE                                         L"m2ts"

#define MPEG2TS_DUMP_BOX_FLAG_NONE                                    DUMP_BOX_FLAG_NONE

#define MPEG2TS_DUMP_BOX_FLAG_INPUT_DATA                              (1 << (DUMP_BOX_FLAG_LAST + 0))
#define MPEG2TS_DUMP_BOX_FLAG_OUTPUT_DATA                             (1 << (DUMP_BOX_FLAG_LAST + 1))

#define MPEG2TS_DUMP_BOX_FLAG_LAST                                    (DUMP_BOX_FLAG_LAST + 2)

class CMpeg2TsDumpBox : public CDumpBox
{
public:
  CMpeg2TsDumpBox(HRESULT *result);
  virtual ~CMpeg2TsDumpBox(void);

  /* get methods */

  /* set methods */

  // sets MPEG2 TS dump box data with stream package data
  // @param streamPackage : the stream package to set
  // @return : true if successful, false otherwise
  bool SetStreamPackage(CStreamPackage *streamPackage);

  // sets if dump box contains input data
  // @param inputData : true if dump box contains input data, false otherwise
  void SetInputData(bool inputData);

  // sets if dump box contains output data
  // @param inputData : true if dump box contains output data, false otherwise
  void SetOutputData(bool outputData);

  /* other methods */

  // tests if dump box contains input data
  // @return : true if dump box contains input data, false otherwise
  bool IsInputData(void);

  // tests if dump box contains output data
  // @return : true if dump box contains output data, false otherwise
  bool IsOutputData(void);

protected:

  // holds stream package data
  uint8_t packageState;
  int32_t packageErrorCode;

  // holds request data
  uint32_t requestFlags;
  uint32_t requestId;
  int64_t requestStart;
  uint32_t requestLength;
  uint32_t requestStreamId;
  uint32_t requestStartTime;

  // holds response data
  uint32_t responseFlags;

  /* methods */

  // gets whole box size
  // method is called to determine whole box size for storing box into buffer
  // @return : size of box 
  virtual uint64_t GetBoxSize(void);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed (added to buffer)
  // @return : number of bytes stored into buffer, 0 if error
  virtual uint32_t GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @param checkType : specifies if check for type is allowed
  // @return : number of bytes read from buffer, 0 if error
  virtual unsigned int ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes, bool checkType);
};

#endif