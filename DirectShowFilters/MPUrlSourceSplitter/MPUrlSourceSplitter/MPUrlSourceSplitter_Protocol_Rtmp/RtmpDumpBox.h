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

#ifndef __RTMP_DUMP_BOX_DEFINED
#define __RTMP_DUMP_BOX_DEFINED

#include "DumpBox.h"

#define RTMP_DUMP_BOX_TYPE                                            L"rtmp"

#define RTMP_DUMP_BOX_FLAG_NONE                                       DUMP_BOX_FLAG_NONE

#define RTMP_DUMP_BOX_FLAG_LAST                                       (DUMP_BOX_FLAG_LAST + 0)

class CRtmpDumpBox : public CDumpBox
{
public:
  CRtmpDumpBox(HRESULT *result);
  virtual ~CRtmpDumpBox(void);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @param checkType : specifies if check for type is allowed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes, bool checkType);
};

#endif