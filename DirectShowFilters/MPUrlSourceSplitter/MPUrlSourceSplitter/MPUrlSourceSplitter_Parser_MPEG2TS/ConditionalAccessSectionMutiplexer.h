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

#ifndef __CONDITIONAL_ACCESS_SECTION_MULTIPLEXER_DEFINED
#define __CONDITIONAL_ACCESS_SECTION_MULTIPLEXER_DEFINED

#include "SectionMultiplexer.h"

#define CONDITIONAL_ACCESS_SECTION_MULTIPLEXER_FLAG_NONE              SECTION_MULTIPLEXER_FLAG_NONE

#define CONDITIONAL_ACCESS_SECTION_MULTIPLEXER_FLAG_LAST              (SECTION_MULTIPLEXER_FLAG_LAST + 0)

class CConditionalAccessSectionMutiplexer : public CSectionMultiplexer
{
public:
  CConditionalAccessSectionMutiplexer(HRESULT *result, unsigned int pid, unsigned int requestedPid, unsigned int continuityCounter);
  virtual ~CConditionalAccessSectionMutiplexer();

  /* get methods */

  /* set methods */

  /* other methods */

  // adds section to multiplexer
  // @param section : the section to add to multiplexer
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT AddSection(CSection *section);

  // multiplexes available sections into stream fragments
  // return : S_OK if successful, error code otherwise
  virtual HRESULT MultiplexSections(void);

protected:

  /* methods */

  // increases reference count in stream fragment
  // @param streamFragment : the MPEG2 TS stream fragment to increase reference count
  virtual void IncreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment);

  // decreases reference count in stream fragment
  // @param streamFragment : the MPEG2 TS stream fragment to decrease reference count
  virtual void DecreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment);
};

#endif