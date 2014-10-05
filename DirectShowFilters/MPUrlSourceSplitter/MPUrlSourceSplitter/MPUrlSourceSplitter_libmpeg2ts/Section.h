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

#ifndef __SECTION_DEFINED
#define __SECTION_DEFINED

#include "Flags.h"
#include "ProgramSpecificInformationPacket.h"
#include "TsPacketCollection.h"

#define SECTION_FLAG_NONE                                             FLAGS_NONE

#define SECTION_FLAG_CHECK_CRC32                                      (1 << (FLAGS_LAST + 0))
#define SECTION_FLAG_EMPTY_SECTION                                    (1 << (FLAGS_LAST + 1))
#define SECTION_FLAG_INCOMPLETE_SECTION                               (1 << (FLAGS_LAST + 2))
#define SECTION_FLAG_COMPLETE_SECTION                                 (1 << (FLAGS_LAST + 3))

#define SECTION_FLAG_LAST                                             (FLAGS_LAST + 4)

#define SECTION_HEADER_SIZE                                           3
#define SECTION_CRC32_SIZE                                            4

#define SECTION_HEADER_TABLE_ID_MASK                                  0x000000FF
#define SECTION_HEADER_TABLE_ID_SHIFT                                 16

#define SECTION_HEADER_SECTION_SYNTAX_INDICATOR_MASK                  0x00000001
#define SECTION_HEADER_SECTION_SYNTAX_INDICATOR_SHIFT                 15

#define SECTION_HEADER_PRIVATE_INDICATOR_MASK                         0x00000001
#define SECTION_HEADER_PRIVATE_INDICATOR_SHIFT                        14

#define SECTION_HEADER_RESERVED_MASK                                  0x00000003
#define SECTION_HEADER_RESERVED_SHIFT                                 12

#define SECTION_HEADER_SECTION_LENGTH_MASK                            0x00000FFF
#define SECTION_HEADER_SECTION_LENGTH_SHIFT                           0

#define SECTION_MAX_SIZE                                              0x00001000

class CSection : public CFlags
{
public:
  CSection(HRESULT *result);
  virtual ~CSection(void);

  /* get methods */

  // gets section table ID
  // @return : section table ID
  virtual unsigned int GetTableId(void);

  // gets reserved value
  // @return : reserved value
  virtual unsigned int GetReserved(void);

  // gets section size with header and CRC32
  // @return : section size with header and CRC32
  virtual unsigned int GetSectionSize(void); 

  // gets section payload size (without header and CRC32)
  // @return : section payload size
  virtual unsigned int GetSectionPayloadSize(void);

  // gets section data (with header and CRC32)
  // @return : section data (with header and CRC32) or NULL if error
  virtual const uint8_t *GetSection(void);

  /* set methods */

  // sets section table ID
  // @param tableId : the section table ID to set
  virtual void SetTableId(unsigned int tableId);

  /* other methods */

  // tests if section is empty
  // @return : true if section is empty, false otherwise
  virtual bool IsSectionEmpty(void);

  // tests if section is incomplete
  // @return : true if section is incomplete, false otherwise
  virtual bool IsSectionIncomplete(void);

  // tests if section is complete
  // @return : true if section is complete, false otherwise
  virtual bool IsSectionComplete(void);

  // tests if section syntax indicator is set
  // @return : true if set, false otherwise
  virtual bool IsSectionSyntaxIndicator(void);

  // tests if private indicator is set
  // @return : true if set, false otherwise
  virtual bool IsPrivateIndicator(void);

  // parses specified PSI packet
  // @param psiPacket : the PSI packet to parse
  // @param startFromSectionPayload : the section payload index to start parsing
  // @return : S_OK if successfull, S_FALSE if more PSI packets are needed to complete section, error code otherwise
  virtual HRESULT Parse(CProgramSpecificInformationPacket *psiPacket, unsigned int startFromSectionPayload);

  // clears current instance to its default state
  virtual void Clear(void);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  virtual CSection *Clone(void);

  // resets section size
  virtual void ResetSize(void);

protected:
  // header structure:
  // table_id                   8 bits
  // section_syntax_indicator   1 bit
  // private_indicator          1 bit
  // reserved                   2 bits
  // section_length             12 bits

  // holds section payload (from at least one PSI packet) and section current payload size
  uint8_t *payload;
  unsigned int currentPayloadSize;

  /* methods */

  // gets new instance of section
  // @return : new section instance or NULL if error
  virtual CSection *CreateItem(void) = 0;

  // deeply clones current instance
  // @param item : the section instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CSection *item);

  // gets whole section size
  // method is called to determine section size for storing section into buffer
  // @return : size of section
  virtual unsigned int GetSectionCalculatedSize(void);

  // gets whole section into payload
  // @return : number of bytes written into buffer, zero if not successful
  virtual unsigned int GetSectionInternal(void);

  // calculates section CRC32
  // @return : number of bytes written into buffer, zero if not successful
  virtual unsigned int CalculateSectionCrc32(void);
};

#endif