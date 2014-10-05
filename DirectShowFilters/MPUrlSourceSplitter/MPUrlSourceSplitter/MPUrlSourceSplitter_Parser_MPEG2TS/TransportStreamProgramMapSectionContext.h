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

#ifndef __TRANSPORT_STREAM_PROGRAM_MAP_SECTION_CONTEXT_DEFINED
#define __TRANSPORT_STREAM_PROGRAM_MAP_SECTION_CONTEXT_DEFINED

#include "SectionContext.h"
#include "TransportStreamProgramMapSection.h"

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_CONTEXT_FLAG_NONE       SECTION_CONTEXT_FLAG_NONE

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_CONTEXT_FLAG_LAST       (SECTION_CONTEXT_FLAG_LAST + 0)

#define TRANSPORT_STREAM_PROGRAM_MAP_PID_NOT_DEFINED                  0xFFFF

class CTransportStreamProgramMapSectionContext : public CSectionContext
{
public:
  CTransportStreamProgramMapSectionContext(HRESULT *result);
  virtual ~CTransportStreamProgramMapSectionContext(void);

  /* get methods */

  // gets orginal section in section context
  // @return : original section in section context (can be NULL if section is not complete)
  virtual CTransportStreamProgramMapSection *GetOriginalSection(void);

  // gets updated section in section context
  // @return : updated section in section context (can be NULL if section is not complete)
  virtual CTransportStreamProgramMapSection *GetUpdatedSection(void);

  // gets transport stream program map PSI packet PID
  // @return : transport stream program map PSI packet PID
  uint16_t GetPID(void);

  /* set methods */

  // sets original section (only reference, section is not cloned, but section is released from memory in destructor)
  // @param section : reference to section to set
  // @return : true if successful, false otherwise (e.g. wrong section type)
  virtual bool SetOriginalSection(CSection *section);

  // sets transport stream program map PSI packet PID
  // @param pid : the transport stream program map PSI packet PID to set
  void SetPID(uint16_t pid);

  /* other methods */

  // creates updated section by cloning original section
  // @return : true if successful, false otherwise
  virtual bool CreateUpdatedSection(void);

protected:
  // holds transport stream program map PSI packet PID
  uint16_t pid;

  /* methods */
};

#endif