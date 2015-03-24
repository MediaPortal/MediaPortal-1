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

#ifndef __MSHS_DUMP_BOX_DEFINED
#define __MSHS_DUMP_BOX_DEFINED

#include "HttpDumpBox.h"

#define MSHS_DUMP_BOX_TYPE                                            L"mshs"

#define MSHS_DUMP_BOX_FLAG_NONE                                       HTTP_DUMP_BOX_FLAG_NONE

#define MSHS_DUMP_BOX_FLAG_LAST                                       (HTTP_DUMP_BOX_FLAG_LAST + 0)

class CMshsDumpBox : public CHttpDumpBox
{
public:
  CMshsDumpBox(HRESULT *result);
  virtual ~CMshsDumpBox(void);

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
  // @return : number of bytes read from buffer, 0 if error
  virtual unsigned int ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes, bool checkType);
};

#endif