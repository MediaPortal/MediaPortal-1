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

#ifndef __MPEG2TS_STREAM_FRAGMENT_CONTEXT_DEFINED
#define __MPEG2TS_STREAM_FRAGMENT_CONTEXT_DEFINED

#include "Flags.h"
#include "Mpeg2tsStreamFragment.h"
#include "TsPacketContextCollection.h"

#define MPEG2TS_STREAM_FRAGMENT_CONTEXT_FLAG_NONE                     FLAGS_NONE

#define MPEG2TS_STREAM_FRAGMENT_CONTEXT_FLAG_LAST                     (FLAGS_LAST + 0)

class CMpeg2tsStreamFragmentContext : CFlags
{
public:
  CMpeg2tsStreamFragmentContext(HRESULT *result, CMpeg2tsStreamFragment *fragment);
  virtual ~CMpeg2tsStreamFragmentContext();

  /* get methods */

  // gets MPEG2 TS stream fragment
  // @return : MPEG2 TS stream fragment or NULL if not set
  CMpeg2tsStreamFragment *GetFragment(void);

  // gets MPEG2 TS packet context collection
  // @return : MPEG2 TS packet context collection or NULL if error
  CTsPacketContextCollection *GetPacketContexts(void);

  /* set methods */

  /* other methods */

protected:
  // reference to stream fragment
  CMpeg2tsStreamFragment *fragment;
  // holds MPEG2 TS packet contexts
  CTsPacketContextCollection *packets;

  /* methods */
};

#endif