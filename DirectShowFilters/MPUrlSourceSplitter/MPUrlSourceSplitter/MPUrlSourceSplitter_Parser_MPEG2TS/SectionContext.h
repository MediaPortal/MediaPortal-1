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

#ifndef __SECTION_CONTEXT_DEFINED
#define __SECTION_CONTEXT_DEFINED

#include "Flags.h"
#include "Section.h"
#include "TsPacketCollection.h"

#define SECTION_CONTEXT_FLAG_NONE                                     FLAGS_NONE

#define SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_EMPTY                   (1 << (FLAGS_LAST + 0))
#define SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_INCOMPLETE              (1 << (FLAGS_LAST + 1))
#define SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_COMPLETE                (1 << (FLAGS_LAST + 2))
#define SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_ERROR                   (1 << (FLAGS_LAST + 3))
#define SECTION_CONTEXT_FLAG_ORIGINAL_SECTION_IS_SECTION              (1 << (FLAGS_LAST + 4))

#define SECTION_CONTEXT_FLAG_LAST                                     (FLAGS_LAST + 5)

class CSectionContext : public CFlags
{
public:
  CSectionContext(HRESULT *result);
  virtual ~CSectionContext(void);

  /* get methods */

  // gets orginal section in section context
  // @return : original section in section context (can be NULL if section is not complete)
  virtual CSection *GetOriginalSection(void);

  // gets updated section in section context
  // @return : updated section in section context (can be NULL if section is not complete)
  virtual CSection *GetUpdatedSection(void);

  // gets MPEG2 TS packet count
  // @return : MPEG2 TS packet count
  unsigned int GetPacketCount(void);

  // gets first MPEG2 TS packet continuity counter
  // @return : first MPEG2 TS packet continuity counter
  unsigned int GetContinuityCounter(void);

  // gets MPEG2 TS packets for replacing original section
  // @return : collection of MPEG2 TS packets
  CTsPacketCollection *GetPackets(void);

  /* set methods */

  // sets original section (only reference, section is not cloned, but section is released from memory in destructor)
  // @param section : reference to section to set
  // @return : true if successful, false otherwise (e.g. wrong section type)
  virtual bool SetOriginalSection(CSection *section);

  // sets original section empty
  // @param sectionEmpty : true if original section is empty, false otherwise
  void SetOriginalSectionEmpty(bool sectionEmpty);

  // sets original section incomplete
  // @param sectionEmpty : true if original section is incomplete, false otherwise
  void SetOriginalSectionIncomplete(bool sectionIncomplete);

  // sets original section complete
  // @param sectionComplete : true if original section is complete, false otherwise
  void SetOriginalSectionComplete(bool sectionComplete);

  // sets original section in error
  // @param sectionError : true if original section is in error, false otherwise
  void SetOriginalSectionError(bool sectionError);

  // sets original section is section or not
  // @param isSection : true if original section is section, false otherwise
  void SetOriginalSectionIsSection(bool isSection);

  // sets MPEG2 TS packet count
  void SetPacketCount(unsigned int packetCount);

  // sets first MPEG2 TS packet continuity counter
  // @param continuityCounter : the first MPEG2 TS packet continuity counter to set
  void SetContinuityCounter(unsigned int continuityCounter);

  /* other methods */

  // tests if original section is empty
  // @return : true if original section is empty, false otherwise
  bool IsOriginalSectionEmpty(void);

  // tests if original section is incomplete
  // @return : true if original section is incomplete, false otherwise
  bool IsOriginalSectionIncomplete(void);

  // tests if original section is complete
  // @return : true if original section is complete, false otherwise
  bool IsOriginalSectionComplete(void);

  // tests if original section is in error
  // @return : true if original section is in error, false otherwise
  bool IsOriginalSectionError(void);

  // tests if original section is section or not
  // @return : true if original section is section, false otherwise
  bool IsOriginalSectionIsSection(void);

  // creates updated section by cloning original section
  // @return : true if successful, false otherwise
  virtual bool CreateUpdatedSection(void) = 0;

protected:

  // holds original section instance
  CSection *originalSection;
  // holds updated section instance
  CSection *updatedSection;
  // holds MPEG2 TS continuity counter of first MPEG2 TS packet
  unsigned int continuityCounter;
  // holds MPEG2 TS packet count
  unsigned int packetCount;
  // holds MPEG2 TS packets for replacing original section
  CTsPacketCollection *packets;

  /* methods */
};

#endif