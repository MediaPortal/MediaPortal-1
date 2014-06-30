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

#ifndef __RTSP_DUMP_BOX_DEFINED
#define __RTSP_DUMP_BOX_DEFINED

#include "DumpBox.h"

#define RTSP_DUMP_BOX_FLAG_NONE                                       DUMP_BOX_FLAG_NONE

#define RTSP_DUMP_BOX_FLAG_LAST                                       (DUMP_BOX_FLAG_LAST + 0)

class CRtspDumpBox : public CDumpBox
{
public:
  CRtspDumpBox(HRESULT *result);
  virtual ~CRtspDumpBox(void);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // gets box factory for creating additional boxes in current box
  // @return : box factory or NULL if error
  virtual CBoxFactory *GetBoxFactory(void);
};

#endif