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

#ifndef __PROGRAM_ASSOCIATION_SECTION_PACKET_CONTEXT_DEFINED
#define __PROGRAM_ASSOCIATION_SECTION_PACKET_CONTEXT_DEFINED

#include "TsPacketContext.h"
#include "ProgramAssociationSectionContext.h"

#define PROGRAM_ASSOCIATION_SECTION_PACKET_CONTEXT_FLAG_NONE          TS_PACKET_CONTEXT_FLAG_NONE

#define PROGRAM_ASSOCIATION_SECTION_PACKET_CONTEXT_FLAG_LAST          (TS_PACKET_CONTEXT_FLAG_LAST + 0)

class CProgramAssociationSectionPacketContext : public CTsPacketContext
{
public:
  CProgramAssociationSectionPacketContext(HRESULT *result);
  virtual ~CProgramAssociationSectionPacketContext(void);

  /* get methods */

  // gets section context
  // @return : section context
  virtual CProgramAssociationSectionContext *GetSectionContext(void);

  /* set methods */

  // sets section context
  // @param sectionContext : section context to set
  // @return : true if successful, false otherwise (e.g. wrong section context type)
  virtual bool SetSectionContext(CSectionContext *sectionContext);

  /* other methods */

protected:

  /* methods */
};

#endif