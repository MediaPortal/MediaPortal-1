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

#ifndef __TS_PACKET_CONTEXT_DEFINED
#define __TS_PACKET_CONTEXT_DEFINED

#include "Flags.h"
#include "SectionContext.h"

#define TS_PACKET_CONTEXT_FLAG_NONE                                     FLAGS_NONE

#define TS_PACKET_CONTEXT_FLAG_SECTION_CONTEXT_OWNER                    (1 << (FLAGS_LAST + 0))

#define TS_PACKET_CONTEXT_FLAG_LAST                                     (FLAGS_LAST + 1)

#define TS_PACKET_INDEX_NOT_SET                                         UINT_MAX

class CTsPacketContext : public CFlags
{
public:
  CTsPacketContext(HRESULT *result);
  virtual ~CTsPacketContext(void);

  /* get methods */

  // gets TS packet index
  // @return : TS packet index or TS_PACKET_INDEX_NOT_SET is not set
  unsigned int GetTsPacketIndex(void);

  // gets section context
  // @return : section context
  virtual CSectionContext *GetSectionContext(void);

  /* set methods */

  // sets TS packet index
  // @param tsPacketIndex : the TS packet index to set
  void SetTsPacketIndex(unsigned int tsPacketIndex);

  // sets section context
  // @param sectionContext : section context to set
  // @return : true if successful, false otherwise (e.g. wrong section context type)
  virtual bool SetSectionContext(CSectionContext *sectionContext);

  // sets section owner flag
  // @param isSectionOwner : flag specyfying that TS packet context is owner of section context
  void SetSectionContextOwner(bool isSectionOwner);

  /* other methods */

  // tests if TS packet context is section context owner 
  bool IsSectionContextOwner(void);

protected:

  // holds TS packet index
  unsigned int tsPacketIndex;
  // holds section context
  CSectionContext *sectionContext;

  /* methods */
};

#endif